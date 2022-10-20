using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Lib;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;

namespace QueueLib {
    public class JsonQueueLoginLogoutResult
    {
        // ReSharper disable once InconsistentNaming
        public Boolean result { get; set; }
        // ReSharper disable once InconsistentNaming
        public string code { get; set; }
        // ReSharper disable once InconsistentNaming
        public string error { get; set; }
        // ReSharper disable once InconsistentNaming
        public string login_when { get; set; }
    }

    // <summary>
    //  Results from an IntellaQueueControl command 
    // </summary>
    public class QueueControlResult {
        // ReSharper disable once InconsistentNaming
        public Boolean Success = false;
        // ReSharper disable once InconsistentNaming
        public string Code = "NO_DATA";
        // ReSharper disable once InconsistentNaming
        public string Msg = "FAILURE: NO_DATA";
        // ReSharper disable once InconsistentNaming
        public JsonHash CmdData;
        // ReSharper disable once InconsistentNaming
        public JsonHash ResultFullJson;
        // ReSharper disable once InconsistentNaming
        public List<OrderedDictionary> ResultSet = new List<OrderedDictionary>();

        // 
        public QueueControlResult(List<OrderedDictionary> commandResult) {
            // Expecting JSON as single row, in single column called 'result'
            // Expecting fields: code, result, error/msg/message, data

            if (commandResult.Count <= 0) {
                this.ResultFullJson = new JsonHash();
                this.CmdData = new JsonHash();
                return;
            }

            string json = (string) commandResult[0]["result"];

            this.ResultFullJson = new JsonHash(json);
            this.Code = this.ResultFullJson.GetString("code");
            this.Success = this.ResultFullJson.GetBool("result");
            this.CmdData = this.ResultFullJson.GetHash("data");
            this.Msg = (this.ResultFullJson.GetString("error") ?? this.ResultFullJson.GetString("msg")) ?? this.ResultFullJson.GetString("message");
        }

        public QueueControlResult(QueryResultSet commandResult) {
            // Expecting JSON as single row, in single column called 'result'
            // Expecting fields: code, result, error/msg/message, data

            if (commandResult.Count <= 0) {
                this.ResultFullJson = new JsonHash();
                this.CmdData = new JsonHash();
                return;
            }

            string json = (string)commandResult[0]["result"];

            this.ResultFullJson = new JsonHash(json);
            this.Code = this.ResultFullJson.GetString("code");
            this.Success = this.ResultFullJson.GetBool("result");
            this.CmdData = this.ResultFullJson.GetHash("data");
            this.Msg = (this.ResultFullJson.GetString("error") ?? this.ResultFullJson.GetString("msg")) ?? this.ResultFullJson.GetString("message");
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    ///  IntellaQueueControl
    /// </summary>
    ///////////////////////////////////////////////////////////////////////////////////////////////

    public class IntellaQueueControl {
        public enum ConnectionType {
            None,
            System,
            Agent
        };

        private ConnectionType m_connectionType = IntellaQueueControl.ConnectionType.None;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private DbHelper m_db = null;
        private static QD.QE_ErrorCallbackFunction m_errorHandler = QD.GenericErrorCallbackFunction;

        private string m_dbLastError;
        private bool m_connectionStarted = false; // Set this when we're done with a successful connection (so we don't try and reconnect while we're first connecting!)
        private bool m_autoReconnect = true;

        private int m_reconnectAttempts = 3;
        private int m_reconnectAttemptsMade = 0;

        private string m_agentDevice;
        private string m_agentNumber;
        private string m_agentExtension;
        private bool m_isManager = false;
        private const string DEFAULT_PORT = "5432";

        private string m_lastEventBackLogUnixTime = "0";
        private string m_dbhost;
        private string m_dbuser;
        private string m_dbpass;
        private string m_dbport;
        private string m_apiHandlerDesc;

        private Boolean m_IsAgentLoggedIn = false;

        public delegate void LogCallBackFunction(string errorMessage);
        LogCallBackFunction m_logCallBack = null;

        public void SetErrorCallback(QD.QE_ErrorCallbackFunction errorHandler) {
            m_errorHandler = errorHandler;
        }
        
        private string _DbErrorHandler(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] msgFormat) {
            this.m_dbLastError = msg;
            this.m_db = null;

            string exception_message = (ex == null) ? "(NoException)" : ex.ToString();

            if (msg == null) {
               msg = "No error message has been set";
            }

            msg = msg + "\r\n\r\nException: " + exception_message;
            
            if (m_logCallBack == null) {
                string error_message = "IntellaQueueControl LogCallback not defined.  Hint: Use IntellaQueueControl.SetLoggingCallBack() to handle these errors.\r\n\r\nError follows:\r\n" + msg;

                DataDumperForm ddf = new DataDumperForm();
                ddf.D(error_message);
                return msg;
            }

            m_logCallBack.Invoke(msg);

            return msg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="port"></param>
        /// <param name="agentNumber"></param>
        /// <param name="agentExtension"></param>
        /// <returns></returns>
        /// 
        [Obsolete("CreateSystemConnection is deprecated, please use CreateAgentConnection instead.")]
        // Migrating to the style of CreateConnection + CreateAgentConnection (CreateSystemConnection is confusing because it creates an 'Agent' connection)
        public JsonQueueLoginLogoutResult CreateSystemConnection(string host, string user, string pass, string port, string agentNumber, string agentExtension) {
            return CreateAgentConnection(host, user, pass, port, agentNumber, agentExtension);
        }

        [Obsolete("CreateSystemConnection is deprecated, please use CreateAgentConnection instead.")]
        // Migrating to the style of CreateConnection + CreateAgentConnection (CreateSystemConnection is confusing because it creates an 'Agent' connection)
        public JsonQueueLoginLogoutResult CreateSystemConnection(string host, string user, string pass, string agentNumber, string agentExtension) {
            return CreateAgentConnection(host, user, pass, DEFAULT_PORT, agentNumber, agentExtension);
        }

        public JsonQueueLoginLogoutResult CreateAgentConnection(string host, string user, string pass, string port, string agentNumber, string agentExtension) {
            if (this.DbConnected() && this.m_IsAgentLoggedIn) {
                return new JsonQueueLoginLogoutResult { result = true };
            }

            m_agentExtension = agentExtension;
            m_agentDevice = "SIP/" + agentExtension;
            m_agentNumber = agentNumber;

            string api_handler_desc = String.Format("CallQueueRemoteAPI: For Device: {0}, Agent Number: {1}", m_agentDevice, m_agentNumber);

            JsonHashResult connect_result = CreateConnection(host, user, pass, port, api_handler_desc);
            if (!connect_result.Success) {
                // Don't pass through Code/Message... Keep detailed postgres connection issues private
                return new JsonQueueLoginLogoutResult { result = false, error = "Database Connection Failed", code = connect_result.Code };
            }

            QueryResultSet result = TryDbSelect("SELECT queue.agent_login({0},{1}::integer,'API') as result", m_agentDevice, m_agentNumber);

            string json = "{}";
            if (result.Count > 0) {
                json = (string)result[0]["result"];
            }

            JsonHash jh = new JsonHash(json);
            m_isManager = jh.GetHash("agent_info").GetBool("is_manager");

            JsonQueueLoginLogoutResult login_status = new JsonQueueLoginLogoutResult {
                code = jh.GetString("code"),
                error = jh.GetString("error"),
                login_when = jh.GetString("login_when"),
                result = jh.GetBool("result")
            };

            if (!login_status.result) {
                if (m_db == null) {
                    login_status.code = "INTERNAL_ERROR";
                    login_status.error = m_dbLastError;
                }
                else {
                    lock (m_db) {
                        m_db.Dispose();
                        m_db = null;
                    }
                }

                // Don't trigger reconnects if our login has failed
                m_connectionStarted = false;
                m_IsAgentLoggedIn = false;
                return login_status;
            }

            // Trigger reconnects (if enabled) since we successfullly logged in            
            m_connectionStarted = true;
            m_IsAgentLoggedIn = true;
            this.m_connectionType = IntellaQueueControl.ConnectionType.Agent;

            return login_status;
        }

        [Obsolete("ReCreateSystemConnection is deprecated, please use CreateAgentConnection instead.")]
        // Migrating to the style of CreateConnection + CreateAgentConnection (CreateSystemConnection is confusing because it creates an 'Agent' connection)
        private JsonQueueLoginLogoutResult ReCreateSystemConnection() {
            return CreateAgentConnection(this.m_dbhost, this.m_dbuser, this.m_dbpass, this.m_dbport, this.m_agentNumber, this.m_agentExtension);
        }

        public JsonQueueLoginLogoutResult ReCreateAgentConnection() {
            return CreateAgentConnection(this.m_dbhost, this.m_dbuser, this.m_dbpass, this.m_dbport, this.m_agentNumber, this.m_agentExtension);
        }
        
        /// <summary>
        /// Create low level connection for system level api calls 
        ///   or
        /// connection to be used opon agent login for agent-toolbar/manager-toolbar related api calls
        /// </summary>
        /// <param name="host"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="port"></param>
        /// <param name="apiHandlerDesc"></param>
        /// <returns></returns>
        private JsonHashResult CreateConnection(string host, string user, string pass, string port, string apiHandlerDesc) {
            this.m_dbhost = host;
            this.m_dbuser = user;
            this.m_dbpass = pass;
            this.m_dbport = port;

            if (this.DbConnected() && this.m_IsAgentLoggedIn) {
                return new JsonHashResult { Success = true, Code = "SUCCESS", Reason = "Success" };
            }

            if (m_db == null) {
                m_db = new DbHelper(host, port, user, pass, "pbx", true, 15);

                // m_db.SetErrorCallback(m_errorHandler);
                m_db.SetErrorCallback(_DbErrorHandler);
                m_db.connect();
            }

            lock (m_db) {
                bool init_failed = false;

                try {
                    m_db.initConnection(DbHelper.generateConnectionDataObject(host, port, user, pass, "pbx"));
                    m_db.SetPrepared(true);
                }
                catch (Exception ex) {
                    Exception e = ex; // Get rid of warning
                    init_failed = true;
                }

                if (init_failed || !m_db.connect()) {
                    string error_msg = m_db.getLastError();

                    this.Log("API connection failed: " + error_msg);
                    m_db = null;
                    m_IsAgentLoggedIn = false;

                    return new JsonHashResult { Success = false, Code = "DB_FAILED", Reason = "Database Connection Failed -- "  + error_msg};
                }
            }

            TryDbSelect("SELECT public.set_who({0})", apiHandlerDesc);
            this.m_apiHandlerDesc = apiHandlerDesc;

            return new JsonHashResult { Success = true, Code = "SUCCESS", Reason = "Success" };
        }

        // TODO: We should maintain a completely separate db handle for the API_Function calls because once CreateAgentConnection() is called, 
        // TODO:  it's expected to be the same connection from start to finish and if we get a random API_Function() call, we shouldn't change our connection that AgentConnection is using
        //
        /// <summary>
        ///   Call a server-side API function, returning JsonHashResult
        /// </summary>
        /// <param name="host"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="port"></param>
        /// <param name="functionName"></param>
        /// <param name="functionOptions"></param>
        /// <returns></returns>
        public JsonHashResult API_Function(string host, string user, string pass, string port, string functionName, JsonHash functionOptions) {
            string api_handler_desc = "CallQueueRemoteAPI: Admin API";

            /*
             var functionOptions = new Lib.JsonHash(String.Format(@"
               ""caseNumber"" : ""{1}"",
               ""statusCode"" : ""{2}"",
             "), <theCaseNumber>, <newStatusCode>);
            */

            if ((this.m_dbhost == null)) {
                // First time connecting

                JsonHashResult new_connect_result = CreateConnection(host, user, pass, port, api_handler_desc);

                if (!new_connect_result.Success) {
                    // Don't pass through Code/Message... Keep detailed postgres connection issues private
                    return new JsonHashResult { Success = false, Code = "DB_FAILED", Reason = "Database Connection Failed" };
                }
            }

            if ((this.m_dbhost == host) && (this.m_dbuser == user) && (this.m_dbpass == pass) && (this.m_dbport == port)) {
                // Re-Use existing connection if possible

                if (!this.DbConnected()) {
                    JsonHashResult reconnect_result = CreateConnection(host, user, pass, port, api_handler_desc);
                    if (!reconnect_result.Success) {
                        // Don't pass through Code/Message... Keep detailed postgres connection issues private
                        return new JsonHashResult { Success = false, Code = "DB_FAILED", Reason = "Database Connection Failed" };
                    }
                }
            }
            else {
                // Connection Changed, drop and connect

                JsonHashResult connect_result = CreateConnection(host, user, pass, port, api_handler_desc);
                if (!connect_result.Success) {
                    // Don't pass through Code/Message... Keep detailed postgres connection issues private
                    return new JsonHashResult { Success = false, Code = "DB_FAILED", Reason = "Database Connection Failed" };
                }
            }

            this.m_connectionType = IntellaQueueControl.ConnectionType.System;

            ///////////////
            // Ready to go!

            // Example: functionName = SetStatusCodeForCaseNumber
            //  then the backend function for the sql query would be queue.api_set_status_code_for_case_number

            JsonHashResult api_result = m_db.DbSelectJsonFunction("public.api_call_backend_function", functionName, functionOptions.ToString());
            // JSON Return: {success: 1, data: {functionName:api_set_status_code_for_case_number; paramsCount:1}}

            this.Log(api_result.ToString());
            MessageBox.Show(api_result.ToString());

            if (!api_result.Success) {
                return new JsonHashResult { Success = false, Code = "API_FUNCTION_NOT_FOUND", Reason = "The API function requested does not exist" };
            }

            return api_result;
        }

        private Boolean DbConnected() {
            if (m_db == null) {
                m_IsAgentLoggedIn = false;
                return false;
            }

            return m_db.isConnected();
        }

        [Obsolete("SystemConnected is deprecated, please use AgentConnected instead.")]
        public Boolean SystemConnected() {
            return this.AgentConnected();
        }

        public Boolean AgentConnected() {
            if (this.m_connectionType != IntellaQueueControl.ConnectionType.Agent) {
                // Must use CreateAgentConnection to set the currently logged in agent.
                return false;
            }

            TryAgentReconnectIfNeeded();

            bool is_connected = this.DbConnected();

            if (m_connectionStarted && !is_connected) {
                this.Log("API getter/setter function called while disconnected.  (If reconnection is enabled, reconnect has failed).  Last API call has failed.");
                return false;
            }

            return is_connected;
        }

        ////
        /// <summary>
        ///  Reconnect if needed
        /// </summary>
        /// <returns>true on connected or reconnected</returns>
        /// <returns>false if we haven't yet initiated a connection</returns>/// 
        /// <returns>false on failed reconnect</returns>
        /// <returns>false on no connection, and reconnects are disabled</returns> 
        /// 
        public Boolean TryAgentReconnectIfNeeded() {
            if (!m_connectionStarted) {
                return false;
            }

            if ((m_db != null) && m_db.isConnected()) {
                return true;
            }

            // not connected
            if (this.m_autoReconnect) {
                do {
                    this.Log("API connection lost, reconnecting");
                    this.m_reconnectAttemptsMade++;

                    this.m_autoReconnect = false; // Don't infinite recurse back into here
                    JsonQueueLoginLogoutResult login_result = this.ReCreateAgentConnection();

                    if (login_result.result) {
                        // we're good now ... in the future try to reconnect if we're down again
                        this.Log("API reconnection successful");
                        this.m_reconnectAttemptsMade = 0;
                        this.m_autoReconnect         = true;
                        return true;
                    }

                    // Reconnect failed
                }
                while (this.m_reconnectAttemptsMade++ < this.m_reconnectAttempts);

                this.Log("API reconnection failed.  Will not try and reconnect again because we've reached maximum attempts: " + m_reconnectAttempts);
                return false;
            }

            this.Log("API connection lost, not reconnecting (reason: disabled)");
            return false;
        }

        /// <summary>
        /// This will allow your application to receive inner debug/logging messages from the API calls
        /// </summary>
        /// <param name="logCallBack">Pointer to your own logging function</param>
        public void SetLoggingCallBack(LogCallBackFunction logCallBack) {
            // this.m_logCallBack = logCallBack;
        }

        /// <summary>
        /// Set the number of times the API will try and reconnect upon a get/set failure
        /// </summary>
        /// <param name="attempts">Number of attempts to try and reconnect if an api command has run on a disconnected connection</param>
        public void SetReconnectAttempts(int attempts) {
            this.m_reconnectAttempts = attempts;
        }

        private void Log(string msg) {
            if (this.m_logCallBack != null) {
                m_logCallBack(msg);
            }
        }

        private QueryResultSet TryDbSelect(string query, params string[] bindArgs) {
            if (m_db == null) {
                return new QueryResultSet();
            }

            return m_db.DbSelect(query, bindArgs);
        }

        [Obsolete("TryDbQueryWithParams is deprecated, please use TryDbSelect instead.")]
        private int TryDbQueryWithParams(string query, List<OrderedDictionary> result, params string[] bindArgs) {
            if (m_db == null) {
                return 0;
            }

            QueryResultSet query_result = m_db.DbSelect(query, bindArgs);
            result = DbHelper.ConvertQueryResultSet_To_ListOfOrderedDictionary(query_result);

            return (m_db.LastQueryWasError() ? 0 : 1);
        }
    
        public JsonQueueLoginLogoutResult AgentLogout() {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSetRecord login_result = m_db.DbSelectSingleRow("SELECT queue.agent_logout({0},'API') as result", m_agentDevice);

            if (login_result.Contains("result")) {
                string json = (string) login_result["result"];
                JsonQueueLoginLogoutResult login_status = JsonConvert.DeserializeObject<JsonQueueLoginLogoutResult>(json);
                return login_status;
            }

            m_connectionStarted = false;
            m_db.Dispose();

            JsonQueueLoginLogoutResult r = new JsonQueueLoginLogoutResult {
                result = false,
                code = "LOGOUT_FAILED_INTERNAL_ERROR"
            };

            return r;
        }

        public QueryResultSet GetLoggedInQueues() {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            return m_db.DbSelect("SELECT * FROM live_queue.v_agent_logins WHERE agent_device = ? ORDER BY queue_name", m_agentDevice);
        }

        public QueryResultSet GetAllQueues() {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result;

            if (m_isManager) {
                result = m_db.DbSelect("SELECT * FROM live_queue.v_queues ORDER BY queue_name");
            }
            else {
                result = m_db.DbSelect(@"
                    SELECT * FROM live_queue.v_queues 
                    WHERE queue_name IN (SELECT * FROM live_queue.v_agent_logins WHERE agent_device = ?)
                    ORDER BY queue_name", this.m_agentDevice);
            }

            return result;
        }

        public QueryResultSet GetAgentStatusPerQueue() {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            return m_db.DbSelect("SELECT * FROM live_queue.v_toolbar_agents WHERE agent_device = ? ORDER BY queue_name, agent_fullname", m_agentDevice);
        }

        public QueryResultSet GetAgentEventBackLog(string eventsSinceUnixTime) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect(
                "SELECT * FROM live_queue.agents_event_backlog WHERE agent_device = ? AND event_when_unixtime > ?::numeric ORDER BY event_when",
                m_agentDevice, eventsSinceUnixTime);

            foreach (QueryResultSetRecord r in result.Reverse()) { 
                Console.WriteLine(r.ToString());
            }

            return result;
        }

        ////
        // Automatically get the next batch of events
        //
        public QueryResultSet GetAgentEventBackLog() {
            QueryResultSet events = GetAgentEventBackLog(this.m_lastEventBackLogUnixTime);

            // Events come in oldest->newwest, so the last event is the most recent
            if (events.Count > 0) {
                QueryResultSetRecord event_item = events[events.Count - 1];
                m_lastEventBackLogUnixTime = (string) event_item["event_when_unixtime"];
            }

            return events;
        }

        public string RecordCallResult(string callLogId, string callResultCode) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect("SELECT queue.set_call_result(?,?) as result", callLogId, callResultCode);

            if (result.Count == 0) {
                return "FAILURE: NO_DATA";
            }

            return (string)result[0]["result"];
        }

        public string RecordCallResult(string callLogId, string callResultCode, string callCaseNumber) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect("SELECT queue.set_call_result(?,?,?) as result", callLogId, callResultCode, callCaseNumber);

            if (result.Count == 0) {
                return "FAILURE: NO_DATA";
            }

            return (string)result[0]["result"];
        }

        public QueueControlResult RecordStart(string channelName) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect("SELECT queue.recording_start(?,?) as result", m_agentDevice, channelName);

            return new QueueControlResult(result);
        }

        public QueueControlResult RecordStop(string channelName) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            channelName = null;

            List<OrderedDictionary> result = new List<OrderedDictionary>();
            TryDbQueryWithParams("SELECT queue.recording_stop({0},{1}) as result", result, m_agentDevice, channelName);

            return new QueueControlResult(result);
        }

        public string ClickToCall(string phoneNumber, string calleeName) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect("SELECT asterisk.pbx_dial(?,?,?) as result", m_agentExtension, phoneNumber, calleeName);

            if (result.Count == 0) {
                return "FAILURE: NO_DATA";
            }

            return (string)result[0]["result"];
        }

        public string ClickToCall(string phoneNumber, string calleeName, string json_opts) {
            try {
                JsonHash opts = new JsonHash(json_opts);
            }
            catch (Exception e) {
                return "FAILURE: Invalid JSON: " + e.ToString();
            }

            if (!this.SystemConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect("SELECT asterisk.pbx_dial(?,?,?,?) as result", m_agentExtension, phoneNumber, calleeName, json_opts);

            if (result.Count == 0) {
                return "FAILURE: NO_DATA";
            }

            return (string)result[0]["result"];
        }

        public QueryResultSet GetAllAgentStatus() {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect("SELECT * FROM live_queue.agents_meta order by agent_firstname, agent_lastname");

            return result;
        }

        public QueryResultSet GetAllAgentStatusPerQueue() {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect("SELECT * FROM live_queue.v_toolbar_agents order by queue_name, agent_fullname");

            return result;
        }

        public QueryResultSet GetAllQueueCallers() {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect("SELECT * FROM live_queue.v_toolbar_callers order by queue_name, waiting_time DESC");

            return result;
        }

        public QueryResultSet GetAvailableStatusCodes() {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            return m_db.DbSelect("SELECT * FROM live_queue.v_agent_status_codes_self WHERE agent_device = ? order by queue_name, status_code_name", m_agentDevice);
        }

        public string SetAgentStatus(string statusCode, string queueName) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect("SELECT live_queue.agent_set_status(?,?,?) as result", m_agentDevice, statusCode, queueName);

            if (result.Count == 0) {
                return "FAILURE: NO_DATA";
            }

            return (string) result[0]["result"];
        }

        public string SetAgentStatus(string statusCode) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect("SELECT live_queue.agent_set_status(?,?) as result", m_agentDevice, statusCode);

            if (result.Count == 0) {
                return "FAILURE: NO_DATA";
            }

            return result[0]["result"];
        }

        public string Debug_SimulateDialerPostback(string callLogId, string phoneNumber, string caseNumber, string resultCode) {
           if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect("SELECT dialer.debug_simulate_dialer_postback(?,?,?,?) as result", callLogId, phoneNumber, caseNumber, resultCode);

            if (result.Count == 0) {
                return "FAILURE: NO_DATA";
            }

            return result[0]["result"];
        }
    }

    public class IntellaQueueNotConnectedException : Exception {
        public IntellaQueueNotConnectedException(string message) : base(message) {
            
        }
    }
}
