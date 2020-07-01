using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

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

using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Reflection;
using System.Net.NetworkInformation;
using System.Windows.Automation;


namespace AuditSec
{
    public partial class AuditSecGUIForm : Form
    {
        DirectorySearcher COMP_SEARCH = MachineInfo.getSearcher();

        int WK_COUNT = 0;
        int WK_DONE;
        int WK_MAX = 400;

        StringBuilder WK_RESULTS = new StringBuilder();

        UsersInfo usersInfo = new UsersInfo();

        static public Hashtable PW_DICO = new Hashtable();
        static public List<Object[]> DESIRED_APPS = new List<Object[]>();



        void saveSettings(bool disposing)
        {
            AuditSec.saveSettings(stdpwBox.Text, pwsBox.Text, desapBox.Text, OUMaskBox.Text, WkMaskBox.Text, adminsBox.Text,
            doWIN7(), doWIN7SP(), doADMPW(), doNONAUTH() ,doMISPLACED(), doBITLOCKER(), doSCCM(), doVIRUS());

            Console.WriteLine("Stop scanning...");
            stopButton.PerformClick();

            if (disposing && (components != null)) components.Dispose();
            //base.Dispose(disposing);

            AuditSec.Exit("Audit Security module has ended.",
                () => { base.Dispose(disposing); return true; });
        }


        public AuditSecGUIForm()
        {
            InitializeComponent();
            showOptions(false);
            this.Text += " v" + AuditSec.curver;

            Console.WriteLine("Opening domain... ");
            try
            {
                DomainCollection dc = Forest.GetCurrentForest().Domains;
                Domain[] domains = new Domain[dc.Count]; dc.CopyTo(domains, 0);
                domainBox.Items.AddRange(domains);

                domainBox.Text = Domain.GetCurrentDomain().Name;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            string me = new DirectorySearcher("(&(ObjectClass=user)(sAMAccountName="
                + UserPrincipal.Current.SamAccountName + "))")
                .FindOne().GetDirectoryEntry().Properties["DistinguishedName"].Value.ToString();
            foreach (string s in me.Split(',').Reverse())
            {
                string[] t = s.Split(new char[]{'='}, 2);
                if (t[0].Equals("OU")) {OUBox.Text = t[1]; break;}
            }

            Console.WriteLine("Setting masks... ");
            if (AuditSec.settings.oumask != null && AuditSec.settings.oumask.Length > 0) OUMaskBox.Text = AuditSec.settings.oumask;
            if (AuditSec.settings.wkmask != null && AuditSec.settings.wkmask.Length > 0) WkMaskBox.Text = AuditSec.settings.wkmask;
            sm0 = OUMaskBox.Text;
            wm0 = WkMaskBox.Text;
            if (AuditSec.settings.admins != null && AuditSec.settings.admins.Length > 0) adminsBox.Text = AuditSec.settings.admins;

            Console.WriteLine("Setting tasks... ");
            taskList.SetItemChecked(0, true);
            taskList.SetItemChecked(1, true);
            taskList.SetItemChecked(2, true);
            taskList.SetItemChecked(3, true);
            taskList.SetItemChecked(4, true);
            taskList.SetItemChecked(5, true);
            taskList.SetItemChecked(6, false);
            taskList.SetItemChecked(7, true);
            doWIN7(AuditSec.settings.dowin7);
            doWIN7SP(AuditSec.settings.dowin7sp);
            doADMPW(AuditSec.settings.doadmpw);
            doNONAUTH(AuditSec.settings.dononauth);
            doMISPLACED(AuditSec.settings.domisplaced);
            doBITLOCKER(AuditSec.settings.dobitlocker);
            doSCCM(AuditSec.settings.dosccm);
            doVIRUS(AuditSec.settings.dovirus);

            //stdpwBox.PasswordChar = '●';


            new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(1000);
                Invoke(new stdpwBoxDelegate(setStdpwBox));

                Console.WriteLine("Loading configuration files... ");
                Invoke(new pwsButtonDelegate(pwsButtonPerformClick));
                Invoke(new desapButtonDelegate(desapButtonPerformClick));

                Invoke(new picpwDelegate(setpicpw));

                Invoke(new welcomeButtonDelegate(welcomeButtonPerformClick));

            })).Start();
        }


        public delegate void welcomeButtonDelegate();
        private void welcomeButtonPerformClick()
        {
            actionsPanel.Controls.Remove(welcomeButton);
            Console.WriteLine("Ready.\n\n");
            spyTimer.Start();
            
            lockedAccountTimer_Tick(this, null);
            lockedAccountTimer.Start();

        }

        public delegate void pwsButtonDelegate();
        private void pwsButtonPerformClick()
        {
            Console.WriteLine("Opening workstation passwords list... ");
            if (AuditSec.settings.pws != null && AuditSec.settings.pws.Length > 0)
                pwsBox.Text = openPwlistFile(AuditSec.settings.pws, false) ? AuditSec.settings.pws : "";
            if (PW_DICO.Count == 0)
                pwsButton.PerformClick();
        }

        public delegate void desapButtonDelegate();
        private void desapButtonPerformClick()
        {
            Console.WriteLine("Opening desired applications list... ");
            if (AuditSec.settings.desap != null && AuditSec.settings.desap.Length > 0)
                desapBox.Text = openDesapFile(AuditSec.settings.desap, false) ? AuditSec.settings.desap : "";
            //if (DESIRED_APPS.Count == 0)
            //    desapButton.PerformClick();
        }

        public void refreshDesap()
        {
            if (desapBox.Text.Length > 0)
                openDesapFile(desapBox.Text, false);
        }

        public delegate void stdpwBoxDelegate();
        private void setStdpwBox()
        {
            Console.WriteLine("Setting standard password... ");
            stdpwBox.Text = AuditSec.settings.stdpw;
            if (stdpwBox.Text.Length == 0)
                stdpwBox.Text = InputStdpw();
        }


        string InputStdpw()
        {
            StringBuilder sb = new StringBuilder();
            string s = null; do
            {
                s = Interaction.InputBox(sb.ToString() + "\n\nAnother Default Password: ", "Local Administrator Default Passwords",
                    s == null ? "" : s);
                if (s != null && s.Length == 0) s = null;
                if (s != null) sb.AppendLine(s);
            } while (s != null);
            return sb.Length == 0 ? "" : sb.ToString();
        }

        public delegate void picpwDelegate();
        private void setpicpw()
        {
            Console.WriteLine("Opening DirXML... ");
            if (AuditSec.settings.picpw != null && AuditSec.settings.picpw.Length == 0) AuditSec.settings.picpw = null;
            UsersInfo.getDIRXMLAttributes(UserPrincipal.Current.SamAccountName);
        }


        private void domainBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            OUBox.Items.Clear();
            OUBox.Text = "";
            OUBox.Items.Add("");
            dptBox.Items.Clear();
            dptBox.Text = "";
            dptBox.Items.Add("");
            if (domainBox.SelectedItem == null) return;
            setEnableGUI(false);
            statusLabel.Text = "Listing the OUs...";
            Cursor.Current = Cursors.WaitCursor;
            string domain = domainBox.SelectedItem.ToString();
            DirectoryContext context = new DirectoryContext(DirectoryContextType.Domain, domain);
            Domain d = Domain.GetDomain(context);
            DirectoryEntry de = d.GetDirectoryEntry();

            DirectorySearcher ds = new DirectorySearcher(de, "(objectClass=organizationalUnit)", null, SearchScope.OneLevel);
            ds.PropertiesToLoad.Add("name");

            foreach (SearchResult r in ds.FindAll())
            {
                string ou = r.Properties["name"][0].ToString();
                bool match = false; try
                {
                    match = System.Text.RegularExpressions.Regex.IsMatch(ou, OUMaskBox.Text,
                             System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                catch (Exception ee)
                {
                    Console.WriteLine("OU=" + ou + " IsMatch Regex=" + OUMaskBox.Text + " = " + ee.Message);
                }
                if (match) OUBox.Items.Add(ou);
                //Console.WriteLine("OU=" + ou + " IsMatch Regex=" + OUMaskBox.Text + " = " + match);
            }
            Cursor.Current = Cursors.Default;
            statusLabel.Text = "Select a Site and eventually a Dpt. and then push Check Computers.";
            setEnableGUI(true);
        }


        private void OUBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            dptBox.Items.Clear();
            dptBox.Text = "";
            dptBox.Items.Add("");
            usersInfo.Clear();
            if (domainBox.Text.Length == 0) return;

            setEnableGUI(false);
            statusLabel.Text = "Listing the Dpts and Workstations...";
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                DirectoryEntry de = ((Domain)domainBox.SelectedItem).GetDirectoryEntry(), ou = null;
                foreach (DirectoryEntry de_ in de.Children)
                    if (de_.Properties["name"].Value.ToString().Equals(OUBox.Text)) { ou = de = de_; break; }

                COMP_SEARCH.SearchRoot = de;
                WK_COUNT = 0;
                foreach (SearchResult r in COMP_SEARCH.FindAll())
                {
                    string wk = r.Properties["name"][0].ToString();
                    bool match = false; try
                    {
                        match = System.Text.RegularExpressions.Regex.IsMatch(wk, WkMaskBox.Text,
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine("Wk=" + wk + " IsMatch Regex=" + WkMaskBox.Text + " = " + match);
                    }
                    if (match)
                    {
                        WK_COUNT++;
                        //Console.WriteLine("Wk=" + wk + " IsMatch Regex=" + WkMaskBox.Text + " = " + match);
                    }
                }
                statusLabel.Text = "Found " + WK_COUNT + " workstation(s) in " + de.Properties["DistinguishedName"].Value.ToString();
                progressBar.Maximum = WK_COUNT;
                progressBar.Value = 0;

                if (ou != null) {//dpt populating
                    DirectorySearcher ds = new DirectorySearcher(ou, "(objectClass=organizationalUnit)",
                        new String[]{"name"}, SearchScope.OneLevel);
                    foreach (SearchResult r2 in ds.FindAll())
                    {
                        string subou = r2.Properties["name"][0].ToString();
                        DirectoryEntry dpt = r2.GetDirectoryEntry();
                        bool found_in_dpt = false;
                        bool found_in_Workstations = false;
                        try
                        {
                            //Console.WriteLine("dpt: " + subou);
                            COMP_SEARCH.SearchRoot = dpt;
                            found_in_dpt = COMP_SEARCH.FindOne() != null;
                        }
                        catch (Exception ee)
                        {
                            //has no Workstations
                        }
                        try {
                            DirectoryEntry Workstations = dpt.Children.Find("OU=Workstations", "organizationalUnit");
                            COMP_SEARCH.SearchRoot = Workstations;
                            found_in_Workstations = COMP_SEARCH.FindOne() != null;
                        }
                        catch (Exception ee)
                        {
                            //has no Workstations
                        }
                        //Console.WriteLine("dpt: " + subou + " Base?Wk:" + found_in_dpt + " Workstations?Wk:" + found_in_Workstations);
                        if (found_in_dpt || found_in_Workstations) dptBox.Items.Add(subou);
                    }
                }

                COMP_SEARCH.SearchRoot = de;

                usersInfo.FindAll(de);
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            Cursor.Current = Cursors.Default;
            setEnableGUI(true);
        }


        private void dptBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (domainBox.Text.Length == 0 || OUBox.Text.Length == 0) return;
            setEnableGUI(false);
            statusLabel.Text = "Listing the Workstations...";
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                DirectoryEntry de = ((Domain)domainBox.SelectedItem).GetDirectoryEntry(), ou = null, subou = null;
                foreach (DirectoryEntry de_ in de.Children)
                    if (de_.Properties["name"].Value.ToString().Equals(OUBox.Text)) { ou = de_; break; }
                if (ou != null) foreach (DirectoryEntry de_ in ou.Children)
                    if (de_.Properties["name"].Value.ToString().Equals(dptBox.Text)) { subou = de_; break; }
                if (subou != null)
                {
                    COMP_SEARCH.SearchRoot = subou;
                    WK_COUNT = 0;
                    foreach (SearchResult r in COMP_SEARCH.FindAll())
                    {
                        string wk = r.Properties["name"][0].ToString();
                        if (System.Text.RegularExpressions.Regex.IsMatch(wk, WkMaskBox.Text,
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            //Console.WriteLine(wk);
                            WK_COUNT++;
                        }
                    }
                    statusLabel.Text = "Found " + WK_COUNT + " workstation(s) in " + subou.Properties["DistinguishedName"].Value.ToString();
                    progressBar.Maximum = WK_COUNT;
                    progressBar.Value = 0;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            Cursor.Current = Cursors.Default;
            setEnableGUI(true);
        }



        string sm0 = ""; int smdt = 0;
        private void sitemaskTimer_Tick(object sender, EventArgs e)
        {
            smdt++;
            if (smdt > 5)
            {
                smdt = 0;
                string sm1 = OUMaskBox.Text;
                if (!sm1.Equals(sm0))
                {
                    Console.WriteLine("Site Mask change detected!");
                    domainBox_SelectedIndexChanged(sender, e);
                }
                sm0 = sm1;
            }
        }

        string wm0 = ""; int wmdt = 0;
        private void compmaskTimer_Tick(object sender, EventArgs e)
        {
            wmdt++;
            if (wmdt > 5)
            {
                wmdt = 0;
                string wm1 = WkMaskBox.Text;
                if (!wm1.Equals(wm0))
                {
                    Console.WriteLine("Computer Mask change detected!");
                    OUBox_SelectedIndexChanged(sender, e);
                }
                wm0 = wm1;
            }
        }



        private void OUMaskBox_TextChanged(object sender, EventArgs e)
        {
            smdt = 0;
        }

        private void WkMaskBox_TextChanged(object sender, EventArgs e)
        {
            wmdt = 0;
        }


        private bool doWIN7()
        {
            return taskList.GetItemChecked(0);
        }

        private void doWIN7(bool enable)
        {
            taskList.SetItemChecked(0, enable);
        }

        private bool doWIN7SP()
        {
            return taskList.GetItemChecked(1);
        }

        private void doWIN7SP(bool enable)
        {
            taskList.SetItemChecked(1, enable);
        }

        private bool doADMPW()
        {
            return taskList.GetItemChecked(2);
        }

        private void doADMPW(bool enable)
        {
            taskList.SetItemChecked(2, enable);
        }

        private bool doNONAUTH()
        {
            return taskList.GetItemChecked(3);
        }

        private void doNONAUTH(bool enable)
        {
            taskList.SetItemChecked(3, enable);
        }

        private bool doMISPLACED()
        {
            return taskList.GetItemChecked(4);
        }

        private void doMISPLACED(bool enable)
        {
            taskList.SetItemChecked(4, enable);
        }

        private bool doBITLOCKER()
        {
            return taskList.GetItemChecked(5);
        }

        private void doBITLOCKER(bool enable)
        {
            taskList.SetItemChecked(5, enable);
        }

        
        private bool doSCCM()
        {
            return taskList.GetItemChecked(6);
        }

        private void doSCCM(bool enable)
        {
            taskList.SetItemChecked(6, enable);
        }

        private bool doVIRUS()
        {
            return taskList.GetItemChecked(7);
        }

        private void doVIRUS(bool enable)
        {
            taskList.SetItemChecked(7, enable);
        }


        public delegate void enableGUIDelegate(Boolean value);
        private void setEnableGUI(Boolean value)
        {
            STOP_REQUEST = value;
            domainBox.Enabled = value;
            OUBox.Enabled = value;
            dptBox.Enabled = value;
            stdpwBox.Enabled = value;
            OUMaskBox.Enabled = value;
            pwsButton.Enabled = value;
            WkMaskBox.Enabled = value;
            taskList.Enabled = value;
            startButton.Enabled = value;
            saveButton.Enabled = value;
            adminsBox.Enabled = value;
        }

        public delegate void issuesCountBoxDelegate();
        private void setIssuesCountBox()
        {
            issuesCountBox.Text = "" + actionsPanel.Controls.Count;
            if (actionsPanel.Controls.Count == 0) setStatusLabel("No Security Issues anymore. You may verify another Site/Department.");
        }

        public delegate void progressBarDelegate();
        private void setProgressBar()
        {
            if (!STOP_REQUEST) progressBar.Value++;
            //Console.WriteLine("Progress: " + progressBar.Value + "/" + progressBar.Maximum);
        }
        private void resetProgressBar()
        {
            progressBar.Value = 0;
        }

        public delegate void statusLabelDelegate(string value);
        private void setStatusLabel(string value)
        {
            WK_RESULTS.AppendLine(value);
            //Console.WriteLine("STATUS: " + value + (STOP_REQUEST ? " -STOP_REQUEST-" : ""));
            if (!STOP_REQUEST) statusLabel.Text = value;
        }

        public delegate void removeActionDelegate(Panel panel);
        private void removeAction(Panel panel)
        {
            if (actionsPanel.Controls.Contains(panel))
                actionsPanel.Controls.Remove(panel);
        }

        public delegate void addActionDelegate(String actionStr, String actionCmd, String machine, String arg1, MachineInfo mi);
        private void addAction(String actionStr, String actionCmd, String machine, String arg1, MachineInfo mi)
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Size = new Size(500, 50);
            Button button = new Button();
            button.Size = new Size(450, 50);
            panel.Controls.Add(button);
            button.Text = actionStr;
            /*
            if (actionCmd.Equals("UNLOCK") || actionCmd.Equals("EXPIRING")) //UNLOCK AND EXPIRING HAVE BEEN MOVED TO A DEDICATED TOOL
            {
                bool found = false;
                foreach (Control c in actionsPanel.Controls)
                {
                    if (c is FlowLayoutPanel && c.Controls.Count > 0 && c.Controls[0] is Button
                        && c.Controls[0].Text.Equals(actionStr)) { found = true; break; }
                }
                if (found) return;
                button.Image = UsersInfo.Resize(UsersInfo.getUserPicture(arg1), 72, 96);
                if (button.Image != null)
                {
                    panel.Size = new Size(500, 100);
                    button.Size = new Size(450, 100);
                }
                string desc = usersInfo.getDisplaynameFromUsername(arg1);
                string sip = usersInfo.getSipFromDisplayname(desc);
                if (sip != null && sip.Length > 0)
                {
                    myWPF.LyncControl lync = AuditSec.newLyncControl(this, sip, button.Parent);
                    panel.Size = new Size(500, 100);
                }
            }
            */
            button.Click += delegate
            {
                //new Thread(new ThreadStart(delegate { })).Start();
                button.Enabled = false;
                String m = machine.ToUpper();
                switch (actionCmd)
                {
                        /*
                    case "UNLOCK":
                        if (!usersInfo.unlockAD(arg1))
                        {
                            Cursor.Current = Cursors.Default;
                            MessageBox.Show("FAILED to unlock account  " + arg1, "Locked User Detection");
                            button.ForeColor = Color.Red;
                            button.Enabled = true;
                        }
                        else
                        {
                            Cursor.Current = Cursors.Default;
                            MessageBox.Show("Successfully unlocked account  " + arg1, "Locked User Detection");
                            actionsPanel.Controls.Remove(button.Parent);
                            Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                        }
                        break;
                    case "EXPIRING":
                        MessageBox.Show("Invite user " + arg1 + " to change his password.", "Expiring User Detection");
                        actionsPanel.Controls.Remove(button.Parent);
                        Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                        break;
                         */
                    case "WIN7":
                        MessageBox.Show("Please consider upgrading " + machine + " to Windows 7.", m);
                        actionsPanel.Controls.Remove(button.Parent);
                        Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                        break;
                    case "WIN7SP":
                        MessageBox.Show("Please consider upgrading " + machine + " to Windows 7 Service Pack 1.", m);
                        actionsPanel.Controls.Remove(button.Parent);
                        Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                        break;
                    case "BITLOCKER":
                        MessageBox.Show("Please consider enabling bitlocker on " + machine + ".", m);
                        actionsPanel.Controls.Remove(button.Parent);
                        Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                        break;
                    case "SCCM":
                        MessageBox.Show("Please consider troubleshooting the SCCM Client for " + machine + "."
                            + "\n\n" + arg1, m);
                        actionsPanel.Controls.Remove(button.Parent);
                        Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                        break;
                    case "VIRUS":
                        MessageBox.Show("Please consider troubleshooting the Antivirus Program for " + machine + "."
                            + "\n\n" + arg1, m);
                        actionsPanel.Controls.Remove(button.Parent);
                        Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                        break;
                    case "UACOFF":
                        MessageBox.Show("Please consider enabling User Account Control (UAC) for " + machine + ".", m);
                        actionsPanel.Controls.Remove(button.Parent);
                        Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                        break;
                    case "ADMPW":
                        string curpw = arg1;
                        string newpw = PW_DICO.ContainsKey(m) ? PW_DICO[m].ToString() : null;
                        if (newpw == null || newpw.Length == 0) newpw = firstPw(stdpwBox.Text);
                        newpw = Interaction.InputBox("New password: ", "Change " + machine + "\\Administrator password", newpw);
                        if (newpw.Length > 0)
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            if (!MachineInfo.ResetPassword(machine, "Administrator", arg1/*stdpwBox.Text*/, newpw,
                                status => { Invoke(new statusLabelDelegate(setStatusLabel), status); return true; }))
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show("FAILED to change password for  " + machine + "\\Administrator", m);
                                button.ForeColor = Color.Red;
                                button.Enabled = true;
                            }
                            else
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show("Successfully changed password for  " + machine + "\\Administrator", m);
                                actionsPanel.Controls.Remove(button.Parent);
                                Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                            }
                        }
                        else button.Enabled = true;
                        break;
                    case "NONAUTH":
                        string users = Interaction.InputBox("Non-authorized administrators: ", "Remove users from " + machine + "\\Administrators", arg1);
                        if (users.Length > 0)
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            if (!MachineInfo.RemoveMembers(machine, "Administrators", users,
                                status => { Invoke(new statusLabelDelegate(setStatusLabel), status); return true; }))
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show("FAILED to remove " + users + " from " + machine + "\\Administrators", m);
                                button.ForeColor = Color.Red;
                                button.Enabled = true;
                            }
                            else
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show("Successfully removed " + users + " from " + machine + "\\Administrators", m);
                                actionsPanel.Controls.Remove(button.Parent);
                                Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                            }
                        }
                        else button.Enabled = true;
                        break;
                    case "MISSADM":
                        string admins = Interaction.InputBox("Missing administrators: ", "Add users to " + machine + "\\Administrators", arg1);
                        if (admins.Length > 0)
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            if (!MachineInfo.AddMembers(machine, "Administrators", admins,
                                status => { Invoke(new statusLabelDelegate(setStatusLabel), status); return true; }))
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show("FAILED to add " + admins + " to " + machine + "\\Administrators", m);
                                button.ForeColor = Color.Red;
                                button.Enabled = true;
                            }
                            else
                            {
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show("Successfully added " + admins + " to " + machine + "\\Administrators", m);
                                actionsPanel.Controls.Remove(button.Parent);
                                Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                            }
                        }
                        else button.Enabled = true;
                        break;
                    case "MISPLACED":
                        m = m.Split(new[] { ',', '=' }, 2, System.StringSplitOptions.None)[1];
                        String from = machine;
                        String to = arg1;
                        from = Regex.Replace(from, @".*CN=", @"");
                        from = Regex.Replace(from, @",OU=", @"\");
                        from = Regex.Replace(from, @",DC=.*", @"");
                        to = Regex.Replace(to, @".*/OU=", @"");
                        to = Regex.Replace(to, @",OU=", @"\");
                        to = Regex.Replace(to, @",DC=.*", @"");
                        try
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            DirectoryEntry machinede = new DirectoryEntry(machine);
                            DirectoryEntry Workstations = new DirectoryEntry(arg1);
                            machinede.MoveTo(Workstations);
                            Workstations.Close();
                            machinede.Close();
                            Cursor.Current = Cursors.Default;
                            MessageBox.Show("Successfully moved:\n\t" + from + "\n\tto->\t" + to, m);
                            actionsPanel.Controls.Remove(button.Parent);
                            Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                        }
                        catch (Exception e)
                        {
                            Cursor.Current = Cursors.Default;
                            MessageBox.Show("FAILED to move:\n\t" + from + "\n\tto->\t" + to + "\nError: " + e.ToString(), m);
                            button.ForeColor = Color.Red;
                            button.Enabled = true;
                        }
                        break;
                    case "WROWNER":
                        string desc_new = arg1;
                        bool clear = false, move = false;
                        try
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            mi.machinede.InvokeSet("Description", desc_new);
                            mi.machinede.CommitChanges();
                            mi.machinede.Close();
                            mi.calc(true);
                            clear = MachineInfo.clearSeclogs(m, null, null);
                            if (!clear)
                                Console.WriteLine("Cannot clear security logs on: " + m);
                            move = false;
                            from = mi.from;
                            to = mi.to;
                            if (mi.misplaced) try
                            {
                                mi.machinede.MoveTo(mi.Workstations);
                                mi.Workstations.Close();
                                mi.machinede.Close();
                                move = true;
                            }
                            catch (Exception ee)
                            {
                                Console.WriteLine("Cannot move: " + mi.from + " to-> " + mi.to + ". Error:" + ee.Message);
                            }

                            Cursor.Current = Cursors.Default;
                            MessageBox.Show("Successfully set owner: " + desc_new
                                + (!clear ? "\nHowever the security logs could not be cleared." : "\nThe security logs were cleared as well.")
                                + (!move ? "\nHowever the AD object could not be moved " : "\nThe AD object was moved")
                                + " from " + from + " to " + to, m);
                            actionsPanel.Controls.Remove(button.Parent);
                            Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                        }
                        catch (Exception e)
                        {
                            Cursor.Current = Cursors.Default;
                            MessageBox.Show("FAILED to set owner: " + desc_new + "\nError: " + e.ToString(), m);
                            button.ForeColor = Color.Red;
                            button.Enabled = true;
                        }
                        break;
                    default:
                        actionsPanel.Controls.Remove(button.Parent);
                        Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
                        break;
                }//switch
                
            }; //+=Click
            actionsPanel.Controls.Add(panel);


            /*
             if (actionCmd.Equals("UNLOCK") || actionCmd.Equals("EXPIRING")) actionsPanel.Controls.SetChildIndex(panel, 0);
             */


            Invoke(new issuesCountBoxDelegate(setIssuesCountBox));
            
        }

        string[] getPws(string pws_)
        {
            if (pws_ == null || pws_.Length == 0) return new string[] { };
            string[] pws = pws_.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return pws == null ? new string[] { } : pws;
        }

        string firstPw(string pws_)
        {
            string[] pws = getPws(pws_);
            return pws.Length > 0 ? pws[0] : "";
        }


        public void startButton_Click(object sender, EventArgs e)
        {
            WK_RESULTS.Clear();
            if (stdpwBox.Text.Length == 0) doADMPW(false);

            if (WK_COUNT > WK_MAX) try {
                String x = Interaction.InputBox(
                    WK_COUNT + " workstations are targeted."
                    + "The maximum is currently set to " + WK_MAX + "."
                    + "\nThe search will therefore be truncated."
                    + "\nTry to select a department to narrow the search."
                    + "\n-or- increase reasonably the maximum: ",
                    "Over " + WK_MAX + " workstations", "" + WK_MAX);
                if (x == null || x.Length == 0) return;
                int new_max = int.Parse(x);
                if (new_max > 2000)
                    MessageBox.Show(new_max + " is too much.\nKeep " + WK_MAX + " as the maximum.", "Workstation maximum");
                else if (new_max < 10)
                    MessageBox.Show(new_max + " is too few.\nKeep " + WK_MAX + " as the maximum.", "Workstation maximum");
                else WK_MAX = new_max;
            }
            catch(Exception ee)
            {
                MessageBox.Show("Invalid entry.\nKeep " + WK_MAX + " as the maximum.", "Workstation maximum");
            }

            Invoke(new enableGUIDelegate(setEnableGUI), false);
            new Thread(new ThreadStart(WaitComplete)).Start();
            new Thread(new ThreadStart(VerifyAll)).Start();
            if (doSCCM()) SCCMCheck_start();
            
        }




        public delegate void setTaskViewDelegate(List<string> active);
        private void setTaskView(List<string> active)
        {
            tasksView.Items.Clear();
            if (active != null) foreach (string task in active) tasksView.Items.Add(task);
        }


        void WaitComplete()
        {
            Invoke(new statusLabelDelegate(setStatusLabel), "Verifying " + COMP_SEARCH.SearchRoot.Name);
            int max = progressBar.Maximum < WK_MAX ? progressBar.Maximum : WK_MAX;
            while (progressBar != null
                && progressBar.Value < max
                   && !STOP_REQUEST)
            {
                Console.WriteLine("(...) Progress: " + progressBar.Value + "/" + max);

                
                List<string> active = Verifying.OrderBy(mi => - mi.getRuntime()).Select(mi => mi.getTask()).ToList<string>();
                //Console.WriteLine("(...) Active: " + string.Join(", ", active.ToArray()));

                Invoke(new setTaskViewDelegate(setTaskView), active);

                Thread.Sleep(2500);
            }
            if (progressBar == null) return;
            Invoke(new statusLabelDelegate(setStatusLabel),
                        "Verification " + (STOP_REQUEST || WK_DONE > WK_MAX ?
                            "incomplete (" + progressBar.Value + "/" + progressBar.Maximum + ")" : "finished")
                + ". Please take corrective actions now.");

            Invoke(new enableGUIDelegate(setEnableGUI), true);
            Invoke(new progressBarDelegate(resetProgressBar));
            Invoke(new setTaskViewDelegate(setTaskView), new List<string>());
        }

        void VerifyAll()
        {
            WK_DONE = 0;
            ThreadPool.SetMaxThreads(12, 12);
            //ThreadPool.SetMinThreads(5, 5);
            try
            {
                Hashtable info = new Hashtable();
                foreach (SearchResult r in COMP_SEARCH.FindAll())
                {
                    MachineInfo mi = new MachineInfo(r, usersInfo, false);
                    info.Add(mi.machine, mi);
                }
                foreach (string machine in info.Keys)
                {
                    WK_DONE++; if (WK_DONE > WK_MAX) break;
                    if (STOP_REQUEST) break;
                    MachineInfo mi = (MachineInfo)info[machine];

                    if (MachineInfo.matches(mi.machine, WkMaskBox.Text))
                    {
                        mi.calc(false);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyMachine), mi);
                    }
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("---Error: " + ee.ToString());
            }

        }

        List<MachineInfo> Verifying = new List<MachineInfo>();
        void VerifyMachine(object state)
        {
            MachineInfo mi = state as MachineInfo;
            Verifying.Add(mi); mi.dotask("PING");

            String description = "'" + mi.machine + "' (" + mi.description + ")";
            description = description.Replace("SCCM", "").Replace("()", "");
            Invoke(new statusLabelDelegate(setStatusLabel), "Verifying " + description + "...");


            bool skip = false;
            Ping ping = new Ping(); PingReply reply;
            try
            {
                if ((reply = ping.Send(mi.machine)).Status != IPStatus.Success) skip = true;
            }
            catch (Exception e)
            {
                skip = true;
            }

            if (skip) 
                Invoke(new statusLabelDelegate(setStatusLabel), "Skip " + description + " since it is not available.");
            else try
            {

                if (!mi.os.ToUpper().StartsWith("WINDOWS 7"))
                {
                    if (doWIN7() && mi.dotask("WIN7"))
                    {
                        String status = description + " has not Windows 7";
                        Invoke(new statusLabelDelegate(setStatusLabel), status);
                        Invoke(new addActionDelegate(addAction), new Object[]{"ACKNOWLEDGE: " + status, "WIN7",
                            mi.machine, null, null});
                    }
                }
                else
                {
                    if (doWIN7SP() && mi.dotask("WIN7SP")
                        && mi.ospak.CompareTo("Service Pack 1") < 0)
                    {
                        String status = description + " has not Windows 7 Service Pack 1";
                        Invoke(new statusLabelDelegate(setStatusLabel), status);
                        Invoke(new addActionDelegate(addAction), new Object[] { "ACKNOWLEDGE: " + status, "WIN7SP",
                            mi.machine, null, null });
                    }
                    string protectionStatus = doBITLOCKER() ? MachineInfo.getProtectionStatus(mi.machine) : "";
                    if (doBITLOCKER() && mi.dotask("BITLOCKER") && MachineInfo.isLaptop(mi.machine)
                        && (//mi.recovery.Length == 0 || //Bitlocker not available in AD anymore
                           "off".Equals(protectionStatus)
                        || "unknown".Equals(protectionStatus)
                        ))
                    {

                        if (protectionStatus == null) protectionStatus = "";
                        protectionStatus = protectionStatus.Replace("off", " is partially encrypted with Bitlocker (ie. protection is suspended)");
                        protectionStatus = protectionStatus.Replace("unknown", " may have a Bitlocker encryption locked state");
                        String status = description + protectionStatus;


                        /*
                        if (mi.recovery.Length != 0 &&
                            "off".Equals(protectionStatus))
                            protectionStatus = " is partially encrypted with Bitlocker (ie. protection is suspended)";
                        protectionStatus = protectionStatus.Replace("off", " is not or partially encrypted with Bitlocker (or key is available in clear)");
                        protectionStatus = protectionStatus.Replace("unknown", " may have a Bitlocker encryption locked state");
                        String status = description;

                        if (mi.recovery.Length == 0)
                            status += " is not encrypted with Bitlocker (or key publication in AD is failed, or you are missing rights in AD)";
                        else if (!"".Equals(protectionStatus))
                            status += protectionStatus;
                        */


                        Invoke(new statusLabelDelegate(setStatusLabel), status);
                        Invoke(new addActionDelegate(addAction), new Object[] { "ACKNOWLEDGE: " + status, "BITLOCKER",
                            mi.machine, null, null });
                    }
                    if (doMISPLACED() && mi.dotask("MISPLACED")
                        && mi.misplaced)
                    {
                        String status = description + " is placed in the wrong OU.";
                        Invoke(new statusLabelDelegate(setStatusLabel), status);
                        Invoke(new addActionDelegate(addAction), new Object[] { "REMEDIATE: " + status
                        + "\nMove: " + mi.from + "\nto-> " + mi.to,
                        "MISPLACED", mi.machinede.Path, mi.Workstations.Path, null});
                    }

                    if (doSCCM() && mi.dotask("SCCM"))
                        SCCMCheck_enqueue(mi.machine, description);

                    if (doADMPW() && mi.dotask("ADMPW"))
                    {
                        foreach(string pw in getPws(stdpwBox.Text))
                        {
                            if (MachineInfo.isValidCredential(mi.machine, mi.machine + "\\Administrator", pw))
                            {
                                String status = description + " has standard administrator password";
                                Invoke(new statusLabelDelegate(setStatusLabel), status);
                                Invoke(new addActionDelegate(addAction), new Object[] {"REMEDIATE: " + status, "ADMPW",
                                    mi.machine, pw, null });
                                break;
                            }
                        }
                    }

                    string nonauth; int uac;
                    if (doNONAUTH() && mi.dotask("NONAUTH")
                        && MachineInfo.isReachableAndValidNetbios(mi.machine))
                    {
                        if ((nonauth = usersInfo.RemoveDelocalizedUsers(
                            MachineInfo.getGroupMembers(mi.machine, "administrators", null, null, true, false, adminsBox.Text)
                            )) != null && nonauth.Length > 0)
                        {
                            String users = nonauth.Trim().Replace("\r\n", ", ");
                            String status = description + " has non-authorized administrators: " + users;
                            Invoke(new statusLabelDelegate(setStatusLabel), status);
                            Invoke(new addActionDelegate(addAction), new Object[] { "REMEDIATE: " + status, "NONAUTH",
                                mi.machine, users, null });
                        }
                        if ((uac = MachineInfo.getUACStatus(mi.machine, null, null, true)) != 1)
                        {
                            String status = description + " has User Access Control (UAC) " + (uac == 0 ? "disabled" : "unknown");
                            Invoke(new statusLabelDelegate(setStatusLabel), status);
                            Invoke(new addActionDelegate(addAction), new Object[] { "ACKNOWLEDGE: " + status, "UACOFF",
                                mi.machine, null, null });
                        }
                    }

                    string Administrators;
                    if (doNONAUTH() && mi.dotask("MISSADM")
                        && MachineInfo.isReachableAndValidNetbios(mi.machine)
                        && (Administrators = MachineInfo.getGroupMembers(mi.machine, "administrators",
                        null, null, false, true, adminsBox.Text)) != null && Administrators.Length > 0)
                    {

                        List<string> current_Administrators = new List<string>(), todo_Administrators = new List<string>();
                        current_Administrators.AddRange(Administrators.Trim().Replace("\r\n", ",").ToUpper().Split(','));

                        //Console.WriteLine("MISSADM     " + mi.machine + " current: " + string.Join(", ", current.ToArray()));

                        List<string> MYCOMPANY_Administrators = current_Administrators.FindAll(
                            admin => admin.StartsWith("MYCOMPANY\\SERVICEDESK-") || admin.StartsWith("MYCOMPANY\\LOCALSUPPORT-"));

                        if (MYCOMPANY_Administrators.Find(admin => admin.EndsWith("SERVICEDESK-WORLDWIDE")) == null)
                            todo_Administrators.Add("MYCOMPANY\\ServiceDesk-Worldwide");
                        if (MYCOMPANY_Administrators.Find(admin => admin.EndsWith("LOCALSUPPORT-WORLDWIDE")) == null)
                            todo_Administrators.Add("MYCOMPANY\\LocalSupport-Worldwide");

//2016-10 not necessary anymore
                        //if (current.Find(ServiceDesk => ServiceDesk.EndsWith("-" + mi.domain)) == null)
                        //    todo.Add("MYCOMPANY\\ServiceDesk-" + mi.domain);

                        //Console.WriteLine("    MISSADM " + mi.machine + " todo: " + string.Join(", ", todo.ToArray()));

                        if (todo_Administrators.Count > 0)
                        {
                            string missing = string.Join(", ", todo_Administrators.ToArray());
                            String status = description + " has missing ServiceDesk security groups: " + missing;
                            Invoke(new statusLabelDelegate(setStatusLabel), status);
                            Invoke(new addActionDelegate(addAction), new Object[] { "REMEDIATE: " + status, "MISSADM",
                                mi.machine, missing, null });
                        }
                    }

                    string loggedin;
                    if (doMISPLACED() && mi.dotask("WROWNER")
                        && MachineInfo.isReachableAndValidNetbios(mi.machine)
                        && (loggedin = MachineInfo.getCurrentUsers(mi.machine)) != null)
                    {
                        loggedin = loggedin.ToUpper();
                        string[] s = loggedin.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        if (s != null && s.Length > 0
                            && loggedin.IndexOf(mi.domainuser.ToUpper()) < 0)
                        {
                            string disp_cur = usersInfo.getDisplaynameFromUsername(mi.domainuser.Split('\\')[1]);
                            String domainuser_new = loggedin.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
                            String disp_new = usersInfo.getDisplaynameFromUsername(domainuser_new.Split('\\')[1]);

                            Console.WriteLine("Found possible Wrong Owner: " + mi.machine
                                + " - Current: " + mi.domainuser + " (" + disp_cur + ")"
                                + " - Loggedin: " + domainuser_new + " (" + disp_new + ")");

                            if (disp_new != null && disp_new.Length > 0)
                            {
                                DirectoryEntry userde_new = usersInfo.getDirectoryentryFromUsername(domainuser_new.Split('\\')[1]);
                                String status = description + " may have the wrong owner set.";
                                Invoke(new statusLabelDelegate(setStatusLabel), status);
                                Invoke(new addActionDelegate(addAction), new Object[] { "REMEDIATE: " + status
                                    + "\nReplace owner: " + disp_cur + " to-> " + disp_new
                                    + "\n1/Clear the Security Logs 2/Move to appropriate OU and Set AD Description",
                                    "WROWNER", mi.machine, disp_new, mi});
                            }
                        }
                    }

                    if (doVIRUS()
                        && MachineInfo.isReachableAndValidNetbios(mi.machine))
                    {
                        object[] AVInfo = MachineInfo.getAVInfo(mi.machine, false);
                        DateTime AVSignatureApplied = (DateTime)AVInfo[0];
                        int days; string status = null;
                        if (AVSignatureApplied == DateTime.MinValue)
                            status = description + " has unknown Antivirus Definitions";
                        else if ((days = Convert.ToInt32((DateTime.Now - AVSignatureApplied).TotalDays)) > 7)
                        {
                            Console.WriteLine(AVInfo[1].ToString());
                            status = description + " has rather outdated Antivirus Definitions\n("
                            + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy HH:mm}", AVSignatureApplied) + ")"
                            + " = " + days + " day" + (days > 1 ? "s" : "") + " ago";
                        }
                        if (status != null)
                        {
                            Invoke(new statusLabelDelegate(setStatusLabel), status);
                            Invoke(new addActionDelegate(addAction), new Object[] { "ACKNOWLEDGE: " + status,
                            "VIRUS", mi.machine, null, null });
                        }
                    }




                }

            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }

            //Thread.Sleep(1000);
            Invoke(new progressBarDelegate(setProgressBar));
            mi.dotask("");
            Verifying.Remove(mi);
        }




        private void stopButton_Click(object sender, EventArgs e)
        {
            if (doSCCM()) SCCMCheck_stop();
            STOP_REQUEST = true;
            Thread.Sleep(5000);
        }

        Boolean STOP_REQUEST = false;





        private void saveButton_Click(object sender, EventArgs e)
        {
            saveReportFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveReportFileDialog.Filter = "Text File|*.txt";
            saveReportFileDialog.Title = "Save the Report File";
            saveReportFileDialog.FileName =
                (dptBox.Text.Length > 0 ? dptBox.Text + "-" : "")
                + OUBox.Text
                + "-" + domainBox.Text.Replace('.', '-')
                + "-" + DateTime.Now.ToString("yyyy-M-dd");
            saveReportFileDialog.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            Console.WriteLine("Save the report into File: " + saveReportFileDialog.FileName);
            using (StreamWriter outfile = new StreamWriter(saveReportFileDialog.FileName))
            {
                outfile.Write(WK_RESULTS.ToString());
                outfile.Close();
            }
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:vincent.fontaine@MYCOMPANY.com");
        }

        ComputerManagement compmgmt = null;

        private void elevatorButton_Click(object sender, EventArgs e)
        {
            if (compmgmt == null) compmgmt = new ComputerManagement(ref WkMaskBox, ref OUMaskBox, ref adminsBox, this);
            this.Hide();
            compmgmt.setMachine("");
            compmgmt.ShowDialog();
        }


        private void lockedAccountTimer_Tick(object sender, EventArgs e)
        {
            /*
            if (!doLOCK()) return;

            foreach (Control control in actionsPanel.Controls)
            {
                if (control is Panel && ((Panel)control).Controls.Count > 0 && ((Panel)control).Controls[0] is Button)
                {
                    Panel panel = control as Panel;
                    Button button = ((Panel)control).Controls[0] as Button;
                    if (button.Text.StartsWith("UNLOCK"))
                    {
                        string desc = button.Text.Split(new char[]{':', '\''})[1].Trim();
                        string user = usersInfo.getUsernameFromDisplayname(desc);
                        bool unlocked = UsersInfo.isUnlockedAD(usersInfo.getDirectoryentryFromUsername(user));
                        if (unlocked) Invoke(new removeActionDelegate(removeAction), new Object[] {panel});
                    }
                }
            }

            foreach (string user in usersInfo.getLockedUsers())
            {
                string desc = usersInfo.getDisplaynameFromUsername(user);
                Invoke(new addActionDelegate(addAction), new Object[] { "UNLOCK: " + desc + "'s account is locked",
                    "UNLOCK", "", user, null});
            }
            foreach (object[] o in usersInfo.getExpiringUsers(90))
            {
                string user = (string)o[0]; int days = (int)o[1];
                string desc = usersInfo.getDisplaynameFromUsername(user);
                Invoke(new addActionDelegate(addAction), new Object[] { "EXPIRING: " + desc + "'s account is expiring in " + days + " day" + (days > 1 ? "s" : ""),
                    "EXPIRING", "", user, null});
            }
             * */
        }



        private void button1_Click(object sender, EventArgs e)
        {
            actionsPanel.Controls.Clear();
            issuesCountBox.Text = "0";
        }

        SCCMCheck sccmcheck = null;
        private void button2_Click(object sender, EventArgs e)
        {
            if (sccmcheck == null) sccmcheck = new SCCMCheck(this);
            this.Hide();
            sccmcheck.ShowDialog();
        }



        void SCCMCheck_doit()
        {
            List<sccmtodoitem> done = new List<sccmtodoitem>();
            foreach(sccmtodoitem item in sccmtodo)
            {
                if (heartbeats.ContainsKey(item.machine))
                {
                    done.Add(item);
                    DateTime heartbeat = (DateTime)heartbeats[item.machine];
                    string status = null; int days = 0;
                    if ((days = Convert.ToInt32((DateTime.Now - heartbeat).TotalDays)) > 7)
                        status = "SCCM reports a heartbeat for " + item.description + " that is rather outdated ("
                            + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy HH:mm}", heartbeat) + ")"
                            + " = " + days + " day" + (days > 1 ? "s" : "") + " ago";
                    else if (heartbeat == DateTime.MinValue)
                        status = "SCCM reports NO Heartbeat for " + item.description;
                    else if (heartbeat == DateTime.MaxValue)
                        status = "SCCM reports NO Client for " + item.description;
                    if (status != null)
                    {
                        Invoke(new statusLabelDelegate(setStatusLabel), status);
                        Invoke(new addActionDelegate(addAction), new Object[] { "ACKNOWLEDGE: " + status,
                            "SCCM", item.machine, null, null });
                    }
                }
            }
            sccmtodo.RemoveAll(item => done.Contains(item));
        }

        void SCCMCheck_enqueue(string machine, string description)
        {
            sccmtodo.Add(new sccmtodoitem(machine.ToUpper(), description));
        }

        Hashtable heartbeats = new Hashtable();

        class sccmtodoitem {
            public sccmtodoitem(string machine, string description)
            {
                this.machine = machine;
                this.description = description;
            }
            public string machine { get; set; }
            public string description { get; set;  }
        }

        List<sccmtodoitem> sccmtodo = new List<sccmtodoitem>();

        void SCCMCheck_start()
        {
            AuditSec.InitializeSMS(false, MYCOMPANY_Settings.SMS_DEFAULT_SERVER, MYCOMPANY_Settings.SMS_DEFAULT_SITE);
            if (smsWorker.IsBusy) Thread.Sleep(10000);
            if (smsWorker.IsBusy) Thread.Sleep(10000);
            if (smsWorker.IsBusy) Thread.Sleep(10000);
            if (!smsWorker.IsBusy)
            {
                string site = OUBox.Text;
                string mask = WkMaskBox.Text;
                mask = mask.Replace(".*", "%").Replace(".", "_");
                if (mask.StartsWith("^")) mask = mask.Substring(1);
                if (mask.EndsWith("$")) mask = mask.Substring(0, mask.Length - 1);
                expandbrackets(ref mask);
                expandparenthesisOR_(ref mask);
                string Name_WHERE = "Name LIKE '" + site + "%' OR " + mask;
                Console.WriteLine("\n***********************************************************************\n");
                Console.WriteLine("    smsWorker WHERE " + Name_WHERE + "\n");
                Console.WriteLine("\n***********************************************************************\n");
                if (!smsWorker.IsBusy) smsWorker.RunWorkerAsync(Name_WHERE);
                else Console.WriteLine("smsWorker busy!");
            }

        }

        void SCCMCheck_stop()
        {
            smsWorker.CancelAsync();
        }

        /*
         
        [ ] Any one character within the specified range ([a=f]) or set ([abcdef]).
         
        ^ Any one character not within the range ([^a=f]) or set ([^abcdef].)
         
        % Any string of 0 (zero) or more characters.
            The following example finds all instances where "Win" is found anywhere in the class name:
            SELECT * FROM meta_class WHERE __Class LIKE "%Win%" 
         
        _ (underscore) Any one character.
            Any literal underscore used in the query string must be escaped by placing it inside [] (square brackets).
        
         AND
         
         OR
         
         NOT
         
         = Equal to 
        <> Not Equal to 
        != Not Equal to 
        < Less than 
        > Greater than 
        <= Less than or equal to 
        >= Greater than or equal to 
        !< Not Less than 
        !> Not greater than 

        */

        bool expandbrackets(ref string s)
        {
            int max = 100, i = 0;
            bool x = false;
            while (expandbracket(ref s) && i < max) { x = true; i++; }
            return x;
        }

        bool expandbracket(ref string s)
        {
            int i = s.IndexOf('{');
            if (i < 0) return false;
            int j = s.IndexOf('}', i + 1);
            if (j < 0) return false;
            try {
                int n = int.Parse(s.Substring(i + 1, j - i - 1));
                int l = i - 1;
                if (l < 0 || s[l] != ']') return false;
                int k = l - 1;
                while (k >= 0) if (s[k] == '[') break; else k--;
                if (k < 0) return false;
                string m = s.Substring(k + 1, l - k - 1);
                string x = "";
                for (int z = 0; z < n; z++) x += "[" + (m.Replace('-', '=')) + "]";
                s = s.Substring(0, k) + x + s.Substring(j + 1);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        bool expandparenthesisOR_(ref string s)
        {
            bool x = expandparenthesisOR(ref s);
            if (!x) s = "Name LIKE '" + s +"'";
            return x;

        }

        bool expandparenthesisOR(ref string s) {
            int i = s.IndexOf('(');
            if (i < 0) return false;
            int j = s.IndexOf(')', i + 1);
            if (j < 0) return false;
            string[] OR = s.Substring(i + 1, j - i - 1).Split('|');
            if (OR == null || OR.Length == 0) return false;
            string f = s.Substring(j + 1);
            string x = "";
            for(int z = 0; z < OR.Length; z++) x += (z == 0 ? "" : " OR ") + "Name LIKE '" + OR[z] + f + "'";
            s = x;
            return true;
        }

        private void smsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker w = sender as BackgroundWorker;
            string Name_WHERE = (string)e.Argument;
            heartbeats.Clear();
            MachineInfo.getSMSClientHeartbeat(AuditSec.settings.smssrv, AuditSec.settings.smssite, "WHERE " + Name_WHERE, ref heartbeats,
                report => { /*Console.WriteLine(report);*/ SCCMCheck_doit();  return !w.CancellationPending; },
                error => {Console.WriteLine(error); w.CancelAsync(); return true;}
            );
            if (!w.CancellationPending) SCCMCheck_doit();
        }

        private void smsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void smsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        ComputerReenabler compenab = null;
        private void CompEnabButton_Click(object sender, EventArgs e)
        {
            if (compenab == null) compenab = new ComputerReenabler(this);
            this.Hide();
            compenab.ShowDialog();
        }



        private void showOptions(bool opt)
        {
            this.Size = new Size(opt ? 1024 : 800, 505);
            optButton.Text = opt ? "<  <  <    Show less Options    <  <  <" : ">  >  >    Show more Options    >  >  >";
        }

        private void optButton_Click(object sender, EventArgs e)
        {
            showOptions(this.Size.Width <= 800);
        }

        TreeWalker walker = TreeWalker.ControlViewWalker;

        private void spyTimer_Tick(object sender, EventArgs e)
        {
            if (detectBox.Checked) try
            {
                string tel = null, desc = null;
                foreach (AutomationElement e1 in AutomationElement.RootElement.FindAll(TreeScope.Children, Condition.TrueCondition))
                {
                    string caption = (string)e1.GetCurrentPropertyValue(AutomationElement.NameProperty); //nonComVisibleBaseClass problem
                    if (caption.StartsWith("IT or HR Requests Detail"))
                    {
                        AutomationElement e2 = e1.FindFirst(TreeScope.Descendants,
                            new PropertyCondition(AutomationElement.NameProperty, "Affected User (required)"));
                        if (e2 != null) e2 = walker.GetParent(e2);
                        if (e2 != null) e2 = walker.GetParent(e2);
                        string affected = "";
                        if (e2 != null) affected = (string)e2.GetCurrentPropertyValue(AutomationElement.NameProperty);
                        if (affected.StartsWith("Affected User (required)"))
                        {
                            affected = affected.Substring(affected.IndexOf((char)160)).Trim();
                            string[] ufl = affected.Split(new char[] { ' ' });
                            if (ufl != null && ufl.Length >= 3) desc = ufl[2] + ", " + ufl[1];
                            //Console.WriteLine("\n#########################\n Affected User detected. \n#########################\n");
                        }
                        
                    }
                    else if (caption.StartsWith("Incoming Call"))
                    {
                        foreach (AutomationElement e2 in e1.FindAll(TreeScope.Subtree, Condition.TrueCondition))
                        {
                            string name = (string)e2.GetCurrentPropertyValue(AutomationElement.NameProperty);
                            if (name.StartsWith("Incoming Call")) tel = name.Substring(13).Trim();
                            else if (name.IndexOf(',') >= 0)
                            {
                                desc = name;
                                Console.WriteLine("\n#########################\n Incoming Call detected. \n#########################\n");
                            }
                            break;
                        }
                    }
                }
                if (tel == null) tel = "";
                if (desc != null)
                {
                    string user = usersInfo.getUsernameFromDisplayname(desc);
                    if (user == null || user.Length == 0)
                    {
                        Console.WriteLine("Unknown user: " + desc);
                        return;
                    }
                    string domainuser = usersInfo.getDomainuserFromUsername(user);
                    //Console.WriteLine("User: " + desc);
                    DirectorySearcher USER_COMP_SEARCH = MachineInfo.getSearcher(COMP_SEARCH.Filter.Replace(
                        "(&", "(&(description=*" + desc + "*)"));
                    string found = null;
                    bool shown = false;
                    foreach (SearchResult r in USER_COMP_SEARCH.FindAll())
                    {
                        MachineInfo mi = new MachineInfo(r, usersInfo, false);
                        found = mi.machine.ToUpper();
                        if (compmgmt == null) compmgmt = new ComputerManagement(ref WkMaskBox, ref OUMaskBox, ref adminsBox, this);
                        if (compmgmt.getMachine().ToUpper().StartsWith(found)) return;
                        Console.WriteLine("Possible machine : " + found + " (" + mi.description + ")");

                        mi.calc(false);

                        //if (MachineInfo.getCurrentUsers(mi.machine).IndexOf(domainuser) >= 0)
                        string fqdn = mi.machinede.Properties["dNSHostName"].Value.ToString();
                        if (new Ping().Send(fqdn).Status == IPStatus.Success)
                        {
                            //                            Console.WriteLine(desc + " is logged in " + machine);
                            if (this.Visible)
                            {
                                Console.WriteLine("show Computer Management ");
                                if (this.Visible) this.Hide();
                                if (compenab != null && compenab.Visible) compenab.Hide();
                                if (compmgmt == null) compmgmt = new ComputerManagement(ref WkMaskBox, ref OUMaskBox, ref adminsBox, this);
                                compmgmt.ShowDialog();
                            }
                            if (!compmgmt.getMachine().ToUpper().StartsWith(found))
                            {
                                shown = true;
                                Console.WriteLine("set Machine " + found);
                                compmgmt.setMachine(found);
                            }

                            break;
                        }
                        else
                        {
                            Console.WriteLine(fqdn + " not reachable.");
                        }
                    }
                    if (found == null) Console.WriteLine("No machine found for " + desc);
                    else if (!shown)
                    {
                        if (this.Visible || (compenab != null && compenab.Visible))
                        {
                            Console.WriteLine("show Computer Management ");
                            if (this.Visible) this.Hide();
                            if (compenab != null && compenab.Visible) compenab.Hide();
                            if (compmgmt == null) compmgmt = new ComputerManagement(ref WkMaskBox, ref OUMaskBox, ref adminsBox, this);
                            compmgmt.ShowDialog();
                        }
                        if (!compmgmt.getMachine().ToUpper().StartsWith(found))
                        {
                            shown = true;
                            Console.WriteLine("set Machine " + found);
                            compmgmt.setMachine(found);
                        }
                    }
                    
                }
            }
            catch (Exception ee)
            {
                //Console.WriteLine("spyTimer error: " + ee.Message + "\nTargetSite: " + ee.TargetSite
                    //+ "\n" + ee.ToString()
                //);
            }
        }

        private void welcomeButton_Click(object sender, EventArgs e)
        {
            actionsPanel.Controls.Remove(welcomeButton);
        }









        private void pwsButton_Click(object sender, EventArgs e)
        {
            openPwlistFileDialog.ShowDialog();
        }


        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pwsBox.Text = "";
            string f = openPwlistFileDialog.FileName;
            pwsBox.Text = openPwlistFile(f, true) ? f : "";

        }

        public static bool openPwlistFile(string f, Boolean showResult)
        {
            PW_DICO.Clear();
            if (!File.Exists(f))
            {
                MessageBox.Show("Invalid Excel file: \"" + f + "\"\n" + "File not found.",
                    "Password List - Open File");
                return false;
            }

            string c = null;
            if (f.ToLower().EndsWith(".xlsx"))
                c = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + f
                + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
            else if (f.ToLower().EndsWith(".xls"))
                c = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + f
                + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
            else
            {
                MessageBox.Show("This is not a valid Excel file: \"" + f + "\"",
                    "Password List - Open File");
                return false;
            }
            try
            {
                OleDbConnection x = new OleDbConnection(c);
                x.Open();
                OleDbDataAdapter a = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", x);
                DataTable t = new DataTable();
                a.Fill(t);
                foreach (DataRow row in t.Rows)
                {
                    string computer = row.ItemArray[0].ToString().ToUpper().Trim();
                    string password = row.ItemArray[1].ToString().Trim();
                    if (computer.Length > 0 && !PW_DICO.ContainsKey(computer))
                        PW_DICO.Add(computer, password);
                }
                if (showResult) MessageBox.Show(PW_DICO.Count + " passwords loaded from file: \"" + f + "\"",
                    "Password List - Open File");
                else Console.WriteLine(PW_DICO.Count + " passwords loaded from file: \"" + f + "\"");
                return true;
            }
            catch (Exception ee)
            {
                PW_DICO.Clear();
                MessageBox.Show("Invalid Excel file: \"" + f + "\"\n" + ee.ToString(),
                    "Password List - Open File");
                return false;
            }
        }











        private void desiredButton_Click(object sender, EventArgs e)
        {
            openDesapFileDialog.ShowDialog();
        }

        private void openDesapFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            desapBox.Text = "";
            string f = openDesapFileDialog.FileName;
            desapBox.Text = openDesapFile(f, true) ? f : "";
        }

        private bool openDesapFile(string f, Boolean showResult)
        {
            DESIRED_APPS.Clear();
            if (!File.Exists(f))
            {
                MessageBox.Show("Invalid Text file: \"" + f + "\"\n" + "File not found.",
                    "Desired Applications List - Open File");
                return false;
            }

            if (!f.ToLower().EndsWith(".txt"))
            {
                MessageBox.Show("This is not a valid Text file: \"" + f + "\"",
                    "Desired Applications List - Open File");
                return false;
            }
            try
            {
                foreach (string line_ in File.ReadAllLines(f))
                {
                    string line = line_.Trim();
                    if (!line.StartsWith("#") && line.Length > 0)
                    {
                        string[] str = line.Split(new char[] { ';' }, StringSplitOptions.None);
                        string desiredVendor = str.Length > 0 ? str[0].Trim() : "";
                        bool forbidden = false;
                        if (desiredVendor.StartsWith("!"))
                        {
                            forbidden = true;
                            desiredVendor = desiredVendor.Substring(1);
                        }
                        string desiredName = str.Length > 1 ? str[1].Trim() : "";
                        if (desiredName.Length == 0)
                        {
                            desiredName = desiredVendor;
                            desiredVendor = "";
                        }
                        string desiredVersion = str.Length > 2 ? str[2].Trim() : "";
                        if (desiredName.Length > 0)
                            DESIRED_APPS.Add(new Object[] { desiredVendor, (forbidden ? "!" : "") + desiredName, desiredVersion });
                    }
                }
                if (showResult) MessageBox.Show(DESIRED_APPS.Count + " desired applications loaded from file: \"" + f + "\"",
                    "Desired Applications List - Open File");
                else Console.WriteLine(DESIRED_APPS.Count + " desired applications loaded from file: \"" + f + "\"");
                return true;
            }
            catch (Exception ee)
            {
                DESIRED_APPS.Clear();
                MessageBox.Show("Invalid Text file: \"" + f + "\"\n" + ee.ToString(),
                    "Desired Applications List - Open File");
                return false;
            }
        }
        
        SCCMReporting sccmlic = null;
        private void licButton_Click(object sender, EventArgs e)
        {
            
            if (sccmlic == null)
                try
                {
                    sccmlic = new SCCMReporting(ref OUMaskBox, this);
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.Message);
                }
            if (sccmlic != null)
            {
                this.Hide();
                sccmlic.ShowDialog();
            }
        }

        private void actionsPanel_Scroll(object sender, ScrollEventArgs e)
        {
            foreach (Control control in actionsPanel.Controls)
            {
                if (control is Panel && ((Panel)control).Controls.Count > 0 && ((Panel)control).Controls[0] is Button)
                {
                    Panel panel = control as Panel;
                    System.Windows.Forms.Integration.ElementHost lyncHost = ((Panel)control).Controls.Count < 2 ? null
                        : ((Panel)control).Controls[1] as System.Windows.Forms.Integration.ElementHost;
                    if (lyncHost != null) lyncHost.Refresh();
                }
            }
        }








    }
}
