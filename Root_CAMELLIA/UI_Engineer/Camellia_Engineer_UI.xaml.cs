using RootTools;
using RootTools.Trees;
using System.Windows.Controls;

namespace Root_CAMELLIA
{
    /// <summary>
    /// Camellia_Engineer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Camellia_Engineer_UI : UserControl
    {
        public Camellia_Engineer_UI()
        {
            InitializeComponent();
        }
        CAMELLIA_Engineer m_engineer;
        public void Init(CAMELLIA_Engineer engineer)
        {
            m_engineer = engineer;
            logViewUI.Init(LogView._logView);
            handlerUI.Init(engineer.m_handler);
            loginUI.Init(engineer.m_login);
            toolBoxUI.Init(engineer.ClassToolBox());
            treeRootUI.Init(engineer.m_treeRoot);
            engineer.RunTree(Tree.eMode.Init);
        }
    }
}
