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


namespace AuditSec
{
    public partial class ComputerManagement : Form
    {
        AuditSecGUIForm parent = null;
        public TextBox WkMaskBox = null, OUMaskBox = null, adminsListBox = null;

        DirectorySearcher COMP_SEARCH =  MachineInfo.getSearcher();
        Hashtable COMPS = new Hashtable();
        Hashtable COMPINFO = new Hashtable();
        UsersInfo USERS = new UsersInfo();





        public ComputerManagement(ref TextBox WkMaskBox, ref TextBox OUMaskBox, ref TextBox adminsListBox,
            AuditSecGUIForm parent)
        {
            this.parent = parent;
            this.WkMaskBox = WkMaskBox;
            this.OUMaskBox = OUMaskBox;
            this.adminsListBox = adminsListBox;
            pws_loaded = true;
            constr();
        }

        bool pws_loaded = false;
        public ComputerManagement()
        {
            constr();
        }

        public void constr()
        {
            Console.WriteLine("Instanciating new Computer Management module...");
            InitializeComponent();
            this.multipcbox.Size = new System.Drawing.Size(261, 138);
            this.Text += " v" + (AuditSec.curver.EndsWith(".0") ? AuditSec.curver.Substring(0, AuditSec.curver.Length - 2) : AuditSec.curver);
            try
            {
                DomainCollection dc = Forest.GetCurrentForest().Domains;
                Domain[] domains = new Domain[dc.Count]; dc.CopyTo(domains, 0);
                domainBox.Items.AddRange(domains);
                if (Domain.GetCurrentDomain() != null)
                    domainBox.Text = Domain.GetCurrentDomain().Name;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            string me = new DirectorySearcher("(&(ObjectClass=user)(sAMAccountName="
                + UserPrincipal.Current.SamAccountName + "))")
                .FindOne().GetDirectoryEntry().Properties["DistinguishedName"].Value.ToString();
            foreach (string s in me.Split(',').Reverse())
            {
                string[] t = s.Split(new char[] { '=' }, 2);
                if (t[0].Equals("OU")) { OUBox.Text = t[1]; break; }
            }

            if (!pws_loaded)
            {
                Console.WriteLine("Trying to load password reference file...");
                if (AuditSec.settings.pws == null) AuditSec.settings.pws = "";
                if (AuditSec.settings.pws.Length == 0) openPwlistFileDialog.ShowDialog();
                if (AuditSec.settings.pws.Length == 0)
                    MessageBox.Show("No password reference file loaded.", "PC Management");
                else
                    pws_loaded = AuditSecGUIForm.openPwlistFile(AuditSec.settings.pws, false);
            }
            if (!pws_loaded) adminpwBox.BackColor = Color.DarkGray;
        }


        void saveSettings(bool disposing)
        {
            if (parent == null) AuditSec.saveSettings_PCMGMT();

            if (disposing && (components != null)) components.Dispose();
            //base.Dispose(disposing);

            if (parent == null) AuditSec.Exit("PC Management module ended.",
                () => { base.Dispose(disposing); return true; });
        }


        public void setMachine(string m)
        {
            bool found = false;
            foreach(var item in machineBox.Items)
                if (item.ToString().StartsWith(m)) {
                    machineBox.Text = item.ToString();
                    found = true;
                    break;
                }
            if (found) return;
            else //machine not in current list
            {
                MachineInfo mi = MachineInfo.getMachine(m);
                domainBox.Text = mi.dNSHostName;
                OUBox.Text = mi.ou;
                machineBox.Text = m;
            }




        }

        public string getMachine()
        {
            return machineBox.Text;
        }


        private void domainBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            OUBox.Items.Clear();
            machineBox.Items.Clear();
            machineBox.Text = "";
            machineBox_SelectedIndexChanged(sender, e);

            //machineBigBox.Text = "";
            userBox.Items.Clear();
            userBox.Text = "";
            costBox.Text = "";
            adminsBox.Items.Clear();
            adminsBox.Text = "";
            enableCompmgmt(false);
            string domain = domainBox.SelectedItem.ToString();
            DirectoryContext context = new DirectoryContext(DirectoryContextType.Domain, domain);
            Domain d = Domain.GetDomain(context);
            DirectoryEntry de = d.GetDirectoryEntry();

            DirectorySearcher ds = new DirectorySearcher(de, "(objectClass=organizationalUnit)", null, SearchScope.OneLevel);
            ds.PropertiesToLoad.Add("name");
            foreach (SearchResult r in ds.FindAll())
            {
                string ou = r.Properties["name"][0].ToString();
                if (System.Text.RegularExpressions.Regex.IsMatch(ou, OUMaskBox != null ? OUMaskBox.Text : AuditSec.defaultOUMask,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    OUBox.Items.Add(ou);
            }
        }

        private void OUBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            machineBox.Items.Clear();
            machineBox.Text = "";
            machineBox_SelectedIndexChanged(sender, e);

            //machineBigBox.Text = "";
            machineBox.Items.Add("");
            userBox.Items.Clear();
            userBox.Text = "";
            costBox.Text = "";
            userBox.Items.Add("");
            adminsBox.Items.Clear();
            adminsBox.Text = "";
            COMPS.Clear();
            COMPINFO.Clear();
            USERS.Clear();
            enableCompmgmt(false);
            if (domainBox.Text.Length == 0) return;
            string domain = domainBox.SelectedItem.ToString();

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                DirectoryEntry de = ((Domain)domainBox.SelectedItem).GetDirectoryEntry();
                foreach (DirectoryEntry de_ in de.Children)
                    if (de_.Properties["name"].Value.ToString().Equals(OUBox.Text)) { de = de_; break; }

                USERS.FindAll(de);
                userBox.Items.AddRange(USERS.getDomainUsers());

                COMP_SEARCH.SearchRoot = de;
                foreach (SearchResult r in COMP_SEARCH.FindAll())
                {
                    string machine = r.Properties["name"].Count > 0 ?
                        r.Properties["name"][0].ToString().ToUpper()
                        : "";
                    string description = r.Properties["description"].Count > 0 ?
                        r.Properties["description"][0].ToString().Replace("(SCCM)", "").Trim()
                        : "";
                    if (MachineInfo.matches(machine, WkMaskBox != null ? WkMaskBox.Text : AuditSec.defaultWkMask))
                    {
                        string m = machine + (description.Length > 0 ? " (" + description + ")" : "");
                        machineBox.Items.Add(m);
                        COMPS.Add(m, description);

                        COMPINFO.Add(m, new MachineInfo(r, USERS, false));
                    }
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            Cursor.Current = Cursors.Default;
        }



        void enableCompmgmt(bool enable)
        {
            ownerButton.Enabled = enable;
            addAdminButton.Enabled = enable;
            revokeAdminButton.Enabled = enable;
            userBox.Enabled = enable;
            findUserButton.Enabled = enable;
            findBox.Enabled = enable;
            adminsBox.Enabled = enable;
            clearusrButton.Enabled = enable;
            userLabel.Enabled = enable;
            //adminpwLabel.Enabled = enable;
            AdminResetButton.Enabled = enable;
            UsrResetButton.Enabled = enable;
            instAppsLabel.Enabled = enable;
            instAppsTable.Enabled = enable;
           WkDisabledBox.Enabled = enable;
           UsrDisabledBox.Enabled = enable;
           UsrLockedBox.Enabled = enable;
           UsrExpiredBox.Enabled = enable;
           UsrExpsoonBox.Enabled = enable;
           remoteButton.Enabled = enable;
            rButton.Enabled = enable;
            cButton.Enabled = enable;
            viewupdBox.Enabled = enable;
            refreshDesapButton.Enabled = enable;
            hotappsBox.Enabled = enable;
            officeCall.Enabled = enable;
            mobileCall.Enabled = enable;
            lyncHost.Enabled = enable;
            showStaffDetails.Enabled = enable;
            batteryButton.Enabled = enable;
            printerButton.Enabled = enable;
            showProfilesDetails.Enabled = enable; if (!enable) showProfilesDetails_last = "";
            showStaffDetails.Enabled = enable;
            if (!enable)
            {
                expandButton.Text = "vvv";
                this.Size = this.MinimumSize;
            }
            expandButton.Enabled = enable;
            costBox.Enabled = enable;
            costLabel.Enabled = enable;
            loggedinLabel.Enabled = enable;
            loggedinBox.Enabled = enable;
            useraccountLabel.Enabled = enable;
            computeraccountLabel.Enabled = enable;
            adminpwLabel.Enabled = enable;
            adminsLabel.Enabled = enable;
        }

        void enableADmgmt(bool enable)
        {
            ownerButton.Enabled = enable;
            userBox.Enabled = enable;
            findUserButton.Enabled = enable;
            findBox.Enabled = enable;
            clearusrButton.Enabled = enable;
            userLabel.Enabled = enable;
//            showStaffDetails.Enabled = enable;

            //adminpwLabel.Enabled = enable;
            //WkDisabledBox.Enabled = enable;
            //UsrDisabledBox.Enabled = enable;
            //UsrLockedBox.Enabled = enable;
            //UsrExpiredBox.Enabled = enable;
            //UsrExpsoonBox.Enabled = enable;
        }

        MachineInfo getMachineInfo()
        {
            return getMachineInfo(false);
        }

        MachineInfo getMachineInfo(bool force)
        {
            if (machineBox.Text.Length == 0) return null;
            if (!COMPINFO.ContainsKey(machineBox.Text)) return null;
            MachineInfo mi = (MachineInfo)COMPINFO[machineBox.Text];
            mi.calc(force);
            return mi;
        }

        string picuser = "";

        private void machineBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string machine = machineBox.Text.Split('(')[0].Trim();


            userBox.Text = "";
            costBox.Text = "";
            findBox.Text = "";
            IPBox.Text = "";
            adminsBox.Items.Clear();
            adminsBox.Text = "";
            picLabel.Text = "";
            picLabel.Image = null;
            picuser = "";
            adminpwBox.Text = "";
            rkeyBox.Text = "";
            mbam.Clear();
            /*
            loggedinBox.Text = "";
            makermodelBox.Text = "";
            chassisBox.Text = "";
            freeBox.Text = "";
            */
            machineBigBox.Text = machine;

            if (!machine.Equals(wmitarget))
            {
                wmitarget = null;
                loggedinBox.Text = "";
                makermodelBox.Text = "";
                chassisBox.Text = "";
                freeBox.Text = "";
                if (wmiWorker.IsBusy) wmiWorker.CancelAsync();
            }

            if (!machine.Equals(insttarget))
            {
                insttarget = null;
                instAppsTable.Rows.Clear();
                instAppsLabel.Text = "Installed Applications";
                if (instAppsWorker.IsBusy) instAppsWorker.CancelAsync();
            }

            WkDisabledBox.Checked = false;
            WkDisabledBox.Enabled = false;
            WkDisabledBox.Text = "disabled";
            UsrDisabledBox.Checked = false;
            UsrDisabledBox.Enabled = false;
            UsrDisabledBox.Text = "disabled";
            UsrLockedBox.Checked = false;
            UsrLockedBox.Enabled = false;
            UsrLockedBox.Text = "locked";
            UsrExpiredBox.Checked = false;
            UsrExpiredBox.Enabled = false;
            UsrExpiredBox.Text = "expired";
            UsrExpsoonBox.Checked = false;
            UsrExpsoonBox.Enabled = false;
            UsrExpsoonBox.Text = "soon expired";

            staffdetails.Search("", true, null);

            Thread.Sleep(100);



            officeCall.Enabled = false;
            mobileCall.Enabled = false;
            if (lyncControl != null) lyncControl.setRemoteSIP(null);
            officeTel = null;
            mobileTel = null;
            toolTip1.SetToolTip(officeCall, null);
            toolTip1.SetToolTip(mobileCall, null);


            enableCompmgmt(false);
            if (machineBox.Text.Length == 0) return;
            string desc = COMPS[machineBox.Text].ToString();

            officeTel = USERS.getOfficeTelFromDisplayname(desc);
            mobileTel = USERS.getMobileTelFromDisplayname(desc);
            officeCall.Enabled = officeTel != null;
            mobileCall.Enabled = mobileTel != null;
            toolTip1.SetToolTip(officeCall, officeTel);
            toolTip1.SetToolTip(mobileCall, mobileTel);

            string sip = USERS.getSipFromDisplayname(desc);
            if (lyncControl != null) lyncControl.setRemoteSIP(sip);
            if (sip != null && sip.Length > 0) lyncHost.Enabled = true;

            remoteButton.Enabled = machine.Length > 0;

            adminpwBox.Text = AuditSecGUIForm.PW_DICO.ContainsKey(machine) ? AuditSecGUIForm.PW_DICO[machine].ToString() : "";

            MachineInfo mi = getMachineInfo(true);
            rkeyBox.Text = mi != null ? mi.recovery : "";

            string user = desc.Length == 0 || USERS.getUsernameFromDisplayname(desc) == null ? "" : USERS.getUsernameFromDisplayname(desc);
            string domainuser = user.Length == 0 || USERS.getDomainuserFromUsername(user) == null ? "" : USERS.getDomainuserFromUsername(user);
            if (domainuser.Length > 0) showStaffDetails.Enabled = true;
            if (domainuser.Length > 0 && userBox.Items.Contains(domainuser))
            {
                userBox.Text = domainuser;
                mbam.setUser(domainuser);
            }

            Cursor.Current = Cursors.WaitCursor;
            WkDisabledBox.Text = "Querying AD...";
            Thread.Sleep(100);
            try
            {
                WkDisabledBox.Enabled = WkDisabledBox.Checked = !UsersInfo.isEnabledAD(mi.machinede);
            }
            catch (Exception ee)
            {
                mi.updatemachinede();
                WkDisabledBox.Enabled = WkDisabledBox.Checked = !UsersInfo.isEnabledAD(mi.machinede);
            }
            WkDisabledBox.Text = machine + " disabled";
            Thread.Sleep(100);
            Cursor.Current = Cursors.Default;

            Cursor.Current = Cursors.WaitCursor;
            UsrDisabledBox.Text = "Querying AD...";
            Thread.Sleep(100);
            UsrDisabledBox.Enabled = UsrDisabledBox.Checked = mi.userde == null ? false : !UsersInfo.isEnabledAD(mi.userde);
            UsrDisabledBox.Text = (mi.userde == null ? "User" : mi.userde.Properties["sAMAccountName"].Value) + " disabled";
            //DirectoryEntry usrde = user.Length == 0 || USERS.getDirectoryentryFromUsername(user) == null ? null : USERS.getDirectoryentryFromUsername(user);
            //UsrDisabledBox.Checked = !isEnabledAD(usrde);
            //UsrDisabledBox.Text = (user.Length == 0 ? "User" : user) + " disabled";
            Thread.Sleep(100);
            Cursor.Current = Cursors.Default;

            Cursor.Current = Cursors.WaitCursor;
            UsrLockedBox.Text = "Querying AD...";
            Thread.Sleep(100);
            UsrLockedBox.Enabled = UsrLockedBox.Checked = mi.userde == null ? false : !UsersInfo.isUnlockedAD(mi.userde);
            UsrLockedBox.Text = (mi.userde == null ? "User" : mi.userde.Properties["sAMAccountName"].Value) + " locked";
            Thread.Sleep(100);
            Cursor.Current = Cursors.Default;

            Cursor.Current = Cursors.WaitCursor;
            UsrExpiredBox.Text = "Querying AD...";
            Thread.Sleep(100);
            UsrExpiredBox.Enabled = UsrExpiredBox.Checked = mi.userde == null ? false : UsersInfo.isExpiredAD(mi.userde);
            UsrExpiredBox.Text = (mi.userde == null ? "User" : mi.userde.Properties["sAMAccountName"].Value) + " expired";
            Thread.Sleep(100);
            Cursor.Current = Cursors.Default;

            Cursor.Current = Cursors.WaitCursor;
            UsrExpsoonBox.Text = "Querying AD...";
            Thread.Sleep(100);
            UsrExpsoonBox.Enabled = UsrExpsoonBox.Checked = mi.userde == null ? false : UsersInfo.isExpiringSoonAD(mi.userde, 90);
            UsrExpsoonBox.Text = (mi.userde == null ? "User" : mi.userde.Properties["sAMAccountName"].Value) + " soon expired";
            Thread.Sleep(100);
            Cursor.Current = Cursors.Default;

            enableADmgmt(true);
/*            UsrDisabledBox.Enabled = mi.userde != null;
            UsrLockedBox.Enabled = mi.userde != null && UsrLockedBox.Checked;
            UsrExpiredBox.Enabled = mi.userde != null && UsrLockedBox.Checked;
            UsrExpsoonBox.Enabled = mi.userde != null && UsrLockedBox.Checked;*/

            Cursor.Current = Cursors.WaitCursor;
            picLabel.Text = "Querying DirXML...";
            Thread.Sleep(100);
            if (user.Length > 0)
            {
                picLabel.Image = UsersInfo.getUserPicture(user);
                picLabel.Text = picLabel.Image == null ? "Pict. N/A" : "Click to Save";
                picuser = desc;
            }
            else picLabel.Text = "";
            Thread.Sleep(100);
            Cursor.Current = Cursors.Default;

            //Console.WriteLine("Checking connectivity with " + machine + "...");
//makermodelBox.Text = "Trying to connect to " + machine + "...";
            Thread.Sleep(100);
            IPAddress IP = MachineInfo.ping(machine, true);

            //Console.WriteLine("Checking Netbios " + machine + "...");
            string netbios = IP == null ? "" : MachineInfo.getNetbiosName(machine);
            if (netbios == null) netbios = "";
            if (netbios.Length == 0 || !netbios.ToUpper().Equals(machine.ToUpper()))
            {
                Console.WriteLine("DNS entry for " + machine + " returned " + (IP == null ? " nothing."
                    : " an IP " + IP + " that is wrong.\nThis points to " + netbios + " in fact."));
                makermodelBox.Text = machine + " is not available.";
                chassisBox.Text = "";
                freeBox.Text = "Promote/Revoke Admin is not possible.";
                //MessageBox.Show(machine + " is not available.\n"
                //+ "\nPromote/Revoke Admin is not possible.", machine);

                if (userBox.Text.Length > 0) staffdetails.Search(domainuser, true, null);
                return;
            }
            else
            {
//makermodelBox.Text = "";
                IPBox.Text = IP == null ? "IP?" : (IP.ToString().Contains("::") ? "IPv6" : "" + IP);
            }
            Thread.Sleep(100);

            Console.WriteLine("Getting " + machine + "\\Administrators members...");
            Cursor.Current = Cursors.WaitCursor;
            string admins = MachineInfo.getGroupMembers(machine, "Administrators", null, null, true, false,
                (adminsListBox != null ? adminsListBox.Text : AuditSec.defaultAdmins));
            Cursor.Current = Cursors.Default;
            if (admins == null)
            {
                //Console.WriteLine(machine + "\\Administrators members: ? (not available)");
                makermodelBox.Text = machine + " is not available.";
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
            enableCompmgmt(true);

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
            if (u.Length > 0 && u != "N/A") staffdetails.Search(u, true, null);


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

           
            //Console.WriteLine("wmiWorker_DoWork finish: " + machine + (e.Cancel ? " - Cancel -" : ""));
        }

        private void wmiWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int n = e.ProgressPercentage;
            string text = (string)e.UserState;
            switch (n) {
                case 1: loggedinBox.Text = text;
                    if (text.IndexOf('\\') > 0) mbam.setUser(text);
                    break;
                case 2: makermodelBox.Text = text; break;
                case 3: chassisBox.Text = text; break;
                case 4: freeBox.Text = text; break;
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

            if (parent != null)
            {
                worker.ReportProgress(0, "Installed Applications - Refresh Desired Applications List... ");
                parent.refreshDesap();
            }
            worker.ReportProgress(0, "Installed Applications - Connecting to Registry Hives... ");
            //Console.WriteLine("Service list:\n" + Machine.ListServices(machine, ".*"));
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
                //Machine.GetInstalledAppsWMI(machine, ref instAppsTable, machine + @"\Administrator", adminpwBox.Text);
                int n = 0; MachineInfo.GetInstalledAppsREG(machine,
                    row => { worker.ReportProgress(50, row); return n++; },
                    () => e.Cancel = /*machineBigBox.Text != machine || */worker.CancellationPending,
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
            worker.ReportProgress(100, "Installed Applications - " + (e.Cancel ? "Incomplete" : "Complete"));
            Console.WriteLine("instAppsWorker_DoWork finish: " + machine + " - " + (e.Cancel ? "Incomplete" : "Complete"));
        }

        private void instAppsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int n = e.ProgressPercentage;
            if ( n == 0 || n == 100)
            {
                string text = (string)e.UserState;
                instAppsLabel.Text = text;
            }
            else try
                {
                    object[] row = (object[])e.UserState;
                    bool view = true;
                    if (hotappsBox.CheckState == CheckState.Checked)
                    {
                        view = false;
                        string Name = row[0].ToString();
                        string Vendor = row[2].ToString();
                        if (HotApps.isHotApp(Name, Vendor, hotapps_include, hotapps_exclude)) view = true;
                    }
                    if (viewupdBox.CheckState != CheckState.Checked)
                    {
                        string Name = row[0].ToString();
                        string Vendor = row[2].ToString();
                        if (Name.Contains("Hotfix")
                            || Name.Contains("Service Pack")
                            || Name.Contains("Language Pack")
                            || Name.Contains("Compatibility Pack")
                            || Name.Contains("MUI (")
                            || Name.Contains("IME (")
                            || Name.Contains("Update")
                            || Name.Contains("Office Proof")
                            || Name.Contains(".NET Framework")
                            || Name.Contains("Web Components")
                            || Name.Contains("SQL Server")
                            || Name.Contains("Redistributable")
                            || Name.Contains("(KB")
                            || Name.Contains("Viewer")
                            ) view = false;
                    }
                    if (view)
                    {
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
            ret.Append("Machine\t" + machineBox.Text + "\r\n");
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
            if (expandButton.Text.StartsWith("^^^"))
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
                int desired = (int)e.Value;
                e.Value = MachineInfo.getDesiredIcon(desired);
                //DataGridViewCell cell = instAppsTable[e.ColumnIndex, e.RowIndex];
                //cell.ToolTipText = stringValue;
            }
            

        }


        private void WkDisabledBox_CheckedChanged(object sender, EventArgs e)
        {
            if (getMachineInfo().machinede == null) return;
            Cursor.Current = Cursors.WaitCursor;

            bool current = !UsersInfo.isEnabledAD(getMachineInfo().machinede);
            if (current != WkDisabledBox.Checked)
            {
                string disp = ("" + getMachineInfo().machinede.Properties["displayName"][0]).Trim();
                bool success = UsersInfo.enableAD(getMachineInfo().machinede, !WkDisabledBox.Checked);
                if (!success) MessageBox.Show("Failed to enable computer account: " + disp, "PC Management");
                else MessageBox.Show("Computer account enabled: " + disp, "PC Management");
            }
            current = !UsersInfo.isEnabledAD(getMachineInfo().machinede);
            if (WkDisabledBox.Checked != current) WkDisabledBox.Checked = current;
            WkDisabledBox.Enabled = WkDisabledBox.Checked;
            Cursor.Current = Cursors.Default;
        }

        private void UsrDisabledBox_CheckedChanged(object sender, EventArgs e)
        {
            if (getMachineInfo().userde == null) return;
            Cursor.Current = Cursors.WaitCursor;

            bool current = !UsersInfo.isEnabledAD(getMachineInfo().userde);
            if (current != UsrDisabledBox.Checked)
            {
                string disp = ("" + getMachineInfo().userde.Properties["displayName"][0]).Trim();
                bool success = UsersInfo.enableUserAD(getMachineInfo().userde, !UsrDisabledBox.Checked);
                if (!success) MessageBox.Show("Failed to enable user account: " + disp, "PC Management");
                else MessageBox.Show("User account enabled: " + disp, "PC Management");
            }
            current = !UsersInfo.isEnabledAD(getMachineInfo().userde);
            if (UsrDisabledBox.Checked != current) UsrDisabledBox.Checked = current;
            UsrDisabledBox.Enabled = UsrDisabledBox.Checked;
            Cursor.Current = Cursors.Default;
        }

        private void UsrLockedBox_CheckedChanged(object sender, EventArgs e)
        {
            if (UsrLockedBox.Checked) UsrLockedBox.Checked = false;
            if (getMachineInfo().userde == null) return;
            Cursor.Current = Cursors.WaitCursor;
            if (!UsrLockedBox.Checked) UsersInfo.unlockAD(getMachineInfo().userde);
            UsrLockedBox.Checked = !UsersInfo.isUnlockedAD(getMachineInfo().userde);
            Cursor.Current = Cursors.Default;
        }



        string findUser_USER;
        string findUser_DESC;
        string findUser_PRINC;
        string findUser_DOMAIN;
        string findUser_WINNT;
        private void findUserButton_Click(object sender, EventArgs e)
        {
            findBox.Text = findBox.Text.Trim();
            if (findBox.Text.Length == 0) return;

            Cursor.Current = Cursors.WaitCursor;
            string domain = null;
            string user;
            if (findBox.Text.Split(new[] { '\\' }).Count() < 2)
            {
                user = findBox.Text;
                //Console.WriteLine("Search user: " + user + "...");
            }
            else
            {
                domain = findBox.Text.Split(new[] { '\\' })[0].ToUpper();
                user = findBox.Text.Split(new[] { '\\' })[1].ToUpper();
                //Console.WriteLine("Search user: " + domain + "\\" + user + "...");
            }

            List<Domain> domains = new List<Domain>();
            if (domain != null)
            {
                DomainCollection dc = Forest.GetCurrentForest().Domains;
                foreach (Domain d in dc) if (d.GetDirectoryEntry().Properties["name"].Value.ToString().ToUpper().Equals(domain))
                    {
                        domains.Add(d);
                        break;
                    }
            }
            else
            {
                Domain current = Domain.GetCurrentDomain();
                domains.Add(current);
                DomainCollection dc = Forest.GetCurrentForest().Domains;
                foreach (Domain d in dc) if (!d.Equals(current)) domains.Add(d);
            }

            foreach (Domain d in domains)
            {
                //Console.WriteLine("Search in: " + d.Name + "...");
                DirectorySearcher ds = new DirectorySearcher(d.GetDirectoryEntry());
                ds.Filter = "(&(ObjectClass=user)(!ObjectClass=computer)(employeeID=*)"
                    + "(|(sn=" + user + "*)(sAMAccountName=" + user + "*)))";
                ds.SearchScope = SearchScope.Subtree;
                ds.PropertiesToLoad.Add("sAMAccountName");
                ds.PropertiesToLoad.Add("displayName");
                ds.PropertiesToLoad.Add("userPrincipalName");
                ds.PropertiesToLoad.Add("givenName");//first
                ds.PropertiesToLoad.Add("sn");//last
                SearchResult r = ds.FindOne();
                if (r == null)
                {
                    //Console.WriteLine("No match.");
                    findBox.ForeColor = Color.Red;
                }
                else
                {
                    //Console.WriteLine("Match found.");
                    findUser_USER = r.Properties["sAMAccountName"].Count > 0 ?
                        r.Properties["sAMAccountName"][0].ToString().ToUpper() : "";
                    findUser_DESC = r.Properties["displayName"].Count > 0 ?
                        r.Properties["displayName"][0].ToString().Trim() : "";
                    findUser_PRINC = r.Properties["userPrincipalName"].Count > 0 ?
                        r.Properties["userPrincipalName"][0].ToString().Trim() : "";
                    findUser_DOMAIN = findUser_PRINC.Split(new[] { '@', '.' }, System.StringSplitOptions.None).Count() > 1 ?
                        findUser_PRINC.Split(new[] { '@', '.' }, System.StringSplitOptions.None)[1].ToUpper() : "";
                    findUser_WINNT = string.Format("WinNT://{0}/{1},user", findUser_DOMAIN, findUser_USER);



                    userBox.Text = "";
                    costBox.Text = "";
                    findBox.Text = findUser_DOMAIN + @"\" + findUser_USER;
                    findBox.ForeColor = Color.Green;

                    //Console.WriteLine("Match found: " + findUser_DESC);
                    //Console.WriteLine("Principal: " + findUser_PRINC);
                    //Console.WriteLine("WinNT: " + findUser_WINNT);

                    MessageBox.Show("Match found: " + findUser_DESC
                        + "\n\nIf it is not the desired user, please refine your query.", "Find User");
                    break;
                }
            }
            Cursor.Current = Cursors.Default;
        }


        private void findBox_TextChanged(object sender, EventArgs e)
        {
            findBox.ForeColor = SystemColors.WindowText;
        }

        private void userBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            findBox.Text = "";
        }



        private void addAdminButton_Click(object sender, EventArgs e)
        {
            string machine = machineBigBox.Text;
            if (machine.Length == 0)
            {
                MessageBox.Show("No machine selected.", "Promote Admin");
                return;
            }

            string domainuser = null, user = null, desc = null, winnt = null;
            if (userBox.Text.Length > 0)
            {
                //Console.WriteLine("addAdmin userBox=" + userBox.Text);
                domainuser = userBox.Text;
                user = domainuser.Split('\\')[1];
                desc = USERS.getDisplaynameFromUsername(user);
                winnt = USERS.getWinntFromUsername(user);
            }
            else if (findBox.ForeColor == Color.Green)
            {
                //Console.WriteLine("addAdmin findBox=" + findBox.Text);
                domainuser = findBox.Text;
                user = domainuser.Split('\\')[1];
                desc = findUser_DESC;
                winnt = findUser_WINNT;
            }
            if (winnt == null)
            {
                MessageBox.Show("No valid user selected: " + user, "Promote Admin");
                return;
            }

            Console.WriteLine("Adding '" + desc + "' to " + machine + "\\Administrators...");
            //Console.WriteLine(winnt);
            Cursor.Current = Cursors.WaitCursor;
            Boolean result = MachineInfo.AddMember(machine, "Administrators", winnt, status => { Console.WriteLine(status); return true; });
            Cursor.Current = Cursors.Default;

            machineBox_SelectedIndexChanged(sender, e);
//            string t = machineBox.Text;
//            machineBox.Text = "";
//            machineBox.Text = t;

            if (!result) {
                MessageBox.Show("FAILED to add '" + desc + "' to " + machine + "\\Administrators", machineBox.Text);
            } else {
                MessageBox.Show("Successfully added '" + desc + "' to " + machine + "\\Administrators", machineBox.Text);
            }
        }




        private void revokeAdminButton_Click(object sender, EventArgs e)
        {
            string machine = machineBox.Text.Split('(')[0].Trim();
            if (machine.Length == 0)
            {
                MessageBox.Show("No machine selected.", "Revoke Admin");
                return;
            }
            StringBuilder admins_to_remove = new StringBuilder();
            foreach (Object i in adminsBox.Items) if (adminsBox.GetItemChecked(adminsBox.Items.IndexOf(i)))
            {
                if (admins_to_remove.Length > 0) admins_to_remove.Append(", ");
                admins_to_remove.Append(i.ToString());
            }
            if (admins_to_remove.ToString().Length == 0)
            {
                MessageBox.Show("No user selected to revoke from Administrators.", machineBox.Text);
                return;
            }

            Console.WriteLine("Removing " + admins_to_remove.ToString() + " from " + machine + "\\Administrators...");
            Cursor.Current = Cursors.WaitCursor;
            Boolean result = MachineInfo.RemoveMembers(machine, "Administrators", admins_to_remove.ToString());
            Cursor.Current = Cursors.Default;

            machineBox_SelectedIndexChanged(sender, e);
            //            string t = machineBox.Text;
            //            machineBox.Text = "";
            //            machineBox.Text = t;

            if (!result)
            {
                MessageBox.Show("FAILED to remove " + admins_to_remove.ToString() + " from " + machine + "\\Administrators", machineBox.Text);      
            } else {
                MessageBox.Show("Successfully removed " + admins_to_remove.ToString() + " from " + machine + "\\Administrators", machineBox.Text);
            }
        }



        

        private void ownerButton_Click(object sender, EventArgs e)
        {
            if (findBox.Text.Length == 0 && userBox.Text.Length == 0 && loggedinBox.Text.Length > 0)
            {
                findBox.Text = loggedinBox.Text;
                findUserButton.PerformClick();
            }

            string domainuser = userBox.Text.Length > 0 ? userBox.Text : findBox.Text;
            if (domainuser.Length == 0) {
                MessageBox.Show("Failed to change owner: no user selected", "Change " + machineBigBox.Text + " Owner");
                return;
            }

            if (!USERS.FindOneDomainUser(domainuser))
            {
                MessageBox.Show("Failed to change owner: user not found: " + domainuser, "Change " + machineBigBox.Text + " Owner");
                return;
            }

            string desc = USERS.getDisplaynameFromUsername(domainuser.Split('\\')[1]);

            string owner = Interaction.InputBox("1/Clear the Security Logs\n\n"
                + "-and-\n\n2/Move to appropriate OU and Set AD Description to:" ,
                "Change " + machineBigBox.Text + " Owner", desc);
            if (owner == null || owner.Length == 0
                || USERS.getUsernameFromDisplayname(owner) == null)
            {
                MessageBox.Show("Failed to change owner: no user selected or unknown user.", "Change " + machineBigBox.Text + " Owner");
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {

                MachineInfo mi = getMachineInfo();

                mi.machinede.InvokeSet("Description", owner);
                mi.machinede.CommitChanges();
                mi.machinede.Close();
                mi.calc(true);
                string m = updateMachineBox(machineBox.Text);
                if (m != null) machineBox.Text = m;

                if (!MachineInfo.clearSeclogs(machineBigBox.Text, null, null)) throw new Exception("Cannot clear Security logs.");
                
                //move machinede context
                bool moved = false;
                if (mi.misplaced) try
                {
                    mi.machinede.MoveTo(mi.Workstations);
                    mi.Workstations.Close();
                    mi.machinede.Close();
                    moved = true;
                }
                catch (Exception ee)
                {
                    throw new Exception("Cannot move: " + mi.from + " to-> " + mi.to);
                }

                //enable account
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Successfully changed owner: " + owner + (moved ? " and location: " + mi.to : ""),
                    "Change " + machineBigBox.Text + " Owner");
            }
            catch (Exception ee)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Failed to change owner: " + owner
                    + "\n\n" + ee.Message,
                    "Change " + machineBigBox.Text + " Owner");
            }
        }


        string updateMachineBox(string old)
        {
            if (COMPINFO.ContainsKey(old))
            {
                machineBox.Items.Remove(old);
                MachineInfo mi = (MachineInfo)COMPINFO[old];
                mi.calc(true);
                string m = mi.machine + (mi.description.Length > 0 ? " (" + mi.description + ")" : "");
                machineBox.Items.Add(m);

                COMPINFO.Remove(old);
                COMPINFO.Add(m, mi);

                COMPS.Remove(old);
                COMPS.Add(m, mi.description);

                return m;
            }
            else
            {
                return null;
            }
        }


        private void clearusrButton_Click(object sender, EventArgs e)
        {
            userBox.Text = "";
            costBox.Text = "";
            findBox.Text = "";
        }

        string c0 = "";
        private void compfindTimer_Tick(object sender, EventArgs e)
        {
            if (compfindBox.Text.ToUpper().Equals("[SEARCH]")) compfindBox.SelectAll();
            if (compfindBox.Text.ToUpper().Equals("")) { compfindBox.Text = "[search]";  compfindBox.SelectAll(); }
            string c1 = compfindBox.Text;
            //Console.WriteLine("compfindbox=c1=" + c1 + " c0=" + c0);
            if (c1.Length > 0 && !c1.ToUpper().Equals("[SEARCH]") && c1.Equals(c0))
            {
                //Console.WriteLine("search=c1=" + c1);
                multipcbox.Items.Clear();
                multipcbox.Text = c1;
                IEnumerator i = machineBox.Items.GetEnumerator();
                i.Reset();
                Console.WriteLine("[search] " + c1);
                while (i.MoveNext()) {
                    string x = i.Current.ToString();
                    if (System.Text.RegularExpressions.Regex.IsMatch(x, ".*" + c1 + ".*",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        multipcbox.Items.Add(x);
                        //Console.WriteLine(multipcbox.Items.Count + ": " + x);
                        if (multipcbox.Items.Count >= 8)
                        {
                            MessageBox.Show("Too many matches found !\n\nThe search result has been truncated.\n"
                                + "Please refine your search criteria.", "Search " + multipcbox.Text);
                            break;
                        }
                    }
                }
                c1 = "";
                compfindBox.Text = "[search]";
                compfindBox.SelectAll();
                if (multipcbox.Items.Count == 1)
                {
                    machineBox.SelectedItem = multipcbox.Items[0].ToString();
                }
                else if (multipcbox.Items.Count >= 2)
                {
                    multipcbox.Visible = true;
                    multipcbox.Focus();
                    complementMultipcBox();
                }
            }
            c0 = c1;
        }

        private void adminsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (adminsBox.SelectedIndex >= 0)
            {
                userBox.Text = "";
                costBox.Text = "";
                findBox.Text = adminsBox.SelectedItem.ToString();
            }
        }

        void complementMultipcBox()
        {
            foreach(var i in multipcbox.Items)
            {
                string d = i.ToString();
                string m = d.IndexOf("(") >= 0 ? d.Substring(0, d.IndexOf("(")).Trim() : d;
                new Thread(new ThreadStart(delegate
                {
                    IPAddress IP = MachineInfo.ping(m, true);
                    string netbios = IP == null ? "" : MachineInfo.getNetbiosName(m);
                    if (netbios == null) netbios = "";
                    if (netbios.Length == 0 || !netbios.ToUpper().Equals(m.ToUpper()))
                        Invoke(new replaceMultipcBoxDelegate(replaceMultipcBox), d, d + " : N/A");
                    string u = MachineInfo.getCurrentUsers(m);
                    if (u != null)
                    {
                        u = u.TrimEnd('\n').Replace("\n", ", ");
                        Invoke(new replaceMultipcBoxDelegate(replaceMultipcBox), d, d + " : " + u);
                    }
                    else
                    {
                        Invoke(new replaceMultipcBoxDelegate(replaceMultipcBox), d, d + " : N/A");
                    }
                })).Start();
            }

        }

        
        public delegate void replaceMultipcBoxDelegate(string before, string after);
        void replaceMultipcBox(string before, string after)
        {
            if (multipcbox.Items.Contains(before))
            {
                multipcbox.Items.Remove(before);
                multipcbox.Items.Add(after);
            }
        }


        private void pwsetButton_Click(object sender, EventArgs e)
        {
            string machine = machineBigBox.Text;
            string m = machineBox.Text;
            string newpw = Interaction.InputBox("New password: ", "Change " + machine + "\\Administrator password", adminpwBox.Text);
            if (newpw.Length > 0)
            {
                Cursor.Current = Cursors.WaitCursor;
                //AuditSecGUIForm.ChangeUserPassword(machine, "Administrator", (string)null, newpw)
                try {
                    new DirectoryEntry("WinNT://" + machine + "/Administrator").Invoke("SetPassword", newpw);
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show("Successfully changed password for  " + machine + "\\Administrator", m);
                } catch(Exception ee) {
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show("FAILED to change password for  " + machine + "\\Administrator"
                        + "\nError: " + ee.Message, m);
                }
            }
        }

        private void ComputerManagement_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (parent != null) parent.Show();
        }

        private void picLabel_Click(object sender, EventArgs e)
        {
            if (picLabel.Image == null) return;
            savePictureFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            savePictureFileDialog.Filter = "Image File|*.jpg";
            savePictureFileDialog.Title = "Save the Picture File";
            savePictureFileDialog.FileName = picuser;
            savePictureFileDialog.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            Console.WriteLine("Save the picture into File: " + savePictureFileDialog.FileName);
            try
            {
                picLabel.Image.Save(savePictureFileDialog.FileName, ImageFormat.Jpeg);
            } catch(Exception ee) {
                MessageBox.Show("Cannot save the picture to " + savePictureFileDialog.FileName
                    + "\n\n" + ee.Message, "Save the Picture File");
            }
        }

        string officeTel = null;
        string mobileTel = null;
        private void callButton_Click(object sender, EventArgs e)
        {
            if (officeTel == null || officeTel.Length == 0) return;
            Console.WriteLine("Start Call " + officeTel);
            CiscoUCThirdPartyLib.Communication communication = new CiscoUCThirdPartyLib.Communication();
            communication.StartCall(officeTel);
        }

        private void mobileCall_Click(object sender, EventArgs e)
        {
            if (mobileTel == null || mobileTel.Length == 0) return;
            Console.WriteLine("Start Call " + mobileTel);
            CiscoUCThirdPartyLib.Communication communication = new CiscoUCThirdPartyLib.Communication();
            communication.StartCall(mobileTel);
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

        private void ComputerManagement_Shown(object sender, EventArgs e)
        {
            compfindBox.Focus();
        }


        private int getOfferAssistanceMethod()
        {
            int result = 0;
            
            // Check if System is XP/2003 with SMS Console installed
            if (File.Exists(Path.Combine(System.Environment.GetEnvironmentVariable("windir"),
                @"PCHealth\HelpCtr\Vendors\CN=Microsoft Corporation,L=Redmond,S=Washington,"
                + @"C=US\Remote Assistance\Escalation\Unsolicited\smsunsolicitedrcui.htm")))
                result = 1;
            
            // Check if system is Vista/2008 (This does not need any SMS/SCCM component)
            // But Remote Assistance feature must be installed on Win 2008 (not default) 
            if (File.Exists(Path.Combine(System.Environment.GetEnvironmentVariable("windir"),
                @"System32\msra.exe")))
                result = 2;

            return result;
        }
        

        private void OfferAssistance(string machine)
        {
            int method = getOfferAssistanceMethod();
            //Type t = System.Reflection.Assembly.GetEntryAssembly().GetType("SMSCliCtrV2.Common", true, true);
            //System.Reflection.PropertyInfo pInfo = t.GetProperty("Hostname");
            //string sHost = (string)pInfo.GetValue(null, null);
            
            if (method == 1)
            {
                Process OfferAssistance = new Process();
                OfferAssistance.StartInfo.FileName =
                    System.Environment.GetEnvironmentVariable("windir")
                    + @"\pchealth\helpctr\binaries\helpctr.exe";
                OfferAssistance.StartInfo.Arguments =
                    string.Format(
                     "-FromStartHelp -url \"hcp://CN=Microsoft Corporation,L=Redmond,S=Washington,"
                     +"C=US/Remote Assistance/Escalation/unsolicited/SMSUnsolicitedRCUI.htm\""
                     +" -ExtraArgument \"NOVICECOMPUTER={0}%\"", machine);
                OfferAssistance.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                OfferAssistance.Start();
            }

            if (method == 2)
            {
                Process OfferAssistance = new Process();
                OfferAssistance.StartInfo.FileName =
                    System.Environment.GetEnvironmentVariable("windir")
                    + @"\System32\msra.exe";
                OfferAssistance.StartInfo.Arguments = string.Format(
                    "/OfferRA {0}", machine);
                OfferAssistance.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                OfferAssistance.Start();
            }
        }

        private void remoteButton_Click(object sender, EventArgs e)
        {
            OfferAssistance(machineBigBox.Text);
        }

        private void refreshDesapButton_Click(object sender, EventArgs e)
        {
            string x = machineBox.Text;
            machineBox.Text = "";
            machineBox.Text = x;
        }

        private void instAppsTable_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void cButton_Click(object sender, EventArgs e)
        {
            ExploreC(machineBigBox.Text);
        }

        private void ExploreC(string machine)
        {
            Process p = new Process();
            p.StartInfo.FileName =
                System.Environment.GetEnvironmentVariable("windir")
                + @"\explorer.exe";
            p.StartInfo.Arguments = string.Format(
                @"\\{0}\c$", machine);
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.Start();
        }

        private void viewupdBox_CheckedChanged(object sender, EventArgs e)
        {
            refreshDesapButton.PerformClick();
        }

        string hotapps_include = HotApps.include;
        string hotapps_exclude = HotApps.exclude;

        private void hotappsBox_CheckedChanged(object sender, EventArgs e)
        {
            if (hotappsBox.CheckState == CheckState.Checked)
            {
                hotapps_include = Interaction.InputBox("Hot Apps positive list: ", "Installed applications", hotapps_include).Trim();
                if (hotapps_include.Length == 0) hotapps_include = HotApps.include;
                hotapps_exclude = Interaction.InputBox("Hot Apps negative list: ", "Installed applications", hotapps_exclude).Trim();
                if (hotapps_exclude.Length == 0) hotapps_exclude = HotApps.exclude;
                this.toolTip1.SetToolTip(this.hotappsBox, "Hot Apps: +++" + hotapps_include + " ---" + hotapps_exclude);
            }
            refreshDesapButton.PerformClick();
            Thread.Sleep(500);
            expandButton.PerformClick();
        }


        private void expandButton_Click(object sender, EventArgs e)
        {
            if (expandButton.Text.EndsWith("v"))
            {
                expandButton.Text = "^^^";
                this.Size = this.MaximumSize;
                if (instAppsTable.Rows.Count > 0) copyAppsToClipboard();
            }
            else
            {
                expandButton.Text = "vvv";
                this.Size = this.MinimumSize;
            }
        }

        private void multipcbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (multipcbox.SelectedItem == null) return;
            string selected = multipcbox.SelectedItem.ToString();
            selected = selected.IndexOf(":") >= 0 ? selected.Substring(0, selected.IndexOf(":")).Trim() : selected;
            multipcbox.Visible = false;
            compfindBox.Text = "[search]";
            compfindBox.Focus();
            machineBox.Text = selected;
        }

        private void compfindBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void multipcbox_TextChanged(object sender, EventArgs e)
        {
            if (multipcbox.Visible)
                compfindBox.Text = multipcbox.Text;
        }



        

        private void domainBox_Click(object sender, EventArgs e)
        {
            multipcbox.Visible = false;

        }

        private void machineBox_MouseClick(object sender, MouseEventArgs e)
        {
            multipcbox.Visible = false;

        }

        private void OUBox_MouseClick(object sender, MouseEventArgs e)
        {
            multipcbox.Visible = false;

        }

        private void domainBox_DropDownClosed(object sender, EventArgs e)
        {
            compfindBox.Focus();
        }

        private void OUBox_DropDownClosed(object sender, EventArgs e)
        {
            compfindBox.Focus();
        }

        private void machineBox_DropDownClosed(object sender, EventArgs e)
        {
            compfindBox.Focus();
        }

        private void openPwlistFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            string f = openPwlistFileDialog.FileName;
            if (f == null) f = "";
            AuditSec.settings.pws = f;
        }

        private void adminpwLabel_MouseClick(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Trying to load password reference file...");
            openPwlistFileDialog.ShowDialog();
            if (AuditSec.settings.pws.Length > 0)
                pws_loaded = AuditSecGUIForm.openPwlistFile(AuditSec.settings.pws, false);
        }

        private void rButton_Click(object sender, EventArgs e)
        {
            rPrompt(machineBigBox.Text);
        }

        private void rPrompt(string machine)
        {
            Process p = new Process();
            p.StartInfo.FileName = Environment.SystemDirectory + @"\cmd.exe";
            string psexec = Environment.SystemDirectory + @"\psexec.exe";
            if (!File.Exists(psexec))
            {
                MessageBox.Show("Error: psexec.exe not found in the sytem.\n" + psexec + "\n\nThis is a requirement for Interactive Command Prompt.",
                    machineBigBox.Text + " Interactive Command Prompt");
                return;
            }
            p.StartInfo.Arguments = "/c \"" + psexec + @" \\" + machine + " cmd – \"";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo.UseShellExecute = true;
            p.Start();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (msg.Msg == 256)
            {
                if (keyData == Keys.Escape)
                {
                    multipcbox.Visible = false;
                    compfindBox.Text = "[search]";
                    compfindBox.SelectAll();
                    compfindBox.Focus();
                    return true;
                }
                else if (keyData == Keys.F5)
                {

                    Console.WriteLine("F5 was pressed! ");
                    refreshDesapButton.PerformClick();
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
            foreach (string m in compenab.reenabled)
            {
                string description = Regex.IsMatch(m, @"^.*\((.*)\)$") ? Regex.Replace(m, @"^.*\((.*)\)$", "$1") : "";
                SearchResult r = compenab.COMPSR.Contains(m) ? (SearchResult)compenab.COMPSR[m] : null;
                COMPS.Add(m, description);
                COMPINFO.Add(m, new MachineInfo(r, USERS, false));
                machineBox.Items.Add(m);
                machineBox.Text = m;
            }
        }






        private void rkeyBox_Click(object sender, EventArgs e)
        {
            /*
            if (rkeyBox.Text.Length > 0)
                Speak("The bitlocker recovery code for computer " + machineBigBox.Text + " is:\n"
                    + (spellBitLocker(rkeyBox.Text) + ".").Replace(", .", "."), true);
             */
        }

        private void batteryButton_Click(object sender, EventArgs e)
        {
            string batt = "" + MachineInfo.getBatteryDetails(machineBigBox.Text);
            MessageBox.Show(this, batt, "Battery Details " + machineBigBox.Text);
        }

        private void printerButton_Click(object sender, EventArgs e)
        {
            List<string> items = MachineInfo.getUSBDevices(machineBigBox.Text, "USBPRINT");
            string printers = items.Count == 0 ? "None" : items.Aggregate((x, y) => x + "\n" + y);
            MessageBox.Show(this, printers, "USB Printers " + machineBigBox.Text);
        }

        private void UsrExpiredBox_CheckedChanged(object sender, EventArgs e)
        {
            if (getMachineInfo().userde == null) return;
            Cursor.Current = Cursors.WaitCursor;
            if (!UsrExpiredBox.Checked)
            {
                string disp = ("" + getMachineInfo().userde.Properties["displayName"][0]).Trim();
                bool success = UsersInfo.resetPassword(getMachineInfo().userde);
                UsrExpsoonBox.Enabled = UsrExpiredBox.Enabled = UsrExpsoonBox.Checked = UsrExpiredBox.Checked = UsersInfo.isExpiredAD(getMachineInfo().userde);
                if (!success) MessageBox.Show("Failed to change user password: " + disp, "PC Management");
                else MessageBox.Show("User password changed: " + disp, "PC Management");
            }
            Cursor.Current = Cursors.Default;
        }

        private void resetPasswordButton_Click(object sender, EventArgs e)
        {
            if (getMachineInfo().userde != null)
                UsersInfo.resetPassword(getMachineInfo().userde);
        }

        StaffDetails_GUI staffdetails = new StaffDetails_GUI(true);
        private void showStaffDetails_Click(object sender, EventArgs e)
        {
            staffdetails.Show();
        }

        private void staffDetailsTimer_Tick(object sender, EventArgs e)
        {
            Hashtable details = staffdetails.getDetails();
            if (details == null || !details.ContainsKey("Cost Center Code"))
                costBox.Text = "";
            else
                costBox.Text = "" + details["Cost Center Code"];
        }

        private void showProfilesDetails_Click(object sender, EventArgs e)
        {
            if (!profileDetailsWorker.IsBusy)
            {
                showProfileDetailsBox.Visible = true;
                profileDetailsWorker.RunWorkerAsync();
            }
        }

        MachineInfo.Files_Bytes total_cb = new MachineInfo.Files_Bytes();
        private void profileDetailsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            total_cb = new MachineInfo.Files_Bytes();
            showProfilesDetails_last = MachineInfo.GetProfilesSizeAsString(machineBigBox.Text, 100, total_cb);
        }


        string showProfilesDetails_last = "";
        private void profileDetailsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            showProfileDetailsBox.Visible = false;
            MessageBox.Show(this, showProfilesDetails_last, "Profiles Details " + machineBigBox.Text);
        }

        private void profileDetailsTimer_Tick(object sender, EventArgs e)
        {
            if (showProfileDetailsBox.Visible)
                showProfileDetailsBox.Text = "Calculating profiles sizes..."
                    + "\r\nFiles: " + total_cb.files.ToString("#,##0")
                    + "\r\nSize : " + (total_cb.bytes / (1024 * 1024)).ToString("#,##0") + "MB"
//                    + "\r\nLast Modified: " + total_cb.last.ToString("dd-MMM-yyyy")
                    ;
        }






    }
}
