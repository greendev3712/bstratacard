using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using log4net;
using Lib;
using System.Drawing;

namespace Lib
{
	public class TabManager
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Dictionary<string, Form> m_openForms;
		private bool m_doNotSaveNextPreviousTab;
		private List<string> m_previouslySelectedTabsKeys;

		/// aka Tab Names
		private string[] m_formNames;

		private TabControl m_tc;

		/////////////////////////////////////////////////////////////////////////////////////
		/// delegate description for initForm callback.
		///
		/// Used to tell child form to refreshList when asked by client via initParameters().
		public delegate Form initFormCallbackType(int formId, string[] parameterNames, string[] parameterValues);

		public delegate object getDataCallbackType(string key);

		private initFormCallbackType m_initFormCallback = null;

		private getDataCallbackType m_getDataCallback = null;

		public TabManager(TabControl tc, string[] formNames, initFormCallbackType initForm, getDataCallbackType getData)
		{
			m_initFormCallback = initForm;
			m_getDataCallback = getData;

			m_formNames = formNames;

			m_tc = tc;

			m_openForms = new Dictionary<string, Form>(formNames.Length + 1);
			m_previouslySelectedTabsKeys = new List<string>(formNames.Length + 1);

			m_tc.MouseClick += new System.Windows.Forms.MouseEventHandler(tabControl_MouseClick);
			m_tc.Deselected += new System.Windows.Forms.TabControlEventHandler(tabControl_Deselected);

			addWelcomeTab(m_tc);
		}

		public Form getFormByName(string name)
		{
			if (m_openForms.ContainsKey(name))
				return m_openForms[name];
			else
				return null;
		}

		public TabControl TabControl
		{
			get
			{
				return m_tc;
			}
		}

		private void switchToLastSelectedTab()
		{
			int idx = m_previouslySelectedTabsKeys.Count - 1;
			if (idx < 0)
				return;
			m_doNotSaveNextPreviousTab = true;
			m_tc.SelectedTab = m_tc.TabPages[m_previouslySelectedTabsKeys[idx]];
			m_previouslySelectedTabsKeys.RemoveAt(idx);
		}

		private void removeTab(int index)
		{
			if (m_tc.SelectedTab != null && m_tc.SelectedTab.Name == m_tc.TabPages[index].Name)
				switchToLastSelectedTab();
			for (int i = 0; i < m_previouslySelectedTabsKeys.Count; i++)
				if (m_previouslySelectedTabsKeys[i] == m_tc.TabPages[index].Name)
					m_previouslySelectedTabsKeys.RemoveAt(i--);

			if (m_openForms.Count > 0)
			{
				m_openForms[m_tc.TabPages[index].Name].Dispose();
				m_openForms.Remove(m_tc.TabPages[index].Name);
			}

			m_tc.TabPages.RemoveAt(index);
			if (m_tc.TabPages.Count == 0)
			{
				addWelcomeTab(m_tc);
			}
		}

		public void addFormInTab(int formId)
		{
			addFormInTab(formId, null, null);
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Creates close tab [X] button. 
		///
		/// ...at the top right of the supplied TabPage
		/// @param targetTabPage ref to TabPage to which we are adding our button
		/// @ return x position of where button was placed
		private int addCloseTabButton(TabPage targetTabPage)
		{
			Button newCloseTabButton = new Button();
			newCloseTabButton.AutoSize = true;
			newCloseTabButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			newCloseTabButton.Text = "X";
			newCloseTabButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			//newCloseTabButton.Width = 30;
			newCloseTabButton.Click += new EventHandler(newCloseTabButton_Click);
			targetTabPage.Controls.Add(newCloseTabButton);
			return targetTabPage.Controls[targetTabPage.Controls.Count - 1].Left = targetTabPage.Width - newCloseTabButton.Width;
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Add refresh [Refresh] button. 
		///
		/// ...at the top right of the supplied TabPage
		/// @param targetTabPage ref to TabPage to which we are adding our button
		/// @ return x position of where button was placed
		private int addRefreshFormButton(TabPage targetTabPage, int offset)
		{
			Button newRefreshButton = new Button();
			newRefreshButton.AutoSize = true;
			newRefreshButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			newRefreshButton.Text = "Refresh";
			newRefreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			//newRefreshButton.Width = 90;
			newRefreshButton.Click += new EventHandler(newRefreshButton_Click);
			targetTabPage.Controls.Add(newRefreshButton);
			return targetTabPage.Controls[targetTabPage.Controls.Count - 1].Left = (offset == 0 ? targetTabPage.Width : offset) - newRefreshButton.Width;
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Handles [Refresh] (refresh form button) click.
		///
		/// Handles button clicks of any dynamically created [Refresh] button at the top 
		/// right of each subform's content area (except the welcome tab)
		private void newRefreshButton_Click(object sender, EventArgs e)
		{
			((ParamForm)m_openForms[((TabPage)((Control)sender).Parent).Name]).buttonRefresh();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Dynamically creates a new sub form and tabpage.
		///
		/// ...based on formId. Parameter arguments are optional.
		/// @param formId specifying which form to open. @see IASoftSetupForms.
		/// @param parameterNames array of parameter names to pass to the new form.
		/// @param parameterValues array of corresponding parameter values.
		/// @todo move into TabManager?
		public void addFormInTab(int formId, string[] parameterNames, string[] parameterValues)
		{
			if (checkForSmartTabSwitch(formId, parameterNames, parameterValues))
				return;

			if (m_initFormCallback == null)
			{
				log.Error("initForm callback is null");
				return;
			}

			log.Info("Opening a new form: " + formId);

			Form newForm;
			string tabKey = m_formNames[formId];

			try
			{
				newForm = m_initFormCallback(formId, parameterNames, parameterValues);
			}
			catch (ConnectionException cex)
			{
				MessageBox.Show(cex.Message, "Connection Error:");
				return;
			}

			m_tc.TabPages.Add(tabKey, tabKey);


			int offset = addCloseTabButton(m_tc.TabPages[tabKey]);
			if (newForm is ParamForm)
				addRefreshFormButton(m_tc.TabPages[tabKey], offset);

			m_tc.SelectedIndex = m_tc.TabPages.Count - 1;
			if (m_tc.TabPages["Welcome"] != null)
			{
				m_tc.TabPages.RemoveByKey("Welcome");
			}
			m_tc.Update();

			newForm.Text = m_formNames[formId];
			newForm.Dock = DockStyle.Fill;

			TabPage currentTab = m_tc.TabPages[m_tc.TabPages.Count - 1];
			for (int i = 0; i < newForm.Controls.Count; i++)
				currentTab.Controls.Add(newForm.Controls[i--]); // must adjust index because above line removes newForm.controls[i] item from newForm.controls collection

			// keep track of form objects that are open
			m_openForms.Add(tabKey, newForm);
		}


		private bool checkForSmartTabSwitch(int formId, string[] parameterNames, string[] parameterValues)
		{
			bool madeTabSwitch = false;
			string key = m_formNames[formId];

			if (m_tc.TabPages.ContainsKey(key))
			{
				m_tc.SelectedTab = m_tc.TabPages[key];
				madeTabSwitch = true;
				m_tc.SelectedTab.Update();
				resetParametersOnFormByName(key, parameterNames, parameterValues);
			}

			return madeTabSwitch;
		}

		private void resetParametersOnFormByName(string formName, string[] parameterNames, string[] parameterValues)
		{
			if (getFormByName(formName) is ParamForm)
			    ((ParamForm)getFormByName(formName)).initParameters(parameterNames, parameterValues, true);
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Handles [X] (close tab button) click.
		///
		/// Handles button clicks of any dynamically created [X] button at the top right 
		/// of each tab content area (except the welcome tab, which has no close button)
		private void newCloseTabButton_Click(object sender, EventArgs e)
		{
			closeCurrentTab();
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Closes the current tab.
		///
		/// If no open tabs are left, reopens welcome tab.
		public void closeCurrentTab()
		{
			removeTab(m_tc.SelectedIndex);
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Dynamically creates a new TabPage welcoming the user to the application.
		///
		/// Called when no other tabs are open.
		private void addWelcomeTab(TabControl target)
		{
			string appName = (string)m_getDataCallback("AppName");

			TabPage newTabPage = new TabPage("Welcome");
			newTabPage.Name = "Welcome";

			Label welcomeLabel = new Label();
			welcomeLabel.AutoSize = true;
			welcomeLabel.Location = new Point(5, 25);
			welcomeLabel.Text = "Welcome";
			if (appName != null && appName != "")
				welcomeLabel.Text += "to the " + appName;
			welcomeLabel.Text += "! \r\n\r\nPlease double click your selection from the menu to the left.";

			//addCloseTabButton(ref newTabPage);

			target.TabPages.Add(newTabPage);
			target.TabPages["Welcome"].Controls.Add(welcomeLabel);
		}

		private void tabControl_Deselected(object sender, TabControlEventArgs e)
		{
			if (m_doNotSaveNextPreviousTab)
			{
				m_doNotSaveNextPreviousTab = false;
				return;
			}

			// handle middle clicking not current tab, middle clicking welcome tab, etc.
			if (m_tc.SelectedTab == null)
				return;

			// clear any old records of this tab name from previos tab list
			for (int i = 0; i < m_previouslySelectedTabsKeys.Count; i++)
				if (m_previouslySelectedTabsKeys[i] == m_tc.TabPages[m_tc.SelectedTab.Name].Name)
					m_previouslySelectedTabsKeys.RemoveAt(i--);

			// keep track of previous tab name
			m_previouslySelectedTabsKeys.Add(m_tc.SelectedTab.Name);
		}

		public int clearTabs()
		{
			return clearTabs(null);
		}

		public int clearTabs(string message)
		{
			int userCanceled = 0;
			if (message == null)
				message = "Close all open tabs?";

			if (!m_tc.TabPages.ContainsKey("Welcome"))
			{
				// query user about closing tabs
				Hashtable data = new Hashtable
				{
					{"Title", "Confirm closing connection"},
					{"Controls", new Dictionary<string, Hashtable> 
										{
											{"heading", new Hashtable()
															{
																{"Type", "Label"},
																{"Label", message}
															}
											}
										}
					}
				};
				PopupQuery areYouSure = new PopupQuery(data);

				// create popup query window and block until it's closed
				if (DialogResult.OK != areYouSure.ShowDialog())
					userCanceled = 1;
			}

			if (userCanceled == 0)
			{
				// close tabs until a tab with name of "Welcome" is present
				//while (!tc.TabPages.ContainsKey("Welcome"))
				//    closeCurrentTab();

				// close tabs until tabcount stops decreasing
				int tabCount;
				do
				{
					tabCount = m_tc.TabPages.Count;
					closeCurrentTab();

				} while (tabCount > m_tc.TabPages.Count);
			}

			return userCanceled;
		}

		/////////////////////////////////////////////////////////////////////////////////
		/// Callback for event when mouse click is detected in the tab area. 
		///
		/// We only care if the middle mouse button is clicked here, in which case we
		/// close the clicked tabbed and call addWelcomeTab() if we closed the last tab.
		/// @param sender ref to the mainTabControl
		/// @param e MouseEventArgs
		private void tabControl_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Middle)
			{
				var tabControl = sender as TabControl;
				var tabs = tabControl.TabPages;

				for (int i = 0; i < tabs.Count; i++)
				{
					if (tabControl.GetTabRect(i).Contains(e.Location))
					{
						removeTab(i);
						break;
					}
				}
			}
		}

	}
}
