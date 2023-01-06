using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace Lib {
    [DebuggerDisplay("Count = {Count}, Contents = {InternalQueryResultSet}")]
    public class QueryResultSet : IEnumerable<QueryResultSetRecord> {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly List<QueryResultSetRecord> m_resultSet;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private bool m_queryFailed = false;

        private int m_enumerator_position = -1;

        public QueryResultSet() {
            // If we never populate the results, consider it a failure
            this.m_queryFailed = true;
            this.m_resultSet = new List<QueryResultSetRecord>();
        }

        public QueryResultSet(List<QueryResultSetRecord> resultSet) {
            m_queryFailed = true;
            this.m_resultSet = resultSet;
        }

        public int Count {
            get {
                return m_resultSet.Count;
            }
        }

        public bool Any() {
            return (m_resultSet.Count > 0);
        }

        public QueryResultSet Clear() {
            m_resultSet.Clear();
            return this;
        }

        public QueryResultSet Clone() {
            QueryResultSet query_result_set = new QueryResultSet();

            foreach (QueryResultSetRecord query_result_set_record in query_result_set) {
                query_result_set.Add(query_result_set_record.Clone());
            }

            return query_result_set;
        }

        public bool IsEmpty() {
            return m_resultSet.Count == 0;
        }

        public bool QueryFailed() {
            return m_queryFailed;
        }

        public void SetQuerySuccess() {
            m_queryFailed = false;
        }

        public void Add(QueryResultSetRecord record) {
            this.m_resultSet.Add(record);
        }

        public QueryResultSetRecord this[int index] {
            get {
                if (index > (this.m_resultSet.Count - 1)) {
                    return null;
                }

                if (index < 0) {
                    return null;
                }

                return this.m_resultSet[index];
            }
            set {
                this.m_resultSet[index] = value;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private List<QueryResultSetRecord> InternalQueryResultSet {
            get {
                return m_resultSet;
            }
        }

        public IEnumerator<QueryResultSetRecord> GetEnumerator() {
            foreach (QueryResultSetRecord r in m_resultSet) {
                yield return r;
            }
        }

        public IEnumerable Reverse() {
            int pos = m_resultSet.Count - 1;

            for (; pos > 0; pos--) {
                yield return m_resultSet[pos];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() // Explicitly implement the non-generic version.
        {
            return (IEnumerator) this;
        }

        public bool MoveNext() {
            m_enumerator_position++;
            return (m_enumerator_position < m_resultSet.Count);
        }

        public void Reset() {
            m_enumerator_position = 0;
        }

        public object Current {
            get
            {
                if (m_enumerator_position == -1) { 
                    return null;
                }

                return m_resultSet[m_enumerator_position];
            }
        }

        /// <summary>
        ///
        // Example: result = ConvertToDictionary_String_QueryResultSet("group");
        // 
        // Given a QueryResultSet
        //     [
        //       {group = 'a', data => 111111},
        //       {group = 'a', data => 222222},
        //       {group = 'a', data => 333333},
        //       {group = 'b', data => 444444},
        //       {group = 'b', data => 555555},
        //       {group = 'b', data => 666666},
        //     ]
        //
        // Returns:
        //   {
        //     a => [ {group = 'a', data => 111111}, {group = 'a', data => 222222}, {group = 'a', data => 333333} ]
        //     b => [ {group = 'b', data => 444444}, {group = 'b', data => 555555}, {group = 'b', data => 666666} ]
        //   }
        //     
        //  Now we can access a Dictionary<string> to get at groups of things
        //  
        //  Accessing result["a"]  will give us all the things thart had group=a, as one list
        //
        /// </summary>
        /// <param name="dictionaryColumn"></param>
        /// <returns></returns>
        public Dictionary<string, QueryResultSet> ConvertToDictionary_String_QueryResultSet(string dictionaryColumn) {
            Dictionary<string, QueryResultSet> d_qrs = new Dictionary<string, QueryResultSet>();

            foreach (QueryResultSetRecord r in this) {
                string dictionary_key_value = r[dictionaryColumn];

                // This is the new per-key QueryResultSet
                QueryResultSet qrs;

                if (d_qrs.ContainsKey(dictionary_key_value)) {
                    qrs = d_qrs[dictionary_key_value];
                }
                else {
                    qrs = new QueryResultSet();
                    d_qrs.Add(dictionary_key_value, qrs);
                }

                qrs.Add(r);
            }

            return d_qrs;
        }

        public JsonHash ToJsonHash() {
            JsonHash jh = new JsonHash();

            foreach (QueryResultSetRecord row in this) {
                jh.AddArrayElement(row.ToJsohHash());
            }

            return jh;
        }
    }

    [DebuggerDisplay("Count = {Count}, Contents = {InternalSortedList}")]
    public class  QueryResultSetRecord : IEnumerable {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly Hashtable m_resultSetRecord = new Hashtable();
        
        private SortedList InternalSortedList {
            get {
                SortedList list = new SortedList();

                foreach (string key in m_resultSetRecord.Keys) {
                    list.Add(key, (string) m_resultSetRecord[key]);
                }

                return list;
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

        public QueryResultSetRecord Clone() {
            QueryResultSetRecord query_result_set_record = new QueryResultSetRecord();

            foreach (DictionaryEntry column in m_resultSetRecord) {
                string key   = column.Key.ToString();
                string value;

                if (column.Value != null) {
                  value = column.Value.ToString();
                }
                else {
                    value = null;
                }

                query_result_set_record.Add(key, value);
            }

            return query_result_set_record;
        }

        public void Add(string itemName, string itemValue) {
            m_resultSetRecord.Add(itemName, itemValue);
        }

        public Boolean Exists(string index) {
            return this.m_resultSetRecord.ContainsKey(index);
        }

        public Boolean Contains(string index) {
            return this.m_resultSetRecord.ContainsKey(index);
        }

        public Boolean ContainsKey(string index) {
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

        public int ToInt(string index) {
            if (!this.m_resultSetRecord.ContainsKey(index)) {
                return 0;
            }

            return Int32.Parse(m_resultSetRecord[index].ToString());
        }

        public double ToDouble(string index)
        {
            if (!this.m_resultSetRecord.ContainsKey(index))
            {
                return 0;
            }

            return Double.Parse(m_resultSetRecord[index].ToString());
        }

        /// <summary>
        /// Get a column from the result set and return its string value
        /// </summary>
        /// <param name="index"></param>
        /// <returns>null if the item doesn't exist or is actually null</returns>
        public string ToString(string index) {
            if (!this.m_resultSetRecord.ContainsKey(index)) {
                return null;
            }

            if (this.m_resultSetRecord[index] == null) {
                return null;
            }

            return this.m_resultSetRecord[index].ToString();
        }

        public Boolean ToBoolean(string index) {
            if (!this.m_resultSetRecord.ContainsKey(index)) {
                return false;
            }

            string item = m_resultSetRecord[index].ToString().ToUpper();

            return Utils.StringToBoolean(item);
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

        /// <summary>
        /// Convert QueryResultSetRecord (Essentially a Hashtable) to a JsonHash
        /// </summary>
        /// <returns>JsonHash (Containing a HashTable)</returns>
        public JsonHash ToJsohHash() {
            JsonHash jh = new JsonHash();

            foreach (string column_name in this.Keys()) {
                jh.AddString(column_name, this[column_name]);
            }

            return jh;
        }
    }
}
