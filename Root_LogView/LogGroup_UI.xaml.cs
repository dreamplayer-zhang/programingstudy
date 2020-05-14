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
        }

        private void comboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void buttonFilter_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
