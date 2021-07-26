using RootTools;
using RootTools.Control;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_JEDI_Sorter.Module
{
    public class Bad : ModuleBase
    {
        #region ToolBox
        Axis m_axis;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "Stage");
            m_unloadEV.GetTools(m_toolBox, this, bInit);
            m_stage.GetTools(m_toolBox, this, bInit);
            //if (bInit) InitPosition();
        }
        #endregion

        #region Stage Axis

        #endregion

        eResult m_eResult = eResult.Good; 
        public UnloadEV m_unloadEV;
        public Stage m_stage;
        public Bad(eResult eResult, IEngineer engineer)
        {
            m_eResult = eResult; 
            string id = eResult.ToString(); 
            m_unloadEV = new UnloadEV(id + ".UnloadEV");
            m_stage = new Stage(id + ".Stage");
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
