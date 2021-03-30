using RootTools;
using RootTools.Trees;
using System.Windows.Controls;

namespace Root_WindII_Option.Engineer
{
    /// <summary>
    /// _bPresent_ToolBox_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WindII_Option_ToolBox_UI : UserControl
    {
        public WindII_Option_ToolBox_UI()
        {
            InitializeComponent();
        }

        WindII_Option_Engineer m_engineer;
        public void Init(WindII_Option_Engineer engineer)
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
