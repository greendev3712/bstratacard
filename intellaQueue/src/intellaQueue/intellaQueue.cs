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
        private bool m_wyUpdateCheckManual = true; // First one is a yes.. Output the results to the statusbar if we're up to date
        private TimeSpan m_wyUpdateCheckInterval = TimeSpan.FromHours(1);
        private DateTime m_wyUpdateLastCheck = DateTime.Now;
        private System.Windows.Forms.Timer m_wyUpdate_Timer;

        /*
        private static readonly ILog log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );
        */

        private static Boolean logFileEnabled = false;
        private static Boolean m_logFileReady = false;
        private static string m_logFileDirectory = Path.Combine(Path.GetTempPath(), "IntellaQueue");
        private static string m_logFileBase = Path.Combine(m_logFileDirectory, "log");
        private static StreamWriter m_logFileHandle = null;
        private static DateTime m_logFileOpenedWhen;
        private static int m_LogFilesKeep = 3;

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
        private Boolean m_toolbarUpdating = false;
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
        private int m_databaseFailures = 0;

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
        private Boolean m_ExceptionPerQueueConfigThrown = false;
        private static DataDumperForm m_MainDF;

        // Grids
        //---------------
        // mainGrid (defined in intellaQueue.Designer.cs) -- Top Grid -- The Per-Queue Overall realtime data
        // 
        // Queue Callers and Agent Status grids are built in setupGrids()

        //----------------------------------------------------------------------

        public static Thread GetGuiThread() {
            return m_guiThread;
        }


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

        // Main QuickDebug (With logging)
        public static string MQD(string msg, params string[] msgFormat) {
            string final_log_line = MQD_NL(msg, msgFormat);

            if (m_logFileReady) {
                try {
                    m_logFileHandle.Write(final_log_line);
                    m_logFileHandle.Flush();
                }
                catch (Exception e) {
                    m_MainDF.D("!!! Could not write to log file: " + e.ToString());
                    m_logFileReady = false;
                }
            }

            return final_log_line;
        }

        // Main QuickDebug (Without logging)
        public static string MQD_NL(string msg, params string[] msgFormat) {
            string log_prefix = QD.GenerateLogLine_WthoutTimestamp();
            string log_line;

            if (msgFormat == null) {
                log_line = (log_prefix + " " + msg);
            }
            else {
                log_line = (log_prefix + " " + String.Format(msg, msgFormat));
            }

            return m_MainDF.D(log_line);
        }

        // Main QuickDebug with Dumper
        private void MQDU(string desc, object thing) {
            string log_prefix = QD.GenerateLogLine();
            m_MainDF.Dumper(log_prefix + " " + desc, thing);
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

            try {
                UploadLog(log_file);
            }
            catch (Exception e) {
                MQD("Exception while uploading log file: {0}.  Exception: {1}", log_file, e.StackTrace.ToString());
                // if (Debugger.IsAttached) { throw; }
            }

            TryOpenAndRotateLog(log_file);

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
        }

        public DbHelper GetDatabaseHandle() { return this.m_mainTSC.m_db; }
        public CurrentCallForm GetCurrentCallForm() { return this.m_currentCallForm; }
        public JsonHash GetLastScreenPopData() { return this.m_lastScreenPopData; }

        private void StartupConfigurationFailure(string message) {
            TopMost = false;
            MessageBox.Show(this, "Configuration not successful, toolbar will exit");
            ApplicationExit();
        }

        private void CreateOrUpdateExtraControl(int additionalControlHeight) {
            if (this.m_extraControl == null) {
                m_extraControl = new Panel();
                this.Controls.Add(m_extraControl);
                this.m_extraControl.Padding = new System.Windows.Forms.Padding(10);
            }

            this.m_extraControlHeight += additionalControlHeight;

            // Extra space on the bottom for more controls
            m_extraControl.Size = new Size(this.Width, m_extraControl.Size.Height + additionalControlHeight);
        }

        private void handleDatabaseSuccess() {
            this.m_databaseFailures = 0;
        }

        // Called every time there is a query failure
        // If we fail too many times in a row, then try to reconnect
        // If reconnecting has failed... we must bail
        //
        private void handleDatabaseFailure() {
            //if (this.m_databaseFailures++ >= 3) {
            //    killDatabaseConnection_ReconnectAndResume();
            //}
        }

        /////////////////////////////////////////////////////////////////////////////////
        /// Handle an error based on a message.
        ///
        /// Put the error message into the status bar, log it, and make a message box appear.
        /// Use a user friendly version if available.
        /// @param errorMessage the error message to handle
        public void launchError(string errorMessage) {
            string friendlyMessage = Helper.findUserFriendlyErrorMessage(errorMessage);
            if (friendlyMessage == "") {
                friendlyMessage = errorMessage;
            }
            else {
                //                log.Error(errorMessage);
            }

            MessageBox.Show(friendlyMessage);
            //setStatus(friendlyMessage);
            // log.Error(friendlyMessage);
        }

        private int HandleConnectionStatusChange(ConnectionManager.Status status, string key) {
            int cancel = 0;

            switch (status) {
                case ConnectionManager.Status.AskPermissionForDelete:
                    if (m_cm.ClickedNode.Nodes.Count > 0) {
                        //cancel = TabManager.clearTabs("Delete Connection and close all open tabs?");
                    }
                    if (cancel == 0) // user did not cancel
                    {
                        //m_tsm.removeTabSet(key);
                    }
                    break;
                case ConnectionManager.Status.AskPermissionForDisconnect:
                    //cancel = TabManager.clearTabs("Close current connection and all open tabs?");
                    break;
                case ConnectionManager.Status.Connected:
                    //string dbProtocolVersion = null;
                    //if (0 != m_cm.DB.getSingleFromDb(ref dbProtocolVersion,
                    //                                    "configurator_protocol_version_required",
                    //                                    "Asterisk.configurator_config"))
                    //{
                    //    cancel = 1;
                    //    string m = "Connection failed to " + key + ": Unable to access protocol version on server. ";
                    //    string f = "Please contact your administrator.";
                    //    log.Error(m + " Local: " + Program.ProtocolVersion);
                    //    setStatus(m + f);
                    //    MessageBox.Show(f, m);
                    //}
                    //else if (dbProtocolVersion != Program.ProtocolVersion)
                    //{
                    //    cancel = 1;
                    //    string m = "Connection failed to " + key + ": Protocol version mismatch. ";
                    //    string f = "Please upgrade this software or contact your administrator.";
                    //    log.Error(m + "Db: " + dbProtocolVersion + " Local: " + Program.ProtocolVersion);
                    //    setStatus(m + f);
                    //    MessageBox.Show(f, m);
                    //}
                    //else
                    //    setStatus("Connected: " + key);

                    break;
                case ConnectionManager.Status.ConnectionFailed:
                case ConnectionManager.Status.Disconnected:
                case ConnectionManager.Status.ChangedCurrentConnection:
                    //setStatus(key);
                    break;
                case ConnectionManager.Status.Connecting:
                    //setStatus(key);
                    //this.Update();
                    break;
                case ConnectionManager.Status.RefreshOpenConnectionNode:
                    //addFormNodes(key);
                    break;
                case ConnectionManager.Status.DbHelperReset:
                    //m_db = m_cm.DB;
                    break;
                case ConnectionManager.Status.SettingsLoad:
                    //					Properties.Settings loadedSettings = Properties.Settings.Default;
                    //					if (m_cm != null)
                    //						m_cm.Settings = loadedSettings.ConnectionsInfo;
                    break;
                case ConnectionManager.Status.SettingsSave:
                    //					Properties.Settings settingsToSave = Properties.Settings.Default;
                    //					if (m_cm != null)
                    //						settingsToSave.ConnectionsInfo = m_cm.Settings;
                    //					settingsToSave.Save();
                    break;
                case ConnectionManager.Status.Error:
                    // log.Debug("No handling defined for Connection Manager status of <" + status.ToString() + ">.");
                    break;
                default:
                    // log.Error("Unknown Connection Manager Status <" + status.ToString() + "> recieved.");
                    break;
            }

            return cancel;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /// Callback for event when doubleclick is detected in the mainTreeView in left 
        /// panel.
        ///
        /// A call is made to open a new tab and form correspoding to the doubleclicked 
        /// item.
        /// @param sender ref to the mainTreeView
        /// @param e EventArgs
        private void m_connectionTreeView_DoubleClick(object sender, EventArgs e) {
            // if no node is selected, cancel
            if (m_connectionTreeView.SelectedNode == null)
                return;

            // check if level of node is root (connections) 
            if (m_connectionTreeView.Nodes.Contains(m_connectionTreeView.SelectedNode)) {
                //if node is not already in connected state
                if (m_connectionTreeView.SelectedNode.Nodes.Count == 0) {
                    // open this node/connection
                    m_cm.switchToConnection(m_connectionTreeView.SelectedNode.Name);
                }
            }
            //else // if not in root level, node is in form level
            //{
            //    // open this node's parent node/connection
            //    m_cm.switchToConnection(m_connectionTreeView.SelectedNode.Parent.Name);

            //    // open form in new tab
            //    TabManager.addFormInTab(Helper.find(Program.m_formNames, m_connectionTreeView.SelectedNode.Name));
            //}
        }

        private void m_connectionTreeView_MouseDown(object sender, MouseEventArgs e) {
            TreeViewHitTestInfo hti = ((TreeView)sender).HitTest(e.Location);

            string rootNodeName = null;

            if (hti.Node != null) {
                if (hti.Node.Level == 0) {
                    m_cm.ClickedNode = hti.Node;

                    // prevent node from having selected forecolor and nonhilighted backcolor when context for 
                    // non-selected node appears
                    m_connectionTreeView.SelectedNode = m_cm.ClickedNode;
                    rootNodeName = m_cm.ClickedNode.Name;
                }
                else if (hti.Node.Level == 1) {
                    rootNodeName = hti.Node.Parent.Name;
                }
            }

            if (rootNodeName != null) {
                //m_tsm.switchToTabSetAndCreateIfNeeded(rootNodeName);
            }
        }

        private DataGridView DroppedDownGrid {
            get {
                foreach (Control c in m_dynamicControls) {
                    if (c is DataGridView && c.Visible) {
                        return (DataGridView)c;
                    }
                }

                return null;
            }
        }

        /// these are used for place holders and to some extent initializers for dynamically created ui elements
        private void hideUiManagedObjects() {
            titlePb.Visible = bodyPb.Visible = footerPb.Visible = dropGridTemplate.Visible = false;
        }

        private void clearDropGrids() {
            List<Control> controlsToRemove = new List<Control>(m_dynamicControls.Count);

            foreach (Control c in m_dynamicControls)
                if (c is DataGridView) {
                    Debug.Print("this.Remove! " + c.Name);
                    this.Controls.Remove(c);
                    controlsToRemove.Add(c);
                }

            foreach (Control c in controlsToRemove) {
                Debug.Print("dyn.Remove! " + c.Name);
                m_dynamicControls.Remove(c);
            }

            Debug.Print("clearDropGrids: not removed count: " + m_dynamicControls.Count);

        }

        private void clearDynamicControls() {
            List<Control> controlsToRemove = new List<Control>(m_dynamicControls.Count);

            foreach (Control c in m_dynamicControls)
                if (!(c is DataGridView)) {
                    Debug.Print("this.Remove! " + c.Name);
                    this.Controls.Remove(c);
                    controlsToRemove.Add(c);
                }

            foreach (Control c in controlsToRemove) {
                Debug.Print("dyn.Remove! " + c.Name);
                m_dynamicControls.Remove(c);
            }

            Debug.Print("clearDynamicControls: not removed count: " + m_dynamicControls.Count);
        }

        private void drawTiledImage(Graphics g, Bitmap tiledImage, Point location, Size size, Point offset) {
            Bitmap leftCap = null, rightCap = null;
            drawTiledImageWithCaps(g, tiledImage, leftCap, rightCap, location, size, offset);
        }

        private void drawTiledImageWithCaps(Graphics g, Bitmap tiledImage, Bitmap leftCap, Bitmap rightCap,
            Point location, Size size) {
            drawTiledImageWithCaps(g, tiledImage, leftCap, rightCap, location, size, new Point(0, 0));
        }

        /// @todo handle if tile width doesn't divide perfectly into available size.width (add one tile)
        /// @todo also in above case crop last tile to only fill available size.width
        /// @todo support above two items w/ height
        private void drawTiledImageWithCaps(Graphics g, Bitmap tiledImage, Bitmap leftCap, Bitmap rightCap,
            Point location, Size size, Point offset) {
            int left_cap_width = leftCap == null ? 0 : leftCap.Width;
            int right_cap_width = rightCap == null ? 0 : rightCap.Width;

            if (left_cap_width + right_cap_width > size.Width) {
                left_cap_width = right_cap_width = size.Width / 2;
                if (left_cap_width + right_cap_width < size.Width)
                    right_cap_width++;
            }

            if (left_cap_width > 0) {
                g.DrawImage(leftCap,
                    location.X + offset.X,
                    location.Y + offset.Y,
                    left_cap_width,
                    leftCap.Height);
            }
            if (right_cap_width > 0) {
                g.DrawImage(rightCap,
                    location.X + offset.X + size.Width - right_cap_width,
                    location.Y + offset.Y,
                    right_cap_width,
                    rightCap.Height);
            }

            int tileNumWidth = (size.Width - left_cap_width - right_cap_width) / tiledImage.Width;
            int tileNumHeight = size.Height / tiledImage.Height;

            if (tileNumWidth > tiledImage.Height && tiledImage.Width == 1) {
                // optimized special case
                int baseStartX = location.X + offset.X + left_cap_width;
                int baseEndX = baseStartX + tileNumWidth;
                int baseY = location.Y + offset.Y;
                for (int i = 0; i < tiledImage.Height; i++)
                    g.DrawLine(new Pen(tiledImage.GetPixel(0, i)),
                        baseStartX,
                        baseY + i,
                        baseEndX,
                        baseY + i);
            }
            else if (tileNumHeight > tiledImage.Width && tiledImage.Height == 1) {
                // optimized special case
                // this one only has a minimal effect for our case.. maybe combine the two calls for the two grids into one?
                // would have to make sure midbar gets drawn after/on top
                int baseStartY = location.Y + offset.Y;
                int baseEndY = baseStartY + tileNumHeight;
                int baseX = location.X + offset.X + left_cap_width;
                for (int i = 0; i < tiledImage.Width; i++)
                    g.DrawLine(new Pen(tiledImage.GetPixel(i, 0)),
                        baseX + i,
                        baseStartY,
                        baseX + i,
                        baseEndY);
            }
            else
                for (int j = 0; j < tileNumHeight; j++)
                    for (int i = 0; i < tileNumWidth; i++)
                        g.DrawImage(tiledImage,
                            location.X + offset.X + left_cap_width + i * tiledImage.Width,
                            location.Y + offset.Y + j * tiledImage.Height,
                            tiledImage.Width,
                            tiledImage.Height);
        }

        private void drawTiledImageWithCaps(Graphics g, Bitmap hvTiledImage, Bitmap hTiledImageTop,
            Bitmap hTiledImageBottom, Bitmap vTiledLeftCap, Bitmap leftCapTop, Bitmap leftCapBottom,
            Bitmap vTiledRightCap, Bitmap rightCapTop, Bitmap rightCapBottom, Point location, Size size) {
            drawTiledImageWithCaps(g, hvTiledImage, hTiledImageTop, hTiledImageBottom, vTiledLeftCap, leftCapTop,
                leftCapBottom, vTiledRightCap, rightCapTop, rightCapBottom, location, size, new Point(0, 0));
        }

        /// @todo handle if tile width doesn't divide perfectly into available size.width (add one tile)
        /// @todo also in above case crop last tile to only fill available size.width
        /// @todo support above two items w/ height
        private void drawTiledImageWithCaps(Graphics g, Bitmap hvTiledImage, Bitmap hTiledImageTop,
            Bitmap hTiledImageBottom, Bitmap vTiledLeftCap, Bitmap leftCapTop, Bitmap leftCapBottom,
            Bitmap vTiledRightCap, Bitmap rightCapTop, Bitmap rightCapBottom, Point location, Size size, Point offset) {
            // left top, left bottom, left middle
            if (leftCapTop != null)
                g.DrawImage(leftCapTop,
                    location.X + offset.X,
                    location.Y + offset.Y,
                    leftCapTop.Width,
                    leftCapTop.Height);
            if (leftCapBottom != null)
                g.DrawImage(leftCapBottom,
                    location.X + offset.X,
                    location.Y + size.Height - leftCapBottom.Height + offset.Y,
                    leftCapBottom.Width,
                    leftCapBottom.Height);
            int tileNumHeight = (size.Height - leftCapTop.Height - leftCapBottom.Height) / vTiledLeftCap.Height;
            for (int i = 0; i < tileNumHeight; i++)
                g.DrawImage(vTiledLeftCap,
                    location.X + offset.X,
                    location.Y + leftCapTop.Height + offset.Y + i * vTiledLeftCap.Height,
                    vTiledLeftCap.Width,
                    vTiledLeftCap.Height);

            // right top, right bottom, right middle
            if (rightCapTop != null)
                g.DrawImage(rightCapTop,
                    location.X + size.Width - rightCapTop.Width + offset.X,
                    location.Y + offset.Y,
                    rightCapTop.Width,
                    rightCapTop.Height);
            if (rightCapBottom != null)
                g.DrawImage(rightCapBottom,
                    location.X + size.Width - rightCapTop.Width + offset.X,
                    location.Y + size.Height - rightCapBottom.Height + offset.Y,
                    rightCapBottom.Width,
                    rightCapBottom.Height);
            tileNumHeight = (size.Height - rightCapTop.Height - rightCapBottom.Height) / vTiledRightCap.Height;
            for (int i = 0; i < tileNumHeight; i++)
                g.DrawImage(vTiledRightCap,
                    location.X + size.Width - rightCapTop.Width + offset.X,
                    location.Y + rightCapTop.Height + offset.Y + i * vTiledRightCap.Height,
                    vTiledRightCap.Width,
                    vTiledRightCap.Height);

            // center top
            int tileNumWidth = (size.Width - leftCapTop.Width - rightCapTop.Width) / hTiledImageTop.Width;
            if (tileNumWidth > hTiledImageTop.Height && hTiledImageTop.Width == 1) {
                // special case optimization
                int baseStartX = location.X + leftCapTop.Width + offset.X;
                int baseY = location.Y + offset.Y;
                int baseEndX = baseStartX + tileNumWidth;
                for (int i = 0; i < hTiledImageTop.Height; i++)
                    g.DrawLine(new Pen(hTiledImageTop.GetPixel(0, i)), baseStartX, baseY + i, baseEndX, baseY + i);
            }
            else
                for (int i = 0; i < tileNumWidth; i++)
                    g.DrawImage(hTiledImageTop,
                        location.X + leftCapTop.Width + offset.X + i * hTiledImageTop.Width,
                        location.Y + offset.Y,
                        hTiledImageTop.Width,
                        hTiledImageTop.Height);

            // center bottom
            tileNumWidth = (size.Width - leftCapBottom.Width - rightCapBottom.Width) / hTiledImageBottom.Width;
            if (tileNumWidth > hTiledImageBottom.Height && hTiledImageBottom.Width == 1) {
                // special case optimization
                int baseStartX = location.X + leftCapBottom.Width + offset.X;
                int baseY = location.Y + size.Height - hTiledImageBottom.Height + offset.Y;
                int baseEndX = baseStartX + tileNumWidth;
                for (int i = 0; i < hTiledImageBottom.Height; i++)
                    g.DrawLine(new Pen(hTiledImageBottom.GetPixel(0, i)), baseStartX, baseY + i, baseEndX, baseY + i);
            }
            else
                for (int i = 0; i < tileNumWidth; i++)
                    g.DrawImage(hTiledImageBottom,
                        location.X + leftCapBottom.Width + offset.X + i * hTiledImageBottom.Width,
                        location.Y + size.Height - hTiledImageBottom.Height + offset.Y,
                        hTiledImageBottom.Width,
                        hTiledImageBottom.Height);

            // center middle
            tileNumWidth = (size.Width - leftCapTop.Width - rightCapTop.Width) / hvTiledImage.Width;
            tileNumHeight = (size.Height - hTiledImageTop.Height - hTiledImageBottom.Height) / hvTiledImage.Height;

            if (hvTiledImage.Width == 1 && hvTiledImage.Height == 1)
                // special case optimization
                g.FillRectangle(new SolidBrush(hvTiledImage.GetPixel(0, 0)),
                    new Rectangle(location.X + offset.X + leftCapTop.Width,
                        location.Y + offset.Y + hTiledImageTop.Height,
                        tileNumWidth,
                        tileNumHeight));
            else
                for (int j = 0; j < tileNumHeight; j++)
                    for (int i = 0; i < tileNumWidth; i++)
                        g.DrawImage(hvTiledImage,
                            location.X + offset.X + leftCapTop.Width + i * hvTiledImage.Width,
                            location.Y + offset.Y + hTiledImageTop.Height + j * hvTiledImage.Height,
                            hvTiledImage.Width,
                            hvTiledImage.Height);
        }

        private void LayoutForm() {
            Debug.Print("layout form!");
            clearDynamicControls();

            if (m_subscribedQueues == null) {
                return;
            }

            int queue_count = m_subscribedQueues.Count;
            int grid_height = 0;
            bool did_find_visible_dropgrid = false;

            for (int i = 0; i < queue_count; i++) {
                string queue_name = m_subscribedQueues.Keys[i];
                List<DataGridView> grids = new List<DataGridView>(2) {
                    (DataGridView) m_subscribedQueues[queue_name]["AgentGrid"]
                };

                if (m_subscribedQueues[queue_name].ContainsKey("TeamGrid"))
                    grids.Add((DataGridView)m_subscribedQueues[queue_name]["TeamGrid"]);

                foreach (DataGridView grid in grids) {
                    if (grid != null && grid.Visible) {
                        did_find_visible_dropgrid = true;
                        grid_height = grid.Height;
                        break;
                    }
                }

                if (grid_height > 0) {
                    break;
                }
            }

            if (!did_find_visible_dropgrid)
                detailViewNameLabel.Text = "";

            int predicted_body_height = mainGrid.Height;
            int predicted_offset_y = mainGrid.Height + ((grid_height > 0) ? MidBarHeight : 0);
            Bitmap bg = new Bitmap(this.Width,
                titlePb.Height + predicted_body_height + footerPb.Height + grid_height +
                ((grid_height > 0) ? MidBarHeight : 0));

            using (Graphics g = Graphics.FromImage(bg)) {
                // top window bar
                drawTiledImageWithCaps(g,
                    queueResources.title_empty_center_tile,
                    queueResources.title_empty_left,
                    queueResources.title_empty_right,
                    titlePb.Location,
                    new Size(this.Width, queueResources.title_empty_left.Height),
                    new Point(0, 0));

                string agent_num_titlebar = "";

                if (m_showAgentNumberInTitleBar) {
                    agent_num_titlebar = "(" + m_agentNum + ") ";
                }

                if ((m_agentNum == null) || (m_agentNum == "")) {
                    titleViewNameLabel.Text = (m_isManager ? "Manager" : "Agent") + " Toolbar - " + m_agentName + " [" + m_agentExtension + "]"; ;
                }
                else {
                    titleViewNameLabel.Text = (m_isManager ? "Manager" : "Agent") + " Toolbar - " + agent_num_titlebar + m_agentName + " [" + m_agentExtension + "]";
                }

                titleViewNameLabel.Location = new Point(this.Width / 2 - titleViewNameLabel.Width / 2, titleViewNameLabel.Location.Y);

                // left border
                drawTiledImage(g,
                    queueResources.border_left_tiled,
                    new Point(titlePb.Location.X, titlePb.Location.Y + queueResources.title_empty_left.Height),
                    new Size(queueResources.border_left_tiled.Width, mainGrid.Height),
                    new Point(0, 0));

                // right border
                drawTiledImage(g,
                    queueResources.border_right_tiled,
                    new Point(titlePb.Location.X + this.Width - queueResources.border_right_tiled.Width,
                        titlePb.Location.Y + queueResources.title_empty_left.Height),
                    new Size(queueResources.border_right_tiled.Width, mainGrid.Height),
                    new Point(0, 0));

                // bottom
                drawTiledImageWithCaps(g,
                    queueResources.footer_center_tile,
                    queueResources.footer_left,
                    queueResources.footer_right,
                    new Point(footerPb.Location.X,
                        queueResources.title_empty_center_tile.Height + predicted_offset_y + grid_height),
                    new Size(this.Width, 100 + queueResources.footer_left.Height));

                if (grid_height > 0) {
                    Point offset = new Point(0, predicted_offset_y);
                    drawTiledImageWithCaps(g,
                        queueResources.midbar_center_center_hvtiled,
                        queueResources.midbar_center_top_htiled,
                        queueResources.midbar_center_bottom_htiled,
                        queueResources.midbar_left_center_vtiled,
                        queueResources.midbar_left_top,
                        queueResources.midbar_left_bottom,
                        queueResources.midbar_right_center_vtiled,
                        queueResources.midbar_right_top,
                        queueResources.midbar_right_bottom,
                        new Point(titlePb.Location.X, titlePb.Location.Y + titlePb.Height + mainGrid.Height),
                        new Size(this.Width, MidBarHeight),
                        new Point(0, 0));

                    detailViewNameLabel.Location = new Point(
                        this.Width / 2 - detailViewNameLabel.Width / 2,
                        queueResources.title_empty_center_tile.Height + mainGrid.Height + MidBarHeight / 2 -
                        detailViewNameLabel.Height / 2);

                    // left border
                    drawTiledImage(g,
                        queueResources.border_left_tiled,
                        bodyPb.Location,
                        new Size(queueResources.border_left_tiled.Width, grid_height),
                        offset);

                    // right border
                    drawTiledImage(g,
                        queueResources.border_right_tiled,
                        new Point(bodyPb.Location.X + this.Width - queueResources.border_right_tiled.Width,
                            bodyPb.Location.Y),
                        new Size(queueResources.border_right_tiled.Width, grid_height),
                        offset);
                }
            }

            this.BackgroundImage = bg;
            this.BackgroundImageLayout = ImageLayout.None;

            int width_diff = this.Width - bodyPb.Width;

            AdjustFormHeight(grid_height);
            sizeAdjustorPb.Location = new Point(m_sizeAdjustorBaseLocation.X + width_diff,
                m_sizeAdjustorBaseLocation.Y + predicted_offset_y + grid_height);
            statusLight.Location = new Point(m_statusLightBaseLocation.X + width_diff,
                m_statusLightBaseLocation.Y + predicted_offset_y + grid_height);

            // Using hidden control because WY_Updater has 'Download and Install' which doesn't work, since we're using a background service updater
            // cmpAutomaticWY_Updater.Location = new Point(queueResources.border_left_tiled.Width, statusLight.Location.Y);
            this.m_cmpToolbarStatusMessage.Location = new Point(queueResources.border_left_tiled.Width, statusLight.Location.Y + 1);

            int drop_grid_y = titlePb.Height + predicted_offset_y;
            for (int i = 0; i < queue_count; i++) {
                string queue_name = m_subscribedQueues.Keys[i];
                List<DataGridView> grids = new List<DataGridView>(2);
                grids.Add((DataGridView)m_subscribedQueues[queue_name]["AgentGrid"]);

                if (m_subscribedQueues[queue_name].ContainsKey("TeamGrid"))
                    grids.Add((DataGridView)m_subscribedQueues[queue_name]["TeamGrid"]);

                foreach (DataGridView grid in grids.Where(grid => grid != null && grid.Visible)) {
                    grid.Location = new Point(queueResources.border_left_tiled.Width, drop_grid_y);
                    break;
                }
            }

            closePb.Location = new Point(m_closeButtonBaseLocation.X + width_diff, m_closeButtonBaseLocation.Y);
            minimizePb.Location = new Point(m_minimizeButtonBaseLocation.X + width_diff, m_minimizeButtonBaseLocation.Y);
            settingsPb.Location = new Point(m_settingsButtonBaseLocation.X + width_diff, m_settingsButtonBaseLocation.Y);

            if (this.m_extraControl != null) {
                m_extraControl.Location = new Point(0, this.Height - m_extraControlHeight);
                m_extraControl.Size = new Size(this.Width, m_extraControlHeight);

                if (m_statusCodesEnabled) {
                    int unused_x = this.Width - m_statusCodesPanel.Width;

                    m_statusCodesCenteringPanel.Width = this.Width;
                    m_statusCodesPanel.Location = new Point(unused_x / 2, 0);
                }
            }

            SetInterFaceSize(m_interfaceSize, true);
        }

        private void SetInterFaceSize(string size, bool firstStart) {

            switch (size) {
                case "large":
                    this.mainGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.5F);
                    this.mainGrid.ColumnHeadersHeight = 29;
                    this.mainGrid.RowTemplate.Height = 26;
                    this.smallToolStripMenuItem.Checked = false;
                    this.mediumToolStripMenuItem.Checked = false;
                    this.largeToolStripMenuItem.Checked = true;
                    break;
                case "medium":
                    this.mainGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F);
                    this.mainGrid.ColumnHeadersHeight = 20;
                    this.mainGrid.RowTemplate.Height = 18;
                    this.smallToolStripMenuItem.Checked = false;
                    this.mediumToolStripMenuItem.Checked = true;
                    this.largeToolStripMenuItem.Checked = false;
                    break;
                case "small":
                    this.mainGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
                    this.mainGrid.ColumnHeadersHeight = 16;
                    this.mainGrid.RowTemplate.Height = 13;
                    this.smallToolStripMenuItem.Checked = true;
                    this.mediumToolStripMenuItem.Checked = false;
                    this.largeToolStripMenuItem.Checked = false;
                    break;
                default:
                    return;
            }

            m_interfaceSize = size;
            Interaction.SaveSetting("IntellaToolBar", "Config", "InterfaceSize", size);

            // Need a better way to redraw the interface... we don't really need to reconnect
            if (!firstStart) {
                Error_DatabaseMainConnection_Failed();
            }
        }

        private void InitDropGridsAndQueueInfo() {
            if ((m_subscribedQueues == null) || (m_subscribedQueues.Count == 0)) {
                return;
            }

            foreach (string queueName in m_subscribedQueues.Keys) {
                if (!m_subscribedQueues.ContainsKey(queueName))
                    m_subscribedQueues.Add(queueName, new Hashtable());

                if (!m_subscribedQueues[queueName].ContainsKey("AgentGrid")) {
                    m_subscribedQueues[queueName].Add("AgentGrid", new TweakedDataGridView());
                    ((DataGridView)m_subscribedQueues[queueName]["AgentGrid"]).Visible = false;
                    // uncomment to allow user window moving by drag clicking on agent grid
                    //((DataGridView)m_subscribedQueues[queueName]["AgentGrid"]).MouseDown += intellaQueue_MouseDown;
                    //((DataGridView)m_subscribedQueues[queueName]["AgentGrid"]).MouseUp += intellaQueue_MouseUp;
                    //((DataGridView)m_subscribedQueues[queueName]["AgentGrid"]).MouseMove += intellaQueue_MouseMove;			
                }
                if (m_enableTeamView && !m_subscribedQueues[queueName].ContainsKey("TeamGrid")) {
                    m_subscribedQueues[queueName].Add("TeamGrid", new TweakedDataGridView());
                    ((DataGridView)m_subscribedQueues[queueName]["TeamGrid"]).Visible = false;
                    // uncomment to allow user window moving by drag clicking on manager grid
                    //((DataGridView)m_subscribedQueues[queueName]["TeamGrid"]).MouseDown += intellaQueue_MouseDown;
                    //((DataGridView)m_subscribedQueues[queueName]["TeamGrid"]).MouseUp += intellaQueue_MouseUp;
                    //((DataGridView)m_subscribedQueues[queueName]["TeamGrid"]).MouseMove += intellaQueue_MouseMove;
                }
            }
        }

        private void DrawImage(Graphics g, Bitmap b, Point location) {
            try {
                g.DrawImage(b, location.X, location.Y, b.Size.Width, b.Size.Height);
            }
            catch (Exception ex) {
                handleError(ex, "DrawImage() Failed");
                if (Debugger.IsAttached) { throw; }
            }
        }

        private void DrawImage(Graphics g, Bitmap b, Point location, Point offset) {
            try {
                g.DrawImage(b, location.X + offset.X, location.Y + offset.Y, b.Size.Width, b.Size.Height);
            }
            catch (Exception ex) {
                handleError(ex, "DrawImage() Failed");
                if (Debugger.IsAttached) { throw; }
            }
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
                m_screenRecord.SetLoggerCallback(MQD);
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

        private void TryOpenAndRotateLog(string log_file) {
            try {
                OpenAndRotateLog();
            }
            catch (Exception e) {
                // TODO: Check if it's being used by another process... check process tree to see if intellaqueue is already running
                MQD_NL("Exception while opening log file: {0}.  Exception: {1}", log_file, e.StackTrace.ToString());
                if (Debugger.IsAttached) { throw; }
            }
        }

        private void OpenAndRotateLog() {
            if (!logFileEnabled) {
                //  return;
            }

            if (!Directory.Exists(m_logFileDirectory)) {
                Directory.CreateDirectory(m_logFileDirectory);
            }

            string todays_logfilename = GetTodaysLogFileName();

            if (!File.Exists(todays_logfilename)) {
                //  See if there's any old logs we need to delete
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(m_logFileDirectory);
                System.IO.FileInfo[] files_info = di.GetFiles();
                List<string> filenames_list = new List<string>();

                foreach (System.IO.FileInfo file_info in files_info) {
                    filenames_list.Add(file_info.FullName);
                }

                filenames_list.Sort();
                int logfiles_count = filenames_list.Count;
                int logfiles_delete = (logfiles_count - m_LogFilesKeep);

                if ((logfiles_delete > 0)) {
                    for (int i = 0; (i <= (logfiles_delete - 1)); i++) {
                        string filename = filenames_list[i];

                        if (filename.StartsWith(m_logFileBase)) {
                            File.Delete(filename);
                        }
                    }
                }
            }

            if (m_logFileHandle != null) {
                m_logFileHandle.Close();
                m_logFileReady = false;
            }

            m_logFileHandle = File.AppendText(todays_logfilename);
            m_logFileOpenedWhen = DateTime.Now;

            m_logFileReady = true;
        }

        private void RotateLogFileIfNeeded() {
            if ((m_logFileOpenedWhen.Day == DateTime.Today.Day)) {
                return;
            }

            if (m_logFileHandle != null) {
                m_logFileHandle.Close();
            }

            string log_file = GetTodaysLogFileName();
            TryOpenAndRotateLog(log_file);
        }

        public static void UploadLog(string logFile) {
            if (!File.Exists(logFile)) {
                MQD("Log file does not exist: {0}", logFile);
                return;
            }

            string log_data = Utils.File_ReadTail(logFile, 1000);
            m_main_db.DbSelect(
                "SELECT queue.toolbar_upload_log_file(?,?,?)",
                m_main_db.escapeString(m_agentDevice),
                m_main_db.escapeString(logFile),
                m_main_db.escapeString(log_data)
            );

            MQD("Uploaded log file: " + logFile);
        }

        private void DatabaseSuccess(string message) {
            checkAgentLogin();
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
            catch (Exception e) {

            }

            //m_MainDF.ThreadSafeShow();
            MQD(errorMessage, argsRest);
        }

        public void ShowDebugWindowFromThread() {
            MethodInvoker inv = delegate {
                m_MainDF.Show();
            };

            this.Invoke(inv);
        }

        public void handleErrorWithStackTrace(Exception ex, string errorMessage) {
            errorMessage += "\r\nStackTrace: " + new StackTrace().ToString();
            handleError(ex, errorMessage);
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

        private void HideShowQueueitem_Click(object sender, EventArgs e) {
            ToolStripMenuItem    menu_item        = (ToolStripMenuItem)sender;
            QueryResultSetRecord queue_assignment = (QueryResultSetRecord)menu_item.Tag;

            string queue_name = queue_assignment["queue_name"];

            if (m_subscribedQueuesHidden.ContainsKey(queue_name)) {
                m_subscribedQueuesHidden.Remove(queue_name);
            }
            else {
                m_subscribedQueuesHidden.Add(queue_name, "1");
            }

            AgentLoginSuccess();
            // TODO: we should redo this so we have a dynamic hide versus rebuilding the entire interface
        }

        private int updateAgentMaxes(string queueName, OrderedDictionary agentCallData) {
            int longestTalkSeconds = 0;

            if (agentCallData == null) {
                return 0;
            }

            // agent_call_data[longest_talk_seconds]
            // agent_call_data[longest_talk_time]

            try {
                longestTalkSeconds = (agentCallData["longest_talk_seconds"] == null || agentCallData["longest_talk_seconds"] == "")
                    ? 0
                    : Int32.Parse((string)agentCallData["longest_talk_seconds"]);
            }
            catch (FormatException ex) {
                handleError(ex, "Format exception " + ex.Message + " on myMaxTalkSeconds of " + agentCallData["longest_talk_seconds"]);
                if (Debugger.IsAttached) { throw; }
            }

            string longest_talk_time = (string)agentCallData["longest_talk_time"];
            longest_talk_time = (longest_talk_time != null && longest_talk_time != "") ? longest_talk_time : "00:00:00";

            int maingrid_queue_position = (int)m_subscribedQueues[queueName]["MainGridRowPosition"];
            mainGrid["max_agent_talk_duration_time", maingrid_queue_position].Value = longest_talk_time;

            return longestTalkSeconds;
        }

        private static void manuallyAutoSizeGrid(DataGridView grid) {
            if (grid.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.Fill) {
                grid.Columns[0].Width = m_agentColumnDropDownWidth;

                if (m_enableTeamView) {
                    grid.Columns[1].Width = m_agentColumnDropDownWidth;
                    grid.Columns[0].Width = m_managerColumnDropDownWidth;
                }
                return;
            }

            int newWidth = 0, newHeight = 0;
            foreach (DataGridViewColumn c in grid.Columns) {
                if (c.Visible)
                    newWidth += c.Width;
            }

            foreach (DataGridViewRow r in grid.Rows) {
                newHeight += r.Height;
            }

            if (grid.RowHeadersVisible)
                newWidth += grid.RowHeadersWidth;
            if (grid.ColumnHeadersVisible)
                newHeight += grid.ColumnHeadersHeight;

            // todo: does not support all various border styles (scollbars may appear); 
            // add support for other border styles?
            if (grid.CellBorderStyle == DataGridViewCellBorderStyle.Single)
                newWidth += 1;

            grid.Size = new Size(newWidth, newHeight);
        }

        private void SetupMainGrid() {
            mainGrid.Location = new Point(queueResources.border_left_tiled.Width, queueResources.title_empty_center_tile.Height);
            mainGrid.Size = new Size(this.Width - 2 * queueResources.border_left_tiled.Width, 20);

            mainGrid.Columns.Clear();

            if (m_enableTeamView) {
                mainGrid.Columns.Add("button-TeamGrid", "Agents");
            }

            mainGrid.Columns.Add("button-agentGrid", "Callers");
            if (m_enableTeamView) {
                mainGrid.Columns.Add("agents_logged_in", "Staffed");
                mainGrid.Columns.Add("agents_idle", "Available");
                mainGrid.Columns.Add("max_agent_talk_duration_time", "Longest Talk");
                mainGrid.Columns.Add("longest_current_hold_time", "Longest Hold");
            }
            mainGrid.Columns.Add("callers_waiting", "Callers Waiting");
            mainGrid.Columns.Add("longest_waiting_time", "Longest Waiting");

            Font old_font = mainGrid.DefaultCellStyle.Font;

            if (m_multiSite) {
                mainGrid.Columns.Add("system", "System");
                mainGrid.Columns["system"].DefaultCellStyle.Font = new Font(old_font, old_font.Style | FontStyle.Bold);
            }

            mainGrid.Columns.Add("queue", "Queue Name");
            mainGrid.Columns[mainGrid.Columns.Count - 1].DefaultCellStyle.Font = new Font(old_font, old_font.Style | FontStyle.Bold);
            mainGrid.Columns[mainGrid.Columns.Count - 1].DefaultCellStyle.Padding = new Padding(15, 0, 15, 0);

            if (m_toolbarConfig.m_showLocationField) {
                mainGrid.Columns.Add("host", "Location");
                mainGrid.Columns[mainGrid.Columns.Count - 1].DefaultCellStyle.Font = new Font(old_font, old_font.Style | FontStyle.Bold);
                mainGrid.Columns[mainGrid.Columns.Count - 1].DefaultCellStyle.Padding = new Padding(15, 0, 15, 0);
            }
            else {
                // Make the queue column big so the titlebar can fit its text
                mainGrid.Columns[mainGrid.Columns.Count - 1].DefaultCellStyle.Padding = new Padding(100, 0, 100, 0);
            }

            foreach (DataGridViewColumn c in mainGrid.Columns)
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void TrimAdjustAndPositionAllDropGrids() {
            Debug.Print("TRIM ALL GRIDS!!");

            foreach (string queue_name in m_subscribedQueues.Keys) {
                List<DataGridView> grids = new List<DataGridView>(2);

                if (m_subscribedQueues[queue_name].ContainsKey("AgentGrid"))
                    grids.Add((DataGridView)m_subscribedQueues[queue_name]["AgentGrid"]);

                if (m_subscribedQueues[queue_name].ContainsKey("TeamGrid"))
                    grids.Add((DataGridView)m_subscribedQueues[queue_name]["TeamGrid"]);

                foreach (DataGridView grid in grids)
                    trimAndAdjustGrid(0, grid);
            }
        }

        private void setupGrids() {
            Debug.Print("SETUP GRIDS!!");

            if (m_subscribedQueues.Count == 0) {
                System.Windows.Forms.MessageBox.Show("You are not assigned to any queues.", "Unable to continue");
                ApplicationExit();
            }

            foreach (string queueName in m_subscribedQueues.Keys) {
                // Two Grids: Agent Grid (Shows Calls in Queue), Manager Grid (Shows Agents Activity)
                List<DataGridView> grids = new List<DataGridView>(2);

                if (m_subscribedQueues[queueName].ContainsKey("AgentGrid"))
                    grids.Add((DataGridView)m_subscribedQueues[queueName]["AgentGrid"]);

                if (m_subscribedQueues[queueName].ContainsKey("TeamGrid"))
                    grids.Add((DataGridView)m_subscribedQueues[queueName]["TeamGrid"]);

                bool is_teamview_grid = false;

                foreach (DataGridView grid in grids) {
                    grid.BackgroundColor = mainGrid.BackgroundColor;
                    grid.ColumnHeadersDefaultCellStyle = mainGrid.ColumnHeadersDefaultCellStyle.Clone();
                    //grid.ColumnHeadersDefaultCellStyle.Font = new Font(grid.ColumnHeadersDefaultCellStyle.Font.FontFamily, grid.ColumnHeadersDefaultCellStyle.Font.Size - 4);
                    grid.DefaultCellStyle = mainGrid.DefaultCellStyle.Clone();
                    grid.Font = (Font)mainGrid.Font.Clone();
                    grid.RowHeadersDefaultCellStyle = mainGrid.RowHeadersDefaultCellStyle.Clone();
                    grid.RowHeadersVisible = mainGrid.RowHeadersVisible;
                    grid.RowTemplate = (DataGridViewRow)mainGrid.RowTemplate.Clone();
                    grid.RowTemplate.Height = mainGrid.RowTemplate.Height;
                    grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    grid.AutoSizeRowsMode = mainGrid.AutoSizeRowsMode;
                    grid.ShowEditingIcon = mainGrid.ShowEditingIcon;
                    grid.AllowUserToAddRows = mainGrid.AllowUserToAddRows;
                    grid.AllowUserToDeleteRows = mainGrid.AllowUserToDeleteRows;
                    grid.AllowUserToResizeRows = false;
                    grid.ColumnHeadersHeightSizeMode = mainGrid.ColumnHeadersHeightSizeMode;
                    grid.ColumnHeadersBorderStyle = mainGrid.ColumnHeadersBorderStyle;
                    //DataGridViewHeaderBorderStyle.None;
                    grid.ReadOnly = mainGrid.ReadOnly;
                    grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                    grid.Size = dropGridTemplate.Size;
                    grid.BorderStyle = dropGridTemplate.BorderStyle;
                    grid.CellBorderStyle = mainGrid.CellBorderStyle; //dropGrid.CellBorderStyle;
                    grid.ColumnHeadersHeight = mainGrid.ColumnHeadersHeight + 1;
                    grid.DefaultCellStyle.SelectionBackColor = Helper.darkenColor(grid.DefaultCellStyle.BackColor);
                    grid.ShowCellToolTips = mainGrid.ShowCellToolTips;

                    grid.CellDoubleClick += DropGridTemplate_CellDoubleClick;
                    grid.SelectionChanged += dropGrid_SelectionChanged;
                    grid.MouseDown += dropGrid_MouseDown;
                    grid.CellMouseUp += dropGrid_CellMouseUp;
                    grid.Visible = false;

                    grid.Tag = is_teamview_grid;
                    grid.Name = queueName;

                    grid.Rows.Clear();
                    grid.Columns.Clear();

                    grid.GridColor = Color.Black;
                    grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
                    grid.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;

                    const string dropGridHeadingTextPrefix = "         ";

                    if (is_teamview_grid) {
                        grid.Columns.Add("agent_callerid_num", dropGridHeadingTextPrefix + "Ext");
                        grid.Columns.Add("agent_fullname", dropGridHeadingTextPrefix + "Agent Name");

                        if (this.m_statusCodesEnabled) {
                            int agent_status_col = grid.Columns.Add("agent_status", dropGridHeadingTextPrefix + "Agent Status");
                            grid.Columns[agent_status_col].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            grid.Columns[agent_status_col].DefaultCellStyle.Font = new Font(mainGrid.Font.FontFamily, mainGrid.Font.Size - 1);
                        }

                        if (showQueueCallerColumn) {
                            grid.Columns.Add("queue_name_caller", dropGridHeadingTextPrefix + "Queue");
                        }

                        grid.Columns.Add("talk_duration_time", dropGridHeadingTextPrefix + "Talk Time");
                        grid.Columns.Add("caller_callerid_name", dropGridHeadingTextPrefix + "Caller Name");
                        grid.Columns.Add("caller_callerid_num", dropGridHeadingTextPrefix + "Caller Number");
                        grid.Columns.Add("call_type", dropGridHeadingTextPrefix + "Call Type");
                        grid.Columns.Add("current_hold_duration_time", dropGridHeadingTextPrefix + "Hold Time");
                        grid.Columns.Add("agent_failed_to_respond_today", dropGridHeadingTextPrefix + "Missed Rings");

                        grid.Columns.Add("agent_device", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;
                        grid.Columns.Add("caller_channel", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;
                        grid.Columns.Add("agent_id", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;
                        grid.Columns.Add("talk_duration_seconds", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;
                        grid.Columns.Add("current_hold_duration_seconds", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;

                        grid.CellMouseClick += AgentStatusGrid_CellMouseClick;
                        // right mouse click on an agent to get 'Set Agent Status' 

                        // Agent Name is default sort
                        grid.Columns[1].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                    }
                    else {
                        grid.Columns.Add("callerid_num", dropGridHeadingTextPrefix + "Caller Number");
                        grid.Columns.Add("callerid_name", dropGridHeadingTextPrefix + "Caller Name");
                        grid.Columns.Add("waiting_time", dropGridHeadingTextPrefix + "Waiting Time");

                        grid.Columns.Add("live_queue_callers_item_id", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;
                        grid.Columns.Add("waiting_seconds", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;
                    }

                    // uncomment to disable useDefaultCellStyler row sorting by column
                    //foreach (DataGridViewColumn c in grid.Columns)
                    //    c.SortMode = DataGridViewColumnSortMode.NotSortable;

                    grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    grid.MultiSelect = false;

                    // add it to the form
                    Debug.Print("Adding! " + grid.Name);
                    this.Controls.Add(grid);
                    m_dynamicControls.Add(grid);

                    // if this loop goes a second time, next grid is a team view grid
                    // (if it's enabled)
                    is_teamview_grid = m_enableTeamView;
                }
            }

            TrimAdjustAndPositionAllDropGrids();
        }

        private void toggleStatusLight() {
            m_lightStatus = !m_lightStatus;
            statusLight.BackgroundImage = m_lightStatus ? queueResources.status_ok : queueResources.status_off;
        }

        //
        // m_liveDatas has been populated by the background db update thread
        //
        private bool UpdateUiDataComponents() {
            lock (m_liveDatas) {
                Dictionary<string, List<OrderedDictionary>> live_caller_data = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["caller"];
                Dictionary<string, List<OrderedDictionary>> live_agent_data  = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["agent"];
                Dictionary<string, List<OrderedDictionary>> live_queue_data  = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["queue"];
                Dictionary<string, List<OrderedDictionary>> live_status_data = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["status"];

                bool need_to_resize_frame = updateGridData(live_caller_data, live_agent_data);

                //
                // UpdateMainGridData
                //

                // Normal Operation
                try {
                    UpdateMainGridData(live_queue_data); // exception thrown from this fn

                    if (this.m_statusCodesEnabled) {
                        m_statusCodesForm.UpdateAgentStatusIfNeeded(live_status_data);
                        m_statusCodesForm.m_statusPopulated = true;
                    }

                    UpdateBarColors(m_liveDatas);
                    return need_to_resize_frame;
                }
                catch (Exception e) {
                    StackTrace s = new StackTrace(e, true);
                    StackFrame f = s.GetFrame(0);
                    string exception_location = string.Format("{0}\r\n  {1}:{2}", f.GetFileName(), f.GetMethod().Name, f.GetFileLineNumber());

                    handleError("updateuidatacomponents(), updatemaingriddata() exception at: \r\n" + exception_location + "\r\ndetail:\r\n" + e.ToString() + "\r\n\r\nStack:" + e.StackTrace.ToString());
                    //if (Debugger.IsAttached) { throw; } // exceptiondispatchinfo.capture(e).throw();

                    //if (debugger.isattached) {
                    //    throw;
                    //    //e = e.flatten();
                    //    //exceptiondispatchinfo.capture(e.innerexception).throw();
                    //}
                }
            }

            return false;
        }

        private void dbRefreshWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            throw new NotImplementedException();
        }

        private void dbRefreshWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            QD.p("RunWorkerCompleted !!!");
        }

        private void ToggleStatusLightFromThread() {
            MethodInvoker inv = delegate {
                toggleStatusLight();
            };

            this.Invoke(inv);
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

            // Are we still connected?
            tsc.m_db.DbSelectFunction("NOW");

            if (tsc.m_db.LastQueryWasError()) {
                Error_DatabaseMainConnection_Failed();
                return;
            }

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
            Dictionary<string, List<OrderedDictionary>> live_caller_data = getLiveCallerData(tsc);
            Dictionary<string, List<OrderedDictionary>> live_agent_data = m_enableTeamView ? getLiveAgentData(tsc) : new Dictionary<string, List<OrderedDictionary>>();
            Dictionary<string, List<OrderedDictionary>> live_queue_data = getLiveQueueData(tsc);
            Dictionary<string, List<OrderedDictionary>> agent_status_available = getAvailableStatusCodes(tsc);
            Dictionary<string, List<OrderedDictionary>> agent_call_data = new Dictionary<string, List<OrderedDictionary>>();
            QueryResultSet agent_current_calls = getCurrentCallsForAgent(tsc);

            //if (tsc.m_serverName.StartsWith("hum")) {
            //    var live_caller_data_r       = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(live_caller_data);
            //    var live_agent_data_r        = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(live_agent_data);
            //    var live_queue_data_r        = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(live_queue_data);
            //    var agent_status_available_r = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(agent_status_available);
            //    var agent_call_data_r        = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(agent_call_data);

            //    // if (m_MainDF.IsPaused()) { Debugger.Break(); }
            //}

            //Dictionary<string, string[]> agent_call_data = new Dictionary<string, string[]>();

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

                    bool call_status_answered = false;
                    bool call_status_answered_new = false;
                    bool call_status_changed = false;
                    bool call_status_ended = false;

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
                if (live_caller_data.ContainsKey(queue_name)) { new_live_datas.AddData_Caller(queue_name, live_caller_data[queue_name]); }
                if (live_agent_data.ContainsKey(queue_name)) { new_live_datas.AddData_Agent(queue_name, live_agent_data[queue_name]); }
                if (live_queue_data.ContainsKey(queue_name)) { new_live_datas.AddData_Queue(queue_name, live_queue_data[queue_name]); }
                if (agent_call_data.ContainsKey(queue_name)) { new_live_datas.AddData_Call(queue_name, agent_call_data[queue_name]); }
                if (agent_status_available.ContainsKey(queue_name)) { new_live_datas.AddData_Status(queue_name, agent_status_available[queue_name]); }
            }

            ///
            // Update the main data store

            lock (m_liveDatas) {
                // Server-Specific Data we just got back
                Dictionary<string, List<OrderedDictionary>> s_live_caller_data = new_live_datas.GetData_Caller();
                Dictionary<string, List<OrderedDictionary>> s_live_agent_data = new_live_datas.GetData_Agent();
                Dictionary<string, List<OrderedDictionary>> s_live_queue_data = new_live_datas.GetData_Queue();
                Dictionary<string, List<OrderedDictionary>> s_agent_status_available = new_live_datas.GetData_Status();
                Dictionary<string, List<OrderedDictionary>> s_agent_call_data = new_live_datas.GetData_Call();

                // QueryResultSetRecord r = DbHelper.ConvertOrderedDictionary_To_QueryResultSetRecord(s_live_caller_data["installer_hs"][0]);

                // Existing live data as of the last update
                Dictionary<string, List<OrderedDictionary>> main_live_caller_data = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["caller"];
                Dictionary<string, List<OrderedDictionary>> main_live_agent_data = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["agent"];
                Dictionary<string, List<OrderedDictionary>> main_live_queue_data = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["queue"];
                Dictionary<string, List<OrderedDictionary>> main_agent_status_available = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["status"];
                Dictionary<string, List<OrderedDictionary>> main_agent_call_data = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["call"];

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
                    Dictionary<string, List<OrderedDictionary>> main_live_data_item = live_data_item_pair[0];
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

        private void InitializeUpdateChecks() {
            // 
            // cmpAutomaticWY_Updater
            // 
            this.cmpAutomaticWY_Updater = new wyDay.Controls.AutomaticUpdaterBackend();
            this.cmpAutomaticWY_Updater.GUID = "a4e05f5a-836b-41a5-97eb-d83e977474e4";
            this.cmpAutomaticWY_Updater.wyUpdateCommandline = "/skipinfo";

            // The background updater service (IntellaUpdate) does the updating
            this.cmpAutomaticWY_Updater.UpdateType = wyDay.Controls.UpdateType.OnlyCheck;

            this.cmpAutomaticWY_Updater.UpdateAvailable += delegate (System.Object sender, System.EventArgs e) {
                wyDay.Controls.AutomaticUpdaterBackend au = (wyDay.Controls.AutomaticUpdaterBackend) sender;

                MQD("[UpdateCheck] Software update available. Restart to apply update. Current Version: [] New Version: {0}", au.Version);

                this.Invoke((MethodInvoker)delegate {
                    this.SetStatusBarMessage(Color.Green, "Software update available. Restart to apply update.");
                });
            };

            this.cmpAutomaticWY_Updater.CheckingFailed += delegate (System.Object sender, wyDay.Controls.FailArgs f) {
                MQD("[UpdateCheck] !!! Failed: {0}", f.ErrorMessage);
                this.SetStatusBarMessage(Color.Red, "Failed to check for software update.  Check log.");
            };

            this.cmpAutomaticWY_Updater.BeforeChecking += delegate (System.Object sender, wyDay.Controls.BeforeArgs b) {
                MQD("[UpdateCheck] Start");
            };

            this.cmpAutomaticWY_Updater.UpdateFailed += delegate (System.Object sender, wyDay.Controls.FailArgs f) {
                MQD("[UpdateCheck] Failed: {0}", f.ToString());
                this.SetStatusBarMessage(Color.Red, "Update Check Failed!");
            };

            this.cmpAutomaticWY_Updater.UpToDate += delegate (object sender, wyDay.Controls.SuccessArgs e) {
                MQD("[UpdateCheck] Complete.  Up to date at version: {0}", e.Version);

                // If we're on automatic checks, we only care about errors
                if (this.m_wyUpdateCheckManual) {
                    this.SetStatusBarMessage(Color.Green, "Update Check Complete.  Up to date");
                }
            };

            // automaticUpdater1.WaitBeforeCheckSecs
            this.cmpAutomaticWY_Updater.Initialize();
            this.cmpAutomaticWY_Updater.AppLoaded();

            this.m_wyUpdate_Timer = new System.Windows.Forms.Timer();
            this.m_wyUpdate_Timer.Interval = (int) this.m_wyUpdateCheckInterval.TotalMilliseconds;
            this.m_wyUpdate_Timer.Tick += delegate (object sender, EventArgs e) {
                CheckForUpdatesAuto();
            };

            this.m_wyUpdate_Timer.Start();
            CheckForUpdatesManual();
        }

        private void M_cmpAutomaticWY_Updater_BeforeChecking(object sender, wyDay.Controls.BeforeArgs e) {
            throw new NotImplementedException();
        }

        private void CheckForUpdatesManual() {
            this.m_wyUpdateCheckManual = true;

            MQD("Checking for updates...");
            this.SetStatusBarMessage(Color.White, "Checking for updates...");

            this.cmpAutomaticWY_Updater.ForceCheckForUpdate(true);
            this.m_wyUpdateLastCheck = DateTime.Now;
        }

        private void CheckForUpdatesAuto() {
            this.m_wyUpdateCheckManual = false;

            MQD("Checking for updates...");

            this.cmpAutomaticWY_Updater.ForceCheckForUpdate(true);
            this.m_wyUpdateLastCheck = DateTime.Now;
        }

        public void SetStatusBarMessage (Color foreColor, string message) {
            if (Thread.CurrentThread != m_guiThread)
            {
                this.Invoke((MethodInvoker) delegate() { _SetStatusBarMessage(foreColor, message); } );
                return;
            }

            _SetStatusBarMessage(foreColor, message);
        }

        private void _SetStatusBarMessage(Color foreColor, string message)
        {
            this.m_cmpToolbarStatusMessage.ForeColor = foreColor;

            this.m_cmpToolbarStatusMessage.Text = message;
            m_ToolbarStatusMessageLastUpdate = DateTime.Now;
        }

        private void UpdateQueueDisplay() {
            if (m_liveGuiDataNeedsUpdate) {
                toggleStatusLight();

                lock (m_liveDatas) {
                    Dictionary<string, List<OrderedDictionary>> agent  = (Dictionary<string, List<OrderedDictionary>>) m_liveDatas["agent"];
                    Dictionary<string, List<OrderedDictionary>> caller = (Dictionary<string, List<OrderedDictionary>>) m_liveDatas["caller"];
                    Dictionary<string, List<OrderedDictionary>> queue  = (Dictionary<string, List<OrderedDictionary>>) m_liveDatas["queue"];
                    Dictionary<string, List<OrderedDictionary>> status = (Dictionary<string, List<OrderedDictionary>>) m_liveDatas["status"];
                    Dictionary<string, List<OrderedDictionary>> call   = (Dictionary<string, List<OrderedDictionary>>) m_liveDatas["call"];

                    if (Debugger.IsAttached) { 
                        Dictionary<string, QueryResultSet> r_agent  = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(agent);
                        Dictionary<string, QueryResultSet> r_caller = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(caller);
                        Dictionary<string, QueryResultSet> r_queue  = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(queue);
                        Dictionary<string, QueryResultSet> r_status = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(status);
                        Dictionary<string, QueryResultSet> r_call   = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(call);
                    }

                    bool need_to_resize_frame = UpdateUiDataComponents();
                    // log.Info("updates done!");

                    if (need_to_resize_frame)
                        LayoutForm();

                    if (!m_updateDisplayTimer.Enabled) {
                        m_updateDisplayTimer.Start();
                        m_toolbarUpdating = false;
                    }
                }

                m_liveGuiDataNeedsUpdate = false;
            }
        }

        // ---------------------------------------------------------------------
        // DB Querys

        private Dictionary<string, List<OrderedDictionary>> getLiveAgentData(ToolbarServerConnection tsc) {
            SortedList<string, Hashtable> subscribed_queues = tsc.m_subscribedQueues;

            // Per-Queue list of caller data
            Hashtable hash_result_set = new Hashtable(); // result[queue_name] = QueryResultSet

            try {
                if (m_multiSite) {
                    hash_result_set = tsc.m_db.DbSelectIntoHashQueryResultSet("prefixed_queue_name", @"
                        SELECT
                           ?::text as server_name,
                           ?::text || '-' || queue_name as prefixed_queue_name,
                           a.* -- includes c.queue_name
                        FROM
                         live_queue.v_toolbar_agents a
                       ",
                       tsc.m_serverName,
                       tsc.m_serverName
                    );
                }
                else {
                    if (tsc.m_subscribedQueues.Count != 0) {
                        hash_result_set = tsc.m_db.DbSelectIntoHashQueryResultSet(
                          "queue_name",
                          "SELECT * FROM live_queue.v_toolbar_agents WHERE queue_name = any(string_to_array(?, ','))",
                          string.Join(",", m_subscribedQueues.Keys.ToArray())
                        );
                    }
                }

                handleDatabaseSuccess();
            }
            catch (Exception ex) {
                handleError("!!! getLiveAgentData() Error getting live data: " + ex.Message);
                throw ex;
            }

            // TODO: Convert all data functions to use QueryResultSetRecord
            Dictionary<string, List<OrderedDictionary>> dslo_result = DbHelper.ConvertHashQueryResultSet_To_DictionaryOfOrderedDictionary(hash_result_set);

            return dslo_result;
        }

        private Dictionary<string, List<OrderedDictionary>> getLiveCallerData(ToolbarServerConnection tsc) {
            SortedList<string, Hashtable> subscribed_queues = tsc.m_subscribedQueues;

            // Per-Queue list of caller data
            Dictionary<string, QueryResultSet> per_queue_caller_data = new Dictionary<string, QueryResultSet>();

            try {
                if (m_multiSite) {
                    JsonHashResult queue_callers     = tsc.m_iqc.GetAllQueueCallers(prefixQueueName: tsc.m_serverName);
                    QueryResultSet queue_callers_qrs = queue_callers.Data.ToQueryResultSet();
                    per_queue_caller_data            = queue_callers_qrs.ConvertToDictionary_String_QueryResultSet("prefixed_queue_name");
                }
                else {
                    JsonHashResult queue_callers     = tsc.m_iqc.GetAllQueueCallers();
                    QueryResultSet queue_callers_qrs = queue_callers.Data.ToQueryResultSet();
                    per_queue_caller_data            = queue_callers_qrs.ConvertToDictionary_String_QueryResultSet("queue_name");
                }

                handleDatabaseSuccess();
            }
            catch (Exception ex) {
                handleError(ex, "getLiveCallerData() Failed");
                if (Debugger.IsAttached) { throw; }
            }

            this.SubscribedQueuesFilter(per_queue_caller_data);

            // If display detail is off for non managers, and we are a non-manager.. hide the caller details!
            if (!m_isManager && !m_displayCallerDetailNonManager) {
                foreach (string queue_name in per_queue_caller_data.Keys) {
                    int caller_num = 1;

                    QueryResultSet per_queue_data = per_queue_caller_data[queue_name];

                    foreach (QueryResultSetRecord row in per_queue_data) {
                        // Each caller is a row
                        row["callerid_name"] = "Caller #" + caller_num;
                        row["callerid_num"]  = caller_num.ToString();
                        caller_num++;
                    }
                }
            }

            // TODO: Convert all data functions to use QueryResultSetRecord
            // Dictionary<string, List<OrderedDictionary>> dslo_result = DbHelper.ConvertHashQueryResultSet_To_DictionaryOfOrderedDictionary(hash_result_set);

            Dictionary<string, List<OrderedDictionary>> dslo_result = Utils.ConvertTo_DictionaryString_ListOrderedDictionary(per_queue_caller_data);

            return dslo_result;
        }

        private void SubscribedQueuesFilter(Dictionary<string, QueryResultSet> per_queue_data) {
            if (m_subscribedQueues.Keys.Count == 0) { 
                return;
            }

            Dictionary<string, QueryResultSet>.KeyCollection per_queue_caller_data_keys = per_queue_data.Keys;

            foreach (string subscribed_queue_name in per_queue_caller_data_keys) {
                if (!this.m_subscribedQueues.ContainsKey(subscribed_queue_name)) {
                    per_queue_data.Remove(subscribed_queue_name);
                }
            }
        }

        // Return a list of status codes indexed per-queue
        private Dictionary<string, List<OrderedDictionary>> getAvailableStatusCodes(ToolbarServerConnection tsc) {
            Dictionary<string, List<OrderedDictionary>> result = new Dictionary<string,List<OrderedDictionary>>();

            try {
                string index_by = (this.m_multiSite ? "prefixed_queue_name" : "queue_name");

                Hashtable hash_result_set = tsc.m_db.DbSelectIntoHashQueryResultSet(index_by, @"
                    SELECT
                        ?::text as server_name,
                        ? || '-' || queue_name as prefixed_queue_name,
                        s.* -- includes c.queue_name
                    FROM
                        live_queue.v_agent_status_codes_self s
                    WHERE
                        s.agent_device = ?
                    ",
                    tsc.m_serverName,
                    tsc.m_serverName,
                    m_agentDevice
                );

                result = DbHelper.ConvertHashQueryResultSet_To_DictionaryOfOrderedDictionary(hash_result_set);
                handleDatabaseSuccess();
            }
            catch (Exception ex) {
                MQD("!!! getAvailableStatusCodes() Error getting available status codes: " + ex.Message);
                Error_DatabaseMainConnection_Failed();
                // if (Debugger.IsAttached) { throw; }
            }

            return result;
        }

        private Dictionary<string, List<OrderedDictionary>> getLiveQueueData(ToolbarServerConnection tsc) {
            SortedList<string, Hashtable> subscribed_queues = tsc.m_subscribedQueues;

            // Per-Queue list of caller data
            Hashtable hash_result_set = new Hashtable(); // result[queue_name] = QueryResultSet

            try {
                if (m_multiSite) {
                    // Hashtable of QueryResultSet
                    hash_result_set = tsc.m_db.DbSelectIntoHashQueryResultSet("prefixed_queue_name", @"
                       SELECT
                          ?::text as server_name,
                          ?::text || '-' || queue_name as prefixed_queue_name,
                          agents_idle as agents_idle_real,
                          version(),
                          q.* -- includes c.queue_name
                       FROM
                         live_queue.v_toolbar_queues q"
                      ,
                      tsc.m_serverName,
                      tsc.m_serverName
                    );
                }
                else {
                    if (subscribed_queues.Count != 0) {
                        hash_result_set = tsc.m_db.DbSelectIntoHashQueryResultSet(
                          "queue_name",
                          "SELECT * FROM live_queue.v_toolbar_queues WHERE queue_name = any(string_to_array(?, ','))",
                          string.Join(",", m_subscribedQueues.Keys.ToArray())
                        );
                    }
                }

                // Generate an Error to handle!
                /*
                Random i = new Random();
                int r = (int)(i.NextDouble() * 10);

                if (r > 8) {
                    Hashtable foo = null;
                    foo.Add("a", "b");
                }
                */

                handleDatabaseSuccess();
            }
            catch (Exception ex) {
                MQD("!!! getLiveQueueData() Error getting live data " + ex.Message);
                Error_DatabaseMainConnection_Failed();
            }

            // TODO: Convert all data functions to use QueryResultSetRecord
            Dictionary<string, List<OrderedDictionary>> dslo_result = DbHelper.ConvertHashQueryResultSet_To_DictionaryOfOrderedDictionary(hash_result_set);

            return dslo_result;
        }

        private QueryResultSet getCurrentCallsForAgent(ToolbarServerConnection tsc) {
            QueryResultSet result = new QueryResultSet();

            if (m_currentCallEnabled == false) {
                return result;
            }

            // We're only taking calls on the main server we're connected to
            if (!tsc.m_isMainServer) {
                return result;
            }


            // FIXME FIXME FIXME we need a general callcenter_live.agents_status that merges callqueue and dialer status
            // and then toolbar agents would need reflect dialer status and then we can query callcenter_live.v_toolbar_agents

            try
            {
                result = tsc.m_db.DbSelect(@"
                    SELECT
                        c.channel,
                        a.call_state,
                        c.call_log_id,
                        c.call_segment_id,
                        c.case_number,
                        c.callerid_num,
                        c.callerid_name,
                        q.queue_longname
                    FROM
                        live_queue.v_toolbar_agents a
                        JOIN live_queue.v_callers   c ON (a.caller_call_log_id::text = c.call_log_id::text) -- FIXME... ideally we should get everything from v_toolbar_agents but it doesn't have segment_id and queue_longname
                        JOIN queue.queues           q ON (c.queue_id = q.queue_id)
                    WHERE
                      a.agent_device = ?
                      AND picked_up_when IS NOT NULL -- Only find calls to this agent that are ANSWERED
                ", m_agentDevice);

                if (result.Count == 0) {
                    // No current dialer calls... dialer?

                    if (this.m_mainServerHasDialer) {
                        result = tsc.m_db.DbSelect(@"
                            SELECT
                                c.from_channel as channel,
                                c.call_status as call_state,
                                c.call_log_id,
                                NULL as call_segment_id, -- FIXME populate
                                --c.case_number,
                                c.to_callerid_num as callerid_num,
                                CASE
                                  WHEN c.to_callerid_name IS NOT NULL THEN c.to_callerid_name
                                  ELSE 'DIALER'::text  -- FIXME, why don't we have name
                                END as callerid_name,
                                cmp.campaign_longname as queue_longname
                            FROM
                                live_dialer.dialers     d
                                JOIN live_core.v_calls  c ON (c.call_log_id = d.call_log_id)
                                JOIN dialer.campaigns cmp ON (d.campaign_id = cmp.campaign_id)
                            WHERE
                              c.from_device = ?
                              AND c.call_pickup_time IS NOT NULL -- Only find calls to this agent that are ANSWERED
                         ", m_agentDevice);
                    }
                }

                handleDatabaseSuccess();
            }
            catch (Exception ex)
            {
                MQD("!!! getCurrentCallsForAgent() Error getting live data " + ex.Message);
                Error_DatabaseMainConnection_Failed();
            }

            return result;
        }

        // ---------------------------------------------------------------------

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
                    }
                );

                //}, textToAdd);
            }
            catch (Exception ex)
            {
                MQD("ScreenCapture - File Capture Failed (CallLogID: {0} -- CallSegmentID: {1})", currentCallData["call_log_id"], currentCallData["call_segment_id"] + "\r\n" + ex.ToString());
            }
        }

        private void stopScreenRecordingIfNecessary()
        {
            if (this.m_screenRecord.IsRunning()) {
                this.m_screenRecord.RecordingStop();
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
            QueryResultSetRecord screenpop_data;
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
        // Update either the 'Calls in Queue' grid, or the 'Agent Status' grid, which ever one is active
        //
        private bool updateGridData(Dictionary<string, List<OrderedDictionary>> liveCallerData, Dictionary<string, List<OrderedDictionary>> liveAgentData) {
            //Debug.Print("updateGridData");

            bool visibleDropgridDidChangeSize = false;

            IList<string> queueNames = m_subscribedQueues.Keys;
            if (queueNames.Count < 1)
                return visibleDropgridDidChangeSize;

            m_lastOP = "updateGridData-1";

            foreach (string queueName in m_subscribedQueues.Keys) {
                bool isTeamGrid = false;
                List<DataGridView> grids = new List<DataGridView>(2);

                m_lastOP = "updateGridData-2 " + queueName;

                if (m_subscribedQueues[queueName].ContainsKey("AgentGrid")) { 
                    grids.Add((DataGridView) m_subscribedQueues[queueName]["AgentGrid"]);
                }

                if (m_subscribedQueues[queueName].ContainsKey("TeamGrid")) { 
                    grids.Add((DataGridView) m_subscribedQueues[queueName]["TeamGrid"]);
                }

                foreach (DataGridView grid in grids) {
                    // live_data is either LiveAgentData or LiveCallerData
                    // The grids for both are handled the same
                    //
                    Dictionary<string, List<OrderedDictionary>> live_data = isTeamGrid ? liveAgentData : liveCallerData;

                    m_lastOP = "updateGridData-3 " + isTeamGrid.ToString();

                    if (live_data == null) {
                        Debug.Print("Result is null");
                        isTeamGrid = true;
                        continue;
                    }

                    // save user row sorting by column
                    SortOrder sortOrder = grid.SortOrder;
                    DataGridViewColumn sortedColumn = grid.SortedColumn;

                    string selectedRowUniqueIdValue = "";
                    try {
                        if (grid.SelectedRows.Count > 0)
                            selectedRowUniqueIdValue =
                                grid[isTeamGrid ? "agent_id" : "live_queue_callers_item_id", grid.SelectedRows[0].Index]
                                    .Value.ToString();
                    }
                    catch (Exception e) {
                        // FIXME EXCEPTION HANDLING
                        Debug.Print("Failed a grid update: " + e.ToString());
                    }

                    m_lastOP = "updateGridData-4 " + isTeamGrid.ToString();

                    int current_row = 0;
                    int total_rows = grid.RowCount;

                    try {
                        foreach (string queue_name in live_data.Keys)
                            foreach (OrderedDictionary resultRow in live_data[queue_name]) {
                                if (!queue_name.Equals(grid.Name))
                                    break;

                                m_lastOP = "updateGridData-5 " + isTeamGrid.ToString() + "" + queue_name;

                                if ((current_row + 1) > total_rows) {
                                    // Add an empty at the end
                                    grid.Rows.Add("", "", "", "", "");
                                }

                                // Store the full agent status db row in the grid tag
                                grid.Rows[current_row].Tag = resultRow;

                                m_lastOP = "updateGridData-6 " + isTeamGrid.ToString() + "" + queue_name;

                                foreach (DataGridViewColumn c in grid.Columns) {
                                    string col_name = c.Name;;
                                    string newValue = "";

                                    m_lastOP = "updateGridData-7 " + isTeamGrid.ToString() + "" + queue_name + " " + col_name;

                                    // !!! possible null dereference
                                    if (resultRow[col_name] == null) {
                                      col_name = "";
                                      MQD("updateGridData() !!! Got null on field name {0}", col_name);
                                    }

                                    newValue = resultRow[col_name].ToString();

                                    m_lastOP = "updateGridData-8 " + isTeamGrid.ToString() + "" + queue_name + " " + col_name;

                                    if (c.Name == "agent_callerid_num")
                                        newValue = "x" + newValue;

                                    m_lastOP = "updateGridData-9 " + isTeamGrid.ToString() + " " + queue_name + " " + col_name;

                                    if (c.Name == "agent_firstname") {
                                        newValue += " " + resultRow["agent_lastname"];

                                        if (string.IsNullOrEmpty(newValue))
                                            newValue = "(Unknown)";
                                    }

                                    m_lastOP = "updateGridData-10 " + isTeamGrid.ToString() + " " + queue_name + " " + col_name;

                                    grid[c.Name, current_row].Value = newValue;
                                }

                                current_row++;
                            }
                    }
                    catch (Exception ex) {
                        handleError(ex, "agent or caller dropgrid processing failed");
                        ProgramError(QD.ERROR_LEVEL.NOTICE, "GUI_AgentOrCallerDropGridProcessingFailed", "");
                        if (Debugger.IsAttached) { throw; }

                        return visibleDropgridDidChangeSize;
                    }

                    m_lastOP = "updateGridData-11 " + isTeamGrid.ToString();

                    // restore user row sorting by column
                    if (sortOrder != SortOrder.None && sortedColumn != null && selectedRowUniqueIdValue != "") {
                        grid.Sort(sortedColumn,
                            sortOrder == SortOrder.Ascending
                                ? ListSortDirection.Ascending
                                : ListSortDirection.Descending);

                        foreach (DataGridViewRow r in grid.Rows) {
                            m_lastOP = "updateGridData-12 " + isTeamGrid.ToString();

                            if (r.Cells[isTeamGrid ? "agent_id" : "live_queue_callers_item_id"].Value.ToString() ==
                                selectedRowUniqueIdValue)
                                r.Selected = true;
                            }
                    }

                    m_lastOP = "updateGridData-13 " + isTeamGrid.ToString();

                    visibleDropgridDidChangeSize = trimAndAdjustGrid(current_row, grid) || visibleDropgridDidChangeSize;

                    // if this loop goes a 2nd time, process as a team grid
                    isTeamGrid = true;
                }
            }

            return visibleDropgridDidChangeSize;
        }

        private void ClearToolbarData() {
           if (Thread.CurrentThread != m_guiThread) {
               this.Invoke((MethodInvoker) _ClearToolbarData);
               return;
           }

           _ClearToolbarData();
        }

        private void _ClearToolbarData() {
            hideAnyVisibleGridsExcept(null);

            if (m_subscribedQueues != null)
                m_subscribedQueues.Clear();

            clearDropGrids();
            mainGrid.Rows.Clear();
            mainGrid.Columns.Clear();
        }

        private void AdjustFormHeight(int gridHeight) {
            int old_height = this.Height;
            this.Height = m_extraControlHeight + titlePb.Height + mainGrid.Height + footerPb.Height + gridHeight +
                          (gridHeight > 0 ? MidBarHeight : 0);

            if (old_height != this.Height)
                Debug.Print("ADJUST HEIGHT " + old_height + "->" + this.Height);
        }

        private void UpdateMainGridData(Dictionary<string, List<OrderedDictionary>> liveQueueData) {
            //List<string> die_die_die = null;
            //Console.WriteLine(die_die_die[0]);  // Fancy little exception

            if (mainGrid.Rows.Count != m_subscribedQueues.Keys.Count) {
                mainGrid.Rows.Clear();

                if (m_subscribedQueues.Keys.Count > 0)
                    mainGrid.Rows.Add(m_subscribedQueues.Keys.Count);
            }

            int i = 0;

            foreach (string queue_name in m_subscribedQueuesOrder.Keys) {
                string queue_heading_text = "";

                if (queue_name == "all") {
                    Debug.Print("");
                }

                if (queue_name != "") {
                    queue_heading_text = m_subscribedQueues[queue_name].ContainsKey("HeadingText")
                        ? m_subscribedQueues[queue_name]["HeadingText"].ToString()
                        : queue_name;
                }

                foreach (DataGridViewColumn col in mainGrid.Columns) {
                    if (col.Name.StartsWith("button-")) {
                        string grid_name = m_enableTeamView && col.Index == 0 ? "TeamGrid" : "AgentGrid";

                        if (mainGrid[col.Name, i].Tag == null) {
                            DataGridViewCell new_button = new DataGridViewTextBoxCell();

                            new_button.Style.BackColor = mainGrid[col.Name, i].Style.BackColor;

                            Hashtable queue_data = m_subscribedQueues[queue_name];
                            DataGridView grid    = (DataGridView)queue_data[grid_name];

                            if (grid != null) {
                                string button_name = grid.Visible ? "up_triangle" : "down_triangle";
                                Color backcolor    = mainGrid[col.Name, i].Style.BackColor;

                                new_button.Tag = new Hashtable() {
                                    {"QueueName", queue_name},
                                    {"GridName", grid_name}, {
                                        "Image",
                                        button_name
                                    },
                                    {"Color", backcolor}
                                };
                            }

                            mainGrid[col.Name, i] = new_button;
                        }
                        else {
                            ((Hashtable)mainGrid[col.Name, i].Tag)["Image"] =
                                ((DataGridView)m_subscribedQueues[queue_name][grid_name]).Visible
                                    ? "up_triangle"
                                    : "down_triangle";
                        }

                        continue;
                    }
                    else {
                        switch (col.Name) {
                            case "queue":
                                mainGrid[col.Name, i].Value = queue_heading_text;
                                continue;
                            case "host":
                                mainGrid[col.Name, i].Value = this.Config_Get_DB_Host();
                                continue;
                            case "system":
                                if (m_multiSite) {
                                    string queue_system_text = m_subscribedQueues[queue_name].ContainsKey("SystemLongName")
                                            ? m_subscribedQueues[queue_name]["SystemLongName"].ToString()
                                            : "";

                                    mainGrid[col.Name, i].Value = queue_system_text;

                                }

                                continue;                                
                        }
                    }

                    string new_value = "";

                    if (liveQueueData.ContainsKey(queue_name)) {
                        if (liveQueueData[queue_name].Count > 0) {
                            if (Debugger.IsAttached) { 
                               OrderedDictionary queue_data = liveQueueData[queue_name][0];
                               QueryResultSetRecord queue_data_record = DbHelper.ConvertOrderedDictionary_To_QueryResultSetRecord(queue_data);
                            }

                            new_value = (string) liveQueueData[queue_name][0][col.Name];
                        }
                    }

                    if (new_value != null) {
                        // This handles updates for:
                        //  agents_logged_in (Staffed)
                        //  agents_idle      (Available)
                        //  

                        mainGrid[col.Name, i].Value = new_value;
                    }
                }

                i++;
            }

            ResizeMainGrid();
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

        private void ResizeMainGrid() {
            ResizeMainGrid(false);
        }

        private void ResizeMainGrid(bool doForceRedraw) {
            DataGridView g = DroppedDownGrid;
            if (g == null) // small mode
            {
                //g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                mainGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                mainGrid.AutoResizeColumns();
                manuallyAutoSizeGrid(mainGrid);

                if (m_enableTeamView) {
                    m_managerColumnDropDownWidth = mainGrid.Columns[0].Width;
                    m_agentColumnDropDownWidth = mainGrid.Columns[1].Width;
                }
                else
                    m_agentColumnDropDownWidth = mainGrid.Columns[0].Width;

                resize(
                    new Size(
                        queueResources.border_left_tiled.Width + queueResources.border_right_tiled.Width +
                        mainGrid.Width, this.Height), doForceRedraw);
            }
            else // dropped/big mode
            {
                mainGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                g.AutoResizeColumns();
                manuallyAutoSizeGrid(g);

                if (g.Width < mainGrid.Width) {
                    g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    g.AutoResizeColumns();
                    g.Width = mainGrid.Width;
                }
                else if (g.Width > mainGrid.Width)
                    mainGrid.Width = g.Width;

                mainGrid.AutoResizeColumns();
                manuallyAutoSizeGrid(mainGrid);
                resize(
                    new Size(
                        queueResources.border_left_tiled.Width + queueResources.border_right_tiled.Width + g.Width,
                        this.Height), doForceRedraw);
            }
        }

        private void UpdateBarColors(Hashtable liveDatas) {
            Dictionary<string, List<OrderedDictionary>> live_queue_data = (Dictionary<string, List<OrderedDictionary>>) liveDatas["queue"];
            Dictionary<string, List<OrderedDictionary>> agent_call_data = (Dictionary<string, List<OrderedDictionary>>) liveDatas["call"];

            foreach (string queue_name in m_subscribedQueues.Keys) {
                // ****** TEMP
                if (!agent_call_data.ContainsKey(queue_name)) {
                    continue;
                }

                //
                // Main Grid

                if (m_enableTeamView) {
                    //
                    // agent_call_data[queue_name][0][longest_talk_seconds]
                    // agent_call_data[queue_name][0][longest_talk_time]
                    //

                    if (Debugger.IsAttached) {
                        QueryResultSet        r = DbHelper.ConvertListOrderedDictionary_To_QueryResultSet(live_queue_data[queue_name]);
                        QueryResultSetRecord rr = DbHelper.ConvertOrderedDictionary_To_QueryResultSetRecord(live_queue_data[queue_name][0]);
                    }

                    string longest_waiting_seconds_str = (string) live_queue_data[queue_name][0]["longest_waiting_seconds"];
                    int longest_waiting_seconds        = Int32.Parse(longest_waiting_seconds_str);
                    int longest_talk_seconds           = updateAgentMaxes(queue_name, agent_call_data[queue_name][0]);
                    int longest_current_hold_seconds   = Int32.Parse((string) live_queue_data[queue_name][0]["longest_current_hold_seconds"]);

                    SetBarColor(longest_talk_seconds, queue_name, true, longest_current_hold_seconds, live_queue_data);
                }
                else { 
                    SetBarColor(0, queue_name, false, 0, live_queue_data);
                }

                // End Main Grid

                // drop grids
                {
                    DataGridView g = (DataGridView) m_subscribedQueues[queue_name]["AgentGrid"];
                    foreach (DataGridViewRow r in g.Rows) { 
                        ColorGridCell(r.Cells["waiting_time"], m_subscribedQueues, queue_name);
                    }

                    g.Refresh();
                }

                if (m_subscribedQueues[queue_name]["TeamGrid"] != null) {
                    DataGridView g = (DataGridView) m_subscribedQueues[queue_name]["TeamGrid"];
                    foreach (DataGridViewRow r in g.Rows) {
                        ColorGridCell(r.Cells["talk_duration_time"],         m_subscribedQueues, queue_name);
                        ColorGridCell(r.Cells["current_hold_duration_time"], m_subscribedQueues, queue_name);

                        if (this.m_statusCodesEnabled) {
                            ColorGridCell(r.Cells["agent_status"], m_subscribedQueues, queue_name);
                        }
                    }
                }
            }
        }

        private void ColorGridCell(DataGridViewCell coloredCell, SortedList<string, Hashtable> subscribedQueues, string queueName) {
            int yellowThreshold, orangeThreshold, redThreshold;

            string col_name = coloredCell.OwningColumn.Name;
            string grid_name = coloredCell.DataGridView.Name;
            object value_cell_value = "0";
            
            try {
                switch (col_name) {
                    case "callers_waiting":
                        yellowThreshold   = 3; // (int)subscribedQueues[queueName]["Threshold_warning1-Manager"];
                        orangeThreshold   = 5; // (int)subscribedQueues[queueName]["Threshold_warning2-Manager"];
                        redThreshold      = 10; // (int)subscribedQueues[queueName]["Threshold_warning3-Manager"];
                        value_cell_value  = coloredCell.OwningRow.Cells["callers_waiting"].Value;
                        break;

                    case "talk_duration_time":
                        yellowThreshold  = (int)subscribedQueues[queueName]["Threshold_warning1-Manager"];
                        orangeThreshold  = (int)subscribedQueues[queueName]["Threshold_warning2-Manager"];
                        redThreshold     = (int)subscribedQueues[queueName]["Threshold_warning3-Manager"];
                        value_cell_value = coloredCell.OwningRow.Cells["talk_duration_seconds"].Value;
                        break;
                    case "current_hold_duration_time":
                        yellowThreshold  = (int)subscribedQueues[queueName]["Hold_time_threshold_warning1-Manager"];
                        orangeThreshold  = (int)subscribedQueues[queueName]["Hold_time_threshold_warning2-Manager"];
                        redThreshold     = (int)subscribedQueues[queueName]["Hold_time_threshold_warning3-Manager"];
                        value_cell_value = coloredCell.OwningRow.Cells["current_hold_duration_seconds"].Value;
                        break;
                    case "waiting_time":
                        yellowThreshold  = (int)subscribedQueues[queueName]["Threshold_warning1-Agent"];
                        orangeThreshold  = (int)subscribedQueues[queueName]["Threshold_warning2-Agent"];
                        redThreshold     = (int)subscribedQueues[queueName]["Threshold_warning3-Agent"];
                        value_cell_value = coloredCell.OwningRow.Cells["waiting_seconds"].Value;
                        break;
                    case "agent_status":
                        yellowThreshold  = 5; // Unused
                        orangeThreshold  = 10;
                        redThreshold     = 20;
                        value_cell_value = coloredCell.OwningRow.Cells["agent_status"].Value;
                        break;
                    default:
                    case null:
                    case "":
                        return;
                }

                int cell_color_value = CellValueToInt(value_cell_value, col_name);
                coloredCell.Style.BackColor = GetColorFromValueAndThresholds(cell_color_value, yellowThreshold,
                    orangeThreshold, redThreshold);
                coloredCell.Style.SelectionBackColor = (coloredCell.Style.BackColor == QueueColorEmpty)
                    ? Helper.darkenColor(QueueColorEmpty)
                    : coloredCell.Style.BackColor;
            }
            catch (Exception e) {
                // FIXME EXCEPTION HANDLING
                Debug.Print("Failed getting thresholds: " + e.ToString());
            }
        }

        ////
        // 
        //
        private int CellValueToInt(object value, string colName) {
            int result = 0;

            if (value == null) {
                return result;
            }

            if (this.m_statusCodesEnabled) {
                // Note: Only managers will see agent_status as a grid field

                if (colName == "agent_status") {
                    // Numeric value comes from whether this is a agent (10) status code or a manager-level (20) status code
                    List<OrderedDictionary> a = m_toolbarConfig.m_managerStatusCodes;

                    // value is the longname of the status code
                    // TODO: SHould make this a Hashtable to get direct access to manger status codes...
                    foreach (OrderedDictionary status_code_item in m_toolbarConfig.m_managerStatusCodes) {
                        if (value.ToString().StartsWith("Available")) {
                            return 1; // Green
                        }

                        var status_split = value.ToString().Split('\n');

                        if (status_split[0] == (string)status_code_item["status_code_longname"]) {
                            if ((string)status_code_item["manager_only"] == "True") {
                                return 20; // Red
                            }

                            return 10; // Orange
                        }
                    }
                }
            }

            if (value is string)
                int.TryParse((string)value, out result);

            return result;
        }
        
        private bool trimAndAdjustGrid(int current_row, DataGridView grid) {
            int oldHeight = grid.Height;
            int total_rows = grid.RowCount;

            // delete out extra rows
            if ((current_row < total_rows))
                for (int i = total_rows - 1; i >= current_row; i--)
                    grid.Rows.Remove(grid.Rows[i]);

            total_rows = grid.RowCount;

            int rowheight = grid.RowCount > 0 && grid.ColumnCount > 0 ? grid[0, 0].Size.Height : grid.RowTemplate.Height;

            int minHeightRowCount = 1;
            int maxRowCount = 10;
            int bonusHeight = 0;

            if (total_rows < minHeightRowCount)
                grid.Height = (minHeightRowCount * rowheight) + grid.ColumnHeadersHeight + bonusHeight;
            else if ((total_rows <= maxRowCount))
                grid.Height = (total_rows * rowheight) + grid.ColumnHeadersHeight + bonusHeight;
            else if ((total_rows > maxRowCount))
                grid.Height = (maxRowCount * rowheight) + grid.ColumnHeadersHeight + bonusHeight;

            bool did_height_change = false;
            if (oldHeight != grid.Height) {
                //Debug.Print("Trim! " + oldHeight + "->" + grid.Height);
                did_height_change = grid.Visible;
            }

            return did_height_change;
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

        private void UpdateDisplayTimer_Tick(System.Object sender, System.EventArgs e) {
            m_updateDisplayTimer.Stop();

            RotateLogFileIfNeeded();

            if (this.m_db_reconnects >= MAX_DB_RECONNECTS) {
                // Completely rebuild, reconnect and re-start all threads
                foreach (KeyValuePair<string, ToolbarServerConnection> toolbar_server_item in m_toolbarServers) {
                    ToolbarServerConnection tsc = toolbar_server_item.Value;
                    tsc.Cancel = true;
                }

                this.ClearToolbarData();
                checkAgentLogin();         // 'Hidden' Agent Login and toolbar rebuild
                goto skip_update;
            }

            if (this.m_program_failures > MAX_PROGRAM_FAILURES) {
                ProgramError(QD.ERROR_LEVEL.FATAL, "MaxToolBarUpdateFailures", "Max Toolbar Failures Reached.  Halting updates.");
                SetStatusBarMessage(Color.Red, "Connection failed. Check network connection and restart");
                return;
            }

            if (!m_db_active) {
                // Database is in the process of reconecting or is otherwise unavailable, no data to update
                goto skip_update;
            }

            // QD.p("UpdateDisplayTimer TICK!");
            UpdateQueueDisplayAndCatchExceptions();

            // Clear Statusbar Message, if needed
            if ((DateTime.Now - m_ToolbarStatusMessageLastUpdate) >= m_ToolbarStatusMessageKeep) {
                this.SetStatusBarMessage(Color.White, "");
            }

        skip_update:
            m_updateDisplayTimer.Start();
        }

        private void UpdateDisplayTimerHealthCheck_Tick(System.Object sender, System.EventArgs e) {
            if (!m_updateDisplayTimer.Enabled) {
                // Only do health checks if we're supposed to be updating
                // m_updateDisplayTimer will be diabled if we're editing db settings, or logging in as another agent, etc
                return;
            }

           //  QD.p("UpdateDisplayTimerHealthCheck TICK!");

            int threshold = 5;

            DateTime now = DateTime.Now;

            foreach (KeyValuePair<string,ToolbarServerConnection> tsc_item in m_toolbarServers) {
                string server_name          = tsc_item.Key;
                ToolbarServerConnection tsc = tsc_item.Value;
                TimeSpan since_last_update  = (now - tsc.m_lastUpdated);

                if (false && since_last_update.Seconds > threshold) {
                    MQD(String.Format("!!! UpdateDisplayTimerHealthCheck -- DoWork ({0}) last update time > threshold {1}s ({2}s since last update)", 
                        server_name,
                        threshold,
                        since_last_update.Seconds
                    ), null);
                }
            }
        }

        /// @todo make sure all db connections (should be just 1, not 2) are closed on exit

        public bool IsRefreshTimerActive() {
            return m_updateDisplayTimer.Enabled;
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

        private static Color GetColorFromValueAndThresholds(int value, int yellowThreshold, int orangeThreshold, int redThreshold, int row_number = -1, Func<int, Color, Color> zebraStripeFn = null) {

            Color result = QueueColorEmpty;

            if (value > redThreshold)
                result = (redThreshold == 0) ? QueueColorEmpty : QueueColorWarning3;
            else if (value > orangeThreshold)
                result = (orangeThreshold == 0) ? QueueColorEmpty : QueueColorWarning2;
            else if (value > yellowThreshold)
                result = (yellowThreshold == 0) ? QueueColorEmpty : QueueColorWarning1;
            else if (value > 0)
                result = QueueColorOk;

            if (zebraStripeFn != null) {
                return zebraStripeFn(row_number, result);
            }

            return result;
        }

        private Color ZebraStripeCallback(int row_number, Color color) {
            if (row_number % 2 == 0) {
                return Helper.darkenColor(color);
            }

            return color;
        }

        /// <summary>
        ///   Set cell colors based on thresholds on a single toolbar row (A single queue)
        /// </summary>
        /// <param name="longestTalkSeconds"></param>
        /// <param name="queueName"></param>
        /// <param name="isManager"></param>
        /// <param name="longestHoldTime"></param>
        /// <param name="liveQueueData"></param>
        private void SetBarColor(int longestTalkSeconds, string queueName, bool isManager, int longestHoldTime, Dictionary<string, List<OrderedDictionary>> liveQueueData) {
            int main_grid_row_index = (int) m_subscribedQueues[queueName]["MainGridRowPosition"]; // The index into mainGrid for our particular queue row

            Color bg_color_ok    = QueueColorOk;
            Color bg_color_empty = QueueColorEmpty;

            if (main_grid_row_index % 2 == 0) {
                bg_color_empty = Helper.darkenColor(bg_color_empty);
                bg_color_ok    = Helper.darkenColor(bg_color_ok);
            }

            Color bg_color_agents_logged_in       = bg_color_empty;
            Color bg_color_agents_idle            = bg_color_empty;
            Color bg_color_talk_new               = bg_color_empty;
            Color bg_color_hold_new               = bg_color_empty;
            Color bg_color_callers_new            = bg_color_empty;
            Color bg_color_longest_waiting_caller = bg_color_empty;

            Dictionary<string, QueryResultSet> live_queue_data_r = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(liveQueueData);
            QueryResultSetRecord live_queue_data = live_queue_data_r[queueName][0];

            if (!m_subscribedQueues[queueName].ContainsKey("Threshold_warning1-Agent")) { 
                // not yet configured
                return;
            }

            int longest_waiting_seconds = live_queue_data.ToInt("longest_waiting_seconds");
            int longest_talk_seconds    = live_queue_data.ToInt("longest_talk_seconds");
            int callers_waiting         = live_queue_data.ToInt("callers_waiting");
            int agents_logged_in        = live_queue_data.ToInt("agents_logged_in");
            int agents_idle             = live_queue_data.ToInt("agents_idle");

            // These thresholds are used as the difference between the number of callers and agents
            //   If yellow threshold is 2, then we will go yellow if there are 2 less agents than then number of callers
            //     ie:  # Callers: 4,  # Agents, 2... Yellow threshold is now hit
            //
            int num_agents_idle_threshold_yellow = 1; // 1-2 additional agents needed to answer callers
            int num_agents_idle_threshold_orange = 3; // 3-4 or more...
            int num_agents_idle_threshold_red    = 5; // 5 or more

            int call_waiting_seconds_threshold_yellow = (int) m_subscribedQueues[queueName]["Threshold_warning1-Agent"];
            int call_waiting_seconds_threshold_orange = (int) m_subscribedQueues[queueName]["Threshold_warning2-Agent"];
            int call_waiting_seconds_threshold_red    = (int) m_subscribedQueues[queueName]["Threshold_warning3-Agent"];

            int num_call_waiting_seconds_threshold_yellow = 2;
            int num_call_waiting_seconds_threshold_orange = 4;
            int num_call_waiting_seconds_threshold_red    = 6;

            int agent_talking_seconds_threshold_yellow = (int) m_subscribedQueues[queueName]["Threshold_warning1-Manager"];
            int agent_talking_seconds_threshold_orange = (int) m_subscribedQueues[queueName]["Threshold_warning2-Manager"];
            int agent_talking_seconds_threshold_red    = (int) m_subscribedQueues[queueName]["Threshold_warning3-Manager"];

            int longest_holding_seconds_threshold_yellow = (int) m_subscribedQueues[queueName]["Hold_time_threshold_warning1-Manager"];
            int longest_holding_seconds_threshold_orange = (int) m_subscribedQueues[queueName]["Hold_time_threshold_warning2-Manager"];
            int longest_holding_seconds_threshold_red    = (int) m_subscribedQueues[queueName]["Hold_time_threshold_warning3-Manager"];

            /*
             bg_color_agents_logged_in = GetColorFromValueAndThresholds(
                agents_logged_in,
                (int) m_subscribedQueues[queueName]["Threshold_warning1-Agent"],
                (int) m_subscribedQueues[queueName]["Threshold_warning2-Agent"],
                (int) m_subscribedQueues[queueName]["Threshold_warning3-Agent"]);
            */

            /*            
             *  Normal Layout
             *  -------------
             *  Staffed
             *  Available
             *  Longest Talk
             *  Longest Hold
             *  Callers Waiting
             *  Longest Waiting
             *   
            */

            // Time Longest Waiting
            bg_color_longest_waiting_caller = GetColorFromValueAndThresholds(
                longest_waiting_seconds,
                call_waiting_seconds_threshold_yellow,
                call_waiting_seconds_threshold_orange,
                call_waiting_seconds_threshold_red,
                main_grid_row_index, ZebraStripeCallback
            );

            // Special case for: Time Longest Waiting: Green color
            if ((longest_waiting_seconds > 0) && (longest_waiting_seconds < call_waiting_seconds_threshold_yellow)) {
                bg_color_longest_waiting_caller = bg_color_ok;
            }

            // Special case for: Agents Staffed (agents_logged_in)
            if (agents_logged_in > 0) {
                bg_color_agents_logged_in = bg_color_ok;
            }

            // Special case for: Available (agents_idle)
            if (callers_waiting == 0) {
                bg_color_agents_idle = bg_color_empty;
            }
            else if (agents_idle >= callers_waiting) {
                // We have enough agents to handle waiting callers
                bg_color_agents_idle = bg_color_ok;
            }
            else {
                // We have fewer agents available than the number of waiting callers
                if (agents_idle == 0) {
                  bg_color_agents_idle = QueueColorWarning3;
                }
                else {
                    // X number of agents available, but less than the # of waiting callers
                    // We have MORE callers waiting, than idle agents
                    //
                    int agent_caller_diff = callers_waiting - agents_idle;

                    bg_color_agents_idle = GetColorFromValueAndThresholds(
                        agent_caller_diff + 1,
                        num_agents_idle_threshold_yellow,
                        num_agents_idle_threshold_orange,
                        num_agents_idle_threshold_red,
                        main_grid_row_index, ZebraStripeCallback
                    );
                }
            }

            // # Callers Waiting
            bg_color_callers_new = GetColorFromValueAndThresholds(
                callers_waiting,
                num_call_waiting_seconds_threshold_yellow,
                num_call_waiting_seconds_threshold_orange,
                num_call_waiting_seconds_threshold_red,
                main_grid_row_index, ZebraStripeCallback
            );


            if (m_enableTeamView) {
                // Longest Talk
                bg_color_talk_new = GetColorFromValueAndThresholds(
                    longest_talk_seconds,
                    agent_talking_seconds_threshold_yellow,
                    agent_talking_seconds_threshold_orange,
                    agent_talking_seconds_threshold_red,
                    main_grid_row_index, ZebraStripeCallback
                );

                // Longest Hold
                bg_color_hold_new = GetColorFromValueAndThresholds(
                    longestHoldTime,
                    longest_holding_seconds_threshold_yellow,
                    longest_holding_seconds_threshold_orange,
                    longest_holding_seconds_threshold_red,
                    main_grid_row_index, ZebraStripeCallback
                );
            }

            // Done Choosing Colors
            // Set Colors

            DataGridViewRow grid_row = mainGrid.Rows[main_grid_row_index];
            
            Helper.SetDataGridViewCell_BackColor(grid_row, "agents_logged_in",             bg_color_agents_logged_in);
            Helper.SetDataGridViewCell_BackColor(grid_row, "agents_idle",                  bg_color_agents_idle);
            Helper.SetDataGridViewCell_BackColor(grid_row, "longest_waiting_time",         bg_color_longest_waiting_caller);
            Helper.SetDataGridViewCell_BackColor(grid_row, "max_agent_talk_duration_time", bg_color_talk_new);
            Helper.SetDataGridViewCell_BackColor(grid_row, "longest_current_hold_time",    bg_color_hold_new);
            Helper.SetDataGridViewCell_BackColor(grid_row, "callers_waiting",              bg_color_callers_new);
            Helper.SetDataGridViewCell_BackColor(grid_row, "queue",                        bg_color_empty);           // Zebra Stripe the entire row.. queue name included

            // For each Cell Column in the MainGrid for just this specific queue

            foreach (DataGridViewCell c in mainGrid.Rows[main_grid_row_index].Cells) {
                // c = Current Cell in the column for queueName

                if (!c.OwningColumn.Name.StartsWith("button-")) {
                    continue;
                }

                ((Hashtable) c.Tag)["Color"] = bg_color_empty;

                if (!mainGrid.GetCellDisplayRectangle(c.ColumnIndex, c.RowIndex, true).Contains(mainGrid.PointToClient(Cursor.Position))) {
                    c.Style.BackColor = bg_color_empty;
                }

                if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left &&
                    m_mouseDownCellColumnIndex != -1 &&
                    m_mouseDownCellRowIndex != -1) {

                    if (m_mouseDownCellRowIndex == c.RowIndex && m_mouseDownCellColumnIndex == c.ColumnIndex) {
                        c.Style.BackColor = Helper.darkenColor(bg_color_empty);
                    }
                    else {
                        c.Style.BackColor = Helper.lightenColor(bg_color_empty);
                    }
                }
            }
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

        public void checkAgentLogin() {
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

            AgentLoginDialogForm agentLoginForm = new AgentLoginDialogForm();
            agentLoginForm.setServerInfo(db_host);
            agentLoginForm.SetValidateCallback(agentLoginValidate);
            agentLoginForm.SetSuccessCallback(AgentLoginSuccess);
            agentLoginForm.setTextBoxValues(agentNum, agentExtension);

            ///////

            this.m_agentLoginPanel = agentLoginForm.GetMainPanel();

            Panel agentLoginCenteringPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(this.Width, this.m_agentLoginPanel.Height),
                // AutoSize     = true,
                // AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                // BorderStyle  = BorderStyle.Fixed3D, // For Debug
            };

            // Standard
            agentLoginForm.Show();

            // New-Style -- Not Working
            //this.Controls.Add(this.m_agentLoginPanel);
    /*
            agentLoginForm.SetIntellaQueueForm(this);

            m_agentLoginPanel.Location = new Point(0, 0);

            this.Controls.Add(this.m_agentLoginPanel);

            this.m_agentLoginPanel.Paint += delegate (object sender, PaintEventArgs e) {
                Panel p = (Panel)sender;

                p.Parent.Width = this.Width; // Toolbar width
                p.Parent.Height = p.Height;  // Caller Panel height

                int total_padding = p.Parent.Size.Width - p.Size.Width;
                int padding_left = total_padding / 2;

                p.Location = new Point(padding_left, 0);
                p.Anchor    = AnchorStyles.None;
            };

            this.Height += 300;


            //////
            */ 
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

            QueryResultSetRecord queue_tenant_result;
            QueryResultSetRecord config_result;

            // Boolean dbIsManagerMonitorEnabled;
            string dbAgentNumber, dbAgentPin, dbIsManager;

            string connect_to_host = Config_Get_DB_Host();
            // connect_to_host = "comm"

			m_iqc_login = m_iqc.CreateAgentConnection(connect_to_host, "IntellaQueue", "IntellaQueue", "443", agentNumber, agentExtension);
            if (!m_iqc_login.success) {
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
            checkAgentLogin();
        }

        //----------------------------------------------------------------------
        // Event Handlers
        //----------------------------------------------------------------------

        //----------------------------------------------------------------------
        // Event Handlers: DropGridTemplate

        private void DropGridTemplate_CellDoubleClick(System.Object sender, System.Windows.Forms.DataGridViewCellEventArgs e) {
            if ((e.RowIndex == -1)) {
                return;
            }

            if (!m_isManager_MontorEnabled) {
                return;
            }

            DataGridView grid = (DataGridView)sender;

            // only Team Grid grid, and only if we're a manager
            if (!(bool)grid.Tag || !m_isManager)
                return;

            string agentExtension = Interaction.GetSetting("IntellaToolBar", "Config", "USER_agentExtension");
            string agentDevice = grid["agent_device", e.RowIndex].Value.ToString();
            string callerChannel = grid["caller_channel", e.RowIndex].Value.ToString();

            // notify db of action
            // TODO: Multi-Server Toolbar Project - Need to be able to monitor remote systems!
            m_main_db.DbSelectFunction("queue.agent_monitor", agentExtension, agentDevice, callerChannel);
        }

        //----------------------------------------------------------------------
        // Event Handlers: Agent Status Grid

        private void AgentStatusGrid_CellMouseClick(Object sender, DataGridViewCellMouseEventArgs e) {
            if (!m_isManager) {
                // Right click menu off the Agent Status Grid is only for Managers!
                return;
            }

            if (e.RowIndex < 0)
                return;

            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            Debug.Print("AgentStatusGrid: RIGHTMOUSEDOWNCELL " + e.RowIndex + " " + e.ColumnIndex);

            // The Clicked row
            TweakedDataGridView t              = (TweakedDataGridView)sender;
            DataGridViewRow     r              = t.Rows[e.RowIndex];
            OrderedDictionary   row_agent_data = (OrderedDictionary)r.Tag;

            // TODO: we should only build this once on startup, and any time the status code list changes
            ContextMenuStrip ms_status_all      = new ContextMenuStrip();
            ContextMenuStrip ms_status_perqueue = new ContextMenuStrip();

            string real_queue  = (string) row_agent_data["queue_name"];                   // live_queue.v_agents_status
            string agent_queue = real_queue;

            if (m_multiSite) {
                agent_queue = (string) row_agent_data["prefixed_queue_name"];
            }

            // FIXME.. Not supported for now (Need cross-site manager permission bit)
            if (!(Boolean) m_subscribedQueues[agent_queue]["IsMainSystem"]) {
                MQD("!!! Cross-Server Agent-Set-Status Not Supported");
                return;
            }
            
            string queue_longname = (string)  m_subscribedQueues[agent_queue]["HeadingText"];       // TODO: Ideally we should get this from the row data in row_agent_data, but we'll need a new data item for queue_longname in live_queue.v_agents_status
            Boolean queue_agg     = (Boolean) m_subscribedQueues[agent_queue]["QueueIsAggregate"]; // TODO: Ideally we should get this from the row data in row_agent_data, but we'll need a new data item for queue_longname in live_queue.v_agents_status

            cmpSetAgentStatusToolStripMenuItem.DropDown         = ms_status_all;
            cmpSetAgentStatusPerQueueToolStripMenuItem.DropDown = ms_status_perqueue;
            cmpSetAgentStatusPerQueueToolStripMenuItem.Text     = "Set Agent Status (" + queue_longname + ")";

            if (queue_agg) {
                // Setting agent status on an aggregate queue makes no sense.  Agents do not 'log into' an aggregate queue
                cmpSetAgentStatusPerQueueToolStripMenuItem.Visible = false;
            }
            else {
                // Only for Regular Queues (Non Aggregate)
                cmpSetAgentStatusPerQueueToolStripMenuItem.Visible = true;
            }

            QueryResultSetRecord row_agent_data_dbg = DbHelper.ConvertOrderedDictionary_To_QueryResultSetRecord(row_agent_data);

            foreach (OrderedDictionary status_code_item in m_toolbarConfig.m_managerStatusCodes) {
                string server_name     = (string) row_agent_data["server_name"];
                ToolStripItem ti_all   = ms_status_all.Items.Add(     (string) status_code_item["status_code_longname"]);

                ti_all.Tag    = new Hashtable() { { "type", "ALL" }, { "item", status_code_item } };
                ti_all.Click += cmpAgentManagerRightClickContextMenu_Click;

                // Only for Regular Queues
                if (!queue_agg) { 
                    // Setting agent status on an aggregate queue makes no sense.  Agents do not 'log into' an aggregate queue
                    ToolStripItem ti_perqueue = ms_status_perqueue.Items.Add((string) status_code_item["status_code_longname"]);

                    ti_perqueue.Tag    = new Hashtable() { { "type", "QUEUE" }, { "item", status_code_item }, { "queue", real_queue }, { "server_name", server_name } };
                    ti_perqueue.Click += cmpAgentManagerRightClickContextMenu_Click;
                }
            }

            cmpAgentManagerRightClickContextMenu_selectedAgentDevice = (string) row_agent_data["agent_device"];
            cmpAgentManagerRightClickContextMenu.Show(Cursor.Position.X, Cursor.Position.Y);
        }

        //----------------------------------------------------------------------
        // Event Handlers: Main Grid

        private void mainGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e) {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0 ||
                e.RowIndex > mainGrid.RowCount - 1 ||
                e.ColumnIndex > mainGrid.ColumnCount - 1)
                return;

            DataGridView grid = (DataGridView)sender;

            if (!grid[e.ColumnIndex, e.RowIndex].OwningColumn.Name.StartsWith("button-"))
                return;

            if (grid[e.ColumnIndex, e.RowIndex].Tag is Hashtable) {

                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                DataGridViewTextBoxCell bc = mainGrid[e.ColumnIndex, e.RowIndex] as DataGridViewTextBoxCell;

                Image img = (Image)getResourceByName((string)((Hashtable)bc.Tag)["Image"]);
                e.Graphics.DrawImageUnscaledAndClipped(img, new Rectangle(e.CellBounds.X + e.CellBounds.Width / 2 - img.Width / 2, e.CellBounds.Y + e.CellBounds.Height / 2 - img.Height / 2, img.Width, img.Height));
                e.Handled = true;
            }
        }

        private void mainGrid_MouseDown(object sender, MouseEventArgs e) {
            deSelectVisibleControls();
        }

        private void mainGrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e) {
            DataGridView grid = (DataGridView)sender;

            if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left) {
                if (m_mouseDownCellColumnIndex >= 0 &&
                    m_mouseDownCellRowIndex >= 0 &&
                    m_mouseDownCellRowIndex == e.RowIndex &&
                    m_mouseDownCellColumnIndex == e.ColumnIndex)
                    modCellColor(grid, e.ColumnIndex, e.RowIndex, ButtonCellColorState.Dark);
                else
                    modCellColor(grid, e.ColumnIndex, e.RowIndex, ButtonCellColorState.Light);

            }
            else {
                modCellColor(grid, e.ColumnIndex, e.RowIndex, ButtonCellColorState.Light);
            }
        }

        private void mainGrid_CellMouseLeave(object sender, DataGridViewCellEventArgs e) {
            modCellColor((DataGridView)sender, e.ColumnIndex, e.RowIndex, ButtonCellColorState.Base);
        }

        private void mainGrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            modCellColor((DataGridView)sender, e.ColumnIndex, e.RowIndex, ButtonCellColorState.Dark);
            m_mouseDownCellColumnIndex = e.ColumnIndex;
            m_mouseDownCellRowIndex = e.RowIndex;
            Debug.Print("MOUSEDOWNCELL " + m_mouseDownCellColumnIndex + " " + m_mouseDownCellRowIndex);
        }

        private void mainGrid_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.RowIndex < 0)
                return;

            deSelectVisibleControls();

            modCellColor((DataGridView)sender, e.ColumnIndex, e.RowIndex, ButtonCellColorState.Light);

            //Debug.Print("CLICK!");

            DataGridView grid = (DataGridView)sender;
            DataGridViewCell c = grid[e.ColumnIndex, e.RowIndex];
            Hashtable tag; // First cell in each of the data rows is tagged with the queue details 
            string queueName;
            string gridName;

            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                // We only care about the leftmost column for left mouse clicks (the queue info dropdowns)

                if (e.RowIndex < 0 ||
                    e.ColumnIndex < 0 ||
                    e.RowIndex > mainGrid.RowCount - 1 ||
                    e.ColumnIndex > mainGrid.ColumnCount - 1) {

                    return;
                }

                if (m_mouseDownCellRowIndex != e.RowIndex || m_mouseDownCellColumnIndex != e.ColumnIndex || m_mouseDownCellColumnIndex < 0 || m_mouseDownCellRowIndex < 0) {
                    // mousedown happened elsewhere from mouseup!
                    m_mouseDownCellRowIndex = -1;
                    m_mouseDownCellColumnIndex = -1;
                    return;
                }

                if (!c.OwningColumn.Name.StartsWith("button-") ||
                    !(c.Tag is Hashtable)) {

                    return;
                }

                tag = (Hashtable)c.Tag;
                queueName = (string)tag["QueueName"];
                gridName = (string)tag["GridName"];

            }
            else {
                // Right mouse click
                if ((e.RowIndex < 0) || (e.ColumnIndex != 3)) {
                    return;
                }

                DataGridViewCell mastercell = grid[0, e.RowIndex];  // Tag is stored on the first cell of the row
                tag = (Hashtable)mastercell.Tag;
                queueName = (string)tag["QueueName"];
                gridName  = (string)tag["GridName"];

                QueueRightClickContextMenu_SelectedQueue = queueName;
                QueueRightClickContextMenu.Show(Cursor.Position.X, Cursor.Position.Y);
                return;
            }

            DataGridView newDropGrid = (DataGridView)m_subscribedQueues[queueName][gridName];

            newDropGrid.Tag = (string)tag["GridName"] == "TeamGrid";

            if (newDropGrid.Visible) {
                detailViewNameLabel.Text = "";
                newDropGrid.Visible = false;
            }
            else {
                string dropGridType = (grid.Columns[e.ColumnIndex].HeaderText == "Callers") ? "Callers Waiting" : grid.Columns[e.ColumnIndex].HeaderText;
                detailViewNameLabel.Text = grid["queue", e.RowIndex].Value.ToString() + " " + dropGridType;
                hideAnyVisibleGridsExcept(newDropGrid);
                newDropGrid.Visible = true;
            }

            foreach (DataGridViewColumn col in grid.Columns) {
                if (col.Name.StartsWith("button-")) {
                    foreach (DataGridViewRow r in grid.Rows) {
                        ((Hashtable)grid[col.Index, r.Index].Tag)["Image"] = "down_triangle";
                    }
                }
            }

            ((Hashtable)c.Tag)["Image"] = ((DataGridView)newDropGrid).Visible ? "up_triangle" : "down_triangle";

            ResizeMainGrid(true);
        }

        //----------------------------------------------------------------------
        // Event Handlers: IntellaQueue Main Form

        /// Below 3 methods handle user dragging of windows without border decorations
        private void intellaQueue_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                if (sender.GetType().ToString() == "intellaQueue.IntellaQueueForm") {
                    Debug.Print("IntellaQueue TitleBar Right Mouse Click");

                    if (Debugger.IsAttached) {
                        cmpTitleBarRightClickMenu.Show(Cursor.Position.X, Cursor.Position.Y);
                    }
                }
            }

            if (sender is DataGridView) {
                DataGridViewCell c = Helper.getGridCellAtPoint((DataGridView)sender, Cursor.Position);
                if (c != null && c.OwningColumn.Name.StartsWith("button-"))
                    return;
            }

            isFormDragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void intellaQueue_MouseUp(object sender, MouseEventArgs e) {
            isFormDragging = false;
        }

        private void intellaQueue_MouseMove(object sender, MouseEventArgs e) {
            if (isFormDragging) {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        //----------------------------------------------------------------------
        // Event Handlers: Toolbar Menu Clicks
 
        private void AdminToolStripMenuItem_Click(System.Object sender, System.EventArgs e) {
            PasswordDialogForm pwForm = new PasswordDialogForm();

            pwForm.SetValidateCallback(AdminPasswordValidate);
            pwForm.SetSuccessCallback(ShowAdminSettings);
            pwForm.Show();
            //pwForm.SetDesktopLocation(MousePosition.X, MousePosition.Y);
        }

        private void AboutToolStripMenuItem_Click(System.Object sender, System.EventArgs e) {
            m_aboutForm = new QueueToolbarAboutForm();
            m_aboutForm.Show();
        }

        private void smallToolStripMenuItem_Click(object sender, EventArgs e) {
            SetInterFaceSize("small", false);
        }

        private void mediumToolStripMenuItem_Click(object sender, EventArgs e) {
            SetInterFaceSize("medium", false);
        }

        private void largeToolStripMenuItem_Click(object sender, EventArgs e) {
            SetInterFaceSize("large", false);
        }

        //----------------------------------------------------------------------
        // GUI Utility
        //----------------------------------------------------------------------
      
        private void sizeAdjustorPb_MouseUp(object sender, MouseEventArgs e) {
            m_isFormResizing = false;
        }

        private void sizeAdjustorPb_MouseDown(object sender, MouseEventArgs e) {
            m_isFormResizing = true;
            m_resizeCursorPoint = Cursor.Position;
            m_resizeFormSize = this.Size;
        }

        private void sizeAdjustorPb_MouseMove(object sender, MouseEventArgs e) {
            if (UserResizeEnabled && m_isFormResizing) {
                Point dif = Point.Subtract(Cursor.Position, new Size(m_resizeCursorPoint));
                Size newSize = new Size(Point.Add(new Point(m_resizeFormSize), new Size(dif)));

                int minWidth = 1;
                if (newSize.Width >= minWidth)
                    resize(newSize);
                else if (this.Width > minWidth)
                    resize(new Size(minWidth, newSize.Height));
            }
        }

        public void resize(Size newSize) {
            resize(newSize, false);
        }

        private void resize(Size newSize, bool doAlwaysResize) {
            if (this.WindowState != FormWindowState.Minimized && (doAlwaysResize || newSize.Width != Size.Width || newSize.Height != Size.Height)) {
                Debug.Print("RESIZE " + Size.ToString() + "->" + newSize.ToString());

                this.Size = newSize;
                LayoutForm();
                this.Refresh();
            }
        }

        private void button_MouseEnter(object sender, EventArgs e) {
            adjustPictureBoxButtonImage((PictureBox)sender, (int)PictureBoxButtonEvents.Enter);
        }

        private void button_MouseLeave(object sender, EventArgs e) {
            adjustPictureBoxButtonImage((PictureBox)sender, (int)PictureBoxButtonEvents.Leave);
        }

        private void button_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            adjustPictureBoxButtonImage((PictureBox)sender, (int)PictureBoxButtonEvents.Down);
        }

        private void button_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) {
                m_doNotShowSettingsDropdownOnNextClick = false;
                return;
            }

            PictureBox p = (PictureBox)sender;
            int buttonEventCode = (int)PictureBoxButtonEvents.UpInside;

            if (e.X < 0 || e.Y < 0 || e.X >= p.Width || e.Y >= p.Height)
                buttonEventCode = (int)PictureBoxButtonEvents.UpOutside;

            int index = adjustPictureBoxButtonImage(p, buttonEventCode);
            string name = (string)p.Tag;
            if (buttonEventCode != (int)PictureBoxButtonEvents.UpInside) {
                m_doNotShowSettingsDropdownOnNextClick = false;
                return;
            }

            string queueName = null;
            bool isManagerSection = false;
            getQueueNameAndType(ref queueName, ref isManagerSection, index);

            // handle button press
            switch (name) {
                case "close":
                    if (m_allowToolbarClose) {
                        ApplicationExit();
                    }

                    // Minimize instead of closing
                    this.WindowState = FormWindowState.Minimized;
                    m_isMinimized = true;

                    break;
                case "minimize":
                    this.WindowState = FormWindowState.Minimized;
                    m_isMinimized = true;
                    break;
                case "settings":
                    if (m_doNotShowSettingsDropdownOnNextClick)
                        m_doNotShowSettingsDropdownOnNextClick = false;
                    else {
                        settingsPb.ContextMenuStrip = MainContextMenu;
                        p.ContextMenuStrip.Show(p, new System.Drawing.Point(0, p.Height));
                        settingsPb.ContextMenuStrip = null;
                    }
                    break;
                default:
                    break;
            }

            m_doNotShowSettingsDropdownOnNextClick = false;
        }

        private void MainContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            if (settingsPb.ClientRectangle.Contains(settingsPb.PointToClient(Control.MousePosition)))
                m_doNotShowSettingsDropdownOnNextClick = true;
        }

        private void MainContextMenu_Opening(object sender, CancelEventArgs e) {
            //e.Cancel = true;
        }

        //----------------------------------------------------------------------
        // Utility - General
        //----------------------------------------------------------------------

        private string Config_Get_DB_Host() {
            return ToolBarHelper.Registry_GetToolbarConfigItem("IntellaToolBar", "Config", "DB_Host");
        }

        private string Config_Get_DB_Port() {
          return ToolBarHelper.Registry_GetToolbarConfigItem("IntellaToolBar", "Config", "DB_Port");
        }

        //----------------------------------------------------------------------
        // Event Handlers: Manager - Agent Status Grid - Click on a Right-Click-Context-Menuitem

        private void cmpAgentManagerRightClickContextMenu_Click(object sender, EventArgs e) {
            ToolStripDropDownItem clicked_item = (ToolStripDropDownItem)sender;

            Hashtable agent_status_click_data = (Hashtable)clicked_item.Tag;
            string set_status_type = (string)agent_status_click_data["type"];
            OrderedDictionary agent_status_item = (OrderedDictionary)agent_status_click_data["item"]; // (from queue.v_queue_agent_status)
            string status_code_name = (string)agent_status_item["status_code_name"];

            if (set_status_type == "ALL") {
                // Set status on this agent for ALL queues
                setNewAgentStatus(status_code_name, cmpAgentManagerRightClickContextMenu_selectedAgentDevice);
            }
            else if (set_status_type == "QUEUE") {
                // Set status on this agent for only this specific queue
                string queue_name = (string)agent_status_click_data["queue"];

                setNewAgentStatus(status_code_name, cmpAgentManagerRightClickContextMenu_selectedAgentDevice, queue_name);
            }
        }

        //----------------------------------------------------------------------

        private void getQueueNameAndType(ref string queueName, ref bool isManagerSection, string index) {
            try {
                getQueueNameAndType(ref queueName, ref isManagerSection, Int32.Parse(index));
            }
            catch (FormatException ex) {
                // parse for int failed
                Debug.Print(ex.Message);
            }
        }

        private void getQueueNameAndType(ref string queueName, ref bool isManagerSection, int index) {
            string[] queueNames;

            if (index >= 0) {
                queueNames = new string[m_subscribedQueues.Count];
                m_subscribedQueues.Keys.CopyTo(queueNames, 0);
                if (m_enableTeamView) {
                    queueName = queueNames[index / 2];
                    isManagerSection = index % 2 == 0;
                }
                else {
                    queueName = queueNames[index];
                    isManagerSection = false;
                }
            }
        }

        private void hideAnyVisibleGridsExcept(DataGridView grid) {
            foreach (Control c in m_dynamicControls) {
                if (grid == null || (c.Tag is bool && !((bool)c.Tag == (bool)grid.Tag && c.Name == grid.Name))) {
                    if (m_subscribedQueues.ContainsKey(c.Name)) {
                        ((DataGridView)m_subscribedQueues[c.Name][(bool)c.Tag ? "TeamGrid" : "AgentGrid"]).Visible = false;
                    }
                }
            }
        }

        private int adjustPictureBoxButtonImage(PictureBox p, int buttonEvent) {
            string newImageName;
            string name;

            if (p.Tag != null)
                name = p.Tag.ToString();
            else {
                name = p.Name;
                if (name.EndsWith("Pb"))
                    name = name.Substring(0, name.Length - 2);
            }
            newImageName = name;

            int index = -1;

            try {
                if (!Int32.TryParse(p.Name, out index))
                    index = -1;
            }
            catch (FormatException ex) {
                Debug.Print(ex.Message);
            }

            switch (buttonEvent) {
                case (int)PictureBoxButtonEvents.UpInside:
                    goto case (int)PictureBoxButtonEvents.Enter;
                case (int)PictureBoxButtonEvents.Enter:
                    newImageName += "_hover";
                    break;
                case (int)PictureBoxButtonEvents.Down:
                    newImageName += "_pressed";
                    break;
                case (int)PictureBoxButtonEvents.UpOutside:
                    break;
                case (int)PictureBoxButtonEvents.Leave:
                    break;
                default:
                    break;
            }

            p.BackgroundImage = (Image)getResourceByName(newImageName);

            p.Tag = name;

            return index;
        }

        private object getResourceByName(string name) {
            ResourceManager rm = new ResourceManager("QueueLib.queueResources", Assembly.GetExecutingAssembly());
            object result = rm.GetObject(name);
            rm.ReleaseAllResources();
            return result;
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e) {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            this.TopMost = menuItem.Checked = !menuItem.Checked;
        }

        private void mainGrid_SelectionChanged(object sender, EventArgs e) {
            deSelectVisibleControls();
        }

        private void dropGrid_SelectionChanged(object sender, EventArgs e) {
            deSelectVisibleControls();

            DataGridView g = (DataGridView)sender;

            foreach (DataGridViewRow r in g.Rows)
                foreach (DataGridViewCell c in r.Cells)
                    c.Style.SelectionBackColor = g.DefaultCellStyle.SelectionBackColor;

            foreach (DataGridViewCell c in g.SelectedCells)
                ColorGridCell(c, m_subscribedQueues, g.Name);
        }

        private void dropGrid_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e) {
            deSelectVisibleControls();
        }

        private void dropGrid_MouseDown(object sender, MouseEventArgs e) {
            deSelectVisibleControls();
        }

        private void deSelectVisibleControls() {
            // need to do this to allow colow change of cell on mouse hover
            mainGrid.ClearSelection();

            // prevent selection cursor on any visible controls
            hiddenGrid.Focus();
            hiddenGrid.Select();
        }

        private void modCellColor(DataGridView grid, int columnIndex, int rowIndex, ButtonCellColorState colorState) {
            if (rowIndex < 0 || columnIndex < 0)
                return;

            DataGridViewCell c = grid[columnIndex, rowIndex];
            if (!c.OwningColumn.Name.StartsWith("button-") || !(c.Tag is Hashtable))
                return;

            Hashtable cellInfo = (Hashtable)c.Tag;
            if (!cellInfo.ContainsKey("Color") || cellInfo["Color"] == null)
                return;

            Color baseColor = (Color)((Hashtable)c.Tag)["Color"];

            switch (colorState) {
                case ButtonCellColorState.Light:
                    c.Style.BackColor = Helper.lightenColor(baseColor);
                    break;
                case ButtonCellColorState.Dark:
                    c.Style.BackColor = Helper.darkenColor(baseColor);
                    break;
                default:
                case ButtonCellColorState.Base:
                    c.Style.BackColor = baseColor;
                    break;
            }
        }

        private void intellaQueue_Resize() {
            // This code prevents drop grid from being in an incorrectly sized state after an un-minimize.
            // @todo Maintain the currently selected row.
            if (m_isMinimized && WindowState != FormWindowState.Minimized) {
                m_isMinimized = false;

                DataGridView visibleDropGrid = DroppedDownGrid;
                if (visibleDropGrid != null) {
                    // removes all rows
                    trimAndAdjustGrid(0, visibleDropGrid);
                    // repopulates dropgrid with latest data
                    UpdateUiDataComponents();
                }

                // make sure window decorations are layed out correctly
                LayoutForm();
            }
        }

        private void intellaQueue_Resize(object sender, EventArgs e) {
            intellaQueue_Resize();
        }

        private void takeNextWaitingCallerToolStripMenuItem_Click(object sender, EventArgs e) {
            string agent_extension = Interaction.GetSetting("IntellaToolBar", "Config", "USER_agentExtension"); // TODO-FIXME>.. we already have this as a global var!

            // **** TODO-FIXME Multi-Server Toolbar Project - Secondary Priority - Take next waiting queue caller from other system!
            m_main_db.DbSelectFunction("queue.grab_queue_member_next", QueueRightClickContextMenu_SelectedQueue, agent_extension);
        }
        
        private void cmpLogoutAgentToolStripMenuItem_Click(object sender, EventArgs e)
        {
          m_main_db.DbSelectFunction("queue.agent_logout", cmpAgentManagerRightClickContextMenu_selectedAgentDevice, "MANAGER_FORCE_LOGOUT");
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_MainDF.Show();
            m_MainDF.Activate();
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.CheckForUpdatesManual();
        }
  
        private void cmpSysTrayIcon_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                if (!cmpSystrayContextMenu.Visible) {
                    cmpSystrayContextMenu.Show();
                    cmpSystrayContextMenu.Location = Cursor.Position;
                }
                else {
                    cmpSystrayContextMenu.Hide();
                }

                return;
            }

            // Everything else
            cmpSystrayContextMenu.Hide();
        }

        private void cmpSetAgentExtensionToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!m_IsToolbarFullySetup) {
                MessageBox.Show("Log into the main application first before using this option");
                return;
            }

            ClearToolbarData();
            checkAgentLogin();
        }

        public void ApplicationExit() {
            IntellaQueueForm.ApplicationExitStatic();
        }

        private void debugToolStripMenuItem1_Click(object sender, EventArgs e) {
            m_MainDF.Show();
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
    }

    internal class IntellaQueueToolbarMissingPerQueueConfigData : Exception {
        public IntellaQueueToolbarMissingPerQueueConfigData(string message) : base(message) {

        }
    }
}
