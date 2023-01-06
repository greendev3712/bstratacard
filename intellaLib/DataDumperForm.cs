using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Lib {
    public partial class DataDumperForm : Form {
        private int m_maxScrollBack = 100000; // TODO: Currently number of characters (but we should be doing # of lines)

        private Boolean m_pause = false;
        private string m_pauseBuffer = "";

        private Thread m_guiThread;
        private bool m_dumperNullOutput = true;

        // This is used for periodic log-uploaders to upload just the newest logs since the last time we got the logs
        private List<string> m_recentLog = new List<string>();
        private DateTime m_recentLogLastUpdated = DateTime.Now;

        // Note: Make sure to construct from the gui thread
        public DataDumperForm(Boolean showByDefault = true) {
            m_guiThread = Thread.CurrentThread;
            InitializeComponent();

            if (showByDefault) { 
                this.Show();
            }
        }

        public DataDumperForm(string nameOfObject, object dataItemToDump) {
            InitializeComponent();

            Dumper(nameOfObject, dataItemToDump);
            this.Show();
        }

        public void ThreadSafeShow() {
            if (Thread.CurrentThread != m_guiThread) {
                // Make sure we do the updates on the gui thread

                MessageBox.Show("Cross-Thread Show");

                this.Invoke((MethodInvoker) delegate {
                   // Running on UI thread
                   base.Show();
                });

                return;
            }

            base.Show();
        }

        public void SetTitle(string titleText) {
            this.Text = titleText;
        }

        public void SetDumperNullOutput(bool value) {
            this.m_dumperNullOutput = value;
        }

        public Boolean IsPaused() {
            return this.m_pause;
        }

        public string GetBacklogText(int backlogLines) {
            // FIXME: backlogLines not yet implemented
            return this.cmpTextBox.Text;
        }

        public string D(string line, params string[] argsRest) {
            string log_line = buildLogLine(line, argsRest);

            if (!this.Visible) {
                doAppendText(log_line);
                return log_line;
            }

            if (Thread.CurrentThread != m_guiThread) {
                // Make sure we do the updates on the gui thread

                this.cmpTextBox.Invoke((MethodInvoker) delegate {
                    // Running on UI thread
                    doAppendText(log_line);
                });

                return log_line;
            }

            doAppendText(log_line);

            return log_line;
        }

        /// <summary>
        /// Build a final log line with the timestamp and wrap it in String.Format()
        /// Example: buildLogLine("Something {0} {1} {2}", foo, bar, baz, ...)
        /// </summary>
        /// <param name="line"></param>
        /// <param name="argsRest"></param>
        /// <returns></returns>
        //
        //
        private string buildLogLine(string line, params string[] argsRest) {
            string string_format_result = line;

            if (argsRest.Length != 0) {
                try {
                    string_format_result = String.Format(line, argsRest);
                }
                catch (Exception ex) {
                    if (Debugger.IsAttached) {
                        throw ex;
                    }
                    else {
                        doAppendText(String.Format("DataDumperForm::buildLogLine() -- Failed to process String.Format({0}, {1})", line, argsRest.ToString()));
                    }
                }
            }

            // We're adding either the String.Format or the original line 
            string new_line = String.Format("[{0}] {1}\r\n", Utils.DateTimePrettyString(DateTime.Now), string_format_result);
            return new_line;
        }

        private void doAppendText(string new_line) {
            lock (m_recentLog) {
                m_recentLog.Add(new_line);
            }

            if (!this.Visible) {
                if (m_pauseBuffer.Length > m_maxScrollBack) {
                    m_pauseBuffer = m_pauseBuffer.Substring(m_pauseBuffer.Length - m_maxScrollBack);
                }

                m_pauseBuffer += new_line;
                return;
            }

            if (this.m_pause) {
                // Store new line in memory until unpaused
                m_pauseBuffer += new_line;
                return;
            }

            if (m_pauseBuffer.Length > 0) {
                // We're here if we're unpaused, but were previously paused and built up a buffer.
                new_line = m_pauseBuffer + new_line;
                m_pauseBuffer = "";
            }
            
            if (this.cmpTextBox.Text.Length > m_maxScrollBack) {
                this.cmpTextBox.Text = this.cmpTextBox.Text.Substring(this.cmpTextBox.Text.Length - m_maxScrollBack);
            }

            this.cmpTextBox.AppendText(new_line);

            return;
        }

        public string GetLatestBacklogText() {
            StringBuilder recent_log_string = new StringBuilder();

            int recent_log_lines = 0;

            lock (m_recentLog) {
                foreach (string line in m_recentLog) {
                    recent_log_string.Append(line);
                }

                recent_log_lines = m_recentLog.Count;

                m_recentLog.Clear();
                m_recentLogLastUpdated = DateTime.Now;
            }

            return recent_log_string.ToString();
        }

        public void Dumper(string nameOfObject, object dataItemToDump) {
            if (dataItemToDump == null) {
                if (m_dumperNullOutput) {
                    D("Item is Null: " + nameOfObject);
                }
                else {
                    D(nameOfObject);
                }

                return;
            }

            if (dataItemToDump is List<OrderedDictionary>)
            {
                D("Item: " + nameOfObject + " " + dataItemToDump.GetType() + "\r\n" + StringifyListOfOrderedDictionary((List<OrderedDictionary>) dataItemToDump));
            }
            else if (dataItemToDump is Dictionary<string, QueryResultSet>)
            {
                D("Item: " + nameOfObject + " " + dataItemToDump.GetType() + "\r\n" + StringifyDictionaryOfQueryResultSet((Dictionary<string, QueryResultSet>) dataItemToDump));
            }
            else if (dataItemToDump is QueryResultSet)
            {
                D("Item: " + nameOfObject + " " + dataItemToDump.GetType() + "\r\n" + StringifyQueryResultSet((QueryResultSet) dataItemToDump));
            }
            else if (dataItemToDump is QueryResultSetRecord)
            {
                D("Item: " + nameOfObject + " " + dataItemToDump.GetType() + "\r\n" + StringifyQueryResultSetRecord((QueryResultSetRecord) dataItemToDump));
            }
            else if (dataItemToDump is JsonHash)
            {
                D("Item: " + nameOfObject + " " + dataItemToDump.GetType() + "\r\n" + StringifyJsonHash((JsonHash) dataItemToDump));
            }
            else if (dataItemToDump is StackTrace)
            {
                D("Item: " + nameOfObject + " " + dataItemToDump.GetType() + "\r\n" + StringifyStackTrace((StackTrace) dataItemToDump));
            }
            else if (dataItemToDump is Dictionary<string, List<OrderedDictionary>>)
            {
                // Backwards compat

                Dictionary<string, List<OrderedDictionary>> dataItemToDumpDirect = (Dictionary<string, List<OrderedDictionary>>) dataItemToDump;
                Dictionary<string, QueryResultSet> ds_qr = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(dataItemToDumpDirect);

                Dumper(nameOfObject, ds_qr);
                return;
            }
            else
            {
                D("Item: " + nameOfObject + " " + dataItemToDump.GetType() + " [UNSUPPORTED TYPE - CANNOT DUMP]\r\n");
            }
        }

        public string StringifyStackTrace(StackTrace thingee) {
            string result = "";

            foreach (StackFrame s in thingee.GetFrames()) {
                result += String.Format("{0}:{1} {2}()", s.GetFileName(), s.GetFileLineNumber(), s.GetMethod().ToString());
            }

            return result;
        }

        public string StringifyJsonHash(JsonHash thingee) {
            return thingee.ToString();
        }

        public string StringifyListOfOrderedDictionary(List<OrderedDictionary> thingee) {
            string result = "";
            int pos = 0;

            if (thingee.Count == 0) {
                return "<Empty>";
            }

            int longest_key = 0;
            ArrayList sorted_keys = new ArrayList();
            OrderedDictionary first_item = thingee[0];
            foreach (string column_name in first_item.Keys) {
                sorted_keys.Add(column_name);

                if (column_name.Length > longest_key) {
                    longest_key = column_name.Length;
                }
            } 

            sorted_keys.Sort();

            string longest_key_format = "{0,-" + longest_key + "}";

            foreach (OrderedDictionary row in thingee) {
                result += pos++ + " {\r\n";

                foreach (string column_name in sorted_keys) {
                    result += String.Format("  " + longest_key_format + " : {1}", column_name, row[column_name] + "\r\n");
                }

                result += "}";
            }

            return result;
        }

        public string StringifyDictionaryOfQueryResultSet(Dictionary<string, QueryResultSet> thingee)
        {
            string result = "";

            if (thingee.Count == 0) {
                return "<Empty>";
            }

            foreach (KeyValuePair<string, QueryResultSet> row in thingee)
            {
                result += row.Key + " : {\r\n" + StringifyQueryResultSet(row.Value, 1) + "}\r\n";
            }

            return result;
        }
        public string StringifyQueryResultSet(QueryResultSet thingee, int indentLevel = 0) {
            string result = "";
            int pos = 0;
            string indent = new String(' ', indentLevel * 4);

            if (thingee.Count == 0) {
                return "<Empty>";
            }

            QueryResultSetRecord first_item = thingee[0];
            ArrayList sorted_keys = first_item.KeysToArrayList();
            sorted_keys.Sort();

            int longest_key_length = Utils.GetLongestTextFromArrayListOfString(sorted_keys).Length;
            string longest_key_format = "{0,-" + longest_key_length + "}";

            foreach (QueryResultSetRecord row in thingee) {
                result += indent + pos++ + " {\r\n";

                foreach (string column_name in sorted_keys) {
                    result += indent + String.Format("  " + longest_key_format + " : {1}", column_name, row[column_name] + "\r\n");
                }

                result += indent + "}\r\n";
            }

            return result;
        }

        public string StringifyQueryResultSetRecord(QueryResultSetRecord thingee) {
            string result = "";
            int pos = 0;

            if (thingee.Count == 0) {
                return "<Empty>";
            }

            ArrayList sorted_keys = thingee.KeysToArrayList();
            sorted_keys.Sort();

            int longest_key = Utils.GetLongestTextFromArrayListOfString(sorted_keys).Length;
            string longest_key_format = "{0,-" + longest_key + "}";

            result += pos++ + " {\r\n";

            foreach (string column_name in sorted_keys) {
                result += String.Format("  " + longest_key_format + " : {1}", column_name, thingee[column_name] + "\r\n");
            }

            result += "}";

            return result;
        }

        /////////////////
        // Event Handlers
        /////////////////

        private void DataDumperForm_Load(object sender, EventArgs e) {
            Screen screen = Screen.FromControl(this);
            int width = screen.Bounds.Width - 400;

            this.Width = width;
        }

        private void DataDumperForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void cmpCloseButton_Click(object sender, EventArgs e) {
            this.Hide();
        }

        private void cmpCopyToClipboardButton_Click(object sender, EventArgs e) {
            Clipboard.SetData(DataFormats.Text, this.cmpTextBox.Text);
        }

        private void cmpPauseButton_Click(object sender, EventArgs e) {
            Button pause_button = (Button) sender;

            this.m_pause = !this.m_pause;

            if (this.m_pause) {
                pause_button.Text = "Paused (Click to Un-Pause)";
            }
            else {
                pause_button.Text = "Pause";
            }
        }

        private void cmpTextBox_VisibleChanged(object sender, EventArgs e) {
            if (this.Visible == false) {
                return;
            }

            this.D("Debug Window Opened");
        }
    }
}
