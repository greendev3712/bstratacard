///////////////////////////////////////////////////////////////////////////////
//
// 1) Wait for the IntellaQueue Toolbar to be updated
// 2) Once it's updated, start it back up again
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Lib;

namespace IntellaQueueRestarter
{
    public partial class IntellaQueueRestarterForm : Form
    {
        private wyDay.Controls.AutomaticUpdaterBackend cmpAutomaticWY_Updater;
        private bool m_wyUpdateCheckManual                  = true; // First one is a yes.. Output the results to the statusbar if we're up to date
        private TimeSpan m_wyUpdateCheckInterval            = TimeSpan.FromSeconds(15);
        private DateTime m_wyUpdateLastCheck                = DateTime.Now;
        private System.Windows.Forms.Timer m_wyUpdate_Timer = new System.Windows.Forms.Timer();

        private DataDumperForm DDF = new DataDumperForm();

        private bool checkDone = false;

        public IntellaQueueRestarterForm() {
            InitializeComponent();
            InitializeUpdateChecks();
        }
        
         private void InitializeUpdateChecks() {
            // 
            // cmpAutomaticWY_Updater
            // 
            this.cmpAutomaticWY_Updater = new wyDay.Controls.AutomaticUpdaterBackend();
            this.cmpAutomaticWY_Updater.GUID = "a4e05f5a-836b-41a5-97eb-d83e977474e4";
            this.cmpAutomaticWY_Updater.wyUpdateCommandline = "/skipinfo";

            // The background updater service (IntellaUpdate) does the updating
            this.cmpAutomaticWY_Updater.UpdateType = wyDay.Controls.UpdateType.OnlyCheck;

            this.cmpAutomaticWY_Updater.UpdateAvailable += delegate (System.Object sender, System.EventArgs e) {
                wyDay.Controls.AutomaticUpdaterBackend au = (wyDay.Controls.AutomaticUpdaterBackend) sender;

                string current_version = "<Unknown>";
                try { current_version = System.IO.File.ReadAllText(@"version.txt"); } catch (Exception ex) { ex.ToString(); }

                DDF.D("[UpdateCheck] Update Available -- Software update available. Restart to apply update. Current Version: {0} New Version: {1}", current_version, au.Version);
                checkDone = true;
            };

            this.cmpAutomaticWY_Updater.CheckingFailed += delegate (System.Object sender, wyDay.Controls.FailArgs f) {
                DDF.D("[UpdateCheck] Update Check Failed --  {0}", f.ErrorMessage);
                checkDone = true;
            };

            this.cmpAutomaticWY_Updater.BeforeChecking += delegate (System.Object sender, wyDay.Controls.BeforeArgs b) {
                DDF.D("[UpdateCheck] Update Check Start");
            };

            this.cmpAutomaticWY_Updater.UpdateFailed += delegate (System.Object sender, wyDay.Controls.FailArgs f) {
                DDF.D("[UpdateCheck] Update Failed -- {0}", f.ErrorMessage);
                checkDone = true;
            };

            this.cmpAutomaticWY_Updater.UpToDate += delegate (object sender, wyDay.Controls.SuccessArgs e) {
                // wyUpdate variable: e.Version doesn't return the correct version... it's empty!
                string current_version = "<Unknown>";
                try { current_version = System.IO.File.ReadAllText(@"version.txt"); } catch (Exception ex) { ex.ToString(); }

                DDF.D("[UpdateCheck] Up to date -- Up to date at version: {0}", current_version);
                DDF.D("[UpdateCheck] Starting up IntellaQueueToolbar");

                if (DDF.Visible) {
                    DDF.D("[UpdateCheck] Debug Enabled... Starting up IntellaQueueToolbar on a delay (10 seconds)");
                    Thread.Sleep((int) TimeSpan.FromSeconds(10).TotalMilliseconds);
                }

                //string path_cwd = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                //Process.Start(path_cwd + "/intellaQueue.exe");

                ApplicationExit();
            };

            // automaticUpdater1.WaitBeforeCheckSecs
            this.cmpAutomaticWY_Updater.Initialize();
            this.cmpAutomaticWY_Updater.AppLoaded();

            // Check right away
            CheckForUpdatesManual();

            // And then recheck frequently
            this.m_wyUpdate_Timer.Interval = (int) this.m_wyUpdateCheckInterval.TotalMilliseconds;
            this.m_wyUpdate_Timer.Tick    += delegate (object sender, EventArgs e) {
                CheckForUpdatesAuto();
            };

            this.m_wyUpdate_Timer.Start();
        }

        private void CheckForUpdatesManual() {
            this.m_wyUpdate_Timer.Stop();

            DDF.D("[CheckForUpdatesManual] Checking for updates... (Manual)");

            this.m_wyUpdateCheckManual = true;
            this.cmpAutomaticWY_Updater.ForceCheckForUpdate(true);
            this.m_wyUpdateLastCheck = DateTime.Now;

            this.m_wyUpdate_Timer.Start();
        }

        private void CheckForUpdatesAuto() {
            this.m_wyUpdate_Timer.Stop();

            checkDone = false;

            // InitializeUpdateChecks();

            DDF.D("[CheckForUpdatesManual] Checking for updates... (Auto)");

            this.m_wyUpdateCheckManual = false;
            this.cmpAutomaticWY_Updater.ForceCheckForUpdate(true);
            this.m_wyUpdateLastCheck = DateTime.Now;

            while (!this.checkDone) {
                Thread.Sleep(100);
            }

            Thread.Sleep(1000);

            // this.cmpAutomaticWY_Updater.Dispose();

            this.m_wyUpdate_Timer.Start();
        }

        private void cmpSysTrayNotifyIcon_MouseClick(object sender, MouseEventArgs e) {
            // SysTray Icon

            if (e.Button == MouseButtons.Right) {
                if (!cmpSysTrayContextMenuStrip.Visible) {
                    cmpSysTrayContextMenuStrip.Show();
                    cmpSysTrayContextMenuStrip.Location = Cursor.Position;
                }
                else {
                    cmpSysTrayContextMenuStrip.Hide();
                }

                return;
            }

            // Everything else
            cmpSysTrayContextMenuStrip.Hide();
        }

        private void cmpSysTrayContextMenuStrip_Click(object sender, EventArgs e) {
            ApplicationExit();
        }

        private void ApplicationExit() {
            this.cmpSysTrayNotifyIcon.Dispose();
            Application.Exit();
        }

        private void IntellaQueueRestarterForm_Load(object sender, EventArgs e) {
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e) {
            DDF.Show();
        }

        private void IntellaQueueRestarterForm_Shown(object sender, EventArgs e) {
            this.Hide();
        }
    }
}
