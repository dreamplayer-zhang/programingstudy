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

namespace RootTools.Camera.Matrox
{
    public class Camera_Matrox : ObservableObject, ICamera
    {
        public event EventHandler Grabed;
        BackgroundWorker bgw_Connect = new BackgroundWorker();

        public MIL_ID m_milApplication;
        public MIL_ID m_milSystem;
        public MIL_ID m_milDigitizer;
        public MIL_ID m_milDisplay;
        public MIL_ID m_milBuffer;

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
            return;
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
            MIL.MappAlloc(MIL.M_DEFAULT, ref m_milApplication);
            MIL.MsysAlloc(MIL.M_SYSTEM_SOLIOS, MIL.M_DEV0, MIL.M_DEFAULT, ref m_milSystem);

            return;
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
        #endregion
    }
}
