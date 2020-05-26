using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls;
using System.Windows.Media;
using RootTools.Trees;
using RootTools.Comm;

namespace RootTools
{
    public class TCPIPServer : ITool, IComm
    {
        public delegate void OnReciveData(byte[] aBuf, int nSize, Socket socket);

        #region ITool
        public UserControl p_ui
        {
            get
            {
                TCPIPServer_UI ui = new TCPIPServer_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        #region Setting
        bool m_bUse = false;
        public bool p_bUse
        {
            get
            {
                return m_bUse;
            }
            set
            {
                if (m_bUse == false && value == true)
                {
                    m_bUse = value;
                    Init();
                }
                else if (m_bUse == true && value == false)
                {
                    m_bUse = value;
                    ThreadStop();
                }
            }
        }

        int m_nPort = 0;
        public int p_nPort
        {
            get
            {
                return m_nPort;
            }
            set
            {
                m_nPort = value;
            }
        }
        #endregion

        #region TCPSocket
        public class TCPSocket : IComm
        {
            public event OnReciveData EventReciveData;

            public Socket m_socket;
            public CommLog m_commLog;

            bool m_bRun = false;
            Thread m_thread;

            public string Send(string sMsg)
            {
                string sSend = Send(sMsg);
                m_commLog.Add(CommLog.eType.Send, sMsg);
                if (sSend != "OK") m_commLog.Add(CommLog.eType.Info, sSend);
                return sSend;
            }

            string SendMsg(string sMsg)
            {
                if (m_socket == null) return "Socket not Defined";
                if (!m_socket.Connected) return "Socket not Connected";
                try
                {
                    byte[] aBuf = Encoding.UTF8.GetBytes(sMsg);
                    m_socket.Send(aBuf);
                    return "OK";
                }
                catch (Exception ex) { return "Send Exception : " + ex.Message; }
            }

            void RunThread()
            {
                m_bRun = true;
                Socket socket = m_socket;
                int nSize;
                byte[] aBuf = new byte[m_nBufRecieve];
                while (socket.Connected)
                {
                    Thread.Sleep(10);
                    if (!m_bRun) break;
                    try
                    {
                        nSize = socket.Receive(aBuf);
                        m_commLog.Add(CommLog.eType.Receive, (nSize < 1024) ? Encoding.ASCII.GetString(aBuf, 0, nSize) : "...");
                        if (EventReciveData != null) EventReciveData(aBuf, nSize, socket);
                    }
                    catch (Exception ex) { m_commLog.Add(CommLog.eType.Info, "Recieve Exception : " + ex.Message); }
                }
            }

            public string p_id { get; set; }
            int m_nBufRecieve = 1024 * 1024;
            public TCPSocket(Socket socket, Log log, int nBufRecieve = -1)
            {
                p_id = socket.RemoteEndPoint.ToString(); 
                m_socket = socket;
                m_commLog = new CommLog(this, log);
                m_nBufRecieve = (nBufRecieve > 0) ? nBufRecieve : m_nBufRecieve;
                m_thread = new Thread(new ThreadStart(RunThread));
                m_thread.Start();
            }

            public void ThreadStop()
            {
                if (m_bRun)
                {
                    m_bRun = false;
                    m_thread.Join();
                }
            }
        }
        public List<TCPSocket> m_aSocket = new List<TCPSocket>();
        #endregion

        public string p_id { get; set; }
        Log m_log;
        int m_nBufRecieve = 1024 * 1024;
        public CommLog m_commLog; 
        public TreeRoot m_treeRoot;
        Socket m_socket = null;
        Thread m_threadListener;
        bool m_bRunListener = false;

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

        void RunTree(Tree treeRoot)
        {
            RunSetTree(treeRoot.GetTree("Set"));
        }

        void RunSetTree(Tree tree)
        {
            p_bUse = tree.Set(m_bUse, false, "Use", "Use Server");
            p_nPort = tree.Set(m_nPort, 0, "Port", "Port Number");
        }
        #endregion

        public TCPIPServer(string id, Log log, int nBufRecieve = -1)
        {
            p_id = id;
            m_log = log;
            m_nBufRecieve = (nBufRecieve > 0) ? nBufRecieve : m_nBufRecieve;
            m_commLog = new CommLog(this, log); 

            m_treeRoot = new TreeRoot(id, log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

            Init();
        }

        public void Init()
        {
            if (!m_bUse) return;
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            try
            {
                m_socket.Bind(new IPEndPoint(IPAddress.Any, m_nPort));
                m_socket.Listen(5);
                m_threadListener = new Thread(new ThreadStart(RunThreadListner));
                m_threadListener.Start();
            }
            catch (Exception ex)
            {
                m_log.Error(p_id + " Server Bind & Listen Fail !!");
                m_log.Error(p_id + " Exception : " + ex.Message);
                return;
            }
        }

        public void ThreadStop()
        {
            foreach (TCPSocket tcpSocket in m_aSocket) tcpSocket.ThreadStop();
            if (m_bRunListener)
            {
                m_bRunListener = false;
                m_threadListener.Abort();
            }
            m_aSocket.Clear();
            if (m_socket != null)
            {
                m_socket.Close();
                m_socket.Dispose();
            }
        }

        void RunThreadListner()
        {
            m_bRunListener = true;
            Thread.Sleep(5000);
            while (m_bRunListener)
            {
                Thread.Sleep(10);
                try
                {
                    Socket socketHandle = m_socket.Accept();
                    TCPSocket tcpSocket = new TCPSocket(socketHandle, m_log);
                    m_aSocket.Add(tcpSocket);
                    m_commLog.Add(CommLog.eType.Info, p_id + " Client Connected, Client IP : " + socketHandle.LocalEndPoint.ToString()); 
                }
                catch (Exception ex) { m_commLog.Add(CommLog.eType.Info, "Listner Exception : " + ex.Message); }
            }
        }

        public string Send(string sMsg)
        {
            return "OK"; 
        }
    }
}
