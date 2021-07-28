using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Threading;

namespace RootTools
{
    // Main Class
    // LogView m_logView = new LogView();
    // m_logView.Init("LogView", m_sModel);
    // ....
    // m_logView.ThreadStop();

    // 사용 Class
    // Log m_log = m_logView.GetLog(m_id, sGroup);
    // m_log.Info("Log Here");  

    public static class LogView
    {
        public static _LogView _logView = new _LogView();

        #region Get Log
        public static Log GetLog(string id)
        {
            return _logView.GetLog(id, null); 
        }

        public static Log GetLog(string id, string sGroup)
        {
            return _logView.GetLog(id, sGroup); 
        }
        #endregion

        #region TCPIP
        static object g_lock = new object();
        public static void Send(Log.Data data)
        {
            lock (g_lock)
            {
                if (_logView.m_client.p_bConnect == false) return;
                _logView.m_client.Send(data.p_sLog);
            }
        }
        #endregion

        public static void Init()
        {
            EQ.m_bRun = true;
            _logView.Init(); 
        }

        public static void ThreadStop()
        {
            EQ.m_bRun = false;
            _logView.ThreadStop(); 
        }
    }

    public class _LogView : NotifyProperty
    {
        #region UI
        public delegate void dgOnChangeTab();
        public event dgOnChangeTab OnChangeTab;

        public string p_sPath { get; set; }

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
        public List<LogGroup> m_aGroup = new List<LogGroup>();
        public Log GetLog(string id, string sGroup)
        {
            if ((sGroup == null) || (sGroup == "")) sGroup = id;
            LogGroup group = GetLogGroup(sGroup); 
            return new Log(id, m_groupTotal, group);
        }

        LogGroup GetLogGroup(string sGroup)
        {
            foreach (LogGroup group in m_aGroup)
            {
                if (group.p_id == sGroup) return group; 
            }
            LogGroup logGroup = new LogGroup(sGroup, Log.eLevel.Info);
            m_aGroup.Add(logGroup); 
            if (OnChangeTab != null) OnChangeTab();
            return logGroup; 
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void StartTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (p_bHold) return;
            foreach (LogGroup group in m_aGroup) group.TimerSave();
        }
        #endregion

        #region TCP Client
        public TCPAsyncClient m_client = null; 
        void InitClient()
        {
            m_client = new TCPAsyncClient(EQ.m_sModel + ".Log", null);
            m_client.p_nPort = 7065;
            m_client.EventReceiveData += M_client_EventReciveData;
        }

        private void M_client_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
        }

        void RunTreeTCPIP(Tree tree)
        {
            m_client.RunTree(tree); 
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot("LogView", null);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
            RunTree(Tree.eMode.Init);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeTCPIP(m_treeRoot.GetTree("TCPIP"));
        }
        #endregion

        public _LogView()
        {
            p_sPath = "c:\\Log"; 
        }

        LogGroup m_groupTotal;
        public void Init()
        {
            m_groupTotal = new LogGroup("Total", Log.eLevel.Info);
            m_aGroup.Add(m_groupTotal);
            InitTree();
            InitClient(); 
            StartTimer();
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
        }

        public void ThreadStop()
        {
            m_client.ThreadStop(); 
            m_timer.Stop(); 
        }
    }
}
