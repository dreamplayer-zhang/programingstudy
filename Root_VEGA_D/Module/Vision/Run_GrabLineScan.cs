using Root_VEGA_D_IPU.Module;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Control.ACS;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using System;
using System.Collections.Generic;
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
            m_dTDIToVRSOffsetZ = tree.Set(m_dTDIToVRSOffsetZ, m_dTDIToVRSOffsetZ,"TDI To VRS Offset Z", "TDI To VRS Offset Z", bVisible);
            //if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
        }

        public override string Run()
        {
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

            Camera_Dalsa camera = (Camera_Dalsa)m_grabMode.m_camera;
            GrabData grabData = m_grabMode.m_GD;

            try
            {
                // 카메라 연결 시도
                camera.Connect();

                int nTimeOut = TIMEOUT_50MS / TIMEOUT_INTERVAL;
                while (camera.p_CamInfo.p_eState != eCamState.Ready)
                {
                    if(nTimeOut-- == 0)
                    {
                        throw new Exception("Camera Connect Error");
                    }
                    Thread.Sleep(TIMEOUT_INTERVAL);
                }
               
                
                int nScanLine = 0;

                // 기계 장치 설정
                //m_grabMode.SetLens();
                m_grabMode.SetLight(true);

                // 메모리 오프셋
                CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);
                cpMemoryOffset.X += m_grabMode.m_ScanStartLine * grabData.m_nFovSize;

                m_grabMode.m_dTrigger = Math.Round(m_grabMode.m_dResY_um * m_grabMode.m_dCamTriggerRatio, 1);     // 트리거 (1 pulse = 3.0 mm)

                int nWaferSizeY_px = (int)Math.Round(m_grabMode.m_nWaferSize_mm * MM_PER_UM / m_grabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수

                while (m_grabMode.m_ScanLineNum > nScanLine)
                {
                    if (EQ.IsStop())
                        return "OK";
                    
                    // 이동 위치 계산
                    int nScanSpeed = Convert.ToInt32((double)m_grabMode.m_nMaxFrame * m_grabMode.m_dTrigger * (double)m_grabMode.m_nScanRate / 100);
                    int nScanOffset_pulse = (int)((double)nScanSpeed * axisXY.p_axisY.GetSpeedValue(Axis.eSpeed.Move).m_acc * 0.5) * 2;

                    int nLineIndex = m_grabMode.m_ScanStartLine + nScanLine;

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
                    if (m_grabMode.m_bUseBiDirectionScan && nScanLine % 2 == 1)
                    {
                        // dStartPosY <--> dEndPosY 바꿈.
                        (dStartPosY, dEndPosY) = (dEndPosY, dStartPosY);
                        (dTriggerStartPosY, dTriggerEndPosY) = (dTriggerEndPosY, dTriggerStartPosY);

                        m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
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

                    // IPU PC와 연결된 상태라면 Send Message
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
                        mapParam[TCPIPComm_VEGA_D.PARAM_NAME_CURRENTSCANLINE] = nScanLine.ToString();
                        mapParam[TCPIPComm_VEGA_D.PARAM_NAME_STARTSCANLINE] = m_grabMode.m_ScanStartLine.ToString();

                        m_module.TcpipCommServer.SendMessage(TCPIPComm_VEGA_D.Command.start, mapParam);
                    }

                    //카메라 그랩 시작
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
                    m_grabMode.StartGrab(mem, tmpMemOffset, nWaferSizeY_px, grabData);

                    // Y축 트리거 발생
                    axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, 0.001, true);
                    
                    // 라인스캔 완료 대기
                    if (m_module.Run(axisXY.p_axisY.WaitReady()))
                        return p_sInfo;

                    // IPU PC와 연결된 상태라면 스캔 종료 메세지 전달
                    if (m_module.TcpipCommServer.IsConnected())
                        m_module.TcpipCommServer.SendMessage(TCPIPComm_VEGA_D.Command.end);

                    // X축을 미리 움직임
                    axisXY.p_axisX.StartMove(dNextPosX);

                    // 다음 이미지 획득을 위해 변수 값 변경
                    nScanLine++;
                    cpMemoryOffset.X += grabData.m_nFovSize;
                }
                m_grabMode.m_camera.StopGrab();

                snapTimeWatcher.Stop();

                // Log
                TempLogger.Write("Snap", string.Format("{0:F3}", (double)snapTimeWatcher.ElapsedMilliseconds / (double)1000));

                return "OK";
            }
            catch(Exception e)
            {
                m_log.Info(e.Message);
                return "OK";
            }
            finally
            {
                m_grabMode.SetLight(false);
            }
        }
    }
}
