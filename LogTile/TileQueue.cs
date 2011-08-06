using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using TShockAPI.Extensions;
using TShockAPI;
using System.IO;
using XNAHelpers;
using System.Threading;

namespace LogTile
{
    public enum Action
    {
        ERROR = 0,
        PLACE = 1,
        BREAK = 2
    };

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
            Console.WriteLine("Tile Logger is starting up...");
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
                    byte tiletype = data.ReadInt8();
                    TileEvent evt = new TileEvent(x, y, player.Name, player.IP, ((type == 0 || type == 2 || type == 4) ? Action.BREAK : Action.PLACE ) );
                    queue.Enqueue( evt );
                }
            }
        }
    }

    public class TileEvent
    {
        private int x;
        private int y;
        private string name;
        private string ip;
        private Action action;

        public TileEvent()
        {
            createEvent( 0, 0, "", "", 0 );
        }

        public TileEvent(int x, int y, string name, string ip, Action action)
        {
            createEvent(x, y, name, ip, action);
        }

        private void createEvent(int x, int y, String name, String ip, Action a)
        {
            this.x = x;
            this.y = y;
            this.name = name;
            this.ip = ip;
            this.action = a;
        }

        public int GetX() { return x; }
        public int GetY() { return y; }
        public String GetName() { return name; }
        public String GetIP() { return ip; }
        public Action GetAction() { return action; }
    }
}
