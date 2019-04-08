using System;
using Advanced_Combat_Tracker;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;

namespace TriggerThingA
{
    public class TriggerThing : UserControl, IActPluginV1
    {

        /*
         * This is going much better than expected
         * Next steps:
         * Data structure for storing the simple triggers
         * 
         * 
         */
        //Gonna try some abstraction
        public delegate void response(Match match);

        private static string me = "(Sym Hollysharp|Rose Hollysharp)";


		private List<Trigger> triggers = new List<Trigger>();

        //Trigger variables
        private int midgard_prev_state = 0;

        private string archive_peripheral = "";

        //Trigger definitions
        public TriggerThing()
        {
			InitTriggers();

		}

		/* ---------- Trigger Setup ---------- */

		public void InitTriggers ()
		{
			//Critlo
			triggers.Add(new Trigger(
				me + ":B9:.*:10004",
				delegate (Match match) {
					playSound("C:\\Users\\Sym\\usefulthings\\ACTv3\\plugins\\ACT.Hojoring-v5.18.2\\resources\\wav\\chime13.wav");
				}
				));

			//Trick Attack
			triggers.Add(new Trigger(
				"15:.*:8D2:Trick Attack:.*:28.{ 6 }:",
				delegate (Match match)
				{
					playSound("C:\\Users\\Sym\\usefulthings\\ACTv3\\plugins\\ACT.Hojoring-v5.18.2\\resources\\wav\\騙し討ち.wav");
				}
				));

			//Sution
			triggers.Add(new Trigger(
				"gains the effect of Suiton",
				delegate (Match match)
				{
					playSound("C:\\Users\\Sym\\usefulthings\\ACTv3\\plugins\\ACT.Hojoring-v5.18.2\\resources\\wav\\すいとん.wav");
				}
				));

			// Midgardsormr
			triggers.Add(new Trigger(
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

			//Midgard Fire marker
			triggers.Add(new Trigger(
				":.{4}:.{4}:0017",
				delegate (Match match)
				{
					playSound("C:\\Users\\Sym\\usefulthings\\ACTv3\\plugins\\ACT.Hojoring-v5.18.2\\resources\\wav\\bigwigs_Alarm.wav");
				}
				));

			//LOOPS
			triggers.Add(new Trigger(
				me + " gains the effect of Looper from  for (13.00|21.00|29.00) Seconds.",
				delegate (Match match)
				{
					switch (match.Groups[2].ToString())
					{
						case "13.00":
							say("First");
							break;
						case "21.00":
							say("Second");
							break;
						case "29.00":
							say("Third");
							break;
					}
				}
				));

			//Archive Peripheral
			triggers.Add(new Trigger(
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
		private void OnCombatEndHandler(bool isImport, CombatToggleEventArgs encounterInfo)
        {
            midgard_prev_state = 0;
        }
        //Function called when combat starts
        private void OnCombatStartHandler(bool isImport, CombatToggleEventArgs encounterInfo)
        {
            midgard_prev_state = 0;
        }

		//Function called for each log line
        private void OnLogLineReadHandler(bool isImport, LogLineEventArgs logInfo)
        {
			if (isImport) return;

            //RegexOptions for a third argument to include things like caseless etc.
            Match match = Regex.Match(logInfo.logLine, "SymsFrigde.eXXMG86jeDLPmjo27rQi");
            if (match.Success)
            {
                DoUpdate();
                return;
            }

			foreach(Trigger t in triggers)
			{
				match = Regex.Match(logInfo.logLine, t.regex);
				if (match.Success)
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

		public Trigger (string rgx, TriggerThing.response rcn)
		{
			regex = rgx;
			reaction = rcn;
		}
	}
}
