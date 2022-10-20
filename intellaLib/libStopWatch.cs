using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using log4net;

namespace Lib
{
	class libStopWatch
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Stopwatch sw;
		private bool isPaused;
		private string name, createTimeStamp;

		public libStopWatch(string optionalCustomName)
		{
			isPaused = false;
			sw = new Stopwatch();
			if (optionalCustomName == null)
				name = "Unnamed stopwatch";
			else
				name = optionalCustomName;
			createTimeStamp = System.DateTime.Now.Ticks.ToString();
		}

		public void start()
		{
			if (sw.IsRunning)
				log.Error(name + ": Trying to start a Timing that's already running. " + createTimeStamp);

			if (isPaused)
				log.Debug(name + ": Resuming Timing. " + createTimeStamp);
			else
				log.Debug(name + ": Starting a new Timing. " + createTimeStamp);
			isPaused = false;

			sw.Start();
		}

		public void stop()
		{
			sw.Stop();
			log.Debug(name + ": Timing stopped. Total Elapsed time: " + sw.Elapsed.ToString() + " " + createTimeStamp);
			isPaused = false;
			sw.Reset();
		}

		public void pause()
		{
			sw.Stop();
			isPaused = true;
			log.Debug(name + ": Timing paused. Elapsed time since starting: " + sw.Elapsed.ToString() + " " + createTimeStamp);
		}

	}
}
