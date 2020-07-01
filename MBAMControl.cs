using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.Text.RegularExpressions;

using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace AuditSec
{
    public partial class MBAMControl : UserControl
        //, ComExt.IServiceProvider, ComExt.IAuthenticate
    {
        /*
        uint _authenticateServiceCookie = ComExt.INVALID;
        ComExt.IProfferService _profferService = null;

        CancellationTokenSource _navigationCts = null;
        Task _navigationTask = null;
        */

        public MBAMControl()
        {
            InitializeComponent();
            KeyBox.Text = "";
            /*
            // set up IAuthenticate as service via IProfferService
            var ax = browser.ActiveXInstance;
            var serviceProvider = (ComExt.IServiceProvider)ax;
            serviceProvider.QueryService(out _profferService);
            _profferService.ProfferService(typeof(ComExt.IAuthenticate).GUID, this, ref _authenticateServiceCookie);
            */
            KeyID = IDBox.Text;
            timer1.Start();
            //ComputerManagement.Speak("Bonjour", true);
        }

        string KeyRecoveryPage = "https://servername/helpdesk/KeyRecoveryPage.aspx";
        string BlankPage = "about:blank";
        string[] reasons = new string[]{
            "Operating System Boot Order changed",
            "BIOS changed",
            "Operating System files modified",
            "Lost Startup Key",
            "Lost PIN",
            "TPM Reset",
            "Lost Passphrase",
            "Lost Smartcard",
            "Other"};

        void search(string x)
        {
            IDBox.Text = x;
            timer_i = timer_max;
        }


        static string me = WindowsIdentity.GetCurrent().Name;
        string user = me;
        public void resetUser()
        {
            user = me;
        }
        public void setUser(string user)
        {
            Console.WriteLine("MBAM: Selected User: " + user);
            this.user = user;
        }
        public void setUser()
        {
            user = Interaction.InputBox("Domain/User: ", "Bitlocker Drive Recovery", me).Trim();
            if (user.Length == 0) user = me;
            Console.WriteLine("MBAM: Selected User: " + user);
        }


        bool submit = false;

        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                if (e.Url.ToString().ToLower().Equals(BlankPage.ToLower()))
                {
                    Console.WriteLine("\n\n\n\nLoading..." + KeyID);

                    //browser.Url = new Uri(KeyRecoveryPage);

                    string additionalHeaderInfo = "Authorization: Basic "
                        + Convert.ToBase64String(Encoding.ASCII.GetBytes("MyUsername" + ":" + "MyPassword"))
                        + System.Environment.NewLine;
                    browser.Navigate(new Uri(KeyRecoveryPage), null, null, additionalHeaderInfo);

                    submit = false;
                }
                else if (!submit)
                {
                    Console.WriteLine("\n\n\n\nSubmitting..." + KeyID);
                    //printHtml();
                    resetUser();
                    setElement("ctl00_content_DomainNameTextBox", user.Split(new char[] { '\\' })[0]);
                    setElement("ctl00_content_UserNameTextBox", user.Split(new char[] { '\\' })[1]);
                    setElement("ctl00_content_KeyIdTextBox", KeyID);
                    setElement("ctl00_content_ReasonCodeSelect", reasons[8]);
                    clickElement("ctl00_content_SubmitButton");
                    submit = true;
                }
                else
                {
                    //printHtml();
                    string RecoveryKey = getElement("ctl00_content_KeyReturnFieldInvisible");
                    Console.WriteLine("\n\n\n\nLoaded. " + RecoveryKey);
                    KeyBox.Text = RecoveryKey == null ? "" : RecoveryKey;
                    if (KeyBox.Text.Length > 0)
                        speakBitlocker();
                    else
                    {
                        AuditSec.Speak("Recovery Key not found.", true, false, null);
                        MessageBox.Show("Recovery Key not found.",
                            "Bitlocker Drive Recovery");
                    }

                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("Error after the document completed: " + ee.ToString());
            }
        }



        public static string spellBitLocker(string word)
        {
            try
            {
                StringBuilder spell = new StringBuilder();
                for (int g = 0; g < 8; g++)
                {
                    if (g == 0)
                        ;
                    else if (g < 7)
                        spell.Append("then it is: ");
                    else if (g == 7)
                        spell.Append("and finally it is: ");
                    for (int i = 0; i < 6; i++)
                    {
                        spell.Append(word[g * 7 + i] + ", ");
                    }
                }
                return spell.ToString().Replace("-", "\nthen it is: ");
            }
            catch (Exception e)
            {
                return word;
            }
        }


        void speakBitlocker()
        {
            if (KeyBox.Text.Length > 0)
                AuditSec.Speak("The recovery key is:\n"
                + (spellBitLocker(KeyBox.Text) + ".").Replace(", .", "."), true, false, null/*AuditSec.TTSReplace*/);
        }

        bool setElement(string id, string text)
        {
            try
            {
                browser.Document.GetElementById(id).SetAttribute("Value", text);
                return true;
            }
            catch (Exception e)
            {
                //Console.WriteLine("Element '" + id + "' not set to '" + text + "': " + e.Message);
                return false;
            }
        }

        string getElement(string id)
        {
            try
            {
                return browser.Document.GetElementById(id).GetAttribute("Value").ToString();
            }
            catch (Exception e)
            {
                //Console.WriteLine("Element '" + id + "' not set to '" + text + "': " + e.Message);
                return null;
            }
        }

        bool clickElement(string id)
        {
            try
            {
                //printElement(browser.Document.GetElementById(id));
                browser.Document.GetElementById(id).RaiseEvent("onclick");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Element '" + id + "' not clicked: " + e.Message);
                return false;
            }
        }

        void printElement(HtmlElement el)
        {
            /*
             * if (
                //||el.TagName.ToUpper().Equals("TD")
                el.TagName.ToUpper().Equals("INPUT")
                || el.TagName.ToUpper().Equals("BUTTON")
                )
            */
            Console.WriteLine(el.TagName + ":" + el.Name + "/" + el.Id + "=" + el.InnerText /*+ "=" +el.OuterText*/);
            foreach (HtmlElement child in el.Children)
                printElement(child);
        }

        void printHtml()
        {
            HtmlElement html = browser.Document.GetElementsByTagName("html")[0];
            printElement(html);
        }




        int timer_i = 0;
        int timer_max = 10;
        string KeyID = "";
        //string volumeid = "";

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (IDBox.Text.Length == 0 || IDBox.Text.ToUpper().Equals(KeyID))
            {
                timer_i = 0;
            }
            else
            {
                KeyBox.Text = "";
                timer_i++;
                if (timer_i > timer_max)
                {
                    KeyID = IDBox.Text.ToUpper();
                    timer_i = 0;
                    /*
                    volumeid = "";
                    string machine = timer_last.ToUpper();
                    MachineInfo mi = MachineInfo.getMachine(machine);
                    if (mi == null)
                    {
                        MessageBox.Show("Machine not found: " + machine, "Bitlocker Drive Recovery");
                    }
                    else
                    {
                        Console.WriteLine(mi.ToString());
                        volumeid = mi.volumeid.Substring(0, 8);
                    }
                    */


                    if (Regex.IsMatch(KeyID, @"^[0-9A-F]{8}$"))
                    {
                        Console.WriteLine("Search Key ID: " + KeyID);
                        setUser();
                        browser.Url = new Uri(BlankPage);
                    }
                    else
                    {
                        Console.WriteLine("Invalid Key ID: " + KeyID);
                        IDBox.Text = KeyID.Trim();
                        //ComputerManagement.Speak("Please enter exactly the first eight hexa digits of the Recovery Key ID.", true);
                        //MessageBox.Show("Please enter exactly the first eight hexa digits of the Recovery Key ID.",
                        //    "Bitlocker Drive Recovery");
                    }
                }
            }
        }



        private void IDBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            timer_i = 0;
        }

        private void KeyBox_Click(object sender, EventArgs e)
        {
            speakBitlocker();
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            if (KeyBox.Text != null && KeyBox.Text.Length > 0)
                Clipboard.SetText(KeyBox.Text);
        }

        public void Clear()
        {
            AuditSec.Speak("", true, false, null);
            IDBox.Text = "";
            KeyBox.Text = "";
            resetUser();
        }

        bool retry = false;

        private void ADTreeRefresh_Click(object sender, EventArgs e)
        {
            if (KeyID == null || KeyID.Length == 0) return;
            Console.WriteLine("Search Key ID: " + KeyID);
            setUser();
            browser.Url = new Uri(BlankPage);
        }

        private void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            AuditSec.InjectAlertBlocker(browser);
            //Console.WriteLine("Browser: navigated. " + e.Url);
        }

        private void clearusrButton_Click(object sender, EventArgs e)
        {
            IDBox.Text = KeyBox.Text = "";
        }


    }


}


    /*




    #region ComExt.IServiceProvider
    public int QueryService(ref Guid guidService, ref Guid riid, ref IntPtr ppvObject)
    {
        if (guidService == typeof(ComExt.IAuthenticate).GUID)
        {
            return this.QueryInterface(ref riid, ref ppvObject);
        }
        return ComExt.E_NOINTERFACE;
    }
    #endregion

    #region ComExt.IAuthenticate
    public int Authenticate(ref IntPtr phwnd, ref string pszUsername, ref string pszPassword)
    {
        phwnd = IntPtr.Zero;
        pszUsername = String.Empty;
        pszPassword = String.Empty;
        return ComExt.S_OK;
    }
    #endregion

    */
    /*

    // COM interfaces and helpers
    public static class ComExt
    {
        public const int S_OK = 0;
        public const int E_NOINTERFACE = unchecked((int)0x80004002);
        public const int E_UNEXPECTED = unchecked((int)0x8000ffff);
        public const int E_POINTER = unchecked((int)0x80004003);
        public const uint INVALID = unchecked((uint)-1);

        static public void QueryService<T>(this IServiceProvider serviceProvider, out T service) where T : class
        {
            Type type = typeof(T);
            IntPtr unk = IntPtr.Zero;
            int result = serviceProvider.QueryService(type.GUID, type.GUID, ref unk);
            if (unk == IntPtr.Zero || result != S_OK)
                throw new COMException(
                    new StackFrame().GetMethod().Name,
                    result != S_OK ? result : E_UNEXPECTED);
            try
            {
                service = (T)Marshal.GetTypedObjectForIUnknown(unk, type);
            }
            finally
            {
                Marshal.Release(unk);
            }
        }

        static public int QueryInterface(this object provider, ref Guid riid, ref IntPtr ppvObject)
        {
            if (ppvObject != IntPtr.Zero)
                return E_POINTER;

            IntPtr unk = Marshal.GetIUnknownForObject(provider);
            try
            {
                return Marshal.QueryInterface(unk, ref riid, out ppvObject);
            }
            finally
            {
                Marshal.Release(unk);
            }
        }

        #region IServiceProvider Interface
        [ComImport()]
        [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IServiceProvider
        {
            [PreserveSig]
            int QueryService(
                [In] ref Guid guidService,
                [In] ref Guid riid,
                [In, Out] ref IntPtr ppvObject);
        }
        #endregion

        #region IProfferService Interface
        [ComImport()]
        [Guid("cb728b20-f786-11ce-92ad-00aa00a74cd0")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IProfferService
        {
            void ProfferService(ref Guid guidService, IServiceProvider psp, ref uint cookie);

            void RevokeService(uint cookie);
        }
        #endregion

        #region IAuthenticate Interface
        [ComImport()]
        [Guid("79eac9d0-baf9-11ce-8c82-00aa004ba90b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAuthenticate
        {
            [PreserveSig]
            int Authenticate([In, Out] ref IntPtr phwnd,
                [In, Out, MarshalAs(UnmanagedType.LPWStr)] ref string pszUsername,
                [In, Out, MarshalAs(UnmanagedType.LPWStr)] ref string pszPassword);
        }
        #endregion
    }

    */


