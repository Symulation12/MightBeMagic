using System;
using Advanced_Combat_Tracker;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;

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
namespace TriggerThingB
{
	public class TriggerThing : UserControl, IActPluginV1
	{
		private Trigger midgardsormr;
		private Trigger archivePeripheral;

		private void ExtremelyOutOfPlaceFunction()
		{

		// Change to false to disable

		// ************************** Trigger Toggles **************************

			midgardsormr.active      = true;

			archivePeripheral.active = true;


		// ************************** Trigger Toggles **************************
		}



		public delegate void response(Match match);

		private List<Trigger> triggers = new List<Trigger>();

		//Trigger variables
		private int midgard_prev_state = 0;

		private string archive_peripheral = "";


		//Trigger definitions
		public TriggerThing()
		{
			InitTriggers();
			ExtremelyOutOfPlaceFunction();

		}

		/* ---------- Trigger Setup ---------- */

		private void InitTriggers()
		{
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


		}









		/* ---------- ACT Plugin Setup Functions ---------- */

		// Program closing
		public void DeInitPlugin()
		{
			ActGlobals.oFormActMain.OnLogLineRead -= OnLogLineReadHandler;
		}
		// Program starting
		public void InitPlugin(System.Windows.Forms.TabPage pluginScreenSpace, System.Windows.Forms.Label pluginStatusText)
		{
			pluginScreenSpace.Text = "Wat?";
			pluginStatusText.Text = "Help I'm trapped inside this small box";
			ActGlobals.oFormActMain.OnLogLineRead += OnLogLineReadHandler;
			ActGlobals.oFormActMain.OnCombatEnd += OnCombatEndHandler;
			ActGlobals.oFormActMain.OnCombatStart += OnCombatStartHandler;

		}


		/* ---------- ACT Event Handlers ---------- */

		
		//Function called when combat ends
		//May be less reliable without hojoring installed 
		private void OnCombatEndHandler(bool isImport, CombatToggleEventArgs encounterInfo)
		{
			midgard_prev_state = 0;
		}
		//Function called when combat starts
		//May be less reliable without hojoring installed 
		private void OnCombatStartHandler(bool isImport, CombatToggleEventArgs encounterInfo)
		{
			midgard_prev_state = 0;
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
					t.reaction(match);
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
		public TriggerThing.response reaction { get; set; }
		public bool active { get; set; }
		public Trigger(string rgx, TriggerThing.response rcn)
		{
			regex = rgx;
			reaction = rcn;
			active = true;
		}
	}
}
