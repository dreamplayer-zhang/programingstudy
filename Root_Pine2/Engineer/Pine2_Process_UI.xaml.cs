using Root_Pine2.Module;
using Root_Pine2_Vision.Module;
using RootTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Pine2.Engineer
{
    /// <summary>
    /// Pine2_Process_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Pine2_Process_UI : UserControl
    {
        public Pine2_Process_UI()
        {
            InitializeComponent();
        }

        Pine2 m_pine2; 
        Pine2_Handler m_handler; 
        public void Init(Pine2_Handler handler)
        {
            m_handler = handler;
            m_pine2 = handler.m_pine2; 

            labelEQState.DataContext = EQ.m_EQ;
            checkBoxStop.DataContext = EQ.m_EQ;
            checkBoxPause.DataContext = EQ.m_EQ;
            checkBoxSimulate.DataContext = EQ.m_EQ;

            comboRecipe.DataContext = handler; 
            textBlockMode.DataContext = m_pine2;
            textBoxWidth.DataContext = m_pine2;
            textBoxThickness.DataContext = m_pine2;
            checkBox3D.DataContext = m_pine2; 

            textBoxWorker.DataContext = m_pine2; 
            textBoxLotID.DataContext = m_pine2;
            textBoxBundle.DataContext = m_pine2;

            checkBoxPaper.DataContext = m_pine2;
            textBoxlStack.DataContext = m_pine2;
            textBoxlStackPaper.DataContext = m_pine2;
            checkBoxBlow.DataContext = m_pine2;
            checkBoxIonBlow.DataContext = m_pine2;
            checkBoxAlignBlow.DataContext = m_pine2;

            checkBoxLotMix2D.DataContext = m_pine2.m_aVisionOption[eVision.Top2D];
            checkBoxBarcode2D.DataContext = m_pine2.m_aVisionOption[eVision.Top2D];
            textBoxBarcode2D.DataContext = m_pine2.m_aVisionOption[eVision.Top2D];
            textBoxBarcodeLength2D.DataContext = m_pine2.m_aVisionOption[eVision.Top2D];
            checkBoxLotMixBottom.DataContext = m_pine2.m_aVisionOption[eVision.Bottom];
            checkBoxBarcodeBottom.DataContext = m_pine2.m_aVisionOption[eVision.Bottom];
            textBoxBarcodeBottom.DataContext = m_pine2.m_aVisionOption[eVision.Bottom];
            textBoxBarcodeLengthBottom.DataContext = m_pine2.m_aVisionOption[eVision.Bottom];

            InitMagazineEV_UI();
            InitLoaderUI(handler.m_loader0, gridLoader, 6);
            InitLoaderUI(handler.m_loader1, gridLoader1);
            InitLoaderUI(handler.m_loader2, gridLoader2);
            InitLoaderUI(handler.m_loader3, gridLoader, 0);
            InitBoatsUI();
            InitTransferUI();
            InitLoadEVUI();

            m_bgwNewLot.DoWork += M_bgwNewLot_DoWork;
        }

        List<MagazineEV_UI> m_aMagazineUI = new List<MagazineEV_UI>(); 
        void InitMagazineEV_UI()
        {
            MagazineEVSet set = m_handler.m_magazineEVSet;
            foreach (InfoStrip.eMagazine eMagazine in Enum.GetValues(typeof(InfoStrip.eMagazine)))
            {
                MagazineEV_UI ui = new MagazineEV_UI();
                ui.Init(set.m_aEV[eMagazine]);
                Grid.SetColumn(ui, (int)eMagazine); 
                gridMagazineEV.Children.Add(ui);
                m_aMagazineUI.Add(ui);

                m_timer.Tick += M_timer_Tick;
                m_timer.Interval = TimeSpan.FromSeconds(0.2);
                m_timer.Start(); 
            }
        }

        List<Loader_UI> m_aLoaderUI = new List<Loader_UI>();
        void InitLoaderUI(dynamic loader, Grid grid, int nColumn)
        {
            Loader_UI ui = new Loader_UI();
            ui.Init(loader);
            Grid.SetColumn(ui, nColumn);
            grid.Children.Add(ui);
            m_aLoaderUI.Add(ui); 
        }

        void InitLoaderUI(dynamic loader, Grid grid)
        {
            Loader_UI ui = new Loader_UI();
            ui.Init(loader);
            Grid.SetRow(ui, 1);
            grid.Children.Add(ui);
            m_aLoaderUI.Add(ui);
        }

        List<Boats_UI> m_aBoatsUI = new List<Boats_UI>(); 
        void InitBoatsUI()
        {
            foreach (eVision eVision in Enum.GetValues(typeof(eVision)))
            {
                Boats_UI ui = new Boats_UI();
                ui.Init(m_handler.m_aBoats[eVision], m_handler.m_pine2);
                Grid.SetColumn(ui, 4 - (int)eVision);
                gridBoat.Children.Add(ui);
                m_aBoatsUI.Add(ui); 
            }
        }

        Transfer_UI m_transferUI; 
        void InitTransferUI()
        {
            Transfer_UI ui = new Transfer_UI();
            ui.Init(m_handler.m_transfer);
            Grid.SetColumn(ui, 3);
            gridLoader.Children.Add(ui);
            m_transferUI = ui; 
        }

        LoadEV_UI m_loadEVUI;
        void InitLoadEVUI()
        {
            LoadEV_UI ui = new LoadEV_UI();
            ui.Init(m_handler.m_loadEV);
            Grid.SetColumn(ui, 7);
            gridLoader.Children.Add(ui);
            m_loadEVUI = ui;
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            grid.Background = (m_pine2.p_eMode == Pine2.eRunMode.Magazine) ? Brushes.Moccasin : Brushes.Silver; 
            switch (EQ.p_eState)
            {
                case EQ.eState.Init: gridEQ.Background = Brushes.White; break;
                case EQ.eState.Home: gridEQ.Background = Brushes.MediumPurple; break;
                case EQ.eState.Ready: gridEQ.Background = Brushes.LightGreen; break;
                case EQ.eState.Run: gridEQ.Background = Brushes.Yellow; break;
                case EQ.eState.ModuleRunList: gridEQ.Background = Brushes.Orange; break;
                case EQ.eState.Error: gridEQ.Background = Brushes.OrangeRed; break;
            }
            foreach (MagazineEV_UI ui in m_aMagazineUI) ui.OnTimer(); 
            foreach (Loader_UI ui in m_aLoaderUI) ui.OnTimer();
            foreach (Boats_UI ui in m_aBoatsUI) ui.OnTimer();
            m_transferUI.OnTimer();
            m_loadEVUI.OnTimer();
            OnTimerRun();

            textBoxBarcode2D.IsEnabled = m_pine2.m_aVisionOption[eVision.Top2D].p_bBarcode;
            textBoxBarcodeLength2D.IsEnabled = m_pine2.m_aVisionOption[eVision.Top2D].p_bBarcode;
            textBoxBarcodeBottom.IsEnabled = m_pine2.m_aVisionOption[eVision.Bottom].p_bBarcode;
            textBoxBarcodeLengthBottom.IsEnabled = m_pine2.m_aVisionOption[eVision.Bottom].p_bBarcode;
        }

        private void textBlockMode_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            m_pine2.p_eMode = 1 - m_pine2.p_eMode;
        }

        #region Run Button
        void OnTimerRun()
        {
            buttonStart.IsEnabled = (EQ.p_eState == EQ.eState.Ready);
            buttonStop.IsEnabled = (EQ.p_eState == EQ.eState.Run); 
            buttonReset.IsEnabled = (EQ.p_eState == EQ.eState.Ready) || (EQ.p_eState == EQ.eState.Error);
            buttonHome.IsEnabled = (EQ.p_eState == EQ.eState.Ready) || (EQ.p_eState == EQ.eState.Init) || (EQ.p_eState == EQ.eState.Error);
            buttonPickerSet.IsEnabled = (EQ.p_eState == EQ.eState.Ready);
            groupBoxRecipe.IsEnabled = (EQ.p_eState == EQ.eState.Ready);
            groupBoxLot.IsEnabled = (EQ.p_eState == EQ.eState.Ready);
            groupBoxStack.IsEnabled = m_pine2.p_eMode == Pine2.eRunMode.Stack && (EQ.p_eState == EQ.eState.Ready);
            groupBoxLoadEV.IsEnabled = m_pine2.p_eMode == Pine2.eRunMode.Stack && (EQ.p_eState == EQ.eState.Ready);
            groupBoxVisionTop.IsEnabled = (EQ.p_eState == EQ.eState.Ready);
            groupBoxVisionBottom.IsEnabled = (EQ.p_eState == EQ.eState.Ready);
            groupBoxMode.IsEnabled = (EQ.p_eState == EQ.eState.Ready) || (EQ.p_eState == EQ.eState.Init);
        }

        private void buttonRecipeSave_Click(object sender, RoutedEventArgs e)
        {
            m_pine2.RecipeSave();
        }

        private void buttonHome_Click(object sender, RoutedEventArgs e)
        {
            switch (EQ.p_eState)
            {
                case EQ.eState.Init:
                case EQ.eState.Error:
                case EQ.eState.Ready:
                    EQ.p_eState = EQ.eState.Home;
                    break; 
            }
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (EQ.p_eState == EQ.eState.Ready) EQ.p_eState = EQ.eState.Run;
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            if (EQ.p_eState == EQ.eState.Run) EQ.p_eState = EQ.eState.Ready; 
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            if (EQ.p_bStop == false)
            {
                EQ.p_bStop = true;
                Thread.Sleep(10);
                EQ.p_bStop = false;
            }
            switch (EQ.p_eState)
            {
                case EQ.eState.Ready:
                case EQ.eState.Error:
                    m_handler.Reset();
                    break; 
            }
        }

        private void buttonPickerSet_Click(object sender, RoutedEventArgs e)
        {
            m_handler.StartPickerSet(); 
        }

        BackgroundWorker m_bgwNewLot = new BackgroundWorker();
        private void buttonNewLot_Click(object sender, RoutedEventArgs e)
        {
            if (m_bgwNewLot.IsBusy)
            {
                m_handler.m_pine2.m_alidNewLot.p_bSet = true; 
                return; 
            }
            m_bgwNewLot.RunWorkerAsync();
        }

        private void M_bgwNewLot_DoWork(object sender, DoWorkEventArgs e)
        {
            m_handler.NewLot();
        }
        #endregion

        private void labelEQState_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (EQ.p_eState == EQ.eState.Error) EQ.p_eState = EQ.eState.Ready; 
        }
    }
}
