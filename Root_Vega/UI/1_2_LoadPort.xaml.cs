using Root_Vega.ManualJob;
using Root_Vega.Module;
using RootTools;
using RootTools.Gem;
using RootTools.Module;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Vega
{
    /// <summary>
    /// _1_2_LoadPort.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class _1_2_LoadPort : UserControl
    {
        ManualJobSchedule m_manualjob;
        public _1_2_LoadPort()
        {
            InitializeComponent();
        }

        Vega_Handler m_handler;
        Loadport m_loadport; 
        public void Init(Loadport loadport, Vega_Handler handler)
        {
            m_loadport = loadport;
            m_handler = handler;
            this.DataContext = loadport;
            textBoxPodID.DataContext = loadport.m_infoPod;
            textBoxLotID.DataContext = loadport.m_infoPod.m_aGemSlot[0];
            textBoxSlotID.DataContext = loadport.m_infoPod.m_aGemSlot[0];
            textBoxRecipe.DataContext = loadport.m_infoPod.m_aGemSlot[0];
            InitButtonLoad();
            InitTimer(); 

            m_manualjob = new ManualJobSchedule(m_loadport, m_handler);
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }


        private void M_timer_Tick(object sender, EventArgs e)
        {
			borderPlaced.Background = m_loadport.m_dioPlaced.p_bIn ? null : Brushes.LightGreen;
            if (m_loadport.m_dioPlaced.p_bIn){m_loadport.m_dioPlaced.Write(false);}
            else{m_loadport.m_dioPlaced.Write(true);}

            borderPresent.Background = m_loadport.m_dioPresent.p_bIn ? null : Brushes.LightGreen;
            if (m_loadport.m_dioPresent.p_bIn) { m_loadport.m_dioPresent.Write(false); }
            else { m_loadport.m_dioPresent.Write(true); }
            
            borderLoad.Background = m_loadport.m_bLoadCheck ? Brushes.LightGreen : null;
            if (m_loadport.m_bLoadCheck) { m_loadport.m_doLoad.Write(true);}
            else { m_loadport.m_doLoad.Write(false); }

            borderUnload.Background = m_loadport.m_bUnLoadCheck ? Brushes.LightGreen : null;
            if (m_loadport.m_bUnLoadCheck) { m_loadport.m_doUnload.Write(true); }
            else { m_loadport.m_doUnload.Write(false); }
            
            borderAlarm.Background = (m_loadport.p_eState == ModuleBase.eState.Error) ? Brushes.Red : null;
            if (m_loadport.p_eState == ModuleBase.eState.Error) { m_loadport.m_doAlarm.Write(true); }
            else { m_loadport.m_doAlarm.Write(false); }

            IonizerStatus.Background = m_loadport.m_diIonizer.p_bIn ? Brushes.Green : Brushes.DarkGray;

            bool bAuto = (m_loadport.m_infoPod.p_eReqAccessLP == GemCarrierBase.eAccessLP.Auto); 
            borderAccessAuto.Background = bAuto ? Brushes.LightGreen : null;
            if (m_loadport.m_infoPod.p_eReqAccessLP == GemCarrierBase.eAccessLP.Auto)
            { m_loadport.m_doAuto.Write(true); m_loadport.m_doManual.Write(false); }
            borderAccessManual.Background = bAuto ? null : Brushes.LightGreen;
            if (m_loadport.m_infoPod.p_eReqAccessLP == GemCarrierBase.eAccessLP.Manual)
            { m_loadport.m_doAuto.Write(false); m_loadport.m_doManual.Write(true); }

            buttonLoad.IsEnabled = IsEnableLoad();
            buttonUnload.IsEnabled = IsEnableUnload();
            //if (m_loadport.m_bIonizerDoorlockCheck == false)
            //{
            //    if (m_loadport.m_diIonizer.p_bIn)
            //    {
            //        m_handler.m_vega.m_doIonizerOnOff.Write(false);
            //        Thread.Sleep(20);
            //        if (m_loadport.m_diIonizer.p_bIn == true) EQ.p_bStop = true;
            //    }
            //}
        }
        #endregion

        #region Button Load
        BackgroundWorker m_bgwLoad = new BackgroundWorker();
        void InitButtonLoad()
        {
            m_bgwLoad.DoWork += M_bgwLoad_DoWork;
            m_bgwLoad.RunWorkerCompleted += M_bgwLoad_RunWorkerCompleted;
        }

        bool IsEnableLoad()
        {
            bool bReadyLoadport = (m_loadport.p_eState == ModuleBase.eState.Ready); 
            bool bReadyToLoad = (m_loadport.m_infoPod.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad);
            bReadyToLoad = true; 
            bool bReadyState =  (m_loadport.m_qModuleRun.Count == 0);
            bool bEQReadyState = (EQ.p_eState == EQ.eState.Ready);
            if (m_loadport.m_infoPod.p_eState != InfoPod.eState.Placed) return false;

            if (m_handler.IsEnableRecovery() == true) return false;
            return bReadyLoadport && bReadyToLoad && bReadyState && bEQReadyState && !m_loadport.m_dioPresent.p_bIn; //forget 조건
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableLoad() == false) return;
            if (m_manualjob.ShowPopup() == false) return;
            m_bgwLoad.RunWorkerAsync(); 
        }

        private void M_bgwLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            ModuleRunBase moduleRun = m_loadport.m_runReadPodID.Clone();
            m_loadport.StartRun(moduleRun);
            Thread.Sleep(100);
            while ((EQ.IsStop() != true) && m_loadport.m_qModuleRun.Count > 0) Thread.Sleep(10); 
            while ((EQ.IsStop() != true) && m_loadport.p_eState == ModuleBase.eState.Run) Thread.Sleep(10);
            moduleRun = m_loadport.m_runLoad.Clone();
            m_loadport.StartRun(moduleRun);
            Thread.Sleep(100);
            while ((EQ.IsStop() != true) && m_loadport.m_qModuleRun.Count > 0) Thread.Sleep(10);
            while ((EQ.IsStop() != true) && m_loadport.p_eState == ModuleBase.eState.Run) Thread.Sleep(10);
        }
        
        private void M_bgwLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            switch (m_loadport.p_eState)
            {
                case ModuleBase.eState.Ready:
                    m_loadport.m_infoPod.p_eState = InfoPod.eState.Load; 
                    if (m_manualjob.SetInfoPod() != "OK") return; 
                    m_loadport.m_infoPod.StartProcess();
                    Thread.Sleep(100); 
                    EQ.p_eState = EQ.eState.Run;
                    break; 
            }
        }
        #endregion

        #region Button Unload
        bool IsEnableUnload()
        {
            bool bReadyLoadport = m_loadport.p_eState == ModuleBase.eState.Ready;
            bool bPlace = m_loadport.CheckPlaced(); 
            bool bReadyToUnload = m_loadport.m_infoPod.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload;
            bool bAccess = m_loadport.m_OHT.p_eAccessLP == GemCarrierBase.eAccessLP.Auto; 
            return bReadyLoadport && bPlace && bReadyToUnload && bAccess; 
        }

        private void buttonUnload_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableUnload() == false) return;
            m_loadport.m_ceidUnload.Send(); 
        }
        #endregion
    }
}
