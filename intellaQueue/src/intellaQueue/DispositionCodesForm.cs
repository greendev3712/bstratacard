using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using intellaQueue;
using KellermanSoftware.CompareNetObjects;
using System.Linq;
using Lib;
using System.Threading;

namespace QueueLib {
    public partial class DispositionCodesForm : Form {
        private Thread m_guiThread;
        private IntellaQueueForm m_intellaQueueForm;
        
        // Index of [queue_name][status_longname] = status_code_name
        //   so that a status longname can point us directly to a status_code_name
        private Dictionary<string, Dictionary<string, string>> m_agentStatusIndex;

        public Boolean m_currentCallDispositionSet = false;
        private int m_statusControlsHeight_Hideable; // The collapsible height of all controls (the amount of space that goes away when we hide)
        private Hashtable m_cmpComboBoxes; // <string, ComboBox>
        
        private DateTime m_dispositionLastSet = DateTime.Now;
        private bool m_dispositionUpdating = false;
        private System.Windows.Forms.Timer m_dispositionUpdateTimeoutTimer = new System.Windows.Forms.Timer();
        private Object m_dispositionMutex = new Object();

        private Color m_button_save_color; //  = b.ForeColor;
        private string m_button_save_text; // = b.Text;

        private DispositionCodesHistoryForm m_cmpDispositionCodesHistoryForm;

        private void ThisFormConstruct(IntellaQueueForm intellaQueueForm) {
            m_guiThread = Thread.CurrentThread;
            this.m_intellaQueueForm = intellaQueueForm;

            // If an update gets stuck
            m_dispositionUpdateTimeoutTimer.Interval = 10 * 1000; // 10 Seconds
            m_dispositionUpdateTimeoutTimer.Tick += DispositionUpdateTimeoutTimer_Tick;

            InitializeComponent();

            // Form Design Defaults
            this.m_button_save_color = this.cmpSaveUpdateData.ForeColor;
            this.m_button_save_text  = this.cmpSaveUpdateData.Text;

            this.m_cmpDispositionCodesHistoryForm = new DispositionCodesHistoryForm(m_intellaQueueForm, m_intellaQueueForm.GetDatabaseHandle());
        }

        public DispositionCodesForm(IntellaQueueForm intellaQueueForm) {
            ThisFormConstruct(intellaQueueForm);
        }

        private void DispositionUpdateTimeoutTimer_Tick(object sender, EventArgs e)
        {
            m_dispositionUpdateTimeoutTimer.Stop();

            if (!m_dispositionUpdating) {
                return;
            }

            this.DispositionSaveButton_RestoreDefaults();

            IntellaQueueForm.MQD("[DispositionSet] Set Disposition Failed -- Server Timeout");

            m_intellaQueueForm.SetStatusBarMessage(Color.Red, "Set Disposition Failed -- Server Timeout.  Please try again.");
            this.DispositionComboBox_SetErrorState();

            m_dispositionUpdating = false;
        }

        public void PopulateDispositionsData()
        {

            DbHelper db = this.m_intellaQueueForm.GetDatabaseHandle();
            QueryResultSet dispositions = db.DbSelect(@"
              SELECT
                disposition_code_name,
                disposition_code_longname,
                manager_only
              FROM
                queue.disposition_codes"
            );

            cmpDispositionComboBox.Items.Add("");

            foreach (QueryResultSetRecord r in dispositions)
            {
                cmpDispositionComboBox.Items.Add(new DispositionCodeComboBoxItem(r["disposition_code_longname"], r["disposition_code_name"], r.ToBoolean("manager_only")));
            }
        }

        public void SetLogger(IntellaQueueForm intellaQueueForm)
        {
            this.m_intellaQueueForm = intellaQueueForm;
        }

        public Panel GetDispositionCodesPanel() {
            return this.cmpDispositionPanel;
        }
        
        public void ResetDisposition_Full() {
            this.m_currentCallDispositionSet = false;

            if (Thread.CurrentThread != m_guiThread)
            {
                // Make sure we do the updates on the gui thread

                this.cmpDispositionComboBox.Invoke((MethodInvoker)delegate
                {
                    // Running on UI thread
                    this.cmpDispositionComboBox.SelectedIndex = 0;
                    ResetDisposition_Selection();
                });

            }
            else
            {
                this.cmpDispositionComboBox.SelectedIndex = 0;
                ResetDisposition_Selection();
            }
        }

        public void ResetDisposition_Selection()
        {
            cmpDispositionComboBox.BackColor = Color.White;
            cmpDispositionComboBox.ForeColor = Color.Black;
        }

        private void cmpStatusComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            var c = (ComboBox) sender;

            if (m_intellaQueueForm == null) {
                return;
            }
        }
        
        private void DispositionForm_Load(object sender, EventArgs e) {

        }

        private void CmpDispositionCode_CallAndCodePanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void CmpSaveUpdatePanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cmpHideShowButton_Click(object sender, EventArgs e)
        {

        }

        private void cmdSaveUpdateData_Click(object sender, EventArgs e)
        {
            Button b = (Button) sender;

            int selected_idx = cmpDispositionComboBox.SelectedIndex;
            if (selected_idx <= 0) {
                return;
            }

            DbHelper db                       = this.m_intellaQueueForm.GetDatabaseHandle();
            JsonHash last_screenpop_data      = this.m_intellaQueueForm.GetLastScreenPopData();

            // CurrentCallForm current_call_form = this.m_intellaQueueForm.GetCurrentCallForm();
            //QueryResultSet current_calls_data = current_call_form.GetCurrentCallData();
            //
            // CallQueue Call Data
            // current_calls_data -- Set Of
            //  channel
            //  call_state
            //  call_log_id
            //  call_segment_id
            //  case_number
            //  callerid_num
            //  callerid_name
            //  queue_longname

            //QueryResultSetRecord current_call_record = null;
            //
            //// Find the call that's not currently on hold
            //foreach (QueryResultSetRecord c in current_calls_data) {
            //    if (c["call_state"] != "TALKING")  {
            //        continue;
            //    }
            //
            //    // The one call that is TALKING is our ACTIVE CALL
            //    current_call_record = c;
            //    break;
            //}

            #region Example ScreenPop Data
            /*
              screenpop_data:  {
                channel: SIP/trunk-bob-00000006
                call_log_id: 9897
                call_segment_id: [null]
                callerid_num: 2123335515
                callerid_name: FooBarBaz

                joined_when: 2019-09-05 16:04:18.96625-04
                queue_call_log_id: 9897
                department_name: 
                picked_up_when: 2019-09-05 16:04:25.88946-04
                userData: [null]
                waiting_seconds: 6.92321
                on_call_with_agent_channel: CallQueue@INTERNAL
                call_uniqueid: 1567713858.15
                on_call_with_agent_device: SIP/1010
                queue_name: carpet_customer_service

              }
              event_additional_data_json:  {
                CallType: QueueCall
                QueueCallerChannel: SIP/trunk-bob-00000006
                CallLogID: 9897
                CallSegmentID: [null]
                QueueCaseNumber: [null]

                QueueDepartmentName: 
                Privilege: user,all
                AgentChannel: CallQueue@INTERNAL
                QueueCalleridName: Carpet
                QueueCallerUniqueID: 1567713858.15
                AgentUniqueID: 1567713858.15
                Event: IntellaQueueDialAgent
                AgentSessionID: 229
                UserEvent: 1
                QueueJoinedWhen: 2019-09-05 16:04:18.96625-04
                AgentFirstName: Test
                QueueDepartmentNum: [null]
                QueueCallLogID: 9897
                AgentOverflowLevel: 0

                QueueName: carpet_customer_service
                QueueCalleridNum: 5515
                AgentLastName: Agent
                AgentNumber: 8000
                __RecievedTime: 09/05/19 16:04:23.45231
              }             
            */
            #endregion

            if (last_screenpop_data == null) {
                this.m_intellaQueueForm.SetStatusBarMessage(Color.Red, "Disposition can only be set when a call has been established.");
                return;
            }

            if (m_dispositionUpdating) {
                this.m_intellaQueueForm.SetStatusBarMessage(Color.Orange, "Disposition update in progress, please wait a moment.");
                return;
            }

            m_dispositionUpdating = true;
            m_dispositionLastSet = DateTime.Now;

            b.Text = "Updating... ";
            b.ForeColor = Color.Green;
            Application.DoEvents();

            JsonHash event_additional_data_json = last_screenpop_data.GetHash("event_additional_data_json");
            JsonHash screenpop_data             = last_screenpop_data.GetHash("screenpop_data");

            string call_type       = event_additional_data_json.GetString("CallType");
            string call_log_id     = screenpop_data.GetString("call_log_id");
            string call_segment_id = screenpop_data.GetString("call_segment_id");
            string callerid_num    = screenpop_data.GetString("callerid_num");

            string selected_item = cmpDispositionComboBox.Items[selected_idx].ToString();

            QueryResultSetRecord result;
            JsonHash result_json;

            IntellaQueueForm.MQD("[DispositionSet] Set Disposition Requested -- CallLogID: {0}, CallSegmentID: {1}, CallerID: {2}, CallType: {3}, Disposition: {4}", call_log_id, call_segment_id, callerid_num, call_type, selected_item);

            // spawn thread
            Utils.SpawnBackgroundThread(
                delegate(object w_sender, System.ComponentModel.DoWorkEventArgs e_args)
                {
                    QueryResultSetRecord update_disp;

                    try
                    {
                        update_disp =
                            db.DbSelectSingleRow(
                                "SELECT queue.api_agent_disposition_set(?,?,?,?,?) as result",
                                call_log_id, call_segment_id, callerid_num, call_type, selected_item
                            );
                    }
                    catch (Exception ex) { 
                        IntellaQueueForm.MQD("[DispositionSet] DB Exception: {0}", ex.ToString());

                        this.DispositionComboBox_SetErrorState();

                        update_disp = new QueryResultSetRecord();

                        update_disp["result"] = @"
                          {
                            ""success"": 0,
                            ""message"": ""Exception has occurred""
                          }
                        ";
                    }

                    m_dispositionUpdateTimeoutTimer.Stop();

                    if (!m_dispositionUpdating) {
                        // This update took too long, we hit the timeout.. Failure was already logged
                        IntellaQueueForm.MQD("[DispositionSet] !!! Set Disposition Finished after timeout -- CallLogID: {0}, CallSegmentID: {1}, CallerID: {2}, CallType: {3}, Disposition: {4}", call_log_id, call_segment_id, callerid_num, call_type, selected_item);
                    }

                    // Complete! 
                    result_json = new JsonHash(update_disp["result"]);

                    this.DispositionSaveButton_RestoreDefaults();

                    if (result_json.GetBool("success") == true)
                    {
                        IntellaQueueForm.MQD("[DispositionSet] Set Disposition Success -- CallLogID: {0}, CallSegmentID: {1}, CallerID: {2}, CallType: {3}, Disposition: {4}", call_log_id, call_segment_id, callerid_num, call_type, selected_item);
                        m_intellaQueueForm.SetStatusBarMessage(Color.Green, "Disposition update complete");
                        this.DispositionComboBox_SetSuccessState();
                    }
                    else {
                        IntellaQueueForm.MQD("[DispositionSet] !!! ERROR Set Disposition Failed -- CallLogID: {0}, CallSegmentID: {1}, CallerID: {2}, CallType: {3}, Disposition: {4}", call_log_id, call_segment_id, callerid_num, call_type, selected_item);
                        IntellaQueueForm.MQD("[DispositionSet] !!! ERROR: {0}", result_json.GetString("message"));

                        m_intellaQueueForm.SetStatusBarMessage(Color.Red, "Disposition update failed -- " + result_json.GetString("message"));
                        this.DispositionComboBox_SetErrorState();
                    }

                    m_dispositionUpdating = false;
                }
            );

            m_dispositionUpdateTimeoutTimer.Start();
        }

        ///
        /// GUI Helpers and Bindings
        ///
        private void DispositionComboBox_SetSuccessState()
        {
            cmpDispositionComboBox.Invoke((MethodInvoker) delegate ()  {
                cmpDispositionComboBox.BackColor = Color.Green;
                cmpDispositionComboBox.ForeColor = Color.White;
            });
        }

        private void DispositionComboBox_SetErrorState()
        {
            this.cmpSaveUpdateData.Invoke((MethodInvoker) delegate() { 
                cmpDispositionComboBox.BackColor = Color.Red;
                cmpDispositionComboBox.ForeColor = Color.White;
            });
        }

        private void DispositionSaveButton_RestoreDefaults()
        {
            this.cmpSaveUpdateData.Invoke((MethodInvoker) delegate()
            {
                this.cmpSaveUpdateData.ForeColor = m_button_save_color;
                this.cmpSaveUpdateData.Text = m_button_save_text;
            });
        }

        private void cmpDispositionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetDisposition_Selection();
        }

        private void cmpDispositionHistory_Click(object sender, EventArgs e) {
            this.m_cmpDispositionCodesHistoryForm.Show();
        }

        // Don't Dispose when the X is clicked.
        private void DispositionCodesForm_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true; // Don't Dispose
            this.Hide();
        }
    }

    class DispositionCodeComboBoxItem
    {
        public string Name;
        public string Value;
        public Boolean ManagerOnly;

        public DispositionCodeComboBoxItem(string name, string value, Boolean managerOnly)
        {
            Name = name;
            Value = value;
            ManagerOnly = managerOnly;
        }

        public override string ToString()
        {
            // Generates the text shown in the combo box
            return Name;
        }
    }
}
