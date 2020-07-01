namespace AuditSec
{
    partial class WMICollector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WMICollector));
            this.table = new System.Windows.Forms.DataGridView();
            this.target = new System.Windows.Forms.TextBox();
            this.Clear = new System.Windows.Forms.Button();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.Go = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.xls_out = new System.Windows.Forms.SaveFileDialog();
            this.xls_in = new System.Windows.Forms.OpenFileDialog();
            this.TargetButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.table)).BeginInit();
            this.SuspendLayout();
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
            // TargetButton
            // 
            this.TargetButton.FlatAppearance.BorderSize = 0;
            this.TargetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TargetButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TargetButton.Image = global::AuditSec.Properties.Resources.Col;
            this.TargetButton.Location = new System.Drawing.Point(1, 1);
            this.TargetButton.Name = "TargetButton";
            this.TargetButton.Size = new System.Drawing.Size(79, 19);
            this.TargetButton.TabIndex = 6;
            this.TargetButton.UseVisualStyleBackColor = true;
            this.TargetButton.Visible = false;
            this.TargetButton.Click += new System.EventHandler(this.TargetButton_Click);
            // 
            // WMICollector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 362);
            this.Controls.Add(this.Clear);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.Go);
            this.Controls.Add(this.target);
            this.Controls.Add(this.TargetButton);
            this.Controls.Add(this.table);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WMICollector";
            this.Text = "Multiple PC WMI Collector";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RegCollector_FormClosed);
            this.Load += new System.EventHandler(this.RegCollector_Load);
            ((System.ComponentModel.ISupportInitialize)(this.table)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView table;
        private System.Windows.Forms.TextBox target;
        private System.Windows.Forms.Button Clear;
        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.Button Go;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.SaveFileDialog xls_out;
        private System.Windows.Forms.OpenFileDialog xls_in;
        private System.Windows.Forms.Button TargetButton;
    }
}