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
	public partial class QueueAppConfigSettingsForm : System.Windows.Forms.Form
	{
		private ToolBarHelper.GenericCallbackSub m_successCallback;
		private bool m_successCallBackSet = false;
		private ToolBarHelper.GenericCallbackSub m_failureCallback;

		private bool m_failureCallBackSet = false;
		private string m_watchQueue = "";

		private DbHelper m_db;
		public void Form_Load()
		{
            lock (m_db)
            {
                m_db = ToolBarHelper.GetDbHelper();
                PopulateQueueNames();
            }
		}

		private void Form_Close()
		{
			if ((!string.IsNullOrEmpty(m_watchQueue)))
			{
				if (m_successCallBackSet == true)
				{
					m_successCallback.Invoke("");
				}
			}
			else
			{
				if (m_failureCallBackSet == true)
				{
					m_failureCallback.Invoke("");
				}
			}
		}

		public void SetSuccessCallback(ToolBarHelper.GenericCallbackSub callback)
		{
			m_successCallback = callback;
			m_successCallBackSet = true;
		}

		public void SetFailureCallback(ToolBarHelper.GenericCallbackSub callback)
		{
			m_failureCallback = callback;
			m_failureCallBackSet = true;
		}

		public object IsQueueConfiguredCorrectly()
		{
			if ((string.IsNullOrEmpty(m_watchQueue)))
			{
				return false;
			}

			return true;
		}

		private void PopulateQueueNames()
		{
			string watchQueue = Interaction.GetSetting("IntellaToolBar", "QueueConfig", "WatchQueue");

			QueryResultSet queue_names = m_db.DbSelect("SELECT queue_name from queue.v_queues");

			if (queue_names.Count == 0) {
                m_watchQueue = "";
				Debug.Print("QueueAppConfigSettingsForm.Form_Load() Querying queue.v_queues failed");
				MessageBox.Show("Error populating queues.", "DB Error");

				return;
			}

            foreach (QueryResultSetRecord queue_item in queue_names) {
				string queue_name = queue_item["queue_name"];

                ComboBox_QueueNames.Items.Add(queue_name);

                if (watchQueue == queue_name) {
                    this.ComboBox_QueueNames.SelectedItem = queue_name;
				}
            }
		}

        //public object ValidateQueue(string queueName)
        //{
        //    List<string> result2 = new List<string>();
        //    if (0 != m_db.getColumn(ref result2, "queue_name", "queue.v_queues", "queue_name", queueName))
        //    {
        //        m_watchQueue = "";
        //        Debug.Print("QueueAppConfigSettingsForm.Form_Load() Querying queue.v_queues failed");
        //        MessageBox.Show("Error getting queuenames from DB.", "Error");
        //        return false;
        //    }

        //    m_watchQueue = queueName;   // A bug? in a previous version meant this line of code 
        //                                // wasn't reached if queue was valid. Perhaps this behavior 
        //                                // was beneficial?

        //    return result2.Count > 0;
        //}

		private void Button_SaveAndConnect_Click(System.Object sender, System.EventArgs e)
		{
			if (string.IsNullOrEmpty(ComboBox_QueueNames.SelectedItem.ToString()))
			{
				MessageBox.Show("Please select a queue.", "Alert");
				return;
			}

			Interaction.SaveSetting("IntellaToolBar", "QueueConfig", "WatchQueue", ComboBox_QueueNames.SelectedItem.ToString());
			m_watchQueue = ComboBox_QueueNames.SelectedItem.ToString();
			this.Close();
		}

		public QueueAppConfigSettingsForm()
		{
			//FormClosed += Form_Close;
			//Shown += Form_Load;
			InitializeComponent();
			this.TopMost = true;
		}

		private void QueueAppConfigSettingsForm_Shown(object sender, EventArgs e)
		{
			Form_Load();
		}

		private void QueueAppConfigSettingsForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
		{
			Form_Close();
		}
	}
}