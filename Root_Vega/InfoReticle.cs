using RootTools;
using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_Vega
{
    public class InfoReticle : GemSlotBase
    {
        #region Recipe
        /// <summary> PJ.Recipe -> ModuleRunList </summary>
        public ModuleRunList m_moduleRunList;
        protected override void RecipeOpen()
        {
            m_moduleRunList.Clear();
            foreach (GemPJ pj in m_aPJ)
            {
                p_sRecipe = pj.m_sRecipeID;
                m_moduleRunList.OpenJob(pj.m_sRecipeID, false);
            }
            m_qProcess.Clear();
            //m_infoPod.StartProcess(); 
        }

        public void RecipeOpen(string sRecipe)
        {
            p_sRecipe = sRecipe; 
            m_moduleRunList.OpenJob(sRecipe, true);
            m_qProcess.Clear(); 
        }

        public string m_sManualRecipe = "";
        void RunTreeRecipe(Tree tree)
        {
            string sRecipe = m_sManualRecipe; 
            m_sManualRecipe = tree.SetFile(m_sManualRecipe, m_sManualRecipe, EQ.m_sModel, "Recipe", "Recipe Name", m_gem.p_bOffline);
            if (sRecipe != m_sManualRecipe) RecipeOpen(m_sManualRecipe);
            if (m_moduleRunList != null) m_moduleRunList.RunTree(tree);
        }
        #endregion

        #region Process 
        /// <summary> Recipe ModuleRunList에 WTR Get, Put 추가 -> Process </summary>
        public Queue<ModuleRunBase> m_qProcess = new Queue<ModuleRunBase>(); 
        void RunTreeProcess(Tree tree)
        {
            ModuleRunBase[] aProcess = m_qProcess.ToArray(); 
            for (int n = 0; n < aProcess.Length; n++)
            {
                ModuleRunBase moduleRun = aProcess[n];
                moduleRun.RunTree(tree.GetTree(n, moduleRun.p_id, false), true);
            }
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeRecipe(tree.GetTree("Recipe", false));
            RunTreeProcess(tree.GetTree("Process", false));
        }
        #endregion

        public string m_sLoadport;
        InfoPod m_infoPod; 
        public InfoReticle(string id, InfoPod infoPod, IEngineer engineer)
        {
            m_infoPod = infoPod; 
            string[] asID = id.Split('.');
            m_sLoadport = asID[0]; 
            InitBase(id, engineer);
            m_moduleRunList = new ModuleRunList(id, engineer);
            m_moduleRunList.Clear();
        }
    }
}
