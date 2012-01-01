using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using TShockAPI;
using TShockAPI.DB;

namespace LogTile
{
	class SaveCommand : AbstractCommand
	{
		private TSPlayer ply;
		private TileQueue tileQueue;

		public SaveCommand( TSPlayer p, TileQueue t )
		{
			ply = p;
			tileQueue = t;
		}

		public override void Execute()
		{
			if( ply == null )
				Console.WriteLine("Telling the queue to save.");
			else
				ply.SendMessage("Telling the queue to save.");

			var queue = tileQueue.GetQueue();
			var list = new List<TileEvent>();
			lock (queue)
			{
				while (queue.Count > 0)
					list.Add(queue.Dequeue());
			}

			if (list.Count > 0)
			{
				string test = JsonConvert.SerializeObject(list);
				String jsonInsert =
							"INSERT INTO LogTile2 (Start, End, Data) VALUES (@0, @1, @2);";
				//reverse method for later String ipAddress = new IPAddress(BitConverter.GetBytes(intAddress)).ToString();
				if (LogTile.DB.Query(jsonInsert, list[0].GetDate(), list[list.Count - 1].GetDate(), test) != 1)
					Console.WriteLine("Error: Could not insert json blob");

				if (LogTile.enableDebugOutput)
					Console.WriteLine("LogTile queue is saving to db...");
				var database = LogTile.DB;

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
				if( LogTile.enableDebugOutput )
					Console.WriteLine("LogTile has written " + list.Count + " edits to the database.");
			}
			else
			{
				if (LogTile.enableDebugOutput)
					Console.WriteLine("Queue is empty.");
			}

			if (ply == null)
				Console.WriteLine("Queue finished saving..");
			else
				ply.SendMessage("Queue finished saving..");
		}
	}
}
