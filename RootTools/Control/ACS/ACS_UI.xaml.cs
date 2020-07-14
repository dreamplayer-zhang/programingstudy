using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.Control.ACS
{
    /// <summary>
    /// ACS_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ACS_UI : UserControl
    {
        public ACS_UI()
        {
            InitializeComponent();
        }

        public void Init(ACS acs)
        {
            this.DataContext = acs;
            acsDIOUI.Init(acs.m_dio);
            acsListAxisUI.Init(acs.m_listAxis);
            treeRootUI.Init(acs.m_treeRoot);
            acs.RunTree(Tree.eMode.Init);
        }
    }
}
