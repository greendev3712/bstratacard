// Old Config manager

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using Lib;

namespace QueueLib
{
	public partial class QueueAppConfigMainForm : System.Windows.Forms.Form
	{
		public delegate void ApplicationExitCallback();

		private bool m_configDatabaseSuccess = false;
		// private bool m_configQueueSuccess = false;

		private ToolBarHelper.GenericCallbackSub m_successCallback;
        private static QD.QE_ErrorCallbackFunction m_errorHandler = QD.GenericErrorCallbackFunction;

		private ApplicationExitCallback m_applicationExitCallback = null;
        private DbHelper m_db;
		private bool m_invoke_success = true;

		public void Form_Open()
		{
            try {
                m_db = new DbHelper();
                m_configDatabaseSuccess = ToolBarHelper.QuickDatabaseSetupCheck(ref m_db);
            }
            catch (Exception e) {
                MessageBox.Show("DB Conection Failed: " + e.ToString());
            }
		}

		public void Form_Close()
		{
			if (IsApplicationConfiguredCorrectly())
			{
				if (m_invoke_success) {
					m_successCallback.Invoke("");
				}
			}
		}

		private void Button_Close_Click(System.Object sender, System.EventArgs e)
		{
			this.Close();
		}

		public bool IsApplicationConfiguredCorrectly()
		{
			if (!m_configDatabaseSuccess)
			{
				MessageBox.Show("Server settings are not complete.  Application will not operate until corrected.", "Error");
				return false;
			}

			return true;
		}

        public void SetErrorCallback(QD.QE_ErrorCallbackFunction errorHandler) {
            m_errorHandler = errorHandler;
        }

        public void SetSuccessCallback(ToolBarHelper.GenericCallbackSub callback)
		{
			m_successCallback = callback;
		}

		public void SetApplicationExitCallback(QueueAppConfigMainForm.ApplicationExitCallback applicationExitCallback) {
			this.m_applicationExitCallback = applicationExitCallback;
		}

		private void Button_QueueSettings_Click(System.Object sender, System.EventArgs e)
		{
			if (!DatabaseSettingsForm.IsDatabaseConfiguredSuccessfully())
			{
				MessageBox.Show("Server settings are not complete.  Please fix server settings and then the queue settings will be available", "Error");
				return;
			}
			QueueAppConfigSettingsForm queueSettingsForm = new QueueAppConfigSettingsForm();
			queueSettingsForm.Show();
		}

		private void Button_ServerSettings_Click(System.Object sender, System.EventArgs e)
		{
			DatabaseSettingsForm dbSettingsForm = new DatabaseSettingsForm();
            dbSettingsForm.SetErrorCallback(m_errorHandler);
            dbSettingsForm.SetSuccessCallback(DatabaseSettingsSuccessCallback);
			dbSettingsForm.SetFailureCallback(DatabaseSettingsFailureCallback);
			dbSettingsForm.Show();

			// Now we're not always on top.. db settings is showing
			this.TopMost = false;
		}

		private void DatabaseSettingsSuccessCallback(string message)
		{
			// Database connect successful, so we don't need to show any warnings when closing config
			m_configDatabaseSuccess = true;

			// Back to always on top
			this.TopMost = true;
		}

		private void DatabaseSettingsFailureCallback(string message)
		{
			// Database connect not successful... display warnings
			m_configDatabaseSuccess = false;

			// Back to always on top
			this.TopMost = true;
		}

		public QueueAppConfigMainForm()
		{
			// FormClosing += Form_Close;
			// Load += Form_Open;
			InitializeComponent();
			this.TopMost = true;
		}

		private void QueueAppConfigMainForm_Load(object sender, EventArgs e)
		{
			Form_Open();
		}

		private void QueueAppConfigMainForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{
			Form_Close();
		}

		private void cmpExitApplicationButton_Click(object sender, EventArgs e) {
			this.m_invoke_success = false;

			if (m_applicationExitCallback != null) {
				m_applicationExitCallback.Invoke();
			}
		}
	}

}