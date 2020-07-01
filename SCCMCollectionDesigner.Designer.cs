namespace AuditSec
{
    partial class SCCMCollectionDesigner
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
            saveSettings(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SCCMCollectionDesigner));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.SiteCodeBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SCCMTree = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.SCCMLabel = new System.Windows.Forms.Label();
            this.BaseColBox = new System.Windows.Forms.TextBox();
            this.DomainsBox = new System.Windows.Forms.CheckedListBox();
            this.DptLabel = new System.Windows.Forms.Label();
            this.DptTree = new System.Windows.Forms.TreeView();
            this.DptColBox = new System.Windows.Forms.TextBox();
            this.PrepareSite = new System.Windows.Forms.Button();
            this.PrepareDpts = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.SiteNameBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.CheckConsistency = new System.Windows.Forms.Button();
            this.Queue = new System.Windows.Forms.DataGridView();
            this.ProceedButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.DptColIDBox = new System.Windows.Forms.TextBox();
            this.BaseColIDBox = new System.Windows.Forms.TextBox();
            this.ADTree = new System.Windows.Forms.TreeView();
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.ADDptBox = new System.Windows.Forms.CheckedListBox();
            this.ADLabel = new System.Windows.Forms.Label();
            this.ForestBox = new System.Windows.Forms.TextBox();
            this.ADCount = new System.Windows.Forms.Label();
            this.baseCount = new System.Windows.Forms.Label();
            this.dptCount = new System.Windows.Forms.Label();
            this.ADTreeRefresh = new System.Windows.Forms.Button();
            this.SCCMTreeRefresh = new System.Windows.Forms.Button();
            this.DptTreeRefresh = new System.Windows.Forms.Button();
            this.TemplDptBox = new System.Windows.Forms.CheckedListBox();
            this.label10 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ResetSchedule = new System.Windows.Forms.Button();
            this.hourSpanBox = new System.Windows.Forms.MaskedTextBox();
            this.startBox = new System.Windows.Forms.MaskedTextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ClearButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dptHourSpanBox = new System.Windows.Forms.MaskedTextBox();
            this.dptStartBox = new System.Windows.Forms.MaskedTextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.Action = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ParentColName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ParentColID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Name_ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Comment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Schedule = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.QueryRule = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CollectionID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Queue)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // SiteCodeBox
            // 
            this.SiteCodeBox.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SiteCodeBox.Location = new System.Drawing.Point(63, 45);
            this.SiteCodeBox.MaxLength = 3;
            this.SiteCodeBox.Name = "SiteCodeBox";
            this.SiteCodeBox.Size = new System.Drawing.Size(33, 20);
            this.SiteCodeBox.TabIndex = 0;
            this.SiteCodeBox.Text = "XXX";
            this.SiteCodeBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.Info;
            this.label2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.Location = new System.Drawing.Point(-1, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Domains";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SCCMTree
            // 
            this.SCCMTree.ImageIndex = 0;
            this.SCCMTree.ImageList = this.imageList1;
            this.SCCMTree.Location = new System.Drawing.Point(280, 176);
            this.SCCMTree.Name = "SCCMTree";
            this.SCCMTree.SelectedImageIndex = 0;
            this.SCCMTree.ShowLines = false;
            this.SCCMTree.Size = new System.Drawing.Size(254, 103);
            this.SCCMTree.TabIndex = 2;
            this.SCCMTree.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.SCCMTree_AfterCollapse);
            this.SCCMTree.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.SCCMTree_AfterExpand);
            this.SCCMTree.NodeMouseHover += new System.Windows.Forms.TreeNodeMouseHoverEventHandler(this.SCCMTree_NodeMouseHover);
            this.SCCMTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SCCMTree_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Col.png");
            this.imageList1.Images.SetKeyName(1, "Col2.png");
            // 
            // SCCMLabel
            // 
            this.SCCMLabel.AutoSize = true;
            this.SCCMLabel.Image = ((System.Drawing.Image)(resources.GetObject("SCCMLabel.Image")));
            this.SCCMLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SCCMLabel.Location = new System.Drawing.Point(277, 140);
            this.SCCMLabel.Name = "SCCMLabel";
            this.SCCMLabel.Size = new System.Drawing.Size(55, 13);
            this.SCCMLabel.TabIndex = 4;
            this.SCCMLabel.Text = "      SCCM";
            // 
            // BaseColBox
            // 
            this.BaseColBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.BaseColBox.Location = new System.Drawing.Point(280, 156);
            this.BaseColBox.Name = "BaseColBox";
            this.BaseColBox.ReadOnly = true;
            this.BaseColBox.Size = new System.Drawing.Size(254, 13);
            this.BaseColBox.TabIndex = 5;
            this.BaseColBox.TabStop = false;
            this.BaseColBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // DomainsBox
            // 
            this.DomainsBox.ColumnWidth = 50;
            this.DomainsBox.FormattingEnabled = true;
            this.DomainsBox.Location = new System.Drawing.Point(18, 99);
            this.DomainsBox.MultiColumn = true;
            this.DomainsBox.Name = "DomainsBox";
            this.DomainsBox.Size = new System.Drawing.Size(248, 34);
            this.DomainsBox.Sorted = true;
            this.DomainsBox.TabIndex = 6;
            // 
            // DptLabel
            // 
            this.DptLabel.AutoSize = true;
            this.DptLabel.Image = ((System.Drawing.Image)(resources.GetObject("DptLabel.Image")));
            this.DptLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DptLabel.Location = new System.Drawing.Point(541, 140);
            this.DptLabel.Name = "DptLabel";
            this.DptLabel.Size = new System.Drawing.Size(94, 13);
            this.DptLabel.TabIndex = 7;
            this.DptLabel.Text = "      Template Dpts";
            // 
            // DptTree
            // 
            this.DptTree.ImageIndex = 0;
            this.DptTree.ImageList = this.imageList1;
            this.DptTree.Location = new System.Drawing.Point(544, 185);
            this.DptTree.Name = "DptTree";
            this.DptTree.SelectedImageIndex = 0;
            this.DptTree.ShowLines = false;
            this.DptTree.Size = new System.Drawing.Size(254, 94);
            this.DptTree.TabIndex = 8;
            this.DptTree.Visible = false;
            this.DptTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.DptTree_AfterSelect);
            // 
            // DptColBox
            // 
            this.DptColBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DptColBox.Location = new System.Drawing.Point(544, 156);
            this.DptColBox.Name = "DptColBox";
            this.DptColBox.ReadOnly = true;
            this.DptColBox.Size = new System.Drawing.Size(254, 13);
            this.DptColBox.TabIndex = 9;
            this.DptColBox.TabStop = false;
            this.DptColBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // PrepareSite
            // 
            this.PrepareSite.BackColor = System.Drawing.SystemColors.Control;
            this.PrepareSite.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PrepareSite.Location = new System.Drawing.Point(264, 73);
            this.PrepareSite.Name = "PrepareSite";
            this.PrepareSite.Size = new System.Drawing.Size(171, 34);
            this.PrepareSite.TabIndex = 3;
            this.PrepareSite.Text = "Prepare SCCM Site";
            this.PrepareSite.UseVisualStyleBackColor = false;
            this.PrepareSite.Click += new System.EventHandler(this.PrepareSiteButton_Click);
            // 
            // PrepareDpts
            // 
            this.PrepareDpts.BackColor = System.Drawing.SystemColors.Control;
            this.PrepareDpts.Location = new System.Drawing.Point(3, 73);
            this.PrepareDpts.Name = "PrepareDpts";
            this.PrepareDpts.Size = new System.Drawing.Size(174, 34);
            this.PrepareDpts.TabIndex = 11;
            this.PrepareDpts.Text = "Prepare  Template Dpts";
            this.PrepareDpts.UseVisualStyleBackColor = false;
            this.PrepareDpts.Click += new System.EventHandler(this.PrepareTemplateDptsCreationButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.SystemColors.Info;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(3, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "New Site";
            // 
            // SiteNameBox
            // 
            this.SiteNameBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SiteNameBox.Location = new System.Drawing.Point(119, 44);
            this.SiteNameBox.MaxLength = 30;
            this.SiteNameBox.Name = "SiteNameBox";
            this.SiteNameBox.Size = new System.Drawing.Size(131, 20);
            this.SiteNameBox.TabIndex = 12;
            this.SiteNameBox.Text = "Xxxxxxxxxx";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(3, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(503, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "This module helps creating new collections in SCCM for a new site and its departm" +
    "ents.";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Image = global::AuditSec.Properties.Resources.Col;
            this.label7.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label7.Location = new System.Drawing.Point(625, 169);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(94, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "      Template Dpts";
            // 
            // CheckConsistency
            // 
            this.CheckConsistency.Location = new System.Drawing.Point(15, 153);
            this.CheckConsistency.Name = "CheckConsistency";
            this.CheckConsistency.Size = new System.Drawing.Size(254, 23);
            this.CheckConsistency.TabIndex = 17;
            this.CheckConsistency.Text = "Check Active Directory / Collections Consistency ";
            this.CheckConsistency.UseVisualStyleBackColor = true;
            this.CheckConsistency.Click += new System.EventHandler(this.CheckStructureConsistency_Click);
            // 
            // Queue
            // 
            this.Queue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Queue.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Action,
            this.ParentColName,
            this.ParentColID,
            this.Name_,
            this.Comment,
            this.Schedule,
            this.QueryRule,
            this.CollectionID});
            this.Queue.Location = new System.Drawing.Point(0, 285);
            this.Queue.Name = "Queue";
            this.Queue.Size = new System.Drawing.Size(814, 240);
            this.Queue.TabIndex = 18;
            this.Queue.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Queue_CellDoubleClick);
            // 
            // ProceedButton
            // 
            this.ProceedButton.Location = new System.Drawing.Point(280, 527);
            this.ProceedButton.Name = "ProceedButton";
            this.ProceedButton.Size = new System.Drawing.Size(254, 26);
            this.ProceedButton.TabIndex = 19;
            this.ProceedButton.Text = "Proceed Queue";
            this.ProceedButton.UseVisualStyleBackColor = true;
            this.ProceedButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(544, 527);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(254, 26);
            this.CancelButton.TabIndex = 21;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // DptColIDBox
            // 
            this.DptColIDBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DptColIDBox.Location = new System.Drawing.Point(693, 140);
            this.DptColIDBox.Name = "DptColIDBox";
            this.DptColIDBox.ReadOnly = true;
            this.DptColIDBox.Size = new System.Drawing.Size(105, 13);
            this.DptColIDBox.TabIndex = 23;
            this.DptColIDBox.TabStop = false;
            this.DptColIDBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // BaseColIDBox
            // 
            this.BaseColIDBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.BaseColIDBox.Location = new System.Drawing.Point(429, 140);
            this.BaseColIDBox.Name = "BaseColIDBox";
            this.BaseColIDBox.ReadOnly = true;
            this.BaseColIDBox.Size = new System.Drawing.Size(105, 13);
            this.BaseColIDBox.TabIndex = 24;
            this.BaseColIDBox.TabStop = false;
            this.BaseColIDBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // ADTree
            // 
            this.ADTree.ImageIndex = 0;
            this.ADTree.ImageList = this.imageList2;
            this.ADTree.Location = new System.Drawing.Point(15, 176);
            this.ADTree.Name = "ADTree";
            this.ADTree.SelectedImageIndex = 0;
            this.ADTree.ShowLines = false;
            this.ADTree.Size = new System.Drawing.Size(254, 103);
            this.ADTree.TabIndex = 1;
            this.ADTree.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.ADTree_AfterCollapse);
            this.ADTree.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.ADTree_AfterExpand);
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
            // ADDptBox
            // 
            this.ADDptBox.BackColor = System.Drawing.SystemColors.Info;
            this.ADDptBox.CheckOnClick = true;
            this.ADDptBox.ColumnWidth = 83;
            this.ADDptBox.FormattingEnabled = true;
            this.ADDptBox.Location = new System.Drawing.Point(544, 185);
            this.ADDptBox.MultiColumn = true;
            this.ADDptBox.Name = "ADDptBox";
            this.ADDptBox.Size = new System.Drawing.Size(84, 94);
            this.ADDptBox.Sorted = true;
            this.ADDptBox.TabIndex = 27;
            // 
            // ADLabel
            // 
            this.ADLabel.AutoSize = true;
            this.ADLabel.Image = global::AuditSec.Properties.Resources.Container;
            this.ADLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ADLabel.Location = new System.Drawing.Point(12, 140);
            this.ADLabel.Name = "ADLabel";
            this.ADLabel.Size = new System.Drawing.Size(100, 13);
            this.ADLabel.TabIndex = 29;
            this.ADLabel.Text = "      Active Directory";
            // 
            // ForestBox
            // 
            this.ForestBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ForestBox.Location = new System.Drawing.Point(164, 140);
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
            this.ADCount.Location = new System.Drawing.Point(111, 266);
            this.ADCount.Name = "ADCount";
            this.ADCount.Size = new System.Drawing.Size(54, 13);
            this.ADCount.TabIndex = 31;
            this.ADCount.Text = "Loading...";
            // 
            // baseCount
            // 
            this.baseCount.AutoSize = true;
            this.baseCount.Location = new System.Drawing.Point(371, 266);
            this.baseCount.Name = "baseCount";
            this.baseCount.Size = new System.Drawing.Size(54, 13);
            this.baseCount.TabIndex = 32;
            this.baseCount.Text = "Loading...";
            // 
            // dptCount
            // 
            this.dptCount.AutoSize = true;
            this.dptCount.Location = new System.Drawing.Point(632, 266);
            this.dptCount.Name = "dptCount";
            this.dptCount.Size = new System.Drawing.Size(54, 13);
            this.dptCount.TabIndex = 33;
            this.dptCount.Text = "Loading...";
            // 
            // ADTreeRefresh
            // 
            this.ADTreeRefresh.BackColor = System.Drawing.Color.Transparent;
            this.ADTreeRefresh.FlatAppearance.BorderSize = 0;
            this.ADTreeRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ADTreeRefresh.ForeColor = System.Drawing.Color.Transparent;
            this.ADTreeRefresh.Image = global::AuditSec.Properties.Resources.refresh;
            this.ADTreeRefresh.Location = new System.Drawing.Point(108, 135);
            this.ADTreeRefresh.Name = "ADTreeRefresh";
            this.ADTreeRefresh.Size = new System.Drawing.Size(18, 18);
            this.ADTreeRefresh.TabIndex = 34;
            this.ADTreeRefresh.UseVisualStyleBackColor = false;
            this.ADTreeRefresh.Click += new System.EventHandler(this.ADTreeRefresh_Click);
            // 
            // SCCMTreeRefresh
            // 
            this.SCCMTreeRefresh.BackColor = System.Drawing.Color.Transparent;
            this.SCCMTreeRefresh.FlatAppearance.BorderSize = 0;
            this.SCCMTreeRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SCCMTreeRefresh.Image = global::AuditSec.Properties.Resources.refresh;
            this.SCCMTreeRefresh.Location = new System.Drawing.Point(349, 135);
            this.SCCMTreeRefresh.Name = "SCCMTreeRefresh";
            this.SCCMTreeRefresh.Size = new System.Drawing.Size(18, 18);
            this.SCCMTreeRefresh.TabIndex = 35;
            this.SCCMTreeRefresh.UseVisualStyleBackColor = false;
            this.SCCMTreeRefresh.Click += new System.EventHandler(this.SCCMTreeRefresh_Click);
            // 
            // DptTreeRefresh
            // 
            this.DptTreeRefresh.BackColor = System.Drawing.Color.Transparent;
            this.DptTreeRefresh.FlatAppearance.BorderSize = 0;
            this.DptTreeRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DptTreeRefresh.Image = global::AuditSec.Properties.Resources.refresh;
            this.DptTreeRefresh.Location = new System.Drawing.Point(638, 135);
            this.DptTreeRefresh.Name = "DptTreeRefresh";
            this.DptTreeRefresh.Size = new System.Drawing.Size(18, 18);
            this.DptTreeRefresh.TabIndex = 36;
            this.DptTreeRefresh.UseVisualStyleBackColor = false;
            this.DptTreeRefresh.Click += new System.EventHandler(this.DptTreeRefresh_Click);
            // 
            // TemplDptBox
            // 
            this.TemplDptBox.CheckOnClick = true;
            this.TemplDptBox.ColumnWidth = 83;
            this.TemplDptBox.FormattingEnabled = true;
            this.TemplDptBox.Location = new System.Drawing.Point(628, 185);
            this.TemplDptBox.MultiColumn = true;
            this.TemplDptBox.Name = "TemplDptBox";
            this.TemplDptBox.Size = new System.Drawing.Size(169, 94);
            this.TemplDptBox.Sorted = true;
            this.TemplDptBox.TabIndex = 37;
            this.TemplDptBox.MouseHover += new System.EventHandler(this.TemplDptBox_MouseHover);
            this.TemplDptBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TemplDptBox_MouseMove);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Image = global::AuditSec.Properties.Resources.Container;
            this.label10.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label10.Location = new System.Drawing.Point(541, 169);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(77, 13);
            this.label10.TabIndex = 38;
            this.label10.Text = "      No  Templ.";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Info;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label17);
            this.panel1.Controls.Add(this.ResetSchedule);
            this.panel1.Controls.Add(this.hourSpanBox);
            this.panel1.Controls.Add(this.startBox);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.SiteNameBox);
            this.panel1.Controls.Add(this.SiteCodeBox);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.PrepareSite);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Location = new System.Drawing.Point(15, 25);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(519, 111);
            this.panel1.TabIndex = 39;
            // 
            // ResetSchedule
            // 
            this.ResetSchedule.BackColor = System.Drawing.SystemColors.Control;
            this.ResetSchedule.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResetSchedule.Location = new System.Drawing.Point(441, 73);
            this.ResetSchedule.Name = "ResetSchedule";
            this.ResetSchedule.Size = new System.Drawing.Size(73, 34);
            this.ResetSchedule.TabIndex = 41;
            this.ResetSchedule.Text = "Reset Schedule";
            this.ResetSchedule.UseVisualStyleBackColor = false;
            this.ResetSchedule.Click += new System.EventHandler(this.ResetSchedule_Click);
            // 
            // hourSpanBox
            // 
            this.hourSpanBox.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Bold);
            this.hourSpanBox.Location = new System.Drawing.Point(408, 45);
            this.hourSpanBox.Mask = "00";
            this.hourSpanBox.Name = "hourSpanBox";
            this.hourSpanBox.PromptChar = '0';
            this.hourSpanBox.Size = new System.Drawing.Size(27, 20);
            this.hourSpanBox.TabIndex = 24;
            this.hourSpanBox.Text = " 6";
            this.hourSpanBox.ValidatingType = typeof(System.DateTime);
            // 
            // startBox
            // 
            this.startBox.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Bold);
            this.startBox.Location = new System.Drawing.Point(330, 45);
            this.startBox.Mask = "00:00";
            this.startBox.Name = "startBox";
            this.startBox.PromptChar = '0';
            this.startBox.Size = new System.Drawing.Size(54, 20);
            this.startBox.TabIndex = 23;
            this.startBox.Text = "14 1";
            this.startBox.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
            this.startBox.ValidatingType = typeof(System.DateTime);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.SystemColors.Info;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(390, 29);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "/    Every";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.SystemColors.Info;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(390, 47);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(13, 13);
            this.label9.TabIndex = 21;
            this.label9.Text = "/";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.SystemColors.Info;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(327, 29);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 13);
            this.label11.TabIndex = 20;
            this.label11.Text = "Start GMT";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.BackColor = System.Drawing.SystemColors.Info;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(261, 29);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(60, 13);
            this.label12.TabIndex = 19;
            this.label12.Text = "Schedule";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.SystemColors.Info;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label4.Location = new System.Drawing.Point(102, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "/    Name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.SystemColors.Info;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label3.Location = new System.Drawing.Point(102, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(12, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "/";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.Info;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(60, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Code";
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(15, 527);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(254, 26);
            this.ClearButton.TabIndex = 40;
            this.ClearButton.Text = "Clear Queue";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Info;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.label18);
            this.panel2.Controls.Add(this.dptHourSpanBox);
            this.panel2.Controls.Add(this.dptStartBox);
            this.panel2.Controls.Add(this.label13);
            this.panel2.Controls.Add(this.label14);
            this.panel2.Controls.Add(this.PrepareDpts);
            this.panel2.Controls.Add(this.label15);
            this.panel2.Controls.Add(this.label16);
            this.panel2.Controls.Add(this.label22);
            this.panel2.Location = new System.Drawing.Point(544, 25);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(254, 111);
            this.panel2.TabIndex = 40;
            // 
            // dptHourSpanBox
            // 
            this.dptHourSpanBox.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dptHourSpanBox.Location = new System.Drawing.Point(150, 43);
            this.dptHourSpanBox.Mask = "00";
            this.dptHourSpanBox.Name = "dptHourSpanBox";
            this.dptHourSpanBox.PromptChar = '0';
            this.dptHourSpanBox.Size = new System.Drawing.Size(27, 20);
            this.dptHourSpanBox.TabIndex = 24;
            this.dptHourSpanBox.Text = " 1";
            this.dptHourSpanBox.ValidatingType = typeof(System.DateTime);
            // 
            // dptStartBox
            // 
            this.dptStartBox.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dptStartBox.Location = new System.Drawing.Point(72, 43);
            this.dptStartBox.Mask = "00:00";
            this.dptStartBox.Name = "dptStartBox";
            this.dptStartBox.PromptChar = '0';
            this.dptStartBox.Size = new System.Drawing.Size(54, 20);
            this.dptStartBox.TabIndex = 23;
            this.dptStartBox.Text = "  3";
            this.dptStartBox.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
            this.dptStartBox.ValidatingType = typeof(System.DateTime);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.BackColor = System.Drawing.SystemColors.Info;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(132, 27);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(51, 13);
            this.label13.TabIndex = 22;
            this.label13.Text = "/    Every";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.BackColor = System.Drawing.SystemColors.Info;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(132, 45);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(12, 13);
            this.label14.TabIndex = 21;
            this.label14.Text = "/";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.BackColor = System.Drawing.SystemColors.Info;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(69, 27);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(56, 13);
            this.label15.TabIndex = 20;
            this.label15.Text = "Start GMT";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.BackColor = System.Drawing.SystemColors.Info;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(3, 27);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(52, 13);
            this.label16.TabIndex = 19;
            this.label16.Text = "Schedule";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.Location = new System.Drawing.Point(3, 3);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(112, 13);
            this.label22.TabIndex = 14;
            this.label22.Text = "Template departments";
            // 
            // ToolTip
            // 
            this.ToolTip.AutoPopDelay = 15000;
            this.ToolTip.BackColor = System.Drawing.SystemColors.Window;
            this.ToolTip.InitialDelay = 500;
            this.ToolTip.ReshowDelay = 100;
            this.ToolTip.ToolTipTitle = "Collection Properties";
            // 
            // Action
            // 
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Action.DefaultCellStyle = dataGridViewCellStyle1;
            this.Action.FillWeight = 60F;
            this.Action.HeaderText = "Action";
            this.Action.Name = "Action";
            this.Action.ReadOnly = true;
            this.Action.Width = 60;
            // 
            // ParentColName
            // 
            this.ParentColName.FillWeight = 80F;
            this.ParentColName.HeaderText = "Parent";
            this.ParentColName.Name = "ParentColName";
            this.ParentColName.ReadOnly = true;
            this.ParentColName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ParentColName.Width = 80;
            // 
            // ParentColID
            // 
            this.ParentColID.FillWeight = 75F;
            this.ParentColID.HeaderText = "ParentID";
            this.ParentColID.Name = "ParentColID";
            this.ParentColID.ReadOnly = true;
            this.ParentColID.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ParentColID.Width = 75;
            // 
            // Name_
            // 
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name_.DefaultCellStyle = dataGridViewCellStyle2;
            this.Name_.FillWeight = 120F;
            this.Name_.HeaderText = "Name";
            this.Name_.Name = "Name_";
            this.Name_.ReadOnly = true;
            this.Name_.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Name_.Width = 120;
            // 
            // Comment
            // 
            this.Comment.FillWeight = 156F;
            this.Comment.HeaderText = "Comment";
            this.Comment.Name = "Comment";
            this.Comment.ReadOnly = true;
            this.Comment.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Comment.Width = 156;
            // 
            // Schedule
            // 
            this.Schedule.FillWeight = 120F;
            this.Schedule.HeaderText = "Schedule";
            this.Schedule.Name = "Schedule";
            this.Schedule.ReadOnly = true;
            this.Schedule.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Schedule.Width = 120;
            // 
            // QueryRule
            // 
            this.QueryRule.FillWeight = 75F;
            this.QueryRule.HeaderText = "QueryRule";
            this.QueryRule.Name = "QueryRule";
            this.QueryRule.ReadOnly = true;
            this.QueryRule.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.QueryRule.Width = 75;
            // 
            // CollectionID
            // 
            this.CollectionID.FillWeight = 75F;
            this.CollectionID.HeaderText = "CollectionID";
            this.CollectionID.Name = "CollectionID";
            this.CollectionID.ReadOnly = true;
            this.CollectionID.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.CollectionID.Width = 75;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.BackColor = System.Drawing.SystemColors.Info;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(438, 47);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(14, 13);
            this.label17.TabIndex = 42;
            this.label17.Text = "h";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.BackColor = System.Drawing.SystemColors.Info;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(183, 45);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(13, 13);
            this.label18.TabIndex = 25;
            this.label18.Text = "h";
            // 
            // SCCMCollectionDesigner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(815, 555);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.DomainsBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.CheckConsistency);
            this.Controls.Add(this.ADDptBox);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.TemplDptBox);
            this.Controls.Add(this.DptTreeRefresh);
            this.Controls.Add(this.SCCMTreeRefresh);
            this.Controls.Add(this.ADTreeRefresh);
            this.Controls.Add(this.DptTree);
            this.Controls.Add(this.SCCMTree);
            this.Controls.Add(this.ADTree);
            this.Controls.Add(this.dptCount);
            this.Controls.Add(this.baseCount);
            this.Controls.Add(this.ADCount);
            this.Controls.Add(this.ForestBox);
            this.Controls.Add(this.ADLabel);
            this.Controls.Add(this.BaseColIDBox);
            this.Controls.Add(this.DptColIDBox);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.ProceedButton);
            this.Controls.Add(this.Queue);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.DptColBox);
            this.Controls.Add(this.DptLabel);
            this.Controls.Add(this.BaseColBox);
            this.Controls.Add(this.SCCMLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SCCMCollectionDesigner";
            this.Text = "SCCM Collection Designer";
            this.Shown += new System.EventHandler(this.SCCMCollectionDesigner_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.Queue)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox SiteCodeBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TreeView SCCMTree;
        private System.Windows.Forms.Label SCCMLabel;
        private System.Windows.Forms.TextBox BaseColBox;
        private System.Windows.Forms.CheckedListBox DomainsBox;
        private System.Windows.Forms.Label DptLabel;
        private System.Windows.Forms.TreeView DptTree;
        private System.Windows.Forms.TextBox DptColBox;
        private System.Windows.Forms.Button PrepareSite;
        private System.Windows.Forms.Button PrepareDpts;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox SiteNameBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button CheckConsistency;
        private System.Windows.Forms.DataGridView Queue;
        private System.Windows.Forms.Button ProceedButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.TextBox DptColIDBox;
        private System.Windows.Forms.TextBox BaseColIDBox;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TreeView ADTree;
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.CheckedListBox ADDptBox;
        private System.Windows.Forms.Label ADLabel;
        private System.Windows.Forms.TextBox ForestBox;
        private System.Windows.Forms.Label ADCount;
        private System.Windows.Forms.Label baseCount;
        private System.Windows.Forms.Label dptCount;
        private System.Windows.Forms.Button ADTreeRefresh;
        private System.Windows.Forms.Button SCCMTreeRefresh;
        private System.Windows.Forms.Button DptTreeRefresh;
        private System.Windows.Forms.CheckedListBox TemplDptBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Action_;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParentColName_;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParentColID_;
        private System.Windows.Forms.DataGridViewTextBoxColumn Comment_;
        private System.Windows.Forms.DataGridViewTextBoxColumn Schedule_;
        private System.Windows.Forms.DataGridViewTextBoxColumn QueryRule_;
        private System.Windows.Forms.DataGridViewTextBoxColumn CollectionID_;
        private System.Windows.Forms.MaskedTextBox startBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.MaskedTextBox hourSpanBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.MaskedTextBox dptHourSpanBox;
        private System.Windows.Forms.MaskedTextBox dptStartBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.Button ResetSchedule;
        private System.Windows.Forms.DataGridViewTextBoxColumn Action;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParentColName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParentColID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Name_;
        private System.Windows.Forms.DataGridViewTextBoxColumn Comment;
        private System.Windows.Forms.DataGridViewTextBoxColumn Schedule;
        private System.Windows.Forms.DataGridViewTextBoxColumn QueryRule;
        private System.Windows.Forms.DataGridViewTextBoxColumn CollectionID;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
    }
}