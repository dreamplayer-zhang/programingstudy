using Root_CAMELLIA.ManualJob;
using Root_EFEM.Module;
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

        private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            //if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            //{
            //    e.Handled = true;
            //}
        }



        //Loadport_RND m_loadport;
        //CAMELLIA_Handler m_handler;
        //BackgroundWorker m_bgwLoad = new BackgroundWorker();
        //RFID_Brooks m_rfid;
        //public void Init(ILoadport loadport, CAMELLIA_Handler handler, IRFID rfid)
        //{
        //    m_loadport = (Loadport_RND)loadport;
        //    m_handler = handler;
        //    m_rfid = (RFID_Brooks)rfid;
        //    this.DataContext = loadport;

        //    InitTimer();
        //    m_bgwLoad.DoWork += M_bgwLoad_DoWork;
        //    m_bgwLoad.RunWorkerCompleted += M_bgwLoad_RunWorkerCompleted;
        //}

        //private void M_bgwLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    switch (m_loadport.p_eState) 
        //    {
        //        case ModuleBase.eState.Ready:
        //            //m_loadport.p_infoCarrier.p_eState = InfoCarrier.eState.Dock;
        //            if (EQ.p_bRecovery == false)
        //            {
        //                InfoCarrier infoCarrier = m_loadport.p_infoCarrier;
        //                ManualJobSchedule manualJobSchedule = new ManualJobSchedule(infoCarrier);
        //                manualJobSchedule.ShowPopup();

        //                foreach(var val in infoCarrier.m_aInfoWafer)
        //                {
        //                    if(val != null)
        //                    {
        //                        object[] obj = { val.m_nSlot, val.p_sWaferID, val.p_sRecipe, val.p_eState };
        //                        datagridSlot.Items.Add(obj);
        //                    }
        //                }
        //            }
        //            //EQ.p_nRnR = 0;
        //            EQ.p_eState = EQ.eState.Run;
        //            break;
        //    }
        //}

        //private void M_bgwLoad_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    m_loadport.StartRun(m_loadport.GetModuleRunDocking().Clone());

        //    if (m_loadport.p_id == "LoadportA") EQ.p_nRunLP = 0;
        //    else if (m_loadport.p_id == "LoadportB") EQ.p_nRunLP = 1;

        //    while ((EQ.IsStop() != true) && m_loadport.IsBusy()) Thread.Sleep(10);
        //    //m_loadport.p_infoCarrier.p_eState = InfoCarrier.eState.Dock;
        //    Thread.Sleep(100);
        //}

        //bool IsEnableLoad()
        //{
        //    bool bReadyLoadport = (m_loadport.p_eState == ModuleBase.eState.Ready);
        //    bool bReadyToLoad = (m_loadport.p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad);
        //    bReadyToLoad = true;
        //    bool bReadyState = (m_loadport.m_qModuleRun.Count == 0);
        //    bool bEQReadyState = (EQ.p_eState == EQ.eState.Ready);
        //    //if (m_loadport.p_infoCarrier.p_eState != InfoCarrier.eState.Placed) return false;
        //    bool bPlaced = m_loadport.CheckPlaced();

        //    if (m_handler.IsEnableRecovery() == true) return false;
        //    return true;
        //    return bReadyLoadport && bReadyToLoad && bReadyState && bEQReadyState && bPlaced;
        //}

        //private void buttonLoad_Click(object sender, RoutedEventArgs e)
        //{
        //    m_bgwLoad.RunWorkerAsync();
        //}

        //bool IsEnableUnloadReq()
        //{
        //    bool bReadyLoadport = (m_loadport.p_eState == ModuleBase.eState.Ready);
        //    bool bReadyToUnload = (m_loadport.p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload);
        //    bool bAccess = (m_loadport.p_infoCarrier.p_eAccessLP == GemCarrierBase.eAccessLP.Auto);
        //    bool bPlaced = m_loadport.CheckPlaced();
        //    return bReadyLoadport && bReadyToUnload && bAccess && bPlaced;
        //}

        //private void buttonUnloadReq_Click(object sender, RoutedEventArgs e)
        //{
        //    if (IsEnableUnloadReq() == false) return;
        //    m_loadport.m_ceidUnloadReq.Send();
        //}


        //#region Timer
        //DispatcherTimer m_timer = new DispatcherTimer();
        //void InitTimer()
        //{
        //    m_timer.Interval = TimeSpan.FromMilliseconds(20);
        //    m_timer.Tick += M_timer_Tick;
        //    m_timer.Start();
        //}

        //private void M_timer_Tick(object sender, EventArgs e)
        //{
        //    //buttonLoad.IsEnabled = IsEnableLoad();
        //    //buttonUnloadReq.IsEnabled = IsEnableUnloadReq();
        //}

        //#endregion


    }
}
