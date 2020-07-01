using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Management;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic;
using System.DirectoryServices.AccountManagement;
using System.Data.OleDb;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;


namespace AuditSec
{
    public partial class HotApps : Form
    {
        static public string include = @"Adobe Reader;Adobe Acrobat;Java\(;Project ;Visio ;VPN Client;Cisco AnyConnect Secure;Firefox ;%PasswordBank%Client%;Flash Player;Evidian Enterprise";
        static public string exclude = @"%Java%Dev%Kit%;Hotfix;Service Pack;Language Pack;Compatibility Pack;MUI \(;IME \(;Update;Office Proof;\.NET Framework;Web Components;SQL Server;Redistributable;\(KB;Viewer;%PasswordBank%Plugin%;%font%Adobe%";

        MachineInfo mi = null;
        public TextBox adminsListBox = null;
        public HotApps()
        {
            Console.WriteLine("Instanciating HotApps...");
            InitializeComponent();
            this.Height = 210;
            includeBox.Text = include;
            excludeBox.Text = exclude;


            string curver = "?";
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                curver = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            else
            {
                curver = Assembly.GetEntryAssembly().GetName().Version.ToString();
                //curver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                //curver = Assembly.GetCallingAssembly().GetName().Version.ToString();
            }
            if (curver == null || curver.Length == 0) curver = "?";
            this.Text += " v" + (curver.EndsWith(".0") ? curver.Substring(0, curver.Length - 2) : curver);


            machineBigBox.Text = System.Windows.Forms.SystemInformation.ComputerName;
            mi = MachineInfo.getMachine(machineBigBox.Text);
            mi.calc(false);
            query();
            //copyAppsToClipboard();
        }


        void saveSettings(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            AuditSec.Exit("HotApps module ended.", () => { base.Dispose(disposing); return true; });
        }






        private void query()
        {
            UseWaitCursor = true;
            refreshButton.Visible = false;
            adminsBox.Items.Clear();
            adminsBox.Text = "";

                wmitarget = null;
                loggedinBox.Text = "";
                makermodelBox.Text = "";
                chassisBox.Text = "";
                freeBox.Text = "";
                if (wmiWorker.IsBusy) wmiWorker.CancelAsync();

                insttarget = null;
                instAppsTable.Rows.Clear();
                ieLabel.Text = "?"; ieLabel.Visible = false;
                firefoxLabel.Text = "?"; firefoxLabel.Visible = false;
                javaLabel.Text = "?"; javaLabel.Visible = false;
                acrobatLabel.Text = "?"; acrobatLabel.Visible = false;
                flashLabel.Text = "?"; flashLabel.Visible = false;
                vpnLabel.Text = "?"; vpnLabel.Visible = false;
                ssoLabel.Text = "?"; ssoLabel.Visible = false;

                instAppsLabel.Text = "Installed Applications";
                if (instAppsWorker.IsBusy) instAppsWorker.CancelAsync();

            Thread.Sleep(100);




            string desc = mi.ToString();





            Console.WriteLine("Getting " + machineBigBox.Text + "\\Administrators members...");
            Cursor.Current = Cursors.WaitCursor;
            string admins = MachineInfo.getGroupMembers(machineBigBox.Text, "Administrators", null, null, true, false,
                (adminsListBox != null ? adminsListBox.Text : AuditSec.defaultAdmins));
            Cursor.Current = Cursors.Default;
            if (admins == null)
            {
                //Console.WriteLine(machine + "\\Administrators members: ? (not available)");
                makermodelBox.Text = machineBigBox.Text + " is not available.";
                chassisBox.Text = "";
                freeBox.Text = "Promote/Revoke Admin is not possible.";
                //MessageBox.Show(machine + " is not available.\n"
                //+ "\nPromote/Revoke Admin is not possible.", machine);
                return;
            }
            
            //Console.WriteLine(machine + "\\Administrators members:\n" + admins + "\n");
            adminsBox.BeginUpdate();
            foreach (string u in admins.Split(new Char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string admin = u.Trim().ToUpper();
                if (admin.Length > 0) adminsBox.Items.Add(admin);
            }
            adminsBox.EndUpdate();

            Thread.Sleep(1000);
            if (wmitarget == null && !wmiWorker.IsBusy) wmiWorker.RunWorkerAsync();
            if (insttarget == null && !instAppsWorker.IsBusy) instAppsWorker.RunWorkerAsync();
        }


        string wmitarget = "";
        private void wmiWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string machine = machineBigBox.Text;
            wmitarget = machine;
            //Console.WriteLine("wmiWorker_DoWork begin: " + machine);
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker.CancellationPending) { e.Cancel = true; return; }
            worker.ReportProgress(1, "Querying WMI...");
            string u = MachineInfo.getCurrentUsers(machine);
            u = u == null ? "N/A" : u.TrimEnd('\n').Replace("\n", ", ");
            worker.ReportProgress(1, u);
            if (worker.CancellationPending) { e.Cancel = true; return; }
            worker.ReportProgress(2, "Querying WMI...");
            worker.ReportProgress(2,
                MachineInfo.getMakerModel(machine) + " S/N "
                + MachineInfo.getSerialNumber(machine));
            if (worker.CancellationPending) { e.Cancel = true; return; }
            worker.ReportProgress(3, "Querying WMI...");
            worker.ReportProgress(3,
                MachineInfo.getChassisType(machine) + " / "
                + MachineInfo.getMB(machine) + " / "
                + MachineInfo.getMhz(machine));
            if (worker.CancellationPending) { e.Cancel = true; return; }
            worker.ReportProgress(4, "Querying WMI...");
            worker.ReportProgress(4,
                MachineInfo.getGB(machine));

            if (worker.CancellationPending) { e.Cancel = true; return; }
            worker.ReportProgress(5, "?"); worker.ReportProgress(5, String.Format(CultureInfo.InvariantCulture, "{0:0.0}", MachineInfo.getIEVersion(machine)));

            //Console.WriteLine("wmiWorker_DoWork finish: " + machine + (e.Cancel ? " - Cancel -" : ""));
        }

        private void wmiWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int n = e.ProgressPercentage;
            string text = (string)e.UserState;
            switch (n) {
                case 1: loggedinBox.Text = text; break;
                case 2: makermodelBox.Text = text; break;
                case 3: chassisBox.Text = text; break;
                case 4: freeBox.Text = text; break;
                case 5: ieLabel.Text = text; ieLabel.Visible = true; break;
                default: break;
            }
            Thread.Sleep(10);
        }

        private void wmiWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                //= "Canceled!";
            }
            else if (e.Error != null)
            {
                //= "Error: " + e.Error.Message;
            }
            else
            {
                //= "Done!";
            }

        }

        string insttarget = "";
        private void instAppsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string machine = machineBigBox.Text;
            insttarget = machine;
            Console.WriteLine("instAppsWorker_DoWork begin: " + machine);
            BackgroundWorker worker = sender as BackgroundWorker;

            worker.ReportProgress(0, "Installed Applications - Connecting to Registry Hives... ");
            //Console.WriteLine("Service list:\n" + Machine.ListServices(machine, ".*"));

            bool withWMI = true;
            if (withWMI)
            {
                worker.ReportProgress(0, "Installed Applications - Querying WMI...");
                int n = 0; MachineInfo.GetInstalledAppsWMI(machine,
                    row => { worker.ReportProgress(50, row); return n++; },
                    () => e.Cancel = worker.CancellationPending,
                    null, null, false, AuditSecGUIForm.DESIRED_APPS);
            }
            else
            {
                bool remregrun = MachineInfo.isServiceRunning(machine, "RemoteRegistry");
                if (!machineBigBox.Text.Equals(machine)) return;
                if (!remregrun)
                {
                    worker.ReportProgress(0, "Installed Applications - Starting Remote Registry...");
                    MachineInfo.StartService(machine, "RemoteRegistry", 30000);
                }
                if (MachineInfo.isServiceRunning(machine, "RemoteRegistry"))
                {
                    worker.ReportProgress(0, "Installed Applications - Querying Registry Hives...");
                    int n = 0; MachineInfo.GetInstalledAppsREG(machine,
                        row => { worker.ReportProgress(50, row); return n++; },
                        () => e.Cancel = worker.CancellationPending,
                        false, AuditSecGUIForm.DESIRED_APPS);
                }
                else
                {
                    worker.ReportProgress(100, "Installed Applications - Registry Hives not available.");
                }
                if (!remregrun)
                {
                    worker.ReportProgress(100, "Installed Applications - Stopping Remote Registry...");
                    MachineInfo.StopService(machine, "RemoteRegistry", 30000);
                }
            }
            worker.ReportProgress(100, "Installed Applications - " + (e.Cancel ? "Incomplete" : "Complete"));
            Console.WriteLine("instAppsWorker_DoWork finish: " + machine + " - " + (e.Cancel ? "Incomplete" : "Complete"));
        }

        public static bool isHotApp(string Name, String Vendor, string include_, string exclude_)
        {
            bool ret = include_.Length == 0 ? true : false;
            foreach (string x_ in include_.Split(new char[] { ';' })) if (x_.Trim().Length > 0)
                {
                    string x = x_.Contains("%") ? x_ : "%" + x_ + "%";
                    x = x.Replace("%", ".*");
                    if (Regex.IsMatch(Name, x, RegexOptions.IgnoreCase)) { ret = true; break; }
                }
            foreach (string x_ in exclude_.Split(new char[] { ';' })) if (x_.Trim().Length > 0)
                {
                    string x = x_.Contains("%") ? x_ : "%" + x_ + "%";
                    x = x.Replace("%", ".*");
                    if (Regex.IsMatch(Name, x, RegexOptions.IgnoreCase)) { ret = false; break; }
                }
            return ret;
        }

        private void instAppsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int n = e.ProgressPercentage;
            if (n == 100)
            {
                UseWaitCursor = false;
                instAppsTable.Sort(instAppsTable.Columns[0], ListSortDirection.Ascending);
            }
            if ( n == 0 || n == 100)
            {
                string text = (string)e.UserState;
                instAppsLabel.Text = text;
            }            
            else try
                {
                    object[] row = (object[])e.UserState;
                    string Name = row[0].ToString();
                    string Version = row[1].ToString();
                    string Vendor = row[2].ToString();

                    float majorminor = 0;
                    try
                    {
                        majorminor = float.Parse(Regex.Replace(Version, @"([^.]*\.[^.]*)\..*", "$1"), CultureInfo.InvariantCulture);
                    }
                    catch (Exception ee)
                    {

                    }

                    bool view = isHotApp(Name, Vendor,
                        hotappsBox.CheckState == CheckState.Checked ? includeBox.Text : "",
                        viewupdBox.CheckState == CheckState.Checked ? "" : excludeBox.Text);


                    if (view)
                    {
                        if (Name.ToLower().Contains("firefox") && majorminor > 0)
                        {
                            float cur = 0; try { cur = float.Parse(firefoxLabel.Text, CultureInfo.InvariantCulture); }
                            catch (Exception ee) { }
                            if (majorminor > cur) { firefoxLabel.Visible = true; firefoxLabel.Text = String.Format(CultureInfo.InvariantCulture, "{0:0.0}", majorminor);}
                        }
                        if (Name.ToLower().Contains("java") && majorminor > 0)
                        {
                            float cur = 0; try { cur = float.Parse(javaLabel.Text, CultureInfo.InvariantCulture); }
                            catch (Exception ee) { }
                            if (majorminor > cur) { javaLabel.Visible = true; javaLabel.Text = String.Format(CultureInfo.InvariantCulture, "{0:0.0}", majorminor); }
                        }
                        if ((Name.ToLower().Contains("adobe reader")
                            || Name.ToLower().Contains("adobe stan")
                            || Name.ToLower().Contains("adobe pro")) && majorminor > 0)
                        {
                            float cur = 0; try { cur = float.Parse(acrobatLabel.Text, CultureInfo.InvariantCulture); }
                            catch (Exception ee) { }
                            if (majorminor > cur) { acrobatLabel.Visible = true; acrobatLabel.Text = String.Format(CultureInfo.InvariantCulture, "{0:0.0}", majorminor); }
                        }
                        if (Name.ToLower().Contains("flash") && majorminor > 0)
                        {
                            float cur = 0; try { cur = float.Parse(flashLabel.Text, CultureInfo.InvariantCulture); }
                            catch (Exception ee) { }
                            if (majorminor > cur) { flashLabel.Visible = true; flashLabel.Text = String.Format(CultureInfo.InvariantCulture, "{0:0.0}", majorminor); }
                        }
                        if ((Name.ToLower().Contains("passwordbank")
                            || Name.ToLower().Contains("evidian")) && majorminor > 0)
                        {
                            float cur = 0; try { cur = float.Parse(ssoLabel.Text, CultureInfo.InvariantCulture); }
                            catch (Exception ee) { }
                            if (majorminor > cur) { ssoLabel.Visible = true; ssoLabel.Text = String.Format(CultureInfo.InvariantCulture, "{0:0.0}", majorminor); }
                        }
                        if ((Name.ToLower().Contains("vpn client")
                            || Name.ToLower().Contains("cisco anyconnect")) && majorminor > 0)
                        {
                            float cur = 0; try { cur = float.Parse(vpnLabel.Text, CultureInfo.InvariantCulture); }
                            catch (Exception ee) { }
                            if (majorminor > cur) { vpnLabel.Visible = true; vpnLabel.Text = String.Format(CultureInfo.InvariantCulture, "{0:0.0}", majorminor); }
                        }
                        instAppsTable.Rows.Add(row);
                        instAppsTable.Sort(instAppsTable.Columns[3], ListSortDirection.Descending);
                    }
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.Message);
                    Console.WriteLine(ee.StackTrace);
                }
            Thread.Sleep(1);
        }


        void copyAppsToClipboard()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append("Machine\t" + machineBigBox.Text + "\r\n");
            ret.Append("Date\t" + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy}", DateAndTime.Now) + "\r\n");
            ret.Append("Status\t" + instAppsLabel.Text + "\r\n");
            ret.Append("\r\n");
            foreach (DataGridViewColumn col in instAppsTable.Columns)
                ret.Append(col.HeaderText + "\t");
            ret.Append("\r\n");
            foreach (DataGridViewRow row in instAppsTable.Rows)
            {
                int i = 1;
                foreach (DataGridViewCell cell in row.Cells)
                    if (i++ < 5) ret.Append(cell.Value.ToString() + "\t");
                ret.Append("\r\n");
            }
            Clipboard.SetText(ret.ToString());
            MessageBox.Show("The list of installed applications on " + machineBigBox.Text + " was copied to the clipboard.",
                    "Installed Applications on " + machineBigBox.Text);
        }


        private void instAppsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
                copyAppsToClipboard();
        }


        private void instAppsTable_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {/*
            // Set the background to red for negative values in the Balance column.
            if (instAppsTable.Columns[e.ColumnIndex].Name.Equals("Balance"))
            {
                Int32 intValue;
                if (Int32.TryParse((String)e.Value, out intValue) &&
                    (intValue < 0))
                {
                    e.CellStyle.BackColor = Color.Red;
                    e.CellStyle.SelectionBackColor = Color.DarkRed;
                }
            }

            */
            
            if (instAppsTable.Columns[e.ColumnIndex].Name.Equals("Desired"))
            {
                int desired = e.Value == null ? 0 : (int)e.Value;
                e.Value = MachineInfo.getDesiredIcon(desired);
                //DataGridViewCell cell = instAppsTable[e.ColumnIndex, e.RowIndex];
                //cell.ToolTipText = stringValue;
            }
            

        }


        

        

        private void adminsLabel_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Getting " + machineBigBox.Text + "\\Administrators members...");
            Cursor.Current = Cursors.WaitCursor;
            string admins = MachineInfo.getGroupMembers(machineBigBox.Text, "Administrators", null, null, false, false,
                (adminsListBox != null ? adminsListBox.Text : AuditSec.defaultAdmins));
            Cursor.Current = Cursors.Default;
            MessageBox.Show(machineBigBox.Text + "\\Administrators members:\n(IT security groups not hidden)\n\n"
                + admins, machineBigBox.Text);
        }


  

        private void hotappsBox_CheckedChanged(object sender, EventArgs e)
        {
            bool check = hotappsBox.CheckState == CheckState.Checked;
            includeBox.Visible = check;
            viewupdBox.Visible = !check;
            if (check)
            {
                viewupdBox.CheckState = CheckState.Unchecked;
                this.toolTip1.SetToolTip(this.hotappsBox, "Hot Apps: " + include);
            }
            Thread.Sleep(500);
            query();
        }




        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (msg.Msg == 256)
            {
                if (keyData == Keys.Escape)
                {
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        ComputerReenabler compenab = null;
        private void reEnabButton_Click(object sender, EventArgs e)
        {
            if (compenab == null) compenab = new ComputerReenabler();
            compenab.ShowDialog();
        }


        private void batteryButton_Click(object sender, EventArgs e)
        {
            string batt = "" + MachineInfo.getBatteryDetails(machineBigBox.Text);
            MessageBox.Show(this, batt, "Battery Details " + machineBigBox.Text);
        }

        private void viewupdBox_CheckedChanged(object sender, EventArgs e)
        {
            bool check = viewupdBox.CheckState == CheckState.Checked;
            excludeBox.Visible = !check;
            query();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            query();
        }

        private void includeBox_TextChanged(object sender, EventArgs e)
        {
            refreshButton.Visible = true;
        }

        private void excludeBox_TextChanged(object sender, EventArgs e)
        {
            refreshButton.Visible = true;
        }


        private void batteryButton_Click_1(object sender, EventArgs e)
        {
            string batt = "" + MachineInfo.getBatteryDetails(machineBigBox.Text);
            MessageBox.Show(this, batt, "Battery Details " + machineBigBox.Text);
        }



    }
}
