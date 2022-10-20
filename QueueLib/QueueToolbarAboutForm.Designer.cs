namespace QueueLib
{
	partial class QueueToolbarAboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueueToolbarAboutForm));
            this.Button_Close = new System.Windows.Forms.Button();
            this.Label_Copyright = new System.Windows.Forms.Label();
            this.Label_Version = new System.Windows.Forms.Label();
            this.Label_AppName = new System.Windows.Forms.Label();
            this.Logo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.Logo)).BeginInit();
            this.SuspendLayout();
            // 
            // Button_Close
            // 
            this.Button_Close.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.Button_Close.Location = new System.Drawing.Point(463, 154);
            this.Button_Close.Name = "Button_Close";
            this.Button_Close.Size = new System.Drawing.Size(75, 23);
            this.Button_Close.TabIndex = 9;
            this.Button_Close.Text = "Close";
            this.Button_Close.UseVisualStyleBackColor = false;
            this.Button_Close.Click += new System.EventHandler(this.Button_Close_Click);
            // 
            // Label_Copyright
            // 
            this.Label_Copyright.AutoSize = true;
            this.Label_Copyright.Location = new System.Drawing.Point(19, 88);
            this.Label_Copyright.Name = "Label_Copyright";
            this.Label_Copyright.Size = new System.Drawing.Size(212, 39);
            this.Label_Copyright.TabIndex = 8;
            this.Label_Copyright.Text = "Copyright 2008-2018. All Rights Reserved.\r\nintellaSoft and the intellaSoft logos " +
    "are\r\ntrademarks of intellaSoft. All rights reserved.";
            this.Label_Copyright.Click += new System.EventHandler(this.Label_Copyright_Click);
            // 
            // Label_Version
            // 
            this.Label_Version.AutoSize = true;
            this.Label_Version.ForeColor = System.Drawing.Color.Gray;
            this.Label_Version.Location = new System.Drawing.Point(15, 51);
            this.Label_Version.Name = "Label_Version";
            this.Label_Version.Size = new System.Drawing.Size(0, 13);
            this.Label_Version.TabIndex = 7;
            // 
            // Label_AppName
            // 
            this.Label_AppName.AutoSize = true;
            this.Label_AppName.BackColor = System.Drawing.Color.White;
            this.Label_AppName.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_AppName.Location = new System.Drawing.Point(12, 18);
            this.Label_AppName.Name = "Label_AppName";
            this.Label_AppName.Size = new System.Drawing.Size(235, 27);
            this.Label_AppName.TabIndex = 6;
            this.Label_AppName.Text = "IntellaQueue Toolbar";
            // 
            // Logo
            // 
            this.Logo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Logo.BackgroundImage")));
            this.Logo.Location = new System.Drawing.Point(288, 42);
            this.Logo.Name = "Logo";
            this.Logo.Size = new System.Drawing.Size(250, 70);
            this.Logo.TabIndex = 5;
            this.Logo.TabStop = false;
            // 
            // QueueToolbarAboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(559, 192);
            this.Controls.Add(this.Button_Close);
            this.Controls.Add(this.Label_Copyright);
            this.Controls.Add(this.Label_Version);
            this.Controls.Add(this.Label_AppName);
            this.Controls.Add(this.Logo);
            this.Name = "QueueToolbarAboutForm";
            this.Text = "IntellaQueue Toolbar About";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.Load += new System.EventHandler(this.QueueToolbarAboutForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Logo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.Button Button_Close;
		internal System.Windows.Forms.Label Label_Copyright;
		internal System.Windows.Forms.Label Label_Version;
		internal System.Windows.Forms.Label Label_AppName;
		internal System.Windows.Forms.PictureBox Logo;

	}
}