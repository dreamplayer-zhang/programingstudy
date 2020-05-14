using System.Windows.Controls;

namespace RootTools
{
    /// <summary>
    /// Log_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Log_Group_UI : UserControl
    {
        public Log_Group_UI()
        {
            InitializeComponent();
        }

        Log_Group m_logGroup; 
        public void Init(Log_Group logGroup)
        {
            m_logGroup = logGroup;
            DataContext = logGroup;
            dataGrid.ItemsSource = logGroup.p_aLog; 
        }
    }
}
