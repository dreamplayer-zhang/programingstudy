using Matrox.MatroxImagingLibrary;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools.Camera.Matrox
{
    public class Camera_Matrox : ObservableObject, ICamera
    {
        Dispatcher _dispatcher = null;
        public event EventHandler Grabed;
        BackgroundWorker bgw_Connect = new BackgroundWorker();

        public MIL_ID m_MilApplication; // Application -> 프로그램당 하나
        public MIL_ID m_MilSystem;      // 프레임그래버
        public MIL_ID m_MilDigitizer;   // 카메라
        public MIL_ID m_MilDisplay;     // 디스플레이
        public MIL_ID m_MilImage;      // 버퍼

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
            RunImageRoiTree(treeRoot.GetTree("Buffer Image ROI"));
            return;
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

            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
            p_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
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
            // variable

            // implement
            _dispatcher = Dispatcher.CurrentDispatcher;
            // Application
            MIL.MappAlloc(MIL.M_NULL, MIL.M_DEFAULT, ref m_MilApplication);

            // System
            MIL.MsysAlloc(MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, MIL.M_DEFAULT, ref m_MilSystem);

            // Display
            MIL.MdispAlloc(m_MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WINDOWED, ref m_MilDisplay);

            // Set dafault values for the image buffer in case no digitizer can be allocated
            long lImgAttributes = MIL.M_IMAGE | MIL.M_DISP | MIL.M_PROC;

            // Inquire the number of digitizers for the system
            MIL_INT nNumberOfDigitizers = MIL.MsysInquire(m_MilSystem, MIL.M_DIGITIZER_NUM, MIL.M_NULL);
            if (nNumberOfDigitizers > 0)
            {
                // Digitizer
                MIL.MdigAlloc(m_MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref m_MilDigitizer);

                // Inquire the digitizer to determine the image buffer size
                p_nImgBand = (int)MIL.MdigInquire(m_MilDigitizer, MIL.M_SIZE_BAND, MIL.M_NULL);
                p_nWidth = (int)MIL.MdigInquire(m_MilDigitizer, MIL.M_SIZE_X, MIL.M_NULL);
                p_nHeight = (int)MIL.MdigInquire(m_MilDigitizer, MIL.M_SIZE_Y, MIL.M_NULL);

                // Add the M_GRAB attribute to the image buffer
                lImgAttributes |= MIL.M_GRAB;
            }

            // Buffer
            MIL.MbufAllocColor(m_MilSystem, (MIL_INT)p_nImgBand, (MIL_INT)p_nWidth, (MIL_INT)p_nHeight, 8 + MIL.M_UNSIGNED, lImgAttributes, ref m_MilImage);

            

            return;
        }

        MIL_INT HookHandler(MIL_INT HookType, MIL_ID EventId, IntPtr UserDataPtr)
        {
            // variable

            // implement
            // 1-Frame 채워지면 들어오는 Callback함수 -> Frame Buffer에서 User Buffer로 복사하는 코드 넣어야 함
            return 0;
        }

        void DisconnectCamera()
        {
            // variable

            // implement
            if (m_MilImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_MilImage);
                m_MilImage = MIL.M_NULL;
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

            if (m_MilApplication != MIL.M_NULL)
            {
                if (_dispatcher != null)
                {
                    _dispatcher.Invoke(new Action(delegate ()
                    {
                        MIL.MappFree(m_MilApplication);
                        m_MilApplication = MIL.M_NULL;
                    }));
                }
            }

            GC.SuppressFinalize(this);
        }

        void LiveGrab()
        {
            if (m_MilDigitizer == MIL.M_NULL || m_MilImage == MIL.M_NULL) 
                return;
            MIL.MdigGrabContinuous(m_MilDigitizer, m_MilImage);
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
                    LiveGrab();
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
