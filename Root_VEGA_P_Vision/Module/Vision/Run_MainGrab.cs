using RootTools;
using RootTools.Camera;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision.Module
{
    class Run_MainGrab: ModuleRunBase
    {
        Vision m_module;
        Vision.MainOptic mainOpt;
        GrabMode MainGrabMode;
        Camera_Dalsa camMain;
        string sMainGrabMode;
        public Run_MainGrab(Vision module)
        {
            m_module = module;
            mainOpt = m_module.m_mainOptic;
            camMain = mainOpt.camTDI;
            sMainGrabMode = "";
            InitModuleRun(module);
        }
        #region Property
        string p_sMainGrabMode
        {
            get { return sMainGrabMode; }
            set
            {
                sMainGrabMode = value;
                MainGrabMode = m_module.GetGrabMode(value);
            }
        }
        #endregion

        public override ModuleRunBase Clone()
        {
            Run_MainGrab run = new Run_MainGrab(m_module);
            run.p_sMainGrabMode = p_sMainGrabMode;

            return run;
        }
        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sMainGrabMode = tree.Set(p_sMainGrabMode, p_sMainGrabMode, m_module.p_asGrabMode, "Grab Mode : Main Grab", "Select GrabMode", bVisible);
        }
        public override string Run()
        {
            if (p_sMainGrabMode == null) return "Grab Mode : Main Grab == Null";

            try
            {
                #region [Local Variable]
                AxisXY axisXY = m_module.m_stage.m_axisXY;
                Axis axisZ = mainOpt.m_axisZ;
                CPoint cpMemoryOffset = new CPoint(MainGrabMode.m_cpMemoryOffset);

                int nScanLine = 0;
                int nMMPerUM = 1000;
                MainGrabMode.m_dTrigger = Convert.ToInt32(10 * MainGrabMode.m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                int nCamWidth = MainGrabMode.m_camera.GetRoiSize().X;
                int nCamHeight = MainGrabMode.m_camera.GetRoiSize().Y;
                int nPodSizeY_px = Convert.ToInt32(MainGrabMode.m_nPodSize_mm * nMMPerUM / MainGrabMode.m_dResY_um);  //파드 영역의 Y픽셀 갯수
                int nPulsePerWidth = nCamWidth * MainGrabMode.m_dTrigger;
                int nPulsePerHeight = nCamHeight * MainGrabMode.m_dTrigger;
                double dXScale = MainGrabMode.m_dResX_um * 10;
                int nTotalTriggerCount = Convert.ToInt32(MainGrabMode.m_dTrigger * nPodSizeY_px);
                int nScanSpeed = Convert.ToInt32((double)MainGrabMode.m_nMaxFrame * MainGrabMode.m_dTrigger * nCamHeight * MainGrabMode.m_nScanRate / 100);
                int nScanOffset_pulse = 40000;

                string strPool = MainGrabMode.m_memoryPool.p_id;
                string strGroup = MainGrabMode.m_memoryGroup.p_id;
                string strMemory = MainGrabMode.m_memoryData.p_id;

                MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

                double dStartPosY = MainGrabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                double dEndPosY = MainGrabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                double dTriggerStartPosY = MainGrabMode.m_rpAxisCenter.Y - nTotalTriggerCount / 2;
                double dTriggerEndPosY = MainGrabMode.m_rpAxisCenter.Y + nTotalTriggerCount / 2;

                if (Math.Abs(dTriggerEndPosY - dStartPosY) > Math.Abs(dTriggerStartPosY - dStartPosY))
                    dTriggerEndPosY += nScanOffset_pulse;
                else
                    dTriggerStartPosY -= nScanOffset_pulse;

                GrabData grabData = MainGrabMode.m_GD;
                grabData.ReverseOffsetY = MainGrabMode.m_nReverseOffsetY;
                #endregion

                MainGrabMode.SetLight(true);
                cpMemoryOffset.X += MainGrabMode.m_ScanStartLine * nCamWidth;

                while(MainGrabMode.m_ScanLineNum>nScanLine)
                {
                    if (EQ.IsStop()) return "OK";

                    if (MainGrabMode.m_bUseBiDirectionScan && (nScanLine % 2 != 0))
                    {
                        GeneralFunction.Swap(ref dStartPosY, ref dEndPosY);
                        MainGrabMode.m_eGrabDirection = eGrabDirection.BackWard;
                    }
                    else
                        MainGrabMode.m_eGrabDirection = eGrabDirection.Forward;

                    grabData.bInvY = MainGrabMode.m_eGrabDirection == eGrabDirection.Forward;

                    double dPosX = MainGrabMode.m_rpAxisCenter.X - nPodSizeY_px * (double)MainGrabMode.m_dTrigger / 2 + 
                        (nScanLine + MainGrabMode.m_ScanStartLine) * nCamWidth * dXScale;

                    if (m_module.Run(m_module.Move(axisZ, MainGrabMode.m_nFocusPosZ)))
                        return p_sInfo;
                    if (m_module.Run(m_module.MoveXY(new RPoint(dPosX, dStartPosY))))
                        return p_sInfo;

                    axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, MainGrabMode.m_dTrigger, 5, true);
                    MainGrabMode.StartGrab(mem, cpMemoryOffset, nPodSizeY_px, grabData);
                    MainGrabMode.Grabed += GrabMode_Grabed;

                    if (m_module.Run(m_module.Move(axisXY.p_axisY, dEndPosY,nScanSpeed)))
                        return p_sInfo;

                    axisXY.p_axisY.RunTrigger(false);
                    nScanLine++;
                    cpMemoryOffset.X += nCamWidth;
                }

                MainGrabMode.m_camera.StopGrab();
                return "OK";

            }
            finally
            {
                MainGrabMode.SetLight(false);
            }

        }
        private void GrabMode_Grabed(object sender, EventArgs e)
        {
            GrabedArgs ga = (GrabedArgs)e;
            m_module.p_nProgress = ga.nProgress;
        }
    }
}
