
using Root_Vega.Module;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.Dalsa;
using RootTools.Memory;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Root_Vega
{
    class _4_ViewerViweModel : ObservableObject
    {
        #region Property
        private ImageViewer_ViewModel m_ImageViewer;
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

        MemoryTool m_MemoryModule;
        public MemoryTool p_MemoryModule
        {
            get
            {
                return m_MemoryModule;
            }
            set
            {
                SetProperty(ref m_MemoryModule, value);
            }
        }
        MemoryPool m_SelectedMemPool;
        public MemoryPool p_SelectedMemPool
        {
            get
            {
                return m_SelectedMemPool;
            }
            set
            {
                SetProperty(ref m_SelectedMemPool, value);
            }
        }

        MemoryGroup m_SelectedMemGroup;
        public MemoryGroup p_SelectedMemGroup
        {
            get
            {
                return m_SelectedMemGroup;
            }
            set
            {
                SetProperty(ref m_SelectedMemGroup, value);
            }
        }

        MemoryData m_SelectedMemData;
        public MemoryData p_SelectedMemData
        {
            get
            {
                return m_SelectedMemData;
            }
            set
            {
                SetProperty(ref m_SelectedMemData, value);
                if (m_SelectedMemData != null)
                    SetImageData();
            }
        }
        ImageData m_imagedata;

        List<DefectDataWrapper> m_arrDefectDataWraper;
        private SimpleShapeDrawerVM m_SimpleShapeDrawer;
        public SimpleShapeDrawerVM p_SimpleShapeDrawer
        {
            get 
            {
                return m_SimpleShapeDrawer; 
            }
            set
            {
                SetProperty(ref m_SimpleShapeDrawer, value);
            }
        }

        ICamera m_MainCamera;
        public ICamera p_MainCamera
        {
            get
            {
                return m_MainCamera;
            }
            set
            {
                SetProperty(ref m_MainCamera, value);
            }
        }
        #endregion
        Vega_Engineer m_Engineer;
        private readonly IDialogService m_DialogService;
        public BackgroundWorker Worker_ViewerUpdate = new BackgroundWorker();
        public Dispatcher _dispatcher;

        public _4_ViewerViweModel(Vega_Engineer engineer, IDialogService dialogService)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            m_Engineer = engineer;
            m_DialogService = dialogService;
            p_MemoryModule = m_Engineer.ClassMemoryTool();
            p_ImageViewer = new ImageViewer_ViewModel(null, dialogService);
            Worker_ViewerUpdate.DoWork += Worker_ViewerUpdate_DoWork;
            Worker_ViewerUpdate.WorkerReportsProgress = true;
            Worker_ViewerUpdate.ProgressChanged += Worker_ViewerUpdate_ProgressChanged;

            m_arrDefectDataWraper = new List<DefectDataWrapper>();
            p_SimpleShapeDrawer = new SimpleShapeDrawerVM(p_ImageViewer);
            p_SimpleShapeDrawer.RectangleKeyValue = System.Windows.Input.Key.D1;
            p_ImageViewer.SetDrawer((DrawToolVM)p_SimpleShapeDrawer);
            
            InitAlarmData();
        }

        public void SetImageData()
        {
            if (p_SelectedMemData == null)
                return;
            m_imagedata = new ImageData(p_SelectedMemData);
            if (p_SelectedMemData.p_id == "Main")
            {
                m_arrDefectDataWraper = m_Engineer.m_handler.m_patternVision.m_arrDefectDataWraper;
                for (int i = 0; i<m_arrDefectDataWraper.Count; i++)
                {
                    p_ImageViewer.SelectedTool.AddDefectInfo(m_arrDefectDataWraper[i]);
                }
            }
            p_ImageViewer.SetImageData(m_imagedata);
        }

        public void SetAlarm()
        {
            Random rand = new Random();
            int nrand = rand.Next(4);

            //if (nrand == 0)
            //    ((GAF_Manager)m_Engineer.ClassGAFManager()).SetAlarm(this.ToString(), eAlarm.TestAlarm1);
            //if (nrand == 1)
            //    ((GAF_Manager)m_Engineer.ClassGAFManager()).SetAlarm(this.ToString(), eAlarm.TestAlarm2);
            //if (nrand == 2)
            //    ((GAF_Manager)m_Engineer.ClassGAFManager()).SetAlarm(this.ToString(), eAlarm.TestAlarm3);
            //if (nrand == 3)
            //    ((GAF_Manager)m_Engineer.ClassGAFManager()).SetAlarm(this.ToString(), eAlarm.TestAlarm4);
        }

        enum eAlarm
        {
            TestAlarm1,
            TestAlarm2,
            TestAlarm3,
            TestAlarm4,
        }

        public void InitAlarmData()
        {
            //((GAF_Manager)m_Engineer.ClassGAFManager()).GetALID(this.ToString(), eAlarm.TestAlarm1, "Viewer Test Alarm 1", "이건 Test용으로 하는거");
            //((GAF_Manager)m_Engineer.ClassGAFManager()).GetALID(this.ToString(), eAlarm.TestAlarm2, "Viewer Test Alarm 2", "1. 공압이 떨어졌습니다. \n 2.선연결 확인해주세요.");
            //((GAF_Manager)m_Engineer.ClassGAFManager()).GetALID(this.ToString(), eAlarm.TestAlarm3, "Viewer Test Alarm 3", "1.어쩌구\n 2. 어쩌구");
            //((GAF_Manager)m_Engineer.ClassGAFManager()).GetALID(this.ToString(), eAlarm.TestAlarm4, "Viewer Test Alarm 4", "?!._-특수문자 테스트!@#$| 되는지 확인");
        }

        int nTime = 0;
        public int p_nTime
        {
            get { return nTime; }
            set
            {
                SetProperty(ref nTime, value);
            }
        }

        private void Worker_ViewerUpdate_DoWork(object sender, DoWorkEventArgs e)
        {  
        }

        void Worker_ViewerUpdate_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            m_ImageViewer.SetImageSource();
         //   m_ImageViewer.SetThumNailIamge();
        }


        public void VisionHome()
        {
            EQ.p_bStop = false;
            PatternVision vision = ((Vega_Handler)m_Engineer.ClassHandler()).m_patternVision;
            vision.p_eState = ModuleBase.eState.Home;
            if (vision.m_CamMain.p_CamInfo.p_eState == eCamState.Init)
                vision.m_CamMain.Connect();
        }

        void Stop()
        {
            EQ.p_bStop = true;
        }

        public void WholeLineScan()
        {
            EQ.p_bStop = false;
            //Vision vision = ((Vega_Handler)m_Engineer.ClassHandler()).p_vision;
            //Vision.Run_Grab Grab = (Vision.Run_Grab)vision.CloneModuleRun("Grab");
            //if (vision.m_aGrabMode.Count == 0)
            //    return;
            //GrabMode mode = vision.m_aGrabMode[0];

            //int ScanWholeLine = (int)Math.Ceiling(Grab.m_yLine * 1000 / (mode.m_camera.GetRoiSize().X * Grab.m_fRes));

            //mode.m_ScanLineNum = ScanWholeLine;
            //mode.m_ScanStartLine = 0;
            //Grab.m_grabMode = mode;
            //p_MainCamera = Grab.m_grabMode.m_camera;
            //vision.StartRun(Grab);
            //Worker_ViewerUpdate.RunWorkerAsync();
        }

        public void ManualAlign()
        {
            var viewModel = new Dialog_ManualAlignViewModel(p_SelectedMemData);
            Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
            if (result.HasValue)
            {
                if (result.Value)
                {
                }
                else
                {
                    // Cancelled
                }
            }
        }

        public void Scan()
        {
            EQ.p_bStop = false;
            PatternVision Patternvision = ((Vega_Handler)m_Engineer.ClassHandler()).m_patternVision;
            if (Patternvision.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            PatternVision.Run_Grab Grab = (PatternVision.Run_Grab)Patternvision.CloneModuleRun("Grab");
            var viewModel = new Dialog_Scan_ViewModel(Patternvision, Grab);
            Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
            if (result.HasValue)
            {
                if (result.Value)
                {
                    p_MainCamera = Grab.m_grabMode.m_camera;
                    Patternvision.StartRun(Grab);
                   // Worker_ViewerUpdate.RunWorkerAsync();
                    //ModuleList modulelist = ((Wind_Handler)m_Engineer.ClassHandler()).m_moduleList;
                    //ModuleRunList runlist = ((Wind_Handler)m_Engineer.ClassHandler()).m_moduleList.m_moduleRunList;
                    //runlist.Clear();
                    //runlist.Add(vision, Grab);
                    //EQ.p_eState = EQ.eState.Ready;
                    //modulelist.StartModuleRuns(); 
                }
                else
                {
                    // Cancelled
                }
            }
        }
        //int m_TEst = 100000;
        CPoint cpcp = new CPoint(0, 0);

        void TestFunction()
        {
        }


        #region RelayCommand
        public RelayCommand CommandVisionHome
        {
            get
            {
                return new RelayCommand(VisionHome);
            }
        }

        public RelayCommand CommandMemorySelect
        {
            get
            {
                return new RelayCommand(SetImageData);
            }
        }
        public RelayCommand CommandScan
        {
            get
            {
                return new RelayCommand(Scan);
            }
        }
        public RelayCommand CommandStop
        {
            get
            {
                return new RelayCommand(Stop);
            }
        }
        public RelayCommand CommandInspection
        {
            get
            {
                return new RelayCommand(WholeLineScan);
            }
        }

        public RelayCommand CommandManualAlign
        {
            get
            {
                return new RelayCommand(ManualAlign);
            }
        }
        public RelayCommand CommandTest
        {
            get
            {
                return new RelayCommand(TestFunction);
            }
        }
        #endregion
    }
}
