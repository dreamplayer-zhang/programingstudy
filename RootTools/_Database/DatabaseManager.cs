using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Emgu.CV.Stitching;

using System.Data;

// DB
using MySql.Data.MySqlClient;
using RootTools.Database;
using System.Data.SqlClient;


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

    public class DatabaseManager
    {
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

        private Database_ConnectSession m_MainConnectSession; // Query 용
        private Database_ConnectSession[] m_ThreadConnectSession; // 스레드 연결세션
        private MySqlCommand m_mySqlCommand; // 쿼리

        protected Lotinfo m_Loftinfo = new Lotinfo(); // 현재 Lot정보
        protected string m_sInspectionID; // INSPECTION ID(DB PRIMARY KEY)

        public DataSet m_DataSet = new DataSet();
        public DataTable m_DefectTable = new DataTable();

        public bool SetDatabase(int _nThreadCount)
        {
            bool bConnect = false;
            m_sInspectionID = "";

            m_MainConnectSession = new Database_ConnectSession((int)ID.MainQuery);
            bConnect = m_MainConnectSession.Connect(); // 메인세션

            m_mySqlCommand = new MySqlCommand();

            if (_nThreadCount < 1) return false;
            else
            {
                m_ThreadConnectSession = new Database_ConnectSession[_nThreadCount];
                for (int i = 0; i < _nThreadCount; i++)
                {
                    m_ThreadConnectSession[i] = new Database_ConnectSession(i);
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

        public int SendQuery(string sQueryMessage) // Main
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand(sQueryMessage, m_MainConnectSession.GetConnection());
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return -1;
            }

        }

        public int SendThreadQuery(int nThreadID, string sQueryMessage) // Inspection Thread
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand(sQueryMessage, m_ThreadConnectSession[nThreadID].GetConnection());
                return cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                return -1;
            }
        }

        public string MakeInsertQuery(string _sTableName)
        {
            string sQuery = "INSERT";
            //        sLotinfoQuery = string.Format("INSERT INTO templotinfo(INSPECTIONID, LOTID, CSTID, WAFERID, RECIPEID)" +
            //" values('{0}','{1}','{2}','{3}','{4}')"

            return sQuery;

        }


        public int CopyTableData(string sSrc, string sDst)
        {
            string sQuery = "INSERT INTO " + sDst + " SELECT * FROM " + sSrc;
            return SendQuery(sQuery);
        }

        public void ClearTableData(string _sTable)
        {
            string sTableClearQuery = "truncate " + _sTable;
            DatabaseManager.Instance.SendQuery(sTableClearQuery);
        }


        //public DataSet SelectData()
        //{
        //    DataSet data = new DataSet();
        //    string sSelectQuery = "SELECT * FROM defectlist";

        //    MySqlDataAdapter ap = new MySqlDataAdapter();
        //    ap.SelectCommand = new MySqlCommand(sSelectQuery, m_MainConnectSession.GetConnection());

        //    ap.Fill(data); // DataSet으로 전체 데이터 복사.
        //    m_DefectTable = data.Tables[0].Copy();
        //    return m_DataSet;
        //}



        #region #LOTINFO, INSPECTIONID 생성
        public void SetLotinfo(string lotid, string partid, string setupid, string cstid, string waferid, string recipeid)
        {
            // 매 Lot 시작시 마다 호출되며, 해당 데이터와 함께 Defect Insert시에 row에 입력됨.
            // 현재 Lot데이터 입력
            m_Loftinfo.SetLotinfo(lotid, partid, setupid, cstid, waferid, recipeid);

            //Inspection ID 생성(KEY)
            m_sInspectionID = MakeInspectionID(m_Loftinfo);

            string sLotinfoQuery;
            sLotinfoQuery = string.Format("INSERT INTO templotinfo(INSPECTIONID, LOTID, CSTID, WAFERID, RECIPEID)" +
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
            string sTime = DateTime.Now.ToString("yyMMddHHMM");
            sResult = string.Format("{0}{1}{2}", _Lotinfo.GetLotID(), _Lotinfo.GetCSTID(), sTime);
            return sResult;
        }

        public string GetInspectionID()
        {
            return m_sInspectionID;
        }

        public Lotinfo GetCurrentLotInfo()
        {
            return m_Loftinfo;
        }

        #endregion



    }
}
