using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace intellaConsole {
    public partial class ErrorTextBox : Form {
        private string m_errorString;

        public ErrorTextBox() {
            InitializeComponent();
        }

        public ErrorTextBox(string message) {
            InitializeComponent();
        }

        public static void Show(string message) {
            ErrorTextBox t = new ErrorTextBox();
            t.SetErrorMessage(message);
            t.Show();
        }

        public void SetErrorMessage(string message) {
            m_errorString = message;
        }

        private void ErrorTextBox_Load(object sender, EventArgs e) {
            this.richTextBoxErrorMessage.Text = this.m_errorString;
        }

        private void buttonExit_Click(object sender, EventArgs e) {
            Application.Exit();
        }
    }
}
