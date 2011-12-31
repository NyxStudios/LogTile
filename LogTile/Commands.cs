using System;
using System.Collections.Generic;
using TShockAPI;
using TShockAPI.DB;
using Terraria;

namespace LogTile
{
	internal class Commands
	{
		private TileHelper helper;
		private Logger log;

		public Commands(Logger l)
		{
			this.helper = LogTile.helper;
			log = l;
		}

		public void addHook()
		{
			//ServerHooks.Chat += handleCommand;
			TShockAPI.Commands.ChatCommands.Add(new Command("lookup", Lookup, "lookup"));
			TShockAPI.Commands.ChatCommands.Add(new Command("rollback", Rollback, "rollback"));
		}

		public void closeHook()
		{
			//ServerHooks.Chat -= handleCommand;
		}

		private void Lookup(CommandArgs args)
		{
			TSPlayer ply = args.Player;
			var param = args.Parameters;
			LogTileArgument argument = new LogTileArgument();
			foreach (string keyval in param)
			{
				string[] pair = keyval.Split('=');
				if (pair.Length < 2)
					continue;
				string key = pair[0];
				string val = pair[1];

				switch (key)
				{
					case "area":
						int radius;
						int.TryParse(val, out radius);
						argument.SetRadius(radius);
						break;
					case "since":
						int time;
						int.TryParse(val, out time);
						argument.SetSince(time);
						break;
					case "page":
						int page;
						int.TryParse(val, out page);
						argument.SetPage(page);
						break;
					case "name":
						argument.SetPlayer(val);
						break;
					case "ip":
						argument.SetIP(val);
						break;
					default:
						break;
				}
			}
			LookupTiles(ply, argument);
		}

		private void Rollback(CommandArgs args)
		{
			TSPlayer ply = args.Player;
			var param = args.Parameters;
			LogTileArgument argument = new LogTileArgument();
			foreach (string keyval in param)
			{
				string[] pair = keyval.Split('=');
				if (pair.Length < 2)
					continue;
				string key = pair[0];
				string val = pair[1];

				switch (key)
				{
					case "area":
						int radius;
						int.TryParse(val, out radius);
						argument.SetRadius(radius);
						break;
					case "since":
						int time;
						int.TryParse(val, out time);
						argument.SetSince(time);
						break;
					case "page":
						int page;
						int.TryParse(val, out page);
						argument.SetPage(page);
						break;
					case "name":
						argument.SetPlayer(val);
						break;
					case "ip":
						argument.SetIP(val);
						break;
					default:
						break;
				}
			}
			Console.WriteLine("Starting Rollback:\nRadius:{0}\nSince:{1}", argument.radius, argument.since);
			var rollback = RollbackTiles(ply, argument);
			Console.WriteLine("Rollback Complete:\nTiles Rolled Back:{0}", rollback); 
			
		}

		/*

        public void handleCommand(messageBuffer buff, int i, String command, HandledEventArgs args )
        {
            TSPlayer ply = TShock.Players[buff.whoAmI];
            if (ply == null)
                return;
            List<String> stuff = command.Split(' ').ToList();
            if (stuff.Count > 0)
            {
                if (stuff[0] == "/lt")
                {
                    args.Handled = true;
                    //drop the first item since weve handled it
                    stuff.RemoveAt( 0 );
                    if( stuff.Count == 0 )
                    {
                        ply.SendMessage( "Command syntax here.");
                        args.Handled = true;
                    }
                    else
                    {
                        if( stuff.Count > 0 )
                        {
                            switch( stuff[0] )
                            {
                                case "check":
                                    ParseCheck(ply, stuff);
                                    break;
                                case "rollback":
                                    ParseRollback(ply, stuff);
                                    break;
                                case "save":
                                    save();
                                    break;
                            }
                        }
                    }

                }
                
            }
        }

        private void ParseCheck(TSPlayer ply, List<String> args)
        {
            // toss out the first one since its what dropped us into this.
            args.RemoveAt(0);
            int radius = 10;
            long date = 600;
            int page = -1;
            string name = "";
            string ip = "";
            while (args.Count > 0)
            {
                String[] s = args[0].Split('=');
                String arg = s[0].ToLower();
                String val = s[1];
                switch (arg)
                {
                    case "area":
                        int.TryParse(val, out radius);
                        args.RemoveAt(0);
                        break;
                    case "since":
                        long.TryParse(val, out date);
                        args.RemoveAt(0);
                        break;
                    case "page":
                        int.TryParse(val, out page);
                        args.RemoveAt(0);
                        break;
                    case "name":
                        name = val;
                        args.RemoveAt(0);
                        break;
                    case "ip":
                        ip = val;
                        args.RemoveAt(0);
                        break;
                    default:
                        args.RemoveAt(0);
                        break;
                }
            }

            LookupTiles(ply, radius, date, page, name, ip);
        }
		*/

		private long LookupTiles(TSPlayer ply, LogTileArgument arg)
		{
			return LookupTiles(ply, arg.radius, arg.since, arg.page, arg.player, arg.ip);
		}

		public long LookupTiles(TSPlayer ply, int radius, long time, int page, string name, string ip)
		{
			var database = TShock.DB;
			String query =
				"SELECT * FROM LogTile WHERE X BETWEEN @0 AND @1 AND Y BETWEEN @2 and @3 AND Date > @4 ORDER BY id DESC;";
			var events = new List<TileEvent>();
			using (var reader = database.QueryReader(query, ply.TileX - radius, ply.TileX + radius, ply.TileY - radius,
			                                         ply.TileY + radius, (LogTile.helper.GetTime() - time)))
			{
				while (reader.Read())
				{
					var e = new TileEvent(reader.Get<int>("X"), reader.Get<int>("Y"),
					                      helper.INTtoString(reader.Get<int>("IP")),
					                      reader.Get<string>("Name"),
					                      helper.getAction(reader.Get<int>("Action")),
					                      reader.Get<int>("TileType"),
					                      reader.Get<int>("Date"));
					events.Add(e);
				}
			}

			if (page <= 0)
			{
				ply.SendMessage("There are " + Math.Ceiling(events.Count/7.0) + " pages. (" + events.Count + "edits)");
				for (var i = 0; i < Math.Min(6, events.Count); i++)
				{
					ply.SendMessage(events[i].parseEvent());
				}
			}
			else if (events.Count > 0)
			{
				for (var i = ((page - 1)*7) - 1; i < Math.Min(page*7 - 1, events.Count); i++)
				{
					ply.SendMessage(events[i].parseEvent());
				}
			}
			else
			{
				ply.SendMessage("No results found.", Color.Green);
			}
			Console.WriteLine("Edits made: " + events.Count);
			return events.Count;
		}

		/*
        private void ParseRollback(TSPlayer ply, List<String> args)
        {
            // toss out the first one since its what dropped us into this.
            args.RemoveAt(0);
            int radius = 10;
            long date = 600;
            while (args.Count > 0)
            {
                String[] s = args[0].Split('=');
                String arg = s[0].ToLower();
                String val = s[1];
                switch (arg)
                {
                    case "area":
                        int.TryParse(val, out radius);
                        args.RemoveAt(0);
                        break;
                    case "since":
                        long.TryParse(val, out date);
                        args.RemoveAt(0);
                        break;
                    default:
                        args.RemoveAt(0);
                        break;
                }
            }
            Console.WriteLine("Starting Rollback:\nRadius:@0\nSince:@1", radius, date);
            var rollback = RollbackTiles(ply, radius, date);
            Console.WriteLine("Rollback Complete:\nTiles Rolled Back:@0", rollback);
        }

		*/

		private long RollbackTiles( TSPlayer ply, LogTileArgument args )
		{
			return RollbackTiles(ply, args.radius, args.since);
		}

        private long RollbackTiles( TSPlayer ply, int radius, long time )
        {
            var database = TShockAPI.TShock.DB;
            String query = "SELECT * FROM LogTile WHERE X BETWEEN @0 AND @1 AND Y BETWEEN @2 and @3 AND Date > @4 ORDER BY id ASC;";
            var events = new List<TileEvent>();
            using (var reader = database.QueryReader(query, ply.TileX - radius, ply.TileX + radius, ply.TileY - radius,
                ply.TileY + radius, (LogTile.helper.GetTime() - time)))
            {
                while (reader.Read())
                {
                    var e = new TileEvent(reader.Get<int>("X"), reader.Get<int>("Y"),
                                             helper.INTtoString(reader.Get<int>("IP")),
                                             reader.Get<string>("Name"),
                                             helper.getAction(reader.Get<int>("Action")),
                                             reader.Get<int>("TileType"),
                                             (long)reader.Get<int>("Date"));
                    events.Add(e);
                }
            }

            List<TileEvent> rollback = new List<TileEvent>();
            foreach (var ev in events )
            {
                if( !rollback.Contains( ev ) )
                {
                    rollback.Add( ev );
                }
            }
            
            foreach (var evt in rollback)
            {
                if (LogTile.helper.getAction(evt.GetAction()) == Action.BREAK)
                {
                    Main.tile[evt.GetX(), evt.GetY()].type = (byte)evt.GetTileType();
                    Main.tile[evt.GetX(), evt.GetY()].active = true;
                }
                else
                {
                    Main.tile[evt.GetX(), evt.GetY()].active = false;
                }

                TSPlayer.All.SendTileSquare(evt.GetX(), evt.GetY(), 1);
            }
            return rollback.Count;
        }
		/**/

		public void save()
		{
			log.saveQueue();
		}

		private class LogTileArgument
		{
			public int radius { get; set; }
			public int since { get; set; }
			public int page { get; set; }
			public string player { get; set; }
			public string ip { get; set; }

			public LogTileArgument()
			{
				radius = 10;
				since = 600;
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
}