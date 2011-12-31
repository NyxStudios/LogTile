using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using TShockAPI;

namespace LogTile
{
	class ConfigFile
	{
		private LogTile logTile = null;
		private string savePath = Path.Combine(TShock.SavePath, "logtile.cfg");
		private ActualConfigFile configFile = null;
		public ConfigFile(LogTile lt)
		{
			logTile = lt;
		}

		public void ReadConfigFile()
		{
			TextReader tr = new StreamReader(savePath);
			string config = tr.ReadToEnd();
			tr.Close();			
			configFile = JsonConvert.DeserializeObject<ActualConfigFile>(config);
			Configuration.defaultRadius = configFile.defaultRadius;
			Configuration.defaultTimeframe = configFile.defaultTimeframe;
			Configuration.enableDebugOutput = configFile.enableDebugOutput;
		}

		public void WriteConfigFile()
		{
			if (!File.Exists(savePath))
			{
				TextWriter tw = new StreamWriter(savePath);
				tw.Write(JsonConvert.SerializeObject(new ActualConfigFile()));
				tw.Close();
			}
		}
	}

	public static class Configuration
	{
		public static int defaultRadius = 10;
		public static int defaultTimeframe = 600;
		public static bool enableDebugOutput = false;
	}

	class ActualConfigFile
	{
		public int defaultRadius = 10;
		public int defaultTimeframe = 600;
		public bool enableDebugOutput = false;
	}
}
