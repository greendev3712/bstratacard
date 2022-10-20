/*
Program.cs
Intellasoft Configurator.
 
@author Mike Jacobi for Intellasoft
*/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using log4net;
using Lib;

namespace IasPbxConfig
{
	/// All Form Name abreviations.
	public enum IASoftSetupForms
	{
		Cos,
		Cosi,
		Ext,
		Extd,
		Trk,
		Trkd,
		Rt,
		Pg,
		Tg,
		Tgi,
		Ri,
		Rs,
		Fc,
		Tv,
		End
	};
	/// All Error condition abreviations.
	public enum Errors { None, Connection };

	//////////////////////////////////////////////////////////////////////////////////
	/// launch point.
	/// 
	/// Holds global constant strings, db connection type, db connection string.
	/// Creates and sets up a db object and launches the main form.
	static class Program
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static string ProtocolVersion = "1";
		public static string Version = "1.0.0";
		public static string Revision = "UnknownRevision";
		public static string AppName =  "intellaSoft Configurator";
		public static string CopyRight = "©  intellaSoft, LLC 2010\nAll rights reserved.";

		/// Full form names. @see IASoftSetupForms
		public static string[] m_formNames = 
		{ 
			"Classes of Service", 
			"Classes of Service Includes", 
			"Extentions", 
			"Extensions Details", 
			"Trunks", 
			"Trunk Details", 
			"Routes",
			"Phone Groups",
			"Trunk Groups",
			"Trunk Group Includes",
			"Registrations IAX",
			"Registrations SIP",
			"Feature Codes",
			"Trace Viewer"
		};

		/// Full user error messages. @see Errors
		public static string[] ErrorMessages = { "", "Connection Failed." };
		/// Toggles stack trace messages etc.
		//private static Boolean Debug = true;

		/// instance of MainForm which creates and contains all others.
		public static MainForm m_mainForm;

		/// The main entry point for the application.
		[STAThread]
		static void Main()
		{
			log.Info("Program Start.");

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			m_mainForm = new MainForm();

			log.Info("Entering main loop.");

			Application.Run(m_mainForm);

			/// dispose mainForm just in case; is there a case where this isn't already disposed?
			m_mainForm.Dispose(); // probably not nedded

			log.Info("Program Ended Normally.");
		}
	}
}
