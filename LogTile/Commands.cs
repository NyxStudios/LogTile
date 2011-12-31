using System;
using System.Collections.Generic;
using System.Threading;
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
			TShockAPI.Commands.ChatCommands.Add(new Command("lt.lookup", Lookup, "lookup"));
			TShockAPI.Commands.ChatCommands.Add(new Command("lt.rollback", Rollback, "rollback"));
			TShockAPI.Commands.ChatCommands.Add(new Command("lt.savequeue", SaveQueue, "savequeue"));
			TShockAPI.Commands.ChatCommands.Add(new Command("lt", Help, "lt"));
		}

		public void closeHook()
		{
			//ServerHooks.Chat -= handleCommand;
		}

		private void Help(CommandArgs args)
		{
			TSPlayer ply = args.Player;
			ply.SendMessage("LogTile Alpha Commands:", Color.Yellow);
			ply.SendMessage("/lookup area=x since=y page=z name=a ip=b", Color.Gold);
			ply.SendMessage("Looks up the changes in a certain area.", Color.Green);
			ply.SendMessage("/rollback area=x since=y page=z name=a ip=b", Color.Gold);
			ply.SendMessage("Rolls back an area to a previous state.", Color.Green);
			ply.SendMessage("/savequeue - Saves the queue.", Color.Gold);
		}

		private void SaveQueue(CommandArgs args)
		{
			save( args.Player, log);
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
			RollbackTiles(ply, argument);
		}

		private void LookupTiles(TSPlayer ply, LogTileArgument args)
		{
			LookupCommand lc = new LookupCommand( ply, args);
			CommandQueue.AddCommand(lc, ply);	
		}

		private void RollbackTiles( TSPlayer ply, LogTileArgument args )
		{
			RollbackCommand rc = new RollbackCommand( ply, args );
			CommandQueue.AddCommand(rc, ply);
		}

		public void save(TSPlayer ply, Logger log)
		{
			log.saveQueue( ply );
		}
	}
}