namespace QueueLib
{
	partial class QueueAppConfigMainForm
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
            this.Button_Close = new System.Windows.Forms.Button();
            this.Button_QueueSettings = new System.Windows.Forms.Button();
            this.Button_ServerSettings = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Button_Close
            // 
            this.Button_Close.Location = new System.Drawing.Point(97, 70);
            this.Button_Close.Name = "Button_Close";
            this.Button_Close.Size = new System.Drawing.Size(98, 23);
            this.Button_Close.TabIndex = 5;
            this.Button_Close.Text = "Close";
            this.Button_Close.UseVisualStyleBackColor = true;
            this.Button_Close.Click += new System.EventHandler(this.Button_Close_Click);
            // 
            // Button_QueueSettings
            // 
            this.Button_QueueSettings.Location = new System.Drawing.Point(97, 12);
            this.Button_QueueSettings.Name = "Button_QueueSettings";
            this.Button_QueueSettings.Size = new System.Drawing.Size(98, 23);
            this.Button_QueueSettings.TabIndex = 4;
            this.Button_QueueSettings.Text = "Queue Settings";
            this.Button_QueueSettings.UseVisualStyleBackColor = true;
            this.Button_QueueSettings.Visible = false;
            this.Button_QueueSettings.Click += new System.EventHandler(this.Button_QueueSettings_Click);
            // 
            // Button_ServerSettings
            // 
            this.Button_ServerSettings.Location = new System.Drawing.Point(97, 41);
            this.Button_ServerSettings.Name = "Button_ServerSettings";
            this.Button_ServerSettings.Size = new System.Drawing.Size(98, 23);
            this.Button_ServerSettings.TabIndex = 3;
            this.Button_ServerSettings.Text = "Server Settings";
            this.Button_ServerSettings.UseVisualStyleBackColor = true;
            this.Button_ServerSettings.Click += new System.EventHandler(this.Button_ServerSettings_Click);
            // 
            // QueueAppConfigMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 101);
            this.Controls.Add(this.Button_Close);
            this.Controls.Add(this.Button_QueueSettings);
            this.Controls.Add(this.Button_ServerSettings);
            this.Name = "QueueAppConfigMainForm";
            this.Text = "IntellaQueue Toolbar Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QueueAppConfigMainForm_FormClosing);
            this.Load += new System.EventHandler(this.QueueAppConfigMainForm_Load);
            this.ResumeLayout(false);

		}

		#endregion

		internal System.Windows.Forms.Button Button_Close;
		internal System.Windows.Forms.Button Button_QueueSettings;
		internal System.Windows.Forms.Button Button_ServerSettings;
	}
}