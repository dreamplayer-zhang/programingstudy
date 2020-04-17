using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.Control.Ajin
{
    /// <summary>
    /// Ajin_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Ajin_UI : UserControl
    {
        public Ajin_UI()
        {
            InitializeComponent();
        }

        public void Init(Ajin ajin)
        {
            this.DataContext = ajin;
            ajinDIOUI.Init(ajin.m_dio);
            ajinListAxisUI.Init(ajin.m_listAxis); 
            treeRootUI.Init(ajin.m_treeRoot);
            ajin.RunTree(Tree.eMode.Init); 
        }
    }
}
