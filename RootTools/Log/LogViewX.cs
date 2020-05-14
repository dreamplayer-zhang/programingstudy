using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace RootTools
{
    #region Description
    // WPF LogView
    // NLog 외부 라이브러리 사용 https://nlog-project.org/
    // Target : 파일, 메모리 등의 Log를 저장할 장소, 하나의 로그가 여러개의 Target을 가질 수 있다.
    // Level : Log의 중요도에 따라 다음과 같이 나뉜다 { Trace, Debug, Info, Warn, Error, Fatal }
    // Rules : Target과 Level 조합

    // Main Class
    // LogView m_logView = new LogView();
    // m_logView.Init("LogView", m_sModel);
    // ....
    // m_logView.ThreadStop();

    // 사용 Class
    // NLog.Logger m_log; 
    // m_log = m_logView.GetLog(m_id, sGroup);
    // m_log.Info("Log Here");  
    #endregion

    public class LogViewX : ObservableObject
    {
        public enum eMaterial
        {
            Wafer,
            Cassette
        };
        public enum eLogType
        {
            FNC,
            XFR,
            PRC,
            LEH,
            ALM,
            CFG,
            COMM,
            ENG,
        };
        public enum eStatus
        {
            START,
            END__,
        };
        public enum eLayOut
        {
            longdate,
            level,
            logger,
            message,
            stacktrace,
        }
        public enum eLotEvent
        {
            TRACK_IN,
            TRACK_OUT,
            CST_IN,
            CST_OUT,
            LOT_IN,
            LOT_OUT,
            MERGE,
            SPLIT,
        }
        public enum eStateALM
        {
            OCCURRED,
            RELEASED,
        }
        public enum eEQP_Stop
        {
            NONE,
            USER_STOP,
            RCMD_STOP,
            ERROR_STOP,
        }
        public enum eCFGType
        {
            CHANGE,
            SETTING,
            SAVE,
        }

        public enum eCOMMType
        {
            SEND,
            RECV,
        }   

        ObservableCollection<LogGroup> m_aGroup = new ObservableCollection<LogGroup>();
        public ObservableCollection<LogGroup> p_aGroup
        {
            get
            {
                return m_aGroup;
            }
            set
            {
                SetProperty(ref m_aGroup,value);
            }
        }
        //string m_id;
        static string m_sLogPath = "c:\\Log";     // Log 저장 폴더, 레지스트리에 저장은 되지만 편집 기능 구현 안됨
        LogGroup m_GroupTotal;                   // Info 이상 Fatal 이하의 Log
        LogGroup m_GroupMarsTotal;

        public void Init()
        {
            InitDefaultGroup();         // Info (전체 Log), Error Log 생성
            StartTimer();
        }

        void InitDefaultGroup()
        {
            m_GroupTotal = new LogGroup(eLogType.ENG, "Total", NLog.LogLevel.Info, NLog.LogLevel.Fatal);
            m_aGroup.Add(m_GroupTotal);
            m_GroupMarsTotal = new LogGroup(eLogType.FNC, "TotalMars", NLog.LogLevel.Info, NLog.LogLevel.Fatal);
            m_aGroup.Add(m_GroupMarsTotal);

            for (int i = 0; i < Enum.GetNames(typeof(eLogType)).Length; i++ )
            {
                string str = Enum.GetName(typeof(eLogType),i).ToString();
                m_aGroup.Add( new LogGroup((eLogType)i, str, NLog.LogLevel.Info, NLog.LogLevel.Fatal));
                var config = new NLog.Config.LoggingConfiguration();    // 현재 설정 값
                var Factory = new NLog.LogFactory();                    // 중간 사용 변수
                InitLogGroup((eLogType)i, config, Enum.GetName(typeof(eLogType),i).ToString());                           // sGroup에 해당하는 LogGroup 가져오거나 생성
                Factory.Configuration = config;
            }
        }

        public void ThreadStop()
        {
            NLog.LogManager.Shutdown();
        }

        static string GetLayout()
        {
            string str = "";

            for (int i = 0; i < Enum.GetNames(typeof(eLayOut)).Length; i++)
            {
                str += "${";            //" ${time}, ${logger}, ${level}, ${message}, ${exception}, ${stacktrace}";
                str += Enum.GetName(typeof(eLayOut), i).ToString();
                str += "}\t";
            }
            return str;
        }

        #region UI Property
        bool _bHold = false;
        public bool p_bHold
        {
            get { return _bHold; }
            set
            {
                SetProperty(ref _bHold, value);
                RaisePropertyChanged("p_bVisible");
            }
        }

        public Visibility p_bVisible
        {
            get { return p_bHold ? Visibility.Visible : Visibility.Hidden; }
        }


        string sTest = "teststet";
        public string p_sTest
        {
            get
            {
                return sTest;
            }
            set
            {
                SetProperty(ref sTest, value);
            }
        }
        public string p_sColume { get; set; }
        public string p_sFilter { get; set; }
        #endregion

        
        #region GetLogger
        // LogView에서 NLog.Logger를 새로 할당해서 받아옴
        public LogWriter GetLog(eLogType logType, string ClassID, string sGroup = "") 
        {   
            if (logType != eLogType.ENG)
            {
                sGroup = logType.ToString();
                ClassID = "Mars";
            }
            var config = new NLog.Config.LoggingConfiguration();    // 현재 설정 값
            var Factory = new NLog.LogFactory();                    // 중간 사용 변수
            InitLogGroup(logType, config, sGroup);                           // sGroup에 해당하는 LogGroup 가져오거나 생성
            Factory.Configuration = config;
            NLog.Logger logger = Factory.GetLogger(ClassID);             // NLog.Logger 할당
            //if (bLog) logger.Info("Get Logger -> " + sGroup);
            LogWriter writer = new LogWriter(logger, this);
            return writer;
        }

        
        // NLog.Logger가 어느 LogGroup에 Log를 남길지 설정함
        void InitLogGroup(eLogType func, NLog.Config.LoggingConfiguration config, string sGroup)
        {
            if (func == eLogType.ENG)
                m_GroupTotal.AddRule(config);    // Info에 Log 남김
            else
                m_GroupMarsTotal.AddRule(config);

            if (sGroup == "") return;
            if (sGroup == null) return;
            foreach (LogGroup logGroup in p_aGroup)     // 기존에 같은 이름의 LogGroup이 있는지 찾음
            {
                if (logGroup.m_sGroupName == sGroup)
                {
                    logGroup.AddRule(config);
                    return;
                }
            }
            LogGroup group = new LogGroup(func, sGroup, NLog.LogLevel.Info, NLog.LogLevel.Fatal);
            group.AddRule(config);  // 없으면 LogGroup 새로 할당해서 Log 남김
            p_aGroup.Add(group);
        }
        #endregion

        #region Timer Invalid
        DispatcherTimer m_timer = new DispatcherTimer();
        void StartTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(250);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (p_bHold) return;
            foreach (LogGroup group in p_aGroup) group.CalcDataTable();
        }
        #endregion

        public class LogDatas
        {
            public string m_sDate
            {
                get;
                set;
            }
            public string m_sLevel
            {
                get;
                set;
            }
            public Color p_sColor { get; set; }
            public string m_sLogger
            {
                get;
                set;
            }
            public string m_sMessage
            {
                get;
                set;
            }
            public string m_sStack
            {
                get;
                set;
            }

            public LogDatas(string logs)
            {
                string[] sLogs = logs.Split('\t');
                if (sLogs.Length != 6)
                    return;
                m_sDate = sLogs[0];
                m_sLevel = sLogs[1];
                m_sLogger = sLogs[2];
                m_sMessage = sLogs[3];

                for (int i = 0; i < Enum.GetNames(typeof(eLogType)).Length -1; i++)
                {
                    if(m_sMessage.IndexOf(Enum.GetName(typeof(eLogType),i)) >= 0)
                    {
                        m_sLevel = Enum.GetName(typeof(eLogType), i);
                    }
                }
                switch (m_sLevel)
                {
                    case "Error": p_sColor = Colors.Red; break;
                    case "Warn": p_sColor = Colors.Purple; break;
                    default: p_sColor =  Colors.Black; break; 
                }

                m_sStack = sLogs[4];
            }
        }
        #region LogGroup Class
        // 다수의 Class에서 선언된 NLog.Logger 가 같은 Group을 사용하는 것을 관리 하기 위한 Class
        public class LogGroup : ObservableObject
        {
            const int c_lLog = 500;                    // DataGrid에 표시될 Log의 최대 개수
            public string m_sGroupName
            {
                get;
                set;
            }
            public NLog.Targets.MemoryTarget m_memory;  // DataGrid에 Log를 추가하기 위해 임시로 저장 되는 Memory Target
            public NLog.Targets.FileTarget m_file;      // File로 Log를 남기기 위한 Target
            NLog.LogLevel m_lvMin;                      // 최소 레벨
            NLog.LogLevel m_lvMax;                      // 최대 레벨
            //DataTable m_dataTable = new DataTable();    // DataTable
            //public DataTable p_dataTable
            //{
            //    get
            //    {
            //        return m_dataTable;
            //    }
            //}
            ObservableCollection<LogDatas> m_LogDatas = new ObservableCollection<LogDatas>();
            public ObservableCollection<LogDatas> p_LogDatas
            {
                get
                {
                    return m_LogDatas;
                }
                set
                {
                    SetProperty(ref m_LogDatas, value);
                }
            }

            bool m_bScrolltoBottm = true;

            public bool p_bScrolltoBottm
            {
                get
                {
                    return m_bScrolltoBottm;
                }
                set
                {
                    SetProperty(ref m_bScrolltoBottm, value);
                }
            }

            bool m_IsLogsChangedPropertyInViewModel = false;
            public bool IsLogsChangedPropertyInViewModel
            {
                get
                {
                    return m_IsLogsChangedPropertyInViewModel;
                }
                set
                {
                    SetProperty(ref m_IsLogsChangedPropertyInViewModel, value);
                }
            }
            

            public LogGroup(LogViewX.eLogType func, string sGroup, NLog.LogLevel lvMin, NLog.LogLevel lvMax)
            {  
                m_sGroupName = sGroup;
                m_lvMin = lvMin;
                m_lvMax = lvMax;
                m_memory = new NLog.Targets.MemoryTarget(sGroup);
                m_memory.Layout = GetLayout();
                m_file = new NLog.Targets.FileTarget(sGroup);
                m_file.Layout = GetLayout();
                if (func == LogViewX.eLogType.ENG)
                {
                    m_file.FileName = m_sLogPath + "\\${shortdate}\\" + EQ.m_sModel + "\\${shortdate}_" + m_file.Name + ".csv";   // FileName 지정 날짜가 바뀌면 자동으로 변경
                }
                else
                {
                    m_file.FileName = m_sLogPath + "\\${shortdate}\\MARS\\${shortdate}_" + m_file.Name + ".csv";   // FileName 지정 날짜가 바뀌면 자동으로 변경
                }
                //InitDataColume(m_dataTable);
            }

            //~LogGroup()
            //{
            //    if (m_dataTable != null)
            //        m_dataTable.Dispose();
            //}

            public void AddRule(NLog.Config.LoggingConfiguration config)
            {
                //if(config.LoggingRules.Contains())
                config.AddRule(m_lvMin, m_lvMax, m_file);
                config.AddRule(m_lvMin, m_lvMax, m_memory);
            }


            #region DataTable
            // Colume 설정
            void InitDataColume(DataTable dataTable)
            {
                for (int n = 0; n < Enum.GetNames(typeof(eLayOut)).Length; n++)
                {
                    DataColumn colume = new DataColumn(Enum.GetName(typeof(eLayOut),n));
                    dataTable.Columns.Add(colume);
                }
              
            }

            object m_lockGroup = new object();

            // UI Timer -> Memory Target에 있는 string을 DataTable로 변환
            public void CalcDataTable()
            {
                if (m_memory.Logs.Count <= 0)
                    return;
                lock (m_lockGroup)
                {
                    while (m_memory.Logs.Count > 0)
                    {
                        string sLog = m_memory.Logs[0];
                        //string[] sLogs = sLog.Split('\t');
                        //DataRow dataRow = m_dataTable.NewRow();
                        ////if (sLogs.Length <= Enum.GetNames(typeof(eLayOut)).Length)
                        ////{
                        //for (int n = 0; n < sLogs.Length - 1; n++)
                        //{
                        //    if (n == sLogs.Length - 2)
                        //    {
                        //        Image img = new Image();
                        //        //var icon = new PackIcon
                        //        //{
                        //        //    Kind = PackIconKind.SmileyHappy
                        //        //};
                        //        dataRow[n] = img;
                        //    }
                        //    else
                        //    dataRow[n] = sLogs[n];
                        //}
                        //m_dataTable.Rows.InsertAt(dataRow, m_dataTable.Rows.Count);  // DataTable에 추가
                        
                        LogDatas data = new LogDatas(sLog);
                        
                        p_LogDatas.Add(data);
                        m_memory.Logs.RemoveAt(0);              // Memory Target에서 삭제
                        
                        if(p_bScrolltoBottm)
                        {
                            DataGridScrollToBottom();
                        }
                    }
                    while (p_LogDatas.Count > c_lLog)
                        p_LogDatas.RemoveAt(0); // DataTable 데이터 개수가 c_lLog를 넘지 않게
                }
            }

            public void DataGridScrollToBottom()
            {
                     IsLogsChangedPropertyInViewModel = true;
                        IsLogsChangedPropertyInViewModel = false;
            }
            #endregion
        }
        #endregion
    }
}
