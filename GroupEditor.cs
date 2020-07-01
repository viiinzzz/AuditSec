using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;
using System.Threading;
using Microsoft.VisualBasic;
using System.DirectoryServices.AccountManagement;


namespace AuditSec
{
    public partial class GroupEditor : Form
    {
        public class Node
        {
            public TreeNode node;
            public int depth;
            public bool uptodate;
            public string GetDepthString()
            {
                StringBuilder ret = new StringBuilder();
                for (int i = 0; i < depth; i++) ret.Append("+");
                return ret.ToString();
            }
            override public string ToString()
            {
                return GetDepthString() + node.Text;
            }
        }

        public class Tree<XNode>
        {
            public GroupEditor GUI; public TreeView tree; public CheckedListBox list;
            public Label label; public Label count; public TextBox basebox, idbox; public string name;
            public Func<Tree<XNode>, bool> Refresh_; public Func<Tree<XNode>, XNode, bool> Open_;
            public int depth, depth2; public List<string> exclude;
            public Hashtable node2item = new Hashtable(), item2node = new Hashtable();

            override public string ToString()
            {
                return name;
            }
            public Tree(GroupEditor GUI, TreeView tree, CheckedListBox list,
                Label label, Label count, TextBox basebox, TextBox idbox, string name,
                Func<Tree<XNode>, bool> Refresh_, Func<Tree<XNode>, XNode, bool> Open_,
                int depth, int depth2, List<string> exclude,
                Func<Label, TextBox, TextBox, bool> Initialize_)
            {
                this.GUI = GUI;  this.tree = tree; this.list = list;
                this.label = label; this.count = count; this.basebox = basebox; this.idbox = idbox; this.name = name;
                this.Refresh_ = Refresh_; this.Open_ = Open_; this.depth = depth; this.depth2 = depth2; this.exclude = exclude;
                if (Initialize_ != null) Initialize_(label, basebox, idbox);
                Refresh();
            }
            public bool shown()
            {
                return GUI != null && GUI.Visible;
            }
            public void Initialize()
            {
                Console.WriteLine("Initializing tree " + (label != null ? label.Text.Trim() : "") + "...");
                if (shown()) GUI.clear(tree);
                if (list != null) if (shown()) GUI.clear(list);
                node2item.Clear();
                item2node.Clear();
            }
            public bool Refresh()
            {
                Initialize();
                bool ret = Refresh_(this);
                Console.WriteLine("Refresh tree " + (label != null ? label.Text.Trim() : "")
                    + ": " + (ret ? "successful" : "failed"));
                return ret;
            }

            public bool Open(XNode node)
            {
                bool ret = Open_(this, node);
                if (!ret) Console.WriteLine("Open node " + (label != null ? label.Text.Trim() : "")
                    + "/" + node.ToString()
                    + ": " + (ret ? "successful" : "failed"));
                return ret;
            }

            public List<XNode> selectNodes(Func<XNode, bool> take)
            {
                List<XNode> output = new List<XNode>();
                selectNodes(tree.Nodes, output, take);
                return output;
            }

            void selectNodes(TreeNodeCollection input, List<XNode> output, Func<XNode, bool> take)
            {
                foreach (TreeNode node in input)
                {
                    XNode xNode = (XNode)node2item[node];
                    if (take(xNode)) output.Add(xNode);
                    selectNodes(node.Nodes, output, take);
                }
            }
        }
        
        public class ADNode : Node
        {
            public DirectoryEntry de; public string Name;
            public string domain, site, siteonly, dpt;
            public bool isDomain, isSite, isDpt, invalid;

            public Tree<ADNode> tree;

            public ADNode(TreeNode node, Tree<ADNode> tree, DirectoryEntry de, int depth)
            {
                this.node = node;
                this.tree = tree;
                this.depth = depth;
                this.uptodate = false;
                Parse(de, this);
                //Console.WriteLine(this.ToString());
            }

            public string ToString()
            {
                return depth + "/" + tree.depth2 + " " + (invalid ? de.Name + " ***INVALID*** " : Name);
            }
        }

        static void Parse(DirectoryEntry de, ADNode node)
        {
            node.invalid = true;
            if (de.Name.ToUpper().StartsWith("OU=")
                || de.Name.ToUpper().StartsWith("DC="))
            {
                string path = de.Path == null ? "" : de.Path;

                path = Regex.Replace(path, "^LDAP://[A-Z.]+/", "", RegexOptions.IgnoreCase);
                path = Regex.Replace(path, "(DC=[A-Z]+),.*$", "$1", RegexOptions.IgnoreCase);
                node.isDomain = path.ToUpper().StartsWith("DC=");
                node.isSite = path.Count(c => c == ',') == 1;
                node.isDpt = path.Count(c => c == ',') == 2;
                string[] path_ = Regex.Replace(path, "[A-Z]+=", "", RegexOptions.IgnoreCase).Split(new char[] { ',' });
                if (node.isDomain)
                {
                    node.domain = path_[0].ToUpper();
                }
                else if (node.isSite)
                {
                    node.site = path_[0];
                    node.domain = path_[1].ToUpper();
                    node.siteonly = Regex.Replace(node.site, "^" + node.domain + " +", "", RegexOptions.IgnoreCase);
                }
                else if (node.isDpt)
                {
                    node.dpt = path_[0];
                    node.site = path_[1];
                    node.domain = path_[2].ToUpper();
                    node.siteonly = Regex.Replace(node.site, "^" + node.domain + " +", "", RegexOptions.IgnoreCase);
                    if (node.siteonly.Length != 3)
                    {
                        //Console.WriteLine("AD: Invalid site name: " + node.siteonly + " (name must be exactly 3 letters long)");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("AD: Unsupported directory entry: " + path + " (only domain, site, dpt are supported)");
                    return;
                }
                node.de = de;
                node.Name = de.Name.Substring(3);
                if (node.isDomain) node.Name = node.Name.ToUpper();
                node.invalid = false;
            }
        }

        public bool ADRefresh(Tree<ADNode> tree)
        {
            bool ret = true;
            if (tree.shown()) tree.GUI.update(tree.tree, true);
            if (tree.depth > 0 && tree.depth2 >= tree.depth) try
            {
                DomainCollection dc = Forest.GetCurrentForest().Domains;
                Domain[] domains = new Domain[dc.Count]; dc.CopyTo(domains, 0);
                domains.ToArray().AsParallel().ForAll(domain =>
                {
                    try
                    {
                        ADNode item = new ADNode(null, tree, domain.GetDirectoryEntry(), 1);
                            TreeNode node = new TreeNode(item.Name, 2, 3);
                            item.node = node;
                            tree.item2node[tree.node2item[node] = item] = node;
                            //Console.WriteLine(domain.GetDirectoryEntry().Name + " " + item.level + "/" + tree.depth2);


                            if (tree.shown()) tree.GUI.set(tree.count, "" + tree.node2item.Count);
                            if (tree.shown()) tree.GUI.add(tree.tree, node);
                            if (tree.shown()) tree.GUI.add(tree.list, item.Name);
                            if (!ADRefresh(tree, node, item, false)) ret = false;
                    }
                    catch (Exception e)
                    {
                        ret = false;
                        Console.WriteLine("AD Error: Domain " + domain + ": " + e.ToString());
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("AD Error: " + e.ToString());
                ret = false;
            }
            if (tree.shown()) tree.GUI.sort(tree.tree);
            if (tree.shown()) foreach (TreeNode node in tree.tree.Nodes) tree.GUI.expand(node, false);
            if (tree.shown()) tree.GUI.update(tree.tree, false);
            return ret;
        }

        bool ADRefresh(Tree<ADNode> tree, TreeNode node, ADNode item, bool expand)
        {
            bool ret = true;
            if (item.depth < (expand ? tree.depth2 : tree.depth))
            {
                List<DirectoryEntry> lde2 = new List<DirectoryEntry>();
                DirectoryEntries children = null;
                children = item.de.Children;
                if (children != null) foreach (DirectoryEntry de2_ in children) lde2.Add(de2_);
                DirectoryEntry[] ade2 = new DirectoryEntry[lde2.Count]; lde2.CopyTo(ade2, 0);
                ade2.AsParallel().WithDegreeOfParallelism(expand ? 1 : 8).ForAll(de2 =>
                {
                    try
                    {
                        ADNode item2 = new ADNode(null, tree, de2, item.depth + 1);
                        if (!item2.invalid)
                        {
                            TreeNode node2 = new TreeNode(item2.Name, 0, 1);
                            item2.node = node2;
                            if (item2.isDomain
                                || (item2.isSite && item2.siteonly.Length == 3)
                                || (item2.isDpt && take(item2.dpt, tree.exclude))
                                )
                            {
                                tree.item2node[tree.node2item[node2] = item2] = node2;
                                if (tree.shown()) tree.GUI.set(tree.count, "" + tree.node2item.Count);
                                if (tree.shown()) tree.GUI.add(node, node2);
                                //Console.WriteLine(level + "+OU=" + item.Name + " (" + item2.de.Path + ")");
                            }
                            if (!expand &&!ADRefresh(tree, node2, item2, expand)) ret = false;
                                
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("AD Error: " + de2.Name + ": " + e.ToString());
                        ret = false;
                    }
                });
                item.uptodate = true;
            }
            return ret;
        }

        bool ADOpen(Tree<ADNode> tree, ADNode item)
        {
            bool ret = item.uptodate || ADRefresh(tree, item.node, item, true);
            return ret;
        }

        bool take(string name, List<string> exclude)
        {
            foreach(string pattern in exclude)
                if (Regex.IsMatch(name, pattern))
                {
                    //Console.WriteLine("Exclude: " + name + "(match " + pattern + ")"); 
                    return false;
                }
            return true;
        }

        Tree<ADNode> AD_TREE;
        bool REFRESH_AT_STARTUP = true;

        public GroupEditor()
        {
            InitializeComponent();
            searchRootBox.Text = Forest.GetCurrentForest().RootDomain.GetDirectoryEntry().Path;
            membersBox.Items.Clear();
        }



        void set(Control control, string text)
        {
            Invoke(new setControlDelegate(setControl), control, text);
        }
        public delegate void setControlDelegate(Control control, string text);
        void setControl(Control control, string text)
        {
            if (control is TextBox) ((TextBox)control).Text = text;
            else if (control is Label) ((Label)control).Text = text;
            else if (control is Button) ((Button)control).Text = text;
            else if (control is Form) ((Form)control).Text = text;
        }

        void begin(object control, bool value)
        {
            Invoke(new beginUpdateDelegate(beginUpdate), control, value);
        }
        public delegate void beginUpdateDelegate(object control, bool value);
        void beginUpdate(object control, bool value)
        {
            if (control is CheckedListBox)
            {
                if (value)
                    ((CheckedListBox)control).BeginUpdate();
                else ((CheckedListBox)control).EndUpdate();
            }
        }

        void set(Control control, Bitmap icon)
        {
            Invoke(new setIconDelegate(setIcon), control, icon);
        }
        public delegate void setIconDelegate(Control control,Bitmap icon);
        void setIcon(Control control, Bitmap icon)
        {
            if (control is Label) ((Label)control).Image = icon;
            else if (control is Button) ((Button)control).Image = icon;
        }


        void add(object control, object value)
        {
            Invoke(new addItemDelegate(addItem), control, value);
        }
        public delegate void addItemDelegate(object control, object value);
        void addItem(object control, object value)
        {
            if (control is TreeNode) ((TreeNode)control).Nodes.Add((TreeNode)value);
            else if (control is TreeNodeCollection) ((TreeNodeCollection)control).Add((TreeNode)value);
            else if (control is TreeView) ((TreeView)control).Nodes.Add((TreeNode)value);

            else if (control is DataGridView) ((DataGridView)control).Rows.Add((object[])value);

            else if (control is CheckedListBox.ObjectCollection)
            {
                if (!((CheckedListBox.ObjectCollection)control).Contains((string)value))
                    ((CheckedListBox.ObjectCollection)control).Add((string)value);
            }
            else if (control is CheckedListBox)
            {
                if (!((CheckedListBox)control).Items.Contains((string)value))
                    ((CheckedListBox)control).Items.Add((string)value);
            }
        }

        void remove(object control, object value)
        {
            Invoke(new addItemDelegate(removeItem), control, value);
        }
        public delegate void removeItemDelegate(object control, object value);
        void removeItem(object control, object value)
        {
            if (control is TreeNode) ((TreeNode)control).Nodes.Remove((TreeNode)value);
            else if (control is TreeNodeCollection) ((TreeNodeCollection)control).Remove((TreeNode)value);
            else if (control is TreeView) ((TreeView)control).Nodes.Remove((TreeNode)value);

            else if (control is CheckedListBox.ObjectCollection)
            {
                if (((CheckedListBox.ObjectCollection)control).Contains((string)value))
                    ((CheckedListBox.ObjectCollection)control).Remove((string)value);
            }
            else if (control is CheckedListBox)
            {
                if (((CheckedListBox)control).Items.Contains((string)value))
                    ((CheckedListBox)control).Items.Remove((string)value);
            }
        }

        void clear(object collection)
        {
            Invoke(new clearCollectionDelegate(clearCollection), collection);
        }
        public delegate void clearCollectionDelegate(object collection);
        void clearCollection(object collection)
        {
            if (collection is TreeNodeCollection) ((TreeNodeCollection)collection).Clear();
            else if (collection is TreeView) ((TreeView)collection).Nodes.Clear();
            else if (collection is CheckedListBox.ObjectCollection) ((CheckedListBox.ObjectCollection)collection).Clear();
            else if (collection is CheckedListBox) ((CheckedListBox)collection).Items.Clear();
        }

        void update(Control control, bool begin)
        {
            Invoke(new updateControlDelegate(updateControl), control, begin);
        }
        public delegate void updateControlDelegate(Control control, bool begin);
        void updateControl(Control control, bool begin)
        {
                if (begin)
                {
                    control.Visible = false;
                    DisabledControls.Add(control);
                    if (control is TreeView) ((TreeView)control).BeginUpdate();
                    else if (control is CheckedListBox) ((CheckedListBox)control).BeginUpdate();
                }
                else
                {
                    if (control is TreeView) ((TreeView)control).EndUpdate();
                    else if (control is CheckedListBox) ((CheckedListBox)control).EndUpdate();
                    DisabledControls.Remove(control); ControlsToEnable.Add(control);
                    if (DisabledControls.Count == 0)
                    {
                        foreach (Control c in ControlsToEnable) c.Visible = true;
                    }
                    if (control is TreeView && ((TreeView)control).Nodes.Count > 0)
                    {
                        ((TreeView)control).SelectedNode = ((TreeView)control).Nodes[0];
                        ((TreeView)control).Nodes[0].EnsureVisible();
                    }
                }
        }
        List<Control> DisabledControls = new List<Control>();
        List<Control> ControlsToEnable = new List<Control>();

        void enable(Control control, bool enable)
        {
            Invoke(new enableControlDelegate(enableControl), control, enable);
        }
        public delegate void enableControlDelegate(Control control, bool enable);
        void enableControl(Control control, bool enable)
        {
            control.Enabled = enable;
        }

        void sort(object collection)
        {
            Invoke(new sortCollectionDelegate(sortCollection), collection);
        }
        public delegate void sortCollectionDelegate(object collection);
        void sortCollection(object collection)
        {
            if (collection is TreeView) ((TreeView)collection).Sort();
            else if (collection is CheckedListBox) ((CheckedListBox)collection).Sorted = true;
        }

        void expand(TreeNode node, bool all)
        {
            Invoke(new expandNodeDelegate(expandNode), node, all);
        }
        public delegate void expandNodeDelegate(TreeNode node, bool all);
        void expandNode(TreeNode node, bool all)
        {
            if (all) node.ExpandAll();
            else node.Expand();
        }

        TreeNode find(TreeNodeCollection col, string fullpath)
        {
            foreach (TreeNode node in col)
            {
                if (fullpath.Equals(node.FullPath)) return node;
                TreeNode ret = find(node.Nodes, fullpath);
                if (ret != null) return ret;
            }
            return null;
        }

        private void ADTreeRefresh_Click(object sender, EventArgs e)
        {
            ADTreeRefresh_Do(false);
        }

        static List<string> ADExclude = new List<string>(new string[] {
/*                    "(?i)Printers",
                    "(?i)Groups",
                    "(?i)Servers",
                    "(?i)System-Accounts",
                    "(?i)Users"*/
                });

        Thread ADTreeRefresh_Do(bool full)
        {
            Thread ret = new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(500);
                
                AD_TREE = new Tree<ADNode>(
                    this, ADTree, null,
                    ADLabel, ADCount, null, ForestBox, ADLabel.Text.Trim(),
                    ADRefresh, ADOpen, full ? 3 : 1, 3, ADExclude,
                    (label, basebox, idbox) =>
                    {
                        set(idbox, Forest.GetCurrentForest().Name); return true;
                    });
            }));
            ret.Start();
            return ret;
        }


        private void MachineChooser_Shown(object sender, EventArgs e)
        {
            ADTree.SelectedNode = null;
            membersBox.Items.Clear();
            if (REFRESH_AT_STARTUP) new Thread(new ThreadStart(delegate
            {
                ADTreeRefresh_Click(null, null);
            })).Start();
            progressWorker.RunWorkerAsync();
        }






        List<TreeNode> getSubNodes(TreeNode node, List<TreeNode> nodes)
        {
            if (nodes == null) nodes = new List<TreeNode>();
            nodes.Add(node);
            foreach (TreeNode node2 in node.Nodes) getSubNodes(node2, nodes);
            return nodes;
        }


        void SetItemsChecked(CheckedListBox box, bool check)
        {
            for (int i = 0; i < box.Items.Count; i++)
                box.SetItemChecked(i, check);
        }

        void SetItemChecked(CheckedListBox box, string item, bool check)
        {
            for (int i = 0; i < box.Items.Count; i++)
                if (item == box.Items[i].ToString())
                    box.SetItemChecked(i, check);
        }

      







        Bitmap[] GlassIcons = new Bitmap[] {
            global::AuditSec.Properties.Resources.Glass,
            global::AuditSec.Properties.Resources.Glass1,
            global::AuditSec.Properties.Resources.Glass2,
            global::AuditSec.Properties.Resources.Glass3
        };
        int Glass_i = 0;

        private void searcher_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            bool begin = e.ProgressPercentage == 99;
            bool end = e.ProgressPercentage == 100;
            if (begin) enableControls(false);
            else if (end) enableControls(true);
            else
            {
                bool add = e.ProgressPercentage == 1;
                string item = e.UserState.ToString();

                if (add) membersBox.Items.Add(item);
                else membersBox.Items.Remove(item);


                int pccount = membersBox.Items.Cast<String>().Where(x => !x.Contains("=")).Count();
                set(membersCount, "" + pccount);
                set(searchButton, GlassIcons[Glass_i = (Glass_i + 1) % GlassIcons.Length]);
            }
        }

        void enableControls(bool Enabled)
        {
            enable(ADTree, Enabled);
            enable(ADTreeRefresh, Enabled);
            enable(searchButton, Enabled);
            set(searchButton, GlassIcons[0]);
            enable(applyButton, Enabled);
        }






        private void ADTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!ADTree.Visible) return;
            if (e.Node.IsSelected)
            {
                string patha = e.Node.FullPath;

                ADNode adNode = AD_TREE == null ? null : (ADNode)AD_TREE.node2item[e.Node];
                if (adNode != null)
                {
                    AD_TREE.Open(adNode);
                    e.Node.Expand();


                    searchRootBox.Text = adNode.de.Path;
                    groupsBox.Items.Clear();
                    groupsLabel.Visible = false;
                    groupName2Ldap.Clear();
                    membersBox.Items.Clear();
                    searchScopeBox.Checked = false;
                    searchButton.PerformClick();
                }

            }
        }

        Hashtable groupName2Ldap = new Hashtable();

        private void SearchButton_Click(object sender, EventArgs e)
        {
            groupsBox.Items.Clear();
            groupsLabel.Visible = false;
            membersBox.Items.Clear();
            groupBox.Text = "";
            groupName2Ldap.Clear();
            if (searchFilterBox.Text.Length == 0) return;

            searchFilterBox.Enabled = false;
            searchScopeBox.Checked = !searchFilterBox.Text.Contains("*");
            DirectorySearcher GROUP_SEARCH;
            GROUP_SEARCH = new DirectorySearcher();
            GROUP_SEARCH.SearchRoot = new DirectoryEntry(searchRootBox.Text);
            GROUP_SEARCH.Filter = "(&(ObjectClass=group)(cn=" + searchFilterBox.Text + "))";
            GROUP_SEARCH.SearchScope = searchScopeBox.Checked ? SearchScope.Subtree : SearchScope.OneLevel;
            GROUP_SEARCH.PropertiesToLoad.Add("name");


            if (searchRootBox.Text == Forest.GetCurrentForest().RootDomain.GetDirectoryEntry().Path)
                foreach (Domain d in Forest.GetCurrentForest().Domains)
                {
                    GROUP_SEARCH.SearchRoot = d.GetDirectoryEntry();
                    Console.WriteLine("Searching group \"" + searchFilterBox.Text + "\" in " + GROUP_SEARCH.SearchRoot.Path + "...");
                    foreach (SearchResult r in GROUP_SEARCH.FindAll())
                    {
                        string name = "" + r.Properties["name"][0];
                        groupsBox.Items.Add(name);
                        groupName2Ldap[name] = r.GetDirectoryEntry();
                    }
                }
            else
            {
                Console.WriteLine("Searching group \"" + searchFilterBox.Text + "\" in " + GROUP_SEARCH.SearchRoot.Path + "...");
                foreach (SearchResult r in GROUP_SEARCH.FindAll())
                {
                    string name = "" + r.Properties["name"][0];
                    groupsBox.Items.Add(name);
                    groupName2Ldap[name] = r.GetDirectoryEntry();
                }
            }
            if (groupsBox.Items.Count == 0)
                searchFilterBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            else
                groupsLabel.Visible = true;

            Console.WriteLine("found " + groupsBox.Items.Count);
            searchFilterBox.Enabled = true;
        }


        private void groupsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            membersBox.Items.Clear();
            groupsBox.Items.Clear();
            groupsLabel.Visible = false;
            groupBox.Text = "";
            usersPanel.Visible = membersPanel.Visible = true;
            addButton.Visible = removeButton.Visible = true;
            applyButton.Visible = false; applyButton.Enabled = true;
            applyButton.BackColor = System.Drawing.SystemColors.Control;
            cancelButton.Text = "Quit";

            string name = groupBox.Text = "" + curGroupsBox;
            DirectoryEntry group = (DirectoryEntry)groupName2Ldap[name];
            if (group == null) return;

            foreach (var m in group.Properties["member"])
            {
                DirectoryEntry user = new DirectoryEntry("LDAP://" + m);
                string principal = "" + user.Properties["userPrincipalName"][0];
                string domainuser = Regex.Replace(principal, @"(\w+)@(\w+)[.].*", "$2\\$1");
                membersBox.Items.Add(domainuser);
            }
                
        }

        string curGroupsBox = null;
        private void groupsBox_MouseMove(object sender, MouseEventArgs e)
        {
            int i = groupsBox.IndexFromPoint(e.Location);
            curGroupsBox = i < 0 ? null : groupsBox.Items[i].ToString();
        }


        //remove member
        bool CANCEL = false;

        private void button2_Click(object sender, EventArgs e)
        {
            string name = groupBox.Text;
            if (name.Length == 0) return;
            AddUserBox.Enabled = false;
            removeButton.Enabled = false;
            addButton.Visible = false;
            cancelButton.Text = "Cancel";
            CANCEL = false;

            DirectoryEntry group = (DirectoryEntry)groupName2Ldap[name];
            PropertyValueCollection members = group.Properties["member"];

            List<string> domainusers = new List<string>();
            foreach (string x in membersBox.SelectedItems)
                domainusers.Add(x);

            List<string> success = new List<string>();
            List<string> failed = new List<string>();
            string forest = "." + Forest.GetCurrentForest().RootDomain.Name;

            progress_i = 0; progressTotal = domainusers.Count;
            domainusers.AsParallel().ForAll(domainuser => 
            {
                progress_i++;

                if (CANCEL)
                    failed.Add(domainuser);
                else
                {
                    string principal = Regex.Replace(domainuser, @"(\w+)\\(\w+)", "$2@$1" + forest);
                    DirectorySearcher USER_SEARCH = new DirectorySearcher();
                    USER_SEARCH.SearchScope = SearchScope.Subtree;
                    USER_SEARCH.Filter = "(&(ObjectClass=user)(!ObjectClass=computer)(userPrincipalName=" + principal + "))";
                    SearchResult r = null; foreach (Domain d in Forest.GetCurrentForest().Domains)
                    {
                        USER_SEARCH.SearchRoot = d.GetDirectoryEntry();
                        if ((r = USER_SEARCH.FindOne()) != null) break;
                    }

                    if (r != null)
                    {
                        try
                        {
                            string dn = Regex.Replace(r.GetDirectoryEntry().Path, "LDAP://[^/]*/(.*)", "$1");
                            progressString = domainuser;

                            Console.WriteLine("Removing... " + progress_i + "/" + progressTotal + "  " + domainuser);
                            lock (members) { members.Remove(dn); }
                            success.Add(domainuser);
                        }
                        catch (Exception ee)
                        {
                            failed.Add(domainuser);
                            Console.WriteLine("Failed removing " + domainuser + ". " + ee.Message);
                        }
                    }
                    else
                    {
                        failed.Add(domainuser);
                        Console.WriteLine("Failed removing " + domainuser + ". Not found");
                    }
                }
            });


            if (failed.Count > 0)
                MessageBox.Show("Could not remove users:\n\n" + failed.Aggregate((x, y) => x + "; " + y));
            foreach(string x in success)
                membersBox.Items.Remove(x);
            if (success.Count > 0)
            {
                applyButton.BackColor = System.Drawing.SystemColors.Control;
                applyButton.Visible = true; cancelButton.Text = "Cancel";
                addButton.Visible = removeButton.Visible = false;
            }
            else
            {
                addButton.Visible = removeButton.Visible = true;
            }
            AddUserBox.Text = success.Count == 0 ? "" : success.Aggregate((x, y) => x + "; " + y);
            progressString = "";
            AddUserBox.Enabled = true;
            removeButton.Enabled = true;
        }


        //add new member

        private void button1_Click(object sender, EventArgs e)
        {
            string name = groupBox.Text;
            if (name.Length == 0) return;
            AddUserBox.Enabled = false;
            addButton.Enabled = false;
            removeButton.Visible = false;
            cancelButton.Text = "Cancel";
            CANCEL = false;

            DirectoryEntry group = (DirectoryEntry)groupName2Ldap[name];
            PropertyValueCollection members = group.Properties["member"];

            string forest = "." + Forest.GetCurrentForest().RootDomain.Name;
            List<string> principals = AddUserBox.Text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                .Select(domainuser => Regex.Replace(domainuser, @"(\w+)\\(\w+)", "$2@$1" + forest)).Where(x => x.Length > 0).ToList();

            List<string> success = new List<string>();
            List<string> failed = new List<string>();

            progress_i = 0; progressTotal = principals.Count;
            principals.AsParallel().ForAll(principal =>
            {
                progress_i++;
                string domainuser = Regex.Replace(principal, @"(\w+)@(\w+)[.].*", "$2\\$1");

                if (CANCEL)
                    failed.Add(domainuser);
                else if (membersBox.Items.Cast<String>().Select(du => du.ToUpper()).Contains(domainuser.ToUpper()))
                    success.Add(domainuser);
                else
                {
                    DirectorySearcher USER_SEARCH = new DirectorySearcher();
                    USER_SEARCH.SearchScope = SearchScope.Subtree;
                    USER_SEARCH.Filter = "(&(ObjectClass=user)(!ObjectClass=computer)(userPrincipalName=" + principal + "))";

                    SearchResult r = null; foreach (Domain d in Forest.GetCurrentForest().Domains)
                    {
                        USER_SEARCH.SearchRoot = d.GetDirectoryEntry();
                        if ((r = USER_SEARCH.FindOne()) != null) break;
                    }

                    if (r != null)
                    {
                        try
                        {
                            string dn = Regex.Replace(r.GetDirectoryEntry().Path, "LDAP://[^/]*/(.*)", "$1");
                            progressString = domainuser;
                            Console.WriteLine("Adding... " + progress_i + "/" + progressTotal + "  " + domainuser);
                            lock (members) { members.Add(dn); }
                            success.Add(domainuser);
                        }
                        catch (Exception ee)
                        {
                            failed.Add(domainuser);
                            Console.WriteLine("Failed adding " + domainuser + ". " + ee.Message);
                        }
                    }
                    else
                    {
                        failed.Add(domainuser);
                        Console.WriteLine("Failed adding " + domainuser + ". Not found");
                    }
                }
            });

            if (failed.Count > 0)
                MessageBox.Show("Could not add users:\n\n" + failed.Aggregate((x, y) => x + "; " + y));
            foreach (string domainuser in success)
                if (!membersBox.Items.Cast<String>().Select(du => du.ToUpper()).Contains(domainuser.ToUpper()))
                membersBox.Items.Add(domainuser);
            if (success.Count > 0)
            {
                applyButton.BackColor = System.Drawing.SystemColors.Control;
                applyButton.Visible = true; cancelButton.Text = "Cancel";
                addButton.Visible = removeButton.Visible = false;
            }
            else
            {
                addButton.Visible = removeButton.Visible = true;
            }
            AddUserBox.Text = failed.Count == 0 ? "" : failed.Aggregate((x, y) => x + "; " + y);
            progressString = "";
            AddUserBox.Enabled = true;
            addButton.Enabled = true;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            try
            {
                string name = groupBox.Text;
                if (name.Length == 0) return;
                DirectoryEntry group = (DirectoryEntry)groupName2Ldap[name];
                if (group != null)
                    group.CommitChanges();
                applyButton.BackColor = System.Drawing.SystemColors.Control;
                applyButton.Visible = false;
                cancelButton.Text = "Quit";
                addButton.Visible = removeButton.Visible = true;
            }
            catch (Exception ee)
            {
                applyButton.Enabled = false;
                addButton.Visible = removeButton.Visible = false;
                applyButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                cancelButton.Text = "Quit";
                Console.WriteLine("Could not commit changes:\n\n" + ee.ToString());
                MessageBox.Show("Could not commit changes:\n\n" + ee.Message);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (applyButton.Visible)
                CANCEL = true;
            if (!applyButton.Visible || !applyButton.Enabled)
            {
                AddUserBox.Text = "";
                usersPanel.Visible = membersPanel.Visible = false;

                groupBox.Text = "";
                groupsBox.Items.Clear();
                groupsLabel.Visible = false;
                membersBox.Items.Clear();

                searchFilterBox.Text = "*";
                groupName2Ldap.Clear();
            }
        }

        int searchwait = 0;
        string searched = "*";
        private void searchFilterBox_TextChanged(object sender, EventArgs e)
        {
            searchwait = 0;
            searchFilterBox.BackColor = System.Drawing.SystemColors.Window;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (searchFilterBox.Text != searched && searchwait++ > 10)
            {
//                Console.WriteLine("Search field change.");
                searched = searchFilterBox.Text;
                searchButton.PerformClick();
                if (groupsBox.Items.Count == 1 && !searchFilterBox.Text.Contains("*"))
                {
//                    Console.WriteLine("Select only one group.");
                    curGroupsBox = groupsBox.Items[0].ToString();
                    groupsBox.SelectedIndex = 0;
                }
            }

        }

        private void searchFilterBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            groupBox.Text = "";
            groupsBox.Items.Clear();
            groupsLabel.Visible = false;
            membersBox.Items.Clear();
            searchScopeBox.Checked = !searchFilterBox.Text.Contains("*");
            if (e.KeyChar == '\r' && searchFilterBox.Text != searched)
            {
//                Console.WriteLine("CR in Search field.");
                searched = searchFilterBox.Text;
                searchButton.PerformClick();
                if (groupsBox.Items.Count == 1 && !searchFilterBox.Text.Contains("*"))
                {
//                    Console.WriteLine("Select only one group.");
                    curGroupsBox = groupsBox.Items[0].ToString();
                    groupsBox.SelectedIndex = 0;
                }
            }
        }

        string progressString = "";
        int progressTotal = 1;
        int progress_i = 0;
        private void progressWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("progressWorker begin");
            while (!progressWorker.CancellationPending)
            {
                Thread.Sleep(500);
                if (this.Visible)
                    progressWorker.ReportProgress(progress_i * 100 / progressTotal, progressString);
            }
            Console.WriteLine("progressWorker end");
        }

        private void progressWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressLabel.Text = e.UserState.ToString().Length == 0 ? "" : e.ProgressPercentage + "%" + e.UserState.ToString();
            //progressLabel.Text = e.ProgressPercentage + "%  " + e.UserState.ToString();
        }

        private void GroupEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            progressWorker.CancelAsync();
        }



    }
}
