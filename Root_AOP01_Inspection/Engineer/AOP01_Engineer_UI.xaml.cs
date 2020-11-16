using RootTools;
using RootTools.Trees;
using System.Windows.Controls;

namespace Root_AOP01_Inspection
{
    /// <summary>
    /// AUP01_Engineer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AOP01_Engineer_UI : UserControl
    {
        public AOP01_Engineer_UI()
        {
            InitializeComponent();
        }

        AOP01_Engineer m_engineer = null;
        public void Init(AOP01_Engineer engineer)
        {
            m_engineer = engineer;
            logViewUI.Init(LogView.m_logView);
            treeRootUI.Init(engineer.m_treeRoot);
            loginUI.Init(engineer.m_login);
            toolBoxUI.Init(engineer.ClassToolBox());
            //handlerUI.Init(engineer.m_handler); // 2020-11-13 
            engineer.RunTree(Tree.eMode.Init);
        }
    }
}
