using RootTools.Module;
using RootTools.Trees;

namespace Root_Wind.Module
{
    public class VisionRun_Run : ModuleRunBase
    {
        Vision m_module;
        Vision.Run_Grab m_runGrab;
        Vision.Run_Inspect m_runInspect;
        public VisionRun_Run(Vision module)
        {
            m_module = module;
            InitModuleRun(module);
            m_runGrab = new Vision.Run_Grab(module);
            m_runInspect = new Vision.Run_Inspect(module);
        }

        public override ModuleRunBase Clone()
        {
            VisionRun_Run run = new VisionRun_Run(m_module);
            run.m_runGrab = (Vision.Run_Grab)m_runGrab.Clone();
            run.m_runInspect = (Vision.Run_Inspect)m_runInspect.Clone();
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            tree.p_bVisible = bVisible; 
            m_runGrab.RunTree(tree.GetTree("Grab"), bVisible, bRecipe);
            m_runInspect.RunTree(tree.GetTree("Inspect"), bVisible, bRecipe);
        }
        public override string Run()
        {
            //forget
            return "OK";
        }
    }
}
