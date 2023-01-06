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
        public delegate void ApplicationExceptionHandler(Exception ex);
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Boolean CatchUnhandledExceptions = true;
        public static  Boolean HandlingException = false;

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

            // TODO: Start logging from here!

            // IntellaQueueForm.TryOpenAndRotateLog("c:\temp\foo.log");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
  
            IntellaQueueForm main_form = new IntellaQueueForm();

            if (!Debugger.IsAttached) {
                Application.Run(main_form);
                return;
            }

            if (CatchUnhandledExceptions) {
                Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e) {
                    if (Program.HandlingException) {
                        return;
                    }

                    Program.HandlingException = true;

                    Exception ex = e.Exception;

                    main_form.ApplicationExceptionHandler(ex);

                    DataDumperForm DDF = new DataDumperForm(true);

                    DDF.SetTitle("Fatal Exception has occurred");
                    DDF.D("Uncaught Exception: " + ex.ToString());
                    DDF.D("Exiting after log upload...");

                    Timer exit_timer = new Timer();
                    exit_timer.Interval = 30000;
                    exit_timer.Tick += delegate(object tsender, EventArgs te) {
                        main_form.ApplicationExit_GuiCleanup();

                        Application.Exit();
                    };
                    exit_timer.Start();

                    Application.Run(DDF);
                };

                Application.Run(main_form);
            }
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e) {
            // this was totally unhandled 
   
            MessageBox.Show(ParseException(e.Exception));
        }
	}
}
