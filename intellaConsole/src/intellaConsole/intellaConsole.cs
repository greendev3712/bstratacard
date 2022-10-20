///////////////////////////////////////////////////////////////////////////////
// The Intellasoft Operator Console
//
// BUGS: 
//   Changing database settings via right click admin menu makes bad stuff happen
//
// TODO:
//   Call phone and then park doesn't work right
//   Park Icon needs to be flipped from white to black when the park row isn't currently selected
//   Conference (3-way)
//   GrabNextQueueMember via double click on caller
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using QueueLib;
using Lib;
using System.Drawing.Drawing2D;

namespace intellaConsole {
    public partial class intellaConsole : Form {
        ///////////////////////////////////////////////////////////////////////

        public static string m_deploymentVersion = "CurrentDev";

        private static string m_registryParent = "intellaConsole";

        private static ToolbarConfiguration m_toolbarConfig;
        private static ToolbarServerConnection m_tsc;
        private static DbHelper m_db;

        Thread m_guiThread = Thread.CurrentThread;
        BackgroundWorker m_dbRefreshWorker;

        private System.Windows.Forms.Timer m_cmpTimerUpdateDisplay  = null;
        private System.Windows.Forms.Timer m_cmpTimerDisplayMessage = null;

        public bool m_firstUpdate = true;

        public DateTime m_reconnectionLastAttempt;
        public int m_reconnectionAttempts = 0;

        public bool m_lastPbxOperationWasFailure      = false;
        public DateTime m_lastPbxOperation            = new DateTime(0);
        private int m_secondsAllowedBetweenOperations = 2;

        private Boolean m_transferMode                = false;
        private Boolean m_transferToVoicemailMode     = false;
        private Boolean m_lightStatus                 = false;

        private List<OrderedDictionary> m_queueCallers     = new List<OrderedDictionary>(); // Current callers waiting in queue, and parked callers
        private List<OrderedDictionary> m_extensionCallers = new List<OrderedDictionary>(); // Current callers to/from the agent phone
        private Hashtable m_extensionPresence;
        private string m_CurrentActiveChannelName   = null;
        private string m_CurrentSelectedChannelName = null;

        // Dynamically created gui widgets
        private DirectoryViewerPanel DirectoryForm;
        private SpeedDialPanel SpeedForm;
        private MessagingPanel MessagingForm;

        public Boolean m_localPhoneAutoAnswer = false;
        public Boolean m_showLogWindow = false;
        public LogWindow m_log         = new LogWindow();

        private string[] m_bitmaps_toload = new string[] {
            "icon_park.png",
            "icon_email.png",
            "icon_blank_transparent.png",
        };

        private Dictionary<string, System.Drawing.Bitmap> m_bitmaps = new Dictionary<string, System.Drawing.Bitmap>();
        private string m_operatorDevice;
        private string m_operatorExten;
        private string m_operatorQueue;

        ///////////////////////////////////////////////////////////////////////

        public intellaConsole() {
            try {
                InitializeComponent();
            }
            catch (Exception ex) {
                if (Debugger.IsAttached) {
                    throw ex;
                }

                ErrorTextBox.Show(ex.ToString());
            }

            if (this.m_showLogWindow) {
                this.m_log.Show();
            }

            string main_resource_prefix = "intellaConsole.Resources.";
            System.Reflection.Assembly our_assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string[] our_resource_names = our_assembly.GetManifestResourceNames();

            foreach (string bitmap_name in m_bitmaps_toload) {
                string fullname = main_resource_prefix + bitmap_name;
                if (!our_resource_names.Contains(fullname)) {
                    throw new Exception("Resource Does not exist in project: " + fullname);
                }

                System.IO.Stream image_stream = our_assembly.GetManifestResourceStream(fullname);
                this.m_bitmaps.Add(bitmap_name, new System.Drawing.Bitmap(image_stream));
            }
        }

        private void intellaConsole_Load(object sender, EventArgs e) {
            if (Debugger.IsAttached) {
                intellaConsole_LoadReal(sender, e);
                return;
            }

            try
            {
              intellaConsole_LoadReal(sender, e);
            }
            catch (Exception ex) {
                ErrorTextBox.Show(ex.ToString());
            }
        }

        public string getOperatorExten() {
            return this.m_operatorExten;
        }

        public string getOperatorDevice() {
            return this.m_operatorDevice;
        }

        public string getOperatorQueue() {
            return this.m_operatorQueue;
        }

        private void drawGradientBackground() {
            Bitmap background = new Bitmap(this.Width, this.Height);
            Graphics g = System.Drawing.Graphics.FromImage(background);

            // green 00926b

            LinearGradientBrush br = new LinearGradientBrush(
                new Rectangle(0, 0, background.Width, 1),
                Helper.webColorToColorObj("007a92"),
                Color.Black,
                0,
                true);

            g.FillRectangle(br, new Rectangle(0, 0, background.Width, background.Height));

            this.BackgroundImage = background;
        }

        private void intellaConsole_LoadReal(object sender, EventArgs e) {
            drawGradientBackground();

            try {
                m_deploymentVersion = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch (Exception ex) {
                ex.ToString(); // Avoid warning
            }

            this.cmpPublishVersionLabel.Text = "Version: " + m_deploymentVersion;

            //////////////////////////
            // Copy widgets from Forms

            DirectoryForm = new DirectoryViewerPanel();
            DirectoryForm.setIntellaConsoleForm(this);
            Panel directory_panel = DirectoryForm.getBodyPanel();
            directory_panel.Location = new Point(0, 0);
            this.panelDirectoryAndSpeedDial.Controls.Add(directory_panel);

            SpeedForm = new SpeedDialPanel();
            SpeedForm.setIntellaConsoleForm(this);
            Panel speed_panel = SpeedForm.getBodyPanel();
            speed_panel.Location = new Point(0, 0);
            this.panelDirectoryAndSpeedDial.Controls.Add(speed_panel);

            MessagingForm = new MessagingPanel();
            MessagingForm.setIntellaConsoleForm(this);
            Panel message_panel = MessagingForm.getBodyPanel();
            message_panel.Location = new Point(0, 0);
            this.panelDirectoryAndSpeedDial.Controls.Add(message_panel);

            // End copy widgets
            ////////////////////

            ///////////////////////
            // Dynamic Widget Stuff

            // Replace TextBoxCallControl with a WaterMarked TextBox
            TextBoxWaterMarked tbwm = new TextBoxWaterMarked();
            tbwm.WaterMarkText = "Enter Phone Number";
            tbwm.Location = this.textBoxCallControl.Location;
            tbwm.Text = this.textBoxCallControl.Text;
            tbwm.Size = this.textBoxCallControl.Size;
            tbwm.DoubleClick += this.textBoxCallControl_DoubleClick;
            tbwm.KeyPress += this.textBoxCallControl_KeyPress;
            this.textBoxCallControl.Dispose();
            this.textBoxCallControl = tbwm;
            this.panelCallControl.Controls.Add(tbwm);

            // End Dynamic Widget Stuff
            ///////////////////////////

            // TODO FIXME: We should clean this up... defaults for main TSC and stuff like that (we shouldn't need to explicitly set if there's only one)
            m_toolbarConfig = new ToolbarConfiguration();
            m_db = new DbHelper(handleDatabaseError);
            m_db.SetPrepared(true);

            m_tsc = new ToolbarServerConnection("MAIN", "Main Server", m_toolbarConfig);
            m_tsc.m_db = m_db;

            m_toolbarConfig.SetMainTSC(m_tsc);

            lock (m_db) {
                ToolBarHelper.SetToolbarServerConnection(m_tsc);
                ToolBarHelper.SetProgramErrorCallback(ProgramError);

                // this also connects to the db
                ToolBarHelper.SetRegistryParent(m_registryParent);
                ToolBarHelper.DatabaseSetupCheck(ref m_db, DatabaseSuccess, StartupConfigurationFailure);
            }

            showSpeedDials();
        }

        private void DatabaseSuccess(string message) {
            m_reconnectionLastAttempt = DateTime.Now;
            DirectoryForm.setDbHelper(m_db);
            SpeedForm.setDbHelper(m_db);
            MessagingForm.setDbHelper(m_db);

            new OperatorLoginWindow(this, loginCheckCallbackFn, loginSuccessCallbackFn, loginFailureCallbackFn);
        }

        private bool loginCheckCallbackFn(OrderedDictionary loginEntry) {
            string exten = (string)loginEntry["exten"];

            QueryResultSetRecord user_login_result = m_db.DbSelectSingleRow(@"
                  SELECT
                    ou.user_id,
                    COALESCE(e.override_extension, e.extension) as dialed_exten,
                    'SIP/' || ou.username as operator_device -- FIXME
                  FROM
                      operator_console.user ou
                      JOIN asterisk.extensions e ON (ou.username = e.extension)
                  WHERE
                      ou.username = {0}", exten
            );

            if (this.m_lastPbxOperationWasFailure == true) {
                MessageBox.Show("Internal Error -- Could not query Operator Console Users -- Does this system have the Operator Console Module loaded?");
                return false;
            }

            if (user_login_result.Exists("user_id")) {
                loginEntry["user_id"]         = user_login_result["user_id"];
                loginEntry["operator_device"] = user_login_result["operator_device"];
                loginEntry["dialed_exten"]    = user_login_result["dialed_exten"]; 
                return true;
            }

            return false;
        }

        private void loginSuccessCallbackFn(OrderedDictionary loginEntry) {
            this.textBoxOperatorExten.Text = (string) loginEntry["dialed_exten"];
            this.m_operatorExten           = (string) loginEntry["exten"];
            this.m_operatorQueue           = (string) loginEntry["queue"];
            this.m_operatorDevice          = (string) loginEntry["operator_device"];

            doInit();
        }

        private void loginFailureCallbackFn(OrderedDictionary loginEntry) {
            Application.Exit();
        }

        public void handleDbReconnection() {
            Console.WriteLine("Reconnecting to server...");

            TimeSpan since_last_attempt = DateTime.Now.Subtract(m_reconnectionLastAttempt);

            if (since_last_attempt.Minutes <= 5) {
                m_reconnectionAttempts++;  // Only count reconnects if they are happening very often
            }
            else {
                m_reconnectionAttempts = 0; // Reset if we've had a nice "long" stretch of connectivity
            }

            if (m_reconnectionAttempts > 5) {
                ErrorTextBox.Show("Not connected to database.  Maxiumum reconnection attempts reached.");
                ProgramError("Not connected to database.  Maxiumum reconnection attempts reached.");
                return;
            }

            killDatabaseConnection_ReconnectAndResume();
        }

        public void killDatabaseConnection_ReconnectAndResume() {
            m_reconnectionLastAttempt = DateTime.Now;

            StopRefreshTimer();
            m_dbRefreshWorker.CancelAsync();

            try
            {
                m_db.connect();
                StartRefreshTimer();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public bool AdminPasswordValidate(string password) {
            if ((password == "intelladmin")) {
                return true;
            }

            return false;
        }

        public void ShowAdminSettings() {
            StopRefreshTimer();
            this.TopMost = false;

            FormAdminSettings fas = new FormAdminSettings(m_registryParent);
            fas.SetSuccessCallback(ShowAdminSettingsSuccess);
            fas.ShowDialog(this);
        }

        private void ShowAdminSettingsSuccess(string message) {
            doInit();
            StartRefreshTimer();
        }

        private void UpdateInternalLiveDataStores(object sender, DoWorkEventArgs e) {
            OperatorConsoleLiveData live_data = new OperatorConsoleLiveData();

            ///////////////////////////////////////////////////////////////////
            // Calls waiting in some fashion, either waiting in queue or parked

            if (m_dbRefreshWorker.CancellationPending) { e.Cancel = true; return; }

            // TODO: WARNING -- Enbedded ? in query.  Will not work with future ?-style bind placeholders
            string query = @"
                (
                    SELECT
                        1 as the_order,
                        c.waiting_seconds::text,
                        c.callerid_num::text,
                        c.callerid_name::text,
                        c.channel::text,
                        'CALLER'::text as call_type,
                        null::text as park_pos
                    FROM
                        live_queue.v_callers c
                    WHERE
                        queue_name = {0}
                        AND picked_up_when IS NULL
                )
                UNION
                (
                    SELECT
                        2 as the_order,
                        current_hold_duration_seconds::text as waiting_seconds,
                        c.from_callerid_name::text as callerid_name,
                        c.from_callerid_num::text as callerid_num,
                        c.from_channel::text as channel,
                        'PARK'::text as call_type,
                        substring(to_component_opts, 'parkingPos=(\d+),?') as park_pos
                    FROM
                        live_core.v_calls c
                    WHERE
                        to_component = 'Park'
                )
                ORDER BY the_order, waiting_seconds DESC";

             m_db.dbQueryWithParams(query, live_data.QueueCallers, getOperatorQueue());

            /////////////////////////
            // Calls to our extension

            if (m_dbRefreshWorker.CancellationPending) { e.Cancel = true; return; }

            m_db.dbQueryWithParams(@"
              SELECT
                *
              FROM
                live_core.v_calls 
              WHERE 
                from_channel LIKE 'SIP/' || {0} || '-%'
                OR to_channel LIKE 'SIP/' || {1} || '-%'
              ORDER BY 
                total_hold_duration_seconds ASC",
            live_data.ExtensionCalls, getOperatorExten(), getOperatorExten());

            /////////////////////////////////////////////////////////////////
            // Device state, to be joined up later with the directory listing

            List<OrderedDictionary> device_state_result = new List<OrderedDictionary>();

            if (m_dbRefreshWorker.CancellationPending) { e.Cancel = true; return; }

            m_db.dbQueryWithParams(@"
              SELECT
                *
              FROM
                live_core.pbx_get_blf()",
            device_state_result); // TODO: Security check... make sure we query only blf we're supposed to get


            foreach (OrderedDictionary r in device_state_result) {
                string device = (string) r["device"];
                live_data.PresenseInfo[device] = r;
            }

            // Done!  We can set our data so the thread finish callback can get at it
            if (m_dbRefreshWorker.CancellationPending) { e.Cancel = true; return; }
            e.Result = live_data;
        }

        private void UpdateDisplay(Object myObject, EventArgs myEventArgs) {
            this.m_cmpTimerUpdateDisplay.Stop();
            toggleStatusLight();

            if (this.m_firstUpdate) {
                this.Width += 1;
                this.m_firstUpdate = false;
            }

            // Are we still connected?
            List<string> arguments = new List<string>();
            List<OrderedDictionary> resultData = new List<OrderedDictionary>();

            int result;
            result = m_db.callSqlFunction("NOW", arguments, resultData);

            if (result != 0) {
                displayMessageBarMessage(Color.Yellow, "Reconnecting to server...");
                handleDbReconnection();
                return;
            }

            OperatorConsoleLiveData live_data = new OperatorConsoleLiveData();
            
            m_dbRefreshWorker = new BackgroundWorker();
            m_dbRefreshWorker.WorkerSupportsCancellation = true;
            m_dbRefreshWorker.DoWork             += new DoWorkEventHandler(dbRefreshWorker_DoWork);
            m_dbRefreshWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(dbRefreshWorker_RunWorkerCompleted);
            m_dbRefreshWorker.ProgressChanged    += new ProgressChangedEventHandler(dbRefreshWorker_ProgressChanged);
            
            try {
                m_dbRefreshWorker.RunWorkerAsync(live_data);
            }
            catch (TargetInvocationException ex) {
                ErrorTextBox.Show("Threading error: creating new thread worker: target invocation exception: \n" + ex.Message + "\n trace:" + ex.StackTrace + "\n");
                if (ex.InnerException != null)
                    ErrorTextBox.Show("Threading error: creating new thread worker: target invocation inner exception: \n" + ex.InnerException.Message + "\n trace:" + ex.InnerException.StackTrace + "\n");
            }
            catch (Exception ex) {
                ErrorTextBox.Show("Threading error: creating new thread worker: exception: \n" + ex.Message + "\n trace:" + ex.StackTrace + "\n");
                if (ex.InnerException != null)
                    ErrorTextBox.Show("Threading error: creating new thread worker: inner exception: \n" + ex.InnerException.Message + "\n trace:" + ex.InnerException.StackTrace + "\n");
            }
        }

        void dbRefreshWorker_DoWork(object sender, DoWorkEventArgs e) {
            UpdateInternalLiveDataStores(sender, e);
        }

        void dbRefreshWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            OperatorConsoleLiveData live_data = (OperatorConsoleLiveData) e.Result;

            this.m_queueCallers      = live_data.QueueCallers;
            this.m_extensionCallers  = live_data.ExtensionCalls;
            this.m_extensionPresence = live_data.PresenseInfo;

            UpdateDisplay_Real();

            this.m_cmpTimerUpdateDisplay.Start();
        }

        void dbRefreshWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            throw new NotImplementedException();
        }

        private void UpdateDisplay_Real() {
            List<OrderedDictionary> queue_callers = this.m_queueCallers;

            /////////////////////////////
            // Update Queue Callers List

            // save selection

            this.dataGridViewCallers.Rows.Clear();

            Image icon_wait = Properties.Resources.stopWatch;
            Image icon_park = Properties.Resources.icon_park_black;
            Image icon_current_status;

            foreach (OrderedDictionary r in queue_callers) {

                try {
                    int seconds = Int32.Parse((string)r["waiting_seconds"]);
                    String formatted_time = string.Format("{0}:{1:00}", (seconds / 60) % 60, seconds % 60);

                    icon_current_status = ((string)(string)r["call_type"] == "CALLER") ? icon_wait : icon_park;

                    this.dataGridViewCallers.Rows.Add((string)r["callerid_num"] + "\n" + (string)r["callerid_name"], icon_current_status, formatted_time);
                }
                catch (Exception ex) {
                    ex.ToString(); // Avoid Warning
                }
            }

            //////////////////////////////
            // Update Extension Calls List

            List<OrderedDictionary> extension_callers = this.m_extensionCallers;

            // Preserve selection
            int selected_index = -1;
            foreach (DataGridViewRow r in this.dataGridViewCurrentCalls.Rows) {
                if (r.Selected) { selected_index = r.Index; }
            }

            this.dataGridViewCurrentCalls.Rows.Clear();
            this.m_CurrentActiveChannelName = null;

            Font dgr_font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            foreach (OrderedDictionary r in extension_callers) {
                DataGridViewRow dgr = new DataGridViewRow();
                string other_channel = getOtherChannel((string)r["from_channel"], (string)r["to_channel"]);
                string call_status = (string)r["call_status"];
                string call_info;

                if (other_channel == (string)r["from_channel"]) {
                    call_info = String.Format("{0} <{1}>", r["from_callerid_name"], r["from_callerid_num"]);
                }
                else {
                    call_info = String.Format("{0} <{1}>", r["to_callerid_name"], r["to_callerid_num"]);
                }

                if (call_status == "Ringing") {
                    dgr.DefaultCellStyle.BackColor = Color.Green;
                    call_info += " *Ringing*";
                }
                else if (call_status == "Hold") {
                    dgr.DefaultCellStyle.BackColor = Color.Orange;
                    call_info += " *Hold*";
                }
                else {
                    // Current Active Call
                    call_info += " *Active*";
                    this.m_CurrentActiveChannelName = other_channel;
                }

                dgr.DefaultCellStyle.Font = dgr_font;
                dgr.CreateCells(this.dataGridViewCurrentCalls, call_info);

                this.dataGridViewCurrentCalls.Rows.Add(dgr);
                if (dgr.Index == selected_index) {
                    dgr.Selected = true;
                }

                if (this.m_CurrentActiveChannelName == null) {
                    // No call that's currently talking
                    if (this.dataGridViewCurrentCalls.Rows.Count == 1) {
                        // Exactly one call to our phone, we can safely make this the active one
                        OrderedDictionary _r = extension_callers[0];
                        this.m_CurrentActiveChannelName = getOtherChannel((string)_r["from_channel"], (string)_r["to_channel"]);
                    }
                }
            }

            ////////////////////////////////////////////
            // Update presense on directory if it's open

            if (this.DirectoryForm.areWeVisible()) {
                this.DirectoryForm.updatePresence(this.m_extensionPresence);
            }
        }

        private void doInit() 
        {
            this.cmpPictureBoxStatusLight.BackgroundImage = Properties.Resources.icon_status_error;
            this.SpeedForm.speedDialPopulate("");
            this.DirectoryForm.directoryPopulate("");

            // Update Timer
            m_cmpTimerUpdateDisplay = new System.Windows.Forms.Timer();
            m_cmpTimerUpdateDisplay.Tick += new EventHandler(UpdateDisplay);
            m_cmpTimerUpdateDisplay.Interval = 2000;
            StartRefreshTimer();

            displayMessageBarMessage(Color.Yellow, "Login Successful.  Console is starting...");
        }

        private void StartupConfigurationFailure(string message) {
            TopMost = false;
            MessageBox.Show(this, "Configuration not successful, console will exit. " + message);
            Environment.Exit(0);
        }

        //////////////////////////////////////////////////////////////////////////////
               
        public void displayMessageBarMessage(Color textColor, string messageText) {
            labelNotificationBar.ForeColor = textColor;
            labelNotificationBar.Text      = messageText;

            if (this.m_cmpTimerDisplayMessage != null) {
                m_cmpTimerDisplayMessage.Stop();
            }

            m_cmpTimerDisplayMessage = new System.Windows.Forms.Timer();
            m_cmpTimerDisplayMessage.Interval = 2000;
            m_cmpTimerDisplayMessage.Tick += new EventHandler(
                delegate(Object myObject, EventArgs myEventArgs) {
                    labelNotificationBar.Text = "";
                    m_cmpTimerDisplayMessage.Stop();
                }
            );

            m_cmpTimerDisplayMessage.Start();
        }

        ///////////////////////////////////////////////////////////////////////
        // Utility Functions

        public void MessageBox_ThreadSafe(string message) {
            if (this.InvokeRequired) {
                this.Invoke((Action) delegate {
                    MessageBox.Show(message);
                });
            }
            else {
                MessageBox.Show(message);
            }
        }

        public void handleDatabaseError(Exception ex, string errorMessage) {
            if (ex != null) {
                errorMessage += ex.ToString();
            }

            MessageBox_ThreadSafe(errorMessage);

            this.m_lastPbxOperationWasFailure = true;
            StopRefreshTimer();
        }

        private void StartRefreshTimer() {
            m_cmpTimerUpdateDisplay.Start();
        }

        public void StopRefreshTimer() {
            this.cmpPictureBoxStatusLight.BackgroundImage = Properties.Resources.icon_status_error;

            if (m_cmpTimerUpdateDisplay != null) {
                m_cmpTimerUpdateDisplay.Stop();
                Debug.Print("Refresh Timer Stopped");
            }
        }

        public void ProgramError(string errorMsg, params string[] errorMsgFormat) {
            m_lastPbxOperationWasFailure = true;
            StopRefreshTimer();
        }

        /// <summary>
        /// Given two channels involved in a call, return which channel is not our own
        /// </summary>
        /// <param name="channel1">A channel involved in the call</param>
        /// <param name="channel2">A channel involved in the call</param>
        /// <returns></returns>
        public string getOtherChannel(string channel1, string channel2) {
            if (channel1.StartsWith(String.Format("SIP/{0}-", getOperatorExten()))) {
                return channel2;
            }

            return channel1;
        }

        public Boolean checkPbxOperationDelay() {
            if (m_lastPbxOperation == new DateTime(0)) {
                return true;
            }

            TimeSpan time_since_last_operation = (DateTime.Now - m_lastPbxOperation);

            if (time_since_last_operation.Seconds >= m_secondsAllowedBetweenOperations) {
                return true;
            }

            displayMessageBarMessage(Color.Yellow, "Please wait a moment before using another command");
            return false;
        }

        public Boolean isActiveCallAvailable() {
            if (this.m_CurrentActiveChannelName == null) {
                displayMessageBarMessage(Color.Yellow, "No currently active call.");
                return false;
            }

            return true;
        }

        public List<OrderedDictionary> getQueues() {
            List<OrderedDictionary> result = new List<OrderedDictionary>();

            m_db.dbQuery("SELECT * FROM queue.queues", result);

            return result;
        }

        /// <summary>
        /// Do a Dial or Blind Transfer depending on what mode we're in.
        /// If the user has already clicked a transfer button, then we will do a transfer to the destination
        /// otherwise we will do a dial
        /// </summary>
        /// <param name="phoneNumber">Destination phone number as if the phone itself has dialed it</param>
        /// <param name="dialedName">Name of the party we are calling</param>
        public void doPbxDial(string phoneNumber, string dialedName) {
            if (!checkPbxOperationDelay()) return;

            m_lastPbxOperation = DateTime.Now;

            if (isTransferMode()) {
                if (this.m_transferToVoicemailMode) {
                    // TODO: FIXME: Not multi-tenant friendly
                    QueryResultSetRecord voicemail_exists = m_db.DbSelectSingleRow(
                        "SELECT voicemail FROM asterisk.v_extensions WHERE COALESCE(override_extension, extension) = {0} AND voicemail = 'yes'",
                        phoneNumber
                    );

                    if (voicemail_exists.Count < 1) {
                        displayMessageBarMessage(Color.Yellow, "Target does not have voicemail");
                        return;
                    }

                    phoneNumber = "**" + phoneNumber;
                }

                displayMessageBarMessage(Color.Yellow, "Executing Operation");

                m_db.DbSelectJsonFunction("asterisk.pbx_transfer_to_phonenumber",
                  this.m_CurrentActiveChannelName,
                  getOperatorExten(),
                  phoneNumber
                );

                setTransferMode(false);
                return;
            }

            displayMessageBarMessage(Color.Yellow, "Executing Operation");

            // Last param is a raw true/false otherwise if we send text we'll be calling the other pbx_dial that the last param is json opts
            m_db.DbSelectJsonFunction("asterisk.pbx_dial",
                new SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType.Text,    getOperatorExten() ),
                new SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType.Text,    phoneNumber ),
                new SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType.Text,    dialedName ),
                new SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType.Boolean, this.m_localPhoneAutoAnswer )
            );
        }

        public void doPbxBridge(string channel1, string channel2) {
            List<OrderedDictionary> resultData = new List<OrderedDictionary>();
            m_lastPbxOperation = DateTime.Now;

            m_db.DbSelectJsonFunction(
                "asterisk.pbx_bridge",
                this.m_CurrentActiveChannelName,
                this.m_CurrentSelectedChannelName
            );

            displayMessageBarMessage(Color.Yellow, "Executing Operation");

            setTransferMode(false);
            return;
        }

        // Did the user click a transfer button  
        //
        public Boolean isTransferMode() {
            return this.m_transferMode;
        }

        private void setTransferMode(Boolean mode) {
            this.m_transferMode = mode;

            if (mode == false) {
                this.m_transferToVoicemailMode = mode;

                labelTransferSelected.Hide();
                labelTransferToVocemailSelected.Hide();
                labelTransferInfo.Hide();
            }
        }

        public Image getResourceImage(string resourceName) {
            return this.m_bitmaps[resourceName];
        }

        ///////////////////////////////////////////////////////////////////////
        // GUI Helpers

        private void showSpeedDials() {
            this.MessagingForm.HideBodyPanel();
            this.DirectoryForm.HideBodyPanel();

            this.SpeedForm.ShowBodyPanel();
        }

        private void showDirectory() {
            this.MessagingForm.HideBodyPanel();
            this.SpeedForm.HideBodyPanel();

            this.DirectoryForm.ShowBodyPanel();
        }

        private void showMessaging() {
            this.DirectoryForm.HideBodyPanel();
            this.SpeedForm.HideBodyPanel();

            this.MessagingForm.ShowBodyPanel();

            //this.DirectoryForm.getBodyPanel().Show();
        }

        private void toggleStatusLight() {
            this.m_lightStatus = !this.m_lightStatus;
            this.cmpPictureBoxStatusLight.BackgroundImage = this.m_lightStatus ? Properties.Resources.icon_status_ok : Properties.Resources.icon_status_off;
        }

        public void StartMessageConversation(string device, string extension, string contactName) {
            this.showMessaging();
            this.MessagingForm.SetCurrentConversationExtension(device, extension, contactName);
        }

        ///////////////////////////////////////////////////////////////////////
        // Event Handlers

        private void dataGridViewCallers_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex < 0) return;
            if (!checkPbxOperationDelay()) return;

            List<OrderedDictionary> resultData = new List<OrderedDictionary>();
            m_lastPbxOperation = DateTime.Now;

            if ((string)this.m_queueCallers[e.RowIndex]["call_type"] == "PARK") {
                string park_pos = (string) this.m_queueCallers[e.RowIndex]["park_pos"];

                // exten, park_lot, park_pos
                m_db.DbSelectJsonFunction("queue.hangup_ringing_queue_channel", this.getOperatorExten());
                m_db.DbSelectJsonFunction("asterisk.pbx_park_pickup",           this.getOperatorExten(), this.m_operatorQueue, park_pos);

                return;
            }

            m_db.DbSelectJsonFunction("queue.hangup_ringing_queue_channel", getOperatorExten());

            OrderedDictionary row = this.m_queueCallers[e.RowIndex];
            m_db.DbSelectJsonFunction("queue.grab_queue_member", (string) row["channel"], getOperatorExten());
        }

        private void buttonDial_Click(object sender, EventArgs e) {
            if (!checkPbxOperationDelay()) return;

            doPbxDial(this.textBoxCallControl.Text, this.textBoxCallControl.Text);
        }

        private void comboBoxCallControl_SelectedIndexChanged(object sender, EventArgs e)
        {
    
        }

        private void buttonPark_Click(object sender, EventArgs e)
        {
            if (!isActiveCallAvailable()) return;
            if (!checkPbxOperationDelay()) return;

            List<OrderedDictionary> resultData = new List<OrderedDictionary>();
            m_lastPbxOperation = DateTime.Now;

            // channel, park_timeout, dest_lot, dest_pos
            m_db.DbSelectJsonFunction("asterisk.pbx_park",
                new SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType.Text,    this.m_CurrentActiveChannelName),
                new SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType.Integer, 0),
                new SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType.Text,    this.m_operatorQueue),
                new SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType.Text,    null)
            );
        }

        private void labelSpeedDialTab_Click(object sender, EventArgs e) {
            showSpeedDials();
        }

        private void labelDirectoryTab_Click(object sender, EventArgs e) {
            if (DirectoryForm == null) {
                return;
            }

            DirectoryForm.setQuickSearchValue("");
            DirectoryForm.directoryPopulate("");
            showDirectory();
        }
   
        private void buttonAlphabet_Click(object sender, EventArgs e) {
            showDirectory();

            // all the buttons are named buttonX
            Button sender_button = (Button) sender;
            char[] button_name = sender_button.Name.ToString().ToCharArray();
            string search_string = button_name[6].ToString();

            DirectoryForm.setQuickSearchValue(search_string);
            DirectoryForm.directoryPopulate(search_string);
        }

        private void buttonShowAll_Click(object sender, EventArgs e) {
            showDirectory();

            DirectoryForm.setQuickSearchValue("");
            DirectoryForm.directoryPopulate("");
        }

        private void buttonHangup_Click(object sender, EventArgs e) {
            if (!isActiveCallAvailable()) return;

            List<OrderedDictionary> resultData = new List<OrderedDictionary>();
            displayMessageBarMessage(Color.Yellow, "Executing Operation");

            m_db.DbSelectJsonFunction("asterisk.pbx_hangup", this.m_CurrentActiveChannelName, "Operator Console");
        }

        private void textBoxCallControl_KeyPress(object sender, KeyPressEventArgs e) {
            TextBox t = (TextBox)sender;

            if (e.KeyChar == (char)System.Windows.Forms.Keys.Enter) {
                buttonDial_Click(sender, null);
            }
        }
        
        private void textBoxCallControl_DoubleClick(object sender, EventArgs e) {
            if (!isTransferMode()) return; // User must first hit transfer, then double click the dial input box to blind transfer to a custom number

            buttonDial_Click(sender, null);
        }

        private void buttonTransfer_Click(object sender, EventArgs e) {
            if (!isActiveCallAvailable()) return;
            if (!checkPbxOperationDelay()) return;

            if (isTransferMode()) {
                setTransferMode(false);
                return;
            }

            labelTransferSelected.Show();
            labelTransferInfo.Show();
            m_transferMode = true;
        }

        private void buttonTransferToVoicemail_Click(object sender, EventArgs e) {
            if (!isActiveCallAvailable()) return;
            if (!checkPbxOperationDelay()) return;

            if (isTransferMode()) {
                setTransferMode(false);
                return;
            }

            labelTransferToVocemailSelected.Show();
            labelTransferInfo.Show();
            m_transferMode = true;
            m_transferToVoicemailMode = true;
        }

        private void buttonConfCall_Click(object sender, EventArgs e) {
            displayMessageBarMessage(Color.Yellow, "Conference is not configured for this user");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == Keys.Escape) {
                setTransferMode(false);
                return false;    // indicate that the keystroke can still be handled by the widget itself
            }

            // Call the base class
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void dataGridViewCurrentCalls_SelectionChanged(object sender, EventArgs e) {
            DataGridView dg = (DataGridView)sender;
            if (dg.Rows.Count == 0) return;
            if (dg.SelectedRows.Count == 0) return;

            DataGridViewRow dgr = dg.SelectedRows[0];
            OrderedDictionary r = this.m_extensionCallers[dgr.Index];

            this.m_CurrentSelectedChannelName = getOtherChannel((string) r["from_channel"], (string) r["to_channel"]);
        }

        private void dataGridViewCurrentCalls_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex < 0) return;
            if (!isTransferMode()) return;
            if (this.m_transferToVoicemailMode) return; // Bridge to voicemail makes no sense

            if (m_CurrentActiveChannelName == m_CurrentSelectedChannelName) {
                displayMessageBarMessage(Color.Yellow, "You cannot transfer a call to yourself!");
                return;
            }

            doPbxBridge(m_CurrentActiveChannelName, m_CurrentSelectedChannelName);
        }

        private void cmpContextMenuStripMainStripMenuItemConfig_Click(object sender, EventArgs e) {
            new OperatorLoginWindow(this, loginCheckCallbackFn, loginSuccessCallbackFn, delegate(OrderedDictionary loginentry) {  }, this.m_operatorQueue, "");
        }

        private void panelCallControl_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button != System.Windows.Forms.MouseButtons.Right) return;

            this.cmpContextMenuStripMain.Show(System.Windows.Forms.Cursor.Position);
        }

        private void cmpContextMenuStripMainStripMenuItemAdmin_Click(object sender, EventArgs e) {
            PasswordDialogForm pwForm = new PasswordDialogForm();
            pwForm.SetValidateCallback(AdminPasswordValidate);
            pwForm.SetSuccessCallback(ShowAdminSettings);
            pwForm.Show();
        }

        private void intellaConsole_SizeChanged(object sender, EventArgs e) {
            try {
                _intellaConsole_SizeChanged(sender, e);
            }
            catch (Exception ex) {
                string foo = ex.ToString();
            }
        }

        private void _intellaConsole_SizeChanged(object sender, EventArgs e) {
            if (!m_firstUpdate) {
                return;
            }

            Form main_form = (Form) sender;

            drawGradientBackground();
            
            int new_width  = this.Width  - this.dataGridViewCallers.Width - 100;
            int new_height = this.Height - this.panelCallControl.Height   - 100;

            panelDirectoryAndSpeedDial.Width  = new_width;
            panelDirectoryAndSpeedDial.Height = new_height;

            ////////////////////
            // Speed Dial Rezize

            Panel speed_panel       = SpeedForm.getBodyPanel();
            DataGridView speed_grid = SpeedForm.GetDataGridView();

            speed_panel.Width  = new_width;
            speed_panel.Height = new_height;
            speed_grid.Width   = new_width;
            speed_grid.Height  = new_height;

            int width_per_col = speed_panel.Width / speed_grid.Columns.Count;
            foreach (DataGridViewColumn col in speed_grid.Columns) {
                col.Width = width_per_col;
            }

            int height_per_row = (speed_panel.Height / speed_grid.Rows.Count) - 5;
            foreach (DataGridViewRow row in speed_grid.Rows) {
                row.Height = height_per_row;
            }

            ////////////////////
            // Directory Rezize

            Panel directory_panel       = DirectoryForm.getBodyPanel();
            DataGridView directory_grid = DirectoryForm.GetDataGridView();

            directory_panel.Width  = new_width;
            directory_panel.Height = new_height;
            directory_grid.Width   = new_width;
            directory_grid.Height  = new_height;

            width_per_col = directory_panel.Width / directory_grid.Columns.Count;
            foreach (DataGridViewColumn col in directory_grid.Columns) {
                col.Width = width_per_col;
            }

            this.cmpPictureBoxStatusLight.Location = new Point((this.Width - this.cmpPictureBoxStatusLight.Width - 275), this.cmpPictureBoxStatusLight.Location.Y);
            this.cmpLogoAndVersionPanel.Location = new Point((this.Width - this.cmpLogoAndVersionPanel.Width - 375), this.cmpLogoAndVersionPanel.Location.Y);
        }

        private void labelExtensionTab_Click(object sender, EventArgs e) {

        }

        private void cmpShowMessagingPanelLabel_Click(object sender, EventArgs e) {
            showMessaging();
        }
    }

    class OperatorConsoleLiveData : Object {
        public List<OrderedDictionary> QueueCallers;
        public List<OrderedDictionary> ExtensionCalls;
        public Hashtable PresenseInfo; // Device state indexed by device ie: "SIP/6200" (to be joined up with the directory list)

        public OperatorConsoleLiveData() {
            this.QueueCallers   = new List<OrderedDictionary>();
            this.ExtensionCalls = new List<OrderedDictionary>();
            this.PresenseInfo   = new Hashtable();
        }
    }
}

class BooleanObject : Object {
    public Boolean value = false;
}