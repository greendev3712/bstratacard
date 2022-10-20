using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using log4net;


namespace Lib
{
	/// This class was created to test an alternative method of changing and scrolling text
	/// in a richtextbox without getting a flicker from the new text appearing at the top
	/// for a split second. Unfortunatley RichTextBox doesn't support overriding OnPaint
	/// as well as other controls so this method does not work (tested on xp.) Leaving
	/// code here for testing on mono for which the current importdll method does not work.
	/// This isn't working here because only one onpaint is called depending on UserPaint
	/// setting set in ctor. We need both OnPaints to work so we can prevent the base onpaint
	/// from being called only occasionally.
	///
	/// Changing UserPaint on the fly as below doesn't seem to work either.. still flickers.
	/// Could be a bug in the below code though, but probably because previous image frame is 
	/// cleared instead of ignored. 
	///
	/// @todo possible solution/hack: take screenshot and display over richtextbox as needed. 
	/// See 
	/// http://www.switchonthecode.com/tutorials/taking-some-screenshots-with-csharp
	/// for screenshot capture code. 
	///
	/// @todo investigate another possible solution: 
	/// http://stackoverflow.com/questions/2539903/sendmessage-vs-wndproc
	///
	/// note: this flickering issue is not a double buffer issue (afaik).
	class TweakedRichTextBox : RichTextBox
	{
		private bool m_drawsEnabled = true;

		public TweakedRichTextBox()
		{
			m_drawsEnabled = true;

			// uncommenting this makes our onpaint get called, but then calling base.onpaint
			// does nothing.
			//this.SetStyle(ControlStyles.UserPaint, true);
		}

		public bool DrawEnabled
		{
			set
			{
				m_drawsEnabled = value;

				if (m_drawsEnabled)
				{
					this.SetStyle(ControlStyles.UserPaint, false);
					//base.Update();
					this.Refresh();
				}
				else
				{
					this.SetStyle(ControlStyles.UserPaint, true);
				}

			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			//if (m_drawsEnabled)
 				//base.OnPaint(e);
		}
	}
}
