using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media;
using RootTools.Trees;

namespace RootTools
{
    public class TCPIPServer : ITool
    {
        public delegate void OnReciveData(byte[] aBuf, int nSize, Socket socket);
        public event OnReciveData EventReciveData;

        #region ITool
        public string p_id { get { return m_id; } }

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

        public string m_id;
        LogWriter m_log;
        public TreeRoot m_treeRoot;
        Socket m_socket = null;
        List<Socket> m_listSocket = new List<Socket>();
        List<Thread> m_listCommThread = new List<Thread>();
        Thread m_threadListener;
        bool m_bRunThreadListener = false;
        bool m_bRun = false;

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

        public TCPIPServer(string id, LogWriter log)
        {
            m_id = id;
            m_log = log;

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
                m_log.Error(m_id + " Server Bind & Listen Fail !!");
                m_log.Error(m_id + " Exception : " + ex.Message);
                return;
            }
        }

        public void ThreadStop()
        {
            if (m_socket != null)
            {
                m_socket.Close();
                m_socket.Dispose();
            }

            if (m_bRunThreadListener)
            {
                m_bRunThreadListener = false;
                m_threadListener.Abort();
            }
            if (m_bRun)
            {
                m_bRun = false;
                foreach (Thread threadComm in m_listCommThread)
                {
                    threadComm.Abort();
                }
            }
            m_listCommThread.Clear();
            m_listSocket.Clear();
        }

        void RunThreadListner()
        {
            m_bRunThreadListener = true;
            Thread.Sleep(5000);
            while (m_bRunThreadListener)
            {
                Thread.Sleep(10);
                try
                {
                    Socket socketHandle = m_socket.Accept();
                    Thread commThread = new Thread(new ParameterizedThreadStart(RunThreadRecive));
                    commThread.Start((object)socketHandle);
                    m_log.Info(m_id + " Client Connected, Client IP : " + socketHandle.LocalEndPoint.ToString());
                    m_listSocket.Add(socketHandle);
                    m_listCommThread.Add(commThread);
                }
                catch (Exception ex)
                {
                    m_log.Error("Listner Fail : " + ex.Message);
                }
            }
        }

        void RunThreadRecive(object objSocket)
        {
            m_bRun = true;
            Socket socket = (Socket)objSocket;
            int nSize;
            byte[] aBuf = new byte[1024 * 1024];
            while (socket.Connected)
            {
                Thread.Sleep(10);
                if (!m_bRun) break;
                try
                {
                    nSize = socket.Receive(aBuf);
                    AddCommLog(false, "RECIVE : " + Encoding.Default.GetString(aBuf));
                    if (EventReciveData != null) EventReciveData(aBuf, nSize, socket);
                }
                catch (Exception ex)
                {
                    m_log.Error("Recive Fail : " + ex.Message);
                    m_listSocket.Remove(socket);
                }
            }
        }

        public string Send(byte[] byteBuf, Socket socket)
        {
            if (socket == null) return "Null";
            if (!socket.Connected) return "Disconnect";
            try
            {
                socket.Send(byteBuf);
                AddCommLog(true, "SEND : " + Encoding.Default.GetString(byteBuf));
            }
            catch (Exception eX)
            {
                m_log.Error("Send : " + eX.Message);
                return "Error";
            }
            return "OK";
        }

        public string Send(string sBuf, Socket socket)
        {
            if (socket == null) return "Null";
            if (!socket.Connected) return "Disconne ct";
            try
            {
                byte[] aBuf = Encoding.UTF8.GetBytes(sBuf);
                socket.Send(aBuf);
                AddCommLog(true, "SEND : " + sBuf);
            }
            catch (Exception eX)
            {
                m_log.Error("Send : " + eX.Message);
                return "Error";
            }
            return "OK";
        }

        public List<Socket> GetClientSocketList()
        {
            return m_listSocket;
        }

        public Socket GetSocket(IPEndPoint ipEndPoint)
        {
            try
            {
                foreach (Socket socket in m_listSocket)
                {
                    if (socket.LocalEndPoint.ToString() == ipEndPoint.ToString()) return socket;
                }
            }
            catch (Exception ex)
            {
                m_log.Error("Get Socket Fail !! : " + ex.Message);
            }
            return null;
        }

        #region Comm Log
        public class CommLog
        {
            public string p_sMsg { get; set; }

            bool m_bSend = false;
            public Brush p_bColor
            {
                get { return m_bSend ? Brushes.Black : Brushes.DarkGray; }
            }

            public CommLog(bool bSend, string sMsg)
            {
                m_bSend = bSend;
                p_sMsg = sMsg;
            }

            public override string ToString()
            {
                return p_sMsg;
            }
        }

        public Queue<CommLog> m_aCommLog = new Queue<CommLog>();
        void AddCommLog(bool bSend, string sMsg)
        {
            m_aCommLog.Enqueue(new CommLog(bSend, DateTime.Now.ToLongTimeString() + " " + sMsg));
        }
        #endregion
    }
}
