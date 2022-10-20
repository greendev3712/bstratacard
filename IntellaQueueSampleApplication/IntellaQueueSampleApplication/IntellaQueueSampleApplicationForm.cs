using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Windows.Forms;
using Lib;
using QueueLib;

namespace IntellaQueueSampleApplication {
    public partial class IntellaQueueSampleApplicationForm : Form {

        private string user = "root"; // "intellaqueue";
        private string pass = "postgresadmin"; // "zellman2014";
        private string host = "192.168.125.110"; // "192.168.70.2";
        private string port = "5432"; // "4001";

        private QueueLib.IntellaQueueControl _QC;
        private Timer _StatusTimer;

        private QueryResultSet m_currentCalls;

        private bool m_DebugData = true;
        private DataDumperForm dd;

        public IntellaQueueSampleApplicationForm() {
            InitializeComponent();

            if (this.m_DebugData) {
               this.dd = new DataDumperForm();
            }

            cmpAgentNumberTextBox.Text = "8000";
            cmpExtensionTextBox.Text   = "1010";

            _QC = new IntellaQueueControl();
            _QC.SetLoggingCallBack(WriteStatusMessage);

            _StatusTimer = new Timer {Interval = 2000};
            _StatusTimer.Tick += (sender, args) => UpdateStatus();
            _StatusTimer.Start();
        }

        private void IntellaQueueSampleApplicationForm_Load(object sender, EventArgs e) {
           
        }

        private void WriteStatusMessage(string message) {
            cmpMessagesTextBox.AppendText(DateTime.Now + " - " + message + "\r\n");
        }

        private void UpdateStatus() {
            QueryResultSet queue_login_status;
            QueryResultSet agent_status_codes;
            QueryResultSet agent_status;
            QueryResultSet agent_backlog;

            if (!_QC.AgentConnected()) {
                cmpCurrentStatusTextBox.Text = "Not Connected";
                cmpPauseComboBox.Items.Clear();
                cmpAgentCallsListBox.Items.Clear();
                return;
            }

            try {
                queue_login_status = _QC.GetLoggedInQueues();
                agent_status_codes = _QC.GetAvailableStatusCodes();
                agent_status       = _QC.GetAgentStatusPerQueue();
                agent_backlog      = _QC.GetAgentEventBackLog();

                m_currentCalls = agent_status;
            }
            catch (IntellaQueueNotConnectedException ex) {
                cmpCurrentStatusTextBox.Text = "Not Connected";
                return;
            }

            //////////////////////////////////////////////////////////////////
            // Current Status (Pause Status)

            // Uncomment (and debug debug) to show all data available
            // if (this.m_DebugData) { dd.Dumper("queue_login_status", queue_login_status); }
            
            cmpCurrentStatusTextBox.Text = "";
            foreach (QueryResultSetRecord queue_item in queue_login_status) {
                string line = "";
                line += queue_item["queue_longname"] + " ";
                line += "(" + queue_item["queue_name"] + ") ";
                line += queue_item["queue_type"] + " ";
                line += queue_item["agent_status"];

                cmpCurrentStatusTextBox.AppendText(line + "\r\n");
            }

            //////////////////////////////////////////////////////////////////
            // Set Status (Pause Status)

            int idx = cmpPauseComboBox.SelectedIndex;
            string selected_pause = "";
            if (idx >= 0) {
                selected_pause = (string)cmpPauseComboBox.Items[idx];
            }

            // Uncomment (and debug debug) to show all data available
            // if (this.m_DebugData) { dd.Dumper("agent_status_codes", agent_status_codes); }

            cmpPauseComboBox.Items.Clear();

            foreach (QueryResultSetRecord status_code_item in agent_status_codes) {
                string queue = (string)status_code_item["queue_name"];                
                string name = (string) status_code_item["status_code_name"];

                string line = String.Format("{0} {1}", queue, name);
                int item_pos = cmpPauseComboBox.Items.Add(line);

                if (selected_pause == line) {
                    cmpPauseComboBox.SelectedIndex = item_pos;
                }
            }

            //////////////////////////////////////////////////////////////////
            // My Calls Status

            idx = cmpAgentCallsListBox.SelectedIndex;
            string selected_call = "";
            if (idx >= 0) {
                selected_call = (string)cmpAgentCallsListBox.Items[idx];
            }

            // Uncomment to show all data available
            // if (this.m_DebugData) { dd.Dumper("agent_status", agent_status); }

            cmpAgentCallsListBox.Items.Clear();
            foreach (QueryResultSetRecord status_item in agent_status) {
                string line = "";
                line += status_item["queue_name"] + " ";
                line += "(" + status_item["caller_channel"] + ") ";
                line += status_item["caller_callerid_name"] + " <";
                line += status_item["caller_callerid_num"] + ">";

                int item_pos = cmpAgentCallsListBox.Items.Add(line);

                if (selected_call == line) {
                    cmpAgentCallsListBox.SelectedIndex = item_pos;
                }
            }

            // Screen Pops

            foreach (QueryResultSetRecord backlog_item in agent_backlog) {
                WriteStatusMessage(String.Format("Event -- At: {0}, {1} -- Phone Number: {2}, Case Number: {3}", 
                     backlog_item["event_when"],
                     backlog_item["event_what"],
                     backlog_item["event_callerid_num"],
                     backlog_item["event_case_number"]
                ));
            }
        }
        
        private void cmpLoginButton_Click(object sender, EventArgs e) {
            JsonQueueLoginLogoutResult login_status;

            try {
                login_status = _QC.CreateAgentConnection("192.168.1.1", user, pass, port, cmpAgentNumberTextBox.Text, cmpExtensionTextBox.Text);
            }
            catch (Exception ex) {
                WriteStatusMessage(ex.ToString());
                return;
            }

            if (!login_status.result) {
                WriteStatusMessage(String.Format("Error Logging in: {0} ({1})", login_status.error, login_status.code));
                return;
            }
            
            WriteStatusMessage(String.Format("Connected and Logged In ({0})", login_status.code));
        }
        
        private void cmpLogoutButton_Click(object sender, EventArgs e) {
            JsonQueueLoginLogoutResult logout_status;

            try {
                logout_status = _QC.AgentLogout();
            }
            catch (Exception ex) {
                WriteStatusMessage(ex.ToString());
                return;
            }

            if (!logout_status.result) {
                WriteStatusMessage(String.Format("Error Logging out: {0} ({1})", logout_status.error, logout_status.code));
                return;
            }

            WriteStatusMessage(String.Format("Disconnected and Logged out ({0})", logout_status.code));
            _QC = new IntellaQueueControl();
        }

        private void cmpPauseButton_Click(object sender, EventArgs e) {
            if (cmpPauseComboBox.SelectedIndex == -1) {
                WriteStatusMessage("No status selected");
                return;
            }

            WriteStatusMessage("Setting Status\r\n");

            string selected_status = (string)cmpPauseComboBox.Items[cmpPauseComboBox.SelectedIndex];
            string[] status_split = selected_status.Split(' ');
            string queue = status_split[0];
            string status = status_split[1];

            string result = _QC.SetAgentStatus(status, queue);

            WriteStatusMessage(result);
        }

        private void cmpUnPauseButton_Click(object sender, EventArgs e) {
            string result = _QC.SetAgentStatus("AVAILABLE");

            WriteStatusMessage(result);
        }

        private void cmpSimulateDialerPostBackButton_Click(object sender, EventArgs e) {
            string result = _QC.Debug_SimulateDialerPostback(cmpCallLogIDTextBox.Text, cmpDialedNumberTextBox.Text, cmpCaseNumberTextBox.Text, cmpResultCodeTextBox.Text);

            WriteStatusMessage(result);
        }

        private void cmpRecordingStopButton_Click(object sender, EventArgs e) {
            int idx = cmpAgentCallsListBox.SelectedIndex;
            if (idx < 0) {
                return;
            }

            QueryResultSetRecord agent_call = m_currentCalls[idx];
            if (!agent_call.Contains("caller_channel")) {
                return;
            }
            
            QueueControlResult result = _QC.RecordStop(agent_call["caller_channel"].ToString());

            // Uncomment to show all data available
            if (m_DebugData) { dd.Dumper("result.CmdData", result.CmdData); }

            string msg = result.Code + " " + result.Msg;
            if (!result.Success) { msg += "\r\n" + result.CmdData; }

            WriteStatusMessage(msg);
        }

        private void cmpRecordingStartButton_Click(object sender, EventArgs e) {
            int idx = cmpAgentCallsListBox.SelectedIndex;
            if (idx < 0) {
                return;
            }

            QueryResultSetRecord agent_call = m_currentCalls[idx];
            if (!agent_call.Contains("caller_channel")) {
                return;
            }
            
            QueueControlResult result = _QC.RecordStart(agent_call["caller_channel"].ToString());

            // Uncomment to show all data available
            if (m_DebugData) { dd.Dumper("result.CmdData", result.CmdData); }

            string msg = result.Code + " " + result.Msg;
            if (!result.Success) { msg += "\r\n" + result.CmdData; }

            WriteStatusMessage(msg);
         }

        private void cmpApiTestButton_Click(object sender, EventArgs e) {
            JsonHash functionOptions = new JsonHash();
            functionOptions.AddString("caseNumber", "1234");
            functionOptions.AddString("statusCode", "abc");

            JsonHashResult h = _QC.API_Function("vbox-markm.intellasoft.local", "root", "postgresadmin", "5432", "Dialer/Campaign/SetStatusCodeForCaseNumber", functionOptions);
        }

        private void label11_Click(object sender, EventArgs e) {

        }

        private void cmpClickToCallButton_Click(object sender, EventArgs e) {
            _QC.ClickToCall(this.cmpClickToCallTextBox.Text, "ClickToCall");
        }
    }
}
