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
        MediaOutput fileMp4;
        int frameRate = 20;                 //Frame rate of the video
        string path;                        //mp4 file name

        bool recordingb;

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
            
            var settings = new VideoEncoderSettings(width: screenWidth, height: screenHeight, framerate: frameRate, codec: VideoCodec.H264);
            settings.EncoderPreset = EncoderPreset.Fast;
            settings.CRF = 17;
            fileMp4 = MediaBuilder.CreateContainer(path).WithVideo(settings).Create();

            StartRecording();
         
        }

        void StartRecording()           //Начинаем запись экрана
        {
            Task t = Task.Factory.StartNew(() =>
            {
                long starttime = DateTime.Now.Ticks;
                long oldtime = DateTime.Now.Ticks;
                long delta = 10000000 / frameRate;          //its 10000000 ticks in 1 seccond
                long curTime=0;
                long frcount = 0;

                while (recordingb)
                {
                    frcount++;
                    var bitmap = TakeScreenShoot();
                    var rect = new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size);
                    var bitLock = bitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    var bitmapData = ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bitmap.Size);

                    while (DateTime.Now.Ticks - oldtime < 500000)
                        Thread.Sleep(5);
                    curTime = DateTime.Now.Ticks;
                    TimeSpan ts = new TimeSpan(curTime - starttime);


                    fileMp4.Video.AddFrame(bitmapData,ts); // Encode the frame
                    //fileMp4.Video.AddFrame(bitmapData);
                    bitmap.UnlockBits(bitLock);

                    //while (DateTime.Now.Ticks - oldtime < delta)
                    //    Thread.Sleep(1000);
                    oldtime = DateTime.Now.Ticks;
                }
                fileMp4.Dispose();
                


            });
            
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

        public static void DoEvents()
        {            
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

    }
}
