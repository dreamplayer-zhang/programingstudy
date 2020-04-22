using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.SQLite;

namespace ATI
{
	public enum SQLiteTableCreateStatus
	{
		Fail = -2,
		ConfigIsNull = -1,
		Success = 0,
		AlreadyExist = 1,
	}
	public enum SQLiteRowInsertStatus
	{
		DataCountIsWrong = -4,
		TableIsNotExist = -3,
		Fail = -2,
		ConfigIsNull = -1,
		Success = 0,
	}
	public class SqliteDataDB
	{
		string DbFilePath { get; set; }
		string ConfigPath { get; set; }

		private SQLiteConnection sqliteDBconnect { get; set; }

		private SQLiteCommand sqLiteCmd;

		private SQLiteDataAdapter sqLiteAdapter { get; set; }

		public bool IsDBInitialized { get; private set; }
		public bool IsConfigInitialized { get; private set; }
		public bool IsConnected { get; private set; }
		Dictionary<string, List<string>> TableDictionary { get; set; }
		/// <summary>
		/// SQLite를 사용한 DB 컨트롤러. 파일에 대한 정합성은 별도로 확인하지 않음
		/// </summary>
		/// <param name="dbFilePath">Sqlite DB File 경로</param>
		/// <param name="configPath">DB Table Config File 경로</param>
		public SqliteDataDB(string dbFilePath, string configPath)
		{
			sqliteDBconnect = null;
			this.DbFilePath = dbFilePath;
			this.ConfigPath = configPath;
			TableDictionary = new Dictionary<string, List<string>>();

			if (this.DbFilePath != string.Empty)
			{
				if (File.Exists(this.DbFilePath))
				{
					IsDBInitialized = true;
				}
				else
				{
#if DEBUG
					Debug.WriteLine(string.Format("SqliteDataDB() - Some file is not exist : {0}", this.DbFilePath));
#endif
					IsDBInitialized = false;
				}
			}
			if (this.ConfigPath != string.Empty)
			{
				if (File.Exists(this.ConfigPath))
				{
					IsConfigInitialized = true;

					var configInfos = File.ReadAllLines(this.ConfigPath).Where(x => !x.StartsWith("//"));
					foreach (var line in configInfos.ToList())
					{
						var tableName = line.Split(',')[0];
						var tableInfo = line.Split(',').ToList().GetRange(1, line.Split(',').Count() - 1);
						TableDictionary.Add(tableName, tableInfo.ToList());
					}
				}
				else
				{
#if DEBUG
					Debug.WriteLine(string.Format("SqliteDataDB() - Some file is not exist : {0}", this.ConfigPath));
#endif
					IsConfigInitialized = false;
				}
				return;
			}
		}
		public void BeginWrite()
		{
			if (!IsConfigInitialized)
				return;
			if (!IsConnected)
				return;

			SQLiteCommand searchCommand = new SQLiteCommand("BEGIN;", sqliteDBconnect);
			var result = searchCommand.ExecuteNonQuery();
		}
		public void Commit()
		{
			if (!IsConfigInitialized)
				return;
			if (!IsConnected)
				return;

			SQLiteCommand searchCommand = new SQLiteCommand("COMMIT;", sqliteDBconnect);
			var result = searchCommand.ExecuteNonQuery();
		}
		/// <summary>
		/// Table에 data를 입력한다. 입력 순서는 초기화시에 사용하는 Config파일을 따라가므로 Config파일에 맞춰서 입력해줘야 합니다
		/// </summary>
		/// <param name="tableName">DB Table 이름</param>
		/// <param name="args">DB에 입력할 데이터</param>
		/// <returns></returns>
		internal SQLiteRowInsertStatus InsertRow(string tableName, params object[] args)
		{
			if (!this.IsConfigInitialized)
				return SQLiteRowInsertStatus.ConfigIsNull;

			string tableListQurey = "SELECT name FROM sqlite_master WHERE type IN ('table', 'view') AND name NOT LIKE 'sqlite_%' UNION ALL SELECT name FROM sqlite_temp_master WHERE type IN ('table', 'view') ORDER BY 1;";

			SQLiteCommand searchCommand = new SQLiteCommand(tableListQurey, sqliteDBconnect);
			var rdr = searchCommand.ExecuteReader();
			bool isTableExist = false;
			while (rdr.Read())
			{
				string s = rdr[0] as string;
				if (s == tableName)
				{
					isTableExist = true;
					break;
				}
			}
			if (!isTableExist)
			{
				return SQLiteRowInsertStatus.TableIsNotExist;
			}

			if (!TableDictionary.ContainsKey(tableName))
			{
				return SQLiteRowInsertStatus.ConfigIsNull;
			}

			if (args.Count() != TableDictionary[tableName].Count)
			{
				return SQLiteRowInsertStatus.DataCountIsWrong;
			}

			StringBuilder stbrInsertQuery = new StringBuilder();
			stbrInsertQuery.Append(string.Format("INSERT INTO {0} (", tableName));

			bool isFirst = true;
			List<string> dataType = new List<string>();
			foreach (var item in TableDictionary[tableName])
			{
				//*LotIndexID(INTEGER)
				//InspStartTime(TEXT)
				//BCRID(TEXT)
				var targetColumn = item.Replace("*", "").Split('(')[0].Trim();
				dataType.Add(item.Split('(')[1].Replace(")", ""));
				if (!isFirst)
				{
					stbrInsertQuery.Append(",");
				}
				stbrInsertQuery.Append(targetColumn);
				if (isFirst)
				{
					isFirst = false;
				}
			}
			stbrInsertQuery.Append(") VALUES (");

			isFirst = true;
			for (int i = 0; i < args.Count(); i++)
			{
				if (!isFirst)
				{
					stbrInsertQuery.Append(",");
				}
				var columnValue = args[i].ToString();
				if (dataType[i] == "TEXT")
				{
					stbrInsertQuery.Append(string.Format("'{0}'", columnValue));
				}
				else
				{
					stbrInsertQuery.Append(string.Format("{0}", columnValue));
				}
				if (isFirst)
				{
					isFirst = false;
				}
			}
			stbrInsertQuery.Append(");");

			SQLiteCommand command = new SQLiteCommand(stbrInsertQuery.ToString(), sqliteDBconnect);
			int makeresult = command.ExecuteNonQuery();

			if (makeresult == -1)
				return SQLiteRowInsertStatus.Fail;
			else
				return SQLiteRowInsertStatus.Success;
		}

		public SQLiteTableCreateStatus CreateTable(string tableName)
		{
			if (!this.IsConfigInitialized)
				return SQLiteTableCreateStatus.ConfigIsNull;
			if (!this.IsConnected)
				return SQLiteTableCreateStatus.Fail;
			//csv format

			string tableListQurey = "SELECT name FROM sqlite_master WHERE type IN ('table', 'view') AND name NOT LIKE 'sqlite_%' UNION ALL SELECT name FROM sqlite_temp_master WHERE type IN ('table', 'view') ORDER BY 1;";

			SQLiteCommand searchCommand = new SQLiteCommand(tableListQurey, sqliteDBconnect);
			var rdr = searchCommand.ExecuteReader();
			while (rdr.Read())
			{
				string s = rdr[0] as string;
				if (s == tableName)
				{
#if DEBUG
					Debug.WriteLine(string.Format("Table is already exist. name={0}", tableName));
#endif
					return SQLiteTableCreateStatus.AlreadyExist;
				}
			}

			string cmdTableMakeQuery = string.Format("create table {0} (", tableName);
			string index = null;
			string primarykey = null;
			//string autoincrement = null;
			int keyindexcount = 1;

			if (!TableDictionary.ContainsKey(tableName))
				return SQLiteTableCreateStatus.ConfigIsNull;

			var tableInfo = TableDictionary[tableName];

			foreach (string column in tableInfo)
			{
				string str = column.Replace("(", " ");
				str = str.Replace(")", "");

				if (str.Substring(0, 1) == "*")//*표시로 index 표시 해준 거
				{
					str = str.Substring(1);
					index = str.Substring(0, str.IndexOf(" "));
					cmdTableMakeQuery = cmdTableMakeQuery + str + ",";

				}
				else if (str.Substring(0, 1) == "@")//@표시로 Primary key표시 해준 거
				{
					str = str.Substring(1);
					primarykey = str.Substring(0, str.IndexOf(" "));
					cmdTableMakeQuery = cmdTableMakeQuery + str + " PRIMARY KEY,";

				}
				else
				{
					cmdTableMakeQuery = cmdTableMakeQuery + str + ",";
				}
			}
			cmdTableMakeQuery = cmdTableMakeQuery.Substring(0, cmdTableMakeQuery.Length - 1); //  "," 하나 더 붙었으니 삭제
			cmdTableMakeQuery = cmdTableMakeQuery + ")";

			SQLiteCommand command = new SQLiteCommand(cmdTableMakeQuery, sqliteDBconnect);
			int makeresult = command.ExecuteNonQuery();

			if (index != null)
			{
				cmdTableMakeQuery = string.Format("create index idx{0}_{1} on {1}({2})", keyindexcount.ToString("00"), tableName, index);
				command = new SQLiteCommand(cmdTableMakeQuery, sqliteDBconnect);
				makeresult = command.ExecuteNonQuery();
				keyindexcount++;
			}
			if (makeresult == -1)
				return SQLiteTableCreateStatus.Fail;
			else
				return SQLiteTableCreateStatus.Success;
		}

		private bool Create()
		{
			if (this.DbFilePath == "")
				return false;
			SQLiteConnection.CreateFile(this.DbFilePath);

			if (!System.IO.File.Exists(DbFilePath))
				return false;

			this.IsDBInitialized = true;
			return true;
		}
		/// <summary>
		/// DB에 접속한다. DB파일 경로 및 설정파일 경로가 초기화되어있지 않은 경우에는 진행되지 않는다
		/// </summary>
		/// <returns></returns>
		public bool Connect()
		{
			//DB Connect
			if (!IsDBInitialized)
			{
				if (!Create())
				{
					return false;
				}
			}
			bool result = true;
			SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
			builder.DataSource = this.DbFilePath;
			builder.Version = 3;

			sqliteDBconnect = new SQLiteConnection();
			sqliteDBconnect.ConnectionString = builder.ConnectionString;
			try
			{
				sqliteDBconnect = sqliteDBconnect.OpenAndReturn();
				result = true;
			}
			catch (Exception ex)
			{
				result = false;
#if DEBUG
				Debug.WriteLine(string.Format("Connect() - {0}", ex.Message));
#endif
			}
			IsConnected = result;
			return result;
		}
		/// <summary>
		/// DB와의 연결을 해제한다
		/// </summary>
		/// <returns></returns>
		public bool Disconnect()
		{
			try
			{
				if (sqliteDBconnect != null)
				{
					sqliteDBconnect.Close();
				}
				if (sqLiteCmd != null)
				{
					sqLiteCmd.Dispose();
				}
				if (sqLiteAdapter != null)
				{
					sqLiteAdapter.Dispose();
				}

				IsConnected = false;
				return true;
			}
			catch (Exception ex)
			{
#if DEBUG
				Debug.WriteLine(string.Format("Disconnect() - {0}", ex.Message));
#endif
				IsConnected = false;
				return false;
			}
		}
		/// <summary>
		/// 지정된 Table의 모든 데이터를 읽어들인 후 DataTable 형태로 반환한다.
		/// </summary>
		/// <param name="tablename">해당 DB에서 읽어올 Table의 이름</param>
		/// <returns></returns>
		public DataTable GetDataTable(string tablename)
		{
			if (!IsDBInitialized)
			{
#if DEBUG
				Debug.WriteLine(string.Format("GetDataTable() - This class is not initialized"));
#endif
				return new DataTable();//빈 DataTable을 반환
			}
			if (!IsConnected)
			{
#if DEBUG
				Debug.WriteLine(string.Format("GetDataTable() - This class is not connected with database"));
#endif
				return new DataTable();//빈 DataTable을 반환
			}
			DataTable DT = new DataTable();

			sqLiteCmd = sqliteDBconnect.CreateCommand();
			sqLiteCmd.CommandText = string.Format("SELECT * FROM {0}", tablename);
			sqLiteAdapter = new SQLiteDataAdapter(sqLiteCmd);
			sqLiteAdapter.AcceptChangesDuringFill = false;
			sqLiteAdapter.Fill(DT);

			DT.TableName = tablename;
			foreach (DataRow row in DT.Rows)
			{
				row.AcceptChanges();
			}
			sqLiteAdapter.Dispose();
			return DT;
		}
		/// <summary>
		/// 지정된 Table의 선택된 Column의 데이터를 읽어들인 후, DataTable 형태로 반환한다.
		/// </summary>
		/// <param name="tablename"></param>
		/// <param name="columns"></param>
		/// <returns></returns>
		public DataTable GetDataTable(string tablename, params string[] columns)
		{
			if (!IsDBInitialized)
			{
#if DEBUG
				Debug.WriteLine(string.Format("GetDataTable() - This class is not initialized"));
#endif
				return new DataTable();//빈 DataTable을 반환
			}
			if (!IsConnected)
			{
#if DEBUG
				Debug.WriteLine(string.Format("GetDataTable() - This class is not connected with database"));
#endif
				return new DataTable();//빈 DataTable을 반환
			}
			DataTable DT = new DataTable();

			sqLiteCmd = sqliteDBconnect.CreateCommand();
			StringBuilder StbrColumns = new StringBuilder();
			for (int i = 0; i < columns.Length; i++)
			{
				StbrColumns.Append(columns[i]);
				if (i + 1 != columns.Length)
				{
					StbrColumns.Append(",");
				}
			}
			sqLiteCmd.CommandText = string.Format("SELECT {0} FROM {1}", StbrColumns.ToString(), tablename);
			sqLiteAdapter = new SQLiteDataAdapter(sqLiteCmd);
			sqLiteAdapter.AcceptChangesDuringFill = false;
			sqLiteAdapter.Fill(DT);

			DT.TableName = tablename;
			foreach (DataRow row in DT.Rows)
			{
				row.AcceptChanges();
			}
			sqLiteAdapter.Dispose();
			return DT;
		}
		public void SaveDataTable(DataTable DT)
		{
			try
			{
				//sqliteDBconnect.Open();
				sqLiteCmd = sqliteDBconnect.CreateCommand();
				sqLiteCmd.CommandText = string.Format("SELECT * FROM {0}", DT.TableName);
				sqLiteAdapter = new SQLiteDataAdapter(sqLiteCmd);
				SQLiteCommandBuilder builder = new SQLiteCommandBuilder(sqLiteAdapter);
				sqLiteAdapter.Update(DT);
				//sqliteDBconnect.Close();
			}
			catch (Exception)
			{
				//System.Windows.MessageBox.Show(Ex.Message);
			}
		}
		public DataTable StartQuery(string tablename, string query)
		{
			if (!IsDBInitialized)
			{
#if DEBUG
				Debug.WriteLine(string.Format("StartQuery() - This class is not initialized"));
#endif
				return new DataTable();//빈 DataTable을 반환
			}
			if (!IsConnected)
			{
#if DEBUG
				Debug.WriteLine(string.Format("StartQuery() - This class is not connected with database"));
#endif
				return new DataTable();//빈 DataTable을 반환
			}
			DataTable DT = new DataTable();

			sqLiteCmd = sqliteDBconnect.CreateCommand();
			sqLiteCmd.CommandText = query;
			sqLiteAdapter = new SQLiteDataAdapter(sqLiteCmd);
			sqLiteAdapter.AcceptChangesDuringFill = false;
			sqLiteAdapter.Fill(DT);

			DT.TableName = tablename;
			foreach (DataRow row in DT.Rows)
			{
				row.AcceptChanges();
			}
			sqLiteAdapter.Dispose();
			return DT;
		}
	}
}