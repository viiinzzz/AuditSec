namespace AuditSec
{
    partial class SetupWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupWizard));
            this.label1 = new System.Windows.Forms.Label();
            this.SelectNone = new System.Windows.Forms.Button();
            this.Setup = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.curverBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SelectAll = new System.Windows.Forms.Button();
            this.Quit = new System.Windows.Forms.Button();
            this.logsBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(357, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Please select the tools you wish a shortcut to be installed on your desktop:";
            // 
            // SelectNone
            // 
            this.SelectNone.FlatAppearance.BorderSize = 0;
            this.SelectNone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SelectNone.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectNone.Location = new System.Drawing.Point(413, 66);
            this.SelectNone.Name = "SelectNone";
            this.SelectNone.Size = new System.Drawing.Size(100, 20);
            this.SelectNone.TabIndex = 3;
            this.SelectNone.Text = "Select None";
            this.SelectNone.UseVisualStyleBackColor = true;
            this.SelectNone.Click += new System.EventHandler(this.SelectNone_Click);
            // 
            // Setup
            // 
            this.Setup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Setup.Location = new System.Drawing.Point(458, 362);
            this.Setup.Name = "Setup";
            this.Setup.Size = new System.Drawing.Size(154, 31);
            this.Setup.TabIndex = 4;
            this.Setup.Text = "Setup Shortcuts";
            this.Setup.UseVisualStyleBackColor = true;
            this.Setup.Click += new System.EventHandler(this.Setup_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(50, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(575, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "You are installing a suite of tools, developped inhouse, for gaining productivity" +
                " with common IT Ops tasks.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Image = global::AuditSec.Properties.Resources.Dpt;
            this.label3.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label3.Location = new System.Drawing.Point(3, 44);
            this.label3.MinimumSize = new System.Drawing.Size(50, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 50);
            this.label3.TabIndex = 6;
            // 
            // curverBox
            // 
            this.curverBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.curverBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.curverBox.Location = new System.Drawing.Point(158, 8);
            this.curverBox.Name = "curverBox";
            this.curverBox.ReadOnly = true;
            this.curverBox.Size = new System.Drawing.Size(130, 15);
            this.curverBox.TabIndex = 7;
            this.curverBox.Text = "___.___.___.___";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(21, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "This release is version ";
            // 
            // SelectAll
            // 
            this.SelectAll.FlatAppearance.BorderSize = 0;
            this.SelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SelectAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectAll.Location = new System.Drawing.Point(519, 66);
            this.SelectAll.Name = "SelectAll";
            this.SelectAll.Size = new System.Drawing.Size(100, 20);
            this.SelectAll.TabIndex = 9;
            this.SelectAll.Text = "Select All";
            this.SelectAll.UseVisualStyleBackColor = true;
            this.SelectAll.Click += new System.EventHandler(this.SelectAll_Click);
            // 
            // Quit
            // 
            this.Quit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Quit.Location = new System.Drawing.Point(458, 399);
            this.Quit.Name = "Quit";
            this.Quit.Size = new System.Drawing.Size(154, 31);
            this.Quit.TabIndex = 10;
            this.Quit.Text = "Quit";
            this.Quit.UseVisualStyleBackColor = true;
            this.Quit.Click += new System.EventHandler(this.Quit_Click);
            // 
            // logsBox
            // 
            this.logsBox.Location = new System.Drawing.Point(53, 92);
            this.logsBox.Multiline = true;
            this.logsBox.Name = "logsBox";
            this.logsBox.ReadOnly = true;
            this.logsBox.Size = new System.Drawing.Size(516, 264);
            this.logsBox.TabIndex = 11;
            this.logsBox.Visible = false;
            // 
            // SetupWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 442);
            this.Controls.Add(this.logsBox);
            this.Controls.Add(this.Quit);
            this.Controls.Add(this.SelectAll);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.curverBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Setup);
            this.Controls.Add(this.SelectNone);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SetupWizard";
            this.Text = "Setup Wizard for the IT Ops Tools Suite";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button SelectNone;
        private System.Windows.Forms.Button Setup;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox curverBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button SelectAll;
        private System.Windows.Forms.Button Quit;
        private System.Windows.Forms.TextBox logsBox;
    }
}