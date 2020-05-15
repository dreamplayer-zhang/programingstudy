using System.Collections.Generic;
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
            dataGrid.ItemsSource = logGroup.p_aLog;
            foreach (DataGridColumn column in dataGrid.Columns) m_asColumn.Add((string)column.Header);
            comboFilter.ItemsSource = m_asColumn;
            comboFilter.SelectedIndex = 2; 
        }

        List<string> m_asColumn = new List<string>(); 
        private void buttonFilter_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
