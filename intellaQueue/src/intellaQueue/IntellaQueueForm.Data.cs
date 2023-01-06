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
        // Live Data Updating
        // ---------------------------------------------------------------------

        private void SetupUpdateThreads() {
            // Handle Updating data from all servers

            foreach (KeyValuePair<string, ToolbarServerConnection> server_item in m_toolbarServers) {
                string server_name = server_item.Key;   // ex: passaic
                ToolbarServerConnection tsc = server_item.Value;
                DbHelper db = tsc.m_db;

                if (!m_multiSite) {
                    if (!tsc.m_isMainServer) {
                        continue;
                    }
                }

                var db_refresh_worker = new BackgroundWorker();
                db_refresh_worker.DoWork             += new DoWorkEventHandler(dbRefreshWorker_DoWork);
                db_refresh_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(dbRefreshWorker_RunWorkerCompleted);
                db_refresh_worker.ProgressChanged    += new ProgressChangedEventHandler(dbRefreshWorker_ProgressChanged);

                try {
                    db_refresh_worker.RunWorkerAsync(tsc);
                }
                catch (Exception ex) {
                    if (Debugger.IsAttached) { throw; }
                    handleError(ex, "Threading error: creating new thread worker.");
                }

                MQD("Spawned DoWork Thread For Server: [{0}]", server_name);
            }
        }

        public void StopRefreshTimer() {
            statusLight.BackgroundImage = queueResources.status_error;

            m_updateDisplayTimer.Stop();
            Debug.Print("Refresh Timer Stopped");
        }

        public void StartRefreshTimer() {
            Debug.Print("Starting RefreshTimer!");

            m_db_active = true;
            m_updateDisplayTimer.Start();
        }

        private void UpdateQueueDisplayAndCatchExceptions() {
            try {
                UpdateQueueDisplay();
            }
            catch (DatabaseException ex) {
                handleError("UpdateDisplayTimer_Tick caught database exception: " + ex.ToString());
                if (Debugger.IsAttached) { throw; }

                Error_DatabaseMainConnection_Failed();
            }
        }

        // ---------------------------------------------------------------------
        // Live Data
        // ---------------------------------------------------------------------

        // CONVERTED
        /// <summary>
        /// Per-Queue Agent Data
        /// </summary>
        /// <param name="tsc"></param>
        /// <returns></returns>
        private Dictionary<string, List<OrderedDictionary>> getLiveAgentData(ToolbarServerConnection tsc, JsonHash haveData = null) {
            return tsc.GetLiveDataHelper(getPerQueueData: tsc.m_iqc.GetAllQueueAgents, haveData: haveData);
        }

        // CONVERTED
        private Dictionary<string, List<OrderedDictionary>> getLiveCallerData(ToolbarServerConnection tsc, JsonHash haveData = null) {
            ToolbarServerConnection.GetPerQueueDataMassagerFunction massager = null;

            // If display detail is off for non managers, and we are a non-manager.. hide the caller details!
            if (!m_isManager && !m_displayCallerDetailNonManager) {
                massager = delegate (Dictionary<string, QueryResultSet> per_queue_live_caller_data) {
                    foreach (string queue_name in per_queue_live_caller_data.Keys) {
                        int caller_num = 1;

                        QueryResultSet per_queue_data = per_queue_live_caller_data[queue_name];

                        foreach (QueryResultSetRecord row in per_queue_data) {
                            // Each caller is a row
                            row["callerid_name"] = "Caller #" + caller_num;
                            row["callerid_num"]  = caller_num.ToString();
                            caller_num++;
                        }
                    }
                };
            }

            return tsc.GetLiveDataHelper(getPerQueueData: tsc.m_iqc.GetAllQueueCallers, dataMassager: massager, haveData: haveData);
        }

        // CONVERTED
        // Return a list of status codes indexed per-queue
        private Dictionary<string, List<OrderedDictionary>> getAvailableStatusCodes(ToolbarServerConnection tsc, JsonHash haveData = null) {
            return tsc.GetLiveDataHelper(getPerQueueData: tsc.m_iqc.GetAvailableStatusCodesSelf, haveData: haveData);
        }

        // CONVERTED
        private Dictionary<string, List<OrderedDictionary>> getLiveQueueData(ToolbarServerConnection tsc, JsonHash haveData = null) {
            return tsc.GetLiveDataHelper(getPerQueueData: tsc.m_iqc.GetAllQueues, haveData: haveData);
        }

        // NOT CONVERTED
        private QueryResultSet getCurrentCallsForAgent(ToolbarServerConnection tsc, JsonHash haveData = null) {
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
            }
            catch (Exception ex)
            {
                MQD("!!! getCurrentCallsForAgent() Error getting live data " + ex.Message);
                Error_DatabaseMainConnection_Failed();
            }

            return result;
        }
    
        private void dbRefreshWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            throw new NotImplementedException();
        }

        private void dbRefreshWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            QD.p("RunWorkerCompleted !!!");
        }

        private void dbRefreshWorker_DoWork(object sender, DoWorkEventArgs e) {
            ToolbarServerConnection tsc = (ToolbarServerConnection) e.Argument;
            ToolBarLiveData new_live_datas;

            tsc.m_operatingThread      = Thread.CurrentThread;
            tsc.m_operatingThread.Name = "DB Update " + tsc.m_serverLongname;

            int log_update_count = 10;

            while (true) {
                // QD.p(String.Format("dbRefreshWorker_DoWork Thread: {0} [{1}] ({2})", System.Threading.Thread.CurrentThread.ManagedThreadId, tsc.m_serverName, tsc.m_serverLongname));
                // ToggleStatusLightFromThread();

                if (tsc.Cancel) {
                    // Something else told us to stop
                    goto exit_thread;
                }

                new_live_datas = new ToolBarLiveData();
                tsc.SetUpdating(true);

                // TODO: We should not be locking m_db here.  Per-query locks should be used instead that are handled deep in DbHelper
                // We should lock the individual TSC data bucket instead
                try {
                    dbRefreshWorker_DoWorkOnTSC(sender, e, tsc, new_live_datas);
                }
                catch (Exception ex) {
                    MQD("NON-DB Exception while processing: dbRefreshWorker_DoWorkOnTSC: " + ex.ToString());

                    // We have a problem!
                    m_db_reconnects++;
                    m_db_reconnecting = true;   // UpdateDisplay will now be paused

                    goto exit_thread;
                }

                /*
                    Error_DatabaseMainConnection_Failed();
                    SetStatusBarMessage(Color.Red, String.Format("Connection interrupted ({0}), reconnecting.", tsc.m_serverName));

                    // Cross-Thread Queue For Logging
                    // https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentqueue-1?redirectedfrom=MSDN&view=netframework-4.8
                    // MQD("[DB] Reconnecting: {0}", tsc.m_serverName);  
                */

                if (!tsc.m_db.isConnected()) {
                    if (tsc.m_db.connect()) {
                        // We're good!

                        if (tsc == m_mainTSC) {
                            m_db_active       = true;  // Resume UpdateDisplayTimer
                            m_db_reconnecting = false; // FIXME: We need PER TSC Reconnecting Flags
                        }

                         SetStatusBarMessage(Color.Green, String.Format("Reconnected to server: {0}", tsc.m_serverName));
                    }
                    else {
                        // Still have a problem... Wait a few
                        System.Threading.Thread.Sleep(5000);
                        continue;
                    }

                    // Try again after we connect
                }

                // For a Major Main-DB Reconnecting event
                if (e.Cancel) {
                    goto exit_thread;
                }

                tsc.m_updateCount++;

                /*
                {
                string msg = String.Format("dbRefreshWorker_DoWork Thread: {0} [{1}] ({2}) -- Completed Update {3}",  
                        System.Threading.Thread.CurrentThread.ManagedThreadId, 
                        tsc.m_serverName, 
                        tsc.m_serverLongname, 
                        tsc.m_updateCount);
                MQD(msg);
                }
                */

                if ((tsc.m_updateCount % (ulong)log_update_count) == 0) {
                    string msg = String.Format("dbRefreshWorker_DoWork Thread: {0} [{1}] ({2}) -- Completed {3} Updates",
                        System.Threading.Thread.CurrentThread.ManagedThreadId,
                        tsc.m_serverName,
                        tsc.m_serverLongname,
                        log_update_count
                    );

                    MQD(msg);
                }

                System.Threading.Thread.Sleep(1500);
            }

          exit_thread:
            tsc.m_operatingThread = null;
            e.Cancel = true;
            return;
        }

        private void dbRefreshWorker_DoWorkOnTSC(object sender, DoWorkEventArgs e, ToolbarServerConnection tsc, ToolBarLiveData new_live_datas) {
            if (!this.m_IsAgentLoggedIn) {
                // Nothing to do yet.  Wait for the next call.
                return;
            }

            /////
            // TODO -- This goes away when we remove m_db of course
            // Are we still connected?
            tsc.m_db.DbSelectFunction("NOW");

            if (tsc.m_db.LastQueryWasError()) {
                Error_DatabaseMainConnection_Failed();
                return;
            }
            ////

            DateTime last_db_connect_time = tsc.m_db.GetDbConnectTime();

            if ((DateTime.Now - last_db_connect_time).TotalSeconds >= m_database_reconnect_sec) {
                // Gracefully reconnect to the db without rebuilding the gui
                MQD("Reconnect to DB: " + tsc.m_serverLongname);
                tsc.m_db.disconnect();
                tsc.m_db.connect();

                if (tsc == m_mainTSC) {
                  this.m_db_reconnecting = false; // Main DB No longer in the Reconnecting state
		        }
            }

            try {
                dbRefreshWorker_DoWorkOnDB(sender, e, tsc, new_live_datas);
            }
            catch (DatabaseException ex) {
                handleError(ex, "DB Error");
                if (Debugger.IsAttached) { throw; }
            }
            catch (Exception ex) {
                handleError(ex, "DB Error");
                if (Debugger.IsAttached) { throw; }
            }

            tsc.m_lastUpdated = DateTime.Now;

            if (tsc.m_isMainServer) {
                m_lastMainDatabaseUpdate = DateTime.Now;
            }
        }

        /// <summary>
        /// - Get data from the backend database
        /// - Massage the data to be displayed in the toolbar gui
        /// - Handle updating current call information
        /// - Handle screenpops
        /// - Handle screen cast recording
        /// </summary>
        /// 
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="tsc"></param>
        /// <param name="new_live_datas"></param>
        ///

        // FIXME: Terminate worker thread and wait for done -- when closing server settings dialogue
        private void dbRefreshWorker_DoWorkOnDB(object sender, DoWorkEventArgs e, ToolbarServerConnection tsc, ToolBarLiveData new_live_datas) {
            JsonHashResult backend       = tsc.m_iqc.GetServerTime();
            DateTime current_local_time  = DateTime.Now;

            if (backend.Success) {
                DateTime current_server_time = Utils.UnixTimeToDateTime(backend.Data.GetInt64("now"));
                tsc.m_serverTimeOffset       = (current_server_time - current_local_time);
            }

            SortedList<string, Hashtable> subscribed_queues = tsc.m_subscribedQueues;

            // QD.p(String.Format("dbRefreshWorker_DoWorkOnDB() Backend Pid: {0}", backend_pid["pg_backend_pid"]));

            /*
             Server:
               $result->{data}{Queues}          = GetLiveData_Queues(...)
               $result->{data}{Agents}          = GetLiveData_Agents(...)
               $result->{data}{Callers}         = GetLiveData_Callers(...)
               $result->{data}{StatusCodesSelf} = GetLiveData_StatusCodesSelf(...)
               $result->{data}{StatusCodesAll}  = GetLiveData_StatusCodesAll(...)
            */

            // One query to rule them all
            JsonHashResult toolbar_live_data        = tsc.m_iqc.GetAllToolbarLiveData();

            JsonHash live_queue_data_jh             = toolbar_live_data.Data.GetHash("Queues");
            JsonHash live_caller_data_jh            = toolbar_live_data.Data.GetHash("Callers");
            JsonHash live_agent_data_jh             = toolbar_live_data.Data.GetHash("Agents");
            JsonHash live_status_codes_self_data_jh = toolbar_live_data.Data.GetHash("StatusCodesSelf");
            JsonHash live_status_codes_all_data_jh  = toolbar_live_data.Data.GetHash("StatusCodesAll");

            // Without team view, we only show our own agent's info.  
            // TODO: Do we need this?  Doesn't team view hide the gui elements that use this???
            if (!m_enableTeamView) {
                live_agent_data_jh = new JsonHash();
            }

            // All data items are indexed by queue_name_prefixed
            Dictionary<string, List<OrderedDictionary>> live_queue_data        = getLiveQueueData(tsc,        live_queue_data_jh);
            Dictionary<string, List<OrderedDictionary>> live_caller_data       = getLiveCallerData(tsc,       live_caller_data_jh);
            Dictionary<string, List<OrderedDictionary>> live_agent_data        = getLiveAgentData(tsc,        live_agent_data_jh);
            Dictionary<string, List<OrderedDictionary>> agent_status_available = getAvailableStatusCodes(tsc, live_status_codes_self_data_jh);
            QueryResultSet agent_current_calls                                 = getCurrentCallsForAgent(tsc, live_status_codes_all_data_jh);

            // To be built below
            Dictionary<string, List<OrderedDictionary>> agent_call_data        = new Dictionary<string, List<OrderedDictionary>>();

            //if (tsc.m_serverName.StartsWith("hum")) {
            //    var live_caller_data_r       = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(live_caller_data);
            //    var live_agent_data_r        = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(live_agent_data);
            //    var live_queue_data_r        = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(live_queue_data);
            //    var agent_status_available_r = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(agent_status_available);
            //    var agent_call_data_r        = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(agent_call_data);

            //    // if (m_MainDF.IsPaused()) { Debugger.Break(); }
            //}

            ////////////////////////////////////
            // We have the data, start using it
            ////////////////////////////////////

            ////
            // ScreenPop and Screen Recording Related

            // Only process this on the local system
            if (tsc.m_isMainServer) {
                ScreenPopIfNecessary(tsc);

                if (m_currentCallEnabled && (m_currentCallForm != null)) {
                    ////////////////////////////////////
                    // Current Call In Progress Handling
                    //
                    // FIXME: This only supports a single call
                    //

                    QueryResultSetRecord current_call = null;

                    bool call_status_answered     = false;
                    bool call_status_answered_new = false;
                    bool call_status_changed      = false;
                    bool call_status_ended        = false;

                    string old_caller_channel = m_currentCallChannel;
                    string new_caller_channel = "";

                    string old_current_call_string = m_currentCallForm.GetCurrentCallString();
                    string new_current_call_string = "";

                    if (agent_current_calls.Count > 0) {
                        // Currently talking to someone
                        current_call = agent_current_calls[0];
                        new_caller_channel = current_call["channel"]; // TODO: Only supports a single call in progress
                        call_status_answered = true;
                    }
                    else {
                        // We used to be talking to someone
                        current_call = this.m_currentCallRecord; // TODO: Only supports a single call in progress
                        call_status_ended = true;
                    }

                    if (old_caller_channel != new_caller_channel) {
                        call_status_changed = true;

                        if (new_caller_channel != "") {
                            // We previously were not talking to someone, and now we are
                            call_status_answered_new = true;
                        }
                    }

                    // TODO: Would be a good place for some event hooks!
                    if (call_status_changed) {
                        // If Current Call Changed.  Either we were off a call and now we're on... or we were on a call and now we're off.

                        if (call_status_ended) {
                            // Current call JUST ended, Last round must have been a call in progress

                            // We now don't have a call in progress
                            StopScreenRecordingIfNecessary();

                            if (m_dispositionCodesEnabled) {
                                if (m_dispositionForm.m_currentCallDispositionSet) {
                                    // Last round was a call in progress AND the user has already set the disposition... Get ready for the next call
                                    m_dispositionForm.ResetDisposition_Full();
                                }
                            }
                        }
                        else {
                            // We have a call that's alive and well

                            if (call_status_answered_new) {
                                // Last round was NO call in progress, but we have a NEW call NOW

                                // Get ready for the new call no matter what.  
                                if (m_dispositionCodesEnabled) {
                                    // If the user did not set a disposition for this call, too bad
                                    m_dispositionForm.ResetDisposition_Full();
                                }

                                StartScreenRecordingIfNecessary(current_call, tsc.m_serverTimeOffset);
                            }
                            else {
                                // We're continuing a call in progress
                            }
                        }

                        // TODO: This check is temp... Eventually we'll have this enabled for all sites
                        if (current_call == null) {
                            // TODO: Handle timeout... clear current call info after 5 minutes
                        }
                        else {
                            // We either have a call right now, or we used to have a call, and now it's ended.

                            if (m_currentCallEnabled) {
                                if (!call_status_ended) {
                                    // If we're in the ended-state, we're not going to have current agent calls data
                                    m_currentCallForm.SetCurrentCallData(agent_current_calls);
                                }

                                // Display the active/ended call information

                                new_current_call_string = String.Format("{0} <{1}> -- {2}",
                                        current_call["callerid_name"],
                                        current_call["callerid_num"],
                                        current_call["queue_longname"]
                                );

                                if (call_status_ended) {
                                    new_current_call_string += " (Ended)";
                                }

                                m_currentCallForm.SetCurrentCallString(new_current_call_string);
                            }
                        }

                        // This is who we are talking to as of right now
                        // TODO: Does not support more than one concurrent call
                        if (current_call != null) {
                            m_currentCallChannel = current_call["channel"];
                        }

                        // We know the status changed, so here's what we have, null or otherwise
                        m_currentCallRecord = current_call;
                    }
                }
            }

            ////////////////

            // populate maximums for longest talk
            foreach (string queue_name in tsc.m_subscribedQueues.Keys) {
                // We use a List of OrderedDictionary just to be compatible with the other live data structures
                // We don't really need a List, so this particular one just has one element, which is [0]

                // This should never happen!
                if (!live_queue_data.ContainsKey(queue_name)) {
                    continue;
                }

                var this_queue_data = live_queue_data[queue_name][0]; // [0]  Because each live_queue_data entry is a single row
                var this_queue_data_r = DbHelper.ConvertOrderedDictionary_To_QueryResultSetRecord(this_queue_data);

                if (queue_name.StartsWith("hum")) {
                    // Debugger.Break();
                }

                //
                // agent_call_data[queue_name][0][longest_talk_seconds]
                // agent_call_data[queue_name][0][longest_talk_time]
                //
                agent_call_data.Add(queue_name, new List<OrderedDictionary> {
                  new OrderedDictionary() {
                      {"longest_talk_seconds", this_queue_data["longest_talk_seconds"]},
                      {"longest_talk_time",    this_queue_data["longest_talk_time"]}
                  }
                });
            }

            foreach (string queue_name in subscribed_queues.Keys) {
                if (live_caller_data.ContainsKey(queue_name))       { new_live_datas.AddData_Caller(queue_name, live_caller_data[queue_name]); }
                if (live_agent_data.ContainsKey(queue_name))        { new_live_datas.AddData_Agent(queue_name,  live_agent_data[queue_name]); }
                if (live_queue_data.ContainsKey(queue_name))        { new_live_datas.AddData_Queue(queue_name,  live_queue_data[queue_name]); }
                if (agent_call_data.ContainsKey(queue_name))        { new_live_datas.AddData_Call(queue_name,   agent_call_data[queue_name]); }
                if (agent_status_available.ContainsKey(queue_name)) { new_live_datas.AddData_Status(queue_name, agent_status_available[queue_name]); }
            }

            ///
            // Update the main data store

            lock (m_liveDatas) {
                // Server-Specific Data we just got back
                Dictionary<string, List<OrderedDictionary>> s_live_caller_data       = new_live_datas.GetData_Caller();
                Dictionary<string, List<OrderedDictionary>> s_live_agent_data        = new_live_datas.GetData_Agent();
                Dictionary<string, List<OrderedDictionary>> s_live_queue_data        = new_live_datas.GetData_Queue();
                Dictionary<string, List<OrderedDictionary>> s_agent_status_available = new_live_datas.GetData_Status();
                Dictionary<string, List<OrderedDictionary>> s_agent_call_data        = new_live_datas.GetData_Call();

                // QueryResultSetRecord r = DbHelper.ConvertOrderedDictionary_To_QueryResultSetRecord(s_live_caller_data["installer_hs"][0]);

                // Existing live data as of the last update
                Dictionary<string, List<OrderedDictionary>> main_live_caller_data       = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["caller"];
                Dictionary<string, List<OrderedDictionary>> main_live_agent_data        = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["agent"];
                Dictionary<string, List<OrderedDictionary>> main_live_queue_data        = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["queue"];
                Dictionary<string, List<OrderedDictionary>> main_agent_status_available = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["status"];
                Dictionary<string, List<OrderedDictionary>> main_agent_call_data        = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["call"];

                // MQDU("s_live_agent_data", DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(s_live_agent_data));
                // MQDU("s_live_agent_data", main_agent_call_data);

                ////
                // Merge our recently aquired data into the data we already have.. (we may have data from other servers... so we can't just clobber what we have)
                // The other servers (ToolbarServerConnections) will each get to this block of code, and each merge in their server-specific data with the overall data
                // This is because we have a single list of all queues from all servers that we send to the gui.  These queues are indexed by ServerName_QueueName for uniqueness

                List<
                    List<
                      Dictionary<string, List<OrderedDictionary>>
                    >
                > live_data_for_server = new List<
                    List<
                      Dictionary<string, List<OrderedDictionary>>
                    >
                >
                () { //                                                    Main m_liveDatas             New Server-Specific data
                    new List<Dictionary<string, List<OrderedDictionary>>> {main_live_caller_data,       s_live_caller_data},
                    new List<Dictionary<string, List<OrderedDictionary>>> {main_live_agent_data,        s_live_agent_data},
                    new List<Dictionary<string, List<OrderedDictionary>>> {main_live_queue_data,        s_live_queue_data},
                    new List<Dictionary<string, List<OrderedDictionary>>> {main_agent_status_available, s_agent_status_available},
                    new List<Dictionary<string, List<OrderedDictionary>>> {main_agent_call_data,        s_agent_call_data}
                };

                int live_list_position = 0;

                // For each live data item in the livedata listing.. 
                //   clear out the specific data for the queues that came back previously on the last run for this particular server
                //
                foreach (List<Dictionary<string, List<OrderedDictionary>>> live_data_item_pair in live_data_for_server) {
                    Dictionary<string, List<OrderedDictionary>> main_live_data_item   = live_data_item_pair[0];
                    Dictionary<string, List<OrderedDictionary>> server_live_data_item = live_data_item_pair[1];

                    // each live data item has a group of queues... clear out the queues for OUR server

                    foreach (string queue_name in subscribed_queues.Keys) {
                        // Merge in New Live Data
                        if (server_live_data_item.ContainsKey(queue_name)) {
                            main_live_data_item[queue_name] = server_live_data_item[queue_name];
                        }
                        else {
                            main_live_data_item.Remove(queue_name);
                        }
                    }

                    live_list_position++;
                }

                // Done Merging
                ////////////////


                // Agent  (By Queue Name)
                // Queue  (By Queue Name)
                // Status (By Queue Name)
                // Call   (By Queue Name)

                /*
                // Alternative way to Merge in new changes for the single-server result of live data... this doesn't work and needs some adjustments
                // x = item from the left (ie: s_live_caller_data)
                //
                s_live_caller_data       .ToList().ForEach(x => main_live_caller_data[x.Key]       = x.Value);
                s_live_agent_data        .ToList().ForEach(x => main_live_agent_data[x.Key]        = x.Value);
                s_live_queue_data        .ToList().ForEach(x => main_live_queue_data[x.Key]        = x.Value);
                s_agent_status_available .ToList().ForEach(x => main_agent_status_available[x.Key] = x.Value);
                s_agent_call_data        .ToList().ForEach(x => main_agent_call_data[x.Key]        = x.Value);
                */

                // Trigger GUI Updates
                m_liveGuiDataNeedsUpdate = true;
            }

            // Debug.Print("DB Update Complete: " + tsc.m_serverLongname);
        }
    }
}
