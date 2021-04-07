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
            Met.PMDatas m_PMData = new Met.PMDatas();
            Met.CheckResult rst;
            Axis axisLifter = m_module.p_axisLifter;
            // PM 동작 하기전에 lifter 내려가 있는지 체크
            if (m_module.LifterDown() != "OK")
            {
                return p_sInfo;
            }
            //Camera Light Off
            m_module.SetLight(false);
            m_log.Info("[CheckCSSAlign] Camera Light Off");
            //셔터 오픈 확인// 셔터가 열려져 있는 상태에서 또 열으라는 명령어를 내린가면 이상 없을 까?
            m_log.Info("[CheckCSSAlign] Sensor Light Open Ready");
            //카메라 연결 확인 // 조명값 0
            string strVRSImageDir = "D:\\Temp\\";
            string strVRSImageFullPath = "";
            Camera_Basler VRS = m_module.p_CamVRS;
            ImageData img = VRS.p_ImageViewer.p_ImageData;
            m_log.Info("[CheckCSSAlign] Navigation Camera Ready");
            //Z축 이동 준비
            Axis axisZ = m_module.p_axisZ;
            m_log.Info("[CheckCSSAlign] Camera Z Ready");
            //Stage XY 축 이동 준비 
            AxisXY axisXY = m_module.p_axisXY;
            m_log.Info("[CheckCSSAlign] Stage Move Ready");
            //1. 시작
            m_PMData.LoadPMData();
            m_log.Info("[CheckCSSAlign] Start");

            //2. Ready 위치로 z축 이동
            if (m_module.Run(axisZ.StartMove(m_dFocusZ_pulse)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCSSAlign] Camera Z Move Done");

            //3.측정 Hole 위치로 이동
            if (m_module.Run(axisXY.StartMove(m_StageCenterPos_pulse)))
                return p_sInfo;

            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCSSAlign] Stage Hole Move Done");

            //4. Top Sensor Image Capture
            if (m_module.Run(axisZ.StartMove(m_dFocusZ_pulse - m_PMData.dCameraAxisOffsetZ)))
            {   
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;

            m_log.Info("[CheckCSSAlign] Camera Z Top Move Done");
            Emgu.CV.Mat Topmat = new Emgu.CV.Mat();
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
                VRS.GrabOneShot();
                //if (VRS.Grab() == "OK")
                {
                    strVRSImageFullPath = string.Format(strVRSImageDir + "PMCameraTopImage.bmp", 0);
                    img.SaveImageSync(strVRSImageFullPath);
                    Topmat = new Emgu.CV.Mat(new System.Drawing.Size(VRS.GetRoiSize().X, VRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride * 3);
                    Topmat.Save(strVRSImageFullPath);
                    m_log.Info("[CheckCSSAlign] Camera Z Top Image Capture Done");

                }
            }));

            //Point SensorCenter = new Point(980, 980);

            // 수정 !클리한 좌표를 얻어오거나, 입력한 Test 박스 데이터를 얻어와서 
           // List<OpenCvSharp.CPlusPlus.Point> m_HolePoints = new List<OpenCvSharp.CPlusPlus.Point>();
            //m_HolePoints.Add(new OpenCvSharp.CPlusPlus.Point(SensorCenter.X, SensorCenter.Y));
            //m_HolePoints = List<OpenCvSharp.CPlusPlus(SensorCenter.X, SensorCenter.Y);
            //return "OK";
            //수정 ! 획득한 좌표로 바꿀것
            //m_HolePoints.Add(new OpenCvSharp.CPlusPlus.Point(InHolePos.X, InHolePos.Y ));
            bool bCalcTopRst = m_PMData.CalcCenterPoint( true,Topmat);
            if (bCalcTopRst == true)
            {
                m_log.Info("[CheckCSSAlign] Calculate Center Point <Top> Done");
            }
            else
            {
                m_log.Info("[CheckCSSAlign] Calculate Center Point <Top> Error");
            }


            //5. Bottom Sensor Image Capture
            if (m_module.Run(axisZ.StartMove(m_dFocusZ_pulse + m_PMData.dCameraAxisOffsetZ)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;

            m_log.Info("[CheckCSSAlign] Camera Z Bottom Move Done");
            Emgu.CV.Mat Botmat = new Emgu.CV.Mat();
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
                VRS.GrabOneShot();
                //if (VRS.Grab() == "OK")
                {
                    strVRSImageFullPath = string.Format(strVRSImageDir + "PMCameraBottomImage.bmp", 0);
                    img.SaveImageSync(strVRSImageFullPath);
                    Botmat = new Emgu.CV.Mat(new System.Drawing.Size(VRS.GetRoiSize().X, VRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride * 3);
                    Botmat.Save(strVRSImageFullPath);
                    m_log.Info("[CheckCSSAlign] Camera Z Bottom Image Capture Done");

                }
            }));

            
            bool bCalcBotRst = m_PMData.CalcCenterPoint(false, Botmat);
            if (bCalcBotRst == true)
            {
                m_log.Info("[CheckCSSAlign] Calculate Center Point <Bottom> Done");
            }
            else
            {
                m_log.Info("[CheckCSSAlign] Calculate Center Point <Bottom> Error");
            }

            //5. Sensor- Camera Tilt 계산
            m_PMData.nAlignAxisPosZ = Convert.ToInt32(m_dFocusZ_pulse);
            rst = m_PMData.CheckCSSAlign();
            if (rst == Met.CheckResult.OK)
            {
                m_log.Info("[CheckCSSAlign] Done");
                
            }
            else
            {
                m_log.Info("[CheckCSSAlign] Error");
            }

            return "OK";
        }
    }
}