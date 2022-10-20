using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace MyCustomControls.InheritedCombo
{
	/// <summary>
	/// Summary description for MultiColumnComboBox.
	/// </summary>
	public delegate void AfterSelectEventHandler();
    public delegate void RowSelectedEventHandler();

	public class MultiColumnComboBox : System.Windows.Forms.ComboBox
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
//		private System.ComponentModel.Container components = null;
		private DataRow selectedRow = null;
		private string displayMember = "";
		private string displayValue = "";
		private DataTable dataTable = null;
		private DataRow[] dataRows = null;
		private string[] columnsToDisplay = null;
		public event AfterSelectEventHandler AfterSelectEvent;
        public event RowSelectedEventHandler RowSelected;

        //public MultiColumnComboBox(System.ComponentModel.IContainer container)
        //{
        //    /// <summary>
        //    /// Required for Windows.Forms Class Composition Designer support
        //    /// </summary>
        //    container.Add(this);
        //    InitializeComponent();

        //    //
        //    // TODO: Add any constructor code after InitializeComponent call
        //    //
        //}

		public MultiColumnComboBox()
		{
			//InitializeComponent();

		}

        private void preventDefaultEmptyDropdown()
        {
            // prevent default empty dropdown
            Type t = this.GetType().BaseType;
            FieldInfo tPropDropDownHeight = t.GetField("PropDropDownHeight", BindingFlags.NonPublic | BindingFlags.Static);
            if (tPropDropDownHeight == null)
                return;
            object propDropDownHeight = (int)tPropDropDownHeight.GetValue(this);

            FieldInfo tProperties = t.BaseType.BaseType.GetField("propertyStore", BindingFlags.NonPublic | BindingFlags.Instance);
            object propertyStore = tProperties.GetValue(this);
            Type tPropertyStore = propertyStore.GetType();
            MethodInfo setInteger = tPropertyStore.GetMethod("SetInteger", BindingFlags.Public | BindingFlags.Instance);
            setInteger.Invoke(propertyStore, new object[] { (Int32)propDropDownHeight, (Int32)1 });
        }

        public void selectByIndex(int newIndex)
        {
            try
            {
                if (dataTable.Rows[newIndex] != null)
                {
                    this.selectedRow = dataTable.Rows[newIndex];
                    this.displayValue = this.Text = dataTable.Rows[newIndex][displayMember].ToString();
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(this, exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void selectByValue(string newValue)
        {
            try
            {
                int newIndex = -1;

                for (int i = 0; i < dataTable.Rows.Count; i++)
                    if (dataTable.Rows[i][displayMember].ToString() == newValue)
                    {
                        newIndex = i;
                        break;
                    }

                if (newIndex >= 0 && dataTable.Rows[newIndex] != null)
                {
                    this.selectedRow = dataTable.Rows[newIndex];
                    this.displayValue = this.Text = dataTable.Rows[newIndex][displayMember].ToString();
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(this, exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public int getSelectedIndex()
        {
            int idx = -1;
            try
            {
                if (dataTable != null)
                    idx = dataTable.Rows.IndexOf(this.selectedRow);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return idx;
        }

        public string getSelectedValue()
        {
            string result = null;

            result = this.selectedRow == null ? null : this.selectedRow[displayMember].ToString();

            return result;
        }

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
        //private void InitializeComponent()
        //{
        //    components = new System.ComponentModel.Container();
        //}
		#endregion

		protected override void OnDropDown(System.EventArgs e){
			Form parent = this.FindForm();
			if(this.dataTable != null || this.dataRows!= null){
				MultiColumnComboPopup popup = new MultiColumnComboPopup(this.dataTable,ref this.selectedRow,columnsToDisplay);
				popup.AfterRowSelectEvent+=new AfterRowSelectEventHandler(MultiColumnComboBox_AfterSelectEvent);
				//popup.Location = new Point(parent.Left + this.Left + 4 ,parent.Top + this.Bottom + this.Height);
                popup.Location = this.PointToScreen(new Point(0, this.Height)); 
				popup.Show();
				if(popup.SelectedRow!=null){
					try{
						this.selectedRow = popup.SelectedRow;
						this.displayValue = popup.SelectedRow[this.displayMember].ToString();
						this.Text = this.displayValue;
					}catch(Exception e2) {
						MessageBox.Show(e2.Message,"Error");	
					}
				}
				if(AfterSelectEvent!=null)
					AfterSelectEvent();
			}
			base.OnDropDown(e);
		}

		private void MultiColumnComboBox_AfterSelectEvent(object sender, DataRow drow){
			try{
				if(drow!=null){
                    this.selectedRow = drow;
					this.displayValue = this.Text = drow[displayMember].ToString();
                    if (RowSelected != null)
                        RowSelected();
				}
			}catch(Exception exp){
				MessageBox.Show(this,exp.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
			}
		}

		public DataRow SelectedRow{
			get{
				return selectedRow;
			}
		}

		public string DisplayValue{
			get{
				return displayValue;
			}
		}

		public new string DisplayMember{
			set{
				displayMember = value;
			}
		}

		public DataTable Table{
			set{
				dataTable = value;
				if(dataTable==null)
					return;
				selectedRow=dataTable.NewRow();
			}
		}

		public DataRow[] Rows{
			set{
				dataRows = value;
			}
		}

		public string[] ColumnsToDisplay{
			set{
				columnsToDisplay = value;
			}
		}

        public void initWithData(string[][] columnData)
        {
            initWithData(columnData, null);
        }

        public void initWithData(string[][] columnData, string valueColumnName)
        {
            preventDefaultEmptyDropdown();

            // not sure if we need to set this table title
            DataTable dtable = new DataTable(columnData[0][0]);

            //set columns names
            foreach (string col in columnData[0])
                //dtable.Columns.Add("Song", typeof(System.String));
                dtable.Columns.Add(col);

            int i, j;

            //Add Rows
            j = 0;
            for (j = 1; j < columnData.Length; j++)
            {
                i = 0;
                DataRow drow = dtable.NewRow();
                foreach (string col in columnData[0])
                {
                    drow[col] = columnData[j][i++];
                }
                dtable.Rows.Add(drow);
            }

            dataTable = dtable;
            displayMember = valueColumnName != null ? valueColumnName : columnData[0][0];
            columnsToDisplay = columnData[0];
        }

	}
}
