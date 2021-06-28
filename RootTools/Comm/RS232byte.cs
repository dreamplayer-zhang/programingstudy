using RootTools.Trees;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Comm
{
    public class RS232byte : NotifyProperty, ITool, IComm
    {
        #region Property
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
        #endregion

        #region Connect Param
        public string m_sPort = "COM99";
        public int m_nBaudrate = 19200;
        public int m_nDataBit = 8;
        public Parity m_eParity = Parity.None;
        public StopBits m_eStopbits = StopBits.None;
        public bool m_bUseCallback = true;
        public Handshake m_eHandshake = Handshake.XOnXOff;
        public bool m_bLog = true;

        void RunConnectTree(Tree tree)
        {
            m_sPort = tree.Set(m_sPort, m_sPort, "Port ID", "RS232 Port Name (COM5)");
            m_nBaudrate = tree.Set(m_nBaudrate, 19200, "Baud Rate", "Baud Rate (bit/sec), 9600, 19200, 38400 ...");
            m_eParity = (Parity)tree.Set(m_eParity, m_eParity, "Parity", "Parity");
            m_nDataBit = tree.Set(m_nDataBit, m_nDataBit, "Data Bit", "Data Bit (7 ~ 8");
            m_eStopbits = (StopBits)tree.Set(m_eStopbits, m_eStopbits, "Stop Bit", "Stop Bit");
            m_bUseCallback = tree.Set(m_bUseCallback, true, "Callback", "Callback");
            m_eHandshake = (Handshake)tree.Set(m_eHandshake, m_eHandshake, "Handshake", "Handshake");
        }
        #endregion

        #region Connect
        public bool p_bConnect
        {
            get { return m_sp != null; }
            set
            {
                if ((m_sp != null) == value) return;
                m_log.Info(p_id + " Connect -> " + value.ToString());
                if (value)
                {
                    _sInfo = Connect();
                    m_commLog.Add(CommLog.eType.Info, _sInfo);
                    if (_sInfo != "OK") m_sp = null;
                }
                else p_sInfo = DisConnect();
                OnPropertyChanged();
            }
        }

        public string Connect()
        {
            if (m_sp != null) ThreadStop();
            try
            {
                m_sp = new SerialPort(m_sPort, m_nBaudrate, m_eParity, m_nDataBit, m_eStopbits);
                m_sp.Handshake = m_eHandshake;

                //m_sp.RtsEnable = true;
                m_sp.Encoding = Encoding.ASCII;
            }
            catch (Exception e) { return "SerialPort Create Error : " + e.Message; }
            if (m_sp == null) return "New SerialPort Assign Error";
            m_sp.ReadTimeout = m_msReadTimeout;
            m_sp.WriteTimeout = m_msWriteTimeout;
            Thread.Sleep(300);
            try
            {
                m_sp.Open();
            }
            catch (Exception e)
            {
                return "SerialPort Open Error : " + e.Message;
            }
            if (m_bUseCallback) m_sp.DataReceived += M_sp_DataReceived;
            return "OK";
        }

        string DisConnect()
        {
            if (m_sp == null) return "SerialPort not Assigned";
            if (m_sp.IsOpen) m_sp.Close();
            m_sp = null;
            return "OK";
        }

        bool CheckConnect()
        {
            if (p_bConnect) return true;
            if (Connect() != "OK") return false;
            if (p_bConnect) return true;
            return false;
        }
        #endregion

        #region Receive
        public delegate void dgOnReceive(byte[] aRead, int nRead);
        public event dgOnReceive OnReceive;

        public int p_lRead
        {
            get { return m_aRead.Length; }
            set
            {
                if (m_aRead.Length == value) return;
                m_aRead = new byte[value]; 
            }
        }
        public byte[] m_aRead = new byte[80]; 
        private void M_sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (OnReceive == null) return;
            SerialPort sp = (SerialPort)sender;
            int nRead = sp.Read(m_aRead, 0, p_lRead);
            string sRead = Encoding.ASCII.GetString(m_aRead, 0, nRead);
            if(m_bLog) m_commLog.Add(CommLog.eType.Receive, sRead);
            if (OnReceive != null) OnReceive(m_aRead, nRead); 
        }
        #endregion

        #region Write
        public string Send(string sMsg)
        {
            lock (m_csLock)
            {
                if(m_bLog) m_commLog.Add(CommLog.eType.Send, sMsg);
                m_sp.Write(sMsg);
                return "OK"; 
            }
        }

        static readonly object m_csLock = new object();
        public string Send(byte[] aWrite, int nWrite, int nOffset = 0)
        {
            lock (m_csLock)
            {
                if (m_sp == null)
                    return "Fail";
                string sWrite = Encoding.ASCII.GetString(aWrite, nOffset, nWrite);
                if (m_bLog) m_commLog.Add(CommLog.eType.Send, sWrite);
                m_sp.Write(aWrite, nOffset, nWrite);
                return "OK";
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
            RunConnectTree(m_treeRoot.GetTree("Connect"));
            RunTimeoutTree(m_treeRoot.GetTree("Timeout"));
            RunSettingTree(m_treeRoot.GetTree("Setting"));
        }

        int m_msWriteTimeout = 2000;
        int m_msReadTimeout = 2000;
        void RunTimeoutTree(Tree tree)
        {
            m_msReadTimeout = tree.Set(m_msReadTimeout, 2000, "Read", "Read Timeout (ms)");
            m_msWriteTimeout = tree.Set(m_msWriteTimeout, 2000, "Write", "Write Timeout (ms)");
        }

        void RunSettingTree(Tree tree)
        {
            m_bLog = tree.Set(m_bLog, m_bLog, "Log", "Write Send/Receive Log");
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                RS232_UI ui = new RS232_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        Log m_log;
        public CommLog m_commLog = null;
        public TreeRoot m_treeRoot;
        public SerialPort m_sp = null;
        public RS232byte(string id, Log log)
        {
            p_id = id;
            m_log = log;
            m_commLog = new CommLog(this, m_log);
            m_treeRoot = new TreeRoot(id, log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
        }

        public void ThreadStop()
        {
            if (m_sp == null) return;
            if (m_sp.IsOpen) m_sp.Close();
            m_sp.Dispose();
            m_sp = null;
        }
    }
}
