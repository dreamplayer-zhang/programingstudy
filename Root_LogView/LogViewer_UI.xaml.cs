using System.Collections.Generic;
using System.IO;
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

        private void tabLog_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            e.Effects = DragDropEffects.Copy;
        }

        private void tabLog_DragLeave(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None; 
        }

        private void tabLog_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            string[] asDrop = (string[])e.Data.GetData(DataFormats.FileDrop);
            comboLog.ItemsSource = null;
            foreach (string sDrop in asDrop) OpenDrop(sDrop);
            comboLog.ItemsSource = m_asLog;
            if (comboLog.SelectedIndex < 0) comboLog.SelectedIndex = 0;
        }

        void OpenDrop(string sDrop)
        {
            if (Directory.Exists(sDrop))
            {
                string[] asFile = Directory.GetFiles(sDrop);
                foreach (string sFile in asFile) OpenDrop(sFile);
                string[] asFolder = Directory.GetDirectories(sDrop);
                foreach (string sFolder in asFolder) OpenDrop(sFolder);
            }
            else OpenLog(sDrop); 
        }

        void OpenLog(string sFile)
        {
            if (CheckFileFilter(sFile) == false) return; 
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

        bool CheckFileFilter(string sFile)
        {
            if (CheckFileFilter(sFile, textBoxFileFilter0.Text) == false) return false;
            if (CheckFileFilter(sFile, textBoxFileFilter1.Text) == false) return false;
            if (CheckFileFilter(sFile, textBoxFileFilter2.Text) == false) return false;
            return true; 
        }

        bool CheckFileFilter(string sFile, string sFilter)
        {
            if (sFilter == "") return true;
            return sFile.Contains(sFilter); 
        }
    }
}
