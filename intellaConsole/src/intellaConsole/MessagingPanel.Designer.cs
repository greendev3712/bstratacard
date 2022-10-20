namespace intellaConsole {
    partial class MessagingPanel {
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
            this.cmpMessageHistoryTextBox = new System.Windows.Forms.TextBox();
            this.cmpMessagingPanel = new System.Windows.Forms.Panel();
            this.cmpMessageHistoryPanel = new System.Windows.Forms.Panel();
            this.cmpMessageBottomBarPanel = new System.Windows.Forms.Panel();
            this.cmpSendMessageTextBoxPanel = new System.Windows.Forms.Panel();
            this.cmpSendButtonPanel = new System.Windows.Forms.Panel();
            this.cmp = new System.Windows.Forms.Button();
            this.cmpSendMessageTextBox = new System.Windows.Forms.TextBox();
            this.cmpSendMessageLabelPanel = new System.Windows.Forms.Panel();
            this.sendMessageLabel = new System.Windows.Forms.Label();
            this.cmpMessageTopbarPanel = new System.Windows.Forms.Panel();
            this.cmpMessageToolbarTopPanelRightPanel = new System.Windows.Forms.Panel();
            this.cmpMessageHistoryTitleBarTalkingToLabel = new System.Windows.Forms.Label();
            this.cmpMessageToolbarTopPanelLeftPanel = new System.Windows.Forms.Panel();
            this.cmpMessageHistoryTitleBarLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmpMessagingPanel.SuspendLayout();
            this.cmpMessageHistoryPanel.SuspendLayout();
            this.cmpMessageBottomBarPanel.SuspendLayout();
            this.cmpSendMessageTextBoxPanel.SuspendLayout();
            this.cmpSendButtonPanel.SuspendLayout();
            this.cmpSendMessageLabelPanel.SuspendLayout();
            this.cmpMessageTopbarPanel.SuspendLayout();
            this.cmpMessageToolbarTopPanelRightPanel.SuspendLayout();
            this.cmpMessageToolbarTopPanelLeftPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmpMessageHistoryTextBox
            // 
            this.cmpMessageHistoryTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpMessageHistoryTextBox.Location = new System.Drawing.Point(0, 0);
            this.cmpMessageHistoryTextBox.Multiline = true;
            this.cmpMessageHistoryTextBox.Name = "cmpMessageHistoryTextBox";
            this.cmpMessageHistoryTextBox.ReadOnly = true;
            this.cmpMessageHistoryTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.cmpMessageHistoryTextBox.Size = new System.Drawing.Size(647, 311);
            this.cmpMessageHistoryTextBox.TabIndex = 0;
            this.cmpMessageHistoryTextBox.VisibleChanged += new System.EventHandler(this.cmpMessageHistoryTextBox_VisibleChanged);
            // 
            // cmpMessagingPanel
            // 
            this.cmpMessagingPanel.AutoSize = true;
            this.cmpMessagingPanel.Controls.Add(this.cmpMessageHistoryPanel);
            this.cmpMessagingPanel.Controls.Add(this.cmpMessageBottomBarPanel);
            this.cmpMessagingPanel.Controls.Add(this.cmpMessageTopbarPanel);
            this.cmpMessagingPanel.Location = new System.Drawing.Point(69, 12);
            this.cmpMessagingPanel.Name = "cmpMessagingPanel";
            this.cmpMessagingPanel.Size = new System.Drawing.Size(647, 355);
            this.cmpMessagingPanel.TabIndex = 1;
            // 
            // cmpMessageHistoryPanel
            // 
            this.cmpMessageHistoryPanel.Controls.Add(this.cmpMessageHistoryTextBox);
            this.cmpMessageHistoryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpMessageHistoryPanel.Location = new System.Drawing.Point(0, 22);
            this.cmpMessageHistoryPanel.Name = "cmpMessageHistoryPanel";
            this.cmpMessageHistoryPanel.Size = new System.Drawing.Size(647, 311);
            this.cmpMessageHistoryPanel.TabIndex = 5;
            // 
            // cmpMessageBottomBarPanel
            // 
            this.cmpMessageBottomBarPanel.Controls.Add(this.cmpSendMessageTextBoxPanel);
            this.cmpMessageBottomBarPanel.Controls.Add(this.cmpSendMessageLabelPanel);
            this.cmpMessageBottomBarPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cmpMessageBottomBarPanel.Location = new System.Drawing.Point(0, 333);
            this.cmpMessageBottomBarPanel.Name = "cmpMessageBottomBarPanel";
            this.cmpMessageBottomBarPanel.Size = new System.Drawing.Size(647, 22);
            this.cmpMessageBottomBarPanel.TabIndex = 4;
            // 
            // cmpSendMessageTextBoxPanel
            // 
            this.cmpSendMessageTextBoxPanel.Controls.Add(this.cmpSendButtonPanel);
            this.cmpSendMessageTextBoxPanel.Controls.Add(this.cmpSendMessageTextBox);
            this.cmpSendMessageTextBoxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpSendMessageTextBoxPanel.Location = new System.Drawing.Point(76, 0);
            this.cmpSendMessageTextBoxPanel.Name = "cmpSendMessageTextBoxPanel";
            this.cmpSendMessageTextBoxPanel.Size = new System.Drawing.Size(571, 22);
            this.cmpSendMessageTextBoxPanel.TabIndex = 0;
            // 
            // cmpSendButtonPanel
            // 
            this.cmpSendButtonPanel.Controls.Add(this.cmp);
            this.cmpSendButtonPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.cmpSendButtonPanel.Location = new System.Drawing.Point(480, 0);
            this.cmpSendButtonPanel.Name = "cmpSendButtonPanel";
            this.cmpSendButtonPanel.Size = new System.Drawing.Size(91, 22);
            this.cmpSendButtonPanel.TabIndex = 4;
            // 
            // cmp
            // 
            this.cmp.BackColor = System.Drawing.Color.Gray;
            this.cmp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmp.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.cmp.Location = new System.Drawing.Point(3, 0);
            this.cmp.Name = "cmp";
            this.cmp.Size = new System.Drawing.Size(85, 23);
            this.cmp.TabIndex = 0;
            this.cmp.Text = "Send";
            this.cmp.UseVisualStyleBackColor = false;
            this.cmp.Click += new System.EventHandler(this.cmp_Click);
            // 
            // cmpSendMessageTextBox
            // 
            this.cmpSendMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpSendMessageTextBox.Location = new System.Drawing.Point(0, 0);
            this.cmpSendMessageTextBox.Name = "cmpSendMessageTextBox";
            this.cmpSendMessageTextBox.Size = new System.Drawing.Size(571, 20);
            this.cmpSendMessageTextBox.TabIndex = 3;
            this.cmpSendMessageTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmpSendMessageTextBox_KeyPress);
            // 
            // cmpSendMessageLabelPanel
            // 
            this.cmpSendMessageLabelPanel.Controls.Add(this.sendMessageLabel);
            this.cmpSendMessageLabelPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.cmpSendMessageLabelPanel.Location = new System.Drawing.Point(0, 0);
            this.cmpSendMessageLabelPanel.Name = "cmpSendMessageLabelPanel";
            this.cmpSendMessageLabelPanel.Size = new System.Drawing.Size(76, 22);
            this.cmpSendMessageLabelPanel.TabIndex = 0;
            // 
            // sendMessageLabel
            // 
            this.sendMessageLabel.AutoSize = true;
            this.sendMessageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendMessageLabel.Location = new System.Drawing.Point(8, 5);
            this.sendMessageLabel.Name = "sendMessageLabel";
            this.sendMessageLabel.Size = new System.Drawing.Size(57, 13);
            this.sendMessageLabel.TabIndex = 0;
            this.sendMessageLabel.Text = "Message";
            // 
            // cmpMessageTopbarPanel
            // 
            this.cmpMessageTopbarPanel.Controls.Add(this.cmpMessageToolbarTopPanelRightPanel);
            this.cmpMessageTopbarPanel.Controls.Add(this.cmpMessageToolbarTopPanelLeftPanel);
            this.cmpMessageTopbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmpMessageTopbarPanel.Location = new System.Drawing.Point(0, 0);
            this.cmpMessageTopbarPanel.Name = "cmpMessageTopbarPanel";
            this.cmpMessageTopbarPanel.Size = new System.Drawing.Size(647, 22);
            this.cmpMessageTopbarPanel.TabIndex = 2;
            // 
            // cmpMessageToolbarTopPanelRightPanel
            // 
            this.cmpMessageToolbarTopPanelRightPanel.Controls.Add(this.cmpMessageHistoryTitleBarTalkingToLabel);
            this.cmpMessageToolbarTopPanelRightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmpMessageToolbarTopPanelRightPanel.Location = new System.Drawing.Point(200, 0);
            this.cmpMessageToolbarTopPanelRightPanel.Name = "cmpMessageToolbarTopPanelRightPanel";
            this.cmpMessageToolbarTopPanelRightPanel.Size = new System.Drawing.Size(447, 22);
            this.cmpMessageToolbarTopPanelRightPanel.TabIndex = 3;
            // 
            // cmpMessageHistoryTitleBarTalkingToLabel
            // 
            this.cmpMessageHistoryTitleBarTalkingToLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpMessageHistoryTitleBarTalkingToLabel.Location = new System.Drawing.Point(6, 5);
            this.cmpMessageHistoryTitleBarTalkingToLabel.Name = "cmpMessageHistoryTitleBarTalkingToLabel";
            this.cmpMessageHistoryTitleBarTalkingToLabel.Size = new System.Drawing.Size(438, 13);
            this.cmpMessageHistoryTitleBarTalkingToLabel.TabIndex = 2;
            this.cmpMessageHistoryTitleBarTalkingToLabel.Text = "Conversation With: ";
            this.cmpMessageHistoryTitleBarTalkingToLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmpMessageToolbarTopPanelLeftPanel
            // 
            this.cmpMessageToolbarTopPanelLeftPanel.Controls.Add(this.cmpMessageHistoryTitleBarLabel);
            this.cmpMessageToolbarTopPanelLeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.cmpMessageToolbarTopPanelLeftPanel.Location = new System.Drawing.Point(0, 0);
            this.cmpMessageToolbarTopPanelLeftPanel.Name = "cmpMessageToolbarTopPanelLeftPanel";
            this.cmpMessageToolbarTopPanelLeftPanel.Size = new System.Drawing.Size(200, 22);
            this.cmpMessageToolbarTopPanelLeftPanel.TabIndex = 0;
            // 
            // cmpMessageHistoryTitleBarLabel
            // 
            this.cmpMessageHistoryTitleBarLabel.AutoSize = true;
            this.cmpMessageHistoryTitleBarLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpMessageHistoryTitleBarLabel.Location = new System.Drawing.Point(4, 5);
            this.cmpMessageHistoryTitleBarLabel.Name = "cmpMessageHistoryTitleBarLabel";
            this.cmpMessageHistoryTitleBarLabel.Size = new System.Drawing.Size(100, 13);
            this.cmpMessageHistoryTitleBarLabel.TabIndex = 1;
            this.cmpMessageHistoryTitleBarLabel.Text = "Message History";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(463, 407);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "label1";
            // 
            // MessagingPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmpMessagingPanel);
            this.Name = "MessagingPanel";
            this.Text = "MessagingForm";
            this.Load += new System.EventHandler(this.MessagingForm_Load);
            this.cmpMessagingPanel.ResumeLayout(false);
            this.cmpMessageHistoryPanel.ResumeLayout(false);
            this.cmpMessageHistoryPanel.PerformLayout();
            this.cmpMessageBottomBarPanel.ResumeLayout(false);
            this.cmpSendMessageTextBoxPanel.ResumeLayout(false);
            this.cmpSendMessageTextBoxPanel.PerformLayout();
            this.cmpSendButtonPanel.ResumeLayout(false);
            this.cmpSendMessageLabelPanel.ResumeLayout(false);
            this.cmpSendMessageLabelPanel.PerformLayout();
            this.cmpMessageTopbarPanel.ResumeLayout(false);
            this.cmpMessageToolbarTopPanelRightPanel.ResumeLayout(false);
            this.cmpMessageToolbarTopPanelLeftPanel.ResumeLayout(false);
            this.cmpMessageToolbarTopPanelLeftPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox cmpMessageHistoryTextBox;
        private System.Windows.Forms.Panel cmpMessagingPanel;
        private System.Windows.Forms.Label cmpMessageHistoryTitleBarLabel;
        private System.Windows.Forms.Panel cmpMessageHistoryPanel;
        private System.Windows.Forms.Panel cmpMessageBottomBarPanel;
        private System.Windows.Forms.TextBox cmpSendMessageTextBox;
        private System.Windows.Forms.Panel cmpMessageTopbarPanel;
        private System.Windows.Forms.Panel cmpSendMessageTextBoxPanel;
        private System.Windows.Forms.Panel cmpSendButtonPanel;
        private System.Windows.Forms.Button cmp;
        private System.Windows.Forms.Panel cmpSendMessageLabelPanel;
        private System.Windows.Forms.Label sendMessageLabel;
        private System.Windows.Forms.Panel cmpMessageToolbarTopPanelRightPanel;
        private System.Windows.Forms.Label cmpMessageHistoryTitleBarTalkingToLabel;
        private System.Windows.Forms.Panel cmpMessageToolbarTopPanelLeftPanel;
        private System.Windows.Forms.Label label1;
    }
}