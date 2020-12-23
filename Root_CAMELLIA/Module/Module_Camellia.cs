using Root_CAMELLIA.Data;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Light;
using RootTools.Module;
using RootTools.Trees;
using System;
using Met = LibSR_Met;
using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.Structure;
using RootTools.ImageProcess;
using RootTools.Camera.Dalsa;
using RootTools.Control.Ajin;
using RootTools.Inspects;
using RootTools.Memory;
using RootTools.RADS;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static RootTools.Control.Axis;
using System.Windows.Diagnostics;
using Root_EFEM.Module;
using Root_EFEM;

namespace Root_CAMELLIA.Module
{
    public class Module_Camellia : ModuleBase, IWTRChild
    {
        public DataManager m_DataManager;
        public MainWindow_ViewModel mwvm;

        #region ToolBox

        AxisXY m_axisXY;
        public AxisXY p_axisXY
        {
            get
            {
                return m_axisXY;
            }
            set
            {
                m_axisXY = value;
            }
        }
        Axis m_axisZ;
        public Axis p_axisZ
        {
            get
            {
                return m_axisZ;
            }
            set
            {
                m_axisZ = value;
            }
        }
        Axis m_axisLifter;
        public Axis p_axisLifter
        {
            get
            {
                return m_axisLifter;
            }
            set
            {
                m_axisLifter = value;
            }
        }

        private bool _bLock = false;
        public bool p_bLock
        {
            get
            {
                return _bLock;
            }
            set
            {
                if (_bLock == value)
                    return;
                _bLock = value;
            }
        }

        #region InfoWafer
        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get
            {
                return _infoWafer;
            }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_reg != null)
                    m_reg.Write("sInfoWafer", m_sInfoWafer);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoWafer_Registry()
        {
            m_reg = new Registry(p_id + ".InfoWafer");
            m_sInfoWafer = m_reg.Read("sInfoWafer", m_sInfoWafer);
            p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        }
        #endregion

        public List<string> p_asChildSlot
        {
            get
            {
                return null;
            }
        }

        InfoWafer.WaferSize m_waferSize;

        DIO_I m_axisXReady;
        DIO_I m_axisYReady;
        DIO_I m_vaccum;
        DIO_O m_vaccumOnOff;
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

        #region Axis WorkPoint
        enum eAxisPos
        {
            Ready,
        }
        private void InitWorkPoint()
        {
            m_axisXY.p_axisX.AddPos(Enum.GetNames(typeof(eAxisPos)));
            m_axisXY.p_axisY.AddPos(Enum.GetNames(typeof(eAxisPos)));
            m_axisZ.AddPos(Enum.GetNames(typeof(eAxisPos)));
            m_axisLifter.AddIO(m_axisXReady);
            m_axisLifter.AddIO(m_axisYReady);
            //m_axisLifter.AddIO(m_vaccum);
            m_axisLifter.p_vaccumDIO_I = m_vaccum;
        }
        #endregion


        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "StageXY");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "StageZ");
            p_sInfo = m_toolBox.Get(ref m_axisLifter, this, "StageLifter");
            p_sInfo = m_toolBox.Get(ref m_CamVRS, this, "VRS");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.Get(ref m_axisXReady, this, "Stage X Ready");
            p_sInfo = m_toolBox.Get(ref m_axisYReady, this, "Stage Y Ready");
            p_sInfo = m_toolBox.Get(ref m_vaccum, this, "Vaccum On");
            p_sInfo = m_toolBox.Get(ref m_vaccumOnOff, this, "Vaccum OnOff");
        }
        #endregion

        public Module_Camellia(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            InitWorkPoint();
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
            AddModuleRunList(new Run_InitCalWaferCentering(this), false, "InitCalCentering");
            AddModuleRunList(new Run_WaferCentering(this), false, "Centering");
            AddModuleRunList(new Run_Measure(this), false, "Measurement");
            AddModuleRunList(new Run_VRSTEST(this), false, "VRSTEST");
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
        public class Run_InitCalWaferCentering : ModuleRunBase
        {
            Module_Camellia m_module;
            DataManager m_DataManager;
            RPoint m_WaferLT_pulse = new RPoint(); // Pulse
            RPoint m_WaferRT_pulse = new RPoint(); // Pulse
            RPoint m_WaferRB_pulse = new RPoint(); // Pulse
            int m_BGIntTime_VIS = 50;
            int m_BGIntTime_NIR = 150;
            int m_Average_VIS = 5;
            int m_Average_NIR = 3;
            bool m_InitialCal = true;

            double m_dResX_um = 1;
            double m_dResY_um = 1;
            int m_EdgeSearchLevel = 30;
            int m_EdgeSearchRange = 20;
            int m_EdgeSearchLength = 500;

            public Run_InitCalWaferCentering(Module_Camellia module)
            {
                m_module = module;
                m_DataManager = module.m_DataManager;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_InitCalWaferCentering run = new Run_InitCalWaferCentering(m_module);
                run.m_DataManager = m_module.m_DataManager;
                run.m_WaferLT_pulse = m_WaferLT_pulse;
                run.m_WaferRT_pulse = m_WaferRT_pulse;
                run.m_WaferRB_pulse = m_WaferRB_pulse;
                run.m_BGIntTime_NIR = m_BGIntTime_NIR;
                run.m_BGIntTime_VIS = m_BGIntTime_VIS;
                run.m_Average_NIR = m_Average_NIR;
                run.m_Average_VIS = m_Average_VIS;
                run.m_InitialCal = m_InitialCal;
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                run.m_EdgeSearchLength = m_EdgeSearchLength;
                run.m_EdgeSearchLevel = m_EdgeSearchLevel;
                run.m_EdgeSearchRange = m_EdgeSearchRange;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_WaferLT_pulse = tree.Set(m_WaferLT_pulse, m_WaferLT_pulse, "Wafer Left Top", "Wafer Left Top (Pulse)", bVisible);
                m_WaferRT_pulse = tree.Set(m_WaferRT_pulse, m_WaferRT_pulse, "Wafer Right Top", "Wafer Right Top (Pulse)", bVisible);
                m_WaferRB_pulse = tree.Set(m_WaferRB_pulse, m_WaferRB_pulse, "Wafer Right Bottom", "Wafer Right Bottom (Pulse)", bVisible);
                m_EdgeSearchRange = tree.Set(m_EdgeSearchRange, m_EdgeSearchRange, "Edge Search Range", "Edge Search Range", bVisible);
                m_EdgeSearchLength = tree.Set(m_EdgeSearchLength, m_EdgeSearchLength, "Edge Search Length", "Edge Search Length", bVisible);
                m_BGIntTime_VIS = tree.Set(m_BGIntTime_VIS, m_BGIntTime_VIS, "VIS Background cal integration time", "VIS Background cal integration(exposure) time", bVisible);
                m_BGIntTime_NIR = tree.Set(m_BGIntTime_NIR, m_BGIntTime_NIR, "NIR Background cal integration time", "NIR Background cal integration(exposure) time", bVisible);
                m_Average_VIS = tree.Set(m_Average_VIS, m_Average_VIS, "VIS Spectrum Count", "VIS Spectrum Count", bVisible);
                m_Average_NIR = tree.Set(m_Average_NIR, m_Average_NIR, "NIR Spectrum Count", "NIR Spectrum Count", bVisible);
                m_InitialCal = tree.Set(m_InitialCal, m_InitialCal, "Initial Calibration", "Initial Calibration", bVisible);
            }

            public override string Run()
            {

                //m_NanoView.Calibration(m_BGIntTime_VIS, m_BGIntTime_NIR, m_Average_VIS, m_Average_NIR, m_InitialCal);
                m_DataManager.m_calibration.Run(m_BGIntTime_VIS, m_BGIntTime_NIR, m_Average_VIS, m_Average_NIR, m_InitialCal);
                AxisXY axisXY = m_module.m_axisXY;
                Axis axisZ = m_module.m_axisZ;
                string strVRSImageDir = "D:\\";
                string strVRSImageFullPath = "";

                //AxisXY axisXY = m_module.m_axisXY;

                Camera_Basler VRS = m_module.m_CamVRS;
                ImageData img = VRS.p_ImageViewer.p_ImageData;


                if (m_module.Run(axisZ.StartMove(47932)))
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
                m_DataManager.m_waferCentering.FindEdge(img, VRS.GetRoiSize(), m_EdgeSearchRange, m_EdgeSearchLength, m_EdgeSearchLevel, WaferCentering.eDir.LT);


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
                m_DataManager.m_waferCentering.FindEdge(img, VRS.GetRoiSize(), m_EdgeSearchRange, m_EdgeSearchLength, m_EdgeSearchLevel, WaferCentering.eDir.RT);


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
                m_DataManager.m_waferCentering.FindEdge(img, VRS.GetRoiSize(), m_EdgeSearchRange, m_EdgeSearchLength, m_EdgeSearchLevel, WaferCentering.eDir.RB);


                m_DataManager.m_waferCentering.CalCenterPoint(VRS.GetRoiSize(), m_dResX_um, m_dResY_um, m_WaferLT_pulse, m_WaferRT_pulse, m_WaferRB_pulse);


                if (m_module.Run(axisXY.StartMove(m_DataManager.m_waferCentering.m_ptCenter)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;

                return "OK";
            }
        }
        public class Run_WaferCentering : ModuleRunBase
        {
            Module_Camellia m_module;
            DataManager m_DataManager;
            RPoint m_WaferLT_pulse = new RPoint(); // Pulse
            RPoint m_WaferRT_pulse = new RPoint(); // Pulse
            RPoint m_WaferRB_pulse = new RPoint(); // Pulse
            int m_EdgeSearchRange = 20;
            int m_EdgeSearchLength = 500;
            int m_EdgeSearchLevel = 30;
            double m_dResX_um = 1;
            double m_dResY_um = 1;

            public Run_WaferCentering(Module_Camellia module)
            {
                m_module = module;
                m_DataManager = module.m_DataManager;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_WaferCentering run = new Run_WaferCentering(m_module);
                run.m_DataManager = m_module.m_DataManager;
                run.m_WaferLT_pulse = m_WaferLT_pulse;
                run.m_WaferRT_pulse = m_WaferRT_pulse;
                run.m_WaferRB_pulse = m_WaferRB_pulse;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_WaferLT_pulse = tree.Set(m_WaferLT_pulse, m_WaferLT_pulse, "Wafer Left Top", "Wafer Left Top (Pulse)", bVisible);
                m_WaferRT_pulse = tree.Set(m_WaferRT_pulse, m_WaferRT_pulse, "Wafer Right Top", "Wafer Right Top (Pulse)", bVisible);
                m_WaferRB_pulse = tree.Set(m_WaferRB_pulse, m_WaferRB_pulse, "Wafer Right Bottom", "Wafer Right Bottom (Pulse)", bVisible);
                m_EdgeSearchRange = tree.Set(m_EdgeSearchRange, m_EdgeSearchRange, "Edge Search Range", "Edge Search Range", bVisible);
                m_EdgeSearchLength = tree.Set(m_EdgeSearchLength, m_EdgeSearchLength, "Edge Search Length", "Edge Search Length", bVisible);
                m_EdgeSearchLevel = tree.Set(m_EdgeSearchLevel, m_EdgeSearchLevel, "Edge Search Level", "Edge Search Level", bVisible);
                m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Camera X Resolution", "Camera X Resolution(um)", bVisible);
                m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Camera Y Resolution", "Camera Y Resolution(um)", bVisible);
            }

            public override string Run()
            {

                AxisXY axisXY = m_module.m_axisXY;
                Axis axisZ = m_module.m_axisZ;
                string strVRSImageDir = "D:\\";
                string strVRSImageFullPath = "";

                //AxisXY axisXY = m_module.m_axisXY;

                Camera_Basler VRS = m_module.m_CamVRS;
                ImageData img = VRS.p_ImageViewer.p_ImageData;


                if (m_module.Run(axisZ.StartMove(47932)))
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
                m_DataManager.m_waferCentering.FindEdge(img, VRS.GetRoiSize(), m_EdgeSearchRange, m_EdgeSearchLength, m_EdgeSearchLevel, WaferCentering.eDir.LT);


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
                m_DataManager.m_waferCentering.FindEdge(img, VRS.GetRoiSize(), m_EdgeSearchRange, m_EdgeSearchLength, m_EdgeSearchLevel, WaferCentering.eDir.RT);


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
                m_DataManager.m_waferCentering.FindEdge(img, VRS.GetRoiSize(), m_EdgeSearchRange, m_EdgeSearchLength, m_EdgeSearchLevel, WaferCentering.eDir.RB);


                m_DataManager.m_waferCentering.CalCenterPoint(VRS.GetRoiSize(), m_dResX_um, m_dResY_um, m_WaferLT_pulse, m_WaferRT_pulse, m_WaferRB_pulse);


                if (m_module.Run(axisXY.StartMove(m_DataManager.m_waferCentering.m_ptCenter)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
                //img.p_Stride;
                //img.GetPtr();

                //if (m_module.Run(axisXY.StartMove(m_WaferLT_pulse)))
                //    return p_sInfo;
                //if (m_module.Run(axisXY.WaitReady()))
                //    return p_sInfo;

                //VRS.GrabOneShot();

                ////? 엣지 따는 함수 추가
                // m_DataManager.m_waferCentering.FindEdge(img.GetByteArray(), WaferCentering.ePos.LT, VRS.GetRoiSize());

                //if (m_module.Run(axisXY.StartMove(m_WaferRT_pulse)))
                //    return p_sInfo;
                //if (m_module.Run(axisXY.WaitReady()))
                //    return p_sInfo;
                ////? 엣지 따는 함수 추가
                //VRS.GrabOneShot();
                ////m_DataManager.m_waferCentering.FindEdge(img.GetByteArray(), WaferCentering.ePos.RT);

                //if (m_module.Run(axisXY.StartMove(m_WaferRB_pulse)))
                //    return p_sInfo;
                //if (m_module.Run(axisXY.WaitReady()))
                //    return p_sInfo;
                //VRS.GrabOneShot();
                //// m_DataManager.m_waferCentering.FindEdge(img.GetByteArray(), WaferCentering.ePos.RB);


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
            (Met.SettingData, Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode;
            public RPoint m_CalWaferCenterPos_pulse = new RPoint(); // Pulse
            public RPoint m_RefWaferCenterPos_pulse = new RPoint(); // Pulse
            public Run_Calibration(Module_Camellia module)
            {
                m_module = module;
                m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
                if (m_SettingDataWithErrorCode.Item1 != null)
                {
                    m_Average_VIS = m_SettingDataWithErrorCode.Item1.nAverage_VIS;
                    m_Average_NIR = m_SettingDataWithErrorCode.Item1.nAverage_NIR;
                    m_BGIntTime_VIS = m_SettingDataWithErrorCode.Item1.nBGIntTime_VIS;
                    m_BGIntTime_NIR = m_SettingDataWithErrorCode.Item1.nBGIntTime_NIR;
                }

                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Calibration run = new Run_Calibration(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_BGIntTime_VIS = tree.Set(m_BGIntTime_VIS, m_BGIntTime_VIS, "VIS Background cal integration time", "VIS Background cal integration(exposure) time", bVisible);
                m_BGIntTime_NIR = tree.Set(m_BGIntTime_NIR, m_BGIntTime_NIR, "NIR Background cal integration time", "NIR Background cal integration(exposure) time", bVisible);
                m_Average_VIS = tree.Set(m_Average_VIS, m_Average_VIS, "VIS Spectrum Count", "VIS Spectrum Count", bVisible);
                m_Average_NIR = tree.Set(m_Average_NIR, m_Average_NIR, "NIR Spectrum Count", "NIR Spectrum Count", bVisible);

                if (m_SettingDataWithErrorCode.Item1 != null)
                {
                    m_SettingDataWithErrorCode.Item1.nAverage_VIS = m_Average_VIS;
                    m_SettingDataWithErrorCode.Item1.nAverage_NIR = m_Average_NIR;
                    m_SettingDataWithErrorCode.Item1.nBGIntTime_VIS = m_BGIntTime_VIS;
                    m_SettingDataWithErrorCode.Item1.nBGIntTime_NIR = m_BGIntTime_NIR;
                    App.m_nanoView.SaveSettingParameters(m_SettingDataWithErrorCode.Item1);
                }

               
                m_InitialCal = tree.Set(m_InitialCal, m_InitialCal, "Initial Calibration", "Initial Calibration", bVisible);
                m_CalWaferCenterPos_pulse = tree.Set(m_CalWaferCenterPos_pulse, m_CalWaferCenterPos_pulse, "Calibration Wafer Center Axis Position", "Calibration Wafer Center Axis Position(Pulse)", bVisible);
                m_RefWaferCenterPos_pulse = tree.Set(m_RefWaferCenterPos_pulse, m_RefWaferCenterPos_pulse, "Reference Wafer Center Axis Position", "Reference Wafer Center Axis Position(Pulse)", bVisible);
            }

            public override string Run()
            {
                AxisXY axisXY = m_module.m_axisXY;
                //if (m_InitialCal)
                //{
                //    MovePoint = new RPoint(m_CalWaferCenterPos_pulse);
                //}
                //else
                //{
                //    MovePoint = new RPoint(m_RefWaferCenterPos_pulse);
                //}
                //if(m_module.Run(axisXY.StartMove(MovePoint)))
                //    return p_sInfo;
                //if(m_module.Run(axisXY.WaitReady()))
                //    return p_sInfo;
                //m_InitalCal은 지금은 무조건 false로 쓰니깐 하드코딩해놓고 보류...
                //centring 하면서 Cal은 동시에 진행해도 됨
                //calibration : 지금 위치에서 그냥 바로 cal 시작
                m_NanoView.Calibration(m_BGIntTime_VIS, m_BGIntTime_NIR, m_Average_VIS, m_Average_NIR, m_InitialCal); //m_InitialCal);
                return "OK";
            }
        }
        public class Run_Measure : ModuleRunBase
        {
            Module_Camellia m_module;
            MainWindow_ViewModel m_mwvm;
            DataManager m_DataManager;
            (Met.SettingData, Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode;
            public RPoint m_StageCenterPos_pulse = new RPoint(); // Pulse
            double m_dResX_um = 1;
            double m_dResY_um = 1;
            double m_dFocusZ_pulse = 1; // Pulse

            int m_Average_VIS = 5;
            int m_Average_NIR = 3;
            bool m_LoadSettingData = false;

            public Run_Measure(Module_Camellia module)
            {
                m_module = module;
                m_mwvm = module.mwvm;
                m_DataManager = module.m_DataManager;

                m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
                if (m_SettingDataWithErrorCode.Item2 == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    m_Average_VIS = m_SettingDataWithErrorCode.Item1.nAverage_VIS;
                    m_Average_NIR = m_SettingDataWithErrorCode.Item1.nAverage_NIR;
                    m_LoadSettingData = true;
                }
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_Measure run = new Run_Measure(m_module);
                run.m_DataManager = m_module.m_DataManager;
                run.m_StageCenterPos_pulse = m_StageCenterPos_pulse;
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                run.m_dFocusZ_pulse = m_dFocusZ_pulse;
                run.m_Average_NIR = m_Average_NIR;
                run.m_Average_VIS = m_Average_VIS;
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_StageCenterPos_pulse = tree.Set(m_StageCenterPos_pulse, m_StageCenterPos_pulse, "Stage Center Axis Position", "Stage Center Axis Position(Pulse)", bVisible);
                m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Camera X Resolution", "Camera X Resolution(um)", bVisible);
                m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Camera Y Resolution", "Camera Y Resolution(um)", bVisible);
                m_dFocusZ_pulse = tree.Set(m_dFocusZ_pulse, m_dFocusZ_pulse, "Focus Z Position", "Focus Z Position(pulse)", bVisible);
                if(m_LoadSettingData == true)
                {

                }
                m_Average_VIS = tree.Set(m_Average_VIS, m_Average_VIS, "VIS Spectrum Count", "VIS Spectrum Count", bVisible);
                m_Average_NIR = tree.Set(m_Average_NIR, m_Average_NIR, "NIR Spectrum Count", "NIR Spectrum Count", bVisible);

                if(m_SettingDataWithErrorCode.Item1 != null)
                {
                    m_SettingDataWithErrorCode.Item1.nAverage_VIS = m_Average_VIS;
                    m_SettingDataWithErrorCode.Item1.nAverage_NIR = m_Average_NIR;
                    App.m_nanoView.SaveSettingParameters(m_SettingDataWithErrorCode.Item1);
                }
            }
            public override string Run()
            {

                Axis axisLifter = m_module.p_axisLifter;

                if (m_module.LifterUpVaccumCheck())
                {
                    if (!m_module.m_vaccum.p_bIn)
                    {
                        if (m_module.Run(axisLifter.StartHome()))
                        {
                            return p_sInfo;
                        }
                        if (m_module.Run(axisLifter.WaitReady()))
                            return p_sInfo;
                    }
                }

                //m_module.p_eState = (eState)5;
                AxisXY axisXY = m_module.p_axisXY;
                Axis axisZ = m_module.p_axisZ;
                
                // stage 48724 , wafer 47932
                if (m_module.Run(axisZ.StartMove(m_dFocusZ_pulse)))
                {
                    return p_sInfo;
                }
                if (m_module.Run(axisZ.WaitReady()))
                    return p_sInfo;

                Camera_Basler VRS = m_module.m_CamVRS;
                ImageData img = VRS.p_ImageViewer.p_ImageData;
                string strVRSImageDir = "D:\\";
                string strVRSImageFullPath = "";
                RPoint MeasurePoint;

                double centerX;
                double centerY;
                if (m_DataManager.m_waferCentering.m_ptCenter.X == 0 && m_DataManager.m_waferCentering.m_ptCenter.Y == 0)
                {
                    centerX = m_StageCenterPos_pulse.X;
                    centerY = m_StageCenterPos_pulse.Y;
                }
                else
                {
                    centerX = m_StageCenterPos_pulse.X - (m_DataManager.m_waferCentering.m_ptCenter.X - m_StageCenterPos_pulse.X);
                    centerY = m_StageCenterPos_pulse.Y - (m_DataManager.m_waferCentering.m_ptCenter.Y - m_StageCenterPos_pulse.Y);
                }

                double RatioX = (int)(BaseDefine.CanvasWidth / BaseDefine.ViewSize);
                double RatioY = (int)(BaseDefine.CanvasHeight / BaseDefine.ViewSize);

                m_mwvm.p_Progress = 0;


                for (int i = 0; i < m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count; i++)
                {

                    double x = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i]].x;
                    double y = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i]].y;

                    double dX = centerX + x * 10000;
                    double dY = centerY + y * 10000;
                    MeasurePoint = new RPoint(dX, dY);

                    if (m_module.Run(axisXY.StartMove(MeasurePoint)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;

                    m_mwvm.p_ArrowX1 = x * RatioX;
                    m_mwvm.p_ArrowY1 = -y * RatioY;
                    if (i < m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count - 1)
                    {
                        double x2 = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i + 1]].x;
                        double y2 = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i + 1]].y;
                        m_mwvm.p_ArrowX2 = x2 * RatioX;
                        m_mwvm.p_ArrowY2 = -y2 * RatioY;
                        m_mwvm.p_ArrowVisible = Visibility.Visible;
                    }

                    App.m_nanoView.SampleMeasure(i, x, y, m_DataManager.recipeDM.MeasurementRD.VISIntegrationTime, m_Average_VIS, m_DataManager.recipeDM.MeasurementRD.NIRIntegrationTime, m_Average_NIR)
;
                        Met.DataManager dm = Met.DataManager.GetInstance();
                    dm.SaveRawData(@"C:\Users\ATI\Desktop\Root_201221수정\MeasureData\", i);
                    m_mwvm.p_RTGraph.DrawReflectanceGraph(i, "Wavelength(nm)", "Reflectance(%)");
                    m_mwvm.p_RTGraph.DrawTransmittanceGraph(i, "Wavelength(nm)", "Reflectance(%)");


                    if (VRS.Grab() == "OK")
                    {
                        strVRSImageFullPath = string.Format(strVRSImageDir + "VRSImage_{0}.bmp", i);
                        img.SaveImageSync(strVRSImageFullPath);
                        //Grab error
                    }
                    //Thread.Sleep(600);

                    m_mwvm.p_Progress = (((double)(i + 1) / m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count) * 100);
                }
                m_mwvm.p_ArrowVisible = Visibility.Hidden;

                //? 세이브?

                if (m_module.Run(axisXY.StartMove(eAxisPos.Ready)))
                {
                    return p_sInfo;
                }
                if (m_module.Run(axisZ.StartHome()))
                {
                    return p_sInfo;
                }
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
                if (m_module.Run(axisZ.WaitReady()))
                    return p_sInfo;

                //if (m_module.LifterUpVaccumCheck())
                //{
                //    if (!m_module.m_vaccum.p_bIn)
                //    {
                //        if (m_module.Run(axisLifter.StartMove(ePosition.SWLimit_Plus)))
                //        {
                //            return p_sInfo;
                //        }
                //        if (m_module.Run(axisLifter.WaitReady()))
                //            return p_sInfo;
                //    }
                //}

                return "OK";
            }
        }
        public class Run_VRSTEST : ModuleRunBase
        {
            Module_Camellia m_module;
            MainWindow_ViewModel m_mwvm;
            DataManager m_DataManager;
            RPoint m_WaferCenterPos_pulse = new RPoint(); // Pulse
            double m_dResX_um = 1;
            double m_dResY_um = 1;
            double m_dFocusZ_pulse = 1; // Pulse

            public Run_VRSTEST(Module_Camellia module)
            {
                m_module = module;
                m_mwvm = module.mwvm;
                m_DataManager = module.m_DataManager;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_VRSTEST run = new Run_VRSTEST(m_module);
                run.m_DataManager = m_module.m_DataManager;
                run.m_WaferCenterPos_pulse = m_WaferCenterPos_pulse;
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_WaferCenterPos_pulse = tree.Set(m_WaferCenterPos_pulse, m_WaferCenterPos_pulse, "Wafer Center Axis Position", "Wafer Center Axis Position(Pulse)", bVisible);
                m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Camera X Resolution", "Camera X Resolution(um)", bVisible);
                m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Camera Y Resolution", "Camera Y Resolution(um)", bVisible);
                m_dFocusZ_pulse = tree.Set(m_dFocusZ_pulse, m_dFocusZ_pulse, "Focus Z Position", "Focus Z Position(pulse)", bVisible);
            }
            public override string Run()
            {
                Camera_Basler VRS = m_module.m_CamVRS;
                ImageData img = VRS.p_ImageViewer.p_ImageData;
                string strVRSImageDir = "D:\\";
                if (VRS.Grab() == "OK")
                {
                    string strVRSImageFullPath = string.Format(strVRSImageDir + "VRSImage_{0}.bmp", 1);
                    img.SaveImageSync(strVRSImageFullPath);
                    //Grab error
                }
                return "OK";
            }
        }

        public override string StateHome()
        {
            p_sInfo = "OK";
            if (EQ.p_bSimulate)
                return "OK";

            Thread.Sleep(200);
            if (m_listAxis.Count == 0) return "OK";
            if (p_eState == eState.Run) return "Invalid State : Run";
            if (EQ.IsStop()) return "Home Stop";

            foreach (Axis axis in m_listAxis)
            {
                if (axis != null) axis.ServoOn(true);
            }
            Thread.Sleep(200);
            if (EQ.IsStop()) return "Home Stop";

            for (int i = 0; i < p_axisLifter.m_bDIO_I.Count; i++)
            {
                p_axisLifter.m_bDIO_I[i] = false;
            }

            p_axisLifter.StartHome();
            if (p_axisLifter.WaitReady() != "OK")
            {
                p_eState = eState.Error;
                p_sInfo = "Lifter Home Error";
                return p_sInfo;
            }

            for (int i = 0; i < p_axisLifter.m_bDIO_I.Count; i++)
            {
                p_axisLifter.m_bDIO_I[i] = true;
            }


            p_axisXY.p_axisX.StartHome();
            p_axisXY.p_axisY.StartHome();
            p_axisZ.StartHome();

            if (p_axisXY.p_axisX.WaitReady() != "OK")
            {
                p_eState = eState.Error;
                return "AxisX Home Error";
            }

            if (p_axisXY.p_axisY.WaitReady() != "OK")
            {
                p_eState = eState.Error;
                return "AxisY Home Error";
            }

            if (p_axisZ.WaitReady() != "OK")
            {
                p_eState = eState.Error;
                return "AxisZ Home Error";
            }
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;

            return p_sInfo;
        }

        public bool LifterUpVaccumCheck()
        {
            if (m_vaccum.p_bIn)
            {
                m_vaccumOnOff.Write(false);
                Thread.Sleep(1000);
            }
            return true;
        }

        public InfoWafer GetInfoWafer(int nID)
        {
            return p_infoWafer;
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            p_infoWafer = infoWafer;
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null)
                infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string IsGetOK(int nID)
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (p_infoWafer == null)
                return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (p_infoWafer != null)
                return p_id + " IsPutOK - InfoWafer Exist";
            if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false)
                return p_id + " not Enable Wafer Size";
            return "OK";
        }

        private string MoveReadyPos()
        {
            if (p_axisLifter.IsInPos(ePosition.Position_0)) return "OK";

            /* XY Ready 위치 이동 */
            if (Run(p_axisXY.p_axisX.StartMove(eAxisPos.Ready)))
                return p_sInfo;
            if (Run(p_axisXY.p_axisY.StartMove(eAxisPos.Ready)))
                return p_sInfo;
            if (Run(p_axisZ.StartMove(eAxisPos.Ready)))
                return p_sInfo;
            if (Run(p_axisXY.WaitReady()))
                return p_sInfo;
            if (Run(p_axisZ.WaitReady()))
                return p_sInfo;
            /* Vaccum Check 후Lifter Up */
            if (LifterUpVaccumCheck())
            {
                if (!m_vaccum.p_bIn)
                {
                    //if (Run(p_axisLifter.StartMove(ePosition.SWLimit_Plus)))
                    if (Run(p_axisLifter.StartMove(ePosition.Position_0)))
                    {
                        return p_sInfo;
                    }
                    if (Run(p_axisLifter.WaitReady()))
                        return p_sInfo;
                }
                else
                {
                    return p_id + "Vaccum Error";
                }
            }

            return "OK";
        }

        public string BeforeGet(int nID)
        {
            string info = MoveReadyPos();
            if (info != "OK")
                return info;
            return "OK";
        }

        public string BeforePut(int nID)
        {
            string info = MoveReadyPos();
            if (info != "OK")
                return info;
            return "OK";
        }

        public string AfterGet(int nID)
        {
            return "OK";
        }

        public string AfterPut(int nID)
        {
            return "OK";
        }

        public bool IsWaferExist(int nID, bool bUseSensor = true)
        {
            if (bUseSensor)
                return (p_infoWafer != null);
            //            return m_diWaferExist.p_bIn;
            return false;
        }

        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }
    }


}
