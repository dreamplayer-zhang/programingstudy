﻿using System.Windows.Controls;
using DALSA.SaperaLT.SapClassBasic;
using System.ComponentModel;
using System.Windows;
using System;
using RootTools.Memory;
using System.Threading;
using RootTools.Trees;
using System.Windows.Data;

namespace RootTools.Camera.Dalsa
{
    public class Camera_Dalsa : ObservableObject, ICamera
    {
        public event EventHandler Grabed;

        #region Property
        public string p_id { get; set; }

        int m_nGrabProgress = 0;
        public int p_nGrabProgress{
            get{
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
        const int c_nBuf = 100;
        int _nBuf = 100;
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
        int m_nGrabTrigger=0;
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

        Log m_log;
        public SapAcquisition m_sapAcq = null;
        public SapBuffer m_sapBuf = null;
        public SapTransfer m_sapXfer = null;
        public SapAcqDevice m_sapDevice = null;
        BackgroundWorker bgw_Connect = new BackgroundWorker();
        ImageData m_ImageLive;
        CPoint m_cpScanOffset = new CPoint();
        int m_nInverseYOffset = 0;
        int m_nGrabCount = 0;
        MemoryData m_Memory;
        IntPtr m_MemPtr = IntPtr.Zero;
        IntPtr[] m_pSapBuf;

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

        public Camera_Dalsa(string id, Log log)
        {
            p_id = id;
            m_log = log;
            p_treeRoot = new TreeRoot(id, m_log);
            bgw_Connect.DoWork += bgw_Connect_DoWork;
            bgw_Connect.RunWorkerCompleted += bgw_Connect_RunWorkerCompleted;
            m_ImageLive = new ImageData(640, 480);
            p_ImageViewer = new ImageViewer_ViewModel(m_ImageLive);
            p_CamParam = new DalsaParameterSet(m_log);
            p_CamInfo = new DalseCamInfo(m_log);
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
            p_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
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
            if (m_sapXfer != null)
            {
                RunImageRoiTree(treeRoot.GetTree("Buffer Image ROI"));
            }
        }

        void RunSetTree(Tree tree)
        {
            p_CamInfo.p_sServer = tree.Set(p_CamInfo.p_sServer, "Dalsa", "Server", "Cam Server Name");
            p_CamInfo.p_sFile = tree.SetFile(p_CamInfo.p_sFile, p_CamInfo.p_sFile, "ccf", "CamFile", "Cam File");
            //p_CamInfo._DeviceUserID = tree.Set(p_CamInfo._DeviceUserID, "Basler", "ID", "Device User ID");
            //m_nGrabTimeout = tree.Set(m_nGrabTimeout, 2000, "Timeout", "Grab Timeout (ms)");
        }

        void RunImageRoiTree(Tree tree)
        {
            
            p_CamParam.p_Width = tree.Set(p_CamParam.p_Width, 100, "Image Width", "Buffer Image Width");
            p_CamParam.p_Height = tree.Set(p_CamParam.p_Height, 100, "Image Height", "Buffer Image Height");
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

        public void ThreadStop()
        {
            //DestroysObjects("Camera Closed -  ThreadStop");
        }

        public void Connect()
        {
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
            SapLocation loc = new SapLocation(p_CamInfo.p_sServer, 0);
            if(SapManager.GetResourceCount(p_CamInfo.p_sServer, SapManager.ResourceType.AcqDevice) > 0 )
                m_sapDevice = new SapAcqDevice(loc);
            if (SapManager.GetResourceCount(p_CamInfo.p_sServer, SapManager.ResourceType.Acq) > 0)          //"Acq" (frame-grabber) "AcqDevice" (camera)
            {
                m_sapAcq = new SapAcquisition(loc, p_CamInfo.p_sFile);
                if (!m_sapAcq.Create())
                {
                    loc.Dispose();
                    p_sInfo = DestroysObjects(p_id + "Error during SapAcquisition creation");
                    return;
                }
                m_sapBuf = new SapBuffer(p_nBuf, m_sapAcq, SapBuffer.MemoryType.ScatterGather);
                m_sapXfer = new SapAcqToBuf(m_sapAcq, m_sapBuf);
            }
            loc.Dispose();
            if (m_sapXfer == null)
            {
                p_sInfo = p_id + " Sapera Transfer Empty";
                return;
            }
            m_sapXfer.Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
            m_sapXfer.XferNotify += new SapXferNotifyHandler(xfer_XferNotify);
            m_sapXfer.XferNotifyContext = this;

            if( m_sapDevice != null && !m_sapDevice.Create())
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
            if(m_sapDevice != null)
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

            return sLog;
        }

        Thread m_GrabThread;

        void TestFunction()
        {
            GrabArea();
        }

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
            m_nGrabCount = 0;
            m_sapXfer.Freeze();
            return "OK";
        }

        public int GetGrabProgress()
        {  
            return Convert.ToInt32((double)m_nGrabTrigger*100/m_nGrabCount);
        }

        public void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false, int ReverseOffsetY = 0)
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
            if (memory.p_nByte != 1)
            {
                p_sInfo = p_id + " MemoryData Byte not 1";
                return;
            }

            p_CamParam.p_eDir = bInvY ? DalsaParameterSet.eDir.Reverse : DalsaParameterSet.eDir.Forward;
            m_Memory = memory;
            m_MemPtr = memory.GetPtr();
            m_cpScanOffset = cpScanOffset;
            m_nInverseYOffset = ReverseOffsetY;
            m_nGrabCount = (int)Math.Truncate(1.0 * nLine / p_CamParam.p_Height);

            m_pSapBuf = new IntPtr[p_nBuf];
            for (int n = 0; n < p_nBuf; n++)
                m_sapBuf.GetAddress(n, out m_pSapBuf[n]);
        
            //m_iBlock = -1;
            m_sapBuf.Index = (int)(0);
            m_nGrabTrigger = 0;
            m_sapXfer.Snap((int)(m_nGrabCount));
            p_CamInfo.p_eState = eCamState.GrabMem;
            m_GrabThread = new Thread(new ThreadStart(RunGrabLineScanThread));
            m_GrabThread.Start();
        }
        private CRect m_LastROI = new CRect(0, 0, 0, 0);
       unsafe void RunGrabLineScanThread()
        {
            StopWatch swGrab = new StopWatch();
            int DelayGrab = (int)(1000 * m_nGrabCount);

            if (m_sapBuf == null)
                p_sInfo =  "CamDalsa Buffer Error !!";

            int lY = m_nGrabCount * Convert.ToInt32(p_CamParam.p_Height);
            int iBlock = 0;

            while (iBlock < m_nGrabCount)
            {
                //if (swGrab.ElapsedMilliseconds > DelayGrab)
                //{
                //    p_sInfo = "Cam Grab Delay Error";
                //    return;
                //}
                Thread.Sleep(1);
                if (iBlock < m_nGrabTrigger)
                {
                    IntPtr ipSrc = m_pSapBuf[(iBlock) % p_nBuf];
                    for (int y = 0; y < p_CamParam.p_Height; y++)
                    {  
                        int yp = y + (iBlock) * p_CamParam.p_Height;
                        if (p_CamParam.p_eDir == DalsaParameterSet.eDir.Reverse)
                        {
                            yp = lY - yp + m_nInverseYOffset;
                        }
                        IntPtr srcPtr = ipSrc + p_CamParam.p_Width*y;
                        IntPtr dstPtr = (IntPtr)((long)m_MemPtr + m_cpScanOffset.X + (yp + m_cpScanOffset.Y) * (long)m_Memory.W);
                        Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, p_CamParam.p_Width, p_CamParam.p_Width);
                        //IntPtr dstPtr = (IntPtr)((long)m_ImageLive.GetPtr() + m_cpScanOffset.X + (y + m_cpScanOffset.Y) * (long)m_ImageLive.p_Size.X);
                        //Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, p_CamParam.p_Width, p_CamParam.p_Width);
                    }
                    iBlock++;

                    m_LastROI.Left = m_cpScanOffset.X;
                    m_LastROI.Right = m_cpScanOffset.X + p_CamParam.p_Width;
                    m_LastROI.Top = m_cpScanOffset.Y;
                    m_LastROI.Bottom = m_cpScanOffset.Y + p_CamParam.p_Height;
                    GrabEvent();

                    if (m_nGrabCount !=0)
                        p_nGrabProgress = Convert.ToInt32((double)iBlock * 100 / m_nGrabCount);
                }
            }
            p_CamInfo.p_eState = eCamState.Ready;
        }
        void GrabEvent()
        {
            if (Grabed != null)
                OnGrabed(new GrabedArgs(m_Memory, m_nGrabTrigger, m_LastROI));
        }
        protected virtual void OnGrabed(GrabedArgs e)
        {
            if (Grabed != null)
                Grabed.Invoke(this, e);
        }
        public void GrabArea()
        {
           //p_CamParam.ReadParamter();
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

            while (p_CamInfo.p_eState == eCamState.GrabLive)
            {
                if (iBlock < m_nGrabTrigger)
                {
                    Thread.Sleep(10);

                    IntPtr ipSrc = m_pSapBuf[(iBlock) % p_nBuf];
                    for (int y = 0; y < p_CamParam.p_Height; y++)
                    {
                        int yp = y + (iBlock) * p_CamParam.p_Height;
                        IntPtr srcPtr = ipSrc + p_CamParam.p_Width * y;
                        IntPtr dstPtr = (IntPtr)((long)m_ImageLive.GetPtr() + m_cpScanOffset.X + (y + m_cpScanOffset.Y) * (long)m_ImageLive.p_Size.X);
                        Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, p_CamParam.p_Width, p_CamParam.p_Width);
                    }
                    iBlock++;
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        m_ImageLive.UpdateImage();
                    });
                }
            }
        }

        #region Camera Event
        static void xfer_XferNotify(object sender, SapXferNotifyEventArgs args)
        {
            Camera_Dalsa cam = args.Context as Camera_Dalsa;
                cam.m_nGrabTrigger++;
        }
        #endregion

        #region RelayCommand
        public RelayCommand ConnectCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
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
                        }
                        else
                            GrabArea();
                    });
            }
        }

        public RelayCommand TestCommand
        {
            get
            {
                return new RelayCommand(TestFunction);
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


    
}
