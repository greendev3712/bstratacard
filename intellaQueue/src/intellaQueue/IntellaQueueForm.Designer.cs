namespace intellaQueue
{
	partial class IntellaQueueForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IntellaQueueForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dropGridTemplate = new System.Windows.Forms.DataGridView();
            this.MainContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.AboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mediumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.largeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayQueuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AdminToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sizeAdjustorPb = new System.Windows.Forms.PictureBox();
            this.settingsPb = new System.Windows.Forms.PictureBox();
            this.minimizePb = new System.Windows.Forms.PictureBox();
            this.bodyPb = new System.Windows.Forms.PictureBox();
            this.titlePb = new System.Windows.Forms.PictureBox();
            this.footerPb = new System.Windows.Forms.PictureBox();
            this.closePb = new System.Windows.Forms.PictureBox();
            this.statusLight = new System.Windows.Forms.PictureBox();
            this.titleAppNameLabel = new System.Windows.Forms.Label();
            this.titleViewNameLabel = new System.Windows.Forms.Label();
            this.hiddenGrid = new System.Windows.Forms.DataGridView();
            this.detailViewNameLabel = new System.Windows.Forms.Label();
            this.QueueRightClickContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.takeNextWaitingCallerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmpAgentManagerRightClickContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmpSetAgentStatusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmpSetAgentStatusPerQueueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmpLogoutAgentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmpTitleBarRightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmpSetAgentExtensionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainGrid = new Lib.TweakedDataGridView();
            this.cmpSysTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmpSystrayContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmpSystrayContextMenuItemAdmin = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.setAgentExtensionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmpExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dropGridTemplate)).BeginInit();
            this.MainContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sizeAdjustorPb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.settingsPb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minimizePb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bodyPb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.titlePb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.footerPb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.closePb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusLight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hiddenGrid)).BeginInit();
            this.QueueRightClickContextMenu.SuspendLayout();
            this.cmpAgentManagerRightClickContextMenu.SuspendLayout();
            this.cmpTitleBarRightClickMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainGrid)).BeginInit();
            this.cmpSystrayContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // dropGridTemplate
            // 
            this.dropGridTemplate.AllowUserToAddRows = false;
            this.dropGridTemplate.AllowUserToDeleteRows = false;
            this.dropGridTemplate.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dropGridTemplate.BackgroundColor = System.Drawing.SystemColors.ControlDark;
            this.dropGridTemplate.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dropGridTemplate.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dropGridTemplate.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dropGridTemplate.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dropGridTemplate.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dropGridTemplate.DefaultCellStyle = dataGridViewCellStyle2;
            this.dropGridTemplate.GridColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dropGridTemplate.Location = new System.Drawing.Point(0, 40);
            this.dropGridTemplate.Name = "dropGridTemplate";
            this.dropGridTemplate.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dropGridTemplate.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dropGridTemplate.RowHeadersVisible = false;
            this.dropGridTemplate.ShowEditingIcon = false;
            this.dropGridTemplate.Size = new System.Drawing.Size(581, 22);
            this.dropGridTemplate.TabIndex = 108;
            this.dropGridTemplate.Visible = false;
            this.dropGridTemplate.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DropGridTemplate_CellDoubleClick);
            // 
            // MainContextMenu
            // 
            this.MainContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AboutToolStripMenuItem,
            this.preferencesToolStripMenuItem,
            this.checkForUpdatesToolStripMenuItem,
            this.alwaysOnTopToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.AdminToolStripMenuItem});
            this.MainContextMenu.Name = "ContextMenuStrip1";
            this.MainContextMenu.Size = new System.Drawing.Size(189, 136);
            this.MainContextMenu.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.MainContextMenu_Closed);
            this.MainContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.MainContextMenu_Opening);
            // 
            // AboutToolStripMenuItem
            // 
            this.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem";
            this.AboutToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.AboutToolStripMenuItem.Text = "About";
            this.AboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // preferencesToolStripMenuItem
            // 
            this.preferencesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aToolStripMenuItem,
            this.displayQueuesToolStripMenuItem});
            this.preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            this.preferencesToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.preferencesToolStripMenuItem.Text = "Preferences";
            // 
            // aToolStripMenuItem
            // 
            this.aToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.smallToolStripMenuItem,
            this.mediumToolStripMenuItem,
            this.largeToolStripMenuItem});
            this.aToolStripMenuItem.Name = "aToolStripMenuItem";
            this.aToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.aToolStripMenuItem.Text = "Interface Size";
            // 
            // smallToolStripMenuItem
            // 
            this.smallToolStripMenuItem.Name = "smallToolStripMenuItem";
            this.smallToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.smallToolStripMenuItem.Text = "Small";
            this.smallToolStripMenuItem.Click += new System.EventHandler(this.smallToolStripMenuItem_Click);
            // 
            // mediumToolStripMenuItem
            // 
            this.mediumToolStripMenuItem.Name = "mediumToolStripMenuItem";
            this.mediumToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.mediumToolStripMenuItem.Text = "Medium";
            this.mediumToolStripMenuItem.Click += new System.EventHandler(this.mediumToolStripMenuItem_Click);
            // 
            // largeToolStripMenuItem
            // 
            this.largeToolStripMenuItem.Name = "largeToolStripMenuItem";
            this.largeToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.largeToolStripMenuItem.Text = "Large";
            this.largeToolStripMenuItem.Click += new System.EventHandler(this.largeToolStripMenuItem_Click);
            // 
            // displayQueuesToolStripMenuItem
            // 
            this.displayQueuesToolStripMenuItem.Name = "displayQueuesToolStripMenuItem";
            this.displayQueuesToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.displayQueuesToolStripMenuItem.Text = "Display Queues";
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.checkForUpdatesToolStripMenuItem.Text = "Check For Updates";
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
            // 
            // alwaysOnTopToolStripMenuItem
            // 
            this.alwaysOnTopToolStripMenuItem.Checked = true;
            this.alwaysOnTopToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
            this.alwaysOnTopToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.alwaysOnTopToolStripMenuItem.Text = "Always on top";
            this.alwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.alwaysOnTopToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.debugToolStripMenuItem.Text = "Show Debug Window";
            this.debugToolStripMenuItem.Click += new System.EventHandler(this.debugToolStripMenuItem_Click);
            // 
            // AdminToolStripMenuItem
            // 
            this.AdminToolStripMenuItem.Name = "AdminToolStripMenuItem";
            this.AdminToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.AdminToolStripMenuItem.Text = "Admin";
            this.AdminToolStripMenuItem.Click += new System.EventHandler(this.AdminToolStripMenuItem_Click);
            // 
            // sizeAdjustorPb
            // 
            this.sizeAdjustorPb.BackColor = System.Drawing.Color.Transparent;
            this.sizeAdjustorPb.Location = new System.Drawing.Point(314, 20);
            this.sizeAdjustorPb.Name = "sizeAdjustorPb";
            this.sizeAdjustorPb.Size = new System.Drawing.Size(10, 19);
            this.sizeAdjustorPb.TabIndex = 120;
            this.sizeAdjustorPb.TabStop = false;
            this.sizeAdjustorPb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.sizeAdjustorPb_MouseDown);
            this.sizeAdjustorPb.MouseMove += new System.Windows.Forms.MouseEventHandler(this.sizeAdjustorPb_MouseMove);
            this.sizeAdjustorPb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.sizeAdjustorPb_MouseUp);
            // 
            // settingsPb
            // 
            this.settingsPb.BackColor = System.Drawing.Color.Transparent;
            this.settingsPb.BackgroundImage = global::QueueLib.queueResources.settings;
            this.settingsPb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.settingsPb.ErrorImage = ((System.Drawing.Image)(resources.GetObject("settingsPb.ErrorImage")));
            this.settingsPb.Location = new System.Drawing.Point(267, 0);
            this.settingsPb.Name = "settingsPb";
            this.settingsPb.Size = new System.Drawing.Size(19, 19);
            this.settingsPb.TabIndex = 115;
            this.settingsPb.TabStop = false;
            this.settingsPb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.settingsPb.MouseEnter += new System.EventHandler(this.button_MouseEnter);
            this.settingsPb.MouseLeave += new System.EventHandler(this.button_MouseLeave);
            this.settingsPb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            // 
            // minimizePb
            // 
            this.minimizePb.BackColor = System.Drawing.Color.Transparent;
            this.minimizePb.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("minimizePb.BackgroundImage")));
            this.minimizePb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.minimizePb.ErrorImage = ((System.Drawing.Image)(resources.GetObject("minimizePb.ErrorImage")));
            this.minimizePb.Location = new System.Drawing.Point(287, 3);
            this.minimizePb.Name = "minimizePb";
            this.minimizePb.Size = new System.Drawing.Size(14, 14);
            this.minimizePb.TabIndex = 114;
            this.minimizePb.TabStop = false;
            this.minimizePb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.minimizePb.MouseEnter += new System.EventHandler(this.button_MouseEnter);
            this.minimizePb.MouseLeave += new System.EventHandler(this.button_MouseLeave);
            this.minimizePb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            // 
            // bodyPb
            // 
            this.bodyPb.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("bodyPb.BackgroundImage")));
            this.bodyPb.Location = new System.Drawing.Point(0, 20);
            this.bodyPb.Name = "bodyPb";
            this.bodyPb.Size = new System.Drawing.Size(325, 48);
            this.bodyPb.TabIndex = 112;
            this.bodyPb.TabStop = false;
            this.bodyPb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseDown);
            this.bodyPb.MouseMove += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseMove);
            this.bodyPb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseUp);
            // 
            // titlePb
            // 
            this.titlePb.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("titlePb.BackgroundImage")));
            this.titlePb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.titlePb.Location = new System.Drawing.Point(0, 0);
            this.titlePb.Name = "titlePb";
            this.titlePb.Size = new System.Drawing.Size(325, 20);
            this.titlePb.TabIndex = 111;
            this.titlePb.TabStop = false;
            this.titlePb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseDown);
            this.titlePb.MouseMove += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseMove);
            this.titlePb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseUp);
            // 
            // footerPb
            // 
            this.footerPb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.footerPb.Location = new System.Drawing.Point(0, 68);
            this.footerPb.Name = "footerPb";
            this.footerPb.Size = new System.Drawing.Size(25, 20);
            this.footerPb.TabIndex = 113;
            this.footerPb.TabStop = false;
            this.footerPb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseDown);
            this.footerPb.MouseMove += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseMove);
            this.footerPb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseUp);
            // 
            // closePb
            // 
            this.closePb.BackColor = System.Drawing.Color.Transparent;
            this.closePb.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("closePb.BackgroundImage")));
            this.closePb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.closePb.ErrorImage = ((System.Drawing.Image)(resources.GetObject("closePb.ErrorImage")));
            this.closePb.Location = new System.Drawing.Point(304, 3);
            this.closePb.Name = "closePb";
            this.closePb.Size = new System.Drawing.Size(14, 14);
            this.closePb.TabIndex = 102;
            this.closePb.TabStop = false;
            this.closePb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.closePb.MouseEnter += new System.EventHandler(this.button_MouseEnter);
            this.closePb.MouseLeave += new System.EventHandler(this.button_MouseLeave);
            this.closePb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            // 
            // statusLight
            // 
            this.statusLight.BackColor = System.Drawing.Color.Transparent;
            this.statusLight.BackgroundImage = global::QueueLib.queueResources.status_error;
            this.statusLight.Location = new System.Drawing.Point(285, 23);
            this.statusLight.Name = "statusLight";
            this.statusLight.Size = new System.Drawing.Size(22, 12);
            this.statusLight.TabIndex = 106;
            this.statusLight.TabStop = false;
            this.statusLight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseDown);
            this.statusLight.MouseMove += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseMove);
            this.statusLight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseUp);
            // 
            // titleAppNameLabel
            // 
            this.titleAppNameLabel.AutoSize = true;
            this.titleAppNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.titleAppNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleAppNameLabel.ForeColor = System.Drawing.Color.White;
            this.titleAppNameLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.titleAppNameLabel.Location = new System.Drawing.Point(7, 3);
            this.titleAppNameLabel.Name = "titleAppNameLabel";
            this.titleAppNameLabel.Size = new System.Drawing.Size(78, 13);
            this.titleAppNameLabel.TabIndex = 121;
            this.titleAppNameLabel.Text = "intellaQueue";
            this.titleAppNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.titleAppNameLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseDown);
            this.titleAppNameLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseMove);
            this.titleAppNameLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseUp);
            // 
            // titleViewNameLabel
            // 
            this.titleViewNameLabel.AutoSize = true;
            this.titleViewNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.titleViewNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleViewNameLabel.ForeColor = System.Drawing.Color.White;
            this.titleViewNameLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.titleViewNameLabel.Location = new System.Drawing.Point(112, 3);
            this.titleViewNameLabel.Name = "titleViewNameLabel";
            this.titleViewNameLabel.Size = new System.Drawing.Size(103, 13);
            this.titleViewNameLabel.TabIndex = 122;
            this.titleViewNameLabel.Text = "Manager Toolbar";
            this.titleViewNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.titleViewNameLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseDown);
            this.titleViewNameLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseMove);
            this.titleViewNameLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseUp);
            // 
            // hiddenGrid
            // 
            this.hiddenGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.hiddenGrid.Location = new System.Drawing.Point(193, 3);
            this.hiddenGrid.Name = "hiddenGrid";
            this.hiddenGrid.Size = new System.Drawing.Size(36, 23);
            this.hiddenGrid.TabIndex = 124;
            this.hiddenGrid.Visible = false;
            // 
            // detailViewNameLabel
            // 
            this.detailViewNameLabel.AutoSize = true;
            this.detailViewNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.detailViewNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.detailViewNameLabel.ForeColor = System.Drawing.Color.White;
            this.detailViewNameLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailViewNameLabel.Location = new System.Drawing.Point(111, 71);
            this.detailViewNameLabel.Name = "detailViewNameLabel";
            this.detailViewNameLabel.Size = new System.Drawing.Size(0, 13);
            this.detailViewNameLabel.TabIndex = 125;
            this.detailViewNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // QueueRightClickContextMenu
            // 
            this.QueueRightClickContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.takeNextWaitingCallerToolStripMenuItem});
            this.QueueRightClickContextMenu.Name = "QueueRightClickContextMenu";
            this.QueueRightClickContextMenu.Size = new System.Drawing.Size(203, 26);
            // 
            // takeNextWaitingCallerToolStripMenuItem
            // 
            this.takeNextWaitingCallerToolStripMenuItem.Name = "takeNextWaitingCallerToolStripMenuItem";
            this.takeNextWaitingCallerToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.takeNextWaitingCallerToolStripMenuItem.Text = "Take Next Waiting Caller";
            this.takeNextWaitingCallerToolStripMenuItem.Click += new System.EventHandler(this.takeNextWaitingCallerToolStripMenuItem_Click);
            // 
            // cmpAgentManagerRightClickContextMenu
            // 
            this.cmpAgentManagerRightClickContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmpSetAgentStatusToolStripMenuItem,
            this.cmpSetAgentStatusPerQueueToolStripMenuItem,
            this.cmpLogoutAgentToolStripMenuItem});
            this.cmpAgentManagerRightClickContextMenu.Name = "cmpAgentManagerRightClickContextMenu";
            this.cmpAgentManagerRightClickContextMenu.Size = new System.Drawing.Size(178, 70);
            // 
            // cmpSetAgentStatusToolStripMenuItem
            // 
            this.cmpSetAgentStatusToolStripMenuItem.Name = "cmpSetAgentStatusToolStripMenuItem";
            this.cmpSetAgentStatusToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.cmpSetAgentStatusToolStripMenuItem.Text = "Set Agent Status All";
            // 
            // cmpSetAgentStatusPerQueueToolStripMenuItem
            // 
            this.cmpSetAgentStatusPerQueueToolStripMenuItem.Name = "cmpSetAgentStatusPerQueueToolStripMenuItem";
            this.cmpSetAgentStatusPerQueueToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.cmpSetAgentStatusPerQueueToolStripMenuItem.Text = "Set Agent Status";
            // 
            // cmpLogoutAgentToolStripMenuItem
            // 
            this.cmpLogoutAgentToolStripMenuItem.Name = "cmpLogoutAgentToolStripMenuItem";
            this.cmpLogoutAgentToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.cmpLogoutAgentToolStripMenuItem.Text = "Logout Agent";
            this.cmpLogoutAgentToolStripMenuItem.Click += new System.EventHandler(this.cmpLogoutAgentToolStripMenuItem_Click);
            // 
            // cmpTitleBarRightClickMenu
            // 
            this.cmpTitleBarRightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmpSetAgentExtensionToolStripMenuItem});
            this.cmpTitleBarRightClickMenu.Name = "cmpAgentManagerRightClickContextMenu";
            this.cmpTitleBarRightClickMenu.Size = new System.Drawing.Size(180, 26);
            // 
            // cmpSetAgentExtensionToolStripMenuItem
            // 
            this.cmpSetAgentExtensionToolStripMenuItem.Name = "cmpSetAgentExtensionToolStripMenuItem";
            this.cmpSetAgentExtensionToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.cmpSetAgentExtensionToolStripMenuItem.Text = "Set Agent Extension";
            this.cmpSetAgentExtensionToolStripMenuItem.Click += new System.EventHandler(this.cmpSetAgentExtensionToolStripMenuItem_Click);
            // 
            // mainGrid
            // 
            this.mainGrid.AllowUserToAddRows = false;
            this.mainGrid.AllowUserToDeleteRows = false;
            this.mainGrid.AllowUserToResizeColumns = false;
            this.mainGrid.AllowUserToResizeRows = false;
            this.mainGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.mainGrid.BackgroundColor = System.Drawing.SystemColors.ControlDark;
            this.mainGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.mainGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(193)))), ((int)(((byte)(193)))), ((int)(((byte)(193)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.mainGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.mainGrid.ColumnHeadersHeight = 16;
            this.mainGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(173)))), ((int)(((byte)(173)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(173)))), ((int)(((byte)(173)))));
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.mainGrid.DefaultCellStyle = dataGridViewCellStyle5;
            this.mainGrid.EnableHeadersVisualStyles = false;
            this.mainGrid.GridColor = System.Drawing.SystemColors.ControlDarkDark;
            this.mainGrid.Location = new System.Drawing.Point(0, 20);
            this.mainGrid.MultiSelect = false;
            this.mainGrid.Name = "mainGrid";
            this.mainGrid.ReadOnly = true;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.mainGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.mainGrid.RowHeadersVisible = false;
            this.mainGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.mainGrid.RowTemplate.Height = 13;
            this.mainGrid.ShowCellToolTips = false;
            this.mainGrid.ShowEditingIcon = false;
            this.mainGrid.Size = new System.Drawing.Size(325, 0);
            this.mainGrid.TabIndex = 123;
            this.mainGrid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.mainGrid_CellMouseDown);
            this.mainGrid.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.mainGrid_CellMouseEnter);
            this.mainGrid.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.mainGrid_CellMouseLeave);
            this.mainGrid.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.mainGrid_CellMouseUp);
            this.mainGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.mainGrid_CellPainting);
            this.mainGrid.SelectionChanged += new System.EventHandler(this.mainGrid_SelectionChanged);
            this.mainGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mainGrid_MouseDown);
            // 
            // cmpSysTrayIcon
            // 
            this.cmpSysTrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("cmpSysTrayIcon.Icon")));
            this.cmpSysTrayIcon.Text = "IntellaToolbar";
            this.cmpSysTrayIcon.Visible = true;
            this.cmpSysTrayIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cmpSysTrayIcon_MouseDown);
            // 
            // cmpSystrayContextMenu
            // 
            this.cmpSystrayContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem1,
            this.cmpSystrayContextMenuItemAdmin,
            this.debugToolStripMenuItem1,
            this.setAgentExtensionToolStripMenuItem,
            this.cmpExitToolStripMenuItem});
            this.cmpSystrayContextMenu.Name = "cmpSystrayContextMenu";
            this.cmpSystrayContextMenu.Size = new System.Drawing.Size(181, 136);
            // 
            // cmpSystrayContextMenuItemAdmin
            // 
            this.cmpSystrayContextMenuItemAdmin.Name = "cmpSystrayContextMenuItemAdmin";
            this.cmpSystrayContextMenuItemAdmin.Size = new System.Drawing.Size(180, 22);
            this.cmpSystrayContextMenuItemAdmin.Text = "Admin";
            this.cmpSystrayContextMenuItemAdmin.Click += new System.EventHandler(this.AdminToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem1
            // 
            this.debugToolStripMenuItem1.Name = "debugToolStripMenuItem1";
            this.debugToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.debugToolStripMenuItem1.Text = "Debug";
            this.debugToolStripMenuItem1.Click += new System.EventHandler(this.debugToolStripMenuItem1_Click);
            // 
            // setAgentExtensionToolStripMenuItem
            // 
            this.setAgentExtensionToolStripMenuItem.Name = "setAgentExtensionToolStripMenuItem";
            this.setAgentExtensionToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.setAgentExtensionToolStripMenuItem.Text = "Set Agent Extension";
            this.setAgentExtensionToolStripMenuItem.Click += new System.EventHandler(this.cmpSetAgentExtensionToolStripMenuItem_Click);
            // 
            // cmpExitToolStripMenuItem
            // 
            this.cmpExitToolStripMenuItem.Name = "cmpExitToolStripMenuItem";
            this.cmpExitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.cmpExitToolStripMenuItem.Text = "Exit";
            this.cmpExitToolStripMenuItem.Click += new System.EventHandler(this.cmpExitToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.aboutToolStripMenuItem1.Text = "About";
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem1_Click);
            // 
            // IntellaQueueForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(325, 110);
            this.Controls.Add(this.detailViewNameLabel);
            this.Controls.Add(this.titleViewNameLabel);
            this.Controls.Add(this.titleAppNameLabel);
            this.Controls.Add(this.hiddenGrid);
            this.Controls.Add(this.sizeAdjustorPb);
            this.Controls.Add(this.mainGrid);
            this.Controls.Add(this.settingsPb);
            this.Controls.Add(this.minimizePb);
            this.Controls.Add(this.bodyPb);
            this.Controls.Add(this.titlePb);
            this.Controls.Add(this.footerPb);
            this.Controls.Add(this.closePb);
            this.Controls.Add(this.dropGridTemplate);
            this.Controls.Add(this.statusLight);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(900, 5);
            this.Name = "IntellaQueueForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "IntellaQueue";
            this.Load += new System.EventHandler(this.Form_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.intellaQueue_MouseUp);
            this.Resize += new System.EventHandler(this.intellaQueue_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.dropGridTemplate)).EndInit();
            this.MainContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sizeAdjustorPb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.settingsPb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minimizePb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bodyPb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.titlePb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.footerPb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.closePb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusLight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hiddenGrid)).EndInit();
            this.QueueRightClickContextMenu.ResumeLayout(false);
            this.cmpAgentManagerRightClickContextMenu.ResumeLayout(false);
            this.cmpTitleBarRightClickMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainGrid)).EndInit();
            this.cmpSystrayContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.DataGridView dropGridTemplate;
		internal System.Windows.Forms.ContextMenuStrip MainContextMenu;
		internal System.Windows.Forms.ToolStripMenuItem AdminToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem AboutToolStripMenuItem;
		internal System.Windows.Forms.PictureBox closePb;
		internal System.Windows.Forms.PictureBox statusLight;
		private System.Windows.Forms.PictureBox titlePb;
		private System.Windows.Forms.PictureBox bodyPb;
		private System.Windows.Forms.PictureBox footerPb;
		internal System.Windows.Forms.PictureBox minimizePb;
		internal System.Windows.Forms.PictureBox settingsPb;
		private System.Windows.Forms.ToolStripMenuItem alwaysOnTopToolStripMenuItem;
		internal System.Windows.Forms.PictureBox sizeAdjustorPb;
		private System.Windows.Forms.Label titleAppNameLabel;
		private System.Windows.Forms.Label titleViewNameLabel;
		private Lib.TweakedDataGridView mainGrid;
		private System.Windows.Forms.DataGridView hiddenGrid;
		private System.Windows.Forms.Label detailViewNameLabel;
        private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mediumToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem largeToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip QueueRightClickContextMenu;
        private System.Windows.Forms.ToolStripMenuItem takeNextWaitingCallerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmpAgentManagerRightClickContextMenu;
        private System.Windows.Forms.ToolStripMenuItem cmpSetAgentStatusToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayQueuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cmpSetAgentStatusPerQueueToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmpTitleBarRightClickMenu;
        private System.Windows.Forms.ToolStripMenuItem cmpSetAgentExtensionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cmpLogoutAgentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon cmpSysTrayIcon;
        private System.Windows.Forms.ContextMenuStrip cmpSystrayContextMenu;
        private System.Windows.Forms.ToolStripMenuItem cmpSystrayContextMenuItemAdmin;
        private System.Windows.Forms.ToolStripMenuItem setAgentExtensionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem cmpExitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
    }
}

