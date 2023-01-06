using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IntellaScreenRecord;
using Lib;

namespace intellaQueue
{
    public partial class IntellaQueueForm : System.Windows.Forms.Form
    {
        public string GetPath_ScreenRecordingTempStorage() {
            return Path.GetTempPath();
        }

        /// <summary>
        /// If we previously had problems uploading screen recordings, we'll try and upload anything we still have locally
        /// (Spawns a new thread)
        /// </summary>
        public void ScreenCapture_UploadFileBacklog() {
            Task t = Task.Factory.StartNew(() => {
                // Upload files that we still have locally
                try {
                    ScreenCapture_UploadFileBacklog_Do();
                }
                catch (Exception ex) {
                    MQD("ScreenCapture_UploadFileBacklog() -- Failed: " + ex.ToString());
                }

                // Anything we didn't upload is fair game to remove
                try {
                    ScreenCapture_CleanupFiles_Do();
                }
                catch (Exception ex) {
                    MQD("ScreenCapture_CleanupFiles_Do() -- Failed: " + ex.ToString());
                }
            });

            return;
        }

        /// <summary>
        /// If we previously had problems uploading screen recordings, we'll try and upload anything we still have locally
        /// </summary>
        public void ScreenCapture_UploadFileBacklog_Do() {
            string screen_recording_storage = GetPath_ScreenRecordingTempStorage();
            string[] files_found_meta       = Directory.GetFiles(screen_recording_storage, "SCREEN-*.meta");

            foreach (string recording_meta_file in files_found_meta) {
                // Example: C:\Users\User\AppData\Local\Temp\SCREEN-1668800525.87115.meta
                MQD("Local Recording, Processing: {0}", recording_meta_file);

                Regex regex   = new Regex(@"(SCREEN\-(\d+\.\d+))\.meta$");
                Match matches = regex.Match(recording_meta_file);
                MatchCollection matched_regex = regex.Matches(recording_meta_file);  

                if (!matches.Success) {
                    MQD("Local Recording, Does not match proper filename format,  Removing: {0}", recording_meta_file);                    
                    File.Delete(recording_meta_file);
                    continue;
                }

                // Actual Metadata 
                string   recording_meta_string = File.ReadAllText(recording_meta_file);
                JsonHash recording_meta_jh     = new JsonHash(recording_meta_string);

                // Video mp4 file: Example: C:\Users\User\AppData\Local\Temp\SCREEN-1668800525.87115.mp4
                string recording_media_file = Path.Combine(screen_recording_storage, matched_regex[0].Groups[1].Value + ".mp4");
                string md5sum               = Utils.File_MD5_Get(recording_media_file);

                // backwards compat
                double RecordingStart_ServerUnixTime = recording_meta_jh.GetDouble("RecordingStart_ServerUnixTime");

                if (RecordingStart_ServerUnixTime == 0) {
                    string RecordingStart_ServerUnixTime_string = matched_regex[0].Groups[2].Value;
                    RecordingStart_ServerUnixTime               = Double.Parse(RecordingStart_ServerUnixTime_string);
                }

                try { 
                    IntellaScreenRecordingMeta recording_meta = new IntellaScreenRecordingMeta{
                        RecordingFilePath               = recording_media_file,
                        RecordingFileMetaPath           = recording_meta_file,
                        MediaHash                       = md5sum,
                        MediaHashType                   = "MD5",
                        CallLogID                       = recording_meta_jh.GetString("call_log_id"),
                        CallSegmentID                   = recording_meta_jh.GetString("call_segment_id"),
                        CallerID_Name                   = recording_meta_jh.GetString("callerid_name"),
                        CallerID_Num                    = recording_meta_jh.GetString("callerid_num"),
                        CallState                       = recording_meta_jh.GetString("call_state"),
                        Queue_LongName                  = recording_meta_jh.GetString("queue_longname"),
                        CaseNumber                      = recording_meta_jh.GetString("case_number"),
                        Channel                         = recording_meta_jh.GetString("channel"),
                        RecordingStart_ServerTimeOffset = recording_meta_jh.GetDouble("RecordingStart_ServerTimeOffset"),
                        RecordingStart_ServerUnixTime   = RecordingStart_ServerUnixTime,
                    };

                    ScreenCapture_UploadFile(recording_meta);
                }
                catch (Exception ex) {
                    MQD("Exception: " + ex.ToString());
                }
            }
        }

        public void ScreenCapture_UploadFile(IntellaScreenRecordingMeta recordingMeta) {
            CanWeRestartAndUpdateSet(false); // Don't allow upgrades while uploading screen recordings

            MQD("File Upload Start (CallLogID: {0} -- CallSegmentID: {1})", recordingMeta.CallLogID, recordingMeta.CallSegmentID);

            string upload_url_endpoint = m_screenRecordingUploadURL + String.Format(
                "?app=Toolbar"
                + "&op=screencast/upload"
                + "&token=abc"
                + "&call_log_id={0}"
                + "&call_segment_id={1}"
                + "&capture_start_unixtime={2}", 

                recordingMeta.CallLogID, 
                recordingMeta.CallSegmentID,
                recordingMeta.RecordingStart_ServerUnixTime.ToString()
            );

            UploadFileToUrl_Result upload_result = Utils.UploadFileToURL_WithRetries(recordingMeta.RecordingFilePath, upload_url_endpoint, m_screenRecordingUploadRetries);

            MQD("File Upload Complete [Result: {0}] (CallLogID: {1} -- CallSegmentID: {2}) -- Attempts: {3}", 
               upload_result.Success.ToString(),
               recordingMeta.CallLogID,
               recordingMeta.CallSegmentID,
               upload_result.Attempts.ToString()
            );

            // No matter what, success or failure.  We must clean up
            File.Delete(recordingMeta.RecordingFilePath);
            File.Delete(recordingMeta.RecordingFileMetaPath);

            CanWeRestartAndUpdateSet(true);
        }

        /// <summary>
        /// If we previously had problems uploading screen recordings, we'll try and upload anything we still have locally
        /// </summary>
        public void ScreenCapture_CleanupFiles_Do() {
            string screen_recording_storage = GetPath_ScreenRecordingTempStorage();
            string[] files_found_meta       = Directory.GetFiles(screen_recording_storage, "SCREEN-*.mp4");

            foreach (string recording_media_file in files_found_meta) {
                // Example: C:\Users\User\AppData\Local\Temp\SCREEN-1668800525.87115.mp4
                MQD("Local Recording, Processing: {0}",                          recording_media_file);
                MQD("Local Recording, MP4 file without metadata, Removing: {0}", recording_media_file);

                File.Delete(recording_media_file);
            }
        }

        private void StartScreenRecordingIfNecessary(QueryResultSetRecord currentCallData, TimeSpan serverTimeOffset)
        {
            if (!this.m_screenRecordingEnabled) {
                MQD("Screen Recording Disabled (Tenant Setting)");
                return;
            }

            if ((m_screenRecordingUploadURL == null) || (m_screenRecordingUploadURL == "")) {
                MQD("Screen Recording Disabled (Tenant Toolbar Screen Recording Upload URL Not Set)");
                return;
            }

            string now_in_seconds             = Utils.DateTimeToUnixTime(DateTime.Now).ToString();
            string screen_recording_file_path = GetPath_ScreenRecordingTempStorage() +  String.Format("SCREEN-{0}.mp4",  now_in_seconds);
            string screen_recording_meta_path = GetPath_ScreenRecordingTempStorage() + String.Format("SCREEN-{0}.meta", now_in_seconds);
            
            JsonHash current_call_data_jh = currentCallData.ToJsohHash();
            File.WriteAllText(screen_recording_meta_path, current_call_data_jh.ToString());

            MQD("Start Screen Recording: " + screen_recording_file_path);

            try
            {
                // Always overwrite for now... TODO, we should see if we still have a file left over and try and re-upload if we failed the last time

                this.m_screenRecord.RecordingStart(screen_recording_file_path,
                    // Callback for when recording is finished.. Runs in the recording thread

                    delegate (IntellaScreenRecordingResult result) {
                        MQD("ScreenCapture -- End Screen Recording: " + screen_recording_file_path);
                        MQD("Capture Start:    " + Utils.DateTimeToUnixTime(result.StartTime).ToString());
                        MQD("ServerTimeOffSet: " + serverTimeOffset.TotalSeconds.ToString());

                        IntellaScreenRecordingMeta recording_meta = new IntellaScreenRecordingMeta{
                            RecordingFilePath               = result.RecordingFilePath,
                            CallLogID                       = currentCallData["call_log_id"],
                            CallSegmentID                   = currentCallData["call_segment_id"],
                            CallerID_Name                   = currentCallData["callerid_name"],
                            CallerID_Num                    = currentCallData["callerid_num"],
                            CallState                       = currentCallData["call_state"],
                            Queue_LongName                  = currentCallData["queue_longname"],
                            CaseNumber                      = currentCallData["case_number"],
                            Channel                         = currentCallData["channel"],
                            RecordingStart_ServerTimeOffset = serverTimeOffset.TotalSeconds,
                            RecordingStart_ServerUnixTime   = (Utils.DateTimeToUnixTime(result.StartTime) + serverTimeOffset.TotalSeconds),
                        };

                        ScreenCapture_UploadFile(recording_meta);
                    }
                );
            }
            catch (Exception ex)
            {
                MQD("File Capture Failed (CallLogID: {0} -- CallSegmentID: {1})", currentCallData["call_log_id"], currentCallData["call_segment_id"] + "\r\n" + ex.ToString());
            }
        }

        // This only gets called if we no longer need screen recording
        // The only reason we'll be stopping screen recording is if we're no longer in a call or the application is ending
        // This does get called every data cycle (generally 2 seconds)
        //
        // FIXME: Make this more event-driven-style
        //
        private void StopScreenRecordingIfNecessary()
        {
            // IsActive == Anything regarding screen recording is currently running
            // Either 
            //  1) Recording
            //  2) Completion callback is running (Our uploader is uploading or is retrying an upload)

            if (this.m_screenRecord.IsActive()) {
                if (this.m_screenRecord.IsRunning()) {
                    // RecordingStop event handler will CanWeRestartAndUpdateSet(true) once we know the file is done writing
                    this.m_screenRecord.RecordingStop();
                    MQD("Stopping");
                }

                return;
            }

            // Completely done with the recording and the recording file
            // Self-Healing (Do we need this?)

            if (!CanWeRestartAndUpdate()) {
                MQD("CanWeRestartAndUpdateSet: true");
                CanWeRestartAndUpdateSet(true);
            }
        }
    }
}
