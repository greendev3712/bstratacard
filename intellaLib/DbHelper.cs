/*
DbHelper.cs
Intellasoft Configurator.
 
@author Mike Jacobi for Intellasoft
*/

using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using log4net;
using Npgsql;

namespace Lib {
    public class SimpleDbConnectionParameters {
        public string Host;
        public string Port;
        public string DB;
        public string User;
        public string Password;
        public string Pooling = "false";

        public Boolean SSL_Required = false;

        public SimpleDbConnectionParameters() {

        }

        [Obsolete ("Use DbHelper Constructor")]
        public SimpleDbConnectionParameters(Hashtable initParams) {
            this.Host         = (string)  initParams["host"];
            this.Port         = (string)  initParams["port"];
            this.DB           = (string)  initParams["database"];
            this.User         = (string)  initParams["user"];
            this.Password     = (string)  initParams["pass"];
            this.Pooling      = (string)  initParams["pooling"];
            this.SSL_Required = (Boolean) initParams["useSSL"];

            // Defaults
            if (this.Pooling == "") { this.Pooling = "false"; }
        }

        public SimpleDbConnectionParameters Clone() {
            SimpleDbConnectionParameters new_clone = new SimpleDbConnectionParameters(this.ToHashtable());
            return new_clone;
        }

        // Backwards Compatability with the old Hashtable
        public Boolean ContainsKey(string index) {
            switch (index) {
                case "host":     return true;
                case "port":     return true;
                case "database": return true;
                case "user":     return true;
                case "pass":     return true;
                case "pooling":  return true;
            }

            return false;
        }

        // Backwards Compatability with the old Hashtable
        public string this[string index] {
            get {
                switch (index) {
                    case "host":     return this.Host;
                    case "port":     return this.Port;
                    case "database": return this.DB;
                    case "user":     return this.User;
                    case "pass":     return this.Password;
                    case "pooling":  return this.Pooling;
                }

                return null;
            }
            set {
                switch (index) {
                    case "host":     this.Host     = value; break;
                    case "port":     this.Port     = value; break;
                    case "database": this.DB       = value; break;
                    case "user":     this.User     = value; break;
                    case "pass":     this.Password = value; break;
                    case "pooling":  this.Pooling  = value; break;
                }

                throw new IndexOutOfRangeException(String.Format("{0} is not a valid parameter for SimpleDbConnectionParameters", index));
            }
        }

        public Hashtable ToHashtable() {
            Hashtable h = new Hashtable() {
                {"host",     this.Host},
                {"port",     this.Port},
                {"user",     this.User},
                {"pass",     this.Password},
                {"database", this.DB},
                {"pooling",  this.Pooling}
            };

            return h;
        }
    }


    //////////////////////////////////////////////////////////////////////////////////
    /// creates and maintains connection to database, and provides
    /// helper methods.
    ///
    /// Each instance handles 1 connection. Currently handles npgsql and odbc connections.
    public class DbHelper : IDisposable {
        private static readonly ILog log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// no options bit index
        public const int OPTION_NONE = 0;
        /// debug option bit index
        public const int OPTION_DEBUG = 1;

        private string m_hostName;
        private string m_hostPort;
        private string m_databaseName;
        private string m_userName;
        private string m_userPassword;
        private string m_connectionString;
        private Boolean m_preparedQueries;

        private DbHelperConnection m_connection;
        private string m_lastError;

        // New-Style Error Handler
        private QD.QE_ErrorCallbackFunction m_handleErrorCallback = QD.GenericErrorCallbackFunction;

        // Old-Style Error Handler
        public delegate void handleErrorLegacyCallback(Exception ex, string errorMessage);

        private string m_type;
        private bool m_isDisposed;

        public bool m_useSSL = false;

        private Hashtable m_dbColumnInfoCache;

        //////////////////////////////////////////////////////////////////////////////////
        /// Constructors
        ///
        /// 

        // Initialize the db connection object using npgsql but do not connect yet
        //
        public DbHelper(string host, string port, string user, string pass, string database, Boolean useSsl, int commandTimeout) {
            this.m_useSSL = useSsl;
            _construct(host, port, user, pass, database, useSsl, commandTimeout);
        }

        public DbHelper(string host, string port, string user, string pass, string database, Boolean useSsl) {
            this.m_useSSL = useSsl;
            _construct(host, port, user, pass, database, useSsl, null);
        }

        public DbHelper(string host, string port, string user, string pass, string database) {
            _construct(host, port, user, pass, database, false, null);
        }

        public DbHelper(handleErrorLegacyCallback errorLegacyCallback) {
            // Backwards-compat
            // Our error handler will Invoke old-style errorCallback with our new-style params we get from the error handler
            m_handleErrorCallback = delegate (QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, string[] msgFormat) {
                errorLegacyCallback.Invoke(ex, String.Format("{0} {1} {2}", errorLevel, errorToken, String.Format(msg, msgFormat)));
                return "";
            };
            
            _construct("", "", "", "", "", false, null);
        }

        private void _construct(string host, string port, string user, string pass, string database, Boolean useSsl, int? commandTimeout) {
            log.Debug("");
            m_connectionString = "";
            m_type = "npgsql";
            m_handleErrorCallback = null;

            m_connection = new DbHelperConnection(m_connectionString, m_type);

            m_connection.m_useSsl = useSsl;
            if (commandTimeout != null) {
                m_connection.m_commandTimeout = (int)commandTimeout;
            }

            m_hostName = host;
            m_hostPort = port;
            m_databaseName = database;
            m_userName = user;
            m_userPassword = pass;

            SimpleDbConnectionParameters connection = generateConnectionDataObject(host, port, user, pass, database, useSsl);
            initConnection(connection);
            m_dbColumnInfoCache = new Hashtable(16);
        }

        public void SetErrorCallback(QD.QE_ErrorCallbackFunction errorHandler) {
            m_handleErrorCallback = errorHandler;
            m_connection.SetErrorCallback(errorHandler);
        }

        public void SetPrepared(Boolean value) {
            this.m_preparedQueries = value;
        }

        /// 
        /// Initializes the db connection object, but does not connect yet.
        /// @param type Type of connection. npgsql and odbc supported.
        /// @param connectionString connection string as needed by corresponding .NET Connection object
        /// @param options bit encoding representing options
        public DbHelper(string type, string connectionString, int options, handleErrorLegacyCallback errorLegacyCallback) {
            m_connectionString = connectionString;
            m_type = type;

            // Backwards-compat
            // Our error handler will Invoke old-style errorCallback with our new-style params we get from the error handler
            m_handleErrorCallback = delegate (QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, string[] msgFormat) {
                errorLegacyCallback.Invoke(ex, String.Format("{0} {1} {2}", errorLevel, errorToken, String.Format(msg, msgFormat)));
                return "";
            };
            
            m_connection = new DbHelperConnection(m_connectionString, m_type);

            m_dbColumnInfoCache = new Hashtable(16);
        }

                /// 
        /// Initializes the db connection object, but does not connect yet.
        /// @param type Type of connection. npgsql and odbc supported.
        /// @param connectionString connection string as needed by corresponding .NET Connection object
        /// @param options bit encoding representing options
        public DbHelper(string type, string connectionString, int options, QD.QE_ErrorCallbackFunction errorCallback) {
            m_connectionString = connectionString;
            m_type = type;

            m_handleErrorCallback = errorCallback;
            
            m_connection = new DbHelperConnection(m_connectionString, m_type);

            m_dbColumnInfoCache = new Hashtable(16);
        }


        public DbHelper() {
            m_connectionString = "";
            m_type = "npgsql";

            m_connection = new DbHelperConnection(m_connectionString, m_type);

            m_dbColumnInfoCache = new Hashtable(16);
        }

        public DbHelper(NpgsqlConnection n, QD.QE_ErrorCallbackFunction errorCallback) {
            m_connectionString = "";
            m_type = "npgsql";
            m_handleErrorCallback = errorCallback;

            m_connection = new DbHelperConnection(n);

            m_dbColumnInfoCache = new Hashtable(16);
        }


        //////////////////////////////////////////////////////////////////////////////////
        /// Destructor.
        ///
        /// Checks if database connection is still open and close it.
        ~DbHelper() {
            log.Debug("Dispose called by GC.");
            Dispose(false);
        }

        public void Dispose() {
            log.Debug("Dispose called manually.");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManagedResources) {
            if (!m_isDisposed) {
                if (disposeManagedResources) {
                    // dispose managed resources
                    if (m_connection != null &&
                        m_connection.isInitialized() &&
                        m_connection.getState().HasFlag(ConnectionState.Closed)) {
                        m_connection.close();
                    }
                    /// The above was moved here from the destructor. We also call Dispose from the 
                    /// end of main(). This should handle the issue of the npgsql 
                    /// connection seeming to be already closed when the dctr tried the above code.
                    /// Sometimes, it was even disposed, which caused an exception/crash with 
                    /// getState(). Seems like it was getting garbage collected.. not sure why; 
                    /// perhaps it was detecting the app exiting, and the connection object was gc'ed
                    /// before the DBHelper object.
                }
                // dispose unmanaged resources (don't have any)
                m_isDisposed = true;
            }
            else {
                log.Error("Dispose called more than once.");
            }
        }

        // Don't throw any exceptions, use the error handler callback instead
        [Obsolete("This method is deprecated. Use SetErrorCallback instead.")]
        public void setErrorHandler(QD.QE_ErrorCallbackFunction errorHandler) {
            this.m_handleErrorCallback = errorHandler;
            this.m_connection.SetErrorCallback(errorHandler);
        }

        public void clearErrorHandler() {
            this.m_handleErrorCallback = null;
        }

        public string getLastError() {
            return this.m_lastError;
        }

        public bool LastQueryWasError() {
            return !(this.m_lastError == null);
        }

        [Obsolete("generateConnectionDataObject should be migrated right into SimpleDbConnectionParameters")]
        public static SimpleDbConnectionParameters generateConnectionDataObject(string host, string port, string user, string pass, string database, bool useSSL = true) {
            SimpleDbConnectionParameters result = new SimpleDbConnectionParameters(new Hashtable()
            {
                {"host",     host},
                {"port",     port},
                {"user",     user},
                {"pass",     pass},
                {"database", database},
                {"useSSL",   useSSL }
            });

            return result;
        }

        public string getDbHost()  // NEW - For error reporting
        {
            return (string)getConnectionParameters().Host;
        }

        public SimpleDbConnectionParameters getConnectionParameters()  // NEW - Get the connection params from the underlying connection object
        {
            return m_connection.getConnectionParameters();
        }

        public SimpleDbConnectionParameters cloneConnectionParameters(string host)  // NEW - clone the connection data object with a new host
        {
            SimpleDbConnectionParameters new_conn = (SimpleDbConnectionParameters)getConnectionParameters().Clone();

            new_conn.Host = host;

            return new_conn;
        }

        public void initConnection(SimpleDbConnectionParameters connectionParameters) {

            m_connection.init(connectionParameters);
        }

        public static Boolean IsConnectionException(Exception ex) {
            if (ex.GetType() == typeof(NpgsqlException)) {
                NpgsqlException n_ex = (NpgsqlException)ex;

                if (n_ex.InnerException.Message.Contains("broken")) {
                    return true;
                }
            }

            return false;
        }

        //////////////////////////////////////////////////////////////////////////////////
        /// encode a string to be used as a literal in an sql statement.
        ///
        /// escapes ' with ''.
        /// @param str ref to string to encode
        /// @todo: find bug which calls this twice on query strings
        /// @todo: look into prepared queries
        private void dbEncode(ref string str) {
            if (str != null) {
                str = str.Replace("'", "''");
            }
        }

        public static string dbEncode(string str) {
            string result = null;
            if (str != null)
                result = str.Replace("'", "''");
            return result;
        }

        private void tackOnWhereCondition(ref string query, ref string[] whereNames, ref string[] whereValues) {
            tackOnWhereCondition(ref query, ref whereNames, ref whereValues, false, false);
        }

        private void tackOnWhereCondition(ref string query, ref string whereName, ref string[] whereValues) {
            string[] whereNames = new string[] { whereName };
            tackOnWhereCondition(ref query, ref whereNames, ref whereValues, false, true);
            whereName = whereNames[0];
        }

        //////////////////////////////////////////////////////////////////////////////////
        /// constructs and concatenates a where clause to the end of an sql query string.
        ///
        /// @param query ref to query string to add to
        /// @param whereNames column names which must have specific value (whereValues) to match
        /// @param whereValues correspoding array of values to whereNames
        /// @param doUseOr If true, one supplied condition must be true; otherwise all must be true (AND).
        /// @todo: find bug which calls this twice on query strings
        private void tackOnWhereCondition(ref string query, ref string[] whereNames, ref string[] whereValues, bool doUseOr, bool doUseIn) {
            // sanity checking
            if (whereNames == null ||
                    whereValues == null ||
                    whereNames.Length <= 0 ||
                    whereValues.Length <= 0 ||
                    whereNames[0] == null ||
                    whereValues[0] == null) {
                return;
            }
            if (!doUseIn && whereNames.Length != whereValues.Length) {
                return;
            }

            dbEncode(ref whereValues[0]);

            if (doUseIn) {
                query += " WHERE " + whereNames[0] + " IN ('" + whereValues[0] + "'";
                for (int i = 1; i < whereValues.Length; i++) {
                    dbEncode(ref whereValues[i]);
                    query += ", '" + whereValues[i] + "'";
                }
                query += ")";
            }
            else {
                query += " WHERE " + whereNames[0] + "='" + whereValues[0] + "'";
                for (int i = 1; i < whereNames.Length; i++) {
                    dbEncode(ref whereValues[i]);
                    query += (doUseOr ? " OR " : " AND ") + whereNames[i] + "='" + whereValues[i] + "'";
                }
            }

        }

        private void tackOnWhereConditionV2(ref string query, Hashtable whereConditions) {
            if (whereConditions == null)
                return;

            string wherePart = "";
            bool isFirstItem = true;
            foreach (object key in whereConditions.Keys) {
                wherePart += isFirstItem ? " WHERE " : " AND ";
                isFirstItem = false;
                if (!(key is string)) {
                    Debug.Print("error: where name not string");
                    return;
                }

                if (whereConditions[key] is string) {
                    wherePart += key + " = '" + dbEncode((string)whereConditions[key]) + "'";
                }
                else if (whereConditions[key] is List<string>) {
                    List<string> list = (List<string>)whereConditions[key];

                    if (list.Count > 0)
                        wherePart += key + " IN('" + dbEncode(list[0]) + "'";
                    for (int i = 1; i < list.Count; i++)
                        wherePart += ", '" + dbEncode(list[i]) + "'";
                    if (list.Count > 0)
                        wherePart += ")";
                }
                else if (whereConditions[key] is IList<string>) {
                    IList<string> list = (IList<string>)whereConditions[key];

                    if (list.Count > 0)
                        wherePart += key + " IN('" + dbEncode(list[0]) + "'";
                    for (int i = 1; i < list.Count; i++)
                        wherePart += ", '" + dbEncode(list[i]) + "'";
                    if (list.Count > 0)
                        wherePart += ")";
                }
                else if (whereConditions[key] == null) {
                    wherePart += key + " IS NULL";
                }
                else {
                    Debug.Print("error: where value not string or list");
                    return;
                }
            }
            query += wherePart;
        }

        private void handleError(string errorMessage) {
            this.m_handleErrorCallback.Invoke(QD.ERROR_LEVEL.ERROR, "DB_ERROR", null, errorMessage);
        }

        private void handleError(Exception ex, string errorMessage) {
            this.m_handleErrorCallback.Invoke(QD.ERROR_LEVEL.ERROR, "DB_ERROR", ex, errorMessage);
        }

        public DateTime GetDbConnectTime() {
            return m_connection.m_connect_time;
        }

        public void disconnect() {
            lock (this.m_connection) {
                m_connection.close();
            }
        }

        
        public bool connect() {
            lock (this.m_connection) {
                return m_connection.connect();
            }
        }

        // TODO: TO BE DEPRECATED
        //
        // return 1 on success
        // return 0 on query error
        [Obsolete("dbQuery is deprecated, please use DbSelect instead.")]
        public int dbQuery(string sqlQuery, List<OrderedDictionary> resultData) {
            lock (this.m_connection) {
                return _dbQuery(sqlQuery, resultData);
            }
        }

        private int _dbQuery(string sqlQuery, List<OrderedDictionary> resultData) {
            DbHelperCommand command;
            IDataReader reader;

            this.m_lastError = null;

            if (resultData != null) {
                resultData.Clear();
            }

            if (!m_connection.getState().HasFlag(ConnectionState.Open)) {
                if (!m_connection.connect()) {
                    handleError("Query Error:\r\n " + sqlQuery + "\r\n\r\nFailed to connect to DB");
                    return 0;
                }
            }

            try {
                command = new DbHelperCommand(sqlQuery, m_connection);
                reader = command.executeReader(); // (CommandBehavior.CloseConnection)
            }
            catch (Exception ex) {
                string error = "Error running SQL:\r\n " + sqlQuery + "\r\n\r\n" + ex.Message + (ex.InnerException == null ? "" : ex.InnerException.Message);
                handleError(ex, error);
                return 0;
            }

            // for each row
            if (resultData == null) {
                reader.Close();
                return 1;
            }

            while (true) {
                Boolean reader_success = false;

                try {
                    reader_success = reader.Read();
                }
                catch (Exception ex) {
                    handleError("Query Exception:\r\n" + sqlQuery + "\r\n\r\n" + ex.Message);
                    resultData.Clear();
                    reader.Close();
                    return 0;
                }

                if (!reader_success) {
                    break;
                }

                OrderedDictionary od = new OrderedDictionary();

                // for each column within that row
                for (int i = 0; i < reader.FieldCount; i++) {
                    // save value if this column is the uid key column
                    string columnName = reader.GetName(i);
                    string columnValue = reader.IsDBNull(i) ? null : reader.GetValue(i).ToString();
                    od.Add(columnName, columnValue);
                }

                resultData.Add(od);
            }

            reader.Close();
            return 1;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// New Stuff for SimpleDB
        ///////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////
        // DbQueryResultSet
        //////////////////////////////////////////

        public QueryResultSet DbQueryResultSet(string sqlQuery, List<SimpleDB_BindArg> bindArgsList) {
            lock (this.m_connection) {
                return _DbQueryResultSet(sqlQuery, bindArgsList);
            }
        }

        /// <summary>
        ///  Do a DB query and return a List&lt;QueryResultSet&gt;
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <returns>List&lt;QueryResultSet&gt;</returns>
        /// 
        public QueryResultSet DbQueryResultSet(string sqlQuery, params string[] bindArgs) {
            lock (this.m_connection) {
                return _DbQueryResultSet(sqlQuery, bindArgs);
            }
        }

        private QueryResultSet _DbQueryResultSet(string sqlQuery, params string[] bindArgs) {
            List<SimpleDB_BindArg> bind_args_list = new List<SimpleDB_BindArg>();

            foreach (string bind_arg in bindArgs) {
                bind_args_list.Add(new SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType.Text, bind_arg));
            }

            return _DbQueryResultSet(sqlQuery, bind_args_list);
        }

        private QueryResultSet _DbQueryResultSet(string sqlQuery, List<SimpleDB_BindArg> bindArgsList) {
            NpgsqlDataReader reader;
            QueryResultSet result = new QueryResultSet();

            this.m_lastError = null;

            if (!m_connection.getState().HasFlag(ConnectionState.Open)) {
                if (!m_connection.connect()) {
                    handleError("Query Error:\r\n " + sqlQuery + "\r\n\r\nFailed to connect to u");
                    return result;
                }
            }

            if (this.m_preparedQueries) {
                int param_pos = 0;

                foreach (SimpleDB_BindArg bind_arg in bindArgsList) {
                    sqlQuery = sqlQuery.Replace("{" + param_pos + "}", "@param" + param_pos);
                    param_pos++;
                }
            }

            try {
                NpgsqlCommand command = new NpgsqlCommand(sqlQuery, m_connection.getNpgSqlConnection());

                if (this.m_preparedQueries) {
                    int param_pos = 0;

                    foreach (SimpleDB_BindArg bind_arg in bindArgsList) {
                        if (bind_arg.Item == null) {
                            bind_arg.Item = DBNull.Value;
                        }

                        command.Parameters.AddWithValue("param" + param_pos, bind_arg.Type, bind_arg.Item);
                        param_pos++;
                    }
                }

                command.Prepare(); // Does this get re-used?... maybe do our own prepared management

                reader = command.ExecuteReader();
            }
            catch (Exception ex) {
                string bind_args_string = "";

                foreach (SimpleDB_BindArg bind_arg in bindArgsList) {
                    if (bind_arg.Item == null) {
                        bind_args_string += ",NULL";
                        continue;
                    }

                    bind_args_string += "\r\n" + bind_arg.Item.ToString();
                }

                if (bindArgsList.Count > 0) {
                    bind_args_string = bind_args_string.Substring(1); // Chop initial ,
                }

                handleError("Error running SQL:\r\n " + sqlQuery + "\r\n\r\nBind:\r\n" +  bind_args_string + "\r\n\r\n" + ex.Message + (ex.InnerException == null ? "" : ex.InnerException.Message));
                return result;
            }

            // for each row
            while (true) {
                Boolean reader_success = false;

                try {
                    reader_success = reader.Read();
                }
                catch (Exception ex) {
                    handleError("Query Exception:\r\n" + sqlQuery + "\r\n\r\n" + ex.Message);
                    reader.Close();
                    return result.Clear();
                }

                if (!reader_success) {
                    break;
                }

                QueryResultSetRecord r = new QueryResultSetRecord();

                // for each column within that row
                for (int i = 0; i < reader.FieldCount; i++) {
                    string column_name = reader.GetName(i);
                    Boolean column_null = reader.IsDBNull(i);

                    string column_value = column_null ? null : reader.GetValue(i).ToString();
                    r.Add(column_name, column_value);
                }

                result.Add(r);
            }

            reader.Close();
            result.SetQuerySuccess();

            return result;
        }

        [ObsoleteAttribute("Use DbSelect/DbSelectSingleRow instead")]
        public string buildDbQueryWithParams(string sqlQueryWithFormats, params string[] bindArgs) {
            return _buildDbQueryWithParams(sqlQueryWithFormats, bindArgs);
        }

        /// <summary>
        /// Backwards compatability with non-prepared queries... only use this internally to avoid warning about buildDbQueryWithParams being obsolete
        /// </summary>
        /// <param name="sqlQueryWithFormats"></param>
        /// <param name="bindArgs"></param>
        /// <returns></returns>
        private string _buildDbQueryWithParams(string sqlQueryWithFormats, params string[] bindArgs) {
            if (this.m_preparedQueries) {
                int param_pos   = 0;
                int replace_pos = 0;

                // We we can use ? anywhere without having to worry about position
                foreach (string bind_arg in bindArgs) {
                    replace_pos = Utils.StringReplaceFirstOccurance(ref sqlQueryWithFormats, "?", "{" +  param_pos++ + "}", replace_pos);
                }
                
                if (sqlQueryWithFormats.Contains("?")) {
                    handleError(new Exception("Not enough BIND args for query"), "Not enough BIND args for query");
                }

                return sqlQueryWithFormats;
            }

            for (int i = 0; i < bindArgs.Length; i++) {
                bindArgs[i] = this.escapeString(bindArgs[i]);
            }

            int arg_num = 0;

            /*
            // Replace all {x} with placeholder positions 
            Regex re = new Regex(@"\{\d\}");
            Match m = re.Match(sqlQueryWithFormats);

            while (m.Success) {
                sqlQueryWithFormats = Regex.Replace(sqlQueryWithFormats, @"\{\d\}", "?");
                m = re.Match(sqlQueryWithFormats);
            }
            */

            // Replace all ? with placeholder positions (other than \? which is a literal ?)
            Regex re = new Regex(@"([^\\?])\?");
            Match m = re.Match(sqlQueryWithFormats);

            while (m.Success) {
                sqlQueryWithFormats = Regex.Replace(sqlQueryWithFormats, @"[^\\?]\?", m.Groups[1].Value + "'{" + arg_num++ + "}'");
                m = re.Match(sqlQueryWithFormats);
            }

            // Replace \? with a regular ?
            sqlQueryWithFormats = Regex.Replace(sqlQueryWithFormats, @"\\\?", "?");

            string sql_query = String.Format(
              sqlQueryWithFormats,
              bindArgs
            );

            return sql_query;
        }

        [ObsoleteAttribute("Use DbSelect/DbSelectSingleRow instead")]
        public int dbQueryWithParams(string sqlQueryWithFormats, List<OrderedDictionary> resultData, params string[] bindArgs) {
            string sql_query = _buildDbQueryWithParams(sqlQueryWithFormats, bindArgs);
            QueryResultSet result = DbSelect(sql_query, bindArgs);

            ConvertQueryResultSet_To_ListOfOrderedDictionary(result, resultData);

            return (this.m_lastError == null) ? 1 : 0;
        }

        //////////////////////////////////////////
        // DbSelect
        //////////////////////////////////////////
        
        public QueryResultSet DbSelect(string sqlQueryWithFormats, params string[] bindArgs) {
            List<SimpleDB_BindArg> bind_args_list = new List<SimpleDB_BindArg>();
            string sql_query = _buildDbQueryWithParams(sqlQueryWithFormats, bindArgs);

            foreach (string bind_arg in bindArgs) {
                bind_args_list.Add(new SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType.Text, bind_arg));
            }

            return DbQueryResultSet(sql_query, bind_args_list);
        }

        // We're going to assume we're using prepared queries if we got passed a bindArgsList
        public QueryResultSet DbSelect(string sqlQueryWithFormats, List<SimpleDB_BindArg> bindArgsList) {
            return DbQueryResultSet(sqlQueryWithFormats, bindArgsList);
        }

        // We're going to assume we're using prepared queries if we got passed a bindArgsList
        public QueryResultSet DbSelectB(string sqlQueryWithFormats, params SimpleDB_BindArg[] bindArgs) {
            List<SimpleDB_BindArg> bind_args_list = new List<SimpleDB_BindArg>();

            foreach (SimpleDB_BindArg bind_arg in bindArgs) {
                bind_args_list.Add(bind_arg);
            }

            return DbQueryResultSet(sqlQueryWithFormats, bind_args_list);
        }

        /// <summary>
        /// Handy helper to query a db function with a specific number of parameters
        /// 
        /// WARNING:
        /// If multiple functions exist of this name but with different parameter types, use DbSelectFunctionB or use explicit casts
        /// For example:
        ///   Both defined, func_name(text,text) func_name(int,int)
        /// 
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="bindArgs"></param>
        /// <returns></returns>
        public QueryResultSet DbSelectFunction(string functionName, params string[] bindArgs) {
            string query = "SELECT " + functionName + "(";

            foreach (string bindArg in bindArgs) { query += "?,";  }

            query = query.TrimEnd(',');
            query += ")";

            return DbSelect(query, bindArgs);
        }

        // We're going to assume we're using prepared queries if we got passed a bindArgsList
        public QueryResultSet DbSelectFunctionB(string sqlQueryWithFormats, params SimpleDB_BindArg[] bindArgs) {
            List<SimpleDB_BindArg> bind_args_list = new List<SimpleDB_BindArg>();

            foreach (SimpleDB_BindArg bind_arg in bindArgs) {
                bind_args_list.Add(bind_arg);
            }

            return DbQueryResultSet(sqlQueryWithFormats, bind_args_list);
        }

        //////////////////////////////////////////
        // DbSelectSingleRow
        //////////////////////////////////////////

        public QueryResultSetRecord DbSelectSingleRow(string sqlQueryWithFormats, params string[] bindArgs) {
            QueryResultSet rs = DbSelect(sqlQueryWithFormats, bindArgs);

            if (rs.Count > 0) {
                return rs[0]; // Each item in a QueryResultSet is a QueryResultSetRecord... return the first one
            }

            // TODO: Fixme check for more than one row

            // Query successfully executed, but we have no results, return something
            return new QueryResultSetRecord();
        }

        public string DbSelectSingleValueString(string sqlQueryWithFormats, List<SimpleDB_BindArg> bindArgsList) {
            QueryResultSet rs = DbSelect(sqlQueryWithFormats, bindArgsList);

            // TODO: Fixme check for more than one row

            if (rs.Count > 0) {
                QueryResultSetRecord rsr = rs[0];
                ArrayList result_keys = rsr.KeysToArrayList();

                if (result_keys.Count > 1) {
                    throw new InvalidOperationException("Result returned more than one column: " + sqlQueryWithFormats);
                }

                if (result_keys.Count == 1) {
                    string db_column_returned = result_keys[0].ToString();

                    return rsr[db_column_returned];
                }

                // No Columns??
            }

            // Query successfully executed, but we have no results, return something
            return null;
        }

        public string DbSelectSingleValueString(string sqlQueryWithFormats, params string[] bindArgs) {
            List<SimpleDB_BindArg> bindArgsList = new List<SimpleDB_BindArg>();
            string sql_query = _buildDbQueryWithParams(sqlQueryWithFormats, bindArgs);

            foreach (string bind_arg in bindArgs) {
                bindArgsList.Add(new SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType.Text, bind_arg));
            }

            return DbSelectSingleValueString(sql_query, bindArgsList);
        }

        // TODO: bind placeholders eventually to be 'standard' ?,?,? placeholders and not {0} {1} {2}
        public JsonHashResult DbSelectJsonFunction(string functionName, List<SimpleDB_BindArg> bindArgsList) {
            if (!this.m_preparedQueries) {
                throw new Exception("DbHelper SetPrepared(true) must be used to use DbHelper.DbSelectJsonFunction()");
            }

            int param = 0;
            List<string> bind_place_holders = new List<string>();
            foreach (SimpleDB_BindArg function_param in bindArgsList) { bind_place_holders.Add("{" + param++ + "}"); }
            string bind_place_holders_string = String.Join(",", bind_place_holders.ToArray());

            // 
            string sql_query = String.Format("SELECT {0}({1}) as result", functionName, bind_place_holders_string);

            string json_result_string = DbSelectSingleValueString(sql_query, bindArgsList);

            JsonHash json_result;

            try {
                json_result = new JsonHash(json_result_string);
            }
            catch (Exception ex) {
                Exception e = ex; // Get rid of warning

                return new JsonHashResult { Success = false, Code = "INTERNAL_FAILURE", Reason = "Internal Failure.  API function did not return JSON" };
            }

            return new JsonHashResult {
                Success = json_result.GetBool("success"),
                Code    = json_result.GetString("code"),
                Reason  = json_result.GetString("reason"),
                Data    = json_result.GetHash("data")
            };
        }
        
        public JsonHashResult DbSelectJsonFunction(string functionName, params SimpleDB_BindArg[] bindArgs) {
            List<SimpleDB_BindArg> bind_args_list = new List<SimpleDB_BindArg>();

            foreach (SimpleDB_BindArg bind_param in bindArgs) {
                bind_args_list.Add(bind_param);
            }

            return DbSelectJsonFunction(functionName, bind_args_list);
        }

        public JsonHashResult DbSelectJsonFunction(string functionName, params string[] functionArgs) {
            List<SimpleDB_BindArg> bind_args_list = new List<SimpleDB_BindArg>();

            foreach (string bind_param in functionArgs) {
                bind_args_list.Add(new SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType.Text, bind_param));
            }

            return DbSelectJsonFunction(functionName, bind_args_list);
        }

        /// <summary>
        /// Execute a query and return a hashtable indexed by the specified hashTableKey
        /// NOTE: Make sure hashTableKey is a unique value across all rows
        /// 
        /// Example:
        ///   SELECT * FROM user_permissions
        ///    Returns:
        ///      user   permission1 permission2
        ///      A      yes         no
        ///      B      yes         no
        /// 
        ///    Assuming hashTableKey=group, This will result in the following hash table
        ///      A => (permission1 => yes, permission2 => no)
        ///      B => (permission1 => yes, permission2 => no)
        /// 
        ///    There must be no more than 1 instance of user: A
        /// 
        /// </summary>
        /// <param name="hashTableKey">Key to index the Hashtable on</param>
        /// <param name="sqlQueryWithFormats">Query</param>
        /// <param name="bindArgs">Parametrized query arguments</param>
        /// <returns>Hashtable</returns>
        public Hashtable DbSelectIntoHash(string hashTableKey, string sqlQueryWithFormats, params string[] bindArgs) {
            QueryResultSet result_set = DbSelect(sqlQueryWithFormats, bindArgs);
            Hashtable h = new Hashtable();

            // TODO: This doesn't handle having the column specified by hashTableKey not exist
            foreach (QueryResultSetRecord row in result_set) {
                if (h.ContainsKey(row[hashTableKey])) {
                    Console.Write("!!!! DbSelectIntoHash hashTableKey already exists: " + row[hashTableKey]);
                    continue;
                }

                h.Add(row[hashTableKey], row);
            }

            return h;
        }

        /// <summary>
        /// Execute a query and return a hashtable indexed by the specified hashTableKey
        /// This will create a List inside each hashtable element, supporting a resultset of multiple entries having the same hashTableKey
        /// 
        /// Example:
        ///    SELECT * FROM user_groups
        ///    Returns:
        ///      group   user
        ///      A       1
        ///      A       2
        ///      B       3
        ///      B       4
        /// 
        ///    Assuming hashTableKey=group, This will result in the following hash table
        ///      A => List(User 1, User 2)
        ///      B => List(User 3, User 4)
        /// 
        ///
        /// 
        ///    As opposed to using 'regular' DbSelectIntoHash, which will fail if there are multiple group 'A' entries.
        /// 
        /// </summary>
        /// <param name="hashTableKey"></param>
        /// <param name="sqlQueryWithFormats"></param>
        /// <param name="nestedHash"></param>
        /// <param name="bindArgs"></param>
        /// <returns>
        ///   Hashtable
        ///     KEY = QueryResultSet -> List<QueryResultSetRecord> (QueryResultSetRecord, QueryResultSetRecord, ...)
        /// </returns>

        public Hashtable DbSelectIntoHashQueryResultSet(string hashTableKey, string sqlQueryWithFormats, params string[] bindArgs) {
            QueryResultSet result_set = DbSelect(sqlQueryWithFormats, bindArgs);
            Hashtable h = new Hashtable();

            // TODO: This doesn't handle having the column specified by hashTableKey not exist
            foreach (QueryResultSetRecord row in result_set) {
                string index_value = row[hashTableKey];
                QueryResultSet hash_result_set;

                if (h.ContainsKey(index_value)) {
                    hash_result_set = (QueryResultSet) h[index_value]; 
                    hash_result_set.Add(row);
                    continue;
                }

                // Start a fresh QueryResultSet, using the current result
                hash_result_set = new QueryResultSet(new List<QueryResultSetRecord>() { row });
                h.Add(index_value, hash_result_set);
            }

            return h;
        }

        public Dictionary<string, QueryResultSet> DbSelectIntoDictQueryResultSet(string dictKey, string sqlQueryWithFormats, params string[] bindArgs) {
            QueryResultSet result_set = DbSelect(sqlQueryWithFormats, bindArgs);
            Dictionary<string, QueryResultSet> d = new Dictionary<string, QueryResultSet>();

            // TODO: This doesn't handle having the column specified by dictKey not exist
            foreach (QueryResultSetRecord row in result_set) {
                string index_value = row[dictKey];
                QueryResultSet d_result_set;

                if (d.ContainsKey(index_value)) {
                    d_result_set = (QueryResultSet) d[index_value];
                    d_result_set.Add(row);
                    continue;
                }

                // Start a fresh QueryResultSet, using the current result
                d_result_set = new QueryResultSet(new List<QueryResultSetRecord>() { row });
                d.Add(index_value, d_result_set);
            }

            return d;
        }


        // For backwards compatabilty
        // Convert this
        //   Hashtable<string, QueryResultSet>
        // to this:
        //   Dictionary<string, List<OrderedDictionary>>
        //
        public static Dictionary<string, List<OrderedDictionary>> ConvertHashQueryResultSet_To_DictionaryOfOrderedDictionary(Hashtable hashResultSet) {
          Dictionary<string, List<OrderedDictionary>> new_dictionary_list = new Dictionary<string,List<OrderedDictionary>>();

          foreach (string index_col in hashResultSet.Keys) {
            List<OrderedDictionary> list_ordered_dictionary = new List<OrderedDictionary>();
            QueryResultSet          result_set              = (QueryResultSet) hashResultSet[index_col];

            foreach (QueryResultSetRecord result_set_record in result_set) {
                // Each OrderedDictionary is a collection of columns from a single data-row 
                OrderedDictionary ordered_dictionary = new OrderedDictionary();

                foreach (string column_name in result_set_record.Keys()) {
                    ordered_dictionary.Add(column_name, result_set_record[column_name]);
                }

                list_ordered_dictionary.Add(ordered_dictionary);
            }

            new_dictionary_list.Add(index_col, list_ordered_dictionary);
          }

          return new_dictionary_list;
        }

        /// <summary>
        /// Convert
        ///   OrderedDictionary
        ///     to
        ///   QueryResultSetRecord
        /// </summary>
        /// <param name="ordered_dictionary"></param>
        /// <returns></returns>
        public static QueryResultSetRecord ConvertOrderedDictionary_To_QueryResultSetRecord(OrderedDictionary ordered_dictionary) {
            QueryResultSetRecord result_set_record = new QueryResultSetRecord();

            foreach (DictionaryEntry entry in ordered_dictionary) {
              result_set_record.Add((string) entry.Key, entry.Value.ToString());
            }

            return result_set_record;
        }

        // For Backwards compat -- old db functions returned List<OrderedDictionary> which is much harder to debug/explore in studio
        public static List<OrderedDictionary> ConvertQueryResultSet_To_ListOfOrderedDictionary(QueryResultSet queryResultSet) {
            List<OrderedDictionary> list_of_ordered_dictionary = new List<OrderedDictionary>();

            ConvertQueryResultSet_To_ListOfOrderedDictionary(queryResultSet, list_of_ordered_dictionary);

            return list_of_ordered_dictionary;
        }

        // Use a passed-in List<OrderedDictionary>
        public static List<OrderedDictionary> ConvertQueryResultSet_To_ListOfOrderedDictionary(QueryResultSet queryResultSet, List<OrderedDictionary> list_of_ordered_dictionary) {
            foreach (QueryResultSetRecord r in queryResultSet) {
                OrderedDictionary od = new OrderedDictionary();

                foreach (string column_name in r) {
                    od.Add(column_name, r[column_name]);
                }

                list_of_ordered_dictionary.Add(od);
            }

            return list_of_ordered_dictionary;
        }

        /// <summary>
        /// Clone
        ///   OrderedDictionary
        /// </summary>
        /// <param name="ordered_dictionary"></param>
        /// <returns></returns>
        public static OrderedDictionary CloneOrderedDictionary(OrderedDictionary ordered_dictionary) {
            OrderedDictionary clone_ordered_dictionary = new OrderedDictionary();

            foreach (DictionaryEntry entry in ordered_dictionary) {
                clone_ordered_dictionary.Add((string)entry.Key, entry.Value.ToString());
            }

            return clone_ordered_dictionary;
        }

        /// <summary>
        /// Convert
        ///   List<OrderedDictionary>
        ///     to
        ///   QueryResultSet
        /// </summary>
        /// <param name="list_ordered_dictionary"></param>
        /// <returns></returns>
        public static QueryResultSet ConvertListOrderedDictionary_To_QueryResultSet(List<OrderedDictionary> list_ordered_dictionary) {
            QueryResultSet result_set = new QueryResultSet();

            foreach (OrderedDictionary entry in list_ordered_dictionary) {
                QueryResultSetRecord result_set_record = ConvertOrderedDictionary_To_QueryResultSetRecord(entry);
                result_set.Add(result_set_record);
            }

            return result_set;
        }

        /// <summary>
        /// Clone
        ///   List<OrderedDictionary>
        /// </summary>
        /// <param name="list_ordered_dictionary"></param>
        /// <returns></returns>
        public static List<OrderedDictionary> CloneListOrderedDictionary(List<OrderedDictionary> list_ordered_dictionary) {
            List<OrderedDictionary> list_ordered_dict = new List<OrderedDictionary>();

            foreach (OrderedDictionary entry in list_ordered_dictionary) {
                OrderedDictionary ordered_dictionary = CloneOrderedDictionary(entry);
                list_ordered_dict.Add(ordered_dictionary);
            }

            return list_ordered_dict;
        }


        /// <summary>
        /// Convert 
        ///   Dictionary<string, List<OrderedDictionary>>
        ///     to
        ///   Dictionary<string, QueryResultSet>
        /// </summary>
        /// <param name="dict_string_list_ordered_dictionary"></param>
        /// <returns></returns>
        public static Dictionary<string, QueryResultSet> ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(Dictionary<string, List<OrderedDictionary>> dict_string_list_ordered_dictionary) {
            Dictionary<string, QueryResultSet> dictionary_string_result = new Dictionary<string, QueryResultSet>();

            foreach (KeyValuePair<string, List<OrderedDictionary>> entry in dict_string_list_ordered_dictionary) {
                string thingee = entry.Key;
                List<OrderedDictionary> list_ordered_dictionary = entry.Value;

                dictionary_string_result.Add(thingee, ConvertListOrderedDictionary_To_QueryResultSet(list_ordered_dictionary));
            }

            return dictionary_string_result;
        }

        /// <summary>
        /// Clone 
        ///   Dictionary<string, List<OrderedDictionary>>
        /// </summary>
        /// <param name="dict_string_list_ordered_dictionary"></param>
        /// <returns></returns>
        public static Dictionary<string, List<OrderedDictionary>> CloneDictionaryString_ListOrderedDictionary(Dictionary<string, List<OrderedDictionary>> dict_string_list_ordered_dictionary) {
            Dictionary<string, List<OrderedDictionary>> dictionary_clone_result = new Dictionary<string, List<OrderedDictionary>>();
            List<OrderedDictionary> list_ordered_dictionary = new List<OrderedDictionary>();

            foreach (KeyValuePair<string, List<OrderedDictionary>> entry in dict_string_list_ordered_dictionary) {
                string thingee = entry.Key;
                list_ordered_dictionary = entry.Value;

                dictionary_clone_result.Add(thingee, CloneListOrderedDictionary(list_ordered_dictionary));
            }

            return dictionary_clone_result;
        }

        public static Dictionary<string, QueryResultSet> Clone_DictionaryString_QueryResultSet(Dictionary<string, QueryResultSet> dict_string_query_result_set) {
            Dictionary<string, QueryResultSet> dictionary_clone_result = new Dictionary<string, QueryResultSet>();
            QueryResultSet query_result_set = new QueryResultSet();

            foreach (KeyValuePair<string, QueryResultSet> entry in dict_string_query_result_set) {
                string thingee = entry.Key;
                query_result_set = entry.Value;
                
                dictionary_clone_result.Add(thingee, query_result_set.Clone());
            }

            return dictionary_clone_result;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// END New Stuff for SimpleDB
        ///////////////////////////////////////////////////////////////////////////////////////////

        public bool isConnected() {
            return m_connection.isInitialized() && (m_connection.getState().HasFlag(ConnectionState.Open));
        }

        // TODO: Return TRUE value on success
        //
        // Return 1 on error
        // Return 0 on success
        [Obsolete("callSqlFunction is deprecated, please use DbSelectJsonFunction or DbSelect instead.")]
        public int callSqlFunction(string functionName, List<string> arguments, List<OrderedDictionary> resultData, Boolean useErrorCallBack = true) {
            string query = "SELECT " + functionName + "(";

            if (arguments != null) {
                if (arguments.Count > 0) {
                    query += "'" + arguments[0] + "'";

                    for (int i = 1; i < arguments.Count; i++) {
                        query += ", '" + arguments[i] + "'";
                    }
                }
            }

            query += ");";

            QueryResultSet result_set = DbSelect(query);
            resultData = DbHelper.ConvertQueryResultSet_To_ListOfOrderedDictionary(result_set);

            if (this.m_lastError != null) { 
                return 1;
            }

            // Success
            return 0;
        }

        public string escapeString(string str) {
            if (str == null) {
                return null;
            }

            return str.Replace("'", "''"); 
        }
    }

    public class SimpleDB_BindArg {
        public NpgsqlTypes.NpgsqlDbType Type;
        public object Item;

        public SimpleDB_BindArg(NpgsqlTypes.NpgsqlDbType dbDataType, object dbDataItem) {
            this.Type = dbDataType;
            this.Item = dbDataItem;
        }
    }

    /*

        //////////////////////////////////////////////////////////////////////////////////
        /// return the valid values for a database column that has a custom datatype.
        /// 
        /// such as t_record, t_yesno.
        /// @param tableName the view or table name. do not include the schema. ex: just use "v_trunks"
        /// @param columnName the name of the custom typed column to get valid values for. ex: "record"
        /// @return a list of valid values
        //        public List<string> getValidValuesForColumn(string tableName, string columnName)
        //        {
        //            List<string> result = new List<string>(0);

        //            string query = @"   
        //                SELECT e.enumlabel 
        //                    FROM pg_type t 
        //                    LEFT OUTER JOIN pg_attribute a ON a.atttypid = t.oid
        //                    LEFT OUTER JOIN pg_class c ON c.oid = a.attrelid
        //                    LEFT OUTER JOIN pg_enum e ON e.enumtypid=t.oid::oid
        //                    WHERE 
        //                        a.attname = '" + columnName + @"' AND
        //                        c.relname = '" + tableName + @"'
        //                            ORDER by e.oid
        //            ";
        //            int error = (int)Errors.None;

        //            bool haveConnection = m_connection.getState().HasFlag(ConnectionState.Open) || 0 == (error = m_connection.connect());

        //            if (haveConnection)
        //            {
        //                log.Info("SQL: " + query);
        //                DbHelperCommand command = new DbHelperCommand(query, m_connection);

        //                IDataReader reader = command.executeReader();

        //                int rowIdx = 0;
        //                while (reader.Read() == true)
        //                {
        //                    result.Add(reader.GetString(0));
        //                    rowIdx++;
        //                }

        //                reader.Close();
        //                //m_connection.Close();
        //            }
        //            //return error;

        //            return result;
        //        }

        /*

        public Hashtable getValidValuesForAllEnumColumnsInTable(string tableName) {
            return getValidValuesForAllEnumColumnsInTable(tableName, false);
        }


        //////////////////////////////////////////////////////////////////////////////////
        /// return the valid values for each database column in a single table that has a
        /// custom datatype.
        /// 
        /// such as t_record, t_yesno, etc. for non custom datatype columns, returns an 
        /// empty list (0 length), unless doNotReturnNonCustomColumns is set.
        /// @param tableName the view or table name. do not include the schema. ex: just 
        ///    use "v_trunks"
        /// @param doNotReturnNonCustomColumns if true, non custom columns are not 
        ///    included in the returned hash
        /// @return a Hashtable of lists of valid values; the hash key is the database column name
        public Hashtable getValidValuesForAllEnumColumnsInTable(string tableName, bool doNotReturnNonCustomColumns) {
            Hashtable result = new Hashtable();

            // take out schema name if present
            string tableNameWithoutSchema = tableName.Substring(tableName.LastIndexOf('.') + 1);

            string ignoreNonCustomColumnsQueryElement = doNotReturnNonCustomColumns ? "AND e.enumlabel <> 'NULL'" : "";

            string query = @"   
                SELECT a.attname, e.enumlabel 
                    FROM pg_type t 
                    LEFT OUTER JOIN pg_attribute a ON a.atttypid = t.oid
                    LEFT OUTER JOIN pg_class c ON c.oid = a.attrelid
                    LEFT OUTER JOIN pg_enum e ON e.enumtypid=t.oid::oid
                    WHERE 
                        c.relname = '" + tableNameWithoutSchema + @"' 
            			" + ignoreNonCustomColumnsQueryElement + @"
                            ORDER by a.attname,e.oid
            ";

            int error = 0; // (int)Errors.None;
            if (m_connection.getState().HasFlag(ConnectionState.Open) || (error = m_connection.connect()) == 0) {
                log.Info("SQL: " + query);
                DbHelperCommand command = new DbHelperCommand(query, m_connection);

                IDataReader reader = command.executeReader(); //(CommandBehavior.CloseConnection)

                int rowIdx = 0;
                string currentEnumColumnName = "";
                List<string> validValuesForCurrentEnumCoulmnName = new List<string>(0);
                while (reader.Read() == true) {
                    string enumColumnName = reader.GetString(0);
                    string validValue = null;
                    if (!reader.IsDBNull(1)) {
                        validValue = reader.GetString(1);
                    }
                    if (currentEnumColumnName != enumColumnName) {
                        if (rowIdx > 0) // this check is needed since the above case will alsways be true on the first row
                        {
                            // add our running list of valid enums to the hash, and reset the list
                            result.Add(currentEnumColumnName, validValuesForCurrentEnumCoulmnName);
                            validValuesForCurrentEnumCoulmnName = new List<string>(0);
                        }
                        currentEnumColumnName = enumColumnName;

                    }
                    if (validValue != null) {
                        validValuesForCurrentEnumCoulmnName.Add(validValue);
                    }
                    rowIdx++;
                }
                // add the last one
                result.Add(currentEnumColumnName, validValuesForCurrentEnumCoulmnName);

                reader.Close();
                //m_connection.Close();
            }
            else
                result = null;


            return result;
        }

        public int getTable(DataGridView grid, string table, Hashtable whereConditions) {
            return getTable(grid, table, whereConditions, null, null);
        }

        public int getTable(DataGridView grid, string table) {
            return getTable(grid, table, null, null, null);
        }

        public int getTable(DataGridView grid, string table, Hashtable whereConditions, Dictionary<string, string> visibleColumns) {
            return getTable(grid, table, whereConditions, visibleColumns, null, null, true);
            //return getTable(grid, table, whereConditions, visibleColumns, null);
        }

        public int getTable(DataGridView grid, string table, Hashtable whereConditions, Dictionary<string, string> visibleColumns, Hashtable specialControls, string keyColumnName) {
            return getTable(grid, table, whereConditions, visibleColumns, specialControls, keyColumnName, true);
            //return getTable(grid, table, whereConditions, visibleColumns, specialControls, keyColumnName, true);
        }

        public int getTable(DataGridView grid, string tableName, Hashtable whereConditions, Dictionary<string, string> visibleColumns, string keyColumnName) {
            return getTable(grid, tableName, whereConditions, visibleColumns, null, keyColumnName, true);
        }

        public int getTable(ref string[][] result, string tableName, Hashtable whereConditions, Dictionary<string, string> visibleColumns, string keyColumnName) {
            return getTable(ref result, tableName, whereConditions, visibleColumns, keyColumnName, false);
        }

        public int getTable(ref string[][] result, string tableName, Dictionary<string, string> visibleColumns, string keyColumnName) {
            return getTable(ref result, tableName, null, visibleColumns, keyColumnName, false);
        }

        */

    //private static string[][] breakUpHash(Hashtable h)
        //{
        //    string[][] result = new string[2][];
        //    bool sameName = false;

        //    int i = 0;
        //    foreach (string key in h.Keys)
        //    {
        //        //if (i == 0)
        //        //{
        //            sameName = h[key] is List<string>;// || h[key] is SortedList || h[key] is SortedList<string, Hashtable>;
        //            result[0] = new string[sameName ? 1 : h.Count];
        //            if (sameName)
        //            {
        //                List<string> l = (List<string>)h[key];
        //                result[1] = new string[l.Count];
        //                result[0][0] = key;
        //                for (int j = 0; j < l.Count; j++)
        //                    result[1][j] = l[j];
        //                break;
        //            }
        //            result[1] = new string[h.Count];
        //        //}

        //        result[0][i] = key;
        //        result[1][i++] = (string)h[key];
        //    }
        //    return result;
        //}

    /*

        [Obsolete("Use DbSelectIntoHash or DbSelectIntoHashQueryResultSet")]
        public int getTable(ref Dictionary<string, List<OrderedDictionary>> result, string tableName, Hashtable whereConditions, string keyColumnName) {
            ///@todo: don't use a datagridview here..
            DataGridView grid = new DataGridView();
            // need this to prevent "uncommited" extra row used for user entry
            grid.AllowUserToAddRows = false;
            //			string[][] wC = breakUpHash(whereCondition);
            int error = getTable(grid, tableName, whereConditions); // (grid, tableName, whereNames, whereValues, visibleColumns, null, keyColumnName, checkForCustomDbType);

            result = new Dictionary<string, List<OrderedDictionary>>(grid.Rows.Count);

            for (int i = 0; i < grid.Rows.Count; i++) {
                string keyValue = grid[keyColumnName, i].Value != null ? grid[keyColumnName, i].Value.ToString() : "";

                OrderedDictionary fields = new OrderedDictionary(grid.Columns.Count);
                foreach (DataGridViewColumn col in grid.Columns) {
                    string cellValue = grid[col.Name, i].Value != null ? grid[col.Name, i].Value.ToString() : "";
                    fields.Add(col.Name, cellValue);
                }
                if (!result.ContainsKey(keyValue)) {
                    List<OrderedDictionary> newList = new List<OrderedDictionary> { fields };
                    result.Add(keyValue, newList);
                }
                else
                    result[keyValue].Add(fields);
            }

            return error;
        }

        /////////////////
        /// Special version of getTable which returns results to a string[][] instead of a DataGridView.
        /// 
        /// Simply calls the DataGridView version and populates a new string[][] from the grid.
        public int getTable(ref string[][] result, string tableName, Hashtable whereConditions, Dictionary<string, string> visibleColumns, string keyColumnName, bool checkForCustomDbType) {
            int i, j;
            DataGridView grid = new DataGridView();
            // need this to prevent "uncommited" extra row used for user entry
            grid.AllowUserToAddRows = false;
            int error = getTable(grid, tableName, whereConditions, visibleColumns, null, keyColumnName, checkForCustomDbType);

            // alocate space in array
            // RowCollection is Null terminated, but we need an axtra entry for our header anyway, so no +1 or -1
            result = new string[grid.Rows.Count + 1][];

            i = 0;
            if (visibleColumns != null) // use visibleColumns if available to populate array to preserve order
            {
                result[0] = new string[visibleColumns.Keys.Count];
                // fill column names
                foreach (string key in visibleColumns.Keys)
                    result[0][i++] = visibleColumns[key];
                // fill data
                for (i = 0; i < grid.Rows.Count; i++) {
                    result[i + 1] = new string[visibleColumns.Keys.Count];
                    j = 0;
                    foreach (string key in visibleColumns.Keys)
                        result[i + 1][j++] = grid[key, i].Value != null ? grid[key, i].Value.ToString() : "";
                }
            }
            else // otherwise loop through the grid directly
            {
                result[0] = new string[grid.Columns.Count];
                // fill column names
                foreach (DataGridViewColumn col in grid.Columns)
                    result[0][i++] = col.Name;
                // fill data
                for (i = 0; i < grid.Rows.Count; i++) {
                    result[i + 1] = new string[grid.Columns.Count];
                    for (j = 0; j < grid.Columns.Count; j++)
                        result[i + 1][j] = grid[j, i].Value != null ? grid[j, i].Value.ToString() : "";
                }
            }


            return error;
        }

        //////////////////////////////////////////////////////////////////////////////////
        /// fills a DataGridView with results from an sql query.
        /// 
        /// @param grid ref to DataGridView to fill
        /// @param tableName table to query
        /// @param whereNames column names which must have specific value (whereValues) to 
        ///     match
        /// @param whereValues correspoding array of values to whereNames
        /// @param visibleColumnNames if supplied, make invisible all columns in grid which 
        ///     do not mach this list
        /// @param visibleColumnHeadings corresponding (to visibleColumnNames) grid column 
        ///     headings to show 
        /// @param keyColumnName if supplied stores copy of this db column in a hidden 
        ///     column at end of grid; allows for easy updates
        /// @return 0 on success, or an error code
        ///
        /// @todo onlyShowSpecifiedColumns == true may be the only case needed, if we find 
        ///     manually adding columns in gui is not needed (true)
        /// @todo look into removing eraseMeNow! hack. Not needed for 
        ///     onlyShowSpecifiedColumns == true?
        /// @todo break apart into 2 separate functions?: for handling when grid's columns 
        ///     are specified in 1. .NET UI Builder, and 2. as function arguments.
        public int getTable(DataGridView grid, string tableName, Hashtable whereConditions, Dictionary<string, string> visibleColumns, Hashtable specialControls, string keyColumnName, bool checkForCustomDbType) { 
            lock (m_connection) { 
                return _getTable(grid, tableName, whereConditions, visibleColumns, specialControls, keyColumnName, checkForCustomDbType);
            }
        }

        public int _getTable(DataGridView grid, string tableName, Hashtable whereConditions, Dictionary<string, string> visibleColumns, Hashtable specialControls, string keyColumnName, bool checkForCustomDbType) {
            int error = 0;
            int i;
            string keyColumnValue = "";
            bool isGridColumnsPreset = !(grid.Columns.Count == 0);

            grid.CancelEdit();

            /// check columns names and types
            if (!m_dbColumnInfoCache.ContainsKey(tableName))
                m_dbColumnInfoCache.Add(tableName, getValidValuesForAllEnumColumnsInTable(tableName));
            Hashtable columns = (Hashtable)m_dbColumnInfoCache[tableName];
            if (specialControls != null) {
                ICollection keys = specialControls.Keys;
                foreach (string key in keys) {
                    List<string> validValues = new List<string>(1);
                    Hashtable thisSpecialControl = (Hashtable)specialControls[key];
                    string controlType = (string)thisSpecialControl["ControlType"];
                    if (controlType == null || controlType == "ComboBox") {
                        string colName = thisSpecialControl.ContainsKey("UidName") ? (string)thisSpecialControl["UidName"] : key;
                        if (0 == getColumn(ref validValues, colName, (string)thisSpecialControl["TableName"])) {
                            if (columns.ContainsKey(key)) {
                                if (((List<string>)columns[key]).Count != 0)
                                    log.Warn("Ignoring DB Enum Type data for " + colName);
                                columns[key] = validValues;
                            }
                            else
                                log.Error("Unknown Column " + key);
                        }
                    }
                    else
                        throw new NotImplementedException("ControlType of " + controlType + " not supported.");
                }
            }

            // make appropriate columns in grid, if needed
            if (null != visibleColumns || !isGridColumnsPreset) {
                grid.Rows.Clear();

                // add columns
                grid.Columns.Clear();
                foreach (string key in columns.Keys) {
                    if (null != visibleColumns && visibleColumns.ContainsKey(key)) {
                        grid.Columns.Add(key, (string)visibleColumns[key]);
                    }
                    else {
                        // do we still need to add these once if visible columns is not specified? for cos form?
                        grid.Columns.Add(key, key);
                        if (!isGridColumnsPreset && visibleColumns == null) {
                            //log.Debug("should only be here if filling mcc");
                        }
                        else {
                            // comment out to show non-visible columns (all columns in db table)
                            grid.Columns[key].Visible = false;
                        }
                    }
                }

                if (null != visibleColumns) {

                    // order the columns to match our visibleColumns dictionary
                    string[] keys = new string[visibleColumns.Keys.Count];
                    visibleColumns.Keys.CopyTo(keys, 0);
                    for (i = 0; i < keys.Length; i++) {
                        grid.Columns[keys[i]].DisplayIndex = i;
                    }
                }
            }
            else if (isGridColumnsPreset) {
                grid.Rows.Clear();
            }

            // populate grid cells' data, making new rows in grid as needed
            if (m_connection.getState().HasFlag(ConnectionState.Open) || m_connection.connect()) {

                // Create an ODBC SQL command that will be executed below.
                string query = "SELECT * FROM " + tableName;
                tackOnWhereConditionV2(ref query, whereConditions);
                //if (whereNames != null && whereValues != null)
                //{
                //    if (whereNames.Length == 1 && whereValues.Length > 1)
                //        tackOnWhereCondition(ref query, ref whereNames[0], ref whereValues);
                //    else
                //        tackOnWhereCondition(ref query, ref whereNames, ref whereValues);
                //}

                log.Info("SQL: " + query);
                DbHelperCommand command = new DbHelperCommand(query, m_connection);

                // Execute the SQL command and return a reader for navigating the results.
                IDataReader reader = command.executeReader(); //(CommandBehavior.CloseConnection)

                // Process entire contents of the results, iterating
                // through each row and through each field of the row.
                int rowIdx = 0;

                //grid.Rows.Clear();

                // for each row
                while (reader.Read() == true) {
                    //while (rowIdx >= grid.Rows.Count) // this should really only happen 0 or 1 times
                    grid.Rows.Add();

                    // for each column within that row
                    for (i = 0; i < reader.FieldCount; i++) {
                        // save value if this column is the uid key column
                        string columnName = reader.GetName(i);
                        if (keyColumnName != null && keyColumnName == columnName && !reader.IsDBNull(i)) {
                            keyColumnValue = reader.GetValue(i).ToString();
                        }

                        // if this column has a custom db type, make it a combobox and populate the options
                        if (checkForCustomDbType && ((List<string>)columns[columnName]).Count > 0) {
                            DataGridViewComboBoxCell c = new DataGridViewComboBoxCell();
                            c.Items.AddRange(((List<string>)columns[columnName]).ToArray());
                            grid[columnName, rowIdx] = c;
                        }

                        // set value of cell
                        if (!reader.IsDBNull(i)) {
                            grid.Rows[rowIdx].Cells[columnName].Value = reader.GetValue(i).ToString();
                        }
                    }

                    if (null != visibleColumns && keyColumnName != null && keyColumnValue != null) {
                        // add hidden column with copy of unique key column
                        //                        if (i == grid.Columns.Count - 1) // todo: remove "- 1" part if eraseMeNow! hack is fixed
                        if (i == grid.Columns.Count) {
                            grid.Columns.Add("_hidden_" + keyColumnName, keyColumnName);
                            // comment out to see special hidden id column present only in datagridview
                            grid.Columns["_hidden_" + keyColumnName].Visible = false;
                        }
                        // populate new column
                        grid[grid.Columns.Count - 1, rowIdx].Value = keyColumnValue;
                    }

                    rowIdx++;
                }

                // make sure to create hidden id column if there is no data in db
                if (rowIdx == 0) {
                    // add hidden column with copy of unique key column
                    //                        if (i == grid.Columns.Count - 1) // todo: remove "- 1" part if eraseMeNow! hack is fixed
                    grid.Columns.Add("_hidden_" + keyColumnName, keyColumnName);
                    // comment out to see special hidden id column present only in datagridview
                    grid.Columns["_hidden_" + keyColumnName].Visible = false;
                }

                // Close the reader and connection (commands are not closed).
                reader.Close();
                //m_connection.Close();
            }

            return error;
        }

                //////////////////////////////////////////////////////////////////////////////////
        /// opens the connection to the database.
        ///
        /// checks to see if it isn't already open. catches and handles an exception.
        /// @param connection connection object to open
        /// @return 0 on success, or an error code
        /// @todo add global error handling
        //private int connect(OdbcConnection connection)
        //{
        //    int result = 0;
        //    try 
        //    {
        //        if (connection.State != ConnectionState.Closed)
        //        {
        //            connection.Close();
        //        }
        //        log.Info("Making SQL connection to " + connection.Database + " with string: " + connection.ConnectionString);
        //        connection.Open();
        //    }
        //    catch(Exception ex)
        //    {
        //        result = (int)Errors.Connection;
        //        //MessageBox.Show(ex.ToString());
        //        log.Error("Connection to SQL failed: " + ex.ToString());
        //    }
        //    return result;
        //}

        public int getColumn(ref List<string> result, string columnName, string tableName) {
            return getColumn(ref result, columnName, tableName, (string[])null, null, null);
        }

        public int getColumn(ref List<string> result, string columnName, string tableName, string whereName, string whereValue) {
            return getColumn(ref result, columnName, tableName, new string[] { whereName }, new string[] { whereValue }, null);
        }

        public int getColumn(ref List<string> result, string columnName, string tableName, string[] whereNames, string[] whereValues) {
            return getColumn(ref result, columnName, tableName, whereNames, whereValues, null);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        /// fills a List of strings with values from a single table column in the database.
        ///
        /// @param result ref to List of strings to create
        /// @param columnName name of the database column to get
        /// @param tableName name of the database table to get from
        /// @param whereNames column names which must have specific value (whereValues) to match
        /// @param whereValues correspoding array of values to whereNames
        /// @return 0 on success or an error code
        public int getColumn(ref List<string> result, string columnName, string tableName, string[] whereNames, string[] whereValues, string orderBy) {
            int error = 0;// (int)Errors.None;

            if (m_connection.getState().HasFlag(ConnectionState.Open) || m_connection.connect()) {
                string query = "SELECT " + columnName + " FROM " + tableName;
                tackOnWhereCondition(ref query, ref whereNames, ref whereValues);
                if (orderBy != null)
                    query += " ORDER BY " + orderBy;
                log.Info("SQL: " + query);
                DbHelperCommand command = new DbHelperCommand(query, m_connection);

                IDataReader reader = command.executeReader(); //(CommandBehavior.CloseConnection)

                int rowIdx = 0;
                while (reader.Read() == true) {
                    //result.Add(reader.GetString(0));
                    result.Add(reader.GetValue(0).ToString());
                    rowIdx++;
                }

                reader.Close();
                //m_connection.Close();
            }
            return error;
        }

        public int updateDbField(string columnName, string newValue, string tableName, string[] whereNames, string[] whereValues, ref string errorMessage) {
            return updateDbField(columnName, newValue, tableName, whereNames, whereValues, ref errorMessage, true);
        }

        public int updateDbField(string columnName, string newValue, string tableName, string whereName, string whereValue, ref string errorMessage) {
            return updateDbField(columnName, newValue, tableName, new string[] { whereName }, new string[] { whereValue }, ref errorMessage, true);
        }

        public int updateDbField(string columnName, string newValue, string tableName, string whereName, string whereValue, ref string errorMessage, bool quoteNewValue) {
            return updateDbField(columnName, newValue, tableName, new string[] { whereName }, new string[] { whereValue }, ref errorMessage, quoteNewValue);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        /// updates a table column (0 or more rows) in the database with a supplied value.
        ///
        /// @param columnName name of the database column to update
        /// @param newValue new value to put into the database column
        /// @param tableName name of the database table to update
        /// @param whereNames column names which must have specific value (whereValues) to match
        /// @param whereValues correspoding array of values to whereNames
        /// @param errorMessage ref to more detailed error message that may be set
        /// @param quoteNewValue whether or not to put single quotes around the new value. needed?
        /// @return 0 on success or an error code
        /// @todo change error codes to match with errors in Program.cs (add 2 and 3 to MainForm?)
        public int updateDbField(string columnName, string newValue, string tableName, string[] whereNames, string[] whereValues, ref string errorMessage, bool quoteNewValue) {
            int error = 0;

            if (null == whereNames || null == whereValues || whereValues.Length != whereNames.Length || whereNames.Length <= 0 || whereValues.Length <= 0) {
                error = 2;
            }
            else {
                //OdbcConnection connection = new OdbcConnection("DSN=PostgreSQL30");
                dbEncode(ref newValue);
                string query = "";
                query += "UPDATE " + tableName + " SET \"" + columnName;
                if (quoteNewValue) {
                    query += "\" = '" + newValue + "'";
                }
                else {
                    query += "\" = " + newValue;
                }
                tackOnWhereCondition(ref query, ref whereNames, ref whereValues);

                try {
                    DbHelperCommand command;

                    if (!m_connection.getState().HasFlag(ConnectionState.Open)) {
                        error = m_connection.connect();
                    }
                    if (error == 0) {
                        log.Info("SQL: " + query);
                        command = new DbHelperCommand(query, m_connection);
                        command.executeNonQuery();
                    }
                }
                catch (Exception ex) {
                    if ((m_options & OPTION_DEBUG) > 0) {
                        errorMessage = ex.ToString();
                    }
                    error = 3;
                }
                finally {
                    //m_connection.Close();
                }
            }
            return error;
        }

        public int getSingleFromDb(ref string target, string columnName, string tableName) {
            return getSingleFromDb(ref target, columnName, tableName, new string[] { }, new string[] { }, false);
        }

        public int getSingleFromDb(ref string target, string columnName, string tableName, bool doNotQuote) {
            return getSingleFromDb(ref target, columnName, tableName, new string[] { }, new string[] { }, doNotQuote);
        }

        public int getSingleFromDb(ref string target, string columnName, string tableName, string whereName, string whereValue) {
            return getSingleFromDb(ref target, columnName, tableName, new string[] { whereName }, new string[] { whereValue }, false);
        }

        public int getSingleFromDb(ref string target, string columnName, string tableName, string whereName, string whereValue, bool doNotQuote) {
            return getSingleFromDb(ref target, columnName, tableName, new string[] { whereName }, new string[] { whereValue }, doNotQuote);
        }

        public int getSingleFromDb(ref string target, string columnName, string tableName, string[] whereNames, string[] whereValues) {
            return getSingleFromDb(ref target, columnName, tableName, whereNames, whereValues, false);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        /// get a single value from the database.
        ///
        /// @param target put the result into this string
        /// @param columnName name of the database column to get
        /// @param tableName name of the database table to get
        /// @param whereNames column names which must have specific value (whereValues) to match
        /// @param whereValues correspoding array of values to whereNames
        /// @param doNotQuote if true, don't put quotes around columnName in sql query (useful 
        ///     for passing in SQL such as MIN(), MAX(), COUNT(*), etc.
        /// @return 0 on success or an error code
        /// @todo change error codes to match with errors in Program.cs (add 2 and 3 to MainForm?)
        public int getSingleFromDb(ref string target, string columnName, string tableName, string[] whereNames, string[] whereValues, bool doNotQuote) {
            int error = 0;

            if (null == whereNames || null == whereValues || whereValues.Length != whereNames.Length || whereNames.Length < 0 || whereValues.Length < 0) {
                error = 2;
                return error;
            }

            if (!m_connection.getState().HasFlag(ConnectionState.Open)) {
                error = m_connection.connect();
            }

            string query;
            if (doNotQuote) {
                query = "SELECT " + columnName + " FROM " + tableName;
            }
            else {
                query = "SELECT \"" + columnName + "\" FROM " + tableName;

            }
            tackOnWhereCondition(ref query, ref whereNames, ref whereValues);

            QueryResultSetRecord r = DbSelectSingleRow(query);
            target = r[columnName];

            return error;
        }

        public int getMultipleFromDb(string[] target, string[] columnNames, string tableName) {
            return getMultipleFromDb(target, columnNames, tableName, (string[])null, null, false);
        }

        public int getMultipleFromDb(string[] target, string[] columnNames, string tableName, string whereName, string whereValue) {
            return getMultipleFromDb(target, columnNames, tableName, new string[] { whereName }, new string[] { whereValue }, false);
        }

        public int getMultipleFromDb(string[] target, string[] columnNames, string tableName, string whereName, string whereValue, bool doNotQuote) {
            return getMultipleFromDb(target, columnNames, tableName, new string[] { whereName }, new string[] { whereValue }, doNotQuote);
        }

        public int getMultipleFromDb(string[] target, string[] columnNames, string tableName, string[] whereNames, string[] whereValues) {
            return getMultipleFromDb(target, columnNames, tableName, whereNames, whereValues, false);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        /// get a multiple values  from the database.
        /// 
        /// Just like getSingleFromDb except gets more than one value (one for each 
        /// columnNames[] supplied, or all values if columnNames is null)
        /// @param target put the result into this string[]
        /// @param columnNames[] names of the database columns to get; if null, then get all 
        ///     columns (SELECT *)
        /// @param tableName name of the database table to get
        /// @param whereNames column names which must have specific value (whereValues) to match
        /// @param whereValues correspoding array of values to whereNames
        /// @param doNotQuote if true, don't put quotes around columnNames in sql query (useful 
        ///     for passing in SQL such as MIN(), MAX(), COUNT(*), etc.
        /// @return 0 on success or an error code
        /// @todo change error codes to match with errors in Program.cs (add 2 and 3 to MainForm?)
        /// @todo make doNotQuote an array/per columnNames[] option?
        public int getMultipleFromDb(string[] target, string[] columnNames, string tableName, string[] whereNames, string[] whereValues, bool doNotQuote) {
            int error = 0;

            if (whereNames != null && whereValues != null && (whereValues.Length != whereNames.Length || whereNames.Length < 0 || whereValues.Length < 0)) {
                error = 2;
            }
            else {
                if (!m_connection.getState().HasFlag(ConnectionState.Open)) {
                    error = m_connection.connect();
                }

                string query = "SELECT ";
                if (columnNames == null || columnNames.Length == 0) {
                    query += "*";
                }
                else {
                    // first one
                    if (!doNotQuote) {
                        query += "\"";
                    }
                    query += columnNames[0];
                    if (!doNotQuote) {
                        query += "\",\"";
                    }
                    else {
                        query += ",";
                    }

                    //rest of them
                    if (doNotQuote) {
                        for (int i = 1; i < columnNames.Length; i++) {
                            query += columnNames[i] + ",";
                        }
                    }
                    else {
                        for (int i = 1; i < columnNames.Length; i++) {
                            query += columnNames[i] + "\",\"";
                        }
                    }

                    // take off the , or ," off the end since we have no more
                    query = query.Substring(0, query.Length - (doNotQuote ? 1 : 2));

                }

                query += " FROM " + tableName;

                tackOnWhereCondition(ref query, ref whereNames, ref whereValues);

                if (error == 0) {
                    try {
                        log.Info("SQL: " + query);
                        DbHelperCommand command = new DbHelperCommand(query, m_connection);
                        IDataReader reader = command.executeReader(); //(CommandBehavior.CloseConnection)

                        try {
                            bool dataFound = false;
                            // todo: handle none found case; test below check
                            if (reader.Read() && reader.FieldCount > 0) {
                                for (int i = 0; i < reader.FieldCount; i++) {
                                    if (reader.IsDBNull(i)) {
                                        //error = 4;
                                        //log.Warn("DBNull found: " + query);
                                    }
                                    else if (reader.GetFieldType(i).UnderlyingSystemType.GetType() != Type.GetType("System.String")) {
                                        /// @todo above check bad? always true? System.RuntimeType?
                                        dataFound = true;
                                        string name = reader.GetName(i).ToString();
                                        string value = reader.GetValue(i).ToString();
                                        //if (name != columnNames[i])
                                        //{
                                        //    MessageBox.Show("uh-oh");
                                        //}
                                        target[i] = value;
                                    }
                                    else /// @todo do we ever go into this??
									{
                                        dataFound = true;
                                        target[i] = reader.GetString(i);
                                    }
                                }
                            }
                            else {
                                ///@todo uncomment below but test if it breaks anything..
                                //error = 1;
                                log.Warn("Nothing found: " + query);
                            }

                            if (null == target || !dataFound) {
                                error = 1;
                                log.Warn("Nothing found: " + query);
                            }

                        }
                        finally {
                            reader.Close();
                        }
                        //m_connection.Close();
                    }
                    catch (Exception ex) {
                        log.Error("Connection Lost: " + ex);
                        error = 3;
                    }
                }
            }
            return error;
        }

        /////////////////////////////////////////////////////////////////////////////////////
        /// inserts new entry into specified database table.
        ///
        /// catches and tries to handle some exceptions.
        /// @param tableName name of the database table to update
        /// @param columnNames names of the database columns for which we are providing values in newValues
        /// @param newValues corresponding (to columnNames) values for new entry
        /// @param errorMessage ref to more detailed error message that may be set
        /// @return 0 on success or an error code
        /// @todo test exceptions
        /// @todo change error codes to match with errors in Program.cs (add 2, 5, and 3 to MainForm?)
        public int insertIntoDb(string tableName, string[] columnNames, string[] newValues, ref string errorMessage) {
            int error = 0;

            string query = "INSERT INTO " + tableName + " (";
            query += columnNames[0];
            for (int i = 1; i < columnNames.Length; i++) {
                query += ", " + columnNames[i];
            }
            dbEncode(ref newValues[0]);
            query += " ) VALUES ( '" + newValues[0] + "'";
            for (int i = 1; i < newValues.Length; i++) {
                dbEncode(ref newValues[i]);
                query += ", '" + newValues[i] + "'";
            }
            query += " )";

            try {
                DbHelperCommand command;

                if (!m_connection.getState().HasFlag(ConnectionState.Open)) {
                    error = m_connection.connect();
                }

                if (error == 0) {
                    try {
                        log.Info("SQL: " + query);
                        command = new DbHelperCommand(query, m_connection);
                        command.executeNonQuery();
                    }
                    catch (Exception ex) {
                        log.Error("Connection Lost: " + ex);
                        error = 3;
                    }

                }
            }
            catch (Exception ex) {
                if (ex.Message.StartsWith("ERROR [23505]")) {
                    errorMessage = "Please check the hilighted row for a duplicate value.";
                }
                else if (ex.Message.Contains("duplicate key value violates unique constraint")) {
                    errorMessage = "Duplicate unique key value. Database insert query failed.";
                }
                else if ((m_options & OPTION_DEBUG) > 0) {
                    errorMessage = ex.ToString();
                }
                error = 5;
                log.Warn(errorMessage);
                log.Debug("ex: " + ex.Message);
            }
            finally {
                //m_connection.Close();
            }
            return error;
        }
        */

    /*
        public int deleteFromDb(string tableName, string whereName, string whereValue, bool any) {
            return deleteFromDb(tableName, new string[] { whereName }, new string[] { whereValue }, any, false);
        }

        public int deleteFromDb(string tableName, string whereName, string whereValue) {
            return deleteFromDb(tableName, new string[] { whereName }, new string[] { whereValue }, false, false);
        }

        public int deleteFromDb(string tableName, string[] whereNames, string[] whereValues) {
            return deleteFromDb(tableName, whereNames, whereValues, false, false);
        }
        */

    /*

        /////////////////////////////////////////////////////////////////////////////////////
        /// delete entry(s) from database table.
        ///
        /// @param tableName name of the database table to delete from
        /// @param whereNames column names which must have specific value (whereValues) to match
        /// @param whereValues correspoding array of values to whereNames
        /// @param anyUnused unused param @todo delete unused param anyUnused?..
        /// @param sameName flag to determine if we should use In where condition
        /// @return 0 or error code
        public int deleteFromDb(string tableName, string[] whereNames, string[] whereValues, bool anyUnused, bool sameName) {
            int error = 0;

            if (!m_connection.getState().HasFlag(ConnectionState.Open)) {
                error = m_connection.connect();
            }

            string query = "DELETE FROM " + tableName;

            if (sameName)
                tackOnWhereCondition(ref query, ref whereNames[0], ref whereValues);
            else
                tackOnWhereCondition(ref query, ref whereNames, ref whereValues);

            if (error == 0) {
                try {
                    log.Info("SQL: " + query);
                    DbHelperCommand command = new DbHelperCommand(query, m_connection);
                    IDataReader reader = command.executeReader(); // (CommandBehavior.CloseConnection)
                    //MessageBox.Show(query);

                    try {
                        // any additional code needed should go here. (such as reading from the reader)
                    }
                    finally {
                        reader.Close();
                        //m_connection.Close();
                    }
                }
                catch (Exception ex) {
                    error = 6;
                    log.Error("Error Deleting: " + ex.Message + (ex.InnerException == null ? "" : ex.InnerException.Message));
                }
            }
            return error;
        }

        /////////////////////////////////////////////////////////////////////////////////////
        /// delegate description for callback used if unique key was changed.
        ///
        /// @param selectedExtensionSuggestion the new selected value for the unique key 
        ///        selection control
        /// @todo consider moving the methods below into a new UI helper class?
        public delegate void refreshCallbackType(string selectedExtensionSuggestion);

        public void addUpdateEventsToControl(Control targetControl, Control uniqueKeyValueHolderControl, string uniqueKeyName, string updateTableName, string updateColumnName) {
            addUpdateEventsToControl(targetControl, uniqueKeyValueHolderControl, uniqueKeyName, updateTableName, updateColumnName, null, null);
        }

        public void addUpdateEventsToControl(Control targetControl, Control uniqueKeyValueHolderControl, string uniqueKeyName, string updateTableName, string updateColumnName, List<string> useThisListOfCorrespondingTargetControlIds) {
            addUpdateEventsToControl(targetControl, uniqueKeyValueHolderControl, uniqueKeyName, updateTableName, updateColumnName, useThisListOfCorrespondingTargetControlIds, null);
        }

        public void addUpdateEventsToControl(Control targetControl, Control uniqueKeyValueHolderControl, string uniqueKeyName, string updateTableName, string updateColumnName, refreshCallbackType uniqueKeyValueHolderControlCallback) {
            addUpdateEventsToControl(targetControl, uniqueKeyValueHolderControl, uniqueKeyName, updateTableName, updateColumnName, null, uniqueKeyValueHolderControlCallback);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        /// add events to a control which launch database actions.
        ///
        /// TextBox and ComboBox Controls are supported. Enter Focus, Validating, and 
        /// Validated events are supported. An anonymous delegate is created and added to the 
        /// Control.
        ///
        /// @param targetControl the Control to add an event to
        /// @param uniqueKeyValueHolderControl another control. check it for the currently 
        ///        selected unique key value.
        /// @param uniqueKeyName name of the unique key column in the database
        /// @param updateTableName name of database table to update on new input
        /// @param updateColumnName name of database column to update on new input
        /// @param useThisListOfCorrespondingTargetControlIds List of strings of ids which 
        ///        correspond to the list of names in a ComboBox targetControl. We pass this 
        ///        in so we can look up the id of the selected item..
        /// @param uniqueKeyValueHolderControlCallback function to callback to pass back the 
        ///        new selected item in case this is a unique key selection control
        /// @todo consider moving the methods below into a new UI helper class?
        public void addUpdateEventsToControl(Control targetControl, Control uniqueKeyValueHolderControl, string uniqueKeyName, string updateTableName, string updateColumnName, List<string> useThisListOfCorrespondingTargetControlIds, refreshCallbackType uniqueKeyValueHolderControlCallback) {
            if (targetControl is TextBox || targetControl is ComboBox) {
                // enter focus
                targetControl.Enter += new System.EventHandler(
                    /////////////////////////////////////////////////////////////////////////////////////
                    /// Enter focus inline delegate function: handle entering focus on a Control. 
                    /// 
                    /// Save the Control's current value. This method is an anonymous delegate. 
                    ///
                    /// Param: sender the Control that is Entering focus
                    /// Param: e EventArgs of the Enter focus event
                    delegate (object sender, EventArgs e) {
                        m_previousValue = ((Control)sender).Text;
                    }
                );
                // validating
                targetControl.Validating += new System.ComponentModel.CancelEventHandler(
                    /////////////////////////////////////////////////////////////////////////////////////
                    /// Validating inline delegate function: handle Validating a new value set in a 
                    /// Control.
                    /// 
                    /// Insert the newly entered value into the database. If the insert fails, cancel the 
                    /// Validating event and give user an error message. This method is an anonymous 
                    /// delegate. 
                    ///
                    /// Param: sender the Control that is Validating
                    /// Param: e CancelEventArgs of the Validating event
                    delegate (object sender, CancelEventArgs e) {
                        string newValue = ((Control)sender).Text;
                        if (useThisListOfCorrespondingTargetControlIds != null
                            && targetControl is ComboBox
                            && useThisListOfCorrespondingTargetControlIds.Count == ((ComboBox)targetControl).Items.Count) {
                            newValue = useThisListOfCorrespondingTargetControlIds[((ComboBox)targetControl).SelectedIndex];
                        }

                        string error = "";
                        string uniqueKeyValue = ((ComboBox)uniqueKeyValueHolderControl).SelectedItem != null ? ((ComboBox)uniqueKeyValueHolderControl).SelectedItem.ToString() : null;

                        if (newValue != m_previousValue && uniqueKeyValue != null) {
                            if (0 != updateDbField(updateColumnName, newValue, updateTableName, uniqueKeyName, uniqueKeyValue, ref error)) {
                                e.Cancel = true;
                                ((Control)sender).Text = m_previousValue;
                                handleError(error);
                            }
                        }
                    }
                );
                // validated
                targetControl.Validated += new System.EventHandler(
                    /////////////////////////////////////////////////////////////////////////////////////
                    /// Validated inline delegate function: handle Validated event on a Control.
                    /// 
                    /// if we made a change, update the previous value to the new value. Also, if the 
                    /// field we updated is a unique key field, make a callback to let the Control's
                    /// Super form reload what it needs to. This method is an anonymous delegate.
                    ///
                    /// Param: sender the Control that is Validated
                    /// Param: e EventArgs of the Validated event
                    delegate (object sender, EventArgs e) {
                        string newValue = ((Control)sender).Text;
                        if (newValue != m_previousValue) {
                            if (uniqueKeyName == updateColumnName && uniqueKeyValueHolderControlCallback != null) {
                                uniqueKeyValueHolderControlCallback(newValue);
                            }
                            m_previousValue = newValue;
                        }
                    }
                );
            }
        } // end void addUpdateEventsToControl()

        public string getNextAvailableUniqueId(string tableName, string columnName, string startValueKeyword) {
            return getNextAvailableUniqueId(tableName, columnName, startValueKeyword, null);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        /// find the next available (numerically with an optional prefix) unique value for a 
        /// database table's column.
        ///
        /// same as other version of getNextAvailableUniqueId, except takes startValueKeyword
        /// instead of startValue
        /// 
        /// @param tableName the database table to find an available unique value in
        /// @param columnName the database column to find an available unique value in
        /// @param startValueKeyword min or max; wether to start with the lowest or highest 
        ///        value currently in the column
        /// @param prefix if supplied, will maintain this prefix and increment a numeric 
        ///        value in the next word
        /// @return the next available unique value
        /// @todo since this uses sql min and max, it may return undesirable results if the 
        ///       keys are in a character type column
        public string getNextAvailableUniqueId(string tableName, string columnName, string startValueKeyword, string prefix) {
            string returnValue = startValueKeyword;
            startValueKeyword = startValueKeyword.ToLower();
            if (prefix == null) {
                switch (startValueKeyword) {
                    case "min":
                        getSingleFromDb(ref returnValue, "MIN(" + columnName + ")", tableName, true);
                        break;
                    case "max":
                        getSingleFromDb(ref returnValue, "MAX(" + columnName + ")", tableName, true);
                        break;
                    default:
                        break;
                }
            }
            else {
                switch (startValueKeyword) {
                    case "min":
                        if (0 < getSingleFromDb(ref returnValue, "MIN(" + columnName + ")", tableName, columnName, prefix + '%', true)) {
                            returnValue = prefix + "0";
                        }
                        break;
                    case "max":
                        if (0 < getSingleFromDb(ref returnValue, "MAX(" + columnName + ")", tableName, columnName, prefix + '%', true)) {
                            returnValue = prefix + "0";
                        }
                        break;
                    default:
                        break;
                }
            }

            try {
                if (prefix == null) {
                    // cases: int, ends in int, no int
                    // Truncates data to passed in size. If new size is > 3, changes last 3 chars to '?'
                    Match m = Regex.Match(returnValue, @"(.*?[^\d])(\d+)$");
                    if (m.Success) // ends in a number
                    {
                        returnValue = getNextAvailableUniqueId(tableName, columnName, Int16.Parse(m.Groups[2].Value), m.Groups[1].Value);
                    }
                    else if (Regex.IsMatch(returnValue, @"^\d+$")) // is a number
                    {
                        returnValue = getNextAvailableUniqueId(tableName, columnName, Int16.Parse(returnValue));
                    }
                    else // does not contain a number
                    {
                        string separator = "-";
                        if (returnValue.Contains(" ")) {
                            separator = " ";
                        }
                        returnValue = getNextAvailableUniqueId(tableName, columnName, 0, returnValue + separator);
                    }
                }
                else {
                    int index = returnValue.LastIndexOf('-');
                    if (index < 0) {
                        index = returnValue.LastIndexOf(' ');
                    }
                    int num = Int16.Parse(returnValue.Substring(index + 1));
                    returnValue = returnValue.Substring(0, index + 1);
                    returnValue = getNextAvailableUniqueId(tableName, columnName, num, returnValue);
                }
            }
            catch {
                if (startValueKeyword == "min") {
                    returnValue = getNextAvailableUniqueId(tableName, columnName, 1);
                }
                else {
                    returnValue = getNextAvailableUniqueId(tableName, columnName, "min");
                }
            }
            return returnValue;
        }

        private string getNextAvailableUniqueId(string tableName, string columnName, int startValue) {
            return getNextAvailableUniqueId(tableName, columnName, startValue, null);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        /// find the next available (numerically with an optional prefix) unique value for a 
        /// database table's column.
        ///
        /// same as other version of getNextAvailableUniqueId, except takes startValue 
        /// instead of startValueKeyword
        /// 
        /// @param tableName the database table to find an available unique value in
        /// @param columnName the database column to find an available unique value in
        /// @param startValue start with this numeric value
        /// @param prefix if supplied, will maintain this prefix and increment a numeric 
        ///        value in the next word
        /// @return the next available unique value
        /// @todo since this uses sql min and max, it may return undesirable results if the 
        ///       keys are in a character type column
        private string getNextAvailableUniqueId(string tableName, string columnName, int startValue, string prefix) {
            string result = "";
            int id = startValue;
            id++;

            if (prefix == null) {
                prefix = "";
            }

            List<string> values;
            values = new List<string>(20);
            getColumn(ref values, columnName, tableName);

            while (values.Contains(prefix + id.ToString()))
                id++;

            result = prefix + id.ToString();

            return result;
        }

        public void reactToGridUpdate(DataGridView grid, int rowIndex, int columnIndex, string tableName) {
            reactToGridUpdate(grid, rowIndex, columnIndex, tableName, null, null);
        }

        public void reactToGridUpdate(DataGridView grid, int rowIndex, int columnIndex, string tableName, string keyColumnName) {
            reactToGridUpdate(grid, rowIndex, columnIndex, tableName, keyColumnName, null);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        /// updates database to reflect a user change made in a DataGridView.
        /// 
        /// handles a value change (update datatbase) or a new row creation (insert into 
        /// database).
        /// @param grid the DataGridView that was changed
        /// @param rowIndex the index of the row in the grid which was changed
        /// @param columnIndex the index of the column in the grid which was changed
        /// @param tableName table to update or insert into
        /// @param keyColumnName the name of the column in the table used as a unique key
        /// @param additionalInsertNamesAndValues list of additional column names and values to insert into table
        /// @todo better error handling (use global error call, or return error to caller)
        public void reactToGridUpdate(DataGridView grid, int rowIndex, int columnIndex, string tableName, string keyColumnName, Hashtable additionalInsertNamesAndValues) {
            string errorMessage = "";
            int columnCount = grid.Columns.Count;
            int visibleColumnCount = 0;
            int[] visibleColumnIndexes = new int[0];
            string[] visibleDbColumnNames = new string[0];
            string[] visibleDbColumnValues = new string[0];
            string keyValue = "";

            if (grid is TweakedDataGridView) {
                if (grid[columnIndex, rowIndex].Value == null) {
                    grid[columnIndex, rowIndex].Value = ((TweakedDataGridView)grid).PreviousValue;
                }
                else {
                    keyValue = grid[columnIndex, rowIndex].Value.ToString();
                    if (((TweakedDataGridView)grid).PreviousValue == keyValue)
                        return;
                }
            }
            else {
                log.Debug("Warning: reacting to an unTweakedDataGridView");
            }

            // grab new data from grid
            bool dataValidForInsert = true;
            for (int i = 0; i < columnCount; i++) {
                if (grid.Columns[i].Visible && !grid.Columns[i].Name.StartsWith("_hidden_")) {
                    if (null == grid[i, rowIndex].Value) {
                        grid[i, rowIndex].Style.BackColor = Color.Pink;
                        grid[i, rowIndex].Style.SelectionBackColor = Color.Red;
                        dataValidForInsert = false;
                    }
                    else {
                        grid[i, rowIndex].Style.BackColor = Color.Empty;
                        grid[i, rowIndex].Style.SelectionBackColor = Color.Empty;
                        Array.Resize(ref visibleColumnIndexes, visibleColumnCount + 1);
                        Array.Resize(ref visibleDbColumnNames, visibleColumnCount + 1);
                        Array.Resize(ref visibleDbColumnValues, visibleColumnCount + 1);
                        if (keyColumnName == null) {
                            keyColumnName = grid.Columns[i].Name;
                        }
                        visibleColumnIndexes[visibleColumnCount] = i;
                        visibleDbColumnNames[visibleColumnCount] = grid.Columns[i].Name;
                        visibleDbColumnValues[visibleColumnCount] = grid[i, rowIndex].Value.ToString();
                        visibleColumnCount++;
                    }
                }
            }

            keyValue = grid[keyColumnName, rowIndex].Value == null ? null : grid[keyColumnName, rowIndex].Value.ToString();

            if (grid.Columns[grid.Columns.Count - 1].Name.StartsWith("_hidden_") && grid.Columns[grid.Columns.Count - 1].HeaderText == keyColumnName) {
                if (null == grid[grid.Columns.Count - 1, rowIndex].Value) {
                    keyValue = null;
                }
                else {
                    keyValue = grid[grid.Columns.Count - 1, rowIndex].Value.ToString();
                }
            }

            int dbResult = 0;

            if (keyValue != null) // update entry in db
            {
                dbResult = updateDbField(grid.Columns[columnIndex].Name, grid[columnIndex, rowIndex].Value.ToString(), tableName, new string[] { keyColumnName }, new string[] { keyValue }, ref errorMessage);
            }
            else // insert into db
            {
                if (dataValidForInsert) {
                    int newPairsCount = 0;
                    if (additionalInsertNamesAndValues != null && (newPairsCount = additionalInsertNamesAndValues.Keys.Count) > 0) {
                        int oldLength = visibleDbColumnNames.Length;
                        Array.Resize(ref visibleDbColumnNames, oldLength + newPairsCount);
                        Array.Resize(ref visibleDbColumnValues, oldLength + newPairsCount);
                        int i = 0;
                        foreach (string key in additionalInsertNamesAndValues.Keys) {
                            visibleDbColumnNames[oldLength + i] = key;
                            visibleDbColumnValues[oldLength + i++] = additionalInsertNamesAndValues[key].ToString();
                        }
                    }

                    dbResult = insertIntoDb(tableName, visibleDbColumnNames, visibleDbColumnValues, ref errorMessage);
                    if (0 < dbResult) {
                        for (int i = 0; i < visibleColumnIndexes.Length; i++) {
                            grid[visibleColumnIndexes[i], rowIndex].Style.BackColor = Color.Pink;
                            grid[visibleColumnIndexes[i], rowIndex].Style.SelectionBackColor = Color.Red;
                        }
                    }
                    else // make sure hidden id column is set so we can edit and double click this new item
                    {
                        string keyName = grid.Columns[grid.Columns.Count - 1].HeaderText;
                        string newKeyValue = null;
                        if (0 == getSingleFromDb(ref newKeyValue, keyName, tableName, visibleDbColumnNames, visibleDbColumnValues))
                            grid[keyName, rowIndex].Value = grid[grid.Columns.Count - 1, rowIndex].Value = newKeyValue;
                    }
                }
                else {
                    dbResult = -1;
                }
            }

            // update grid's hidden unique key column for changed row
            if (dbResult == 0 && grid.Columns[grid.Columns.Count - 1].Name.StartsWith("_hidden_")) {
                string gridKeyColumnName = grid.Columns[grid.Columns.Count - 1].HeaderText;
                if (grid[gridKeyColumnName, rowIndex] != null) {
                    grid[grid.Columns.Count - 1, rowIndex].Value = grid[gridKeyColumnName, rowIndex].Value;
                }
            }
            if (dbResult != 0 && errorMessage != "")
                handleError(errorMessage);
            //else
            //{
            //    if (grid is TweakedDataGridView)
            //        ((TweakedDataGridView)grid).PreviousValue = keyValue;

            //}
        }

        */

    /*
        /// @todo test this and use it in place of getTable() for non-Grid uses
        public int getDbData(ref Dictionary<string, List<OrderedDictionary>> resultData, string tableName, Hashtable whereConditions, List<string> selectThese, string myKeyColumnName) {
            return getDbData(ref resultData, tableName, whereConditions, selectThese, myKeyColumnName, null);
        }

        public int getDbData(ref Dictionary<string, List<OrderedDictionary>> resultData, string tableName, Hashtable whereConditions, List<string> selectThese, string myKeyColumnName, string groupBy) {
            int error = 0;

            if (m_connection.getState().HasFlag(ConnectionState.Open) || (error = m_connection.connect()) == 0) {
                string selectText = "*";
                if (selectThese != null && selectThese.Count > 0) {
                    selectText = selectThese[0];
                    for (int i = 1; i < selectThese.Count; i++)
                        selectText += ", " + selectThese[i];
                }

                // Create an ODBC SQL command that will be executed below.
                string query = "SELECT " + selectText + " FROM " + tableName;
                tackOnWhereConditionV2(ref query, whereConditions);
                if (groupBy != null)
                    query += " GROUP BY " + groupBy;

                log.Info("SQL: " + query);
                DbHelperCommand command = new DbHelperCommand(query, m_connection);

                // Execute the SQL command and return a reader for navigating the results.
                IDataReader reader = command.executeReader(); //(CommandBehavior.CloseConnection)

                // Process entire contents of the results, iterating
                // through each row and through each field of the row.
                int rowIdx = 0;

                //grid.Rows.Clear();

                // for each row
                while (reader.Read() == true) {
                    //while (rowIdx >= grid.Rows.Count) // this should really only happen 0 or 1 times
                    //grid.Rows.Add();

                    string myKeyColumnValue = rowIdx.ToString();
                    OrderedDictionary od = new OrderedDictionary();

                    // for each column within that row
                    for (int i = 0; i < reader.FieldCount; i++) {
                        // save value if this column is the uid key column
                        string columnName = reader.GetName(i);
                        string columnValue = reader.IsDBNull(i) ? null : reader.GetValue(i).ToString();
                        if (myKeyColumnName != null && myKeyColumnName == columnName && columnValue != null)
                            myKeyColumnValue = columnValue;
                        od.Add(columnName, columnValue);
                    }

                    if (resultData.ContainsKey(myKeyColumnValue))
                        resultData[myKeyColumnValue].Add(od);
                    else {
                        List<OrderedDictionary> lod = new List<OrderedDictionary> { od };
                        resultData.Add(myKeyColumnValue, lod);
                    }
                    rowIdx++;
                }

                // Close the reader (keep connection open)
                reader.Close();
            }
            return error;
        }

       */
}
