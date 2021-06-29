using Root_Pine2_Vision.Engineer;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_Pine2_Vision
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

        #region Loaded
        Pine2_Vision_Engineer m_engineer = new Pine2_Vision_Engineer();
        Pine2_Vision_Handler m_handler;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_engineer.Init("Pine2");
            engineerUI.Init(m_engineer);
            m_handler = (Pine2_Vision_Handler)m_engineer.ClassHandler();
            Init();
        }

        void Init()
        {
        }
        #endregion

        #region Closing
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }
        #endregion
    }
}
