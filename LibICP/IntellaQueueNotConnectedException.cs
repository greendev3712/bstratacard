using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibICP
{
    public class IntellaQueueNotConnectedException : Exception {
        public IntellaQueueNotConnectedException(string message) : base(message) {
            
        }
    }

}
