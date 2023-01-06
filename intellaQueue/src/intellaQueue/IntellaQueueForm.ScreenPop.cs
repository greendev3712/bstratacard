using Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lib;
using QueueLib;

namespace intellaQueue
{
    public partial class IntellaQueueForm : System.Windows.Forms.Form
    {
        private Boolean ScreenPopIfNecessary(ToolbarServerConnection tsc) {
            if (!tsc.m_isMainServer) {
                // Only screenpops for the main server connection
                return false;
            }

            QueryResultSet backlog_items;

            backlog_items = tsc.m_db.DbSelect(@"
                SELECT
                    *
                FROM
                    live_queue.agents_event_backlog
                WHERE
                    agent_device = ?
                    AND event_when_unixtime > ?::numeric
                ORDER BY
                    event_when",

                m_agentDevice, m_lastAgentBackLogLastUnixTime
            );

            if (backlog_items.Count == 0) {
                return false;
            }

            JsonHash event_additional_data_json;
            JsonHash screenpop_data_json;

            string screen_pop_data_string = "";

            foreach (QueryResultSetRecord backlog_item in backlog_items) {
                m_lastAgentBackLogLastUnixTime = backlog_item["event_when_unixtime"];

                if (backlog_item["event_what"] == "CALL_START") {
                    event_additional_data_json = new JsonHash(backlog_item["event_additional_data_json"]);

                    string caller_channel_queue  = event_additional_data_json.GetStringOrEmpty("QueueCallerChannel");
                    string caller_channel_dialer = event_additional_data_json.GetStringOrEmpty("DialerCalleeChannel");

                    if ((caller_channel_queue == "") && (caller_channel_dialer == "")) {
                        continue;
                    }

                    string caller_channel = (caller_channel_queue != "") ? caller_channel_queue : caller_channel_dialer;

                    // TODO: We need a DbSelectSingleValue
                    screen_pop_data_string = tsc.m_db.DbSelectSingleValueString("SELECT live_queue.agent_screenpop_data(?,?,?) as screenpop_data", backlog_item["backlog_item_id"], m_agentDevice, caller_channel);

                    if (screen_pop_data_string == "") {
                        MQD("SELECT live_queue.agent_screenpop_data({0}','{1}','{2}') as screenpop_data", backlog_item["backlog_item_id"], m_agentDevice, caller_channel);
                        MQD("ScreenPop CALL_START with no active call to: {0}", caller_channel);

                        if (backlog_item.Exists("event_callerid_num") && (backlog_item["event_callerid_num"] != null) && (backlog_item["event_callerid_num"].Length < 10)) {
                            MQD("Note:  ScreenPops might be disabled server-side for less than 10-digit CallerID");
                        }

                        continue;
                    }

//                    if (Debugger.IsAttached) {
//                        DataDumperForm df = new DataDumperForm();
//                        df.Dumper("ScreenPop", screenpop_data);
//                    }

                    screenpop_data_json    = new JsonHash(screen_pop_data_string);
                    screen_pop_data_string = System.Uri.EscapeUriString(screen_pop_data_string);

                    // Full data related to the Screen Pop
                    m_lastScreenPopData = new JsonHash(new Hashtable() { { "screenpop_data", screenpop_data_json.GetHashTable() }, { "event_additional_data_json", event_additional_data_json.GetHashTable() } });
                    MQDU("LastScreenPopData", m_lastScreenPopData);

                    if (!m_screenPopsEnabled || (m_screenPopURL == "") || (m_screenPopURL == null)) {
                       if (!m_screenPopsEnabled) {
                            MQD("ScreenPops are not enabled (Missing Agent License)");
                        }

                       if ((m_screenPopURL == "") || (m_screenPopURL == null)) {
                            MQD("ScreenPops are not enabled (Missing ScreenPopURL)");
                        }

                       return false; // Did everything else, but not screenpoping
                    }

                    // We're in the middle of a call now, we can't restart
                    CanWeRestartAndUpdateSet(false);

                    // MQDU("Screen Pop", screenpop_data_json);
                    MQD("ScreenPop URL: {0}?data={1}", m_screenPopURL, screen_pop_data_string);
                    System.Diagnostics.Process.Start(String.Format("{0}?data={1}", m_screenPopURL, screen_pop_data_string));
                }
            }

            // TODO: Keep this in sync with API Docs /intellasoft/CallQueue/docs and with info in live_queue.agent_screenpop_data
            //
            // The following fields and specs are the same as in GetAgentEventBackLog
            //   event_when                   String -  High precision timestamp of event as an ISO date string (with time zone offset from     UTC), Example: 2000-01-01 10:00:00.123456-04
            //   event_when_unixtime          String -  High precision unixtime of event, Example: 946738800.123456
            //   event_what                   String -  Type of event.  Current possible events include CALL_START/CALL_END
            //   agent_device                 String -  Device (extension) where the agent is logged in from for the agent related to the event
            //   agent_number                 Numeric - Agent Number (Agent Login) for the agent related to the event
            //   agent_id                     Numeric - Unique identifier for the agent related to the event
            //   session_id                   Numeric - Unique session id of the agent's session for the agent related to the event
            //   event_call_log_id            Numeric - Internal Unique call log id of the connected call that the event is related to (if any)
            //   event_case_number            String  - Case id of the related call (if any)
            //   event_callerid_name          String  - Name of the caller related to the event (if any)
            //   event_callerid_num           String  - Callerid number of the caller related to the event (if any)
            //   event_additional_data_json   String  - JSON encoded data related to the event (reserved for future expansion)
            //   event_userfield              String  - Current user defined data that has been attached to the call.  Best practice is to use JSON
            //
            // With the following additional fields:
            //   queue_name                 String  - Queue name token
            //   uniqueid                   Numeric - Related UniqueID of this call.  Internal tracking number
            //   call_log_id                String  - Internal Unique call log id of the connected call that the event is related to (if any)
            //   joined_when                String  - High precision timestamp of event as an ISO date string (with time zone offset from UTC),  Example: 2000-01-01 10:00:00.123456-04
            //   picked_up_when             String  - High precision timestamp of event as an ISO date string (with time zone offset from UTC),  Example: 2000-01-01 10:00:00.123456-04
            //   on_call_with_agent_device  String  - Device (extension) where the agent is logged in from for the agent related to the event
            //   on_call_with_agent_number  Numeric - Agent Number (Agent Login) for the agent related to the event
            //   on_call_with_agent_channel String  - Agent Channel that is/was currently talking to the caller
            //   department_name            String  - Specific department name within the queue that this caller called into
            //   department_num             String  - Specific department number within the queue that this caller called into
            //   queue_id                   Numeric - Unique identifier for the queue
            //   queue_dialer_channel       String  - Internal channel used for the agent dialer
            //   priority                   Numeric - Priority of this queue for this agent
            //   case_number                String  - Case number associated with this call
            //   call_segment_id            String  - Specific Call Segment within the Call Log ID of this call
            //   channel                    String  - Channel of the outside caller
            //   joined_when_unixtime       Numeric - Time in seconds since the "epoch" (standard UnixTime) for when the caller joined the queue
            //   waiting_seconds            Numeric - Time in seconds the caller waited in queue before getting answered by an agent
            //   waiting_time               String  - HH:MM:SS formatted time of how long the caller waited in queue before getting answered by an agent
            //
            // Backwards Compatability
            //   callerid_name               String - Name of the caller related to the event (if any)
            //   callerid_num                String - Callerid number of the caller related to the event (if any)
            //   agent_device                String - Device (extension) where the agent is logged in from for the agent related to the event
            //

            return true;
        }

        private void ScreenPop_LaunchLoginIfNeeded() {
            if (this.m_screenPop_LoginLaunched == true) {
                return;
            }

            this.m_screenPop_LoginLaunched = true;

            if (this.m_screenPopLoginUrl != "") {
                if (Debugger.IsAttached) {
                    MQD("[ScreenPop] -- Launch ScreenPopLoginURL Skipped -- DEV-MODE Debugger Attached");
                }
                else {
                    System.Diagnostics.Process.Start(this.m_screenPopLoginUrl);
                }
            }
        }
    }
}
