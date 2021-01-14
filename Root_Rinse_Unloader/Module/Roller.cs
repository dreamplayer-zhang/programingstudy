using RootTools;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_Rinse_Unloader.Module
{
    public class Roller : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
//            p_sInfo = m_toolBox.Get(ref m_axisRotate[0], this, "Rotate0");
//            p_sInfo = m_toolBox.Get(ref m_axisRotate[1], this, "Rotate1");
//            foreach (Line line in m_aLine) line.GetTools(m_toolBox);
            if (bInit) { }
        }
        #endregion


        RinseU m_rinse;
        public Roller(string id, IEngineer engineer, RinseU rinse)
        {
            p_id = id;
            m_rinse = rinse;
//            initILines();
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
