using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

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
            StackTrace s = new StackTrace();
            StackFrame[] stackFrames = s.GetFrames();

            DateTime time_stamp = DateTime.Now;
            string time_stamp_str = time_stamp.ToString();

            StackFrame f = s.GetFrame(2); // Frame(1) is going to be the func that called GenerateLogLine... we want the one before that
            return (String.Format("[{0}] {1}:{2} {3}:{4}()", time_stamp_str, f.GetFileName(), f.GetFileLineNumber().ToString(), f.GetMethod().Module.ToString(), f.GetMethod().Name));
        }

        public static string GenerateLogLine_WthoutTimestamp() {
            StackTrace s = new StackTrace();
            StackFrame[] stackFrames = s.GetFrames();

            StackFrame f = s.GetFrame(2); // Frame(1) is going to be the func that called GenerateLogLine... we want the one before that
            return (String.Format("{0}:{1} {2}:{3}()", f.GetFileName(), f.GetFileLineNumber().ToString(), f.GetMethod().Module.ToString(), f.GetMethod().Name));
        }

        public static string p (string msg) {
            StackTrace s = new StackTrace();
            StackFrame[] stackFrames = s.GetFrames();

            DateTime time_stamp = DateTime.Now;
            string time_stamp_str = time_stamp.ToString();

            StackFrame f = s.GetFrame(1);

            string log_line = String.Format("[{0}] {1}:{2} {3}:{4}() {5}", time_stamp_str, f.GetFileName(), f.GetFileLineNumber().ToString(), f.GetMethod().Module.ToString(), f.GetMethod().Name, msg);
            Console.WriteLine(log_line);
            return log_line;
        }

        public static string p(string msg, int stackFramesGoBack) {
            StackTrace s = new StackTrace();
            StackFrame[] stackFrames = s.GetFrames();

            DateTime time_stamp = DateTime.Now;
            string time_stamp_str = time_stamp.ToString();

            StackFrame f = s.GetFrame(2 + stackFramesGoBack);

            string log_line = String.Format("[{0}] {1}:{2} {3}:{4}() {}", time_stamp_str, f.GetFileName(), f.GetFileLineNumber().ToString(), f.GetMethod().Module.ToString(), f.GetMethod().Name, msg);
            Console.WriteLine(log_line);
            return log_line;
        }

    }
}
