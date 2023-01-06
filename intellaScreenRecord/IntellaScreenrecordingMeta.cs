using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntellaScreenRecord
{
    // There's something to be said about having definitions ahead of time about what your data looks like
    // This is one step of many that'll be involved in "hardening" the data, so we know what we're working with versus querying everything dynamically from JSON
    public class IntellaScreenRecordingMeta
    {
        public string RecordingFilePath;
        public string RecordingFileMetaPath;

        public string MediaHash;
        public string MediaHashType;

        public string CallLogID;
        public string CallSegmentID;
        public string CallerID_Name;
        public string CallerID_Num;
        public string CallState;
        public string Queue_LongName;
        public string CaseNumber;
        public string Channel;
        public double RecordingStart_ServerUnixTime;
        public double RecordingStart_ServerTimeOffset;
    }
}
