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
    class Run_StainGrab:ModuleRunBase
    {
        Vision m_module;
        Vision.MainOptic mainOpt;
        GrabMode StainGrabMode;
        Camera_Basler camStain;
        string sStainGrabMode;

        public Run_StainGrab(Vision module)
        {
            m_module = module;
            mainOpt = m_module.m_mainOptic;
            camStain = mainOpt.camStain;
            InitModuleRun(module);
        }
        #region Property
        string p_sStainGrabMode
        {
            get { return sStainGrabMode; }
            set
            {
                sStainGrabMode = value;
                StainGrabMode = m_module.GetGrabMode(value);
            }
        }
        #endregion
        public override ModuleRunBase Clone()
        {
            Run_StainGrab run = new Run_StainGrab(m_module);
            run.p_sStainGrabMode = p_sStainGrabMode;

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sStainGrabMode = tree.Set(p_sStainGrabMode, p_sStainGrabMode, m_module.p_asGrabMode, "Grab Mode : Stain Grab", "Select GrabMode", bVisible);
        }

        public override string Run()
        {
            if (p_sStainGrabMode == null) return "Grab Mode : Side Grab == Null";

            try
            {
                #region [Local Variable]
                AxisXY axisXY = m_module.axisXY;
                Axis axisZ = mainOpt.m_axisZ;

                StainGrabMode.m_dTrigger = Convert.ToInt32(10 * StainGrabMode.m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                int nCamWidth = StainGrabMode.m_camera.GetRoiSize().X;
                int nCamHeight = StainGrabMode.m_camera.GetRoiSize().Y;
                int nPodSizeY_px = Convert.ToInt32(StainGrabMode.m_nPodSize_mm * mainOpt.m_pulsePermm / StainGrabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nPulsePerWidth = nCamWidth * StainGrabMode.m_dTrigger;
                int nPulsePerHeight = nCamHeight * StainGrabMode.m_dTrigger;

                string strPool = StainGrabMode.m_memoryPool.p_id;
                string strGroup = StainGrabMode.m_memoryGroup.p_id;
                string strMemory = StainGrabMode.m_memoryData.p_id;

                MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

                //가로 총 Pixel 갯수 : PodWidth * 1000 / camera res X
                //가로 횟수 : 총 Pixel 갯수 / CamWidth
                int nXCount = nPodSizeY_px / nCamWidth;
                int nYCount = nPodSizeY_px / nCamHeight;
                #endregion

                StainGrabMode.SetLight(true);

                if (m_module.Run(mainOpt.Move(axisZ, StainGrabMode.m_nFocusPosZ)))
                    return p_sInfo;

                for (int x = 0; x < nXCount; x++)
                {
                    if (EQ.IsStop())
                        return "OK";

                    double dPosX = StainGrabMode.m_rpAxisCenter.X - (nPulsePerWidth * nXCount / 2) + x * nPulsePerWidth;

                    if (m_module.Run(mainOpt.Move(axisXY.p_axisX, dPosX)))
                        return p_sInfo;

                    for (int y = 0; y < nYCount; y++)
                    {
                        camStain.Grab();

                        IntPtr ptr = mem.GetPtr();
                        Parallel.For(0, nCamHeight, (i) => {
                            Marshal.Copy(camStain.p_ImageData.m_aBuf, 0, (IntPtr)((long)ptr + (x * nCamWidth) + (i * mem.W)), nCamWidth);
                        });

                        double dPosY = StainGrabMode.m_rpAxisCenter.Y - (nPulsePerHeight * nYCount / 2) + y * nPulsePerHeight;

                        if(m_module.Run(mainOpt.Move(axisXY.p_axisY,dPosY)))
                            return p_sInfo;
                    }
                }
            }
            finally
            {
                StainGrabMode.SetLight(false);
            }
           
            return "OK";
        }
    }
}
