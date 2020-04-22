using System.Windows.Controls;

namespace Root_Vega
{
    /// <summary>
    /// _1_Main.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class _1_Main : UserControl
    {
        public _1_Main()
        {
            InitializeComponent();
        }

        Vega_Engineer m_engineer;
        Vega_Handler m_handler; 
        public void Init(Vega_Engineer engineer)
        {
            m_engineer = engineer;
            m_handler = engineer.m_handler; 
            moduleVision.Init(m_handler.m_patternVision);
            moduleSideVision.Init(m_handler.m_sideVision);
            moduleRobot.Init(m_handler.m_robot);
            loadportA.Init(m_handler.m_aLoadport[0]);
            loadportB.Init(m_handler.m_aLoadport[1]);
        }
    }
}
