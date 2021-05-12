using Root_VEGA_P.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_VEGA_P.Engineer
{
    public class VEGA_P_Recipe
    {
        #region ModuleBase
        /// <summary> Recipe 편집에 필요한 Module 이름 List </summary>
        public List<string> m_asModule = new List<string>();
        /// <summary> Recipe 편집에 필요한 Module List </summary>
        List<ModuleBase> m_aModule = new List<ModuleBase>();
        public void AddModule(params ModuleBase[] modules)
        {
            foreach (ModuleBase module in modules)
            {
                m_asModule.Add(module.p_id);
                m_aModule.Add(module);
            }
        }
        #endregion

        public void AddRunGetPut(string sChildGet, string sChildPut)
        {
            RTR.Run_GetPut run = m_rtr.GetModuleRunGetPut(sChildGet, sChildPut);
            if (run == null) return; 
            m_moduleRunList.Add(m_rtr, run);
            m_moduleRunList.RunTree(Tree.eMode.Init); 
        }

        public ModuleRunList m_moduleRunList;
        public string m_id;
        IEngineer m_engineer;
        RTR m_rtr; 
        Log m_log;
        public VEGA_P_Recipe(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = engineer;
            m_rtr = ((VEGA_P_Handler)engineer.ClassHandler()).m_rtr; 
            m_log = LogView.GetLog(id);
            m_moduleRunList = new ModuleRunList(id, engineer);
        }
    }
}
