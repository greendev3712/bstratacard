using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using Lib;
using log4net;

namespace Lib
{
	public class ConnectionManager
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private ContextMenuStrip connectionContextMenuStrip;
		private ToolStripMenuItem newConnectionToolStripMenuItem;
		private ToolStripMenuItem connectToolStripMenuItem;
		private ToolStripMenuItem editConnectionToolStripMenuItem;
		private ToolStripMenuItem deleteConnectionToolStripMenuItem;

		private TreeView m_connectionTreeView;
		private TreeNodeCollection m_connectionNodes;
		private TreeNode m_clickedNode = null;

		private DbHelper m_currentDb = null;
		private Dictionary<string, DbHelper> m_dbs = null;

		private Dictionary<string, SimpleDbConnectionParameters> m_availableConnections;

		private StringCollection m_connectionSettings = null;
		
		public enum Status
		{
			None,
			Connecting,
			Connected,
			RefreshOpenConnectionNode,
			Disconnected,
			ConnectionFailed,
			AskPermissionForDisconnect,
			AskPermissionForDelete,
			DbHelperReset,
			SettingsSave,
			SettingsLoad,
			ChangedCurrentConnection,
			Error
		};

		/////////////////////////////////////////////////////////////////////////////////////
		/// delegate description for callback.
		///
		/// @param errorMessage text for the errorMessage
		public delegate int statusChangeCallbackType(Status status, string key);

		private statusChangeCallbackType m_notifyClientOfStatusChange = null;

        private static QD.QE_ErrorCallbackFunction m_errorHandler = QD.GenericErrorCallbackFunction;
        
		private static readonly List<string> ConnectionParameterKeys = new List<string>
		{
			"id",
			"name",
			"host", 
			"port", 
			"user", 
			"save_pass", 
			"pass", 
			"database"
		};

		private static readonly Dictionary<string, Hashtable> PopupControlInitData = new Dictionary<string, Hashtable>()
		{
			{"heading", new Hashtable()
							{
								{"Type", "Heading"},
								{"Label", "New Connection Details:"}
							}
			},
			{"name", new Hashtable()
							{
								{"Type", "TextBox"},
								{"Label", "Name"},
								{"Value", "New Connection"}
							}
			},
			{"host", new Hashtable()
							{
								{"Type", "TextBox"},
								{"Label", "Hostname"},
							}
			},
			{"port", new Hashtable()
							{
								{"Type", "TextBox"},
								{"Label", "Port"},
								{"Value", "5432"}
							}
			},
			{"user", new Hashtable()
							{
								{"Type", "TextBox"},
								{"Label", "User Name"},
							}
			},
			{"save_pass", new Hashtable() // must appear before pass in current implementation
							{
								{"Type", "CheckBox"},
								{"Label", "Save Password"},
								{"Value", true}
							}
			},
			{"pass", new Hashtable()
							{
								{"Type", "TextBox"},
								{"Label", "Password"},
							}
			},
			{"database", new Hashtable()
							{
								{"Type", "TextBox"},
								{"Label", "Database"},
								{"Value", "pbx"}
							}
			}
		};

		private static readonly Hashtable PopupInitData = new Hashtable()
		{
			{"Title", "Create New Connection"},
			{"Controls", PopupControlInitData}
		};

		public ContextMenuStrip ContextMenu
		{
			get
			{
				return connectionContextMenuStrip;
			}
		}

		public ToolStripMenuItem NewConnectionMenuItem
		{
			get
			{
				return newConnectionToolStripMenuItem;
			}
		}

		public DbHelper DB
		{
			get
			{
				return m_currentDb;
			}
		}

		public TreeView ConnectionTreeView
		{
			set
			{
				m_connectionTreeView = value;
			}
			get
			{
				return m_connectionTreeView;
			}
		}

		public TreeNode ClickedNode
		{
			set
			{
				m_clickedNode = value;
			}
			get
			{
				return m_clickedNode;
			}
		}

		public StringCollection Settings
		{
			get
			{
				return m_connectionSettings;
			}
			set
			{
				m_connectionSettings = value;
			}
		}

		public ConnectionManager(QD.QE_ErrorCallbackFunction errorHandler, StringCollection connectionSettings, TreeView connectionTreeView, statusChangeCallbackType statusChangeCallback)
		{
			connectionContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
			newConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			editConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			deleteConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

			connectionContextMenuStrip.SuspendLayout();

			// 
			// newConnectionToolStripMenuItem
			// 
			newConnectionToolStripMenuItem.Name = "newConnectionToolStripMenuItem";
			newConnectionToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			newConnectionToolStripMenuItem.Text = "New Connection";
			newConnectionToolStripMenuItem.Click += new System.EventHandler(newConnectionToolStripMenuItem_Click);
			// 
			// connectionContextMenuStrip
			// 
			connectionContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            connectToolStripMenuItem,
            editConnectionToolStripMenuItem,
            deleteConnectionToolStripMenuItem});
			connectionContextMenuStrip.Name = "connectionContextMenuStrip";
			connectionContextMenuStrip.Size = new System.Drawing.Size(153, 92);
			connectionContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(connectionContextMenuStrip_Opening);
			// 
			// connectToolStripMenuItem
			// 
			connectToolStripMenuItem.Name = "connectToolStripMenuItem";
			connectToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			connectToolStripMenuItem.Text = "Connect";
			connectToolStripMenuItem.Click += new System.EventHandler(connectToolStripMenuItem_Click);
			// 
			// editConnectionToolStripMenuItem
			// 
			editConnectionToolStripMenuItem.Name = "editConnectionToolStripMenuItem";
			editConnectionToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			editConnectionToolStripMenuItem.Text = "Edit";
			editConnectionToolStripMenuItem.Click += new System.EventHandler(editConnectionToolStripMenuItem_Click);
			// 
			// deleteConnectionToolStripMenuItem
			// 
			deleteConnectionToolStripMenuItem.Name = "deleteConnectionToolStripMenuItem";
			deleteConnectionToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			deleteConnectionToolStripMenuItem.Text = "Delete";
			deleteConnectionToolStripMenuItem.Click += new System.EventHandler(deleteConnectionToolStripMenuItem_Click);

			connectionContextMenuStrip.ResumeLayout(false);

			m_notifyClientOfStatusChange = statusChangeCallback;
			//m_currentDb = db;
			m_dbs = new Dictionary<string, DbHelper>(1);
			m_connectionTreeView = connectionTreeView;
			m_connectionNodes = connectionTreeView.Nodes;
			m_availableConnections = new Dictionary<string, SimpleDbConnectionParameters>();

			m_connectionSettings = connectionSettings;

			if (m_connectionSettings == null)
				clearConnectionsFromSettings();
			else
				m_availableConnections = loadConnectionsFromSettings();

		}

		public void refreshDisplayedConnections()
		{
			//remember if a connection is open
			string[] openNodesName = new string[m_connectionNodes.Count];
			bool[] openNodesAreExpanded = new bool[m_connectionNodes.Count];
			int i = 0;
			foreach (TreeNode n in m_connectionNodes)
			{
				if (n.Nodes.Count > 0 && m_availableConnections.ContainsKey(n.Name))
				{
					openNodesName[i] = n.Name;
					if (n.IsExpanded)
						openNodesAreExpanded[i] = true;
					else
						openNodesAreExpanded[i] = false;
				}
				else
				{
					openNodesName[i] = null;
					openNodesAreExpanded[i] = false;
				}
				i++;
			}

			// reload ui connection treeview nodes
			m_connectionNodes.Clear();
			foreach (string id in m_availableConnections.Keys)
			{
				TreeNode node = new TreeNode((string)m_availableConnections[id]["name"] + " (" + (string)m_availableConnections[id]["host"] + ")");
				node.Name = id;
				node.ContextMenuStrip = connectionContextMenuStrip;
				m_connectionNodes.Add(node);
			}

			// reapply remembered states
			for (i = 0; i < openNodesName.Length; i++)
				if (null != openNodesName[i])
				{
					//addFormNodes(openNodeName);
					notifyClient(Status.RefreshOpenConnectionNode, openNodesName[i]);

					Helper.highlightNode(m_connectionNodes[openNodesName[i]]);
					if (openNodesAreExpanded[i])
						m_connectionNodes[openNodesName[i]].Expand();
				}
		}

		private int notifyClient(Status status, string key)
		{
			if (m_notifyClientOfStatusChange != null)
				return m_notifyClientOfStatusChange(status, key);
			else
				return 0;
		}

		private void clearConnectionsFromSettings()
		{
			//StringCollection sc = new StringCollection();
			//Properties.Settings set = Properties.Settings.Default;
			//set.ConnectionsInfo = sc;
			//set.Save();

			m_connectionSettings = new StringCollection();
			notifyClient(Status.SettingsSave, "");
		}

		private Dictionary<string, SimpleDbConnectionParameters> loadConnectionsFromSettings()
		{
			int i = 0, connectionParameterCount = ConnectionParameterKeys.Count;
			//Properties.Settings set = Properties.Settings.Default;
			notifyClient(Status.SettingsLoad, "");
            Dictionary<string, SimpleDbConnectionParameters> connections = new Dictionary<string, SimpleDbConnectionParameters>(connectionParameterCount);
			if (connections == null)
                connections = new Dictionary<string, SimpleDbConnectionParameters>(connectionParameterCount); // not needed

			string currentConnectionName = null;
			foreach (string s in m_connectionSettings)
			{
				if (i == 0)
				{
					currentConnectionName = s;
                    connections.Add(currentConnectionName, new SimpleDbConnectionParameters());
				}

				connections[currentConnectionName][ConnectionParameterKeys[i]] = s;

				if (++i >= connectionParameterCount)
					i = 0;
			}
			return connections;
		}


        private void saveConnectionsToSettings(Dictionary<string, SimpleDbConnectionParameters> connections, string connectionKeyForUnencryptedPass)
		{
            /* FIXME
            
			//StringCollection sc = new StringCollection();
			//Properties.Settings set = Properties.Settings.Default;
			m_connectionSettings = new StringCollection();

			foreach (string connKey in connections.Keys)
			{
				bool savePass = false;
				foreach (string paramKey in ConnectionParameterKeys)
				{
					if (paramKey == "id" && connections[connKey][paramKey] == null)
						m_connectionSettings.Add(connKey);
					else if (paramKey == "pass")
					{
						if (savePass)
						{
							if (connKey == connectionKeyForUnencryptedPass)
								// save back encrypted password to passed in connection data
								connections[connKey][paramKey] = Helper.encryptOrDecrypt((string)connections[connKey][paramKey], "abc123", true);
							m_connectionSettings.Add((string)connections[connKey][paramKey]);
						}
						else
							m_connectionSettings.Add("");
					}
					else if (connections[connKey][paramKey] is bool)
						m_connectionSettings.Add((bool)connections[connKey][paramKey] ? "YES" : "NO");
					else
						m_connectionSettings.Add((string)connections[connKey][paramKey]);

					if (paramKey == "save_pass")
					{
						if (connections[connKey][paramKey] is bool)
							savePass = (bool)connections[connKey][paramKey];
						else
						{
							string val = ((string)connections[connKey][paramKey]).ToUpper();
							savePass = val == "YES" || val == "Y" || val == "TRUE";
						}
					}
				}
			}
			//set.ConnectionsInfo = sc;
			//set.Save();
			notifyClient(Status.SettingsSave, "");
            */
		}

		public void switchToConnection(string connectionId)
		{
            /* FIXME
			string connectionLabel = (m_availableConnections[connectionId])["name"];

			if (m_dbs.ContainsKey(connectionId))
			{
				m_currentDb = m_dbs[connectionId];
				notifyClient(Status.ChangedCurrentConnection, "Viewing Connection: " + connectionLabel);
				return;
			}

            SimpleDbConnectionParameters connectionDetails = m_availableConnections[connectionId];
			connectionDetails = connectionDetails.Clone();

			// prompt user for password
			if ((string)m_availableConnections[connectionId]["pass"] == "" &&
					((m_availableConnections[connectionId]["save_pass"] is bool && !(bool)m_availableConnections[connectionId]["save_pass"])
						|| (m_availableConnections[connectionId]["save_pass"] is string && !Helper.atob((string)m_availableConnections[connectionId]["save_pass"]))))
			{
				Hashtable data = new Hashtable
				{
					{"Title", "Authenticate with Server"},
					{"Controls", new Dictionary<string, Hashtable> 
									{
										{"heading", new Hashtable()
														{
															{"Type", "Heading"},
															{"Label", connectionLabel + " Credentials:"}
														}
										},
										{"Password:", new Hashtable()
														{
															{"Type", "TextBox"},
															//{"Label", "Password"},
														}
										}
									}
					}
				};
				PopupQuery passwordPrompt = new PopupQuery(data);

				/// create popup query window and block until it's closed
				if (DialogResult.OK == passwordPrompt.ShowDialog())
					connectionDetails["pass"] = (string)((Hashtable)data["Values"])["Password:"];
				else
					return;
			}
			else
				// decrypt password
				try
				{
					connectionDetails["pass"] = Helper.encryptOrDecrypt((string)connectionDetails["pass"], "abc123", false);
				}
				catch
				{
					log.Error("Password decryption failed for " + connectionLabel);
				}

			//// clean up ui elements of previous connection and reset database class
			//if (0 < closeCurrentConnection())
			//    return; // user canceled

			//setStatus("Connecting to " + connectionLabel + "...");
			//this.Update();
			notifyClient(Status.Connecting, "Connecting to " + connectionLabel + "...");

			// init this connection with parameters
			//            Program.m_db.initConnection(newConnectionDetailsWithPassword != null ? newConnectionDetailsWithPassword : (Hashtable)m_availableConnections[connectionId]);
			DbHelper db = new DbHelper(m_errorHandler);
	
			db.initConnection(connectionDetails);

			bool isTestConnection = false;

			// attempt to connect to server
			try
			{
				db.connect();
			}
			catch (ConnectionException cex)
			{
				isTestConnection = connectionLabel.ToString() == "__test_debug";

				//setStatus("Connection Failed: " + connectionLabel);
				notifyClient(Status.ConnectionFailed, "Connection Failed: " + connectionLabel);
				//MessageBox.Show("Could not connect to " + connectionLabel + ": \n" + cex.Message, "Connection Error");
				// alert user about failed connection
				Hashtable data = new Hashtable
				{
					{"Title", "Connection Error"},
					{"Controls", new Dictionary<string, Hashtable> 
									{
										{connectionLabel + ": ", new Hashtable()
																	{
																		{"Type", "Heading"}
																	}
										},
										{cex.Message + (isTestConnection ? " Continuing in test debug mode.)" : ""), new Hashtable()
																	{
																		{"Type", "Label"},
																	}
										},
									}
					},
					{"NoCancelButton", true}
				};
				PopupQuery areYouSure = new PopupQuery(data);
				// create popup query window and block until it's closed
				areYouSure.ShowDialog();
				if (!isTestConnection)
				{
					return;
				}
			}

			m_dbs.Add(connectionId, db);
			m_currentConnectionKey = connectionId;
			m_currentDb = db;

			if (!isTestConnection && 0 != notifyClient(Status.Connected, connectionLabel))
			{
				// reset database class
				if (m_currentDb != null)
				{
					if (m_dbs.ContainsKey(connectionId))
						m_dbs.Remove(connectionId);
					m_currentDb.Dispose();
					m_currentDb = null;
				}
				m_currentDb = new DbHelper(m_errorHandler);
				notifyClient(Status.DbHelperReset, "");
				
				return;
			}

			//addFormNodes(connectionId);
			notifyClient(Status.RefreshOpenConnectionNode, connectionId);

			Helper.highlightNode(m_connectionNodes[connectionId]);
			m_connectionNodes[connectionId].Expand();
            */
		}


		private void connectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (connectToolStripMenuItem.Text == "Connect")
				switchToConnection(m_clickedNode.Name);
			else if (0 == closeConnection(m_clickedNode.Name))
				notifyClient(Status.Disconnected, "Disconnected: " + m_clickedNode.Name);
		}

		private int closeConnection(string connectionKey)
		{
			// close tabs if user is ok with it
			//int userCanceled = clearTabs(mainTabControl, "Close current connection and all open tabs?");
			int userCanceled = notifyClient(Status.AskPermissionForDisconnect, "");
			if (0 == userCanceled) // if user did not cancel
			{
				// clean up remaining ui elements of previous connection 
				m_connectionNodes[connectionKey].Nodes.Clear();
				Helper.unHighlightNode(m_connectionNodes[connectionKey]);

				// reset database class
				if (m_currentDb != null)
				{
					if (m_dbs.ContainsKey(connectionKey))
						m_dbs.Remove(connectionKey);
					m_currentDb.Dispose();
					m_currentDb = null;
				}

				m_currentDb = new DbHelper();
                m_currentDb.SetErrorCallback(m_errorHandler);

				notifyClient(Status.DbHelperReset, "");
			}
			return userCanceled;
		}

		private void editConnectionToolStripMenuItem_Click(object sender, EventArgs e)
		{
            /* FIXME
			string connectionKey = m_clickedNode.Name;

			Hashtable data = Helper.deepClone(PopupInitData);
            SimpleDbConnectionParameters connectionDetails = m_availableConnections[connectionKey].Clone();

			// decrypt password
			if ((string)connectionDetails["pass"] != "" &&
					((connectionDetails["save_pass"] is bool && (bool)connectionDetails["save_pass"])
						|| (connectionDetails["save_pass"] is string && Helper.atob((string)connectionDetails["save_pass"]))))
				connectionDetails["pass"] = Helper.encryptOrDecrypt((string)connectionDetails["pass"], "abc123", false);

			data.Add("Values", connectionDetails);
			data["Title"] = "Edit Connection";
            ((Dictionary<string, SimpleDbConnectionParameters>)data["Controls"])["heading"]["Label"] = m_availableConnections[connectionKey]["name"] + " Details:";
			PopupQuery newConnectionForm = new PopupQuery(data);

			/// create popup query window and block until it's closed
			if (DialogResult.OK == newConnectionForm.ShowDialog())
			{
				//((Hashtable)data["Values"])["pass"] = encryptOrDecrypt((string)((Hashtable)data["Values"])["pass"], "abc123", true);
				m_availableConnections[connectionKey] = data["Values"];

				saveConnectionsToSettings(m_availableConnections, connectionKey); // this will encrypt the pass

				refreshDisplayedConnections();
			}
            */
		}

		private void deleteConnectionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (0 < notifyClient(Status.AskPermissionForDelete, m_clickedNode.Name))
				return; // user canceled
			m_availableConnections.Remove(m_clickedNode.Name);
			saveConnectionsToSettings(m_availableConnections, m_clickedNode.Name);
			m_availableConnections = loadConnectionsFromSettings(); // not needed
			refreshDisplayedConnections();

			// @todo guarantee the displayed TabControl and the selected mainTreeView node are synced (set one or the other)
			// not needed but guarantees that the visible TabControl is synced with the selected Node
			//if (mainTreeView.SelectedNode != null)
			//    m_tsm.switchToTabSet(mainTreeView.SelectedNode.Name);

		}

		private void connectionContextMenuStrip_Opening(object sender, EventArgs e)
		{
			if (m_clickedNode.Nodes.Count > 0)
				connectToolStripMenuItem.Text = "Disconnect";
			else
				connectToolStripMenuItem.Text = "Connect";
		}

		private void newConnectionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Hashtable data = Helper.deepClone(PopupInitData);
			PopupQuery newConnectionForm = new PopupQuery(data);

            /* FIXME
			/// create popup query window and block until it's closed
			if (DialogResult.OK == newConnectionForm.ShowDialog())
			{
				// check for key collision and generate a new key if needed
				string newConnectionKey = (string)((Hashtable)data["Values"])["name"];
				if (m_availableConnections.ContainsKey(newConnectionKey))
					newConnectionKey = Helper.findAvailableSimilarKey(m_availableConnections.ToHashtable(), newConnectionKey);
				m_availableConnections.Add(newConnectionKey, (Hashtable)data["Values"]);

				saveConnectionsToSettings(m_availableConnections, newConnectionKey);

				refreshDisplayedConnections();
			}
            */
		}


	}
}
