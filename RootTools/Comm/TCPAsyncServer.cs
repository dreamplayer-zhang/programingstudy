using RootTools.Trees;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Controls;

namespace RootTools.Comm
{
    public class TCPAsyncServer : ITool, IComm
    {
        public delegate void OnReciveData(byte[] aBuf, int nSize, Socket socket);
        public event OnReciveData EventReciveData;

        #region ITool
        public UserControl p_ui
        {
            get
            {
                TCPAsyncServer_UI ui = new TCPAsyncServer_UI();
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
                if (value) InitServer();
                else ThreadStop();
            }
        }

        public int p_nPort { get; set; }

        void RunTreeSetting(Tree tree)
        {
            bool bUse = tree.Set(p_bUse, false, "Use", "Use Server");
            p_nPort = tree.Set(p_nPort, 5000, "Port", "Port Number");
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

        #region Server Socket
        Socket m_socket = null;
        void InitServer()
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.Bind(new IPEndPoint(IPAddress.Any, p_nPort));
            m_socket.Listen(5);
            m_cbAccept = new AsyncCallback(CallBackAccept);
            m_socket.BeginAccept(m_cbAccept, null);
            m_cbSend = new AsyncCallback(CallBackSend);
            m_commLog.Add(CommLog.eType.Info, "Init Server Socket");
        }
        #endregion

        #region Accept
        AsyncCallback m_cbAccept;
        Socket m_socketComm = null;
        void CallBackAccept(IAsyncResult ar)
        {
            if (m_socket == null) return;
            try
            {
                m_socketComm = m_socket.EndAccept(ar);
                Async async = new Async(m_lMaxBuffer);
                async.m_socket = m_socketComm;
                m_cbReceive = new AsyncCallback(CallBackReceive);
                m_socketComm.BeginReceive(async.m_aBuf, 0, m_lMaxBuffer, SocketFlags.None, m_cbReceive, async);
                m_commLog.Add(CommLog.eType.Info, "Accept Client Socket");
            }
            catch (Exception e)
            {
                m_commLog.Add(CommLog.eType.Info, "CallBackAccept Exception" + e.ToString());
                if(m_socket != null)
                    m_socket.Close();
                InitServer();
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
                if (lReceive > 0) EventReciveData(async.m_aBuf, lReceive, async.m_socket);
                if (m_bCommLog) m_commLog.Add(CommLog.eType.Receive, (lReceive < 256) ? Encoding.ASCII.GetString(async.m_aBuf, 0, lReceive) : "Large Data");
                async.m_socket.BeginReceive(async.m_aBuf, 0, m_lMaxBuffer, SocketFlags.None, m_cbReceive, async);
            }
            catch (Exception e)
            {
                if (m_socket != null) m_commLog.Add(CommLog.eType.Info, "CallBackReceive Exception : " + e.Message+e.StackTrace);
                m_socket.Close();
                InitServer();
            }
        }
        #endregion

        #region Send
        AsyncCallback m_cbSend;
        public string Send(string sMsg)
        {
            if (m_socket == null) return "Not Connected";
            Async async = new Async(1);
            async.m_aBuf = Encoding.Default.GetBytes(sMsg);
            async.m_socket = m_socketComm;
            m_socketComm.BeginSend(async.m_aBuf, 0, async.m_aBuf.Length, SocketFlags.None, m_cbSend, async);
            return "OK";
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
        public TCPAsyncServer(string id, Log log, int lMaxBuffer = 4096)
        {
            p_id = id;
            m_lMaxBuffer = lMaxBuffer;
            m_log = log;
            InitCommLog();
            InitTree();
        }

        public void ThreadStop()
        {
            if (m_socket != null) m_socket.Close();
            m_socket = null;
        }

    }
}
