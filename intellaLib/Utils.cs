using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Security.Principal;
using Microsoft.Win32;
using System.Net.NetworkInformation;

namespace Lib {
    public static class Utils {
        public static OrderedDictionary Globals = new OrderedDictionary();

        private static QD.QD_LoggerFunction m_logger_qd = QD_Internal; // QuickDebug logger
        private static QD.QE_LoggerFunction m_logger_qe = QE_Internal; // QuickError logger

        public static void SetLoggerCallBack_QD(QD.QD_LoggerFunction qd)
        {
            m_logger_qd = qd;
        }

        public static void SetLoggerCallBack_QE(QD.QE_LoggerFunction qe)
        {
            m_logger_qe = qe;
        }

        public static void QD(string msg, params string[] argsRest)
        {
            m_logger_qd.Invoke(msg, argsRest);
        }

        public static void QE(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] argsRest)
        {
            m_logger_qe.Invoke(errorLevel, errorToken, ex, msg, argsRest);
        }

        private static string QD_Internal(string msg, params string[] msgFormat)
        {
            string log_line = String.Format(msg, msgFormat);

            Console.WriteLine(log_line);

            return log_line;
        }

        private static string QE_Internal(QD.ERROR_LEVEL errorLevel, string errorToken, Exception ex, string msg, params string[] msgFormat)
        {
            string log_line = "ERROR: " + String.Format("[{0} {1}] ", errorToken, errorLevel) + String.Format(msg, msgFormat);

            Console.WriteLine(log_line);

            return log_line;
        }

        public static Boolean StringToBoolean(string item) {
            // Various ways we can be true!  Everything else is false
                        
            switch (item.ToUpper()) {
                case "1":
                    return true;
                case "YES":
                    return true;
                case "Y":
                    return true;
                case "TRUE":
                    return true;
                case "T":
                    return true;
            }

            return false;

        }

        ///////////////////////////////////////////////////////////////////////////////////
        /// Graphics Utils
        ///////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentForm"></param>
        /// <param name="largestFontSize"></param>
        /// <param name="boxWidth"></param>
        /// <param name="boxHeight"></param>
        /// <param name="boxText"></param>
        /// <param name="boxFont"></param>
        /// <returns></returns>
        ///
        public static List<int> FindLargestFontForBox(Form parentForm, int largestFontSize, int boxWidth, int boxHeight, string boxText, Font boxFont)
        {
	        Dictionary<int, Dictionary<string, int>> font_sizes = new Dictionary<int, Dictionary<string, int>>();

	        int font_size = 0;

	        // Figure out the biggest font we can use, with the text of: boxText
            Graphics gr = parentForm.CreateGraphics();
	        for (font_size = 1; font_size <= largestFontSize; font_size++) {
		        Font font = new Font(boxFont.FontFamily, font_size, boxFont.Style, boxFont.Unit, boxFont.GdiCharSet, boxFont.GdiVerticalFont);
                // ReSharper disable once IdentifierTypo
		        SizeF sizef = gr.MeasureString(boxText, font, 512);
		        Size size = sizef.ToSize();
		        Dictionary<string, int> width_height = new Dictionary<string, int>();

		        width_height.Add("width", size.Width);
		        width_height.Add("height", size.Height);
		        font_sizes.Add(font_size, width_height);
	        }

            gr.Dispose();

	        // Will be populated with the dimensions in pixels of the drawn text itself, using the biggest font we can use
	        int font_height = 0;
	        int font_width = 0;

	        // see which font size fills the size without going over
	        for (font_size = 1; font_size <= largestFontSize; font_size++) {
		        font_height = font_sizes[font_size]["height"];
		        font_width = font_sizes[font_size]["width"];

		        if (((font_height >= boxHeight) | (font_width >= boxWidth))) {
                    // Max... keep font size where we are
			        break;
		        }
	        }

	        List<int> ret = new List<int> {font_width, font_height, font_size};
            return ret;
        }

        public static SizeF GUI_FindSizeOfRenderedText(Form parentForm, string theString, Font theFont) {
            Graphics gr = parentForm.CreateGraphics();

            return gr.MeasureString(theString, theFont);
        }

        ///////////////////////////////////////////////////////////////////////////////////
        /// String Utils
        ///////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// </summary>
        /// <param name="result"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        ///
        public static string GetLongestTextFromResult(List<OrderedDictionary> result, string fieldName)
        {
	        string longest_text = "";

	        foreach (OrderedDictionary result_item in result) {
		        string text_item = result_item[fieldName].ToString();

		        if ((text_item.Length > longest_text.Length)) {
			        longest_text = text_item;
		        }
	        }

	        return longest_text;
        }

        public static string GetLongestTextFromQueryResultSet(QueryResultSet result, string fieldName) {
            string longest_text = "";

            foreach (QueryResultSetRecord result_item in result) {
                string text_item = result_item[fieldName];

                if ((text_item.Length > longest_text.Length)) {
                    longest_text = text_item;
                }
            }

            return longest_text;
        }

        public static string GetLongestTextFromDataGridViewColumns(DataGridViewColumnCollection columns, string fieldName) {
            string longest_text = "";

            foreach (DataGridViewColumn result_item in columns) {
                string text_item = result_item.HeaderText;

                if ((text_item.Length > longest_text.Length)) {
                    longest_text = text_item;
                }
            }

            return longest_text;
        }

        public static string GetLongestTextFromArrayListOfString(ArrayList ar) {
            string longest_key = "";

            foreach (string key_name in ar) {
                if (key_name.Length > longest_key.Length) {
                    longest_key = key_name;
                }
            }

            return longest_key;
        }

        /// <summary>
        /// Replace first occurance of a string, returning where we 'left of'
        /// Example:
        ///   string foo = "Some thing or some other thing"
        ///   int pos = StringReplaceFirstOccurance(foo, "thing", "x");  // pos = 6,  because we shortened the string to 'Some x or some other thing'
        ///   
        /// </summary>
        /// <param name="theString"></param>
        /// <param name="searchFor"></param>
        /// <param name="replaceWith"></param>
        /// <returns>Position of the end of the replacement</returns>
        public static int StringReplaceFirstOccurance(ref string theString, string searchFor, string replaceWith, int startPosition = 0) {
            int pos_found = theString.IndexOf(searchFor);

            if (pos_found < 0) {
                return theString.Length;
            }

            theString = theString.Substring(0, pos_found) + replaceWith + theString.Substring(pos_found + searchFor.Length);

            return pos_found + replaceWith.Length;
        }

        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">Color to correct.</param>
        /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
        /// Negative values produce darker colors.</param>
        /// <returns>
        /// Corrected <see cref="Color"/> structure.
        /// </returns>
        public static Color ChangeColorBrightness(Color color, double correctionFactor)
        {
            double red   = (double) color.R;
            double green = (double) color.G;
            double blue  = (double) color.B;

            if (correctionFactor < 0) {
                correctionFactor  = 1 + correctionFactor;
                red              *= correctionFactor;
                green            *= correctionFactor;
                blue             *= correctionFactor;
            }
            else {
                red   = (255 - red)   * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue  = (255 - blue)  * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (int) red, (int) green, (int) blue);
        }

        public static double DateTimeToUnixTime(DateTime theTime)
        {
            return theTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static DateTime UnixTimeToDateTime(double unixTime)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime ret_date_time = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            ret_date_time = ret_date_time.AddSeconds(unixTime).ToLocalTime();

            return ret_date_time;
        }

        public static Boolean UploadFileToURL(string localFile, string url)
        {
            WebClient myWebClient = new WebClient();

            if (!File.Exists(localFile)) {
                QD("Utils::UploadFileToURL() Failed to upload file: {0} to [{1}] -- Local file does not exist", localFile, url);
                return false;
            }

            try
            {
                // HTTP POST
                byte[] response_bytes = myWebClient.UploadFile(url, localFile);

                WebHeaderCollection h = myWebClient.ResponseHeaders;
                string response_text = System.Text.Encoding.ASCII.GetString(response_bytes);

                QD("Utils::UploadFileToURL() Completed upload file: {0} to [{1}] -- {2}", localFile, url, response_text);
            }
            catch (System.Net.WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    int response_code    = (int) ((HttpWebResponse) e.Response).StatusCode;
                    string response_desc = ((HttpWebResponse) e.Response).StatusDescription.ToString();

                    QD("Utils::UploadFileToURL() Failed to upload file: {0} to [{1}] -- {2}: {3}", localFile, url, response_code.ToString(), response_desc);
                }
               else
                {
                    QD("Utils::UploadFileToURL() Failed to upload file: {0} to [{1}] -- Unknown Error", localFile, url);
                }

                return false;
            }
            catch (Exception e)
            {
                QE(Lib.QD.ERROR_LEVEL.WARNING, "UtilsFileUploadException", null, "File upload has failed: " + e.ToString());
                return false;
            }

            return true;
        }

        ///////////////////////////////////////////////////////////////////////////////////
        /// File Related
        ///////////////////////////////////////////////////////////////////////////////////

        public static Bitmap GetImageFromWebServer(string url) {
            // load a bitmap from a Web response stream
		    WebRequest req = WebRequest.Create(url);
		    WebResponse resp = req.GetResponse();
		    Stream s = resp.GetResponseStream();
		    Bitmap bmp = new Bitmap(s);
		    return bmp;
        }

        public static MemoryStream GetSoundFromWebServer(string url)
        {
		    MemoryStream ms = new MemoryStream();
		    WebRequest req = WebRequest.Create(url);
		    WebResponse resp = req.GetResponse();
		    Stream s = resp.GetResponseStream();

		    byte[] buffer = new byte[32769];
	        bool done = false;

		    while ((done == false)) {
		        int read = s.Read(buffer, 0, buffer.Length);

		        if ((read > 0)) {
				    ms.Write(buffer, 0, read);
			    } else {
				    done = true;
			    }
		    }

	        ms.Seek(0, 0);
		    return ms;
        }

        public static string File_ReadTail(string logFile, int tailCount) {
            int count = 0;
            string content;
            byte[] buffer = new byte[1];
            FileStream fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            List<string> returned_lines = new List<string>();

            // read to the end.
            fs.Seek(0, SeekOrigin.End);

            // read backwards 'n' lines
            while (count < tailCount) {
                if (fs.Position == 0) {
                    // We're at the beginning of the file already... done
                    break;
                }

                fs.Seek(-1, SeekOrigin.Current);
                fs.Read(buffer, 0, 1);

                if (buffer[0] == '\n') { 
                    count++;
                }

                fs.Seek(-1, SeekOrigin.Current); // fs.Read(...) advances the position, so we need to go back again
            }

            fs.Seek(1, SeekOrigin.Current); // go past the last '\n'

            // read the last n lines
            StreamReader sr = new StreamReader(fs);
            content = sr.ReadToEnd();

            fs.Close();

            return content;
        }

        public static void SetApplicationAutomaticStartup(bool startupEnabled, string applicationName) {
            Microsoft.Win32.RegistryKey key;

            if (Utils.IsAdministratorSimple()) {
                key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            }
            else {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);                
            }

            if (startupEnabled) {
                key.SetValue(applicationName, Application.ExecutablePath);
            }
            else {
                key.DeleteValue(applicationName);
            }
        }

        /*
         // Example
         Utils.SpawnBackgroundThread(
             delegate(object w_sender, System.ComponentModel.DoWorkEventArgs e) { 
                // Example: Hashtable foo = (Hashtable) e.Argument;  // Arbitrary data passed... this is the 'data' parameter
             }
         );
        */
        public static bool SpawnBackgroundThread(DoWorkEventHandler doWork, object data = null)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(doWork);

            try
            {
                worker.RunWorkerAsync(data);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached) { throw; }
                // handleError(ex, "Threading error: creating new thread worker.");
                return false;
            }

            return true;
        }


        /*
         // Example
         Utils.SpawnBackgroundThread(
             delegate(object w_sender, System.ComponentModel.DoWorkEventArgs e) { 
                // Example: Hashtable foo = (Hashtable) e.Argument;  // Arbitrary data passed... this is the 'data' parameter
             },
             delegate(object w_sender, System.ComponentModel.RunWorkerCompletedEventArgs e) { 

             },
             delegate(object w_sender, System.ComponentModel.ProgressChangedEventArgs e) { 

             }
         );
        */
        public static bool SpawnBackgroundThread(DoWorkEventHandler doWork, ProgressChangedEventHandler doWorkProgress, RunWorkerCompletedEventHandler doWorkComplete, object data = null) { 
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork             += new DoWorkEventHandler(doWork);
            worker.ProgressChanged    += new ProgressChangedEventHandler(doWorkProgress);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(doWorkComplete);

            try {
                worker.RunWorkerAsync(data);
            }
            catch (Exception ex) {
                if (Debugger.IsAttached) { throw; }
                // handleError(ex, "Threading error: creating new thread worker.");
                return false;
            }

            return true;
        }

        public static PingReply PingHost (string hostName, int timeout = 15, string sendBuffer = "") {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            if (sendBuffer == "") {
                sendBuffer = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            }

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.

            byte[] buffer = Encoding.ASCII.GetBytes(sendBuffer);

            PingReply reply = pingSender.Send(hostName, timeout, buffer, options);

            // Success Check (reply.status == IPStatus.Success);
            return reply;
        }

        public static bool IsAdministratorSimple()
        {
            /*
            try { 
                Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\VB and VBA Program Settings\\IntellaToolBar", "AdministratorTest", "1");
                MessageBox.Show("YAY");
                return true;
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
                return false;
            }
            */

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool IsAdministrator()
        {
            bool _level = false;

            // Get Identity:
            WindowsIdentity user = WindowsIdentity.GetCurrent();

            // Set Principal
            WindowsPrincipal role = new WindowsPrincipal(user);

            #region Test Operating System for UAC:
            if (Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version.Major < 6)
            {
                // False:
                _level = false;

                // Todo: Exception/ Exception Log
            }
            #endregion
            else
            {
                #region Test Identity Not Null:
                if (user == null)
                {
                    // False:
                    _level = false;

                    // Todo: "Exception Log / Exception"
                }
                #endregion
                else
                {
                    #region Ensure Security Role:

                    if (!(role.IsInRole(WindowsBuiltInRole.Administrator)))
                    {
                        // False:
                        _level = false;

                        // Todo: "Exception Log / Exception"
                    }
                    else
                    {
                        // True:
                        _level = true;
                    }

                    #endregion
                }
            }

            return _level;
        }

        public static void Elevate(string arguments = "") {
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);           

            if (!hasAdministrativeRight)
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.UseShellExecute  = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName         = Application.ExecutablePath;
                startInfo.Verb             = "runas";
                startInfo.Arguments        = arguments;

                try
                {
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    // Probably System.ComponentModel.Win32Exception ?
                    MessageBox.Show("Run As Admin Failed: " + ex.ToString());
                    return;
                }
            }
        }

        public static Dictionary<string, List<OrderedDictionary>> ConvertTo_DictionaryString_ListOrderedDictionary(Dictionary<string, QueryResultSet> dictionary_string_queryResultSet) {
            Dictionary<string, List<OrderedDictionary>> result = new Dictionary<string, List<OrderedDictionary>>();

            foreach (KeyValuePair<string, QueryResultSet> query_result_set_item in dictionary_string_queryResultSet) {
                string dictionary_string = query_result_set_item.Key;
                               
                // This is the thing getting converted to a List<OrderedDictionary>
                QueryResultSet query_result_set = dictionary_string_queryResultSet[dictionary_string];

                // This is the new List<OrderedDictionary>
                List<OrderedDictionary> list_ordered_dictionary = DbHelper.ConvertQueryResultSet_To_ListOfOrderedDictionary(query_result_set);

                result.Add(dictionary_string, list_ordered_dictionary);
            }

            return result;
        }
    }
}
