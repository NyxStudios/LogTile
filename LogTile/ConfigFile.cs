using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TShockAPI;

namespace LogTile
{
	class ConfigFile
	{
		private LogTile logTile = null;
		public ConfigFile(LogTile lt)
		{
			logTile = lt;
		}

		public void ReadConfigFile()
		{
			string configFile = "";
			TextReader tr = new StreamReader(Path.Combine(TShock.SavePath, "logtile.cfg"));
			configFile = tr.ReadToEnd();
			string[] config = configFile.Split(' ');
			foreach (string c in config)
			{
				if (c.Contains("debug="))
				{
					string cC = c.Remove(6);
					if (cC == "false")
					{
						logTile.enableDebugOutput = false;
					} else
					{
						logTile.enableDebugOutput = true;
					}
				}
			}
		}

		public void WriteConfigFile()
		{
			if (!File.Exists(Path.Combine(TShock.SavePath, "logtile.cfg")))
			{
				TShockAPI.FileTools.CreateFile(Path.Combine(TShock.SavePath, "logtile.cfg"));
				TextWriter tw = new StreamWriter(Path.Combine(TShock.SavePath, "logtile.cfg"));
				tw.WriteLine("debug=false");
				tw.Close();
			}
		}
	}
}
