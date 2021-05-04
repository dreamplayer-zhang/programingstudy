using Root_WindII_Option.Engineer;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_WindII_Option
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

        //WindII_Option_Engineer m_engineer = new WindII_Option_Engineer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!Directory.Exists(@"C:\Recipe\Wind2Option")) Directory.CreateDirectory(@"C:\Recipe\Wind2Option");
            //m_engineer.Init("Wind2_Option");
            //engineerUI.Init(m_engineer);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //m_engineer.ThreadStop();
        }
    }
}
