using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogTile;
using Action = LogTile.Action;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main(string[] args)
		{
			TileHelper helper = new TileHelper();
			Console.WriteLine( "Starting a time test of filling a list.");
			List<TileEvent> list = new List<TileEvent>();
			int start = DateTime.Now.Millisecond;
			for (int i = 0; i < 100; i++ )
			{
				for (int j = 0; j < 100; j++)
				{
					TileEvent evt = new TileEvent( i, j, "", "", Action.PLACE, i+j, DateTime.Now.Millisecond );
					list.Add( evt );
				}
			}
			int end = DateTime.Now.Millisecond;
			Console.WriteLine( "Time taken: {0}", (end - start));

			Console.WriteLine( "Starting a time test of iterating through the loop.");
			List<TileEvent> list2 = new List<TileEvent>();
			int start2 = DateTime.Now.Millisecond;
			for (int i = 0; i < 100; i++ )
			{
				for (int j = 0; j < 100; j++)
				{
				}
			}
			int end2 = DateTime.Now.Millisecond;
			Console.WriteLine( "Time taken: {0}", (end2 - start2));

			Console.Read();
		}
	}
}
