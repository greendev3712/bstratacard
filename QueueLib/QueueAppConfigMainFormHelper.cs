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
	public class QueueAppConfigMainFormHelper
	{
		private bool m_configDatabaseSuccess = false;
		private ToolBarHelper.GenericCallbackSub m_successCallback;
		private ToolBarHelper.GenericCallbackSub m_popupLoginCallback;

        private static QD.QE_ErrorCallbackFunction m_errorHandler = QD.GenericErrorCallbackFunction;

        private DbHelper m_db;

		public QueueAppConfigMainFormHelper(DbHelper db)
		{
            m_db = new DbHelper(handleError);
            m_db.SetErrorCallback(m_errorHandler);

            lock (m_db)
            {
                //m_db = db;
                DatabaseSettingsForm.SetDbHelper(m_db);
                m_configDatabaseSuccess = ToolBarHelper.QuickDatabaseSetupCheck(ref m_db);
            }
		}


		public void SetSuccessCallback(ToolBarHelper.GenericCallbackSub callback)
		{
			m_successCallback = callback;
		}

		public void SetPopupLoginCallback(ToolBarHelper.GenericCallbackSub callback)
		{
			m_popupLoginCallback = callback;
		}

        public void SetErrorCallback(QD.QE_ErrorCallbackFunction errorHandler) {
            m_errorHandler = errorHandler;
        }

        public void handleError(Exception ex, string errorMessage) {
            m_errorHandler.Invoke(QD.ERROR_LEVEL.FATAL, "ERROR", ex, errorMessage);
        }

        public void popupServerSettingsForm(ConnectionManager cm)
		{
			DatabaseSettingsForm dbSettingsForm = new DatabaseSettingsForm();
            dbSettingsForm.SetErrorCallback(m_errorHandler);
            dbSettingsForm.SetSuccessCallback(DatabaseSettingsSuccessCallback);
			dbSettingsForm.SetFailureCallback(DatabaseSettingsFailureCallback);
            // dbSettingsForm.setConnectionManager(cm);

            dbSettingsForm.Show();
		}

		private void DatabaseSettingsSuccessCallback(string message)
		{
			// Database connect successful, so we don't need to show any warnings when closing config
			m_configDatabaseSuccess = true;
		    m_popupLoginCallback.Invoke("");
		}

		private void DatabaseSettingsFailureCallback(string message)
		{
			// Database connect not successful... display warnings
			m_configDatabaseSuccess = false;
			MessageBox.Show("Server settings are not complete.  Application will not operate until corrected.", "Error");
		}
	}

}