using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.DirectoryServices;
using System.Net.NetworkInformation;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using smsclictr.automation;
using System.Management;
using System.Collections;
using Microsoft.VisualBasic;

namespace AuditSec
{
    public partial class SCCMCheck : Form
    {
//        AuditSecGUIForm parent;
        Form parent;
        string f16 = null, f17 = null, string21 = null;


        public SCCMCheck(AuditSecGUIForm parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        public SCCMCheck()
        {
            this.parent = null;
            InitializeComponent();
        }


        private void SCCMCheck_Shown(object sender, EventArgs e)
        {
            AuditSec.InitializeSMS(false, MYCOMPANY_Settings.SMS_DEFAULT_SERVER, MYCOMPANY_Settings.SMS_DEFAULT_SITE);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs args)
        {
            BackgroundWorker w = sender as BackgroundWorker;
            DirectoryEntry de = (DirectoryEntry)args.Argument;
            Cursor.Current = Cursors.WaitCursor;


            w.ReportProgress(0, "Pre-Installation Phase");
            w.ReportProgress(0, "================================================================================");

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox1.Text + "...");
            string machine = de.Properties["cn"].Value.ToString().ToUpper();
            string description = de.Properties["description"].Count > 0 ?
                de.Properties["description"][0].ToString() : "";
            string installed = ""; try
            {
                DateTime i = de.Properties["whenCreated"].Count > 0 ?
                    DateTime.ParseExact(de.Properties["whenCreated"][0].ToString(),
                    "dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US"), DateTimeStyles.None)
                    : new DateTime();
                installed = "Installed on " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy}", i);
            }
            catch (Exception ee) { Console.WriteLine(ee.ToString()); }
            w.ReportProgress(0, machine + " (" + description + ")");
            w.ReportProgress(0, installed);
            w.ReportProgress(1, CheckState.Checked);

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox2.Text + "...");
            w.ReportProgress(2, UsersInfo.isEnabledAD(de) ?
                CheckState.Checked : CheckState.Unchecked);

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox3.Text + "...");
            try
            {
                w.ReportProgress(0, "Path=" + de.Path.ToUpper().Replace("LDAP://CN=", "")
                    .Replace("OU=", "").Replace("DC=", "").Replace(",", "."));
                w.ReportProgress(3, System.Text.RegularExpressions.Regex.IsMatch(de.Path,
                    "LDAP://([A-Za-z.]+/)*CN=[A-Z]+[0-9]+,OU=Workstations,OU=[A-Z ]+,OU=[A-Z]+,DC=[A-Z]+,DC=MYCOMPANY,DC=INT",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?
                    CheckState.Checked : CheckState.Unchecked);
            }
            catch (Exception e)
            {
                w.ReportProgress(0, e.Message);
            }

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox4.Text + "...");
            Ping ping = new Ping(); PingReply reply;
            w.ReportProgress(0, "ping " + machine + "...");
            try
            {
                if ((reply = ping.Send(machine)).Status == IPStatus.Success)
                {
                    w.ReportProgress(0, "Reply from " + reply.Address);
                    w.ReportProgress(4, CheckState.Checked);
                }
                else
                {
                    w.ReportProgress(0, reply.Status);
                    w.ReportProgress(4, CheckState.Unchecked);
                }
            }
            catch (Exception e)
            {
                w.ReportProgress(0, e.Message);
                w.ReportProgress(4, CheckState.Unchecked);
            }

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox5.Text + "...");
            string fqdn = de.Properties["dNSHostName"].Value.ToString();
            w.ReportProgress(0, "ping " + fqdn + "...");
            try
            {
                if ((reply = ping.Send(fqdn)).Status == IPStatus.Success)
                {
                    w.ReportProgress(0, "Reply from " + reply.Address);
                    w.ReportProgress(5, CheckState.Checked);
                }
                else
                {
                    w.ReportProgress(0, reply.Status);
                    w.ReportProgress(5, CheckState.Unchecked);
                    w.ReportProgress(0, "\r\nFurther checks adjourned since computer not reachable.");
                    w.ReportProgress(0, "Computer description: " + description);
                    return;
                }
            }
            catch (Exception e)
            {
                w.ReportProgress(0, e.Message);
                w.ReportProgress(5, CheckState.Unchecked);
                w.ReportProgress(0, "\r\nFurther checks adjourned since computer not reachable.");
                w.ReportProgress(0, "Computer description: " + description);
                return;
            }

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox6.Text + "...");
            try
            {
                w.ReportProgress(6, Directory.Exists(@"\\" + machine + @"\admin$") ?
                    CheckState.Checked : CheckState.Unchecked);
            }
            catch (Exception e)
            {
                w.ReportProgress(0, e.Message);
                w.ReportProgress(6, CheckState.Unchecked);
            }

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox7.Text + "...");
            //string admins = MachineInfo.getGroupMembers(machine, "administrators", null, null, false, false, parent == null ? "" : parent.adminsBox.Text);
            string admins = MachineInfo.getGroupMembers(machine, "administrators", null, null, false, false, AuditSec.defaultAdmins);
            w.ReportProgress(0, "Administrators: " + (admins == null ? "none" : admins.TrimEnd('\n').Replace("\n", ", ")));
            bool s1 = false, s2 = false;
            if (admins != null) foreach (string u in admins.Split(new Char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string user = u.Trim().ToUpper();
                if (user.ToUpper().StartsWith("MYCOMPANY\\")) s1 = true;
                if (user.ToUpper().EndsWith("\\DOMAIN ADMINS")) s2 = true;
            }
            w.ReportProgress(7, s1 && s2 ?
                    CheckState.Checked : CheckState.Unchecked);

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox8.Text + "...");
            string freedisk = MachineInfo.getGB(machine);
            w.ReportProgress(8, freedisk != null ?
                    CheckState.Checked : CheckState.Unchecked);
            string users = MachineInfo.getCurrentUsers(machine);
            w.ReportProgress(0, "Currently logged users: " + (users == null ? "none." : users.TrimEnd('\n').Replace("\n", ", ")));

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox9.Text + "...");
            if (freedisk != null) try
                {
                    w.ReportProgress(0, freedisk.Replace("\n", ""));
                    string f = freedisk.Replace("C: has ", "");
                    f = f.Substring(0, f.IndexOf("GB"));
                    w.ReportProgress(9, float.Parse(freedisk != null ? f.Trim() : "0") >= 5 ?
                        CheckState.Checked : CheckState.Unchecked);
                }
                catch (Exception e)
                {
                    w.ReportProgress(0, freedisk.Replace("\n", "") + "---" + e.Message);
                }
            else
            {
                w.ReportProgress(0, "The free disk space information is not available.");
                w.ReportProgress(9, CheckState.Unchecked);
            }

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox10.Text + "...");
            try
            {
                bool remregrun = MachineInfo.isServiceRunning(machine, "RemoteRegistry");
                if (!remregrun)
                {
                    MachineInfo.StartService(machine, "RemoteRegistry", 30000);
                }
                if (MachineInfo.isServiceRunning(machine, "RemoteRegistry"))
                {
                    RegistryKey HKLM = null;
                    bool rrok = (HKLM = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machine)) != null;
                    w.ReportProgress(10, rrok ?
                        CheckState.Checked : CheckState.Unchecked);
                }
                else
                {
                    w.ReportProgress(10, CheckState.Unchecked);
                }
                if (!remregrun)
                {
                    MachineInfo.StopService(machine, "RemoteRegistry", 30000);
                }
            }
            catch (Exception e)
            {
                w.ReportProgress(0, e.Message);
                w.ReportProgress(10, CheckState.Unchecked);
            }

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox11.Text + "...");
            try
            {
                w.ReportProgress(11, Directory.Exists(@"\\" + machine + @"\admin$\system32\ccmsetup") ?
                    CheckState.Checked : CheckState.Unchecked);
            }
            catch (Exception e)
            {
                w.ReportProgress(0, e.Message);
            }


            w.ReportProgress(0, "\r\n\r\nInstallation Phase");
            w.ReportProgress(0, "================================================================================");

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox12.Text + "...");
            w.ReportProgress(12, MachineInfo.isServicePresent(machine, "ccmsetup")
                && (MachineInfo.isServiceRunning(machine, "ccmsetup")
                || MachineInfo.StartService(machine, "ccmsetup", 30000)) ?
                CheckState.Checked : CheckState.Unchecked);

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox13.Text + "...");
            w.ReportProgress(13, MachineInfo.isServicePresent(machine, "msiserver")
                && (MachineInfo.isServiceRunning(machine, "msiserver")
                || MachineInfo.StartService(machine, "msiserver", 30000)) ?
                CheckState.Checked : CheckState.Unchecked);

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox14.Text + "...");
            w.ReportProgress(14, MachineInfo.isServicePresent(machine, "LanmanServer")
                && (MachineInfo.isServiceRunning(machine, "LanmanServer")
                || MachineInfo.StartService(machine, "LanmanServer", 30000)) ?
                CheckState.Checked : CheckState.Unchecked);

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox15.Text + "...");
            w.ReportProgress(15, MachineInfo.isServicePresent(machine, "BITS")
                && (MachineInfo.isServiceRunning(machine, "BITS")
                || MachineInfo.StartService(machine, "BITS", 30000)) ?
                CheckState.Checked : CheckState.Unchecked);

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox16.Text + "...");
            try
            {
                f16 = @"\\" + machine + @"\admin$\system32\ccmsetup\ccmsetup.log";
                f17 = @"\\" + machine + @"\admin$\system32\ccmsetup\client.msi.log";
                bool f16e = File.Exists(f16);
                bool f17e = File.Exists(f17);
                DateTime f16d = f16e ? File.GetLastWriteTime(f16) : new DateTime();
                DateTime f17d = f17e ? File.GetLastWriteTime(f17) : new DateTime();
                w.ReportProgress(16, new Object[]{
                    f16e && f17e ? CheckState.Checked : CheckState.Unchecked,
                    f16d, f17d });
                if (!f16e) w.ReportProgress(0, "ccmsetup.log not found!");
                if (!f17e) w.ReportProgress(0, "client.msi.log not found!");
            }
            catch (Exception e)
            {
                w.ReportProgress(0, e.Message);
            }

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox17.Text + "...");
            try
            {
                w.ReportProgress(17, Directory.Exists(@"\\" + machine + @"\admin$\system32\CCM") ?
                    CheckState.Checked : CheckState.Unchecked);
            }
            catch (Exception e)
            {
                w.ReportProgress(0, e.Message);
            }



            w.ReportProgress(0, "\r\n\r\nPost-Installation Phase");
            w.ReportProgress(0, "================================================================================");

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox18.Text + "...");
            w.ReportProgress(18, MachineInfo.isServicePresent(machine, "CcmExec")
                && (MachineInfo.isServiceRunning(machine, "CcmExec")
                || MachineInfo.StartService(machine, "CcmExec", 30000)) ?
                CheckState.Checked : CheckState.Unchecked);

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox19.Text + "...");
            MachineInfo.refreshManagementPoint(machine,
                report => { w.ReportProgress(0, report); return true; },
                (check, MP) => { w.ReportProgress(19, new Object[]{check ? CheckState.Checked : CheckState.Unchecked, MP}); return true; });

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox20.Text + "...");
            MachineInfo.enableSMSAutoAssignment(machine,
                report => { w.ReportProgress(0, report); return true; },
                (check, site) => { w.ReportProgress(20, new Object[]{check ? CheckState.Checked : CheckState.Unchecked, site}); return true; });

            if (w.CancellationPending) return;
            w.ReportProgress(0, "\r\nChecking: " + checkBox21.Text + "...");
            string21 = MachineInfo.getSMSComponents(machine,
                report => { w.ReportProgress(0, report); return true; },
                check => { w.ReportProgress(21, check ? CheckState.Checked : CheckState.Unchecked); return true; });



            w.ReportProgress(22, true);
            w.ReportProgress(23, true);
            w.ReportProgress(24, true);
            w.ReportProgress(25, true);
            w.ReportProgress(26, true);
            w.ReportProgress(27, true);
            w.ReportProgress(28, true);
            w.ReportProgress(29, true);

            w.ReportProgress(0, "");
            w.ReportProgress(0, "Final Client Health Check not implemented. Do it locally/remotely please.");
            w.ReportProgress(0, "================================================================================");
            w.ReportProgress(0, "All checks performed.");
            w.ReportProgress(0, "Computer description: " + description);
            w.ReportProgress(0, "Currently logged users: " + (users == null ? "none." : users.TrimEnd('\n').Replace("\n", ", ")));

            Cursor.Current = Cursors.Default;

            MessageBox.Show("Do it locally/remotely please.\n"
                + "\nComputer description: " + description
                + "\nCurrently logged users: " + (users == null ? "none." : users.TrimEnd('\n').Replace("\n", ", ")),
                "Final Client Health Check");
            return;
        }
                
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker w = sender as BackgroundWorker;
            if (e.Cancelled) {
                w.ReportProgress(0, "Further tests cancelled.");
                w.ReportProgress(0, "================================================================================");
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorker w = sender as BackgroundWorker;
            if (w.CancellationPending) return;

            int p = e.ProgressPercentage; if (p == 0) {
                reportBox.Text += e.UserState.ToString() + "\r\n";
                reportBox.SelectionStart = reportBox.Text.Length;
                reportBox.ScrollToCaret();
                reportBox.Refresh();
            } else {
                switch(p) {
                    case 1:
                        checkBox1.CheckState = (CheckState)e.UserState;
                        checkBox1.Enabled = true;
                        reportBox.Text += checkBox1.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 2:
                        checkBox2.CheckState = (CheckState)e.UserState;
                        checkBox2.Enabled = true;
                        reportBox.Text += checkBox2.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 3:
                        checkBox3.CheckState = (CheckState)e.UserState;
                        checkBox3.Enabled = true;
                        reportBox.Text += checkBox3.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 4:
                        checkBox4.CheckState = (CheckState)e.UserState;
                        checkBox4.Enabled = true;
                        reportBox.Text += checkBox4.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 5:
                        checkBox5.CheckState = (CheckState)e.UserState;
                        checkBox5.Enabled = true;
                        reportBox.Text += checkBox5.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 6:
                        checkBox6.CheckState = (CheckState)e.UserState;
                        checkBox6.Enabled = true;
                        reportBox.Text += checkBox6.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 7:
                        checkBox7.CheckState = (CheckState)e.UserState;
                        checkBox7.Enabled = true;
                        reportBox.Text += checkBox7.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 8:
                        checkBox8.CheckState = (CheckState)e.UserState;
                        checkBox8.Enabled = true;
                        reportBox.Text += checkBox8.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 9:
                        checkBox9.CheckState = (CheckState)e.UserState;
                        checkBox9.Enabled = true;
                        reportBox.Text += checkBox9.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 10:
                        checkBox10.CheckState = (CheckState)e.UserState;
                        checkBox10.Enabled = true;
                        reportBox.Text += checkBox10.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 11:
                        checkBox11.CheckState = (CheckState)e.UserState;
                        checkBox11.Enabled = true;
                        reportBox.Text += checkBox11.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 12:
                        checkBox12.CheckState = (CheckState)e.UserState;
                        checkBox12.Enabled = true;
                        reportBox.Text += checkBox12.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 13:
                        checkBox13.CheckState = (CheckState)e.UserState;
                        checkBox13.Enabled = true;
                        reportBox.Text += checkBox13.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 14:
                        checkBox14.CheckState = (CheckState)e.UserState;
                        checkBox14.Enabled = true;
                        reportBox.Text += checkBox14.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 15:
                        checkBox15.CheckState = (CheckState)e.UserState;
                        checkBox15.Enabled = true;
                        reportBox.Text += checkBox15.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 16:
                        checkBox16.CheckState = (CheckState)((Object[])e.UserState)[0];
                        button16.Text = String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy HH:mm}", (DateTime)((Object[])e.UserState)[1]);
                        button17.Text = String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy HH:mm}", (DateTime)((Object[])e.UserState)[2]);
                        checkBox16.Enabled = true;
                        reportBox.Text += checkBox16.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 17:
                        checkBox17.CheckState = (CheckState)e.UserState;
                        checkBox17.Enabled = true;
                        reportBox.Text += checkBox17.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 18:
                        checkBox18.CheckState = (CheckState)e.UserState;
                        checkBox18.Enabled = true;
                        reportBox.Text += checkBox18.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 19:
                        checkBox19.CheckState = (CheckState)((Object[])e.UserState)[0];
                        label19.Text = (string)((Object[])e.UserState)[1];
                        checkBox19.Enabled = true;
                        reportBox.Text += checkBox19.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 20:
                        checkBox20.CheckState = (CheckState)((Object[])e.UserState)[0];
                        label20.Text = (string)((Object[])e.UserState)[1];
                        checkBox20.Enabled = true;
                        reportBox.Text += checkBox20.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 21:
                        checkBox21.CheckState = (CheckState)e.UserState;
                        checkBox21.Enabled = true;
                        reportBox.Text += checkBox21.CheckState == CheckState.Checked ? "This test is passed.\r\n" : "This test is failed.\r\n";
                        break;
                    case 22:
                        checkBox22.Enabled = (bool)e.UserState;
                        break;
                    case 23:
                        checkBox23.Enabled = (bool)e.UserState;
                        break;
                    case 24:
                        checkBox24.Enabled = (bool)e.UserState;
                        break;
                    case 25:
                        checkBox25.Enabled = (bool)e.UserState;
                        break;
                    case 26:
                        checkBox26.Enabled = (bool)e.UserState;
                        break;
                    case 27:
                        checkBox27.Enabled = (bool)e.UserState;
                        break;
                    case 28:
                        checkBox28.Enabled = (bool)e.UserState;
                        break;
                    case 29:
                        checkBox29.Enabled = (bool)e.UserState;
                        break;
                }
                Thread.Sleep(500);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (change) {change = false; return;}
            if (worker.IsBusy
                || computerBox.ForeColor == Color.Green
                || computerBox.Text.Equals("[search]")) return;
            clearAll();
            string machine = computerBox.Text;
            //SearchResult r = MachineInfo.getSearcher("cn=" + machine).FindOne();
            MachineInfo mi = MachineInfo.getMachine(machine);
            if (mi != null)
            {
                computerBox.ForeColor = Color.Green;
                if (worker.IsBusy) Thread.Sleep(10000);
                if (worker.IsBusy) Thread.Sleep(10000);
                if (worker.IsBusy) Thread.Sleep(10000);
                if (worker.IsBusy) Thread.Sleep(10000);
                if (worker.IsBusy) Thread.Sleep(10000);
                if (worker.IsBusy) Thread.Sleep(10000);
                if (!worker.IsBusy)
                {
                    if (!smsWorker.IsBusy) smsWorker.RunWorkerAsync(machine);
                    else Console.WriteLine("smsWorker busy!");

                    //worker.RunWorkerAsync(r.GetDirectoryEntry());
                    worker.RunWorkerAsync(mi.machinede);
                }
                else
                {
                    reportBox.Text += machine + " check adjourned since program busy. Try again.\r\n";
                }

            }
            else
            {
                computerBox.ForeColor = Color.Red;
                reportBox.Text += "Computer not found.";
            }
        }

        Boolean change = false;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            change = true;
            if (worker.IsBusy) worker.CancelAsync();
            computerBox.ForeColor = Color.Black;
            int i = computerBox.SelectionStart;
            computerBox.Text = computerBox.Text.ToUpper();
            computerBox.SelectionStart = i;
            clearAll();
            if (computerBox.Text.Length == 0)
            {
                computerBox.Text = "[Search]";
                computerBox.SelectAll();
            }
        }


        private void button16_Click(object sender, EventArgs e)
        {
            if (f16 == null) return;
            string tmp;
            File.Copy(f16, tmp = MachineInfo.GetTempFilePathWithExtension(".tmp"));
            Process pad = new Process();
            pad.StartInfo.FileName = "notepad.exe";
            pad.StartInfo.Arguments = tmp;
            pad.Start();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (f17 == null) return;
            string tmp;
            File.Copy(f17, tmp = MachineInfo.GetTempFilePathWithExtension(".tmp"));
            Process pad = new Process();
            pad.StartInfo.FileName = "notepad.exe";
            pad.StartInfo.Arguments = tmp;
            pad.Start();
        }

        private void SCCMCheck_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (parent != null) parent.Show();
            else AuditSec.Exit("SCCM Client Check tool has ended.", null);
        }

        void clearAll()
        {
            reportBox.Text = "";
            checkBox1.Enabled = false; checkBox1.CheckState = CheckState.Indeterminate;
            checkBox2.Enabled = false; checkBox2.CheckState = CheckState.Indeterminate;
            checkBox3.Enabled = false; checkBox3.CheckState = CheckState.Indeterminate;
            checkBox4.Enabled = false; checkBox4.CheckState = CheckState.Indeterminate;
            checkBox5.Enabled = false; checkBox5.CheckState = CheckState.Indeterminate;
            checkBox6.Enabled = false; checkBox6.CheckState = CheckState.Indeterminate;
            checkBox7.Enabled = false; checkBox7.CheckState = CheckState.Indeterminate;
            checkBox8.Enabled = false; checkBox8.CheckState = CheckState.Indeterminate;
            checkBox9.Enabled = false; checkBox9.CheckState = CheckState.Indeterminate;
            checkBox10.Enabled = false; checkBox10.CheckState = CheckState.Indeterminate;
            checkBox11.Enabled = false; checkBox11.CheckState = CheckState.Indeterminate;
            checkBox12.Enabled = false; checkBox12.CheckState = CheckState.Indeterminate;
            checkBox13.Enabled = false; checkBox13.CheckState = CheckState.Indeterminate;
            checkBox14.Enabled = false; checkBox14.CheckState = CheckState.Indeterminate;
            checkBox15.Enabled = false; checkBox15.CheckState = CheckState.Indeterminate;
            checkBox16.Enabled = false; checkBox16.CheckState = CheckState.Indeterminate;
            f16 = null; button16.Text = "...";
            f17 = null; button17.Text = "...";
            checkBox17.Enabled = false; checkBox17.CheckState = CheckState.Indeterminate;
            checkBox18.Enabled = false; checkBox18.CheckState = CheckState.Indeterminate;
            checkBox19.Enabled = false; checkBox19.CheckState = CheckState.Indeterminate;
            checkBox20.Enabled = false; checkBox20.CheckState = CheckState.Indeterminate;
            label19.Text = "..."; label20.Text = "...";
            checkBox21.Enabled = false; checkBox21.CheckState = CheckState.Indeterminate;
            string21 = "N/A";
            checkBox22.Enabled = false; checkBox22.CheckState = CheckState.Indeterminate;
            checkBox23.Enabled = false; checkBox23.CheckState = CheckState.Indeterminate;
            checkBox24.Enabled = false; checkBox24.CheckState = CheckState.Indeterminate;
            checkBox25.Enabled = false; checkBox25.CheckState = CheckState.Indeterminate;
            checkBox26.Enabled = false; checkBox26.CheckState = CheckState.Indeterminate;
            checkBox27.Enabled = false; checkBox27.CheckState = CheckState.Indeterminate;
            checkBox28.Enabled = false; checkBox28.CheckState = CheckState.Indeterminate;
            checkBox29.Enabled = false; checkBox29.CheckState = CheckState.Indeterminate;
            heartbeatLabel.Text = "...";
        }

        private void button21_Click(object sender, EventArgs e)
        {
            MessageBox.Show(string21, "SMS Components List");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(reportBox.Text);
        }


        
        private void smsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string machine = (string)e.Argument;
            BackgroundWorker w = sender as BackgroundWorker;
            w.ReportProgress(1, "Querying SCCM...");
            //w.ReportProgress(0, "Collecting heartbeat records for " + machine + " from " + smssrv + "/" + smssite + "...");
            bool error = false;
            DateTime heartbeat = MachineInfo.getSMSClientHeartbeat(AuditSec.settings.smssrv, AuditSec.settings.smssite, machine,
                ee => { error = true; w.ReportProgress(0, ee); MessageBox.Show(ee, "Collecting heartbeat records"); return true; }
                );
            string x = error ? "Heartbeat records N/A"
                : heartbeat == DateTime.MaxValue ? "SCCM reports NO Client is installed"
                : heartbeat == DateTime.MinValue ? "SCCM reports a Client is installed but NO heartbeat"
                : "Heartbeat on " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy HH:mm}", heartbeat);
            w.ReportProgress(1, x);
            //w.ReportProgress(0, x);
        }


        private void smsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int p = e.ProgressPercentage; if (p == 0)
            {
                reportBox.Text += e.UserState.ToString() + "\r\n";
                reportBox.SelectionStart = reportBox.Text.Length;
                reportBox.ScrollToCaret();
                reportBox.Refresh();
            }
            else if (p == 1)
            {
                heartbeatLabel.Text = e.UserState.ToString();
            }
        }

        public void setComputer(string comp)
        {
            computerBox.Text = comp;
        }




    }
}
