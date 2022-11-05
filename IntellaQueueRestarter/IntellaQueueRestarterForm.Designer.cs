namespace IntellaQueueRestarter
{
    partial class IntellaQueueRestarterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IntellaQueueRestarterForm));
            this.cmpSysTrayNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmpSysTrayContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmpSysTrayContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmpSysTrayNotifyIcon
            // 
            this.cmpSysTrayNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("cmpSysTrayNotifyIcon.Icon")));
            this.cmpSysTrayNotifyIcon.Text = "IntellaQueueRestarter";
            this.cmpSysTrayNotifyIcon.Visible = true;
            this.cmpSysTrayNotifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.cmpSysTrayNotifyIcon_MouseClick);
            // 
            // cmpSysTrayContextMenuStrip
            // 
            this.cmpSysTrayContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.cmpSysTrayContextMenuStrip.Name = "cmpSysTrayContextMenuStrip";
            this.cmpSysTrayContextMenuStrip.Size = new System.Drawing.Size(110, 48);
            this.cmpSysTrayContextMenuStrip.Click += new System.EventHandler(this.cmpSysTrayContextMenuStrip_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.debugToolStripMenuItem.Text = "Debug";
            this.debugToolStripMenuItem.Click += new System.EventHandler(this.debugToolStripMenuItem_Click);
            // 
            // IntellaQueueRestarterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "IntellaQueueRestarterForm";
            this.Text = "IntellaQueueRestarter";
            this.Load += new System.EventHandler(this.IntellaQueueRestarterForm_Load);
            this.Shown += new System.EventHandler(this.IntellaQueueRestarterForm_Shown);
            this.cmpSysTrayContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon cmpSysTrayNotifyIcon;
        private System.Windows.Forms.ContextMenuStrip cmpSysTrayContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
    }
}

