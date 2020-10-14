using Root_ASIS.Module;
using RootTools.Trees;
using System.Windows.Controls;

namespace Root_ASIS
{
    /// <summary>
    /// Teach_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainTeach_UI : UserControl
    {
        public MainTeach_UI()
        {
            InitializeComponent();
        }

        ASIS_Engineer m_engineer = null;
        ASIS_Handler m_handler;
        MainTeach m_teach; 
        public void Init(MainTeach teach, ASIS_Engineer engineer)
        {
            m_engineer = engineer; 
            m_handler = (ASIS_Handler)engineer.ClassHandler();
            m_teach = teach; 
            teach0UI.Init(m_handler.m_aBoat[Boat.eBoat.Boat0].m_teach);
            teach1UI.Init(m_handler.m_aBoat[Boat.eBoat.Boat1].m_teach);
            treeUI.Init(m_teach.m_treeRoot);
            m_teach.RunTree(Tree.eMode.Init); 
        }
    }
}
