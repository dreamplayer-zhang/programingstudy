using Root_Rinse_Unloader.Engineer;
using Root_Rinse_Unloader.Module;
using RootTools;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
            RinseU rinse = m_handler.m_rinse;
            bool bBlink = rinse.m_bBlink;

            buttonMode.IsEnabled = EQ.p_eState == EQ.eState.Ready;
            buttonHome.IsEnabled = EQ.p_eState != EQ.eState.Run;
            buttonStart.IsEnabled = (EQ.p_eState == EQ.eState.Ready) && (m_handler.m_storage.IsProtrusion() == false); 
            buttonPause.IsEnabled = EQ.p_eState == EQ.eState.Run;
            buttonReset.IsEnabled = (EQ.p_eState == EQ.eState.Error) || (EQ.p_eState == EQ.eState.Ready);

            bool bRun = bBlink && (EQ.p_eState == EQ.eState.Run);
            buttonStart.Foreground = (bRun && EQ.p_bPickerSet == false) ? Brushes.Red : Brushes.Black;
            borderState.Background = (EQ.p_eState == EQ.eState.Ready || EQ.p_eState == EQ.eState.Run) ? Brushes.SeaGreen : Brushes.Gold;


            if (rinse.m_tcpip.m_tcpSocket == null)
            {
                borderLoadState.Background = Brushes.Crimson;
                m_handler.m_rinse.p_eStateLoader = EQ.eState.Null; 
            }
            else if(rinse.m_tcpip.m_tcpSocket.m_socket.Connected)
            {
                borderLoadState.Background = (rinse.p_eStateLoader == EQ.eState.Ready || rinse.p_eStateLoader == EQ.eState.Run) ? Brushes.SeaGreen : Brushes.Gold;
            }
            else
            {
                borderLoadState.Background = Brushes.Crimson;
                m_handler.m_rinse.p_eStateLoader = EQ.eState.Null;
            }

            gridRed.Background = (bBlink && (EQ.p_eState == EQ.eState.Error)) ? Brushes.Crimson : Brushes.DarkRed;
            gridYellow.Background = (bBlink && (EQ.p_eState == EQ.eState.Ready)) ? Brushes.Gold : Brushes.YellowGreen;
            gridGreen.Background = (bBlink && (EQ.p_eState == EQ.eState.Run)) ? Brushes.SeaGreen : Brushes.DarkGreen;

            tabControlStorage.SelectedIndex = (int)m_handler.m_rinse.p_eMode;
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
            textBoxRotateSpeed.DataContext = m_handler.m_rinse; 
            buttonMode.DataContext = m_handler.m_rinse;
            textBoxWidth.DataContext = m_handler.m_rinse;
            magazineUI.Init(m_handler.m_rinse, m_handler.m_storage);
            stackUI.Init(m_handler.m_storage, m_handler.m_loader);
            tabControlStorage.SelectedIndex = (int)m_handler.m_rinse.p_eMode;
            textBlockStripState0.DataContext = m_handler.m_roller.m_aLine[0];
            textBlockStripState1.DataContext = m_handler.m_roller.m_aLine[1];
            textBlockStripState2.DataContext = m_handler.m_roller.m_aLine[2];
            textBlockStripState3.DataContext = m_handler.m_roller.m_aLine[3];
            textBlockRailState0.DataContext = m_handler.m_rail.m_aLine[0];
            textBlockRailState1.DataContext = m_handler.m_rail.m_aLine[1];
            textBlockRailState2.DataContext = m_handler.m_rail.m_aLine[2];
            textBlockRailState3.DataContext = m_handler.m_rail.m_aLine[3];
            checkBoxEQStop.DataContext = EQ.m_EQ; 
            progressUI.Init(m_handler.m_rinse, m_engineer);
            labelSend.DataContext = m_handler.m_rinse; 
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
        private void buttonMode_Click(object sender, RoutedEventArgs e)
        {
            m_handler.m_rinse.p_eMode = (RinseU.eRunMode)(1 - (int)m_handler.m_rinse.p_eMode);
            tabControlStorage.SelectedIndex = (int)m_handler.m_rinse.p_eMode;
        }

        private void buttonHome_Click(object sender, RoutedEventArgs e)
        {
            foreach (Roller.Line line in m_handler.m_roller.m_aLine) line.p_eSensor = Roller.Line.eSensor.Empty; 
            EQ.p_bStop = false;
            EQ.p_eState = EQ.eState.Home;
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (m_handler.m_storage.IsProtrusion()) return; 
            foreach (Storage.Magazine magazine in m_handler.m_storage.m_aMagazine) magazine.RunClamp(magazine.p_bCheck);
            EQ.p_bStop = false; 
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
            m_handler.Reset();
            EQ.p_bStop = false; 
            EQ.p_eState = EQ.eState.Ready;
        }

        private void textBoxWidth_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            DependencyProperty property = TextBox.TextProperty;
            BindingExpression binding = BindingOperations.GetBindingExpression((TextBox)sender, property);
            if (binding != null) binding.UpdateSource();
        }
        #endregion
    }
}
