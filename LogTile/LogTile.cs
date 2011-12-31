using System;
using System.Data;
using System.IO;
using System.Threading;
using Community.CsharpSqlite.SQLiteClient;
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
		private Thread CommandQueueThread;
		public static TileHelper helper;
		private Commands com;
		public static  bool enableDebugOutput = false;
		private Thread fileWriter;
		public static bool isRunning = true;
		public static IDbConnection DB;

		public override Version Version
		{
			get { return new Version("1.2"); }
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
			Order = 5;
		}

		public override void Initialize()
		{
			StartLogTile();
		}

		public void StartLogTile()
		{
			queue = new TileQueue(this);
			log = new Logger(queue);
			helper = new TileHelper();
			com = new Commands(log);

			if (TShock.Config.StorageType.ToLower() == "sqlite")
			{
				string sql = Path.Combine(TShock.SavePath, "logtile.sqlite");
				DB = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
			}
			else if (TShock.Config.StorageType.ToLower() == "mysql")
			{
				try
				{
					var hostport = TShock.Config.MySqlHost.Split(':');
					DB = new MySqlConnection();
					DB.ConnectionString =
						String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
									  hostport[0],
									  hostport.Length > 1 ? hostport[1] : "3306",
									  TShock.Config.MySqlDbName,
									  TShock.Config.MySqlUsername,
									  TShock.Config.MySqlPassword
							);
				}
				catch (MySqlException ex)
				{
					Log.Error(ex.ToString());
					throw new Exception("MySql not setup correctly");
				}
			}
			else
			{
				throw new Exception("Invalid storage type");
			}
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
			var creator = new SqlTableCreator(DB,
											  DB.GetSqlType() == SqlType.Sqlite
			                                  	? (IQueryBuilder) new SqliteQueryCreator()
			                                  	: new MysqlQueryCreator());
			creator.EnsureExists(table);

			logThread = new Thread(log.SaveTimer);
			CommandQueueThread = new Thread( CommandQueue.ProcessQueue);

			logThread.Start();
			CommandQueueThread.Start();

			queue.addHook();
			com.addHook();

			fileWriter = new Thread(ConfigFileManager);
			fileWriter.Start();
		}

		protected void ConfigFileManager()
		{
			ConfigFile cfg = new ConfigFile(this);
			cfg.WriteConfigFile();
			cfg.ReadConfigFile();
			enableDebugOutput = Configuration.enableDebugOutput;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				TShock.Initialized -= StartLogTile;
				fileWriter.Abort();
				queue.closeHook();
				com.closeHook();
				log.saveQueue( null );
				isRunning = false;
				logThread.Abort();
				CommandQueueThread.Abort();

			}
		}
	}
}