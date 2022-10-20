using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Lib
{
	/// Adds another layer above tabs.
	/// Manages a list of Tab sets. aka TabControls. aka TabManagers. aka used loosely here.
	public class TabSetManager
	{
		private TabManager m_tm;
		private Hashtable m_tms;

		private DockStyle TcDockStyle;
		private Point TcLocation;
		private Size TcSize;

		private string[] FormNames;
		private TabManager.initFormCallbackType InitFormCallback;
		private TabManager.getDataCallbackType GetDataCallback;

		private string BaseTmId;

		public TabSetManager(string newId, TabControl tc, string[] formNames, TabManager.initFormCallbackType initForm, TabManager.getDataCallbackType getData)
		{
			BaseTmId = newId;

			TcDockStyle = tc.Dock;
			TcLocation = tc.Location;
			TcSize = tc.Size;

			FormNames = formNames;
			InitFormCallback = initForm;
			GetDataCallback = getData;

			tc.Name = newId;
			m_tm = new TabManager(tc, formNames, initForm, getData);
			m_tms = new Hashtable(5); // guess at 5 sets
			m_tms.Add(newId, m_tm);
		}

		public TabManager TabManager
		{
			get
			{
				return m_tm;
			}
		}

		public bool switchToTabSet(string id)
		{
			// if this tabset doesn't exist exit with error
			if (!m_tms.ContainsKey(id))
				return true;

			// if this tabset is already the current one, exit normally
			if (m_tm == m_tms[id])
				return false;

			Control parent = m_tm.TabControl.Parent;
			parent.Controls.Remove(m_tm.TabControl);
			m_tm = (TabManager)m_tms[id];
			parent.Controls.Add(m_tm.TabControl);
			
			return false;
		}

		public bool addNewTabSet(string newId)
		{
			if (m_tms.ContainsKey(newId))
				return true;

			TabControl tc = new TabControl();
			tc.Dock = TcDockStyle;
			tc.Location = TcLocation;
			tc.Name = newId;
			tc.SelectedIndex = 0;
			tc.Size = TcSize;
			tc.TabIndex = 0;

			TabManager tm = new TabManager(tc, FormNames, InitFormCallback, GetDataCallback);
			m_tms.Add(newId, tm);

			return false;
		}

		// proabably not needed
		//public bool addAndSwitchToNewTabSet(string newId)
		//{
		//    if (addNewTabSet(newId))
		//        return true;
		//    switchToTabSet(newId);
		//    return false;
		//}

		public bool switchToTabSetAndCreateIfNeeded(string newId)
		{
			addNewTabSet(newId);
			switchToTabSet(newId);
			return false;
		}
		
		public bool removeTabSet(string key)
		{
			// if this key doesn't exist, return with error
			if (!m_tms.ContainsKey(key))
				return true;

			// select any other TabManager to switch to. Prefer not switching to orignal passed in TC (uibase)
			string newKey = BaseTmId;
			foreach (string k in m_tms.Keys)
			{
				if (k != key && k != BaseTmId)
				{
					newKey = k;
					break;
				}
			}

			// switch to it
			switchToTabSet(newKey);

			// finally, remove the 
			m_tms.Remove(key);

			return false;
		}
	}
}
