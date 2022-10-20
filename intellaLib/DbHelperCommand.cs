/*
DbHelperCommand.cs
Intellasoft Configurator.
 
@author Mike Jacobi for Intellasoft
*/

using System;
using System.Collections.Generic;
using System.Text;
using log4net;
//using Microsoft.Data.Odbc;
using Npgsql;
using System.Data;

namespace Lib
{
	internal class DbHelperCommand
	{
		private static readonly ILog log = LogManager.GetLogger(
		System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private bool m_typeIsNpgsql;
		private NpgsqlCommand m_npgsqlCommand;
//		private OdbcCommand m_odbcCommand;

		public DbHelperCommand(string query, DbHelperConnection connection)
		{
			if (connection.isTypeNpgsql())
			{
				m_typeIsNpgsql = true;
				m_npgsqlCommand = new NpgsqlCommand(query, (NpgsqlConnection)connection.getUnderlyingConnection());
			}
//			else
//				m_odbcCommand = new OdbcCommand(query, (OdbcConnection)connection.getUnderlyingConnection());
		}

		public IDataReader executeReader()
		{
			return m_typeIsNpgsql ? (IDataReader)m_npgsqlCommand.ExecuteReader() : null;//(IDataReader)m_odbcCommand.ExecuteReader(); //(CommandBehavior.CloseConnection)
		}

		public int executeNonQuery()
		{
			return m_typeIsNpgsql ? m_npgsqlCommand.ExecuteNonQuery() : 0;//m_odbcCommand.ExecuteNonQuery();
		}
	}
}
