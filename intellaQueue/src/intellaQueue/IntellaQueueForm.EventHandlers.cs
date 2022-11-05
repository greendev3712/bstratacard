using Lib;
using Microsoft.VisualBasic;
using QueueLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace intellaQueue
{
    public partial class IntellaQueueForm
    {
        //----------------------------------------------------------------------
        // Event Handlers
        //----------------------------------------------------------------------
   

        /////////////////////////////////////////////////////////////////////////////////
        /// Callback for event when doubleclick is detected in the mainTreeView in left 
        /// panel.
        ///
        /// A call is made to open a new tab and form correspoding to the doubleclicked 
        /// item.
        /// @param sender ref to the mainTreeView
        /// @param e EventArgs
        private void m_connectionTreeView_DoubleClick(object sender, EventArgs e) {
            // if no node is selected, cancel
            if (m_connectionTreeView.SelectedNode == null)
                return;

            // check if level of node is root (connections) 
            if (m_connectionTreeView.Nodes.Contains(m_connectionTreeView.SelectedNode)) {
                //if node is not already in connected state
                if (m_connectionTreeView.SelectedNode.Nodes.Count == 0) {
                    // open this node/connection
                    m_cm.switchToConnection(m_connectionTreeView.SelectedNode.Name);
                }
            }
            //else // if not in root level, node is in form level
            //{
            //    // open this node's parent node/connection
            //    m_cm.switchToConnection(m_connectionTreeView.SelectedNode.Parent.Name);

            //    // open form in new tab
            //    TabManager.addFormInTab(Helper.find(Program.m_formNames, m_connectionTreeView.SelectedNode.Name));
            //}
        }

        private void m_connectionTreeView_MouseDown(object sender, MouseEventArgs e) {
            TreeViewHitTestInfo hti = ((TreeView)sender).HitTest(e.Location);

            string rootNodeName = null;

            if (hti.Node != null) {
                if (hti.Node.Level == 0) {
                    m_cm.ClickedNode = hti.Node;

                    // prevent node from having selected forecolor and nonhilighted backcolor when context for 
                    // non-selected node appears
                    m_connectionTreeView.SelectedNode = m_cm.ClickedNode;
                    rootNodeName = m_cm.ClickedNode.Name;
                }
                else if (hti.Node.Level == 1) {
                    rootNodeName = hti.Node.Parent.Name;
                }
            }

            if (rootNodeName != null) {
                //m_tsm.switchToTabSetAndCreateIfNeeded(rootNodeName);
            }
        }

        private void button_MouseEnter(object sender, EventArgs e) {
            adjustPictureBoxButtonImage((PictureBox) sender, (int)PictureBoxButtonEvents.Enter);
        }

        private void button_MouseLeave(object sender, EventArgs e) {
            adjustPictureBoxButtonImage((PictureBox) sender, (int) PictureBoxButtonEvents.Leave);
        }

        private void button_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            adjustPictureBoxButtonImage((PictureBox)sender, (int) PictureBoxButtonEvents.Down);
        }

        private void button_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) {
                m_doNotShowSettingsDropdownOnNextClick = false;
                return;
            }

            PictureBox p = (PictureBox)sender;
            int buttonEventCode = (int) PictureBoxButtonEvents.UpInside;

            if (e.X < 0 || e.Y < 0 || e.X >= p.Width || e.Y >= p.Height)
                buttonEventCode = (int) PictureBoxButtonEvents.UpOutside;

            int index = adjustPictureBoxButtonImage(p, buttonEventCode);
            string name = (string) p.Tag;
            if (buttonEventCode != (int) PictureBoxButtonEvents.UpInside) {
                m_doNotShowSettingsDropdownOnNextClick = false;
                return;
            }

            string queueName = null;
            bool isManagerSection = false;
            getQueueNameAndType(ref queueName, ref isManagerSection, index);

            // handle button press
            switch (name) {
                case "close":
                    if (m_allowToolbarClose) {
                        ApplicationExit();
                    }

                    // Minimize instead of closing
                    this.WindowState = FormWindowState.Minimized;
                    m_isMinimized = true;

                    break;
                case "minimize":
                    this.WindowState = FormWindowState.Minimized;
                    m_isMinimized = true;
                    break;
                case "settings":
                    if (m_doNotShowSettingsDropdownOnNextClick)
                        m_doNotShowSettingsDropdownOnNextClick = false;
                    else {
                        settingsPb.ContextMenuStrip = MainContextMenu;
                        p.ContextMenuStrip.Show(p, new System.Drawing.Point(0, p.Height));
                        settingsPb.ContextMenuStrip = null;
                    }
                    break;
                default:
                    break;
            }

            m_doNotShowSettingsDropdownOnNextClick = false;
        }

        private void MainContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            if (settingsPb.ClientRectangle.Contains(settingsPb.PointToClient(Control.MousePosition)))
                m_doNotShowSettingsDropdownOnNextClick = true;
        }

        private void MainContextMenu_Opening(object sender, CancelEventArgs e) {
            //e.Cancel = true;
        }

        //----------------------------------------------------------------------
        // Event Handlers: DropGridTemplate

        private void DropGridTemplate_CellDoubleClick(System.Object sender, System.Windows.Forms.DataGridViewCellEventArgs e) {
            if ((e.RowIndex == -1)) {
                return;
            }

            if (!m_isManager_MontorEnabled) {
                return;
            }

            DataGridView grid = (DataGridView)sender;

            // only Team Grid grid, and only if we're a manager
            if (!(bool)grid.Tag || !m_isManager)
                return;

            string agentExtension = Interaction.GetSetting("IntellaToolBar", "Config", "USER_agentExtension");
            string agentDevice = grid["agent_device", e.RowIndex].Value.ToString();
            string callerChannel = grid["caller_channel", e.RowIndex].Value.ToString();

            // notify db of action
            // TODO: Multi-Server Toolbar Project - Need to be able to monitor remote systems!
            m_main_db.DbSelectFunction("queue.agent_monitor", agentExtension, agentDevice, callerChannel);
        }

        //----------------------------------------------------------------------
        // Event Handlers: Agent Status Grid

        private void AgentStatusGrid_CellMouseClick(Object sender, DataGridViewCellMouseEventArgs e) {
            if (!m_isManager) {
                // Right click menu off the Agent Status Grid is only for Managers!
                return;
            }

            if (e.RowIndex < 0)
                return;

            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            Debug.Print("AgentStatusGrid: RIGHTMOUSEDOWNCELL " + e.RowIndex + " " + e.ColumnIndex);

            // The Clicked row
            TweakedDataGridView t              = (TweakedDataGridView)sender;
            DataGridViewRow     r              = t.Rows[e.RowIndex];
            OrderedDictionary   row_agent_data = (OrderedDictionary)r.Tag;

            // TODO: we should only build this once on startup, and any time the status code list changes
            ContextMenuStrip ms_status_all      = new ContextMenuStrip();
            ContextMenuStrip ms_status_perqueue = new ContextMenuStrip();

            string real_queue  = (string) row_agent_data["queue_name"];                   // live_queue.v_agents_status
            string agent_queue = real_queue;

            if (m_multiSite) {
                agent_queue = (string) row_agent_data["prefixed_queue_name"];
            }

            // FIXME.. Not supported for now (Need cross-site manager permission bit)
            if (!(Boolean) m_subscribedQueues[agent_queue]["IsMainSystem"]) {
                MQD("!!! Cross-Server Agent-Set-Status Not Supported");
                return;
            }
            
            string queue_longname = (string)  m_subscribedQueues[agent_queue]["HeadingText"];       // TODO: Ideally we should get this from the row data in row_agent_data, but we'll need a new data item for queue_longname in live_queue.v_agents_status
            Boolean queue_agg     = (Boolean) m_subscribedQueues[agent_queue]["QueueIsAggregate"]; // TODO: Ideally we should get this from the row data in row_agent_data, but we'll need a new data item for queue_longname in live_queue.v_agents_status

            cmpSetAgentStatusToolStripMenuItem.DropDown         = ms_status_all;
            cmpSetAgentStatusPerQueueToolStripMenuItem.DropDown = ms_status_perqueue;
            cmpSetAgentStatusPerQueueToolStripMenuItem.Text     = "Set Agent Status (" + queue_longname + ")";

            if (queue_agg) {
                // Setting agent status on an aggregate queue makes no sense.  Agents do not 'log into' an aggregate queue
                cmpSetAgentStatusPerQueueToolStripMenuItem.Visible = false;
            }
            else {
                // Only for Regular Queues (Non Aggregate)
                cmpSetAgentStatusPerQueueToolStripMenuItem.Visible = true;
            }

            QueryResultSetRecord row_agent_data_dbg = DbHelper.ConvertOrderedDictionary_To_QueryResultSetRecord(row_agent_data);

            foreach (OrderedDictionary status_code_item in m_toolbarConfig.m_managerStatusCodes) {
                string server_name     = (string) row_agent_data["server_name"];
                ToolStripItem ti_all   = ms_status_all.Items.Add(     (string) status_code_item["status_code_longname"]);

                ti_all.Tag    = new Hashtable() { { "type", "ALL" }, { "item", status_code_item } };
                ti_all.Click += cmpAgentManagerRightClickContextMenu_Click;

                // Only for Regular Queues
                if (!queue_agg) { 
                    // Setting agent status on an aggregate queue makes no sense.  Agents do not 'log into' an aggregate queue
                    ToolStripItem ti_perqueue = ms_status_perqueue.Items.Add((string) status_code_item["status_code_longname"]);

                    ti_perqueue.Tag    = new Hashtable() { { "type", "QUEUE" }, { "item", status_code_item }, { "queue", real_queue }, { "server_name", server_name } };
                    ti_perqueue.Click += cmpAgentManagerRightClickContextMenu_Click;
                }
            }

            cmpAgentManagerRightClickContextMenu_selectedAgentDevice = (string) row_agent_data["agent_device"];
            cmpAgentManagerRightClickContextMenu.Show(Cursor.Position.X, Cursor.Position.Y);
        }

        //----------------------------------------------------------------------
        // Event Handlers: Main Grid

        private void mainGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e) {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0 ||
                e.RowIndex > mainGrid.RowCount - 1 ||
                e.ColumnIndex > mainGrid.ColumnCount - 1)
                return;

            DataGridView grid = (DataGridView)sender;

            if (!grid[e.ColumnIndex, e.RowIndex].OwningColumn.Name.StartsWith("button-"))
                return;

            if (grid[e.ColumnIndex, e.RowIndex].Tag is Hashtable) {

                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                DataGridViewTextBoxCell bc = mainGrid[e.ColumnIndex, e.RowIndex] as DataGridViewTextBoxCell;

                Image img = (Image)getResourceByName((string)((Hashtable)bc.Tag)["Image"]);
                e.Graphics.DrawImageUnscaledAndClipped(img, new Rectangle(e.CellBounds.X + e.CellBounds.Width / 2 - img.Width / 2, e.CellBounds.Y + e.CellBounds.Height / 2 - img.Height / 2, img.Width, img.Height));
                e.Handled = true;
            }
        }

        private void mainGrid_MouseDown(object sender, MouseEventArgs e) {
            deSelectVisibleControls();
        }

        private void mainGrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e) {
            DataGridView grid = (DataGridView)sender;

            if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left) {
                if (m_mouseDownCellColumnIndex >= 0 &&
                    m_mouseDownCellRowIndex >= 0 &&
                    m_mouseDownCellRowIndex == e.RowIndex &&
                    m_mouseDownCellColumnIndex == e.ColumnIndex)
                    modCellColor(grid, e.ColumnIndex, e.RowIndex, ButtonCellColorState.Dark);
                else
                    modCellColor(grid, e.ColumnIndex, e.RowIndex, ButtonCellColorState.Light);

            }
            else {
                modCellColor(grid, e.ColumnIndex, e.RowIndex, ButtonCellColorState.Light);
            }
        }

        private void mainGrid_CellMouseLeave(object sender, DataGridViewCellEventArgs e) {
            modCellColor((DataGridView)sender, e.ColumnIndex, e.RowIndex, ButtonCellColorState.Base);
        }

        private void mainGrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            modCellColor((DataGridView)sender, e.ColumnIndex, e.RowIndex, ButtonCellColorState.Dark);
            m_mouseDownCellColumnIndex = e.ColumnIndex;
            m_mouseDownCellRowIndex = e.RowIndex;
            Debug.Print("MOUSEDOWNCELL " + m_mouseDownCellColumnIndex + " " + m_mouseDownCellRowIndex);
        }

        private void mainGrid_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.RowIndex < 0)
                return;

            deSelectVisibleControls();

            modCellColor((DataGridView)sender, e.ColumnIndex, e.RowIndex, ButtonCellColorState.Light);

            //Debug.Print("CLICK!");

            DataGridView grid = (DataGridView)sender;
            DataGridViewCell c = grid[e.ColumnIndex, e.RowIndex];
            Hashtable tag; // First cell in each of the data rows is tagged with the queue details 
            string queueName;
            string gridName;

            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                // We only care about the leftmost column for left mouse clicks (the queue info dropdowns)

                if (e.RowIndex < 0 ||
                    e.ColumnIndex < 0 ||
                    e.RowIndex > mainGrid.RowCount - 1 ||
                    e.ColumnIndex > mainGrid.ColumnCount - 1) {

                    return;
                }

                if (m_mouseDownCellRowIndex != e.RowIndex || m_mouseDownCellColumnIndex != e.ColumnIndex || m_mouseDownCellColumnIndex < 0 || m_mouseDownCellRowIndex < 0) {
                    // mousedown happened elsewhere from mouseup!
                    m_mouseDownCellRowIndex = -1;
                    m_mouseDownCellColumnIndex = -1;
                    return;
                }

                if (!c.OwningColumn.Name.StartsWith("button-") ||
                    !(c.Tag is Hashtable)) {

                    return;
                }

                tag = (Hashtable)c.Tag;
                queueName = (string)tag["QueueName"];
                gridName = (string)tag["GridName"];

            }
            else {
                // Right mouse click
                if ((e.RowIndex < 0) || (e.ColumnIndex != 3)) {
                    return;
                }

                DataGridViewCell mastercell = grid[0, e.RowIndex];  // Tag is stored on the first cell of the row
                tag = (Hashtable)mastercell.Tag;
                queueName = (string)tag["QueueName"];
                gridName  = (string)tag["GridName"];

                QueueRightClickContextMenu_SelectedQueue = queueName;
                QueueRightClickContextMenu.Show(Cursor.Position.X, Cursor.Position.Y);
                return;
            }

            DataGridView newDropGrid = (DataGridView)m_subscribedQueues[queueName][gridName];

            newDropGrid.Tag = (string)tag["GridName"] == "TeamGrid";

            if (newDropGrid.Visible) {
                detailViewNameLabel.Text = "";
                newDropGrid.Visible = false;
            }
            else {
                string dropGridType = (grid.Columns[e.ColumnIndex].HeaderText == "Callers") ? "Callers Waiting" : grid.Columns[e.ColumnIndex].HeaderText;
                detailViewNameLabel.Text = grid["queue", e.RowIndex].Value.ToString() + " " + dropGridType;
                hideAnyVisibleGridsExcept(newDropGrid);
                newDropGrid.Visible = true;
            }

            foreach (DataGridViewColumn col in grid.Columns) {
                if (col.Name.StartsWith("button-")) {
                    foreach (DataGridViewRow r in grid.Rows) {
                        ((Hashtable)grid[col.Index, r.Index].Tag)["Image"] = "down_triangle";
                    }
                }
            }

            ((Hashtable)c.Tag)["Image"] = ((DataGridView)newDropGrid).Visible ? "up_triangle" : "down_triangle";

            ResizeMainGrid(true);
        }

        //----------------------------------------------------------------------
        // Event Handlers: IntellaQueue Main Form

        /// Below 3 methods handle user dragging of windows without border decorations
        private void intellaQueue_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                if (sender.GetType().ToString() == "intellaQueue.IntellaQueueForm") {
                    Debug.Print("IntellaQueue TitleBar Right Mouse Click");

                    if (Debugger.IsAttached) {
                        cmpTitleBarRightClickMenu.Show(Cursor.Position.X, Cursor.Position.Y);
                    }
                }
            }

            if (sender is DataGridView) {
                DataGridViewCell c = Helper.getGridCellAtPoint((DataGridView)sender, Cursor.Position);
                if (c != null && c.OwningColumn.Name.StartsWith("button-"))
                    return;
            }

            isFormDragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void intellaQueue_MouseUp(object sender, MouseEventArgs e) {
            isFormDragging = false;
        }

        private void intellaQueue_MouseMove(object sender, MouseEventArgs e) {
            if (isFormDragging) {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        //----------------------------------------------------------------------
        // Event Handlers: Toolbar Menu Clicks
 
        private void AdminToolStripMenuItem_Click(System.Object sender, System.EventArgs e) {
            PasswordDialogForm pwForm = new PasswordDialogForm();

            pwForm.SetValidateCallback(AdminPasswordValidate);
            pwForm.SetSuccessCallback(ShowAdminSettings);
            pwForm.Show();

            //pwForm.SetDesktopLocation(MousePosition.X, MousePosition.Y);
        }

        private void AboutToolStripMenuItem_Click(System.Object sender, System.EventArgs e) {
            this.AboutWindowShow();
        }

        private void smallToolStripMenuItem_Click(object sender, EventArgs e) {
            SetInterFaceSize("small", false);
        }

        private void mediumToolStripMenuItem_Click(object sender, EventArgs e) {
            SetInterFaceSize("medium", false);
        }

        private void largeToolStripMenuItem_Click(object sender, EventArgs e) {
            SetInterFaceSize("large", false);
        }


        //----------------------------------------------------------------------
        // GUI Utility
        //----------------------------------------------------------------------
      
        private void sizeAdjustorPb_MouseUp(object sender, MouseEventArgs e) {
            m_isFormResizing = false;
        }

        private void sizeAdjustorPb_MouseDown(object sender, MouseEventArgs e) {
            m_isFormResizing    = true;
            m_resizeCursorPoint = Cursor.Position;
            m_resizeFormSize    = this.Size;
        }

        private void sizeAdjustorPb_MouseMove(object sender, MouseEventArgs e) {
            if (UserResizeEnabled && m_isFormResizing) {
                Point dif    = Point.Subtract(Cursor.Position, new Size(m_resizeCursorPoint));
                Size newSize = new Size(Point.Add(new Point(m_resizeFormSize), new Size(dif)));

                int minWidth = 1;

                if (newSize.Width >= minWidth) {
                    resize(newSize);
                }
                else if (this.Width > minWidth) {
                    resize(new Size(minWidth, newSize.Height));
                }
            }
        }

        //----------------------------------------------------------------------
        // Event Handlers: Manager - Agent Status Grid - Click on a Right-Click-Context-Menuitem

        private void cmpAgentManagerRightClickContextMenu_Click(object sender, EventArgs e) {
            ToolStripDropDownItem clicked_item = (ToolStripDropDownItem)sender;

            Hashtable agent_status_click_data   = (Hashtable) clicked_item.Tag;
            string set_status_type              = (string) agent_status_click_data["type"];
            OrderedDictionary agent_status_item = (OrderedDictionary)agent_status_click_data["item"]; // (from queue.v_queue_agent_status)
            string status_code_name             = (string) agent_status_item["status_code_name"];

            if (set_status_type == "ALL") {
                // Set status on this agent for ALL queues
                setNewAgentStatus(status_code_name, cmpAgentManagerRightClickContextMenu_selectedAgentDevice);
            }
            else if (set_status_type == "QUEUE") {
                // Set status on this agent for only this specific queue
                string queue_name = (string)agent_status_click_data["queue"];

                setNewAgentStatus(status_code_name, cmpAgentManagerRightClickContextMenu_selectedAgentDevice, queue_name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e) {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            this.TopMost = menuItem.Checked = !menuItem.Checked;
        }

        private void mainGrid_SelectionChanged(object sender, EventArgs e) {
            deSelectVisibleControls();
        }

        private void dropGrid_SelectionChanged(object sender, EventArgs e) {
            deSelectVisibleControls();

            DataGridView g = (DataGridView)sender;

            foreach (DataGridViewRow r in g.Rows)
                foreach (DataGridViewCell c in r.Cells)
                    c.Style.SelectionBackColor = g.DefaultCellStyle.SelectionBackColor;

            foreach (DataGridViewCell c in g.SelectedCells)
                ColorGridCell(c, m_subscribedQueues, g.Name);
        }
        
        private void dropGrid_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e) {
            deSelectVisibleControls();
        }

        private void dropGrid_MouseDown(object sender, MouseEventArgs e) {
            deSelectVisibleControls();
        }

        private void takeNextWaitingCallerToolStripMenuItem_Click(object sender, EventArgs e) {
            string agent_extension = Interaction.GetSetting("IntellaToolBar", "Config", "USER_agentExtension"); // TODO-FIXME>.. we already have this as a global var!

            // **** TODO-FIXME Multi-Server Toolbar Project - Secondary Priority - Take next waiting queue caller from other system!
            m_main_db.DbSelectFunction("queue.grab_queue_member_next", QueueRightClickContextMenu_SelectedQueue, agent_extension);
        }
        
        private void cmpLogoutAgentToolStripMenuItem_Click(object sender, EventArgs e)
        {
          m_main_db.DbSelectFunction("queue.agent_logout", cmpAgentManagerRightClickContextMenu_selectedAgentDevice, "MANAGER_FORCE_LOGOUT");
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.CheckForUpdatesManual();
        }
  
        ///////////////////////////////////////////////////////////////////////
        // DisplayTimer
        ///////////////////////////////////////////////////////////////////////

        private void UpdateDisplayTimer_Tick(System.Object sender, System.EventArgs e) {
            m_updateDisplayTimer.Stop();

            RotateLogFileIfNeeded();

            if (this.m_db_reconnects >= MAX_DB_RECONNECTS) {
                // Completely rebuild, reconnect and re-start all threads
                foreach (KeyValuePair<string, ToolbarServerConnection> toolbar_server_item in m_toolbarServers) {
                    ToolbarServerConnection tsc = toolbar_server_item.Value;
                    tsc.Cancel = true;
                }

                ResetApplicationAndCheckAgentLogin();         // 'Hidden' Agent Login and toolbar rebuild
                goto skip_update;
            }

            if (this.m_program_failures > MAX_PROGRAM_FAILURES) {
                ProgramError(QD.ERROR_LEVEL.FATAL, "MaxToolBarUpdateFailures", "Max Toolbar Failures Reached.  Halting updates.");
                SetStatusBarMessage(Color.Red, "Connection failed. Check network connection and restart");
                return;
            }

            if (!m_db_active) {
                // Database is in the process of reconecting or is otherwise unavailable, no data to update
                goto skip_update;
            }

            // QD.p("UpdateDisplayTimer TICK!");
            UpdateQueueDisplayAndCatchExceptions();

            // Clear Statusbar Message, if needed
            if ((DateTime.Now - m_ToolbarStatusMessageLastUpdate) >= m_ToolbarStatusMessageKeep) {
                this.SetStatusBarMessage(Color.White, "");
            }

        skip_update:
            m_updateDisplayTimer.Start();
        }

        private void UpdateDisplayTimerHealthCheck_Tick(System.Object sender, System.EventArgs e) {
            if (!m_updateDisplayTimer.Enabled) {
                // Only do health checks if we're supposed to be updating
                // m_updateDisplayTimer will be diabled if we're editing db settings, or logging in as another agent, etc
                return;
            }

           //  QD.p("UpdateDisplayTimerHealthCheck TICK!");

            int threshold = 5;

            DateTime now = DateTime.Now;

            foreach (KeyValuePair<string,ToolbarServerConnection> tsc_item in m_toolbarServers) {
                string server_name          = tsc_item.Key;
                ToolbarServerConnection tsc = tsc_item.Value;
                TimeSpan since_last_update  = (now - tsc.m_lastUpdated);

                if (false && since_last_update.Seconds > threshold) {
                    MQD(String.Format("!!! UpdateDisplayTimerHealthCheck -- DoWork ({0}) last update time > threshold {1}s ({2}s since last update)", 
                        server_name,
                        threshold,
                        since_last_update.Seconds
                    ), null);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // SysTray
        ///////////////////////////////////////////////////////////////////////

        private void cmpSysTrayIcon_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                if (!cmpSystrayContextMenu.Visible) {
                    cmpSystrayContextMenu.Show();
                    cmpSystrayContextMenu.Location = Cursor.Position;
                }
                else {
                    cmpSystrayContextMenu.Hide();
                }

                return;
            }

            // Everything else
            cmpSystrayContextMenu.Hide();
        }

        private void cmpSetAgentExtensionToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!m_IsToolbarFullySetup) {
                MessageBox.Show("Log into the main application first before using this option");
                return;
            }

            AgentLogin_Prompt();
        }
        
        private void cmpExitToolStripMenuItem_Click(object sender, EventArgs e) {
            this.ApplicationExit();
        }

        ///////////////////////////////////////////////////////////////////////
        // Debug Menu
        ///////////////////////////////////////////////////////////////////////
            
        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_MainDF.Show();
            m_MainDF.Activate();
        }

        private void debugToolStripMenuItem1_Click(object sender, EventArgs e) {
            m_MainDF.Show();
        }
    }
}
