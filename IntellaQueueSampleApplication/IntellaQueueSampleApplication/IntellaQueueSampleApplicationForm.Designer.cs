namespace IntellaQueueSampleApplication {
    partial class IntellaQueueSampleApplicationForm {
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
            this.label1 = new System.Windows.Forms.Label();
            this.cmpExtensionTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmpAgentNumberTextBox = new System.Windows.Forms.TextBox();
            this.cmpLogoutButton = new System.Windows.Forms.Button();
            this.cmpPauseButton = new System.Windows.Forms.Button();
            this.cmpPauseComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmpUnPauseButton = new System.Windows.Forms.Button();
            this.cmpSimilateDialerPostBackButton = new System.Windows.Forms.Button();
            this.cmpDialedNumberTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cmpCaseNumberTextBox = new System.Windows.Forms.TextBox();
            this.cmpMessagesTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cmpCurrentStatusTextBox = new System.Windows.Forms.TextBox();
            this.cmpLoginButton = new System.Windows.Forms.Button();
            this.cmpCallLogIDTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cmpResultCodeTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cmpAgentCallsListBox = new System.Windows.Forms.ListBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cmpRecordingStartButton = new System.Windows.Forms.Button();
            this.cmpRecordingStopButton = new System.Windows.Forms.Button();
            this.cmpApiTestButton = new System.Windows.Forms.Button();
            this.cmpClickToCallTextBox = new System.Windows.Forms.TextBox();
            this.cmpClickToCallLabel = new System.Windows.Forms.Label();
            this.cmpClickToCallButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(42, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Extension";
            // 
            // cmpExtensionTextBox
            // 
            this.cmpExtensionTextBox.Location = new System.Drawing.Point(131, 36);
            this.cmpExtensionTextBox.Name = "cmpExtensionTextBox";
            this.cmpExtensionTextBox.Size = new System.Drawing.Size(100, 20);
            this.cmpExtensionTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(42, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Agent Number";
            // 
            // cmpAgentNumberTextBox
            // 
            this.cmpAgentNumberTextBox.Location = new System.Drawing.Point(131, 66);
            this.cmpAgentNumberTextBox.Name = "cmpAgentNumberTextBox";
            this.cmpAgentNumberTextBox.Size = new System.Drawing.Size(100, 20);
            this.cmpAgentNumberTextBox.TabIndex = 3;
            // 
            // cmpLogoutButton
            // 
            this.cmpLogoutButton.Location = new System.Drawing.Point(126, 107);
            this.cmpLogoutButton.Name = "cmpLogoutButton";
            this.cmpLogoutButton.Size = new System.Drawing.Size(75, 23);
            this.cmpLogoutButton.TabIndex = 5;
            this.cmpLogoutButton.Text = "Logout";
            this.cmpLogoutButton.UseVisualStyleBackColor = true;
            this.cmpLogoutButton.Click += new System.EventHandler(this.cmpLogoutButton_Click);
            // 
            // cmpPauseButton
            // 
            this.cmpPauseButton.Location = new System.Drawing.Point(551, 125);
            this.cmpPauseButton.Name = "cmpPauseButton";
            this.cmpPauseButton.Size = new System.Drawing.Size(75, 23);
            this.cmpPauseButton.TabIndex = 6;
            this.cmpPauseButton.Text = "Set Pause";
            this.cmpPauseButton.UseVisualStyleBackColor = true;
            this.cmpPauseButton.Click += new System.EventHandler(this.cmpPauseButton_Click);
            // 
            // cmpPauseComboBox
            // 
            this.cmpPauseComboBox.FormattingEnabled = true;
            this.cmpPauseComboBox.Location = new System.Drawing.Point(424, 141);
            this.cmpPauseComboBox.Name = "cmpPauseComboBox";
            this.cmpPauseComboBox.Size = new System.Drawing.Size(121, 21);
            this.cmpPauseComboBox.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(422, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(114, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Pause Queue/Reason";
            // 
            // cmpUnPauseButton
            // 
            this.cmpUnPauseButton.Location = new System.Drawing.Point(551, 154);
            this.cmpUnPauseButton.Name = "cmpUnPauseButton";
            this.cmpUnPauseButton.Size = new System.Drawing.Size(75, 23);
            this.cmpUnPauseButton.TabIndex = 9;
            this.cmpUnPauseButton.Text = "UnPause All";
            this.cmpUnPauseButton.UseVisualStyleBackColor = true;
            this.cmpUnPauseButton.Click += new System.EventHandler(this.cmpUnPauseButton_Click);
            // 
            // cmpSimilateDialerPostBackButton
            // 
            this.cmpSimilateDialerPostBackButton.Location = new System.Drawing.Point(314, 84);
            this.cmpSimilateDialerPostBackButton.Name = "cmpSimilateDialerPostBackButton";
            this.cmpSimilateDialerPostBackButton.Size = new System.Drawing.Size(312, 23);
            this.cmpSimilateDialerPostBackButton.TabIndex = 10;
            this.cmpSimilateDialerPostBackButton.Text = "Simulate Dialer Postback";
            this.cmpSimilateDialerPostBackButton.UseVisualStyleBackColor = true;
            this.cmpSimilateDialerPostBackButton.Click += new System.EventHandler(this.cmpSimulateDialerPostBackButton_Click);
            // 
            // cmpDialedNumberTextBox
            // 
            this.cmpDialedNumberTextBox.Location = new System.Drawing.Point(420, 36);
            this.cmpDialedNumberTextBox.Name = "cmpDialedNumberTextBox";
            this.cmpDialedNumberTextBox.Size = new System.Drawing.Size(100, 20);
            this.cmpDialedNumberTextBox.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(417, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Dialed Number";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(523, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Case Number";
            // 
            // cmpCaseNumberTextBox
            // 
            this.cmpCaseNumberTextBox.Location = new System.Drawing.Point(526, 36);
            this.cmpCaseNumberTextBox.Name = "cmpCaseNumberTextBox";
            this.cmpCaseNumberTextBox.Size = new System.Drawing.Size(100, 20);
            this.cmpCaseNumberTextBox.TabIndex = 14;
            // 
            // cmpMessagesTextBox
            // 
            this.cmpMessagesTextBox.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpMessagesTextBox.Location = new System.Drawing.Point(12, 474);
            this.cmpMessagesTextBox.Multiline = true;
            this.cmpMessagesTextBox.Name = "cmpMessagesTextBox";
            this.cmpMessagesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.cmpMessagesTextBox.Size = new System.Drawing.Size(1001, 96);
            this.cmpMessagesTextBox.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 458);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Messages";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 328);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(74, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Current Status";
            // 
            // cmpCurrentStatusTextBox
            // 
            this.cmpCurrentStatusTextBox.Location = new System.Drawing.Point(12, 344);
            this.cmpCurrentStatusTextBox.Multiline = true;
            this.cmpCurrentStatusTextBox.Name = "cmpCurrentStatusTextBox";
            this.cmpCurrentStatusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.cmpCurrentStatusTextBox.Size = new System.Drawing.Size(1001, 96);
            this.cmpCurrentStatusTextBox.TabIndex = 18;
            // 
            // cmpLoginButton
            // 
            this.cmpLoginButton.Location = new System.Drawing.Point(45, 107);
            this.cmpLoginButton.Name = "cmpLoginButton";
            this.cmpLoginButton.Size = new System.Drawing.Size(75, 23);
            this.cmpLoginButton.TabIndex = 19;
            this.cmpLoginButton.Text = "Login";
            this.cmpLoginButton.UseVisualStyleBackColor = true;
            this.cmpLoginButton.Click += new System.EventHandler(this.cmpLoginButton_Click);
            // 
            // cmpCallLogIDTextBox
            // 
            this.cmpCallLogIDTextBox.Location = new System.Drawing.Point(314, 36);
            this.cmpCallLogIDTextBox.Name = "cmpCallLogIDTextBox";
            this.cmpCallLogIDTextBox.Size = new System.Drawing.Size(100, 20);
            this.cmpCallLogIDTextBox.TabIndex = 20;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(311, 20);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "CallLogID";
            // 
            // cmpResultCodeTextBox
            // 
            this.cmpResultCodeTextBox.Location = new System.Drawing.Point(382, 62);
            this.cmpResultCodeTextBox.Name = "cmpResultCodeTextBox";
            this.cmpResultCodeTextBox.Size = new System.Drawing.Size(244, 20);
            this.cmpResultCodeTextBox.TabIndex = 22;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(311, 65);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 13);
            this.label9.TabIndex = 23;
            this.label9.Text = "Result Code";
            // 
            // cmpAgentCallsListBox
            // 
            this.cmpAgentCallsListBox.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmpAgentCallsListBox.FormattingEnabled = true;
            this.cmpAgentCallsListBox.Location = new System.Drawing.Point(15, 183);
            this.cmpAgentCallsListBox.Name = "cmpAgentCallsListBox";
            this.cmpAgentCallsListBox.Size = new System.Drawing.Size(998, 95);
            this.cmpAgentCallsListBox.TabIndex = 25;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 164);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(46, 13);
            this.label10.TabIndex = 26;
            this.label10.Text = "My Calls";
            // 
            // cmpRecordingStartButton
            // 
            this.cmpRecordingStartButton.Location = new System.Drawing.Point(15, 284);
            this.cmpRecordingStartButton.Name = "cmpRecordingStartButton";
            this.cmpRecordingStartButton.Size = new System.Drawing.Size(121, 23);
            this.cmpRecordingStartButton.TabIndex = 27;
            this.cmpRecordingStartButton.Text = "Recording Start";
            this.cmpRecordingStartButton.UseVisualStyleBackColor = true;
            this.cmpRecordingStartButton.Click += new System.EventHandler(this.cmpRecordingStartButton_Click);
            // 
            // cmpRecordingStopButton
            // 
            this.cmpRecordingStopButton.Location = new System.Drawing.Point(142, 284);
            this.cmpRecordingStopButton.Name = "cmpRecordingStopButton";
            this.cmpRecordingStopButton.Size = new System.Drawing.Size(121, 23);
            this.cmpRecordingStopButton.TabIndex = 28;
            this.cmpRecordingStopButton.Text = "Recording Stop";
            this.cmpRecordingStopButton.UseVisualStyleBackColor = true;
            this.cmpRecordingStopButton.Click += new System.EventHandler(this.cmpRecordingStopButton_Click);
            // 
            // cmpApiTestButton
            // 
            this.cmpApiTestButton.Location = new System.Drawing.Point(89, 136);
            this.cmpApiTestButton.Name = "cmpApiTestButton";
            this.cmpApiTestButton.Size = new System.Drawing.Size(75, 23);
            this.cmpApiTestButton.TabIndex = 29;
            this.cmpApiTestButton.Text = "API Test";
            this.cmpApiTestButton.UseVisualStyleBackColor = true;
            this.cmpApiTestButton.Click += new System.EventHandler(this.cmpApiTestButton_Click);
            // 
            // cmpClickToCallTextBox
            // 
            this.cmpClickToCallTextBox.Location = new System.Drawing.Point(695, 36);
            this.cmpClickToCallTextBox.Name = "cmpClickToCallTextBox";
            this.cmpClickToCallTextBox.Size = new System.Drawing.Size(100, 20);
            this.cmpClickToCallTextBox.TabIndex = 30;
            // 
            // cmpClickToCallLabel
            // 
            this.cmpClickToCallLabel.AutoSize = true;
            this.cmpClickToCallLabel.Location = new System.Drawing.Point(692, 20);
            this.cmpClickToCallLabel.Name = "cmpClickToCallLabel";
            this.cmpClickToCallLabel.Size = new System.Drawing.Size(62, 13);
            this.cmpClickToCallLabel.TabIndex = 31;
            this.cmpClickToCallLabel.Text = "Click to Call";
            this.cmpClickToCallLabel.Click += new System.EventHandler(this.label11_Click);
            // 
            // cmpClickToCallButton
            // 
            this.cmpClickToCallButton.Location = new System.Drawing.Point(695, 62);
            this.cmpClickToCallButton.Name = "cmpClickToCallButton";
            this.cmpClickToCallButton.Size = new System.Drawing.Size(75, 23);
            this.cmpClickToCallButton.TabIndex = 32;
            this.cmpClickToCallButton.Text = "Dial";
            this.cmpClickToCallButton.UseVisualStyleBackColor = true;
            this.cmpClickToCallButton.Click += new System.EventHandler(this.cmpClickToCallButton_Click);
            // 
            // IntellaQueueSampleApplicationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1025, 603);
            this.Controls.Add(this.cmpClickToCallButton);
            this.Controls.Add(this.cmpClickToCallLabel);
            this.Controls.Add(this.cmpClickToCallTextBox);
            this.Controls.Add(this.cmpApiTestButton);
            this.Controls.Add(this.cmpRecordingStopButton);
            this.Controls.Add(this.cmpRecordingStartButton);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.cmpAgentCallsListBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.cmpResultCodeTextBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.cmpCallLogIDTextBox);
            this.Controls.Add(this.cmpLoginButton);
            this.Controls.Add(this.cmpCurrentStatusTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cmpMessagesTextBox);
            this.Controls.Add(this.cmpCaseNumberTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmpDialedNumberTextBox);
            this.Controls.Add(this.cmpSimilateDialerPostBackButton);
            this.Controls.Add(this.cmpUnPauseButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmpPauseComboBox);
            this.Controls.Add(this.cmpPauseButton);
            this.Controls.Add(this.cmpLogoutButton);
            this.Controls.Add(this.cmpAgentNumberTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmpExtensionTextBox);
            this.Controls.Add(this.label1);
            this.Name = "IntellaQueueSampleApplicationForm";
            this.Text = "IntellaQueue Sample Application";
            this.Load += new System.EventHandler(this.IntellaQueueSampleApplicationForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox cmpExtensionTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox cmpAgentNumberTextBox;
        private System.Windows.Forms.Button cmpLogoutButton;
        private System.Windows.Forms.Button cmpPauseButton;
        private System.Windows.Forms.ComboBox cmpPauseComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button cmpUnPauseButton;
        private System.Windows.Forms.Button cmpSimilateDialerPostBackButton;
        private System.Windows.Forms.TextBox cmpDialedNumberTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox cmpCaseNumberTextBox;
        private System.Windows.Forms.TextBox cmpMessagesTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox cmpCurrentStatusTextBox;
        private System.Windows.Forms.Button cmpLoginButton;
        private System.Windows.Forms.TextBox cmpCallLogIDTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox cmpResultCodeTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ListBox cmpAgentCallsListBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button cmpRecordingStartButton;
        private System.Windows.Forms.Button cmpRecordingStopButton;
        private System.Windows.Forms.Button cmpApiTestButton;
        private System.Windows.Forms.TextBox cmpClickToCallTextBox;
        private System.Windows.Forms.Label cmpClickToCallLabel;
        private System.Windows.Forms.Button cmpClickToCallButton;
    }
}

