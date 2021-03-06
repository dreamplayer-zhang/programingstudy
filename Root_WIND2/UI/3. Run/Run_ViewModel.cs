using Root_EFEM.Module;
using Root_WIND2.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Module;
using RootTools.OHTNew;
using RootTools_Vision;
using RootTools.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;

namespace Root_WIND2
{
    class Run_ViewModel : ObservableObject
    {
        #region Module
        Loadport_RND m_loadport1;
        public Loadport_RND p_LoadPort1
        {
            get
            {
                return m_loadport1;
            }
            set
            {
                SetProperty(ref m_loadport1, value);
            }
        }
        Loadport_RND m_loadport2;
        public Loadport_RND p_LoadPort2
        {
            get
            {
                return m_loadport2;
            }
            set
            {
                SetProperty(ref m_loadport2, value);
            }
        }
        Aligner_RND m_Aligner;
        public Aligner_RND p_Aligner
        {
            get
            {
                return m_Aligner;
            }
            set
            {
                SetProperty(ref m_Aligner, value);
            }
        }
        WTR_RND m_WTR;
        public WTR_RND p_WTR
        {
            get
            {
                return m_WTR;
            }
            set
            {

                SetProperty(ref m_WTR, value);
            }
        }
        BackSideVision m_BackSideVision;
        public BackSideVision p_BackSideVision
        {
            get
            {
                return m_BackSideVision;
            }
            set
            {
                SetProperty(ref m_BackSideVision, value);
            }
        }
        EdgeSideVision m_EdgeVision;
        public EdgeSideVision p_EdgeVision
        {
            get { return m_EdgeVision; }
            set
            {
                SetProperty(ref m_EdgeVision, value);
            }
        }
        Vision m_Vision;
        public Vision p_Vision
        {
            get
            {
                return m_Vision;
            }
            set
            {
                SetProperty(ref m_Vision, value);
            }
        }

        public EQ.eState p_EQState
        {
            get
            {
                return EQ.m_EQ.p_eState;
            }
        }
        #endregion

        Setup_ViewModel m_SetupVM;
        RootViewer_ViewModel m_Viewer = new RootViewer_ViewModel();
        MemoryTool m_ToolMemory;
        ImageData m_imagedata;
        ObservableCollection<TK4S> m_aTK4S = new ObservableCollection<TK4S>();
        public ObservableCollection<TK4S> p_aTK4S
        {
            get
            {
                return m_aTK4S;
            }
            set
            {
                SetProperty(ref m_aTK4S, value);
            }
        }

        ObservableCollection<FFUModule> m_aFFU = new ObservableCollection<FFUModule>();
        public ObservableCollection<FFUModule> p_aFFU
        {
            get
            {
                return m_aFFU;
            }
            set
            {
                SetProperty(ref m_aFFU, value);
            }
        }


        public RootViewer_ViewModel p_Viewer
        {
            get
            {
                return m_Viewer;
            }
            set
            {
                SetProperty(ref m_Viewer, value);
            }
        }



        public Run_ViewModel(Setup_ViewModel setupvm)
        {
            m_SetupVM = setupvm;
            init();
        }
        public void init()
        {
            WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
            if (engineer.m_eMode == WIND2_Engineer.eMode.EFEM)
            {
                p_LoadPort1 = (Loadport_RND)((WIND2_Handler)(engineer.ClassHandler())).p_aLoadport[0];
                p_LoadPort2 = (Loadport_RND)((WIND2_Handler)(engineer.ClassHandler())).p_aLoadport[1];
                try
                {
                    p_Aligner = (Aligner_RND)((WIND2_Handler)(engineer.ClassHandler())).p_Aligner;
                }
                catch
                {

                }

                p_WTR = (WTR_RND)((WIND2_Handler)(engineer.ClassHandler())).p_WTR;
                p_EdgeVision = (EdgeSideVision)((WIND2_Handler)(engineer.ClassHandler())).p_EdgeSideVision;
                p_BackSideVision = (BackSideVision)((WIND2_Handler)(engineer.ClassHandler())).p_BackSideVision;
                p_Vision = (Vision)((WIND2_Handler)(engineer.ClassHandler())).p_Vision;

                m_Viewer.init(null, GlobalObjects.Instance.Get<DialogService>());
                m_ToolMemory = engineer.ClassMemoryTool();

                p_Viewer.SetImageData(p_BackSideVision.GetMemoryData(BackSideVision.ScanMemory.BackSide));
                p_ModuleList = engineer.ClassModuleList();
                p_aTK4S = ((WIND2_Handler)(engineer.ClassHandler())).p_WIND2.m_tk4s.p_aTK4S;
                p_aFFU = ((WIND2_Handler)(engineer.ClassHandler())).p_WIND2.m_FFUGourp.p_aFFU;
				bgwLoad.DoWork += BgwLoad_DoWork;
            	bgwLoad.RunWorkerCompleted += BgwLoad_RunWorkerCompleted;
            }
            else // engineer.m_eMode 가 무슨용도인지 모르겠다.. Vision 모드일 경우 p_ModuleList를 engineer로부터 못받아 오기 때문에 임시로 받아오도록 수정
            {
                p_ModuleList = engineer.ClassModuleList();
            }
        }

        public void EQHome()
        {
            EQ.p_bStop = false;
            EQ.p_eState = EQ.eState.Home;

            DatabaseManager.Instance.SelectData();
            m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;
        }

        ModuleList m_ModuleList;
        public ModuleList p_ModuleList
        {
            get
            {
                return m_ModuleList;
            }
            set
            {
                SetProperty(ref m_ModuleList, value);
            }
        }

        WIND2_Handler m_handler;
        public WIND2_Handler p_handler
        {
            get { return m_handler; }
            set
            {
                SetProperty(ref m_handler, value);
            }
        }

BackgroundWorker bgwLoad = new BackgroundWorker();

        public void LoadLoadport1CST()
        {

            bgwLoad.RunWorkerAsync();
            //WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
            ////m_SetupVM.maintVM.HandlerUI.GetModuleList_UI().ModuleListRunOpen();
            ////m_SetupVM.maintVM.HandlerUI.GetModuleList_UI().ModuleListRun();
            //((WIND2_Handler)(engineer.ClassHandler())).p_aLoadport[0].RunDocking();
            ////m_moduleRunList.OpenJob("C:\\Recipe\\RNR_ALL.RunWIND2");
        }
        //bool IsEnable_Recovery(WIND2_Handler handler)
        //{
        //    if (IsRunModule())
        //        return false;
        //    if (EQ.p_eState != EQ.eState.Ready)
        //        return false;
        //    if (EQ.p_bStop == true)
        //        return false;
        //    return m_handler.IsEnableRecovery();
        //}

        //bool IsRunModule()
        //{
        //    if (IsRunModule((Loadport_RND)m_handler.m_aLoadport[0]))
        //        return true;
        //    if (IsRunModule((Loadport_RND)m_handler.m_aLoadport[1]))
        //        return true;
        //    if (IsRunModule(m_handler.m_wtr))
        //        return true;
        //    if (IsRunModule(m_handler.m_Aligner))
        //        return true;
        //    if (IsRunModule(m_handler.m_camellia))
        //        return true;
        //    return false;
        //}

        //bool IsRunModule(ModuleBase module)
        //{
        //    if (module.p_eState == ModuleBase.eState.Run)
        //        return true;
        //    if (module.p_eState == ModuleBase.eState.Home)
        //        return true;
        //    return (module.m_qModuleRun.Count > 0);
        //}

        public void RecoveryCommand()
        {
            WIND2_Handler handler = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler());
            //if (IsEnable_Recovery(handler) == false)
            //    return;
            handler.CalcRecover();
            EQ.p_bStop = false;
            EQ.p_eState = EQ.eState.Run;
            EQ.p_bRecovery = true;
        }
        private void BgwLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void BgwLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
            ((WIND2_Handler)engineer.ClassHandler()).bLoad = true;
            //m_SetupVM.maintVM.HandlerUI.GetModuleList_UI().ModuleListRunOpen();
            //m_SetupVM.maintVM.HandlerUI.GetModuleList_UI().ModuleListRun();
            ((WIND2_Handler)(engineer.ClassHandler())).p_aLoadport[0].RunDocking();//kkkk
            //m_moduleRunList.OpenJob("C:\\Recipe\\RNR_ALL.RunWIND2");
        }

        public void FuncBackSideImageView()
        {
            m_imagedata = p_BackSideVision.GetMemoryData(BackSideVision.ScanMemory.BackSide);
            m_imagedata.p_nByte = 1;
            m_imagedata.p_nPlane = 3;
            p_Viewer.SetImageData(m_imagedata);
        }

        public void FuncEdgeTopImageView()
        {
            m_imagedata = p_EdgeVision.GetMemoryData(EdgeSideVision.EDGE_TYPE.EdgeTop);
            m_imagedata.p_nByte = 1;
            m_imagedata.p_nPlane = 3;
            p_Viewer.SetImageData(m_imagedata);
        }
        public void FuncEdgeSideImageView()
        {
            m_imagedata = p_EdgeVision.GetMemoryData(EdgeSideVision.EDGE_TYPE.EdgeSide);
            m_imagedata.p_nByte = 1;
            m_imagedata.p_nPlane = 3;
            p_Viewer.SetImageData(m_imagedata);
        }
        public void FuncEdgeBtmImageView()
        {
            m_imagedata = p_EdgeVision.GetMemoryData(EdgeSideVision.EDGE_TYPE.EdgeBottom);
            m_imagedata.p_nByte = 1;
            m_imagedata.p_nPlane = 3;
            p_Viewer.SetImageData(m_imagedata);
        }
        public void FuncEBRImageView()
        {
            m_imagedata = p_EdgeVision.GetMemoryData(EdgeSideVision.EDGE_TYPE.EBR);
            m_imagedata.p_nByte = 1;
            m_imagedata.p_nPlane = 3;
            p_Viewer.SetImageData(m_imagedata);
        }

        public ICommand btnMode
        {
            get
            {
                return new RelayCommand(() =>
                {
                    UIManager.Instance.ChangeUIMode();
                });
            }
        }

        public RelayCommand CommandHome
        {
            get
            {
                return new RelayCommand(EQHome);
            }
        }

        public RelayCommand CommandLoadCST
        {
            get { return new RelayCommand(LoadLoadport1CST); }
        }
        public RelayCommand CommandBackSideImageView
        {
            get
            {
                return new RelayCommand(FuncBackSideImageView);
            }
        }

        public RelayCommand CommandEdgeTopImageView
        {
            get
            {
                return new RelayCommand(FuncEdgeTopImageView);
            }
        }
        public RelayCommand CommandEdgeSideImageView
        {
            get
            {
                return new RelayCommand(FuncEdgeSideImageView);
            }
        }
        public RelayCommand CommandEdgeBtmImageView
        {
            get
            {
                return new RelayCommand(FuncEdgeBtmImageView);
            }
        }
        public RelayCommand CommandEBRImageView
        {
            get
            {
                return new RelayCommand(FuncEBRImageView);
            }
        }

        public RelayCommand CommandRecovery
        {
            get
            {
                return new RelayCommand(RecoveryCommand);
            }
        }


        private Database_DataView_VM m_DataViewer_VM = new Database_DataView_VM();
        public Database_DataView_VM p_DataViewer_VM
        {
            get { return this.m_DataViewer_VM; }
            set { SetProperty(ref m_DataViewer_VM, value); }
        }
    }
}
