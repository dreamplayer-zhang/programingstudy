using System;
using System.Windows.Controls;

namespace Root_LogView.Server
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

        LogServer.LogGroup m_logGroup; 
        public void Init(LogServer.LogGroup logGroup)
        {
            m_logGroup = logGroup;
            DataContext = logGroup;
            dataGrid.ItemsSource = logGroup.p_aLog;
            dataGrid.LayoutUpdated += DataGrid_LayoutUpdated;
            p_nCount = dataGrid.Items.Count;
        }

        int _nCount = 0;
        int p_nCount
        {
            set
            {
                if (_nCount == value) return;
                _nCount = value;
                if (value > 0) dataGrid.ScrollIntoView(dataGrid.Items[value - 1]);
            }
        }

        private void DataGrid_LayoutUpdated(object sender, EventArgs e)
        {
            p_nCount = dataGrid.Items.Count;
        }
    }
}
