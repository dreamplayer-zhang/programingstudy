using Matrox.MatroxImagingLibrary;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools.Camera.Matrox
{
    public class Camera_Matrox : ObservableObject, ICamera
    {
        Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        public event EventHandler Grabed;
        BackgroundWorker bgw_Connect = new BackgroundWorker();
        ImageData m_ImageLive;

        private MIL_ID m_MilApplication;                     // Application -> 프로그램당 하나
        private MIL_ID m_MilSystem;                          // 프레임그래버
        private MIL_ID m_MilDigitizer;                       // 카메라
        private MIL_ID m_MilDisplay;                         // 디스플레이
        private MIL_ID[] m_MilBuffers = new MIL_ID[c_nBuf];  // 버퍼
        
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
            m_ImageLive = new ImageData(1920, 1080, 1);
            p_ImageViewer = new ImageViewer_ViewModel(m_ImageLive, null, _dispatcher);
            p_CamInfo = new MatroxCamInfo(m_log);
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
            p_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
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
                MIL.MsysAlloc(MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, MIL.M_DEFAULT, ref m_MilSystem);
            // Display
            if (m_MilDisplay == MIL.M_NULL)
                MIL.MdispAlloc(m_MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WINDOWED, ref m_MilDisplay);
            // Set dafault values for the image buffer in case no digitizer can be allocated
            long lImgAttributes = MIL.M_IMAGE | MIL.M_DISP | MIL.M_PROC;
            // Inquire the number of digitizers for the system
            if (m_MilSystem != MIL.M_NULL)
            {
                MIL_INT nNumberOfDigitizers = MIL.MsysInquire(m_MilSystem, MIL.M_DIGITIZER_NUM, MIL.M_NULL);
                if (nNumberOfDigitizers > 0)
                {
                    // Digitizer
                    if (m_MilDigitizer == MIL.M_NULL)
                    {
                        MIL.MdigAlloc(m_MilSystem, MIL.M_DEFAULT, /*p_CamInfo.p_sFile*/"M_DEFAULT", MIL.M_DEFAULT, ref m_MilDigitizer);

                        // Inquire the digitizer to determine the image buffer size
                        p_nImgBand = (int)MIL.MdigInquire(m_MilDigitizer, MIL.M_SIZE_BAND, MIL.M_NULL);
                        p_nWidth = (int)MIL.MdigInquire(m_MilDigitizer, MIL.M_SIZE_X, MIL.M_NULL);
                        p_nHeight = (int)MIL.MdigInquire(m_MilDigitizer, MIL.M_SIZE_Y, MIL.M_NULL);

                        // Add the M_GRAB attribute to the image buffer
                        lImgAttributes |= MIL.M_GRAB;
                    }
                }
            }
            // Buffer
            for (int i = 0; i<p_nBuf; i++)
            {
                MIL.MbufAlloc2d(m_MilSystem, (MIL_INT)p_nWidth, (MIL_INT)p_nHeight, 8 + MIL.M_UNSIGNED, lImgAttributes, ref m_MilBuffers[i]);
            }

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

        void LiveGrab()
        {
            // variable
            UserDataObject userObject = new UserDataObject();

            int nbFrames = 0;
            int n = 0;
            int nbFramesReplayed = 0;
            double frameRate = 0;
            double timeWait = 0;
            double totalReplay = 0;
            double grabScale = 1.0;

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
            while (iBlock < 10000)
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
            return new CPoint();
        }

        public void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false, int ReserveOffsetY = 0)
        {
            return;
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
            return "OK";
        }

        public void ThreadStop()
        {
            return;
        }

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
