// TODO: While running and connected to one system, changing to another system doesn't switch until restarting the app
//

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

// using System.Runtime.ExceptionServices;

namespace intellaQueue
{
    public partial class IntellaQueueForm : System.Windows.Forms.Form {
        public static string[] m_args;     // Application Commandline Arguments
        private static Thread m_guiThread; // Main GUI Thread

        public static IntellaQueueForm IntellaQueueFormApplicationForm; // So we can get to the main Form from anywhere

        private Label m_cmpToolbarStatusMessage = new Label(); // Message to display at the bottom of the toolbar.
        private DateTime m_ToolbarStatusMessageLastUpdate = DateTime.Now;
        private TimeSpan m_ToolbarStatusMessageKeep = TimeSpan.FromSeconds(15);

        private wyDay.Controls.AutomaticUpdaterBackend cmpAutomaticWY_Updater;
        private bool m_wyUpdateCheckManual                  = true; // First one is a yes.. Output the results to the statusbar if we're up to date
        private TimeSpan m_wyUpdateCheckInterval            = TimeSpan.FromHours(1);
        private DateTime m_wyUpdateLastCheck                = DateTime.Now;
        private System.Windows.Forms.Timer m_wyUpdate_Timer = new System.Windows.Forms.Timer();

        /*
        private static readonly ILog log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );
        */

        private static Boolean logFileEnabled    = false;
        private static Boolean m_logFileReady    = false;
        private static string m_logFileDirectory = Path.Combine(Path.GetTempPath(), "IntellaQueue");
        private static string m_logFileBase      = Path.Combine(m_logFileDirectory, "log");
        private static StreamWriter m_logFileHandle = null;
        private static DateTime m_logFileOpenedWhen;
        private static int m_LogFilesKeep = 3;
        private static double m_uploadLogFileMinutes = 15; 
        private LogFileUploader m_logFileUploader;

        // Live Data Grids
        LiveDataGridViewForm m_ldg_callers;
        LiveDataGridViewForm m_ldg_queues;

        private static int MidBarHeight = 20;

        private static bool UserResizeEnabled = false;

        private static Color QueueColorEmpty    = Color.FromArgb(173, 173, 173);
        private static Color QueueColorOk       = Color.FromArgb(141, 193, 102);
        private static Color QueueColorWarning1 = Color.FromArgb(243, 219, 119);
        private static Color QueueColorWarning2 = Color.FromArgb(233, 138, 75);
        private static Color QueueColorWarning3 = Color.FromArgb(211, 101, 101);

        private bool m_IsToolbarFullySetup = false;
        private bool m_IsAgentLoggedIn = false;
        private bool m_multiSite = false;
        private static Boolean m_enableTeamView = false;
        private static Boolean m_showAgentNumberInTitleBar = true;
        public static Boolean m_isManager_MontorEnabled = true;
        public static Boolean m_isManager = false;
        public static Boolean m_isDeveloper = false;
        public static Boolean m_displayCallerDetailNonManager = true;

        private static Point m_statusLightBaseLocation;
        private static Point m_dropGridBaseLocation;
        private static Point m_sizeAdjustorBaseLocation;
        private static Point m_closeButtonBaseLocation;
        private static Point m_minimizeButtonBaseLocation;
        private static Point m_settingsButtonBaseLocation;
        private static List<Control> m_dynamicControls;

        private ToolbarConfiguration m_toolbarConfig; // so timer refs and settings stay resident
        private System.Windows.Forms.Timer m_updateDisplayTimer;
        private System.Windows.Forms.Timer m_updateDisplayTimerHealthCheck;

        private SortedList<string, Hashtable> m_subscribedQueues = new SortedList<string, Hashtable>();     // Populated by fillQueuesFromDb()
        private SortedList<string, Hashtable> m_subscribedQueuesMain = new SortedList<string, Hashtable>(); // Populated by fillQueuesFromDB()
        private Hashtable m_subscribedQueuesHidden = new Hashtable(); // 
        private Dictionary<string, string> m_subscribedQueuesOrder;

        // Very very new IntellaQueueControl for primary toolbar connection webservices
        LibICP.IntellaQueueControl m_iqc = new LibICP.IntellaQueueControl();
        LibICP.JsonQueueLoginLogoutResult m_iqc_login;

        // ----------------------------------------------------------------
        // NEW Server object which contains m_subscribedQueues*, m_db and other queue fields
        private ToolbarServerConnection m_mainTSC                            = new ToolbarServerConnection();
        private Dictionary<string, ToolbarServerConnection> m_toolbarServers = new Dictionary<string, ToolbarServerConnection>(); // Also contains m_mainTSC
        // ----------------------------------------------------------------

        private static QD.QE_ErrorCallbackFunction m_errorHandler = QD.GenericErrorCallbackFunction;

        private static int      MAX_DB_RECONNECTS       = 3;                         // Max MAIN DB reconnect attempts before we perform a 'fresh start' toolbar
        private static TimeSpan MAX_DB_RECONNECT_WINDOW = TimeSpan.FromMinutes(5);   // If we get hit MAX_DB_RECONNECTS in MAX_DB_RECONNECT_WINDOW, then we consider this a PROGRAM_FAILURE
        private static int      MAX_PROGRAM_FAILURES    =  3;                        // Max exceptions/errors before just freezing the toolbar and giving up

        private bool m_db_active       = false;
        private bool m_db_reconnecting = false;         // If this true, we will try and reconnect to the db inside the ToolbarServerConnection Data Update Thread (DoWorkOnTSC)
        private int m_db_reconnects    = 0;             // How many times we've recently reconncted to the db
        DateTime m_db_last_reconnect   = DateTime.Now;  // How recent have we connected to the db

        private int m_program_failures = 0;
        private DateTime m_program_failure_last;

        private int m_database_reconnect_sec = 60 * 60;    // To avoid excessive memory usage in the backend, reconnect to the database every m_database_reconnect_sec
                                                           // Reconnect every hour

        private string QueueRightClickContextMenu_SelectedQueue;
        // When we right click on a queue row, this is the queue we clicked on

        private string cmpAgentManagerRightClickContextMenu_selectedAgentDevice;
        // When we right click on a the agent status grid, this is the agent we clicked on

        private TreeView m_connectionTreeView = null;
        private ConnectionManager m_cm = null;
        private static DbHelper m_main_db;

        // Current Logged in Agent (To this toolbar)
        private static string m_tenantName = "";
        private static string m_agentName = "";
        private static string m_agentID = "";
        private static string m_agentNum = "";
        private static string m_agentPin = "";
        private static string m_agentDevice = "";
        private static string m_agentExtension = "";
        private static string m_lastAgentBackLogLastUnixTime = "0";

        // Toolbar Data
        private DateTime m_lastMainDatabaseUpdate = DateTime.Now;
        private ToolBarLiveData m_toolbarLiveData;
        private Hashtable m_liveDatas = new Hashtable(5);
        private bool m_liveGuiDataNeedsUpdate = false;
        private string m_lastOP = ""; // Debug/Tracing

        ///////////////////////////////////////////////////////////////////////
        // General Toolbar Configuration

        private bool m_allowToolbarClose = true;                // Will be populated by per-tenant config (queue.tenant -> toolbar_allow_close)
        private bool m_agentAutoLogin    = false;               // Will be populated by per-tenant config (queue.tenant -> toolbar_auto_login)
        private bool m_hideInterface     = false;               // Will be populated by per-tenant config (queue.tenant -> toolbar_hide_interface)

        // Screen Pop Related
        private bool     m_screenPopsEnabled          = false;  // Will be populated by tenant/agent config
        private bool     m_screenPop_LoginLaunched    = false;
        private string   m_screenPopLoginUrl          = "";
        private string   m_screenPopURL               = "";     // Will be populated by per-queue  config (queue.toolbar_config -> screenpop_url)

        // Screen Recording Related
        private bool     m_screenRecordingEnabled     = false;  // Will be populated by per-tenant config (queue.tenant -> toolbar_screen_recording_enabled)
        private string   m_screenRecordingUploadURL   = "";     // Will be populated by per-tenant config (queue.tenant -> toolbar_screen_recording_upload_url)
        private JsonHash m_lastScreenPopData          = null;
        private IntellaScreenRecording m_screenRecord = new IntellaScreenRecording();

        ///////////////////////////////////////////////////////////////////////

        // Does the main server have a Dialer ?
        private bool m_mainServerHasDialer = false;

        // Agent Login Related
        private Panel m_agentLoginPanel;

        // Status Code Related
        public bool m_statusCodesEnabled = true; // FIXME... This used to be an optional thing, but now every site has status codes.  This can be removed.
        private StatusCodesForm m_statusCodesForm;
        private Panel m_statusCodesPanel;

        // Current Call Related
        public bool m_currentCallEnabled = false; // FIXME: Needs per-tenant toolbar setting
        private CurrentCallForm m_currentCallForm;
        private Panel m_currentCallCenteringPanel;
        private QueryResultSetRecord m_currentCallRecord = null; // FIXME: Only supports one call!
        private string m_currentCallChannel = "";

        // Disposition Code Related
        public bool m_dispositionCodesEnabled = false; // FIXME: Needs per-tenant toolbar setting
        private DispositionCodesForm m_dispositionForm;
        private Panel m_dispositionCenteringPanel;

        /// <summary>
        /// /
        /// </summary>

        private Panel m_dispositionPanel;
        private Panel m_currentCallerPanel;

        // Columns Enabled or Disabled
        private bool showQueueCallerColumn = true; // TODO: Make this a db config item

        private Panel m_statusCodesCenteringPanel;
        // Used to align the Pause Codes panel x-center with respect to toolbar form


        // Used to align the Disposition Codes panel x-center with respect to toolbar form

        private Hashtable m_currentChannelsTalkingTo = new Hashtable();

        private bool m_lightStatus = false;

        // Top Menubar Related
        private QueueToolbarAboutForm m_aboutForm;
        private ToolStripItem cmpCallQueueSnapshotToolStripItem;

        private QueueAppConfigMainForm m_QueueAppConfigMainForm;
        //private QueueAppConfigMainFormHelper m_mainHelper; // for new connection manager stuff which is broken

        private static int m_mouseDownCellRowIndex = -1, m_mouseDownCellColumnIndex = -1;
        private static int m_managerColumnDropDownWidth = 0, m_agentColumnDropDownWidth = 0;

        /// 3 vars to handle user dragging of windows without border decorations
        private bool isFormDragging = false;

        private Point dragCursorPoint;
        private Point dragFormPoint;

        private bool m_doNotShowSettingsDropdownOnNextClick = false;

        public enum PictureBoxButtonEvents {
            Enter,
            Leave,
            Down,
            UpInside,
            UpOutside
        };

        public enum ButtonCellColorState {
            Base,
            Light,
            Dark
        };

        // used for mouse drag window/form resizing by user (disabled)
        private bool m_isFormResizing;
        private Point m_resizeCursorPoint;
        private Size m_resizeFormSize;

        private static bool m_isMinimized = false;
        private string m_interfaceSize = "small";

        // Space for Additional Controls at the bottom of the toolbar (Used when pause codes are enabled)
        private Panel m_extraControl = null;
        public int m_extraControlHeight = 0; // FIXME... shouldn'd be public

        // Debugging/Exception related
        private static DataDumperForm m_MainDF;

        // Grids
        //---------------
        // mainGrid (defined in intellaQueue.Designer.cs) -- Top Grid -- The Per-Queue Overall realtime data
        // 
        // Queue Callers and Agent Status grids are built in setupGrids()

        //----------------------------------------------------------------------


        // Old-Style ProgramError
        // public delegate void ProgramErrorCallbackFunction(string errorMsg, params string[] errorMsgFormat); // Fatal program Error
        private void ProgramError(string errorMsg, params string[] errorMsgFormat) {
            string log_line = "ERROR: " + String.Format(errorMsg, errorMsgFormat);
            handleError(log_line);
            StopRefreshTimer();
        }

        // New-Style QE_LoggerFunction with Exception
        // public delegate string QE_LoggerFunction(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] msgFormat);        // QuickError -- Non-fatal error
        private string ProgramError(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] msgFormat) {
            string log_line = "ERROR: " + String.Format("[{0} {1}] ", errorToken, errorLevel) + String.Format(msg, msgFormat);
            handleError(log_line);
            StopRefreshTimer();

            return log_line;
        }

        // New-Style ProgramError without associated Exception
        private string ProgramError(QD.ERROR_LEVEL errorLevel, string errorToken, string msg, params string[] msgFormat) {
            string log_line = "ERROR: " + String.Format("[{0} {1}] ", errorToken, errorLevel) + String.Format(msg, msgFormat);
            handleError(log_line);
            StopRefreshTimer();

            return log_line;
        }

        // public delegate string QE_ErrorCallbackFunction(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] msgFormat); // QuickError -- Fatal Error
        public static string DatabaseErrorCallbackFunction(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] msgFormat) {
            string log_line = String.Format(String.Format("{0} {1} {2}", errorLevel, errorToken, msg), msgFormat);

            if (errorLevel == QD.ERROR_LEVEL.FATAL) {
                ApplicationExitStatic();
            }

            log_line = MQD(log_line);

            return log_line;
        }

        // Start building the form
        public IntellaQueueForm() : base() {
            IntellaQueueFormApplicationForm = this;

            m_IsToolbarFullySetup = false;

            m_guiThread = Thread.CurrentThread;

            // This call is required by the Windows Form Designer.
            InitializeComponent();

            this.Closing += delegate(object sender, CancelEventArgs e) {
                if (!m_allowToolbarClose) {
                    e.Cancel = true;

                    this.WindowState = FormWindowState.Minimized;
                    m_isMinimized = true;
                }

                ApplicationExit();
            };
        
            // StatusCode Bottom Widget is created in AgentLoginSuccess
            string interface_size = Interaction.GetSetting("IntellaToolBar", "Config", "InterfaceSize");
            if (interface_size != "") {
                switch (interface_size) {
                    case "small":
                    case "medium":
                    case "large":
                        m_interfaceSize = interface_size;
                        break;
                }
            }

            SetInterFaceSize(m_interfaceSize, true);

            if (System.Diagnostics.Debugger.IsAttached) {
                // Do not set always on top (interferes with debugger)
                m_isDeveloper = true;
            }
            else {
                // Set always on top based on local pref
                this.TopMost = alwaysOnTopToolStripMenuItem.Checked;
                m_isDeveloper = false; // Default until otherwise specified in agentLoginValidate()
            }

            // double buffering goodness
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer, true);

            m_dynamicControls = new List<Control>();

            m_statusLightBaseLocation = new Point(statusLight.Location.X, statusLight.Location.Y);
            m_dropGridBaseLocation = new Point(dropGridTemplate.Location.X + queueResources.border_left_tiled.Width, dropGridTemplate.Location.Y);
            m_sizeAdjustorBaseLocation = new Point(sizeAdjustorPb.Location.X, sizeAdjustorPb.Location.Y);
            m_closeButtonBaseLocation = new Point(closePb.Location.X, closePb.Location.Y);
            m_minimizeButtonBaseLocation = new Point(minimizePb.Location.X, minimizePb.Location.Y);
            m_settingsButtonBaseLocation = new Point(settingsPb.Location.X, settingsPb.Location.Y);

            m_isManager = false;
            m_isManager_MontorEnabled = false;

            m_updateDisplayTimer = new System.Windows.Forms.Timer();
            m_updateDisplayTimer.Tick += UpdateDisplayTimer_Tick;

            //m_connectionTreeView = new TreeView();
            //Properties.Settings settings = Properties.Settings.Default;
            //m_cm = new ConnectionManager(launchError, settings.ConnectionsInfo, m_connectionTreeView, handleConnectionStatusChange);
            //this.fileToolStripMenuItem.DropDownItems.Insert(0, m_cm.NewConnectionMenuItem);
            //m_cm.refreshDisplayedConnections();

            hideUiManagedObjects();

            LayoutForm();

            if (!UserResizeEnabled) {
                sizeAdjustorPb.MouseDown += intellaQueue_MouseDown;
                sizeAdjustorPb.MouseUp += intellaQueue_MouseUp;
                sizeAdjustorPb.MouseMove += intellaQueue_MouseMove;
            }

            mainGrid.MouseDown += intellaQueue_MouseDown;
            mainGrid.MouseUp += intellaQueue_MouseUp;
            mainGrid.MouseMove += intellaQueue_MouseMove;

            m_IsToolbarFullySetup = true;
        }

        ////
        // Agent Login Successful
        //
        // Database successfully configured
        // queue successfully configured
        //
        // Start updating data
        //
        private void ConfigurationSuccess() {
            // Upload Previous Log File Data
            string log_file = GetTodaysLogFileName();

            TryAndUploadLogFile(log_file); // Upload what we have so far
            TryOpenAndRotateLog(log_file);
            TryAndUploadCurrentLog();

            MQD("-- Setup Main Toolbar Connection");

            m_toolbarConfig = new ToolbarConfiguration(m_mainTSC)
            {
                AgentDevice                   = m_agentDevice,
                Manager                       = m_isManager,
                DisplayCallerDetailNonManager = m_displayCallerDetailNonManager,
                m_multiSite                   = this.m_multiSite,
                m_agentStatusCodesEnabled     = m_statusCodesEnabled // TODO: Get option from DB
            };

            ///////////////////////////////////////////////////////////////////
            // Ensure we have dedicated access to the DB before proceeding

            int lock_attempts = 10;

            TryLock l = new TryLock(m_main_db);

            while (!l.GetLock() && lock_attempts-- > 0) {
                Thread.Sleep(1000);
            }

            if (!l.HasLock) {
                // private string ProgramError(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, string[] msgFormat) {

                ProgramError(QD.ERROR_LEVEL.ERROR, "DB_LOCK_FAILED", new Exception(""), "Main database lock failed");
                return;
            }

            ///////////////////////////////////////////////////////////////////
            // Main DB connection -- m_main_db locked

            QueueVersion.CheckVersion();

            GetAndProcessQueueSetup();

            // TODO: New: LiveDataGrid
            // m_ldg_agents = new LiveDataGridViewForm();
            // m_ldg_agents.Show();
            // BuildLiveAgentsGrid();

            l.Dispose();

            // Main DB connection -- m_main_db un locked
            ///////////////////////////////////////////////////////////////////

            m_toolbarConfig.SetQueue(m_subscribedQueues);
            m_toolbarConfig.SetUpdateDisplayTimer(m_updateDisplayTimer);

            // Update ALL Toolbar Data Sources, Including the MainTSC
            foreach (ToolbarServerConnection tsc in m_toolbarServers.Values) {
                m_toolbarConfig.UpdateToolbarConfiguration(tsc);
            }

            MQD("-- Start HealthCheck Timer");

            m_updateDisplayTimerHealthCheck = new System.Windows.Forms.Timer();
            m_updateDisplayTimerHealthCheck.Interval = 2000;
            m_updateDisplayTimerHealthCheck.Tick += UpdateDisplayTimerHealthCheck_Tick;
            m_updateDisplayTimerHealthCheck.Start();

            MQD("-- Initalize Grids");

            InitDropGridsAndQueueInfo();
            refresh();

            MQD("-- Start Refresh Timer");
            StartRefreshTimer();

            MQD("-- Setup Data Threads");
            SetupUpdateThreads();

            MQD("-- Setup LogFile Uploader");
            this.m_logFileUploader = new LogFileUploader(
                timerInterval:         (int) TimeSpan.FromMinutes(m_uploadLogFileMinutes).TotalSeconds,
                logFileUploadCallback: TryAndUploadCurrentLog
            );
        }

        public DbHelper GetDatabaseHandle() { return this.m_mainTSC.m_db; }
        public CurrentCallForm GetCurrentCallForm() { return this.m_currentCallForm; }
        public JsonHash GetLastScreenPopData() { return this.m_lastScreenPopData; }

        private void StartupConfigurationFailure(string message) {
            TopMost = false;
            MessageBox.Show(this, "Configuration not successful, toolbar will exit");
            ApplicationExit();
        }

        private void handleDatabaseSuccess() {
        }
        
        private void Form_Load(object sender, System.EventArgs e) {
            m_MainDF = new DataDumperForm(Debugger.IsAttached); // If debugger, show window on startup
            m_MainDF.SetTitle("Debug Log");
            m_MainDF.SetDumperNullOutput(false);

            // Temp
            //Form intellaCast = new IntellaCast.IntellaCastTestForm();
            //intellaCast.Show();
            // Temp

            //////////////////
            // Logging Related

            Utils.SetLoggerCallBack_QD(MQD);
            Utils.SetLoggerCallBack_QE(ProgramError);

            if (this.m_screenRecordingEnabled) {
                m_screenRecord.SetLoggerCallBack_QD(MQD);
            }

            //////////////////

            this.m_cmpToolbarStatusMessage.AutoSize = true;
            this.m_cmpToolbarStatusMessage.Anchor = AnchorStyles.Left;
            this.m_cmpToolbarStatusMessage.ForeColor = Color.White;
            this.m_cmpToolbarStatusMessage.Font = new Font(this.m_cmpToolbarStatusMessage.Font.FontFamily, this.m_cmpToolbarStatusMessage.Font.Size, FontStyle.Bold);
            // this.m_cmpToolbarStatusMessage.Location is set in: LayoutForm()
            this.Controls.Add(this.m_cmpToolbarStatusMessage);

            //////////////////

            // Minimum Requirements for GUI work has been met
            // We have:
            //  - Vanilla Form Loaded - No Major GUI Elements Present
            //  - Logging System
            //
            //  Note: We still have more to do when the user logs in, but we have enough built in the gui to start checking for udpates

            // Check for Updates on a regular basis
            if (true || !Debugger.IsAttached) {
                MQD("-- Start Update Checks");
                InitializeUpdateChecks();
            }

            //////////////////

            // default interval until we get config from db
            m_updateDisplayTimer.Interval = 1000;

            // Initalize our livedatas
            m_toolbarLiveData = new ToolBarLiveData();

            m_liveDatas["caller"] = new Dictionary<string, List<OrderedDictionary>>();
            m_liveDatas["agent"] = new Dictionary<string, List<OrderedDictionary>>();
            m_liveDatas["queue"] = new Dictionary<string, List<OrderedDictionary>>();
            m_liveDatas["status"] = new Dictionary<string, List<OrderedDictionary>>();
            m_liveDatas["call"] = new Dictionary<string, List<OrderedDictionary>>();

            m_main_db = new DbHelper();
            m_main_db.SetPrepared(true);
            m_main_db.SetErrorCallback(DatabaseErrorCallbackFunction);

            // All Special case stuff for MAIN
            // TODO: AgentOtherServersConnect() needs to be normalized to handle connecting to ALL servers

            // Add our main server into our list of servers
            // m_toolbarConfig = Pass on our global toolbar configuration (AgentDevice, IsManager, and other global toolbar settings)
            ToolbarServerConnection tsc = new ToolbarServerConnection("LOCAL", "Local Server", m_toolbarConfig);
            m_mainTSC = tsc;

            // Subscribedqueues for MAIN is populated in fillQueuesFromDb()
            tsc.m_subscribedQueues = m_subscribedQueuesMain;
            tsc.m_isMainServer = true;
            tsc.m_db = m_main_db;
            tsc.m_iqc = m_iqc;

            m_toolbarServers.Add("LOCAL", tsc);

            ToolBarHelper.SetToolbarServerConnection(m_mainTSC);
            ToolBarHelper.SetProgramErrorCallback(ProgramError);

            // FIXME.. to be removed
            // This also connects to the database
            ToolBarHelper.DatabaseSetupCheck(ref m_main_db, null, DatabaseSuccess, StartupConfigurationFailure);
        }

        public string GetTodaysLogFileName() {
            DateTime today = DateTime.Today;

            string logsuffix = string.Format("{0}{1,2:D2}{2,2:D2}", today.Year, today.Month, today.Day);
            string todays_logfilename = (m_logFileBase + (logsuffix + ".log"));

            return todays_logfilename;
        }

        private void DatabaseSuccess(string message) {
            ResetApplicationAndCheckAgentLogin();
        }

        public void handleError(Exception ex, string errorMessage, params string[] argsRest) {
            if (ex != null) {
                System.Type type = ex.GetType();

                errorMessage = "Exception Type: " + type + "\r\n" + errorMessage;

                if (type.ToString().StartsWith("Npg")) {
                    // DB Error.  We handle reconnects elsewhere.  Only log this internally.  If we fail reconnect too many times, we'll handle
                    // showing the debug log popup elsewhere
                    MQD("--- Main Error Handler ---");
                    MQD(errorMessage, argsRest);
                    return;
                }

                handleError(errorMessage + "\r\nStack Trace:" + ex.StackTrace.ToString(), argsRest);
            }
            else {
                string error = String.Format(errorMessage, argsRest);
                MQD("!!! Error has occurred: {0}", error);
            }

            // TODO: Figure out if this is a DB error or not

            Error_DatabaseMainConnection_Failed();
        }

        public void handleError(string errorMessage, params string[] argsRest) {
            MQD("--- Main Error Handler ---");

            try {
                ShowDebugWindowFromThread();
            }
            catch (Exception ex) {
                ex.ToString(); // Avoid warnings
            }

            //m_MainDF.ThreadSafeShow();
            MQD(errorMessage, argsRest);
        }

        // CONVERTED
        private void GetAndProcessQueueSetup() {
            string agent_num = Interaction.GetSetting("IntellaToolBar", "Config", "USER_agentNumber");

            // Queue Assignments (What Queues are we looking at)
            // Show Queues (Queues Listing Override) -- cfg_get_agents_toolbar_show_queues
            // Forced Additional Queues              -- cfg_get_agents_toolbar_show_additional_queues
            // Hidden Queue Processing               -- Local setting (for now)

            // Clean Slate.  We will fully populate m_subscribedQueues going forward
            m_subscribedQueues.Clear();

            // TODO: Should make a separate view for what queues to allow in the toolbar, and if they are currently hidden, whether the user can unhide them
            MQD("-- Getting Queue Assignments and Prefs");

            JsonHash queue_assignments_and_prefs_jh    = this.m_iqc_login.agent_data.GetItem("queue_assignments_and_prefs");
            QueryResultSet queue_assignments_and_prefs = queue_assignments_and_prefs_jh.ToQueryResultSet();

            displayQueuesToolStripMenuItem.DropDownItems.Clear();

            MQD("-- Getting Additional Queues");
            JsonHashResult additional_queues_result = this.m_iqc.GetQueuesAdditional(prefixQueueName: "LOCAL");
            QueryResultSet additional_queues        = additional_queues_result.Data.ToQueryResultSet();

            foreach (QueryResultSetRecord additional_queue in additional_queues) {
                // We reference subscribed queues by prefix-<QueueName>, always
                if (m_multiSite) {
                    additional_queue["queue_name"] = additional_queue["prefixed_queue_name"];
                }
                else {
                    additional_queue["queue_name"] = additional_queue["queue_name"];
                }

                queue_assignments_and_prefs.Add(additional_queue);
            }

            MQD("-- Connect to Other Monitored Server(s)");

            // FIXME WEBSERVICE
            // Handle monitoring 'Other' servers
            AgentOtherServersConnect();

            //////////////////////////////
            // Build list of hidden queues. TODO: This can go away once we do the above todo

            Hashtable showqueues = new Hashtable();

            MQD("-- Getting Show Queues List");
            JsonHashResult show_queues_result = this.m_iqc.GetQueuesShow(prefixQueueName: "LOCAL");
            QueryResultSet show_queues        = show_queues_result.Data.ToQueryResultSet();

            MQD("-- Populate Shown Queues");
            if (show_queues.Count > 0) {
                foreach (QueryResultSetRecord showqueue_item in show_queues) {
                    if (m_multiSite) {
                        showqueues.Add(showqueue_item["prefixed_queue_name"], 1);
                    }
                    else {
                        showqueues.Add(showqueue_item["queue_name"], 1);
                    }
                }
            }

            // End hidden queue list processing

            ////
            // Build Subscribed Queues

            m_subscribedQueuesMain.Clear();  // FIXME: Special case for MAIN

            MQD("-- Build Subscribed Queues");

            foreach (QueryResultSetRecord queue_assignment in queue_assignments_and_prefs) {
                string queue_name;
                string queue_name_real = queue_assignment["queue_name"];
                string queue_longname = queue_assignment["queue_longname"];
                Boolean queue_is_aggregate = queue_assignment.ToBoolean("is_aggregate_queue");

                if (m_multiSite) {
                    queue_name = queue_assignment["prefixed_queue_name"];
                }
                else {
                    queue_name = queue_assignment["queue_name"];
                }

                MQD("--   {0}", queue_name);

                // If showqueues is blank, don't do any filtering
                if (showqueues.Keys.Count != 0) {
                    if (!showqueues.ContainsKey(queue_name) && !m_subscribedQueuesHidden.ContainsKey(queue_name)) {
                        m_subscribedQueuesHidden.Add(queue_name, 1);
                    }
                }

                if (!m_subscribedQueuesHidden.ContainsKey(queue_name)) {
                    if (m_subscribedQueues.ContainsKey(queue_name)) {
                        MQD("!!! ASSERTION FAILURE -- Tried subscribing to queue that was already subscribed: {0}", queue_name);
                    }
                    else {
                        // This queue is not hidden
                        m_subscribedQueues.Add(queue_name, new Hashtable());
                        m_subscribedQueues[queue_name].Add("HeadingText", queue_longname);
                        m_subscribedQueues[queue_name].Add("SystemName", "LOCAL");
                        m_subscribedQueues[queue_name].Add("SystemLongName", "LOCAL");
                        m_subscribedQueues[queue_name].Add("QueueNameReal", queue_name_real);
                        m_subscribedQueues[queue_name].Add("QueueIsAggregate", queue_is_aggregate);
                        m_subscribedQueues[queue_name].Add("IsMainSystem", true);
                        m_subscribedQueues[queue_name].Add("IsOtherSystem", false);
                    }
                }

                // Populate Preferences menu (inside the MonkeyWrench menu)
                // With queues to hide/show

                var item = new System.Windows.Forms.ToolStripMenuItem() {
                    Text = queue_longname
                };

                //                if ((string) queue_assignment["toolbar_show_queue"] == "T") {
                //                    item.Checked = true;
                //                }

                // TODO: We should keep track of user hide/show settings locally
                // TODO: If the toolbar exits, we lose the checked/unchecked queues that the user has changed for this session
                if (m_subscribedQueuesHidden.ContainsKey(queue_name)) {
                    item.Checked = false;
                }
                else {
                    item.Checked = true;
                }

                item.Click += HideShowQueueitem_Click;
                item.Tag = queue_assignment;

                displayQueuesToolStripMenuItem.DropDownItems.Add(item);

                // FIXME: Special case for MAIN
                if (m_subscribedQueues.ContainsKey(queue_name)) {
                    Hashtable subscribed_queue_hash = (Hashtable)m_subscribedQueues[queue_name];
                    m_subscribedQueuesMain[queue_name] = (Hashtable)subscribed_queue_hash.Clone();
                }
            }

            MQD("-- Sorting Queues List [1]");

            //
            // Sort the Subscribed queues... show MAIN Queues first

            var sorted_servers_list = new Dictionary<string, string>();
            var sorted_queues_list = new Dictionary<string, string>();

            foreach (KeyValuePair<string, ToolbarServerConnection> toolbar_server_item in m_toolbarServers) {
                string server_name = toolbar_server_item.Key;
                ToolbarServerConnection tsc = toolbar_server_item.Value;
                string server_order_item;

                if (tsc.m_isMainServer) {
                    // Main goes first
                    server_order_item = "0";
                }
                else {
                    // Then all the others, ordered by name first
                    server_order_item = "1";
                }

                sorted_servers_list.Add(
                    tsc.m_serverName,
                    String.Format("{0}-{1}", server_order_item, tsc.m_serverLongname) // This is what we are sorting on
                );
            }

            MQD("-- Sorting Queues List [2]");

            // We don't actually need to sort this, but we have a list of servers in order, just in case we need it in the future
            sorted_servers_list = sorted_servers_list.OrderBy(n => n.Value).ToDictionary(x => x.Key, x => x.Value);

            foreach (string queue_name in m_subscribedQueues.Keys) {
                Hashtable subscribed_queue = m_subscribedQueues[queue_name];
                string queue_longname = (string)subscribed_queue["HeadingText"];
                string server_name = (string)subscribed_queue["SystemName"];

                string server_sort_item = sorted_servers_list[server_name];

                sorted_queues_list.Add(
                    queue_name,
                    String.Format("{0}-{1}", server_sort_item, queue_longname) // This is what we are sorting on
                );
            }

            // This is the important sort... all queues in order by server, MAIN first
            m_subscribedQueuesOrder = sorted_queues_list.OrderBy(n => n.Value).ToDictionary(x => x.Key, x => x.Value);
            
            MQD("-- Populate Queue Positions");

            // So that when we do updateAgentMaxes(), we know what queue is in what position
            // (Index into the main grid rows)
            int i = 0;
            foreach (string subscribed_queue in m_subscribedQueuesOrder.Keys) {
                m_subscribedQueues[subscribed_queue]["MainGridRowPosition"] = i++;
            }
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
        private void dbRefreshWorker_DoWorkOnDB(object sender, DoWorkEventArgs e, ToolbarServerConnection tsc, ToolBarLiveData new_live_datas) {
            JsonHashResult backend       = tsc.m_iqc.GetServerTime();
            DateTime current_local_time  = DateTime.Now;

            if (backend.Success) {
                DateTime current_server_time = Utils.UnixTimeToDateTime(backend.Data.GetInt64("now"));
                tsc.m_serverTimeOffset       = (current_server_time - current_local_time);
            }

            SortedList<string, Hashtable> subscribed_queues = tsc.m_subscribedQueues;

            // QD.p(String.Format("dbRefreshWorker_DoWorkOnDB() Backend Pid: {0}", backend_pid["pg_backend_pid"]));

            // All data items are indexed by queue_name_prefixed
            Dictionary<string, List<OrderedDictionary>> live_caller_data       = getLiveCallerData(tsc);
            Dictionary<string, List<OrderedDictionary>> live_agent_data        = m_enableTeamView ? getLiveAgentData(tsc) : new Dictionary<string, List<OrderedDictionary>>();
            Dictionary<string, List<OrderedDictionary>> live_queue_data        = getLiveQueueData(tsc);
            Dictionary<string, List<OrderedDictionary>> agent_status_available = getAvailableStatusCodes(tsc);
            Dictionary<string, List<OrderedDictionary>> agent_call_data        = new Dictionary<string, List<OrderedDictionary>>();
            QueryResultSet agent_current_calls                                 = getCurrentCallsForAgent(tsc);

            //if (tsc.m_serverName.StartsWith("hum")) {
            //    var live_caller_data_r       = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(live_caller_data);
            //    var live_agent_data_r        = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(live_agent_data);
            //    var live_queue_data_r        = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(live_queue_data);
            //    var agent_status_available_r = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(agent_status_available);
            //    var agent_call_data_r        = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(agent_call_data);

            //    // if (m_MainDF.IsPaused()) { Debugger.Break(); }
            //}

            // Dictionary<string, string[]> agent_call_data = new Dictionary<string, string[]>();

            ////////////////////////////////////
            // We have the data, start using it
            ////////////////////////////////////

            ////
            // ScreenPop and Screen Recording Related

            // Only process this on the local system
            if (tsc.m_isMainServer) {
                screenPopIfNecessary(tsc);

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
                            stopScreenRecordingIfNecessary();

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

                                startScreenRecordingIfNecessary(current_call, tsc.m_serverTimeOffset);
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

        private void startScreenRecordingIfNecessary(QueryResultSetRecord currentCallData, TimeSpan serverTimeOffset)
        {
            if (!this.m_screenRecordingEnabled) {
                return;
            }

            List<string> textToAdd = new List<string>
            {
                "Testing",
                "Testing2",
                "Testing3"
            };

            string screen_recording_file_path = Path.GetTempPath() + "SCREEN.mp4";

            MQD("ScreenCapture -- Start Screen Recording: " + screen_recording_file_path);

            try
            {
                // Always overwrite for now... TODO, we should see if we still have a file left over and try and re-upload if we failed the last time

                this.m_screenRecord.RecordingStart(screen_recording_file_path,
                    // Callback for when recording is finished
                    delegate (IntellaScreenRecordingResult result) {
                        MQD("ScreenCapture -- End Screen Recording: " + screen_recording_file_path);
                        MQD("ScreenCapture - File Upload Start (CallLogID: {0} -- CallSegmentID: {1})", currentCallData["call_log_id"], currentCallData["call_segment_id"]);

                        MQD("Capture Start:    " + Utils.DateTimeToUnixTime(result.StartTime).ToString());
                        MQD("ServerTimeOffSet: " + serverTimeOffset.TotalSeconds.ToString());

                        // Starts a new thread behind the scenes
                        Utils.UploadFileToURL(result.RecordingFilePath, m_screenRecordingUploadURL + 
                            String.Format("?app=Toolbar&op=screencast/upload&token=abc&call_log_id={0}&call_segment_id={1}&capture_start_unixtime={2}", 
                            currentCallData["call_log_id"], 
                            currentCallData["call_segment_id"],
                            (Utils.DateTimeToUnixTime(result.StartTime) + serverTimeOffset.TotalSeconds).ToString()
                        ));

                        MQD("ScreenCapture - File Upload Complete (CallLogID: {0} -- CallSegmentID: {1})", currentCallData["call_log_id"], currentCallData["call_segment_id"]);

                        CanWeRestartAndUpdateSet(true);
                    }
                );

                //}, textToAdd);
            }
            catch (Exception ex)
            {
                MQD("ScreenCapture - File Capture Failed (CallLogID: {0} -- CallSegmentID: {1})", currentCallData["call_log_id"], currentCallData["call_segment_id"] + "\r\n" + ex.ToString());
            }
        }

        // The only reason we'll be stopping screen recording is if we're no longer in a call or the application is ending
        // This gets called every data cycle (generally 2 seconds)
        private void stopScreenRecordingIfNecessary()
        {
            if (this.m_screenRecord.IsRunning()) {
                this.m_screenRecord.RecordingStop();
                // RecordingStop event handler will CanWeRestartAndUpdateSet(true) once we know the file is done writing
            }
            else {
                if (!CanWeRestartAndUpdate()) {
                    CanWeRestartAndUpdateSet(true);
                }
            }
        }

        private Boolean screenPopIfNecessary(ToolbarServerConnection tsc) {
            if (!tsc.m_isMainServer) {
                // Only screenpops for the main server connection
                return false;
            }

            QueryResultSet backlog_items;

            backlog_items = tsc.m_db.DbSelect(@"
                SELECT
                    *
                FROM
                    live_queue.agents_event_backlog
                WHERE
                    agent_device = ?
                    AND event_when_unixtime > ?::numeric
                ORDER BY
                    event_when",

                m_agentDevice, m_lastAgentBackLogLastUnixTime
            );

            if (backlog_items.Count == 0) {
                return false;
            }

            JsonHash event_additional_data_json;
            JsonHash screenpop_data_json;

            string screen_pop_data_string = "";

            foreach (QueryResultSetRecord backlog_item in backlog_items) {
                m_lastAgentBackLogLastUnixTime = backlog_item["event_when_unixtime"];

                if (backlog_item["event_what"] == "CALL_START") {
                    event_additional_data_json = new JsonHash(backlog_item["event_additional_data_json"]);

                    string caller_channel_queue  = event_additional_data_json.GetStringOrEmpty("QueueCallerChannel");
                    string caller_channel_dialer = event_additional_data_json.GetStringOrEmpty("DialerCalleeChannel");

                    if ((caller_channel_queue == "") && (caller_channel_dialer == "")) {
                        continue;
                    }

                    string caller_channel = (caller_channel_queue != "") ? caller_channel_queue : caller_channel_dialer;

                    // TODO: We need a DbSelectSingleValue
                    screen_pop_data_string = tsc.m_db.DbSelectSingleValueString("SELECT live_queue.agent_screenpop_data(?,?,?) as screenpop_data", backlog_item["backlog_item_id"], m_agentDevice, caller_channel);

                    if (screen_pop_data_string == "") {
                        MQD("SELECT live_queue.agent_screenpop_data({0}','{1}','{2}') as screenpop_data", backlog_item["backlog_item_id"], m_agentDevice, caller_channel);
                        MQD("ScreenPop CALL_START with no active call to: {0}", caller_channel);

                        if (backlog_item.Exists("event_callerid_num") && (backlog_item["event_callerid_num"] != null) && (backlog_item["event_callerid_num"].Length < 10)) {
                            MQD("Note:  ScreenPops might be disabled server-side for less than 10-digit CallerID");
                        }

                        continue;
                    }

//                    if (Debugger.IsAttached) {
//                        DataDumperForm df = new DataDumperForm();
//                        df.Dumper("ScreenPop", screenpop_data);
//                    }

                    screenpop_data_json    = new JsonHash(screen_pop_data_string);
                    screen_pop_data_string = System.Uri.EscapeUriString(screen_pop_data_string);

                    // Full data related to the Screen Pop
                    m_lastScreenPopData = new JsonHash(new Hashtable() { { "screenpop_data", screenpop_data_json.GetHashTable() }, { "event_additional_data_json", event_additional_data_json.GetHashTable() } });
                    MQDU("LastScreenPopData", m_lastScreenPopData);

                    if (!m_screenPopsEnabled || (m_screenPopURL == "") || (m_screenPopURL == null)) {
                       return false; // Did everything else, but not screenpoping
                    }

                    // We're in the middle of a call now, we can't restart
                    CanWeRestartAndUpdateSet(false);

                    // MQDU("Screen Pop", screenpop_data_json);
                    MQDU("URL", String.Format("{0}?data={1}", m_screenPopURL, screen_pop_data_string));
                    System.Diagnostics.Process.Start(String.Format("{0}?data={1}", m_screenPopURL, screen_pop_data_string));
                }
            }

            // TODO: Keep this in sync with API Docs /intellasoft/CallQueue/docs and with info in live_queue.agent_screenpop_data
            //
            // The following fields and specs are the same as in GetAgentEventBackLog
            //   event_when                   String -  High precision timestamp of event as an ISO date string (with time zone offset from     UTC), Example: 2000-01-01 10:00:00.123456-04
            //   event_when_unixtime          String -  High precision unixtime of event, Example: 946738800.123456
            //   event_what                   String -  Type of event.  Current possible events include CALL_START/CALL_END
            //   agent_device                 String -  Device (extension) where the agent is logged in from for the agent related to the event
            //   agent_number                 Numeric - Agent Number (Agent Login) for the agent related to the event
            //   agent_id                     Numeric - Unique identifier for the agent related to the event
            //   session_id                   Numeric - Unique session id of the agent's session for the agent related to the event
            //   event_call_log_id            Numeric - Internal Unique call log id of the connected call that the event is related to (if any)
            //   event_case_number            String  - Case id of the related call (if any)
            //   event_callerid_name          String  - Name of the caller related to the event (if any)
            //   event_callerid_num           String  - Callerid number of the caller related to the event (if any)
            //   event_additional_data_json   String  - JSON encoded data related to the event (reserved for future expansion)
            //   event_userfield              String  - Current user defined data that has been attached to the call.  Best practice is to use JSON
            //
            // With the following additional fields:
            //   queue_name                 String  - Queue name token
            //   uniqueid                   Numeric - Related UniqueID of this call.  Internal tracking number
            //   call_log_id                String  - Internal Unique call log id of the connected call that the event is related to (if any)
            //   joined_when                String  - High precision timestamp of event as an ISO date string (with time zone offset from UTC),  Example: 2000-01-01 10:00:00.123456-04
            //   picked_up_when             String  - High precision timestamp of event as an ISO date string (with time zone offset from UTC),  Example: 2000-01-01 10:00:00.123456-04
            //   on_call_with_agent_device  String  - Device (extension) where the agent is logged in from for the agent related to the event
            //   on_call_with_agent_number  Numeric - Agent Number (Agent Login) for the agent related to the event
            //   on_call_with_agent_channel String  - Agent Channel that is/was currently talking to the caller
            //   department_name            String  - Specific department name within the queue that this caller called into
            //   department_num             String  - Specific department number within the queue that this caller called into
            //   queue_id                   Numeric - Unique identifier for the queue
            //   queue_dialer_channel       String  - Internal channel used for the agent dialer
            //   priority                   Numeric - Priority of this queue for this agent
            //   case_number                String  - Case number associated with this call
            //   call_segment_id            String  - Specific Call Segment within the Call Log ID of this call
            //   channel                    String  - Channel of the outside caller
            //   joined_when_unixtime       Numeric - Time in seconds since the "epoch" (standard UnixTime) for when the caller joined the queue
            //   waiting_seconds            Numeric - Time in seconds the caller waited in queue before getting answered by an agent
            //   waiting_time               String  - HH:MM:SS formatted time of how long the caller waited in queue before getting answered by an agent
            //
            // Backwards Compatability
            //   callerid_name               String - Name of the caller related to the event (if any)
            //   callerid_num                String - Callerid number of the caller related to the event (if any)
            //   agent_device                String - Device (extension) where the agent is logged in from for the agent related to the event
            //

            return true;
        }

        private void ScreenPop_LaunchLoginIfNeeded() {
            if (this.m_screenPop_LoginLaunched == true) {
                return;
            }

            this.m_screenPop_LoginLaunched = true;

            if (this.m_screenPopLoginUrl != "") {
                if (Debugger.IsAttached) {
                    MQD("[ScreenPop] -- Launch ScreenPopLoginURL Skipped -- DEV-MODE Debugger Attached");
                }
                else {
                    System.Diagnostics.Process.Start(this.m_screenPopLoginUrl);
                }
            }
        }

        ////
        // Set our status for all queues we are assigned to
        //
        // WARNING:  In this function: newAgentStatus is expected to be the longname, 
        // TODO: use .Tag on the combobox to store the agent status row so we can pass the status_code_name
        // 
        internal void setNewAgentStatus(string newAgentStatus) {
            MQD("Set Agent Status [Self]: {0}", newAgentStatus);

            try {
                QueryResultSetRecord set_status = m_main_db.DbSelectSingleRow(@"
                    SELECT
                    live_queue.agent_set_status(
                        {0},
                        (SELECT status_code_name FROM live_queue.v_agent_status_codes_self WHERE agent_device = {0} AND status_code_longname = {1} LIMIT 1)
                    )", m_agentDevice, newAgentStatus);

                MQD("Agent Set Status -- Status: {0}, Device: {1} -- Result: {2}", newAgentStatus, m_agentDevice, set_status["agent_set_status"]);
            }
            catch (Exception e) {
                handleError(e, "setNewAgentStatus(x) Failed");
                if (Debugger.IsAttached) { throw; }
            }
        }

        internal void setNewAgentStatus(string newAgentStatus, string agentDevice) {
            MQD("Set Agent Status [{0}]: {1}", agentDevice, newAgentStatus);

            try {
                QueryResultSetRecord set_status = m_main_db.DbSelectSingleRow("SELECT live_queue.agent_set_status(?,?)", agentDevice, newAgentStatus);
                MQD("Agent Set Status -- Status: {0}, Device: {1} -- Result: {2}", newAgentStatus, agentDevice, set_status["agent_set_status"]);
            }
            catch (Exception e) {
                handleError(e, "setNewAgentStatus(x,x) Failed");
                if (Debugger.IsAttached) { throw; }
            }
        }

        // Note: make sure to pass in the *real* queue name, not the prefixed one
        public void setNewAgentStatus(string newAgentStatus, string agentDevice, string queueName) {
            if (agentDevice == null) {
                agentDevice = m_agentDevice;
            }

            MQD("Set Agent Status [{0}]: {1} (Queue: {2}", agentDevice, newAgentStatus, queueName);

            try {
                QueryResultSetRecord set_status = m_main_db.DbSelectSingleRow("SELECT live_queue.agent_set_status(?,?,?)", agentDevice, newAgentStatus, queueName);
                MQD("Agent Set Status -- Status: {0}, Device: {1}, Queue: {2} -- Result: {3}", newAgentStatus, agentDevice, queueName, set_status["agent_set_status"]);
            }
            catch (Exception e) {
                handleError(e, "setNewAgentStatus(x,x,x) Failed");
                if (Debugger.IsAttached) { throw; }
            }
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

        ////
        // This is called from a worker thread that has a failed db connection
        // Flag that we're going to try to reconnect and start again
        //   so that the main thread can do a full reconnect
        //
        private void Error_DatabaseMainConnection_Failed() {
            // If since last update was a while ago, reset the error count
            TimeSpan since_last_error = DateTime.Now - this.m_program_failure_last;

            if (since_last_error.TotalMinutes > 2) {
                this.m_program_failures = 0;
            }

            this.m_program_failures++;
            this.m_program_failure_last = DateTime.Now;
            this.m_db_reconnecting      = true;
            this.m_db_active            = false;

            MQD("!!! Attempting to reconnect to database and resume operations (Program Failures: {0})", this.m_program_failures.ToString());
            SetStatusBarMessage(Color.Red, "Connection interrupted.  Reconnecting...");
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

        /// <summary>
        /// Validate the admin password for when clicking on the Wrench Menu -> Admin
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool AdminPasswordValidate(string password) {
            if ((password == "intelladmin")) {
                return true;
            }

            return false;
        }

        public void ShowAdminSettings() {
            StopRefreshTimer();
            this.TopMost = false;

            m_QueueAppConfigMainForm = new QueueAppConfigMainForm();
            m_QueueAppConfigMainForm.SetErrorCallback(m_errorHandler);
            m_QueueAppConfigMainForm.SetSuccessCallback(ShowAdminSettingsSuccess);
            m_QueueAppConfigMainForm.SetApplicationExitCallback(ApplicationExit);
            m_QueueAppConfigMainForm.Show();

            //m_mainHelper = new QueueAppConfigMainFormHelper(m_db);
            //m_mainHelper.SetPopupLoginCallback(checkAgentLogin);
            //m_mainHelper.SetSuccessCallback(ShowAdminSettingsSuccess);
            //m_mainHelper.popupServerSettingsForm(m_cm);
        }

        public void ResetApplicationAndCheckAgentLogin() {
            ClearToolbarData();
            StopRefreshTimer();

            if (this.m_db_reconnecting) {
                // Do a 'hidden' login, use existing credentials and only pop if there's an issue.
                AgentLoginDialogForm.ValidationCode login_result = agentLoginValidate(m_agentExtension, m_agentNum, m_agentPin);
                if (login_result == AgentLoginDialogForm.ValidationCode.Success) {
                    AgentLoginSuccess();
                }

                return;
            }

            string agentNum       = Interaction.GetSetting("IntellaToolBar", "Config", "USER_agentNumber");
            string agentExtension = Interaction.GetSetting("IntellaToolBar", "Config", "USER_agentExtension");
            string agentPin       = Interaction.GetSetting("IntellaToolBar", "Config", "USER_agentPin");
            string db_host        = Config_Get_DB_Host();

            m_agentAutoLogin = Utils.StringToBoolean(Interaction.GetSetting("IntellaToolBar", "Config", "USER_AutoLogin"));

            if (m_agentAutoLogin) {
                AgentLoginDialogForm.ValidationCode validate_result = agentLoginValidate(agentExtension, agentNum, agentPin);

                if (validate_result == AgentLoginDialogForm.ValidationCode.Success) {
                    AgentLoginSuccess();
                    return;
                }

                // Otherwise if we have some sort of failure then we'll need to ensure we get a new login
            }

            AgentLogin_Prompt();
        }

        private bool agentExtensionValidate(string extension) {
            Match m = Regex.Match(extension, @"\d+$");
            if (!m.Success) // ends in a number
                return false;

            return true;
        }

        public AgentLoginDialogForm.ValidationCode agentLoginValidate(string agentExtension, string agentNumber, string agentPin) {
            AgentLoginDialogForm.ValidationCode validationResult = AgentLoginDialogForm.ValidationCode.Fail;
            bool extensionValidated = false;

            extensionValidated = agentExtensionValidate(agentExtension);
            if (!extensionValidated) {
                Interaction.SaveSetting("IntellaToolBar", "Config", "USER_agentExtension", "");
                return AgentLoginDialogForm.ValidationCode.BadExt;
            }

            if (agentNumber == "") {
                Interaction.SaveSetting("IntellaToolBar", "Config", "USER_agentNumber", "");
                return AgentLoginDialogForm.ValidationCode.NeedNumber;
            }

            if (agentPin == "") {
                Interaction.SaveSetting("IntellaToolBar", "Config", "USER_agentPin", "");
            }

            m_isManager_MontorEnabled = false;

            QueryResultSetRecord config_result;

            // Boolean dbIsManagerMonitorEnabled;
            string dbAgentPin;

            string connect_to_host = Config_Get_DB_Host();
            // connect_to_host = "comm"

			m_iqc_login = m_iqc.CreateAgentConnection(connect_to_host, "IntellaQueue", "IntellaQueue", "443", agentNumber, agentExtension);
            if (!m_iqc_login.success) {
                MessageBox.Show("Login was not successful: \n" + m_iqc_login.reason);

                return AgentLoginDialogForm.ValidationCode.Fail;
            }

            JsonHash module_info  = m_iqc_login.agent_data.GetItem("module_info");
            JsonHash agent_info   = m_iqc_login.agent_data.GetItem("agent_info");
            JsonHash queue_tenant = m_iqc_login.agent_data.GetItem("queue_tenant");

            m_agentExtension = agentExtension;
            m_agentNum       = agentNumber;

            m_tenantName = agent_info.GetString("tenant_name");

            m_agentDevice             = agent_info.GetString("device");
            m_agentName               = agent_info.GetString("agent_firstname") + " " + agent_info.GetString("agent_lastname");
            m_agentID                 = agent_info.GetString("agent_id");
            dbAgentPin                = agent_info.GetString("agent_pin");
            m_screenPopsEnabled       = agent_info.GetBool("screenpops_enabled");
            m_isManager               = agent_info.GetBool("is_manager");
            m_isManager_MontorEnabled = m_isManager;

            m_screenRecordingEnabled   = queue_tenant.GetBool("toolbar_screen_recording_enabled");
            m_screenRecordingUploadURL = queue_tenant.GetString("toolbar_screen_recording_upload_url");
            m_allowToolbarClose        = queue_tenant.GetBool("toolbar_allow_close");
            m_agentAutoLogin           = queue_tenant.GetBool("toolbar_auto_login");
            m_hideInterface            = queue_tenant.GetBool("toolbar_hide_interface");

            // When we re-launch we'll need a local setting saved so we know what's up with autologin
            Interaction.SaveSetting("IntellaToolBar", "Config", "USER_AutoLogin", m_agentAutoLogin.ToString());

            // Login Details Populated

            if (dbAgentPin == null) { 
                dbAgentPin = "";
            }

            if ((dbAgentPin == "") || (agentPin == dbAgentPin)) {
                validationResult = (int) AgentLoginDialogForm.ValidationCode.Success;
                m_agentPin       = dbAgentPin;

                Interaction.SaveSetting("IntellaToolBar", "Config", "USER_agentNumber",    agentNumber);
                Interaction.SaveSetting("IntellaToolBar", "Config", "USER_agentExtension", agentExtension);
                Interaction.SaveSetting("IntellaToolBar", "Config", "USER_agentPin",       agentPin);            // Not the bet approach... we only need to save the pin if we're doing autologinch
            }
            else if (agentPin == "") { 
                validationResult = AgentLoginDialogForm.ValidationCode.NeedPin;
            }

            // FIXME: Hacky
            /*
            if (m_agentAutoLogin) {
                validationResult = AgentLoginDialogForm.ValidationCode.Success;;
            }
            */

            config_result = m_main_db.DbSelectSingleRow("SELECT * FROM queue.toolbar_config");

            // TODO:  We should only be setting globals if we're completely successfully logged in (pin validation successful as well if there is one)
            m_isDeveloper                   = agent_info.GetBool("is_developer");
            m_enableTeamView                = config_result.ToBoolean("team_view");
            m_showAgentNumberInTitleBar     = config_result.ToBoolean("display_agent_num_titlebar");
            m_displayCallerDetailNonManager = config_result.ToBoolean("display_caller_detail_non_manager");
            m_multiSite                     = config_result.ToBoolean("multi_site");
            m_currentCallEnabled            = config_result.ToBoolean("current_call_enabled");
            m_dispositionCodesEnabled       = config_result.ToBoolean("disposition_codes_enabled");

            if (m_isManager) {
                m_enableTeamView = true;
            }

            // Screen Pops

            if (m_screenPopsEnabled) {
                m_screenPopURL      = config_result["screenpop_url"];
                m_screenPopLoginUrl = config_result["screenpop_url_login"];

                MQD("Configured ScreenPop URL: " + m_screenPopURL);
            }

            if (module_info.Exists("Dialer")) {
                this.m_mainServerHasDialer = true;
            }

            // -----------------------------------------------------------------------------
            // TODO: Could perhaps have this as an additional callback
            // -----------------------------------------------------------------------------

            return validationResult;
        }

        private void refresh() {
            //Debug.Print("REFRESH!");
            //drawForm();
            SetupMainGrid();
            setupGrids();
            Debug.Print("REFRESH!--");
            UpdateQueueDisplayAndCatchExceptions();
            TrimAdjustAndPositionAllDropGrids();
        }

        // NEW - Connect to the other (s after agent is successfully logged in
        public void AgentOtherServersConnect() {  
            QueryResultSet db_monitor_other_servers;
            QueryResultSet db_monitor_other_queues;

            if (!m_multiSite) {
                return;
            }

            // Debug.Print("DFDF");
            // Debug.Print(String.Format("server:{0}, name:{1}, addr:{2}", server["server_name"], server["server_longname"], server["server_addr"]));
            // DataDumperForm df = new DataDumperForm();
            // df.Dumper("db_monitor_other_servers", db_monitor_other_servers);
            // df.Dumper("db_monitor_other_queues", db_monitor_other_queues);

            // db_monitor_other_servers[#]["server_name"]  ["server_longname"]  ["server_addr"]
            //                         [0]  passaic          Passaic             acs-passaic.client.intellasoft.local (192.168.1.201)
            //                         [1]  hudson           Hudson              acs-hudson.client.intellasoft.local  (192.168.2.201)
            db_monitor_other_servers = m_main_db.DbSelect(@"
                SELECT   
                -- TODO: For us testing, we use server_addr_external and for production it's server_addr
                DISTINCT
                    r.server_name,
                    r.server_longname,
                    r.server_addr,
                    r.server_addr_vpn,
                    r.server_addr_external,
                    r.db_username,
                    r.db_password
                FROM
                    queue.toolbar_config_agent_server_monitor c
                    JOIN public.additional_reporting_servers r USING (server_id)
                WHERE
                    c.agent_num = ?
                ",
                m_agentNum
            );

            // db_monitor_other_queues[#]["server_name"]  ["queue_name"]
            //                        [0]  hudson          carpet_scheduling
            //                        [1]  hudson          renovations_customer_service
            //                        [2]  hudson          vermont_schedule
            //                        [3]  passaic          carpet_customer_service
            //                        [4]  passaic          carpet_scheduling
            //                        [5]  passaic          hard_surface_scheduling
            //                        [6]  passaic          kitchens_nhance
            db_monitor_other_queues = m_main_db.DbSelect(@"
                SELECT
                    r.server_name,
                    c.queue_name,
                    r.server_name || '-' || queue_name as prefixed_queue_name
                FROM
                    queue.toolbar_config_agent_server_monitor c
                    JOIN public.additional_reporting_servers r USING (server_id)
                WHERE
                    c.agent_num = ?
                ORDER BY
                    r.server_name,
                    c.queue_name", m_agentNum
            );

            // m_otherServers["hudson"]  = new Server("Hudson")
            // m_otherServers["passaic"] = new Server("Passaic")
            foreach (QueryResultSetRecord server in db_monitor_other_servers) {
                MQD(String.Format("Additional Server Monitor: {0}", server["server_name"]));

                // m_toolbarConfig = Pass on our global toolbar configuration (AgentDevice, IsManager, and other global toolbar settings)
                ToolbarServerConnection tsc   = new ToolbarServerConnection(server["server_name"], server["server_longname"], m_toolbarConfig);
                string server_name            = server["server_name"];
                m_toolbarServers[server_name] = tsc;

                SimpleDbConnectionParameters connection_params = m_main_db.cloneConnectionParameters(server["server_addr"]);
                connection_params.Host     = server["server_addr"];
                connection_params.User     = "root";          // server["db_username"]; // FIXME.  https://stackoverflow.com/questions/12712235/create-postgresql-9-role-with-login-user-just-to-execute-functions
                connection_params.Password = "postgresadmin"; // server["db_password"];
                
                if (Debugger.IsAttached || Dns.GetHostName().StartsWith("Win7") || Dns.GetHostName().StartsWith("vbox")) {
                    connection_params.Host = server["server_addr_external"];
                }

                PingReply ping = Utils.PingHost(connection_params.Host);
                if (ping.Status == IPStatus.Success) {
                    MQD("  Other Server Ping Successful: {0}", connection_params.Host);
                }
                else {
                    MQD("  Other Server Ping NOT Successful: {0}", connection_params.Host);

                    if (server["server_addr_vpn"] != "") {
                        MQD("Try VPN Address for: {0} -- VPN Addr: ", server["server_addr_vpn"]);

                        ping = Utils.PingHost(server["server_addr_vpn"]);
                        if (ping.Status != IPStatus.Success) {
                            MQD("  Other Server VPN Ping NOT Successful: {0}", connection_params.Host);
                            // continue;  FIXME... we should skip this, but we can't with the current design, since db_monitor_other_queues is already populated
                        }
                        else {
                            connection_params.Host = server["server_addr_vpn"];
                        }
                    }
                    else {
                        MQD("  No alternative address to try, skippiing");
                    }
                }

                tsc.m_db = new DbHelper(handleErrorWithStackTrace);
                tsc.m_db.SetErrorCallback(DatabaseErrorCallbackFunction);
                tsc.InitConnection(connection_params);
                tsc.Connect();
            }

            // m_otherServers["hudson"].m_subscribedQueues
            //   ["carpet_scheduling"]           ["HeadingText"] == "Carpet Sch"
            //   ["renovations_customer_service"]["HeadingText"] == "Renovations CS"
            //   ["renovations_scheduling"]      ["HeadingText"] == "Renovations Scheduling"
            //
            // m_otherServers["passaic"].m_subscribedQueues
            //   ["carpet_customer_service"]["HeadingText"] == "Carpet CS"
            //   ["carpet_scheduling"]      ["HeadingText"] == "Carpet Sch"
            //   ["hard_surface_scheduling"]["HeadingText"] == "Hard Surface Sch"
            //   ["kitchens_nhance"]        ["HeadingText"] == "Kitchen Installation"
            foreach (QueryResultSetRecord queue in db_monitor_other_queues)
            {
                string queue_name         = queue["prefixed_queue_name"];
                string queue_name_real    = queue["queue_name"];
                string server_name        = queue["server_name"];

                if (!m_toolbarServers.ContainsKey(server_name)) {
                    continue;
                }

                ToolbarServerConnection srv = m_toolbarServers[server_name];
                QueryResultSet db_queue_info;

                db_queue_info = srv.m_db.DbSelect(@"
                      SELECT                               queue_longname, 
                               0                        as queue_is_aggregate  -- TODO: Should this be NULL or FALSE, what is best?
                      FROM     queue.queues
                      WHERE    queue_name = ?
                    UNION
                      SELECT   queue_aggregate_longname as queue_longname, 
                               1                        as queue_is_aggregate  -- TODO: Should this be TRUE, what is best?
                      FROM     queue.queue_aggregates
                      WHERE    queue_aggregate_name = ?
                ", srv.m_serverLongname, queue_name_real);

                if (db_queue_info.Count == 0) {
                    MessageBox.Show("Invalid remote queue: " + server_name + "Queue Name: " + queue_name_real);
                }
                else if (db_queue_info.Count > 1) {
                    MessageBox.Show("Duplicate remote queue: " + server_name + "Queue Name: " + queue_name_real);
                }
                else {
                    // Private Per-Server subscribed queues so we know which queues on a per-connection basis we're monitoring
                    srv.m_subscribedQueues.Add(queue_name, new Hashtable());
                    srv.m_subscribedQueues[queue_name].Add("HeadingText",      db_queue_info[0]["queue_longname"]);
                    srv.m_subscribedQueues[queue_name].Add("SystemName",       srv.m_serverName);
                    srv.m_subscribedQueues[queue_name].Add("SystemLongName",   srv.m_serverLongname);
                    srv.m_subscribedQueues[queue_name].Add("QueueNameReal",    queue_name_real);
                    srv.m_subscribedQueues[queue_name].Add("QueueIsAggregate", db_queue_info[0].ToBoolean("queue_is_aggregate"));
                    srv.m_subscribedQueues[queue_name].Add("IsMainSystem",     false);
                    srv.m_subscribedQueues[queue_name].Add("IsOtherSystem",    true);

                    // Main subscribed queues list for the gui grid
                    m_subscribedQueues.Add(queue_name, new Hashtable());
                    m_subscribedQueues[queue_name].Add("HeadingText",      db_queue_info[0]["queue_longname"]);
                    m_subscribedQueues[queue_name].Add("SystemName",       srv.m_serverName);
                    m_subscribedQueues[queue_name].Add("SystemLongName",   srv.m_serverLongname);
                    m_subscribedQueues[queue_name].Add("QueueNameReal",    queue_name_real);
                    m_subscribedQueues[queue_name].Add("QueueIsAggregate", db_queue_info[0].ToBoolean("queue_is_aggregate"));
                    m_subscribedQueues[queue_name].Add("IsMainSystem",     false);
                    m_subscribedQueues[queue_name].Add("IsOtherSystem",    true);
                }
            }
        }

        public void AgentLoginSuccess() {
            // try { 

            _AgentLoginSuccess();

            this.m_IsAgentLoggedIn = true;

            // }
            // catch (Exception ex) {
            //     MessageBox.Show(ex.StackTrace.ToString());
            // }
        }

        public void _AgentLoginSuccess() {
            // FIXME: We're still using a postgres connection!  Repoint to the agent_specific server if we have one
            string agent_specific_server = m_iqc_login.agent_data.GetString("agent_specific_server");

            // Switch the db connection
            if (agent_specific_server != null) {
                ToolBarHelper.DatabaseSetupCheck(ref m_main_db, agent_specific_server, null, StartupConfigurationFailure);
            }

            if (m_hideInterface) {
              // Note: Why would we want to hide the interface you ask?
              // If we're using the toolbar for sole purpose of screen recording during calls, then we'll just hide the main app and go systray-only

              this.ShowInTaskbar = false; // No longer in the bottom taskbar
              this.Hide();                // Bye bye interface!
            }

            ConfigurationSuccess();

            // So that the ExtraControl properly rebuilds
            if (this.m_extraControl != null)
            {
                this.m_extraControl.Dispose();
                this.m_extraControl = null;

                m_extraControlHeight = 0;
            }

            string agent_num       = Interaction.GetSetting("IntellaToolBar", "Config", "USER_agentNumber");
            string agent_extension = Interaction.GetSetting("IntellaToolBar", "Config", "USER_agentExtension");

            ScreenPop_LaunchLoginIfNeeded();

            titleViewNameLabel.Text = (m_isManager ? "Manager" : "Agent") + " Toolbar - (" + agent_num + ") " + m_agentName;

            // notify db agent is logged in
            //
            // this is the first real DB call done during post-login
            // Avoid blocking the gui thread when we have errors from an updater thread

            int attempts = 5;
            int success = 0;

            using (TryLock try_lock = new TryLock(m_main_db)) {
                while (attempts-- > 0) { 
                    if (try_lock.GetLock()) {
                       QueryResultSet agent_set_extension = m_main_db.DbSelectFunction("queue.set_agent_extension", agent_num, agent_extension);

                       if (agent_set_extension.Count > 0) {
                            QueryResultSetRecord r = agent_set_extension[0];
                            string result = r["set_agent_extension"];

                            if (result == "1") {
                                 success = 1;
                            }
                       }

                       attempts = 0;
                       break;
                    }

                    Thread.Sleep(1000);
                }
            }

            if ((success == 0) || (attempts == -1)) {
                MessageBox.Show("Call Center: Toolbar Already Logged In Elsewhere -- AgentID: " + agent_num);
                ApplicationExit();
                return;
            }

            MQD("Feature: StatusCodes");

            if (m_enableTeamView) {
                MQD("Feature: TeamView Enabled");
                m_statusCodesForm = new StatusCodesForm(m_subscribedQueues, m_isManager);
            }
            else {
                m_statusCodesForm = new StatusCodesForm(); // One status action for all queues (set all queues to the selected status)
            }

            if (m_dispositionCodesEnabled) {
                MQD("Feature: Dispositions Enabled");
                m_dispositionForm = new DispositionCodesForm(this);
            }

            if (m_currentCallEnabled) {
                MQD("Feature: CurrentCall Enabled");
                m_currentCallForm = new CurrentCallForm();
            }

            if (m_screenPopsEnabled) {
                MQD("Feature: Screenpops Enabled");
            }

            // We (AgentLoginSuccess) can get called multiple times during the application life.. start this menu from scratch every time
            this.debugToolStripMenuItem.DropDownItems.Clear();

            if (m_isManager)
            {
                MQD("Feature: Manager Enabled");

                this.debugToolStripMenuItem.Enabled = true;

                // Rename 'Show Debug Window' to 'Debug'
                // 
                this.debugToolStripMenuItem.Text = "Debug"; 
                
                this.cmpCallQueueSnapshotToolStripItem = this.debugToolStripMenuItem.DropDownItems.Add("CallQueue Snapshot", null, (sender, args) =>
                {
                    // cmpCallQueueSnapshotToolStripItem OnClick Handler
                    QueryResultSet result_data = m_main_db.DbSelectFunction("queue.save_queue_snapshot");

                    string message = "Failed.  Unknown Error";

                    if (result_data.Count > 0) {
                        message = (string)result_data[0]["save_queue_snapshot"];
                        message += "\n\nUse Ctrl-C to copy this message\nYou will need this ID number for technical support";
                    }

                    MessageBox.Show(message);
                });

                // We're migrating 'Show Debug Window' to inside 'Debug' since managers have more debug functions
                this.debugToolStripMenuItem.DropDownItems.Add("Show Debug Window", null, debugToolStripMenuItem_Click);

                if (m_isDeveloper && m_screenPopsEnabled)
                {
                    this.debugToolStripMenuItem.DropDownItems.Add("Set Screenpop URL", null, (sender, args) =>
                    {

                        Hashtable data = new Hashtable {
                            {"Title", "Set Screenpop URL"},
                            {"Controls", new Dictionary<string, Hashtable> {
                                    {
                                        "heading", new Hashtable() {
                                            {"Type", "Label"},
                                            {"Label", "Current:" + m_screenPopURL}
                                        }
                                    },

                                    {
                                        "url", new Hashtable() {
                                            {"Type", "TextBox"},
                                            {"Label", "New URL"},
                                               {"Value", m_screenPopURL}
                                        }
                                    }
                                }
                            }
                        };

                        PopupQuery screenpop_url = new PopupQuery(data);
                        DialogResult result = screenpop_url.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            Hashtable values = (Hashtable)data["Values"];
                            string new_url = (string)values["url"];

                            MQD("Set New Toolbar URL: " + new_url);
                            m_screenPopURL = new_url;
                        }
                    });
                }
            }

            if (m_currentCallEnabled)
            {
                // Move widgets from StatusCodesForm
                m_currentCallerPanel = m_currentCallForm.GetMainPanel();

                m_currentCallCenteringPanel = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(this.Width, m_currentCallerPanel.Height),
                    // AutoSize     = true,
                    // AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                    // BorderStyle  = BorderStyle.Fixed3D, // For Debug
                };

                m_currentCallForm.SetIntellaQueueForm(this);
                m_currentCallerPanel.Location = new Point(0, 0);

                CreateOrUpdateExtraControl(m_currentCallerPanel.Height);

                this.m_extraControl.Controls.Add(m_currentCallCenteringPanel);
                this.m_currentCallCenteringPanel.Controls.Add(m_currentCallerPanel);

                this.m_currentCallerPanel.Paint += delegate (object sender, PaintEventArgs e) {
                    Panel p = (Panel)sender;

                    p.Parent.Width = this.Width; // Toolbar width
                    p.Parent.Height = p.Height;  // Caller Panel height

                    int total_padding = p.Parent.Size.Width - p.Size.Width;
                    int padding_left = total_padding / 2;

                    p.Location = new Point(padding_left, 0);
                    p.Anchor = AnchorStyles.None;
                };
            }

            if (m_dispositionCodesEnabled)
            {
                // Move widgets from StatusCodesForm
                m_dispositionPanel = m_dispositionForm.GetDispositionCodesPanel();

                m_dispositionCenteringPanel = new Panel {
                    Location     = new Point(0, m_extraControlHeight),
                    Size         = new Size(this.Width, m_dispositionPanel.Height),
                    // AutoSize     = true,
                    // AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                    // BorderStyle  = BorderStyle.Fixed3D, // For Debug
                };
 
                m_dispositionPanel.Location = new Point(0, 0);

                CreateOrUpdateExtraControl(m_dispositionPanel.Height);
 
                this.m_extraControl.Controls.Add(m_dispositionCenteringPanel);
                this.m_dispositionCenteringPanel.Controls.Add(m_dispositionPanel);
 
                this.m_dispositionPanel.Paint += delegate (object sender, PaintEventArgs e) {
                    Panel p = (Panel) sender;

                    p.Parent.Width = this.Width; // Toolbar width
                    p.Parent.Height = p.Height;  // Disposition Panel height

                    int total_padding = p.Parent.Size.Width - p.Size.Width;
                    int padding_left = total_padding / 2;

                    p.Location = new Point(padding_left, 0);
                    p.Anchor = AnchorStyles.None;
                };

                this.m_dispositionForm.PopulateDispositionsData();
            }

            // Status Codes Control Creation
            //
            // Create a panel to be the parent of all the components that make up StatusCodesForm
            //  and 'drop in' StatusCodesForm into the main toolbar
            // 
            // TODO: This needs to be a dropdown for each queue... and then a set-all at the bottom of the toolbar
            if (m_statusCodesEnabled) {
                // Move widgets from StatusCodesForm
                m_statusCodesPanel = m_statusCodesForm.GetPauseCodesPanel();

                m_statusCodesCenteringPanel = new Panel {
                    Location     = new Point(0, this.m_extraControlHeight), // (x,y) Start after any other previously created 'extra' controls
                    Size         = new Size(this.Width, m_statusCodesPanel.Height),
                    AutoSize     = true,
                    AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                    // BorderStyle  = BorderStyle.Fixed3D, // For Debug
                };

                m_statusCodesForm.SetIntellaQueueForm(this);
                m_statusCodesPanel.Location = new Point(0, 0);

                CreateOrUpdateExtraControl(m_statusCodesPanel.Height);

                this.m_extraControl.Controls.Add(m_statusCodesCenteringPanel);
                this.m_statusCodesCenteringPanel.Controls.Add(m_statusCodesPanel);
            }

            ResizeMainGrid(true); // will also resize form and layoutForm

            SetStatusBarMessage(Color.Green,"Successfully Connected");
        }

        private void ShowAdminSettingsSuccess(string message) {
            Debug.Print("ShowAdminSettingsSuccess");

            // Show agent login dialogue
            // Verify agent login
            // If login success, start db refresh timer
            ResetApplicationAndCheckAgentLogin();
        }


        //----------------------------------------------------------------------

        public void ApplicationExit() {
            IntellaQueueForm.ApplicationExitStatic();
        }

        public static void ApplicationExitStatic() {
            if (Thread.CurrentThread != IntellaQueueForm.GetGuiThread()) {
                IntellaQueueForm.IntellaQueueFormApplicationForm.Invoke((MethodInvoker) delegate() { IntellaQueueFormApplicationForm.ApplicationExitInvoke(); });
                return;
             }

            IntellaQueueFormApplicationForm.ApplicationExitInvoke();
        }

        /// <summary>
        /// Actual application exit, object-specific cleanup
        /// </summary>
        public void ApplicationExitInvoke() {
            m_screenRecord.RecordingStop();

            while (m_screenRecord.IsRunning()) {
                 MQD("Waiting for screen recording to stop");
                 System.Threading.Thread.Sleep(1000);
            }

            this.m_mainTSC.Cancel = true;

            foreach (KeyValuePair<string, ToolbarServerConnection> item in this.m_toolbarServers) {
                ToolbarServerConnection tsc = item.Value;
                tsc.Cancel = true;

                // FIXME: .Join here will lock indefinitely even though the thread is ended ??
                // tsc.m_operatingThread.Join();

                while (tsc.m_operatingThread != null) {
//                  Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
//                  Console.WriteLine(this.m_mainTSC.m_operatingThread.ManagedThreadId);
                  System.Threading.Thread.Sleep(500);
                }
            }

            // Make sure we don't continue to display the SysTray Icon after closing
            cmpSysTrayIcon.Dispose();

            Application.Exit();
        }

        // Event Handlers
        // These have to be in IntellaQueueForm.cs
        //   otherwise Visual Studio cannot find them when trying to do a jump from the 'Events' editor in the Form Builder
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e) {
            AboutWindowShow();
        }
    }

    internal class IntellaQueueToolbarMissingPerQueueConfigData : Exception {
        public IntellaQueueToolbarMissingPerQueueConfigData(string message) : base(message) {

        }
    }
}
