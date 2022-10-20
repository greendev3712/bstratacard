namespace intellaConsole
{
    partial class SpeedDialEntryEditor
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
            this.cmpLabelFirstName = new System.Windows.Forms.Label();
            this.cmpLabelLastName = new System.Windows.Forms.Label();
            this.cmpLabelPhoneNumber = new System.Windows.Forms.Label();
            this.cmpLabelEmail = new System.Windows.Forms.Label();
            this.cmpTextBoxFirstName = new System.Windows.Forms.TextBox();
            this.cmpTextBoxLastName = new System.Windows.Forms.TextBox();
            this.cmpTextBoxEmail = new System.Windows.Forms.TextBox();
            this.cmpButtonSave = new System.Windows.Forms.Button();
            this.cmpButtonCancel = new System.Windows.Forms.Button();
            this.cmpButtonDelete = new System.Windows.Forms.Button();
            this.cmpTextBoxPhoneNumber = new MaskedTextBox.MaskedTextBox();
            this.SuspendLayout();
            // 
            // cmpLabelFirstName
            // 
            this.cmpLabelFirstName.AutoSize = true;
            this.cmpLabelFirstName.Location = new System.Drawing.Point(13, 8);
            this.cmpLabelFirstName.Name = "cmpLabelFirstName";
            this.cmpLabelFirstName.Size = new System.Drawing.Size(57, 13);
            this.cmpLabelFirstName.TabIndex = 0;
            this.cmpLabelFirstName.Text = "First Name";
            // 
            // cmpLabelLastName
            // 
            this.cmpLabelLastName.AutoSize = true;
            this.cmpLabelLastName.Location = new System.Drawing.Point(12, 34);
            this.cmpLabelLastName.Name = "cmpLabelLastName";
            this.cmpLabelLastName.Size = new System.Drawing.Size(58, 13);
            this.cmpLabelLastName.TabIndex = 2;
            this.cmpLabelLastName.Text = "Last Name";
            // 
            // cmpLabelPhoneNumber
            // 
            this.cmpLabelPhoneNumber.AutoSize = true;
            this.cmpLabelPhoneNumber.Location = new System.Drawing.Point(10, 60);
            this.cmpLabelPhoneNumber.Name = "cmpLabelPhoneNumber";
            this.cmpLabelPhoneNumber.Size = new System.Drawing.Size(78, 13);
            this.cmpLabelPhoneNumber.TabIndex = 4;
            this.cmpLabelPhoneNumber.Text = "Phone Number";
            // 
            // cmpLabelEmail
            // 
            this.cmpLabelEmail.AutoSize = true;
            this.cmpLabelEmail.Location = new System.Drawing.Point(13, 86);
            this.cmpLabelEmail.Name = "cmpLabelEmail";
            this.cmpLabelEmail.Size = new System.Drawing.Size(32, 13);
            this.cmpLabelEmail.TabIndex = 6;
            this.cmpLabelEmail.Text = "Email";
            // 
            // cmpTextBoxFirstName
            // 
            this.cmpTextBoxFirstName.Location = new System.Drawing.Point(94, 5);
            this.cmpTextBoxFirstName.Name = "cmpTextBoxFirstName";
            this.cmpTextBoxFirstName.Size = new System.Drawing.Size(157, 20);
            this.cmpTextBoxFirstName.TabIndex = 1;
            // 
            // cmpTextBoxLastName
            // 
            this.cmpTextBoxLastName.Location = new System.Drawing.Point(94, 31);
            this.cmpTextBoxLastName.Name = "cmpTextBoxLastName";
            this.cmpTextBoxLastName.Size = new System.Drawing.Size(157, 20);
            this.cmpTextBoxLastName.TabIndex = 3;
            // 
            // cmpTextBoxEmail
            // 
            this.cmpTextBoxEmail.Location = new System.Drawing.Point(94, 83);
            this.cmpTextBoxEmail.Name = "cmpTextBoxEmail";
            this.cmpTextBoxEmail.Size = new System.Drawing.Size(157, 20);
            this.cmpTextBoxEmail.TabIndex = 7;
            // 
            // cmpButtonSave
            // 
            this.cmpButtonSave.Location = new System.Drawing.Point(16, 109);
            this.cmpButtonSave.Name = "cmpButtonSave";
            this.cmpButtonSave.Size = new System.Drawing.Size(65, 23);
            this.cmpButtonSave.TabIndex = 8;
            this.cmpButtonSave.Text = "Save";
            this.cmpButtonSave.UseVisualStyleBackColor = true;
            this.cmpButtonSave.Click += new System.EventHandler(this.cmpButtonSave_Click);
            // 
            // cmpButtonCancel
            // 
            this.cmpButtonCancel.Location = new System.Drawing.Point(186, 109);
            this.cmpButtonCancel.Name = "cmpButtonCancel";
            this.cmpButtonCancel.Size = new System.Drawing.Size(65, 23);
            this.cmpButtonCancel.TabIndex = 10;
            this.cmpButtonCancel.Text = "Cancel";
            this.cmpButtonCancel.UseVisualStyleBackColor = true;
            this.cmpButtonCancel.Click += new System.EventHandler(this.cmpButtonCancel_Click);
            // 
            // cmpButtonDelete
            // 
            this.cmpButtonDelete.Location = new System.Drawing.Point(103, 109);
            this.cmpButtonDelete.Name = "cmpButtonDelete";
            this.cmpButtonDelete.Size = new System.Drawing.Size(65, 23);
            this.cmpButtonDelete.TabIndex = 9;
            this.cmpButtonDelete.Text = "Delete";
            this.cmpButtonDelete.UseVisualStyleBackColor = true;
            this.cmpButtonDelete.Click += new System.EventHandler(this.cmpButtonDelete_Click);
            // 
            // cmpTextBoxPhoneNumber
            // 
            this.cmpTextBoxPhoneNumber.Location = new System.Drawing.Point(94, 57);
            this.cmpTextBoxPhoneNumber.Masked = MaskedTextBox.Mask.Digit;
            this.cmpTextBoxPhoneNumber.Name = "cmpTextBoxPhoneNumber";
            this.cmpTextBoxPhoneNumber.Size = new System.Drawing.Size(157, 20);
            this.cmpTextBoxPhoneNumber.TabIndex = 5;
            // 
            // SpeedDialEntryEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(272, 141);
            this.Controls.Add(this.cmpTextBoxPhoneNumber);
            this.Controls.Add(this.cmpButtonDelete);
            this.Controls.Add(this.cmpButtonCancel);
            this.Controls.Add(this.cmpButtonSave);
            this.Controls.Add(this.cmpTextBoxEmail);
            this.Controls.Add(this.cmpTextBoxLastName);
            this.Controls.Add(this.cmpTextBoxFirstName);
            this.Controls.Add(this.cmpLabelEmail);
            this.Controls.Add(this.cmpLabelPhoneNumber);
            this.Controls.Add(this.cmpLabelLastName);
            this.Controls.Add(this.cmpLabelFirstName);
            this.Name = "SpeedDialEntryEditor";
            this.Text = "Edit Speed Dial";
            this.Load += new System.EventHandler(this.SpeedDialEntryEditor_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label cmpLabelFirstName;
        private System.Windows.Forms.Label cmpLabelLastName;
        private System.Windows.Forms.Label cmpLabelPhoneNumber;
        private System.Windows.Forms.Label cmpLabelEmail;
        private System.Windows.Forms.TextBox cmpTextBoxFirstName;
        private System.Windows.Forms.TextBox cmpTextBoxLastName;
        private System.Windows.Forms.TextBox cmpTextBoxEmail;
        private System.Windows.Forms.Button cmpButtonSave;
        private System.Windows.Forms.Button cmpButtonCancel;
        private System.Windows.Forms.Button cmpButtonDelete;
        private MaskedTextBox.MaskedTextBox cmpTextBoxPhoneNumber;
    }
}