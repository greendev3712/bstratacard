using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using IntellaScreenRecord;
using System.IO;
using Lib;
using LibICP;

namespace QueueLib
{
	public partial class AgentLoginDialogForm : System.Windows.Forms.Form
	{
		public enum ValidationCode { Success, Fail, NeedPin, NeedNumber, BadExt };

        private string m_ServerInfo = ""; // Used in debug mode, to show the destination server in the title bar of the login box

		private ValidateCallback m_validateCallback;
		private SuccessCallback m_successCallback;
        public delegate AgentLoginDialogForm.ValidationCode ValidateCallback(string agentExtension, string agentNumber, string agentPin);
		public delegate void SuccessCallback();

		private int m_trysDefault = 3;

		private int m_trys = 0;
		private bool m_isLoggedIn;

		private void Form_Show()
		{
			// TEMP TEMP
			if (Debugger.IsAttached) {
				button1.Visible = true;
			}

			m_trys = m_trysDefault;
			m_isLoggedIn = false;

            if (Debugger.IsAttached) {
                if (m_ServerInfo != "") {
                    this.Text += " (" + m_ServerInfo + ")";
                }
            }

			if (agentExtensionTextBox.Text == "") {
				agentExtensionTextBox.Focus();
            }
			else
			{
                AgentLoginDialogForm.ValidationCode result = m_validateCallback.Invoke(agentExtensionTextBox.Text, agentNumberTextBox.Text, "");

                if (result == ValidationCode.Success) {
                    agentNumberTextBox.Focus();
                }
                else if (result == ValidationCode.NeedPin) {
                    agentPinTextBox.Visible = agentPinLabel.Visible = true;
                    agentPinTextBox.Focus();
                }
                else {
                    agentNumberTextBox.Clear();
                    agentNumberTextBox.Focus();
                }

			}

			this.TopMost = true;
		}

        public Panel GetMainPanel() {
            return this.cmpAgentLoginMainPanel;
        }

        public void setServerInfo(string serverInfo) {
            this.m_ServerInfo = serverInfo;
        }

		public void setTextBoxValues(string agentNum, string agentExtension)
		{
			agentNumberTextBox.Text = agentNum;
			agentExtensionTextBox.Text = agentExtension;
		}

		public void SetValidateCallback(ValidateCallback callback)
		{
			m_validateCallback = callback;
		}

		public void SetSuccessCallback(SuccessCallback callback)
		{
			m_successCallback = callback;
		}
		
		private void textBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (e.KeyChar == Strings.Chr(13)) // Enter key
				login();
		}

		private void login()
		{
			m_isLoggedIn = false;
            AgentLoginDialogForm.ValidationCode result = m_validateCallback.Invoke(agentExtensionTextBox.Text, agentNumberTextBox.Text, agentPinTextBox.Text);

			if (result == ValidationCode.BadExt) {
				MessageBox.Show("Bad Extension", "Error");
				agentExtensionTextBox.Focus();
			}
			else if (result == ValidationCode.Success)
			{
				m_isLoggedIn = true;
				DialogResult = System.Windows.Forms.DialogResult.OK;
				this.Close();
				m_successCallback.Invoke();
			}
			else if (result == ValidationCode.NeedPin)
			{
				agentPinTextBox.Visible = agentPinLabel.Visible = true;
				agentPinTextBox.Focus();
			}
			else if (result == ValidationCode.NeedNumber)
			{
				agentNumberTextBox.Focus();
			}
			else
			{
				if (agentPinTextBox.Visible)
				{
					agentPinTextBox.Text = "";
					agentPinTextBox.Focus();
				}
				else
				{
					agentNumberTextBox.Text = "";
					agentNumberTextBox.Focus();
				}

				if (--m_trys <= 0)
					this.Close();
			}
		}
		
		public AgentLoginDialogForm()
		{
			// required by the designer.
			InitializeComponent();

			// Add any initialization after the InitializeComponent() call.
		}

		private void PasswordDialogForm_Shown(object sender, EventArgs e)
		{
			Form_Show();
		}

		private void AgentLoginDialogForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
		{
			if (!ToolBarHelper.quickAgentLoginSetupCheck() || !m_isLoggedIn)
			{
				MessageBox.Show("Agent Login not setup correctly. Toolbar will now exit.", "Error");
				Application.Exit();
			}
		}

		private void loginButton_Click(object sender, EventArgs e)
		{
            login();
		}


		// TEMP TEMP
		private void button1_Click(object sender, EventArgs e) {
			IntellaScreenRecording m_screenRecord = new IntellaScreenRecording();

			string screen_recording_file_path = Path.GetTempPath() + "SCREEN.mp4";
			
            m_screenRecord.RecordingStart(screen_recording_file_path,
                // Callback for when recording is finished
                delegate (IntellaScreenRecordingResult result) {
                    Console.WriteLine("ScreenCapture -- End Screen Recording: " + screen_recording_file_path);
                }
            );
		}
	}
}

