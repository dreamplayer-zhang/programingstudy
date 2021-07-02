using Emgu.CV;
using Emgu.CV.Structure;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Database;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Root_VEGA_D.Module
{
    public class Run_PatternAlign : ModuleRunBase
    {
        Vision m_module;

        // Scan Parameter
        public GrabMode m_grabMode = null;
        public int m_nCurScanLine = 0;
        string m_sGrabMode = "";
        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }

        // Template Matching Parameter
        public int m_nSearchAreaSize = 1000;
        public double m_dMatchScore = 0.9;
        public Run_PatternAlign(Vision module)
        {
            m_module = module;
            InitModuleRun(module);
        }
        public override ModuleRunBase Clone()
        {
            Run_PatternAlign run = new Run_PatternAlign(m_module);
            run.p_sGrabMode = p_sGrabMode;
            run.m_nSearchAreaSize = m_nSearchAreaSize;
            run.m_dMatchScore = m_dMatchScore;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_nSearchAreaSize = tree.Set(m_nSearchAreaSize, m_nSearchAreaSize, "Template Matching Search Area Size", "Template Matching Search Area Size", bVisible);
            m_dMatchScore = tree.Set(m_dMatchScore, m_dMatchScore, "Template Matching Score", "Template Matching Score", bVisible);
        }

        public override string Run()
        {
            // Align Mark 스캔
            if (m_grabMode == null) return "Grab Mode == null";

            // StopWatch 설정
            StopWatch snapTimeWatcher = new StopWatch();
            snapTimeWatcher.Start();

            // 상수 값
            const int TIMEOUT_10MS = 10000;     // ms
            const int TIMEOUT_50MS = 50000;     // ms
            const int TIMEOUT_INTERVAL = 10;    // ms
            const int RESCAN_MAX = 3;
            int MM_PER_UM = 1000;

            // 축 가져오기
            AxisXY axisXY = m_module.AxisXY;
            Axis axisZ = m_module.AxisZ;
            Axis axisRotate = m_module.AxisRotate;

            Camera_Dalsa camMain = (Camera_Dalsa)m_grabMode.m_camera;
            Camera_Basler camRADS = (Camera_Basler)m_module.CamRADS;
            GrabData grabData = m_grabMode.m_GD;

            // variable
            CPoint cptTopCenter;
            CPoint cptBottomCenter;
            CPoint cptTopResultCenter;
            CPoint cptBottomResultCenter;
            Run_MakeTemplateImage moduleRun = (Run_MakeTemplateImage)m_module.CloneModuleRun("MakeTemplateImage");
            string strPool = m_grabMode.m_memoryPool.p_id;
            string strGroup = m_grabMode.m_memoryGroup.p_id;
            string strMemory = m_grabMode.m_memoryData.p_id;
            MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
            bool bFoundTop = false;
            bool bFoundBottom = false;
            Image<Gray, byte> imgTop = new Image<Gray, byte>("D:\\AlignMarkTemplateImage\\TopTemplateImage.bmp");
            Image<Gray, byte> imgBottom = new Image<Gray, byte>("D:\\AlignMarkTemplateImage\\BottomTemplateImage.bmp");

            try
            {
                // RADS 연결
                if (m_grabMode.pUseRADS && m_module.RADSControl.p_IsRun == false)
                {
                    m_module.RADSControl.StartRADS();

                    StopWatch sw = new StopWatch();
                    if (camRADS.p_CamInfo._OpenStatus == false) camRADS.Connect();
                    while (camRADS.p_CamInfo._OpenStatus == false)
                    {
                        if (sw.ElapsedMilliseconds > 15000)
                        {
                            sw.Stop();
                            return "RADS Camera Not Connected";
                        }
                    }
                    sw.Stop();
                    camRADS.SetMulticast();
                    camRADS.GrabContinuousShot();
                }

                // 카메라 연결 시도
                camMain.Connect();

                int nTimeOut = TIMEOUT_50MS / TIMEOUT_INTERVAL;
                while (camMain.p_CamInfo.p_eState != eCamState.Ready)
                {
                    if (nTimeOut-- == 0)
                    {
                        throw new Exception("Camera Connect Error");
                    }
                    Thread.Sleep(TIMEOUT_INTERVAL);
                }

                // 스캔 라인 초기화
                m_nCurScanLine = 0;

                // 기계 장치 설정
                //m_grabMode.SetLens();
                m_grabMode.SetLight(true);

                // 메모리 오프셋
                CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);
                cpMemoryOffset.X += m_grabMode.m_ScanStartLine * grabData.m_nFovSize;

                m_grabMode.m_dTrigger = Math.Round(m_grabMode.m_dResY_um * m_grabMode.m_dCamTriggerRatio, 1);     // 트리거 (1 pulse = 3.0 mm)

                int nWaferSizeY_px = (int)Math.Round(m_grabMode.m_nWaferSize_mm * MM_PER_UM / m_grabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수

                while (m_grabMode.m_ScanLineNum > m_nCurScanLine)
                {
                    if (EQ.IsStop())
                        return "OK";

                    // 이동 위치 계산
                    int nScanSpeed = Convert.ToInt32((double)m_grabMode.m_nMaxFrame * m_grabMode.m_dTrigger * (double)m_grabMode.m_nScanRate / 100);
                    int nScanOffset_pulse = (int)((double)nScanSpeed * axisXY.p_axisY.GetSpeedValue(Axis.eSpeed.Move).m_acc * 0.5) * 2;

                    int nLineIndex = m_grabMode.m_ScanStartLine + m_nCurScanLine;

                    double dfov_mm = grabData.m_nFovSize * m_grabMode.m_dResX_um * 0.001;
                    double dOverlap_mm = grabData.m_nOverlap * m_grabMode.m_dResX_um * 0.001;
                    double dPosX = m_grabMode.m_rpAxisCenter.X + m_grabMode.m_nWaferSize_mm * 0.5 - nLineIndex * (dfov_mm - dOverlap_mm);
                    double dNextPosX = dPosX - (dfov_mm - dOverlap_mm);
                    double dPosZ = m_grabMode.m_dFocusPosZ;

                    double dMarginY = m_grabMode.m_nWaferSize_mm * 0.1;
                    double dTriggerStartPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y - m_grabMode.m_nWaferSize_mm * 0.5 - dMarginY;
                    double dTriggerEndPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y + m_grabMode.m_nWaferSize_mm * 0.5 + dMarginY;
                    double dStartPosY = dTriggerStartPosY - nScanOffset_pulse;
                    double dEndPosY = dTriggerEndPosY + nScanOffset_pulse;

                    // Grab 방향 및 시작, 종료 위치 설정
                    m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                    if (m_grabMode.m_bUseBiDirectionScan && m_nCurScanLine % 2 == 1)
                    {
                        // dStartPosY <--> dEndPosY 바꿈.
                        (dStartPosY, dEndPosY) = (dEndPosY, dStartPosY);
                        (dTriggerStartPosY, dTriggerEndPosY) = (dTriggerEndPosY, dTriggerStartPosY);

                        m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                    }

                    //포커스 높이로 Z축 이동
                    if (m_module.Run(axisZ.StartMove(dPosZ)))
                        return p_sInfo;

                    // 시작 위치로 X, Y축 이동
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                    if (m_module.Run(axisXY.StartMove(dPosX, dStartPosY)))
                        return p_sInfo;

                    // 이동 대기
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                    if (m_module.Run(axisZ.WaitReady()))
                        return p_sInfo;

                    grabData.bInvY = m_grabMode.m_eGrabDirection == eGrabDirection.BackWard;
                    grabData.nScanOffsetY = nLineIndex * m_grabMode.m_nYOffset;

                    // 카메라 그랩 시작
                    CPoint tmpMemOffset = new CPoint(cpMemoryOffset);
                    // IPU PC와 연결된 상태에서는 이미지 데이터가 복사될 Main PC의 Memory 위치가
                    // Memory Width를 넘어가게 되면 다시 0부터 이미지를 얻어오도록 Memory Offset을 계산
                    long div = tmpMemOffset.X / mem.W;
                    long remain = tmpMemOffset.X - mem.W * div;
                    long offset = remain % grabData.m_nFovSize;
                    tmpMemOffset.X = (int)(remain - offset);
                    m_grabMode.StartGrab(mem, tmpMemOffset, nWaferSizeY_px, grabData);

                    // Y축 트리거 발생
                    axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, 0.001, true);

                    // 라인스캔 완료 대기
                    if (m_module.Run(axisXY.p_axisY.WaitReady()))
                        return p_sInfo;

                    // 다음 이미지 획득을 위해 변수 값 변경
                    m_nCurScanLine++;
                    cpMemoryOffset.X += grabData.m_nFovSize;
                }
                m_grabMode.m_camera.StopGrab();

                snapTimeWatcher.Stop();

                // Log
                TempLogger.Write("Snap", string.Format("{0:F3}", (double)snapTimeWatcher.ElapsedMilliseconds / (double)1000));

                return "OK";
            }
            catch (Exception e)
            {
                m_log.Info(e.Message);
                return "OK";
            }
            finally
            {
                // 조명 off
                m_grabMode.SetLight(false);

                // RADS 기능 off
                if (m_grabMode.pUseRADS && m_module.RADSControl.p_IsRun == true)
                {
                    m_module.RADSControl.StopRADS();

                    if (camRADS.p_CamInfo._IsGrabbing == true) camRADS.StopGrab();
                }
            }

            // implement
            cptTopCenter = moduleRun.m_cptTopAlignMarkCenterPos;
            cptBottomCenter = moduleRun.m_cptBottomAlignMarkCenterPos;

            // Top Template Image Processing
            Point ptStart = new Point(cptTopCenter.X - (m_nSearchAreaSize / 2), cptTopCenter.Y - (m_nSearchAreaSize / 2));
            Point ptEnd = new Point(cptTopCenter.X + (m_nSearchAreaSize / 2), cptTopCenter.Y + (m_nSearchAreaSize / 2));
            CRect crtSearchArea = new CRect(ptStart, ptEnd);
            Image<Gray, byte> imgSrc = m_module.GetGrayByteImageFromMemory_12bit(mem, crtSearchArea);
            bFoundTop = m_module.TemplateMatching(mem, crtSearchArea, imgSrc, imgTop, out cptTopResultCenter, m_dMatchScore);

            // Bottom Template Image Processing
            ptStart = new Point(cptBottomCenter.X - (m_nSearchAreaSize / 2), cptBottomCenter.Y - (m_nSearchAreaSize / 2));
            ptEnd = new Point(cptBottomCenter.X + (m_nSearchAreaSize / 2), cptBottomCenter.Y + (m_nSearchAreaSize / 2));
            crtSearchArea = new CRect(ptStart, ptEnd);
            imgSrc = m_module.GetGrayByteImageFromMemory_12bit(mem, crtSearchArea);
            bFoundBottom = m_module.TemplateMatching(mem, crtSearchArea, imgSrc, imgBottom, out cptBottomResultCenter, m_dMatchScore);

            // Calculate Theta
            if (bFoundTop && bFoundBottom) // Top & Bottom 모두 Template Matching 성공했을 경우
            {
                double dThetaRadian = Math.Atan2((double)(cptBottomResultCenter.Y - cptTopResultCenter.Y), (double)(cptBottomResultCenter.X - cptTopResultCenter.X));
                double dThetaDegree = dThetaRadian * (180 / Math.PI);
                dThetaDegree -= 90;

                // Rotate 축 Theta만큼 회전
                if (m_module.Run(axisRotate.StartMove(dThetaDegree * -1)))
                    return p_sInfo;
                if (m_module.Run(axisRotate.WaitReady()))
                    return p_sInfo;
            }

            return "OK";
        }
    }
}
