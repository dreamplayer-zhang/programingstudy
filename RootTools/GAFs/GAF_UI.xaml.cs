using System.Windows.Controls;

namespace RootTools.GAFs
{
    /// <summary>
    /// GAF_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GAF_UI : UserControl
    {
        public GAF_UI()
        {
            InitializeComponent();
        }

        GAF m_gaf;
        public void Init(GAF gaf)
        {
            if (gaf == null) return; 
            m_gaf = gaf;
            this.DataContext = gaf;
            ceidListUI.Init(gaf.m_listCEID);
            svidListUI.Init(gaf.m_listSVID);
            alidListUI.Init(gaf.m_listALID);
        }
    }
}
