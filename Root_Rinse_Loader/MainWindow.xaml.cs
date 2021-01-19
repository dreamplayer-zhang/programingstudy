using Root_Rinse_Loader.Engineer;
using System.ComponentModel;
using System.IO;
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
            comboMain.ItemsSource = new string[] { "Main UI", "Engineer" };
        }

        #region Loaded
        RinseL_Engineer m_engineer = new RinseL_Engineer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\Rinse_Loader")) Directory.CreateDirectory(@"C:\Recipe\Rinse_Loader");
            m_engineer.Init("Rinse_Loader");
            engineerUI.Init(m_engineer); 
            mainUI.Init(m_engineer); 
        }
        #endregion

        #region Closing
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }
        #endregion

        #region UI Controls
        private void comboMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboMain.SelectedIndex < 0) return;
            tabMain.SelectedIndex = comboMain.SelectedIndex; 
        }
        #endregion
    }
}
