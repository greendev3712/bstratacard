using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using BytescoutScreenCapturingLib;
using Lib;

namespace IntellaCast
{
    public partial class IntellaCastTestForm : Form
    {
        public IntellaCastTestForm()
        {
            InitializeComponent();

            if (!Debugger.IsAttached)
            {
                automaticUpdater1.UpdateType = wyDay.Controls.UpdateType.Automatic;
            }
        }

        private void bStartRecording_Click(object sender, EventArgs e)
        {
            List<string> textToAdd = new List<string>
            {
                "Testing",
                "Testing2",
                "Testing3"
            };

            try
            {
                ScreenCapture.StartCapture("Test.mp4", delegate(ScreenCaptureResult sc) {
                    Utils.UploadFileToURL(sc.CaptureFile, "http://vbox-markm.intellasoft.local/pbx/UploadFile.fcgi?app=Toolbar&op=screencast/upload&token=abc&call_log_id=1000");
                }, textToAdd);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        private void bStopRecording_Click(object sender, EventArgs e)
        {
            ScreenCapture.StopCapture();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Debugger.IsAttached) { 
                automaticUpdater1.ForceCheckForUpdate();
            }
        }

        private void automaticUpdater1_UpToDate(object sender, wyDay.Controls.SuccessArgs e)
        {
            MessageBox.Show("You're all up to date!");
        }

        private void automaticUpdater1_UpdateAvailable(object sender, EventArgs e)
        {
            MessageBox.Show("There is an update available.");
        }

        private void automaticUpdater1_UpdateSuccessful(object sender, wyDay.Controls.SuccessArgs e)
        {
            MessageBox.Show("Update completed successfully.");
        }

        private void automaticUpdater1_UpdateFailed(object sender, wyDay.Controls.FailArgs e)
        {
            MessageBox.Show(e.ErrorMessage);
        }

        private void automaticUpdater1_CheckingFailed(object sender, wyDay.Controls.FailArgs e)
        {
            MessageBox.Show(e.ErrorMessage);
        }
    }
}
