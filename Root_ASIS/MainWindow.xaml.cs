using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_ASIS
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
            if (!Directory.Exists(@"C:\Recipe\ASIS")) Directory.CreateDirectory(@"C:\Recipe\ASIS");
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        ASIS_Engineer m_engineer = new ASIS_Engineer();
        MainTeach m_teach; 
        void Init()
        {
            m_engineer.Init("ASIS");
            engineerUI.Init(m_engineer);
            m_teach = new MainTeach("Teach", m_engineer); 
            teachUI.Init(m_teach, m_engineer);
            mainUI.Init(m_engineer);
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }
    }
}
