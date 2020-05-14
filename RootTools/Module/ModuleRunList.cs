using RootTools.Trees;
using System.Collections.Generic;

namespace RootTools.Module
{
    /// <summary> ModuleRunList : ModuleRundml List 관리 (파일저장, 실행), Recipe 또는 Sequence에서 사용 할 수 있다 </summary>
    public class ModuleRunList
    {
        #region ModuleRun List
        public List<ModuleRunBase> m_aModuleRun = new List<ModuleRunBase>();

        public void Add(string sModule, string sModuleRun)
        {
            ModuleBase module = GetModule(sModule);
            if (module == null) return;
            Add(module, sModuleRun);
        }

        public void Add(ModuleBase module, string sModuleRun)
        {
            if (module == null) return;
            ModuleRunBase moduleRun = module.CloneModuleRun(sModuleRun);
            if (moduleRun == null) return;
            Add(module, moduleRun);
        }

        public void Add(ModuleBase module, ModuleRunBase moduleRun)
        {
            if (module == null) return;
            if (moduleRun == null) return;
            moduleRun.m_moduleBase = module;
            m_aModuleRun.Add(moduleRun);
        }

        public void Clear()
        {
            m_aModuleRun.Clear();
            RunTree(Tree.eMode.Init);
        }

        public List<string> GetModuleRunNames(string sModule)
        {
            ModuleBase module = GetModule(sModule);
            if (module == null) return null;
            return module.m_asModuleRun;
        }

        public List<string> GetRecipeRunNames(string sModule)
        {
            ModuleBase module = GetModule(sModule);
            if (module == null) return null;
            return module.m_asRecipe;
        }

        ModuleBase GetModule(string sModule)
        {
            if (m_moduleList == null) m_moduleList = m_engineer.ClassModuleList();
            if (m_moduleList == null) return null;
            return m_moduleList.GetModule(sModule);
        }
        #endregion

        #region File
        public void SaveJob(string sFile)
        {
            Job job = null;
            try
            {
                job = new Job(sFile, true, m_log);
                job.Set("ModuleRuns", "Count", m_aModuleRun.Count);
                for (int n = 0; n < m_aModuleRun.Count; n++)
                {
                    string sKey = n.ToString("00");
                    ModuleRunBase moduleRun = m_aModuleRun[n];
                    job.Set(sKey, "Module", moduleRun.m_moduleBase.p_id);
                    job.Set(sKey, "ModuleRun", moduleRun.m_sModuleRun);
                }
                m_treeRoot.m_job = job;
                RunTree(Tree.eMode.JobSave);
            }
            finally
            {
                if (job != null) job.Close();
            }
        }

        public void OpenJob(string sFile, bool bClear = true)
        {
            if (bClear) m_aModuleRun.Clear();
            Job job = null;
            if (sFile == "") return;
            try
            {
                job = new Job(sFile, false, m_log);
                int lRun = job.Set("ModuleRuns", "Count", m_aModuleRun.Count);
                for (int n = 0; n < lRun; n++)
                {
                    string sKey = n.ToString("00");
                    string sModule = job.Set(sKey, "Module", "");
                    string sModuleRun = job.Set(sKey, "ModuleRun", "");
                    Add(sModule, sModuleRun);
                }
                m_treeRoot.m_job = job;
                RunTree(Tree.eMode.JobOpen);
                RunTree(Tree.eMode.Init);
            }
            finally
            {
                if (job != null) job.Close();
            }
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
            RunTree(Tree.eMode.Init);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTree(m_treeRoot);
        }

        public void RunTree(Tree tree)
        {
            for (int n = 0; n < m_aModuleRun.Count; n++)
            {
                ModuleRunBase moduleRun = m_aModuleRun[n];
                moduleRun.RunTree(tree.GetTree(n, moduleRun.p_id, false), true);
            }
        }
        #endregion

        string m_id;
        Log m_log;
        IEngineer m_engineer;
        ModuleList m_moduleList;
        public TreeRoot m_treeRoot;
        public ModuleRunList(string id, IEngineer engineer, string sLogGroup = "")
        {
            m_id = id;
            m_engineer = engineer;
            m_moduleList = engineer.ClassModuleList();
            m_log = LogView.GetLog(m_id, sLogGroup);
            m_treeRoot = new TreeRoot(m_id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
        }
    }
}
