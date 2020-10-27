using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Root_AOP01
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
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

        #region Window Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\AOP01")) Directory.CreateDirectory(@"C:\Recipe\AOP01");
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        #region Mode UI
        public SelectMode ModeSelect;
        public Setup_Panel Setup;
        public Run_Panel Run;
        #endregion

        #region ViewModel
        private Setup_ViewModel m_Setup;
        private Run_ViewModel m_Run;
        #endregion


        AOP01_Engineer m_engineer = new AOP01_Engineer();

        void Init()
        {
            Init_ViewModel();
            Init_UI();

            //m_engineer.Init("AOP01");
            //engineerUI.Init(m_engineer);
        }
        void Init_ViewModel()
        {
            m_Setup = new Setup_ViewModel(this);
            m_Run = new Run_ViewModel(this);
        }
        void Init_UI()
        {
            ModeSelect = new SelectMode();
            ModeSelect.Init(this);

            Setup = new Setup_Panel();
            Setup.DataContext = m_Setup;

            Run = new Run_Panel();

            MainPanel.Children.Clear();
            MainPanel.Children.Add(ModeSelect);
        }
        void ThreadStop()
        {
            //m_engineer.ThreadStop();
        }

    }
}
