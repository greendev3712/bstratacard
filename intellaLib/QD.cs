using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Lib
{
    /// <summary>
    /// QuickDebug
    /// </summary>
    public class QD
    {
        public delegate string QE_LoggerFunction(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] msgFormat);        // QuickError -- Non-fatal error
        public delegate string QE_ErrorCallbackFunction(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] msgFormat); // QuickError -- Fatal Error

        public delegate string QD_LoggerFunction(string msg, params string[] msgFormat); // QuickDebug

        public delegate string LoggerCallbackObj(string desc, object thing);
        public delegate string LoggerCallbackMsg(string msg, params string[] argsRest);

        public enum LOG_LEVEL
        {
            INFO,
            DEBUG,
            TRACE,
        };

        public enum ERROR_LEVEL
        {
            NOTICE,
            WARNING,
            ERROR,
            FATAL,
        };

        public static string GenericErrorCallbackFunction(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] msgFormat) {
            string log_line = String.Format(String.Format("{0} {1} {2}", errorLevel, errorToken, msg), msgFormat);

            MessageBox.Show(log_line);

            if (errorLevel == QD.ERROR_LEVEL.FATAL) {
                Application.Exit();
            }

            return p(log_line);
        }

        public static string GenerateLogLine() {
            StackTrace s = new StackTrace(true);
            StackFrame[] stackFrames = s.GetFrames();

            DateTime time_stamp = DateTime.Now;
            string time_stamp_str = time_stamp.ToString();

            StackFrame f = s.GetFrame(2); // Frame(1) is going to be the func that called GenerateLogLine... we want the one before that\
            string filename = StackTrace_FilterFileName(f.GetFileName());

            return (String.Format("[{0}] {1}:{2} {3}:{4}()", time_stamp_str, filename, f.GetFileLineNumber().ToString(), f.GetMethod().Module.ToString(), f.GetMethod().Name));
        }

        public static string GenerateLogLine_WithoutTimestamp(int stackFramesGoBack = 0) {
            StackTrace s             = new StackTrace(true);
            StackFrame[] stackFrames = s.GetFrames();

            // By default go back to our caller, which will be 
            StackFrame f = s.GetFrame(2 + stackFramesGoBack); // Frame(1) is going to be the func that called GenerateLogLine... we want the one before that

            string caller_additional = "";

            if (f.GetFileLineNumber() == 0) {
                // Anonymous sub... also stick on the next frame
                StackFrame f2 = s.GetFrame(3 + stackFramesGoBack);

                if (f2 != null) {
                    // Example: C:\intellaApps\intellaQueue\src\intellaQueue\IntellaQueueForm.Data.cs
                    string filename_f2 = StackTrace_FilterFileName(f2.GetFileName());

                    caller_additional = String.Format("{0}:{1} {2}:{3}() -> ", filename_f2, f2.GetFileLineNumber().ToString(), f2.GetMethod().Module.ToString(), f2.GetMethod().Name);
                }
            }

            string filename = StackTrace_FilterFileName(f.GetFileName());
            
            return (String.Format("{0}{1}:{2} {3}:{4}()", caller_additional, filename, f.GetFileLineNumber().ToString(), f.GetMethod().Module.ToString(), f.GetMethod().Name));
        }

        /// <summary>
        /// Strip out contextually irrelavent path prefix from a StackTrace filename
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string StackTrace_FilterFileName(string fileName) {
            // Example: C:\intellaApps\intellaQueue\src\intellaQueue\IntellaQueueForm.Data.cs

            Regex regex   = new Regex(@".*src\\(.*)$");
            Match matches = regex.Match(fileName);

            if (matches.Success) {
                fileName = matches.Groups[1].Value;
            }

            return fileName;
        }

        public static string p (string msg) {
            StackTrace s             = new StackTrace();
            StackFrame[] stackFrames = s.GetFrames();

            string time_stamp_str = Utils.DateTimePrettyString(DateTime.Now);

            StackFrame f = s.GetFrame(1);

            string log_line = String.Format("[{0}] {1}:{2} {3}:{4}() {5}", time_stamp_str, f.GetFileName(), f.GetFileLineNumber().ToString(), f.GetMethod().Module.ToString(), f.GetMethod().Name, msg);
            Console.WriteLine(log_line);
            return log_line;
        }

        public static string p(string msg, int stackFramesGoBack) {
            StackTrace s = new StackTrace();
            StackFrame[] stackFrames = s.GetFrames();

            string time_stamp_str =  Utils.DateTimePrettyString(DateTime.Now);

            StackFrame f = s.GetFrame(2 + stackFramesGoBack);

            string log_line = String.Format("[{0}] {1}:{2} {3}:{4}() {}", time_stamp_str, f.GetFileName(), f.GetFileLineNumber().ToString(), f.GetMethod().Module.ToString(), f.GetMethod().Name, msg);
            Console.WriteLine(log_line);
            return log_line;
        }

    }
}
