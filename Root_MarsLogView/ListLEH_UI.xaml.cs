using System.Windows.Controls;

namespace Root_MarsLogView
{
    /// <summary>
    /// ListLEH_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ListLEH_UI : UserControl
    {
        public ListLEH_UI()
        {
            InitializeComponent();
        }

        ListLEH m_listLEH;
        public void Init(ListLEH listLEH)
        {
            m_listLEH = listLEH;
            DataContext = listLEH;
            dataGrid.ItemsSource = listLEH.p_aLEHView;
        }
    }
}
