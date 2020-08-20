using NLog.Targets;
using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Threading;

namespace Root_MarsLogView
{
    public class MarsLogViewer
    {
        #region TCPIP
        public TCPIPServer[] m_tcpServer = new TCPIPServer[2]; 
        void InitTCP(int n)
        {
            m_tcpServer[n] = new TCPIPServer("TCP" + n.ToString(), null);
            m_tcpServer[n].EventReciveData += MarsLogViewer_EventReciveData;
        }

        Queue<string> m_qLog = new Queue<string>();
        private void MarsLogViewer_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            socket.Send(aBuf);
            m_qLog.Enqueue(Encoding.ASCII.GetString(aBuf, 0, nSize));
        }
        #endregion

        #region Error
        class Error
        {
            public string m_sError;
            public string m_sLog; 
        }

        Queue<Error> m_qError = new Queue<Error>(); 
        public void AddError(string sError, string sLog)
        {
            Error error = new Error();
            error.m_sError = sError;
            error.m_sLog = sLog;
            m_qError.Enqueue(error); 
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            while (m_qLog.Count > 0)
            {
                string sLog = m_qLog.Dequeue();
                string[] asLog = sLog.Split('\t');
                if (asLog.Length < 4) AddError("Log Length", sLog);  
                else
                {
                    switch (asLog[3])
                    {
                        case "PRC": m_listPRC.Add(sLog, asLog); break;
                        case "XFR": m_listXFR.Add(sLog, asLog); break;
                        case "FNC": m_listFNC.Add(sLog, asLog); break;
                        case "LEH": m_listLEH.Add(sLog, asLog); break;
                        case "CFG": m_listCFG.Add(sLog, asLog); break;
                    }
                }
            }
        }
        #endregion

        #region Write Event
        public void WriteEvent(string sLog)
        {
            if (sLog[sLog.Length - 1] == '$') sLog = sLog.Substring(0, sLog.Length - 1);
            string sFile = GetFileName(sLog);
            bool bFirst = File.Exists(sFile); 
            StreamWriter sw = new StreamWriter(new FileStream(sFile, FileMode.Append));
            if (bFirst) sw.WriteLine("Applied Samsung Standard Logging Spec Version: 2.0");
            sw.WriteLine(sLog);
            sw.Close(); 
        }

        string m_sFilePath = "c:";
        string GetFileName(string sLog)
        {
            string sTime = sLog.Substring(0, 4) + sLog.Substring(5, 2) + sLog.Substring(8, 2) + sLog.Substring(11, 2); 
            return m_sFilePath + "\\Logs\\EventLog\\EventLog" + sTime + ".txt"; 
        }

        void RunTreeFile(Tree tree)
        {
            m_sFilePath = tree.Set(m_sFilePath, m_sFilePath, "Path", "Mars Log Save File Path");
            Directory.CreateDirectory(m_sFilePath + "\\Logs");
            Directory.CreateDirectory(m_sFilePath + "\\Logs\\EventLog");
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot; 
        void InitTreeRoot()
        {
            m_treeRoot = new TreeRoot("MarsLog", null); 
            RunTree(Tree.eMode.RegRead);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeFile(m_treeRoot.GetTree("File")); 
            m_listPRC.RunTree(m_treeRoot.GetTree("PRC"));
            m_listXFR.RunTree(m_treeRoot.GetTree("XFR"));
            m_listFNC.RunTree(m_treeRoot.GetTree("FNC"));
            m_listLEH.RunTree(m_treeRoot.GetTree("LEH"));
            m_listCFG.RunTree(m_treeRoot.GetTree("CFG"));
        }
        #endregion

        public ListPRC m_listPRC;
        public ListXFR m_listXFR;
        public ListFNC m_listFNC;
        public ListLEH m_listLEH;
        public ListCFG m_listCFG;
        public MarsLogViewer()
        {
            m_listPRC = new ListPRC(this);
            m_listXFR = new ListXFR(this);
            m_listFNC = new ListFNC(this);
            m_listLEH = new ListLEH(this);
            m_listCFG = new ListCFG(this);
            InitTCP(0);
            InitTCP(1);
            InitTreeRoot(); 

            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        public void ThreadStop()
        {
            m_tcpServer[0].ThreadStop();
            m_tcpServer[1].ThreadStop();
        }
    }
}
