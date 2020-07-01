namespace AuditSec
{
    partial class StaffDetailsControl
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
            this.components = new System.ComponentModel.Container();
            this.domainuserLabel = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.domainuserBox = new System.Windows.Forms.MaskedTextBox();
            this.Copy = new System.Windows.Forms.Button();
            this.Refresh = new System.Windows.Forms.Button();
            this.browser = new System.Windows.Forms.WebBrowser();
            this.browserPanel = new System.Windows.Forms.Panel();
            this.StaffDetailsBox = new System.Windows.Forms.TextBox();
            this.browserPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // domainuserLabel
            // 
            this.domainuserLabel.AutoSize = true;
            this.domainuserLabel.Location = new System.Drawing.Point(-3, 2);
            this.domainuserLabel.Name = "domainuserLabel";
            this.domainuserLabel.Size = new System.Drawing.Size(73, 13);
            this.domainuserLabel.TabIndex = 99;
            this.domainuserLabel.Text = "Domain\\User:";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 250;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // domainuserBox
            // 
            this.domainuserBox.BackColor = System.Drawing.SystemColors.Control;
            this.domainuserBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.domainuserBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.domainuserBox.Location = new System.Drawing.Point(76, 0);
            this.domainuserBox.Name = "domainuserBox";
            this.domainuserBox.Size = new System.Drawing.Size(150, 19);
            this.domainuserBox.TabIndex = 0;
            this.domainuserBox.TextChanged += new System.EventHandler(this.domainuserBox_TextChanged);
            this.domainuserBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.domainuserBox_KeyDown);
            // 
            // Copy
            // 
            this.Copy.BackColor = System.Drawing.Color.Transparent;
            this.Copy.FlatAppearance.BorderSize = 0;
            this.Copy.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this.Copy.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.Copy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Copy.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Copy.ForeColor = System.Drawing.SystemColors.Control;
            this.Copy.Image = global::AuditSec.Properties.Resources.copy;
            this.Copy.Location = new System.Drawing.Point(364, 0);
            this.Copy.Name = "Copy";
            this.Copy.Size = new System.Drawing.Size(18, 18);
            this.Copy.TabIndex = 100;
            this.Copy.UseVisualStyleBackColor = false;
            this.Copy.Click += new System.EventHandler(this.Copy_Click);
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
            this.Refresh.Location = new System.Drawing.Point(382, 0);
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
            this.browser.Size = new System.Drawing.Size(400, 275);
            this.browser.TabIndex = 99;
            this.browser.TabStop = false;
            this.browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.browser_DocumentCompleted);
            this.browser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.browser_Navigated);
            // 
            // browserPanel
            // 
            this.browserPanel.Controls.Add(this.browser);
            this.browserPanel.Location = new System.Drawing.Point(0, 19);
            this.browserPanel.Name = "browserPanel";
            this.browserPanel.Size = new System.Drawing.Size(400, 278);
            this.browserPanel.TabIndex = 99;
            this.browserPanel.Visible = false;
            // 
            // StaffDetailsBox
            // 
            this.StaffDetailsBox.BackColor = System.Drawing.Color.White;
            this.StaffDetailsBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.StaffDetailsBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StaffDetailsBox.Location = new System.Drawing.Point(0, 20);
            this.StaffDetailsBox.MaxLength = 52;
            this.StaffDetailsBox.Multiline = true;
            this.StaffDetailsBox.Name = "StaffDetailsBox";
            this.StaffDetailsBox.ReadOnly = true;
            this.StaffDetailsBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.StaffDetailsBox.Size = new System.Drawing.Size(400, 281);
            this.StaffDetailsBox.TabIndex = 102;
            // 
            // StaffDetailsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.browserPanel);
            this.Controls.Add(this.StaffDetailsBox);
            this.Controls.Add(this.Copy);
            this.Controls.Add(this.Refresh);
            this.Controls.Add(this.domainuserBox);
            this.Controls.Add(this.domainuserLabel);
            this.Name = "StaffDetailsControl";
            this.Size = new System.Drawing.Size(400, 300);
            this.browserPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label domainuserLabel;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.MaskedTextBox domainuserBox;
        private System.Windows.Forms.Button Copy;
        private System.Windows.Forms.Button Refresh;
        private System.Windows.Forms.WebBrowser browser;
        private System.Windows.Forms.Panel browserPanel;
        public System.Windows.Forms.TextBox StaffDetailsBox;
    }
}
