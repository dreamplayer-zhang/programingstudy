using System.Windows.Controls;
using System.Windows.Input;

namespace RootTools.Control
{
    /// <summary>
    /// ListDO_LongID_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ListDIO_LongID_UI : UserControl
    {
        public ListDIO_LongID_UI()
        {
            InitializeComponent();
        }

        private void TextBlock_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (m_listDIO.m_eDIO == ListDIO.eDIO.Input)
            {
                BitDI bitDi = (BitDI)listView.SelectedItem;
                if (bitDi == null) return;
                if (EQ.p_bSimulate) bitDi.p_bOn = !bitDi.p_bOn;
            }
            else if (m_listDIO.m_eDIO == ListDIO.eDIO.Output)
            {
                BitDO bitDO = (BitDO)listView.SelectedItem;
                if (bitDO == null) return;
                bitDO.Write(!bitDO.p_bOn);
            }
        }

        ListDIO m_listDIO; 
        public void Init(ListDIO listDIO)
        {
            m_listDIO = listDIO;
            DataContext = listDIO; 
            listView.ItemsSource = listDIO.m_aDIO;
        }
    }
}
