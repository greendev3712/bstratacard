/*
DbHelperConnections.cs
Intellasoft Configurator.
 
@author Mike Jacobi for Intellasoft
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Diagnostics;
using log4net;
using Npgsql;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace Lib
{
	[Serializable]
	public class ConnectionException : System.Exception
	{
		public ConnectionException()
		{
		}

		public ConnectionException(string message)
			: base(message)
		{
		}

		public ConnectionException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected ConnectionException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}


    [Obsolete("DbHelperConnection needs to be just straight up merged into SimpleDB")]
    internal class DbHelperConnection
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
	    );

		private bool m_typeIsNpgsql;
		private NpgsqlConnection m_npgsqlConnection;
        public bool m_useSsl = false;
	    public int m_commandTimeout = 0;
        private SimpleDbConnectionParameters m_connectionParameters;  // NEW - save the hash host/port/user/pass/database for cloning later
        public DateTime m_connect_time;

		QD.QE_ErrorCallbackFunction m_handleErrorCallback = null;

		public DbHelperConnection(string connectionString, string type)
		{
			if (type == "npgsql")
				m_typeIsNpgsql = true;

			init(connectionString, new SimpleDbConnectionParameters());
		}

		public DbHelperConnection(NpgsqlConnection n)
		{
			m_typeIsNpgsql = true;
			m_npgsqlConnection = n;
			
		}

		public bool isTypeNpgsql()
		{
			return m_typeIsNpgsql;
		}

        public NpgsqlConnection getNpgSqlConnection() {
            return m_npgsqlConnection;
        }

		public object getUnderlyingConnection()
		{
			return m_typeIsNpgsql ? (object)m_npgsqlConnection : null;//m_odbcConnection;
		}

		public ConnectionState getState()
		{
			return m_typeIsNpgsql ? m_npgsqlConnection.FullState : ConnectionState.Broken;//m_odbcConnection.State;
		}

		public bool isInitialized()
		{
			return m_npgsqlConnection != null;// || m_odbcConnection != null;
		}

		public void close()
		{
			if (m_typeIsNpgsql)
			{
				m_npgsqlConnection.Close();
                NpgsqlConnection.ClearPool(m_npgsqlConnection);
                //m_npgsqlConnection.ClearPool(); 
            }
		}

		private string getDatabase()
		{
			return m_typeIsNpgsql ? m_npgsqlConnection.Database : "";
		}

		private string getConnectionString()
		{
			return m_typeIsNpgsql ? m_npgsqlConnection.ConnectionString : "";
		}

        public SimpleDbConnectionParameters getConnectionParameters() 
	    {
	        return m_connectionParameters;
	    }

        public bool connect()
        {
            lock (this.m_npgsqlConnection)
            {
                return _connect();
            }
        }

        public void SetErrorCallback(QD.QE_ErrorCallbackFunction errorHandler) {
            this.m_handleErrorCallback = errorHandler;
        }

		private void handleError(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] msgFormat) {
			if (this.m_handleErrorCallback != null) {
				this.m_handleErrorCallback.Invoke(errorLevel, errorToken, ex, String.Format(msg, msgFormat));
				return;
			}

			MessageBox.Show(String.Format("DbHelperConnection: {0} {1} {2}", errorLevel, errorToken, String.Format(msg, msgFormat)));
		}

	    private bool _connect() {
            if (false && Debugger.IsAttached)
            {
                if (!this.getState().HasFlag(ConnectionState.Open)) {
                    log.Warn("SimpleDB Connecting/Reconnecting");
                    this.close();
                    log.Info("Making SQL connection to " + this.getDatabase() + " with string: " + this.getConnectionString() + " isNpgsql:" + m_typeIsNpgsql);
                    this.open();
                    m_connect_time = DateTime.Now;
                }

                return true;
            }

            // Regular (Production)
            if (!this.getState().HasFlag(ConnectionState.Open)) { 
			    try {
				    log.Warn("SimpleDB Connecting/Reconnecting");
				    /// @todo perhaps it's better to throw an exception here?
				    this.close();
				    log.Info("Making SQL connection to " + this.getDatabase() + " with string: " + this.getConnectionString() + " isNpgsql:" + m_typeIsNpgsql);

				    this.open();
                    m_connect_time = DateTime.Now;
					return true;
				}
                catch (System.Net.Sockets.SocketException ex) {
                    // Example: {"No connection could be made because the target machine actively refused it"}
					log.Error("Connection to SQL failed: " + ex.ToString());

					SimpleDbConnectionParameters connection = this.getConnectionParameters();
					handleError(QD.ERROR_LEVEL.ERROR, "DB_CONENCTION_FAILED", ex, "Failed connection to: {0}:{1} (SSL: {2})", connection.Host, connection.Port, connection.SSL_Required.ToString());

					return false;
                }
			    catch (Exception ex) {
                    // Some other exception!

                    string error_msg = "Connection to SQL failed: " + ex.ToString();

                    if (ex.InnerException != null) {
                        error_msg += " -- " + ex.InnerException.ToString();
                    }

					MessageBox.Show(error_msg);

				    log.Error(error_msg);
				    ConnectionException x = new ConnectionException(error_msg);
					return false;
			    }

			}

			return true;
		}

		private void open()
		{
            if (m_typeIsNpgsql) { 
				m_npgsqlConnection.Open();
            }
		}

        internal void init(SimpleDbConnectionParameters connectionParameters)
		{
			init(null, connectionParameters);
		}

        private void init(string connectionString, SimpleDbConnectionParameters connectionParameters)
		{
            //     connectionString connectionParameters
            // (1) not set          not set                just bail
            // (2)     set          not set                temp - want to throw exception to see if connectionString handling is necessary
            // (3)     set              set                temp - throw exception - what takes priority?
            // (4) not set              set                preferred method

            connectionString = connectionString ?? "";  // NEW - simplify values to coalesce into ""
            if (connectionString == "" && connectionParameters == null) { return; }  // NEW - (1) just bail

            // NEW - (2) temp debugging to figure out usages of connectionString by itself
		    if (connectionString != "" && connectionParameters == null) {
		        throw new Exception("DbHelperConnection: Only using connectionString - not preferred method");
		    }
            // NEW - (3) temp debugging to see if both string and params are used
            if (connectionString != "" && connectionParameters != null) { 
                throw new Exception("DbHelperConnection: Both connectionString and connectionParameters"); 
            }

            // NEW - (4) preferred method of only using connectionParameters
            if (connectionString == "" && connectionParameters != null) {
		        m_connectionParameters = connectionParameters;  // NEW - save the connectionParameters for cloning later

                if (m_typeIsNpgsql) {
		            // npgsql: "Server=myhostname;Port=5432;User Id=myusername;Password=mypass;Database=mydatabase;"
		            connectionString =
		                  "Server="    + connectionParameters["host"]
		                + ";Pooling="  + connectionParameters["pooling"]
		                + ";Port="     + connectionParameters["port"]
		                + ";User Id="  + connectionParameters["user"]
		                + ";Password=" + connectionParameters["pass"]
		                + ";Database=" + connectionParameters["database"]
		                + ";";

		            if (this.m_useSsl) {
		                connectionString += "SslMode=Require;TrustServerCertificate=true;";
		            }

                    if (this.m_commandTimeout != 0) {
                        connectionString += "CommandTimeout=" + this.m_commandTimeout + ";";
                    }
		        }
		        else {
		            // "DSN=MyDSN;UID=Admin;PWD=Test" (UID = User name, PWD = password.)
		            if (connectionParameters.ContainsKey("dsn"))
		                connectionString = "DSN=" + connectionParameters["dsn"];
		            else {
		                /// @todo test below line of code: never tried odbc connection without DSN
		                /// @todo research possible other handlers. possible options and
		                /// their connection strings at http://www.connectionstrings.com/postgre-sql
		                connectionString =
		                    "Driver={PostgreSQL}"
		                    + ";Server=" + connectionParameters["host"]
		                    + ";Port=" + connectionParameters["port"]
		                    + ";Database=" + connectionParameters["database"];
		            }
		            connectionString += ";UID=" + connectionParameters["user"]
		                                + ";PWD=" + connectionParameters["pass"]
		                                + ";";
		        }

		    }

            // NEW - With the new code above, this should always be true
		    if (connectionString != "")
			{
				if (m_typeIsNpgsql)
					m_npgsqlConnection = new NpgsqlConnection(connectionString);
			}
         }
     }
}
