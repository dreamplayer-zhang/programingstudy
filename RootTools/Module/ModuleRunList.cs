using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RootTools.Module
{
    /// <summary> ModuleRunList : ModuleRundml List 관리 (파일저장, 실행), Recipe 또는 Sequence에서 사용 할 수 있다 </summary>
    public class ModuleRunList : ObservableObject
    {
        #region ModuleRun List
        ObservableCollection<ModuleRunBase> m_aModuleRun = new ObservableCollection<ModuleRunBase>();
        public ObservableCollection<ModuleRunBase> p_aModuleRun
        {
            get { return m_aModuleRun; }
            set
            {
                SetProperty(ref m_aModuleRun, value);
            }
        }

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
            p_aModuleRun.Add(moduleRun);
        }

        public void Clear()
        {
            p_aModuleRun.Clear();
            RunTree(Tree.eMode.Init);
        }

        public void Undo()
        {
            p_aModuleRun.RemoveAt(p_aModuleRun.Count - 1);
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
                job.Set("ModuleRuns", "Count", p_aModuleRun.Count);
                for (int n = 0; n < p_aModuleRun.Count; n++)
                {
                    string sKey = n.ToString("00");
                    ModuleRunBase moduleRun = p_aModuleRun[n];
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
            if (bClear) p_aModuleRun.Clear();
            Job job = null;
            if (sFile == "") return;
            try
            {
                job = new Job(sFile, false, m_log);
                int lRun = job.Set("ModuleRuns", "Count", p_aModuleRun.Count);
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
            for (int n = 0; n < p_aModuleRun.Count; n++)
            {
                ModuleRunBase moduleRun = p_aModuleRun[n];
                moduleRun.RunTree(tree.GetTree(n, moduleRun.p_id, false), true);
            }
        }
        #endregion

        string m_id;
        Log m_log;
        IEngineer m_engineer;
        ModuleList m_moduleList;
        public TreeRoot m_treeRoot;
        public TreeRoot p_treeRoot
        {
            get { return m_treeRoot; }
            set { SetProperty(ref m_treeRoot, value); }
        }
        
        public ModuleRunList(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = engineer;
            m_moduleList = engineer.ClassModuleList();
            m_log = LogView.GetLog(m_id, "ModuleRunList");
            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
        }

        public ModuleRunList Copy()
        {
            ModuleRunList temp = new ModuleRunList(m_id, m_engineer);

            temp.p_aModuleRun = new ObservableCollection<ModuleRunBase>(p_aModuleRun);
            return temp;
        }

        public ObservableCollection<ModuleRunBase> CopyModuleRun()
        {
            return new ObservableCollection<ModuleRunBase>(m_aModuleRun);
        }
    }
}
