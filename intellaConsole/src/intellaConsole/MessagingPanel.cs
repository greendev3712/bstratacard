using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lib;

namespace intellaConsole {
    public partial class MessagingPanel : Form {
        private intellaConsole m_ic;
        private DbHelper m_db;

        private bool m_messagingIsEnabled = false;
        string m_messagingExtension   = null;
        string m_messagingDevice      = null;
        string m_messagingContactName = null;
        string m_messageHistory       = "";

        private string m_messageCheckSinceWhen = "0";
        private Timer m_messageCheckTimer;

        public MessagingPanel() {
            InitializeComponent();
        }

        public void setIntellaConsoleForm(intellaConsole ic) {
            this.m_ic = ic;
        }

        public void SetCurrentConversationExtension(string device, string extension, string contactName) {
            m_messagingExtension   = extension;
            m_messagingDevice      = device;
            m_messagingContactName = contactName;

            this.SetMessageDestinationInfo(contactName + " <" + extension + ">");
        }

        public Panel getBodyPanel() {
            return this.cmpMessagingPanel;
        }

        // We are connected to the db
        public void setDbHelper(DbHelper db) {
            this.m_db = db;

            m_db.DbSelect("SET extra_float_digits = 3"); // Maybe not needed
            m_messageCheckSinceWhen = m_db.DbSelectSingleValueString("SELECT extract('epoch' from NOW())");
        }

        public void ShowBodyPanel() {
            this.cmpMessagingPanel.Show();
        }

        public void HideBodyPanel() {
            this.cmpMessagingPanel.Hide();
        }

        private void MessagingForm_Load(object sender, EventArgs e) {
            this.TopMost = true;
        }

        // Set the info that sits just below the titlebar to note who we are talking to
        public void SetMessageDestinationInfo(string destinationInfo) {
            // We're talking to someone new
            m_messageHistory = "";
            cmpMessageHistoryTextBox.Text = "";

            this.cmpMessageHistoryTitleBarTalkingToLabel.Text = "Conversation With: " + destinationInfo;

            // TODO: We should get this when the operator logs in
            string operator_extension = this.m_ic.getOperatorExten();
            string operator_device   = this.m_ic.getOperatorDevice();

            m_messageCheckTimer = new Timer();
            m_messageCheckTimer.Interval = 2000;
            m_messageCheckTimer.Tick += delegate (System.Object sender, System.EventArgs e) {
                m_messageCheckTimer.Stop();
                
                QueryResultSet messages = m_db.DbSelect(
                  @"
                    SELECT
                      *,
                      extract('epoch' from log_when)::text as log_when_unixtime -- Avoid npgsql conversions that lose precision
                    FROM
                      operator_console_live.messages
                    WHERE
                      destination_operator_device        = {0}
                      AND extract('epoch' from log_when) > {1}::double precision
                    ORDER BY
                      log_when ASC
                  ",
                  operator_device, m_messageCheckSinceWhen
                );

                foreach (QueryResultSetRecord message in messages) {
                    addMessageToMessageHistory(message["from_extension"], message["body"]);
                    m_messageCheckSinceWhen = message["log_when_unixtime"];
                }

                m_messageCheckTimer.Start();
            };

            m_messageCheckTimer.Start();
            addMessageToMessageHistory("Notice", "Starting conversation with:" + destinationInfo);
        }

        // Probably should be using .Paint... but this was pretty easy
        //
        private void cmpMessageHistoryTextBox_VisibleChanged(object sender, EventArgs e) {
            if (!m_messagingIsEnabled) {
                this.cmpMessageHistoryTextBox.Text = @"
This module is not currently installed
";
                return;
            }

            this.cmpMessageHistoryTextBox.Text = @"
No conversation currently active.

Please use the 'Send Message' button on an extension in 'Directory' to start a conversation.
";
        }

        private void cmpSendMessageTextBox_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == Convert.ToChar(Keys.Return)) { 
                sendCurrentMessage();
            }
        }

        private void cmp_Click(object sender, EventArgs e) {
            sendCurrentMessage();
        }

        private void addMessageToMessageHistory(string fromWho, string message) {
            string now = DateTime.Now.ToString("MM/dd hh:mm:ss tt");
            message    = String.Format("[{0}] <{1}> {2}", now, fromWho, message);

            message += "\r\n";

            m_messageHistory += message;
            cmpMessageHistoryTextBox.AppendText(message);
        }

        private void sendCurrentMessage() {
            string new_message_entry = cmpSendMessageTextBox.Text;

            addMessageToMessageHistory("Operator", new_message_entry);
            cmpSendMessageTextBox.Text = "";

            Application.DoEvents();

            // FromExten, ToDevice, Message, Opts
            JsonHashResult result = m_db.DbSelectJsonFunction("operator_console.api_send_message_to_phone", m_ic.getOperatorExten(), m_messagingDevice, new_message_entry, "");

            if (result.Success) {
                addMessageToMessageHistory("System", "The message has been delivered");
            }
            else { 
                addMessageToMessageHistory("System", "The system could not deliver the message.");
            }


        }
    }
}
