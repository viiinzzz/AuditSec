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
using System.Net;

namespace AuditSec
{
    public partial class WMICollector : Form
    {
        public WMICollector()
        {
            InitializeComponent();
        }

        List<WMIQuery> queries = new List<WMIQuery>();

        private void RegCollector_Load(object sender, EventArgs e)
        {
            queries.Clear();
            table.Columns.Clear();
            table.Columns.Add("C0", "Machine");
            WMIQuery q;

            queries.Add(q = new WMIQuery("USB Printers", MachineInfo.getUSBPrinters));
            table.Columns.Add("C" + table.Columns.Count, q.name);

            queries.Add(q = new WMIQuery("USB Storage", MachineInfo.getUSBStorage));
            table.Columns.Add("C" + table.Columns.Count, q.name);

            table.AutoResizeColumns();

            this.Text = "Multiple WMI Values Collector";

            TargetButton.Visible = true;
            if (table.Columns.Count > 0)
                xls_in.ShowDialog();
        }

        bool ADOpen(SCCMCollectionDesigner.Tree<SCCMCollectionDesigner.ADNode> tree, SCCMCollectionDesigner.ADNode item)
        {
            return true;
        }



        class WMIQuery
        {
            public string name;
            public Func<string, List<string>> query;
            public WMIQuery(string name, Func<string, List<string>> query)
            {
                this.name = name;
                this.query = query;
            }
        }



        const int Print = 0;
        const int Cell_Unknown = 1;
        const int Cell_Error = 2;
        const int Finish = 100;

        const string Value_Dots = "…";
        const string Value_Batsu = "×";
        const string Value_Question = "?";

        void query(object state)
        {
            string machine = (string)state;
            List<object> result = new List<object>();
            result.Add(machine);
            worker.ReportProgress(Cell_Unknown, result.ToArray());



            if (!worker.CancellationPending) try
            {
                IPAddress IP = MachineInfo.ping(machine, false);
                if (IP == null) throw new Exception("Machine unreachable.");

                string netbios = MachineInfo.getNetbiosName(machine);
                if (netbios == null || !netbios.ToUpper().Equals(machine.ToUpper()))
                    throw new Exception("Machine unreachable.");

                foreach (WMIQuery q in queries) if (!worker.CancellationPending)
                try
                {
                        List<string> v = q.query(machine);
                        result.Add(v.Count == 0 ? "" : v.Aggregate((x, y) => x + "; " + y)); //"[]"
                        
                    Console.WriteLine(result.Aggregate((x, y) => x + ", " + y));
                    worker.ReportProgress(Cell_Unknown, result.ToArray());
                }
                catch (Exception e)
                {
                    //result.Add("Read error");
                    result.Add(Value_Batsu);
                    worker.ReportProgress(Cell_Error, result.ToArray());
                    worker.ReportProgress(Print, machine + " - WMI read error: " + e.Message + "\n" + e.ToString());
                    break;
                }
                else
                {
                    //result.Add("Aborted.");
                    result.Add(Value_Batsu);
                    worker.ReportProgress(Cell_Error, result.ToArray());
                    break;
                }
            }
            catch (Exception e)
            {
                result.Add(Value_Question);
                worker.ReportProgress(Cell_Unknown, result.ToArray());
                //Console.WriteLine(machine + " - Processing error: " + e.Message + "\n" + e.ToString());
            }

            active.Remove(machine);
        }

        List<string> active = new List<string>();
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (DataGridViewRow row in table.Rows)
            {
                if (worker.CancellationPending) break;
                string machine = row.Cells[0].Value.ToString();
                ThreadPool.QueueUserWorkItem(new WaitCallback(query), machine);
                active.Add(machine);
                if (worker.CancellationPending) break;
            }
            while (active.Count > 0)
            {
                if (worker.CancellationPending)
                    Console.WriteLine("Stop requested. Waiting remainder to abort..." + active.Count);
                else
                    Console.WriteLine("All tasks were queued. Waiting remainder to finish..." + active.Count);
                worker.ReportProgress(Finish, null);
                Thread.Sleep(10000);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0) Console.WriteLine(e.UserState);
            else if (e.ProgressPercentage == Cell_Unknown
                || e.ProgressPercentage == Cell_Error)
            {
                object[] array = (object[])e.UserState;
                string machine = array[0].ToString();
                foreach (DataGridViewRow row in table.Rows)
                {
                    if (row.Cells[0].Value.ToString() == machine)
                    {

                        for (int i = 0; i < row.Cells.Count; i++)
                        {
                            string newvalue = "" +(array.Length > i ? array[i] :
                                    (e.ProgressPercentage == Cell_Unknown ? Value_Dots :
                                    e.ProgressPercentage == Cell_Error ? Value_Batsu : "¤"));

                            string oldvalue = "" + row.Cells[i].Value;

                            if (oldvalue == Value_Question || oldvalue == Value_Dots
                            || oldvalue == Value_Batsu || oldvalue == MachineInfo.NOTREACHABLE)
                            {
                                row.Cells[i].Value = newvalue;
                            }
                            else
                            {
                                if (newvalue == Value_Question || newvalue == Value_Dots
                                || newvalue == Value_Batsu || newvalue == MachineInfo.NOTREACHABLE
                                )
                                {
                                    row.Cells[i].Value = ("" + row.Cells[i].Value).Replace(" " + Value_Batsu, "") + " " + Value_Batsu;
                                }
                                else
                                {
                                    row.Cells[i].Value = newvalue;
                                }
                            }
                        }
                        break;
                    }
                }
                Go.Text = active.Count + " Stop";
            }
            else if (e.ProgressPercentage == Finish)
            {
                Go.Text = active.Count + " !Stop";
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Go.Text = "Go";
            TargetButton.Visible = true;
            target.Visible = true;
            Save.Visible = true;
            Clear.Visible = true;
        }

        void setRow(DataGridViewRow row, string value)
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
      

        private void table_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string machine = table.Rows[e.RowIndex].Cells[0].Value.ToString();
            Console.WriteLine("Queueing " + machine + "...");
            setRow(table.Rows[e.RowIndex], Value_Dots);
            ThreadPool.QueueUserWorkItem(new WaitCallback(query), machine);
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
                        setRow(table.Rows[i], Value_Question);

                    }
                target.Text = "";
                Clear.Visible = true;
                Go.Visible = true;
                Save.Visible = true;
            }
        }


        private void Clear_Click(object sender, EventArgs e)
        {
            table.Rows.Clear();
            Clear.Visible = false;
            Go.Visible = false;
            Save.Visible = false;
            target.Text = "";
        }

        private void Go_Click(object sender, EventArgs e)
        {
            if (!worker.IsBusy)
            {
                Console.WriteLine("----------------------------------------------------------------------\n\n"
                    + "Started.\n\n"
                    + "----------------------------------------------------------------------");
                Go.Text = "Stop";
                TargetButton.Visible = false;
                target.Visible = false;
                Save.Visible = false;
                Clear.Visible = false;
                worker.RunWorkerAsync();
            }
            else
            {
                Console.WriteLine("----------------------------------------------------------------------\n\n"
                    + "Stop requested...\n\n"
                    + "----------------------------------------------------------------------");
                TargetButton.Visible = true;
                target.Visible = true;
                Save.Visible = true;
                Clear.Visible = true;
                worker.CancelAsync();
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            xls_out.ShowDialog();
        }

        private void xls_out_FileOk(object sender, CancelEventArgs e)
        {
            MSOfficeTools.exportExcel(xls_out.FileName, table, "Registry", x => {
                if (x == Value_Dots) return Value_Question;
                else if (x == Value_Batsu) return Value_Question;
                else if (x == MachineInfo.NOTREACHABLE) return Value_Question;
                else return x.Replace(Value_Dots, "");
            });
        }

        private void RegCollector_FormClosed(object sender, FormClosedEventArgs e)
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
                    int max = 10; foreach (DataRow row in t.Rows) while (max-- > 0)
                    {
                        foreach (object o in row.ItemArray) contents.Append(o + " "); contents.AppendLine();
                    }
                }

                int i = 0; foreach (DataRow row in t.Rows)
                {
                    if (i++ == 0)
                    {
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
                                if (("" + table.Columns[j].HeaderText) != ("" + o))
                                {
                                    INVALID = "The Excel column C" + j + " has a different name rather than in the template.\n\n\t'"
                                        + o + "' not equal to '" + table.Columns[j].HeaderText + "'\n\n\t" + contents;
                                    break;
                                }
                                else j++;
                            if (INVALID != null) break;
                        }
                    }
                    else try
                    {
                        table.Rows.Add(row.ItemArray);
                        table.Rows[i].HeaderCell.Value = "" + (i + 1);
                    }
                    catch (Exception ee)
                    {
                        INVALID = "While adding row " + i + ": " + ee.Message;
                    }                    
                }

                Clear.Visible = INVALID == null;
                Go.Visible = INVALID == null;
                Save.Visible = INVALID == null;

                if (INVALID != null)
                    MessageBox.Show(this, "Invalid Excel File:\n\n\t" + xls_in.FileName + "\n\n\nReason:\n\n\t" + INVALID);
            }
        }

        private void table_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && table[e.ColumnIndex, e.RowIndex].Value.ToString().Length == 0)
                showComputerChooser();
        }

        private void label_Click(object sender, EventArgs e)
        {
            showComputerChooser();
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
                    setRow(table.Rows[i], Value_Question);
                }
            }

            foreach (DataGridViewRow row in table.Rows)
            {
                row.HeaderCell.Value = "" + (row.Index + 1);
            }
            //table.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);

            if (table.Rows.Count > 0)
            {
                target.Text = "";
                Clear.Visible = true;
                Go.Visible = true;
                Save.Visible = true;
            }
        }

        private void TargetButton_Click(object sender, EventArgs e)
        {
            showComputerChooser();
        }


















    }
}
