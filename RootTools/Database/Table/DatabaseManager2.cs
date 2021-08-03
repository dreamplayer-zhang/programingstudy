using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Emgu.CV.Stitching;

// DB
using MySql.Data.MySqlClient;
using RootTools.Database;
using System.Data.SqlClient;
using System.Reflection;


namespace RootTools.Database
{
	// Singleton. 프로젝트 어디서나 바로바로 객체 없이호출.
	// DB Manager 역할.
	// 스레드 & CONNECTION 관리
	// 쿼리 및 데이터 관리.
	
	public class DatabaseManager2 : ObservableObject
	{
		//#region Singleton
		//private DatabaseManager() { }
		//static readonly DatabaseManager _DBManager = new DatabaseManager();
		////private static readonly Lazy<DatabaseManager> instacne = new Lazy<DatabaseManager>(() => new DatabaseManager());
		//public static DatabaseManager Instance
		//{
		//	get
		//	{
		//		return _DBManager;
		//	}
		//}


		//#endregion		
		//#region Singleton
		//private DatabaseManager() { }
		//static readonly DatabaseManager _DBManager = new DatabaseManager();
		////private static readonly Lazy<DatabaseManager> instacne = new Lazy<DatabaseManager>(() => new DatabaseManager());
		//public static DatabaseManager Instance
		//{
		//	get
		//	{
		//		return _DBManager;
		//	}
		//}


		//#endregion

		private Database_ConnectSession2 m_MainConnectSession; // Query 용
		//private Database_ConnectSession[] m_ThreadConnectSession; // 스레드 연결세션
		private MySqlCommand m_mySqlCommand; // 쿼리

		protected Lotinfo2 m_Loftinfo = new Lotinfo2(); // 현재 Lot정보
		protected string m_sInspectionID; // INSPECTION ID(DB PRIMARY KEY)
		public string InspectionID 
		{ 
			get 
			{ 
				return m_sInspectionID; 
			}
			set
			{
				m_sInspectionID = value;
			}
		}

		public bool Connected { get; private set; }

		//public DataSet m_DataSet = new DataSet();
		//public DataTable m_DefectTable = new DataTable();
		//public DataTable pDefectTable
		//{
		//	get => m_DefectTable;
		//	set => SetProperty(ref m_DefectTable, value);
		//}


		public bool GetConnectionStatus()
		{
			if (!Connected)
			{
				TempLogger.Write("Database", "GetConnectionStatus() DB is not initialized");
				return false;
			}
			if (m_MainConnectSession != null)
				return m_MainConnectSession.GetConnectionState();
			else
				return false;
		}

		public bool ValidateDatabase(string dbName, string tableName)
		{
			try
			{
				if (!Connected)
				{
					TempLogger.Write("Database", "ValidateDatabase() DB is not initialized");
					return false;
				}

				DataSet data = new DataSet();
				string sSelectQuery = string.Format("select distinct TABLE_NAME from INFORMATION_SCHEMA.columns where table_schema='{0}'", dbName); // Temp
				MySqlCommand cmd = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());
				MySqlDataReader rdr = cmd.ExecuteReader();
				List<string> tableList = new List<string>();
				while (rdr.Read())
				{
					object table = rdr["TABLE_NAME"];
					tableList.Add(table.ToString());
					//sSelectQuery = "select column_name from INFORMATION_SCHEMA.columns where table_schema='wind2' and table_name='"+a.ToString()+"'";
					//cmd = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());
					//MySqlDataReader rdr2 = cmd.ExecuteReader();
					//while (rdr2.Read())
					//               {
					//	object b = rdr["COLUMN_NAME"];
					//}
					////for ()

				}
				rdr.Close();
				//bool isSame = true;
				FieldInfo[] defectFieldInfos = null;
				List<string> columnList = new List<string>();
				for (int i = 0; i < tableList.Count; i++)
				{
					if (tableList[i].Equals(tableName))
					{
						Type defectType = typeof(Defect);
						defectFieldInfos = defectType.GetFields(BindingFlags.Instance | BindingFlags.Public);
					}
					sSelectQuery = string.Format("select column_name from INFORMATION_SCHEMA.columns where table_schema='{0}' and table_name='{1}'", dbName, tableList[i]);
					cmd = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());

					rdr = cmd.ExecuteReader();
					//int a = 0;
					while (rdr.Read())
					{
						object columnName = rdr["COLUMN_NAME"];
						columnList.Add(columnName.ToString());
					}

					object[] tt = defectFieldInfos[i].GetCustomAttributes(true);
					ObsoleteAttribute ob = (ObsoleteAttribute)tt[0];
					string test = ob.Message;
					if (defectFieldInfos.Length != columnList.Count)
                    {
						//isSame = false;
						break;
                    }
				}
				rdr.Close();
				return true;
			}
			catch(Exception e)
			{
				TempLogger.Write("Database", e);
				return false;
			}
		}


		public DatabaseManager2(string sServerName = "localhost", string sDBName = "wind2", string sUid = "root", string sPw = "root")
		{
			if(Connected)
			{
				m_MainConnectSession.Disconnect();
			}
			Connected = false;
			m_sInspectionID = "";


			m_MainConnectSession = new Database_ConnectSession2(sServerName, sDBName, sUid, sPw);//(int)ID.MainQuery, 
			Connected = m_MainConnectSession.Connect(); // 메인세션

			m_mySqlCommand = new MySqlCommand();

			//if (_nThreadCount <= 1) return false;
			//else
			//{
			//	m_ThreadConnectSession = new Database_ConnectSession[_nThreadCount];
			//	for (int i = 0; i < _nThreadCount; i++)
			//	{
			//		m_ThreadConnectSession[i] = new Database_ConnectSession(i, sServerName, sDBName, sUid, sPw);
			//		bConnect = m_ThreadConnectSession[i].Connect(); // 스레드 세션
			//	}
			//}
			//return bConnect;
		}
		~DatabaseManager2()
		{
			m_mySqlCommand.Dispose();
		}


		//public void ThreadConnect(int nThreadCount)
		//{
		//	for (int i = 0; i < nThreadCount; i++)
		//	{
		//		m_ThreadConnectSession[i] = new Database_ConnectSession(i);
		//		m_ThreadConnectSession[i].Connect();
		//	}
		//}

		public int SendQuery(string sQueryMessage) // Main
		{
			if (m_MainConnectSession.IsConnected == false)
			{
				TempLogger.Write("Database", "SendQuery() m_MainConnectSession is not initialized");
				return -1;
			}
#if !DEBUG
			try
			{

#endif
				MySqlCommand cmd = new MySqlCommand(sQueryMessage, m_MainConnectSession.GetConnection());
				return cmd.ExecuteNonQuery();
				//return cmd.ExecuteNonQuery();

#if !DEBUG
			}
			catch (Exception ex)
			{
				string sMessage = ex.Message;
				return -1;
			}

#endif
		}

//		public int SendThreadQuery(int nThreadID, string sQueryMessage) // Inspection Thread
//		{
//#if !DEBUG
//			try
//			{

//#endif
//				MySqlCommand cmd = new MySqlCommand(sQueryMessage, m_ThreadConnectSession[nThreadID].GetConnection());
//				return cmd.ExecuteNonQuery();
//#if !DEBUG
//			}
//			catch (Exception ex)
//			{
//				return -1;
//			}

//#endif
//		}

		public void ClearTableData(string _sTable)
		{
			if(CheckExistTable(_sTable))
			{
				string sTableClearQuery = "truncate " + _sTable;
				SendQuery(sTableClearQuery);
			}
		}


		#region Data Select

		public DataTable SelectData(string dataBase, string tableName)
		{
			if (m_MainConnectSession.IsConnected == false)
				return null;
#if !DEBUG
			try
			{

#endif
			DataSet data = new DataSet();
			string sSelectQuery = string.Format("SELECT * FROM {0}.{1}", dataBase, tableName); // Temp
			MySqlDataAdapter ap = new MySqlDataAdapter();


			ap.SelectCommand = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());
			ap.Fill(data); // DataSet으로 전체 데이터 복사.
			ap.Dispose();
			return data.Tables[0].Copy();
#if !DEBUG
			}
			catch (Exception ex)
			{
				string sMsg = ex.Message;
				return null;
			}
#endif
		}


		public DataTable SelectTable(string dbName, string sTable)
		{
			if(!Connected)
			{
				TempLogger.Write("Database","SelectTable() - Error. Is Not Connected");
				return new DataTable();
			}
			DataSet data = new DataSet();
			DataTable table;
#if !DEBUG
			try
			{

#endif
				string sSelectQuery = string.Format("SELECT * FROM {0}.{1};", dbName, sTable);
				MySqlDataAdapter ap = new MySqlDataAdapter();
				ap.SelectCommand = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());
				ap.Fill(data);
				table = data.Tables[0].Copy();
				return table;
#if !DEBUG
			}
			catch (Exception ex)
			{
				TempLogger.Write("Database", ex);
				table = data.Tables[0].Copy();
				return table;
			}
#endif
		}

		public DataTable SelectTablewithInspectionID(string sTable, string sInspectionID)
		{
			if (!Connected)
			{
				TempLogger.Write("Database", "SelectTablewithInspectionID() - Error. Is Not Connected");
				return new DataTable();
			}
			DataSet data = new DataSet();
			DataTable table;
#if !DEBUG
			try
			{
#endif
				string sSelectQuery = string.Format("SELECT * FROM wind2.{0} where m_strInspectionID = '{1}';", sTable, sInspectionID);
				MySqlDataAdapter ap = new MySqlDataAdapter();
				ap.SelectCommand = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());
				ap.Fill(data);
				table = data.Tables[0].Copy();
				return table;
#if !DEBUG
			}
			catch (Exception ex)
			{
				TempLogger.Write("Database", ex);
				table = data.Tables[0].Copy();
				return table;
			}

#endif
		}

		#endregion



		public void AddDefectData(Defect _defect)
		{
			if (!Connected)
			{
				TempLogger.Write("Database", "AddDefectData() - Error. Is Not Connected");
			}
			//string sError;
#if !DEBUG
			try
			{

#endif
			StringBuilder sbQuery = new StringBuilder();
				StringBuilder sbColumList = new StringBuilder();
				StringBuilder sbValueList = new StringBuilder();
				Type type = typeof(Defect);
				FieldInfo[] fld = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
				for (int i = 0; i < fld.Length; i++)
				{
					var f = fld[i];
					object obj = f.GetValue(_defect);
					sbColumList.Append(f.Name);
					sbValueList.Append("'" + obj + "'");
					if (i != fld.Length - 1)
					{
						sbColumList.Append(",");
						sbValueList.Append(",");
					}
				}
				sbQuery.AppendFormat("INSERT INTO defect({0}) values({1})", sbColumList.ToString(), sbValueList.ToString());
				SendQuery(sbQuery.ToString());
#if !DEBUG
			}
			catch (Exception ex)
			{
				TempLogger.Write("Database", ex);
			}

#endif
		}
		public bool CheckExistTable(string tableName)
		{
			if (!Connected)
			{
				TempLogger.Write("Database", "CheckExistTable() - Error. Is Not Connected");
			}
			string query = string.Format("SELECT * FROM {0};", tableName);
			try
			{
				var code = SendQuery(query);
				if (code == -1)
					return true;//다른 에러일 수도 있으니 예외처리등이 필요? ㅁ?ㄹ
				else
					return false;//추가 예외처리 필요
			}
			catch (MySqlException ex)
			{
				if (ex.Code == (int)DB_ERROR.TABLE_IS_MISSING)
				{
					return false;
				}
				else
				{
					return false;//추가 예외처리 필요
				}
			}
		}
		public void CreateTable(string tableName, Type type, string keyName)
		{
			if (!Connected)
			{
				TempLogger.Write("Database", "CreateTable() - Error. Is Not Connected");
			}
			StringBuilder stbr = new StringBuilder();
			stbr.Append(string.Format("CREATE TABLE {0} (",tableName));
			FieldInfo[] fld = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			for (int i = 0; i < fld.Count(); i++)
			{
				stbr.Append(fld[i].Name);
				string nullAble = " NULL";
				if(keyName == fld[i].Name)
				{
					nullAble = " NOT NULL AUTO_INCREMENT";
				}
				nullAble += ",";

				if (fld[i].FieldType == typeof(int))
				{
					stbr.Append(" int(11) " + nullAble);
				}
				else if (fld[i].FieldType == typeof(float))
				{
					stbr.Append(" double "+ nullAble);
				}
				else if (fld[i].FieldType == typeof(string) || fld[i].FieldType == typeof(String))
				{
					stbr.Append(" varchar(45) "+ nullAble);
				}
			}
			stbr.Append(string.Format("PRIMARY KEY ({0})) ENGINE=InnoDB DEFAULT CHARSET=utf8;", keyName));
			var result = SendQuery(stbr.ToString());
		}

		public void AddDefectDataList(List<Defect> _defectlist, string tableName)
		{
			if (!Connected)
			{
				TempLogger.Write("Database", "AddDefectDataList() - Error. Is Not Connected");
			}
#if !DEBUG
			try
			{
#endif
			if (!CheckExistTable(tableName))
				{
					//var tableQuery = string.Format("CREATE TABLE '{0}' ('m_nDefectIndex' int(11) NOT NULL AUTO_INCREMENT,'m_strInspectionID' varchar(45) DEFAULT NULL,'m_nDefectCode' int(11) DEFAULT NULL,'m_fSize' double DEFAULT NULL,'m_fWidth' double DEFAULT NULL,'m_fHeight' double DEFAULT NULL,'m_fRelX' double DEFAULT NULL,'m_fRelY' double DEFAULT NULL,'m_fAbsX' double DEFAULT NULL,'m_fAbsY' double DEFAULT NULL,'m_fGV' double DEFAULT NULL,'m_nChipIndexX' int(11) DEFAULT NULL,'m_nCHipIndexY' int(11) DEFAULT NULL,PRIMARY KEY ('m_nDefectIndex')) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;", tableName);
					CreateTable(tableName, typeof(Defect),nameof(Defect.m_nDefectIndex));
				}
				StringBuilder temp = new StringBuilder();
				StringBuilder sbQuery = new StringBuilder();
				StringBuilder sbColumList = new StringBuilder();
				StringBuilder sValueList = new StringBuilder();
				List<string> sbValueList = new List<string>();
				Type type = typeof(Defect);
				FieldInfo[] fld = type.GetFields(BindingFlags.Instance | BindingFlags.Public).Where(x => x.Name != nameof(Defect.m_nDefectIndex).ToString()).ToArray();

				for (int defectListNum = 0; defectListNum < _defectlist.Count; defectListNum++)
				{
					temp.Clear();
					sbColumList.Clear();
					for (int i = 0; i < fld.Length; i++)
					{
						var f = fld[i];
						object obj = f.GetValue(_defectlist[defectListNum]);
						sbColumList.Append(f.Name);
						if (i == 0)
							temp.Append("(");

						temp.AppendFormat("'{0}'", obj);

						if (i != fld.Length - 1)
						{
							sbColumList.Append(",");
							temp.Append(",");
						}
						else
							temp.Append(")");
					}
					sbValueList.Add(temp.ToString());
				}

				sbQuery.AppendFormat("INSERT INTO {0}({1}) values", tableName, sbColumList.ToString());
				for (int i = 0; i < sbValueList.Count; i++)
				{
					sbQuery.Append(sbValueList[i]);
					if (i != sbValueList.Count - 1)
						sbQuery.Append(",");
				}
				 SendQuery(sbQuery.ToString());
#if !DEBUG
			}
			catch (Exception ex)
			{
				TempLogger.Write("Database", ex);
			}

#endif
		}

		public void AddMeasurementDataList(List<Measurement> _measurelist)
		{
			if (!Connected)
			{
				TempLogger.Write("Database", "AddMeasurementDataList() - Error. Is Not Connected");
			}
#if !DEBUG
			try
			{
#endif
			SendQuery("TRUNCATE measurement;");
			StringBuilder temp = new StringBuilder();
			StringBuilder sbQuery = new StringBuilder();
			StringBuilder sbColumList = new StringBuilder();
			StringBuilder sValueList = new StringBuilder();
			List<string> sbValueList = new List<string>();
			Type type = typeof(Measurement);
			FieldInfo[] fld = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

			for (int measureListNum = 0; measureListNum < _measurelist.Count; measureListNum++)
			{
				temp.Clear();
				sbColumList.Clear();
				for (int i = 0; i < fld.Length; i++)
				{
					var f = fld[i];
					object obj = f.GetValue(_measurelist[measureListNum]);
					sbColumList.Append(f.Name);
					if (i == 0)
						temp.Append("(");

					temp.AppendFormat("'{0}'", obj);

					if (i != fld.Length - 1)
					{
						sbColumList.Append(",");
						temp.Append(",");
					}
					else
						temp.Append(")");
				}
				sbValueList.Add(temp.ToString());
			}

			sbQuery.AppendFormat("INSERT INTO measurement({0}) values", sbColumList.ToString());
			for (int i = 0; i < sbValueList.Count; i++)
			{
				sbQuery.Append(sbValueList[i]);
				if (i != sbValueList.Count - 1)
					sbQuery.Append(",");
			}
			SendQuery(sbQuery.ToString());
#if !DEBUG
			}
			catch (Exception ex)
			{
				MessageBox.Show("DB Query Error : (AddDefectDataList)" + ex.Message);
			}

#endif
		}

		public void SetLotinfo(string lotid, string partid, string setupid, string cstid, string waferid, string recipeid)
		{
			if (!Connected)
			{
				TempLogger.Write("Database", "SetLotinfo() - Error. Is Not Connected");
			}
			//Inspection ID 생성(KEY)
			m_Loftinfo.SetLotinfo(lotid, partid, setupid, cstid, waferid, recipeid);
			m_sInspectionID = MakeInspectionID(m_Loftinfo);
			string sLotinfoQuery;
			sLotinfoQuery = string.Format("INSERT INTO lotinfo(INSPECTIONID, LOTID, CSTID, WAFERID, RECIPEID)" +
				" values('{0}','{1}','{2}','{3}','{4}')"
				, m_sInspectionID
				, m_Loftinfo.GetLotID()
				, m_Loftinfo.GetCSTID()
				, m_Loftinfo.GetWaferID()
				, m_Loftinfo.GetRecipeID()
				);
			SendQuery(sLotinfoQuery);
		}
		public string MakeInspectionID(Lotinfo2 _Lotinfo)
		{
			string sResult = "";
			string sTime = DateTime.Now.ToString("yyyyMMddHHmmss");
			sResult = string.Format("{0}{1}{2}", _Lotinfo.GetLotID(), _Lotinfo.GetCSTID(), sTime);
			return sResult;
		}

		public string GetInspectionID()
		{
			return m_sInspectionID;
		}
	}
}
