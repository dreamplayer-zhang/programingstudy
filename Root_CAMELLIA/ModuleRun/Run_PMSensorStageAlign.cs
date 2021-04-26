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
using System.Drawing;
using static Root_CAMELLIA.Module.Module_Camellia;
using Met = Root_CAMELLIA.LibSR_Met;
namespace Root_CAMELLIA.Module
{
    class Run_PMSensorStageAlign : ModuleRunBase
    {
        Module_Camellia m_module;
        DataManager m_DataManager;
        public RPoint m_StageCenterPos_pulse = new RPoint(); // Pulse
        double m_dFocusZ_pulse = 1; // Pulse
        double m_dHolefocusZ_pulse = 1; // Pulse

        public Run_PMSensorStageAlign (Module_Camellia module)
        {
            m_module = module;
            m_DataManager = module.m_DataManager;
            InitModuleRun(module);
        }
        public override ModuleRunBase Clone()
        {
            Run_PMSensorStageAlign run = new Run_PMSensorStageAlign(m_module);
            run.m_StageCenterPos_pulse = m_StageCenterPos_pulse;
            run.m_dFocusZ_pulse = m_dFocusZ_pulse;
            run.m_dHolefocusZ_pulse = m_dHolefocusZ_pulse;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bReadOnly)
        {
            m_StageCenterPos_pulse = tree.Set(m_StageCenterPos_pulse, m_StageCenterPos_pulse, "Stage Center Axis Position", "Stage Center Axis Position(Pulse)", bVisible);
            m_dFocusZ_pulse = tree.Set(m_dFocusZ_pulse, m_dFocusZ_pulse, "Focus Z Position", "Focus Z Position(pulse)", bVisible);
            m_dHolefocusZ_pulse = tree.Set(m_dHolefocusZ_pulse, m_dHolefocusZ_pulse, "Hole Focus Z Position", "Hole Focus Z Position(pulse)", bVisible);
        }

        public override string Run()
        {

            Met.PMDatas m_PMData = new Met.PMDatas();
            Met.Nanoview m_nanoview = new Met.Nanoview();
            bool rstHole = false;
            m_PMData.neee = 0;
            m_PMData.FittingPoints.Clear();

            Axis axisLifter = m_module.p_axisLifter;
            // PM 동작 하기전에 lifter 내려가 있는지 체크
            if (m_module.LifterDown() != "OK")
            {
                return p_sInfo;
            }

            //Camera Light Off // 조명값 0
            m_module.SetLight(false);
            m_log.Info("[CheckCameraSensorOffset] Camera Light Off");
            //셔터 오픈 확인// 셔터가 열려져 있는 상태에서 또 열으라는 명령어를 내린가면 이상 없을 까?
            bool bCheckShutter = m_nanoview.CheckShutter();
            bool bShutterOpen = false;

            if (!bCheckShutter)
            {
                bShutterOpen = m_nanoview.ShutterMotion(bCheckShutter);
            }
            else
            {
                bShutterOpen = bCheckShutter;
            }
            m_log.Info("[CheckCSSAlign] Sensor Light Open Ready");
            //카메라 연결 확인 
            string strVRSImageDir = "D:\\Temp\\";
            string strVRSImageFullPath = "";
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
            m_log.Info("[CheckCameraSensorOffset] Navigation Camera Ready");
            //Z축 이동 준비
            Axis axisZ = m_module.p_axisZ;
            m_log.Info("[CheckCameraSensorOffset] Camera Z Ready");
            //Stage XY 축 이동 준비 
            AxisXY axisXY = m_module.p_axisXY;
            m_log.Info("[CheckCameraSensorOffset] Stage Move Ready");
            //1. 시작
            m_PMData.LoadPMData();
            m_log.Info("[CheckCameraSensorOffset] Start");

            //2.측정 Hole 위치로 이동
            if (m_module.Run(axisXY.StartMove(m_StageCenterPos_pulse)))
                return p_sInfo;

            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCameraSensorOffset] Stage Hole Move Done");

            //3. Ready 위치로 z축 이동
            if (m_module.Run(axisZ.StartMove(m_dFocusZ_pulse)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCameraSensorOffset] Camera Z Move Done");
            //Ready 위치 이미지 grab
            Emgu.CV.Mat ReadyPositionMat = new Emgu.CV.Mat();
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
                VRS.GrabOneShot();
                {
                    strVRSImageFullPath = string.Format(strVRSImageDir + "PMHoleReadyImage.bmp", 0);
                    img.SaveImageSync(strVRSImageFullPath);
                    ReadyPositionMat = new Emgu.CV.Mat(new System.Drawing.Size(VRS.GetRoiSize().X, VRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride * 3);
                    ReadyPositionMat.Save(strVRSImageFullPath);
                    m_log.Info("[CheckCameraSensorOffset] Hole Ready Image Capture Done");
                }
            }));

            // 이전에 확인했던 엣지 데이터 리스트 등 다 Clear 하기

            //4. Hole Center_Move Left
            RPoint pHoleLeftEdgePos_pulse = new RPoint();
            pHoleLeftEdgePos_pulse.X = m_StageCenterPos_pulse.X - m_PMData.nInHoleLeftMovePosX;
            pHoleLeftEdgePos_pulse.Y = m_StageCenterPos_pulse.Y;
            m_dHolefocusZ_pulse = m_dFocusZ_pulse + m_PMData.dFocusHoleEdgeLeftZ;

            //위치로 z축 이동
            if (m_module.Run(axisZ.StartMove(m_dHolefocusZ_pulse)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCameraSensorOffset] Camera Z Move Done");

            // PMData에서 오프셋 값 받아와서 Hole 왼쪽 엣지 이동

            if (m_module.Run(axisXY.StartMove(pHoleLeftEdgePos_pulse)))
                return p_sInfo;

            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCameraSensorOffset] Stage Hole Move Done");
            Thread.Sleep(1000);
            // 홀 왼쪽 엣지 이미지 그랩
            Emgu.CV.Mat HoleEdgeLeftMat = new Emgu.CV.Mat();
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
                VRS.GrabOneShot();
                {
                    strVRSImageFullPath = string.Format(strVRSImageDir + "PMHoleEdgeLeftImage.bmp", 0);
                    img.SaveImageSync(strVRSImageFullPath);
                    HoleEdgeLeftMat = new Emgu.CV.Mat(new System.Drawing.Size(VRS.GetRoiSize().X, VRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride * 3);
                    HoleEdgeLeftMat.Save(strVRSImageFullPath);
                    m_log.Info("[CheckCameraSensorOffset] Hole Edge Left Image Capture Done");
                }
            }));

            //홀 왼쪽 엣지 따기
            rstHole = m_PMData.FindEdgePoint(HoleEdgeLeftMat, pHoleLeftEdgePos_pulse, (int)Met.CheckEdge.Left);
            m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.CalImage(m_PMData.EdgeDrawImg);
            OpenCvSharp.CPlusPlus.Cv2.ImWrite(@"D:\Temp\LeftEdge" + ".jpg", m_PMData.EdgeDrawImg);
            ////RPoint pHoleLeftEdgePos_pulse = new RPoint();
            ////pHoleLeftEdgePos_pulse.X = 1762149;
            ////pHoleLeftEdgePos_pulse.Y = 1572840;
            ////OpenCvSharp.CPlusPlus.Mat sourceImageLeft = OpenCvSharp.CPlusPlus.Cv2.ImRead("C:\\Users\\ATI\\Desktop\\Met DS\\new20210324\\best\\x1762149_y1572840_z496025.png", OpenCvSharp.LoadMode.GrayScale);
            ////rstHole = m_PMData.FindEdgePointTest(sourceImageLeft, pHoleLeftEdgePos_pulse, (int)Met.CheckEdge.Left);
            
            //5. Hole Center_Move Right
            RPoint pHoleRightEdgePos_pulse = new RPoint();

            pHoleRightEdgePos_pulse.X = m_StageCenterPos_pulse.X + m_PMData.nInHoleRightMovePosX;
            pHoleRightEdgePos_pulse.Y = m_StageCenterPos_pulse.Y;
            m_dHolefocusZ_pulse = m_dFocusZ_pulse + m_PMData.dFocusHoleEdgeRightZ;
            //혹시 엣지마다 Z 위치값을 다르게 가져가야 한다면 교체 할것 

            //위치로 z축 이동
            if (m_module.Run(axisZ.StartMove(m_dHolefocusZ_pulse)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCameraSensorOffset] Camera Z Move Done");

            // PMData에서 오프셋 값 받아와서 오른쪽 Hole 엣지 이동

            if (m_module.Run(axisXY.StartMove(pHoleRightEdgePos_pulse)))
                return p_sInfo;

            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCameraSensorOffset] Stage Hole Move Done");

            // 홀 오른쪽 엣지 이미지 그랩
            Emgu.CV.Mat HoleEdgeRightMat = new Emgu.CV.Mat();
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
                VRS.GrabOneShot();
                {
                    strVRSImageFullPath = string.Format(strVRSImageDir + "PMHoleEdgeRightImage.bmp", 0);
                    img.SaveImageSync(strVRSImageFullPath);
                    HoleEdgeRightMat = new Emgu.CV.Mat(new System.Drawing.Size(VRS.GetRoiSize().X, VRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride * 3);
                    HoleEdgeRightMat.Save(strVRSImageFullPath);
                    m_log.Info("[CheckCameraSensorOffset] Hole Edge Right Image Capture Done");
                }
            }));

            //홀 오른쪽 엣지 따기
            rstHole = m_PMData.FindEdgePoint(HoleEdgeRightMat, pHoleRightEdgePos_pulse, (int)Met.CheckEdge.Right);
            m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.CalImage(m_PMData.EdgeDrawImg);
            OpenCvSharp.CPlusPlus. Cv2.ImWrite(@"D:\Temp\RightEdge"+".jpg", m_PMData.EdgeDrawImg);
            ////RPoint pHoleRightEdgePos_pulse = new RPoint();
            ////pHoleRightEdgePos_pulse.X = 1881100;
            ////pHoleRightEdgePos_pulse.Y = 1572840;
            ////OpenCvSharp.CPlusPlus.Mat sourceImageRight = OpenCvSharp.CPlusPlus.Cv2.ImRead("C:\\Users\\ATI\\Desktop\\Met DS\\new20210324\\best\\x1881100_y1572840_z496025.png", OpenCvSharp.LoadMode.GrayScale);
            ////rstHole = m_PMData.FindEdgePointTest(sourceImageRight, pHoleRightEdgePos_pulse, (int)Met.CheckEdge.Right);

            //6.Hole Center_Move up
            RPoint pHoleupEdgePos_pulse = new RPoint();

            pHoleupEdgePos_pulse.X = m_StageCenterPos_pulse.X;
            pHoleupEdgePos_pulse.Y = m_StageCenterPos_pulse.Y - m_PMData.nInHoleUpMovePosY;
            m_dHolefocusZ_pulse = m_dFocusZ_pulse + m_PMData.dFocusHoleEdgeUpZ;
            //혹시 엣지마다 Z 위치값을 다르게 가져가야 한다면 교체 할것 

            //위치로 z축 이동
            if (m_module.Run(axisZ.StartMove(m_dHolefocusZ_pulse)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCameraSensorOffset] Camera Z Move Done");

            // PMData에서 오프셋 값 받아와서 Hole 위 엣지 이동

            if (m_module.Run(axisXY.StartMove(pHoleupEdgePos_pulse)))
                return p_sInfo;

            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCameraSensorOffset] Stage Hole Move Done");

            // 홀 위 엣지 이미지 그랩
            Emgu.CV.Mat HoleEdgeUptMat = new Emgu.CV.Mat();
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
                VRS.GrabOneShot();
                {
                    strVRSImageFullPath = string.Format(strVRSImageDir + "PMHoleEdgeUpImage.bmp", 0);
                    img.SaveImageSync(strVRSImageFullPath);
                    HoleEdgeUptMat = new Emgu.CV.Mat(new System.Drawing.Size(VRS.GetRoiSize().X, VRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride * 3);
                    HoleEdgeUptMat.Save(strVRSImageFullPath);
                    m_log.Info("[CheckCameraSensorOffset] Hole Edge Up Image Capture Done");
                }
            }));
            //홀 위 엣지 따기
            rstHole = m_PMData.FindEdgePoint(HoleEdgeUptMat, pHoleupEdgePos_pulse, (int)Met.CheckEdge.Up);
            m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.CalImage(m_PMData.EdgeDrawImg);
            ////RPoint pHoleupEdgePos_pulse = new RPoint();
            ////pHoleupEdgePos_pulse.X = 1821860;
            ////pHoleupEdgePos_pulse.Y = 1508531;
            ////OpenCvSharp.CPlusPlus.Mat sourceImageUp = OpenCvSharp.CPlusPlus.Cv2.ImRead("C:\\Users\\ATI\\Desktop\\Met DS\\new20210324\\best\\x1821860_y1508531_z496025.png", OpenCvSharp.LoadMode.GrayScale);
            ////rstHole = m_PMData.FindEdgePointTest(sourceImageUp, pHoleupEdgePos_pulse, (int)Met.CheckEdge.Up);
            //7. Hole Center_Move Dawn
            RPoint pHoleDawnEdgePos_pulse = new RPoint();

            pHoleDawnEdgePos_pulse.X = m_StageCenterPos_pulse.X;
            pHoleDawnEdgePos_pulse.Y = m_StageCenterPos_pulse.Y + m_PMData.nInHoleDaumMovePosY;
            m_dHolefocusZ_pulse = m_dFocusZ_pulse + m_PMData.dFocusHoleEdgeDawnZ;
            //혹시 엣지마다 Z 위치값을 다르게 가져가야 한다면 교체 할것 

            //위치로 z축 이동
            if (m_module.Run(axisZ.StartMove(m_dHolefocusZ_pulse)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCameraSensorOffset] Camera Z Move Done");

            // PMData에서 오프셋 값 받아와서 Hole 아래 엣지 이동

            if (m_module.Run(axisXY.StartMove(pHoleDawnEdgePos_pulse)))
                return p_sInfo;

            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCameraSensorOffset] Stage Hole Move Done");

            // 홀 아래 엣지 이미지 그랩
            Emgu.CV.Mat HoleEdgeDawnMat = new Emgu.CV.Mat();
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
                VRS.GrabOneShot();
                {
                    strVRSImageFullPath = string.Format(strVRSImageDir + "PMHoleEdgeDawnImage.bmp", 0);
                    img.SaveImageSync(strVRSImageFullPath);
                    HoleEdgeDawnMat = new Emgu.CV.Mat(new System.Drawing.Size(VRS.GetRoiSize().X, VRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride * 3);
                    HoleEdgeDawnMat.Save(strVRSImageFullPath);
                    m_log.Info("[CheckCameraSensorOffset] Hole Edge Dawn Image Capture Done");
                }
            }));
            //홀 아래 엣지 따기

            rstHole = m_PMData.FindEdgePoint(HoleEdgeDawnMat, pHoleDawnEdgePos_pulse, (int)Met.CheckEdge.Dawn);
            m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.CalImage(m_PMData.EdgeDrawImg);

            
            ////RPoint pHoleDawnEdgePos_pulse = new RPoint();
            ////pHoleDawnEdgePos_pulse.X = 1821860;
            ////pHoleDawnEdgePos_pulse.Y = 1627045;
            ////OpenCvSharp.CPlusPlus.Mat sourceImageDawn = OpenCvSharp.CPlusPlus.Cv2.ImRead("C:\\Users\\ATI\\Desktop\\Met DS\\new20210324\\best\\x1821860_y1627045_z496025.png", OpenCvSharp.LoadMode.GrayScale);
            ////rstHole = m_PMData.FindEdgePointTest(sourceImageDawn, pHoleDawnEdgePos_pulse, (int)Met.CheckEdge.Dawn);
            ////// 8.Hole Center 
            ////m_PMData.CheckHoleCenterOffset(1821860, 1572840);
            // Hole Offset 계산
            //문제
            m_PMData.CheckHoleCenterOffset(m_StageCenterPos_pulse.X, m_StageCenterPos_pulse.Y);
            


            
            //9.Sensor Center
            //Ready 위치로 이동
            if (m_module.Run(axisXY.StartMove(m_StageCenterPos_pulse)))
                return p_sInfo;

            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCameraSensorOffset] Stage Ready Move Done");

            // Sensor focus Ready 위치로 z축 이동
            if (m_module.Run(axisZ.StartMove(m_dFocusZ_pulse)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;
            m_log.Info("[CheckCameraSensorOffset] Sensor focus Camera Z Move Done");
            //Ready 위치 이미지 grab
            //Emgu.CV.Mat ReadyPositionMat = new Emgu.CV.Mat();
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
                VRS.GrabOneShot();
                {
                    strVRSImageFullPath = string.Format(strVRSImageDir + "PMSensorImage.bmp", 0);
                    img.SaveImageSync(strVRSImageFullPath);
                    ReadyPositionMat = new Emgu.CV.Mat(new System.Drawing.Size(VRS.GetRoiSize().X, VRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride * 3);
                    ReadyPositionMat.Save(strVRSImageFullPath);
                    m_log.Info("[CheckCameraSensorOffset] Hole Sensor Image Capture Done");
                }
            }));
            // Sensor Center 찾기
            bool bCalcSensorCenter= m_PMData.CalcCenterPoint(true, ReadyPositionMat, false, m_StageCenterPos_pulse.X, m_StageCenterPos_pulse.Y );
            if (bCalcSensorCenter == true)
            {
                m_log.Info("[CheckCameraSensorOffset] Calculate Center Point <Sensor> Done");
            }
            else
            {
                m_log.Info("[CheckCameraSensorOffset] Calculate Center Point <Sensor> Error");
            }
            

            //10 Result
            // 두 Center 좌표 그려진 이미지 뿌리기
            Bitmap Imagbitmap = ReadyPositionMat.Bitmap;
            OpenCvSharp.CPlusPlus.Mat ReadyMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(Imagbitmap);

            OpenCvSharp.CPlusPlus.Mat HoleCenterCheckMat = new OpenCvSharp.CPlusPlus.Mat();
            
            ////m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.CalImage();
            m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.p_pmSensorHoleAlign.p_HoleOffsetX = m_PMData.dCalcHoleCenterOffsetX;
            m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.p_pmSensorHoleAlign.p_HoleOffsetY = m_PMData.dCalcHoleCenterOffsetY;

            // 먼저 Hole 센터 좌표 그려진 이미지 뿌리고

           
            HoleCenterCheckMat = m_PMData.CenterPointCheck(ReadyMat);
            m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.CalImage(HoleCenterCheckMat);
            //Test
            ////OpenCvSharp.CPlusPlus.Mat HoleCenterCheckMat = new OpenCvSharp.CPlusPlus.Mat();
            ////OpenCvSharp.CPlusPlus.Mat sourceImage = OpenCvSharp.CPlusPlus.Cv2.ImRead("C:\\Users\\ATI\\Desktop\\Met DS\\new20210324\\x1821860_y157840_z476453.png", OpenCvSharp.LoadMode.AnyColor);
            ////HoleCenterCheckMat = m_PMData.CenterPointCheck(sourceImage, 1821860, 1572840, true);
            ////m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.CalImage(HoleCenterCheckMat);
            ////// 
            //////// Sensor Offset 계산
            m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.p_pmSensorHoleAlign.p_SensorOffsetX = m_PMData.dCalcSensorOffsetX;
            m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.p_pmSensorHoleAlign.p_SensorOffsetY = m_PMData.dCalcSensorOffsetY;
            // 다음에 Sensor 좌표 그려진 이미지 뿌리기


            // Total Offset 계산하고
            m_PMData.Sensor_Camera_HoleOffset();
            //////// Total Offset 뿌리기
            m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.p_pmSensorHoleAlign.p_TotalOffsetX = m_PMData.dCalcTotalOffsetX;
            m_module.mwvm.EngineerViewModel.p_PMCheckReview_ViewModel.m_pmSensorHoleOffset_VM.p_pmSensorHoleAlign.p_TotalOffsetY = m_PMData.dCalcTotalOffsetY;

            return "OK";
        }
    }
}
