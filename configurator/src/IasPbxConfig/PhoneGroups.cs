/*
PhoneGroups.cs
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
	/// The PhoneGroups Form; unique functioanlity goes here. Inherits a generic database 
	/// table to DataGidView functionality from TableForm.
	public class PhoneGroups : TableForm
	{
		/// Form title displayed in a large label at the top of the form
		private const string Title = "";

		/// database table name associated with trunks
		private const string TableName = "Asterisk.v_phonegroups";
		/// unique key column name in TableName table
		private const string KeyName = "phonegroup_name";

		/// db column names to show in our grid, along with heading names for each
		public static Dictionary<string, string> VisibleColumns = new Dictionary<string, string>()
		{
			{ "phonegroup_name", "Name"},
			{ "phonegroup_longname", "Description"},
			{ "callerid_name", "Callerid Name" },
			{ "callerid_number", "Callerid Num" },
			{ "extensions_start", "Start" },
			{ "extensions_end", "End" },
			{ "record", "Record" }
		};

		public PhoneGroups(DbHelper db)
		{
			init(db, Title, TableName, KeyName, VisibleColumns);
		}
	}
}
