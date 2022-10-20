using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using Lib;
using System.Deployment;

namespace QueueLib
{

	public class QueueVersion
	{
		public static string m_toolbarVersion = "2.1";
		// bump this up if any tables/views/columns/etc have been changed
		public static string m_toolbarProtocol = "21";

        public static string GetToolBarBuild() {
            return System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString("yyyy.MM.dd.HHmm");
        }

		// make sure we're running the exact same version that the server is expecting.
		public static void CheckVersion()
		{
            DbHelper m_db = ToolBarHelper.GetDbHelper();

			string server_protocol = m_db.DbSelectSingleValueString("SELECT toolbar_protocol_version_required FROM queue.toolbar_config");

			if (server_protocol != m_toolbarProtocol)
			{
				string msg = "This is the incorrect toolbar to run for the connected system." + Strings.Chr(10);
			  	       msg += "Toolbar Protocol: " + m_toolbarProtocol + Strings.Chr(10);
				       msg += "Server Requires: " + server_protocol;

				MessageBox.Show(msg, "Error");
                Application.Exit();
			}

		}
	}
}