namespace intellaQueue
{
    partial class DispositionCodesHistoryForm
    {
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.cmpDataGridViewPanel = new System.Windows.Forms.Panel();
            this.cmpMainPanel = new System.Windows.Forms.Panel();
            this.cmpBottomPanel = new System.Windows.Forms.Panel();
            this.cmpBottomPanel_RightPanel = new System.Windows.Forms.Panel();
            this.cmpBottomPanel_CenterPanel = new System.Windows.Forms.Panel();
            this.cmpBottomPanel_LeftPanel = new System.Windows.Forms.Panel();
            this.cmpCloseButton = new System.Windows.Forms.Button();
            this.cmpSaveButton = new System.Windows.Forms.Button();
            this.cmpDispositionGridView = new Lib.LiveDataGridView();
            this.cmpDataGridViewPanel.SuspendLayout();
            this.cmpMainPanel.SuspendLayout();
            this.cmpBottomPanel.SuspendLayout();
            this.cmpBottomPanel_RightPanel.SuspendLayout();
            this.cmpBottomPanel_LeftPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmpDispositionGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // cmpDataGridViewPanel
            // 
            this.cmpDataGridViewPanel.Controls.Add(this.cmpDispositionGridView);
            this.cmpDataGridViewPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmpDataGridViewPanel.Location = new System.Drawing.Point(0, 0);
            this.cmpDataGridViewPanel.Name = "cmpDataGridViewPanel";
            this.cmpDataGridViewPanel.Size = new System.Drawing.Size(930, 315);
            this.cmpDataGridViewPanel.TabIndex = 1;
            // 
            // cmpMainPanel
            // 
            this.cmpMainPanel.AutoSize = true;
            this.cmpMainPanel.Controls.Add(this.cmpBottomPanel);
            this.cmpMainPanel.Controls.Add(this.cmpDataGridViewPanel);
            this.cmpMainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpMainPanel.Location = new System.Drawing.Point(0, 0);
            this.cmpMainPanel.Name = "cmpMainPanel";
            this.cmpMainPanel.Size = new System.Drawing.Size(930, 460);
            this.cmpMainPanel.TabIndex = 2;
            this.cmpMainPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // cmpBottomPanel
            // 
            this.cmpBottomPanel.Controls.Add(this.cmpBottomPanel_RightPanel);
            this.cmpBottomPanel.Controls.Add(this.cmpBottomPanel_CenterPanel);
            this.cmpBottomPanel.Controls.Add(this.cmpBottomPanel_LeftPanel);
            this.cmpBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cmpBottomPanel.Location = new System.Drawing.Point(0, 406);
            this.cmpBottomPanel.Name = "cmpBottomPanel";
            this.cmpBottomPanel.Size = new System.Drawing.Size(930, 54);
            this.cmpBottomPanel.TabIndex = 1;
            // 
            // cmpBottomPanel_RightPanel
            // 
            this.cmpBottomPanel_RightPanel.Controls.Add(this.cmpSaveButton);
            this.cmpBottomPanel_RightPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.cmpBottomPanel_RightPanel.Location = new System.Drawing.Point(781, 0);
            this.cmpBottomPanel_RightPanel.Name = "cmpBottomPanel_RightPanel";
            this.cmpBottomPanel_RightPanel.Size = new System.Drawing.Size(149, 54);
            this.cmpBottomPanel_RightPanel.TabIndex = 1;
            // 
            // cmpBottomPanel_CenterPanel
            // 
            this.cmpBottomPanel_CenterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpBottomPanel_CenterPanel.Location = new System.Drawing.Point(149, 0);
            this.cmpBottomPanel_CenterPanel.Name = "cmpBottomPanel_CenterPanel";
            this.cmpBottomPanel_CenterPanel.Size = new System.Drawing.Size(781, 54);
            this.cmpBottomPanel_CenterPanel.TabIndex = 1;
            // 
            // cmpBottomPanel_LeftPanel
            // 
            this.cmpBottomPanel_LeftPanel.Controls.Add(this.cmpCloseButton);
            this.cmpBottomPanel_LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.cmpBottomPanel_LeftPanel.Location = new System.Drawing.Point(0, 0);
            this.cmpBottomPanel_LeftPanel.Name = "cmpBottomPanel_LeftPanel";
            this.cmpBottomPanel_LeftPanel.Size = new System.Drawing.Size(149, 54);
            this.cmpBottomPanel_LeftPanel.TabIndex = 0;
            // 
            // cmpCloseButton
            // 
            this.cmpCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmpCloseButton.Location = new System.Drawing.Point(36, 17);
            this.cmpCloseButton.Name = "cmpCloseButton";
            this.cmpCloseButton.Size = new System.Drawing.Size(75, 23);
            this.cmpCloseButton.TabIndex = 1;
            this.cmpCloseButton.Text = "Close";
            this.cmpCloseButton.UseVisualStyleBackColor = true;
            this.cmpCloseButton.Click += new System.EventHandler(this.cmpCloseButton_Click);
            // 
            // cmpSaveButton
            // 
            this.cmpSaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmpSaveButton.Location = new System.Drawing.Point(39, 17);
            this.cmpSaveButton.Name = "cmpSaveButton";
            this.cmpSaveButton.Size = new System.Drawing.Size(75, 23);
            this.cmpSaveButton.TabIndex = 2;
            this.cmpSaveButton.Text = "Save";
            this.cmpSaveButton.UseVisualStyleBackColor = true;
            this.cmpSaveButton.Click += new System.EventHandler(this.cmpSaveButton_Click);
            // 
            // cmpDispositionGridView
            // 
            this.cmpDispositionGridView.AllowUserToAddRows = false;
            this.cmpDispositionGridView.AllowUserToDeleteRows = false;
            this.cmpDispositionGridView.AllowUserToResizeColumns = false;
            this.cmpDispositionGridView.AllowUserToResizeRows = false;
            this.cmpDispositionGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.cmpDispositionGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            this.cmpDispositionGridView.BackgroundColor = System.Drawing.SystemColors.ControlDarkDark;
            this.cmpDispositionGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.cmpDispositionGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(193)))), ((int)(((byte)(193)))), ((int)(((byte)(193)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.5F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.cmpDispositionGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.cmpDispositionGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(173)))), ((int)(((byte)(173)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.5F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(143)))), ((int)(((byte)(143)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.cmpDispositionGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.cmpDispositionGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpDispositionGridView.EnableHeadersVisualStyles = false;
            this.cmpDispositionGridView.GridColor = System.Drawing.SystemColors.ControlDarkDark;
            this.cmpDispositionGridView.Location = new System.Drawing.Point(0, 0);
            this.cmpDispositionGridView.MultiSelect = false;
            this.cmpDispositionGridView.Name = "cmpDispositionGridView";
            this.cmpDispositionGridView.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 18.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.cmpDispositionGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.cmpDispositionGridView.RowHeadersVisible = false;
            this.cmpDispositionGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.cmpDispositionGridView.RowTemplate.Height = 13;
            this.cmpDispositionGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.cmpDispositionGridView.ShowCellToolTips = false;
            this.cmpDispositionGridView.ShowEditingIcon = false;
            this.cmpDispositionGridView.Size = new System.Drawing.Size(930, 315);
            this.cmpDispositionGridView.TabIndex = 0;
            this.cmpDispositionGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.liveDataGridView1_CellContentClick);
            // 
            // DispositionCodesHistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(930, 460);
            this.Controls.Add(this.cmpMainPanel);
            this.MinimumSize = new System.Drawing.Size(400, 200);
            this.Name = "DispositionCodesHistoryForm";
            this.Text = "Disposition History";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DispositionCodesHistoryForm_FormClosing);
            this.Load += new System.EventHandler(this.DispositionCodesHistoryForm_Load);
            this.VisibleChanged += new System.EventHandler(this.DispositionCodesHistoryForm_VisibleChanged);
            this.Resize += new System.EventHandler(this.DispositionCodesHistoryForm_Resize);
            this.cmpDataGridViewPanel.ResumeLayout(false);
            this.cmpMainPanel.ResumeLayout(false);
            this.cmpBottomPanel.ResumeLayout(false);
            this.cmpBottomPanel_RightPanel.ResumeLayout(false);
            this.cmpBottomPanel_LeftPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmpDispositionGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Lib.LiveDataGridView cmpDispositionGridView;
        private System.Windows.Forms.Panel cmpDataGridViewPanel;
        private System.Windows.Forms.Panel cmpMainPanel;
        private System.Windows.Forms.Panel cmpBottomPanel;
        private System.Windows.Forms.Panel cmpBottomPanel_RightPanel;
        private System.Windows.Forms.Panel cmpBottomPanel_CenterPanel;
        private System.Windows.Forms.Panel cmpBottomPanel_LeftPanel;
        private System.Windows.Forms.Button cmpSaveButton;
        private System.Windows.Forms.Button cmpCloseButton;
    }
}