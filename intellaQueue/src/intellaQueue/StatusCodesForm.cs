using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using intellaQueue;
using KellermanSoftware.CompareNetObjects;
using System.Linq;
using Lib;

namespace QueueLib {
    public partial class StatusCodesForm : Form {
        private IntellaQueueForm m_intellaQueueForm;

        public bool m_statusPopulated = false;

        private bool m_statusIsUpdating;
        private bool m_FirstAgentStatusUpdate = true;
        private bool m_multiQueueStatus       = false;
        private bool m_statusCodesHide        = false;
        private int  m_fullStatusCodesHeight  = 0;     // For Hide/Show
        private bool m_initCollapsed          = true;  // When in multi-queue mode, initially show everything as collapsed;
        private bool m_statusIsDroppedDown    = false;

        // Indexed By [queue_name][X] = status detail
        // Each queue name has a list of statuses, in order
        //
        private Dictionary<string, QueryResultSet> m_agentStatusLast = new Dictionary<string, QueryResultSet>();
        
        // Index of [queue_name][status_longname] = status_code_name
        //   so that a status longname can point us directly to a status_code_name
        private Dictionary<string, Dictionary<string, string>> m_agentStatusIndex;

        private int m_statusControlsHeight_Hideable; // The collapsible height of all controls (the amount of space that goes away when we hide)
        private Hashtable m_cmpComboBoxes; // <string, ComboBox>
        
        private void StatusCodesFormConstruct() {
            m_statusIsUpdating = true; // So we don't trigger any set-status when we're populating the list

            InitializeComponent();

            cmpStatusComboBox.DrawMode = DrawMode.OwnerDrawVariable;

            //if (m_multiQueueStatus) {
            //    HideStatusCodes();
            //}

            m_statusIsUpdating = false;
        }

        public StatusCodesForm() {
            StatusCodesFormConstruct();
        }

        // subscribedQueues is the grid data for the queues
        public StatusCodesForm(SortedList<string, Hashtable> subscribedQueues, bool multiQueueStatusMode) {
            StatusCodesFormConstruct();

            if (multiQueueStatusMode == false) {
                return;
            }

            // Multi-Status (Show multiple Queue-Status ComboBox controls
            // Manager Only for now

            m_multiQueueStatus = multiQueueStatusMode;
            cmpCurrentStatusLabel.Dispose();
            
            const int extra_pixel_width_for_queue_labels = 75;
            
            // Start out with the same style/position as the single status control
            ComboBox c = this.CopyStatusComboBox(cmpStatusComboBox);
            cmpStatusComboBox.Dispose(); // Get rid of our 'Template' combo box that's part of the Form

            // Debug
            // cmpPauseCodesPanel.BackColor   = Color.Red;
            // cmpPauseCodesPanel.BorderStyle = BorderStyle.Fixed3D;
            //

            //cmpPauseCodesPanel.Controls.Add(hide_show_button);

            cmpHideShowPanel.Location = new Point(0,0); // Ignore the location that's on the design forum
            cmpPauseCodesPanel.Controls.Add(cmpHideShowPanel);

            // Debug
            //cmpHideShowPanel.BorderStyle = BorderStyle.Fixed3D;
            //cmpHideShowPanel.BackColor   = Color.Blue;
            //

            m_fullStatusCodesHeight = cmpPauseCodesPanel.Height;

            ////////////////////////////////
            // Start Drawing Status controls

            int start_drawing_pause_at_y = cmpHideShowButton.Location.Y + (cmpHideShowButton.Height / 2);

            // First status location is going to be right below the Hide/Show
            //c.Location = new Point(cmpHideShowButton.Location.X + extra_pixel_width_for_queue_labels, cmpHideShowButton.Location.Y + cmpHideShowButton.Height); 
            c.Location = new Point(cmpHideShowButton.Location.X + extra_pixel_width_for_queue_labels, start_drawing_pause_at_y); 

            // Start out with the same style/position as the single status label
            Label previous_queue_label    = cmpCurrentStatusLabel;
            previous_queue_label.Location = new Point(cmpCurrentStatusLabel.Location.X, start_drawing_pause_at_y); // We use + c.Height to move down one row
            previous_queue_label.Width   += extra_pixel_width_for_queue_labels;

            cmpPauseCodesPanel.Width += extra_pixel_width_for_queue_labels;

            this.m_cmpComboBoxes = new Hashtable();
            var cmp_queue_labels = new Hashtable();

            // var ordered_subscribedqueues = subscribedQueues.Select(x["queue_longname"] => x[""]).OrderBy(x => x.Key);

            /*
             * For managers, adding a set all 'queue'
            Dictionary<string, Hashtable> ordered_queues = new Dictionary<string,Hashtable>();
            foreach (KeyValuePair<string, Hashtable> subscribed_queue in subscribedQueues) {
                ordered_queues.Add(subscribed_queue.Key, subscribed_queue.Value);
            }
            ordered_queues.Add("All Queues", new Hashtable());

            ordered_queues["All Queues"]["HeadingText"] = "zzzz"; // Not ideal, quick hack to put this at the bottom of the list
            ordered_queues.OrderBy(x => x.Value["HeadingText"]);
            ordered_queues["All Queues"]["HeadingText"] = "All Queues"; // Put in a proper heading
            */
            
            ////
            // Draw "Status:" label for each Queue
            //
            // Right now: c = Template for building each Queue Label

            foreach (KeyValuePair<string, Hashtable> subscribed_queue in subscribedQueues) {
                string queue_name    = subscribed_queue.Key;
                Hashtable queue_data = subscribed_queue.Value;

                // TEMP Until we support shared-queues
                Boolean is_main_system     = (Boolean) queue_data["IsMainSystem"];
                Boolean is_queue_aggregate = (Boolean) queue_data["QueueIsAggregate"];

                if (!is_main_system) {
                    continue;
                }

                if (is_queue_aggregate) {
                    // Agents are not 'logged in' to queue aggregates.  Can't change a pause status for an agg
                    continue;
                }

                c = CopyStatusComboBox(c);
                c.Location = new Point(c.Location.X, (c.Location.Y + c.Height)); // Move down one line

                // So we can get the queue_name when changing individual queue status
                queue_data.Add("queue_name", queue_name);
                c.Tag = queue_data;

                Label queue_label = new Label {
                    Text      = "Status: " + (string) queue_data["HeadingText"],
                    Font      = previous_queue_label.Font,
                    ForeColor = previous_queue_label.ForeColor,
                    Location  = new Point(previous_queue_label.Location.X, (previous_queue_label.Location.Y + c.Height)),
                    Size      = previous_queue_label.Size
                };
                cmp_queue_labels.Add(queue_name, queue_label);

                previous_queue_label = queue_label;

                Debug.Print("new queue label: " + queue_label.Text + " Location: " + queue_label.Location.Y);

                this.m_cmpComboBoxes.Add(queue_name, c);

                if (this.m_initCollapsed) { 
                  queue_label.Hide();
                  c.Hide();
                }

                cmpPauseCodesPanel.Controls.Add(queue_label);
                cmpPauseCodesPanel.Controls.Add(c);

                m_fullStatusCodesHeight         += c.Height; // Save the height for show/hide
                m_statusControlsHeight_Hideable += c.Height;
            }

            if (this.m_initCollapsed) {
               // Currently hidden
               this.m_statusCodesHide = true;
            }
            else {
                cmpPauseCodesPanel.Height = m_fullStatusCodesHeight;
            }
                         
            /*
            // For Managers: Add a 'Set All'
            c = CopyStatusComboBox(c);
            c.Location = new Point(c.Location.X, (c.Location.Y + c.Height)); // Move down one line

            Label set_all_label = new Label {
                Text = "Status: All Queues",
                Font = previous_queue_label.Font,
                ForeColor = previous_queue_label.ForeColor,
                Location = new Point(previous_queue_label.Location.X, (previous_queue_label.Location.Y + c.Height)),
                Size = previous_queue_label.Size
            };
            cmp_queue_labels.Add("__all_queues", "All Queues");

            this.m_cmpComboBoxes.Add("__all_queues", c);

            cmpPauseCodesPanel.Controls.Add(set_all_label);
            cmpPauseCodesPanel.Controls.Add(c);

            cmpPauseCodesPanel.Height += c.Height += c.Height;
            // End of 'Set All'
            */

            cmpHideShowButton.Click += delegate(object sender, EventArgs args) {
                int total_queue_controls_height = (this.m_cmpComboBoxes.Count * c.Height);

                if (!this.m_statusCodesHide) {
                    // HIDE
                    // HideStatusCodes(sender); // NEW -- Trying to make this work
                    // return;

                    this.m_statusCodesHide = true;

                    foreach (string queue_name in this.m_cmpComboBoxes.Keys) {
                        ComboBox _c = (ComboBox) this.m_cmpComboBoxes[queue_name];
                        _c.Hide();
                    }

                    foreach (string queue_name in cmp_queue_labels.Keys) {
                        Label _l = (Label)cmp_queue_labels[queue_name];
                        _l.Hide();
                    }

                    this.m_intellaQueueForm.m_extraControlHeight -= total_queue_controls_height;
                    this.m_intellaQueueForm.resize(new Size(this.m_intellaQueueForm.Width, this.m_intellaQueueForm.Height - total_queue_controls_height));

                    return;
                }

                // SHOW

                foreach (string queue_name in this.m_cmpComboBoxes.Keys) {
                    ComboBox _c = (ComboBox)this.m_cmpComboBoxes[queue_name];
                    _c.Show();
                }

                foreach (string queue_name in cmp_queue_labels.Keys) {
                    Label _l = (Label)cmp_queue_labels[queue_name];
                    _l.Show();
                }

                // Restore original height
                int bottom_padding_y = 10; // Space at the very bottom of the main window
                this.m_intellaQueueForm.m_extraControlHeight += total_queue_controls_height + bottom_padding_y;
                this.m_intellaQueueForm.resize(new Size(this.m_intellaQueueForm.Width, this.m_intellaQueueForm.Height + total_queue_controls_height + bottom_padding_y));

                // No longer hidden
                this.m_statusCodesHide = false; 
            };
        }

        //private void HideStatusCodes(object sender) {
        //    Button hide_show_button = (Button) sender;

        //    int new_button_location_y;
        //    int total_queue_controls_height = (this.m_cmpComboBoxes.Count + 1) * c.Height;

        //    new_button_location_y = hide_show_button.Location.Y - ((this.m_cmpComboBoxes.Count) * c.Height);

        //    cmpPauseCodesPanel.Height = hide_show_button.Height + 10;
        //    this.m_statusCodesHide = true;

        //    hide_show_button.Location = new Point(hide_show_button.Location.X, new_button_location_y);

        //    foreach (string queue_name in this.m_cmpComboBoxes.Keys) {
        //        ComboBox _c = (ComboBox)this.m_cmpComboBoxes[queue_name];
        //        _c.Hide();
        //    }

        //    foreach (string queue_name in cmp_queue_labels.Keys) {
        //        Label _l = (Label)cmp_queue_labels[queue_name];
        //        _l.Hide();
        //    }

        //    this.m_intellaQueueForm.m_extraControlHeight -= total_queue_controls_height;
        //    this.m_intellaQueueForm.resize(new Size(this.m_intellaQueueForm.Width, this.m_intellaQueueForm.Height - total_queue_controls_height));
        //}

        private ComboBox CopyStatusComboBox(ComboBox srcComboBox) {
            ComboBox c = new ComboBox();

            c.Location              = new Point(srcComboBox.Location.X, srcComboBox.Location.Y);
            c.SelectedIndexChanged += cmpStatusComboBox_SelectedIndexChanged;
            c.DrawItem             += cmpStatusComboBox_DrawItem;
            c.DropDown             += cmpStatusComboBox_DropDown;
            c.DropDownClosed       += cmpStatusComboBox_DropDownClosed;
            c.Size                  = new Size(srcComboBox.Size.Width, srcComboBox.Size.Height);
            c.DrawMode              = DrawMode.OwnerDrawVariable;
            c.FormattingEnabled     = true;
            c.DropDownStyle         = System.Windows.Forms.ComboBoxStyle.DropDownList;

            return c;
        }

        public void SetIntellaQueueForm(IntellaQueueForm intellaQueueForm) {
            this.m_intellaQueueForm = intellaQueueForm;
        }

        public Panel GetPauseCodesPanel() {
            return this.cmpPauseCodesPanel;
        }

        public void SetStatusUpdating(bool val) {
            this.m_statusIsUpdating = val;

            if (this.m_multiQueueStatus) {
                foreach (ComboBox queue_status_combo in this.m_cmpComboBoxes.Values) {
                    queue_status_combo.Enabled = !val;
                }

                return;
            }

            // (If new val = true: Don't allow users to mokey with the combobox while it's updating
            this.cmpStatusComboBox.Enabled = !val;
        }

        private void cmpStatusComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            var c = (ComboBox) sender;

            if (m_intellaQueueForm == null) {
                return;
            }

            if (m_statusIsUpdating) {
                return;
            }

            //if (!m_statusPopulated) {
            //    return;
            //}

            c.Enabled = false; // Gets re-enabled in the gui update thread when we have the actual status

            // Flag our current status, so that when something comes back we can check for differences
            // Ex: 
            //  Our current status is Available
            //  We submit a new status "Break"
            //  Next update comes back, that our current status is still Available (perhaps a manager just manually set us Available at the same time we requested Break
            //  If we don't set our local copy that it's the current status, then it'll come back as if nothing changed, and then when UpdateAgentStatusIfNeeded, 
            //    it will detect no changes and not show our true current status
            //
            //

            string selected_status_code_longname = (string) c.SelectedItem;

            // FIXME: Major Hack... we shouldn't have to hard code this
            if (selected_status_code_longname == "Not Logged In") {
                return;
            }

            if (this.m_multiQueueStatus) {
                var subscribed_queue_info = (Hashtable) c.Tag;
                
                //this.cmpStatusComboBox.SelectedIndex = 0;
                //this.cmpStatusComboBox.SelectedValue = "<Status Updating>";

                string queue_name = (string) subscribed_queue_info["QueueNameReal"]; // Don't use the predfixed queue name since we're sending this directly to the server
                string status_code_name = m_agentStatusIndex[queue_name][selected_status_code_longname];
                m_intellaQueueForm.setNewAgentStatus(status_code_name, null, queue_name);

                /*
                m_agentStatusLast[queue_name][0]["is_current_status"] = "True";
                */
            }
            else {
                m_intellaQueueForm.setNewAgentStatus(selected_status_code_longname);

                /*
                ICollection agent_status_last_queues = m_agentStatusLast.Keys;

                foreach (string queue_name in agent_status_last_queues) {
                    foreach (OrderedDictionary status_item in m_agentStatusLast[queue_name]) {
                        if ((string) status_item["status_code_longname"] == selected_status_code_longname) {
                            status_item["is_current_status"] = "True";
                        }
                    }
                }
                */
            }
            
            // This and the commented stuf above not working as intended... we get a 'flicker' effect
            // When the next result set that comes back is not the new updated status list and is instead a status from prior to when our 
            // update threads got the new data
            //this.SetStatusUpdating(true);
        }

        protected void cmpStatusComboBox_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index < 0) {
                return;
            }

            var combo = sender as ComboBox;
            if (combo == null) {
                return;
            }

            if (!combo.DroppedDown) {
                // Closed and status: available
                // Draw regular looking combobox, white background + black text

                if (combo.Enabled == false) {
                    e.Graphics.FillRectangle(new SolidBrush(SystemColors.Window), e.Bounds);
                    return;
                }

                if (combo.Items[e.Index].ToString() == "Available") {
                    e.Graphics.FillRectangle(new SolidBrush(Color.LightGreen), e.Bounds);

                    e.Graphics.DrawString(combo.Items[e.Index].ToString(),
                        e.Font,
                        new SolidBrush(Color.Black),
                        new Point(e.Bounds.X, e.Bounds.Y));

                }
                else {
                    // Closed and status: not available
                    // Draw red background + black text

                    e.Graphics.FillRectangle(new SolidBrush(Color.Red), e.Bounds);

                    e.Graphics.DrawString(combo.Items[e.Index].ToString(),
                        e.Font,
                        new SolidBrush(Color.Black),
                        new Point(e.Bounds.X, e.Bounds.Y));
                }

                return;
            }

            // Open and item is selected
            // Draw regular white text + selectedcolor background

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
                e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds);

                e.Graphics.DrawString(combo.Items[e.Index].ToString(),
                    e.Font,
                    new SolidBrush(Color.White),
                    new Point(e.Bounds.X, e.Bounds.Y));

            }
            else {
                // Open and item is not selected
                // Draw regular white background with black text

                e.Graphics.FillRectangle(new SolidBrush(SystemColors.Window), e.Bounds);

                e.Graphics.DrawString(combo.Items[e.Index].ToString(),
                    e.Font,
                    new SolidBrush(Color.Black),
                    new Point(e.Bounds.X, e.Bounds.Y));
            }
        }

        public Boolean UpdateAgentStatusIfNeeded(Dictionary<string, List<OrderedDictionary>> _newAgentStatus) {
            if (m_statusIsDroppedDown) {
                // Avoid updating if someone's using the dropdown!
                return true;
            }

            // list of status codes indexed per-queue
            var newAgentStatus = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(_newAgentStatus);
            ComboBox agent_status_combobox = this.cmpStatusComboBox;
            
            // Dictionary<string, QueryResultSet> q_new = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(newAgentStatus);
            // Dictionary<string, QueryResultSet> q_old = DbHelper.ConvertDictionaryString_ListOrderedDictionary_To_DictionaryString_QueryResultSet(m_agentStatusLast);

            // If FirstAgentStatusUpdate is true, skip the early bailout because we need to populate our list
            // Without this check, If we start the toolbar, and we have no status (not loged in),
            //  then an empty last status and an empty new status will result in thinking we're unchanged
            //  and then we'll wind up doing nothing
            if (!m_FirstAgentStatusUpdate) {
                CompareLogic compare_objects = new CompareLogic();
                ComparisonResult cresult     = compare_objects.Compare(m_agentStatusLast, newAgentStatus);

                if (cresult.AreEqual) {
                    // Unchanged
                    return false;
                }
            }

            // We are updating the status, do not fire event handlers for agent SetStatus
            this.SetStatusUpdating(true);

            m_FirstAgentStatusUpdate = false;

            if (!m_multiQueueStatus) {
                // Repopulate agent status listbox
                this.SetStatusUpdating(true); // Don't trigger SelectedIndex changes

                ComboBox c = agent_status_combobox;

                c.Enabled = true; // Re-enable combobox after a previously executed status change
                c.Items.Clear();
                c.SelectedIndex = -1;

                // No statuses available, we're not logged into any queues
                if (newAgentStatus.Count() == 0) {
                    c.Items.Add("Not Logged In");
                    c.SelectedIndex = 0;
                    c.Enabled = false;
                    c.BackColor = System.Drawing.Color.Red;
                    c.ForeColor = System.Drawing.Color.White;
                    goto done;
                }
            }
           
            // Showing status for individual queues!
            // TODO: FIXME: For now we assume the status is the same for all queues

            string selected_status = "";

            if (m_multiQueueStatus) {
                // Populate 'Not logged in' to appropriate boxes
                foreach (string queue_name in this.m_cmpComboBoxes.Keys) {
                    if (newAgentStatus.ContainsKey(queue_name)) { continue;  }

                    // Queue does not exist in the newly given status list, we're not a part of it anymore

                    ComboBox c = (ComboBox) this.m_cmpComboBoxes[queue_name];
                    c.Items.Clear();
                    c.Items.Add("Not Logged In");
                    c.SelectedIndex = 0;
                    c.Enabled = false;
                    c.BackColor = System.Drawing.Color.Red;
                    c.ForeColor = System.Drawing.Color.White;
                }
            }

            // Handle populating all of the status codes we need to display
            this.m_agentStatusIndex = new Dictionary<string, Dictionary<string, string>>();

            foreach (string queue_name in newAgentStatus.Keys) {
                QueryResultSetRecord first_agent_status_item = newAgentStatus[queue_name][0];
                string real_queue_name = first_agent_status_item["queue_name"]; // We always want the actual queue name (Since multisite uses prefixed queue names)

                this.m_agentStatusIndex.Add(real_queue_name, new Dictionary<string, string>()); // Index for mapping status_code_longname to status_code_name

                if (m_multiQueueStatus) {
                    agent_status_combobox = (ComboBox) this.m_cmpComboBoxes[queue_name];
                    if (agent_status_combobox == null) {
                        // We don't have a status box for this queue
                        continue;
                    }

                    agent_status_combobox.Enabled = true;
                    agent_status_combobox.Items.Clear();
                    agent_status_combobox.SelectedIndex = -1;
                }

                if (newAgentStatus[queue_name].Count == 0) {
                    agent_status_combobox.Items.Add("Not Logged In");
                    agent_status_combobox.Enabled = false;
                }

                // Actual insert of the status code
                foreach (QueryResultSetRecord status_item in newAgentStatus[queue_name]) {
                    // ReSharper disable once PossibleNullReferenceException
                    int pos = agent_status_combobox.Items.Add(status_item["status_code_longname"]);

                    Dictionary<string, string> queue_status_items = this.m_agentStatusIndex[real_queue_name];
                    queue_status_items.Add((string) status_item["status_code_longname"], (string) status_item["status_code_name"]);

                    string status = (string) status_item["is_current_status"];

                    if (Utils.StringToBoolean(status)) {
                        agent_status_combobox.SelectedIndex = pos;
                        selected_status = (string) status_item["status_code_name"];
                    }
                }

                if (selected_status == "AVAILABLE") {
                    agent_status_combobox.BackColor = SystemColors.Window;
                }
                else {
                    // Anything other than Available is paused
                    agent_status_combobox.BackColor = System.Drawing.Color.Red;
                }

                // TODO: FIXME: We only show status off of one queue if we're not a manager
                if (!m_multiQueueStatus) {
                    break;                    
                }
            }

        done:
            this.SetStatusUpdating(false); // Let the user change status, no more gui changes

            // Clone the agent status so we have our own copy... otherwise we'll wind up with a pointer from to m_agentStatusLast -> newAgentStatus
            //  and this will result in m_agentStatusLast being always the new one!
            //
            m_agentStatusLast = DbHelper.Clone_DictionaryString_QueryResultSet(newAgentStatus);
            return true;
        }

        private void StatusCodesForm_Load(object sender, EventArgs e) {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e) {

        }

        private void cmpStatusComboBox_DropDown(object sender, EventArgs e) {
            this.m_statusIsDroppedDown = true;
        }

        private void cmpStatusComboBox_DropDownClosed(object sender, EventArgs e) {
            this.m_statusIsDroppedDown = false;
        }
    }
}
