using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace WindowsApplication1
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form{
        private System.Windows.Forms.Button button1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private Button copyButton;
        private Label label1;
        private Button button2;
        private MyCustomControls.InheritedCombo.MultiColumnComboBox multiColumnComboBox1;
		//private System.ComponentModel.IContainer components;
		private DataTable dtable;

		public Form1(){
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		public static void Main(string[] args)	{
			Application.Run(new Form1());
		}
		
        //protected override void Dispose( bool disposing )	{
        //    if( disposing )
        //    {
        //        if(components != null)
        //        {
        //            components.Dispose();
        //        }
        //    }
        //    base.Dispose( disposing );
        //}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.button1 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.copyButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.multiColumnComboBox1 = new MyCustomControls.InheritedCombo.MultiColumnComboBox();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(182, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Load";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(38, 101);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(381, 149);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(373, 123);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // copyButton
            // 
            this.copyButton.Location = new System.Drawing.Point(264, 15);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(75, 23);
            this.copyButton.TabIndex = 6;
            this.copyButton.Text = "copy";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(261, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "label1";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(182, 63);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "get status";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // multiColumnComboBox1
            // 
            this.multiColumnComboBox1.DropDownHeight = 106;
            this.multiColumnComboBox1.FormattingEnabled = true;
            this.multiColumnComboBox1.IntegralHeight = false;
            this.multiColumnComboBox1.Location = new System.Drawing.Point(42, 17);
            this.multiColumnComboBox1.Name = "multiColumnComboBox1";
            this.multiColumnComboBox1.Size = new System.Drawing.Size(121, 21);
            this.multiColumnComboBox1.TabIndex = 9;
            this.multiColumnComboBox1.RowSelected += new MyCustomControls.InheritedCombo.RowSelectedEventHandler(this.multiColumnComboBox1_AfterSelectEvent);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(461, 281);
            this.Controls.Add(this.multiColumnComboBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.copyButton);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Multi Column Combo Test";
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e) {
			CreateDataTable();
			multiColumnComboBox1.Table = dtable;
			multiColumnComboBox1.DisplayMember = "Band";
			multiColumnComboBox1.ColumnsToDisplay = new string[]{"Band","Song","Album"};
            multiColumnComboBox1.selectByIndex(1);
            multiColumnComboBox1.selectByValue("Mr. Big");
			//button1.Enabled = false;
		}

		private void CreateDataTable(){
			dtable = new DataTable("Rock");
			//set columns names
			dtable.Columns.Add("Band",typeof(System.String));
			dtable.Columns.Add("Song",typeof(System.String));
			dtable.Columns.Add("Album",typeof(System.String));

			//Add Rows
			DataRow drow = dtable.NewRow();
			drow["Band"] = "Iron Maiden";
			drow["Song"] = "Wasted Years";
			drow["Album"] = "Ed Hunter";
			dtable.Rows.Add(drow);

			drow = dtable.NewRow();
            drow["Band"] = DateTime.UtcNow.ToString();
			drow["Song"] = "Enter Sandman";
			drow["Album"] = "Metallica";
			dtable.Rows.Add(drow);
			
			drow = dtable.NewRow();
			drow["Band"] = "Jethro Tull";
			drow["Song"] = "Locomotive Breath";
			drow["Album"] = "Aqualung";
			dtable.Rows.Add(drow);
			
			drow = dtable.NewRow();
			drow["Band"] = "Mr. Big";
			drow["Song"] = "Seven Impossible Days";
			drow["Album"] = "Japandemonium";
			dtable.Rows.Add(drow);
		}

        private void copyButton_Click(object sender, EventArgs e)
        {
            TabPage currentTab = tabControl1.TabPages[tabControl1.TabPages.Count - 1];
            //currentTab.Controls.Add(multiColumnComboBox1);
            for (int i = 0; i < this.Controls.Count; i++)
            {
                if (this.Controls[i] is MyCustomControls.InheritedCombo.MultiColumnComboBox)
                {
                    currentTab.Controls.Add(this.Controls[i--]); // must adjust index because above line removes newForm.controls[i] item from newForm.controls collection
                    copyButton.Enabled = false;
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            updateStatusLabel("");
        }

        private void updateStatusLabel(string prefix)
        {
            // get status
            string status = "";

            int idx = multiColumnComboBox1.getSelectedIndex();
            string val = multiColumnComboBox1.getSelectedValue();

            if (val == null)
                val = "-null-";

            status = prefix + ", " + idx.ToString() + ", " + val;

            label1.Text = status;

        }

        private void multiColumnComboBox1_AfterSelectEvent()
        {
            updateStatusLabel("event!");
        }
	}
}
