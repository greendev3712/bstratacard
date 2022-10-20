namespace IasPbxConfig
{
	partial class TrunkGroupIncludes
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
			this.cosLabel = new System.Windows.Forms.Label();
			this.includedGridLabel = new System.Windows.Forms.Label();
			this.availableGridLabel = new System.Windows.Forms.Label();
			this.cosiGrid = new System.Windows.Forms.DataGridView();
			this.cosaGrid = new System.Windows.Forms.DataGridView();
			this.unincludeButton = new System.Windows.Forms.Button();
			this.includeButton = new System.Windows.Forms.Button();
			this.moveDownButton = new System.Windows.Forms.Button();
			this.moveUpButton = new System.Windows.Forms.Button();
			this.cosiLabel = new System.Windows.Forms.Label();
			this.uidJumpToMultiCombo = new Lib.InheritedCombo.MultiColumnComboBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.cosiGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.cosaGrid)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// cosLabel
			// 
			this.cosLabel.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.cosLabel, 4);
			this.cosLabel.Location = new System.Drawing.Point(3, 29);
			this.cosLabel.Name = "cosLabel";
			this.cosLabel.Size = new System.Drawing.Size(70, 13);
			this.cosLabel.TabIndex = 0;
			this.cosLabel.Text = "Trunk Group:";
			// 
			// includedGridLabel
			// 
			this.includedGridLabel.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.includedGridLabel, 2);
			this.includedGridLabel.Location = new System.Drawing.Point(3, 69);
			this.includedGridLabel.Name = "includedGridLabel";
			this.includedGridLabel.Size = new System.Drawing.Size(51, 13);
			this.includedGridLabel.TabIndex = 2;
			this.includedGridLabel.Text = "Included:";
			// 
			// availableGridLabel
			// 
			this.availableGridLabel.AutoSize = true;
			this.availableGridLabel.Location = new System.Drawing.Point(333, 69);
			this.availableGridLabel.Name = "availableGridLabel";
			this.availableGridLabel.Size = new System.Drawing.Size(53, 13);
			this.availableGridLabel.TabIndex = 3;
			this.availableGridLabel.Text = "Available:";
			// 
			// cosiGrid
			// 
			this.cosiGrid.AllowUserToAddRows = false;
			this.cosiGrid.AllowUserToDeleteRows = false;
			this.cosiGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cosiGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.tableLayoutPanel1.SetColumnSpan(this.cosiGrid, 2);
			this.cosiGrid.Location = new System.Drawing.Point(3, 85);
			this.cosiGrid.Name = "cosiGrid";
			this.cosiGrid.ReadOnly = true;
			this.tableLayoutPanel1.SetRowSpan(this.cosiGrid, 2);
			this.cosiGrid.Size = new System.Drawing.Size(272, 184);
			this.cosiGrid.TabIndex = 4;
			this.cosiGrid.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.cosiGrid_CellMouseUp);
			// 
			// cosaGrid
			// 
			this.cosaGrid.AllowUserToAddRows = false;
			this.cosaGrid.AllowUserToDeleteRows = false;
			this.cosaGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cosaGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.cosaGrid.Location = new System.Drawing.Point(333, 85);
			this.cosaGrid.Name = "cosaGrid";
			this.cosaGrid.ReadOnly = true;
			this.tableLayoutPanel1.SetRowSpan(this.cosaGrid, 2);
			this.cosaGrid.Size = new System.Drawing.Size(272, 184);
			this.cosaGrid.TabIndex = 5;
			this.cosaGrid.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.cosaGrid_CellMouseUp);
			this.cosaGrid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.cosaGrid_MouseUp);
			this.cosaGrid.SelectionChanged += new System.EventHandler(this.cosaGrid_SelectionChanged);
			// 
			// unincludeButton
			// 
			this.unincludeButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.unincludeButton.Location = new System.Drawing.Point(281, 118);
			this.unincludeButton.Name = "unincludeButton";
			this.unincludeButton.Size = new System.Drawing.Size(46, 23);
			this.unincludeButton.TabIndex = 6;
			this.unincludeButton.Text = ">";
			this.unincludeButton.UseVisualStyleBackColor = true;
			this.unincludeButton.Click += new System.EventHandler(this.unincludeButton_Click);
			// 
			// includeButton
			// 
			this.includeButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.includeButton.Location = new System.Drawing.Point(281, 213);
			this.includeButton.Name = "includeButton";
			this.includeButton.Size = new System.Drawing.Size(46, 23);
			this.includeButton.TabIndex = 7;
			this.includeButton.Text = "<";
			this.includeButton.UseVisualStyleBackColor = true;
			this.includeButton.Click += new System.EventHandler(this.includeButton_Click);
			// 
			// moveDownButton
			// 
			this.moveDownButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.moveDownButton.Location = new System.Drawing.Point(32, 275);
			this.moveDownButton.Name = "moveDownButton";
			this.moveDownButton.Size = new System.Drawing.Size(75, 23);
			this.moveDownButton.TabIndex = 8;
			this.moveDownButton.Text = "v";
			this.moveDownButton.UseVisualStyleBackColor = true;
			this.moveDownButton.Click += new System.EventHandler(this.moveDownButton_Click);
			// 
			// moveUpButton
			// 
			this.moveUpButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.moveUpButton.Location = new System.Drawing.Point(171, 275);
			this.moveUpButton.Name = "moveUpButton";
			this.moveUpButton.Size = new System.Drawing.Size(75, 23);
			this.moveUpButton.TabIndex = 9;
			this.moveUpButton.Text = "^";
			this.moveUpButton.UseVisualStyleBackColor = true;
			this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
			// 
			// cosiLabel
			// 
			this.cosiLabel.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.cosiLabel, 4);
			this.cosiLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 18.25F);
			this.cosiLabel.Location = new System.Drawing.Point(3, 0);
			this.cosiLabel.Name = "cosiLabel";
			this.cosiLabel.Size = new System.Drawing.Size(254, 29);
			this.cosiLabel.TabIndex = 10;
			this.cosiLabel.Text = "Trunk Group Includes";
			// 
			// uidJumpToMultiCombo
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.uidJumpToMultiCombo, 4);
			this.uidJumpToMultiCombo.FormattingEnabled = true;
			this.uidJumpToMultiCombo.Location = new System.Drawing.Point(3, 45);
			this.uidJumpToMultiCombo.Name = "uidJumpToMultiCombo";
			this.uidJumpToMultiCombo.Size = new System.Drawing.Size(193, 21);
			this.uidJumpToMultiCombo.TabIndex = 11;
			this.uidJumpToMultiCombo.Text = "Select a Trunk Group";
			this.uidJumpToMultiCombo.RowSelected += new Lib.InheritedCombo.RowSelectedEventHandler(this.uidJumpToMultiCombo_RowSelected);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.cosiLabel, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.includeButton, 2, 5);
			this.tableLayoutPanel1.Controls.Add(this.moveUpButton, 1, 6);
			this.tableLayoutPanel1.Controls.Add(this.uidJumpToMultiCombo, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.moveDownButton, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.cosaGrid, 3, 4);
			this.tableLayoutPanel1.Controls.Add(this.unincludeButton, 2, 4);
			this.tableLayoutPanel1.Controls.Add(this.includedGridLabel, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.cosLabel, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.cosiGrid, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.availableGridLabel, 3, 3);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 7;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(608, 301);
			this.tableLayoutPanel1.TabIndex = 12;
			// 
			// TrunkGroupIncludes
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(608, 301);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "TrunkGroupIncludes";
			this.Text = "Trunk Group Includes";
			this.Load += new System.EventHandler(this.ClassesOfServiceIncludes_Load);
			((System.ComponentModel.ISupportInitialize)(this.cosiGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.cosaGrid)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label cosLabel;
		private System.Windows.Forms.Label includedGridLabel;
		private System.Windows.Forms.Label availableGridLabel;
		private System.Windows.Forms.DataGridView cosiGrid;
		private System.Windows.Forms.DataGridView cosaGrid;
		private System.Windows.Forms.Button unincludeButton;
		private System.Windows.Forms.Button includeButton;
		private System.Windows.Forms.Button moveDownButton;
		private System.Windows.Forms.Button moveUpButton;
		private System.Windows.Forms.Label cosiLabel;
		private Lib.InheritedCombo.MultiColumnComboBox uidJumpToMultiCombo;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}