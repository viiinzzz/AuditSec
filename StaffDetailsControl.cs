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
using System.Collections;


namespace AuditSec
{
    public partial class StaffDetailsControl : UserControl
    {
        public Hashtable details = new Hashtable();
        public StringBuilder detailsString = new StringBuilder();

        int timer_i = 0;
        int timer_max = 10;
        string domainuser = "";

        string BlankPage = "about:blank";
        string getStaffDetailsURL(string domainuser) { return "http://MYCOMPANY.COM/Person.aspx?accountname=" + domainuser.Replace(@"\", "%5C");}

        Action<Hashtable> updateDetails = null;

        public StaffDetailsControl()
        {
            InitializeComponent();
            domainuser = domainuserBox.Text;
            timer1.Start();
        }



        public bool DONE = false;
        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (updateDetails != null) updateDetails(null);
            try
            {
                if (e.Url.ToString().ToLower().Equals(BlankPage.ToLower()))
                {
                    details.Clear();
                    detailsString.Clear();
                    domainuserBox.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    string additionalHeaderInfo = "Authorization: Basic "
                        + Convert.ToBase64String(Encoding.ASCII.GetBytes("MyUsername" + ":" + "MyPassword"))
                        + System.Environment.NewLine;
                    browser.Navigate(new Uri(getStaffDetailsURL(domainuser)), null, null, additionalHeaderInfo);
                }
                else
                {
                    extract();
                    if (details.Count == 0) domainuserBox.ForeColor = System.Drawing.Color.Red;
                    else
                    {
                        StaffDetailsBox.Text = detailsString.ToString();
                        detailsString.Insert(0, "Username=" + domainuser + "\r\n");
                        details["Username"] = domainuser;
                        domainuserBox.ForeColor = System.Drawing.Color.DarkGreen;

                        if (updateDetails != null) updateDetails(details);

                    }
                }
            }
            catch (Exception ee)
            {
                domainuserBox.ForeColor = System.Drawing.Color.Red;
                Console.WriteLine("Error after the document completed: " + ee.ToString());
            }
            DONE = true;
        }

        
        void extract(HtmlElement el)
        {
            if (el.TagName.ToUpper().Equals("DIV") && el.InnerHtml != null && Regex.IsMatch(el.InnerHtml, "^(.*) *: *<A.*>(.*)</A>$"))
            {
                string label = Regex.Replace(el.InnerHtml, "^(.*) *: *<A.*>(.*)</A>$", "$1").Trim();
                string value = Regex.Replace(el.InnerHtml, "^(.*) *: *<A.*>(.*)</A>$", "$2").Trim();
                //Console.WriteLine(label + "=" + value);
                details[label] = value;
                detailsString.AppendLine(label + "=" + value);
            }
            foreach (HtmlElement child in el.Children)
                extract(child);
        }


        void extract()
        {
            HtmlElement html = browser.Document.GetElementsByTagName("html")[0];
            //Console.WriteLine(html.OuterHtml);
            extract(html);
        }




        private void timer1_Tick(object sender, EventArgs e)
        {
            if (domainuserBox.Text.Length == 0 || domainuserBox.Text.ToUpper().Equals(domainuser))
            {
                timer_i = 0;
                if (!domainuserBox.ReadOnly) domainuserBox.Focus();
            }
            else
            {
                StaffDetailsBox.Text = "";
                timer_i++;
                if (timer_i > timer_max)
                {
                    domainuser = domainuserBox.Text.ToUpper();
                    timer_i = 0;
                    startSearch();
                }
            }
        }

        void startSearch()
        {
            DONE = false;
            browser.Url = new Uri(BlankPage);
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            if (StaffDetailsBox.Text != null && StaffDetailsBox.Text.Length > 0)
                Clipboard.SetText(StaffDetailsBox.Text);
        }


        private void Refresh_Click(object sender, EventArgs e)
        {
            if (domainuser == null || domainuser.Length == 0) return;
            startSearch();
        }

        private void domainuserBox_TextChanged(object sender, EventArgs e)
        {
            timer_i = 0;
            domainuserBox.ForeColor = System.Drawing.SystemColors.ControlText;
            StaffDetailsBox.Text = "";
            details.Clear();
            detailsString.Clear();
        }

        private void domainuserBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                timer_i = timer_max;
        }


        public void Search(string x, bool locksearch, Action<Hashtable> updateDetails)
        {
            if (updateDetails != null) this.updateDetails = updateDetails;
            domainuserBox.Text = x;
            if (locksearch) { domainuserBox.ReadOnly = true; StaffDetailsBox.Focus(); }
            timer_i = timer_max;
        }

        private void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            AuditSec.InjectAlertBlocker(browser);
            //Console.WriteLine("Browser: navigated. " + e.Url);
        }
    }
}
