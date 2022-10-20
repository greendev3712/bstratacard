namespace QueueLib
{
    partial class DispositionCodesForm
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
            this.cmpDispositionPanel = new System.Windows.Forms.Panel();
            this.cmpDispositionCode_CallAndCodePanel = new System.Windows.Forms.Panel();
            this.cmpDispositionComboBox = new System.Windows.Forms.ComboBox();
            this.cmpCurrentDispositionLabel = new System.Windows.Forms.Label();
            this.cmpSaveUpdatePanel = new System.Windows.Forms.TableLayoutPanel();
            this.cmpSaveUpdateData = new System.Windows.Forms.Button();
            this.cmpDispositionHistory = new System.Windows.Forms.Button();
            this.cmpDispositionPanel.SuspendLayout();
            this.cmpDispositionCode_CallAndCodePanel.SuspendLayout();
            this.cmpSaveUpdatePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmpDispositionPanel
            // 
            this.cmpDispositionPanel.AutoSize = true;
            this.cmpDispositionPanel.Controls.Add(this.cmpDispositionCode_CallAndCodePanel);
            this.cmpDispositionPanel.Controls.Add(this.cmpSaveUpdatePanel);
            this.cmpDispositionPanel.Location = new System.Drawing.Point(83, 41);
            this.cmpDispositionPanel.Margin = new System.Windows.Forms.Padding(1);
            this.cmpDispositionPanel.Name = "cmpDispositionPanel";
            this.cmpDispositionPanel.Size = new System.Drawing.Size(361, 56);
            this.cmpDispositionPanel.TabIndex = 0;
            // 
            // cmpDispositionCode_CallAndCodePanel
            // 
            this.cmpDispositionCode_CallAndCodePanel.Controls.Add(this.cmpDispositionComboBox);
            this.cmpDispositionCode_CallAndCodePanel.Controls.Add(this.cmpCurrentDispositionLabel);
            this.cmpDispositionCode_CallAndCodePanel.Location = new System.Drawing.Point(0, 0);
            this.cmpDispositionCode_CallAndCodePanel.Name = "cmpDispositionCode_CallAndCodePanel";
            this.cmpDispositionCode_CallAndCodePanel.Size = new System.Drawing.Size(358, 30);
            this.cmpDispositionCode_CallAndCodePanel.TabIndex = 1;
            this.cmpDispositionCode_CallAndCodePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.CmpDispositionCode_CallAndCodePanel_Paint);
            // 
            // cmpDispositionComboBox
            // 
            this.cmpDispositionComboBox.FormattingEnabled = true;
            this.cmpDispositionComboBox.Location = new System.Drawing.Point(100, 4);
            this.cmpDispositionComboBox.Name = "cmpDispositionComboBox";
            this.cmpDispositionComboBox.Size = new System.Drawing.Size(253, 21);
            this.cmpDispositionComboBox.TabIndex = 3;
            this.cmpDispositionComboBox.SelectedIndexChanged += new System.EventHandler(this.cmpDispositionComboBox_SelectedIndexChanged);
            // 
            // cmpCurrentDispositionLabel
            // 
            this.cmpCurrentDispositionLabel.AutoSize = true;
            this.cmpCurrentDispositionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpCurrentDispositionLabel.ForeColor = System.Drawing.Color.White;
            this.cmpCurrentDispositionLabel.Location = new System.Drawing.Point(4, 10);
            this.cmpCurrentDispositionLabel.Name = "cmpCurrentDispositionLabel";
            this.cmpCurrentDispositionLabel.Size = new System.Drawing.Size(92, 13);
            this.cmpCurrentDispositionLabel.TabIndex = 4;
            this.cmpCurrentDispositionLabel.Text = "Set Disposition";
            // 
            // cmpSaveUpdatePanel
            // 
            this.cmpSaveUpdatePanel.ColumnCount = 3;
            this.cmpSaveUpdatePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.cmpSaveUpdatePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.cmpSaveUpdatePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.cmpSaveUpdatePanel.Controls.Add(this.cmpSaveUpdateData, 1, 0);
            this.cmpSaveUpdatePanel.Controls.Add(this.cmpDispositionHistory, 2, 0);
            this.cmpSaveUpdatePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cmpSaveUpdatePanel.Location = new System.Drawing.Point(0, 27);
            this.cmpSaveUpdatePanel.Margin = new System.Windows.Forms.Padding(0);
            this.cmpSaveUpdatePanel.Name = "cmpSaveUpdatePanel";
            this.cmpSaveUpdatePanel.RowCount = 1;
            this.cmpSaveUpdatePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cmpSaveUpdatePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.cmpSaveUpdatePanel.Size = new System.Drawing.Size(361, 29);
            this.cmpSaveUpdatePanel.TabIndex = 0;
            this.cmpSaveUpdatePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.CmpSaveUpdatePanel_Paint);
            // 
            // cmpSaveUpdateData
            // 
            this.cmpSaveUpdateData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpSaveUpdateData.Location = new System.Drawing.Point(123, 3);
            this.cmpSaveUpdateData.Name = "cmpSaveUpdateData";
            this.cmpSaveUpdateData.Size = new System.Drawing.Size(114, 23);
            this.cmpSaveUpdateData.TabIndex = 0;
            this.cmpSaveUpdateData.Text = "Save / Update";
            this.cmpSaveUpdateData.UseVisualStyleBackColor = true;
            this.cmpSaveUpdateData.Click += new System.EventHandler(this.cmdSaveUpdateData_Click);
            // 
            // cmpDispositionHistory
            // 
            this.cmpDispositionHistory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmpDispositionHistory.Location = new System.Drawing.Point(283, 3);
            this.cmpDispositionHistory.Name = "cmpDispositionHistory";
            this.cmpDispositionHistory.Size = new System.Drawing.Size(75, 23);
            this.cmpDispositionHistory.TabIndex = 1;
            this.cmpDispositionHistory.Text = "History";
            this.cmpDispositionHistory.UseVisualStyleBackColor = true;
            this.cmpDispositionHistory.Click += new System.EventHandler(this.cmpDispositionHistory_Click);
            // 
            // DispositionCodesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(554, 257);
            this.Controls.Add(this.cmpDispositionPanel);
            this.Name = "DispositionCodesForm";
            this.Text = "DispositionCodes";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DispositionCodesForm_FormClosing);
            this.Load += new System.EventHandler(this.DispositionForm_Load);
            this.cmpDispositionPanel.ResumeLayout(false);
            this.cmpDispositionCode_CallAndCodePanel.ResumeLayout(false);
            this.cmpDispositionCode_CallAndCodePanel.PerformLayout();
            this.cmpSaveUpdatePanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel cmpDispositionPanel;
        private System.Windows.Forms.TableLayoutPanel cmpSaveUpdatePanel;
        private System.Windows.Forms.Button cmpSaveUpdateData;
        private System.Windows.Forms.Label cmpCurrentDispositionLabel;
        private System.Windows.Forms.ComboBox cmpDispositionComboBox;
        private System.Windows.Forms.Panel cmpDispositionCode_CallAndCodePanel;
        private System.Windows.Forms.Button cmpDispositionHistory;
    }
}