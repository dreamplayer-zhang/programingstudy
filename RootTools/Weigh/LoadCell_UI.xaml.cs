using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.Weigh
{
    /// <summary>
    /// CAS_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoadCell_UI : UserControl
    {
        public LoadCell_UI()
        {
            InitializeComponent();
        }
        LoadCell m_CAS;
        public void Init(LoadCell cas)
        {
            m_CAS = cas;
            DataContext = cas;
            rs232UI.Init(cas.p_rs232);
            treeRootUI.Init(cas.m_treeRootSetup);
            cas.RunTreeSetup(Tree.eMode.Init);
        }
    }
}
