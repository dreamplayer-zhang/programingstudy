using Root_Rinse_Loader.Engineer;
using System.Windows;

namespace Root_Rinse_Loader
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
        RinseL_Engineer m_engineer = new RinseL_Engineer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_engineer.Init("Rinse_Loader");
        }
        #endregion

        #region Closing
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }
        #endregion

    }
}
