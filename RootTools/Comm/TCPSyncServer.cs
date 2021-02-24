using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RootTools.Comm
{
    public class TCPSyncServer : ITool, IComm
    {
        public delegate void OnReciveData(byte[] aBuf, int nSize, Socket socket);
        public event OnReciveData EventReciveData;
        
        #region ITool
        public UserControl p_ui
        {
            get
            {
                TCPSyncServer_UI ui = new TCPSyncServer_UI();
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

        #region Server Socket
        Socket m_socket = null;
        void InitServer()
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.Bind(new IPEndPoint(IPAddress.Any, p_nPort));
            m_socket.Listen(5);
            m_cbAccept = new AsyncCallback(CallBackAccept);
            m_socket.BeginAccept(m_cbAccept, null);
            m_commLog.Add(CommLog.eType.Info, "Init Server Socket");
        }
        #endregion

        #region Accept
        AsyncCallback m_cbAccept;
        Socket m_socketComm = null;
        void CallBackAccept(IAsyncResult ar)
        {
            m_socketComm = m_socket.EndAccept(ar);
            m_commLog.Add(CommLog.eType.Info, "Accept Client Socket");
            InitThread(); 
        }
        #endregion

        #region Read Thread 
        bool m_bThread = false;
        Thread m_thread;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        byte[] m_aBufRead = null; 
        void RunThread()
        {
            m_bThread = true; 
            while (m_bThread)
            {
                Thread.Sleep(1); 
                try
                {
                    int lReceive = m_socketComm.Receive(m_aBufRead);
                    if (lReceive > 0) EventReciveData(m_aBufRead, lReceive, m_socketComm);
                    if (m_bCommLog) m_commLog.Add(CommLog.eType.Receive, (lReceive < 64) ? Encoding.ASCII.GetString(m_aBufRead, 0, lReceive) : "Large Data");
                }
                catch (Exception e) { m_commLog.Add(CommLog.eType.Info, "Receive Exception : " + e.Message); }
            }
        }
        #endregion

        #region Send
        public string Send(string sMsg)
        {
            if (m_socketComm == null) return "Not Connected";
            byte[] aBuf = Encoding.Default.GetBytes(sMsg);
            m_socketComm.Send(aBuf);
            if (m_bCommLog) m_commLog.Add(CommLog.eType.Send, (sMsg.Length < 64) ? sMsg : "Large Data");
            return "OK";
        }

        public string Send(byte[] aBuf)
        {
            if (m_socketComm == null) return "Not Connected";
            m_socketComm.Send(aBuf);
            if (m_bCommLog) m_commLog.Add(CommLog.eType.Send, (aBuf.Length < 64) ? Encoding.ASCII.GetString(aBuf, 0, aBuf.Length) : "Large Data");
            return "OK";
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
        Log m_log;
        public TCPSyncServer(string id, Log log, int lMaxBuffer = 4096)
        {
            p_id = id;
            m_aBufRead = new byte[lMaxBuffer];
            m_log = log;
            InitCommLog();
            InitTree();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Interrupt(); 
                m_thread.Abort(); 
            }
            if (m_socketComm != null)
            {
                m_socketComm.Shutdown(SocketShutdown.Both);
                m_socketComm.Close(); 
            }
            if (m_socket != null) m_socket.Close();
            m_socket = null;
        }
    }
}
