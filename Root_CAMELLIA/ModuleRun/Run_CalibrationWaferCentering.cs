﻿using Root_CAMELLIA.Data;
using Met = Root_CAMELLIA.LibSR_Met;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Light;
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
    public class Run_CalibrationWaferCentering : ModuleRunBase
    {
        Module_Camellia m_module;
        DataManager m_DataManager;
        (Met.SettingData, Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode;
        RPoint m_WaferLT_pulse = new RPoint(); // Pulse
        RPoint m_WaferRT_pulse = new RPoint(); // Pulse
        RPoint m_WaferRB_pulse = new RPoint(); // Pulse
        int m_EdgeSearchRange = 20;
        int m_EdgeSearchLevel = 30;
        double m_dResX_um = 1;
        double m_dResY_um = 1;
        double m_dFocusZ = 0;

        bool m_InitialCal = false;
        public bool m_useCal = true;
        public bool m_useCentering = true;

        List<double> m_aLightPower = new List<double>();
        LightSet m_lightSet;

        void RunTreeLight(Tree tree, bool bVisible, bool bReadOnly)
        {
            while (m_aLightPower.Count < m_lightSet.m_aLight.Count)
                m_aLightPower.Add(0);
            for (int n = 0; n < m_aLightPower.Count; n++)
            {
                m_aLightPower[n] = tree.Set(m_aLightPower[n], m_aLightPower[n], m_lightSet.m_aLight[n].m_sName, "Light Power (0 ~ 100 %%)", bVisible, bReadOnly);
            }
        }

        public void SetLight(bool bOn)
        {
            for (int n = 0; n < m_aLightPower.Count; n++)
            {
                m_lightSet.m_aLight[n].m_light.p_fSetPower = bOn ? m_aLightPower[n] : 0;
            }
        }

        public Run_CalibrationWaferCentering(Module_Camellia module)
        {
            m_module = module;
            m_DataManager = module.m_DataManager;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_CalibrationWaferCentering run = new Run_CalibrationWaferCentering(m_module);
            run.m_DataManager = m_module.m_DataManager;
            run.m_InitialCal = m_InitialCal;
            run.m_WaferLT_pulse = m_WaferLT_pulse;
            run.m_WaferRT_pulse = m_WaferRT_pulse;
            run.m_WaferRB_pulse = m_WaferRB_pulse;
            run.m_EdgeSearchRange = m_EdgeSearchRange;
            run.m_EdgeSearchLevel = m_EdgeSearchLevel;
            run.m_useCal = m_useCal;
            run.m_useCentering = m_useCentering;
            run.m_dFocusZ = m_dFocusZ;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_WaferLT_pulse = tree.Set(m_WaferLT_pulse, m_WaferLT_pulse, "Wafer Left Top", "Wafer Left Top (Pulse)", bVisible);
            m_WaferRT_pulse = tree.Set(m_WaferRT_pulse, m_WaferRT_pulse, "Wafer Right Top", "Wafer Right Top (Pulse)", bVisible);
            m_WaferRB_pulse = tree.Set(m_WaferRB_pulse, m_WaferRB_pulse, "Wafer Right Bottom", "Wafer Right Bottom (Pulse)", bVisible);
            m_EdgeSearchRange = tree.Set(m_EdgeSearchRange, m_EdgeSearchRange, "Edge Search Range", "Edge Search Range", bVisible);
            m_EdgeSearchLevel = tree.Set(m_EdgeSearchLevel, m_EdgeSearchLevel, "Edge Search Level", "Edge Search Level", bVisible);
            m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Camera X Resolution", "Camera X Resolution(um)", bVisible);
            m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Camera Y Resolution", "Camera Y Resolution(um)", bVisible);
            m_InitialCal = tree.Set(m_InitialCal, m_InitialCal, "Initial Calibration", "Initial Calibration", bVisible);
            m_useCal = tree.Set(m_useCal, m_useCal, "Use Calibration", "Use Calibration", bVisible);
            m_useCentering = tree.Set(m_useCentering, m_useCentering, "Use Centering", "Use Centering", bVisible);
            m_dFocusZ = tree.Set(m_dFocusZ, m_dFocusZ, "Focus Z Pos", "Focus Z Pos", bVisible);
            //RunTreeLight(tree.GetTree("LightPower", false), bVisible, bReadOnly);
        }

        public override string Run()
        {

            AxisXY axisXY = m_module.p_axisXY;
            Axis axisZ = m_module.p_axisZ;
            string strVRSImageDir = "D:\\";
            string strVRSImageFullPath = "";

            if (m_useCal)
            {
                m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
                if (m_SettingDataWithErrorCode.Item2 == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    Met.SettingData setting = m_SettingDataWithErrorCode.Item1;
                    if (m_useCentering)
                    {
                        if (m_DataManager.m_calibration.Run(m_InitialCal) != "OK")
                        {
                            return "Calibration Error";
                        }
                    }
                    else
                    {
                        App.m_nanoView.Calibration(setting.nBGIntTime_VIS, setting.nBGIntTime_NIR, setting.nAverage_VIS, setting.nAverage_NIR, m_InitialCal);
                    }

                }
            }

            if (!m_useCentering)
                return "OK";

            m_DataManager.m_waferCentering.FindEdgeInit();
            SetLight(true);

            Camera_Basler VRS = m_module.m_CamVRS;
            ImageData img = VRS.p_ImageViewer.p_ImageData;


            if (m_module.LifterDown() != "OK")
            {
                return p_sInfo;
            }

            m_module.VaccumOnOff(true);



            if (m_module.Run(axisZ.StartMove(m_dFocusZ)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;


            if (m_module.Run(axisXY.StartMove(m_WaferLT_pulse)))
                return p_sInfo;
            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;



            
            if (VRS.Grab() == "OK")
            {
                strVRSImageFullPath = string.Format(strVRSImageDir + "VRSImage_{0}.bmp", 0);
                img.SaveImageSync(strVRSImageFullPath);
                //Grab error
            }
            else
            {
                return "Grab Error";
            }

            CenteringParam param = new CenteringParam(img, VRS.GetRoiSize(), m_EdgeSearchRange, m_EdgeSearchLevel, WaferCentering.eDir.LT);
            ThreadPool.QueueUserWorkItem(m_DataManager.m_waferCentering.FindEdge, param);

            if (m_module.Run(axisXY.StartMove(m_WaferRT_pulse)))
                return p_sInfo;
            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;

            if (VRS.Grab() == "OK")
            {
                strVRSImageFullPath = string.Format(strVRSImageDir + "VRSImage_{0}.bmp", 1);
                img.SaveImageSync(strVRSImageFullPath);
                //Grab error
            }
            else
            {
                return "Grab Error";
            }


            param = new CenteringParam(img, VRS.GetRoiSize(), m_EdgeSearchRange, m_EdgeSearchLevel, WaferCentering.eDir.RT);
            ThreadPool.QueueUserWorkItem(m_DataManager.m_waferCentering.FindEdge, param);
            if (m_module.Run(axisXY.StartMove(m_WaferRB_pulse)))
                return p_sInfo;
            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;

            if (VRS.Grab() == "OK")
            {
                strVRSImageFullPath = string.Format(strVRSImageDir + "VRSImage_{0}.bmp", 2);
                img.SaveImageSync(strVRSImageFullPath);
                //Grab error
            }
            else
            {
                return "Grab Error";
            }


            param = new CenteringParam(img, VRS.GetRoiSize(), m_EdgeSearchRange, m_EdgeSearchLevel, WaferCentering.eDir.RB);
            ThreadPool.QueueUserWorkItem(m_DataManager.m_waferCentering.FindEdge, param);

            while (!m_DataManager.m_waferCentering.FindEdgeDone && m_useCentering)
            {
                if (m_DataManager.m_waferCentering.ErrorString != "OK")
                {
                    return "Find Edge Error";
                }
                if (EQ.IsStop())
                {
                    return "EQ Stop";
                }
            }


            m_DataManager.m_waferCentering.CalCenterPoint(VRS.GetRoiSize(), m_dResX_um, m_dResY_um, m_WaferLT_pulse, m_WaferRT_pulse, m_WaferRB_pulse);

            while (!m_DataManager.m_calibration.CalDone && m_useCal)
            {
                if (EQ.IsStop())
                {
                    return "EQ Stop";
                }
            }

            SetLight(false);

            return "OK";
        }
    }
}
