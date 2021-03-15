using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace Root_MarsLogView
{
    public class MarsLogViewer
    {
        #region TCPIP
        public TCPIPServer[] m_tcpServer = new TCPIPServer[2]; 
        void InitTCP()
        {
            m_tcpServer[0] = new TCPIPServer("TCPIP EFEM", null);
            m_tcpServer[0].EventReciveData += MarsLogViewer0_EventReciveData;
            m_tcpServer[1] = new TCPIPServer("TCPIP Vision", null);
            m_tcpServer[1].EventReciveData += MarsLogViewer1_EventReciveData;
        }

        public class Mars
        {
            public int m_iTCP;
            public string m_sLog; 

            public Mars(int iTCP, string sLog)
            {
                m_iTCP = iTCP;
                m_sLog = sLog; 
            }
        }

        Queue<Mars> m_qLog = new Queue<Mars>();
        private void MarsLogViewer0_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            socket.Send(aBuf, nSize, SocketFlags.None);
            m_qLog.Enqueue(new Mars(0, Encoding.ASCII.GetString(aBuf, 0, nSize)));
        }

        private void MarsLogViewer1_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            socket.Send(aBuf, nSize, SocketFlags.None);
            m_qLog.Enqueue(new Mars(1, GetVisionString(Encoding.ASCII.GetString(aBuf, 0, nSize))));
        }

        string GetVisionString(string sVision)
        {
            string[] asMars = new string[14] { "", "", "", "'PRC'", "", "", "", "'Wafer'", "", "", "", "$", "0", "$" };
            string[] asVision = sVision.Split(',');
            GetDateTime(asMars); 
            foreach (string sCmd in asVision)
            {
                string[] asCmd = sCmd.Split(':');
                if (asCmd.Length >= 2)
                {
                    switch (asCmd[0])
                    {
                        case "ModuleID": asMars[2] = '\'' + asCmd[1] + '\''; break;
                        case "LogType": asMars[3] = '\'' + asCmd[1] + '\''; break;
                        case "EventID": asMars[4] = '\'' + asCmd[1] + '\''; break;
                        case "Status": asMars[5] = '\'' + asCmd[1] + '\''; break;
                        case "WaferID": asMars[6] = '\'' + asCmd[1] + '\''; break;
                        case "SlotNo": asMars[8] = "1"; break;
                        case "LotID": asMars[9] = '\'' + asCmd[1] + '\''; break;
                        case "RecipeName": asMars[10] = '\'' + asCmd[1] + '\''; break;
                        case "StepNumber": asMars[11] = asCmd[1]; break;
                        case "StepSeq": asMars[12] = asCmd[1]; break;
                        case "StepName": asMars[13] = '\'' + asCmd[1] + '\''; break;
                    }
                }
            }
            string sLog = asMars[0];
            for (int n = 1; n < 14; n++) sLog += '\t' + asMars[n]; 
            return sLog; 
        }

        void GetDateTime(string[] asMars)
        {
            DateTime dtNow = DateTime.Now;
            asMars[0] = string.Format("{0:0000}/{1:00}/{2:00}", dtNow.Year, dtNow.Month, dtNow.Day);
            asMars[1] = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", dtNow.Hour, dtNow.Minute, dtNow.Second, dtNow.Millisecond);
        }

        #endregion

        #region Error
        public void AddError(string sError, string sLog)
        {
            m_listError.Add(sError, sLog); 
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            while (m_qLog.Count > 0)
            {
                Mars mars = m_qLog.Dequeue();
                string sLog = mars.m_sLog;
                string[] asLog = sLog.Split('\t');
                if (asLog.Length < 4) AddError("Log Length", sLog);  
                else
                {
                    switch (asLog[3])
                    {
                        case "'PRC'": m_listPRC.Add(mars.m_iTCP, sLog, asLog); break;
                        case "'XFR'": m_listXFR.Add(mars.m_iTCP, sLog, asLog); break;
                        case "'FNC'": m_listFNC.Add(mars.m_iTCP, sLog, asLog); break;
                        case "'LEH'": m_listLEH.Add(mars.m_iTCP, sLog, asLog); break;
                        case "'CFG'": m_listCFG.Add(sLog, asLog); break;
                        case "'Reset'":
                        case "Reset": Reset(mars.m_iTCP); break; 
                    }
                }
            }
        }

        void Reset(int iTCP)
        {
            DateTime dt = DateTime.Now;
            string sDate = dt.Year.ToString("0000/") + dt.Month.ToString("00/") + dt.Day.ToString("00");
            string sTime = dt.Hour.ToString("00:") + dt.Minute.ToString("00:") + dt.Second.ToString("00") + "." + dt.Millisecond.ToString("000");
            m_listFNC.Reset(iTCP, sDate, sTime);
            m_listPRC.Reset(iTCP, sDate, sTime);
            m_listXFR.Reset(iTCP, sDate, sTime);
            m_listLEH.Reset(iTCP, sDate, sTime);
        }
        #endregion

        #region Write Event
        public void WriteEvent(string sLog)
        {
            if (sLog[sLog.Length - 1] == '$') sLog = sLog.Substring(0, sLog.Length - 1);
            string sFile = GetFileName(sLog);
            bool bExist = File.Exists(sFile); 
            StreamWriter sw = new StreamWriter(new FileStream(sFile, FileMode.Append));
            if (bExist == false) sw.WriteLine("Applied Samsung Standard Logging Spec Version: 2.0");
            sw.WriteLine(sLog);
            sw.Close(); 
        }

        public string m_sFilePath = "c:";
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
            m_listError.RunTree(m_treeRoot.GetTree("Error"));
        }
        #endregion

        public ListPRC m_listPRC;
        public ListXFR m_listXFR;
        public ListFNC m_listFNC;
        public ListLEH m_listLEH;
        public ListCFG m_listCFG;
        public ListError m_listError; 
        public MarsLogViewer()
        {
            m_listPRC = new ListPRC(this);
            m_listXFR = new ListXFR(this);
            m_listFNC = new ListFNC(this);
            m_listLEH = new ListLEH(this);
            m_listCFG = new ListCFG(this);
            m_listError = new ListError(this); 
            InitTCP();
            InitTreeRoot(); 

            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        public void ThreadStop()
        {
            DateTime dt = DateTime.Now;
            string sDate = dt.Year.ToString("0000/") + dt.Month.ToString("00/") + dt.Day.ToString("00");
            string sTime = dt.Hour.ToString("00:") + dt.Minute.ToString("00:") + dt.Second.ToString("00") + "." + dt.Millisecond.ToString("000");
            m_listPRC.ThreadStop(sDate, sTime);
            m_listXFR.ThreadStop(sDate, sTime);
            m_listFNC.ThreadStop(sDate, sTime);
            m_listLEH.ThreadStop(sDate, sTime);
            m_tcpServer[0].ThreadStop();
            m_tcpServer[1].ThreadStop();
        }
    }
}
