using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using TShockAPI.DB;

namespace LogTile
{
	public class LookupCommand : AbstractCommand
	{
		private TSPlayer ply;
		private LogTileArgument args;

		public LookupCommand(TSPlayer p, LogTileArgument a)
		{
			ply = p;
			args = a;
		}

		public override void Execute()
		{
			var database = LogTile.DB;
			String query =
				"SELECT * FROM LogTile WHERE X BETWEEN @0 AND @1 AND Y BETWEEN @2 and @3 AND Date > @4 ORDER BY id DESC;";
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
										  reader.Get<int>("Date"));
					events.Add(e);
				}
			}

			if (args.page <= 0)
			{
				ply.SendMessage("There are " + Math.Ceiling(events.Count / 7.0) + " pages. (" + events.Count + "edits)");
				for (var i = 0; i < Math.Min(6, events.Count); i++)
				{
					ply.SendMessage(events[i].parseEvent());
				}
			}
			else if (events.Count > 0)
			{
				for (var i = ((args.page - 1) * 7) - 1; i < Math.Min(args.page * 7 - 1, events.Count); i++)
				{
					ply.SendMessage(events[i].parseEvent());
				}
			}
			else
			{
				ply.SendMessage("No results found.", Color.Green);
			}
			ply.SendMessage("Edits made: " + events.Count, Color.Green);
		}
	}
}
