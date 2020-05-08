using System.Collections.Generic;
using System.Windows.Controls;

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

    public class LogViewer : NotifyProperty
    {
        #region UI
        public delegate void dgOnChangeTab();
        public event dgOnChangeTab OnChangeTab;
        #endregion

        #region List ILogSet
        public List<ILog> m_aLogSet = new List<ILog>();
        #endregion

        #region Get Log
/*        public Log GetLog(string sLogSet = "ATI", string sGroup = "")
        {
            var config = new NLog.Config.LoggingConfiguration();    // 현재 설정 값
            var factory = new NLog.LogFactory();                    // 중간 사용 변수
            ILogSet logSet = GetLogSet(sLogSet); 
        }*/
        #endregion

        public void Init()
        {
//            InitDefaultGroup();         // Info (전체 Log), Error Log 생성
//            StartTimer();
        }

        public void ThreadStop()
        {
            NLog.LogManager.Shutdown();
        }
    }
}
