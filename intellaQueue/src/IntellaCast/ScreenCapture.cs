using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BytescoutScreenCapturingLib;
using System.Threading;
using System.Windows.Forms;
using Lib;

namespace IntellaCast
{
    public class ScreenCaptureResult
    {
        public Capturer Capturer;
        public string CaptureFile;
        public DateTime CaptureStart;
    }

    public static class ScreenCapture
    {
        public delegate void ScreenCaptureCompleteCallback(ScreenCaptureResult sender);

        private static Boolean m_captureRunning = false;
        private static ScreenCaptureCompleteCallback m_captureCompleteCallback;
        private static QD.LoggerCallbackMsg m_logger = QD_Internal; // QuickDebug logger

        public static void SetLoggerCallBack(QD.LoggerCallbackMsg qd)
        {
            m_logger = qd;
        }

        private static void QD (string msg, params string[] argsRest) {
            m_logger.Invoke(msg, argsRest);
        }

        private static string QD_Internal(string msg, params string[] argsRest)
        {
            string log_line = String.Format(msg, argsRest);

            Console.WriteLine(log_line);

            return log_line;
        }

        public static Boolean IsRunning()
        {
            return m_captureRunning;
        }

        public static Thread StartCapture(string outputFilename, ScreenCaptureCompleteCallback captureCompleteCallback)
        {
            return StartCapture(outputFilename, captureCompleteCallback, null);
        }

        public static Thread StartCapture(string outputFileName, ScreenCaptureCompleteCallback captureCompleteCallback, List<string> textOverlay)
        {
            m_captureCompleteCallback = captureCompleteCallback;

            Thread t = new Thread(delegate () {
                StartCaptureInternal(textOverlay, outputFileName);
            });

            t.Start();

            return t;
        }

        private static void StartCaptureInternal(List<string> TextOverlay, string outputFilename)
        {
            if (m_captureRunning == true) {
                QD("IntellaCast - Capture already in progress.");
                return;
            }

            // Set our running capture monitoring global to true
            m_captureRunning = true;

            // I am using the primary screen width/height here.
            // We might want to lower the resolution to decrease file size?
            int primaryScreenWidth  = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int primaryScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            Capturer capturer = new Capturer();

            ScreenCaptureResult result = new ScreenCaptureResult();
            result.Capturer    = capturer;
            result.CaptureFile = outputFilename;

            capturer.CapturingType = CaptureAreaType.catScreen;

            // Need to change this to the path/filename it will be stored as
            capturer.OutputFileName = outputFilename;

            capturer.OutputWidth = primaryScreenWidth;
            capturer.OutputHeight = primaryScreenHeight;

            // Let's add a running timestamp
            capturer.OverlayingRedTextCaption = "Recording: {RUNNINGMIN}:{RUNNINGSEC}:{RUNNINGMSEC} on {CURRENTYEAR}-{CURRENTMONTH}-{CURRENTDAY} at {CURRENTHOUR}:{CURRENTMIN}:{CURRENTSEC}:{CURRENTMSEC}";

            if (TextOverlay != null)
            {
                // We have some text to add to the recording
                // Going to shoot for top left corner, see how's it goeses
                int x = 50;
                int y = 50;

                foreach (string s in TextOverlay)
                {
                    capturer.AddTextOverlay(x, y, s, "MV Boli", 20, true, false, false, 0xffffff);
                    y += 50;
                }
            }

            QD("IntellaCastInternal - Capture Start: " + outputFilename);
            result.CaptureStart = DateTime.UtcNow;
            capturer.Run();

            while (m_captureRunning) 
            {
                Thread.Sleep(1000);
            }

            capturer.Stop();

            QD("IntellaCastInternal - Capture Complete: " + outputFilename);
            m_captureCompleteCallback.Invoke(result);
        }

        public static void StopCapture()
        {
            if (m_captureRunning == false)
            {
                QD("IntellaCastInternal - Capture already stopped");
                return;
            }

            m_captureRunning = false;
        }
    }
}