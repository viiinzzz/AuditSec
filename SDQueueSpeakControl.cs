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
    public partial class SDQueueSpeakControl : UserControl
    {
        string BlankPage = "about:blank";


        public SDQueueSpeakControl()
        {
            InitializeComponent();
            startSearch();
        }



        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                if (e.Url.ToString().ToLower().Equals(BlankPage.ToLower()))
                {
                    URLBox.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    string additionalHeaderInfo = "Authorization: Basic "
                        + Convert.ToBase64String(Encoding.ASCII.GetBytes("MyUsername" + ":" + "MyPassword"))
                        + System.Environment.NewLine;
                    browser.Navigate(new Uri(URLBox.Text), null, null, additionalHeaderInfo);
                }
                else
                {
                    OutputBox.Text = "";
                    extract();
                    AuditSec.Speak(OutputBox.Text, true, false, null);
                    URLBox.ForeColor = System.Drawing.Color.DarkGreen;
                }
            }
            catch (Exception ee)
            {
                URLBox.ForeColor = System.Drawing.Color.Red;
                Console.WriteLine("Error after the document completed: " + ee.ToString());
            }
        }

        
        void extract(HtmlElement el)
        {
            if (el.TagName.ToUpper().Equals("TD") && el.GetAttribute("role") == "gridcell")
            {
                string text = (el.InnerText == null ? "" : el.InnerText).Replace('\n', ' ').Trim();
                Console.WriteLine(@"""" + text + @"""\n");
                bool isNumber = Regex.IsMatch(text, @"\d+");
                if (isNumber)  OutputBox.Text += "\r\n";
                if (!isNumber && text.Length > 0)
                    OutputBox.Text += (OutputBox.Text.Length > 0 ? " " : "") + text;
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




        void startSearch()
        {
            browser.Url = new Uri(BlankPage);
        }








        private void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            AuditSec.InjectAlertBlocker(browser);
            //Console.WriteLine("Browser: navigated. " + e.Url);
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            startSearch();
        }
    }
}
