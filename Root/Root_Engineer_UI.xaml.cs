using RootTools;
using System.Windows.Controls;

namespace Root
{
    /// <summary>
    /// Engineer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Root_Engineer_UI : UserControl
    {
        public Root_Engineer_UI()
        {
            InitializeComponent();
        }

        Root_Engineer m_engineer = null; 
        public void Init(Root_Engineer engineer)
        {
            m_engineer = engineer; 
            logViewUI.Init(LogViewer.m_logView); 
            loginUI.Init(engineer.m_login);
            toolBoxUI.Init(engineer.ClassToolBox());
            handlerUI.Init(engineer.m_handler); 
        }
    }
}
