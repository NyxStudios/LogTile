using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TShockAPI;

namespace LogTile
{
	static public class CommandQueue
	{
		private static List<AbstractCommand> queue = new List<AbstractCommand>();

		public static void AddCommand(AbstractCommand cmd, TSPlayer ply)
		{
			if( ply != null )
				ply.SendMessage( String.Format("There are currently {0} items in the queue.  Your request will be added to the bottom.", queue.Count), Color.Green);
			lock( queue )
			{
				queue.Add(cmd);
			}
		}

		static public void ProcessQueue()
		{
			
			while( LogTile.isRunning)
			{
				if( queue.Count > 0 )
				{
					AbstractCommand cmd;
					lock( queue )
					{
						cmd = queue[0];
					}
					cmd.Execute();
					lock (queue)
					{
						queue.RemoveAt(0);
					}
				}
				Thread.Sleep(100);
			}
		}
	}
}
