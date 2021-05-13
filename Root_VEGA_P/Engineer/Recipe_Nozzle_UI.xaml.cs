using System.Windows.Controls;

namespace Root_VEGA_P.Engineer
{
    /// <summary>
    /// Recipe_Nozzle_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Recipe_Nozzle_UI : UserControl
    {
        public Recipe_Nozzle_UI()
        {
            InitializeComponent();
        }
        
        public void Init(VEGA_P_Handler handler)
        {
            nozzleSetEOPDoor.Init(handler.m_EOP.m_door.m_particleCounterSet.m_nozzleSet);
            nozzleSetEIPPlate.Init(handler.m_EIP_Plate.m_particleCounterSet.m_nozzleSet);
            nozzleSetEIPCover.Init(handler.m_EIP_Cover.m_particleCounterSet.m_nozzleSet);
            nozzleSetEOPDome.Init(handler.m_EOP.m_dome.m_particleCounterSet.m_nozzleSet); 
        }
    }
}
