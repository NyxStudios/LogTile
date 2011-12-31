using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogTile
{
	public class LogTileArgument
	{
		public int radius { get; set; }
		public int since { get; set; }
		public int page { get; set; }
		public string player { get; set; }
		public string ip { get; set; }

		public LogTileArgument()
		{
			radius = Configuration.defaultRadius;
			since = Configuration.defaultTimeframe;
			page = 0;
			player = "";
			ip = "";
		}

		public void SetRadius(int r)
		{
			radius = r;
		}

		public void SetSince(int s)
		{
			since = s;
		}

		public void SetPage(int p)
		{
			page = p;
		}

		public void SetPlayer(string p)
		{
			player = p;
		}

		public void SetIP(string i)
		{
			ip = i;
		}
	}
}
