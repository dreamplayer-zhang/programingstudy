using System.Windows.Controls;

namespace Root_MarsLogView
{
    /// <summary>
    /// ListFNC_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ListFNC_UI : UserControl
    {
        public ListFNC_UI()
        {
            InitializeComponent();
        }

        ListFNC m_listFNC;
        public void Init(ListFNC listFNC)
        {
            m_listFNC = listFNC;
            DataContext = listFNC;
            dataGrid.ItemsSource = listFNC.p_aFNCView;
        }
    }
}
