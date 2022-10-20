/*
namespace Lib
{
	partial class TableForm
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
			this.deleteSelectedButton = new System.Windows.Forms.Button();
			this.doubleClickMessageLabel = new System.Windows.Forms.Label();
			this.copySelectedButton = new System.Windows.Forms.Button();
			this.grid = new Lib.TweakedDataGridView();
			this.tableFormTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.titleLabel = new System.Windows.Forms.Label();
			this.buttonPanel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.tableFormTableLayoutPanel.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// deleteSelectedButton
			// 
			this.deleteSelectedButton.Location = new System.Drawing.Point(3, 41);
			this.deleteSelectedButton.Name = "deleteSelectedButton";
			this.deleteSelectedButton.Size = new System.Drawing.Size(106, 23);
			this.deleteSelectedButton.TabIndex = 17;
			this.deleteSelectedButton.Text = "Delete Selected";
			this.deleteSelectedButton.UseVisualStyleBackColor = true;
			this.deleteSelectedButton.Click += new System.EventHandler(this.trkDeleteSelectedButton_Click);
			// 
			// doubleClickMessageLabel
			// 
			this.doubleClickMessageLabel.AutoSize = true;
			this.doubleClickMessageLabel.Location = new System.Drawing.Point(153, 8);
			this.doubleClickMessageLabel.Name = "doubleClickMessageLabel";
			this.doubleClickMessageLabel.Size = new System.Drawing.Size(221, 13);
			this.doubleClickMessageLabel.TabIndex = 15;
			this.doubleClickMessageLabel.Text = "Double Click a trunk to view or edit its details.";
			// 
			// copySelectedButton
			// 
			this.copySelectedButton.Location = new System.Drawing.Point(3, 3);
			this.copySelectedButton.Name = "copySelectedButton";
			this.copySelectedButton.Size = new System.Drawing.Size(106, 23);
			this.copySelectedButton.TabIndex = 14;
			this.copySelectedButton.Text = "Copy Selected";
			this.copySelectedButton.UseVisualStyleBackColor = true;
			this.copySelectedButton.Click += new System.EventHandler(this.trkCopySelectedButton_Click);
			// 
			// grid
			// 
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				    | System.Windows.Forms.AnchorStyles.Left)
				    | System.Windows.Forms.AnchorStyles.Right)));
			this.grid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.grid.Location = new System.Drawing.Point(3, 32);
			this.grid.Name = "grid";
			this.grid.Size = new System.Drawing.Size(743, 206);
			this.grid.TabIndex = 13;
			this.grid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellClick);
			this.grid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.trkGrid_CellDoubleClick);
			this.grid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.trkGrid_CellEndEdit);
			this.grid.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.grid_ColumnHeaderMouseClick);
			this.grid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.trkGrid_DataError);
			// 
			// tableFormTableLayoutPanel
			// 
			this.tableFormTableLayoutPanel.ColumnCount = 1;
			this.tableFormTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableFormTableLayoutPanel.Controls.Add(this.titleLabel, 0, 0);
			this.tableFormTableLayoutPanel.Controls.Add(this.buttonPanel, 0, 3);
			this.tableFormTableLayoutPanel.Controls.Add(this.grid, 0, 2);
			this.tableFormTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableFormTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.tableFormTableLayoutPanel.Name = "tableFormTableLayoutPanel";
			this.tableFormTableLayoutPanel.RowCount = 4;
			this.tableFormTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableFormTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableFormTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableFormTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableFormTableLayoutPanel.Size = new System.Drawing.Size(749, 324);
			this.tableFormTableLayoutPanel.TabIndex = 18;
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 18.25F);
			this.titleLabel.Location = new System.Drawing.Point(3, 0);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(61, 29);
			this.titleLabel.TabIndex = 10;
			this.titleLabel.Text = "Title";
			// 
			// buttonPanel
			// 
			this.buttonPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonPanel.Controls.Add(this.copySelectedButton);
			this.buttonPanel.Controls.Add(this.doubleClickMessageLabel);
			this.buttonPanel.Controls.Add(this.deleteSelectedButton);
			this.buttonPanel.Location = new System.Drawing.Point(3, 244);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(377, 77);
			this.buttonPanel.TabIndex = 19;
			// 
			// TableForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(749, 324);
			this.Controls.Add(this.tableFormTableLayoutPanel);
			this.KeyPreview = true;
			this.Name = "TableForm";
			this.Text = "TableForm";
			this.Load += new System.EventHandler(this.TableForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.tableFormTableLayoutPanel.ResumeLayout(false);
			this.tableFormTableLayoutPanel.PerformLayout();
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button deleteSelectedButton;
		private System.Windows.Forms.Label doubleClickMessageLabel;
		private System.Windows.Forms.Button copySelectedButton;
		private Lib.TweakedDataGridView grid;
		public System.Windows.Forms.TableLayoutPanel tableFormTableLayoutPanel;
		public System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.Label titleLabel;
	}
}
*/