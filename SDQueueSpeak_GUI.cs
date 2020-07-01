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
    public partial class SDQueueSpeak_GUI : Form
    {
        bool NODISPOSE = false;
        public SDQueueSpeak_GUI(bool NODISPOSE)
        {
            InitializeComponent();
            this.NODISPOSE = NODISPOSE;
        }

        private void StaffDetails_GUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (NODISPOSE) Visible = false;
            else Dispose();
        }

    }
}
