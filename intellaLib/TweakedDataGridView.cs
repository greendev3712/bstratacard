using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using log4net;

namespace Lib
{
	public class TweakedDataGridView : DataGridView
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private string m_previousValue;

		public TweakedDataGridView()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.EditingControlShowing += TweakedDataGridView_EditingControlShowing;
		}

        void TweakedDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e) {
            
        }

		public string PreviousValue
		{
            get {
				return m_previousValue;
			}
		}

        public void SetRowColor(int rowIndex, System.Drawing.Color colorType) {
            if (rowIndex < 0)
                return;

            if (rowIndex > base.Rows.Count)
                return;

            //foreach (DataGridViewRow row in base.Rows) {
                //row.DefaultCellStyle.BackColor = colorType;
            //}

            base.Rows[rowIndex].DefaultCellStyle.BackColor = colorType;
        }

		protected override void OnCellEnter(DataGridViewCellEventArgs e)
		{
			if (this[e.ColumnIndex, e.RowIndex].Value != null)
			{
				m_previousValue = this[e.ColumnIndex, e.RowIndex].Value.ToString();
				log.Debug("set previous value to " + m_previousValue);
			}
			else
				log.Debug("Did not set previous value. Cell is null");

			base.OnCellEnter(e);
		}
	}
}
