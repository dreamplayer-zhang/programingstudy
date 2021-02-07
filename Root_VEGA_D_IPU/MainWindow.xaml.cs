using Root_VEGA_D_IPU.Engineer;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_VEGA_D_IPU
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
            if (!Directory.Exists(@"C:\Recipe\VEGA_D_IPU")) Directory.CreateDirectory(@"C:\Recipe\VEGA_D_IPU");
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }

        VEGA_D_IPU_Engineer m_engineer = new VEGA_D_IPU_Engineer();
        void Init()
        {
            m_engineer.Init("VEGA_D_IPU");
            engineerUI.Init(m_engineer);
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }
        #endregion

    }
}
