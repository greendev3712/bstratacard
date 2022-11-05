using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using log4net.Layout;

namespace Lib {
    public partial class LiveDataGridView : TweakedDataGridView {
        [Obsolete("This is obsolete, use CellCallback instead")]
        public delegate Color? CellColorCallback(DataGridViewCell cell, string columnName, string columnText, string columnTextRaw, Hashtable columnTag, QueryResultSetRecord rowData);

        public delegate bool  CellCallback(DataGridViewCell cell, string columnName, string columnText, string columnTextRaw, Hashtable columnTag, QueryResultSetRecord rowData);

        private CellColorCallback m_cellColorCallback = Callback_CellColor_NoOp;
        private CellCallback      m_cellDataCallback  = Callback_Cell_Noop;

        private bool m_AutoSetFontSizes = true;

        ///////////////////////////////////////

        public LiveDataGridView() {
            InitializeComponent();

            this.SetupCmpTweakedDataGridView();
        }

        public void Set_AutoSetFontSizes(bool val) {
            this.m_AutoSetFontSizes = val;
        }

        public void SetCellColorCallback(CellColorCallback cellColorCallback) {
            m_cellColorCallback = cellColorCallback;
        }

        public void Set_CellCallback(CellCallback cellCallback) {
           m_cellDataCallback = cellCallback;
        }

        public int Get_MaxGridWidth() {
            return this.Columns.GetColumnsWidth(DataGridViewElementStates.Visible);
        }

        private void SetupCmpTweakedDataGridView() {
            LiveDataGridView grid = this;

            System.Windows.Forms.DataGridViewCellStyle dataGridViewColumnHeadersCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewRowHeadersCellStyle    = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewDefaultCellStyle       = new System.Windows.Forms.DataGridViewCellStyle();

            grid.Name                     = "grid";
            grid.ReadOnly                 = true;
            grid.AllowUserToAddRows       = false;
            grid.AllowUserToDeleteRows    = false;
            grid.AllowUserToResizeColumns = false;
            grid.AllowUserToResizeRows    = false;
            grid.ShowCellToolTips         = false;
            grid.ShowEditingIcon          = false;
            grid.RowHeadersVisible        = false;
            grid.MultiSelect              = false;
            grid.GridColor                = Color.Black;
            grid.GridColor                = System.Drawing.SystemColors.ControlDarkDark;

            dataGridViewColumnHeadersCellStyle.Alignment          = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewColumnHeadersCellStyle.BackColor          = System.Drawing.Color.FromArgb(((int)(((byte)(193)))), ((int)(((byte)(193)))), ((int)(((byte)(193)))));
            dataGridViewColumnHeadersCellStyle.Font               = new System.Drawing.Font("Microsoft Sans Serif", 16.5F);
            dataGridViewColumnHeadersCellStyle.ForeColor          = System.Drawing.SystemColors.WindowText;
            dataGridViewColumnHeadersCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewColumnHeadersCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewColumnHeadersCellStyle.WrapMode           = System.Windows.Forms.DataGridViewTriState.True;

            dataGridViewRowHeadersCellStyle.Alignment          = System.Windows.Forms.DataGridViewContentAlignment.BottomCenter;
            dataGridViewRowHeadersCellStyle.BackColor          = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(173)))), ((int)(((byte)(173)))));
            dataGridViewRowHeadersCellStyle.Font               = new System.Drawing.Font("Microsoft Sans Serif", 16.5F);
            dataGridViewRowHeadersCellStyle.ForeColor          = System.Drawing.SystemColors.ControlText;
            dataGridViewRowHeadersCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewRowHeadersCellStyle.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewRowHeadersCellStyle.WrapMode           = System.Windows.Forms.DataGridViewTriState.False;

            dataGridViewDefaultCellStyle.Alignment          = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewDefaultCellStyle.BackColor          = System.Drawing.SystemColors.Control;
            dataGridViewDefaultCellStyle.Font               = new System.Drawing.Font("Microsoft Sans Serif", 18.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewDefaultCellStyle.ForeColor          = System.Drawing.SystemColors.WindowText;
            dataGridViewDefaultCellStyle.SelectionBackColor = Helper.darkenColor(grid.DefaultCellStyle.BackColor); // Grid body is a little darker than the headers
            dataGridViewDefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewDefaultCellStyle.WrapMode           = System.Windows.Forms.DataGridViewTriState.True;

            grid.ColumnHeadersDefaultCellStyle = dataGridViewColumnHeadersCellStyle;
            grid.RowHeadersDefaultCellStyle    = dataGridViewDefaultCellStyle;
            grid.DefaultCellStyle              = dataGridViewRowHeadersCellStyle;

            grid.DefaultCellStyle.SelectionBackColor = Helper.darkenColor(grid.DefaultCellStyle.BackColor);
            grid.DefaultCellStyle.Alignment          = DataGridViewContentAlignment.MiddleCenter;

            grid.ColumnHeadersBorderStyle     = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersHeight         += 5; // A little padding
            grid.ColumnHeadersHeightSizeMode  = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.AutoSizeColumnsMode          = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            grid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            grid.RowTemplate.Height      = 13;
            grid.AutoSizeRowsMode        = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

            grid.BackgroundColor           = System.Drawing.SystemColors.ControlDark;
            grid.BorderStyle               = System.Windows.Forms.BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;

            grid.SelectionMode             = DataGridViewSelectionMode.FullRowSelect;

            //grid.ColumnHeadersDefaultCellStyle.Font = new Font(grid.ColumnHeadersDefaultCellStyle.Font.FontFamily, grid.ColumnHeadersDefaultCellStyle.Font.Size - 4);

            grid.ColumnHeadersHeightSizeMode = grid.ColumnHeadersHeightSizeMode;
            grid.ColumnHeadersBorderStyle    = grid.ColumnHeadersBorderStyle;

            //grid.BorderStyle = dropGridTemplate.BorderStyle;
            //grid.Tag = is_teamview_grid;
            //grid.Name = queueName;

            this.Resize += (object sender, EventArgs e) => {
                this.SetOptimalGridFontSizes();
            };
            
            grid.CellMouseDown += (object sender, DataGridViewCellMouseEventArgs e) => {
                // Placeholder for.... something?
                return;
            };

            //grid.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(grid_CellMouseEnter);
            //grid.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(grid_CellMouseLeave);
            //grid.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(grid_CellMouseUp);
            //grid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(grid_CellPainting);
            //grid.SelectionChanged += new System.EventHandler(grid_SelectionChanged);
            //grid.MouseDown += new System.Windows.Forms.MouseEventHandler(grid_MouseDown);
            //grid.CellDoubleClick += DropGridTemplate_CellDoubleClick;

            //    // right mouse click on an agent to get 'Set Agent Status' 
            //    //grid.CellMouseClick += AgentStatusGrid_CellMouseClick;

            //    // Agent Name is default sort
            //    grid.Columns[1].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            //}
        }

        public int GetColumnIndexByName(string colName) {
            if (!this.Columns.Contains(colName)) {
                return -1;
            }

            // ReSharper disable once PossibleNullReferenceException
            return this.Columns[colName].Index;
        }

        /// <summary>
        ///   Add a column to the grid
        /// </summary>
        /// <param name="colName">The QueryResultSet column to use when populating data for this column</param>
        /// <param name="colNameRaw">The QueryResultSet column containing 'raw' data (if needed) for the purposes of easy comparisons (ie: time in seconds)</param>
        /// <param name="colHeaderText"></param>
        public void AddGridColumn(string colName, string colNameRaw, string colHeaderText) {
            int col_pos = this.Columns.Add(colName, colHeaderText);

            this.Columns[col_pos].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

            ////
            // Custom Data in the Column

            Hashtable tag_data = new Hashtable(); // NOTE: Per Column Tag Hashtable, for each grid column
            tag_data.Add("raw_col_name", colNameRaw);

            this.Columns[col_pos].Tag = tag_data;

            ////
            // Special Cases should be handled by a SetFillColumn(columnName)

            if (colName == "agent_fullname") {
                this.Columns[col_pos].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            if (colName == "agent_status") {
                this.Columns[col_pos].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                this.Columns[col_pos].DefaultCellStyle.Font = new Font(this.Font.FontFamily, this.Font.Size - 1);
            }

            if (colName == "queue_name_caller") {
                this.Columns[col_pos].DefaultCellStyle.Font = new Font(this.Font.FontFamily, this.Font.Size - 1);
            }
        }

        public void PopulateDataRows(QueryResultSet dataRows) {
            if (dataRows.Count == 0) {
                return;
            }

            if (dataRows.Count > this.Rows.Count) {
                this.Rows.Add(dataRows.Count - this.Rows.Count);
            }
            else if (dataRows.Count < this.Rows.Count) {
                int rows_to_remove = this.Rows.Count - dataRows.Count;

                while (rows_to_remove-- > 0) {
                    this.Rows.RemoveAt(this.Rows.Count - 1);
                }
            }

            int edit_row = 0;
            DataGridViewTextBoxColumn col_textbox;

            foreach (QueryResultSetRecord row_data in dataRows) {
                int edit_col = 0;

                this.Rows[edit_row].Tag = row_data; // NOTE: Per-row Tag QueryResultSetRecord, in each grid row 

                foreach (DataGridViewColumn col in this.Columns) {
                    // Don't populate columns that we don't have in the grid... we may have extra 'hidden' columns for data purposes
                    //if (!this.Columns.Contains(col)) {
                        //continue;
                    //}

                    DataGridViewCell cell       = this.Rows[edit_row].Cells[edit_col];
                    Hashtable col_tag_hashtable = (Hashtable) this.Columns[edit_col].Tag;

                    if (col_tag_hashtable == null) {
                        col_tag_hashtable = new Hashtable();
                        col_tag_hashtable.Add("raw_col_name", "UNKNOWN");
                    }

                    string raw_col_name_for_cell = col_tag_hashtable["raw_col_name"].ToString();
                    string col_text              = row_data[col.Name];
                    string col_raw_text          = row_data[raw_col_name_for_cell];

                    if (col.CellType.Name == "DataGridViewTextBoxCell") {
                        col_textbox = (DataGridViewTextBoxColumn) col;

                        cell.Value = col_text;
                    }

                    Color? cell_color = this.m_cellColorCallback(cell, col.Name, col_text, col_raw_text, col_tag_hashtable, row_data);

                    this.m_cellDataCallback(cell, col.Name, col_text, col_raw_text, col_tag_hashtable, row_data);

                    if (cell_color != null) {
                      Color cell_color_actual = (Color) cell_color;
                      cell.Style.BackColor = cell_color_actual;
                    }

                    edit_col++;
                }

                edit_row++;
            }
        }
        
        public void SetOptimalGridFontSizes() {
            if (!this.m_AutoSetFontSizes) {
                return;
            }

            if (this.Columns.Count == 0) {
                return;
            }

            //this.Columns.IndexOf("agent_fullname)

            string longest_column_label = Utils.GetLongestTextFromDataGridViewColumns(this.Columns, "col_label");

            int perfect_column_width  = (this.Width / this.Columns.Count) - 10;
            int perfect_column_height = this.ColumnHeadersHeight;

            Font font = this.ColumnHeadersDefaultCellStyle.Font;

            List<int> sizes = Utils.FindLargestFontForBox(this.FindForm(), 50, perfect_column_width, perfect_column_height, longest_column_label, font);
            int font_size   = sizes[2];
            this.ColumnHeadersDefaultCellStyle.Font = new Font(font.FontFamily, font_size, font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont);

            this.AutoResizeColumns();
        }

        private static Color? Callback_CellColor_NoOp(DataGridViewCell cell, string columnName, string columnText, string columnTextRaw, Hashtable columnTag, QueryResultSetRecord rowData) {
            // No color change by default
            return null; 
        }

        private static bool Callback_Cell_Noop(DataGridViewCell cell, string columnName, string columnText, string columnTextRaw, Hashtable columnTag, QueryResultSetRecord rowData) {
            // No changes
            return false;
        }
    }
}
