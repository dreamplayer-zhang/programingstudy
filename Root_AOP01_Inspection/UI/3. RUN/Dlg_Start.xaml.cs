using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using RootTools;
using RootTools.Module;
using Root_AOP01_Inspection.Module;
using Root_EFEM.Module;
using static Root_EFEM.Module.WTR_RND;
using System.Windows.Input;
using System.ComponentModel;
using RootTools.Gem;
using System.Threading;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using RootTools.Trees;

namespace Root_AOP01_Inspection
{
    /// <summary>
    /// Dlg_Start.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Dlg_Start : Window
    {
        AOP01_Engineer m_engineer;
        AOP01_Handler m_handler;
        AOP01_Recipe m_recipe;
        MainVision m_mainvision;
        WTRCleanUnit m_wtrcleanunit;
        WTRArm m_wtr;
        Arm m_arm;
        RNR m_aRnR;
        Loadport_Cymechs[] m_loadport = new Loadport_Cymechs[2];
        Loadport_RND[] m_rndloadport = new Loadport_RND[2];
        AOP01_Handler.eLoadport LoadportType;
        public Dlg_Start()
        {
            InitializeComponent();
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public void Init(MainVision mainvision, WTRCleanUnit wtrcleanunit, Loadport_Cymechs loadport1,
            Loadport_Cymechs loadport2, AOP01_Engineer engineer)
        {
            m_aRnR = new RNR();
            m_aRecipe = new ObservableCollection<Recipe>();
            m_engineer = engineer;
            m_handler = engineer.m_handler;
            m_recipe = engineer.m_handler.m_recipe;
            m_arm = wtrcleanunit.m_dicArm[0];
            m_wtrcleanunit = wtrcleanunit;
            m_wtr = m_wtrcleanunit.p_aArm[0];
            m_mainvision = mainvision;
            m_loadport[0] = loadport1;
            m_loadport[1] = loadport2;
            listviewRCP.ItemsSource = m_aRecipe;
            LoadportType = AOP01_Handler.eLoadport.Cymechs;
            //RNRCount.DataContext = m_aRnR;
            //RNRMode.DataContext = m_aRnR;
            Test.DataContext = m_aRnR;
            InitButtonLoad();
            InitTimer();
        }
        public void Init(MainVision mainvision, WTRCleanUnit wtrcleanunit, Loadport_RND loadport1,
            Loadport_RND loadport2, AOP01_Engineer engineer)
        {
            RNR m_aRnR = new RNR();
            m_aRecipe = new ObservableCollection<Recipe>();
            m_engineer = engineer;
            m_handler = engineer.m_handler;
            m_recipe = engineer.m_handler.m_recipe;
            m_arm = wtrcleanunit.m_dicArm[0];
            m_wtrcleanunit = wtrcleanunit;
            m_wtr = m_wtrcleanunit.p_aArm[0];
            m_mainvision = mainvision;
            m_rndloadport[0] = loadport1;
            m_rndloadport[1] = loadport2;
            listviewRCP.ItemsSource = m_aRecipe;
            LoadportType = AOP01_Handler.eLoadport.RND;
            //RNRCount.DataContext = m_aRnR;
            //RNRMode.DataContext = m_aRnR;
            InitButtonLoad();
            InitTimer();
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
            switch(LoadportType)
            {
                case AOP01_Handler.eLoadport.Cymechs:
                    Placed1.Background = m_loadport[0].m_diPlaced.p_bIn == true ? Brushes.SteelBlue : Brushes.LightGray;
                    Present1.Background = m_loadport[0].m_diPresent.p_bIn == true ? Brushes.SteelBlue : Brushes.LightGray;
                    //Load1.Background = m_loadport[0].m_bLoadCheck == true ? Brushes.SteelBlue : Brushes.LightGray;
                    //UnLoad1.Background = m_loadport[0].m_bUnLoadCheck == true ? Brushes.SteelBlue : Brushes.LightGray;
                    Alarm1.Background = m_loadport[0].p_eState == ModuleBase.eState.Error ? Brushes.Red : Brushes.LightGray;
                    Placed2.Background = m_loadport[1].m_diPlaced.p_bIn ? Brushes.SteelBlue : Brushes.LightGray;
                    Present2.Background = m_loadport[1].m_diPresent.p_bIn ? Brushes.SteelBlue : Brushes.LightGray;
                    //Load2.Background = m_loadport[1].m_bLoadCheck ? Brushes.SteelBlue : Brushes.LightGray;
                    //UnLoad2.Background = m_loadport[1].m_bUnLoadCheck ? Brushes.SteelBlue : Brushes.LightGray;
                    Alarm2.Background = m_loadport[1].p_eState == ModuleBase.eState.Error ? Brushes.Red : Brushes.LightGray;
                    break;
                case AOP01_Handler.eLoadport.RND:
                    Placed1.Background = m_rndloadport[0].m_diPlaced.p_bIn == true ? Brushes.SteelBlue : Brushes.LightGray;
                    Present1.Background = m_rndloadport[0].m_diPresent.p_bIn == true ? Brushes.SteelBlue : Brushes.LightGray;
                    //Load1.Background = m_rndloadport[0].m_bLoadCheck == true ? Brushes.SteelBlue : Brushes.LightGray;
                    //UnLoad1.Background = m_rndloadport[0].m_bUnLoadCheck == true ? Brushes.SteelBlue : Brushes.LightGray;
                    Alarm1.Background = m_rndloadport[0].p_eState == ModuleBase.eState.Error ? Brushes.Red : Brushes.LightGray;
                    Placed2.Background = m_rndloadport[1].m_diPlaced.p_bIn ? Brushes.SteelBlue : Brushes.LightGray;
                    Present2.Background = m_rndloadport[1].m_diPresent.p_bIn ? Brushes.SteelBlue : Brushes.LightGray;
                    //Load2.Background = m_rndloadport[1].m_bLoadCheck ? Brushes.SteelBlue : Brushes.LightGray;
                    //UnLoad2.Background = m_rndloadport[1].m_bUnLoadCheck ? Brushes.SteelBlue : Brushes.LightGray;
                    Alarm2.Background = m_rndloadport[1].p_eState == ModuleBase.eState.Error ? Brushes.Red : Brushes.LightGray;
                    break;
            }


            Loadport1.Background = Loadport1_Check.IsChecked==true ? Brushes.AliceBlue : null;
            Loadport2.Background = Loadport2_Check.IsChecked == true ? Brushes.AliceBlue : null;
            //ButtonLoad1.IsEnabled = IsEnableLoad(0);
            ButtonUnLoadReq1.IsEnabled = IsEnableUnload(0);
            ButtonLoad2.IsEnabled = IsEnableLoad(1);
            ButtonUnLoadReq2.IsEnabled = IsEnableUnload(1);
        }
        #endregion
        #region Button Load UnLoad
        BackgroundWorker m_bgwLoad = new BackgroundWorker();
        void InitButtonLoad()
        {
            m_bgwLoad.DoWork += M_bgwLoad_DoWork;
            m_bgwLoad.RunWorkerCompleted += M_bgwLoad_RunWorkerCompleted;
        }
        bool IsEnableLoad(int LPNum)
        {
            bool bReadyLoadport = (m_loadport[LPNum].p_eState == ModuleBase.eState.Ready);
            bool bReadyToLoad = (m_loadport[LPNum].p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad);
            bReadyToLoad = true;
            bool bReadyState = (m_loadport[LPNum].m_qModuleRun.Count == 0);
            bool bEQReadyState = (EQ.p_eState == EQ.eState.Ready);
            if (m_loadport[LPNum].p_infoCarrier.p_eState != InfoCarrier.eState.Placed) return false;

            if (m_handler.IsEnableRecovery() == true) return false;
            return bReadyLoadport && bReadyToLoad && bReadyState && bEQReadyState && !m_loadport[LPNum].m_diPresent.p_bIn; //forget 조건
        }
        bool IsEnableUnload(int LPNum)
        {
            bool bReadyLoadport = m_loadport[LPNum].p_eState == ModuleBase.eState.Ready;
            bool bPlace = m_loadport[LPNum].CheckPlaced();
            bool bReadyToUnload = m_loadport[LPNum].p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload;
            bool bAccess = m_loadport[LPNum].m_OHT.p_eAccessLP == GemCarrierBase.eAccessLP.Auto;
            return bReadyLoadport && bPlace && bReadyToUnload && bAccess;
        }

        private void ButtonLoad1_Click(object sender, RoutedEventArgs e)
        {
            m_handler.m_nRnR = m_aRnR.p_bRnR ? m_aRnR.p_nRnR : 1;
            if (IsEnableLoad(0) == false) return;
            //if (m_manualjob.ShowPopup() == false) return;
            m_bgwLoad.RunWorkerAsync();
        }

        private void ButtonUnLoadReq1_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableUnload(0) == false) return;
            //m_loadport[0].m_ceidUnload.Send();
        }

        private void ButtonLoad2_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableLoad(1) == false) return;
            //if (m_manualjob.ShowPopup() == false) return;
            m_bgwLoad.RunWorkerAsync();
        }

        private void ButtonUnLoadReq2_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableUnload(1) == false) return;
            //m_loadport.m_ceidUnload.Send();
        }

       
        private void M_bgwLoad_DoWork(object sender, DoWorkEventArgs e)
        {
        //    ModuleRunBase moduleRun = m_loadport[0].m_runReadPodID.Clone();
        //    m_loadport[0].StartRun(moduleRun);
        //    Thread.Sleep(100);
        //    while ((EQ.IsStop() != true) && m_loadport[0].m_qModuleRun.Count > 0) Thread.Sleep(10);
        //    while ((EQ.IsStop() != true) && m_loadport[0].p_eState == ModuleBase.eState.Run) Thread.Sleep(10);
        //    moduleRun = m_loadport[0].m_runLoad.Clone();
        //    m_loadport[0].StartRun(moduleRun);
        //    Thread.Sleep(100);
        //    while ((EQ.IsStop() != true) && m_loadport.m_qModuleRun.Count > 0) Thread.Sleep(10);
        //    while ((EQ.IsStop() != true) && m_loadport.p_eState == ModuleBase.eState.Run) Thread.Sleep(10);
        }

        private void M_bgwLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //switch (m_loadport.p_eState)
            //{
            //    case ModuleBase.eState.Ready:
            //        m_loadport.m_infoPod.p_eState = InfoPod.eState.Load;
            //        if (m_manualjob.SetInfoPod() != "OK") return;
            //        m_loadport.m_infoPod.StartProcess();
            //        Thread.Sleep(100);
            //        EQ.p_eState = EQ.eState.Run;
            //        break;
            //}
        }
        #endregion
        #region Loadport Check Box
        private void LP1CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(Loadport2_Check.IsChecked==true)
                Loadport2_Check.IsChecked = false;
        }

        private void LP2CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(Loadport1_Check.IsChecked==true)
                Loadport1_Check.IsChecked = false;
        }
        #endregion
        #region Recipe List
        public class Recipe : NotifyProperty
        {
            int _nNumber = 1;
            public int p_nNumber
            {
                get { return _nNumber; }
                set
                {
                    _nNumber = value;
                    OnPropertyChanged();
                }
            }
            public string p_sRecipeName { get; set; }
            public string p_sDate { get; set; }

            public Recipe(int nNumber, string sRecipe, string sDate)
            {
                p_nNumber = nNumber + 1;
                p_sRecipeName = sRecipe;
                p_sDate = sDate;
            }
        }
        public ObservableCollection<Recipe> m_aRecipe { get; set; }

        public void ClearRecipe()
        {
            m_aRecipe.Clear();
        }

        public void AddRecipe(string sRecipe, string sDate)
        {
            Recipe recipe = new Recipe(m_aRecipe.Count, sRecipe, sDate);
            m_aRecipe.Add(recipe);
        }
        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearRecipe();
            string[] Getfiles = Directory.GetFiles(m_recipe.m_sPath, "*.AOP01");
            foreach (string files in Getfiles)
            {
                FileInfo file = new FileInfo(files);
                string rcpname = file.Name;
                AddRecipe(rcpname, file.LastWriteTime.ToString());
            }
        }
        void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {  
            Recipe typeItem = (Recipe)listviewRCP.SelectedItem;
            string sRecipeName = typeItem.p_sRecipeName.ToString();
            string sRecipe = m_recipe.m_sPath + sRecipeName;
            m_recipe.m_moduleRunList.OpenJob(sRecipe);
            m_recipe.m_moduleRunList.RunTree(Tree.eMode.Init);
        }
        #endregion

        #region RnR Property
        public class RNR : NotifyProperty
        {

            bool _bRnR = false;
            public bool p_bRnR
            {
                get { return _bRnR; }
                set
                {
                    _bRnR = value;
                    OnPropertyChanged();
                }
            }

            int _nRnR = 1;
            public int p_nRnR
            {
                get { return _nRnR; }
                set
                {
                    _nRnR = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion
    }

}
