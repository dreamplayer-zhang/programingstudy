using System.Windows.Controls;
using System.Windows.Media;

namespace RootTools.OHT
{
    /// <summary>
    /// OHTHistory_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OHTHistory_UI : UserControl
    {
        public OHTHistory_UI()
        {
            InitializeComponent();
        }

        OHTBase.History m_history;
        public void Init(OHTBase.History history)
        {
            m_history = history;
            this.DataContext = history;
            listView.ItemsSource = history.m_aData; 
        }
    }
}
