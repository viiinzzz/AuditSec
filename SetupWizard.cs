using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Security.Policy;
using Microsoft.Win32;
using DeploymentUtilities;


namespace AuditSec
{
    public partial class SetupWizard : Form
    {

        static public string[] tools_command = new string[] {
            "unlocker", "pcmanagement", "auditsec", "mbam", "groupedit", "hotapps", 
            "sccmreporting", "sccmcheck", "sccmdesign", "regcol", "wmicol", "sizecol",
            "staffdetails",
            "update",
        //hidden commands
            "sdqspeak"
        };

        static public string[] tools_desc = new string[] {
            "AD Account Unlocker", "PC Management", "Workstations Security Audit", "Bitlocker Drive Recovery", "AD Group Editor", "Hot Applications Check", 
            "SCCM Reporting", "SCCM Client Check", "SCCM Collection Designer", "Multiple PC Registry Collector", "Multiple PC WMI Collector", "Multiple PC Profile Size Collector",
            "Show Staff Details",
            "Update (please runas admin) IT Ops Tools Suite",
        //
            ""
        };

        static public string[] tools_icon = new string[] {
            "expired", "mgmt", "search", "bitlocker", "Dpt1", "mgmt", 
            "repo", "test", "collection", "reg", "reg", "dirsize1",
            "Dpt1",
            "upda",
        //
            ""
        };

        static public bool[] tools_defaultsel = new bool[] {
            true, true, true, false, false, false, 
            false, true, false, false, false, false,
            false,
            false,
        //
            false
        };

        static public new Func<Form>[] tools_new = new Func<Form>[] {
            () => new Unlocker(), () => new ComputerManagement(), () => new AuditSecGUIForm(), () => new MBAM_GUI(), () => new GroupEditor(), () => new HotApps(), 
            () => new SCCMReporting(), () => new SCCMCheck(), () => new SCCMCollectionDesigner(), () => new RegCollector(), () => new WMICollector(), () => new SizeCollector(),
            () => new StaffDetails_GUI(false),
            () => new Form(),
        //
            () => new SDQueueSpeak_GUI(false)
        };


        public static SetupWizard wizardInstance = null;

        public SetupWizard()
        {
            wizardInstance = this;
            InitializeComponent();
            curverBox.Text = AuditSec.curver;
            for(int i = 0; i < tools_command.Length; i++)
                addCheckBox(tools_command[i], "        " + tools_desc[i], tools_icon[i], tools_defaultsel[i]);
        }

        private void Setup_Click(object sender, EventArgs e)
        {
            createDesktopShortcuts();
            Setup.Visible = false;
            if (selectedCount == 0)
                try
                {
                    DeploymentUtils.UninstallMe(x => { logsBox.Visible = true; logsBox.Text += x + "\r\n"; return true; });
                    label2.Text = "The uninstall is now finished.";
                    label1.Text = "Bye.";
                }
                catch (Exception ee)
                {
                    label2.Text = "The uninstall failed.";
                    label1.Text = ee.Message;
                }
            else
            {
                label2.Text = "The setup is now finished.";
                label1.Text = "Use the shortcuts on your desktop to run the tools.";
            }
            label1.Visible = true;
        }

        private void SelectNone_Click(object sender, EventArgs e)
        {
            foreach (CheckBox c in getCheckBoxes()) c.Checked = false;
        }



        Image getIcon(string icon)
        {
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(
                    "AuditSec.Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            object obj = rm.GetObject(icon);
            if (obj is Image) return (Image)obj;
            else if (obj is Icon) return ((Icon)obj).ToBitmap();
            else return null;
        }

        void addCheckBox(string name, string label, string icon, bool defaultsel)
        {
            if (name != "update")
            {
                int n = getCheckBoxes().Count;
                int items_per_column = 8;
//                int items_per_row = 2;
                int x = n / items_per_column;
                int y = n % items_per_column;
//                int x = n % items_per_row;
//                int y = n / items_per_row;
//                Console.WriteLine("X=" + x + " Y=" + y + " " + label);

                int column_gap = 250;
                int row_gap = 30;
                int x0 = 100;
                int y0 = 100;
                System.Windows.Forms.CheckBox box = new System.Windows.Forms.CheckBox();
                box.AutoSize = true;
                box.Location = new System.Drawing.Point(x0 + x * column_gap, y0 + y * row_gap);
                box.Name = name;
                box.MinimumSize = box.MaximumSize = box.Size = new System.Drawing.Size(column_gap-30, row_gap);
                box.TabIndex = n;
                box.Text = label;
                box.UseVisualStyleBackColor = true;
                box.CheckedChanged += (o, e) => {
                    selectedCount += box.Checked ? 1 : -1;
                    Setup.Text = selectedCount == 0 ? "Uninstall Application" : "Setup Shortcuts";
                };
                Image image = getIcon(icon);
                if (image != null) image = new Bitmap(image, new Size(24, 24));
                box.Image = image;
                box.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
                Controls.Add(box);
                box.Checked = defaultsel;

                System.Windows.Forms.LinkLabel test = new System.Windows.Forms.LinkLabel();
                test.Location = new System.Drawing.Point(x0-30 + x * column_gap, y0+7 + y * row_gap);
                test.Name = name + "_test";
                test.Size = new System.Drawing.Size(30, 20);
                test.Text = "test";
                test.Click += (o, e) => {
                    this.Visible = false;
                    test.Visible = false;
                    AuditSec.readSettings();
                    Thread T = new Thread(new ThreadStart(() =>
                        {
                            Console.WriteLine("\n\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> " + Thread.CurrentThread.Name + "\n");
                            try
                            {
                                Application.Run(tools_new[n]());
                            }
                            catch (Exception ee)
                            {
                                Console.WriteLine("Oops! Seems this tool cannot be tested here.\n" + ee.ToString());
                            }
                            this.Invoke(new showWizardDelegate(() => { wizardInstance.Visible = true; }));
                        }));
                    T.Name = "IT_Ops_Tools_Suite-"+name + "_test"+"-Thread";
                    T.SetApartmentState(ApartmentState.STA);
                    T.Start();
                };
                Controls.Add(test);
            }
        }

        int selectedCount = 0;

        delegate void showWizardDelegate();



        List<CheckBox> getCheckBoxes()
        {
            return Controls.Cast<Component>().Where(c => c is CheckBox).Cast<CheckBox>().ToList();
        }

        List<LinkLabel> getLinkLabels()
        {
            return Controls.Cast<Component>().Where(c => c is LinkLabel).Cast<LinkLabel>().ToList();
        }

        public List<string> getSelectedTools()
        {
            return getCheckBoxes().Where(c => c.Checked).Select(c => c.Name).ToList();
        }


        public void createDesktopShortcuts()
        {
            Console.WriteLine("Creating desktop shortcuts...");
            label1.Visible = SelectAll.Visible = SelectNone.Visible = false;
            List<string> selected = getSelectedTools();
            for (int i = 0; i < tools_command.Length; i++) if (tools_desc[i].Length > 0)
            {
                string command = tools_command[i], desc = tools_desc[i], icon = tools_icon[i];
                string lnkpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), desc + ".lnk");
                CheckBox box = tools_command[i] == "update" ? null : getCheckBoxes().First(c => c.Name == tools_command[i]);
                LinkLabel test = tools_command[i] == "update" ? null : getLinkLabels().First(c => c.Name == tools_command[i] + "_test");
                if (box != null) box.Enabled = false;
                if (test != null) test.Visible = false;
                try
                {
                    if (selected.Contains(command) || command == "update")
                    {
                        if (!AuditSec.exportResource(icon, Path.Combine(
                            Application.StartupPath, icon + ".ico"), false))
                            Console.WriteLine("Cannot create desktop icon. (" + icon + ")");
                        IWshRuntimeLibrary.IWshShortcut lnk = (IWshRuntimeLibrary.IWshShortcut)new IWshRuntimeLibrary.WshShellClass().CreateShortcut(lnkpath);

                        if (command == "update")
                            lnk.TargetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "MYCOMPANY\\AuditSec.appref-ms");
                        else
                        {
                            lnk.TargetPath = Application.ExecutablePath;
                            lnk.Arguments = "/" + command;
                        }
                        lnk.Description = "Launch " + desc + ".";
                        lnk.IconLocation = Path.Combine(Application.StartupPath, icon + ".ico");
                        lnk.Save();
                    }
                    else
                    {
                        if (box != null) box.Checked = false;
                        if (File.Exists(lnkpath)) File.Delete(lnkpath);
                        if (box != null) box.Visible = false;
                    }
                }
                catch (Exception e)
                {
                    if (box != null) box.Checked = false;
                    Console.WriteLine("createDesktopShortcuts error: " + lnkpath + ": " + e.Message);
                }

            }
        }


       public static bool switchArgs(string[] args)
        {
            try
            {
                bool nomatch = true;
                for (int i = 0; i < tools_command.Length; i++)
                {
                    string command = tools_command[i];
                    Func<Form> constr = tools_new[i];
                    if (args.Contains("/" + command) && command != "update")
                    {
                        nomatch = false;
                        Console.WriteLine("Launching appropriate tool...");
                        AuditSec.readSettings();
                        Application.Run(constr());
                    }

                }
                if (nomatch)
                {
                    /*if (!isAdministrator())
                    {
                        MessageBox.Show("The setup needs to run with Elevated rights in order to execute properly.\n\n"
                            + "Please right-click 'setup.exe' and then click 'Run as administrator'.",
                            "IT O p s  T o o l s  S u i t e");
                        return true;
                    }
                    else */return false;
                }
                else return true;
            }
            catch (Exception e)
            {
                AuditSec.Exit("Fatal Error:\n\n" + e.ToString(), null);
                return true;
            }
        }


        static bool isAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool admin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!admin) return false;

            string test = Path.Combine(Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), "MYCOMPANY"), "sccmqueries.wql.readme");
            bool copy = false; try
            {
                copy = AuditSec.exportResource("sccmqueries_wql", test, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return copy;
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {
            foreach (CheckBox c in getCheckBoxes()) c.Checked = true;
        }

        private void Quit_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Setup Wizard ended.");
            Dispose();
        }



       


        /*
        Console.WriteLine("Name of the identity before impersonation: " + WindowsIdentity.GetCurrent().Name + ".");

        IntPtr token;
        bool succeded = LogonUser("UserB", "192.168.10.2", "PasswordB", LogonType.Network, LogonProvider.Default, out token);
        if (!succeded) {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        } 

        WindowsIdentity adminIdentity = new WindowsIdentity(@"BUILTIN\Administrators");


        WindowsImpersonationContext impersonationContext = adminIdentity.Impersonate();

        Console.WriteLine("Name of the identity after impersonation: " + WindowsIdentity.GetCurrent().Name + ".");
        
         
         //protected stuff
         
         
        Console.WriteLine(adminIdentity.ImpersonationLevel);
        impersonationContext.Undo();
        Console.Write("Name of the identity after performing an Undo on the impersonation: " + WindowsIdentity.GetCurrent().Name);
        */


        /*
        [PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
         */




        /*

        public enum LogonType : int
        {
            Interactive = 2, Network = 3, Batch = 4, Service = 5, Unlock = 7, NetworkCleartText = 8, NewCredentials = 9,
        }

        public enum LogonProvider : int
        {
            Default = 0,
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool LogonUser(
            string principal,
            string authority,
            string password,
            LogonType logonType,
            LogonProvider logonProvider,
            out IntPtr token); 




        public class Impersonation : IDisposable { 
    
            #region Dll Imports    

            [DllImport("kernel32.dll")]    
            private static extern Boolean CloseHandle(IntPtr hObject);
   
            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]  
            private static extern bool LogonUser(string username, string domain,     
                string password, LogonType logonType,                
                LogonProvider logonProvider,                               
                out IntPtr userToken);     
            
            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]  
            private static extern bool DuplicateToken(IntPtr token, int impersonationLevel,  
                ref IntPtr duplication);       
            
            [DllImport("advapi32.dll", SetLastError = true)] 
            static extern bool ImpersonateLoggedOnUser(IntPtr userToken); 

            #endregion    
            

            #region Private members 
            private bool _disposed;   
            private WindowsImpersonationContext _impersonationContext;
            #endregion    
            
            #region Constructors     
            public Impersonation(String username, String domain, String password)   
            {     
                IntPtr userToken = IntPtr.Zero;  
                IntPtr userTokenDuplication = IntPtr.Zero;      
                // Logon with user and get token.    
                bool loggedOn = LogonUser(username, domain, password, 
                    LogonType.Interactive, LogonProvider.Default,       
                    out userToken);   
                if (loggedOn)       
                {          
                    try       
                    {           
                        // Create a duplication of the usertoken, this is a solution      
                        // for the known bug that is published under KB article Q319615.      
                        if (DuplicateToken(userToken, 2, ref userTokenDuplication))  
                        {              
                            // Create windows identity from the token and impersonate the user.       
                            WindowsIdentity identity = new WindowsIdentity(userTokenDuplication);  
                            _impersonationContext = identity.Impersonate();   
                        }            
                        else        
                        {               
                            // Token duplication failed! 
                            // Use the default ctor overload   
                            // that will use Mashal.GetLastWin32Error();    
                            // to create the exceptions details.               
                            throw new Exception("Could not copy token");      
                        }       
                    }          
                    finally        
                    {            
                        // Close usertoken handle duplication when created.   
                        if (!userTokenDuplication.Equals(IntPtr.Zero))      
                        {                   
                            // Closes the handle of the user.  
                            CloseHandle(userTokenDuplication);           
                            userTokenDuplication = IntPtr.Zero;        
                        }             
                        // Close usertoken handle when created.     
                        if (!userToken.Equals(IntPtr.Zero))        
                        {                 
                            // Closes the handle of the user.   
                            CloseHandle(userToken);       
                            userToken = IntPtr.Zero;      
                        }     
                    }     
                }   
                else   
                {              
                    throw new Exception("Login failed");    
                }   
            }   
            ~Impersonation()   
            { 
                Dispose(false);  
            }   
            #endregion  
            
            #region Public methods   
            public void Revert() 
            {     
                if (_impersonationContext != null) 
                {        
                    // Revert to previous user.   
                    _impersonationContext.Undo(); 
                    _impersonationContext = null;  
                }   
            }    
            
            #endregion  
            
            #region IDisposable implementation.  
            public void Dispose()   
            {   
                Dispose(true);  
                GC.SuppressFinalize(this);  
            }    
            
            protected virtual void Dispose(bool disposing) 
            {     
                if (!_disposed)      
                {         
                    Revert();  
                    _disposed = true;   
                } 
            }   
            #endregion
        } 



        */








    }
}
