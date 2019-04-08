// reference:D:\usefulthings\ACTv3\NLua.dll
using System;
using Advanced_Combat_Tracker;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using NLua;
using System.Collections.Generic;
using System.Xml;

namespace TriggerThingalso
{
    public class Triggerthingalso : UserControl, IActPluginV1
    {

		Lua sandbox = new Lua();
		//I think this is fine? They're ordered and all
		List<LuaFunction> functions = new List<LuaFunction>();
		List<string> triggers = new List<string>();
		static readonly string pathToScripts = "C:\\Users\\Sym\\source\\repos\\TriggerThing\\TriggerThing\\Scripts\\";

        // Internal setup stuff
        public Triggerthingalso()
        {
			CreateUI();
			//Set up environment
			sandbox.DoString("import = function() end");
			sandbox.DoString("resetOnCombatStart = {}");
			sandbox.DoString("resetOnCombatEnd = {}");
			LoadScripts();

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
		private System.Windows.Forms.Label label1;
		private Button button1;

		private void CreateUI()
		{
			//Init components then hit pause?
			this.label1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();

			//Label1
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(15, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(114, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Where might this be?";

			//Button1
			this.button1.Location = new System.Drawing.Point(15, 50);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(141, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "I put in new stuff";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);

			//I dunno what this is, but it doesn't seem to work without this
			//Probably somethying to do with the box itself
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button1);
			this.Name = "Where is this?";
			this.Size = new System.Drawing.Size(686, 384);
			this.ResumeLayout(false);

			//Hit play?
			this.PerformLayout();
		}

		//Handlers for UI elements

		private void button1_Click(object sender, EventArgs e)
		{
			LoadScripts();
		}


		#endregion


		//ACT plugin setup stuff

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
			pluginScreenSpace.Controls.Add(this);
			this.Dock = DockStyle.Fill;

			ActGlobals.oFormActMain.OnLogLineRead += OnLogLineReadHandler;
			ActGlobals.oFormActMain.OnCombatEnd += OnCombatEndHandler;
			ActGlobals.oFormActMain.OnCombatStart += OnCombatStartHandler;

		}




		// Update yourself
		// May not be used
		private void DoUpdate()
        {
            ActPluginData pluginData = ActGlobals.oFormActMain.PluginGetSelfData(this);
            pluginData.pluginFile.Refresh();
            ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, false);
            Application.DoEvents();
            ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, true);
         //   Say("Updated");
        }

		private void LoadScripts()
		{
			/*
			 * Do something involving reading a file to identify what functions I'm looking for
			 * Load those functions into the sandbox, then pull them out and store them into the functions list
			 * Somehow associate these with regexs
			 */
			 //Prep in case of a reload
			triggers.Clear();
			functions.Clear();
			
			//execute all lua files first
			DirectoryInfo dInfo = new DirectoryInfo(pathToScripts);
			FileInfo[] files = dInfo.GetFiles("*.lua");
			foreach(FileInfo file in files)
			{
				sandbox.DoFile(file.FullName);
			}
			//Get config files and identify the functions and their associated regexs
			files = dInfo.GetFiles("*.xml");
			XmlDocument doc = new XmlDocument();
			XmlNodeList nodes;
			foreach(FileInfo file in files)
			{
				doc.Load(file.FullName);
				nodes = doc.SelectNodes("/trigger");
				foreach(XmlNode node in nodes)
				{
					triggers.Add(node.SelectSingleNode("\regex").Value);
					Say(node.SelectSingleNode("\regex").Value);
					functions.Add(sandbox[node.SelectSingleNode("\function").Value] as LuaFunction);
				}
			}
		}

       
		//Event handlers

        //Function called when combat ends
        private void OnCombatEndHandler(bool isImport, CombatToggleEventArgs encounterInfo)
        {
			if (isImport) return;
        }
        //Function called when combat starts
        private void OnCombatStartHandler(bool isImport, CombatToggleEventArgs encounterInfo)
        {
			if (isImport) return;
		}

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

			//I think foreach looks nicer, but I need to know the index for getting the function out of the sister list
			for(int i  = 0; i < triggers.Count; i++)
			{
				match = Regex.Match(logInfo.logLine, triggers[i]);
				if (match.Success)
				{
					if (functions[i] == null) Say("Borked");
					//ok uh
					//How about I just force the array into lua? I guess
					ReplaceMatchInSandbox(match.Groups);
					//Please forgive for having no idea what I'm doing, but this should work
					//I have no idea if the "as" thing is needed, it might just be another way to cast. It seems ok though
					
					Object[] output = functions[i].Call();
					string toDo = output[0] as string;
					string outputString = output[1] as string;
					switch(toDo)
					{
						case "say": //Say
							Say(outputString);
							break;
						case "play":// Play
							PlaySound(outputString);
							break;
						case "add"://Spoof log line
							AddLogLine(outputString);
							break;
					}
				}
			}
            
        }
		// Helper Functions because apparently the build in ones aren't working?

		private void ReplaceMatchInSandbox(GroupCollection groups)
		{
			sandbox.DoString("match={}"); // Reset table in sandbox
			for(int i = 0; i<groups.Count;i++)
			{
				sandbox.DoString("table.insert(match,\""+groups[i].ToString()+"\")");
			}
			return;
		}

		//Output Functions

		/* Uses ACT's default tts engine to say a given line
		 * 
		 * Just in case I need to debug or add something
		 * 
		*/
		private void Say(string line)
		{
			ActGlobals.oFormActMain.TTS(line);
		}

		/* Uses ACT to play the specified sound file
		 * 
		 * If the ACT playsound function fails to find the file nothing happens, so I added a notification for it
		 * 
		 * 
		 */
		private void PlaySound(string file)
		{
			if (!File.Exists(file))
			{
				Say("dead");
				return;
			}
			ActGlobals.oFormActMain.PlaySound(file);
		}

		/* Spoofs an /echo line in the log file for the purpose of triggering other ACT plugins
		 * 
		 * For example, Special spell timers visuals could be triggered if set up to do so from this function
		 */
		private void AddLogLine(string line)
		{
			string output = "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "." + DateTime.Now.Millisecond + "] 00:000a:" + line;
			ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, output);
		}
	}

	// this was how this was formatted on the internet 
	public class Trigger
	{
		public string regex { get; set; }
		public string function { get; set; }
	}

}
