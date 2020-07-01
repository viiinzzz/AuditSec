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
using System.Windows.Automation.Peers;


namespace AuditSec
{
    public partial class Unlocker : Form
    {
        UsersInfo usersInfo = new UsersInfo();

        void saveSettings(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            //base.Dispose(disposing);

            AuditSec.Exit("AD Account Unlocker tool has ended.",
                () => { base.Dispose(disposing); return true; });
        }


        public Unlocker()
        {
            InitializeComponent();
            this.Text += " v" + AuditSec.curver;
            AuditSec.Speak("Welcome to A.D. account unlocker.", false, false, AuditSec.TTSReplace);


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
            sm0 = OUMaskBox.Text;

            speakWorker.RunWorkerAsync();
            actionsWorker.RunWorkerAsync();
            new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(1000);

//                Console.WriteLine("Loading configuration files... ");

                Invoke(new picpwDelegate(setpicpw));

                Invoke(new welcomeButtonDelegate(welcomeButtonPerformClick));

            })).Start();
        }


        public delegate void welcomeButtonDelegate();
        private void welcomeButtonPerformClick()
        {
            actionsPanel.Controls.Remove(welcomeButton);
            Console.WriteLine("Ready.\n\n");
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
            if (Stop.Visible) Stop.PerformClick();
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
            setEnableGUI(true);
        }

        bool firstOUBox_SelectedIndexChanged = true;
        private void OUBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Stop.Visible) Stop.PerformClick();
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

                usersInfo.FindAll(de);

                statusLabel.Text = "Found " + usersInfo.getUsersCount() + " user" + (usersInfo.getUsersCount() > 1 ? "s" : "")
                    + " in " + domainBox.Text + "/" + OUBox.Text + "/" + dptBox.Text + "    Click the Play button.";

                string speech = "Found " + usersInfo.getUsersCount() + " user" + (usersInfo.getUsersCount() > 1 ? "s" : "")
                    + " in " + Regex.Replace(Regex.Replace(domainBox.Text.ToUpper(), @"([^.]*)\..*", "$1"), "(.)", "$1.")
                    + " " + OUBox.Text.ToLower()
                    + " " + Regex.Replace(dptBox.Text.ToUpper(), "(.)", "$1.") + ".";
                
                if (firstOUBox_SelectedIndexChanged)
                {
                    AuditSec.Speak(speech, false, false, null);
                    firstOUBox_SelectedIndexChanged = false;
                }
                else speak(speech);
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            Cursor.Current = Cursors.Default;
            setEnableGUI(true);
        }

        void speak(string x)
        {
            speechs.Clear();
            AuditSec.Speak(x, true, false, null);
        }

        private void dptBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Stop.Visible) Stop.PerformClick();
            if (domainBox.Text.Length == 0 || OUBox.Text.Length == 0) return;
            setEnableGUI(false);
            //statusLabel.Text = "Listing the Workstations...";
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
                    /*
                     * 
                     * 
                     * 
                     * */
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



        private void OUMaskBox_TextChanged(object sender, EventArgs e)
        {
            smdt = 0;
        }

        private void WkMaskBox_TextChanged(object sender, EventArgs e)
        {
            wmdt = 0;
        }




        public delegate void enableGUIDelegate(Boolean value);
        private void setEnableGUI(Boolean value)
        {
            domainBox.Enabled = value;
            OUBox.Enabled = value;
            dptBox.Enabled = value;
            OUMaskBox.Enabled = value;
            Clear.Enabled = value;
        }

        //public delegate void statusLabelDelegate(string value);
        /*private void setStatusLabel(string value)
        {
            WK_RESULTS.AppendLine(value);
            //Console.WriteLine("STATUS: " + value + (STOP_REQUEST ? " -STOP_REQUEST-" : ""));
            
        }*/

        public delegate void removeActionDelegate(Panel panel);
        private void removeAction(Panel panel)
        {
            if (actionsPanel.Controls.Contains(panel))
                actionsPanel.Controls.Remove(panel);
        }


        public delegate FlowLayoutPanel addActionDelegate(String actionStr, String actionCmd, String user, bool decentralized, Image pic);
        private FlowLayoutPanel addAction(String actionStr, String actionCmd, String user, bool decentralized, Image pic)
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Size = new Size(500, 50);
            Button button = new Button();
            button.Size = new Size(450, 46);
            panel.Controls.Add(button);
            button.Text = actionStr;
            if (decentralized) button.BackColor = System.Drawing.Color.LightCoral;
            myWPF.LyncControl lync = null;
            string pres = "";
            if (actionCmd.Equals("UNLOCK") || actionCmd.Equals("EXPIRING"))
            {
                bool found = false;
                foreach (Control c in actionsPanel.Controls)
                {
                    if (c is FlowLayoutPanel && c.Controls.Count > 0 && c.Controls[0] is Button
                        && c.Controls[0].Text.Equals(actionStr)) { found = true; break; }
                }
                if (found) return null;
                button.Image = pic;
                if (button.Image != null)
                {
                    panel.Size = new Size(500, 100);
                    button.Size = new Size(450, 96);
                }
                string desc = usersInfo.getDisplaynameFromUsername(user);
                string sip = usersInfo.getSipFromDisplayname(desc);
                if (sip != null && sip.Length > 0)
                {
                    lync = AuditSec.newLyncControl(this, sip, button.Parent);
                    pres = lync.getPresenceStatus(); Console.WriteLine(sip + " = " + pres);
                    panel.Size = new Size(500, 100);
                }                
            }

            button.Click += delegate
            {
                button.Enabled = false;
                switch (actionCmd)
                {
                    case "UNLOCK":
                        if (!usersInfo.unlockAD(user))
                        {
                            Cursor.Current = Cursors.Default;
                            speak("FAILED to unlock account  " + user);
                            MessageBox.Show("FAILED to unlock account  " + user, "Locked User Detection");
                            button.ForeColor = Color.Red;
                            button.Enabled = true;
                        }
                        else
                        {
                            Cursor.Current = Cursors.Default;
                            speak("Successfully unlocked account  " + user);
                            MessageBox.Show("Successfully unlocked account  " + user, "Locked User Detection");
                            actionsPanel.Controls.Remove(button.Parent);
                        }
                        break;
                    case "EXPIRING":
                        if (lync != null) lync.openIMWindow();
                        speak("Invite user " + user + " to change the password.");
                        MessageBox.Show("Invite user " + user + " to change the password.", "Expiring User Detection");
                        actionsPanel.Controls.Remove(button.Parent);
                        break;
                    default:
                        actionsPanel.Controls.Remove(button.Parent);
                        break;
                }//switch
                
            }; //+=Click

            string speech = actionStr.Replace(actionCmd + ": ", "");
            speech = Regex.Replace(speech, "(.*), (.*)'s ", "$2 $1's ");
            speech = Regex.Replace(speech, @"\*\*\*(.*)\*\*\*", "Beware, this staff is $1.");
            switch (pres)
            {
                case "Free":            speech += ". This user is ONLINE and free."; break;
                case "Busy":            speech += ". This user is ONLINE but busy."; break;
                case "DoNotDisturb":    speech += ". This user is ONLINE but very busy."; break;
                case "Away":            speech += ". This user is ONLINE but away."; break;

                case "Offline":         speech += ". This user is offline."; break;
                case "None":            speech += ". This user is offline."; break;
                default:
                    Console.WriteLine("status not coded: " + pres);
                    break;
            }
            speechs.Add(speech);
            return panel;
        }
        


        private void lockedAccountTimer_Tick(object sender, EventArgs e)
        {
            statusLabel.Text = "Checking " + usersInfo.getUsersCount() + " user" + (usersInfo.getUsersCount() > 1 ? "s" : "") + "...";
            speak("Account checking...");
            //if (!running) return;
            foreach (Control control in actionsPanel.Controls)
            {
                if (control is Panel && ((Panel)control).Controls.Count > 0 && ((Panel)control).Controls[0] is Button)
                {
                    Panel panel = control as Panel;
                    Button button = ((Panel)control).Controls[0] as Button;
                    if (button.Text.StartsWith("UNLOCK"))
                    {
                        string desc = button.Text.Split(new char[] { ':', '\'' })[1].Trim();
                        string user = usersInfo.getUsernameFromDisplayname(desc);
                        bool unlocked = UsersInfo.isUnlockedAD(usersInfo.getDirectoryentryFromUsername(user));
                        if (unlocked) Invoke(new removeActionDelegate(removeAction), new Object[] { panel });
                    }
                    else if (button.Text.StartsWith("EXPIRING"))
                    {
                        string desc = button.Text.Split(new char[] { ':', '\'' })[1].Trim();
                        string user = usersInfo.getUsernameFromDisplayname(desc);
                        int days = UsersInfo.daysToExpiration(usersInfo.getDirectoryentryFromUsername(user), MAXDAYS);
                        bool expiring = days < ALARMDAYS && days >=0;
                        if (!expiring) Invoke(new removeActionDelegate(removeAction), new Object[] { panel });
                    }
                }
            }
            int users_count = usersInfo.getUsersCount();

            var expiring_u = usersInfo.getExpiringUsers(MAXDAYS, ALARMDAYS).ToList().OrderByDescending(o => (int)o[1]).Select(o => new
            {
                days = (int)o[1],
                user = (string)o[0],
                desc = usersInfo.getDisplaynameFromUsername((string)o[0]),
                decentralized = usersInfo.getDecentralizedFromUsername((string)o[0]),
                external = usersInfo.getDirectoryentryFromUsername((string)o[0]).Path.ToUpper().Contains("OU=EXTERNAL,"),
                //de = usersInfo.getDirectoryentryFromUsername((string)o[0]).Path
            }).ToList(); foreach (var o in expiring_u) if (SHOW_EXTERNAL || !o.external)
            {
                actions.Insert(0, new Object[] { "EXPIRING: " + o.desc + "'s password is "
                    + (o.days == 0 ? "expired" : "expiring in " + o.days + " day" + (o.days > 1 ? "s" : ""))
                    + (o.decentralized || o.external ? "\n***" : "") + (o.decentralized ? " DECENTRALIZED" : "") + (o.external ? " EXTERNAL" : "") + (o.decentralized || o.external ? " ***" : ""),
                    "EXPIRING", o.user, o.decentralized, null });
            }

            var locked_u = usersInfo.getLockedUsers().ToList().Select(u => new
            {
                user = u,
                desc = usersInfo.getDisplaynameFromUsername(u),
                decentralized = usersInfo.getDecentralizedFromUsername(u),
                external = usersInfo.getDirectoryentryFromUsername(u).Path.ToUpper().Contains("/EXTERNAL")
            }).ToList(); foreach (var o in locked_u) if (SHOW_EXTERNAL || !o.external)
                {
                    actions.Insert(0, new Object[] { "UNLOCK: " + o.desc + "'s account is locked"
                    + (o.decentralized || o.external ? "\n***" : "") + (o.decentralized ? " DECENTRALIZED" : "") + (o.external ? " EXTERNAL" : "") + (o.decentralized || o.external ? " ***" : ""),
                    "UNLOCK", o.user, o.decentralized, null });
                }

            statusLabel.Text = "Check completed: " + users_count + " user" + (users_count > 1 ? "s" : "") + " verified. Locked: " + locked_u.Count + ". Expiring: " + expiring_u.Count + ".";
            int actions_count = locked_u.Count + expiring_u.Count;
            speak("Check completed. There are " + actions_count + " user" + (actions_count > 1 ? "s" : "") + " to look at, "
                + locked_u.Count + (locked_u.Count > 1 ? " are " : " is ") + "locked and " + expiring_u.Count + (expiring_u.Count > 1 ? " are " : " is ") + "expiring.");
            Console.WriteLine("Timer getting job done!");

        }

        static bool SHOW_EXTERNAL = false;

        static int MAXDAYS = 90;
        static int ALARMDAYS = 4;

        static Hashtable PictureCache = new Hashtable();
        static Image getPicture(string user)
        {
            if (PictureCache.Contains(user)) return (Image)PictureCache[user];
            else return (Image)(PictureCache[user] = UsersInfo.Resize(UsersInfo.getUserPicture(user), 67, 90));
        }




        List<Object[]> actions = new List<Object[]>();
        private void actionsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!IsDisposed)
                if (actions.Count() > 0)
                {
                    Object[] o = actions[0]; actions.RemoveAt(0);
                    o[4] = getPicture((string)o[2]);
                    actionsWorker.ReportProgress(50, o);
                }
                else Thread.Sleep(1000);
        }
        private void actionsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Object[] o = (Object[])e.UserState;
            FlowLayoutPanel panel = (FlowLayoutPanel)Invoke(new addActionDelegate(addAction), o);
            if (panel != null)
            {
                actionsPanel.Controls.Add(panel);
                //actionsPanel.Controls.SetChildIndex(panel, 0);
            }
        }

        List<string> speechs = new List<string>();
        private void speakWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!IsDisposed)
                if (speechs.Count() > 0)
                {
                    string speech = speechs[0]; speechs.RemoveAt(0);
                    AuditSec.SpeakWait(speech, false, false, AuditSec.TTSReplace);
                }
                else Thread.Sleep(1000);
        }



        private void clearButton_Click(object sender, EventArgs e)
        {
            actionsPanel.Controls.Clear();
            speak("Action list cleared!");
        }


        private void welcomeButton_Click(object sender, EventArgs e)
        {
            actionsPanel.Controls.Remove(welcomeButton);
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

        private void Run_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Checking " + usersInfo.getUsersCount() + " user(s)...";
            Run.Visible = false;
            Stop.Visible = true;
            lockedAccountTimer.Start();
            lockedAccountTimer_Tick(this, null);

        }

        private void Stop_Click(object sender, EventArgs e)
        {
            Stop.Visible = false;
            Run.Visible = true;
            lockedAccountTimer.Stop();
            statusLabel.Text = "Check suspended " + usersInfo.getUsersCount() + " user" + (usersInfo.getUsersCount() > 1 ? "s" : "") + ".";
            speak("Account check suspended.");
        }

        private void ouch_Click(object sender, EventArgs e)
        {
            speak("ouch!");
            System.Diagnostics.Process.Start("mailto:vincent.fontaine@MYCOMPANY.com");
        }

        private void cool_Click(object sender, EventArgs e)
        {
            speak("coooool!");
            System.Diagnostics.Process.Start("mailto:vincent.fontaine@MYCOMPANY.com");
        }







    }
}
