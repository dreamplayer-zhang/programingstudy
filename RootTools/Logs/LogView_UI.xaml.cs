using System.Windows.Controls;
using System.Windows.Media;

namespace RootTools.Logs
{
    /// <summary>
    /// LogView_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogView_UI : UserControl
    {
        public LogView_UI()
        {
            InitializeComponent();
        }

        _LogView m_logView; 
        public void Init(_LogView logView)
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
            foreach (ILog log in m_logView.m_aLogSet) UpdateLogTab(log); 
        }

        void UpdateLogTab(ILog log)
        {
            foreach (TabItem tabItem in tabLog.Items)
            {
                if ((string)tabItem.Header == log.p_id) return; 
            }
            TabItem item = new TabItem();
            item.Header = log.p_id;
            item.Content = log.p_ui;
            item.Background = Brushes.AliceBlue;
            tabLog.Items.Add(item);
        }
    }
}
