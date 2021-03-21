using RootTools.Camera.Matrox;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision.Module
{
    class Run_ZStack:ModuleRunBase
    {
        Vision m_module;
        Vision.MainOptic mainOpt;
        GrabMode ZStackGrabMode;
        Camera_Matrox camZStack;
        string sZStackGrabMode;

        public Run_ZStack(Vision module)
        {
            m_module = module;
            mainOpt = m_module.m_mainOptic;
            camZStack = mainOpt.camZStack;
            sZStackGrabMode = "";
            InitModuleRun(module);
        }

        string p_sZStackGrabMode
        {
            get { return sZStackGrabMode; }
            set
            {
                sZStackGrabMode = value;
                ZStackGrabMode = m_module.GetGrabMode(value);
            }
        }

        public override ModuleRunBase Clone()
        {
            Run_ZStack run = new Run_ZStack(m_module);
            run.p_sZStackGrabMode = p_sZStackGrabMode;

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sZStackGrabMode = tree.Set(p_sZStackGrabMode, p_sZStackGrabMode, m_module.p_asGrabMode, "Grab Mode : ZStack Grab", "Select GrabMode", bVisible);
        }

        public override string Run()
        {
            if (p_sZStackGrabMode == null) return "Grab Mode : ZStack Grab == Null";
            try
            {

            }
            finally
            {
                ZStackGrabMode.SetLight(false);
            }
            return "OK";
        }
    }
}
