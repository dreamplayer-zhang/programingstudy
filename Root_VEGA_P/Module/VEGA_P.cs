using RootTools;
using RootTools.Module;

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

        public FlowSensor m_flowSensor;
        public VEGA_P(string id, IEngineer engineer)
        {
            m_flowSensor = new FlowSensor("FlowSensor", this);
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
