namespace AuditSec
{
    partial class ComputerChooser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComputerChooser));
            this.DomainsBox = new System.Windows.Forms.CheckedListBox();
            this.CancelButton = new System.Windows.Forms.Button();
            this.ADTree = new System.Windows.Forms.TreeView();
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.DptsBox = new System.Windows.Forms.CheckedListBox();
            this.ADLabel = new System.Windows.Forms.Label();
            this.ForestBox = new System.Windows.Forms.TextBox();
            this.ADCount = new System.Windows.Forms.Label();
            this.ADTreeRefresh = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.OKButton = new System.Windows.Forms.Button();
            this.MachinesBox = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.PCCount = new System.Windows.Forms.TextBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.SitesBox = new System.Windows.Forms.CheckedListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.searcher = new System.ComponentModel.BackgroundWorker();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // DomainsBox
            // 
            this.DomainsBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DomainsBox.CheckOnClick = true;
            this.DomainsBox.ColumnWidth = 50;
            this.DomainsBox.FormattingEnabled = true;
            this.DomainsBox.Location = new System.Drawing.Point(266, 21);
            this.DomainsBox.MultiColumn = true;
            this.DomainsBox.Name = "DomainsBox";
            this.DomainsBox.Size = new System.Drawing.Size(63, 210);
            this.DomainsBox.Sorted = true;
            this.DomainsBox.TabIndex = 6;
            this.DomainsBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.DomainsBox_ItemCheck);
            this.DomainsBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DomainsBox_MouseMove);
            // 
            // CancelButton
            // 
            this.CancelButton.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton.Location = new System.Drawing.Point(166, 231);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(160, 30);
            this.CancelButton.TabIndex = 21;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = false;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ADTree
            // 
            this.ADTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ADTree.ImageIndex = 0;
            this.ADTree.ImageList = this.imageList2;
            this.ADTree.Location = new System.Drawing.Point(3, 21);
            this.ADTree.Name = "ADTree";
            this.ADTree.SelectedImageIndex = 0;
            this.ADTree.ShowLines = false;
            this.ADTree.Size = new System.Drawing.Size(254, 240);
            this.ADTree.TabIndex = 1;
            this.ADTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ADTree_AfterSelect);
            // 
            // imageList2
            // 
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList2.Images.SetKeyName(0, "Container.png");
            this.imageList2.Images.SetKeyName(1, "Container2.png");
            this.imageList2.Images.SetKeyName(2, "Domain.png");
            this.imageList2.Images.SetKeyName(3, "Domain2.png");
            // 
            // DptsBox
            // 
            this.DptsBox.BackColor = System.Drawing.SystemColors.Window;
            this.DptsBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DptsBox.CheckOnClick = true;
            this.DptsBox.ColumnWidth = 83;
            this.DptsBox.FormattingEnabled = true;
            this.DptsBox.Location = new System.Drawing.Point(509, 22);
            this.DptsBox.MultiColumn = true;
            this.DptsBox.Name = "DptsBox";
            this.DptsBox.Size = new System.Drawing.Size(166, 240);
            this.DptsBox.Sorted = true;
            this.DptsBox.TabIndex = 27;
            this.DptsBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.DptsBox_ItemCheck);
            this.DptsBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ADDptBox_MouseMove);
            // 
            // ADLabel
            // 
            this.ADLabel.AutoSize = true;
            this.ADLabel.Image = global::AuditSec.Properties.Resources.Container;
            this.ADLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ADLabel.Location = new System.Drawing.Point(3, 5);
            this.ADLabel.Name = "ADLabel";
            this.ADLabel.Size = new System.Drawing.Size(100, 13);
            this.ADLabel.TabIndex = 29;
            this.ADLabel.Text = "      Active Directory";
            // 
            // ForestBox
            // 
            this.ForestBox.BackColor = System.Drawing.SystemColors.Window;
            this.ForestBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ForestBox.Location = new System.Drawing.Point(152, 7);
            this.ForestBox.Name = "ForestBox";
            this.ForestBox.ReadOnly = true;
            this.ForestBox.Size = new System.Drawing.Size(105, 13);
            this.ForestBox.TabIndex = 30;
            this.ForestBox.TabStop = false;
            this.ForestBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // ADCount
            // 
            this.ADCount.AutoSize = true;
            this.ADCount.Location = new System.Drawing.Point(3, 242);
            this.ADCount.Name = "ADCount";
            this.ADCount.Size = new System.Drawing.Size(54, 13);
            this.ADCount.TabIndex = 31;
            this.ADCount.Text = "Loading...";
            // 
            // ADTreeRefresh
            // 
            this.ADTreeRefresh.BackColor = System.Drawing.Color.Transparent;
            this.ADTreeRefresh.FlatAppearance.BorderSize = 0;
            this.ADTreeRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ADTreeRefresh.ForeColor = System.Drawing.Color.Transparent;
            this.ADTreeRefresh.Image = global::AuditSec.Properties.Resources.refresh;
            this.ADTreeRefresh.Location = new System.Drawing.Point(105, 2);
            this.ADTreeRefresh.Name = "ADTreeRefresh";
            this.ADTreeRefresh.Size = new System.Drawing.Size(18, 18);
            this.ADTreeRefresh.TabIndex = 34;
            this.ADTreeRefresh.UseVisualStyleBackColor = false;
            this.ADTreeRefresh.Click += new System.EventHandler(this.ADTreeRefresh_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Image = global::AuditSec.Properties.Resources.Container;
            this.label10.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label10.Location = new System.Drawing.Point(506, 7);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(85, 13);
            this.label10.TabIndex = 38;
            this.label10.Text = "      Departments";
            // 
            // ToolTip
            // 
            this.ToolTip.AutoPopDelay = 15000;
            this.ToolTip.BackColor = System.Drawing.SystemColors.Window;
            this.ToolTip.InitialDelay = 500;
            this.ToolTip.ReshowDelay = 100;
            this.ToolTip.ToolTipTitle = "Collection Properties";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Image = global::AuditSec.Properties.Resources.Domain;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Location = new System.Drawing.Point(263, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 39;
            this.label1.Text = "      Domains";
            // 
            // OKButton
            // 
            this.OKButton.BackColor = System.Drawing.SystemColors.Control;
            this.OKButton.Location = new System.Drawing.Point(1, 231);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(160, 30);
            this.OKButton.TabIndex = 40;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = false;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // MachinesBox
            // 
            this.MachinesBox.BackColor = System.Drawing.SystemColors.Control;
            this.MachinesBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MachinesBox.ColumnWidth = 163;
            this.MachinesBox.FormattingEnabled = true;
            this.MachinesBox.Location = new System.Drawing.Point(683, 21);
            this.MachinesBox.MultiColumn = true;
            this.MachinesBox.Name = "MachinesBox";
            this.MachinesBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.MachinesBox.Size = new System.Drawing.Size(326, 208);
            this.MachinesBox.Sorted = true;
            this.MachinesBox.TabIndex = 41;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.Control;
            this.label2.Image = global::AuditSec.Properties.Resources.Col;
            this.label2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.Location = new System.Drawing.Point(-1, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 42;
            this.label2.Text = "      SelectedPCs";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.PCCount);
            this.panel1.Controls.Add(this.SearchButton);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.OKButton);
            this.panel1.Controls.Add(this.CancelButton);
            this.panel1.Location = new System.Drawing.Point(683, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(326, 262);
            this.panel1.TabIndex = 43;
            // 
            // PCCount
            // 
            this.PCCount.BackColor = System.Drawing.SystemColors.Control;
            this.PCCount.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PCCount.Location = new System.Drawing.Point(91, 5);
            this.PCCount.Name = "PCCount";
            this.PCCount.ReadOnly = true;
            this.PCCount.Size = new System.Drawing.Size(105, 13);
            this.PCCount.TabIndex = 46;
            this.PCCount.TabStop = false;
            this.PCCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // SearchButton
            // 
            this.SearchButton.BackColor = System.Drawing.Color.Transparent;
            this.SearchButton.FlatAppearance.BorderSize = 0;
            this.SearchButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SearchButton.ForeColor = System.Drawing.Color.Transparent;
            this.SearchButton.Image = global::AuditSec.Properties.Resources.Glass;
            this.SearchButton.Location = new System.Drawing.Point(12, 237);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(18, 18);
            this.SearchButton.TabIndex = 46;
            this.SearchButton.UseVisualStyleBackColor = false;
            this.SearchButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // SitesBox
            // 
            this.SitesBox.BackColor = System.Drawing.SystemColors.Window;
            this.SitesBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SitesBox.CheckOnClick = true;
            this.SitesBox.ColumnWidth = 83;
            this.SitesBox.FormattingEnabled = true;
            this.SitesBox.Location = new System.Drawing.Point(335, 22);
            this.SitesBox.MultiColumn = true;
            this.SitesBox.Name = "SitesBox";
            this.SitesBox.Size = new System.Drawing.Size(166, 240);
            this.SitesBox.Sorted = true;
            this.SitesBox.TabIndex = 44;
            this.SitesBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.SitesBox_ItemCheck);
            this.SitesBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SiteBox_MouseMove);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Image = global::AuditSec.Properties.Resources.Container;
            this.label3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label3.Location = new System.Drawing.Point(332, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 13);
            this.label3.TabIndex = 45;
            this.label3.Text = "      Sites";
            // 
            // searcher
            // 
            this.searcher.WorkerReportsProgress = true;
            this.searcher.WorkerSupportsCancellation = true;
            this.searcher.DoWork += new System.ComponentModel.DoWorkEventHandler(this.searcher_DoWork);
            this.searcher.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.searcher_ProgressChanged);
            // 
            // ComputerChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1008, 262);
            this.Controls.Add(this.SitesBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.MachinesBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DomainsBox);
            this.Controls.Add(this.DptsBox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.ADTreeRefresh);
            this.Controls.Add(this.ADTree);
            this.Controls.Add(this.ADCount);
            this.Controls.Add(this.ForestBox);
            this.Controls.Add(this.ADLabel);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ComputerChooser";
            this.Text = "PC Chooser";
            this.Shown += new System.EventHandler(this.MachineChooser_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox DomainsBox;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.TreeView ADTree;
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.CheckedListBox DptsBox;
        private System.Windows.Forms.Label ADLabel;
        private System.Windows.Forms.TextBox ForestBox;
        private System.Windows.Forms.Label ADCount;
        private System.Windows.Forms.Button ADTreeRefresh;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.DataGridViewTextBoxColumn Action_;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParentColName_;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParentColID_;
        private System.Windows.Forms.DataGridViewTextBoxColumn Comment_;
        private System.Windows.Forms.DataGridViewTextBoxColumn Schedule_;
        private System.Windows.Forms.DataGridViewTextBoxColumn QueryRule_;
        private System.Windows.Forms.DataGridViewTextBoxColumn CollectionID_;
        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.ListBox MachinesBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckedListBox SitesBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button SearchButton;
        private System.ComponentModel.BackgroundWorker searcher;
        private System.Windows.Forms.TextBox PCCount;
    }
}