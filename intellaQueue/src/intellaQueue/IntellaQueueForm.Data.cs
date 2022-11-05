using Lib;
using LibICP;
using QueueLib;
using IntellaScreenRecord;

// using log4net;

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Net.NetworkInformation;
using System.Net;

namespace intellaQueue
{
    public partial class IntellaQueueForm
    {
        //----------------------------------------------------------------------
        // Utility - General
        //----------------------------------------------------------------------

        public string GetTenantName () {
            return IntellaQueueForm.m_tenantName;
        }

        public string GetAgentID () {
            return IntellaQueueForm.m_agentID;
        }

        public string GetAgentNumber () {
            return IntellaQueueForm.m_agentNum;
        }

        public string GetAgentDevice () {
            return IntellaQueueForm.m_agentDevice;
        }


        private string Config_Get_DB_Host() {
            return ToolBarHelper.Registry_GetToolbarConfigItem("IntellaToolBar", "Config", "DB_Host");
        }

        private string Config_Get_DB_Port() {
          return ToolBarHelper.Registry_GetToolbarConfigItem("IntellaToolBar", "Config", "DB_Port");
        }

        //----------------------------------------------------------------------
        // Need Label 
        //----------------------------------------------------------------------

        private void getQueueNameAndType(ref string queueName, ref bool isManagerSection, int index) {
            string[] queueNames;

            if (index >= 0) {
                queueNames = new string[m_subscribedQueues.Count];
                m_subscribedQueues.Keys.CopyTo(queueNames, 0);
                if (m_enableTeamView) {
                    queueName = queueNames[index / 2];
                    isManagerSection = index % 2 == 0;
                }
                else {
                    queueName = queueNames[index];
                    isManagerSection = false;
                }
            }
        }

        private void InitDropGridsAndQueueInfo() {
            if ((m_subscribedQueues == null) || (m_subscribedQueues.Count == 0)) {
                return;
            }

            foreach (string queueName in m_subscribedQueues.Keys) {
                if (!m_subscribedQueues.ContainsKey(queueName))
                    m_subscribedQueues.Add(queueName, new Hashtable());

                if (!m_subscribedQueues[queueName].ContainsKey("AgentGrid")) {
                    m_subscribedQueues[queueName].Add("AgentGrid", new TweakedDataGridView());
                    ((DataGridView) m_subscribedQueues[queueName]["AgentGrid"]).Visible = false;
                    // uncomment to allow user window moving by drag clicking on agent grid
                    //((DataGridView)m_subscribedQueues[queueName]["AgentGrid"]).MouseDown += intellaQueue_MouseDown;
                    //((DataGridView)m_subscribedQueues[queueName]["AgentGrid"]).MouseUp += intellaQueue_MouseUp;
                    //((DataGridView)m_subscribedQueues[queueName]["AgentGrid"]).MouseMove += intellaQueue_MouseMove;			
                }
                if (m_enableTeamView && !m_subscribedQueues[queueName].ContainsKey("TeamGrid")) {
                    m_subscribedQueues[queueName].Add("TeamGrid", new TweakedDataGridView());
                    ((DataGridView) m_subscribedQueues[queueName]["TeamGrid"]).Visible = false;
                    // uncomment to allow user window moving by drag clicking on manager grid
                    //((DataGridView)m_subscribedQueues[queueName]["TeamGrid"]).MouseDown += intellaQueue_MouseDown;
                    //((DataGridView)m_subscribedQueues[queueName]["TeamGrid"]).MouseUp += intellaQueue_MouseUp;
                    //((DataGridView)m_subscribedQueues[queueName]["TeamGrid"]).MouseMove += intellaQueue_MouseMove;
                }
            }
        }


        // ---------------------------------------------------------------------
        // Utility
        // ---------------------------------------------------------------------

        private void SubscribedQueuesFilter(Dictionary<string, QueryResultSet> per_queue_data) {
            if (m_subscribedQueues.Keys.Count == 0) { 
                return;
            }

            // Because we cannot modify the collection when iterating on it directly
            List<string> key_names = new List<string>();
            foreach (string key_entry in per_queue_data.Keys) { key_names.Add(key_entry); }


            foreach (string subscribed_queue_name in key_names) {
                if (!this.m_subscribedQueues.ContainsKey(subscribed_queue_name)) {
                    per_queue_data.Remove(subscribed_queue_name);
                }
            }
        }

        // ---------------------------------------------------------------------
        // DB Queries
        // ---------------------------------------------------------------------

        private Dictionary<string, List<OrderedDictionary>> getLiveAgentData(ToolbarServerConnection tsc) {
            SortedList<string, Hashtable> subscribed_queues = tsc.m_subscribedQueues;

            // Per-Queue list of caller data
            Hashtable hash_result_set = new Hashtable(); // result[queue_name] = QueryResultSet

            try {
                if (m_multiSite) {
                    hash_result_set = tsc.m_db.DbSelectIntoHashQueryResultSet("prefixed_queue_name", @"
                        SELECT
                           ?::text as server_name,
                           ?::text || '-' || queue_name as prefixed_queue_name,
                           a.* -- includes c.queue_name
                        FROM
                         live_queue.v_toolbar_agents a
                       ",
                       tsc.m_serverName,
                       tsc.m_serverName
                    );
                }
                else {
                    if (tsc.m_subscribedQueues.Count != 0) {
                        hash_result_set = tsc.m_db.DbSelectIntoHashQueryResultSet(
                          "queue_name",
                          "SELECT * FROM live_queue.v_toolbar_agents WHERE queue_name = any(string_to_array(?, ','))",
                          string.Join(",", m_subscribedQueues.Keys.ToArray())
                        );
                    }
                }

                handleDatabaseSuccess();
            }
            catch (Exception ex) {
                handleError("!!! getLiveAgentData() Error getting live data: " + ex.Message);
                throw ex;
            }

            // TODO: Convert all data functions to use QueryResultSetRecord
            Dictionary<string, List<OrderedDictionary>> dslo_result = DbHelper.ConvertHashQueryResultSet_To_DictionaryOfOrderedDictionary(hash_result_set);

            return dslo_result;
        }

        private Dictionary<string, List<OrderedDictionary>> getLiveCallerData(ToolbarServerConnection tsc) {
            SortedList<string, Hashtable> subscribed_queues = tsc.m_subscribedQueues;

            // Per-Queue list of caller data
            Dictionary<string, QueryResultSet> per_queue_caller_data = new Dictionary<string, QueryResultSet>();

            try {
                if (m_multiSite) {
                    JsonHashResult queue_callers     = tsc.m_iqc.GetAllQueueCallers(prefixQueueName: tsc.m_serverName);
                    QueryResultSet queue_callers_qrs = queue_callers.Data.ToQueryResultSet();
                    per_queue_caller_data            = queue_callers_qrs.ConvertToDictionary_String_QueryResultSet("prefixed_queue_name");
                }
                else {
                    JsonHashResult queue_callers     = tsc.m_iqc.GetAllQueueCallers();
                    QueryResultSet queue_callers_qrs = queue_callers.Data.ToQueryResultSet();
                    per_queue_caller_data            = queue_callers_qrs.ConvertToDictionary_String_QueryResultSet("queue_name");
                }

                handleDatabaseSuccess();
            }
            catch (Exception ex) {
                handleError(ex, "getLiveCallerData() Failed");
                if (Debugger.IsAttached) { throw; }
            }

            this.SubscribedQueuesFilter(per_queue_caller_data);

            // If display detail is off for non managers, and we are a non-manager.. hide the caller details!
            if (!m_isManager && !m_displayCallerDetailNonManager) {
                foreach (string queue_name in per_queue_caller_data.Keys) {
                    int caller_num = 1;

                    QueryResultSet per_queue_data = per_queue_caller_data[queue_name];

                    foreach (QueryResultSetRecord row in per_queue_data) {
                        // Each caller is a row
                        row["callerid_name"] = "Caller #" + caller_num;
                        row["callerid_num"]  = caller_num.ToString();
                        caller_num++;
                    }
                }
            }

            // TODO: Convert all data functions to use QueryResultSetRecord
            // Dictionary<string, List<OrderedDictionary>> dslo_result = DbHelper.ConvertHashQueryResultSet_To_DictionaryOfOrderedDictionary(hash_result_set);

            Dictionary<string, List<OrderedDictionary>> dslo_result = Utils.ConvertTo_DictionaryString_ListOrderedDictionary(per_queue_caller_data);

            return dslo_result;
        }

        // Return a list of status codes indexed per-queue
        private Dictionary<string, List<OrderedDictionary>> getAvailableStatusCodes(ToolbarServerConnection tsc) {
            Dictionary<string, List<OrderedDictionary>> result = new Dictionary<string,List<OrderedDictionary>>();

            try {
                string index_by = (this.m_multiSite ? "prefixed_queue_name" : "queue_name");

                Hashtable hash_result_set = tsc.m_db.DbSelectIntoHashQueryResultSet(index_by, @"
                    SELECT
                        ?::text as server_name,
                        ? || '-' || queue_name as prefixed_queue_name,
                        s.* -- includes c.queue_name
                    FROM
                        live_queue.v_agent_status_codes_self s
                    WHERE
                        s.agent_device = ?
                    ",
                    tsc.m_serverName,
                    tsc.m_serverName,
                    m_agentDevice
                );

                result = DbHelper.ConvertHashQueryResultSet_To_DictionaryOfOrderedDictionary(hash_result_set);
                handleDatabaseSuccess();
            }
            catch (Exception ex) {
                MQD("!!! getAvailableStatusCodes() Error getting available status codes: " + ex.Message);
                Error_DatabaseMainConnection_Failed();
                // if (Debugger.IsAttached) { throw; }
            }

            return result;
        }

        private Dictionary<string, List<OrderedDictionary>> getLiveQueueData(ToolbarServerConnection tsc) {
            SortedList<string, Hashtable> subscribed_queues = tsc.m_subscribedQueues;

            // Per-Queue list of caller data
            Hashtable hash_result_set = new Hashtable(); // result[queue_name] = QueryResultSet

            try {
                if (m_multiSite) {
                    // Hashtable of QueryResultSet
                    hash_result_set = tsc.m_db.DbSelectIntoHashQueryResultSet("prefixed_queue_name", @"
                       SELECT
                          ?::text as server_name,
                          ?::text || '-' || queue_name as prefixed_queue_name,
                          agents_idle as agents_idle_real,
                          version(),
                          q.* -- includes c.queue_name
                       FROM
                         live_queue.v_toolbar_queues q"
                      ,
                      tsc.m_serverName,
                      tsc.m_serverName
                    );
                }
                else {
                    if (subscribed_queues.Count != 0) {
                        hash_result_set = tsc.m_db.DbSelectIntoHashQueryResultSet(
                          "queue_name",
                          "SELECT * FROM live_queue.v_toolbar_queues WHERE queue_name = any(string_to_array(?, ','))",
                          string.Join(",", m_subscribedQueues.Keys.ToArray())
                        );
                    }
                }

                // Generate an Error to handle!
                /*
                Random i = new Random();
                int r = (int)(i.NextDouble() * 10);

                if (r > 8) {
                    Hashtable foo = null;
                    foo.Add("a", "b");
                }
                */

                handleDatabaseSuccess();
            }
            catch (Exception ex) {
                MQD("!!! getLiveQueueData() Error getting live data " + ex.Message);
                Error_DatabaseMainConnection_Failed();
            }

            // TODO: Convert all data functions to use QueryResultSetRecord
            Dictionary<string, List<OrderedDictionary>> dslo_result = DbHelper.ConvertHashQueryResultSet_To_DictionaryOfOrderedDictionary(hash_result_set);

            return dslo_result;
        }

        private QueryResultSet getCurrentCallsForAgent(ToolbarServerConnection tsc) {
            QueryResultSet result = new QueryResultSet();

            if (m_currentCallEnabled == false) {
                return result;
            }

            // We're only taking calls on the main server we're connected to
            if (!tsc.m_isMainServer) {
                return result;
            }


            // FIXME FIXME FIXME we need a general callcenter_live.agents_status that merges callqueue and dialer status
            // and then toolbar agents would need reflect dialer status and then we can query callcenter_live.v_toolbar_agents

            try
            {
                result = tsc.m_db.DbSelect(@"
                    SELECT
                        c.channel,
                        a.call_state,
                        c.call_log_id,
                        c.call_segment_id,
                        c.case_number,
                        c.callerid_num,
                        c.callerid_name,
                        q.queue_longname
                    FROM
                        live_queue.v_toolbar_agents a
                        JOIN live_queue.v_callers   c ON (a.caller_call_log_id::text = c.call_log_id::text) -- FIXME... ideally we should get everything from v_toolbar_agents but it doesn't have segment_id and queue_longname
                        JOIN queue.queues           q ON (c.queue_id = q.queue_id)
                    WHERE
                      a.agent_device = ?
                      AND picked_up_when IS NOT NULL -- Only find calls to this agent that are ANSWERED
                ", m_agentDevice);

                if (result.Count == 0) {
                    // No current dialer calls... dialer?

                    if (this.m_mainServerHasDialer) {
                        result = tsc.m_db.DbSelect(@"
                            SELECT
                                c.from_channel as channel,
                                c.call_status as call_state,
                                c.call_log_id,
                                NULL as call_segment_id, -- FIXME populate
                                --c.case_number,
                                c.to_callerid_num as callerid_num,
                                CASE
                                  WHEN c.to_callerid_name IS NOT NULL THEN c.to_callerid_name
                                  ELSE 'DIALER'::text  -- FIXME, why don't we have name
                                END as callerid_name,
                                cmp.campaign_longname as queue_longname
                            FROM
                                live_dialer.dialers     d
                                JOIN live_core.v_calls  c ON (c.call_log_id = d.call_log_id)
                                JOIN dialer.campaigns cmp ON (d.campaign_id = cmp.campaign_id)
                            WHERE
                              c.from_device = ?
                              AND c.call_pickup_time IS NOT NULL -- Only find calls to this agent that are ANSWERED
                         ", m_agentDevice);
                    }
                }

                handleDatabaseSuccess();
            }
            catch (Exception ex)
            {
                MQD("!!! getCurrentCallsForAgent() Error getting live data " + ex.Message);
                Error_DatabaseMainConnection_Failed();
            }

            return result;
        }

        // ---------------------------------------------------------------------
    }
}
