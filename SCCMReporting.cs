using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Management;
using Microsoft.VisualBasic;
using System.Data.OleDb;
using System.Reflection;
using System.DirectoryServices;
using System.Collections;
using System.Security.Principal;
using System.DirectoryServices.ActiveDirectory;
using System.Threading;
using System.Web.UI;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Drawing.Imaging;



namespace AuditSec
{
    public partial class SCCMReporting : Form
    {

        AuditSecGUIForm parent;
        public System.Windows.Forms.TextBox OUMaskBox = null;
        UsersInfo usersInfo = new UsersInfo();


        //string queriespath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\sccmqueries.txt";
        string queriespath = Path.Combine(Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "MYCOMPANY"), "sccmqueries.wql");
        string queriesnew = Path.Combine(Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "MYCOMPANY"), "sccmqueries.wql.new");
        string queriestmp = Path.Combine(Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "MYCOMPANY"), "sccmqueries.wql.tmp");
        string queriesbak = Path.Combine(Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "MYCOMPANY"), "sccmqueries.wql.bak");
        string sccmdict = Path.Combine(Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "MYCOMPANY"), "sccmdict.txt");
        string queriessched = Path.Combine(Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "MYCOMPANY"), "sccmqueries.sched");

        long getFileVersion(string path)
        {
            try {
                string line = File.ReadAllLines(path)[0].Trim();
                if (!line.StartsWith("#")) throw new Exception();
                return Convert.ToInt64(line.Substring(1).Trim());
            } catch(Exception e)
            {
                return 0;
            }
        }

        bool DIRXMLdisabled = false;
        public string progver = "?";
        string[] dictComboItems = null;

        public SCCMReporting(ref System.Windows.Forms.TextBox OUMaskBox, AuditSecGUIForm parent)
            : this()
        {
            this.parent = parent;
            this.OUMaskBox = OUMaskBox;
        }
        public SCCMReporting()
        {
            Console.WriteLine("Instanciating new SCCM Reporting module...");


            if (Environment.Is64BitOperatingSystem)
            {
                if (!AuditSec.exportResource("SciLexer64", Environment.SystemDirectory + @"\SciLexer64.dll", false))
                    throw new Exception("File installation failure");
            }
            else
            {
                if (!AuditSec.exportResource("SciLexer", Environment.SystemDirectory + @"\SciLexer.dll", false))
                    throw new Exception("File installation failure");
            }

            if (!AuditSec.exportResource("sccmdict", sccmdict, true))
                throw new Exception("File installation failure");

            if (!AuditSec.exportResource("sccmqueries", queriestmp, true))
                throw new Exception("File installation failure");
            long queriestmpv = File.Exists(queriestmp) ? getFileVersion(queriestmp) : 0;
            long queriesnewv = File.Exists(queriesnew) ? getFileVersion(queriesnew) : 0;

            InitializeComponent();
            jobButton.Text = "";
            progver = "v" + (AuditSec.curver.EndsWith(".0") ? AuditSec.curver.Substring(0, AuditSec.curver.Length - 2) : AuditSec.curver);
            this.Text += " " + progver;
            showOptions(false, false);
            editor.Margins[0].Width = 20;
            editor.IsBraceMatching = true;
            editor.Whitespace.Mode = ScintillaNET.WhitespaceMode.VisibleAfterIndent;
            editor.LineWrapping.Mode = ScintillaNET.LineWrappingMode.Word;
            editor.Caret.HighlightCurrentLine = true;
            editor.Caret.CurrentLineBackgroundColor = Color.LightYellow;
            editor.ConfigurationManager.Language = "mssql";
            editor.ConfigurationManager.Configure();

            //args2.Margins[0].Width = 20;
            args2.IsBraceMatching = true;
            args2.Whitespace.Mode = ScintillaNET.WhitespaceMode.VisibleAlways;
            args2.LineWrapping.Mode = ScintillaNET.LineWrappingMode.Word;
            args2.Caret.HighlightCurrentLine = true;
            args2.BackColor = System.Drawing.SystemColors.Control;
            args2.Caret.CurrentLineBackgroundColor = Color.LightYellow;
            args2.ConfigurationManager.Language = "mssql";
            args2.ConfigurationManager.Configure();

            loadDictionary();
            dictComboItems = new string[DICT.Count];
            DICT.Keys.CopyTo(dictComboItems, 0);
            dictCombo.Items.Add("");
            dictCombo.Items.AddRange(dictComboItems);

            pubBox.Text = AuditSec.settings.smspub.Trim().ToLower();
            timeBox.Text = AuditSec.settings.smstime.Replace(":", "").Trim();
            Console.WriteLine("Schedule@" + timeBox.Text);
            if (timeBox.Text.Replace(":", "").Trim().Length == 0)
            {
                timeBox.Text = "0600";
                Console.WriteLine("Schedule@" + timeBox.Text);
            }

            ADSBox.Items.AddRange(MachineInfo.getPropertyNames());
            for (int i = 0; i < ADSBox.Items.Count; i++) ADSBox.SetItemChecked(i, true);
            DIRXMLBox.Items.AddRange(UsersInfo.DIRXMLalias);
            for (int i = 0; i < DIRXMLBox.Items.Count; i++) DIRXMLBox.SetItemChecked(i, true);

            WQL.Visible = false;


            //Console.WriteLine("queriestmpv " + queriestmpv);
            //Console.WriteLine("queriesnewv " + queriesnewv);
            if (queriestmpv > queriesnewv)
            {
                string YES = Interaction.InputBox("New queries definitions are available.\n\n"
                    + "v" + queriesnewv + " -> v" + queriestmpv + "\n\n                    Merge ?",
                    "Queries definitions", "Yes");
                if (YES != null && YES.ToUpper().Equals("YES"))
                {

                    string NO = Interaction.InputBox("Clear all previously saved queries definitions ?\n\n"
                    + " * It is advised to answer 'Yes' to avoid old/new queries mix.\n"
                    + " * Please answer 'No' if you authored your own queries and want to retain them.",
                    "Queries definition", "No");

                    loadQueries(false);
                    bool reloadSched = hasSched() && saveSched(false);
                    clearQueries();

                    if (NO != null && NO.ToUpper().Equals("NO")) loadQueries(true);

                    try
                    {
                        loadQueries(queriestmp, false, true);
                        querySaveAll();
                        if (File.Exists(queriesbak)) File.Delete(queriesbak);
                        if (File.Exists(queriesnew)) File.Move(queriesnew, queriesbak);
                        File.Move(queriestmp, queriesnew);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error while merging new queries definitions:\n" + e.ToString());
                        MessageBox.Show("Error while merging new queries definitions:\n" + e.Message);
                    }

                    if (reloadSched) loadSched(false);
                }
                else loadQueries(true);
            }
            else loadQueries(true);
            queryCombo.DrawMode = DrawMode.OwnerDrawFixed;

            SMSSERVER_Box.Items.AddRange(MYCOMPANY_Settings.SMS_SERVER_SITE_DESC.Select(row => row[0]).ToArray<object>());
            SMSSERVER_Box.SelectedIndex = 0;
            SMSSERVER_Box_Changed(this, null);


            online = new Online(RefreshRowHeader, () => pingBox.Checked);
            onlineWorker.RunWorkerAsync();

            new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(500);
                Invoke(new selectQueryDelegate(selectQuery), "___SCCM Reporting:README___");

                loadADS();
                statusLabel1.Text = @"SMS=\\" + AuditSec.settings.smssrv + @"\Root\sms\" + AuditSec.settings.smssite
                    + " - ADS=LDAP://" + new PrincipalContext(ContextType.Domain).ConnectedServer
                    + " - DIRXML=" + AuditSec.defaultLdap;
                scheduleWorker.RunWorkerAsync();
            })).Start();

            new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(2500);
                Invoke(new checkdirxmlDelegate(checkdirxml));
                Invoke(new browsepubDelegate(browsepub), true);
                Invoke(new checkScheduleStartDelegate(checkScheduleStart));

            })).Start();


            new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(60000);
                Console.WriteLine("Starting GC monitoring...");
                while(Visible)
                {
                    long totalKB = GC.GetTotalMemory(false) / 1024;
                    Console.WriteLine("GC: " + totalKB + "KB");
                    Thread.Sleep(30000);
                }
                Console.WriteLine("Ending GC monitoring.");
            })).Start();

        }

        public delegate void checkdirxmlDelegate();
        private void checkdirxml()
        {
            Console.WriteLine("Opening DirXML... ");
            if (AuditSec.settings.picpw != null && AuditSec.settings.picpw.Length == 0) AuditSec.settings.picpw = null;
            DIRXMLdisabled = UsersInfo.getDIRXMLAttributes(UserPrincipal.Current.SamAccountName) == null;
        }

        public delegate void browsepubDelegate(bool check);
        void browsepub(bool check)
        {
            string path;
            if (check) //just checking path defined
            {
                path = pubBox.Text;
                if (path.Length > 0 && Directory.Exists(path)) return;
                else browsepub(false); 
            }
            else //ask path
            {
                path = Interaction.InputBox("Paste the Publication path here:\n    - or cancel to Browse -", "Publication path").ToLower();
                if (path.Length > 0 && Directory.Exists(path))
                    pubBox.Text = path;
                else
                {
                    path = pubBox.Text;
                    if (path.Length > 0 && Directory.Exists(path)) pubFolderBrowserDialog.SelectedPath = path;
                    pubFolderBrowserDialog.ShowDialog();
                    pubBox.Text = pubFolderBrowserDialog.SelectedPath.ToLower();
                } 
                
            }
        }

        void saveSettings(bool disposing)
        {
            saveChanges(false);
            if (parent == null) AuditSec.saveSettings_SMSDIRXML(pubBox.Text, timeBox.Text);

            if (disposing && (components != null)) components.Dispose();
            //base.Dispose(disposing);

            if (parent == null) AuditSec.Exit("SCCM Reporting module ended.",
                () => { base.Dispose(disposing); return true; });
        }

        void enableGUI(bool enable)
        {           
            filteredDomainBox.Visible = !enable;
            filteredOUBox.Visible = !enable;
            filteredDptBox.Visible = !enable;
            filteredWUBox.Visible = !enable;
            filteredADSBox.Visible = !enable;
            filteredDIRXMLBox.Visible = !enable;
            filteredQueryBox.Visible = !enable;
            filteredArgsBox.Visible = !enable;
            filteredLargeBox.Visible = !enable;
            filteredFreqBox.Visible = !enable;
            //filteredSplitBox.Visible = !enable;

            domainBox.Enabled = enable;
            OUBox.Enabled = enable;
            dptBox.Enabled = enable;
            WUBox.Enabled = enable;

            WQL.Visible = !enable;
            ADSBox.Enabled = enable;
            DIRXMLBox.Enabled = enable && !DIRXMLdisabled;
            startButton.Enabled = enable;
            startButton2.Enabled = enable;
            exportButton.Enabled = enable;

            argsButton.Enabled = enable;
            templateLevelBox.Enabled = enable;
            saveButton.Enabled = enable;
            saveasButton.Enabled = enable;
            newButton.Enabled = enable;
            renButton.Enabled = enable;
            delButton.Enabled = enable;

            editor.Enabled = enable;
            args.Enabled = enable;
            queryCombo.Enabled = enable;

            previewBox.Enabled = enable;
            preview2Box.Enabled = enable;
            largeBox.Enabled = enable;
            large2Box.Enabled = enable;

            freqBox.Enabled = enable;
            timeBox.Enabled = enable;
            schedBox.Enabled = enable;
            splitBox.Enabled = enable;

            browseButton.Enabled = enable;
            notifyBox.Enabled = enable;
            loadSchedButton.Enabled = enable;
            saveSchedButton.Enabled = enable;
            dictCombo.Enabled = enable;

            if (enable)
            {
                progressBar1.Maximum = 60;
                progressBar1.Value = 0;
            }
        }


        public delegate void selectQueryDelegate(string q);
        private void selectQuery(string q)
        {
            if (queryCombo.Items.Contains(q))
                queryCombo.Text = q;
        }

        public delegate void domainBoxItemsAddDelegate(Domain domain);
        private void domainBoxItemsAdd(Domain domain)
        {
            domainBox.Items.Add(domain);
            domainBox_Click(null, null);
        }

        public delegate void OUBoxItemsAddDelegate(string item, string path);
        private void OUBoxItemsAdd(string item, string path)
        {
            statusLabel1.Text = ("Loading ADS structure... " + path);
            ADSPATH.Add(path);
            bool hasLower = !item.ToUpper().Equals(item);
            if (!OUBox.Items.Contains(item))
                if (!hasLower) OUBox.Items.Add(item);
            OUBox_Click(null, null);
        }

        string[] dptSpecial = { "!Unknown-SCCM", "External" }; 
        public delegate void dptBoxItemsAddDelegate(string item, string path);
        private void dptBoxItemsAdd(string item, string path)
        {
            statusLabel1.Text = ("Loading ADS structure... " + path);
            ADSPATH.Add(path);
            bool hasLower = !item.ToUpper().Equals(item);
            bool special = dptSpecial.Contains(item);
            if (!dptBox.Items.Contains(item))
                if (!hasLower || special) dptBox.Items.Add(item);
        }


        public void setFilteredBoxes(bool invoke, bool unattended, string queryname, string queryargs,
            List<string> ADSChecked_, List<string> DIRXMLChecked_, List<string> splitColumns_,
            int largeChecked_, string notifyRecipients_, List<string> freqChecked_,
            List<string> domainChecked_, List<string> OUChecked_, List<string> dptChecked_, List<string> WUChecked_)
        {
            if (invoke) Invoke(new setFilteredBoxesDelegate(setFilteredBoxes), unattended, queryname, queryargs,
                 ADSChecked_, DIRXMLChecked_, splitColumns_,
                 largeChecked_, notifyRecipients_, freqChecked_,
                 domainChecked_, OUChecked_, dptChecked_, WUChecked_);
            else setFilteredBoxes(unattended, queryname, queryargs,
                 ADSChecked_ , DIRXMLChecked_, splitColumns_,
                 largeChecked_, notifyRecipients_, freqChecked_,
                 domainChecked_, OUChecked_, dptChecked_, WUChecked_);
        }
        public delegate void setFilteredBoxesDelegate(bool unattended, string queryname, string queryargs,
            List<string> ADSChecked_, List<string> DIRXMLChecked_, List<string> splitColumns_,
            int largeChecked_, string notifyRecipients_, List<string> freqChecked_,
            List<string> domainChecked_, List<string> OUChecked_, List<string> dptChecked_, List<string> WUChecked_);
        private void setFilteredBoxes(bool unattended, string queryname, string queryargs,
            List<string> ADSChecked_, List<string> DIRXMLChecked_, List<string> splitColumns_,
            int largeChecked_, string notifyRecipients_, List<string> freqChecked_,
            List<string> domainChecked_, List<string> OUChecked_, List<string> dptChecked_, List<string> WUChecked_)
        {
            filteredQueryBox.Text = queryname;
            filteredArgsBox.Text = queryargs;
            if (!unattended)
            {
                filteredADSBox.Text = ADSBox.CheckedItems.Count == 0 ? "*"
                    : ADSBox.CheckedItems.Cast<string>().Aggregate((x, y) => x + ", " + y);
                filteredDIRXMLBox.Text = DIRXMLdisabled ? "N/A" : DIRXMLBox.CheckedItems.Count == 0 ? "*"
                    : DIRXMLBox.CheckedItems.Cast<string>().Aggregate((x, y) => x + ", " + y);

                filteredDomainBox.Text = domainBox.CheckedItems.Count == 0 ? "*"
                    : domainBox.CheckedItems.Cast<Domain>().Select(x => x.Name).Aggregate((x, y) => x + ", " + y);
                filteredOUBox.Text = OUBox.CheckedItems.Count == 0 ? "*"
                    : OUBox.CheckedItems.Cast<string>().Aggregate((x, y) => x + ", " + y);
                filteredDptBox.Text = dptBox.CheckedItems.Count == 0 ? "*"
                    : dptBox.CheckedItems.Cast<string>().Aggregate((x, y) => x + ", " + y);
                filteredWUBox.Text = WUBox.CheckedItems.Count == 0 ? WUBox.Items[0].ToString()
                    : WUBox.CheckedItems.Cast<string>().Aggregate((x, y) => x + ", " + y);

                filteredFreqBox.Text = freqBox.CheckedItems.Count == 0 ? "-"
                    : freqBox.CheckedItems.Cast<string>().Aggregate((x, y) => x + ", " + y);
                largeChecked_ = (largeBox.CheckState == CheckState.Checked ? 1 : 0)
                    + (large2Box.CheckState == CheckState.Checked ? 1 : 0);
                filteredLargeBox.Text = largeChecked_ > 1 ? "5000+" : largeChecked_ > 0 ? "1000+" : "-";
                filteredRecipientsBox.Text = notifyRecipients;
                filteredSplitBox.Text = CX.SPLITCOLUMNS.Count == 0 ? "" : CX.SPLITCOLUMNS.Aggregate((x, y) => x + ", " + y);
            }
            else
            {
                filteredADSBox.Text = ADSChecked_.Count == 0 ? "*"
                    : ADSChecked_.Aggregate((x, y) => x + ", " + y);
                filteredDIRXMLBox.Text = DIRXMLdisabled ? "N/A" : DIRXMLChecked_.Count == 0 ? "*"
                    : DIRXMLChecked_.Aggregate((x, y) => x + ", " + y);

                filteredDomainBox.Text = domainChecked_.Count == 0 || domainChecked_.Count == domainBox.Items.Count ? "*"
                    : domainChecked_.Aggregate((x, y) => x + ", " + y);
                filteredOUBox.Text = OUChecked_.Count == 0 || OUChecked_.Count == OUBox.Items.Count ? "*"
                    : OUChecked_.Aggregate((x, y) => x + ", " + y);
                filteredDptBox.Text = dptChecked_.Count == 0 ? "*"
                    : dptChecked_.Aggregate((x, y) => x + ", " + y);
                filteredWUBox.Text = WUChecked_.Count == 0 ? WUBox.Items[0].ToString()
                    : WUChecked_.Aggregate((x, y) => x + ", " + y);

                filteredFreqBox.Text = freqChecked_.Count == 0 ? "-"
                    : freqChecked_.Aggregate((x, y) => x + ", " + y);
                filteredLargeBox.Text = largeChecked_ > 1 ? "5000+" : largeChecked_ > 0 ? "1000+" : "-";
                filteredRecipientsBox.Text = notifyRecipients_;
                filteredSplitBox.Text = splitColumns_.Count == 0 ? "" : splitColumns_.Aggregate((x, y) => x + ", " + y);
            }
        }


        public void setWQL(string text) //null clears and set not visible
        {
            setWQL(text, true);
        }

        public void setWQL(string text, bool invoke) //null clears and set not visible
        {
            if (invoke) Invoke(new setWQLTextDelegate(setWQLText), text == null ? "" : text, text != null, text != null);
            else setWQLText(text == null ? "" : text, text != null, text != null);
        }

        public delegate void setWQLTextDelegate(string text, bool append, bool visible);

        StringBuilder WQLText = new StringBuilder();

        private void setWQLText(string text, bool append, bool visible)
        {
            try
            {
                if (text != null)
                {
                    if (CX != null && CX.PARSER_DEBUG != null)
                    {
                        //CX.PARSER_DEBUG.AppendLine("\r\nWQL:" + text + "\r\n");
                        SCCMParser.DebugLine(CX, "\r\nWQL:" + text + "\r\n");
                    }
                    else Console.WriteLine("WQL: " + text);
                }


                if (append) {WQLText.Append(WQLText.Length > 0 ? "\r\n" : ""); WQLText.Append(text.Replace("\n", "\r\n"));}
                else {WQLText.Clear(); WQLText.Append(text);}
                int i = WQLText.Length, n = 0; bool fit = true; while (--i >= 0 && (fit = (n += WQLText[i] == '\n' ? 1 : 0) < 1000)) ;
                if (!fit) WQLText.Remove(0, i);
                WQL.Text = null; GC.Collect();
                WQL.Text = WQLText.ToString();

                /*
                if (append) WQL.Text += (WQL.Text.Length > 0 ? "\r\n" : "") + text.Replace("\n", "\r\n");
                else WQL.Text = text;
                int i = WQL.Text.Length, n = 0; bool fit = true; while (--i >= 0 && (fit = (n += WQL.Text[i] == '\n' ? 1 : 0) < 1000)) ;
                if (!fit) WQL.Text = WQL.Text.Substring(i);
                */

                WQL.SelectionStart = WQL.Text.Length;
                WQL.ScrollToCaret();
                WQL.Refresh();
                WQL.Visible = visible;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failure: WQL Logs: write to form: " + e.ToString());
            }

            /*
            if (CX != null && CX.PARSER_DEBUG != null)
                try
                {
                    SCCMParser.writeParserDebug(CX, this, false, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failure: WQL Logs: write to disk: " + e.ToString());
                }
             * */
        }



        private string getWQLselectionInvoke()
        {
            return (string)Invoke(new getWQLselectionDelegate(getWQLselection));
        }
        public delegate string getWQLselectionDelegate();
        private string getWQLselection()
        {
            return WQL.SelectedText;
        }

        private string getQueryArgsInvoke()
        {
            return (string)Invoke(new getQueryArgsDelegate(getQueryArgs));
        }
        public delegate string getQueryArgsDelegate();
        private string getQueryArgs()
        {
            return args.Text;
        }





        public void setThreads(string threads)
        {
            setThreads(threads, false);
        }
        public void setThreads(string threads, bool invoke)
        {
            if (invoke) Invoke(new setThreads_Delegate(setThreads_), threads);
            else setThreads_(threads);
        }
        public delegate void setThreads_Delegate(string threads);
        private void setThreads_(string threads)
        {
            threadsLabel1.Text = threads;
        }


        public string getStatus()
        {
            return statusLabel1.Text;
        }
        public void setStatus(string status)
        {
            setStatus(status, false);
        }
        public void setStatus(string status, bool invoke)
        {
            if (invoke) Invoke(new setStatus_Delegate(setStatus_), status);
            else setStatus_(status);
        }
        public delegate void setStatus_Delegate(string status);
        private void setStatus_(string status)
        {
            statusLabel1.Text = status;
        }


        bool freezeGUI = false;
        List<string> ADSPATH = new List<string>();

        public delegate void loadADSDelegate();
        void loadADS()
        {
            freezeGUI = true;
            //setWQL(null);
            setWQL("");
            setWQL("Please wait.");
            setWQL("");
            setWQL("Loading ADS structure...");
            Console.WriteLine("ADS OU Mask: " + (OUMaskBox == null ? AuditSec.defaultOUMask : OUMaskBox.Text));
            setStatus("Loading ADS structure...", true);
            try
            {
                DomainCollection dc = Forest.GetCurrentForest().Domains;
                Domain[] domains = new Domain[dc.Count]; dc.CopyTo(domains, 0);
                string current = Domain.GetCurrentDomain().Name;
                domains.AsParallel().ForAll(domain =>
                {
                    Invoke(new domainBoxItemsAddDelegate(domainBoxItemsAdd), domain);
                    ADSPATH.Add(domain.ToString().ToUpper());
                    setStatus("Loading ADS structure... " + domain.ToString().ToUpper(), true);
                    DirectoryEntry de = domain.GetDirectoryEntry();
                    DirectorySearcher ds = new DirectorySearcher(de, "(objectClass=organizationalUnit)", null, SearchScope.OneLevel);
                    ds.PropertiesToLoad.Add("name");
                    List<SearchResult> sr = new List<SearchResult>(); foreach (SearchResult r in ds.FindAll()) sr.Add(r);
                    sr.ToArray().AsParallel().ForAll(r =>
                    {
                        string ou = r.Properties["name"][0].ToString();
                        bool match = false; try
                        {
                            match = Regex.IsMatch(ou, (OUMaskBox == null ? AuditSec.defaultOUMask : OUMaskBox.Text),
                                     RegexOptions.IgnoreCase);
                        }
                        catch (Exception ee)
                        {
                            setWQL("OU=" + ou + " IsMatch Regex=" + (OUMaskBox == null ? AuditSec.defaultOUMask : OUMaskBox.Text) + " = " + ee.Message);
                        }
                        if (match)
                        {
                            Invoke(new OUBoxItemsAddDelegate(OUBoxItemsAdd), ou, domain.ToString().ToUpper() + "/" + ou);

                            DirectorySearcher ds2 = new DirectorySearcher(r.GetDirectoryEntry(), "(objectClass=organizationalUnit)",
                                new String[] { "name" }, SearchScope.OneLevel);
                            foreach (SearchResult r2 in ds2.FindAll())
                            {
                                string dpt = r2.Properties["name"][0].ToString();
                                Invoke(new dptBoxItemsAddDelegate(dptBoxItemsAdd), dpt, domain.ToString().ToUpper() + "/" + ou + "/" + dpt);
                            }
                        }
                    });
                });
            }
            catch (Exception e)
            {
                setWQL(e.ToString());
            }

            //loadStaffStatus = loadStaff();

            setWQL(null);
            freezeGUI = false;
        }


        Hashtable ITOps = null, ITOpsMgmt = null;
        const int loadStaffDone = 1, loadStaffError = -1, loadStaffNotDone = 0;
        int loadStaffStatus = loadStaffNotDone;

        int loadStaff()
        {
            try
            {
                setWQL("");
                setWQL("Loading ITOps Members...");
                ITOps = Staff.getMembers_ITOps(OUBox, x =>
                {
                    Console.WriteLine(x);
                    setStatus("Loading " + x, true);
                    return true;
                });
                ITOpsMgmt = Staff.getMembers_ITOpsMgmt(OUBox, x =>
                {
                    Console.WriteLine(x);
                    setStatus("Loading " + x, true);
                    return true;
                });
                MachineInfo.setITOps(ITOps, ITOpsMgmt);
                return loadStaffDone;
            }
            catch (Exception e)
            {
                setWQL(e.ToString());
                return loadStaffError;
            }
        }


        void setADSBoxCheckedItems(string value) {
            ADSBox.BeginUpdate();
            string[] checkedItems = value.Split(new char[]{'+'});
            for (int i = 0; i < ADSBox.Items.Count; i++)
                ADSBox.SetItemChecked(i, checkedItems.Contains(ADSBox.Items[i].ToString()));
            ADSBox.TopIndex = ADSBox.CheckedIndices.Count > 0 ? ADSBox.CheckedIndices[0] : 0;
            ADSBox.EndUpdate();
        }

        void setDIRXMLBoxCheckedItems(string value)
        {
            DIRXMLBox.BeginUpdate();
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < DIRXMLBox.Items.Count; i++)
                DIRXMLBox.SetItemChecked(i, checkedItems.Contains(DIRXMLBox.Items[i].ToString()));
            DIRXMLBox.TopIndex = DIRXMLBox.CheckedIndices.Count > 0 ? DIRXMLBox.CheckedIndices[0] : 0;
            DIRXMLBox.EndUpdate();

        }

        void setDomainBoxCheckedItems(string value)
        {
            domainBox.BeginUpdate();
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < domainBox.Items.Count; i++)
                domainBox.SetItemChecked(i, checkedItems.Contains(domainBox.Items[i].ToString()));
            domainBox.TopIndex = domainBox.CheckedIndices.Count > 0 ? domainBox.CheckedIndices[0] : 0;
            domainBox.EndUpdate();
            domainBox_Click(null, null);
        }

        void setOUBoxCheckedItems(string value)
        {
            OUBox.BeginUpdate();
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < OUBox.Items.Count; i++)
                OUBox.SetItemChecked(i, checkedItems.Contains(OUBox.Items[i].ToString()));
            OUBox.TopIndex = OUBox.CheckedIndices.Count > 0 ? OUBox.CheckedIndices[0] : 0;
            OUBox.EndUpdate();
            OUBox_Click(null, null);
        }

        void setDptBoxCheckedItems(string value)
        {
            dptBox.BeginUpdate();
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < dptBox.Items.Count; i++)
                dptBox.SetItemChecked(i, checkedItems.Contains(dptBox.Items[i].ToString()));
            dptBox.TopIndex = dptBox.CheckedIndices.Count > 0 ? dptBox.CheckedIndices[0] : 0;
            dptBox.EndUpdate();
            dptBox_Click(null, null);
        }

        void setWUBoxCheckedItems(string value)
        {
            WUBox.BeginUpdate();
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < WUBox.Items.Count; i++)
                WUBox.SetItemChecked(i, checkedItems.Contains(WUBox.Items[i].ToString()));
            //WUBox.TopIndex = WUBox.CheckedIndices.Count > 0 ? WUBox.CheckedIndices[0] : 0;
            WUBox.EndUpdate();
            WUBox_Click(null, null);
        }

        void setFreqBoxCheckedItems(string value)
        {
            freqBox.BeginUpdate();
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < freqBox.Items.Count; i++)
                freqBox.SetItemChecked(i, checkedItems.Contains(freqBox.Items[i].ToString()));
            freqBox.EndUpdate();
            freqBox_Click(null, null);
        }

        void setLargeBoxes(string value)
        {
            largeBox.CheckState = CheckState.Unchecked;
            large2Box.CheckState = CheckState.Unchecked;
            int large = 0;
            if (value.Trim().Length > 0) try
            {
                large = int.Parse(value.Trim());
            }
            catch(Exception e)
            {
                Console.WriteLine("Invalid Large value: " + value);
            }
            if (large > 0) largeBox.CheckState = CheckState.Checked;
            if (large > 1) large2Box.CheckState = CheckState.Checked;


            largeBox_Click(null, null);
        }

        string notifyRecipients = null;
        void setNotifyRecipients(string value)
        {
            notifyRecipients = value;
        }

        public class DictEntry
        {
            public string Table;
            public string Name;
            public string Type;
            public string Comment;
            public string Line;
            public DictEntry(string[] words, ref string TABLE)
            {
                if (words.Length < 2) throw new Exception("Invalid dict entry:" +words.Aggregate((x, y) => y + " " + x));
                Name = words[0];
                Type = words[1].ToUpper();
                Comment = words.Length > 2 ? words[2] : "";
                TABLE = Table = Type.Equals("TABLE") ? Name : TABLE;
                Line = Table + (Type.Equals("TABLE") ? "" : "." + Name);
                //Console.WriteLine(Line + " AS " + Type + " " + Comment);
            }
        }

        Hashtable DICT = new Hashtable();

        void loadDictionary()
        {
            try {
                string TABLE = null;
                if (File.Exists(sccmdict))
                        foreach(string line in File.ReadAllLines(sccmdict))
                            if (line.Trim().Length > 0)
                                {
                                    DictEntry entry = new DictEntry(
                                        line.Split(new char[]{'\t'}, 3, StringSplitOptions.RemoveEmptyEntries), ref TABLE);
                                    if (DICT.Contains(entry.Line)) DICT.Remove(entry.Line);
                                    DICT.Add(entry.Line, entry);
                                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Could not read SCCMDict.txt file: " + e.Message);
                Console.WriteLine(e.ToString());
            }
        }


        void loadQueries(bool show)
        {
            loadQueries(queriespath, true, show);
        }

        public Hashtable QUERIES = new Hashtable();
        void clearQueries()
        {
            QUERIES.Clear();
            queryCombo.Items.Clear();
        }
        void loadQueries(string path, bool clear, bool show)
        {
            if (clear)
            {
                clearQueries();
            }
            string lastline = "";
            int lastlinei = 0;
            try {
                string queryname = null;
                StringBuilder queryargs = new StringBuilder();
                StringBuilder querydef = new StringBuilder();
                if (File.Exists(path))
                    foreach (string line in File.ReadAllLines(path))
                    {
                        lastline = line;
                        lastlinei++;
                        if (!line.StartsWith("#") && line.Trim().Length > 0)
                        {
                            //Console.WriteLine("Loading queries: " + line);
                            if (line.StartsWith("¤"))
                            {
                                if (queryname != null && queryname.Length > 0
                                    && querydef != null && querydef.Length > 0)
                                    loadQueriesAccept(ref queryname, ref queryargs, ref querydef);
                                queryname = null;
                                queryargs.Clear();
                                querydef.Clear();
                            }
                            else
                            {
                                if (queryname == null)
                                    queryname = line;
                                else if (querydef.Length > 0)
                                    querydef.AppendLine(line);
                                else if ("SELECT".Equals(line.Split(new char[] { ' ', '\t' }, 2,
                                    StringSplitOptions.RemoveEmptyEntries)[0].ToUpper()))
                                    querydef.AppendLine(line);
                                else if ("WHERE".Equals(line.Split(new char[] { ' ', '\t' }, 2,
                                    StringSplitOptions.RemoveEmptyEntries)[0].ToUpper()))
                                    querydef.AppendLine(line);
                                else
                                    queryargs.Append(line + " ");
                            }
                        }
                    }
                if (queryname != null && queryname.Length > 0 && querydef != null && querydef.Length > 0)
                    loadQueriesAccept(ref queryname, ref queryargs, ref querydef);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot load query: " + e.Message + "\n" + lastlinei + ": " + lastline);
                setWQL("Cannot load query: " + e.Message + "\n" + lastlinei + ": " + lastline + "\n"
                    + e.StackTrace, false);
            }
            if (show) setWQL(QUERIES.Count + " query definitions loaded from file: \"" + path + "\"", false);
        }


        void loadQueriesAccept(ref string queryname, ref StringBuilder queryargs, ref StringBuilder querydef)
        {
            while (queryargs.Length > 0 && (
                queryargs[0] == ' ' || queryargs[0] == '\n'
                || queryargs[0] == '\t' || queryargs[0] == ','))
                queryargs.Remove(0, 1);
            if (QUERIES.ContainsKey(queryname)) QUERIES.Remove(queryname);
            else queryCombo.Items.Add(queryname);
            QUERIES.Add(queryname, new string[] { SCCMParser.removeArgDuplicates(queryargs.ToString()), querydef.ToString() });
Console.WriteLine("Loaded query [" + queryname + "]"
    //+ "\nargs (" + queryargs.ToString() + ")\n" + querydef.ToString()
    );
        }


        /*
        string removeArgDuplicates(string args_)
        {
            Hashtable h = new Hashtable();
            List<string> l = new List<string>();
            string[] args = args_.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (args == null) return "";
            foreach (string arg in args)
            {
                string[] kv = arg.Split(new char[] { '=' }, 2, StringSplitOptions.None);
                string k = kv.Length > 0 ? kv[0].Trim() : "";
                string v = kv.Length > 1 ? kv[1].Trim() : "";
                if (h.Contains(k))
                {
                    h.Remove(k);
                    l.Remove(k);
                }
                h.Add(k, v);
                l.Add(k);
            }
            StringBuilder sb = new StringBuilder();
            foreach (string k in l) sb.Append((sb.Length > 0 ? ", " : "") + k + "=" + h[k]);
            return sb.ToString();
        }
        */


        void startButtonPerformClick(bool invoke)
        {
            if (invoke) Invoke(new startButtonPerformClick_Delegate(startButtonPerformClick_));
            else startButtonPerformClick_();
        }
        public delegate void startButtonPerformClick_Delegate();
        void startButtonPerformClick_()
        {
            startButton.PerformClick();
        }


        private void queryButton_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;
            foldArgs();
            DEBUG = true;
            if (todo_enabled)
            {
                string queryname = todo_queryname, queryargs = todo_queryargs, querydef = todo_querydef;
                todo_hlate = -1; todo_enabled = false;
                todo_queryname = null; todo_queryargs = null; todo_querydef = null;
                queryStart(queryname, queryargs, querydef);
            }
            else
            {
                queryStart(null, null, null);
            }
        }

        private void queryStart(string queryname, string queryargs, string querydef)
        {
            bool unattended = queryname != null;
            queryname = unattended ? queryname : queryCombo.Text;
            saveChanges(unattended);
            filteredDomainBox.Text = "";
            filteredOUBox.Text = "";
            filteredDptBox.Text = "";
            filteredWUBox.Text = "";
            filteredADSBox.Text = "";
            filteredDIRXMLBox.Text = "";
            filteredQueryBox.Text = "";
            filteredArgsBox.Text = "";
            filteredLargeBox.Text = "";
            filteredFreqBox.Text = "";
            filteredSplitBox.Text = "";

            statusLabel1.Text = "Parsing query [" + queryname + "]...";
            threadsLabel1.Text = "";
            if (!unattended)
            {
                queryargs = args.Text;
                if (editor.Selection.Length > 0)
                {
                    querydef = editor.Selection.Text;
                    threadsLabel1.Text = "Sub-query only";
                    MessageBox.Show("Operating on sub-query only:\n\n" + querydef);
                }
                else querydef = editor.Text;
            }

            enableGUI(false);
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    _start_exception = null;
                    SCCMParser.prepareQuery(CX = new SCCMParser.Context(),
                        this, queryname, querydef, queryargs, unattended);

                    int si = queryargs.ToUpper().IndexOf("SELECTADS=");
                    if (si >= 0) si += 10;
                    int sj = si < 0 ? -1 : queryargs.IndexOf(',', si);
                    string selectads = si < 0 ? null :
                    queryargs.Substring(si, sj < 0 ? queryargs.Length - si : sj - si).ToLower();




                   

                    if (selectads != null && (
                            selectads.Contains(" itopsmembers")
                         || selectads.Contains("itopsmanagement")
                       ))
                    {
                        if (selectads.Contains(" itopsmembers"))
                            setWQL("ADS: 'ITOps Members' column is selected.");
                        if (selectads.Contains(" itopsmanagement"))
                            setWQL("ADS: 'ITOps Management' column is selected.");
                        if (loadStaffStatus == loadStaffError)
                            setWQL("Loading ITOps Members skipped because it already failed.");
                        else if (loadStaffStatus == loadStaffDone)
                            setWQL("Loading ITOps Members already done.");
                        else if (loadStaffStatus == loadStaffNotDone)
                        {
                            loadStaffStatus = loadStaff();
                            if (loadStaffStatus == loadStaffDone)
                                setWQL("Loading ITOps Members done.");
                            else
                                setWQL("Loading ITOps Members failed.");
                        }
                    }
                }
                catch (Exception ee)
                {
                    _start_exception = ee;
                }
                _start_queryname = queryname;
                _start_unattended = unattended;
                Invoke(new _start_ButtonPerformClickDelegate(_start_ButtonPerformClick));
            })).Start();
        }

        
        public delegate void _start_ButtonPerformClickDelegate();
        private void _start_ButtonPerformClick()
        {
            _start_Button.PerformClick();
        }
        
        Exception _start_exception = null;
        string _start_queryname = null;
        bool _start_unattended = false;

        private void _start_Button_Click(object sender, EventArgs e)
        {
            if (_start_exception != null)
            {
                string semessage = _start_exception.Message.ToLower().StartsWith("parse error: ") ?
                    _start_exception.Message : _start_exception.ToString();
                if (CX != null && CX.PARSER_DEBUG != null)
                    SCCMParser.DebugLine(CX, "\n" + semessage);
                else Console.WriteLine(semessage);
                        setWQL(semessage);
                        setThreads("Parse Error");
                        setStatus(_start_exception.Message);
                if (!_start_unattended) MessageBox.Show(semessage);
                WQL.Visible = false;
                SCCMParser.TemplateClose(CX, this);
                enableGUI(true);
                return;
            }

            SCCMParser.TemplateClose(CX, this);
            Console.WriteLine("Running : query [" + _start_queryname + "]...");
            statusLabel1.Text = "Running query [" + _start_queryname + "]...";
            threadsLabel1.Text = "";

            int qwi = 0; while (queryWorker.IsBusy && qwi < 10)
            {
                Console.WriteLine("Query Worker..." + qwi++);
                threadsLabel1.Text = "QRY ...";
                Thread.Sleep(1000);
            }

            if (queryWorker.IsBusy)
            {
                Console.WriteLine("Dismissed query [" + _start_queryname + "]. Work already in progress.");
                threadsLabel1.Text = "QRY N/A";
                if (!_start_unattended) MessageBox.Show("Dismissed query [" + _start_queryname + "].\nWork already in progress.");
                WQL.Visible = false;
                return;
            }
            else
            {
                if (!connectingWorker.IsBusy)
                {
                    //Console.WriteLine("Connection Worker none.");
                    threadsLabel1.Text = "CNX none";
                    connectingWorker.RunWorkerAsync();
                }
                int cwi = 0; while (!connectingWorker.IsBusy && cwi < 10)
                {
                    Console.WriteLine("Connection Worker..." + cwi);
                    threadsLabel1.Text = "CNX ...";
                    Thread.Sleep(1000);
                }
                if (!connectingWorker.IsBusy)
                {
                    Console.WriteLine("Connection Worker failure.");
                    threadsLabel1.Text = "CNX N/A";
                    return;
                }

                //Console.WriteLine("Query Worker started.");
                threadsLabel1.Text = "Thread QRY!";
                //enableGUI(false);
                queryWorker.RunWorkerAsync(_start_unattended);

            }
        }



        private void exportResultFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            new Thread(new ThreadStart(delegate
            {
                string fname = exportResultFileDialog.FileName;

                statusLabel1.Text = "Exporting to: " + Path.GetFileName(fname) + "...";
                threadsLabel1.Text = "";
                freezeGUI = true;
                bool success = exportResult(fname,
                    (status, thread) =>
                    { statusLabel1.Text = status; threadsLabel1.Text = thread; return true; },
                    false);

                string recipients = notifyBox.CheckState != CheckState.Checked ? null
                    : Interaction.InputBox("To: ", "Notify the result by email", filteredRecipientsBox.Text);
                if (recipients != null) recipients = recipients.Trim();
                if (recipients != null && recipients.Length == 0) recipients = null;
                string cc = recipients == null || notifyBox.CheckState != CheckState.Checked ? null
                    : Interaction.InputBox("Cc: ", "Notify the result by email", ccBox.Text);
                if (cc != null) cc = cc.Trim();
                if (cc != null && cc.Length == 0) cc = null;
                if (recipients != null)
                {
                    setArgsNotify(recipients);
                    MSOfficeTools.outlookNotify(fname, success, filteredRecipientsBox.Text,
                        ccBox.Text, UserPrincipal.Current.EmailAddress);
                }
                freezeGUI = false;
                statusLabel1.Text = (success ? " Successfully exported to: " : "Failed to export to: ")
                    + Path.GetFileName(fname);
                threadsLabel1.Text = "";
            })).Start();
            
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;
            exportResultFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string count = !CX.unattended && appendCountBox.Checked ? "=" + countLabel.Text : "";
            string fname = exportResultFileDialog.FileName;
            exportResultFileDialog.FileName = fname + count;
            exportResultFileDialog.ShowDialog();
            exportResultFileDialog.FileName = fname;
        }

        bool unattendedExport(Func<string, bool> statusLabel, Func<string, bool> threadsLabel)
        {
            string filter =
                  "-" + filteredDomainBox.Text
                + "-" + filteredOUBox.Text
                + "-" + filteredDptBox.Text
                + "-" + filteredWUBox.Text;
            filter = filter.Replace("-*", "").Replace(",", "-").Replace(" ", "");
            string preview = previewBox.CheckState == CheckState.Checked || preview2Box.CheckState == CheckState.Checked ?
                "-Preview" : "";
            string truncated = CX.TRUNCATED ? "-Truncated" : "";
            string date = "-" + DateTime.Now.ToString("yyyy-M-dd");
            string by = "-by " + UserPrincipal.Current.DisplayName.Select(c => char.IsUpper(c) ? "" + c : "").Reverse()
                .Aggregate((x, y) => y.Length > 0 ? x + y : x) + " on " + System.Environment.MachineName;

            string fname = "SCCM Reporting-" + filteredQueryBox.Text + filter + preview + truncated + date + by + ".xlsx";

            //string dir0 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir0 = AuditSec.APPDATA;
            string dir1 = pubBox.Text;
            if (!Directory.Exists(dir1))
            {
                Console.WriteLine("Unattended Export aborted due to missing Publish Directory setting.\n" + fname);
                return false;
            }

            string f0 = Path.Combine(dir0, fname);
            string f1 = Path.Combine(dir1, fname);

            statusLabel("SCHEDULED JOB | Exporting to: " + fname + "...");
            threadsLabel("");
            bool success = exportResult(f0, (status, thread) => { statusLabel(status); threadsLabel(thread); return true;}, true);
            if (success) try
            {
                statusLabel("SCHEDULED JOB | Publishing to: " + dir1 + "...");
                Console.WriteLine("Publish... \"" + f1 + "\"");
                File.Copy(f0, f1);
            } catch(Exception e)
            {
                success = false;
                Console.WriteLine("Failed to Copy result to Publish Directory: " + e.Message
                    + "\nSource: " + f0 + "\nDest: " + f1);
            }
            if (notifyBox.CheckState == CheckState.Checked)
                if (success)
                    MSOfficeTools.outlookNotify(f1, success, filteredRecipientsBox.Text,
                        null/*ccBox.Text*/, UserPrincipal.Current.EmailAddress);
                else
                    MSOfficeTools.outlookNotify(f1, success, UserPrincipal.Current.EmailAddress,
                        null, null);
            freezeGUI = false;
            statusLabel("SCHEDULED JOB | " +  (success ? " Successfully exported to: " : "Failed to export to: ") + fname);
            threadsLabel("");

            return success;
        }


        private bool exportResult(string f, Func<string, string, bool> update, bool unattended)
        {
            Color[] colcolors = new Color[result.Columns.Count];
            for (int i = 0; i < colcolors.Length; i++)
                colcolors[i] = result.Columns[i].HeaderCell.Style.BackColor;

            List<int> hiddenColumns = new List<int>();
            for (int i = 0; i < CX.COLHIDE.Length; i++)
                if (CX.COLHIDE[i]) hiddenColumns.Add(i);

            //return MSOfficeTools.exportResult_Split(f, update, unattended,
            //    table, colcolors, hiddenColumns.ToArray(), result.RowCount - 1,
            //    progver, splitBox.CheckState.Equals(CheckState.Checked) ? CX.SPLITCOLUMNS : null,
            //    CX.PARSER_TEMPL, CX.PARSER_DEBUG, this);
            return MSOfficeTools.exportResult_Split(f, update, unattended, CSVBox.Checked,
                table, colcolors, hiddenColumns.ToArray(), result.RowCount - 1,
                progver, splitBox.CheckState.Equals(CheckState.Checked) ? CX.SPLITCOLUMNS : null,
                CX.PARSER_TEMPL_PATH, CX.PARSER_DEBUG_PATH, this);
        }


        private void SCCMLicenses_FormClosed(object sender, FormClosedEventArgs e)
        {
            STOP = true; PAUSE = false;
            if (parent != null) parent.Show();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;
            foldArgs();
            if (ROW_COUNTING)
            {
                CX.TRUNCATED = true;
                report("Count interrupted. Result truncated.", false);
            }
            else
            {
                if (!STOP)
                    report("Query interrupted. User cancellation.", false);
            }
            if (STOP) WQL.Visible = !WQL.Visible;// false;
        }




        SCCMParser.Context CX = null;
        public bool STOP = true, PAUSE = false;


        public delegate void resultAddColumnDelegate(string columnName, Type columnType, Color back, bool hide, bool clear);
        private void resultAddColumn(string columnName, Type columnType, Color back, bool hide, bool clear)
        {
            if (clear)
            {
                //Console.WriteLine("Resetting the DataTable object (XLS)...");
                table = new System.Data.DataTable("SCCM Reporting");
                tablehash0.Clear();
                tablehash.Clear();
                result.Columns.Clear();
                result.Rows.Clear();
                online.Clear();
                countLabel.Text = "0";
                exportButton.Enabled = false;
            }
            else
            {
                bool success = false;
                int i = 1;
                do
                {
                    string name = columnName + (i < 2 ? "" : "" + i);
                    try
                    {
                        table.Columns.Add(name, columnType != null && columnType != typeof(List<string>) ? columnType : typeof(string));
                        int c = result.Columns.Add(name, name);
                        if (back != Color.Transparent)
                        {
                            result.EnableHeadersVisualStyles = false;
                            result.Columns[c].HeaderCell.Style.BackColor = back;
                        }
                        result.Columns[c].Visible = !hide;
                        success = true;
                        if (i >= 2) setWQL("Warning: Column name '" + columnName + "' is already declared. Using increment '" + name + "'");
                    }
                    catch (Exception e)
                    {
                        i++; if (i > 10)
                        {
                            setWQL("Error: Column name '" + name + "' : " + e.Message);
                            break;
                        }
                    }
                } while (!success);
                exportButton.Enabled = true;
            }
        }

        void prepareColumns()
        {
            Invoke(new resultAddColumnDelegate(resultAddColumn), new object[] { null, null, Color.Transparent, false, true });
            for (int i = 0; i < CX.COLNAME.Length; i++)
            {
                Color back = CX.COLCOLOR[i] == null || Color.FromName(CX.COLCOLOR[i]).Equals(Color.Black)
                    ? Color.Transparent : Color.FromName(CX.COLCOLOR[i]);
                Invoke(new resultAddColumnDelegate(resultAddColumn), new object[] { CX.COLALIAS[i], CX.COLTYPE[i], back, CX.COLHIDE[i], false });
            }
            for (int i = 0; i < CX.ADSCOLNAME.Length; i++)
            {
                Color back = Color.PowderBlue;
                Invoke(new resultAddColumnDelegate(resultAddColumn), new object[] { CX.ADSCOLNAME[i], CX.ADSCOLTYPE[i], back, false, false });
            }
            for (int i = 0; i < CX.DIRXMLCOLNAME.Length; i++)
            {
                Color back = Color.DarkSeaGreen;
                Invoke(new resultAddColumnDelegate(resultAddColumn), new object[] { CX.DIRXMLCOLALIAS[i], CX.DIRXMLCOLTYPE[i], back, false, false });
            }
        }




        public delegate void setExportDescriptionDelegate(bool unattended);

        void setExportDescription(bool unattended)
        {
            if (unattended) exportResultFileDialog.FileName = "";
            else
            {
                string filter =
                      "-" + filteredDomainBox.Text
                    + "-" + filteredOUBox.Text
                    + "-" + filteredDptBox.Text
                    + "-" + filteredWUBox.Text;
                filter = filter.Replace("-*", "").Replace(",", "-").Replace(" ", "");
                string preview = previewBox.CheckState == CheckState.Checked || preview2Box.CheckState == CheckState.Checked ?
                    "-Preview" : "";
                string truncated = CX.TRUNCATED ?
                    "-Truncated" : "";
                string date = "-" + DateTime.Now.ToString("yyyy-M-dd");
                exportResultFileDialog.FileName = "SCCM Reporting-" + queryCombo.Text + filter + preview + truncated + date;
            }
        }


        System.Data.DataTable table = new System.Data.DataTable("SCCM Reporting");
        Hashtable tablehash = new Hashtable(), tablehash0 = new Hashtable();
        List<Task> activeTasks = new List<Task>();
        int ACTIVE_COUNT = 0, MAX_THREADS = 50, JOB_SECONDS = 0, DISP_COUNT = 0, SUB_COUNT = 0, SUB_TOTAL = 0, ROWS_COUNT = 0;
        DateTime JOB_STARTED = DateTime.MinValue, LAST_PURGE = DateTime.MinValue;
        bool DISP_CONNECTING = false, ROW_COUNTING = false, JOB_AUTO = false;
        string SCOPE_PATH = "";
        Exception COLL_EXCEP = null;


        Thread get_MOC_to_LMO_Converter(ManagementObjectCollection collection, List<ManagementObject> rows)
        {
            
            Thread T = new Thread(new ThreadStart(delegate
            {
                try
                {
                    COLL_EXCEP = null;
                    ROW_COUNTING = true;
                    //Console.WriteLine("Enumerating the collection: Started.");
                    foreach (ManagementObject o in collection)
                    {
                        rows.Add(o);
                        ROWS_COUNT = rows.Count;
                        if (STOP || !ROW_COUNTING) break;
                    }
                    if (!ROW_COUNTING) Console.WriteLine("Enumerating the collection: Truncated.");
                    //else Console.WriteLine("Enumerating the collection: Finished.");
                }
                catch (Exception e)
                {
                    COLL_EXCEP = e;
                    Console.WriteLine("Enumerating the collection: Exception: " + e.Message);
                }
                ROW_COUNTING = false;
            }));
            T.Start();
            return T;

        }

        private void queryWorker_DoWork(object sender, DoWorkEventArgs args)
        {
            bool unattended = (bool)args.Argument;
            JOB_AUTO = unattended;
            JOB_STARTED = DateTime.Now;

            Invoke(new setExportDescriptionDelegate(setExportDescription), unattended);

            ThreadPool.SetMaxThreads(MAX_THREADS, MAX_THREADS);
            string cursub = null;
            ManagementScope scope = null; try
            {
                setWQL(null);
                setWQL(CX.QUERY);
                string OK = unattended ? "OK" :
                    (startButton_runDirect ? "OK" :
                    Interaction.InputBox("Start the Query ?", "WQL", "OK"));
                startButton_runDirect = false;
                if (OK == null || !OK.ToUpper().Equals("OK")) throw new Exception("User cancellation.");
                
                /*if (!unattended)
                {
                    string WQLselection = getWQLselectionInvoke();
                    string queryargs = getQueryArgsInvoke();
                    if (WQLselection.Length > 0)
                        try
                        {
                            prepareQuery(QUERYNAME + " (Sub-query only)", WQLselection, queryargs, unattended);
                            MessageBox.Show("Operating on sub-query only:\n\n" + QUERY);
                            setWQL(null);
                            setWQL(QUERY);
                        }
                        catch (Exception e)
                        {
                            PARSER_DEBUG.AppendLine("\nParsing error: " + e.ToString());
                            Console.WriteLine("Parsing error: " + e.ToString());
                            throw e;
                        }
                }*/

                try {
                    prepareColumns();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    throw new Exception("Cannot create columns. " + e.Message);
                }
                List<string> subQUERIES = SCCMParser.getSubQueries(CX, this);
                scope = new ManagementScope(@"\\" + AuditSec.settings.smssrv + @"\Root\sms\" + AuditSec.settings.smssite);
                SCOPE_PATH = scope.Path.ToString();

                JOB_STARTED = DateTime.Now;
                LAST_PURGE = DateTime.Now;
                JOB_SECONDS = 0;
                STOP = false; PAUSE = false; DISP_CONNECTING = true;
                ROWS_COUNT = 0;
                WMIcache.Clear();

                setWQL("\r\n==========QUERYRUN========================================================================================\r\n");
                SUB_COUNT = 0; SUB_TOTAL = subQUERIES.Count; foreach (string subQUERY in subQUERIES)
                {
                    cursub = subQUERY;
                    SUB_COUNT++;
                    string subStr = "subquery " + SUB_COUNT + "/" + SUB_TOTAL;
                    report(-1, (subQUERIES.Count > 1 ? subStr + " - " : "")
                        + "Connecting to " + scope.Path + "...", unattended);
                    Console.WriteLine("Running : " + subStr + (unattended ? " (Scheduled Job)" :"")
                        //+ "\n" + subQUERY
                        );

                    ManagementObjectCollection
                        collection = new ManagementObjectSearcher(scope, new ObjectQuery(subQUERY)/*,
                    new EnumerationOptions() {ReturnImmediately = true, Rewindable = false}*/).Get();

                    Console.WriteLine("Returned: " + subStr + (unattended ? " (Scheduled Job)" : "")
                        + (collection == null ? "\nNone" : ""));
                    if (collection == null) throw new Exception("Query returned null result.");

                    List<ManagementObject> rows = new List<ManagementObject>();

                    Thread conv = get_MOC_to_LMO_Converter(collection, rows);
                    while (!STOP && conv.IsAlive)
                    {
                        //Console.WriteLine("Enumerating the collection..."
                        //    + (int)((DateTime.Now - JOB_STARTED).TotalSeconds) + "s");
                        Thread.Sleep(5000);
                    }
                    if (conv.IsAlive) try
                        {
                            Console.WriteLine("Enumerating the collection: Abort!");
                            conv.Interrupt();
                            conv.Abort();
                            Thread.Sleep(2500);
                            DISP_CONNECTING = false;
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine("Enumerating the collection. Abort: Error: " + ee.Message);
                        }

                    if (STOP && conv.IsAlive)
                        MessageBox.Show("Oops, we are experiencing problems to stop that query..."
                        + "You'd better restart the program.", "Stop a query");

                    collection.Dispose(); collection = null;
                    if (COLL_EXCEP != null) throw COLL_EXCEP;
                    
                    Console.WriteLine("\n" + "Result Count: " + ROWS_COUNT);
                    report(0, "Result Count: ", ROWS_COUNT, unattended);
                    DISP_CONNECTING = false;
                    //bool header = true;
                    int n = 0; if (!STOP) foreach (ManagementObject o in rows)
                    {
                        if (STOP) break;
                        //else if (header) header = false;
                        else
                        {
                            //Console.WriteLine("Setup new task...");
                            Task task = new Task(o, n++, ROWS_COUNT, unattended);
                            //Console.WriteLine("Queuing task " + (task.Index()+1) + "/" + task.QCount());
                            try
                            {
                                activeTasks.Add(task);
                                ThreadPool.QueueUserWorkItem(new WaitCallback(Query1), task);
                                purge();
                            }
                            catch (Exception ee)
                            {
                                Console.WriteLine("Failed queuing task: " + ee.Message);
                            }
                        }
                    }
                }

                while (activeTasks.Count > 0)
                {
                        Console.WriteLine("All tasks were queued. Waiting remainder to finish..." + activeTasks.Count);
                        Thread.Sleep(5000);
                        purge();

                }
                Console.WriteLine("All tasks were finished." + activeTasks.Count);

                if (CX.ADDUSERNOTFOUND)
                    add_UserNotFound_Rows(CX.unattended);


                if (JOB_AUTO)
                {
                    //export
                    unattendedExport(
                        status => { statusLabel1.Text = status; return true; },
                        threads => { threadsLabel1.Text = threads; return true; });
                    Thread.Sleep(5000);
                    int BREAK_MINUTES = 2;
                    for (int i = BREAK_MINUTES * 12; i >= 0; i--)
                    {
                        Thread.Sleep(5000);
                        report("Job finished ...will soon be available again... " + i, unattended);
                    }
                }

            }
            catch (ManagementException ee)
            {
                string extendedError = ee.Message + ". "+ ee.ErrorCode.ToString();
                if (extendedError.StartsWith("Quota violation"))
                    extendedError = "Quota violation. Too long a request (" + cursub.Length + ")";
                
                try
                {
                    string desc = ee.ErrorInformation["Description"].ToString();
                    desc = desc.Replace(cursub/*CX.QUERY*/, "");
                    extendedError += "\n" + desc;
                    extendedError = extendedError.Replace("Failed\nFailed to parse WQL string", "Failed to parse WQL string");
                }
                catch (Exception eee)
                {
                    //Console.WriteLine(eee.ToString());
                }
                Console.WriteLine("Query interrupted. Management Exception. " + extendedError);
                if (extendedError.StartsWith("Quota violation"))
                    Console.WriteLine("Query: " + cursub);
                setWQL(ee.Message);
                report(0, "Query interrupted. Management Exception. " + extendedError, null, unattended);
                activeTasks.Clear();
            }
            catch (Exception ee)
            {
                Console.WriteLine("Query interrupted.\n" + ee.ToString());
                if (ee.Message.StartsWith("User cancellation.")) setWQL(ee.Message);
                else setWQL(ee.ToString());
                report(0, "Query interrupted. " + ee.Message, null, unattended);
                activeTasks.Clear();
            }
            if (scope != null) try
            {
                scope.Path = new ManagementPath();
            }
            catch (Exception ee)
            {
                Console.WriteLine("Could not clean ManagementScope! " + ee.Message);
            }

        }

        /*
        public List<string[]> getCollectionID(string CollectionLike)
        {
            List<string[]> result = new List<string[]>();
            string QUERY =
                "SELECT CollectionID, Name, Comment FROM SMS_Collection "
                + " WHERE ( Name LIKE \"" + CollectionLike + "\""
                + " OR Comment LIKE \"" + CollectionLike + "\" )";
            ManagementScope scope = null; try
            {
                scope = new ManagementScope(@"\\" + AuditSec.settings.smssrv + @"\Root\sms\" + AuditSec.settings.smssite);
                Console.WriteLine("Running : Query Collection like \"" + CollectionLike + "\"");
                ManagementObjectCollection moc = new ManagementObjectSearcher(scope, new ObjectQuery(QUERY)).Get();
                if (moc == null) throw new Exception("Query returned null result.");
                else foreach (ManagementObject mo in moc)
                {
                    string CollectionID = mo.Properties["CollectionID"] != null ? mo.Properties["CollectionID"].Value.ToString() : null;
                    string Name = mo.Properties["Name"] != null ? mo.Properties["Name"].Value.ToString() : null;
                    string Comment = mo.Properties["Comment"] != null ? mo.Properties["Comment"].Value.ToString() : null;
                    if (CollectionID != null && CollectionID.Length > 0
                        && Name != null && Name.Length > 0)
                    {
                        string[] array = new string[] { CollectionID, Name, Comment };
                        result.Add(array);
                        Console.WriteLine("    CollectionID=" + CollectionID + "\n        Name=" + Name + "\n        Comment=" + Comment);
                    }
                }
            }
            catch (ManagementException ee)
            {
                string extendedError = ee.Message + ". "+ ee.ErrorCode.ToString();
                if (extendedError.StartsWith("Quota violation"))
                    extendedError = "Quota violation. Too long a request (" + QUERY.Length + ")";
                
                try
                {
                    string desc = ee.ErrorInformation["Description"].ToString();
                    desc = desc.Replace(CX.QUERY, "");
                    extendedError += "\n" + desc;
                }
                catch (Exception eee)
                {
                    //Console.WriteLine(eee.ToString());
                }
                Console.WriteLine("Query interrupted. Management Exception. " + extendedError);
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
            Console.WriteLine("Returned: Query Collection Like \"" + CollectionLike + "\": " + result.Count);
            return result;
        }
        */


        private void connectingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("Opening connection.");
            STOP = false; PAUSE = false; while (DISP_CONNECTING || !STOP)
            {
                if (DISP_CONNECTING)
                {
                    try
                    {
                        connectingWorker.ReportProgress(0);
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine("Exception while reporting: " + ee.Message);
                    }
                }
                Thread.Sleep(500);
            }
            Console.WriteLine("Closing connection.");
        }

        char[] waitingChars = new char[] { '|', '/', '-', '\\' };
        private void connectingWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string unattstr = JOB_AUTO ? "SCHEDULED JOB | " : "";
            if (DISP_CONNECTING)
            {
                progressBar1.Maximum = 60;
                progressBar1.Value = DISP_COUNT % 60;
                statusLabel1.Text = unattstr + "Connecting to " + SCOPE_PATH + "... " + waitingChars[DISP_COUNT++ % 4];
                threadsLabel1.Text = (STOP ? "Cancel" : "Count " +  ROWS_COUNT)
                    + "-" + (int)((DateTime.Now - JOB_STARTED).TotalSeconds) + "s";
            }
            else 
            {
                DISP_COUNT = 0;
                threadsLabel1.Text = "Thread " + ACTIVE_COUNT + "-" + (int)((DateTime.Now - JOB_STARTED).TotalSeconds) + "s";
            }
        } 
        
        void purge()
        {
            int DT = (int)((DateTime.Now - LAST_PURGE).TotalMilliseconds);
            if (DT > 10000)
            {
                try
                {
                    DateTime t0 = DateTime.Now;
                    Task.purge(activeTasks, ref ACTIVE_COUNT);
                    DateTime t1 = DateTime.Now;
                    int dt = (int)((t1 - t0).TotalMilliseconds);
                    //Console.WriteLine("Tasks were purged successfully (" + dt + "ms)"
                    //    + " - Active count: " + ACTIVE_COUNT
                    //    + " - Total count: " + activeTasks.Count);
                    threadsLabel1.Text = "Thread " + ACTIVE_COUNT + "-" + (int)((DateTime.Now - JOB_STARTED).TotalSeconds) + "s";
                }
                catch (Exception ee)
                {
                    Console.WriteLine("Failed purging tasks: " + ee.Message);
                }
                LAST_PURGE = DateTime.Now;
            }
            //else Console.WriteLine("Delayed purging tasks (" + DT + "ms)");
        }


        private void threadsLabel1_Click(object sender, EventArgs e)
        {
            
            Console.WriteLine("Thread listing invoked...");
            string threadList = "";  try
            {
                lock (activeTasks)
                {
                    Task.purge(activeTasks, ref ACTIVE_COUNT);
                    threadList = Task.ToString(activeTasks);
                }
            }
            catch (Exception ee)
            {
                ACTIVE_COUNT = -1;
            }
            //setWQL("Threads: " + ACTIVE_COUNT + "\nTasks:\n" + threadList);
            Console.WriteLine("Threads: " + ACTIVE_COUNT + "\nTasks:\n" + threadList);
            
        }

        void report(string x, bool unattended)
        {
            report(table.Rows.Count - 1, x, null, unattended);
        }
        void report(string x, object o, bool unattended)
        {
            report(table.Rows.Count - 1, x, o, unattended);
        }
        void report(int p, string x, bool unattended)
        {
            report(p, x, null, unattended);
        }
        void report(int p, string x, object o, bool unattended)
        {
            if (STOP) return;
            try
            {
                queryWorker.ReportProgress(p, new Object[] { x, o, unattended });
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while reporting: " + e.Message);
            }
        }

        private void queryWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int p = e.ProgressPercentage;
            string x = (string)((object[])e.UserState)[0];
            object o = ((object[])e.UserState)[1];
            bool unattended = (bool)((object[])e.UserState)[2];
            string unattstr = unattended ? "SCHEDULED JOB | "  : "";

            string SUB = SUB_TOTAL > 1 ? "Subquery " + SUB_COUNT + "/" + SUB_TOTAL + " - " : "";
            if (x.StartsWith("enableGUI"))
            {
                bool enabled = (bool)o;
                enableGUI(enabled);
            }
            else if (x.StartsWith("Result Count"))
            {
                int count = (int)o;
                enableGUI(false);
                progressBar1.Maximum = count;
                progressBar1.Value = 0;
                if (count <= 0) { STOP = false; PAUSE = false; }
                stopButton.Enabled = true;
                statusLabel1.Text = unattstr + SUB + x + count;
            }
            else if (x.StartsWith("Query interrupted"))
            {
                stopButton.Enabled = false;
                STOP = true; PAUSE = false; DISP_CONNECTING = false;
                if (CX != null && CX.QUERY.Length > 0) x = x.Replace(CX.QUERY, "");
                Console.WriteLine(x + "\n\n" + CX.QUERY);
                if (!unattended) MessageBox.Show(x + "\n\n" + CX.QUERY);
                enableGUI(true);
                statusLabel1.Text = unattstr + x;
            }

            else if (x.StartsWith("Count interrupted"))
            {
                ROW_COUNTING = false;
                PAUSE = false;
                Console.WriteLine(x + "\n\nClick on Stop once more to abort the request.");
                if (!unattended) MessageBox.Show(x + "\n\nClick on Stop once more to abort the request.");
                statusLabel1.Text = unattstr + x;
            }

            else if (x.StartsWith("Connecting"))
            {
                DISP_COUNT = 0;
                DISP_CONNECTING = true;
                enableGUI(false);
                progressBar1.Maximum = 60;
                progressBar1.Value = 0;
                STOP = false; PAUSE = false;
                stopButton.Enabled = true;
                statusLabel1.Text = unattstr + SUB + x;
            }

            else if (x.StartsWith("In progress")) //new row insertion
            {
                stopButton.Enabled = true;
                if (o != null && o.GetType() == typeof(object[]))
                {
                    object[] row = (object[])o;
                    string col0 = row[0].ToString();
                    StringBuilder transtyping = new StringBuilder();
                    try
                    {
                        transtypeRow(ref row, transtyping);
                        string rowstring = new List<object>(row).Aggregate((m, n) =>
                            (m is List<string> ? (((List<string>)m).Count == 0 ? "" : ((List<string>)m).Aggregate((m1, m2) => m1 + "; " + m2)) : m.ToString())
                            + "\n"
                            + (n is List<string> ? (((List<string>)n).Count == 0 ? "" : ((List<string>)n).Aggregate((n1, n2) => n1 + "; " + n2)) : n.ToString())
                        ).ToString();

                        bool col0dupl = "!NOTFOUND!".Equals(col0) ? false : tablehash0.Contains(col0);
                        bool rowdupl = "!NOTFOUND!".Equals(col0) ? false : tablehash.Contains(rowstring);
                        List<object[]> rr2 = col0dupl ? (List<object[]>)tablehash0[col0] : new List<object[]>();

                        bool ROWADD_REQ = true;
                        if(!mergePanicBox.Checked) {

                        if (rowdupl)
                        {
                            ROWADD_REQ = false;
Console.WriteLine("Warning: Duplicate row found and eliminated: " + col0 + "\n" + rowstring);
                            setWQL("Warning: Duplicate row found and eliminated: " + col0);
                        }
                        else if (col0dupl && rr2.Count == 1)
                        {
                            object[] rr = rr2[0];
                            DataGridViewRow resultRow = (DataGridViewRow)rr[0];
                            DataRow tableRow = (DataRow)rr[1];
                            for (int i = 1; i < row.Length; i++)
                            {
                                string value = row[i].ToString().Trim();
                                var list = new List<string>(resultRow.Cells[i].Value.ToString()
                                    .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                                    .Select(m => (m.EndsWith(";") ? m.Substring(0, m.Length - 1) : m).Trim());

                                if ((  value.Length > 0
                                    && (// if value not yet in existing multivalue
                                        !list.Contains(value)
                                     || // or 
                                     //issue maybe found 2015 nov 26 : replace CX.COLNOAGGREG.Length  with CX.AGGREGFLAG.Length
                                    //(i < CX.COLNOAGGREG.Length && CX.COLAGGREGFLAG[i] != null && CX.COLAGGREGFLAG[i].Equals("COUNT")
                                        (i < CX.COLAGGREGFLAG.Length && CX.COLAGGREGFLAG[i] != null && CX.COLAGGREGFLAG[i].Equals("COUNT")
                                        && value.Equals("1"))
                                    )
                                    && !(i < CX.COLNOAGGREG.Length ? CX.COLNOAGGREG[i] : false)
                                ) || (
                                       (value.Length == 0 && list.Count() > 0) || (value.Length > 0 && list.Count() == 0)
                                ))
                                    
                                {
Console.WriteLine("Warning: Duplicate item found: " + col0 + "\n[" + SCCMCalc.getColAlias(CX, i) + "] has not yet value " + row[i]
    + ".\tPrevious value " + resultRow.Cells[i].Value.ToString().Replace("\n", "; ") + ".");

                                    //setWQL("Warning: Duplicate item [" + SCCMCalc.getColAlias(CX, i) + "] found and merged in row: " + col0);
                                    ROWADD_REQ = false;
                                }
                            }
                            if (!ROWADD_REQ) //Aggregation multivalue
                            {
                                for (int i = 1; i < row.Length; i++)
                                {
                                    string value = row[i].ToString().Trim();
                                    var list = new List<string>(resultRow.Cells[i].Value.ToString()
                                        .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                                        .Select(m => (m.EndsWith(";") ? m.Substring(0, m.Length - 1) : m).Trim());

                                    if (   value.Length > 0
                                        && (
                                            !list.Contains(value)
                                         || (i < CX.COLNOAGGREG.Length && CX.COLAGGREGFLAG[i] != null && CX.COLAGGREGFLAG[i].Equals("COUNT")
                                            && value.Equals("1"))
                                        )
                                        && !(i < CX.COLNOAGGREG.Length ? CX.COLNOAGGREG[i] : false))
                                    {
                                        bool z1 = SCCMCalc.isZeroValue(CX, i, resultRow.Cells[i].Value, null);
                                        bool z2 = SCCMCalc.isZeroValue(CX, i, row[i], null);
                                        if (z1 && !z2) tableRow[i] = resultRow.Cells[i].Value =
                                                row[i];
                                        else if (!z1 && !z2)
                                        {
                                            /*object[] row_ = new object[row.Length]; int j = 0;
                                            foreach (object rowj in row) row_[j++] = rowj;
                                            for (j = 0; j < i; j++) row_[j] = resultRow.Cells[j].Value;*/

                                            string oldType = resultRow.Cells[i].Value.GetType().Name;
                                            string oldValue = resultRow.Cells[i].Value.ToString().Replace("\n", " ");

                                            tableRow[i] = resultRow.Cells[i].Value =
                                                SCCMCalc.aggregateValue(CX, i, resultRow.Cells[i].Value, row[i], null);
                                            setWQL("Warning: Duplicate item [" + SCCMCalc.getColAlias(CX, i) + "] found and merged in row: " + col0
                                               + "\n                        (" + oldType + ")" + oldValue
                                                + (SCCMCalc.getColAggregFlag(CX, i) == null ? "+"
                                                : " *" + SCCMCalc.getColAggregFlag(CX, i) + "*")
                                                + " (" + row[i].GetType().Name + ")" + row[i].ToString().Replace("\n", " ")
                                                + " = (" + tableRow[i].GetType().Name + ")" + tableRow[i].ToString().Replace("\n", " "));
                                        }
                                    }
                                }
Console.WriteLine("Warning: Duplicate item found and merged: " + col0);
                                //setWQL("Warning: Duplicate items found and merged in row: " + col0);
                                string rowstring2 = new List<object>(tableRow.ItemArray).Aggregate(
                                    (m, n) => m.ToString() + "\n" + n.ToString()).ToString();
Console.WriteLine("\t\tMerged row is as follows: " + rowstring2.Replace("\n", "; ") + "\n");
                            }

                        }
                        }
                        if (ROWADD_REQ)
                        {
                            DataGridViewRow resultRow = result.Rows[result.Rows.Add(row)];
                            DataRow tableRow = table.Rows.Add(row);//has pb
                            if ("COMPUTER".Equals(CX.COLALIAS[0].ToUpper())) online.Add(col0);

                            if (tablehash.Contains(rowstring)) tablehash.Remove(rowstring);
                            tablehash.Add(rowstring, col0);

                            object[] rr = new object[] { resultRow, tableRow };
                            rr2.Add(rr);
                            if (tablehash0.Contains(col0)) tablehash0.Remove(col0);
                            tablehash0.Add(col0, rr2);
                        }
                    }
                    catch (Exception ee)
                    {
                        statusLabel1.Text = unattstr + "Error: Type mismatch: " + col0;
                        string error0 = errorBuilder(ee, col0, row, transtyping, false);
                        string error1 = errorBuilder(ee, col0, row, transtyping, true);
                        Console.WriteLine(error1);
                        setWQL(error1);
                        if (!PAUSE)
                        {
                            PAUSE = true;
                            string YES = unattended ? "YES" : Interaction.InputBox("Continue processing ?\n\n" + error0, "Row insertion failure", "Yes");
                            PAUSE = false;
                            if (!"YES".Equals(YES.ToUpper())) STOP = true;
                        }
                    }
                    countLabel.Text = "" + (result.RowCount - 1);
                }
                if (o != null && o.GetType() == typeof(Task))
                {
                    Task task = (Task)o;
                    progressBar1.Maximum = task.QCount();
                    progressBar1.Value = task.Index();
                    statusLabel1.Text = unattstr + SUB + x + " " + task.Index() + "/" + task.QCount();
                    threadsLabel1.Text = "Thread " + ACTIVE_COUNT + "-" + (int)((DateTime.Now - JOB_STARTED).TotalSeconds) + "s";
                }
            }
            else
            {
                statusLabel1.Text = unattstr + x;
                bool console = true;
                if (x.StartsWith("Job finished ...will")) console = false;
                if (x.StartsWith("All tasks were queued. Waiting")) console = false;
                if(console) Console.WriteLine(x);
            }
        }

        void add_UserNotFound_Rows(bool unattended)
        {
            if (CX.USERNOTFOUND == null || CX.USERNOTFOUND.Count == 0) return;
            string unattstr = unattended ? "SCHEDULED JOB | " : "";
            int submitted = CX.USERNOTFOUND.Count, validated = 0;
            Console.WriteLine("All tasks were finished. Final task: Users without any Computer: Submitted " + submitted);
            setStatus(unattstr + "Users without any Computer ...Submitted " + submitted + "...");
            setWQL("Warning: Users without any Computer: Submitted " + submitted);


            CX.USERNOTFOUND.AsParallel().ForAll(Username =>
                {
                    while (PAUSE) Thread.Sleep(500);
                    object[] row = getUserRow(Username, ref submitted, ref validated, unattended);
                    if (row != null)
                    {
                        report("In progress", row, unattended);
                    }
                }
            );

            /*
            foreach (string Username in CX.USERNOTFOUND)
            {
                while (PAUSE) Thread.Sleep(500);
                object[] row = getUserRow(Username, ref submitted, ref validated, unattended);
                if (row != null)
                {
                    report("In progress", row, unattended);
                    //transtypeRow(ref row, null);
                    //result.Rows.Add(row);
                    //table.Rows.Add(row);//has pb
                }
            }
            */

            Console.WriteLine(unattstr + "Users without any Computer: Submitted " + submitted + "/Validated " + validated);
            setStatus(unattstr + "Users without any Computer: Submitted " + submitted + "/Validated " + validated);
            setWQL("Warning: Users without any Computer: Submitted " + submitted + "/Validated " + validated);
        }

        object[] getUserRow(string Username, ref int submitted, ref int validated, bool unattended)
        {
            string unattstr = unattended ? "SCHEDULED JOB | "  : "";
            object[] row_ = null;
            setStatus(unattstr + "Users without any Computer ...Submitted " + submitted + "/Validated " + validated + "...");
            try
            {
                Hashtable dirxml = !DIRXMLdisabled && Username != null ? UsersInfo.getDIRXMLAttributes(Username) : null;
                if (dirxml != null)
                {
                    List<object> row = new List<object>();
                    bool DELETEROW = false;

                    for (int i = 0; i < CX.COLNAME.Length; i++)
                    {
                        string COLUMN = CX.COLALIAS[i].ToUpper();
                        SCCMCalc.processValue(CX, text => { setWQL(text); return true; }, i,
                            (i == 0 ? "!NOTFOUND!" : (COLUMN.EndsWith("USER") ? Username : "")),
                            CimType.None, row, ref DELETEROW, null, getWMI);
                    }


                    for (int a = 0; a < CX.ADSCOLNAME.Length; a++)
                        row.Add(UsersInfo.getValue(CX.ADSCOLTYPE[a], ""));
                    for (int d = 0; d < CX.DIRXMLCOLNAME.Length; d++)
                        row.Add(dirxml != null ? dirxml[CX.DIRXMLCOLNAME[d]] : UsersInfo.getValue(CX.DIRXMLCOLTYPE[d], ""));
                    setWQL("Information: User without any Computer: " + Username);
                    validated++;
                    row_ = row.ToArray();
                }
                else
                {
                    setWQL("Warning: User is not referenced: " + Username);
                }
            }
            catch (Exception ee)
            {
                setWQL("Error: User error: " + Username + ": " + ee.Message);
                Console.WriteLine(ee.ToString());
            }
            submitted--;
            return row_;
        }




        void transtypeRow(ref object[] row, StringBuilder sb)
        {
            for (int colindex = 0; colindex < row.Length; colindex++)
                if (row[colindex] is List<string>)
                        row[colindex] = ((List<string>)row[colindex]).Count == 0 ? ""
                            : ((List<string>)row[colindex]).Aggregate((x, y) => x + ";\n" + y);
                else if (row[colindex] == null || (
                    !SCCMCalc.getColType(CX, colindex).Equals(typeof(List<string>)) &&
                    !SCCMCalc.getColType(CX, colindex).Equals(row[colindex].GetType())
                    ))
                    SCCMCalc.transtypeValue(CX, colindex, ref row[colindex], sb);
        }

        string errorBuilder(Exception ee, string col0, object[] row, StringBuilder transtyping, bool list)
        {
            StringBuilder error = new StringBuilder();
            try
            {
                error.AppendLine("Error: Type mismatch: " + col0);
                if (transtyping.Length > 0) error.AppendLine(transtyping.ToString());
                error.AppendLine(list ? ee.ToString() : ee.Message);
                int j = 0; if (list) for (int i = 0; i < CX.COLNAME.Length; i++, j++)
                        if (row[j] == null || !CX.COLTYPE[i].Equals(row[j].GetType())) try
                            {
                                error.AppendLine("\t[" + CX.COLALIAS[i] + "] TYPE " + CX.COLTYPE[i].Name
                                + " cannot store value: (" + (row[j] == null ? "unknown-type" : row[j].GetType().Name) + ")" + row[j] + ".");
                            }
                            catch (Exception eee)
                            {
                                error.AppendLine("\t[" + CX.COLALIAS[i] + "] has exception: " + eee.Message);
                            }
                if (list) for (int i = 0; i < CX.ADSCOLNAME.Length; i++, j++)
                        if (row[j] == null || !CX.ADSCOLTYPE[i].Equals(row[j].GetType())) try
                            {
                                error.AppendLine("\t[" + CX.ADSCOLNAME[i] + "] TYPE " + CX.ADSCOLTYPE[i].Name
                                + " cannot store value: (" + (row[j] == null ? "unknown-type" : row[j].GetType().Name) + ")" + row[j] + ".");
                            }
                            catch (Exception eee)
                            {
                                error.AppendLine("\t[" + CX.ADSCOLNAME[i] + "] has exception: " + eee.Message);
                            }
                if (list) for (int i = 0; i < CX.DIRXMLCOLNAME.Length; i++, j++)
                        if (row[j] == null || !CX.DIRXMLCOLTYPE[i].Equals(row[j].GetType())) try
                            {
                                error.AppendLine("\t[" + CX.DIRXMLCOLALIAS[i] + "] TYPE " + CX.DIRXMLCOLTYPE[i].Name
                                + " cannot store value: (" + (row[j] == null ? "unknown-type" : row[j].GetType().Name) + ")" + row[j] + ".");
                            }
                            catch (Exception eee)
                            {
                                error.AppendLine("\t[" + CX.DIRXMLCOLALIAS[i] + "] has exception: " + eee.Message);
                            }
            }
            catch(Exception eeee)
            {
                error.AppendLine("DEBUG ERROR: " + eeee.ToString());
            }
            return error.ToString();
        }


        private void queryWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            JOB_SECONDS = (int)((DateTime.Now - JOB_STARTED).TotalSeconds);

            Console.WriteLine("Job took " + JOB_SECONDS + "s");
            threadsLabel1.Text = "";
            int match = result.RowCount - 1; if (match < 0) match = 0;
            string completionText =
                (STOP ? "Query incomplete: " : "Query completed: ")
                + match + " match" + (match > 1 ? "(es)" : "")
                + " - " + JOB_SECONDS + "s"
                + " - " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy HH:mm}", DateTime.Now);
            setWQL("Information: " + completionText);

            STOP = true;
            statusLabel1.Text = completionText;
            threadsLabel1.Text = JOB_SECONDS + "s";
            /*
            if (CX.COLALIAS.Contains("PrimaryUser"))
            {
                int pui = Array.FindIndex(CX.COLALIAS, x => x == "PrimaryUser");
                if (result.Columns.Count > pui) result.Sort(result.Columns[pui], ListSortDirection.Ascending);
                if (table.Columns.Count > pui) table.DefaultView.Sort = table.Columns[pui] + " ASC";
            }*/
            if (result.Columns.Count > 0) result.Sort(result.Columns[0], ListSortDirection.Ascending);
            if (table.Columns.Count > 0) table.DefaultView.Sort = table.Columns[0] + " ASC";

            if (CX.COMBINE) combineResult();

            SCCMParser.DebugClose(CX, this);

            if (!JOB_AUTO) MessageBox.Show(completionText);
            
            enableGUI(true);
            stopButton.Enabled = true;
        }

        void combineResult()
        {
           
            Console.WriteLine("Hiding=>Clearing rows...");
            for (int r = table.Rows.Count - 1; r >= 0; r--)
            {
                DataRow row = table.Rows[r];
                string index = row[0].ToString();
                //Console.WriteLine("Hiding=>Clearing rows..." + index);
                for (int c = 0; c < row.ItemArray.Length; c++)
                    if (c < CX.COLHIDE.Length && CX.COLHIDE[c]) row[c] = SCCMCalc.getZeroValue(CX, c, CimType.None, null);
            }
                            
                            
            Console.WriteLine("Combining rows...");
            for (int r = table.Rows.Count - 1; r > 0; r--)
            {
                DataRow row = table.Rows[r];
                string index = row[0].ToString();
                DataRow row_ = table.Rows[r - 1];
                string index_ = row_[0].ToString();
                if (index_.Equals(index))
                {
                    bool keep = true, combfound = false, keep2 = false;
                    for (int c = 0; c < row_.ItemArray.Length; c++)
                        if (c < CX.COLCOMB.Length && CX.COLCOMB[c])
                        {
                            combfound = true;
                            bool empty = row[c].ToString().Length == 0;
                            bool empty_ = row_[c].ToString().Length == 0;
                            if (!empty_ && !empty) keep2 = true;
                        }
                    if (combfound) keep = keep2;
                    if (!keep)
                    {
                        Console.WriteLine("Combining rows..." + index);
                        for (int c = 0; c < row_.ItemArray.Length; c++)
                            if (c < CX.COLCOMB.Length && CX.COLCOMB[c])
                            {
                                bool empty = row[c].ToString().Length == 0;
                                bool empty_ = row_[c].ToString().Length == 0;
                                if (empty_ && !empty) row_[c] = row[c];
                            }
                        table.Rows.Remove(row);
                    }
                    else Console.WriteLine("Combining rows..." + index + " dismissed.");
                }
            }

        }




        class Task
        {
            bool unattended;
            ManagementObject o;
            string sub;
            DateTime task_t0;
            int index, qcount;

            public Task(ManagementObject o, int index, int qcount, bool unattended)
            {
                this.o = o;
                this.qcount = qcount;
                this.index = index;
                this.unattended = unattended;
                sub = "INIT" + index + "/" + qcount;
                task_t0 = DateTime.Now;
            }
            public int Index() { return index; }
            public int QCount() { return qcount; }
            public bool isUnattended() { return unattended; }
            public bool isInitialization()
            {
                return sub.StartsWith("INIT");
            }
            public ManagementObject getWMI()
            {
                return o;
            }
            public void subtask(string sub)
            {
                this.sub = sub;
                task_t0 = DateTime.Now;
            }
            public void terminates()
            {
                this.sub = "END" + index + "/" + qcount; ;
                task_t0 = DateTime.Now;
            }
            public bool isTerminated()
            {
                return sub.StartsWith("END");
            }
            public long getRuntime()
            {
                return Convert.ToInt64((DateTime.Now - task_t0).TotalSeconds);
            }
            override public string ToString()
            {
                List<string> l = new List<string>();
                foreach (PropertyData p in o.Properties)
                {
                    if (p.Value is ManagementBaseObject)
                        foreach (PropertyData p2 in ((ManagementBaseObject)p.Value).Properties)
                            l.Add(p2.Name + "=" + (p2.Value == null ? null : p2.Value.ToString()));
                    else l.Add(p.Name + "=" + p.Value.ToString());
                }
                return sub + "=" + getRuntime() + "\" [" + l.Aggregate((x, y) => x + ", " + y) + "]";
            }
            public static string ToString(List<Task> active)
            {
                return active.Select(task => task.ToString()).Aggregate((x, y) => x + "\n" + y);
            }
           
            public static void purge(List<Task> active, ref int count)
            {
                active.RemoveAll(task => task.isTerminated());
                count = active.Count(task => !task.isInitialization() && !task.isTerminated());
            }
        }



        void Query1(object state)
        {
            Task task = (Task)state;
            bool unattended = task.isUnattended();
            try
            {
                List<object> row = new List<object>();
                bool DELETEROW = false;
                MachineInfo mi = null;
                Hashtable dirxml = null;
                string topUser = null, lastUser = null, primaryUser = null;

                while (PAUSE) Thread.Sleep(500);

                for (int i = 0; i < CX.COLNAME.Length; i++)
                {
                    task.subtask("COL" + (i + 1) + "-" + CX.COLNAME.Length + "-" + CX.COLNAME[i]);
                    Object value = null; CimType type = CimType.None;
                    if (CX.COLTABLE[i] == null || CX.TABLECOUNT == 1)
                    {
                        value = task.getWMI().Properties[CX.COLNAME[i]] != null ? task.getWMI().Properties[CX.COLNAME[i]].Value : null;
                        if (value != null) type = task.getWMI().Properties[CX.COLNAME[i]].Type;
                    }
                    else
                    {

                        PropertyData table = null;
                        try
                        {
                            table = task.getWMI().Properties[CX.COLTABLE[i]];
                        }
                        catch (ManagementException e)
                        {
                            if (e.Message.StartsWith("Not found")) setWQL("Warning: Table " + CX.COLTABLE[i] + " not found.");
                            else throw e;
                        }
                        
                        if (table != null && table.Value is ManagementBaseObject)
                        {
                            ManagementBaseObject o2 = (ManagementBaseObject)task.getWMI().Properties[CX.COLTABLE[i]].Value;
                            value = o2.Properties[CX.COLNAME[i]] != null ? o2.Properties[CX.COLNAME[i]].Value : null;
                            if (value != null) type = o2.Properties[CX.COLNAME[i]].Type;
                        }
                    }
                    //Console.WriteLine("{0}[{2}]: {1}", COLNAME[i], value, rows.Count);


                    StringBuilder sb = DEBUG && CX.COLDEBUG[i] ? new StringBuilder() : null;

                    value = SCCMCalc.processValue(CX, text => { setWQL(text); return true; },
                        i, value, type, row, ref DELETEROW, sb, getWMI);
                    if (sb != null) Console.WriteLine(sb.ToString());

                    if (value != null && value.ToString().Length > 0)
                    {
                        string COLUMN = CX.COLALIAS[i].ToUpper();
                        string UPPER = value.ToString().ToUpper();

                        if ("TOPUSER".Equals(COLUMN)) topUser = UPPER;
                        if ("LASTUSER".Equals(COLUMN)) lastUser = UPPER;
                        if ("PRIMARYUSER".Equals(COLUMN)) primaryUser = UPPER;

                        if ("COMPUTER".Equals(COLUMN))
                        {
                            string Computer = value.ToString().ToUpper();
                            task.subtask("COMP-" + Computer);
                            if (STOP) break;
                            mi = MachineInfo.getMachine(Computer, usersInfo);
                            if (STOP) break;
                            //Console.WriteLine("linkADS " + Computer + "\n" + mi.ToString());
                            string miPrimaryUser = mi == null ? null : mi.getPrimaryUser();
                            if (miPrimaryUser != null && miPrimaryUser.Length > 0)
                                primaryUser = miPrimaryUser;
                        }
                    }

                    if (STOP) break;
                }

                task.subtask("USER-" + topUser + "-" + lastUser + "-" + primaryUser);
                string Username = null; if (Username == null) try
                {
                    if (primaryUser != null && primaryUser.Length > 0 && usersInfo.FindOneUser(primaryUser))
                        Username = primaryUser;
                } catch (Exception e) { Console.WriteLine(e.ToString()); }
                if (Username == null) try
                {
                    if (topUser != null && topUser.Length > 0 && usersInfo.FindOneUser(topUser))
                        Username = topUser;
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
                if (Username == null) try
                {
                    if (lastUser != null && lastUser.Length > 0 && usersInfo.FindOneUser(lastUser))
                        Username = lastUser;
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
                //Console.WriteLine("TOP-" + topUser + "-LAST-" + lastUser + "-PRIMARY-" + primaryUser + "-USER-" + Username);


                task.subtask("DIRXML-" + Username);
                if (!DIRXMLdisabled && CX.DIRXMLCOLNAME.Length > 0)
                {
                    dirxml = Username != null ? UsersInfo.getDIRXMLAttributes(Username) : null;
                    //Console.WriteLine("linkDIRXML " + RegularUser + " (" + Username + ")\n" + ToString(dirxml));
                }                    
                    
                if (CX.USERNOTFOUND != null && Username != null && Username.Length > 0
                    && CX.USERNOTFOUND.Contains(Username.ToUpper()))
                    CX.USERNOTFOUND.RemoveAll(x => x == Username.ToUpper());              

                task.subtask("COL-ADS");
                for (int a = 0; a < CX.ADSCOLNAME.Length; a++)
                    row.Add(mi != null ? mi.getProperty(CX.ADSCOLNAME[a]) : UsersInfo.getValue(CX.ADSCOLTYPE[a], ""));
                task.subtask("COL-DIRXML");
                for (int d = 0; d < CX.DIRXMLCOLNAME.Length; d++)
                    row.Add(dirxml != null ? dirxml[CX.DIRXMLCOLNAME[d]] : UsersInfo.getValue(CX.DIRXMLCOLTYPE[d], ""));
                if (DELETEROW)
                {
                    task.subtask("ROW-DEL");
                }
                else
                {
                    task.subtask("ROW-ADD");
                    if (mi != null
                        //|| CX.TABLECOUNT == 1
                        || !CX.RESTABLE
                        )
                    {
                        object[] row_ = row.ToArray();
                        report("In progress", row_, unattended);
                        report("In progress", task, unattended);
                    }
                }
            }
            catch (Exception ee)
            {
                task.subtask("EXCEP");
                Console.WriteLine(ee.ToString());
                setWQL("Query interrupted. " + ee.ToString());
                report("Query interrupted. " + ee.Message, unattended);
            }
            task.terminates();
        }


      

        string ToString(Hashtable h)
        {
            if (h == null) return null;
            StringBuilder sb = new StringBuilder();
            foreach (DictionaryEntry entry in h)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(entry.Key + "=" + entry.Value);
            }
            return sb.ToString();
        }

        bool argsEquals(string args1, string args2)
        {
            List<string> l1 = args1.Split(new char[] { ',' }).ToList().Select(x => x.Trim()).ToList(); l1.Sort();
            List<string> l2 = args2.Split(new char[] { ',' }).ToList().Select(x => x.Trim()).ToList(); l2.Sort();
            return l1.Aggregate((x, y) => x + ", " + y).Equals(l2.Aggregate((x, y) => x + ", " + y));
        }

        
        private void saveChanges(bool unattended)
        {
            bool invoke = unattended;
            if (invoke) Invoke(new saveChanges_Delegate(saveChanges_), unattended);
            else saveChanges_(unattended);
        }
        public delegate bool saveChanges_Delegate(bool unattended);
        bool saveChanges_(bool unattended)
        {
            if (lastquery.Length > 0
                && (!argsEquals(lastargs, args.Text)
                || !lasteditor.Equals(editor.Text)))
            {
                string argsDiff = "";
                if (!argsEquals(lastargs, args.Text))
                {
                    //Console.WriteLine("Arguments changed in query [" + lastquery + "]\n"
                    //   + lastargs + "\n-> " + args.Text);
                    argsDiff = Diff.DiffTextString(lastargs, args.Text, false);
                    Console.WriteLine("Arguments changed in query [" + lastquery + "]\n" + argsDiff);
                }
                string bodyDiff = "";
                if (!lasteditor.Equals(editor.Text))
                {
                    //Console.WriteLine("Body changed in query [" + lastquery + "]\n"
                    //+ lasteditor + "\n-> " + editor.Text);
                    bodyDiff = Diff.DiffTextString(lasteditor, editor.Text, false);
                    Console.WriteLine("Body changed in query [" + lastquery + "]\n" + bodyDiff);
                }
                statusLabel1.Text = "Saving changes in query [" + lastquery + "]...";
                threadsLabel1.Text = "";
                //Console.WriteLine("Changes detected:\n-before-\nargs (" + lastargs + ")\n" + lasteditor
                //    + "-after-\nargs (" + args.Text + ")\n" + editor.Text);
                string YES = unattended ? "YES" : Interaction.InputBox("Keep changes: ", lastquery, "Yes");
                if ("YES".Equals(YES.ToUpper()) && QUERIES.Contains(lastquery))
                {
                    QUERIES[lastquery] = new string[] { args.Text, editor.Text };
                    querySaveDelete(lastquery, args.Text, editor.Text, true, null);
                    return true;
                }
                else return false;
            }
            else return false;
        }

        string lastquery = "", lastargs = "", lasteditor = "";
        private void queryCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (freezeGUI) { queryCombo.Text = null;  return; }
            foldArgs();

            saveChanges(false);
            resultAddColumn(null, null, Color.Transparent, false, true );


            statusLabel1.Text = queryCombo.Text;
            threadsLabel1.Text = "";
            lastquery = queryCombo.Text;
            lastargs = args.Text = "";
            applyArgs(true);
            lasteditor = editor.Text = "";
            if (!QUERIES.Contains(lastquery)) return;
            string[] o = (string[])QUERIES[lastquery];
            lastargs = args.Text = (string)o[0];

            Color back = Color.White;
            if (queryCombo.Text.ToLower().StartsWith("_template_ - base template")) back = Color.LightYellow;
            else if (queryCombo.Text.ToLower().StartsWith("_template_ - base variables")) back = Color.LightYellow;
            else if (queryCombo.Text.ToLower().StartsWith("_template_")) back = Color.PeachPuff;
            else if (queryCombo.Text.ToLower().StartsWith("_condition_")) back = Color.PeachPuff;
            else if (queryCombo.Text.ToLower().StartsWith("file")) back = Color.PaleTurquoise;
            else if (queryCombo.Text.ToLower().StartsWith("workstation")) back = Color.LightGreen;
            else if (queryCombo.Text.ToLower().StartsWith("usage")) back = Color.Plum;
            else if (queryCombo.Text.ToLower().StartsWith("program")) back = Color.PowderBlue;

            editor.BackColor = back;
            for (int i = 0; i < 32; i++) editor.Styles[i].BackColor = back;
            args2.BackColor = back;
            for (int i = 0; i < 32; i++) args2.Styles[i].BackColor = back;
            args.BackColor = back;

            lasteditor = editor.Text = (string)o[1];

            //Console.WriteLine("Selected query [" + lastquery + "]\nargs (" + lastargs + ")\n" + lasteditor);


            ADSBox.BeginUpdate();
            for (int i = 0; i < ADSBox.Items.Count; i++) ADSBox.SetItemChecked(i, false);
            ADSBox.EndUpdate();
            DIRXMLBox.BeginUpdate();
            for (int i = 0; i < DIRXMLBox.Items.Count; i++) DIRXMLBox.SetItemChecked(i, false);
            DIRXMLBox.EndUpdate();
            applyArgs(true);

            filteredSplitBox.Text = "";
            editor.Focus();
        }

        void applyArgs(bool reset)
        {
            if (reset)
            {
                setADSBoxCheckedItems("");
                setDIRXMLBoxCheckedItems("");
                setDomainBoxCheckedItems("");
                setOUBoxCheckedItems("");
                setDptBoxCheckedItems("");
                setWUBoxCheckedItems("");
                setFreqBoxCheckedItems("");
                setLargeBoxes("");
                setNotifyRecipients("");
            }
            foreach (string arg_ in args.Text.Split(new char[] { ',' }))
            {
                string arg = arg_.Trim();
                if (arg.ToUpper().StartsWith("SELECTADS=")) { setADSBoxCheckedItems(""); setADSBoxCheckedItems(arg.Substring(10)); }
                else if (arg.ToUpper().StartsWith("SELECTDIRXML=")) { setDIRXMLBoxCheckedItems(""); setDIRXMLBoxCheckedItems(arg.Substring(13)); }
                else if (arg.ToUpper().StartsWith("SELECTDOMAIN=")) { setDomainBoxCheckedItems(""); setDomainBoxCheckedItems(arg.Substring(13)); }
                else if (arg.ToUpper().StartsWith("SELECTSITE=")) { setOUBoxCheckedItems(""); setOUBoxCheckedItems(arg.Substring(11)); }
                else if (arg.ToUpper().StartsWith("SELECTDPT=")) { setDptBoxCheckedItems(""); setDptBoxCheckedItems(arg.Substring(10)); }
                else if (arg.ToUpper().StartsWith("SELECTOBJECT=")) { setWUBoxCheckedItems(""); setWUBoxCheckedItems(arg.Substring(13)); }
                else if (arg.ToUpper().StartsWith("FREQUENCE=")) { setFreqBoxCheckedItems(""); setFreqBoxCheckedItems(arg.Substring(10)); }
                else if (arg.ToUpper().StartsWith("LARGE=")) { setLargeBoxes(""); setLargeBoxes(arg.Substring(6)); }
                else if (arg.ToUpper().StartsWith("NOTIFY=")) { setNotifyRecipients(""); setNotifyRecipients(arg.Substring(7)); }
            }
        }

        public void applyArgsUnattended(string queryargs,
            List<string> ADSChecked_, List<string> DIRXMLChecked_,
            ref int largeChecked_, ref string notifyRecipients_, List<string> freqChecked_,
            List<string> domainChecked_, List<string> OUChecked_, List<string> dptChecked_, List<string> WUChecked_,
            bool reset)
        {
            if (reset)
            {
                setADSChecked_Unattended("", ADSChecked_);
                setDIRXMLChecked_Unattended("", DIRXMLChecked_);
                setDomainChecked_Unattended("", domainChecked_);
                setOUChecked_Unattended("", OUChecked_);
                setDptChecked_Unattended("", dptChecked_);
                setWUChecked_Unattended("", WUChecked_);
                setFreqChecked_Unattended("", freqChecked_);
                setLargeChecked_Unattended("", ref largeChecked_);
                setNotifyRecipients_Unattended("", ref notifyRecipients_);
            }
            foreach (string arg_ in queryargs.Split(new char[] { ',' }))
            {
                string arg = arg_.Trim();
                if (arg.ToUpper().StartsWith("SELECTADS=")) { setADSChecked_Unattended("", ADSChecked_); setADSChecked_Unattended(arg.Substring(10), ADSChecked_); }
                else if (arg.ToUpper().StartsWith("SELECTDIRXML=")) { setDIRXMLChecked_Unattended("", DIRXMLChecked_); setDIRXMLChecked_Unattended(arg.Substring(13), DIRXMLChecked_); }
                else if (arg.ToUpper().StartsWith("SELECTDOMAIN=")) { setDomainChecked_Unattended("", domainChecked_); setDomainChecked_Unattended(arg.Substring(13), domainChecked_); }
                else if (arg.ToUpper().StartsWith("SELECTSITE=")) { setOUChecked_Unattended("", OUChecked_); setOUChecked_Unattended(arg.Substring(11), OUChecked_); }
                else if (arg.ToUpper().StartsWith("SELECTDPT=")) { setDptChecked_Unattended("", dptChecked_); setDptChecked_Unattended(arg.Substring(10), dptChecked_); }
                else if (arg.ToUpper().StartsWith("SELECTOBJECT=")) { setWUChecked_Unattended("", WUChecked_); setWUChecked_Unattended(arg.Substring(13), WUChecked_); }
                else if (arg.ToUpper().StartsWith("FREQUENCE=")) { setFreqChecked_Unattended("", freqChecked_); setFreqChecked_Unattended(arg.Substring(10), freqChecked_); }
                else if (arg.ToUpper().StartsWith("LARGE=")) { setLargeChecked_Unattended("", ref largeChecked_); setLargeChecked_Unattended(arg.Substring(6), ref largeChecked_); }
                else if (arg.ToUpper().StartsWith("NOTIFY=")) { setNotifyRecipients_Unattended("", ref notifyRecipients_); setNotifyRecipients_Unattended(arg.Substring(7), ref notifyRecipients_); }
            }
        }


        void setADSChecked_Unattended(string value, List<string> ADSChecked_)
        {
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < ADSBox.Items.Count; i++)
                if (checkedItems.Contains(ADSBox.Items[i].ToString()))
                    ADSChecked_.Add(ADSBox.Items[i].ToString());
        }

        void setDIRXMLChecked_Unattended(string value, List<string> DIRXMLChecked_)
        {
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < DIRXMLBox.Items.Count; i++)
                if (checkedItems.Contains(DIRXMLBox.Items[i].ToString()))
                    DIRXMLChecked_.Add(DIRXMLBox.Items[i].ToString());
        }

        void setDomainChecked_Unattended(string value, List<string> domainChecked_)
        {
            domainChecked_.Clear();
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < domainBox.Items.Count; i++)
                if (checkedItems.Contains(domainBox.Items[i].ToString()))
                    domainChecked_.Add(domainBox.Items[i].ToString());

            if (domainChecked_.Count == 0)
            {
                for (int k = 0; k < domainBox.Items.Count; k++)
                {
                    string item = domainBox.Items[k].ToString();
                    domainChecked_.Add(item);
                }
            }
        }

        void setOUChecked_Unattended(string value, List<string> OUChecked_)
        {
            OUChecked_.Clear();
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < OUBox.Items.Count; i++)
                if (checkedItems.Contains(OUBox.Items[i].ToString()))
                    OUChecked_.Add(OUBox.Items[i].ToString());

            if (OUChecked_.Count == 0)
            {
                for (int k = 0; k < OUBox.Items.Count; k++)
                {
                    string item = OUBox.Items[k].ToString();
                    OUChecked_.Add(item);
                }
            }
        }

        void setDptChecked_Unattended(string value, List<string> dptChecked_)
        {
            dptChecked_.Clear();
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < dptBox.Items.Count; i++)
                if (checkedItems.Contains(dptBox.Items[i].ToString()))
                    dptChecked_.Add(dptBox.Items[i].ToString());
        }

        void setWUChecked_Unattended(string value, List<string> WUChecked_)
        {
            WUChecked_.Clear();
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < WUBox.Items.Count; i++)
                if (checkedItems.Contains(WUBox.Items[i].ToString()))
                    WUChecked_.Add(WUBox.Items[i].ToString());
        }

        void setFreqChecked_Unattended(string value, List<string> freqChecked_)
        {
            string[] checkedItems = value.Split(new char[] { '+' });
            for (int i = 0; i < freqBox.Items.Count; i++)
                if (checkedItems.Contains(freqBox.Items[i].ToString()))
                    freqChecked_.Add(freqBox.Items[i].ToString());
        }

        void setLargeChecked_Unattended(string value, ref int largeChecked_)
        {
            largeChecked_ = 0;
            if (value.Trim().Length > 0) try
                {
                    largeChecked_ = int.Parse(value.Trim());
                    if (largeChecked_ < 0) largeChecked_ = 0;
                    if (largeChecked_ > 2) largeChecked_ = 2;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Invalid Large value: " + value);
                }
        }

        void setNotifyRecipients_Unattended(string value, ref string notifyRecipients_)
        {
            notifyRecipients_ = value;
        }


        void horodatage()
        {

            editor.Text = Regex.Replace(editor.Text, @"[\r\n]*/\*Modified by[^\*]*\*/", "", IgnoreCaseSingleLine)
                + "\n/*Modified by " + UserPrincipal.Current.SamAccountName
                + " on " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy HH:mm}", DateTime.Now) + "*/";
        }

        void querySaveAll()
        {
            StringBuilder sb = new StringBuilder();
            foreach(string name in queryCombo.Items)
            {
                string args = ((string[])QUERIES[name])[0];
                string query = ((string[])QUERIES[name])[1];
                sb.Append("\n" + name + "\n" + args + "\n" + query + "\n¤\n");
                statusLabel1.Text = "Saved all queries";
                threadsLabel1.Text = "";
            }
            File.WriteAllText(queriespath, sb.ToString());
        }

        void querySaveDelete(string name, string args, string query, bool save, string rename)
        {
            querySaveDelete(queriespath, name, args, query, save, rename);
            querySaveDelete(queriespath + ".user", name, args, query, save, rename);
        }
        
        void querySaveDelete(string path, string name, string args, string query, bool save, string rename)
        {

            StringBuilder sb = new StringBuilder();
            bool purge = false;
            if (File.Exists(path)) foreach (string line in File.ReadAllLines(path))
                    if (!purge)
                    {
                        if (line.Equals(name)) purge = true;
                        else sb.AppendLine(line);
                    }
                    else if (line.StartsWith("¤")) purge = false;
            if (name != null)
            {
                if (save)
                {
                    sb.Append("\n" + (rename == null ? name : rename) + "\n"
                        + SCCMParser.removeArgDuplicates(args) + "\n" + query + "\n¤\n\n");
                    statusLabel1.Text = "Saved query [" + (rename == null ? name : rename) + "]\nargs ("
                        + SCCMParser.removeArgDuplicates(args) + ")\n" + query;
                    threadsLabel1.Text = "";
                }
                else
                {
                    statusLabel1.Text = "Deleted query [" + name + "]";
                    threadsLabel1.Text = "";
                }
            }
            else
            {
                statusLabel1.Text = "Saved all queries";
                threadsLabel1.Text = "";
            }
            File.WriteAllText(path, sb.ToString());

            saveSched(false);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;
            horodatage();
            if (queryCombo.Text.Length > 0) 
                querySaveDelete(queryCombo.Text, args.Text, editor.Text, true, null);
            else 
            {
                string rename = Interaction.InputBox("Name: ", "Save Query As", "New Query").Trim();
                if (rename.Length == 0) return;
                if (queryCombo.Items.Contains(rename))
                    MessageBox.Show("Query already exists: " + rename, "Save Query As");
                else
                {
                    QUERIES.Add(rename, new string[] { args.Text, editor.Text });
                    queryCombo.Items.Add(rename);
                    queryCombo.SelectedItem = rename;
                    lastquery = "";
                    querySaveDelete(rename, args.Text, editor.Text, true, null);
                }
            }
        }

        string forbiddenQueryNameWord(string name)
        {
            if (name.ToUpper().IndexOf(" SELECT ") >= 0) return "SELECT";
            if (name.ToUpper().IndexOf(" DISTINCT ") >= 0) return "DISTINCT";
            if (name.ToUpper().IndexOf(" JOIN ") >= 0) return "JOIN";
            if (name.ToUpper().IndexOf(" WHERE ") >= 0) return "WHERE";
            if (name.ToUpper().IndexOf(" AND ") >= 0) return "AND";
            if (name.ToUpper().IndexOf(" OR ") >= 0) return "OR";
            return null;
        }

        bool isForbiddenQueryName(string name)
        {
            string forbidden = forbiddenQueryNameWord(name);
            if (forbidden != null)
            {
                MessageBox.Show("Query name must not use reserved word: " + forbidden + "\n" + name, "Reserved word");
                return true;
            }
            else return false;
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;
            saveChanges(false);

            string name = Interaction.InputBox("Name: ", "New Query", "New Query");
            name = name.Trim();
            if (name.Length == 0) return;
            if (isForbiddenQueryName(name)) return;
            if (queryCombo.Items.Contains(name))
            {
                MessageBox.Show("Query already exists: " + name, "New Query");
                queryCombo.SelectedItem = name;
            }
            else
            {
                QUERIES.Add(name, new string[] { "", "SELECT " });
                queryCombo.Items.Add(name);
                queryCombo.SelectedItem = name;
            }
        }



        private void saveasButton_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;
            horodatage();
            string name = queryCombo.Text;
            if (name.Length == 0) return;
            string rename = Interaction.InputBox("Name: ", "Save Query As", name + " - Copy").Trim();
            if (rename.Length == 0) return;
            if (isForbiddenQueryName(name)) return;
            if (queryCombo.Items.Contains(rename))
                MessageBox.Show("Query already exists: " + rename, "Save Query As");
            else
            {
                QUERIES.Add(rename, new string[] { args.Text, editor.Text });
                lastquery = "";
                queryCombo.Items.Add(rename);
                queryCombo.Text = rename;
                querySaveDelete(rename, args.Text, editor.Text, true, null);
            }
        }

        private void renButton_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;
            saveChanges(false);

            string name = queryCombo.Text;
            if (name.Length == 0) return;
            string rename = Interaction.InputBox("Rename: ", "Rename Query", name).Trim();
            if (rename.Length == 0) return;
            if (isForbiddenQueryName(name)) return;
            if (queryCombo.Items.Contains(rename))
                MessageBox.Show("Query already exists: " + rename, "Rename Query");
            else
            {
                querySaveDelete(name, args.Text, editor.Text, true, rename);
                QUERIES.Add(rename, QUERIES[name]);
                QUERIES.Remove(name);
                lastquery = "";
                queryCombo.Items.Add(rename);
                queryCombo.Items.Remove(name);
                queryCombo.Text = rename;
            }
        }



        private void delButton_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;
            string name = queryCombo.Text;
            if (name.Length == 0) return;
            querySaveDelete(name, null, null, false, null);
            QUERIES.Remove(name);
            lastquery = "";
            queryCombo.Items.Remove(name);
            queryCombo.Text = "";
            editor.Text = "";
            args.Text = "";
        }


        private void setArgsNotify(string recipients)
        {
            if (recipients != null && recipients.Trim().Length == 0) recipients = null;
            if (recipients != null) recipients = recipients.Replace(",", ";");
            this.BeginInvoke(new MethodInvoker(delegate
            {
                foldArgs();
                StringBuilder sb = new StringBuilder(args.Text);
                int i = args.Text.ToUpper().IndexOf("NOTIFY=");
                while (i - 1 >= 0 && (args.Text[i - 1] == ' ' || args.Text[i - 1] == ',')) i--;
                int j = args.Text.IndexOf(",", i + 1); if (j < 0) j = args.Text.Length;
                if (i >= 0) sb.Remove(i, j - i);
                string plus = recipients == null ? "" : "Notify=" + recipients;
                if (plus.Length == 0) { args.Text = sb.ToString(); return; }
                if (sb.ToString().Trim().Length > 0) sb.Append(", ");
                sb.Append(plus);
                while (sb.Length > 0 && (sb[0] == ' ' || sb[0] == '\n' || sb[0] == '\t' || sb[0] == ',')) sb.Remove(0, 1);
                args.Text = sb.ToString();
            }), null);
        }


        private void DIRXMLBox_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate {
                foldArgs();
                DIRXMLChecked.Clear();
                for (int k = 0; k < DIRXMLBox.Items.Count; k++)
                {
                    string item = DIRXMLBox.Items[k].ToString();
                    bool check = DIRXMLBox.GetItemChecked(k);
                    if (check) DIRXMLChecked.Add(item);
                }

                StringBuilder sb = new StringBuilder(args.Text);
                int i = args.Text.ToUpper().IndexOf("SELECTDIRXML=");
                while(i-1 >= 0 && (args.Text[i-1] == ' ' || args.Text[i-1] == ',')) i--;
                int j = args.Text.IndexOf("," , i + 1); if (j < 0) j = args.Text.Length;
                if (i >= 0) sb.Remove(i, j - i);
                string plus = "";
                for (int k = 0; k < DIRXMLBox.Items.Count; k++)
                    if (DIRXMLBox.GetItemChecked(k))
                    {
                        if (plus.Length == 0) plus = "SelectDIRXML=";
                        else plus = plus + "+";
                        plus = plus + DIRXMLBox.Items[k].ToString();
                    }
                if (plus.Length == 0) { args.Text = sb.ToString(); return; }
                if (sb.ToString().Trim().Length > 0) sb.Append(", ");
                sb.Append(plus);
                while (sb.Length > 0 && (sb[0] == ' ' || sb[0] == '\n' || sb[0] == '\t' || sb[0] == ',')) sb.Remove(0, 1);
                args.Text = sb.ToString();
            }), null);
        }

        private void ADSBox_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate {
                foldArgs();
                ADSChecked.Clear();
                for (int k = 0; k < ADSBox.Items.Count; k++)
                {
                    string item = ADSBox.Items[k].ToString();
                    bool check = ADSBox.GetItemChecked(k);
                    if (check) ADSChecked.Add(item);
                }

                StringBuilder sb = new StringBuilder(args.Text);
                int i = args.Text.ToUpper().IndexOf("SELECTADS=");
                while (i - 1 >= 0 && (args.Text[i - 1] == ' ' || args.Text[i - 1] == ',')) i--;
                int j = args.Text.IndexOf(",", i + 1); if (j < 0) j = args.Text.Length;
                if (i >= 0) sb.Remove(i, j - i);
                string plus = "";
                for (int k = 0; k < ADSBox.Items.Count; k++)
                    if (ADSBox.GetItemChecked(k))
                    {
                        if (plus.Length == 0) plus = "SelectADS=";
                        else plus = plus + "+";
                        plus = plus + ADSBox.Items[k].ToString();
                    }
                if (plus.Length == 0) { args.Text = sb.ToString(); return; }
                if (sb.ToString().Trim().Length > 0) sb.Append(", ");
                sb.Append(plus);
                while (sb.Length > 0 && (sb[0] == ' ' || sb[0] == '\n' || sb[0] == '\t' || sb[0] == ',')) sb.Remove(0, 1);
                args.Text = sb.ToString();
            }), null);
        }

        List<string> ADSChecked = new List<string>();
        List<string> DIRXMLChecked = new List<string>();
        List<string> domainChecked = new List<string>();
        List<string> OUChecked = new List<string>();
        List<string> dptChecked = new List<string>();
        List<string> WUChecked = new List<string>();

        private void domainBox_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                foldArgs();
                domainChecked.Clear();
                for (int k = 0; k < domainBox.Items.Count; k++)
                {
                    string item = domainBox.Items[k].ToString();
                    bool check = domainBox.GetItemChecked(k);
                    if (check) domainChecked.Add(item);
                }
                if (domainChecked.Count == 0)
                {
                    for (int k = 0; k < domainBox.Items.Count; k++)
                    {
                        string item = domainBox.Items[k].ToString();
                        domainChecked.Add(item);
                    }
                }
                //Console.WriteLine("OU clause=\n" + getOUClause());
                StringBuilder sb = new StringBuilder(args.Text);
                int i = args.Text.ToUpper().IndexOf("SELECTDOMAIN=");
                while (i - 1 >= 0 && (args.Text[i - 1] == ' ' || args.Text[i - 1] == ',')) i--;
                int j = args.Text.IndexOf(",", i + 1); if (j < 0) j = args.Text.Length;
                if (i >= 0) sb.Remove(i, j - i);
                string plus = "";
                for (int k = 0; k < domainBox.Items.Count; k++)
                    if (domainBox.GetItemChecked(k))
                    {
                        if (plus.Length == 0) plus = "SelectDomain=";
                        else plus = plus + "+";
                        plus = plus + domainBox.Items[k].ToString();
                    }
                if (plus.Length == 0) { args.Text = sb.ToString(); return; }
                if (sb.ToString().Trim().Length > 0) sb.Append(", ");
                sb.Append(plus);
                while (sb.Length > 0 && (sb[0] == ' ' || sb[0] == '\n' || sb[0] == '\t' || sb[0] == ',')) sb.Remove(0, 1);
                args.Text = sb.ToString();

                checkWU(sender, e);
            }), null);
        }

        void checkWU(object sender, EventArgs e)
        {
            if (WUBox.CheckedItems.Count == 0)
            {
                WUBox.SetItemChecked(0, true);
                WUBox_Click(sender, e);
            }
        }

        private void OUBox_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                foldArgs();
                OUChecked.Clear();
                for (int k = 0; k < OUBox.Items.Count; k++)
                {
                    string item = OUBox.Items[k].ToString();
                    bool check = OUBox.GetItemChecked(k);
                    if (check) OUChecked.Add(item);
                }
                if (OUChecked.Count == 0)
                {
                    for (int k = 0; k < OUBox.Items.Count; k++)
                    {
                        string item = OUBox.Items[k].ToString();
                        OUChecked.Add(item);
                    }
                }
                //Console.WriteLine("OU clause=\n" + getOUClause());
                StringBuilder sb = new StringBuilder(args.Text);
                int i = args.Text.ToUpper().IndexOf("SELECTSITE=");
                while (i - 1 >= 0 && (args.Text[i - 1] == ' ' || args.Text[i - 1] == ',')) i--;
                int j = args.Text.IndexOf(",", i + 1); if (j < 0) j = args.Text.Length;
                if (i >= 0) sb.Remove(i, j - i);
                string plus = "";
                for (int k = 0; k < OUBox.Items.Count; k++)
                    if (OUBox.GetItemChecked(k))
                    {
                        if (plus.Length == 0) plus = "SelectSite=";
                        else plus = plus + "+";
                        plus = plus + OUBox.Items[k].ToString();
                    }
                if (plus.Length == 0) { args.Text = sb.ToString(); return; }
                if (sb.ToString().Trim().Length > 0) sb.Append(", ");
                sb.Append(plus);
                while (sb.Length > 0 && (sb[0] == ' ' || sb[0] == '\n' || sb[0] == '\t' || sb[0] == ',')) sb.Remove(0, 1);
                args.Text = sb.ToString();

                checkWU(sender, e);
            }), null);
        }

        private void dptBox_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                foldArgs();
                dptChecked.Clear();
                for (int k = 0; k < dptBox.Items.Count; k++)
                {
                    string item = dptBox.Items[k].ToString();
                    bool check = dptBox.GetItemChecked(k);
                    if (check) dptChecked.Add(item);
                }
                //Console.WriteLine("OU clause=\n" + getOUClause());
                StringBuilder sb = new StringBuilder(args.Text);
                int i = args.Text.ToUpper().IndexOf("SELECTDPT=");
                while (i - 1 >= 0 && (args.Text[i - 1] == ' ' || args.Text[i - 1] == ',')) i--;
                int j = args.Text.IndexOf(",", i + 1); if (j < 0) j = args.Text.Length;
                if (i >= 0) sb.Remove(i, j - i);
                string plus = "";
                for (int k = 0; k < dptBox.Items.Count; k++)
                    if (dptBox.GetItemChecked(k))
                    {
                        if (plus.Length == 0) plus = "SelectDpt=";
                        else plus = plus + "+";
                        plus = plus + dptBox.Items[k].ToString();
                    }
                if (plus.Length == 0) { args.Text = sb.ToString(); return; }
                if (sb.ToString().Trim().Length > 0) sb.Append(", ");
                sb.Append(plus);
                while (sb.Length > 0 && (sb[0] == ' ' || sb[0] == '\n' || sb[0] == '\t' || sb[0] == ',')) sb.Remove(0, 1);
                args.Text = sb.ToString();

                checkWU(sender, e);
            }), null);
        }

        public int getLarge(bool unattended, int largeChecked_)
        {
            if (!unattended)
                return (largeBox.CheckState.Equals(CheckState.Checked) ? 1 : 0)
                + (large2Box.CheckState.Equals(CheckState.Checked) ? 1 : 0);
            else
                return largeChecked_;
        }

        public string getOUClause(bool unattended,
            List<string> domainChecked_, List<string> OUChecked_, List<string> dptChecked_, List<string> WUChecked_)
        {
            if (!unattended)
                return SCCMParser.getOUClause(domainChecked, OUChecked, dptChecked, WUChecked,
                    domainBox.Items.Count, OUBox.Items.Count, dptBox.Items.Count,
                    ADSPATH);
            else
                return SCCMParser.getOUClause(domainChecked_, OUChecked_, dptChecked_, WUChecked_,
                    domainBox.Items.Count, OUBox.Items.Count, dptBox.Items.Count,
                    ADSPATH);
        }



        private void domainLabel_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < domainBox.Items.Count; i++)
                domainBox.SetItemChecked(i, false);
            domainBox_Click(sender, e);
        }

        private void OULabel_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < OUBox.Items.Count; i++)
                OUBox.SetItemChecked(i, false);
            OUBox_Click(sender, e);
        }


        private void dptLabel_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dptBox.Items.Count; i++)
                dptBox.SetItemChecked(i, false);
            dptBox_Click(sender, e);
        }

        private void adsLabel_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < ADSBox.Items.Count; i++)
                ADSBox.SetItemChecked(i, false);
            ADSBox_Click(sender, e);
        }

        private void dirxmlLabel_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < DIRXMLBox.Items.Count; i++)
                DIRXMLBox.SetItemChecked(i, false);
            DIRXMLBox_Click(sender, e);
        }


        private void freqBox_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                foldArgs();
                StringBuilder sb = new StringBuilder(args.Text);
                int i = args.Text.ToUpper().IndexOf("FREQUENCE");
                while (i - 1 >= 0 && (args.Text[i - 1] == ' ' || args.Text[i - 1] == ',')) i--;
                int j = args.Text.IndexOf(",", i + 1); if (j < 0) j = args.Text.Length;
                if (i >= 0) sb.Remove(i, j - i);
                string plus = "";
                for (int k = 0; k < freqBox.Items.Count; k++)
                    if (freqBox.GetItemChecked(k))
                    {
                        if (plus.Length == 0) plus = "Frequence=";
                        else plus = plus + "+";
                        plus = plus + freqBox.Items[k].ToString();
                    }
                if (plus.Length == 0) {
                    args.Text = sb.ToString();
                }
                else
                {
                    if (sb.ToString().Trim().Length > 0) sb.Append(", ");
                    sb.Append(plus);
                    while (sb.Length > 0 && (sb[0] == ' ' || sb[0] == '\n' || sb[0] == '\t' || sb[0] == ',')) sb.Remove(0, 1);
                    args.Text = sb.ToString();
                }


                if (saveChanges_(false) && !schedBox.Checked
                    && freqBox.SelectedIndices.Count > 0) schedBox.Checked = true;

            }), null);
        }

        private void optButton_Click(object sender, EventArgs e)
        {
            foldArgs();
            showOptions(this.Size.Width < 1100, false);
        }


        void showOptions(bool opt, bool invoke)
        {
            if (invoke) Invoke(new showOptions_Delegate(showOptions_), opt);
            else showOptions_(opt);
        }
        delegate void showOptions_Delegate(bool opt);
        void showOptions_(bool opt)
        {
            this.Size = new Size(opt ? 1100 : 1024, 640);
            optButton.Text = opt ? "Options <" : "Options >";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:vincent.fontaine@MYCOMPANY.com");
        }

        private void adsLabel_Click(object sender, EventArgs e)
        {
            foldArgs();
            int x = 0;
            for (int i = 0; i < ADSBox.Items.Count; i++)
                if (ADSBox.GetItemChecked(i)) x++;
            for (int i = 0; i < ADSBox.Items.Count; i++)
                ADSBox.SetItemChecked(i, x > 0 ? false : true);
            ADSBox_Click(sender, e);
        }

        private void dirxmLabel_Click(object sender, EventArgs e)
        {
            foldArgs();
            int x = 0;
            for (int i = 0; i < DIRXMLBox.Items.Count; i++)
                if (DIRXMLBox.GetItemChecked(i)) x++;
            for (int i = 0; i < DIRXMLBox.Items.Count; i++)
                DIRXMLBox.SetItemChecked(i, x > 0 ? false : true);
            DIRXMLBox_Click(sender, e);
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            statusLabel1.Text = "Lines: " + editor.Text.Split('\n').Length + "   Length: " + editor.TextLength;
            showPosition();
        }


        private void result_Sorted(object sender, EventArgs e)
        {
            for(int i = 0; i < result.RowCount; i++)
                result.Rows[i].HeaderCell.Value = "" + (i+1);
            result.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
            result.AutoResizeColumns();

            for (int i = 0; i < result.ColumnCount; i++)
                if (result.Columns[i].Width > COL_MAX_WIDTH) result.Columns[i].Width = COL_MAX_WIDTH;

        }
        int COL_MAX_WIDTH = 300;


        private void largeBox_CheckedChanged(object sender, EventArgs e)
        {
            /*
            MessageBox.Show("Select option 'Large' for splitting the WQL query into multiple subqueries.\n"
                +"This is useful to help large WQL queries to complete since there is a limit on result size.\n"
                + "One box ticked divides into 10 subqueries (good for subset ranging 1,000 - 5,000)\n"
                + "Two box ticked divides into 100 subqueries (good for subset greater than 5,000)\n"
            , "Options: Large");
            */
        }

        private void large2Box_CheckedChanged(object sender, EventArgs e)
        {
            /*
            MessageBox.Show("Select option 'Large' for splitting the WQL query into multiple subqueries.\n"
                + "This is useful to help large WQL queries to complete since there is a limit on result size.\n"
                + "One box ticked divides into 10 subqueries (good for subset ranging 1,000 - 5,000)\n"
                + "Two box ticked divides into 100 subqueries (good for subset greater than 5,000)\n"
            , "Options: Large");
            */
        }

        private void previewBox_CheckedChanged(object sender, EventArgs e)
        {
            MessageBox.Show("Select option 'Preview' for narrowing the WQL query.\n"
            + "This is useful when authoring a new WQL query since it reduces testing time.\n"
            + "One box ticked narrows to 10% (good for subset ranging 1,000 - 5,000)\n"
            + "Two box ticked narrows to 1% (good for subset greater than 5,000)\n"
            , "Options: Preview");
        }

        private void preview2Box_CheckedChanged(object sender, EventArgs e)
        {
            MessageBox.Show("Select option 'Preview' for narrowing the WQL query.\n"
            + "This is useful when authoring a new WQL query since it reduces testing time.\n"
            + "One box ticked narrows to 10% (good for subset ranging 1,000 - 5,000)\n"
            + "Two box ticked narrows to 1% (good for subset greater than 5,000)\n"
            , "Options: Preview");
        }

        private void editor_KeyPress(object sender, KeyPressEventArgs e)
        {
            showPosition();
        }

        private void editor_Click(object sender, EventArgs e)
        {
            showPosition();
        }

        void showPosition()
        {
            int pos = editor.CurrentPos;
            int ch = pos + 1;
            int col = editor.PointXFromPosition(pos) / 8 - 3;
            //int ln = editor.PointYFromPosition(pos) / 16;
            int ln = editor.Lines.FromPosition(pos).Number;

            //threadsLabel1.Text = "Ln:" + (ln + 1) + " Col:" + col + " Ch:" + ch;
            threadsLabel1.Text = "L" + (ln + 1) + " c" + col + " #" + ch;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;

            browsepub(false);
        }

        int SCHED_HOURSEXTENT = 12;
        double todo_hlate = 0; bool todo_enabled = false;
        string todo_queryname = null, todo_queryargs = null, todo_querydef = null,
            todo_freqstring = null, todo_laststring = null, todo_latestring = null, todo_yesstring = null;

        private void scheduleWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int s = 10; s >= 0; s--) {
                Thread.Sleep(1000);
                if (SCHED_TEST) { Console.Write("~" + s); jobButtonAppendText(true, "~" + s); }
            }
            while (true)
            {
                todo_hlate = 0; todo_enabled = false;
                todo_queryname = null; todo_queryargs = null; todo_querydef = null;
                todo_freqstring = null; todo_laststring = null; todo_latestring = null; todo_yesstring = null;
                DateTime now = DateTime.Now;
                int hour = 0, minute = 0;
                float nowTime = now.Hour + now.Minute / 60f;
                float minTime = 0, maxTime = SCHED_HOURSEXTENT;
                DateTime nowMin = now;

                if (schedBox.Checked)
                {
                    jobButtonAppendText(true, null);
                    jobButtonAppendText(true, "\n  Scheduled Jobs @" + now.ToString("HH:mm yyyy-M-dd") + "\n  ...\n");
                    if (editor.Enabled)
                        if (Directory.Exists(pubBox.Text))
                        {
                            string[] hm = timeBox.Text.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            if (hm != null && hm.Length == 2) try
                                {
                                    hour = int.Parse(hm[0]);
                                    minute = int.Parse(hm[1]);
                                    minTime = hour + minute / 60f;
                                    maxTime = minTime + SCHED_HOURSEXTENT;
                                    nowMin = now.AddHours(-minTime);
                                }
                                catch (Exception ee)
                                {
                                    Console.WriteLine("Invalid schedule datetime: " + timeBox.Text);
                                }

                            bool intime = ((nowTime >= minTime) && (nowTime < maxTime))
                                        || ((nowTime + 24 >= minTime) && (nowTime + 24 < maxTime));

                            List<string> QUERYNAMES = new List<string>(); foreach (string queryname in QUERIES.Keys) QUERYNAMES.Add(queryname);

                            foreach (string queryname in QUERYNAMES)
                            {
                                if (SCHED_TEST) { Console.Write("~"); jobButtonAppendText(true, "~"); }
                                string freq = null, queryargs = null, querydef = null;
                                if (QUERIES.Contains(queryname))
                                {
                                    string[] o = (string[])QUERIES[queryname];
                                    queryargs = o[0]; querydef = o[1];
                                    freq = Regex.Replace(queryargs.ToUpper(),
                                        @"([^,]*,\s*)*FREQUENCE=([A-Z0-9]+).*", "\n$2");
                                    freq = freq.StartsWith("\n") ? freq.Trim() : null;
                                }
                                if (freq != null)
                                {
                                    bool todo = false;
                                    DateTime last = DateTime.MinValue;
                                    string dir0 = AuditSec.APPDATA;
                                    string dir1 = pubBox.Text;
                                    string filestart = "SCCM Reporting-" + queryname + "-";
                                    foreach (string f in Directory.EnumerateFiles(dir0))
                                    {
                                        DateTime datetime = File.GetLastWriteTime(f);
                                        if (Path.GetFileName(f).StartsWith(filestart))
                                        {
                                            if (SCHED_TEST) Console.WriteLine(Path.GetFileName(f)
                                                + " -- " + datetime.ToString("yyyy-M-dd HH:mm"));
                                            if (datetime > last) last = datetime;
                                        }
                                    }

                                    bool inday = true;
                                    DateTime Want = now; switch (freq)
                                    {
                                        case "HOURLY": Want = now.AddHours(-1); break;
                                        case "DAILY": Want = Want.AddDays(-1); break;
                                        case "MON": do { Want = Want.AddDays(-1); } while (Want.DayOfWeek != DayOfWeek.Monday);
                                            inday = nowMin.DayOfWeek == DayOfWeek.Monday; break;
                                        case "TUE": do { Want = Want.AddDays(-1); } while (Want.DayOfWeek != DayOfWeek.Tuesday);
                                            inday = nowMin.DayOfWeek == DayOfWeek.Tuesday; break;
                                        case "WED": do { Want = Want.AddDays(-1); } while (Want.DayOfWeek != DayOfWeek.Wednesday);
                                            inday = nowMin.DayOfWeek == DayOfWeek.Wednesday; break;
                                        case "THU": do { Want = Want.AddDays(-1); } while (Want.DayOfWeek != DayOfWeek.Thursday);
                                            inday = nowMin.DayOfWeek == DayOfWeek.Thursday; break;
                                        case "FRI": do { Want = Want.AddDays(-1); } while (Want.DayOfWeek != DayOfWeek.Friday);
                                            inday = nowMin.DayOfWeek == DayOfWeek.Friday; break;
                                        case "SAT": do { Want = Want.AddDays(-1); } while (Want.DayOfWeek != DayOfWeek.Saturday);
                                            inday = nowMin.DayOfWeek == DayOfWeek.Saturday; break;
                                        case "SUN": do { Want = Want.AddDays(-1); } while (Want.DayOfWeek != DayOfWeek.Sunday);
                                            inday = nowMin.DayOfWeek == DayOfWeek.Sunday; break;
                                        case "MONTH1ST": do { Want = Want.AddDays(-1); } while (Want.Day != 1); break;
                                        case "MONTHEND": do { Want = Want.AddDays(-1); } while (Want.Day != 1); Want.AddDays(-1); break;
                                        default: Want = DateTime.MaxValue; break;
                                    }

                                    double hlate = Want.Subtract(last).TotalHours;
                                    int hlate_ = (int)Math.Floor(hlate);
                                    if (hlate > 0) todo = true;

                                    string freqstring = freq + (freq.Equals("HOURLY") ? "" : "@" + timeBox.Text);
                                    string laststring = last == DateTime.MinValue ? "Never." : last.ToString("yyyy-M-dd (ddd) HH:mm");
                                    string latestring = last == DateTime.MinValue ? null : hlate_ + " hour" + (hlate >= 2 ? "s" : "") + " late";
                                    string yesstring = todo ? "Yes" + (latestring == null ? "" : ", " + latestring)
                                        + (((intime && inday) || freq.Equals("HOURLY")) ? "."
                                        : ", but not allowed during (currentday) ti:me: " + now.ToString("(ddd) HH:mm")) : null;
                                    string nostring = !todo ? "Not yet, " + (-hlate_) + " hour" + (-hlate >= 2 ? "s" : "") + " to go." : null;

                                    string jobstring = "==========JOBSCHED====================================================================\n"
                                        + "     JOB : " + queryname + "\n"
                                        + "     FREQ: " + freqstring + "\n"
                                        + "     LAST: " + laststring + "\n"
                                        + "     TODO: " + (todo ? yesstring : nostring) + "\n";

                                    //if (SCHED_TEST)
                                    Console.WriteLine(jobstring);

                                    jobButtonAppendText(true, jobstring);

                                    if (editor.Enabled && todo && hlate > todo_hlate
                                        && ((intime && inday) || freq.Equals("HOURLY")))
                                    {
                                        todo_queryname = queryname;
                                        todo_queryargs = queryargs;
                                        todo_querydef = querydef;
                                        todo_freqstring = freqstring;
                                        todo_laststring = laststring;
                                        todo_latestring = latestring;
                                        todo_yesstring = yesstring;
                                        todo_hlate = hlate;
                                    }
                                }
                            }

                            if (todo_hlate > 0)
                            {
                                string todostring = "----------JOBTODO---------------------------------------------------------------------\n"
                                    + "  >  JOB : " + todo_queryname + "\n"
                                    + "  >  FREQ: " + todo_freqstring + "\n"
                                    + "  >  LAST: " + todo_laststring + "\n"
                                    + "  >  TODO: " + todo_yesstring + "  ...will start shortly...\n"
                                    ;
                                Console.WriteLine(todostring);
                                jobButtonAppendText(true, "\n" + todostring + "\n\n  You may click  > > > HERE < < <  to suspend all Job scheduling.");
                                jobLabelPerformClick(true);
                                for (int i = 20; i >= 0; i--)
                                {
                                    setStatus("SCHEDULED JOB | " + todo_queryname + "   ...will start shortly... " + i, true);
                                    Thread.Sleep(2000);
                                    setStatus("", true);
                                    Thread.Sleep(500);
                                    if (SCHED_TEST) { Console.Write("~" + i); jobButtonAppendText(true, "~" + i); }
                                    if (!schedBox.Checked) break;
                                }
                                jobButtonHide(true);
                                if (schedBox.CheckState == CheckState.Checked)
                                {
                                    Console.WriteLine("==========JOBSTART====================================================================\n"
                                        + "  >  JOB : " + todo_queryname);
                                    if (!SCHED_TEST)
                                    {
                                        todo_enabled = true;

                                        startButtonPerformClick(true);
                                    }
                                    else Console.WriteLine("Job scheduling: TEST MODE enabled. Job submission cancelled.");
                                }
                            }


                        }
                        else
                        {
                            //Console.WriteLine("Publication directory does not exists: " + pubBox.Text);
                            schedBoxUncheck(true);
                            MessageBox.Show("Job scheduling was suspended because:\n\n    a Publication directory is not yet set.",
                                "Job scheduling");

                        }
                    else
                    {
                        //Console.WriteLine("Busy with another running job.");
                    }
                    for (int s = 30; s >= 0; s--) {
                        Thread.Sleep(1000);
                        if (SCHED_TEST) { Console.Write("~" + s); jobButtonAppendText(true, "~" + s); }
                        if (!schedBox.Checked) break;
                    }
                }
                else {
                    Thread.Sleep(1000);
                    if (SCHED_TEST) { Console.Write("~" + 1); jobButtonAppendText(true, "~" + 1); }
                }
            }
        }
        bool SCHED_TEST = false;//true;




        private void largeBox_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                foldArgs();
                int large = (largeBox.CheckState == CheckState.Checked ? 1 : 0)
                    + (large2Box.CheckState == CheckState.Checked ? 1 : 0);

                StringBuilder sb = new StringBuilder(args.Text);
                int i = args.Text.ToUpper().IndexOf("LARGE=");
                while (i - 1 >= 0 && (args.Text[i - 1] == ' ' || args.Text[i - 1] == ',')) i--;
                int j = args.Text.IndexOf(",", i + 1); if (j < 0) j = args.Text.Length;
                if (i >= 0) sb.Remove(i, j - i);
                string plus = large == 0 ? "" : "Large=" + large;
                if (plus.Length == 0) { args.Text = sb.ToString(); return; }
                if (sb.ToString().Trim().Length > 0) sb.Append(", ");
                sb.Append(plus);
                while (sb.Length > 0 && (sb[0] == ' ' || sb[0] == '\n' || sb[0] == '\t' || sb[0] == ',')) sb.Remove(0, 1);
                args.Text = sb.ToString();
            }), null);
        }

        private void large2Box_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                foldArgs();
                int large = (largeBox.CheckState == CheckState.Checked ? 1 : 0)
                    + (large2Box.CheckState == CheckState.Checked ? 1 : 0);

                StringBuilder sb = new StringBuilder(args.Text);
                int i = args.Text.ToUpper().IndexOf("LARGE=");
                while (i - 1 >= 0 && (args.Text[i - 1] == ' ' || args.Text[i - 1] == ',')) i--;
                int j = args.Text.IndexOf(",", i + 1); if (j < 0) j = args.Text.Length;
                if (i >= 0) sb.Remove(i, j - i);
                string plus = large == 0 ? "" : "Large=" + large;
                if (plus.Length == 0) { args.Text = sb.ToString(); return; }
                if (sb.ToString().Trim().Length > 0) sb.Append(", ");
                sb.Append(plus);
                while (sb.Length > 0 && (sb[0] == ' ' || sb[0] == '\n' || sb[0] == '\t' || sb[0] == ',')) sb.Remove(0, 1);
                args.Text = sb.ToString();
            }), null);
        }

        void foldArgs()
        {
            if (argsButton.Text.StartsWith("^"))
            {
                argsButton.Text = "v v v";
                doFoldArgs();
            }
        }

        static RegexOptions IgnoreCaseSingleLine = RegexOptions.IgnoreCase | RegexOptions.Singleline;

        void doFoldArgs()
        {
            templateLevelBox.Visible = false;
            templateLevelLabel.Visible = false;
            StringBuilder sb = new StringBuilder();
            foreach (char c in args2.Text) if ((c >= 32 && c < 127) || c == '\r' || c == '\n') sb.Append(c);
            sb.Replace('\r', '\n');
            while (sb.Length > 0 && sb[sb.Length - 1] == '\n') sb.Remove(sb.Length - 1, 1);
            string args_ = sb.ToString();
            bool match;
            statusLabel1.Text = "Collapsing arguments..." + args_.Length;
            //Console.WriteLine(args_.Replace("\n", "<LF>\n").Replace("\r", "<CR>\r"));
            do
            {
                match =
                    Regex.IsMatch(args_, @"[\n]{2,}", IgnoreCaseSingleLine);
                if (match) args_ =
                    Regex.Replace(args_, @"[\n]{2,}", "\n", IgnoreCaseSingleLine);
                statusLabel1.Text = "Collapsing arguments..." + args_.Length;
                //Console.WriteLine("\n0----\n" + args_.Replace("\n", "<LF>\n").Replace("\r", "<CR>\r"));
            } while (match);
            do
            {
                match =
                    Regex.IsMatch(args_, @"\n\s{1,}([^=\n]+)\n", IgnoreCaseSingleLine);
                if (match) args_ =
                    Regex.Replace(args_, @"\n\s{1,}([^=\n]+)\n", "\n$1\n", IgnoreCaseSingleLine);
                statusLabel1.Text = "Collapsing arguments..." + args_.Length;
                //Console.WriteLine("\n1a----\n" + args_.Replace("\n", "<LF>\n").Replace("\r", "<CR>\r"));
            } while (match);
            do
            {
                match =
                    Regex.IsMatch(args_, @"\n([^=\n]+)\s{1,}\n", IgnoreCaseSingleLine);
                if (match) args_ =
                    Regex.Replace(args_, @"\n([^=\n]+)\s{1,}\n", "\n$1\n", IgnoreCaseSingleLine);
                statusLabel1.Text = "Collapsing arguments..." + args_.Length;
                //Console.WriteLine("\n1a----\n" + args_.Replace("\n", "<LF>\n").Replace("\r", "<CR>\r"));
            } while (match);
            do
            {
                match =
                    Regex.IsMatch(args_, @"\n([^=\n]+)\n", IgnoreCaseSingleLine);
                if (match) args_ =
                    Regex.Replace(args_, @"\n([^=\n]+)\n", ";$1\n", IgnoreCaseSingleLine);
                statusLabel1.Text = "Collapsing arguments..." + args_.Length;
                //Console.WriteLine("\n1b----\n" + args_.Replace("\n", "<LF>\n").Replace("\r", "<CR>\r"));
            } while (match);
            do
            {
                match =
                    Regex.IsMatch(args_, @"\n(.)", IgnoreCaseSingleLine);
                if (match) args_ =
                    Regex.Replace(args_, @"\n(.)", ", $1", IgnoreCaseSingleLine);
                statusLabel1.Text = "Collapsing arguments..." + args_.Length;
                //Console.WriteLine("\n2----\n" + args_.Replace("\n", "<LF>\n").Replace("\r", "<CR>\r"));
            } while (match);

            args.Text = args_;
            args2.Visible = false;
            filteredArgsBox.Visible = false;
            statusLabel1.Text = "";
        }

        void doUnfoldArgs()
        {
            args2.Text = args.Text.Replace(", ", "\r\n\r\n").Replace(",", "\r\n\r\n");
            filteredArgsBox.Text = "";
            args2.Visible = true;
            filteredArgsBox.Visible = true;
            statusLabel1.Text = "Arguments Editor. Click on ^ ^ ^ to Collapse and return to the Query Editor.";
            templateLevelBox.Visible = true;
            templateLevelLabel.Visible = true;
        }

        private void argsButton_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;
            if (argsButton.Text.StartsWith("v"))
            {
                argsButton.Text = "^ ^ ^";
                doUnfoldArgs();
            }
            else
            {
                argsButton.Text = "v v v";
                doFoldArgs();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            statusLabel1.Text = "Launch 'Windows Management Instrumentation Tester'. Please Connect "
                + @"\\" + AuditSec.settings.smssrv + @"\Root\sms\" + AuditSec.settings.smssite;
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"wbem\wbemtest.exe");
            Process p = Process.Start(psi);
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (msg.Msg == 256)
            {
                if (keyData == (Keys.Control | Keys.S))
                {
                    //Console.WriteLine("CTRL-S was pressed! ");
                    saveButton.PerformClick();
                    return true;
                }
                else if (keyData == Keys.Escape)
                {
                    //Console.WriteLine("ESCAPE was pressed! ");
                    ESCAPE_FLAG = true;
                    SendMessage(dictCombo.Handle.ToInt32(), CB_SHOWDROPDOWN, 0, IntPtr.Zero);
                    return true;
                }
                else if (keyData == Keys.Back)
                {
                    //Console.WriteLine("DELETE was pressed! ");

                    System.Windows.Forms.Control control = this;
                    System.Windows.Forms.ContainerControl container = control as System.Windows.Forms.ContainerControl;
                    while (container != null)
                    {
                        control = container.ActiveControl;
                        container = control as System.Windows.Forms.ContainerControl;
                    }

                    ESCAPE_FLAG = true;
                    SendMessage(dictCombo.Handle.ToInt32(), CB_SHOWDROPDOWN, 0, IntPtr.Zero);

                    control.Focus();
                    return base.ProcessCmdKey(ref msg, keyData);
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void lastparseButton_Click(object sender, EventArgs e)
        {
            if (CX == null || CX.PARSER_DEBUG_PATH == null || !File.Exists(CX.PARSER_DEBUG_PATH)) return;
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = File.Exists(@"C:\Program Files\Notepad++\notepad++.exe") ? @"C:\Program Files\Notepad++\notepad++.exe"
                : Environment.SystemDirectory + @"\notepad.exe";
            psi.Arguments = CX.PARSER_DEBUG_PATH;
            Process p = Process.Start(psi);
        }


        private void queryCombo_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                Graphics g = e.Graphics;
                Rectangle r = e.Bounds;
                string query = queryCombo.Items[e.Index].ToString();
                string args = ((string[])QUERIES[query])[0].ToUpper();
                bool scheduled = args.IndexOf("FREQUENCE=") >= 0;

                string body = ((string[])QUERIES[query])[1].ToUpper();
                int m, n, l; string modified = (m = body.IndexOf("/*MODIFIED BY ")) < 0 ? null :
                    body.Substring(m + 14, l =
                    (n = body.IndexOf("*/", m + 14)) < 0 ? body.Length - m - 14 : n - m - 14);
                modified = modified == null ? null : (m = modified.IndexOf(" ON ")) >= 0 ? modified.Substring(m + 4).Trim() : null;
                bool recent = false; try
                {
                    DateTime t0; double h = -1;  recent = modified != null
                        &&  (h = DateTime.Now.Subtract(
                         t0 = DateTime.ParseExact(modified, "d-MMM-yyyy HH:mm", new CultureInfo("en-US"))).TotalHours) < 24 * 7 * 2;
                    //Console.WriteLine(query + " ---modified--- " + modified + " --h-- " + Math.Round(h));
                }
                catch (Exception ee) {
                    //Console.WriteLine(query + " ---modified--- " + modified + " exception:"  + ee.ToString());
                }


                Color back = queryCombo.BackColor;
                Font font = queryCombo.Font;
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Near;

                if (query.ToLower().StartsWith("_template_ - base template")) back = Color.Gold;
                else if (query.ToLower().StartsWith("_template_ - base variables")) back = Color.Gold;
                else if (query.ToLower().StartsWith("_template_")) back = Color.SandyBrown;
                else if (query.ToLower().StartsWith("_condition_")) back = Color.SandyBrown;
                else if (query.ToLower().StartsWith("file")) back = Color.SkyBlue;
                else if (query.ToLower().StartsWith("workstation")) back = Color.LimeGreen;
                else if (query.ToLower().StartsWith("usage")) back = Color.MediumOrchid;
                else if (query.ToLower().StartsWith("program")) back = Color.RoyalBlue;

                bool bold = e.State != (DrawItemState.NoAccelerator | DrawItemState.NoFocusRect);
                if (bold) font = new Font(font, FontStyle.Bold);
                g.FillRectangle(new SolidBrush(back), r);
                g.DrawString(query, font, new SolidBrush(Color.Black), r, sf);
                if (scheduled) g.DrawImage(!bold ? CLOCK_IMG : CLOCK_IMG_BRIGHT, r.X + r.Width - r.Height, r.Y, r.Height, r.Height);
                if (recent) g.DrawImage(!bold ? RECENT_IMG : RECENT_IMG_BRIGHT, r.X + r.Width - 2 * r.Height, r.Y, r.Height, r.Height);
            }    
        }


        Image CLOCK_IMG = (Image)new System.Resources.ResourceManager("AuditSec.Properties.Resources",
            System.Reflection.Assembly.GetExecutingAssembly()).GetObject("Clock");
        Image RECENT_IMG = (Image)new System.Resources.ResourceManager("AuditSec.Properties.Resources",
            System.Reflection.Assembly.GetExecutingAssembly()).GetObject("Recent");

        Image CLOCK_IMG_BRIGHT = getBrighter((Image)new System.Resources.ResourceManager("AuditSec.Properties.Resources",
            System.Reflection.Assembly.GetExecutingAssembly()).GetObject("Clock"), 50);
        Image RECENT_IMG_BRIGHT = getBrighter((Image)new System.Resources.ResourceManager("AuditSec.Properties.Resources",
            System.Reflection.Assembly.GetExecutingAssembly()).GetObject("Recent"), 50);

        static Image getBrighter(Image Img, int percent)
        {
            float v = percent * 0.01f;
            float[][] colorMatrixElements = {
                new float[] { 1, 0, 0, 0, 0 },
                new float[] { 0, 1, 0, 0, 0 },
                new float[] { 0, 0, 1, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { v, v, v, 0, 1 }
            };
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            Graphics g = default(Graphics);
            Bitmap b = new Bitmap(Convert.ToInt32(Img.Width), Convert.ToInt32(Img.Height));
            g = Graphics.FromImage(b);
            g.DrawImage(Img, new Rectangle(0, 0, b.Width + 1, b.Height + 1), 0, 0, b.Width + 1, b.Height + 1,
                GraphicsUnit.Pixel, imageAttributes);
            return(Image)b;
        }


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, int msg, int wParam, IntPtr lParam);
        public const int CB_SHOWDROPDOWN = 0x14F;

        Point dictComboLocation = new Point(520, 75);
        //Size dictComboSize = new Size(408, 21);
        //Size dictComboSize2 = new Size(200, 21);

        bool ESCAPE_FLAG = false;

        private void editor_KeyUp(object sender, KeyEventArgs e)
        {
            if (ESCAPE_FLAG)
            {
                ESCAPE_FLAG = false;
                return;
            }
            showPosition();

            int i = editor.CurrentPos - 1, j = i;
            string table = null; char c;
            if (i > 0 && (c = editor.Text[i]) == '.')
            {
                if (i > 0) i--;
                while (i > 0 && (char.IsLetterOrDigit(c = editor.Text[i]) || c == '_')) i--;
                if (j - i - 1 >= 0)
                {
                    table = editor.Text.Substring(i + 1, j - i - 1).ToUpper();
                    if (table.Length == 0) table = null;
                }
            }

            int k = editor.CurrentPos - 1, l = i;
            string table2 = null; char c2;
            if (k > 0 && (c2 = editor.Text[k]) == '_')
            {
                if (k > 0) k--;
                while (k > 0 && (char.IsLetterOrDigit(c2 = editor.Text[k]) || c2 == '_')) k--;
                if (l - k - 1 >= 0)
                {
                    table2 = editor.Text.Substring(k + 1, l - k - 1).ToUpper();
                    if (table2.Length == 0) table2 = null;
                }
                if (table2 != null && table2.Length == 0) table2 = null;
                if (table2 != null && !table2.StartsWith("SMS")) table2 = null;
            }

            //Console.WriteLine("KU table=" + table);
            //Console.WriteLine("KU table2=" + table2);

            if (table != null || table2 != null)
            {
                dictCombo.Location = new Point(
                    editor.PointXFromPosition(editor.CurrentPos) + editor.Location.X,
                    editor.PointYFromPosition(editor.CurrentPos) + editor.Location.Y);
                //dictCombo.Size = dictComboSize2;
                dictLabel.Hide();
                SendMessage(dictCombo.Handle.ToInt32(), CB_SHOWDROPDOWN, 1, IntPtr.Zero);
            }
            else
            {
                SendMessage(dictCombo.Handle.ToInt32(), CB_SHOWDROPDOWN, 0, IntPtr.Zero);
            }
        }


        private void dictCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            //bool table = editor.CurrentPos > 0 && editor.Text[editor.CurrentPos - 1] == '.';
            if (dictCombo.SelectedIndex > 0)
            {
                string insert = dictCombo.Items[dictCombo.SelectedIndex].ToString();
                editor.InsertText(insert);
            }
            editor.Focus();
            dictCombo.Items.Clear();
            dictCombo.Text = "";
        }


        private void dictCombo_DropDown(object sender, EventArgs e)
        {
            int i = editor.CurrentPos - 1, j = i;
            string table = null; char c;
            if (i > 0 && (c = editor.Text[i]) == '.')
            {
                if (i > 0) i--;
                while (i > 0 && (char.IsLetterOrDigit(c = editor.Text[i]) || c == '_')) i--;
                if (j - i - 1 >= 0)
                {
                    table = editor.Text.Substring(i + 1, j - i - 1).ToUpper();
                    if (table.Length == 0) table = null;
                }
            }
            if (table != null)
            {
                //Console.WriteLine("table=" + table);
                Hashtable TABLENAME = new Hashtable();
                SCCMParser.parseTables(editor.Text, TABLENAME);
                if (TABLENAME.Contains(table)) table = ((string)TABLENAME[table]).ToUpper();
            }
            if (table != null) table = table + '.';

            int k = editor.CurrentPos - 1, l = i;
            string table2 = null; char c2;
            if (k > 0 && (c2 = editor.Text[k]) == '_')
            {
                if (k > 0) k--;
                while (k > 0 && (char.IsLetterOrDigit(c2 = editor.Text[k]) || c2 == '_')) k--;
                if (l - k - 1 >= 0)
                {
                    table2 = editor.Text.Substring(k + 1, l - k - 1).ToUpper();
                    if (table2.Length == 0) table2 = null;
                }
                if (table2 != null && table2.Length == 0) table2 = null;
                if (table2 != null && !table2.StartsWith("SMS")) table2 = null;
                else table2 += "_";
            }

            //Console.WriteLine("DD table=" + table);
            //Console.WriteLine("DD table2=" + table2);

            dictCombo.Items.Clear();
            if (table == null)
            {
                var filter = dictComboItems.Where(x => x.IndexOf('.') == -1);
                if (table2 != null)
                    filter = filter.Where(x => x.ToUpper().StartsWith(table2)).Select(x => x.Substring(table2.Length));
                if (filter.Count() > 0)
                {
                    //Console.WriteLine(filter.Aggregate((x, y) => x + "\n" + y));
                    dictCombo.Items.AddRange(filter.ToArray());
                }
            }
            else
            {
                var filter = dictComboItems.Where(x => x.ToUpper().StartsWith(table));
                if (filter.Count() > 0)
                {
                    filter = filter.Select(x => x.Substring(table.Length));
                    //Console.WriteLine(filter.Aggregate((x, y) => x + "\n" + y));
                    dictCombo.Items.AddRange(filter.ToArray());
                }
                dictCombo.Items.Add(" " + table);
                //dictCombo.SelectedText = " " + table;
            }
            dictCombo.Focus();
        }

        private void dictCombo_DropDownClosed(object sender, EventArgs e)
        {
            dictCombo.Location = dictComboLocation;
            //dictCombo.Size = dictComboSize;
            dictLabel.Show();
        }

        private void editor_Scroll(object sender, ScrollEventArgs e)
        {
            SendMessage(dictCombo.Handle.ToInt32(), CB_SHOWDROPDOWN, 0, IntPtr.Zero);
        }

        bool startButton_runDirect = false;
        bool DEBUG = true;
        private void startButton2_Click(object sender, EventArgs e)
        {
            startButton.PerformClick();
            startButton_runDirect = true;
            DEBUG = false;
        }

        private void jobButton_Click(object sender, EventArgs e)
        {
            if (schedBox.Checked)
            {
                schedBox.Checked = false;
                MessageBox.Show("Job scheduling suspended.\n\nTick the Sched box to resume.", "Job scheduling");
            }
            jobButton.Visible = false;
        }

        private void jobLabel_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;
            jobLabelPerformClick();
        }

        void jobLabelPerformClick(bool invoke)
        {
            if (invoke) Invoke(new jobLabelPerformClick_Delegate(jobLabelPerformClick));
            else jobLabelPerformClick();
        }
        public delegate void jobLabelPerformClick_Delegate();
        void jobLabelPerformClick()
        {
            showOptions(true, true);
            jobButton.Visible = true;
            jobButton.Focus();
        }

        void schedBoxUncheck(bool invoke)
        {
            if (invoke) Invoke(new schedBoxUncheck_Delegate(schedBoxUncheck));
            else schedBoxUncheck();
        }
        public delegate void schedBoxUncheck_Delegate();
        void schedBoxUncheck()
        {
            schedBox.Checked = false;
        }

        void jobButtonHide(bool invoke)
        {
            if (invoke) Invoke(new jobButtonHide_Delegate(jobButtonHide));
            else jobButtonHide();
        }
        public delegate void jobButtonHide_Delegate();
        void jobButtonHide()
        {
            jobButton.Visible = false;
        }

        void jobButtonAppendText(bool invoke, string text)
        {
            if (invoke) Invoke(new jobButtonAppendText_Delegate(jobButtonAppendText), text);
            else jobButtonAppendText(text);
        }
        public delegate void jobButtonAppendText_Delegate(string text);
        void jobButtonAppendText(string text)
        {
            if (text == null) jobButton.Text = "";
            else
            {
                jobButton.Text += text;

                List<string> lines = new List<string>(jobButton.Text.Replace("\r\n", "\n").Split(new char[] { '\n' }).Reverse()),
                linesend = new List<string>();
                int i = 22; foreach (string line in lines) { linesend.Add(line); if (i-- <= 0) break; }
                linesend.Reverse();
                jobButton.Text = linesend.Aggregate((x, y) => x + "\n" + y);

            }
        }

        private void freqBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                IEnumerator myEnumerator;
                myEnumerator = freqBox.CheckedIndices.GetEnumerator();
                int y;
                while (myEnumerator.MoveNext() != false)
                {
                    y = (int)myEnumerator.Current;
                    freqBox.SetItemChecked(y, false);
                }
            }
        }

        private void queryCombo_DropDown(object sender, EventArgs e)
        {
            jobButtonHide();
        }


        private void timeBox_TextChanged(object sender, EventArgs e)
        {
            string[] hm = timeBox.Text.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (hm != null && hm.Length == 2) try
                {
                    int hour = int.Parse(hm[0]);
                    int minute = int.Parse(hm[1]);
                    int hour2 = (hour + SCHED_HOURSEXTENT) % 24;
                    timeBox2.Text = (hour2 < 10 ? "0" : "") + hour2 + ":" + (minute < 10 ? "0" : "") + minute;
                }
                catch (Exception ee)
                {
                    Console.WriteLine("Invalid schedule datetime: " + timeBox.Text);
                    timeBox2.Text = "00:00";
                }
        }



        private void loadSchedButton_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;
            enableGUI(false);
            loadSched(true);
            setStatus("", true);
            enableGUI(true);
        }

        private void saveSchedButton_Click(object sender, EventArgs e)
        {
            if (freezeGUI) return;
            enableGUI(false);
            saveSched(true);
            setStatus("", true);
            enableGUI(true);
        }

        bool loadSched(bool show)
        {
            int i = 0; try
            {
                List<string[]> schedules = new List<string[]>();
                string[] schedule = null; int j = 0;
                if (File.Exists(queriessched))
                    foreach (string line in File.ReadAllLines(queriessched))
                        if (!line.StartsWith("/*"))
                        {
                            if (schedule == null) { schedule = new string[3]; j = 0; }
                            schedule[j++] = line;
                            if (j == 3) { schedules.Add(schedule); schedule = null; }
                        }
                string selected = queryCombo.SelectedText;
                queryCombo.SelectedText = "";
                foreach (string[] sched in schedules)
                {
                    string queryname = sched[0].Trim();
                    string frequence = sched[1].Trim();
                    string notify = sched[2].Trim();

                    if (QUERIES.Contains(queryname))
                    {
                        string[] o = (string[])QUERIES[queryname],
                            o2 = new string[] { SCCMParser.removeArgDuplicates(o[0] + ", Frequence=" + frequence + ", Notify=" + notify), o[1] };
                        QUERIES[queryname] = o2;
                        i++;
                        if (show) setStatus("Schedule loading: " + i + " ...", true);
                    }
                }
                queryCombo.SelectedText = selected;
                if (show) MessageBox.Show(i + " Job schedule" + (i >= 2 ? "s" : "") + " have been loaded from file:\n\n\t" + queriessched, "Schedule load");
                return true;
            }
            catch (Exception ee)
            {
                Console.WriteLine("Schedule load: " + ee.ToString());
                MessageBox.Show(i + " Job schedule" + (i >= 2 ? "s" : "") + " have been loaded from file:\n\n\t" + queriessched
                    + "\n\n" + ee.Message, "Schedule load");
                return false;
            }
        }

        bool saveSched(bool show)
        {
            int i = 0; try
            {
                StringBuilder sb = new StringBuilder();
                List<string> QUERYNAMES = new List<string>(); foreach (string queryname in QUERIES.Keys) QUERYNAMES.Add(queryname);
                foreach (string queryname in QUERYNAMES)
                {
                    if (QUERIES.Contains(queryname))
                    {
                        string[] o = (string[])QUERIES[queryname];
                        string queryargs = o[0];
                        if (queryargs.ToUpper().IndexOf("FREQUENCE=") >= 0)
                        {
                            string frequence = Regex.Replace(queryargs,
                                @"([^,]*,\s*)*FREQUENCE=([^,]*).*", "\n$2", RegexOptions.IgnoreCase);
                            frequence = frequence.StartsWith("\n") ? frequence.Trim() : null;
                            if (frequence != null && frequence.Length == 0) frequence = null;
                            string notify = Regex.Replace(queryargs,
                                @"([^,]*,\s*)*NOTIFY=([^,]*).*", "\n$2", RegexOptions.IgnoreCase);
                            notify = notify.StartsWith("\n") ? notify.Trim() : "";
                            if (frequence != null)
                            {
                                i++;
                                if (show) setStatus("Schedule saving: " + i + " ...", true);
                                string s = queryname + "\n\t" + frequence + "\n\t" + notify;
                                sb.AppendLine(s);
                                Console.WriteLine(s);
                            }
                        }
                    }
                }
                sb.AppendLine("/*Modified by " + UserPrincipal.Current.SamAccountName
                + " on " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy HH:mm}", DateTime.Now) + "*/");

                File.WriteAllText(queriessched, sb.ToString());
                if (show) MessageBox.Show(i + " Job schedule" + (i >= 2 ? "s" : "") + " have been saved into file:\n\n\t" + queriessched
                    //+ "\n\n" + sb.ToString()
                    , "Schedule save");
                if (!File.Exists(queriessched)) return false;
                if (show)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = File.Exists(@"C:\Program Files\Notepad++\notepad++.exe") ? @"C:\Program Files\Notepad++\notepad++.exe"
                        : Environment.SystemDirectory + @"\notepad.exe";
                    psi.Arguments = queriessched;
                    Process p = Process.Start(psi);
                }
                return true;
            }
            catch (Exception ee)
            {
                Console.WriteLine("Schedule save: " + ee.ToString());
                MessageBox.Show(i + " Job schedule" + (i >= 2 ? "s" : "") + " have been saved into file:\n\n\t" + queriessched
                    + "\n\n" + ee.Message, "Schedule save");
                return false;
            }
        }

        bool hasSched()
        {
            int i = 0; 
            List<string> QUERYNAMES = new List<string>(); foreach (string queryname in QUERIES.Keys) QUERYNAMES.Add(queryname);
            foreach (string queryname in QUERYNAMES)
            {
                if (QUERIES.Contains(queryname))
                {
                    string[] o = (string[])QUERIES[queryname];
                    string queryargs = o[0];
                    if (queryargs.ToUpper().IndexOf("FREQUENCE=") >= 0)
                    {
                        string frequence = Regex.Replace(queryargs,
                            @"([^,]*,\s*)*FREQUENCE=([^,]*).*", "\n$2", RegexOptions.IgnoreCase);
                        frequence = frequence.StartsWith("\n") ? frequence.Trim() : null;
                        if (frequence != null && frequence.Length == 0) frequence = null;
                        if (frequence != null) return true;
                    }
                }
            }
            return false;      
        }



        public delegate void checkScheduleStartDelegate();

        void checkScheduleStart()
        {
            bool sched = false;
            List<string> QUERYNAMES = new List<string>(); foreach (string queryname in QUERIES.Keys) QUERYNAMES.Add(queryname);
            foreach (string queryname in QUERYNAMES)
            {
                if (QUERIES.Contains(queryname))
                {
                    string[] o = (string[])QUERIES[queryname];
                    string queryargs = o[0];
                    if (queryargs.ToUpper().IndexOf("FREQUENCE=") >= 0) { sched = true; break; }
                }
            }
            if (sched)
            {
                string YES = Interaction.InputBox("Job schedules are defined.\n\n"
                    + "Do you want to start the Job scheduling engine now ?",
                    "Job scheduling", "Yes");
                if (YES != null && YES.ToUpper().Equals("YES")) schedBox.Checked = true;
            }
        }

        private void WUBox_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                foldArgs();
                WUChecked.Clear();
                for (int k = 0; k < WUBox.Items.Count; k++)
                {
                    string item = WUBox.Items[k].ToString();
                    bool check = WUBox.GetItemChecked(k);
                    if (check) WUChecked.Add(item);
                }
                //Console.WriteLine("OU clause=\n" + getOUClause());
                StringBuilder sb = new StringBuilder(args.Text);
                int i = args.Text.ToUpper().IndexOf("SELECTOBJECT=");
                while (i - 1 >= 0 && (args.Text[i - 1] == ' ' || args.Text[i - 1] == ',')) i--;
                int j = args.Text.IndexOf(",", i + 1); if (j < 0) j = args.Text.Length;
                if (i >= 0) sb.Remove(i, j - i);
                string plus = "";
                for (int k = 0; k < WUBox.Items.Count; k++)
                    if (WUBox.GetItemChecked(k))
                    {
                        if (plus.Length == 0) plus = "SelectObject=";
                        else plus = plus + "+";
                        plus = plus + WUBox.Items[k].ToString();
                    }
                if (plus.Length == 0) { args.Text = sb.ToString(); return; }
                if (sb.ToString().Trim().Length > 0) sb.Append(", ");
                sb.Append(plus);
                while (sb.Length > 0 && (sb[0] == ' ' || sb[0] == '\n' || sb[0] == '\t' || sb[0] == ',')) sb.Remove(0, 1);
                args.Text = sb.ToString();
            }), null);

        }


        private void templateLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i;
            if ((i = templateLevelBox.SelectedIndex) >= 0)
            {
                args2.Text = Regex.Replace(args2.Text, @"[\r\n]*\s*TemplateLevel\s*=\s*[^\r\n]+", "");
                args2.Text += "\n\nTemplateLevel=" + templateLevelBox.Items[i];
            }
        }

        private void templateLevel_VisibleChanged(object sender, EventArgs e)
        {
            if (!templateLevelBox.Visible)
            {
                templateLevelBox.SelectedItem = null;
                return;
            }
            templateLevelBox.Items.Clear();
            templateLevelBox.SelectedText = "";
            foreach (string queryname in QUERIES.Keys)
            {
                if (queryname.ToLower().StartsWith("_template_ - base template - level "))
                    templateLevelBox.Items.Add(queryname.Substring(35));
            }
            string level = null;
            if (Regex.IsMatch(args2.Text, @"TemplateLevel\s*=", SCCMParser.IgnoreCaseSingleLine))
                level = Regex.Replace(args2.Text, @"^.*TemplateLevel\s*=\s*([^\r\n]+).*$", "$1", SCCMParser.IgnoreCaseSingleLine);
            if (level != null)
                if (templateLevelBox.Items.Contains(level)) templateLevelBox.SelectedItem = level;
        }

        Hashtable WMIcache = new Hashtable();
        object getWMI(string func, object arg)
        {
            switch(func)
            {
                case "COLLCOUNT":
                    //string query = "SELECT ResourceID FROM SMS_CM_RES_COLL_" + arg;
                    string query = "SELECT __CLASS FROM SMS_CollectionMember_a WHERE CollectionID=\"" + arg + "\"";
                    if (WMIcache.Contains(query))
                        return WMIcache[query];
                    else try
                    {
                        ManagementScope scope = new ManagementScope(@"\\" + AuditSec.settings.smssrv + @"\Root\sms\" + AuditSec.settings.smssite);
                        ManagementObjectCollection col = new ManagementObjectSearcher(scope, new ObjectQuery(query),
                            new EnumerationOptions() { ReturnImmediately = true, Rewindable = false }).Get();
                        int result = col.Count;
                        col.Dispose(); col = null;
                        if (!WMIcache.Contains(query))
                        {
                            WMIcache.Add(query, result);
                            Console.WriteLine("Query   : " + func + " " + arg + " = " + result);
                        }
                        return result;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Query   " + func + " " + arg + " = error: " + e.ToString());
                        return -1;
                    }

                default:
                    Console.WriteLine("Query   " + func + " " + arg + ": unknown function " + func);
                    return arg;
            
            }
        }

        SCCMCheck sccmcheck = null;
        ComputerManagement compmgmt = null;
        ComputerReenabler compenab = null;
        
        private void result_DoubleClick(object sender, EventArgs e)
        {
            if (result.SelectedCells.Count != 1) return;
            string sel = result.SelectedCells[0].Value == null ? "" : result.SelectedCells[0].Value.ToString().Trim();
            if (sel.Length == 0) return;

            MachineActionGUI dlg = new MachineActionGUI(sel);
            DialogResult r = dlg.ShowDialog();
            if (r == DialogResult.OK && dlg.Radio == 0)
            {
                if (sccmcheck == null) sccmcheck = new SCCMCheck();
                sccmcheck.setComputer(sel);
                sccmcheck.ShowDialog();
            }
            else if (r == DialogResult.OK && dlg.Radio == 1)
            {
                if (compmgmt == null) compmgmt = new ComputerManagement();
                compmgmt.setMachine(sel);
                compmgmt.ShowDialog();
            }
            else if (r == DialogResult.OK && dlg.Radio == 2)
            {
                if (compenab == null) compenab = new ComputerReenabler();
                compenab.setMachine(sel);
                compenab.ShowDialog();
            }       
            else  if (r == DialogResult.OK && dlg.Radio == 3)
            {
                if (File.Exists(SMSCC))
                {
                    Process smscc = new Process();
                    smscc.StartInfo.FileName = SMSCC;
                    smscc.StartInfo.Arguments = sel;
                    smscc.Start();
                }
                else
                {
                     MessageBox.Show("External program not found:\n" + SMSCC, "Machine Action");
                }
            }
        
        }

        static string SMSCC = @"C:\Program Files\SCCM Tools\SCCM Client Center\SMSCliCtrV2.exe";



        class MachineActionGUI : Form
        {
            private Button okButton;
            private Button cancelButton;
            private GroupBox radiogroup;
            private RadioButton radio1, radio2, radio3, radio4;

            public int Radio;

            void OnRadio(Object sender, EventArgs e)
            {
                int n = 0;
                foreach (Object o in radiogroup.Controls)
                {
                    if (o is RadioButton)
                    {
                        RadioButton r = (RadioButton)o;
                        if (r.Checked)
                            Radio = n;
                        n++;
                    }
                }
            }

            public MachineActionGUI(string machine)
            {
                Size = new Size(400, 245);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                Text = "Machine Action";

                okButton = new Button();
                okButton.DialogResult = DialogResult.OK;
                okButton.Location = new Point(20, 175);
                okButton.Size = new Size(80, 25);
                okButton.Text = "OK";
                Controls.Add(okButton);

                cancelButton = new Button();
                cancelButton.Location = new Point(300, 175);
                cancelButton.Size = new Size(80, 25);
                cancelButton.Text = "Cancel";
                cancelButton.DialogResult = DialogResult.Cancel;
                Controls.Add(cancelButton);

                radiogroup = new GroupBox();
                radiogroup.Text = "Choose what action to perform on " + machine;
                radiogroup.Location = new Point(10, 30);
                radiogroup.Size = new Size(380, 110);
                Controls.Add(radiogroup);

                radio1 = new RadioButton();
                radio1.Location = new Point(10, 15);
                radio1.Size = new Size(360, 25);
                radio1.Click += new EventHandler(OnRadio);
                radio1.Text = "SCCM Check";
                radiogroup.Controls.Add(radio1);

                radio2 = new RadioButton();
                radio2.Location = new Point(10, 40);
                radio2.Size = new Size(360, 25);
                radio2.Click += new EventHandler(OnRadio);
                radio2.Text = "Computer Management";
                radiogroup.Controls.Add(radio2);

                radio3 = new RadioButton();
                radio3.Location = new Point(10, 65);
                radio3.Size = new Size(360, 25);
                radio3.Click += new EventHandler(OnRadio);
                radio3.Text = "Computer reEnabler";
                radiogroup.Controls.Add(radio3);

                radio4 = new RadioButton();
                radio4.Location = new Point(10, 90);
                radio4.Size = new Size(360, 25);
                radio4.Click += new EventHandler(OnRadio);
                radio4.Text = "SCCM Client Center";
                radiogroup.Controls.Add(radio4);
            }

        }


        Online online;

        bool RefreshRowHeader(string machine)
        {
            onlineWorker.ReportProgress(0, machine);
            return true;
        }

        bool RefreshRowHeader_(string machine)
        {
            int rowi = getRowIndex(machine); if (rowi >= 0)
            {
                result.InvalidateRow(rowi);
                return true;
            }
            else return false;
        }

        int getRowIndex(string machine)
        {
            if (result.Columns.Count == 0
                || !"COMPUTER".Equals(result.Columns[0].Name.ToUpper()))
                return -1;
            else foreach(DataGridViewRow row in result.Rows)
                if (machine.Equals(row.Cells[0].Value.ToString()))
                    return row.Index;
            return -1;
        }

        private void result_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (pingBox.Checked
                && result.Columns.Count > 0
                && "COMPUTER".Equals(result.Columns[0].Name.ToUpper())
                && result.Rows[e.RowIndex].Cells[0].Value != null)
            {
                string machine = result.Rows[e.RowIndex].Cells[0].Value.ToString();
                Icon icon = online.getIcon(machine);
                int xPosition = e.RowBounds.X + result.RowHeadersWidth - icon.Width;
                int yPosition = e.RowBounds.Y + ((result.Rows[e.RowIndex].Height - icon.Height) / 2);
                Rectangle rectangle = new Rectangle(xPosition, yPosition, icon.Width, icon.Height);
                try
                {
                    e.Graphics.DrawIcon(icon, rectangle);
                }
                catch (Exception ee)
                {
                    Console.WriteLine("RowPostPaint Error: " + ee.Message);
                }
            }
        }

        private void onlineWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            online.Start();
        }

        private void onlineWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            RefreshRowHeader_(e.UserState.ToString());
        }

        private void pingBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!pingBox.Checked) result.Invalidate();
        }

        private void SMSSERVER_Box_Changed(object sender, EventArgs e)
        {
            foreach(string[] row in MYCOMPANY_Settings.SMS_SERVER_SITE_DESC)
                if (row[0] == SMSSERVER_Box.SelectedItem.ToString())
                {
                    string server = row[0];
                    string site = row[1];
                    string desc = row[2];
                    SMSSITE_Box.Text = site;
                    SMSDESC_Box.Text = desc;
                    AuditSec.InitializeSMS(true, server, site);

                    statusLabel1.Text = @"SMS=\\" + AuditSec.settings.smssrv + @"\Root\sms\" + AuditSec.settings.smssite
                    + " - ADS=LDAP://" + new PrincipalContext(ContextType.Domain).ConnectedServer
                    + " - DIRXML=" + AuditSec.defaultLdap;
                }
        }

        private void SCCMReporting_Resize(object sender, EventArgs e)
        {
            result.Height = 262 + this.Height - this.MinimumSize.Height;
        }






    }
}
