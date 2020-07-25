using System;
using Advanced_Combat_Tracker;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

/* Instructions for adding new things
 * 1. Go to the InitTriggers function
 * 2. add a new trigger to the triggers list
 * 3. Done
 * 
 *  Syntax looks like this
 *  triggers.Add(
 *		new Trigger("regular expression", 
 *		delegate (Match match) 
 *		{
 *			//Do things with lines that matched the regular expression
 *		}
 *		));
 * 
 * forward and backslashes in regular expressions need to be escaped
 * 
 * Output functions are 
 * say("tts");
 * playSound("path//to//sound//file");
 * addLine("Text to add into log");
 * 
 */
namespace LiterallyMagic
{
	public class LiterallyMagic : UserControl, IActPluginV1
	{
		

		/* ==================== Set Player Name =======================*/
		//          Type your characters name in the quotes below
		//             Can also be a regular expression
		//examples: "Sym Hollysharp"
		//          ".* Hollysharp"
		//          "(?:Sym Hollysharp|Rose Hollysharp)"

		private string playerName = "(?:Sym|Rose|Lily) Hollysharp"; // Fill in player name

		/* ==================== Set Player Name =======================*/


		private void ExtremelyOutOfPlaceFunction()
		{

		// Change to false to disable

		// ************************** Trigger Toggles **************************

			midgardsormr.active      = false;

			archivePeripheral.active = false;

			naelFireballs.active     = false;

			autoEnd.active			 = true;


		// ************************** Trigger Toggles **************************
		}

		private Trigger midgardsormr;
		private Trigger archivePeripheral;
		private Trigger naelFireballs;
		private Trigger autoEnd;


		public delegate void response(Match match);

		private List<Trigger> triggers = new List<Trigger>();

		//Trigger variables
		private int midgard_prev_state = 0;

		private string archive_peripheral = "";

		private int fireball_count = 0;
		private bool had_second_fireball = false;
		private string second_fireball_name = "";


		//WT automation varaibles
		private bool wt_mode = false;
		private List<string[]> wt_lists;
		private Dictionary<string, int> wt_list_dictionary;





		//Trigger definitions
		public LiterallyMagic()
		{
			CreateUI();
			InitTriggers();
			ExtremelyOutOfPlaceFunction();

		}

		/* ---------- Trigger Setup ---------- */

		private void InitTriggers()
		{

			/*triggers.Add(new Trigger("02:Changed primary player to ([\\w' ]+)",
				delegate(Match match)
				{
					playerName = match.Groups[1].ToString();
				}));*/

			triggers.Add(new Trigger("SymsFrigde.eXXMG86jeDLPmjo27rQi",
				delegate (Match match)
				{
					ActPluginData pluginData = ActGlobals.oFormActMain.PluginGetSelfData(this);
					pluginData.pluginFile.Refresh();
					ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, false);
					Application.DoEvents();
					ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, true);
					say("Updated");
				}));

			//WT Automation

			triggers.Add(new Trigger("WT_MODE:ON",
				delegate (Match match)
				{
					wt_mode = true;
					say("Wondrous Tales mode On");
					if (wt_lists == null) wt_lists = new List<string[]>();
					else wt_lists.Clear();
				}));
			triggers.Add(new Trigger("WT_MODE:OFF",
				delegate (Match match)
				{
					wt_mode = false;
					say("Wondrous Tales mode Off");
				}));
			triggers.Add(new Trigger("WT:([a-zA-Z0-9 '\\-\\(\\);]+)",
				delegate (Match match)
				{
					if (wt_mode) wt_lists.Add(match.Groups[1].ToString().Split(';'));
				}));
			triggers.Add(new Trigger("WT_FUNC:CALC",
				delegate (Match match)
				{
					if (wt_mode)
					{
						tBox.Clear();
						if (wt_list_dictionary == null) wt_list_dictionary = new Dictionary<string, int>();
						else wt_list_dictionary.Clear();

						foreach (string[] sA in wt_lists)
						{
							foreach (string s in sA)
							{
								string sL = s.ToLower().Trim();
								if (wt_list_dictionary.ContainsKey(sL)) wt_list_dictionary[sL]++;
								else wt_list_dictionary.Add(sL, 1);
							}
						}

						List<KeyValuePair<string, int>> wt_sorted = new List<KeyValuePair<string, int>>();
						foreach(KeyValuePair<string,int> kv in wt_list_dictionary)
						{
							wt_sorted.Add(kv);
						}
						wt_sorted.Sort((a,b) => b.Value.CompareTo(a.Value));
						foreach(KeyValuePair<string,int> kv in wt_sorted)
						{
							tBox.AppendText(kv.Key + ": " + kv.Value + Environment.NewLine);
						
						}
						
					}
				}));


			// Midgardsormr
			triggers.Add(midgardsormr = new Trigger(
				"Midgardsormr:(31AD|31AC|31B0|31AE)",
				delegate (Match match)
				{
					string value = match.Value;

					if (value == "Midgardsormr:31AD") { midgard_prev_state = 1; say("Cardinal Corner"); return; } // flip
					if (value == "Midgardsormr:31AC") { midgard_prev_state = 2; say("In Out"); return; }// spin

					if (midgard_prev_state == 1) // flip
					{
						if (value == "Midgardsormr:31AE") say("Cardinal"); //spin
						if (value == "Midgardsormr:31B0") say("Corner");  //flip
						midgard_prev_state = 0;
					}
					else if (midgard_prev_state == 2) // spin
					{
						if (value == "Midgardsormr:31AE") say("Out");  //spin
						if (value == "Midgardsormr:31B0") say("In");  //flip
						midgard_prev_state = 0;
					}
				}
				));

			//Archive Peripheral
			triggers.Add(archivePeripheral = new Trigger(
				"1B:.{8}:Right Arm Unit:.{4}:.{4}:009(\\w):",
				delegate (Match match)
				{
					//Should be a single character D or C.
					// D = Blue, C = Orange
					string colorChar = match.Groups[1].ToString();
					// If assuming order will be static fails, can switch to checking what appears to be the UnitID of each hand
					// 0 = Unset, 1 = blue, 2 = orange
					if (archive_peripheral.Length < 2) archive_peripheral += colorChar; // This character should either be D or C, if it's not then something was incorrectly captured
					else //The final set has been reached
					{
						archive_peripheral += colorChar; // Adding the final character

						switch (archive_peripheral)
						{
							case "DDD":
								say("West");
								break;
							case "CDD":
								say("North West");
								break;
							case "CCD":
								say("North East");
								break;
							case "CCC":
								say("East");
								break;
							case "DCC":
								say("South East");
								break;
							case "DDC":
								say("South West");
								break;
							default:
								say("I broke it");
								break;
						}
						archive_peripheral = "";
					}
				}
				));

			if (playerName != "")
			{
				//Nael Fireballs A, player name filled in
				triggers.Add(naelFireballs = new Trigger(
					"23:.{8}:.*:.{8}:([\\s\\w']*):.{4}:.{4}:0005",
					delegate (Match match)
					{
						string name = match.Groups[1].ToString();
						switch (fireball_count)
						{
							case 0: // First Fire
								if (Regex.Match(name, playerName).Success) say("Fire in");
								break;
							case 1: // Second Fire
								if (Regex.Match(name, playerName).Success)
								{
									had_second_fireball = true;
									say("Fire out");
								}
								break;
							case 2: // Third Fire
								if (had_second_fireball) say("Avoid fireball");
								else if (Regex.Match(name, playerName).Success) say("Fire in");
								break;
							case 3: // Fourth fire
								if (Regex.Match(name, playerName).Success) say("Fire in");
								had_second_fireball = false;
								break;
						}
						
						fireball_count = fireball_count == 3 ? 0 : fireball_count + 1;
					}));
			}
			else
			{
				//Nael Fireballs B, player name NOT filled in
				triggers.Add(naelFireballs = new Trigger(
					"23:.{8}:.*:.{8}:([\\s\\w']*):.{4}:.{4}:0005",
					delegate (Match match)
					{
						string firstName = match.Groups[1].ToString().Split(' ')[0];
						//addLine(firstName);
						switch (fireball_count)
						{
							case 0: // First Fire
								say(firstName + " in");
								break;
							case 1: // Second Fire
								second_fireball_name = firstName;
								say(firstName + " out");
								break;
							case 2: // Third Fire
								say(firstName + " in. " + second_fireball_name + " out");
								second_fireball_name = "";
								break;
							case 3: // Fourth fire
								say(firstName + " in");
								had_second_fireball = false;
								break;
						}
						fireball_count = fireball_count == 3 ? 0 : fireball_count + 1;
					}));
			}

			triggers.Add(autoEnd = new Trigger("(wipeout|0038:end|21:([0-9,a-f,A-F]{8}):40000010)",
				delegate(Match match)
				{
					ActGlobals.oFormActMain.EndCombat(true);
				}
			));

			//Paisley Park load preset
			/*triggers.Add(new Trigger("^\\[\\d\\d:\\d\\d:\\d\\d\\.\\d{3}] 00:0038:PPLP:(.*)",
				delegate (Match match)
				{
					say(match.Groups[1].ToString());
					loadPreset(match.Groups[1].ToString());
				}
			));*/

			/*triggers.Add(new Trigger("^\\[\\d\\d:\\d\\d:\\d\\d\\.\\d{3}] 00:0038:PPPM:(.*)",
				delegate (Match match)
				{*/
					//{"One":{"X":108.0,"Y":0.0,"Z":100.0,"ID":1,"Active":true}}
					/*
					 * {"One" :
					 *	{"X":108.0,"Y":0.0,"Z":100.0,"ID":1,"Active":true}
					 * }
					 * A;X;Y;Z
					 */
				/*	string[] split = match.Groups[1].ToString().Split(';');
					//say("Placing " + split[0] + " at X " + split[1] + " Y " + split[2] + " Z " + split[3]);
					string output = "{\"" + split[0] + "\":{\"X\":" + split[1] + ",\"Y\":" + split[2] + ",\"Z\":" + split[3] + ",\"Active\":true}}";


					placeMarker(output);
				}
			));*/
			//The Epic of Alexander (Ultimate) has begun.
			/*triggers.Add(new Trigger("The Epic of Alexander (Ultimate) has begun.",
				async delegate (Match match)
				{
					await Task.Delay(2000);
					loadPreset("TEA");
				}
			));*/



		}









		/* ---------- ACT Plugin Setup Functions ---------- */

		// Program closing
		public void DeInitPlugin()
		{
			ActGlobals.oFormActMain.OnLogLineRead -= OnLogLineReadHandler;
			ActGlobals.oFormActMain.OnCombatEnd -= OnCombatEndHandler;
			ActGlobals.oFormActMain.OnCombatStart -= OnCombatStartHandler;
		}
		// Program starting
		public void InitPlugin(System.Windows.Forms.TabPage pluginScreenSpace, System.Windows.Forms.Label pluginStatusText)
		{
			pluginScreenSpace.Text = "Wat?";
			pluginStatusText.Text = "Help I'm trapped inside this small box";
			pluginScreenSpace.Controls.Add(this);
			this.Dock = DockStyle.Fill;
			ActGlobals.oFormActMain.OnLogLineRead += OnLogLineReadHandler;
			ActGlobals.oFormActMain.OnCombatEnd += OnCombatEndHandler;
			ActGlobals.oFormActMain.OnCombatStart += OnCombatStartHandler;
			

		}

		#region Copy pasta from a window form maker in another project

		//Holds the components to make them easier to dispose I guess?
		private System.ComponentModel.IContainer components = null;

		//If you're supposed to dispose stuff and there is stuff to dispose, dispose it
		//Seems to override something and is never called in the same so I guess it's ok?
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		//Component Definitions
		private System.Windows.Forms.TextBox tBox;

		private void CreateUI()
		{
			//Init components then hit pause?
			this.tBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();

			//tBox
			this.tBox.AutoSize = true;
			this.tBox.Location = new System.Drawing.Point(15, 15);
			this.tBox.Name = "tBox";
			this.tBox.Size = new System.Drawing.Size(400, 600);
			this.tBox.TabIndex = 0;
			this.tBox.Text = "Where might this be?";
			this.tBox.Multiline = true;

			//I dunno what this is, but it doesn't seem to work without this
			//Probably somethying to do with the box itself
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tBox);
			this.Name = "Where is this?";
			this.Size = new System.Drawing.Size(686, 384);
			this.ResumeLayout(false);

			//Hit play?
			this.PerformLayout();
		}


		#endregion


		/* ---------- ACT Event Handlers ---------- */


		//Function called when combat ends
		//May be less reliable without hojoring installed 
		private void OnCombatEndHandler(bool isImport, CombatToggleEventArgs encounterInfo)
		{
			midgard_prev_state = 0;
			had_second_fireball = false;
			fireball_count = 0;
			second_fireball_name = "";
		}
		//Function called when combat starts
		//May be less reliable without hojoring installed 
		private void OnCombatStartHandler(bool isImport, CombatToggleEventArgs encounterInfo)
		{
			midgard_prev_state = 0;
			had_second_fireball = false;
			fireball_count = 0;
			second_fireball_name = "";
		}

		//Function called for each log line
		private void OnLogLineReadHandler(bool isImport, LogLineEventArgs logInfo)
		{
			if (isImport) return;

			//RegexOptions for a third argument to include things like caseless etc.
			Match match;


			foreach (Trigger t in triggers)
			{
				match = Regex.Match(logInfo.logLine, t.regex);
				if (match.Success && t.active)
				{
					t.reaction.BeginInvoke(match,null,null);
				}
			}
		}


		/* ---------- Helper/Output Functions ---------- */

		private void say(string line)
		{
			ActGlobals.oFormActMain.TTS(line);
		}

		private void playSound(string file)
		{
			if (!File.Exists(file))
			{
				say("dead");
				return;
			}
			ActGlobals.oFormActMain.PlaySound(file);
		}

		private void addLine(string line)
		{
			string output = "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "." + DateTime.Now.Millisecond + "] 00:000a:" + line;
			ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, output);
		}

		private void placeMarker(string json)
		{
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://localhost:1337/place/");
			request.Method = "POST";
			request.ContentType = "application/json";
			using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
			{
				sw.Write(json);
			}

			try
			{
				WebResponse wr = request.GetResponse();
				using (Stream webStream = wr.GetResponseStream() ?? Stream.Null)
				using (StreamReader responseReader = new StreamReader(webStream))
				{
					string response = responseReader.ReadToEnd();
					//PRETTY SURE there's nothing to do with this info so...
				}
			}
			catch (Exception e)
			{
				Console.Out.WriteLine("LUL NOBODY CAN SEE THIS ANYWAY: " + e.Message);
			}
		}

		private void loadPreset(string preset)
		{
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://localhost:1337/preset/" + preset + "/load");
			request.Method = "POST";
			request.ContentType = "application/json";
			using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
			{
				sw.Write("ASDFLOL");
			}

			try
			{
				WebResponse wr = request.GetResponse();
				using (Stream webStream = wr.GetResponseStream() ?? Stream.Null)
				using (StreamReader responseReader = new StreamReader(webStream))
				{
					string response = responseReader.ReadToEnd();
					//PRETTY SURE there's nothing to do with this info so...
				}
			}
			catch (Exception e)
			{
				Console.Out.WriteLine("LUL NOBODY CAN SEE THIS ANYWAY: " + e.Message);
			}


		}

		private void DoUpdate()
		{
			ActPluginData pluginData = ActGlobals.oFormActMain.PluginGetSelfData(this);
			pluginData.pluginFile.Refresh();
			ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, false);
			Application.DoEvents();
			ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, true);
			say("Updated");
		}
	}

	public class Trigger
	{

		public string regex { get; set; }
		public LiterallyMagic.response reaction { get; set; }
		public bool active { get; set; }
		public Trigger(string rgx, LiterallyMagic.response rcn)
		{
			regex = rgx;
			reaction = rcn;
			active = true;
		}
	}
}
