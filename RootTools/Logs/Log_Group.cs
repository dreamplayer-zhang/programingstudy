using NLog;
using NLog.Config;
using NLog.Targets;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace RootTools
{
    public class Log_Group : ILogGroup
    {
        const int c_lLog = 500; 

        #region ILog
        public string p_id { get; set; }

        public UserControl p_ui
        {
            get
            {
                Log_Group_UI ui = new Log_Group_UI();
                ui.Init(this);
                return ui;
            }
        }

        public void CalcData()
        {
            if (m_memory.Logs.Count <= 0) return;
            while (m_memory.Logs.Count > 0)
            {
                if (m_memory.Logs[0] == null) m_memory.Logs.RemoveAt(0);
                else
                {
                    Data data = new Data(m_memory.Logs[0]);
                    m_memory.Logs.RemoveAt(0);
                    p_aLog.Add(data);
                }
            }
            while (p_aLog.Count > c_lLog) p_aLog.RemoveAt(0);
        }
        #endregion

        #region Layout
        string m_sLayoutMemory = "${time}\t${level}\t${logger}\t${message}\t${stacktrace:topFrames=4:skipFrames=1}";
        string m_sLayoutFile = "${longdate}\t${level}\t${logger}\t${message}\t${stacktrace:topFrames=4:skipFrames=1}";
        #endregion

        #region Data
        public ObservableCollection<Data> p_aLog { get; set; }
        public class Data
        {
            public string p_sTime { get; set; }
            public string p_sLevel { get; set; }
            public string p_sLogger { get; set; }
            public string p_sMessage { get; set; }
            public string p_sStackTrace { get; set; }
            public Brush p_sColor { get; set; } 

            public Data(string sLog)
            {
                string[] asLog = sLog.Split('\t');
                if (asLog.Length > 0) p_sTime = asLog[0];
                if (asLog.Length > 1) p_sLevel = asLog[1];
                if (asLog.Length > 2) p_sLogger = asLog[2];
                if (asLog.Length > 3) p_sMessage = asLog[3];
                if (asLog.Length > 4) p_sStackTrace = asLog[4];
                switch (p_sLevel)
                {
                    case "Fatal":
                    case "Error": p_sColor = Brushes.Yellow; break;
                    case "Warn": p_sColor = Brushes.GreenYellow; break;
                    default: p_sColor = Brushes.White; break;
                }
            }
        }
        #endregion

        public void AddRule(LoggingConfiguration config)
        {
            config.AddRule(m_lvMin, m_lvMax, m_file);
            config.AddRule(m_lvMin, m_lvMax, m_memory);
        }

        public MemoryTarget m_memory;  // DataGrid에 Log를 추가하기 위해 임시로 저장 되는 Memory Target
        public FileTarget m_file;      // File로 Log를 남기기 위한 Target
        LogLevel m_lvMin;              // 최소 레벨
        LogLevel m_lvMax;              // 최대 레벨
        public Log_Group(string sGroup, LogLevel lvMin, LogLevel lvMax)
        {
            p_aLog = new ObservableCollection<Data>();
            p_id = sGroup;
            m_lvMin = lvMin;
            m_lvMax = lvMax;
            m_memory = new MemoryTarget(sGroup);
            m_memory.Layout = m_sLayoutMemory; 
            m_file = new FileTarget(sGroup);
            m_file.Layout = m_sLayoutFile; 
            m_file.FileName = "c:\\Log\\${shortdate}\\" + EQ.m_sModel + "\\${shortdate}_" + m_file.Name + ".log"; // FileName 지정 날짜가 바뀌면 자동으로 변경
        }
    }
}
