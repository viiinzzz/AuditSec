
using System;
namespace AuditSec
{
    partial class ComputerManagement
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComputerManagement));
            this.domainPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.domainLabel = new System.Windows.Forms.Label();
            this.domainBox = new System.Windows.Forms.ComboBox();
            this.OUPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.OULabel = new System.Windows.Forms.Label();
            this.OUBox = new System.Windows.Forms.ComboBox();
            this.machinePanel = new System.Windows.Forms.FlowLayoutPanel();
            this.machineLabel = new System.Windows.Forms.Label();
            this.machineBox = new System.Windows.Forms.ComboBox();
            this.compfindBox = new System.Windows.Forms.TextBox();
            this.userPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.userLabel = new System.Windows.Forms.Label();
            this.clearusrButton = new System.Windows.Forms.Button();
            this.userBox = new System.Windows.Forms.ComboBox();
            this.addAdminButton = new System.Windows.Forms.Button();
            this.revokeAdminButton = new System.Windows.Forms.Button();
            this.adminsBox = new System.Windows.Forms.CheckedListBox();
            this.adminpwBox = new System.Windows.Forms.TextBox();
            this.AdminResetButton = new System.Windows.Forms.Button();
            this.adminpwLabel = new System.Windows.Forms.Label();
            this.rkeyBox = new System.Windows.Forms.TextBox();
            this.findUserButton = new System.Windows.Forms.Button();
            this.ownerButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.findLabel = new System.Windows.Forms.Label();
            this.findBox = new System.Windows.Forms.TextBox();
            this.compfindTimer = new System.Windows.Forms.Timer(this.components);
            this.UsrResetButton = new System.Windows.Forms.Button();
            this.UsrExpsoonBox = new System.Windows.Forms.CheckBox();
            this.UsrExpiredBox = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.adminsLabel = new System.Windows.Forms.Label();
            this.machineBigBox = new System.Windows.Forms.TextBox();
            this.wmiGroup = new System.Windows.Forms.GroupBox();
            this.freeBox = new System.Windows.Forms.TextBox();
            this.chassisBox = new System.Windows.Forms.TextBox();
            this.makermodelBox = new System.Windows.Forms.TextBox();
            this.printerButton = new System.Windows.Forms.Button();
            this.batteryButton = new System.Windows.Forms.Button();
            this.picLabel = new System.Windows.Forms.Label();
            this.instAppsTable = new System.Windows.Forms.DataGridView();
            this.appName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.appVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.appVendor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.appInstallDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Desired = new System.Windows.Forms.DataGridViewImageColumn();
            this.instAppsLabel = new System.Windows.Forms.Label();
            this.savePictureFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.WkDisabledBox = new System.Windows.Forms.CheckBox();
            this.UsrDisabledBox = new System.Windows.Forms.CheckBox();
            this.UsrLockedBox = new System.Windows.Forms.CheckBox();
            this.instAppsWorker = new System.ComponentModel.BackgroundWorker();
            this.wmiWorker = new System.ComponentModel.BackgroundWorker();
            this.loggedinPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.loggedinLabel = new System.Windows.Forms.Label();
            this.loggedinBox = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.hotappsBox = new System.Windows.Forms.CheckBox();
            this.remoteButton = new System.Windows.Forms.Button();
            this.mobileCall = new System.Windows.Forms.Button();
            this.officeCall = new System.Windows.Forms.Button();
            this.refreshDesapButton = new System.Windows.Forms.Button();
            this.cButton = new System.Windows.Forms.Button();
            this.expandButton = new System.Windows.Forms.Button();
            this.rButton = new System.Windows.Forms.Button();
            this.reEnabButton = new System.Windows.Forms.Button();
            this.showStaffDetails = new System.Windows.Forms.Button();
            this.showProfilesDetails = new System.Windows.Forms.Button();
            this.IPBox = new System.Windows.Forms.TextBox();
            this.viewupdBox = new System.Windows.Forms.CheckBox();
            this.multipcbox = new System.Windows.Forms.ComboBox();
            this.openPwlistFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.lyncHost = new System.Windows.Forms.Integration.ElementHost();
            this.lyncControl = new myWPF.LyncControl();
            this.mbam = new MBAMControl();
            this.useraccountLabel = new System.Windows.Forms.Label();
            this.computeraccountLabel = new System.Windows.Forms.Label();
            this.costBox = new System.Windows.Forms.TextBox();
            this.costLabel = new System.Windows.Forms.Label();
            this.staffDetailsTimer = new System.Windows.Forms.Timer(this.components);
            this.profileDetailsWorker = new System.ComponentModel.BackgroundWorker();
            this.showProfileDetailsBox = new System.Windows.Forms.TextBox();
            this.profileDetailsTimer = new System.Windows.Forms.Timer(this.components);
            this.domainPanel.SuspendLayout();
            this.OUPanel.SuspendLayout();
            this.machinePanel.SuspendLayout();
            this.userPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.wmiGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.instAppsTable)).BeginInit();
            this.loggedinPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // domainPanel
            // 
            this.domainPanel.Controls.Add(this.domainLabel);
            this.domainPanel.Controls.Add(this.domainBox);
            this.domainPanel.Location = new System.Drawing.Point(12, 5);
            this.domainPanel.Name = "domainPanel";
            this.domainPanel.Size = new System.Drawing.Size(220, 28);
            this.domainPanel.TabIndex = 17;
            // 
            // domainLabel
            // 
            this.domainLabel.AutoSize = true;
            this.domainLabel.Location = new System.Drawing.Point(3, 8);
            this.domainLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.domainLabel.Name = "domainLabel";
            this.domainLabel.Size = new System.Drawing.Size(52, 13);
            this.domainLabel.TabIndex = 2;
            this.domainLabel.Text = "   Domain";
            // 
            // domainBox
            // 
            this.domainBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.domainBox.FormattingEnabled = true;
            this.domainBox.Location = new System.Drawing.Point(61, 3);
            this.domainBox.Name = "domainBox";
            this.domainBox.Size = new System.Drawing.Size(150, 21);
            this.domainBox.Sorted = true;
            this.domainBox.TabIndex = 0;
            this.domainBox.SelectedIndexChanged += new System.EventHandler(this.domainBox_SelectedIndexChanged);
            this.domainBox.DropDownClosed += new System.EventHandler(this.domainBox_DropDownClosed);
            this.domainBox.Click += new System.EventHandler(this.domainBox_Click);
            // 
            // OUPanel
            // 
            this.OUPanel.Controls.Add(this.OULabel);
            this.OUPanel.Controls.Add(this.OUBox);
            this.OUPanel.Location = new System.Drawing.Point(238, 5);
            this.OUPanel.Name = "OUPanel";
            this.OUPanel.Size = new System.Drawing.Size(113, 28);
            this.OUPanel.TabIndex = 18;
            // 
            // OULabel
            // 
            this.OULabel.AutoSize = true;
            this.OULabel.Location = new System.Drawing.Point(3, 8);
            this.OULabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.OULabel.Name = "OULabel";
            this.OULabel.Size = new System.Drawing.Size(28, 13);
            this.OULabel.TabIndex = 3;
            this.OULabel.Text = " Site";
            // 
            // OUBox
            // 
            this.OUBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.OUBox.FormattingEnabled = true;
            this.OUBox.Location = new System.Drawing.Point(37, 3);
            this.OUBox.Name = "OUBox";
            this.OUBox.Size = new System.Drawing.Size(72, 21);
            this.OUBox.Sorted = true;
            this.OUBox.TabIndex = 1;
            this.OUBox.SelectedIndexChanged += new System.EventHandler(this.OUBox_SelectedIndexChanged);
            this.OUBox.DropDownClosed += new System.EventHandler(this.OUBox_DropDownClosed);
            this.OUBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OUBox_MouseClick);
            // 
            // machinePanel
            // 
            this.machinePanel.Controls.Add(this.machineLabel);
            this.machinePanel.Controls.Add(this.machineBox);
            this.machinePanel.Location = new System.Drawing.Point(12, 36);
            this.machinePanel.Name = "machinePanel";
            this.machinePanel.Size = new System.Drawing.Size(262, 26);
            this.machinePanel.TabIndex = 19;
            // 
            // machineLabel
            // 
            this.machineLabel.AutoSize = true;
            this.machineLabel.Location = new System.Drawing.Point(3, 8);
            this.machineLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.machineLabel.Name = "machineLabel";
            this.machineLabel.Size = new System.Drawing.Size(52, 13);
            this.machineLabel.TabIndex = 3;
            this.machineLabel.Text = "Computer";
            // 
            // machineBox
            // 
            this.machineBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.machineBox.FormattingEnabled = true;
            this.machineBox.Location = new System.Drawing.Point(61, 3);
            this.machineBox.Name = "machineBox";
            this.machineBox.Size = new System.Drawing.Size(198, 21);
            this.machineBox.Sorted = true;
            this.machineBox.TabIndex = 1;
            this.machineBox.SelectedIndexChanged += new System.EventHandler(this.machineBox_SelectedIndexChanged);
            this.machineBox.DropDownClosed += new System.EventHandler(this.machineBox_DropDownClosed);
            this.machineBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.machineBox_MouseClick);
            // 
            // compfindBox
            // 
            this.compfindBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.compfindBox.Location = new System.Drawing.Point(357, 9);
            this.compfindBox.MaxLength = 9;
            this.compfindBox.Name = "compfindBox";
            this.compfindBox.Size = new System.Drawing.Size(60, 20);
            this.compfindBox.TabIndex = 38;
            this.compfindBox.Text = "[search]";
            this.compfindBox.TextChanged += new System.EventHandler(this.compfindBox_TextChanged);
            // 
            // userPanel
            // 
            this.userPanel.Controls.Add(this.userLabel);
            this.userPanel.Controls.Add(this.clearusrButton);
            this.userPanel.Controls.Add(this.userBox);
            this.userPanel.Location = new System.Drawing.Point(420, 36);
            this.userPanel.Name = "userPanel";
            this.userPanel.Size = new System.Drawing.Size(202, 28);
            this.userPanel.TabIndex = 20;
            // 
            // userLabel
            // 
            this.userLabel.AutoSize = true;
            this.userLabel.Location = new System.Drawing.Point(3, 8);
            this.userLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.userLabel.Name = "userLabel";
            this.userLabel.Size = new System.Drawing.Size(29, 13);
            this.userLabel.TabIndex = 3;
            this.userLabel.Text = "User";
            // 
            // clearusrButton
            // 
            this.clearusrButton.Font = new System.Drawing.Font("Wingdings 2", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.clearusrButton.Location = new System.Drawing.Point(38, 3);
            this.clearusrButton.Name = "clearusrButton";
            this.clearusrButton.Size = new System.Drawing.Size(21, 21);
            this.clearusrButton.TabIndex = 5;
            this.clearusrButton.TabStop = false;
            this.clearusrButton.Text = "O";
            this.toolTip1.SetToolTip(this.clearusrButton, "Clear the user field");
            this.clearusrButton.UseVisualStyleBackColor = true;
            this.clearusrButton.Click += new System.EventHandler(this.clearusrButton_Click);
            // 
            // userBox
            // 
            this.userBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.userBox.FormattingEnabled = true;
            this.userBox.Location = new System.Drawing.Point(65, 3);
            this.userBox.Name = "userBox";
            this.userBox.Size = new System.Drawing.Size(130, 21);
            this.userBox.Sorted = true;
            this.userBox.TabIndex = 6;
            this.userBox.TabStop = false;
            // 
            // addAdminButton
            // 
            this.addAdminButton.Location = new System.Drawing.Point(275, 59);
            this.addAdminButton.Name = "addAdminButton";
            this.addAdminButton.Size = new System.Drawing.Size(142, 20);
            this.addAdminButton.TabIndex = 23;
            this.addAdminButton.TabStop = false;
            this.addAdminButton.Text = "< < < Promote Admin < < <";
            this.addAdminButton.UseVisualStyleBackColor = true;
            this.addAdminButton.Click += new System.EventHandler(this.addAdminButton_Click);
            // 
            // revokeAdminButton
            // 
            this.revokeAdminButton.Location = new System.Drawing.Point(275, 79);
            this.revokeAdminButton.Name = "revokeAdminButton";
            this.revokeAdminButton.Size = new System.Drawing.Size(142, 20);
            this.revokeAdminButton.TabIndex = 25;
            this.revokeAdminButton.TabStop = false;
            this.revokeAdminButton.Text = "> > > Revoke Admin > > >";
            this.revokeAdminButton.UseVisualStyleBackColor = true;
            this.revokeAdminButton.Click += new System.EventHandler(this.revokeAdminButton_Click);
            // 
            // adminsBox
            // 
            this.adminsBox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.adminsBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.adminsBox.CheckOnClick = true;
            this.adminsBox.FormattingEnabled = true;
            this.adminsBox.Location = new System.Drawing.Point(62, 3);
            this.adminsBox.Name = "adminsBox";
            this.adminsBox.Size = new System.Drawing.Size(195, 45);
            this.adminsBox.TabIndex = 26;
            this.adminsBox.TabStop = false;
            this.adminsBox.SelectedIndexChanged += new System.EventHandler(this.adminsBox_SelectedIndexChanged);
            // 
            // adminpwBox
            // 
            this.adminpwBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.adminpwBox.Font = new System.Drawing.Font("Lucida Console", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.adminpwBox.Location = new System.Drawing.Point(212, 224);
            this.adminpwBox.MaxLength = 20;
            this.adminpwBox.Name = "adminpwBox";
            this.adminpwBox.ReadOnly = true;
            this.adminpwBox.Size = new System.Drawing.Size(120, 15);
            this.adminpwBox.TabIndex = 3;
            this.adminpwBox.TabStop = false;
            // 
            // AdminResetButton
            // 
            this.AdminResetButton.Location = new System.Drawing.Point(125, 224);
            this.AdminResetButton.Name = "AdminResetButton";
            this.AdminResetButton.Size = new System.Drawing.Size(83, 22);
            this.AdminResetButton.TabIndex = 42;
            this.AdminResetButton.TabStop = false;
            this.AdminResetButton.Text = "Reset admin";
            this.AdminResetButton.UseVisualStyleBackColor = true;
            this.AdminResetButton.Click += new System.EventHandler(this.pwsetButton_Click);
            // 
            // adminpwLabel
            // 
            this.adminpwLabel.AutoSize = true;
            this.adminpwLabel.Location = new System.Drawing.Point(122, 207);
            this.adminpwLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.adminpwLabel.Name = "adminpwLabel";
            this.adminpwLabel.Size = new System.Drawing.Size(36, 13);
            this.adminpwLabel.TabIndex = 2;
            this.adminpwLabel.Text = "Admin";
            this.adminpwLabel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.adminpwLabel_MouseClick);
            // 
            // rkeyBox
            // 
            this.rkeyBox.Font = new System.Drawing.Font("Lucida Console", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rkeyBox.Location = new System.Drawing.Point(283, 242);
            this.rkeyBox.MaxLength = 120;
            this.rkeyBox.Multiline = true;
            this.rkeyBox.Name = "rkeyBox";
            this.rkeyBox.ReadOnly = true;
            this.rkeyBox.Size = new System.Drawing.Size(299, 36);
            this.rkeyBox.TabIndex = 3;
            this.rkeyBox.TabStop = false;
            this.rkeyBox.Click += new System.EventHandler(this.rkeyBox_Click);
            // 
            // findUserButton
            // 
            this.findUserButton.Location = new System.Drawing.Point(25, 3);
            this.findUserButton.Name = "findUserButton";
            this.findUserButton.Size = new System.Drawing.Size(35, 21);
            this.findUserButton.TabIndex = 33;
            this.findUserButton.TabStop = false;
            this.findUserButton.Text = "Find";
            this.toolTip1.SetToolTip(this.findUserButton, "Find a user");
            this.findUserButton.UseVisualStyleBackColor = true;
            this.findUserButton.Click += new System.EventHandler(this.findUserButton_Click);
            // 
            // ownerButton
            // 
            this.ownerButton.Location = new System.Drawing.Point(275, 39);
            this.ownerButton.Name = "ownerButton";
            this.ownerButton.Size = new System.Drawing.Size(142, 20);
            this.ownerButton.TabIndex = 36;
            this.ownerButton.TabStop = false;
            this.ownerButton.Text = "< < < Set as Owner < < <";
            this.ownerButton.UseVisualStyleBackColor = true;
            this.ownerButton.Click += new System.EventHandler(this.ownerButton_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.findLabel);
            this.flowLayoutPanel1.Controls.Add(this.findUserButton);
            this.flowLayoutPanel1.Controls.Add(this.findBox);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(420, 71);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(202, 26);
            this.flowLayoutPanel1.TabIndex = 37;
            // 
            // findLabel
            // 
            this.findLabel.AutoSize = true;
            this.findLabel.Location = new System.Drawing.Point(3, 0);
            this.findLabel.Name = "findLabel";
            this.findLabel.Size = new System.Drawing.Size(16, 13);
            this.findLabel.TabIndex = 38;
            this.findLabel.Text = "   ";
            // 
            // findBox
            // 
            this.findBox.Location = new System.Drawing.Point(66, 3);
            this.findBox.MaxLength = 30;
            this.findBox.Name = "findBox";
            this.findBox.Size = new System.Drawing.Size(130, 20);
            this.findBox.TabIndex = 39;
            this.findBox.TabStop = false;
            // 
            // compfindTimer
            // 
            this.compfindTimer.Enabled = true;
            this.compfindTimer.Interval = 2000;
            this.compfindTimer.Tick += new System.EventHandler(this.compfindTimer_Tick);
            // 
            // UsrResetButton
            // 
            this.UsrResetButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.UsrResetButton.Location = new System.Drawing.Point(6, 224);
            this.UsrResetButton.Name = "UsrResetButton";
            this.UsrResetButton.Size = new System.Drawing.Size(88, 22);
            this.UsrResetButton.TabIndex = 65;
            this.UsrResetButton.Text = "Reset user";
            this.UsrResetButton.UseVisualStyleBackColor = true;
            this.UsrResetButton.Click += new System.EventHandler(this.resetPasswordButton_Click);
            // 
            // UsrExpsoonBox
            // 
            this.UsrExpsoonBox.AutoSize = true;
            this.UsrExpsoonBox.Location = new System.Drawing.Point(7, 247);
            this.UsrExpsoonBox.Name = "UsrExpsoonBox";
            this.UsrExpsoonBox.Size = new System.Drawing.Size(86, 17);
            this.UsrExpsoonBox.TabIndex = 66;
            this.UsrExpsoonBox.TabStop = false;
            this.UsrExpsoonBox.Text = "soon expired";
            this.UsrExpsoonBox.UseVisualStyleBackColor = true;
            // 
            // UsrExpiredBox
            // 
            this.UsrExpiredBox.AutoSize = true;
            this.UsrExpiredBox.Location = new System.Drawing.Point(7, 264);
            this.UsrExpiredBox.Name = "UsrExpiredBox";
            this.UsrExpiredBox.Size = new System.Drawing.Size(60, 17);
            this.UsrExpiredBox.TabIndex = 64;
            this.UsrExpiredBox.TabStop = false;
            this.UsrExpiredBox.Text = "expired";
            this.UsrExpiredBox.UseVisualStyleBackColor = true;
            this.UsrExpiredBox.CheckedChanged += new System.EventHandler(this.UsrExpiredBox_CheckedChanged);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.adminsLabel);
            this.flowLayoutPanel2.Controls.Add(this.adminsBox);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(12, 80);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(262, 55);
            this.flowLayoutPanel2.TabIndex = 40;
            // 
            // adminsLabel
            // 
            this.adminsLabel.AutoSize = true;
            this.adminsLabel.Location = new System.Drawing.Point(3, 0);
            this.adminsLabel.Name = "adminsLabel";
            this.adminsLabel.Size = new System.Drawing.Size(53, 13);
            this.adminsLabel.TabIndex = 41;
            this.adminsLabel.Text = "Admins    ";
            this.adminsLabel.Click += new System.EventHandler(this.adminsLabel_Click);
            // 
            // machineBigBox
            // 
            this.machineBigBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.machineBigBox.Font = new System.Drawing.Font("Lucida Console", 32.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.machineBigBox.Location = new System.Drawing.Point(12, 134);
            this.machineBigBox.Name = "machineBigBox";
            this.machineBigBox.ReadOnly = true;
            this.machineBigBox.Size = new System.Drawing.Size(262, 43);
            this.machineBigBox.TabIndex = 41;
            // 
            // wmiGroup
            // 
            this.wmiGroup.Controls.Add(this.freeBox);
            this.wmiGroup.Controls.Add(this.chassisBox);
            this.wmiGroup.Controls.Add(this.makermodelBox);
            this.wmiGroup.Location = new System.Drawing.Point(275, 134);
            this.wmiGroup.Name = "wmiGroup";
            this.wmiGroup.Size = new System.Drawing.Size(211, 88);
            this.wmiGroup.TabIndex = 42;
            this.wmiGroup.TabStop = false;
            // 
            // freeBox
            // 
            this.freeBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.freeBox.Location = new System.Drawing.Point(2, 52);
            this.freeBox.Multiline = true;
            this.freeBox.Name = "freeBox";
            this.freeBox.ReadOnly = true;
            this.freeBox.Size = new System.Drawing.Size(203, 30);
            this.freeBox.TabIndex = 2;
            // 
            // chassisBox
            // 
            this.chassisBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chassisBox.Location = new System.Drawing.Point(2, 35);
            this.chassisBox.Name = "chassisBox";
            this.chassisBox.ReadOnly = true;
            this.chassisBox.Size = new System.Drawing.Size(203, 13);
            this.chassisBox.TabIndex = 1;
            // 
            // makermodelBox
            // 
            this.makermodelBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.makermodelBox.Location = new System.Drawing.Point(2, 15);
            this.makermodelBox.Name = "makermodelBox";
            this.makermodelBox.ReadOnly = true;
            this.makermodelBox.Size = new System.Drawing.Size(203, 13);
            this.makermodelBox.TabIndex = 0;
            // 
            // printerButton
            // 
            this.printerButton.FlatAppearance.BorderSize = 0;
            this.printerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.printerButton.Image = global::AuditSec.Properties.Resources.printer;
            this.printerButton.Location = new System.Drawing.Point(486, 222);
            this.printerButton.Name = "printerButton";
            this.printerButton.Size = new System.Drawing.Size(18, 18);
            this.printerButton.TabIndex = 65;
            this.printerButton.TabStop = false;
            this.toolTip1.SetToolTip(this.printerButton, "Show Local Printer");
            this.printerButton.UseVisualStyleBackColor = true;
            this.printerButton.Click += new System.EventHandler(this.printerButton_Click);
            // 
            // batteryButton
            // 
            this.batteryButton.FlatAppearance.BorderSize = 0;
            this.batteryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.batteryButton.Image = global::AuditSec.Properties.Resources.batt;
            this.batteryButton.Location = new System.Drawing.Point(508, 222);
            this.batteryButton.Name = "batteryButton";
            this.batteryButton.Size = new System.Drawing.Size(18, 18);
            this.batteryButton.TabIndex = 64;
            this.batteryButton.TabStop = false;
            this.toolTip1.SetToolTip(this.batteryButton, "Show Battery Data");
            this.batteryButton.UseVisualStyleBackColor = true;
            this.batteryButton.Click += new System.EventHandler(this.batteryButton_Click);
            // 
            // picLabel
            // 
            this.picLabel.AutoSize = true;
            this.picLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.picLabel.Location = new System.Drawing.Point(492, 100);
            this.picLabel.MaximumSize = new System.Drawing.Size(90, 120);
            this.picLabel.MinimumSize = new System.Drawing.Size(90, 120);
            this.picLabel.Name = "picLabel";
            this.picLabel.Size = new System.Drawing.Size(90, 120);
            this.picLabel.TabIndex = 3;
            this.picLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.picLabel.Click += new System.EventHandler(this.picLabel_Click);
            // 
            // instAppsTable
            // 
            this.instAppsTable.AllowUserToAddRows = false;
            this.instAppsTable.AllowUserToDeleteRows = false;
            this.instAppsTable.AllowUserToOrderColumns = true;
            this.instAppsTable.BackgroundColor = System.Drawing.SystemColors.Control;
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
            this.instAppsTable.Location = new System.Drawing.Point(2, 306);
            this.instAppsTable.Name = "instAppsTable";
            this.instAppsTable.ReadOnly = true;
            this.instAppsTable.Size = new System.Drawing.Size(620, 252);
            this.instAppsTable.TabIndex = 43;
            this.instAppsTable.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.instAppsTable_CellContentClick);
            this.instAppsTable.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.instAppsTable_CellFormatting);
            // 
            // appName
            // 
            this.appName.FillWeight = 250F;
            this.appName.HeaderText = "Name";
            this.appName.Name = "appName";
            this.appName.ReadOnly = true;
            this.appName.Width = 250;
            // 
            // appVersion
            // 
            this.appVersion.HeaderText = "Version";
            this.appVersion.Name = "appVersion";
            this.appVersion.ReadOnly = true;
            // 
            // appVendor
            // 
            this.appVendor.FillWeight = 80F;
            this.appVendor.HeaderText = "Vendor";
            this.appVendor.Name = "appVendor";
            this.appVendor.ReadOnly = true;
            this.appVendor.Width = 80;
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
            this.Desired.FillWeight = 40F;
            this.Desired.HeaderText = "Desired";
            this.Desired.Name = "Desired";
            this.Desired.ReadOnly = true;
            this.Desired.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Desired.Width = 40;
            // 
            // instAppsLabel
            // 
            this.instAppsLabel.AutoSize = true;
            this.instAppsLabel.Location = new System.Drawing.Point(10, 287);
            this.instAppsLabel.Name = "instAppsLabel";
            this.instAppsLabel.Size = new System.Drawing.Size(106, 13);
            this.instAppsLabel.TabIndex = 44;
            this.instAppsLabel.Text = "Installed Applications";
            // 
            // savePictureFileDialog
            // 
            this.savePictureFileDialog.Title = "Report - Save File";
            this.savePictureFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
            // 
            // WkDisabledBox
            // 
            this.WkDisabledBox.AutoSize = true;
            this.WkDisabledBox.Location = new System.Drawing.Point(125, 190);
            this.WkDisabledBox.Name = "WkDisabledBox";
            this.WkDisabledBox.Size = new System.Drawing.Size(65, 17);
            this.WkDisabledBox.TabIndex = 47;
            this.WkDisabledBox.TabStop = false;
            this.WkDisabledBox.Text = "disabled";
            this.WkDisabledBox.UseVisualStyleBackColor = true;
            this.WkDisabledBox.CheckedChanged += new System.EventHandler(this.WkDisabledBox_CheckedChanged);
            // 
            // UsrDisabledBox
            // 
            this.UsrDisabledBox.AutoSize = true;
            this.UsrDisabledBox.Location = new System.Drawing.Point(7, 190);
            this.UsrDisabledBox.Name = "UsrDisabledBox";
            this.UsrDisabledBox.Size = new System.Drawing.Size(65, 17);
            this.UsrDisabledBox.TabIndex = 48;
            this.UsrDisabledBox.TabStop = false;
            this.UsrDisabledBox.Text = "disabled";
            this.UsrDisabledBox.UseVisualStyleBackColor = true;
            this.UsrDisabledBox.CheckedChanged += new System.EventHandler(this.UsrDisabledBox_CheckedChanged);
            // 
            // UsrLockedBox
            // 
            this.UsrLockedBox.AutoSize = true;
            this.UsrLockedBox.Location = new System.Drawing.Point(7, 207);
            this.UsrLockedBox.Name = "UsrLockedBox";
            this.UsrLockedBox.Size = new System.Drawing.Size(58, 17);
            this.UsrLockedBox.TabIndex = 49;
            this.UsrLockedBox.TabStop = false;
            this.UsrLockedBox.Text = "locked";
            this.UsrLockedBox.UseVisualStyleBackColor = true;
            this.UsrLockedBox.CheckedChanged += new System.EventHandler(this.UsrLockedBox_CheckedChanged);
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
            // loggedinPanel
            // 
            this.loggedinPanel.Controls.Add(this.loggedinLabel);
            this.loggedinPanel.Controls.Add(this.loggedinBox);
            this.loggedinPanel.Location = new System.Drawing.Point(420, 5);
            this.loggedinPanel.Name = "loggedinPanel";
            this.loggedinPanel.Size = new System.Drawing.Size(202, 28);
            this.loggedinPanel.TabIndex = 52;
            // 
            // loggedinLabel
            // 
            this.loggedinLabel.AutoSize = true;
            this.loggedinLabel.Location = new System.Drawing.Point(3, 8);
            this.loggedinLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.loggedinLabel.Name = "loggedinLabel";
            this.loggedinLabel.Size = new System.Drawing.Size(57, 13);
            this.loggedinLabel.TabIndex = 3;
            this.loggedinLabel.Text = "Logged in ";
            // 
            // loggedinBox
            // 
            this.loggedinBox.Location = new System.Drawing.Point(66, 3);
            this.loggedinBox.MaxLength = 30;
            this.loggedinBox.Name = "loggedinBox";
            this.loggedinBox.ReadOnly = true;
            this.loggedinBox.Size = new System.Drawing.Size(130, 20);
            this.loggedinBox.TabIndex = 35;
            this.loggedinBox.TabStop = false;
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
            this.hotappsBox.Location = new System.Drawing.Point(392, 286);
            this.hotappsBox.Name = "hotappsBox";
            this.hotappsBox.Size = new System.Drawing.Size(92, 17);
            this.hotappsBox.TabIndex = 58;
            this.hotappsBox.Text = "Hot Apps only";
            this.toolTip1.SetToolTip(this.hotappsBox, "Hoy Apps:");
            this.hotappsBox.UseVisualStyleBackColor = true;
            this.hotappsBox.CheckedChanged += new System.EventHandler(this.hotappsBox_CheckedChanged);
            // 
            // remoteButton
            // 
            this.remoteButton.Image = global::AuditSec.Properties.Resources.RA;
            this.remoteButton.Location = new System.Drawing.Point(420, 100);
            this.remoteButton.Name = "remoteButton";
            this.remoteButton.Size = new System.Drawing.Size(66, 38);
            this.remoteButton.TabIndex = 53;
            this.remoteButton.TabStop = false;
            this.toolTip1.SetToolTip(this.remoteButton, "Remote Assistance");
            this.remoteButton.UseVisualStyleBackColor = true;
            this.remoteButton.Click += new System.EventHandler(this.remoteButton_Click);
            // 
            // mobileCall
            // 
            this.mobileCall.Image = global::AuditSec.Properties.Resources.iph;
            this.mobileCall.Location = new System.Drawing.Point(583, 202);
            this.mobileCall.Name = "mobileCall";
            this.mobileCall.Size = new System.Drawing.Size(40, 40);
            this.mobileCall.TabIndex = 51;
            this.mobileCall.TabStop = false;
            this.toolTip1.SetToolTip(this.mobileCall, "Call Mobile Phone");
            this.mobileCall.UseVisualStyleBackColor = true;
            this.mobileCall.Click += new System.EventHandler(this.mobileCall_Click);
            // 
            // officeCall
            // 
            this.officeCall.Image = global::AuditSec.Properties.Resources.cipp;
            this.officeCall.Location = new System.Drawing.Point(583, 163);
            this.officeCall.Name = "officeCall";
            this.officeCall.Size = new System.Drawing.Size(40, 40);
            this.officeCall.TabIndex = 50;
            this.officeCall.TabStop = false;
            this.toolTip1.SetToolTip(this.officeCall, "Call Desk Phone");
            this.officeCall.UseVisualStyleBackColor = true;
            this.officeCall.Click += new System.EventHandler(this.callButton_Click);
            // 
            // refreshDesapButton
            // 
            this.refreshDesapButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.refreshDesapButton.Location = new System.Drawing.Point(275, 100);
            this.refreshDesapButton.Name = "refreshDesapButton";
            this.refreshDesapButton.Size = new System.Drawing.Size(46, 21);
            this.refreshDesapButton.TabIndex = 54;
            this.refreshDesapButton.Text = "F5";
            this.toolTip1.SetToolTip(this.refreshDesapButton, "Refresh this form");
            this.refreshDesapButton.UseVisualStyleBackColor = true;
            this.refreshDesapButton.Click += new System.EventHandler(this.refreshDesapButton_Click);
            // 
            // cButton
            // 
            this.cButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cButton.Location = new System.Drawing.Point(375, 100);
            this.cButton.Name = "cButton";
            this.cButton.Size = new System.Drawing.Size(42, 21);
            this.cButton.TabIndex = 56;
            this.cButton.TabStop = false;
            this.cButton.Text = "\\\\C$";
            this.toolTip1.SetToolTip(this.cButton, "Open the remote folder");
            this.cButton.UseVisualStyleBackColor = true;
            this.cButton.Click += new System.EventHandler(this.cButton_Click);
            // 
            // expandButton
            // 
            this.expandButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.expandButton.Image = global::AuditSec.Properties.Resources.repo1;
            this.expandButton.Location = new System.Drawing.Point(583, 241);
            this.expandButton.Name = "expandButton";
            this.expandButton.Size = new System.Drawing.Size(40, 40);
            this.expandButton.TabIndex = 59;
            this.expandButton.TabStop = false;
            this.expandButton.Text = "vvv";
            this.toolTip1.SetToolTip(this.expandButton, "Show Installed Applications");
            this.expandButton.UseVisualStyleBackColor = true;
            this.expandButton.Click += new System.EventHandler(this.expandButton_Click);
            // 
            // rButton
            // 
            this.rButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rButton.Location = new System.Drawing.Point(327, 100);
            this.rButton.Name = "rButton";
            this.rButton.Size = new System.Drawing.Size(42, 21);
            this.rButton.TabIndex = 61;
            this.rButton.TabStop = false;
            this.rButton.Text = "C:\\>_";
            this.toolTip1.SetToolTip(this.rButton, "Get a remote prompt");
            this.rButton.UseVisualStyleBackColor = true;
            this.rButton.Click += new System.EventHandler(this.rButton_Click);
            // 
            // reEnabButton
            // 
            this.reEnabButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.reEnabButton.Location = new System.Drawing.Point(12, 60);
            this.reEnabButton.Name = "reEnabButton";
            this.reEnabButton.Size = new System.Drawing.Size(96, 21);
            this.reEnabButton.TabIndex = 62;
            this.reEnabButton.Text = "Find Orphaned";
            this.toolTip1.SetToolTip(this.reEnabButton, "If you can\'t find a computer, you may find it in the orphaned section");
            this.reEnabButton.UseVisualStyleBackColor = true;
            this.reEnabButton.Click += new System.EventHandler(this.reEnabButton_Click);
            // 
            // showStaffDetails
            // 
            this.showStaffDetails.FlatAppearance.BorderSize = 0;
            this.showStaffDetails.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.showStaffDetails.Image = global::AuditSec.Properties.Resources.Dpt18;
            this.showStaffDetails.Location = new System.Drawing.Point(564, 222);
            this.showStaffDetails.Name = "showStaffDetails";
            this.showStaffDetails.Size = new System.Drawing.Size(18, 18);
            this.showStaffDetails.TabIndex = 66;
            this.showStaffDetails.TabStop = false;
            this.toolTip1.SetToolTip(this.showStaffDetails, "Show Staff Details");
            this.showStaffDetails.UseVisualStyleBackColor = true;
            this.showStaffDetails.Click += new System.EventHandler(this.showStaffDetails_Click);
            // 
            // showProfilesDetails
            // 
            this.showProfilesDetails.FlatAppearance.BorderSize = 0;
            this.showProfilesDetails.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.showProfilesDetails.Image = global::AuditSec.Properties.Resources.dirsize;
            this.showProfilesDetails.Location = new System.Drawing.Point(530, 222);
            this.showProfilesDetails.Name = "showProfilesDetails";
            this.showProfilesDetails.Size = new System.Drawing.Size(18, 18);
            this.showProfilesDetails.TabIndex = 70;
            this.showProfilesDetails.TabStop = false;
            this.toolTip1.SetToolTip(this.showProfilesDetails, "Show Battery Data");
            this.showProfilesDetails.UseVisualStyleBackColor = true;
            this.showProfilesDetails.Click += new System.EventHandler(this.showProfilesDetails_Click);
            // 
            // IPBox
            // 
            this.IPBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.IPBox.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IPBox.Location = new System.Drawing.Point(276, 127);
            this.IPBox.Name = "IPBox";
            this.IPBox.ReadOnly = true;
            this.IPBox.Size = new System.Drawing.Size(142, 11);
            this.IPBox.TabIndex = 55;
            // 
            // viewupdBox
            // 
            this.viewupdBox.AutoSize = true;
            this.viewupdBox.Location = new System.Drawing.Point(490, 286);
            this.viewupdBox.Name = "viewupdBox";
            this.viewupdBox.Size = new System.Drawing.Size(92, 17);
            this.viewupdBox.TabIndex = 57;
            this.viewupdBox.Text = "View Updates";
            this.viewupdBox.UseVisualStyleBackColor = true;
            this.viewupdBox.CheckedChanged += new System.EventHandler(this.viewupdBox_CheckedChanged);
            // 
            // multipcbox
            // 
            this.multipcbox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.multipcbox.FormattingEnabled = true;
            this.multipcbox.Location = new System.Drawing.Point(357, 8);
            this.multipcbox.Name = "multipcbox";
            this.multipcbox.Size = new System.Drawing.Size(24, 138);
            this.multipcbox.Sorted = true;
            this.multipcbox.TabIndex = 60;
            this.multipcbox.Visible = false;
            this.multipcbox.SelectedIndexChanged += new System.EventHandler(this.multipcbox_SelectedIndexChanged);
            this.multipcbox.TextChanged += new System.EventHandler(this.multipcbox_TextChanged);
            // 
            // openPwlistFileDialog
            // 
            this.openPwlistFileDialog.FileName = "*.xls; *.xlsx";
            this.openPwlistFileDialog.Filter = "Excel Documents|*.xls; *.xlsx";
            this.openPwlistFileDialog.Title = "PC Management works better with a Password List - Open File";
            this.openPwlistFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openPwlistFileDialog_FileOk);
            // 
            // lyncHost
            // 
            this.lyncHost.Location = new System.Drawing.Point(588, 100);
            this.lyncHost.Name = "lyncHost";
            this.lyncHost.Size = new System.Drawing.Size(30, 61);
            this.lyncHost.TabIndex = 46;
            this.lyncHost.TabStop = false;
            this.lyncHost.Text = "Lync";
            this.lyncHost.Child = this.lyncControl;
            // 
            // mbam
            // 
            this.mbam.BackColor = System.Drawing.SystemColors.Control;
            this.mbam.Location = new System.Drawing.Point(212, 241);
            this.mbam.Name = "mbam";
            this.mbam.Size = new System.Drawing.Size(370, 40);
            this.mbam.TabIndex = 63;
            // 
            // useraccountLabel
            // 
            this.useraccountLabel.AutoSize = true;
            this.useraccountLabel.Location = new System.Drawing.Point(4, 177);
            this.useraccountLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.useraccountLabel.Name = "useraccountLabel";
            this.useraccountLabel.Size = new System.Drawing.Size(29, 13);
            this.useraccountLabel.TabIndex = 4;
            this.useraccountLabel.Text = "User";
            // 
            // computeraccountLabel
            // 
            this.computeraccountLabel.AutoSize = true;
            this.computeraccountLabel.Location = new System.Drawing.Point(122, 177);
            this.computeraccountLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.computeraccountLabel.Name = "computeraccountLabel";
            this.computeraccountLabel.Size = new System.Drawing.Size(52, 13);
            this.computeraccountLabel.TabIndex = 67;
            this.computeraccountLabel.Text = "Computer";
            // 
            // costBox
            // 
            this.costBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.costBox.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.costBox.Location = new System.Drawing.Point(485, 60);
            this.costBox.Name = "costBox";
            this.costBox.ReadOnly = true;
            this.costBox.Size = new System.Drawing.Size(131, 11);
            this.costBox.TabIndex = 68;
            // 
            // costLabel
            // 
            this.costLabel.AutoSize = true;
            this.costLabel.Font = new System.Drawing.Font("Lucida Console", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.costLabel.Location = new System.Drawing.Point(420, 63);
            this.costLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.costLabel.Name = "costLabel";
            this.costLabel.Size = new System.Drawing.Size(60, 8);
            this.costLabel.TabIndex = 69;
            this.costLabel.Text = "cost center";
            // 
            // staffDetailsTimer
            // 
            this.staffDetailsTimer.Enabled = true;
            this.staffDetailsTimer.Interval = 1000;
            this.staffDetailsTimer.Tick += new System.EventHandler(this.staffDetailsTimer_Tick);
            // 
            // profileDetailsWorker
            // 
            this.profileDetailsWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.profileDetailsWorker_DoWork);
            this.profileDetailsWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.profileDetailsWorker_RunWorkerCompleted);
            // 
            // showProfileDetailsBox
            // 
            this.showProfileDetailsBox.BackColor = System.Drawing.SystemColors.Info;
            this.showProfileDetailsBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.showProfileDetailsBox.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.showProfileDetailsBox.Location = new System.Drawing.Point(313, 224);
            this.showProfileDetailsBox.Multiline = true;
            this.showProfileDetailsBox.Name = "showProfileDetailsBox";
            this.showProfileDetailsBox.ReadOnly = true;
            this.showProfileDetailsBox.Size = new System.Drawing.Size(213, 38);
            this.showProfileDetailsBox.TabIndex = 71;
            this.showProfileDetailsBox.Text = "Calculating profiles sizes...\r\nFiles: 0,000,000\r\nBytes: 0,000,000";
            this.showProfileDetailsBox.Visible = false;
            // 
            // profileDetailsTimer
            // 
            this.profileDetailsTimer.Enabled = true;
            this.profileDetailsTimer.Interval = 1000;
            this.profileDetailsTimer.Tick += new System.EventHandler(this.profileDetailsTimer_Tick);
            // 
            // ComputerManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 283);
            this.Controls.Add(this.showProfileDetailsBox);
            this.Controls.Add(this.showProfilesDetails);
            this.Controls.Add(this.adminpwBox);
            this.Controls.Add(this.costLabel);
            this.Controls.Add(this.costBox);
            this.Controls.Add(this.printerButton);
            this.Controls.Add(this.showStaffDetails);
            this.Controls.Add(this.batteryButton);
            this.Controls.Add(this.mbam);
            this.Controls.Add(this.rkeyBox);
            this.Controls.Add(this.UsrResetButton);
            this.Controls.Add(this.UsrLockedBox);
            this.Controls.Add(this.UsrExpsoonBox);
            this.Controls.Add(this.computeraccountLabel);
            this.Controls.Add(this.AdminResetButton);
            this.Controls.Add(this.useraccountLabel);
            this.Controls.Add(this.UsrExpiredBox);
            this.Controls.Add(this.reEnabButton);
            this.Controls.Add(this.adminpwLabel);
            this.Controls.Add(this.multipcbox);
            this.Controls.Add(this.rButton);
            this.Controls.Add(this.expandButton);
            this.Controls.Add(this.hotappsBox);
            this.Controls.Add(this.viewupdBox);
            this.Controls.Add(this.cButton);
            this.Controls.Add(this.IPBox);
            this.Controls.Add(this.refreshDesapButton);
            this.Controls.Add(this.loggedinPanel);
            this.Controls.Add(this.compfindBox);
            this.Controls.Add(this.remoteButton);
            this.Controls.Add(this.mobileCall);
            this.Controls.Add(this.officeCall);
            this.Controls.Add(this.UsrDisabledBox);
            this.Controls.Add(this.WkDisabledBox);
            this.Controls.Add(this.lyncHost);
            this.Controls.Add(this.instAppsLabel);
            this.Controls.Add(this.instAppsTable);
            this.Controls.Add(this.picLabel);
            this.Controls.Add(this.machineBigBox);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.wmiGroup);
            this.Controls.Add(this.addAdminButton);
            this.Controls.Add(this.ownerButton);
            this.Controls.Add(this.machinePanel);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.userPanel);
            this.Controls.Add(this.revokeAdminButton);
            this.Controls.Add(this.domainPanel);
            this.Controls.Add(this.OUPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(642, 600);
            this.MinimumSize = new System.Drawing.Size(642, 321);
            this.Name = "ComputerManagement";
            this.Text = "PC Management";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ComputerManagement_FormClosed);
            this.Shown += new System.EventHandler(this.ComputerManagement_Shown);
            this.domainPanel.ResumeLayout(false);
            this.domainPanel.PerformLayout();
            this.OUPanel.ResumeLayout(false);
            this.OUPanel.PerformLayout();
            this.machinePanel.ResumeLayout(false);
            this.machinePanel.PerformLayout();
            this.userPanel.ResumeLayout(false);
            this.userPanel.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.wmiGroup.ResumeLayout(false);
            this.wmiGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.instAppsTable)).EndInit();
            this.loggedinPanel.ResumeLayout(false);
            this.loggedinPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel domainPanel;
        private System.Windows.Forms.Label domainLabel;
        private System.Windows.Forms.ComboBox domainBox;
        private System.Windows.Forms.FlowLayoutPanel OUPanel;
        private System.Windows.Forms.Label OULabel;
        private System.Windows.Forms.ComboBox OUBox;
        private System.Windows.Forms.FlowLayoutPanel machinePanel;
        private System.Windows.Forms.Label machineLabel;
        private System.Windows.Forms.ComboBox machineBox;
        private System.Windows.Forms.FlowLayoutPanel userPanel;
        private System.Windows.Forms.Label userLabel;
        private System.Windows.Forms.Button addAdminButton;
        private System.Windows.Forms.Button revokeAdminButton;
        private System.Windows.Forms.CheckedListBox adminsBox;
        private System.Windows.Forms.Label adminpwLabel;
        private System.Windows.Forms.TextBox adminpwBox;
        private System.Windows.Forms.TextBox rkeyBox;
        private System.Windows.Forms.Button findUserButton;
        private System.Windows.Forms.Button ownerButton;
        private System.Windows.Forms.Button clearusrButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label findLabel;
        private System.Windows.Forms.TextBox compfindBox;
        private System.Windows.Forms.Timer compfindTimer;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label adminsLabel;
        private System.Windows.Forms.TextBox machineBigBox;
        private System.Windows.Forms.Button AdminResetButton;
        private System.Windows.Forms.GroupBox wmiGroup;
        private System.Windows.Forms.TextBox chassisBox;
        private System.Windows.Forms.TextBox makermodelBox;
        private System.Windows.Forms.TextBox freeBox;
        private System.Windows.Forms.Label picLabel;
        private System.Windows.Forms.DataGridView instAppsTable;
        private System.Windows.Forms.Label instAppsLabel;
        private System.Windows.Forms.Integration.ElementHost lyncHost;
        private myWPF.LyncControl lyncControl = new myWPF.LyncControl();
        private System.Windows.Forms.SaveFileDialog savePictureFileDialog;
        private System.Windows.Forms.CheckBox WkDisabledBox;
        private System.Windows.Forms.CheckBox UsrDisabledBox;
        private System.Windows.Forms.CheckBox UsrLockedBox;
        private System.Windows.Forms.Button officeCall;
        private System.Windows.Forms.Button mobileCall;
        private System.ComponentModel.BackgroundWorker instAppsWorker;
        private System.ComponentModel.BackgroundWorker wmiWorker;
        private System.Windows.Forms.FlowLayoutPanel loggedinPanel;
        private System.Windows.Forms.Label loggedinLabel;
        private System.Windows.Forms.TextBox loggedinBox;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button remoteButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn appName;
        private System.Windows.Forms.DataGridViewTextBoxColumn appVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn appVendor;
        private System.Windows.Forms.DataGridViewTextBoxColumn appInstallDate;
        private System.Windows.Forms.DataGridViewImageColumn Desired;
        private System.Windows.Forms.Button refreshDesapButton;
        private System.Windows.Forms.TextBox IPBox;
        private System.Windows.Forms.Button cButton;
        private System.Windows.Forms.CheckBox viewupdBox;
        private System.Windows.Forms.CheckBox hotappsBox;
        private System.Windows.Forms.Button expandButton;
        private System.Windows.Forms.ComboBox multipcbox;
        private System.Windows.Forms.ComboBox userBox;
        private System.Windows.Forms.TextBox findBox;
        private System.Windows.Forms.OpenFileDialog openPwlistFileDialog;
        private System.Windows.Forms.Button rButton;
        private System.Windows.Forms.Button reEnabButton;
        private MBAMControl mbam;
        private System.Windows.Forms.Button batteryButton;
        private System.Windows.Forms.Button printerButton;
        private System.Windows.Forms.CheckBox UsrExpiredBox;
        private System.Windows.Forms.Button UsrResetButton;
        private System.Windows.Forms.Label useraccountLabel;
        private System.Windows.Forms.CheckBox UsrExpsoonBox;
        private System.Windows.Forms.Label computeraccountLabel;
        private System.Windows.Forms.Button showStaffDetails;
        private System.Windows.Forms.TextBox costBox;
        private System.Windows.Forms.Label costLabel;
        private System.Windows.Forms.Timer staffDetailsTimer;
        private System.Windows.Forms.Button showProfilesDetails;
        private System.ComponentModel.BackgroundWorker profileDetailsWorker;
        private System.Windows.Forms.TextBox showProfileDetailsBox;
        private System.Windows.Forms.Timer profileDetailsTimer;
    }
}