/*
MainForm.cs
Intellasoft Configurator.
 
@author Mike Jacobi for Intellasoft
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using log4net;
using System.Diagnostics;
using System.Configuration;
using Lib;

namespace IasPbxConfig
{
	//////////////////////////////////////////////////////////////////////////////////
	/// The main program Form; contains everything else.
	///
	/// Contains The main menu, a split view area for browsing and viewing through 
	/// the other forms, along with a tab area to switch between open forms. Also 
	/// has a status bar at the bottom. Handles the creation and releasing of sub 
	/// forms.
	/// 
	/// @todo Status bar interaction (message details (backtrace, full exception message))
	public partial class MainForm : Form
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private bool m_isDisposed;

		private TabSetManager m_tsm = null;
		private ConnectionManager m_cm = null;

		//////////////////////////////////////////////////////////////////////////////////
		/// Constructor.
		///
		/// Populates the left split view list of available forms. 
		/// Calls addWelcomeTab() to create a welcome message tab on startup.
		public MainForm()
		{
			log.Info("Initializing main form..");

			// set Title Bar
			this.Text = Program.AppName;

			InitializeComponent();

			Properties.Settings settings = Properties.Settings.Default;
			m_cm = new ConnectionManager(launchError, settings.ConnectionsInfo, mainTreeView.Nodes, handleConnectionStatusChange);
			this.fileToolStripMenuItem.DropDownItems.Insert(0, m_cm.NewConnectionMenuItem);
			m_cm.refreshDisplayedConnections();

			m_tsm = new TabSetManager("main", designerMainTabControl, Program.m_formNames, initFormById, getDataById);
		}

		public TabManager TabManager
		{
			get
			{
				return m_tsm.TabManager;
			}
		}

		private int handleConnectionStatusChange(ConnectionManager.Status status, string key)
		{
			int cancel = 0;

			switch (status)
			{
				case ConnectionManager.Status.AskPermissionForDelete:
					if (m_cm.ClickedNode.Nodes.Count > 0)
						cancel = TabManager.clearTabs("Delete Connection and close all open tabs?");
					if (cancel == 0) // user did not cancel
						m_tsm.removeTabSet(key);
					break;
				case ConnectionManager.Status.AskPermissionForDisconnect:
					cancel = TabManager.clearTabs("Close current connection and all open tabs?");
					break;
				case ConnectionManager.Status.Connected:
					string dbProtocolVersion = null;
					if (0 != m_cm.DB.getSingleFromDb(ref dbProtocolVersion,
														"configurator_protocol_version_required",
														"Asterisk.configurator_config"))
					{
						cancel = 1;
						string m = "Connection failed to " + key + ": Unable to access protocol version on server. ";
						string f = "Please contact your administrator.";
						log.Error(m + " Local: " + Program.ProtocolVersion);
						setStatus(m + f);
						MessageBox.Show(f, m);
					}
					else if (dbProtocolVersion != Program.ProtocolVersion)
					{
						cancel = 1;
						string m = "Connection failed to " + key + ": Protocol version mismatch. ";
						string f = "Please upgrade this software or contact your administrator.";
						log.Error(m + "Db: " + dbProtocolVersion + " Local: " + Program.ProtocolVersion);
						setStatus(m + f);
						MessageBox.Show(f, m);
					}
					else
						setStatus("Connected: " + key);

					break;
				case ConnectionManager.Status.ConnectionFailed:
				case ConnectionManager.Status.Disconnected:
				case ConnectionManager.Status.ChangedCurrentConnection:
					setStatus(key);
					break;
				case ConnectionManager.Status.Connecting:
					setStatus(key);
					this.Update();
					break;
				case ConnectionManager.Status.RefreshOpenConnectionNode:
					addFormNodes(key);
					break;
				case ConnectionManager.Status.DbHelperReset:
					//m_db = m_cm.DB;
					break;
				case ConnectionManager.Status.SettingsLoad:
					Properties.Settings loadedSettings = Properties.Settings.Default;
					if (m_cm != null)
						m_cm.Settings = loadedSettings.ConnectionsInfo;
					break;
				case ConnectionManager.Status.SettingsSave:
					Properties.Settings settingsToSave = Properties.Settings.Default;
					if (m_cm != null)
						settingsToSave.ConnectionsInfo = m_cm.Settings;
					settingsToSave.Save();
					break;
				case ConnectionManager.Status.Error:
					log.Debug("No handling defined for Connection Manager status of <" + status.ToString() + ">.");
					break;
				default:
					log.Error("Unknown Connection Manager Status <" + status.ToString() + "> recieved.");
					break;
			}

			return cancel;
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Destructor.
		///
		/// Checks if database connection is still open and close it.
		~MainForm()
		{
			log.Debug("Dispose called by GC.");
			Dispose(false);
		}

		public new void Dispose()
		{
			log.Debug("Dispose called manually.");
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected override void Dispose(bool disposeManagedResources)
		{
			if (!m_isDisposed)
			{
				if (disposeManagedResources)
				{
					// @todo make ConnectionManager : Idisposable and have it dispos all of its connections
					// dispose managed resources
					if (m_cm.DB != null)
						m_cm.DB.Dispose();
					if (components != null)
						components.Dispose();
				}
				base.Dispose(disposeManagedResources);
				// dispose unmanaged resources (don't have any)
				m_isDisposed = true;
			}
			else
			{
				log.Error("Dispose called more than once.");
			}
		}

		private void addFormNodes(string connectionNodeName)
		{
			TreeNode[] newNodes = new TreeNode[] { };
			for (int i = 0; i < (int)IASoftSetupForms.End; i++)
			{
				Array.Resize(ref newNodes, i + 1);
				newNodes[i] = new TreeNode(Program.m_formNames[i]);
				newNodes[i].Name = Program.m_formNames[i];
			}
			mainTreeView.Nodes[connectionNodeName].Nodes.AddRange(newNodes);
		}

		private Form initFormById(int formId, string[] parameterNames, string[] parameterValues)
		{
			Timing.start();

			Form newForm;
			switch (formId)
			{
				case (int)IASoftSetupForms.Cos:
					newForm = new ClassesOfService(m_cm.DB);
					break;
				case (int)IASoftSetupForms.Cosi:
					newForm = new ClassesOfServiceIncludes(m_cm.DB, parameterNames, parameterValues);
					break;
				case (int)IASoftSetupForms.Ext:
					newForm = new Extensions(m_cm.DB);
					break;
				case (int)IASoftSetupForms.Extd:
					newForm = new ExtensionDetails(m_cm.DB, parameterNames, parameterValues);
					break;
				case (int)IASoftSetupForms.Trk:
					newForm = new Trunks(m_cm.DB);
					break;
				case (int)IASoftSetupForms.Trkd:
					newForm = new TrunkDetails(m_cm.DB, parameterNames, parameterValues);
					break;
				case (int)IASoftSetupForms.Rt:
					newForm = new Routes(m_cm.DB);
					break;
				case (int)IASoftSetupForms.Pg:
					newForm = new PhoneGroups(m_cm.DB);
					break;
				case (int)IASoftSetupForms.Tg:
					newForm = new TrunkGroups(m_cm.DB);
					break;
				case (int)IASoftSetupForms.Tgi:
					newForm = new TrunkGroupIncludes(m_cm.DB, parameterNames, parameterValues);
					break;
				case (int)IASoftSetupForms.Ri:
					newForm = new RegistrationsIax(m_cm.DB);
					break;
				case (int)IASoftSetupForms.Rs:
					newForm = new RegistrationsSip(m_cm.DB);
					break;
				case (int)IASoftSetupForms.Fc:
					newForm = new FeatureCodes(m_cm.DB);
					break;
				case (int)IASoftSetupForms.Tv:
					newForm = new TraceViewer(m_cm.DB);
					break;
				default:
					newForm = null;
					break;
			}

			Timing.stop();

			return newForm;
		}

		private object getDataById(string key)
		{
			object result = null;

			switch (key)
			{
				case "AppName":
					result = Program.AppName;
					break;
				default:
					break;
			}

			return result;
		}

		/////////////////////////////////////////////////////////////////////////////////
		/// Change the message displayed in the bottom status strip.
		///
		/// We clear all items in tthe strip before adding the message to keep things 
		/// clean.
		/// @todo make the message clickable to see a full backtrace/exception/etc.
		/// @param message the message to display
		internal void setStatus(string message)
		{
			//bottomStatusStrip.Text = message;
			bottomStatusStrip.Items.Clear();
			bottomStatusStrip.Items.Add(message);
		}

		/////////////////////////////////////////////////////////////////////////////////
		/// Handle an error based on a code.
		///
		/// Put the error message into the status bar, log it, and make a message box appear.
		/// @param error errorCode of error to handle @see Program.ErrorMessages
		public void launchError(int error)
		{
			//closeCurrentTab();
			////mainTabControl.TabPages[mainTabControl.SelectedIndex].Dispose();
			MessageBox.Show(Program.ErrorMessages[error]);
			setStatus(Program.ErrorMessages[error]);
			log.Error(Program.ErrorMessages[error]);
		}

		/////////////////////////////////////////////////////////////////////////////////
		/// Handle an error based on a message.
		///
		/// Put the error message into the status bar, log it, and make a message box appear.
		/// Use a user friendly version if available.
		/// @param errorMessage the error message to handle
		public void launchError(string errorMessage)
		{
			//closeCurrentTab();
			////mainTabControl.TabPages[mainTabControl.SelectedIndex].Dispose();
			string friendlyMessage = Helper.findUserFriendlyErrorMessage(errorMessage);
			if (friendlyMessage == "")
			{
				friendlyMessage = errorMessage;
			}
			else
			{
				log.Error(errorMessage);
			}

			MessageBox.Show(friendlyMessage);
			setStatus(friendlyMessage);
			log.Error(friendlyMessage);
		}

		/////////////////////////////////////////////////////////////////////////////////
		/// Callback for paint event for mainPanel.
		///
		/// mainPanel contains the topMenuStrip, mainSplitContainer, and bottomStatusStrip
		/// @param sender ref to the mainPanel
		/// @param e PaintEventArgs
		private void panel1_Paint(object sender, PaintEventArgs e)
		{
		}

		/////////////////////////////////////////////////////////////////////////////////
		/// Callback for event when doubleclick is detected in the mainTreeView in left 
		/// panel.
		///
		/// A call is made to open a new tab and form correspoding to the doubleclicked 
		/// item.
		/// @param sender ref to the mainTreeView
		/// @param e EventArgs
		private void mainTreeView_DoubleClick(object sender, EventArgs e)
		{
			// if no node is selected, cancel
			if (mainTreeView.SelectedNode == null)
				return;

			// check if level of node is root (connections) 
			if (mainTreeView.Nodes.Contains(mainTreeView.SelectedNode))
			{
				//if node is not already in connected state
				if (mainTreeView.SelectedNode.Nodes.Count == 0)
				{
					// open this node/connection
					m_cm.switchToConnection(mainTreeView.SelectedNode.Name);
				}
			}
			else // if not in root level, node is in form level
			{
				// open this node's parent node/connection
				m_cm.switchToConnection(mainTreeView.SelectedNode.Parent.Name);

				// open form in new tab
				TabManager.addFormInTab(Helper.find(Program.m_formNames, mainTreeView.SelectedNode.Name));
			}
		}

		private void mainTreeView_MouseDown(object sender, MouseEventArgs e)
		{
			TreeViewHitTestInfo hti = ((TreeView)sender).HitTest(e.Location);

			string rootNodeName = null;

			if (hti.Node != null)
			{
				if (hti.Node.Level == 0)
				{
					m_cm.ClickedNode = hti.Node;

					// prevent node from having selected forecolor and nonhilighted backcolor when context for 
					// non-selected node appears
					mainTreeView.SelectedNode = m_cm.ClickedNode;
					rootNodeName = m_cm.ClickedNode.Name;
				}
				else if (hti.Node.Level == 1)
				{
					rootNodeName = hti.Node.Parent.Name;
				}
			}

			if (rootNodeName != null)
				m_tsm.switchToTabSetAndCreateIfNeeded(rootNodeName);
		}

		/////////////////////////////////////////////////////////////////////////////////
		/// Callback for event when exit option is clicked in top File Menu.
		///
		/// We call this.Close() to close the mainForm and thus the entire program.
		/// @param sender ref to the exitToolStripMenuItem1
		/// @param e EventArgs        
		private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		/////////////////////////////////////////////////////////////////////////////////
		/// Callback for event when View Log option is clicked in top Tools Menu.
		///
		/// Exec notepad.exe with our log file, and handle errors.
		/// @param sender ref to the viewLogToolStripMenuItem
		/// @param e EventArgs        
		private void viewLogToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// open log in notepad
			//Process p = null;
			//try
			//{
			//    p = new Process();
			//    p.StartInfo.FileName = "notepad";
			//    p.StartInfo.Arguments = "IasPbxLog.txt";
			//    p.Start();
			//    //p.WaitForExit();
			//}
			//catch (Exception ex)
			//{
			//    log.Error("Error opening log with notpead: " + ex.Message + " : " + ex.StackTrace.ToString());
			//    launchError(ex.Message);
			//}

			// open log in Lib.LogViewer
			LogViewer lv = new LogViewer("IasPbxLog.txt");
			lv.Show();
		}

		private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
		{
			// detect ctrl + d and if current tab is a TableForm, call its deleteSelectedRows()
			if (Control.ModifierKeys == Keys.Control && e.KeyChar == 4)
			{
				Form f = TabManager.getFormByName(m_tsm.TabManager.TabControl.SelectedTab.Name);
				if (f is TableForm)
					((TableForm)f).deleteSelectedRows();

			}
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using(TextReader tr = new StreamReader(@"Resources\gitVersion.txt"))
			{
				Program.Revision = tr.ReadToEnd();
			}

			Hashtable popupData = new Hashtable
			{
				{"Title", "About"},
				{"Controls", new Dictionary<string, Hashtable> 
					{
						{"logo image", new Hashtable()
							{
								{"Type", "Image"},
								{"Path", "Resources/about_logo.png"}
							}
						},
						{"\r\n" + Program.AppName, new Hashtable()
							{
								{"Type", "Heading"}
							}
						},
						{"Version: " + Program.Version + "---" + Program.Revision, new Hashtable()
							{
								{"Type", "Label"},
							}
						},
						{Program.CopyRight, new Hashtable()
							{
								{"Type", "Label"},
							}
						},
					}
				},
				{"NoCancelButton", true}
				};
				PopupQuery about = new PopupQuery(popupData);
				// create popup query window and block until it's closed
				about.ShowDialog();
		}

		bool didJustMove = false;

		private void MainForm_ResizeEnd(object sender, EventArgs e)
		{
			if (didJustMove)
			{
				didJustMove = false;
				return;
			}

			Form f = TabManager.getFormByName(m_tsm.TabManager.TabControl.SelectedTab.Name);
			if (f is TraceViewer)
				((TraceViewer)f).Notify_ResizeEnd(sender, e);
		}

		private void MainForm_Move(object sender, EventArgs e)
		{
			didJustMove = true;
		}

	}
}
