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
        }
        #endregion

        #region Lamp
        void TimerLamp()
        {
            Vega vega = m_engineer.m_handler.m_vega;
            gridLampR.Background = vega.m_doLamp.ReadDO(Vega.eLamp.Red) ? Brushes.Red : Brushes.DarkRed;
            gridLampY.Background = vega.m_doLamp.ReadDO(Vega.eLamp.Yellow) ? Brushes.LightYellow : Brushes.YellowGreen;
            gridLampG.Background = vega.m_doLamp.ReadDO(Vega.eLamp.Green) ? Brushes.LightGreen : Brushes.DarkGreen;
        }
        #endregion

        Vega_Engineer m_engineer = new Vega_Engineer();
        void Init()
        {
            IDialogService dialogService = new DialogService(this);
            DialogInit(dialogService);
            m_engineer.Init("Vega");
            _Maint.engineerUI.Init(m_engineer);
            ConnectViewModel(dialogService);
            textState.DataContext = EQ.m_EQ;
            textGemState.DataContext = m_engineer.ClassGem();
            textGemControl.DataContext = m_engineer.ClassGem();
            textLastError.DataContext = m_engineer.m_gaf.m_listALID;

            _Main.Init(m_engineer);

            InitTimer(); 
        }

        void ConnectViewModel(IDialogService dialogService)
        {
            mvm = new _1_Mainview_ViewModel(m_engineer, dialogService);
            //_Main.DataContext = mvm;

            _2_3_OriginViewModel ovm = new _2_3_OriginViewModel(m_engineer, dialogService);
            _Recipe._Origin.DataContext = ovm;
            //_recipe._recipeOrigin.DataContext = rovm;

            _2_5_MainVisionViewModel suvm = new _2_5_MainVisionViewModel(m_engineer, dialogService);
            _Recipe._Strip.DataContext = suvm;

            _2_6_SideViewModel sivm = new _2_6_SideViewModel(m_engineer, dialogService);
            _Recipe._Side.DataContext = sivm;

            _2_7_EdgeBoxViewModel evm = new _2_7_EdgeBoxViewModel(m_engineer, dialogService);
            _Recipe._EdgeBox.DataContext = evm;

            _2_4_PositionViewModel pvm = new _2_4_PositionViewModel(m_engineer, dialogService);
            _Recipe._Position.DataContext = pvm;

            vvm = new _4_ViewerViweModel(m_engineer, dialogService);
            _Viewer.DataContext = vvm;

            _6_LogViewModel lvm = new _6_LogViewModel(m_engineer);
            _Log.DataContext = lvm;

            _7_AlarmViewModel avm = new _7_AlarmViewModel();
            _Alarm.DataContext = avm;

            _5_ResultViewModel rvm = new _5_ResultViewModel(m_engineer, dialogService);
            _Result.DataContext = rvm;

            _10_SettingViewModel settingvm = new _10_SettingViewModel(m_engineer, dialogService);
            _Setting.DataContext = settingvm;
        }

        _1_Mainview_ViewModel mvm;
        _4_ViewerViweModel vvm;

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }

        void DialogInit(IDialogService dialogService)
        {
            dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();
            dialogService.Register<Dialog_Scan_ViewModel, Dialog_Scan>();
            dialogService.Register<Dialog_ManualAlignViewModel, Dialog_ManualAlign>();
            dialogService.Register<Dialog_SideScan_ViewModel, Dialog_SideScan>();
            dialogService.Register<Dialog_AutoFocus_ViewModel, Dialog_AutoFocus>();
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
            m_engineer.m_gaf.m_listALID.ShowPopup(); 
        }

    }
}
