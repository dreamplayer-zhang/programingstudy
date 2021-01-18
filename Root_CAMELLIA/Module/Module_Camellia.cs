﻿using Root_CAMELLIA.Data;
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
        DIO_I m_vacuum;
        DIO_O m_vacuumOnOff;
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
            m_axisLifter.p_vaccumDIO_I = m_vacuum;
        }
        #endregion

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
            p_sInfo = m_toolBox.Get(ref m_vacuum, this, "Vaccum On");
            p_sInfo = m_toolBox.Get(ref m_vacuumOnOff, this, "Vaccum OnOff");
        }
        public Module_Camellia(string id, IEngineer engineer)
        {
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            base.InitBase(id, engineer);
            InitWorkPoint();
            m_DataManager = DataManager.Instance;
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_InitCalibration(this), true, "InitCalCentering");
            AddModuleRunList(new Run_CalibrationWaferCentering(this), true, "Bacground Calibration_Centering");
            AddModuleRunList(new Run_Measure(this), true, "Measurement");
        }

        public class Run_Delay : ModuleRunBase
        {
            Module_Camellia m_module;
            double m_secDelay = 2;
            public Run_Delay(Module_Camellia module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }

            public override string Run()
            {
                Thread.Sleep((int)(1000 * m_secDelay));
                return "OK";
            }
        }
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

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_calWaferCenterPos_pulse = tree.Set(m_calWaferCenterPos_pulse, m_calWaferCenterPos_pulse, "Calibration Wafer Center Axis Position", "Calibration Wafer Center Axis Position(Pulse)", bVisible);
                m_refWaferCenterPos_pulse = tree.Set(m_refWaferCenterPos_pulse, m_refWaferCenterPos_pulse, "Reference Wafer Center Axis Position", "Reference Wafer Center Axis Position(Pulse)", bVisible);
                m_useCalWafer = tree.Set(m_useCalWafer, m_useCalWafer, "Use Cal Wafer", "Use Cal Wafer", bVisible);
                m_useRefWafer = tree.Set(m_useRefWafer, m_useRefWafer, "Use Ref Wafer", "Use Ref Wafer", bVisible);
                m_InitialCal = tree.Set(m_InitialCal, m_InitialCal, "Initial Calibration", "Initial Calibration", bVisible);
            }

            public override string Run()
            {

                AxisXY axisXY = m_module.m_axisXY;

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
        public class Run_CalibrationWaferCentering : ModuleRunBase
        {
            Module_Camellia m_module;
            DataManager m_DataManager;
            (Met.SettingData, Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode;
            RPoint m_WaferLT_pulse = new RPoint(); // Pulse
            RPoint m_WaferRT_pulse = new RPoint(); // Pulse
            RPoint m_WaferRB_pulse = new RPoint(); // Pulse
            int m_EdgeSearchRange = 20;
            int m_EdgeSearchLength = 1000;
            int m_EdgeSearchLevel = 30;
            double m_dResX_um = 1;
            double m_dResY_um = 1;

            bool m_InitialCal = false;
            public bool m_useCal = true;
            public bool m_useCentering = true;

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
                run.m_EdgeSearchLength = m_EdgeSearchLength;
                run.m_EdgeSearchLevel = m_EdgeSearchLevel;
                run.m_useCal = m_useCal;
                run.m_useCentering = m_useCentering;
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
                m_InitialCal = tree.Set(m_InitialCal, m_InitialCal, "Initial Calibration", "Initial Calibration", bVisible);
                m_useCal = tree.Set(m_useCal, m_useCal, "Use Calibration", "Use Calibration", bVisible);
                m_useCentering = tree.Set(m_useCentering, m_useCentering, "Use Centering", "Use Centering", bVisible);
            }

            public override string Run()
            {

                AxisXY axisXY = m_module.m_axisXY;
                Axis axisZ = m_module.m_axisZ;
                string strVRSImageDir = "D:\\";
                string strVRSImageFullPath = "";

                if (m_useCal)
                {
                    m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
                    if (m_SettingDataWithErrorCode.Item2 == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                    {
                        Met.SettingData setting = m_SettingDataWithErrorCode.Item1;
                        m_DataManager.m_calibration.Run(setting.nBGIntTime_VIS, setting.nBGIntTime_NIR, setting.nAverage_VIS, setting.nAverage_NIR, m_InitialCal);
                    }
                }

                if (!m_useCentering)
                    return "OK";

                Camera_Basler VRS = m_module.m_CamVRS;
                ImageData img = VRS.p_ImageViewer.p_ImageData;


                if (m_module.LifterDown() != "OK")
                {
                    return p_sInfo;
                }



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

            public Run_Measure(Module_Camellia module)
            {
                m_module = module;
                m_mwvm = module.mwvm;
                m_DataManager = module.m_DataManager;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_Measure run = new Run_Measure(m_module);
                run.m_StageCenterPos_pulse = m_StageCenterPos_pulse;
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                run.m_dFocusZ_pulse = m_dFocusZ_pulse;
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_StageCenterPos_pulse = tree.Set(m_StageCenterPos_pulse, m_StageCenterPos_pulse, "Stage Center Axis Position", "Stage Center Axis Position(Pulse)", bVisible);
                m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Camera X Resolution", "Camera X Resolution(um)", bVisible);
                m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Camera Y Resolution", "Camera Y Resolution(um)", bVisible);
                m_dFocusZ_pulse = tree.Set(m_dFocusZ_pulse, m_dFocusZ_pulse, "Focus Z Position", "Focus Z Position(pulse)", bVisible);
            }
            public override string Run()
            {

                Axis axisLifter = m_module.p_axisLifter;
                if(m_module.LifterDown() != "OK")
                {
                    return p_sInfo;
                }

                m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
                Met.SettingData setting = null;
                if (m_SettingDataWithErrorCode.Item2 == Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    setting = m_SettingDataWithErrorCode.Item1;
                }
                else
                {
                    return "SettingDataLoad Error";
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

                Met.DataManager dm = Met.DataManager.GetInstance();

                dm.ClearRawData();


                for (int i = 0; i < m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint.Count; i++)
                {

                    double x = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i]].x;
                    double y = m_DataManager.recipeDM.MeasurementRD.DataSelectedPoint[m_DataManager.recipeDM.MeasurementRD.DataMeasurementRoute[i]].y;

                    double dX = centerX - x * 10000;
                    double dY = centerY - y * 10000;
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

                    if(App.m_nanoView.SampleMeasure(i, x, y, m_DataManager.recipeDM.MeasurementRD.VISIntegrationTime, setting.nAverage_VIS, m_DataManager.recipeDM.MeasurementRD.NIRIntegrationTime, setting.nAverage_NIR) != Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                    {
                        return "Layer Model Not Ready";
                    }

                    dm.SaveRawData(@"C:\Users\ATI\Desktop\MeasureData\test" + i, i);
                    App.m_nanoView.GetThickness(i);
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
            if (!LifterMoveVacuumCheck())
            {
                p_eState = eState.Error;
                p_sInfo = "Vacuum is not turn off";
                return p_sInfo;
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

        public string LifterDown()
        {
            if (p_axisLifter.IsInPos(eAxisPos.Ready))
            {
                return "OK";
            }

            if (LifterMoveVacuumCheck())
            {
                if (!m_vacuum.p_bIn)
                {
                    if (Run(p_axisLifter.StartMove(eAxisPos.Ready)))
                    {
                        return p_sInfo;
                    }
                    if (Run(p_axisLifter.WaitReady()))
                        return p_sInfo;
                }
                else
                {
                    p_sInfo = p_id + " Vacuum is not turn off";
                    return p_sInfo;
                }
            }
            else
            {
                p_sInfo = p_id + " Vacuum is not turn off";
                return p_sInfo;
            }

            if (!m_vacuum.p_bIn)
            {
                VaccumOnOff(true);
            }

            return "OK";
        }

        public string LifterUp()
        {

            if (LifterMoveVacuumCheck())
            {
                if (!m_vacuum.p_bIn)
                {
                    if (p_axisLifter.IsInPos(ePosition.Position_0))
                    {
                        return "OK";
                    }

                    if (Run(p_axisLifter.StartMove(ePosition.Position_0)))
                    {
                        return p_sInfo;
                    }
                    if (Run(p_axisLifter.WaitReady()))
                        return p_sInfo;
                }
                else
                {
                    p_sInfo = p_id + " Vacuum is not turn off";
                    return p_sInfo;
                }
            }
            else
            {
                p_sInfo = p_id + " Vacuum is not turn off";
                return p_sInfo;
            }
            return "OK";
        }

        public bool LifterMoveVacuumCheck()
        {
            if (m_vacuum.p_bIn)
            {
                VaccumOnOff(false);
            }

            if (m_vacuum.p_bIn)
            {
                return false;
            }

            return true;
        }

        public void VaccumOnOff(bool onOff)
        {
            m_vacuumOnOff.Write(onOff);
            Thread.Sleep(1000);
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
            if (LifterUp() != "OK")
                return p_sInfo;

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

        enum eCheckWafer
        {
            InfoWafer,
            Sensor
        }
        eCheckWafer m_eCheckWafer = eCheckWafer.InfoWafer;
        public bool IsWaferExist(int nID)
        {
            switch (m_eCheckWafer)
            {
                case eCheckWafer.Sensor: return false; //m_diWaferExist.p_bIn;
                default: return (p_infoWafer != null);
            }
        }

        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");
            m_waferSize.RunTree(tree.GetTree("Wafer Size", false), true);
        }

    }


}
