/*
TableForm.cs
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
using log4net;

/*

namespace Lib
{
	//////////////////////////////////////////////////////////////////////////////////
	/// The TableForm Parent Form; shows a DataGridView (trkGrid) with all trunks in database.
	///
	/// Near identical copy of Trunks form. 
	/// Allows adding, editing, and deleting (with delete key) of Route entries. 
	/// Provides buttons for deleting and copying all selected Routes.
	/// @todo: Look into a way to combine common functionalilty into parent class?
	public partial class TableForm : ParamForm
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// pointer to a pre-initialized dbhelper. passed in through constructor. one per
		/// instance. can pass in same one for different instances.
		private DbHelper m_db;

		/// name of this form (appears as big label at the top of the form
		private string m_name;
		/// database table name associated with this form
		private string m_tableName;
		/// unique key column name in TableName table
		private string m_keyName;
		/// db column names to show in our grid, along with heading names for each
		private Dictionary<string, string> m_visibleColumns;

		private List<string> m_requiredColumnNames;

		private Hashtable m_specialControlsData;

		private SortOrder m_sortOrder;
		private string m_sortedColumnId;

		//////////////////////////////////////////////////////////////////////////////////
		/// Constructor.
		///
		/// allocates members.
		/// calls load(), which in turn populates extGrid through a call to refresh().
		public TableForm()
		{
			log.Debug("");

			InitializeComponent();

			grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
			grid.EditMode = DataGridViewEditMode.EditOnEnter;
		}

		public TweakedDataGridView Grid
		{
			get
			{
				return grid;
			}
		}

		public DbHelper Db
		{
			get
			{
				return m_db;
			}
		}

		protected void init(DbHelper db, string name, string tableName, string keyName, Dictionary<string, string> visibleColumns)
		{
			init(db, name, tableName, keyName, visibleColumns, true, null, null);
		}

		protected void init(DbHelper db, string name, string tableName, string keyName, Dictionary<string, string> visibleColumns, Hashtable specialControlsData)
		{
			init(db, name, tableName, keyName, visibleColumns, true, null, specialControlsData);
		}

		protected void init(DbHelper db, string name, string tableName, string keyName, Dictionary<string, string> visibleColumns, bool doRefresh)
		{
			init(db, name, tableName, keyName, visibleColumns, doRefresh, null, null);
		}

		protected void init(DbHelper db, string name, string tableName, string keyName, Dictionary<string, string> visibleColumns, bool doRefresh, List<string> requiredColumnNames)
		{
			init(db, name, tableName, keyName, visibleColumns, doRefresh, requiredColumnNames, null);
		}

		protected void init(DbHelper db, string name, string tableName, string keyName, Dictionary<string, string> visibleColumns, bool doRefresh, List<string> requiredColumnNames, Hashtable specialControlsData)
		{
			m_db = db;
			m_name = name;
			m_tableName = tableName;
			m_keyName = keyName;
			m_visibleColumns = visibleColumns;
			ButtonRefreshCallback = refresh;
			titleLabel.Text = m_name;
			m_requiredColumnNames = requiredColumnNames;
			m_specialControlsData = specialControlsData;
			if (doRefresh)
				refresh();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// called from end of constructor.
		///
		/// Calls refresh() to populate grid. We could call refresh directly from the 
		/// constructor but having a load method keeps a similar pattern to other Forms.
		//private void load()
		//{
		//    ButtonRefreshCallback = refresh;
		//    trkLabel.Text = m_name;
		//    refresh();
		//}

		//////////////////////////////////////////////////////////////////////////////////
		/// (re)populates trkGrid (Grid of Routes).
		/// 
		/// ...through call to DBHelpers getTable(). Called when refresh button is clicked.
		/// @todo handle getTable errors
		protected virtual void refresh()
		{
			m_db.getTable(
				grid,
				m_tableName,
                new Hashtable(),
				m_visibleColumns,
				m_specialControlsData,
				m_keyName
			);
			loadColumnSortsetting();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for Load event; empty method.
		///
		/// @param sender self?
		/// @param e EventArgs
		/// @todo investigate why control never enters this function.
		private void TableForm_Load(object sender, EventArgs e)
		{
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for CellEndEdit event; Inserts or Updates user change into database.
		///
		/// ...through call to DBHelper's reactToGridUpdate().
		/// 
		/// @param sender the trkGrid
		/// @param e DataGridViewCellEventArgs with row and column index of edited cell
		private void trkGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			m_db.reactToGridUpdate(grid, e.RowIndex, e.ColumnIndex, m_tableName, m_keyName);
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for Copy Selected Button's Click event; makes similar but unique 
		/// copies of selected rows.
		///
		/// Handles unique naming through DbHelper's getNextAvailableUniqueId(). Creates 
		/// new entries in database with reactToGridUpdate(). Follow see link for inline 
		/// comments.
		/// @param sender the Button
		/// @param e EventArgs 
		/// @todo handle errors from reactToGridUpdate()
		/// @see Extensions.trkCopySelectedButton_Click()
		private void trkCopySelectedButton_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < grid.SelectedRows.Count; i++)
			{
				Hashtable valuesForRequiredColumns = new Hashtable();
				DataGridViewRow newRow = (DataGridViewRow)grid.Rows[i].Clone();
				for (int j = 0; j < newRow.Cells.Count; j++)
				{
					object newValue = grid.SelectedRows[i].Cells[j].Value;
					string name = grid.Columns[j].Name;
					if (m_requiredColumnNames != null && m_requiredColumnNames.Contains(name))
						valuesForRequiredColumns.Add(name, newValue);
					if (name == m_keyName || (name.EndsWith("_name") && !name.StartsWith("_hidden_")))
					{
						if (newValue != null)
							newValue = (object)m_db.getNextAvailableUniqueId(m_tableName, grid.Columns[j].Name, newValue.ToString());
					}

					if (!grid.Columns[j].Name.StartsWith("_hidden_"))
						newRow.Cells[j].Value = newValue;
				}
				int newRowIndex = grid.Rows.Add(newRow);
				m_db.reactToGridUpdate(grid, newRowIndex, 0, m_tableName, m_keyName, valuesForRequiredColumns);
			}
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for Delete Selected Button's Click event; deletes all selected rows.
		///
		/// ...Through mimicing a user keypress of the delete key. This works because .NET 
		/// already handles creating a UserDeletingRow Event for each selected row. This 
		/// event is handled in trkGrid_UserDeletingRow().
		/// @param sender the Button
		/// @param e EventArgs 
		private void trkDeleteSelectedButton_Click(object sender, EventArgs e)
		{
			deleteSelectedRows();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Delete selected grid row(s) in the database and grid
		///
		/// ...through DBHelper's deleteFromDb(). Check if unique key is present in grid 
		/// first. Delete all rows with one transaction.
		/// @todo check for errors/exceptions
		public void deleteSelectedRows()
		{
			int count = grid.SelectedRows.Count;

			if (count <= 0)
				return;

			string rowWord = count == 1 ? "Row" : "Rows";
			Hashtable data = new Hashtable
			{
				{"Title", "Delete " + rowWord},
				{"Controls", new Dictionary<string, Hashtable> 
					{
						{"heading", new Hashtable()
							{
								{"Type", "Label"},
								{"Label", count.ToString() + " " + rowWord.ToLower() + " will be deleted. Are you sure?"}
							}
						}
					}
				}
			};
			PopupQuery areYouSure = new PopupQuery(data);
			// create popup query window and block until it's closed
			if (DialogResult.OK != areYouSure.ShowDialog())
				return;

			List<string> toDelete = new List<string>(3);

			foreach (DataGridViewRow r in grid.SelectedRows)
			{
				if (r.Cells[r.Cells.Count - 1].Value != null && r.Cells[r.Cells.Count - 1].Value.ToString() != "")
					toDelete.Add(r.Cells[r.Cells.Count - 1].Value.ToString());
				grid.Rows.Remove(r);
			}

			if (toDelete.Count > 0)
			{
				m_db.deleteFromDb(m_tableName, new string[] { m_keyName }, toDelete.ToArray(), true, true);
				toDelete.Clear();
			}
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for CellDoubleClick event; launches Trkd (Trunk Details) form.
		///
		/// Opens Trkd form in new tab, passing the clicked Trk Id
		/// @param sender the grid
		/// @param e DataGridViewCellEventArgs with row and column index of clicked cell
		/// @todo handle error opening new form?
		public virtual void trkGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			// over ride this method in derived classes to handle cell doubleclick event
		}

		private void trkGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			log.Error("DataError: " + e.Exception.Message + (e.Exception.InnerException == null ? "" : "; " + e.Exception.InnerException.Message));
		}

		private void grid_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count)
				return;     // Filter out, Header and Adder Rows
			if (e.ColumnIndex < 0 || e.ColumnIndex >= grid.Columns.Count)
				return;     // Filter out bad column indexes
			if (!((Object)grid.EditingControl is ComboBox))
				return;		// Filter out non-ComboBox Cells

			grid.BeginEdit(true);
			ComboBox comboBox = (ComboBox)grid.EditingControl;
			comboBox.DroppedDown = true;
		}

		protected void loadColumnSortsetting()
		{
			if (m_sortedColumnId != null)
			{
				DataGridViewColumn sortedColumn = grid.Columns[m_sortedColumnId];
				grid.Sort(sortedColumn, m_sortOrder == SortOrder.Ascending ? 
										ListSortDirection.Ascending : 
										ListSortDirection.Descending);
			}
		}

		private void saveColumnSortsetting()
		{
			m_sortOrder = grid.SortOrder;
			m_sortedColumnId = m_sortOrder == SortOrder.None ? 
									null : 
									grid.SortedColumn.Name;
		}

		private void grid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			saveColumnSortsetting();
		}
	}
}
*/