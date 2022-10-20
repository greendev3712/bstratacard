/*
NameValueGrid.cs
Intellasoft Configurator.
 
@author Mike Jacobi for Intellasoft
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;
using log4net;

/*

namespace Lib
{
	//////////////////////////////////////////////////////////////////////////////////
	/// Handles Name Value Grid functionality through helper functions.
	///
	/// Each instance handles 1 grid, passed in through the constructor. A data object 
	/// (@see m_data[][]) is also passed in to initialize and setup the grid. 
	/// A NameValueGrid handles an unlimited length list of Names and Value Controls
	/// (TextBox or ComboBox) and handles events for changing values.
	public class NameValueGrid
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// descriptive index names to m_data[][]
		public enum DataIndexes
		{
			Label,                  /// (Name) Label to show next to control
			ControlType,            /// type of control to make (TextBox, ComboBox)
			ColumnName,             /// database column name for modifying control
			PopulateTableName,      /// for ComboBox, table to use to populate options
			PopulateIdColumnName,   /// for ComboBox, column name for corresponding ids
			PopulateNameColumnName, /// for ComboBox, column name to use to populate options
			ControlOptionString,    /// for ComboBox, how to update ColumnName: byName(default) or byId
		};

		/// the DataGridView we are working with. passed in through constructor. one per 
		/// instance.
		private DataGridView m_grid;

		/// pointer to a pre-initialized dbhelper. passed in through constructor. one per
		/// instance. can pass in same one for different instances.
		private DbHelper m_db;

		/// Grid's 'name column' name
		static string NameHeading = "Name";
		/// Grid's 'value column' name
		static string ControlHeading = "Control";
		/// Grid's 'data order index' name
		static string OrderHeading = "DataOrderIndex";

		/// grid that are affected by special actions in this one. nvGridName=>nvGridRef 
		private Hashtable m_affectedGrids;

		/// options for this nvgrid (optional)
		private Hashtable m_options;

		/// keep track of hidden name/value pairs. which ones are hidden are passed in through options.
		/// Even though they are hidden, they are still accessible.
		private Hashtable m_hiddenRows;

		/// array of length equal to number of rows in m_grid. each item is 
		/// another array of data for that row's control; @see DataIndexes.
		private string[][] m_data;
		/// holds comboboxes' ids, other information for each control can be stored here
		List<string>[][] m_controlAttributes;
		/// databse table name associated with this grid
		private string m_tableName;
		/// database unique key column name to use when working with this grid
		private string m_uidKeyName;
		/// spot to hold previous value of the active Control while it is edited 
		/// by user, in case their edit is canceled.
		private string m_previousValue;
		/// used internally to handle active/inactive state of grid.
		/// cell validating events are not handled when false.
		private bool m_active;

		private bool m_singleRowTableMode;

		DataGridViewEditingControlShowingEventHandler m_cellControlShowingEventHandler;
		DataGridViewCellValidatingEventHandler m_cellControlValidatingEventHandler;
		DataGridViewCellEventHandler m_cellControlValidatedEventHandler;
		/////////////////////////////////////////////////////////////////////////////////////
		/// delegate description for callback.
		///
		/// @param errorMessage text for the errorMessage
		public delegate void handleErrorCallbackType(string errorMessage);

		getControlsCurrentValueCallbackType m_uniqueKeyValueHolderControlCallback = null;
		refreshCallbackType m_refreshUniqueKeyValueHolderControlCallback = null;
		handleErrorCallbackType m_handleErrorCallback = null;

		//////////////////////////////////////////////////////////////////////////////////
		/// Constructor.
		///
		/// Copy passed in object references into member variables..
		/// @param grid DataGridView object every instance of this class will 
		/// work with.
		/// @param options Hash of NVGrid options such as HiddenRows, excludeColumns, and initMethod
		/// @param data Data used to initialize the grid and the controls within it.
		/// @see m_data for description.
		/// @param uidKeyName Database Unique Id column name associated w/ grid changes
		/// @param tableName Database table name associated w/ grid changes
		public NameValueGrid(DbHelper db, DataGridView grid, Hashtable options, string[][] data, string uidKeyName, string tableName, List<NameValueGrid> affectedNVGrids)
		{
			m_db = db;

			m_grid = grid;
			m_grid.EditMode = DataGridViewEditMode.EditOnEnter;

			m_hiddenRows = new Hashtable();
			if (options == null)
			{
				m_options = new Hashtable();
			}
			else
			{
				m_options = options;

				if (m_options.ContainsKey("SingleRowTable"))
				{
					string value = ((string)m_options["SingleRowTable"]).ToLower();
					if (value == "true" || value == "yes")
						m_singleRowTableMode = true;
				}

				if (m_options.ContainsKey("HiddenRows"))
				{
					foreach (string s in ((Array)(m_options["HiddenRows"])))
					{
						m_hiddenRows.Add(s, null);
					}

				}
			}
			if (data == null)
			{
				m_data = new string[0][];
			}
			else
			{
				m_data = data;
			}
			m_uidKeyName = uidKeyName;
			m_tableName = tableName;

			m_affectedGrids = new Hashtable();
			if (affectedNVGrids != null)
			{
				for (int i = 0; i < affectedNVGrids.Count; i++)
				{
					m_affectedGrids.Add(affectedNVGrids[i].getName(), affectedNVGrids[i]);
				}
			}

			m_controlAttributes = new List<string>[m_data.Length][];

			m_grid.Columns.Add(NameHeading, "");
			m_grid.Columns.Add(ControlHeading, "");
			m_grid.Columns.Add(OrderHeading, "");
			m_grid.Columns[OrderHeading].Visible = false;

			// We are now handling keeping track of the order using a hidden 3rd column in the grid
			// which stores data indexes, so we are no longer setting the grid to NotSortable.
			//
			// @todo: clients need to keep track of order and sorting in some situations (see
			// ExtensionDetails and TrunkDetails "new copy of current" button); change so 
			// everything is handled in NVG class
			//
			// m_grid.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
			// m_grid.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;

			m_grid.ReadOnly = false;
			m_grid.Columns[NameHeading].ReadOnly = true;

			m_grid.EditMode = DataGridViewEditMode.EditOnEnter;

			m_grid.AllowUserToOrderColumns = false;

			if (!m_singleRowTableMode)
			{
				// deactivate grid; grid becomes active when uid is selected via call to refresh(uid)
				deactivate();
			}

			m_grid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(grid_CellClick);
		}

		/////////////////////////////////////////////////////////////////////////////////////
		/// delegate description for callback.
		///
		/// @param name name of name-value pair to return value for
		/// @return corresponding value
		public delegate string getControlsCurrentValueCallbackType(string name);

		/////////////////////////////////////////////////////////////////////////////////////
		/// delegate description for callback used if unique key was changed.
		///
		/// @param selectedExtensionSuggestion the new selected value for the unique key 
		///        selection control
		public delegate void refreshCallbackType(string selectedExtensionSuggestion, bool doSuppressDetailControlRefresh);

		private delegate void changeTableCallbackType();

		/////////////////////////////////////////////////////////////////////////////////////
		/// Runs when one of grid's value/contol cells enters focus: handle EditingControlShowing 
		/// event on a DataGridViewCell's Control. 
		/// 
		/// Save the Control's current value so we can cancel a chnage if needed. We manually add/remove
		/// a ref to this method to the event handler in AddUpdateEvents().
		///
		/// Param: c the DataGridView
		/// Param: e EventArgs of the Enter focus event, including a ref to the current Control.
		private void cellControlShowing(object c, DataGridViewEditingControlShowingEventArgs e)
		{
			m_previousValue = e.Control.Text;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		/// Runs when a value/control cell loses focus: handles a new value set in 
		/// a Control within a grid's DataGridViewCell
		/// 
		/// Insert the newly entered value into the database. If the insert fails, cancel the 
		/// CellValidating event and give user an error message. We manually add/remove
		/// a ref to this method to the event handler in AddUpdateEvents().
		///
		/// Param: g the DataGridView that is Validating
		/// Param: e DataGridViewCellValidatingEventArgs of the Validating event
		private void cellControlValidating(object g, DataGridViewCellValidatingEventArgs e)
		{
			// ignore if grid not active or if selected cell not in a control column
			if (m_active && e.ColumnIndex == m_grid.Columns[ControlHeading].Index)
			{
				string uniqueKeyValue = null;
				if (m_uniqueKeyValueHolderControlCallback != null)
				{
					uniqueKeyValue = m_uniqueKeyValueHolderControlCallback(m_uidKeyName);
				}

				if (uniqueKeyValue != null || m_singleRowTableMode) // ignore if no unique key is selected
				{
					int row = e.RowIndex;
					int dataIndex = convertToDataIndex(row);
					string newValue = e.FormattedValue.ToString();
					string updateColumnName = null;
					if (m_data.Length > 0)
					{
						updateColumnName = m_data[dataIndex][(int)DataIndexes.ColumnName];
					}
					else
					{
						updateColumnName = m_grid[NameHeading, row].Value.ToString();
					}

					string error = "";

					// if this control item is configured to be updated by changing the id instead 
					// of the name, change newValue from name to id here
					if (m_grid.EditingControl is ComboBox &&
						m_data.Length > 0 &&
						m_data[dataIndex].Length > (int)DataIndexes.ControlOptionString &&
						m_data[dataIndex][(int)DataIndexes.ControlOptionString] == "byId")
					{

						newValue = convertComboItemNameToId(row, newValue);
					}

					// if we are not using m_data, get the unique id value here
					// @todo what is this???
					if (m_data.Length == 0)
					{
						//int errorCode = Program.m_db.getSingleFromDb(newValue, 
						//Console.WriteLine("d");
					}

					if (newValue != m_previousValue)
					{
						if (0 != m_db.updateDbField(updateColumnName, newValue, m_tableName, m_uidKeyName, uniqueKeyValue, ref error))
						{
							e.Cancel = true;
							if (m_grid.EditingControl is ComboBox)
							{
								ComboBox cb = (ComboBox)m_grid.EditingControl;
								cb.Text = m_previousValue;
							}
							else // TextBox
							{
								TextBox tb = (TextBox)m_grid.EditingControl;
								tb.Text = m_previousValue;
							}
							//m_grid.UpdateCellValue(1, e.RowIndex);
							log.Debug(error);
							handleError("Update failed: " + error);
						}
					}
				}
			}
		}

		private void handleError(string errorMessage)
		{
			//Program.m_mainForm.launchError(errorMessage);
			if (m_handleErrorCallback != null)
				m_handleErrorCallback(errorMessage);
		}

		/////////////////////////////////////////////////////////////////////////////////////
		/// Handle CellValidated event on a Control within a Grid cell.
		/// 
		/// If we made a change, update the previous value to the new value. Also, if the 
		/// field we updated is a unique key field, make a callback to let the Grid's
		/// Super form reload what it needs to. Also check and handle any needed changeTable
		/// action. We manually add/remove a ref to this method to the event handler in 
		/// AddUpdateEvents().
		///
		/// Param: g the the Grid that is Validated
		/// Param: e EventArgs of the Validated event, which includes a ref to the cell
		private void cellControlValidated(object g, DataGridViewCellEventArgs e)
		{
			// ignore if grid not active opr if selected cell not in a control column
			if (m_active && e.ColumnIndex == m_grid.Columns[ControlHeading].Index)
			{
				string newValue = ((DataGridView)g)[ControlHeading, e.RowIndex].Value == null ? "" : ((DataGridView)g)[ControlHeading, e.RowIndex].Value.ToString();
				string updateColumnName = null;

				if (getMode() == "auto")
				{
					updateColumnName = ((DataGridView)g)[NameHeading, e.RowIndex].Value.ToString();
				}
				else
				{
					updateColumnName = m_data[e.RowIndex][(int)DataIndexes.ColumnName];
				}

				if (newValue != m_previousValue)
				{
					if (m_uidKeyName == updateColumnName && m_refreshUniqueKeyValueHolderControlCallback != null)
					{
						m_refreshUniqueKeyValueHolderControlCallback(newValue, true);
					}
					m_previousValue = newValue;
				}

				// check if the cell we changed should trigger a special change action
				string[] gridTableNames = null;
				if (((DataGridView)g)[NameHeading, e.RowIndex].Value != null && ((DataGridView)g)[ControlHeading, e.RowIndex].Value != null)
				{
					gridTableNames = checkIfChangeTable(((DataGridView)g)[NameHeading, e.RowIndex].Value.ToString(), ((DataGridView)g)[ControlHeading, e.RowIndex].Value.ToString());
				}
				if (gridTableNames != null)
				{
					// remove events from eventHandler to prevent event reentry when we reset another grid
					m_grid.EditingControlShowing -= m_cellControlShowingEventHandler;
					m_grid.CellValidating -= m_cellControlValidatingEventHandler;
					m_grid.CellValidated -= m_cellControlValidatedEventHandler;

					// change the table in the affected grid, reseting it
					((NameValueGrid)m_affectedGrids[gridTableNames[0]]).changeTable(gridTableNames[1]);

					// add our events back to this grid
					m_grid.EditingControlShowing += m_cellControlShowingEventHandler;
					m_grid.CellValidating += m_cellControlValidatingEventHandler;
					m_grid.CellValidated += m_cellControlValidatedEventHandler;
				}
			}
		}

		private void addRowsAndInitCells()
		{
			Hashtable validValues = m_db.getValidValuesForAllEnumColumnsInTable(m_tableName);

			if (getMode() != "auto")
			{
				List<string> dataKeys = new List<string>(m_data.Length);
				string[] dbKeys = new string[validValues.Count];
				validValues.Keys.CopyTo(dbKeys, 0);
				try
				{
					for (int j = 0; j < m_data.Length; j++)
						dataKeys.Add(m_data[j][(int)DataIndexes.ColumnName]);
				}
				catch (Exception ex)
				{
					log.Error(ex.Message);
				}

				for (int k = 0; k < dbKeys.Length; k++)
				{
					string key = dbKeys[k];
					if (dataKeys.Contains(key))
					{
						int idx = dataKeys.IndexOf(key);

						try 
						{
							if (m_data[idx][(int)DataIndexes.ControlType] != "ComboBox")
							{
								continue;
							}

							List<string> ids = new List<string>(0);
							List<string> names = new List<string>(0);

							/// @todo cache these so we don't refetch from db if multiple comboboxes
							/// use same options.
							m_db.getColumn(ref ids, m_data[idx][(int)DataIndexes.PopulateIdColumnName], m_data[idx][(int)DataIndexes.PopulateTableName]);
							m_db.getColumn(ref names, m_data[idx][(int)DataIndexes.PopulateNameColumnName], m_data[idx][(int)DataIndexes.PopulateTableName]);

							m_controlAttributes[idx] = new List<string>[2];
							m_controlAttributes[idx][0] = ids;
							m_controlAttributes[idx][1] = names;

							validValues[key] = names;
						}
						catch (Exception ex)
						{
							log.Error(ex.Message);
						}

					}
					else
					{
						validValues.Remove(key);
					}
				}
			}

			// initialize names, controls, and values in grid using data from database
			string[] columnNames;

			if (m_data.Length == 0)
			{
				columnNames = new string[validValues.Keys.Count];
				validValues.Keys.CopyTo(columnNames, 0);
			}
			else
			{
				columnNames = new string[m_data.Length];
				try
				{
					for (int j = 0; j < m_data.Length; j++)
						columnNames[j] = m_data[j][(int)DataIndexes.ColumnName];
				}
				catch (Exception ex)
				{
					log.Error(ex.Message);
				}

			}

			if (m_controlAttributes.Length < columnNames.Length)
				m_controlAttributes = new List<string>[columnNames.Length][];

			int i = 0;
			foreach (string columnName in columnNames)
			{
				if (getExcludes().Contains(columnName))
				{
					// if this column name is marked for exclusion, skip it.
					Array.Resize(ref m_controlAttributes, m_controlAttributes.Length - 1);
					continue;
				}

				// add row to grid
				m_grid.Rows.Add();

				try
				{
					// set name cell
					if (m_data.Length > i && m_data[i] != null && m_data[i].Length > (int)DataIndexes.Label && m_data[i][(int)DataIndexes.Label] != null)
						m_grid[NameHeading, i].Value = m_data[i][(int)DataIndexes.Label];
					else
						m_grid[NameHeading, i].Value = columnName;

					// set value cell
					List<string> values = (List<string>)validValues[columnName];
					if (values.Count > 0)
					{
						DataGridViewComboBoxCell c = new DataGridViewComboBoxCell();
						c.Items.AddRange(values.ToArray());
						if (m_controlAttributes.Length > i)
						{
							if (m_controlAttributes[i] == null)
							m_controlAttributes[i] = new List<string>[2];
							m_controlAttributes[i][1] = values;
						}
						else
							log.Error("not enough attributes.");
						m_grid[ControlHeading, i] = c;
					}

					m_grid[OrderHeading, i].Value = i;

					// set both cells' bg colors to disabled
					m_grid[NameHeading, i].Style.BackColor = System.Drawing.Color.LightGray;
					m_grid[ControlHeading, i].Style.BackColor = System.Drawing.Color.LightGray;
				}
				catch (Exception ex)
				{
					log.Error(ex.Message);
				}

				i++;
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////
		/// add Controls (if needed) and events (which launch database actions) to the grid, 
		/// which handle each row's Control.
		///
		/// TextBox and ComboBox Controls are supported. Enter Focus, Validating, and 
		/// Validated events are supported. An anonymous delegate is created and added 
		/// to the Grid for each Event. These delegates handle TextBox and ComboBox
		/// Controls in each row of the grid.
		///
		/// @param uniqueKeyValueHolderControl another control. check it for the currently 
		///        selected unique key value.
		/// @param uniqueKeyValueHolderControlCallback function to callback to pass back the 
		///        new selected item in case this is a unique key selection control
		/// @todo move anonymous delegates out into separate functions?
		/// @todo functionalize! this method is too big.
		public void addUpdateEvents(getControlsCurrentValueCallbackType uniqueKeyValueHolderControlCallback, refreshCallbackType refreshUniqueKeyValueHolderControlCallback)
		{
			// copy params if needed
			if (m_uniqueKeyValueHolderControlCallback == null)
			{
				m_uniqueKeyValueHolderControlCallback = uniqueKeyValueHolderControlCallback;
			}
			if (m_refreshUniqueKeyValueHolderControlCallback == null)
			{
				m_refreshUniqueKeyValueHolderControlCallback = refreshUniqueKeyValueHolderControlCallback;
			}

			// remove any previously added event handlers from our cells' controls' events
			m_grid.EditingControlShowing -= m_cellControlShowingEventHandler;
			m_grid.CellValidating -= m_cellControlValidatingEventHandler;
			m_grid.CellValidated -= m_cellControlValidatedEventHandler;

			// declare eventhandlers which will handle db update events on each control in grid
			m_cellControlShowingEventHandler =
				new DataGridViewEditingControlShowingEventHandler(cellControlShowing);
			m_cellControlValidatingEventHandler =
				new DataGridViewCellValidatingEventHandler(cellControlValidating);
			m_cellControlValidatedEventHandler =
				new DataGridViewCellEventHandler(cellControlValidated);

			addRowsAndInitCells();

			// add events to grid to handle db update events on each control in grid
			m_grid.EditingControlShowing += m_cellControlShowingEventHandler;
			m_grid.CellValidating += m_cellControlValidatingEventHandler;
			m_grid.CellValidated += m_cellControlValidatedEventHandler;

			if (m_singleRowTableMode)
			{
				refresh();
			}

		} // end addUpdateEvents()

		private NameValueGrid findNameValueGrid(string name, NameValueGrid[] grids)
		{
			NameValueGrid result = null;
			for (int i = 0; i < grids.Length; i++)
			{
				if (grids[i].getName() == name)
				{
					return grids[i];
				}
			}
			return result;
		}

		private string getName()
		{
			return (string)m_options["name"];
		}

		private string[] checkAndChangeTable(string name, string value)
		{
			return checkIfChangeTable(name, value, true);
		}

		private string[] checkIfChangeTable(string name, string value)
		{
			return checkIfChangeTable(name, value, false);
		}

		private string[] checkIfChangeTable(string name, string value, bool makeChange)
		{
			string[] result = null;
			// check for special Change*OnData Options
			if (m_options.ContainsKey("changeTableOnDataInField"))
			{
				Hashtable change = (Hashtable)m_options["changeTableOnDataInField"];
				if (change.ContainsKey(name))
				{
					Hashtable values = (Hashtable)(change[name]);
					if (values.ContainsKey(value))
					{
						result = (string[])values[value];
						if (makeChange)
						{
							((NameValueGrid)m_affectedGrids[result[0]]).changeTable(result[1]);
						}
					}
				}
			}
			return result;
		}

		public void changeTable(string newTableName)
		{
			if (!m_active)
			{
				if (newTableName == null)
					return;
			}
			else if (newTableName == null || newTableName == m_tableName)
			{
				if (newTableName == null)
				{
					deactivate();
					clear();
				}
				return;
			}

			m_tableName = newTableName;
			if (m_uniqueKeyValueHolderControlCallback != null && m_refreshUniqueKeyValueHolderControlCallback != null)
			{
				m_grid.Rows.Clear();
				addUpdateEvents(m_uniqueKeyValueHolderControlCallback, m_refreshUniqueKeyValueHolderControlCallback);
				string uidValue = m_uniqueKeyValueHolderControlCallback(m_uidKeyName);
				if (uidValue != null)
				{
					refresh(uidValue);
				}
			}
		}

		public void refresh()
		{
			refresh(null, false);
		}

		public void refresh(string uidKeyValue)
		{
			refresh(uidKeyValue, false);
		}

		//////////////////////////////////////////////////////////////////////////////////////
		/// refresh the values set in each control/row of the name value grid, based on a UID.
		///
		/// @param uidKeyValue value for unique key for setting all values on each row/control
		/// @todo handle db errors
		/// @todo combine single db get loops into one statement (Add getmultiple to DBHelper?)
		public void refresh(string uidKeyValue, bool bipassTriggers)
		{
			bool dataFound = false;

			if (m_data.Length == 0) // data not provided
			{
				int error = -1;
				string[] results = new string[m_grid.Rows.Count];
				string[] columnNames = new string[m_grid.Rows.Count];

				for (int i = 0; i < m_grid.Rows.Count; i++)
				{
					columnNames[i] = m_grid[NameHeading, i].Value.ToString();
				}
				error = m_db.getMultipleFromDb(results, columnNames, m_tableName, m_uidKeyName, uidKeyValue);
				if (error < 1)
				{
					for (int i = 0; i < m_grid.Rows.Count; i++)
					{
						if (results[i] != null)
						{
							dataFound = true;
						}
						else
						{
							results[i] = "";
						}
						m_grid[ControlHeading, i].Value = results[i];
						checkAndChangeTable(columnNames[i], results[i]);
					}
				}
				else
				{
					for (int i = 0; i < m_grid.Rows.Count; i++)
					{
						m_grid[ControlHeading, i].Value = "";
						checkAndChangeTable(columnNames[i], "");
					}
				}
			}
			else // data provided
			{
				int error = -1;
				string[] results = new string[m_data.Length];
				string[] columnNames = new string[m_data.Length];
				string[] gridLabels = new string[m_data.Length];

				for (int i = 0; i < m_data.Length; i++)
				{
					int dataIndex = (int)m_grid[OrderHeading, i].Value;
					columnNames[i] = m_data[dataIndex][(int)DataIndexes.ColumnName];
					gridLabels[i] = m_data[dataIndex][(int)DataIndexes.Label];
				}
				error = m_db.getMultipleFromDb(results, columnNames, m_tableName, m_uidKeyName, uidKeyValue);
				if (error < 1)
				{
					for (int i = 0; i < m_grid.Rows.Count; i++)
					{
						if (results[i] != null)
						{
							dataFound = true;
						}
						else
						{
							results[i] = "";
						}

						int dataIndex = (int)m_grid[OrderHeading, i].Value;

						// if this control item is configured to be updated by changing the id instead 
						// of the name, we just got the id here. So we do one more step to convert it to the name.
						if (m_controlAttributes[dataIndex] != null &&
							m_controlAttributes[dataIndex][0] != null &&
							m_data[dataIndex][(int)DataIndexes.ControlType] == "ComboBox" &&
							m_data[dataIndex][(int)DataIndexes.ControlOptionString] == "byId")
						{
							if (m_controlAttributes[dataIndex][0] != null)
							{
								int idx = m_controlAttributes[dataIndex][0].IndexOf(results[i]);
								results[i] = ((DataGridViewComboBoxCell)m_grid[ControlHeading, i]).Items[idx].ToString();
							}
						}
						m_grid[ControlHeading, i].Value = results[i];
						if (!bipassTriggers)
						{
							checkAndChangeTable(gridLabels[i], results[i]);
						}
					}
				}
				else
				{
					for (int i = 0; i < m_grid.Rows.Count; i++)
					{
						m_grid[ControlHeading, i].Value = "";
						if (!bipassTriggers)
						{
							checkAndChangeTable(gridLabels[i], "");
						}
					}
				}
			}

			// reset previous value to the currently selected cell if a Control cell is selected.
			if (m_grid.SelectedCells.Count > 0 && m_grid.SelectedCells[0].OwningColumn.Name == ControlHeading)
			{
				m_previousValue = m_grid.SelectedCells[0].FormattedValue != null ? m_grid.SelectedCells[0].FormattedValue.ToString() : null;
			}

			if (m_hiddenRows.Keys.Count > 0)
			{
				// populate hidden values
				string[] keys = new string[m_hiddenRows.Keys.Count];
				string[] results = new string[keys.Length];
				m_hiddenRows.Keys.CopyTo(keys, 0);
				int error = -1;
				error = m_db.getMultipleFromDb(results, keys, m_tableName, m_uidKeyName, uidKeyValue);
				for (int i = 0; i < keys.Length; i++)
				{
					m_hiddenRows[keys[i]] = results[i];
					if (!bipassTriggers)
					{
						checkAndChangeTable(keys[i], results[i]);
					}
				}
			}
			// a uid is selected, so activate the grid if we filled in any cells above
			if (dataFound)
			{
				activate();
			}
			else
			{
				deactivate();
			}

		} // end refresh()

		//////////////////////////////////////////////////////////////////////////////////
		/// gets a combobox items id value from a name value.
		///
		/// through the internal m_controlAttributes[][] Lists.
		/// @param rowIndex the 0 based index of the row/control to look at
		/// @param name the name value to convert
		/// @return the corresponding id value
		/// @todo handle error
		public string convertComboItemNameToId(int rowIndex, string name)
		{
			string result = null;

			int dataIndex = (int)m_grid[OrderHeading, rowIndex].Value;
			int idx = m_controlAttributes[dataIndex][1].IndexOf(name);

			result = m_controlAttributes[dataIndex][0][idx];
			return result;
		}

		//////////////////////////////////////////////////////////////////////////////////////
		/// change a grid's cells' background colors.
		///
		/// @param grid the DataGridView to change
		/// @param color the new color to set
		private void changeGridBgColor(DataGridView grid, System.Drawing.Color color)
		{
			int i, j;
			for (i = 0; i < m_grid.Rows.Count; i++)
			{
				for (j = 0; j < grid.Rows[i].Cells.Count; j++)
				{
					grid[j, i].Style.BackColor = color;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////////////////
		/// clear the grid.
		///
		/// set all value fields to empty
		private void clear()
		{
			foreach (DataGridViewRow r in m_grid.Rows)
				r.Cells[ControlHeading].Value = null;
		}

		//////////////////////////////////////////////////////////////////////////////////////
		/// deactivate the grid.
		///
		/// set an internal flag to disallow validating events,
		/// set grid to readonly,
		/// change cells' bg color to gray
		private void deactivate()
		{
			m_active = false;
			m_grid.Columns[NameHeading].ReadOnly = true;
			m_grid.Columns[ControlHeading].ReadOnly = true;
			changeGridBgColor(m_grid, System.Drawing.Color.LightGray);
		}

		//////////////////////////////////////////////////////////////////////////////////////
		/// activate the grid.
		///
		/// set an internal flag to allow validating events,
		/// set grid to not readonly,
		/// change cells' bg color to white
		private void activate()
		{
			m_active = true;
			m_grid.Columns[NameHeading].ReadOnly = true;
			m_grid.Columns[ControlHeading].ReadOnly = false;
			if (m_grid.SelectedCells.Count == 1 && m_grid.SelectedCells.Contains(m_grid[NameHeading, 0]))
			{
				m_grid[NameHeading, 0].Selected = false;
				m_grid[ControlHeading, 0].Selected = true;
			}
			changeGridBgColor(m_grid, System.Drawing.Color.White);
		}

		/////////////////////////////////////////////////////////////////////////////////////
		/// gets the value for a name.
		/// 
		/// @param name the name to return a value for
		/// @return the corresponding value, or null if name was not found
		public string getControlsCurrentValue(string name)
		{
			string result = null;
			if (m_hiddenRows.ContainsKey(name))
			{
				result = (string)m_hiddenRows[name];
			}
			else
			{
				for (int i = 0; i < m_grid.Rows.Count; i++)
				{
					if (m_grid[NameHeading, i].Value.ToString() == name)
					{
						result = m_grid[ControlHeading, i].Value.ToString();
					}
				}
			}
			return result;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		/// gets the DataGridViewCell of the Value Control for a name.
		/// 
		/// won't search through hidden rows.
		/// @param name the name to return a Cell for
		/// @return the corresponding Cell, or null if name was not found
		public DataGridViewCell getControl(string name)
		{
			DataGridViewCell result = null;
			for (int i = 0; i < m_grid.Rows.Count; i++)
			{
				if (m_grid[NameHeading, i].Value.ToString() == name)
				{
					result = m_grid[ControlHeading, i];
					break;
				}
			}
			return result;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		/// UID key/column name getter
		/// 
		/// @return the uid column name for this NVGrid
		public string getUidColumnName()
		{
			return m_uidKeyName;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		/// gets the mode/init method for this NVGrid (eg "auto").
		/// 
		/// @return the mode/init method for this NVGrid (eg "auto").
		public string getMode()
		{
			string result = "";
			if (m_options.ContainsKey("initMethod"))
			{
				result = m_options["initMethod"].ToString();
			}
			return result;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		/// gets List of excluded column names for this NVGrid.
		/// 
		/// These columns are ignored when the grid is populated in auto mode.
		/// @return List of excluded column names for this NVGrid.
		public List<string> getExcludes()
		{
			List<string> result = new List<string>(0);
			if (m_options.ContainsKey("excludeColumns"))
			{
				result = (List<string>)m_options["excludeColumns"];
			}
			return result;
		}

		public int convertToDataIndex(int rowIndex)
		{
			return (int)m_grid[OrderHeading, rowIndex].Value;
		}

		public int findRowById(int dataId)
		{
			foreach (DataGridViewRow r in m_grid.Rows)
				if (dataId == (int)r.Cells[OrderHeading].Value)
					return r.Index;

			return -1;
		}

		private void grid_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.RowIndex >= m_grid.Rows.Count)
				return;     // Filter out, Header and Adder Rows
			if (e.ColumnIndex != m_grid.Columns[ControlHeading].Index)
				return;     // Filter out other columns
			if (!((Object)m_grid.EditingControl is ComboBox))
			    return;		// Filter out non-ComboBox Cells

			m_grid.BeginEdit(true);
			ComboBox comboBox = (ComboBox)m_grid.EditingControl;
			comboBox.DroppedDown = true;
		}
	}
}
*/