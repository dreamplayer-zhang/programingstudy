using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P.Module
{
    public class LoadPort : ModuleBase//, IRTRChild
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
        }
        #endregion



        public LoadPort(string id, IEngineer engineer)
        {
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
