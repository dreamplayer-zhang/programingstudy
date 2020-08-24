using System.Windows.Controls;

namespace Root_MarsLogView
{
    /// <summary>
    /// ListXFR_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ListXFR_UI : UserControl
    {
        public ListXFR_UI()
        {
            InitializeComponent();
        }

        ListXFR m_listXFR;
        public void Init(ListXFR listXFR)
        {
            m_listXFR = listXFR;
            DataContext = listXFR;
            dataGrid.ItemsSource = listXFR.p_aXFRView;
        }
    }
}
