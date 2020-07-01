using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using System.Windows.Forms;

namespace AuditSec
{
    partial class Unlocker
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Unlocker));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.domainLabel = new System.Windows.Forms.Label();
            this.domainBox = new System.Windows.Forms.ComboBox();
            this.Run = new System.Windows.Forms.Button();
            this.Stop = new System.Windows.Forms.Button();
            this.OULabel = new System.Windows.Forms.Label();
            this.OUBox = new System.Windows.Forms.ComboBox();
            this.OUMaskLabel = new System.Windows.Forms.Label();
            this.OUMaskBox = new System.Windows.Forms.TextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.actionsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.welcomeButton = new System.Windows.Forms.Button();
            this.actionsGroup = new System.Windows.Forms.GroupBox();
            this.ctrlPanel = new System.Windows.Forms.Panel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.dptBox = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.sitemaskTimer = new System.Windows.Forms.Timer(this.components);
            this.lockedAccountTimer = new System.Windows.Forms.Timer(this.components);
            this.Clear = new System.Windows.Forms.Button();
            this.actionsWorker = new System.ComponentModel.BackgroundWorker();
            this.bitlockerGroupBox = new System.Windows.Forms.GroupBox();
            this.mbamControl1 = new MBAMControl();
            this.cool = new System.Windows.Forms.Button();
            this.ouch = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.speakWorker = new System.ComponentModel.BackgroundWorker();
            this.flowLayoutPanel1.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.actionsPanel.SuspendLayout();
            this.actionsGroup.SuspendLayout();
            this.ctrlPanel.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.flowLayoutPanel5.SuspendLayout();
            this.bitlockerGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.domainLabel);
            this.flowLayoutPanel1.Controls.Add(this.domainBox);
            this.flowLayoutPanel1.Controls.Add(this.Run);
            this.flowLayoutPanel1.Controls.Add(this.Stop);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(5, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(220, 28);
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
            this.domainBox.Size = new System.Drawing.Size(135, 21);
            this.domainBox.Sorted = true;
            this.domainBox.TabIndex = 0;
            this.domainBox.SelectedIndexChanged += new System.EventHandler(this.domainBox_SelectedIndexChanged);
            // 
            // Run
            // 
            this.Run.FlatAppearance.BorderSize = 0;
            this.Run.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Run.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Run.Image = global::AuditSec.Properties.Resources.Run;
            this.Run.Location = new System.Drawing.Point(193, 3);
            this.Run.Name = "Run";
            this.Run.Size = new System.Drawing.Size(22, 22);
            this.Run.TabIndex = 15;
            this.toolTip.SetToolTip(this.Run, "Start looking for Locked and Expiring Accounts!");
            this.Run.UseVisualStyleBackColor = true;
            this.Run.Click += new System.EventHandler(this.Run_Click);
            // 
            // Stop
            // 
            this.Stop.FlatAppearance.BorderSize = 0;
            this.Stop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Stop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Stop.Image = global::AuditSec.Properties.Resources.Stop;
            this.Stop.Location = new System.Drawing.Point(3, 31);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(22, 22);
            this.Stop.TabIndex = 16;
            this.toolTip.SetToolTip(this.Stop, "Stop looking for Locked and Expiring Accounts!");
            this.Stop.UseVisualStyleBackColor = true;
            this.Stop.Visible = false;
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // OULabel
            // 
            this.OULabel.AutoSize = true;
            this.OULabel.Location = new System.Drawing.Point(8, 63);
            this.OULabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.OULabel.Name = "OULabel";
            this.OULabel.Size = new System.Drawing.Size(102, 13);
            this.OULabel.TabIndex = 3;
            this.OULabel.Text = "Template Messages";
            // 
            // OUBox
            // 
            this.OUBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.OUBox.FormattingEnabled = true;
            this.OUBox.Location = new System.Drawing.Point(34, 3);
            this.OUBox.Name = "OUBox";
            this.OUBox.Size = new System.Drawing.Size(72, 21);
            this.OUBox.Sorted = true;
            this.OUBox.TabIndex = 1;
            this.OUBox.SelectedIndexChanged += new System.EventHandler(this.OUBox_SelectedIndexChanged);
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
            this.OUMaskLabel.Visible = false;
            // 
            // OUMaskBox
            // 
            this.OUMaskBox.Location = new System.Drawing.Point(3, 24);
            this.OUMaskBox.Name = "OUMaskBox";
            this.OUMaskBox.Size = new System.Drawing.Size(110, 20);
            this.OUMaskBox.TabIndex = 3;
            this.OUMaskBox.Text = "^[A-Z]{3}$|^Orphaned Workstations$";
            this.OUMaskBox.Visible = false;
            this.OUMaskBox.TextChanged += new System.EventHandler(this.OUMaskBox_TextChanged);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 445);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(787, 22);
            this.statusStrip.TabIndex = 6;
            this.statusStrip.Text = "statusStrip";
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = false;
            this.statusLabel.AutoToolTip = true;
            this.statusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(772, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.Text = "Select a Domain, a Site";
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
            this.actionsPanel.Size = new System.Drawing.Size(547, 360);
            this.actionsPanel.TabIndex = 7;
            this.actionsPanel.Scroll += new System.Windows.Forms.ScrollEventHandler(this.actionsPanel_Scroll);
            // 
            // welcomeButton
            // 
            this.welcomeButton.Font = new System.Drawing.Font("Freestyle Script", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.welcomeButton.ForeColor = System.Drawing.Color.Green;
            this.welcomeButton.Image = global::AuditSec.Properties.Resources.chainbrk;
            this.welcomeButton.Location = new System.Drawing.Point(3, 3);
            this.welcomeButton.Name = "welcomeButton";
            this.welcomeButton.Size = new System.Drawing.Size(540, 350);
            this.welcomeButton.TabIndex = 0;
            this.welcomeButton.Text = "Welcome to AD Account Unlocker !\r\nPlease wait a moment...";
            this.welcomeButton.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.welcomeButton.UseVisualStyleBackColor = true;
            this.welcomeButton.Click += new System.EventHandler(this.welcomeButton_Click);
            // 
            // actionsGroup
            // 
            this.actionsGroup.Controls.Add(this.actionsPanel);
            this.actionsGroup.Location = new System.Drawing.Point(231, 63);
            this.actionsGroup.Name = "actionsGroup";
            this.actionsGroup.Size = new System.Drawing.Size(553, 379);
            this.actionsGroup.TabIndex = 8;
            this.actionsGroup.TabStop = false;
            this.actionsGroup.Text = "Locked and Expiring Accounts : Action List";
            // 
            // ctrlPanel
            // 
            this.ctrlPanel.Controls.Add(this.OULabel);
            this.ctrlPanel.Controls.Add(this.textBox1);
            this.ctrlPanel.Controls.Add(this.flowLayoutPanel4);
            this.ctrlPanel.Controls.Add(this.flowLayoutPanel1);
            this.ctrlPanel.Location = new System.Drawing.Point(0, 0);
            this.ctrlPanel.Name = "ctrlPanel";
            this.ctrlPanel.Size = new System.Drawing.Size(228, 439);
            this.ctrlPanel.TabIndex = 9;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(5, 79);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(223, 353);
            this.textBox1.TabIndex = 14;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.Controls.Add(this.label2);
            this.flowLayoutPanel4.Controls.Add(this.OUBox);
            this.flowLayoutPanel4.Controls.Add(this.label3);
            this.flowLayoutPanel4.Controls.Add(this.dptBox);
            this.flowLayoutPanel4.Location = new System.Drawing.Point(5, 33);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(220, 28);
            this.flowLayoutPanel4.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 8);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(25, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Site";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(112, 8);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Dpt";
            // 
            // dptBox
            // 
            this.dptBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dptBox.FormattingEnabled = true;
            this.dptBox.Location = new System.Drawing.Point(142, 3);
            this.dptBox.Name = "dptBox";
            this.dptBox.Size = new System.Drawing.Size(72, 21);
            this.dptBox.Sorted = true;
            this.dptBox.TabIndex = 4;
            this.dptBox.SelectedIndexChanged += new System.EventHandler(this.dptBox_SelectedIndexChanged);
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.Controls.Add(this.OUMaskLabel);
            this.flowLayoutPanel5.Controls.Add(this.OUMaskBox);
            this.flowLayoutPanel5.Location = new System.Drawing.Point(623, 6);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(116, 55);
            this.flowLayoutPanel5.TabIndex = 7;
            // 
            // sitemaskTimer
            // 
            this.sitemaskTimer.Enabled = true;
            this.sitemaskTimer.Interval = 1000;
            this.sitemaskTimer.Tick += new System.EventHandler(this.sitemaskTimer_Tick);
            // 
            // lockedAccountTimer
            // 
            this.lockedAccountTimer.Interval = 300000;
            this.lockedAccountTimer.Tick += new System.EventHandler(this.lockedAccountTimer_Tick);
            // 
            // Clear
            // 
            this.Clear.BackColor = System.Drawing.Color.LightCoral;
            this.Clear.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.Clear.FlatAppearance.BorderSize = 2;
            this.Clear.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Clear.Location = new System.Drawing.Point(745, 51);
            this.Clear.Name = "Clear";
            this.Clear.Size = new System.Drawing.Size(42, 22);
            this.Clear.TabIndex = 14;
            this.Clear.Text = "Clear";
            this.toolTip.SetToolTip(this.Clear, "Clear the Action List!");
            this.Clear.UseVisualStyleBackColor = false;
            this.Clear.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // actionsWorker
            // 
            this.actionsWorker.WorkerReportsProgress = true;
            this.actionsWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.actionsWorker_DoWork);
            this.actionsWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.actionsWorker_ProgressChanged);
            // 
            // bitlockerGroupBox
            // 
            this.bitlockerGroupBox.Controls.Add(this.mbamControl1);
            this.bitlockerGroupBox.Location = new System.Drawing.Point(231, 0);
            this.bitlockerGroupBox.Name = "bitlockerGroupBox";
            this.bitlockerGroupBox.Size = new System.Drawing.Size(386, 61);
            this.bitlockerGroupBox.TabIndex = 19;
            this.bitlockerGroupBox.TabStop = false;
            this.bitlockerGroupBox.Text = "Bitlocker Drive Recovery Key";
            this.toolTip.SetToolTip(this.bitlockerGroupBox, "Quick access to MBAM portal");
            // 
            // mbamControl1
            // 
            this.mbamControl1.BackColor = System.Drawing.SystemColors.Control;
            this.mbamControl1.Location = new System.Drawing.Point(6, 15);
            this.mbamControl1.Name = "mbamControl1";
            this.mbamControl1.Size = new System.Drawing.Size(371, 40);
            this.mbamControl1.TabIndex = 18;
            // 
            // cool
            // 
            this.cool.FlatAppearance.BorderSize = 0;
            this.cool.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cool.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cool.Image = global::AuditSec.Properties.Resources.like;
            this.cool.Location = new System.Drawing.Point(765, 0);
            this.cool.Name = "cool";
            this.cool.Size = new System.Drawing.Size(22, 22);
            this.cool.TabIndex = 20;
            this.toolTip.SetToolTip(this.cool, "I Like it. I\'d like a new feature.");
            this.cool.UseVisualStyleBackColor = true;
            this.cool.Click += new System.EventHandler(this.cool_Click);
            // 
            // ouch
            // 
            this.ouch.FlatAppearance.BorderSize = 0;
            this.ouch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.ouch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ouch.Image = global::AuditSec.Properties.Resources.dislike;
            this.ouch.Location = new System.Drawing.Point(753, 6);
            this.ouch.Name = "ouch";
            this.ouch.Size = new System.Drawing.Size(22, 22);
            this.ouch.TabIndex = 21;
            this.toolTip.SetToolTip(this.ouch, "It\'s clumsy. I\'d like you to change something.");
            this.ouch.UseVisualStyleBackColor = true;
            this.ouch.Click += new System.EventHandler(this.ouch_Click);
            // 
            // speakWorker
            // 
            this.speakWorker.WorkerReportsProgress = true;
            this.speakWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.speakWorker_DoWork);
            // 
            // Unlocker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(787, 467);
            this.Controls.Add(this.ouch);
            this.Controls.Add(this.cool);
            this.Controls.Add(this.flowLayoutPanel5);
            this.Controls.Add(this.Clear);
            this.Controls.Add(this.bitlockerGroupBox);
            this.Controls.Add(this.ctrlPanel);
            this.Controls.Add(this.actionsGroup);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(803, 505);
            this.MinimumSize = new System.Drawing.Size(803, 505);
            this.Name = "Unlocker";
            this.Text = "AD Account Unlocker";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.actionsPanel.ResumeLayout(false);
            this.actionsGroup.ResumeLayout(false);
            this.actionsGroup.PerformLayout();
            this.ctrlPanel.ResumeLayout(false);
            this.ctrlPanel.PerformLayout();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel4.PerformLayout();
            this.flowLayoutPanel5.ResumeLayout(false);
            this.flowLayoutPanel5.PerformLayout();
            this.bitlockerGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label domainLabel;
        private System.Windows.Forms.Label OULabel;
        private System.Windows.Forms.Label OUMaskLabel;
        private System.Windows.Forms.TextBox OUMaskBox;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.FlowLayoutPanel actionsPanel;
        private System.Windows.Forms.GroupBox actionsGroup;
        private System.Windows.Forms.Panel ctrlPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private Timer sitemaskTimer;
        private Timer lockedAccountTimer;
        public ComboBox domainBox;
        public ComboBox OUBox;
        public ComboBox dptBox;
        private Button welcomeButton;
        private TextBox textBox1;
        private Label label2;
        private Label label3;
        private Button Stop;
        private Button Run;
        private Button Clear;
        private System.ComponentModel.BackgroundWorker actionsWorker;
        private MBAMControl mbamControl1;
        private GroupBox bitlockerGroupBox;
        private Button cool;
        private Button ouch;
        private ToolTip toolTip;
        private System.ComponentModel.BackgroundWorker speakWorker;
    }
}

