using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision.Module
{
    class Run_SideGrab:ModuleRunBase
    {
        Vision m_module;
        Vision.SideOptic sideOpt;
        GrabMode SidegrabMode;
        Camera_Basler camSide;
        string sSideGrabMode;

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

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sSideGrabMode = tree.Set(p_sSideGrabMode, p_sSideGrabMode, m_module.p_asGrabMode, "Grab Mode : Side Grab", "Select GrabMode", bVisible);
        }

        public override string Run()
        {
            if (p_sSideGrabMode == null) return "Grab Mode : Side Grab == Null";

            try
            {
                #region [Local Variable]
                AxisXY axisXY = m_module.m_stage.m_axisXY;
                Axis axisZ = sideOpt.axisZ;

                CPoint cpMemoryOffset = new CPoint(SidegrabMode.m_cpMemoryOffset);

                SidegrabMode.m_dTrigger = Convert.ToInt32(10 * SidegrabMode.m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                int nCamWidth = SidegrabMode.m_camera.GetRoiSize().X;
                int nCamHeight = SidegrabMode.m_camera.GetRoiSize().Y;
                int nPodSizeY_px = Convert.ToInt32(SidegrabMode.m_nPodSize_mm * sideOpt.m_pulsePermm / SidegrabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nPulsePerWidth = nCamWidth * SidegrabMode.m_dTrigger;
                int nPulsePerHeight = nCamHeight * SidegrabMode.m_dTrigger;
                //가로 총 Pixel 갯수 : PodWidth * 1000 / camera res X
                //가로 횟수 : 총 Pixel 갯수 / CamWidth
                int nXCount = nPodSizeY_px / nCamWidth;
                int nYCount = nPodSizeY_px / nCamHeight;
                #endregion
                int lightcnt = SidegrabMode.m_lightSet.m_aLight.Count;

                if (m_module.sideGrabCnt > 3) m_module.sideGrabCnt = 0;

                InfoPod.ePod parts = m_module.p_infoPod.p_ePod;
                //MemoryData mem = sideOpt.GetMemoryData(parts.ToString());
                Vision.eUpDown e = Enum.ToObject(typeof(Vision.eUpDown), m_module.p_infoPod.p_bTurn);
                MemoryData mem = sideOpt.GetMemoryData(parts.ToString() + "."+);
                if (m_module.Run(m_module.Move(axisZ, SidegrabMode.m_nFocusPosZ)))
                    return p_sInfo;

                //enum 순서 고려해야됨
                for (int x=0;x<nXCount;x++)
                {
                    for(int lc=0;lc<lightcnt;lc++)
                    {
                        SidegrabMode.SetLight(lc,true);

                        double dPosY = SidegrabMode.m_rpAxisCenter.Y - nPulsePerWidth*(nXCount / 2 + x);

                        if (m_module.Run(m_module.Move(m_module.m_stage.m_axisXY.p_axisY, dPosY)))
                            return p_sInfo;

                        camSide.Grab();
                        IntPtr ptr = mem.GetPtr(lc);

                        Parallel.For(0, nCamHeight, (i) => {
                            Marshal.Copy(camSide.p_ImageData.m_aBuf, 0, (IntPtr)((long)ptr + (x * nCamWidth) + (i * mem.W)), nCamWidth);
                        });

                        SidegrabMode.SetLight(false);
                    }
                }
            }
            finally
            {
                SidegrabMode.SetLight(false);
            }

            return "OK";
        }
    }
}
