using RootTools.Memory;
using RootTools.Trees;
using SisoSDKLib;
using SiSoFramegrabber;
using PylonC.NET;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.ComponentModel;

namespace RootTools.Camera.Silicon
{
    public class Camera_Silicon : ObservableObject, ICamera
    {
        Framegrabber m_fgSiso;
        PYLON_DEVICE_HANDLE m_hDev;  // Handle for the pylon device

        Log m_log;
        TreeRoot m_treeRoot;
        ImageData m_ImageLive;
        MemoryData m_Memory;
        BackgroundWorker bgw_Connect;
        ImageViewer_ViewModel m_ImageViewer;

        bool m_bscanDir;

        int m_nDeviceIndex;
        int m_nYEnd;
        int m_nGrabTrigger;
        int m_nInverseYOffset;
        int m_nByte;
        int m_lGrab; //전체 그랩 횟수
        int m_Width;
        int m_Height;
        int m_nGrabProgress;
        int m_nSkipGrabCount;
        string m_sMCF;

        IntPtr m_MemPtr;
        IntPtr m_pBufGrab; //Frame Grabber꺼
        CRect m_LastROI;
        CPoint m_cpScanOffset;
        CPoint m_szBuf;
        Thread m_GrabThread;

        fgErrorType rc;

        //public delegate void Dele_ProgressBar(int value);
        //public event Dele_ProgressBar ProgressBarUpdate;
        #region Property
        public int p_nDeviceIndex
        {
            get
            {
                return m_nDeviceIndex;
            }
            set
            {
                SetProperty(ref m_nDeviceIndex, value);
            }
        }
        public string p_sMCF
        {
            get { return m_sMCF; }
            set
            {
                SetProperty(ref m_sMCF, value);
            }
        }
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

        public string p_id { get; set; }
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

        public CPoint p_sz
        { get => m_szBuf; set => SetProperty(ref m_szBuf, value); }
        #endregion
        #region UI
        public UserControl p_ui
        {
            get
            {
                Camera_Silicon_UI ui = new Camera_Silicon_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        public event EventHandler Grabed;

        // 생성자
        public Camera_Silicon(string id, Log log)
        {
            p_id = id;
            m_log = log;

            m_pBufGrab = IntPtr.Zero;
            m_nByte = 1;
            m_lGrab = 0;
            m_Width = 0;
            m_Height = 0;
            m_nGrabTrigger = 0;
            m_nSkipGrabCount = -1;
            rc = fgErrorType.fgeOK;

            m_szBuf = new CPoint(0, 0);
            m_fgSiso = new Framegrabber();
            m_hDev = new PYLON_DEVICE_HANDLE();
            m_ImageLive = new ImageData(640, 480);
            bgw_Connect = new BackgroundWorker();
            p_ImageViewer = new ImageViewer_ViewModel(m_ImageLive);

            p_treeRoot = new TreeRoot(id, m_log);
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
            p_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            bgw_Connect.DoWork += bgw_Connect_DoWork;
            bgw_Connect.RunWorkerCompleted += bgw_Connect_RunWorkerCompleted;
        }

        public void M_treeRoot_UpdateTree()
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

        public void RunTree(Tree treeRoot)
        {
            RunIndexTree(treeRoot.GetTree("Camera Index"));
            RunSetTree(treeRoot.GetTree("Connect Set"));
        }

        void RunIndexTree(Tree tree)
        {
            p_nDeviceIndex = tree.Set(p_nDeviceIndex, 0, "Camera Index", "Camera Index");
        }

        void RunSetTree(Tree tree)
        {
            p_sMCF = tree.SetFile(p_sMCF, p_sMCF, "MCF", "CamFile", "CamFile");
        }

        void bgw_Connect_DoWork(object sender, DoWorkEventArgs e)
        {
            ConnectCamera();
        }

        public void Connect()
        {
            if (!bgw_Connect.IsBusy)
                bgw_Connect.RunWorkerAsync();
        }

        void bgw_Connect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunTree(Tree.eMode.Init);
        }

        void ConnectCamera()
        {
            if (m_hDev != null)
            {
                if (!PylonC.NET.Pylon.DeviceIsOpen(m_hDev)) //disconnect
                {
                    Initialize();
                }
                else
                {
                    MessageBox.Show("Device is open");
                }
            }
            else
            {
                Initialize();
            }
        }

        void DisconnectCamera()
        {
            if (m_hDev != null)
            {
                if (PylonC.NET.Pylon.DeviceIsOpen(m_hDev))
                {
                    PylonC.NET.Pylon.DeviceClose(m_hDev);

                    if (m_pBufGrab != IntPtr.Zero)
                    {
                        m_fgSiso.FgFreeMemEx(m_pBufGrab);
                        m_pBufGrab = IntPtr.Zero;
                    }

                    DeleteGrabberMem();
                    m_fgSiso.FgFreeGrabber();

                    MessageBox.Show("카메라 연결 종료");
                }
            }
            else
            {
                MessageBox.Show("카메라 연결을 확인하세요");
            }
        }

        public void Initialize()
        {
            // Basler Camera Connect
            try
            {
                PylonC.NET.Pylon.Initialize();
                uint unNumbDevices = PylonC.NET.Pylon.EnumerateDevices();
                PylonCreateDevice();

                if (m_hDev == null)
                {
                    MessageBox.Show(p_nDeviceIndex.ToString("DeviceID 0 not Found!!"));
                    return;
                }

                PylonC.NET.Pylon.DeviceOpen(m_hDev, PylonC.NET.Pylon.cPylonAccessModeControl | PylonC.NET.Pylon.cPylonAccessModeStream);
                PylonC.NET.Pylon.DeviceFeatureFromString(m_hDev, "UserSetSelector", "UserSet1");
                PylonC.NET.Pylon.DeviceExecuteCommandFeature(m_hDev, "UserSetLoad");

                NODEMAP_HANDLE hNodeMap = PylonC.NET.Pylon.DeviceGetNodeMap(m_hDev);
                m_Width = m_szBuf.X = (int)GenApi.IntegerGetValue(GenApi.NodeMapGetNode(hNodeMap, "Width"));
                m_Height = m_szBuf.Y = (int)GenApi.IntegerGetValue(GenApi.NodeMapGetNode(hNodeMap, "Height"));
            }
            catch (Exception)
            {
                MessageBox.Show("Camera Init Fail");
                return;
            }

            // Silicon FrameGrabber Connect
            if (m_fgSiso.FgInitConfig(p_sMCF, (uint)p_nDeviceIndex) != fgErrorType.fgeOK)
            {
                MessageBox.Show("Frame Grabber Init Config Error");
                return;
            }

            AllocateGrabberMem(m_szBuf);
            RegisterAPC();
            SetSisoParam();

            MessageBox.Show("Camera is Connected!");
        }

        void PylonCreateDevice()
        {
            if (p_nDeviceIndex >= 0)
                m_hDev = PylonC.NET.Pylon.CreateDeviceByIndex((uint)p_nDeviceIndex);

            if (p_nDeviceIndex < 0)
                MessageBox.Show("Device Index not Defined !!");

            if (m_hDev == null)
                MessageBox.Show("Device Create Error !!");
        }

        void AllocateGrabberMem(CPoint szBuf)
        {
            if (m_szBuf == szBuf)
                return;

            DeleteGrabberMem();
            ulong lSize = (ulong)(szBuf.X * szBuf.Y * m_nByte);
            m_pBufGrab = m_fgSiso.FgAllocMemEx(lSize, m_nByte);
            m_szBuf = szBuf;

            if (m_pBufGrab == IntPtr.Zero)
                MessageBox.Show("Allocate Grabber Mem Error");
        }

        fgErrorType DeleteGrabberMem()
        {
            if (m_pBufGrab != IntPtr.Zero)
            {
                rc = m_fgSiso.FgFreeMemEx(m_pBufGrab);
                if (rc != fgErrorType.fgeOK)
                    MessageBox.Show("Delete Grabber Mem Error : " + rc.ToString());
            }

            m_pBufGrab = IntPtr.Zero;
            m_szBuf.X = m_szBuf.Y = 0;
            return fgErrorType.fgeOK;
        }

        void RegisterAPC()
        {
            FgApcControlFlags flags = FgApcControlFlags.FG_APC_CONTROL_BASIC;
            FgApcControlCtrlFlags ctrlFlags = FgApcControlCtrlFlags.FG_APC_IGNORE_STOP | FgApcControlCtrlFlags.FG_APC_IGNORE_TIMEOUTS;
            rc = m_fgSiso.FgRegisterAPCHandler(this, ApcEventHandler, (uint)p_nDeviceIndex, flags, ctrlFlags, m_pBufGrab);
        }

        public static int ApcEventHandler(object sender, APCEvent ev)//xfercallback
        {
            unsafe
            {
                Framegrabber grabber = sender as Framegrabber;
                FgAPCTransferData data = grabber.APCCallbackPins[2].Target as FgAPCTransferData;
                GCHandle handle = GCHandle.FromIntPtr(data.mReceiverObject);

                Camera_Silicon cam = handle.Target as Camera_Silicon;

                if (cam != null)
                    cam.m_nGrabTrigger++;

                //System.Diagnostics.Debug.WriteLine("m_nTrigger : " + cam.m_nGrabTrigger);
            }
            return 0;
        }

        public void SetSisoParam()
        {
            rc = m_fgSiso.FgSetParameterByName("FG_WIDTH", m_Width, (uint)p_nDeviceIndex);
            rc = m_fgSiso.FgSetParameterByName("FG_HEIGHT", m_Height, (uint)p_nDeviceIndex);
            rc = m_fgSiso.FgSetParameterByName("FG_TRIGGERSTATE", 0/*TS_ACTIVE*/, (uint)p_nDeviceIndex);
        }

        public double GetFps() => throw new NotImplementedException();

        public CPoint GetRoiSize()
        {
            return new CPoint(m_Width, m_Height);
        }
        public void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null)
        {
            AllocateGrabberMem(m_szBuf);

            PylonC.NET.Pylon.DeviceFeatureFromString(m_hDev, "TriggerMode", "On");

            m_cpScanOffset = cpScanOffset;
            m_LastROI = new CRect();
            m_Memory = memory;
            m_MemPtr = memory.GetPtr();
            m_lGrab = nLine / m_Height;
            m_nInverseYOffset = m_GrabData.ReverseOffsetY;
            m_nYEnd = ((int)Math.Truncate(1.0 * nLine / m_Height) - 1) * m_Height;
            m_cpScanOffset.Y = 0;
            m_nGrabTrigger = 0;
            m_bscanDir = m_GrabData.bInvY;
            m_nSkipGrabCount = m_GrabData.m_nSkipGrabCount;

            rc = m_fgSiso.FgAcquireEx((uint)p_nDeviceIndex, m_lGrab, (int)FgAcquisitionFlags.ACQ_STANDARD, m_pBufGrab);

            m_GrabThread = new Thread(new ThreadStart(RunGrabLineScanThread));
            m_GrabThread.Start();
        }
        unsafe void RunGrabLineScanThread()
        {
            int iBlock = 0;
            while (iBlock < m_lGrab)
            {
                if(m_nSkipGrabCount > 0)
                {
                    // TDI 카메라와 Silicon Grabber의 해상도가 달라서 트리거 갯수를 다르게 줘야해서 Skip 할 수 있도록 추가
                    if (m_nGrabTrigger % m_nSkipGrabCount != 0) //LADS를 사용할 땐 m_nGrabTrigger가 TDI 기준으로 카운트가 올라감
                        continue;
                }

                if (iBlock < m_nGrabTrigger)
                {
                    int nY = iBlock * m_Height;

                    bool Scandir = m_bscanDir;
                    IntPtr ipSrc = m_fgSiso.FgGetImagePtrEx(iBlock + 1, (uint)p_nDeviceIndex, m_pBufGrab);

                    Parallel.For(0, m_Height, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (y) =>
                    {
                        int yp;

                        if (!Scandir)
                            yp = m_nYEnd - (y + iBlock * m_Height) + m_nInverseYOffset;
                        else
                            yp = y + iBlock * m_Height;

                        IntPtr srcPtr = ipSrc + m_Width * y;
                        IntPtr dstPtr = (IntPtr)((long)m_MemPtr + m_cpScanOffset.X + (yp + m_cpScanOffset.Y) * m_Memory.W);

                        Buffer.MemoryCopy((void*)srcPtr, (void*)dstPtr, m_Width, m_Width);
                    });

                    m_LastROI.Left = m_cpScanOffset.X;
                    m_LastROI.Right = m_cpScanOffset.X + m_Width;
                    m_LastROI.Top = m_cpScanOffset.Y;
                    m_LastROI.Bottom = m_cpScanOffset.Y + m_Height;

                    iBlock++;

                    GrabEvent();
                }
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

        public string StopGrab()
        {
            unsafe
            {
                rc = m_fgSiso.FgStopAcquireEx((uint)p_nDeviceIndex, m_pBufGrab, 0);
                m_fgSiso.FgStopAcquire((uint)p_nDeviceIndex);

                if (rc != fgErrorType.fgeOK)
                    return "Stop Grab Error : " + rc.ToString();

                m_nGrabTrigger = -1;
                m_lGrab = 0;

                return "OK";
            }
        }

        public void ThreadStop()
        {
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

                });
            }
        }

        public RelayCommand StopGrabCommand
        {
            get
            {
                return new RelayCommand(delegate
                {

                });
            }
        }
        #endregion
    }
}
