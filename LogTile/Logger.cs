using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using TShockAPI;
using TShockAPI.DB;

namespace LogTile
{
	internal class Logger
	{
		private TileQueue tileQueue;
		public Logger(TileQueue tq)
		{
			tileQueue = tq;
		}

		public void SaveTimer()
		{
			Thread.Sleep(60000);
			while (LogTile.isRunning)
			{
				saveQueue( null );
				Thread.Sleep(60000);
			}
		}

		public void saveQueue( TSPlayer ply)
		{
			SaveCommand sc = new SaveCommand(ply, tileQueue);
			CommandQueue.AddCommand(sc, ply);
		}
	}
}