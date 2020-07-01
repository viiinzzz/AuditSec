using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace AuditSec
{
    public partial class StaffDetails_GUI : Form
    {
        bool NODISPOSE = false;
        public StaffDetails_GUI(bool NODISPOSE)
        {
            InitializeComponent();
            this.NODISPOSE = NODISPOSE;
        }

        public void Search(string domainuser, bool locksearch, Action<Hashtable> updateDetails)
        {
            staffDetailsControl1.Search(domainuser, locksearch, updateDetails);
        }

        public Hashtable getDetails()
        {
            return staffDetailsControl1.details;
        }

        public String getDetailsString()
        {
            return staffDetailsControl1.detailsString.ToString();
        }

        private void StaffDetails_GUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (NODISPOSE) Visible = false;
            else Dispose();
        }

        public bool isDone()
        {
            return staffDetailsControl1.DONE;
        }
    }
}
