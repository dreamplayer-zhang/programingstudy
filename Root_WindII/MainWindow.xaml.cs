using Root_WindII.Engineer;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_WindII
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

        WindII_Engineer m_engineer = new WindII_Engineer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!Directory.Exists(@"C:\Recipe\Wind2")) Directory.CreateDirectory(@"C:\Recipe\Wind2");
            //m_engineer.Init("Wind2");
            //engineerUI.Init(m_engineer);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }
    }
}
