namespace Lib {
    partial class LiveDataGridViewForm {
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
            this.cmpLiveDataGridView = new Lib.LiveDataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.cmpLiveDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // cmpLiveDataGridView
            // 
            this.cmpLiveDataGridView.AllowUserToAddRows = false;
            this.cmpLiveDataGridView.AllowUserToDeleteRows = false;
            this.cmpLiveDataGridView.AllowUserToResizeColumns = false;
            this.cmpLiveDataGridView.AllowUserToResizeRows = false;
            this.cmpLiveDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.cmpLiveDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            this.cmpLiveDataGridView.BackgroundColor = System.Drawing.SystemColors.ControlDark;
            this.cmpLiveDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.cmpLiveDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(193)))), ((int)(((byte)(193)))), ((int)(((byte)(193)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.5F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.cmpLiveDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.cmpLiveDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(173)))), ((int)(((byte)(173)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.5F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(143)))), ((int)(((byte)(143)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.cmpLiveDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.cmpLiveDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpLiveDataGridView.EnableHeadersVisualStyles = false;
            this.cmpLiveDataGridView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpLiveDataGridView.GridColor = System.Drawing.Color.Black;
            this.cmpLiveDataGridView.Location = new System.Drawing.Point(0, 0);
            this.cmpLiveDataGridView.MultiSelect = false;
            this.cmpLiveDataGridView.Name = "cmpLiveDataGridView";
            this.cmpLiveDataGridView.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 18.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.cmpLiveDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.cmpLiveDataGridView.RowHeadersVisible = false;
            this.cmpLiveDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.cmpLiveDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.cmpLiveDataGridView.ShowCellToolTips = false;
            this.cmpLiveDataGridView.ShowEditingIcon = false;
            this.cmpLiveDataGridView.Size = new System.Drawing.Size(1070, 334);
            this.cmpLiveDataGridView.TabIndex = 0;
            // 
            // LiveDataGridViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1070, 334);
            this.Controls.Add(this.cmpLiveDataGridView);
            this.Name = "LiveDataGridViewForm";
            this.Text = "LiveDataGridViewForm";
            ((System.ComponentModel.ISupportInitialize)(this.cmpLiveDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private LiveDataGridView cmpLiveDataGridView;
    }
}