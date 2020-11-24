using Root_CAMELLIA.Data;
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

namespace Root_CAMELLIA
{
    public class PointMeasurement : ModuleBase
    {
        #region Data
        public DataManager m_DataManager { get; set; }
        #endregion

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

        public PointMeasurement(string id, IEngineer engineer)
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
            AddModuleRunList(new Run_Calibration(this), false, "Calibration");
            AddModuleRunList(new Run_Align(this), false, "Center Align");
            AddModuleRunList(new Run_MovePointMeasurement(this), false, "Measurement");
        }

        public class Run_Delay : ModuleRunBase
        {
            PointMeasurement m_module;
            public Run_Delay(PointMeasurement module)
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
            public PointMeasurement Module { get; private set; }
            public Run_Calibration(PointMeasurement module)
            {
                Module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_Calibration run = new Run_Calibration(Module);
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
            public PointMeasurement Module { get; private set; }
            public Run_Align(PointMeasurement module)
            {
                Module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_Align run = new Run_Align(Module);
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                //m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }
            public override string Run()
            {
                // XYAlign위해서 세곳이동해서 찍기
                //? LeftTop 이동
                //? 그랩 및 포인트 계산

                //?

                return "OK";
            }
        }
        public class Run_MovePointMeasurement : ModuleRunBase
        {
            public PointMeasurement Module { get; private set; }
            public DataManager m_DataManager { get; private set; }
            public Run_MovePointMeasurement(PointMeasurement module)
            {
                Module = module;
                m_DataManager = module.m_DataManager;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_MovePointMeasurement run = new Run_MovePointMeasurement(Module);
                m_DataManager = Module.m_DataManager;
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                //m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }
            public override string Run()
            {

                    //? 계산 thread 동작 시작

                    for (int i = 0; i < m_DataManager.recipeDM.TeachingRD.DataSelectedPoint.Count; i++)
                    {

                        // Point 이동.
                        // Align 후 보정해야함
                        double dX = m_DataManager.recipeDM.TeachingRD.DataSelectedPoint[m_DataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]].x * 10000;
                        double dY = m_DataManager.recipeDM.TeachingRD.DataSelectedPoint[m_DataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]].y * 10000;
                        if (Module.Run(Module.m_axisX.StartMove(dX, dY)))
                        {
                            return p_sInfo;
                        }

                        if (Module.Run(Module.m_axisX.WaitReady()))
                        {
                            return p_sInfo;
                        }

                        // 계측.
                        // 측정
                        for (int n = 0; n < int.MaxValue; n++) ; // 임시  --> 계측까지!


                        //이동시간..?
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
