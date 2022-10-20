using System;
using System.Collections.Generic;
using System.Text;
//using System.Diagnostics;
using log4net;

namespace Lib
{
	///////////////////
	/// Timing Class
	/// A simple static wrapper of .net's StopWatch. Added automatic logging 
	/// using log4net.
	static public class Timing
	{
		//not limited to 10; just the pre-load #
		static List<libStopWatch> stopWatchStack = new List<libStopWatch>(10); 

		//static List<List

		static public void start()
		{
			start(null);
		}

		static public void start(string name)
		{
			libStopWatch l = new libStopWatch(name);
			stopWatchStack.Add(l);
			stopWatchStack[stopWatchStack.Count - 1].start();
		}

		static public void stop()
		{
			if (stopWatchStack.Count <= 0)
				return;

			libStopWatch l = stopWatchStack[stopWatchStack.Count - 1];
			l.stop();
			stopWatchStack.Remove(l);
		}

		static public void pause()
		{
			if (stopWatchStack.Count <= 0)
				return;

			stopWatchStack[stopWatchStack.Count - 1].pause();
		}

	}
}
