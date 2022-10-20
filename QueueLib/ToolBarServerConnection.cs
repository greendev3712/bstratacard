using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Lib;

namespace QueueLib
{
    public class ToolBarServerConnection
    {
        private SSLTcpClient m_ssl_tcp_client;
        private DbHelper m_db;

        private string m_host;
        private string m_port;
        private string m_tenant;
        private string m_username;
        private string m_password;
        private string m_extension;
        private string m_agentNumber;
        private string m_agentPin;

        FailureCallback m_failureCallback;
        public delegate void FailureCallback(String message);

        public ToolBarServerConnection(string host, string port, string tenant, string username, string password)
        {
            m_ssl_tcp_client = new SSLTcpClient(host, Int32.Parse(port), host);
            Thread.Sleep(1000);

            m_host = host;
            m_port = port;
            m_tenant = tenant;
            m_username = username;
            m_password = password;
            
            string result = m_ssl_tcp_client.ReadLine();
            if ((result == null) || (result != "ToolBarDaemon: 1.0"))
            {
                QD.p("Failed handshake with ToolBarDaemon. Got: " + result);
                throw new Exception("Failed handshake with ToolBarDaemon");
            }
        }

        public void SetFailureCallback(FailureCallback c)  {
            m_failureCallback = c;
        }

        public ToolBarServerConnection(DbHelper db)
        {
            // For the m_db style connection
            this.m_db = db;
        }


        // Support the old style direct-to-db connection
        public void SetDbHelper(DbHelper db)
        {
            this.m_db = db;
        }


        public string ReadLine()
        {
            return m_ssl_tcp_client.ReadLine();
        }

        public JsonHash ReadJsonResult()
        {
            StringBuilder full_result = new StringBuilder();
            string line;

            do
            {
                line = ReadLine();
                if (line == null) { break; }

                full_result.Append(line);
            }
            while (line != null);

            return new JsonHash(full_result.ToString());
        }

        public bool WriteLine(string line)
        {
            try {
                m_ssl_tcp_client.WriteLine(line);
                return true;
            }
            catch (IOException e) {
                // No longer connected
                m_failureCallback(e.ToString());
            }

            return false;
        }

        public string ServerLogin()
        {
            WriteLine(String.Format("Login={{{0}}},{{{1}}},{{{2}}}", m_tenant, m_username, m_password));

            string result = ReadLine();
            if ((result != null) && (result != "LOGIN: SUCCESS")) { 
                return result;
            }

            return "SUCCESS";
        }

        public string AgentLogin(string extension, string agentNumber, string agentPin)
        {
            m_extension    = extension;
            m_agentNumber = agentNumber;
            m_agentPin    = agentPin;

            if (m_db != null)
            {
                string exten_result = null;

                lock (m_db)
                {
                    if (m_db.getSingleFromDb(ref exten_result, "extension", "asterisk.extensions", "extension", extension) != 0)
                    {
                        return "AGENT LOGIN: Invalid Extension";
                    }
                }

                return "SUCCESS";
            }

            WriteLine(String.Format("Agent={{{0}}},{{{1}}},{{{2}}}", extension, agentNumber, agentPin));
            string result = ReadLine();
            if ((result != null) && (result != "AGENT LOGIN: SUCCESS")) { 
                return result;
            }

            return "SUCCESS";
        }


        /// <summary>
        /// Read a JSON result from the server connection, convert to a QueryResultSetRecord (Single row data)
        /// </summary>
        /// <returns>QueryResultSetRecord of the converted JSON</returns>
        private QueryResultSetRecord GetJsonResult_ReturnQueryResultSetRecord() {
            JsonHash result = ReadJsonResult();
            return result.ToQueryResultSetRecord();
        }

        /// <summary>
        /// Read a JSON result from the server connection, convert to a QueryResult (Multi-Row data)
        /// </summary>
        /// <returns>QueryResultSet of the converted JSON (Containing one or more QueryResultSetRecord)</returns>
        private QueryResultSet GetJsonResult_ReturnQueryResultSet() {
            JsonHash result = ReadJsonResult();
            return result.ToQueryResultSet();
        }

        /// <summary>
        /// Read a JSON result from the server connection, convert to a HashTable keyd on [key]
        /// </summary>
        /// <returns>QueryResultSet of the converted JSON (Containing one or more QueryResultSetRecord)</returns>
        private Hashtable GetJsonResult_ReturnHashTable(string keyName) {
            JsonHash result = ReadJsonResult();
            Hashtable h = result.GetHashTable();


            return h;
        }


        public string GetToolBarLibVersionRequired() {
            return "2.0.0.0";
        }

        public string GetToolBarProtocolVersionRequired() {
            return "30";
        }

        /// <summary>
        /// Single-Row Agent Data
        /// 
        /// </summary>
        /// <returns></returns>
        public QueryResultSetRecord GetAgentInfo()
        {
            if (m_db != null) {
                QueryResultSetRecord agent_info;

                lock (m_db) {
                    agent_info = m_db.DbSelectSingleRow(@"
                        SELECT agent_num, agent_pin, is_manager, agent_firstname, agent_lastname
                        FROM queue.agents
                        WHERE agent_num = ?", m_agentNumber);
                }

                return agent_info;
            }

            WriteLine(String.Format("agent_data"));
            return GetJsonResult_ReturnQueryResultSetRecord();
        }

        /// <summary>
        /// Multi-Row Queue Assignments and Per Queue Settings/Prefs
        /// </summary>
        /// <returns></returns>
        public QueryResultSet GetQueueAssignmentsAndPrefs() {
            WriteLine(String.Format("queue_agent_assignments"));
            return GetJsonResult_ReturnQueryResultSet();
        }

        public Hashtable GetQueuesShow_ToHashTable(string agentNumber) {
            WriteLine(String.Format("toolbar_config_show_queues agent_num={{{0}}}", agentNumber));
            return GetJsonResult_ReturnHashTable("queue_name");
        }
    }
}
