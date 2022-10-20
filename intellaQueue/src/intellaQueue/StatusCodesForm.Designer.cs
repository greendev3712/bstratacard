namespace QueueLib {
    partial class StatusCodesForm {
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
            this.cmpPauseCodesPanel = new System.Windows.Forms.Panel();
            this.cmpStatusComboBox = new System.Windows.Forms.ComboBox();
            this.cmpCurrentStatusLabel = new System.Windows.Forms.Label();
            this.cmpHideShowPanel = new System.Windows.Forms.Panel();
            this.cmpHideShowTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.cmpHideShowButton = new System.Windows.Forms.Button();
            this.cmpPauseCodesPanel.SuspendLayout();
            this.cmpHideShowPanel.SuspendLayout();
            this.cmpHideShowTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmpPauseCodesPanel
            // 
            this.cmpPauseCodesPanel.AutoSize = true;
            this.cmpPauseCodesPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmpPauseCodesPanel.Controls.Add(this.cmpStatusComboBox);
            this.cmpPauseCodesPanel.Controls.Add(this.cmpCurrentStatusLabel);
            this.cmpPauseCodesPanel.Location = new System.Drawing.Point(10, 108);
            this.cmpPauseCodesPanel.Margin = new System.Windows.Forms.Padding(1);
            this.cmpPauseCodesPanel.Name = "cmpPauseCodesPanel";
            this.cmpPauseCodesPanel.Size = new System.Drawing.Size(353, 32);
            this.cmpPauseCodesPanel.TabIndex = 0;
            // 
            // cmpStatusComboBox
            // 
            this.cmpStatusComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmpStatusComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpStatusComboBox.FormattingEnabled = true;
            this.cmpStatusComboBox.Location = new System.Drawing.Point(97, 8);
            this.cmpStatusComboBox.Name = "cmpStatusComboBox";
            this.cmpStatusComboBox.Size = new System.Drawing.Size(253, 21);
            this.cmpStatusComboBox.TabIndex = 2;
            this.cmpStatusComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cmpStatusComboBox_DrawItem);
            this.cmpStatusComboBox.DropDown += new System.EventHandler(this.cmpStatusComboBox_DropDown);
            this.cmpStatusComboBox.SelectedIndexChanged += new System.EventHandler(this.cmpStatusComboBox_SelectedIndexChanged);
            this.cmpStatusComboBox.DropDownClosed += new System.EventHandler(this.cmpStatusComboBox_DropDownClosed);
            // 
            // cmpCurrentStatusLabel
            // 
            this.cmpCurrentStatusLabel.AutoSize = true;
            this.cmpCurrentStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpCurrentStatusLabel.ForeColor = System.Drawing.Color.White;
            this.cmpCurrentStatusLabel.Location = new System.Drawing.Point(3, 11);
            this.cmpCurrentStatusLabel.Name = "cmpCurrentStatusLabel";
            this.cmpCurrentStatusLabel.Size = new System.Drawing.Size(88, 13);
            this.cmpCurrentStatusLabel.TabIndex = 1;
            this.cmpCurrentStatusLabel.Text = "Current Status";
            // 
            // cmpHideShowPanel
            // 
            this.cmpHideShowPanel.Controls.Add(this.cmpHideShowTableLayoutPanel);
            this.cmpHideShowPanel.Location = new System.Drawing.Point(9, 172);
            this.cmpHideShowPanel.Margin = new System.Windows.Forms.Padding(0);
            this.cmpHideShowPanel.Name = "cmpHideShowPanel";
            this.cmpHideShowPanel.Size = new System.Drawing.Size(499, 29);
            this.cmpHideShowPanel.TabIndex = 1;
            // 
            // cmpHideShowTableLayoutPanel
            // 
            this.cmpHideShowTableLayoutPanel.ColumnCount = 3;
            this.cmpHideShowTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.cmpHideShowTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.cmpHideShowTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.cmpHideShowTableLayoutPanel.Controls.Add(this.cmpHideShowButton, 1, 0);
            this.cmpHideShowTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpHideShowTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.cmpHideShowTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.cmpHideShowTableLayoutPanel.Name = "cmpHideShowTableLayoutPanel";
            this.cmpHideShowTableLayoutPanel.RowCount = 1;
            this.cmpHideShowTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cmpHideShowTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.cmpHideShowTableLayoutPanel.Size = new System.Drawing.Size(499, 29);
            this.cmpHideShowTableLayoutPanel.TabIndex = 0;
            this.cmpHideShowTableLayoutPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // cmpHideShowButton
            // 
            this.cmpHideShowButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpHideShowButton.Location = new System.Drawing.Point(169, 3);
            this.cmpHideShowButton.Name = "cmpHideShowButton";
            this.cmpHideShowButton.Size = new System.Drawing.Size(160, 23);
            this.cmpHideShowButton.TabIndex = 0;
            this.cmpHideShowButton.Text = "Hide/Show Status Controls";
            this.cmpHideShowButton.UseVisualStyleBackColor = true;
            // 
            // StatusCodesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(515, 264);
            this.Controls.Add(this.cmpHideShowPanel);
            this.Controls.Add(this.cmpPauseCodesPanel);
            this.Name = "StatusCodesForm";
            this.Text = "PauseCodes";
            this.Load += new System.EventHandler(this.StatusCodesForm_Load);
            this.cmpPauseCodesPanel.ResumeLayout(false);
            this.cmpPauseCodesPanel.PerformLayout();
            this.cmpHideShowPanel.ResumeLayout(false);
            this.cmpHideShowTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel cmpPauseCodesPanel;
        private System.Windows.Forms.ComboBox cmpStatusComboBox;
        private System.Windows.Forms.Label cmpCurrentStatusLabel;
        private System.Windows.Forms.Panel cmpHideShowPanel;
        private System.Windows.Forms.TableLayoutPanel cmpHideShowTableLayoutPanel;
        private System.Windows.Forms.Button cmpHideShowButton;
    }
}