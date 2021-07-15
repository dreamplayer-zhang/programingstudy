using RootTools.Control;
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
        int m_nCalibrationCnt = 1;

        (Met.SettingData, Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode;
        public bool m_isPM = false;
        public Dlg_Engineer_ViewModel.PM_SR_Parameter SR_Parameter = new Dlg_Engineer_ViewModel.PM_SR_Parameter();
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
            run.m_nCalibrationCnt = m_nCalibrationCnt;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_calWaferCenterPos_pulse = tree.Set(m_calWaferCenterPos_pulse, m_calWaferCenterPos_pulse, "Calibration Wafer Center Axis Position", "Calibration Wafer Center Axis Position(Pulse)", bVisible);
            m_refWaferCenterPos_pulse = tree.Set(m_refWaferCenterPos_pulse, m_refWaferCenterPos_pulse, "Reference Wafer Center Axis Position", "Reference Wafer Center Axis Position(Pulse)", bVisible);
            m_useCalWafer = tree.Set(m_useCalWafer, m_useCalWafer, "Use Cal Wafer", "Use Cal Wafer", bVisible);
            m_useRefWafer = tree.Set(m_useRefWafer, m_useRefWafer, "Use Ref Wafer", "Use Ref Wafer", bVisible);
            m_InitialCal = tree.Set(m_InitialCal, m_InitialCal, "Initial Calibration", "Initial Calibration", bVisible);
            m_nCalibrationCnt = tree.Set(m_nCalibrationCnt, m_nCalibrationCnt, "Calibration Retry Count", "Calibration Retry", bVisible);

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

                if (!m_isPM)
                {
                    m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
                    if (m_SettingDataWithErrorCode.Item2 != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                    {
                        return "Load Parameter Error";
                    }
                    LibSR_Met.DataManager.GetInstance().m_SettngData = m_SettingDataWithErrorCode.Item1;
                    LibSR_Met.DataManager.GetInstance().m_SettngData = m_SettingDataWithErrorCode.Item1;
                }
                m_dataManager.m_calibration.Run(m_InitialCal, m_isPM, false, m_nCalibrationCnt);
                //App.m_nanoView.Calibration(m_InitialCal);
                //System.Threading.Thread.Sleep(8000);
            }

            if (m_useRefWafer)
            {
                if (m_module.Run(axisXY.StartMove(m_refWaferCenterPos_pulse)))
                    return p_sInfo;

                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;

                if (!m_isPM)
                {
                    m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
                    if (m_SettingDataWithErrorCode.Item2 != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                    {
                        return "Load Parameter Error";
                    }
                    LibSR_Met.DataManager.GetInstance().m_SettngData = m_SettingDataWithErrorCode.Item1;
                }

                m_dataManager.m_calibration.Run(m_InitialCal, m_isPM, false, m_nCalibrationCnt);
            }

            return "OK";
        }
    }
}
