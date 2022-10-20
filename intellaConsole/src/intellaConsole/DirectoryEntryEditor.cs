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
    public partial class DirectoryEntryEditor : Form
    {
        private QueryResultSetRecord m_directoryEntry;

        public delegate void SaveButtonCallbackFunction(QueryResultSetRecord directoryEntry, string sqlCmd );
        private SaveButtonCallbackFunction m_saveButtonCallback;

        public DirectoryEntryEditor(QueryResultSetRecord directoryEntry, SaveButtonCallbackFunction saveBtnCallbackFn)
        {
            InitializeComponent();

            this.cmpTextBoxFirstName.Text   = directoryEntry["first_name"]   as string;
            this.cmpTextBoxLastName.Text    = directoryEntry["last_name"]    as string;
            this.cmpTextBoxPhoneNumber.Text = directoryEntry["phone_number"] as string;
            this.cmpTextBoxEmail.Text       = directoryEntry["email"]        as string;
         // this.cmpAreaTextBox.Text        = directoryEntry["area"]         as string;
            this.cmpTextBoxDepartment.Text  = directoryEntry["department"]   as string;

            this.m_directoryEntry     = directoryEntry;
            this.m_saveButtonCallback = saveBtnCallbackFn;
            this.StartPosition        = FormStartPosition.CenterParent;
        }

        private void cmpButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmpButtonSave_Click(object sender, EventArgs e)
        {
            this.m_directoryEntry["first_name"]   = this.cmpTextBoxFirstName.Text;
            this.m_directoryEntry["last_name"]    = this.cmpTextBoxLastName.Text;
            this.m_directoryEntry["phone_number"] = this.cmpTextBoxPhoneNumber.Text;
            this.m_directoryEntry["email"]        = this.cmpTextBoxEmail.Text;
         // this.m_directoryEntry["area"]         = this.cmpAreaTextBox.Text;
            this.m_directoryEntry["department"]   = this.cmpTextBoxDepartment.Text;

            // Call callback (which was saved in the form constructor)
            this.m_saveButtonCallback(this.m_directoryEntry, "update/insert");
            this.Close();
        }

        private void cmpButtonDelete_Click(object sender, EventArgs e)
        {
            // Call callback (which was saved in the form constructor)
            this.m_saveButtonCallback(this.m_directoryEntry, "delete");
            this.Close();
        }

        private void DirectoryEntryEditor_Load(object sender, EventArgs e) {
            foreach (Control c in this.Controls) {
                if (c is TextBox) {
                    TextBox t = (TextBox) c;

                    t.KeyPress += new KeyPressEventHandler(delegate(object _sender, KeyPressEventArgs _e) {
                        if (_e.KeyChar == (char) Keys.Enter) {
                            cmpButtonSave_Click(_sender, _e);
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
