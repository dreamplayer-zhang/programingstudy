using Root_Pine2.Engineer;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_Pine2
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
        Pine2_Engineer m_engineer = new Pine2_Engineer();
        Pine2_Handler m_handler;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\Pine2")) Directory.CreateDirectory(@"C:\Recipe\Pine2");
            m_engineer.Init("Pine2");
            m_handler = (Pine2_Handler)m_engineer.ClassHandler();
            engineerUI.Init(m_engineer);
            mainUI.Init(m_engineer); 
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
