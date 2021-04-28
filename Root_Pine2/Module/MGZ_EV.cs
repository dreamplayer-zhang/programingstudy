using RootTools;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_Pine2.Module
{
    public class MGZ_EV : ModuleBase
    {
        #region ToolBox

        public override void GetTools(bool bInit)
        {
            //p_sInfo = m_toolBox.GetDIO(ref m_dioEV, this, "Elevator", "Down", "Up");
            if (bInit) InitTools();
        }

        void InitTools()
        {
        }
        #endregion

        public MGZ_EV(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
