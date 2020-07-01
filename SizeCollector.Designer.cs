namespace AuditSec
{
    partial class SizeCollector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SizeCollector));
            this.target = new System.Windows.Forms.TextBox();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.xls_out = new System.Windows.Forms.SaveFileDialog();
            this.xls_in = new System.Windows.Forms.OpenFileDialog();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.Hint = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.Hint2 = new System.Windows.Forms.Button();
            this.LoadButton = new System.Windows.Forms.Button();
            this.ClearButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.Go = new System.Windows.Forms.Button();
            this.table = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.table)).BeginInit();
            this.SuspendLayout();
            // 
            // target
            // 
            this.target.BackColor = System.Drawing.Color.White;
            this.target.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.target.Location = new System.Drawing.Point(1, 26);
            this.target.Multiline = true;
            this.target.Name = "target";
            this.target.Size = new System.Drawing.Size(79, 21);
            this.target.TabIndex = 1;
            this.target.Click += new System.EventHandler(this.target_Click);
            this.target.TextChanged += new System.EventHandler(this.target_TextChanged);
            // 
            // worker
            // 
            this.worker.WorkerReportsProgress = true;
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.worker_DoWork);
            this.worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.worker_ProgressChanged);
            this.worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
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
            // BrowseButton
            // 
            this.BrowseButton.FlatAppearance.BorderSize = 0;
            this.BrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BrowseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BrowseButton.Image = global::AuditSec.Properties.Resources.Col;
            this.BrowseButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.BrowseButton.Location = new System.Drawing.Point(0, -1);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(80, 22);
            this.BrowseButton.TabIndex = 6;
            this.BrowseButton.Text = "  Browse";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // Hint
            // 
            this.Hint.BackColor = System.Drawing.SystemColors.Control;
            this.Hint.FlatAppearance.BorderSize = 0;
            this.Hint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Hint.Font = new System.Drawing.Font("Segoe Script", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Hint.Image = global::AuditSec.Properties.Resources.l_arr22;
            this.Hint.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.Hint.Location = new System.Drawing.Point(80, 2);
            this.Hint.Name = "Hint";
            this.Hint.Size = new System.Drawing.Size(703, 45);
            this.Hint.TabIndex = 7;
            this.Hint.Text = "     type in this box, the name of the machine you want to add to the target list" +
                " - or paste a <CR> separated list";
            this.Hint.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.Hint.UseVisualStyleBackColor = false;
            this.Hint.Click += new System.EventHandler(this.Hint_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.buttonPanel);
            this.splitContainer1.Panel1MinSize = 20;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Go);
            this.splitContainer1.Panel2.Controls.Add(this.target);
            this.splitContainer1.Panel2.Controls.Add(this.Hint);
            this.splitContainer1.Panel2.Controls.Add(this.table);
            this.splitContainer1.Panel2MinSize = 0;
            this.splitContainer1.Size = new System.Drawing.Size(784, 362);
            this.splitContainer1.SplitterDistance = 25;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 9;
            // 
            // buttonPanel
            // 
            this.buttonPanel.BackColor = System.Drawing.SystemColors.Control;
            this.buttonPanel.Controls.Add(this.Hint2);
            this.buttonPanel.Controls.Add(this.BrowseButton);
            this.buttonPanel.Controls.Add(this.LoadButton);
            this.buttonPanel.Controls.Add(this.ClearButton);
            this.buttonPanel.Controls.Add(this.SaveButton);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonPanel.Location = new System.Drawing.Point(0, 0);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(784, 22);
            this.buttonPanel.TabIndex = 9;
            // 
            // Hint2
            // 
            this.Hint2.BackColor = System.Drawing.SystemColors.Control;
            this.Hint2.FlatAppearance.BorderSize = 0;
            this.Hint2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Hint2.Font = new System.Drawing.Font("Segoe Script", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Hint2.Image = global::AuditSec.Properties.Resources.l_arr22;
            this.Hint2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Hint2.Location = new System.Drawing.Point(160, -1);
            this.Hint2.Name = "Hint2";
            this.Hint2.Size = new System.Drawing.Size(624, 22);
            this.Hint2.TabIndex = 8;
            this.Hint2.Text = "     click these buttons to browse for machines or load previous results";
            this.Hint2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Hint2.UseVisualStyleBackColor = false;
            this.Hint2.Click += new System.EventHandler(this.Hint2_Click);
            // 
            // LoadButton
            // 
            this.LoadButton.FlatAppearance.BorderSize = 0;
            this.LoadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.LoadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoadButton.Image = global::AuditSec.Properties.Resources.Col;
            this.LoadButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadButton.Location = new System.Drawing.Point(80, -1);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(80, 22);
            this.LoadButton.TabIndex = 10;
            this.LoadButton.Text = "Load";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.Load_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.FlatAppearance.BorderSize = 0;
            this.ClearButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ClearButton.Image = global::AuditSec.Properties.Resources.Axe;
            this.ClearButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ClearButton.Location = new System.Drawing.Point(160, -1);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(65, 22);
            this.ClearButton.TabIndex = 3;
            this.ClearButton.Text = "Clear";
            this.ClearButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.Clear_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.BackColor = System.Drawing.Color.Transparent;
            this.SaveButton.FlatAppearance.BorderSize = 0;
            this.SaveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SaveButton.Image = global::AuditSec.Properties.Resources.save22;
            this.SaveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SaveButton.Location = new System.Drawing.Point(225, -1);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(65, 22);
            this.SaveButton.TabIndex = 5;
            this.SaveButton.Text = "Save";
            this.SaveButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.SaveButton.UseVisualStyleBackColor = false;
            this.SaveButton.Click += new System.EventHandler(this.Save_Click);
            // 
            // Go
            // 
            this.Go.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.Go.FlatAppearance.BorderSize = 0;
            this.Go.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Go.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Go.Image = global::AuditSec.Properties.Resources.Run;
            this.Go.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Go.Location = new System.Drawing.Point(3, 3);
            this.Go.Name = "Go";
            this.Go.Size = new System.Drawing.Size(77, 22);
            this.Go.TabIndex = 4;
            this.Go.Text = "Go";
            this.Go.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Go.UseVisualStyleBackColor = true;
            this.Go.Click += new System.EventHandler(this.Go_Click);
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
            this.table.Size = new System.Drawing.Size(784, 336);
            this.table.TabIndex = 1;
            this.table.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.table_CellDoubleClick);
            this.table.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.table_RowsAdded);
            this.table.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.table_RowsRemoved);
            this.table.SelectionChanged += new System.EventHandler(this.table_SelectionChanged);
            // 
            // SizeCollector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 362);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SizeCollector";
            this.Text = "Multiple PC Profile Size Collector";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SizeCollector_FormClosed);
            this.Load += new System.EventHandler(this.SizeCollector_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.table)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox target;
        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.SaveFileDialog xls_out;
        private System.Windows.Forms.OpenFileDialog xls_in;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.Button Hint;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button Go;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.DataGridView table;
        private System.Windows.Forms.Button Hint2;
        private System.Windows.Forms.Button LoadButton;
    }
}