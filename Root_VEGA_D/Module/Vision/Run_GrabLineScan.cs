using Emgu.CV;
using Emgu.CV.Structure;
using Root_VEGA_D_IPU.Module;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Control.ACS;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_VEGA_D.Module
{
    public class Run_GrabLineScan : ModuleRunBase
    {
        Vision m_module;

        //bool m_bInvDir = false;
        public GrabMode m_grabMode = null;
        //double m_dTDIToVRSOffsetZ = 0;
        string m_sGrabMode = "";
        public bool m_bIPUCompleted = true;
        public int m_nCurScanLine = 0;
        public bool m_bWaitRun = false;
        public object m_lockWaitRun = new object();
        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }

        public Run_GrabLineScan(Vision module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_GrabLineScan run = new Run_GrabLineScan(m_module);
            run.p_sGrabMode = p_sGrabMode;
            run.m_bIPUCompleted = m_bIPUCompleted;
            run.m_nCurScanLine = m_nCurScanLine;
            run.m_bWaitRun = m_bWaitRun;
            //run.m_dTDIToVRSOffsetZ = m_dTDIToVRSOffsetZ;

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            //m_dTDIToVRSOffsetZ = tree.Set(m_dTDIToVRSOffsetZ, m_dTDIToVRSOffsetZ, "TDI To VRS Offset Z", "TDI To VRS Offset Z", bVisible);
            //if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
        }


        double m_dAFBestFocusPosY;
        int m_nAFBestGVSum;
        string RunAutoFocus()
        {
            if (!m_grabMode.m_bUseAF)
            {
                m_dAFBestFocusPosY = m_grabMode.m_dFocusPosZ;
                return "OK";
            }

            if (m_grabMode.m_nAFLaserThreshold <= 0)
                return "Auto focus Threadhold value must be bigger than zero.";

            Camera_Basler camRADS = (Camera_Basler)m_module.CamRADS;
            AxisXY axisXY = m_module.AxisXY;
            Axis axisZ = m_module.AxisZ;

            m_grabMode.SetLight(true);

            try
            {
                if (camRADS.p_CamInfo._OpenStatus == false) camRADS.Connect();

                int nTryCount = 0;
                while(nTryCount < m_grabMode.m_nRetryCount)
                {
                    // RADS Voltage Reset
                    m_module.RADSControl.ResetController();

                    // 레티클 중심 XY위치, Z축 시작위치로 이동
                    if (m_module.Run(axisXY.StartMove(m_grabMode.m_rpAxisCenter.X, m_grabMode.m_rpAxisCenter.Y)))
                        return p_sInfo;
                    if (m_module.Run(axisZ.StartMove(m_grabMode.m_dAFStartZ)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                    if (m_module.Run(axisZ.WaitReady()))
                        return p_sInfo;

                    // 중앙 위치의 GV 합 리셋
                    m_nAFBestGVSum = 0;

                    camRADS.Grabed += m_camera_Grabed;

                    bool bPastGrabbingState = camRADS.p_CamInfo._IsGrabbing;
                    if (bPastGrabbingState == false)
                        camRADS.GrabContinuousShot();

                    // Z축 목표위치로 이동
                    if (m_module.Run(axisZ.StartMove(m_grabMode.m_dAFEndZ, m_grabMode.m_dAFSearchSpeed)))
                        return p_sInfo;
                    if (m_module.Run(axisZ.WaitReady()))
                        return p_sInfo;

                    camRADS.Grabed -= m_camera_Grabed;

                    if (bPastGrabbingState == false)
                        camRADS.GrabStop();

                    // 중앙에 RADS 레이저가 맞춰진적이 없다면
                    if (m_nAFBestGVSum <= 0)
                    {
                        m_log.Info(string.Format("AutoFocus Try ({0}/{1}) - Cannot find best Y position by auto focusing", nTryCount, m_grabMode.m_nRetryCount));
                        nTryCount++;
                    }
                    else
                        break;
                }

                // Offset 적용
                if (m_nAFBestGVSum <= 0)
                    m_dAFBestFocusPosY = m_grabMode.m_dFocusPosZ;
                else
                    m_dAFBestFocusPosY += m_grabMode.m_dAFOffset;

                // 포커스 위치로 이동
                if (m_module.Run(axisZ.StartMove(m_dAFBestFocusPosY)))
                    return p_sInfo;
                if (m_module.Run(axisZ.WaitReady()))
                    return p_sInfo;
            }
            catch (Exception e)
            {
                m_log.Info(e.Message);
            }
            finally
            {
                m_grabMode.SetLight(false);

                if(m_nAFBestGVSum <= 0)
                    m_log.Info(string.Format("AutoFocus is failed, Z Pos is set to {0}", m_dAFBestFocusPosY));
                else
                    m_log.Info(string.Format("AutoFocus is successful, Z Pos is set to {0}", m_dAFBestFocusPosY));
            }

            return "OK";
        }

        void m_camera_Grabed(object sender, System.EventArgs e)
        {
            Camera_Basler camRADS = m_module.CamRADS;
            IntPtr intPtr = camRADS.p_ImageData.GetPtr();  // R 채널 데이터

            unsafe
            {
                CPoint size = camRADS.p_ImageData.p_Size;
                byte* arrImg = (byte*)intPtr.ToPointer();
                if (arrImg != null && camRADS != null)
                {
                    int[] profile = new int[size.Y];

                    // 각 행별로 모든 GV값 더하기
                    Parallel.For(0, size.Y, (y) =>
                    {
                        for (int x = 0; x < size.X; x++)
                        {
                            int pixel = arrImg[size.X * y + x];
                            if (pixel >= m_grabMode.m_nAFLaserThreshold)     // 설정된 Threshold 이상의 값만 계산에 사용
                                profile[y] += pixel;
                        }
                    });

                    // RADS laser의 위치 찾기
                    int laserY = 0;
                    int nSum = profile[0];
                    for (int i = 1; i < size.Y; i++)
                    {
                        if (nSum <= profile[i])
                        {
                            laserY = i;
                            nSum = profile[i];
                        }
                    }

                    // 이전 RADS laser 정보와 비교하여 중심에 가까울 때의 Z축 위치 찾기
                    int nCenterOnImg = size.Y / 2;
                    Axis axisZ = m_module.AxisZ;
                    ACSAxis acsAxisZ = axisZ as ACSAxis;
                    if (acsAxisZ != null)
                    {
                        if (nSum > m_grabMode.m_nAFLaserThreshold)
                        {
                            double curPosZ = acsAxisZ.GetActualPosition();
                            double diffPast = Math.Abs(nCenterOnImg - m_dAFBestFocusPosY);
                            double diffNew = Math.Abs(nCenterOnImg - curPosZ);

                            // 새로 발견한 위치가 중심에 더 가까울 경우
                            if (diffPast > diffNew && nSum > m_nAFBestGVSum)
                            {
                                m_dAFBestFocusPosY = curPosZ;
                                m_nAFBestGVSum = nSum;
                            }
                        }
                    }
                }
            }
        }

        string StartRADS()
        {
            if (m_grabMode.pUseRADS && m_module.RADSControl.p_IsRun == false)
                return m_module.StartRADS(m_grabMode.pRADSOffset);

            return "OK";
        }

        string StopRADS()
        {
            if (m_grabMode.pUseRADS && m_module.RADSControl.p_IsRun == true)
                return m_module.StopRADS();

            return "OK";
        }

        string ConnectMainCam()
        {
            const int TIMEOUT_50MS = 50000;     // ms
            const int TIMEOUT_INTERVAL = 10;    // ms

            Camera_Dalsa camMain = (Camera_Dalsa)m_grabMode.m_camera;

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

            return "OK";
        }
        string RunAlign(MemoryData mem, int nSnapCount, double startPosY, double endPosY, double startTriggerY, double endTriggerY, out CRect rectBotMarker)
        {
            rectBotMarker = new CRect(0, 0, 0, 0);

            if (m_grabMode.m_bUseAlign == false)
                return "OK";

            m_grabMode.SetLight(true);

            m_log.Info("Align Start");

            try
            {
                double dPosX = m_grabMode.m_rpAxisCenter.X + m_grabMode.m_nWaferSize_mm * 0.5 - (m_grabMode.m_nCenterX - m_grabMode.m_GD.m_nFovSize * 0.5) * m_grabMode.m_dResX_um * 0.001;

                CPoint memOffset = new CPoint(0, 0);
                if (m_module.Run(m_module.RunLineScan(m_grabMode, mem, memOffset, nSnapCount, dPosX, startPosY, endPosY, startTriggerY, endTriggerY)))
                    return p_sInfo;

                Image<Gray, byte> imgTop = new Image<Gray, byte>(m_grabMode.p_sTopTemplateFile);
                Image<Gray, byte> imgBot = new Image<Gray, byte>(m_grabMode.p_sBottomTemplateFile);

                CRect searchTopArea = new CRect(0, (int)(m_grabMode.m_nTopCenterY - m_grabMode.m_nSearchAreaSize * 0.5), m_grabMode.m_GD.m_nFovSize, (int)(m_grabMode.m_nTopCenterY + m_grabMode.m_nSearchAreaSize * 0.5));
                CRect searchBotArea = new CRect(0, (int)(m_grabMode.m_nBottomCenterY - m_grabMode.m_nSearchAreaSize * 0.5), m_grabMode.m_GD.m_nFovSize, (int)(m_grabMode.m_nBottomCenterY + m_grabMode.m_nSearchAreaSize * 0.5));
                Image<Gray, byte> imgTopArea = m_module.GetGrayByteImageFromMemory_12bit(mem, searchTopArea);
                Image<Gray, byte> imgBotArea = m_module.GetGrayByteImageFromMemory_12bit(mem, searchBotArea);

                Image<Gray, byte> imgTop_div4 = imgTop.Resize(0.25, Emgu.CV.CvEnum.Inter.Linear);
                Image<Gray, byte> imgBot_div4 = imgBot.Resize(0.25, Emgu.CV.CvEnum.Inter.Linear);
                Image<Gray, byte> imgTopArea_div4 = imgTopArea.Resize(0.25, Emgu.CV.CvEnum.Inter.Linear);
                Image<Gray, byte> imgBotArea_div4 = imgBotArea.Resize(0.25, Emgu.CV.CvEnum.Inter.Linear);

                // 1/16 다운스케일 이미지로 템플릿 매칭
                CPoint ptCenterTop = new CPoint();
                CPoint ptCenterBot = new CPoint();
                bool bTopResult = TemplateMatch(imgTopArea_div4, imgTop_div4, m_grabMode.m_dMatchScore, out ptCenterTop);
                bool bBotResult = TemplateMatch(imgBotArea_div4, imgBot_div4, m_grabMode.m_dMatchScore, out ptCenterBot);
                if (bTopResult && bBotResult)
                {
                    ptCenterTop.X *= 4;
                    ptCenterTop.Y *= 4;
                    ptCenterBot.X *= 4;
                    ptCenterBot.Y *= 4;

                    searchTopArea = new CRect(new CPoint(searchTopArea.Left + ptCenterTop.X, searchTopArea.Top + ptCenterTop.Y), imgTop.Width + 8, imgTop.Height + 8);
                    searchBotArea = new CRect(new CPoint(searchBotArea.Left + ptCenterBot.X, searchBotArea.Top + ptCenterBot.Y), imgBot.Width + 8, imgBot.Height + 8);

                    imgTopArea = m_module.GetGrayByteImageFromMemory_12bit(mem, searchTopArea);
                    imgBotArea = m_module.GetGrayByteImageFromMemory_12bit(mem, searchBotArea);

                    // 좁혀진 영역에서 다시 템플릿 매칭
                    bTopResult = TemplateMatch(imgTopArea, imgTop, m_grabMode.m_dMatchScore, out ptCenterTop);
                    bBotResult = TemplateMatch(imgBotArea, imgBot, m_grabMode.m_dMatchScore, out ptCenterBot);

                    if (bTopResult && bBotResult)
                    {
                        ptCenterTop = new CPoint(searchTopArea.Left + ptCenterTop.X, searchTopArea.Top + ptCenterTop.Y);
                        ptCenterBot = new CPoint(searchBotArea.Left + ptCenterBot.X, searchBotArea.Top + ptCenterBot.Y);

                        double dThetaRadian = Math.Atan2((double)(ptCenterBot.Y - ptCenterTop.Y), (double)(ptCenterBot.X - ptCenterTop.X));
                        double dThetaDegree = dThetaRadian * (180 / Math.PI);
                        dThetaDegree -= 90;

                        // Rotate 축 Theta만큼 회전
                        Axis axisRotate = m_module.AxisRotate;
                        if (m_module.Run(axisRotate.StartMove(axisRotate.p_posActual + dThetaDegree * -1, 0.05)))
                            return p_sInfo;
                        if (m_module.Run(axisRotate.WaitReady()))
                            return p_sInfo;

                        // IPU 연결된 상태라면 좌하단 Align Marker 위치 전달을 위해 찾아야함
                        if (m_module.TcpipCommServer.IsConnected())
                        {
                            if (m_module.Run(m_module.RunLineScan(m_grabMode, mem, memOffset, nSnapCount, dPosX, startPosY, endPosY, startTriggerY, endTriggerY)))
                                return p_sInfo;

                            searchBotArea = new CRect(0, (int)(m_grabMode.m_nBottomCenterY - m_grabMode.m_nSearchAreaSize * 0.5), m_grabMode.m_GD.m_nFovSize, (int)(m_grabMode.m_nBottomCenterY + m_grabMode.m_nSearchAreaSize * 0.5));
                            imgBotArea = m_module.GetGrayByteImageFromMemory_12bit(mem, searchBotArea);
                            imgBotArea_div4 = imgBotArea.Resize(0.25, Emgu.CV.CvEnum.Inter.Linear);

                            ptCenterBot = new CPoint();
                            bBotResult = TemplateMatch(imgBotArea_div4, imgBot_div4, m_grabMode.m_dMatchScore, out ptCenterBot);

                            if (!bBotResult)
                                return "Cannot get Bottom Align Marker Position";

                            int botMarkerX = (int)(0 + m_grabMode.m_nCenterX - m_grabMode.m_GD.m_nFovSize * 0.5 + (ptCenterBot.X - imgBot_div4.Width * 0.5) * 4 - 4);
                            int botMarkerY = (int)(m_grabMode.m_nBottomCenterY - m_grabMode.m_nSearchAreaSize * 0.5 + (ptCenterBot.Y - imgBot_div4.Height * 0.5) * 4 - 4);

                            ptCenterBot.X *= 4;
                            ptCenterBot.Y *= 4;

                            searchBotArea = new CRect(new CPoint(searchBotArea.Left + ptCenterBot.X, searchBotArea.Top + ptCenterBot.Y), imgBot.Width + 8, imgBot.Height + 8);

                            imgBotArea = m_module.GetGrayByteImageFromMemory_12bit(mem, searchBotArea);

                            bBotResult = TemplateMatch(imgBotArea, imgBot, m_grabMode.m_dMatchScore, out ptCenterBot);

                            if (!bBotResult)
                                return "Cannot get Bottom Align Marker Position";

                            botMarkerX += (int)(ptCenterBot.X - imgBot.Width * 0.5);
                            botMarkerY += (int)(ptCenterBot.Y - imgBot.Height * 0.5);

                            rectBotMarker.Left = botMarkerX;
                            rectBotMarker.Top = botMarkerY;
                            rectBotMarker.Right = rectBotMarker.Left + imgBot.Width;
                            rectBotMarker.Bottom = rectBotMarker.Top + imgBot.Height;

                            m_log.Info(string.Format("LeftBottom Align Marker is found - {0}, {1}", botMarkerX, botMarkerY));
                        }

                        m_log.Info(string.Format("Align Success, theta difference = {0}", dThetaDegree * -1));

                        return "OK";
                    }
                }
            }
            catch (Exception ex)
            {
                m_log.Info(ex.Message);
            }
            finally
            {
                m_grabMode.SetLight(false);
            }

            m_log.Info("Align Failed");

            return "Align Failed";
        }
        bool TemplateMatch(Image<Gray, byte> imgTargetArea, Image<Gray, byte> imgTemplate, double dMatchScore, out CPoint ptResult)
        {
            int nWidthDiff = 0;
            int nHeightDiff = 0;
            System.Windows.Point ptMaxRelative = new System.Windows.Point();
            float fMaxScore = float.MinValue;
            bool bFoundTemplate = false;

            Image<Gray, float> imgResult = imgTargetArea.MatchTemplate(imgTemplate, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);
            nWidthDiff = imgTargetArea.Width - imgResult.Width;
            nHeightDiff = imgTargetArea.Height - imgResult.Height;
            float[,,] matches = imgResult.Data;

            for (int x = 0; x < matches.GetLength(1); x++)
            {
                for (int y = 0; y < matches.GetLength(0); y++)
                {
                    if (fMaxScore < matches[y, x, 0] && dMatchScore <= matches[y, x, 0])
                    {
                        fMaxScore = matches[y, x, 0];
                        ptMaxRelative.X = x;
                        ptMaxRelative.Y = y;
                        bFoundTemplate = true;
                    }
                }
            }
            ptResult = new CPoint();
            ptResult.X = (int)(ptMaxRelative.X) + (int)(nWidthDiff / 2);
            ptResult.Y = (int)(ptMaxRelative.Y) + (int)(nHeightDiff / 2);

            return bFoundTemplate;
        }

        bool m_bContinuousConnectedIPU = false;
        bool CheckIPUConnectStatus()
        {
            if (m_bContinuousConnectedIPU)
            {
                if(m_module.TcpipCommServer.IsConnected())
                    return true;
                else
                {
                    m_bContinuousConnectedIPU = false;
                    return false;
                }
            }
            else
                return false;
        }

        public override string Run()
        {
            if (m_grabMode == null) return "Grab Mode == null";

            // StopWatch 설정
            StopWatch snapTimeWatcher = new StopWatch();
            snapTimeWatcher.Start();

            // 상수 값
            int MM_TO_UM = 1000;

            // 축 가져오기
            AxisXY axisXY = m_module.AxisXY;
            Axis axisZ = m_module.AxisZ;
            Axis axisRotate = m_module.AxisRotate;

            Camera_Dalsa camMain = (Camera_Dalsa)m_grabMode.m_camera;
            Camera_Basler camRADS = (Camera_Basler)m_module.CamRADS;
            GrabData grabData = m_grabMode.m_GD;

            // IPU와 연결 시, 이미지 검사 완료되었을 때의 변수
            m_bIPUCompleted = false;

            try
            {
                // Z축 Auto Focus
                if (m_module.Run(RunAutoFocus()))
                    return p_sInfo;

                // RADS 연결
                if (m_module.Run(StartRADS()))
                    return p_sInfo;

                // 카메라 연결 시도
                if (m_module.Run(ConnectMainCam()))
                    return p_sInfo;

                // 스캔 라인 초기화
                m_nCurScanLine = 0;

                // 메모리 오프셋
                CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);
                cpMemoryOffset.X += m_grabMode.m_ScanStartLine * grabData.m_nFovSize;

                // 변수 계산
                m_grabMode.m_dTrigger = Math.Round(m_grabMode.m_dResY_um * m_grabMode.m_dCamTriggerRatio, 1);     // 트리거 (1 pulse = 3.0 mm)

                int nWaferSizeY_px = (int)Math.Round(m_grabMode.m_nWaferSize_mm * MM_TO_UM / m_grabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수

                double dfov_mm = grabData.m_nFovSize * m_grabMode.m_dResX_um * 0.001;
                double dOverlap_mm = grabData.m_nOverlap * m_grabMode.m_dResX_um * 0.001;
                double dPosZ = m_dAFBestFocusPosY;//m_grabMode.m_dFocusPosZ;

                double dTriggerStartPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y - m_grabMode.m_nWaferSize_mm * 0.5;
                double dTriggerEndPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y + m_grabMode.m_nWaferSize_mm * 0.5;

                Axis.Speed speedY = axisXY.p_axisY.GetSpeedValue(Axis.eSpeed.Move);

                double accDistance = speedY.m_acc * speedY.m_v * 0.5 * 2.0;
                double decDistance = speedY.m_dec * speedY.m_v * 0.5 * 2.0;

                double dStartPosY = dTriggerStartPosY - accDistance;
                double dEndPosY = dTriggerEndPosY + decDistance;

                string strPool = m_grabMode.m_memoryPool.p_id;
                string strGroup = m_grabMode.m_memoryGroup.p_id;
                string strMemory = m_grabMode.m_memoryData.p_id;
                MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

                // 얼라인
                CRect rectBotMarker = new CRect(0, 0, 0, 0);
                if (m_module.Run(RunAlign(mem, nWaferSizeY_px, dStartPosY, dEndPosY, dTriggerStartPosY, dTriggerEndPosY, out rectBotMarker)))
                    return p_sInfo;

                // IPU 접속 대기
                while (m_bWaitRun && !EQ.IsStop())
                {
                    Thread.Sleep(10);
                }

                // IPU에 Bottom Align Marker Position 전달
                if (m_module.TcpipCommServer.IsConnected())
                {
                    // 'LineStart' 메세지 전달
                    Dictionary<string, string> mapParam = new Dictionary<string, string>();
                    mapParam["BOT_ALIGN_MARKER_POS_X"] = rectBotMarker.Left.ToString();
                    mapParam["BOT_ALIGN_MARKER_POS_Y"] = rectBotMarker.Bottom.ToString();

                    m_module.TcpipCommServer.SendMessage(TCPIPComm_VEGA_D.Command.RcpName, mapParam);
                }

                // 기계 장치 설정
                //m_grabMode.SetLens();
                m_grabMode.SetLight(true);

                while (m_grabMode.m_ScanLineNum > m_nCurScanLine)
                {
                    if (EQ.IsStop())
                        return "OK";

                    lock (m_lockWaitRun)
                    {
                        if (m_bWaitRun)
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        // 이동 위치 계산
                        //int nScanSpeed = Convert.ToInt32((double)m_grabMode.m_nMaxFrame * m_grabMode.m_dTrigger * (double)m_grabMode.m_nScanRate / 100);
                        //int nScanOffset_pulse = (int)((double)nScanSpeed * axisXY.p_axisY.GetSpeedValue(Axis.eSpeed.Move).m_acc * 0.5) * 2;

                        int nLineIndex = m_grabMode.m_ScanStartLine + m_nCurScanLine;

                        double dPosX = m_grabMode.m_rpAxisCenter.X + m_grabMode.m_nWaferSize_mm * 0.5 - nLineIndex * (dfov_mm - dOverlap_mm);
                        double dNextPosX = dPosX - (dfov_mm - dOverlap_mm);

                        // Grab 방향 및 시작, 종료 위치 설정
                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                        if (m_grabMode.m_bUseBiDirectionScan && m_nCurScanLine % 2 == 1)
                        {
                            // 역방향 (아래 -> 위)
                            (dTriggerStartPosY, dTriggerEndPosY) = (dTriggerEndPosY, dTriggerStartPosY);

                            dStartPosY = dTriggerStartPosY + accDistance;
                            dEndPosY = dTriggerEndPosY - decDistance;
                            
                            m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                        }
                        else
                        {
                            // 정방향 (위 -> 아래)
                            dStartPosY = dTriggerStartPosY - accDistance;
                            dEndPosY = dTriggerEndPosY + decDistance;
                        }

                        //if (m_grabMode.m_dVRSFocusPos != 0)
                        //    dPosZ = m_grabMode.m_dVRSFocusPos + m_dTDIToVRSOffsetZ;

                        //포커스 높이로 Z축 이동
                        if (m_module.Run(axisZ.StartMove(dPosZ)))
                            return p_sInfo;

                        // 이동 대기
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        grabData.bInvY = m_grabMode.m_eGrabDirection == eGrabDirection.BackWard;
                        grabData.nScanOffsetY = nLineIndex * m_grabMode.m_nYOffset;
                        
                        CPoint tmpMemOffset = new CPoint(cpMemoryOffset);

                        // IPU PC와 연결된 상태라면
                        m_bContinuousConnectedIPU = m_module.TcpipCommServer.IsConnected();
                        if (CheckIPUConnectStatus())
                        {
                            // 'LineStart' 메세지 전달
                            Dictionary<string, string> mapParam = new Dictionary<string, string>();
                            mapParam[TCPIPComm_VEGA_D.PARAM_NAME_OFFSETX] = cpMemoryOffset.X.ToString();
                            mapParam[TCPIPComm_VEGA_D.PARAM_NAME_OFFSETY] = cpMemoryOffset.Y.ToString();
                            mapParam[TCPIPComm_VEGA_D.PARAM_NAME_SCANDIR] = true.ToString();
                            mapParam[TCPIPComm_VEGA_D.PARAM_NAME_FOV] = grabData.m_nFovSize.ToString();
                            mapParam[TCPIPComm_VEGA_D.PARAM_NAME_OVERLAP] = grabData.m_nOverlap.ToString();
                            mapParam[TCPIPComm_VEGA_D.PARAM_NAME_LINE] = nWaferSizeY_px.ToString();
                            mapParam[TCPIPComm_VEGA_D.PARAM_NAME_TOTALSCANLINECOUNT] = m_grabMode.m_ScanLineNum.ToString();
                            mapParam[TCPIPComm_VEGA_D.PARAM_NAME_CURRENTSCANLINE] = m_nCurScanLine.ToString();
                            mapParam[TCPIPComm_VEGA_D.PARAM_NAME_STARTSCANLINE] = m_grabMode.m_ScanStartLine.ToString();

                            m_module.TcpipCommServer.SendMessage(TCPIPComm_VEGA_D.Command.LineStart, mapParam);

                            // IPU PC와 연결된 상태에서는 이미지 데이터가 복사될 Main PC의 Memory 위치가
                            // Memory Width를 넘어가게 되면 다시 0부터 이미지를 얻어오도록 Memory Offset을 계산
                            long div = tmpMemOffset.X / mem.W;
                            long remain = tmpMemOffset.X - mem.W * div;
                            long offset = remain % grabData.m_nFovSize;
                            tmpMemOffset.X = (int)(remain - offset);
                        }

                        // 라인 스캔
                        if (m_module.Run(m_module.RunLineScan(m_grabMode, mem, tmpMemOffset, nWaferSizeY_px, dPosX, dStartPosY, dEndPosY, dTriggerStartPosY, dTriggerEndPosY)))
                            return p_sInfo;

                        // IPU PC와 연결된 상태라면 'LineEnd' 메세지 전달
                        if (CheckIPUConnectStatus())
                        {
                            Thread.Sleep(3000); // IPU에서 검사로 인한 이미지 그랩 지연을 감안하여 LineEnd 메세지를 3초 후에 전달

                            m_module.TcpipCommServer.SendMessage(TCPIPComm_VEGA_D.Command.LineEnd);
                        }

                        // 다음 이미지 획득을 위해 변수 값 변경
                        m_nCurScanLine++;
                        cpMemoryOffset.X += grabData.m_nFovSize;
                    }
                }
                m_grabMode.m_camera.StopGrab();

                snapTimeWatcher.Stop();

                // IPU와 연결상태에 이미지 검사가 끝나기까지 대기
                while (CheckIPUConnectStatus() && !m_bIPUCompleted && !EQ.IsStop())
                {
                    Thread.Sleep(10);
                }

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
                StopRADS();
            }
        }
    }
}
