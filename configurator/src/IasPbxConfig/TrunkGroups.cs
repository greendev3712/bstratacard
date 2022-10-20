/*
TrunkGroups.cs
Intellasoft Configurator.
 
@author Mike Jacobi for Intellasoft
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Lib;

namespace IasPbxConfig
{
	//////////////////////////////////////////////////////////////////////////////////
	/// The TrunkGroups Form; unique functioanlity goes here. Inherits a generic database 
	/// table to DataGidView functionality from TableForm.
	public class TrunkGroups : TableForm
	{
		/// Form title displayed in a large label at the top of the form
		private const string Title = "Trunk Groups";

		/// database table name associated with trunks
		private const string TableName = "Asterisk.v_trunk_groups";
		/// unique key column name in TableName table
		private const string KeyName = "trunk_group_name";

		/// db column names to show in our grid, along with heading names for each
		public static Dictionary<string, string> VisibleColumns = new Dictionary<string, string>()
		{
			{ "trunk_group_name", "Name"},
			{ "trunk_group_longname", "Description"},
			{ "capacity", "Capacity" },
			{ "callerid", "Callerid" },
			{ "record", "Record" }
		};

		public TrunkGroups(DbHelper db)
		{
			init(db, Title, TableName, KeyName, VisibleColumns);
		}

		public override void trkGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && ((DataGridView)sender)[KeyName, e.RowIndex].Value != null)
				Program.m_mainForm.TabManager.addFormInTab((int)IASoftSetupForms.Tgi, new string[] { KeyName }, new string[] { ((DataGridView)sender)[KeyName, e.RowIndex].Value.ToString() });
		}

	}
}
