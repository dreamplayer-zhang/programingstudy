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
        }

        private void textFilterTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_logGroup.m_sFilterTime = ((TextBox)e.Source).Text.ToString();
            m_logGroup.InvalidFilter(); 
        }

        private void textFilterLogger_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_logGroup.m_sFilterLogger = ((TextBox)e.Source).Text.ToString();
            m_logGroup.InvalidFilter();
        }

        private void textFilterMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_logGroup.m_sFilterMessage = ((TextBox)e.Source).Text.ToString();
            m_logGroup.InvalidFilter();
        }

        private void textFilterStackTrace_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_logGroup.m_sFilterStackTrace = ((TextBox)e.Source).Text.ToString();
            m_logGroup.InvalidFilter();
        }
    }
}
