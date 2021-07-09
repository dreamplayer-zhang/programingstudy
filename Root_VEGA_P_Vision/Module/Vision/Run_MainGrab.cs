using RootTools;
using RootTools.Camera;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision.Module
{
    class Run_MainGrab : ModuleRunBase
    {
        Vision m_module;
        Vision.MainOptic mainOpt;
        GrabMode MainGrabMode;
        string sMainGrabMode;
        int nScanHeightNum;
        public Run_MainGrab(Vision module)
        {
            m_module = module;
            Init();
            InitModuleRun(module);
        }

        void Init()
        {
            mainOpt = m_module.m_mainOptic;
            sMainGrabMode = "";
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
            run.nScanHeightNum = nScanHeightNum;
            run.scanheights = scanheights;
            return run;
        }
        public List<int> scanheights = new List<int>();
        public List<int> ScanHeights
        {
            get
            {
                List<int> heights = new List<int>();
                foreach (int n in scanheights)
                    heights.Add(n);
                return heights;
            }
        }
        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sMainGrabMode = tree.Set(p_sMainGrabMode, p_sMainGrabMode, m_module.p_asGrabMode, "Grab Mode : MainGrab", "Select GrabMode", bVisible);
            if (MainGrabMode != null)
            {
                MainGrabMode.RunTreeLinescanOption(tree, bVisible);
            }
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
                int nFOV = MainGrabMode.m_GD.m_nFovSize;
                int nCamHeight = MainGrabMode.m_camera.GetRoiSize().Y;
                int nPodSizeY_px = Convert.ToInt32(MainGrabMode.m_nPodYSize_mm * nMMPerUM / MainGrabMode.m_dResY_um);  //파드 영역의 Y픽셀 갯수
                int nPulsePerWidth = nFOV * MainGrabMode.m_dTrigger;
                int nPulsePerHeight = nCamHeight * MainGrabMode.m_dTrigger;
                double dXScale = MainGrabMode.m_dResX_um * 10;
                int nTotalTriggerCount = Convert.ToInt32(MainGrabMode.m_dTrigger * nPodSizeY_px);
                int nScanSpeed = Convert.ToInt32((double)MainGrabMode.m_nMaxFrame * MainGrabMode.m_dTrigger * nCamHeight * MainGrabMode.m_nScanRate / 100);
                int nScanOffset_pulse = 40000;

                //InfoPod.ePod parts = m_module.p_infoPod.p_ePod;
                //Vision.eUpDown upDown = (Vision.eUpDown)Enum.ToObject(typeof(Vision.eUpDown), m_module.p_infoPod.p_bTurn);

                //MemoryData mem = mainOpt.GetMemoryData(parts,Vision.MainOptic.eInsp.Main,upDown);

                MemoryData mem = mainOpt.GetMemoryData(InfoPod.ePod.EIP_Cover, Vision.MainOptic.eInsp.Main, Vision.eUpDown.Front);

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
                cpMemoryOffset.X += MainGrabMode.m_ScanStartLine * nFOV;

                if (m_module.Run(axisZ.StartMove(MainGrabMode.m_nFocusPosZ)))
                    return p_sInfo;
                if (m_module.Run(axisZ.WaitReady()))
                    return p_sInfo;

                while (MainGrabMode.m_ScanLineNum > nScanLine)
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

                    double dPosX = MainGrabMode.m_rpAxisCenter.X + nPodSizeY_px * (double)MainGrabMode.m_dTrigger / 2 -
                        (nScanLine + MainGrabMode.m_ScanStartLine) * nFOV * dXScale;

                    if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;

                    axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, MainGrabMode.m_dTrigger, 5, true);
                    MainGrabMode.StartGrab(mem, cpMemoryOffset, nPodSizeY_px, grabData);
                    MainGrabMode.Grabed += GrabMode_Grabed;

                    if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.p_axisY.WaitReady()))
                        return p_sInfo;

                    axisXY.p_axisY.RunTrigger(false);
                    nScanLine++;
                    cpMemoryOffset.X += nFOV;
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
