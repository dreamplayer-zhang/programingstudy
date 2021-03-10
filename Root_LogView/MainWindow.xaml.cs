using Root_LogView.Server;
using System.Windows;

namespace Root_LogView
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

        LogServer m_logServer = new LogServer();
        LogViewer m_logViewer = new LogViewer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            logServerUI.Init(m_logServer);
            logViewerUI.Init(m_logViewer);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_logServer.ThreadStop();
        }
    }
}
