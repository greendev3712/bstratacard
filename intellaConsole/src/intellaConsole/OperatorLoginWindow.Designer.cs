namespace intellaConsole
{
    partial class OperatorLoginWindow
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
            this.cmpLabelExtension = new System.Windows.Forms.Label();
            this.cmpLabelQueue = new System.Windows.Forms.Label();
            this.cmpButtonLogin = new System.Windows.Forms.Button();
            this.cmpTextBoxExtension = new System.Windows.Forms.TextBox();
            this.cmpComboBoxQueue = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cmpLabelExtension
            // 
            this.cmpLabelExtension.AutoSize = true;
            this.cmpLabelExtension.Location = new System.Drawing.Point(12, 12);
            this.cmpLabelExtension.Name = "cmpLabelExtension";
            this.cmpLabelExtension.Size = new System.Drawing.Size(53, 13);
            this.cmpLabelExtension.TabIndex = 0;
            this.cmpLabelExtension.Text = "Extension";
            // 
            // cmpLabelQueue
            // 
            this.cmpLabelQueue.AutoSize = true;
            this.cmpLabelQueue.Location = new System.Drawing.Point(12, 38);
            this.cmpLabelQueue.Name = "cmpLabelQueue";
            this.cmpLabelQueue.Size = new System.Drawing.Size(39, 13);
            this.cmpLabelQueue.TabIndex = 2;
            this.cmpLabelQueue.Text = "Queue";
            // 
            // cmpButtonLogin
            // 
            this.cmpButtonLogin.Location = new System.Drawing.Point(15, 61);
            this.cmpButtonLogin.Name = "cmpButtonLogin";
            this.cmpButtonLogin.Size = new System.Drawing.Size(213, 23);
            this.cmpButtonLogin.TabIndex = 4;
            this.cmpButtonLogin.Text = "Login";
            this.cmpButtonLogin.UseVisualStyleBackColor = true;
            this.cmpButtonLogin.Click += new System.EventHandler(this.cmpButtonLogin_Click);
            // 
            // cmpTextBoxExtension
            // 
            this.cmpTextBoxExtension.Location = new System.Drawing.Point(71, 9);
            this.cmpTextBoxExtension.Name = "cmpTextBoxExtension";
            this.cmpTextBoxExtension.Size = new System.Drawing.Size(157, 20);
            this.cmpTextBoxExtension.TabIndex = 1;
            this.cmpTextBoxExtension.TextChanged += new System.EventHandler(this.cmpTextBoxExtension_TextChanged);
            // 
            // cmpComboBoxQueue
            // 
            this.cmpComboBoxQueue.FormattingEnabled = true;
            this.cmpComboBoxQueue.Location = new System.Drawing.Point(71, 34);
            this.cmpComboBoxQueue.Name = "cmpComboBoxQueue";
            this.cmpComboBoxQueue.Size = new System.Drawing.Size(157, 21);
            this.cmpComboBoxQueue.TabIndex = 5;
            // 
            // OperatorLoginWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(248, 95);
            this.Controls.Add(this.cmpComboBoxQueue);
            this.Controls.Add(this.cmpTextBoxExtension);
            this.Controls.Add(this.cmpButtonLogin);
            this.Controls.Add(this.cmpLabelQueue);
            this.Controls.Add(this.cmpLabelExtension);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OperatorLoginWindow";
            this.Text = "Operator Login";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OperatorLoginWindow_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label cmpLabelExtension;
        private System.Windows.Forms.Label cmpLabelQueue;
        private System.Windows.Forms.Button cmpButtonLogin;
        private System.Windows.Forms.TextBox cmpTextBoxExtension;
        private System.Windows.Forms.ComboBox cmpComboBoxQueue;
    }
}