using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace RootTools.Inspects
{
	public enum MYSQLError
	{
		CONNECT_ERROR = -2,//연결실패. 커스텀에러
		INIT_ERROR = -1,//초기화를 하지 않음. 커스텀 에러
		SUCCESS = 0,
		AUTH_FAIL = 1044, //권한없는찐따
		DB_NOT_FOUND = 1049,//1049 DB가 업서요
		QUERY_IS_NOT_CORRECT = 1064,//1064 쿼리문이 잘못됨
		TABLE_IS_MISSING = 1146,//1146 Table이 없음
		COLUMN_IS_MISMATCHING = 1136,//1136 컬럼개수가 일치하지않음 
		UNKNOWN_TABLE = 1051,
	}
	public class DBConnector
	{
		private MySqlConnection oCnn = null;
		private string connectStr;
		public bool IsInitialize { get; private set; }
		public bool Connected { get; private set; }

		public DBConnector(string serverName, string dbName, string uid, string pw)
		{
			connectStr = string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};", serverName, dbName, uid, pw);
			oCnn = new MySqlConnection(connectStr);//DB 연결
			IsInitialize = true;
			Connected = false;
		}
		public void Close()
		{
			oCnn.Close();
			oCnn.Dispose();
		}
		public bool Open()//TODO : try/catch문으로 error코드를 반환하도록 쉊ㅇ해야 함
		{
			try
			{
				oCnn.Open();
				Connected = true;
				return true;
			}
			catch
			{
				Connected = false;
				return false;
			}
		}
		public int SendNonQuery(string query)
		{
			if (!IsInitialize)
			{
				return -1;
			}
			if (!Connected)
			{
				return -2;
			}

			MySqlCommand cmd = new MySqlCommand(query, oCnn);

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (MySql.Data.MySqlClient.MySqlException ex)
			{
				return ex.Number;
			}

			return 0;
		}
		public int SendQuery(string query, ref string result)
		{
			if (!IsInitialize)
			{
				return -1;
			}
			if (!Connected)
			{
				return -2;
			}

			MySqlCommand cmd = new MySqlCommand(query, oCnn);

			try
			{
				using (MySqlDataReader Reader = cmd.ExecuteReader())
				{
					while (Reader.Read())
					{
						for (int i = 0; i < Reader.FieldCount; i++)
						{
							result += Reader.GetString(i);
						}
					}
				}
			}
			catch (MySql.Data.MySqlClient.MySqlException ex)
			{
				return ex.Number;
			}

			return 0;
		}

		public DataSet GetDataSet(string tableName)
		{
			//DataSet da;
			if (!IsInitialize)
			{
				return new DataSet();
			}
			if (!Connected)
			{
				return new DataSet();
			}
			DataSet result = new DataSet();

			string query = string.Format("SELECT * FROM inspections.tempdata;");
			MySqlDataAdapter da = new MySqlDataAdapter();
			MySqlCommand cmd = oCnn.CreateCommand();
			cmd.CommandText = query;
			da.SelectCommand = cmd;
			da.Fill(result, tableName);

			return result;
		}
	}
}
