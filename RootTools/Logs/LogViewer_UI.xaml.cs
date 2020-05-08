using System.Windows.Controls;
using System.Windows.Media;

namespace RootTools.Logs
{
    /// <summary>
    /// LogView_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogViewer_UI : UserControl
    {
        public LogViewer_UI()
        {
            InitializeComponent();
        }

        LogViewer m_logView; 
        public void Init(LogViewer logView)
        {
            m_logView = logView;
            DataContext = logView;
            UpdateLogTab();
            logView.OnChangeTab += LogView_OnChangeTab;
        }

        private void LogView_OnChangeTab()
        {
            UpdateLogTab();
        }

        void UpdateLogTab()
        {
            tabLog.Items.Clear(); 
            foreach (ILog logSet in m_logView.m_aLogSet)
            {
                TabItem item = new TabItem();
                item.Header = logSet.p_id;
                item.Content = logSet.p_ui;
                item.Background = Brushes.AliceBlue; 
                tabLog.Items.Add(item); 
            }
        }
    }
}
