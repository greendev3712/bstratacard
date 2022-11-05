using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lib;

namespace intellaQueue
{
    public partial class DispositionCodesHistoryForm : Form
    {
        private DbHelper m_db = null;
        private IntellaQueueForm m_intellaQueueForm;

        private static Color QueueColorRegular = Color.FromArgb(173, 173, 173);
        private static Color QueueColorGreen   = Color.FromArgb(141, 193, 102);
        private static Color QueueColorYellow  = Color.FromArgb(243, 219, 119);
        private static Color QueueColorOrange  = Color.FromArgb(233, 138, 75);
        private static Color QueueColorRed     = Color.FromArgb(211, 101, 101);

        private bool m_saveMode = false;

        private QueryResultSet m_dispositionSaveData = new QueryResultSet();

        private int m_dataGridInitialHeight       = 0;
        private int m_dataGridFormHeightDifferece = 0;

        public DispositionCodesHistoryForm(IntellaQueueForm iq, DbHelper db) {
            InitializeComponent();

            // Very much required!
            m_intellaQueueForm  = iq;
            m_db                = db;

            // iq.GetTenantName(), iq.GetAgentNumber()

            cmpDispositionGridView.Set_AutoSetFontSizes(false);
            cmpDispositionGridView.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.0F, System.Drawing.FontStyle.Bold,    System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            cmpDispositionGridView.DefaultCellStyle.Font              = new System.Drawing.Font("Microsoft Sans Serif", 12.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            cmpDispositionGridView.ReadOnly                           = false;

            // cmpDispositionGridView = new LiveDataGridView {Width = m_parentPanel.Width, Height = m_parentPanel.Height};
            cmpDispositionGridView.Set_CellCallback(this.CellCallback);
            cmpDispositionGridView.SelectionChanged += (sender, args) => { LiveDataGridView g = (LiveDataGridView) sender; g.ClearSelection(); };
            cmpDispositionGridView.CellValueChanged += this.DataGridView_CellValueChanged;
            cmpDispositionGridView.MouseClick       += this.DataGridView_MouseClick;

            cmpDispositionGridView.AddGridColumn("call_start_time",    "call_start_time",    "Call Started");
            cmpDispositionGridView.AddGridColumn("callerid_from_name", "callerid_from_name", "From Name");
            cmpDispositionGridView.AddGridColumn("callerid_from_num",  "callerid_from_num",  "From Num");
            cmpDispositionGridView.AddGridColumn("callerid_to_name",   "callerid_to_name",   "To Name");
            cmpDispositionGridView.AddGridColumn("callerid_to_num",    "callerid_to_num",    "To Num");
            cmpDispositionGridView.AddGridColumn("duration_time",      "duration_time",      "Duration");
            cmpDispositionGridView.AddGridColumn("disposition_code",   "disposition_code",   "Disposition");

            // Custom Columns: TODO. Add custom column support to AddGridColumn

            DataGridViewComboBoxColumn dgv_combo_box_column = new DataGridViewComboBoxColumn();
            dgv_combo_box_column.Name             = "disposition_code_edit";
            dgv_combo_box_column.HeaderText       = "Disposition Code Update";

            QueryResultSet disposition_codes = m_db.DbSelect(@"
                SELECT
                    disposition_code_name,
                    disposition_code_longname,
                    manager_only
                FROM
                    queue.disposition_codes
                WHERE
                  tenant_name = ? or tenant_name = 'default' -- HACK ALERT for APOLLO.. we must update everyone's toolbars and then we can have dispositions only in apollo tenant
                ORDER BY
                  disposition_code_longname",

                m_intellaQueueForm.GetTenantName()
            );

            foreach (QueryResultSetRecord disposision in disposition_codes) {
                dgv_combo_box_column.Items.Add(new KeyValuePair<string,string> (disposision["disposition_code_longname"], disposision["disposition_code_name"]));
            }

            string max_column_text = Utils.GetLongestTextFromQueryResultSet(disposition_codes, "disposition_code_longname");
            int max_column_width   = (int) Utils.GUI_FindSizeOfRenderedText(this, max_column_text, cmpDispositionGridView.DefaultCellStyle.Font).Width;


            dgv_combo_box_column.Width         = max_column_width + 25;
            dgv_combo_box_column.AutoSizeMode  = DataGridViewAutoSizeColumnMode.None;

            dgv_combo_box_column.DisplayMember = "Key";
            dgv_combo_box_column.ValueMember   = "Value";

            cmpDispositionGridView.Columns.Add(dgv_combo_box_column);

            foreach (DataGridViewColumn col in cmpDispositionGridView.Columns) {
                if (col.Name != "disposition_code_edit") {
                    col.ReadOnly = true;
                }
            }
        }

        private void PopulateData() {
            m_saveMode            = false; // Disable SelectionChange events
            cmpDispositionGridView.Rows.Clear();

            QueryResultSet disposition_history = m_db.DbSelect(@"
                SELECT
                  call_log_id,
                  null as call_segment_id, -- FIXME -- Does not exist in view
                  call_start_time,
                  callerid_from_name,
                  callerid_from_num,
                  callerid_to_name,
                  callerid_to_num,
                  duration_time,
                  disposition_code
                FROM
                  log_asterisk.v_calls
                WHERE
                  call_start_time > NOW() - '24 hours'::interval
                  AND ((agent_from_id = ?) OR (agent_to_id = ?))
                ORDER BY
                  call_start_time desc
                ",
                m_intellaQueueForm.GetAgentID(),
                m_intellaQueueForm.GetAgentID()
            );

            cmpDispositionGridView.PopulateDataRows(disposition_history);
            
            m_saveMode            = true;
            m_dispositionSaveData = new QueryResultSet();
        }

        /// <summary>
        /// Called for each cell in the data grid view.
        /// This is called upon running cmpDispositionGridView.PopulateDataRows()
        /// </summary>
        /// <param name="cell">Specific cell we are in</param>
        /// <param name="columnName">The name of the column we're operating on</param>
        /// <param name="columnText">The label of the column we're operating on</param>
        /// <param name="columnTextRaw">The 'raw' value of the column ??</param>
        /// <param name="columnTag">Any associated extra data to go along with the Column</param>
        /// <param name="rowData">The QueryResultSetRecoed that was used to populate the entire row we're in</param>
        /// <returns>Whether or not the cell was changed</returns>
        public bool CellCallback(DataGridViewCell cell, string columnName, string columnText, string columnTextRaw, Hashtable columnTag, QueryResultSetRecord rowData) {
            // TODO: LiveDataGridView should implement an auto zebra if enabled

            bool changed = false;

            // Zebra Stripe
            if ((cell.RowIndex % 2) == 0) {
                cell.Style.BackColor = Utils.ChangeColorBrightness(QueueColorRegular, 0.5);
                changed = true;
            }
            else {
                cell.Style.BackColor = QueueColorRegular;
                changed = true;
            }

            switch (columnName) {
                case "disposition_code_edit": {
                    DataGridViewComboBoxCell combo = (DataGridViewComboBoxCell) cell;
                    break;
                }
                default: {
                    break;
                }
            }

            return changed;
        }

        private void liveDataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) {

        }

        private void DispositionCodesHistoryForm_VisibleChanged(object sender, EventArgs e) {
            if (!this.Visible) {
                return;
            }

            this.PopulateData();

            this.Width = DisposisionCodesForm_MaxWidth();

            // Make sure we have final dimensions
            Application.DoEvents();

            m_dataGridInitialHeight       = this.cmpDataGridViewPanel.Height;
            m_dataGridFormHeightDifferece = this.Height - m_dataGridInitialHeight;
        }

        private int DisposisionCodesForm_MaxWidth() {

            int max_form_width = this.cmpDispositionGridView.Columns.GetColumnsWidth(DataGridViewElementStates.Visible);
            
            //System.Windows.Forms.SystemInformation.BorderSize

            if ((this.cmpDispositionGridView.ScrollBars & ScrollBars.Vertical) == ScrollBars.Vertical) {
                max_form_width += System.Windows.Forms.SystemInformation.VerticalScrollBarWidth * 2;
            }

            // Size s = System.Windows.Forms.SystemInformation.BorderSize;

            max_form_width += 2; // Borders

            return max_form_width;
        }

        private void DispositionCodesHistoryForm_Resize(object sender, EventArgs e) {
            int max_form_width = DisposisionCodesForm_MaxWidth();
            // IntellaQueueForm.MQD("{0} {1}",  max_grid_width.ToString(), System.Windows.Forms.SystemInformation.VerticalScrollBarWidth.ToString());

            if (this.Width > max_form_width) {
                this.Width = max_form_width; //new Size(max_grid_width, this.Height);
            }

            // int new_height_difference = this.Height - this.cmpDispositionGridView.Height;

            int bottom_panel_height   = this.cmpBottomPanel.Height;
            int new_grid_height       = this.Height - bottom_panel_height;

            // this.cmpDispositionGridView.Height = new_grid_height;
            this.cmpDataGridViewPanel.Height   = new_grid_height - 50;

            // IntellaQueueForm.MQD("{0}",  this.cmpDispositionGridView.ClientSize.ToString());
        }
                
        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e) {
            LiveDataGridView dg = (LiveDataGridView) sender;

            if (!this.m_saveMode) {
                return;
            }

            DataGridViewCell cell         = dg.CurrentCell;
            DataGridViewRow row           = dg.CurrentRow;
            QueryResultSetRecord row_data = (QueryResultSetRecord) row.Tag;

            QueryResultSetRecord save_row = row_data.Clone();
            save_row["disposition_code"]  = cell.Value.ToString(); // New disposition code to set


            IntellaQueueForm.MQD("[Disposition] Flagged record for Save CallLogID: {0}, CallSegmentID: {1}, Disposition: {2}", save_row["call_log_id"], save_row["call_segment_id"], save_row["disposition_code"]);
            m_dispositionSaveData.Add(save_row);
        }

        private void DataGridView_MouseClick(object sender, MouseEventArgs e) {
            LiveDataGridView dg   = (LiveDataGridView) sender;
            DataGridViewCell cell = dg.CurrentCell;

            if (cell.OwningColumn == dg.Columns["disposition_code_edit"]) {
                DataGridViewComboBoxCell cb_cell = (DataGridViewComboBoxCell) cell;

                dg.BeginEdit(true);
                ((ComboBox) dg.EditingControl).DroppedDown = true;
            }
        }

        private void DispositionCodesHistoryForm_Load(object sender, EventArgs e) {

        }

        private void panel1_Paint(object sender, PaintEventArgs e) {

        }

        private void cmpCloseButton_Click(object sender, EventArgs e) {
            this.Hide();
        }

        private void cmpSaveButton_Click(object sender, EventArgs e) {
            IntellaQueueForm.MQD("[Disposition] Saving Changes...");

            foreach (QueryResultSetRecord row in m_dispositionSaveData) {
                m_db.DbSelect("UPDATE log_asterisk.calls SET disposition_code = ? WHERE call_log_id = ?", row["disposition_code"], row["call_log_id"]);
            }

            IntellaQueueForm.MQD("[Disposition] Changes Saved");

            this.PopulateData();
        }

        private void DispositionCodesHistoryForm_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true; // Don't dispose
            this.Hide();
        }
    }
}
