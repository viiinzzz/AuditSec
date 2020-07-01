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


namespace AuditSec
{
    public partial class ComputerChooser : Form
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
            public ComputerChooser GUI; public TreeView tree; public CheckedListBox list;
            public Label label; public Label count; public TextBox basebox, idbox; public string name;
            public Func<Tree<XNode>, bool> Refresh_; public Func<Tree<XNode>, XNode, bool> Open_;
            public int depth, depth2; public List<string> exclude;
            public Hashtable node2item = new Hashtable(), item2node = new Hashtable();

            override public string ToString()
            {
                return name;
            }
            public Tree(ComputerChooser GUI, TreeView tree, CheckedListBox list,
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
                                if (item2.isSite) if (tree.shown()) tree.GUI.add(SitesBox, item2.site);
                                if (item2.isDpt) if (tree.shown()) tree.GUI.add(DptsBox, item2.dpt);
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

        public ComputerChooser()
        {
            InitializeComponent();
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
            /*if (control is TreeNode) ((TreeNode)control).Nodes.Add((TreeNode)value);
            else if (control is TreeNodeCollection) ((TreeNodeCollection)control).Add((TreeNode)value);
            else if (control is TreeView) ((TreeView)control).Nodes.Add((TreeNode)value);

            else if (control is DataGridView) ((DataGridView)control).Rows.Add((object[])value);

            else if (control is CheckedListBox.ObjectCollection)
            {
                if (!((CheckedListBox.ObjectCollection)control).Contains((string)value))
                    ((CheckedListBox.ObjectCollection)control).Add((string)value);
            }
            else */if (control is CheckedListBox)
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

            //else if (control is DataGridView) ((DataGridView)control).Rows.Remove((object[])value);

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
                    DomainsBox.Enabled = DptsBox.Enabled = DptsBox.Enabled = control.Visible = false;
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
                        DomainsBox.Enabled = DptsBox.Enabled = DptsBox.Enabled = true;
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
                    "(?i)Printers",
                    "(?i)Groups",
                    "(?i)Servers",
                    "(?i)System-Accounts",
                    "(?i)Users"
                });

        Thread ADTreeRefresh_Do(bool full)
        {
            Thread ret = new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(500);
                
                AD_TREE = new Tree<ADNode>(
                    this, ADTree, DomainsBox,
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
            SetItemsChecked(DomainsBox, false);
            SetItemsChecked(SitesBox, false);
            SetItemsChecked(DptsBox, false);
            MachinesBox.Items.Clear();
            if (REFRESH_AT_STARTUP) new Thread(new ThreadStart(delegate
            {
                ADTreeRefresh_Click(null, null);
            })).Start();
        }



        bool DIALOG_RESULT = false;
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (searcher.IsBusy) searcher.CancelAsync();
            else
            {
                DIALOG_RESULT = false;
                ADTree.SelectedNode = null;
                SetItemsChecked(DomainsBox, false);
                SetItemsChecked(SitesBox, false);
                SetItemsChecked(DptsBox, false);
                MachinesBox.Items.Clear();
                Close();
            }
        }


        public List<string> getSelectedMachines()
        {
            return DIALOG_RESULT ? MachinesBox.Items.Cast<String>()
                .Where(x => !x.Contains("=")).Select(x => x.IndexOf(" ") >= 0 ? x.Substring(0, x.IndexOf(" ")) : x ).ToList()
                : new List<string>();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (MachinesBox.Items.Cast<String>().Where(x => !x.Contains("(") && x.Contains("=")).Count() > 0)
            {
                if (!searcher.IsBusy) searcher.RunWorkerAsync();
            }
            else
            {
                DIALOG_RESULT = true;
                Close();
            }
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

        private void ADTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!ADTree.Visible) return;
            if (e.Node.IsSelected)
            {
                SetItemsChecked(DomainsBox, false);
                SetItemsChecked(SitesBox, false);
                SetItemsChecked(DptsBox, false);

                string patha = e.Node.FullPath;

                ADNode adNode = AD_TREE == null ? null : (ADNode)AD_TREE.node2item[e.Node];
                if (adNode != null)
                {
                    AD_TREE.Open(adNode);
                    e.Node.Expand();

                    if (adNode.isDomain)
                    {
                        SetItemChecked(DomainsBox, adNode.domain, true);
                    }
                    else if (adNode.isSite)
                    {
                        SetItemChecked(DomainsBox, adNode.domain, true);
                        SetItemChecked(SitesBox, adNode.site, true);
                    }
                    else if (adNode.isDpt)
                    {
                        SetItemChecked(DomainsBox, adNode.domain, true);
                        SetItemChecked(SitesBox, adNode.site, true);
                        SetItemChecked(DptsBox, adNode.dpt, true);
                    }

                    e.Node.Expand();
                }

                calcSelectedMachines(-1, 0, false);
            }
        }

        string curADDptBox = null;

        private void ADDptBox_MouseMove(object sender, MouseEventArgs e)
        {
            int i = DptsBox.IndexFromPoint(e.Location);
            curADDptBox = i < 0 ? null : DptsBox.Items[i].ToString();
        }

        string curDomainsBox = null;
        private void DomainsBox_MouseMove(object sender, MouseEventArgs e)
        {
            int i = DomainsBox.IndexFromPoint(e.Location);
            curDomainsBox = i < 0 ? null : DomainsBox.Items[i].ToString();
        }

        string curSiteBox = null;
        private void SiteBox_MouseMove(object sender, MouseEventArgs e)
        {
            int i = SitesBox.IndexFromPoint(e.Location);
            curSiteBox = i < 0 ? null : SitesBox.Items[i].ToString();
        }

      


        void calcSelectedMachines(int boxn, int index, bool check)
        {
            string forest = ("." + ForestBox.Text).Replace(".", ",DC=");
            MachinesBox.Items.Clear();
            List<string> SelectedDomains = (DomainsBox.CheckedItems.Count == 0 && !(boxn == 0 && check))
                || (DomainsBox.CheckedItems.Count == 1 && (boxn == 0 && !check))
                ? DomainsBox.Items.Cast<String>().ToList()
                : DomainsBox.CheckedItems.Cast<String>().ToList();
            List<string> SelectedSites = (SitesBox.CheckedItems.Count == 0 && !(boxn == 1 && check))
                || (SitesBox.CheckedItems.Count == 1 && (boxn == 1 && !check))
                ? SitesBox.Items.Cast<String>().ToList()
                : SitesBox.CheckedItems.Cast<String>().ToList();
            List<string> SelectedDpts = (DptsBox.CheckedItems.Count == 0 && !(boxn == 2 && check))
                || (DptsBox.CheckedItems.Count == 1 && (boxn == 2 && !check))
                ? DptsBox.Items.Cast<String>().ToList()
                : DptsBox.CheckedItems.Cast<String>().ToList();
            if (boxn == 0 && check) SelectedDomains.Add(DomainsBox.Items[index].ToString());
            else if (boxn == 0 && !check && DomainsBox.CheckedItems.Count != 1) SelectedDomains.Remove(DomainsBox.Items[index].ToString());
            else if (boxn == 1 && check) SelectedSites.Add(SitesBox.Items[index].ToString());
            else if (boxn == 1 && !check && SitesBox.CheckedItems.Count != 1) SelectedSites.Remove(SitesBox.Items[index].ToString());
            else if (boxn == 2 && check) SelectedDpts.Add(DptsBox.Items[index].ToString());
            else if (boxn == 2 && !check && DptsBox.CheckedItems.Count != 1) SelectedDpts.Remove(DptsBox.Items[index].ToString());

            if (SelectedDomains.Count == DomainsBox.Items.Count) SelectedDomains.Clear();
            //if (SelectedSites.Count == SitesBox.Items.Count) SelectedSites.Clear();
            if (SelectedDpts.Count == DptsBox.Items.Count) SelectedDpts.Clear();

            Console.WriteLine("Selected Domains: " + (SelectedDomains.Count > 0 ? SelectedDomains.Aggregate((x, y) => x + ", " + y) : ""));
            Console.WriteLine("Selected Sites: " + (SelectedSites.Count > 0 ? SelectedSites.Aggregate((x, y) => x + ", " + y) : ""));
            Console.WriteLine("Selected Dpts: " + (SelectedDpts.Count > 0 ? SelectedDpts.Aggregate((x, y) => x + ", " + y) : ""));
            foreach (string domain in SelectedDomains)
            {
                if (SelectedSites.Count == 0)
                    MachinesBox.Items.Add("DC=" + domain + forest);
                else foreach (string site in SelectedSites)
                    {
                        if (SelectedDpts.Count == 0)
                            MachinesBox.Items.Add("OU=" + site + ",DC=" + domain + forest);
                        else foreach (string dpt in SelectedDpts)
                            {
                                MachinesBox.Items.Add("OU=" + dpt + ",OU=" + site + ",DC=" + domain + forest);
                            }
                    }
            }
        }

        private void DomainsBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ADTree.SelectedNode = null;
            calcSelectedMachines(0, e.Index, e.NewValue == CheckState.Checked);
        }

        private void SitesBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ADTree.SelectedNode = null;
            calcSelectedMachines(1, e.Index, e.NewValue == CheckState.Checked);
        }

        private void DptsBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ADTree.SelectedNode = null;
            calcSelectedMachines(2, e.Index, e.NewValue == CheckState.Checked);
        }


        static public DirectorySearcher getSearcher(DirectoryEntry root)
        {
            DirectorySearcher COMP_SEARCH;
            COMP_SEARCH = new DirectorySearcher();
            if (root != null)
                COMP_SEARCH.SearchRoot = root;
            COMP_SEARCH.Filter = "(&ObjectCategory=computer)";
            COMP_SEARCH.SearchScope = SearchScope.Subtree;
            COMP_SEARCH.PropertiesToLoad.Add("name");
            COMP_SEARCH.PropertiesToLoad.Add("description");
            COMP_SEARCH.PropertiesToLoad.Add("dNSHostName");
            COMP_SEARCH.PropertiesToLoad.Add("operatingSystem");
            COMP_SEARCH.PropertiesToLoad.Add("operatingSystemVersion");
            COMP_SEARCH.PropertiesToLoad.Add("operatingSystemServicePack");
            COMP_SEARCH.PropertiesToLoad.Add("whenCreated");
            COMP_SEARCH.PropertiesToLoad.Add("whenChanged");
            return COMP_SEARCH;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!searcher.IsBusy) searcher.RunWorkerAsync();
        }

        private void searcher_DoWork(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("\n\nSearching machines...");
            searcher.ReportProgress(99);
            begin(MachinesBox, true);
            MachinesBox.Items.Cast<String>().Where(x => !x.Contains("(") && x.Contains("=")).ToArray()
                .AsParallel().ForAll(ldap =>
            {
                if (!searcher.CancellationPending) try
                {
                    Console.WriteLine("\nFind PCs in " + ldap + "...");
                    //remove(MachinesBox.Items, ldap);
                    searcher.ReportProgress(0, ldap);
                    //List<string> found = new List<string>();
                    DirectorySearcher ds = getSearcher(new DirectoryEntry("LDAP://" + ldap));
                    ds.FindAll().Cast<SearchResult>().ToList().AsParallel().ForAll(r =>
                    {
                        if (!searcher.CancellationPending)
                        {
                            string machine = r.Properties["name"].Count > 0 ?
                                r.Properties["name"][0].ToString().ToUpper()
                                : "";
                            string description = r.Properties["description"].Count > 0 ?
                                r.Properties["description"][0].ToString().Replace("(SCCM)", "").Trim()
                                : "";
                            string m = machine + (description.Length > 0 ? " (" + description + ")" : "");
                            if (r.GetDirectoryEntry().Path.ToLower().Contains(",ou=workstations,"))
                            {
                                //Console.WriteLine(m + " --- " + r.GetDirectoryEntry().Path);
                                //found.Add(m);
                                //add(MachinesBox, m);
                                searcher.ReportProgress(1, m);
                            }
                        }
                    });
                    //Console.WriteLine(found.Count + " found in " + ldap + ".");
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.ToString());
                }
            });
            begin(MachinesBox, false);
            searcher.ReportProgress(100);
            Console.WriteLine("\nSearching machine done.");
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

                if (add) MachinesBox.Items.Add(item);
                else MachinesBox.Items.Remove(item);


                int pccount = MachinesBox.Items.Cast<String>().Where(x => !x.Contains("=")).Count();
                set(PCCount, "" + pccount);
                set(SearchButton, GlassIcons[Glass_i = (Glass_i + 1) % GlassIcons.Length]);
            }
        }

        void enableControls(bool Enabled)
        {
            enable(ADTree, Enabled);
            enable(DomainsBox, Enabled);
            enable(SitesBox, Enabled);
            enable(DptsBox, Enabled);
            enable(ADTreeRefresh, Enabled);
            enable(SearchButton, Enabled);
            enable(OKButton, Enabled);
            set(SearchButton, GlassIcons[0]);
        }

    }
}
