using Lib;
using LibICP;
using QueueLib;
using IntellaScreenRecord;

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Net.NetworkInformation;
using System.Net;

namespace intellaQueue
{
    public partial class IntellaQueueForm : System.Windows.Forms.Form
    {
        private DataGridView DroppedDownGrid {
            get {
                foreach (Control c in m_dynamicControls) {
                    if (c is DataGridView && c.Visible) {
                        return (DataGridView)c;
                    }
                }

                return null;
            }
        }

        // Utility

        public static Thread GetGuiThread() {
            return m_guiThread;
        }

        private void CreateOrUpdateExtraControl(int additionalControlHeight) {
            if (this.m_extraControl == null) {
                m_extraControl = new Panel();
                this.Controls.Add(m_extraControl);
                this.m_extraControl.Padding = new System.Windows.Forms.Padding(10);
            }

            this.m_extraControlHeight += additionalControlHeight;

            // Extra space on the bottom for more controls
            m_extraControl.Size = new Size(this.Width, m_extraControl.Size.Height + additionalControlHeight);
        }

        
        private int CellValueToInt(object value, string colName) {
            int result = 0;

            if (value == null) {
                return result;
            }

            if (this.m_statusCodesEnabled) {
                // Note: Only managers will see agent_status as a grid field

                if (colName == "agent_status") {
                    // Numeric value comes from whether this is a agent (10) status code or a manager-level (20) status code
                    List<OrderedDictionary> a = m_toolbarConfig.m_managerStatusCodes;

                    // value is the longname of the status code
                    // TODO: SHould make this a Hashtable to get direct access to manger status codes...
                    foreach (OrderedDictionary status_code_item in m_toolbarConfig.m_managerStatusCodes) {
                        if (value.ToString().StartsWith("Available")) {
                            return 1; // Green
                        }

                        var status_split = value.ToString().Split('\n');

                        if (status_split[0] == (string)status_code_item["status_code_longname"]) {
                            if ((string)status_code_item["manager_only"] == "True") {
                                return 20; // Red
                            }

                            return 10; // Orange
                        }
                    }
                }
            }

            if (value is string)
                int.TryParse((string)value, out result);

            return result;
        }
        
        ///////////////////////////////////////////////////////////////////////
        // Helpers
        
        public void AboutWindowShow() {
            m_aboutForm = new QueueToolbarAboutForm();
            m_aboutForm.Show();
        }

        public void AgentLogin_Prompt(string dbHost = null, string agentNum = null, string agentExtension = null) {
            StopRefreshTimer();

            if (dbHost == null) {
                dbHost = Config_Get_DB_Host();
            }

            if (agentNum == null) {
                agentNum = Interaction.GetSetting("IntellaToolBar", "Config", "USER_agentNumber");
            }

            if (agentExtension == null) {
                agentExtension = Interaction.GetSetting("IntellaToolBar", "Config", "USER_agentExtension");
            }

            AgentLoginDialogForm agentLoginForm = new AgentLoginDialogForm();
            agentLoginForm.SetLoggerCallBack_QD(MQD);
            agentLoginForm.setServerInfo(dbHost);
            agentLoginForm.SetValidateCallback(agentLoginValidate);
            agentLoginForm.SetSuccessCallback(AgentLoginSuccess);
            agentLoginForm.setTextBoxValues(agentNum, agentExtension);

            ///////

            this.m_agentLoginPanel = agentLoginForm.GetMainPanel();

            Panel agentLoginCenteringPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(this.Width, this.m_agentLoginPanel.Height),
                // AutoSize     = true,
                // AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                // BorderStyle  = BorderStyle.Fixed3D, // For Debug
            };

            // Standard
            agentLoginForm.Show();

            // New-Style -- Not Working
            //this.Controls.Add(this.m_agentLoginPanel);
    /*
            agentLoginForm.SetIntellaQueueForm(this);

            m_agentLoginPanel.Location = new Point(0, 0);

            this.Controls.Add(this.m_agentLoginPanel);

            this.m_agentLoginPanel.Paint += delegate (object sender, PaintEventArgs e) {
                Panel p = (Panel)sender;

                p.Parent.Width = this.Width; // Toolbar width
                p.Parent.Height = p.Height;  // Caller Panel height

                int total_padding = p.Parent.Size.Width - p.Size.Width;
                int padding_left = total_padding / 2;

                p.Location = new Point(padding_left, 0);
                p.Anchor    = AnchorStyles.None;
            };

            this.Height += 300;


            //////
            */ 
        }

        ///////////////////////////////////////////////////////////////////////
        
        private bool trimAndAdjustGrid(int current_row, DataGridView grid) {
            int oldHeight = grid.Height;
            int total_rows = grid.RowCount;

            // delete out extra rows
            if ((current_row < total_rows))
                for (int i = total_rows - 1; i >= current_row; i--)
                    grid.Rows.Remove(grid.Rows[i]);

            total_rows = grid.RowCount;

            int rowheight = grid.RowCount > 0 && grid.ColumnCount > 0 ? grid[0, 0].Size.Height : grid.RowTemplate.Height;

            int minHeightRowCount = 1;
            int maxRowCount = 10;
            int bonusHeight = 0;

            if (total_rows < minHeightRowCount)
                grid.Height = (minHeightRowCount * rowheight) + grid.ColumnHeadersHeight + bonusHeight;
            else if ((total_rows <= maxRowCount))
                grid.Height = (total_rows * rowheight) + grid.ColumnHeadersHeight + bonusHeight;
            else if ((total_rows > maxRowCount))
                grid.Height = (maxRowCount * rowheight) + grid.ColumnHeadersHeight + bonusHeight;

            bool did_height_change = false;
            if (oldHeight != grid.Height) {
                //Debug.Print("Trim! " + oldHeight + "->" + grid.Height);
                did_height_change = grid.Visible;
            }

            return did_height_change;
        }
        

        ////
        // Update either the 'Calls in Queue' grid, or the 'Agent Status' grid, which ever one is active
        //
        private bool updateGridData(Dictionary<string, List<OrderedDictionary>> liveCallerData, Dictionary<string, List<OrderedDictionary>> liveAgentData) {
            //Debug.Print("updateGridData");

            bool visibleDropgridDidChangeSize = false;

            IList<string> queueNames = m_subscribedQueues.Keys;
            if (queueNames.Count < 1)
                return visibleDropgridDidChangeSize;

            m_lastOP = "updateGridData-1";

            foreach (string queueName in m_subscribedQueues.Keys) {
                bool isTeamGrid = false;
                List<DataGridView> grids = new List<DataGridView>(2);

                m_lastOP = "updateGridData-2 " + queueName;

                if (m_subscribedQueues[queueName].ContainsKey("AgentGrid")) { 
                    grids.Add((DataGridView) m_subscribedQueues[queueName]["AgentGrid"]);
                }

                if (m_subscribedQueues[queueName].ContainsKey("TeamGrid")) { 
                    grids.Add((DataGridView) m_subscribedQueues[queueName]["TeamGrid"]);
                }

                foreach (DataGridView grid in grids) {
                    // live_data is either LiveAgentData or LiveCallerData
                    // The grids for both are handled the same
                    //
                    Dictionary<string, List<OrderedDictionary>> live_data = isTeamGrid ? liveAgentData : liveCallerData;

                    m_lastOP = "updateGridData-3 " + isTeamGrid.ToString();

                    if (live_data == null) {
                        Debug.Print("Result is null");
                        isTeamGrid = true;
                        continue;
                    }

                    // save user row sorting by column
                    SortOrder sortOrder = grid.SortOrder;
                    DataGridViewColumn sortedColumn = grid.SortedColumn;

                    string selectedRowUniqueIdValue = "";
                    try {
                        if (grid.SelectedRows.Count > 0)
                            selectedRowUniqueIdValue =
                                grid[isTeamGrid ? "agent_id" : "live_queue_callers_item_id", grid.SelectedRows[0].Index]
                                    .Value.ToString();
                    }
                    catch (Exception e) {
                        // FIXME EXCEPTION HANDLING
                        Debug.Print("Failed a grid update: " + e.ToString());
                    }

                    m_lastOP = "updateGridData-4 " + isTeamGrid.ToString();

                    int current_row = 0;
                    int total_rows = grid.RowCount;

                    try {
                        foreach (string queue_name in live_data.Keys)
                            foreach (OrderedDictionary resultRow in live_data[queue_name]) {
                                if (!queue_name.Equals(grid.Name))
                                    break;

                                m_lastOP = "updateGridData-5 " + isTeamGrid.ToString() + "" + queue_name;

                                if ((current_row + 1) > total_rows) {
                                    // Add an empty at the end
                                    grid.Rows.Add("", "", "", "", "");
                                }

                                // Store the full agent status db row in the grid tag
                                grid.Rows[current_row].Tag = resultRow;

                                m_lastOP = "updateGridData-6 " + isTeamGrid.ToString() + "" + queue_name;

                                foreach (DataGridViewColumn c in grid.Columns) {
                                    string col_name = c.Name;;
                                    string newValue = "";

                                    m_lastOP = "updateGridData-7 " + isTeamGrid.ToString() + "" + queue_name + " " + col_name;

                                    // !!! possible null dereference
                                    if (resultRow[col_name] == null) {
                                      col_name = "";
                                      MQD("updateGridData() !!! Got null on field name {0}", col_name);
                                    }

                                    newValue = resultRow[col_name].ToString();

                                    m_lastOP = "updateGridData-8 " + isTeamGrid.ToString() + "" + queue_name + " " + col_name;

                                    if (c.Name == "agent_callerid_num")
                                        newValue = "x" + newValue;

                                    m_lastOP = "updateGridData-9 " + isTeamGrid.ToString() + " " + queue_name + " " + col_name;

                                    if (c.Name == "agent_firstname") {
                                        newValue += " " + resultRow["agent_lastname"];

                                        if (string.IsNullOrEmpty(newValue))
                                            newValue = "(Unknown)";
                                    }

                                    m_lastOP = "updateGridData-10 " + isTeamGrid.ToString() + " " + queue_name + " " + col_name;

                                    grid[c.Name, current_row].Value = newValue;
                                }

                                current_row++;
                            }
                    }
                    catch (Exception ex) {
                        handleError(ex, "agent or caller dropgrid processing failed");
                        ProgramError(QD.ERROR_LEVEL.NOTICE, "GUI_AgentOrCallerDropGridProcessingFailed", "");
                        if (Debugger.IsAttached) { throw; }

                        return visibleDropgridDidChangeSize;
                    }

                    m_lastOP = "updateGridData-11 " + isTeamGrid.ToString();

                    // restore user row sorting by column
                    if (sortOrder != SortOrder.None && sortedColumn != null && selectedRowUniqueIdValue != "") {
                        grid.Sort(sortedColumn,
                            sortOrder == SortOrder.Ascending
                                ? ListSortDirection.Ascending
                                : ListSortDirection.Descending);

                        foreach (DataGridViewRow r in grid.Rows) {
                            m_lastOP = "updateGridData-12 " + isTeamGrid.ToString();

                            if (r.Cells[isTeamGrid ? "agent_id" : "live_queue_callers_item_id"].Value.ToString() ==
                                selectedRowUniqueIdValue)
                                r.Selected = true;
                            }
                    }

                    m_lastOP = "updateGridData-13 " + isTeamGrid.ToString();

                    visibleDropgridDidChangeSize = trimAndAdjustGrid(current_row, grid) || visibleDropgridDidChangeSize;

                    // if this loop goes a 2nd time, process as a team grid
                    isTeamGrid = true;
                }
            }

            return visibleDropgridDidChangeSize;
        }

        private void ClearToolbarData() {
           if (Thread.CurrentThread != m_guiThread) {
               this.Invoke((MethodInvoker) _ClearToolbarData);
               return;
           }

           _ClearToolbarData();
        }

        private void _ClearToolbarData() {
            hideAnyVisibleGridsExcept(null);

            if (m_subscribedQueues != null)
                m_subscribedQueues.Clear();

            clearDropGrids();
            mainGrid.Rows.Clear();
            mainGrid.Columns.Clear();
        }

        private void AdjustFormHeight(int gridHeight) {
            int old_height = this.Height;
            this.Height = m_extraControlHeight + titlePb.Height + mainGrid.Height + footerPb.Height + gridHeight +
                          (gridHeight > 0 ? MidBarHeight : 0);

            if (old_height != this.Height)
                Debug.Print("ADJUST HEIGHT " + old_height + "->" + this.Height);
        }

        private void UpdateMainGridData(Dictionary<string, List<OrderedDictionary>> liveQueueData) {
            //List<string> die_die_die = null;
            //Console.WriteLine(die_die_die[0]);  // Fancy little exception

            if (mainGrid.Rows.Count != m_subscribedQueues.Keys.Count) {
                mainGrid.Rows.Clear();

                if (m_subscribedQueues.Keys.Count > 0)
                    mainGrid.Rows.Add(m_subscribedQueues.Keys.Count);
            }

            int i = 0;

            foreach (string queue_name in m_subscribedQueuesOrder.Keys) {
                string queue_heading_text = "";

                if (queue_name == "all") {
                    Debug.Print("");
                }

                if (queue_name != "") {
                    queue_heading_text = m_subscribedQueues[queue_name].ContainsKey("HeadingText")
                        ? m_subscribedQueues[queue_name]["HeadingText"].ToString()
                        : queue_name;
                }

                foreach (DataGridViewColumn col in mainGrid.Columns) {
                    if (col.Name.StartsWith("button-")) {
                        string grid_name = m_enableTeamView && col.Index == 0 ? "TeamGrid" : "AgentGrid";

                        if (mainGrid[col.Name, i].Tag == null) {
                            DataGridViewCell new_button = new DataGridViewTextBoxCell();

                            new_button.Style.BackColor = mainGrid[col.Name, i].Style.BackColor;

                            Hashtable queue_data = m_subscribedQueues[queue_name];
                            DataGridView grid    = (DataGridView)queue_data[grid_name];

                            if (grid != null) {
                                string button_name = grid.Visible ? "up_triangle" : "down_triangle";
                                Color backcolor    = mainGrid[col.Name, i].Style.BackColor;

                                new_button.Tag = new Hashtable() {
                                    {"QueueName", queue_name},
                                    {"GridName", grid_name}, {
                                        "Image",
                                        button_name
                                    },
                                    {"Color", backcolor}
                                };
                            }

                            mainGrid[col.Name, i] = new_button;
                        }
                        else {
                            ((Hashtable) mainGrid[col.Name, i].Tag)["Image"] =
                                ((DataGridView)m_subscribedQueues[queue_name][grid_name]).Visible
                                    ? "up_triangle"
                                    : "down_triangle";
                        }

                        continue;
                    }
                    else {
                        switch (col.Name) {
                            case "queue":
                                mainGrid[col.Name, i].Value = queue_heading_text;
                                continue;
                            case "host":
                                mainGrid[col.Name, i].Value = this.Config_Get_DB_Host();
                                continue;
                            case "system":
                                if (m_multiSite) {
                                    string queue_system_text = m_subscribedQueues[queue_name].ContainsKey("SystemLongName")
                                            ? m_subscribedQueues[queue_name]["SystemLongName"].ToString()
                                            : "";

                                    mainGrid[col.Name, i].Value = queue_system_text;

                                }

                                continue;                                
                        }
                    }

                    string new_value = "";

                    if (liveQueueData.ContainsKey(queue_name)) {
                        if (liveQueueData[queue_name].Count > 0) {
                            if (Debugger.IsAttached) { 
                               OrderedDictionary queue_data = liveQueueData[queue_name][0];
                               QueryResultSetRecord queue_data_record = DbHelper.ConvertOrderedDictionary_To_QueryResultSetRecord(queue_data);
                            }

                            new_value = (string) liveQueueData[queue_name][0][col.Name];
                        }
                    }

                    if (new_value != null) {
                        // This handles updates for:
                        //  agents_logged_in (Staffed)
                        //  agents_idle      (Available)
                        //  

                        mainGrid[col.Name, i].Value = new_value;
                    }
                }

                i++;
            }

            ResizeMainGrid();
        }

        // these are used for place holders and to some extent initializers for dynamically created ui elements
        private void hideUiManagedObjects() {
            titlePb.Visible = bodyPb.Visible = footerPb.Visible = dropGridTemplate.Visible = false;
        }

        private void clearDropGrids() {
            List<Control> controlsToRemove = new List<Control>(m_dynamicControls.Count);

            foreach (Control c in m_dynamicControls)
                if (c is DataGridView) {
                    Debug.Print("this.Remove! " + c.Name);
                    this.Controls.Remove(c);
                    controlsToRemove.Add(c);
                }

            foreach (Control c in controlsToRemove) {
                Debug.Print("dyn.Remove! " + c.Name);
                m_dynamicControls.Remove(c);
            }

            Debug.Print("clearDropGrids: not removed count: " + m_dynamicControls.Count);

        }

        private void clearDynamicControls() {
            List<Control> controlsToRemove = new List<Control>(m_dynamicControls.Count);

            foreach (Control c in m_dynamicControls)
                if (!(c is DataGridView)) {
                    Debug.Print("this.Remove! " + c.Name);
                    this.Controls.Remove(c);
                    controlsToRemove.Add(c);
                }

            foreach (Control c in controlsToRemove) {
                Debug.Print("dyn.Remove! " + c.Name);
                m_dynamicControls.Remove(c);
            }

            Debug.Print("clearDynamicControls: not removed count: " + m_dynamicControls.Count);
        }

        private void drawTiledImage(Graphics g, Bitmap tiledImage, Point location, Size size, Point offset) {
            Bitmap leftCap = null, rightCap = null;
            drawTiledImageWithCaps(g, tiledImage, leftCap, rightCap, location, size, offset);
        }

        private void drawTiledImageWithCaps(Graphics g, Bitmap tiledImage, Bitmap leftCap, Bitmap rightCap,
            Point location, Size size) {
            drawTiledImageWithCaps(g, tiledImage, leftCap, rightCap, location, size, new Point(0, 0));
        }

        /// @todo handle if tile width doesn't divide perfectly into available size.width (add one tile)
        /// @todo also in above case crop last tile to only fill available size.width
        /// @todo support above two items w/ height
        private void drawTiledImageWithCaps(Graphics g, Bitmap tiledImage, Bitmap leftCap, Bitmap rightCap,
            Point location, Size size, Point offset) {
            int left_cap_width = leftCap == null ? 0 : leftCap.Width;
            int right_cap_width = rightCap == null ? 0 : rightCap.Width;

            if (left_cap_width + right_cap_width > size.Width) {
                left_cap_width = right_cap_width = size.Width / 2;
                if (left_cap_width + right_cap_width < size.Width)
                    right_cap_width++;
            }

            if (left_cap_width > 0) {
                g.DrawImage(leftCap,
                    location.X + offset.X,
                    location.Y + offset.Y,
                    left_cap_width,
                    leftCap.Height);
            }
            if (right_cap_width > 0) {
                g.DrawImage(rightCap,
                    location.X + offset.X + size.Width - right_cap_width,
                    location.Y + offset.Y,
                    right_cap_width,
                    rightCap.Height);
            }

            int tileNumWidth = (size.Width - left_cap_width - right_cap_width) / tiledImage.Width;
            int tileNumHeight = size.Height / tiledImage.Height;

            if (tileNumWidth > tiledImage.Height && tiledImage.Width == 1) {
                // optimized special case
                int baseStartX = location.X + offset.X + left_cap_width;
                int baseEndX = baseStartX + tileNumWidth;
                int baseY = location.Y + offset.Y;
                for (int i = 0; i < tiledImage.Height; i++)
                    g.DrawLine(new Pen(tiledImage.GetPixel(0, i)),
                        baseStartX,
                        baseY + i,
                        baseEndX,
                        baseY + i);
            }
            else if (tileNumHeight > tiledImage.Width && tiledImage.Height == 1) {
                // optimized special case
                // this one only has a minimal effect for our case.. maybe combine the two calls for the two grids into one?
                // would have to make sure midbar gets drawn after/on top
                int baseStartY = location.Y + offset.Y;
                int baseEndY = baseStartY + tileNumHeight;
                int baseX = location.X + offset.X + left_cap_width;
                for (int i = 0; i < tiledImage.Width; i++)
                    g.DrawLine(new Pen(tiledImage.GetPixel(i, 0)),
                        baseX + i,
                        baseStartY,
                        baseX + i,
                        baseEndY);
            }
            else
                for (int j = 0; j < tileNumHeight; j++)
                    for (int i = 0; i < tileNumWidth; i++)
                        g.DrawImage(tiledImage,
                            location.X + offset.X + left_cap_width + i * tiledImage.Width,
                            location.Y + offset.Y + j * tiledImage.Height,
                            tiledImage.Width,
                            tiledImage.Height);
        }

        private void drawTiledImageWithCaps(Graphics g, Bitmap hvTiledImage, Bitmap hTiledImageTop,
            Bitmap hTiledImageBottom, Bitmap vTiledLeftCap, Bitmap leftCapTop, Bitmap leftCapBottom,
            Bitmap vTiledRightCap, Bitmap rightCapTop, Bitmap rightCapBottom, Point location, Size size) {
            drawTiledImageWithCaps(g, hvTiledImage, hTiledImageTop, hTiledImageBottom, vTiledLeftCap, leftCapTop,
                leftCapBottom, vTiledRightCap, rightCapTop, rightCapBottom, location, size, new Point(0, 0));
        }

        /// @todo handle if tile width doesn't divide perfectly into available size.width (add one tile)
        /// @todo also in above case crop last tile to only fill available size.width
        /// @todo support above two items w/ height
        private void drawTiledImageWithCaps(Graphics g, Bitmap hvTiledImage, Bitmap hTiledImageTop,
            Bitmap hTiledImageBottom, Bitmap vTiledLeftCap, Bitmap leftCapTop, Bitmap leftCapBottom,
            Bitmap vTiledRightCap, Bitmap rightCapTop, Bitmap rightCapBottom, Point location, Size size, Point offset) {
            // left top, left bottom, left middle
            if (leftCapTop != null)
                g.DrawImage(leftCapTop,
                    location.X + offset.X,
                    location.Y + offset.Y,
                    leftCapTop.Width,
                    leftCapTop.Height);
            if (leftCapBottom != null)
                g.DrawImage(leftCapBottom,
                    location.X + offset.X,
                    location.Y + size.Height - leftCapBottom.Height + offset.Y,
                    leftCapBottom.Width,
                    leftCapBottom.Height);
            int tileNumHeight = (size.Height - leftCapTop.Height - leftCapBottom.Height) / vTiledLeftCap.Height;
            for (int i = 0; i < tileNumHeight; i++)
                g.DrawImage(vTiledLeftCap,
                    location.X + offset.X,
                    location.Y + leftCapTop.Height + offset.Y + i * vTiledLeftCap.Height,
                    vTiledLeftCap.Width,
                    vTiledLeftCap.Height);

            // right top, right bottom, right middle
            if (rightCapTop != null)
                g.DrawImage(rightCapTop,
                    location.X + size.Width - rightCapTop.Width + offset.X,
                    location.Y + offset.Y,
                    rightCapTop.Width,
                    rightCapTop.Height);
            if (rightCapBottom != null)
                g.DrawImage(rightCapBottom,
                    location.X + size.Width - rightCapTop.Width + offset.X,
                    location.Y + size.Height - rightCapBottom.Height + offset.Y,
                    rightCapBottom.Width,
                    rightCapBottom.Height);
            tileNumHeight = (size.Height - rightCapTop.Height - rightCapBottom.Height) / vTiledRightCap.Height;
            for (int i = 0; i < tileNumHeight; i++)
                g.DrawImage(vTiledRightCap,
                    location.X + size.Width - rightCapTop.Width + offset.X,
                    location.Y + rightCapTop.Height + offset.Y + i * vTiledRightCap.Height,
                    vTiledRightCap.Width,
                    vTiledRightCap.Height);

            // center top
            int tileNumWidth = (size.Width - leftCapTop.Width - rightCapTop.Width) / hTiledImageTop.Width;
            if (tileNumWidth > hTiledImageTop.Height && hTiledImageTop.Width == 1) {
                // special case optimization
                int baseStartX = location.X + leftCapTop.Width + offset.X;
                int baseY = location.Y + offset.Y;
                int baseEndX = baseStartX + tileNumWidth;
                for (int i = 0; i < hTiledImageTop.Height; i++)
                    g.DrawLine(new Pen(hTiledImageTop.GetPixel(0, i)), baseStartX, baseY + i, baseEndX, baseY + i);
            }
            else
                for (int i = 0; i < tileNumWidth; i++)
                    g.DrawImage(hTiledImageTop,
                        location.X + leftCapTop.Width + offset.X + i * hTiledImageTop.Width,
                        location.Y + offset.Y,
                        hTiledImageTop.Width,
                        hTiledImageTop.Height);

            // center bottom
            tileNumWidth = (size.Width - leftCapBottom.Width - rightCapBottom.Width) / hTiledImageBottom.Width;
            if (tileNumWidth > hTiledImageBottom.Height && hTiledImageBottom.Width == 1) {
                // special case optimization
                int baseStartX = location.X + leftCapBottom.Width + offset.X;
                int baseY = location.Y + size.Height - hTiledImageBottom.Height + offset.Y;
                int baseEndX = baseStartX + tileNumWidth;
                for (int i = 0; i < hTiledImageBottom.Height; i++)
                    g.DrawLine(new Pen(hTiledImageBottom.GetPixel(0, i)), baseStartX, baseY + i, baseEndX, baseY + i);
            }
            else
                for (int i = 0; i < tileNumWidth; i++)
                    g.DrawImage(hTiledImageBottom,
                        location.X + leftCapBottom.Width + offset.X + i * hTiledImageBottom.Width,
                        location.Y + size.Height - hTiledImageBottom.Height + offset.Y,
                        hTiledImageBottom.Width,
                        hTiledImageBottom.Height);

            // center middle
            tileNumWidth = (size.Width - leftCapTop.Width - rightCapTop.Width) / hvTiledImage.Width;
            tileNumHeight = (size.Height - hTiledImageTop.Height - hTiledImageBottom.Height) / hvTiledImage.Height;

            if (hvTiledImage.Width == 1 && hvTiledImage.Height == 1)
                // special case optimization
                g.FillRectangle(new SolidBrush(hvTiledImage.GetPixel(0, 0)),
                    new Rectangle(location.X + offset.X + leftCapTop.Width,
                        location.Y + offset.Y + hTiledImageTop.Height,
                        tileNumWidth,
                        tileNumHeight));
            else
                for (int j = 0; j < tileNumHeight; j++)
                    for (int i = 0; i < tileNumWidth; i++)
                        g.DrawImage(hvTiledImage,
                            location.X + offset.X + leftCapTop.Width + i * hvTiledImage.Width,
                            location.Y + offset.Y + hTiledImageTop.Height + j * hvTiledImage.Height,
                            hvTiledImage.Width,
                            hvTiledImage.Height);
        }

        private void LayoutForm() {
            Debug.Print("layout form!");
            clearDynamicControls();

            if (m_subscribedQueues == null) {
                return;
            }

            int queue_count = m_subscribedQueues.Count;
            int grid_height = 0;
            bool did_find_visible_dropgrid = false;

            for (int i = 0; i < queue_count; i++) {
                string queue_name = m_subscribedQueues.Keys[i];
                List<DataGridView> grids = new List<DataGridView>(2) {
                    (DataGridView) m_subscribedQueues[queue_name]["AgentGrid"]
                };

                if (m_subscribedQueues[queue_name].ContainsKey("TeamGrid"))
                    grids.Add((DataGridView)m_subscribedQueues[queue_name]["TeamGrid"]);

                foreach (DataGridView grid in grids) {
                    if (grid != null && grid.Visible) {
                        did_find_visible_dropgrid = true;
                        grid_height = grid.Height;
                        break;
                    }
                }

                if (grid_height > 0) {
                    break;
                }
            }

            if (!did_find_visible_dropgrid)
                detailViewNameLabel.Text = "";

            int predicted_body_height = mainGrid.Height;
            int predicted_offset_y = mainGrid.Height + ((grid_height > 0) ? MidBarHeight : 0);
            Bitmap bg = new Bitmap(this.Width,
                titlePb.Height + predicted_body_height + footerPb.Height + grid_height +
                ((grid_height > 0) ? MidBarHeight : 0));

            using (Graphics g = Graphics.FromImage(bg)) {
                // top window bar
                drawTiledImageWithCaps(g,
                    queueResources.title_empty_center_tile,
                    queueResources.title_empty_left,
                    queueResources.title_empty_right,
                    titlePb.Location,
                    new Size(this.Width, queueResources.title_empty_left.Height),
                    new Point(0, 0));

                string agent_num_titlebar = "";

                if (m_showAgentNumberInTitleBar) {
                    agent_num_titlebar = "(" + m_agentNum + ") ";
                }

                if ((m_agentNum == null) || (m_agentNum == "")) {
                    titleViewNameLabel.Text = (m_isManager ? "Manager" : "Agent") + " Toolbar - " + m_agentName + " [" + m_agentExtension + "]"; ;
                }
                else {
                    titleViewNameLabel.Text = (m_isManager ? "Manager" : "Agent") + " Toolbar - " + agent_num_titlebar + m_agentName + " [" + m_agentExtension + "]";
                }

                titleViewNameLabel.Location = new Point(this.Width / 2 - titleViewNameLabel.Width / 2, titleViewNameLabel.Location.Y);

                // left border
                drawTiledImage(g,
                    queueResources.border_left_tiled,
                    new Point(titlePb.Location.X, titlePb.Location.Y + queueResources.title_empty_left.Height),
                    new Size(queueResources.border_left_tiled.Width, mainGrid.Height),
                    new Point(0, 0));

                // right border
                drawTiledImage(g,
                    queueResources.border_right_tiled,
                    new Point(titlePb.Location.X + this.Width - queueResources.border_right_tiled.Width,
                        titlePb.Location.Y + queueResources.title_empty_left.Height),
                    new Size(queueResources.border_right_tiled.Width, mainGrid.Height),
                    new Point(0, 0));

                // bottom
                drawTiledImageWithCaps(g,
                    queueResources.footer_center_tile,
                    queueResources.footer_left,
                    queueResources.footer_right,
                    new Point(footerPb.Location.X,
                        queueResources.title_empty_center_tile.Height + predicted_offset_y + grid_height),
                    new Size(this.Width, 100 + queueResources.footer_left.Height));

                if (grid_height > 0) {
                    Point offset = new Point(0, predicted_offset_y);
                    drawTiledImageWithCaps(g,
                        queueResources.midbar_center_center_hvtiled,
                        queueResources.midbar_center_top_htiled,
                        queueResources.midbar_center_bottom_htiled,
                        queueResources.midbar_left_center_vtiled,
                        queueResources.midbar_left_top,
                        queueResources.midbar_left_bottom,
                        queueResources.midbar_right_center_vtiled,
                        queueResources.midbar_right_top,
                        queueResources.midbar_right_bottom,
                        new Point(titlePb.Location.X, titlePb.Location.Y + titlePb.Height + mainGrid.Height),
                        new Size(this.Width, MidBarHeight),
                        new Point(0, 0));

                    detailViewNameLabel.Location = new Point(
                        this.Width / 2 - detailViewNameLabel.Width / 2,
                        queueResources.title_empty_center_tile.Height + mainGrid.Height + MidBarHeight / 2 -
                        detailViewNameLabel.Height / 2);

                    // left border
                    drawTiledImage(g,
                        queueResources.border_left_tiled,
                        bodyPb.Location,
                        new Size(queueResources.border_left_tiled.Width, grid_height),
                        offset);

                    // right border
                    drawTiledImage(g,
                        queueResources.border_right_tiled,
                        new Point(bodyPb.Location.X + this.Width - queueResources.border_right_tiled.Width,
                            bodyPb.Location.Y),
                        new Size(queueResources.border_right_tiled.Width, grid_height),
                        offset);
                }
            }

            this.BackgroundImage = bg;
            this.BackgroundImageLayout = ImageLayout.None;

            int width_diff = this.Width - bodyPb.Width;

            AdjustFormHeight(grid_height);
            sizeAdjustorPb.Location = new Point(m_sizeAdjustorBaseLocation.X + width_diff,
                m_sizeAdjustorBaseLocation.Y + predicted_offset_y + grid_height);
            statusLight.Location = new Point(m_statusLightBaseLocation.X + width_diff,
                m_statusLightBaseLocation.Y + predicted_offset_y + grid_height);

            // Using hidden control because WY_Updater has 'Download and Install' which doesn't work, since we're using a background service updater
            // cmpAutomaticWY_Updater.Location = new Point(queueResources.border_left_tiled.Width, statusLight.Location.Y);
            this.m_cmpToolbarStatusMessage.Location = new Point(queueResources.border_left_tiled.Width, statusLight.Location.Y + 1);

            int drop_grid_y = titlePb.Height + predicted_offset_y;
            for (int i = 0; i < queue_count; i++) {
                string queue_name = m_subscribedQueues.Keys[i];
                List<DataGridView> grids = new List<DataGridView>(2);
                grids.Add((DataGridView)m_subscribedQueues[queue_name]["AgentGrid"]);

                if (m_subscribedQueues[queue_name].ContainsKey("TeamGrid"))
                    grids.Add((DataGridView)m_subscribedQueues[queue_name]["TeamGrid"]);

                foreach (DataGridView grid in grids.Where(grid => grid != null && grid.Visible)) {
                    grid.Location = new Point(queueResources.border_left_tiled.Width, drop_grid_y);
                    break;
                }
            }

            closePb.Location = new Point(m_closeButtonBaseLocation.X + width_diff, m_closeButtonBaseLocation.Y);
            minimizePb.Location = new Point(m_minimizeButtonBaseLocation.X + width_diff, m_minimizeButtonBaseLocation.Y);
            settingsPb.Location = new Point(m_settingsButtonBaseLocation.X + width_diff, m_settingsButtonBaseLocation.Y);

            if (this.m_extraControl != null) {
                m_extraControl.Location = new Point(0, this.Height - m_extraControlHeight);
                m_extraControl.Size = new Size(this.Width, m_extraControlHeight);

                if (m_statusCodesEnabled) {
                    int unused_x = this.Width - m_statusCodesPanel.Width;

                    m_statusCodesCenteringPanel.Width = this.Width;
                    m_statusCodesPanel.Location = new Point(unused_x / 2, 0);
                }
            }

            SetInterFaceSize(m_interfaceSize, true);
        }

        private void SetInterFaceSize(string size, bool firstStart) {

            switch (size) {
                case "large":
                    this.mainGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.5F);
                    this.mainGrid.ColumnHeadersHeight = 29;
                    this.mainGrid.RowTemplate.Height = 26;
                    this.smallToolStripMenuItem.Checked = false;
                    this.mediumToolStripMenuItem.Checked = false;
                    this.largeToolStripMenuItem.Checked = true;
                    break;
                case "medium":
                    this.mainGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F);
                    this.mainGrid.ColumnHeadersHeight = 20;
                    this.mainGrid.RowTemplate.Height = 18;
                    this.smallToolStripMenuItem.Checked = false;
                    this.mediumToolStripMenuItem.Checked = true;
                    this.largeToolStripMenuItem.Checked = false;
                    break;
                case "small":
                    this.mainGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
                    this.mainGrid.ColumnHeadersHeight = 16;
                    this.mainGrid.RowTemplate.Height = 13;
                    this.smallToolStripMenuItem.Checked = true;
                    this.mediumToolStripMenuItem.Checked = false;
                    this.largeToolStripMenuItem.Checked = false;
                    break;
                default:
                    return;
            }

            m_interfaceSize = size;
            Interaction.SaveSetting("IntellaToolBar", "Config", "InterfaceSize", size);

            // Need a better way to redraw the interface... we don't really need to reconnect
            if (!firstStart) {
                Error_DatabaseMainConnection_Failed();
            }
        }

        private void DrawImage(Graphics g, Bitmap b, Point location, Point offset) {
            try {
                g.DrawImage(b, location.X + offset.X, location.Y + offset.Y, b.Size.Width, b.Size.Height);
            }
            catch (Exception ex) {
                handleError(ex, "DrawImage() Failed");
                if (Debugger.IsAttached) { throw; }
            }
        }
        
        private void SetupMainGrid() {
            mainGrid.Location = new Point(queueResources.border_left_tiled.Width, queueResources.title_empty_center_tile.Height);
            mainGrid.Size = new Size(this.Width - 2 * queueResources.border_left_tiled.Width, 20);

            mainGrid.Columns.Clear();

            if (m_enableTeamView) {
                mainGrid.Columns.Add("button-TeamGrid", "Agents");
            }

            mainGrid.Columns.Add("button-agentGrid", "Callers");
            if (m_enableTeamView) {
                mainGrid.Columns.Add("agents_logged_in", "Staffed");
                mainGrid.Columns.Add("agents_idle", "Available");
                mainGrid.Columns.Add("max_agent_talk_duration_time", "Longest Talk");
                mainGrid.Columns.Add("longest_current_hold_time", "Longest Hold");
            }
            mainGrid.Columns.Add("callers_waiting", "Callers Waiting");
            mainGrid.Columns.Add("longest_waiting_time", "Longest Waiting");

            Font old_font = mainGrid.DefaultCellStyle.Font;

            if (m_multiSite) {
                mainGrid.Columns.Add("system", "System");
                mainGrid.Columns["system"].DefaultCellStyle.Font = new Font(old_font, old_font.Style | FontStyle.Bold);
            }

            mainGrid.Columns.Add("queue", "Queue Name");
            mainGrid.Columns[mainGrid.Columns.Count - 1].DefaultCellStyle.Font = new Font(old_font, old_font.Style | FontStyle.Bold);
            mainGrid.Columns[mainGrid.Columns.Count - 1].DefaultCellStyle.Padding = new Padding(15, 0, 15, 0);

            if (m_toolbarConfig.m_showLocationField) {
                mainGrid.Columns.Add("host", "Location");
                mainGrid.Columns[mainGrid.Columns.Count - 1].DefaultCellStyle.Font = new Font(old_font, old_font.Style | FontStyle.Bold);
                mainGrid.Columns[mainGrid.Columns.Count - 1].DefaultCellStyle.Padding = new Padding(15, 0, 15, 0);
            }
            else {
                // Make the queue column big so the titlebar can fit its text
                mainGrid.Columns[mainGrid.Columns.Count - 1].DefaultCellStyle.Padding = new Padding(100, 0, 100, 0);
            }

            foreach (DataGridViewColumn c in mainGrid.Columns)
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void TrimAdjustAndPositionAllDropGrids() {
            Debug.Print("TRIM ALL GRIDS!!");

            foreach (string queue_name in m_subscribedQueues.Keys) {
                List<DataGridView> grids = new List<DataGridView>(2);

                if (m_subscribedQueues[queue_name].ContainsKey("AgentGrid"))
                    grids.Add((DataGridView) m_subscribedQueues[queue_name]["AgentGrid"]);

                if (m_subscribedQueues[queue_name].ContainsKey("TeamGrid"))
                    grids.Add((DataGridView) m_subscribedQueues[queue_name]["TeamGrid"]);

                foreach (DataGridView grid in grids)
                    trimAndAdjustGrid(0, grid);
            }
        }

        private void setupGrids() {
            Debug.Print("SETUP GRIDS!!");

            if (m_subscribedQueues.Count == 0) {
                System.Windows.Forms.MessageBox.Show("You are not assigned to any queues.", "Unable to continue");
                ApplicationExit();
            }

            foreach (string queueName in m_subscribedQueues.Keys) {
                // Two Grids: Agent Grid (Shows Calls in Queue), Manager Grid (Shows Agents Activity)
                List<DataGridView> grids = new List<DataGridView>(2);

                if (m_subscribedQueues[queueName].ContainsKey("AgentGrid"))
                    grids.Add((DataGridView)m_subscribedQueues[queueName]["AgentGrid"]);

                if (m_subscribedQueues[queueName].ContainsKey("TeamGrid"))
                    grids.Add((DataGridView)m_subscribedQueues[queueName]["TeamGrid"]);

                bool is_teamview_grid = false;

                foreach (DataGridView grid in grids) {
                    grid.BackgroundColor = mainGrid.BackgroundColor;
                    grid.ColumnHeadersDefaultCellStyle = mainGrid.ColumnHeadersDefaultCellStyle.Clone();
                    //grid.ColumnHeadersDefaultCellStyle.Font = new Font(grid.ColumnHeadersDefaultCellStyle.Font.FontFamily, grid.ColumnHeadersDefaultCellStyle.Font.Size - 4);
                    grid.DefaultCellStyle = mainGrid.DefaultCellStyle.Clone();
                    grid.Font = (Font)mainGrid.Font.Clone();
                    grid.RowHeadersDefaultCellStyle = mainGrid.RowHeadersDefaultCellStyle.Clone();
                    grid.RowHeadersVisible = mainGrid.RowHeadersVisible;
                    grid.RowTemplate = (DataGridViewRow)mainGrid.RowTemplate.Clone();
                    grid.RowTemplate.Height = mainGrid.RowTemplate.Height;
                    grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    grid.AutoSizeRowsMode = mainGrid.AutoSizeRowsMode;
                    grid.ShowEditingIcon = mainGrid.ShowEditingIcon;
                    grid.AllowUserToAddRows = mainGrid.AllowUserToAddRows;
                    grid.AllowUserToDeleteRows = mainGrid.AllowUserToDeleteRows;
                    grid.AllowUserToResizeRows = false;
                    grid.ColumnHeadersHeightSizeMode = mainGrid.ColumnHeadersHeightSizeMode;
                    grid.ColumnHeadersBorderStyle = mainGrid.ColumnHeadersBorderStyle;
                    //DataGridViewHeaderBorderStyle.None;
                    grid.ReadOnly = mainGrid.ReadOnly;
                    grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                    grid.Size = dropGridTemplate.Size;
                    grid.BorderStyle = dropGridTemplate.BorderStyle;
                    grid.CellBorderStyle = mainGrid.CellBorderStyle; //dropGrid.CellBorderStyle;
                    grid.ColumnHeadersHeight = mainGrid.ColumnHeadersHeight + 1;
                    grid.DefaultCellStyle.SelectionBackColor = Helper.darkenColor(grid.DefaultCellStyle.BackColor);
                    grid.ShowCellToolTips = mainGrid.ShowCellToolTips;

                    grid.CellDoubleClick += DropGridTemplate_CellDoubleClick;
                    grid.SelectionChanged += dropGrid_SelectionChanged;
                    grid.MouseDown += dropGrid_MouseDown;
                    grid.CellMouseUp += dropGrid_CellMouseUp;
                    grid.Visible = false;

                    grid.Tag = is_teamview_grid;
                    grid.Name = queueName;

                    grid.Rows.Clear();
                    grid.Columns.Clear();

                    grid.GridColor = Color.Black;
                    grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
                    grid.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;

                    const string dropGridHeadingTextPrefix = "         ";

                    if (is_teamview_grid) {
                        grid.Columns.Add("agent_callerid_num", dropGridHeadingTextPrefix + "Ext");
                        grid.Columns.Add("agent_fullname", dropGridHeadingTextPrefix + "Agent Name");

                        if (this.m_statusCodesEnabled) {
                            int agent_status_col = grid.Columns.Add("agent_status", dropGridHeadingTextPrefix + "Agent Status");
                            grid.Columns[agent_status_col].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            grid.Columns[agent_status_col].DefaultCellStyle.Font = new Font(mainGrid.Font.FontFamily, mainGrid.Font.Size - 1);
                        }

                        if (showQueueCallerColumn) {
                            grid.Columns.Add("queue_name_caller", dropGridHeadingTextPrefix + "Queue");
                        }

                        grid.Columns.Add("talk_duration_time", dropGridHeadingTextPrefix + "Talk Time");
                        grid.Columns.Add("caller_callerid_name", dropGridHeadingTextPrefix + "Caller Name");
                        grid.Columns.Add("caller_callerid_num", dropGridHeadingTextPrefix + "Caller Number");
                        grid.Columns.Add("call_type", dropGridHeadingTextPrefix + "Call Type");
                        grid.Columns.Add("current_hold_duration_time", dropGridHeadingTextPrefix + "Hold Time");
                        grid.Columns.Add("agent_failed_to_respond_today", dropGridHeadingTextPrefix + "Missed Rings");

                        grid.Columns.Add("agent_device", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;
                        grid.Columns.Add("caller_channel", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;
                        grid.Columns.Add("agent_id", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;
                        grid.Columns.Add("talk_duration_seconds", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;
                        grid.Columns.Add("current_hold_duration_seconds", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;

                        grid.CellMouseClick += AgentStatusGrid_CellMouseClick;
                        // right mouse click on an agent to get 'Set Agent Status' 

                        // Agent Name is default sort
                        grid.Columns[1].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                    }
                    else {
                        grid.Columns.Add("callerid_num", dropGridHeadingTextPrefix + "Caller Number");
                        grid.Columns.Add("callerid_name", dropGridHeadingTextPrefix + "Caller Name");
                        grid.Columns.Add("waiting_time", dropGridHeadingTextPrefix + "Waiting Time");

                        grid.Columns.Add("live_queue_callers_item_id", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;
                        grid.Columns.Add("waiting_seconds", "");
                        grid.Columns[grid.Columns.Count - 1].Visible = false;
                    }

                    // uncomment to disable useDefaultCellStyler row sorting by column
                    //foreach (DataGridViewColumn c in grid.Columns)
                    //    c.SortMode = DataGridViewColumnSortMode.NotSortable;

                    grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    grid.MultiSelect = false;

                    // add it to the form
                    Debug.Print("Adding! " + grid.Name);
                    this.Controls.Add(grid);
                    m_dynamicControls.Add(grid);

                    // if this loop goes a second time, next grid is a team view grid
                    // (if it's enabled)
                    is_teamview_grid = m_enableTeamView;
                }
            }

            TrimAdjustAndPositionAllDropGrids();
        }

        private void toggleStatusLight() {
            m_lightStatus = !m_lightStatus;
            statusLight.BackgroundImage = m_lightStatus ? queueResources.status_ok : queueResources.status_off;
        }

        //
        // m_liveDatas has been populated by the background db update thread
        //
        private bool UpdateUiDataComponents() {
            lock (m_liveDatas) {
                Dictionary<string, List<OrderedDictionary>> live_caller_data = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["caller"];
                Dictionary<string, List<OrderedDictionary>> live_agent_data  = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["agent"];
                Dictionary<string, List<OrderedDictionary>> live_queue_data  = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["queue"];
                Dictionary<string, List<OrderedDictionary>> live_status_data = (Dictionary<string, List<OrderedDictionary>>)m_liveDatas["status"];

                bool need_to_resize_frame = updateGridData(live_caller_data, live_agent_data);

                //
                // UpdateMainGridData
                //

                // Normal Operation
                try {
                    UpdateMainGridData(live_queue_data); // exception thrown from this fn

                    if (this.m_statusCodesEnabled) {
                        m_statusCodesForm.UpdateAgentStatusIfNeeded(live_status_data);
                        m_statusCodesForm.m_statusPopulated = true;
                    }

                    UpdateBarColors(m_liveDatas);
                    return need_to_resize_frame;
                }
                catch (Exception e) {
                    StackTrace s = new StackTrace(e, true);
                    StackFrame f = s.GetFrame(0);
                    string exception_location = string.Format("{0}\r\n  {1}:{2}", f.GetFileName(), f.GetMethod().Name, f.GetFileLineNumber());

                    handleError("updateuidatacomponents(), updatemaingriddata() exception at: \r\n" + exception_location + "\r\ndetail:\r\n" + e.ToString() + "\r\n\r\nStack:" + e.StackTrace.ToString());
                    //if (Debugger.IsAttached) { throw; } // exceptiondispatchinfo.capture(e).throw();

                    //if (debugger.isattached) {
                    //    throw;
                    //    //e = e.flatten();
                    //    //exceptiondispatchinfo.capture(e.innerexception).throw();
                    //}
                }
            }

            return false;
        }

        private void ToggleStatusLightFromThread() {
            MethodInvoker inv = delegate {
                toggleStatusLight();
            };

            this.Invoke(inv);
        }


        private void HideShowQueueitem_Click(object sender, EventArgs e) {
            ToolStripMenuItem    menu_item        = (ToolStripMenuItem)sender;
            QueryResultSetRecord queue_assignment = (QueryResultSetRecord)menu_item.Tag;

            string queue_name = queue_assignment["queue_name"];

            if (m_subscribedQueuesHidden.ContainsKey(queue_name)) {
                m_subscribedQueuesHidden.Remove(queue_name);
            }
            else {
                m_subscribedQueuesHidden.Add(queue_name, "1");
            }

            AgentLoginSuccess();
            // TODO: we should redo this so we have a dynamic hide versus rebuilding the entire interface
        }

        private int updateAgentMaxes(string queueName, OrderedDictionary agentCallData) {
            int longestTalkSeconds = 0;

            if (agentCallData == null) {
                return 0;
            }

            // agent_call_data[longest_talk_seconds]
            // agent_call_data[longest_talk_time]

            try {
                longestTalkSeconds = (agentCallData["longest_talk_seconds"] == null || agentCallData["longest_talk_seconds"] == "")
                    ? 0
                    : Int32.Parse((string)agentCallData["longest_talk_seconds"]);
            }
            catch (FormatException ex) {
                handleError(ex, "Format exception " + ex.Message + " on myMaxTalkSeconds of " + agentCallData["longest_talk_seconds"]);
                if (Debugger.IsAttached) { throw; }
            }

            string longest_talk_time = (string)agentCallData["longest_talk_time"];
            longest_talk_time = (longest_talk_time != null && longest_talk_time != "") ? longest_talk_time : "00:00:00";

            int maingrid_queue_position = (int)m_subscribedQueues[queueName]["MainGridRowPosition"];
            mainGrid["max_agent_talk_duration_time", maingrid_queue_position].Value = longest_talk_time;

            return longestTalkSeconds;
        }

        private static void manuallyAutoSizeGrid(DataGridView grid) {
            if (grid.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.Fill) {
                grid.Columns[0].Width = m_agentColumnDropDownWidth;

                if (m_enableTeamView) {
                    grid.Columns[1].Width = m_agentColumnDropDownWidth;
                    grid.Columns[0].Width = m_managerColumnDropDownWidth;
                }
                return;
            }

            int newWidth = 0, newHeight = 0;
            foreach (DataGridViewColumn c in grid.Columns) {
                if (c.Visible)
                    newWidth += c.Width;
            }

            foreach (DataGridViewRow r in grid.Rows) {
                newHeight += r.Height;
            }

            if (grid.RowHeadersVisible)
                newWidth += grid.RowHeadersWidth;
            if (grid.ColumnHeadersVisible)
                newHeight += grid.ColumnHeadersHeight;

            // todo: does not support all various border styles (scollbars may appear); 
            // add support for other border styles?
            if (grid.CellBorderStyle == DataGridViewCellBorderStyle.Single)
                newWidth += 1;

            grid.Size = new Size(newWidth, newHeight);
        }


        private void ResizeMainGrid() {
            ResizeMainGrid(false);
        }

        private void ResizeMainGrid(bool doForceRedraw) {
            DataGridView g = DroppedDownGrid;
            if (g == null) // small mode
            {
                //g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                mainGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                mainGrid.AutoResizeColumns();
                manuallyAutoSizeGrid(mainGrid);

                if (m_enableTeamView) {
                    m_managerColumnDropDownWidth = mainGrid.Columns[0].Width;
                    m_agentColumnDropDownWidth = mainGrid.Columns[1].Width;
                }
                else
                    m_agentColumnDropDownWidth = mainGrid.Columns[0].Width;

                resize(
                    new Size(
                        queueResources.border_left_tiled.Width + queueResources.border_right_tiled.Width +
                        mainGrid.Width, this.Height), doForceRedraw);
            }
            else // dropped/big mode
            {
                mainGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                g.AutoResizeColumns();
                manuallyAutoSizeGrid(g);

                if (g.Width < mainGrid.Width) {
                    g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    g.AutoResizeColumns();
                    g.Width = mainGrid.Width;
                }
                else if (g.Width > mainGrid.Width)
                    mainGrid.Width = g.Width;

                mainGrid.AutoResizeColumns();
                manuallyAutoSizeGrid(mainGrid);
                resize(
                    new Size(
                        queueResources.border_left_tiled.Width + queueResources.border_right_tiled.Width + g.Width,
                        this.Height), doForceRedraw);
            }
        }

        private void UpdateBarColors(Hashtable liveDatas) {
            Dictionary<string, List<OrderedDictionary>> live_queue_data = (Dictionary<string, List<OrderedDictionary>>) liveDatas["queue"];
            Dictionary<string, List<OrderedDictionary>> agent_call_data = (Dictionary<string, List<OrderedDictionary>>) liveDatas["call"];

            foreach (string queue_name in m_subscribedQueues.Keys) {
                // ****** TEMP
                if (!agent_call_data.ContainsKey(queue_name)) {
                    continue;
                }

                //
                // Main Grid

                if (m_enableTeamView) {
                    //
                    // agent_call_data[queue_name][0][longest_talk_seconds]
                    // agent_call_data[queue_name][0][longest_talk_time]
                    //

                    if (Debugger.IsAttached) {
                        QueryResultSet        r = DbHelper.ConvertListOrderedDictionary_To_QueryResultSet(live_queue_data[queue_name]);
                        QueryResultSetRecord rr = DbHelper.ConvertOrderedDictionary_To_QueryResultSetRecord(live_queue_data[queue_name][0]);
                    }

                    string longest_waiting_seconds_str = (string) live_queue_data[queue_name][0]["longest_waiting_seconds"];
                    int longest_waiting_seconds        = Int32.Parse(longest_waiting_seconds_str);
                    int longest_talk_seconds           = updateAgentMaxes(queue_name, agent_call_data[queue_name][0]);
                    int longest_current_hold_seconds   = Int32.Parse((string) live_queue_data[queue_name][0]["longest_current_hold_seconds"]);

                    SetBarColor(longest_talk_seconds, queue_name, true, longest_current_hold_seconds, live_queue_data);
                }
                else { 
                    SetBarColor(0, queue_name, false, 0, live_queue_data);
                }

                // End Main Grid

                // drop grids
                {
                    DataGridView g = (DataGridView) m_subscribedQueues[queue_name]["AgentGrid"];
                    foreach (DataGridViewRow r in g.Rows) { 
                        ColorGridCell(r.Cells["waiting_time"], m_subscribedQueues, queue_name);
                    }

                    g.Refresh();
                }

                if (m_subscribedQueues[queue_name]["TeamGrid"] != null) {
                    DataGridView g = (DataGridView) m_subscribedQueues[queue_name]["TeamGrid"];
                    foreach (DataGridViewRow r in g.Rows) {
                        ColorGridCell(r.Cells["talk_duration_time"],         m_subscribedQueues, queue_name);
                        ColorGridCell(r.Cells["current_hold_duration_time"], m_subscribedQueues, queue_name);

                        if (this.m_statusCodesEnabled) {
                            ColorGridCell(r.Cells["agent_status"], m_subscribedQueues, queue_name);
                        }
                    }
                }
            }
        }

        private void ColorGridCell(DataGridViewCell coloredCell, SortedList<string, Hashtable> subscribedQueues, string queueName) {
            int yellowThreshold, orangeThreshold, redThreshold;

            string col_name = coloredCell.OwningColumn.Name;
            string grid_name = coloredCell.DataGridView.Name;
            object value_cell_value = "0";
            
            try {
                switch (col_name) {
                    case "callers_waiting":
                        yellowThreshold   = 3; // (int)subscribedQueues[queueName]["Threshold_warning1-Manager"];
                        orangeThreshold   = 5; // (int)subscribedQueues[queueName]["Threshold_warning2-Manager"];
                        redThreshold      = 10; // (int)subscribedQueues[queueName]["Threshold_warning3-Manager"];
                        value_cell_value  = coloredCell.OwningRow.Cells["callers_waiting"].Value;
                        break;

                    case "talk_duration_time":
                        yellowThreshold  = (int)subscribedQueues[queueName]["Threshold_warning1-Manager"];
                        orangeThreshold  = (int)subscribedQueues[queueName]["Threshold_warning2-Manager"];
                        redThreshold     = (int)subscribedQueues[queueName]["Threshold_warning3-Manager"];
                        value_cell_value = coloredCell.OwningRow.Cells["talk_duration_seconds"].Value;
                        break;
                    case "current_hold_duration_time":
                        yellowThreshold  = (int)subscribedQueues[queueName]["Hold_time_threshold_warning1-Manager"];
                        orangeThreshold  = (int)subscribedQueues[queueName]["Hold_time_threshold_warning2-Manager"];
                        redThreshold     = (int)subscribedQueues[queueName]["Hold_time_threshold_warning3-Manager"];
                        value_cell_value = coloredCell.OwningRow.Cells["current_hold_duration_seconds"].Value;
                        break;
                    case "waiting_time":
                        yellowThreshold  = (int)subscribedQueues[queueName]["Threshold_warning1-Agent"];
                        orangeThreshold  = (int)subscribedQueues[queueName]["Threshold_warning2-Agent"];
                        redThreshold     = (int)subscribedQueues[queueName]["Threshold_warning3-Agent"];
                        value_cell_value = coloredCell.OwningRow.Cells["waiting_seconds"].Value;
                        break;
                    case "agent_status":
                        yellowThreshold  = 5; // Unused
                        orangeThreshold  = 10;
                        redThreshold     = 20;
                        value_cell_value = coloredCell.OwningRow.Cells["agent_status"].Value;
                        break;
                    default:
                    case null:
                    case "":
                        return;
                }

                int cell_color_value = CellValueToInt(value_cell_value, col_name);
                coloredCell.Style.BackColor = GetColorFromValueAndThresholds(cell_color_value, yellowThreshold,
                    orangeThreshold, redThreshold);
                coloredCell.Style.SelectionBackColor = (coloredCell.Style.BackColor == QueueColorEmpty)
                    ? Helper.darkenColor(QueueColorEmpty)
                    : coloredCell.Style.BackColor;
            }
            catch (Exception e) {
                // FIXME EXCEPTION HANDLING
                Debug.Print("Failed getting thresholds: " + e.ToString());
            }
        }


        private static Color GetColorFromValueAndThresholds(int value, int yellowThreshold, int orangeThreshold, int redThreshold, int row_number = -1, Func<int, Color, Color> zebraStripeFn = null) {

            Color result = QueueColorEmpty;

            if (value > redThreshold)
                result = (redThreshold == 0) ? QueueColorEmpty : QueueColorWarning3;
            else if (value > orangeThreshold)
                result = (orangeThreshold == 0) ? QueueColorEmpty : QueueColorWarning2;
            else if (value > yellowThreshold)
                result = (yellowThreshold == 0) ? QueueColorEmpty : QueueColorWarning1;
            else if (value > 0)
                result = QueueColorOk;

            if (zebraStripeFn != null) {
                return zebraStripeFn(row_number, result);
            }

            return result;
        }

        private Color ZebraStripeCallback(int row_number, Color color) {
            if (row_number % 2 == 0) {
                return Helper.darkenColor(color);
            }

            return color;
        }

        /// <summary>
        ///   Set cell colors based on thresholds on a single toolbar row (A single queue)
        /// </summary>
        /// <param name="longestTalkSeconds"></param>
        /// <param name="queueName"></param>
        /// <param name="isManager"></param>
        /// <param name="longestHoldTime"></param>
        /// <param name="liveQueueData"></param>
        private void SetBarColor(int longestTalkSeconds, string queueName, bool isManager, int longestHoldTime, Dictionary<string, List<OrderedDictionary>> liveQueueData) {
            int main_grid_row_index = (int) m_subscribedQueues[queueName]["MainGridRowPosition"]; // The index into mainGrid for our particular queue row

            Color bg_color_ok    = QueueColorOk;
            Color bg_color_empty = QueueColorEmpty;

            if (main_grid_row_index % 2 == 0) {
                bg_color_empty = Helper.darkenColor(bg_color_empty);
                bg_color_ok    = Helper.darkenColor(bg_color_ok);
            }

            Color bg_color_agents_logged_in       = bg_color_empty;
            Color bg_color_agents_idle            = bg_color_empty;
            Color bg_color_talk_new               = bg_color_empty;
            Color bg_color_hold_new               = bg_color_empty;
            Color bg_color_callers_new            = bg_color_empty;
            Color bg_color_longest_waiting_caller = bg_color_empty;

            Dictionary<string, QueryResultSet> live_queue_data_r = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(liveQueueData);
            QueryResultSetRecord live_queue_data = live_queue_data_r[queueName][0];

            if (!m_subscribedQueues[queueName].ContainsKey("Threshold_warning1-Agent")) { 
                // not yet configured
                return;
            }

            int longest_waiting_seconds = live_queue_data.ToInt("longest_waiting_seconds");
            int longest_talk_seconds    = live_queue_data.ToInt("longest_talk_seconds");
            int callers_waiting         = live_queue_data.ToInt("callers_waiting");
            int agents_logged_in        = live_queue_data.ToInt("agents_logged_in");
            int agents_idle             = live_queue_data.ToInt("agents_idle");

            // These thresholds are used as the difference between the number of callers and agents
            //   If yellow threshold is 2, then we will go yellow if there are 2 less agents than then number of callers
            //     ie:  # Callers: 4,  # Agents, 2... Yellow threshold is now hit
            //
            int num_agents_idle_threshold_yellow = 1; // 1-2 additional agents needed to answer callers
            int num_agents_idle_threshold_orange = 3; // 3-4 or more...
            int num_agents_idle_threshold_red    = 5; // 5 or more

            int call_waiting_seconds_threshold_yellow = (int) m_subscribedQueues[queueName]["Threshold_warning1-Agent"];
            int call_waiting_seconds_threshold_orange = (int) m_subscribedQueues[queueName]["Threshold_warning2-Agent"];
            int call_waiting_seconds_threshold_red    = (int) m_subscribedQueues[queueName]["Threshold_warning3-Agent"];

            int num_call_waiting_seconds_threshold_yellow = 2;
            int num_call_waiting_seconds_threshold_orange = 4;
            int num_call_waiting_seconds_threshold_red    = 6;

            int agent_talking_seconds_threshold_yellow = (int) m_subscribedQueues[queueName]["Threshold_warning1-Manager"];
            int agent_talking_seconds_threshold_orange = (int) m_subscribedQueues[queueName]["Threshold_warning2-Manager"];
            int agent_talking_seconds_threshold_red    = (int) m_subscribedQueues[queueName]["Threshold_warning3-Manager"];

            int longest_holding_seconds_threshold_yellow = (int) m_subscribedQueues[queueName]["Hold_time_threshold_warning1-Manager"];
            int longest_holding_seconds_threshold_orange = (int) m_subscribedQueues[queueName]["Hold_time_threshold_warning2-Manager"];
            int longest_holding_seconds_threshold_red    = (int) m_subscribedQueues[queueName]["Hold_time_threshold_warning3-Manager"];

            /*
             bg_color_agents_logged_in = GetColorFromValueAndThresholds(
                agents_logged_in,
                (int) m_subscribedQueues[queueName]["Threshold_warning1-Agent"],
                (int) m_subscribedQueues[queueName]["Threshold_warning2-Agent"],
                (int) m_subscribedQueues[queueName]["Threshold_warning3-Agent"]);
            */

            /*            
             *  Normal Layout
             *  -------------
             *  Staffed
             *  Available
             *  Longest Talk
             *  Longest Hold
             *  Callers Waiting
             *  Longest Waiting
             *   
            */

            // Time Longest Waiting
            bg_color_longest_waiting_caller = GetColorFromValueAndThresholds(
                longest_waiting_seconds,
                call_waiting_seconds_threshold_yellow,
                call_waiting_seconds_threshold_orange,
                call_waiting_seconds_threshold_red,
                main_grid_row_index, ZebraStripeCallback
            );

            // Special case for: Time Longest Waiting: Green color
            if ((longest_waiting_seconds > 0) && (longest_waiting_seconds < call_waiting_seconds_threshold_yellow)) {
                bg_color_longest_waiting_caller = bg_color_ok;
            }

            // Special case for: Agents Staffed (agents_logged_in)
            if (agents_logged_in > 0) {
                bg_color_agents_logged_in = bg_color_ok;
            }

            // Special case for: Available (agents_idle)
            if (callers_waiting == 0) {
                bg_color_agents_idle = bg_color_empty;
            }
            else if (agents_idle >= callers_waiting) {
                // We have enough agents to handle waiting callers
                bg_color_agents_idle = bg_color_ok;
            }
            else {
                // We have fewer agents available than the number of waiting callers
                if (agents_idle == 0) {
                  bg_color_agents_idle = QueueColorWarning3;
                }
                else {
                    // X number of agents available, but less than the # of waiting callers
                    // We have MORE callers waiting, than idle agents
                    //
                    int agent_caller_diff = callers_waiting - agents_idle;

                    bg_color_agents_idle = GetColorFromValueAndThresholds(
                        agent_caller_diff + 1,
                        num_agents_idle_threshold_yellow,
                        num_agents_idle_threshold_orange,
                        num_agents_idle_threshold_red,
                        main_grid_row_index, ZebraStripeCallback
                    );
                }
            }

            // # Callers Waiting
            bg_color_callers_new = GetColorFromValueAndThresholds(
                callers_waiting,
                num_call_waiting_seconds_threshold_yellow,
                num_call_waiting_seconds_threshold_orange,
                num_call_waiting_seconds_threshold_red,
                main_grid_row_index, ZebraStripeCallback
            );


            if (m_enableTeamView) {
                // Longest Talk
                bg_color_talk_new = GetColorFromValueAndThresholds(
                    longest_talk_seconds,
                    agent_talking_seconds_threshold_yellow,
                    agent_talking_seconds_threshold_orange,
                    agent_talking_seconds_threshold_red,
                    main_grid_row_index, ZebraStripeCallback
                );

                // Longest Hold
                bg_color_hold_new = GetColorFromValueAndThresholds(
                    longestHoldTime,
                    longest_holding_seconds_threshold_yellow,
                    longest_holding_seconds_threshold_orange,
                    longest_holding_seconds_threshold_red,
                    main_grid_row_index, ZebraStripeCallback
                );
            }

            // Done Choosing Colors
            // Set Colors

            DataGridViewRow grid_row = mainGrid.Rows[main_grid_row_index];
            
            Helper.SetDataGridViewCell_BackColor(grid_row, "agents_logged_in",             bg_color_agents_logged_in);
            Helper.SetDataGridViewCell_BackColor(grid_row, "agents_idle",                  bg_color_agents_idle);
            Helper.SetDataGridViewCell_BackColor(grid_row, "longest_waiting_time",         bg_color_longest_waiting_caller);
            Helper.SetDataGridViewCell_BackColor(grid_row, "max_agent_talk_duration_time", bg_color_talk_new);
            Helper.SetDataGridViewCell_BackColor(grid_row, "longest_current_hold_time",    bg_color_hold_new);
            Helper.SetDataGridViewCell_BackColor(grid_row, "callers_waiting",              bg_color_callers_new);
            Helper.SetDataGridViewCell_BackColor(grid_row, "queue",                        bg_color_empty);           // Zebra Stripe the entire row.. queue name included

            // For each Cell Column in the MainGrid for just this specific queue

            foreach (DataGridViewCell c in mainGrid.Rows[main_grid_row_index].Cells) {
                // c = Current Cell in the column for queueName

                if (!c.OwningColumn.Name.StartsWith("button-")) {
                    continue;
                }

                ((Hashtable) c.Tag)["Color"] = bg_color_empty;

                if (!mainGrid.GetCellDisplayRectangle(c.ColumnIndex, c.RowIndex, true).Contains(mainGrid.PointToClient(Cursor.Position))) {
                    c.Style.BackColor = bg_color_empty;
                }

                if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left &&
                    m_mouseDownCellColumnIndex != -1 &&
                    m_mouseDownCellRowIndex != -1) {

                    if (m_mouseDownCellRowIndex == c.RowIndex && m_mouseDownCellColumnIndex == c.ColumnIndex) {
                        c.Style.BackColor = Helper.darkenColor(bg_color_empty);
                    }
                    else {
                        c.Style.BackColor = Helper.lightenColor(bg_color_empty);
                    }
                }
            }
        }


        private void hideAnyVisibleGridsExcept(DataGridView grid) {
            foreach (Control c in m_dynamicControls) {
                if (grid == null || (c.Tag is bool && !((bool)c.Tag == (bool)grid.Tag && c.Name == grid.Name))) {
                    if (m_subscribedQueues.ContainsKey(c.Name)) {
                        ((DataGridView)m_subscribedQueues[c.Name][(bool)c.Tag ? "TeamGrid" : "AgentGrid"]).Visible = false;
                    }
                }
            }
        }

        private int adjustPictureBoxButtonImage(PictureBox p, int buttonEvent) {
            string newImageName;
            string name;

            if (p.Tag != null)
                name = p.Tag.ToString();
            else {
                name = p.Name;
                if (name.EndsWith("Pb"))
                    name = name.Substring(0, name.Length - 2);
            }
            newImageName = name;

            int index = -1;

            try {
                if (!Int32.TryParse(p.Name, out index))
                    index = -1;
            }
            catch (FormatException ex) {
                Debug.Print(ex.Message);
            }

            switch (buttonEvent) {
                case (int)PictureBoxButtonEvents.UpInside:
                    goto case (int)PictureBoxButtonEvents.Enter;
                case (int)PictureBoxButtonEvents.Enter:
                    newImageName += "_hover";
                    break;
                case (int)PictureBoxButtonEvents.Down:
                    newImageName += "_pressed";
                    break;
                case (int)PictureBoxButtonEvents.UpOutside:
                    break;
                case (int)PictureBoxButtonEvents.Leave:
                    break;
                default:
                    break;
            }

            p.BackgroundImage = (Image)getResourceByName(newImageName);

            p.Tag = name;

            return index;
        }

        private object getResourceByName(string name) {
            ResourceManager rm = new ResourceManager("QueueLib.queueResources", Assembly.GetExecutingAssembly());
            object result = rm.GetObject(name);
            rm.ReleaseAllResources();
            return result;
        }
        
        private void deSelectVisibleControls() {
            // need to do this to allow colow change of cell on mouse hover
            mainGrid.ClearSelection();

            // prevent selection cursor on any visible controls
            hiddenGrid.Focus();
            hiddenGrid.Select();
        }

        public void SetStatusBarMessage (Color foreColor, string message) {
            if (Thread.CurrentThread != m_guiThread)
            {
                this.Invoke((MethodInvoker) delegate() { _SetStatusBarMessage(foreColor, message); } );
                return;
            }

            _SetStatusBarMessage(foreColor, message);
        }

        private void _SetStatusBarMessage(Color foreColor, string message)
        {
            this.m_cmpToolbarStatusMessage.ForeColor = foreColor;

            this.m_cmpToolbarStatusMessage.Text = message;
            m_ToolbarStatusMessageLastUpdate = DateTime.Now;
        }

        private void UpdateQueueDisplay() {
            if (m_liveGuiDataNeedsUpdate) {
                toggleStatusLight();

                lock (m_liveDatas) {
                    Dictionary<string, List<OrderedDictionary>> agent  = (Dictionary<string, List<OrderedDictionary>>) m_liveDatas["agent"];
                    Dictionary<string, List<OrderedDictionary>> caller = (Dictionary<string, List<OrderedDictionary>>) m_liveDatas["caller"];
                    Dictionary<string, List<OrderedDictionary>> queue  = (Dictionary<string, List<OrderedDictionary>>) m_liveDatas["queue"];
                    Dictionary<string, List<OrderedDictionary>> status = (Dictionary<string, List<OrderedDictionary>>) m_liveDatas["status"];
                    Dictionary<string, List<OrderedDictionary>> call   = (Dictionary<string, List<OrderedDictionary>>) m_liveDatas["call"];

                    if (Debugger.IsAttached) { 
                        Dictionary<string, QueryResultSet> r_agent  = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(agent);
                        Dictionary<string, QueryResultSet> r_caller = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(caller);
                        Dictionary<string, QueryResultSet> r_queue  = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(queue);
                        Dictionary<string, QueryResultSet> r_status = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(status);
                        Dictionary<string, QueryResultSet> r_call   = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(call);
                    }

                    bool need_to_resize_frame = UpdateUiDataComponents();
                    // log.Info("updates done!");

                    if (need_to_resize_frame)
                        LayoutForm();

                    if (!m_updateDisplayTimer.Enabled) {
                        m_updateDisplayTimer.Start();
                    }
                }

                m_liveGuiDataNeedsUpdate = false;
            }
        }

        private void modCellColor(DataGridView grid, int columnIndex, int rowIndex, ButtonCellColorState colorState) {
            if (rowIndex < 0 || columnIndex < 0)
                return;

            DataGridViewCell c = grid[columnIndex, rowIndex];
            if (!c.OwningColumn.Name.StartsWith("button-") || !(c.Tag is Hashtable))
                return;

            Hashtable cellInfo = (Hashtable)c.Tag;
            if (!cellInfo.ContainsKey("Color") || cellInfo["Color"] == null)
                return;

            Color baseColor = (Color)((Hashtable)c.Tag)["Color"];

            switch (colorState) {
                case ButtonCellColorState.Light:
                    c.Style.BackColor = Helper.lightenColor(baseColor);
                    break;
                case ButtonCellColorState.Dark:
                    c.Style.BackColor = Helper.darkenColor(baseColor);
                    break;
                default:
                case ButtonCellColorState.Base:
                    c.Style.BackColor = baseColor;
                    break;
            }
        }

        private void intellaQueue_Resize() {
            // This code prevents drop grid from being in an incorrectly sized state after an un-minimize.
            // @todo Maintain the currently selected row.
            if (m_isMinimized && WindowState != FormWindowState.Minimized) {
                m_isMinimized = false;

                DataGridView visibleDropGrid = DroppedDownGrid;
                if (visibleDropGrid != null) {
                    // removes all rows
                    trimAndAdjustGrid(0, visibleDropGrid);
                    // repopulates dropgrid with latest data
                    UpdateUiDataComponents();
                }

                // make sure window decorations are layed out correctly
                LayoutForm();
            }
        }

        private void intellaQueue_Resize(object sender, EventArgs e) {
            intellaQueue_Resize();
        }

        public void resize(Size newSize, bool doAlwaysResize = false) {
            if (this.WindowState != FormWindowState.Minimized && (doAlwaysResize || newSize.Width != Size.Width || newSize.Height != Size.Height)) {
                Debug.Print("RESIZE " + Size.ToString() + "->" + newSize.ToString());

                this.Size = newSize;
                LayoutForm();
                this.Refresh();
            }
        }
    }
 }
