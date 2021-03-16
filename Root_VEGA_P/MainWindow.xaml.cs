using Root_VEGA_P.Engineer;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_VEGA_P
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

        VEGA_P_Engineer m_engineer = new VEGA_P_Engineer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\VEGA_P")) Directory.CreateDirectory(@"C:\Recipe\VEGA_P");
            m_engineer.Init("VEGA_P");
            engineerUI.Init(m_engineer);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }
    }
}
