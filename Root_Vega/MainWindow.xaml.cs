using Root_Vega.Module;
using RootTools;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Vega
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Window Event
        public MainWindow()
        {
            InitializeComponent();
            if (this.WindowState == WindowState.Maximized)
            {
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                NormalizeButton.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick; 
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            textBoxDateTime.Text = DateTime.Now.ToString();
            TimerLamp();
            //TimerUI(App.m_engineer.m_login.p_eLevel); 
            TimerUI(Login.eLevel.Admin);
        }

        void TimerUI(Login.eLevel level)
        {
            if (EQ.p_eState != EQ.eState.Run) _Main.m_bRecovery = false; 
            textState.Text = _Main.m_bRecovery ? "Recovery" : EQ.p_eState.ToString(); 
            _Recipe.Visibility = (level >= Login.eLevel.Operator) ? Visibility.Visible : Visibility.Hidden;
            _Maint.Visibility = (level >= Login.eLevel.Admin) ? Visibility.Visible : Visibility.Hidden; 
            _Viewer.Visibility = (level >= Login.eLevel.Worker) ? Visibility.Visible : Visibility.Hidden;
            _Result.Visibility = (level >= Login.eLevel.Worker) ? Visibility.Visible : Visibility.Hidden;
            _Setting.Visibility = (level >= Login.eLevel.Admin) ? Visibility.Visible : Visibility.Hidden;
        }
        #endregion

        #region Lamp
        void TimerLamp()
        {
            Vega vega = App.m_engineer.m_handler.m_vega;
            gridLampR.Background = vega.m_doLamp.ReadDO(Vega.eLamp.Red) ? Brushes.Red : Brushes.Black;
            gridLampY.Background = vega.m_doLamp.ReadDO(Vega.eLamp.Yellow) ? Brushes.LightYellow : Brushes.Black;
            gridLampG.Background = vega.m_doLamp.ReadDO(Vega.eLamp.Green) ? Brushes.LightGreen : Brushes.Black;
        }
        #endregion

        //Vega_Engineer App.m_engineer = new Vega_Engineer();
        void Init()
        {
            IDialogService dialogService = new DialogService(this);
            DialogInit(dialogService);
            App.m_engineer.Init("Vega");
            _Maint.engineerUI.Init(App.m_engineer);
            ConnectViewModel(dialogService);
            textState.DataContext = EQ.m_EQ;
            textGemState.DataContext = App.m_engineer.ClassGem();
            textGemControl.DataContext = App.m_engineer.ClassGem();
            textLastError.DataContext = App.m_engineer.m_gaf.m_listALID;

            _Main.Init(App.m_engineer);
            loginMaunUI.Init(App.m_engineer.m_login); 

            InitTimer(); 

            if(!System.IO.Directory.Exists(@"C:\Recipe\VEGA"))
			{
                System.IO.Directory.CreateDirectory(@"C:\Recipe\VEGA");
			}
        }

        void ConnectViewModel(IDialogService dialogService)
        {
            mvm = new _1_Mainview_ViewModel(App.m_engineer, dialogService);
            _Main.DataContext = mvm;
            ((Vega_Handler)App.m_engineer.ClassHandler()).m_patternVision.m_mvm = mvm;
            ((Vega_Handler)App.m_engineer.ClassHandler()).m_sideVision.m_mvm = mvm;

            _Recipe.DataContext = new _2_RecipeViewModel(App.m_engineer);

            //_2_3_OriginViewModel ovm = new _2_3_OriginViewModel(App.m_engineer, dialogService);
            //_Recipe._Origin.DataContext = ovm;
            //_recipe._recipeOrigin.DataContext = rovm;

            //_2_4_PositionViewModel pvm = new _2_4_PositionViewModel(App.m_engineer, dialogService);
            //_Recipe._Position.DataContext = pvm;

            _2_5_MainVisionViewModel suvm = new _2_5_MainVisionViewModel(App.m_engineer, dialogService);
            _Recipe._Strip.DataContext = suvm;
            ((Vega_Handler)App.m_engineer.ClassHandler()).m_patternVision.m_mvvm= suvm;

            _2_6_SideViewModel sivm = new _2_6_SideViewModel(App.m_engineer, dialogService);
            _Recipe._Side.DataContext = sivm;
            ((Vega_Handler)App.m_engineer.ClassHandler()).m_sideVision.m_sivm= sivm;

            _2_7_EdgeBoxViewModel evm = new _2_7_EdgeBoxViewModel(App.m_engineer, dialogService);
            _Recipe._SideEdgeBox.DataContext = evm;

            _2_11_EBRViewModel ebvm = new _2_11_EBRViewModel(App.m_engineer, dialogService);
            _Recipe._EBR.DataContext = ebvm;
            ((Vega_Handler)App.m_engineer.ClassHandler()).m_patternVision.m_ebrvm = ebvm;

            _2_12_EBREdgeBoxViewModel ebedvm = new _2_12_EBREdgeBoxViewModel(App.m_engineer, dialogService);
            _Recipe._EBREdge.DataContext = ebedvm;

            _2_10_BevelEdgeBoxViewModel bebvm = new _2_10_BevelEdgeBoxViewModel(App.m_engineer, dialogService);
            _Recipe._BevelEdgeBox.DataContext = bebvm;

            _2_8_D2D_InspViewModel divm = new _2_8_D2D_InspViewModel(App.m_engineer, dialogService);
            _Recipe._D2D_Insp.DataContext = divm;

            _2_9_BevelViewModel bevm = new _2_9_BevelViewModel(App.m_engineer, dialogService);
            _Recipe._Bevel.DataContext = bevm;
            ((Vega_Handler)App.m_engineer.ClassHandler()).m_sideVision.m_bevm = bevm;

            vvm = new _4_ViewerViweModel(App.m_engineer, dialogService);
            _Viewer.DataContext = vvm;

            _6_LogViewModel lvm = new _6_LogViewModel(App.m_engineer);
            _Log.DataContext = lvm;

            _7_AlarmViewModel avm = new _7_AlarmViewModel();
            _Alarm.DataContext = avm;

            _5_ResultViewModel rvm = new _5_ResultViewModel(App.m_engineer, dialogService);
            _Result.DataContext = rvm;

            _10_SettingViewModel settingvm = new _10_SettingViewModel(App.m_engineer, dialogService);
            _Setting.DataContext = settingvm;
        }

        _1_Mainview_ViewModel mvm;
        _4_ViewerViweModel vvm;

        void ThreadStop()
        {
            App.m_engineer.ThreadStop();
        }

        void DialogInit(IDialogService dialogService)
        {
            dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();
            dialogService.Register<Dialog_Scan_ViewModel, Dialog_Scan>();
            dialogService.Register<Dialog_ManualAlignViewModel, Dialog_ManualAlign>();
			dialogService.Register<Dialog_SideScan_ViewModel, Dialog_SideScan>();
			dialogService.Register<Dialog_SettingFDC_ViewModel, Dialog_SettingFDC>();
			dialogService.Register<Dialog_AutoFocus_ViewModel, Dialog_AutoFocus>();
            dialogService.Register<Dialog_LADS_ViewModel, Dialog_LADS>();
        }


        private void MainTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem ti = (TabItem) MainTab.SelectedItem;
            tb_CurrenView.Text = ti.Header.ToString();

        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            NormalizeButton.Visibility = Visibility.Visible;
            MaximizeButton.Visibility = Visibility.Collapsed;
        }

        private void NormalizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            MaximizeButton.Visibility = Visibility.Visible;
            NormalizeButton.Visibility = Visibility.Collapsed;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2)
            {
                if(this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    MaximizeButton.Visibility = Visibility.Visible;
                    NormalizeButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    NormalizeButton.Visibility = Visibility.Visible;
                    MaximizeButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                this.DragMove();
            }
        }

        private void textLastError_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            App.m_engineer.m_gaf.m_listALID.ShowPopup(); 
        }

    }
}
