namespace IntellaCast
{
    partial class IntellaCastTestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.bStartRecording = new System.Windows.Forms.Button();
            this.bStopRecording = new System.Windows.Forms.Button();
            this.automaticUpdater1 = new wyDay.Controls.AutomaticUpdater();
            ((System.ComponentModel.ISupportInitialize)(this.automaticUpdater1)).BeginInit();
            this.SuspendLayout();
            // 
            // bStartRecording
            // 
            this.bStartRecording.Location = new System.Drawing.Point(184, 195);
            this.bStartRecording.Name = "bStartRecording";
            this.bStartRecording.Size = new System.Drawing.Size(116, 55);
            this.bStartRecording.TabIndex = 0;
            this.bStartRecording.Text = "Start Recording";
            this.bStartRecording.UseVisualStyleBackColor = true;
            this.bStartRecording.Click += new System.EventHandler(this.bStartRecording_Click);
            // 
            // bStopRecording
            // 
            this.bStopRecording.Location = new System.Drawing.Point(342, 198);
            this.bStopRecording.Name = "bStopRecording";
            this.bStopRecording.Size = new System.Drawing.Size(116, 55);
            this.bStopRecording.TabIndex = 1;
            this.bStopRecording.Text = "Stop Recording";
            this.bStopRecording.UseVisualStyleBackColor = true;
            this.bStopRecording.Click += new System.EventHandler(this.bStopRecording_Click);
            // 
            // automaticUpdater1
            // 
            this.automaticUpdater1.ContainerForm = this;
            this.automaticUpdater1.DaysBetweenChecks = 1;
            this.automaticUpdater1.GUID = "a4bb3471-4ed0-4d02-973a-5d20360a7bf6";
            this.automaticUpdater1.Location = new System.Drawing.Point(184, 56);
            this.automaticUpdater1.Name = "automaticUpdater1";
            this.automaticUpdater1.Size = new System.Drawing.Size(16, 16);
            this.automaticUpdater1.TabIndex = 2;
            this.automaticUpdater1.UpdateType = wyDay.Controls.UpdateType.DoNothing;
            this.automaticUpdater1.wyUpdateCommandline = "/skipinfo";
            this.automaticUpdater1.CheckingFailed += new wyDay.Controls.FailHandler(this.automaticUpdater1_CheckingFailed);
            this.automaticUpdater1.UpdateAvailable += new System.EventHandler(this.automaticUpdater1_UpdateAvailable);
            this.automaticUpdater1.UpdateFailed += new wyDay.Controls.FailHandler(this.automaticUpdater1_UpdateFailed);
            this.automaticUpdater1.UpdateSuccessful += new wyDay.Controls.SuccessHandler(this.automaticUpdater1_UpdateSuccessful);
            this.automaticUpdater1.UpToDate += new wyDay.Controls.SuccessHandler(this.automaticUpdater1_UpToDate);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.automaticUpdater1);
            this.Controls.Add(this.bStopRecording);
            this.Controls.Add(this.bStartRecording);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.automaticUpdater1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bStartRecording;
        private System.Windows.Forms.Button bStopRecording;
        private wyDay.Controls.AutomaticUpdater automaticUpdater1;
    }
}

