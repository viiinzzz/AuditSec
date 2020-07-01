namespace AuditSec
{
    partial class SDQueueSpeakControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.URLLabel = new System.Windows.Forms.Label();
            this.URLBox = new System.Windows.Forms.MaskedTextBox();
            this.Refresh = new System.Windows.Forms.Button();
            this.browser = new System.Windows.Forms.WebBrowser();
            this.browserPanel = new System.Windows.Forms.Panel();
            this.OutputBox = new System.Windows.Forms.TextBox();
            this.browserPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // URLLabel
            // 
            this.URLLabel.AutoSize = true;
            this.URLLabel.Location = new System.Drawing.Point(-3, 2);
            this.URLLabel.Name = "URLLabel";
            this.URLLabel.Size = new System.Drawing.Size(67, 13);
            this.URLLabel.TabIndex = 99;
            this.URLLabel.Text = "Queue URL:";
            // 
            // URLBox
            // 
            this.URLBox.BackColor = System.Drawing.SystemColors.Control;
            this.URLBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.URLBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.URLBox.Location = new System.Drawing.Point(70, 0);
            this.URLBox.Name = "URLBox";
            this.URLBox.Size = new System.Drawing.Size(912, 13);
            this.URLBox.TabIndex = 0;
            this.URLBox.Text = "http://servicew.pxl.int/assystweb/application.do#eventsearch%2FEventSearchDelegat" +
                "ingDispatchAction.do%3Fdispatch%3DloadQuery%26context%3Dselect%26queryProfileFor" +
                "m.queryProfileId%3D7755";
            // 
            // Refresh
            // 
            this.Refresh.BackColor = System.Drawing.Color.Transparent;
            this.Refresh.FlatAppearance.BorderSize = 0;
            this.Refresh.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.Refresh.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.Refresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Refresh.ForeColor = System.Drawing.SystemColors.Control;
            this.Refresh.Image = global::AuditSec.Properties.Resources.refresh;
            this.Refresh.Location = new System.Drawing.Point(1006, 0);
            this.Refresh.Name = "Refresh";
            this.Refresh.Size = new System.Drawing.Size(18, 18);
            this.Refresh.TabIndex = 101;
            this.Refresh.UseVisualStyleBackColor = false;
            this.Refresh.Click += new System.EventHandler(this.Refresh_Click);
            // 
            // browser
            // 
            this.browser.IsWebBrowserContextMenuEnabled = false;
            this.browser.Location = new System.Drawing.Point(0, 0);
            this.browser.MinimumSize = new System.Drawing.Size(20, 20);
            this.browser.Name = "browser";
            this.browser.Size = new System.Drawing.Size(982, 275);
            this.browser.TabIndex = 99;
            this.browser.TabStop = false;
            this.browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.browser_DocumentCompleted);
            this.browser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.browser_Navigated);
            // 
            // browserPanel
            // 
            this.browserPanel.Controls.Add(this.browser);
            this.browserPanel.Location = new System.Drawing.Point(42, 19);
            this.browserPanel.Name = "browserPanel";
            this.browserPanel.Size = new System.Drawing.Size(982, 278);
            this.browserPanel.TabIndex = 99;
            this.browserPanel.Visible = false;
            // 
            // OutputBox
            // 
            this.OutputBox.BackColor = System.Drawing.Color.White;
            this.OutputBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.OutputBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OutputBox.Location = new System.Drawing.Point(43, 20);
            this.OutputBox.MaxLength = 52;
            this.OutputBox.Multiline = true;
            this.OutputBox.Name = "OutputBox";
            this.OutputBox.ReadOnly = true;
            this.OutputBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.OutputBox.Size = new System.Drawing.Size(981, 280);
            this.OutputBox.TabIndex = 102;
            // 
            // SDQueueSpeakControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.browserPanel);
            this.Controls.Add(this.OutputBox);
            this.Controls.Add(this.Refresh);
            this.Controls.Add(this.URLBox);
            this.Controls.Add(this.URLLabel);
            this.Name = "SDQueueSpeakControl";
            this.Size = new System.Drawing.Size(1024, 300);
            this.browserPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label URLLabel;
        private System.Windows.Forms.MaskedTextBox URLBox;
        private System.Windows.Forms.Button Refresh;
        private System.Windows.Forms.WebBrowser browser;
        private System.Windows.Forms.Panel browserPanel;
        public System.Windows.Forms.TextBox OutputBox;
    }
}
