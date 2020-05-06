using System.Windows;

namespace Root_LogViewer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        LogViewer m_logViewer = new LogViewer(); 
        public MainWindow()
        {
            InitializeComponent();
            logViewerUI.Init(m_logViewer); 
        }
    }
}
