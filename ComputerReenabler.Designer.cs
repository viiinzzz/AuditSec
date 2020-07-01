namespace AuditSec
{
    partial class ComputerReenabler
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
            this.components = new System.ComponentModel.Container();
            this.disabledView = new System.Windows.Forms.ListView();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.OULabel = new System.Windows.Forms.Label();
            this.OUBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.startBox = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.domainLabel = new System.Windows.Forms.Label();
            this.domainBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.disabledBox = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.whoBox = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel4.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // disabledView
            // 
            this.disabledView.Location = new System.Drawing.Point(10, 106);
            this.disabledView.MultiSelect = false;
            this.disabledView.Name = "disabledView";
            this.disabledView.Size = new System.Drawing.Size(462, 150);
            this.disabledView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.disabledView.TabIndex = 0;
            this.disabledView.UseCompatibleStateImageBehavior = false;
            this.disabledView.View = System.Windows.Forms.View.List;
            this.disabledView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.disabledView_ItemSelectionChanged);
            this.disabledView.DoubleClick += new System.EventHandler(this.disabledView_DoubleClick);
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.Controls.Add(this.OULabel);
            this.flowLayoutPanel4.Controls.Add(this.OUBox);
            this.flowLayoutPanel4.Controls.Add(this.label3);
            this.flowLayoutPanel4.Controls.Add(this.startBox);
            this.flowLayoutPanel4.Location = new System.Drawing.Point(10, 42);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(462, 28);
            this.flowLayoutPanel4.TabIndex = 8;
            // 
            // OULabel
            // 
            this.OULabel.AutoSize = true;
            this.OULabel.Location = new System.Drawing.Point(3, 8);
            this.OULabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.OULabel.Name = "OULabel";
            this.OULabel.Size = new System.Drawing.Size(43, 13);
            this.OULabel.TabIndex = 3;
            this.OULabel.Text = "      Site";
            // 
            // OUBox
            // 
            this.OUBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.OUBox.FormattingEnabled = true;
            this.OUBox.Location = new System.Drawing.Point(52, 3);
            this.OUBox.Name = "OUBox";
            this.OUBox.Size = new System.Drawing.Size(72, 21);
            this.OUBox.Sorted = true;
            this.OUBox.TabIndex = 1;
            this.OUBox.SelectionChangeCommitted += new System.EventHandler(this.OUBox_SelectionChangeCommitted);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(130, 8);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(146, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "        Computers starting with  ";
            // 
            // startBox
            // 
            this.startBox.Location = new System.Drawing.Point(282, 3);
            this.startBox.Name = "startBox";
            this.startBox.Size = new System.Drawing.Size(72, 20);
            this.startBox.TabIndex = 11;
            this.startBox.TextChanged += new System.EventHandler(this.startBox_TextChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.domainLabel);
            this.flowLayoutPanel1.Controls.Add(this.domainBox);
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.disabledBox);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(10, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(462, 28);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // domainLabel
            // 
            this.domainLabel.AutoSize = true;
            this.domainLabel.Location = new System.Drawing.Point(3, 8);
            this.domainLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.domainLabel.Name = "domainLabel";
            this.domainLabel.Size = new System.Drawing.Size(43, 13);
            this.domainLabel.TabIndex = 2;
            this.domainLabel.Text = "Domain";
            // 
            // domainBox
            // 
            this.domainBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.domainBox.Enabled = false;
            this.domainBox.FormattingEnabled = true;
            this.domainBox.Location = new System.Drawing.Point(52, 3);
            this.domainBox.Name = "domainBox";
            this.domainBox.Size = new System.Drawing.Size(150, 21);
            this.domainBox.Sorted = true;
            this.domainBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(208, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Disabled OU";
            // 
            // disabledBox
            // 
            this.disabledBox.Location = new System.Drawing.Point(281, 3);
            this.disabledBox.Name = "disabledBox";
            this.disabledBox.ReadOnly = true;
            this.disabledBox.Size = new System.Drawing.Size(171, 20);
            this.disabledBox.TabIndex = 12;
            this.disabledBox.Text = "Orphaned Workstations";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.label4);
            this.flowLayoutPanel2.Controls.Add(this.whoBox);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(10, 72);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(462, 28);
            this.flowLayoutPanel2.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 8);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(211, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Select Who and then Click on the Machine";
            // 
            // whoBox
            // 
            this.whoBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.whoBox.FormattingEnabled = true;
            this.whoBox.Location = new System.Drawing.Point(220, 3);
            this.whoBox.Name = "whoBox";
            this.whoBox.Size = new System.Drawing.Size(232, 21);
            this.whoBox.Sorted = true;
            this.whoBox.TabIndex = 1;
            // 
            // ComputerReenabler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 261);
            this.Controls.Add(this.flowLayoutPanel4);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.disabledView);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Name = "ComputerReenabler";
            this.Text = "Computer Re-enabler";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Reenabler_FormClosed);
            this.Load += new System.EventHandler(this.Reenabler_Load);
            this.Shown += new System.EventHandler(this.Reenabler_Shown);
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel4.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView disabledView;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.Label OULabel;
        private System.Windows.Forms.ComboBox OUBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label domainLabel;
        private System.Windows.Forms.ComboBox domainBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox startBox;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox disabledBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox whoBox;
    }
}