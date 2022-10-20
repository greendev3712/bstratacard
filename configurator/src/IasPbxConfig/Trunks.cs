/*
Trunks.cs
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
	public class Trunks : TableForm
	{
		private const string Title = "Trunks";

		/// database table name associated with trunks
		private const string TableName = "Asterisk.v_trunks";
		/// unique key column name in TableName table
		private const string KeyName = "trunk_name";

		/// db column names to show in our grid, along with heading names for each
		public static Dictionary<string, string> VisibleColumns = new Dictionary<string, string>
		{
			{ "trunk_name", "Name"},
			{ "trunk_longname", "Description"},
			{ "device_type", "Device Type" },
			{ "capacity", "Capacity" }
		};

		private static Hashtable SpecialControls = new Hashtable()
		{
			{ "device_type", new Hashtable() 
				{ 
					{ "ControlType", "ComboBox" },
					{ "TableName", "Asterisk.v_device_types" },
					{ "UidName", "device_type" }
				}
			}
		};

		public Trunks(DbHelper db)
		{
			init(db, Title, TableName, KeyName, VisibleColumns, SpecialControls);
		}

		public override void trkGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && ((DataGridView)sender)[KeyName, e.RowIndex].Value != null)
				Program.m_mainForm.TabManager.addFormInTab((int)IASoftSetupForms.Trkd, new string[] { KeyName }, new string[] { ((DataGridView)sender)[KeyName, e.RowIndex].Value.ToString() });
		}
	}
}
