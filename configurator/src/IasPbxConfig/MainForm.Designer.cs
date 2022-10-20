namespace IasPbxConfig
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mainPanel = new System.Windows.Forms.Panel();
			this.bottomStatusStrip = new System.Windows.Forms.StatusStrip();
			this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
			this.mainTreeView = new System.Windows.Forms.TreeView();
			this.designerMainTabControl = new System.Windows.Forms.TabControl();
			this.topMenuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.viewLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mainPanel.SuspendLayout();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.topMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainPanel
			// 
			this.mainPanel.AutoSize = true;
			this.mainPanel.Controls.Add(this.bottomStatusStrip);
			this.mainPanel.Controls.Add(this.mainSplitContainer);
			this.mainPanel.Controls.Add(this.topMenuStrip);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = new System.Drawing.Point(0, 0);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = new System.Drawing.Size(927, 495);
			this.mainPanel.TabIndex = 4;
			this.mainPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
			// 
			// bottomStatusStrip
			// 
			this.bottomStatusStrip.AutoSize = false;
			this.bottomStatusStrip.Location = new System.Drawing.Point(0, 473);
			this.bottomStatusStrip.Name = "bottomStatusStrip";
			this.bottomStatusStrip.Size = new System.Drawing.Size(927, 22);
			this.bottomStatusStrip.TabIndex = 5;
			this.bottomStatusStrip.Text = "statusStrip1";
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.mainSplitContainer.Location = new System.Drawing.Point(3, 27);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.mainTreeView);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.designerMainTabControl);
			this.mainSplitContainer.Size = new System.Drawing.Size(912, 443);
			this.mainSplitContainer.SplitterDistance = 206;
			this.mainSplitContainer.TabIndex = 4;
			// 
			// mainTreeView
			// 
			this.mainTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.mainTreeView.Location = new System.Drawing.Point(4, 3);
			this.mainTreeView.Name = "mainTreeView";
			this.mainTreeView.Size = new System.Drawing.Size(199, 437);
			this.mainTreeView.TabIndex = 0;
			this.mainTreeView.DoubleClick += new System.EventHandler(this.mainTreeView_DoubleClick);
			this.mainTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mainTreeView_MouseDown);
			// 
			// designerMainTabControl
			// 
			this.designerMainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.designerMainTabControl.Location = new System.Drawing.Point(0, 0);
			this.designerMainTabControl.Name = "designerMainTabControl";
			this.designerMainTabControl.SelectedIndex = 0;
			this.designerMainTabControl.Size = new System.Drawing.Size(702, 443);
			this.designerMainTabControl.TabIndex = 0;
			// 
			// topMenuStrip
			// 
			this.topMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.topMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.topMenuStrip.Name = "topMenuStrip";
			this.topMenuStrip.Size = new System.Drawing.Size(927, 24);
			this.topMenuStrip.TabIndex = 3;
			this.topMenuStrip.Text = "menuStrip";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewLogToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.toolsToolStripMenuItem.Text = "Tools";
			// 
			// viewLogToolStripMenuItem
			// 
			this.viewLogToolStripMenuItem.Name = "viewLogToolStripMenuItem";
			this.viewLogToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
			this.viewLogToolStripMenuItem.Text = "View Log";
			this.viewLogToolStripMenuItem.Click += new System.EventHandler(this.viewLogToolStripMenuItem_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
			this.helpToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
			this.aboutToolStripMenuItem.Text = "About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(927, 495);
			this.Controls.Add(this.mainPanel);
			this.IsMdiContainer = true;
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(640, 480);
			this.Name = "MainForm";
			this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainForm_KeyPress);
			this.Move += new System.EventHandler(this.MainForm_Move);
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			this.mainSplitContainer.ResumeLayout(false);
			this.topMenuStrip.ResumeLayout(false);
			this.topMenuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel mainPanel;
		private System.Windows.Forms.SplitContainer mainSplitContainer;
		private System.Windows.Forms.TreeView mainTreeView;
		private System.Windows.Forms.MenuStrip topMenuStrip;
		private System.Windows.Forms.StatusStrip bottomStatusStrip;
		private System.Windows.Forms.TabControl designerMainTabControl;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewLogToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
	}
}

