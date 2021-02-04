using Root_CAMELLIA.Module;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Module
{
    public class Run_Delay : ModuleRunBase
    {
        Module_Camellia m_module;
        double m_secDelay = 2;
        public Run_Delay(Module_Camellia module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_Delay run = new Run_Delay(m_module);
            run.m_secDelay = m_secDelay;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
        }

        public override string Run()
        {
            Thread.Sleep((int)(1000 * m_secDelay));
            return "OK";
        }
    }
}
