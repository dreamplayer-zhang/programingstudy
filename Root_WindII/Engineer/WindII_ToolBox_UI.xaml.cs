using RootTools;
using RootTools.Trees;
using System.Windows.Controls;

namespace Root_WindII.Engineer
{
    /// <summary>
    /// WindII_ToolBox_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WindII_ToolBox_UI : UserControl
    {
        public WindII_ToolBox_UI()
        {
            InitializeComponent();
        }

        WindII_Engineer m_engineer;
        public void Init(WindII_Engineer engineer)
        {
            m_engineer = engineer;
            logViewUI.Init(LogView._logView);
            treeRootUI.Init(engineer.m_treeRoot);
            loginUI.Init(engineer.m_login);
            toolBoxUI.Init(engineer.ClassToolBox());
            engineer.RunTree(Tree.eMode.Init);
        }
    }
}
