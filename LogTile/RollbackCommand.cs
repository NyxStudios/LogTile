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
			String query = "SELECT * FROM LogTile WHERE X BETWEEN @0 AND @1 AND Y BETWEEN @2 and @3 AND Date > @4 ORDER BY id ASC;";
			var events = new List<TileEvent>();
			using (var reader = database.QueryReader(query, ply.TileX - args.radius, ply.TileX + args.radius, ply.TileY - args.radius,
				ply.TileY + args.radius, (LogTile.helper.GetTime() - args.since)))
			{
				while (reader.Read())
				{
					var e = new TileEvent(reader.Get<int>("X"), reader.Get<int>("Y"),
											 LogTile.helper.INTtoString(reader.Get<int>("IP")),
											 reader.Get<string>("Name"),
											 LogTile.helper.getAction(reader.Get<int>("Action")),
											 reader.Get<int>("TileType"),
											 (long)reader.Get<int>("Date"));
					events.Add(e);
				}
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
