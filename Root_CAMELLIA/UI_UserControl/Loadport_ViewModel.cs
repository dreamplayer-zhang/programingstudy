using Root_CAMELLIA.ManualJob;
using Root_EFEM.Module;
using RootTools;
using RootTools.Gem;
using RootTools.Module;
using RootTools.OHTNew;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_CAMELLIA
{
    public class Loadport_ViewModel : ObservableObject
    {

        #region global
        Loadport_RND m_loadport;
        CAMELLIA_Handler m_handler;
        RFID_Brooks m_rfid;
        BackgroundWorker m_bgwLoad;
        DispatcherTimer m_timer;
        #endregion

        #region Property
        bool m_isEnableLoad = false;
        public bool p_isEnableLoad
        {
            get
            {
                return m_isEnableLoad;
            }
            set
            {
                SetProperty(ref m_isEnableLoad, value);
            }
        }

        bool m_isEnableUnload = false;
        public bool p_isEnableUnload
        {
            get
            {
                return m_isEnableUnload;
            }
            set
            {
                SetProperty(ref m_isEnableUnload, value);
            }
        }

        ObservableCollection<DataGridWaferInfo> m_waferList = new ObservableCollection<DataGridWaferInfo>();
        public ObservableCollection<DataGridWaferInfo> p_waferList
        {
            get
            {
                return m_waferList;
            }
            set
            {
                SetProperty(ref m_waferList, value);
            }
        }

        InfoCarrier m_infoCarrier;
        public InfoCarrier p_infoCarrier
        {
            get
            {
                return m_infoCarrier;
            }
            set
            {
                SetProperty(ref m_infoCarrier, value);
            }
        }
        #endregion

        public Loadport_ViewModel(int nIdx)
        {
            m_handler = App.m_engineer.m_handler;
            m_loadport = (Loadport_RND)m_handler.m_aLoadport[nIdx];
            m_rfid = (RFID_Brooks)m_handler.m_aRFID[nIdx];

            InitTimer();

            m_bgwLoad = new BackgroundWorker();
            m_bgwLoad.DoWork += M_bgwLoad_DoWork;
            m_bgwLoad.RunWorkerCompleted += M_bgwLoad_RunWorkerCompleted;
        }


        #region Command
        public ICommand CmdLoadClick
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Load();  
                });
            }
        }

        public ICommand CmdUnloadClick
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Unload();
                });
            }
        }

        #endregion

        #region Function

        void Load()
        {
            m_bgwLoad.RunWorkerAsync();
        }
        void Unload()
        {
            if (IsEnableUnloadReq() == false) return;
            m_loadport.m_ceidUnloadReq.Send();
        }

        private void M_bgwLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            switch (m_loadport.p_eState)
            {
                case ModuleBase.eState.Ready:
                    //m_loadport.p_infoCarrier.p_eState = InfoCarrier.eState.Dock;
                    if (EQ.p_bRecovery == false)
                    {
                        p_infoCarrier = m_loadport.p_infoCarrier;
                        ManualJobSchedule manualJobSchedule = new ManualJobSchedule(p_infoCarrier);
                        manualJobSchedule.ShowPopup();
                        //p_waferList = new ObservableCollection<InfoWafer>(infoCarrier.m_aInfoWafer.ToList());
                        StopWatch sw = new StopWatch();
                        sw.Start();
                        int idx = 1;
                        ObservableCollection<DataGridWaferInfo> temp = new ObservableCollection<DataGridWaferInfo>();
                        foreach (var val in p_infoCarrier.m_aInfoWafer)
                        {
                            if (val != null)
                            {
                                temp.Add(new DataGridWaferInfo(idx++, val.p_sWaferID, val.p_sRecipe, val.p_eState));
                                //object[] obj = { val.m_nSlot, val.p_sWaferID, val.p_sRecipe, val.p_eState };
                                //datagridSlot.Items.Add(obj);
                            }
                        }
                        p_waferList = temp;

                        sw.Stop();
                        System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
                    }
                    
                 
                    //EQ.p_nRnR = 0;
                    EQ.p_eState = EQ.eState.Run;
                    break;
            }
            if (m_loadport.p_eState != ModuleBase.eState.Ready)
            {
                CustomMessageBox.Show("Error", "Module is Not Ready!", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Error);
            }
        }

        private void M_bgwLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            m_loadport.StartRun(m_loadport.GetModuleRunDocking().Clone());

            if (m_loadport.p_id == "LoadportA") EQ.p_nRunLP = 0;
            else if (m_loadport.p_id == "LoadportB") EQ.p_nRunLP = 1;

            while ((EQ.IsStop() != true) && m_loadport.IsBusy()) Thread.Sleep(10);
            //m_loadport.p_infoCarrier.p_eState = InfoCarrier.eState.Dock;
            Thread.Sleep(100);
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
            return true;
            return bReadyLoadport && bReadyToLoad && bReadyState && bEQReadyState && bPlaced;
        }

        bool IsEnableUnloadReq()
        {
            bool bReadyLoadport = (m_loadport.p_eState == ModuleBase.eState.Ready);
            bool bReadyToUnload = (m_loadport.p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload);
            bool bAccess = (m_loadport.p_infoCarrier.p_eAccessLP == GemCarrierBase.eAccessLP.Auto);
            bool bPlaced = m_loadport.CheckPlaced();
            return bReadyLoadport && bReadyToUnload && bAccess && bPlaced;
        }



        void InitTimer()
        {
            m_timer = new DispatcherTimer();
            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }


        private void M_timer_Tick(object sender, EventArgs e)
        {
            p_isEnableLoad = IsEnableLoad();
            p_isEnableUnload = IsEnableUnloadReq();
        }

        #endregion

        public class DataGridWaferInfo : ObservableObject
        {

            public DataGridWaferInfo(int idx, string waferID, string recipeID, GemSlotBase.eState state)
            {
                p_Index = idx;
                p_waferId = waferID;
                p_recipeID = recipeID;
                p_state = state;
            }

            int m_Index = 0;
            public int p_Index
            {
                get
                {
                    return m_Index;
                }
                set
                {
                    SetProperty(ref m_Index, value);
                }
            }

            string m_waferId = "";
            public string p_waferId
            {
                get
                {
                    return m_waferId;
                }
                set
                {
                    SetProperty(ref m_waferId, value);
                }
            }

            string m_recipeID = "";
            public string p_recipeID
            {
                get
                {
                    return m_recipeID;
                }
                set
                {
                    SetProperty(ref m_recipeID, value);
                }
            }

            GemSlotBase.eState m_state = GemSlotBase.eState.Empty;
            public GemSlotBase.eState p_state
            {
                get
                {
                    return m_state;
                }
                set
                {
                    SetProperty(ref m_state, value);
                }
            }
        }
    }
}
