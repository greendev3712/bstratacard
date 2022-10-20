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
    public partial class SpeedDialPanel : Form {
        ///////////////////////////////////////////////////////////////////////

        private QueryResultSet m_speedDial = new QueryResultSet();

        private DbHelper m_db;
        private intellaConsole m_ic;

        // Upon right clicking a speed dial cell, save the cell index coords
        private int m_selectedSpeedDialRow = 0;
        private int m_selectedSpeedDialCol = 0;

        ///////////////////////////////////////////////////////////////////////

        public void setIntellaConsoleForm(intellaConsole ic) {
            this.m_ic = ic;
        }

        public void setDbHelper(DbHelper db) {
            this.m_db = db;
        }

        public Panel getBodyPanel() {
            return this.panelSpeedDial;
        }

        public SpeedDialPanel() {
            InitializeComponent();
        }

        // Get the sequential cell number based on the row and column index clicked
        private int getCellNumber( int rowIdx, int colIdx ) {
            if ((rowIdx < 0) || (colIdx < 0)) return -1;  // Ignore header clicks

            return (rowIdx * this.dataGridViewSpeedDial.ColumnCount) + colIdx;
        }

        private void dataGridViewSpeedDial_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            int cell_number = getCellNumber(e.RowIndex, e.ColumnIndex);
            if (cell_number < 0) return;

            string phone_number = (string) this.m_speedDial[cell_number]["phone_number"];
            if ((phone_number == null) || (phone_number == ""))  {

                return;
            }

            m_ic.doPbxDial(phone_number, (string)this.m_speedDial[cell_number]["speed_dial_name"]);
        }

        public void speedDialUpdateCallbackFn(QueryResultSetRecord speedDialEntry, string sqlCmd )
        {
            if ( sqlCmd == "update/insert" ) {
                if ( speedDialEntry.Contains("speed_dial_id") ) {
                    m_db.DbSelect(@"
                        UPDATE
                          operator_console.speed_dial
                        SET
                          first_name    = {0},
                          last_name     = {1},
                          phone_number  = {2},
                          email         = {3},
                          ordering      = {4}::integer
                          -- area       = '',
                          -- department = ''
                        WHERE
                          speed_dial_id = {5}::text
                        ",
                        (string) speedDialEntry["first_name"],   (string) speedDialEntry["last_name"],
                        (string) speedDialEntry["phone_number"], (string) speedDialEntry["email"],
                        (string) speedDialEntry["ordering"],     (string) speedDialEntry["speed_dial_id"]
                    );
                }
                else {
                    // TOOD: Make an editable view where you can set the username and it figures the user_id
                    // TODO: Perhaps make a sessions table and then this program creates a temporary table and
                    // TODO: then only this DB session has access to that single row
                    m_db.DbSelect(@"
                        WITH get_user_id AS (
                          SELECT user_id FROM operator_console.user WHERE username = {0}
                        )
                        INSERT INTO operator_console.speed_dial 
                                    (user_id,    first_name, last_name, phone_number, email, ordering)
                            SELECT  user_id,     {1},        {2},       {3},          {4},   {5}::integer
                            FROM    get_user_id
                        ",
                        m_ic.getOperatorExten(),
                        (string)speedDialEntry["first_name"],   (string)speedDialEntry["last_name"],
                        (string)speedDialEntry["phone_number"], (string)speedDialEntry["email"],
                        (string)speedDialEntry["ordering"]
                    );
                }
            }
            else if ( sqlCmd == "delete" ) {
                m_db.DbSelect(@"
                    DELETE FROM operator_console.speed_dial
                    WHERE       speed_dial_id = {0}
                    ",
                    (string) speedDialEntry["speed_dial_id"]
                );
            }
            else {
                MessageBox.Show("### speedDialsUpdateFn invalid sqlCmd: " + sqlCmd);
                return;
            }

            // TODO: Not doing any speed dial searching right now
            this.speedDialPopulate("");
        }

        public void speedDialPopulate(string searchFilter)
        {
            string useSearchFilter = (searchFilter.Length == 0) ? "FALSE" : "TRUE";

            QueryResultSet speed_dials = m_db.DbSelect(@"
                SELECT   speed_dial_id,
                         first_name,
                         last_name,
                         (first_name || ' ' || COALESCE(last_name, '')) as speed_dial_name, 
                         phone_number,
                         email,
                         ordering,
                         area,
                         department 
                FROM     operator_console.v_speed_dial 
                WHERE    username = {0} AND 
                         ({1}::boolean IS FALSE OR ((first_name ILIKE {2} || '%') OR (last_name ILIKE {3} || '%') OR (phone_number ILIKE {4} || '%')))
                ORDER BY ordering", m_ic.getOperatorExten(), useSearchFilter, searchFilter, searchFilter, searchFilter);

            int speed_rows = 7;

            this.m_speedDial.Clear();
            this.dataGridViewSpeedDial.Rows.Clear();
            this.dataGridViewSpeedDial.Rows.Add(speed_rows);

            int max_speed_dials = this.dataGridViewSpeedDial.ColumnCount * this.dataGridViewSpeedDial.RowCount;
            int speed_dial_pos = 0;
            int speed_dial_insert_at = 0;
            int speed_dials_added = 0;
            int cell;
            int row;

            foreach (QueryResultSetRecord speed_dial in speed_dials) {
                if (speed_dial_pos > max_speed_dials) {
                    break;
                }

                speed_dial_insert_at = speed_dial_pos;

                if (speed_dial["ordering"] != "") {
                    speed_dial_insert_at = speed_dial.ToInt("ordering");

                    if (speed_dial_insert_at > max_speed_dials) {
                        // Example: Can't fit this one if the db tells us we're at position 50 and we can only hold 32
                        continue;
                    }
                }

                do {
                    cell = speed_dial_insert_at % this.dataGridViewSpeedDial.ColumnCount;
                    row  = speed_dial_insert_at / this.dataGridViewSpeedDial.ColumnCount;
                }
                while ((this.dataGridViewSpeedDial.Rows[row].Cells[cell].Value != null) && speed_dial_insert_at++ < max_speed_dials);

                this.dataGridViewSpeedDial.Rows[row].Cells[cell].Value = speed_dial["speed_dial_name"] + "\n" + speed_dial["phone_number"];
                this.m_speedDial.Add(speed_dial); // So button clicks can see the data
                speed_dials_added++;

                // Only bump up the position if we're in a normally-ordered position
                // Example: Say we're curently processing a speed dial for position 5
                // But ordering=10 on this entry, which means it wants to be specifically put in button position 10
                // So we've now put the buttin on position 10, but we need to resume where we left off at speed_dial_pos
                // 
                if (speed_dial_insert_at == speed_dial_pos) {
                    speed_dial_pos++;
                }
            }

            // Fill in the rest
            if (speed_dials_added < max_speed_dials) {
                for (int i = speed_dials_added; i < max_speed_dials; i++) {
                    this.m_speedDial.Add(new QueryResultSetRecord());
                }
            }

            // Reselect the previously selected cell (the selection is lost due to the above clear and repopulate
            // The selection is saved in dataGridViewSpeedDial_CellMouseDown()
            this.dataGridViewSpeedDial.Rows[this.m_selectedSpeedDialRow].Cells[this.m_selectedSpeedDialCol].Selected = true;
        }

        // Right click speed dial entry editor 
        public void dataGridViewSpeedDial_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right) return;
            
            QueryResultSetRecord speedDialEntry;

            int cell_number = getCellNumber(e.RowIndex, e.ColumnIndex);
            if ( cell_number < 0 ) return;

            // Save the cell indicies in order to reselect the cell after a repopulate
            this.m_selectedSpeedDialRow = e.RowIndex;
            this.m_selectedSpeedDialCol = e.ColumnIndex;

            // Select the clicked cell
            this.dataGridViewSpeedDial.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;

            // Nothing else to do for a regular click, only process right clicking below here
            if (e.Button != MouseButtons.Right) return;

            speedDialEntry = this.m_speedDial[cell_number];

            // The ordering will be set in the database
            speedDialEntry["ordering"] = cell_number.ToString();

            SpeedDialEntryEditor sdee = new SpeedDialEntryEditor(speedDialEntry, speedDialUpdateCallbackFn);
            sdee.ShowDialog(m_ic);

            // Get mouse position relative to the grid and show the context menu
            // var relativeMousePosition = DataGridView1.PointToClient(Cursor.Position);
            // this.ContextMenuStrip1.Show(DataGridView1, relativeMousePosition);
        }

        public void ShowBodyPanel() {
            this.panelSpeedDial.Show();
        }

        public void HideBodyPanel() {
            this.panelSpeedDial.Hide();
        }

        public DataGridView GetDataGridView() {
            return this.dataGridViewSpeedDial;
        }
    }
}
