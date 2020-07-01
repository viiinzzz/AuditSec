namespace AuditSec
{
    partial class StaffDetails_GUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (NODISPOSE) return;
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StaffDetails_GUI));
            this.staffDetailsControl1 = new StaffDetailsControl();
            this.SuspendLayout();
            // 
            // staffDetailsControl1
            // 
            this.staffDetailsControl1.BackColor = System.Drawing.SystemColors.Control;
            this.staffDetailsControl1.Location = new System.Drawing.Point(0, 0);
            this.staffDetailsControl1.Name = "staffDetailsControl1";
            this.staffDetailsControl1.Size = new System.Drawing.Size(400, 300);
            this.staffDetailsControl1.TabIndex = 0;
            // 
            // StaffDetails_GUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(400, 301);
            this.Controls.Add(this.staffDetailsControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StaffDetails_GUI";
            this.Text = "Staff Details (as found in portal)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StaffDetails_GUI_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private StaffDetailsControl staffDetailsControl1;

    }
}