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
        Axis m_axisX;
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
                //m_module.m_axisX.StartMove(12902, 123021);
                return "OK";
            }
        }
        public class Run_Calibration : ModuleRunBase
        {
            public Stage Module { get; private set; }
            public Run_Calibration(Stage module)
            {
                Module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_MovePoint run = new Run_MovePoint(Module);
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                //m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }
            public override string Run()
            {

                return "OK";
            }
        }
        public class Run_Align : ModuleRunBase
        {
            public Stage Module { get; private set; }
            public Run_Align(Stage module)
            {
                Module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_MovePoint run = new Run_MovePoint(Module);
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                //m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }
            public override string Run()
            {

                return "OK";
            }
        }
        public class Run_MovePoint : ModuleRunBase
        {
            public Stage Module { get; private set; }
            public Run_MovePoint(Stage module)
            {
                Module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_MovePoint run = new Run_MovePoint(Module);
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                //m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }
            public override string Run()
            {

                while (true)
                {
                    // Point 이동.


                    // 계측.


                    // Pause 누르면 잠시 멈춤
                    /*
                     * if(Pause){
                     *      머 스탑..
                     * }
                    */
                }


                return "OK";
            }
        }
    }
}
