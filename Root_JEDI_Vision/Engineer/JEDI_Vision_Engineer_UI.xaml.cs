using RootTools;
using RootTools.Trees;
using System.Windows.Controls;

namespace Root_JEDI_Vision.Engineer
{
    /// <summary>
    /// JEDI_Vision_Engineer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class JEDI_Vision_Engineer_UI : UserControl
    {
        public JEDI_Vision_Engineer_UI()
        {
            InitializeComponent();
        }

        JEDI_Vision_Engineer m_engineer = null;
        public void Init(JEDI_Vision_Engineer engineer)
        {
            m_engineer = engineer;
            logViewUI.Init(LogView._logView);
            treeRootUI.Init(engineer.m_treeRoot);
            loginUI.Init(engineer.m_login);
            toolBoxUI.Init(engineer.ClassToolBox());
            handlerUI.Init(engineer.m_handler);
            engineer.RunTree(Tree.eMode.Init);
        }
    }
}
