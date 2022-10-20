using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Lib
{
	static public class Helper
	{
		private static string Tab = "   ";

		public static Color lightenColor(Color color)
		{
			const int LightenDegree = 30;
			return Color.FromArgb(color.A,
									Math.Min(255, color.R + LightenDegree),
									Math.Min(255, color.G + LightenDegree),
									Math.Min(255, color.B + LightenDegree));
		}

		public static Color darkenColor(Color color)
		{
			const int DarkenDegree = 30;
			return Color.FromArgb(color.A,
									Math.Max(0, color.R - DarkenDegree),
									Math.Max(0, color.G - DarkenDegree),
									Math.Max(0, color.B - DarkenDegree));
		}

        public static Color webColorToColorObj(string webColor) {
            Color color_obj;

            int red_val   = int.Parse(webColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int green_val = int.Parse(webColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int blue_val  = int.Parse(webColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            color_obj = Color.FromArgb(red_val, green_val, blue_val);

            return color_obj;
        }

        public static string colorObjToWebColor(Color colorObj) {
            string web_color = "";

            web_color += colorObj.R.ToString("X");
            web_color += colorObj.G.ToString("X");
            web_color += colorObj.B.ToString("X");

            return web_color;
        }

		public static void placeImageOnFormAt(Image i, Form f, int x, int y)
		{
			placeImageOnFormAt(i, f, x, y, null);
		}

		public static void placeImageOnFormAt(Image i, Form f, PictureBox p)
		{
			placeImageOnFormAt(i, f, -1, -1, p);
		}

		private static void placeImageOnFormAt(Image i, Form f, int x, int y, PictureBox p)
		{
			if (p == null)
			{
				p = new PictureBox();
				p.Location = new Point(x, y);
			}
			p.Image = i;
			p.Size = new System.Drawing.Size(i.Size.Width, i.Size.Height);
			if (!f.Controls.Contains(p))
				f.Controls.Add(p);
		}

		public static string deepTextDump(object o)
		{
			return deepTextDump(o, 0, "");
		}

		private static string deepTextDump(object o, int tabLevel, string previousResult)
		{
			string result = "";

			string tabs = "";
			for (int i = 0; i < tabLevel; i++)
				tabs += Tab;

			bool doTabFirstPrint = (previousResult != "" && previousResult.EndsWith("\r\n"));

			if (o is string)
			{
				result += (doTabFirstPrint ? tabs : "\"") + (string)o + "\"\r\n";
			}
			else if (o is Hashtable)
			{
				result += (doTabFirstPrint ? tabs : "") + "Hashtable(" + ((Hashtable)o).Count + "):\r\n";
				foreach (string key in ((Hashtable)o).Keys)
				{
					string tabs1 = tabs + Tab;
					result += tabs1 + "" + tabs1 + key + " -> ";
					result += deepTextDump(((Hashtable)o)[key], tabLevel + 1, "");
				}
			}
			else if (o == null)
			{
				result += (doTabFirstPrint ? tabs : result) + "--NULL--\r\n";
			}
			else // List <x>
			{
				result += (doTabFirstPrint ? tabs : result) + "List(" + ((ICollection)o).Count + "):\r\n";
				foreach (var item in (ICollection)o)
				{
					result += tabs + deepTextDump(item, tabLevel + 1, result);
				}
			}

			return result;
		}

		public static DateTime fromUnixtime(double unixtime)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(unixtime);//.ToLocalTime();
		}

		public static double toUnixtime(DateTime value)
		{
			//create Timespan by subtracting the value provided from
			//the Unix Epoch
			TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0));//.ToLocalTime());

			//return the total seconds (which is a UNIX timestamp)
			return (double)span.TotalSeconds;
		}

		public static List<string> deepClone(List<string> source)
		{
			List<String> clone = new List<string>(source);
			return clone;
		}

		public static Dictionary<string, Hashtable> deepClone(Dictionary<string, Hashtable> source)
		{
			Dictionary<string, Hashtable> clone = new Dictionary<string, Hashtable>();
			string[] keys = new string[source.Keys.Count];
			source.Keys.CopyTo(keys, 0);

			for (int i = 0; i < source.Keys.Count; i++)
				clone.Add(keys[i], deepClone(source[keys[i]]));

			return clone;
		}

		public static Hashtable deepClone(Hashtable source)
		{
			Hashtable clone = new Hashtable();
			string[] keys = new string[source.Keys.Count];
			source.Keys.CopyTo(keys, 0);

			for (int i = 0; i < source.Keys.Count; i++)
			{
				object value = source[keys[i]];
				if (value is Dictionary<string, Hashtable>)
					clone.Add(keys[i], deepClone((Dictionary<String, Hashtable>)value));
				else if (value is List<string>)
					clone.Add(keys[i], deepClone((List<string>)value));
				else if (value is Hashtable)
					clone.Add(keys[i], deepClone((Hashtable)value));
				else
					clone.Add(keys[i], value);
			}

			return clone;
		}

		public static string encryptOrDecrypt(string data, string pass, bool doEncrpyt)
		{
			byte[] u8_Salt = new byte[] { 0x22, 0x11, 0x31, 0x8E, 0xD0, 0x32, 0x95, 0x34, 0x26, 0x75, 0x23, 0xAB, 0xF6 };

			//if (data.Length > 0 && data[0] == 'V' && doEncrpyt)
			//    System.Diagnostics.Debugger.Break();

			PasswordDeriveBytes i_Pass = new PasswordDeriveBytes(pass, u8_Salt);

			Rijndael i_Alg = Rijndael.Create();
			i_Alg.Key = i_Pass.GetBytes(32);
			i_Alg.IV = i_Pass.GetBytes(16);

			ICryptoTransform i_Trans = (doEncrpyt) ? i_Alg.CreateEncryptor() : i_Alg.CreateDecryptor();

			MemoryStream i_Mem = new MemoryStream();
			CryptoStream i_Crypt = new CryptoStream(i_Mem, i_Trans, CryptoStreamMode.Write);

			byte[] u8_Data;
			if (doEncrpyt) u8_Data = Encoding.Unicode.GetBytes(data);
			else u8_Data = Convert.FromBase64String(data);

			try
			{
				i_Crypt.Write(u8_Data, 0, u8_Data.Length);
				i_Crypt.FlushFinalBlock();
				if (doEncrpyt) return Convert.ToBase64String(i_Mem.ToArray());
				else return Encoding.Unicode.GetString(i_Mem.ToArray());
			}
			catch 
            { 
                return null; 
            }
			finally 
            { 
                i_Crypt.Close(); 
            }
		}

		public static void clearNodes(TreeNodeCollection tnc)
		{
			foreach (TreeNode tn in tnc)
				tn.Nodes.Clear();
		}

		public static void highlightNode(TreeNode tn)
		{
			highlightNode(tn, true);
		}

		public static void unHighlightNode(TreeNode tn)
		{
			highlightNode(tn, false);
		}

		public static void highlightNode(TreeNode tn, bool doHighlight)
		{
			if (tn.NodeFont != null)
			{
				FontFamily ff = tn.NodeFont.FontFamily;
				float fontSize = tn.NodeFont.Size;
				tn.NodeFont = new System.Drawing.Font(ff, fontSize, doHighlight ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular);
			}
			else
				tn.NodeFont = new System.Drawing.Font("Microsoft Sans Serif", 8F, doHighlight ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular);
			tn.Text += string.Empty;
			tn.BackColor = doHighlight ? Color.Aquamarine : Color.Transparent;
		}

		public static void unHighlightNodes(TreeNodeCollection tnc)
		{
			foreach (TreeNode tn in tnc)
				highlightNode(tn, false);
		}

		public static bool atob(string a)
		{
			a = a.ToUpper();
			return a == "YES" || a == "Y" || a == "TRUE";
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// Searches for a string str within an array of strings.
		///
		/// Looks for a complete, exact match.
		/// @param strings Array of strings to check.
		/// @param str What we're looking for
		/// @return first matching index or -1
		public static int find(string[] strings, string str)
		{
			int result = -1;
			for (int i = 0; i < strings.Length; i++)
			{
				if (str == strings[i])
				{
					result = i;
					break;
				}
			}
			return result;
		}

		public static string findAvailableSimilarKey(Dictionary<string, Hashtable> dict, string newConnectionKey)
		{
			string returnValue = "";
			int numberSuffix = 1;

			Match m = Regex.Match(newConnectionKey, @"(.*?[^\d])(\d+)$");
			if (m.Success) // ends in a number
			{
				returnValue = m.Groups[1].Value;
				numberSuffix = Int16.Parse(m.Groups[2].Value);
			}
			else if (Regex.IsMatch(newConnectionKey, @"^\d+$")) // is a number
				numberSuffix = Int16.Parse(newConnectionKey);
			else // does not contain a number
				returnValue = newConnectionKey;

			while (dict.ContainsKey(returnValue + numberSuffix.ToString()))
				numberSuffix++;

			return returnValue + numberSuffix.ToString();
		}

		public static void ignoreReadonlyKeys(KeyEventArgs k)
		{
			ignoreReadonlyKeys(k, null);
		}

		public static void ignoreReadonlyKeys(KeyPressEventArgs kp)
		{
			ignoreReadonlyKeys(null, kp);
		}

		private static void ignoreReadonlyKeys(KeyEventArgs k, KeyPressEventArgs kp)
		{
			List<Keys> ReadOnlyKeys = new List<Keys>
			{
				Keys.Up,
				Keys.Down,
				Keys.Left,
				Keys.Right,
				Keys.Home,
				Keys.End,
				Keys.Control,
				Keys.LButton,
				Keys.ShiftKey,
				Keys.LButton | Keys.ShiftKey
			};

			if (k != null)
			{
				// allow movement etc keys
				if (ReadOnlyKeys.Contains(k.KeyCode))
					return;
				// allow ctrl-c copy
				if (Control.ModifierKeys == Keys.Control && k.KeyCode == Keys.C)
					return;
				// ignore everything else
				k.Handled = true;
			}
			else if (kp != null)
			{
				// allow ctrl-c copy
				if (Control.ModifierKeys == Keys.Control && kp.KeyChar == 3)
					return;
				// ignore everything else
				kp.Handled = true;
			}
		}

		/// not used, but tested
		public static void selectGridsRowByIndex(DataGridView grid, int newSelectedRowIndex)
		{
			if (grid.SelectedRows.Count != 1 || grid.SelectedRows[0].Index != newSelectedRowIndex)
				for (int i = 0; i < grid.Rows.Count; i++)
					grid.Rows[i].Selected = i == newSelectedRowIndex;
		}

		public static void selectGridsRowsBySelectedCells(DataGridView grid)
		{
			for (int i = 0; i < grid.Rows.Count; i++)
				for (int j = 0; j < grid.Rows[i].Cells.Count; j++)
					if (grid.Rows[i].Cells[j].Selected)
					{
						grid.Rows[i].Selected = true;
						break;
					}
		}

		public static DataGridViewCell getGridCellAtPoint(DataGridView grid, Point point)
		{
			Point p = grid.PointToClient(point);
			DataGridView.HitTestInfo hti = grid.HitTest(p.X, p.Y);
			if (hti.RowIndex < 0 ||
				hti.RowIndex >= grid.Rows.Count ||
				hti.ColumnIndex < 0 ||
				hti.ColumnIndex >= grid.Columns.Count)
				return null;
 
			return grid[hti.ColumnIndex, hti.RowIndex];
		}

		/////////////////////////////////////////////////////////////////////////////////
		/// Search an exception/error message for key phrases and return a user friendly 
		/// message.
		///
		/// If no match is found, returns a default unknown error message.
		/// @param errorMessage non user friendlymessage to parse
		/// @return a user frindly version of the input
		public static string findUserFriendlyErrorMessage(string errorMessage)
		{
			string result = "";

			if (errorMessage.Contains("error from Perl function \"asterisk_reload\": Couldn't connect to reloader daemon"))
			{
				result = "The Asterisk SQL reload script is not running on the server. To make changes, ask an administrator to relaunch it.";
			}
			else if (errorMessage.Contains("ERROR: error from Perl function "))
			{
				if (errorMessage.Contains("insert"))
				{
					result = "Asterisk Configuration Server DB Insert error. See log for details.";
				}
				else
				{
					result = "Asterisk Configuration Server Error. See log for details.";
				}
			}
			else
			{
				result = "Unknown error! Please check the log or contact an administrator.";
			}

			return result;
		}

        public static void SetDataGridViewCell_BackColor(DataGridViewRow gridRow, string cellName, Color backColor) {
            DataGridView grid = gridRow.DataGridView;
            
            if (!grid.Columns.Contains(cellName)) {
                return;
            }

            gridRow.Cells[cellName].Style.BackColor = backColor;
        }
	}
}
