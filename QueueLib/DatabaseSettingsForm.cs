using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Deployment;
using System.Diagnostics;
using System.Windows.Forms;
using System.Security.Principal;
using Lib;
using Npgsql;

namespace QueueLib
{
	public partial class DatabaseSettingsForm : System.Windows.Forms.Form
	{
		private ToolBarHelper.GenericCallbackSub m_successCallback;
		private bool m_successCallBackSet = false;
		private ToolBarHelper.GenericCallbackSub m_failureCallback;
        private string m_registryParent = "IntellaToolBar";

        private bool m_failureCallBackSet = false;

        private static ToolbarServerConnection m_tsc;
		private static DbHelper m_db;
        private static QD.QE_ErrorCallbackFunction m_errorHandler = QD.GenericErrorCallbackFunction;

        private bool m_forceClose = false;

        private string m_host;
        private string m_port;
        private string m_user;
        private string m_pass;
        private string m_dbname;

        private string m_default_host = "";
        private string m_default_port = "5432";
        private string m_default_user = "root";
        private string m_default_pass = "postgresadmin";
        private string m_default_db   = "pbx";

        // private ConnectionManager m_cm;
        
        public void ConstructHelper() {
            if (!Debugger.IsAttached) {
               this.TopMost = true;
            }

            if (Utils.IsAdministratorSimple()) {
                cmpRunAsAdministratorButton.Text   = "Running As Admin";
                cmpRunAsAdministratorButton.Click -= this.cmpRunAsAdministratorButton_Click;

                cmpRunAsAdministratorButton.Font = new System.Drawing.Font(
                    cmpRunAsAdministratorButton.Font.FontFamily, 
                    cmpRunAsAdministratorButton.Font.Size, 
                    System.Drawing.FontStyle.Bold
                );
            }
        }

        public DatabaseSettingsForm(string registryParent) {
            m_registryParent = registryParent;

            InitializeComponent(); // This call is required by the designer.
            ConstructHelper();
        }

		public DatabaseSettingsForm()
		{
            InitializeComponent(); 			// This call is required by the designer.
            ConstructHelper();
		}

        public void SetRegistryParent(string registryParent) {
            m_registryParent = registryParent;
        }

        public void SetErrorCallback(QD.QE_ErrorCallbackFunction errorHandler) {
            m_errorHandler = errorHandler;
        }

        //public void setConnectionManager(ConnectionManager cm)
        //{
        //    m_cm = cm;
        //    setConnectionTreeView(m_cm.ConnectionTreeView);
        //    connectionToolStripMenuItem.DropDownItems.Insert(0, m_cm.NewConnectionMenuItem);
        //}

		public void setConnectionTreeView(TreeView connectionTreeView)
		{
			//connectionTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			connectionTreeView.Dock = DockStyle.Fill;
			this.Controls.Add(connectionTreeView);
			//connectionTreeView.BringToFront();
		}

		private void InitSettings()
		{
            string db_Host = ToolBarHelper.Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Host");
            string db_Port = ToolBarHelper.Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Port");
            string db_User = ToolBarHelper.Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_User");
            string db_Pass = ToolBarHelper.Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Pass");
            string db_Name = ToolBarHelper.Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Name");

            this.m_host   = db_Host;
            this.m_port   = db_Port;
            this.m_user   = db_User;
            this.m_pass   = db_Pass;
            this.m_dbname = db_Name;

            bool debugging = Debugger.IsAttached;

            if ((string.IsNullOrEmpty(db_Host) || string.IsNullOrEmpty(db_Port) || string.IsNullOrEmpty(db_User) || string.IsNullOrEmpty(db_Pass) || string.IsNullOrEmpty(db_Name)))
			{
                // First Run!

                if (!debugging) { 
                  // If we're installing for the first time, this stuff will all be empty... so hide advanced by default so the users don't mess anything up
                  this.cmpAdvancedPanel.Visible     = false;
                  this.cmpSaveAndConnectButton.Text = "Connect";
                }

                // We've never been set up.  Put in some sane defaults
                db_Port = m_default_port;
                db_User = m_default_user;
                db_Pass = m_default_pass;
                db_Name = m_default_db;

                // Make sure saveAndConnect uses these values even if they are not changed
                TextBox_Host.Tag = "UPDATED";

                if (debugging) {
                    // Make sure saveAndConnect uses these values even if they are not changed
                    // We'll use the defaults if nothing is changed when in development mode
                    TextBox_Port.Tag = "UPDATED";
                    TextBox_User.Tag = "UPDATED";
                    TextBox_Pass.Tag = "UPDATED";
                    TextBox_Name.Tag = "UPDATED";

                    // There's no 'standard' host.  Stick something in here for the heck of it
                    db_Host = m_default_host; // Only for testing.. otherwise we will get a reasonable hostname for the default

                    TextBox_Host.Text = db_Host;
                    TextBox_Port.Text = db_Port;
                    TextBox_User.Text = db_User;
                    TextBox_Pass.Text = db_Pass;
                    TextBox_Name.Text = db_Name;
                    return;
                }

                // Production! (Or forced-non-debug mode)
                // Auto-Detect Host for Live-Deployments!

                try {
                    // TODO: Now we just assume the connect-to is the same as where we got the app from, but this may not always be the case in the future
                    // TODO: ... for clustered deployments

                    Uri deployment_uri = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.UpdateLocation;
                    if ((deployment_uri != null) && (deployment_uri.Host != "")) {
                        db_Host = deployment_uri.Host;
                    }
                }
                catch (Exception ex) {
                    Exception e = ex; // Get rid of warning

                    // Could not auto-detect host.  We need manual setup.

                    // Year-2020 New-Style Server URL: default_server.txt

                    try {
                        string default_server = System.IO.File.ReadAllText("default_settings.txt");
                        db_Host = default_server;
                    }
                    catch (Exception exi) {
                        Exception ei = exi; // Get rid of warning
                        
                    }

                    // One last attempt at getting the url... it may have been passed to us as a command line argument 
                    // During privilege escillation
                    if (db_Host == "") {
                        string[] program_args = (string[])Utils.Globals["ARGS"];
                        if (program_args != null) {
                            if (program_args.Length > 0) {
                                db_Host = program_args[0];
                            }
                        }
                        else {
                            // Otherwise we have no choice but to ask the user
                            this.cmpAdvancedPanel.Visible = true;
                            this.cmpSaveAndConnectButton.Text = "Save And Connect";
                        }
                    }
                }

                if (db_Host == "kc02.gtxit.com") {
                    db_User = "c30034";
                    db_Pass = "b12b79b1225c023e106febda3005bca8";
                }

                /*
                 * For Apollo
                 * 
                db_Host = "kc02.gtxit.com";
                db_Port = "4001";
                db_User = "c30034";
                db_Pass = "b12b79b1225c023e106febda3005bca8";
                */
                  
                // END - First Run Block
			}
              
            // These will be used when the user does a Save/Connect
            this.m_host   = db_Host;
            this.m_port   = db_Port;
            this.m_user   = db_User;
            this.m_pass   = db_Pass;
            this.m_dbname = db_Name;

            ////
            // Populate the settings 

			TextBox_Host.Text = db_Host;

            if (db_Port == m_default_port) {
                TextBox_Port.Text = "<Default>";
                TextBox_Port.ForeColor = System.Drawing.Color.Gray;
            }
            else {
				TextBox_Port.Text = db_Port;
            }
            
            if (db_User == m_default_user) {
                TextBox_User.Text = "<Default>";
                TextBox_User.ForeColor = System.Drawing.Color.Gray;
            }
            else {
                TextBox_User.Text = db_User;
            }

            if (db_Pass == m_default_pass) {
                TextBox_Pass.Text = "<Default>";
                TextBox_Pass.ForeColor = System.Drawing.Color.Gray;
            }
            else {
                TextBox_Pass.Text = db_Pass;
            }

            if (db_Name == m_default_db) {
                TextBox_Name.Text = "<Default>";
                TextBox_Name.ForeColor = System.Drawing.Color.Gray;
            }
            else {
                TextBox_Name.Text = db_Name;
            }
		}

        public static bool IsDatabaseConfiguredSuccessfully()
        {
            return m_db.isConnected();
        }

		public void SetSuccessCallback(ToolBarHelper.GenericCallbackSub callback)
		{
			m_successCallback = callback;
			m_successCallBackSet = true;
		}

		public void SetFailureCallback(ToolBarHelper.GenericCallbackSub callback)
		{
			m_failureCallback = callback;
			m_failureCallBackSet = true;
		}

        internal static void SetDbHelper(DbHelper db)
        {
            lock (db)
            {
                m_db = db;
            }
        }

        internal static void SetToolbarServerConnection(ToolbarServerConnection tsc) {
            m_tsc = tsc;

            lock (tsc.m_db) {
                m_db = tsc.m_db;
            }
        }

		private void Button_SaveAndConnect_Click(System.Object sender, System.EventArgs e)
		{
			saveAndConnect();
		}

		private void saveAndConnect()
		{
            string host = this.m_host;
            string port = this.m_port;
            string user = this.m_user;
            string pass = this.m_pass;
            string db   = this.m_dbname;

            // TestBox_Host is special because it's already entered when the form loads... so it wont trigger the enter event
            //   if someone puts in a new host
            if (((string) TextBox_Host.Tag == "UPDATED") || (TextBox_Host.Text != host)) { host = TextBox_Host.Text; }
            if (((string) TextBox_Port.Tag == "UPDATED") && (TextBox_Port.Text != ""))   { port = TextBox_Port.Text; }
            if (((string) TextBox_User.Tag == "UPDATED") && (TextBox_User.Text != ""))   { user = TextBox_User.Text; }
            if (((string) TextBox_Pass.Tag == "UPDATED") && (TextBox_Pass.Text != ""))   { pass = TextBox_Pass.Text; }
            if (((string) TextBox_Name.Tag == "UPDATED") && (TextBox_Name.Text != ""))   { db   = TextBox_Name.Text; }

            lock (m_db)
            {
                bool connect_result = false;
                string errmsg = "";

                try {
                    m_db.initConnection(connectionParameters: DbHelper.generateConnectionDataObject(host, port, user, pass, db));
                    connect_result = m_db.connect();
                }
                catch (NpgsqlException ex) {
                    errmsg = ex.InnerException.Message;
                }
                catch (Exception ex) {
                    errmsg = ex.Message;
                }

                if (!connect_result)
                {
                    MessageBox.Show("Connection Failed: host: " + TextBox_Host.Text, "Error:" + errmsg);
                    return;
                }
            }

            this.m_host   = host;
            this.m_port   = port;
            this.m_user   = user;
            this.m_pass   = pass;
            this.m_dbname = db;

            ToolBarHelper.Registry_SetToolbarConfigItem(m_registryParent, "Config", "DB_Host", host);
            ToolBarHelper.Registry_SetToolbarConfigItem(m_registryParent, "Config", "DB_Port", port);
            ToolBarHelper.Registry_SetToolbarConfigItem(m_registryParent, "Config", "DB_User", user);
            ToolBarHelper.Registry_SetToolbarConfigItem(m_registryParent, "Config", "DB_Pass", pass);
            ToolBarHelper.Registry_SetToolbarConfigItem(m_registryParent, "Config", "DB_Name", db);

            // Handle Automatic Startup
            if (!Debugger.IsAttached) { 
                Utils.SetApplicationAutomaticStartup(this.cmpRunAtStartupCheckBox.Checked, "IntellaQueueToolbar");
            }

            MessageBox.Show("Now connected to " + TextBox_Host.Text + ".", "Connection Successful.");
			this.Close();
		}

		public void Form_Close()
		{
            if (this.m_forceClose) {
                this.Dispose();
                return;
            }

            bool dbConnected = false;

            lock (m_db)
            {
                dbConnected = m_db.isConnected();
            }

			if (dbConnected)
			{
				if (m_successCallBackSet)
				{
					m_successCallback.Invoke("");
				}
			}
			else
			{
				if (m_failureCallBackSet)
				{
					m_failureCallback.Invoke("");
				}
			}

            this.Dispose();
		}

		private void DatabaseSettingsForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
		{
			Form_Close();
		}

		private void DatabaseSettingsForm_Shown(object sender, EventArgs e)
		{
			InitSettings();
			TextBox_Host.Focus();
		}

		private void TextBox_Host_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (e.KeyChar == Strings.Chr(13)) // Enter key
				saveAndConnect();
		}

        private void DatabaseSettingsForm_Load(object sender, EventArgs e) {

        }

        private void cmpShowHideAdvancedButton_Click(object sender, EventArgs e) {
            cmpAdvancedPanel.Visible = !cmpAdvancedPanel.Visible;
        }


        private void TextBox_Default_Enter(object sender, EventArgs e) {
            TextBox t = (TextBox) sender;

            if (t.Text == "<Default>") {
                t.Tag       = "UPDATED";
                t.Text      = "";
                t.ForeColor = System.Drawing.SystemColors.ControlText;
            }

            if (t.Tag == null) {
                t.Tag = "ENTER";
            }
        }


        private void TextBox_Default_Changed(object sender, EventArgs e) {
            TextBox t = (TextBox) sender;

            if (t.Tag != null) {
                string tag_string = (string) t.Tag;

                if (tag_string == "ENTER") {
                    t.Tag = "UPDATED";
                }
            }
        }

        private void TextBox_Port_Enter(object sender, EventArgs e) {
            TextBox_Default_Enter(sender, e);
        }

        private void TextBox_Name_Enter(object sender, EventArgs e) {
            TextBox_Default_Enter(sender, e);
        }

        private void TextBox_User_Enter(object sender, EventArgs e) {
            TextBox_Default_Enter(sender, e);
        }

        private void TextBox_Pass_Enter(object sender, EventArgs e) {
            TextBox_Default_Enter(sender, e);
        }

        private void Label1_Click(object sender, EventArgs e) {

        }

        private void TextBox_Host_TextChanged(object sender, EventArgs e) {
            TextBox_Default_Changed(sender, e);
        }

        private void TextBox_Port_TextChanged(object sender, EventArgs e) {
            TextBox_Default_Changed(sender, e);
        }

        private void TextBox_Name_TextChanged(object sender, EventArgs e) {
            TextBox_Default_Changed(sender, e);
        }

        private void TextBox_User_TextChanged(object sender, EventArgs e) {
            TextBox_Default_Changed(sender, e);
        }

        private void TextBox_Pass_TextChanged(object sender, EventArgs e) {
            TextBox_Default_Changed(sender, e);
        }

        private void cmpRunAsAdministratorButton_Click(object sender, EventArgs e) {
            string arguments = "";

            // First arg to intellaQueue.exe is the Connect-to DB-Host... see if we can prepopulate it
            try { 
                Uri deployment_uri = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.UpdateLocation;
                if ((deployment_uri != null) && (deployment_uri.Host != "")) {
                    arguments = deployment_uri.Host;
                }
            }
            catch (Exception ex) {
                Exception exx = ex; // Get rid of warning
            }

            Utils.Elevate(arguments); // Become Admin!
            this.m_forceClose = true; // Don't do any cleanups or registry saves
            Application.Exit();       // Elevated app will now write to HKEY_LOCAL_MACHINE
        }
    }
}
