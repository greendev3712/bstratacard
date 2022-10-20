namespace QueueLib
{
    partial class CurrentCallForm
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
            this.cmpCallMainPanel = new System.Windows.Forms.Panel();
            this.cmpCallPanel = new System.Windows.Forms.Panel();
            this.cmpCurrentCallLabel = new System.Windows.Forms.Label();
            this.cmpCurrentCall = new System.Windows.Forms.TextBox();
            this.cmpCallMainPanel.SuspendLayout();
            this.cmpCallPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmpCallMainPanel
            // 
            this.cmpCallMainPanel.AutoSize = true;
            this.cmpCallMainPanel.Controls.Add(this.cmpCallPanel);
            this.cmpCallMainPanel.Location = new System.Drawing.Point(83, 41);
            this.cmpCallMainPanel.Margin = new System.Windows.Forms.Padding(1);
            this.cmpCallMainPanel.Name = "cmpCallMainPanel";
            this.cmpCallMainPanel.Size = new System.Drawing.Size(361, 34);
            this.cmpCallMainPanel.TabIndex = 0;
            // 
            // cmpCallPanel
            // 
            this.cmpCallPanel.Controls.Add(this.cmpCurrentCallLabel);
            this.cmpCallPanel.Controls.Add(this.cmpCurrentCall);
            this.cmpCallPanel.Location = new System.Drawing.Point(0, 0);
            this.cmpCallPanel.Name = "cmpCallPanel";
            this.cmpCallPanel.Size = new System.Drawing.Size(358, 31);
            this.cmpCallPanel.TabIndex = 1;
            // 
            // cmpCurrentCallLabel
            // 
            this.cmpCurrentCallLabel.AutoSize = true;
            this.cmpCurrentCallLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpCurrentCallLabel.ForeColor = System.Drawing.Color.White;
            this.cmpCurrentCallLabel.Location = new System.Drawing.Point(3, 9);
            this.cmpCurrentCallLabel.Name = "cmpCurrentCallLabel";
            this.cmpCurrentCallLabel.Size = new System.Drawing.Size(78, 13);
            this.cmpCurrentCallLabel.TabIndex = 1;
            this.cmpCurrentCallLabel.Text = "On Call With";
            // 
            // cmpCurrentCall
            // 
            this.cmpCurrentCall.Location = new System.Drawing.Point(97, 6);
            this.cmpCurrentCall.Name = "cmpCurrentCall";
            this.cmpCurrentCall.ReadOnly = true;
            this.cmpCurrentCall.Size = new System.Drawing.Size(253, 20);
            this.cmpCurrentCall.TabIndex = 1;
            // 
            // CurrentCallForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(554, 257);
            this.Controls.Add(this.cmpCallMainPanel);
            this.Name = "CurrentCallForm";
            this.Text = "CurrentCall";
            this.cmpCallMainPanel.ResumeLayout(false);
            this.cmpCallPanel.ResumeLayout(false);
            this.cmpCallPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel cmpCallMainPanel;
        private System.Windows.Forms.Label cmpCurrentCallLabel;
        private System.Windows.Forms.TextBox cmpCurrentCall;
        private System.Windows.Forms.Panel cmpCallPanel;
    }
}