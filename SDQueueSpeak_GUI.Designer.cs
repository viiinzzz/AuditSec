namespace AuditSec
{
    partial class SDQueueSpeak_GUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SDQueueSpeak_GUI));
            this.sdQueueSpeakControl1 = new SDQueueSpeakControl();
            this.SuspendLayout();
            // 
            // sdQueueSpeakControl1
            // 
            this.sdQueueSpeakControl1.BackColor = System.Drawing.Color.White;
            this.sdQueueSpeakControl1.Location = new System.Drawing.Point(0, 0);
            this.sdQueueSpeakControl1.Name = "sdQueueSpeakControl1";
            this.sdQueueSpeakControl1.Size = new System.Drawing.Size(1024, 300);
            this.sdQueueSpeakControl1.TabIndex = 0;
            // 
            // SDQueueSpeak_GUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1022, 301);
            this.Controls.Add(this.sdQueueSpeakControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SDQueueSpeak_GUI";
            this.Text = "ServiceDesk Queue Speak (as found in Axyos)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StaffDetails_GUI_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private SDQueueSpeakControl sdQueueSpeakControl1;


    }
}