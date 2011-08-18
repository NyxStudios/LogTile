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
        private Dictionary<TSPlayer, int> chestMap;
        private Dictionary<TSPlayer, Item[]> itemMap;
        public TileQueue()
        {
            queue = new Queue<TileEvent>();
            chestMap = new Dictionary<TSPlayer, int>();
            itemMap = new Dictionary<TSPlayer, Item[]>();
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
            try
            {
                PacketTypes packet = args.MsgID;
                using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
                {
                    TSPlayer player = TShock.Players[args.Msg.whoAmI];
                    if (packet == PacketTypes.Tile)
                    {
                        byte type = data.ReadInt8();
                        int x = data.ReadInt32();
                        int y = data.ReadInt32();
                        bool fail = true;
                        Action act;
                        if (type == 0 || type == 2 || type == 4)
                            act = Action.BREAK;
                        else if (type == 1 || type == 3)
                            act = Action.PLACE;
                        else
                            act = Action.ERROR;

                        byte tileType = 0;

                        if (act == Action.BREAK)
                        {
                            tileType = Main.tile[x, y].type;
                            fail = data.ReadBoolean();
                        }
                        else if (act == Action.PLACE)
                        {
                            tileType = data.ReadInt8();
                            fail = false;
                        }
                        if (act != Action.ERROR && !fail)
                        {
                            TileEvent evt = new TileEvent(x, y, player.Name, player.IP, act, tileType,
                                                          LogTile.helper.GetTime());
                            queue.Enqueue(evt);
                        }
                    }
                    else if (packet == PacketTypes.TileKill)
                    {
                        int x = data.ReadInt32();
                        int y = data.ReadInt32();
                        TileEvent evt = new TileEvent(x, y, player.Name, player.IP, Action.BREAK, 0x15,
                                                      LogTile.helper.GetTime());
                        queue.Enqueue(evt);
                    }
                    else if( packet == PacketTypes.ChestOpen )
                    {
                        int chestID = data.ReadInt16();
                        int x = data.ReadInt32();
                        int y = data.ReadInt32();
                        int curChest = 0;
                        if( !chestMap.TryGetValue( player, out curChest ) )
                        {
                            chestMap.Add( player, chestID );
                            itemMap.Add(player, Main.chest[chestID].item);
                        }
                        else
                        {
                            chestMap.Remove(player);
                            itemMap.Remove(player);
                        }
                    }
                    else if (packet == PacketTypes.ChestItem)
                    {
                        int chestID = data.ReadInt16();
                        byte itemSlot = data.ReadInt8();
                        byte stack = data.ReadInt8();
                        int curChest = 0;
                        Console.WriteLine( chestID );
                        if (chestMap.TryGetValue(player, out curChest) && chestID == curChest)
                        {
                            Item[] curItems = Main.chest[chestID].item;
                            Item[] oldItems = itemMap[ player ];
                            Item c_it = curItems[itemSlot];
                            Item o_it = oldItems[itemSlot];


                            Console.WriteLine("Item: " + c_it.type + " Old: " + o_it.type);
                        }
                        Console.WriteLine( curChest );

                    }
                    else if (packet == PacketTypes.ChestGetContents)
                    {
                        int x = data.ReadInt32();
                        int y = data.ReadInt32();
                        Console.WriteLine( "GETChestContents: (" +x +  ", " + y + ")");
                    }
                }
            } catch( Exception e )
            {
                Console.WriteLine( e.Message);
            }
        }
    }
}
