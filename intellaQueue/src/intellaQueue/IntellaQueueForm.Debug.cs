using Lib;
using Microsoft.VisualBasic;
using QueueLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace intellaQueue
{
    public partial class IntellaQueueForm
    {
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

        private void TryAndUploadCurrentLog() {
            try {
                TryAndUploadCurrentLog_Do();
            }
            catch (Exception ex) {
                MQD("Exception while running: TryAndUploadCurrentLog_Do: {0}", ex.ToString());
            }
        }

        private void TryAndUploadCurrentLog_Do() {
            string current_log_text = m_MainDF.GetLatestBacklogText();

            if (current_log_text.Length != 0) {
                this.m_iqc.UploadLogText("current.log", current_log_text);
                MQD("Uploading log text -- Complete");
            }
            else {
                MQD("Uploading log text -- Skipped (No new log lines to upload)");
            }
        }

        private void TryAndUploadLogFile(string log_file) {
            try {
                UploadLog(log_file);
            }
            catch (Exception e) {
                MQD("Exception while uploading log file: {0}.  Exception: {1}", log_file, e.StackTrace.ToString());
                // if (Debugger.IsAttached) { throw; }
            }
        }

        public void UploadLog(string logFile) {
            if (!File.Exists(logFile)) {
                MQD("Log file does not exist: {0}", logFile);
                return;
            }

            string log_data = Utils.File_ReadTail(logFile, 1000);

            this.m_iqc.UploadLogFile(logFile);

            MQD("Uploaded log file: " + logFile);
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

    }
}
