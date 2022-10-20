namespace QueueLib {
    partial class QueueOverviewGridForm {
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
            this.tweakedDataGridView1 = new Lib.TweakedDataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.tweakedDataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // tweakedDataGridView1
            // 
            this.tweakedDataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tweakedDataGridView1.Location = new System.Drawing.Point(0, 0);
            this.tweakedDataGridView1.Name = "tweakedDataGridView1";
            this.tweakedDataGridView1.Size = new System.Drawing.Size(240, 150);
            this.tweakedDataGridView1.TabIndex = 0;
            // 
            // QueueOverviewGridForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 431);
            this.Controls.Add(this.tweakedDataGridView1);
            this.Name = "QueueOverviewGridForm";
            this.Text = "QueueOverviewGridForm";
            ((System.ComponentModel.ISupportInitialize)(this.tweakedDataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Lib.TweakedDataGridView tweakedDataGridView1;
    }
}