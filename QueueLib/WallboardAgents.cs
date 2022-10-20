using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Lib;

namespace QueueLib {
    public class WallboardAgents {
        private LiveDataGridView m_AgentsDataGridView;
        public DbHelper m_db;

        private string m_wallboard_name;
        private string m_queueName = "inbound_all";
        private QueryResultSet m_agent_fields = new QueryResultSet();
        private Hashtable m_agent_field_names = new Hashtable();

        private QueryResultSetRecord m_dataThresholds;
        private Hashtable m_statusCodes; // Hashtable of string -> QueryResultSetRecord
        private Panel m_parentPanel; // What panel are we embedded in
        private int m_adjustedGridSizeTo = 0; // Number of rows that are current grid is adjusted to fit

        private static Color QueueColorRegular = Color.FromArgb(173, 173, 173);
        private static Color QueueColorGreen   = Color.FromArgb(141, 193, 102);
        private static Color QueueColorYellow  = Color.FromArgb(243, 219, 119);
        private static Color QueueColorOrange  = Color.FromArgb(233, 138, 75);
        private static Color QueueColorRed     = Color.FromArgb(211, 101, 101);
        
        public WallboardAgents(Panel parentPanel, string wallboardName) {
            this.m_parentPanel    = parentPanel;
            this.m_wallboard_name = wallboardName;
        }

        public void BuildAgentsWallboard() {
            m_dataThresholds = m_db.DbSelectSingleRow(@" 
                SELECT
                    call_waiting_seconds_threshold_yellow,  -- integer NOT NULL DEFAULT 60,
                    call_waiting_seconds_threshold_orange,  -- integer NOT NULL DEFAULT 120,
                    call_waiting_seconds_threshold_red,     -- integer NOT NULL DEFAULT 240,
                    agent_talking_seconds_threshold_yellow, -- integer NOT NULL DEFAULT 120,
                    agent_talking_seconds_threshold_orange, -- integer NOT NULL DEFAULT 240,
                    agent_talking_seconds_threshold_red,    -- integer NOT NULL DEFAULT 480,
                    hold_time_seconds_threshold_yellow,     -- integer NOT NULL DEFAULT 120,
                    hold_time_seconds_threshold_orange,     -- integer NOT NULL DEFAULT 240,
                    hold_time_seconds_threshold_red         -- integer NOT NULL DEFAULT 480,
                FROM
                    queue.v_toolbar_config_perqueue WHERE queue_name = '{0}'", m_queueName);

            m_statusCodes = m_db.DbSelectIntoHash("status_code_name", 
                  @"SELECT
                     status_code_name,
                     status_code_longname,
                     user_defined,
                     manager_only
                   FROM
                     queue.v_agent_status_codes");

            m_AgentsDataGridView = new LiveDataGridView {Width = m_parentPanel.Width, Height = m_parentPanel.Height};
            m_AgentsDataGridView.SetCellColorCallback(this.CellColorCallback);
            m_AgentsDataGridView.SelectionChanged += (sender, args) => { LiveDataGridView g = (LiveDataGridView) sender; g.ClearSelection(); };
            m_parentPanel.Controls.Add(m_AgentsDataGridView);

            m_agent_field_names = new Hashtable();

            m_agent_fields = m_db.DbSelect("SELECT * FROM queue.toolbar_config_display_agents WHERE toolbar_config_display_group = ? ORDER BY col_pos", this.m_wallboard_name);
            if (m_agent_fields.QueryFailed()) {
                // TODO: Logging
                return;
            }

            foreach (QueryResultSetRecord r in this.m_agent_fields) {
                m_AgentsDataGridView.AddGridColumn(r["col_data_item"], r["col_data_item_raw"], r["col_label"]);
                // m_agent_field_names.Add(r["col_data_item"], "1");
            }

            this.Update();
            m_AgentsDataGridView.SetOptimalGridFontSizes();
        }

        public void Update() {
            QueryResultSet agent_data = m_db.DbSelect("SELECT * FROM live_queue.v_toolbar_agents WHERE queue_name = ? order by agent_fullname", m_queueName);

            if (agent_data.Count == 0) {
                m_AgentsDataGridView.Rows.Clear();
                return;
            }

            QueryResultSet q = new QueryResultSet();;

            // Massage the data a little bit
            foreach (QueryResultSetRecord row in agent_data) {
                if ((row["agent_queue_status_code"] == "AVAILABLE") && (row["call_type"] != "")) {
                  row["agent_queue_status_code"] = "ONCALL";
                  row["agent_status"]            = "Call In Progress";
                }

                if ((row["caller_callerid_name"].ToLower() == "unknown") || 
                    (row["caller_callerid_name"].ToLower() == "private") ||
                    (row["caller_callerid_name"].ToLower() == "restricted") 
                    ) {
                        row["caller_callerid_name"] = "-";
                }

                if ((row["agent_status"] == "Call In Progress") && (row["caller_callerid_num"] == "")) {
                    row["caller_callerid_name"] = "-";
                }

                q.Add(row); // Don't need once we're done figuring the dynamic row sizing

                // Testing for dynamic row sizing
                /*
                q.Add(row);
                q.Add(row);
                q.Add(row);
                q.Add(row);
                q.Add(row);
                q.Add(row);
                */
            }

            m_AgentsDataGridView.PopulateDataRows(q); // Don't need once we're done figuring the dynamic row sizing
            //m_AgentsDataGridView.PopulateDataRows(agent_data);

            return;
            

            // Little hacky... try and fit the extra on the screen
            // We're blatantly assuming that we're including agent_callerid_num in the wallboard display
            //
            if ((m_AgentsDataGridView.Rows.Count > m_adjustedGridSizeTo) && (m_AgentsDataGridView.Rows.GetRowCount(DataGridViewElementStates.Displayed) < m_AgentsDataGridView.Rows.Count)) {
                Font font = m_AgentsDataGridView.RowsDefaultCellStyle.Font;
                if (font == null) {
                    font = m_AgentsDataGridView.Font;
                }

                string longest_extension = Utils.GetLongestTextFromQueryResultSet(agent_data, "agent_callerid_num");
                int exten_col_index      = m_AgentsDataGridView.GetColumnIndexByName("agent_callerid_num");

                int perfect_cell_height = m_parentPanel.Height / m_AgentsDataGridView.Rows.Count - 1; // -1 to not include the column headers
                int perfect_cell_width  = m_AgentsDataGridView.Columns[exten_col_index].Width;

                List<int> sizes = Utils.FindLargestFontForBox(m_parentPanel.FindForm(), 50, perfect_cell_width, perfect_cell_height, longest_extension, font);
                int font_size = sizes[2];

                foreach (DataGridViewRow row in m_AgentsDataGridView.Rows) {
                    row.InheritedStyle.Font = new Font(font.FontFamily, font_size, font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont);
                    row.Height = perfect_cell_height;
                }

                m_adjustedGridSizeTo = m_AgentsDataGridView.Rows.Count;
            }
        }
        
        /// <summary>
        ///   Called for each cell populated
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="columnText"></param>
        /// <param name="columnTextRaw"></param>
        /// <param name="columnTag"></param>
        /// <param name="rowData"></param>
        /// <returns></returns>
        
        public Color? CellColorCallback(DataGridViewCell cell, string columnName, string columnText, string columnTextRaw, Hashtable columnTag, QueryResultSetRecord rowData) {
            int ?cell_value = 0;
            int cell_value_parse;
            string threshold_prefix = "";

            int? threshold_green  = 1;
            int? threshold_yellow = null;
            int? threshold_orange = null;
            int? threshold_red    = null;

            switch (columnName) {
                case "talk_duration_time":
                    threshold_prefix = "agent_talking_seconds_threshold_";
                    if (int.TryParse(columnTextRaw, out cell_value_parse)) {
                        cell_value = (int) cell_value_parse;
                    }
                    if (columnText == "*Ringing*") {
                        cell_value = 1;
                    }
                    
                    break;
                case "current_hold_duration_time":
                    threshold_prefix = "hold_time_seconds_threshold_";
                    if (int.TryParse(columnTextRaw, out cell_value_parse)) {
                        cell_value = (int) cell_value_parse;
                    }
                    break;
                case "waiting_time":
                    threshold_prefix = "call_waiting_seconds_threshold_";
                    if (int.TryParse(columnTextRaw, out cell_value_parse)) {
                        cell_value = (int) cell_value_parse;
                    }
                    break;
                case "agent_status":
                    threshold_yellow = 5; // Unused
                    threshold_orange = 10;
                    threshold_red    = 20;

                    string current_status_code = columnTextRaw;
                    QueryResultSetRecord status_code_data = (QueryResultSetRecord) m_statusCodes[current_status_code];

                    if (current_status_code.StartsWith("AVAILABLE")) {
                       cell_value = threshold_green;
                    }
                    else if (current_status_code == "ONCALL") {
                        cell_value = threshold_yellow;
                    }
                    else if ((status_code_data != null) && (status_code_data["manager_only"] == "True")) {
                        cell_value = threshold_red;
                    }
                    else {
                        // Any non-Available non-manager status code
                        cell_value = threshold_orange;
                    }

                    break;
                default:
                    return null;
            }

            if (threshold_prefix != "") {
                threshold_yellow = m_dataThresholds.ItemInt(threshold_prefix + "yellow");
                threshold_orange = m_dataThresholds.ItemInt(threshold_prefix + "orange");
                threshold_red    = m_dataThresholds.ItemInt(threshold_prefix + "red");
            }

            if ((threshold_red != null) && (threshold_red > 0) && (cell_value >= threshold_red)) {
                return QueueColorRed;
            }
    
            if ((threshold_orange != null) && (threshold_orange > 0) && (cell_value >= threshold_orange)) {
                return QueueColorOrange;
            }

            if ((threshold_yellow != null) && (threshold_yellow > 0) && (cell_value >= threshold_yellow)) {
                return QueueColorYellow;
            }

            if ((threshold_green != null) && (threshold_green > 0) && (cell_value >= threshold_green)) {
                return QueueColorGreen;
            }

            return QueueColorRegular;
        }
    }
}
