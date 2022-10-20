using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;

namespace intellaConsole
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
        [STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

//            try {
               Application.Run(new intellaConsole());
//            }
//            catch (Exception ex) {
//                ErrorTextBox.Show(ex.ToString());
//                throw ex;
//            }

            /*
            try
            {
                Application.Run(new intellaQueue());
            }
            catch (TargetInvocationException ex)
            {
                log.Error("Threading error: Unhandled exception: target invocation exception: \n" + ex.Message + "\n trace:" + ex.StackTrace + "\n");
                if (ex.InnerException != null)
                    log.Error("Threading error: Unhandled exception: target invocation inner exception: \n" + ex.InnerException.Message + "\n trace:" + ex.InnerException.StackTrace + "\n");
            }
            catch (Exception ex)
            {
                log.Error("Threading error: Unhandled exception: exception: \n" + ex.Message + "\n trace:" + ex.StackTrace + "\n");
                if (ex.InnerException != null)
                    log.Error("Threading error: Unhandled exception: inner exception: \n" + ex.InnerException.Message + "\n trace:" + ex.InnerException.StackTrace + "\n");
            }
             */
		}
	}
}
