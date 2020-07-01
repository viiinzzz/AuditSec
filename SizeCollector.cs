using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Management;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualBasic;
using System.Globalization;
using System.Web.UI;
using System.Collections;
using System.Text.RegularExpressions;

namespace AuditSec
{
    public partial class SizeCollector : Form
    {
        
        const int REPORT_NEWROWS = 0;
        const int REPORT_NEWROWS_ERROR = -1;

        const string SELECTED = "Selected";

        const string Value_Dots = "…";
        const string Value_Batsu = "×";
        const string Value_Question = "?";

        const string TRYINGTOREACH = "Trying to reach.";

        const string SIZEFORMAT = "#,0";

        const string MACHINE = "Machine";
        const string USERNAME = "Username";
        const string SCANDATE = "Scandate";
        const string TOTALFILES = "Total Files";
        const string TOTALSIZE = "Total Size (MB)";
        const string LASTMODIFIED = "Last Modified";


        public SizeCollector()
        {
            InitializeComponent();
            setButtonsEnabled(true, true);
        }

        void setButtonsEnabled(bool Enabled, bool check_Go)
        {
            target.Text = "";
            BrowseButton.Enabled = LoadButton.Enabled = target.Enabled = Enabled;
            SaveButton.Enabled = ClearButton.Enabled = Enabled && table.Rows.Count > 0;
            if (check_Go)
            {
                Go.Enabled = table.Rows.Count > 0;
                Go.Text = worker.IsBusy ? "" : "Go";
                Go.Image = worker.IsBusy ? global::AuditSec.Properties.Resources.Stop : global::AuditSec.Properties.Resources.Run;

            }
        }

        void applySizeFormat(DataGridViewCellStyle cellStyle)
        {
            cellStyle.Format = SIZEFORMAT;
            cellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        string[] COLUMNS_HEADERS = new string[] {MACHINE, USERNAME, SCANDATE, TOTALFILES, TOTALSIZE, LASTMODIFIED};
        Dictionary<string, int> COLUMNS = new Dictionary<string, int>();

        private void SizeCollector_Load(object sender, EventArgs e)
        {
            foreach(string HEADER in COLUMNS_HEADERS)
                COLUMNS[HEADER] = table.Columns.Add("C" + table.Columns.Count, HEADER);

            applySizeFormat(table.Columns[COLUMNS[TOTALFILES]].DefaultCellStyle);
            applySizeFormat(table.Columns[COLUMNS[TOTALSIZE]].DefaultCellStyle);

            foreach (string type in MachineInfo.TYPEFILEX.Keys.OrderBy(type => type))
            {
                string HEADER = type.Replace(" ", "\n");
                COLUMNS[HEADER] = table.Columns.Add("C" + table.Columns.Count, HEADER);
                applySizeFormat(table.Columns[table.Columns.Count - 1].DefaultCellStyle);
            }
            table.AutoResizeColumns();

            new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(1000);
                Invoke(new show_xls_in_delegate(show_xls_in));
            })).Start();
        }

        delegate void show_xls_in_delegate();
        void show_xls_in()
        {
            if (table.Columns.Count > 0)
            {
                xls_in.ShowDialog();
            }
        }


        void query(object state)
        {
            Tuple<string, MachineInfo.Files_Bytes> tuple = (Tuple<string, MachineInfo.Files_Bytes>)state;
            string machine = tuple.Item1;
            MachineInfo.Files_Bytes total_cb = tuple.Item2;
            List<object[]> new_rows = new List<object[]>();
            try
            {
                if (!MachineInfo.isReachableAndValidNetbios(machine))
                {
                    new_rows.Add(new object[] { machine, MachineInfo.NOTREACHABLE });
                    worker.ReportProgress(REPORT_NEWROWS_ERROR, new_rows);
                }
                else
                {
                    Dictionary<string, Tuple<int, long, DateTime, Dictionary<string, long>>> ret = MachineInfo.GetProfilesSize(machine, total_cb);

                    bool aborted = total_cb.aborted;
                    string retString = MachineInfo.GetProfilesSizeAsString(ret, 100);
                    List<object[]> retArrays = MachineInfo.GetProfilesSizeAsObjectArrayList(ret, machine);

                    if (!aborted)
                    {
                        Console.WriteLine(retString);
                        if (retArrays.Count == 0)
                        {
                            new_rows.Add(new object[] { machine, MachineInfo.NOPROFILEFOUND });
                            worker.ReportProgress(REPORT_NEWROWS, new_rows);
                        }
                        else
                        {
                            new_rows.AddRange(retArrays);
                            worker.ReportProgress(REPORT_NEWROWS, new_rows);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Aborted.");
                        new_rows.Add(new object[] { machine, Value_Batsu + (total_cb.markers.Count == 0 ? MachineInfo.NOPROFILEFOUND : total_cb.markers.Aggregate((x, y) => x + "+" + y)) });
                        worker.ReportProgress(REPORT_NEWROWS_ERROR, new_rows);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(machine + " - Processing error: " + e.Message
                    //+ "\n" + e.ToString()
                    );
            }
            
            active.Remove(tuple);
        }

        List<Tuple<string, MachineInfo.Files_Bytes>> active = new List<Tuple<string, MachineInfo.Files_Bytes>>();

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            normSearch1();
            foreach (DataGridViewRow row in table.Rows) row.Selected = false;
            while (active.Count > 0)
            {
                foreach(var tuple in active.ToArray())
                {
                    MachineInfo.Files_Bytes total_cb = tuple.Item2;
                    if (worker.CancellationPending) total_cb.aborted = true;

                    //periodic table updates
                    string machine = tuple.Item1;
                    foreach (DataGridViewRow row in table.Rows)
                    {
                        string row_machine = "" + row.Cells[0].Value;
                        string row_username = "" + row.Cells[1].Value;
                        if (row_machine == machine
                            && (row_username == Value_Question
                            || (row_username + row.Cells[1].Value).StartsWith(Value_Batsu)
                            || (row_username + row.Cells[1].Value).StartsWith(Value_Dots)
                            ))
                        {
                            row.SetValues(new object[] { machine,
                                (total_cb.aborted ? Value_Batsu : Value_Dots)
                                + (!total_cb.connected ? TRYINGTOREACH :
                                                        (total_cb.markers.Count == 0 ? MachineInfo.NOPROFILEFOUND : total_cb.markers.Aggregate((x, y) => x + "+" + y))),
                                total_cb.scandate.ToString("yyyy/MM/dd"), total_cb.files, total_cb.bytes  / (1024 * 1024)});
                            row.DefaultCellStyle.BackColor = Color.LightYellow;
                        }
                    }
                }

                if (worker.CancellationPending)
                    Console.WriteLine("Stop requested. Waiting remainder to abort..." + active.Count);
                else
                    Console.WriteLine("All tasks were queued. Waiting remainder to finish..." + active.Count);
                //worker.ReportProgress(REPORT_COMPLETE, null);
                Thread.Sleep(5000);
            }
        }


        //ReportProgress
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            bool ERROR = e.ProgressPercentage == REPORT_NEWROWS_ERROR;
            List<object[]> new_rows = (List<object[]>)e.UserState;

            updateRows(new_rows, ERROR);

            Go.Text = "" + active.Count;
            Go.Enabled = !worker.CancellationPending;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            setButtonsEnabled(true, true);
        }


        void setRowEmptyCells(DataGridViewRow row, string value)
        {
            for (int i = 1; i < row.Cells.Count; i++)
            {
                string x = "" + row.Cells[i].Value;
                if (x == ""
                    || x == Value_Dots
                    || x == Value_Question
                    || x == Value_Batsu
                    || x == MachineInfo.NOTREACHABLE)
                    row.Cells[i].Value = value;
                else row.Cells[i].Value = row.Cells[i].Value + value;
            }
        }

        List<Tuple<DataGridViewRow, string>> getRowsWhereMachineIs(string machine)
        {
            List<Tuple<DataGridViewRow, string>> result = new List<Tuple<DataGridViewRow, string>>();
            foreach(DataGridViewRow row in table.Rows)
            {
                string row_machine = "" + row.Cells[0].Value;
                string row_user = "" + row.Cells[1].Value;
                if (row_machine == machine)
                    result.Add(new Tuple<DataGridViewRow, string>(row, row_user));
            }
            return result;
        }

        void updateRows(List<object[]> new_rows, bool ERROR)
        {
            if (new_rows.Count == 0) return;

            string machine = "" + new_rows[0][0];
            string username = "" + new_rows[0][1];

            bool NOPROFILEFOUND = username == MachineInfo.NOPROFILEFOUND;
            bool NOTREACHABLE = username == MachineInfo.NOTREACHABLE;
            bool ABORTED = username.StartsWith(Value_Batsu);

            if (NOPROFILEFOUND || NOTREACHABLE)
                Console.WriteLine("updateRows " + machine + " " + new_rows[0][1]);

            List<Tuple<DataGridViewRow, string>> machine_rows = getRowsWhereMachineIs(machine);

            List<string> new_users = new_rows.Where(array => array.Length > 0 && array[1] != null)
                .Select(array => array[1].ToString()).ToList();

            List<DataGridViewRow> rowsToDelete = machine_rows.Where(tuple => !new_users.Contains(tuple.Item2))
                .Select(tuple => tuple.Item1).ToList();

            List<Tuple<DataGridViewRow, object[]>> rowsToUpdate = machine_rows.Where(tuple => new_users.Contains(tuple.Item2))
                .Select(tuple => new Tuple<DataGridViewRow, object[]>(tuple.Item1, new_rows.First(array => array[1].ToString() == tuple.Item2)))
                .Select(tuple => { tuple.Item1.DefaultCellStyle.BackColor = ERROR ? Color.LightCoral : Color.LightGreen ; return tuple; }).ToList();
            
            //add
            List<DataGridViewRow> rowsAdded = new_rows.Where(array => array.Length > 0 && array[1] != null
                && !machine_rows.Select(tuple => tuple.Item2).Contains(array[1].ToString()))
                .Select(array => { DataGridViewRow row = table.Rows[table.Rows.Add(array)];
                    row.DefaultCellStyle.BackColor = ERROR ? Color.LightCoral : Color.LightGreen; return row; }).ToList();

            //update
            foreach (Tuple<DataGridViewRow, object[]> tuple in rowsToUpdate) tuple.Item1.SetValues(tuple.Item2);
            
            //delete
            if (!ABORTED) foreach (DataGridViewRow row in rowsToDelete) table.Rows.Remove(row);

            table.Sort(table.Columns[0], ListSortDirection.Ascending);
            for(int i = 0; i < table.Rows.Count; i++) table.Rows[i].HeaderCell.Value = "" + (i + 1);
        }

        List<Dictionary<string, string>> getTableAsList()
        {
            return table.Rows.OfType<DataGridViewRow>()
                .Select(r =>
                    {
                        Dictionary<string, string> dic = r.Cells.OfType<DataGridViewCell>().ToDictionary(c => c.OwningColumn.HeaderText, c => (c.Value ?? "").ToString());
                        dic.Add(SELECTED, "" + r.Selected);
                        return dic;
                    }
                ).ToList();
        }

        void normSearch0()
        {
            foreach (DataGridViewRow row in table.Rows)
                if (row.Cells[COLUMNS[USERNAME]].Value == MachineInfo.NOTREACHABLE)
                    row.Cells[COLUMNS[USERNAME]].Value = Value_Question;

            List<Dictionary<string, string>> data = getTableAsList();
            foreach (object[] values in data.Where(row => row[USERNAME] != Value_Question).Select(row => row[MACHINE]).Distinct()
                 .Where(computer => data.Where(row => row[MACHINE] == computer && row[USERNAME] == Value_Question).Count() == 0)
                 .Select(computer => new object[] { computer, Value_Question }))
                table.Rows.Add(values);

            table.Sort(table.Columns[0], ListSortDirection.Ascending);
        }

        void normSearch1()
        {
            List<string> machines = getTableAsList()
                .Where(row => row[USERNAME] == Value_Question && row[SELECTED].ToLower() == "true")
                .Select(row => row[MACHINE]).Distinct().ToList();
            foreach (Tuple<string, MachineInfo.Files_Bytes> tuple in machines.Select(machine => new Tuple<string, MachineInfo.Files_Bytes>(machine, new MachineInfo.Files_Bytes())))
            {
                if (worker.CancellationPending) break;
                ThreadPool.QueueUserWorkItem(new WaitCallback(query), tuple);
                active.Add(tuple);
            }
        }

      

        private void target_TextChanged(object sender, EventArgs e)
        {
            if (table.Columns.Count > 0 && target.Text.IndexOf('\n') >= 0)
            {
                //table.Rows.Clear();
                string[] machines = target.Text.ToUpper().Split(new char[] { '\r', '\n', '\t', ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (machines != null) foreach (string machine in machines)
                    {
                        int i = table.Rows.Add(new object[] { machine });
                        table.Rows[i].HeaderCell.Value = "" + (i + 1);
                        setRowEmptyCells(table.Rows[i], Value_Question);

                    }
                target.Text = "";
                ClearButton.Enabled = true;
                Go.Enabled = true;
                SaveButton.Enabled = true;
            }
        }


        private void Clear_Click(object sender, EventArgs e)
        {
            table.Rows.Clear();
            setButtonsEnabled(true, true);
        }

        private void Go_Click(object sender, EventArgs e)
        {
            if (!worker.IsBusy)
            {
                Console.WriteLine("----------------------------------------------------------------------\n\n"
                    + "Started.\n\n"
                    + "----------------------------------------------------------------------");
                int n_selected = 0; foreach (DataGridViewRow row in table.Rows) if (row.Selected) n_selected++;
                if (n_selected == 0) foreach (DataGridViewRow row in table.Rows) row.Selected = true;
                normSearch0();
                worker.RunWorkerAsync();
                setButtonsEnabled(false, true);
            }
            else
            {
                Console.WriteLine("----------------------------------------------------------------------\n\n"
                    + "Stop requested...\n\n"
                    + "----------------------------------------------------------------------");
                worker.CancelAsync();
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            xls_out.ShowDialog();
        }

        private void xls_out_FileOk(object sender, CancelEventArgs e)
        {
            MSOfficeTools.exportExcel(xls_out.FileName, table, GetType().Name, x => {
                if (x == Value_Dots) return Value_Question;
                else if (x == Value_Batsu) return Value_Question;
                else if (x == MachineInfo.NOTREACHABLE) return Value_Question;
                else return x.Replace(Value_Dots, "");
            });
        }

        private void SizeCollector_FormClosed(object sender, FormClosedEventArgs e)
        {
            AuditSec.Exit(this.Text, null);
        }

        private void xls_in_FileOk(object sender, CancelEventArgs e)
        {
            System.Data.DataTable t = MSOfficeTools.loadExcelFile(xls_in.FileName, false);
            if (t != null)
            {
                string INVALID = null;

                StringBuilder contents = new StringBuilder();
                {
                    int max = 10; foreach (DataRow row in t.Rows)
                        if (max-- == 0) break;
                        else
                        {
                            foreach (object o in row.ItemArray) contents.Append(o + " ");
                            contents.AppendLine();
                        }
                }

                bool first = true; foreach (DataRow row in t.Rows) if (row.ItemArray.Length > 0)
                {
                    if (first)
                    {
                        first = false;
                        DataRow header = row;
                        int colCount = header.ItemArray.Length;
                        if (colCount != table.Columns.Count)
                        {
                            INVALID = "The Excel workseet has " + (colCount > table.Columns.Count ? "more" : "less")
                                + " columns than the template.\n"
                                + colCount + (colCount > table.Columns.Count ? " > " : " < ") + table.Columns.Count;
                            break;
                        }
                        else
                        {
                            int j = 0;
                            foreach (object o in header.ItemArray)
                                if (("" + table.Columns[j].HeaderText.Replace('\n', ' ')) != ("" + o))
                                {
                                    INVALID = "The Excel column C" + j + " has a different name rather than in the template.\n\n\t'"
                                        + o + "' not equal to '" + table.Columns[j].HeaderText.Replace('\n', ' ') + "'\n\n\t" + contents.Replace(' ', '\u00A0');
                                    break;
                                }
                                else j++;
                            if (INVALID != null) break;
                        }
                    }
                    else
                    {
                        try
                        {
                            int i = table.Rows.Add(row.ItemArray);
                            table.Rows[i].HeaderCell.Value = "" + (i + 1);
                        }
                        catch (Exception ee)
                        {
                            INVALID = "While adding row " + (table.Rows.Count + 1) + ": " + ee.Message;
                        }
                    }
                }

                table.Sort(table.Columns[0], ListSortDirection.Ascending);

                bool VALID = INVALID == null;
                SaveButton.Enabled = Go.Enabled = ClearButton.Enabled = VALID;
                Hint2.Visible = Hint.Visible = !VALID;

                if (INVALID != null)
                    MessageBox.Show(this, "Invalid Excel File:\n\n\t" + xls_in.FileName + "\n\n\nReason:\n\n\t" + INVALID);

            }
        }



        ComputerChooser cc = new ComputerChooser();
        void showComputerChooser()
        {
            cc.ShowDialog(this);
            foreach (string machine in cc.getSelectedMachines().Select(x => x.Trim().ToUpper()))
            {
                bool found = false;
                foreach (DataGridViewRow row in table.Rows)
                    if (machine == "" + row.Cells[0].Value)
                {
                    found = true;
                    break;
                }
                if (!found)
                {
                    int i = table.Rows.Add(new object[] { machine });
                    setRowEmptyCells(table.Rows[i], Value_Question);
                }
            }

            foreach (DataGridViewRow row in table.Rows)
            {
                row.HeaderCell.Value = "" + (row.Index + 1);
            }
            //table.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);

            setButtonsEnabled(true, true);
        }

        private void table_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            setButtonsEnabled(true, true);
        }

        private void table_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            setButtonsEnabled(true, true);
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            Hint2.Visible = Hint.Visible = false;
            showComputerChooser();
        }

        private void Hint_Click(object sender, EventArgs e)
        {
            Hint.Visible = false;
        }

        private void target_Click(object sender, EventArgs e)
        {
            Hint2.Visible = Hint.Visible = false;
        }

        private void Hint2_Click(object sender, EventArgs e)
        {
            Hint2.Visible = false;
        }

        private void Load_Click(object sender, EventArgs e)
        {
            Hint2.Visible = Hint.Visible = false;
            xls_in.ShowDialog();
        }

        bool request_Go = false;
        private void table_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (worker.IsBusy) return;

            //single machine update job
            foreach (DataGridViewRow row in table.Rows) row.Selected = false;
            table.Rows[e.RowIndex].Selected = true;
            request_Go = true;
            Console.WriteLine("----------------------------------------------------------------------\n\n"
                + "Row selection: " + table.Rows[e.RowIndex].Cells[COLUMNS[MACHINE]].Value + "\n\n"
                + "----------------------------------------------------------------------");
        }

        private void table_SelectionChanged(object sender, EventArgs e)
        {
            if (worker.IsBusy) return;

            //select all rows that have the same machine names that are already selected.
            List<string> selected_machines = getTableAsList().Where(row => row[SELECTED] == "" + true).Select(row => row[MACHINE]).Distinct().ToList();
            Console.WriteLine("----------------------------------------------------------------------\n\n"
                + "Selected machines: " + (selected_machines.Count > 0 ? selected_machines.Aggregate((x, y) => x + ", " + y) : "none.") + "\n\n"
                + "----------------------------------------------------------------------");
            foreach (DataGridViewRow row in table.Rows)
                if (selected_machines.Contains(row.Cells[COLUMNS[MACHINE]].Value)) row.Selected = true;

            //start the scan job if requested
            if (selected_machines.Count > 0 && request_Go)
            {
                Console.WriteLine("----------------------------------------------------------------------\n\n"
                    + "Start requested...\n\n"
                    + "----------------------------------------------------------------------");
                Go.PerformClick();
            }
            request_Go = false;
        }







    }
}
