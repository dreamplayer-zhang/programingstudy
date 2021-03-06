using RootTools;
using RootTools.Module;
using System.Collections.Generic;

namespace Root_WIND2
{
    public class WIND2_Recipe : NotifyProperty
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

        ModuleRunList m_moduleRunList;
        public ModuleRunList p_moduleRunList
        {
            get { return m_moduleRunList; }
            set
            {
                m_moduleRunList = value;
                OnPropertyChanged("m_moduleRunList");
            }
        }

        public string m_id;
        IEngineer m_engineer;
        Log m_log;
        public WIND2_Recipe(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);
            p_moduleRunList = new ModuleRunList(id, engineer);
        }
    }
}
