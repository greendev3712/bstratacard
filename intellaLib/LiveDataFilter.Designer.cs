namespace Lib
{
	partial class LiveDataViewer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tableLayoutPanelEverything = new System.Windows.Forms.TableLayoutPanel();
			this.headingLabel = new System.Windows.Forms.Label();
			this.splitContainerEverything = new System.Windows.Forms.SplitContainer();
			this.tableLayoutPanelTop = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.eventsDataGridView = new System.Windows.Forms.DataGridView();
			this.headerTableLayoutPanel = new Lib.FastTlp();
			this.tableLayoutPanelBottom = new System.Windows.Forms.TableLayoutPanel();
			this.label2 = new System.Windows.Forms.Label();
			this.eventDetailsTextbox = new System.Windows.Forms.TextBox();
			this.tableLayoutPanelEverything.SuspendLayout();
			this.splitContainerEverything.Panel1.SuspendLayout();
			this.splitContainerEverything.Panel2.SuspendLayout();
			this.splitContainerEverything.SuspendLayout();
			this.tableLayoutPanelTop.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.eventsDataGridView)).BeginInit();
			this.tableLayoutPanelBottom.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanelEverything
			// 
			this.tableLayoutPanelEverything.ColumnCount = 1;
			this.tableLayoutPanelEverything.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelEverything.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelEverything.Controls.Add(this.headingLabel, 0, 0);
			this.tableLayoutPanelEverything.Controls.Add(this.splitContainerEverything, 0, 1);
			this.tableLayoutPanelEverything.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelEverything.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelEverything.Name = "tableLayoutPanelEverything";
			this.tableLayoutPanelEverything.RowCount = 2;
			this.tableLayoutPanelEverything.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelEverything.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelEverything.Size = new System.Drawing.Size(576, 305);
			this.tableLayoutPanelEverything.TabIndex = 0;
			// 
			// headingLabel
			// 
			this.headingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.headingLabel.AutoSize = true;
			this.headingLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 18.25F);
			this.headingLabel.Location = new System.Drawing.Point(3, 0);
			this.headingLabel.Name = "headingLabel";
			this.headingLabel.Size = new System.Drawing.Size(570, 29);
			this.headingLabel.TabIndex = 0;
			this.headingLabel.Text = "Trace Viewer";
			// 
			// splitContainerEverything
			// 
			this.splitContainerEverything.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainerEverything.Location = new System.Drawing.Point(3, 32);
			this.splitContainerEverything.Name = "splitContainerEverything";
			this.splitContainerEverything.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerEverything.Panel1
			// 
			this.splitContainerEverything.Panel1.Controls.Add(this.tableLayoutPanelTop);
			// 
			// splitContainerEverything.Panel2
			// 
			this.splitContainerEverything.Panel2.BackColor = System.Drawing.SystemColors.Control;
			this.splitContainerEverything.Panel2.Controls.Add(this.tableLayoutPanelBottom);
			this.splitContainerEverything.Size = new System.Drawing.Size(570, 270);
			this.splitContainerEverything.SplitterDistance = 174;
			this.splitContainerEverything.TabIndex = 1;
			// 
			// tableLayoutPanelTop
			// 
			this.tableLayoutPanelTop.ColumnCount = 1;
			this.tableLayoutPanelTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelTop.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanelTop.Controls.Add(this.panel1, 0, 1);
			this.tableLayoutPanelTop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelTop.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelTop.Name = "tableLayoutPanelTop";
			this.tableLayoutPanelTop.RowCount = 2;
			this.tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelTop.Size = new System.Drawing.Size(570, 174);
			this.tableLayoutPanelTop.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(564, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "Events";
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.eventsDataGridView);
			this.panel1.Controls.Add(this.headerTableLayoutPanel);
			this.panel1.Location = new System.Drawing.Point(3, 23);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(564, 148);
			this.panel1.TabIndex = 4;
			this.panel1.Resize += new System.EventHandler(this.panel1_Resize);
			// 
			// eventsDataGridView
			// 
			this.eventsDataGridView.AllowUserToAddRows = false;
			this.eventsDataGridView.AllowUserToDeleteRows = false;
			this.eventsDataGridView.AllowUserToResizeColumns = false;
			this.eventsDataGridView.AllowUserToResizeRows = false;
			this.eventsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.eventsDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.eventsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.eventsDataGridView.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.eventsDataGridView.Location = new System.Drawing.Point(0, 55);
			this.eventsDataGridView.Name = "eventsDataGridView";
			this.eventsDataGridView.ReadOnly = true;
			this.eventsDataGridView.RowHeadersVisible = false;
			this.eventsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.eventsDataGridView.Size = new System.Drawing.Size(564, 93);
			this.eventsDataGridView.TabIndex = 3;
			this.eventsDataGridView.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(this.eventsDataGridView_ColumnWidthChanged);
			this.eventsDataGridView.Scroll += new System.Windows.Forms.ScrollEventHandler(this.eventsDataGridView_Scroll);
			this.eventsDataGridView.SelectionChanged += new System.EventHandler(this.eventsDataGridView_SelectionChanged);
			// 
			// headerTableLayoutPanel
			// 
			this.headerTableLayoutPanel.AutoScroll = true;
			this.headerTableLayoutPanel.AutoSize = true;
			this.headerTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.headerTableLayoutPanel.ColumnCount = 1;
			this.headerTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.headerTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.headerTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.headerTableLayoutPanel.Name = "headerTableLayoutPanel";
			this.headerTableLayoutPanel.RowCount = 1;
			this.headerTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.headerTableLayoutPanel.Size = new System.Drawing.Size(564, 0);
			this.headerTableLayoutPanel.TabIndex = 2;
			// 
			// tableLayoutPanelBottom
			// 
			this.tableLayoutPanelBottom.ColumnCount = 1;
			this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBottom.Controls.Add(this.label2, 0, 0);
			this.tableLayoutPanelBottom.Controls.Add(this.eventDetailsTextbox, 0, 1);
			this.tableLayoutPanelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelBottom.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
			this.tableLayoutPanelBottom.RowCount = 2;
			this.tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBottom.Size = new System.Drawing.Size(570, 92);
			this.tableLayoutPanelBottom.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
			this.label2.Location = new System.Drawing.Point(3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(564, 20);
			this.label2.TabIndex = 1;
			this.label2.Text = "Details";
			// 
			// eventDetailsTextbox
			// 
			this.eventDetailsTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.eventDetailsTextbox.Location = new System.Drawing.Point(3, 23);
			this.eventDetailsTextbox.Multiline = true;
			this.eventDetailsTextbox.Name = "eventDetailsTextbox";
			this.eventDetailsTextbox.ReadOnly = true;
			this.eventDetailsTextbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.eventDetailsTextbox.Size = new System.Drawing.Size(564, 66);
			this.eventDetailsTextbox.TabIndex = 2;
			// 
			// LiveDataViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(576, 305);
			this.Controls.Add(this.tableLayoutPanelEverything);
			this.Name = "LiveDataViewer";
			this.Text = "TraceViewer";
			this.tableLayoutPanelEverything.ResumeLayout(false);
			this.tableLayoutPanelEverything.PerformLayout();
			this.splitContainerEverything.Panel1.ResumeLayout(false);
			this.splitContainerEverything.Panel2.ResumeLayout(false);
			this.splitContainerEverything.ResumeLayout(false);
			this.tableLayoutPanelTop.ResumeLayout(false);
			this.tableLayoutPanelTop.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.eventsDataGridView)).EndInit();
			this.tableLayoutPanelBottom.ResumeLayout(false);
			this.tableLayoutPanelBottom.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelEverything;
		private System.Windows.Forms.Label headingLabel;
		private System.Windows.Forms.SplitContainer splitContainerEverything;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTop;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBottom;
		private System.Windows.Forms.Label label1;
		//private Lib.F.FastTlp eventsTableLayoutPanel;
		private System.Windows.Forms.Label label2;
		//private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTopSub;
		private Lib.FastTlp headerTableLayoutPanel;
		private System.Windows.Forms.DataGridView eventsDataGridView;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TextBox eventDetailsTextbox;
	}
}