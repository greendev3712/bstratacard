/*
ClassesOfServiceIncludes.cs
Intellasoft Configurator.
 
@author Mike Jacobi for Intellasoft
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//using Microsoft.Data.Odbc;
using log4net;
using Lib;

namespace IasPbxConfig
{
	//////////////////////////////////////////////////////////////////////////////////
	/// The Classes of Service Includes Form: for any given COS, shows grids for both
	/// available and included COS Includes.
	/// 
	/// Allows including/unincluding the COS Includes, and adjusting order of included
	/// COS Includes.
	public partial class ClassesOfServiceIncludes : ParamForm
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// list of database COS Id's correspoding to cosCombobox
		private List<string> m_cosIds;
		/// list of COS names shown in cosCombobox
		private List<string> m_cosNames;

		private static DbHelper m_db;

		//////////////////////////////////////////////////////////////////////////////////
		/// Constructor.
		///
		/// allocates members.
		/// populates cosComboBox through call to load();
		public ClassesOfServiceIncludes(DbHelper db)
		{
			log.Debug("");
			m_parameterNames = new List<string>(0);
			m_parameterValues = new List<string>(0);
			InitializeComponent();

			m_db = db;
			load();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Constructor with parameters.
		///
		/// allocates members and copies passed parameters in.
		/// populates cosComboBox through call to load();
		/// @param parameterNames ref to names of parameters supplied by caller.
		/// @param parameterValues ref to corresponding values of parameters.
		public ClassesOfServiceIncludes(DbHelper db, string[] parameterNames, string[] parameterValues)
		{
			InitializeComponent();

			m_db = db;

			initParameters(parameterNames, parameterValues);

			load();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// called from end of constructor.
		///
		/// Populates cosComboBox through call to DBHelper's getColumn(). If supplied as a
		/// form parameter, sets the selected COS in the cosComboBox.
		/// @todo handle errors on getColumn calls like Extension.cs's load()
		private void load()
		{
			ParamInitRefreshCallback = handleUidSelectByParameter;
			ButtonRefreshCallback = handleButtonRefresh;

			m_cosIds = new List<string>(0);
			m_cosNames = new List<string>(0);

			m_db.getColumn(ref m_cosIds, "cos_id", "Asterisk.v_cos");
			m_db.getColumn(ref m_cosNames, "cos_name", "Asterisk.v_cos");

			//cosComboBox.Items.AddRange(m_cosNames.ToArray());
			string[][] initData = null;
			m_db.getTable(ref initData, "Asterisk.v_cos", ClassesOfService.VisibleColumns, "cos_name");
			uidJumpToMultiCombo.initWithData(initData);

			handleUidSelectByParameter();
		}

		private bool m_didReload = false;

		private void handleUidSelectByParameter()
		{
			int selectedIdx = m_parameterNames.IndexOf("selected_cos");
			if (selectedIdx >= 0)
			{
				int newIndex = m_cosIds.IndexOf(m_parameterValues[selectedIdx]);
				if (newIndex < 0)
				{
					if (!m_didReload)
					{
						m_didReload = true;
						load(); // (reload)
					}
					else
					{
						log.Error("Cannot find uid " + m_parameterValues[selectedIdx]);
					}
				}
				else
				{
					// this will trigger cosComboBox's SelectedIndexChanged event which will 
					// call refresh() and (re)populate our grids
					uidJumpToMultiCombo.selectByIndex(newIndex);
				}
			}
			m_didReload = false;
		}

		private void handleButtonRefresh()
		{
			string value = uidJumpToMultiCombo.getSelectedValue();
			load();
			uidJumpToMultiCombo.selectByValue(value);
			//refresh();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// (re)populates cosiGrid and cosaGrid (COS Includes Included and COS Includes 
		/// Available).
		/// 
		/// ...through call to DBHelpers getTable(). Called when cosComboBox selection 
		/// changes.
		/// @todo handle getTable errors
		/// @todo handle empty cosComboBox selection?
		private void refresh()
		{
			//m_db.getTable(cosiGrid, "Asterisk.v_cos_includes", new string[] { "cos_id" }, new string[] { m_cosIds[uidJumpToMultiCombo.getSelectedIndex()] }, new Dictionary<string, string>() { { "include_type", "Type" }, { "include_name", "Name" } });
			m_db.getTable(cosiGrid, "Asterisk.v_cos_includes", new Hashtable(){ { "cos_id" , new List<string>() { m_cosIds[uidJumpToMultiCombo.getSelectedIndex()] }}}, new Dictionary<string, string>() { { "include_type", "Type" }, { "include_name", "Name" } });
			foreach (DataGridViewColumn c in cosiGrid.Columns)
				c.SortMode = DataGridViewColumnSortMode.NotSortable;
			//m_db.getTable(cosaGrid, "Asterisk.v_cos_includes_available", new string[] { "cos_id" }, new string[] { m_cosIds[uidJumpToMultiCombo.getSelectedIndex()] }, new Dictionary<string, string>() { { "context_type_longname", "Type" }, { "context_label", "Name" }, { "asterisk_context", "Description" } });
			m_db.getTable(cosaGrid, "Asterisk.v_cos_includes_available", new Hashtable() { { "cos_id", new List<string>() { m_cosIds[uidJumpToMultiCombo.getSelectedIndex()] } } }, new Dictionary<string, string>() { { "context_type_longname", "Type" }, { "context_label", "Name" }, { "asterisk_context", "Description" } });
			Helper.selectGridsRowByIndex(cosiGrid, 0);
			Helper.selectGridsRowByIndex(cosaGrid, 0);
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Adjusts order of selected included COS Include.
		/// 
		/// Acts on cosiGrid. Adjusted position through update call on 
		/// v_cos_includes.cosi_pos. Back end handles renumbering etc. We simply refresh 
		/// both grids at the end.
		/// @param newPos new position
		/// @todo better handling of db error conditions (call global error func)
		private void adjustPos(string newPos)
		{
			if (cosiGrid["cosi_id", cosiGrid.CurrentRow.Index] != null) // check if Include is selected
			{
				string[] whereNames = { "cosi_id" };
				string[] whereValues = { cosiGrid["cosi_id", cosiGrid.CurrentRow.Index].Value.ToString() };
				string errorMessage = "";
				m_db.updateDbField("cosi_pos", newPos, "Asterisk.v_cos_includes", whereNames, whereValues, ref errorMessage, false);
				refresh();
				if (errorMessage != "")
				{
					MessageBox.Show(errorMessage);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for Load event; empty method.
		///
		/// @param sender self?
		/// @param e EventArgs
		/// @todo investigate why control never enters this function.
		private void ClassesOfServiceIncludes_Load(object sender, EventArgs e)
		{
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for cosComboBox's SelectedIndexChanged event; refreshes both grids.
		///
		/// ... through call to refresh().
		/// @param sender cosComboBox
		/// @param e EventArgs
		private void uidJumpToMultiCombo_RowSelected()
		{
			refresh();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for Uninclude Buttons's Click event; deletes selected included 
		/// Include from database.
		///
		/// ...through call to DBHelper's deleteFromDb(). Backend does rest (update 
		/// available Includes). We simply refresh both grids at the end.
		/// @param sender unincludeButton
		/// @param e EventArgs
		/// @todo handle db errors.
		private void unincludeButton_Click(object sender, EventArgs e)
		{
			List<string> deleteUids = new List<string>(cosiGrid.SelectedRows.Count);
			for (int i = 0; i < cosiGrid.SelectedRows.Count; i++)
			{
				int gridIndex = cosiGrid.SelectedRows[i].Index;
				if (cosiGrid["cosi_id", gridIndex] != null)
					deleteUids.Add(cosiGrid["cosi_id", gridIndex].Value.ToString());
			}
			
			if (deleteUids.Count > 0)
			{
				m_db.deleteFromDb("Asterisk.v_cos_includes", new string[] { "cosi_id" }, deleteUids.ToArray(), true, true);
				refresh();
			}
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for Include Buttons's Click event; Inserts selected available 
		/// Include into database.
		///
		/// ...through call to DBHelper's insertIntoDb(). Backend does rest (update 
		/// included Includes). We simply refresh both grids at the end.
		/// @param sender includeButton
		/// @param e EventArgs
		/// @todo handle db errors better (use global error method).
		private void includeButton_Click(object sender, EventArgs e)
		{
			if (cosaGrid["cos_id", cosaGrid.CurrentRow.Index] != null && cosaGrid["include_id", cosaGrid.CurrentRow.Index] != null)
			{
				string errorMessage = "";
				if (0 < m_db.insertIntoDb("Asterisk.v_cos_includes", new string[] { "cos_include_id", "cos_id" }, new string[] { cosaGrid["include_id", cosaGrid.CurrentRow.Index].Value.ToString(), cosaGrid["cos_id", cosaGrid.CurrentRow.Index].Value.ToString() }, ref errorMessage))
				{
					MessageBox.Show(errorMessage);
				}
				refresh();
			}
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for Move Selected Include Position Up Buttons's Click event; adjusts 
		/// selected Includes position up by reducing its value by 1.
		///
		/// ...through call to adjustPos(). The supplied value is then passed on and 
		/// evaluated in SQL.
		/// @param sender moveUpButton
		/// @param e EventArgs
		private void moveUpButton_Click(object sender, EventArgs e)
		{
			adjustPos("cosi_pos - 1");
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for Move Selected Include Position Down Buttons's Click event; 
		/// adjusts selected Includes position up by increasing its value by 1.
		///
		/// ...through call to adjustPos(). The supplied value is then passed on and 
		/// evaluated in SQL.
		/// @param sender moveDownButton
		/// @param e EventArgs
		private void moveDownButton_Click(object sender, EventArgs e)
		{
			adjustPos("cosi_pos + 1");
		}

		private void cosiGrid_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
		{
			Helper.selectGridsRowsBySelectedCells((DataGridView)sender);
		}

		private void cosaGrid_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
		{
			//Helper.selectGridsRowsBySelectedCells((DataGridView)sender);
		}

		private bool m_ignoreSelectionEvent = false;
		private void cosaGrid_SelectionChanged(object sender, EventArgs e)
		{
			// allow only 1 row to be selected and make sure it is the current row
			if (!m_ignoreSelectionEvent)
			{
				m_ignoreSelectionEvent = true;
				foreach (DataGridViewRow r in cosaGrid.Rows)
					r.Selected = false;
				if (cosaGrid.CurrentRow != null)
					cosaGrid.CurrentRow.Selected = true;
				m_ignoreSelectionEvent = false;
			}
		}

		private void cosaGrid_MouseUp(object sender, MouseEventArgs e)
		{
			// above code (cosaGrid_SelectionChanged) lags behind by one row when selected
			// with mouse drag in row header area (to the left of the grid cells. The below
			// code handles adjusting the selected row on mouse up (at the end of the drag).
			// not ideal but works. @todo: make ideal to prevent any visible lag
			foreach (DataGridViewRow r in cosaGrid.Rows)
				r.Selected = false;
			cosaGrid.CurrentRow.Selected = true;
		}

	}
}
