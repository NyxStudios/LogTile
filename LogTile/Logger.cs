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
            var queue = tileQueue.GetQueue();
            lock (queue)
            {
                long count = queue.Count;
                if (count > 0)
                {
                    Console.WriteLine("LogTile queue is saving to db...");
                    var database = TShockAPI.TShock.DB;

                    for (var i = 0; i < count; i++)
                    {
                        TileEvent evt = queue.Dequeue();

                        String query =
                            "INSERT INTO LogTile (X, Y, IP, Name, Action, TileType, Date) VALUES (@0, @1, @2, @3, @4, @5, @6);";
                        //reverse method for later String ipAddress = new IPAddress(BitConverter.GetBytes(intAddress)).ToString();
                        int intAddress = BitConverter.ToInt32(IPAddress.Parse(evt.GetIP()).GetAddressBytes(), 0);
                        if( database.Query(query, evt.GetX(), evt.GetY(), intAddress, evt.GetName(), evt.GetAction(),
                                       evt.GetTileType(), evt.GetDate()) < 1 )
                            queue.Enqueue( evt );
                    }
                    Console.WriteLine("LogTile has finished writing to db. " + count + " edits were saved.");
                }
                else
                {
                    Console.WriteLine("Queue is empty.");
                }
            }
        }
    }
}
