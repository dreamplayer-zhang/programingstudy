using RootTools.Trees;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Root_LogView.Server
{
    /// <summary>
    /// LogServer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogServer_UI : UserControl
    {
        public LogServer_UI()
        {
            InitializeComponent();
        }

        LogServer m_logServer;
        public void Init(LogServer logServer)
        {
            m_logServer = logServer;
            DataContext = logServer;
            tabSetup.SelectedIndex = 0;
            comboLog.SelectedIndex = 0;
            UpdateLogTab();
            logServer.OnChangeTab += LogServer_OnChangeTab;
            treeRootUI.Init(logServer.m_treeRoot);
            logServer.RunTree(Tree.eMode.Init); 
        }

        private void LogServer_OnChangeTab()
        {
            UpdateLogTab();
            tabSetup.SelectedIndex = 0;
            comboLog.SelectedIndex = 0;
        }

        void UpdateLogTab()
        {
            foreach (LogServer.LogGroup log in m_logServer.m_aGroup) UpdateLogTab(log);
            m_asLog.Clear();
            comboLog.ItemsSource = null;
            foreach (LogServer.LogGroup log in m_logServer.m_aGroup) m_asLog.Add(log.p_id);
            comboLog.ItemsSource = m_asLog;

        }

        void UpdateLogTab(LogServer.LogGroup log)
        {
            foreach (TabItem tabItem in tabLog.Items)
            {
                if ((string)tabItem.Header == log.p_id) return;
            }
            TabItem item = new TabItem();
            item.Header = log.p_id;
            item.Height = 0;
            item.Content = log.p_ui;
            tabLog.Items.Add(item);
        }

        List<string> m_asLog = new List<string>();
        private void comboLog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboLog.SelectedIndex < 0) return;
            tabSetup.SelectedIndex = 0;
            checkSetup.IsChecked = false; 
            tabLog.SelectedIndex = comboLog.SelectedIndex;
        }

        private void checkSetup_Click(object sender, RoutedEventArgs e)
        {
            tabSetup.SelectedIndex = (checkSetup.IsChecked == true) ? 1 : 0;
        }
    }
}
