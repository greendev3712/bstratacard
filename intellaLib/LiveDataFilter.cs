using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using log4net;

namespace Lib
{
	public partial class LiveDataViewer : ParamForm
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int GridColumnWhitespacePaddingWidth = 15;
		private const int GridColumnMinimumWidth = 120;

		private bool isInitDone = false;

		private Hashtable m_eventData;

		protected const string kHeading = "HeadingText";

		protected const string kColumns = "Columns";
		protected const string kEvents = "Events";

		protected const string kType = "Type";
		protected const string kTypeUnixtime = "TypeUnixtime";

		//////////////////////////////////////////////////////////////////////////////////
		/// Constructor.
		///
		/// allocates members.
		/// calls load(), which, in turns, populates all implemented Controls
		public LiveDataViewer()
		{
			log.Debug("");
			m_parameterNames = new List<string>(0);
			m_parameterValues = new List<string>(0);
			m_eventData = new Hashtable();

			m_eventData[kColumns] = new List<Hashtable>(4);
			m_eventData[kEvents] = new List<Hashtable>();

			InitializeComponent();

			load();
		}

		public void initEventData(Hashtable initialEventData)
		{
			m_eventData = initialEventData;
		}

		public void addEvent(Hashtable singleEvent)
		{
			addEventSingle(singleEvent, true);
			filterEventGrid();
		}

		private void addEventSingle(Hashtable singleEvent, bool doAddToData)
		{
			if (doAddToData)
				((List<Hashtable>)m_eventData[kEvents]).Add(singleEvent);

			int cc = ((List<Hashtable>)m_eventData[kColumns]).Count;
			eventsDataGridView.Rows.Add();
			eventsDataGridView.Rows[eventsDataGridView.Rows.Count - 1].Tag = singleEvent;

			for (int j = 0; j < cc; j++)
			{
				string displayedValue = (string)singleEvent[((List<Hashtable>)m_eventData[kColumns])[j][kHeading]];
				object type = ((List<Hashtable>)m_eventData[kColumns])[j][kType];
				if (type != null)
					type = type.ToString();
				switch ((string)type)
				{
					case kTypeUnixtime:
						displayedValue = (Helper.fromUnixtime(Double.Parse(displayedValue))).ToString();
						break;
					case null:
						goto default;
					default:
						break;
				}
				eventsDataGridView[j, eventsDataGridView.Rows.Count - 1].Value = displayedValue;
			}
		}

		public void addEventRange(IEnumerable<Hashtable> multipleEvents)
		{
			addEventRange(multipleEvents, false);
		}

		private void addEventRange(IEnumerable<Hashtable> multipleEvents, bool isInitStep)
		{
			if (!isInitStep)
				((List<Hashtable>)m_eventData[kEvents]).AddRange(multipleEvents);

			foreach (var item in multipleEvents)
				addEventSingle(item, false);
			filterEventGrid();
		}

		public virtual void load()
		{
			updateEventGrid();
			initHeader();
			isInitDone = true;
		}

		private void initHeader()
		{
			headerTableLayoutPanel.Padding = new System.Windows.Forms.Padding(headerTableLayoutPanel.Padding.Left, headerTableLayoutPanel.Padding.Top, headerTableLayoutPanel.Padding.Right, headerTableLayoutPanel.Padding.Bottom + SystemInformation.HorizontalScrollBarHeight + 5);

			headerTableLayoutPanel.Controls.Clear();
			headerTableLayoutPanel.RowStyles.Clear();
			headerTableLayoutPanel.ColumnStyles.Clear();
			headerTableLayoutPanel.ColumnCount = ((List<Hashtable>)m_eventData[kColumns]).Count;
			headerTableLayoutPanel.RowCount = 2;
			headerTableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
			headerTableLayoutPanel.AutoSize = true;
			headerTableLayoutPanel.MouseWheel += new MouseEventHandler(headerTableLayoutPanel_MouseWheel);

			for (int i = 0; i < headerTableLayoutPanel.ColumnCount; i++)
			{
				for (int j = 0; j < headerTableLayoutPanel.RowCount; j++)
				{
					if (j == 0)
					{
						// Header Title
						Label columnHeaderLabel = new Label();
						//hi.Text = "hi there lalalalalaa lalalalaa la al ala lal alalalaallala yay." + i.ToString() + j.ToString();
						columnHeaderLabel.Text = ((List<Hashtable>)m_eventData[kColumns])[i][kHeading].ToString();
						columnHeaderLabel.Dock = DockStyle.Fill;
						columnHeaderLabel.AutoSize = true;
						columnHeaderLabel.AutoEllipsis = true;
						headerTableLayoutPanel.Controls.Add(columnHeaderLabel, i, j);
						continue;
					}
					// Header Filter
					TextBox tb = new TextBox();
					tb.Dock = DockStyle.Fill;
					tb.TextChanged += new EventHandler(tb_TextChanged);
					tb.Enter += new EventHandler(tb_Enter);
					tb.Tag = i;
					headerTableLayoutPanel.Controls.Add(tb, i, j);
				}
			}

			headerTableLayoutPanel.ColumnStyles.Clear();
			for (int i = 0; i < headerTableLayoutPanel.ColumnCount; i++)
			{
				headerTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			}
			for (int i = 0; i < headerTableLayoutPanel.RowCount; i++)
			{
				headerTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
			}

			float totalWidth = updateHeaderWidths();
			headerTableLayoutPanel.Size = new System.Drawing.Size((int)totalWidth, headerTableLayoutPanel.Size.Height);
		}

		void tb_Enter(object sender, EventArgs e)
		{

		}

		void headerTableLayoutPanel_MouseWheel(object sender, MouseEventArgs e)
		{
			log.Debug("hi");
		}

		void tb_TextChanged(object sender, EventArgs e)
		{
			TextBox tb = (TextBox)sender;

			filterEventGrid();
		}

		void filterEventGrid()
		{

			foreach (DataGridViewRow r in eventsDataGridView.Rows)
			{
				bool willBeVisible = true;
				for (int i = 0; willBeVisible && i + 1 < headerTableLayoutPanel.Controls.Count; i += 2)
				{
					TextBox tb = (TextBox)headerTableLayoutPanel.Controls[i + 1];
					if (tb.Text == "")
						continue;
					willBeVisible = r.Cells[(int)tb.Tag].Value.ToString().Contains(tb.Text);
				}
				r.Visible = willBeVisible;
			}

		}

		private void updateEventGrid()
		{
			eventsDataGridView.Rows.Clear();
			eventsDataGridView.Columns.Clear();

			int rc = ((List<Hashtable>)m_eventData[kEvents]).Count;
			int cc = ((List<Hashtable>)m_eventData[kColumns]).Count;

			for (int j = 0; j < cc; j++)
			{
				eventsDataGridView.Columns.Add(((List<Hashtable>)m_eventData[kColumns])[j][kHeading].ToString(), "");
				eventsDataGridView.Columns[eventsDataGridView.Columns.Count - 1].SortMode = DataGridViewColumnSortMode.NotSortable;
			}

			addEventRange((List<Hashtable>)m_eventData[kEvents], true);
			int totalWidth = 0;
			for (int j = 0; j < cc; j++)
			{
				int maxWidth = 0;
				for (int i = 0; i < rc; i++)
				{

					if (maxWidth < eventsDataGridView[j, i].PreferredSize.Width)
						maxWidth = eventsDataGridView[j, i].PreferredSize.Width;

				}
				eventsDataGridView.Columns[j].Width = Math.Max(GridColumnMinimumWidth, maxWidth + GridColumnWhitespacePaddingWidth);
				totalWidth += eventsDataGridView.Columns[j].Width;

			}
			filterEventGrid();

			if (eventsDataGridView.Size.Width > totalWidth && eventsDataGridView.Columns.Count > 0)
				eventsDataGridView.Columns[eventsDataGridView.Columns.Count - 1].Width += eventsDataGridView.Size.Width - totalWidth - 17;

		}

		private float updateHeaderWidths()
		{
			float totalWidth = 0;
			for (int i = 0; i < headerTableLayoutPanel.ColumnStyles.Count; i++)
			{
				headerTableLayoutPanel.ColumnStyles[i].SizeType = SizeType.Absolute;
				headerTableLayoutPanel.ColumnStyles[i].Width = eventsDataGridView.Columns[i].Width - (headerTableLayoutPanel.CellBorderStyle == TableLayoutPanelCellBorderStyle.None ? 0 : 1);
				totalWidth += headerTableLayoutPanel.ColumnStyles[i].Width;
			}
			return totalWidth;
		}

		private void updateHeaderScrolling(int x)
		{
			headerTableLayoutPanel.AutoScrollPosition = new Point(x, 0);
		}

		private void eventsDataGridView_Scroll(object sender, ScrollEventArgs e)
		{
			if (e.ScrollOrientation != ScrollOrientation.HorizontalScroll)
				return;
			updateHeaderScrolling(e.NewValue);
		}

		private void panel1_Resize(object sender, EventArgs e)
		{
			// adjust size to cover scrollbar on header
			eventsDataGridView.Size = new Size(eventsDataGridView.Size.Width, panel1.Size.Height - headerTableLayoutPanel.Size.Height + SystemInformation.HorizontalScrollBarHeight + 0);

		}

		private void eventsDataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
		{
			if (isInitDone)
				updateHeaderWidths();
		}

		public void Notify_ResizeEnd(object sender, EventArgs e)
		{
			updateEventGrid();
		}

		private void eventsDataGridView_SelectionChanged(object sender, EventArgs e)
		{
			// row got selected
			eventDetailsTextbox.Text = Helper.deepTextDump(eventsDataGridView.CurrentRow.Tag);
		}
	}
}
