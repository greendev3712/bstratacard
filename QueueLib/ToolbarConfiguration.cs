using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using Lib;

namespace QueueLib
{
    public class ToolbarConfiguration
	{
        // temp
        private ToolbarServerConnection m_mainServerTSC;
        public Boolean m_multiSite = false;

        private string m_agentDevice;
        public SortedList<string, Hashtable> m_subscribedQueues; // Pointer to main m_subscribedQueues... direct editing
        public List<OrderedDictionary> m_managerStatusCodes;
        public Hashtable m_default_toolbar_colors = new Hashtable();

        private Boolean m_isManager;
        private Boolean m_displayCallerDetailNonManager;
        public Boolean m_showLocationField = true;
        public Boolean m_agentStatusCodesEnabled = false;

        public int m_updateConfigurationInterval = 300; // 5 minutes
		public int m_updateDisplayInterval       = 2;

		private Timer withEventsField_m_configUpdateTimer = new Timer();

		private Timer m_configUpdateTimer {
			get { return withEventsField_m_configUpdateTimer; }

			set	{
				if (withEventsField_m_configUpdateTimer != null) {
					withEventsField_m_configUpdateTimer.Tick -= ToolBarConfigUpdate_Tick;
				}

				withEventsField_m_configUpdateTimer = value;
				if (withEventsField_m_configUpdateTimer != null) {
					withEventsField_m_configUpdateTimer.Tick += ToolBarConfigUpdate_Tick;
				}
			}
		}

		private Timer m_displayUpdateTimer;

        //----------------------------------------------------------------------

        public ToolbarConfiguration() {
        }

        public ToolbarConfiguration(ToolbarServerConnection mainTSC) {
            this.m_mainServerTSC = mainTSC;
        }

        public void SetMainTSC(ToolbarServerConnection mainTSC) {
            this.m_mainServerTSC = mainTSC;
        }

		public void SetQueue(SortedList<string, Hashtable> newQueues)
		{
			m_subscribedQueues = newQueues; // Pointer
		}

		public void SetUpdateDisplayTimer(Timer timerControl)
		{
			m_displayUpdateTimer = timerControl;
		}

		private void ToolBarConfigUpdate_Tick(System.Object sender, System.EventArgs e)
		{
            UpdateToolbarConfiguration(m_mainServerTSC);
		}

		private void updateDisplayInterval()
		{
			if (m_updateDisplayInterval != 0 && m_displayUpdateTimer != null)
			{
				m_displayUpdateTimer.Interval = m_updateDisplayInterval * 1000;
			}
		}

		public bool UpdateToolbarConfiguration(ToolbarServerConnection tsc) {
            DbHelper db = tsc.m_db;

            if (!tsc.m_isMainServer) {
                // If we're not the main connection, just popularte per-queue settings (like colors)
                return getPerQueueConfigFromDbAndSetToSubscribedQueues(tsc);
            }

            QueryResultSetRecord result = db.DbSelectSingleRow("SELECT database_settings_poll_time_seconds, display_poll_time_seconds FROM queue.toolbar_config");

            try {
                if (tsc.m_isMainServer) { 
                    m_updateConfigurationInterval = Int32.Parse(result["database_settings_poll_time_seconds"]);
                    m_updateDisplayInterval       = Int32.Parse(result["display_poll_time_seconds"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not retrieve toolbar configuration", "Error");
                Debug.Print(ex.Message);
                Application.Exit();
            }

            // Additional config (use try because we might be connecting to an old version db)
            try {
                result = db.DbSelectSingleRow("SELECT show_location_field FROM queue.toolbar_config");
                m_showLocationField = result.ToBoolean("show_location_field");
            }
            catch (Exception ex) {
                Debug.Print(ex.Message);
                // This is okay, if the field doesn't exist in this version, the default is true
            }

            // Agent Status Codes (use try because we might be connecting to an old version db)
                
            QueryResultSet status_codes         = db.DbSelect("SELECT * FROM queue.v_agent_status_codes");
            List<OrderedDictionary> result_data = DbHelper.ConvertQueryResultSet_To_ListOfOrderedDictionary(status_codes);

            // var a = DbHelper.ConvertListOrderedDictionary_To_QueryResultSet(result_data);

            // TODO: data compare result_data and m_managerStatusCodes
            // TODO: if changed, need to trigger a manager toolbar agent grid right click menu rebuild (cmpAgentManagerRightClickContextMenu)

            m_managerStatusCodes = result_data;

			if (m_updateConfigurationInterval != 0) {
				m_configUpdateTimer.Interval = m_updateConfigurationInterval * 1000;

				if (!m_configUpdateTimer.Enabled) {
					m_configUpdateTimer.Start();
				}
			}

			updateDisplayInterval();

			return getPerQueueConfigFromDbAndSetToSubscribedQueues(tsc);
		}

		private bool getPerQueueConfigFromDbAndSetToSubscribedQueues(ToolbarServerConnection tsc)
		{
            DbHelper db = tsc.m_db;
			bool result = true;

            if (m_subscribedQueues == null) {
                return false;
            }

            QueryResultSet tb_config_perqueue;
            
            if (m_multiSite) {
                tb_config_perqueue = db.DbSelect(@"
                    SELECT
                        ?::text as server_name,
                        ?::text || '-' || queue_name as prefixed_queue_name,
                        c.* -- includes c.queue_name
                    FROM
                        queue.v_toolbar_config_perqueue c",
                 tsc.m_serverName,
                 tsc.m_serverName
                 );
            }
            else {
                tb_config_perqueue = db.DbSelect("SELECT * FROM queue.v_toolbar_config_perqueue");
            }

            /*KeyValuePair<string,List<OrderedDictionary>> */ 
            foreach (QueryResultSetRecord tb_config in tb_config_perqueue) {
                string queueName;

                if (m_multiSite) {
                    queueName = tb_config["prefixed_queue_name"];
                }
                else {
                    queueName = tb_config["queue_name"];
                }

                if (!m_subscribedQueues.ContainsKey(queueName)) {
                    continue;
                }

                m_subscribedQueues[queueName]["Threshold_warning1-Agent"] = Int32.Parse(tb_config["call_waiting_seconds_threshold_yellow"]);
                m_subscribedQueues[queueName]["Threshold_warning2-Agent"] = Int32.Parse(tb_config["call_waiting_seconds_threshold_orange"]);
                m_subscribedQueues[queueName]["Threshold_warning3-Agent"] = Int32.Parse(tb_config["call_waiting_seconds_threshold_red"]);

                m_subscribedQueues[queueName]["Threshold_warning1-Manager"] = Int32.Parse(tb_config["agent_talking_seconds_threshold_yellow"]);
                m_subscribedQueues[queueName]["Threshold_warning2-Manager"] = Int32.Parse(tb_config["agent_talking_seconds_threshold_orange"]);
                m_subscribedQueues[queueName]["Threshold_warning3-Manager"] = Int32.Parse(tb_config["agent_talking_seconds_threshold_red"]);

                m_subscribedQueues[queueName]["Hold_time_threshold_warning1-Manager"] = Int32.Parse(tb_config["hold_time_seconds_threshold_yellow"]);
                m_subscribedQueues[queueName]["Hold_time_threshold_warning2-Manager"] = Int32.Parse(tb_config["hold_time_seconds_threshold_orange"]);
                m_subscribedQueues[queueName]["Hold_time_threshold_warning3-Manager"] = Int32.Parse(tb_config["hold_time_seconds_threshold_red"]);
            }

			return result;
		}

        public string AgentDevice {
            get {
                return this.m_agentDevice;
            }
            set {
                this.m_agentDevice = value;
            }
        }

        public Boolean Manager {
            get {
                return this.m_isManager;
            }
            set {
                this.m_isManager = value;
            }
        }

        public Boolean DisplayCallerDetailNonManager {
            get {
                return this.m_displayCallerDetailNonManager;
            }
            set {
                this.m_displayCallerDetailNonManager = value;
            }
        }

        public void SetManager(bool m_isManager) {
            throw new NotImplementedException();
        }
    }
}