using System.Windows.Controls;

namespace Root_MarsLogView
{
    /// <summary>
    /// ListError_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ListError_UI : UserControl
    {
        public ListError_UI()
        {
            InitializeComponent();
        }

        ListError m_listError;
        public void Init(ListError listError)
        {
            m_listError = listError;
            DataContext = listError;
            dataGrid.ItemsSource = listError.p_aError;
        }
    }
}
