using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Lib;

namespace intellaConsole
{
    public partial class OperatorLoginWindow : Form
    {
        private static string m_registryParent = "intellaConsole";

        public delegate bool LoginCheckCallbackFunction(OrderedDictionary loginEntry);
        public delegate void LoginSuccessCallbackFunction(OrderedDictionary loginEntry);
        public delegate void LoginFailureCallbackFunction(OrderedDictionary loginEntry);

        private LoginCheckCallbackFunction   m_loginCheckCallback;
        private LoginSuccessCallbackFunction m_loginSuccessCallback;
        private LoginFailureCallbackFunction m_loginFailureCallback;

        private OrderedDictionary m_loginEntry = new OrderedDictionary();
        private bool m_loginSuccessful = false;

        private List<OrderedDictionary> m_queues;

        public OperatorLoginWindow(intellaConsole ic, LoginCheckCallbackFunction loginButtonCallback, LoginSuccessCallbackFunction loginSuccessCallback, LoginFailureCallbackFunction loginFailureCallback)
        {
            string queue = Interaction.GetSetting(m_registryParent, "Config", "OperatorQueue");
            string exten = Interaction.GetSetting(m_registryParent, "Config", "OperatorExten");

            _Construct(ic, loginButtonCallback, loginSuccessCallback, loginFailureCallback, queue, exten);
        }

        public OperatorLoginWindow(intellaConsole ic, LoginCheckCallbackFunction loginButtonCallback, LoginSuccessCallbackFunction loginSuccessCallback, LoginFailureCallbackFunction loginFailureCallback, string queue, string exten) {
            _Construct(ic, loginButtonCallback, loginSuccessCallback, loginFailureCallback, queue, exten);
        }

        private void _Construct(intellaConsole ic, LoginCheckCallbackFunction loginButtonCallback, LoginSuccessCallbackFunction loginSuccessCallback, LoginFailureCallbackFunction loginFailureCallback, string queue, string exten) {
            this.m_loginCheckCallback   = loginButtonCallback;
            this.m_loginSuccessCallback = loginSuccessCallback;
            this.m_loginFailureCallback = loginFailureCallback;

            if ((exten != "") && (queue != "") && this.handleLogin(exten, queue)) {
                return;
            }

            InitializeComponent();

            this.m_queues = ic.getQueues();

            this.cmpTextBoxExtension.Text = exten;
            foreach (OrderedDictionary r in this.m_queues) { this.cmpComboBoxQueue.Items.Add(r["queue_longname"]); }

            this.ShowDialog(ic);
        }

        private bool handleLogin( string exten, string queue ) 
        {
            this.m_loginEntry = new OrderedDictionary();
            this.m_loginEntry["exten"] = exten;
            this.m_loginEntry["queue"] = queue;

            if ( this.m_loginCheckCallback(this.m_loginEntry) ) {
                Interaction.SaveSetting(m_registryParent, "Config", "OperatorExten", exten);
                Interaction.SaveSetting(m_registryParent, "Config", "OperatorQueue", queue);

                this.m_loginSuccessful = true;
                this.m_loginSuccessCallback(this.m_loginEntry);

                return true;
            }

            return false;
        }

        private void cmpButtonLogin_Click(object sender, EventArgs e)
        {
            if (this.cmpComboBoxQueue.SelectedIndex == -1) {
                MessageBox.Show("Invalid queue");
            }

            string queue_name = (string) this.m_queues[this.cmpComboBoxQueue.SelectedIndex]["queue_name"];

            if (this.handleLogin(this.cmpTextBoxExtension.Text, queue_name)) {
                this.Close();
            }
            else {
                MessageBox.Show("Invalid extension");
            }
        }

        private void OperatorLoginWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ( ! this.m_loginSuccessful ) {
                this.m_loginFailureCallback(this.m_loginEntry);
            }
        }

        private void cmpTextBoxExtension_TextChanged(object sender, EventArgs e) {

        }
    }
}
