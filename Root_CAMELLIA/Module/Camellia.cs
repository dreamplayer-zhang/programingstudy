﻿using Root_CAMELLIA.Data;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Light;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_CAMELLIA.Module
{
    class Camellia : ModuleBase
    {
        public DataManager m_DataManager;

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
            Camellia m_module;
            public Run_InitCalibration(Camellia module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_InitCalibration run = new Run_InitCalibration(m_module);
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
            Camellia m_module;

            public DataManager m_DataManager;
            public RPoint m_WaferCenterPos = new RPoint(); // Pulse
            public double m_dResX_um = 1;
            public double m_dResY_um = 1;


            public Run_Measure(Camellia module)
            {
                m_module = module;
                m_DataManager = module.m_DataManager;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_Measure run = new Run_Measure(m_module);
                run.m_DataManager = m_module.m_DataManager;
                run.m_WaferCenterPos = m_WaferCenterPos;
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                //m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }
            public override string Run()
            {
                AxisXY axisXY = m_module.m_axisXY;
                Axis axisZ = m_module.m_axisZ;
                Camera_Basler VRS = m_module.m_CamVRS;
                ImageData img = VRS.p_ImageViewer.p_ImageData;
                string strVRSImageDir = "D:\\";
                string strVRSImageFullPath = "";
                RPoint MeasurePoint;
                for (int i = 0; i < m_DataManager.recipeDM.TeachingRD.DataSelectedPoint.Count; i++)
                {
                    double dX = m_DataManager.recipeDM.TeachingRD.DataSelectedPoint[m_DataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]].x * 10000;
                    double dY = m_DataManager.recipeDM.TeachingRD.DataSelectedPoint[m_DataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]].y * 10000;
                    MeasurePoint = new RPoint(dX, dY);

                    if (m_module.Run(axisXY.StartMove(MeasurePoint)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;

                    if (VRS.Grab() != "OK")
                    {
                        //Grab error
                    }
                    strVRSImageFullPath = string.Format(strVRSImageDir + "VRSImage_{0}.bmp", i);
                    img.SaveImageSync(strVRSImageFullPath);
                }
                return "OK";
            }
        }
    }
}
