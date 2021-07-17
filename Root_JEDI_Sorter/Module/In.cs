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
    public class In : ModuleBase
    {
        #region ToolBox
        Axis m_axis; 
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "Stage"); 
            m_loadEV.GetTools(m_toolBox, this, bInit);
            if (bInit) InitPosition(); 
        }

        #endregion

        #region Stage Axis
        void InitPosition()
        {

        }
        #endregion 

        public LoadEV m_loadEV;
        public Stage m_stage; 
        public In(string id, IEngineer engineer)
        {
            m_loadEV = new LoadEV(id + ".LoadEV");
            m_stage = new Stage(id + ".Stage");
            base.InitBase(id, engineer);
        }
    }
}
