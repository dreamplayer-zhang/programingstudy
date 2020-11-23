using RootTools;
using RootTools.Comm;
using RootTools.Module;

namespace Root_Vega.Module
{
    public class FFU : ModuleBase
    {

        #region ToolBox
        public RS232 m_rs232;
        public override void GetTools(bool bInit)
        {
        }

        private void M_rs232_OnRecieve(string sRead)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        public FFU(string id, IEngineer engineer)
        {
            p_id = id;
            InitBase(id, engineer); 
        }
    }
}
