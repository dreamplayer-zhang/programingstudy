using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RootTools
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
            foreach (ILogGroup log in m_logView.m_aLogGroup) UpdateLogTab(log);
            m_asLog.Clear();
            comboLog.ItemsSource = null;
            foreach (ILogGroup log in m_logView.m_aLogGroup) m_asLog.Add(log.p_id);
            comboLog.ItemsSource = m_asLog; 
        }

        void UpdateLogTab(ILogGroup log)
        {
            foreach (TabItem tabItem in tabLog.Items)
            {
                if ((string)tabItem.Header == log.p_id) return; 
            }
            TabItem item = new TabItem();
            item.Header = log.p_id;
            item.Height = 0; 
            item.Content = log.p_ui;
            item.Background = Brushes.AliceBlue;
            tabLog.Items.Add(item);
        }

        List<string> m_asLog = new List<string>();
        private void comboLog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboLog.SelectedIndex < 0) return;
            tabLog.SelectedIndex = comboLog.SelectedIndex; 
        }
    }
}
