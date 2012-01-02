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
				var temp = seperateEventsByUser(list);
				var database = LogTile.DB;
				foreach( KeyValuePair<String, List<TileEvent>> pair in temp)
				{
					var tempList = pair.Value;
					var tempSerial = JsonConvert.SerializeObject(tempList);
					string ip = tempList[0].ip;
					string name = tempList[0].name;

					String tempInsert =
							"INSERT INTO LogTile2 (Name, IP, Start, End, Data) VALUES (@0, @1, @2, @3, @4);";
					//reverse method for later String ipAddress = new IPAddress(BitConverter.GetBytes(intAddress)).ToString();
					if (LogTile.DB.Query(tempInsert, name, ip, tempList[0].GetDate(), tempList[tempList.Count - 1].GetDate(), tempSerial) != 1)
						Console.WriteLine("Error: Could not insert json blob");

					if (LogTile.enableDebugOutput)
						Console.WriteLine("LogTile queue is saving to db...");
					

				}
				/*string test = JsonConvert.SerializeObject(list);
				String jsonInsert =
							"INSERT INTO LogTile2 (Start, End, Data) VALUES (@0, @1, @2);";
				//reverse method for later String ipAddress = new IPAddress(BitConverter.GetBytes(intAddress)).ToString();
				if (LogTile.DB.Query(jsonInsert, list[0].GetDate(), list[list.Count - 1].GetDate(), test) != 1)
					Console.WriteLine("Error: Could not insert json blob");

				if (LogTile.enableDebugOutput)
					Console.WriteLine("LogTile queue is saving to db...");
				var database = LogTile.DB;*/

				foreach (TileEvent evt in list)
				{
					String query =
						"INSERT INTO LogTile (X, Y, IP, Name, Action, TileType, Date, Frame) VALUES (@0, @1, @2, @3, @4, @5, @6, @7);";
					//reverse method for later String ipAddress = new IPAddress(BitConverter.GetBytes(intAddress)).ToString();
					int intAddress = BitConverter.ToInt32(IPAddress.Parse(evt.GetIP()).GetAddressBytes(), 0);
					if (database.Query(query, evt.GetX(), evt.GetY(), intAddress, evt.GetName(), evt.GetAction(),
					                   evt.GetTileType(), evt.GetDate(), evt.frameNumber) != 1)
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

		private Dictionary<string, List<TileEvent>> seperateEventsByUser(List<TileEvent> evts)
		{
			Dictionary<string, List<TileEvent>> ipEvents = new Dictionary<string, List<TileEvent>>();

			foreach( TileEvent e in evts )
			{
				string ip = e.ip;

				List<TileEvent> tempList;

				if (ipEvents.ContainsKey(ip))
				{
					tempList = ipEvents[ip];
				}
				else
				{
					tempList = new List<TileEvent>();
					ipEvents.Add( ip, tempList );
				}

				tempList.Add(e);
			}

			return ipEvents;
		}

	}
}
