using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.Control.Ajin
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AjinAxis_UI : UserControl
    {
        public AjinAxis_UI()
        {
            InitializeComponent();
        }

        AjinAxis m_axis;
        public void Init(AjinAxis axis)
        {
            this.DataContext = axis;
            m_axis = axis;
            //treeRootMainUI.Init(axis.p_treeRootMain);
            axis.RunTree(Tree.eMode.Init);
            axis.RunSetupTree(Tree.eMode.Init); 
        }
    }
}
