using RootTools.Trees;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Comm
{
    public class TCPAsyncClient : NotifyProperty, ITool, IComm
    {
        public delegate void OnReciveData(byte[] aBuf, int nSize, Socket socket);
        public event OnReciveData EventReceiveData;

        #region ITool
        public UserControl p_ui
        {
            get
            {
                TCPAsyncClient_UI ui = new TCPAsyncClient_UI();
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
                if (value) InitClient();
                else ThreadStop();
            }
        }

        string _sIP = "127.0.0.1";
        public string p_sIP
        {
            get { return _sIP; }
            set
            {
                _sIP = value;
                OnPropertyChanged();
            }
        }

        public int p_nPort { get; set; }

        void RunTreeSetting(Tree tree)
        {
            bool bUse = tree.Set(p_bUse, false, "Use", "Use Server");
            p_sIP = tree.Set(_sIP, "127.0.0.1", "IP", "IP Address");
            p_nPort = tree.Set(p_nPort, p_nPort, "Port", "Port Number");
            p_bUse = bUse;
        }
        #endregion

        #region Async
        public class Async
        {
            public byte[] m_aBuf;
            public Socket m_socket;

            public Async(int nL)
            {
                m_aBuf = new byte[nL];
            }
        }
        #endregion

        #region Client Socket
        Socket m_socket = null;
        public void InitClient()
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_commLog.Add(CommLog.eType.Info, "Init Client Socket");
            m_cbSend = new AsyncCallback(CallBackSend);
            Connect();
        }
        #endregion

        #region Connect
        public bool p_bConnect
        {
            get { return (m_socket == null) ? false : m_socket.Connected; }
        }

        public void Connect()
        {
            try
            {
                //if (m_socket == null) return;
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_socket.Connect(p_sIP, p_nPort);
                Async async = new Async(m_lMaxBuffer);
                async.m_socket = m_socket;
                m_cbReceive = new AsyncCallback(CallBackReceive);
                m_socket.BeginReceive(async.m_aBuf, 0, m_lMaxBuffer, SocketFlags.None, m_cbReceive, async);
                m_commLog.Add(CommLog.eType.Info, "Connected");
            }
            catch (Exception e)
            {
                m_commLog.Add(CommLog.eType.Info, "Connect Error " + e.Message);
            }
        }
        #endregion

        #region ThreadConnect
        bool m_bThreadConnect = false; 
        Thread m_threadConnect;
        public void ThreadConnect()
        {
            m_bThreadConnect = true;
            Thread.Sleep(5000); 
            while (m_bThreadConnect)
            {
                Thread.Sleep(1000);
                if (_bUse == true)
                {
                    if (m_socket.Connected == false)
                        Connect();
                }
            }
        }
        #endregion

        #region Receive
        AsyncCallback m_cbReceive;
        void CallBackReceive(IAsyncResult ar)
        {
            Async async = (Async)ar.AsyncState;
            if (m_socket == null) return;
            try
            {
                int lReceive = async.m_socket.EndReceive(ar);
                if (lReceive > 0)
                {
                    if (EventReceiveData != null) EventReceiveData(async.m_aBuf, lReceive, async.m_socket);
                    if (m_bCommLog) m_commLog.Add(CommLog.eType.Receive, (lReceive < 256) ? Encoding.Default.GetString(async.m_aBuf, 0, lReceive) : "Large Data");
                }
                async.m_socket.BeginReceive(async.m_aBuf, 0, m_lMaxBuffer, SocketFlags.None, m_cbReceive, async);
            }
            catch (Exception e)
            {
                if (m_socket != null) m_commLog.Add(CommLog.eType.Info, "CallBack Exception : " + e.Message);
            }
        }
        #endregion

        #region Send
        AsyncCallback m_cbSend;
        static readonly object g_lock = new object();
        public string Send(string sMsg)
        {
            if (m_socket == null || !m_socket.Connected) return "Not Connected";
            lock (g_lock)
            {
                Async async = new Async(1);
                async.m_aBuf = Encoding.Default.GetBytes(sMsg);
                async.m_socket = m_socket;
                m_socket.BeginSend(async.m_aBuf, 0, async.m_aBuf.Length, SocketFlags.None, m_cbSend, async);
                return "OK";
            }
        }

        public void Send(byte[] p)
        {
            m_socket.Send(p);
        }

        void CallBackSend(IAsyncResult ar)
        {
            Async async = (Async)ar.AsyncState;
            int lSend = async.m_socket.EndSend(ar);
            if (m_bCommLog) m_commLog.Add(CommLog.eType.Send, (lSend < 64) ? Encoding.Default.GetString(async.m_aBuf, 0, lSend) : "Large Data");
        }
        #endregion

        #region CommLog
        public CommLog m_commLog;
        void InitCommLog()
        {
            m_commLog = new CommLog(this, m_log);
        }

        bool m_bCommLog = true;
        void RunTreeCommLog(Tree tree)
        {
            m_bCommLog = tree.Set(m_bCommLog, m_bCommLog, "Enable", "CommLog Enable (false = Fast)");
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(p_id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
        }

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
            RunTreeSetting(treeRoot.GetTree("Setting"));
            RunTreeCommLog(treeRoot.GetTree("CommLog"));
        }
        #endregion

        public string p_id { get; set; }
        int m_lMaxBuffer = 4096;
        Log m_log;
        
        public TCPAsyncClient(string id, Log log, int lMaxBuffer = 4096)
        {
            p_id = id;
            m_lMaxBuffer = lMaxBuffer;
            m_log = log;
            InitCommLog();
            InitTree();
            m_threadConnect = new Thread(ThreadConnect);
            m_threadConnect.Start();
        }

        public void ThreadStop()
        {
            if (m_bThreadConnect)
            {
                m_bThreadConnect = false;
                m_threadConnect.Join(); 
            }
            if (m_socket != null) m_socket.Close();
            m_socket = null;
            m_commLog.Add(CommLog.eType.Info, "Socket Closed");
        }

    }
}
