using RootTools;
using RootTools.Camera;
using RootTools.Control;
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

            try
            {
                int nScanLine = 0;
                int nRescanCount = 0;

                // 기계 장치 설정
                m_grabMode.SetLens();
                m_grabMode.SetLight(true);

                // 메모리 오프셋
                CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);
                cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_GD.m_nFovSize;

                int nWaferSizeY_px = Convert.ToInt32(m_grabMode.m_nWaferSize_mm * MM_PER_UM / m_grabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수

                m_grabMode.m_dTrigger = Convert.ToInt32(m_grabMode.m_dResY_um);     // 트리거 (1 pulse = 3.0 mm)

                while (m_grabMode.m_ScanLineNum > nScanLine)
                {
                    if (EQ.IsStop())
                        return "OK";

                    bool bNormal = true;

                    //ybkwon0113
                    // 위에서 아래로 찍는것을 정방향으로 함, 즉 Y축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향
                    // Grab하기 위해 이동할 Y축의 시작 끝 점
                    int nScanSpeed = Convert.ToInt32((double)m_grabMode.m_nMaxFrame * m_grabMode.m_dTrigger * (double)m_grabMode.m_nScanRate / 100);
                    int nScanOffset_pulse = (int)((double)nScanSpeed * axisXY.p_axisY.GetSpeedValue(Axis.eSpeed.Move).m_acc * 0.5)*2;
                    double dStartPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                    double dEndPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                    // Grab 방향 및 시작, 종료 위치 설정
                    m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                    if (m_grabMode.m_bUseBiDirectionScan && nScanLine % 2 == 1)
                    {
                        // dStartPosY <--> dEndPosY 바꿈.
                        (dStartPosY, dEndPosY) = (dEndPosY, dStartPosY);
                        
                        m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                    }
                    
                    double dfovum = m_grabMode.m_GD.m_nFovSize * m_grabMode.m_dResX_um * 10;
                    double dPosX = m_grabMode.m_rpAxisCenter.X + m_grabMode.m_ptXYAlignData.X + nWaferSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * dfovum;
                    double dNextPosX = m_grabMode.m_rpAxisCenter.X + m_grabMode.m_ptXYAlignData.X + nWaferSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + 1 + m_grabMode.m_ScanStartLine) * dfovum;
                    double dPosZ = m_grabMode.m_nFocusPosZ;

                    if(m_grabMode.m_dVRSFocusPos != 0)
                        dPosZ = m_grabMode.m_dVRSFocusPos + m_dTDIToVRSOffsetZ;

                    //포커스 높이로 이동
                    if (m_module.Run(axisZ.StartMove(dPosZ)))
                        return p_sInfo;

                    // XY 찍는 위치로 이동
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                    if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
        
                    if (m_module.Run(axisZ.WaitReady()))
                        return p_sInfo;

                    double dTriggerStartPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y - nTotalTriggerCount / 2;
                    double dTriggerEndPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y + nTotalTriggerCount / 2 ;
                    axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

                    GrabData gd = m_grabMode.m_GD;
                    gd.bInvY = m_grabMode.m_eGrabDirection == eGrabDirection.BackWard;
                    gd.nScanOffsetY = (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_nYOffset;

                    //카메라 그랩 시작
                    string strPool = m_grabMode.m_memoryPool.p_id;
                    string strGroup = m_grabMode.m_memoryGroup.p_id;
                    string strMemory = m_grabMode.m_memoryData.p_id;
                    MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                    m_grabMode.StartGrab(mem, cpMemoryOffset, nWaferSizeY_px, m_grabMode.m_GD);
                    
                    // Y축 트리거 발생
                    if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                        return p_sInfo;

                    // 스캔 축 타임 아웃
                    int nScanAxisTimeOut = TIMEOUT_50MS / TIMEOUT_INTERVAL;

                    // 스캔 축이 트리거 설정 레이지에 도달했으면
                    if (m_grabMode.m_eGrabDirection == eGrabDirection.Forward)
                    {
                        while (axisXY.p_axisY.p_posActual < dTriggerEndPosY)
                        {
                            Thread.Sleep(TIMEOUT_INTERVAL);
                            if (--nScanAxisTimeOut <= 0)
                            {
                                m_log.Info("TimeOut - Scan Axis Error Forward");
                                bNormal = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        while (axisXY.p_axisY.p_posActual > dTriggerStartPosY)
                        {
                            Thread.Sleep(TIMEOUT_INTERVAL);
                            if (--nScanAxisTimeOut <= 0)
                            {
                                m_log.Info("TimeOut - Scan Axis Error Reverse");
                                bNormal = false;
                                break;
                            }
                        }
                    }

                    // X축을 미리 움직임
                    axisXY.p_axisX.StartMove(dNextPosX);
                    axisXY.p_axisY.RunTrigger(false);

                    // 카메라 그랩 완료 대기
                    int nCameraTimeOut = TIMEOUT_10MS / TIMEOUT_INTERVAL;
                    while (m_grabMode.m_camera.p_nGrabProgress != 100)
                    {
                        Thread.Sleep(TIMEOUT_INTERVAL);
                        m_log.Info("Wait Camera GrabProcess");
                        if(--nCameraTimeOut <= 0)
                        {
                            m_log.Info("TimeOut Camera GrabProcess");
                            bNormal = false;
                            break;
                        }
                    }

                    // 스캔 결과에 따라 
                    if (bNormal == true)
                    {
                        //WIND2EventManager.OnSnapDone(this, new SnapDoneArgs(new CPoint(startOffsetX, startOffsetY), cpMemoryOffset + new CPoint(m_grabMode.m_GD.m_nFovSize, nWaferSizeY_px)));

                        nScanLine++;
                        cpMemoryOffset.X += m_grabMode.m_GD.m_nFovSize;
                    }
                    else //비정상 스캔일때.
                    {
                        m_grabMode.m_camera.StopGrab();
                        if (nRescanCount > RESCAN_MAX)
                        {
                            throw new Exception("Run_GrabLineScan Rescan Count Over");
                        }
                        nRescanCount++;

                        if (nScanLine > 0)
                        {
                            nScanLine--;
                            cpMemoryOffset.X -= m_grabMode.m_GD.m_nFovSize;
                        }
                    }
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
