﻿using RootTools;
using RootTools.Camera;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using RootTools_Vision.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Root_WIND2.Module
{
    public class Run_Inspect : ModuleRunBase
    {
        Vision m_module;

        string m_sRecipeName = string.Empty;

        double m_dTDIToVRSOffsetZ = 0;

        // Grab 관련 파라매터 (이거 나중에 구조 변경 필요할듯)
        //bool m_bInvDir = false;
        public GrabModeFront m_grabMode = null;
        string m_sGrabMode = "";

        #region [Getter Setter]
        public string RecipeName 
        { 
            get => m_sRecipeName;
            set => m_sRecipeName = value;
        }

        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }
        #endregion

        public Run_Inspect(Vision module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_Inspect run = new Run_Inspect(m_module);
            run.p_sGrabMode = p_sGrabMode;
            run.RecipeName = this.RecipeName;
            run.m_dTDIToVRSOffsetZ = m_dTDIToVRSOffsetZ;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_sRecipeName = tree.SetFile(m_sRecipeName, m_sRecipeName, "rcp", "Recipe", "Recipe Name", bVisible);
            // 이거 다 셋팅 되어 있는거 가져와야함
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            m_dTDIToVRSOffsetZ = tree.Set(m_dTDIToVRSOffsetZ, m_dTDIToVRSOffsetZ, "TDI To VRS Offset Z", "TDI To VRS Offset Z", bVisible);
        }

        public override string Run()
        {
            StopWatch inspectionTimeWatcher = new StopWatch();
            inspectionTimeWatcher.Start();

            //레시피에 GrabMode 저장하고 있어야함

            RootTools_Vision.WorkManager3.WorkManager workManager = GlobalObjects.Instance.GetNamed<RootTools_Vision.WorkManager3.WorkManager>("frontInspection");
            if(workManager == null)
            {
                throw new ArgumentException("WorkManager가 초기화되지 않았습니다(null)");
            }
            workManager.Stop();

            //InspectionManagerFrontside inspectionFront = GlobalObjects.Instance.Get<InspectionManagerFrontside>();
            //inspectionFront.Stop();

            if (m_grabMode == null) return "Grab Mode == null";

            if (EQ.IsStop() == false)
            {
                if (workManager.OpenRecipe(m_sRecipeName) == false)
                    return "Recipe Open Fail";

                workManager.Start(false);

            }
            else
            {
                workManager.Stop();
            }

//#define TEST_ONLY_INSPECTION
//#if TEST_ONLY_INSPECTION

            //ImageData frontImage = GlobalObjects.Instance.GetNamed<ImageData>("FrontImage");
            //frontImage.ClearImage();
            try
            {
                m_grabMode.SetLens();
                m_grabMode.SetLight(true);

                AxisXY axisXY = m_module.AxisXY;
                Axis axisZ = m_module.AxisZ;
                Axis axisRotate = m_module.AxisRotate;
                CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);

                int nScanLine = 0;
                int nMMPerUM = 1000;

                double dXScale = m_grabMode.m_dTargetResX_um * 10;
                cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_GD.m_nFovSize;
                m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_grabMode.m_dTargetResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                int nWaferSizeY_px = Convert.ToInt32(m_grabMode.m_nWaferSize_mm * nMMPerUM / m_grabMode.m_dTargetResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수
                int nScanOffset_pulse = 40000;

                int nWaferSizeX_px = Convert.ToInt32(m_grabMode.m_nWaferSize_mm * nMMPerUM / m_grabMode.m_dTargetResX_um);
                int nWholeWaferScanLineNumber = (int)Math.Ceiling((double)nWaferSizeX_px / m_grabMode.m_GD.m_nFovSize);

                int startOffsetX = cpMemoryOffset.X;
                int startOffsetY = cpMemoryOffset.Y;
                //int startOffsetY = 0;
                const int nTimeOut_10s = 10000; //ms
                const int nTimeOut_50s = 50000; //ms
                const int nTimeOutInterval = 10; // ms
                int nRescanCount = 0;
                const int nRescanTotal = 3;
                while (nWholeWaferScanLineNumber > nScanLine)
                {
                    if (EQ.IsStop())
                        return "OK";

                    bool bNormal = true;
                    // 위에서 아래로 찍는것을 정방향으로 함, 즉 Y축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향
                    // Grab하기 위해 이동할 Y축의 시작 끝 점
                    //ybkwon0113
                    int nScanSpeed = Convert.ToInt32((double)m_grabMode.m_nMaxFrame * m_grabMode.m_dTrigger * (double)m_grabMode.m_nScanRate / 100);
                    nScanOffset_pulse = (int)((double)nScanSpeed * axisXY.p_axisY.GetSpeedValue(Axis.eSpeed.Move).m_acc * 0.5) * 2;
                    double dStartPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                    double dEndPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                    m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                    if (m_grabMode.m_bUseBiDirectionScan && nScanLine % 2 == 1)
                    {
                        double dTemp = dStartPosY;  // dStartPosY <--> dEndPosY 바꿈.
                        dStartPosY = dEndPosY;
                        dEndPosY = dTemp;
                        m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                    }
                    double dfovum = m_grabMode.m_GD.m_nFovSize * dXScale;
                    double dPosX = m_grabMode.m_rpAxisCenter.X + m_grabMode.m_ptXYAlignData.X + nWaferSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_GD.m_nFovSize * dXScale;
                    double dNextPosX = m_grabMode.m_rpAxisCenter.X + m_grabMode.m_ptXYAlignData.X + nWaferSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + 1 + m_grabMode.m_ScanStartLine) * m_grabMode.m_GD.m_nFovSize * dXScale;

                    double dPosZ = m_grabMode.m_nFocusPosZ;
                    if (m_grabMode.m_dVRSFocusPos != 0)
                    {
                        dPosZ = m_grabMode.m_dVRSFocusPos + m_dTDIToVRSOffsetZ;
                    }
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
                    double dTriggerEndPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y + nTotalTriggerCount / 2;

                    axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

                    string strPool = m_grabMode.m_memoryPool.p_id;
                    string strGroup = m_grabMode.m_memoryGroup.p_id;
                    string strMemory = m_grabMode.m_memoryData.p_id;

                    MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

                    GrabData gd = m_grabMode.m_GD;
                    gd.bInvY = m_grabMode.m_eGrabDirection == eGrabDirection.BackWard;
                    gd.nScanOffsetY = (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_nYOffset;
                    gd.ReverseOffsetY = m_grabMode.m_nReverseOffsetY;
                    //카메라 그랩 시작
                    m_grabMode.StartGrab(mem, cpMemoryOffset, nWaferSizeY_px, m_grabMode.m_GD);
                    //Y축 트리거 발생
                    if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                        return p_sInfo;

                    //스캔 축 타임 아웃
                    int nScanAxisTimeOut = nTimeOut_50s / nTimeOutInterval;
                    // 스캔 축이 트리거 설정 레이지에 도달했으면
                    if (m_grabMode.m_eGrabDirection == eGrabDirection.Forward)
                    {
                        while (axisXY.p_axisY.p_posActual < dTriggerEndPosY)
                        {
                            System.Threading.Thread.Sleep(nTimeOutInterval);
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
                            System.Threading.Thread.Sleep(nTimeOutInterval);
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
                    int nCameraTimeOut = nTimeOut_10s / nTimeOutInterval;
                    while (m_grabMode.m_camera.p_nGrabProgress != 100)
                    {
                        System.Threading.Thread.Sleep(nTimeOutInterval);
                        m_log.Info("Wait Camera GrabProcess");
                        if (--nCameraTimeOut <= 0)
                        {
                            m_log.Info("TimeOut Camera GrabProcess");
                            bNormal = false;
                            break;
                        }
                    }
                    if (bNormal == true)
                    {
                        //WIND2EventManager.OnSnapDone(this, new SnapDoneArgs(new CPoint(startOffsetX, startOffsetY), cpMemoryOffset + new CPoint(m_grabMode.m_GD.m_nFovSize, nWaferSizeY_px)));
                        //workManager.CheckSnapDone(new Rect(new Point(startOffsetX, startOffsetY), new Point(cpMemoryOffset.X + m_grabMode.m_GD.m_nFovSize, cpMemoryOffset.Y + nWaferSizeY_px)));
                        nScanLine++;
                        cpMemoryOffset.X += m_grabMode.m_GD.m_nFovSize;
                    }
                    else //비정상 스캔일때.
                    {
                        m_grabMode.m_camera.StopGrab();
                        if (nRescanCount > nRescanTotal)
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


                if (workManager.WaitWorkDone(ref EQ.m_EQ.StopToken(), 60 * 3 /*3 minutes*/) == false)
                {
                        inspectionTimeWatcher.Stop();

                        TempLogger.Write("Inspection", "Time out!!!");
                        return "OK";
                } // 5 minutes

                inspectionTimeWatcher.Stop();
                TempLogger.Write("Inspection", string.Format("{0:F3}", (double)inspectionTimeWatcher.ElapsedMilliseconds / (double)1000));
                return "OK";
            }


            finally
            {
                m_grabMode.SetLight(false);
            }
        }
    }
}
