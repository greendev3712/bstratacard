namespace Lib
{
	partial class LogViewer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//protected override void Dispose(bool disposing)
		//{
		//    if (disposing && (components != null))
		//    {
		//        components.Dispose();
		//    }
		//    base.Dispose(disposing);
		//}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.logBox = new Lib.TweakedRichTextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.AlwaysOnTopCheckBox = new System.Windows.Forms.CheckBox();
			this.clearButton = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// logBox
			// 
			this.logBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this.logBox, 3);
			this.logBox.Location = new System.Drawing.Point(3, 32);
			this.logBox.Name = "logBox";
			this.logBox.Size = new System.Drawing.Size(786, 538);
			this.logBox.TabIndex = 0;
			this.logBox.Text = "";
			this.logBox.Resize += new System.EventHandler(this.logBox_Resize);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.logBox, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.AlwaysOnTopCheckBox, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.clearButton, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(792, 573);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold);
			this.label1.Location = new System.Drawing.Point(3, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(85, 17);
			this.label1.TabIndex = 1;
			this.label1.Text = "Messages:";
			// 
			// AlwaysOnTopCheckBox
			// 
			this.AlwaysOnTopCheckBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.AlwaysOnTopCheckBox.AutoSize = true;
			this.AlwaysOnTopCheckBox.Checked = true;
			this.AlwaysOnTopCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.AlwaysOnTopCheckBox.Location = new System.Drawing.Point(697, 6);
			this.AlwaysOnTopCheckBox.Name = "AlwaysOnTopCheckBox";
			this.AlwaysOnTopCheckBox.Size = new System.Drawing.Size(92, 17);
			this.AlwaysOnTopCheckBox.TabIndex = 2;
			this.AlwaysOnTopCheckBox.Text = "Always on top";
			this.AlwaysOnTopCheckBox.UseVisualStyleBackColor = true;
			this.AlwaysOnTopCheckBox.CheckedChanged += new System.EventHandler(this.AlwaysOnTopCheckBox_CheckedChanged);
			// 
			// clearButton
			// 
			this.clearButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.clearButton.Location = new System.Drawing.Point(616, 3);
			this.clearButton.Name = "clearButton";
			this.clearButton.Size = new System.Drawing.Size(75, 23);
			this.clearButton.TabIndex = 3;
			this.clearButton.Text = "Clear";
			this.clearButton.UseVisualStyleBackColor = true;
			this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
			// 
			// LogViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(792, 573);
			this.Controls.Add(this.tableLayoutPanel1);
			this.MinimumSize = new System.Drawing.Size(640, 480);
			this.Name = "LogViewer";
			this.Text = "Log Viewer";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private TweakedRichTextBox logBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox AlwaysOnTopCheckBox;
		private System.Windows.Forms.Button clearButton;
	}
}