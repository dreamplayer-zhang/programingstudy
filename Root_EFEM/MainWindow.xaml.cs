using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_EFEM
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
            if (!Directory.Exists(@"C:\Recipe\EFEM")) Directory.CreateDirectory(@"C:\Recipe\EFEM");
            Init(); 
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop(); 
        }
        #endregion

        EFEM_Engineer m_engineer = new EFEM_Engineer();
        void Init()
        {
            m_engineer.Init("EFEM");
            engineerUI.Init(m_engineer); 
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }
    }
}
