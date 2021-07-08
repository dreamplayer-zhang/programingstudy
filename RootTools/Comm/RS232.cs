using RootTools.Trees;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Comm
{
    public class RS232 : NotifyProperty, ITool, IComm
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

        void RunConnectTree(Tree tree)
        {
            m_sPort = tree.Set(m_sPort, m_sPort, "Port ID", "RS232 Port Name (COM5)");
            m_nBaudrate = tree.Set(m_nBaudrate, 19200, "Baud Rate", "Baud Rate (bit/sec), 9600, 19200, 38400 ...");
            m_eParity = (Parity)tree.Set(m_eParity, m_eParity, "Parity", "Parity");
            m_nDataBit = tree.Set(m_nDataBit, m_nDataBit, "Data Bit", "Data Bit (7 ~ 8");
            m_eStopbits = (StopBits)tree.Set(m_eStopbits, m_eStopbits, "Stop Bit", "Stop Bit");
            m_bUseCallback = tree.Set(m_bUseCallback, true, "Callback", "Callback"); 
        }
        #endregion

        #region Connect
        public bool p_bConnect
        {
            get { return m_sp != null; }
            set
            {
                if ((m_sp != null) == value) return;

                if(m_log != null)
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

        string Connect()
        {
            if (m_sp != null) ThreadStop();
            try
            {
                m_sp = new SerialPort(m_sPort, m_nBaudrate, m_eParity, m_nDataBit, m_eStopbits);
                m_sp.Handshake = Handshake.XOnXOff;
                
                //m_sp.RtsEnable = true;
                m_sp.Encoding = Encoding.ASCII; 
            }
            catch (Exception e) { return "SerialPort Create Error : " + e.Message; }
            if (m_sp == null) return "New SerialPort Assign Error";
            m_sp.ReadTimeout = m_msReadTimeout;
            m_sp.WriteTimeout = m_msWriteTimeout;
            Thread.Sleep(300);
            try { 
                m_sp.Open();
            }
            catch (Exception e) {
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

        #region Protocol
        public enum eStartBit
        {
            None,
            STX,
        }
        string m_sStart = "";
        eStartBit _eStartBit = eStartBit.None; 
        eStartBit p_eStartBit
        {
            get { return _eStartBit; }
            set
            {
                if (_eStartBit == value) return;
                _eStartBit = value;
                switch (value)
                {
                    case eStartBit.None: m_sStart = ""; break;
                    case eStartBit.STX: m_sStart = ((char)0x02).ToString(); break;
                    default: p_sInfo = "Unknown Start Bit : " + value.ToString(); break;
                }
            }
        }

        public enum eEndBit
        {
            None,
            CR,
            LF,
            CRLF,
            LFCR_4CH,
            ETX
        }
        string m_sEnd = "";
        eEndBit _eEndBit = eEndBit.None; 
        eEndBit p_eEndBit
        {
            get { return _eEndBit; }
            set
            {
                if (_eEndBit == value) return;
                _eEndBit = value;
                switch (value)
                {
                    case eEndBit.None: m_sEnd = ""; break;
                    case eEndBit.CR: m_sEnd = "\r"; break;
                    case eEndBit.LF: m_sEnd = "\n"; break;
                    case eEndBit.CRLF: m_sEnd = "\r\n"; break;
                    case eEndBit.LFCR_4CH: m_sEnd = "\n\r$>"; break;
                    case eEndBit.ETX: m_sEnd = ((char)0x03).ToString(); break;
                    default: p_sInfo = "Unknown End Bit : " + value.ToString(); break; 
                }
            }
        }

        void RunProtocolTree(Tree tree)
        {
            p_eStartBit = (eStartBit)tree.Set(p_eStartBit, p_eStartBit, "Start", "Start Bit Type");
            p_eEndBit = (eEndBit)tree.Set(p_eEndBit, p_eEndBit, "End", "End Bit Type"); 
        }
        #endregion

        #region Receive
        public delegate void dgOnReceive(string sRead);
        public event dgOnReceive OnReceive;

        public string m_sRead = "";
        private void M_sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //if (OnReceive == null) return;
            SerialPort sp = (SerialPort)sender;
            string sRead = sp.ReadExisting();
            m_commLog.Add(CommLog.eType.Receive, sRead.Trim());
            m_nDataReceive = 0; 
            DataReceive(sRead, sender); 
        }

        int m_nDataReceive = 0;
        void DataReceive(string sRead, object sender)
        {
            m_nDataReceive++; 
            if ((m_sRead == "") && (p_eStartBit != eStartBit.None))
            {
                int nIndex = sRead.IndexOf(m_sStart);
                if (nIndex < 0)
                {
                    m_sRead = "";
                    return; 
                }
                nIndex += m_sStart.Length; 
                sRead = sRead.Substring(nIndex, sRead.Length - nIndex); 
            }
            string sReceive = DataRead(sRead, sender);
            if ((sReceive != "") && (sRead != sReceive))
            {
                m_sRead = sReceive; 
                DataReceive(sReceive, sender);
            }
        }

        string DataRead(string sRead, object sender)
        {
            int nIndex;
            try
            {
                if (p_eEndBit == eEndBit.None)
                {
                    if (OnReceive != null) OnReceive(sRead);
                    return "";
                }
                else if (p_eEndBit == eEndBit.LFCR_4CH)
                {
                    nIndex = sRead.IndexOf(m_sEnd);
                    if (nIndex < 0)
                    {
                        StopWatch sw = new StopWatch();
                        sw.Start();
                        while (nIndex < 0)
                        {
                            SerialPort sp = (SerialPort)sender;
                            string strSpare = sp.ReadExisting();
                            sRead = sRead + strSpare;
                            nIndex = sRead.IndexOf(m_sEnd);
                            if (sw.ElapsedMilliseconds > this.m_msReadTimeout && nIndex < 0)
                            {
                                sw.Stop();
                                return "";
                            }
                        }
                    }
                    if (OnReceive != null) OnReceive(sRead.Substring(0, nIndex));
                    return "";
                }
                nIndex = sRead.IndexOf(m_sEnd);
                if (nIndex < 0) return sRead;
                if (OnReceive != null) OnReceive(sRead.Substring(0, nIndex));
                nIndex += m_sEnd.Length;
                if (sRead.Length < nIndex) return "";
                else return sRead.Substring(nIndex, sRead.Length - nIndex);
            }
            catch (Exception) { return ""; }
        }
        #endregion

        #region Write
        static readonly object m_csLock = new object();
        StopWatch m_swWrite = new StopWatch();

        public string Send(string sMsg)
        {
            if (CheckConnect() == false) 
            {
                p_sInfo = "Can't Write : Check Connect";
                return p_sInfo; 
            }
            try
            {
                string sSend = m_sStart + sMsg + m_sEnd;
                if (p_eEndBit == eEndBit.LFCR_4CH)
                {
                    sSend = m_sStart + sMsg + "\r\n";
                }
                lock (m_csLock)
                {
                    while (m_swWrite.ElapsedMilliseconds < 10) Thread.Sleep(1);
                    m_commLog.Add(CommLog.eType.Send, sSend); 
                    m_sp.Write(sSend);
                    //char[] test = sSend.ToCharArray();
                    //m_sp.Write(test, 0, 9);
                    m_swWrite.Restart(); 
                }
                return "OK"; 
            }
            catch (Exception e) 
            { 
                p_sInfo = "Write Error : " + e.Message;
                return p_sInfo; 
            }
        }

        public string Send(byte[] aWrite, int nWrite, int nOffset = 0)
        {
            string sWrite = Encoding.ASCII.GetString(aWrite, nOffset, nWrite); 
            return Send(sWrite);
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
            RunProtocolTree(m_treeRoot.GetTree("Protocol"));
            RunTimeoutTree(m_treeRoot.GetTree("Timeout")); 
        }

        int m_msWriteTimeout = 2000;
        int m_msReadTimeout = 2000; 
        void RunTimeoutTree(Tree tree)
        {
            m_msReadTimeout = tree.Set(m_msReadTimeout, 2000, "Read", "Read Timeout (ms)");
            m_msWriteTimeout = tree.Set(m_msWriteTimeout, 2000, "Write", "Write Timeout (ms)");
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
        public RS232(string id, Log log)
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
