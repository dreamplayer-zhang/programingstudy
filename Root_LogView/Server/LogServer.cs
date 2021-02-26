using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Root_LogView.Server
{
    public class LogServer
    {
        #region LogData
        public class LogData
        {
            DateTime m_dt = DateTime.Now;
            public int m_nServerID;
            public string m_sLog;
            public LogData(int nServerID, string sLog)
            {
                m_nServerID = nServerID;
                m_sLog = sLog;
            }

            public string m_sGroup = null;
            public void CalcLog()
            {
                string[] asLog = m_sLog.Split('\t');
                if (asLog.Length < 3) asLog[0] = m_nServerID.ToString() + ". " + asLog[0];
                else
                {
                    asLog[0] += " ~ " + m_dt.Second.ToString() + "." + m_dt.Millisecond.ToString();
                    m_sGroup = m_nServerID.ToString() + "." + asLog[2];
                    asLog[2] = m_sGroup; 

                }
                m_sLog = asLog[0];
                for (int n = 1; n < asLog.Length; n++) m_sLog += '\t' + asLog[n]; 
            }
        }
        Queue<LogData> m_qLogData = new Queue<LogData>(); 
        #endregion

        #region TCPServer
        int _nServer = 0; 
        public int p_nServer
        {
            get { return _nServer; }
            set
            {
                if (_nServer == value) return;
                _nServer = value;
                InitTCPServer(); 
            }
        }

        public class TCPServer
        {
            LogServer m_logServer;
            int m_nID; 
            public TCPAsyncServer m_server;
            public TCPServer(LogServer logServer, int nID)
            {
                m_logServer = logServer; 
                m_nID = nID; 
                m_server = new TCPAsyncServer("TCPServer" + nID.ToString(), null);
                m_server.EventReciveData += M_server_EventReciveData;
            }

            private void M_server_EventReciveData(byte[] aBuf, int nSize, Socket socket)
            {
                socket.Send(aBuf);
                LogData logData = new LogData(m_nID, Encoding.ASCII.GetString(aBuf, 0, nSize));
                m_logServer.m_qLogData.Enqueue(logData); 
            }
        }

        List<TCPAsyncServer> m_aServer = new List<TCPAsyncServer>(); 
        void InitTCPServer()
        {
            while (m_aServer.Count > p_nServer) m_aServer.RemoveAt(m_aServer.Count - 1); 
            while (m_aServer.Count < p_nServer)
            {
                int nID = m_aServer.Count; 
                TCPAsyncServer server = new TCPAsyncServer("TCPServer" + nID.ToString(), null);
                server.p_nPort = 7060 + nID; 
                m_aServer.Add(server); 
            }
        }

        void RunTreeTCPIP(Tree tree)
        {
            p_nServer = tree.Set(p_nServer, 1, "Count", "TCPIP Server Count"); 
            foreach (TCPAsyncServer server in m_aServer) server.RunTree(tree.GetTree(server.p_id));
        }
        #endregion

        #region LogGroup
        public class LogGroup
        {
            public string m_id;
            public LogGroup(string id)
            {
                m_id = id;
            }

            Queue<LogData> m_qLogData = new Queue<LogData>(); 
            public void AddLog(LogData logData)
            {
                m_qLogData.Enqueue(logData); 
            }

            public void SaveLog(string sFile)
            {
                if (m_qLogData.Count == 0) return; 
                using (StreamWriter writer = new StreamWriter(sFile + m_id + ".txt"))
                {
                    while (m_qLogData.Count > 0)
                    {
                        LogData logData = m_qLogData.Dequeue();
                        writer.WriteLine(logData.m_sLog); 
                    }
                }
            }
        }
        List<LogGroup> m_aGroup = new List<LogGroup>(); 
        void InitGroup()
        {
            m_aGroup.Add(new LogGroup("Total")); 
        }

        LogGroup GetGroup(string sGroup)
        {
            foreach (LogGroup group in m_aGroup)
            {
                if (group.m_id == sGroup) return group; 
            }
            LogGroup logGroup = new LogGroup(sGroup);
            m_aGroup.Add(logGroup);
            return logGroup; 
        }

        void AddLogGroup(LogData logData)
        {
            logData.CalcLog();
            m_aGroup[0].AddLog(logData);
            if (logData.m_sGroup != null) GetGroup(logData.m_sGroup).AddLog(logData); 
        }

        string m_sPath = "c:\\Log"; 
        void RunTreeFile(Tree tree)
        {
            m_sPath = tree.Set(m_sPath, m_sPath, "Path", "Log File Path");
            Directory.CreateDirectory(m_sPath); 
        }
        #endregion

        #region Thread Save
        bool m_bThread = false;
        Thread m_thread;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start(); 
        }

        void RunThread()
        {
            DateTime dateTime = new DateTime(2000, 1, 1);
            string sDate = "";
            string sPath = ""; 
            m_bThread = true;
            Thread.Sleep(1000);
            while (m_bThread)
            {
                Thread.Sleep(100);
                while (m_qLogData.Count > 0) AddLogGroup(m_qLogData.Dequeue());
                if (dateTime != DateTime.Today)
                {
                    DateTime dt = DateTime.Now;
                    sDate = dt.Year.ToString() + '-' + dt.Month.ToString("00") + '-' + dt.Day.ToString("00");
                    sPath = m_sPath + "\\" + sDate;
                    Directory.CreateDirectory(sPath);
                    sPath += "\\" + sDate + '_'; 
                }
                foreach (LogGroup group in m_aGroup) group.SaveLog(sPath); 
            }
        }
        #endregion

        #region Tree
        TreeRoot m_treeRoot; 
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
            InitGroup(); 
            InitThread(); 
        }
    }
}
