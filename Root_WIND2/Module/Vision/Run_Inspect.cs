using RootTools;
using RootTools.Camera;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.Module
{
    public class Run_Inspect : ModuleRunBase
    {
        Vision m_module;

        string m_sRecipeName = string.Empty;

        double m_dTDIToVRSOffsetZ = 0;

        // Grab 관련 파라매터 (이거 나중에 구조 변경 필요할듯)
        bool m_bInvDir = false;
        public GrabMode m_grabMode = null;
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

            //레시피에 GrabMode 저장하고 있어야함
            InspectionManagerFrontside inspectionFront = GlobalObjects.Instance.Get<InspectionManagerFrontside>();
            inspectionFront.Stop();

            if (m_grabMode == null) return "Grab Mode == null";

            if (EQ.IsStop() == false)
            {
                if (inspectionFront.Recipe.Read(m_sRecipeName, true) == false)
                    return "Recipe Open Fail";

                inspectionFront.Start();

            }
            else
            {
                inspectionFront.Stop();
            }

            try
            {
                m_module.p_bStageVac = true;
                m_grabMode.SetLight(true);

                AxisXY axisXY = m_module.AxisXY;
                Axis axisZ = m_module.AxisZ;
                Axis axisRotate = m_module.AxisRotate;
                CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);
                int nScanLine = 0;
                int nMMPerUM = 1000;

                double dXScale = m_grabMode.m_dResX_um * 10;
                cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_GD.m_nFovSize;
                m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_grabMode.m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                int nWaferSizeY_px = Convert.ToInt32(m_grabMode.m_nWaferSize_mm * nMMPerUM / m_grabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수
                int nScanOffset_pulse = 40000;

                int startOffsetX = cpMemoryOffset.X;
                int startOffsetY = 0;

                while (m_grabMode.m_ScanLineNum > nScanLine)
                {
                    if (EQ.IsStop())
                        return "OK";

                    // 위에서 아래로 찍는것을 정방향으로 함, 즉 Y축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향
                    // Grab하기 위해 이동할 Y축의 시작 끝 점
                    //ybkwon0113
                    int nScanSpeed = Convert.ToInt32((double)m_grabMode.m_nMaxFrame * m_grabMode.m_dTrigger * (double)m_grabMode.m_nScanRate / 100);
                    nScanOffset_pulse = (int)((double)nScanSpeed * axisXY.p_axisY.GetSpeedValue(Axis.eSpeed.Move).m_acc * 0.5) * 2;
                    double dStartPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                    double dEndPosY = m_grabMode.m_rpAxisCenter.Y + m_grabMode.m_ptXYAlignData.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                    m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                    //  if (m_grabMode.m_bUseBiDirectionScan && Math.Abs(axisXY.p_axisY.p_posActual - dStartPosY) > Math.Abs(axisXY.p_axisY.p_posActual - dEndPosY))
                    if (m_grabMode.m_bUseBiDirectionScan && nScanLine % 2 == 1)
                    {
                        double dTemp = dStartPosY;  // dStartPosY <--> dEndPosY 바꿈.
                        dStartPosY = dEndPosY;
                        dEndPosY = dTemp;
                        m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                    }
                    System.Diagnostics.Debug.Print("s:" + dStartPosY.ToString() + "e: " + dEndPosY.ToString());
                    double dfovum = m_grabMode.m_GD.m_nFovSize * dXScale;
                    double dPosX = m_grabMode.m_rpAxisCenter.X + m_grabMode.m_ptXYAlignData.X + nWaferSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_GD.m_nFovSize * dXScale;
                    double dNextPosX = m_grabMode.m_rpAxisCenter.X + m_grabMode.m_ptXYAlignData.X + nWaferSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + 1 + m_grabMode.m_ScanStartLine) * m_grabMode.m_GD.m_nFovSize * dXScale;

                    double dPosZ = m_grabMode.m_nFocusPosZ;
                    if (m_grabMode.m_dVRSFocusPos != 0)
                    {
                        dPosZ = m_grabMode.m_dVRSFocusPos + m_dTDIToVRSOffsetZ;
                    }
                    if (m_module.Run(axisZ.StartMove(m_grabMode.m_nFocusPosZ)))
                        return p_sInfo;

                    //if (nScanLine == 0)
                    if (true)
                    {
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                    }
                    else
                    {
                        if (m_module.Run(axisXY.p_axisY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisXY.p_axisY.StartMove(dStartPosY)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                    }
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

                    m_grabMode.StartGrab(mem, cpMemoryOffset, nWaferSizeY_px, m_grabMode.m_GD);
                    //m_grabMode.StartGrabColor(mem, cpMemoryOffset, nWaferSizeY_px, m_grabMode.m_eGrabDirection == eGrabDirection.BackWard);

                    if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                        return p_sInfo;
                    if (m_grabMode.m_eGrabDirection == eGrabDirection.Forward)
                    {
                        while (axisXY.p_axisY.p_posActual < dTriggerEndPosY)
                        {
                            System.Threading.Thread.Sleep(10);
                        }
                    }
                    else
                    {
                        while (axisXY.p_axisY.p_posActual > dTriggerStartPosY)
                        {
                            System.Threading.Thread.Sleep(10);
                        }
                    }
                    axisXY.p_axisX.StartMove(dNextPosX);

                    axisXY.p_axisY.RunTrigger(false);

                    while (m_grabMode.m_camera.p_nGrabProgress != 100)
                    {
                        System.Threading.Thread.Sleep(10);
                        m_log.Info("Wait Camera GrabProcess");
                    }
                    WIND2EventManager.OnSnapDone(this, new SnapDoneArgs(new CPoint(startOffsetX, startOffsetY), cpMemoryOffset + new CPoint(m_grabMode.m_GD.m_nFovSize, nWaferSizeY_px)));


                    nScanLine++;
                    cpMemoryOffset.X += m_grabMode.m_GD.m_nFovSize;
                }
                m_grabMode.m_camera.StopGrab();
                return "OK";
            }
            finally
            {
                m_grabMode.SetLight(false);
            }
        }
    }
}
