using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using log4net;
using System.Runtime.InteropServices;

namespace Lib
{
	public partial class LogViewer : Form
	{
		/// stuff for suppressing draws for a control
		// Not likely to work on mono/linux
		private const int WM_SETREDRAW = 0x000B;
		private const int WM_USER = 0x400;
		private const int EM_GETEVENTMASK = (WM_USER + 59);
		private const int EM_SETEVENTMASK = (WM_USER + 69);
		[DllImport("user32", CharSet = CharSet.Auto)]
		private extern static IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
	
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const long MaxLines = 100;

		private FileSystemWatcher m_watcher;
		private string m_path;
		private bool m_isDisposed;

		private long m_eofFilePosition;

		// currently we are assumming logFilePath is the filename and path is .
		// @todo: support full and releative paths
		public LogViewer(string logFilePath)
		{
			m_path = logFilePath;
			m_watcher = new FileSystemWatcher(".", logFilePath);
			m_watcher.NotifyFilter = NotifyFilters.LastWrite;

			// keep this form on top
			this.TopMost = true;
			InitializeComponent();
			//richTextBox1.MaxLength = 10000;
			logBox.ReadOnly = true;

			m_eofFilePosition = loadFileToRichTextBox(logFilePath);

			m_watcher.Changed += new FileSystemEventHandler(onChange);
			m_watcher.EnableRaisingEvents = true;
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Destructor.
		///
		/// Checks if database connection is still open and close it.
		~LogViewer()
		{
			log.Debug("Dispose called by GC.");
			Dispose(false);
		}

		public new void Dispose()
		{
			log.Debug("Dispose called manually.");
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected override void Dispose(bool disposeManagedResources)
		{
			if (!m_isDisposed)
			{
				if (disposeManagedResources)
				{
					// @todo make ConnectionManager : Idisposable and have it dispos all of its connections
					// dispose managed resources
					if (m_watcher != null)
					{
						m_watcher.EnableRaisingEvents = false;
						m_watcher.Dispose();
					}
					if (components != null)
						components.Dispose();
				}
				base.Dispose(disposeManagedResources);
				// dispose unmanaged resources (don't have any)
				m_isDisposed = true;
			}
			else
			{
				log.Error("Dispose called more than once.");
			}
		}

		private long loadFileToRichTextBox(string path)
		{
			return loadFileToRichTextBox(path, -1, true);
		}

		private long loadFileToRichTextBox(string path, long startPosition)
		{
			return loadFileToRichTextBox(path, startPosition, true);
		}

		private long loadFileToRichTextBox(string path, long startPosition, bool doPreventRedraws)
		{
			long eofPosition, adjustedStartPosition, newLineCount;
			long currentBoxLineCount = countLines(logBox);

			// These mode flags are needed to be able to open the log file our app may already be writing to.
			// A different solution to the locking problem may be to configure log4net lib to use a "minimal 
			// lock". See http://hoenes.blogspot.com/2006/08/displaying-log4net-log-file-on-aspnet.html
			FileStream logFs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);


			if (startPosition == -1) // -1 means start at scrollback from end of file
			{
				// this result seems to be off sometimes...
				//long newPos = Math.Max(0, countLines(logFs) - MaxLines);
				//seekForwardByLines(logFs, newPos);

				// this result seems to be off sometimes...
				//logFs.Position = getPositionFromLinesIn(logFs, Math.Max(0, countLines(logFs) - MaxLines));

				// this ones seems consistently correct..
				adjustedStartPosition = getPositionFromRLinesBack(logFs, MaxLines);
			}
			else
				adjustedStartPosition = startPosition;

			logFs.Seek(adjustedStartPosition, SeekOrigin.Begin);
			newLineCount = countLines(logFs);
			logFs.Seek(adjustedStartPosition, SeekOrigin.Begin);

			if (doPreventRedraws && newLineCount + currentBoxLineCount > MaxLines)
				//removeFirstLines(newLineCount + currentBoxLineCount - MaxLines);
				runWithoutRedraws(logBox, removeFirstLines, newLineCount + currentBoxLineCount - MaxLines);

			string text;
			TextReader logTr = new StreamReader(logFs);
			while (null != (text = logTr.ReadLine()))
				append(text + "\n");

			// remove 1 line at a time instead; doesn't handle a cleared viewer
			//if (startPosition >= 0)
			//    while (null != (text = logTr.ReadLine()))
			//    {
			//        //removeFirstLine();
			//        //runWithoutRedraws(logBox, removeFirstLine);
			//        append(text + "\n");
			//    }
			//else
			//    while (null != (text = logTr.ReadLine()))
			//        append(text + "\n");

			eofPosition = logFs.Position;
			logTr.Close();
			logFs.Close();

			//if (InvokeRequired)
			//    Invoke(new MethodInvoker(jumpToBottom));

			//jumpToBottom();

			return eofPosition;
		}

		private delegate void simpleDelegate();
		private delegate void simpleDelegateSetString(string message);
		private delegate void simpleDelegateSetLong(long message);

		// Runs code in a func without drawing the control until the end.
		// Not likely to work on mono/linux
		private void runWithoutRedraws(Control control, simpleDelegateSetLong toRunCallback, long param)
		{
			IntPtr eventMask = IntPtr.Zero;
			IntPtr h = (IntPtr)GetControlProperty(control, "Handle");
			try
			{
				// Stop redrawing:
				SendMessage(h, WM_SETREDRAW, 0, IntPtr.Zero);
				// Stop sending of events:
				eventMask = SendMessage(h, EM_GETEVENTMASK, 0, IntPtr.Zero);
				toRunCallback(param);
			}
			finally
			{
				// turn on events
				SendMessage(h, EM_SETEVENTMASK, 0, eventMask);
				// turn on redrawing
				SendMessage(h, WM_SETREDRAW, 1, IntPtr.Zero);
			}
		}

		delegate void SetValueDelegate(Object obj, Object val, Object[] index);

		public void SetControlProperty(Control ctrl, String propName, Object val)
		{
			PropertyInfo propInfo = ctrl.GetType().GetProperty(propName);
			Delegate dgtSetValue = new SetValueDelegate(propInfo.SetValue);
			ctrl.Invoke(dgtSetValue, new Object[3] { ctrl, val, /*index*/null });
		}

		delegate Object GetValueDelegate(Object obj, Object[] index);

		public Object GetControlProperty(Control ctrl, String propName)
		{
			PropertyInfo propInfo = ctrl.GetType().GetProperty(propName);
			Delegate dgtSetValue = new GetValueDelegate(propInfo.GetValue);
			return ctrl.Invoke(dgtSetValue, new Object[2] { ctrl, /*index*/null });
		}

		protected void removeFirstLine()
		{
			removeFirstLines(1);
		}

		/// commented code are various failed attempts to reset text and scroll it in a 
		/// richTextBox. (all failed). (only tested working method so far on winxp is
		/// importdll method to disable control draws. Leaving commented code here for
		/// testing in mono.
		protected void removeFirstLines(long lineCount)
		{
			//if (richTextBox1.InvokeRequired)
			//    richTextBox1.BeginInvoke(new simpleDelegate(richTextBox1.SuspendLayout));
			//else
			//    richTextBox1.SuspendLayout();

			string t;
			if (logBox.InvokeRequired)
				t = (string)GetControlProperty(logBox, "Text");
			else
				t = logBox.Text;

			int startIndex;
			while (lineCount-- > 0)
			{
				startIndex = t.IndexOf('\n');
				t = startIndex < 0 ? "" : t.Substring(startIndex + 1);
			}

			if (logBox.InvokeRequired)
			{
				SetControlProperty(logBox, "DrawEnabled", false);
				//SetControlProperty(logBox, "Text", "");
				SetControlProperty(logBox, "Text", t);
				//logBox.BeginInvoke(new simpleDelegate(logBox.ResetText));
				//logBox.BeginInvoke(new simpleDelegate(logBox.Clear));
				//logBox.BeginInvoke(new addDelegate(logBox.AppendText), t);
				SetControlProperty(logBox, "SelectionStart", GetControlProperty(logBox, "TextLength"));
				logBox.BeginInvoke(new simpleDelegate(logBox.ScrollToCaret));
				SetControlProperty(logBox, "DrawEnabled", true);
			}
			else
			{
				logBox.DrawEnabled = false;
				//logBox.Clear();
				//logBox.Text = "";
				logBox.Text = t;
				//logBox.ResetText();
				//logBox.AppendText(t);
				logBox.SelectionStart = logBox.TextLength;
				logBox.ScrollToCaret();
				logBox.DrawEnabled = true;
				//logBox.Select();
			}
			//if (richTextBox1.InvokeRequired)
			//    richTextBox1.BeginInvoke(new simpleDelegate(richTextBox1.ResumeLayout));
			//else
			//    richTextBox1.ResumeLayout();
		}

		protected long countLines(RichTextBox r)
		{
			long count = 0;
			string t;
			if (logBox.InvokeRequired)
				t = (string)GetControlProperty(logBox, "Text");
			else
				t = logBox.Text;

			foreach (char c in t)
				if (c == '\n')
					count++;
			//log.Debug(count.ToString());
			return count;
		}

		protected void append(string message)
		{
			if (this.InvokeRequired)
				this.BeginInvoke(new simpleDelegate(this.BringToFront));
			else
				this.BringToFront();

			if (logBox.InvokeRequired)
			{
				logBox.BeginInvoke(new simpleDelegateSetString(logBox.AppendText), message);
				//richTextBox1.BeginInvoke(new addDelegate2(richTextBox1.Select));
			}
			else
			{
				logBox.AppendText(message);
				//richTextBox1.Select();
			}
			//SetControlProperty(richTextBox1, "SelectionStart", GetControlProperty(richTextBox1, "TextLength"));

			if (logBox.InvokeRequired)
				logBox.BeginInvoke(new simpleDelegate(logBox.ScrollToCaret));
			else
				logBox.ScrollToCaret();

			//if (logBox.InvokeRequired)
			//    logBox.BeginInvoke(new simpleDelegate(logBox.Update));
			//else
			//    logBox.Update();

		}

		private void seekForwardByLines(FileStream fs, long lineCount)
		{
			TextReader tr = new StreamReader(fs);
			while (lineCount-- > 0)
				tr.ReadLine();
		}

		private long countLines(FileStream fs)
		{
			long lineCount = 0;

			TextReader tr = new StreamReader(fs);
			while (null != tr.ReadLine())
				lineCount++;

			fs.Position = 0;

			return lineCount;
		}

		private long getPositionFromRLinesBack(FileStream fs, long linesBackCount)
		{
			long position;// = fs.Length;
			long origPosition = fs.Position;

			fs.Position = fs.Length;

			while (true)
			{
				if (fs.Position <= 0)
				{
					position = 0;
					break;
				}

				fs.Position--;

				if (fs.ReadByte() == '\n')
				{
					if (--linesBackCount < 0)
					{
						position = fs.Position;
						break;
					}
				}
				fs.Position--;
			}

			fs.Position = origPosition;
			return position;
		}

		private long getPositionFromLinesIn(FileStream fs, long linesInCount)
		{
			long position;
			long origPosition;
			position = origPosition = fs.Position;

			TextReader tr = new StreamReader(fs);
			while (tr.ReadLine() != null && linesInCount-- > 0)
				position = fs.Position;

			fs.Position = origPosition;
			return position;
		}

		private void onChange(object source, FileSystemEventArgs e)
		{
			//FileSystemWatcher fsw = (FileSystemWatcher)source;

			m_eofFilePosition = loadFileToRichTextBox(m_path, m_eofFilePosition);
		}

		private void AlwaysOnTopCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (((CheckBox)sender).Checked)
				this.TopMost = true;
			else
				this.TopMost = false;
		}

		private void clearButton_Click(object sender, EventArgs e)
		{
			logBox.Clear();
		}
		
		private void logBox_Resize(object sender, EventArgs e)
		{
			jumpToBottom();
		}

		private Object m_lock = new Object();

		private void jumpToBottom()
		{
			logBox.DrawEnabled = false;
			logBox.Text = logBox.Text;
			logBox.SelectionStart = logBox.TextLength;
			logBox.ScrollToCaret();
			logBox.DrawEnabled = true;
		}
	}
}
