Imports System.Collections.Specialized
Imports [Lib]
Imports QueueLib

Public Class selectQueue
    Private Sub selectQueue_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        wallBd.RestartWallboard()
    End Sub

    Private Sub selectQueue_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        selectQueue_init()
    End Sub

    Private Sub selectQueue_init()
        Dim wallboard As String = GetSetting("intellaBoard", "queue", "Wallboard")

        Dim data_result As List(Of OrderedDictionary) = New List(Of OrderedDictionary)
        wallBd.m_db.dbQuery("SELECT * FROM queue.wallboards", data_result)

        wallboardToMonitorCombo.Items.Clear()
        wallboardToMonitorCombo.ResetText()

        For i As Integer = 0 To (data_result.Count - 1)
            Dim wallboard_item As String = data_result.Item(i).Item("wallboard_name")
            wallboardToMonitorCombo.Items.Add(wallboard_item)

            If (wallboard_item = wallboard) Then
                wallboardToMonitorCombo.SelectedIndex = i
            End If
        Next i

        Try
            StartAlertingAfterText.Text = wallBd.Interface_Config_LongestWaiting_AudioAlertIntervals.Keys.First
            PlayAlertEveryText.Text = wallBd.Interface_Config_LongestWaiting_SecondsBetweenAudioAlerts
            'AlertOnNewCallInQueueCheckBox.Checked = wallBd.Interface_Config_NewCallerVisualAlert ' TODO: need per-queue alert config
            DebugCheckBox.Checked = wallBd.Interface_Config_Debug
        Catch ex As Exception
            Console.WriteLine("Exception:" & ex.Message)
        End Try

        BackgroundColorLabel.BackColor = [Lib].Helper.webColorToColorObj(wallBd.Interface_Config_Board_BackgroundColor)

        'Dim Interface_Config_NewCallerVisualAlert As Boolean = False  ' populated by wallboard config in db
        'Dim Interface_Config_LongestWaiting_AudioAlert As Boolean = False ' populated by wallboard config in db
        'Dim Interface_Config_LongestWaiting_AudioAlertIntervals As Dictionary(Of Integer, String) 'Intervals[Interval][WavFile]  ' populated by wallboard config in db
        'Dim Interface_Config_LongestWaiting_SecondsBetweenAudioAlerts As Integer ' populated by wallboard config in db
        'Dim Interface_Config_Debug as Boolean = False ' populated by wallboard config in db
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim monitored_wallboard As String = wallboardToMonitorCombo.Text

        Dim wallboard_result As List(Of OrderedDictionary) = New List(Of OrderedDictionary)
        wallBd.m_db.dbQuery("SELECT * FROM queue.wallboards WHERE wallboard_name = '" & monitored_wallboard & "'", wallboard_result)

        If (wallboard_result.Count = 0) Then
            MsgBox("Wallboard selected does not exist")
            selectQueue_init()
            Exit Sub
        End If

        ' success

        SaveSetting("intellaBoard", "queue", "Wallboard", monitored_wallboard)
        Me.Hide()

        Dim sql As String = String.Format(
            "UPDATE queue.wallboards SET newcaller_visual_alert = '{0}', longest_waiting_audio_alert_repeat_seconds = '{1}', longest_waiting_audio_alert_seconds = '{2}', debug = '{3}' WHERE wallboard_name = '{4}'",
            AlertOnNewCallInQueueCheckBox.Checked.ToString,
            PlayAlertEveryText.Text,
            StartAlertingAfterText.Text,
            DebugCheckBox.Checked.ToString,
            monitored_wallboard)

        Dim update_result As List(Of OrderedDictionary) = New List(Of OrderedDictionary)
        wallBd.m_db.dbQuery(sql, update_result)
        wallBd.RestartWallboard()
    End Sub

    Private Sub wallboardToMonitorCombo_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles wallboardToMonitorCombo.SelectedIndexChanged
        ' Update the local config with what's in the db
        Dim wallboard_result As List(Of OrderedDictionary) = New List(Of OrderedDictionary)
        wallBd.m_db.dbQuery("SELECT * FROM queue.wallboards WHERE wallboard_name = '" & wallboardToMonitorCombo.Text & "'", wallboard_result)

        If (wallboard_result.Count = 0) Then
            MsgBox("Wallboard selected does not exist")
            selectQueue_init()
            Exit Sub
        End If

        AlertOnNewCallInQueueCheckBox.Checked = wallboard_result.Item(0).Item("newcaller_visual_alert")
        StartAlertingAfterText.Text = wallboard_result.Item(0).Item("longest_waiting_audio_alert_seconds")
        PlayAlertEveryText.Text = wallboard_result.Item(0).Item("longest_waiting_audio_alert_repeat_seconds")
        DebugCheckBox.Checked = wallboard_result.Item(0).Item("debug")
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Dim a = [Lib].Helper.webColorToColorObj("aabbcc").ToArgb
        ColorDialog1.CustomColors = New Integer() {Color.ForestGreen.ToArgb}
        ColorDialog1.Color = [Lib].Helper.webColorToColorObj("aabbcc")

        ColorDialog1.ShowDialog()
        BackgroundColorLabel.BackColor = ColorDialog1.Color()

        wallBd.Interface_Config_Board_BackgroundColor = [Lib].Helper.colorObjToWebColor(ColorDialog1.Color())
        wallBd.BackColor = ColorDialog1.Color()
    End Sub

    Private Sub databaseConfigSuccess()
        SaveSetting("intellaBoard", "queue", "Wallboard", "")
        selectQueue_init()
    End Sub

    Private Sub databaseConfigFailure()
        MsgBox("Connection Failed")
    End Sub

    Private Sub cmpDatabaseSetupButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmpDatabaseSetupButton.Click
        Dim db_settings_form As DatabaseSettingsForm = New DatabaseSettingsForm("IntellaWallboard")

        db_settings_form.SetSuccessCallback(AddressOf databaseConfigSuccess)
        db_settings_form.SetFailureCallback(AddressOf databaseConfigFailure)

        db_settings_form.Show()
    End Sub

    Private Sub cmpCancelButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmpCancelButton.Click
        Me.Hide()
        wallBd.RestartWallboard()
    End Sub
End Class