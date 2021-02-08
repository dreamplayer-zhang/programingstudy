﻿using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Root_CAMELLIA.Data;
using Met = Root_CAMELLIA.LibSR_Met;

namespace Root_CAMELLIA.Module
{
    public class Run_InitCalibration : ModuleRunBase
    {
        Module_Camellia m_module;
        DataManager m_dataManager;
        RPoint m_calWaferCenterPos_pulse = new RPoint(); // Pulse
        RPoint m_refWaferCenterPos_pulse = new RPoint(); // Pulse
        bool m_useCalWafer = true;
        bool m_useRefWafer = true;
        bool m_InitialCal = true;


        (Met.SettingData, Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode;
        public Run_InitCalibration(Module_Camellia module)
        {
            m_module = module;
            m_dataManager = module.m_DataManager;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_InitCalibration run = new Run_InitCalibration(m_module);
            run.m_calWaferCenterPos_pulse = m_calWaferCenterPos_pulse;
            run.m_refWaferCenterPos_pulse = m_refWaferCenterPos_pulse;
            run.m_useCalWafer = m_useCalWafer;
            run.m_useRefWafer = m_useRefWafer;
            run.m_InitialCal = m_InitialCal;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_calWaferCenterPos_pulse = tree.Set(m_calWaferCenterPos_pulse, m_calWaferCenterPos_pulse, "Calibration Wafer Center Axis Position", "Calibration Wafer Center Axis Position(Pulse)", bVisible);
            m_refWaferCenterPos_pulse = tree.Set(m_refWaferCenterPos_pulse, m_refWaferCenterPos_pulse, "Reference Wafer Center Axis Position", "Reference Wafer Center Axis Position(Pulse)", bVisible);
            m_useCalWafer = tree.Set(m_useCalWafer, m_useCalWafer, "Use Cal Wafer", "Use Cal Wafer", bVisible);
            m_useRefWafer = tree.Set(m_useRefWafer, m_useRefWafer, "Use Ref Wafer", "Use Ref Wafer", bVisible);
            m_InitialCal = tree.Set(m_InitialCal, m_InitialCal, "Initial Calibration", "Initial Calibration", bVisible);

        }



        public override string Run()
        {
            AxisXY axisXY = m_module.p_axisXY;

            if (m_module.LifterDown() != "OK")
            {
                return p_sInfo;
            }

            if (m_useCalWafer)
            {

                if (m_module.Run(axisXY.StartMove(m_calWaferCenterPos_pulse)))
                    return p_sInfo;

                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;

                m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
                if (m_SettingDataWithErrorCode.Item2 == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    Met.SettingData setting = m_SettingDataWithErrorCode.Item1;
                    App.m_nanoView.Calibration(setting.nBGIntTime_VIS, setting.nBGIntTime_NIR, setting.nAverage_VIS, setting.nAverage_NIR, m_InitialCal);
                }
            }

            if (m_useRefWafer)
            {
                if (m_module.Run(axisXY.StartMove(m_refWaferCenterPos_pulse)))
                    return p_sInfo;

                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;

                m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
                if (m_SettingDataWithErrorCode.Item2 == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    Met.SettingData setting = m_SettingDataWithErrorCode.Item1;
                    App.m_nanoView.Calibration(setting.nBGIntTime_VIS, setting.nBGIntTime_NIR, setting.nAverage_VIS, setting.nAverage_NIR, m_InitialCal);
                }
            }

            return "OK";
        }
    }
}