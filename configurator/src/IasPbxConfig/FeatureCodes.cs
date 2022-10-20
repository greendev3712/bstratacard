/*
FeatureCodes.cs
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
	/// The Feature Codes Form; gives view and ability to modify Feature Codes 
	///
	public partial class FeatureCodes : ParamForm
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// database table name associated with trunks
		private const string TableName = "Asterisk.v_feature_codes";
		///// unique key column name in TableName table
		//private const string KeyName = null;

		/// Name-Value Grid Helper object for trunk general options
		private NameValueGrid m_generalOptionsGrid;

		/// some NVGrid options for the general option NameValueGrid
		private static Hashtable GeneralOptionOptions = new Hashtable()
		{
			{"name", "Feature Codes"},
			{"initMethod", "auto"},
			{"SingleRowTable", "True"}
		};

		private static DbHelper m_db;

		//////////////////////////////////////////////////////////////////////////////////
		/// Constructor.
		///
		/// allocates members.
		/// calls load(), which, in turns, populates all implemented Controls
		public FeatureCodes(DbHelper db)
		{
			log.Debug("");
			m_parameterNames = new List<string>(0);
			m_parameterValues = new List<string>(0);
			InitializeComponent();
			m_db = db;

			load();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// called from end of constructor; loads our data from db into the form.
		///
		/// Populates all implemented Controls (except for uid comboBox) 
		/// through call to DBHelper's getColumn(). 
		/// 
		/// registers anonymous callbacks to events on implemented controls with calls to
		/// DBHelper's addUpdateEventsToControl(). This makes user changes on the controls
		/// make immediate database changes.
		/// 
		/// Populates uid-jump-to comboBox (trunks) through call to refreshList().
		/// 
		/// @todo handle errors on getColumn calls like Extension.cs's load()
		private void load()
		{
			ButtonRefreshCallback = refreshDetails;

			m_generalOptionsGrid = new NameValueGrid(m_db, generalOptionsGrid, GeneralOptionOptions, null, null, TableName, null);
			m_generalOptionsGrid.addUpdateEvents(null, null);
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
			m_generalOptionsGrid.refresh(null);
		}


	} // end class TrunkDetails

}
