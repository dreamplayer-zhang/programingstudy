using Root_Rinse_Unloader.Engineer;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_Rinse_Unloader
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

        #region Loaded
        RinseU_Engineer m_engineer = new RinseU_Engineer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\Rinse_Unloader")) Directory.CreateDirectory(@"C:\Recipe\Rinse_Unloader");
            m_engineer.Init("Rinse_Unloader");
            engineerUI.Init(m_engineer);
        }
        #endregion

        #region Closing
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }
        #endregion
    }
}
