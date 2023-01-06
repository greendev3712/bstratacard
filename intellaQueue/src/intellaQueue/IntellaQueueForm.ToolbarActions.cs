using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lib;

namespace intellaQueue
{
    public partial class IntellaQueueForm : System.Windows.Forms.Form
    {
        ////
        // Set our status for all queues we are assigned to
        //
        // WARNING:  In this function: newAgentStatus is expected to be the longname, 
        // TODO: use .Tag on the combobox to store the agent status row so we can pass the status_code_name
        // 
        internal void setNewAgentStatus(string newAgentStatus) {
            MQD("Set Agent Status [Self]: {0}", newAgentStatus);

            try {
                QueryResultSetRecord set_status = m_main_db.DbSelectSingleRow(@"
                    SELECT
                    live_queue.agent_set_status(
                        {0},
                        (SELECT status_code_name FROM live_queue.v_agent_status_codes_self WHERE agent_device = {0} AND status_code_longname = {1} LIMIT 1)
                    )", m_agentDevice, newAgentStatus);

                MQD("Agent Set Status -- Status: {0}, Device: {1} -- Result: {2}", newAgentStatus, m_agentDevice, set_status["agent_set_status"]);
            }
            catch (Exception e) {
                handleError(e, "setNewAgentStatus(x) Failed");
                if (Debugger.IsAttached) { throw; }
            }
        }

        internal void setNewAgentStatus(string newAgentStatus, string agentDevice) {
            MQD("Set Agent Status [{0}]: {1}", agentDevice, newAgentStatus);

            try {
                QueryResultSetRecord set_status = m_main_db.DbSelectSingleRow("SELECT live_queue.agent_set_status(?,?)", agentDevice, newAgentStatus);
                MQD("Agent Set Status -- Status: {0}, Device: {1} -- Result: {2}", newAgentStatus, agentDevice, set_status["agent_set_status"]);
            }
            catch (Exception e) {
                handleError(e, "setNewAgentStatus(x,x) Failed");
                if (Debugger.IsAttached) { throw; }
            }
        }

        // Note: make sure to pass in the *real* queue name, not the prefixed one
        public void setNewAgentStatus(string newAgentStatus, string agentDevice, string queueName) {
            if (agentDevice == null) {
                agentDevice = m_agentDevice;
            }

            MQD("Set Agent Status [{0}]: {1} (Queue: {2}", agentDevice, newAgentStatus, queueName);

            try {
                QueryResultSetRecord set_status = m_main_db.DbSelectSingleRow("SELECT live_queue.agent_set_status(?,?,?)", agentDevice, newAgentStatus, queueName);
                MQD("Agent Set Status -- Status: {0}, Device: {1}, Queue: {2} -- Result: {3}", newAgentStatus, agentDevice, queueName, set_status["agent_set_status"]);
            }
            catch (Exception e) {
                handleError(e, "setNewAgentStatus(x,x,x) Failed");
                if (Debugger.IsAttached) { throw; }
            }
        }
    }
}
