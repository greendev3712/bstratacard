using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public class LogFileUploader
    {
        public delegate void LogFileUploadCallback();

        private System.Windows.Forms.Timer m_logFileUploaderTimer = new System.Windows.Forms.Timer();
        private LogFileUploadCallback m_logFileUploadCallback;

        public LogFileUploader(int timerInterval, LogFileUploadCallback logFileUploadCallback) {
            this.m_logFileUploadCallback = logFileUploadCallback;

            m_logFileUploaderTimer.Interval = (int) TimeSpan.FromSeconds(timerInterval).TotalMilliseconds;
            m_logFileUploaderTimer.Tick    += delegate (object sender, EventArgs e) {
               m_logFileUploaderTimer.Stop();
               m_logFileUploadCallback.Invoke();
               m_logFileUploaderTimer.Start();
            };

            m_logFileUploaderTimer.Start();
        }
    }
}
