using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Root_LogView
{
    /// <summary>
    /// LogViewer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogViewer_UI : UserControl
    {
        public LogViewer_UI()
        {
            InitializeComponent();
        }

        LogViewer m_logViewer; 
        public void Init(LogViewer logViewer)
        {
            m_logViewer = logViewer;
            DataContext = logViewer;
            InitTabClip(); 
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true; 
            dlg.Filter = "Log Files (*.log)|*.log";
            if (dlg.ShowDialog() == false)
            {
                MessageBox.Show("Log File not Found");
                return; 
            }
            comboLog.ItemsSource = null;
            foreach (string sFile in dlg.FileNames) OpenLog(sFile); 
            comboLog.ItemsSource = m_asLog;
            comboLog.SelectedIndex = 0; 
        }

        void OpenLog(string sFile)
        {
            foreach (string str in m_asLog)
            {
                if (str == sFile) return;
            }
            m_asLog.Add(sFile);
            LogGroup_UI ui = m_logViewer.OpenLog(sFile);
            TabItem item = new TabItem();
            item.Header = sFile;
            item.Height = 0;
            item.Content = ui;
            tabLog.Items.Add(item);
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            tabLog.Items.Clear();
            m_asLog.Clear();
            comboLog.ItemsSource = null; 
            m_logViewer.m_aLog.Clear(); 
        }

        List<string> m_asLog = new List<string>();
        private void comboLog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboLog.SelectedIndex < 0) return;
            tabLog.SelectedIndex = comboLog.SelectedIndex;
        }

        void InitTabClip()
        {
            TabItem item = new TabItem();
            item.Header = "Clip";
            item.Content = m_logViewer.m_logClip.p_ui;
            tabMain.Items.Add(item); 
        }
    }
}
