using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;

namespace RootTools.Comm
{
    public class CommLog
    {
        const int c_lLog = 256; 

        public enum eType
        {
            Send,
            Receive,
            Info,
        }

        #region class Log
        public class LogData
        {
            string _sTime = "";
            public string p_sTime
            {
                get { return _sTime; }
            }

            string _sMsg = "";
            public string p_sMsg
            {
                get { return _sMsg; }
            }

            Brush _bColor = Brushes.Black;
            public Brush p_bColor
            {
                get { return _bColor; }
            }

            public LogData(eType type, string sMsg)
            {
                _sTime = DateTime.Now.ToLongTimeString();
                _sMsg = sMsg;
                switch (type)
                {
                    case eType.Send: _bColor = Brushes.Black; break;
                    case eType.Receive: _bColor = Brushes.DarkGreen; break;
                    default: _bColor = Brushes.Red; break;
                }
            }
        }
        #endregion

        #region List Log
        public ObservableCollection<LogData> m_aLog = new ObservableCollection<LogData>();
        public Queue<LogData> m_qLog = new Queue<LogData>(); 

        public void Add(eType type, string sMsg)
        {
            m_qLog.Enqueue(new LogData(type, sMsg));
            if (m_log != null)
            {
                string sHead = "";
                switch (type)
                {
                    case eType.Send: sHead = " ==> "; break;
                    case eType.Receive: sHead = "<==  "; break;
                    default: sHead = "Info : "; break;
                }
                m_log.Info(sHead + sMsg);
            }
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (m_qLog.Count == 0) return;
            int l = m_qLog.Count; 
            for (int n = 0; n < l; n++) m_aLog.Add(m_qLog.Dequeue());
            while (m_aLog.Count > c_lLog) m_aLog.RemoveAt(0); 
        }
        #endregion

        #region IComm
        public IComm m_comm; 
        public void Send(string sMsg)
        {
            m_comm.Send(sMsg); 
        }
        #endregion

        Log m_log; 
        public CommLog(IComm comm, Log log)
        {
            m_comm = comm; 
            m_log = log;
            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }
    }
}
