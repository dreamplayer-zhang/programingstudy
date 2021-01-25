using Root_Rinse_Loader.Engineer;
using Root_Rinse_Loader.Module;
using RootTools;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Root_Rinse_Loader
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            comboMain.ItemsSource = new string[] { "Main UI", "Engineer" };
            comboMain.SelectedIndex = 0; 
        }

        #region Loaded
        RinseL_Engineer m_engineer = new RinseL_Engineer();
        RinseL_Handler m_handler; 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\Rinse_Loader")) Directory.CreateDirectory(@"C:\Recipe\Rinse_Loader");
            m_engineer.Init("Rinse_Loader");
            engineerUI.Init(m_engineer);
            mainUI.Init(m_engineer);
            m_handler = (RinseL_Handler)m_engineer.ClassHandler();
        }

        void Init()
        {
            textBlockState.DataContext = EQ.m_EQ;
            textBolckUnloadState.DataContext = m_handler.m_rinse;
            textBlockRinseState.DataContext = m_handler.m_rinse;
            buttonMode.DataContext = m_handler.m_rinse;
            textBoxWidth.DataContext = m_handler.m_rinse; 
        }
        #endregion

        #region Closing
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }
        #endregion

        #region UI Controls
        private void comboMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboMain.SelectedIndex < 0) return;
            tabMain.SelectedIndex = comboMain.SelectedIndex; 
        }
        #endregion

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

        private void buttonMode_Click(object sender, RoutedEventArgs e)
        {
            m_handler.m_rinse.p_eMode = (RinseL.eRunMode)(1 - (int)m_handler.m_rinse.p_eMode);
        }
    }
}
