using Root_EFEM.Module;
using Root_WindII.Engineer;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WindII
{
    public class OperationUI_ViewModel : ObservableObject
    {

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

        public void Init()
        {
            WindII_Engineer engineer = GlobalObjects.Instance.Get<WindII_Engineer>();
            
                p_LoadPort1 = (Loadport_RND)((WindII_Handler)(engineer.ClassHandler())).p_aLoadport[0];
                p_LoadPort2 = (Loadport_RND)((WindII_Handler)(engineer.ClassHandler())).p_aLoadport[1];
                //try
                //{
                //    p_Aligner = (Aligner_RND)((WIND2_Handler)(engineer.ClassHandler())).p_Aligner;
                //}
                //catch
                //{

                //}

                //p_WTR = (WTR_RND)((WIND2_Handler)(engineer.ClassHandler())).p_WTR;
                //p_EdgeVision = (EdgeSideVision)((WIND2_Handler)(engineer.ClassHandler())).p_EdgeSideVision;
                //p_BackSideVision = (BackSideVision)((WIND2_Handler)(engineer.ClassHandler())).p_BackSideVision;
                //p_Vision = (Vision)((WIND2_Handler)(engineer.ClassHandler())).p_Vision;

                //m_Viewer.init(null, GlobalObjects.Instance.Get<DialogService>());
                //m_ToolMemory = engineer.ClassMemoryTool();

                //p_Viewer.SetImageData(p_BackSideVision.GetMemoryData(BackSideVision.ScanMemory.BackSide));
                //p_ModuleList = engineer.ClassModuleList();
                //p_aTK4S = ((WIND2_Handler)(engineer.ClassHandler())).p_WIND2.m_tk4s.p_aTK4S;
                //p_aFFU = ((WIND2_Handler)(engineer.ClassHandler())).p_WIND2.m_FFUGourp.p_aFFU;
            
            bgwLoad.DoWork += BgwLoad_DoWork;
        }
        public void EQHome()
        {
            EQ.p_bStop = false;
            EQ.p_eState = EQ.eState.Home;
        }

        private void BgwLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            WindII_Engineer engineer = GlobalObjects.Instance.Get<WindII_Engineer>();
            ((WindII_Handler)engineer.ClassHandler()).bLoad = true;
            //m_SetupVM.maintVM.HandlerUI.GetModuleList_UI().ModuleListRunOpen();
            //m_SetupVM.maintVM.HandlerUI.GetModuleList_UI().ModuleListRun();
            ((WindII_Handler)(engineer.ClassHandler())).p_aLoadport[0].RunDocking();//kkkk
            //m_moduleRunList.OpenJob("C:\\Recipe\\RNR_ALL.RunWIND2");
        }
        BackgroundWorker bgwLoad = new BackgroundWorker();

        public void LoadLoadport1CST()
        {

            bgwLoad.RunWorkerAsync();
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

        
    }
}
