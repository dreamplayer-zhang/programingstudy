using System.Windows.Controls;
using DALSA.SaperaLT.SapClassBasic;
using System.ComponentModel;
using System.Windows;
using System;
using RootTools.Memory;
using System.Threading;
using RootTools.Trees;
using System.Windows.Data;
using System.Threading.Tasks;
using RootTools_CLR;
using System.Collections.Generic;
using System.Diagnostics;

namespace RootTools.Camera.Dalsa
{
    public class Camera_Dalsa : ObservableObject, ICamera
    {
        public event EventHandler Grabed;

        enum ResourceIdx
        {
            Gray=0,
            Color,
        }
        #region Property
        public string p_id { get; set; }
        public bool bStopThread
        {
            get;
            set;
        }
        int m_nGrabProgress = 0;
        public int p_nGrabProgress
        {
            get
            {
                return m_nGrabProgress;
            }
            set
            {
                SetProperty(ref m_nGrabProgress, value);
            }
        }


        string _sInfo = "";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                if (value == "OK") return;
                //                AddCommLog(Brushes.Red, value);
                m_log.Warn(value);
            }
        }
        ImageViewer_ViewModel m_ImageViewer;
        public ImageViewer_ViewModel p_ImageViewer
        {
            get
            {
                return m_ImageViewer;
            }
            set
            {
                SetProperty(ref m_ImageViewer, value);
            }
        }
        DalseCamInfo m_CamInfo;
        public DalseCamInfo p_CamInfo
        {
            get
            {
                return m_CamInfo;
            }
            set
            {
                SetProperty(ref m_CamInfo, value);
            }
        }
        DalsaParameterSet m_CamParam;
        public DalsaParameterSet p_CamParam
        {
            get
            {
                return m_CamParam;
            }
            set
            {
                SetProperty(ref m_CamParam, value);
            }
        }
        const int c_nBuf = 2000;
        int _nBuf = 2000;
        public int p_nBuf
        {
            get
            {
                return _nBuf;
            }
            set
            {
                SetProperty(ref _nBuf, value);
                _nBuf = (value > c_nBuf) ? c_nBuf : value;
            }
        }
        int m_nGrabTrigger = 0;
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                Camera_Dalsa_UI ui = new Camera_Dalsa_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        bool Scandir;
        Log m_log;
        public SapAcquisition m_sapAcq = null;
        public SapBuffer m_sapBuf = null;
        public SapTransfer m_sapXfer = null;
        public SapAcqDevice m_sapDevice = null;
        BackgroundWorker bgw_Connect = new BackgroundWorker();
        BackgroundWorker bgw_AreaConnect = new BackgroundWorker();

        ImageData m_ImageLive;
        CPoint m_cpScanOffset = new CPoint();
        int m_nInverseYOffset = 0;
        int m_nGrabCount = 0;
        MemoryData m_Memory;
        IntPtr m_MemPtr = IntPtr.Zero;
        IntPtr m_RedMemPtr = IntPtr.Zero;
        IntPtr m_GreenMemPtr = IntPtr.Zero;
        IntPtr m_BlueMemPtr = IntPtr.Zero;
        IntPtr[] m_pSapBuf;
        int m_nLine = 0;
        TreeRoot m_treeRoot = null;

        double m_dPReXScale = 1; //Gray
        double m_dPReXScaleR = 1;
        double m_dPReXScaleG = 1;
        double m_dPReXScaleB = 1;
        double m_dPReXShiftR = 0;
        double m_dPReXShiftG = 0;
        double m_dPReXShiftB = 0;

        int m_nPreWidth = 8000;
        int m_nPreWidthR = 8000;
        int m_nPreWidthG = 8000;
        int m_nPreWidthB = 8000;

        public bool m_bGrabThreadOn = false;   // true When thread is arrived in Grab Loop
        const int thread = 12;
        const int threadBuff = 16000;
        CLR_IP m_clrip = new CLR_IP();

        List<byte[]> m_buff = new List<byte[]>();
        List<byte[]> m_buffR = new List<byte[]>();
        List<byte[]> m_buffG = new List<byte[]>();
        List<byte[]> m_buffB = new List<byte[]>();
        const int overlapsize = 100;

        List<byte[]> m_Overlap = new List<byte[]>();
        List<byte[]> m_OverlapR = new List<byte[]>();
        List<byte[]> m_OverlapG = new List<byte[]>();
        List<byte[]> m_OverlapB = new List<byte[]>();
        List<bool> ThreadState = new List<bool>();
        GrabData m_GD = new GrabData();

        double[] m_pdOverlap = new double[overlapsize];
        int m_nPreOverlap = 100;
        bool enableCameraconfig = false;
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

        public Camera_Dalsa(string id, Log log)
        {
            p_id = id;
            m_log = log;
            p_treeRoot = new TreeRoot(id, m_log);
            bgw_AreaConnect.DoWork += bgw_AreaConnect_DoWork;
            bgw_AreaConnect.RunWorkerCompleted += Bgw_AreaConnect_RunWorkerCompleted;
            bgw_Connect.DoWork += bgw_Connect_DoWork;
            bgw_Connect.RunWorkerCompleted += bgw_Connect_RunWorkerCompleted;
            m_ImageLive = new ImageData(640, 480);
            p_ImageViewer = new ImageViewer_ViewModel(m_ImageLive);
            p_CamParam = new DalsaParameterSet(m_log);
            p_CamInfo = new DalseCamInfo(m_log);
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
            p_treeRoot.UpdateTree += M_treeRoot_UpdateTree;

            for (int n = 0; n < thread; n++)
            {
                m_buff.Add(new byte[threadBuff]);
                m_buffR.Add(new byte[threadBuff]);
                m_buffG.Add(new byte[threadBuff]);
                m_buffB.Add(new byte[threadBuff]);
                m_Overlap.Add(new byte[overlapsize]);
                m_OverlapR.Add(new byte[overlapsize]);
                m_OverlapG.Add(new byte[overlapsize]);
                m_OverlapB.Add(new byte[overlapsize]);
                ThreadState.Add(true);
            }
            // init
            double dRatio = 1.0 / m_nPreOverlap;
            for (int n = 0; n < m_nPreOverlap; n++)
            {
                m_pdOverlap[n] = dRatio * (double)n;
            }
            m_clrip.Cpp_CreatInterpolationData(0, m_dPReXScaleR, m_dPReXShiftR, m_nPreWidthR);
            m_clrip.Cpp_CreatInterpolationData(1, m_dPReXScaleG, m_dPReXShiftG, m_nPreWidthG);
            m_clrip.Cpp_CreatInterpolationData(2, m_dPReXScaleB, m_dPReXShiftB, m_nPreWidthB);
        }

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.Init);
            RunTree(Tree.eMode.RegWrite);
        }
        public void RunTree(Tree.eMode mode)
        {
            p_treeRoot.p_eMode = mode;
            RunTree(p_treeRoot);
        }
        void RunTree(Tree treeRoot)
        {
            RunSetTree(treeRoot.GetTree("Connect Set"));
            RunImageRoiTree(treeRoot.GetTree("Buffer Image ROI", true, m_sapXfer != null));
            if(enableCameraconfig)
                RunCameraConfig(treeRoot.GetTree("Camera Config", true, m_sapXfer != null), m_sapXfer != null);
        }

        void RunSetTree(Tree tree)
        {
            p_CamInfo.p_sServer = tree.Set(p_CamInfo.p_sServer, "Dalsa", "Server", "Cam Server Name");
            p_CamInfo.p_sFile = tree.SetFile(p_CamInfo.p_sFile, p_CamInfo.p_sFile, "ccf", "CamFile", "Cam File");
            p_CamInfo.p_sAreaCamFile = tree.SetFile(p_CamInfo.p_sAreaCamFile, p_CamInfo.p_sAreaCamFile, "ccf", "Area Cam file", "Area Cam File");
            p_CamInfo.p_nResourceIdx = tree.Set(p_CamInfo.p_nResourceIdx, p_CamInfo.p_nResourceIdx, "Resource Count", "Resource Count");
            enableCameraconfig = tree.Set(enableCameraconfig, enableCameraconfig, "Use Camera Config", "Use Camera Config");
        }

        void RunImageRoiTree(Tree tree)
        {
            if (m_sapXfer == null)
            {
                tree.HideAllItem();
                return;
            }

            p_CamParam.p_Width = tree.Set(p_CamParam.p_Width, 100, "Image Width", "Buffer Image Width");
            p_CamParam.p_Height = tree.Set(p_CamParam.p_Height, 100, "Image Height", "Buffer Image Height");
            p_CamParam.p_eDeviceScanType = (DalsaParameterSet.eDeviceScanType)tree.Set(p_CamParam.p_eDeviceScanType, p_CamParam.p_eDeviceScanType, "Device Scan Type", "Device Scan Type");
            p_CamParam.p_eTriggerMode = (DalsaParameterSet.eTriggerMode)tree.Set(p_CamParam.p_eTriggerMode, p_CamParam.p_eTriggerMode, "Trigger Mode", "Trigger Mode");
        }

        void RunCameraConfig(Tree tree, bool bVisible)
        {
            p_CamParam.p_eUserSetPowerup = (DalsaParameterSet.eUserSet)tree.Set(p_CamParam.p_eUserSetPowerup, p_CamParam.p_eUserSetPowerup, "Power-up UserSet", "Selects the UserSet Configuration Set on camera power-up or reset", bVisible);
            p_CamParam.p_eUserSetCurrent = (DalsaParameterSet.eUserSet)tree.Set(p_CamParam.p_eUserSetCurrent, p_CamParam.p_eUserSetCurrent, "Current UserSet", "Selects and Current UserSet", bVisible);
            //p_CamParam.p_nRotaryEncoderMultiplier = tree.Set(p_CamParam.p_nRotaryEncoderMultiplier, p_CamParam.p_nRotaryEncoderMultiplier, "RotaryEncoderMultiplier", "Specifies a multiplication factor for the rotary encoder output pulse generator", bVisible);
            //p_CamParam.p_nRotaryEncoderDivider = tree.Set(p_CamParam.p_nRotaryEncoderDivider, p_CamParam.p_nRotaryEncoderDivider, "RotaryEncoderDivider", "Specifies a division factor for the rotary encoder output pulse generator", bVisible);
        }

        #endregion 

        void bgw_Connect_DoWork(object sender, DoWorkEventArgs e)
        {
            ConnectCammera();
        }

        void bgw_Connect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunTree(Tree.eMode.Init);
        }
        void bgw_AreaConnect_DoWork(object sender, DoWorkEventArgs e)
        {
            GrabArea();
        }
        private void Bgw_AreaConnect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunTree(Tree.eMode.Init);
        }
        public void ThreadStop()
        {
            //DestroysObjects("Camera Closed -  ThreadStop");
        }

        public void Connect()
        {
            if (!bgw_Connect.IsBusy && p_CamInfo.p_eState == eCamState.Init)
                bgw_Connect.RunWorkerAsync();
        }

        void ConnectCammera()
        {
            if (p_CamInfo.p_sServer == "")
            {
                p_sInfo = p_id + "Empty Server Name";
                return;
            }
            if (p_CamInfo.p_sFile == "")
            {
                p_sInfo = p_id + "Empty Config File Name";
                return;
            }

            SapLocation loc = new SapLocation(p_CamInfo.p_sServer, p_CamInfo.p_nResourceIdx);
            SapManager.GetServerName(loc);

            if (SapManager.GetResourceCount(p_CamInfo.p_sServer, SapManager.ResourceType.Acq) > 0)          //"Acq" (frame-grabber) "AcqDevice" (camera)
            {
                m_sapAcq = new SapAcquisition(loc, p_CamInfo.p_sFile);

                if (!m_sapAcq.Create())
                {
                    loc.Dispose();
                    p_sInfo = DestroysObjects(p_id + "Error during SapAcquisition creation");
                    return;
                }

                SapFormat bufformat = m_sapAcq.XferParams.Format;

                if (bufformat == SapFormat.RGB8888 || bufformat == SapFormat.RGBP8 || bufformat == SapFormat.Mono16 || bufformat == SapFormat.RGB888)
                    m_sapBuf = new SapBuffer(p_nBuf, m_sapAcq.XferParams.Width, m_sapAcq.XferParams.Height, bufformat, SapBuffer.MemoryType.ScatterGather);
                else if (bufformat == SapFormat.Mono8)
                    m_sapBuf = new SapBuffer(p_nBuf, m_sapAcq, SapBuffer.MemoryType.ScatterGather);

                m_sapXfer = new SapAcqToBuf(m_sapAcq, m_sapBuf);
            }

            SapLocation loc2 = new SapLocation(p_CamInfo.p_sServer, 0);
            if (SapManager.GetResourceCount(p_CamInfo.p_sServer, SapManager.ResourceType.AcqDevice) > 0)
                m_sapDevice = new SapAcqDevice(loc2, false);

            if (m_sapXfer == null)
                return;

            m_sapXfer.Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
            m_sapXfer.XferNotify += new SapXferNotifyHandler(xfer_XferNotify);
            m_sapXfer.XferNotifyContext = this;

            if (m_sapDevice != null && !m_sapDevice.Create())
            {
                p_sInfo = DestroysObjects(p_id + "Error during SapDevice creation");
                return;
            }

            if (!m_sapBuf.Create())
            {
                p_sInfo = DestroysObjects(p_id + "Error during SapBuffer creation");
                return;
            }

            if (!m_sapXfer.Create())
            {
                p_sInfo = DestroysObjects(p_id + "Error during SapTransfer creation");
                return;
            }
            loc2.Dispose();
            loc.Dispose();
            p_CamParam.SetCamHandle(m_sapDevice, m_sapAcq);
            p_CamInfo.p_eState = eCamState.Ready;
            p_CamParam.ReadParamter();
            m_log.Info(p_id + "Connect Success");
        }

        public void ConnectAsSlave()
        {
            if (p_CamInfo.p_sServer == "")
            {
                p_sInfo = p_id + "Empty Server Name";
                return;
            }
            if (p_CamInfo.p_sFile == "")
            {
                p_sInfo = p_id + "Empty Config File Name";
                return;
            }

            SapLocation loc = new SapLocation(p_CamInfo.p_sServer, p_CamInfo.p_nResourceIdx);
            m_sapAcq = new SapAcquisition(loc, p_CamInfo.p_sFile);

            if (!m_sapAcq.Create())
            {
                loc.Dispose();
                p_sInfo = DestroysObjects(p_id + "Error during SapAcquisition creation");
                return;
            }

            SapFormat bufformat = m_sapAcq.XferParams.Format;

            if (bufformat == SapFormat.RGB8888 || bufformat == SapFormat.RGBP8 || bufformat == SapFormat.Mono16)
                m_sapBuf = new SapBuffer(p_nBuf, m_sapAcq.XferParams.Width, m_sapAcq.XferParams.Height, bufformat, SapBuffer.MemoryType.ScatterGather);
            else if (bufformat == SapFormat.Mono8)
                m_sapBuf = new SapBuffer(p_nBuf, m_sapAcq, SapBuffer.MemoryType.ScatterGather);

            m_sapXfer = new SapAcqToBuf(m_sapAcq, m_sapBuf);

            m_sapXfer.Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
            m_sapXfer.XferNotify += new SapXferNotifyHandler(xfer_XferNotify);
            m_sapXfer.XferNotifyContext = this;

            if (!m_sapBuf.Create())
            {
                p_sInfo = DestroysObjects(p_id + "Error during SapBuffer creation");
                return;
            }

            if (!m_sapXfer.Create())
            {
                p_sInfo = DestroysObjects(p_id + "Error during SapTransfer creation");
                return;
            }

            p_CamParam.SetCamHandle(m_sapDevice, m_sapAcq);
            p_CamInfo.p_eState = eCamState.Ready;
            p_CamParam.ReadParamter();
            m_log.Info(p_id + "Connect Success");
        }

        string DestroysObjects(string sLog)
        {
            p_CamInfo.p_eState = eCamState.Init;
            if (sLog != null)
                m_log.Error(sLog);
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
            if (m_sapDevice != null)
            {
                m_sapDevice.Destroy();
                m_sapDevice.Dispose();
                m_sapDevice = null;
            }
            if (m_sapAcq != null)
            {
                m_sapAcq.Destroy();
                m_sapAcq.Dispose();
                m_sapAcq = null;
            }
            p_CamParam.DisconnectCamHandle();

            RunTree(Tree.eMode.Update);

            return sLog;
        }

        Thread m_GrabThread;

        public CPoint GetRoiSize()
        {
            return new CPoint(m_CamParam.p_Width, m_CamParam.p_Height);
        }

        public CPoint p_sz
        {
            get { return new CPoint(m_CamParam.p_Width, m_CamParam.p_Height); }
            set { }
        }

        public string StopGrab()
        {
            bStopThread = true;
            //m_nGrabCount = 0;
            //p_CamParam.p_eTDIMode = DalsaParameterSet.eTDIMode.Tdi;
            p_CamParam.p_eDeviceScanType = DalsaParameterSet.eDeviceScanType.Linescan;
            p_CamParam.p_eTriggerMode = DalsaParameterSet.eTriggerMode.External;
            if (m_sapDevice != null)
                m_sapDevice.UpdateFeaturesToDevice();
            m_sapXfer.Freeze();
            return "OK";
        }

        public int GetGrabProgress()
        {
            return Convert.ToInt32((double)m_nGrabTrigger * 100 / m_nGrabCount);
        }

        public void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData grabData = null, bool bTest = false)
        {
            if (EQ.p_bSimulate)
            {
                p_sInfo = "OK";
                return;
            }
            if (p_CamInfo.p_eState != eCamState.Ready && p_CamInfo.p_eState != eCamState.Done)
            {
                p_sInfo = p_id + " State not Ready : " + p_CamInfo.p_eState.ToString();
                return;
            }
            //if (memory.p_nByte != 1)
            //{
            //    p_sInfo = p_id + " MemoryData Byte not 1";
            //    return;
            //}
            m_GD = grabData != null ? grabData : m_GD;
            m_Memory = memory;
            m_MemPtr = memory.GetPtr();

            Scandir = grabData.bInvY;
            bStopThread = false;

            if (m_sapBuf.BytesPerPixel > 1)
            {
                m_RedMemPtr = m_Memory.GetPtr(0);
                m_GreenMemPtr = m_Memory.GetPtr(1);
                m_BlueMemPtr = m_Memory.GetPtr(2);
            }

            if (enableCameraconfig)
                m_CamParam.p_eUserSetCurrent = (DalsaParameterSet.eUserSet)m_GD.nUserSet;

            if (Scandir == true) //ybkwon0113
            {
                //p_CamParam.SetGrabDirection(DalsaParameterSet.eDir.Forward);
            }
            else
            {
                //p_CamParam.SetGrabDirection(DalsaParameterSet.eDir.Reverse);
            }

            m_cpScanOffset = cpScanOffset;
            m_nInverseYOffset = grabData.ReverseOffsetY;
            m_nGrabCount = (int)Math.Truncate(1.0 * nLine / p_CamParam.p_Height) - 1;
            m_nLine = nLine;
            m_pSapBuf = new IntPtr[p_nBuf];
            for (int n = 0; n < p_nBuf; n++)
                m_sapBuf.GetAddress(n, out m_pSapBuf[n]);

            //m_sapBuf.Clear();
            //m_iBlock = -1;
            m_sapBuf.Index = (int)(0);
            m_nGrabTrigger = 0;
            m_sapXfer.Snap((int)(m_nGrabCount));

            p_CamInfo.p_eState = eCamState.GrabMem;
            m_nOffsetTest = grabData.nScanOffsetY;

            if (bTest)
                m_GrabThread = new Thread(new ThreadStart(RunGrabLineColorScanThread2));
            else
            {
                if (m_sapBuf.BytesPerPixel == 1 || m_sapBuf.BytesPerPixel == 2)
                    m_GrabThread = new Thread(new ThreadStart(RunGrabLineScanThread));
                else
                    m_GrabThread = new Thread(new ThreadStart(RunGrabLineColorScanThread));
            }

            m_GrabThread.Start();
        }
        public void GrabLineScanColor(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData grabData = null)
        {
            if (EQ.p_bSimulate)
            {
                p_sInfo = "OK";
                return;
            }
            if (p_CamInfo.p_eState != eCamState.Ready && p_CamInfo.p_eState != eCamState.Done)
            {
                p_sInfo = p_id + " State not Ready : " + p_CamInfo.p_eState.ToString();
                return;
            }


            Scandir = grabData.bInvY;
            m_Memory = memory;
            m_MemPtr = memory.GetPtr();
            m_RedMemPtr = m_Memory.GetPtr(0);
            m_GreenMemPtr = m_Memory.GetPtr(1);
            m_BlueMemPtr = m_Memory.GetPtr(2);

            m_cpScanOffset = cpScanOffset;
            m_nInverseYOffset = grabData.ReverseOffsetY;
            m_nGrabCount = (int)Math.Truncate(1.0 * nLine / p_CamParam.p_Height) - 1;

            m_pSapBuf = new IntPtr[p_nBuf];
            for (int n = 0; n < p_nBuf; n++)
                m_sapBuf.GetAddress(n, out m_pSapBuf[n]);

            m_sapBuf.Index = 0;
            m_nGrabTrigger = 0;
            m_sapXfer.Snap(m_nGrabCount);
            p_CamInfo.p_eState = eCamState.GrabMem;

            m_nOffsetTest = 0;
            m_GrabThread = new Thread(new ThreadStart(RunGrabLineColorScanThread));
            m_GrabThread.Start();
        }
        int m_nOffsetTest = 0;
        private CRect m_LastROI = new CRect(0, 0, 0, 0);
        unsafe void RunGrabLineScanOverlapThread()
        {
            StopWatch swGrab = new StopWatch();
            int DelayGrab = 1000 * m_nGrabCount;

            if (m_sapBuf == null)
                p_sInfo = "CamDalsa Buffer Error !!";

            int iBlock = 0;
            int nByteCnt = m_sapBuf.BytesPerPixel;
            int nScanOffsetX = m_cpScanOffset.X;
            int nScanOffsetY = m_cpScanOffset.Y;
            int nCamWidth = p_CamParam.p_Width;
            int nCamHeight = p_CamParam.p_Height;
            int nFovStart = m_GD.m_nFovStart;
            int nOverlap = m_GD.m_nOverlap;

            if (nOverlap > overlapsize)
            {
                MessageBox.Show("Overlap 이 100 보다큽니다.");
                nOverlap = overlapsize;
            }
            int nFovSize = (int)(m_GD.m_nFovSize / m_GD.m_dScale) + nOverlap;

            if (nCamWidth < nFovStart + nFovSize)
            {
                MessageBox.Show("FovStart+ nFovSize+ nOverlap가 CamWidth보다 큽니다.(" + nFovStart.ToString() + " + " + nFovSize.ToString() + " > " + nCamWidth.ToString() + ")");
                nFovSize = nCamWidth - nFovStart;
            }
            int nBufSize = nCamHeight * nCamWidth;
            long nMemWidth = m_Memory.W;

            if (m_GD.m_nOverlap != m_nPreOverlap)
            {
                m_nPreOverlap = m_GD.m_nOverlap;
                if (m_nPreOverlap != 0)
                {
                    double dRatio = 1.0 / m_nPreOverlap;
                    for (int n = 0; n < m_nPreOverlap; n++)
                    {
                        m_pdOverlap[n] = dRatio * n;
                    }
                }
            }

            if (m_GD.m_dScale != m_dPReXScale
                || nFovSize != m_nPreWidth)
            {
                m_dPReXScale = m_GD.m_dScale;
                m_nPreWidth = nFovSize;
                m_clrip.Cpp_CreatInterpolationData(0, m_dPReXScale, 0, m_nPreWidth);
            }

            const int nTimeOut_10s = 10000; //ms            
            const int nTimeOutInterval = 10; // ms
            int nScanAxisTimeOut = nTimeOut_10s / nTimeOutInterval;
            int previBlock = 0;
            while (iBlock < m_nGrabCount)
            {
                if (previBlock == iBlock)
                {
                    Thread.Sleep(nTimeOutInterval);
                    if (--nScanAxisTimeOut <= 0)
                    {
                        m_log.Info("TimeOut - RunGrabLineScanOverlapThread");
                        m_nGrabTrigger = m_nGrabCount;
                    }
                }
                else
                {
                    previBlock = iBlock;
                    nScanAxisTimeOut = nTimeOut_10s / nTimeOutInterval;
                }
                if (iBlock < m_nGrabTrigger)
                {
                    IntPtr ipSrc = m_pSapBuf[iBlock % p_nBuf];

                    Parallel.For(0, nCamHeight, new ParallelOptions { MaxDegreeOfParallelism = thread }, (y) =>
                    {
                        int yp;
                        if (Scandir)
                            yp = m_nLine - (y + iBlock * nCamHeight) + m_nInverseYOffset + m_nOffsetTest;
                        else
                            yp = y + iBlock * nCamHeight + nScanOffsetY + m_nOffsetTest;


                        long n = nScanOffsetX + yp * nMemWidth;
                        IntPtr srcPtr = ipSrc + nCamWidth * y * nByteCnt + nFovStart;
                        IntPtr dstPtr = (IntPtr)((long)m_MemPtr + n);
                        int nThreadIdx = GetReadyThread();

                        byte* pDst = (byte*)dstPtr.ToPointer();
                        int* pSrc = (int*)srcPtr.ToPointer();
                        for (int i = 0; i < nFovSize; i++)
                            m_buff[nThreadIdx][i] = (byte)pSrc[i];

                        if (m_nPreOverlap != 0 && nScanOffsetX >= nFovSize - nOverlap)
                        {
                            // overlap buffer
                            fixed (byte* pbO = &m_Overlap[nThreadIdx][0])
                                Buffer.MemoryCopy(pDst, pbO, nOverlap, nOverlap);
                        }

                        fixed (byte* pb = &m_buff[nThreadIdx][0])
                            m_clrip.Cpp_ProcessInterpolation(0, nThreadIdx, pb, 1, nCamWidth, nFovSize, pDst);

                        if (m_nPreOverlap != 0 && nScanOffsetX >= nFovSize - nOverlap)
                        {
                            fixed (byte* pbO = &m_Overlap[nThreadIdx][0])
                            {
                                Overlap(pbO, pDst, nOverlap);
                            }
                        }

                        SetTheadDone(nThreadIdx);
                    });
                    iBlock++;
                    m_LastROI.Left = nScanOffsetX;
                    m_LastROI.Right = nScanOffsetX + nCamWidth;
                    m_LastROI.Top = nScanOffsetY;
                    m_LastROI.Bottom = nScanOffsetX + nCamHeight;
                    GrabEvent();

                    if (m_nGrabCount != 0)
                        p_nGrabProgress = Convert.ToInt32((double)iBlock * 100 / m_nGrabCount);
                }
            }
            p_CamInfo.p_eState = eCamState.Ready;
        }
        unsafe void RunGrabLineScanThread()
        {
            StopWatch swGrab = new StopWatch();
            int DelayGrab = (int)(1000 * m_nGrabCount);

            if (m_sapBuf == null)
                p_sInfo = "CamDalsa Buffer Error !!";

            int lY = m_nGrabCount * Convert.ToInt32(p_CamParam.p_Height);
            int iBlock = 0;
            int nByteCnt = m_sapBuf.BytesPerPixel;
            int nCamHeight = p_CamParam.p_Height;
            int nCamWidth = p_CamParam.p_Width;
            int nFOV = m_GD.m_nFovSize;
            long lMemoryWidth = (long)m_Memory.W;
            int nMemoryOffsetX = m_cpScanOffset.X ;
            int nMemoryOffsetY = m_cpScanOffset.Y;
            while (iBlock < m_nGrabCount)
            {
                if (iBlock < m_nGrabTrigger)
                {
                    IntPtr ipSrc = m_pSapBuf[(iBlock) % c_nBuf];
                    Parallel.For(0, nCamHeight, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (y) =>
                    {
                        int yp;
                        if (Scandir)
                            yp = lY - (y + (iBlock) * nCamHeight) + m_nInverseYOffset;
                        else
                            yp = y + (iBlock) * nCamHeight;

                        IntPtr srcPtr = ipSrc + nCamWidth * y * nByteCnt + m_GD.m_nFovStart;
                        IntPtr dstPtr = (IntPtr)((long)m_MemPtr + nMemoryOffsetX * nByteCnt + (yp + nMemoryOffsetY) * lMemoryWidth);

                        long lDataWidth = nFOV * nByteCnt;
                        Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, lDataWidth, lDataWidth);
                    });
                    iBlock++;

                    m_LastROI.Left = nMemoryOffsetX;
                    m_LastROI.Right = nMemoryOffsetX + nCamWidth;
                    m_LastROI.Top = nMemoryOffsetY;
                    m_LastROI.Bottom = nMemoryOffsetY + nCamHeight;
                    GrabEvent();

                    //if (m_nGrabCount != 0)
                    //    p_nGrabProgress = Convert.ToInt32((double)iBlock * 100 / m_nGrabCount);
                }
            }
            p_CamInfo.p_eState = eCamState.Ready;
        }
        void SetTheadBusy(int n)
        {
            ThreadState[n] = false;
        }
        void SetTheadDone(int n)
        {
            ThreadState[n] = true;
        }
        object o = new object();
        int GetReadyThread()
        {
            lock (o)
            {
                for (int n = 0; n < ThreadState.Count; n++)
                {
                    if (ThreadState[n] == true)
                    {
                        ThreadState[n] = false;
                        return n;
                    }
                }
                MessageBox.Show("스레드가 모지랍니다.");
                return 0;
            }
        }
        unsafe void RunGrabLineColorScanThread2()
        {
            StopWatch swGrab = new StopWatch();
            int DelayGrab = 1000 * m_nGrabCount;

            if (m_sapBuf == null)
                p_sInfo = "CamDalsa Buffer Error !!";

            int iBlock = 0;
            int nByteCnt = m_sapBuf.BytesPerPixel;
            int nScanOffsetX = m_cpScanOffset.X;
            int nScanOffsetY = m_cpScanOffset.Y;
            int nCamWidth = p_CamParam.p_Width;
            int nCamHeight = p_CamParam.p_Height;
            int nBufSize = nCamHeight * nCamWidth;
            long nMemWidth = m_Memory.W;
            int nOffsetR = 0;
            int nOffsetG = 0;
            int nOffsetB = 0;

            while (iBlock < m_nGrabCount && !bStopThread)
            {
                if (iBlock < m_nGrabTrigger)
                {
                    IntPtr ipSrc = m_pSapBuf[iBlock % p_nBuf];
                    Parallel.For(0, nCamHeight, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (y) =>
                    {
                        int yp;
                        if (Scandir)
                            yp = m_nGrabCount * nCamHeight - (y + (iBlock) * nCamHeight) + m_nInverseYOffset;
                        else
                            yp = y + iBlock * nCamHeight + nScanOffsetY;

                        long nR = nScanOffsetX + (yp + nOffsetR) * nMemWidth;
                        long nG = nScanOffsetX + (yp + nOffsetG) * nMemWidth;
                        long nB = nScanOffsetX + (yp + nOffsetB) * nMemWidth;
                        IntPtr srcPtr = ipSrc + nCamWidth * y * nByteCnt;
                        IntPtr RedPtr = (IntPtr)((long)m_RedMemPtr + nR);
                        IntPtr GreenPtr = (IntPtr)((long)m_GreenMemPtr + nG);
                        IntPtr BluePtr = (IntPtr)((long)m_BlueMemPtr + nB);

                        if (m_sapBuf.Format == SapFormat.RGB8888)
                        {
                            byte* pRed = (byte*)RedPtr.ToPointer();
                            byte* pGreen = (byte*)GreenPtr.ToPointer();
                            byte* pBlue = (byte*)BluePtr.ToPointer();
                            int* pSrc = (int*)srcPtr.ToPointer();

                            byte R = 0x00, G = 0x00, B = 0x00;
                            for (int i = 0; i < nCamWidth; i++)
                            {
                                int utmp = pSrc[i];

                                B = (byte)((utmp) & 0xff);
                                G = (byte)((utmp >> 8) & 0xff);
                                R = (byte)((utmp >> 16) & 0xff);

                                *pRed++ = R;
                                *pGreen++ = G;
                                *pBlue++ = B;
                            }
                        }
                        else if (m_sapBuf.Format == SapFormat.RGBP8)
                        {
                            Buffer.MemoryCopy((void*)srcPtr, (void*)RedPtr, nCamWidth, nCamWidth);
                            Buffer.MemoryCopy((void*)(srcPtr + nBufSize), (void*)GreenPtr, nCamWidth, nCamWidth);
                            Buffer.MemoryCopy((void*)(srcPtr + nBufSize * 2), (void*)BluePtr, nCamWidth, nCamWidth);
                        }
                    });
                    iBlock++;

                    m_LastROI.Left = nScanOffsetX;
                    m_LastROI.Right = nScanOffsetX + nCamWidth;
                    m_LastROI.Top = nScanOffsetY;
                    m_LastROI.Bottom = nScanOffsetX + nCamHeight;
                    GrabEvent();

                    if (m_nGrabCount != 0)
                        p_nGrabProgress = Convert.ToInt32((double)iBlock * 100 / m_nGrabCount);
                }
            }
            p_CamInfo.p_eState = eCamState.Ready;
        }
        unsafe void RunGrabLineColorScanThread()
        {
            StopWatch swGrab = new StopWatch();
            int DelayGrab = 1000 * m_nGrabCount;

            if (m_sapBuf == null)
                p_sInfo = "CamDalsa Buffer Error !!";

            int iBlock = 0;
            int nByteCnt = m_sapBuf.BytesPerPixel;
            int nScanOffsetX = m_cpScanOffset.X;
            int nScanOffsetY = m_cpScanOffset.Y;
            int nCamWidth = p_CamParam.p_Width;
            int nCamHeight = p_CamParam.p_Height;
            int nFovStart = m_GD.m_nFovStart;
            int nOverlap = m_GD.m_nOverlap;

            if (nOverlap > overlapsize)
            {
                MessageBox.Show("Overlap 이 100 보다큽니다.");
                nOverlap = overlapsize;
            }
            int fR = (int)((double)m_GD.m_nFovSize / m_GD.m_dScaleR);
            int fG = (int)((double)m_GD.m_nFovSize / m_GD.m_dScaleG);
            int fB = (int)((double)m_GD.m_nFovSize / m_GD.m_dScaleB);
            int nFovSize = Math.Max(fB, Math.Max(fR, fG));
            nFovSize = nFovSize + nOverlap;
            if(m_GD.m_nFovSize > nFovSize)
            {
                nFovSize = m_GD.m_nFovSize;
            }

            if (nCamWidth < nFovStart + nFovSize)
            {
                MessageBox.Show("FovStart+ nFovSize+ nOverlap가 CamWidth보다 큽니다.(" + nFovStart.ToString() + " + " + nFovSize.ToString() + " > " + nCamWidth.ToString() + ")");
                nFovSize = nCamWidth - nFovStart;
            }

            int nBufSize = nCamHeight * nCamWidth;
            long nMemWidth = m_Memory.W;

            if (m_GD.m_nOverlap != m_nPreOverlap)
            {
                m_nPreOverlap = m_GD.m_nOverlap;
                if (m_nPreOverlap != 0)
                {
                    double dRatio = 1.0 / m_nPreOverlap;
                    for (int n = 0; n < m_nPreOverlap; n++)
                    {
                        m_pdOverlap[n] = dRatio * (double)n;
                    }
                }
            }

            int nXShiftR = (m_GD.m_dShiftR >= 0) ? (int)(m_GD.m_dShiftR) : (int)Math.Ceiling(m_GD.m_dShiftR);    // 정수값만 취함 (5.7 -> 5, -5.7 -> -5)
            int nXShiftG = (m_GD.m_dShiftG >= 0) ? (int)(m_GD.m_dShiftG) : (int)Math.Ceiling(m_GD.m_dShiftG);
            int nXShiftB = (m_GD.m_dShiftB >= 0) ? (int)(m_GD.m_dShiftB) : (int)Math.Ceiling(m_GD.m_dShiftB);

            if (m_GD.m_dScaleR != m_dPReXScaleR
                || (m_GD.m_dShiftR - nXShiftR) != m_dPReXShiftR
                || nFovSize != m_nPreWidthR)
            {
                m_dPReXScaleR = m_GD.m_dScaleR;
                m_dPReXShiftR = m_GD.m_dShiftR - nXShiftR;
                m_nPreWidthR = nFovSize;
                m_clrip.Cpp_CreatInterpolationData(0, m_dPReXScaleR, m_dPReXShiftR, m_nPreWidthR);
            }
            if (m_GD.m_dScaleG != m_dPReXScaleG
                || (m_GD.m_dShiftG - nXShiftG) != m_dPReXShiftG
                || nFovSize != m_nPreWidthG)
            {
                m_dPReXScaleG = m_GD.m_dScaleG;
                m_dPReXShiftG = m_GD.m_dShiftG - nXShiftG;
                m_nPreWidthG = nFovSize;
                m_clrip.Cpp_CreatInterpolationData(1, m_dPReXScaleG, m_dPReXShiftG, m_nPreWidthG);
            }
            if (m_GD.m_dScaleB != m_dPReXScaleB
                || (m_GD.m_dShiftB - nXShiftB) != m_dPReXShiftB
                || nFovSize != m_nPreWidthB)
            {
                m_dPReXScaleB = m_GD.m_dScaleB;
                m_dPReXShiftB = m_GD.m_dShiftB - nXShiftB;
                m_nPreWidthB = nFovSize;
                m_clrip.Cpp_CreatInterpolationData(2, m_dPReXScaleB, m_dPReXShiftB, m_nPreWidthB);
            }

            const int nTimeOut_10s = 10000; //ms            
            const int nTimeOutInterval = 10; // ms
            int nScanAxisTimeOut = nTimeOut_10s / nTimeOutInterval;
            int previBlock = 0;
            Console.WriteLine("Grab Loop Start");
            m_bGrabThreadOn = true;
            while (iBlock < m_nGrabCount)
            {
                if (previBlock == iBlock)
                {
                    Thread.Sleep(nTimeOutInterval);
                    if (--nScanAxisTimeOut <= 0)
                    {
                        m_log.Info("TimeOut - RunGrabLineColorScanThread");
                        m_nGrabTrigger = m_nGrabCount;
                    }

                }
                else
                {
                    previBlock = iBlock;
                    nScanAxisTimeOut = nTimeOut_10s / nTimeOutInterval;
                }
                if (iBlock < m_nGrabTrigger)
                {
                    IntPtr ipSrc = m_pSapBuf[iBlock % p_nBuf];

                    Parallel.For(0, nCamHeight, new ParallelOptions { MaxDegreeOfParallelism = thread }, (y) =>
                    {

                        int yp;
                        if (Scandir)
                            yp = m_nLine - (y + (iBlock) * nCamHeight) + m_nInverseYOffset + m_nOffsetTest;
                        else
                            yp = y + iBlock * nCamHeight + nScanOffsetY + m_nOffsetTest;

                        int ypR = yp + m_GD.m_nYShiftR;
                        int ypG = yp + m_GD.m_nYShiftG;
                        int ypB = yp + m_GD.m_nYShiftB;

                        if (m_GD.m_bUseFlipVertical == true)     // 영상 상하 반전
                        {
                            ypR = m_Memory.p_sz.Y - yp + m_GD.m_nYShiftR;
                            ypG = m_Memory.p_sz.Y - yp + m_GD.m_nYShiftG;
                            ypB = m_Memory.p_sz.Y - yp + m_GD.m_nYShiftB;
                        }

                        if (ypR < 0) ypR = 0;
                        if (ypG < 0) ypG = 0;
                        if (ypB < 0) ypB = 0;

                        long nR = nScanOffsetX + ypR * nMemWidth + nXShiftR;
                        long nG = nScanOffsetX + ypG * nMemWidth + nXShiftG;
                        long nB = nScanOffsetX + ypB * nMemWidth + nXShiftB;
                        IntPtr srcPtr = ipSrc + nCamWidth * y * nByteCnt + nFovStart* nByteCnt ;
                        IntPtr RedPtr = (IntPtr)((long)m_RedMemPtr + nR);
                        IntPtr GreenPtr = (IntPtr)((long)m_GreenMemPtr + nG);
                        IntPtr BluePtr = (IntPtr)((long)m_BlueMemPtr + nB);
                        int nThreadIdx = GetReadyThread();

                        if (m_sapBuf.Format == SapFormat.RGB8888  || m_sapBuf.Format == SapFormat.RGB888)
                        {
                            byte* pRed = (byte*)RedPtr.ToPointer();
                            byte* pGreen = (byte*)GreenPtr.ToPointer();
                            byte* pBlue = (byte*)BluePtr.ToPointer();
                            int* pSrc = (int*)srcPtr.ToPointer();

                            byte R = 0x00, G = 0x00, B = 0x00;
                            for (int i = 0; i < nFovSize; i++)
                            {
                                int utmp = pSrc[i];

                                B = (byte)((utmp) & 0xff);
                                G = (byte)((utmp >> 8) & 0xff);
                                R = (byte)((utmp >> 16) & 0xff);

                                m_buffR[nThreadIdx][i] = R;
                                m_buffG[nThreadIdx][i] = G;
                                m_buffB[nThreadIdx][i] = B;
                            }

                            if (m_nPreOverlap != 0 && nScanOffsetX >= nFovSize - nOverlap)
                            {
                                // overlap buffer
                                fixed (byte* pbOR = &m_OverlapR[nThreadIdx][0], pbOG = &m_OverlapG[nThreadIdx][0], pbOB = &m_OverlapB[nThreadIdx][0])
                                {
                                    Buffer.MemoryCopy((void*)(pRed), (void*)pbOR, nOverlap, nOverlap);
                                    Buffer.MemoryCopy((void*)(pGreen), (void*)pbOG, nOverlap, nOverlap);
                                    Buffer.MemoryCopy((void*)(pBlue), (void*)pbOB, nOverlap, nOverlap);
                                }
                            }

                            fixed (byte* pbR = &m_buffR[nThreadIdx][0], pbG = &m_buffG[nThreadIdx][0], pbB = &m_buffB[nThreadIdx][0])
                            {
                                m_clrip.Cpp_ProcessInterpolation(0, nThreadIdx, pbR, 1, nCamWidth, nFovSize, pRed);
                                m_clrip.Cpp_ProcessInterpolation(1, nThreadIdx, pbG, 1, nCamWidth, nFovSize, pGreen);
                                m_clrip.Cpp_ProcessInterpolation(2, nThreadIdx, pbB, 1, nCamWidth, nFovSize, pBlue);
                            }
                            if (m_nPreOverlap != 0 && nScanOffsetX >= nFovSize - nOverlap)
                            {
                                fixed (byte* pbOR = &m_OverlapR[nThreadIdx][0], pbOG = &m_OverlapG[nThreadIdx][0], pbOB = &m_OverlapB[nThreadIdx][0])
                                {
                                    Overlap(pbOR, pRed, nOverlap);
                                    Overlap(pbOG, pGreen, nOverlap);
                                    Overlap(pbOB, pBlue, nOverlap);
                                }
                            }
                        }
                        else if (m_sapBuf.Format == SapFormat.RGBP8)
                        {
                            Buffer.MemoryCopy((void*)srcPtr, (void*)RedPtr, nCamWidth, nCamWidth);
                            Buffer.MemoryCopy((void*)(srcPtr + nBufSize), (void*)GreenPtr, nCamWidth, nCamWidth);
                            Buffer.MemoryCopy((void*)(srcPtr + nBufSize * 2), (void*)BluePtr, nCamWidth, nCamWidth);
                        }
                        SetTheadDone(nThreadIdx);
                    });
                    iBlock++;
                    m_LastROI.Left = nScanOffsetX;
                    m_LastROI.Right = nScanOffsetX + nCamWidth;
                    m_LastROI.Top = nScanOffsetY;
                    m_LastROI.Bottom = nScanOffsetX + nCamHeight;
                    GrabEvent();

                    if (m_nGrabCount != 0)
                        p_nGrabProgress = Convert.ToInt32((double)iBlock * 100 / m_nGrabCount);
                }
            }
            p_CamInfo.p_eState = eCamState.Ready;
            Console.WriteLine("Grab Loop End");
        }
        unsafe void Overlap(byte* pS, byte* pD, int nOverlap)
        {
            for (int n = 0; n < nOverlap; n++, pS++, pD++)
            {
                *pD = (byte)((double)*pD * m_pdOverlap[n] + (double)*pS * m_pdOverlap[nOverlap - n - 1]);
            }
        }
        void GrabEvent()
        {
            if (Grabed != null)
                OnGrabed(new GrabedArgs(m_Memory, m_nGrabTrigger, m_LastROI, p_nGrabProgress));
        }
        protected virtual void OnGrabed(GrabedArgs e)
        {
            if (Grabed != null)
                Grabed.Invoke(this, e);
        }
        void ConnectAreaCammera()
        {
            if (p_CamInfo.p_sServer == "")
            {
                p_sInfo = p_id + "Empty Server Name";
                return;
            }
            if (p_CamInfo.p_sAreaCamFile == "")
            {
                p_sInfo = p_id + "Empty Config File Name";
                return;
            }

            SapLocation loc = new SapLocation(p_CamInfo.p_sServer, p_CamInfo.p_nResourceIdx);
            SapManager.GetServerName(loc);

            if (SapManager.GetResourceCount(p_CamInfo.p_sServer, SapManager.ResourceType.Acq) > 0)          //"Acq" (frame-grabber) "AcqDevice" (camera)
            {
                m_sapAcq = new SapAcquisition(loc, p_CamInfo.p_sAreaCamFile);

                if (!m_sapAcq.Create())
                {
                    loc.Dispose();
                    p_sInfo = DestroysObjects(p_id + "Error during SapAcquisition creation");
                    return;
                }

                SapFormat bufformat = m_sapAcq.XferParams.Format;

                if (bufformat == SapFormat.RGB8888 || bufformat == SapFormat.RGBP8)
                    m_sapBuf = new SapBuffer(p_nBuf, m_sapAcq.XferParams.Width, m_sapAcq.XferParams.Height, bufformat, SapBuffer.MemoryType.ScatterGather);
                else if (bufformat == SapFormat.Mono8)
                    m_sapBuf = new SapBuffer(p_nBuf, m_sapAcq, SapBuffer.MemoryType.ScatterGather);

                m_sapXfer = new SapAcqToBuf(m_sapAcq, m_sapBuf);
            }

            SapLocation loc2 = new SapLocation(p_CamInfo.p_sServer, 0);
            if (SapManager.GetResourceCount(p_CamInfo.p_sServer, SapManager.ResourceType.AcqDevice) > 0)
                m_sapDevice = new SapAcqDevice(loc2, false);

            if (m_sapXfer == null)
                return;

            m_sapXfer.Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
            m_sapXfer.XferNotify += new SapXferNotifyHandler(xfer_XferNotify);
            m_sapXfer.XferNotifyContext = this;

            if (m_sapDevice != null && !m_sapDevice.Create())
            {
                p_sInfo = DestroysObjects(p_id + "Error during SapDevice creation");
                return;
            }

            if (!m_sapBuf.Create())
            {
                p_sInfo = DestroysObjects(p_id + "Error during SapBuffer creation");
                return;
            }

            if (!m_sapXfer.Create())
            {
                p_sInfo = DestroysObjects(p_id + "Error during SapTransfer creation");
                return;
            }
            loc2.Dispose();
            loc.Dispose();
            p_CamParam.SetCamHandle(m_sapDevice, m_sapAcq);
            p_CamInfo.p_eState = eCamState.Ready;
            m_log.Info(p_id + "Connect Success");
        }
        public void GrabArea()
        {
            DestroysObjects("Disconnect command - grab area");
            ConnectAreaCammera();
            p_CamParam.SetAreaParams();

            RunTree(Tree.eMode.Init);
            m_ImageLive = new ImageData(p_CamParam.p_Width, p_CamParam.p_Height, 1);
            p_ImageViewer.SetImageData(m_ImageLive);
            m_pSapBuf = new IntPtr[p_nBuf];

            for (int n = 0; n < p_nBuf; n++)
                m_sapBuf.GetAddress(n, out m_pSapBuf[n]);

            p_CamInfo.p_eState = eCamState.GrabLive;
            m_nGrabTrigger = 0;
            m_sapXfer.Grab();
            m_GrabThread = new Thread(new ThreadStart(RunGrabAreaScanThread));
            m_GrabThread.Start();
        }
        unsafe void RunGrabAreaScanThread()
        {
            int iBlock = 0;
            int height = p_CamParam.p_Height;
            int width = p_CamParam.p_Width;

            while (p_CamInfo.p_eState == eCamState.GrabLive)
            {
                Thread.Sleep(10);

                IntPtr ipSrc = m_pSapBuf[(iBlock) % p_nBuf];
                Parallel.For(0, height, (y) => {
                    IntPtr srcPtr = ipSrc + width * y;
                    IntPtr dstPtr = (IntPtr)((long)m_ImageLive.GetPtr() + m_cpScanOffset.X + (y + m_cpScanOffset.Y) * (long)m_ImageLive.p_Size.X);
                    Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, width, width);
                });
                iBlock++;
                Application.Current.Dispatcher.Invoke(delegate
                {
                    m_ImageLive.UpdateImage();
                });
            }
        }
        public void AbortGrabThread()
        {
            if (m_GrabThread != null)
                m_GrabThread.Abort();

            p_CamInfo.p_eState = eCamState.Ready;
        }
        public double GetFps() { return 0; }

        #region Camera Event
        static void xfer_XferNotify(object sender, SapXferNotifyEventArgs args)
        {
            Camera_Dalsa cam = args.Context as Camera_Dalsa;
            cam.m_nGrabTrigger++;
            //Debug.WriteLine("XferTrigger : " + cam.m_nGrabTrigger);
        }
        #endregion

        #region RelayCommand
        public RelayCommand ConnectCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    if (!bgw_Connect.IsBusy && p_CamInfo.p_eState == eCamState.Init)
                        bgw_Connect.RunWorkerAsync();
                });
            }
        }
        public RelayCommand DisConnectCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    DestroysObjects("Disconnect Command");
                });
            }
        }
        public RelayCommand LiveGrabCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    if (p_CamInfo.p_eState == eCamState.GrabLive)
                    {
                        m_sapXfer.Freeze();
                        p_CamInfo.p_eState = eCamState.Ready;
                        DestroysObjects("Live Grab Stop");
                    }
                    else
                    {
                        GrabArea();
                    }
                });
            }
        }
        public RelayCommand StopGrabCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    if (p_CamInfo.p_eState == eCamState.GrabLive)
                    {
                        m_sapXfer.Freeze();
                        p_CamInfo.p_eState = eCamState.Ready;
                        DestroysObjects("Live Grab Stop");
                    }
                });
            }
        }
        #endregion
    }
    public class CameraConnectStateConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            eCamState state = (eCamState)value;
            switch (state)
            {
                case eCamState.Init:
                    return false;
                default:
                    return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        #endregion
    }

    public class CameraNotConnectStateConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            eCamState state = (eCamState)value;
            switch (state)
            {
                case eCamState.Init:
                    return true;
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        #endregion
    }

    public class CameraCanGrabConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            eCamState state = (eCamState)value;
            switch (state)
            {
                case eCamState.GrabLive:
                    return false;
                default:
                    return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        #endregion
    }

    public class CameraCanNotGrabConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            eCamState state = (eCamState)value;
            switch (state)
            {
                case eCamState.GrabLive:
                    return true;
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        #endregion
    }


}





