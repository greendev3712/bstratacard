/*
ExtensionDetails.cs
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
using Lib;

namespace IasPbxConfig
{
	//////////////////////////////////////////////////////////////////////////////////
	/// The Extension Details Form; gives view and ability to modify all of an
	/// extension's details.
	///
	/// Allows picking an extension from a full list or a filtered list based on a 
	/// selected phonegroup. Editable details: extension, phonegroup, class of service,
	/// device type, caller id name. Also allows adding a new extension, optionally
	/// copying details from the currently selected extension.
	public partial class ExtensionDetails : ParamForm
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// database table name associated with extensions
		private const string TableName = "Asterisk.v_extensions";
		/// unique key column name in TableName table
		private const string KeyName = "extension";

		/// Name-Value Grid Helper object for currently selected extension's general options
		private NameValueGrid m_generalOptionsGrid;
		/// Name-Value Grid Helper object for currently selected extension's device options
		private NameValueGrid m_deviceOptionsGrid;
		/// Name-Value Grid Helper object for currently selected extension's voicemail options
		private NameValueGrid m_voicemailOptionsGrid;

		/// some NVGrid options for the device option NameValueGrid
		private static Hashtable VoicemailOptionOptions = new Hashtable()
		{
			{"name", "Extension Details Voicemail Options"},
			{"initMethod", "auto"},
			{"excludeColumns", new List<string> {"uniqueid", "device_id"}}
		};

		/// some NVGrid options for the device option NameValueGrid
		private static Hashtable DeviceOptionOptions = new Hashtable()
		{
			{"name", "Extension Details Device Options"},
			{"initMethod", "auto"},
			{"excludeColumns", new List<string> {"device_id"}}
		};
		/// data for populating device option NameValueGrid;
		/// empty because we are using auto initMethod above
		//private static string[][] DeviceOptionControlData = 
		//{
		//};

		/// some NVGrid options for the general option NameValueGrid
		private static Hashtable GeneralOptionOptions = new Hashtable()
		{
			{"name", "Extension Details General Options"},
			{"HiddenRows", new string[] {"device_id"}},
			{"changeTableOnDataInField", new Hashtable()
				{
					{"Type", new Hashtable()
						{
							{"SIP", new string[] {"Extension Details Device Options", "Asterisk.v_devices_sip_attributes"}},
							{"IAX2", new string[] {"Extension Details Device Options", "Asterisk.v_devices_iax_attributes"}}
						}
					},
					{"Voicemail", new Hashtable()
						{
							{"yes", new string[] {"Extension Details Voicemail Options", "Asterisk.v_voicemail"}},
							{"no", new string[] {"Extension Details Voicemail Options", null}}
						}
					}
				}
			}
		};
		/// data for populating general option NameValueGrid
		private static string[][] GeneralOptionControlData = 
		{
			// Label, Control Type, SQL Update Column name for each row in the grid
			new string[] {"Extension", "TextBox", KeyName},
			new string[] {
				"Phonegroup", 
				"ComboBox", 
				"phonegroup_id", 
				"Asterisk.v_phonegroups", 
				"phonegroup_id", 
				"phonegroup_name",
				"byId" //(default)
			},
			new string[] {
				"Class of service", 
				"ComboBox", 
				"class_of_service_id", 
				"Asterisk.v_cos", 
				"cos_id", 
				"cos_name",
				"byId" //(default)
			},
			//new string[] {"Description", "Textbox", "trunk_longname"},
			//new string[] {"Capacity", "TextBox", "capacity", "", ""},
			new string[] {
				"Type", 
				"ComboBox", 
				"device_type", 
				"Asterisk.device_types", 
				"device_type_id", 
				"device_type",
				"byName" //(default)
			},
			new string[] {"Caller ID Name", "TextBox", "callerid_name"},
			new string[] {"Voicemail", "TextBox", "voicemail"},
			//new string[] {"Caller ID Number", "TextBox", "callerid_number"},
			//new string[] {
			//	"Trunkhandler", 
			//	"ComboBox", 
			//	"incoming_trunkhandler_id", 
			//	"Asterisk.v_trunk_groups", 
			//	"trunk_group_id", 
			//	"trunk_group_name",
			//	"byId"
			//},
			//new string[] {"Record", "TextBox", "record"}
		};

		/// list of database phonegroup Ids corresponding to extDetailPhonegroupJumpToCombo
		private List<string> m_phonegroupIds;
		/// list of phonegroup names shown in extDetailPhonegroupJumpToCombo
		private List<string> m_phonegroupNames;

		/// list of database Ids corresponding to names in uid-jump-to ComboBox (in this case the extension)
		private List<string> m_uidIds;
		/// list of names in ComboBox (the extension, same as m_uidIds in this case)
		private List<string> m_uidNames;

		private static DbHelper m_db;

		//////////////////////////////////////////////////////////////////////////////////
		/// Constructor.
		///
		/// allocates members.
		/// calls load(), which, in turns, populates all implemented Controls
		public ExtensionDetails(DbHelper db)
		{
			log.Debug("");
			m_parameterNames = new List<string>(0);
			m_parameterValues = new List<string>(0);
			InitializeComponent();
			m_db = db;

			load();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Constructor.
		///
		/// allocates members and copies passed parameters in.
		/// calls load(), which, in turns, populates all implemented Controls
		/// @param parameterNames ref to names of parameters supplied by caller
		/// @param parameterValues ref to corresponding values of parameters.
		public ExtensionDetails(DbHelper db, string[] parameterNames, string[] parameterValues)
		{
			initParameters(parameterNames, parameterValues);
			InitializeComponent();
			m_db = db;

			load();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// called from end of constructor; loads our data from db into the form.
		///
		/// Populates phonegroup-jump-to comboBox, and all other implemented Controls 
		/// through call to DBHelper's getColumn(). 
		/// 
		/// Selects an entry in the phonegroup-jump-to comboBox, which launches event which 
		/// in turn populates uid comboBox (extensions).
		/// 
		/// registers anonymous callbacks to events on implemented controls with calls to
		/// DBHelper's addUpdateEventsToControl(). This makes user changes on the controls
		/// make immediate database changes.
		/// 
		/// @todo handle errors on getColumn calls like Extension.cs's load()
		private void load()
		{
			ParamInitRefreshCallback = refreshList;
			ButtonRefreshCallback = refreshDetails;

			// initialize phone groups comboBox
			m_phonegroupIds = new List<string>(0);
			m_phonegroupNames = new List<string>(0);

			m_db.getColumn(ref m_phonegroupIds, "phonegroup_id", "Asterisk.v_phonegroups");
			m_db.getColumn(ref m_phonegroupNames, "phonegroup_name", "Asterisk.v_phonegroups");

			//            extDetailPhonegroupJumpToCombo.Items.AddRange(m_phonegroupNames.ToArray());

			// add all phonegroups option
			//            extDetailPhonegroupJumpToCombo.Items.Add("All phonegroups");

			string[][] initData = null;
			m_db.getTable(ref initData, "Asterisk.v_phonegroups", PhoneGroups.VisibleColumns, "phonegroup_name");
			phonegroupJumpToMultiCombo.initWithData(initData);

			// auto-select phonegroup if supplied as parameter
			int selectedIdx = m_parameterNames.IndexOf("phonegroup_id");
			if (selectedIdx >= 0) // select passed in phonegroup id
			{
				phonegroupJumpToMultiCombo.selectByIndex(m_phonegroupIds.IndexOf(m_parameterValues[selectedIdx]));
			}
			else if (phonegroupJumpToMultiCombo.getCount() > 0) // autoselect phonegroup if none specified
			{
				if (phonegroupJumpToMultiCombo.getValueAtIndex(phonegroupJumpToMultiCombo.getCount() - 1) == "All phonegroups")
				{
					// select "All phonegroups" if option present
					phonegroupJumpToMultiCombo.selectByIndex(phonegroupJumpToMultiCombo.getCount() - 1);
				}
				else
				{
					// otherwise select 1st one
					phonegroupJumpToMultiCombo.selectByIndex(0);
				}
			}

			m_voicemailOptionsGrid = new NameValueGrid(m_db, voicemailOptionsGrid, VoicemailOptionOptions, null, "device_id", "Asterisk.v_voicemail", null);
			m_deviceOptionsGrid = new NameValueGrid(m_db, deviceOptionsGrid, DeviceOptionOptions, null, "device_id", "Asterisk.v_devices_sip_attributes", null);
			m_generalOptionsGrid = new NameValueGrid(m_db, generalOptionsGrid, GeneralOptionOptions, GeneralOptionControlData, KeyName, TableName, new List<NameValueGrid> { m_deviceOptionsGrid, m_voicemailOptionsGrid });
			m_generalOptionsGrid.addUpdateEvents(new NameValueGrid.getControlsCurrentValueCallbackType(getCurrentUid), new NameValueGrid.refreshCallbackType(refreshList));
			m_deviceOptionsGrid.addUpdateEvents(new NameValueGrid.getControlsCurrentValueCallbackType(m_generalOptionsGrid.getControlsCurrentValue), new NameValueGrid.refreshCallbackType(refreshList));
			m_voicemailOptionsGrid.addUpdateEvents(new NameValueGrid.getControlsCurrentValueCallbackType(m_generalOptionsGrid.getControlsCurrentValue), new NameValueGrid.refreshCallbackType(refreshList));

			refreshList();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// gets the current UID value from the uid control object.
		///
		/// Useful as a callback (why it takes an unused arg). Returns null if nothing is 
		/// selected.
		/// @name UID Column name; not used.
		/// @return value of currently selected UID Control
		/// @todo make new delegate with no args?
		private string getCurrentUid(string name)
		{
			string result = null;
			if (uidJumpToMultiCombo.getSelectedValue() != null)
			{
				result = uidJumpToMultiCombo.getSelectedValue();
			}
			return result;
		}

		private void refreshList()
		{
			refreshList(null, false);
		}
		private void refreshList(string selectedUniqueIdSuggestion)
		{
			refreshList(selectedUniqueIdSuggestion, false);
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// (re)load just the uid-jump-to ComboBox, based on currently selected phonegroup 
		/// in phonegroup-jump-to comboBox.
		/// 
		/// ...through calls do DBHelper's getColumn(). Also selects an item on the 
		/// uid-jump-to ComboBox; this creates an event which in turn makes a 
		/// selection on each implemented detail control.
		/// 
		/// @param selectedUniqueIdSuggestion optional name of item in loaded comboBox to
		///     select.
		/// @todo handle errors on getColumn calls like Extension.cs's load()
		private void refreshList(string selectedUniqueIdSuggestion, bool suppressDetailControlRefresh)
		{
			m_uidIds = new List<string>(0);
			//m_uidNames = new List<string>(0);

			string currentPhonegroupId = phonegroupJumpToMultiCombo.getSelectedValue();

			if (0 <= m_phonegroupIds.IndexOf(currentPhonegroupId)) // if selected phonegroup is in our list
			{
				// populate with extensions based on selected phonegroup id
				m_db.getColumn(ref m_uidIds, KeyName, TableName, "phonegroup_id", currentPhonegroupId);
				//Program.m_db.getColumn(ref m_uidNames, KeyName, TableName, "phonegroup_id", currentPhonegroupId);
			}
			else
			{
				// populate with all uids (extensions)
				m_db.getColumn(ref m_uidIds, KeyName, TableName);
				//Program.m_db.getColumn(ref m_uidNames, KeyName, TableName);
			}
			m_uidNames = m_uidIds;

			// clear the combobox and repopulate it with our new lists
			//uidJumpToCombo.Items.Clear();
			//uidJumpToCombo.Items.AddRange(m_uidNames.ToArray());

			string[][] initData = null;
			//m_db.getTable(ref initData, TableName, null, null, Extensions.VisibleColumns, KeyName, false);
			m_db.getTable(ref initData, TableName, null, Extensions.VisibleColumns, KeyName, false);
			uidJumpToMultiCombo.initWithData(initData);

			// make a selection in our newly populated comboBox based on passed in selectedUniqueIdSuggestion
			bool selected = false;
			if (selectedUniqueIdSuggestion != null)
			{
				int selectedExtensionSuggestionIndex = uidJumpToMultiCombo.getIndexOfValue(selectedUniqueIdSuggestion);
				if (selectedExtensionSuggestionIndex >= 0)
				{
					selected = true;
					uidJumpToMultiCombo.selectByIndex(selectedExtensionSuggestionIndex, suppressDetailControlRefresh);
				}
			}

			// if no selection was made yet, make one based on parameter passed into constructor
			if (!selected)
			{
				int selectedIdx = m_parameterNames.IndexOf(KeyName);
				if (selectedIdx >= 0 && uidJumpToMultiCombo.getSelectedIndex() < 0)
				{
					uidJumpToMultiCombo.selectByIndex(m_uidIds.IndexOf(m_parameterValues[selectedIdx]), suppressDetailControlRefresh);
				}
			}

		}

		//////////////////////////////////////////////////////////////////////////////////
		/// makes selections in nvgrids based on currently selected item in uid-jump-to 
		/// ComboBox.
		///
		/// ...through calls to each nvGrids' refresh()
		/// @todo the below code makes an extra call to db.getmultiple(), because of the 
		/// changetable trigger in m_generalOptionsGrid on m_deviceOptionsGrid; need to 
		/// find a way around this.
		private void refreshDetails()
		{
			// handle case when selected extension is passed in through constructor, since 
			// extension dropdown is filled before nvgrids are alloced.
			/// @todo why not change the order instead of this check? would that cause extra
			/// sql queries?
			if (m_generalOptionsGrid == null)
				return;

			m_generalOptionsGrid.refresh(uidJumpToMultiCombo.getSelectedValue());
			m_deviceOptionsGrid.refresh(m_generalOptionsGrid.getControlsCurrentValue(m_deviceOptionsGrid.getUidColumnName()));
			m_voicemailOptionsGrid.refresh(m_generalOptionsGrid.getControlsCurrentValue(m_deviceOptionsGrid.getUidColumnName()));
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// sanity check user entered new values for a new entry in the database.
		/// 
		/// makes sure none of the values are empty and provides defaults for each detail, 
		/// with calls to database if needed.
		/// @param insertNames ref to names of details to check.
		/// @param insertValues ref to corresponding values of details to check.
		/// @param setAllDefaults if true will ignore current values passed in and set them 
		/// all to defaults.
		/// @return adjusts insertValues to sane values.
		/// @todo add error checking to db calls
		private void checkNewValues(ref string[] insertNames, ref string[] insertValues, bool setAllDefaults)
		{
			int i;
			for (i = 0; i < insertNames.Length; i++)
			{
				if (insertValues[i] == "" || setAllDefaults)
				{
					string newValue = "";
					switch (insertNames[i])
					{
						case "extension":
							insertValues[i] = m_db.getNextAvailableUniqueId(TableName, insertNames[i], "max");
							break;
						case "class_of_service_id":
						case "phonegroup_id":
						case "device_type":
							m_db.getSingleFromDb(ref newValue, "max(" + insertNames[i] + ")", TableName, true);
							insertValues[i] = newValue;
							break;
						case "callerid_name":
							insertValues[i] = "New phone user";
							break;
						default:
							m_db.getSingleFromDb(ref newValue, "min(" + insertNames[i] + ")", TableName, true);
							insertValues[i] = newValue;
							break;
					}
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for comboBox's SelectedIndexChanged event; refreshes the uid-jump-to
		/// comboBox.
		///
		/// ... through call to refreshExtensionList().
		/// @param sender the comboBox
		/// @param e EventArgs
		private void phonegroupJumpToMultiCombo_RowSelected()
		{
			refreshList();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for comboBox's SelectedIndexChanged event; (re)selects the 
		/// appropriate items in all implemented detail Controls.
		///
		/// ... through call to refreshDetails().
		/// @param sender the comboBox
		/// @param e EventArgs
		private void uidJumpToMultiCombo_RowSelected()
		{
			refreshDetails();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for Create New Button Click event; creates a new extension entry in 
		/// the database.
		/// 
		/// ... through call to DBHelper's insertIntoDb(). Makes copy of currently selected 
		/// details or provides new defaults based on Make Copy Of Current checkBox. 
		/// 
		/// Sanity checks new data through checkNewValues(). Handles errors on DB Insert. 
		/// Refreshes the uids-jump-to comboBox on success.
		/// 
		/// @param sender the button
		/// @param e EventArgs
		private void extDetailNewExtensionButton_Click(object sender, EventArgs e)
		{
			string[] insertNames = new string[GeneralOptionControlData.Length];
			string[] tmpValues = new string[GeneralOptionControlData.Length];

			int i;
			for (i = 0; i < insertNames.Length; i++)
			{
				insertNames[i] = GeneralOptionControlData[i][2];
			}

			string errorMessage = "";
			bool setAllDefaults = false;

			int rowIndex = m_generalOptionsGrid.findRowById(0);
			if (copyOfCurrentCheckBox.Checked && 
				generalOptionsGrid["Control", rowIndex].Value != null) // nothing to copy if null
			{
				// uid value
				tmpValues[0] = m_db.getNextAvailableUniqueId(TableName, KeyName, generalOptionsGrid["Control", rowIndex].Value.ToString());
				//tmpValues[1] = typeCombo.SelectedIndex >= 0 ? typeCombo.Items[typeCombo.SelectedIndex].ToString() : "";

				// all other values
				for (i = 1; i < tmpValues.Length; i++)
				{
					rowIndex = m_generalOptionsGrid.findRowById(i);
					tmpValues[i] = generalOptionsGrid["Control", rowIndex].Value.ToString();
				}

				for (i = 1; i < tmpValues.Length; i++)
				{
					///@todo completely internalize convertToDataIndex() to NameValueGrid.cs
					///		Users of NVGrid's should not have to worry about user order/sorting of rows

					// if this control item is configured to be updated by changing the id instead 
					// of the name, change newValue from name to id here
					if (GeneralOptionControlData[i][(int)NameValueGrid.DataIndexes.ControlType] == "ComboBox" &&
					   GeneralOptionControlData[i][(int)NameValueGrid.DataIndexes.ControlOptionString] == "byId")
					{
						//ComboBox cb = (ComboBox)generalOptionsGrid.EditingControl;
						//int idx = cb.Items.IndexOf(tmpValues[i]);
						//tmpValues[i] = m_controlAttributes[row][0][idx];
						rowIndex = m_generalOptionsGrid.findRowById(i);
						tmpValues[i] = m_generalOptionsGrid.convertComboItemNameToId(rowIndex, tmpValues[i]);
					}
				}
			}
			else
			{
				setAllDefaults = true;
			}

			// sanity check and adjust
			checkNewValues(ref insertNames, ref tmpValues, setAllDefaults);

			// db insert
			int dbResult = m_db.insertIntoDb(TableName, insertNames, tmpValues, ref errorMessage);
			if (0 < dbResult)
			{
				Program.m_mainForm.setStatus(errorMessage);
			}
			else
			{
				// refresh uid-jump-to comboBox
				refreshList(tmpValues[0]);
			}
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Callback for Load event; empty method.
		///
		/// @param sender self?
		/// @param e EventArgs
		/// @todo investigate why control never enters this function.
		private void ExtensionDetails_Load(object sender, EventArgs e)
		{

		}

		private void generalOptionsGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			log.Error("DataError on Extd general Options Grid.");
		}

	} // end class ExtensionDetails
} // end namespace IasPbxConfig
