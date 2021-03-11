using RootTools.Trees;
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
            tabSetup.SelectedIndex = 0;
            comboLog.SelectedIndex = 0;
            UpdateLogTab();
            logView.OnChangeTab += LogView_OnChangeTab;
            treeRootUI.Init(logView.m_treeRoot);
            logView.RunTree(Tree.eMode.Init); 
        }

        private void LogView_OnChangeTab()
        {
            UpdateLogTab();
            comboLog.SelectedIndex = 0;
            comboLog.SelectedIndex = 0; 
        }

        void UpdateLogTab()
        {
            foreach (LogGroup log in m_logView.m_aGroup) UpdateLogTab(log);
            m_asLog.Clear();
            comboLog.ItemsSource = null;
            foreach (LogGroup log in m_logView.m_aGroup) m_asLog.Add(log.p_id);
            comboLog.ItemsSource = m_asLog;
            
        }

        void UpdateLogTab(LogGroup log)
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
