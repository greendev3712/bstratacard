namespace intellaConsole {
    partial class DirectoryViewerPanel {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DirectoryViewerPanel));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.cmpDirectoryPanel = new System.Windows.Forms.Panel();
            this.radioButtonSearchDirectoryByExt = new System.Windows.Forms.RadioButton();
            this.buttonShowAll = new System.Windows.Forms.Button();
            this.textBoxQuickSearch = new System.Windows.Forms.TextBox();
            this.radioButtonSearchDirectoryByBoth = new System.Windows.Forms.RadioButton();
            this.dataGridViewDirectory = new System.Windows.Forms.DataGridView();
            this.cmpDataGridViewDirectoryColumnContactName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cmpDataGridViewDirectoryColumnStatus = new System.Windows.Forms.DataGridViewImageColumn();
            this.cmpDataGridViewDirectoryColumnSendMessage = new System.Windows.Forms.DataGridViewImageColumn();
            this.cmpDataGridViewDirectoryColumnVMail = new System.Windows.Forms.DataGridViewImageColumn();
            this.cmpDataGridViewDirectoryColumnNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cmpDataGridViewDirectoryColumnDepartment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.radioButtonSearchDirectoryByLast = new System.Windows.Forms.RadioButton();
            this.radioButtonSearchDirectoryByFirst = new System.Windows.Forms.RadioButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.cmpDirectoryPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDirectory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // cmpDirectoryPanel
            // 
            this.cmpDirectoryPanel.Controls.Add(this.radioButtonSearchDirectoryByExt);
            this.cmpDirectoryPanel.Controls.Add(this.buttonShowAll);
            this.cmpDirectoryPanel.Controls.Add(this.textBoxQuickSearch);
            this.cmpDirectoryPanel.Controls.Add(this.radioButtonSearchDirectoryByBoth);
            this.cmpDirectoryPanel.Controls.Add(this.dataGridViewDirectory);
            this.cmpDirectoryPanel.Controls.Add(this.radioButtonSearchDirectoryByLast);
            this.cmpDirectoryPanel.Controls.Add(this.radioButtonSearchDirectoryByFirst);
            this.cmpDirectoryPanel.Controls.Add(this.pictureBox1);
            this.cmpDirectoryPanel.Location = new System.Drawing.Point(30, 32);
            this.cmpDirectoryPanel.Name = "cmpDirectoryPanel";
            this.cmpDirectoryPanel.Size = new System.Drawing.Size(700, 422);
            this.cmpDirectoryPanel.TabIndex = 62;
            // 
            // radioButtonSearchDirectoryByExt
            // 
            this.radioButtonSearchDirectoryByExt.AutoSize = true;
            this.radioButtonSearchDirectoryByExt.BackColor = System.Drawing.Color.Black;
            this.radioButtonSearchDirectoryByExt.Font = new System.Drawing.Font("Traditional Arabic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonSearchDirectoryByExt.Location = new System.Drawing.Point(519, 7);
            this.radioButtonSearchDirectoryByExt.Name = "radioButtonSearchDirectoryByExt";
            this.radioButtonSearchDirectoryByExt.Size = new System.Drawing.Size(14, 13);
            this.radioButtonSearchDirectoryByExt.TabIndex = 84;
            this.radioButtonSearchDirectoryByExt.Tag = "Both";
            this.radioButtonSearchDirectoryByExt.UseVisualStyleBackColor = false;
            // 
            // buttonShowAll
            // 
            this.buttonShowAll.BackColor = System.Drawing.Color.Black;
            this.buttonShowAll.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.buttonShowAll.FlatAppearance.BorderSize = 0;
            this.buttonShowAll.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
            this.buttonShowAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Black;
            this.buttonShowAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonShowAll.Font = new System.Drawing.Font("Traditional Arabic", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonShowAll.ForeColor = System.Drawing.Color.Transparent;
            this.buttonShowAll.Image = ((System.Drawing.Image)(resources.GetObject("buttonShowAll.Image")));
            this.buttonShowAll.Location = new System.Drawing.Point(614, 3);
            this.buttonShowAll.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.buttonShowAll.Name = "buttonShowAll";
            this.buttonShowAll.Size = new System.Drawing.Size(85, 24);
            this.buttonShowAll.TabIndex = 82;
            this.buttonShowAll.UseVisualStyleBackColor = true;
            this.buttonShowAll.Click += new System.EventHandler(this.buttonShowAll_Click);
            // 
            // textBoxQuickSearch
            // 
            this.textBoxQuickSearch.Location = new System.Drawing.Point(3, 3);
            this.textBoxQuickSearch.Name = "textBoxQuickSearch";
            this.textBoxQuickSearch.Size = new System.Drawing.Size(333, 20);
            this.textBoxQuickSearch.TabIndex = 56;
            this.textBoxQuickSearch.TextChanged += new System.EventHandler(this.textBoxQuickSearch_TextChanged);
            // 
            // radioButtonSearchDirectoryByBoth
            // 
            this.radioButtonSearchDirectoryByBoth.AutoSize = true;
            this.radioButtonSearchDirectoryByBoth.BackColor = System.Drawing.Color.Black;
            this.radioButtonSearchDirectoryByBoth.Font = new System.Drawing.Font("Traditional Arabic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonSearchDirectoryByBoth.Location = new System.Drawing.Point(578, 7);
            this.radioButtonSearchDirectoryByBoth.Name = "radioButtonSearchDirectoryByBoth";
            this.radioButtonSearchDirectoryByBoth.Size = new System.Drawing.Size(14, 13);
            this.radioButtonSearchDirectoryByBoth.TabIndex = 60;
            this.radioButtonSearchDirectoryByBoth.Tag = "Both";
            this.radioButtonSearchDirectoryByBoth.UseVisualStyleBackColor = false;
            this.radioButtonSearchDirectoryByBoth.Click += new System.EventHandler(this.textBoxQuickSearch_TextChanged);
            // 
            // dataGridViewDirectory
            // 
            this.dataGridViewDirectory.BackgroundColor = System.Drawing.Color.Gray;
            this.dataGridViewDirectory.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewDirectory.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridViewDirectory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDirectory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cmpDataGridViewDirectoryColumnContactName,
            this.cmpDataGridViewDirectoryColumnStatus,
            this.cmpDataGridViewDirectoryColumnSendMessage,
            this.cmpDataGridViewDirectoryColumnVMail,
            this.cmpDataGridViewDirectoryColumnNumber,
            this.cmpDataGridViewDirectoryColumnDepartment});
            this.dataGridViewDirectory.Location = new System.Drawing.Point(2, 27);
            this.dataGridViewDirectory.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.dataGridViewDirectory.MultiSelect = false;
            this.dataGridViewDirectory.Name = "dataGridViewDirectory";
            this.dataGridViewDirectory.ReadOnly = true;
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Traditional Arabic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewDirectory.RowHeadersDefaultCellStyle = dataGridViewCellStyle11;
            this.dataGridViewDirectory.RowHeadersVisible = false;
            this.dataGridViewDirectory.RowHeadersWidth = 20;
            dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.Color.DarkGoldenrod;
            dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewDirectory.RowsDefaultCellStyle = dataGridViewCellStyle12;
            this.dataGridViewDirectory.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataGridViewDirectory.RowTemplate.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.dataGridViewDirectory.RowTemplate.ReadOnly = true;
            this.dataGridViewDirectory.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewDirectory.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridViewDirectory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewDirectory.Size = new System.Drawing.Size(697, 393);
            this.dataGridViewDirectory.TabIndex = 1;
            this.dataGridViewDirectory.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewDirectory_CellContentClick);
            this.dataGridViewDirectory.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewDirectory_CellDoubleClick);
            this.dataGridViewDirectory.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridViewDirectory_CellMouseDown);
            // 
            // cmpDataGridViewDirectoryColumnContactName
            // 
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.cmpDataGridViewDirectoryColumnContactName.DefaultCellStyle = dataGridViewCellStyle8;
            this.cmpDataGridViewDirectoryColumnContactName.Frozen = true;
            this.cmpDataGridViewDirectoryColumnContactName.HeaderText = "Name";
            this.cmpDataGridViewDirectoryColumnContactName.Name = "cmpDataGridViewDirectoryColumnContactName";
            this.cmpDataGridViewDirectoryColumnContactName.ReadOnly = true;
            this.cmpDataGridViewDirectoryColumnContactName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.cmpDataGridViewDirectoryColumnContactName.Width = 350;
            // 
            // cmpDataGridViewDirectoryColumnStatus
            // 
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.NullValue = "System.Drawing.Bitmap";
            this.cmpDataGridViewDirectoryColumnStatus.DefaultCellStyle = dataGridViewCellStyle9;
            this.cmpDataGridViewDirectoryColumnStatus.Frozen = true;
            this.cmpDataGridViewDirectoryColumnStatus.HeaderText = "Status";
            this.cmpDataGridViewDirectoryColumnStatus.Image = global::intellaConsole.Properties.Resources.icon_blank_transparent;
            this.cmpDataGridViewDirectoryColumnStatus.Name = "cmpDataGridViewDirectoryColumnStatus";
            this.cmpDataGridViewDirectoryColumnStatus.ReadOnly = true;
            this.cmpDataGridViewDirectoryColumnStatus.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.cmpDataGridViewDirectoryColumnStatus.Width = 35;
            // 
            // cmpDataGridViewDirectoryColumnSendMessage
            // 
            this.cmpDataGridViewDirectoryColumnSendMessage.Frozen = true;
            this.cmpDataGridViewDirectoryColumnSendMessage.HeaderText = "Send Message";
            this.cmpDataGridViewDirectoryColumnSendMessage.Name = "cmpDataGridViewDirectoryColumnSendMessage";
            this.cmpDataGridViewDirectoryColumnSendMessage.ReadOnly = true;
            // 
            // cmpDataGridViewDirectoryColumnVMail
            // 
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle10.NullValue = "System.Drawing.Bitmap";
            this.cmpDataGridViewDirectoryColumnVMail.DefaultCellStyle = dataGridViewCellStyle10;
            this.cmpDataGridViewDirectoryColumnVMail.Frozen = true;
            this.cmpDataGridViewDirectoryColumnVMail.HeaderText = "VM";
            this.cmpDataGridViewDirectoryColumnVMail.Image = global::intellaConsole.Properties.Resources.icon_blank_transparent;
            this.cmpDataGridViewDirectoryColumnVMail.Name = "cmpDataGridViewDirectoryColumnVMail";
            this.cmpDataGridViewDirectoryColumnVMail.ReadOnly = true;
            this.cmpDataGridViewDirectoryColumnVMail.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.cmpDataGridViewDirectoryColumnVMail.Width = 35;
            // 
            // cmpDataGridViewDirectoryColumnNumber
            // 
            this.cmpDataGridViewDirectoryColumnNumber.Frozen = true;
            this.cmpDataGridViewDirectoryColumnNumber.HeaderText = "Number";
            this.cmpDataGridViewDirectoryColumnNumber.Name = "cmpDataGridViewDirectoryColumnNumber";
            this.cmpDataGridViewDirectoryColumnNumber.ReadOnly = true;
            this.cmpDataGridViewDirectoryColumnNumber.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.cmpDataGridViewDirectoryColumnNumber.Width = 115;
            // 
            // cmpDataGridViewDirectoryColumnDepartment
            // 
            this.cmpDataGridViewDirectoryColumnDepartment.Frozen = true;
            this.cmpDataGridViewDirectoryColumnDepartment.HeaderText = "Department";
            this.cmpDataGridViewDirectoryColumnDepartment.Name = "cmpDataGridViewDirectoryColumnDepartment";
            this.cmpDataGridViewDirectoryColumnDepartment.ReadOnly = true;
            this.cmpDataGridViewDirectoryColumnDepartment.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.cmpDataGridViewDirectoryColumnDepartment.Width = 197;
            // 
            // radioButtonSearchDirectoryByLast
            // 
            this.radioButtonSearchDirectoryByLast.AutoSize = true;
            this.radioButtonSearchDirectoryByLast.BackColor = System.Drawing.Color.Black;
            this.radioButtonSearchDirectoryByLast.Font = new System.Drawing.Font("Traditional Arabic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonSearchDirectoryByLast.Location = new System.Drawing.Point(457, 7);
            this.radioButtonSearchDirectoryByLast.Name = "radioButtonSearchDirectoryByLast";
            this.radioButtonSearchDirectoryByLast.Size = new System.Drawing.Size(14, 13);
            this.radioButtonSearchDirectoryByLast.TabIndex = 59;
            this.radioButtonSearchDirectoryByLast.UseVisualStyleBackColor = false;
            this.radioButtonSearchDirectoryByLast.Click += new System.EventHandler(this.radioButtonSearchDirectoryByLast_Click);
            // 
            // radioButtonSearchDirectoryByFirst
            // 
            this.radioButtonSearchDirectoryByFirst.AutoSize = true;
            this.radioButtonSearchDirectoryByFirst.BackColor = System.Drawing.Color.Black;
            this.radioButtonSearchDirectoryByFirst.Checked = true;
            this.radioButtonSearchDirectoryByFirst.Font = new System.Drawing.Font("Traditional Arabic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonSearchDirectoryByFirst.Location = new System.Drawing.Point(388, 7);
            this.radioButtonSearchDirectoryByFirst.Name = "radioButtonSearchDirectoryByFirst";
            this.radioButtonSearchDirectoryByFirst.Size = new System.Drawing.Size(14, 13);
            this.radioButtonSearchDirectoryByFirst.TabIndex = 58;
            this.radioButtonSearchDirectoryByFirst.TabStop = true;
            this.radioButtonSearchDirectoryByFirst.UseVisualStyleBackColor = false;
            this.radioButtonSearchDirectoryByFirst.Click += new System.EventHandler(this.radioButtonSearchDirectoryByFirst_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(339, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(360, 24);
            this.pictureBox1.TabIndex = 83;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // DirectoryViewerPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 465);
            this.Controls.Add(this.cmpDirectoryPanel);
            this.Name = "DirectoryViewerPanel";
            this.Text = "DirectoryViewer";
            this.cmpDirectoryPanel.ResumeLayout(false);
            this.cmpDirectoryPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDirectory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel cmpDirectoryPanel;
        private System.Windows.Forms.TextBox textBoxQuickSearch;
        private System.Windows.Forms.RadioButton radioButtonSearchDirectoryByBoth;
        private System.Windows.Forms.DataGridView dataGridViewDirectory;
        private System.Windows.Forms.RadioButton radioButtonSearchDirectoryByLast;
        private System.Windows.Forms.RadioButton radioButtonSearchDirectoryByFirst;
        private System.Windows.Forms.Button buttonShowAll;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.RadioButton radioButtonSearchDirectoryByExt;
        private System.Windows.Forms.DataGridViewTextBoxColumn cmpDataGridViewDirectoryColumnContactName;
        private System.Windows.Forms.DataGridViewImageColumn cmpDataGridViewDirectoryColumnStatus;
        private System.Windows.Forms.DataGridViewImageColumn cmpDataGridViewDirectoryColumnSendMessage;
        private System.Windows.Forms.DataGridViewImageColumn cmpDataGridViewDirectoryColumnVMail;
        private System.Windows.Forms.DataGridViewTextBoxColumn cmpDataGridViewDirectoryColumnNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn cmpDataGridViewDirectoryColumnDepartment;
    }
}