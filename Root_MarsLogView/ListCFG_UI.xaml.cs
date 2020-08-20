using System.Windows.Controls;

namespace Root_MarsLogView
{
    /// <summary>
    /// ListCFG_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ListCFG_UI : UserControl
    {
        public ListCFG_UI()
        {
            InitializeComponent();
        }

        ListCFG m_listCFG;
        public void Init(ListCFG listCFG)
        {
            m_listCFG = listCFG;
            DataContext = listCFG;
            dataGrid.ItemsSource = listCFG.p_aCFGView;
        }
    }
}
