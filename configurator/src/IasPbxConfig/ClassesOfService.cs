/*
ClassesOfService.cs
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
	public class ClassesOfService : TableForm
	{
		private const string Title = "Classes of Service";

		/// database table name associated with trunks
		private const string TableName = "Asterisk.v_cos";
		/// unique key column name in TableName table
		private const string KeyName = "cos_id";

		/// db column names to show in our grid, along with heading names for each
		public static Dictionary<string, string> VisibleColumns = new Dictionary<string, string>
		{
			{ "cos_name", "Name" },
			{ "cos_longname", "Description" }
		};

		public ClassesOfService(DbHelper db)
		{
			init(db, Title, TableName, KeyName, VisibleColumns);
		}

		public override void trkGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && ((DataGridView)sender)[KeyName, e.RowIndex].Value != null)
				Program.m_mainForm.TabManager.addFormInTab((int)IASoftSetupForms.Cosi, new string[] { "selected_cos" }, new string[] { ((DataGridView)sender)[KeyName, e.RowIndex].Value.ToString() });
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// based on rules specified here, checks COS in database to see if we are allowed 
		/// to delete it.
		/// 
		/// @param cosId database COS Id of COS we are checking if deletable
		/// @return whether or not we are allowed to delete this COS
		/// @todo: get info about how to tell if cos is in use by phonegroup
		/// @todo: actually use this function
		private bool checkIfCosDeletable(string cosId)
		{
		    // Rows cannot be deleted if the class of service is used by an extension [v_extensions] or phonegroup [v_phonegroups]
		    // Rows cannot be deleted if the class of service has includes associated [v_cos_includes] 
		    bool result = false;
		//    string dbResult = "";
		//    bool checkFailed = false;

		//    // check extensions
		//    if (0 == m_db.getSingleFromDb(ref dbResult, "COUNT(*)", "Asterisk.v_extensions", "class_of_service_id", cosId, true))
		//    {
		//        if (dbResult != "0")
		//        {
		//            checkFailed = true;
		//        }
		//    }
		//    else
		//    {
		//        checkFailed = true;
		//    }

		//    // check cos includes
		//    if (!checkFailed && 0 == m_db.getSingleFromDb(ref dbResult, "COUNT(*)", "Asterisk.v_cos_includes", "cos_id", cosId, true))
		//    {
		//        if (dbResult != "0")
		//        {
		//            checkFailed = true;
		//        }
		//        else
		//        {
		//            result = true;
		//        }
		//    }
		//    else
		//    {
		//        checkFailed = true;
		//    }

		    return result;
		}

		// usage:
					//if (checkIfCosDeletable(cosGrid[0, e.RowIndex].Value.ToString()))
					//{
					//    // delete row
					//    m_db.deleteFromDb("Asterisk.v_cos", "cos_id", cosGrid[0, e.RowIndex].Value.ToString());
					//    cosGrid.Rows.RemoveAt(e.RowIndex);
					//}
					//else
					//{
					//    MessageBox.Show("Delete Not Permitted. Please check to see if this COS is in use by an extension, phone group, or COS include.");
					//}

	}
}
