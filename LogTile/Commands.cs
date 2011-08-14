using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TShockAPI;
using TShockAPI.DB;
using Terraria;
using TerrariaAPI.Hooks;

namespace LogTile
{
    class Commands
    {
        private TileHelper helper;
        private Logger log;
        public Commands( Logger l)
        {
            this.helper = LogTile.helper;
            log = l;
        }

        public void addHook()
        {
            ServerHooks.Chat += handleCommand;
        }

        public void closeHook()
        {
            
        }

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
                    //drop the first item since weve handled it
                    stuff.RemoveAt( 0 );
                    if( stuff.Count == 0 )
                    {
                        ply.SendMessage( "Command syntax here.");
                    }
                    else
                    {
                        while( stuff.Count > 0 )
                        {
                            switch( stuff[0] )
                            {
                                case "check":
                                    ParseCheck(ply, stuff);
                                    break;
                            }
                        }
                    }

                }
                args.Handled = true;
            }
        }

        private void ParseCheck( TSPlayer ply, List<String> args )
        {
            // toss out the first one since its what dropped us into this.
            args.RemoveAt(0);
            int radius = 10;
            long date = 600;
            while( args.Count > 0 )
            {
                String[] s = args[0].Split( '=' );
                String arg = s[0].ToLower();
                String val = s[1];
                switch( arg )
                {
                    case "area":
                        int.TryParse(val, out radius);
                        args.RemoveAt( 0 );
                        break;
                    case "since":
                        long.TryParse(val, out date);
                        args.RemoveAt( 0 );
                        break;
                    default:
                        args.RemoveAt(0);
                        break;
                }
            }

            LookupTiles(ply, radius, date);
        }

        public long LookupTiles( TSPlayer ply, int radius, long time )
        {
            log.saveQueue();
            var database = TShockAPI.TShock.DB;
            String query = "SELECT * FROM LogTile WHERE X BETWEEN @0 AND @1 AND Y BETWEEN @2 and @3 AND Date > @4 ORDER BY id DESC;";
            var events = new List<TileEvent>();
            using (var reader = database.QueryReader(query, ply.TileX - radius, ply.TileX + radius, ply.TileY - radius, ply.TileY + radius, (LogTile.helper.GetTime()-time)))
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
                    Console.WriteLine(e.parseEvent());
                    Console.WriteLine(events.Count);
                }
                Console.WriteLine("Final count: " + events.Count);
            }
            
            if (events.Count > 0)
            {
                ply.SendMessage( events.Count + " results found.", Color.Green);
                for (var i = 0; i < events.Count; i++)
                {
                    Console.WriteLine(events[i].parseEvent());
                    ply.SendMessage(events[i].parseEvent());
                }
            }
            else
            {
                ply.SendMessage("No results found.", Color.Green);
            }

            return events.Count;
        }
    }
}
