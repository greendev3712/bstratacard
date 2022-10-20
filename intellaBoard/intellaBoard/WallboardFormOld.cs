using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Net;
using System.Collections.Specialized;
using System.Drawing;
using QueueLib;
using Lib;
using Microsoft.VisualBasic.CompilerServices;

public class wallBd {
    private bool Debug = true;
    private bool DeveloperMode = false; // Debugger.IsAttached
    private bool IsRestarting = false; // False up until we draw all the wallboard widgets for the first time

    // ' TODO: FIXME, LogoEnable = false, ClockEnabled = false, AgentsWallboard = false... the queue labels don't draw right
    private object LogoEnabled = false; // This is populated based on whether queue.wallboards theme_wallboard_logo is defined
    private object ClockEnabled = false; // This is populated based on value of queue.wallboards wallboard_clock_enabled
    private object AgentsWallboard = false; // This is populated based on value of queue.wallboards wallboard_agent_view 

    // Agent Wallboard Related
    private WallboardAgents WallboardAgentsWidget;
    private Panel MainFormWorkingPanel; // TODO: This will evetually get used by the wallboard grid abstraction too

    // Note: Registry DB Connection INFO: [HKEY_CURRENT_USER\Software\VB and VBA Program Settings\IntellaWallBoard\Config]
    private QueryResultSetRecord m_wallboard_config;
    private object WallboardResourceDirectory = @"c:\wallboard";
    private string LogFilename = WallboardResourceDirectory + @"\wallboard-";
    private FileStream LogFileHandle;
    private int LogFilesKeep = 30;
    private DateTime LogFileOpenedWhen;

    private Media.SoundPlayer SoundPlayer;

    // ''''''''''''
    // ' Images

    private string border_top_left_image_filename;
    private string border_top_image_filename;
    private string border_top_right_image_filename;
    private string border_left_image_filename;
    private string border_right_image_filename;
    private string border_bottom_left_image_filename;
    private string border_bottom_image_filename;
    private string border_bottom_right_image_filename;

    private string logo_graphic_image_filename;
    private string title_background_image_filename;
    private string bottom_graphic_image_filename;

    // ' End Images
    // ''''''''''''

    // Dimentions and location of unused area in the middle of the form that sits between the border and top/bottom graphics (we put the wallboard texts here)
    private int MainFormWorkingHeight;
    private int MainFormWorkingWidth;
    private int MainFormBottomBorderHeight;
    private int MainFormBottomGraphicWidth;
    private int MainFormBottomGraphicHeight;
    private int MainFormStartX;
    private int MainFormStartY;

    // Dimensions and location of the unused area at the top of the form that sits in between the logo and the time
    private int TopSectionStartX;
    private int TopSectionStartY;

    public string MonitoredWallboard;                           // Populated name of the wallboard we are monitoring (must exist in live_queue.v_wallboards)
    public List<string> MonitoredQueues;                     // Populated list of the queues inside the wallboard
    public Dictionary<string, int> QueueCounts;          // Populated list of the last known counts of callers waiting in queue (indexed by queue)

    private List<string> StatusLogItems;                         // Text of the status log item
    private List<DateTime> StatusLogItemsWhen;                       // When the status log item was written

    private Label StatusLogLabel;                                   // Log type text output at the top of the screen (errors, warnings, etc)
    private List<Label> QueueLabels;                             // Queue column labels indexed by position
    private List<Label> LineLabels;                              // Line labels indexed by position
    private Dictionary<string, Dictionary<string, Label>> QueueDataBoxes;   // Queue Data cells indexed by queue_name.field_name
    private PictureBox TitlesBackgroundPicture;                     // Queue labels need to be added to this control to preserve transparency
    private PictureBox BackPanel;                                   // One background picturebox for all queue data cells with colored rectangles drawn in
    private PictureBox BottomPicture;                               // Displayed at the bottom right of the main background
    private List<PictureBox> BorderPictureBoxes;
    private List<PictureBox> BorderColoredPictureBoxes;
    // Optionally we draw a thick border in this picturebox when we want to do a visual alert on the board

    private int QueueLabels_Position_Y = 150;

    public string Interface_Config_Board_BackgroundColor = "ecd8b6"; // Background color for the main wallboard screen - default is a tannish color
    public Dictionary<string, Dictionary<string, int>> Interface_Config_QueueAlerts;  // populated by wallboard config in db (per-queue alerts)  var[queue_name][alert_type] = value
    public Dictionary<int, string> Interface_Config_LongestWaiting_AudioAlertIntervals; // Intervals[Interval][WavFile]  ' populated by wallboard config in db
    public int Interface_Config_LongestWaiting_SecondsBetweenAudioAlerts; // populated by wallboard config in db
    public bool Interface_Config_Debug = false; // populated by wallboard config in db

    private object LastInterfaceUpdateWasError = 0; // count of how many db errors have happened in a row
    private object LastInterfaceUpdateWasAlerting = false;
    private DateTime LastInterfaceAudioAlertWhen;

    private int longest_waiting = 4;
    private int calls_in_queue = 0;

    public DbHelper m_db;

    private List<Control> GeneratedControls = new List<Control>();

    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    // ' Startup and Restart
    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    public wallBd() {
        InitializeComponent();
    }

    private void wallBd_Load(object sender, EventArgs e) {
        try {
            OpenAndRotateLog();
        }
        catch (Exception ex) {
            MsgBox[ex.ToString()];
            Environment.Exit(0);
        }

        try {
            CreateDBConnection();
        }
        catch (Exception ex) {
            WriteToLog(Conversions.ToString("Exception: " + Chr[13]) + Chr[10] + ex.ToString());
            throw ex;
        }
    }

    public void CreateDBConnection() {
        m_db = new DbHelper(StartupConfigurationFailure);
        ToolbarServerConnection tsc;

        tsc = new ToolbarServerConnection();
        tsc.m_db = m_db;

        ToolBarHelper.SetToolbarServerConnection(tsc);
        ToolBarHelper.SetRegistryParent("IntellaWallBoard");

        try {
            ToolBarHelper.DatabaseSetupCheck(m_db, DatabaseSuccess, StartupConfigurationFailure);
        }
        catch (Exception ex) {
            WriteToLog("Database connection Failed: " + ex.Message);
            MsgBox["Database connection Failed: " + ex.Message];
            SaveSetting["IntellaWallBoard", "Config", "DB_Host", ""];
            Environment.Exit(0);
        }
    }

    public void DatabaseSuccess() {
        m_db.setErrorHandler(DatabaseErrorCallback);
        StartupWallboard(true);
    }

    public void StartupWallboard(bool firstRun) {
        qd("Starting Wallboard");

        if (DeveloperMode != true) {
            Button1.Hide();
            Button2.Hide();
            Button3.Hide();
            Button4.Hide();
        }

        // Delayed loading so we can draw the form right away, and show the "Loading..." label while we fetch web resources
        var start_drawing_timer = new Timer();
        start_drawing_timer.Interval = 1;

        LoadingLabel.Show();

        start_drawing_timer.Tick += (object timer_sender, System.EventArgs timer_e) => {
            start_drawing_timer.Stop();

            InitializeMainConfig();

            if (firstRun)
                DrawWallBoardGraphics();

            if (Operators.ConditionalCompareObjectEqual(AgentsWallboard, true, false))
                DrawWallboardAgentWidgets();
            else
                DrawWallboardGridWidgets();

            LoadingLabel.Hide();

            if (string.IsNullOrEmpty(MonitoredWallboard)) {
                selectQueue.Visible = true;
                return;
            }

            qd("Start Update Timer");
            this.Timer1.Start();
            StatusLogAppend("Wallboard updates have started");
        };

        start_drawing_timer.Start();
    }

    public void RestartWallboard() {
        this.Show(); // In case we were hidden by a startup failure

        qd("Restarting Wallboard");
        qd("Stop Update Timer");
        this.Timer1.Stop();
        IsRestarting = true;

        // ' Controls we don't want to lose, and don't need to regenerate
        // BorderPictureBoxes.Item(3).Controls.Remove(Me.ConfigButton)

        // Me.XbuttonOn.Parent = Me
        // Me.XbuttonOff.Parent = Me
        // Me.ConfigButton.Parent = Me
        // Me.StatusUpdatingGreenPictureBox.Parent = Me
        // Me.StatusUpdatingGrayPictureBox.Parent = Me
        // Me.StatusUpdatingStoppedPictureBox.Parent = Me

        if (Operators.ConditionalCompareObjectEqual(AgentsWallboard, true, false))
            DestroyWallboardAgentWidgets();
        else
            DestroyWallboardGridWidgets();

        for (int i = GeneratedControls.Count - 1; i >= 0; i += -1) {
            var c = GeneratedControls[i];
            GeneratedControls.Remove(c);
            c.Dispose();
        }

        // Me.Controls.Clear()

        // Reset ourselves to default globals
        LogoEnabled = false;
        ClockEnabled = false;
        AgentsWallboard = false;

        StartupWallboard(true);
    }

    private void StartupConfigurationFailure(string message) {
        if (message != null)
            message = "  " + message;

        MsgBox["Database connection failed." + message];
        SaveSetting["IntellaWallBoard", "Config", "DB_Host", ""];
        Environment.Exit(0);
    }

    private void StartupConfigurationFailure(Exception ex, string message) {
        if (message != null)
            message = "  " + message;

        MsgBox["Database connection failed." + message];
        SaveSetting["IntellaWallBoard", "Config", "DB_Host", ""];
        Environment.Exit(0);
    }

    private void InitializeMainConfig() {
        MonitoredWallboard = GetSetting["intellaBoard", "queue", "Wallboard"];

        // (Re)init alerts (in case we're the first loaded wallboard, or we're switching wallboards)
        Interface_Config_LongestWaiting_AudioAlertIntervals = new Dictionary<int, string>();
        Interface_Config_Debug = false;
        bool new_call_visual_alert = false;           // true/false whether to display a visual alert when there's a new call in queue
        int longest_waiting_alert = 0;    // number of seconds before starting to alert for calls waiting too long

        // Get Wallboard Configuration
        m_wallboard_config = m_db.DbSelectSingleRow("SELECT * FROM queue.wallboards WHERE wallboard_name = '{0}'", MonitoredWallboard);

        if (m_wallboard_config.Count == 0) {
            MsgBox["Wallboard configuration does not exist for selected wallboard: " + MonitoredWallboard];
            Timer1.Stop();
            this.Hide();
            selectQueue.Visible = true;
            return;
        }

        if (m_wallboard_config.Item("debug") == "True")
            Interface_Config_Debug = true;

        if (m_wallboard_config.Item("newcaller_visual_alert") == "True")
            new_call_visual_alert = true;

        if (m_wallboard_config.Item("board_bgcolor") != "")
            Interface_Config_Board_BackgroundColor = m_wallboard_config.Item("board_bgcolor");

        if (m_wallboard_config.Item("theme_wallboard_logo") != "")
            LogoEnabled = true;

        if (m_wallboard_config.Item("wallboard_clock_enabled") == "True")
            ClockEnabled = true;

        if (m_wallboard_config.Item("wallboard_agent_view") == "True")
            AgentsWallboard = true;

        longest_waiting_alert = m_wallboard_config.Item("longest_waiting_audio_alert_seconds");

        // Repeat the last audio alert for longest waiting every x seconds
        Interface_Config_LongestWaiting_SecondsBetweenAudioAlerts = m_wallboard_config.Item("longest_waiting_audio_alert_repeat_seconds");

        // ''''''''''''''''''''
        // ' Alerting per-queue

        var wallboard_cfg_perqueue_result = new List<OrderedDictionary>();
        m_db.dbQuery("SELECT * FROM queue.v_wallboard_queue_cfg WHERE wallboard_name = '" + MonitoredWallboard + "'", wallboard_cfg_perqueue_result);

        Interface_Config_QueueAlerts = new Dictionary<string, Dictionary<string, int>>();

        foreach (OrderedDictionary monitored_queue in wallboard_cfg_perqueue_result) {
            string queue_name = monitored_queue.Item("queue_name");

            var alert_types = new Dictionary<string, int>();
            Interface_Config_QueueAlerts.Add(queue_name, alert_types);

            if (monitored_queue.Item("enable_alert").ToString() == "True") {
                alert_types.Add("longest_waiting", longest_waiting_alert);

                if (new_call_visual_alert)
                    alert_types.Add("new_caller_visual_alert", 1);
                else
                    alert_types.Add("new_caller_visual_alert", 0);
            }
            else {
                alert_types.Add("longest_waiting", 0);
                alert_types.Add("new_caller_visual_alert", 0);
            }
        }

        string alert_intervals = m_wallboard_config.Item("longest_waiting_audio_alert_seconds");
        if (!string.IsNullOrEmpty(alert_intervals)) {
            Array alert_intervals_list = alert_intervals.Split(",");

            string alert_sounds = m_wallboard_config.Item("longest_waiting_audio_alert_sounds");
            Array alert_sounds_list = "".Split(",");
            if (!string.IsNullOrEmpty(alert_sounds))
                alert_sounds_list = alert_sounds.Split(",");

            for (int i = 0, loopTo = alert_intervals_list.Length - 1; i <= loopTo; i++) {
                try {
                    Interface_Config_LongestWaiting_AudioAlertIntervals.Add(Conversions.ToInteger(int.Parse(alert_intervals_list.GetValue(i))), alert_sounds_list.GetValue(i));
                }
                catch (Exception e) {
                }
            }
        }

        string server_address = GetSetting["IntellaWallBoard", "Config", "DB_Host"];
        string url = "http://" + server_address + "/pbx/getFile.fcgi?type=audio&file=alert.wav";

        try {
            MemoryStream ms = GetSoundFromWebServer("alert.wav");
            SoundPlayer = new Media.SoundPlayer();
            SoundPlayer.Stream = ms;
            SoundPlayer.LoadAsync();
        }
        catch {
        }
    }

    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    // ' DB Error Handling
    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    private void DatabaseErrorCallback(Exception ex, string errorMsg) {
        if (LastInterfaceUpdateWasError >= 15) {
            this.Timer1.Stop();
            MsgBox[Conversions.ToString("There was a problem communicating with the server.  Please restart the application." + Chr[10]) + Chr[10] + "If the problem persists, please contact support"];
            return;
        }

        if (LastInterfaceUpdateWasError >= 5) {
            StatusLogAppend("Reconnecting to server...");
            m_db.connect();
            StatusLogAppend("Connected");
            return;
        }

        if (Debugger.IsAttached)
            MessageBox.Show("DatabaseError: " + errorMsg);

        LastInterfaceUpdateWasError = LastInterfaceUpdateWasError + 1;
        StatusLogAppend("Could not retrieve wallboard data, trying again...");
    }


    private void DatabaseErrorCallback(string errorMsg) {
        if (LastInterfaceUpdateWasError >= 15) {
            this.Timer1.Stop();
            MsgBox[Conversions.ToString("There was a problem communicating with the server.  Please restart the application." + Chr[10]) + Chr[10] + "If the problem persists, please contact support"];
            return;
        }

        if (LastInterfaceUpdateWasError >= 5) {
            StatusLogAppend("Reconnecting to server...");
            m_db.connect();
            StatusLogAppend("Connected");
            return;
        }

        if (Debugger.IsAttached)
            MessageBox.Show("DatabaseError: " + errorMsg);

        LastInterfaceUpdateWasError = LastInterfaceUpdateWasError + 1;
        StatusLogAppend("Could not retrieve wallboard data, trying again...");
    }

    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    // ' Logging and Debug
    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    private void RotateLogFileIfNeeded() {
        if (LogFileOpenedWhen.Day == Now.Day)
            return;

        LogFileHandle.Close();
        OpenAndRotateLog();
    }

    private void OpenAndRotateLog() {
        if (!Directory.Exists(WallboardResourceDirectory))
            Directory.CreateDirectory(WallboardResourceDirectory);

        DateTime today = Now;
        string logsuffix = string.Format("{0}{1,2:D2}{2,2:D2}", today.Year, today.Month, today.Day);
        string todays_logfilename = LogFilename + logsuffix + ".log";

        if (!File.Exists(todays_logfilename)) {
            // See if there's any old logs we need to delete
            var di = new System.IO.DirectoryInfo(WallboardResourceDirectory);
            IO.FileInfo[] files_info = di.GetFiles();
            IO.FileInfo file_info;
            var filenames_list = new List<string>();

            foreach (var file_info in files_info)
                filenames_list.Add(file_info.FullName);

            filenames_list.Sort();

            int logfiles_count = filenames_list.Count;
            int logfiles_delete = logfiles_count - LogFilesKeep;

            if (logfiles_delete > 0) {
                for (int i = 0, loopTo = logfiles_delete - 1; i <= loopTo; i++) {
                    string filename = filenames_list[i];

                    if (filename.StartsWith(LogFilename))
                        File.Delete(filename);
                }
            }

            LogFileHandle = File.Create(todays_logfilename);
        }
        else
            LogFileHandle = File.Open(todays_logfilename, FileMode.Append, FileAccess.Write);

        LogFileOpenedWhen = Now;
    }

    public void qd(string message) {
        if (Interface_Config_Debug != true)
            return;

        WriteToLog(message);

        var result = new List<OrderedDictionary>();

        try {
            m_db.dbQuery(string.Format("SELECT * FROM asterisk.log_print_str('wallboard', '{0} -- {1}')", MonitoredWallboard, message), result);
        }
        catch (Exception ex) {
        }
    }

    public void WriteToLog(string message) {
        var line = new UTF8Encoding(true).GetBytes(Conversions.ToString(Conversions.ToString(TimeOfDay.ToString(Now) + " " + message) + Chr[13]) + Chr[10]);
        LogFileHandle.Write(line, 0, line.Length);
    }


    public void StatusLogAppend(string msg) {
        WriteToLog(msg);

        if (StatusLogItems == null) {
            MsgBox[Conversions.ToString("Tried to log to StatusLog, but StatusLog Not Ready. Message Follows:" + Chr[10]) + Chr[10] + msg];
            return;
        }

        StatusLogItems.Add(msg);
        StatusLogItemsWhen.Add(DateTime.Now);
        StatusLogUpdate();

        if (StatusLogItems.Count > 6) {
            StatusLogItems.RemoveAt(0);
            StatusLogItemsWhen.RemoveAt(0);
        }
    }

    public void StatusLogUpdate() {
        StatusLogLabel.Text = "";

        if (StatusLogItems.Count == 0)
            return;

        // Expire out old statuslog messages
        for (int i = StatusLogItems.Count - 1; i >= 0; i += -1) {
            if ((DateTime.Now - StatusLogItemsWhen[i]).TotalSeconds > 30) {
                StatusLogItems.RemoveAt(i);
                StatusLogItemsWhen.RemoveAt(i);
            }
        }

        // Display current messages
        if (StatusLogItems.Count == 0)
            return;

        for (int i = 0, loopTo = StatusLogItems.Count - 1; i <= loopTo; i++)
            StatusLogLabel.Text = Conversions.ToString(StatusLogLabel.Text + StatusLogItemsWhen[i].ToString() + " " + StatusLogItems[i]) + Chr[10];
    }

    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    // ' Updates and Helpers
    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    private void Timer1_Tick(object sender, EventArgs e) {
        // Try
        updateWallboardDisplay();
    }

    private void updateWallboardDisplay() {
        StatusLogUpdate();
        RotateLogFileIfNeeded();

        // ' Toggle Green/Gray Blinker showing that we're updating
        if (this.StatusUpdatingGreenPictureBox.Visible == true) {
            this.StatusUpdatingGrayPictureBox.BringToFront();
            this.StatusUpdatingGrayPictureBox.Visible = true;
            this.StatusUpdatingGreenPictureBox.Visible = false;
            this.StatusUpdatingStoppedPictureBox.Visible = false;
        }
        else {
            this.StatusUpdatingGreenPictureBox.BringToFront();
            this.StatusUpdatingGreenPictureBox.Visible = true;
            this.StatusUpdatingGrayPictureBox.Visible = false;
            this.StatusUpdatingStoppedPictureBox.Visible = false;
        }

        // ' Clock
        string myDateNow;
        string myTimeNow;
        myDateNow = Format[Now, "MMMM, d"];
        myTimeNow = Format[Now, "Short time"];
        this.lblDate.Text = myDateNow;
        this.lblTime.Text = myTimeNow;

        // TODO: Investigate this... BottomPicture randomly pops to the front after updating the Queue Stats Wallboard
        BottomPicture.SendToBack();

        // Update Specific Wallboard

        var last_db_error_count = LastInterfaceUpdateWasError;

        if (Operators.ConditionalCompareObjectEqual(AgentsWallboard, true, false))
            WallboardAgentsWidget.Update();
        else
            UpdateWallboardStatsTableDisplay();

        if (LastInterfaceUpdateWasError > last_db_error_count)
            // ' We got an error from the above update
            // ' DatabaseErrorCallback handles errors and reconnecting
            return;

        // Got a good query result
        if (LastInterfaceUpdateWasError)
            StatusLogAppend("Wallboard updates have resumed successfully.");

        LastInterfaceUpdateWasError = 0;
    }

    private void UpdateWallboardStatsTableDisplay() {
        string queue_name;
        string queue_longname;
        int pos;
        string field_label;
        string field_name;
        string value;
        string hidden;

        // '''''''''''''''''''''
        // Queue Info

        var data_result = new List<OrderedDictionary>();
        var query_result = m_db.dbQuery("SELECT * FROM live_queue.v_wallboard WHERE wallboard_name = '" + MonitoredWallboard + "'", data_result);
        if (query_result == 0)
            // ' Errors are handled in DatabaseErrorCallback
            return;

        bool alerting_any = false; // are we alerting for any queues
        bool need_alert = false;
        bool need_audio_alert = false;

        for (int i = 0, loopTo = data_result.Count - 1; i <= loopTo; i++) {
            queue_name = data_result.ElementAt(i).Item("queue_name");
            queue_longname = data_result.ElementAt(i).Item("queue_longname");
            pos = data_result.ElementAt(i).Item("pos");
            field_label = data_result.ElementAt(i).Item("field_label");
            field_name = data_result.ElementAt(i).Item("field_name");
            value = data_result.ElementAt(i).Item("value");
            hidden = data_result.ElementAt(i).Item("hidden");

            // '''''''
            // Alerts

            if (Interface_Config_QueueAlerts[queue_name]["new_caller_visual_alert"] != 0) {
                alerting_any = true;

                if ((field_name ?? "") == "CALLS_IN_QUEUE") {
                    if (DeveloperMode == true)
                        value = Conversions.ToString(calls_in_queue);

                    int last_count = QueueCounts[queue_name];
                    if (Conversions.ToDouble(value) > last_count) {
                        qd(string.Format("Need Alert: Queue Count Increase. Old Value: {0}, New Value: {1}", last_count, value));
                        StatusLogAppend(string.Format("New call in queue: {0}", queue_longname));
                        need_alert = true;
                    }

                    QueueCounts[queue_name] = Conversions.ToInteger(value);
                }
            }

            // ''''''''
            // Debug '
            // ''''''''

            if ((field_name ?? "") == "CUR_LONGEST_WAITING") {
                if (DeveloperMode == true)
                    value = Conversions.ToString(longest_waiting);
            }
            else if ((field_name ?? "") == "CUR_LONGEST_WAITING")
                value = "0"; // '''''''''''''''''''''''
            else if ((field_name ?? "") == "AVG_ANSWER_SPEED_DAY")
                value = "0";// '''''''''''''''''''''''

            // ''''''''''''''''''
            // Handle Alerting '
            // ''''''''''''''''''

            int longest_waiting_start_alerting = Interface_Config_QueueAlerts[queue_name]["longest_waiting"];
            if (longest_waiting_start_alerting != 0) {
                alerting_any = true;

                if ((field_name ?? "") == "CUR_LONGEST_WAITING_SECONDS") {
                    if (DeveloperMode == true)
                        value = Conversions.ToString(longest_waiting);

                    // TODO: for each interval check if we're >= and then play the sound
                    if (value > Interface_Config_LongestWaiting_AudioAlertIntervals.Keys.First) {
                        // MsgBox("Playing " & Interface_Config_LongestWaitingAudioAlertIntervals.Item(0))
                        qd(string.Format("Need Alert: Longest Waiting Too Long.  Threshold: {0}, Longest Waiting: {1}", Interface_Config_LongestWaiting_AudioAlertIntervals.Keys.First, value));

                        TimeSpan time_since_last_alert = Now - LastInterfaceAudioAlertWhen;
                        if (time_since_last_alert.TotalSeconds >= longest_waiting_start_alerting) {
                            if (time_since_last_alert.TotalSeconds >= Interface_Config_LongestWaiting_SecondsBetweenAudioAlerts) {
                                qd(string.Format("Show Alert: Longest Waiting in queue {0} Too Long.  Seconds since last alert: {1}", queue_longname, time_since_last_alert.TotalSeconds));
                                StatusLogAppend(string.Format("Call in queue {0} has been waiting longer than {1} seconds.  Alert!", queue_longname, longest_waiting_start_alerting));
                                need_audio_alert = true;
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(value))
                value = "-";

            if ((hidden ?? "") == "true")
                continue;

            try {
                var queue_data_box = QueueDataBoxes[queue_name][field_name];
                queue_data_box.Text = value;
                queue_data_box.BringToFront();
            }
            catch {
                StatusLogAppend(string.Format("Error: Could not populate item: {0}, for queue: {1}", field_name, queue_name));
            }
        }

        if (alerting_any) {
            if (need_alert) {
                if (LastInterfaceUpdateWasAlerting) {
                    qd(string.Format("Clear Alert: Queue Count Increase"));
                    need_alert = false;
                }
                else {
                    qd(string.Format("Show Alert: Queue Count Increase"));
                    need_alert = true;
                }
            }
            else if (LastInterfaceUpdateWasAlerting)
                qd(string.Format("Clear Alert: Queue Count Increase"));

            ProcessBorderAlert(Conversions.ToInteger(need_alert));
        }
        else if (LastInterfaceUpdateWasAlerting) {
            // Might have just switched from alerting to non-alerting, clear out old border alerts
            qd(string.Format("Clear Alert: Queue Count Increase"));
            ProcessBorderAlert(Conversions.ToInteger(false));
        }

        if (need_audio_alert) {
            SoundPlayer.Play();
            LastInterfaceAudioAlertWhen = Now;
        }
    }

    private void ProcessBorderAlert(int needAlert) {
        if (needAlert) {
            for (int i = 0, loopTo = BorderPictureBoxes.Count - 1; i <= loopTo; i++) {
                var border_item = BorderPictureBoxes[i];
                var colored_border_item = BorderColoredPictureBoxes[i];

                foreach (Control c in border_item.Controls)
                    c.Parent = colored_border_item;
            }

            foreach (PictureBox border_item in BorderPictureBoxes)
                border_item.Hide();

            foreach (PictureBox colored_border_item in BorderColoredPictureBoxes)
                colored_border_item.Show();

            LastInterfaceUpdateWasAlerting = true;
        }
        else {
            for (int i = 0, loopTo1 = BorderPictureBoxes.Count - 1; i <= loopTo1; i++) {
                var border_item = BorderPictureBoxes[i];
                var colored_border_item = BorderColoredPictureBoxes[i];

                foreach (Control c in colored_border_item.Controls)
                    c.Parent = border_item;
            }


            foreach (PictureBox colored_border_item in BorderColoredPictureBoxes)
                colored_border_item.Hide();

            foreach (PictureBox border_item in BorderPictureBoxes)
                border_item.Show();

            LastInterfaceUpdateWasAlerting = false;
        }
    }

    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    // ' Draw the Interface
    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    public void DrawWallBoardGraphics() {
        qd(string.Format("Draw Wallboard Graphics"));

        // leave some room to click on the studio interface
        if (Debugger.IsAttached == true) {
            // Almost full screen, so we can click back on studio
            this.Width = Screen.PrimaryScreen.Bounds.Width - 50;
            this.Height = Screen.PrimaryScreen.Bounds.Height - 50;
        }
        else {
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.Height = Screen.PrimaryScreen.Bounds.Height;
        }

        BorderPictureBoxes = new List<PictureBox>();
        BorderColoredPictureBoxes = new List<PictureBox>();

        border_top_left_image_filename = m_wallboard_config.Item("theme_wallboard_border_top_left");     // default "border-top-left.png"
        border_top_image_filename = m_wallboard_config.Item("theme_wallboard_border_top");          // default "border-top.png"
        border_top_right_image_filename = m_wallboard_config.Item("theme_wallboard_border_top_right");    // default "border-top-right.png"
        border_left_image_filename = m_wallboard_config.Item("theme_wallboard_border_left");         // default "border-left.png"
        border_right_image_filename = m_wallboard_config.Item("theme_wallboard_border_right");        // default "border-right.png"
        border_bottom_left_image_filename = m_wallboard_config.Item("theme_wallboard_border_bottom_left");  // default "border-bottom-left.png"
        border_bottom_image_filename = m_wallboard_config.Item("theme_wallboard_border_bottom");       // default "border-bottom.png"
        border_bottom_right_image_filename = m_wallboard_config.Item("theme_wallboard_border_bottom_right"); // default "border-bottom-right.png"
        logo_graphic_image_filename = m_wallboard_config.Item("theme_wallboard_logo");                // default "logo.png"
        title_background_image_filename = m_wallboard_config.Item("theme_wallboard_title_border");        // default "title-border.png"
        bottom_graphic_image_filename = m_wallboard_config.Item("theme_wallboard_bottom_graphic");      // default "bottom-graphic.png"

        var border_top_left_image = new Bitmap(1, 1);
        var border_top_image = new Bitmap(1, 1);
        var border_top_right_image = new Bitmap(1, 1);
        var border_left_image = new Bitmap(1, 1);
        var border_right_image = new Bitmap(1, 1);
        var border_bottom_left_image = new Bitmap(1, 1);
        var border_bottom_image = new Bitmap(1, 1);
        var border_bottom_right_image = new Bitmap(1, 1);

        var logo_graphic_image = new Bitmap(1, 1);
        var title_background_image = new Bitmap(1, 1);
        var bottom_graphic_image = new Bitmap(1, 1);

        if (!string.IsNullOrEmpty(border_top_left_image_filename))
            border_top_left_image = GetImageFromWebServer("wallboard/" + border_top_left_image_filename);
        if (!string.IsNullOrEmpty(border_top_left_image_filename))
            border_top_image = GetImageFromWebServer("wallboard/" + border_top_left_image_filename);
        if (!string.IsNullOrEmpty(border_top_image_filename))
            border_top_image = GetImageFromWebServer("wallboard/" + border_top_image_filename);
        if (!string.IsNullOrEmpty(border_top_right_image_filename))
            border_top_right_image = GetImageFromWebServer("wallboard/" + border_top_right_image_filename);
        if (!string.IsNullOrEmpty(border_left_image_filename))
            border_left_image = GetImageFromWebServer("wallboard/" + border_left_image_filename);
        if (!string.IsNullOrEmpty(border_right_image_filename))
            border_right_image = GetImageFromWebServer("wallboard/" + border_right_image_filename);
        if (!string.IsNullOrEmpty(border_bottom_left_image_filename))
            border_bottom_left_image = GetImageFromWebServer("wallboard/" + border_bottom_left_image_filename);
        if (!string.IsNullOrEmpty(border_bottom_image_filename))
            border_bottom_image = GetImageFromWebServer("wallboard/" + border_bottom_image_filename);
        if (!string.IsNullOrEmpty(border_bottom_right_image_filename))
            border_bottom_right_image = GetImageFromWebServer("wallboard/" + border_bottom_right_image_filename);

        if (!string.IsNullOrEmpty(logo_graphic_image_filename))
            logo_graphic_image = GetImageFromWebServer("wallboard/" + logo_graphic_image_filename);
        if (!string.IsNullOrEmpty(title_background_image_filename))
            title_background_image = GetImageFromWebServer("wallboard/" + title_background_image_filename);
        if (!string.IsNullOrEmpty(bottom_graphic_image_filename))
            bottom_graphic_image = GetImageFromWebServer("wallboard/" + bottom_graphic_image_filename);

        // '''''''''''''''
        // ' Top Left

        var border_top_left_picturebox = new PictureBox();

        if (!string.IsNullOrEmpty(border_top_left_image_filename)) {
            border_top_left_picturebox.BackgroundImage = border_top_left_image;
            border_top_left_picturebox.Size = new Size(border_top_left_image.Width, border_top_left_image.Height);
            border_top_left_picturebox.Location = new Point(0, 0);
            this.Controls.Add(border_top_left_picturebox);
            BorderPictureBoxes.Add(border_top_left_picturebox);
        }

        // '''''''''''''''
        // ' Top Center

        var border_top_picturebox = new PictureBox();
        var border_top_start_x = border_top_left_image.Width;
        var border_top_width = this.Width - border_top_left_image.Width - border_top_right_image.Width;

        if (!string.IsNullOrEmpty(border_top_image_filename)) {
            border_top_picturebox.BackgroundImage = border_top_image;
            border_top_picturebox.Size = new Size(border_top_width, border_top_image.Height);
            border_top_picturebox.Location = new Point(border_top_start_x, 0);
            this.Controls.Add(border_top_picturebox);
            BorderPictureBoxes.Add(border_top_picturebox);
        }

        // '''''''''''''''
        // ' Top Right

        var border_top_right_picturebox = new PictureBox();
        var border_top_right_start_x = border_top_width + border_top_left_image.Width;
        int border_top_right_start_y = 0;

        if (!string.IsNullOrEmpty(border_top_right_image_filename)) {
            border_top_right_picturebox.BackgroundImage = border_top_right_image;
            border_top_right_picturebox.Size = new Size(border_bottom_right_image.Width, border_bottom_right_image.Height);
            border_top_right_picturebox.Location = new Point(border_top_right_start_x, 0);
            this.Controls.Add(border_top_right_picturebox);
            BorderPictureBoxes.Add(border_top_right_picturebox);
        }

        // '''''''''''''''
        // ' Left

        var border_left_picturebox = new PictureBox();
        int border_left_start_y = border_top_left_image.Height;
        int border_left_height = this.Height - border_top_left_image.Height - border_bottom_left_image.Height;

        if (!string.IsNullOrEmpty(border_left_image_filename)) {
            border_left_picturebox.BackgroundImage = border_left_image;
            border_left_picturebox.Size = new Size(border_left_image.Width, border_left_height);
            border_left_picturebox.Location = new Point(0, border_left_start_y);
            this.Controls.Add(border_left_picturebox);
            BorderPictureBoxes.Add(border_left_picturebox);
        }

        // '''''''''''''''
        // ' Right

        var border_right_picturebox = new PictureBox();
        int border_right_start_y = border_top_right_image.Height;
        int border_right_start_x = this.Width - border_right_image.Width;
        int border_right_height = this.Height - border_top_right_image.Height - border_bottom_right_image.Height;

        if (!string.IsNullOrEmpty(border_right_image_filename)) {
            border_right_picturebox.BackgroundImage = border_right_image;
            border_right_picturebox.Size = new Size(border_right_image.Width, border_right_height);
            border_right_picturebox.Location = new Point(border_right_start_x, border_right_start_y);
            this.Controls.Add(border_right_picturebox);
            BorderPictureBoxes.Add(border_right_picturebox);
        }

        // '''''''''''''''
        // ' Bottom Left

        var border_bottom_left_picturebox = new PictureBox();
        var border_bottom_left_y_start = this.Height - border_bottom_left_image.Height;

        if (!string.IsNullOrEmpty(border_bottom_left_image_filename)) {
            border_bottom_left_picturebox.BackgroundImage = border_bottom_left_image;
            border_bottom_left_picturebox.Size = new Size(border_bottom_left_image.Width, border_bottom_left_image.Height);
            border_bottom_left_picturebox.Location = new Point(0, border_bottom_left_y_start);
            this.Controls.Add(border_bottom_left_picturebox);
            BorderPictureBoxes.Add(border_bottom_left_picturebox);
        }

        // '''''''''''''''
        // ' Bottom Center

        var border_bottom_picturebox = new PictureBox();
        var border_bottom_start_x = border_bottom_left_image.Width;
        var border_bottom_start_y = this.Height - border_bottom_image.Height;
        var border_bottom_width = this.Width - border_bottom_left_image.Width - border_bottom_right_image.Width;

        if (!string.IsNullOrEmpty(border_bottom_image_filename)) {
            border_bottom_picturebox.BackgroundImage = border_bottom_image;
            border_bottom_picturebox.Size = new Size(border_bottom_width, border_bottom_image.Height);
            border_bottom_picturebox.Location = new Point(border_bottom_start_x, border_bottom_start_y);
            this.Controls.Add(border_bottom_picturebox);
            BorderPictureBoxes.Add(border_bottom_picturebox);
        }

        MainFormBottomBorderHeight = border_bottom_image.Height;

        // '''''''''''''''
        // ' Bottom Right

        var border_bottom_right_picturebox = new PictureBox();
        var border_bottom_right_start_x = this.Width - border_bottom_right_image.Width;
        var border_bottom_right_start_y = this.Height - border_bottom_right_image.Height;

        if (!string.IsNullOrEmpty(border_bottom_right_image_filename)) {
            border_bottom_right_picturebox.BackgroundImage = border_bottom_right_image;
            border_bottom_right_picturebox.Size = new Size(border_bottom_left_image.Width, border_bottom_left_image.Height);
            border_bottom_right_picturebox.Location = new Point(border_bottom_right_start_x, border_bottom_right_start_y);
            this.Controls.Add(border_bottom_right_picturebox);
            BorderPictureBoxes.Add(border_bottom_right_picturebox);
        }

        // '''''''''''''''
        // ' Main Form

        // ' For Border Color replacement (For New Call Alert)
        foreach (PictureBox border_item in BorderPictureBoxes) {
            var colored_border_item = new PictureBox();
            colored_border_item.Location = border_item.Location;
            colored_border_item.Size = border_item.Size;
            colored_border_item.BackColor = Color.Green;
            colored_border_item.BringToFront();
            colored_border_item.Hide();

            this.Controls.Add(colored_border_item);
            BorderColoredPictureBoxes.Add(colored_border_item);
        }

        // ' Logo
        var logo_graphic_picturebox = new PictureBox();
        var logo_graphic_start_x = border_left_image.Width + 5;
        var logo_graphic_start_y = border_top_image.Height + 15;

        if (!string.IsNullOrEmpty(logo_graphic_image_filename)) {
            logo_graphic_picturebox.BackgroundImage = logo_graphic_image;

            logo_graphic_picturebox.Size = new Size(logo_graphic_image.Width, logo_graphic_image.Height);
            logo_graphic_picturebox.Location = new Point(logo_graphic_start_x, logo_graphic_start_y);
            logo_graphic_picturebox.BackColor = Color.Transparent;
            this.Controls.Add(logo_graphic_picturebox);
            GeneratedControls.Add(logo_graphic_picturebox);
        }
        else {
            logo_graphic_picturebox.Height = 0;
            logo_graphic_picturebox.Width = 0;
        }

        // ' Bottom Picture
        var bottom_graphic_picturebox = new PictureBox();

        if (Operators.ConditionalCompareObjectEqual(AgentsWallboard, false, false)) {
            var bottom_graphic_start_x = this.Width - border_right_image.Width - bottom_graphic_image.Width;
            var bottom_graphic_start_y = this.Height - border_bottom_image.Height - bottom_graphic_image.Height;

            bottom_graphic_picturebox.BackgroundImage = bottom_graphic_image;
            bottom_graphic_picturebox.Size = new Size(bottom_graphic_image.Width, bottom_graphic_image.Height);
            bottom_graphic_picturebox.Location = new Point(bottom_graphic_start_x, bottom_graphic_start_y);
            bottom_graphic_picturebox.BackColor = Color.Transparent;
            this.Controls.Add(bottom_graphic_picturebox);
            GeneratedControls.Add(bottom_graphic_picturebox);
        }

        BottomPicture = bottom_graphic_picturebox;
        MainFormBottomGraphicWidth = BottomPicture.Width;
        MainFormBottomGraphicHeight = BottomPicture.Height;

        // ' TODO: FIXME:  Not sure why, but if we're redrawing the wallboard and we run this block, we lose these widgets
        if (!IsRestarting) {
            this.StatusUpdatingGreenPictureBox.Parent = border_top_left_picturebox;
            this.StatusUpdatingGrayPictureBox.Parent = border_top_left_picturebox;
            this.StatusUpdatingStoppedPictureBox.Parent = border_top_left_picturebox;

            this.XbuttonOn.Parent = border_top_right_picturebox;
            this.XbuttonOn.Location = new Point(8, 8);
            this.XbuttonOff.Parent = border_top_right_picturebox;
            this.XbuttonOff.Location = new Point(8, 8);

            this.ConfigButton.Parent = border_top_picturebox;
            this.ConfigButton.Location = new Point(border_top_width - 18, 7);
        }

        // ' Background
        this.BackColor = Helper.webColorToColorObj(Interface_Config_Board_BackgroundColor);

        // '''''''''''''''''''''''''''''''''''''''''''''''''''''''
        // Borders have been drawn
        // Logo has been drawn
        // Queue Label background bar has been drawn
        // Bottom flair has been drawn (bottom graphic)
        // '''''''''''''''''''''''''''''''''''''''''''''''''''''''

        // ' Queue Titles Background Bar
        var title_background_picturebox = new PictureBox(); // Used for the regular wallboard, not used for the agent detail display

        // TODO: We should be having some sort of main app mode handled at a higher level
        if (Operators.ConditionalCompareObjectEqual(AgentsWallboard, true, false)) {
            title_background_picturebox.Height = 0;
            title_background_picturebox.Width = 0;
            bottom_graphic_picturebox.Height = 0;
            bottom_graphic_picturebox.Width = 0;
        }
        else {
            // ' Queue Titles Background Bar
            var title_border_start_x = border_left_image.Width;
            var title_border_start_y = logo_graphic_start_y + logo_graphic_picturebox.Size.Height; // QueueLabels_Position_Y

            title_background_picturebox.BackgroundImage = title_background_image;
            title_background_picturebox.Size = new Size(this.Width - border_left_image.Width - border_right_image.Width, title_background_image.Height);
            title_background_picturebox.Location = new Point(title_border_start_x, title_border_start_y);
            title_background_picturebox.BackColor = Color.Transparent;
            this.Controls.Add(title_background_picturebox);
            GeneratedControls.Add(title_background_picturebox);

            TitlesBackgroundPicture = title_background_picturebox;
        }

        // '''
        // Update calculated dimensions for figuring out font sizes when we draw our main text widgets

        TopSectionStartX = border_left_picturebox.Width + logo_graphic_picturebox.Width + 10;
        TopSectionStartY = border_top_picturebox.Height;

        MainFormWorkingHeight = this.Height - border_top_picturebox.Height - logo_graphic_picturebox.Height - title_background_picturebox.Height - border_bottom_picturebox.Height - bottom_graphic_picturebox.Height;
        MainFormWorkingWidth = this.Width - border_left_picturebox.Width - border_right_picturebox.Width;
        MainFormStartX = border_left_picturebox.Width;
        MainFormStartY = border_top_picturebox.Height + title_background_picturebox.Height;

        // ''''''''''''
        // ' Status Log

        int status_log_offset_x = 0;
        var status_log_height = logo_graphic_picturebox.Height;

        if (LogoEnabled)
            status_log_offset_x = logo_graphic_picturebox.Width + 10;
        else
            status_log_height = 100;

        // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        // ' Not really graphics... but stuff that sticks around between wallboard stop/start

        StatusLogLabel = new Label();
        // 'StatusLogLabel.BackColor = Color.Tan
        StatusLogLabel.BackColor = Color.Transparent;
        StatusLogLabel.ForeColor = Color.Black;
        StatusLogLabel.Size = new Size(500, 100);
        StatusLogLabel.Location = new Point(MainFormStartX + status_log_offset_x, TopSectionStartY);
        this.Controls.Add(StatusLogLabel);
        GeneratedControls.Add(StatusLogLabel);

        StatusLogItems = new List<string>();
        StatusLogItemsWhen = new List<DateTime>();

        // ' We've just added the status log, our mainform is below it
        MainFormStartY += status_log_height + 1; // ' This is the spacing between the queues titlebar and the first line (label + data cell)  Ie: Line 1 "Calls In Queue     1"

        // ''''''
        // Clock

        if (ClockEnabled) {
            this.lblTime.Location = new Point(MainFormWorkingWidth - this.lblTime.Size.Width, this.lblTime.Location.Y);
            this.lblDate.Location = new Point(MainFormWorkingWidth - this.lblDate.Size.Width, this.lblDate.Location.Y);
            this.lblTime.Show();
            this.lblDate.Show();
        }
        else {
            this.lblTime.Hide();
            this.lblDate.Hide();
        }
    }

    // TODO: We should have a whole new Form module for the Agent Widgets
    // TODO: And then we can split out the wallboard grid into its own module as well

    public void DrawWallboardAgentWidgets() {
        cmpNotificationTextBox.Hide();

        qd(string.Format("Draw Wallboard Widgets"));
        if (string.IsNullOrEmpty(MonitoredWallboard))
            return;

        // Container For Per-Agent Detail Wallboard
        MainFormWorkingPanel = new Panel();
        MainFormWorkingPanel.Height = MainFormWorkingHeight;
        MainFormWorkingPanel.Width = MainFormWorkingWidth;
        MainFormWorkingPanel.Location = new Point(MainFormStartX, MainFormStartY); // Draw after the status log
        this.Controls.Add(MainFormWorkingPanel);
        GeneratedControls.Add(MainFormWorkingPanel);

        // WallboardAgentsWidget = New WallboardAgents(Me.MainFormWorkingPanel, MonitoredWallboard)
        WallboardAgentsWidget = new WallboardAgents(MainFormWorkingPanel, MonitoredWallboard);
        WallboardAgentsWidget.m_db = m_db;
        WallboardAgentsWidget.BuildAgentsWallboard();
    }

    public void DrawWallboardGridWidgets() {
        qd(string.Format("Draw Wallboard Widgets"));
        if (string.IsNullOrEmpty(MonitoredWallboard))
            return;

        MonitoredQueues = new List<string>();

        var queues_result = new List<OrderedDictionary>();
        m_db.dbQuery("SELECT queue_name,queue_longname FROM queue.v_wallboard_queue_cfg WHERE wallboard_name = '" + MonitoredWallboard + "' ORDER BY pos", queues_result);

        var labels_result = new List<OrderedDictionary>();
        m_db.dbQuery("SELECT DISTINCT field_name,field_label,pos from live_queue.v_wallboard WHERE wallboard_name = '" + MonitoredWallboard + "' AND hidden = 'false' ORDER BY pos", labels_result);

        if (queues_result.Count == 0 | labels_result.Count == 0) {
            StatusLogAppend("ERROR: Wallboard selected does not exist.  Please choose a valid Wallboard to monitor");
            return;
        }

        // '''''''''''''''
        // ' Overall Config

        // c2c888
        // b0a5a3
        // dbcc59
        // d88978

        var Brushes = new List<SolidBrush>()
        {
            new SolidBrush(Color.FromArgb(194, 200, 136)),
            new SolidBrush(Color.FromArgb(176, 165, 163)),
            new SolidBrush(Color.FromArgb(219, 204, 89)),
            new SolidBrush(Color.FromArgb(216, 137, 120)),
            new SolidBrush(Color.BurlyWood),
            new SolidBrush(Color.OliveDrab),
            new SolidBrush(Color.PeachPuff)
        };

        // Main Drawing Area for labels and data
        int WallboardAreaWidth = MainFormWorkingWidth;
        int WallboardAreaHeight = MainFormWorkingHeight - BottomPicture.Height;

        bool debug_borders = false; // '''''''''''''''''''''''''''''
        int show_queues = queues_result.Count;
        int queue_label_font_size;
        int queue_label_width;
        int queue_label_height = 26;
        int line_label_font_size;
        int line_label_height;
        int line_label_width;
        int line_label_padding_y = 5;      // Padding at the bottom of each line label
        int line_label_padding;            // Top/Bottom Padding of each line label background graphic
        int start_queue_labels_x;
        int cell_start_y = MainFormStartY;
        int cell_font_size;
        int cell_left_padding;
        int cell_top_padding = 30;               // Padding at the top of data cells
        int line_label_leftside_x_padding = 30;  // How much padding to add on the left of the first column of wallboard data
        object data_column_start_x;                 // Where to start drawing the background colors for columns, queue labels, and data cells
        int backpanel_extra_height = 0;
        // This the padding to add to the 'right side' of the sidebar Line Labels, before we start drawing the data cells

        // '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        var longest_label_text = Utils.GetLongestTextFromResult(labels_result, "field_label");
        var longest_queue_text = Utils.GetLongestTextFromResult(queues_result, "queue_longname");

        // Queue Label Width has to be the biggest of either the longest queue name, or the longest data item
        if (longest_queue_text.Length < "00:00:00".Length)
            longest_queue_text = "00:00:00";

        // '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        // HACK FOR ZELLMAN
        // Unless we have a small number of queues
        if (queues_result.Count == 1) {
            backpanel_extra_height = 40;
            WallboardAreaHeight += backpanel_extra_height;
        }

        // See what our biggest line label font size can be (based on height)
        double perfect_label_height = (WallboardAreaHeight - labels_result.Count * line_label_padding_y) / (double)labels_result.Count; // Reduce mainform size by the space taken by padding
        double perfect_label_width = WallboardAreaWidth * 0.1; // Line Labels should only take 10% of the entire working area

        // Make this configurable in the db... line label percentage of the screen
        if (queues_result.Count == 1)
            // HACK FOR ZELLMAN
            perfect_label_width = WallboardAreaWidth * 0.3;// Then line labels can take up to 50%

        var label_bestfit = Utils.FindLargestFontForBox(this, 50, perfect_label_width, perfect_label_height, longest_label_text, new Font("Microsoft Sans Serif", 1, FontStyle.Bold));
        var label_bestfit_width = label_bestfit.Item(0);
        var label_bestfit_height = label_bestfit.Item(1);
        var label_bestfit_font_size = label_bestfit.Item(2);

        line_label_width = label_bestfit_width;

        if (labels_result.Count < 3) {
            // expand line labels to fill space
            // line_label_rightside_x_padding = 10;
            line_label_padding_y = 0;
            line_label_height = Conversions.ToInteger(WallboardAreaHeight / (double)labels_result.Count - 25);
        }

        // HACK FOR ZELLMAN
        // Unless we have a small number of queues
        if (queues_result.Count == 1) {
            label_bestfit_width = 400;
            backpanel_extra_height = 40;
        }

        // queue_width is the width of each queue data cell that's drawn top to bottom
        double perfect_queue_width = (WallboardAreaWidth - perfect_label_width) / queues_result.Count - 10; // Minus some space for padding
        int perfect_queue_height = queue_label_height;
        int font_height = 0;
        int font_width = 0; // Will be the width in pixels of the longest queue label with the biggest font size we can use

        var queue_bestfit = Utils.FindLargestFontForBox(this, 50, perfect_queue_width, perfect_queue_height, longest_queue_text, new Font("Microsoft Sans Serif", 1, FontStyle.Bold));
        var queue_bestfit_width = queue_bestfit.Item(0);
        var queue_bestfit_height = queue_bestfit.Item(1);
        var queue_bestfit_font_size = queue_bestfit.Item(2) + 10;

        queue_label_width = Conversions.ToInteger(perfect_queue_width);
        queue_label_height = perfect_queue_height;
        queue_label_font_size = 17; // Font size of Queue Names at the top

        line_label_height = Conversions.ToInteger(perfect_label_height);
        line_label_font_size = label_bestfit_font_size;

        cell_font_size = line_label_font_size + 10;
        cell_left_padding = -10;

        // Hack for KWI
        // If (queues_result.Count = 2) Then
        // cell_font_size += 35
        // End If

        data_column_start_x = line_label_leftside_x_padding + perfect_label_width; // Start drawing data cells on this x

        // Calculate padding of line label background image
        line_label_padding = genericLineLabel.Image.Height - queue_label_height - 8;

        // DeveloperMode Debug Stuff
        if (DeveloperMode & TextBox1.Text != "")
            queue_label_width = int.Parse(TextBox1.Text);

        // Offset of where we start drawing data cells
        int queue_and_label_offset = font_width;
        // If (perfect_label_width > queue_label_width) Then
        // queue_and_label_offset = perfect_label_width
        // End If

        start_queue_labels_x = label_bestfit_width + 50; // ' queue_label_width + line_label_rightside_x_padding ' start drawing queue labels and data cells at this x offset

        // Make sure lines meet the minimim hard coded line graphic height
        // TODO: Dynamically build the line graphic
        if (line_label_height < this.genericLineLabel.Image.Height)
            line_label_height = this.genericLineLabel.Image.Height;

        if (queues_result.Count == 1)
            // HACK FOR ZELLMAN
            cell_font_size = 125;

        // '''''''''''''''''''''''''''''''''''
        // Debugging

        if (cmpLineLabelHeightTextBox.Text != "") {
        }

        TextBox1.Text = label_bestfit_width;
        TextBox2.Text = queue_bestfit_width;
        cmpLineLabelHeightTextBox.Text = line_label_height;

        // '''''''''''''''''''
        // ' Background Colors

        BackPanel = new PictureBox();
        // BackPanel.BorderStyle = BorderStyle.FixedSingle

        BackPanel.Location = new Point(MainFormStartX, cell_start_y);
        BackPanel.Size = new Size(WallboardAreaWidth, WallboardAreaHeight);
        this.Controls.Add(BackPanel);

        int queue_label_width_padding = 20; // Whitespace in between queue columns

        BackPanel.Paint += (object sender, System.Windows.Forms.PaintEventArgs e) => {
            Graphics g = e.Graphics;

            // Dim total_labels_height As Integer = line_label_height * labels_result.Count
            int total_labels_height = BackPanel.Height;

            int draw_x = Conversions.ToInteger(data_column_start_x);
            var loopTo = queues_result.Count;
            for (int i = 0; i <= loopTo; i++) {
                var draw_rect = new Rectangle(new Point(draw_x, 0), new Size(queue_label_width - queue_label_width_padding, total_labels_height)
);

                try {
                    g.FillRectangle(Brushes, draw_rect);
                }
                catch (Exception q) {
                    MsgBox[q.ToString];
                }

                draw_x += queue_label_width;
            }
        };

        // '''''''''''''''
        // ' Queue Columns

        QueueLabels = new List<Label>(); // Queue Column Labels, indexed by position
        QueueDataBoxes = new Dictionary<string, Dictionary<string, Label>>();  // Queue Data cells indexed by QueueName.QUeueposition
        QueueCounts = new Dictionary<string, int>(); // Queue caller counts per queue

        int col_label_x = Conversions.ToInteger(data_column_start_x);

        // For each queue cfg item in 
        for (int i = 0, loopTo = queues_result.Count - 1; i <= loopTo; i++) {
            string queue_name = queues_result.ElementAt(i).Item("queue_name");
            MonitoredQueues.Add(queue_name);
            QueueCounts.Add(queue_name, 0);
            QueueDataBoxes.Add(queue_name, new Dictionary<string, Label>()); // Initialize empty field_name mapping for this queue

            var queue_col_label = new Label();
            queue_col_label.Name = queue_name + "_ColumnLabel";
            queue_col_label.Font = new Font("Microsoft Sans Serif", queue_label_font_size, FontStyle.Bold, GraphicsUnit.Point, 1, true);
            queue_col_label.Size = new Size(queue_label_width, queue_label_height);
            queue_col_label.Location = new Point(col_label_x, 0);
            queue_col_label.Text = queues_result.ElementAt(i).Item("queue_longname");
            queue_col_label.TextAlign = ContentAlignment.MiddleLeft;
            queue_col_label.BackColor = Color.Transparent;

            if (debug_borders)
                queue_col_label.BorderStyle = BorderStyle.FixedSingle;

            col_label_x += queue_label_width;

            QueueLabels.Add(queue_col_label);

            TitlesBackgroundPicture.Controls.Add(queue_col_label);
        }

        // ''''''''''''''''''''''''''''''''
        // ' Data Item Labels (Sidebar Labels), Like 'Longest Waiting'

        LineLabels = new List<Label>(); // Line labels indexed by position

        int line_label_y = 0; // position relative to inside backpanel

        // Each Line Label's width goes across the entire MainForm
        // Each Line Label's height is calculated based on the number of line items and the biggest font height we can display for each line item
        for (int i = 0, loopTo1 = labels_result.Count - 1; i <= loopTo1; i++) {
            string field_name = labels_result.ElementAt(i).Item("field_name");

            var line_label = new Label();
            line_label.Name = field_name + "_LineLabel";
            line_label.Font = new Font("Microsoft Sans Serif", line_label_font_size, FontStyle.Bold, GraphicsUnit.Point, 1, true);
            line_label.Size = new Size(WallboardAreaWidth, line_label_height);
            line_label.Location = new Point(0, line_label_y); // x = 0, we start relative to the left side of the back panel
            line_label.Padding = new Padding(0, line_label_padding, 0, line_label_padding);
            line_label.Text = " " + labels_result.ElementAt(i).Item("field_label");
            line_label.TextAlign = ContentAlignment.MiddleLeft;
            line_label.BackColor = Color.Transparent;

            // '''
            // line_label.BackColor = Color.Red

            if (debug_borders)
                line_label.BorderStyle = BorderStyle.FixedSingle;

            // HACK FOR ZELLMAN
            if (queues_result.Count > 3) {
            }

            line_label.ImageAlign = ContentAlignment.TopLeft;
            line_label_y += line_label_height + line_label_padding_y;

            LineLabels.Add(line_label);
            BackPanel.Controls.Add(line_label); // add to backpanel for transparency
        }

        // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        // ' Queue Data Cells (Each Line will show data for each queue)

        int cell_label_y = cell_start_y;
        // Dim cell_label_x As Integer = cell_start_x

        for (int line = 0, loopTo2 = labels_result.Count - 1; line <= loopTo2; line++) {
            int cell_label_x = Conversions.ToInteger(data_column_start_x + line_label_leftside_x_padding);

            for (int column = 0, loopTo3 = queues_result.Count - 1; column <= loopTo3; column++) {
                string queue_name = queues_result.ElementAt(column).Item("queue_name");
                string field_name = labels_result.ElementAt(line).Item("field_name");

                var cell_label = new Label();
                cell_label.Name = Conversions.ToString(line) + "_" + Conversions.ToString(column) + "_CellLabel";
                cell_label.Font = new Font("Microsoft Sans Serif", cell_font_size);
                cell_label.Size = new Size(queue_label_width, line_label_height);

                cell_label.Location = new Point(cell_label_x + cell_left_padding, cell_top_padding);
                cell_label.Text = "-";
                cell_label.TextAlign = ContentAlignment.MiddleLeft;

                cell_label.BackColor = Color.Transparent;
                cell_label.Padding = new Padding(cell_left_padding, 0, 0, 0);
                cell_label_x += queue_label_width;

                if (debug_borders)
                    cell_label.BorderStyle = BorderStyle.FixedSingle;

                QueueDataBoxes[queue_name].Add(field_name, cell_label);
                LineLabels[line].Controls.Add(cell_label);
            }

            cell_label_y += line_label_height;
        }

        // trying to get backpanel visual size
        // Dim p As TextBox = New TextBox()
        // p.Size = New Size(BackPanel.Width, BackPanel.Height)
        // p.Location = New Point(0, 0)
        // p.Show()
        // p.BringToFront()
        // BackPanel.Controls.Add(p)

        // '''''''''''''''''''''''''''''''
        // User defined notification text
        var textbox_location_y = MainFormStartY + BackPanel.Size.Height + 10;
        int textbox_left_right_padding = 5;

        cmpNotificationTextBox.Location = new Point(MainFormStartX + textbox_left_right_padding, textbox_location_y);
        cmpNotificationTextBox.Size = new Size(MainFormWorkingWidth - textbox_left_right_padding * 2, MainFormWorkingHeight - BackPanel.Size.Height - 20);
        cmpNotificationTextBox.BackColor = this.BackColor;
        cmpNotificationTextBox.Text = GetSetting["IntellaWallBoard", "Config", "NotifyMessage"];
    }

    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    // ' File Util
    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    public object GetImageFromWebServer(string image) {
        string server_address = GetSetting["IntellaWallBoard", "Config", "DB_Host"];
        string url = "http://" + server_address + "/pbx/getImage.fcgi?image=" + image;

        // load a bitmap from a Web response stream
        try {
            return Utils.GetImageFromWebServer(url);
        }
        catch (Exception e) {
            MsgBox["Failed loading image from server: " + image + " " + e.Message];
            Environment.Exit(0);
        }

        return "";
    }

    public object GetSoundFromWebServer(string sound) {
        string server_address = GetSetting["IntellaWallBoard", "Config", "DB_Host"];
        string url = "http://" + server_address + "/pbx/getFile.fcgi?type=audio&file=" + sound;

        try {
            return Utils.GetSoundFromWebServer(url);
        }
        catch (Exception e) {
        }

        return "";
    }

    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    // ' GUI Rebuild
    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    public void DestroyWallboardAgentWidgets() {
        qd(string.Format("Destroy Wallboard Widgets"));

        if (!(BackPanel == null))
            BackPanel.Dispose();
    }

    public void DestroyWallboardGridWidgets() {
        qd(string.Format("Destroy Wallboard Widgets"));

        // destroy queue labels
        if (!(QueueLabels == null)) {
            for (int i = 0, loopTo = QueueLabels.Count - 1; i <= loopTo; i++)
                QueueLabels[i].Dispose();
        }

        // destroy line labels
        if (!(LineLabels == null)) {
            for (int i = 0, loopTo1 = LineLabels.Count - 1; i <= loopTo1; i++)
                LineLabels[i].Dispose();
        }

        // destroy data cells
        if (!(QueueDataBoxes == null)) {
            foreach (KeyValuePair<string, Dictionary<string, Label>> databox_queue_item in QueueDataBoxes) {
                var databox_queue_fields = databox_queue_item.Value;

                foreach (KeyValuePair<string, Label> databox_item in databox_queue_fields) {
                    var databox_item_label = databox_item.Value;
                    databox_item_label.Dispose();
                }
            }

            QueueDataBoxes.Clear();
        }

        if (!(BackPanel == null))
            BackPanel.Dispose();
    }

    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    // ' GUI Close
    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    private void wallBd_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e) {
        // m_db.disconnect()
        SaveSetting["IntellaWallBoard", "Config", "NotifyMessage", cmpNotificationTextBox.Text];
    }


    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    // ' GUI Bindings
    // ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    private void XbuttonOn_Click(object sender, EventArgs e) {
        SaveSetting["IntellaWallBoard", "Config", "NotifyMessage", cmpNotificationTextBox.Text];
        Environment.Exit(0);
    }

    private void XbuttonOff_Click(object sender, EventArgs e) {
        SaveSetting["IntellaWallBoard", "Config", "NotifyMessage", cmpNotificationTextBox.Text];
        Environment.Exit(0);
    }

    private void XbuttonOff_MouseHover(object sender, EventArgs e) {
        XbuttonOn.Visible = true;
        XbuttonOff.Visible = false;
    }

    private void XbuttonOn_MouseLeave(object sender, EventArgs e) {
        XbuttonOff.Visible = true;
        XbuttonOn.Visible = false;
    }

    private void Button1_Click(object sender, EventArgs e) {
        StatusLogAppend("Wallboard updates have been paused");
        Timer1.Stop();
        selectQueue.Visible = true;
    }

    private void Button1_Click_1(object sender, EventArgs e) {
        longest_waiting -= 1;
    }

    private void Button2_Click(object sender, EventArgs e) {
        longest_waiting += 1;
    }

    private void Button4_Click(object sender, EventArgs e) {
        calls_in_queue -= 1;
    }

    private void Button3_Click(object sender, EventArgs e) {
        calls_in_queue += 1;
    }

    private void cmpNotificationTextBox_TextChanged(object sender, EventArgs e) {
        TextBox t = sender;
        int max_lines = 2;
    }

    private void cmpNotificationTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
        TextBox t = sender;

        if (t.Lines.Length >= 2 & e.KeyValue.ToString == Chr[13].ToString)
            e.Handled = true;
    }

    private void Button5_Click(object sender, EventArgs e) {
        RestartWallboard();
    }
}
