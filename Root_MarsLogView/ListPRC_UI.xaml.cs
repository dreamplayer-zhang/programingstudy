using System.Windows.Controls;

namespace Root_MarsLogView
{
    /// <summary>
    /// ListPRC_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ListPRC_UI : UserControl
    {
        public ListPRC_UI()
        {
            InitializeComponent();
        }

        ListPRC m_listPRC; 
        public void Init(ListPRC listPRC)
        {
            m_listPRC = listPRC;
            DataContext = listPRC;
            dataGrid.ItemsSource = listPRC.p_aPRCView; 
        }
    }
}
