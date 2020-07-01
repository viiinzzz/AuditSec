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
    public partial class RegCollector : Form
    {
        public RegCollector()
        {
            InitializeComponent();
        }

        private void RegCollector_Load(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(3000);
                Invoke(new show_reg_delegate(show_reg));
            })).Start();
        }

        public delegate void show_reg_delegate();
        void show_reg()
        {
            reg.ShowDialog(this);
        }

        public delegate void show_xls_in_delegate();
        void show_xls_in()
        {
           xls_in.ShowDialog(this);
        }

        bool ADOpen(SCCMCollectionDesigner.Tree<SCCMCollectionDesigner.ADNode> tree, SCCMCollectionDesigner.ADNode item)
        {
            //if (tree.shown()) tree.GUI.update(tree.tree, true);
          //bool ret = item.uptodate || ADRefresh(tree, item.node, item, true);
            //if (tree.shown()) tree.GUI.sort(tree.tree);
            //if (tree.shown()) tree.GUI.update(tree.tree, false);
            return true;// ret;
        }

        List<RegQuery> queries = new List<RegQuery>();
        Dictionary<RegQuery, List<string>> filters_def = new Dictionary<RegQuery, List<string>>();
        Dictionary<RegQuery, Tuple<Func<string, bool>, Func<string, bool>>> filters_pat = new Dictionary<RegQuery, Tuple<Func<string, bool>, Func<string, bool>>>();

        bool TIMESTAMPING = true;
        int COLCOUNT = 0;

        private void reg_FileOk(object sender, CancelEventArgs e)
        {
            reg_Load(File.ReadAllLines(reg.FileName));
        }

        void reg_Load(string[] reg_lines)
        {
            hintUseTempl1.Visible = false;

            COLCOUNT = 0;
            queries.Clear();
            table.Columns.Clear();
            table.Columns.Add("C0", "Machine");
            string hive = "", key = ""; bool used = true, line1 = true;
            List<string> filter = new List<string>(), global_filter = new List<string>();
            foreach(string line_ in reg_lines)
            {
                string line = line_.Trim();
                if (line1)
                {
                    line1 = false;
                    if (line != "Windows Registry Editor Version 5.00")
                    {
                        MessageBox.Show("Not a valid .reg file !", "Multiple PC Registry Collector");
                        break;
                    }
                }
                else if (line.Length == 0 || line.StartsWith("//"))
                {
                    ;
                }
                else if (line.StartsWith("["))
                {
                    if (filters_def.Count == 0 && filter.Count > 0)
                    {
                        global_filter.AddRange(filter);
                        filter.Clear();
                    }

                    if (!used)
                    {
                        var q = new RegQuery(hive, key, null);
                        queries.Add(q);

                        List<string> filter2 = new List<string>(global_filter); filter2.AddRange(filter);
                        filters_def.Add(q, filter2);
                    }
                    key = line.Substring(1, line.IndexOf(']') - 1);
                    hive = key.Substring(0, key.IndexOf('\\'));
                    key = key.Substring(hive.Length + 1);
                    used = false;
                    filter.Clear();
                    //Console.WriteLine("Hive: " + hive);
                    //Console.WriteLine("Key: " + key);
                }
                else if (line.StartsWith("\""))
                {
                    string param = line.Substring(1, line.IndexOf('"', 1) - 1);
                    //Console.WriteLine("Param: " + param);
                    var q = new RegQuery(hive, key, param);
                    queries.Add(q);

                    List<string> filter2 = new List<string>(global_filter); filter2.AddRange(filter);
                    filters_def.Add(q, filter2); filter = new List<string>();
                    used = true;
                }
                else filter.Add(line);
            }
            if (!used)
            {
                var q = new RegQuery(hive, key, null);
                queries.Add(q);

                List<string> filter2 = new List<string>(global_filter); filter2.AddRange(filter);
                filters_def.Add(q, filter2);
            }

            foreach(var kv in filters_def)
            {
                var isValue_not_pattern_Array = kv.Value.Select(s => {
                    bool not = s.StartsWith("!"),
                        v = s.ToLower().StartsWith("value ")||s.ToLower().StartsWith("!value "),
                        p = s.ToLower().StartsWith("param ")||s.ToLower().StartsWith("!param ");
                    string vs = v ? (not ? s.Substring(7) : s.Substring(6)) : null, ps = p ? (not ? s.Substring(7) : s.Substring(6)) : null;
                    return v ? new Tuple<int, bool, string>(1, not, vs) : p ? new Tuple<int, bool, string>(-1, not, ps) : new Tuple<int, bool, string>(0, not, s);
                }).Where(t => t.Item1 != 0 && IsValidRegex(t.Item3))
                .Select(t => new Tuple<bool, bool, Regex>(t.Item1 == 1, t.Item2, new Regex(t.Item3)));
                
                var not_pattern_ValueArray = isValue_not_pattern_Array.Where(t => t.Item1).Select(t => new Tuple<bool, Regex>(t.Item2, t.Item3));
                var not_pattern_ParamArray = isValue_not_pattern_Array.Where(t => !t.Item1).Select(t => new Tuple<bool, Regex>(t.Item2, t.Item3));

                Func<string, bool> filterValue = s => {
                    if (not_pattern_ValueArray.Count() == 0) return true;
                    bool not_match = true;
                    foreach(var not_pattern in not_pattern_ValueArray.Where(t => t.Item1))//-
                        if (not_pattern.Item2.IsMatch(s)) {not_match = false; break;}
                    bool match = not_pattern_ValueArray.Where(t => !t.Item1).Count() == 0; //false;
                    foreach(var not_pattern in not_pattern_ValueArray.Where(t => !t.Item1))//+
                        if (not_pattern.Item2.IsMatch(s)) {match = true; break;}
                    return match && not_match;
                };

                Func<string, bool> filterParam = s => {
                    if (not_pattern_ParamArray.Count() == 0) return true;
                    bool not_match = true;
                    foreach(var not_pattern in not_pattern_ParamArray.Where(t => t.Item1))//-
                        if (not_pattern.Item2.IsMatch(s)) { not_match = false; break; }
                    bool match = not_pattern_ParamArray.Where(t => !t.Item1).Count() == 0; //false;
                    foreach(var not_pattern in not_pattern_ParamArray.Where(t => !t.Item1))//+
                        if (not_pattern.Item2.IsMatch(s)) {match = true; break;}
                    return match && not_match;
                };

                filters_pat.Add(kv.Key, new Tuple<Func<string, bool>, Func<string, bool>> (filterParam, filterValue));
            }


            //int trim = getPathTrim(queries);
            //string title = queries[0].path.Substring(0, trim);
            string title = Path.GetFileNameWithoutExtension(reg.FileName);

            this.Text = "Multiple PC Registry Collector - " + title;
            foreach (RegQuery q in queries)
            {
//                string col = (queries.Count <= 1 ? q.param : ((trim > 0 ? "..." : "") + q.path.Substring(trim))) + (q.paramListingRequested ? "*" : "");
                string col = q.path  + (q.paramListingRequested ? "*" : "");
                //Console.WriteLine("Adding column: " + col);
                table.Columns.Add("C" + table.Columns.Count, col.Replace("\\", "\n"));
            }

            COLCOUNT = table.Columns.Count;
            if (TIMESTAMPING)
            {
                foreach (RegQuery q in queries)
                {
                    string flt = filters_def[q].Count == 0 ? "" : filters_def[q].Aggregate((x, y) => x + "\n" + y);
                    table.Columns.Add("C" + table.Columns.Count, flt);
                }
            }

            //            table.AutoResizeColumns();
            RegCollector_ResizeEnd(null, null);

            Hint0.Visible = RegButton.Visible = COLCOUNT == 0;
            HintTarget.Visible = HintOpen.Visible = XLButton.Visible = ADButton.Visible = target.Visible = !RegButton.Visible;

            /*
            if (table.Columns.Count > 0)
                new Thread(new ThreadStart(delegate
                {
                    Invoke(new show_xls_in_delegate(show_xls_in));
                })).Start();
            */
        }


        private static bool IsValidRegex(string pattern)
        {
            try
            {
                if (string.IsNullOrEmpty(pattern))
                    return false;
                Regex.Match("", pattern);
                return true;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid pattern: " + pattern);
                return false;
            }
        }



        string valueReplacer(object value)
        {
            if (value == null) return null;
            string ret = value.ToString();
            ret = ret.Replace("&", "");
            ret = Regex.Replace(ret, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
            return ret;
        }


        class RegQuery
        {
            public MachineInfo.RegHive hive;
            public string key;
            public string param;
            public string path;
            public RegQuery(string hive, string key, string param)
            {
                this.hive = MachineInfo.getHive(hive);
                this.key = key;
                this.param = param;
                this.path = MachineInfo.getShortHiveString(hive) + "\\" + key + "\\" + (param == null ? "" : param);
            }
            public bool paramListingRequested { get { return param == null; } }
        }
        /*
        int getPathTrim(List<RegQuery> queries)
        {
            if (queries.Count == 0) return 0;
            int i = 0;
            List<string> paths = queries.Select(q => q.path).ToList();
            bool hivepassed = false;
            bool cont = true; while(cont)
            {
                //Console.WriteLine(i + ": " + paths.Select(p => i >= p.Length ? "¤" : "" + p[i]).Aggregate((x, y) => x + ", " + y)); 
                if (!hivepassed)
                {
                    if (paths.Any(p => i < p.Length && p[i] == '\\'))
                        hivepassed = true;
                }

                if (i >= paths[0].Length) break;
                else if (paths.Any(p => i >= p.Length || p[i] != paths[0][i])) break;
                else i++;
                
            }
            return hivepassed ? i : 0;
        }
        */

        const int Print = 0;
        const int Cell_Unknown = 1;
        const int Cell_Error = 2;
        const int Finish = 100;

        const string Value_Dots = "…";
        const string Value_Batsu = "×";
        const string Value_Question = "?";

        string Now { get { return DateTime.Now.ToString("yyMMdd"); } }
        Tuple<object, string> stamped(object o) { return new Tuple<object, string>(o, Now); }
        void Add(object o, List<Tuple<object, string>> result) { result.Add(stamped(o)); }

        void query(object state)
        {
            string machine = (string)state;
            List<Tuple<object, string>> result = new List<Tuple<object, string>>();
            Add(machine, result);
            try
            {
                worker.ReportProgress(Cell_Unknown, result.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(machine + " results can't be reported to the table."); 
                return;
            }
            if (!worker.CancellationPending) try
            {
                MachineInfo.WmiReg wmiReg = MachineInfo.getWmiReg(machine, null, null);
                if (!wmiReg.Connected)
                {
                    Add(wmiReg.Error, result);
                    worker.ReportProgress(Cell_Error, result.ToArray());
                    worker.ReportProgress(Print, machine + " - " + wmiReg.Error);
                }
                else
                {
                    List<string[]> profiles = new List<string[]>();
                    try
                    {
                        var subkeys = MachineInfo.GetRegistrySubKeys(wmiReg, MachineInfo.RegHive.HKEY_USERS, "")
                            .Where(k => !k.EndsWith("_Classes") && Regex.IsMatch(k, ".*-.*-.*-.*-.*"));
                        if (subkeys == null) throw new Exception("No user found.");
                        foreach (string sid in subkeys)
                        {
                            object v = MachineInfo.GetRegistryValue(wmiReg, MachineInfo.RegHive.HKEY_USERS,
                                sid + "\\Volatile Environment", "USERNAME");
                            if (v != null)
                            {
                                string username = v.ToString();
                                profiles.Add(new string[] { sid, username });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        worker.ReportProgress(Print, machine + " - Registry Hives read error: " + e.Message + "\n" + e.ToString());
                        profiles = null;
                    }

                    foreach (RegQuery q in queries)
                        if (!worker.CancellationPending)
                        {

                            
                            if (q.hive.Equals(MachineInfo.RegHive.HKEY_CURRENT_USER))
                            {
                                if (profiles != null)
                                    try
                                    {
                                        List<string[]> values = profiles.Select(profile => new { sid = profile[0], username = profile[1] }).Select(su => {
                                            object v = q.paramListingRequested ?
                                                MachineInfo.GetRegistrySubkeysValues(wmiReg, MachineInfo.RegHive.HKEY_USERS, su.sid + "\\" + q.key,
                                                    filters_pat[q].Item1, valueReplacer, filters_pat[q].Item2)
                                                :  MachineInfo.GetRegistryValue(wmiReg, MachineInfo.RegHive.HKEY_USERS, su.sid + "\\" + q.key, q.param);
                                            string s = v == null ? null : v.ToString();
                                            return new string[]{ su.username, s };
                                        }).ToList();
                                        Add(values.Count == 0 ? "" : values.Select(us => us[1]).Aggregate((x, y) => x + "\n" + y), result);//"[]"
                                    }
                                    catch (Exception e)
                                    {
                                        Add(Value_Batsu, result);
                                        worker.ReportProgress(Cell_Error, result.ToArray());
                                        worker.ReportProgress(Print, machine + " - Registry Hives read error: " + e.Message + "\n" + e.ToString());
                                        break;
                                    }
                                else
                                {
                                    Add(Value_Batsu, result);
                                    worker.ReportProgress(Cell_Error, result.ToArray());
                                    break;
                                }
                            }

                            else //not HKCU
                            {
                                object v = q.paramListingRequested ?
                                    MachineInfo.GetRegistrySubkeysValues(wmiReg, q.hive, q.key, filters_pat[q].Item1, valueReplacer, filters_pat[q].Item2)
                                    : valueReplacer(MachineInfo.GetRegistryValue(wmiReg, q.hive, q.key, q.param));
                                string s = v == null ? null : v.ToString();
                                Add(s == null ? "" : s, result);//"[]"
                            }
                        }
                    //Console.WriteLine(result.Aggregate((x, y) => x + ", " + y));
                    worker.ReportProgress(Cell_Unknown, result.ToArray());
                    

                    wmiReg.Disconnect();
                }  
            }
            catch (Exception e)
            {
                //result.Add("Processing error");
                Add(Value_Batsu, result);
                worker.ReportProgress(Cell_Error, result.ToArray());
                Console.WriteLine(machine + " - Processing error: " + e.Message
                    //+ "\n" + e.ToString()
                    );
            }

            active.Remove(machine);
        }




        List<string> active = new List<string>();
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var todo = table.Rows.Cast<DataGridViewRow>().Select(row => new {
                machine = row.Cells[0].Value.ToString(),
                status = row.Cells[1].Value.ToString()
            }).ToList();
            var todo1st = todo.Where(item => item.status == Value_Question || item.status == Value_Dots).ToList();
            var todo2nd = todo.Where(item => item.status == MachineInfo.NOTREACHABLE).ToList();
            var todo3rd = todo.Where(item => !todo1st.Contains(item) && !todo2nd.Contains(item)).ToList();
            todo.Clear(); todo.AddRange(todo1st); todo.AddRange(todo2nd); todo.AddRange(todo3rd);

            ThreadPool.SetMaxThreads(50, 50);
            foreach (var item in todo)
            {
                if (worker.CancellationPending) break;
                ThreadPool.QueueUserWorkItem(new WaitCallback(query), item.machine);
                active.Add(item.machine);
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
                Tuple<object, string>[] array = (Tuple<object, string>[])e.UserState;
                string machine = array[0].Item1.ToString();
                //string stamp = array[0].Item2.ToString();
                foreach (DataGridViewRow row in table.Rows) if (row.Cells[0].Value.ToString() == machine)
                {

//                        if (TIMESTAMPING) row.Cells[COLCOUNT].Value = stamp;
                    for (int i = 1; i < COLCOUNT/*row.Cells.Count*/; i++)
                    {
                        if (array.Length <= i) break;
                        string stamp = array[i].Item2;
                        string newvalue = "" +(array.Length > i ? array[i].Item1 :
                                (e.ProgressPercentage == Cell_Unknown ? Value_Dots :
                                e.ProgressPercentage == Cell_Error ? Value_Batsu : "¤"));

                        string oldvalue = "" + row.Cells[i].Value;

                        if (oldvalue == Value_Question || oldvalue == Value_Dots
                        || oldvalue == Value_Batsu || oldvalue == MachineInfo.NOTREACHABLE)
                        {
                            row.Cells[i].Value = newvalue;
                            if (TIMESTAMPING) row.Cells[i + COLCOUNT - 1].Value = stamp;
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
                                if (TIMESTAMPING) row.Cells[i + COLCOUNT - 1].Value = stamp;
                            }
                        }
                    }


                    break;
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
            XLButton.Visible = ADButton.Visible = target.Visible = Save.Visible = Clear.Visible = true;
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
            HintOpen.Visible = HintTarget.Visible = false;
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
            Clear.Visible = Go.Visible = Save.Visible = false;
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
                HintTarget.Visible = HintOpen.Visible = Hint0.Visible = XLButton.Visible = ADButton.Visible = target.Visible = Save.Visible = Clear.Visible = false;
                worker.RunWorkerAsync();
            }
            else
            {
                Console.WriteLine("----------------------------------------------------------------------\n\n"
                    + "Stop requested...\n\n"
                    + "----------------------------------------------------------------------");
                XLButton.Visible = ADButton.Visible = target.Visible = Save.Visible = Clear.Visible = true;
                worker.CancelAsync();
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            xls_out.ShowDialog();
        }

        private void xls_out_FileOk(object sender, CancelEventArgs e)
        {
            new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(1000);
                Invoke(new show_xls_out_delegate(show_xls_out));
            })).Start();

        }


        public delegate void show_xls_out_delegate();
        void show_xls_out()
        {
            int col = 0;
            MSOfficeTools.exportExcel(xls_out.FileName, table, "Registry",
                x =>
                {
                    if (x == Value_Dots) x = Value_Question;
                    else if (x == Value_Batsu) x = Value_Question;
                    else if (x == MachineInfo.NOTREACHABLE) x = Value_Question;
                    else
                    {
                        x = x.Replace(Value_Dots, "");
                        x = x.Replace('\n', '¤');
                    } return x;
                }, x => "C" + (col++) + ":\n" + x.Replace('\n', '¤'));
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
                    int maxrow = 5; foreach (DataRow row in t.Rows)
                    {
                        int maxcol = 5;
                        foreach (object o in row.ItemArray) { contents.Append(o + " "); if (maxcol-- < 0) break; }
                        contents.AppendLine(); if (maxrow-- < 0) break;
                    }
                }

                int i = 0; foreach (DataRow row in t.Rows)
                {
                    if (i == 0)
                    {
                        DataRow header = row;
                        int colCount = header.ItemArray.Length;
                        if (colCount != table.Columns.Count)
                        {
                            INVALID = "The Excel workseet has " + (colCount > table.Columns.Count ? "more" : "less")
                                + " columns than the template.\n"
                                + colCount + (colCount > table.Columns.Count ? " > " : " < ") + table.Columns.Count
                                + "'\n\nSheet starts with:\n" + contents;
                            break;
                        }
                        else
                        {
                            int j = 0;
                            foreach (object o in header.ItemArray)
                            {
                                string curcol = "" + table.Columns[j].HeaderText;
                                string col = Regex.Replace(("" + o).Replace("¤", "\n"), @"^C\d+:\n", "");
                                if (curcol != col)
                                {
                                    INVALID = "The Excel column C" + j + " has a different name rather than in the template.\n\n\t'"
                                        + ("" + o).Replace("¤", "\n").Replace("\r", "\\r").Replace("\n", "\\n")
                                        + "' not equal to '" + table.Columns[j].HeaderText.Replace("\r", "\\r").Replace("\n", "\\n")
                                        + "'\n\nSheet starts with:\n" + contents;
                                    break;
                                }
                                else j++;
                            }
                            if (INVALID != null) break;
                        }
                    }
                    else
                    {
                        int j=0; try
                        {
                            table.Rows.Add();
                            for (j = 0; j < table.Rows[i-1].Cells.Count; j++)
                                if (j < row.ItemArray.Length) table.Rows[i-1].Cells[j].Value = row.ItemArray[j];
                            //table.Rows.Add(row.ItemArray);
                            table.Rows[i-1].HeaderCell.Value = "" + i;
                        }
                        catch (Exception ee)
                        {
                            INVALID = "While adding row " + i + ":\n" + ee.Message + "\n\n"
                            //+ " i=" + i + " table.Rows.Count=" + table.Rows.Count
                            //+ " j=" + j + " table.Rows[i].Cells.Count=" + table.Rows[i].Cells.Count
                            //+ " row.ItemArray.Length=" + row.ItemArray.Length
                            ;
                        }
                    }
                    i++;
                }

                Clear.Visible = Go.Visible = Save.Visible = INVALID == null;
                if (Go.Visible) Hint0.Visible = HintTarget.Visible = HintOpen.Visible = false;

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
            HintOpen.Visible = HintTarget.Visible = false;
            showComputerChooser();
        }

        private void RegButton_Click(object sender, EventArgs e)
        {
            reg.ShowDialog();
        }

        private void XlsButton_Click(object sender, EventArgs e)
        {
            HintOpen.Visible = HintTarget.Visible = false;
            xls_in.ShowDialog();
        }

        private void table_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            Hint0.Visible = false;
        }

        private void Hint0_Click(object sender, EventArgs e)
        {
            Hint0.Visible = false;
        }

        private void HintTarget_Click(object sender, EventArgs e)
        {
            HintTarget.Visible = false;
        }

        private void HintOpen_Click(object sender, EventArgs e)
        {
            HintOpen.Visible = false;
        }

        private void RegCollector_ResizeEnd(object sender, EventArgs e)
        {
            if (COLCOUNT > 0)
            {
                //table.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
                int W = Size.Width - table.RowHeadersWidth - 25;
                int w = W / COLCOUNT;
                foreach (DataGridViewColumn col in table.Columns) col.Width = w;
            }
        }

        private void hintUseTempl1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Resources.ResourceManager rm = new System.Resources.ResourceManager(
                        "AuditSec.Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
                object obj = rm.GetObject("IE_Toolbars");
                String reg_lines = Encoding.UTF8.GetString((byte[])obj);
                reg_Load(reg_lines.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
            }
            catch (Exception ee)
            {
                Console.WriteLine("Error while opening template IT Toolbars.reg: " + ee.Message + "\n" + ee.StackTrace);
            }
        }
























    }
}
