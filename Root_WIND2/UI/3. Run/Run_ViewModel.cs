using Root_EFEM.Module;
using Root_WIND2.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Module;
using RootTools.OHTNew;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
                p_BackSideVision  = (BackSideVision)((WIND2_Handler)(engineer.ClassHandler())).p_BackSideVision;
                p_Vision = (Vision)((WIND2_Handler)(engineer.ClassHandler())).p_Vision;
            
                m_Viewer.init(null, GlobalObjects.Instance.Get<DialogService>());
                m_ToolMemory = engineer.ClassMemoryTool();

                m_imagedata = new ImageData(m_ToolMemory.GetMemory("BackSide Vision.BackSide Memory", "BackSide Vision", "Main"));
                m_imagedata.p_nByte = 3;
                p_Viewer.SetImageData(m_imagedata);

                p_ModuleList = engineer.ClassModuleList();
                p_aTK4S = ((WIND2_Handler)(engineer.ClassHandler())).p_WIND2.m_tk4s.p_aTK4S;
            }
        }

        public void EQHome()
        {
            EQ.p_bStop = false;
            EQ.p_eState = EQ.eState.Home;
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

        public void LoadLoadport1CST()
        {
            WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
            //m_SetupVM.maintVM.HandlerUI.GetModuleList_UI().ModuleListRunOpen();
            //m_SetupVM.maintVM.HandlerUI.GetModuleList_UI().ModuleListRun();
            ((WIND2_Handler)(engineer.ClassHandler())).p_aLoadport[0].RunDocking();
            //m_moduleRunList.OpenJob("C:\\Recipe\\RNR_ALL.RunWIND2");
        }

        public void FuncBackSideImageView()
        {
            m_imagedata = new ImageData(m_ToolMemory.GetMemory("BackSide Vision.BackSide Memory", "BackSide Vision", "Main"));
            m_imagedata.p_nByte = 3;
            p_Viewer.SetImageData(m_imagedata);
        }

        public void FuncEdgeTopImageView()
        {
            m_imagedata =  p_EdgeVision.GetMemoryData(EdgeSideVision.EDGE_TYPE.EdgeTop);
            m_imagedata.p_nByte = 3;
            p_Viewer.SetImageData(m_imagedata);
        }
        public void FuncEdgeSideImageView()
        {
            m_imagedata = p_EdgeVision.GetMemoryData(EdgeSideVision.EDGE_TYPE.EdgeSide);
            m_imagedata.p_nByte = 3;
            p_Viewer.SetImageData(m_imagedata);
        }
        public void FuncEdgeBtmImageView()
        {
            m_imagedata = p_EdgeVision.GetMemoryData(EdgeSideVision.EDGE_TYPE.EdgeBottom);
            m_imagedata.p_nByte = 3;
            p_Viewer.SetImageData(m_imagedata);
        }
        public void FuncEBRImageView()
        {
            m_imagedata = p_EdgeVision.GetMemoryData(EdgeSideVision.EDGE_TYPE.EBR);
            m_imagedata.p_nByte = 3;
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

    }
}
