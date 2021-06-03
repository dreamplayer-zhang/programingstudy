using Root_WindII.Engineer;
using RootTools_Vision;
using System.ComponentModel;
using System.IO;
using System.Windows;
using RootTools.Gem.XGem;

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

        //WindII_Engineer m_engineer = new WindII_Engineer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!Directory.Exists(@"C:\Recipe\Wind2")) Directory.CreateDirectory(@"C:\Recipe\Wind2");
            //m_engineer.Init("Wind2");
            //engineerUI.Init(m_engineer);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            GlobalObjects.Instance.Get<WindII_Engineer>().ThreadStop();
        }

        private void GemOffline_Click(object sender, RoutedEventArgs e)
        {
            GlobalObjects.Instance.Get<WindII_Engineer>().ClassGem().p_eReqControl = XGem.eControl.OFFLINE;
        }

        private void GemLocal_Click(object sender, RoutedEventArgs e)
        {
            GlobalObjects.Instance.Get<WindII_Engineer>().ClassGem().p_eReqControl = XGem.eControl.LOCAL;
        }

        private void GemOnline_Click(object sender, RoutedEventArgs e)
        {
            GlobalObjects.Instance.Get<WindII_Engineer>().ClassGem().p_eReqControl = XGem.eControl.ONLINEREMOTE;
        }
    }
}
