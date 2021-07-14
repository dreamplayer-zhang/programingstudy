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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_CAMELLIA
{
    public class Loadport_ViewModel : ObservableObject
    {

        #region global
     
        CAMELLIA_Handler m_handler;
        RFID_Brooks m_rfid;
        BackgroundWorker m_bgwLoad;
        DispatcherTimer m_timer;
        #endregion

        #region Property

        Loadport_RND m_loadport;
        public Loadport_RND p_loadport
        {
            get
            {
                return m_loadport;
            }
            set
            {
                SetProperty(ref m_loadport, value);
            }
        }

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

        bool m_isPlaced = false;
        public bool p_isPlaced
        {
            get
            {
                return m_isPlaced;
            }
            set
            {
                SetProperty(ref m_isPlaced, value);
            }
        }

        bool m_isPresent = false;
        public bool p_isPresent
        {
            get
            {
                return m_isPresent;
            }
            set
            {
                SetProperty(ref m_isPresent, value);
            }
        }

        bool m_isLoad = false;
        public bool p_isLoad
        {
            get
            {
                return m_isLoad;
            }
            set
            {
                SetProperty(ref m_isLoad, value);
            }
        }

        private bool m_isUnload = false;

        public bool p_isUnload
        {
            get 
            { 
                return m_isUnload;
            }
            set 
            {
                SetProperty(ref m_isUnload, value);
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

        int m_dataSelectIndex = 0;
        public int p_dataSelectIndex
        {
            get
            {
                return m_dataSelectIndex;
            }
            set
            {
                SetProperty(ref m_dataSelectIndex, value);
            }
        }

        string m_CurrentRecipeID = "";
        public string p_CurrentRecipeID
        {
            get
            {
                return m_CurrentRecipeID;
            }
            set
            {
                SetProperty(ref m_CurrentRecipeID, value);
            }
        }

        int m_totalSelect = 0;
        public int p_totalSelect
        {
            get
            {
                return m_totalSelect;
            }
            set
            {
                SetProperty(ref m_totalSelect, value);
            }
        }

        int m_totalDone = 0;
        public int p_totalDone
        {
            get
            {
                return m_totalDone;
            }
            set
            {
                SetProperty(ref m_totalDone, value);
            }
        }

        bool m_isRNR = false;
        public bool p_isRNR
        {
            get
            {
                return m_isRNR;
            }
            set
            {
                SetProperty(ref m_isRNR, value);
            }
        }

        int m_totalRNR = 0;
        public int p_totalRNR
        {
            get
            {
                return m_totalRNR;
            }
            set
            {
                SetProperty(ref m_totalRNR, value);
            }
        }

        int m_currentRNR = 0;
        public int p_currentRNR
        {
            get
            {
                return m_currentRNR;
            }
            set
            {
                SetProperty(ref m_currentRNR, value);
            }
        }

        double m_progressValue = 0;
        public double p_progressValue
        {
            get
            {
                return m_progressValue;
            }
            set
            {
                SetProperty(ref m_progressValue, value);
            }
        }

        Dlg_ManualJob_ViewModel manualJob_ViewModel;
        DialogService dialogService;
        #endregion

        public Loadport_ViewModel(int nIdx, MainWindow_ViewModel main)
        {
            m_handler = App.m_engineer.m_handler;
            p_loadport = (Loadport_RND)m_handler.m_aLoadport[nIdx];
            m_rfid = (RFID_Brooks)m_handler.m_aRFID[nIdx];
            
            //p_loadport.m_OHTNew.p_eAccessLP
            InitTimer();

            m_bgwLoad = new BackgroundWorker();
            m_bgwLoad.DoWork += M_bgwLoad_DoWork;
            m_bgwLoad.RunWorkerCompleted += M_bgwLoad_RunWorkerCompleted;

            for(int i = 0; i < p_loadport.p_infoCarrier.m_aGemSlot.Count; i++)
            {
                p_loadport.p_infoCarrier.m_aGemSlot[i].StateChanged += StateChange;
            }

            manualJob_ViewModel = main.ManualJobViewModel;
            dialogService = main.dialogService;

            m_handler.OnRnRDone += M_handler_OnRnRDone;
            m_handler.OnListUpdate += M_handler_OnListUpdate;
        }

        #region Event

        private void M_handler_OnListUpdate()
        {
            if (this.p_loadport.p_id == App.m_engineer.m_handler.m_aLoadport[EQ.p_nRunLP].p_id)
                UpdateList();
        }

        private void M_handler_OnRnRDone()
        {
            if(this.p_loadport.p_id == App.m_engineer.m_handler.m_aLoadport[EQ.p_nRunLP].p_id)
                p_currentRNR++;
        }

        private void StateChange(object sender, EventArgs e)
        {
            if (EQ.p_eState != EQ.eState.ModuleRunList)
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (p_waferList.Count == 0)
                    {
                        return;
                    }

                    //p_currentRNR = m_handler.p_currentRNR;

                    if (p_totalDone >= p_totalSelect)
                    {
                        p_totalDone = 0;
                        
                    }

                    p_waferList[24 - ((InfoWafer)sender).m_nSlot].p_state = ((InfoWafer)sender).p_eState;
                    if (((InfoWafer)sender).p_eState == GemSlotBase.eState.Done)
                    {
                        p_totalDone++;

                        p_progressValue = (double)p_totalDone / p_totalSelect * 100;
                    }
                    p_dataSelectIndex = 24 - ((InfoWafer)sender).m_nSlot;
                }));
        }

        #endregion


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

        public ICommand CmdOHTManual
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (EQ.p_bSimulate)
                        p_loadport.p_infoCarrier.p_eAccessLP = GemCarrierBase.eAccessLP.Manual;
                    else
                    {
                        p_loadport.p_infoCarrier.p_eReqAccessLP = GemCarrierBase.eAccessLP.Manual;
                        p_loadport.m_OHTsemi.m_bAuto = false;
                    }
                });
            }
        }

        public ICommand CmdOHTAuto
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (EQ.p_bSimulate)
                        p_loadport.p_infoCarrier.p_eAccessLP = GemCarrierBase.eAccessLP.Auto;
                    else
                    {
                        p_loadport.p_infoCarrier.p_eReqAccessLP = GemCarrierBase.eAccessLP.Auto;
                        p_loadport.m_OHTsemi.m_bAuto = true;
                    }
                });
            }
        }

        public ICommand CmdInService
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!App.m_engineer.p_bUseXGem)
                    {
                        return;
                    }
                    if (EQ.p_eState != EQ.eState.Ready || p_loadport.p_diPlaced.p_bIn)
                    {
                        p_loadport.p_infoCarrier.p_eReqTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                    }
                    else if(EQ.p_eState == EQ.eState.Ready && !p_loadport.p_diPlaced.p_bIn)
                    {
                        p_loadport.p_infoCarrier.p_eReqTransfer = GemCarrierBase.eTransfer.ReadyToLoad;
                    }
                    //p_infoCarrier.p_eReqTransfer = GemCarrierBase.eTransfer.OutOfService;
                });
            }
        }

        public ICommand CmdOutOfService
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(App.m_engineer.p_bUseXGem)
                        p_loadport.p_infoCarrier.p_eReqTransfer = GemCarrierBase.eTransfer.OutOfService;
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
            p_loadport.m_ceidUnloadReq.Send();
        }

        private void M_bgwLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            switch (p_loadport.p_eState)
            {
                case ModuleBase.eState.Ready:
                    if (!EQ.p_bRecovery)
                    {
                        p_infoCarrier = p_loadport.p_infoCarrier;
                        p_waferList.Clear();
                        p_isRNR = false;
                        p_CurrentRecipeID = "";
                        p_dataSelectIndex = 0;
                        p_totalSelect = 0;
                        p_totalDone = 0;
                        p_progressValue = 0;
                        var viewModel = manualJob_ViewModel;
                        viewModel.InitData(p_infoCarrier);
                        var dialog = dialogService.GetDialog(viewModel) as Dlg_ManualJob;
                        Nullable<bool> result = dialog.ShowDialog();

                        if (result.HasValue)
                        {
                            if (!result.Value)
                            {
                                p_loadport.StartRun(p_loadport.GetModuleRunUndocking().Clone());
                                return;
                            }
                            else
                            {
                                p_currentRNR = 0;
                                p_isRNR = viewModel.p_checkRnR;
                                if (viewModel.p_checkRnR)
                                {
                                    p_isRNR = true;
                                    p_totalRNR = EQ.p_nRnR;
                                }
                                else
                                {
                                    p_isRNR = false;
                                    p_totalRNR = 0;
                                }
                                m_handler.p_process.MakeRnRSeq();
                                UpdateList();
                            }
                        }
                    }
                   

                    //if (EQ.p_bRecovery == false)
                    //{
                    //    p_infoCarrier = p_loadport.p_infoCarrier;
                    //    ManualJobSchedule manualJobSchedule = new ManualJobSchedule(p_infoCarrier);
                    //    p_waferList.Clear();
                    //    p_dataSelectIndex = 0;
                    //    if (!manualJobSchedule.ShowPopup())
                    //    {
                    //        p_loadport.StartRun(p_loadport.GetModuleRunUndocking().Clone());
                    //        return;
                    //    }
                    //    //p_waferList = new ObservableCollection<InfoWafer>(infoCarrier.m_aInfoWafer.ToList());
                    //    StopWatch sw = new StopWatch();
                    //    sw.Start();
                       
                    //    sw.Stop();
                    //    System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
                    //}


                    //EQ.p_nRnR = 0;
                    EQ.p_eState = EQ.eState.Run;
                    break;
            }
            if (p_loadport.p_eState != ModuleBase.eState.Ready)
            {
                CustomMessageBox.Show("Error", "Module is Not Ready!", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Error);
            }
        }

        void UpdateList()
        {
            int firstIdx = 0;
            int idx = 1;
            ObservableCollection<DataGridWaferInfo> temp = new ObservableCollection<DataGridWaferInfo>();

            foreach (InfoWafer val in p_infoCarrier.m_aGemSlot)
            {
                if (val != null)
                {
                    temp.Insert(0, new DataGridWaferInfo(idx++, val.p_sWaferID, val.p_sRecipe, val.p_eState));
                    if (val.p_eState == GemSlotBase.eState.Select)
                    {
                        p_totalSelect++;
                    }
                    if (p_CurrentRecipeID == "" && val.p_sRecipe != "")
                    {
                        p_CurrentRecipeID = val.p_sRecipe.Replace(Path.GetExtension(val.p_sRecipe), "");
                        firstIdx = idx - 1;
                    }
                    //object[] obj = { val.m_nSlot, val.p_sWaferID, val.p_sRecipe, val.p_eState };
                    //datagridSlot.Items.Add(obj);
                }
            }
            p_waferList = temp;

            p_dataSelectIndex = 24 - firstIdx + 1;
        }

        private void M_bgwLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            p_loadport.StartRun(p_loadport.GetModuleRunDocking().Clone());

            if (p_loadport.p_id == "LoadportA") EQ.p_nRunLP = 0;
            else if (p_loadport.p_id == "LoadportB") EQ.p_nRunLP = 1;

            while ((EQ.IsStop() != true) && p_loadport.IsBusy()) Thread.Sleep(10);
            //m_loadport.p_infoCarrier.p_eState = InfoCarrier.eState.Dock;
            Thread.Sleep(100);
        }


        bool IsEnableLoad()
        {
            if (p_loadport.p_diDocked.p_bIn)
            {
                return false;
            }
            bool bReadyLoadport = p_loadport.p_eState == ModuleBase.eState.Ready;
            bool bReadyToLoad = true;
            if (App.m_engineer.p_bUseXGem && !p_loadport.p_infoCarrier.m_gem.p_bOffline)
                bReadyToLoad = p_loadport.p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad;
            
            bool bReadyState = p_loadport.m_qModuleRun.Count == 0;
            bool bEQReadyState = EQ.p_eState == EQ.eState.Ready;
            //if (m_loadport.p_infoCarrier.p_eState != InfoCarrier.eState.Placed) return false;
            bool bPlaced = p_loadport.CheckPlaced();
            if (m_handler.IsEnableRecovery() == true) return false;
            return bReadyLoadport && bReadyToLoad && bReadyState && bEQReadyState && bPlaced;
        }

        bool IsEnableUnloadReq()
        {
            bool bReadyLoadport = p_loadport.p_eState == ModuleBase.eState.Ready;
            bool bReadyToUnload = true;
            if (App.m_engineer.p_bUseXGem && !p_loadport.p_infoCarrier.m_gem.p_bOffline)
                bReadyToUnload = p_loadport.p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload;
            bool bAccess = p_loadport.p_infoCarrier.p_eAccessLP == GemCarrierBase.eAccessLP.Auto;
            bool bPlaced = p_loadport.CheckPlaced();
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
                if (recipeID != "")
                    p_recipeID = recipeID.Replace(Path.GetExtension(recipeID), "");
                else
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
