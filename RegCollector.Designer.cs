namespace AuditSec
{
    partial class RegCollector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegCollector));
            this.reg = new System.Windows.Forms.OpenFileDialog();
            this.table = new System.Windows.Forms.DataGridView();
            this.target = new System.Windows.Forms.TextBox();
            this.Clear = new System.Windows.Forms.Button();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.Go = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.xls_out = new System.Windows.Forms.SaveFileDialog();
            this.xls_in = new System.Windows.Forms.OpenFileDialog();
            this.ADButton = new System.Windows.Forms.Button();
            this.RegButton = new System.Windows.Forms.Button();
            this.XLButton = new System.Windows.Forms.Button();
            this.HintTarget = new System.Windows.Forms.Button();
            this.HintOpen = new System.Windows.Forms.Button();
            this.Hint0 = new System.Windows.Forms.Button();
            this.hintUseTempl1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.table)).BeginInit();
            this.SuspendLayout();
            // 
            // reg
            // 
            this.reg.Filter = "Registry files|*.reg";
            this.reg.Title = "Open the Registry pattern file";
            this.reg.FileOk += new System.ComponentModel.CancelEventHandler(this.reg_FileOk);
            // 
            // table
            // 
            this.table.AllowUserToAddRows = false;
            this.table.AllowUserToOrderColumns = true;
            this.table.AllowUserToResizeRows = false;
            this.table.BackgroundColor = System.Drawing.Color.White;
            this.table.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.table.Dock = System.Windows.Forms.DockStyle.Fill;
            this.table.Location = new System.Drawing.Point(0, 0);
            this.table.Name = "table";
            this.table.RowHeadersWidth = 80;
            this.table.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.table.Size = new System.Drawing.Size(784, 362);
            this.table.TabIndex = 0;
            this.table.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.table_CellContentClick);
            this.table.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.table_CellContentDoubleClick);
            this.table.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.table_RowsAdded);
            // 
            // target
            // 
            this.target.BackColor = System.Drawing.SystemColors.Control;
            this.target.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.target.Location = new System.Drawing.Point(1, 22);
            this.target.Multiline = true;
            this.target.Name = "target";
            this.target.Size = new System.Drawing.Size(79, 19);
            this.target.TabIndex = 1;
            this.target.Visible = false;
            this.target.TextChanged += new System.EventHandler(this.target_TextChanged);
            // 
            // Clear
            // 
            this.Clear.Location = new System.Drawing.Point(1, 86);
            this.Clear.Name = "Clear";
            this.Clear.Size = new System.Drawing.Size(79, 22);
            this.Clear.TabIndex = 3;
            this.Clear.Text = "Clear";
            this.Clear.UseVisualStyleBackColor = true;
            this.Clear.Visible = false;
            this.Clear.Click += new System.EventHandler(this.Clear_Click);
            // 
            // worker
            // 
            this.worker.WorkerReportsProgress = true;
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // Go
            // 
            this.Go.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.Go.Location = new System.Drawing.Point(1, 42);
            this.Go.Name = "Go";
            this.Go.Size = new System.Drawing.Size(79, 22);
            this.Go.TabIndex = 4;
            this.Go.Text = "Go";
            this.Go.UseVisualStyleBackColor = true;
            this.Go.Visible = false;
            this.Go.Click += new System.EventHandler(this.Go_Click);
            // 
            // Save
            // 
            this.Save.BackColor = System.Drawing.Color.Transparent;
            this.Save.Location = new System.Drawing.Point(1, 64);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(79, 22);
            this.Save.TabIndex = 5;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = false;
            this.Save.Visible = false;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // xls_out
            // 
            this.xls_out.Filter = "Excel Files|*.xlsx";
            this.xls_out.OverwritePrompt = false;
            this.xls_out.Title = "Export to an Excel file";
            this.xls_out.FileOk += new System.ComponentModel.CancelEventHandler(this.xls_out_FileOk);
            // 
            // xls_in
            // 
            this.xls_in.Filter = "Excel Files|*.xlsx";
            this.xls_in.Title = "Open previous results from an Excel File";
            this.xls_in.FileOk += new System.ComponentModel.CancelEventHandler(this.xls_in_FileOk);
            // 
            // ADButton
            // 
            this.ADButton.FlatAppearance.BorderSize = 0;
            this.ADButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ADButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ADButton.Image = global::AuditSec.Properties.Resources.Col;
            this.ADButton.Location = new System.Drawing.Point(1, 1);
            this.ADButton.Name = "ADButton";
            this.ADButton.Size = new System.Drawing.Size(39, 19);
            this.ADButton.TabIndex = 6;
            this.ADButton.UseVisualStyleBackColor = true;
            this.ADButton.Visible = false;
            this.ADButton.Click += new System.EventHandler(this.TargetButton_Click);
            // 
            // RegButton
            // 
            this.RegButton.FlatAppearance.BorderSize = 0;
            this.RegButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RegButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RegButton.Image = global::AuditSec.Properties.Resources.reg1;
            this.RegButton.Location = new System.Drawing.Point(1, 1);
            this.RegButton.Name = "RegButton";
            this.RegButton.Size = new System.Drawing.Size(79, 19);
            this.RegButton.TabIndex = 7;
            this.RegButton.UseVisualStyleBackColor = true;
            this.RegButton.Click += new System.EventHandler(this.RegButton_Click);
            // 
            // XLButton
            // 
            this.XLButton.FlatAppearance.BorderSize = 0;
            this.XLButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.XLButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XLButton.Image = global::AuditSec.Properties.Resources.open;
            this.XLButton.Location = new System.Drawing.Point(40, 1);
            this.XLButton.Name = "XLButton";
            this.XLButton.Size = new System.Drawing.Size(39, 19);
            this.XLButton.TabIndex = 8;
            this.XLButton.UseVisualStyleBackColor = true;
            this.XLButton.Visible = false;
            this.XLButton.Click += new System.EventHandler(this.XlsButton_Click);
            // 
            // HintTarget
            // 
            this.HintTarget.BackColor = System.Drawing.SystemColors.Control;
            this.HintTarget.FlatAppearance.BorderSize = 0;
            this.HintTarget.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.HintTarget.Font = new System.Drawing.Font("Segoe Script", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HintTarget.Image = global::AuditSec.Properties.Resources.l_arr22;
            this.HintTarget.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.HintTarget.Location = new System.Drawing.Point(80, 22);
            this.HintTarget.Name = "HintTarget";
            this.HintTarget.Size = new System.Drawing.Size(703, 24);
            this.HintTarget.TabIndex = 9;
            this.HintTarget.Text = "     type in this box, the name of the machine you want to add to the target list" +
                " - or paste a <CR> separated list";
            this.HintTarget.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.HintTarget.UseVisualStyleBackColor = false;
            this.HintTarget.Visible = false;
            this.HintTarget.Click += new System.EventHandler(this.HintTarget_Click);
            // 
            // HintOpen
            // 
            this.HintOpen.BackColor = System.Drawing.SystemColors.Control;
            this.HintOpen.FlatAppearance.BorderSize = 0;
            this.HintOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.HintOpen.Font = new System.Drawing.Font("Segoe Script", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HintOpen.Image = global::AuditSec.Properties.Resources.l_arr22;
            this.HintOpen.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.HintOpen.Location = new System.Drawing.Point(80, 1);
            this.HintOpen.Name = "HintOpen";
            this.HintOpen.Size = new System.Drawing.Size(703, 24);
            this.HintOpen.TabIndex = 10;
            this.HintOpen.Text = "     Click the \'Computers\' icon to browse AD or click the \'Open\' icon to load an " +
                "Excel file";
            this.HintOpen.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.HintOpen.UseVisualStyleBackColor = false;
            this.HintOpen.Visible = false;
            this.HintOpen.Click += new System.EventHandler(this.HintOpen_Click);
            // 
            // Hint0
            // 
            this.Hint0.BackColor = System.Drawing.Color.White;
            this.Hint0.FlatAppearance.BorderSize = 0;
            this.Hint0.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Hint0.Font = new System.Drawing.Font("Segoe Script", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Hint0.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Hint0.Location = new System.Drawing.Point(1, 114);
            this.Hint0.Name = "Hint0";
            this.Hint0.Size = new System.Drawing.Size(782, 129);
            this.Hint0.TabIndex = 11;
            this.Hint0.Text = "     This tool scans remote computers\' WMI repository for registry values,\r\nand c" +
                "onsolidates the retrieved data into a table that can be exported to Excel.\r\n";
            this.Hint0.UseVisualStyleBackColor = false;
            this.Hint0.Click += new System.EventHandler(this.Hint0_Click);
            // 
            // hintUseTempl1
            // 
            this.hintUseTempl1.BackColor = System.Drawing.SystemColors.Control;
            this.hintUseTempl1.FlatAppearance.BorderSize = 0;
            this.hintUseTempl1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.hintUseTempl1.Font = new System.Drawing.Font("Segoe Script", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hintUseTempl1.Image = global::AuditSec.Properties.Resources.r_arr22;
            this.hintUseTempl1.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.hintUseTempl1.Location = new System.Drawing.Point(80, 0);
            this.hintUseTempl1.Name = "hintUseTempl1";
            this.hintUseTempl1.Size = new System.Drawing.Size(703, 24);
            this.hintUseTempl1.TabIndex = 12;
            this.hintUseTempl1.Text = "     Use template \'IE Toolbars\'";
            this.hintUseTempl1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.hintUseTempl1.UseVisualStyleBackColor = false;
            this.hintUseTempl1.Click += new System.EventHandler(this.hintUseTempl1_Click);
            // 
            // RegCollector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 362);
            this.Controls.Add(this.hintUseTempl1);
            this.Controls.Add(this.Hint0);
            this.Controls.Add(this.HintOpen);
            this.Controls.Add(this.HintTarget);
            this.Controls.Add(this.XLButton);
            this.Controls.Add(this.Clear);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.Go);
            this.Controls.Add(this.target);
            this.Controls.Add(this.ADButton);
            this.Controls.Add(this.RegButton);
            this.Controls.Add(this.table);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RegCollector";
            this.Text = "Multiple PC Registry Collector";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RegCollector_FormClosed);
            this.Load += new System.EventHandler(this.RegCollector_Load);
            this.ResizeEnd += new System.EventHandler(this.RegCollector_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.table)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog reg;
        private System.Windows.Forms.DataGridView table;
        private System.Windows.Forms.TextBox target;
        private System.Windows.Forms.Button Clear;
        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.Button Go;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.SaveFileDialog xls_out;
        private System.Windows.Forms.OpenFileDialog xls_in;
        private System.Windows.Forms.Button ADButton;
        private System.Windows.Forms.Button RegButton;
        private System.Windows.Forms.Button XLButton;
        private System.Windows.Forms.Button HintTarget;
        private System.Windows.Forms.Button HintOpen;
        private System.Windows.Forms.Button Hint0;
        private System.Windows.Forms.Button hintUseTempl1;
    }
}