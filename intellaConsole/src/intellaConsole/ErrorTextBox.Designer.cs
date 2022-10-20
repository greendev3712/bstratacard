namespace intellaConsole {
    partial class ErrorTextBox {
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
            this.richTextBoxErrorMessage = new System.Windows.Forms.RichTextBox();
            this.cmpTableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.cmpButtonExit = new System.Windows.Forms.Button();
            this.cmpTableLayoutPanelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBoxErrorMessage
            // 
            this.richTextBoxErrorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxErrorMessage.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxErrorMessage.Name = "richTextBoxErrorMessage";
            this.richTextBoxErrorMessage.ReadOnly = true;
            this.richTextBoxErrorMessage.Size = new System.Drawing.Size(539, 247);
            this.richTextBoxErrorMessage.TabIndex = 0;
            this.richTextBoxErrorMessage.Text = "";
            // 
            // cmpTableLayoutPanelMain
            // 
            this.cmpTableLayoutPanelMain.ColumnCount = 1;
            this.cmpTableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cmpTableLayoutPanelMain.Controls.Add(this.richTextBoxErrorMessage, 0, 0);
            this.cmpTableLayoutPanelMain.Controls.Add(this.cmpButtonExit, 0, 1);
            this.cmpTableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpTableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.cmpTableLayoutPanelMain.Name = "cmpTableLayoutPanelMain";
            this.cmpTableLayoutPanelMain.RowCount = 2;
            this.cmpTableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cmpTableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.cmpTableLayoutPanelMain.Size = new System.Drawing.Size(545, 282);
            this.cmpTableLayoutPanelMain.TabIndex = 2;
            // 
            // cmpButtonExit
            // 
            this.cmpButtonExit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cmpButtonExit.Location = new System.Drawing.Point(3, 256);
            this.cmpButtonExit.Name = "cmpButtonExit";
            this.cmpButtonExit.Size = new System.Drawing.Size(539, 23);
            this.cmpButtonExit.TabIndex = 1;
            this.cmpButtonExit.Text = "Exit";
            this.cmpButtonExit.UseVisualStyleBackColor = true;
            this.cmpButtonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // ErrorTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 282);
            this.Controls.Add(this.cmpTableLayoutPanelMain);
            this.Name = "ErrorTextBox";
            this.Text = "An Error Has Occurred";
            this.Load += new System.EventHandler(this.ErrorTextBox_Load);
            this.cmpTableLayoutPanelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxErrorMessage;
        private System.Windows.Forms.TableLayoutPanel cmpTableLayoutPanelMain;
        private System.Windows.Forms.Button cmpButtonExit;
    }
}