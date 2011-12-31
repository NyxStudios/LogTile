using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Streams;
using Hooks;
using TShockAPI;
using Terraria;

namespace LogTile
{
	internal class TileQueue
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
					var name = player.Name;
					if (player.IsLoggedIn)
					{
						name = player.UserAccountName;
					}
					switch (packet)
					{
						case PacketTypes.Tile:
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
									TileEvent evt = new TileEvent(x, y, name, player.IP, act, tileType,
									                              LogTile.helper.GetTime());
									queue.Enqueue(evt);
								}
								break;
							}
						case PacketTypes.TileKill:
							{
								int x = data.ReadInt32();
								int y = data.ReadInt32();
								TileEvent evt = new TileEvent(x, y, name, player.IP, Action.BREAK, 0x15,
								                              LogTile.helper.GetTime());
								queue.Enqueue(evt);
								break;
							}
						case PacketTypes.ChestOpen:
							{
								int chestID = data.ReadInt16();
								int x = data.ReadInt32();
								int y = data.ReadInt32();
								int curChest = 0;
								if (!chestMap.TryGetValue(player, out curChest)) // chest being opened
								{
									chestMap.Add(player, chestID);
									itemMap.Add(player, Main.chest[chestID].item);
								}
								else // chest is being closed
								{
									chestMap.Remove(player);
									itemMap.Remove(player);
								}

								break;
							}
						case PacketTypes.ChestItem:
							{
								int chestID = data.ReadInt16();
								byte itemSlot = data.ReadInt8();
								byte stack = data.ReadInt8();
								int curChest = 0;
								int type = itemMap[player][itemSlot].type;
								Console.WriteLine(type);
								Item[] curItems = Main.chest[chestID].item;
								Console.WriteLine(curItems[itemSlot].type);
								itemMap.Remove(player);
								itemMap.Add(player, curItems);
								break;
							}
						case PacketTypes.ChestGetContents:
							{
								int x = data.ReadInt32();
								int y = data.ReadInt32();
								Console.WriteLine("GETChestContents: (" + x + ", " + y + ")");
								break;
							}
						case PacketTypes.SignNew:
							{
								int id = data.ReadInt16();
								int x = data.ReadInt32();
								int y = data.ReadInt32();
								string text = data.ReadString();
								break;
							}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
}