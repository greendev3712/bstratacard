using System;
using System.Windows.Forms;
using log4net;
using System.Text;
using System.Diagnostics;
using Lib;

namespace intellaQueue
{
    public static class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Boolean CatchUnhandledExceptions = false;

        public static string ParseException(Exception e) {
            var indent = new String((char)32, 4);

            var sb = new StringBuilder();

            sb.AppendLine(DateTime.Now.ToString("dd MMMM yyyy, HH:mm:ss"));
            sb.AppendLine();
            sb.AppendLine("An unhandled exception has been caught on the main program thread.");
            sb.AppendLine();
            sb.AppendLine("Messages");
            //GetExceptionMessages(e, sb, 0, indent);
            sb.AppendLine();
            sb.AppendLine("Target Site");
            sb.AppendLine(indent + e.TargetSite.Name);
            sb.AppendLine();
            sb.AppendLine("Stack Trace");
            sb.AppendLine(indent + e.StackTrace);

            return sb.ToString();
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (Debugger.IsAttached) {
                doMain(args);
                return;
            }

            try {
                doMain(args);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        static void doMain(string[] args) {

            // So anywhere in the app we can access command line args!
            Utils.Globals.Add("ARGS", args);

            // IntellaQueueForm.TryOpenAndRotateLog("c:\temp\foo.log");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (CatchUnhandledExceptions && !Debugger.IsAttached)
            {
                Application.ThreadException += Application_ThreadException;
            }

            Application.Run(new IntellaQueueForm());

            /*
                        try
                        {
                            Application.Run(new IntellaQueueForm());
                        }
                        catch (Exception ex)
                        {
                            log.Error("Unhandled exception: exception: \n" + ex.Message + "\n trace:" + ex.StackTrace + "\n");
                            if (ex.InnerException != null)
                                log.Error("Unhandled exception: inner exception: \n" + ex.InnerException.Message + "\n trace:" + ex.InnerException.StackTrace + "\n");

                            MessageBox.Show(ex.StackTrace.ToString());
                        }
            */
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e) {
            //this was totally unhandled 
   
            MessageBox.Show(ParseException(e.Exception));
        }
	}
}
