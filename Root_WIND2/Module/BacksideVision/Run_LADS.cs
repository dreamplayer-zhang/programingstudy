using System;
using RootTools;
using RootTools.Camera;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;

namespace Root_WIND2.Module
{
    class Run_LADS : ModuleRunBase
    {
        // Member
        BackSideVision m_module;
        GrabMode m_grabMode = null;
        string m_sGrabMode = "";

        public string p_sGrabMode
        {
            get
            {
                return m_sGrabMode;
            }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }

        public Run_LADS(BackSideVision module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_LADS run = new Run_LADS(m_module);
            run.p_sGrabMode = p_sGrabMode;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode : LADS", "Select GrabMode", bVisible);
            //if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
        }

        public override string Run()
        {
            if (m_grabMode == null)
                return "Grab Mode == null";

            try
            {
                m_grabMode.SetLight(true);

                AxisXY axisXY = m_module.AxisXY;
                Axis axisZ = m_module.AxisZ;
                CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);
                int nScanLine = 0;
                int nMMPerUM = 1000;
                int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
                int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

                double dXScale = m_grabMode.m_dResX_um * 10;
                cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth;
                m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_grabMode.m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                int nWaferSizeY_px = Convert.ToInt32(m_grabMode.m_nWaferSize_mm * nMMPerUM / m_grabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수
                int nScanOffset_pulse = 10000;
                string strPool = m_grabMode.m_memoryPool.p_id;
                string strGroup = m_grabMode.m_memoryGroup.p_id;
                string strMemory = m_grabMode.m_memoryData.p_id;

                MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                int nScanSpeed = Convert.ToInt32((double)m_grabMode.m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_grabMode.m_nScanRate / 100);

                double dStartPosY = m_grabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                double dEndPosY = m_grabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                double dTriggerStartPosY = m_grabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                double dTriggerEndPosY = m_grabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                GrabData grabData = m_grabMode.m_GD;

                while (m_grabMode.m_ScanLineNum > nScanLine)
                {
                    if (EQ.IsStop())
                        return "OK";

                    double dPosX = m_grabMode.m_rpAxisCenter.X - nWaferSizeY_px * (double)m_grabMode.m_dTrigger / 2
                        + (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

                    if (m_grabMode.m_bUseBiDirectionScan && (nScanLine % 2 != 0))
                    {
                        GeneralFunction.Swap(ref dStartPosY, ref dEndPosY);
                        m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                    }
                    else
                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

                    grabData.bInvY = m_grabMode.m_eGrabDirection == eGrabDirection.Forward;

                    if (m_module.Run(axisZ.StartMove(m_grabMode.m_nFocusPosZ)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                    if (m_module.Run(axisZ.WaitReady()))
                        return p_sInfo;

                    if (Math.Abs(dTriggerEndPosY - dStartPosY) > Math.Abs(dTriggerStartPosY - dStartPosY))
                        dTriggerEndPosY += nScanOffset_pulse;

                    else
                        dTriggerStartPosY -= nScanOffset_pulse;

                    axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger*nCamHeight /2,5, true);

                    m_grabMode.StartGrab(mem, cpMemoryOffset, nWaferSizeY_px, grabData);

                    if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;

                    axisXY.p_axisY.RunTrigger(false);


                    while (m_grabMode.m_camera.p_nGrabProgress != 100)
                    {
                        System.Threading.Thread.Sleep(10);
                        m_log.Info("Wait Camera GrabProcess");
                    }
                    m_grabMode.m_camera.StopGrab();
                    nScanLine++;
                    cpMemoryOffset.X += nCamWidth;
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
