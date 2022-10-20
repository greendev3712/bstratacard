using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Lib;
using log4net;

namespace IasPbxConfig
{
	public partial class TraceViewer : LiveDataViewer
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static DbHelper m_db;

		private const string kTimestamp = "Timestamp";
		private const string kChannel = "Channel";
		private const string kModule = "Module";
		private const string kMessage = "Message";
		private const string kLogLevel = "LogLevel";

		private const string kCallerId = "CallerIdName";
		private const string kCallerIdNumber = "CallerIdNumber";
		private const string kExtraData = "ExtraData";

		private Timer m_timer;
		private Timer m_timer2;

		// random data from another class for testing
		private static Hashtable ExtraData = new Hashtable()
		{
			{"name", "Extension Details General Options"},
			{"HiddenRows", new List<Object> {"device_id"}},
			{"changeTableOnDataInField", new Hashtable()
				{
					{"Type", new Hashtable()
						{
							{"SIP", new List<Object> {"Extension Details Device Options", "Asterisk.v_devices_sip_attributes"}},
							{"IAX2", new List<Object> {"Extension Details Device Options", "Asterisk.v_devices_iax_attributes"}}
						}
					},
					{"Voicemail", new Hashtable()
						{
							{"yes", new List<Object> {"Extension Details Voicemail Options", "Asterisk.v_voicemail"}},
							{"no", new List<Object> {"Extension Details Voicemail Options", null}}
						}
					}
				}
			}
		};
		
		public TraceViewer(DbHelper db)
		{
			InitializeComponent();
			m_db = db;

			m_timer = new Timer();
			m_timer.Interval = 1 * 1000;
			m_timer.Tick += new EventHandler(m_timer_Tick);
			m_timer.Start();

			m_timer2 = new Timer();
			m_timer2.Interval = 10 * 1000;
			m_timer2.Tick += new EventHandler(m_timer2_Tick);
			m_timer2.Start();
		}

		void m_timer_Tick(object sender, EventArgs e)
		{
			base.addEvent(new Hashtable() 
								{
									{kTimestamp , Helper.toUnixtime(DateTime.Now).ToString()},
									{kChannel, "404"},
									{kModule, "module D"},
									{kMessage, "You say hello, I say goodbye!"},
									{kExtraData, ExtraData}
								});



		}

		void m_timer2_Tick(object sender, EventArgs e)
		{
			base.addEventRange(new List<Hashtable>()
									{
										new Hashtable() 
											{
												{kTimestamp , Helper.toUnixtime(DateTime.Now).ToString()},
												{kChannel, "555"},
												{kModule, "module EE"},
												{kMessage, "Goodbye, goodbye."},
												{kExtraData, ExtraData}
											},
										new Hashtable() 
											{
												{kTimestamp , Helper.toUnixtime(DateTime.Now).ToString()},
												{kChannel, "666"},
												{kModule, "module F!!!!!"},
												{kMessage, "I don't know why you say Hello."},
												{kExtraData, ExtraData}
											},
										new Hashtable() 
											{
												{kTimestamp , Helper.toUnixtime(DateTime.Now).ToString()},
												{kChannel, "7777777"},
												{kModule, "just another module"},
												{kMessage, "I say goodbye."},
												{kExtraData, ExtraData}
											},
									}
								);



		}

		public override void load()
		{
			Hashtable initialData = new Hashtable()
			{
				{
					kColumns, new List<Hashtable>()
								{
									new Hashtable() { 
														{kHeading, kTimestamp}, 
														{kType, kTypeUnixtime}
													},
									new Hashtable() { {kHeading, kChannel} },
									new Hashtable() { {kHeading, kModule} },
									new Hashtable() { {kHeading, kLogLevel} },
									new Hashtable() { {kHeading, kMessage} }
								}
				},
				{
					kEvents, new List<Hashtable>()
								{
									new Hashtable() 
									{
										{kTimestamp , Helper.toUnixtime(DateTime.Parse("10/10/2010 10:10:10")).ToString()},
										{kChannel, "1"},
										{kModule, "module a"},
										{kMessage, "Hello there!"},
										{kExtraData, ExtraData}
									},
									new Hashtable() 
									{
										{kTimestamp , Helper.toUnixtime(DateTime.Now.AddDays(-13)).ToString()},
										{kChannel, "2"},
										{kModule, "module b"},
										{kMessage, "Hi there!"},
										{kLogLevel, "Debug"},
										{kExtraData, ExtraData}
									},
									new Hashtable() 
									{
										{kTimestamp , Helper.toUnixtime(DateTime.Now.AddHours(-13)).ToString()},
										{kChannel, "3"},
										{kModule, "module c"},
										{kMessage, "Why, Hello!"},
										{kExtraData, ExtraData}
									},
								}
				}
			};

			base.initEventData (initialData);

			base.load();

		}
	}
}
