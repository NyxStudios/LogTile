using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using TShockAPI;
using TShockAPI.DB;
using Terraria;
using Hooks;

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
            ServerHooks.Chat -= handleCommand;
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
                    args.Handled = true;
                    if (!ply.Group.HasPermission("logtile"))
                        return;
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

        public long LookupTiles( TSPlayer ply, int radius, long time, int page, string name, string ip )
        {
            var database = TShockAPI.TShock.DB;

            String query = "SELECT * FROM LogTile WHERE X BETWEEN @X1 AND @X2 AND Y BETWEEN @Y1 and @Y2 AND Date > @Date";
            
            if (name != "")
                query += " AND Name = @Name";
            if (ip != "")
                query += " AND Name = @IP";
           
            query += " ORDER BY id DESC;";

            var events = new List<TileEvent>();
            var reader = QueryReaderDict(database, query, new Dictionary<string, object>
                                                        {
                                                            {"X1", ply.TileX - radius},
                                                            {"X2", ply.TileX + radius},
                                                            {"Y1", ply.TileY - radius},
                                                            {"Y2", ply.TileY + radius},
                                                            {"Date", LogTile.helper.GetTime()-time},
                                                            {"Name", name},
                                                            {"IP", ip},
                                                        });

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

            if( page == -1 )
            {
                ply.SendMessage( "There are " + Math.Ceiling( events.Count / 7.0 ) + " pages. (" + events.Count + "edits)");
                for (var i = 0; i < 6; i++)
                {
                    ply.SendMessage(events[i].parseEvent());
                }
            }
            else if (events.Count > 0)
            {
                for (var i = ((page-1)*7)-1; i < Math.Min(page*7-1, events.Count); i++)
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

        private void ParseRollback(TSPlayer ply, List<String> args)
        {
            // toss out the first one since its what dropped us into this.
            args.RemoveAt(0);
            int radius = 10;
            long date = 600;
            var name = "";
            var ip = "";
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
            Console.WriteLine(String.Format("Starting Rollback:\nRadius:{0}\nSince:{1}", radius, date) );
            var rollback = RollbackTiles(ply, radius, date, name, ip);
            Console.WriteLine(String.Format("Rollback Complete:\nTiles Rolled Back:{0}", rollback) );
        }

        public long RollbackTiles( TSPlayer ply, int radius, long time, string name, string ip )
        {
            var database = TShockAPI.TShock.DB;
            String query = "SELECT * FROM LogTile WHERE X BETWEEN @X1 AND @X2 AND Y BETWEEN @Y1 and @Y2 AND Date > @Date";
            
            if (name != "")
                query += " AND Name = @Name";
            if (ip != "")
                query += " AND Name = @IP";
           
            query += " ORDER BY id ASC;";

            var events = new List<TileEvent>();
            var reader = QueryReaderDict(database, query, new Dictionary<string, object>
                                                        {
                                                            {"X1", ply.TileX - radius},
                                                            {"X2", ply.TileX + radius},
                                                            {"Y1", ply.TileY - radius},
                                                            {"Y2", ply.TileY + radius},
                                                            {"Date", LogTile.helper.GetTime()-time},
                                                            {"Name", name},
                                                            {"IP", ip},
                                                        });

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

        public QueryResult QueryReaderDict(IDbConnection olddb, string query, Dictionary<string, object> values)
        {
            var db = olddb.CloneEx();
            db.Open();
            using (var com = db.CreateCommand())
            {
                com.CommandText = query;
                foreach (var kv in values)
                    com.AddParameter("@" + kv.Key, kv.Value);

                return new QueryResult(db, com.ExecuteReader());
            }
        }

        public void save()
        {
            log.saveQueue();
        }
    }
}
