using Root_CAMELLIA.Data;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Light;
using RootTools.Module;
using RootTools.Trees;
using System;
using Met = LibSR_Met;

namespace Root_CAMELLIA.Module
{
    public class Module_Camellia : ModuleBase
    {
        public DataManager m_DataManager;
        public Met.Nanoview Nanoview;
        public MainWindow_ViewModel mwvm;

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

        public Module_Camellia(string id, IEngineer engineer)
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
            AddModuleRunList(new Run_Calibration(this), false, "Calibration");
            AddModuleRunList(new Run_WaferCentering(this), false, "Centering");
            AddModuleRunList(new Run_Measure(this), false, "Measurement");

        }

        protected override void RunThread()
        {
            switch (p_eState)
            {
                case eState.Init:
                    p_bEnableHome = true;
                    p_sRun = "Initialize";
                    break;
                case eState.Home:
                    p_bEnableHome = false;
                    p_sRun = "Stop";
                    string sStateHome = StateHome();
                    if (sStateHome == "OK") p_eState = eState.Ready;
                    else StopHome();
                    break;
                case eState.Ready:
                    p_bEnableHome = true;
                    p_sRun = p_sModuleRun;
                    string sStateReady = StateReady();
                    if (sStateReady != "OK")
                    {
                        p_eState = eState.Error;
                        m_qModuleRun.Clear();
                    }
                    if (m_qModuleRun.Count > 0) p_eState = eState.Run;
                    break;
                case eState.Run:
                    p_bEnableHome = false;
                    p_sRun = "Stop";
                    string sStateRun = StateRun();
                    if (sStateRun != "OK")
                    {
                        p_eState = eState.Error;
                        m_qModuleRun.Clear();
                    }
                    if (m_qModuleRun.Count == 0) p_eState = eState.Ready;
                    break;
                case eState.Error:
                    p_bEnableHome = false;
                    p_sRun = "Reset";
                    break;
                default:
                    p_bEnableHome = true;
                    break;

            }
        }


        public class Run_Delay : ModuleRunBase
        {
            Module_Camellia m_module;
            public Run_Delay(Module_Camellia module)
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
            Module_Camellia m_module;
            public RPoint m_WaferLT_pulse = new RPoint(); // Pulse
            public RPoint m_WaferRT_pulse = new RPoint(); // Pulse
            public RPoint m_WaferRB_pulse = new RPoint(); // Pulse
            public Run_WaferCentering(Module_Camellia module)
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
                m_WaferLT_pulse = tree.Set(m_WaferLT_pulse, m_WaferLT_pulse,"Wafer Left Top", "Wafer Left Top (Pulse)");
                m_WaferRT_pulse = tree.Set(m_WaferRT_pulse, m_WaferRT_pulse, "Wafer Right Top", "Wafer Right Top (Pulse)");
                m_WaferRB_pulse = tree.Set(m_WaferRB_pulse, m_WaferRB_pulse, "Wafer Right Bottom", "Wafer Right Bottom (Pulse)");
            }

            public override string Run()
            {
                AxisXY axisXY = m_module.m_axisXY;

                Camera_Basler VRS = m_module.m_CamVRS;
                ImageData img = VRS.p_ImageViewer.p_ImageData;


                if (m_module.Run(axisXY.StartMove(m_WaferLT_pulse)))
                    return p_sInfo;
                if(m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
                //? 엣지 따는 함수 추가


                if (m_module.Run(axisXY.StartMove(m_WaferRT_pulse)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
                //? 엣지 따는 함수 추가

                if (m_module.Run(axisXY.StartMove(m_WaferRB_pulse)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
                //? 엣지 따는 함수 추가

                return "OK";
            }
        }
        public class Run_Calibration : ModuleRunBase
        {
            Module_Camellia m_module;
            Met.Nanoview m_NanoView;
            int m_BGIntTime_VIS = 50;
            int m_BGIntTime_NIR = 150;
            int m_Average_VIS = 5;
            int m_Average_NIR = 3;
            bool m_InitialCal = false;
            public RPoint m_CalWaferCenterPos_pulse = new RPoint(); // Pulse
            public RPoint m_RefWaferCenterPos_pulse = new RPoint(); // Pulse
            public Run_Calibration(Module_Camellia module)
            {
                m_module = module;
                m_NanoView = module.Nanoview;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Calibration run = new Run_Calibration(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_BGIntTime_VIS = tree.Set(m_BGIntTime_VIS, m_BGIntTime_VIS, "VIS Background cal integration time", "VIS Background cal integration(exposure) time");
                m_BGIntTime_NIR = tree.Set(m_BGIntTime_NIR, m_BGIntTime_NIR, "NIR Background cal integration time", "NIR Background cal integration(exposure) time");
                m_Average_VIS = tree.Set(m_Average_VIS, m_Average_VIS, "VIS Spectrum Count", "VIS Spectrum Count");
                m_Average_NIR = tree.Set(m_Average_NIR, m_Average_NIR, "NIR Spectrum Count", "NIR Spectrum Count");
                m_InitialCal = tree.Set(m_InitialCal, m_InitialCal, "Initial Calibration", "Initial Calibration");
                m_CalWaferCenterPos_pulse = tree.Set(m_CalWaferCenterPos_pulse, m_CalWaferCenterPos_pulse, "Calibration Wafer Center Axis Position", "Calibration Wafer Center Axis Position(Pulse)");
                m_RefWaferCenterPos_pulse = tree.Set(m_RefWaferCenterPos_pulse, m_RefWaferCenterPos_pulse, "Reference Wafer Center Axis Position", "Reference Wafer Center Axis Position(Pulse)");
            }

            public override string Run()
            {
                AxisXY axisXY = m_module.m_axisXY;
                RPoint MovePoint;
                if (m_InitialCal)
                {
                    MovePoint = new RPoint(m_CalWaferCenterPos_pulse);
                }
                else
                {
                    MovePoint = new RPoint(m_RefWaferCenterPos_pulse);
                }
                if(m_module.Run(axisXY.StartMove(MovePoint)))
                    return p_sInfo;
                if(m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
                m_NanoView.Calibration(m_BGIntTime_VIS,m_BGIntTime_NIR,m_Average_VIS,m_Average_NIR, m_InitialCal);
                return "OK";
            }
        }
        public class Run_Measure : ModuleRunBase
        {
            Module_Camellia m_module;
            Met.Nanoview m_NanoView;
            MainWindow_ViewModel m_mwvm;
            DataManager m_DataManager;
            RPoint m_WaferCenterPos_pulse = new RPoint(); // Pulse
            double m_dResX_um = 1;
            double m_dResY_um = 1;
            double m_dFocusZ_pulse = 1; // Pulse

            public Run_Measure(Module_Camellia module)
            {
                m_module = module;
                m_NanoView = module.Nanoview;
                m_mwvm = module.mwvm;
                m_DataManager = module.m_DataManager;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_Measure run = new Run_Measure(m_module);
                run.m_DataManager = m_module.m_DataManager;
                run.m_WaferCenterPos_pulse = m_WaferCenterPos_pulse;
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_WaferCenterPos_pulse = tree.Set(m_WaferCenterPos_pulse, m_WaferCenterPos_pulse, "Wafer Center Axis Position", "Wafer Center Axis Position(Pulse)");
                m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Camera X Resolution", "Camera X Resolution(um)");
                m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Camera Y Resolution", "Camera Y Resolution(um)");
                m_dFocusZ_pulse = tree.Set(m_dFocusZ_pulse, m_dFocusZ_pulse, "Focus Z Position", "Focus Z Position(pulse)");
            }
            public override string Run()
            {
                //m_module.p_eState = (eState)5;
                AxisXY axisXY = m_module.m_axisXY;
                Axis axisZ = m_module.m_axisZ;
                Camera_Basler VRS = m_module.m_CamVRS;
                ImageData img = VRS.p_ImageViewer.p_ImageData;
                string strVRSImageDir = "D:\\";
                string strVRSImageFullPath = "";
                RPoint MeasurePoint;
                for (int i = 0; i < m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count; i++)
                {
                    double dX = m_WaferCenterPos_pulse.X + m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i]].x * 10000;
                    double dY = m_WaferCenterPos_pulse.Y + m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i]].y * 10000;
                    MeasurePoint = new RPoint(dX, dY);

                    if (m_module.Run(axisXY.StartMove(MeasurePoint)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;

                    

                    VRS.GrabContinuousShot();
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
