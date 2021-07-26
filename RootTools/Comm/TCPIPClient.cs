using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Windows.Controls;
using RootTools.Trees;
using System.ComponentModel;
using System.Collections.Generic;

namespace RootTools.Comm
{
    public class TCPIPClient : NotifyProperty, ITool, IComm
    {
        public event TCPIPServer.OnReciveData EventReciveData;
        public event TCPIPServer.OnConnect EventConnect;

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
                if (value == _bUse) return;
                _bUse = value;
                OnPropertyChanged();
                if ((value == false) && (m_socket != null))
                {
                    m_socket.Close();
                    m_socket.Dispose();
                    m_socket = null;
                }
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

        int _nPort = 0;
        public int p_nPort
        {
            get { return _nPort; }
            set
            {
                _nPort = value;
                OnPropertyChanged();
            }
        }

        int _nConnectInterval = 3000;
        public int p_nConnectInterval
        {
            get { return _nConnectInterval; }
            set
            {
                _nConnectInterval = (value < 1000) ? 3000 : value;
                OnPropertyChanged();
            }
        }

        void RunTreeSetting(Tree tree)
        {
            p_bUse = tree.Set(_bUse, false, "Use", "Use Server");
            p_sIP = tree.Set(_sIP, "127.0.0.1", "IP", "IP Address");
            p_nPort = tree.Set(_nPort, 0, "Port", "Port Number");
            p_nConnectInterval = tree.Set(_nConnectInterval, 3000, "Interval", "Connection Interval (ms)");
        }
        #endregion

        #region Connect
        Socket m_socket = null;

        public bool p_bConnect
        {
            get { return (m_socket == null) ? false : m_socket.Connected; }
        }

        public string Connect()
        {
            try
            {
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                if (m_socket != null) m_socket.BeginConnect(p_sIP, p_nPort, new AsyncCallback(CallBack_Connect), m_socket);
                OnPropertyChanged("p_bConnect");
                return "OK";
            }
            catch (SocketException eX) { return "Connect :" + eX.Message; }
        }

        void CallBack_Connect(IAsyncResult ar)
        {
            try
            {
                if (ar == null) return;
                Socket socket = (Socket)ar.AsyncState;
                if (socket.Connected == false) return;
                socket.EndConnect(ar);

                if (EventConnect != null) EventConnect(socket);

                socket.BeginReceive(m_aBufRead, 0, m_aBufRead.Length, SocketFlags.None, new AsyncCallback(CallBack_Receive), socket);
                p_sInfo = p_id + " is Connect !!";
            }
            catch (SocketException eX) { p_sInfo = "CallBack_Connect : " + eX.Message; }
        }
        #endregion

        #region Receive & Send
        byte[] m_aBufRead;
        void CallBack_Receive(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                if (ar == null || !socket.Connected)
                {
                    // 연결에 문제 있음을 확인
                    m_commLog.Add(CommLog.eType.Info, "Disconnect !!");

                    if (EventReciveData != null) EventReciveData(m_aBufRead, 0, socket);
                    socket.Close();

                    return;
                }

                int nReadLength = socket.EndReceive(ar);
                if (nReadLength > 0)
                {
                    byte[] buf = new byte[m_aBufRead.Length];
                    Array.Copy(m_aBufRead, buf, nReadLength);

                    m_commLog.Add(CommLog.eType.Receive, (nReadLength < 1024) ? Encoding.ASCII.GetString(buf, 0, nReadLength) : "...");

                    if (EventReciveData != null) EventReciveData(buf, nReadLength, socket);

                    socket.BeginReceive(m_aBufRead, 0, m_aBufRead.Length, SocketFlags.None, new AsyncCallback(CallBack_Receive), socket);
                }
                else m_commLog.Add(CommLog.eType.Info, "CallBack_Receive Close");
            }
            catch (SocketException ex)
            {
                // SocketException 발생
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                {
                    m_commLog.Add(CommLog.eType.Info, "Disconnected from server !!");

                    if (EventReciveData != null) EventReciveData(m_aBufRead, 0, socket);
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

        Queue<byte[]> m_qSendByte = new Queue<byte[]>();
        Queue<string> m_qSend = new Queue<string>();
        public string Send(string sMsg)
        {
            m_qSend.Enqueue(sMsg);
            return "OK";
        }
        public string Send(byte[] sMsg)
        {
            m_qSendByte.Enqueue(sMsg);
            return "OK";
        }
        string SendMsg(string sMsg)
        {
            try
            {
                m_commLog.Add(CommLog.eType.Send, sMsg);
                m_socket.Send(Encoding.ASCII.GetBytes(sMsg));
            }
            catch (Exception e)
            {
                m_socket = null;
                OnPropertyChanged("p_bConnect");
                p_sInfo = "Send : " + e.Message;
                return p_sInfo;
            }
            return "OK";
        }
        string SendMsg(byte[] sMsg)
        {
            try
            {
                m_commLog.Add(CommLog.eType.Send, Encoding.ASCII.GetString(sMsg));
                m_socket.Send(sMsg);
            }
            catch (Exception e)
            {
                m_socket = null;
                OnPropertyChanged("p_bConnect");
                p_sInfo = "Send : " + e.Message;
                return p_sInfo;
            }
            return "OK";
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
            RunTreeSetting(m_treeRoot.GetTree("Setting"));
        }

        public void RunTree(Tree treeRoot)
        {
            RunTreeSetting(treeRoot.GetTree("Setting"));
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread;

        void RunThread()
        {
            m_bThread = true;
            while (m_bThread)
            {
                Thread.Sleep(10);
                if (p_bUse)
                {
                    if (p_bConnect == false)
                    {
                        Thread.Sleep(5000);
                        p_sInfo = Connect();
                    }
                    else if (m_qSend.Count > 0)
                    {
                        string sMsg = m_qSend.Peek();
                        string sSend = SendMsg(sMsg);
                        if (sSend == "OK") m_qSend.Dequeue();
                        else m_commLog.Add(CommLog.eType.Info, sSend);
                    }
                    else if (m_qSendByte.Count > 0)
                    {
                        byte[] sMsg = m_qSendByte.Peek();
                        string sSend = SendMsg(sMsg);
                        if (sSend == "OK") m_qSendByte.Dequeue();
                        else m_commLog.Add(CommLog.eType.Info, sSend);
                    }
                }
            }
        }
        #endregion

        Log m_log;
        public TreeRoot m_treeRoot;
        public CommLog m_commLog = null;
        public TCPIPClient(string id, Log log, int nBufReceive = -1)
        {
            p_id = id;
            m_log = log;
            m_aBufRead = new byte[(nBufReceive > 0) ? nBufReceive : 4096];

            m_commLog = new CommLog(this, m_log);

            m_treeRoot = new TreeRoot(id, log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_qSend.Clear();
                m_qSendByte.Clear();
                m_thread.Join();
            }
            if (m_socket != null)
            {
                m_socket.Close();
                m_socket.Dispose();
                m_socket = null;
            }
        }

        public bool IsConnected()
        {
            if (m_socket != null)
                return m_socket.Connected;

            return false;
        }
    }
}
