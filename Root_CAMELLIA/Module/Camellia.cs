using Root_CAMELLIA.Data;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Light;
using RootTools.Module;
using RootTools.Trees;
using System;

namespace Root_CAMELLIA.Module
{
    class Camellia : ModuleBase
    {
        #region Data
        public DataManager m_DataManager
        {
            get; set;
        }
        #endregion

        #region ToolBox
        AxisXY m_axisXY;
        Axis m_axisZ;
        Camera_Basler m_CamVRS;

        #region Light
        LightSet m_lightSet;
        public int GetLightByName(string str)
        {
            for (int i = 0; i < m_lightSet.m_aLight.Count; i++)
            {
                if (m_lightSet.m_aLight[i].m_sName.IndexOf(str) >= 0)
                {
                    return Convert.ToInt32(m_lightSet.m_aLight[i].p_fPower);
                }
            }
            return 0;
        }
        public void SetLightByName(string str, int nValue)
        {
            for (int i = 0; i < m_lightSet.m_aLight.Count; i++)
            {
                if (m_lightSet.m_aLight[i].m_sName.IndexOf(str) >= 0)
                {
                    m_lightSet.m_aLight[i].m_light.p_fSetPower = nValue;
                }
            }
        }
        #endregion


        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "StageXY");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "StageZ");
            p_sInfo = m_toolBox.Get(ref m_CamVRS, this, "VRS");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
        }
        #endregion

        public Camellia(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
            m_DataManager = DataManager.Instance;
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            AddModuleRunList(new Run_Measure(this), false, "Measurement");
        }


        public class Run_Delay : ModuleRunBase
        {
            Camellia m_module;
            public Run_Delay(Camellia module)
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
        public class Run_WaferCentering : ModuleRunBase
        {
            Camellia m_module;
            public Run_WaferCentering(Camellia module)
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
        public class Run_Calibration : ModuleRunBase
        {
            Camellia m_module;
            public Run_Calibration(Camellia module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Calibration run = new Run_Calibration(m_module);
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
        public class Run_InitCalibration : ModuleRunBase
        {
            public Camellia Module
            {
                get; private set;
            }
            public Run_InitCalibration(Camellia module)
            {
                Module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_InitCalibration run = new Run_InitCalibration(Module);
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
        public class Run_MonitorCalibration : ModuleRunBase
        {
            Camellia m_module;
            public Run_MonitorCalibration(Camellia module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_MonitorCalibration run = new Run_MonitorCalibration(m_module);
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
        public class Run_Measure : ModuleRunBase
        {
            public Camellia Module
            {
                get; private set;
            }
            public DataManager m_DataManager
            {
                get; private set;
            }
            public Run_Measure(Camellia module)
            {
                Module = module;
                m_DataManager = module.m_DataManager;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_Measure run = new Run_Measure(Module);
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
                    //if (Module.Run(Module.m_axisX.StartMove(dX, dY)))
                    //{
                    //    return p_sInfo;
                    //}

                    //if (Module.Run(Module.m_axisX.WaitReady()))
                    //{
                    //    return p_sInfo;
                    //}

                    // 계측.
                    // 측정
                    for (int n = 0; n < int.MaxValue; n++)
                        ; // 임시  --> 계측까지!


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
