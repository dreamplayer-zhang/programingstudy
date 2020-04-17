using System.Windows.Controls;

namespace RootTools
{
    /// <summary>
    /// InfoList_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InfoList_UI : UserControl
    {
        public InfoList_UI()
        {
            InitializeComponent();
        }

        InfoList m_infoList; 
        public void Init(InfoList infoList)
        {
            m_infoList = infoList;
            this.DataContext = infoList; 
        }
    }
}
