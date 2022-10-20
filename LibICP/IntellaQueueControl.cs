using Lib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using RestSharp;

namespace LibICP
{
    public class IntellaQueueControl {
        public enum ConnectionType {
            None,
            System,
            Agent
        };

        private ConnectionType m_connectionType = IntellaQueueControl.ConnectionType.None;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private RestClient m_icp_rc;  // RC = RestClient
        private static QD.QE_ErrorCallbackFunction m_errorHandler = QD.GenericErrorCallbackFunction;

        private DbHelper m_db;

        private bool m_connectionStarted = false; // Set this when we're done with a successful connection (so we don't try and reconnect while we're first connecting!)
        private bool m_autoReconnect = true;

        private int m_reconnectAttempts = 3;
        private int m_reconnectAttemptsMade = 0;

        private string m_agentDevice;
        private string m_agentNumber;
        private string m_agentExtension;
        private bool m_isManager = false;

        private const string DEFAULT_PORT = "443";

        private string m_lastEventBackLogUnixTime = "0";
        private string m_dbhost;
        private string m_dbuser;
        private string m_dbpass;
        private string m_dbport;

        private Boolean m_IsAgentLoggedIn = false;

        public delegate void LogCallBackFunction(string errorMessage);
        LogCallBackFunction m_logCallBack = null;

        // CONVERTED
        public void SetErrorCallback(QD.QE_ErrorCallbackFunction errorHandler) {
            m_errorHandler = errorHandler;
        }
        
        // CONVERTED
        private string _CommandErrorHandler(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] msgFormat) {
            this.m_icp_rc = null;

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

        // TODO
        /// <summary>
        ///   Ability to run system-level commands using an API account
        /// </summary>
        /// <param name="host"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        /// 
        
        public JsonHashResult CreateSystemConnection(string host, string user, string pass, string port) {
            throw new NotImplementedException();
        }
        
        // CONVERTED
        public JsonQueueLoginLogoutResult CreateAgentConnection(string host, string user, string pass, string port, string agentNumber, string agentExtension, string agentPin = null) {
            m_agentExtension = agentExtension;
            m_agentDevice    = "SIP/" + agentExtension;
            m_agentNumber    = agentNumber;

            JsonQueueLoginLogoutResult login_status = CreateConnetionAndAuthHelper(host, user, pass, port, agentNumber, agentExtension, agentPin);
            if (!login_status.success) {
                return login_status;
            }

            // Some systems may tell us to connect to somewhere else
            string agent_specific_server = "comm5.nyc.intellasoft.net";

            if (agent_specific_server != null) {
                login_status = CreateConnetionAndAuthHelper(agent_specific_server, user, pass, port, agentNumber, agentExtension, agentPin);
            }

            // Trigger reconnects (if enabled) since we successfullly logged in            
            m_connectionStarted   = true;
            m_IsAgentLoggedIn     = true;
            this.m_connectionType = IntellaQueueControl.ConnectionType.Agent;

            login_status.agent_data.AddString("agent_specific_server", agent_specific_server);

            return login_status;
        }
        
        // CONVERTED
        private JsonQueueLoginLogoutResult CreateConnetionAndAuthHelper(string host, string user, string pass, string port, string agentNumber, string agentExtension, string agentPin = null) {
            string api_handler_desc = String.Format("CallQueueRemoteAPI: For Extension: {0}, Agent Number: {1}", agentExtension, agentNumber);

            JsonHashResult connect_result = CreateConnection(host, user, pass, port, api_handler_desc);
            if (!connect_result.Success) {
                this.Log("IntellaQueueControl: Could not connect or authenticate:" + connect_result.ToString());

                // Don't pass through Code/Message... Keep detailed postgres connection issues private
                return new JsonQueueLoginLogoutResult { success = false, reason = "Connection Failed", code = connect_result.Code };
            }

            JsonQueueLoginLogoutResult login_status = Agent_Auth(m_agentExtension, m_agentNumber);

            if (!login_status.success) {
                lock (m_icp_rc) {
                    m_icp_rc.Dispose();
                    m_icp_rc = null;
                }

                // Don't trigger reconnects if our login has failed
                m_connectionStarted = false;
                m_IsAgentLoggedIn   = false;
            }

            return login_status;
        }

        // CONVEDRTED
        public JsonQueueLoginLogoutResult Agent_Auth(string agentExtension, string agentNumber, string agentPin = null) {
            JsonHash login_data = new JsonHash();
            login_data.AddString("agent_extension", m_agentExtension);
            login_data.AddString("agent_number",    m_agentNumber);

            JsonHashResult result = RestRequest_SendJson(Method.Post, "/api/public/CallQueue/AgentAuth", login_data);

            JsonQueueLoginLogoutResult login_status = new JsonQueueLoginLogoutResult {
                success      = result.Success,
                code         = result.Code,
                reason       = result.Reason,
                agent_data   = result.Data,
            };

            return login_status;
        }

        // CONVERTED
        public JsonQueueLoginLogoutResult ReCreateAgentConnection() {
            return CreateAgentConnection(this.m_dbhost, this.m_dbuser, this.m_dbpass, this.m_dbport, this.m_agentNumber, this.m_agentExtension);
        }
        
        // CONVERTED
        private JsonHashResult RestRequest_SendJson(Method method, string path, JsonHash json) {
            RestRequest request = new RestRequest(path, method);

            if (json != null) {
                request.AddBody(json.ToJson(), "application/json");
            }

            JsonHashResult jhr;
            RestResponse result = m_icp_rc.Execute(request);

            if (!result.IsSuccessful && (result.ContentLength == 0)) {
                JsonHash data_jh = new JsonHash();
                data_jh.AddString("Exception", result.ErrorException.ToString());

                jhr = new JsonHashResult{
                    Success  = false,
                    Code     = "REQUEST_FAILED",
                    Reason   = result.ErrorMessage,
                    Data     = data_jh,
                };

                return jhr;
            }

            // MessageBox.Show(result.Content);

            JsonHash jh = new JsonHash(result.Content);

            jhr = new JsonHashResult{
                Success  = jh.GetBool("success"),
                Code     = jh.GetString("code"),
                Reason   = jh.GetString("reason"),
                Data     = jh.GetItem("data")
            };

            return jhr;
        }

        // CONVERTED
        private JsonHashResult RestRequest(Method method, string path) {
            return RestRequest_SendJson(method, path, null);
        }

        // CONVERTED
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

            // Base:
            //   https://user:pass@server.domain.com
            // We will be then using requests like:
            //   https://user:pass@server.domain.com/public/CallQueue/AgentLogin
            //

            string proto = "https";
            if (port == "80") { proto = "http"; }

            string base_url = String.Format("{0}://{1}:{2}@{3}:{4}", proto, user, pass, host, port);
            m_icp_rc = new RestClient(base_url);

            // m_db.SetErrorCallback(m_errorHandler);
            //m_db.SetErrorCallback(_DbErrorHandler); // FIXME

            return this.GetServerTime();
        }

        public JsonHashResult GetServerTime() {
            return this.RestRequest(Method.Get, "/api/public/Core/ConnectionTest");
        }

        // CONVERTED
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
                    // Don't pass through Code/Message... Keep detailed connection issues private
                    return new JsonHashResult { Success = false, Code = "CONNECTION_FAILED", Reason = "Connection Failed" };
                }
            }

            if ((this.m_dbhost == host) && (this.m_dbuser == user) && (this.m_dbpass == pass) && (this.m_dbport == port)) {
                // Re-Use existing connection details
            }
            else {
                // Connection Changed, drop and connect

                JsonHashResult connect_result = CreateConnection(host, user, pass, port, api_handler_desc);
                if (!connect_result.Success) {
                    // Don't pass through Code/Message... Keep detailed postgres connection issues private
                    return new JsonHashResult { Success = false, Code = "CONNECTION_FAILED", Reason = "Connection Failed" };
                }
            }

            this.m_connectionType = IntellaQueueControl.ConnectionType.System;

            ///////////////
            // Ready to go!

            // Example: functionName = SetStatusCodeForCaseNumber
            //  then the backend function for the sql query would be queue.api_set_status_code_for_case_number

            // FIXME: we should send the full path if we have a function name containing a path
            // FIXME: otherwise we'll fall-back to old-style /api/public/CallQueue/{1}
            //
            JsonHashResult api_result = RestRequest_SendJson(Method.Post, String.Format("/api/public/CallQueue/{0}", functionName), functionOptions);
               
            // JSON Return: {success: 1, data: {functionName:api_set_status_code_for_case_number; paramsCount:1}}

            this.Log(api_result.ToString());
            MessageBox.Show(api_result.ToString());

            if (!api_result.Success) {
                return new JsonHashResult { Success = false, Code = "API_FUNCTION_NOT_FOUND", Reason = "The API function requested does not exist" };
            }

            return api_result;
        }

        // TODO
        public Boolean AgentConnected() {
            if (this.m_connectionType != IntellaQueueControl.ConnectionType.Agent) {
                // Must use CreateAgentConnection to set the currently logged in agent.
                return false;
            }

            return true;
        }

        // TODO
        public Boolean SystemConnected() {
            if (this.m_connectionType != IntellaQueueControl.ConnectionType.Agent) {
                // Must use CreateAgentConnection to set the currently logged in agent.
                return false;
            }

            return true;
        }

        // TODO
        /// <summary>
        /// This will allow your application to receive inner debug/logging messages from the API calls
        /// </summary>
        /// <param name="logCallBack">Pointer to your own logging function</param>
        public void SetLoggingCallBack(LogCallBackFunction logCallBack) {
            // this.m_logCallBack = logCallBack;
        }

        // TODO
        /// <summary>
        /// Set the number of times the API will try and reconnect upon a get/set failure
        /// </summary>
        /// <param name="attempts">Number of attempts to try and reconnect if an api command has run on a disconnected connection</param>
        public void SetReconnectAttempts(int attempts) {
            this.m_reconnectAttempts = attempts;
        }

        // CONVERTED
        private void Log(string msg) {
            if (this.m_logCallBack != null) {
                m_logCallBack(msg);
            }
        }
    
        public JsonQueueLoginLogoutResult AgentLogout() {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            JsonHash logout_data = new JsonHash();
            logout_data.AddString("agent_device", this.m_agentDevice);

            JsonHashResult result = RestRequest_SendJson(Method.Post, "/api/public/CallQueue/AgentLogout", logout_data);

            JsonQueueLoginLogoutResult logout_status = new JsonQueueLoginLogoutResult {
                success    = result.Success,
                code       = result.Code,
                reason     = result.Reason,
                agent_data = result.Data,
            };

            // JsonConvert.DeserializeObject<JsonQueueLoginLogoutResult>(json);

            m_connectionStarted = false;

            return logout_status;
        }

        // TODO
        public QueryResultSet GetLoggedInQueues() {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            // return m_db.DbSelect("SELECT * FROM live_queue.v_agent_logins WHERE agent_device = ? ORDER BY queue_name", m_agentDevice);

            return null;
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

        public CommandResult RecordStart(string channelName) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            QueryResultSet result = m_db.DbSelect("SELECT queue.recording_start(?,?) as result", m_agentDevice, channelName);

            return new CommandResult(result);
        }

        public CommandResult RecordStop(string channelName) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            channelName = null;

            List<OrderedDictionary> result = new List<OrderedDictionary>();
            m_db.DbSelect("SELECT queue.recording_stop({0},{1}) as result", m_agentDevice, channelName);

            return new CommandResult(result);
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

            if (!this.AgentConnected()) {

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
                 
        public JsonHashResult GetQueuesShow(string prefixQueueName = null) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            JsonHash send_params = new JsonHash();
            send_params.AddString("agent_extension", m_agentExtension);
            send_params.AddString("agent_number",    m_agentNumber);

            if (prefixQueueName != null) {
                send_params.AddString("prefix_queue_name", prefixQueueName);
             }

            JsonHashResult result = this.RestRequest_SendJson(Method.Post, "/api/public/CallQueue/CFG_GetQueues_Show", send_params);

            return result;
        }
        
        public JsonHashResult GetQueuesAdditional(string prefixQueueName = null) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            JsonHash send_params = new JsonHash();
            send_params.AddString("agent_extension", m_agentExtension);
            send_params.AddString("agent_number",    m_agentNumber);

            if (prefixQueueName != null) {
                send_params.AddString("prefix_queue_name", prefixQueueName);
            }

            JsonHashResult result = this.RestRequest_SendJson(Method.Post, "/api/public/CallQueue/CFG_GetQueues_Additional", send_params);

            return result;
        }

        // Converted
        public JsonHashResult GetAllAgentStatusPerQueue(string prefixQueueName = null) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            JsonHash send_params = new JsonHash();
            send_params.AddString("agent_extension", m_agentExtension);
            send_params.AddString("agent_number",    m_agentNumber);

            if (prefixQueueName != null) {
                send_params.AddString("prefix_queue_name", prefixQueueName);
             }

            JsonHashResult result =  this.RestRequest_SendJson(Method.Post, "/api/public/CallQueue/GetLiveData_Agents", send_params);

            return result;
        }

        // CONVERTED
        public JsonHashResult GetAllQueueCallers(string prefixQueueName = null) {
            if (!this.AgentConnected()) {
                throw new IntellaQueueNotConnectedException("Active IntellaQueue connection must exist for this function call");
            }

            JsonHash send_params = new JsonHash();
            send_params.AddString("agent_extension", m_agentExtension);
            send_params.AddString("agent_number",    m_agentNumber);

            if (prefixQueueName != null) {
                send_params.AddString("prefix_queue_name", prefixQueueName);
             }

            JsonHashResult result = this.RestRequest_SendJson(Method.Post, "/api/public/CallQueue/GetLiveData_Callers", send_params);

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
}
