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
	public enum ID
	{
		MainQuery = 100,
	}
	public enum DB_ERROR
	{
		CONNECT_ERROR = -2,//연결실패. 커스텀에러
		INIT_ERROR = -1,//초기화를 하지 않음. 커스텀 에러
		SUCCESS = 0, // 성공
		AUTH_FAIL = 1044, //권한 없음
		DB_NOT_FOUND = 1049,//DB 찾을 수 없음.
		QUERY_IS_NOT_CORRECT = 1064,//쿼리문이 잘못됨
		TABLE_IS_MISSING = 1146,//Table이 없음
		COLUMN_IS_MISMATCHING = 1136,//컬럼개수가 일치하지않음 
		UNKNOWN_TABLE = 1051, // 알수 없는 테이블 이름
	}

	public class DatabaseManager : ObservableObject
	{
		#region Singleton
		public DatabaseManager() { }
		static readonly DatabaseManager _DBManager = new DatabaseManager();
		//private static readonly Lazy<DatabaseManager> instacne = new Lazy<DatabaseManager>(() => new DatabaseManager());
		public static DatabaseManager Instance
		{
			get
			{
				return _DBManager;
			}
		}


		#endregion

		private Database_ConnectSession m_MainConnectSession; // Query 용
		private Database_ConnectSession[] m_ThreadConnectSession; // 스레드 연결세션
		private MySqlCommand m_mySqlCommand; // 쿼리

		protected Lotinfo m_Loftinfo = new Lotinfo(); // 현재 Lot정보
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

		public DataSet m_DataSet = new DataSet();
		public DataTable m_DefectTable = new DataTable();
		public DataTable pDefectTable
		{
			get => m_DefectTable;
			set => SetProperty(ref m_DefectTable, value);
		}


		public bool GetConnectionStatus()
		{
			if (m_MainConnectSession != null)
				return m_MainConnectSession.GetConnectionState();
			else
				return false;
		}

		public bool ValidateDatabase()
		{
			try
			{
				DataSet data = new DataSet();
				string sSelectQuery = "select distinct TABLE_NAME from INFORMATION_SCHEMA.columns where table_schema='wind2'"; // Temp
				MySqlCommand cmd = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());
				MySqlDataReader rdr = cmd.ExecuteReader();
				List<string> tableList = new List<string>();
				List<string> columnListTest = new List<string>();
				while (rdr.Read())
				{
					object table = rdr["TABLE_NAME"];
					tableList.Add(table.ToString());
				}
				rdr.Close();

				bool isSame = true;
				FieldInfo[] defectFieldInfos = null;
				FieldInfo[] lotinfoFieldInfos = null;
				List<string> columnList = new List<string>();

				for (int i = 0; i < tableList.Count; i++)
				{
					sSelectQuery = "select column_name from INFORMATION_SCHEMA.columns where table_schema='wind2' and table_name='" + tableList[i] + "'";
					cmd = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());
					rdr = cmd.ExecuteReader();

					while (rdr.Read())
					{
						object columnName = rdr["COLUMN_NAME"];
						columnList.Add(columnName.ToString());
					}
					rdr.Close();

					if (tableList[i].Equals("defect"))
					{
						Type defectType = typeof(Defect);
						defectFieldInfos = defectType.GetFields(BindingFlags.Instance | BindingFlags.Public);

						if (defectFieldInfos.Length != columnList.Count)
						{
							columnList = new List<string>();
							isSame = false;
							break;
						}
                        else
                        {
							for (int j = 0; j < defectFieldInfos.Length; j++)
							{
								if (!defectFieldInfos[j].Name.Equals(columnList[j]))
								{
									//object[] tt = defectFieldInfos[j].GetCustomAttributes(true);
									//ObsoleteAttribute ob = (ObsoleteAttribute)tt[0];
									//string test = ob.Message;

									columnList = new List<string>();
									isSame = false;
									break;
								}
							}
							if (!isSame)
							{
								break;
							}
						}
					}
					else if (tableList[i].Equals("lotinfo"))
					{
						Type lotinfoType = typeof(Lotinfo);
						lotinfoFieldInfos = lotinfoType.GetFields(BindingFlags.Instance | BindingFlags.Public);

						if (lotinfoFieldInfos.Length != columnList.Count)
						{
							columnList = new List<string>();
							isSame = false;
							break;
						}
                        else
                        {
							for (int j = 0; j < lotinfoFieldInfos.Length; j++)
							{
								if (!lotinfoFieldInfos[j].Name.Equals(columnList[j]))
								{
									//object[] tt = lotinfoFieldInfos[j].GetCustomAttributes(true);
									//ObsoleteAttribute ob = (ObsoleteAttribute)tt[0];
									//string test = ob.Message;

									columnList = new List<string>();
									isSame = false;
									break;
								}
							}
							if (!isSame)
							{
								columnList = new List<string>();
								break;
							}
						}
					}
					columnList = new List<string>();
				}

				if (tableList.Count == 0)
				{
					isSame = false;
				}

				if (!isSame)
				{
					if (tableList.Count > 0)
					{
						if (MessageBox.Show("Current DB component version does not match. Do you want to backup old DB then create new DB?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
						{
							// Backup and Reset DB
							string strDBTime = "wind2_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
							sSelectQuery = "create database `" + strDBTime + "`";
							cmd = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());
							rdr = cmd.ExecuteReader();
							rdr.Close();

							sSelectQuery = "rename table `wind2`.`defect` to `" + strDBTime + "`.`defect`, `wind2`.`lotinfo` to `" + strDBTime + "`.`lotinfo`";
							cmd = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());
							rdr = cmd.ExecuteReader();
							rdr.Close();

							// Create empty tables
							CreateTable("defect", typeof(Defect), nameof(Defect.m_nDefectIndex));
							CreateTable("lotinfo", typeof(Lotinfo), nameof(Lotinfo.sInspectionID));
						}
					}
					else if (tableList.Count == 0)
                    {
						if (MessageBox.Show("Current DB table is empty. Do you want to create new DB?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
						{
							// Create empty tables
							CreateTable("defect", typeof(Defect), nameof(Defect.m_nDefectIndex));
							CreateTable("lotinfo", typeof(Lotinfo), nameof(Lotinfo.sInspectionID));
						}
					}

					isSame = true;
				}
				return true;
			}
			catch(Exception e)
			{
				TempLogger.Write("Database", e);
				return false;
			}
		}


		public bool SetDatabase(int _nThreadCount, string sServerName = "localhost", string sDBName = "wind2", string sUid = "root", string sPw = "root")
		{
			bool bConnect = false;

			m_sInspectionID = "";


			m_MainConnectSession = new Database_ConnectSession((int)ID.MainQuery, sServerName, sDBName, sUid, sPw);
			bConnect = m_MainConnectSession.Connect(); // 메인세션

			m_mySqlCommand = new MySqlCommand();

			if (_nThreadCount <= 1) return false;
			else
			{
				m_ThreadConnectSession = new Database_ConnectSession[_nThreadCount];
				for (int i = 0; i < _nThreadCount; i++)
				{
					m_ThreadConnectSession[i] = new Database_ConnectSession(i, sServerName, sDBName, sUid, sPw);
					bConnect = m_ThreadConnectSession[i].Connect(); // 스레드 세션
				}
			}
			return bConnect;
		}


		public void ThreadConnect(int nThreadCount)
		{
			for (int i = 0; i < nThreadCount; i++)
			{
				m_ThreadConnectSession[i] = new Database_ConnectSession(i);
				m_ThreadConnectSession[i].Connect();
			}
		}
		public DataSet GetDataSet(string sQueryMessage) // Main
		{
			if (m_MainConnectSession.IsConnected == false)
				return null;
#if !DEBUG
			try
			{

#endif
			MySqlCommand cmd = new MySqlCommand(sQueryMessage, m_MainConnectSession.GetConnection());

			MySqlDataReader table = cmd.ExecuteReader();
			DataSet ds = new DataSet();
			DataTable dataTable = new DataTable();
			ds.Tables.Add(dataTable);
			ds.EnforceConstraints = false;
			dataTable.Load(table);
			table.Close();

			return ds;

#if !DEBUG
			}
			catch (Exception ex)
			{
				string sMessage = ex.Message;
				DataSet ds = new DataSet();
				return ds;
			}

#endif
		}
		public int SendQuery(string sQueryMessage) // Main
		{
			if (m_MainConnectSession.IsConnected == false)
				return -1;
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

		public int SendThreadQuery(int nThreadID, string sQueryMessage) // Inspection Thread
		{
#if !DEBUG
			try
			{

#endif
				MySqlCommand cmd = new MySqlCommand(sQueryMessage, m_ThreadConnectSession[nThreadID].GetConnection());
				return cmd.ExecuteNonQuery();
#if !DEBUG
			}
			catch (Exception ex)
			{
				return -1;
			}

#endif
		}

		public void ClearTableData(string _sTable)
		{
			if(CheckExistTable(_sTable))
			{
				string sTableClearQuery = "truncate " + _sTable;
				DatabaseManager.Instance.SendQuery(sTableClearQuery);
			}
		}


		#region Data Select

		public void SelectData()
		{
			if (m_MainConnectSession.IsConnected == false)
				return;
#if !DEBUG
			try
			{

#endif
				DataSet data = new DataSet();
				string sSelectQuery = "SELECT * FROM wind2.defect"; // Temp
				MySqlDataAdapter ap = new MySqlDataAdapter();
			
			
				ap.SelectCommand = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());
				ap.Fill(data); // DataSet으로 전체 데이터 복사.
				m_DefectTable = data.Tables[0].Copy();
#if !DEBUG
			}
			catch (Exception ex)
			{
				string sMsg = ex.Message;
			}
#endif
		}

		public void SelectData(string tableName)
		{
			if (m_MainConnectSession.IsConnected == false)
				return;
#if !DEBUG
			try
			{

#endif
			DataSet data = new DataSet();
			string sSelectQuery = "SELECT * FROM wind2." + tableName; // Temp
			MySqlDataAdapter ap = new MySqlDataAdapter();


			ap.SelectCommand = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());
			ap.Fill(data); // DataSet으로 전체 데이터 복사.
			m_DefectTable = data.Tables[0].Copy();
#if !DEBUG
			}
			catch (Exception ex)
			{
				string sMsg = ex.Message;
			}
#endif
		}


		public DataTable SelectTable(string sTable)
		{
			DataSet data = new DataSet();
			DataTable table;
#if !DEBUG
			try
			{

#endif
				string sSelectQuery = string.Format("SELECT * FROM wind2.{0};", sTable);
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
		public DataTable SelectTableDatetime(string sTable, string startDatetime, string endDatetime)
		{
			DataSet data = new DataSet();
			DataTable table;

			FieldInfo[] lotinfoFieldInfos = null;
			Type lotinfoType = typeof(Lotinfo);
			lotinfoFieldInfos = lotinfoType.GetFields(BindingFlags.Instance | BindingFlags.Public);
#if !DEBUG
			try
			{
#endif

			string sSelectQuery = null;
			if (startDatetime != null && endDatetime != null)
            {
				sSelectQuery = string.Format("SELECT * FROM wind2.{0} WHERE DATE({1}) BETWEEN '{2}' AND '{3}';", sTable, lotinfoFieldInfos[0].Name, startDatetime, endDatetime);
			}
			else if (startDatetime != null && endDatetime == null)
            {
				sSelectQuery = string.Format("SELECT * FROM wind2.{0} WHERE DATE({1}) >= '{2}';", sTable, lotinfoFieldInfos[0].Name, startDatetime);
			}
			else if (startDatetime == null && endDatetime != null)
            {
				sSelectQuery = string.Format("SELECT * FROM wind2.{0} WHERE DATE({1}) =< '{2}';", sTable, lotinfoFieldInfos[0].Name, endDatetime);
			}
			
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


		public DataTable SelectCurrentInspectionDefect(string tableName = "defect")
		{
			DataSet data = new DataSet();
			DataTable table;
#if !DEBUG
			try
			{
#endif
			string sSelectQuery = string.Format("SELECT * FROM wind2.{0} where m_strInspectionID = '{1}';", tableName, m_sInspectionID);
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



		public void AddDefectData(Defect _defect)
		{
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
			StringBuilder stbr = new StringBuilder();
			stbr.Append(string.Format("CREATE TABLE {0} (",tableName));
			FieldInfo[] fld = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			for (int i = 0; i < fld.Count(); i++)
			{
				stbr.Append(fld[i].Name);
				string nullAble = " NULL";
				if(keyName == fld[i].Name)
				{
					if (fld[i].FieldType == typeof(int))
                    {
						nullAble = " NOT NULL AUTO_INCREMENT";
					}
					else
					{
						nullAble = " NOT NULL";
					}
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
				else if (fld[i].FieldType == typeof(DateTime))
				{
					stbr.Append(" datetime " + nullAble);
				}
			}
			stbr.Append(string.Format("PRIMARY KEY ({0})) ENGINE=InnoDB DEFAULT CHARSET=utf8;", keyName));
			var result = SendQuery(stbr.ToString());
		}

		public void AddDefectDataList(List<Defect> _defectlist, string tableName)
		{
#if !DEBUG
			try
			{
#endif
				if(tableName == "null" && !CheckExistTable(tableName))
				{
					//var tableQuery = string.Format("CREATE TABLE '{0}' ('m_nDefectIndex' int(11) NOT NULL AUTO_INCREMENT,'m_strInspectionID' varchar(45) DEFAULT NULL,'m_nDefectCode' int(11) DEFAULT NULL,'m_fSize' double DEFAULT NULL,'m_fWidth' double DEFAULT NULL,'m_fHeight' double DEFAULT NULL,'m_fRelX' double DEFAULT NULL,'m_fRelY' double DEFAULT NULL,'m_fAbsX' double DEFAULT NULL,'m_fAbsY' double DEFAULT NULL,'m_fGV' double DEFAULT NULL,'m_nChipIndexX' int(11) DEFAULT NULL,'m_nChipIndexY' int(11) DEFAULT NULL,PRIMARY KEY ('m_nDefectIndex')) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;", tableName);
				//CreateTable(tableName, typeof(Defect),nameof(Defect.m_nDefectIndex));
					return;
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


		public void AddDefectDataListNoAutoCount(List<Defect> _defectlist, string tableName)
		{
#if !DEBUG
			try
			{
#endif
			if (tableName == "null" && !CheckExistTable(tableName))
			{
				//var tableQuery = string.Format("CREATE TABLE '{0}' ('m_nDefectIndex' int(11) NOT NULL AUTO_INCREMENT,'m_strInspectionID' varchar(45) DEFAULT NULL,'m_nDefectCode' int(11) DEFAULT NULL,'m_fSize' double DEFAULT NULL,'m_fWidth' double DEFAULT NULL,'m_fHeight' double DEFAULT NULL,'m_fRelX' double DEFAULT NULL,'m_fRelY' double DEFAULT NULL,'m_fAbsX' double DEFAULT NULL,'m_fAbsY' double DEFAULT NULL,'m_fGV' double DEFAULT NULL,'m_nChipIndexX' int(11) DEFAULT NULL,'m_nChipIndexY' int(11) DEFAULT NULL,PRIMARY KEY ('m_nDefectIndex')) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;", tableName);
				//CreateTable(tableName, typeof(Defect),nameof(Defect.m_nDefectIndex));
				return;
			}
			StringBuilder temp = new StringBuilder();
			StringBuilder sbQuery = new StringBuilder();
			StringBuilder sbColumList = new StringBuilder();
			StringBuilder sValueList = new StringBuilder();
			List<string> sbValueList = new List<string>();
			Type type = typeof(Defect);
			FieldInfo[] fld = type.GetFields(BindingFlags.Instance | BindingFlags.Public).ToArray();

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

		public void SetLotinfo(DateTime inspectionstart, DateTime inspectionend, string lotid, string partid, string setupid, string cstid, string waferid, string recipeid)
		{
			//Inspection ID 생성(KEY)
			m_Loftinfo.SetLotinfo(inspectionstart, inspectionend, lotid, partid, setupid, cstid, waferid, recipeid);
			m_sInspectionID = MakeInspectionID(m_Loftinfo);
			string sLotinfoQuery;

			sLotinfoQuery = string.Format("INSERT INTO lotinfo(InspectionStart, InspectionEnd, sInspectionID, sLotID, sCSTID, sWaferID, sRecipeID)" +
				" values('{0}','{1}','{2}','{3}','{4}','{5}','{6}')"
				, inspectionstart.ToString("yyyyMMddHHmmss")
				, inspectionend.ToString("yyyyMMddHHmmss")
				, m_sInspectionID
				, m_Loftinfo.GetLotID()
				, m_Loftinfo.GetCSTID()
				, m_Loftinfo.GetWaferID()
				, m_Loftinfo.GetRecipeID()
				);
			SendQuery(sLotinfoQuery);
		}

		public void SetLotinfo(DateTime inspectionstart, DateTime inspectionend, string recipeid)
		{
			//Inspection ID 생성(KEY)
			m_Loftinfo.SetLotinfo(inspectionstart, inspectionend, "LotID", "CSTID", "SetupID", "CSID", "WaferID", recipeid.Replace(".rcp", ""));
			m_sInspectionID = MakeInspectionID(m_Loftinfo);
			string sLotinfoQuery;

			sLotinfoQuery = string.Format("INSERT INTO lotinfo(InspectionStart, InspectionEnd, sInspectionID, sLotID, sCSTID, sWaferID, sRecipeID)" +
				" values('{0}','{1}','{2}','{3}','{4}','{5}','{6}')"
				, inspectionstart.ToString("yyyyMMddHHmmss")
				, inspectionend.ToString("yyyyMMddHHmmss")
				, m_sInspectionID
				, "null"
				, "null"
				, "null"
				, recipeid.Replace(".rcp", "")
				);
			SendQuery(sLotinfoQuery);
		}

		public void SetLotinfo(Lotinfo lotInfo)
		{
			//Inspection ID 생성(KEY)
			m_Loftinfo.SetLotinfo(lotInfo);
			m_sInspectionID = MakeInspectionID(m_Loftinfo);
			string sLotinfoQuery;

			sLotinfoQuery = string.Format("INSERT INTO lotinfo(InspectionStart, InspectionEnd, sInspectionID, sLotID, sCSTID, sWaferID, sRecipeID)" +
				" values('{0}','{1}','{2}','{3}','{4}','{5}','{6}')"
				, m_Loftinfo.InspectionStart.ToString("yyyyMMddHHmmss")
				, m_Loftinfo.InspectionEnd.ToString("yyyyMMddHHmmss")
				, m_sInspectionID
				, m_Loftinfo.GetLotID()
				, m_Loftinfo.GetCSTID()
				, m_Loftinfo.GetWaferID()
				, m_Loftinfo.GetRecipeID()
				);
			SendQuery(sLotinfoQuery);
		}


		public string MakeInspectionID(Lotinfo _Lotinfo)
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
