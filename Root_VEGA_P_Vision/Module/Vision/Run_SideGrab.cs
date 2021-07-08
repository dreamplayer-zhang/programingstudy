using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision.Module
{
    class Run_SideGrab : ModuleRunBase
    {
        Vision m_module;
        Vision.SideOptic sideOpt;
        GrabMode SidegrabMode;
        Camera_Basler camSide;
        string sSideGrabMode;
        Vision.SideOptic.eSide side;
        double rowPos, colPos;
        long BFExpose,Expose45,ringExpose;
        public Run_SideGrab(Vision module)
        {
            m_module = module;
            sideOpt = m_module.m_sideOptic;
            camSide = sideOpt.camSide;
            sSideGrabMode = "";
            InitModuleRun(module);
        }
        #region Property
        string p_sSideGrabMode
        {
            get { return sSideGrabMode; }
            set
            {
                sSideGrabMode = value;
                SidegrabMode = m_module.GetGrabMode(value);
            }
        }
        #endregion
        public override ModuleRunBase Clone()
        {
            Run_SideGrab run = new Run_SideGrab(m_module);
            run.p_sSideGrabMode = p_sSideGrabMode;
            run.side = side;
            run.rowPos = rowPos;
            run.colPos = colPos;
            run.Expose45 = Expose45;
            run.BFExpose = BFExpose;
            run.ringExpose = ringExpose;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sSideGrabMode = tree.Set(p_sSideGrabMode, p_sSideGrabMode, m_module.p_asGrabMode,
                "Grab Mode : Side Grab", "Select GrabMode", bVisible);
            side = (Vision.SideOptic.eSide)tree.Set(side, side, "Scan Direction", "Side Scan Direction", bVisible);
            rowPos = tree.Set(rowPos, rowPos, "Side Row Scan Pos", "Side Row Scan Pos", bVisible);
            colPos = tree.Set(colPos, colPos, "Side Col Scan Pos", "Side Col Scan Pos", bVisible);
            Expose45 = tree.Set(Expose45, Expose45, "45 Degree Exposure Time", "45 Degree Exposure Time", bVisible);
            BFExpose = tree.Set(BFExpose, BFExpose, "BF Exposure Time", "BF Exposure Time", bVisible);
            ringExpose = tree.Set(ringExpose, ringExpose, "Ring Exposure Time", "Ring Exposure Time", bVisible);
        }

        public override string Run()
        {
            if (p_sSideGrabMode == null) return "Grab Mode : Side Grab == Null";

            AxisXY axisXY = m_module.m_stage.m_axisXY;
            Axis axisZ = sideOpt.axisZ;
            Axis axisR = m_module.m_stage.m_axisR;

            try
            {
                if (!camSide.IsConnected())
                    camSide.Connect();

                #region [Local Variable]

                CPoint cpMemoryOffset = new CPoint(SidegrabMode.m_cpMemoryOffset);

                SidegrabMode.m_dTrigger = Convert.ToInt32(10 * SidegrabMode.m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                int nCamWidth = SidegrabMode.m_camera.GetRoiSize().X;
                int nCamHeight = SidegrabMode.m_camera.GetRoiSize().Y;
                int nPodSizeY_px = Convert.ToInt32(SidegrabMode.m_nPodYSize_mm * sideOpt.m_pulsePermm / SidegrabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nPulsePerWidth = nCamWidth * SidegrabMode.m_dTrigger;
                int nPulsePerHeight = nCamHeight * SidegrabMode.m_dTrigger;

                //가로 총 Pixel 갯수 : PodWidth * 1000 / camera res X
                //가로 횟수 : 총 Pixel 갯수 / CamWidth
                //int nXCount = nPodSizeY_px / nCamWidth;
                int nXCount = 10;
                int nYCount = nPodSizeY_px / nCamHeight;
                List<int> illumList = new List<int>();
                for (int i = 0; i < SidegrabMode.m_lightSet.m_aLight.Count; i++)
                    if (SidegrabMode.GetLight(i) > 0) illumList.Add(i);

                int lightcnt = illumList.Count;

                //InfoPod.ePod parts = m_module.p_infoPod.p_ePod;
                InfoPod.ePod parts = InfoPod.ePod.EIP_Cover;
                //-> 찍는거 확인한다음에 어떻게 집어넣을지 확인해야됨 (계획 : p_bTurn + grabcount(어차피 두번직으니까))
                //Vision.eUpDown upDown = (Vision.eUpDown)Enum.ToObject(typeof(Vision.eUpDown), m_module.p_infoPod.p_bTurn);

                #endregion

                if (m_module.Run(axisZ.StartMove(SidegrabMode.m_nFocusPosZ)))
                    return p_sInfo;
                if (m_module.Run(axisZ.WaitReady()))
                    return p_sInfo;

                int scannum = side.Equals(Vision.SideOptic.eSide.All) ? 4 : 1;

                SidegrabMode.SetLight(false);

                for (int j = 0; j < scannum; j++)
                {
                    MemoryData mem;
                    double rotate = 0;
                    if (scannum == 1)
                    {
                        mem = sideOpt.GetMemoryData(parts, side);
                        rotate = 90000 * (int)side;
                    }
                    else
                    {
                        mem = sideOpt.GetMemoryData(parts, (Vision.SideOptic.eSide)j);
                        rotate = 90000 * j;
                    }

                    double posX = (((int)side) % 2 == 0) ? rowPos : colPos;

                    if (m_module.Run(axisR.StartMove(rotate)))
                        return p_sInfo;
                    if (m_module.Run(axisR.WaitReady()))
                        return p_sInfo;

                    if (m_module.Run(axisXY.p_axisX.StartMove(posX)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.p_axisY.WaitReady()))
                        return p_sInfo;


                    for (int lc = 0; lc < lightcnt; lc++)
                    {
                        long expose = 0;
                        switch(SidegrabMode.m_lightSet.m_aLight[illumList[lc]].m_sName)
                        {
                            case "Side BF":
                                expose = BFExpose;
                                break;
                            case "Side Power":
                                expose = Expose45;
                                break;
                            case "Side Ring":
                                expose = ringExpose;
                                break;
                        }

                        camSide.SetExposureTime(expose);

                        SidegrabMode.SetLight(illumList[lc], true);

                        IntPtr ptr = mem.GetPtr(lc);

                        for (int x = 0; x < nXCount; x++)
                        {
                            double dPosY = SidegrabMode.m_rpAxisCenter.Y - nPulsePerWidth * (-nXCount / 2 + x);

                            if (m_module.Run(axisXY.p_axisY.StartMove(dPosY)))
                                return p_sInfo;
                            if (m_module.Run(axisXY.p_axisY.WaitReady()))
                                return p_sInfo;

                            camSide.Grab();

                            Parallel.For(0, nCamHeight, (i) =>
                            {
                                Marshal.Copy(camSide.p_ImageData.m_aBuf, i * nCamWidth, (IntPtr)((long)ptr + ((nCamWidth * x) + (mem.W * i))), nCamWidth);
                            });
                        }
                        SidegrabMode.SetLight(false);
                    }
                }
            }
            finally
            {
                m_module.Run(axisR.StartHome());
                m_module.Run(axisR.WaitReady());

                SidegrabMode.SetLight(false);
            }

            return "OK";
        }
    }
}
