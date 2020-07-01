using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Reflection;
using System.Text;
using Microsoft.VisualBasic;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Diagnostics;
using System.Security.Principal;
using System.Security.Permissions;
using System.ComponentModel;

namespace AuditSec
{
    static public class AuditSec
    {
        public static string defaultOUMask = "^[A-Z]{3}$|^Orphaned Workstations$";
        public static string defaultWkMask = "^[A-Z]{3}[0-9]{6}$";
        public static string defaultAdmins = "MYCOMPANY\\, \\ishelpdesk, \\temp_admin, useradmin";
        public static string defaultLdap = "LDAP://servername:389/ou=USER,o=MYCOMPANY";

        public static string usagelogs = @"\\servername.MYCOMPANY.com\wapps\WIN7\AuditSec\usagelogs";

        static StreamWriter stdout, stderr;
        static StreamWriter fs;
        static StringBuilder sbout = new StringBuilder();
        static bool DO_LOGFILE = System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed;

        public static string APPDATA = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MYCOMPANY");

        public static string guid = "?";
        public static string curusr = "?";
        public static string curver = "?";



        [STAThread]
        static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "IT_Ops_Tools_Suite-Main-Thread";
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> " + Thread.CurrentThread.Name + "\n");

            appStart();
            if (!SetupWizard.switchArgs(args))
                Application.Run(new SetupWizard());
        }

        public static void appStart() {
            DO_LOGFILE = true;
            if (DO_LOGFILE) try
            {
                StringWriter sw = new StringWriter();
                Console.SetOut(sw);
                Console.SetError(sw);
                stdout = new StreamWriter(Console.OpenStandardOutput());
                stdout.AutoFlush = true;
                stderr = new StreamWriter(Console.OpenStandardError());
                stderr.AutoFlush = true;

                if (!Directory.Exists(APPDATA)) Directory.CreateDirectory(APPDATA);

                //string sourceFilePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "MyFile.txt");
                string destFilePath = Path.Combine(APPDATA, "auditsec-out.txt");
                //MessageBox.Show("DEBUG enabled.\n\nThe log file is here: " + destFilePath, "AuditSec");

                fs = new StreamWriter(destFilePath, false);
                Thread t = new Thread(new ThreadStart(delegate
                {
                    while (true) try
                        {
                            Thread.Sleep(2000);
                            string s = sw.ToString();
                            if (s.Length > 0)
                            {
                                sw.GetStringBuilder().Clear();
                                string now = String.Format(new CultureInfo("en-US"), "[{0:yyyy-MMM-dd hh:mm:ss}]", DateTime.Now);
                                stdout.WriteLine(now + "\r\n" + s);
                                sbout.AppendLine(now + "\r\n" + s);
                                fs.Write(now + "\r\n" + s);
                                fs.Flush();
                            }
                        }
                        catch (Exception e)
                        {
                            if (e.ToString().Contains("Parameter name: chunkLength"))
                                Console.WriteLine("...Console logging delayed...");
                            else
                                Console.WriteLine("Console logging failure: " + e.ToString());
                        }
                })); t.IsBackground = true; t.Start();
            }
            catch (Exception e)
            {
                DO_LOGFILE = false;
                Console.WriteLine("Error: cannot open Logfile: " + e.Message);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            guid = getGUID().ToString();
            curusr = UserPrincipal.Current.SamAccountName;
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                curver = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            else
            {
                curver = Assembly.GetEntryAssembly().GetName().Version.ToString();
                //curver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                //curver = Assembly.GetCallingAssembly().GetName().Version.ToString();
            }
            if (curver == null || curver.Length == 0) curver = "?";

            Console.WriteLine("Application is starting...");
            Console.WriteLine("GUID: " + guid);
            Console.WriteLine("Username: " + curusr);
            Console.WriteLine("Current appli version: " + curver);

            DateTime startdate = DateTime.UtcNow;
            string usagelogsfile = Path.Combine(usagelogs, guid + "-" + String.Format(new CultureInfo("en-US"), "{0:yyMMdd-hhmmss}", startdate) + ".log");
            
            try{
                File.WriteAllText(usagelogsfile, Environment.MachineName + "\t" + Environment.OSVersion +"\t" + (Environment.Is64BitOperatingSystem ? "x64" : "x32")
                    + "\t" + curusr + "\t" + String.Format(new CultureInfo("en-US"), "{0:yyyy-MMM-dd hh:mm:ss}", startdate) + "\tv" + curver);
            } catch(Exception e)
            {
                Console.WriteLine(usagelogsfile + ": " + e.Message);
            }

            if (Environment.Is64BitOperatingSystem)
            {
                MessageBox.Show("Welcome to IT Ops Tools Suite !\n\n"
                            + "You are running on a 64 bits operating system.\n\n"
                            + "This program is currenly not tested for this platform.",
                            "IT O p s  T o o l s  S u i t e");
            }
        }





        public static void Exit(string message, Func<bool> dispose)
        {
            Console.WriteLine("Application is finishing...");
            if (SetupWizard.wizardInstance == null)
            {
                if (fs != null) fs.Flush();
                if (DO_LOGFILE)
                {
                    Console.SetOut(stdout);
                    Console.SetOut(stderr);
                }
                textOutputBox("Thanks!", "Thanks for having used this application. Press OK to Quit the application",
                    (message != null ? message : "") + "\r\n\r\n" + sbout.ToString());
            }
            if (dispose != null) dispose();
            Console.WriteLine("Application finished.");
            if (SetupWizard.wizardInstance == null)
                Application.Exit();
        }


        public static myWPF.LyncControl newLyncControl(Form form, string sip, Control parent)
        {
            try
            {
                myWPF.LyncControl lync;
                if (SetupWizard.wizardInstance == null)
                    lync = newLyncControl_(sip, parent);
                else lync = (myWPF.LyncControl)form.Invoke(new newLyncControlDelegate(newLyncControl_), sip, parent);
                return lync;
            }
            catch (Exception e)
            {
                Console.WriteLine("WPF Instanciation error:\n" + e.ToString());
                return null;
            }

        }
        delegate myWPF.LyncControl newLyncControlDelegate(string sip, Control parent);
        static myWPF.LyncControl newLyncControl_(string sip, Control parent)
        {
            myWPF.LyncControl lync = new myWPF.LyncControl();
            lync.setRemoteSIP(sip);
            System.Windows.Forms.Integration.ElementHost lyncHost = new System.Windows.Forms.Integration.ElementHost();
            lyncHost.Dock = DockStyle.Fill;
            lyncHost.BackColor = SystemColors.Control;
            lyncHost.Size = new System.Drawing.Size(30, 50);
            lyncHost.Child = lync;
            lyncHost.Enabled = true;
            lyncHost.Visible = false;
            parent.Controls.Add(lyncHost);
            lyncHost.Visible = true;
            return lync;
        }


        public static void ShowLogs()
        {
            textOutputBox("Debug Logs", "Console logging (Program output):",
                "\r\n\r\n" + sbout.ToString());
        }


        public static bool exportResource(string res, string dest, bool force)
        {
            if (!File.Exists(dest) || force)
            {
                /*if (!isAdministrator())
                {
                    string error = "You need to be an administrator for this module to work properly.";
                    MessageBox.Show("Could not export resource: " + dest + "\n" + error);
                    return false;
                }*/
                try
                {
                    string error = exportResource_(res, dest, force);
                    if (error != null)
                    {
                        MessageBox.Show("Could not export resource: " + dest + "\n" + error);
                        return false;
                    }
                    else return true;
                }
                catch (Exception e)
                {
                    string error = e.Message;
                    MessageBox.Show("Could not export resource: " + dest + "\n" + error);
                    return false;
                }
            }
            else
            {
                //Console.WriteLine("Resource already present: " + dest);
                return true;
            }
        }

        public static string exportResource_(string res, string dest, bool force)
        {
            try
            {
                System.Resources.ResourceManager rm = new System.Resources.ResourceManager(
                    "AuditSec.Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
                object obj = rm.GetObject(res);
                
                byte[] source = new byte[0] { };
                if (obj is byte[]) source = (byte[])(obj);
                else if (obj is string) source = System.Text.Encoding.UTF8.GetBytes((string)obj);
                else if (obj is Icon) using (MemoryStream ms = new MemoryStream())
                    {
                        ((Icon)obj).Save(ms);
                        source = ms.ToArray();
                    }
                else return "Resource type unknown: " + obj.GetType().Name;

                string tmp = MachineInfo.GetTempFilePathWithExtension(".tmp");
                string cmd = MachineInfo.GetTempFilePathWithExtension(".cmd");

                int size = source.Length;
                BinaryWriter bw = new BinaryWriter(File.OpenWrite(tmp));
                bw.Write(source); bw.Flush(); bw.Close();

                bool directCopy = false;
                try
                {
                    if (File.Exists(dest)) File.Move(dest, tmp + ".swap");
                    File.Move(tmp, dest);
                    directCopy = true;
                }
                catch(Exception ee)
                {
                    Console.WriteLine("directCopy error: " + ee.ToString());
                }

                if (!directCopy)
                {
                    string copy = "@copy \"" + tmp + "\" \"" + dest + "\"";
                    File.WriteAllText(cmd, copy);

                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "cmd.exe";
                    psi.Arguments = "/c \"" + cmd + "\""; // /c
                    psi.Verb = "runas";

                    psi.WindowStyle = ProcessWindowStyle.Minimized;
                    //Console.WriteLine(psi.FileName + " " + psi.Arguments);
                    //Console.WriteLine("\"" + cmd + "\"\n=" + copy);

                    Process copyProcess = Process.Start(psi);
                    while (!copyProcess.HasExited) Thread.Sleep(1000);
                }


                if (File.Exists(dest) && (size > 0))
                {
                    Console.WriteLine("Exported successfully resource: " + dest + " (" + size + "bytes)");
                    File.Delete(tmp);
                    File.Delete(cmd);
                    return null;
                }
                else
                {
                    try {
                        if (File.Exists(dest)) File.Delete(dest);
                    }
                    catch (Exception ee) { }
                    return "Copy failure. " + " (0/" + size + "bytes)";
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return e.Message;
            }          
        }
        
        static Guid getGUID()
        {
            try
            {
                RegistryKey HKLM = RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, "");
                Guid GUID = Guid.Parse((string)HKLM.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography").GetValue("MachineGuid"));
                return GUID;
            }
            catch (Exception e)
            {
                return Guid.Empty;
            }
        }

        static DES des = getDES();
        static DES getDES()
        {
            try
            {
                Guid GUID = getGUID();
                if (GUID.Equals(Guid.Empty)) throw new Exception("GUID unknown");

                byte[] GUID0 = GUID.ToByteArray(), GUID1 = new byte[8], GUID2 = new byte[8];
                for (int i = 0; i < 8; i++) GUID1[i] = GUID0[i];
                for (int i = 8; i < 16; i++) GUID2[i - 8] = GUID0[i];
                DES des = DES.Create(); des.Key = GUID1; des.IV = GUID2;
                return des;
            }
            catch (Exception e)
            {
                Console.WriteLine("Initialization error: " + e.ToString());
                return null;
            }
        }

        static void encryptSetting(ref string setting, string label)
        {
            if (setting == null)
                setting = "";
            if (setting.Length > 0 && des != null) try
                {
                    byte[] dec = (new UTF8Encoding()).GetBytes(setting);
                    //Console.WriteLine("DEC: " + BitConverter.ToString(dec).Replace("-", ":"));
                    MemoryStream ms = new MemoryStream();
                    CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                    cs.Write(dec, 0, dec.Length); cs.Close(); byte[] enc = ms.ToArray(); ms.Close();
                    //Console.WriteLine("ENC: " + BitConverter.ToString(enc).Replace("-", ":"));
                    setting = Convert.ToBase64String(enc);
                }
                catch (Exception e)
                {
                    setting = "";
                    Console.WriteLine("Error encrypting " + label + ": " + e.Message);
                }
            else setting = "";
        }

        static void decryptSetting(ref string setting, string label)
        {
            if (setting == null)
                setting = "";
            else if (setting.Length > 0 && des != null) try
                {
                    byte[] enc = Convert.FromBase64String(setting);
                    //Console.WriteLine("ENC: " + BitConverter.ToString(enc).Replace("-",":"));
                    MemoryStream ms = new MemoryStream(enc);
                    CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
                    byte[] dec = new Byte[enc.Length]; cs.Read(dec, 0, enc.Length); cs.Close(); ms.Close();
                    //Console.WriteLine("DEC: " + BitConverter.ToString(enc).Replace("-", ":"));
                    setting = (new UTF8Encoding()).GetString(dec);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error decrypting " + label + ": " + e.Message);
                }
        }


        public static Settings settings = new Settings();
        public class Settings
        {
            public string appver = "";
            public string stdpw = "";
            public string pws = "";
            public string desap = "";
            public string oumask = "";
            public string wkmask = "";
            public string admins = "";
            public bool dowin7 = true;
            public bool dowin7sp = true;
            public bool doadmpw = true;
            public bool dononauth = true;
            public bool domisplaced = true;
            public bool dobitlocker = true;
            public bool dosccm = false;
            public bool dovirus = true;
            public string picpw = "";
            public string smssrv = "";
            public string smssite = "";
            public string smspub = "";
            public string smstime = "";
        }


        public static void saveSettings(string stdpw, string pws, string desap, string oumask, string wkmask, string admins,
            bool dowin7, bool dowin7sp, bool doadmpw, bool dononauth , bool domisplaced, bool dobitlocker, bool dosccm, bool dovirus)
        {
            Console.WriteLine("Saving settings...");

            settings.appver = curver;
            settings.pws = pws;
            settings.desap = desap;
            settings.oumask = oumask;
            settings.wkmask = wkmask;
            settings.admins = admins;
            settings.dowin7 = dowin7;
            settings.dowin7sp = dowin7sp;
            settings.doadmpw = doadmpw;
            settings.dononauth = dononauth;
            settings.domisplaced = domisplaced;
            settings.dobitlocker = dobitlocker;
            settings.dosccm = dosccm;
            settings.dovirus = dovirus;
            settings.smssrv = settings.smssrv != null ? settings.smssrv : "";
            settings.smssite = settings.smssite != null ? settings.smssite : "";
            /*string stdpw = settings.stdpw;*/ encryptSetting(ref stdpw, "stdpw"); settings.stdpw = stdpw;
            string picpw = AuditSec.settings.picpw; encryptSetting(ref picpw, "picpw"); settings.picpw = picpw;

            //Console.WriteLine("encrypted stdpw = \"" + settings.stdpw + "\"");
            //Console.WriteLine("encrypted picpw = \"" + settings.picpw + "\"");
            try
            {
                SaviorClass.Savior.Save(settings, @"Software\MYCOMPANY\AuditSec");
            }
            catch (Exception ee)
            {
                MessageBox.Show("Error while saving user's preferences:\n" + ee.ToString(), "Saving settings...");
            }
        }


        public static void saveSettings_PCMGMT(/*string pws*/)
        {

            Console.WriteLine("Saving settings...");

            settings.appver = curver;
            /*settings.pws = pws;*/
            string stdpw = settings.stdpw; encryptSetting(ref stdpw, "stdpw"); settings.stdpw = stdpw;
            string picpw = AuditSec.settings.picpw; encryptSetting(ref picpw, "picpw"); settings.picpw = picpw;

            //Console.WriteLine("encrypted stdpw = \"" + settings.stdpw + "\"");
            //Console.WriteLine("encrypted picpw = \"" + settings.picpw + "\"");
            try
            {
                SaviorClass.Savior.Save(settings, @"Software\MYCOMPANY\AuditSec");
            }
            catch (Exception ee)
            {
                MessageBox.Show("Error while saving user's preferences:\n" + ee.ToString(), "Saving settings...");
            }
        }


        public static void saveSettings_SMSDIRXML(string smspub, string smstime)
        {

            Console.WriteLine("Saving settings...");

            settings.appver = curver;
            settings.smssrv = AuditSec.settings.smssrv != null ? AuditSec.settings.smssrv : "";
            settings.smssite = AuditSec.settings.smssite != null ? AuditSec.settings.smssite : "";
            settings.smspub = smspub;
            settings.smstime = smstime;
            string stdpw = settings.stdpw; encryptSetting(ref stdpw, "stdpw"); settings.stdpw = stdpw;
            string picpw = AuditSec.settings.picpw; encryptSetting(ref picpw, "picpw"); settings.picpw = picpw;

            //Console.WriteLine("encrypted stdpw = \"" + settings.stdpw + "\"");
            //Console.WriteLine("encrypted picpw = \"" + settings.picpw + "\"");
            try
            {
                SaviorClass.Savior.Save(settings, @"Software\MYCOMPANY\AuditSec");
            }
            catch (Exception ee)
            {
                MessageBox.Show("Error while saving user's preferences:\n" + ee.ToString(), "Saving settings...");
            }
        }

        public static void saveSettings_SMS()
        {

            Console.WriteLine("Saving settings...");

            settings.appver = curver;
            settings.smssrv = AuditSec.settings.smssrv != null ? AuditSec.settings.smssrv : "";
            settings.smssite = AuditSec.settings.smssite != null ? AuditSec.settings.smssite : "";
            try
            {
                SaviorClass.Savior.Save(settings, @"Software\MYCOMPANY\AuditSec");
            }
            catch (Exception ee)
            {
                MessageBox.Show("Error while saving user's preferences:\n" + ee.ToString(), "Saving settings...");
            }
        }



        static bool readSettingsDone = false;
        public static string readSettings()
        {
            if (readSettingsDone) return curver;

            Console.WriteLine("Reading user settings...");
            try
            {
                SaviorClass.Savior.Read(settings, @"Software\MYCOMPANY\AuditSec");
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot read settings: " + e.Message);
            }

            string savver = settings.appver;
            if (savver == null || savver.Length == 0) savver = "?";
            Console.WriteLine("Saved setting version: " + savver);

            bool updated = true;
            bool majmineq = false;
            try
            {
                if (!curver.Equals("?") && !savver.Equals("?"))
                {
                    Version cur = new Version(curver);
                    Version sav = new Version(savver);
                    updated = cur.CompareTo(sav) > 0;
                    majmineq = cur.Major == sav.Major && cur.Minor == sav.Minor;
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("Version mismatch: " + e.Message);
            }
            if (updated)
            {
                if (majmineq)
                {
                    MessageBox.Show("The application was updated from version '" + savver + "' to version '" + curver + "'"
                        + "\n\n" + UpdateNews.news,
                        "AuditSec Update");
                }
                else
                {
                    settings = new Settings();
                    MessageBox.Show("The application was upgraded from version '" + savver + "' to version '" + curver + "'\n"
                        + " \nPlease set again your preferred settings."
                        + "\n\n" + UpdateNews.news,
                        "AuditSec Upgrade");
                }
            }

            decryptSetting(ref settings.stdpw, "stdpw");
            decryptSetting(ref settings.picpw, "picpw");

            readSettingsDone = true;
            return curver;
        }


        static bool smsinit = false;
        public static void InitializeSMS(bool force, string SMSSERVER, string SMSSITE)
        {
            if (smsinit && !force) return;
            smsinit = true;
            if (AuditSec.settings.smssrv == null || AuditSec.settings.smssrv.Length == 0
                || AuditSec.settings.smssrv != SMSSERVER)
            {
                AuditSec.settings.smssrv = SMSSERVER;
                smsinit = false;
            }
            if (AuditSec.settings.smssite == null || AuditSec.settings.smssite.Length == 0
                || AuditSec.settings.smssite != SMSSITE)
            {
                AuditSec.settings.smssite = SMSSITE;
                smsinit = false;
            }
            if (smsinit) return;
            bool cancel = false;
            string ret;
            if (!force || AuditSec.settings.smssrv == null || AuditSec.settings.smssrv.Length == 0)
            {
                ret = Interaction.InputBox("SCCM Server:\n\t(You may cancel to disable the SCCM feature)",
                    "Specify SCCM connection profile", AuditSec.settings.smssrv);
                if (ret != null && ret.Length > 0) AuditSec.settings.smssrv = ret;
                else cancel = true;
            }
            if (!cancel)
            {
                if (!force || AuditSec.settings.smssite == null || AuditSec.settings.smssite.Length == 0)
                {
                    ret = Interaction.InputBox("Site Name:", "Specify SCCM connection profile", AuditSec.settings.smssite);
                    if (ret != null && ret.Length > 0) AuditSec.settings.smssite = ret;
                    smsinit = true;
                }
            }
        }


        static public bool picdisabled = false;
        static string DIRXMLAccessShowing = "";

        static public void checkDIRXMLAccess(string lasterror)
        {
            //Console.WriteLine("Checking DIRXML access...");
            if (AuditSec.settings.picpw == null && !picdisabled)
            {
                while (DIRXMLAccessShowing.Length > 0) Thread.Sleep(1000);
                lock (DIRXMLAccessShowing)
                {
                    DIRXMLAccessShowing = "DIRXMLDialogShowing";
                    if (DialogResult.OK != pwInputBox("DirXML Authentication",
                        "Employee data retrieval requires authentication against DirXML"
                        + "\n\nUsername: " + UserPrincipal.Current.SamAccountName + (lasterror == null ? "" : " - " + lasterror)
                        + "\n\nPassword: (you may cancel to disable this feature)"
                        ,
                        ref AuditSec.settings.picpw) || AuditSec.settings.picpw.Length == 0)
                    {
                        AuditSec.settings.picpw = null;
                        picdisabled = true;
                    }
                    DIRXMLAccessShowing = "";
                }
            }
        }


        public static DialogResult pwInputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;
            textBox.PasswordChar = '●';

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(10, 20, 380, 75);
            textBox.SetBounds(50, 100, 300, 15);
            buttonOk.SetBounds(228, 170, 75, 23);
            buttonCancel.SetBounds(309, 170, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(400, 200);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        public static void textOutputBox(string title, string message, string text)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            textBox.ReadOnly = true;
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.Both;
            Button buttonOk = new Button();


            System.Windows.Forms.LinkLabel link = new System.Windows.Forms.LinkLabel() {            
                TabIndex = 10,
                TabStop = true,
                Text = "Contact the Author"
            };
            link.LinkClicked += (sender, e) => { System.Diagnostics.Process.Start("mailto:vincent.fontaine@MYCOMPANY.com"); };


            form.Text = title;
            label.Text = message;
            textBox.Text = text;
            textBox.ScrollToCaret();

            buttonOk.Text = "OK";
            buttonOk.DialogResult = DialogResult.OK;

            link.SetBounds(450, 10, 150, 13);
            label.SetBounds(10, 20, 580, 75);
            textBox.SetBounds(50, 50, 500, 300);
            buttonOk.SetBounds(500, 370, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(600, 400);
            form.Controls.AddRange(new Control[] { label, link, textBox, buttonOk});
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;

            DialogResult dialogResult = form.ShowDialog();
        }





        static System.Speech.Synthesis.SpeechSynthesizer TTS_en = new System.Speech.Synthesis.SpeechSynthesizer();

        static System.Speech.Synthesis.SpeechSynthesizer TTS = TTS_en;

        static System.Speech.Synthesis.SpeechSynthesizer TTS_fr = new System.Speech.Synthesis.SpeechSynthesizer();
        static bool TTSfr_ok = new Func<bool>(() =>
        {
            try { TTS_fr.SelectVoice("Hortense_Dri40_16kHz"); }
            catch (Exception e) { return false; }
            return true;
        })();

        static System.Speech.Synthesis.SpeechSynthesizer TTS_local = new Func<System.Speech.Synthesis.SpeechSynthesizer>(() =>
        {
            string co = System.Globalization.RegionInfo.CurrentRegion.DisplayName.ToUpper();
            if (co == "FRANCE" && TTSfr_ok)
            {
                TTS_fr.Rate = 200;
                //TTSfr.SpeakAsync("Bonjour");
                Console.WriteLine("TTS: Localized voice: French.");
                return TTS_fr;
            }
            //TTSen.SpeakAsync("Hello");
            Console.WriteLine("TTS: No localized voice.");
            return TTS_en;
        })();




        public static void Speak(string text, bool cancel, bool local, Func<string, string> TTSReplace)
        {
            TTS.Rate = -5;
            Console.WriteLine("\tTTS: " + text);
            if (cancel) TTS.SpeakAsyncCancelAll();
            if (text.Trim().Length > 0)
            {
                TTS.SpeakAsync(TTSReplace != null ? TTSReplace(text) : text);
            }
        }

        public static void SpeakWait(string text, bool cancel, bool local, Func<string, string> TTSReplace)
        {
            Console.WriteLine("\tTTS: " + text);
            if (cancel) TTS.SpeakAsyncCancelAll();
            if (text.Trim().Length > 0)
            {
                try
                {
                    TTS.Speak(TTSReplace != null ? TTSReplace(text) : text);
                }
                catch (Exception e)
                {
                    Console.WriteLine("\tTTS: " + text + ". Discard: " + e.Message);
                }
            }
        }


        public static string TTSReplace(string text)
        {
            return text
                .Replace("_", " ")
                .Replace("#", " number ")
                ;
        }


        public static void InjectAlertBlocker(WebBrowser browser)
        {
            string alertBlocker = "window.alert = function () { }";

            /*
            //solution 1: use mshtml. buggy registration assembly. forget.
            HtmlElement head = browser.Document.GetElementsByTagName("head")[0];
            HtmlElement scriptEl = browser.Document.CreateElement("script");
            mshtml.IHTMLScriptElement element = (mshtml.IHTMLScriptElement)scriptEl.DomElement;
            string alertBlocker = "window.alert = function () { }";
            element.text = alertBlocker;
            head.AppendChild(scriptEl);
            */

            //solution 2:
            browser.Document.InvokeScript("eval", new object[] { alertBlocker });

            //solution 3:
            //browser.Document.InvokeScript("execScript", new Object[] { alertBlocker, "JavaScript" });

            /*
            //solution 4:
            HtmlElement head = browser.Document.GetElementsByTagName("head")[0];
            HtmlElement scriptEl = browser.Document.CreateElement("script");
            scriptEl.SetAttribute("text", "function InjectAlertBlocker() { " + alertBlocker + " }");
            head.AppendChild(scriptEl);
            browser.Document.InvokeScript("InjectAlertBlocker");
            */
        }


    }
}