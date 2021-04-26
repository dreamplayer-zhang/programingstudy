using RootTools;
using RootTools.Module;
using RootTools.ParticleCounter;
using RootTools.Trees;

namespace Root_VEGA_P.Module
{
    public class VEGA_P :ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_flowSensor.GetTools(this); 
            if (bInit)
            {
                m_flowSensor.m_modbus.Connect(); 
            }
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }

        public override void InitMemorys()
        {
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_sample.RunTree(tree.GetTree("Particle Counter"), true);
        }
        #endregion

        public FlowSensor m_flowSensor;
        public ParticleCounterBase.Sample m_sample; 
        public VEGA_P(string id, IEngineer engineer)
        {
            m_flowSensor = new FlowSensor("FlowSensor", this);
            m_sample = new ParticleCounterBase.Sample(); 
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
