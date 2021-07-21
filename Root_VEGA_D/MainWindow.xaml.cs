using Root_EFEM.Module;
using Root_VEGA_D.Engineer;
using Root_VEGA_D.Module;
using RootTools;
using RootTools.Module;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Root_VEGA_D
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        #region TitleBar
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
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
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

        #endregion

        #region Window Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\VEGA_D")) Directory.CreateDirectory(@"C:\Recipe\VEGA_D");

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        VEGA_D_Handler m_handler;
        VEGA_D_Engineer m_engineer;
        Loadport_Cymechs[] m_loadport_Cymechs = new Loadport_Cymechs[2];
        Login_UI m_login;
        OHTs_UI m_ohts= new OHTs_UI();
        Login.eLevel m_level;

        MainWindow_ViewModel m_mainWindowViewModel; 

        public MainWindow()
        {
            InitializeComponent();
            
            Init();
        }

        void Init()
        {
            m_mainWindowViewModel = new MainWindow_ViewModel(this);
            this.DataContext = m_mainWindowViewModel;

            m_engineer = App.m_engineer;

            m_engineer.Init("VEGA_D");
            engineerUI.Init(m_engineer);
            m_handler = m_engineer.m_handler;
            loadportA.Init(m_handler.m_aLoadport[0], m_handler, m_handler.m_aRFID[0]);
            loadportB.Init(m_handler.m_aLoadport[1], m_handler, m_handler.m_aRFID[1]);
            //RecipeWizard_UI.init(m_engineer);
            InitTimer();
            InitFFU();
            m_loadport_Cymechs[0] = (Loadport_Cymechs)m_handler.m_aLoadport[0];
            m_loadport_Cymechs[1] = (Loadport_Cymechs)m_handler.m_aLoadport[1];
            VersionInfo.Text = "Ver " + assemblyVersion.ToString();
            btnLogin.Content = "User";
            LoadportAState.DataContext = m_handler.m_loadport[0];
            LoadportBState.DataContext = m_handler.m_loadport[1];
            RobotState.DataContext = m_handler.m_wtr;
            VisionState.DataContext = m_handler.m_vision;
            btnLogin.DataContext = m_engineer.m_login;
            engineerTab.Visibility = (m_engineer.m_login.p_eLevel >= Login.eLevel.Admin) ? Visibility.Visible : Visibility.Collapsed;
            //ReviewTab.Visibility = (m_engineer.m_login.p_eLevel >= Login.eLevel.Operator) ? Visibility.Visible : Visibility.Collapsed;
            RunTab.Visibility = (m_engineer.m_login.p_eLevel >= Login.eLevel.Operator) ? Visibility.Visible : Visibility.Collapsed;
            RecipeManagerTab.Visibility = (m_engineer.m_login.p_eLevel >= Login.eLevel.Operator) ? Visibility.Visible : Visibility.Collapsed; ;
            //RecipeWizardTab.Visibility = (m_engineer.m_login.p_eLevel >= Login.eLevel.Operator) ? Visibility.Visible : Visibility.Collapsed;
            //TextBlockRetID.DataContext = m_handler.m_aLoadport[0].p_infoCarrier.m_aGemSlot[0];

            m_handler.m_vision.LineScanStatusChanged += m_mainWindowViewModel.M_vision_LineScanStatusChanged;
        }

        //bool m_blogin = false;

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }

        bool IsEnable_Resume()
        {
            if (EQ.p_eState != EQ.eState.Ready) return false;
            if (m_handler.m_process.m_qSequence.Count <= 0) return false;
            return true;
        }
        private void buttonResume_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnable_Resume() == false) return;
            EQ.p_bStop = false;
            EQ.p_eState = EQ.eState.Run;
        }

        bool IsEnable_Pause()
        {
            if (EQ.p_eState != EQ.eState.Run) return false;
            return true;
        }
        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnable_Pause() == false) return;
            EQ.p_eState = EQ.eState.Ready;
        }
        void InitFFU()
        {
            FanUI0.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[0];
            FanUI1.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[1];
            FanUI2.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[2];
            FanUI3.DataContext = m_handler.m_FFU.p_aUnit[0].p_aFan[3];
            FDC_CDA1.DataContext = m_handler.m_interlock;
            FDC_CDA2.DataContext = m_handler.m_interlock;
        }

        bool IsRunModule(ModuleBase module)
        {
            if (module.p_eState == ModuleBase.eState.Run) return true;
            if (module.p_eState == ModuleBase.eState.Home) return true;
            return (module.m_qModuleRun.Count > 0);
        }
        bool IsRunModule()
        {
            if (IsRunModule((Loadport_Cymechs)m_handler.m_aLoadport[0])) return true;
            if (IsRunModule((Loadport_Cymechs)m_handler.m_aLoadport[1])) return true;
            if (IsRunModule(m_handler.m_wtr)) return true;
            if (IsRunModule(m_handler.m_vision)) return true;
            return false;
        }
        bool IsEnable_Initial()
        {
            if (IsRunModule()) return false;
            switch (EQ.p_eState)
            {
                case EQ.eState.Run: return false;
                case EQ.eState.Home: return false;
            }
            return true;
        }
        private void buttonInitialize_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnable_Initial() == false) return;
            EQ.p_bStop = false;
            m_handler.m_process.ClearInfoWafer();
            EQ.p_eState = EQ.eState.Home;
            //Camellia Camera Connect
        }

        bool IsEnable_Recovery()
        {
            if (IsRunModule()) return false;
            if (IsErrorModule()) return false;
            if (EQ.p_eState != EQ.eState.Ready) return false;
            if (m_handler.m_bIsPossible_Recovery == false) return false;

            if (EQ.p_bStop == true) return false;
            return m_handler.IsEnableRecovery();
        }
        private void buttonRecovery_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnable_Recovery() == false) return;
            m_handler.m_bIsPossible_Recovery = false;
            m_handler.CalcRecover();
            EQ.p_eState = EQ.eState.Run;
            EQ.p_bRecovery = true;
        }
        private void buttonBuzzOff_Click(object sender, RoutedEventArgs e)
        {
            m_engineer.BuzzerOff();
        }
        bool IsErrorModule()
        {
            if (IsErrorModule(m_handler.m_loadport[0]) || IsErrorModule(m_handler.m_loadport[1]) || 
                IsErrorModule(m_handler.m_wtr) || IsErrorModule(m_handler.m_vision))
                return true;
            else
                return false;
        }
        bool IsErrorModule(ModuleBase module)
        {
            if (module.p_eState == ModuleBase.eState.Error)
                return true;
            else
                return false;
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
            TimerUI();
            TimerLamp();
            NowTime.Text = "Date : " + DateTime.Now.ToString("yyyy.MM.dd tt hh:mm:ss", CultureInfo.InvariantCulture);
            LoadportABack.Background = m_handler.m_aLoadport[0].p_infoCarrier.m_aInfoWafer[0] != null && m_handler.m_aLoadport[0].p_bPlaced && m_handler.m_aLoadport[0].p_bPresent ? Brushes.MediumSlateBlue : Brushes.AliceBlue;
            LoadportBBack.Background = m_handler.m_aLoadport[1].p_infoCarrier.m_aInfoWafer[0] != null && m_handler.m_aLoadport[1].p_bPlaced && m_handler.m_aLoadport[1].p_bPresent ? Brushes.MediumSlateBlue : Brushes.AliceBlue;
            RobotBack.Background = m_handler.m_wtr.m_dicArm[0].p_infoWafer != null ? Brushes.MediumSlateBlue : Brushes.AliceBlue;
            VisionBack.Background = m_handler.m_vision.p_infoWafer != null ? Brushes.MediumSlateBlue : Brushes.AliceBlue;
            buttonResume.IsEnabled = IsEnable_Resume();
            buttonPause.IsEnabled = IsEnable_Pause();
            buttonInitialize.IsEnabled = IsEnable_Initial();
            buttonRecovery.IsEnabled = IsEnable_Recovery();
            if (m_loadport_Cymechs[0].m_swLotTime.IsRunning)
                InspectTime.Text = String.Format("{0:00}:{1:00}:{2:00}", m_loadport_Cymechs[0].m_swLotTime.Elapsed.Hours, m_loadport_Cymechs[0].m_swLotTime.Elapsed.Minutes, m_loadport_Cymechs[0].m_swLotTime.Elapsed.Seconds);
            else
                InspectTime.Text = String.Format("{0:00}:{1:00}:{2:00}", m_loadport_Cymechs[1].m_swLotTime.Elapsed.Hours, m_loadport_Cymechs[1].m_swLotTime.Elapsed.Minutes, m_loadport_Cymechs[1].m_swLotTime.Elapsed.Seconds);
            RNRCount.Text = EQ.p_nRnR < 1 ? "0" : EQ.p_nRnR.ToString();
            btnFP_Isolator.IsChecked = m_handler.m_interlock.m_diFP_Isolator.p_bIn;
            btnIsolator_V.IsChecked = m_handler.m_interlock.m_diIsolator_VPre.p_bIn;
            btn_Factory_Air_Pad.IsChecked = m_handler.m_interlock.m_diFactory_Air_PadPre.p_bIn;
            btn_Air_Tank.IsChecked = m_handler.m_interlock.m_diAir_TankPre.p_bIn;
            btn_X_Bottom.IsChecked = m_handler.m_interlock.m_diX_BottomPre.p_bIn;
            btn_X_Side_Master.IsChecked = m_handler.m_interlock.m_diX_SideMasterPre.p_bIn;
            btn_X_Side_Slave.IsChecked = m_handler.m_interlock.m_diX_SideSlavePre.p_bIn;
            btn_Y_Bottom.IsChecked = m_handler.m_interlock.m_diY_BottomPre.p_bIn;
            btn_Y_Side_Master.IsChecked = m_handler.m_interlock.m_diY_SideMasterPre.p_bIn;
            btn_Y_Side_Slave.IsChecked = m_handler.m_interlock.m_diY_SideSlavePre.p_bIn;
        }
        void TimerUI()
        {
            if (EQ.p_eState != EQ.eState.Run) EQ.p_bRecovery = false;
            //textState.Text = m_bRecovery ? "Recovery" : EQ.p_eState.ToString();
            EQState.Foreground = EQ.p_eState.ToString() == "Error" ? Brushes.Red : Brushes.Green;
            EQState.Text = EQ.p_bRecovery ? "Recovery" : EQ.p_eState.ToString();
        }

        void TimerLamp()
        {
            //working
            lampRed.Background = EQ.p_eState == EQ.eState.Error ? Brushes.Crimson : Brushes.LavenderBlush;
            lampYellow.Background = EQ.p_eState == EQ.eState.Ready ? Brushes.Gold : Brushes.Ivory;
            lampGreen.Background = EQ.p_eState == EQ.eState.Run ? Brushes.SeaGreen : Brushes.Honeydew;
        }
		#endregion

		private void btnLogin_Click(object sender, RoutedEventArgs e)
		{
            m_login = new Login_UI(m_engineer);
            m_login.ShowDialog();
            engineerTab.Visibility = (m_engineer.m_login.p_eLevel >= Login.eLevel.Admin) ? Visibility.Visible : Visibility.Collapsed;
            //ReviewTab.Visibility = (m_engineer.m_login.p_eLevel >= Login.eLevel.Operator) ? Visibility.Visible : Visibility.Collapsed;
            RunTab.Visibility = (m_engineer.m_login.p_eLevel >= Login.eLevel.Operator) ? Visibility.Visible : Visibility.Collapsed;
            RecipeManagerTab.Visibility = (m_engineer.m_login.p_eLevel >= Login.eLevel.Operator) ? Visibility.Visible : Visibility.Collapsed;
            //RecipeWizardTab.Visibility = (m_engineer.m_login.p_eLevel >= Login.eLevel.Operator) ? Visibility.Visible : Visibility.Collapsed;
        }

		private void btnOHT_Click(object sender, RoutedEventArgs e)
		{
            if (m_ohts.IsLoaded == false)
            {
                m_ohts.Init((VEGA_D_Handler)m_engineer.ClassHandler());
                m_ohts.Show();
            }
            else m_ohts.Show();

        }
    }
	public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value.ToString() != "Error")
            {
                return Brushes.Black;
            }
            else
            {
                return Brushes.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

