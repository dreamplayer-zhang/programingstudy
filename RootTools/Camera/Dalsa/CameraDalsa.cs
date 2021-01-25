using DALSA.SaperaLT.SapClassBasic;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Camera.Dalsa
{
    public class CameraDalsa : NotifyProperty, ICamera
    {
        public event EventHandler Grabed;

        #region Property
        public string p_id { get; set; }

        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value == "OK") return;
                m_log.Warn(value);
            }
        }

        public UserControl p_ui
        {
            get
            {
                CameraDalsa_UI ui = new CameraDalsa_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Connect
        public delegate void dgOnConnect();
        public event dgOnConnect OnConnect;

        BackgroundWorker m_bgwConnect = new BackgroundWorker();
        void InitConnect()
        {
            m_bgwConnect.DoWork += M_bgwConnect_DoWork;
            m_bgwConnect.RunWorkerCompleted += M_bgwConnect_RunWorkerCompleted;
            m_bgwConnect.RunWorkerAsync();
        }

        private void M_bgwConnect_DoWork(object sender, DoWorkEventArgs e)
        {
            p_sInfo = Connect();
            if (p_sInfo != "OK") SapDispose();
            OnPropertyChanged("p_bConnect");
        }

        SapLocation m_sapLocation = null;
        SapAcqDevice m_sapDevice = null;
        SapAcquisition m_sapAcq = null;
        SapBuffer m_sapBuf = null;
        public SapTransfer m_sapXfer = null;
        string Connect()
        {
            if (m_sServer == "") return "Server Name not Defined : " + p_id;
            if (m_sCamFile == "") return "Cam File Path not Defined : " + p_id;
            SapDispose(); 
            m_sapLocation = new SapLocation(m_sServer, 0);
            if (SapManager.GetResourceCount(m_sServer, SapManager.ResourceType.AcqDevice) <= 0) return "Server Location not Found : " + m_sServer;
            m_sapDevice = new SapAcqDevice(m_sapLocation);
            if (SapManager.GetResourceCount(m_sServer, SapManager.ResourceType.Acq) <= 0) return "Acqusition Device not Found : " + m_sServer;
            if (m_sapDevice == null) return "Acqusition Device not Assigned : " + m_sServer;
            if (m_sapDevice.Create() == false) return "Acqusition Device Create Error : " + p_id; 
            m_sapAcq = new SapAcquisition(m_sapLocation, m_sCamFile);
            if (m_sapAcq == null) return "SapAcqusition not Assigned + " + p_id;
            if (m_sapAcq.Create() == false) return "SapAcqusition Create Error : " + p_id;
            m_sapBuf = new SapBuffer(p_nCamBuf, m_sapAcq, SapBuffer.MemoryType.ScatterGather);
            if (m_sapBuf == null) return "SapBuffer Assign Error : " + p_id; 
            if (m_sapBuf.Create() == false) return "SapBuffer Create Error : " + p_id;
            GetCameraAddress();
            m_sapXfer = new SapAcqToBuf(m_sapAcq, m_sapBuf);
            if (m_sapXfer == null) return "Sap Transfer Assign Error : " + p_id;
            m_sapXfer.Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
            m_sapXfer.XferNotify += M_sapXfer_XferNotify;
            m_sapXfer.XferNotifyContext = this;
            if (m_sapXfer.Create() == false) return "Sap Transfer Create Error : " + p_id;
            ReadAOI(); 
            return "OK";
        }

        void SapDispose()
        {
            if (m_sapXfer != null)
            {
                m_sapXfer.Freeze();
                m_sapXfer.Destroy();
                m_sapXfer.Dispose();
                m_sapXfer = null;
            }
            if (m_sapBuf != null)
            {
                m_sapBuf.Destroy();
                m_sapBuf.Dispose();
                m_sapBuf = null;
            }
            if (m_sapAcq != null)
            {
                m_sapAcq.Destroy();
                m_sapAcq.Dispose();
                m_sapAcq = null; 
            }
            if (m_sapDevice != null)
            {
                m_sapDevice.Destroy();
                m_sapDevice.Dispose();
                m_sapDevice = null; 
            }
            if (m_sapLocation != null) 
            {
                m_sapLocation.Dispose();
                m_sapLocation = null; 
            }
        }

        private void M_bgwConnect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunTree(Tree.eMode.Init);
            if (OnConnect != null) OnConnect(); 
        }

        public bool p_bConnect
        {
            get { return (m_sapXfer != null); }
            set
            {
                if (p_bConnect == value) return;
                OnPropertyChanged(); 
                if (value)
                {
                    if (m_sapXfer != null) return; 
                    if (m_bgwConnect.IsBusy == false) m_bgwConnect.RunWorkerAsync();
                }
                else if (m_sapXfer != null) SapDispose();
            }
        }

        int _nCamBuf = 0; 
        public int p_nCamBuf
        {
            get { return _nCamBuf; }
            set
            {
                _nCamBuf = value;
                if (m_sapXfer != null) GetCameraAddress(); 
            }
        }
        int _nResourceCnt = 0;
        public int p_nResourceCnt
        {
            get
            {
                return _nResourceCnt;
            }
            set
            {
                _nResourceCnt = value;
            }
        }

        IntPtr[] m_aCamBuf = null; 
        void GetCameraAddress()
        {
            m_aCamBuf = new IntPtr[p_nCamBuf];
            for (int n = 0; n < p_nCamBuf; n++) m_sapBuf.GetAddress(n, out m_aCamBuf[n]); 
        }

        string m_sServer = "";
        string m_sCamFile = "";
        void RunTreeConnect(Tree tree)
        {
            m_sServer = tree.Set(m_sServer, m_sServer, "Server", "Camera Dalsa Server Name");
            m_sCamFile = tree.SetFile(m_sCamFile, m_sCamFile, "ccf", "Cam File", "Camera Dalsa Cam File");
            p_nCamBuf = tree.Set(p_nCamBuf, p_nCamBuf, "Buffer", "Camera Buffer Count");
        }
        #endregion

        #region AOI
        public int p_nByte { get; set; }
        public CPoint p_sz { get; set; }

        void ReadAOI()
        {
            int n = 0; 
            m_sapAcq.GetParameter(SapAcquisition.Prm.PIXEL_DEPTH, out n);
            p_nByte = n / 8;
            m_sapAcq.GetParameter(SapAcquisition.Prm.CROP_WIDTH, out n);
            p_sz.X = n;
            m_sapAcq.GetParameter(SapAcquisition.Prm.CROP_HEIGHT, out n);
            p_sz.Y = n; 
        }

        void RunTreeAOI(Tree tree)
        {
            if (m_sapXfer == null) return;
            tree.Set(p_nByte, p_nByte, "Pixel Byte", "Camera Pixel Depth (Byte)", true, true);
            tree.Set(p_sz, p_sz, "ROI", "Camera ROI Size", true, true);
        }
        #endregion

        #region MemoryData
        public MemoryTool m_memoryTool;
        public MemoryPool m_memoryPool = null;
        public string p_sMemoryPool
        {
            get { return (m_memoryPool != null) ? m_memoryPool.p_id : ""; }
            set
            {
                m_memoryPool = m_memoryTool.GetPool(value);
                OnPropertyChanged();
                if (m_memoryPool != null)
                {
                    foreach (string sGroup in m_memoryPool.m_asGroup)
                    {
                        if (p_sMemoryGroup == sGroup) return;
                    }
                }
                p_sMemoryGroup = "";
            }
        }

        public MemoryGroup m_memoryGroup = null;
        public string p_sMemoryGroup
        {
            get { return (m_memoryGroup != null) ? m_memoryGroup.p_id : ""; }
            set
            {
                if (m_memoryPool == null) return;
                m_memoryGroup = m_memoryPool.GetGroup(value);
                OnPropertyChanged();
                if (m_memoryGroup != null)
                {
                    foreach (string sMemory in m_memoryGroup.m_asMemory)
                    {
                        if (p_sMemoryData == sMemory) return;
                    }
                }
                p_sMemoryData = "";
            }
        }

        public MemoryData m_memoryData = null;
        public string p_sMemoryData
        {
            get { return (m_memoryData != null) ? m_memoryData.p_id : ""; }
            set
            {
                if (m_memoryGroup == null) return;
                m_memoryData = m_memoryGroup.GetMemory(value);
                OnPropertyChanged();
            }
        }

        public void SetMemoryData(MemoryData memoryData)
        {
            if (memoryData == null)
            {
                p_sMemoryData = "";
                return;
            }
            p_sMemoryPool = memoryData.m_group.m_pool.p_id;
            p_sMemoryGroup = memoryData.m_group.p_id;
            p_sMemoryData = memoryData.p_id;
            RunTree(Tree.eMode.Init);
        }

        void RunTreeMemory(Tree tree)
        {
            p_sMemoryPool = tree.Set(p_sMemoryPool, p_sMemoryPool, m_memoryTool.m_asPool, "Pool", "Memory Pool ID");
            if (m_memoryPool == null) return;
            p_sMemoryGroup = tree.Set(p_sMemoryGroup, p_sMemoryGroup, m_memoryPool.m_asGroup, "Group", "Memory Group ID");
            if (m_memoryGroup == null) return;
            p_sMemoryData = tree.Set(p_sMemoryData, p_sMemoryData, m_memoryGroup.m_asMemory, "Memory", "Memory Data ID");
        }
        #endregion

        #region Grab
        public bool p_bOnGrab { get { return m_sapXfer.Grabbing; } }

        bool m_bLive = false; 
        public void GrabLineScan(MemoryData memory, CPoint cp0, int nLine, int nScanOffsetY = 0, bool bInvY = false, int yInvOffset = 0)
        {
            SetMemoryData(memory);
            p_sInfo = StartGrab(cp0, nLine, bInvY, yInvOffset); 
        }

        public string StartGrab()
        {
            return StartGrab(m_cp0, m_nLine, m_bInvY, m_yInvOffset); 
        }

        public string StartGrab(CPoint cp0, int nLine, bool bInvY = false, int yInvOffset = 0)
        {
            if (EQ.p_bSimulate) return "OK"; 
            if (m_sapXfer == null) return "Camera not Connected";
            if (m_memoryData == null) return "MemoryData not Assigned";
            if (m_sapXfer.Grabbing) return "Camera is OnGrabbing";
            m_bLive = false;
            m_nXfer = 0; 
            p_nGrabProgress = 0;
            m_qGrab.Clear(); 
            m_cp0 = cp0;
            m_nLine = nLine;
            m_bInvY = bInvY;
            m_yInvOffset = yInvOffset;
            int nGrabCount = (int)(1.0 * nLine / p_sz.Y);
            m_sapBuf.Index = 0;
            m_sapXfer.Snap(nGrabCount);
            m_log.Info("m_sapXfer.Snap = " + nGrabCount.ToString());
            return "OK"; 
        }

        public string StartLive()
        {
            if (EQ.p_bSimulate) return "OK";
            if (m_sapXfer == null) return "Camera not Connected";
            if (m_memoryData == null) return "MemoryData not Assigned";
            if (m_sapXfer.Grabbing) return "Camera is OnGrabbing";
            m_bLive = true;
            m_qGrab.Clear();
            m_sapXfer.Grab();
            return "OK";
        }

        public string StopGrab()
        {
            m_sapXfer.Freeze();
            return "OK";
        }
        public CPoint GetRoiSize()
        {
            return null;
        }

        Queue<int> m_qGrab = new Queue<int>();
        int m_nXfer = 0; 
        private void M_sapXfer_XferNotify(object sender, SapXferNotifyEventArgs args)
        {
            //for (int n = 0; n < args.EventCount; n++)
            {
                m_qGrab.Enqueue(m_nXfer);
                m_log.Info("Xfer = " + m_nXfer.ToString()); 
                m_nXfer++;
            }
        }

        CPoint m_cp0 = new CPoint();
        int m_nLine = 1024;
        bool m_bInvY = false;
        int m_yInvOffset = 0; 
        void RunTreeGrab(Tree tree)
        {
            m_cp0 = tree.Set(m_cp0, m_cp0, "Start Pos", "Start Pixel Position at MemoryData");
            m_nLine = tree.Set(m_nLine, m_nLine, "Lines", "Image Y Size (pixel)"); 
            m_bInvY = tree.Set(m_bInvY, m_bInvY, "Inverse Y", "Inverse Y when Camera Buffer -> MemoryData");
            m_yInvOffset = tree.Set(m_yInvOffset, m_yInvOffset, "Inverse Y Offset", "Image Y Offset at MemoryData When Inverse Y", m_bInvY); 
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread;
        void RunThread()
        {
            m_bThread = true;
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(5);
                while (m_qGrab.Count > 0)
                {
                    if (m_bLive) p_sInfo = ThreadLive();
                    else p_sInfo = ThreadGrab();
                }
            }
        }

        int _nGrabProgress = 0;
        public int p_nGrabProgress
        {
            get { return _nGrabProgress; }
            set
            {
                if (_nGrabProgress == value) return;
                _nGrabProgress = value;
                OnPropertyChanged();
            }
        }

        string ThreadGrab()
        {
            int iGrab = m_qGrab.Dequeue();
            int y0 = m_cp0.Y + iGrab * p_sz.Y;
            if (m_bInvY) y0 = m_nLine - y0 + m_yInvOffset;
            p_sInfo = CopyBuf(iGrab, y0, m_bInvY ? -1 : 1); 
            p_nGrabProgress++;
            return p_sInfo;
        }

        string ThreadLive()
        {
            while (m_qGrab.Count > 1) m_qGrab.Dequeue();
            int iGrab = m_qGrab.Dequeue();
            p_sInfo = CopyBuf(iGrab, m_cp0.Y, 1);
            return p_sInfo;
        }

        unsafe string CopyBuf(int iGrab, int y0, int dMem)
        {
            int wCam = p_nByte * p_sz.X;
            int wMem = m_memoryData.p_nByte * m_memoryData.p_sz.X;
            dMem *= wMem; 
            IntPtr pSrc = m_aCamBuf[(iGrab + 1) % p_nCamBuf];
            IntPtr pDst = m_memoryData.GetPtr(0, m_cp0.X, y0); 
            for (int y = 0; y < p_sz.Y; y++, pSrc += wCam, pDst += dMem)
            {
                Buffer.MemoryCopy(pSrc.ToPointer(), pDst.ToPointer(), wCam, wCam); 
            }
            //forget MemoryData Invalid View
            return "OK"; 
        }
        #endregion

        #region Tree
        public TreeRoot p_treeRoot { get; set; }
        private void P_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            p_treeRoot.p_eMode = mode;
            bool bOpen = (m_sapXfer != null);
            RunTreeConnect(p_treeRoot.GetTree("Connect", false));
            RunTreeMemory(p_treeRoot.GetTree("Memory", false));
            RunTreeGrab(p_treeRoot.GetTree("Start Grab", true, bOpen));
            RunTreeAOI(p_treeRoot.GetTree("AOI", false, bOpen));
        }
        #endregion

        Log m_log;
        public CameraDalsa(string id, Log log, MemoryTool memoryTool)
        {
            p_id = id;
            m_log = log;
            m_memoryTool = memoryTool;

            p_treeRoot = new TreeRoot(id, m_log);
            RunTree(Tree.eMode.RegRead);
            p_treeRoot.UpdateTree += P_treeRoot_UpdateTree;

            p_nByte = 1;
            p_sz = new CPoint(1024, 1024); 

            InitConnect();

            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_qGrab.Clear();
                EQ.p_bStop = true;
                m_thread.Join();
            }
            SapDispose();
        }

        public double GetFps() { return 0; }

        public void GrabLineScanColor(MemoryData memory, CPoint cpScanOffset, int nLine, int nScanOffsetY = 0, bool bInvY = false, int ReserveOffsetY = 0) { }
    }
}
