using RootTools.Trees;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Comm
{
    public class TCPSyncClient : NotifyProperty, ITool, IComm
    {
        public delegate void OnReciveData(byte[] aBuf, int nSize, Socket socket);
        public event OnReciveData EventReciveData;

        #region ITool
        public UserControl p_ui
        {
            get
            {
                TCPSyncClient_UI ui = new TCPSyncClient_UI();
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
            p_nPort = tree.Set(p_nPort, 5000, "Port", "Port Number");
            p_bUse = bUse;
        }
        #endregion

        #region Client Socket
        Socket m_socket = null;
        void InitClient()
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_commLog.Add(CommLog.eType.Info, "Init Client Socket");
            Connect();
        }
        #endregion

        #region Connect
        public bool p_bConnect
        {
            get { return (m_socket == null) ? false : m_socket.Connected; }
        }

        void Connect()
        {
            try
            {
                m_socket.Connect(p_sIP, p_nPort);
                m_commLog.Add(CommLog.eType.Info, "Connected");
                InitThread();
            }
            catch (Exception e)
            {
                m_commLog.Add(CommLog.eType.Info, "Connect Error " + e.Message);
            }
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
                    int lReceive = m_socket.Receive(m_aBufRead);
                    if (lReceive > 0) EventReciveData(m_aBufRead, lReceive, m_socket);
                    //m_commLog.Add(CommLog.eType.Receive, (lReceive < 64) ? Encoding.ASCII.GetString(m_aBufRead, 0, lReceive) : "Large Data");
                }
                catch (Exception e) { m_commLog.Add(CommLog.eType.Info, "Receive Exception : " + e.Message); }
            }
        }
        #endregion

        #region Send
        public int m_lRead = 0; 
        public string Send(string sMsg)
        {
            m_socket.Send(Encoding.ASCII.GetBytes(sMsg));
            //m_commLog.Add(CommLog.eType.Send, (sMsg.Length < 64) ? sMsg : "Large Data");
            return "OK";
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
        }
        #endregion

        public string p_id { get; set; }
        Log m_log;
        public CommLog m_commLog;
        public TCPSyncClient(string id, Log log, int lMaxBuffer = 4096)
        {
            p_id = id;
            m_aBufRead = new byte[lMaxBuffer]; 
            m_log = log;
            m_commLog = new CommLog(this, log);

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
            if (m_socket != null) m_socket.Close();
            m_socket = null;
        }
    }
}
