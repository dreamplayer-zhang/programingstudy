using Root_EFEM.Module;
using Root_VEGA_D.Engineer;
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
using System.Windows.Threading;

namespace Root_VEGA_D
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        VEGA_D_Handler m_handler;
        public MainWindow()
        {
            InitializeComponent();
        }
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

        //bool m_blogin = false;



        #region Window Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\VEGA_D")) Directory.CreateDirectory(@"C:\Recipe\VEGA_D");
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        VEGA_D_Engineer m_engineer = new VEGA_D_Engineer();
        void Init()
        {
            m_engineer.Init("VEGA_D");
            engineerUI.Init(m_engineer);
            m_handler = m_engineer.m_handler;
            loadportA.Init(m_handler.m_aLoadport[0], m_handler, m_handler.m_aRFID[0]);
            loadportB.Init(m_handler.m_aLoadport[1], m_handler, m_handler.m_aRFID[1]);
            InitTimer();
            TextBlockRetID.DataContext = m_handler.m_aLoadport[0].p_infoCarrier.m_aGemSlot[0];
            Is_InLP.Background = (m_handler.m_aLoadport[0].p_infoCarrier.m_aInfoWafer[0] != null)||(m_handler.m_aLoadport[0].p_infoCarrier.m_aInfoWafer[1] != null) ? Brushes.MediumBlue: Brushes.Gray;
            Is_InRTR.Background = m_handler.m_wtr.m_dicArm[0].p_infoWafer== null ? Brushes.MediumBlue : Brushes.Gray;
            Is_InVS.Background = m_handler.m_vision.p_infoWafer == null ? Brushes.MediumBlue : Brushes.Gray;
        }

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
            if (IsRunModule(m_handler.m_visionIPU)) return true;
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
            if (EQ.p_eState != EQ.eState.Ready) return false;
            if (EQ.p_bStop == true) return false;
            return m_handler.IsEnableRecovery();
        }
        private void buttonRecovery_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnable_Recovery() == false) return;
            m_handler.CalcRecover();
            EQ.p_bStop = false;
            EQ.p_eState = EQ.eState.Run;
            EQ.p_bRecovery = true;
        }

        private void buttonBuzzOff_Click(object sender, RoutedEventArgs e)
        {
            m_engineer.BuzzerOff();
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

            buttonResume.IsEnabled = IsEnable_Resume();
            buttonPause.IsEnabled = IsEnable_Pause();
            buttonInitialize.IsEnabled = IsEnable_Initial();
            buttonRecovery.IsEnabled = IsEnable_Recovery();
        }
        void TimerUI()
        {
            if (EQ.p_eState != EQ.eState.Run) EQ.p_bRecovery = false;
            //textState.Text = m_bRecovery ? "Recovery" : EQ.p_eState.ToString();
            textState.Text = EQ.p_bRecovery ? "Recovery" : EQ.p_eState.ToString();
            textState.Text = string.Format("State : {0}", textState.Text);
        }

        void TimerLamp()
        {
            //working
            lampRed.Background = EQ.p_eState == EQ.eState.Error ? Brushes.Crimson : Brushes.LavenderBlush;
            lampYellow.Background = EQ.p_eState == EQ.eState.Ready ? Brushes.Gold : Brushes.Ivory;
            lampGreen.Background = EQ.p_eState == EQ.eState.Run ? Brushes.SeaGreen : Brushes.Honeydew;
        }
        #endregion
    }
}
