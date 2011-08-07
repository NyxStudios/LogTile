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
        public Commands()
        {
            this.helper = LogTile.helper;
            Console.WriteLine( helper.getAction( 1 ).ToString() );
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
            String[] stuff = command.Split(' ');
            if (stuff.Length > 0)
            {
                if (stuff[0] == "/lt")
                {
                    ply.SendMessage(String.Join(", ", stuff));
                    if( stuff.Length == 1 )
                    {
                        ply.SendMessage( "Command syntax here.");
                    }
                    else
                    {
                        if( stuff[1].Equals("check") )
                        {
                            int radius = 10;
                            if (stuff.Length > 2)
                                int.TryParse(stuff[2], out radius);
                            var database = TShockAPI.TShock.DB;
                            String query = "SELECT * FROM LogTile WHERE X BETWEEN @0 AND @1 AND Y BETWEEN @2 and @3 ORDER BY id DESC LIMIT 7;";
                            var events = new List<TileEvent>();
                            using (var reader = database.QueryReader(query, ply.TileX - radius, ply.TileX + radius, ply.TileY - radius, ply.TileY + radius))
                            {
                                while (reader.Read())
                                {
                                    Console.WriteLine("tet");
                                    Console.WriteLine(helper.INTtoString(reader.Get<int>("IP")));
                                    Console.WriteLine(helper.getAction(reader.Get<int>("Action")).ToString());
                                    events.Add(new TileEvent(reader.Get<int>("X"), reader.Get<int>("Y"),
                                                             helper.INTtoString(reader.Get<int>("IP")),
                                                             reader.Get<string>("Name"),
                                                             helper.getAction(reader.Get<int>("Action")),
                                                             reader.Get<int>("TileType")));
                                }
                            }

                            if( events.Count > 0 )
                            {
                                for (var index = 0; index < Math.Max(7, events.Count); index++)
                                {
                                    ply.SendMessage(events[index].parseEvent());
                                }
                            }
                            else
                            {
                                ply.SendMessage( "No results found", Color.Green);
                            }
                        }
                    }

                }
                args.Handled = true;
            }
        }
    }
}
