using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using FFMediaToolkit;               //Install-Package FFMediaToolkit
using FFMediaToolkit.Graphics;
using FFMediaToolkit.Encoding;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenRecord
{
    public enum SystemMetric
    {
        VirtualScreenWidth = 78, // CXVIRTUALSCREEN 0x0000004E 
        VirtualScreenHeight = 79, // CYVIRTUALSCREEN 0x0000004F 
        SM_CYFULLSCREEN = 17,
        SM_CXFULLSCREEN = 16,
    }

    public partial class MainWindow : Window
    {
        int screenWidth, screenHeight;          //Screen size
        string appPath;        
        int frameRate = 20;                 //Frame rate of the video
        string path;                        //mp4 file name
        long recordTime = 30;               //Record time in minutes

        bool recordingb;
        Bitmap bitmap = null;
        long fnumber = 0;

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
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }

        public Bitmap TakeScreenShoot()
        {
            var sc = GetScalingFactor();


            Bitmap shoot = new Bitmap(screenWidth, screenHeight);
            var graphics = Graphics.FromImage(shoot);
            {
                graphics.CopyFromScreen(0, 0, 0, 0, shoot.Size);                
            }
            return shoot;
        }
        public void TakeScreenShoot2()
        {
            var sc = GetScalingFactor();


            //shoot = new Bitmap(screenWidth, screenHeight);
            var graphics = Graphics.FromImage(bitmap);
            {
                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }            
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        public MainWindow()
        {
                        
            InitializeComponent();
            InitVariables();

        }

        void InitVariables()
        {
            screenWidth = GetSystemMetrics(SystemMetric.VirtualScreenWidth);
            screenHeight = GetSystemMetrics(SystemMetric.VirtualScreenHeight);
            mainWindow.Title = "Screen size: " + screenWidth.ToString() + " ×" + screenHeight.ToString();
            recordingb = false;
            appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            FFmpegLoader.FFmpegPath = System.IO.Path.Combine(appPath,"ffmpeg");

            textInfo.IsReadOnly = true;
            textInfo.Text = "";
            path = "test_video.mp4";
            textFileName.Text = path;
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            if (!recordingb) return;
            recordingb = false;
            string times = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            textInfo.Text = times+" Stop recording.\n" + textInfo.Text;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            AddLog("Start recording");
            if (recordingb) return;
            recordingb = true;
            
            path=textFileName.Text;
            if (System.IO.Path.GetDirectoryName(path)=="")
                path=System.IO.Path.Combine(appPath, path);
            WriteVideo(path);

            string times = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            textInfo.Text = times+" Start recording to file: " + path + "\n" + textInfo.Text;
        }

        void WriteVideo(string path)
        {
            //FFmpegLoader.FFmpegPath = @"D:\Temp\FFmpeg\";           //.\ffmpeg\x86_64\      Or copy to output folder
            
            /*
            var settings = new VideoEncoderSettings(width: screenWidth/2, height: screenHeight/2, framerate: frameRate, codec: VideoCodec.H265);
            //settings.EncoderPreset = EncoderPreset.Fast;
            settings.EncoderPreset = EncoderPreset.VerySlow;
            settings.CRF = 17;
            fileMp4 = MediaBuilder.CreateContainer(path).WithVideo(settings).Create();
            //*/

            StartRecording(path);
         
        }

        void StartRecording(string path)           //Начинаем запись экрана
        {
            Task t = Task.Factory.StartNew(() =>
            {
                while (recordingb)
                {
                    int i = 1;
                    Task t1 = Task.Factory.StartNew(() =>
                    {
                        long starttime = DateTime.Now.Ticks;
                        long oldtime = DateTime.Now.Ticks;
                        long delta = 10000000 / frameRate;          //its 10000000 ticks in 1 seccond
                        long curTime = 0;
                        long frcount = 0;                        

                        bitmap = TakeScreenShoot();
                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size);

                        
                        VideoEncoderSettings settings = null;
                        try
                        {
                            settings = new VideoEncoderSettings(width: screenWidth / 2, height: screenHeight / 2, framerate: frameRate, codec: VideoCodec.H264);
                        }
                        catch (Exception e)
                        {
                            AddLog("settings = new VideoEncoderSettings: " + e.Message);
                        }
                        settings.EncoderPreset = EncoderPreset.Fast;
                        //settings.EncoderPreset = EncoderPreset.VerySlow;
                        settings.CRF = 17;
                        string s = path;// + fnumber.ToString("0000");
                        int ind = s.LastIndexOf(".");
                        if (ind == -1) s = path + fnumber.ToString("0000");
                        else
                        {
                            s = s.Substring(0, ind) + fnumber.ToString("0000") + s.Substring(ind);
                        }
                        AddLog("Start recording to a file: " + s);
                        MediaOutput fileMp4 = null;
                        try
                        {
                            fileMp4 = MediaBuilder.CreateContainer(s).WithVideo(settings).Create();
                        }
                        catch (Exception e)
                        {
                            AddLog("fileMp4 = MediaBuilder.CreateContainer(s): " + e.Message);
                        }
                        long startrecording = DateTime.Now.Ticks;

                        while (recordingb && ((DateTime.Now.Ticks - startrecording) < recordTime * 60 * 10000000))
                        {
                            frcount++;

                            try
                            {
                                TakeScreenShoot2();
                            }
                            catch (Exception e)
                            {
                                AddLog("TakeScreenShoot2(): " + e.Message);
                            }
                        //System.Drawing.Rectangle rect = new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size);

                        BitmapData bitLock = null;
                            bitLock = bitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                            ImageData bitmapData = ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bitmap.Size);


                            while (DateTime.Now.Ticks - oldtime < 500000)
                                Thread.Sleep(5);
                            curTime = DateTime.Now.Ticks;
                            TimeSpan ts = new TimeSpan(curTime - starttime);

                            try
                            {
                                fileMp4.Video.AddFrame(bitmapData, ts); // Encode the frame                            
                        }
                            catch (Exception e)
                            {
                                AddLog("fileMp4.Video.AddFrame(bitmapData, ts): " + e.Message);
                            }
                        //fileMp4.Video.AddFrame(bitmapData);
                        bitmap.UnlockBits(bitLock);

                            oldtime = DateTime.Now.Ticks;

                        }
                        try
                        {
                            Thread.Sleep(50);
                            fileMp4.Dispose();
                        }
                        catch (Exception e)
                        {
                            AddLog("fileMp4.Dispose(): " + e.Message);
                        }
                        fnumber += 1;
                        
                    });
                    while (!t1.IsCompleted)
                        Thread.Sleep(500);
                }
            });
            
            
        }

        public void AddLog(string log)
        {
            try
            {
                using (StreamWriter writetext = new StreamWriter("log.txt",true))
                {
                    writetext.WriteLine(log);
                }
            }
            catch (Exception) { }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "test_video"; // Default file name
            dlg.DefaultExt = ".mp4"; // Default file extension
            dlg.Filter = "Mp4 video format (.mp4)|*.mp4"; // Filter files by extension
            dlg.InitialDirectory = appPath;
            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                path = System.IO.Path.Combine(dlg.InitialDirectory,dlg.FileName);
                textFileName.Text = path;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            recordingb = false;
            mainWindow.Hide();
            Thread.Sleep(60000);
        }

        public static void DoEvents()
        {            
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

    }
}
