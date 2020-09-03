using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using RootTools;
using System.Windows.Media;


namespace Root_Siltron
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
            m_ModeUI.tbDate.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        #endregion

        #region Title Bar
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
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        Siltron_Engineer m_engineer = new Siltron_Engineer();

        void Init()
        {
            m_engineer.Init("Siltron");
            engineerUI.Init(m_engineer);

            //InitTimer();
            //InitUI();
        }

        #region UI
        public SetupHome m_SetupUI;
        private SetupUIManger m_SetupViewModel;
        public SelectMode m_ModeUI;
        void InitUI()
        {
            m_SetupUI = new SetupHome();
            ((SetupUIManger)m_SetupUI.DataContext).init(this);
            m_SetupViewModel = (SetupUIManger)m_SetupUI.DataContext;

            m_ModeUI = new SelectMode();
            m_ModeUI.Init(this);

            Home();
        }
        void Home()
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(m_ModeUI);
        }
        void Setup()
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(m_SetupUI);
        }
        #endregion

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }

        






    }
}
