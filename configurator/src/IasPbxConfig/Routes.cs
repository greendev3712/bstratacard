using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lib;

namespace IasPbxConfig
{
	public class Routes : TableForm
	{
		private const string Title = "Routes";

		/// database table name associated with this form
		private const string TableName = "Asterisk.v_routes";
		/// unique key column name in TableName table
		private const string KeyName = "route_name";

		/// db column names to show in our grid, along with heading names for each
		private static Dictionary<string, string> VisibleColumns = new Dictionary<string, string>
		{
			{ "route_name", "Route" },
			{ "route_longname", "Description" },
			{ "trunk_group_id", "Trunk Group" },
			{ "pattern", "Pattern" },
			{ "record", "Record" },
			{ "replace", "Replace" },
			{ "remove", "Remove" },
			{ "prepend", "Prepend" },
			{ "postpend", "Postpend" }
		};

		public Routes(DbHelper db)
		{
			init(db, Title, TableName, KeyName, VisibleColumns);
		}
	}
}
