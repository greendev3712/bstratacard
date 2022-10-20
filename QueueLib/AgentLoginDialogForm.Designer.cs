namespace QueueLib
{
	partial class AgentLoginDialogForm
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
            this.agentNumberTextBox = new System.Windows.Forms.TextBox();
            this.agentNumberLabel = new System.Windows.Forms.Label();
            this.agentPinTextBox = new System.Windows.Forms.TextBox();
            this.agentPinLabel = new System.Windows.Forms.Label();
            this.agentExtensionLabel = new System.Windows.Forms.Label();
            this.agentExtensionTextBox = new System.Windows.Forms.TextBox();
            this.loginButton = new System.Windows.Forms.Button();
            this.cmpAgentLoginMainPanel = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.cmpAgentLoginMainPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // agentNumberTextBox
            // 
            this.agentNumberTextBox.Location = new System.Drawing.Point(90, 46);
            this.agentNumberTextBox.Name = "agentNumberTextBox";
            this.agentNumberTextBox.Size = new System.Drawing.Size(132, 20);
            this.agentNumberTextBox.TabIndex = 5;
            this.agentNumberTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress);
            // 
            // agentNumberLabel
            // 
            this.agentNumberLabel.AutoSize = true;
            this.agentNumberLabel.Location = new System.Drawing.Point(9, 49);
            this.agentNumberLabel.Name = "agentNumberLabel";
            this.agentNumberLabel.Size = new System.Drawing.Size(75, 13);
            this.agentNumberLabel.TabIndex = 2;
            this.agentNumberLabel.Text = "Agent Number";
            // 
            // agentPinTextBox
            // 
            this.agentPinTextBox.Location = new System.Drawing.Point(90, 78);
            this.agentPinTextBox.Name = "agentPinTextBox";
            this.agentPinTextBox.PasswordChar = '*';
            this.agentPinTextBox.Size = new System.Drawing.Size(132, 20);
            this.agentPinTextBox.TabIndex = 7;
            this.agentPinTextBox.Visible = false;
            this.agentPinTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress);
            // 
            // agentPinLabel
            // 
            this.agentPinLabel.AutoSize = true;
            this.agentPinLabel.Location = new System.Drawing.Point(31, 81);
            this.agentPinLabel.Name = "agentPinLabel";
            this.agentPinLabel.Size = new System.Drawing.Size(53, 13);
            this.agentPinLabel.TabIndex = 4;
            this.agentPinLabel.Text = "Agent Pin";
            this.agentPinLabel.Visible = false;
            // 
            // agentExtensionLabel
            // 
            this.agentExtensionLabel.AutoSize = true;
            this.agentExtensionLabel.Location = new System.Drawing.Point(31, 17);
            this.agentExtensionLabel.Name = "agentExtensionLabel";
            this.agentExtensionLabel.Size = new System.Drawing.Size(53, 13);
            this.agentExtensionLabel.TabIndex = 6;
            this.agentExtensionLabel.Text = "Extension";
            // 
            // agentExtensionTextBox
            // 
            this.agentExtensionTextBox.Location = new System.Drawing.Point(90, 14);
            this.agentExtensionTextBox.Name = "agentExtensionTextBox";
            this.agentExtensionTextBox.Size = new System.Drawing.Size(132, 20);
            this.agentExtensionTextBox.TabIndex = 3;
            this.agentExtensionTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress);
            // 
            // loginButton
            // 
            this.loginButton.Location = new System.Drawing.Point(147, 114);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(75, 23);
            this.loginButton.TabIndex = 9;
            this.loginButton.Text = "Login";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // cmpAgentLoginMainPanel
            // 
            this.cmpAgentLoginMainPanel.Controls.Add(this.button1);
            this.cmpAgentLoginMainPanel.Controls.Add(this.agentExtensionLabel);
            this.cmpAgentLoginMainPanel.Controls.Add(this.loginButton);
            this.cmpAgentLoginMainPanel.Controls.Add(this.agentNumberLabel);
            this.cmpAgentLoginMainPanel.Controls.Add(this.agentExtensionTextBox);
            this.cmpAgentLoginMainPanel.Controls.Add(this.agentNumberTextBox);
            this.cmpAgentLoginMainPanel.Controls.Add(this.agentPinLabel);
            this.cmpAgentLoginMainPanel.Controls.Add(this.agentPinTextBox);
            this.cmpAgentLoginMainPanel.Location = new System.Drawing.Point(-1, 0);
            this.cmpAgentLoginMainPanel.Name = "cmpAgentLoginMainPanel";
            this.cmpAgentLoginMainPanel.Size = new System.Drawing.Size(252, 156);
            this.cmpAgentLoginMainPanel.TabIndex = 10;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 114);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(128, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "Test Screen Recording";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // AgentLoginDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(252, 156);
            this.Controls.Add(this.cmpAgentLoginMainPanel);
            this.Name = "AgentLoginDialogForm";
            this.Text = "Agent Login";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AgentLoginDialogForm_FormClosed);
            this.Shown += new System.EventHandler(this.PasswordDialogForm_Shown);
            this.cmpAgentLoginMainPanel.ResumeLayout(false);
            this.cmpAgentLoginMainPanel.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		internal System.Windows.Forms.TextBox agentNumberTextBox;
		internal System.Windows.Forms.Label agentNumberLabel;
		internal System.Windows.Forms.TextBox agentPinTextBox;
		internal System.Windows.Forms.Label agentPinLabel;
		private System.Windows.Forms.Label agentExtensionLabel;
		private System.Windows.Forms.TextBox agentExtensionTextBox;
		private System.Windows.Forms.Button loginButton;
		private System.Windows.Forms.Panel cmpAgentLoginMainPanel;
        private System.Windows.Forms.Button button1;
    }
}