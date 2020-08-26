using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_AUP01
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

        #region Window Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\AUP01")) Directory.CreateDirectory(@"C:\Recipe\AUP01");
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        AUP01_Engineer m_engineer = new AUP01_Engineer();
        void Init()
        {
            m_engineer.Init("AUP01");
            engineerUI.Init(m_engineer);
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }

    }
}
