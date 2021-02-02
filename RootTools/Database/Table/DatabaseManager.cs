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
		private DatabaseManager() { }
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

		public void SendQuery(string sQueryMessage) // Main
		{
#if !DEBUG
			try
			{
#endif
				MySqlCommand cmd = new MySqlCommand(sQueryMessage, m_MainConnectSession.GetConnection());
				cmd.ExecuteNonQuery();
				//return cmd.ExecuteNonQuery();

#if !DEBUG
			}
			catch (Exception ex)
			{
				string sMessage = ex.Message;
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
			string sTableClearQuery = "truncate " + _sTable;
			DatabaseManager.Instance.SendQuery(sTableClearQuery);
		}


		#region Data Select

		public void SelectData()
		{
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
				MessageBox.Show(ex.Message);
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
				MessageBox.Show("SelectTablewithInspectionID : " + ex.Message);
				table = data.Tables[0].Copy();
				return table;
			}

#endif
		}

		#endregion



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
				MessageBox.Show(ex.Message);
			}

#endif
		}

		public void AddDefectDataList(List<Defect> _defectlist)
		{
#if !DEBUG
			try
			{
#endif
				StringBuilder temp = new StringBuilder();
				StringBuilder sbQuery = new StringBuilder();
				StringBuilder sbColumList = new StringBuilder();
				StringBuilder sValueList = new StringBuilder();
				List<string> sbValueList = new List<string>();
				Type type = typeof(Defect);
				FieldInfo[] fld = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

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

				sbQuery.AppendFormat("INSERT INTO defect({0}) values", sbColumList.ToString());
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
