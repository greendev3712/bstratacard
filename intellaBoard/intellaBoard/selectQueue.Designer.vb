<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class selectQueue
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.wallboardToMonitorCombo = New System.Windows.Forms.ComboBox()
        Me.labelQueue1 = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.PlayAlertEveryText = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.StartAlertingAfterText = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.AlertOnNewCallInQueueCheckBox = New System.Windows.Forms.CheckBox()
        Me.DebugCheckBox = New System.Windows.Forms.CheckBox()
        Me.ColorDialog1 = New System.Windows.Forms.ColorDialog()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.BackgroundColorLabel = New System.Windows.Forms.Label()
        Me.cmpDatabaseSetupButton = New System.Windows.Forms.Button()
        Me.cmpCancelButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'wallboardToMonitorCombo
        '
        Me.wallboardToMonitorCombo.FormattingEnabled = True
        Me.wallboardToMonitorCombo.Location = New System.Drawing.Point(122, 54)
        Me.wallboardToMonitorCombo.Name = "wallboardToMonitorCombo"
        Me.wallboardToMonitorCombo.Size = New System.Drawing.Size(178, 21)
        Me.wallboardToMonitorCombo.TabIndex = 0
        '
        'labelQueue1
        '
        Me.labelQueue1.AutoSize = True
        Me.labelQueue1.Location = New System.Drawing.Point(12, 57)
        Me.labelQueue1.Name = "labelQueue1"
        Me.labelQueue1.Size = New System.Drawing.Size(104, 13)
        Me.labelQueue1.TabIndex = 1
        Me.labelQueue1.Text = "Wallboard to monitor"
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(233, 356)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(195, 23)
        Me.Button1.TabIndex = 36
        Me.Button1.Text = "Save Settings"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'PlayAlertEveryText
        '
        Me.PlayAlertEveryText.Location = New System.Drawing.Point(106, 173)
        Me.PlayAlertEveryText.Name = "PlayAlertEveryText"
        Me.PlayAlertEveryText.Size = New System.Drawing.Size(45, 20)
        Me.PlayAlertEveryText.TabIndex = 37
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(11, 105)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(242, 20)
        Me.Label1.TabIndex = 38
        Me.Label1.Text = "Longest Waiting Audio Alerts"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 174)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(81, 13)
        Me.Label2.TabIndex = 39
        Me.Label2.Text = "Play Alert Every"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(11, 19)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(73, 20)
        Me.Label3.TabIndex = 40
        Me.Label3.Text = "General"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 144)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(92, 13)
        Me.Label4.TabIndex = 42
        Me.Label4.Text = "Start Alerting After"
        '
        'StartAlertingAfterText
        '
        Me.StartAlertingAfterText.Location = New System.Drawing.Point(106, 140)
        Me.StartAlertingAfterText.Name = "StartAlertingAfterText"
        Me.StartAlertingAfterText.Size = New System.Drawing.Size(45, 20)
        Me.StartAlertingAfterText.TabIndex = 41
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(155, 144)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(47, 13)
        Me.Label5.TabIndex = 43
        Me.Label5.Text = "seconds"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(155, 177)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(47, 13)
        Me.Label6.TabIndex = 44
        Me.Label6.Text = "seconds"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(12, 260)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(137, 13)
        Me.Label8.TabIndex = 47
        Me.Label8.Text = "Alert On New Call In Queue"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.Location = New System.Drawing.Point(11, 221)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(272, 20)
        Me.Label9.TabIndex = 45
        Me.Label9.Text = "New Calls In Queue Visual Alerts"
        '
        'AlertOnNewCallInQueueCheckBox
        '
        Me.AlertOnNewCallInQueueCheckBox.AutoSize = True
        Me.AlertOnNewCallInQueueCheckBox.Location = New System.Drawing.Point(158, 260)
        Me.AlertOnNewCallInQueueCheckBox.Name = "AlertOnNewCallInQueueCheckBox"
        Me.AlertOnNewCallInQueueCheckBox.Size = New System.Drawing.Size(15, 14)
        Me.AlertOnNewCallInQueueCheckBox.TabIndex = 48
        Me.AlertOnNewCallInQueueCheckBox.UseVisualStyleBackColor = True
        '
        'DebugCheckBox
        '
        Me.DebugCheckBox.AutoSize = True
        Me.DebugCheckBox.Location = New System.Drawing.Point(318, 58)
        Me.DebugCheckBox.Name = "DebugCheckBox"
        Me.DebugCheckBox.Size = New System.Drawing.Size(58, 17)
        Me.DebugCheckBox.TabIndex = 49
        Me.DebugCheckBox.Text = "Debug"
        Me.DebugCheckBox.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(286, 144)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(142, 23)
        Me.Button2.TabIndex = 50
        Me.Button2.Text = "Background Color"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'BackgroundColorLabel
        '
        Me.BackgroundColorLabel.AutoSize = True
        Me.BackgroundColorLabel.Location = New System.Drawing.Point(434, 149)
        Me.BackgroundColorLabel.Name = "BackgroundColorLabel"
        Me.BackgroundColorLabel.Size = New System.Drawing.Size(55, 13)
        Me.BackgroundColorLabel.TabIndex = 51
        Me.BackgroundColorLabel.Text = "                "
        '
        'cmpDatabaseSetupButton
        '
        Me.cmpDatabaseSetupButton.Location = New System.Drawing.Point(122, 12)
        Me.cmpDatabaseSetupButton.Name = "cmpDatabaseSetupButton"
        Me.cmpDatabaseSetupButton.Size = New System.Drawing.Size(178, 23)
        Me.cmpDatabaseSetupButton.TabIndex = 52
        Me.cmpDatabaseSetupButton.Text = "Database Setup"
        Me.cmpDatabaseSetupButton.UseVisualStyleBackColor = True
        '
        'cmpCancelButton
        '
        Me.cmpCancelButton.Location = New System.Drawing.Point(15, 356)
        Me.cmpCancelButton.Name = "cmpCancelButton"
        Me.cmpCancelButton.Size = New System.Drawing.Size(195, 23)
        Me.cmpCancelButton.TabIndex = 53
        Me.cmpCancelButton.Text = "Cancel"
        Me.cmpCancelButton.UseVisualStyleBackColor = True
        '
        'selectQueue
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(440, 391)
        Me.ControlBox = False
        Me.Controls.Add(Me.cmpCancelButton)
        Me.Controls.Add(Me.cmpDatabaseSetupButton)
        Me.Controls.Add(Me.BackgroundColorLabel)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.DebugCheckBox)
        Me.Controls.Add(Me.AlertOnNewCallInQueueCheckBox)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.StartAlertingAfterText)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.PlayAlertEveryText)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.labelQueue1)
        Me.Controls.Add(Me.wallboardToMonitorCombo)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "selectQueue"
        Me.Text = "Queue Selection"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents wallboardToMonitorCombo As System.Windows.Forms.ComboBox
    Friend WithEvents labelQueue1 As System.Windows.Forms.Label
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents PlayAlertEveryText As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents StartAlertingAfterText As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents AlertOnNewCallInQueueCheckBox As System.Windows.Forms.CheckBox
    Friend WithEvents DebugCheckBox As System.Windows.Forms.CheckBox
    Friend WithEvents ColorDialog1 As System.Windows.Forms.ColorDialog
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents BackgroundColorLabel As System.Windows.Forms.Label
    Friend WithEvents cmpDatabaseSetupButton As System.Windows.Forms.Button
    Friend WithEvents cmpCancelButton As System.Windows.Forms.Button
End Class
