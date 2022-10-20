namespace intellaConsole {
    partial class SpeedDialPanel {
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
            this.panelSpeedDial = new System.Windows.Forms.Panel();
            this.dataGridViewSpeedDial = new System.Windows.Forms.DataGridView();
            this.speedDial1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.speedDial2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.speedDial3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelSpeedDial.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSpeedDial)).BeginInit();
            this.SuspendLayout();
            // 
            // panelSpeedDial
            // 
            this.panelSpeedDial.Controls.Add(this.dataGridViewSpeedDial);
            this.panelSpeedDial.Location = new System.Drawing.Point(57, 39);
            this.panelSpeedDial.Name = "panelSpeedDial";
            this.panelSpeedDial.Size = new System.Drawing.Size(700, 395);
            this.panelSpeedDial.TabIndex = 0;
            // 
            // dataGridViewSpeedDial
            // 
            this.dataGridViewSpeedDial.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.NullValue = null;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewSpeedDial.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewSpeedDial.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSpeedDial.ColumnHeadersVisible = false;
            this.dataGridViewSpeedDial.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.speedDial1,
            this.speedDial2,
            this.speedDial3,
            this.Column4});
            this.dataGridViewSpeedDial.Location = new System.Drawing.Point(1, 2);
            this.dataGridViewSpeedDial.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.dataGridViewSpeedDial.MultiSelect = false;
            this.dataGridViewSpeedDial.Name = "dataGridViewSpeedDial";
            this.dataGridViewSpeedDial.ReadOnly = true;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewSpeedDial.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewSpeedDial.RowHeadersVisible = false;
            this.dataGridViewSpeedDial.RowHeadersWidth = 44;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.DarkGoldenrod;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewSpeedDial.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewSpeedDial.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dataGridViewSpeedDial.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataGridViewSpeedDial.RowTemplate.Height = 44;
            this.dataGridViewSpeedDial.RowTemplate.ReadOnly = true;
            this.dataGridViewSpeedDial.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewSpeedDial.Size = new System.Drawing.Size(700, 391);
            this.dataGridViewSpeedDial.TabIndex = 1;
            this.dataGridViewSpeedDial.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewSpeedDial_CellDoubleClick);
            this.dataGridViewSpeedDial.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridViewSpeedDial_CellMouseDown);
            // 
            // speedDial1
            // 
            this.speedDial1.Frozen = true;
            this.speedDial1.HeaderText = "Column1";
            this.speedDial1.Name = "speedDial1";
            this.speedDial1.ReadOnly = true;
            this.speedDial1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.speedDial1.Width = 174;
            // 
            // speedDial2
            // 
            this.speedDial2.Frozen = true;
            this.speedDial2.HeaderText = "Column2";
            this.speedDial2.Name = "speedDial2";
            this.speedDial2.ReadOnly = true;
            this.speedDial2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.speedDial2.Width = 174;
            // 
            // speedDial3
            // 
            this.speedDial3.Frozen = true;
            this.speedDial3.HeaderText = "Column3";
            this.speedDial3.Name = "speedDial3";
            this.speedDial3.ReadOnly = true;
            this.speedDial3.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.speedDial3.Width = 174;
            // 
            // Column4
            // 
            this.Column4.Frozen = true;
            this.Column4.HeaderText = "Column4";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column4.Width = 174;
            // 
            // SpeedDialPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(868, 525);
            this.Controls.Add(this.panelSpeedDial);
            this.Name = "SpeedDialPanel";
            this.Text = "SpeedDialPanel";
            this.panelSpeedDial.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSpeedDial)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelSpeedDial;
        private System.Windows.Forms.DataGridView dataGridViewSpeedDial;
        private System.Windows.Forms.DataGridViewTextBoxColumn speedDial1;
        private System.Windows.Forms.DataGridViewTextBoxColumn speedDial2;
        private System.Windows.Forms.DataGridViewTextBoxColumn speedDial3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
    }
}