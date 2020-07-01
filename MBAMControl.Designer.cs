namespace AuditSec
{
    partial class MBAMControl
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
            this.browserPanel = new System.Windows.Forms.Panel();
            this.browser = new System.Windows.Forms.WebBrowser();
            this.label1 = new System.Windows.Forms.Label();
            this.KeyBox = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.IDBox = new System.Windows.Forms.MaskedTextBox();
            this.Copy = new System.Windows.Forms.Button();
            this.Refresh = new System.Windows.Forms.Button();
            this.clearusrButton = new System.Windows.Forms.Button();
            this.browserPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // browserPanel
            // 
            this.browserPanel.Controls.Add(this.browser);
            this.browserPanel.Location = new System.Drawing.Point(0, 40);
            this.browserPanel.Name = "browserPanel";
            this.browserPanel.Size = new System.Drawing.Size(371, 317);
            this.browserPanel.TabIndex = 99;
            // 
            // browser
            // 
            this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browser.IsWebBrowserContextMenuEnabled = false;
            this.browser.Location = new System.Drawing.Point(0, 0);
            this.browser.MinimumSize = new System.Drawing.Size(20, 20);
            this.browser.Name = "browser";
            this.browser.Size = new System.Drawing.Size(371, 317);
            this.browser.TabIndex = 99;
            this.browser.TabStop = false;
            this.browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.browser_DocumentCompleted);
            this.browser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.browser_Navigated);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 99;
            this.label1.Text = "Rec.K.ID:";
            // 
            // KeyBox
            // 
            this.KeyBox.BackColor = System.Drawing.SystemColors.Control;
            this.KeyBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.KeyBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyBox.Location = new System.Drawing.Point(96, 2);
            this.KeyBox.MaxLength = 52;
            this.KeyBox.Multiline = true;
            this.KeyBox.Name = "KeyBox";
            this.KeyBox.ReadOnly = true;
            this.KeyBox.Size = new System.Drawing.Size(292, 36);
            this.KeyBox.TabIndex = 2;
            this.KeyBox.Text = "000000-000000-000000-000000-000000-000000-000000-000000";
            this.KeyBox.Click += new System.EventHandler(this.KeyBox_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 250;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // IDBox
            // 
            this.IDBox.BackColor = System.Drawing.SystemColors.Control;
            this.IDBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.IDBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IDBox.Location = new System.Drawing.Point(0, 20);
            this.IDBox.Mask = "&&&&&&&&";
            this.IDBox.Name = "IDBox";
            this.IDBox.Size = new System.Drawing.Size(86, 19);
            this.IDBox.TabIndex = 0;
            this.IDBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.IDBox_KeyPress);
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
            this.Copy.Location = new System.Drawing.Point(77, 1);
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
            this.Refresh.Location = new System.Drawing.Point(58, 1);
            this.Refresh.Name = "Refresh";
            this.Refresh.Size = new System.Drawing.Size(18, 18);
            this.Refresh.TabIndex = 101;
            this.Refresh.UseVisualStyleBackColor = false;
            this.Refresh.Click += new System.EventHandler(this.ADTreeRefresh_Click);
            // 
            // clearusrButton
            // 
            this.clearusrButton.FlatAppearance.BorderSize = 0;
            this.clearusrButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.clearusrButton.Font = new System.Drawing.Font("Wingdings 2", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.clearusrButton.Location = new System.Drawing.Point(83, 19);
            this.clearusrButton.Name = "clearusrButton";
            this.clearusrButton.Size = new System.Drawing.Size(18, 21);
            this.clearusrButton.TabIndex = 102;
            this.clearusrButton.TabStop = false;
            this.clearusrButton.Text = "O";
            this.clearusrButton.UseVisualStyleBackColor = true;
            this.clearusrButton.Click += new System.EventHandler(this.clearusrButton_Click);
            // 
            // MBAMControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.IDBox);
            this.Controls.Add(this.KeyBox);
            this.Controls.Add(this.clearusrButton);
            this.Controls.Add(this.Copy);
            this.Controls.Add(this.Refresh);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.browserPanel);
            this.Name = "MBAMControl";
            this.Size = new System.Drawing.Size(371, 362);
            this.browserPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel browserPanel;
        private System.Windows.Forms.WebBrowser browser;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.MaskedTextBox IDBox;
        public System.Windows.Forms.TextBox KeyBox;
        private System.Windows.Forms.Button Copy;
        private System.Windows.Forms.Button Refresh;
        private System.Windows.Forms.Button clearusrButton;
    }
}
