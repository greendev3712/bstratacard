using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib;

namespace LibICP
{
    public class JsonQueueLoginLogoutResult
    {
        // ReSharper disable once InconsistentNaming
        public Boolean success { get; set; }

        // ReSharper disable once InconsistentNaming
        public string code { get; set; }

        // ReSharper disable once InconsistentNaming
        public string reason { get; set; }

        // ReSharper disable once InconsistentNaming
        public JsonHash agent_data { get; set; }
    }
}
