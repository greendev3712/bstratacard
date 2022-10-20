namespace QueueLib
{
	partial class QueueAppConfigSettingsForm
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
			this.ComboBox_QueueNames = new System.Windows.Forms.ComboBox();
			this.Label2 = new System.Windows.Forms.Label();
			this.Label1 = new System.Windows.Forms.Label();
			this.Button_SaveAndConnect = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ComboBox_QueueNames
			// 
			this.ComboBox_QueueNames.FormattingEnabled = true;
			this.ComboBox_QueueNames.Location = new System.Drawing.Point(82, 42);
			this.ComboBox_QueueNames.Name = "ComboBox_QueueNames";
			this.ComboBox_QueueNames.Size = new System.Drawing.Size(121, 21);
			this.ComboBox_QueueNames.TabIndex = 31;
			// 
			// Label2
			// 
			this.Label2.AutoSize = true;
			this.Label2.Location = new System.Drawing.Point(12, 45);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(39, 13);
			this.Label2.TabIndex = 30;
			this.Label2.Text = "Queue";
			// 
			// Label1
			// 
			this.Label1.AutoSize = true;
			this.Label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label1.Location = new System.Drawing.Point(91, 9);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(94, 13);
			this.Label1.TabIndex = 29;
			this.Label1.Text = "Queue Settings";
			// 
			// Button_SaveAndConnect
			// 
			this.Button_SaveAndConnect.Location = new System.Drawing.Point(82, 238);
			this.Button_SaveAndConnect.Name = "Button_SaveAndConnect";
			this.Button_SaveAndConnect.Size = new System.Drawing.Size(121, 23);
			this.Button_SaveAndConnect.TabIndex = 28;
			this.Button_SaveAndConnect.Text = "Save and Connect";
			this.Button_SaveAndConnect.UseVisualStyleBackColor = true;
			this.Button_SaveAndConnect.Click += new System.EventHandler(this.Button_SaveAndConnect_Click);
			// 
			// QueueAppConfigSettingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Controls.Add(this.ComboBox_QueueNames);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.Button_SaveAndConnect);
			this.Name = "QueueAppConfigSettingsForm";
			this.Text = "Queue Settings";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.QueueAppConfigSettingsForm_FormClosed);
			this.Shown += new System.EventHandler(this.QueueAppConfigSettingsForm_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.ComboBox ComboBox_QueueNames;
		internal System.Windows.Forms.Label Label2;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.Button Button_SaveAndConnect;

	}
}