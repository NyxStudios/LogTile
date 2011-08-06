using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TShockAPI.DB;
using System.Net;

namespace LogTile
{
    class Logger
    {
        private TileQueue tileQueue;
        private bool isRunning = true;

        public Logger( TileQueue tq )
        {
            tileQueue = tq;
        }

        public void SaveTimer()
        {
            Thread.Sleep(60000);
            while( isRunning )
            {
                saveQueue();
                Thread.Sleep(60000);
            }
        }

        public void stop()
        {
            isRunning = false;
        }

        public void saveQueue()
        {
            var database = TShockAPI.TShock.DB;
            var queue = tileQueue.GetQueue();
            long count = queue.Count;
            for (var i = 0; i < count; i++)
            {
                TileEvent evt = queue.Dequeue();

                String query = "INSERT INTO LogTile (X, Y, IP, Name, Action, TileType) VALUES (@0, @1, @2, @3, @4, @5);";
                //reverse method for later String ipAddress = new IPAddress(BitConverter.GetBytes(intAddress)).ToString();
                int intAddress = BitConverter.ToInt32(IPAddress.Parse(evt.GetIP()).GetAddressBytes(), 0);
                database.Query(query, evt.GetX(), evt.GetY(), intAddress, evt.GetName(), evt.GetAction(), evt.GetTileType() );
            }
        }
    }
}
