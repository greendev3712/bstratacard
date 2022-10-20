<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class wallBd
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(wallBd))
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.StatusUpdatingGreenPictureBox = New System.Windows.Forms.PictureBox()
        Me.StatusUpdatingGrayPictureBox = New System.Windows.Forms.PictureBox()
        Me.StatusUpdatingStoppedPictureBox = New System.Windows.Forms.PictureBox()
        Me.XbuttonOn = New System.Windows.Forms.PictureBox()
        Me.lblTime = New System.Windows.Forms.Label()
        Me.lblDate = New System.Windows.Forms.Label()
        Me.XbuttonOff = New System.Windows.Forms.PictureBox()
        Me.ConfigButton = New System.Windows.Forms.Button()
        Me.genericLineLabel = New System.Windows.Forms.Label()
        Me.q4Text1 = New System.Windows.Forms.Label()
        Me.q3Text1 = New System.Windows.Forms.Label()
        Me.q2Text1 = New System.Windows.Forms.Label()
        Me.q1Text1 = New System.Windows.Forms.Label()
        Me.LoadingLabel = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.cmpNotificationTextBox = New System.Windows.Forms.TextBox()
        Me.cmpRebuildButton = New System.Windows.Forms.Button()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TextBox2 = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cmpLineLabelHeightTextBox = New System.Windows.Forms.TextBox()
        Me.cmpRebuildWallboardPanel = New System.Windows.Forms.Panel()
        CType(Me.StatusUpdatingGreenPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.StatusUpdatingGrayPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.StatusUpdatingStoppedPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.XbuttonOn, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.XbuttonOff, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.cmpRebuildWallboardPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'Timer1
        '
        Me.Timer1.Interval = 2000
        '
        'StatusUpdatingGreenPictureBox
        '
        Me.StatusUpdatingGreenPictureBox.BackColor = System.Drawing.Color.Transparent
        Me.StatusUpdatingGreenPictureBox.BackgroundImage = CType(resources.GetObject("StatusUpdatingGreenPictureBox.BackgroundImage"), System.Drawing.Image)
        Me.StatusUpdatingGreenPictureBox.Location = New System.Drawing.Point(5, 15)
        Me.StatusUpdatingGreenPictureBox.Name = "StatusUpdatingGreenPictureBox"
        Me.StatusUpdatingGreenPictureBox.Size = New System.Drawing.Size(22, 12)
        Me.StatusUpdatingGreenPictureBox.TabIndex = 75
        Me.StatusUpdatingGreenPictureBox.TabStop = False
        Me.StatusUpdatingGreenPictureBox.Visible = False
        '
        'StatusUpdatingGrayPictureBox
        '
        Me.StatusUpdatingGrayPictureBox.BackColor = System.Drawing.Color.Transparent
        Me.StatusUpdatingGrayPictureBox.BackgroundImage = CType(resources.GetObject("StatusUpdatingGrayPictureBox.BackgroundImage"), System.Drawing.Image)
        Me.StatusUpdatingGrayPictureBox.Location = New System.Drawing.Point(5, 15)
        Me.StatusUpdatingGrayPictureBox.Name = "StatusUpdatingGrayPictureBox"
        Me.StatusUpdatingGrayPictureBox.Size = New System.Drawing.Size(22, 12)
        Me.StatusUpdatingGrayPictureBox.TabIndex = 76
        Me.StatusUpdatingGrayPictureBox.TabStop = False
        Me.StatusUpdatingGrayPictureBox.Visible = False
        '
        'StatusUpdatingStoppedPictureBox
        '
        Me.StatusUpdatingStoppedPictureBox.BackColor = System.Drawing.Color.Transparent
        Me.StatusUpdatingStoppedPictureBox.BackgroundImage = CType(resources.GetObject("StatusUpdatingStoppedPictureBox.BackgroundImage"), System.Drawing.Image)
        Me.StatusUpdatingStoppedPictureBox.Location = New System.Drawing.Point(38, 13)
        Me.StatusUpdatingStoppedPictureBox.Name = "StatusUpdatingStoppedPictureBox"
        Me.StatusUpdatingStoppedPictureBox.Size = New System.Drawing.Size(22, 12)
        Me.StatusUpdatingStoppedPictureBox.TabIndex = 77
        Me.StatusUpdatingStoppedPictureBox.TabStop = False
        Me.StatusUpdatingStoppedPictureBox.Visible = False
        '
        'XbuttonOn
        '
        Me.XbuttonOn.BackColor = System.Drawing.Color.Transparent
        Me.XbuttonOn.BackgroundImage = CType(resources.GetObject("XbuttonOn.BackgroundImage"), System.Drawing.Image)
        Me.XbuttonOn.Location = New System.Drawing.Point(1248, 8)
        Me.XbuttonOn.Name = "XbuttonOn"
        Me.XbuttonOn.Size = New System.Drawing.Size(16, 16)
        Me.XbuttonOn.TabIndex = 78
        Me.XbuttonOn.TabStop = False
        Me.XbuttonOn.Visible = False
        '
        'lblTime
        '
        Me.lblTime.BackColor = System.Drawing.Color.Transparent
        Me.lblTime.Font = New System.Drawing.Font("Microsoft Sans Serif", 24.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTime.ForeColor = System.Drawing.Color.DimGray
        Me.lblTime.Location = New System.Drawing.Point(933, 89)
        Me.lblTime.Name = "lblTime"
        Me.lblTime.Size = New System.Drawing.Size(268, 37)
        Me.lblTime.TabIndex = 80
        Me.lblTime.Text = "-"
        Me.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblDate
        '
        Me.lblDate.BackColor = System.Drawing.Color.Transparent
        Me.lblDate.Font = New System.Drawing.Font("Microsoft Sans Serif", 24.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDate.ForeColor = System.Drawing.Color.DimGray
        Me.lblDate.Location = New System.Drawing.Point(931, 43)
        Me.lblDate.Name = "lblDate"
        Me.lblDate.Size = New System.Drawing.Size(270, 37)
        Me.lblDate.TabIndex = 81
        Me.lblDate.Text = "-"
        Me.lblDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'XbuttonOff
        '
        Me.XbuttonOff.BackColor = System.Drawing.Color.Transparent
        Me.XbuttonOff.BackgroundImage = CType(resources.GetObject("XbuttonOff.BackgroundImage"), System.Drawing.Image)
        Me.XbuttonOff.Location = New System.Drawing.Point(1248, 8)
        Me.XbuttonOff.Name = "XbuttonOff"
        Me.XbuttonOff.Size = New System.Drawing.Size(16, 16)
        Me.XbuttonOff.TabIndex = 82
        Me.XbuttonOff.TabStop = False
        '
        'ConfigButton
        '
        Me.ConfigButton.Location = New System.Drawing.Point(1230, 8)
        Me.ConfigButton.Name = "ConfigButton"
        Me.ConfigButton.Size = New System.Drawing.Size(18, 18)
        Me.ConfigButton.TabIndex = 85
        Me.ConfigButton.Text = "-"
        Me.ConfigButton.UseVisualStyleBackColor = True
        '
        'genericLineLabel
        '
        Me.genericLineLabel.BackColor = System.Drawing.Color.Transparent
        Me.genericLineLabel.Font = New System.Drawing.Font("Microsoft Sans Serif", 30.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, CType(0, Byte))
        Me.genericLineLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.genericLineLabel.Image = CType(resources.GetObject("genericLineLabel.Image"), System.Drawing.Image)
        Me.genericLineLabel.Location = New System.Drawing.Point(33, 183)
        Me.genericLineLabel.Name = "genericLineLabel"
        Me.genericLineLabel.Size = New System.Drawing.Size(1175, 55)
        Me.genericLineLabel.TabIndex = 2
        Me.genericLineLabel.Text = "     -"
        Me.genericLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.genericLineLabel.Visible = False
        '
        'q4Text1
        '
        Me.q4Text1.BackColor = System.Drawing.Color.Transparent
        Me.q4Text1.Font = New System.Drawing.Font("Microsoft Sans Serif", 24.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.q4Text1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.q4Text1.Location = New System.Drawing.Point(993, 185)
        Me.q4Text1.Name = "q4Text1"
        Me.q4Text1.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.q4Text1.Size = New System.Drawing.Size(220, 47)
        Me.q4Text1.TabIndex = 92
        Me.q4Text1.Text = "-"
        Me.q4Text1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.q4Text1.Visible = False
        '
        'q3Text1
        '
        Me.q3Text1.BackColor = System.Drawing.Color.Transparent
        Me.q3Text1.Font = New System.Drawing.Font("Microsoft Sans Serif", 24.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.q3Text1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.q3Text1.Location = New System.Drawing.Point(754, 185)
        Me.q3Text1.Name = "q3Text1"
        Me.q3Text1.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.q3Text1.Size = New System.Drawing.Size(239, 47)
        Me.q3Text1.TabIndex = 91
        Me.q3Text1.Text = "-"
        Me.q3Text1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.q3Text1.Visible = False
        '
        'q2Text1
        '
        Me.q2Text1.BackColor = System.Drawing.Color.Transparent
        Me.q2Text1.Font = New System.Drawing.Font("Microsoft Sans Serif", 24.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.q2Text1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.q2Text1.Location = New System.Drawing.Point(515, 185)
        Me.q2Text1.Name = "q2Text1"
        Me.q2Text1.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.q2Text1.Size = New System.Drawing.Size(239, 47)
        Me.q2Text1.TabIndex = 90
        Me.q2Text1.Text = "-"
        Me.q2Text1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.q2Text1.Visible = False
        '
        'q1Text1
        '
        Me.q1Text1.BackColor = System.Drawing.Color.Transparent
        Me.q1Text1.Font = New System.Drawing.Font("Microsoft Sans Serif", 24.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.q1Text1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.q1Text1.Location = New System.Drawing.Point(287, 185)
        Me.q1Text1.Name = "q1Text1"
        Me.q1Text1.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.q1Text1.Size = New System.Drawing.Size(228, 47)
        Me.q1Text1.TabIndex = 9
        Me.q1Text1.Text = "-"
        Me.q1Text1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.q1Text1.Visible = False
        '
        'LoadingLabel
        '
        Me.LoadingLabel.AutoSize = True
        Me.LoadingLabel.Font = New System.Drawing.Font("Microsoft Sans Serif", 48.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LoadingLabel.Location = New System.Drawing.Point(485, 185)
        Me.LoadingLabel.Name = "LoadingLabel"
        Me.LoadingLabel.Size = New System.Drawing.Size(316, 73)
        Me.LoadingLabel.TabIndex = 93
        Me.LoadingLabel.Text = "Loading..."
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(455, 58)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 94
        Me.Button1.Text = "-wait"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(455, 87)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(75, 23)
        Me.Button2.TabIndex = 95
        Me.Button2.Text = "+wait"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(573, 87)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(75, 23)
        Me.Button3.TabIndex = 97
        Me.Button3.Text = "+calls"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(573, 58)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(75, 23)
        Me.Button4.TabIndex = 96
        Me.Button4.Text = "-calls"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'cmpNotificationTextBox
        '
        Me.cmpNotificationTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.cmpNotificationTextBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmpNotificationTextBox.ForeColor = System.Drawing.Color.Red
        Me.cmpNotificationTextBox.Location = New System.Drawing.Point(25, 638)
        Me.cmpNotificationTextBox.Multiline = True
        Me.cmpNotificationTextBox.Name = "cmpNotificationTextBox"
        Me.cmpNotificationTextBox.Size = New System.Drawing.Size(1239, 70)
        Me.cmpNotificationTextBox.TabIndex = 98
        '
        'cmpRebuildButton
        '
        Me.cmpRebuildButton.Location = New System.Drawing.Point(3, 3)
        Me.cmpRebuildButton.Name = "cmpRebuildButton"
        Me.cmpRebuildButton.Size = New System.Drawing.Size(137, 23)
        Me.cmpRebuildButton.TabIndex = 99
        Me.cmpRebuildButton.Text = "Rebuild Wallboard"
        Me.cmpRebuildButton.UseVisualStyleBackColor = True
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(105, 36)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(137, 20)
        Me.TextBox1.TabIndex = 100
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(35, 39)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(64, 13)
        Me.Label1.TabIndex = 101
        Me.Label1.Text = "Label Width"
        '
        'TextBox2
        '
        Me.TextBox2.Location = New System.Drawing.Point(105, 62)
        Me.TextBox2.Name = "TextBox2"
        Me.TextBox2.Size = New System.Drawing.Size(137, 20)
        Me.TextBox2.TabIndex = 102
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(35, 65)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(70, 13)
        Me.Label2.TabIndex = 103
        Me.Label2.Text = "Queue Width"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(13, 89)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(86, 13)
        Me.Label3.TabIndex = 105
        Me.Label3.Text = "line_label_height"
        '
        'cmpLineLabelHeightTextBox
        '
        Me.cmpLineLabelHeightTextBox.Location = New System.Drawing.Point(105, 86)
        Me.cmpLineLabelHeightTextBox.Name = "cmpLineLabelHeightTextBox"
        Me.cmpLineLabelHeightTextBox.Size = New System.Drawing.Size(137, 20)
        Me.cmpLineLabelHeightTextBox.TabIndex = 104
        '
        'cmpRebuildWallboardPanel
        '
        Me.cmpRebuildWallboardPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.cmpRebuildWallboardPanel.Controls.Add(Me.cmpRebuildButton)
        Me.cmpRebuildWallboardPanel.Controls.Add(Me.Label3)
        Me.cmpRebuildWallboardPanel.Controls.Add(Me.cmpLineLabelHeightTextBox)
        Me.cmpRebuildWallboardPanel.Controls.Add(Me.TextBox1)
        Me.cmpRebuildWallboardPanel.Controls.Add(Me.Label2)
        Me.cmpRebuildWallboardPanel.Controls.Add(Me.Label1)
        Me.cmpRebuildWallboardPanel.Controls.Add(Me.TextBox2)
        Me.cmpRebuildWallboardPanel.Location = New System.Drawing.Point(654, 8)
        Me.cmpRebuildWallboardPanel.Name = "cmpRebuildWallboardPanel"
        Me.cmpRebuildWallboardPanel.Size = New System.Drawing.Size(299, 118)
        Me.cmpRebuildWallboardPanel.TabIndex = 106
        Me.cmpRebuildWallboardPanel.Visible = False
        '
        'wallBd
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(1280, 720)
        Me.ControlBox = False
        Me.Controls.Add(Me.cmpRebuildWallboardPanel)
        Me.Controls.Add(Me.cmpNotificationTextBox)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.LoadingLabel)
        Me.Controls.Add(Me.q2Text1)
        Me.Controls.Add(Me.q4Text1)
        Me.Controls.Add(Me.q3Text1)
        Me.Controls.Add(Me.q1Text1)
        Me.Controls.Add(Me.ConfigButton)
        Me.Controls.Add(Me.XbuttonOn)
        Me.Controls.Add(Me.XbuttonOff)
        Me.Controls.Add(Me.lblDate)
        Me.Controls.Add(Me.lblTime)
        Me.Controls.Add(Me.StatusUpdatingGrayPictureBox)
        Me.Controls.Add(Me.StatusUpdatingStoppedPictureBox)
        Me.Controls.Add(Me.StatusUpdatingGreenPictureBox)
        Me.Controls.Add(Me.genericLineLabel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "wallBd"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        CType(Me.StatusUpdatingGreenPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.StatusUpdatingGrayPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.StatusUpdatingStoppedPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.XbuttonOn, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.XbuttonOff, System.ComponentModel.ISupportInitialize).EndInit()
        Me.cmpRebuildWallboardPanel.ResumeLayout(False)
        Me.cmpRebuildWallboardPanel.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents StatusUpdatingGreenPictureBox As System.Windows.Forms.PictureBox
    Friend WithEvents StatusUpdatingGrayPictureBox As System.Windows.Forms.PictureBox
    Friend WithEvents StatusUpdatingStoppedPictureBox As System.Windows.Forms.PictureBox
    Friend WithEvents XbuttonOn As System.Windows.Forms.PictureBox
    Friend WithEvents lblTime As System.Windows.Forms.Label
    Friend WithEvents lblDate As System.Windows.Forms.Label
    Friend WithEvents XbuttonOff As System.Windows.Forms.PictureBox
    Friend WithEvents ConfigButton As System.Windows.Forms.Button
    Friend WithEvents genericLineLabel As System.Windows.Forms.Label
    Friend WithEvents q4Text1 As System.Windows.Forms.Label
    Friend WithEvents q3Text1 As System.Windows.Forms.Label
    Friend WithEvents q2Text1 As System.Windows.Forms.Label
    Friend WithEvents q1Text1 As System.Windows.Forms.Label
    Friend WithEvents LoadingLabel As System.Windows.Forms.Label
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Button3 As System.Windows.Forms.Button
    Friend WithEvents Button4 As System.Windows.Forms.Button
    Friend WithEvents cmpNotificationTextBox As System.Windows.Forms.TextBox
    Friend WithEvents cmpRebuildButton As System.Windows.Forms.Button
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents cmpLineLabelHeightTextBox As System.Windows.Forms.TextBox
    Friend WithEvents cmpRebuildWallboardPanel As System.Windows.Forms.Panel

End Class
