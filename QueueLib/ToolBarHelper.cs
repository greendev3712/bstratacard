using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using Lib;
using Npgsql;

namespace QueueLib
{

    public class ToolBarHelper
    {
        public delegate void GenericCallbackSub(string message);
        public delegate void InitializeCallbackFunction();
        public delegate void ProgramErrorCallbackFunction(string errorMsg, params string[] errorMsgFormat); // Fatal program Error

        private static ToolBarHelper.ProgramErrorCallbackFunction m_programErrorCallback;

        private static QD.QE_ErrorCallbackFunction m_errorHandler = QD.GenericErrorCallbackFunction;

        private static bool m_programErrorCallbackSet = false;

        private static string m_registryParent = "IntellaToolBar";

//        public delegate void LoggerCallbackObj(string desc, object thing);
//        public delegate void LoggerCallbackMsg(string msg, params string[] argsRest);

        private static QD.LoggerCallbackMsg m_logger = null;

        private static ToolbarServerConnection m_tsc;

        public static void ProgramError()
        {
            if (m_programErrorCallbackSet == true)
            {
                m_programErrorCallback.Invoke("");
            }
        }

        public void SetErrorCallback(QD.QE_ErrorCallbackFunction errorHandler) {
            m_errorHandler = errorHandler;
        }

        public static void SetLoggerCallback(QD.LoggerCallbackMsg loggerFn) {
            m_logger = loggerFn;
        }

        public static void SetProgramErrorCallback(ToolBarHelper.ProgramErrorCallbackFunction callback) {
            m_programErrorCallback = callback;
            m_programErrorCallbackSet = true;
        }

        public static void SetToolbarServerConnection(ToolbarServerConnection tsc) {
            m_tsc = tsc;
            DatabaseSettingsForm.SetToolbarServerConnection(m_tsc);
        }

        public static void SetRegistryParent(string registryParent) {
            m_registryParent = registryParent;
        }

        private static void Log(string msg, params string[] argsRest) {
            if (m_logger != null) {
                m_logger.Invoke(msg, argsRest);
            }
            else {
                Console.WriteLine("ToolBarHelper -- No Logger Defined! Logging To Console: " + String.Format(msg, argsRest));
            }
        }


        public static DbHelper GetDbHelper()
        {
            return m_tsc.m_db;
        }

        public static ToolbarServerConnection GetToolbarServerConnection() {
            return m_tsc;
        }

        // Get a local registry config item
        // With AutoFix...
        //
        // Prevoiusly we would store *ALL* settings in LOCAL_USER
        // But for workstations where different user logins need to all use the toolbar, we don't want to have to resetup and reconfigure the db connection
        //  for every single user
        //
        //  This will auto-migrate from LOCAL_USER to LOCAL_MACHINE when we retreieve a registry config item
        //
        public static string Registry_GetToolbarConfigItem(string registryParent, string registrySubKey, string registryItem) {
            string config_key_name = "Software\\VB and VBA Program Settings\\" + registryParent + "\\" + registrySubKey;
            RegistryKey local_machine_val = null;
            string config_val = null;

            try {
                local_machine_val = Registry.LocalMachine.OpenSubKey(config_key_name);
                Log("Registry_GetToolbarConfigItem Open LOCAL_MACHINE -- SUCCESS: {0}", registryItem);
            }
            catch (Exception e) {
                Log("Registry_GetToolbarConfigItem Open LOCAL_MACHINE -- FAILED: {0} [{1}]", registryItem, e.Message);
                Console.WriteLine("Open KEY Failed: " + config_key_name + " -- " + e.ToString());
            }

            if (local_machine_val != null) {
                object config_val_obj = local_machine_val.GetValue(registryItem);

                if (config_val_obj != null) {
                    config_val = local_machine_val.GetValue(registryItem).ToString();
                }
            }

            if (config_val == null || (config_val == "")) {
                // Get the item from LOCAL_USER and fix
                config_val = Interaction.GetSetting(registryParent, registrySubKey, registryItem);

                if (config_val != "") { 
                    try {
                        // Adminidtrator can set HKEY_LOCAL_MACHINE
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\" + config_key_name, registryItem, config_val);
                        Log("Registry_GetToolbarConfigItem Migrate to HKEY_LOCAL_MACHINE -- SUCCESS: {0})", registryItem);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.ToString());
                        // No access to the registry... this is fine.
                    }
                }
            }

            // Bergen Custom -- We'll need to figure out how to remove this... because some people's settings come from the LOCAL_MACHINE
            //   which can't be edited by a normal non-admin user and will basically be stuck there for the life of the machine
            //   maybe we need some sort of 'update time' and compare LOCAL_MACHINE vs CURRENT_USER and use the most recent one that's set
            //
            // If a bergen toolbar user is using the old ip. update
            if (registryItem == "DB_Host" && (config_val == "128.191.17.65")) {
                config_val = "pbx-a.newbridgehealth.org";
                Registry_SetToolbarConfigItem(registryParent, registrySubKey, registryItem, config_val);
            }

            if (registryItem == "DB_Host" && (config_val == "ch02.gtxit.com")) {
                Registry_SetToolbarConfigItem(registryParent, registrySubKey, "DB_Port", "4001");
            }

            return config_val;
        }

       // Main Toolbar Config (Try and set for the LOCAL_MACHINE.  Otherwise store in LOCAL_USER
       public static void Registry_SetToolbarConfigItem(string registryParent, string registrySubKey, string registryItem, string configVal) {
            string config_key_name = "Software\\VB and VBA Program Settings\\" + registryParent + "\\" + registrySubKey;

            try { 
                Registry.SetValue("HKEY_LOCAL_MACHINE\\" + config_key_name, registryItem, configVal);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                // No access to the registry... this is fine... we'll save it locally

                Interaction.SaveSetting(registryParent, registrySubKey, registryItem, configVal);
            }
       }

        public static bool QuickDatabaseSetupCheck(ref DbHelper dbHelper)
        {
            string host     = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Host");
            string port     = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Port");
            string user     = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_User");
            string pass     = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Pass");
            string database = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Name");

            lock (dbHelper)
            {
                dbHelper.initConnection(DbHelper.generateConnectionDataObject(host, port, user, pass, database));
                return dbHelper.connect();
            }
        }

        public static bool quickAgentLoginSetupCheck()
        {
            // Get from HKEY_LOCAL_USER
            return  "" != Interaction.GetSetting(m_registryParent, "Config", "USER_agentNumber") &&
                    "" != Interaction.GetSetting(m_registryParent, "Config", "USER_agentExtension");
        }
        
        public static bool DatabaseSetup(ref DbHelper db)
        {
            string DB_Host = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Host");
            string DB_Port = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Port");
            string DB_User = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_User");
            string DB_Pass = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Pass");
            string DB_Name = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Name");

            lock (db)
            {
                // Already have a connection configured
                int db_port_int;

                if (!Int32.TryParse(DB_Port, out db_port_int)) {
                    return false;
                }

                try { 
                  db.initConnection(DbHelper.generateConnectionDataObject(DB_Host, DB_Port, DB_User, DB_Pass, DB_Name));
                }
                catch (Exception ex) {
                    MessageBox.Show("Connection Failed. Host: \r\n" + DB_Host + "\r\nError: " + ex.Message);
                    return false;
                }

                // NEW - Using new centralized DB connection handling
                if ( DoConnection(db) == false ) {
                    return false;
                }
            }

            return true;
        }

        public static bool DatabaseSetupCheck(ref DbHelper db, string db_host, GenericCallbackSub successCallback, GenericCallbackSub failureCallback)
        {
            string DB_Host = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Host");
            string DB_Port = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Port");
            string DB_User = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_User");
            string DB_Pass = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Pass");
            string DB_Name = Registry_GetToolbarConfigItem(m_registryParent, "Config", "DB_Name");

            // Allow override of the host
            if (db_host != null) {
                DB_Host = db_host;
            }

            DatabaseSettingsForm dbSettingsForm = new DatabaseSettingsForm();
            dbSettingsForm.SetErrorCallback(m_errorHandler);
            dbSettingsForm.SetRegistryParent(m_registryParent);

            dbSettingsForm.SetSuccessCallback(successCallback);
            dbSettingsForm.SetFailureCallback(failureCallback);

            if ((string.IsNullOrEmpty(DB_Host) | string.IsNullOrEmpty(DB_Port) | string.IsNullOrEmpty(DB_User) | string.IsNullOrEmpty(DB_Pass) | string.IsNullOrEmpty(DB_Name)))
            {
                // Installing for the first time... offer admin
                // Utils.Elevate();

                DatabaseSettingsForm.SetDbHelper(db);
                dbSettingsForm.Show();
                return false;
            }

            lock (db)
            {
                // Already have a connection configured

                try {
                    db.initConnection(DbHelper.generateConnectionDataObject(DB_Host, DB_Port, DB_User, DB_Pass, DB_Name));
                }
                catch (Exception ex) {
                    MessageBox.Show("Connection Failed. Host: \r\n" + DB_Host + "\r\nError: " + ex.Message);
                    return false;
                }

                // NEW - Using new centralized DB connection handling
                if (DoConnection(db) == false) {
                    return false;
                }
            }

            if (successCallback != null) {
                successCallback.Invoke("");
            }

            return true;
        }

        public static bool DoConnection(DbHelper db)  // NEW - Centralize DB connection error handling
        {
            bool connect_result = false;
            string errmsg = "";

            if (false && Debugger.IsAttached)
            {
                connect_result = db.connect();
                goto done;
            }

            // Regular (Production)
            try
            {
                connect_result = db.connect();
            }
            catch (NpgsqlException ex)
            {
                errmsg = ex.InnerException.Message;
            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
            }

          done:
            if (!connect_result)
            {
                MessageBox.Show("Connection Failed. Host: \r\n" + db.getDbHost() + "\r\nError: " + errmsg);
                return false;
            }

            return true;
        }
    }
}