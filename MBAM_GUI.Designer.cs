namespace AuditSec
{
    partial class MBAM_GUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MBAM_GUI));
            this.mbam = new MBAMControl();
            this.SuspendLayout();
            // 
            // mbam
            // 
            this.mbam.BackColor = System.Drawing.SystemColors.Control;
            this.mbam.Location = new System.Drawing.Point(0, 0);
            this.mbam.Name = "mbam";
            this.mbam.Size = new System.Drawing.Size(371, 357);
            this.mbam.TabIndex = 0;
            // 
            // MBAM_GUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(372, 41);
            this.Controls.Add(this.mbam);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MBAM_GUI";
            this.Text = "Bitlocker Drive Recovery";
            this.ResumeLayout(false);

        }

        #endregion

        private MBAMControl mbam;
    }
}