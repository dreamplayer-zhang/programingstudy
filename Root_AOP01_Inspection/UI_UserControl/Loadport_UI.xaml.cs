using System;
using System.Windows;
using System.Windows.Controls;
using RootTools;
using RootTools.Gem;
using RootTools.Module;
using Root_EFEM.Module;
using System.Windows.Threading;
using System.Windows.Media;
using System.ComponentModel;
using Root_AOP01_Inspection.UI._3._RUN;
using System.Threading;
using RootTools.OHTNew;
using Root_AOP01_Inspection.Module;

namespace Root_AOP01_Inspection.UI_UserControl
{
    /// <summary>
    /// Loadport_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Loadport_UI : UserControl
    {
        ManualJobSchedule m_manualjob;
        public Loadport_UI()
        {
            InitializeComponent();
        }

        AOP01_Engineer m_engineer;
        AOP01_Handler m_handler;
        Loadport_AOP01 m_loadport;
        InfoCarrier m_infoCarrier;
        RFID_Brooks m_rfid;

        public void Init(ILoadport loadport, AOP01_Engineer engineer, IRFID rfid)
        {
            m_loadport = (Loadport_AOP01)loadport;
            m_infoCarrier = m_loadport.p_infoCarrier;
            
            m_engineer = engineer;
            m_handler = engineer.m_handler;
            m_rfid = (RFID_Brooks)rfid;
            this.DataContext = loadport;

            textBoxPodID.DataContext = loadport.p_infoCarrier;
            textBoxLotID.DataContext = loadport.p_infoCarrier.m_aGemSlot[0];
            textBoxSlotID.DataContext = loadport.p_infoCarrier.m_aGemSlot[0];
            textBoxRecipe.DataContext = loadport.p_infoCarrier.m_aGemSlot[0];
            InitButtonLoad();
            InitTimer();

            m_manualjob = new ManualJobSchedule(m_loadport, m_engineer, m_infoCarrier);
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }
        private void M_timer_Tick(object sender, EventArgs e)
        {
            Placed.Background = m_loadport.m_diPlaced.p_bIn == false ? Brushes.SteelBlue : Brushes.LightGray;
            Present.Background = m_loadport.p_diPresent.p_bIn == false ? Brushes.SteelBlue : Brushes.LightGray;
            Load.Background = m_loadport.m_bLoadCheck == true ? Brushes.SteelBlue : Brushes.LightGray;
            UnLoad.Background = m_loadport.m_bUnLoadCheck == true ? Brushes.SteelBlue : Brushes.LightGray;
            Alarm.Background = m_loadport.p_eState == ModuleBase.eState.Error ? Brushes.Red : Brushes.LightGray;
            ButtonLoad.IsEnabled = IsEnableLoad();            
            ButtonUnLoadReq.IsEnabled = IsEnableUnloadReq();  
        }


            BackgroundWorker m_bgwLoad = new BackgroundWorker();
        void InitButtonLoad()
        {
            m_bgwLoad.DoWork += M_bgwLoad_DoWork;
            m_bgwLoad.RunWorkerCompleted += M_bgwLoad_RunWorkerCompleted;
        }
        private void M_bgwLoad_DoWork(object sender, DoWorkEventArgs e)
        {

        }
        private void M_bgwLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            switch (m_loadport.p_eState)
            {
                case ModuleBase.eState.Ready:
                    if (m_manualjob.SetInfoPod() != "OK") return;
                    Thread.Sleep(100);
                    EQ.p_eState = EQ.eState.Run; 
                    break;
            }
        }
        bool IsEnableLoad()
        {
            bool bReadyLoadport = (m_loadport.p_eState == ModuleBase.eState.Ready);
            bool bReadyToLoad = (m_loadport.p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad);
            bReadyToLoad = true;
            bool bReadyState = (m_loadport.m_qModuleRun.Count == 0);
            bool bEQReadyState = (EQ.p_eState == EQ.eState.Ready || EQ.p_eState == EQ.eState.Idle);
            if (m_loadport.p_infoCarrier.p_eState != InfoCarrier.eState.Placed) return false;
            bool bPlaced = m_loadport.CheckPlaced();

            if (m_handler.IsEnableRecovery() == true) return false;
            return bReadyLoadport && bReadyToLoad && bReadyState && bEQReadyState && bPlaced;
        }

        public static string sLoadportNum;
        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            sLoadportNum = m_loadport.p_id;
            if (IsEnableLoad() == false) return;
            if (m_loadport.p_id == "LoadportA") EQ.p_nRunLP = 0;
            else if (m_loadport.p_id == "LoadportB") EQ.p_nRunLP = 1;
            ModuleRunBase moduleRun = m_rfid.m_runReadID.Clone();
            m_rfid.StartRun(moduleRun);
            while ((EQ.IsStop() != true) && m_rfid.IsBusy()) Thread.Sleep(10);
            m_loadport.RunDocking();
            if (m_loadport.p_infoCarrier.m_aInfoWafer[0] == null)
            {
                m_loadport.m_alidEmpty.Run(true, "Pod Empty");
                m_loadport.RunUndocking();
                return;
            }
            if (m_manualjob.ShowPopup(m_handler) == false) return;
            m_bgwLoad.RunWorkerAsync();
        }
        bool IsEnableUnloadReq()
        {
            bool bReadyLoadport = (m_loadport.p_eState == ModuleBase.eState.Ready);
            bool bReadyToUnload = (m_loadport.p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload);
            bool bAccess = (m_loadport.p_infoCarrier.p_eAccessLP == GemCarrierBase.eAccessLP.Auto);
            bool bPlaced = m_loadport.CheckPlaced();
            return bReadyLoadport && bReadyToUnload && bAccess & bPlaced;
        }
        private void ButtonUnLoadReq_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableUnloadReq() == false) return;
            //m_loadport.m_ceidUnloadReq.Send();
        }
    }
}
