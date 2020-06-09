using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Windows.Controls;
using RootTools.Trees;

namespace RootTools.Comm
{
    public class TCPIPClient : NotifyProperty, ITool, IComm
    {
        public event TCPIPServer.OnReciveData EventReciveData;

        #region ITool
        public string p_id { get; set; }

        string _sInfo = "";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value == "OK") return;
                m_commLog.Add(CommLog.eType.Info, value);
            }
        }

        public UserControl p_ui
        {
            get
            {
                TCPIPClient_UI ui = new TCPIPClient_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        #region Setting
        bool _bUse = false;
        public bool p_bUse
        {
            get
            {
                return _bUse;
            }
            set
            {
                if (value == _bUse) return;
                _bUse = value;
                if (value) InitSocket();
                else ThreadStop();
            }
        }

        string _sIP = "0.0.0.0";
        public string p_sIP
        {
            get { return _sIP; } 
            set { _sIP = value; }
        }

        int _nPort = 0;
        public int p_nPort
        {
            get { return _nPort; }
            set { _nPort = value; }
        }

        int _nConnectInterval = 3000;
        public int p_nConnectInterval
        {
            get { return _nConnectInterval; }
            set { _nConnectInterval = (value < 1000) ? 3000 : value; }
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

        void RunTree(Tree treeRoot)
        {
            RunSetTree(treeRoot.GetTree("Set"));
        }

        void RunSetTree(Tree tree)
        {
            p_bUse = tree.Set(_bUse, false, "Use", "Use Server");
            p_sIP = tree.Set(_sIP, "0.0.0.0", "IP", "IP Address");
            p_nPort = tree.Set(_nPort, 0, "Port", "Port Number");
            p_nConnectInterval = tree.Set(_nConnectInterval, 3000, "Interval", "Connection Interval (ms)");
        }
        #endregion

        #region Socket
        void InitSocket()
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            m_threadConnect = new Thread(new ThreadStart(RunThreadConnect));
            m_threadConnect.Start();
        }

        public string Send(byte[] byteBuf)
        {
            if (!m_socket.Connected) return "Disconnect";
            try
            {
                m_commLog.Add(CommLog.eType.Send, Encoding.Default.GetString(byteBuf));
                m_socket.Send(byteBuf);
            }
            catch (Exception e)
            {
                p_sInfo = "Send : " + e.Message; 
                return p_sInfo;
            }
            return "OK";
        }

        public string Send(string sMsg)
        {
            if (m_socket == null) return "Null";
            if (!m_socket.Connected) return "Disconnect";
            try
            {
                byte[] aBuf = Encoding.UTF8.GetBytes(sMsg);
                m_commLog.Add(CommLog.eType.Send, sMsg); 
                m_socket.Send(aBuf);
            }
            catch (Exception eX)
            {
                p_sInfo = "Send : " + eX.Message;
                return p_sInfo;
            }
            return "OK";
        }
        #endregion

        #region Thread
        void RunThreadConnect()
        {
            m_bRunConnect = true;
            Thread.Sleep(5000);
            while (m_bRunConnect)
            {
                Thread.Sleep(10);
                Thread.Sleep(_nConnectInterval);
                if (p_bUse)
                {
                    if (!m_socket.Connected)
                    {
                        try
                        {
                            m_socket.Connect(_sIP, _nPort);
                            p_sInfo = p_id + " Connect Success !!"; 
                            if (m_threadRecive != null)
                            {
                                m_bRunRecive = false;
                                m_threadRecive.Join();
                                m_threadRecive = null;
                            }
                            m_threadRecive = new Thread(new ThreadStart(RunThreadRecive));
                            m_threadRecive.Start();
                        }
                        catch (Exception ex)
                        {
                            p_sInfo = p_id + " Connect Fail : " + ex.Message; 
                        }
                    }
                }
            }
        }

        void RunThreadRecive()
        {
            m_bRunRecive = true;
            int nSize;
            byte[] aBuf = new byte[m_nBufRecieve];
            while (m_socket.Connected)
            {
                Thread.Sleep(10);
                if (!m_bRunRecive) break;
                try
                {
                    nSize = m_socket.Receive(aBuf);
                    m_commLog.Add(CommLog.eType.Receive, (nSize < 1024) ? Encoding.ASCII.GetString(aBuf, 0, nSize) : "..."); 
                    if (EventReciveData != null) EventReciveData(aBuf, nSize, m_socket);
                }
                catch (Exception ex)
                {
                    p_sInfo = "Recive Fail : " + ex.Message; 
                }
            }
        }
        #endregion

        Log m_log;
        int m_nBufRecieve = 1024 * 1024; 
        public TreeRoot m_treeRoot;
        Socket m_socket = null;
        bool m_bRunConnect = false;
        Thread m_threadConnect;
        bool m_bRunRecive = false;
        Thread m_threadRecive;
        public CommLog m_commLog = null;
        public TCPIPClient(string id, Log log, int nBufRecieve = -1)
        {
            p_id = id;
            m_log = log;
            m_nBufRecieve = (nBufRecieve > 0) ? nBufRecieve : m_nBufRecieve; 
            m_commLog = new CommLog(this, m_log);

            m_treeRoot = new TreeRoot(id, log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

            InitSocket();
        }

        public void ThreadStop()
        {
            if (m_bRunConnect)
            {
                m_bRunConnect = false;
                m_threadConnect.Abort();
            }
            if (m_bRunRecive)
            {
                m_bRunRecive = false;
                m_threadRecive.Abort();
            }
            m_socket.Close();
            m_socket.Dispose();
        }
    }
}
