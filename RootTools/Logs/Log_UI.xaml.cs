using System.Windows.Controls;

namespace RootTools.Logs
{
    /// <summary>
    /// Log_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Log_UI : UserControl
    {
        public Log_UI()
        {
            InitializeComponent();
        }

        Log m_log; 
        public void Init(Log log)
        {
            m_log = log;
            DataContext = log; 
        }
    }
}
