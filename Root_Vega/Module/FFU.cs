using RootTools;
using RootTools.Comm;
using RootTools.Module;

namespace Root_Vega.Module
{
    public class FFU : ModuleBase
    {

        #region ToolBox
        public Modbus m_modbus;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_modbus, this, "Modbus"); 
        }
        #endregion

        public FFU(string id, IEngineer engineer)
        {
            p_id = id;
            InitBase(id, engineer); 
        }
    }
}
