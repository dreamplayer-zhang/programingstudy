using Microsoft.Win32.SafeHandles;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools.Comm
{
    public class NamedPipe : ITool, IComm
    {
        public const char c_cSeparate = ';';
        const uint c_nOpenMode = 0x40000003;

        #region Property
        string _sInfo = "";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                if (_sInfo == "OK") return;
                m_commLog.Add(CommLog.eType.Info, value); 
            }
        }
        #endregion

        #region Timer ReadMsg
        public delegate void dgReadMsg(string sMsg);
        public event dgReadMsg ReadMsg;

        Queue<string> m_qMsg = new Queue<string>(); 
        void ReadTimerMsg(string sMsg)
        {
            m_qMsg.Enqueue(sMsg);
        }

        DispatcherTimer m_timer = new DispatcherTimer(); 
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(10);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            while (m_qMsg.Count > 0)
            {
                string sMsg = m_qMsg.Dequeue();
                if (ReadMsg != null) ReadMsg(sMsg); 
            }
            CheckSend(); 
        }
        #endregion

        #region ITool
        public string p_id { get { return m_id; } }

        public UserControl p_ui
        {
            get
            {
                NamedPipe_UI ui = new NamedPipe_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        #region DLL Import
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(String pipeName, uint dwOpenMode, uint dwPipeMode, uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize, uint nDefaultTimeOut, IntPtr lpSecurityAttributes);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(SafeFileHandle hNamedPipe, IntPtr lpOverlapped);
        #endregion

        #region Name
        string m_sRecieveID = "";
        string m_sSendID = "";
        string _sTarget = "";
        public string p_sTarget
        {
            get { return _sTarget; }
            set
            {
                if (_sTarget == value) return;
                m_sRecieveID = value + "-" + m_id;
                m_sSendID = m_id + "-" + value;
                m_log.Info(m_id + " Change Target : " + _sTarget + " -> " + value);
                _sTarget = value;
            }
        }

        void RunNameTree(Tree tree)
        {
            tree.Set(m_id, m_id, "ID", "Host ID", true, true);
            p_sTarget = tree.Set(_sTarget, "", "Target", "Target ID");
        }
        #endregion

        #region Send
        int m_iSend = 0;
        public List<SendData> m_aSend = new List<SendData>();
        public string Send(params object[] objs)
        {
            if (objs.Length == 0) return m_id + " Has no Object";
            string sMsg = GetMsg(objs);
            return AddSend(sMsg);
        }

        public string Send(string sMsg)
        {
            return AddSend(GetMsg(sMsg)); 
        }

        string AddSend(string sMsg)
        {
            SendData send = new SendData(sMsg);
            m_iSend = (m_iSend + 1) % 1000;
            send.m_swSend.Restart(); 
            m_aSend.Add(send);
            p_sInfo = SendPipe(send);
            return p_sInfo;
        }

        static object g_lock = new object();
        string SendPipe(SendData send, string sSendID = "")
        {
            if (sSendID == "") sSendID = m_sSendID;
            if (sSendID == "") return "Target not Defined";
            lock (g_lock)
            {
                NamedPipeClientStream pipe = new NamedPipeClientStream(".", sSendID, PipeDirection.Out);
                try { pipe.Connect(300); }
                catch (Exception) { return m_id + " Client Connect Error"; }
                if (pipe.CanWrite == false) return m_id + "Can't Write Pipe : " + send.p_sMsg;
                m_commLog.Add(CommLog.eType.Send, send.p_sMsg); 
                send.Write(pipe);
                return "OK";
            }
        }

        string GetMsg(params object[] objs)
        {
            string sMsg = m_id + c_cSeparate + m_iSend.ToString("000");
            for (int n = 0; n < objs.Length; n++) sMsg += c_cSeparate + objs[n].ToString();
            return sMsg;
        }

        bool m_bRunThreadSend = false;
        Thread m_threadSend;
        void RunThreadSend()
        {
            StopWatch swConnect = new StopWatch();
            m_bRunThreadSend = true;
            Thread.Sleep(1000);
            while (m_bRunThreadSend)
            {
                Thread.Sleep(100);
                if (m_aSend.Count > 0)
                {
                    if (m_aSend[0].m_swSend.ElapsedMilliseconds > m_msSendAgain)
                    {
                        m_aSend[0].m_swSend.Restart();
                        if (m_aSend[0].m_nSendTry >= 1) ReadTimerMsg("Disconnect");
                        p_sInfo = "Send Retry = " + m_aSend[0].m_nSendTry.ToString() + ", " + m_aSend[0].p_sMsg;
                        p_sInfo = SendPipe(m_aSend[0]);
                        //if (m_aSend[0].m_nSendTry > 10) m_aSend.RemoveAt(0); 
                    }
                }
            }
        }

        SendData GetSendIndex(string sMsg)
        {
            for (int n = 0; n < m_aSend.Count; n++)
            {
                if (sMsg == m_aSend[n].p_sMsg) return m_aSend[n];
            }
            return null;
        }

        string SendReply(string sMsg)
        {
            SendData send = new SendData(sMsg);
            p_sInfo = SendPipe(send);
            return p_sInfo;
        }

        double m_secSendAgain = 5;
        int m_msSendAgain = 5000; 
        void RunSendTree(Tree tree)
        {
            m_secSendAgain = tree.Set(m_secSendAgain, 5, "SendAgain", "Send Again Time (sec)");
            m_msSendAgain = (int)(1000 * m_secSendAgain); 
        }

        public ObservableCollection<string> m_aSendLog = new ObservableCollection<string>();
        void CheckSend()
        {
            if (IsInvalidSend() == false) return;
            m_aSendLog.Clear();
            for (int n = 0; n < m_aSend.Count; n++)
            {
                m_aSendLog.Add(m_aSend[n].p_sMsg);
            }
        }

        bool IsInvalidSend()
        {
            if (m_aSendLog.Count != m_aSend.Count) return true;
            for (int n = 0; n < m_aSendLog.Count; n++)
            {
                if (m_aSendLog[n] != m_aSend[n].p_sMsg) return true;
            }
            return false;
        }
        #endregion

        #region Recieve
        bool m_bRunThreadListen = false;
        Thread m_threadListen;
        void RunThreadListen()
        {
            m_bRunThreadListen = true;
            Thread.Sleep(1000);
            while (m_bRunThreadListen)
            {
                Thread.Sleep(10);
                SafeFileHandle handle = CreateNamedPipe(@"\\.\pipe\" + m_sRecieveID, c_nOpenMode, 0, 255, (uint)m_lBufRecieve, (uint)m_lBufRecieve, 0, IntPtr.Zero);
                if (handle.IsInvalid == false)
                {
                    int nConnect = ConnectNamedPipe(handle, IntPtr.Zero);
                    if (nConnect > 0)
                    {
                        Thread threadRead = new Thread(new ParameterizedThreadStart(RunThreadRead));
                        threadRead.Start(handle);
                    }
                }
            }
        }

        private void RunThreadRead(object obj)
        {
            SafeFileHandle handle = (SafeFileHandle)obj;
            FileStream streamRead = null;
            try
            {
                ASCIIEncoding encoder = new ASCIIEncoding();
                int nRead = 0;
                byte[] bufRead = new byte[m_lBufRecieve];
                streamRead = new FileStream(handle, FileAccess.Read, (int)m_lBufRecieve, true);
                if (streamRead == null) return;
                StopWatch swRead = new StopWatch();
                while (swRead.ElapsedMilliseconds < m_msRecieveTimeout)
                {
                    try
                    {
                        int nByte = streamRead.Read(bufRead, nRead, (int)m_lBufRecieve - nRead);
                        nRead += nByte;
                    }
                    catch (Exception e) { p_sInfo = "ThreadListen Read Error : " + e.Message; }
                    if ((nRead > 0) && (bufRead[nRead - 1] == 0x0a))
                    {
                        try
                        {
                            string sMsg = encoder.GetString(bufRead, 0, nRead - 1);
                            m_commLog.Add(CommLog.eType.Receive, sMsg);
                            string[] sMsgs = sMsg.Split(c_cSeparate);
                            if (sMsgs[0] == m_id)
                            {
                                SendData send = GetSendIndex(sMsg);
                                if (send != null) m_aSend.Remove(send);
                            }
                            if (sMsgs[0] == p_sTarget)
                            {
                                string sReply = SendReply(sMsg);
                                if (sReply == "OK")
                                {
                                    m_log.Info(m_id + " ReadMsg = " + sMsg);
                                    ReadTimerMsg(sMsg);
                                }
                            }
                        }
                        catch (Exception e) { p_sInfo = "ThreadListen Data Convert Error : " + e.Message; }
                        return;
                    }
                    Thread.Sleep(10); 
                }
            }
            finally
            {
                if (streamRead != null) streamRead.Close();
                if (handle != null) handle.Close();
            }
        }

        int m_lBufRecieve = 1024;
        double m_secRecieveTimeout = 2;
        int m_msRecieveTimeout = 2000; 
        void RunRecieveTree(Tree tree)
        {
            m_lBufRecieve = tree.Set(m_lBufRecieve, 1024, "Buffer", "Buffer Size (byte)");
            m_secRecieveTimeout = tree.Set(m_secRecieveTimeout, 2, "Timeout", "Read Timeout (sec)");
            m_msRecieveTimeout = (int)(1000 * m_secRecieveTimeout); 
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
            m_aSend.Clear(); 
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTree(m_treeRoot);
        }

        void RunTree(Tree treeRoot)
        {
            RunNameTree(treeRoot.GetTree("Name"));
            RunSendTree(treeRoot.GetTree("Send"));
            RunRecieveTree(treeRoot.GetTree("Recieve"));
        }
        #endregion

        string m_id;
        Log m_log;
        public CommLog m_commLog = null;
        public TreeRoot m_treeRoot; 
        public NamedPipe(string id, Log log)
        {
            m_id = id;
            m_log = log;
            m_commLog = new CommLog(this, m_log);

            m_treeRoot = new TreeRoot(id, log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

            InitTimer(); 

            m_threadListen = new Thread(new ThreadStart(RunThreadListen));
            m_threadListen.Start();
            m_threadSend = new Thread(new ThreadStart(RunThreadSend));
            m_threadSend.Start();
        }

        public void ThreadStop()
        {
            if (m_bRunThreadListen)
            {
                m_bRunThreadListen = false;
                SendData send = new SendData("ThreadStop");
                while (m_threadListen.IsAlive)
                {
                    SendPipe(send, m_sRecieveID);
                    Thread.Sleep(20);
                }
                m_threadListen.Interrupt();
                m_threadListen.Abort();
            }
            if (m_bRunThreadSend)
            {
                m_bRunThreadSend = false;
                m_threadSend.Join();
            }
        }

        public class SendData
        {
            public string p_sMsg { get; set; }
            public byte[] m_bufSend = null;
            public StopWatch m_swSend = new StopWatch();
            public int m_nSendTry = 0;

            public SendData(string sMsg)
            {
                m_swSend.Restart();
                p_sMsg = sMsg;
                ASCIIEncoding encoder = new ASCIIEncoding();
                m_bufSend = encoder.GetBytes(p_sMsg + "\n");
            }

            public string Write(NamedPipeClientStream pipeSend)
            {
                m_swSend.Restart();
                pipeSend.Write(m_bufSend, 0, m_bufSend.Length);
                pipeSend.Flush();
                m_nSendTry++;
                return "OK";
            }
        }
    }
}
