using RootTools;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Module
{
    public class Stage : ModuleBase
    {
        #region ToolBox
        Axis m_axisRotate;
        AxisXY m_axisXZ;
        DIO_O m_doVac;
        DIO_O m_doBlow;
        MemoryPool m_memoryPool;
        readonly MemoryGroup m_memoryGroup;
        LightSet m_lightSet;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Rotate");
            p_sInfo = m_toolBox.Get(ref m_axisXZ, this, "Camera XZ");
            p_sInfo = m_toolBox.Get(ref m_doVac, this, "Stage Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doBlow, this, "Stage Blow");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
        }
        #endregion

        public Stage(string id, IEngineer engineer)
        {   
            base.InitBase(id, engineer);
            //            InitMemory();
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }


        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
        }

        public class Run_Delay : ModuleRunBase
        {
            Stage m_module;
            public Run_Delay(Stage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return "OK";
            }
        }
    }
}
