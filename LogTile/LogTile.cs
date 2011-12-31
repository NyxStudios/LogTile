using System;
using System.Threading;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;
using Terraria;

namespace LogTile
{
	[APIVersion(1, 10)]
	public class LogTile : TerrariaPlugin
	{
		private TileQueue queue;
		private Logger log;
		private Thread logThread;
		public static TileHelper helper;
		private Commands com;

		public override Version Version
		{
			get { return new Version("1.1"); }
		}

		public override string Name
		{
			get { return "Log Tile"; }
		}

		public override string Author
		{
			get { return "Zach Piispanen"; }
		}

		public override string Description
		{
			get { return "Logs all your tile edits into a database."; }
		}

		public LogTile(Main game)
			: base(game)
		{
		}

		public override void Initialize()
		{
			while (TShock.DB == null)
				Thread.Sleep(500);
			StartLogTile();
		}

		public void StartLogTile()
		{
			queue = new TileQueue();
			log = new Logger(queue);
			helper = new TileHelper();
			com = new Commands(log);

			var database = TShock.DB;

			var table = new SqlTable("LogTile",
			                         new SqlColumn("id", MySqlDbType.Int32) {Primary = true, AutoIncrement = true},
			                         new SqlColumn("X", MySqlDbType.Int32),
			                         new SqlColumn("Y", MySqlDbType.Int32),
			                         new SqlColumn("IP", MySqlDbType.Int32),
			                         new SqlColumn("Name", MySqlDbType.Text),
			                         new SqlColumn("Action", MySqlDbType.Int32),
			                         new SqlColumn("TileType", MySqlDbType.Int32),
			                         new SqlColumn("Date", MySqlDbType.Int32)
				);
			var creator = new SqlTableCreator(database,
			                                  database.GetSqlType() == SqlType.Sqlite
			                                  	? (IQueryBuilder) new SqliteQueryCreator()
			                                  	: new MysqlQueryCreator());
			creator.EnsureExists(table);

			logThread = new Thread(log.SaveTimer);

			logThread.Start();
			queue.addHook();
			com.addHook();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				queue.closeHook();
				com.closeHook();
				log.saveQueue();
				log.stop();
				logThread.Abort();
			}
		}
	}
}