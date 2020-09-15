using RootTools;
using RootTools.Trees;
using System.Windows.Controls;

namespace Root_ASIS
{
    /// <summary>
    /// ASIS_Engineer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASIS_Engineer_UI : UserControl
    {
        public ASIS_Engineer_UI()
        {
            InitializeComponent();
        }

        ASIS_Engineer m_engineer = null;
        public void Init(ASIS_Engineer engineer)
        {
            m_engineer = engineer;
            logViewUI.Init(LogView.m_logView);
            treeRootUI.Init(engineer.m_treeRoot);
            loginUI.Init(engineer.m_login);
            toolBoxUI.Init(engineer.ClassToolBox());
            handlerUI.Init(engineer.m_handler);
            engineer.RunTree(Tree.eMode.Init);
        }
    }
}
