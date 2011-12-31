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
		private bool isRunning = true;
		private LogTile logTile;
		public Logger(TileQueue tq, LogTile lt)
		{
			tileQueue = tq;
			logTile = lt;
		}

		public void SaveTimer()
		{
			Thread.Sleep(60000);
			while (isRunning)
			{
				saveQueue();
				Thread.Sleep(60000);
			}
		}

		public void stop()
		{
			isRunning = false;
		}

		public void saveQueue()
		{
			var queue = tileQueue.GetQueue();
			var list = new List<TileEvent>();
			lock (queue)
			{
				while (queue.Count > 0)
					list.Add(queue.Dequeue());
			}
			if (list.Count > 0)
			{	
				if (logTile.enableDebugOutput)
					Console.WriteLine("LogTile queue is saving to db...");
				var database = TShock.DB;

				foreach (TileEvent evt in list)
				{
					String query =
						"INSERT INTO LogTile (X, Y, IP, Name, Action, TileType, Date) VALUES (@0, @1, @2, @3, @4, @5, @6);";
					//reverse method for later String ipAddress = new IPAddress(BitConverter.GetBytes(intAddress)).ToString();
					int intAddress = BitConverter.ToInt32(IPAddress.Parse(evt.GetIP()).GetAddressBytes(), 0);
					if (database.Query(query, evt.GetX(), evt.GetY(), intAddress, evt.GetName(), evt.GetAction(),
					                   evt.GetTileType(), evt.GetDate()) != 1)
					{
						Console.WriteLine("Error, failure to save edit.\n" + evt);
					}
				}
				if (logTile.enableDebugOutput)
					Console.WriteLine("LogTile has finished writing to db. " + list.Count + " edits were saved.");
			}
			else
			{
				if (logTile.enableDebugOutput)
					Console.WriteLine("Queue is empty.");
			}
		}
	}
}