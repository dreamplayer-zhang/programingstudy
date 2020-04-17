using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.Comm
{
    /// <summary>
    /// NamedPipe_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NamedPipe_UI : UserControl
    {
        public NamedPipe_UI()
        {
            InitializeComponent();
        }

        NamedPipe m_namedPipe; 
        public void Init(NamedPipe namedPipe)
        {
            m_namedPipe = namedPipe;
            this.DataContext = namedPipe;
            commLogUI.Init(namedPipe.m_commLog);
            treeRootUI.Init(namedPipe.m_treeRoot);
            m_namedPipe.RunTree(Tree.eMode.Init);
        }
    }
}
