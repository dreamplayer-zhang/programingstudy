using Root_CAMELLIA.Data;
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
    class Run_PMSensorCameraTilt : ModuleRunBase
    {
        Module_Camellia m_module;
        DataManager m_DataManager;
        public RPoint m_StageCenterPos_pulse = new RPoint(); // Pulse
        //double m_dResX_um = 1;
        //double m_dResY_um = 1;
        double m_dFocusZ_pulse = 1; // Pulse
        public Run_PMSensorCameraTilt (Module_Camellia module)
        {
            m_module = module;
            m_DataManager = module.m_DataManager;
            InitModuleRun(module);
        }
        public override ModuleRunBase Clone()
        {
            Run_PMSensorCameraTilt run = new Run_PMSensorCameraTilt (m_module);
            run.m_StageCenterPos_pulse = m_StageCenterPos_pulse;
            //run.m_dResX_um = m_dResX_um;
            //run.m_dResY_um = m_dResY_um;
            run.m_dFocusZ_pulse = m_dFocusZ_pulse;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_StageCenterPos_pulse = tree.Set(m_StageCenterPos_pulse, m_StageCenterPos_pulse, "Stage Center Axis Position", "Stage Center Axis Position(Pulse)", bVisible);
            //m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Camera X Resolution", "Camera X Resolution(um)", bVisible);
            //m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Camera Y Resolution", "Camera Y Resolution(um)", bVisible);
            m_dFocusZ_pulse = tree.Set(m_dFocusZ_pulse, m_dFocusZ_pulse, "Focus Z Position", "Focus Z Position(pulse)", bVisible);
        }

        public override string Run()
        {
            Axis axisLifter = m_module.p_axisLifter;
            // PM 동작 하기전에 lifter 내려가 있는지 체크
            if (m_module.LifterDown() != "OK")
            {
                return p_sInfo;
            }

            //축 이동 준비
            AxisXY axisXY = m_module.p_axisXY;
            Axis axisZ = m_module.p_axisZ;
            string strVRSImageDir = "D:\\Temp\\";
            string strVRSImageFullPath = "";

            Camera_Basler VRS = m_module.p_CamVRS;
            ImageData img = VRS.p_ImageViewer.p_ImageData;
            //z축 이동
            if (m_module.Run(axisZ.StartMove(m_dFocusZ_pulse)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;
            //double centerX;
            //double centerY;
            //if (m_DataManager.m_waferCentering.m_ptCenter.X == 0 && m_DataManager.m_waferCentering.m_ptCenter.Y == 0)
            //{
            //    centerX = m_StageCenterPos_pulse.X;
            //    centerY = m_StageCenterPos_pulse.Y;
            //}

            if (m_module.Run(axisXY.StartMove(m_StageCenterPos_pulse)))
                return p_sInfo;

            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;

            if (VRS.Grab() == "OK")
            {
                strVRSImageFullPath = string.Format(strVRSImageDir + "PMCameraImageTest.bmp", 0);
                //img.SaveImageSync(strVRSImageFullPath);
                Emgu.CV.Mat mat = new Emgu.CV.Mat(new System.Drawing.Size(VRS.GetRoiSize().X, VRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride * 3);
                mat.Save(strVRSImageFullPath);
                //Grab error
            }
            else
            {
                return "Grab Error";
            }

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
            return "OK";
        }
    }
}