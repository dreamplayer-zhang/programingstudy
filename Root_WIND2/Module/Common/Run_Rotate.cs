using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.Module
{
    public class Run_Rotate : ModuleRunBase
    {
        Vision m_module;
        public Run_Rotate(Vision module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        double m_fDeg = 0;
        public override ModuleRunBase Clone()
        {
            Run_Rotate run = new Run_Rotate(m_module);
            run.m_fDeg = m_fDeg;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_fDeg = tree.Set(m_fDeg, m_fDeg, "Degree", "Rotation Degree (0 ~ 360)", bVisible);
        }

        public override string Run()
        {
            int pulseRound = m_module.PulseRound;
            Axis axis = m_module.AxisRotate;
            int pulse = (int)Math.Round(m_module.PulseRound * m_fDeg / 360);
            while (pulse < axis.p_posCommand)
                pulse += pulseRound;
            {
                axis.p_posCommand -= pulseRound;
                axis.p_posActual -= pulseRound;
            }
            if (m_module.Run(axis.StartMove(pulse)))
                return p_sInfo;
            return axis.WaitReady();
        }
    }
}
