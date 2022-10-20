using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Lib;

namespace intellaConsole
{
    public partial class SpeedDialEntryEditor : Form
    {
        private QueryResultSetRecord m_speedDialEntry;

        public delegate void SaveButtonCallbackFunction(QueryResultSetRecord speedDialEntry, string sqlCmd );
        private SaveButtonCallbackFunction m_saveButtonCallback;

        public SpeedDialEntryEditor( QueryResultSetRecord speedDialEntry, SaveButtonCallbackFunction saveBtnCallbackFn )
        {
            InitializeComponent();
            
            this.cmpTextBoxFirstName.Text   = speedDialEntry["first_name"];
            this.cmpTextBoxLastName.Text    = speedDialEntry["last_name"];
            this.cmpTextBoxPhoneNumber.Text = speedDialEntry["phone_number"];
            this.cmpTextBoxEmail.Text       = speedDialEntry["email"];
         // this.cmpAreaTextBox.Text        = speedDialEntry["area"];
         // this.cmpDepartmentTextBox.Text  = speedDialEntry["department"];

            this.m_speedDialEntry     = speedDialEntry;
            this.m_saveButtonCallback = saveBtnCallbackFn;
            this.StartPosition        = FormStartPosition.CenterParent;
        }

        private void cmpButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmpButtonSave_Click(object sender, EventArgs e)
        {
            this.m_speedDialEntry["first_name"]   = this.cmpTextBoxFirstName.Text;
            this.m_speedDialEntry["last_name"]    = this.cmpTextBoxLastName.Text;
            this.m_speedDialEntry["phone_number"] = this.cmpTextBoxPhoneNumber.Text;
            this.m_speedDialEntry["email"]        = this.cmpTextBoxEmail.Text;
         // this.m_speedDialEntry["area"]         = this.cmpAreaTextBox.Text;
         // this.m_speedDialEntry["department"]   = this.cmpDepartmentTextBox.Text;

            // Call callback (which was saved in the form constructor)
            this.m_saveButtonCallback(this.m_speedDialEntry, "update/insert");
            this.Close();
        }

        private void cmpButtonDelete_Click(object sender, EventArgs e)
        {
            // Call callback (which was saved in the form constructor)
            this.m_saveButtonCallback(this.m_speedDialEntry, "delete");
            this.Close();
        }

        private void SpeedDialEntryEditor_Load(object sender, EventArgs e) {
            foreach (Control c in this.Controls) {
                if (c is TextBox) {
                    TextBox t = (TextBox)c;

                    t.KeyPress += new KeyPressEventHandler(delegate(object _sender, KeyPressEventArgs _e) {
                        if (_e.KeyChar == (char)Keys.Enter) {
                            cmpButtonSave_Click(_sender, _e);
                            return;
                        }

                        if (_e.KeyChar == (char)Keys.Escape) {
                            cmpButtonCancel_Click(_sender, _e);
                        }
                    });
                }
            }
        }
    }
}
