using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using QuadSolutions.Minerva.ContactManagement;

namespace QuadSolutions.Minerva.CMForms {
	/// <summary>
	/// Summary description for CompanyControl.
	/// </summary>
	public class CompanyControl : System.Windows.Forms.UserControl {
		private ActiproSoftware.TabStripPanel.FitTabStripPanel detailsTabPanel;
		private QuadSolutions.Minerva.WinControls.CommunicationViewer communicationViewer;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private QuadSolutions.Minerva.WinControls.qsNotes NotesControl;
		private QuadSolutions.Minerva.WinControls.ChildListviewer addressList;
		private QuadSolutions.Minerva.WinControls.ChildListviewer personList;
		private QuadSolutions.Minerva.WinControls.ChildListviewer activityList;
		private QuadSolutions.Minerva.WinControls.ChildListviewer documentList;
		private QuadSolutions.Minerva.WinControls.AccountingViewer accountingViewer;
		private QuadSolutions.Minerva.WinControls.CommercialViewer commercialViewer;

		private Person person=null;
		private Address personAddress=null;
		private Phone personPhone=null;
		private AddressForm addressForm;
		private bool isnewrecord;
		private bool dataLoaded=false;
		private string userID="";

		private Accounting accounting=null;
		private QuadSolutions.Minerva.WinControls.PersonViewer personViewer1;
		private Notes notes=null;
		
		public CompanyControl() {
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			detailsTabPanel.SelectedIndex = 0;
			InitializeTabControl(0);
			
			// TODO: Add any initialization after the InitForm call

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		
		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CompanyControl));
			this.detailsTabPanel = new ActiproSoftware.TabStripPanel.FitTabStripPanel();
			this.addressList = new QuadSolutions.Minerva.WinControls.ChildListviewer();
			this.personList = new QuadSolutions.Minerva.WinControls.ChildListviewer();
			this.commercialViewer = new QuadSolutions.Minerva.WinControls.CommercialViewer();
			this.communicationViewer = new QuadSolutions.Minerva.WinControls.CommunicationViewer();
			this.NotesControl = new QuadSolutions.Minerva.WinControls.qsNotes();
			this.accountingViewer = new QuadSolutions.Minerva.WinControls.AccountingViewer();
			this.documentList = new QuadSolutions.Minerva.WinControls.ChildListviewer();
			this.activityList = new QuadSolutions.Minerva.WinControls.ChildListviewer();
			this.personViewer1 = new QuadSolutions.Minerva.WinControls.PersonViewer();
			this.SuspendLayout();
			// 
			// detailsTabPanel
			// 
			this.detailsTabPanel.AccessibleDescription = ((string)(resources.GetObject("detailsTabPanel.AccessibleDescription")));
			this.detailsTabPanel.AccessibleName = ((string)(resources.GetObject("detailsTabPanel.AccessibleName")));
			this.detailsTabPanel.Alignment = ActiproSoftware.TabStripPanel.TabStripPanelAlignment.Top;
			this.detailsTabPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("detailsTabPanel.Anchor")));
			this.detailsTabPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("detailsTabPanel.BackgroundImage")));
			this.detailsTabPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("detailsTabPanel.Dock")));
			this.detailsTabPanel.Enabled = ((bool)(resources.GetObject("detailsTabPanel.Enabled")));
			this.detailsTabPanel.Font = ((System.Drawing.Font)(resources.GetObject("detailsTabPanel.Font")));
			this.detailsTabPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("detailsTabPanel.ImeMode")));
			this.detailsTabPanel.Items.AddRange(new ActiproSoftware.TabStripPanel.TabStripPanelItem[] {
																										  new ActiproSoftware.TabStripPanel.TabStripPanelItem("Addresses", -1, null, this.addressList),
																										  new ActiproSoftware.TabStripPanel.TabStripPanelItem("Persons", -1, null, this.personList),
																										  new ActiproSoftware.TabStripPanel.TabStripPanelItem("Commercial", -1, null, this.commercialViewer),
																										  new ActiproSoftware.TabStripPanel.TabStripPanelItem("Communication", -1, null, this.communicationViewer),
																										  new ActiproSoftware.TabStripPanel.TabStripPanelItem("Notes", -1, null, this.NotesControl),
																										  new ActiproSoftware.TabStripPanel.TabStripPanelItem("Accounting", -1, null, this.accountingViewer),
																										  new ActiproSoftware.TabStripPanel.TabStripPanelItem("Documents", -1, null, this.documentList),
																										  new ActiproSoftware.TabStripPanel.TabStripPanelItem("Activities", -1, null, this.activityList)});
			this.detailsTabPanel.Location = ((System.Drawing.Point)(resources.GetObject("detailsTabPanel.Location")));
			this.detailsTabPanel.Name = "detailsTabPanel";
			this.detailsTabPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("detailsTabPanel.RightToLeft")));
			this.detailsTabPanel.Size = ((System.Drawing.Size)(resources.GetObject("detailsTabPanel.Size")));
			this.detailsTabPanel.TabIndex = ((int)(resources.GetObject("detailsTabPanel.TabIndex")));
			this.detailsTabPanel.Visible = ((bool)(resources.GetObject("detailsTabPanel.Visible")));
			this.detailsTabPanel.AfterSelect += new System.EventHandler(this.detailsTabPanel_AfterSelect);
			// 
			// addressList
			// 
			this.addressList.AccessibleDescription = ((string)(resources.GetObject("addressList.AccessibleDescription")));
			this.addressList.AccessibleName = ((string)(resources.GetObject("addressList.AccessibleName")));
			this.addressList.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("addressList.Anchor")));
			this.addressList.AutoScroll = ((bool)(resources.GetObject("addressList.AutoScroll")));
			this.addressList.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("addressList.AutoScrollMargin")));
			this.addressList.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("addressList.AutoScrollMinSize")));
			this.addressList.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("addressList.BackgroundImage")));
			this.addressList.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("addressList.Dock")));
			this.addressList.Enabled = ((bool)(resources.GetObject("addressList.Enabled")));
			this.addressList.Font = ((System.Drawing.Font)(resources.GetObject("addressList.Font")));
			this.addressList.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("addressList.ImeMode")));
			this.addressList.Location = ((System.Drawing.Point)(resources.GetObject("addressList.Location")));
			this.addressList.Name = "addressList";
			this.addressList.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("addressList.RightToLeft")));
			this.addressList.Size = ((System.Drawing.Size)(resources.GetObject("addressList.Size")));
			this.addressList.TabIndex = ((int)(resources.GetObject("addressList.TabIndex")));
			this.addressList.Visible = ((bool)(resources.GetObject("addressList.Visible")));
			this.addressList.GridDoubleClick += new QuadSolutions.Minerva.WinControls.ChildListviewer.SelectedRow(this.addressList_GridDoubleClick);
			// 
			// personList
			// 
			this.personList.AccessibleDescription = ((string)(resources.GetObject("personList.AccessibleDescription")));
			this.personList.AccessibleName = ((string)(resources.GetObject("personList.AccessibleName")));
			this.personList.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("personList.Anchor")));
			this.personList.AutoScroll = ((bool)(resources.GetObject("personList.AutoScroll")));
			this.personList.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("personList.AutoScrollMargin")));
			this.personList.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("personList.AutoScrollMinSize")));
			this.personList.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("personList.BackgroundImage")));
			this.personList.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("personList.Dock")));
			this.personList.Enabled = ((bool)(resources.GetObject("personList.Enabled")));
			this.personList.Font = ((System.Drawing.Font)(resources.GetObject("personList.Font")));
			this.personList.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("personList.ImeMode")));
			this.personList.Location = ((System.Drawing.Point)(resources.GetObject("personList.Location")));
			this.personList.Name = "personList";
			this.personList.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("personList.RightToLeft")));
			this.personList.Size = ((System.Drawing.Size)(resources.GetObject("personList.Size")));
			this.personList.TabIndex = ((int)(resources.GetObject("personList.TabIndex")));
			this.personList.Visible = ((bool)(resources.GetObject("personList.Visible")));
			// 
			// commercialViewer
			// 
			this.commercialViewer.AccessibleDescription = ((string)(resources.GetObject("commercialViewer.AccessibleDescription")));
			this.commercialViewer.AccessibleName = ((string)(resources.GetObject("commercialViewer.AccessibleName")));
			this.commercialViewer.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("commercialViewer.Anchor")));
			this.commercialViewer.AutoScroll = ((bool)(resources.GetObject("commercialViewer.AutoScroll")));
			this.commercialViewer.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("commercialViewer.AutoScrollMargin")));
			this.commercialViewer.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("commercialViewer.AutoScrollMinSize")));
			this.commercialViewer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("commercialViewer.BackgroundImage")));
			this.commercialViewer.CommercialObject = null;
			this.commercialViewer.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("commercialViewer.Dock")));
			this.commercialViewer.Enabled = ((bool)(resources.GetObject("commercialViewer.Enabled")));
			this.commercialViewer.Font = ((System.Drawing.Font)(resources.GetObject("commercialViewer.Font")));
			this.commercialViewer.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("commercialViewer.ImeMode")));
			this.commercialViewer.Location = ((System.Drawing.Point)(resources.GetObject("commercialViewer.Location")));
			this.commercialViewer.Name = "commercialViewer";
			this.commercialViewer.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("commercialViewer.RightToLeft")));
			this.commercialViewer.Size = ((System.Drawing.Size)(resources.GetObject("commercialViewer.Size")));
			this.commercialViewer.TabIndex = ((int)(resources.GetObject("commercialViewer.TabIndex")));
			this.commercialViewer.Visible = ((bool)(resources.GetObject("commercialViewer.Visible")));
			// 
			// communicationViewer
			// 
			this.communicationViewer.AccessibleDescription = ((string)(resources.GetObject("communicationViewer.AccessibleDescription")));
			this.communicationViewer.AccessibleName = ((string)(resources.GetObject("communicationViewer.AccessibleName")));
			this.communicationViewer.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("communicationViewer.Anchor")));
			this.communicationViewer.AutoScroll = ((bool)(resources.GetObject("communicationViewer.AutoScroll")));
			this.communicationViewer.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("communicationViewer.AutoScrollMargin")));
			this.communicationViewer.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("communicationViewer.AutoScrollMinSize")));
			this.communicationViewer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("communicationViewer.BackgroundImage")));
			this.communicationViewer.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("communicationViewer.Dock")));
			this.communicationViewer.EMail = "";
			this.communicationViewer.Enabled = ((bool)(resources.GetObject("communicationViewer.Enabled")));
			this.communicationViewer.Font = ((System.Drawing.Font)(resources.GetObject("communicationViewer.Font")));
			this.communicationViewer.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("communicationViewer.ImeMode")));
			this.communicationViewer.Location = ((System.Drawing.Point)(resources.GetObject("communicationViewer.Location")));
			this.communicationViewer.Name = "communicationViewer";
			this.communicationViewer.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("communicationViewer.RightToLeft")));
			this.communicationViewer.Size = ((System.Drawing.Size)(resources.GetObject("communicationViewer.Size")));
			this.communicationViewer.TabIndex = ((int)(resources.GetObject("communicationViewer.TabIndex")));
			this.communicationViewer.URL = "";
			this.communicationViewer.Visible = ((bool)(resources.GetObject("communicationViewer.Visible")));
			// 
			// NotesControl
			// 
			this.NotesControl.AccessibleDescription = ((string)(resources.GetObject("NotesControl.AccessibleDescription")));
			this.NotesControl.AccessibleName = ((string)(resources.GetObject("NotesControl.AccessibleName")));
			this.NotesControl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("NotesControl.Anchor")));
			this.NotesControl.AutoScroll = ((bool)(resources.GetObject("NotesControl.AutoScroll")));
			this.NotesControl.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("NotesControl.AutoScrollMargin")));
			this.NotesControl.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("NotesControl.AutoScrollMinSize")));
			this.NotesControl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("NotesControl.BackgroundImage")));
			this.NotesControl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("NotesControl.Dock")));
			this.NotesControl.Enabled = ((bool)(resources.GetObject("NotesControl.Enabled")));
			this.NotesControl.Font = ((System.Drawing.Font)(resources.GetObject("NotesControl.Font")));
			this.NotesControl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("NotesControl.ImeMode")));
			this.NotesControl.IsNew = false;
			this.NotesControl.Location = ((System.Drawing.Point)(resources.GetObject("NotesControl.Location")));
			this.NotesControl.Name = "NotesControl";
			this.NotesControl.NotesObject = null;
			this.NotesControl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("NotesControl.RightToLeft")));
			this.NotesControl.RowID = new System.Guid("00000000-0000-0000-0000-000000000000");
			this.NotesControl.Size = ((System.Drawing.Size)(resources.GetObject("NotesControl.Size")));
			this.NotesControl.TabIndex = ((int)(resources.GetObject("NotesControl.TabIndex")));
			this.NotesControl.UserID = null;
			this.NotesControl.Visible = ((bool)(resources.GetObject("NotesControl.Visible")));
			// 
			// accountingViewer
			// 
			this.accountingViewer.AccessibleDescription = ((string)(resources.GetObject("accountingViewer.AccessibleDescription")));
			this.accountingViewer.AccessibleName = ((string)(resources.GetObject("accountingViewer.AccessibleName")));
			this.accountingViewer.AccountingObject = null;
			this.accountingViewer.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("accountingViewer.Anchor")));
			this.accountingViewer.AutoScroll = ((bool)(resources.GetObject("accountingViewer.AutoScroll")));
			this.accountingViewer.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("accountingViewer.AutoScrollMargin")));
			this.accountingViewer.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("accountingViewer.AutoScrollMinSize")));
			this.accountingViewer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("accountingViewer.BackgroundImage")));
			this.accountingViewer.BankObject = null;
			this.accountingViewer.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("accountingViewer.Dock")));
			this.accountingViewer.Enabled = ((bool)(resources.GetObject("accountingViewer.Enabled")));
			this.accountingViewer.Font = ((System.Drawing.Font)(resources.GetObject("accountingViewer.Font")));
			this.accountingViewer.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("accountingViewer.ImeMode")));
			this.accountingViewer.IsNew = false;
			this.accountingViewer.Location = ((System.Drawing.Point)(resources.GetObject("accountingViewer.Location")));
			this.accountingViewer.Name = "accountingViewer";
			this.accountingViewer.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("accountingViewer.RightToLeft")));
			this.accountingViewer.Size = ((System.Drawing.Size)(resources.GetObject("accountingViewer.Size")));
			this.accountingViewer.TabIndex = ((int)(resources.GetObject("accountingViewer.TabIndex")));
			this.accountingViewer.Visible = ((bool)(resources.GetObject("accountingViewer.Visible")));
			// 
			// documentList
			// 
			this.documentList.AccessibleDescription = ((string)(resources.GetObject("documentList.AccessibleDescription")));
			this.documentList.AccessibleName = ((string)(resources.GetObject("documentList.AccessibleName")));
			this.documentList.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("documentList.Anchor")));
			this.documentList.AutoScroll = ((bool)(resources.GetObject("documentList.AutoScroll")));
			this.documentList.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("documentList.AutoScrollMargin")));
			this.documentList.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("documentList.AutoScrollMinSize")));
			this.documentList.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("documentList.BackgroundImage")));
			this.documentList.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("documentList.Dock")));
			this.documentList.Enabled = ((bool)(resources.GetObject("documentList.Enabled")));
			this.documentList.Font = ((System.Drawing.Font)(resources.GetObject("documentList.Font")));
			this.documentList.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("documentList.ImeMode")));
			this.documentList.Location = ((System.Drawing.Point)(resources.GetObject("documentList.Location")));
			this.documentList.Name = "documentList";
			this.documentList.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("documentList.RightToLeft")));
			this.documentList.Size = ((System.Drawing.Size)(resources.GetObject("documentList.Size")));
			this.documentList.TabIndex = ((int)(resources.GetObject("documentList.TabIndex")));
			this.documentList.Visible = ((bool)(resources.GetObject("documentList.Visible")));
			// 
			// activityList
			// 
			this.activityList.AccessibleDescription = ((string)(resources.GetObject("activityList.AccessibleDescription")));
			this.activityList.AccessibleName = ((string)(resources.GetObject("activityList.AccessibleName")));
			this.activityList.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("activityList.Anchor")));
			this.activityList.AutoScroll = ((bool)(resources.GetObject("activityList.AutoScroll")));
			this.activityList.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("activityList.AutoScrollMargin")));
			this.activityList.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("activityList.AutoScrollMinSize")));
			this.activityList.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("activityList.BackgroundImage")));
			this.activityList.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("activityList.Dock")));
			this.activityList.Enabled = ((bool)(resources.GetObject("activityList.Enabled")));
			this.activityList.Font = ((System.Drawing.Font)(resources.GetObject("activityList.Font")));
			this.activityList.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("activityList.ImeMode")));
			this.activityList.Location = ((System.Drawing.Point)(resources.GetObject("activityList.Location")));
			this.activityList.Name = "activityList";
			this.activityList.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("activityList.RightToLeft")));
			this.activityList.Size = ((System.Drawing.Size)(resources.GetObject("activityList.Size")));
			this.activityList.TabIndex = ((int)(resources.GetObject("activityList.TabIndex")));
			this.activityList.Visible = ((bool)(resources.GetObject("activityList.Visible")));
			// 
			// personViewer1
			// 
			this.personViewer1.AccessibleDescription = ((string)(resources.GetObject("personViewer1.AccessibleDescription")));
			this.personViewer1.AccessibleName = ((string)(resources.GetObject("personViewer1.AccessibleName")));
			this.personViewer1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("personViewer1.Anchor")));
			this.personViewer1.AutoScroll = ((bool)(resources.GetObject("personViewer1.AutoScroll")));
			this.personViewer1.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("personViewer1.AutoScrollMargin")));
			this.personViewer1.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("personViewer1.AutoScrollMinSize")));
			this.personViewer1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("personViewer1.BackgroundImage")));
			this.personViewer1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("personViewer1.Dock")));
			this.personViewer1.EMail = "";
			this.personViewer1.Enabled = ((bool)(resources.GetObject("personViewer1.Enabled")));
			this.personViewer1.Font = ((System.Drawing.Font)(resources.GetObject("personViewer1.Font")));
			this.personViewer1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("personViewer1.ImeMode")));
			this.personViewer1.Location = ((System.Drawing.Point)(resources.GetObject("personViewer1.Location")));
			this.personViewer1.Name = "personViewer1";
			this.personViewer1.PersonsObject = null;
			this.personViewer1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("personViewer1.RightToLeft")));
			this.personViewer1.Size = ((System.Drawing.Size)(resources.GetObject("personViewer1.Size")));
			this.personViewer1.TabIndex = ((int)(resources.GetObject("personViewer1.TabIndex")));
			this.personViewer1.URL = "";
			this.personViewer1.Visible = ((bool)(resources.GetObject("personViewer1.Visible")));
			// 
			// CompanyControl
			// 
			this.AccessibleDescription = ((string)(resources.GetObject("$this.AccessibleDescription")));
			this.AccessibleName = ((string)(resources.GetObject("$this.AccessibleName")));
			this.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("$this.Anchor")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.personViewer1,
																		  this.addressList,
																		  this.activityList,
																		  this.personList,
																		  this.commercialViewer,
																		  this.communicationViewer,
																		  this.NotesControl,
																		  this.accountingViewer,
																		  this.documentList,
																		  this.detailsTabPanel});
			this.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("$this.Dock")));
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "CompanyControl";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.TabIndex = ((int)(resources.GetObject("$this.TabIndex")));
			this.Visible = ((bool)(resources.GetObject("$this.Visible")));
			this.ResumeLayout(false);

		}
		#endregion

		#region Function

		private void detailsTabPanel_AfterSelect(object sender, System.EventArgs e) {
			InitializeTabControl(detailsTabPanel.SelectedIndex);
			switch(detailsTabPanel.SelectedIndex) {
				case 0: //address
					break;
				case 1: //company
					break;
				case 2: //communication
					break;
				case 3: //notes
					break;
				case 4: //accounting
					break;
				case 5: //commercial
					break;
				case 6: //activities
					break;
				case 7: //documents
					break;
				default:
					break;
			}
		}

		private void InitializeTabControl(int Index){
			Size detailsSize=new Size(150,500);
			detailsSize.Width+=375;
			detailsSize.Height-=230;
			detailsTabPanel.Items[Index].BoundControl.Location=new Point(4,150);
			detailsTabPanel.Items[Index].BoundControl.Size=detailsSize;

		}
		
		public void LoadData(ref Companies editcompany,string currentUser) {
			company=editcompany;
			this.userID=currentUser;
			companyAddress=company.CompanyAddress;
			Guid linkid=company.RowID;
			companyPhone=company.CompanyPhone;
			this.companyViewer.LoadData(ref company);
			this.companyViewer.LoadDefaultAddress(ref companyAddress);
			this.communicationViewer.LoadData();
			this.addressList.LoadData(linkid,WinControls.ChildListviewer.Tables.Address);
			this.personList.LoadData(linkid,WinControls.ChildListviewer.Tables.Persons);
			this.accountingViewer.BankList.LoadData(linkid,WinControls.ChildListviewer.Tables.Bank);
		}
		public void LoadData(Guid rowid,string currentUser) {
			company=new Companies();
			this.userID=currentUser;
			if(company.SelectValue(rowid)){
				
				companyAddress=company.CompanyAddress;
				//Guid linkid=company.RowID;
				this.companyViewer.LoadData(ref company);
				this.companyViewer.LoadDefaultAddress(ref companyAddress);
				this.addressList.LoadData(rowid,WinControls.ChildListviewer.Tables.Address);
				this.personList.LoadData(rowid,WinControls.ChildListviewer.Tables.Persons);
				this.accountingViewer.BankList.LoadData(rowid,WinControls.ChildListviewer.Tables.Bank);
				this.accountingViewer.LoadData();
			}
			else {
				MessageBox.Show("Failed at Company Id");
			}
		}

		public void EditSave() {
			DataAccessLayer.DataBaseLayer dataaccess=null;
			try{
				if (isnewrecord) {
					if(companyAddress==null){
						//this is the object that hold all addresses of the company
						companyAddress=new Address();
						companyAddress.LoadData();
					}
					this.companyViewer.CompaniesObject.Email=this.communicationViewer.EMail;
					this.companyViewer.CompaniesObject.URL=this.communicationViewer.URL;
					dataaccess=this.companyViewer.EditSave("sa",ref companyAddress);
					this.commercialViewer.CommercialObject.LinkID=this.companyViewer.RowID;
					this.commercialViewer.EditSave(ref dataaccess);
					this.companyAddress.AssignLinkId(this.companyViewer.RowID);
					this.companyAddress.Insert(ref dataaccess);
					this.notes.AssignLinkId(this.companyViewer.RowID);
					this.notes.Insert(ref dataaccess);
					this.accountingViewer.AccountingObject.LinkID=this.companyViewer.RowID;
					this.accountingViewer.EditSave(ref dataaccess);
					dataaccess.CommitTransaction();
					this.EnableControl(false);
				}else {//edit mode
				}
			}
			catch(Exception e) {
				dataaccess.RollbackTransaction();
			}
		}

		public void EditStart() {
			this.EnableControl(true);
			ClearAll();
			LoadAll();
		}

		public void EditCancel() {
			this.EnableControl(false);
		}

		public void EnableControl(bool enableIt) {
			this.companyViewer.EditStart();
			this.communicationViewer.EditStart();
			this.accountingViewer.EditStart();
			this.commercialViewer.EditStart();
			
		}
		public void ClearAll(){
			this.CompanyDetails.ClearAll();
			this.accountingViewer.ClearAll();
			this.addressList.ClearAll();
		}
		public void LoadAll(){
			isnewrecord=true;
			accounting=new Accounting();
			notes=new Notes();


			companyAddress=new Address();
			this.companyAddress.LoadData();
			this.commercialViewer.LoadData();
			this.NotesControl.LoadData("sa",ref notes,isnewrecord);
			this.accountingViewer.LoadData(ref accounting,isnewrecord);


		}
		#endregion

		#region ControlsProperties
		
		public WinControls.ChildListviewer Addresses{
			get{return addressList;}
		}
		public WinControls.ChildListviewer Persons{
			get{return personList;}
		}
		public WinControls.ChildListviewer	Documents{
			get{return documentList;}
		}
		public WinControls.ChildListviewer Activities{
			get{return activityList;}
		}
		public WinControls.CompanyViewer CompanyDetails{
			get{return this.companyViewer;}
		}


		#endregion

		private void personList_Load(object sender, System.EventArgs e) {
			//
		}


		private void addressList_GridDoubleClick(object sender, System.EventArgs e) {
			if (this.addressList.GridControl.SelectedItems.Count==0) {return;}
			if(this.addressList.GridControl.SelectedItems[0].GetRow().Position==-1) {
				addressForm=new AddressForm();
				addressForm.EditStart(ref this.companyAddress,true);
				addressForm.Show();
				

			}
			else {
				Guid selectedID=(Guid)this.addressList.GridControl.SelectedItems[0].GetRow().Cells["RowID"].Value;
				if (this.addressList.GridControl.SelectedItems.Count>0) {
					if (company.CompanyAddress.AddressTable.Rows.Count>0) {
						if(company.CompanyAddress.FindAddressinTable(selectedID)){
							Address editaddress=company.CompanyAddress;
							addressForm=new AddressForm();
							addressForm.EditStart(ref editaddress,false);
							addressForm.Show();
						}
					}
				}
			}
		}

		#region Properties
		public bool IsNewRecord{
			get{return isnewrecord;}
			set{isnewrecord=value;}

		}
		public bool DataLoaded {
			get {return dataLoaded;}
		}
		#endregion
	}
}
