using RootTools;
using RootTools.Module;
using System.Collections.Generic;
using System.Dynamic;

namespace Root_Vega
{
    /// <summary> Recipe 편집 및 저장 </summary>
    public class Vega_Recipe : ObservableObject
    {
        #region ModuleBase
        /// <summary> Recipe 편집에 필요한 Module 이름 List </summary>
        public List<string> m_asModule = new List<string>();
        public List<string> p_asModule
        {
            get { return m_asModule; }
            set { SetProperty(ref m_asModule, value); }
        }
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
        Log m_log;
        public Vega_Recipe(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);
            m_moduleRunList = new ModuleRunList(id, engineer);
        }
    }
}
