using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Comm
{
    public class TCPIPServer : ITool, IComm
    {
        public delegate void OnReciveData(byte[] aBuf, int nSize, Socket socket);
        public delegate void OnAccept(Socket socket);
        public delegate void OnConnect(Socket socket);
        public event OnReciveData EventReciveData;
        public event OnAccept EventAccept;
        public event OnConnect EventConnect;

        #region ITool
        public UserControl p_ui
        {
            get
            {
                TCPIPServer_UI ui = new TCPIPServer_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Setting
        bool _bUse = false;
        public bool p_bUse
        {
            get { return _bUse; }
            set
            {
                if (_bUse == value) return;
                _bUse = value;
                if (value) Init();
                else ThreadStop();
            }
        }

        public int p_nPort { get; set; }
        #endregion

        #region TCPSocket
        public class TCPSocket
        {
            public event OnReciveData EventReciveData;
            public event OnAccept EventAccept;

            public Socket m_socket;
            public CommLog m_commLog;

            bool m_bRun = false;
            Thread m_thread;

            Queue<string> m_qSend = new Queue<string>();
            public string Send(string sMsg)
            {
                if(sMsg == null)
                {
                    return "OK";
                }
                m_qSend.Enqueue(sMsg);
                return "OK"; 
            }

            string SendMsg(string sMsg)
            {
                if (m_socket == null) return "Socket not Defined";
                if (!m_socket.Connected) return "Socket not Connected";
                try
                {
                    byte[] aBuf = Encoding.UTF8.GetBytes(sMsg);
                    m_socket.Send(aBuf);
                    m_commLog.Add(CommLog.eType.Send, sMsg);
                    return "OK";
                }
                catch (Exception ex) { return "Send Exception : " + ex.Message; }
            }

            void RunThread()
            {
                m_bRun = true;
                Socket socket = m_socket;
                while (EQ.m_bRun && m_bRun && socket.Connected)
                {
                    Thread.Sleep(10);
                    if (m_qSend.Count > 0)
                    {
                        string sMsg = m_qSend.Peek();
                        string sSend = SendMsg(sMsg);
                        if (sSend == "OK") m_qSend.Dequeue();
                        else m_commLog.Add(CommLog.eType.Info, sSend);
                    }
                }
            }

            int m_nBufReceive = 1920 * 1080 *3 ;
            byte[] m_aReadBuff = null;
            void CallBack_Receive(IAsyncResult ar)
            {
                Socket socket = (Socket)ar.AsyncState; 
                try
                {
                    if (ar == null || !socket.Connected)
                    {
                        // 연결에 문제 있음을 확인
                        m_commLog.Add(CommLog.eType.Info, "Disconnect !!");

                        if (EventReciveData != null) EventReciveData(m_aReadBuff, 0, socket);
                        socket.Close();

                        return;
                    }

                    int nReadLength = socket.EndReceive(ar);
                    if (nReadLength > 0)
                    {
                        m_commLog.Add(CommLog.eType.Receive, (nReadLength < 1024) ? Encoding.ASCII.GetString(m_aReadBuff, 0, nReadLength) : "...");

                        socket.BeginReceive(m_aReadBuff, 0, m_aReadBuff.Length, SocketFlags.None, new AsyncCallback(CallBack_Receive), socket);
                        if (EventReciveData != null) EventReciveData(m_aReadBuff, nReadLength, socket);
                    }
                    else m_commLog.Add(CommLog.eType.Info, "CallBack_Receive Close");
                }
                catch (SocketException ex)
                {
                    // SocketException 발생
                    if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        m_commLog.Add(CommLog.eType.Info, "Disconnected from client !!");

                        if (EventReciveData != null) EventReciveData(m_aReadBuff, 0, socket);
                        socket.Close();
                    }
                    else
                    {
                        m_commLog.Add(CommLog.eType.Info, "Receive SocketException : " + ex.Message + ex.StackTrace);
                    }
                }
                catch (Exception eX)
                {
                    // Exception 발생
                    m_commLog.Add(CommLog.eType.Info, "Receive Exception : " + eX.Message);
                }
            }

            public string p_id { get; set; }
            public TCPSocket(TCPIPServer server, Socket socket, int nBufReceive = -1)
            {
                p_id = socket.RemoteEndPoint.ToString(); 
                m_socket = socket;
                m_commLog = server.m_commLog;
                m_nBufReceive = (nBufReceive > 0) ? nBufReceive : m_nBufReceive;
                if ((m_aReadBuff == null) || (m_aReadBuff.Length != m_nBufReceive)) m_aReadBuff = new byte[m_nBufReceive];
                socket.BeginReceive(m_aReadBuff, 0, m_aReadBuff.Length, SocketFlags.None, new AsyncCallback(CallBack_Receive), socket);
                m_thread = new Thread(new ThreadStart(RunThread));
                m_thread.Start();
            }

            public void ThreadStop()
            {
                if (m_bRun)
                {
                    m_bRun = false;
                    m_socket.Close(); 
                    m_thread.Join();
                }
            }
        }
        public TCPSocket m_tcpSocket = null;
        #endregion

        #region Accept
        void CallBack_Accept(IAsyncResult ar)
        {
            if (m_bRun == false) return;
            if (m_socket == null) return; 
            try
            {
                if (m_tcpSocket != null) m_tcpSocket.ThreadStop(); 
                m_socket = (Socket)ar.AsyncState;
                Thread.Sleep(5); 
                if (m_socket == null) return; 
                Socket socket = m_socket.EndAccept(ar);
                m_socket.BeginAccept(new AsyncCallback(CallBack_Accept), m_socket);
                TCPSocket tcpSocket = new TCPSocket(this, socket);
                m_tcpSocket = tcpSocket;
                m_tcpSocket.EventReciveData += M_tcpSocket_EventReciveData;
                m_commLog.Add(CommLog.eType.Info, tcpSocket.p_id + " is Connect !!");

                if (EventAccept != null) EventAccept(socket);
            }
            catch (SocketException eX)
            {
                m_commLog.Add(CommLog.eType.Info, "CallBack_Accept : " + eX.Message);
            }
        }

        private void M_tcpSocket_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            if (EventReciveData != null)
            {  
                EventReciveData(aBuf, nSize, socket);
            }
            
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTree(m_treeRoot);
        }

        public void RunTree(Tree treeRoot)
        {
            RunSetTree(treeRoot.GetTree("Set"));
        }

        void RunSetTree(Tree tree)
        {
            bool bUse = tree.Set(p_bUse, false, "Use", "Use Server");
            p_nPort = tree.Set(p_nPort, p_nPort, "Port", "Port Number"); 
            p_bUse = bUse; 
        }
        #endregion

        public string p_id { get; set; }
        Log m_log;
        int m_nBufReceive = 1920 * 1080 * 3;
        public TreeRoot m_treeRoot;
        public Socket m_socket = null;
        public CommLog m_commLog;

        public TCPIPServer(string id, Log log, int nBufReceive = -1)
        {
            p_nPort = 5000; 
            p_id = id;
            m_log = log;
            m_commLog = new CommLog(this, log); 
            m_nBufReceive = (nBufReceive > 0) ? nBufReceive : m_nBufReceive;

            m_treeRoot = new TreeRoot(id, log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
        }

        public void Init()
        {
            if (!p_bUse) return;
            m_bRun = true;
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            try
            {
                m_socket.Bind(new IPEndPoint(IPAddress.Any, p_nPort));
                m_socket.Listen(5);
                m_socket.ReceiveTimeout = 1;
                m_socket.BeginAccept(new AsyncCallback(CallBack_Accept), m_socket);
            }
            catch (Exception ex)
            {
                m_log?.Error(p_id + " Server Bind & Listen Fail !!");
                m_log?.Error(p_id + " Exception : " + ex.Message);
                return;
            }
        }

        public bool m_bRun = true; 
        public void ThreadStop()
        {
            m_bRun = false; 
            if (m_tcpSocket != null) m_tcpSocket.ThreadStop();
            if (m_socket != null)
            {
                m_socket.Close();
                m_socket.Dispose();
                m_socket = null;
            }
        }

        public string Send(string sMsg) 
        {
            if (m_tcpSocket == null) return "Not Connected"; 
            return m_tcpSocket.Send(sMsg); 
        }

        public bool IsConnected()
        {
            if (m_tcpSocket != null)
                if(m_tcpSocket.m_socket != null)
                    return m_tcpSocket.m_socket.Connected;

            return false;
        }
    }
}
