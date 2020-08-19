using RootTools.Comm;
using System;
using System.Collections.Generic;
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
                    }
                }
            }
        }
        #endregion

        #region Write Log
        public void WriteLog(string sLog, string[] asLog)
        {
            //forget
        }
        #endregion

        public ListPRC m_listPRC;
        public MarsLogViewer()
        {
            m_listPRC = new ListPRC(this); 
            InitTCP(0);
            InitTCP(1);

            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick; ;
            m_timer.Start();
        }

        public void ThreadStop()
        {
            m_tcpServer[0].ThreadStop();
            m_tcpServer[1].ThreadStop();
        }
    }
}
