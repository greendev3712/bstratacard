using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Lib
{
    public class FastTlp : TableLayoutPanel
    {
        public FastTlp()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

		protected override Point ScrollToControl(Control activeControl)
		{
			//return base.ScrollToControl(activeControl);
			return AutoScrollPosition;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			//base.OnMouseWheel(e);
		}

    }
}
