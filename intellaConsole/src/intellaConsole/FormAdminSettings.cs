using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QueueLib;
using Lib;

namespace intellaConsole {
    public partial class FormAdminSettings : Form {
        private bool m_configDatabaseSuccess = false;
        //private bool m_configQueueSuccess = false;
        private ToolBarHelper.GenericCallbackSub m_successCallback;
        private DbHelper m_db;
        private string m_registryParent;

        public FormAdminSettings(string registryParent) {
            this.m_registryParent = registryParent;

            InitializeComponent();
        }

        public void Form_Open() {
            m_db = new DbHelper(null);
            m_configDatabaseSuccess = ToolBarHelper.QuickDatabaseSetupCheck(ref m_db);
        }

		public void Form_Close()
		{
			if (IsApplicationConfiguredCorrectly())
			{
				m_successCallback.Invoke("");
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

		public void SetSuccessCallback(ToolBarHelper.GenericCallbackSub callback)
		{
			m_successCallback = callback;
		}

		private void Button_QueueSettings_Click(System.Object sender, System.EventArgs e)
		{
			if (!DatabaseSettingsForm.IsDatabaseConfiguredSuccessfully())
			{
				MessageBox.Show("Server settings are not complete.  Please fix server settings and then the queue settings will be available", "Error");
				return;
			}

//			QueueAppConfigSettingsForm queueSettingsForm = new QueueAppConfigSettingsForm();
//			queueSettingsForm.Show();
		}

		private void Button_ServerSettings_Click(System.Object sender, System.EventArgs e)
		{
			DatabaseSettingsForm dbSettingsForm = new DatabaseSettingsForm();

            dbSettingsForm.SetRegistryParent(this.m_registryParent);
			dbSettingsForm.SetSuccessCallback(DatabaseSettingsSuccessCallback);
			dbSettingsForm.SetFailureCallback(DatabaseSettingsFailureCallback);
			dbSettingsForm.Show();
		}

		private void DatabaseSettingsSuccessCallback(string message)
		{
			// Database connect successful, so we don't need to show any warnings when closing config
			m_configDatabaseSuccess = true;
		}

		private void DatabaseSettingsFailureCallback(string message)
		{
			// Database connect not successful... display warnings
			m_configDatabaseSuccess = false;
		}
        
        private void FormAdminSettings_Load(object sender, EventArgs e)
		{
			Form_Open();
		}

        private void FormAdminSettings_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{
			Form_Close();
		}
    }
}
