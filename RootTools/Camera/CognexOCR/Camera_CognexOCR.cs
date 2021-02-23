using RootTools.Comm;
using RootTools.Memory;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using RootTools.Trees;
using System.Windows.Media.Imaging;

namespace RootTools.Camera.CognexOCR
{
    public class Camera_CognexOCR : ObservableObject, ICamera
    {
        public event System.EventHandler Grabed;
        void _Dummy()
        {
            if (Grabed != null) Grabed(null, null);
        }

        #region Property
        public string p_id { get; set; }
        public bool bStopThread
        {
            get;
            set;
        }

        string _sInfo = "Info";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                RaisePropertyChanged(); 
                if (value == "OK") return;
                p_sOCR = "Error";
                p_fScore = 0; 
                m_tcpip.m_commLog.Add(CommLog.eType.Info, value); 
                m_log.Warn(value);
            }
        }

        public int p_nGrabProgress
        {
            get { return 0; }
            set { }
        }

        string _sOCR = "OCR"; 
        public string p_sOCR
        {
            get { return _sOCR; }
            set
            {
                _sOCR = value;
                RaisePropertyChanged(); 
            }
        }

        double _fScore = 0; 
        public double p_fScore
        { 
            get { return _fScore; }
            set
            {
                _fScore = value;
                RaisePropertyChanged(); 
            }
        }
        #endregion


        #region Read OCR
        public string ReadOCR(ref string sID, ref double dSocre)
        {
            p_sInfo = SendReadOCR();
            if (p_sInfo != "OK") return p_sInfo;
            p_sInfo = WaitReply(m_secWaitReply);
            if (p_sInfo == "OK")
            {
                sID = p_sOCR;
                dSocre = p_fScore;
            }
            return p_sInfo; 
        }
        #endregion

        #region TCPIP
        private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            try
            {
                string sMsg = Encoding.Default.GetString(aBuf, 0, nSize);
                sMsg = sMsg.Replace("\r\n", "");
                m_log.Info("<--  " + sMsg);
                string[] sMsgs = sMsg.Split(',');
                p_sOCR = sMsgs[0];
                p_fScore = (sMsgs.Length > 1) ? Convert.ToDouble(sMsgs[1]) : 0;
            }
            finally { m_bSendCmd = false; }
        }

        public string SendReadOCR()
        {
            return SendCmd(m_sReadCmd);
        }

        bool m_bSendCmd = false; 
        string SendCmd(string sCmd)
        {
            m_log.Info(" --> " + sCmd);
            sCmd += (char)0x0d;
            sCmd += (char)0x0a;
            p_sInfo = m_tcpip.Send(sCmd);
            m_bSendCmd = (p_sInfo == "OK"); 
            return p_sInfo;
        }

        string WaitReply(double secWaitReply)
        {
            int msWaitReply = (int)(1000 * secWaitReply); 
            StopWatch sw = new StopWatch();
            try
            {
                while (sw.ElapsedMilliseconds < msWaitReply)
                {
                    if (m_bSendCmd == false) return "OK"; 
                    Thread.Sleep(10);
                }
                return "Wait Reply Timeout";
            }
            finally
            {
                m_bSendCmd = false;
            }
        }

        string m_sReadCmd = "READ(-1)";
        double m_secWaitReply = 1;
        void RunTreeOCR(Tree tree)
        {
            m_sReadCmd = tree.Set(m_sReadCmd, m_sReadCmd, "Read Cmd", "Cognex Read Command");
            m_secWaitReply = tree.Set(m_secWaitReply, m_secWaitReply, "Wait Reply", "Wait Reply Time (sec)"); 
        }
        #endregion

        #region FTP
        public BitmapImage ReadImage()
        {
            try
            {
                WebClient ftp = new WebClient();
                ftp.Credentials = new NetworkCredential(m_sUserName, m_sPassword);

                //Thread.Sleep(100);
                //BitmapImage image = new BitmapImage(new Uri("ftp://" + m_tcpip.p_sIP + "/image.bmp"));

                Thread.Sleep(1000);
                ftp.DownloadFile("ftp://" + m_tcpip.p_sIP + "/image.bmp", "c:\\Log\\CognexOCR.bmp");
                Thread.Sleep(100);
                BitmapImage image = new BitmapImage(new Uri("c:\\Log\\CognexOCR.bmp")); 

                return image;
            }
            catch (Exception e) 
            {
                p_sInfo = "Read Image Error : " + e.Message;
                EQ.p_eState = EQ.eState.Error;
                EQ.p_bStop = true;
            }
            return null; 
        }

        string m_sUserName = "admin"; 
        string m_sPassword = "";
        void RunTreeFTP(Tree tree)
        {
            m_sUserName = tree.Set(m_sUserName, m_sUserName, "UserName", "FTP UserName");
            m_sPassword = tree.Set(m_sPassword, m_sPassword, "Password", "FTP Password"); 
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                Camera_CognexOCR_UI ui = new Camera_CognexOCR_UI();
                ui.Init(this);
                return (UserControl)ui;
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
            RunTree(m_treeRoot);
        }

        void RunTree(Tree treeRoot)
        {
            RunTreeOCR(treeRoot.GetTree("OCR")); 
            RunTreeFTP(treeRoot.GetTree("FTP"));
        }
        #endregion

        Log m_log;
        public TCPIPClient m_tcpip;
        TreeRoot m_treeRoot = null;
        public TreeRoot p_treeRoot
        {
            get
            {
                return m_treeRoot;
            }
            set
            {
                SetProperty(ref m_treeRoot, value);
            }
        }
        public Camera_CognexOCR(string id, Log log)
        {
            p_id = id;
            m_log = log;

            m_tcpip = new TCPIPClient(id, log);
            m_tcpip.EventReciveData += M_tcpip_EventReciveData;

            m_treeRoot = new TreeRoot(id, log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree; 
            RunTree(Tree.eMode.RegRead);

        }

        public void ThreadStop()
        {
            m_tcpip.ThreadStop(); 
        }

        public CPoint p_sz
        {
            get { return null; }
            set { }
        }

        public CPoint GetRoiSize()
        {
            return null;
        }
        public double GetFps() { return 0; }
        public void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null, bool bTest = false) { }
        public string StopGrab()
        {
            return "FALSE";
        }
        public void GrabLineScanColor(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null) { }
    }
}
