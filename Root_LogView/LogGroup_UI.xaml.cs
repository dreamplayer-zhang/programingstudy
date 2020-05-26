using System;
using System.Windows;
using System.Windows.Controls;

namespace Root_LogView
{
    /// <summary>
    /// LogGroup_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogGroup_UI : UserControl
    {
        public LogGroup_UI()
        {
            InitializeComponent();
        }

        LogGroup m_logGroup; 
        public void Init(LogGroup logGroup)
        {
            m_logGroup = logGroup;
            DataContext = logGroup;
            dataGrid.ItemsSource = logGroup.p_aLogFilter;
            comboLevel.ItemsSource = Enum.GetValues(typeof(LogGroup.eLevel));
            comboLevel.SelectedIndex = 0; 
            if (logGroup.m_logClip == null) stackLog.Width = 0;
            else stackClip.Width = 0; 
        }

        #region Filter
        private void textFilterTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_logGroup.m_filter.m_sTime = ((TextBox)e.Source).Text.ToString();
            m_logGroup.InvalidFilter(); 
        }

        private void textFilterLogger_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_logGroup.m_filter.m_sLogger = ((TextBox)e.Source).Text.ToString();
            m_logGroup.InvalidFilter();
        }

        private void textFilterMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_logGroup.m_filter.m_sMessage = ((TextBox)e.Source).Text.ToString();
            m_logGroup.InvalidFilter();
        }

        private void textFilterStackTrace_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_logGroup.m_filter.m_sStackTrace = ((TextBox)e.Source).Text.ToString();
            m_logGroup.InvalidFilter();
        }

        private void comboLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_logGroup.m_filter.m_eLevel = (LogGroup.eLevel)comboLevel.SelectedItem;
            m_logGroup.InvalidFilter();
        }
        #endregion 

        #region Clip Function
        private void buttonUndo_Click(object sender, RoutedEventArgs e)
        {
            m_logGroup.Undo(); 
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            m_logGroup.RemoveSelection(dataGrid.SelectedItems); 
        }

        private void buttonRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            m_logGroup.RemoveAll();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            m_logGroup.FileSave(dataGrid.Items); 
        }

        private void buttonSaveSimple_Click(object sender, RoutedEventArgs e)
        {
            m_logGroup.FileSave(dataGrid.Items, true);
        }
        #endregion

        #region Log Function
        private void buttonSendClipSelect_Click(object sender, RoutedEventArgs e)
        {
            m_logGroup.StackLog(); 
            m_logGroup.SendClip(dataGrid.SelectedItems);
        }

        private void buttonSendClipAll_Click(object sender, RoutedEventArgs e)
        {
            m_logGroup.StackLog();
            m_logGroup.SendClip(dataGrid.Items);
        }

        private void buttonSendClipAllFile_Click(object sender, RoutedEventArgs e)
        {
            m_logGroup.StackLog();
            m_logGroup.m_logViewer.SendClip(m_logGroup.m_filter); 
        }
        #endregion
    }
}
