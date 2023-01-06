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
        public delegate void LogFileUploadCompleteCallback(List<JsonHashResult> upload_results);

        // Main QuickDebug (With logging)
        public static string MQD(string msg, params string[] msgFormat) {
            string final_log_line = MQD_NL(1, msg, msgFormat);

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

        /// <summary>
        /// Main QuickDebug (Write to the debug window, without writing to the log file)
        /// </summary>
        /// <param name="stackFramesGoBack">How many stack frames to go back to pull our caller fn</param>
        /// <param name="msg"></param>
        /// <param name="msgFormat"></param>
        /// <returns></returns>
        public static string MQD_NL(int stackFramesGoBack, string msg, params string[] msgFormat) {
            string log_prefix = QD.GenerateLogLine_WithoutTimestamp(stackFramesGoBack);
            string log_line;

            if (msgFormat.Length == 0) { msgFormat = null; }

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
                MQD_NL(1, "Exception while opening log file: {0}.  Exception: {1}", log_file, e.StackTrace.ToString());
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

        /// <summary>
        /// Upload the current backlog in a background thread.
        /// Note: Exceptions are caught and logged internally
        /// </summary>
        private void TryAndUploadCurrentLog(LogFileUploadCompleteCallback completedCallback = null) {
            try {
                TryAndUploadCurrentLog_Do_InThread(completedCallback);
            }
            catch (Exception ex) {
                MQD("Exception while running: TryAndUploadCurrentLog_Do: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// [Helper] Upload the current backlog in a background thread.
        /// Note: Exceptions are caught and logged internally
        /// </summary>
        private void TryAndUploadCurrentLog_Do_InThread(LogFileUploadCompleteCallback completedCallback = null) {
            string current_log_text = m_MainDF.GetLatestBacklogText();

            if (current_log_text.Length == 0) {
                MQD("Uploading log text -- Skipped (No new log lines to upload)");
                return;
            }

            Task t = Task.Factory.StartNew(() => {
                TryAndUploadCurrentLog_Do(current_log_text, completedCallback);
            });
        }

        /// <summary>
        /// [Worker] Upload the current backlog in the current thread (blocking)
        /// Note: Exceptions are caught and logged internally
        /// </summary>
        private void TryAndUploadCurrentLog_Do(string currentLogText, LogFileUploadCompleteCallback completedCallback = null) {
            int attempts = this.m_UploadLogRetries;

            List<JsonHashResult> upload_results = new List<JsonHashResult>();

            while (attempts-- > 0) {
                JsonHashResult upload_result = this.m_iqc.UploadLogText("current.log", currentLogText);
                upload_results.Add(upload_result);

                if (upload_result.Success) {
                    MQD("Uploading log text -- Complete");
                    goto done;
                }

                MQD("Uploading log text failed: {0} -- {1}", upload_result.Code, upload_result.Reason);

                System.Threading.Thread.Sleep(5000);
            }

            MQD("Uploading log text abort.  Too many failures.");

           done:
            if (completedCallback != null) {
                completedCallback.Invoke(upload_results);
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
