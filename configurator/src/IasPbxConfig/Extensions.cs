/*
Extensions.cs
Intellasoft Configurator.
 
@author Mike Jacobi for Intellasoft
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Lib;
using Lib.InheritedCombo;
using log4net;

namespace IasPbxConfig
{
	public class Extensions : TableForm
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string Title = "Extensions";

		/// database table name associated with trunks
		private const string TableName = "Asterisk.v_extensions";
		/// unique key column name in TableName table
		private const string KeyName = "extension";

		/// list of database phonegroup Ids corresponding to extPhoneGroupComboBox
		private List<string> m_phonegroupIds;
		
		/// db column names to show in our grid, along with heading names for each
		public static Dictionary<string, string> VisibleColumns = new Dictionary<string, string>
		{
			{ "extension", "Extension" },
			{ "device_type", "Device Type" },
			{ "callerid_name", "Caller ID Name" },
			{ "class_of_service", "Class of Service" }
		};

		private static Hashtable SpecialControls = new Hashtable()
		{
			{ "class_of_service", new Hashtable() 
				{ 
					{ "ControlType", "ComboBox" },
					{ "TableName", "Asterisk.v_cos" },
					{ "UidName", "cos_name" }
				}
			},
			{ "device_type", new Hashtable() 
				{ 
					{ "ControlType", "ComboBox" },
					{ "TableName", "Asterisk.v_device_types" },
					{ "UidName", "device_type" }
				}
			}
		};

		private static List<string> RequiredColumnNames = new List<string> {"phonegroup_id"};

		private MultiColumnComboBox phonegroupMultiComboBox;

		public Extensions(DbHelper db)
		{
			// init parent TableForm; pass false for doRefresh because setting our mccb value below will refresh 
			// it.
			init(db, Title, TableName, KeyName, VisibleColumns, false, RequiredColumnNames, SpecialControls);

			/// Keep a list of ids for phonegroups which corressponds to the list in the combo, so we can 
			/// reference it when a row is selected on the Combo
			/// @todo: have mccb's initdata() grab and save ids so this will no longer be needed
			m_phonegroupIds = new List<string>();
			int error = db.getColumn(ref m_phonegroupIds, "phonegroup_id", "Asterisk.v_phonegroups");
			if (error > 0)
				log.Error("");

			/// @todo research a nicer way to add this combo. 
			/// 
			/// The problem here is all the ui elements of the form belong to the inherited parent class,
			/// TableForm.
			/// 
			/// The original plan was to add it in the extension UI builder, but unfortunately visual inheritance 
			/// for TableLayoutPanels is not supported by .net (at least in 2.0, not sure about newer versions.),
			/// even if the the TableLayoutPanel's Modifier property is set to Public. 
			/// 
			/// Possible solutions are to move the MCCCombo directly into the form, or into a regualr panel, 
			/// which supports UI/Visual Inheritance. Unfortunately that creates a new problem of how to resize 
			/// our subform and grid properly when the window size changes.
			/// 
			/// The current solution is adding the MCCB manually programmatically below, which works but is not
			/// as clean as keeping ui elements in the UI builder.
			phonegroupMultiComboBox = new MultiColumnComboBox();
			phonegroupMultiComboBox.FormattingEnabled = true; // is this needed?
			phonegroupMultiComboBox.RowSelected += new Lib.InheritedCombo.RowSelectedEventHandler(phonegroupMultiComboBox_RowSelected);
			//phonegroupMultiComboBox.Text = "Select a Phonegroup";
			string[][] initData = null;
			db.getTable(ref initData, "Asterisk.v_phonegroups", PhoneGroups.VisibleColumns, "phonegroup_name");
			phonegroupMultiComboBox.initWithData(initData);

			// select the first option if present
			if (phonegroupMultiComboBox.getCount() > 0)
			{
				phonegroupMultiComboBox.selectByIndex(0); // this will also refresh the grid
			}
			else
				phonegroupMultiComboBox.Text = "No Phonegroups found";
			
			// add it to an empty row set to Autosize (in TableForm's UIBuilder)
			tableFormTableLayoutPanel.Controls.Add(phonegroupMultiComboBox, 0, 1); 
		}

		public override void trkGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && ((DataGridView)sender)[KeyName, e.RowIndex].Value != null)
				Program.m_mainForm.TabManager.addFormInTab((int)IASoftSetupForms.Extd, new string[] { KeyName }, new string[] { ((DataGridView)sender)[KeyName, e.RowIndex].Value.ToString() });
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// Extensions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(749, 324);
			this.Name = "Extensions";
			this.ResumeLayout(false);

		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for extPhonegroupComboBox's SelectedIndexChanged event; refreshes 
		/// grid.
		///
		/// ... through call to refresh().
		/// @param sender extPhonegroupComboBox
		/// @param e EventArgs
		private void phonegroupMultiComboBox_RowSelected()
		{
			// call TableForm's refresh() to update the table (grid)
			refresh();
		}

		/// this refresh overrides TableForm's refresh because we need to look at the 
		/// phonegroup dropdown value before refreshing the grid.
		protected override void refresh()
		{
			Db.getTable(
				Grid,
				TableName,
				//new string[] { "phonegroup_id" },
				//new string[] { m_phonegroupIds[phonegroupMultiComboBox.getSelectedIndex()] },
				new Hashtable() { { "phonegroup_id", new List<string>() { m_phonegroupIds[phonegroupMultiComboBox.getSelectedIndex()] } } },
				VisibleColumns,
				SpecialControls,
				KeyName
			);
			loadColumnSortsetting();
		}
	}
}
