using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.ParticleCounter
{
    /// <summary>
    /// ParticleCounterBase_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ParticleCounterBase_UI : UserControl
    {
        public ParticleCounterBase_UI()
        {
            InitializeComponent();
        }

        ParticleCounterBase m_pcb;
        public void Init(ParticleCounterBase pcb)
        {
            m_pcb = pcb;
            DataContext = pcb;
            treeRootUI.Init(pcb.m_treeRoot); 
            modbusUI.Init(pcb.m_modbus);
            pcb.RunTree(Tree.eMode.Init); 
        }
    }
}
