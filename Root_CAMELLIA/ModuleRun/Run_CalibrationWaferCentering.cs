using Root_CAMELLIA.Data;
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
using static Root_CAMELLIA.Module.Module_Camellia;

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

        bool m_bUseCustomSpeed = false;
        double m_dMoveSpeedX = 0;
        double m_dMoveSpeedY = 0;
        double m_dMoveAccX = -1;
        double m_dMoveDecX = -1;
        double m_dMoveAccY = -1;
        double m_dMoveDecY = -1;

        bool m_InitialCal = false;
        public bool m_useCal = true;
        public bool m_useCentering = true;

        public bool m_isPM = false;
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
            run.m_dResX_um = m_dResX_um;
            run.m_dResY_um = m_dResY_um;
            run.m_useCal = m_useCal;
            run.m_useCentering = m_useCentering;
            run.m_dFocusZ = m_dFocusZ;
            run.m_dMoveSpeedX = m_dMoveSpeedX;
            run.m_dMoveSpeedY = m_dMoveSpeedY;
            run.m_dMoveAccX = m_dMoveAccX;
            run.m_dMoveAccY = m_dMoveAccY;
            run.m_dMoveDecX = m_dMoveDecX;
            run.m_dMoveDecY = m_dMoveDecY;
            run.m_bUseCustomSpeed = m_bUseCustomSpeed;
            return run;
        }

        private bool CheckVaildParameter()
        {
            if(m_dMoveSpeedX == 0 || m_dMoveSpeedY == 0)
            {
                return false;
            }
            if(m_dMoveAccX == -1 || m_dMoveDecX == -1)
            {
                return false;
            }
            if(m_dMoveAccY == -1 || m_dMoveDecY == -1)
            {
                return false;
            }
            return true;
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
            
            m_dMoveSpeedX = tree.Set(m_dMoveSpeedX, m_dMoveSpeedX, "Move Speed X", "Move Speed X", bVisible);
            m_dMoveSpeedY = tree.Set(m_dMoveSpeedY, m_dMoveSpeedY, "Move Speed Y", "Move Speed Y", bVisible);
            m_dMoveAccX = tree.Set(m_dMoveAccX, m_dMoveAccX, "Move Acc X", "Move Acc X", bVisible);
            m_dMoveAccY = tree.Set(m_dMoveAccY, m_dMoveAccY, "Move Acc Y", "Move Acc Y", bVisible);
            m_dMoveDecX = tree.Set(m_dMoveDecX, m_dMoveDecX, "Move Dec X", "Move Dec X", bVisible);
            m_dMoveDecY = tree.Set(m_dMoveDecY, m_dMoveDecY, "Move Dec Y", "Move Dec Y", bVisible);
            m_bUseCustomSpeed = tree.Set(m_bUseCustomSpeed, m_bUseCustomSpeed, "Use Custom Speed", "Use Custom Speed", bVisible);
            //RunTreeLight(tree.GetTree("LightPower", false), bVisible, bReadOnly);
        }

        public override string Run()
        {
            StopWatch test = new StopWatch();
            test.Start();
            m_log.Warn("Measure Start");
            AxisXY axisXY = m_module.p_axisXY;
            Axis axisZ = m_module.p_axisZ;
            string strVRSImageDir = "D:\\";
            string strVRSImageFullPath = "";
            Met.DataManager.GetInstance().m_SettngData.nMeasureIntTime_NIR = m_DataManager.recipeDM.MeasurementRD.NIRIntegrationTime;
            Met.DataManager.GetInstance().m_SettngData.nMeasureIntTime_VIS = m_DataManager.recipeDM.MeasurementRD.VISIntegrationTime;

            if (m_module.LifterDown() != "OK")
            {
                return p_sInfo;
            }

            if (m_module.Run(axisZ.StartMove(491453)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;

            if (m_useCal)
            {
                if (m_useCentering)
                {
                    if (m_DataManager.m_calibration.Run(m_InitialCal,m_DataManager.recipeDM.MeasurementRD.NIRIntegrationTime, m_DataManager.recipeDM.MeasurementRD.VISIntegrationTime, m_isPM) != "OK")
                    {
                        return "Calibration fail";
                    }
                }
                else
                {
                    if (m_DataManager.m_calibration.Run(m_InitialCal,m_DataManager.recipeDM.MeasurementRD.NIRIntegrationTime, m_DataManager.recipeDM.MeasurementRD.VISIntegrationTime, m_isPM, false) != "OK")
                    {
                        return "Calibration fail";
                    }
                }
            }

            if (!m_useCentering)
            {
                if (m_module.Run(axisZ.StartMove(0)))
                    return p_sInfo;
                if (m_module.Run(axisZ.WaitReady()))
                    return p_sInfo;
                return "OK";
            }

            m_DataManager.m_waferCentering.FindEdgeInit();

            m_module.SetLight(true);

            Camera_Basler VRS = m_module.p_CamVRS;
            ImageData img = VRS.p_ImageViewer.p_ImageData;

            StopWatch sw = new StopWatch();
            if (VRS.p_CamInfo._OpenStatus == false) VRS.Connect();
            while (VRS.p_CamInfo._OpenStatus == false)
            {
                if (sw.ElapsedMilliseconds > 15000)
                {
                    sw.Stop();
                    return "Navigation Camera Not Connected";
                }
            }
            sw.Stop();


            m_module.VaccumOnOff(true);





            if (m_bUseCustomSpeed && CheckVaildParameter())
            {
                if (m_module.Run(axisXY.p_axisX.StartMove(m_WaferLT_pulse.X, m_dMoveSpeedX, m_dMoveAccX, m_dMoveDecX)))
                {
                    return p_sInfo;
                }
                if (m_module.Run(axisXY.p_axisY.StartMove(m_WaferLT_pulse.Y, m_dMoveSpeedY, m_dMoveAccY, m_dMoveDecY)))
                {
                    return p_sInfo;
                }
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
            }
            else
            {
                if (m_module.Run(axisXY.StartMove(m_WaferLT_pulse)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
            }
            //Thread.Sleep(1000);

            //ImageData asdv = new ImageData(VRS.p_ImageViewer.p_ImageData.m_MemData);

            if (VRS.Grab() == "OK")
            {
                //asdv.SetData(VRS.p_ImageViewer.p_ImageData.GetPtr(), new CRect(0,0, 2044, 2044), (int)VRS.p_ImageViewer.p_ImageData.p_Stride, 3);
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


            if (m_bUseCustomSpeed && CheckVaildParameter())
            {
                if (m_module.Run(axisXY.p_axisX.StartMove(m_WaferRT_pulse.X, m_dMoveSpeedX, m_dMoveAccX, m_dMoveDecX)))
                {
                    return p_sInfo;
                }
                if (m_module.Run(axisXY.p_axisY.StartMove(m_WaferRT_pulse.Y, m_dMoveSpeedY, m_dMoveAccY, m_dMoveDecY)))
                {
                    return p_sInfo;
                }
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
            }
            else
            {
                if (m_module.Run(axisXY.StartMove(m_WaferRT_pulse)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
            }
            // return "OK";
            //m_DataManager.m_waferCentering.FindEdge(param);
            //Thread.Sleep(1000);





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
            //m_DataManager.m_waferCentering.FindEdge(param);

            // Thread.Sleep(1000);
            if (m_bUseCustomSpeed && CheckVaildParameter())
            {
                if (m_module.Run(axisXY.p_axisX.StartMove(m_WaferRB_pulse.X, m_dMoveSpeedX, m_dMoveAccX, m_dMoveDecX)))
                {
                    return p_sInfo;
                }
                if (m_module.Run(axisXY.p_axisY.StartMove(m_WaferRB_pulse.Y, m_dMoveSpeedY, m_dMoveAccY, m_dMoveDecY)))
                {
                    return p_sInfo;
                }
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
            }
            else
            {
                if (m_module.Run(axisXY.StartMove(m_WaferRB_pulse)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
            }

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
            //m_DataManager.m_waferCentering.FindEdge(param);
            while ((!m_DataManager.m_waferCentering.FindLTEdgeDone || !m_DataManager.m_waferCentering.FindRTEdgeDone
                || !m_DataManager.m_waferCentering.FindRBEdgeDone) && m_useCentering)
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

            if (m_module.Run(axisZ.StartMove(0)))
                return p_sInfo;
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;

            m_module.SetLight(false);

            test.Stop();
            m_log.Warn("Calibration End >> " + test.ElapsedMilliseconds);

            return "OK";
        }
    }
}
