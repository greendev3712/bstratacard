using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace IntellaUpdate
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();

            if (!System.Diagnostics.EventLog.SourceExists("IntellaUpdater"))
            {
                System.Diagnostics.EventLog.CreateEventSource("IntellaUpdater", "IntellaUpdaterLog");
            }

            eventLog1.Source = "IntellaUpdater";
            eventLog1.Log    = "IntellaUpdaterLog";
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("The IntellaUpdater Service has started.");

            var directory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            eventLog1.WriteEntry("The current executing path for this service is " + directory);

            var programFilesPathEnum    = Environment.SpecialFolder.ProgramFilesX86;
            var programFilesPathLiteral = Environment.GetFolderPath(programFilesPathEnum);
            eventLog1.WriteEntry("Current base Program Files (x86): " + programFilesPathLiteral);

            Timer timer = new Timer();
            timer.Interval = 30000; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

            try
            {
                eventLog1.WriteEntry("Checking for updates on service start.");
                Process.Start(programFilesPathLiteral + "\\Intellasoft\\bin\\wyUpdate.exe", "/fromservice");
                eventLog1.WriteEntry("Service start update check process has completed.");
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry(e.Message);
            }
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            try
            {

                var programFilesPathEnum = Environment.SpecialFolder.ProgramFilesX86;
                var programFilesPathLiteral = Environment.GetFolderPath(programFilesPathEnum);
                Process.Start(programFilesPathLiteral + "\\Intellasoft\\bin\\wyUpdate.exe", "/fromservice");
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry(e.Message);
            }
            
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("The IntellaUpdater Service has stopped.");
        }

        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e) {

        }
    }
}
