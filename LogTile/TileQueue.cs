using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using TShockAPI.Extensions;
using TShockAPI;
using System.IO;
using XNAHelpers;
using System.Threading;

namespace LogTile
{
    class TileQueue
    {
        private volatile Queue<TileEvent> queue;

        public TileQueue()
        {
            queue = new Queue<TileEvent>();
        }

        public Queue<TileEvent> GetQueue()
        {
            return queue;
        }
        public void addHook()
        {
            NetHooks.GetData += ParseData;
        }

        public void closeHook()
        {
            NetHooks.GetData -= ParseData;
        }

        private void ParseData(GetDataEventArgs args)
        {
            PacketTypes packet = args.MsgID;
            
            

            using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
            {        
                if( packet == PacketTypes.Tile || packet == PacketTypes.TileKill )
                {
                    TSPlayer player = TShock.Players[args.Msg.whoAmI];
                    byte type = data.ReadInt8();
                    int x = data.ReadInt32();
                    int y = data.ReadInt32();

                    Action act;
                    if (type == 0 || type == 2 || type == 4)
                        act = Action.BREAK;
                    else if (type == 1 || type == 3)
                        act = Action.PLACE;
                    else
                        act = Action.ERROR;

                    byte tileType;

                    if (act == Action.BREAK)
                        tileType = Main.tile[x, y].type;
                    else
                        tileType = data.ReadInt8();

                    if (act != Action.ERROR)
                    {
                        TileEvent evt = new TileEvent(x, y, player.Name, player.IP, act, tileType, LogTile.helper.GetTime());
                        queue.Enqueue(evt);
                    }
                }
            }
        }
    }
}
