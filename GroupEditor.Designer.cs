namespace AuditSec
{
    partial class GroupEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GroupEditor));
            this.ADTree = new System.Windows.Forms.TreeView();
            this.searchIcons = new System.Windows.Forms.ImageList(this.components);
            this.ADLabel = new System.Windows.Forms.Label();
            this.ForestBox = new System.Windows.Forms.TextBox();
            this.ADCount = new System.Windows.Forms.Label();
            this.ADTreeRefresh = new System.Windows.Forms.Button();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.cancelButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.addButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.groupsLabel = new System.Windows.Forms.Label();
            this.membersBox = new System.Windows.Forms.ListBox();
            this.membersLabel = new System.Windows.Forms.Label();
            this.membersPanel = new System.Windows.Forms.Panel();
            this.groupBox = new System.Windows.Forms.TextBox();
            this.membersCount = new System.Windows.Forms.TextBox();
            this.groupsBox = new System.Windows.Forms.ListBox();
            this.searchFilterBox = new System.Windows.Forms.TextBox();
            this.searchButton = new System.Windows.Forms.Button();
            this.searchLabel = new System.Windows.Forms.Label();
            this.searchRootBox = new System.Windows.Forms.TextBox();
            this.searchPanel = new System.Windows.Forms.Panel();
            this.searchScopeBox = new System.Windows.Forms.CheckBox();
            this.usersPanel = new System.Windows.Forms.Panel();
            this.progressLabel = new System.Windows.Forms.Label();
            this.usersLabel = new System.Windows.Forms.Label();
            this.AddUserBox = new System.Windows.Forms.TextBox();
            this.searchTimer = new System.Windows.Forms.Timer(this.components);
            this.progressWorker = new System.ComponentModel.BackgroundWorker();
            this.membersPanel.SuspendLayout();
            this.searchPanel.SuspendLayout();
            this.usersPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ADTree
            // 
            this.ADTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ADTree.ImageIndex = 0;
            this.ADTree.ImageList = this.searchIcons;
            this.ADTree.Location = new System.Drawing.Point(747, 21);
            this.ADTree.Name = "ADTree";
            this.ADTree.SelectedImageIndex = 0;
            this.ADTree.ShowLines = false;
            this.ADTree.Size = new System.Drawing.Size(254, 238);
            this.ADTree.TabIndex = 1;
            this.ADTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ADTree_AfterSelect);
            // 
            // searchIcons
            // 
            this.searchIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("searchIcons.ImageStream")));
            this.searchIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.searchIcons.Images.SetKeyName(0, "Container.png");
            this.searchIcons.Images.SetKeyName(1, "Container2.png");
            this.searchIcons.Images.SetKeyName(2, "Domain.png");
            this.searchIcons.Images.SetKeyName(3, "Domain2.png");
            // 
            // ADLabel
            // 
            this.ADLabel.AutoSize = true;
            this.ADLabel.Image = global::AuditSec.Properties.Resources.Container;
            this.ADLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ADLabel.Location = new System.Drawing.Point(747, 5);
            this.ADLabel.Name = "ADLabel";
            this.ADLabel.Size = new System.Drawing.Size(100, 13);
            this.ADLabel.TabIndex = 29;
            this.ADLabel.Text = "      Active Directory";
            // 
            // ForestBox
            // 
            this.ForestBox.BackColor = System.Drawing.SystemColors.Window;
            this.ForestBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ForestBox.Location = new System.Drawing.Point(896, 5);
            this.ForestBox.Name = "ForestBox";
            this.ForestBox.ReadOnly = true;
            this.ForestBox.Size = new System.Drawing.Size(105, 13);
            this.ForestBox.TabIndex = 30;
            this.ForestBox.TabStop = false;
            this.ForestBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ForestBox.Visible = false;
            // 
            // ADCount
            // 
            this.ADCount.AutoSize = true;
            this.ADCount.Location = new System.Drawing.Point(747, 242);
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
            this.ADTreeRefresh.Location = new System.Drawing.Point(849, 2);
            this.ADTreeRefresh.Name = "ADTreeRefresh";
            this.ADTreeRefresh.Size = new System.Drawing.Size(18, 18);
            this.ADTreeRefresh.TabIndex = 34;
            this.ADTreeRefresh.UseVisualStyleBackColor = false;
            this.ADTreeRefresh.Visible = false;
            this.ADTreeRefresh.Click += new System.EventHandler(this.ADTreeRefresh_Click);
            // 
            // ToolTip
            // 
            this.ToolTip.AutoPopDelay = 15000;
            this.ToolTip.BackColor = System.Drawing.SystemColors.Window;
            this.ToolTip.InitialDelay = 500;
            this.ToolTip.ReshowDelay = 100;
            this.ToolTip.ToolTipTitle = "Group Editor";
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.SystemColors.Control;
            this.cancelButton.Location = new System.Drawing.Point(598, 235);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(79, 24);
            this.cancelButton.TabIndex = 21;
            this.cancelButton.Text = "Quit";
            this.ToolTip.SetToolTip(this.cancelButton, "Discard changes");
            this.cancelButton.UseVisualStyleBackColor = false;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // removeButton
            // 
            this.removeButton.BackColor = System.Drawing.SystemColors.Control;
            this.removeButton.Location = new System.Drawing.Point(598, 175);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(79, 24);
            this.removeButton.TabIndex = 53;
            this.removeButton.Text = "< <";
            this.ToolTip.SetToolTip(this.removeButton, "Remove members");
            this.removeButton.UseVisualStyleBackColor = false;
            this.removeButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // addButton
            // 
            this.addButton.BackColor = System.Drawing.SystemColors.Control;
            this.addButton.Location = new System.Drawing.Point(598, 145);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(79, 24);
            this.addButton.TabIndex = 52;
            this.addButton.Text = "> >";
            this.ToolTip.SetToolTip(this.addButton, "Add members");
            this.addButton.UseVisualStyleBackColor = false;
            this.addButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // applyButton
            // 
            this.applyButton.BackColor = System.Drawing.SystemColors.Control;
            this.applyButton.Location = new System.Drawing.Point(598, 205);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(79, 24);
            this.applyButton.TabIndex = 50;
            this.applyButton.Text = "Apply";
            this.ToolTip.SetToolTip(this.applyButton, "Commit changes");
            this.applyButton.UseVisualStyleBackColor = false;
            this.applyButton.Visible = false;
            this.applyButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // groupsLabel
            // 
            this.groupsLabel.AutoSize = true;
            this.groupsLabel.Image = global::AuditSec.Properties.Resources.Dpt18;
            this.groupsLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.groupsLabel.Location = new System.Drawing.Point(3, 51);
            this.groupsLabel.Name = "groupsLabel";
            this.groupsLabel.Size = new System.Drawing.Size(59, 13);
            this.groupsLabel.TabIndex = 39;
            this.groupsLabel.Text = "      Groups";
            this.groupsLabel.Visible = false;
            // 
            // membersBox
            // 
            this.membersBox.BackColor = System.Drawing.SystemColors.Control;
            this.membersBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.membersBox.ColumnWidth = 163;
            this.membersBox.FormattingEnabled = true;
            this.membersBox.Location = new System.Drawing.Point(0, 28);
            this.membersBox.MultiColumn = true;
            this.membersBox.Name = "membersBox";
            this.membersBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.membersBox.Size = new System.Drawing.Size(326, 234);
            this.membersBox.Sorted = true;
            this.membersBox.TabIndex = 41;
            // 
            // membersLabel
            // 
            this.membersLabel.AutoSize = true;
            this.membersLabel.BackColor = System.Drawing.SystemColors.Control;
            this.membersLabel.Image = global::AuditSec.Properties.Resources.Dpt18;
            this.membersLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.membersLabel.Location = new System.Drawing.Point(-1, 5);
            this.membersLabel.Name = "membersLabel";
            this.membersLabel.Size = new System.Drawing.Size(68, 13);
            this.membersLabel.TabIndex = 42;
            this.membersLabel.Text = "      Members";
            // 
            // membersPanel
            // 
            this.membersPanel.BackColor = System.Drawing.SystemColors.Control;
            this.membersPanel.Controls.Add(this.groupBox);
            this.membersPanel.Controls.Add(this.membersBox);
            this.membersPanel.Controls.Add(this.membersCount);
            this.membersPanel.Controls.Add(this.membersLabel);
            this.membersPanel.Location = new System.Drawing.Point(683, 0);
            this.membersPanel.Name = "membersPanel";
            this.membersPanel.Size = new System.Drawing.Size(326, 262);
            this.membersPanel.TabIndex = 43;
            this.membersPanel.Visible = false;
            // 
            // groupBox
            // 
            this.groupBox.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox.Location = new System.Drawing.Point(67, 5);
            this.groupBox.Name = "groupBox";
            this.groupBox.ReadOnly = true;
            this.groupBox.Size = new System.Drawing.Size(256, 13);
            this.groupBox.TabIndex = 56;
            // 
            // membersCount
            // 
            this.membersCount.BackColor = System.Drawing.SystemColors.Control;
            this.membersCount.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.membersCount.Location = new System.Drawing.Point(105, 3);
            this.membersCount.Name = "membersCount";
            this.membersCount.ReadOnly = true;
            this.membersCount.Size = new System.Drawing.Size(105, 13);
            this.membersCount.TabIndex = 46;
            this.membersCount.TabStop = false;
            this.membersCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // groupsBox
            // 
            this.groupsBox.BackColor = System.Drawing.SystemColors.Window;
            this.groupsBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.groupsBox.ColumnWidth = 163;
            this.groupsBox.FormattingEnabled = true;
            this.groupsBox.Location = new System.Drawing.Point(0, 67);
            this.groupsBox.MultiColumn = true;
            this.groupsBox.Name = "groupsBox";
            this.groupsBox.Size = new System.Drawing.Size(741, 195);
            this.groupsBox.Sorted = true;
            this.groupsBox.TabIndex = 50;
            this.groupsBox.SelectedIndexChanged += new System.EventHandler(this.groupsBox_SelectedIndexChanged);
            this.groupsBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.groupsBox_MouseMove);
            // 
            // searchFilterBox
            // 
            this.searchFilterBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchFilterBox.Location = new System.Drawing.Point(6, 21);
            this.searchFilterBox.Name = "searchFilterBox";
            this.searchFilterBox.Size = new System.Drawing.Size(318, 20);
            this.searchFilterBox.TabIndex = 51;
            this.searchFilterBox.Text = "*";
            this.searchFilterBox.TextChanged += new System.EventHandler(this.searchFilterBox_TextChanged);
            this.searchFilterBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.searchFilterBox_KeyPress);
            // 
            // searchButton
            // 
            this.searchButton.BackColor = System.Drawing.Color.Transparent;
            this.searchButton.FlatAppearance.BorderSize = 0;
            this.searchButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.searchButton.ForeColor = System.Drawing.Color.Transparent;
            this.searchButton.Image = global::AuditSec.Properties.Resources.Glass;
            this.searchButton.Location = new System.Drawing.Point(6, 2);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(18, 18);
            this.searchButton.TabIndex = 52;
            this.searchButton.UseVisualStyleBackColor = false;
            this.searchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // searchLabel
            // 
            this.searchLabel.AutoSize = true;
            this.searchLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchLabel.Image = global::AuditSec.Properties.Resources.Glass;
            this.searchLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.searchLabel.Location = new System.Drawing.Point(3, 5);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(65, 13);
            this.searchLabel.TabIndex = 53;
            this.searchLabel.Text = "      Group";
            // 
            // searchRootBox
            // 
            this.searchRootBox.BackColor = System.Drawing.SystemColors.Window;
            this.searchRootBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.searchRootBox.Location = new System.Drawing.Point(75, 5);
            this.searchRootBox.Name = "searchRootBox";
            this.searchRootBox.ReadOnly = true;
            this.searchRootBox.Size = new System.Drawing.Size(335, 13);
            this.searchRootBox.TabIndex = 54;
            this.searchRootBox.Text = "everywhere";
            this.searchRootBox.Visible = false;
            // 
            // searchPanel
            // 
            this.searchPanel.BackColor = System.Drawing.SystemColors.Window;
            this.searchPanel.Controls.Add(this.ADTreeRefresh);
            this.searchPanel.Controls.Add(this.ADTree);
            this.searchPanel.Controls.Add(this.ADCount);
            this.searchPanel.Controls.Add(this.ForestBox);
            this.searchPanel.Controls.Add(this.ADLabel);
            this.searchPanel.Controls.Add(this.searchButton);
            this.searchPanel.Controls.Add(this.searchScopeBox);
            this.searchPanel.Controls.Add(this.searchRootBox);
            this.searchPanel.Controls.Add(this.groupsBox);
            this.searchPanel.Controls.Add(this.searchLabel);
            this.searchPanel.Controls.Add(this.groupsLabel);
            this.searchPanel.Controls.Add(this.searchFilterBox);
            this.searchPanel.Location = new System.Drawing.Point(0, 0);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Size = new System.Drawing.Size(1009, 262);
            this.searchPanel.TabIndex = 55;
            // 
            // searchScopeBox
            // 
            this.searchScopeBox.AutoSize = true;
            this.searchScopeBox.Location = new System.Drawing.Point(349, 23);
            this.searchScopeBox.Name = "searchScopeBox";
            this.searchScopeBox.Size = new System.Drawing.Size(61, 17);
            this.searchScopeBox.TabIndex = 55;
            this.searchScopeBox.Text = "subtree";
            this.searchScopeBox.UseVisualStyleBackColor = true;
            this.searchScopeBox.Visible = false;
            // 
            // usersPanel
            // 
            this.usersPanel.BackColor = System.Drawing.SystemColors.Control;
            this.usersPanel.Controls.Add(this.progressLabel);
            this.usersPanel.Controls.Add(this.cancelButton);
            this.usersPanel.Controls.Add(this.usersLabel);
            this.usersPanel.Controls.Add(this.removeButton);
            this.usersPanel.Controls.Add(this.addButton);
            this.usersPanel.Controls.Add(this.AddUserBox);
            this.usersPanel.Controls.Add(this.applyButton);
            this.usersPanel.Location = new System.Drawing.Point(0, 0);
            this.usersPanel.Name = "usersPanel";
            this.usersPanel.Size = new System.Drawing.Size(685, 262);
            this.usersPanel.TabIndex = 56;
            this.usersPanel.Visible = false;
            // 
            // progressLabel
            // 
            this.progressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.progressLabel.Location = new System.Drawing.Point(59, 3);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(531, 18);
            this.progressLabel.TabIndex = 58;
            // 
            // usersLabel
            // 
            this.usersLabel.AutoSize = true;
            this.usersLabel.Image = global::AuditSec.Properties.Resources.Dpt18;
            this.usersLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.usersLabel.Location = new System.Drawing.Point(1, 5);
            this.usersLabel.Name = "usersLabel";
            this.usersLabel.Size = new System.Drawing.Size(52, 13);
            this.usersLabel.TabIndex = 54;
            this.usersLabel.Text = "      Users";
            // 
            // AddUserBox
            // 
            this.AddUserBox.Location = new System.Drawing.Point(4, 26);
            this.AddUserBox.Multiline = true;
            this.AddUserBox.Name = "AddUserBox";
            this.AddUserBox.Size = new System.Drawing.Size(586, 233);
            this.AddUserBox.TabIndex = 51;
            // 
            // searchTimer
            // 
            this.searchTimer.Enabled = true;
            this.searchTimer.Interval = 500;
            this.searchTimer.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // progressWorker
            // 
            this.progressWorker.WorkerReportsProgress = true;
            this.progressWorker.WorkerSupportsCancellation = true;
            this.progressWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.progressWorker_DoWork);
            this.progressWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.progressWorker_ProgressChanged);
            // 
            // GroupEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1008, 262);
            this.Controls.Add(this.membersPanel);
            this.Controls.Add(this.usersPanel);
            this.Controls.Add(this.searchPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GroupEditor";
            this.Text = "AD Group Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GroupEditor_FormClosing);
            this.Shown += new System.EventHandler(this.MachineChooser_Shown);
            this.membersPanel.ResumeLayout(false);
            this.membersPanel.PerformLayout();
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            this.usersPanel.ResumeLayout(false);
            this.usersPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView ADTree;
        private System.Windows.Forms.ImageList searchIcons;
        private System.Windows.Forms.Label ADLabel;
        private System.Windows.Forms.TextBox ForestBox;
        private System.Windows.Forms.Label ADCount;
        private System.Windows.Forms.Button ADTreeRefresh;
        private System.Windows.Forms.DataGridViewTextBoxColumn Action_;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParentColName_;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParentColID_;
        private System.Windows.Forms.DataGridViewTextBoxColumn Comment_;
        private System.Windows.Forms.DataGridViewTextBoxColumn Schedule_;
        private System.Windows.Forms.DataGridViewTextBoxColumn QueryRule_;
        private System.Windows.Forms.DataGridViewTextBoxColumn CollectionID_;
        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.Label groupsLabel;
        private System.Windows.Forms.ListBox membersBox;
        private System.Windows.Forms.Label membersLabel;
        private System.Windows.Forms.Panel membersPanel;
        private System.Windows.Forms.TextBox membersCount;
        private System.Windows.Forms.ListBox groupsBox;
        private System.Windows.Forms.TextBox searchFilterBox;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.TextBox searchRootBox;
        private System.Windows.Forms.Panel searchPanel;
        private System.Windows.Forms.CheckBox searchScopeBox;
        private System.Windows.Forms.TextBox groupBox;
        private System.Windows.Forms.Panel usersPanel;
        private System.Windows.Forms.Label usersLabel;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.TextBox AddUserBox;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Timer searchTimer;
        private System.ComponentModel.BackgroundWorker progressWorker;
        private System.Windows.Forms.Label progressLabel;
    }
}