using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using log4net;

namespace Lib
{
	public partial class PopupQuery : Form
	{
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// pointer to initData passed in from client
		Hashtable m_initData;

		/// list of key names to all added controls; for convenience when saving new values back to initData
		string[] m_controlKeys;

		public PopupQuery(Hashtable initData)
		{
			this.TopMost = true;
			InitializeComponent();
			m_initData = initData;
			Hashtable values = initData.ContainsKey("Values") ? (Hashtable)initData["Values"] : null;
			int keyCount = initData.Keys.Count;
			string[] keys = new string[keyCount];
			initData.Keys.CopyTo(keys, 0);

			foreach (String key in keys)
			{
				switch (key)
				{
					case "NoCancelButton":
						Point cancelPos = ((Control)CancelButton).Location;
						okCancelPanel.Controls.Remove((Control)CancelButton);
						((Control)AcceptButton).Location = cancelPos;
						//this.Update();
						break;
					case "Title":
						this.Text = (string)initData["Title"];
						break;
					case "Controls":
						createControls((Dictionary<string, Hashtable>)initData["Controls"], values);
						break;
					case "Values":
						break;
					default:
						log.Warn("Did not find key <" + key + "> in initData for PopupQuery.");
						break;
				}
			}

		}

		private void createControls(Dictionary<string, Hashtable> initData, Hashtable values)
		{
			int keyCount = initData.Keys.Count;
			string[] keys = new string[keyCount];
			initData.Keys.CopyTo(keys, 0);
			Control firstControl = null;

			tableLayoutPanel1.Controls.Clear();
			tableLayoutPanel1.RowStyles.Clear();
			tableLayoutPanel1.ColumnStyles.Clear();
			tableLayoutPanel1.AutoSize = true;
			tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			tableLayoutPanel1.ColumnCount = 2;
			tableLayoutPanel1.RowCount = keys.Length;
			//this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			while (tableLayoutPanel1.RowStyles.Count < tableLayoutPanel1.RowCount)
				tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			while (tableLayoutPanel1.ColumnStyles.Count < tableLayoutPanel1.ColumnCount)
				tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));


			m_controlKeys = new string[keys.Length];

			int i = 0;
			int controlCount = 0;
			foreach (string key in keys) // for each new control to create
			{
				Hashtable controlData = initData[key];
				if (!controlData.ContainsKey("Type"))
				{
					log.Warn("Control type not specified in initData for PopupQuery.");
					continue;
				}
				Label newLabel = new Label();
				if (controlData.ContainsKey("Label"))
					newLabel.Text = (string)controlData["Label"];
				else
					newLabel.Text = key;
				newLabel.AutoSize = true;
				newLabel.Anchor = AnchorStyles.Left;
				tableLayoutPanel1.Controls.Add(newLabel, 0, i);
				Control newControl = null;
				switch ((string)controlData["Type"])
				{
					case "TextBox":
						newControl = new TextBox();
						((TextBox)newControl).UseSystemPasswordChar = newLabel.Text.ToUpper().StartsWith("PASS");
						if (controlData.ContainsKey("Value"))
							newControl.Text = (string)controlData["Value"];
						break;
					case "ComboBox":
						/// @todo: implement combo
						newControl = new ComboBox();
						break;
					case "CheckBox":
						newControl = new CheckBox();
						if (controlData.ContainsKey("Value") && controlData["Value"] is bool)
							((CheckBox)newControl).Checked = (bool)controlData["Value"];
						else if (controlData.ContainsKey("Value") && controlData["Value"] is string)
						{
							string val = ((string)controlData["Value"]).ToUpper();
							((CheckBox)newControl).Checked = val == "YES" || val == "Y" || val == "TRUE";
						}
						break;
					case "Image":
						Panel p = new Panel();
						p.BackgroundImage = Image.FromFile(controlData["Path"].ToString());
						p.BackgroundImageLayout = ImageLayout.Center;
						p.Height = p.BackgroundImage.Height;
						p.Width = p.BackgroundImage.Width;
						tableLayoutPanel1.SetColumnSpan(p, 2);
						tableLayoutPanel1.Controls.Remove(newLabel);
						tableLayoutPanel1.Controls.Add(p, 0, i++);
						newLabel.Text = "";
						continue;
					case "Heading":
						newLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold);
						//// reduce empty space under this text caused by bold font
						//int tmp = (int)tableLayoutPanel1.RowStyles[i].Height;
						//tableLayoutPanel1.RowStyles[i].SizeType = SizeType.Absolute;
						//tableLayoutPanel1.RowStyles[i].Height = tmp + 32;
						goto case "Label";
					case "Label":
						tableLayoutPanel1.SetColumnSpan(newLabel, 2);
						i++;
						continue;
					default:
						log.Warn("Unknown control type <" + controlData["Type"] + "> in initData for PopupQuery.");
						continue;
				}
				// replace default values if provided
				if (values != null && values.ContainsKey(key))
					if ((string)controlData["Type"] == "CheckBox")
					{
						if (values[key] is bool)
							((CheckBox)newControl).Checked = (bool)values[key];
						else if (values[key] is string)
						{
							string val = ((string)values[key]).ToUpper();
							((CheckBox)newControl).Checked = val == "YES" || val == "Y" || val == "TRUE";
						}
					}
					else
						newControl.Text = (string)values[key];

				newControl.AutoSize = true;
				newControl.Anchor = AnchorStyles.Left;
				newControl.Name = key;
				newControl.TabIndex = ++controlCount;
				if (controlCount == 1)
					firstControl = newControl;
				tableLayoutPanel1.Controls.Add(newControl, 1, i);
				m_controlKeys[i++] = key;
			}

			((Control)AcceptButton).TabIndex = ++controlCount;
			((Control)CancelButton).TabIndex = ++controlCount;
			if (firstControl != null)
				//firstControl.Focus();
				this.ActiveControl = firstControl;

		}

		// ok button clicked
		private void button1_Click(object sender, EventArgs e)
		{
			// save new values back to initdata, and add an additional hash of just keys->values
			Hashtable values = new Hashtable(m_controlKeys.Length); // this may be more than needed, but it doesn't have to be exact in either direction
			foreach (string key in m_controlKeys)
			{
				if (key == null)
					continue;
				Hashtable controlData = ((Dictionary<string, Hashtable>)m_initData["Controls"])[key];
				object value = null;
				if (tableLayoutPanel1.Controls[key] is CheckBox)
					value = ((CheckBox)tableLayoutPanel1.Controls[key]).Checked;
				else
					value = tableLayoutPanel1.Controls[key].Text;
				if (controlData.ContainsKey("Value"))
					controlData["Value"] = value;
				else
					controlData.Add("Value", value);
				values.Add(key, value);
			}
			if (m_initData.Contains("Values"))
				m_initData.Remove("Values");
			m_initData.Add("Values", values);

			DialogResult = DialogResult.OK;

			// close this form/window
			this.Close();
		}

	}
}
