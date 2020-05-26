using RootTools;
using RootTools.DMC;
using RootTools.Module;

namespace Root.Module
{
    public class LoadPort_DMC : ModuleBase
    {
        #region ToolBox
        DMCCore m_dmc;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_dmc, this, "DMC");
        }
        #endregion

        public LoadPort_DMC(string id, IEngineer engineer)
        {
            p_id = id;
            m_log = LogView.GetLog(id, id);
            InitBase(id, engineer);
        }

    }
}
