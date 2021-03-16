using Root_VEGA_P_Vision.Engineer;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_VEGA_P_Vision
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

        VEGA_P_Vision_Engineer m_engineer = new VEGA_P_Vision_Engineer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\VEGA_P_Vision")) Directory.CreateDirectory(@"C:\Recipe\VEGA_P_Vision");
            m_engineer.Init("VEGA_P_Vision");
            engineerUI.Init(m_engineer);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }
    }
}
