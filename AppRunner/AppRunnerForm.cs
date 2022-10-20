using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppRunner
{
    public partial class AppRunnerForm : Form
    {
        public AppRunnerForm()
        {
            InitializeComponent();
        }
                
        private void AppRunnerForm_Load(object sender, EventArgs e)
        {
            this.Hide();
            MessageBox.Show("Run run run");
            Application.Exit();
        }
    }
}
