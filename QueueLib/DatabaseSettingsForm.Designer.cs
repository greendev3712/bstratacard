namespace QueueLib
{
	partial class DatabaseSettingsForm
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
            this.TextBox_Pass = new System.Windows.Forms.TextBox();
            this.TextBox_User = new System.Windows.Forms.TextBox();
            this.TextBox_Name = new System.Windows.Forms.TextBox();
            this.TextBox_Port = new System.Windows.Forms.TextBox();
            this.TextBox_Host = new System.Windows.Forms.TextBox();
            this.Label6 = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.cmpSaveAndConnectButton = new System.Windows.Forms.Button();
            this.topMenuStrip = new System.Windows.Forms.MenuStrip();
            this.connectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmpShowHideAdvancedButton = new System.Windows.Forms.Button();
            this.cmpAdvancedPanel = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cmpRunAsAdministratorButton = new System.Windows.Forms.Button();
            this.cmpRunAtStartupCheckBox = new System.Windows.Forms.CheckBox();
            this.topMenuStrip.SuspendLayout();
            this.cmpAdvancedPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TextBox_Pass
            // 
            this.TextBox_Pass.Location = new System.Drawing.Point(66, 165);
            this.TextBox_Pass.Name = "TextBox_Pass";
            this.TextBox_Pass.PasswordChar = '*';
            this.TextBox_Pass.Size = new System.Drawing.Size(205, 20);
            this.TextBox_Pass.TabIndex = 35;
            this.TextBox_Pass.TextChanged += new System.EventHandler(this.TextBox_Pass_TextChanged);
            this.TextBox_Pass.Enter += new System.EventHandler(this.TextBox_Pass_Enter);
            // 
            // TextBox_User
            // 
            this.TextBox_User.Location = new System.Drawing.Point(66, 126);
            this.TextBox_User.Name = "TextBox_User";
            this.TextBox_User.Size = new System.Drawing.Size(205, 20);
            this.TextBox_User.TabIndex = 34;
            this.TextBox_User.TextChanged += new System.EventHandler(this.TextBox_User_TextChanged);
            this.TextBox_User.Enter += new System.EventHandler(this.TextBox_User_Enter);
            // 
            // TextBox_Name
            // 
            this.TextBox_Name.Location = new System.Drawing.Point(66, 87);
            this.TextBox_Name.Name = "TextBox_Name";
            this.TextBox_Name.Size = new System.Drawing.Size(205, 20);
            this.TextBox_Name.TabIndex = 33;
            this.TextBox_Name.TextChanged += new System.EventHandler(this.TextBox_Name_TextChanged);
            this.TextBox_Name.Enter += new System.EventHandler(this.TextBox_Name_Enter);
            // 
            // TextBox_Port
            // 
            this.TextBox_Port.Location = new System.Drawing.Point(66, 47);
            this.TextBox_Port.Name = "TextBox_Port";
            this.TextBox_Port.Size = new System.Drawing.Size(205, 20);
            this.TextBox_Port.TabIndex = 32;
            this.TextBox_Port.TextChanged += new System.EventHandler(this.TextBox_Port_TextChanged);
            this.TextBox_Port.Enter += new System.EventHandler(this.TextBox_Port_Enter);
            // 
            // TextBox_Host
            // 
            this.TextBox_Host.Location = new System.Drawing.Point(66, 10);
            this.TextBox_Host.Name = "TextBox_Host";
            this.TextBox_Host.Size = new System.Drawing.Size(205, 20);
            this.TextBox_Host.TabIndex = 31;
            this.TextBox_Host.TextChanged += new System.EventHandler(this.TextBox_Host_TextChanged);
            this.TextBox_Host.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_Host_KeyPress);
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Location = new System.Drawing.Point(3, 89);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(53, 13);
            this.Label6.TabIndex = 30;
            this.Label6.Text = "Database";
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Location = new System.Drawing.Point(3, 50);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(26, 13);
            this.Label5.TabIndex = 29;
            this.Label5.Text = "Port";
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(3, 172);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(53, 13);
            this.Label4.TabIndex = 28;
            this.Label4.Text = "Password";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(3, 133);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(55, 13);
            this.Label3.TabIndex = 27;
            this.Label3.Text = "Username";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(3, 17);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(29, 13);
            this.Label2.TabIndex = 26;
            this.Label2.Text = "Host";
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(84, 7);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(111, 13);
            this.Label1.TabIndex = 25;
            this.Label1.Text = "Database Settings";
            this.Label1.Click += new System.EventHandler(this.Label1_Click);
            // 
            // cmpSaveAndConnectButton
            // 
            this.cmpSaveAndConnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpSaveAndConnectButton.ForeColor = System.Drawing.Color.ForestGreen;
            this.cmpSaveAndConnectButton.Location = new System.Drawing.Point(76, 35);
            this.cmpSaveAndConnectButton.Name = "cmpSaveAndConnectButton";
            this.cmpSaveAndConnectButton.Size = new System.Drawing.Size(125, 23);
            this.cmpSaveAndConnectButton.TabIndex = 36;
            this.cmpSaveAndConnectButton.Text = "Save and Connect";
            this.cmpSaveAndConnectButton.UseVisualStyleBackColor = true;
            this.cmpSaveAndConnectButton.Click += new System.EventHandler(this.Button_SaveAndConnect_Click);
            // 
            // topMenuStrip
            // 
            this.topMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectionToolStripMenuItem});
            this.topMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.topMenuStrip.Name = "topMenuStrip";
            this.topMenuStrip.Size = new System.Drawing.Size(286, 24);
            this.topMenuStrip.TabIndex = 37;
            this.topMenuStrip.Text = "menuStrip1";
            // 
            // connectionToolStripMenuItem
            // 
            this.connectionToolStripMenuItem.Name = "connectionToolStripMenuItem";
            this.connectionToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.connectionToolStripMenuItem.Text = "Connection";
            // 
            // cmpShowHideAdvancedButton
            // 
            this.cmpShowHideAdvancedButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmpShowHideAdvancedButton.Location = new System.Drawing.Point(76, 70);
            this.cmpShowHideAdvancedButton.Name = "cmpShowHideAdvancedButton";
            this.cmpShowHideAdvancedButton.Size = new System.Drawing.Size(125, 23);
            this.cmpShowHideAdvancedButton.TabIndex = 38;
            this.cmpShowHideAdvancedButton.Text = "Show/Hide Advanced";
            this.cmpShowHideAdvancedButton.UseVisualStyleBackColor = true;
            this.cmpShowHideAdvancedButton.Click += new System.EventHandler(this.cmpShowHideAdvancedButton_Click);
            // 
            // cmpAdvancedPanel
            // 
            this.cmpAdvancedPanel.Controls.Add(this.TextBox_Port);
            this.cmpAdvancedPanel.Controls.Add(this.Label3);
            this.cmpAdvancedPanel.Controls.Add(this.TextBox_Host);
            this.cmpAdvancedPanel.Controls.Add(this.TextBox_Pass);
            this.cmpAdvancedPanel.Controls.Add(this.Label2);
            this.cmpAdvancedPanel.Controls.Add(this.Label4);
            this.cmpAdvancedPanel.Controls.Add(this.TextBox_User);
            this.cmpAdvancedPanel.Controls.Add(this.Label5);
            this.cmpAdvancedPanel.Controls.Add(this.TextBox_Name);
            this.cmpAdvancedPanel.Controls.Add(this.Label6);
            this.cmpAdvancedPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cmpAdvancedPanel.Location = new System.Drawing.Point(0, 208);
            this.cmpAdvancedPanel.Name = "cmpAdvancedPanel";
            this.cmpAdvancedPanel.Size = new System.Drawing.Size(286, 193);
            this.cmpAdvancedPanel.TabIndex = 39;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.cmpRunAtStartupCheckBox);
            this.panel1.Controls.Add(this.cmpRunAsAdministratorButton);
            this.panel1.Controls.Add(this.Label1);
            this.panel1.Controls.Add(this.cmpShowHideAdvancedButton);
            this.panel1.Controls.Add(this.cmpSaveAndConnectButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(286, 161);
            this.panel1.TabIndex = 40;
            // 
            // cmpRunAsAdministratorButton
            // 
            this.cmpRunAsAdministratorButton.ForeColor = System.Drawing.Color.Crimson;
            this.cmpRunAsAdministratorButton.Location = new System.Drawing.Point(76, 105);
            this.cmpRunAsAdministratorButton.Name = "cmpRunAsAdministratorButton";
            this.cmpRunAsAdministratorButton.Size = new System.Drawing.Size(125, 23);
            this.cmpRunAsAdministratorButton.TabIndex = 39;
            this.cmpRunAsAdministratorButton.Text = "Run as Administrator";
            this.cmpRunAsAdministratorButton.UseVisualStyleBackColor = true;
            this.cmpRunAsAdministratorButton.Visible = false;
            this.cmpRunAsAdministratorButton.Click += new System.EventHandler(this.cmpRunAsAdministratorButton_Click);
            // 
            // cmpRunAtStartupCheckBox
            // 
            this.cmpRunAtStartupCheckBox.AutoSize = true;
            this.cmpRunAtStartupCheckBox.Checked = true;
            this.cmpRunAtStartupCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cmpRunAtStartupCheckBox.Location = new System.Drawing.Point(91, 141);
            this.cmpRunAtStartupCheckBox.Name = "cmpRunAtStartupCheckBox";
            this.cmpRunAtStartupCheckBox.Size = new System.Drawing.Size(95, 17);
            this.cmpRunAtStartupCheckBox.TabIndex = 40;
            this.cmpRunAtStartupCheckBox.Text = "Run at Startup";
            this.cmpRunAtStartupCheckBox.UseVisualStyleBackColor = true;
            // 
            // DatabaseSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(286, 401);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.cmpAdvancedPanel);
            this.Controls.Add(this.topMenuStrip);
            this.MainMenuStrip = this.topMenuStrip;
            this.MinimumSize = new System.Drawing.Size(302, 163);
            this.Name = "DatabaseSettingsForm";
            this.Text = "Database Settings";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DatabaseSettingsForm_FormClosed);
            this.Load += new System.EventHandler(this.DatabaseSettingsForm_Load);
            this.Shown += new System.EventHandler(this.DatabaseSettingsForm_Shown);
            this.topMenuStrip.ResumeLayout(false);
            this.topMenuStrip.PerformLayout();
            this.cmpAdvancedPanel.ResumeLayout(false);
            this.cmpAdvancedPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.TextBox TextBox_Pass;
		internal System.Windows.Forms.TextBox TextBox_User;
		internal System.Windows.Forms.TextBox TextBox_Name;
		internal System.Windows.Forms.TextBox TextBox_Port;
		internal System.Windows.Forms.TextBox TextBox_Host;
		internal System.Windows.Forms.Label Label6;
		internal System.Windows.Forms.Label Label5;
		internal System.Windows.Forms.Label Label4;
		internal System.Windows.Forms.Label Label3;
		internal System.Windows.Forms.Label Label2;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.Button cmpSaveAndConnectButton;
		private System.Windows.Forms.MenuStrip topMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem connectionToolStripMenuItem;
        private System.Windows.Forms.Button cmpShowHideAdvancedButton;
        private System.Windows.Forms.Panel cmpAdvancedPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button cmpRunAsAdministratorButton;
        private System.Windows.Forms.CheckBox cmpRunAtStartupCheckBox;
    }
}