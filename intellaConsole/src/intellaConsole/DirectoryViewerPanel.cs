using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QueueLib;
using Lib;

namespace intellaConsole {
    public partial class DirectoryViewerPanel : Form {
        ///////////////////////////////////////////////////////////////////////

        private static int e_DISPLAY_FIRST_NAME_FIRST = 0;
        private static int e_DISPLAY_LAST_NAME_FIRST = 1;

        private QueryResultSet m_directory = new QueryResultSet();
        private DbHelper m_db;
        private intellaConsole m_ic;

        // Upon right clicking a directory row, save the index
        private int m_selectedDirectoryRow = 0;
        private Hashtable m_presenceInfo = new Hashtable(); // Most recent poll of device-state presence

        private int m_display_name_type = e_DISPLAY_FIRST_NAME_FIRST;

        private Image m_icon_email;

        ///////////////////////////////////////////////////////////////////////

        public DirectoryViewerPanel() {
            InitializeComponent();

            ///////////////////////
            // Dynamic Widget Stuff

            // Replace TextBoxQuickSearch with a WaterMarked TextBox
            TextBoxWaterMarked tbwm = new TextBoxWaterMarked();
            tbwm.WaterMarkText = "Enter Search";
            tbwm.Location = this.textBoxQuickSearch.Location;
            tbwm.Text = this.textBoxQuickSearch.Text;
            tbwm.Size = this.textBoxQuickSearch.Size;
            tbwm.TextChanged += this.textBoxQuickSearch_TextChanged;
            this.textBoxQuickSearch.Dispose();
            this.textBoxQuickSearch = tbwm;
            this.cmpDirectoryPanel.Controls.Add(tbwm);

            // End Dynamic Widget Stuff
            ///////////////////////////
        }

        public void setIntellaConsoleForm(intellaConsole ic) {
            this.m_ic = ic;

            // Set resources from main Form
            this.m_icon_email = this.m_ic.getResourceImage("icon_email.png");
        }

        public Boolean areWeVisible() {
            return this.cmpDirectoryPanel.Visible;
        }

        public void setDbHelper(DbHelper db) {
            this.m_db = db;
        }
        
        public Panel getBodyPanel() {
            return this.cmpDirectoryPanel;
        }

        /// <summary>
        /// Get the raw data related to the specified row in the directory grid
        /// </summary>
        /// <param name="rowIndex">Index of the row to get data on (0 based)</param>
        public QueryResultSetRecord getDataForRow(int rowIndex) {
            return m_directory[rowIndex];
        }

        public void setQuickSearchValue(string value) {
            this.textBoxQuickSearch.Text = value;
        }

        // Get the sequential cell number based on the row and column index clicked
        private int getRowNumber(int rowIdx, int colIdx) {
            if ((rowIdx < 0) || (colIdx < 0)) return -1;  // Ignore header clicks
            return rowIdx;
        }

        public void directoryEntryUpdateCallbackFn(QueryResultSetRecord directoryEntry, string sqlCmd )
        {
            string query;

            if ( sqlCmd == "update/insert" ) {
                if ( directoryEntry.Exists("directory_entry_id") ) {
                    m_db.DbSelect(@"
                       UPDATE
                         operator_console.directory_entry
                       SET
                          first_name    = {0},
                          last_name     = {1},
                          phone_number  = {2},
                          email         = {3},
                          -- area       = '   ',
                          department    = {4}
                        WHERE
                          directory_entry_id = {5}
                       ",

                       directoryEntry["first_name"],   directoryEntry["last_name"],
                       directoryEntry["phone_number"], directoryEntry["email"],
                       directoryEntry["department"],   directoryEntry["directory_entry_id"]
                    );
                }
                else {
                    // TOOD: Make an editable view where you can set the username and it figures the user_id
                    // TODO: Perhaps make a sessions table and then this program creates a temporary table and
                    // TODO: then only this DB session has access to that single row
                    m_db.DbSelect(
                      @"WITH get_user_id AS (
                          SELECT user_id FROM operator_console.user WHERE username = {0}
                        )
                        INSERT INTO operator_console.directory_entry
                                    (user_id,    first_name, last_name, phone_number, email, department)
                            SELECT  user_id,     {1},        {2},       {3},          {4},   {5}
                            FROM    get_user_id",
                        m_ic.getOperatorExten(),

                        directoryEntry["first_name"],
                        directoryEntry["last_name"],  directoryEntry["phone_number"],
                        directoryEntry["email"],      directoryEntry["department"]
                    );
                }
            }
            else if ( sqlCmd == "delete" ) {
                m_db.DbSelect(@"
                    DELETE FROM operator_console.directory_entry
                    WHERE directory_entry_id = {0}",
                    directoryEntry["directory_entry_id"]
                );
            }
            else {
                MessageBox.Show("### INVALID sqlCmd: " + sqlCmd);
                return;
            }

            this.directoryPopulate(this.textBoxQuickSearch.Text);
        }

        public void directoryPopulate(string searchFilter) {
            string useSearchFilter = (searchFilter.Length == 0) ? "FALSE" : "TRUE";
            string query, whereClause, order_clause, directory_item_column;

            if (this.radioButtonSearchDirectoryByFirst.Checked) {
                whereClause = String.Format(@"first_name ILIKE '{0}%'", searchFilter);
                order_clause = "ORDER BY lower(first_name), lower(last_name)";
                directory_item_column = "(first_name || ' ' || COALESCE(last_name, '')) as directory_entry_name";
            }
            else if (this.radioButtonSearchDirectoryByLast.Checked) {
                whereClause = String.Format(@"last_name ILIKE '{0}%'", searchFilter);
                order_clause = "ORDER BY lower(last_name), lower(first_name)";
                directory_item_column = "(last_name || ', ' || COALESCE(first_name, '')) as directory_entry_name";
            }
            else {
                whereClause = String.Format(
                    @"(first_name ILIKE '{0}%') OR (last_name ILIKE '{1}%') OR (phone_number ILIKE '{2}%')",
                    searchFilter, searchFilter, searchFilter
                );

                order_clause = "ORDER BY lower(first_name), lower(last_name)";
                directory_item_column = "(first_name || ' ' || COALESCE(last_name, '')) as directory_entry_name";
            }
//b
            query = String.Format(
                  // TODO: Handle selecting by the tenant_id
                  @"SELECT   directory_entry_id,
                             first_name,
                             last_name, 
                             {0}, 
                             phone_number,
                             voicemail,
                             device,
                             device_status,
                             email,
                             area,
                             department 
                    FROM     operator_console.v_directory_entry 
                    WHERE    ({1} IS FALSE OR ({2}))
                    {3}", directory_item_column, useSearchFilter, whereClause, order_clause);


            m_directory = m_db.DbSelect(query);

            this.dataGridViewDirectory.Rows.Clear();

            Image icon_green_dot  = Properties.Resources.greenDot;
            Image icon_red_dot    = Properties.Resources.redDot;
            Image icon_email      = this.m_ic.getResourceImage("icon_email.png");
            Image icon_blank      = this.m_ic.getResourceImage("icon_blank_transparent.png");
            Image icon_dir        = Properties.Resources.icon_table;
            Image icon_current_status_voicemail;
            Image icon_current_status_exten;
            Image icon_current_status_message = icon_blank;

            foreach (QueryResultSetRecord r in this.m_directory) {
                icon_current_status_voicemail = ((string) r["voicemail"] == "True") ? icon_email : icon_blank;

                string state = (string)r["device_status"];

                switch (state) {
                    case "Idle":
                        icon_current_status_exten = icon_green_dot;
                        icon_current_status_message = icon_email;
                        break;
                    case "InUse":
                        icon_current_status_exten = icon_red_dot;
                        icon_current_status_message = icon_email;
                        break;
                    case "Unavailable":
                        // Cannot send messages to phones that are offline!
                        icon_current_status_message = icon_blank;
                        goto state_default;
                    default:
                      state_default:
                        if (((string)r["directory_entry_id"] != null)) {
                            // Specific icon for a directory entry
                            icon_current_status_exten = icon_dir;
                        }
                        else {
                            icon_current_status_exten = icon_blank;
                        }

                        break;
                }

                this.dataGridViewDirectory.Rows.Add(
                    r["directory_entry_name"],
                    icon_current_status_exten,
                    icon_current_status_message,
                    icon_current_status_voicemail,
                    r["phone_number"],
                    r["department"]
                );
            }

            ToolTip a = new ToolTip();
            a.AutoPopDelay = 5000;
            a.InitialDelay = 1000;
            a.ReshowDelay = 500;
            a.ShowAlways = true;

            a.SetToolTip(dataGridViewDirectory, "Foo");

            // Reselect the previously selected cell (the selection is lost due to the above clear and repopulate
            // The selection is saved in dataGridViewDirectory_CellMouseDown()
            int numRows = this.dataGridViewDirectory.Rows.Count;

            if ( this.m_selectedDirectoryRow >= numRows ) {
                this.m_selectedDirectoryRow = numRows - 1;
            }
            
            this.dataGridViewDirectory.Rows[this.m_selectedDirectoryRow].Selected = true;
            this.dataGridViewDirectory.FirstDisplayedScrollingRowIndex = this.m_selectedDirectoryRow;

            int lastRow = numRows - 1;
            int column_index_name   = this.dataGridViewDirectory.Columns.IndexOf(this.cmpDataGridViewDirectoryColumnContactName);
            int column_index_status = this.dataGridViewDirectory.Columns.IndexOf(this.cmpDataGridViewDirectoryColumnStatus);
            int column_index_vmail  = this.dataGridViewDirectory.Columns.IndexOf(this.cmpDataGridViewDirectoryColumnVMail);
            this.dataGridViewDirectory.Rows[lastRow].Cells[column_index_name].Style.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.dataGridViewDirectory.Rows[lastRow].Cells[column_index_name].Value           = "...Right click to add a new directory entry...";

            // Get rid of the red X's on the last row (the blank insert row for the user to right click on)
            this.dataGridViewDirectory.Rows[lastRow].Cells[column_index_status].Value = null;
            this.dataGridViewDirectory.Rows[lastRow].Cells[column_index_vmail].Value  = null;
        }

        private void dataGridViewDirectory_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex < 0) return;
            if (e.RowIndex >= this.m_directory.Count) return;
            if (e.ColumnIndex < 0) return;
            if (!m_ic.checkPbxOperationDelay()) return;

            int index = e.RowIndex;

            if (e.ColumnIndex == cmpDataGridViewDirectoryColumnSendMessage.Index) {
                QueryResultSetRecord row_directory_entry = this.m_directory[index];
                string extension_device = row_directory_entry["device"];

                if (extension_device == "") {
                    // Only local devices can be messaged, not 'Speed Dial' Directory Entries (Which only have a phone number to be called)
                    m_ic.displayMessageBarMessage(Color.Yellow, "Only local extensions can sent messages");
                    return;
                }

                if (!this.m_presenceInfo.Contains(extension_device)) {
                    m_ic.displayMessageBarMessage(Color.Yellow, "Only phones which are connected can be sent messages");
                    // Only devices with presence
                    return;
                }

                OrderedDictionary device_presence = (OrderedDictionary) m_presenceInfo[extension_device];
                if (device_presence["state"].ToString() == "Unavailable") {
                    m_ic.displayMessageBarMessage(Color.Yellow, "Only phones which are connected can be sent messages");
                    return;
                }

                string contact_name = row_directory_entry["first_name"];
                if (contact_name != "") { contact_name += " "; }
                contact_name += row_directory_entry["last_name"];

                this.m_ic.StartMessageConversation(extension_device, row_directory_entry["phone_number"], contact_name);
                return;
            }

            // Otherwise any other double click is a dial
            m_ic.doPbxDial((string) this.m_directory[index]["phone_number"], (string) this.m_directory[index]["directory_entry_name"]);
        }

        // Right click directory entry editor 
        private void dataGridViewDirectory_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right) return;

            QueryResultSetRecord directoryEntry;            
            int row_number = getRowNumber(e.RowIndex, e.ColumnIndex);
            if (row_number < 0) return;

            // Save the cell indicies in order to reselect the cell after a repopulate
            this.m_selectedDirectoryRow = row_number;

            // Select the clicked cell
            this.dataGridViewDirectory.Rows[row_number].Selected = true;

            // Nothing else to do for a regular click, only process right clicking below here
            if (e.Button != MouseButtons.Right) return;

            directoryEntry = (row_number < this.m_directory.Count)
                ? this.m_directory[row_number]
                : new QueryResultSetRecord();

            // Either editing an existing item or creating a new item (which won't have an id)
            if ( (directoryEntry.Count == 0) || directoryEntry.Exists("directory_entry_id") && (directoryEntry["directory_entry_id"] != null)) {
                DirectoryEntryEditor dee = new DirectoryEntryEditor(directoryEntry, directoryEntryUpdateCallbackFn);
                dee.ShowDialog(m_ic);
            } else { 
                // An item from extensions is merged in but cannot be edited for now
                MessageBox.Show("Cannot edit company extensions, only directory entries", "Cannot Edit Entry", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void textBoxQuickSearch_TextChanged(object sender, EventArgs e) {
            this.directoryPopulate(this.textBoxQuickSearch.Text);
        }

        public void ShowBodyPanel() {
            this.cmpDirectoryPanel.Show();
        }

        public void HideBodyPanel() {
            this.cmpDirectoryPanel.Hide();
        }

        private void buttonShowAll_Click(object sender, EventArgs e) {
            setQuickSearchValue("");
            directoryPopulate("");            
        }

        public void updatePresence(Hashtable presenseInfo) {
            this.m_presenceInfo = presenseInfo; // So row clicks can see most recent presence data (ie: dataGridViewDirectory_CellDoubleClick)

            Image icon_green_dot  = Properties.Resources.greenDot;
            Image icon_red_dot    = Properties.Resources.redDot;
            Image icon_yellow_dot = Properties.Resources.icon_dot_yellow;
            Image icon_blank      = Properties.Resources.icon_blank_transparent;
            Image icon_current_status_exten;
            Image icon_current_status_message = icon_blank;

            int column_index_status = this.dataGridViewDirectory.Columns.IndexOf(this.cmpDataGridViewDirectoryColumnStatus);

            foreach (DataGridViewRow r in this.dataGridViewDirectory.Rows) {
                if (r.Index >= this.m_directory.Count) break; // We're at the end (the blank row in the grid at the bottom for adding new records)

                string device = (string)this.m_directory[r.Index]["device"];
                if (device == null) continue; // Skip if this isn't a device_state capable entry
                if (!presenseInfo.Contains(device)) continue; // Skip if the current up to date presense info does not have this device

                OrderedDictionary device_presense = (OrderedDictionary) presenseInfo[device];
                string state = (string) device_presense["state"];

                switch (state) {
                    case "Idle":
                        icon_current_status_exten = icon_green_dot;
                        icon_current_status_message = m_icon_email;
                        break;
                    case "InUse":
                        icon_current_status_exten = icon_red_dot;
                        icon_current_status_message = m_icon_email;
                        break;
                    case "Ringing":
                        icon_current_status_exten = icon_yellow_dot;
                        icon_current_status_message = m_icon_email;
                        break;
                    case "Unavailable":
                        // Cannot send messages to phones that are offline!
                        icon_current_status_message = icon_blank;
                        icon_current_status_exten = icon_blank;
                        break;
                    default:
                        icon_current_status_exten = icon_blank;
                        break;
                }

                r.Cells[column_index_status].Value = icon_current_status_exten;
            }
        }

        private void radioButtonSearchDirectoryByFirst_Click(object sender, EventArgs e) {
            this.textBoxQuickSearch_TextChanged(sender, e);
        }

        private void radioButtonSearchDirectoryByLast_Click(object sender, EventArgs e) {
            this.textBoxQuickSearch_TextChanged(sender, e);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridViewDirectory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        public DataGridView GetDataGridView() {
            return this.dataGridViewDirectory;
        }
    }
}
