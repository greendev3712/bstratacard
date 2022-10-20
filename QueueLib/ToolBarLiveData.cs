using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using Lib;

namespace QueueLib {
    // A live-data segment containing per-queue live-data
    public class ToolbarLiveDataSegment : IEnumerable {
        // Each ToolbarLiveDataSegment is the entire data for a specific type, for a number of queues
        //
        // Example:
        //  ToolbarLiveDataSegment (Type Caller)
        //    Containing
        //       queue: sales
        //                Caller 1
        //                Caller 2
        //       queue: support
        //                Caller 1
        //                Caller 2
        //
        //
        // each <string> element in m_liveDataSegment is a queue
        // The inner QueryResultSet is all the live data for the queue for this particular segment
        // Segments that can be used: 
        public enum SegmentType {
            Caller,
            Agent,
            Queue,
            Call,
            Status
        };

        private Dictionary<string, QueryResultSet> m_liveDataSegment;
        public readonly SegmentType Type; 

        public ToolbarLiveDataSegment(SegmentType segmentType) {
            // Default empty, so accessors have something non-null to work with before data population
            m_liveDataSegment = new Dictionary<string, QueryResultSet>();  
            this.Type = segmentType;
        }

        public void SetSegmentData(Dictionary<string, QueryResultSet> segmentData) {
            this.m_liveDataSegment = segmentData;
        }

        public int Count {
            get {
                return m_liveDataSegment.Count;
            }
        }

        public bool Any() {
            return (m_liveDataSegment.Count > 0);
        }

        public ToolbarLiveDataSegment Clear() {
            m_liveDataSegment.Clear();
            return this;
        }

        public bool IsEmpty() {
            return m_liveDataSegment.Count == 0;
        }

        public ICollection Keys() {
            return m_liveDataSegment.Keys;
        }

        public ArrayList KeysToArrayList() {
            ArrayList result = new ArrayList();

            foreach (string key_name in this.Keys()) {
                result.Add(key_name);
            }

            return result;
        }

        public IEnumerator GetEnumerator() {
            foreach (string key in m_liveDataSegment.Keys) {
                yield return key;
            }
        }

        public QueryResultSet this[string index] {
            get {
                if (!this.m_liveDataSegment.ContainsKey(index)) {
                    return null;
                }

                return this.m_liveDataSegment[index];
            }
            set {
                this.m_liveDataSegment[index] = value;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private Dictionary<string, QueryResultSet> InternalToolbarLiveDataSegment {
            get {
                return m_liveDataSegment;
            }
        }
    }


//  public class ToolBarLiveData : IEnumerable {

    [DebuggerDisplay("Count = {Count}, Contents = {InternalToolbarLiveDataSegments}")]
    public class ToolBarLiveData : IEnumerable {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private Dictionary<string, Dictionary<string, List<OrderedDictionary>>> m_liveData;

        private ToolbarLiveDataSegment m_callerDataSegment;  // Per-Queue Per-Caller Status
        private ToolbarLiveDataSegment m_agentDataSegment;   // Per-Queue Per-Agent Status
        private ToolbarLiveDataSegment m_queueDataSegment;   // Per-Queue Overall Status
        private ToolbarLiveDataSegment m_callDataSegment;    // Per-Queue  ??
        private ToolbarLiveDataSegment m_statusDataSegment;  // Per-Queue Agent Status?

        private Dictionary<ToolbarLiveDataSegment.SegmentType, ToolbarLiveDataSegment> m_dataSegments;

        //[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        //private bool m_queryFailed = false;

        public ToolBarLiveData() {
            m_callerDataSegment = new ToolbarLiveDataSegment(ToolbarLiveDataSegment.SegmentType.Caller);
            m_agentDataSegment  = new ToolbarLiveDataSegment(ToolbarLiveDataSegment.SegmentType.Agent);
            m_queueDataSegment  = new ToolbarLiveDataSegment(ToolbarLiveDataSegment.SegmentType.Queue);
            m_callDataSegment   = new ToolbarLiveDataSegment(ToolbarLiveDataSegment.SegmentType.Call);
            m_statusDataSegment = new ToolbarLiveDataSegment(ToolbarLiveDataSegment.SegmentType.Status);

            m_dataSegments = new Dictionary<ToolbarLiveDataSegment.SegmentType, ToolbarLiveDataSegment>();
            m_dataSegments.Add(m_callerDataSegment.Type, m_callerDataSegment);
            m_dataSegments.Add(m_agentDataSegment.Type, m_agentDataSegment);
            m_dataSegments.Add(m_queueDataSegment.Type, m_queueDataSegment);
            m_dataSegments.Add(m_callDataSegment.Type, m_callDataSegment);
            m_dataSegments.Add(m_statusDataSegment.Type, m_statusDataSegment);

            //////
            // Old

            m_liveData = new Dictionary<string, Dictionary<string, List<OrderedDictionary>>>();
            m_liveData.Add("caller", new Dictionary<string, List<OrderedDictionary>>());
            m_liveData.Add("agent",  new Dictionary<string, List<OrderedDictionary>>());
            m_liveData.Add("queue",  new Dictionary<string, List<OrderedDictionary>>());
            m_liveData.Add("call",   new Dictionary<string, List<OrderedDictionary>>());
            m_liveData.Add("status", new Dictionary<string, List<OrderedDictionary>>());
        }

        /// <summary>
        ///   Take the given newLiveData and merge it into our existing liveData store
        /// </summary>
        /// <param name="newLiveData"></param>
        ///
        public void Merge (ToolBarLiveData newLiveData) {
             foreach (ToolbarLiveDataSegment new_live_data_segment in newLiveData) {
                 // Segment from the current data that will be replaced/updated
                 ToolbarLiveDataSegment cur_live_data_segment = this[new_live_data_segment.Type];

                 foreach (string queue_name in new_live_data_segment.Keys()) {
                     cur_live_data_segment[queue_name] = new_live_data_segment[queue_name];
                 }
             }
        }

        public int Count {
            get {
                return m_dataSegments.Count;
            }
        }

        public bool Any() {
            return (m_dataSegments.Count > 0);
        }

        public ToolBarLiveData Clear() {
            m_liveData.Clear();
            return this;
        }

        public bool IsEmpty() {
            return m_dataSegments.Count == 0;
        }

        public void Add(string queueName, Dictionary<string,List<OrderedDictionary>> item) {
            this.m_liveData.Add(queueName, item);
        }

        public void SetSegmentData(ToolbarLiveDataSegment.SegmentType segmentType, ToolbarLiveDataSegment liveDataSegment) {
            this[segmentType] = liveDataSegment;
        }

        public Dictionary<string, List<OrderedDictionary>> GetData_Caller() {
            return m_liveData["caller"];
        }

        public Dictionary<string, List<OrderedDictionary>> GetData_Agent() {
            return m_liveData["agent"];
        }

        public Dictionary<string, List<OrderedDictionary>> GetData_Queue() {
            return m_liveData["queue"];
        }

        public Dictionary<string, List<OrderedDictionary>> GetData_Call() {
            return m_liveData["call"];
        }

        public Dictionary<string, List<OrderedDictionary>> GetData_Status() {
            return m_liveData["status"];
        }

        ////
        // Add data to the "callers" list
        // Add a block of data specific to a single queue, to the callers list
        //
        public void AddData_Caller(string queueName, List<OrderedDictionary> list) {
            Dictionary<string, List<OrderedDictionary>> caller_data;
            caller_data = m_liveData["caller"];
            caller_data.Add(queueName, list);
        }

        ////
        // Add data to the "agent" list
        // Add a block of data specific to a single queue, to the callers list
        //
        public void AddData_Agent(string queueName, List<OrderedDictionary> list) {
            Dictionary<string, List<OrderedDictionary>> caller_data;
            caller_data = m_liveData["agent"];
            caller_data.Add(queueName, list);
        }

        ////
        // Add data to the "queue" list
        // Add a block of data specific to a single queue, to the callers list
        //
        public void AddData_Queue(string queueName, List<OrderedDictionary> list) {
            Dictionary<string, List<OrderedDictionary>> caller_data;
            caller_data = m_liveData["queue"];
            caller_data.Add(queueName, list);
        }

        ////
        // Add data to the "call" list
        // Add a block of data specific to a single queue, to the callers list
        //
        public void AddData_Call(string queueName, List<OrderedDictionary> list) {
            Dictionary<string, List<OrderedDictionary>> caller_data;
            caller_data = m_liveData["call"];
            caller_data.Add(queueName, list);
        }

        ////
        // Add data to the "status" list
        // Add a block of data specific to a single queue, to the callers list
        //
        public void AddData_Status(string queueName, List<OrderedDictionary> list) {
            Dictionary<string, List<OrderedDictionary>> caller_data;
            caller_data = m_liveData["status"];
            caller_data.Add(queueName, list);
        }
        
        public ICollection Keys() {
            return m_dataSegments.Keys;
        }

        public ArrayList KeysToArrayList() {
            ArrayList result = new ArrayList();

            foreach (string key_name in this.Keys()) {
                result.Add(key_name);
            }

            return result;
        }

        public IEnumerator GetEnumerator() {
            foreach (ToolbarLiveDataSegment.SegmentType segmentType in m_dataSegments.Keys) {
                yield return segmentType;
            }
        }

        public Boolean ContainsSegment(ToolbarLiveDataSegment.SegmentType segmentType) {
            return m_dataSegments.ContainsKey(segmentType);
        }

        public ToolbarLiveDataSegment this[ToolbarLiveDataSegment.SegmentType index] {
            get {
                if (!this.m_dataSegments.ContainsKey(index)) {
                    return null;
                }

                return this.m_dataSegments[index];
            }
            set {
                if (!this.ContainsSegment(index)) {
                    throw new IndexOutOfRangeException();
                }

                this.m_dataSegments[index] = value;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private Dictionary<ToolbarLiveDataSegment.SegmentType, ToolbarLiveDataSegment> InternalToolbarLiveDataSegments {
            get {
                return m_dataSegments;
            }
        }
    }

    /*
    [DebuggerDisplay("Count = {Count}, Contents = {InternalHashTable}")]
    public class  QueryResultSetRecord : IEnumerable {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly Hashtable m_resultSetRecord = new Hashtable();
        
        private Hashtable InternalHashTable {
            get {
                return m_resultSetRecord;
            }
        }

        public QueryResultSetRecord() {
        }

        public QueryResultSetRecord(Hashtable resultSetHashRecord) {
            this.m_resultSetRecord = resultSetHashRecord;
        }

        public int Count {
            get {
                return m_resultSetRecord.Count;
            }
        }

        public void Add(string itemName, string itemValue) {
            m_resultSetRecord.Add(itemName, itemValue);
        }

        public Boolean Exists(string index) {
            return this.m_resultSetRecord.ContainsKey(index);
        }

        public Boolean IsNull(string index) {
            if (!this.m_resultSetRecord.ContainsKey(index)) {
                return true;
            }

            if (this.m_resultSetRecord[index] == null) {
                return true;
            }

            // Exists and is non-null
            return false;
        }

        public Boolean ToBoolean(string index) {
            if (!this.m_resultSetRecord.ContainsKey(index)) {
                return false;
            }

            string item = m_resultSetRecord[index].ToString().ToUpper();

            switch (item) {
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

        public string this[string index] {
            get {
                if (!this.m_resultSetRecord.ContainsKey(index)) {
                    return "";
                }

                if (this.m_resultSetRecord[index] == null) {
                    return "";
                }

                return this.m_resultSetRecord[index].ToString();
            }
            set {
                this.m_resultSetRecord[index] = value;
            }
        }

        public int? ItemInt(string index) {
            if (!this.m_resultSetRecord.ContainsKey(index)) {
                return null;
            }

            int val;
            int.TryParse(m_resultSetRecord[index].ToString(), out val);
            return val;
        }

        public ICollection Keys() {
            return this.m_resultSetRecord.Keys;
        }

        public ArrayList KeysToArrayList() {
            ArrayList result = new ArrayList();

            foreach (string key_name in this.Keys()) {
                result.Add(key_name);
            }

            return result;
        }

        public IEnumerator GetEnumerator() {
            foreach (string key in m_resultSetRecord.Keys) {
                yield return key;
            }
        }

        internal class HashtableDebugView {
            private Hashtable hashtable;
            // public const string TestStringProxy = "This should appear in the debug window.";

            // The constructor for the type proxy class must have a 
            // constructor that takes the target type as a parameter.
            public HashtableDebugView(Hashtable hashtable) {
                this.hashtable = hashtable;
            }
        }
    }
     */
}

