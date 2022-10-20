/*
RegistrationsIax.cs
Intellasoft Configurator.
 
@author Mike Jacobi for Intellasoft
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lib;

namespace IasPbxConfig
{
	//////////////////////////////////////////////////////////////////////////////////
	/// The RegistrationsIax Form; unique functioanlity goes here. Inherits a generic database 
	/// table to DataGidView functionality from TableForm.
	public class RegistrationsIax : TableForm
	{
		/// Form title displayed in a large label at the top of the form
		private const string Title = "Registrations Iax";

		/// database table name associated with trunks
		private const string TableName = "Asterisk.registrations_iax";
		/// unique key column name in TableName table
		private const string KeyName = "username";

		/// db column names to show in our grid, along with heading names for each
		private static Dictionary<string, string> VisibleColumns = new Dictionary<string, string>()
		{
			{ "username", "Username" },
			{ "host", "Host" },
			{ "port", "Port" },
			{ "secret", "Secret" },
			{ "secret_is_rsa_key", "RSA" },
			{ "enabled", "Enabled" }
		};

		public RegistrationsIax(DbHelper db)
		{
			init(db, Title, TableName, KeyName, VisibleColumns);
		}
	}
}
