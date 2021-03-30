using RootTools;
using RootTools.Trees;
using System.Windows.Controls;

namespace Root_VEGA_P_Vision.Engineer
{
    /// <summary>
    /// VEGA_P_Vision_ToolBox_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VEGA_P_Vision_ToolBox_UI : UserControl
    {
        public VEGA_P_Vision_ToolBox_UI()
        {
            InitializeComponent();
        }

        VEGA_P_Vision_Engineer m_engineer;
        public void Init(VEGA_P_Vision_Engineer engineer)
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
