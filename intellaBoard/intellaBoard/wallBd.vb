Imports System
Imports System.IO
Imports System.Net
Imports System.Collections.Specialized
Imports System.Drawing
Imports QueueLib
Imports [Lib]

Public Class wallBd
    Dim Debug As Boolean = True
    Dim DeveloperMode As Boolean = False ' Debugger.IsAttached
    Dim Debug_Borders As Boolean = False ''''''''''''''''''''''''''''''
    Dim IsRestarting As Boolean = False ' False up until we draw all the wallboard widgets for the first time

    '' TODO: FIXME, LogoEnable = false, ClockEnabled = false, AgentsWallboard = false... the queue labels don't draw right
    Dim LogoEnabled = False ' This is populated based on whether queue.wallboards theme_wallboard_logo is defined
    Dim ClockEnabled = False ' This is populated based on value of queue.wallboards wallboard_clock_enabled
    Dim AgentsWallboard = False ' This is populated based on value of queue.wallboards wallboard_agent_view 

    ' Agent Wallboard Related
    Dim WallboardAgentsWidget As WallboardAgents
    Dim MainFormWorkingPanel As Panel ' TODO: This will evetually get used by the wallboard grid abstraction too

    ' Note: Registry DB Connection INFO: [HKEY_CURRENT_USER\Software\VB and VBA Program Settings\IntellaWallBoard\Config]
    Dim m_wallboard_config As QueryResultSetRecord
    Dim WallboardResourceDirectory = "c:\wallboard"
    Dim LogFilename As String = WallboardResourceDirectory & "\wallboard-"
    Dim LogFileHandle As FileStream
    Dim LogFilesKeep As Integer = 30
    Dim LogFileOpenedWhen As Date

    Dim SoundPlayer As Media.SoundPlayer

    '''''''''''''
    '' Images

    Dim border_top_left_image_filename As String
    Dim border_top_image_filename As String
    Dim border_top_right_image_filename As String
    Dim border_left_image_filename As String
    Dim border_right_image_filename As String
    Dim border_bottom_left_image_filename As String
    Dim border_bottom_image_filename As String
    Dim border_bottom_right_image_filename As String

    Dim logo_graphic_image_filename As String
    Dim title_background_image_filename As String
    Dim bottom_graphic_image_filename As String

    '' End Images
    '''''''''''''

    ' Dimentions and location of unused area in the middle of the form that sits between the border and top/bottom graphics (we put the wallboard texts here)
    Dim MainFormWorkingHeight As Integer
    Dim MainFormWorkingWidth As Integer
    Dim MainFormBottomBorderHeight As Integer
    Dim MainFormBottomGraphicWidth As Integer
    Dim MainFormBottomGraphicHeight As Integer
    Dim MainFormStartX As Integer
    Dim MainFormStartY As Integer

    ' Dimensions and location of the unused area at the top of the form that sits in between the logo and the time
    Dim TopSectionStartX As Integer
    Dim TopSectionStartY As Integer

    Public MonitoredWallboard As String                           ' Populated name of the wallboard we are monitoring (must exist in live_queue.v_wallboards)
    Public MonitoredQueues As List(Of String)                     ' Populated list of the queues inside the wallboard
    Public QueueCounts As Dictionary(Of String, Integer)          ' Populated list of the last known counts of callers waiting in queue (indexed by queue)

    Dim StatusLogItems As List(Of String)                         ' Text of the status log item
    Dim StatusLogItemsWhen As List(Of Date)                       ' When the status log item was written

    Dim StatusLogLabel As Label                                   ' Log type text output at the top of the screen (errors, warnings, etc)
    Dim QueueLabels As List(Of Label)                             ' Queue column labels indexed by position
    Dim LineLabels As List(Of Label)                              ' Line labels indexed by position
    Dim QueueDataBoxes As Dictionary(Of String, Dictionary(Of String, Label))   ' Queue Data cells indexed by queue_name.field_name
    Dim TitlesBackgroundPicture As PictureBox                     ' Queue labels need to be added to this control to preserve transparency
    Dim BackPanel As PictureBox                                   ' One background picturebox for all queue data cells with colored rectangles drawn in
    Dim BottomPicture As PictureBox                               ' Displayed at the bottom right of the main background
    Dim BorderPictureBoxes As List(Of PictureBox)
    Dim BorderColoredPictureBoxes As List(Of PictureBox)
    '   Optionally we draw a thick border in this picturebox when we want to do a visual alert on the board

    Dim QueueLabels_Position_Y As Integer = 150

    Public Interface_Config_Board_BackgroundColor As String = "ecd8b6" ' Background color for the main wallboard screen - default is a tannish color
    Public Interface_Config_QueueAlerts As Dictionary(Of String, Dictionary(Of String, Integer))  ' populated by wallboard config in db (per-queue alerts)  var[queue_name][alert_type] = value
    Public Interface_Config_LongestWaiting_AudioAlertIntervals As Dictionary(Of Integer, String) 'Intervals[Interval][WavFile]  ' populated by wallboard config in db
    Public Interface_Config_LongestWaiting_SecondsBetweenAudioAlerts As Integer ' populated by wallboard config in db
    Public Interface_Config_Debug As Boolean = False ' populated by wallboard config in db

    Dim LastInterfaceUpdateWasError = 0 ' count of how many db errors have happened in a row
    Dim LastInterfaceUpdateWasAlerting = False
    Dim LastInterfaceAudioAlertWhen As Date

    Dim longest_waiting As Integer = 4
    Dim calls_in_queue As Integer = 0

    Public m_db As DbHelper

    Private GeneratedControls As List(Of Control) = New List(Of Control)

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    '' Startup and Restart
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub wallBd_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'DeveloperMode = Debugger.IsAttached
        Debug_Borders = DeveloperMode

        Try
            OpenAndRotateLog()
        Catch ex As Exception
            MsgBox(ex.ToString)
            End
        End Try

        Try
            CreateDBConnection()
        Catch ex As Exception
            WriteToLog("Exception: " & Chr(13) & Chr(10) & ex.ToString)
            Throw (ex)
        End Try
    End Sub

    Public Sub CreateDBConnection()
        m_db = New DbHelper(AddressOf StartupConfigurationFailure)
        Dim tsc As ToolbarServerConnection

        tsc = New ToolbarServerConnection()
        tsc.m_db = m_db

        ToolBarHelper.SetToolbarServerConnection(tsc)
        ToolBarHelper.SetRegistryParent("IntellaWallBoard")

        Try
            ToolBarHelper.DatabaseSetupCheck(m_db, AddressOf DatabaseSuccess, AddressOf StartupConfigurationFailure)
        Catch ex As Exception
            WriteToLog("Database connection Failed: " + ex.Message)
            MsgBox("Database connection Failed: " + ex.Message)
            SaveSetting("IntellaWallBoard", "Config", "DB_Host", "")
            End
        End Try
    End Sub

    Public Sub DatabaseSuccess()
        m_db.setErrorHandler(AddressOf DatabaseErrorCallback)
        StartupWallboard(True)
    End Sub

    Public Sub StartupWallboard(ByVal firstRun As Boolean)
        qd("Starting Wallboard")

        If (DeveloperMode <> True) Then
            Button1.Hide()
            Button2.Hide()
            Button3.Hide()
            Button4.Hide()
        End If

        ' Delayed loading so we can draw the form right away, and show the "Loading..." label while we fetch web resources
        Dim start_drawing_timer As Timer = New Timer()
        start_drawing_timer.Interval = 1

        LoadingLabel.Show()

        AddHandler start_drawing_timer.Tick, (
            Sub(timer_sender As System.Object, timer_e As System.EventArgs)
                start_drawing_timer.Stop()

                InitializeMainConfig()

                If (firstRun) Then
                    DrawWallBoardGraphics()
                End If

                If AgentsWallboard = True Then
                    DrawWallboardAgentWidgets()
                Else
                    DrawWallboardGridWidgets()
                End If

                LoadingLabel.Hide()

                If (MonitoredWallboard = "") Then
                    selectQueue.Visible = True
                    Exit Sub
                End If

                qd("Start Update Timer")
                Me.Timer1.Start()
                StatusLogAppend("Wallboard updates have started")
            End Sub
        )

        start_drawing_timer.Start()
    End Sub

    Sub RestartWallboard()
        Me.Show() ' In case we were hidden by a startup failure

        qd("Restarting Wallboard")
        qd("Stop Update Timer")
        Me.Timer1.Stop()
        IsRestarting = True

        '' Controls we don't want to lose, and don't need to regenerate
        'BorderPictureBoxes.Item(3).Controls.Remove(Me.ConfigButton)

        'Me.XbuttonOn.Parent = Me
        'Me.XbuttonOff.Parent = Me
        'Me.ConfigButton.Parent = Me
        'Me.StatusUpdatingGreenPictureBox.Parent = Me
        'Me.StatusUpdatingGrayPictureBox.Parent = Me
        'Me.StatusUpdatingStoppedPictureBox.Parent = Me

        If (AgentsWallboard = True) Then
            DestroyWallboardAgentWidgets()
        Else
            DestroyWallboardGridWidgets()
        End If

        For i As Integer = Me.GeneratedControls.Count() - 1 To 0 Step -1
            Dim c As Control = Me.GeneratedControls.Item(i)
            Me.GeneratedControls.Remove(c)
            c.Dispose()
        Next i

        'Me.Controls.Clear()

        ' Reset ourselves to default globals
        Me.LogoEnabled = False
        Me.ClockEnabled = False
        Me.AgentsWallboard = False

        StartupWallboard(True)
    End Sub

    Private Sub StartupConfigurationFailure(message As String)
        If (message <> Nothing) Then
            message = "  " & message
        End If

        MsgBox("Database connection failed." + message)
        SaveSetting("IntellaWallBoard", "Config", "DB_Host", "")
        End
    End Sub

    Private Sub StartupConfigurationFailure(ex As Exception, message As String)
        If (message <> Nothing) Then
            message = "  " & message
        End If

        MsgBox("Database connection failed." + message)
        SaveSetting("IntellaWallBoard", "Config", "DB_Host", "")
        End
    End Sub

    Private Sub InitializeMainConfig()
        MonitoredWallboard = GetSetting("intellaBoard", "queue", "Wallboard")

        ' (Re)init alerts (in case we're the first loaded wallboard, or we're switching wallboards)
        Interface_Config_LongestWaiting_AudioAlertIntervals = New Dictionary(Of Integer, String)
        Interface_Config_Debug = False
        Dim new_call_visual_alert = False           ' true/false whether to display a visual alert when there's a new call in queue
        Dim longest_waiting_alert As Integer = 0    ' number of seconds before starting to alert for calls waiting too long

        ' Get Wallboard Configuration
        m_wallboard_config = m_db.DbSelectSingleRow("SELECT * FROM queue.wallboards WHERE wallboard_name = '{0}'", MonitoredWallboard)

        If (m_wallboard_config.Count = 0) Then
            MsgBox("Wallboard configuration does not exist for selected wallboard: " & MonitoredWallboard)
            Timer1.Stop()
            Me.Hide()
            selectQueue.Visible = True
            Exit Sub
        End If

        If (m_wallboard_config.Item("debug") = "True") Then
            Interface_Config_Debug = True
        End If

        If (m_wallboard_config.Item("newcaller_visual_alert") = "True") Then
            new_call_visual_alert = True
        End If

        If (m_wallboard_config.Item("board_bgcolor") <> "") Then
            Interface_Config_Board_BackgroundColor = m_wallboard_config.Item("board_bgcolor")
        End If

        If (m_wallboard_config.Item("theme_wallboard_logo") <> "") Then
            LogoEnabled = True
        End If

        If (m_wallboard_config.Item("wallboard_clock_enabled") = "True") Then
            ClockEnabled = True
        End If

        If (m_wallboard_config.Item("wallboard_agent_view") = "True") Then
            AgentsWallboard = True
        End If

        longest_waiting_alert = m_wallboard_config.Item("longest_waiting_audio_alert_seconds")

        ' Repeat the last audio alert for longest waiting every x seconds
        Interface_Config_LongestWaiting_SecondsBetweenAudioAlerts = m_wallboard_config.Item("longest_waiting_audio_alert_repeat_seconds")

        '''''''''''''''''''''
        '' Alerting per-queue

        Dim wallboard_cfg_perqueue_result As List(Of OrderedDictionary) = New List(Of OrderedDictionary)
        m_db.dbQuery("SELECT * FROM queue.v_wallboard_queue_cfg WHERE wallboard_name = '" & MonitoredWallboard & "'", wallboard_cfg_perqueue_result)

        Interface_Config_QueueAlerts = New Dictionary(Of String, Dictionary(Of String, Integer))

        For Each monitored_queue As OrderedDictionary In wallboard_cfg_perqueue_result
            Dim queue_name As String = monitored_queue.Item("queue_name")

            Dim alert_types As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)
            Interface_Config_QueueAlerts.Add(queue_name, alert_types)

            If (monitored_queue.Item("enable_alert").ToString() = "True") Then
                alert_types.Add("longest_waiting", longest_waiting_alert)

                If (new_call_visual_alert) Then
                    alert_types.Add("new_caller_visual_alert", 1)
                Else
                    alert_types.Add("new_caller_visual_alert", 0)
                End If
            Else
                alert_types.Add("longest_waiting", 0)
                alert_types.Add("new_caller_visual_alert", 0)
            End If

        Next

        Dim alert_intervals As String = m_wallboard_config.Item("longest_waiting_audio_alert_seconds")
        If (alert_intervals <> "") Then
            Dim alert_intervals_list As Array = alert_intervals.Split(",")

            Dim alert_sounds As String = m_wallboard_config.Item("longest_waiting_audio_alert_sounds")
            Dim alert_sounds_list As Array = "".Split(",")
            If (alert_sounds <> "") Then
                alert_sounds_list = alert_sounds.Split(",")
            End If

            For i As Integer = 0 To (alert_intervals_list.Length - 1)
                Try
                    Interface_Config_LongestWaiting_AudioAlertIntervals.Add(Integer.Parse(alert_intervals_list.GetValue(i)), alert_sounds_list.GetValue(i))
                Catch e As Exception
                End Try
            Next i
        End If

        Dim server_address As String = GetSetting("IntellaWallBoard", "Config", "DB_Host")
        Dim url As String = "http://" & server_address & "/pbx/getFile.fcgi?type=audio&file=alert.wav"

        Try
            Dim ms As MemoryStream = GetSoundFromWebServer("alert.wav")
            SoundPlayer = New Media.SoundPlayer
            SoundPlayer.Stream = ms
            SoundPlayer.LoadAsync()
        Catch
        End Try
    End Sub

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    '' DB Error Handling
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    Private Function DatabaseErrorCallback(errorLevel As QD.ERROR_LEVEL, errorToken As String, ex As Exception, msg As String, ParamArray msgFormat As String()) As String
        If (LastInterfaceUpdateWasError >= 15) Then
            Me.Timer1.Stop()
            MsgBox("There was a problem communicating with the server.  Please restart the application." & Chr(10) & Chr(10) & "If the problem persists, please contact support")
            Exit Function
        End If

        If (LastInterfaceUpdateWasError >= 5) Then
            StatusLogAppend("Reconnecting to server...")
            m_db.connect()
            StatusLogAppend("Connected")
            Exit Function
        End If

        If (Debugger.IsAttached) Then
            MessageBox.Show("DatabaseError: " & msg)
        End If

        LastInterfaceUpdateWasError = LastInterfaceUpdateWasError + 1
        StatusLogAppend("Could not retrieve wallboard data, trying again...")
    End Function


    Private Sub DatabaseErrorCallback(errorMsg As String)
        If (LastInterfaceUpdateWasError >= 15) Then
            Me.Timer1.Stop()
            MsgBox("There was a problem communicating with the server.  Please restart the application." & Chr(10) & Chr(10) & "If the problem persists, please contact support")
            Exit Sub
        End If

        If (LastInterfaceUpdateWasError >= 5) Then
            StatusLogAppend("Reconnecting to server...")
            m_db.connect()
            StatusLogAppend("Connected")
            Exit Sub
        End If

        If (Debugger.IsAttached) Then
            MessageBox.Show("DatabaseError: " & errorMsg)
        End If

        LastInterfaceUpdateWasError = LastInterfaceUpdateWasError + 1
        StatusLogAppend("Could not retrieve wallboard data, trying again...")
    End Sub

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    '' Logging and Debug
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    Private Sub RotateLogFileIfNeeded()
        If (LogFileOpenedWhen.Day = Now.Day) Then
            Exit Sub
        End If

        LogFileHandle.Close()
        OpenAndRotateLog()
    End Sub

    Private Sub OpenAndRotateLog()
        If (Not Directory.Exists(WallboardResourceDirectory)) Then
            Directory.CreateDirectory(WallboardResourceDirectory)
        End If

        Dim today As Date = Now
        Dim logsuffix As String = String.Format("{0}{1,2:D2}{2,2:D2}", today.Year, today.Month, today.Day)
        Dim todays_logfilename = LogFilename & logsuffix & ".log"

        If (Not File.Exists(todays_logfilename)) Then
            ' See if there's any old logs we need to delete
            Dim di As New IO.DirectoryInfo(WallboardResourceDirectory)
            Dim files_info As IO.FileInfo() = di.GetFiles()
            Dim file_info As IO.FileInfo
            Dim filenames_list As List(Of String) = New List(Of String)

            For Each file_info In files_info
                filenames_list.Add(file_info.FullName)
            Next

            filenames_list.Sort()

            Dim logfiles_count As Integer = filenames_list.Count
            Dim logfiles_delete As Integer = (logfiles_count - LogFilesKeep)

            If (logfiles_delete > 0) Then
                For i As Integer = 0 To (logfiles_delete - 1)
                    Dim filename As String = filenames_list.Item(i)

                    If (filename.StartsWith(LogFilename)) Then
                        File.Delete(filename)
                    End If
                Next i
            End If

            LogFileHandle = File.Create(todays_logfilename)
        Else
            LogFileHandle = File.Open(todays_logfilename, FileMode.Append, FileAccess.Write)
        End If

        LogFileOpenedWhen = Now
    End Sub

    Public Sub qd(ByVal message As String)
        If (Interface_Config_Debug <> True) Then
            Exit Sub
        End If

        WriteToLog(message)

        Dim result As List(Of OrderedDictionary) = New List(Of OrderedDictionary)

        Try
            m_db.dbQuery(String.Format("SELECT * FROM asterisk.log_print_str('wallboard', '{0} -- {1}')", MonitoredWallboard, message), result)
        Catch ex As Exception

        End Try
    End Sub

    Public Sub WriteToLog(ByVal message As String)
        Dim line As Byte() = New System.Text.UTF8Encoding(True).GetBytes(TimeOfDay.ToString(Now) & " " & message & Chr(13) & Chr(10))
        LogFileHandle.Write(line, 0, line.Length)
    End Sub


    Public Sub StatusLogAppend(ByVal msg As String)
        WriteToLog(msg)

        If (StatusLogItems Is Nothing) Then
            MsgBox("Tried to log to StatusLog, but StatusLog Not Ready. Message Follows:" & Chr(10) & Chr(10) & msg)
            Exit Sub
        End If

        StatusLogItems.Add(msg)
        StatusLogItemsWhen.Add(DateTime.Now)
        StatusLogUpdate()

        If (StatusLogItems.Count() > 6) Then
            StatusLogItems.RemoveAt(0)
            StatusLogItemsWhen.RemoveAt(0)
        End If
    End Sub

    Public Sub StatusLogUpdate()
        StatusLogLabel.Text = ""

        If (StatusLogItems.Count() = 0) Then
            Exit Sub
        End If

        ' Expire out old statuslog messages
        For i As Integer = (StatusLogItems.Count() - 1) To 0 Step -1
            If ((DateTime.Now - StatusLogItemsWhen.Item(i)).TotalSeconds > 30) Then
                StatusLogItems.RemoveAt(i)
                StatusLogItemsWhen.RemoveAt(i)
            End If
        Next

        ' Display current messages
        If (StatusLogItems.Count() = 0) Then
            Exit Sub
        End If

        For i As Integer = 0 To (StatusLogItems.Count() - 1)
            StatusLogLabel.Text = StatusLogLabel.Text & StatusLogItemsWhen.Item(i).ToString() & " " & StatusLogItems.Item(i) & Chr(10)
        Next i
    End Sub

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    '' Updates and Helpers
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        '       Try
        updateWallboardDisplay()
        '    Catch ex As Exception
        '   WriteToLog("Exception:" & Chr(13) & Chr(10) & ex.ToString)
        '    Throw (ex)
        '    End Try

    End Sub

    Private Sub updateWallboardDisplay()
        StatusLogUpdate()
        RotateLogFileIfNeeded()

        '' Toggle Green/Gray Blinker showing that we're updating
        If Me.StatusUpdatingGreenPictureBox.Visible = True Then
            Me.StatusUpdatingGrayPictureBox.BringToFront()
            Me.StatusUpdatingGrayPictureBox.Visible = True
            Me.StatusUpdatingGreenPictureBox.Visible = False
            Me.StatusUpdatingStoppedPictureBox.Visible = False
        Else
            Me.StatusUpdatingGreenPictureBox.BringToFront()
            Me.StatusUpdatingGreenPictureBox.Visible = True
            Me.StatusUpdatingGrayPictureBox.Visible = False
            Me.StatusUpdatingStoppedPictureBox.Visible = False
        End If

        '' Clock
        Dim myDateNow As String
        Dim myTimeNow As String
        myDateNow = Format(Now, "MMMM, d")
        myTimeNow = Format(Now, "Short time")
        Me.lblDate.Text = myDateNow
        Me.lblTime.Text = myTimeNow

        ' TODO: Investigate this... BottomPicture randomly pops to the front after updating the Queue Stats Wallboard
        BottomPicture.SendToBack()

        ' Update Specific Wallboard

        Dim last_db_error_count = LastInterfaceUpdateWasError

        If (AgentsWallboard = True) Then
            WallboardAgentsWidget.Update()
        Else
            UpdateWallboardStatsTableDisplay()
        End If

        If (LastInterfaceUpdateWasError > last_db_error_count) Then
            '' We got an error from the above update
            '' DatabaseErrorCallback handles errors and reconnecting
            Exit Sub
        End If

        ' Got a good query result
        If (LastInterfaceUpdateWasError) Then
            StatusLogAppend("Wallboard updates have resumed successfully.")
        End If

        LastInterfaceUpdateWasError = 0
    End Sub

    Private Sub UpdateWallboardStatsTableDisplay()
        Dim queue_name As String
        Dim queue_longname As String
        Dim pos As Integer
        Dim field_label As String
        Dim field_name As String
        Dim value As String
        Dim hidden As String

        ''''''''''''''''''''''
        ' Queue Info

        Dim data_result As List(Of OrderedDictionary) = New List(Of OrderedDictionary)
        Dim query_result = m_db.dbQuery("SELECT * FROM live_queue.v_wallboard WHERE wallboard_name = '" & MonitoredWallboard & "'", data_result)
        If (query_result = 0) Then
            '' Errors are handled in DatabaseErrorCallback
            Exit Sub
        End If

        Dim alerting_any As Boolean = False ' are we alerting for any queues
        Dim need_alert As Boolean = False
        Dim need_audio_alert As Boolean = False

        For i As Integer = 0 To (data_result.Count - 1)
            queue_name = data_result.ElementAt(i).Item("queue_name")
            queue_longname = data_result.ElementAt(i).Item("queue_longname")
            pos = data_result.ElementAt(i).Item("pos")
            field_label = data_result.ElementAt(i).Item("field_label")
            field_name = data_result.ElementAt(i).Item("field_name")
            value = data_result.ElementAt(i).Item("value")
            hidden = data_result.ElementAt(i).Item("hidden")

            ''''''''
            ' Alerts

            If (Interface_Config_QueueAlerts.Item(queue_name).Item("new_caller_visual_alert") <> 0) Then
                alerting_any = True

                If (field_name = "CALLS_IN_QUEUE") Then
                    If (DeveloperMode = True) Then value = calls_in_queue

                    Dim last_count = QueueCounts.Item(queue_name)
                    If (value > last_count) Then
                        qd(String.Format("Need Alert: Queue Count Increase. Old Value: {0}, New Value: {1}", last_count, value))
                        StatusLogAppend(String.Format("New call in queue: {0}", queue_longname))
                        need_alert = True
                    End If

                    QueueCounts.Item(queue_name) = value
                End If
            End If

            '''''''''
            ' Debug '
            '''''''''

            If (False And DeveloperMode = True) Then
                If (field_name = "CUR_LONGEST_WAITING") Then
                    value = longest_waiting
                ElseIf (field_name = "CUR_LONGEST_WAITING") Then
                    value = "0" ''''''''''''''''''''''''
                ElseIf (field_name = "AVG_ANSWER_SPEED_DAY") Then
                    value = "0" ''''''''''''''''''''''''
                End If
            End If

            '''''''''''''''''''
            ' Handle Alerting '
            '''''''''''''''''''

            Dim longest_waiting_start_alerting = Interface_Config_QueueAlerts.Item(queue_name).Item("longest_waiting")
            If (longest_waiting_start_alerting <> 0) Then
                alerting_any = True

                If (field_name = "CUR_LONGEST_WAITING_SECONDS") Then
                    If (DeveloperMode = True) Then value = longest_waiting

                    ' TODO: for each interval check if we're >= and then play the sound
                    If (value > Interface_Config_LongestWaiting_AudioAlertIntervals.Keys.First) Then
                        'MsgBox("Playing " & Interface_Config_LongestWaitingAudioAlertIntervals.Item(0))
                        qd(String.Format("Need Alert: Longest Waiting Too Long.  Threshold: {0}, Longest Waiting: {1}", Interface_Config_LongestWaiting_AudioAlertIntervals.Keys.First, value))

                        Dim time_since_last_alert As TimeSpan = (Now - LastInterfaceAudioAlertWhen)
                        If (time_since_last_alert.TotalSeconds >= longest_waiting_start_alerting) Then
                            If (time_since_last_alert.TotalSeconds >= Interface_Config_LongestWaiting_SecondsBetweenAudioAlerts) Then
                                qd(String.Format("Show Alert: Longest Waiting in queue {0} Too Long.  Seconds since last alert: {1}", queue_longname, time_since_last_alert.TotalSeconds))
                                StatusLogAppend(String.Format("Call in queue {0} has been waiting longer than {1} seconds.  Alert!", queue_longname, longest_waiting_start_alerting))
                                need_audio_alert = True
                            End If
                        End If
                    End If
                End If
            End If

            If (value = "") Then
                value = "-"
            End If

            If (hidden = "true") Then
                Continue For
            End If

            Try
                Dim queue_data_box As Label = QueueDataBoxes.Item(queue_name).Item(field_name)
                queue_data_box.Text = value
                queue_data_box.BringToFront()
            Catch
                StatusLogAppend(String.Format("Error: Could not populate item: {0}, for queue: {1}", field_name, queue_name))
            End Try

        Next i

        If (alerting_any) Then
            If (need_alert) Then
                If (LastInterfaceUpdateWasAlerting) Then
                    qd(String.Format("Clear Alert: Queue Count Increase"))
                    need_alert = False
                Else
                    qd(String.Format("Show Alert: Queue Count Increase"))
                    need_alert = True
                End If
            Else
                If (LastInterfaceUpdateWasAlerting) Then
                    qd(String.Format("Clear Alert: Queue Count Increase"))
                End If
            End If

            ProcessBorderAlert(need_alert)
        ElseIf (LastInterfaceUpdateWasAlerting) Then
            ' Might have just switched from alerting to non-alerting, clear out old border alerts
            qd(String.Format("Clear Alert: Queue Count Increase"))
            ProcessBorderAlert(False)
        End If

        If (need_audio_alert) Then
            SoundPlayer.Play()
            LastInterfaceAudioAlertWhen = Now
        End If
    End Sub

    Private Sub ProcessBorderAlert(ByVal needAlert As Integer)
        If (needAlert) Then

            For i As Integer = 0 To (BorderPictureBoxes.Count - 1)
                Dim border_item As PictureBox = BorderPictureBoxes.Item(i)
                Dim colored_border_item As PictureBox = BorderColoredPictureBoxes.Item(i)

                For Each c As Control In border_item.Controls
                    c.Parent = colored_border_item
                Next
            Next i

            For Each border_item As PictureBox In BorderPictureBoxes
                border_item.Hide()
            Next

            For Each colored_border_item As PictureBox In BorderColoredPictureBoxes
                colored_border_item.Show()
            Next

            LastInterfaceUpdateWasAlerting = True
        Else
            For i As Integer = 0 To (BorderPictureBoxes.Count - 1)
                Dim border_item As PictureBox = BorderPictureBoxes.Item(i)
                Dim colored_border_item As PictureBox = BorderColoredPictureBoxes.Item(i)

                For Each c As Control In colored_border_item.Controls
                    c.Parent = border_item
                Next
            Next i


            For Each colored_border_item As PictureBox In BorderColoredPictureBoxes
                colored_border_item.Hide()
            Next

            For Each border_item As PictureBox In BorderPictureBoxes
                border_item.Show()
            Next

            LastInterfaceUpdateWasAlerting = False
        End If
    End Sub

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    '' Draw the Interface
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    Sub DrawWallBoardGraphics()
        qd(String.Format("Draw Wallboard Graphics"))

        ' leave some room to click on the studio interface
        If (System.Diagnostics.Debugger.IsAttached = True) Then
            ' Almost full screen, so we can click back on studio
            Me.Width = Screen.PrimaryScreen.Bounds.Width - 50
            Me.Height = Screen.PrimaryScreen.Bounds.Height - 50

            'Me.Width = 1280
            'Me.Height = 720

            'Me.Width = Screen.PrimaryScreen.Bounds.Width
            'Me.Height = Screen.PrimaryScreen.Bounds.Height

            'Me.cmpRebuildWallboardPanel.Visible = True
        Else
            Me.Width = Screen.PrimaryScreen.Bounds.Width
            Me.Height = Screen.PrimaryScreen.Bounds.Height
        End If

        BorderPictureBoxes = New List(Of PictureBox)
        BorderColoredPictureBoxes = New List(Of PictureBox)

        border_top_left_image_filename = m_wallboard_config.Item("theme_wallboard_border_top_left")     ' default "border-top-left.png"
        border_top_image_filename = m_wallboard_config.Item("theme_wallboard_border_top")          ' default "border-top.png"
        border_top_right_image_filename = m_wallboard_config.Item("theme_wallboard_border_top_right")    ' default "border-top-right.png"
        border_left_image_filename = m_wallboard_config.Item("theme_wallboard_border_left")         ' default "border-left.png"
        border_right_image_filename = m_wallboard_config.Item("theme_wallboard_border_right")        ' default "border-right.png"
        border_bottom_left_image_filename = m_wallboard_config.Item("theme_wallboard_border_bottom_left")  ' default "border-bottom-left.png"
        border_bottom_image_filename = m_wallboard_config.Item("theme_wallboard_border_bottom")       ' default "border-bottom.png"
        border_bottom_right_image_filename = m_wallboard_config.Item("theme_wallboard_border_bottom_right") ' default "border-bottom-right.png"
        logo_graphic_image_filename = m_wallboard_config.Item("theme_wallboard_logo")                ' default "logo.png"
        title_background_image_filename = m_wallboard_config.Item("theme_wallboard_title_border")        ' default "title-border.png"
        bottom_graphic_image_filename = m_wallboard_config.Item("theme_wallboard_bottom_graphic")      ' default "bottom-graphic.png"

        Dim border_top_left_image As Bitmap = New Bitmap(1, 1)
        Dim border_top_image As Bitmap = New Bitmap(1, 1)
        Dim border_top_right_image As Bitmap = New Bitmap(1, 1)
        Dim border_left_image As Bitmap = New Bitmap(1, 1)
        Dim border_right_image As Bitmap = New Bitmap(1, 1)
        Dim border_bottom_left_image As Bitmap = New Bitmap(1, 1)
        Dim border_bottom_image As Bitmap = New Bitmap(1, 1)
        Dim border_bottom_right_image As Bitmap = New Bitmap(1, 1)

        Dim logo_graphic_image As Bitmap = New Bitmap(1, 1)
        Dim title_background_image As Bitmap = New Bitmap(1, 1)
        Dim bottom_graphic_image As Bitmap = New Bitmap(1, 1)

        If border_top_left_image_filename <> "" Then border_top_left_image = GetImageFromWebServer("wallboard/" & border_top_left_image_filename)
        If border_top_left_image_filename <> "" Then border_top_image = GetImageFromWebServer("wallboard/" & border_top_left_image_filename)
        If border_top_image_filename <> "" Then border_top_image = GetImageFromWebServer("wallboard/" & border_top_image_filename)
        If border_top_right_image_filename <> "" Then border_top_right_image = GetImageFromWebServer("wallboard/" & border_top_right_image_filename)
        If border_left_image_filename <> "" Then border_left_image = GetImageFromWebServer("wallboard/" & border_left_image_filename)
        If border_right_image_filename <> "" Then border_right_image = GetImageFromWebServer("wallboard/" & border_right_image_filename)
        If border_bottom_left_image_filename <> "" Then border_bottom_left_image = GetImageFromWebServer("wallboard/" & border_bottom_left_image_filename)
        If border_bottom_image_filename <> "" Then border_bottom_image = GetImageFromWebServer("wallboard/" & border_bottom_image_filename)
        If border_bottom_right_image_filename <> "" Then border_bottom_right_image = GetImageFromWebServer("wallboard/" & border_bottom_right_image_filename)

        If logo_graphic_image_filename <> "" Then logo_graphic_image = GetImageFromWebServer("wallboard/" & logo_graphic_image_filename)
        If title_background_image_filename <> "" Then title_background_image = GetImageFromWebServer("wallboard/" & title_background_image_filename)
        If bottom_graphic_image_filename <> "" Then bottom_graphic_image = GetImageFromWebServer("wallboard/" & bottom_graphic_image_filename)

        ''''''''''''''''
        '' Top Left

        Dim border_top_left_picturebox As PictureBox = New PictureBox

        If (border_top_left_image_filename <> "") Then
            border_top_left_picturebox.BackgroundImage = border_top_left_image
            border_top_left_picturebox.Size = New Size(border_top_left_image.Width, border_top_left_image.Height)
            border_top_left_picturebox.Location = New Point(0, 0)
            Me.Controls.Add(border_top_left_picturebox)
            BorderPictureBoxes.Add(border_top_left_picturebox)
        End If

        ''''''''''''''''
        '' Top Center

        Dim border_top_picturebox As PictureBox = New PictureBox
        Dim border_top_start_x = border_top_left_image.Width
        Dim border_top_width = Me.Width - border_top_left_image.Width - border_top_right_image.Width

        If (border_top_image_filename <> "") Then
            border_top_picturebox.BackgroundImage = border_top_image
            border_top_picturebox.Size = New Size(border_top_width, border_top_image.Height)
            border_top_picturebox.Location = New Point(border_top_start_x, 0)
            Me.Controls.Add(border_top_picturebox)
            BorderPictureBoxes.Add(border_top_picturebox)
        End If

        ''''''''''''''''
        '' Top Right

        Dim border_top_right_picturebox As PictureBox = New PictureBox
        Dim border_top_right_start_x = border_top_width + border_top_left_image.Width
        Dim border_top_right_start_y = 0

        If (border_top_right_image_filename <> "") Then
            border_top_right_picturebox.BackgroundImage = border_top_right_image
            border_top_right_picturebox.Size = New Size(border_bottom_right_image.Width, border_bottom_right_image.Height)
            border_top_right_picturebox.Location = New Point(border_top_right_start_x, 0)
            Me.Controls.Add(border_top_right_picturebox)
            BorderPictureBoxes.Add(border_top_right_picturebox)
        End If

        ''''''''''''''''
        '' Left

        Dim border_left_picturebox As PictureBox = New PictureBox
        Dim border_left_start_y As Integer = border_top_left_image.Height
        Dim border_left_height As Integer = Me.Height - border_top_left_image.Height - border_bottom_left_image.Height

        If (border_left_image_filename <> "") Then
            border_left_picturebox.BackgroundImage = border_left_image
            border_left_picturebox.Size = New Size(border_left_image.Width, border_left_height)
            border_left_picturebox.Location = New Point(0, border_left_start_y)
            Me.Controls.Add(border_left_picturebox)
            BorderPictureBoxes.Add(border_left_picturebox)
        End If

        ''''''''''''''''
        '' Right

        Dim border_right_picturebox As PictureBox = New PictureBox
        Dim border_right_start_y As Integer = border_top_right_image.Height
        Dim border_right_start_x As Integer = Me.Width - border_right_image.Width
        Dim border_right_height As Integer = Me.Height - border_top_right_image.Height - border_bottom_right_image.Height

        If (border_right_image_filename <> "") Then
            border_right_picturebox.BackgroundImage = border_right_image
            border_right_picturebox.Size = New Size(border_right_image.Width, border_right_height)
            border_right_picturebox.Location = New Point(border_right_start_x, border_right_start_y)
            Me.Controls.Add(border_right_picturebox)
            BorderPictureBoxes.Add(border_right_picturebox)
        End If

        ''''''''''''''''
        '' Bottom Left

        Dim border_bottom_left_picturebox As PictureBox = New PictureBox
        Dim border_bottom_left_y_start = Me.Height - border_bottom_left_image.Height

        If (border_bottom_left_image_filename <> "") Then
            border_bottom_left_picturebox.BackgroundImage = border_bottom_left_image
            border_bottom_left_picturebox.Size = New Size(border_bottom_left_image.Width, border_bottom_left_image.Height)
            border_bottom_left_picturebox.Location = New Point(0, border_bottom_left_y_start)
            Me.Controls.Add(border_bottom_left_picturebox)
            BorderPictureBoxes.Add(border_bottom_left_picturebox)
        End If

        ''''''''''''''''
        '' Bottom Center

        Dim border_bottom_picturebox As PictureBox = New PictureBox
        Dim border_bottom_start_x = border_bottom_left_image.Width
        Dim border_bottom_start_y = Me.Height - border_bottom_image.Height
        Dim border_bottom_width = Me.Width - border_bottom_left_image.Width - border_bottom_right_image.Width

        If (border_bottom_image_filename <> "") Then
            border_bottom_picturebox.BackgroundImage = border_bottom_image
            border_bottom_picturebox.Size = New Size(border_bottom_width, border_bottom_image.Height)
            border_bottom_picturebox.Location = New Point(border_bottom_start_x, border_bottom_start_y)
            Me.Controls.Add(border_bottom_picturebox)
            BorderPictureBoxes.Add(border_bottom_picturebox)
        End If

        MainFormBottomBorderHeight = border_bottom_image.Height

        ''''''''''''''''
        '' Bottom Right

        Dim border_bottom_right_picturebox As PictureBox = New PictureBox
        Dim border_bottom_right_start_x = Me.Width - border_bottom_right_image.Width
        Dim border_bottom_right_start_y = Me.Height - border_bottom_right_image.Height

        If (border_bottom_right_image_filename <> "") Then
            border_bottom_right_picturebox.BackgroundImage = border_bottom_right_image
            border_bottom_right_picturebox.Size = New Size(border_bottom_left_image.Width, border_bottom_left_image.Height)
            border_bottom_right_picturebox.Location = New Point(border_bottom_right_start_x, border_bottom_right_start_y)
            Me.Controls.Add(border_bottom_right_picturebox)
            BorderPictureBoxes.Add(border_bottom_right_picturebox)
        End If

        ''''''''''''''''
        '' Main Form

        '' For Border Color replacement (For New Call Alert)
        For Each border_item As PictureBox In BorderPictureBoxes
            Dim colored_border_item As PictureBox = New PictureBox
            colored_border_item.Location = border_item.Location
            colored_border_item.Size = border_item.Size
            colored_border_item.BackColor = Color.Green
            colored_border_item.BringToFront()
            colored_border_item.Hide()

            Me.Controls.Add(colored_border_item)
            BorderColoredPictureBoxes.Add(colored_border_item)
        Next

        '' Logo
        Dim logo_graphic_picturebox As PictureBox = New PictureBox
        Dim logo_graphic_start_x = border_left_image.Width + 5
        Dim logo_graphic_start_y = border_top_image.Height + 15

        If (logo_graphic_image_filename <> "") Then
            logo_graphic_picturebox.BackgroundImage = logo_graphic_image

            logo_graphic_picturebox.Size = New Size(logo_graphic_image.Width, logo_graphic_image.Height)
            logo_graphic_picturebox.Location = New Point(logo_graphic_start_x, logo_graphic_start_y)
            logo_graphic_picturebox.BackColor = Color.Transparent
            Me.Controls.Add(logo_graphic_picturebox)
            Me.GeneratedControls.Add(logo_graphic_picturebox)
        Else
            logo_graphic_picturebox.Height = 0
            logo_graphic_picturebox.Width = 0
        End If

        '' Bottom Picture
        Dim bottom_graphic_picturebox As PictureBox = New PictureBox

        If (AgentsWallboard = False) Then
            Dim bottom_graphic_start_x = Me.Width - border_right_image.Width - bottom_graphic_image.Width
            Dim bottom_graphic_start_y = Me.Height - border_bottom_image.Height - bottom_graphic_image.Height

            bottom_graphic_picturebox.BackgroundImage = bottom_graphic_image
            bottom_graphic_picturebox.Size = New Size(bottom_graphic_image.Width, bottom_graphic_image.Height)
            bottom_graphic_picturebox.Location = New Point(bottom_graphic_start_x, bottom_graphic_start_y)
            bottom_graphic_picturebox.BackColor = Color.Transparent
            Me.Controls.Add(bottom_graphic_picturebox)
            Me.GeneratedControls.Add(bottom_graphic_picturebox)
        End If

        BottomPicture = bottom_graphic_picturebox
        MainFormBottomGraphicWidth = BottomPicture.Width
        MainFormBottomGraphicHeight = BottomPicture.Height

        '' TODO: FIXME:  Not sure why, but if we're redrawing the wallboard and we run this block, we lose these widgets
        If (Not IsRestarting) Then
            Me.StatusUpdatingGreenPictureBox.Parent = border_top_left_picturebox
            Me.StatusUpdatingGrayPictureBox.Parent = border_top_left_picturebox
            Me.StatusUpdatingStoppedPictureBox.Parent = border_top_left_picturebox

            Me.XbuttonOn.Parent = border_top_right_picturebox
            Me.XbuttonOn.Location = New Point(8, 8)
            Me.XbuttonOff.Parent = border_top_right_picturebox
            Me.XbuttonOff.Location = New Point(8, 8)

            Me.ConfigButton.Parent = border_top_picturebox
            Me.ConfigButton.Location = New Point(border_top_width - 18, 7)
        End If

        '' Background
        Me.BackColor = Helper.webColorToColorObj(Interface_Config_Board_BackgroundColor)

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' Borders have been drawn
        ' Logo has been drawn
        ' Queue Label background bar has been drawn
        ' Bottom flair has been drawn (bottom graphic)
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        '' Queue Titles Background Bar
        Dim title_background_picturebox As PictureBox = New PictureBox ' Used for the regular wallboard, not used for the agent detail display

        ' TODO: We should be having some sort of main app mode handled at a higher level
        If AgentsWallboard = True Then
            title_background_picturebox.Height = 0
            title_background_picturebox.Width = 0
            bottom_graphic_picturebox.Height = 0
            bottom_graphic_picturebox.Width = 0
        Else
            '' Queue Titles Background Bar
            Dim title_border_start_x = border_left_image.Width
            Dim title_border_start_y = logo_graphic_start_y + logo_graphic_picturebox.Size.Height ' QueueLabels_Position_Y

            title_background_picturebox.BackgroundImage = title_background_image
            title_background_picturebox.Size = New Size(Me.Width - border_left_image.Width - border_right_image.Width, title_background_image.Height)
            title_background_picturebox.Location = New Point(title_border_start_x, title_border_start_y)
            title_background_picturebox.BackColor = Color.Transparent
            Me.Controls.Add(title_background_picturebox)
            Me.GeneratedControls.Add(title_background_picturebox)

            TitlesBackgroundPicture = title_background_picturebox
        End If

        ''''
        ' Update calculated dimensions for figuring out font sizes when we draw our main text widgets

        TopSectionStartX = border_left_picturebox.Width + logo_graphic_picturebox.Width + 10
        TopSectionStartY = border_top_picturebox.Height

        MainFormWorkingHeight = Me.Height - border_top_picturebox.Height - logo_graphic_picturebox.Height - title_background_picturebox.Height - border_bottom_picturebox.Height - bottom_graphic_picturebox.Height
        MainFormWorkingWidth = Me.Width - border_left_picturebox.Width - border_right_picturebox.Width
        MainFormStartX = border_left_picturebox.Width
        MainFormStartY = border_top_picturebox.Height + title_background_picturebox.Height

        '''''''''''''
        '' Status Log

        Dim status_log_offset_x = 0
        Dim status_log_height = logo_graphic_picturebox.Height

        If (LogoEnabled) Then
            status_log_offset_x = logo_graphic_picturebox.Width + 10
        Else
            status_log_height = 100
        End If

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '' Not really graphics... but stuff that sticks around between wallboard stop/start

        StatusLogLabel = New Label()
        ''StatusLogLabel.BackColor = Color.Tan
        StatusLogLabel.BackColor = Color.Transparent
        StatusLogLabel.ForeColor = Color.Black
        StatusLogLabel.Size = New Size(500, 100)
        StatusLogLabel.Location = New Point(MainFormStartX + status_log_offset_x, TopSectionStartY)
        Me.Controls.Add(StatusLogLabel)
        Me.GeneratedControls.Add(StatusLogLabel)

        StatusLogItems = New List(Of String)
        StatusLogItemsWhen = New List(Of Date)

        '' We've just added the status log, our mainform is below it
        MainFormStartY += status_log_height + 1 '' This is the spacing between the queues titlebar and the first line (label + data cell)  Ie: Line 1 "Calls In Queue     1"

        '''''''
        ' Clock

        If (ClockEnabled) Then
            Me.lblTime.Location = New Point(MainFormWorkingWidth - Me.lblTime.Size.Width, Me.lblTime.Location.Y)
            Me.lblDate.Location = New Point(MainFormWorkingWidth - Me.lblDate.Size.Width, Me.lblDate.Location.Y)
            Me.lblTime.Show()
            Me.lblDate.Show()
        Else
            Me.lblTime.Hide()
            Me.lblDate.Hide()
        End If
    End Sub

    ' TODO: We should have a whole new Form module for the Agent Widgets
    ' TODO: And then we can split out the wallboard grid into its own module as well

    Sub DrawWallboardAgentWidgets()
        cmpNotificationTextBox.Hide()

        qd(String.Format("Draw Wallboard Widgets"))
        If (MonitoredWallboard = "") Then
            Exit Sub
        End If

        ' Container For Per-Agent Detail Wallboard
        MainFormWorkingPanel = New Panel()
        MainFormWorkingPanel.Height = MainFormWorkingHeight
        MainFormWorkingPanel.Width = MainFormWorkingWidth
        MainFormWorkingPanel.Location = New Point(MainFormStartX, MainFormStartY) ' Draw after the status log
        Me.Controls.Add(MainFormWorkingPanel)
        Me.GeneratedControls.Add(MainFormWorkingPanel)

        'WallboardAgentsWidget = New WallboardAgents(Me.MainFormWorkingPanel, MonitoredWallboard)
        WallboardAgentsWidget = New WallboardAgents(Me.MainFormWorkingPanel, Me.MonitoredWallboard)
        WallboardAgentsWidget.m_db = m_db
        WallboardAgentsWidget.BuildAgentsWallboard()
    End Sub

    Sub DrawWallboardGridWidgets()
        qd(String.Format("Draw Wallboard Widgets"))
        If (MonitoredWallboard = "") Then
            Exit Sub
        End If

        MonitoredQueues = New List(Of String)

        Dim queues_result As List(Of OrderedDictionary) = New List(Of OrderedDictionary)
        m_db.dbQuery("SELECT queue_name,queue_longname FROM queue.v_wallboard_queue_cfg WHERE wallboard_name = '" & MonitoredWallboard & "' ORDER BY pos", queues_result)

        Dim labels_result As List(Of OrderedDictionary) = New List(Of OrderedDictionary)
        m_db.dbQuery("SELECT DISTINCT field_name,field_label,pos from live_queue.v_wallboard WHERE wallboard_name = '" & MonitoredWallboard & "' AND hidden = 'false' ORDER BY pos", labels_result)

        If ((queues_result.Count = 0) Or (labels_result.Count = 0)) Then
            StatusLogAppend("ERROR: Wallboard selected does not exist.  Please choose a valid Wallboard to monitor")
            Exit Sub
        End If

        ''''''''''''''''
        '' Overall Config

        ' c2c888
        ' b0a5a3
        ' dbcc59
        ' d88978

        Dim Brushes As List(Of SolidBrush) = New List(Of SolidBrush) From {
          New SolidBrush(Color.FromArgb(194, 200, 136)),
          New SolidBrush(Color.FromArgb(176, 165, 163)),
          New SolidBrush(Color.FromArgb(219, 204, 89)),
          New SolidBrush(Color.FromArgb(216, 137, 120)),
          New SolidBrush(Color.BurlyWood),
          New SolidBrush(Color.OliveDrab),
          New SolidBrush(Color.PeachPuff)
        }

        ' Main Drawing Area for labels and data
        Dim WallboardAreaWidth As Integer = MainFormWorkingWidth
        Dim WallboardAreaHeight As Integer = MainFormWorkingHeight - BottomPicture.Height

        Dim show_queues As Integer = queues_result.Count
        Dim queue_label_font_size As Integer
        Dim queue_label_width As Integer
        Dim queue_label_height As Integer = 26
        Dim line_label_font_size As Integer
        Dim line_label_height As Integer
        Dim line_label_width As Integer
        Dim line_label_padding_y As Integer = 5      ' Padding at the bottom of each line label
        Dim line_label_padding As Integer            ' Top/Bottom Padding of each line label background graphic
        Dim start_queue_labels_x As Integer
        Dim cell_start_y As Integer = MainFormStartY
        Dim cell_font_size As Integer
        Dim cell_left_padding As Integer
        Dim cell_top_padding As Integer = 5     ' Padding at the top of data cells
        Dim line_label_leftside_x_padding = 120  ' How much padding to add on the left of the first column of wallboard data
        Dim data_column_start_x                 ' Where to start drawing the background colors for columns, queue labels, and data cells
        Dim backpanel_extra_height = 0
        ' This the padding to add to the 'right side' of the sidebar Line Labels, before we start drawing the data cells

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        Dim longest_label_text = Utils.GetLongestTextFromResult(labels_result, "field_label")
        Dim longest_queue_text = Utils.GetLongestTextFromResult(queues_result, "queue_longname")

        ' Queue Label Width has to be the biggest of either the longest queue name, or the longest data item
        If (longest_queue_text.Length < "00:00:00".Length) Then
            longest_queue_text = "00:00:00"
        End If

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        ' HACK FOR ZELLMAN
        ' Unless we have a small number of queues
        If (queues_result.Count = 1) Then
            backpanel_extra_height = 40
            WallboardAreaHeight += backpanel_extra_height
        End If

        ' See what our biggest line label font size can be (based on height)
        Dim perfect_label_height = ((WallboardAreaHeight - (labels_result.Count * line_label_padding_y)) / labels_result.Count) ' Reduce mainform size by the space taken by padding
        Dim perfect_label_width = (WallboardAreaWidth * 0.1) ' Line Labels should only take 10% of the entire working area

        ' Make this configurable in the db... line label percentage of the screen
        If (queues_result.Count = 1) Then
            ' HACK FOR ZELLMAN
            perfect_label_width = (WallboardAreaWidth * 0.3) ' Then line labels can take up to 50%
        End If

        Dim label_bestfit = Utils.FindLargestFontForBox(Me, 60, perfect_label_width, perfect_label_height, longest_label_text, New Font("Microsoft Sans Serif", 1, FontStyle.Bold))
        Dim label_bestfit_width = label_bestfit.Item(0)
        Dim label_bestfit_height = label_bestfit.Item(1)
        Dim label_bestfit_font_size = label_bestfit.Item(2)

        line_label_width = label_bestfit_width

        If (labels_result.Count < 3) Then
            ' expand line labels to fill space
            ' line_label_rightside_x_padding = 10;
            line_label_padding_y = 0
            line_label_height = (WallboardAreaHeight / labels_result.Count) - 25
        End If

        ' HACK FOR ZELLMAN
        ' Unless we have a small number of queues
        If (queues_result.Count = 1) Then
            label_bestfit_width = 400
            backpanel_extra_height = 40
        End If

        ' queue_width is the width of each queue data cell that's drawn top to bottom
        Dim perfect_queue_width = ((WallboardAreaWidth - perfect_label_width) / queues_result.Count) - 10 ' Minus some space for padding
        Dim perfect_queue_height = queue_label_height
        Dim font_height = 0
        Dim font_width = 0 ' Will be the width in pixels of the longest queue label with the biggest font size we can use

        Dim queue_bestfit = Utils.FindLargestFontForBox(Me, 50, perfect_queue_width, perfect_queue_height, longest_queue_text, New Font("Microsoft Sans Serif", 1, FontStyle.Bold))
        Dim queue_bestfit_width = queue_bestfit.Item(0)
        Dim queue_bestfit_height = queue_bestfit.Item(1)
        Dim queue_bestfit_font_size = queue_bestfit.Item(2) + 10

        queue_label_width = perfect_queue_width
        queue_label_height = perfect_queue_height
        queue_label_font_size = 17 ' Font size of Queue Names at the top

        line_label_height = perfect_label_height
        line_label_font_size = label_bestfit_font_size + 4

        cell_font_size = line_label_font_size + 10
        cell_left_padding = -10

        ' Hack for KWI
        If (queues_result.Count = 2) Then
            cell_font_size += 24
        End If

        data_column_start_x = line_label_leftside_x_padding + perfect_label_width ' Start drawing data cells on this x

        ' Calculate padding of line label background image
        line_label_padding = genericLineLabel.Image.Height - queue_label_height - 8

        ' DeveloperMode Debug Stuff
        If (DeveloperMode And TextBox1.Text <> "") Then
            queue_label_width = Integer.Parse(TextBox1.Text)
        End If

        ' Offset of where we start drawing data cells
        Dim queue_and_label_offset = font_width
        'If (perfect_label_width > queue_label_width) Then
        '    queue_and_label_offset = perfect_label_width
        'End If

        start_queue_labels_x = label_bestfit_width + 50 '  ' queue_label_width + line_label_rightside_x_padding ' start drawing queue labels and data cells at this x offset

        ' Make sure lines meet the minimim hard coded line graphic height
        ' TODO: Dynamically build the line graphic
        If (line_label_height < Me.genericLineLabel.Image.Height) Then
            line_label_height = Me.genericLineLabel.Image.Height
        End If

        If (queues_result.Count = 1) Then
            line_label_width += 100
            line_label_font_size -= 10
            ' HACK FOR ZELLMAN
            ''cell_font_size = 125
        End If

        ''''''''''''''''''''''''''''''''''''
        ' Debugging

        If (cmpLineLabelHeightTextBox.Text <> "") Then
            'line_label_height = Integer.Parse(cmpLineLabelHeightTextBox.Text)
        End If

        TextBox1.Text = label_bestfit_width
        TextBox2.Text = queue_bestfit_width
        cmpLineLabelHeightTextBox.Text = line_label_height

        ''''''''''''''''''''
        '' Background Colors
        ''
        '' This builds the backgrounds for columns (each queue is a column)

        BackPanel = New PictureBox
        ' BackPanel.BorderStyle = BorderStyle.FixedSingle

        BackPanel.Location = New Point(MainFormStartX, cell_start_y + cell_top_padding)
        BackPanel.Size = New Size(WallboardAreaWidth, WallboardAreaHeight)
        Me.Controls.Add(BackPanel)

        Dim queue_label_width_padding = 20 ' Whitespace in between queue columns

        AddHandler BackPanel.Paint, (
            Sub(sender As Object, e As System.Windows.Forms.PaintEventArgs)
                Dim g As Graphics = e.Graphics

                Dim total_labels_height As Integer = BackPanel.Height
                Dim draw_x As Integer = data_column_start_x

                For i As Integer = 0 To queues_result.Count
                    Dim draw_rect As Rectangle = New Rectangle(
                        New Point(draw_x, 0),
                        New Size(queue_label_width - queue_label_width_padding, total_labels_height)
                    )

                    Try
                        g.FillRectangle(Brushes.Item(i), draw_rect)
                    Catch q As Exception
                        MsgBox(q.ToString)
                    End Try

                    draw_x += queue_label_width
                Next i
            End Sub
        )

        ''''''''''''''''
        '' Queue Columns

        QueueLabels = New List(Of Label) ' Queue Column Labels, indexed by position
        QueueDataBoxes = New Dictionary(Of String, Dictionary(Of String, Label))()  ' Queue Data cells indexed by QueueName.QUeueposition
        QueueCounts = New Dictionary(Of String, Integer) ' Queue caller counts per queue

        Dim col_label_x As Integer = data_column_start_x

        ' For each queue cfg item in 
        For i As Integer = 0 To (queues_result.Count - 1)
            Dim queue_name As String = queues_result.ElementAt(i).Item("queue_name")
            MonitoredQueues.Add(queue_name)
            QueueCounts.Add(queue_name, 0)
            QueueDataBoxes.Add(queue_name, New Dictionary(Of String, Label)) ' Initialize empty field_name mapping for this queue

            Dim queue_col_label = New Label
            queue_col_label.Name = queue_name & "_ColumnLabel"
            queue_col_label.Font = New Font("Microsoft Sans Serif", queue_label_font_size, FontStyle.Bold, GraphicsUnit.Point, 1, True)
            queue_col_label.Size = New Size(queue_label_width, queue_label_height)
            queue_col_label.Location = New Point(col_label_x, 0)
            queue_col_label.Text = queues_result.ElementAt(i).Item("queue_longname")
            queue_col_label.TextAlign = ContentAlignment.MiddleLeft
            queue_col_label.BackColor = Color.Transparent

            If (Debug_Borders) Then
                queue_col_label.BorderStyle = BorderStyle.FixedSingle
            End If

            col_label_x += queue_label_width

            QueueLabels.Add(queue_col_label)

            TitlesBackgroundPicture.Controls.Add(queue_col_label)
        Next i

        '''''''''''''''''''''''''''''''''
        '' Data Item Labels (Sidebar Labels), Like 'Longest Waiting'

        LineLabels = New List(Of Label) ' Line labels indexed by position

        Dim line_label_y As Integer = 0 ' position relative to inside backpanel

        ' Each Line Label's width goes across the entire MainForm
        ' Each Line Label's height is calculated based on the number of line items and the biggest font height we can display for each line item
        For i As Integer = 0 To (labels_result.Count - 1)
            Dim field_name As String = labels_result.ElementAt(i).Item("field_name")

            Dim line_label = New Label
            line_label.Name = field_name & "_LineLabel"
            line_label.Font = New Font("Microsoft Sans Serif", line_label_font_size, FontStyle.Bold, GraphicsUnit.Point, 1, True)
            line_label.Size = New Size(WallboardAreaWidth, line_label_height)
            line_label.Location = New Point(0, line_label_y) ' x = 0, we start relative to the left side of the back panel
            line_label.Padding = New Padding(0, line_label_padding, 0, line_label_padding)
            line_label.Text = " " + labels_result.ElementAt(i).Item("field_label")
            line_label.TextAlign = ContentAlignment.MiddleLeft
            line_label.BackColor = Color.Transparent

            ''''
            'line_label.BackColor = Color.Red

            If (Debug_Borders) Then
                line_label.BorderStyle = BorderStyle.FixedSingle
            End If

            ' HACK FOR ZELLMAN
            If (queues_result.Count() > 3) Then
                'line_label.Image = Me.genericLineLabel.Image
            End If

            line_label.ImageAlign = ContentAlignment.TopLeft
            line_label_y += line_label_height + line_label_padding_y

            LineLabels.Add(line_label)
            BackPanel.Controls.Add(line_label) ' add to backpanel for transparency
        Next i

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '' Queue Data Cells (Each Line will show data for each queue)

        Dim cell_label_y As Integer = cell_start_y

        For line As Integer = 0 To (labels_result.Count - 1)
            'Dim cell_label_x As Integer = data_column_start_x + line_label_leftside_x_padding
            Dim cell_label_x As Integer = data_column_start_x

            For column As Integer = 0 To (queues_result.Count - 1)
                Dim queue_name As String = queues_result.ElementAt(column).Item("queue_name")
                Dim field_name As String = labels_result.ElementAt(line).Item("field_name")

                Dim cell_label = New Label
                cell_label.Name = line & "_" & column & "_CellLabel"
                cell_label.Font = New Font("Microsoft Sans Serif", cell_font_size)
                cell_label.Size = New Size(queue_label_width, line_label_height)

                'cell_label.Location = New Point(cell_label_x + cell_left_padding, cell_top_padding)
                'cell_label.Location = New Point(cell_label_x, cell_top_padding)
                cell_label.Location = New Point(cell_label_x, 0)
                cell_label.Text = "-"
                cell_label.TextAlign = ContentAlignment.MiddleLeft

                cell_label.BackColor = Color.Transparent
                'cell_label.Padding = New Padding(cell_left_padding, 0, 0, 0)
                cell_label_x += queue_label_width

                If (Debug_Borders) Then
                    cell_label.BorderStyle = BorderStyle.FixedSingle
                    'cell_label.ForeColor = Color.BlueViolet
                    'cell_label.BackColor = Color.DimGray
                End If

                QueueDataBoxes.Item(queue_name).Add(field_name, cell_label)
                LineLabels.Item(line).Controls.Add(cell_label)
            Next column

            cell_label_y += line_label_height
        Next line

        ' trying to get backpanel visual size
        'Dim p As TextBox = New TextBox()
        'p.Size = New Size(BackPanel.Width, BackPanel.Height)
        'p.Location = New Point(0, 0)
        'p.Show()
        'p.BringToFront()
        'BackPanel.Controls.Add(p)

        ''''''''''''''''''''''''''''''''
        ' User defined notification text
        Dim textbox_location_y = MainFormStartY + BackPanel.Size.Height + 10
        Dim textbox_left_right_padding As Integer = 5

        cmpNotificationTextBox.Location = New Point(MainFormStartX + textbox_left_right_padding, textbox_location_y)
        cmpNotificationTextBox.Size = New Size(MainFormWorkingWidth - (textbox_left_right_padding * 2), MainFormWorkingHeight - BackPanel.Size.Height - 20)
        cmpNotificationTextBox.BackColor = Me.BackColor
        cmpNotificationTextBox.Text = GetSetting("IntellaWallBoard", "Config", "NotifyMessage")
    End Sub

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    '' File Util
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    Function GetImageFromWebServer(ByVal image As String)
        Return GetResourceFromWebServer("image", image)
    End Function

    Function GetSoundFromWebServer(ByVal sound As String)
        Return GetResourceFromWebServer("audio", sound)
    End Function

    Function GetResourceFromWebServer(ByVal resourceType As String, ByVal resourceName As String)
        Dim server_address As String = GetSetting("IntellaWallBoard", "Config", "DB_Host")
        Dim server_path As String = server_address & "/pbx/getFile.fcgi?type=" & resourceType & "&file=" & resourceName

        Dim exception_message As String = ""

        ' First try HTTPS
        Dim url As String = "http://" & server_path

        ' Trust all certs... HACK FOR KWI... Doesn't trust LetsEncrypt
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (Function(sender, certificate, chain, sslPolicyErrors) True)

        ' Fix SSL
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3

        Try
            Return _GetResourceFromWebServer(resourceType, url)
        Catch e As Exception
            url = "http://" & server_path

            exception_message = Environment.NewLine & Environment.NewLine & "Tried HTTPS: " & e.Message

            Try
                Return _GetResourceFromWebServer(resourceType, url)
            Catch e2 As Exception

                If (resourceType = "image") Then
                    exception_message &= Environment.NewLine & Environment.NewLine & "Tried HTTP: " & e2.Message

                    MsgBox("Failed loading " & resourceType & " from server: " & resourceName & " " & exception_message)
                End If
            End Try
        End Try

        Return ""
    End Function

    Function _GetResourceFromWebServer(ByVal resourceType As String, ByVal url As String) As Object
        If (resourceType = "audio") Then
            Return Utils.GetSoundFromWebServer(url)
        ElseIf (resourceType = "image") Then
            Return Utils.GetImageFromWebServer(url)
        End If

        Throw New Exception("_GetResourceFromWebServer() Unknown resource type")
    End Function

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    '' GUI Rebuild
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    Sub DestroyWallboardAgentWidgets()
        qd(String.Format("Destroy Wallboard Widgets"))

        If (Not (BackPanel Is Nothing)) Then
            BackPanel.Dispose()
        End If
    End Sub

    Sub DestroyWallboardGridWidgets()
        qd(String.Format("Destroy Wallboard Widgets"))

        ' destroy queue labels
        If (Not (QueueLabels Is Nothing)) Then
            For i As Integer = 0 To (QueueLabels.Count - 1)
                QueueLabels.Item(i).Dispose()
            Next
        End If

        ' destroy line labels
        If (Not (LineLabels Is Nothing)) Then
            For i As Integer = 0 To (LineLabels.Count - 1)
                LineLabels.Item(i).Dispose()
            Next
        End If

        ' destroy data cells
        If (Not (QueueDataBoxes Is Nothing)) Then
            For Each databox_queue_item As KeyValuePair(Of String, Dictionary(Of String, Label)) In QueueDataBoxes
                Dim databox_queue_fields As Dictionary(Of String, Label) = databox_queue_item.Value

                For Each databox_item As KeyValuePair(Of String, Label) In databox_queue_fields
                    Dim databox_item_label As Label = databox_item.Value()
                    databox_item_label.Dispose()
                Next
            Next

            QueueDataBoxes.Clear()
        End If

        If (Not (BackPanel Is Nothing)) Then
            BackPanel.Dispose()
        End If
    End Sub

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    '' GUI Close
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    Private Sub wallBd_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        ' m_db.disconnect()
        SaveSetting("IntellaWallBoard", "Config", "NotifyMessage", cmpNotificationTextBox.Text)
    End Sub


    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    '' GUI Bindings
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    Private Sub XbuttonOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles XbuttonOn.Click
        SaveSetting("IntellaWallBoard", "Config", "NotifyMessage", cmpNotificationTextBox.Text)
        End
    End Sub

    Private Sub XbuttonOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles XbuttonOff.Click
        SaveSetting("IntellaWallBoard", "Config", "NotifyMessage", cmpNotificationTextBox.Text)
        End
    End Sub

    Private Sub XbuttonOff_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles XbuttonOff.MouseHover
        XbuttonOn.Visible = True
        XbuttonOff.Visible = False
    End Sub

    Private Sub XbuttonOn_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles XbuttonOn.MouseLeave
        XbuttonOff.Visible = True
        XbuttonOn.Visible = False
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ConfigButton.Click
        StatusLogAppend("Wallboard updates have been paused")
        Timer1.Stop()
        selectQueue.Visible = True
    End Sub

    Private Sub Button1_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        longest_waiting -= 1
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        longest_waiting += 1
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        calls_in_queue -= 1
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        calls_in_queue += 1
    End Sub

    Private Sub cmpNotificationTextBox_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmpNotificationTextBox.TextChanged
        Dim t As TextBox = sender
        Dim max_lines = 2

        'If (t.Lines.Length > MAX_LINES) Then
        '    Dim temp As string[] = new string[max_lines]
        '    For i As Integer = 0 To max_lines

        '        temp.Item(i) = t.Lines[i];
        '    Next i

        '    t.Lines. = temp
        'End If
    End Sub

    Private Sub cmpNotificationTextBox_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles cmpNotificationTextBox.KeyDown
        Dim t As TextBox = sender

        If (t.Lines.Length >= 2 And e.KeyValue.ToString = Chr(13).ToString) Then
            e.Handled = True
        End If

    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles cmpRebuildButton.Click
        RestartWallboard()
    End Sub
End Class
