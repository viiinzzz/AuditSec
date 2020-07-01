using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Collections;
using System.DirectoryServices.AccountManagement;
using Microsoft.Office.Interop.Excel;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Drawing;
using System.Data.OleDb;


namespace AuditSec
{
    class MSOfficeTools
    {

        private static string ExcelColumnIndexToName(int Index)
        {
            string range = "";
            if (Index < 0) return range;
            for (int i = 1; Index + i > 0; i = 0)
            {
                range = ((char)(65 + Index % 26)).ToString() + range; Index /= 26;
            }
            if (range.Length > 1)
                range = ((char)((int)range[0] - 1)).ToString() + range.Substring(1);
            return range;
        }


        static object missing = System.Reflection.Missing.Value;


        public static System.Data.DataTable loadExcelFile(string f, bool show)
        {
            string cnx;
            if (f.ToLower().EndsWith(".xlsx"))
                cnx = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + f
                + ";Extended Properties=\"Excel 12.0;HDR=No;IMEX=1;READONLY=TRUE\"";
            else if (f.ToLower().EndsWith(".xls"))
                cnx = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + f
                + ";Extended Properties=\"Excel 8.0;HDR=No;IMEX=1;READONLY=TRUE\"";
            else
            {
                if (show)
                    MessageBox.Show(f + "\n\nis not an Excel file.", "Excel file loading");
                else
                    Console.WriteLine("EXCEL: " + f + " is not an Excel file.");
                return null;
            }
            try
            {
                OleDbConnection x = new OleDbConnection(cnx);
                x.Open();
                System.Data.DataTable schemaTable = x.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
                string tableName = schemaTable.Rows[0][2].ToString().Trim();
                OleDbDataAdapter a = new OleDbDataAdapter("SELECT * FROM [" + tableName + "]", x);
                System.Data.DataTable t = new System.Data.DataTable();
                a.Fill(t); x.Close();
                return t;
            }
            catch (Exception e)
            {
                if (show)
                    MessageBox.Show(f + "\n\nis not a valid Excel file.\n\n" + e.Message, "Excel file loading");
                else
                    Console.WriteLine("EXCEL: " + f + " is not a valid Excel file.\n" + e.ToString());
                return null;
            }
        }


        public static bool exportExcel(string f, DataGridView grid, string SheetName,
            Func<string, string> replaceCell = null, Func<string, string> replaceHeader = null)
        {
            System.Data.DataTable table = new System.Data.DataTable(SheetName);
            foreach (DataGridViewColumn col in grid.Columns) table.Columns.Add((replaceHeader == null ? col.HeaderText : replaceHeader(col.HeaderText)));
            foreach (DataGridViewRow row in grid.Rows)
            {
                List<object> values = new List<object>();
                foreach(DataGridViewCell cell in row.Cells)
                    values.Add(cell.Value == null ? "" : (replaceCell == null ? cell.Value.ToString() : replaceCell(cell.Value.ToString())));
                table.Rows.Add(values.ToArray<object>());
            }
            return exportExcel(f, table, SheetName);
        }

        public static bool exportExcel(string f, System.Data.DataTable table, string SheetName)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Workbook workbook = null, workbook2 = null;
            bool ret = false;

            if (File.Exists(f))
            {
                string YES = Interaction.InputBox(f + " exists. Overwrite?", "Export the result into an Excel File", "Yes");
                if (YES == null || !YES.ToUpper().Equals("YES")) return false;
                try
                {
                    File.Delete(f);
                }
                catch (Exception e)
                {
                    MessageBox.Show(f + " cannot be deleted.\n" + e.ToString(),
                        "Export the result into an Excel File");
                    return false;
                }
            }

            try
            {
                string htm = f + ".htm";
                Console.WriteLine("Export-1:HTM... \"" + htm + "\"     " + table.Rows.Count);

                System.Data.DataTable table_ = new System.Data.DataTable();
                table_.TableName = table.TableName;
                foreach (DataColumn c in table.Columns)
                {
                    if (c.DataType.Equals(typeof(DateTime)))
                        table_.Columns.Add(c.ColumnName, typeof(string));
                    else
                        table_.Columns.Add(c.ColumnName, c.DataType);
                }
                foreach (DataRow r in table.Rows)
                {
                    DataRow r_ = table_.NewRow();
                    for (int i = 0; i < table.Columns.Count; i++)
                        if (table.Columns[i].DataType.Equals(typeof(DateTime)))
                            r_[i] = DateTime.MinValue.Equals(r[i]) ? "" :
                                String.Format(new CultureInfo("en-US"), "&nbsp;{0:yyyy/MM/dd}", (DateTime)r[i]);
                        else
                            r_[i] = r[i];
                    table_.Rows.Add(r_);
                }

                using (StreamWriter sw = new StreamWriter(htm, false, Encoding.UTF8))
                {
                    using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                    {
                        System.Web.UI.WebControls.DataGrid grid = new System.Web.UI.WebControls.DataGrid();
                        grid.HeaderStyle.Font.Bold = true;
                        grid.DataSource = table_;
                        grid.DataBind();
                        grid.RenderControl(hw);
                    }
                }

                excel = new Microsoft.Office.Interop.Excel.Application();
                workbook = excel.Workbooks.Open(htm,
                    missing, missing, missing, missing, missing, missing, missing,
                    missing, missing, missing, missing, missing, missing, missing);
                Worksheet worksheet = (Worksheet)excel.ActiveSheet;

                excel.Rows.EntireRow.AutoFit();
                excel.Columns.EntireColumn.AutoFit();

                if (f.ToLower().EndsWith(".xls"))
                {
                    Console.WriteLine("Export-2:XLS... \"" + f + "\"");
                    workbook.SaveAs(f, XlFileFormat.xlWorkbookNormal, missing, missing, false, false,
                        XlSaveAsAccessMode.xlNoChange, missing, missing, missing, missing, missing);
                }
                else if (f.ToLower().EndsWith(".xlsx"))
                {
                    Console.WriteLine("Export-2:XLSX... \"" + f + "\"");
                    workbook.SaveAs(f, XlFileFormat.xlWorkbookDefault, missing, missing, false, false,
                        XlSaveAsAccessMode.xlNoChange, missing, missing, missing, missing, missing);
                }
                else
                {
                    Console.WriteLine("Export-2:NONE. \"" + f + "\"");
                }

                if (File.Exists(f))
                {
                    File.Delete(htm);
                }


                try
                {
                    Range r = (Range)worksheet.UsedRange;
                    bool delzerodate = (bool)r.Sort(worksheet.get_Range("A1"), XlSortOrder.xlAscending,
                        missing, missing, XlSortOrder.xlAscending, missing, XlSortOrder.xlAscending, XlYesNoGuess.xlGuess, missing,
                        false, XlSortOrientation.xlSortColumns, XlSortMethod.xlStroke,
                        XlSortDataOption.xlSortTextAsNumbers, XlSortDataOption.xlSortTextAsNumbers, XlSortDataOption.xlSortTextAsNumbers);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Export-2:NONE. \"" + f + "\"");
                }

                worksheet.Name = SheetName;

                string FORMAT_ERROR = null;
                try
                {
                    List<object> Widths = new List<object>();
                    object Height = 15;
                    for (int s = 1; s <= workbook.Worksheets.Count; s++)
                    {
                        Worksheet sheet = (Worksheet)workbook.Worksheets.get_Item(s);
                        sheet.Activate();
                        Range range = sheet.get_Range("A1");
                        range = sheet.get_Range(range, range.get_End(XlDirection.xlToRight));
                        range = sheet.get_Range(range, range.get_End(XlDirection.xlDown));
                        string table_name = "Table_" + sheet.Name;
                        sheet.ListObjects.AddEx(XlListObjectSourceType.xlSrcRange, range,
                            missing, XlYesNoGuess.xlYes, missing).Name = table_name;


                        string style = "TableStyleMedium1";

                        sheet.ListObjects[table_name].TableStyle = style;

                        sheet.get_Range("B2").Select();
                        excel.ActiveWindow.FreezePanes = true;
                        sheet.get_Range("A1").Select();

                        if (s == 1)
                        {
                            for (int column = 0; column < sheet.Columns.Count && column < 100; column++)
                            {
                                string c = ExcelColumnIndexToName(column);
                                var col = sheet.get_Range(c + 1);
                                var cell = col.Value;
                                string value = cell != null ? cell.ToString() : "";
                                if (value.Length > 0)
                                {
                                    Widths.Add(col.ColumnWidth);
                                    //Console.WriteLine("Column#" + c + "=" + value + ", Width=" + col.ColumnWidth);
                                }
                                else
                                {
                                    //Console.WriteLine("Column#" + c + " ***break***");
                                    break;
                                }
                            }
                            var row = sheet.get_Range("A1");
                            //Console.WriteLine("Row#1, Height=" + row.RowHeight);
                            Height = row.RowHeight;
                        }
                        else
                        {
                            for (int column = 0; column < sheet.Columns.Count && column < 100 && column < Widths.Count; column++)
                            {
                                string c = ExcelColumnIndexToName(column);
                                var col = sheet.get_Range(c + 1);
                                var cell = col.Value;
                                string value = cell != null ? cell.ToString() : "";
                                if (value.Length > 0)
                                {
                                    //Console.WriteLine("Column " + column + " Widths.Count " + Widths.Count);
                                    var width = Widths.ElementAt(column);
                                    col.ColumnWidth = width;
                                    //Console.WriteLine("Column#" + c + ", Width=" + width);
                                }
                                else
                                {
                                    //Console.WriteLine("Column#" + c + " ***break***");
                                    break;
                                }
                            }
                            sheet.Rows.RowHeight = Height;
                        }
                    }
                }
                catch (Exception e)
                {
                    FORMAT_ERROR = e.Message;
                    Console.WriteLine("Table formatting failed: " + e.ToString());
                }


                {
                    Console.WriteLine("Export-3:FINAL... \"" + f + "\"");
                    ((Worksheet)workbook.Worksheets.get_Item(1)).Activate();
                    excel.Visible = true;

                    workbook.Save();
                    {
                        ((_Worksheet)worksheet).Activate();
                        MessageBox.Show(table.Rows.Count + " lines saved to file: \"" + f + "\""
                            + (FORMAT_ERROR == null ? "" : "\n\nHowever, table formatting failed:\n" + FORMAT_ERROR),
                            "Export the result into an Excel File");
                    }
                    ret = true;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("Error while exporting to file: \"" + f + "\"\n" + ee.Message,
                    "Export the result into an Excel File");
            }

            try
            {
                if (workbook != null) workbook.Close(missing, missing, missing);
                if (workbook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                workbook = null;

                //if (workbook2 != null) workbook2.Close(missing, missing, missing);
                if (workbook2 != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook2);
                workbook2 = null;

                if (excel != null) excel.Quit();
                if (excel != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                excel = null;

                GC.Collect();
            }
            catch (Exception e)
            {
                Console.Write("Excel cleaning error: " + e.ToString());
            }
            return ret;
        }






        public static bool exportResult_Split(string f, Func<string, string, bool> update, bool unattended, bool want_csv,
            System.Data.DataTable table, Color[] colcolors, int[] colhide, int RowCount,
            string progver, List<string> splitColumns,
            //StringBuilder PARSER_TEMPL, StringBuilder PARSER_DEBUG, SCCMReporting GUI)
            string PARSER_TEMPL_PATH, string PARSER_DEBUG_PATH, SCCMReporting GUI)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Workbook workbook = null, workbook2 = null;
            bool ret = false;

            string unattstr = unattended ? "SCHEDULED JOB | " : "";
            if (File.Exists(f))
            {
                string YES = unattended ? "YES" : Interaction.InputBox(f + " exists. Overwrite?", "Export the result into an Excel File", "Yes");
                if (YES == null || !YES.ToUpper().Equals("YES")) return false;
                try
                {
                    File.Delete(f);
                }
                catch (Exception e)
                {
                    if (unattended) Console.WriteLine("Export the result into an Excel File: "
                        + f + " cannot be deleted.\n" + e.ToString());
                    else MessageBox.Show(f + " cannot be deleted.\n" + e.ToString(),
                        "Export the result into an Excel File");
                    return false;
                }
            }

           

            string SPLIT_ERROR = null;
            try
            {
                string htm = f + ".htm";
                Console.WriteLine("Export-1:HTM... \"" + htm + "\"     " + table.Rows.Count);

                System.Data.DataTable table_ = new System.Data.DataTable();
                table_.TableName = table.TableName;
                foreach (DataColumn c in table.Columns)
                {
                    if (c.DataType.Equals(typeof(DateTime)))
                        table_.Columns.Add(c.ColumnName, typeof(string));
                    else
                        table_.Columns.Add(c.ColumnName, c.DataType);
                }
                foreach (DataRow r in table.Rows)
                {
                    DataRow r_ = table_.NewRow();
                    for (int i = 0; i < table.Columns.Count; i++)
                        if (table.Columns[i].DataType.Equals(typeof(DateTime)))
                            r_[i] = DateTime.MinValue.Equals(r[i]) ? "" :
                                String.Format(new CultureInfo("en-US"), "&nbsp;{0:yyyy/MM/dd}", (DateTime)r[i]);
                        else
                            r_[i] = r[i];
                    table_.Rows.Add(r_);
                }

                using (StreamWriter sw = new StreamWriter(htm, false, Encoding.UTF8))
                {
                    using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                    {
                        System.Web.UI.WebControls.DataGrid grid = new System.Web.UI.WebControls.DataGrid();
                        grid.HeaderStyle.Font.Bold = true;
                        grid.DataSource = table_;
                        grid.DataBind();
                        grid.RenderControl(hw);
                    }
                }

                excel = new Microsoft.Office.Interop.Excel.Application();
                workbook = excel.Workbooks.Open(htm,
                    missing, missing, missing, missing, missing, missing, missing,
                    missing, missing, missing, missing, missing, missing, missing);
                Worksheet worksheet = (Worksheet)excel.ActiveSheet;

                excel.Rows.EntireRow.AutoFit();
                excel.Columns.EntireColumn.AutoFit();

                if (f.ToLower().EndsWith(".xls"))
                {
                    Console.WriteLine("Export-2:XLS... \"" + f + "\"");
                    workbook.SaveAs(f, XlFileFormat.xlWorkbookNormal, missing, missing, false, false,
                        XlSaveAsAccessMode.xlNoChange, missing, missing, missing, missing, missing);
                }
                else if (f.ToLower().EndsWith(".xlsx"))
                {
                    Console.WriteLine("Export-2:XLSX... \"" + f + "\"");
                    workbook.SaveAs(f, XlFileFormat.xlWorkbookDefault, missing, missing, false, false,
                        XlSaveAsAccessMode.xlNoChange, missing, missing, missing, missing, missing);
                }
                else
                {
                    Console.WriteLine("Export-2:NONE. \"" + f + "\"");
                }

                if (File.Exists(f))
                {
                    File.Delete(htm);
                    if (want_csv)
                    {
                        string csv = f.Replace(".xlsx", ".csv").Replace(".xls", ".csv");
                        if (!csv.EndsWith(".csv")) throw new Exception("Incorrect CSV filename: " + csv);

                        Console.WriteLine("Export-2:CSV... \"" + csv + "\"");
                        string f2 = f + ".csv";
                        if (File.Exists(f2)) File.Delete(f2);
                        if (File.Exists(csv)) File.Delete(csv);
                        workbook.SaveCopyAs(f2);
                        workbook2 = excel.Workbooks.Open(f2,
                            missing, true, missing, missing, missing, missing, missing,
                            missing, missing, missing, missing, missing, missing, missing);

                        Worksheet worksheet2 = workbook2.ActiveSheet;
                        foreach (int i in colhide.Reverse())
                        {
                            string c = ExcelColumnIndexToName(i);
                            worksheet2.get_Range(c + "1", Type.Missing).EntireColumn.Delete(missing);
                        }

                        workbook2.SaveAs(csv, XlFileFormat.xlCSV, missing, missing, false, false,
                            XlSaveAsAccessMode.xlNoChange, XlSaveConflictResolution.xlLocalSessionChanges,
                            missing, missing, missing, missing);
                        workbook2.Close(false, missing, missing);
                        if (File.Exists(f2)) File.Delete(f2);
                    }
                }


                try
                {
                    Range r = (Range)worksheet.UsedRange;
                    bool delzerodate = (bool)r.Sort(worksheet.get_Range("A1"), XlSortOrder.xlAscending,
                        missing, missing, XlSortOrder.xlAscending, missing, XlSortOrder.xlAscending, XlYesNoGuess.xlGuess, missing,
                        false, XlSortOrientation.xlSortColumns, XlSortMethod.xlStroke,
                        XlSortDataOption.xlSortTextAsNumbers, XlSortDataOption.xlSortTextAsNumbers, XlSortDataOption.xlSortTextAsNumbers);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Export-2:NONE. \"" + f + "\"");
                }

                /*
                if (!exit) try
                {
                    Range r = (Range)worksheet.UsedRange;
                    bool delzerodate = (bool)r.Replace("01/01/0001 00:00", "",
                        XlLookAt.xlWhole, XlSearchOrder.xlByRows, true, missing, missing, missing);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Zero Date to Blank String replacement: " + e.ToString());
                }
                */

                worksheet.Name = "All";

                Hashtable FULL_H = new Hashtable();

                List<Counts> COUNTS = new List<Counts>();
                Hashtable COUNTS_H = new Hashtable();
                Counts All_COUNTS = new Counts("All");
                COUNTS.Add(All_COUNTS);

                int rowMax = worksheet.get_Range("A" + worksheet.Rows.Count, Type.Missing).get_End(XlDirection.xlUp).Row;
                All_COUNTS.Set("All", rowMax - 1);

                string lastSplit = null;
                Worksheet after = null;
                Hashtable splitSheets = new Hashtable();
                if (splitColumns != null && splitColumns.Count > 0)
                    try //splitting
                    {
                        //worksheet.Name = "All";
                        Range headerRow = worksheet.get_Range("1:1");
                        Hashtable splitColumnLetters = new Hashtable();
                        foreach (string splitColumn in splitColumns) splitColumnLetters.Add(splitColumn, "");
                        {
                            int column = 0; string value = ""; do
                            {
                                string col = ExcelColumnIndexToName(column);
                                //Console.WriteLine(col + "1");
                                var cell = worksheet.get_Range(col + "1").Value;
                                value = cell != null ? cell.ToString().ToUpper() : "";
                                if (value.Length > 0)
                                {
                                    foreach (string splitColumn in splitColumns)
                                        if (splitColumn.ToUpper().Equals(value))
                                        {
                                            splitColumnLetters.Remove(splitColumn);
                                            splitColumnLetters.Add(splitColumn, col);
                                            if (!COUNTS_H.Contains(splitColumn))
                                            {
                                                Counts split_COUNTS = new Counts(splitColumn);
                                                COUNTS_H.Add(splitColumn, split_COUNTS);
                                                COUNTS.Add(split_COUNTS);
                                            }
                                        }
                                }
                            } while (value.Length > 0 && column++ < 100);
                        }
                        
                        update(unattstr + splitColumns.Aggregate((x, y) => x + ", " + y) + " split in progress..." + Path.GetFileName(f),
                            0 + "/" + rowMax);

                        //Console.WriteLine("2.." + rowMax);
                        for (int row = 2; row <= rowMax; row++)
                        {
                            Range sourceRow = worksheet.get_Range(row + ":" + row);

                            Hashtable splitColumnValues = new Hashtable();
                            foreach (string splitColumn in splitColumns)
                            {
                                string col = (string)splitColumnLetters[splitColumn];
                                //Console.WriteLine(col + row);
                                var cell = col.Length > 0 ? worksheet.get_Range(col + row).Value : null;
                                splitColumnValues.Add(splitColumn, cell == null ? "" :
                                    cell.ToString().ToUpper().Replace(':', '-').Replace('\\', '-').Replace('/', '-')
                                    .Replace('?', '-').Replace('*', '-').Replace('[', '-').Replace(']', '-')
                                    .Replace(';', '+').Replace(" ", "-")
                                    .Replace("&", " and ").Replace("@", " at ").Replace("-", "·"));
                            }
                            int SheetNameMaxLength = 30;
                            foreach (string splitColumn in splitColumns)
                            {
                                string col = (string)splitColumnLetters[splitColumn];
                                //Console.WriteLine(col + row);
                                var cell = col.Length > 0 ? worksheet.get_Range(col + row).Value : null;
                                string fullvalue = cell == null ? "" : cell.ToString().ToUpper();

                                int valueMaxLength = SheetNameMaxLength - (splitColumn.Length + 1);
                                string value = (string)splitColumnValues[splitColumn];
                                if (valueMaxLength > 0 && value.Length > valueMaxLength)
                                {
                                    value = value.Substring(0, valueMaxLength) + OVERLIMIT_SHEETNAME_SYMBOL;
                                    GUI.setWQL("Sheet name '" + splitColumn + "_" + ((string)splitColumnValues[splitColumn])
                                        + "' truncated to '" + splitColumn + "_" + value + "'");
                                    Console.WriteLine("Sheet name '" + splitColumn + "_" + ((string)splitColumnValues[splitColumn])
                                        + "' truncated to '" + splitColumn + "_" + value + "'");
                                }
                                if (value.Length > 0)
                                {
                                    string newname = splitColumn + "_" + value;
                                    lastSplit = newname;

                                    if (!FULL_H.Contains(newname)) FULL_H.Add(newname, fullvalue);

                                    Worksheet splitSheet = splitSheets.Contains(newname) ?
                                        (Worksheet)splitSheets[newname] : null;
                                    if (splitSheet == null)
                                    {
                                        //Console.WriteLine("Creating new workbook sheet: " + newname);

                                        after = null;
                                        for (int s = 1; s <= workbook.Worksheets.Count; s++)
                                        {
                                            Worksheet sheet =
                                                (Worksheet)workbook.Worksheets.get_Item(s);
                                            //Console.WriteLine("\t" + newname + " after? " + sheet.Name);
                                            if (sheet.Name.CompareTo(newname) > 0) break;
                                            //Console.WriteLine("\t" + newname + " after! " + sheet.Name);
                                            after = sheet;
                                        }


                                        splitSheet = (Worksheet)workbook.Worksheets.Add(
                                        missing, after != null ? after : missing, missing, missing);
                                        splitSheet.Name = newname;
                                        splitSheets.Add(newname, splitSheet);
                                        headerRow.Copy(splitSheet.get_Range("A1", Type.Missing));
                                    }
                                    Range EndofcolumnA =
                                        splitSheet.get_Range("A" + splitSheet.Rows.Count, Type.Missing);
                                    int LastRow = EndofcolumnA.get_End(XlDirection.xlUp).Row;
                                    int NewRow = LastRow + 1;
                                    Range pasteToRow = splitSheet.get_Range("A" + NewRow);
                                    sourceRow.Copy(pasteToRow);

                                    ((Counts)COUNTS_H[splitColumn]).Increment(value);
                                }
                            }


                            update(unattstr + splitColumns.Aggregate((x, y) => x + ", " + y) + " split in progress..." + Path.GetFileName(f),
                                row + "/" + rowMax);
                        }


                        workbook.Save();
                    }
                    catch (System.Runtime.InteropServices.COMException e)
                    {
                        SPLIT_ERROR = e.Message + "\nlastSplit=" + lastSplit;
                        Console.WriteLine(splitColumns.Aggregate((x, y) => x + ", " + y) + " split failed: " + e.ToString()
                            + "\nNew_Sheet_Name=" + lastSplit + " New_Sheet_Name_Length=" + lastSplit.Length);
                    }
                    catch (Exception e)
                    {
                        SPLIT_ERROR = e.Message;
                        Console.WriteLine(splitColumns.Aggregate((x, y) => x + ", " + y) + " split failed: " + e.ToString());
                    }

                string FORMAT_ERROR = null;
                try
                {
                    update(unattstr + "Table formatting in progress..." + Path.GetFileName(f),
                        0 + "/" + workbook.Worksheets.Count);
                    List<object> Widths = new List<object>();
                    object Height = 15;
                    for (int s = 1; s <= workbook.Worksheets.Count; s++)
                    {
                        update(unattstr + "Table formatting in progress..." + Path.GetFileName(f),
                            s + "/" + workbook.Worksheets.Count);
                        Worksheet sheet = (Worksheet)workbook.Worksheets.get_Item(s);
                        sheet.Activate();
                        Range range = sheet.get_Range("A1");
                        range = sheet.get_Range(range, range.get_End(XlDirection.xlToRight));
                        range = sheet.get_Range(range, range.get_End(XlDirection.xlDown));
                        string table_name = "Table_" + sheet.Name;
                        sheet.ListObjects.AddEx(XlListObjectSourceType.xlSrcRange, range,
                            missing, XlYesNoGuess.xlYes, missing).Name = table_name;


                        string style = "TableStyleMedium1";
                        if (f.ToLower().Contains("-_template_")) style = "TableStyleMedium7";
                        else if (f.ToLower().Contains("-_condition_")) style = "TableStyleMedium7";
                        else if (f.ToLower().Contains("-file")) style = "TableStyleMedium6";
                        else if (f.ToLower().Contains("-workstation")) style = "TableStyleMedium4";
                        else if (f.ToLower().Contains("-usage")) style = "TableStyleMedium5";
                        else if (f.ToLower().Contains("-program")) style = "TableStyleMedium2";

                        sheet.ListObjects[table_name].TableStyle = style;

                        for (int i = 0; i < colcolors.Length; i++)
                        {
                            string c = ExcelColumnIndexToName(i);
                            if (!colcolors[i].Equals(Color.Empty) && !colcolors[i].Equals(Color.Transparent))
                                sheet.get_Range(c + "1").Interior.Color = System.Drawing.ColorTranslator.ToOle(colcolors[i]);
                        }

                        foreach (int i in colhide.Reverse())
                        {
                            string c = ExcelColumnIndexToName(i);
                            sheet.get_Range(c + "1", Type.Missing).EntireColumn.Delete(missing);
                        }

                        sheet.get_Range("B2").Select();
                        excel.ActiveWindow.FreezePanes = true;
                        sheet.get_Range("A1").Select();

                        if (s == 1)
                        {
                            for (int column = 0; column < sheet.Columns.Count && column < 100; column++)
                            {
                                update(unattstr + "Table formatting in progress..." + Path.GetFileName(f),
                                    s + "/" + workbook.Worksheets.Count + " " + (column + 1) + "/");
                                string c = ExcelColumnIndexToName(column);
                                var col = sheet.get_Range(c + 1);
                                var cell = col.Value;
                                string value = cell != null ? cell.ToString() : "";
                                if (value.Length > 0)
                                {
                                    Widths.Add(col.ColumnWidth);
                                    //Console.WriteLine("Column#" + c + "=" + value + ", Width=" + col.ColumnWidth);
                                }
                                else
                                {
                                    //Console.WriteLine("Column#" + c + " ***break***");
                                    break;
                                }
                            }
                            var row = sheet.get_Range("A1");
                            //Console.WriteLine("Row#1, Height=" + row.RowHeight);
                            Height = row.RowHeight;
                        }
                        else
                        {
                            for (int column = 0; column < sheet.Columns.Count && column < 100 && column < Widths.Count; column++)
                            {
                                update(unattstr + "Table formatting in progress..." + Path.GetFileName(f),
                                    s + "/" + workbook.Worksheets.Count + " " + (column + 1) + "/" + Widths.Count);
                                string c = ExcelColumnIndexToName(column);
                                var col = sheet.get_Range(c + 1);
                                var cell = col.Value;
                                string value = cell != null ? cell.ToString() : "";
                                if (value.Length > 0)
                                {
                                    //Console.WriteLine("Column " + column + " Widths.Count " + Widths.Count);
                                    var width = Widths.ElementAt(column);
                                    col.ColumnWidth = width;
                                    //Console.WriteLine("Column#" + c + ", Width=" + width);
                                }
                                else
                                {
                                    //Console.WriteLine("Column#" + c + " ***break***");
                                    break;
                                }
                            }
                            sheet.Rows.RowHeight = Height;
                        }
                    }
                }
                catch (Exception e)
                {
                    FORMAT_ERROR = e.Message;
                    Console.WriteLine("Table formatting failed: " + e.ToString());
                }

                try
                {
                    Worksheet wqlSheet = (Worksheet)workbook.Worksheets.Add(missing, worksheet, missing, missing);
                    wqlSheet.Name = "WQL Request";
                    wqlSheet.PageSetup.Application.ActiveWindow.DisplayGridlines = false;
                    ((Range)wqlSheet.Columns["A", missing]).ColumnWidth = 150;
                    ((Range)wqlSheet.Columns["A", missing]).WrapText = true;
                    ((Range)wqlSheet.Columns["A", missing]).Font.Name = "Microsoft Sans Serif";
                    ((Range)wqlSheet.Columns["A", missing]).Font.Size = 9.75;

                    int l = 1;
                    //List<string> steps = new List<string>(PARSER_TEMPL.ToString().Replace("\t", "    ")
                    List<string> steps = new List<string>(File.ReadAllText(PARSER_TEMPL_PATH).Replace("\t", "    ")
                        .Split(new char[] { '¤' }, StringSplitOptions.RemoveEmptyEntries));
                    steps.Insert(0, "AuditSec - SCCM Reporting - " + progver);
                    foreach (string step in steps)
                    {
                        string[] lines = step.Trim().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        bool line1 = true;
                        foreach (string line in lines)
                            if (line.Trim().Length > 0)
                            {
                                wqlSheet.get_Range("A" + l).Value = "'" + line;
                                if (line1)
                                {
                                    ((Range)wqlSheet.Rows["" + l, missing]).Font.Bold = true;
                                    line1 = false;
                                }
                                l++;
                            }
                        l += 3;
                    }
                }
                catch (Exception e)
                {
                    FORMAT_ERROR = e.Message;
                    Console.WriteLine("WQL Request writing failed: " + e.ToString());
                }


                try
                {
                    Worksheet wqlSheet = (Worksheet)workbook.Worksheets.Add(missing, worksheet, missing, missing);
                    wqlSheet.Name = "WQL Logs";
                    wqlSheet.PageSetup.Application.ActiveWindow.DisplayGridlines = false;
                    ((Range)wqlSheet.Columns["A", missing]).ColumnWidth = 150;
                    ((Range)wqlSheet.Columns["A", missing]).WrapText = true;
                    ((Range)wqlSheet.Columns["A", missing]).Font.Name = "Microsoft Sans Serif";
                    ((Range)wqlSheet.Columns["A", missing]).Font.Size = 9.75;

                    int l = 1;
                    //List<string> lines = new List<string>(PARSER_DEBUG.ToString().Replace("\t", "    ")
                    //    .Split(new char[] { '\r', '\n' }, StringSplitOptions.None));
                    List<string> lines = new List<string>(File.ReadAllLines(PARSER_DEBUG_PATH)).Select(line => line.Replace("\t", "    ")).ToList();
                    List<string> beforeQueryrun = new List<string>();
                    while (lines.Count > 0 && lines[0].IndexOf("=QUERYRUN=") < 0)
                    {
                        if (lines[0].ToLower().StartsWith("wql:error:")
                            || lines[0].ToLower().StartsWith("wql:warning:")
                            || lines[0].ToLower().StartsWith("wql:information:"))
                            beforeQueryrun.Add(lines[0]);
                        lines.RemoveAt(0);
                    }
                    lines.InsertRange(0, beforeQueryrun);
                    lines.Insert(0, "Report ran by: " + UserPrincipal.Current.SamAccountName);
                    lines.Insert(1, "");
                    foreach (string line_ in lines)
                    {
                        string line = line_;
                        if (line.ToUpper().StartsWith("WQL:")) line = line.Substring(4);
                        else if (line.IndexOf("=QUERYRUN=") >= 0) line = "";
                        if (line.StartsWith("Last parse text written into file:")) line = "";
                        if (line.Trim().Length > 0)
                        {
                            wqlSheet.get_Range("A" + l).Value = "'" + line;
                            if (l == 1)
                                ((Range)wqlSheet.Rows["" + l, missing]).Font.Bold = true;
                            if (line.ToLower().StartsWith("error:"))
                                ((Range)wqlSheet.Rows["" + l, missing]).Font.Color =
                                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.OrangeRed);
                            else if (line.ToLower().StartsWith("warning:"))
                                ((Range)wqlSheet.Rows["" + l, missing]).Font.Color =
                                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkRed);
                            else if (line.ToLower().StartsWith("information:"))
                                ((Range)wqlSheet.Rows["" + l, missing]).Font.Color =
                                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Green);
                            l++;
                        }
                    }
                }
                catch (Exception e)
                {
                    FORMAT_ERROR = e.Message;
                    Console.WriteLine("WQL Logs writing failed: " + e.ToString());
                }


                try//counts
                {
                    Worksheet countsSheet = (Worksheet)workbook.Worksheets.Add(missing, worksheet, missing, missing);
                    countsSheet.Name = "Counts";
                    countsSheet.PageSetup.Application.ActiveWindow.DisplayGridlines = false;
                    ((Range)countsSheet.Columns["A", missing]).Font.Name = "Microsoft Sans Serif";
                    ((Range)countsSheet.Columns["A", missing]).Font.Size = 9.75;
                    int c = 0;
                    foreach(Counts split_COUNTS in COUNTS) if (split_COUNTS.getTotal() > 0)
                    {
                        int l = 1;
                        foreach(object[] dual in split_COUNTS.Get())
                        {
                            string item = (string)dual[0];
                            int count = (int)dual[1];
                            string sheetname = (split_COUNTS.getSplitColumn().Equals("All") ? "" : split_COUNTS.getSplitColumn() + "_") + item;


                            countsSheet.get_Range(ExcelColumnIndexToName(c) + l).Value = item;
                            countsSheet.get_Range(ExcelColumnIndexToName(c + 1) + l).Value = count;
                            if (l != 1)
                            {
                                countsSheet.get_Range(ExcelColumnIndexToName(c) + l).BorderAround2(
                                    missing, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, missing, missing);
                                countsSheet.get_Range(ExcelColumnIndexToName(c + 1) + l).BorderAround2(
                                    missing, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, missing, missing);
                                Hyperlink link_go = (Hyperlink)countsSheet.Hyperlinks.Add(
                                    countsSheet.get_Range(ExcelColumnIndexToName(c) + l, Type.Missing),
                                    "#" + sheetname + "!A1", Type.Missing,
                                    "Go to "
                                    //+ sheetname
                                    + (FULL_H.Contains(sheetname) ? FULL_H[sheetname] : sheetname)
                                    + " (" + count + " item" + (count > 1 ? "s" : "") + ")", item);
                                Worksheet sheet = (Worksheet)splitSheets[sheetname];
                                if (sheet != null)
                                {
                                    Hyperlink link_return = (Hyperlink)sheet.Hyperlinks.Add(
                                        sheet.get_Range("A1", Type.Missing),
                                        "#Counts!A1", Type.Missing,
                                        "Return to Counts", sheet.get_Range("A1").Value.ToString());
                                }
                            }
                            if (l == 1)
                            {
                                countsSheet.get_Range(ExcelColumnIndexToName(c) + l).Font.Bold = true;
                                countsSheet.get_Range(ExcelColumnIndexToName(c + 1) + l).Font.Bold = true;
                                countsSheet.get_Range(ExcelColumnIndexToName(c) + l).Font.Color =
                                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                                countsSheet.get_Range(ExcelColumnIndexToName(c + 1) + l).Font.Color =
                                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                                countsSheet.get_Range(ExcelColumnIndexToName(c) + l).Interior.Color =
                                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
                                countsSheet.get_Range(ExcelColumnIndexToName(c + 1) + l).Interior.Color =
                                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
                            }
                            l++;
                        }
                        c += 3;
                    }
                    countsSheet.Columns.EntireColumn.AutoFit();
                }
                catch (Exception e)
                {
                    FORMAT_ERROR = e.Message;
                    Console.WriteLine("Counts writing failed: " + e.ToString());
                }


                {
                    Console.WriteLine("Export-3:FINAL... \"" + f + "\"");
                    update(unattstr + "Final saving in progress..." + Path.GetFileName(f), "");
                    ((Worksheet)workbook.Worksheets.get_Item(1)).Activate();
                    if (!unattended) excel.Visible = true;

                    workbook.Save(); if (!unattended)
                    {
                        ((_Worksheet)worksheet).Activate();
                        MessageBox.Show(RowCount + " lines saved to file: \"" + f + "\""
                            + (SPLIT_ERROR == null ? "" : "\n\nHowever, Domain, Site, Department split failed:\n" + SPLIT_ERROR)
                            + (FORMAT_ERROR == null ? "" : "\n\nHowever, table formatting failed:\n" + FORMAT_ERROR),
                            "Export the result into an Excel File");
                    }
                    else Console.WriteLine(RowCount + " lines saved to file: \"" + f + "\""
                        + (SPLIT_ERROR == null ? "" : "\n\nHowever, Domain, Site, Department split failed:\n" + SPLIT_ERROR)
                        + (FORMAT_ERROR == null ? "" : "\n\nHowever, table formatting failed:\n" + FORMAT_ERROR)
                        );

                    ret = true;
                }
            }
            catch (Exception ee)
            {
                if (!unattended) MessageBox.Show("Error while exporting to file: \"" + f + "\"\n" + ee.Message,
                    "Export the result into an Excel File");
                else Console.WriteLine("Error while exporting to file: " + f + " - " + ee.Message + "\n" + ee.StackTrace);
            }

            try
            {
                if (workbook != null) workbook.Close(missing, missing, missing);
                if (workbook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                workbook = null;

                //if (workbook2 != null) workbook2.Close(missing, missing, missing);
                if (workbook2 != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook2);
                workbook2 = null;

                if (excel != null) excel.Quit();
                if (excel != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                excel = null;

                GC.Collect();
            }
            catch (Exception e)
            {
                Console.Write("Excel cleaning error: " + e.ToString());
            }
            return ret;
        }

        static char OVERLIMIT_SHEETNAME_SYMBOL = '¨';
        
        class Counts
        {
            string splitColumn;
            int total = 0;
            Hashtable H = new Hashtable();
            public string getSplitColumn() { return splitColumn; }
            public int getTotal() { return total; }
            public Counts(string splitColumn)
            {
                this.splitColumn = splitColumn;
            }
            public void Increment(string item)
            {
                total++;
                if (!H.Contains(item))
                    H.Add(item, (int)1);
                else
                {
                    int i = (int)H[item];
                    H.Remove(item);
                    H.Add(item, ++i);
                }
            }
            public void Set(string item, int count)
            {
                total += count;
                if (H.Contains(item))
                {
                    int oldcount = (int)H[item];
                    total -= oldcount;
                    H.Remove(item);
                }
                H.Add(item, count);
            }
            public List<object[]> Get()
            {
                List<object[]> L = new List<object[]>();
                foreach (string item in H.Keys)
                {
                    int i = (int)H[item];
                    int j; for (j = 0; j < L.Count; j++) if (i > (int)L[j][1]) break;
                    L.Insert(j, new object[] { item, i });
                }
                L.Insert(0, new object[] { splitColumn, total });
                return L;
            }
        }


        public static bool outlookNotify(string f, bool success, string to_, string cc, string me)
        {
            try
            {
                Outlook.Application oApp = new Outlook.Application();
                Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
                if (me != null && me.Trim().Length > 0)
                {
                    Outlook.Recipient oRecip = (Outlook.Recipient)oMsg.Recipients.Add(me.Trim());
                    oRecip.Type = (int)Outlook.OlMailRecipientType.olBCC;
                }
                if (cc != null && cc.Trim().Length > 0)
                {
                    Outlook.Recipient oRecip = (Outlook.Recipient)oMsg.Recipients.Add(cc.Trim());
                    oRecip.Type = (int)Outlook.OlMailRecipientType.olCC;
                }
                string[] tos = null;
                if (to_ != null && to_.Trim().Length > 0
                    && (tos = to_.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) != null && tos.Length > 0
                    )
                    foreach (string to in tos)
                    {
                        Outlook.Recipient oRecip = (Outlook.Recipient)oMsg.Recipients.Add(to.Trim());
                        oRecip.Type = (int)Outlook.OlMailRecipientType.olTo;
                    }
                oMsg.Recipients.ResolveAll();
                oMsg.Subject = Path.GetFileName(f);
                oMsg.Body = "This is a notification from a 'AuditSec-SCCM Reporting' program instance."
                    + "\n\nA new report is available here:"
                    + "\n\n\tfile://" + f.Replace(" ", "%20")
                    + "\n\nFor further inqueries, please ask the SCCM Reporting team."
                    //+ "\n\n\tmailto:+SCCMReporting@MYCOMPANY.com"
                    + (cc != null ? "\n\n\tmailto:" + cc : "");
                /* attachment
                String sSource = "C:\\setupxlg.txt";
                String sDisplayName = "MyFirstAttachment";
                int iPosition = (int)oMsg.Body.Length + 1;
                int iAttachType = (int)Outlook.OlAttachmentType.olByValue;  
                Outlook.Attachment oAttach = oMsg.Attachments.Add(sSource,iAttachType,iPosition,sDisplayName);
                */
                // If you want to, display the message.
                // oMsg.Display(true);  //modal

                oMsg.Save();
                oMsg.Send();
                //oRecip = null;
                //oAttach = null;
                oMsg = null;
                oApp = null;
                Console.WriteLine("notifyRecipients: success.");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("notifyRecipients: failure: " + e.Message);
                return false;
            }


        }


    }
}
