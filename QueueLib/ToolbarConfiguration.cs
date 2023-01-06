using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using Lib;
using LibICP;

namespace QueueLib
{
    public class ToolbarConfiguration
	{
        // temp
        private ToolbarServerConnection m_mainServerTSC;
        public  Boolean                m_multiSite = false;

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

        // CONVERTED
		public bool UpdateToolbarConfiguration(ToolbarServerConnection tsc) {
            if (!tsc.m_isMainServer) {
                // If we're not the main connection, just popularte per-queue settings (like colors)
                return GetPerQueueConfig_AndSetToSubscribedQueues(tsc);
            }

            JsonHashResult config_result = tsc.m_iqc.GetConfig();

            if (config_result.Success) {
                Utils.ConfigurationSetting_SetInteger(ref m_updateConfigurationInterval, config_result.Data.GetString("database_settings_poll_time_seconds"));
                Utils.ConfigurationSetting_SetInteger(ref m_updateDisplayInterval,       config_result.Data.GetString("display_poll_time_seconds"));
                Utils.ConfigurationSetting_SetBoolean(ref m_showLocationField,           config_result.Data.GetString("show_location_field"));
            }
            else {
                // TODO: Add a logger to ToolbarConfiguration
                Debug.Print("Failed getting toolbar config: {0} -- {1}", config_result.Code, config_result.Reason);
            }

            JsonHashResult status_codes_result = tsc.m_iqc.GetAvailableStatusCodesAll();

            // For backwards compat
            QueryResultSet status_codes = status_codes_result.Data.ToQueryResultSet();
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

			return GetPerQueueConfig_AndSetToSubscribedQueues(tsc);
		}

		private bool GetPerQueueConfig_AndSetToSubscribedQueues(ToolbarServerConnection tsc)
		{
            if (m_subscribedQueues == null) {
                return false;
            }

            JsonHashResult config_per_queue_result = tsc.m_iqc.GetConfig_PerQueue();
            if (!config_per_queue_result.Success) {
                return false;
            }

            QueryResultSet tb_config_perqueue = config_per_queue_result.Data.ToQueryResultSet();

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

			return true;
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