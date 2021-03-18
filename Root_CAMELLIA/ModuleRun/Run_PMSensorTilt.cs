﻿using Root_CAMELLIA.Data;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static Root_CAMELLIA.Module.Module_Camellia;
using Met = Root_CAMELLIA.LibSR_Met;

namespace Root_CAMELLIA.Module
{
    class Run_PMSensorTilt : ModuleRunBase
    {
        Module_Camellia m_module;
        MainWindow_ViewModel m_mwvm;
        DataManager m_DataManager;
        RPoint m_calWaferCenterPos_pulse = new RPoint(); // Init calibration Sample 위치
        bool m_useCalWafer = true;
        bool m_InitialCal = true;

        (Met.SettingData, Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode;
        public Run_PMSensorTilt(Module_Camellia module)
        {
            m_module = module;
            m_mwvm = module.mwvm;
            m_DataManager = module.m_DataManager;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_PMSensorTilt run = new Run_PMSensorTilt(m_module);
            run.m_calWaferCenterPos_pulse = m_calWaferCenterPos_pulse;
            run.m_useCalWafer = m_useCalWafer;
            run.m_InitialCal = m_InitialCal;
            return run;
        }
        //수정? 함수 세팅 입력 인자값이 제대로 되었는지 확인
        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_calWaferCenterPos_pulse = tree.Set(m_calWaferCenterPos_pulse, m_calWaferCenterPos_pulse, "Calibration Wafer Center Axis Position", "Calibration Wafer Center Axis Position(Pulse)", bVisible);
            m_useCalWafer = tree.Set(m_useCalWafer, m_useCalWafer, "Use Cal Wafer", "Use Cal Wafer", bVisible);
            m_InitialCal = tree.Set(m_InitialCal, m_InitialCal, "Initial Calibration", "Initial Calibration", bVisible);

        }

        public override string Run()
        {
            Met.PMDatas PMDatas = new Met.PMDatas();
            Axis axisLifter = m_module.p_axisLifter;
            // PM 동작 하기전에 lifter 내려가 있는지 체크
            if (m_module.LifterDown() != "OK")
            {
                return p_sInfo;
            }
            // 시작
            m_log.Info("[CheckSensorTilt] Start");
            // 현재 축값 받아오기
            AxisXY axisXY = m_module.p_axisXY;
            m_log.Info("[CheckSensorTilt] Stage Move Ready");
            
            
            //Cal Sample 로 이동 & Init Calibration
            if (m_useCalWafer)
            {

                if (m_module.Run(axisXY.StartMove(m_calWaferCenterPos_pulse)))
                    return p_sInfo;

                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
            }

            m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
            Met.SettingData setting = null;
            if (m_SettingDataWithErrorCode.Item2 == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
            {
                setting = m_SettingDataWithErrorCode.Item1;

                //Init Cal
                if (App.m_nanoView.Calibration(setting.nBGIntTime_VIS, setting.nBGIntTime_NIR, setting.nAverage_VIS, setting.nAverage_NIR, m_InitialCal, m_DataManager.recipeDM.MeasurementRD.VISIntegrationTime, m_DataManager.recipeDM.MeasurementRD.NIRIntegrationTime) == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    m_InitialCal = false;
                    m_log.Info("[CheckSensorTilt] Init Calbration OK");

                }
                else
                {
                    m_log.Info("[CheckSensorTilt] Init Calbration fail");
                    return "[CheckSensorTilt] Init Calbration fail";
                }
                //Sample Cal
                if (App.m_nanoView.Calibration(setting.nBGIntTime_VIS, setting.nBGIntTime_NIR, setting.nAverage_VIS, setting.nAverage_NIR, m_InitialCal, m_DataManager.recipeDM.MeasurementRD.VISIntegrationTime, m_DataManager.recipeDM.MeasurementRD.NIRIntegrationTime) == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    m_log.Info("[CheckSensorTilt] Sample Calbration OK");
                }
                else
                {
                    m_log.Info("[CheckSensorTilt] Init Calbration fail");
                    return "[CheckSensorTilt] Init Calbration fail";
                }
            }
            else
            {
                m_log.Info("[CheckSensorTilt] Nano-View SettingDataLoad Error");
                return "[CheckSensorTilt] Nano-View SettingDataLoad Error";
            }
            //
            object obj;
            for (int n = 0; n < PMDatas.SensorTiltRepeatNum; n++)
            {
                if (App.m_nanoView.SampleMeasure(0, 0, 0, m_DataManager.recipeDM.MeasurementRD.VISIntegrationTime, setting.nAverage_VIS, m_DataManager.recipeDM.MeasurementRD.NIRIntegrationTime, setting.nAverage_NIR,
                       m_mwvm.SettingViewModel.p_ExceptNIR, m_DataManager.recipeDM.MeasurementRD.UseTransmittance, m_DataManager.recipeDM.MeasurementRD.UseThickness,
                       m_DataManager.recipeDM.MeasurementRD.LowerWaveLength, m_DataManager.recipeDM.MeasurementRD.UpperWaveLength) == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    string sDate = DateTime.Now.Year.ToString("0000") + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00");
                    string sFileName = Met.ConstValue.PATH_PM_RESULT_FOLDER + sDate + @"\CheckSensorTilt" + n + "_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + ".csv";
                    if (Met.DataManager.GetInstance().SaveCheckSensorData(sFileName, 0))
                    {
                        
                        m_log.Info("[CheckSensorTilt] Acquire Refelctance" + "[" + n + "]");
                    }
                    else
                    {
                        m_log.Info("[CheckSensorTilt] Acquire Refelctance Error" + "[" + n + "]");
                        return "[CheckSensorTilt] Acquire Refelctance Error" + "[" + n + "]";
                    }
                }
                else
                {
                    m_log.Info("[CheckSensorTilt] Measuring Error" + "[" + n + "]");
                    return "[CheckSensorTilt] Measuring Error" + "[" + n + "]";
                }
                Met.CheckResult rst;
                //rst = Met.PMDatas.CheckSensorTilt();

            }



            //리턴값 반환할 것
            return "OK";
        }
        private void SaveCheckSensorData()
        {

        }
    }
}

