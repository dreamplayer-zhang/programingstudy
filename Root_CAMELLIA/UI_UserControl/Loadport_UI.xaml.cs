﻿using Root_EFEM.Module;
using RootTools;
using RootTools.Gem;
using RootTools.Module;
using RootTools.OHTNew;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

namespace Root_CAMELLIA.UI_UserControl
{
    /// <summary>
    /// Loadport_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Loadport_UI : UserControl
    {
        public Loadport_UI()
        {
            InitializeComponent();
        }

        Loadport_RND m_loadport;
        CAMELLIA_Handler m_handler;
        BackgroundWorker m_bgwLoad = new BackgroundWorker();
        public void Init(ILoadport loadport, CAMELLIA_Handler handler)
        {
            m_loadport = (Loadport_RND)loadport;
            m_handler = handler;
            this.DataContext = loadport;

            InitTimer();
            m_bgwLoad.DoWork += M_bgwLoad_DoWork;
            m_bgwLoad.RunWorkerCompleted += M_bgwLoad_RunWorkerCompleted;
        }

        private void M_bgwLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            switch (m_loadport.p_eState) 
            {
                case ModuleBase.eState.Ready:
                    //m_loadport.p_infoCarrier.p_eState = InfoCarrier.eState.Dock;
                    EQ.p_eState = EQ.eState.Run;
                    break;
            }
        }

        private void M_bgwLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            //RFID Reading //working
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

        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
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

        private void buttonUnloadReq_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableUnloadReq() == false) return;
            m_loadport.m_ceidUnloadReq.Send();
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
            buttonLoad.IsEnabled = IsEnableLoad();
            buttonUnloadReq.IsEnabled = IsEnableUnloadReq();
        }

        #endregion

        
    }
}