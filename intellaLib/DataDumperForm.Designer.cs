namespace Lib {
    partial class DataDumperForm {
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
            this.cmpCopyToClipboardButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.cmpCloseButton = new System.Windows.Forms.Button();
            this.cmpPauseButton = new System.Windows.Forms.Button();
            this.cmpTextBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmpCopyToClipboardButton
            // 
            this.cmpCopyToClipboardButton.AutoSize = true;
            this.cmpCopyToClipboardButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpCopyToClipboardButton.Location = new System.Drawing.Point(3, 3);
            this.cmpCopyToClipboardButton.Name = "cmpCopyToClipboardButton";
            this.cmpCopyToClipboardButton.Size = new System.Drawing.Size(238, 20);
            this.cmpCopyToClipboardButton.TabIndex = 2;
            this.cmpCopyToClipboardButton.Text = "Copy to Clipboard";
            this.cmpCopyToClipboardButton.UseVisualStyleBackColor = true;
            this.cmpCopyToClipboardButton.Click += new System.EventHandler(this.cmpCopyToClipboardButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.cmpTextBox, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(740, 343);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.cmpCopyToClipboardButton, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmpCloseButton, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmpPauseButton, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 314);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(734, 26);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // cmpCloseButton
            // 
            this.cmpCloseButton.AutoSize = true;
            this.cmpCloseButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cmpCloseButton.Location = new System.Drawing.Point(491, 3);
            this.cmpCloseButton.Name = "cmpCloseButton";
            this.cmpCloseButton.Size = new System.Drawing.Size(240, 20);
            this.cmpCloseButton.TabIndex = 0;
            this.cmpCloseButton.Text = "Close";
            this.cmpCloseButton.UseVisualStyleBackColor = true;
            this.cmpCloseButton.Click += new System.EventHandler(this.cmpCloseButton_Click);
            // 
            // cmpPauseButton
            // 
            this.cmpPauseButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpPauseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpPauseButton.Location = new System.Drawing.Point(247, 3);
            this.cmpPauseButton.Name = "cmpPauseButton";
            this.cmpPauseButton.Size = new System.Drawing.Size(238, 20);
            this.cmpPauseButton.TabIndex = 3;
            this.cmpPauseButton.Text = "Pause";
            this.cmpPauseButton.UseVisualStyleBackColor = true;
            this.cmpPauseButton.Click += new System.EventHandler(this.cmpPauseButton_Click);
            // 
            // cmpTextBox
            // 
            this.cmpTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpTextBox.Location = new System.Drawing.Point(3, 3);
            this.cmpTextBox.Multiline = true;
            this.cmpTextBox.Name = "cmpTextBox";
            this.cmpTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.cmpTextBox.Size = new System.Drawing.Size(734, 305);
            this.cmpTextBox.TabIndex = 1;
            this.cmpTextBox.VisibleChanged += new System.EventHandler(this.cmpTextBox_VisibleChanged);
            // 
            // DataDumperForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 343);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DataDumperForm";
            this.Text = "DataDumperForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DataDumperForm_FormClosing);
            this.Load += new System.EventHandler(this.DataDumperForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmpCopyToClipboardButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button cmpCloseButton;
        private System.Windows.Forms.TextBox cmpTextBox;
        private System.Windows.Forms.Button cmpPauseButton;
    }
}