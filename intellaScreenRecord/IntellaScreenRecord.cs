using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

// Dependency: NuGet Install-Package FFMediaToolkit
using FFMediaToolkit;
using FFMediaToolkit.Graphics;
using FFMediaToolkit.Encoding;

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Lib;

namespace IntellaScreenRecord
{
    public enum SystemMetric
    {
        VirtualScreenWidth  = 78, // CXVIRTUALSCREEN 0x0000004E 
        VirtualScreenHeight = 79, // CYVIRTUALSCREEN 0x0000004F 
        SM_CYFULLSCREEN = 17,
        SM_CXFULLSCREEN = 16,
    }

    public class IntellaScreenRecording
    {
        public delegate void ScreenRecordingCompleteCallback(IntellaScreenRecordingResult result);

        /// ///////////////////////////////////////////////////////////////////
        
        // Callbacks
        private QD.QD_LoggerFunction m_logger = null;
        private ScreenRecordingCompleteCallback m_screenRecordingCompleteCallback;

        private int screenWidth, screenHeight;      // Screen size
        private int frameRate = 20;                 // Frame rate of the video

        private string appPath;
        private MediaOutput m_mediaOutput;
        private bool m_currentlyRecording;
        private IntellaScreenRecordingResult m_RecordingResult;

        /// ///////////////////////////////////////////////////////////////////

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(SystemMetric metric);

        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,        
        }

        private float GetScalingFactor()
        {
            Graphics g                = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop            = g.GetHdc();
            int LogicalScreenHeight   = GetDeviceCaps(desktop, (int) DeviceCap.VERTRES);
            int PhysicalScreenHeight  = GetDeviceCaps(desktop, (int) DeviceCap.DESKTOPVERTRES);
            float ScreenScalingFactor = (float) PhysicalScreenHeight / (float) LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }

        public Bitmap TakeScreenShot()
        {
            var sc = GetScalingFactor();

            Bitmap shot = new Bitmap(screenWidth, screenHeight);
            var graphics = Graphics.FromImage(shot);

            graphics.CopyFromScreen(0, 0, 0, 0, shot.Size);                

            return shot;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        public IntellaScreenRecording()
        {
            InitVariables();
        }

        void InitVariables()
        {
            screenWidth  = GetSystemMetrics(SystemMetric.VirtualScreenWidth);
            screenHeight = GetSystemMetrics(SystemMetric.VirtualScreenHeight);
            //mainWindow.Title = "Screen size: " + screenWidth.ToString() + " ×" + screenHeight.ToString();
            m_currentlyRecording = false;

            // Where to find FFMPEG
            appPath                 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            FFmpegLoader.FFmpegPath = System.IO.Path.Combine(appPath, "ffmpeg");
        }
 
        public bool RecordingStart(string path, ScreenRecordingCompleteCallback recordingCompleteCallback)
        {
            if (m_currentlyRecording) {
                return false;
            }

            m_currentlyRecording = true;

            if (System.IO.Path.GetDirectoryName(path) == "") {
                path = System.IO.Path.Combine(appPath, path);
            }

            WriteVideo(path);

            m_screenRecordingCompleteCallback = recordingCompleteCallback;

            m_RecordingResult                   = new IntellaScreenRecordingResult();
            m_RecordingResult.StartTime         = DateTime.Now;
            m_RecordingResult.RecordingFilePath = path;

            return true;
        }
                
        public bool IsRunning() {
            return m_currentlyRecording;
        }

        public bool RecordingStop()
        {
            if (!m_currentlyRecording) return false;
            m_currentlyRecording = false;

            return true;
        }

        private void WriteVideo(string path)
        {
            var settings           = new VideoEncoderSettings(width: screenWidth, height: screenHeight, framerate: frameRate, codec: VideoCodec.H264);
            settings.EncoderPreset = EncoderPreset.Fast;
            settings.CRF           = 17;

            m_mediaOutput = MediaBuilder.CreateContainer(path).WithVideo(settings).Create();

            DoRecording();
        }

        private void DoRecording()
        {
            Task t = Task.Factory.StartNew(() =>
            {
                long starttime = DateTime.Now.Ticks;
                long oldtime   = DateTime.Now.Ticks;
                long delta     = 10000000 / frameRate;          // its 10000000 ticks in 1 seccond
                long curTime   = 0;
                long frcount   = 0;

                while (m_currentlyRecording) {
                    frcount++;

                    var bitmap     = TakeScreenShot();
                    var rect       = new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size);
                    var bitLock    = bitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    var bitmapData = ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bitmap.Size);

                    while (DateTime.Now.Ticks - oldtime < 500000) {
                        Thread.Sleep(5);
                    }

                    curTime = DateTime.Now.Ticks;
                    TimeSpan ts = new TimeSpan(curTime - starttime);

                    m_mediaOutput.Video.AddFrame(bitmapData,ts); // Encode the frame
                    //m_mediaOutput.Video.AddFrame(bitmapData);
                    bitmap.UnlockBits(bitLock);

                    //while (DateTime.Now.Ticks - oldtime < delta)
                    //    Thread.Sleep(1000);
                    oldtime = DateTime.Now.Ticks;
                }

                m_mediaOutput.Dispose();

                m_RecordingResult.Success = true;
                m_RecordingResult.EndTime = DateTime.Now;

                m_screenRecordingCompleteCallback.Invoke(m_RecordingResult);
            });
        }
        
        // QD.QD_LoggerFunction = (string msg, params string[] msgFormat)
        public void SetLoggerCallback(QD.QD_LoggerFunction loggerFn) {
            m_logger = loggerFn;
        }
    }

    public class IntellaScreenRecordingResult
    {
        public DateTime StartTime;
        public DateTime EndTime;

        public string RecordingFilePath;

        public bool Success;
    }
}
