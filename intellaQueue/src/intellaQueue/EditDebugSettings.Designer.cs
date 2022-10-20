namespace QueueLib {
    partial class EditDebugSettings {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.okButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmpScreenPopUrlTextBox = new System.Windows.Forms.TextBox();
            this.cmpCancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Location = new System.Drawing.Point(86, 38);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 24;
            this.okButton.Text = "&OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(617, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "$phoneNumber";
            // 
            // cmpScreenPopUrlTextBox
            // 
            this.cmpScreenPopUrlTextBox.Location = new System.Drawing.Point(2, 12);
            this.cmpScreenPopUrlTextBox.Name = "cmpScreenPopUrlTextBox";
            this.cmpScreenPopUrlTextBox.Size = new System.Drawing.Size(617, 20);
            this.cmpScreenPopUrlTextBox.TabIndex = 26;
            // 
            // cmpCancelButton
            // 
            this.cmpCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmpCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmpCancelButton.Location = new System.Drawing.Point(2, 38);
            this.cmpCancelButton.Name = "cmpCancelButton";
            this.cmpCancelButton.Size = new System.Drawing.Size(75, 23);
            this.cmpCancelButton.TabIndex = 27;
            this.cmpCancelButton.Text = "&Cancel";
            this.cmpCancelButton.Click += new System.EventHandler(this.cmpCancelButton_Click);
            // 
            // EditDebugSettings
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(757, 73);
            this.Controls.Add(this.cmpCancelButton);
            this.Controls.Add(this.cmpScreenPopUrlTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditDebugSettings";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Screen Pop URL";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox cmpScreenPopUrlTextBox;
        private System.Windows.Forms.Button cmpCancelButton;

    }
}
