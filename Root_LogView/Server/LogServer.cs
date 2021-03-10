using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_LogView.Server
{
    public class LogServer
    {
        public delegate void dgOnChangeTab();
        public event dgOnChangeTab OnChangeTab;

        #region LogData
        public class LogData
        {
            DateTime m_dt = DateTime.Now;
            public string m_sLog;
            public LogData(string sLog)
            {
                m_sLog = sLog;
            }

            public string m_sDate;
            public string p_sTime { get; set; }
            public string p_sLevel { get; set; }
            public string p_sLogger { get; set; }
            public string p_sMessage { get; set; }
            public string p_sStackTrace { get; set; }
            public Brush p_sColor { get; set; }

            public void CalcLog()
            {
                string[] asLog = m_sLog.Split('\t');
                DateTime dt = DateTime.Now;
                m_sDate = dt.ToShortDateString();
                p_sTime = dt.Hour.ToString("00") + '.' + dt.Minute.ToString("00") + '.' + dt.Second.ToString("00") + '.' + dt.Millisecond.ToString("000");
                if (asLog.Length > 1) 
                {
                    p_sLevel = asLog[1];
                    switch (asLog[1])
                    {
                        case "Fatal": p_sColor = Brushes.Yellow; break;
                        case "Error": p_sColor = Brushes.Yellow; break;
                        case "Warn": p_sColor = Brushes.GreenYellow; break;
                        default: p_sColor = Brushes.White; break;
                    }
                }
                else
                {
                    p_sLevel = "Error"; 
                    p_sColor = Brushes.Red;
                }
                if (asLog.Length > 2) p_sLogger = asLog[2];
                if (asLog.Length > 3) p_sMessage = asLog[3];
                if (asLog.Length > 4) p_sStackTrace = asLog[4];
                m_sLog = asLog[0];
                for (int n = 1; n < asLog.Length; n++) m_sLog += '\t' + asLog[n]; 
            }
        }
        Queue<LogData> m_qLogData = new Queue<LogData>();
        #endregion

        #region TCPServer
        public TCPIPServer m_server; 
        void InitServer()
        {
            m_server = new TCPIPServer("TCPServer", null);
            m_server.p_nPort = 7065;
            m_server.EventReciveData += M_server_EventReciveData;
        }

        private void M_server_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            socket.Send(aBuf);
            LogData logData = new LogData(Encoding.ASCII.GetString(aBuf, 0, nSize));
            m_qLogData.Enqueue(logData);
        }

        void RunTreeTCPIP(Tree tree)
        {
            m_server.RunTree(tree.GetTree(m_server.p_id)); 
        }
        #endregion

        #region LogGroup
        public class LogGroup
        {
            public string p_id { get; set; }
            public LogGroup(string id)
            {
                p_aLog = new ObservableCollection<LogData>();
                p_id = id;
            }

            public UserControl p_ui
            {
                get
                {
                    LogGroup_UI ui = new LogGroup_UI();
                    ui.Init(this);
                    return ui;
                }
            }

            public Queue<LogData> m_qLog = new Queue<LogData>(); 
            public void AddLog(LogData logData)
            {
                m_qLog.Enqueue(logData); 
            }

            const int c_lLog = 500;
            public ObservableCollection<LogData> p_aLog { get; set; }
            public void LogSave(string sPath)
            {
                string sDate = m_qLog.Peek().m_sDate;
                sPath += "\\" + sDate;
                Directory.CreateDirectory(sPath);
                using (StreamWriter writer = new StreamWriter(sPath + "\\" + sDate + "_" + p_id + ".txt", true, Encoding.Default))
                {
                    while (m_qLog.Count > 0)
                    {
                        LogData data = m_qLog.Peek();
                        if (data.m_sDate != sDate) return;
                        m_qLog.Dequeue();
                        writer.WriteLine(data.m_sLog);
                        p_aLog.Add(data);
                        while (p_aLog.Count > c_lLog) p_aLog.RemoveAt(0);
                    }
                }
            }
        }
        public List<LogGroup> m_aGroup = new List<LogGroup>(); 
        void InitGroup()
        {
            m_aGroup.Add(new LogGroup("Total")); 
        }

        LogGroup GetGroup(string sGroup)
        {
            foreach (LogGroup group in m_aGroup)
            {
                if (group.p_id == sGroup) return group; 
            }
            LogGroup logGroup = new LogGroup(sGroup);
            m_aGroup.Add(logGroup);
            if (OnChangeTab != null) OnChangeTab();
            return logGroup; 
        }

        void AddLogGroup(LogData logData)
        {
            logData.CalcLog();
            m_aGroup[0].AddLog(logData);
            if (logData.p_sLogger != null) GetGroup(logData.p_sLogger).AddLog(logData); 
        }

        string m_sPath = "c:\\Log"; 
        void RunTreeFile(Tree tree)
        {
            m_sPath = tree.Set(m_sPath, m_sPath, "Path", "Log File Path");
            Directory.CreateDirectory(m_sPath); 
        }
        #endregion

        #region Thread Save ==> Timer & Display
        bool m_bThread = false;
        Thread m_thread;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start(); 
        }

        void RunThread()
        {
            m_bThread = true;
            Thread.Sleep(1000);
            while (m_bThread)
            {
                Thread.Sleep(100);
                while (m_qLogData.Count > 0) AddLogGroup(m_qLogData.Dequeue());
                foreach (LogGroup group in m_aGroup)
                {
                    while (group.m_qLog.Count > 0) group.LogSave(m_sPath);
                }
            }
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot; 
        void InitTree()
        {
            m_treeRoot = new TreeRoot(p_id, null);
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
            RunTreeFile(m_treeRoot.GetTree("File"));
        }
        #endregion

        public string p_id { get; set; }
        public LogServer()
        {
            p_id = "LogServer";
            InitTree();
            InitServer(); 
            InitGroup(); 
            InitThread(); 
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join(); 
            }
            m_server.ThreadStop(); 
        }
    }
}
