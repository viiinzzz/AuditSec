
using System;
namespace AuditSec
{
    partial class HotApps
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HotApps));
            this.adminsBox = new System.Windows.Forms.CheckedListBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.adminsLabel = new System.Windows.Forms.Label();
            this.machineBigBox = new System.Windows.Forms.TextBox();
            this.wmiGroup = new System.Windows.Forms.GroupBox();
            this.batteryButton = new System.Windows.Forms.Button();
            this.freeBox = new System.Windows.Forms.TextBox();
            this.chassisBox = new System.Windows.Forms.TextBox();
            this.makermodelBox = new System.Windows.Forms.TextBox();
            this.loggedinPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.loggedinLabel = new System.Windows.Forms.Label();
            this.loggedinBox = new System.Windows.Forms.TextBox();
            this.instAppsTable = new System.Windows.Forms.DataGridView();
            this.appName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.appVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.appVendor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.appInstallDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Desired = new System.Windows.Forms.DataGridViewImageColumn();
            this.instAppsLabel = new System.Windows.Forms.Label();
            this.instAppsWorker = new System.ComponentModel.BackgroundWorker();
            this.wmiWorker = new System.ComponentModel.BackgroundWorker();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.hotappsBox = new System.Windows.Forms.CheckBox();
            this.viewupdBox = new System.Windows.Forms.CheckBox();
            this.includeBox = new System.Windows.Forms.TextBox();
            this.ieLabel = new System.Windows.Forms.Label();
            this.firefoxLabel = new System.Windows.Forms.Label();
            this.javaLabel = new System.Windows.Forms.Label();
            this.acrobatLabel = new System.Windows.Forms.Label();
            this.excludeBox = new System.Windows.Forms.TextBox();
            this.refreshButton = new System.Windows.Forms.Button();
            this.flashLabel = new System.Windows.Forms.Label();
            this.ssoLabel = new System.Windows.Forms.Label();
            this.vpnLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel2.SuspendLayout();
            this.wmiGroup.SuspendLayout();
            this.loggedinPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.instAppsTable)).BeginInit();
            this.SuspendLayout();
            // 
            // adminsBox
            // 
            this.adminsBox.BackColor = System.Drawing.Color.White;
            this.adminsBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.adminsBox.CheckOnClick = true;
            this.adminsBox.Enabled = false;
            this.adminsBox.FormattingEnabled = true;
            this.adminsBox.Location = new System.Drawing.Point(53, 3);
            this.adminsBox.Name = "adminsBox";
            this.adminsBox.Size = new System.Drawing.Size(149, 60);
            this.adminsBox.TabIndex = 26;
            this.adminsBox.TabStop = false;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.adminsLabel);
            this.flowLayoutPanel2.Controls.Add(this.adminsBox);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(2, 106);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(207, 70);
            this.flowLayoutPanel2.TabIndex = 40;
            // 
            // adminsLabel
            // 
            this.adminsLabel.AutoSize = true;
            this.adminsLabel.Location = new System.Drawing.Point(3, 0);
            this.adminsLabel.Name = "adminsLabel";
            this.adminsLabel.Size = new System.Drawing.Size(44, 13);
            this.adminsLabel.TabIndex = 41;
            this.adminsLabel.Text = "Admins ";
            this.adminsLabel.Click += new System.EventHandler(this.adminsLabel_Click);
            // 
            // machineBigBox
            // 
            this.machineBigBox.BackColor = System.Drawing.Color.White;
            this.machineBigBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.machineBigBox.Enabled = false;
            this.machineBigBox.Font = new System.Drawing.Font("Lucida Console", 32.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.machineBigBox.Location = new System.Drawing.Point(5, 7);
            this.machineBigBox.Name = "machineBigBox";
            this.machineBigBox.ReadOnly = true;
            this.machineBigBox.Size = new System.Drawing.Size(302, 43);
            this.machineBigBox.TabIndex = 41;
            // 
            // wmiGroup
            // 
            this.wmiGroup.Controls.Add(this.batteryButton);
            this.wmiGroup.Controls.Add(this.freeBox);
            this.wmiGroup.Controls.Add(this.chassisBox);
            this.wmiGroup.Controls.Add(this.makermodelBox);
            this.wmiGroup.Controls.Add(this.loggedinPanel);
            this.wmiGroup.Controls.Add(this.flowLayoutPanel2);
            this.wmiGroup.Location = new System.Drawing.Point(560, -3);
            this.wmiGroup.Name = "wmiGroup";
            this.wmiGroup.Size = new System.Drawing.Size(211, 181);
            this.wmiGroup.TabIndex = 42;
            this.wmiGroup.TabStop = false;
            // 
            // batteryButton
            // 
            this.batteryButton.FlatAppearance.BorderSize = 0;
            this.batteryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.batteryButton.Image = global::AuditSec.Properties.Resources.batt;
            this.batteryButton.Location = new System.Drawing.Point(193, 10);
            this.batteryButton.Name = "batteryButton";
            this.batteryButton.Size = new System.Drawing.Size(16, 16);
            this.batteryButton.TabIndex = 65;
            this.batteryButton.TabStop = false;
            this.batteryButton.UseVisualStyleBackColor = true;
            this.batteryButton.Click += new System.EventHandler(this.batteryButton_Click_1);
            // 
            // freeBox
            // 
            this.freeBox.BackColor = System.Drawing.Color.White;
            this.freeBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.freeBox.Enabled = false;
            this.freeBox.Location = new System.Drawing.Point(2, 52);
            this.freeBox.Multiline = true;
            this.freeBox.Name = "freeBox";
            this.freeBox.ReadOnly = true;
            this.freeBox.Size = new System.Drawing.Size(203, 30);
            this.freeBox.TabIndex = 2;
            // 
            // chassisBox
            // 
            this.chassisBox.BackColor = System.Drawing.Color.White;
            this.chassisBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chassisBox.Enabled = false;
            this.chassisBox.Location = new System.Drawing.Point(2, 35);
            this.chassisBox.Name = "chassisBox";
            this.chassisBox.ReadOnly = true;
            this.chassisBox.Size = new System.Drawing.Size(203, 13);
            this.chassisBox.TabIndex = 1;
            // 
            // makermodelBox
            // 
            this.makermodelBox.BackColor = System.Drawing.Color.White;
            this.makermodelBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.makermodelBox.Enabled = false;
            this.makermodelBox.Location = new System.Drawing.Point(2, 15);
            this.makermodelBox.Name = "makermodelBox";
            this.makermodelBox.ReadOnly = true;
            this.makermodelBox.Size = new System.Drawing.Size(203, 13);
            this.makermodelBox.TabIndex = 0;
            // 
            // loggedinPanel
            // 
            this.loggedinPanel.Controls.Add(this.loggedinLabel);
            this.loggedinPanel.Controls.Add(this.loggedinBox);
            this.loggedinPanel.Location = new System.Drawing.Point(2, 82);
            this.loggedinPanel.Name = "loggedinPanel";
            this.loggedinPanel.Size = new System.Drawing.Size(207, 24);
            this.loggedinPanel.TabIndex = 52;
            // 
            // loggedinLabel
            // 
            this.loggedinLabel.AutoSize = true;
            this.loggedinLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loggedinLabel.Location = new System.Drawing.Point(3, 8);
            this.loggedinLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.loggedinLabel.Name = "loggedinLabel";
            this.loggedinLabel.Size = new System.Drawing.Size(49, 13);
            this.loggedinLabel.TabIndex = 3;
            this.loggedinLabel.Text = "Logged";
            // 
            // loggedinBox
            // 
            this.loggedinBox.BackColor = System.Drawing.Color.White;
            this.loggedinBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.loggedinBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loggedinBox.Location = new System.Drawing.Point(58, 3);
            this.loggedinBox.MaxLength = 30;
            this.loggedinBox.Name = "loggedinBox";
            this.loggedinBox.ReadOnly = true;
            this.loggedinBox.Size = new System.Drawing.Size(142, 19);
            this.loggedinBox.TabIndex = 35;
            this.loggedinBox.TabStop = false;
            // 
            // instAppsTable
            // 
            this.instAppsTable.AllowUserToAddRows = false;
            this.instAppsTable.AllowUserToDeleteRows = false;
            this.instAppsTable.AllowUserToOrderColumns = true;
            this.instAppsTable.BackgroundColor = System.Drawing.Color.White;
            this.instAppsTable.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.instAppsTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.instAppsTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.appName,
            this.appVersion,
            this.appVendor,
            this.appInstallDate,
            this.Desired});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.instAppsTable.DefaultCellStyle = dataGridViewCellStyle2;
            this.instAppsTable.Location = new System.Drawing.Point(2, 203);
            this.instAppsTable.Name = "instAppsTable";
            this.instAppsTable.ReadOnly = true;
            this.instAppsTable.RowHeadersVisible = false;
            this.instAppsTable.Size = new System.Drawing.Size(769, 199);
            this.instAppsTable.TabIndex = 43;
            this.instAppsTable.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.instAppsTable_CellFormatting);
            // 
            // appName
            // 
            this.appName.FillWeight = 250F;
            this.appName.HeaderText = "Name";
            this.appName.Name = "appName";
            this.appName.ReadOnly = true;
            this.appName.Width = 300;
            // 
            // appVersion
            // 
            this.appVersion.HeaderText = "Version";
            this.appVersion.Name = "appVersion";
            this.appVersion.ReadOnly = true;
            // 
            // appVendor
            // 
            this.appVendor.FillWeight = 200F;
            this.appVendor.HeaderText = "Vendor";
            this.appVendor.Name = "appVendor";
            this.appVendor.ReadOnly = true;
            this.appVendor.Width = 200;
            // 
            // appInstallDate
            // 
            dataGridViewCellStyle1.Format = "yyyy-MMM-dd";
            dataGridViewCellStyle1.NullValue = null;
            this.appInstallDate.DefaultCellStyle = dataGridViewCellStyle1;
            this.appInstallDate.HeaderText = "Installed";
            this.appInstallDate.Name = "appInstallDate";
            this.appInstallDate.ReadOnly = true;
            // 
            // Desired
            // 
            this.Desired.FillWeight = 50F;
            this.Desired.HeaderText = "Desired";
            this.Desired.Name = "Desired";
            this.Desired.ReadOnly = true;
            this.Desired.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Desired.Width = 50;
            // 
            // instAppsLabel
            // 
            this.instAppsLabel.AutoSize = true;
            this.instAppsLabel.Location = new System.Drawing.Point(2, 187);
            this.instAppsLabel.Name = "instAppsLabel";
            this.instAppsLabel.Size = new System.Drawing.Size(106, 13);
            this.instAppsLabel.TabIndex = 44;
            this.instAppsLabel.Text = "Installed Applications";
            // 
            // instAppsWorker
            // 
            this.instAppsWorker.WorkerReportsProgress = true;
            this.instAppsWorker.WorkerSupportsCancellation = true;
            this.instAppsWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.instAppsWorker_DoWork);
            this.instAppsWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.instAppsWorker_ProgressChanged);
            this.instAppsWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.instAppsWorker_RunWorkerCompleted);
            // 
            // wmiWorker
            // 
            this.wmiWorker.WorkerReportsProgress = true;
            this.wmiWorker.WorkerSupportsCancellation = true;
            this.wmiWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.wmiWorker_DoWork);
            this.wmiWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.wmiWorker_ProgressChanged);
            this.wmiWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.wmiWorker_RunWorkerCompleted);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 1000;
            this.toolTip1.ReshowDelay = 500;
            this.toolTip1.ShowAlways = true;
            // 
            // hotappsBox
            // 
            this.hotappsBox.AutoSize = true;
            this.hotappsBox.Checked = true;
            this.hotappsBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.hotappsBox.Location = new System.Drawing.Point(2, 408);
            this.hotappsBox.Name = "hotappsBox";
            this.hotappsBox.Size = new System.Drawing.Size(92, 17);
            this.hotappsBox.TabIndex = 58;
            this.hotappsBox.Text = "Hot Apps only";
            this.toolTip1.SetToolTip(this.hotappsBox, "Hoy Apps:");
            this.hotappsBox.UseVisualStyleBackColor = true;
            this.hotappsBox.CheckedChanged += new System.EventHandler(this.hotappsBox_CheckedChanged);
            // 
            // viewupdBox
            // 
            this.viewupdBox.AutoSize = true;
            this.viewupdBox.Location = new System.Drawing.Point(390, 408);
            this.viewupdBox.Name = "viewupdBox";
            this.viewupdBox.Size = new System.Drawing.Size(92, 17);
            this.viewupdBox.TabIndex = 57;
            this.viewupdBox.Text = "View Updates";
            this.viewupdBox.UseVisualStyleBackColor = true;
            this.viewupdBox.Visible = false;
            this.viewupdBox.CheckedChanged += new System.EventHandler(this.viewupdBox_CheckedChanged);
            // 
            // includeBox
            // 
            this.includeBox.BackColor = System.Drawing.Color.White;
            this.includeBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.includeBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.includeBox.ForeColor = System.Drawing.Color.Green;
            this.includeBox.Location = new System.Drawing.Point(2, 431);
            this.includeBox.MaxLength = 1000;
            this.includeBox.Multiline = true;
            this.includeBox.Name = "includeBox";
            this.includeBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.includeBox.Size = new System.Drawing.Size(382, 41);
            this.includeBox.TabIndex = 59;
            this.includeBox.TabStop = false;
            this.includeBox.TextChanged += new System.EventHandler(this.includeBox_TextChanged);
            // 
            // ieLabel
            // 
            this.ieLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ieLabel.Image = global::AuditSec.Properties.Resources.ie;
            this.ieLabel.Location = new System.Drawing.Point(0, 53);
            this.ieLabel.Name = "ieLabel";
            this.ieLabel.Size = new System.Drawing.Size(64, 64);
            this.ieLabel.TabIndex = 60;
            this.ieLabel.Text = "?";
            this.ieLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // firefoxLabel
            // 
            this.firefoxLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.firefoxLabel.Image = global::AuditSec.Properties.Resources.firefox;
            this.firefoxLabel.Location = new System.Drawing.Point(60, 53);
            this.firefoxLabel.Name = "firefoxLabel";
            this.firefoxLabel.Size = new System.Drawing.Size(64, 64);
            this.firefoxLabel.TabIndex = 61;
            this.firefoxLabel.Text = "?";
            this.firefoxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // javaLabel
            // 
            this.javaLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.javaLabel.Image = global::AuditSec.Properties.Resources.java;
            this.javaLabel.Location = new System.Drawing.Point(0, 114);
            this.javaLabel.Name = "javaLabel";
            this.javaLabel.Size = new System.Drawing.Size(64, 64);
            this.javaLabel.TabIndex = 62;
            this.javaLabel.Text = "?";
            this.javaLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // acrobatLabel
            // 
            this.acrobatLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.acrobatLabel.Image = global::AuditSec.Properties.Resources.acrobat;
            this.acrobatLabel.Location = new System.Drawing.Point(180, 53);
            this.acrobatLabel.Name = "acrobatLabel";
            this.acrobatLabel.Size = new System.Drawing.Size(64, 64);
            this.acrobatLabel.TabIndex = 63;
            this.acrobatLabel.Text = "?";
            this.acrobatLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // excludeBox
            // 
            this.excludeBox.BackColor = System.Drawing.Color.White;
            this.excludeBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.excludeBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.excludeBox.ForeColor = System.Drawing.Color.Red;
            this.excludeBox.Location = new System.Drawing.Point(390, 431);
            this.excludeBox.MaxLength = 1000;
            this.excludeBox.Multiline = true;
            this.excludeBox.Name = "excludeBox";
            this.excludeBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.excludeBox.Size = new System.Drawing.Size(381, 41);
            this.excludeBox.TabIndex = 64;
            this.excludeBox.TabStop = false;
            this.excludeBox.TextChanged += new System.EventHandler(this.excludeBox_TextChanged);
            // 
            // refreshButton
            // 
            this.refreshButton.FlatAppearance.BorderSize = 0;
            this.refreshButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refreshButton.Image = global::AuditSec.Properties.Resources.refresh;
            this.refreshButton.Location = new System.Drawing.Point(0, 0);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(16, 16);
            this.refreshButton.TabIndex = 66;
            this.refreshButton.TabStop = false;
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // flashLabel
            // 
            this.flashLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flashLabel.Image = global::AuditSec.Properties.Resources.flash;
            this.flashLabel.Location = new System.Drawing.Point(240, 53);
            this.flashLabel.Name = "flashLabel";
            this.flashLabel.Size = new System.Drawing.Size(64, 64);
            this.flashLabel.TabIndex = 67;
            this.flashLabel.Text = "?";
            this.flashLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ssoLabel
            // 
            this.ssoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ssoLabel.Image = global::AuditSec.Properties.Resources.sso;
            this.ssoLabel.Location = new System.Drawing.Point(420, 52);
            this.ssoLabel.Name = "ssoLabel";
            this.ssoLabel.Size = new System.Drawing.Size(64, 64);
            this.ssoLabel.TabIndex = 68;
            this.ssoLabel.Text = "?";
            this.ssoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // vpnLabel
            // 
            this.vpnLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.vpnLabel.Image = global::AuditSec.Properties.Resources.vpn;
            this.vpnLabel.Location = new System.Drawing.Point(360, 52);
            this.vpnLabel.Name = "vpnLabel";
            this.vpnLabel.Size = new System.Drawing.Size(64, 64);
            this.vpnLabel.TabIndex = 69;
            this.vpnLabel.Text = "?";
            this.vpnLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // HotApps
            // 
            this.AccessibleName = "HotApps";
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(774, 472);
            this.Controls.Add(this.vpnLabel);
            this.Controls.Add(this.ssoLabel);
            this.Controls.Add(this.flashLabel);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.excludeBox);
            this.Controls.Add(this.acrobatLabel);
            this.Controls.Add(this.javaLabel);
            this.Controls.Add(this.firefoxLabel);
            this.Controls.Add(this.ieLabel);
            this.Controls.Add(this.includeBox);
            this.Controls.Add(this.hotappsBox);
            this.Controls.Add(this.viewupdBox);
            this.Controls.Add(this.wmiGroup);
            this.Controls.Add(this.instAppsLabel);
            this.Controls.Add(this.instAppsTable);
            this.Controls.Add(this.machineBigBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(780, 500);
            this.MinimumSize = new System.Drawing.Size(780, 210);
            this.Name = "HotApps";
            this.Text = "Hot Applications Check";
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.wmiGroup.ResumeLayout(false);
            this.wmiGroup.PerformLayout();
            this.loggedinPanel.ResumeLayout(false);
            this.loggedinPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.instAppsTable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox adminsBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label adminsLabel;
        private System.Windows.Forms.TextBox machineBigBox;
        private System.Windows.Forms.GroupBox wmiGroup;
        private System.Windows.Forms.TextBox chassisBox;
        private System.Windows.Forms.TextBox makermodelBox;
        private System.Windows.Forms.TextBox freeBox;
        private System.Windows.Forms.DataGridView instAppsTable;
        private System.Windows.Forms.Label instAppsLabel;
        private System.ComponentModel.BackgroundWorker instAppsWorker;
        private System.ComponentModel.BackgroundWorker wmiWorker;
        private System.Windows.Forms.FlowLayoutPanel loggedinPanel;
        private System.Windows.Forms.Label loggedinLabel;
        private System.Windows.Forms.TextBox loggedinBox;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox viewupdBox;
        private System.Windows.Forms.CheckBox hotappsBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn appName;
        private System.Windows.Forms.DataGridViewTextBoxColumn appVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn appVendor;
        private System.Windows.Forms.DataGridViewTextBoxColumn appInstallDate;
        private System.Windows.Forms.DataGridViewImageColumn Desired;
        private System.Windows.Forms.Button batteryButton;
        private System.Windows.Forms.TextBox includeBox;
        private System.Windows.Forms.Label ieLabel;
        private System.Windows.Forms.Label firefoxLabel;
        private System.Windows.Forms.Label javaLabel;
        private System.Windows.Forms.Label acrobatLabel;
        private System.Windows.Forms.TextBox excludeBox;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Label flashLabel;
        private System.Windows.Forms.Label ssoLabel;
        private System.Windows.Forms.Label vpnLabel;
    }
}