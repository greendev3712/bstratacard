using Lib;
using QueueLib;

using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

namespace QueueLib {
    public class ToolbarServerConnection
    {
        public ToolbarConfiguration          m_toolbarConfig;

        private bool                         m_dataIsUpdating;
        public DbHelper                      m_db;
        public SimpleDbConnectionParameters  m_dbConnectionParameters;
        public LibICP.IntellaQueueControl    m_iqc;
        public SortedList<string, Hashtable> m_subscribedQueues;
        public Hashtable                     m_subscribedQueuesHidden;
        public Hashtable                     m_liveDatas;
        public Boolean                       m_isMainServer;
    
        public string                        m_serverName;
        public string                        m_serverLongname;
        public DateTime                      m_lastUpdated;
        public UInt64                        m_updateCount;
        public TimeSpan                      m_serverTimeOffset;  // Timespan difference between server time and our time (clocks are assumed to not be 100% in sync)

        public bool                          Cancel = false;      // Terminate the ToolBarServerConnection Data Update Thread if true
        
        public Thread m_operatingThread      { get; set; }

        //public QueryResultSet m_dbMonitorOtherServers;
        //public QueryResultSet m_dbMonitorOtherQueues;
        //public Dictionary<string, QueryResultSet> m_monitorOtherQueues;

        public ToolbarServerConnection() {
            m_updateCount = 0;
        }

        public ToolbarServerConnection(string serverName, string serverLongname, ToolbarConfiguration toolbarConfig) {
            m_toolbarConfig          = toolbarConfig;
            m_lastUpdated            = DateTime.Now;
            m_serverName             = serverName;
            m_serverLongname         = serverLongname;
            m_subscribedQueues       = new SortedList<string, Hashtable>();
            m_subscribedQueuesHidden = new Hashtable();
            m_isMainServer           = false;
            m_iqc                    = null;
        }

        public void InitConnection(SimpleDbConnectionParameters connectionParameters) {
            m_dbConnectionParameters = connectionParameters;

            m_db.initConnection(m_dbConnectionParameters);
            m_db.SetPrepared(true);
        }

        public bool Connect() {
            m_updateCount = 0;

            return ToolBarHelper.DoConnection(m_db);
        }

        // ---------------------------------------------------------------------
        // DB Querys

        public ToolbarLiveDataSegment GetLiveAgentData(ToolbarServerConnection tsc) {
            SortedList<string, Hashtable> subscribed_queues = tsc.m_subscribedQueues;
            Dictionary<string, QueryResultSet> dict_result_set;
            ToolbarLiveDataSegment result = new ToolbarLiveDataSegment(ToolbarLiveDataSegment.SegmentType.Agent);

            // TODO: need a helper to make a SELECT ... WHERE X IN (...) list so that we only query queues we're interested in
            Hashtable h = new Hashtable() {
                {"queue_name", new List<string>(subscribed_queues.Keys)}
            };
            List<string> queue_names = (List<string>) h["queue_name"];

            if (queue_names.Count == 0) {
               return result;
            }

            JsonHashResult agent_status_per_queue;
            QueryResultSet agent_status_per_queue_qrs;
            string agent_status_hash_key;
            string server_name_prefix = null;

            if (m_toolbarConfig.m_multiSite) {
                server_name_prefix    = tsc.m_serverName;
                agent_status_hash_key = "prefixed_queue_name";
            }
            else {
                agent_status_hash_key = "queue_name";
            }

            // Basically a 'Hashify'.. dict_result_set[queue_name] will = the data for that queue
            agent_status_per_queue     = tsc.m_iqc.GetAllAgentStatusPerQueue(prefixQueueName: server_name_prefix);
            agent_status_per_queue_qrs = agent_status_per_queue.Data.ToQueryResultSet();
            dict_result_set            = agent_status_per_queue_qrs.ConvertToDictionary_String_QueryResultSet(agent_status_hash_key);

            result.SetSegmentData(dict_result_set);

            return result;
        }

        private ToolbarLiveDataSegment GetLiveCallerData(ToolbarServerConnection tsc) {
            SortedList<string, Hashtable> subscribed_queues = tsc.m_subscribedQueues;
            Dictionary<string, QueryResultSet> dict_result_set;
            ToolbarLiveDataSegment result = new ToolbarLiveDataSegment(ToolbarLiveDataSegment.SegmentType.Caller);

            // TODO: need a helper to make a SELECT ... WHERE X IN (...) list so that we only query queues we're interested in
            Hashtable h = new Hashtable() {
                {"queue_name", new List<string>(subscribed_queues.Keys)}
            };
            List<string> queue_names = (List<string>)h["queue_name"];

            if (queue_names.Count == 0) {
                return result;
            }

            if (m_toolbarConfig.m_multiSite) {
                dict_result_set = tsc.m_db.DbSelectIntoDictQueryResultSet("prefixed_queue_name", @"
                    SELECT
                        '{0}'::text as server_name,
                        '{0}' || '-' || queue_name as prefixed_queue_name,
                        c.* -- includes c.queue_name
                    FROM
                        live_queue.v_toolbar_callers c", tsc.m_serverName);
            }
            else {
                dict_result_set = tsc.m_db.DbSelectIntoDictQueryResultSet("queue_name", @"
                    SELECT
                        c.*
                    FROM
                        live_queue.v_toolbar_callers c");
            }

            // If display detail is off for non managers, and we are a non-manager.. hide the caller details!
            if (!m_toolbarConfig.Manager && !m_toolbarConfig.DisplayCallerDetailNonManager) {
                foreach (string queue_name in dict_result_set.Keys) {
                    int caller_num = 1;

                    foreach (QueryResultSetRecord row in dict_result_set[queue_name]) {
                        // Each caller is a row
                        row["callerid_name"] = "Caller #" + caller_num;
                        row["callerid_num"]  = caller_num.ToString();
                        caller_num++;
                    }
                }
            }

            result.SetSegmentData(dict_result_set);
            return result;
        }

        // Return a list of status codes indexed per-queue
        private ToolbarLiveDataSegment GetAvailableStatusCodes(ToolbarServerConnection tsc) {
            SortedList<string, Hashtable> subscribed_queues = tsc.m_subscribedQueues;
            Dictionary<string, QueryResultSet> dict_result_set;
            ToolbarLiveDataSegment result = new ToolbarLiveDataSegment(ToolbarLiveDataSegment.SegmentType.Status);

            // TODO: need a helper to make a SELECT ... WHERE X IN (...) list so that we only query queues we're interested in
            Hashtable h = new Hashtable() {
                {"queue_name", new List<string>(subscribed_queues.Keys)}
            };
            List<string> queue_names = (List<string>)h["queue_name"];

            if (queue_names.Count == 0) {
                return result;
            }

            if (m_toolbarConfig.m_multiSite) {
                dict_result_set = tsc.m_db.DbSelectIntoDictQueryResultSet("prefixed_queue_name", @"
                    SELECT
                        '{0}'::text as server_name,
                        '{0}' || '-' || queue_name as prefixed_queue_name,
                        s.* -- includes c.queue_name
                    FROM
                        live_queue.v_agent_status_codes_self s
                    WHERE
                        s.agent_device = '{1}'", tsc.m_serverName, m_toolbarConfig.AgentDevice);
            }
            else {
                dict_result_set = tsc.m_db.DbSelectIntoDictQueryResultSet("queue_name", @"
                    SELECT
                        s.*
                    FROM
                        live_queue.v_agent_status_codes_self s
                    WHERE
                        s.agent_device = '{1}'", tsc.m_serverName, m_toolbarConfig.AgentDevice);
            }

            result.SetSegmentData(dict_result_set);
            return result;
        }

        private ToolbarLiveDataSegment GetLiveQueueData(ToolbarServerConnection tsc) {
            SortedList<string, Hashtable> subscribed_queues = tsc.m_subscribedQueues;
            Dictionary<string, QueryResultSet> dict_result_set;
            ToolbarLiveDataSegment result = new ToolbarLiveDataSegment(ToolbarLiveDataSegment.SegmentType.Queue);

            // TODO: need a helper to make a SELECT ... WHERE X IN (...) list so that we only query queues we're interested in
            Hashtable h = new Hashtable() {
                {"queue_name", new List<string>(subscribed_queues.Keys)}
            };
            List<string> queue_names = (List<string>)h["queue_name"];

            if (queue_names.Count == 0) {
                return result;
            }

            if (m_toolbarConfig.m_multiSite) {
                dict_result_set = tsc.m_db.DbSelectIntoDictQueryResultSet("prefixed_queue_name", @"
                    SELECT
                        '{0}'::text as server_name,
                        '{0}' || '-' || queue_name as prefixed_queue_name,
                        q.* -- includes c.queue_name
                    FROM
                        live_queue.v_toolbar_queues q", tsc.m_serverName);
            }
            else {
                dict_result_set = tsc.m_db.DbSelectIntoDictQueryResultSet("queue_name", @"
                    SELECT
                        q.*
                    FROM
                        live_queue.v_toolbar_queues q", tsc.m_serverName);
            }

            result.SetSegmentData(dict_result_set);
            return result;
        }

        public void SetUpdating(bool value) { 
            this.m_dataIsUpdating = value;
        }

        public bool IsUpdating() { 
            return this.m_dataIsUpdating;
        }
    }
}