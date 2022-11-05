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
		private void Form_Load()
		{
			string toolbar_version;
			string current_build_version;

			if (Debugger.IsAttached) {
				current_build_version = "Current-Build";
				toolbar_version       = "Current-Dev";

				goto done;
			}

			// If using the Visual Studio "ClickOnce" Installer
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed) {
                toolbar_version = "CO:" + System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
			}
			else {
				// wyUpdate or AutoUpdater.net
                string current_version = "<Unknown>";
                try { current_version = System.IO.File.ReadAllText(@"version.txt"); } catch (Exception ex) { ex.ToString(); }

				toolbar_version = current_version;
			}

			// Stick on specific build version information if we have it
            current_build_version = "QV:" + QueueVersion.GetToolBarBuild();
            try { current_build_version = System.IO.File.ReadAllText(@"version_build.txt"); } catch (Exception ex) { ex.ToString(); }

		done:
			this.Label_Version.Text = "Version: " + toolbar_version + "\nBuild: " + current_build_version;
			this.TopMost = true;

			string year = DateTime.Now.Year.ToString();
			this.Label_Copyright.Text = String.Format(@"Copyright 2008-{0}. All Rights Reserved.
intellaSoft and the intellaSoft logos are
trademarks of intellaSoft. All rights reserved.", year);
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