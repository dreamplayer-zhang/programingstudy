using RootTools.Module;
using RootTools.Trees;

namespace Root_VEGA_P_Vision.Module
{
    class Run_Rotate : ModuleRunBase
    {
        Vision m_module;
        int nDegree;

        public Run_Rotate(Vision module)
        {
            m_module = module;
            nDegree = 0;
            InitModuleRun(module);
        }
        public Run_Rotate(Vision module,int nDegree)
        {
            m_module = module;
            this.nDegree = nDegree;
            InitModuleRun(module);
        }
        public override ModuleRunBase Clone()
        {
            Run_Rotate run = new Run_Rotate(m_module);

            return run;
        }
        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            nDegree = tree.Set(nDegree, nDegree, "Degree", "Degree for Rotate", bVisible);
        }
        public override string Run()
        {
            if (nDegree == 0) return "Degree Is 0";

            if (m_module.Run(m_module.m_stage.Rotate(nDegree)))
                return p_sInfo;

            return "OK";
        }
    }
}
