using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace intellaQueue
{
    public partial class IntellaQueueForm
    {
        public string GetTenantName () {
            return IntellaQueueForm.m_tenantName;
        }

        public string GetAgentID () {
            return IntellaQueueForm.m_agentID;
        }

        public string GetAgentNumber () {
            return IntellaQueueForm.m_agentNum;
        }

        public string GetAgentDevice () {
            return IntellaQueueForm.m_agentDevice;
        }
    }
}
