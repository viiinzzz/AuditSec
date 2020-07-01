using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Management;
using System.Collections;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;
using System.Threading;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;
using Microsoft.VisualBasic;


namespace AuditSec
{
    public partial class SCCMCollectionDesigner : Form
    {
        void saveSettings(bool disposing)
        {
            AuditSec.saveSettings_SMS();

            if (disposing && (components != null)) components.Dispose();
            //base.Dispose(disposing);

            AuditSec.Exit("SCCM Collection Designer module ended.",
                () => { base.Dispose(disposing); return true; });
        }

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
            public SCCMCollectionDesigner GUI; public TreeView tree; public CheckedListBox list;
            public Label label; public Label count; public TextBox basebox, idbox; public string name;
            public Func<Tree<XNode>, bool> Refresh_; public Func<Tree<XNode>, XNode, bool> Open_;
            public int depth, depth2; public List<string> exclude;
            public Hashtable node2item = new Hashtable(), item2node = new Hashtable();

            override public string ToString()
            {
                return name;
            }
            public Tree(SCCMCollectionDesigner GUI, TreeView tree, CheckedListBox list,
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
        

        public class ColNode : Node
        {
            public string CollectionID;
            public string Name;
            public string Comment;

            public string Members = "";
            public string Schedules = "";
            public string Query = "";

            public Tree<ColNode> tree;

            public ColNode(string CollectionID, string Name, string Comment)
            {
                this.CollectionID = CollectionID;
                this.Name = Name;
                this.Comment = Comment;
                this.uptodate = false;
                this.depth = depth;
                this.node = null;
            }

            public string getMembers(bool force)
            {
                if (force) return Members = getCollectionSchedules(CollectionID);
                else return Members.Length > 0 ? Members : Members = getCollectionMembers(CollectionID);
            }

            public string getSchedules(bool force)
            {
                if (force) return Schedules = getCollectionSchedules(CollectionID);
                return Schedules.Length > 0 ? Schedules : Schedules = getCollectionSchedules(CollectionID);
            }

            public string getQuery(bool force)
            {
                if (force) return Query = getCollectionQuery(CollectionID);
                return Query.Length > 0 ? Query : Query = getCollectionQuery(CollectionID);
            }

        }


        bool ColRefresh(Tree<ColNode> tree)
        {
            bool ret = false;
            if (tree.depth > 0 && tree.depth2 >= tree.depth
                && tree.idbox.Text.Length > 0)
            {
                if (tree.shown()) tree.GUI.update(tree.tree, true);
                List<ColNode> sub = getSubCollections(tree.idbox.Text, false);
                foreach (ColNode item in sub) item.depth = 1;
                ret = ColRefresh(tree, tree.tree.Nodes, sub, false);
                if (tree.shown()) tree.GUI.sort(tree.tree);
                if (tree.shown()) foreach (TreeNode node in tree.tree.Nodes) tree.GUI.expand(node, false);
                if (tree.shown()) tree.GUI.update(tree.tree, false);
            }
            return ret;
        }

        bool ColRefresh(Tree<ColNode> tree, TreeNodeCollection siblings, List<ColNode> subCols, bool expand)
        {
            bool ret = false;
            {
                ret = true; subCols.AsParallel().WithDegreeOfParallelism(expand ? 1 : 8).ForAll(item => {
                    
                    foreach(ColNode item2 in tree.item2node.Keys)
                        if (item2.Name.Equals(item.Name)) { item = item2; break; }


                    if (item.depth <= (expand ? tree.depth2 : tree.depth)
                    && take(item.Name, tree.exclude)) try
                    {
                        //Console.WriteLine(tree.name + " walk TreeNode name=" + item.Name + " depth=" + item.depth + "/" + tree.depth2 + " expand="
                        //    + expand + " uptodate=" + item.uptodate + " node=" + (item.node == null ? "null" : "defined"));

                        if (item.node == null)
                        {
                            item.node = new TreeNode(item.Name, 0, 1);
                            tree.item2node[tree.node2item[item.node] = item] = item.node;
                            if (tree.shown()) tree.GUI.set(tree.count, "" + tree.node2item.Count);
                            if (tree.shown()) tree.GUI.add(siblings, item.node);
                            if (item.Name.ToUpper().StartsWith("WW-")) if (tree.shown()) tree.GUI.add(tree.list, item.Name.Substring(3));
                        }
                        if (!expand)
                        {
                            List<ColNode> sub2 = getSubCollections(item.CollectionID, false);
                            foreach (ColNode item2 in sub2) item2.depth = item.depth + 1;
                            if (!ColRefresh(tree, item.node.Nodes, sub2, expand)) ret = false;
                            item.uptodate = true;
                        }
                    }
                    catch (Exception e) { Console.WriteLine("WQL: " + item.Name + ": " + e.ToString()); }});
            }
            return ret;
        }

        bool ColOpen(Tree<ColNode> tree, ColNode item)
        {
            //if (tree.shown()) tree.GUI.update(tree.tree, true);
            bool ret = ColRefresh(tree, item.node.Nodes, getSubCollections(item.CollectionID, false), true);
            //if (tree.shown()) tree.GUI.sort(tree.tree);
            //if (tree.shown()) tree.GUI.update(tree.tree, false);
            return ret;
        }


        static public string getFirstCollectionID(string CollectionLike)
        {
            return getFirstCollectionID(CollectionLike, false, true, true);
        }

        static public string getFirstCollectionID(string CollectionLike, bool nameonly, bool like, bool show)
        {
            List<ColNode> Collection = getCollection(CollectionLike, nameonly, like, show);
            return Collection.Count > 0 ? Collection.First().CollectionID : "";
        }

        static public ColNode getFirstCollection(string CollectionLike, bool nameonly, bool like, bool show)
        {
            List<ColNode> Collection = getCollection(CollectionLike, nameonly, like, show);
            return Collection.Count > 0 ? Collection.First() : null;
        }

        static public List<ColNode> getCollection(string CollectionLike)
        {
            return getCollection(CollectionLike, false, true, true);
        }

        static public List<ColNode> getCollection(string CollectionLike, bool nameonly, bool like, bool show)
        {
            List<ColNode> result = new List<ColNode>();
            string QUERY =
                "SELECT CollectionID, Name, Comment FROM SMS_Collection "
                + " WHERE ( Name " + (like ? "LIKE" : "=") + " \"" + CollectionLike + "\""
                + (nameonly ? "" : " OR Comment " + (like ? "LIKE" : "=") + " \"" + CollectionLike + "\" ")
                + " )";
            ManagementScope scope = null; try
            {
                scope = new ManagementScope(@"\\" + AuditSec.settings.smssrv + @"\Root\sms\" + AuditSec.settings.smssite);
                if (show) Console.WriteLine("Running : Query Collection like \"" + CollectionLike + "\""
                    + "\n" + QUERY);
                ManagementObjectCollection moc = new ManagementObjectSearcher(scope, new ObjectQuery(QUERY)).Get();
                if (moc == null) throw new Exception("Query returned null result.");
                else foreach (ManagementObject mo in moc)
                    {
                        string CollectionID = getStringValue(mo, null, "CollectionID");
                        string Name = getStringValue(mo, null, "Name");
                        string Comment = getStringValue(mo, null, "Comment");

                        string Members = getCollectionMembers(CollectionID);
                        string Schedules = getCollectionSchedules(CollectionID);

                        if (CollectionID != null && CollectionID.Length > 0
                            && Name != null && Name.Length > 0)
                        {
                            ColNode colItem = new ColNode(CollectionID, Name, Comment);
                            result.Add(colItem);
                            if (show)
                                Console.WriteLine("    CollectionID=" + CollectionID
                                    + "\n        Name=" + Name
                                    + "\n        Comment=" + Comment
                                    );
                        }
                    }
            }
            catch (ManagementException ee)
            {
                string extendedError = ee.Message + ". " + ee.ErrorCode.ToString();
                if (extendedError.StartsWith("Quota violation"))
                    extendedError = "Quota violation. Too long a request (" + QUERY.Length + ")";

                try
                {
                    string desc = ee.ErrorInformation["Description"].ToString();
                    desc = desc.Replace(QUERY, "");
                    extendedError += "\n" + desc;
                }
                catch (Exception eee)
                {
                    //Console.WriteLine(eee.ToString());
                }
                Console.WriteLine("Query interrupted. Management Exception. " + extendedError
                    + "\n" + QUERY + "\n" + ee.ToString()
                    );
                if (extendedError.StartsWith("Quota violation"))
                    Console.WriteLine("Query: " + QUERY);
            }
            catch (Exception ee)
            {
                Console.WriteLine("Query interrupted.\n" + ee.ToString());
            }
            if (scope != null) try
                {
                    scope.Path = new ManagementPath();
                }
                catch (Exception ee)
                {
                    Console.WriteLine("Could not clean ManagementScope! " + ee.Message);
                }
            if (show) Console.WriteLine("Returned: Query Collection Like \"" + CollectionLike + "\": " + result.Count);
            return result;
        }

        static List<ColNode> getSubCollections(string parentCollectionID)
        {
            return getSubCollections(parentCollectionID, true);
        }

        static string getStringValue(ManagementObject mo, string table, string prop)
        {
            try
            {
                if (table == null)
                    return mo.Properties[prop] != null ? mo.Properties[prop].Value.ToString() : "";
                else
                {
                    PropertyData pd = mo.Properties[table] == null
                        || mo.Properties[table].Value == null
                        || !(mo.Properties[table].Value is ManagementBaseObject)
                        ? null : ((ManagementBaseObject)mo.Properties[table].Value).Properties[prop];
                    return
                        pd == null
                        || pd.Value == null
                        ? "" : pd.Value.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("getStringValue " + table + "." + prop + " = " + e.Message + "\n" + e.ToString());
                return "";
            }
        }

        static List<ColNode> getSubCollections(string parentCollectionID, bool show)
        {
            List<ColNode> result = new List<ColNode>();
            string QUERY =
                "SELECT CollectionID, Name, Comment"
                + " FROM SMS_Collection"
                + " RIGHT JOIN SMS_CollectToSubCollect AS CTSC"
                + " ON CTSC.subCollectionID = CollectionID"
                + " WHERE CTSC.parentCollectionID = \"" + parentCollectionID + "\"";
            ManagementScope scope = null; try
            {
                scope = new ManagementScope(@"\\" + AuditSec.settings.smssrv + @"\Root\sms\" + AuditSec.settings.smssite);
                if (show) Console.WriteLine("Running : Query SubCollections in parentCollectionID \"" + parentCollectionID + "\""
                    + "\n" + QUERY);
                ManagementObjectCollection moc = new ManagementObjectSearcher(scope, new ObjectQuery(QUERY)).Get();
                if (moc == null) throw new Exception("Query returned null result.");
                else foreach (ManagementObject mo in moc)
                    {
                        string CollectionID = getStringValue(mo, null, "CollectionID");
                        string Name = getStringValue(mo, null, "Name");
                        string Comment = getStringValue(mo, null, "Comment");

                        if (CollectionID != null && CollectionID.Length > 0
                            && Name != null && Name.Length > 0)
                        {
                            ColNode colItem = new ColNode(CollectionID, Name, Comment);
                            result.Add(colItem);
                            if (show)
                                Console.WriteLine("    CollectionID=" + CollectionID
                                    + "\n        Name=" + Name
                                    + "\n        Comment=" + Comment
                                    );
                        }
                    }
            }
            catch (ManagementException ee)
            {
                string extendedError = ee.Message + ". " + ee.ErrorCode.ToString();
                if (extendedError.StartsWith("Quota violation"))
                    extendedError = "Quota violation. Too long a request (" + QUERY.Length + ")";

                try
                {
                    string desc = ee.ErrorInformation["Description"].ToString();
                    desc = desc.Replace(QUERY, "");
                    extendedError += "\n" + desc;
                }
                catch (Exception eee)
                {
                    //Console.WriteLine(eee.ToString());
                }
                Console.WriteLine("Query interrupted. Management Exception. " + extendedError
                    + "\n" + QUERY + "\n" + ee.ToString()
                    );
                if (extendedError.StartsWith("Quota violation"))
                    Console.WriteLine("Query: " + QUERY);
            }
            catch (Exception ee)
            {
                Console.WriteLine("Query interrupted.\n" + ee.ToString());
            }
            if (scope != null) try
                {
                    scope.Path = new ManagementPath();
                }
                catch (Exception ee)
                {
                    Console.WriteLine("Could not clean ManagementScope! " + ee.Message);
                }
            if (show) Console.WriteLine("Returned: Query SubCollections in parentCollectionID \"" + parentCollectionID + "\": " + result.Count);
            return result;
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

                //test environment
                path = path.Replace("OU=SCM-000001,OU=DEVINFRA,OU=SDL,", "");

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
                        if (!item.invalid && !item.de.Name.Equals("DC=DEVMYCOMPANY"))
                        {
                            TreeNode node = new TreeNode(item.Name, 2, 3);
                            item.node = node;
                            tree.item2node[tree.node2item[node] = item] = node;
                            //Console.WriteLine(domain.GetDirectoryEntry().Name + " " + item.level + "/" + tree.depth2);


                            if (tree.shown()) tree.GUI.set(tree.count, "" + tree.node2item.Count);
                            if (tree.shown()) tree.GUI.add(tree.tree, node);
                            if (tree.shown()) tree.GUI.add(tree.list, item.Name);
                            if (!ADRefresh(tree, node, item, false)) ret = false;
                        }
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

        bool F12_alreadyshown = false;
        bool ADRefresh(Tree<ADNode> tree, TreeNode node, ADNode item, bool expand)
        {
            bool ret = true;
            if (item.depth < (expand ? tree.depth2 : tree.depth))
            {
                List<DirectoryEntry> lde2 = new List<DirectoryEntry>();
                DirectoryEntries children = null;
                if (!item.de.Name.Equals("DC=SDL")
                    && !item.de.Name.Equals("DC=DEVMYCOMPANY"))
                {
                    //prod environment
                    children = item.de.Children;
                }
                else if (item.de.Name.Equals("DC=SDL")) try
                {
                    //test environment
                    DirectoryEntry de = item.de; children = de.Children;
                    foreach (DirectoryEntry de2_ in children) Console.WriteLine(de.Name + "/" + de2_.Name);

                    de = children.Find("OU=SDL"); children = de.Children;
                    //foreach (DirectoryEntry de2_ in children) Console.WriteLine(de.Name + "/" + de2_.Name);
                    de = children.Find("OU=DEVINFRA"); children = de.Children;
                    //foreach (DirectoryEntry de2_ in children) Console.WriteLine(de.Name + "/" + de2_.Name);
                    de = children.Find("OU=SCM-000001"); children = de.Children;
                    //foreach (DirectoryEntry de2_ in children) Console.WriteLine(de.Name + "/" + de2_.Name);
                    if (!F12_alreadyshown)
                    {
                        F12_alreadyshown = true;
                        MessageBox.Show("Test Environment mode activated.\n\nPress F12 to show Logs.", "Test Environment");
                    }
                }
                catch (Exception e)
                {
                    children = null;
                    Console.WriteLine("Test Environment mode failure\n" + e.ToString());
                    MessageBox.Show("Test Environment mode failure\n\nPress F12 to show Logs.", "Test Environment");
                }
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
                                if (item2.isDpt) if (tree.shown()) tree.GUI.add(ADDptBox, item2.dpt);
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
            //if (tree.shown()) tree.GUI.update(tree.tree, true);
            bool ret = item.uptodate || ADRefresh(tree, item.node, item, true);
            //if (tree.shown()) tree.GUI.sort(tree.tree);
            //if (tree.shown()) tree.GUI.update(tree.tree, false);
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
        Tree<ColNode> SCCM_TREE, DPT_TREE;
        bool REFRESH_AT_STARTUP = true;

        public SCCMCollectionDesigner()
        {
            InitializeComponent();
            AuditSec.InitializeSMS(false, MYCOMPANY_Settings.SMS_DEFAULT_SERVER, MYCOMPANY_Settings.SMS_DEFAULT_SITE);
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
                DepartmentSubstract();
            }
            else if (control is CheckedListBox)
            {
                if (!((CheckedListBox)control).Items.Contains((string)value))
                    ((CheckedListBox)control).Items.Add((string)value);
                DepartmentSubstract();
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
            if (control == DptTree)
            {
                updateControl(ADDptBox, begin);
                updateControl(TemplDptBox, begin);
            }
            else
            {
                if (begin)
                {
                    PrepareSite.Enabled = CheckConsistency.Enabled = PrepareDpts.Enabled = ClearButton.Enabled = ProceedButton.Enabled =
                        SiteCodeBox.Enabled = SiteNameBox.Enabled = startBox.Enabled = hourSpanBox.Enabled =
                        DomainsBox.Enabled = dptStartBox.Enabled = dptHourSpanBox.Enabled =
                        ADDptBox.Enabled = ADDptBox.Enabled = control.Visible = false;
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
                        PrepareSite.Enabled = CheckConsistency.Enabled = PrepareDpts.Enabled = ClearButton.Enabled = ProceedButton.Enabled =
                        SiteCodeBox.Enabled = SiteNameBox.Enabled = startBox.Enabled = hourSpanBox.Enabled =
                        DomainsBox.Enabled = dptStartBox.Enabled = dptHourSpanBox.Enabled =
                        ADDptBox.Enabled = ADDptBox.Enabled = true;
                    }
                    if (control is TreeView && ((TreeView)control).Nodes.Count > 0)
                    {
                        ((TreeView)control).SelectedNode = ((TreeView)control).Nodes[0];
                        ((TreeView)control).Nodes[0].EnsureVisible();
                    }
                }
            }
        }
        List<Control> DisabledControls = new List<Control>();
        List<Control> ControlsToEnable = new List<Control>();

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


        void DepartmentSubstract()
        {
            Invoke(new dptSubstractDelegate(dptSubstract));
        }
        public delegate void dptSubstractDelegate();
        void dptSubstract()
        {
            for (int i = 0; i < TemplDptBox.Items.Count; i++)
            {
                TemplDptBox.SetItemChecked(i, true);
                ADDptBox.Items.Remove(TemplDptBox.Items[i]);
            }
        }











        private void ADTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!ADTree.Visible) return;
            if (e.Node.IsSelected)
            {
                string patha = e.Node.FullPath;
                SiteNameBox.Text = ""; SiteCodeBox.Text = "";
                DomainsBox.BeginUpdate();
                for (int i = 0; i < DomainsBox.Items.Count; i++) DomainsBox.SetItemChecked(i, false);

                ADNode adNode = AD_TREE == null ? null : (ADNode)AD_TREE.node2item[e.Node];
                if (adNode != null)
                {
                    TreeNode nodeb = null; string pathb = null;
                    if (adNode.isSite)
                    {
                        Console.WriteLine("AD: selected: " + adNode.domain + "/" + adNode.site);
                        AD_TREE.Open(adNode);
                        e.Node.Expand();

                        SiteCodeBox.Text = adNode.siteonly.ToUpper();
                        if (adNode.de.Properties["description"].Value != null)
                            SiteNameBox.Text = adNode.de.Properties["description"].Value.ToString();
                        int index = DomainsBox.Items.IndexOf(adNode.domain);
                        if (index != -1) DomainsBox.SetItemChecked(index, true);
                        foreach (ADNode adNode2 in AD_TREE.selectNodes(adNode2_ => {
                            return adNode2_ != null && adNode2_.node != adNode.node && adNode2_.isSite
                                && ( adNode2_.site.Equals(adNode.site) || adNode2_.siteonly.Equals(adNode.siteonly)); }))
                        {
                            //Console.WriteLine("AD: related: " + adNode2.domain + "/" + adNode2.site);
                            index = DomainsBox.Items.IndexOf(adNode2.domain);
                            if (index != -1) DomainsBox.SetItemChecked(index, true);
                        }
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain + @"\" + adNode.site + " " + adNode.domain);
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain + @"\" + adNode.site);
                    }
                    else if (adNode.isDomain)
                    {
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain);
                    }
                    else if (adNode.isDpt)
                    {
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain + @"\" + adNode.site + " " + adNode.domain
                                                                        + @"\" + adNode.site + " " + adNode.domain + " " + adNode.dpt);
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain + @"\" + adNode.site
                                                                        + @"\" + adNode.site + " " + adNode.dpt);
                    }
                    if (nodeb != null) SCCMTree.SelectedNode = nodeb;
                    else MessageBox.Show(ADLabel.Text + ": Container selected: " + patha
                        + "\n\n" + SCCMLabel.Text + ": Collection not found: " + pathb,
                        "SCCM Collection Designer");

                }
                
                DomainsBox.EndUpdate();
            }
        }


        private void ADTree_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (!ADTree.Visible) return;
            if (e.Node.IsSelected)
            {
                ADNode adNode = AD_TREE == null ? null : (ADNode)AD_TREE.node2item[e.Node]; if (adNode != null)
                {
                    TreeNode nodeb = null; string pathb = null;
                    if (adNode.isSite)
                    {
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain + @"\" + adNode.site + " " + adNode.domain);
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain + @"\" + adNode.site);
                    }
                    else if (adNode.isDomain)
                    {
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain);
                    }
                    else if (adNode.isDpt)
                    {
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain + @"\" + adNode.site + " " + adNode.domain
                                                                        + @"\" + adNode.site + " " + adNode.domain + " " + adNode.dpt);
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain + @"\" + adNode.site
                                                                        + @"\" + adNode.site + " " + adNode.dpt);
                    }
                    if (nodeb != null)
                    {
                        nodeb.Collapse();
                        SCCMTree.SelectedNode = nodeb;
                    }
                    //else MessageBox.Show(ADLabel.Text + ": Container selected: " + patha
                        //+ "\n\n" + SCCMLabel.Text + ": Collection not found: " + pathb,
                        //"SCCM Collection Designer");

                }
            } else ADTree.SelectedNode = e.Node;
        }

        private void ADTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (!ADTree.Visible) return;
            if (e.Node.IsSelected)
            {
                ADNode adNode = AD_TREE == null ? null : (ADNode)AD_TREE.node2item[e.Node]; if (adNode != null)
                {
                    TreeNode nodeb = null; string pathb = null;
                    if (adNode.isSite)
                    {
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain + @"\" + adNode.site + " " + adNode.domain);
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain + @"\" + adNode.site);
                    }
                    else if (adNode.isDomain)
                    {
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain);
                    }
                    else if (adNode.isDpt)
                    {
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain + @"\" + adNode.site + " " + adNode.domain
                                                                            + @"\" + adNode.site + " " + adNode.domain + " " + adNode.dpt);
                        if (nodeb == null) nodeb = find(SCCMTree.Nodes, pathb = adNode.domain + @"\" + adNode.site
                                                                            + @"\" + adNode.site + " " + adNode.dpt);
                    }
                    if (nodeb != null)
                    {
                        nodeb.Expand();
                        SCCMTree.SelectedNode = nodeb;
                    }
                    //else MessageBox.Show(ADLabel.Text + ": Container selected: " + patha
                        //+ "\n\n" + SCCMLabel.Text + ": Collection not found: " + pathb,
                        //"SCCM Collection Designer");

                }
            }
            else ADTree.SelectedNode = e.Node;
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


        private void SCCMTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!SCCMTree.Visible) return;
            TreeNode nodeb = e.Node; if (nodeb.IsSelected)
            {
                string pathb = nodeb.FullPath;
                //                                    (SDL)   \ (BBB)    (_SDL)     \ BBB_    (SDL_)*(GRO OSM)
                string patha = Regex.Replace(pathb, @"([^\\]+)\\([^\\ ]+)( [^\\ ]+)*\\[^\\ ]+ (\1 )*([^\\]+)", @"$1\$2\$5");
                patha = Regex.Replace(patha, @"([^\\]+)\\([^\\ ]+) [^\\ ]+", @"$1\$2");
                ColNode colNode = SCCM_TREE == null ? null : (ColNode)SCCM_TREE.node2item[nodeb];
                if (colNode != null)
                {
                    Console.WriteLine("Site collections: selected: " + colNode.Name + ". " + pathb + " --> " + patha);
                    if (colNode.depth == 2) { SCCM_TREE.Open(colNode); nodeb.Expand(); }

                    TreeNode nodea = find(ADTree.Nodes, patha);
                    if (nodea != null) ADTree.SelectedNode = nodea;
                    else MessageBox.Show(SCCMLabel.Text + ": Collection selected: " + pathb
                        + "\n\n" + ADLabel.Text + ": Container not found: " + patha,
                        "SCCM Collection Designer");
                }
            }
        }

        private void SCCMTree_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (!SCCMTree.Visible) return;
            TreeNode nodeb = e.Node; if (nodeb.IsSelected)
            {
                string pathb = nodeb.FullPath;
                //                                    (SDL)   \ (BBB)    (_SDL)     \ BBB_    (SDL_)*(GRO OSM)
                string patha = Regex.Replace(pathb, @"([^\\]+)\\([^\\ ]+)( [^\\ ]+)*\\[^\\ ]+ (\1 )*([^\\]+)", @"$1\$2\$5");
                patha = Regex.Replace(patha, @"([^\\]+)\\([^\\ ]+) [^\\ ]+", @"$1\$2");
                //Console.WriteLine("Site collections: collapsed: " + pathb + " --> " + patha);
                TreeNode nodea = find(ADTree.Nodes, patha);
                if (nodea != null)
                {
                    nodea.Collapse();
                    ADTree.SelectedNode = nodea;
                }
                else MessageBox.Show(SCCMLabel.Text + ": Collection selected: " + pathb
                        + "\n\n" + ADLabel.Text + ": Container not found: " + patha,
                        "SCCM Collection Designer");
            } else SCCMTree.SelectedNode = e.Node;
        }


        private void SCCMTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (!SCCMTree.Visible) return;
            TreeNode nodeb = e.Node; SCCMTree.SelectedNode = nodeb; if (nodeb.IsSelected)
            {
                string pathb = nodeb.FullPath;
                //                                    (SDL)   \ (BBB)    (_SDL)     \ BBB_    (SDL_)*(GRO OSM)
                string patha = Regex.Replace(pathb, @"([^\\]+)\\([^\\ ]+)( [^\\ ]+)*\\[^\\ ]+ (\1 )*([^\\]+)", @"$1\$2\$5");
                patha = Regex.Replace(patha, @"([^\\]+)\\([^\\ ]+) [^\\ ]+", @"$1\$2");
                //Console.WriteLine("Site collections: collapsed: " + pathb + " --> " + patha);
                TreeNode nodea = find(ADTree.Nodes, patha);
                if (nodea != null)
                {
                    nodea.Expand();
                    ADTree.SelectedNode = nodea;
                }
                else MessageBox.Show(SCCMLabel.Text + ": Collection selected: " + pathb
                        + "\n\n" + ADLabel.Text + ": Container not found: " + patha,
                        "SCCM Collection Designer");
            } else SCCMTree.SelectedNode = e.Node;
        }


        private void DptTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.IsSelected)
            {
                ColNode colNode = DPT_TREE == null ? null : (ColNode)DPT_TREE.node2item[e.Node];
                if (colNode != null && colNode.depth == 1)
                {
                    Console.WriteLine("Template Dpt: selected: " + colNode.Name);
                }
            }

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
                    ADRefresh, ADOpen, full ? 3 : 2, 3, ADExclude,
                    (label, basebox, idbox) =>
                    {
                        set(idbox, Forest.GetCurrentForest().Name); return true;
                    });
            }));
            ret.Start();
            return ret;
        }


        private void SCCMTreeRefresh_Click(object sender, EventArgs e)
        {
            SCCMTreeRefresh_Do(false);
        }

        Thread SCCMTreeRefresh_Do(bool full)
        {
            Thread ret = new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(500);
                List<string> exclude = new List<string>(new string[] {
                    @"^\!.*"
                });
                SCCM_TREE = new Tree<ColNode>(
                    this, SCCMTree, null,
                    SCCMLabel, baseCount, BaseColBox, BaseColIDBox, SCCMLabel.Text.Trim(),
                    ColRefresh, ColOpen, full ? 3 : 2, 3, exclude,
                    (label, basebox, idbox) =>
                    {
                        string wbem = @"\\" + AuditSec.settings.smssrv + @"\Root\sms\" + AuditSec.settings.smssite;
                        set(this, "SCCM Collection Designer - " + wbem);
                        Console.WriteLine("WBEM: " + wbem);
                        if (basebox.Text.Length == 0) set(basebox, "MYCOMPANY Workstations");
                        ColNode node = getFirstCollection(basebox.Text, true, true, false);
                        if (node == null) MessageBox.Show("Base Collection not found: " + basebox.Text
                            + "\nin WBEM: " + wbem, "SCCM Collection Designer");
                        set(basebox, node == null ? "" : node.Name);
                        set(idbox, node == null ? "" : node.CollectionID);
                        return true;
                    });
            }));
            ret.Start();
            return ret;
        }

        private void DptTreeRefresh_Click(object sender, EventArgs e)
        {
            DptTreeRefresh_Do();
        }

        Thread DptTreeRefresh_Do()
        {
            Thread ret = new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(500);
                List<string> exclude = new List<string>(new string[] {

                });
                DPT_TREE = new Tree<ColNode>(
                    this, DptTree, TemplDptBox,
                    DptLabel, dptCount, DptColBox, DptColIDBox, DptLabel.Text.Trim(),
                    ColRefresh, ColOpen, 1, 1, exclude,
                    (label, basebox, idbox) =>
                    {
                        if (basebox.Text.Length == 0) set(basebox, "!WW-Departments%");
                        ColNode node = getFirstCollection(basebox.Text, true, true, false);
                        set(basebox, node == null ? "" : node.Name);
                        set(idbox, node == null ? "" : node.CollectionID);
                        return true;
                    });
            }));
            ret.Start();
            return ret;
        }


        private void SCCMCollectionDesigner_Shown(object sender, EventArgs e)
        {
            //Trees populating

            if (REFRESH_AT_STARTUP) new Thread(new ThreadStart(delegate
            {
                ADTreeRefresh_Click(null, null);
                DptTreeRefresh_Click(null, null);
                Thread.Sleep(5000);
                SCCMTreeRefresh_Click(null, null);
            })).Start();
        }





        public class QueueItem
        {
            public string Action;
            public string ParentColName;
            public string ParentColID;
            public string Name;
            public string Comment;
            public string Schedule;
            public string QueryRule;
            public string CollectionID;
            public DataGridViewRow row = null;

            public QueueItem(string Action, string ParentColName, string ParentColID, string Name, string Comment, string Schedule, string QueryRule, string CollectionID)
            {
                this.Action = Action;
                this.ParentColName = ParentColName;
                this.ParentColID = ParentColID;
                this.Name = Name;
                this.Comment = Comment;
                this.Schedule = Schedule;
                this.QueryRule = QueryRule;
                this.ParentColID = ParentColID;
                this.CollectionID = CollectionID;
            }            
        }

        public string getParentColID(string ParentColName)
        {
            if (ParentColName == null || ParentColName.Length == 0)
                return "";
            if (ParentColName.Equals(BaseColBox.Text))
                return BaseColIDBox.Text;
            else if (ParentColName.Equals(DptColBox.Text))
                return DptColIDBox.Text;
            else
            {
                foreach (QueueItem q in getQueuedItems(Queue))
                    if (ParentColName.Equals(q.Name))
                        return q.CollectionID;
            }
            return "";
        }


        public string getCollectionID(string Name)
        {
            foreach (object key in SCCM_TREE.item2node.Keys)
            {
                ColNode colNode = (ColNode)key;
                if (colNode.Name.Equals(Name)) return colNode.CollectionID;
            }
            return "";
        }

        QueueItem newQueueItem(string Action, string ParentColName, string Name, string Comment, string Schedule, string QueryRule)
        {
            string CollectionID = getCollectionID(Name);
            QueueItem q = new QueueItem(
                CollectionID.Length > 0 ? "Update" : Action,
                ParentColName, getCollectionID(ParentColName), Name, Comment, Schedule, QueryRule, CollectionID);
            int i = Queue.Rows.Add(new Object[] {
                q.Action, q.ParentColName, q.ParentColID, q.Name, q.Comment, q.Schedule, q.QueryRule, q.CollectionID});
            q.row = Queue.Rows[i];
            return q;
        }

        QueueItem newQueueItem(string Action, string ParentColName, string ParentColID, string Name, string Comment, string Schedule, string QueryRule, string CollectionID,
            DataGridViewRow row)
        {
            QueueItem q = new QueueItem(
                Action, ParentColName, ParentColID, Name, Comment, Schedule, QueryRule, CollectionID);
            q.row = row;
            return q;
        }

        List<QueueItem> getQueuedItems()
        {
            return getQueuedItems(Queue);
        }

        List<QueueItem> getQueuedItems(DataGridView Queue)
        {
            List<QueueItem> ret = new List<QueueItem>();
            foreach (DataGridViewRow row in Queue.Rows)
                if (row.Cells[0].Value != null)
                {
                    ret.Add(newQueueItem(
                        row.Cells[0].Value.ToString(),
                        row.Cells[1].Value.ToString(),
                        row.Cells[2].Value.ToString(),
                        row.Cells[3].Value.ToString(),
                        row.Cells[4].Value.ToString(),
                        row.Cells[5].Value.ToString(),
                        row.Cells[6].Value.ToString(),
                        row.Cells[7].Value.ToString(),
                        row));
                }
            return ret;
        }

        QueueItem getQueuedItem(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return null;
            DataGridViewRow row = Queue.Rows[e.RowIndex];
            if (row.Cells[0].Value == null) return null;
            return newQueueItem(
                        row.Cells[0].Value.ToString(),
                        row.Cells[1].Value.ToString(),
                        row.Cells[2].Value.ToString(),
                        row.Cells[3].Value.ToString(),
                        row.Cells[4].Value.ToString(),
                        row.Cells[5].Value.ToString(),
                        row.Cells[6].Value.ToString(),
                        row.Cells[7].Value.ToString(),
                        row);
        }

        private void PrepareSiteButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\nPrepare Site Creation...");
            if (
                SiteNameBox.Text.Length > 0
                && SiteCodeBox.Text.Length > 0
                && DomainsBox.CheckedItems.Count > 0
                && TemplDptBox.CheckedItems.Count > 0
                && ForestBox.Text.Length > 0
                && BaseColIDBox.Text.Length > 0)
            {
                string top = BaseColBox.Text;
                int hourSpan = 24; try { hourSpan = int.Parse(hourSpanBox.Text); }
                catch (Exception ee)
                {
                    Console.WriteLine("Invalid hourSpan: " + hourSpanBox.Text);
                }

                foreach (string domain in DomainsBox.CheckedItems)
                {
                    string domainforest = (domain + "." + ForestBox.Text).ToLower();
                    string domaincomment = "All " + domain + " Workstations with SCCM Client installed";
                    QueueItem domainq = newQueueItem("Create", top, domain, domaincomment, "", "");

                    string site = SiteNameBox.Text;
                    string code = SiteCodeBox.Text;
                    string codedomain = code + (DomainsBox.CheckedItems.Count > 1 ? " " + domain : "");
                    string sitecomment = "All " + codedomain + " Workstations with SCCM Client installed"
                        + " (" + site + ")";
                    QueueItem siteq = newQueueItem("Create", domain, codedomain, sitecomment, "", "");

                    foreach (string dpt in TemplDptBox.CheckedItems)
                    {
                        string sitedpt = codedomain + " " + dpt;
                        string dptcomment = "All " + sitedpt + " Workstations with SCCM Client installed"
                            + " (" + site + ")";
                        string dptsched = startBox.Text + ", Every" + (hourSpan == 1 ? "" : " " + hourSpan) + " hour" + (hourSpan < 2 ? "" : "s");
                        string dptquery = "select SMS_R_System.ResourceId, SMS_R_System.ResourceType, SMS_R_System.Name,"
                            + " SMS_R_System.SMSUniqueIdentifier, SMS_R_System.ResourceDomainORWorkgroup, SMS_R_System.Client from  SMS_R_System"
                            + " where SMS_R_System.SystemOUName = \"" + domainforest + "/" + code + "/" + dpt + "/Workstations\""
                            + " and SMS_R_System.OperatingSystemNameandVersion like \"%Workstation%\" and SMS_R_System.Client = 1";
                        QueueItem dptq = newQueueItem("Create", codedomain, sitedpt, dptcomment, dptsched, dptquery);
                    }
                }



                Console.WriteLine("Done.");
            }
            else
            {
                Console.WriteLine("Invalid parameters.");

            }
        }




        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Queue_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            QueueItem q = getQueuedItem(e);
            if (q == null) return;
            Console.WriteLine("\nProceed Row...");
            string answer = Interaction.InputBox("Do you want to proceed:\n\n    " + q.Action + " Collection " + q.Name
                + "\n\nYES / NO"
                , "SCCM Collection Designer - Proceed Row", "Yes");
            Console.WriteLine("Proceed " + q.Action + " Collection " + q.Name + ": " + answer);
            if (answer.ToUpper().Equals("YES"))
            {
                proceed(q);
            }
            else //Proceed = NO
            {
                Console.WriteLine("Skip " + q.Action + " Collection " + q.Name + ": " + answer);
            }
            Console.WriteLine("Done.");
            DptTreeRefresh_Do();
            SCCMTreeRefresh_Do(false);
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\nProceed Queue...");
            List<QueueItem> qs = getQueuedItems();
            bool ALL = false;
            foreach(QueueItem q in qs)
            {
                string answer = ALL ? "All" : Interaction.InputBox("Do you want to proceed:\n\n    " + q.Action + " Collection " + q.Name
                    + "\n\nYES / NO / ALL / QUIT"
                    , "SCCM Collection Designer - Proceed Queue", "Yes");
                Console.WriteLine("Proceed " + q.Action + " Collection " + q.Name + ": " + answer);
                if (ALL || answer.ToUpper().Equals("YES") || answer.ToUpper().Equals("ALL")) 
                {
                    if (answer.ToUpper().Equals("ALL")) ALL = true;
                    proceed(q);
                }
                else if (answer.ToUpper().Equals("QUIT"))
                {
                    MessageBox.Show("Proceed Queue aborted by user.", "SCCM Collection Designer - Proceed Queue");
                    return;
                }
                else //Proceed = NO
                {
                    Console.WriteLine("Skip " + q.Action + " Collection " + q.Name + ": " + answer);
                }
            }
            Console.WriteLine("Done.");
            DptTreeRefresh_Do();
            SCCMTreeRefresh_Do(false);
        }

        private void proceed(QueueItem q)
        {
            if (q == null) return;
            if (q.Action.Equals("Create"))
            {
                CreateCollection(q);
                q.row.Cells[7].Value = q.CollectionID;
                q.row.Cells[0].Value = q.Action;
                Console.WriteLine("CollectionID: " + (q.CollectionID.Length == 0 ? "Failure" : q.CollectionID));

                foreach (QueueItem q2 in getQueuedItems())
                    if (q2 != q && q2.ParentColID.Length == 0 && q2.ParentColName.Equals(q.Name))
                        q2.row.Cells[2].Value = q2.ParentColID = q.CollectionID;
            }
            else if (q.Action.Equals("Delete"))
            {
                DeleteCollection(q);
                q.row.Cells[7].Value = q.CollectionID;
                q.row.Cells[0].Value = q.Action;
            }
            else if (q.Action.Equals("Update"))
            {
                UpdateCollection(q);
                q.row.Cells[0].Value = q.Action;
            }
            else
            {
                Console.WriteLine(q.Action + ": Invalid Action.");
                Queue.Rows.Remove(q.row);
            }

        }


        private void ClearButton_Click(object sender, EventArgs e)
        {
            Queue.Rows.Clear();
        }

        char[] turn = new char[] { '/', '-', '\\', '|' };

        private void CheckStructureConsistency_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\nCheck AD <==> SCCM Consistency...");
            Thread T1 = ADTreeRefresh_Do(true);
            Thread T2 = SCCMTreeRefresh_Do(true);
            new Thread(new ThreadStart(delegate
            {
                int turni = 0; while (T1.IsAlive || T2.IsAlive)
                {
                    set(CheckConsistency, "Check Active Directory "+ turn[turni++ % 4]+" Collections Consistency");
                    Thread.Sleep(250);
                }
                set(CheckConsistency, "Check Active Directory / Collections Consistency");


                Console.WriteLine("List Active Directory OUs...");
                Hashtable AD_H = new Hashtable();
                check(AD_TREE, AD_H);

                Console.WriteLine("List SCCM OUs...");
                Hashtable SCCM_H = new Hashtable();
                check(SCCM_TREE, SCCM_H);

                Console.WriteLine("Compute SCCM OU suggested deletion list...");
                Hashtable delete_H = new Hashtable(SCCM_H);
                foreach (string patha in AD_H.Keys) delete_H.Remove(patha);
                List<string> delete_pathb = delete_H.Values.Cast<string>().ToList(); delete_pathb.Sort();
                foreach (string pathb in delete_pathb)
                    Console.WriteLine("Collections: Suggest Delete: " + pathb);
                Console.WriteLine("Collections: Suggest Delete: " + delete_pathb.Count + " item(s)");

                Console.WriteLine("Compute SCCM OU suggested creation list...");
                Hashtable create_H = new Hashtable(AD_H);
                foreach (string patha in SCCM_H.Keys) create_H.Remove(patha);
                List<string> create_patha = create_H.Values.Cast<string>().ToList(); create_patha.Sort();
                foreach (string patha in create_patha)
                    Console.WriteLine("Collections: Suggest Create: " + patha);
                Console.WriteLine("Collections: Suggest Create: " + create_patha.Count + " items(s)");

                Console.WriteLine("Enqueue SCCM OU suggested deletion list......");
                if (SCCM_TREE != null) foreach (string pathb in delete_pathb)
                {
                    TreeNode nodeb = find(SCCMTree.Nodes, pathb);
                    ColNode item = nodeb == null ? null : (ColNode)SCCM_TREE.node2item[nodeb];
                    if (item != null)
                    {
                        ColNode parent = nodeb.Parent == null ? null : (ColNode)SCCM_TREE.node2item[nodeb.Parent];
                        
                        add(Queue, new object[] { "Delete",
                            parent == null ? BaseColBox.Text : parent.Name,
                            parent == null ? BaseColIDBox.Text : parent.CollectionID,
                            item.Name,
                            item.Comment,
                            item.getSchedules(false),
                            item.getQuery(false),
                            item.CollectionID
                        });
                    }
                }

                Console.WriteLine("Enqueue SCCM OU suggested creation list......");
                string top = BaseColBox.Text;
                int hourSpan = 24; try {
                    hourSpan = int.Parse(hourSpanBox.Text);
                }
                catch (Exception ee)
                {
                    Console.WriteLine("Invalid hourSpan: " + hourSpanBox.Text);
                }
                if (SCCM_TREE != null) foreach (string patha in create_patha)
                {
                    TreeNode nodea = find(ADTree.Nodes, patha);
                    ADNode item = nodea == null ? null : (ADNode)AD_TREE.node2item[nodea];
                    if (item != null)
                    {

                        List<string> _domains = new List<string>();
                        string _domain = null;
                        string _domainforest = null;
                        string _domaincomment = null;
                        string _code = null;
                        string _codedomain = null;
                        string _codedomain2 = null;
                        string _sitecomment = null;
                        string _sitename = null;
                        string _dpt = null;
                        string _sitedpt = null;
                        string _sitedpt2 = null;
                        string _dptcomment = null;
                        string _dptcomment2 = null;
                        string _dptsched = null;
                        string _dptquery = null;

                        Console.WriteLine("Enqueue SCCM OU suggested creation list......2 "  +item.ToString());
                        try
                        {
                            if (item.isDomain || item.isSite || item.isDpt)
                            {
                                //_domains.Add(item.domain);
                                foreach (ADNode adNode2 in AD_TREE.selectNodes(adNode2_ =>
                                {
                                    return adNode2_ != null && adNode2_.node != item.node && adNode2_.isSite
                                        && (adNode2_.site.Equals(item.site) || adNode2_.siteonly.Equals(item.siteonly));
                                })) _domains.Add(adNode2.domain);

                                _domain = item.domain;
                                _domainforest = (item.domain + "." + ForestBox.Text).ToLower();
                                _domaincomment = "All " + item.domain + " Workstations with SCCM Client installed";
                            }
                            if (item.isSite || item.isDpt)
                            {
                                _code = item.site;
                                _codedomain = _code + (_domains.Count > 1 ? " " + item.domain : "");
                                _codedomain2 = _code + (_domains.Count > 1 ? "" : " " + item.domain);
                                //Console.WriteLine("_codedomain: " + _codedomain);
                                _sitename = item.de.Properties["description"].Value != null ? item.de.Properties["description"].Value.ToString() : "";
                                _sitecomment = "All " + _codedomain + " Workstations with SCCM Client installed"
                                    + " (" + _sitename + ")";
                            }
                            if (item.isDpt)
                            {
                                _sitedpt = _codedomain + " " + item.dpt;
                                _sitedpt2 = _codedomain2 + " " + item.dpt;
                                _dptcomment = "All " + _sitedpt + " Workstations with SCCM Client installed"
                                    + " (" + _sitename + ")";
                                _dptcomment2 = "All " + _sitedpt2 + " Workstations with SCCM Client installed"
                                    + " (" + _sitename + ")";
                                _dptsched = startBox.Text + ", Every" + (hourSpan == 1 ? "" : " " + hourSpan) + " hour" + (hourSpan < 2 ? "" : "s");
                                _dptquery = "select SMS_R_System.ResourceId, SMS_R_System.ResourceType, SMS_R_System.Name,"
                                    + " SMS_R_System.SMSUniqueIdentifier, SMS_R_System.ResourceDomainORWorkgroup, SMS_R_System.Client from  SMS_R_System"
                                    + " where SMS_R_System.SystemOUName = \"" + _domainforest + "/" + _code + "/" + item.dpt + "/Workstations\""
                                    + " and SMS_R_System.OperatingSystemNameandVersion like \"%Workstation%\" and SMS_R_System.Client = 1";
                            }
                            if (item.isDpt)
                            {
                                if (getCollectionID(_codedomain).Length == 0 && getCollectionID(_codedomain2).Length > 0)
                                {
                                    _codedomain = _codedomain2;
                                    _sitedpt = _sitedpt2;
                                    _dptcomment = _dptcomment2;
                                }
                                //Console.WriteLine("Suggest Create dpt: " + _codedomain + " :" + _sitedpt + " :" + _dptcomment + " :" + _dptsched + " :" + _dptquery);
                                newQueueItem("Create", _codedomain, _sitedpt, _dptcomment, _dptsched, _dptquery);
                            }
                            else if (item.isSite)
                            {
                                if (getCollectionID(_codedomain).Length == 0 && getCollectionID(_codedomain2).Length > 0)
                                    _codedomain = _codedomain2;
                                //Console.WriteLine("Suggest Create site: " + _domain + " :" + _codedomain + " :" + _sitecomment);
                                newQueueItem("Create", _domain, _codedomain, _sitecomment, "", "");
                            }
                            else if (item.isDomain)
                            {
                                //Console.WriteLine("Suggest Create domain: " + top + " :" + _domain + " :" + _domaincomment);
                                newQueueItem("Create", top, _domain, _domaincomment, "", "");
                            }
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine("suggest create error: " + ee.ToString());
                        }
                    }
                }

                Console.WriteLine("Check Active Directory / Collections Consistency done.");

            })).Start();
        }





        void check<XNode>(Tree<XNode> tree, Hashtable H)
        {
            check<XNode>(tree.tree.Nodes, tree, H);
        }

        void check<XNode>(TreeNodeCollection nodes, Tree<XNode> tree, Hashtable H)
        {
            foreach (TreeNode node in nodes)
                try
                {
                    string path = node.FullPath;
                    if (tree is Tree<ColNode>)
                    {
                        //Console.WriteLine(tree.name + ": " + path);
                        path = Regex.Replace(path, @"([^\\ ]+)(?:\\([^\\ ]+)(?: [^\\ ]+)?){2} ([^\\]+)", @"$1\$2\$3");
                        path = Regex.Replace(path, @"([^\\ ]+)(?:\\([^\\ ]+)(?: [^\\ ]+)?){1}", @"$1\$2");
                        //Console.WriteLine("   " + tree.name + ": " + path);
                    }
                    //string value = node.Text;
                    //if (XNode is ColNode)
                    //    value = ((ColNode)tree.node2item[node]).CollectionID;
                    //Console.WriteLine(newpath);
                    if (!H.Contains(path))
                    {
                        H.Add(path, node.FullPath);
                        Console.WriteLine(tree.name + ": added: [" + path + "] = " + node.FullPath);
                    }
                    else
                    {
                        Console.WriteLine(tree.name + ": already: [" + path + "] = " + H[path] + " / discarded = " + node.FullPath);
                    }
                    check(node.Nodes, tree, H);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
        }

        private void PrepareTemplateDptsCreationButton_Click(object sender, EventArgs e)
        {

            Console.WriteLine("\nPrepare Dpt Creation...");
            if (
                ADDptBox.CheckedItems.Count > 0
                && BaseColIDBox.Text.Length > 0
                && DptColIDBox.Text.Length > 0)
            {
                string ww = DptColBox.Text;
                foreach (string dpt in ADDptBox.CheckedItems)
                {
                    string wwdpt = "WW-" + dpt;
                    string dptcomment = "Worldwide all " + dpt + " Workstations with SCCM Client installed";
                    int dptHourSpan = 24; try {dptHourSpan = int.Parse(dptHourSpanBox.Text);} catch(Exception ee) {
                        Console.WriteLine("Invalid dptSpanHour: " + dptHourSpanBox.Text);
                    }
                    string dptsched = dptStartBox.Text + ", Every" + (dptHourSpan == 1 ? "" : " " + dptHourSpan) + " hour" + (dptHourSpan < 2 ? "" : "s");
                    string dptquery = "select SMS_R_System.ResourceId, SMS_R_System.ResourceType, SMS_R_System.Name,"
                            + " SMS_R_System.SMSUniqueIdentifier, SMS_R_System.ResourceDomainORWorkgroup, SMS_R_System.Client from  SMS_R_System"
                            + " where SMS_R_System.SystemOUName like \"%/" + dpt + "/Workstations\""
                            + " and SMS_R_System.OperatingSystemNameandVersion like \"%Workstation%\" and SMS_R_System.Client = 1";
                    string CollectionID = getCollectionID(wwdpt);
                    QueueItem dptq = newQueueItem(CollectionID.Length > 0 ? "Update" : "Create",
                        ww, /*getParentColID(ww),*/ wwdpt, dptcomment, dptsched, dptquery);
                }
                Console.WriteLine("Done.");
            }
            else
            {
                if (ADDptBox.CheckedItems.Count == 0)
                    Console.WriteLine("Prepare Template Dpts: Invalid parameters: No Templ. List must have at least one item checked.");
                if (BaseColIDBox.Text.Length == 0)
                    Console.WriteLine("Prepare Template Dpts: Invalid parameters: Base Collection not found: " + BaseColBox.Text);
                if (DptColIDBox.Text.Length == 0)
                    Console.WriteLine("Prepare Template Dpts: Invalid parameters: Dpts Collection not found: " + DptColBox.Text);
            }
        }



        string DeleteCollection(QueueItem q)
        {
            if (q.Action.Equals("Delete"))
            {
                if (getWqlConnectionManager() == null)
                {
                    q.Action = "Failure Delete. No Connection";
                    return "";
                }
                else
                {
                    q.CollectionID = DeleteCollection(getWqlConnectionManager(), q.CollectionID);
                    q.Action = (q.CollectionID.Length == 0 ? "Success" : "Failure") + " Delete";
                    return q.CollectionID;
                }
            }
            else
            {
                q.Action = "Invalid Action";
                return q.CollectionID = "";
            }
        }

        public static string DeleteCollection(WqlConnectionManager cnx, string CollectionID)
        {
            try
            {
                Console.WriteLine("WQL: Deleting a Collection... " + CollectionID);
                IResultObject col = cnx.GetInstance(@"SMS_Collection.CollectionID='" + CollectionID + "'");
                if (col == null) throw new Exception("Collection not found.");
                col.Delete();
                Console.WriteLine("WQL: Deleting a Collection... done.");
                return "";
            }
            catch (SmsException ex)
            {
                Console.WriteLine("WQL: Deleting a Collection: " + CollectionID + ": Failure: " + ex.Message);
                Console.WriteLine(ex.ToString());
                return CollectionID;
            }
        }

        bool UpdateCollection(QueueItem q)
        {
            if (q.Action.Equals("Update"))
            {
                if (getWqlConnectionManager() == null)
                {
                    q.Action = "Failure Update. No Connection";
                    return false;
                }
                else
                {
                    bool success = UpdateCollection(getWqlConnectionManager(), q.CollectionID,
                        q.Name, q.Comment, q.QueryRule, q.Schedule);
                    q.Action = (success ? "Success" : "Failure") + " Update";
                    return true;
                }
            }
            else
            {
                q.Action = "Invalid Action";
                return false;
            }
        }

        public static bool UpdateCollection(WqlConnectionManager cnx, string CollectionID,
            string Name, string Comment, string queries, string sched)
        {
            List<string> queries_ = new List<string>();
            foreach (string q in queries.Split(new char[] { '¤' })) queries_.Add(q.Trim());


            string ruleName = Name;
            try
            {
                Console.WriteLine("WQL: Updating a Collection... " + CollectionID);
                IResultObject col = cnx.GetInstance(@"SMS_Collection.CollectionID='" + CollectionID + "'");
                if (col == null) throw new Exception("Collection not found.");


                Console.WriteLine("Name: " + Name);
                col["Name"].StringValue = Name;
                Console.WriteLine("Comment: " + Comment);
                col["Comment"].StringValue = Comment;

                if (sched.Length > 0)
                {
                    List<IResultObject> schedToken = getRecurringScheduleToken(cnx, sched);
                    if (schedToken != null)
                    {
                        Console.WriteLine("Schedule: Periodic");
                        col["RefreshType"].IntegerValue = 2; //PERIODIC
                        col.SetArrayItems("RefreshSchedule", schedToken);
                    }
                    else
                    {
                        Console.WriteLine("Schedule: Manual");
                        col["RefreshType"].IntegerValue = 1; //MANUAL
                    }
                }
                else
                {
                    Console.WriteLine("Schedule: Manual");
                    col["RefreshType"].IntegerValue = 1; //MANUAL
                    col.SetArrayItems("RefreshSchedule", new List<IResultObject>());
                }

                // Save the new collection object and properties.
                // In this case, it seems necessary to 'get' the object again to access the properties.
                col.Put(); col.Get();
                Console.WriteLine("WQL: Updating a Collection... done.");



                Console.WriteLine("WQL: Replacing queries...");
                //get the collectionrules           
                List<IResultObject> rules = col.GetArrayItems("CollectionRules");
                Console.WriteLine("WQL: queries count: " + (rules == null ? 0 : rules.Count));
                if (rules != null) foreach (IResultObject rule in rules)
                    {
                        string RuleName = rule["RuleName"].StringValue;
                        Console.WriteLine("WQL: Removing membership rule " + RuleName + "...");
                    
                        if (rule.PropertyNames.Contains("ResourceID"))//SMS_CollectionRuleDirect
                        {
                            IResultObject delRule = cnx.CreateInstance("SMS_CollectionRuleDirect");
                            delRule["ResourceID"].IntegerValue = rule["ResourceID"].IntegerValue;

                            Dictionary<string, object> inParams = new Dictionary<string, object>();
                            inParams.Add("collectionRule", delRule);
                            col.ExecuteMethod("DeleteMembershipRule", inParams);
                            Console.WriteLine("WQL: Rule direct " + RuleName + " membership removed.");
                        }
                        else if (rule.PropertyNames.Contains("QueryExpression"))//SMS_CollectionRuleQuery
                        {
                            IResultObject delRule = cnx.CreateInstance("SMS_CollectionRuleQuery");
                            delRule["QueryExpression"].StringValue = rule["QueryExpression"].StringValue;
                            delRule["RuleName"].StringValue = rule["LimitToCollectionID"].StringValue;
                            delRule["QueryID"].IntegerValue = rule["QueryID"].IntegerValue;


                            Dictionary<string, object> inParams = new Dictionary<string, object>();
                            inParams.Add("collectionRule", delRule);
                            col.ExecuteMethod("DeleteMembershipRule", inParams);
                            Console.WriteLine("WQL: Rule query " + RuleName + " membership removed.");
                        }
                        else
                        {
                            Console.WriteLine("WQL: Rule " + RuleName + " membership not removed.");
                        }
                    }

                int qi = 1;
                foreach (string query in queries_)
                {
                    Console.WriteLine("WQL: Adding a query...");
                    Console.WriteLine("Query: " + query);
                    Dictionary<string, object> validateQueryParameters = new Dictionary<string, object>();
                    validateQueryParameters.Add("WQLQuery", queries_);///////////////
                    IResultObject result = cnx.ExecuteMethod("SMS_CollectionRuleQuery", "ValidateQuery", validateQueryParameters);
                    Console.WriteLine("WQL: Adding a query... validated.");
                    IResultObject newQueryRule = cnx.CreateInstance("SMS_CollectionRuleQuery");
                    string ruleName_i = ruleName + (qi > 1 ? " " + qi : ""); qi++;
                    Console.WriteLine("RuleName: " + ruleName_i);
                    newQueryRule["RuleName"].StringValue = ruleName_i;
                    newQueryRule["QueryExpression"].StringValue = query;
                    Console.WriteLine("WQL: Adding a query " + ruleName_i + " ... created.");

                    Dictionary<string, object> addMembershipRuleParameters = new Dictionary<string, object>();
                    addMembershipRuleParameters.Add("collectionRule", newQueryRule);
                    string queryID = col.ExecuteMethod("AddMembershipRule", addMembershipRuleParameters)["QueryID"].StringValue;
                    Console.WriteLine("WQL: Adding a query" + ruleName_i + "... done.");
                    Console.WriteLine("QueryID: " + queryID);
                    
                }

                Console.WriteLine("WQL: Refreshing the new Collection...");
                Dictionary<string, object> requestRefreshParameters = new Dictionary<string, object>();
                requestRefreshParameters.Add("IncludeSubCollections", false);
                col.ExecuteMethod("RequestRefresh", requestRefreshParameters);



                Console.WriteLine("WQL: Updating a Collection: " + Name + ": Success: " + CollectionID);
                return true;
            }
            catch (SmsException e)
            {
                Console.WriteLine("WQL: Updating a Collection: " + CollectionID + ": Failure: " + e.Message);
                Console.WriteLine(e.ToString());
                return false;
            }
            catch (ManagementException e)
            {
                string extendedError = e.Message + ". " + e.ErrorCode.ToString();
                try{
                    string desc = e.ErrorInformation["Description"].ToString();
                    extendedError += "\n" + desc;
                }
                catch (Exception eee)
                {
                    //Console.WriteLine(eee.ToString());
                }
                Console.WriteLine("WQL: Updating a Collection: " + CollectionID + ": Failure: " + extendedError);
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        string CreateCollection(QueueItem q)
        {
            if (q.Action.Equals("Create"))
            {
                if (getWqlConnectionManager() == null)
                {
                    q.Action = "Failure Create. No Connection";
                    return "";
                }
                else
                {
                    q.CollectionID = CreateCollection(getWqlConnectionManager(),
                        q.ParentColID, q.Name, q.Comment, q.QueryRule, q.Schedule);
                    q.Action = (q.CollectionID.Length > 0 ? "Success" : "Failure") + " Create";
                    return q.CollectionID;
                }
            }
            else
            {
                q.Action = "Invalid Action";
                return q.CollectionID = "";
            }
        }


        static string print(IResultObject o)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string p in o.PropertyNames) try
                {
                    sb.AppendLine(p + "=" + o.Properties[p].StringValue);
                }
                catch (Exception e)
                {
                    sb.AppendLine(p + "=?");
                }
            return sb.ToString();
        }


        public static string CreateCollection(WqlConnectionManager cnx, string ParentColID,
            string Name, string Comment, string query, string sched)
        {
            bool OwnedByThisSite = true;
            string ruleName = Name;
            try
            {
                Console.WriteLine("WQL: Creating a new Collection...");
                IResultObject col = cnx.CreateInstance("SMS_Collection");

                Console.WriteLine("Name: " + Name);
                col["Name"].StringValue = Name;
                Console.WriteLine("Comment: " + Comment);
                col["Comment"].StringValue = Comment;
                Console.WriteLine("OwnedByThisSite: " + OwnedByThisSite);
                col["OwnedByThisSite"].BooleanValue = OwnedByThisSite;

                if (sched.Length > 0)
                {
                    List<IResultObject> schedToken = getRecurringScheduleToken(cnx, sched);
                    if (schedToken != null)
                    {
                        Console.WriteLine("Schedule: Periodic");
                        col["RefreshType"].IntegerValue = 2; //PERIODIC
                        col.SetArrayItems("RefreshSchedule", schedToken);
                    }
                    else
                    {
                        Console.WriteLine("Schedule: Manual");
                        col["RefreshType"].IntegerValue = 1; //MANUAL
                    }
                }
                else
                {
                    Console.WriteLine("Schedule: Manual");
                    col["RefreshType"].IntegerValue = 1; //MANUAL
                }

                // Save the new collection object and properties.
                // In this case, it seems necessary to 'get' the object again to access the properties.
                col.Put(); col.Get();
                Console.WriteLine("WQL: Creating a new Collection... done.");


                Console.WriteLine("WQL: Linking the new Collection...");
                IResultObject link = cnx.CreateInstance("SMS_CollectToSubCollect");
                // Define parent relationship (in this case, off the main collection node).
                Console.WriteLine("parentCollectionID: " + ParentColID);
                link["parentCollectionID"].StringValue = ParentColID;
                string CollectionID = col["CollectionID"].StringValue;
                Console.WriteLine("CollectionID: " + CollectionID);
                link["subCollectionID"].StringValue = CollectionID;
                link.Put();
                Console.WriteLine("WQL: Linking the new Collection... done.");

                Console.WriteLine("WQL: Adding a query...");
                Console.WriteLine("Query: " + query);

                bool addquery = false;
                if (query.Length == 0)
                    Console.WriteLine("WQL: Adding a query... aborted. (empty query)");
                else try
                {
                    Dictionary<string, object> validateQueryParameters = new Dictionary<string, object>();
                    validateQueryParameters.Add("WQLQuery", query);
                    IResultObject result = cnx.ExecuteMethod("SMS_CollectionRuleQuery", "ValidateQuery", validateQueryParameters);
                    if (result["ReturnValue"].BooleanValue == true)
                    {
                        Console.WriteLine("WQL: Adding a query... validated.");
                        addquery = true;
                    }
                    else
                    {
                        Console.WriteLine("WQL: Adding a query... aborted. (invalid query)");
                    }
                }
                catch (SmsException ex)
                {
                    Console.WriteLine("WQL: Adding a query... aborted. (invalid query) " + ex.Message);
                }                

                if (addquery)
                {
                    IResultObject newQueryRule = cnx.CreateInstance("SMS_CollectionRuleQuery");
                    Console.WriteLine("RuleName: " + ruleName);
                    newQueryRule["RuleName"].StringValue = ruleName;
                    newQueryRule["QueryExpression"].StringValue = query;
                    Console.WriteLine("WQL: Adding a query... created.");

                    Dictionary<string, object> addMembershipRuleParameters = new Dictionary<string, object>();
                    addMembershipRuleParameters.Add("collectionRule", newQueryRule);
                    string queryID = col.ExecuteMethod("AddMembershipRule", addMembershipRuleParameters)["QueryID"].StringValue;
                    Console.WriteLine("WQL: Adding a query... done.");
                    Console.WriteLine("QueryID: " + queryID);
                }

                Console.WriteLine("WQL: Refreshing the new Collection...");
                Dictionary<string, object> requestRefreshParameters = new Dictionary<string, object>();
                requestRefreshParameters.Add("IncludeSubCollections", false);
                col.ExecuteMethod("RequestRefresh", requestRefreshParameters);

                Console.WriteLine("WQL: Creating a new Collection: " + Name + ": Success: " + CollectionID);
                return CollectionID;
            }

            catch (SmsException ex)
            {
                Console.WriteLine("WQL: Creating a new Collection: " + Name + ": Failure: " + ex.Message);
                Console.WriteLine(ex.ToString());
                return "";
            }
        }


        public static List<IResultObject> getRecurringScheduleToken(WqlConnectionManager connection, string sched)
        {
            Console.WriteLine("WQL: Parsing a Recurring Schedule String...");
            Console.WriteLine("Schedule: " + sched);

            DateTime today = DateTime.UtcNow;
            today = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
            if (Regex.IsMatch(sched, @"(\d{2}):(\d{2}), Everyday", RegexOptions.IgnoreCase))
                try {
                    int h = int.Parse(Regex.Replace(sched, @"(\d{2}):(\d{2}), Everyday", "$1", RegexOptions.IgnoreCase));
                    int m = int.Parse(Regex.Replace(sched, @"(\d{2}):(\d{2}), Everyday", "$2", RegexOptions.IgnoreCase));
                    string startTime = ManagementDateTimeConverter.ToDmtfDateTime(today.AddHours(h).AddMinutes(m));
                    return getRecurringScheduleToken(connection, 24, 0, startTime, true);
                } catch(Exception e)
                {
                    Console.WriteLine("Invalid ScheduleToken string: " + sched + ": " + e.Message);
                    return null;
                }
            else if (Regex.IsMatch(sched, @"(\d{2}):(\d{2}), Every (\d+) Hours", RegexOptions.IgnoreCase))
                try {
                    int h0 = int.Parse(Regex.Replace(sched, @"(\d{2}):(\d{2}), Every (\d+) Hours", "$3", RegexOptions.IgnoreCase));
                    int h = int.Parse(Regex.Replace(sched, @"(\d{2}):(\d{2}), Every (\d+) Hours", "$1", RegexOptions.IgnoreCase));
                    int m = int.Parse(Regex.Replace(sched, @"(\d{2}):(\d{2}), Every (\d+) Hours", "$2", RegexOptions.IgnoreCase));
                    string startTime = ManagementDateTimeConverter.ToDmtfDateTime(today.AddHours(h).AddMinutes(m));
                    return getRecurringScheduleToken(connection, h0, 0, startTime, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Invalid ScheduleToken string: " + sched + ": " + e.Message);
                    return null;
                }
            else if (Regex.IsMatch(sched, @"(\d{2}):(\d{2}), Every Hour", RegexOptions.IgnoreCase))
                try
                {
                    int h = int.Parse(Regex.Replace(sched, @"(\d{2}):(\d{2}), Every Hour", "$1", RegexOptions.IgnoreCase));
                    int m = int.Parse(Regex.Replace(sched, @"(\d{2}):(\d{2}), Every Hour", "$2", RegexOptions.IgnoreCase));
                    string startTime = ManagementDateTimeConverter.ToDmtfDateTime(today.AddHours(h).AddMinutes(m));
                    return getRecurringScheduleToken(connection, 1, 0, startTime, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Invalid ScheduleToken string: " + sched + ": " + e.Message);
                    return null;
                }
            else
            {
                Console.WriteLine("WQL: Parsing a Recurring Schedule String: Invalid pattern.");
                return null;
            }
        }


        public static List<IResultObject> getRecurringScheduleToken(WqlConnectionManager connection,
            int hourSpan, int hourDuration, string startTime, bool isGmt)
        {
            try
            {
                Console.WriteLine("WQL: Create a new Recurring Schedule Token...");
                Console.WriteLine("hourSpan: " + hourSpan);
                Console.WriteLine("hourDuration: " + hourDuration);
                Console.WriteLine("startTime: " + startTime + (isGmt ? " GMT" : ""));

                IResultObject recurInterval = connection.CreateEmbeddedObjectInstance("SMS_ST_RecurInterval");
                recurInterval["DaySpan"].IntegerValue = hourSpan / 24;
                recurInterval["HourSpan"].IntegerValue = hourSpan % 24;
                recurInterval["MinuteSpan"].IntegerValue = 0;
                recurInterval["DayDuration"].IntegerValue = hourDuration / 24;
                recurInterval["HourDuration"].IntegerValue = hourDuration % 24;
                recurInterval["MinuteDuration"].IntegerValue = 0;
                recurInterval["StartTime"].StringValue = startTime;
                recurInterval["IsGMT"].BooleanValue = isGmt;

                // Creating array to use as a parameters for the WriteToString method.
                List<IResultObject> scheduleTokens = new List<IResultObject>();
                scheduleTokens.Add(recurInterval);

                // Creating dictionary object to pass parameters to the WriteToString method.
                Dictionary<string, object> inParams = new Dictionary<string, object>();
                inParams["TokenData"] = scheduleTokens;

                // Initialize the outParams object.
                IResultObject outParams = null;

                // Call WriteToString method to decode the schedule token.
                outParams = connection.ExecuteMethod("SMS_ScheduleMethods", "WriteToString", inParams);

                // Output schedule token as an interval string.
                // Note: The return value for this method is always 0, so this check is just best practice.
                if (outParams["ReturnValue"].IntegerValue == 0)
                {
                    Console.WriteLine("WQL: Create a new Recurring Schedule Token: Success: " + outParams["StringData"].StringValue);
                }
                return scheduleTokens;
            }
            catch (SmsException ex)
            {
                Console.WriteLine("WQL: Create a new Recurring Schedule Token: Failed: " + ex.InnerException.Message);
                return null;
            }
        }

        public static string getCollectionMembers(string collectionID)
        {
            return getWqlConnectionManager() == null ? "" : getCollectionMembers(getWqlConnectionManager(), collectionID);
        }

        public static string getCollectionMembers(WqlConnectionManager connection, string collectionID)
        {
            try
            {
                IResultObject collection = connection.GetInstance(string.Format("SMS_Collection.CollectionID='{0}'", collectionID));
                string Members = collection["RefreshType"].IntegerValue.ToString();
                //Console.WriteLine(collectionID + " Members: " + Members);
                return Members;
            }
            catch (Exception e)
            {
                Console.WriteLine(collectionID + " Members: " + e.Message + "\n" + e.ToString());
                return "";
            }
        }


        public static string getCollectionQuery(string collectionID)
        {
            return getWqlConnectionManager() == null ? "" : getCollectionQuery(getWqlConnectionManager(), collectionID);
        }

        public static string getCollectionQuery(WqlConnectionManager connection, string collectionID)
        {
            try
            {
                IResultObject collection = connection.GetInstance(string.Format("SMS_Collection.CollectionID='{0}'", collectionID));
                StringBuilder sb = new StringBuilder();

                List<IResultObject> rules = collection.GetArrayItems("CollectionRules");
                if (rules != null) foreach (IResultObject rule in rules)
                {
                    string Rule = rule["QueryExpression"].StringValue;
                    if (sb.Length > 0) sb.Append("\n¤\n");
                    sb.Append(Rule);
                }
                //Console.WriteLine(collectionID + " Query: " + sb.ToString());
                return sb.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(collectionID + " Query: " + e.Message + "\n" + e.ToString());
                return "";
            }
        }


        public static string getCollectionSchedules(string collectionID)
        {
            return getWqlConnectionManager() == null ? "" : getCollectionSchedules(getWqlConnectionManager(), collectionID);
        }

        public static string getCollectionSchedules(WqlConnectionManager connection, string collectionID)
        {
            try
            {
                IResultObject collection = connection.GetInstance(string.Format("SMS_Collection.CollectionID='{0}'", collectionID));
                ManagementBaseObject[] Schedules = (ManagementBaseObject[])collection["RefreshSchedule"].ObjectArrayValue;

                StringBuilder sb = new StringBuilder();
                if (Schedules != null) foreach(ManagementBaseObject so in Schedules)
                {
                    SCCMCalc.SMS_ScheduleToken token = SCCMCalc.parseScheduleToken(so);
                    if (sb.Length > 0) sb.Append(" & ");
                    sb.Append(token.ToString());
                }
                string RefreshType = collection["RefreshType"].IntegerValue.ToString().Replace("1", "MANUAL").Replace("2", "PERIODIC");
                sb.Append(" (" + RefreshType + ")");

                //Console.WriteLine(collectionID + " Schedule: " + sb.ToString());
                return sb.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(collectionID + " Schedule: " + e.Message + "\n" + e.ToString());
                return "";
            }
        }

        static WqlConnectionManager WQLCON = null;
        public static WqlConnectionManager getWqlConnectionManager()
        {
            if (WQLCON == null) WQLCON = newWqlConnectionManager();
            return WQLCON;
        }

        static WqlConnectionManager newWqlConnectionManager()
        {
            string serverName = AuditSec.settings.smssrv;
            //scope = new ManagementScope(@"\\" + AuditSec.settings.smssrv + @"\Root\sms\" + AuditSec.settings.smssite);

            try
            {
                SmsNamedValuesDictionary namedValues = new SmsNamedValuesDictionary();
                WqlConnectionManager connection = new WqlConnectionManager(namedValues);
                connection.Connect(serverName);
                Console.WriteLine("WQL: Connection success."
                    + "\n" + connection.ConnectionScope);
                return connection;
            }
            catch (SmsException e)
            {
                Console.WriteLine("WQL: Failed to Connect. Error: " + e.Message);
                return null;
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("WQL: Failed to authenticate. Error:" + e.Message);
                return null;
            }
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (msg.Msg == 256)
            {
                if (keyData == Keys.F12)
                {

                    Console.WriteLine("F12 was pressed! ");
                    AuditSec.ShowLogs();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SCCMTree_NodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e)
        {
            if (e.Node != null && SCCM_TREE != null)
            {
                ColNode colNode = (ColNode)SCCM_TREE.node2item[e.Node]; if (colNode != null)
                {
                    string Schedules = colNode.getSchedules(true);
                    if (Schedules.EndsWith("(MANUAL)")) Schedules = "(MANUAL)";
                    Schedules = Schedules.Replace(" (PERIODIC)", "");
                    ToolTip.ToolTipTitle = colNode.Name;
                    ToolTip.SetToolTip(SCCMTree,
                        "Comment: " + colNode.Comment
                        + "\nSchedule: " + Schedules
                        + "\nCollectionID: " + colNode.CollectionID);
                }
            }

        }

        private void TemplDptBox_MouseHover(object sender, EventArgs e)
        {
            if (curTemplDpt != null && DPT_TREE != null)
            {
                string Name = "WW-" + curTemplDpt;
                TreeNode node = null; foreach (TreeNode node2 in DptTree.Nodes) if (node2.Text.Equals(Name)) { node = node2; break; } if (node != null)
                {
                    ColNode colNode = (ColNode)DPT_TREE.node2item[node]; if (colNode != null)
                    {
                        string Schedules = colNode.getSchedules(true);
                        if (Schedules.EndsWith("(MANUAL)")) Schedules = "(MANUAL)";
                        Schedules = Schedules.Replace(" (PERIODIC)", "");
                        ToolTip.SetToolTip(TemplDptBox,
                            "CollectionID: " + colNode.CollectionID
                            + "\nName: " + colNode.Name
                            + "\nComment: " + colNode.Comment
                            + "\nSchedules: " + Schedules);
                    }
                }
            }
        }

        string curTemplDpt = null;
        private void TemplDptBox_MouseMove(object sender, MouseEventArgs e)
        {
            int i = TemplDptBox.IndexFromPoint(e.Location);
            curTemplDpt = i < 0 ? null : TemplDptBox.Items[i].ToString();
        }

        private void ResetSchedule_Click(object sender, EventArgs e)
        {
            if (SCCM_TREE == null) return;
            if (SCCMTree.SelectedNode == null) return;

            int hourSpan = 24; try { hourSpan = int.Parse(hourSpanBox.Text); }
            catch (Exception ee)
            {
                Console.WriteLine("Invalid hourSpan: " + hourSpanBox.Text);
            }
            string dptsched = startBox.Text + ", Every" + (hourSpan == 1 ? "" : " " + hourSpan) + " hour" + (hourSpan < 2 ? "" : "s");


            foreach (TreeNode node in getSubNodes(SCCMTree.SelectedNode, null))
            {
                ColNode item = (ColNode)SCCM_TREE.node2item[node];
                ColNode parent = node.Parent == null ? null : (ColNode)SCCM_TREE.node2item[node.Parent];
                string oldsched = item.getSchedules(false).Replace(" (PERIODIC)", "").Replace(" (MANUAL)", "");
                string newsched = item.depth == 3 ? dptsched.Replace(",", " GMT,") : "";
                if (!newsched.ToLower().Equals(oldsched.ToLower()))
                {
                    Console.WriteLine("Reschedule Collection " + item.Name
                        + " -- Old Schedule: " + oldsched
                        + " -- New Schedule: " + newsched);
                    add(Queue, new object[] {
                        "Update",
                        parent == null ? BaseColBox.Text : parent.Name,
                        parent == null ? BaseColIDBox.Text : parent.CollectionID,
                        item.Name,
                        item.Comment,
                        dptsched,
                        item.getQuery(false),
                        item.CollectionID
                    });
                }
            }
        }

        List<TreeNode> getSubNodes(TreeNode node, List<TreeNode> nodes)
        {
            if (nodes == null) nodes = new List<TreeNode>();
            nodes.Add(node);
            foreach (TreeNode node2 in node.Nodes) getSubNodes(node2, nodes);
            return nodes;
        }


    }
}
