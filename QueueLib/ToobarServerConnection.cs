using Lib;
using QueueLib;

using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Collections.Specialized;
using System.Diagnostics;

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
        public bool                          m_multiSite = false;
    
        public string                        m_serverName;
        public string                        m_serverLongname;
        public DateTime                      m_lastUpdated;
        public UInt64                        m_updateCount;
        public TimeSpan                      m_serverTimeOffset;  // Timespan difference between server time and our time (clocks are assumed to not be 100% in sync)

        public bool                          Cancel = false;      // Terminate the ToolBarServerConnection Data Update Thread if true
        
        public Thread m_operatingThread      { get; set; }

        QD.QD_LoggerFunction          m_logCallBack  = null;
        QD.QE_ErrorCallbackFunction   m_errorHandler = null;

        //public QueryResultSet m_dbMonitorOtherServers;
        //public QueryResultSet m_dbMonitorOtherQueues;
        //public Dictionary<string, QueryResultSet> m_monitorOtherQueues;

        // All our per-queue data follows the same structure.. We have a set of queue-specific records per queue-name
        public delegate JsonHashResult GetPerQueueDataFunction(string prefixQueueName);

        // Run this callback on the per_queue_data after we've grabbed it from the backend, and before converting it to a final Dictionary<string, List<OrderedDictionary>>
        public delegate void GetPerQueueDataMassagerFunction(Dictionary<string, QueryResultSet> per_queue_data);

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

        public void SetErrorCallback(QD.QE_ErrorCallbackFunction errorHandler) {
            m_errorHandler = errorHandler;
        }

        public void SetLogCallback(QD.QD_LoggerFunction logHandler) {
            m_logCallBack = logHandler;
        }

        public void Error(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] msgFormat) {
            m_errorHandler.Invoke(errorLevel, errorToken, ex, msg, msgFormat);
        }

        public void Log(string msg, params string[] msgFormat) {
            m_logCallBack.Invoke(msg, msgFormat);
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
            ToolbarLiveDataSegment result = new ToolbarLiveDataSegment(ToolbarLiveDataSegment.SegmentType.Caller);
            // result.SetSegmentData();
            return result;
        }

        // Return a list of status codes indexed per-queue
        private ToolbarLiveDataSegment GetAvailableStatusCodes(ToolbarServerConnection tsc) {
            ToolbarLiveDataSegment result = new ToolbarLiveDataSegment(ToolbarLiveDataSegment.SegmentType.Status);
            // result.SetSegmentData();
            return result;
        }

        private ToolbarLiveDataSegment GetLiveQueueData(ToolbarServerConnection tsc) {
            ToolbarLiveDataSegment result = new ToolbarLiveDataSegment(ToolbarLiveDataSegment.SegmentType.Queue);
            // result.SetSegmentData();
            return result;
        }

        // Generically do a request for any kind of Per-Queue data, post process it and return!

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getPerQueueData">What function do we call in order to get the specific type of per-queue data we're looking for</param>
        /// <param name="dataMassager">Post GetPerQueueDataFunction/getPerQueueData, optionally modify the data</param>
        /// <param name="haveData">If we already have the data, skip calling GetPerQueueDataFunction/getPerQueueData</param>
        /// <returns></returns>
        public Dictionary<string, List<OrderedDictionary>> GetLiveDataHelper(GetPerQueueDataFunction getPerQueueData, GetPerQueueDataMassagerFunction dataMassager = null, JsonHash haveData = null) {
            // To return to caller
            Dictionary<string, QueryResultSet> per_queue_data = new Dictionary<string, QueryResultSet>();

            // Generate an Error to handle!
            /*
            Random i = new Random();
            int r    = (int)(i.NextDouble() * 10);

            if (r > 8) {
                Hashtable foo = null;
                foo.Add("a", "b");
            }
            */

            string get_per_queue_data_method_name = getPerQueueData.Method.Name;

            string prefix_queue_name = null;
            string queue_name_field  = "queue_name";
            JsonHashResult per_queue_data_jh = new JsonHashResult();  // Web-API Return Result, with Arrays of Hashes
            QueryResultSet per_queue_data_qrs;                        // Rows of data, Basically Arrays of Hashes but using our DB-Style QueryResultSet for backwards compatability

            if (m_multiSite) {
                prefix_queue_name = this.m_serverName;
                queue_name_field  = "prefixed_queue_name";
            }

            try {
                if (haveData == null) {
                    per_queue_data_jh   = getPerQueueData.Invoke(prefixQueueName: prefix_queue_name);
                }
                else {
                    // If we already have the data, don't fetch it just process it
                    per_queue_data_jh   = new JsonHashResult();
                    per_queue_data_jh.Success = true;
                    per_queue_data_jh.Data    = haveData;
                }

                per_queue_data_qrs  = per_queue_data_jh.Data.ToQueryResultSet();
                per_queue_data      = per_queue_data_qrs.ConvertToDictionary_String_QueryResultSet(queue_name_field); // Build Dictionary using the queue_name_field as the lookup
            }
            catch (Exception ex) {
                Error(QD.ERROR_LEVEL.ERROR, "GetLiveDataHelper_Fetch_Fail", ex, "GetLiveDataHelper() Failed running {0}()", get_per_queue_data_method_name);
                if (Debugger.IsAttached) { throw; }
            }
            finally {
                if (!per_queue_data_jh.Success) {
                    Log("GetLiveDataHelper() Failed to fetch {0}(): [{1}] -- {2}", get_per_queue_data_method_name, per_queue_data_jh.Code, per_queue_data_jh.Reason);
                }
            }

            this.SubscribedQueuesFilter(per_queue_data);

            if (dataMassager != null) {
                dataMassager.Invoke(per_queue_data);
            }

            // For backwards compatability we use the old-style Dictionary of OrderedDictionary
            // This is the legacy version of a QueryResultSet
            Dictionary<string, List<OrderedDictionary>> dslo_result = Utils.ConvertTo_DictionaryString_ListOrderedDictionary(per_queue_data);
            return dslo_result;
        }

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

        public void SetUpdating(bool value) { 
            this.m_dataIsUpdating = value;
        }

        public bool IsUpdating() { 
            return this.m_dataIsUpdating;
        }
    }
}