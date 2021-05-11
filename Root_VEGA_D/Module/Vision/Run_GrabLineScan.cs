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
        double m_dTDIToVRSOffsetZ = 0;
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
            run.m_dTDIToVRSOffsetZ = m_dTDIToVRSOffsetZ;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            m_dTDIToVRSOffsetZ = tree.Set(m_dTDIToVRSOffsetZ, m_dTDIToVRSOffsetZ, "TDI To VRS Offset Z", "TDI To VRS Offset Z", bVisible);
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
                if (m_module.Run(axisZ.StartMove(m_grabMode.m_dAFEndZ, 1)))
                    return p_sInfo;
                if (m_module.Run(axisZ.WaitReady()))
                    return p_sInfo;

                camRADS.Grabed -= m_camera_Grabed;

                if (bPastGrabbingState == false)
                    camRADS.GrabStop();

                // 중앙에 RADS 레이저가 맞춰진적이 없다면
                if (m_nAFBestGVSum <= 0)
                    return "Cannot find best Y position by auto focusing";

                // Offset 적용
                m_dAFBestFocusPosY += m_grabMode.m_dAFOffset;
            }
            catch (Exception e)
            {
                m_log.Info(e.Message);
            }
            finally
            {
                m_grabMode.SetLight(false);
            }

            return "OK";
        }

        void m_camera_Grabed(object sender, System.EventArgs e)
        {
            Debug.WriteLine("m_camera_Grabed");
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
                            if (diffPast > diffNew)
                            {
                                m_dAFBestFocusPosY = curPosZ;
                                m_nAFBestGVSum = nSum;
                            }
                        }
                    }
                }
            }
        }

        string ConnectRADS()
        {
            Camera_Basler camRADS = (Camera_Basler)m_module.CamRADS;

            if (m_grabMode.pUseRADS && m_module.RADSControl.p_IsRun == false)
            {
                m_module.RADSControl.m_timer.Start();
                m_module.RADSControl.p_IsRun = true;
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

                // Offset 설정
                m_module.RADSControl.p_connect.SetADSOffset(m_grabMode.pRADSOffset);

                // RADS 카메라 설정
                camRADS.SetMulticast();
                camRADS.GrabContinuousShot();
            }

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
                if (m_module.Run(ConnectRADS()))
                    return p_sInfo;

                // 카메라 연결 시도
                if (m_module.Run(ConnectMainCam()))
                    return p_sInfo;

                // 스캔 라인 초기화
                m_nCurScanLine = 0;

                // 기계 장치 설정
                //m_grabMode.SetLens();
                m_grabMode.SetLight(true);

                // 메모리 오프셋
                CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);
                cpMemoryOffset.X += m_grabMode.m_ScanStartLine * grabData.m_nFovSize;

                m_grabMode.m_dTrigger = Math.Round(m_grabMode.m_dResY_um * m_grabMode.m_dCamTriggerRatio, 1);     // 트리거 (1 pulse = 3.0 mm)

                int nWaferSizeY_px = (int)Math.Round(m_grabMode.m_nWaferSize_mm * MM_TO_UM / m_grabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수

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

                        // 분주비 재설정
                        int nEncoderMul = camMain.GetEncoderMultiplier();
                        int nEncoderDiv = camMain.GetEncoderDivider();
                        camMain.SetEncoderMultiplier(1);
                        camMain.SetEncoderDivider(1);
                        camMain.SetEncoderMultiplier(nEncoderMul);
                        camMain.SetEncoderDivider(nEncoderDiv);

                        int nLineIndex = m_grabMode.m_ScanStartLine + m_nCurScanLine;

                        double dfov_mm = grabData.m_nFovSize * m_grabMode.m_dResX_um * 0.001;
                        double dOverlap_mm = grabData.m_nOverlap * m_grabMode.m_dResX_um * 0.001;
                        double dPosX = m_grabMode.m_rpAxisCenter.X + m_grabMode.m_nWaferSize_mm * 0.5 - nLineIndex * (dfov_mm - dOverlap_mm);
                        double dNextPosX = dPosX - (dfov_mm - dOverlap_mm);
                        double dPosZ = m_dAFBestFocusPosY;//m_grabMode.m_dFocusPosZ;

                        double dTriggerStartPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y - m_grabMode.m_nWaferSize_mm * 0.5;
                        double dTriggerEndPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y + m_grabMode.m_nWaferSize_mm * 0.5;

                        Axis.Speed speedY = axisXY.p_axisY.GetSpeedValue(Axis.eSpeed.Move);

                        double accDistance = speedY.m_acc * speedY.m_v * 0.5 * 2.0;
                        double decDistance = speedY.m_dec * speedY.m_v * 0.5 * 2.0;
                        
                        double dStartPosY, dEndPosY;

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

                        if (m_grabMode.m_dVRSFocusPos != 0)
                            dPosZ = m_grabMode.m_dVRSFocusPos + m_dTDIToVRSOffsetZ;

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

                        // IPU PC와 연결된 상태라면 'LineStart' 메세지 전달
                        if (m_module.TcpipCommServer.IsConnected())
                        {
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
                        }

                        // 카메라 그랩 시작
                        string strPool = m_grabMode.m_memoryPool.p_id;
                        string strGroup = m_grabMode.m_memoryGroup.p_id;
                        string strMemory = m_grabMode.m_memoryData.p_id;
                        MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

                        CPoint tmpMemOffset = new CPoint(cpMemoryOffset);
                        if (m_module.TcpipCommServer.IsConnected())
                        {
                            // IPU PC와 연결된 상태에서는 이미지 데이터가 복사될 Main PC의 Memory 위치가
                            // Memory Width를 넘어가게 되면 다시 0부터 이미지를 얻어오도록 Memory Offset을 계산
                            long div = tmpMemOffset.X / mem.W;
                            long remain = tmpMemOffset.X - mem.W * div;
                            long offset = remain % grabData.m_nFovSize;
                            tmpMemOffset.X = (int)(remain - offset);
                        }

                        // Y축 트리거 발생 설정
                        axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, 0.001, true);

                        // 카메라 스냅 시작
                        m_grabMode.StartGrab(mem, tmpMemOffset, (int)(nWaferSizeY_px * 0.98), grabData);
                        
                        // Y축 목표 위치로 이동
                        if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY)))
                            return p_sInfo;

                        //// 라인스캔 완료 대기
                        if (m_module.Run(axisXY.p_axisY.WaitReady()))
                            return p_sInfo;

                        // 이미지 스냅 스레드 동작중이라면 중지
                        Camera_Dalsa dalsaCam = m_grabMode.m_camera as Camera_Dalsa;
                        if (dalsaCam == null)
                            return "OK";
                        else
                        {
                            while (dalsaCam.p_CamInfo.p_eState != eCamState.Ready && !EQ.IsStop())
                            {
                                Thread.Sleep(10);
                            }
                        }

                        // IPU PC와 연결된 상태라면 'LineEnd' 메세지 전달
                        if (m_module.TcpipCommServer.IsConnected())
                            m_module.TcpipCommServer.SendMessage(TCPIPComm_VEGA_D.Command.LineEnd);

                        // 다음 이미지 획득을 위해 변수 값 변경
                        m_nCurScanLine++;
                        cpMemoryOffset.X += grabData.m_nFovSize;
                    }
                }
                m_grabMode.m_camera.StopGrab();

                snapTimeWatcher.Stop();

                // IPU와 연결상태에 이미지 검사가 끝나기까지 대기
                while (m_module.TcpipCommServer.IsConnected() && !m_bIPUCompleted && !EQ.IsStop())
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
                if (m_grabMode.pUseRADS && m_module.RADSControl.p_IsRun == true)
                {
                    m_module.RADSControl.m_timer.Stop();
                    m_module.RADSControl.p_IsRun = false;
                    m_module.RADSControl.StopRADS();
                    if (camRADS.p_CamInfo._IsGrabbing == true) camRADS.StopGrab();
                }
            }
        }
    }
}
