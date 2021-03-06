using Matrox.MatroxImagingLibrary;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace RootTools.Camera.Matrox
{
    public class Camera_Matrox : ObservableObject, ICamera
    {
        Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        public event EventHandler Grabed;
        BackgroundWorker bgw_Connect = new BackgroundWorker();
        public ImageData m_ImageLive;
        RootTools_CLR.CLR_3D m_clr3D = new RootTools_CLR.CLR_3D();
        private MIL_ID m_MilApplication;                     // Application -> 프로그램당 하나
        private MIL_ID m_MilSystem;                          // 프레임그래버
        private MIL_ID m_MilDigitizer;                       // 카메라
        private MIL_ID m_MilDisplay;                         // 디스플레이
    //    private MIL_ID m_MilImage = MIL.M_NULL;              // MIL Image buffer identifier.
        private MIL_ID[] m_MilBuffers = new MIL_ID[c_nBuf];  // 버퍼

        static int m_nTest = 0;
        int m_nOffset = 0;
        
        public bool m_bGrabThreadOn = false;   // true When thread is arrived in Grab Loop

        int m_nWidth = 0;
        public int p_nWidth
        {
            get
            {
                return m_nWidth;
            }
            set
            {
                SetProperty(ref m_nWidth, value);
            }
        }
        int m_nHeight = 0;
        public int p_nHeight
        {
            get
            {
                return m_nHeight;
            }
            set
            {
                SetProperty(ref m_nHeight, value);
            }
        }
        int m_nImgBand = 0;
        public int p_nImgBand
        {
            get
            {
                return m_nImgBand;
            }
            set
            {
                SetProperty(ref m_nImgBand, value);
            }
        }

        Log m_log;

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
                m_log.Warn(value);
            }
        }

        byte[] bufarr = null;
        public byte[] p_aBuf
        {
            get
            {
                MIL.MbufGet2d(m_MilBuffers[0], 0, 0, p_nWidth, p_nHeight, bufarr);
                return bufarr;
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

        MatroxCamInfo m_CamInfo;
        public MatroxCamInfo p_CamInfo
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
        const int c_nBuf = 10000;
        int _nBuf = c_nBuf;
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
        //int m_nGrabTrigger = 0;

        public CPoint p_sz
        {
            get { return new CPoint(); }
            set { }
        }

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

        #region UI
        public UserControl p_ui
        {
            get
            {
                Camera_Matrox_UI ui = new Camera_Matrox_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

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
            RunImageRoiTree(treeRoot.GetTree("Buffer Image ROI"));
            return;
        }

        void RunSetTree(Tree tree)
        {
            p_CamInfo.p_sFile = tree.SetFile(p_CamInfo.p_sFile, p_CamInfo.p_sFile, "dcf", "CamFile", "Cam File");
            p_CamInfo.p_nSystemNum = tree.Set(p_CamInfo.p_nSystemNum, p_CamInfo.p_nSystemNum, "System Number", "System Number");
        }

        void RunImageRoiTree(Tree tree)
        {
            p_nImgBand = tree.Set(p_nImgBand, p_nImgBand, "Image Band", "Image Band");
            p_nWidth = tree.Set(p_nWidth, p_nWidth, "Image Width", "Image Width");
            p_nHeight = tree.Set(p_nHeight, p_nHeight, "Image Height", "Image Height");
        }
        #endregion

        // 생성자
        public Camera_Matrox(string id, Log log)
        {
            p_id = id;
            m_log = log;
            p_treeRoot = new TreeRoot(id, m_log);
            bgw_Connect.DoWork += bgw_Connect_Dowork;
            bgw_Connect.RunWorkerCompleted += bgw_Connect_RunWorkerCompleted;
            //m_ImageLive = new ImageData(1920, 1080, 1);
            m_ImageLive = new ImageData(4096, 4000, 1);
            p_ImageViewer = new ImageViewer_ViewModel(m_ImageLive, null, _dispatcher);
            p_CamInfo = new MatroxCamInfo(m_log);
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
            p_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }
        public void Init3D(MemoryData MemConv, MemoryData MemHeight, MemoryData MemBright,MemoryData MemRaw, int nMaxOverlapSize,int nMaxFrameNum)
        {
            unsafe 
            {
                m_MemConv = MemConv;
                m_MemHeight = MemHeight;
                m_MemBright = MemBright;
                m_MemRaw = MemRaw;
                byte* ppConvertedImage = (byte*)m_MemConv.GetPtr();
                ushort* ppBuffHeight = (ushort*)m_MemHeight.GetPtr();
                short* ppBuffBright = (short*)m_MemBright.GetPtr();
                byte* ppBuffRaw = (byte*)m_MemRaw.GetPtr();
                int szImageBufferX = (int)m_MemConv.W;
                int szImageBufferY= (int)m_MemConv.H;
                int szMaxRawImageX = p_nWidth;//(int)m_MemRaw.W;
                int szMaxRawImageY = p_nHeight;//(int)m_MemRaw.H;
                m_clr3D.Init3D(ppConvertedImage, ppBuffHeight, ppBuffBright, szImageBufferX, szImageBufferY, ppBuffRaw, szMaxRawImageX, szMaxRawImageY, nMaxOverlapSize, nMaxFrameNum);               
            }
        }
        public void Connect()
        {
            if (!bgw_Connect.IsBusy && p_CamInfo.p_eState == eCamState.Init)
                bgw_Connect.RunWorkerAsync();
        }

        void bgw_Connect_Dowork(object sender, DoWorkEventArgs e)
        {
            ConnectCamera();
        }

        void bgw_Connect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunTree(Tree.eMode.Init);
        }

        void ConnectCamera()
        {
            // Application 
            if (m_MilApplication == MIL.M_NULL)
                MIL.MappAlloc(MIL.M_NULL, MIL.M_DEFAULT, ref m_MilApplication);
            // System
            if (m_MilSystem == MIL.M_NULL)
                MIL.MsysAlloc(MIL.M_DEFAULT, "M_SYSTEM_RAPIXOCXP", p_CamInfo.p_nSystemNum, MIL.M_DEFAULT, ref m_MilSystem);  // system discriptor 는 연결 타입
            //MIL.MsysAlloc(MIL.M_DEFAULT, "M_SYSTEM_RAPIXOCXP", MIL.M_DEFAULT, MIL.M_DEFAULT, ref m_MilSystem);  // system discriptor 는 연결 타입

            //MIL.MsysAlloc(MIL.M_DEFAULT, MIL.M_SYSTEM_SOLIOS, MIL.M_DEFAULT, MIL.M_DEFAULT, ref m_MilSystem);  // system discriptor 는 연결 타입
            //MIL.MsysAlloc(MIL.M_DEFAULT, MIL.M_SYSTEM_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref m_MilSystem);  // system discriptor 는 연결 타입
            // Display
            if (m_MilDisplay == MIL.M_NULL)
                MIL.MdispAlloc(m_MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WINDOWED, ref m_MilDisplay);

            // Set dafault values for the image buffer in case no digitizer can be allocated
            long lImgAttributes = MIL.M_IMAGE + MIL.M_DISP;
            int FRAMEBURST_SIZE = 1;
            // Inquire the number of digitizers for the system
            if (m_MilSystem != MIL.M_NULL)
            {
                MIL_INT nNumberOfDigitizers = MIL.MsysInquire(m_MilSystem, MIL.M_DIGITIZER_NUM, MIL.M_NULL);
                if (nNumberOfDigitizers > 0)
                {
                    // Digitizer
                    if (m_MilDigitizer == MIL.M_NULL)  
                    {
                        MIL.MdigAlloc(m_MilSystem, MIL.M_DEFAULT, p_CamInfo.p_sFile, MIL.M_DEFAULT, ref m_MilDigitizer);
  
                        p_nImgBand = (int)MIL.MdigInquire(m_MilDigitizer, MIL.M_SIZE_BAND, MIL.M_NULL);
                        p_nWidth = (int)MIL.MdigInquire(m_MilDigitizer, MIL.M_SIZE_X, MIL.M_NULL);
                        p_nHeight = (int)MIL.MdigInquire(m_MilDigitizer, MIL.M_SIZE_Y, MIL.M_NULL);
                   //     int offsetY = (int)MIL.MdigInquire(m_MilDigitizer, MIL.M_SOURCE_OFFSET_Y, MIL.M_NULL);
                    //    MIL.MdigControl(m_MilDigitizer, MIL.M_SOURCE_OFFSET_Y, 446);
                    //    offsetY = (int)MIL.MdigInquire(m_MilDigitizer, MIL.M_SOURCE_OFFSET_Y, MIL.M_NULL);
                        // Add M_GRAB attribute if a digitizer is allocated.
                        lImgAttributes |= MIL.M_GRAB;

                        if (FRAMEBURST_SIZE != 1)
                        {
                            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_FRAME_BURST_SIZE, FRAMEBURST_SIZE);
                            /////* Sets the maximum amount of time to wait for all the frames to be grabbed. */
                            /////* The value, in seconds, is set to 100ms. */
                            double dFrameBurstMaxTime = 1.00;//0.100;
                            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_FRAME_BURST_MAX_TIME, dFrameBurstMaxTime); 
                            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_FRAME_BURST_END_TRIGGER_STATE, MIL.M_ENABLE);
                            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_FRAME_BURST_END_TRIGGER_SOURCE, MIL.M_AUX_IO0);
                            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_TRIGGER_ACTIVATION, MIL.M_DEFAULT);
                            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_TRIGGER_SOURCE, MIL.M_DEFAULT);
                            MIL.MsysControl(m_MilSystem, MIL.M_ALLOCATION_OVERSCAN, MIL.M_DISABLE);
                        }                    
                    }
                }
            }
       //     MIL.MbufAllocColor(m_MilSystem, p_nImgBand, p_nWidth, p_nHeight, 8 + MIL.M_UNSIGNED, lImgAttributes, ref m_MilImage);
         //   MIL.MbufClear(m_MilImage, 0);

            //MIL.MappControl(MIL.M_DEFAULT, MIL.M_ERROR, MIL.M_PRINT_DISABLE);
            // Allocate the grab buffers and clear them.
            for (int i = 0; i < p_nBuf; i++)
            {

                MIL.MbufAlloc2d(m_MilSystem, (MIL_INT)p_nWidth, (MIL_INT)p_nHeight * FRAMEBURST_SIZE, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_HOST_MEMORY , ref m_MilBuffers[i]);

                if (m_MilBuffers[i] != MIL.M_NULL)
                {
                    MIL.MbufClear(m_MilBuffers[i], 0);
                }
            }
            MIL.MappControl(MIL.M_DEFAULT, MIL.M_ERROR, MIL.M_PRINT_DISABLE);

            // Free buffers to leave space for possible temporary buffers.
        //    for (int n = 0; n < 2 && p_nBuf > 0; n++)
       //     {
        //        p_nBuf--;
        //        MIL.MbufFree(m_MilBuffers[p_nBuf]);
        //    }
            p_CamInfo.p_eState = eCamState.Ready;
            //m_ImageLive.ReAllocate(new CPoint(p_nWidth, p_nHeight), 1);
            return;
        }

        void DisconnectCamera()
        {
            // variable

            // implement
            for (int i = 0; i<p_nBuf; i++)
            {
                if (m_MilBuffers[i] != MIL.M_NULL)
                {
                    MIL.MbufFree(m_MilBuffers[i]);
                    m_MilBuffers[i] = MIL.M_NULL;
                }
            }
            if (m_MilDisplay != MIL.M_NULL)
            {
                MIL.MdispFree(m_MilDisplay);
                m_MilDisplay = MIL.M_NULL;
            }
            if (m_MilDigitizer != MIL.M_NULL)
            {
                MIL.MdigFree(m_MilDigitizer);
                m_MilDigitizer = MIL.M_NULL;
            }
            if (m_MilSystem != MIL.M_NULL)
            {
                MIL.MsysFree(m_MilSystem);
                m_MilSystem = MIL.M_NULL;
            }
            GC.SuppressFinalize(this);
        }

        public void LiveGrab()
        {
            // variable
            UserDataObject userObject = new UserDataObject();

            MIL_INT licenseModules = 0;
            MIL_INT frameCount = 0;
            MIL_INT frameMissed = 0;
            MIL_INT compressAttribute = 0;

            // implement
            if (m_MilApplication == MIL.M_NULL) return;
            if (m_MilSystem == MIL.M_NULL) return;
            if (m_MilDisplay == MIL.M_NULL) return;
            if (m_MilDigitizer == MIL.M_NULL) return;
            for (int i = 0; i < p_nBuf; i++)
            {
                if (m_MilBuffers[i] == MIL.M_NULL) return;
            }

            userObject.NbGrabStart = 0;
            GCHandle userObjectHandle = GCHandle.Alloc(userObject);

            MIL_DIG_HOOK_FUNCTION_PTR grabStartDelegate = new MIL_DIG_HOOK_FUNCTION_PTR(ArchiveFunction);
            MIL.MdigHookFunction(m_MilDigitizer, MIL.M_GRAB_END, grabStartDelegate, GCHandle.ToIntPtr(userObjectHandle));
            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS);

            int iBlock = 0;
            while (iBlock < 10)
            {
                Thread.Sleep(100);
                MIL.MdigGrab(m_MilDigitizer, m_MilBuffers[(iBlock) % p_nBuf]);
                iBlock++;
            }

            MIL.MdigHookFunction(m_MilDigitizer, MIL.M_GRAB_END + MIL.M_UNHOOK, grabStartDelegate, GCHandle.ToIntPtr(userObjectHandle));
            userObjectHandle.Free();
        }

        MIL_INT ArchiveFunction(MIL_INT HookType, MIL_ID EventId, IntPtr UserDataPtr)
        {
            // variable

            // implement
            // 1-Frame 채워지면 들어오는 Callback함수 -> Frame Buffer에서 User Buffer로 복사하는 코드 넣어야 함
            if (UserDataPtr != IntPtr.Zero)
            {
                GCHandle userObjectHandle = GCHandle.FromIntPtr(UserDataPtr);
                UserDataObject userData = userObjectHandle.Target as UserDataObject;

                if (userData != null)
                {
                    MIL.MbufGet2d(m_MilBuffers[(userData.NbGrabStart) % p_nBuf], 0, 0, p_nWidth, p_nHeight, m_ImageLive.m_aBuf);
                    if (_dispatcher != null)
                    {
                        _dispatcher.Invoke(new Action(delegate ()
                        {
                            m_ImageLive.UpdateImage();
                        }));
                    }
                    userData.NbGrabStart++;
                }
            }
            return 0;
        }

        public double GetFps()
        {
            return 0;
        }

        public CPoint GetRoiSize()
        {
            return new CPoint(p_nWidth, p_nHeight);
        }
        GrabData m_GrabData;
        int m_nGrabCount = 0;
        int m_nGrabTrigger = 0;
        Thread m_GrabThread;
        MemoryData m_Memory;//Raw
        MemoryData m_MemBright;
        MemoryData m_MemHeight;
        MemoryData m_MemConv;
        MemoryData m_MemRaw;
        IntPtr m_MemPtr = IntPtr.Zero;
        CPoint m_cpScanOffset = new CPoint();
        GCHandle userObjectHandle;

        private const int BUFFERING_SIZE_MAX = 20;
        MIL_DIG_HOOK_FUNCTION_PTR grabStartDelegate = new MIL_DIG_HOOK_FUNCTION_PTR(LineScanArchiveFunction);
        UserDataObject userObject = new UserDataObject();
        public void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null, bool bTest = false)
        {
            m_nGrabCount = (int)Math.Truncate(1.0 * nLine / p_nHeight);
            m_nGrabTrigger = 0;
            m_Memory = memory;
            m_MemPtr = memory.GetPtr();
            m_cpScanOffset = cpScanOffset;
            p_CamInfo.p_eState = eCamState.GrabMem;
            m_nOffset = m_GrabData.nScanOffsetY;

            MIL_INT licenseModules = 0;
            MIL_INT frameCount = 0;
            MIL_INT frameMissed = 0;
            MIL_INT compressAttribute = 0;

            // implement
            if (m_MilApplication == MIL.M_NULL)
                return;
            if (m_MilSystem == MIL.M_NULL)
                return;
            if (m_MilDisplay == MIL.M_NULL)
                return;
            if (m_MilDigitizer == MIL.M_NULL)
                return;
            for (int i = 0; i < p_nBuf; i++)
            {
                if (m_MilBuffers[i] == MIL.M_NULL)
                    return;
            }
           
            grabStartDelegate = new MIL_DIG_HOOK_FUNCTION_PTR(LineScanArchiveFunction);
            userObjectHandle = GCHandle.Alloc(this);
            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_TIMEOUT, MIL.M_INFINITE); // Grab 대기 시간
            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS);
            MIL.MdigProcess(m_MilDigitizer, m_MilBuffers, m_nGrabCount, MIL.M_SEQUENCE + MIL.M_COUNT(m_nGrabCount), 
                MIL.M_ASYNCHRONOUS + MIL.M_TRIGGER_FOR_FIRST_GRAB, grabStartDelegate, GCHandle.ToIntPtr(userObjectHandle));
            //MdigProcess(MilDigitizer, MilGrabBuf, GRAB_NUM, M_SEQUENCE + M_COUNT(TOTAL_FRAME_NUM), M_ASYNCHRONOUS + M_TRIGGER_FOR_FIRST_GRAB, ProcessingFunction, &UserHookData);

            m_GrabThread = new Thread(new ThreadStart(RunGrabLineScanThread));
            m_GrabThread.Start();
            return;
        }
        public void GrabZScan(MemoryData memory, int GrabCnt,CPoint memoffset)
        {
            m_nGrabCount = GrabCnt;
            m_nGrabTrigger = 0;
            m_Memory = memory;

            p_CamInfo.p_eState = eCamState.GrabMem;

            MIL_INT licenseModules = 0;
            MIL_INT frameCount = 0;
            MIL_INT frameMissed = 0;
            MIL_INT compressAttribute = 0;

            // implement
            if (m_MilApplication == MIL.M_NULL)
                return;
            if (m_MilSystem == MIL.M_NULL)
                return;
            if (m_MilDisplay == MIL.M_NULL)
                return;
            if (m_MilDigitizer == MIL.M_NULL)
                return;
            for (int i = 0; i < p_nBuf; i++)
            {
                if (m_MilBuffers[i] == MIL.M_NULL)
                    return;
            }
            
            userObjectHandle = GCHandle.Alloc(this);
            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_TIMEOUT, MIL.M_INFINITE); // Grab 대기 시간
            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS);
            MIL.MdigProcess(m_MilDigitizer, m_MilBuffers, m_nGrabCount, MIL.M_SEQUENCE + MIL.M_COUNT(m_nGrabCount), 
                MIL.M_ASYNCHRONOUS + MIL.M_TRIGGER_FOR_FIRST_GRAB, grabStartDelegate, GCHandle.ToIntPtr(userObjectHandle));

            m_GrabThread = new Thread(new ParameterizedThreadStart(RunGrabZScanThread));
            m_GrabThread.Start(new ScanParam(memory,memoffset));
            return;
        }
        public class ScanParam
        {
            public MemoryData mem;
            public CPoint memoffset;
            public ScanParam(MemoryData mem, CPoint memoffset)
            {
                this.mem = mem;
                this.memoffset = memoffset;
            }
        }

        public void Grab3DScan(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData GrabData = null, bool bTest = false)
        {
            userObjectHandle = GCHandle.Alloc(this);
            MIL.MdigProcess(m_MilDigitizer, m_MilBuffers, m_nGrabCount, MIL.M_STOP, MIL.M_ASYNCHRONOUS + MIL.M_TRIGGER_FOR_FIRST_GRAB, grabStartDelegate, GCHandle.ToIntPtr(userObjectHandle));
            m_GrabData = GrabData;
            m_nGrabCount = nLine;// (int)Math.Truncate(1.0 * nLine / p_nHeight);
            m_nGrabTrigger = 0;
            m_Memory = memory;
            m_MemPtr = memory.GetPtr();
            m_cpScanOffset = cpScanOffset;
            p_CamInfo.p_eState = eCamState.GrabMem;

            MIL_INT licenseModules = 0;
            MIL_INT frameCount = 0;
            MIL_INT frameMissed = 0;
            MIL_INT compressAttribute = 0;

            // implement
            if (m_MilApplication == MIL.M_NULL)
                return;
            if (m_MilSystem == MIL.M_NULL)
                return;
            if (m_MilDisplay == MIL.M_NULL)
                return;
            if (m_MilDigitizer == MIL.M_NULL)
                return;
            for (int i = 0; i < p_nBuf; i++)
            {
                if (m_MilBuffers[i] == MIL.M_NULL)
                    return;
            }

            //m_nGrabCount = 300;


             grabStartDelegate = new MIL_DIG_HOOK_FUNCTION_PTR(LineScanArchiveFunction);
           
            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_TIMEOUT, MIL.M_INFINITE); // Grab 대기 시간
            MIL.MdigControl(m_MilDigitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS);
            MIL.MdigProcess(m_MilDigitizer, m_MilBuffers, c_nBuf, MIL.M_START, MIL.M_ASYNCHRONOUS + MIL.M_TRIGGER_FOR_FIRST_GRAB, grabStartDelegate, GCHandle.ToIntPtr(userObjectHandle));

            if (m_GrabData != null)
                m_clr3D.MakeImageSimple(m_cpScanOffset.X, m_cpScanOffset.Y, 30, 30, 32, m_nGrabCount, 0, m_GrabData.m_nOverlap, 0, 0, false, false, 0, 0);


            m_GrabThread = new Thread(new ThreadStart(RunGrab3DScanThread));
            m_GrabThread.Start();
            return;
        }
      //  byte[][] m_srcarray = null;
        static void CopyToMem(byte[] s, IntPtr d, int sz)
        {
            Marshal.Copy(s, 0, d,sz);
        }
        unsafe void RunGrab3DScanThread()
        {
            try
            {
              
                StopWatch swGrab = new StopWatch();
                int DelayGrab = (int)(1000 * m_nGrabCount);
                byte[] srcarray = new byte[p_nWidth * p_nHeight];

                int lY = m_nGrabCount * Convert.ToInt32(p_nHeight);
                int iBlock = 0;
                m_bGrabThreadOn = true;
                const int nTimeOut_10s = 10000; //ms            
                const int nTimeOutInterval = 10; // ms
                int nScanAxisTimeOut = nTimeOut_10s / nTimeOutInterval;
                int previBlock = 0;
                byte** ppDst = m_clr3D.GetFGBuffer();
                while (iBlock < m_nGrabCount)
                {
                    if (previBlock == iBlock)
                    {
                        Thread.Sleep(nTimeOutInterval);
                        if (--nScanAxisTimeOut <= 0)
                        {
                            m_log.Info("TimeOut - RunGrabLineColorScanThread" + iBlock.ToString()+" / "+ m_nGrabCount.ToString());
                           
                            break;
                        }

                    }
                    else
                    {
                        previBlock = iBlock;
                        nScanAxisTimeOut = nTimeOut_10s / nTimeOutInterval;
                    }

                    if(iBlock < m_nGrabTrigger)
                    {
                        
                     //   MIL.MbufGet2d(m_MilBuffers[(iBlock) % p_nBuf], 0, 0, p_nWidth, p_nHeight,srcarray);
                     //   int yp = (iBlock) * p_nHeight;
                     //   IntPtr dstPtr = (IntPtr)((long)m_MemPtr + 0 + (yp + 0) * (long)m_Memory.W);
                      //  Task.Run(() => CopyToMem(srcarray, dstPtr, p_nWidth * p_nHeight));
                     //   Marshal.Copy(srcarray, 0, dstPtr, p_nWidth * p_nHeight);
                       
                       /* fixed (byte* p = srcarray)
                        {
                           
                            IntPtr srcPtr = (IntPtr)(p);
                            IntPtr dstPtr = (IntPtr)((long)m_MemPtr + 0 + (yp + 0) * (long)m_Memory.W);

                            Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, p_nWidth* p_nHeight, p_nWidth * p_nHeight);
                        }
                       
                      
                        
                        Parallel.For(0, p_nHeight, (y) =>
                        //  for(int y = 0;y< p_nHeight;y++)
                        {
                            int yp = y + (iBlock) * p_nHeight;
                            fixed (byte* p = srcarray)
                            {
                                IntPtr srcPtr = (IntPtr)(p + p_nWidth * y);
                                IntPtr dstPtr = (IntPtr)((long)m_MemPtr + 0+ (yp +0) * (long)m_Memory.W);
                         
                                 Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, p_nWidth, p_nWidth);
                            }
                        }
                      );

                        int ypp = (iBlock) * p_nHeight;
                        ppDst[iBlock] = (byte*)((long)m_MemPtr + 0 + (ypp + 0) * (long)m_Memory.W);
                        m_clr3D.SetFrameNum(iBlock);
                       */
                        iBlock++;

                        //GrabEvent();
                        //if (m_nGrabCount != 0)
                        //    p_nGrabProgress = Convert.ToInt32((double)iBlock * 100 / m_nGrabCount);
                    }
                }
            }
            catch (Exception ex)
            {
                m_log.Info(ex.Message);
                m_log.Info(ex.StackTrace);
            }
            finally
            {
                MIL.MdigProcess(m_MilDigitizer, m_MilBuffers, c_nBuf, MIL.M_STOP, MIL.M_DEFAULT, grabStartDelegate, GCHandle.ToIntPtr(userObjectHandle));
                p_CamInfo.p_eState = eCamState.Ready;
           //     userObjectHandle.Free();
            }
        }
        unsafe void RunGrabZScanThread(object scanParam)
        {
            ScanParam param = (ScanParam)scanParam;
            Stopwatch swGrab = new Stopwatch();
            int DelayGrab = 1000 * m_nGrabCount;
            byte[] srcarr = new byte[p_nWidth * p_nHeight];

            int iBlock = 0;
            MemoryData m = param.mem;
            Debug.WriteLine("memoffset X : " + param.memoffset.X + "memoffset Y : "+param.memoffset.Y);
            try
            {
                while (iBlock < m_nGrabCount)
                {
                    if (iBlock >= m_nGrabTrigger) continue;
                    Thread.Sleep(200);
                    m_MemPtr = m.GetPtr(iBlock);

                    Debug.WriteLine("line : " + iBlock);
                    MIL.MbufGet2d(m_MilBuffers[(iBlock) % p_nBuf], 0, 0, p_nWidth, p_nHeight, srcarr);
                    Parallel.For(0, p_nHeight, (y) =>
                    {
                        fixed (byte* p = srcarr)
                        {
                            IntPtr srcPtr = (IntPtr)p + p_nWidth * y;
                            IntPtr dstPtr = (IntPtr)((long)m_MemPtr + param.memoffset.X + (param.memoffset.Y + y) * m_Memory.W);
                            Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, p_nWidth, p_nWidth);
                        }
                    });
                    iBlock++;
                }
            }
            finally
            {
                p_CamInfo.p_eState = eCamState.Ready;
               // userObjectHandle.Free();
            }

        }
        unsafe void RunGrabLineScanThread()
        {
            StopWatch swGrab = new StopWatch();
            int DelayGrab = (int)(1000 * m_nGrabCount);
            byte[] srcarray = new byte[p_nWidth * p_nHeight];

            int lY = m_nGrabCount * Convert.ToInt32(p_nHeight);
            int iBlock = 0;
            while (iBlock < m_nGrabCount)
            {
                if (iBlock < m_nGrabTrigger)
                {   
                    MIL.MbufGet2d(m_MilBuffers[(iBlock) % p_nBuf], 0, 0, p_nWidth, p_nHeight, srcarray);
                    Parallel.For(0, p_nHeight, (y) =>
                    {
                        int yp = y + (iBlock) * p_nHeight + m_nOffset;
                        fixed (byte* p = srcarray)
                        {
                            IntPtr srcPtr = (IntPtr)p + p_nWidth * y;
                            IntPtr dstPtr = (IntPtr)((long)m_MemPtr + m_cpScanOffset.X + (yp + m_cpScanOffset.Y) * (long)m_Memory.W);
                            Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, p_nWidth, p_nWidth);
                        }
                    });
                    iBlock++;
                    //GrabEvent();
                    //if (m_nGrabCount != 0)
                    //    p_nGrabProgress = Convert.ToInt32((double)iBlock * 100 / m_nGrabCount);
                }
            }
            p_CamInfo.p_eState = eCamState.Ready;
            //userObjectHandle.Free();
        }

        void GrabEvent()
        {
            if (Grabed != null)
                OnGrabed(new GrabedArgs(null, m_nGrabTrigger, new CRect(), p_nGrabProgress));
        }
        protected virtual void OnGrabed(GrabedArgs e)
        {
            if (Grabed != null)
                Grabed.Invoke(this, e);
        }

        public static MIL_INT LineScanArchiveFunction(MIL_INT HookType, MIL_ID EventId, IntPtr UserDataPtr)
        {
            if (UserDataPtr != IntPtr.Zero)
            {
                GCHandle handle = (GCHandle)(UserDataPtr);
                Camera_Matrox cam = handle.Target as Camera_Matrox;
                if (cam != null)
                {
                    if (cam.m_nGrabTrigger < cam.m_nGrabCount)
                    {
                        MIL_ID nCurrBufferId = new MIL_ID();
                        MIL.MdigGetHookInfo(EventId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref nCurrBufferId);

                        MIL_INT pBufferAddress = new MIL_INT();
                        MIL.MbufInquire(nCurrBufferId, MIL.M_HOST_ADDRESS, ref pBufferAddress);
                        unsafe
                        {
                            byte** ppDst = cam.m_clr3D.GetFGBuffer();
                            ppDst[cam.m_nGrabTrigger] = (byte*)((long)pBufferAddress);// + cam.m_nHeight * cam.p_nWidth * );
                            cam.m_clr3D.SetFrameNum(cam.m_nGrabTrigger);

                        }

                        //  Debug.WriteLine("Trigger : " + cam.m_nGrabTrigger);
                        cam.m_nGrabTrigger++;
                    }
                }

            }
            return 0;
        }

        public string StartGrab()
        {
            if (m_MilDigitizer == MIL.M_NULL)
                return "Digitizer is NULL";
            MIL.MdigGrabContinuous(m_MilDigitizer, m_MilBuffers[0]);
            return "OK";
        }

        public string StopGrab()
        {
            if (m_MilDigitizer == MIL.M_NULL)
                return "Digitizer is NULL";
            MIL.MdigHalt(m_MilDigitizer);
            MIL.MdigProcess(m_MilDigitizer, m_MilBuffers, m_nGrabCount, MIL.M_STOP, MIL.M_DEFAULT, grabStartDelegate, GCHandle.ToIntPtr(userObjectHandle));
            return "OK";
        }

        public void ThreadStop()
        {
            return;
        }

        public void GrabLineScanColor(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null) { }

        #region RelayCommand
        public RelayCommand ConnectCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    if (!bgw_Connect.IsBusy)
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
                    DisconnectCamera();
                });
            }
        }

        public RelayCommand LiveGrabCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    Thread th = new Thread(new ThreadStart(LiveGrab));
                    th.Start();
                });
            }
        }

        public RelayCommand StopGrabCommand
        {
            get
            {
                return new RelayCommand(delegate
                {
                    StopGrab();
                });
            }

        }
        #endregion
    }
}
