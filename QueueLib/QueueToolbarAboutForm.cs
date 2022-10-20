using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace QueueLib
{

	public partial class QueueToolbarAboutForm : System.Windows.Forms.Form
	{
        public static string m_deploymentVersion = "CurrentDev";

		private void Form_Load()
		{
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                QueueVersion.m_toolbarVersion = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            else
                QueueVersion.m_toolbarVersion = m_deploymentVersion;

			this.Label_Version.Text = "version " + QueueVersion.m_toolbarVersion + " build " + QueueVersion.GetToolBarBuild();
			this.TopMost = true;
		}

		private void Button_Close_Click(System.Object sender, System.EventArgs e)
		{
			this.Close();
		}
		public QueueToolbarAboutForm()
		{
			InitializeComponent();
		}

		private void QueueToolbarAboutForm_Load(object sender, EventArgs e)
		{
			Form_Load();
		}

        private void Label_Copyright_Click(object sender, EventArgs e)
        {

        }
	}

}