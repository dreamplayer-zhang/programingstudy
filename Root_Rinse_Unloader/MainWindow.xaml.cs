using Root_Rinse_Unloader.Engineer;
using Root_Rinse_Unloader.Module;
using RootTools;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Rinse_Unloader
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitTimer();
        }

        #region TImer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.01);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            buttonHome.IsEnabled = EQ.p_eState != EQ.eState.Run;
            buttonStart.IsEnabled = EQ.p_eState == EQ.eState.Ready;
            buttonPause.IsEnabled = EQ.p_eState == EQ.eState.Run;
            buttonReset.IsEnabled = (EQ.p_eState == EQ.eState.Error) || (EQ.p_eState == EQ.eState.Ready);
            buttonPickerSet.IsEnabled = EQ.p_eState == EQ.eState.Ready;

            borderState.Background = (EQ.p_eState == EQ.eState.Ready || EQ.p_eState == EQ.eState.Run) ? Brushes.SeaGreen : Brushes.Gold;
            borderLoadState.Background = (m_handler.m_rinse.p_eStateLoader == EQ.eState.Ready || m_handler.m_rinse.p_eStateLoader == EQ.eState.Run) ? Brushes.SeaGreen : Brushes.Gold;

            bool bBlink = m_handler.m_rinse.m_bBlink; 
            gridRed.Background = (bBlink && (EQ.p_eState == EQ.eState.Error)) ? Brushes.Crimson : Brushes.DarkRed;
            gridYellow.Background = (bBlink && (EQ.p_eState == EQ.eState.Run)) ? Brushes.Gold : Brushes.YellowGreen;
            gridGreen.Background = (bBlink && (EQ.p_eState == EQ.eState.Ready)) ? Brushes.SeaGreen : Brushes.DarkGreen;
        }
        #endregion

        #region Loaded
        RinseU_Engineer m_engineer = new RinseU_Engineer();
        RinseU_Handler m_handler;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\Rinse_Unloader")) Directory.CreateDirectory(@"C:\Recipe\Rinse_Unloader");
            m_engineer.Init("Rinse_Unloader");
            engineerUI.Init(m_engineer);
            m_handler = (RinseU_Handler)m_engineer.ClassHandler();
            Init();
        }

        void Init()
        {
            textBlockState.DataContext = EQ.m_EQ;
            textBlockLoadState.DataContext = m_handler.m_rinse;
            TextBlockMode.DataContext = m_handler.m_rinse;
            textBoxWidth.DataContext = m_handler.m_rinse;
            textBlockStripState0.DataContext = m_handler.m_roller.m_aLine[0];
            textBlockStripState1.DataContext = m_handler.m_roller.m_aLine[1];
            textBlockStripState2.DataContext = m_handler.m_roller.m_aLine[2];
            textBlockStripState3.DataContext = m_handler.m_roller.m_aLine[3];
        }
        #endregion

        #region Closing
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }
        #endregion

        #region TitleBar
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
        #endregion

        #region Control Function
        private void buttonHome_Click(object sender, RoutedEventArgs e)
        {
            foreach (Roller.Line line in m_handler.m_roller.m_aLine) line.p_eSensor = Roller.Line.eSensor.Empty; 
            EQ.p_bStop = false;
            EQ.p_eState = EQ.eState.Home;
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_eState = EQ.eState.Run;
        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_eState = EQ.eState.Ready;
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            foreach (Roller.Line line in m_handler.m_roller.m_aLine) line.p_eSensor = Roller.Line.eSensor.Empty;
            m_handler.m_rinse.RunBuzzerOff();
            EQ.p_eState = EQ.eState.Ready;
        }

        private void buttonPickerSet_Click(object sender, RoutedEventArgs e)
        {
            m_handler.StartPickerSet();
        }
        #endregion

        #region PickerSet Control Function
        private void buttonPickerSetUp_Click(object sender, RoutedEventArgs e)
        {
            m_handler.m_loader.RunPickerDown(false);
        }

        private void buttonPickerSetDown_Click(object sender, RoutedEventArgs e)
        {
            m_handler.m_loader.RunPickerDown(true);
        }

        private void buttonPickerSetVacOn_Click(object sender, RoutedEventArgs e)
        {
            m_handler.m_loader.RunVacuum(true);
        }

        private void buttonPickerSetVacOff_Click(object sender, RoutedEventArgs e)
        {
            m_handler.m_loader.RunVacuum(false);
        }
        #endregion
    }
}
