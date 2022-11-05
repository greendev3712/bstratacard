using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using intellaQueue;
using Lib;
using System.Threading;

namespace QueueLib {
    public partial class CurrentCallForm : Form {
        private Thread m_guiThread;
        private IntellaQueueForm m_intellaQueueForm;

        private QueryResultSet m_currentCallsData;
        private string m_currentCallString;

        private void ThisFormConstruct() {
            m_guiThread = Thread.CurrentThread;
            InitializeComponent();
        }

        public CurrentCallForm() {
            ThisFormConstruct();
        }

        // subscribedQueues is the grid data for the queues
        public CurrentCallForm(SortedList<string, Hashtable> subscribedQueues, bool multiQueueStatusMode) {
            ThisFormConstruct();
        }
        
        public void SetIntellaQueueForm(IntellaQueueForm intellaQueueForm) {
            this.m_intellaQueueForm = intellaQueueForm;
        }

        public Panel GetMainPanel() {
            return this.cmpCallMainPanel;
        }

        /// <summary>
        ///  Warning: THIS FORM Only handles a SINGLE CALL (the first one)
        /// </summary>
        /// <param name="currentCallData"></param>
        public void SetCurrentCallData(QueryResultSet currentsCallData)
        {
            // Set Of
            //  channel
            //  call_state
            //  call_log_id
            //  call_segment_id
            //  case_number
            //  callerid_num
            //  callerid_name
            //  queue_longname

            this.m_currentCallsData = currentsCallData;
        }

        public QueryResultSet GetCurrentCallData()
        {
            if (this.m_currentCallsData == null) { 
                return new QueryResultSet(); // Empty result
            }


            return this.m_currentCallsData;
        }

        public void SetCurrentCallString(string newString)
        {
            if (this.cmpCurrentCall.Text == newString) { 
                // Don't bother updating if we have the same string.  Avoid extra gui operations
                // FIXME:  Our caller shouldn't be calling us if we're the same string!
                return;
            }

            if (Thread.CurrentThread != m_guiThread) { 
                // Make sure we do the updates on the gui thread

                this.cmpCurrentCall.Invoke((MethodInvoker) delegate { 
                    // Running on UI thread
                    this.cmpCurrentCall.Text = newString;
                });

            }
            else { 
                this.cmpCurrentCall.Text = newString;
            }

            m_currentCallString = newString;
        }

        public string GetCurrentCallString()
        {
            return this.m_currentCallString;
        }
    }
}
