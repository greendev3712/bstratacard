using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace intellaConsole {
    public partial class LogWindow : Form {
        public LogWindow() {
            InitializeComponent();
        }

        public LogWindow(string message) {
            InitializeComponent();
            this.richTextBoxLog.Text = message + "\n";
        }

        public void Write(string message) {
            this.richTextBoxLog.Text += message + "\n";
        }

        private void buttonClose_Click(object sender, EventArgs e) {
            this.Hide();
        }

        protected override void OnClosing(CancelEventArgs e) {
            e.Cancel = true;
            this.Hide();
        }
    }
}
