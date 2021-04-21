using Root_EFEM.Module;
using Root_VEGA_D.Engineer;
using RootTools;
using RootTools.Gem;
using RootTools.Module;
using RootTools.OHTNew;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Root_VEGA_D
{
    /// <summary>
    /// Loadport_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Loadport_UI : UserControl
    {
        BackgroundWorker m_bgwLoad = new BackgroundWorker();
        DispatcherTimer m_timer = new DispatcherTimer();

        VEGA_D_Handler m_handler;
        Loadport_Cymechs m_loadport;
        ManualJobSchedule m_manualjob;
        RFID_Brooks m_rfid;

        public Loadport_UI()
        {
            InitializeComponent();
        }
        #region Timer
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            buttonLoad.IsEnabled = IsEnableLoad();
            buttonUnloadReq.IsEnabled = IsEnableUnloadReq();
        }

        bool IsEnableLoad()
        {
            bool bReadyLoadport = (m_loadport.p_eState == ModuleBase.eState.Ready);
            bool bReadyToLoad = (m_loadport.p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad);
            bReadyToLoad = true;
            bool bReadyState = (m_loadport.m_qModuleRun.Count == 0);
            bool bEQReadyState = (EQ.p_eState == EQ.eState.Ready);
            //if (m_loadport.p_infoCarrier.p_eState != InfoCarrier.eState.Placed) return false;
            bool bPlaced = m_loadport.CheckPlaced();

            if (m_handler.IsEnableRecovery() == true) return false;
            return bReadyLoadport && bReadyToLoad && bReadyState && bEQReadyState && bPlaced;
        }

        bool IsEnableUnloadReq()
        {
            bool bReadyLoadport = (m_loadport.p_eState == ModuleBase.eState.Ready);
            bool bReadyToUnload = (m_loadport.p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload);
            bool bAccess = (m_loadport.p_infoCarrier.p_eAccessLP == GemCarrierBase.eAccessLP.Auto);
            bool bPlaced = m_loadport.CheckPlaced();
            return bReadyLoadport && bReadyToUnload && bAccess & bPlaced;
        }
        #endregion
        public void Init(ILoadport loadport, VEGA_D_Handler handler, IRFID rfid)
        {
            m_loadport = (Loadport_Cymechs)loadport;
            m_handler = handler;
            m_rfid = (RFID_Brooks)rfid;
            this.DataContext = m_loadport;

            if(m_rfid.m_sResult != "OK")
            textBoxPodID.DataContext = loadport.p_infoCarrier;
            else
            { textBoxPodID.IsEnabled = false; }
            textBoxLotID.DataContext = loadport.p_infoCarrier.m_aGemSlot[0];
            textBoxRecipeID.DataContext = loadport.p_infoCarrier.m_aGemSlot[0];

            InitTimer();
            m_bgwLoad.DoWork += M_bgwLoad_DoWork;
            m_bgwLoad.RunWorkerCompleted += M_bgwLoad_RunWorkerCompleted;
            InfoCarrier infoCarrier = m_loadport.p_infoCarrier;
            infoCarrier.m_aInfoWafer[0] = (InfoWafer)infoCarrier.m_aGemSlot[0];
            infoCarrier.m_aInfoWafer[0].p_eState = GemSlotBase.eState.Exist;
            m_manualjob = new ManualJobSchedule(m_handler.m_engineer,m_loadport,infoCarrier);
        }

        private void M_bgwLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            switch (m_loadport.p_eState)
            {
                case ModuleBase.eState.Ready:
                    if (m_manualjob.CheckInfoPod() != "OK") return;
                    Thread.Sleep(100);
                    //InfoCarrier infoCarrier = m_loadport.p_infoCarrier;
                    //ManualJobSchedule manualJob = new ManualJobSchedule(infoCarrier);
                    //manualJob.ShowPopup();
                    EQ.p_eState = EQ.eState.Run;
                    break;
            }
        }

        private void M_bgwLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            while ((EQ.IsStop() != true) && m_loadport.IsBusy()) Thread.Sleep(10);
            Thread.Sleep(100);
        }

        #region Button Click Event
        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableLoad() == false) return;
            if (m_loadport.p_id == "LoadportA") EQ.p_nRunLP = 0;
            else if (m_loadport.p_id == "LoadportB") EQ.p_nRunLP = 1;
            //ModuleRunBase moduleRun = m_rfid.m_runReadID.Clone();
            //m_rfid.StartRun(moduleRun);
            //while ((EQ.IsStop() != true) && m_rfid.IsBusy()) Thread.Sleep(10);
            m_loadport.p_open = false;
            ModuleRunBase Docking = m_loadport.m_runDocking.Clone();
            m_loadport.StartRun(Docking);
            //m_loadport.RunDocking();
            if (m_loadport.p_infoCarrier.m_aInfoWafer[0] == null)
            {
                m_loadport.m_alidInforeticle.Run(true, "Reticle Info Error, Check Reticle in Loadport");
                ModuleRunBase UnDocking = m_loadport.m_runUndocking.Clone();
                m_loadport.StartRun(UnDocking);
                //m_loadport.RunUndocking();
                return;
            }
            if (m_manualjob.ShowPopup(m_handler) == false) return;
            m_bgwLoad.RunWorkerAsync();
        }

        private void buttonUnloadReq_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
