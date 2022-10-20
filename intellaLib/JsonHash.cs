using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lib {
    [JsonConverter(typeof(JsonHashSerializer))]
    public class JsonHash : IEnumerable {
        private readonly Hashtable m_json_hash  = null;

        private bool m_valuePresent = false;
        private string m_valueString;
        private string m_json;

        public string LastError;

        public JsonHash(string json) {
            m_json_hash = new Hashtable();
            m_json = json;

            JsonStringToHashTable(json, m_json_hash, null, 0);
            m_valuePresent = true;
        }

        public JsonHash(Hashtable h) {
            m_json_hash = h;
            m_valuePresent = true;
        }

        // Given an a list of hashtable, for convenience just stuff it into a hashtable so we can access it the same way as a hashtable
        public JsonHash(List<Hashtable> l_ht) {
            Hashtable h = new Hashtable();
            int pos = 0;

            foreach (Hashtable ht in l_ht) {
                h[pos.ToString()] = ht;
                pos++;
            }

            m_json_hash = h;
            m_valuePresent = true;
        }

        // construct a JsonHash where nothing exists
        public JsonHash() {
            m_json_hash = new Hashtable();
        }

        public int Count {
            get {
                return m_json_hash.Count;
            }
        }

        public bool Any() {
            return (m_json_hash.Count > 0);
        }

        public JsonHash Clear() {
            m_json_hash.Clear();
            return this;
        }

        public JsonHash Clone() {
            Hashtable cloned_hash = (Hashtable) this.m_json_hash.Clone();
            JsonHash json_hash = new JsonHash(cloned_hash);
            return json_hash;
        }

        public bool IsEmpty() {
            return m_json_hash.Count == 0;
        }

        public object this[string index] {
            get {
                return this.m_json_hash[index];
            }
            set {
                this.m_json_hash[index] = value;
            }
        }

        public IEnumerator GetEnumerator() {
            IOrderedEnumerable<string> ordered_keys = this.m_json_hash.Keys.Cast<string>().OrderBy(c => c);

            foreach (string key in ordered_keys) {
                DictionaryEntry de = new DictionaryEntry(key, this.m_json_hash[key]);
                yield return de;
            }
        }

        ////////////////////////////////

        public Hashtable GetHashTable() {
            return m_json_hash;
        }

        public void AddString(string key, string value) {
            m_json_hash.Add(key, value);
        }

        public QueryResultSetRecord ToQueryResultSetRecord() {
            Hashtable rhash             = this.GetHashTable();
            QueryResultSetRecord record = new QueryResultSetRecord(rhash);

            return record;
        }

        public QueryResultSet ToQueryResultSet() {
            QueryResultSetRecord record;
            QueryResultSet       record_set = new QueryResultSet();
                        
            // We'll be assuming that we have a list of ordered keys, like 0,1,2,3,4...x+1
            // The resulting return will be as if this came from a database lookup, so we'll have QueryResultSetRecord's inside of a QueryResultSet
            foreach (DictionaryEntry item in this) {
                object item_value = item.Value;

                if (item_value.GetType() == typeof(Hashtable)) {
                    Hashtable h = (Hashtable) item_value;

                    record = new QueryResultSetRecord(h);
                    record_set.Add(record);
                    continue;
                }
            }

            return record_set;
        }

        public bool Exists(string item) {
            JsonHash i = this.GetItem(item);
            return (i.Count > 1);
        }

        public bool ValuePresent() {
            return m_valuePresent;
        }


        public JsonHash GetItem(string item) {
            object o = m_json_hash[item];

            var inner_item_ht = o as Hashtable;
            if (inner_item_ht != null) {
                return new JsonHash((Hashtable) inner_item_ht);
            }

            var inner_item_list_ht = o as List<Hashtable>;
            if (inner_item_list_ht != null) {
                return new JsonHash((List<Hashtable>) inner_item_list_ht);
            }

            var inner_item_string = o as string;
            if (inner_item_string != null) {
                JsonHash jh = new JsonHash { m_valueString = inner_item_string, m_valuePresent = true };
                return jh;
            }

            // Nothing found
            return new JsonHash { LastError = "NOT_FOUND" };
        }

        /// <summary>
        /// Do a deep get of a value.
        /// Example: GetItemDeep("foo", "bar", "0", "baz"); // Get the JsonHash->{foo}{bar}[0]{baz} value from the internal JSON
        /// </summary>
        /// <param name="items"></param>
        /// <returns>null on any level of item not found</returns>
        public JsonHash GetItemDeep(params string[] items) {
            JsonHash current_jh = this;

            foreach (string item in items) {
                // Keep going deeper until we can't any more
                current_jh = current_jh.GetItem(item);

                // If this is not a container, then we're done
                // We may have landed on a string value or it's not found at all
                if (current_jh.IsEmpty()) {
                    return current_jh;
                }
            }

            // If we were given no input, nothing found
            this.LastError = "NOT_FOUND";

            return new JsonHash();
        }


        public JsonHash GetHash(string item) {
            object o = m_json_hash[item];

            var hashtable = o as Hashtable;
            if (hashtable == null) {
                return new JsonHash();
            }

            JsonHash jh = new JsonHash(hashtable);
            return jh;
        }

        public string GetString(string item) {
            object o = m_json_hash[item];

            var s = o as string;
            if (s == null) {
                return null;
            }

            return s;
        }

        /// <summary>
        /// If the current JsonHash element is a single string value, we can return the string value
        /// </summary>
        /// <returns></returns>
        public string GetStringValue() {
            return m_valueString;
        }

        /// <summary>
        /// If the current JsonHash element is a boolean value, we can return it directly
        /// </summary>
        /// <returns></returns>
        public bool GetBooleanValue() {
            return Utils.StringToBoolean(m_valueString);
        }

        public int GetIntegerValue() {
            try {
                return int.Parse(m_valueString);
            }
            catch (Exception ex) {
                ex.ToString(); // avoid unused variable warning
            }

            return 0;
        }

        public long GetInt64(string item) {
            object o = m_json_hash[item];

            var s = o as string;
            if (s == null) {
                return 0;
            }

            return Int64.Parse(s);
        }

        public long GetInt64Value() {
            return Int64.Parse(m_valueString);
        }

        public string GetStringOrEmpty(string item) {
            string string_item = GetString(item);
            return string_item ?? "";
        }

        public bool GetBool(string item) {
            object o = m_json_hash[item];

            if (!(o is string)) {
                return false;
            }

            string s = (string)o;
            if (s == null) {
                return false;
            }

            return Utils.StringToBoolean(s);
        }

        private static void JsonStringToHashTable(string json, Hashtable parentJsonHt, JsonTextReader reader, int stackLevel) {
            if (stackLevel == 0) {
                // Console.WriteLine("----------");
            }

            if (reader == null) {
                if (json == null) {
                    return;
                }

                reader = new JsonTextReader(new StringReader(json));
            }

            int stack_level = stackLevel;
            string property_name = null;
            Boolean property_is_array = false;
            List<Hashtable> property_ht_array = new List<Hashtable>();

            while (reader.Read()) {
                string spacer = "";
                for (int i = 0; i < stack_level; i++) {
                    spacer += "  ";
                }

                /*
                if (reader.Value != null)
                    Console.WriteLine("{0} Token: {1}, Value: {2}", spacer, reader.TokenType, reader.Value);
                else
                    Console.WriteLine("{0} Token: {1}", spacer, reader.TokenType);
                */

                if ((reader.Value != null) && (reader.TokenType.ToString() == "PropertyName")) {
                    property_name = reader.Value.ToString();
                    continue;
                }

                if (reader.TokenType.ToString() == "StartObject") {
                    stack_level++;

                    if (property_name == null) {
                        // This is the first item... we are the hashtable
                        continue;
                    }

                    // If we're here, we already have our parent hashtable... we must be in a nested object
                    // We must have already processed a "Token: PropertyName, Value: xxxxx"

                    Hashtable inner_json_ht = new Hashtable();
                    JsonStringToHashTable(null, inner_json_ht, reader, stack_level);

                    if (property_is_array) {
                        property_ht_array.Add(inner_json_ht);
                    }
                    else {
                        parentJsonHt.Add(property_name, inner_json_ht);
                    }

                    stack_level--;
                    continue;
                }

                if (reader.TokenType.ToString() == "StartArray") {
                    property_ht_array = new List<Hashtable>();
                    property_is_array = true;
                    continue;
                }

                if (reader.TokenType.ToString() == "EndArray") {
                    property_is_array = false;

                    if (property_name != null) {
                        // TODO: FIXME -- Should throw a manformed json exception if in strict mode
                        if (parentJsonHt.ContainsKey(property_name)) {
                            parentJsonHt[property_name] = property_ht_array;
                        }
                        else {
                            parentJsonHt.Add(property_name, property_ht_array);
                        }
                    }

                    continue;
                }

                if (reader.TokenType.ToString() == "EndObject") {
                    return;
                }

                // Actual Data Items Processing

                if (property_name == null) {
                    // This should never happen, but this makes resharper happy
                    continue;
                }

                if (reader.Value == null) {
                    parentJsonHt.Add(property_name, null);
                    continue;
                }

                if (reader.TokenType.ToString() == "String") {
                    // TODO: Handle String Array

                    string property_value = reader.Value.ToString();

                    if (parentJsonHt.ContainsKey(property_name)) {
                        // TODO: FIXME: Throw MalformedJSON if in strict mode
                        parentJsonHt[property_name] = property_value;
                    }
                    else {
                        parentJsonHt.Add(property_name, property_value);
                    }

                    continue;
                }

                if (reader.TokenType.ToString() == "Integer") {
                    // TODO: Handle Integer Array

                    string property_value = reader.Value.ToString();
                    parentJsonHt.Add(property_name, property_value);
                    continue;
                }
            }
        }

        public static string StringifyHashTable(Hashtable ht, int stack_level) {
            string spacer = "";
            for (int i = 0; i < stack_level; i++) { spacer += "  "; }

            string result = spacer + "{\r\n";
            spacer += "  ";

            foreach (string hash_item in ht.Keys) {
                var hash_val = ht[hash_item];

                if (hash_val == null) {
                    result += spacer + hash_item + ": " + "[null]\r\n";
                }
                else if (hash_val is string) {
                    result += spacer + hash_item + ": " + hash_val + "\r\n";
                }
                else if (hash_val is Hashtable) {
                    result += spacer + hash_item + ":" + StringifyHashTable((Hashtable)hash_val, stack_level + 1);
                }
            }

            spacer = spacer.Substring(0, spacer.Length - 2);

            return result + spacer + "}\r\n";
        }

        public override string ToString() {
            string output = JsonConvert.SerializeObject(this.m_json_hash, Newtonsoft.Json.Formatting.Indented);
            return output;
            // return StringifyHashTable(this.m_json_hash, 0);
        }

        public string ToJson() {
            return this.ToString();
        }

        public ICollection Keys() {
            return this.m_json_hash.Keys;
        }
    }

    // Our 'Standard' for returning results from API type calls
    public class JsonHashResult {
        public Boolean  Success  { get; set; } = false;
        public string   Code     { get; set; } = "UNKNOWN";
        public string   Reason   { get; set; } = "A result has not been stored";
        public JsonHash Data     { get; set; } = new JsonHash();

        public override string ToString() {
            Hashtable ht = new Hashtable();
            ht.Add("success", this.Success);
            ht.Add("code",    this.Code);
            ht.Add("reason",  this.Reason);
            ht.Add("data",    this.Data);

            JsonHash jh = new JsonHash(ht);
            return jh.ToString();
        }
    }

    public class JsonHashSerializer : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var json_hash = value as JsonHash;


            writer.WriteStartObject(); // Start of a Hash/Object

            foreach (DictionaryEntry de in json_hash) {
                string key_name = de.Key.ToString();
                object key_value = de.Value;

                writer.WritePropertyName(key_name); // Properties within the object
                serializer.Serialize(writer, key_value);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) {
            return typeof(JsonHash).IsAssignableFrom(objectType);
        }
    }
}


