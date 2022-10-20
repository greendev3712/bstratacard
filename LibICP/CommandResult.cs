using Lib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibICP
{
    // <summary>
    //  Results from an IntellaQueueControl command 
    // </summary>
    public class CommandResult {
        // ReSharper disable once InconsistentNaming
        public Boolean Success = false;
        // ReSharper disable once InconsistentNaming
        public string Code = "NO_DATA";
        // ReSharper disable once InconsistentNaming
        public string Msg = "FAILURE: NO_DATA";
        // ReSharper disable once InconsistentNaming
        public JsonHash CmdData;
        // ReSharper disable once InconsistentNaming
        public JsonHash ResultFullJson;
        // ReSharper disable once InconsistentNaming
        public List<OrderedDictionary> ResultSet = new List<OrderedDictionary>();

        // 
        public CommandResult(List<OrderedDictionary> commandResult) {
            // Expecting JSON as single row, in single column called 'result'
            // Expecting fields: code, result, error/msg/message, data

            if (commandResult.Count <= 0) {
                this.ResultFullJson = new JsonHash();
                this.CmdData = new JsonHash();
                return;
            }

            string json = (string) commandResult[0]["result"];

            this.ResultFullJson = new JsonHash(json);
            this.Code           = this.ResultFullJson.GetString("code");
            this.Success        = this.ResultFullJson.GetBool("result");
            this.CmdData        = this.ResultFullJson.GetHash("data");
            this.Msg            = (this.ResultFullJson.GetString("error") ?? this.ResultFullJson.GetString("msg")) ?? this.ResultFullJson.GetString("message");
        }

        public CommandResult(QueryResultSet commandResult) {
            // Expecting JSON as single row, in single column called 'result'
            // Expecting fields: code, result, error/msg/message, data

            if (commandResult.Count <= 0) {
                this.ResultFullJson = new JsonHash();
                this.CmdData = new JsonHash();
                return;
            }

            string json = (string)commandResult[0]["result"];

            this.ResultFullJson = new JsonHash(json);
            this.Code = this.ResultFullJson.GetString("code");
            this.Success = this.ResultFullJson.GetBool("result");
            this.CmdData = this.ResultFullJson.GetHash("data");
            this.Msg = (this.ResultFullJson.GetString("error") ?? this.ResultFullJson.GetString("msg")) ?? this.ResultFullJson.GetString("message");
        }
    }
}
