using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace RootTools.Logs
{
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

    public static class LogViewer
    {
        public static _LogView m_logView = new _LogView();

        #region Get Log
        public static Log GetLog(string id)
        {
            return m_logView.GetLog(id, null, null); 
        }

        public static Log GetLog(string id, string sGroup)
        {
            return m_logView.GetLog(id, sGroup, null); 
        }

        public static Log GetLog(string id, string sGroup, string sSet)
        {
            return m_logView.GetLog(id, sGroup, sSet); 
        }
        #endregion

        public static void Init()
        {
            m_logView.Init(); 
        }

        public static void ThreadStop()
        {
            LogManager.Shutdown(); 
        }
    }

    public class _LogView : NotifyProperty
    {
        #region UI
        public delegate void dgOnChangeTab();
        public event dgOnChangeTab OnChangeTab;

        bool _bHold = false;
        public bool p_bHold
        {
            get { return _bHold; }
            set
            {
                _bHold = value;
                OnPropertyChanged(); 
            }
        }

        #endregion

        #region List Log
        public List<ILog> m_aLogSet = new List<ILog>();
        #endregion

        #region Get Log
        public Log GetLog(string id, string sGroup, string sSet)
        {
            var config = new LoggingConfiguration();    // 현재 설정 값
            var factory = new LogFactory();             // 중간 사용 변수
            ILog logSet = GetLogSet(sSet);
            return null;
        }
        /*        public Log GetLog(string sLogSet, string sGroup)
                {
                    InitLogGroup(config, sGroup); 
                    ILog logSet = GetLogSet(sLogSet); 
                }
                */

        ILog GetLogSet(string sSet)
        {
            if ((sSet == null) || (sSet == "")) return null;
            foreach (ILog log in m_aLogSet)
            {
                if (log.p_id == sSet) return log; 
            }
            Log_Set logSet = new Log_Set(sSet);
            m_aLogSet.Add(logSet);
            if (OnChangeTab != null) OnChangeTab();
            return logSet; 
        }
        
        void InitLogGroup(LoggingConfiguration config, string sGroup)
        {
            m_groupTotal.AddRule(config);
            if ((sGroup == null) || (sGroup == "")) return;

        }

        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void StartTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(200);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (p_bHold) return;
            foreach (ILog log in m_aLogSet) log.CalcData();
        }
        #endregion

        Log_Group m_groupTotal;
        public void Init()
        {
            m_groupTotal = new Log_Group("Total", LogLevel.Info, LogLevel.Fatal);
            m_aLogSet.Add(m_groupTotal);
            //            StartTimer();
        }

        public void ThreadStop()
        {
            LogManager.Shutdown();
        }
    }
}
