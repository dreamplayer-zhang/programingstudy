using RootTools;
using RootTools.GAFs;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            if(this.WindowState == WindowState.Maximized)
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

        Vega_Engineer m_engineer = new Vega_Engineer();
        void Init()
        {
            IDialogService dialogService = new DialogService(this);
            DialogInit(dialogService);
            m_engineer.Init("Vega");
            _Maint.engineerUI.Init(m_engineer);
            ConnectViewModel(dialogService);
            //((GAF_Manager)m_engineer.ClassGAFManager()).UpdateTree();
            textState.DataContext = EQ.m_EQ;
            textGemState.DataContext = m_engineer.ClassGem();
            textGemControl.DataContext = m_engineer.ClassGem();
            textLastError.DataContext = m_engineer.m_gaf.m_listALID;

            _Main.Init(m_engineer); 
        }

        void ConnectViewModel(IDialogService dialogService)
        {
            mvm = new _1_Mainview_ViewModel(m_engineer, dialogService);
            //_Main.DataContext = mvm;

            _2_3_OriginViewModel ovm = new _2_3_OriginViewModel(m_engineer, dialogService);
            _Recipe._Origin.DataContext = ovm;
            //_recipe._recipeOrigin.DataContext = rovm;

            _2_5_SurfaceViewModel suvm = new _2_5_SurfaceViewModel(m_engineer, dialogService);
            _Recipe._Surface.DataContext = suvm;

            _2_6_SideViewModel sivm = new _2_6_SideViewModel(m_engineer, dialogService);
            _Recipe._Side.DataContext = sivm;

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

            _8_OHT_ViewModel opticvm = new _8_OHT_ViewModel(m_engineer, dialogService);
            _Optic.DataContext = opticvm;

            _10_SettingViewModel settingvm = new _10_SettingViewModel(m_engineer, dialogService);
            _Setting.DataContext = settingvm;
        }

        _1_Mainview_ViewModel mvm;
        _4_ViewerViweModel vvm;

        //DispatcherTimer m_timer = new DispatcherTimer();
        //Random m_rand = new Random();
        //int nDay = 1;

        //void m_timer_Tick(object sender, EventArgs e)
        //{
        //    if (nDay == 31)
        //        return;

        //    SYSTEMTIME Systime = new SYSTEMTIME();


        //    int nHour = m_rand.Next(24);
        //    int nMinute = m_rand.Next(60);
        //    int nSec = m_rand.Next(60);

        //    Systime.wYearvi = Convert.ToUInt16(2020);
        //    Systime.wDayOfWeek = 4;
        //    Systime.wMonth = Convert.ToUInt16(1);
        //    Systime.wDay = Convert.ToUInt16(nDay);
        //    Systime.wHour = Convert.ToUInt16(nHour);
        //    Systime.wMinute = Convert.ToUInt16(nMinute);
        //    Systime.wSecond = Convert.ToUInt16(nSec);


        //    int i = SetSystemTime(ref Systime);

        //    mvm.SetAlarm();
        //    vvm.SetAlarm();

        //    DateTime settime = new DateTime(Systime.wYearvi, Systime.wMonth, Systime.wDay, Systime.wHour, Systime.wMinute, Systime.wSecond);

        //    int nSecc = m_rand.Next(3600);

        //    DateTime endtime = settime + TimeSpan.FromSeconds(nSecc);

        //    Systime.wYearvi = Convert.ToUInt16(endtime.Year);
        //    Systime.wDayOfWeek = 4;
        //    Systime.wMonth = Convert.ToUInt16(endtime.Month);
        //    Systime.wDay = Convert.ToUInt16(endtime.Day);
        //    Systime.wHour = Convert.ToUInt16(endtime.Hour);
        //    Systime.wMinute = Convert.ToUInt16(endtime.Minute);
        //    Systime.wSecond = Convert.ToUInt16(endtime.Second);

        //    i = SetSystemTime(ref Systime);

        //    mvm.ClearAlarm();
        //    nDay++;

        //}


        public struct SYSTEMTIME
        {
            public ushort wYearvi;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }
        [DllImport("kernel32")]
        public static extern int SetSystemTime(ref SYSTEMTIME lpSystemTime);


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

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
