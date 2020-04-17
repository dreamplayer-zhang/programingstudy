using RootTools;
using RootTools.Module;
using System.Collections.Generic;

namespace Root_Wind
{
    /// <summary> Recipe 편집 및 저장 </summary>
    public class Wind_Recipe : NotifyProperty
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

        public ModuleRunList m_moduleRunList;

        public string m_id;
        IEngineer m_engineer;
        LogWriter m_log;
        public Wind_Recipe(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = engineer;
            m_log = engineer.ClassLogView().GetLog(LogView.eLogType.ENG, id);
            m_moduleRunList = new ModuleRunList(id, engineer);
        }
    }
}
