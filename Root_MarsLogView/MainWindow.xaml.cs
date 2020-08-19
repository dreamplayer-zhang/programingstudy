using System.Windows;

namespace Root_MarsLogView
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        MarsLogViewer m_logViewer = new MarsLogViewer(); 
        public MainWindow()
        {
            InitializeComponent();
            logViewerUI.Init(m_logViewer); 
        }
    }
}
