using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using System.Windows.Forms;

namespace AuditSec
{
    partial class AuditSecGUIForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AuditSecGUIForm));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.domainLabel = new System.Windows.Forms.Label();
            this.domainBox = new System.Windows.Forms.ComboBox();
            this.OULabel = new System.Windows.Forms.Label();
            this.OUBox = new System.Windows.Forms.ComboBox();
            this.tasksGroup = new System.Windows.Forms.GroupBox();
            this.taskList = new System.Windows.Forms.CheckedListBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.stdpwLabel = new System.Windows.Forms.Label();
            this.stdpwBox = new System.Windows.Forms.TextBox();
            this.OUMaskLabel = new System.Windows.Forms.Label();
            this.OUMaskBox = new System.Windows.Forms.TextBox();
            this.openPwlistFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.pwsButton = new System.Windows.Forms.Button();
            this.pwsBox = new System.Windows.Forms.TextBox();
            this.WkMaskLabel = new System.Windows.Forms.Label();
            this.WkMaskBox = new System.Windows.Forms.TextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.actionsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.welcomeButton = new System.Windows.Forms.Button();
            this.actionsGroup = new System.Windows.Forms.GroupBox();
            this.ctrlPanel = new System.Windows.Forms.Panel();
            this.licButton = new System.Windows.Forms.Button();
            this.optButton = new System.Windows.Forms.Button();
            this.CompEnabButton = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.elevatorButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel7 = new System.Windows.Forms.FlowLayoutPanel();
            this.issuesCountLabel = new System.Windows.Forms.Label();
            this.issuesCountBox = new System.Windows.Forms.TextBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.dptBox = new System.Windows.Forms.ComboBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel6 = new System.Windows.Forms.FlowLayoutPanel();
            this.saveReportFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.sitemaskTimer = new System.Windows.Forms.Timer(this.components);
            this.compmaskTimer = new System.Windows.Forms.Timer(this.components);
            this.lockedAccountTimer = new System.Windows.Forms.Timer(this.components);
            this.smsWorker = new System.ComponentModel.BackgroundWorker();
            this.tasksView = new System.Windows.Forms.ListView();
            this.tasksLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel8 = new System.Windows.Forms.FlowLayoutPanel();
            this.adminsLabel = new System.Windows.Forms.Label();
            this.adminsBox = new System.Windows.Forms.TextBox();
            this.optLabel = new System.Windows.Forms.Label();
            this.spyTimer = new System.Windows.Forms.Timer(this.components);
            this.flowLayoutPanel9 = new System.Windows.Forms.FlowLayoutPanel();
            this.desapButton = new System.Windows.Forms.Button();
            this.desapBox = new System.Windows.Forms.TextBox();
            this.openDesapFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.flowLayoutPanel10 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.detectBox = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.tasksGroup.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.actionsPanel.SuspendLayout();
            this.actionsGroup.SuspendLayout();
            this.ctrlPanel.SuspendLayout();
            this.flowLayoutPanel7.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.flowLayoutPanel5.SuspendLayout();
            this.flowLayoutPanel6.SuspendLayout();
            this.flowLayoutPanel8.SuspendLayout();
            this.flowLayoutPanel9.SuspendLayout();
            this.flowLayoutPanel10.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.domainLabel);
            this.flowLayoutPanel1.Controls.Add(this.domainBox);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(5, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(212, 28);
            this.flowLayoutPanel1.TabIndex = 0;
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
            this.domainBox.FormattingEnabled = true;
            this.domainBox.Location = new System.Drawing.Point(52, 3);
            this.domainBox.Name = "domainBox";
            this.domainBox.Size = new System.Drawing.Size(150, 21);
            this.domainBox.Sorted = true;
            this.domainBox.TabIndex = 0;
            this.domainBox.SelectedIndexChanged += new System.EventHandler(this.domainBox_SelectedIndexChanged);
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
            this.OUBox.SelectedIndexChanged += new System.EventHandler(this.OUBox_SelectedIndexChanged);
            // 
            // tasksGroup
            // 
            this.tasksGroup.Controls.Add(this.taskList);
            this.tasksGroup.Location = new System.Drawing.Point(5, 99);
            this.tasksGroup.Name = "tasksGroup";
            this.tasksGroup.Size = new System.Drawing.Size(212, 163);
            this.tasksGroup.TabIndex = 1;
            this.tasksGroup.TabStop = false;
            this.tasksGroup.Text = "Task List";
            // 
            // taskList
            // 
            this.taskList.BackColor = System.Drawing.SystemColors.Control;
            this.taskList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.taskList.CheckOnClick = true;
            this.taskList.FormattingEnabled = true;
            this.taskList.Items.AddRange(new object[] {
            "Check Windows 7",
            "Check Windows 7 SP1",
            "Check Administrator Password",
            "Check Administrators Group/UAC",
            "Check AD-Misplaced/Wrong Owner",
            "Check Bitlocker",
            "Check SCCM NO Client/Heartbeat",
            "Check Forefront Virus Definition"});
            this.taskList.Location = new System.Drawing.Point(6, 19);
            this.taskList.Name = "taskList";
            this.taskList.Size = new System.Drawing.Size(204, 135);
            this.taskList.TabIndex = 0;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.stdpwLabel);
            this.flowLayoutPanel2.Controls.Add(this.stdpwBox);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(787, 91);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(212, 47);
            this.flowLayoutPanel2.TabIndex = 2;
            // 
            // stdpwLabel
            // 
            this.stdpwLabel.AutoSize = true;
            this.stdpwLabel.Location = new System.Drawing.Point(3, 8);
            this.stdpwLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.stdpwLabel.Name = "stdpwLabel";
            this.stdpwLabel.Size = new System.Drawing.Size(89, 13);
            this.stdpwLabel.TabIndex = 1;
            this.stdpwLabel.Text = "         Default Pw.";
            // 
            // stdpwBox
            // 
            this.stdpwBox.Location = new System.Drawing.Point(98, 3);
            this.stdpwBox.Multiline = true;
            this.stdpwBox.Name = "stdpwBox";
            this.stdpwBox.Size = new System.Drawing.Size(110, 41);
            this.stdpwBox.TabIndex = 0;
            // 
            // OUMaskLabel
            // 
            this.OUMaskLabel.AutoSize = true;
            this.OUMaskLabel.Location = new System.Drawing.Point(3, 8);
            this.OUMaskLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.OUMaskLabel.Name = "OUMaskLabel";
            this.OUMaskLabel.Size = new System.Drawing.Size(90, 13);
            this.OUMaskLabel.TabIndex = 2;
            this.OUMaskLabel.Text = "            Site Mask";
            // 
            // OUMaskBox
            // 
            this.OUMaskBox.Location = new System.Drawing.Point(99, 3);
            this.OUMaskBox.Name = "OUMaskBox";
            this.OUMaskBox.Size = new System.Drawing.Size(110, 20);
            this.OUMaskBox.TabIndex = 3;
            this.OUMaskBox.Text = "^[A-Z]{3}$|^Orphaned Workstations$";
            this.OUMaskBox.TextChanged += new System.EventHandler(this.OUMaskBox_TextChanged);
            // 
            // openPwlistFileDialog
            // 
            this.openPwlistFileDialog.FileName = "*.xls; *.xlsx";
            this.openPwlistFileDialog.Filter = "Excel Documents|*.xls; *.xlsx";
            this.openPwlistFileDialog.Title = "Password List - Open File";
            this.openPwlistFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // startButton
            // 
            this.startButton.BackColor = System.Drawing.Color.Wheat;
            this.startButton.FlatAppearance.BorderColor = System.Drawing.Color.Orange;
            this.startButton.FlatAppearance.BorderSize = 2;
            this.startButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.startButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startButton.Location = new System.Drawing.Point(3, 28);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(139, 31);
            this.startButton.TabIndex = 3;
            this.startButton.Text = "Check Computers";
            this.startButton.UseVisualStyleBackColor = false;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.BackColor = System.Drawing.Color.LightCoral;
            this.stopButton.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.stopButton.FlatAppearance.BorderSize = 2;
            this.stopButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.stopButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stopButton.Location = new System.Drawing.Point(148, 28);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(67, 31);
            this.stopButton.TabIndex = 4;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = false;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.pwsButton);
            this.flowLayoutPanel3.Controls.Add(this.pwsBox);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(787, 141);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(212, 28);
            this.flowLayoutPanel3.TabIndex = 5;
            // 
            // pwsButton
            // 
            this.pwsButton.Location = new System.Drawing.Point(3, 3);
            this.pwsButton.Name = "pwsButton";
            this.pwsButton.Size = new System.Drawing.Size(50, 23);
            this.pwsButton.TabIndex = 6;
            this.pwsButton.Text = "Pw. list";
            this.pwsButton.UseVisualStyleBackColor = true;
            this.pwsButton.Click += new System.EventHandler(this.pwsButton_Click);
            // 
            // pwsBox
            // 
            this.pwsBox.Location = new System.Drawing.Point(59, 3);
            this.pwsBox.Name = "pwsBox";
            this.pwsBox.ReadOnly = true;
            this.pwsBox.Size = new System.Drawing.Size(149, 20);
            this.pwsBox.TabIndex = 1;
            // 
            // WkMaskLabel
            // 
            this.WkMaskLabel.AutoSize = true;
            this.WkMaskLabel.Location = new System.Drawing.Point(3, 8);
            this.WkMaskLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.WkMaskLabel.Name = "WkMaskLabel";
            this.WkMaskLabel.Size = new System.Drawing.Size(90, 13);
            this.WkMaskLabel.TabIndex = 2;
            this.WkMaskLabel.Text = "   Computer Mask";
            // 
            // WkMaskBox
            // 
            this.WkMaskBox.Location = new System.Drawing.Point(99, 3);
            this.WkMaskBox.Name = "WkMaskBox";
            this.WkMaskBox.Size = new System.Drawing.Size(110, 20);
            this.WkMaskBox.TabIndex = 3;
            this.WkMaskBox.Text = "^[A-Z]{3}[0-9]{6}$";
            this.WkMaskBox.TextChanged += new System.EventHandler(this.WkMaskBox_TextChanged);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progressBar,
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 445);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1010, 22);
            this.statusStrip.TabIndex = 6;
            this.statusStrip.Text = "statusStrip";
            // 
            // progressBar
            // 
            this.progressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 16);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = false;
            this.statusLabel.AutoToolTip = true;
            this.statusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(893, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.Text = "Select a Domain, a Site, choose tasks and then push \'Check Computers\'";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // actionsPanel
            // 
            this.actionsPanel.AutoScroll = true;
            this.actionsPanel.AutoSize = true;
            this.actionsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.actionsPanel.Controls.Add(this.welcomeButton);
            this.actionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionsPanel.Location = new System.Drawing.Point(3, 16);
            this.actionsPanel.Name = "actionsPanel";
            this.actionsPanel.Size = new System.Drawing.Size(547, 423);
            this.actionsPanel.TabIndex = 7;
            this.actionsPanel.Scroll += new System.Windows.Forms.ScrollEventHandler(this.actionsPanel_Scroll);
            // 
            // welcomeButton
            // 
            this.welcomeButton.Font = new System.Drawing.Font("Freestyle Script", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.welcomeButton.ForeColor = System.Drawing.Color.Green;
            this.welcomeButton.Location = new System.Drawing.Point(3, 3);
            this.welcomeButton.Name = "welcomeButton";
            this.welcomeButton.Size = new System.Drawing.Size(541, 413);
            this.welcomeButton.TabIndex = 0;
            this.welcomeButton.Text = "Welcome to Workstations Security Audit !\r\n\r\nPlease wait a moment...";
            this.welcomeButton.UseVisualStyleBackColor = true;
            this.welcomeButton.Click += new System.EventHandler(this.welcomeButton_Click);
            // 
            // actionsGroup
            // 
            this.actionsGroup.Controls.Add(this.actionsPanel);
            this.actionsGroup.Location = new System.Drawing.Point(231, 0);
            this.actionsGroup.Name = "actionsGroup";
            this.actionsGroup.Size = new System.Drawing.Size(553, 442);
            this.actionsGroup.TabIndex = 8;
            this.actionsGroup.TabStop = false;
            this.actionsGroup.Text = "Corrective Actions";
            // 
            // ctrlPanel
            // 
            this.ctrlPanel.Controls.Add(this.licButton);
            this.ctrlPanel.Controls.Add(this.optButton);
            this.ctrlPanel.Controls.Add(this.CompEnabButton);
            this.ctrlPanel.Controls.Add(this.button2);
            this.ctrlPanel.Controls.Add(this.elevatorButton);
            this.ctrlPanel.Controls.Add(this.flowLayoutPanel7);
            this.ctrlPanel.Controls.Add(this.flowLayoutPanel4);
            this.ctrlPanel.Controls.Add(this.flowLayoutPanel1);
            this.ctrlPanel.Controls.Add(this.tasksGroup);
            this.ctrlPanel.Location = new System.Drawing.Point(0, 0);
            this.ctrlPanel.Name = "ctrlPanel";
            this.ctrlPanel.Size = new System.Drawing.Size(228, 442);
            this.ctrlPanel.TabIndex = 9;
            // 
            // licButton
            // 
            this.licButton.BackColor = System.Drawing.Color.LemonChiffon;
            this.licButton.FlatAppearance.BorderColor = System.Drawing.Color.Gold;
            this.licButton.FlatAppearance.BorderSize = 2;
            this.licButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.licButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.licButton.Location = new System.Drawing.Point(5, 408);
            this.licButton.Name = "licButton";
            this.licButton.Size = new System.Drawing.Size(139, 31);
            this.licButton.TabIndex = 17;
            this.licButton.Text = "SCCM Reporting";
            this.licButton.UseVisualStyleBackColor = false;
            this.licButton.Visible = false;
            // 
            // optButton
            // 
            this.optButton.Location = new System.Drawing.Point(5, 63);
            this.optButton.Name = "optButton";
            this.optButton.Size = new System.Drawing.Size(212, 31);
            this.optButton.TabIndex = 16;
            this.optButton.Text = ">  >  >    Show more Options    >  >  >";
            this.optButton.UseVisualStyleBackColor = true;
            this.optButton.Click += new System.EventHandler(this.optButton_Click);
            // 
            // CompEnabButton
            // 
            this.CompEnabButton.BackColor = System.Drawing.Color.LightCoral;
            this.CompEnabButton.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.CompEnabButton.FlatAppearance.BorderSize = 2;
            this.CompEnabButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CompEnabButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CompEnabButton.Location = new System.Drawing.Point(150, 374);
            this.CompEnabButton.Name = "CompEnabButton";
            this.CompEnabButton.Size = new System.Drawing.Size(67, 31);
            this.CompEnabButton.TabIndex = 15;
            this.CompEnabButton.Text = "Computer reEnabler";
            this.CompEnabButton.UseVisualStyleBackColor = false;
            this.CompEnabButton.Click += new System.EventHandler(this.CompEnabButton_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.LightCoral;
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.button2.FlatAppearance.BorderSize = 2;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(150, 408);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(67, 31);
            this.button2.TabIndex = 14;
            this.button2.Text = "SCCM clientChk";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // elevatorButton
            // 
            this.elevatorButton.BackColor = System.Drawing.Color.PaleGreen;
            this.elevatorButton.FlatAppearance.BorderColor = System.Drawing.Color.ForestGreen;
            this.elevatorButton.FlatAppearance.BorderSize = 2;
            this.elevatorButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.elevatorButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.elevatorButton.Location = new System.Drawing.Point(5, 374);
            this.elevatorButton.Name = "elevatorButton";
            this.elevatorButton.Size = new System.Drawing.Size(139, 31);
            this.elevatorButton.TabIndex = 12;
            this.elevatorButton.Text = "PC Management";
            this.elevatorButton.UseVisualStyleBackColor = false;
            this.elevatorButton.Click += new System.EventHandler(this.elevatorButton_Click);
            // 
            // flowLayoutPanel7
            // 
            this.flowLayoutPanel7.Controls.Add(this.issuesCountLabel);
            this.flowLayoutPanel7.Controls.Add(this.issuesCountBox);
            this.flowLayoutPanel7.Controls.Add(this.startButton);
            this.flowLayoutPanel7.Controls.Add(this.stopButton);
            this.flowLayoutPanel7.Controls.Add(this.saveButton);
            this.flowLayoutPanel7.Controls.Add(this.button1);
            this.flowLayoutPanel7.Location = new System.Drawing.Point(3, 259);
            this.flowLayoutPanel7.Name = "flowLayoutPanel7";
            this.flowLayoutPanel7.Size = new System.Drawing.Size(222, 92);
            this.flowLayoutPanel7.TabIndex = 11;
            // 
            // issuesCountLabel
            // 
            this.issuesCountLabel.AutoSize = true;
            this.issuesCountLabel.Location = new System.Drawing.Point(3, 8);
            this.issuesCountLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.issuesCountLabel.Name = "issuesCountLabel";
            this.issuesCountLabel.Size = new System.Drawing.Size(133, 13);
            this.issuesCountLabel.TabIndex = 2;
            this.issuesCountLabel.Text = "Security Issues Count        ";
            // 
            // issuesCountBox
            // 
            this.issuesCountBox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.issuesCountBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.issuesCountBox.Enabled = false;
            this.issuesCountBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.issuesCountBox.Location = new System.Drawing.Point(142, 3);
            this.issuesCountBox.Name = "issuesCountBox";
            this.issuesCountBox.Size = new System.Drawing.Size(76, 19);
            this.issuesCountBox.TabIndex = 3;
            this.issuesCountBox.Text = "0";
            this.issuesCountBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(3, 65);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(139, 23);
            this.saveButton.TabIndex = 9;
            this.saveButton.Text = "Save Report As...";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.LightCoral;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.button1.FlatAppearance.BorderSize = 2;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(148, 65);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(66, 23);
            this.button1.TabIndex = 13;
            this.button1.Text = "Clear";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.Controls.Add(this.OULabel);
            this.flowLayoutPanel4.Controls.Add(this.OUBox);
            this.flowLayoutPanel4.Controls.Add(this.dptBox);
            this.flowLayoutPanel4.Location = new System.Drawing.Point(5, 33);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(212, 28);
            this.flowLayoutPanel4.TabIndex = 6;
            // 
            // dptBox
            // 
            this.dptBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dptBox.FormattingEnabled = true;
            this.dptBox.Location = new System.Drawing.Point(130, 3);
            this.dptBox.Name = "dptBox";
            this.dptBox.Size = new System.Drawing.Size(72, 21);
            this.dptBox.Sorted = true;
            this.dptBox.TabIndex = 4;
            this.dptBox.SelectedIndexChanged += new System.EventHandler(this.dptBox_SelectedIndexChanged);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(912, 3);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(96, 13);
            this.linkLabel1.TabIndex = 10;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Contact the Author";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.Controls.Add(this.OUMaskLabel);
            this.flowLayoutPanel5.Controls.Add(this.OUMaskBox);
            this.flowLayoutPanel5.Location = new System.Drawing.Point(787, 33);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(212, 28);
            this.flowLayoutPanel5.TabIndex = 7;
            // 
            // flowLayoutPanel6
            // 
            this.flowLayoutPanel6.Controls.Add(this.WkMaskLabel);
            this.flowLayoutPanel6.Controls.Add(this.WkMaskBox);
            this.flowLayoutPanel6.Location = new System.Drawing.Point(787, 62);
            this.flowLayoutPanel6.Name = "flowLayoutPanel6";
            this.flowLayoutPanel6.Size = new System.Drawing.Size(212, 28);
            this.flowLayoutPanel6.TabIndex = 8;
            // 
            // saveReportFileDialog
            // 
            this.saveReportFileDialog.Title = "Report - Save File";
            this.saveReportFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
            // 
            // sitemaskTimer
            // 
            this.sitemaskTimer.Enabled = true;
            this.sitemaskTimer.Interval = 1000;
            this.sitemaskTimer.Tick += new System.EventHandler(this.sitemaskTimer_Tick);
            // 
            // compmaskTimer
            // 
            this.compmaskTimer.Enabled = true;
            this.compmaskTimer.Interval = 1000;
            this.compmaskTimer.Tick += new System.EventHandler(this.compmaskTimer_Tick);
            // 
            // lockedAccountTimer
            // 
            this.lockedAccountTimer.Interval = 60000;
            this.lockedAccountTimer.Tick += new System.EventHandler(this.lockedAccountTimer_Tick);
            // 
            // smsWorker
            // 
            this.smsWorker.WorkerReportsProgress = true;
            this.smsWorker.WorkerSupportsCancellation = true;
            this.smsWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.smsWorker_DoWork);
            this.smsWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.smsWorker_ProgressChanged);
            this.smsWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.smsWorker_RunWorkerCompleted);
            // 
            // tasksView
            // 
            this.tasksView.Location = new System.Drawing.Point(787, 287);
            this.tasksView.Name = "tasksView";
            this.tasksView.Size = new System.Drawing.Size(221, 155);
            this.tasksView.TabIndex = 11;
            this.tasksView.UseCompatibleStateImageBehavior = false;
            this.tasksView.View = System.Windows.Forms.View.Tile;
            // 
            // tasksLabel
            // 
            this.tasksLabel.AutoSize = true;
            this.tasksLabel.Location = new System.Drawing.Point(787, 271);
            this.tasksLabel.Name = "tasksLabel";
            this.tasksLabel.Size = new System.Drawing.Size(69, 13);
            this.tasksLabel.TabIndex = 12;
            this.tasksLabel.Text = "Active Tasks";
            // 
            // flowLayoutPanel8
            // 
            this.flowLayoutPanel8.Controls.Add(this.adminsLabel);
            this.flowLayoutPanel8.Controls.Add(this.adminsBox);
            this.flowLayoutPanel8.Location = new System.Drawing.Point(787, 170);
            this.flowLayoutPanel8.Name = "flowLayoutPanel8";
            this.flowLayoutPanel8.Size = new System.Drawing.Size(212, 41);
            this.flowLayoutPanel8.TabIndex = 13;
            // 
            // adminsLabel
            // 
            this.adminsLabel.AutoSize = true;
            this.adminsLabel.Location = new System.Drawing.Point(3, 8);
            this.adminsLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.adminsLabel.Name = "adminsLabel";
            this.adminsLabel.Size = new System.Drawing.Size(50, 13);
            this.adminsLabel.TabIndex = 2;
            this.adminsLabel.Text = "Admins   ";
            // 
            // adminsBox
            // 
            this.adminsBox.Location = new System.Drawing.Point(59, 3);
            this.adminsBox.Multiline = true;
            this.adminsBox.Name = "adminsBox";
            this.adminsBox.Size = new System.Drawing.Size(143, 33);
            this.adminsBox.TabIndex = 3;
            this.adminsBox.Text = "pxl\\, \\ishelpdesk, \\temp_admin, useradmin";
            // 
            // optLabel
            // 
            this.optLabel.AutoSize = true;
            this.optLabel.Location = new System.Drawing.Point(784, 14);
            this.optLabel.Name = "optLabel";
            this.optLabel.Size = new System.Drawing.Size(43, 13);
            this.optLabel.TabIndex = 14;
            this.optLabel.Text = "Options";
            // 
            // spyTimer
            // 
            this.spyTimer.Interval = 1000;
            this.spyTimer.Tick += new System.EventHandler(this.spyTimer_Tick);
            // 
            // flowLayoutPanel9
            // 
            this.flowLayoutPanel9.Controls.Add(this.desapButton);
            this.flowLayoutPanel9.Controls.Add(this.desapBox);
            this.flowLayoutPanel9.Location = new System.Drawing.Point(787, 236);
            this.flowLayoutPanel9.Name = "flowLayoutPanel9";
            this.flowLayoutPanel9.Size = new System.Drawing.Size(212, 28);
            this.flowLayoutPanel9.TabIndex = 15;
            // 
            // desapButton
            // 
            this.desapButton.Location = new System.Drawing.Point(3, 3);
            this.desapButton.Name = "desapButton";
            this.desapButton.Size = new System.Drawing.Size(50, 23);
            this.desapButton.TabIndex = 6;
            this.desapButton.Text = "Des.ap";
            this.desapButton.UseVisualStyleBackColor = true;
            this.desapButton.Click += new System.EventHandler(this.desiredButton_Click);
            // 
            // desapBox
            // 
            this.desapBox.Location = new System.Drawing.Point(59, 3);
            this.desapBox.Name = "desapBox";
            this.desapBox.ReadOnly = true;
            this.desapBox.Size = new System.Drawing.Size(149, 20);
            this.desapBox.TabIndex = 1;
            // 
            // openDesapFileDialog
            // 
            this.openDesapFileDialog.FileName = "*.txt";
            this.openDesapFileDialog.Filter = "Text Documents|*.txt";
            this.openDesapFileDialog.Title = "Desired Application List - Open File";
            this.openDesapFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openDesapFileDialog_FileOk);
            // 
            // flowLayoutPanel10
            // 
            this.flowLayoutPanel10.Controls.Add(this.label1);
            this.flowLayoutPanel10.Controls.Add(this.detectBox);
            this.flowLayoutPanel10.Location = new System.Drawing.Point(787, 212);
            this.flowLayoutPanel10.Name = "flowLayoutPanel10";
            this.flowLayoutPanel10.Size = new System.Drawing.Size(212, 21);
            this.flowLayoutPanel10.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "PC Mgmt";
            // 
            // detectBox
            // 
            this.detectBox.AutoSize = true;
            this.detectBox.Location = new System.Drawing.Point(59, 3);
            this.detectBox.Name = "detectBox";
            this.detectBox.Size = new System.Drawing.Size(143, 17);
            this.detectBox.TabIndex = 17;
            this.detectBox.Text = "detect CuciLync && Axyos";
            this.detectBox.UseVisualStyleBackColor = true;
            // 
            // AuditSecGUIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1010, 467);
            this.Controls.Add(this.flowLayoutPanel10);
            this.Controls.Add(this.flowLayoutPanel9);
            this.Controls.Add(this.optLabel);
            this.Controls.Add(this.flowLayoutPanel8);
            this.Controls.Add(this.tasksLabel);
            this.Controls.Add(this.tasksView);
            this.Controls.Add(this.flowLayoutPanel5);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.flowLayoutPanel3);
            this.Controls.Add(this.ctrlPanel);
            this.Controls.Add(this.actionsGroup);
            this.Controls.Add(this.flowLayoutPanel6);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AuditSecGUIForm";
            this.Text = "Workstations Security Audit";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tasksGroup.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.actionsPanel.ResumeLayout(false);
            this.actionsGroup.ResumeLayout(false);
            this.actionsGroup.PerformLayout();
            this.ctrlPanel.ResumeLayout(false);
            this.flowLayoutPanel7.ResumeLayout(false);
            this.flowLayoutPanel7.PerformLayout();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel4.PerformLayout();
            this.flowLayoutPanel5.ResumeLayout(false);
            this.flowLayoutPanel5.PerformLayout();
            this.flowLayoutPanel6.ResumeLayout(false);
            this.flowLayoutPanel6.PerformLayout();
            this.flowLayoutPanel8.ResumeLayout(false);
            this.flowLayoutPanel8.PerformLayout();
            this.flowLayoutPanel9.ResumeLayout(false);
            this.flowLayoutPanel9.PerformLayout();
            this.flowLayoutPanel10.ResumeLayout(false);
            this.flowLayoutPanel10.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox tasksGroup;
        private System.Windows.Forms.CheckedListBox taskList;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.TextBox stdpwBox;
        private System.Windows.Forms.OpenFileDialog openPwlistFileDialog;
        private System.Windows.Forms.Label domainLabel;
        private System.Windows.Forms.Label OULabel;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Label stdpwLabel;
        private System.Windows.Forms.Label OUMaskLabel;
        private System.Windows.Forms.TextBox OUMaskBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.TextBox pwsBox;
        private System.Windows.Forms.Label WkMaskLabel;
        private System.Windows.Forms.TextBox WkMaskBox;
        private System.Windows.Forms.Button pwsButton;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.FlowLayoutPanel actionsPanel;
        private System.Windows.Forms.GroupBox actionsGroup;
        private System.Windows.Forms.Panel ctrlPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel6;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.SaveFileDialog saveReportFileDialog;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel7;
        private System.Windows.Forms.Label issuesCountLabel;
        private System.Windows.Forms.TextBox issuesCountBox;
        private System.Windows.Forms.Button elevatorButton;
        private Timer sitemaskTimer;
        private Timer compmaskTimer;
        private Timer lockedAccountTimer;
        private Button button1;
        private Button button2;
        private System.ComponentModel.BackgroundWorker smsWorker;
        private ListView tasksView;
        private Button CompEnabButton;
        public ComboBox domainBox;
        public ComboBox OUBox;
        public ComboBox dptBox;
        private Button optButton;
        private Label tasksLabel;
        private FlowLayoutPanel flowLayoutPanel8;
        private Label adminsLabel;
        private Label optLabel;
        public TextBox adminsBox;
        private Timer spyTimer;
        private Button welcomeButton;
        private FlowLayoutPanel flowLayoutPanel9;
        private Button desapButton;
        private TextBox desapBox;
        private System.Windows.Forms.OpenFileDialog openDesapFileDialog;
        private Button licButton;
        private FlowLayoutPanel flowLayoutPanel10;
        private Label label1;
        private CheckBox detectBox;
    }
}

