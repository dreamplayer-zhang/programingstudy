using Root_VEGA_D.Engineer;
using System.IO;
using System.Windows;

namespace Root_VEGA_D
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
            if (!Directory.Exists(@"C:\Recipe\VEGA_D")) Directory.CreateDirectory(@"C:\Recipe\VEGA_D");
            Init();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        VEGA_D_Engineer m_engineer = new VEGA_D_Engineer();
        void Init()
        {
            m_engineer.Init("VEGA_D");
            engineerUI.Init(m_engineer);
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }
    }
}
