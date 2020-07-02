using RootTools.Module;
using RootTools.Trees;

namespace Root_Vega.Module
{
    public class Run_LADS : ModuleRunBase
    {
        SideVision m_module;
        int m_nTestParam;

        public Run_LADS(SideVision module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_LADS run = new Run_LADS(m_module);
            run.m_nTestParam = m_nTestParam;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            tree.p_bVisible = bVisible;
            m_nTestParam = tree.Set(m_nTestParam, m_nTestParam, "Test Parameter", "Test Script", bVisible);
        }

        public override string Run()
        {
            return "OK";
        }
    }
}