using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;
using Microsoft.VisualBasic;
using System.Collections;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;

namespace AuditSec
{
    public partial class ComputerReenabler : Form
    {
        AuditSecGUIForm parent;
        DirectorySearcher COMP_SEARCH = MachineInfo.getSearcher();
        UsersInfo USERS = new UsersInfo();


        public ComputerReenabler()
        {
            this.parent = null;
            InitializeComponent();
        }

        public ComputerReenabler(AuditSecGUIForm parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void Reenabler_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (parent != null) parent.Show();
        }

        private void Reenabler_Shown(object sender, EventArgs e)
        {
            //Console.WriteLine("Computer Re-enabler shown.");
            reenabled.Clear();
            COMPSR.Clear();
        }

        string targetMachine = null;
        string targetDomain = null;
        public void setMachine(string machine)
        {
            MachineInfo mi = MachineInfo.getMachine(machine);
            if (mi == null) return;
            targetDomain = mi.dNSHostName;
            targetMachine = machine;
        }

        Hashtable MDE = new Hashtable();
        bool change = false;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (change) {
                change = false;

                MDE.Clear();
                disabledView.Items.Clear();
                Domain domain = (Domain)domainBox.SelectedItem;
                if (domain == null) return;
                if (startBox.Text.Length == 0) return;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    DirectoryEntry de = domain.GetDirectoryEntry();
                    foreach (DirectoryEntry de_ in de.Children)
                        if (de_.Properties["name"].Value.ToString().Equals(disabledBox.Text)) { de = de_; break; }

                    COMP_SEARCH.SearchRoot = de;
                    COMP_SEARCH.Filter = "(&(ObjectCategory=computer)(name=" + startBox.Text.ToUpper() + "*))";
                    Console.WriteLine("Listing machines in " + de.Name + "/" + startBox.Text.ToUpper() + "* ...");
                    foreach (SearchResult r in COMP_SEARCH.FindAll())
                    {
                        string machine = r.Properties["name"].Count > 0 ?
                            r.Properties["name"][0].ToString().ToUpper()
                            : "";
                        string description = r.Properties["description"].Count > 0 ?
                            r.Properties["description"][0].ToString().Replace("(SCCM)", "").Trim()
                            : "";
                        DirectoryEntry machinede = r.GetDirectoryEntry();
                        string m = machine + (description.Length > 0 ? " (" + description + ")" : "");
                        //Console.WriteLine(m);
                        disabledView.Items.Add(m);
                        MDE.Add(machine, machinede);
                    }
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.ToString());
                }
                Cursor.Current = Cursors.Default;
            }
        }

        private void startBox_TextChanged(object sender, EventArgs e)
        {
            //Console.WriteLine("Computer mask set to: " + startBox.Text);
            change = true;
        }

        private void disabledView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void OUBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            startBox.Text = OUBox.Text;
            whoBox.Items.Clear();
            if (domainBox.Text.Length == 0 || OUBox.Text.Length == 0)
            {
                Console.WriteLine("Listing users aborted. domain/OU=" + domainBox.Text + "/"+ OUBox.Text);

                return;
            }
            DirectoryEntry de = ((Domain)domainBox.SelectedItem).GetDirectoryEntry();
            foreach (DirectoryEntry de_ in de.Children)
                if (de_.Properties["name"].Value.ToString().Equals(OUBox.Text)) { de = de_; break; }
            Cursor.Current = Cursors.WaitCursor;
            Console.WriteLine("Listing users in " + de.Name + "...");
            USERS.Clear();
            USERS.FindAll(de);
            whoBox.Items.AddRange(USERS.getUsers());
            string me_user = UserPrincipal.Current.SamAccountName.ToUpper();
            string me_domain = UserPrincipal.Current.UserPrincipalName.Split(new []{'@', '.'})[1].ToUpper();
            string me_domainuser = me_domain + "\\" + me_user;
            if (USERS.FindOneDomainUser(me_domainuser))
            {
                whoBox.Items.Add(USERS.getDisplaynameFromUsername(me_user));
                whoBox.Text = USERS.getDisplaynameFromUsername(me_user);
            }
            Cursor.Current = Cursors.Default;
        }

        private void Reenabler_Load(object sender, EventArgs e)
        {
            //Console.WriteLine("form load");
            domainBox.Items.Clear();

            if (parent != null)
            {
                domainBox.Items.Add(parent.domainBox.SelectedItem);
                domainBox.SelectedItem = parent.domainBox.SelectedItem;
            }
            else
                try
                {
                    DomainCollection dc = Forest.GetCurrentForest().Domains;
                    Domain[] domains = new Domain[dc.Count]; dc.CopyTo(domains, 0);
                    domainBox.Items.AddRange(domains);
                    domainBox.Text = Domain.GetCurrentDomain().Name;
                }
                catch (Exception ee)
                {
                    Console.WriteLine(e.ToString());
                }
            if (targetDomain != null) domainBox.Text = targetDomain;

            OUBox.Items.Clear();
            if (parent != null)
            {
                OUBox.Items.AddRange(parent.OUBox.Items.OfType<string>().ToList().ToArray<string>());
                startBox.Text = OUBox.Text = parent.OUBox.Text;
            }
            else
            {
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
                        match = System.Text.RegularExpressions.Regex.IsMatch(ou, AuditSec.defaultOUMask,
                                 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine("OU=" + ou + " IsMatch Regex=" + AuditSec.defaultOUMask + " = " + ee.Message);
                    }
                    if (match) OUBox.Items.Add(ou);
                    //Console.WriteLine("OU=" + ou + " IsMatch Regex=" + OUMaskBox.Text + " = " + match);
                }

                string me = new DirectorySearcher("(&(ObjectClass=user)(sAMAccountName="
                    + UserPrincipal.Current.SamAccountName + "))")
                    .FindOne().GetDirectoryEntry().Properties["DistinguishedName"].Value.ToString();
                foreach (string s in me.Split(',').Reverse())
                {
                    string[] t = s.Split(new char[] { '=' }, 2);
                    if (t[0].Equals("OU")) { OUBox.Text = t[1]; break; }
                }
            }

            OUBox_SelectionChangeCommitted(sender, e);
            change = true;
            timer1_Tick(sender, e);

            if (targetMachine != null)
            {
                OUBox.SelectedItem = null; OUBox.Text = "";
                startBox.Text = targetMachine;
            }
        }

        public List<string> reenabled = new List<string>();
        public Hashtable COMPSR = new Hashtable();
        int selectedindex = -1;

        private void disabledView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            try
            {
                //Console.WriteLine("selectionchanged: " + e.ItemIndex + " " + e.IsSelected + " " + e.Item + " --- lastindex " + selectedindex); 
                if (!e.IsSelected) return;
                if (selectedindex >= 0) {
                    selectedindex = -1;
                    return;
                }

                string selected = e.Item.Text;
                selectedindex = e.ItemIndex;

                string answer = Interaction.InputBox("Are you OK to re-enable " + selected, "Computer Re-enabler", "Yes");
                if (!answer.ToUpper().Equals("YES")) return;

                Cursor.Current = Cursors.WaitCursor;


                string machine = selected.IndexOf('(') < 0 ? selected : selected.Substring(0, selected.IndexOf('(')).Trim();
                DirectoryEntry machinede = MDE.ContainsKey(machine) ? (DirectoryEntry)MDE[machine] : null;
                if (machinede == null) throw new Exception("Machine DirectoryEntry not found: '" + machine + "'");
                DirectoryEntry Workstations = null;
                string disp = whoBox.Text.Trim();
                if (disp.Length > 0)
                {
                    string user = USERS.getUsernameFromDisplayname(disp);
                    if (user == null || user.Length == 0) throw new Exception("New Owner not found: '" + disp + "'");
                    DirectoryEntry de = USERS.getDirectoryentryFromUsername(user);
                    if (de == null) throw new Exception("New Owner DirectoryEntry not found: '" + disp + "'");
                    Workstations = de.Parent.Parent.Children.Find("OU=Workstations", "organizationalUnit");
                    if (Workstations == null) throw new Exception("Workstations OU not found in " + de.Parent.Name);
                }
                else
                    try
                    {
                        Workstations = ((Domain)domainBox.SelectedItem).GetDirectoryEntry()
                            .Children.Find("OU=" + OUBox.Text, "organizationalUnit")
                            .Children.Find("OU=!Unknown-SCCM", "organizationalUnit");
                    }
                    catch (Exception ee)
                    {
                        throw new Exception("!Unknown-SCCM OU not found in " + OUBox.Text);
                    }
                if (!MachineInfo.reassign(machine, disp, machinede, Workstations))
                    throw new Exception("Failed to re-enable " + selected);
                string to = Workstations == null ? "" : Workstations.Path;
                to = Regex.Replace(to, @".*/OU=", @"");
                to = Regex.Replace(to, @",OU=", @"\");
                to = Regex.Replace(to, @",DC=.*", @"");

                disabledView.BeginUpdate();
                disabledView.Items.Remove(e.Item);
                disabledView.EndUpdate();


                COMP_SEARCH.SearchRoot = Workstations;
                COMP_SEARCH.Filter = "(&(ObjectCategory=computer)(name=" + machine + "))";
                SearchResult sr = COMP_SEARCH.FindOne();
                if (COMPSR != null) COMPSR.Add(selected, sr);
                reenabled.Add(selected);

                Cursor.Current = Cursors.Default;
                MessageBox.Show(selected + " has been re-enabled successfully."
                    + "\nLocation: " + to, "Computer Re-enabler");
            }
            catch (Exception ee)
            {
                //selected_item = null;
                Console.WriteLine(ee.ToString());
                Cursor.Current = Cursors.Default;
                MessageBox.Show(ee.Message, "Computer Re-enabler");
            }


        }


        private void disabledView_DoubleClick(object sender, EventArgs e)
        {
            int sel = disabledView.SelectedIndices.Count == 0 ? -1 : disabledView.SelectedIndices[0];
            disabledView.SelectedIndices.Clear();
        }


    }
}
