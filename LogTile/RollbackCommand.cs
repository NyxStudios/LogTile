using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using TShockAPI.DB;
using Terraria;

namespace LogTile
{
	class RollbackCommand : AbstractCommand
	{
		private TSPlayer ply;
		private LogTileArgument args;
		public RollbackCommand( TSPlayer p, LogTileArgument a)
		{
			ply = p;
			args = a;
		}

		public override void Execute()
		{
			ply.SendMessage("Starting Rollback: Radius:" + args.radius + " Since: " + args.since, Color.Green);

			var database = LogTile.DB;
			String query = "SELECT * FROM LogTile WHERE X BETWEEN @X1 AND @X2 AND Y BETWEEN @Y1 and @Y2 AND Date > @Date";

			if(!String.IsNullOrEmpty(args.player))
				query += " AND Name = @Name";
			if(!String.IsNullOrEmpty(args.ip))
				query += " AND Name = @IP";

			query += " ORDER BY id ASC;";

			var events = new List<TileEvent>();
			var reader = database.QueryReaderDict(query, new Dictionary<string, object>
                    {
                        {"X1", ply.TileX - args.radius},
                        {"X2", ply.TileX + args.radius},
                        {"Y1", ply.TileY - args.radius},
                        {"Y2", ply.TileY + args.radius},
                        {"Date", LogTile.helper.GetTime()-args.since},
                        {"Name", args.player},
                        {"IP", args.ip},
                    });
			while (reader.Read())
			{
				var e = new TileEvent(reader.Get<int>("X"), reader.Get<int>("Y"),
											LogTile.helper.INTtoString(reader.Get<int>("IP")),
											reader.Get<string>("Name"),
											LogTile.helper.getAction(reader.Get<int>("Action")),
											reader.Get<int>("TileType"),
											(long)reader.Get<int>("Date"),
											reader.Get<int>("Frame"));
				events.Add(e);
			}

			List<TileEvent> rollback = new List<TileEvent>();
			foreach (var ev in events)
			{
				if (!rollback.Contains(ev))
				{
					rollback.Add(ev);
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

			ply.SendMessage("Rollback Complete: Tiles Rolled Back: " + rollback.Count, Color.Green);
		}
	}
}
