﻿using Microsoft.VisualBasic;
using QueueLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace intellaQueue
{
    public partial class IntellaQueueForm : System.Windows.Forms.Form
    {
        private Timer m_cmpApplicationRestartCheckTimer;
        private bool m_canWeUpdateAndRestart = false; // During-Call will set this to false

        private void InitializeUpdateChecks() {
            // 
            // cmpAutomaticWY_Updater
            // 
            this.cmpAutomaticWY_Updater                     = new wyDay.Controls.AutomaticUpdaterBackend();
            this.cmpAutomaticWY_Updater.GUID                = "a4e05f5a-836b-41a5-97eb-d83e977474e4";
            this.cmpAutomaticWY_Updater.wyUpdateCommandline = "/skipinfo";

            // The background updater service (IntellaUpdate) does the updating
            this.cmpAutomaticWY_Updater.UpdateType = wyDay.Controls.UpdateType.OnlyCheck;

            this.cmpAutomaticWY_Updater.UpdateAvailable += delegate (System.Object sender, System.EventArgs e) {
                string current_version = "<Unknown>";
                try { current_version = System.IO.File.ReadAllText(@"version.txt"); } catch (Exception ex) { ex.ToString(); }

                wyDay.Controls.AutomaticUpdaterBackend au = (wyDay.Controls.AutomaticUpdaterBackend) sender;

                MQD("[UpdateCheck] Software update available. Restart to apply update. Current Version: {0}, New Version: {1}", current_version, au.Version);

                // Run this on the GUI thread so we can interact with the GUI
                this.Invoke((MethodInvoker) delegate {
                    this.SetStatusBarMessage(Color.Green, "Software update available. Restart to apply update.");
                });

                // Only restart if we're not in the middle of a something (Like a phone call)
                // Main application (In the data processing: See IntellaQueueForm.Data.cs) will flip the CanWeRestartAndUpdate flag when necessary
                //
                this.m_cmpApplicationRestartCheckTimer          = new System.Windows.Forms.Timer();
                this.m_cmpApplicationRestartCheckTimer.Interval = (int) TimeSpan.FromSeconds(10).TotalMilliseconds;
                this.m_cmpApplicationRestartCheckTimer.Tick    += delegate (object tsender, EventArgs ea) {
                    if (this.CanWeRestartAndUpdate()) {
                        string path_cwd = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                        // Process.Start(path_cwd + "/IntellaQueueRestarter.exe");
                        // ApplicationExit();
                    }
                };

                this.m_cmpApplicationRestartCheckTimer.Start();
            };

            this.cmpAutomaticWY_Updater.CheckingFailed += delegate (System.Object sender, wyDay.Controls.FailArgs f) {
                MQD("[UpdateCheck] !!! Failed: {0}", f.ErrorMessage);
                this.SetStatusBarMessage(Color.Red, "Failed to check for software update.  Check log.");
            };

            this.cmpAutomaticWY_Updater.BeforeChecking += delegate (System.Object sender, wyDay.Controls.BeforeArgs b) {
                MQD("[UpdateCheck] Start");
            };

            this.cmpAutomaticWY_Updater.UpdateFailed += delegate (System.Object sender, wyDay.Controls.FailArgs f) {
                MQD("[UpdateCheck] Failed: {0}", f.ToString());
                this.SetStatusBarMessage(Color.Red, "Update Check Failed!");
            };

            this.cmpAutomaticWY_Updater.UpToDate += delegate (object sender, wyDay.Controls.SuccessArgs e) {
                // wyUpdate variable: e.Version doesn't return the correct version... it's empty!
                string current_version = "<Unknown>";
                try { current_version = System.IO.File.ReadAllText(@"version.txt"); } catch (Exception ex) { ex.ToString(); }

                MQD("[UpdateCheck] Complete.  Up to date at version: {0} {1}", current_version, e.Version);

                // If we're on automatic checks, we only care about errors
                if (this.m_wyUpdateCheckManual) {
                    this.SetStatusBarMessage(Color.Green, "Update Check Complete.  Up to date");
                }
            };

            // automaticUpdater1.WaitBeforeCheckSecs
            this.cmpAutomaticWY_Updater.Initialize();
            this.cmpAutomaticWY_Updater.AppLoaded();

            this.m_wyUpdate_Timer.Interval = (int) this.m_wyUpdateCheckInterval.TotalMilliseconds;
            this.m_wyUpdate_Timer.Tick    += delegate (object sender, EventArgs e) {
                CheckForUpdatesAuto();
            };

            this.m_wyUpdate_Timer.Start();
            CheckForUpdatesManual();
        }

        private void M_cmpAutomaticWY_Updater_BeforeChecking(object sender, wyDay.Controls.BeforeArgs e) {
            throw new NotImplementedException();
        }

        private void CheckForUpdatesManual() {
            bool update_timer_running = this.m_wyUpdate_Timer.Enabled;

            if (update_timer_running) {
                this.m_wyUpdate_Timer.Stop();
            }

            this.m_wyUpdateCheckManual = true;

            MQD("Checking for updates...");
            this.SetStatusBarMessage(Color.White, "Checking for updates...");

            this.cmpAutomaticWY_Updater.ForceCheckForUpdate(true);
            this.m_wyUpdateLastCheck = DateTime.Now;

            if (update_timer_running) {
                this.m_wyUpdate_Timer.Start();
            }
        }

        private void CheckForUpdatesAuto() {
            this.m_wyUpdate_Timer.Stop();

            this.m_wyUpdateCheckManual = false;
            MQD("Checking for updates...");

            this.cmpAutomaticWY_Updater.ForceCheckForUpdate(true);
            this.m_wyUpdateLastCheck = DateTime.Now;

            this.m_wyUpdate_Timer.Start();
        }

        private void CanWeRestartAndUpdateSet(bool val) {
            MQD("CanWeRestartAndUpdateSet: " + val.ToString());

            this.m_canWeUpdateAndRestart = val;
        }

        public bool CanWeRestartAndUpdate() {
            return this.m_canWeUpdateAndRestart;
        }
    }
}