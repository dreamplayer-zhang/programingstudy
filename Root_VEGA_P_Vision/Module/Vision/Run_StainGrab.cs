using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision.Module
{
    class Run_StainGrab : ModuleRunBase
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
            sStainGrabMode = "";
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
            if (p_sStainGrabMode == null) return "Grab Mode : Stain Grab == Null";

            try
            {
                if (camStain == null)
                    camStain.Connect();
                #region [Local Variable]
                AxisXY axisXY = m_module.m_stage.m_axisXY;
                Axis axisZ = mainOpt.m_axisZ;

                StainGrabMode.m_dTrigger = Convert.ToInt32(10 * StainGrabMode.m_dResY_um);  // 1pulse = 0.1um -> 20pulse = 1um
                int nCamWidth = StainGrabMode.m_camera.GetRoiSize().X;
                int nCamHeight = StainGrabMode.m_camera.GetRoiSize().Y;
                int nPodSizeY_px = Convert.ToInt32(StainGrabMode.m_nPodYSize_mm * 1000 / StainGrabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nPulsePerWidth = nCamWidth * StainGrabMode.m_dTrigger;
                int nPulsePerHeight = nCamHeight * StainGrabMode.m_dTrigger;


                //tmp
                //InfoPod.ePod parts = m_module.p_infoPod.p_ePod;
                //Vision.eUpDown upDown = (Vision.eUpDown)Enum.ToObject(typeof(Vision.eUpDown), m_module.p_infoPod.p_bTurn);

                //MemoryData mem = mainOpt.GetMemoryData(parts, Vision.MainOptic.eInsp.Stain, upDown);
                MemoryData mem = mainOpt.GetMemoryData(InfoPod.ePod.EIP_Cover, Vision.MainOptic.eInsp.Stain, Vision.eUpDown.Front);

                //가로 총 Pixel 갯수 : PodWidth * 1000 / camera res X
                //가로 횟수 : 총 Pixel 갯수 / CamWidth
                //int nXCount = (int)Math.Ceiling((double)(nPodSizeY_px / nCamWidth)) + 1;
                //int nYCount = nPodSizeY_px / nCamHeight;
                int nXCount = 3;
                int nYCount = 3;
                #endregion

                List<int> illumList = new List<int>();
                for (int i = 0; i < StainGrabMode.m_lightSet.m_aLight.Count; i++)
                    if (StainGrabMode.GetLight(i) > 0) illumList.Add(i);
                if (m_module.Run(m_module.Move(axisZ, StainGrabMode.m_nFocusPosZ)))
                    return p_sInfo;

                foreach(int v in illumList)
                {
                    StainGrabMode.SetLight(v, true);
                    for (int x = 0; x < nXCount; x++)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        double dPosX = StainGrabMode.m_rpAxisCenter.X /*+ nPodSizeY_px * 1000*/ - nPulsePerWidth * (x-1);

                        //if (m_module.Run(m_module.Move(axisXY.p_axisX, dPosX)))
                        if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, StainGrabMode.m_rpAxisCenter.Y + nPodSizeY_px * 500))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;

                        for (int y = 0; y < nYCount; y++)
                        {
                            double tmp = (double)nYCount / 2 + y;
                            double dPosY = StainGrabMode.m_rpAxisCenter.Y /*+ nPodSizeY_px * 500*/ - nPulsePerHeight * (y-1);

                            if (m_module.Run(axisXY.p_axisY.StartMove(dPosY)))
                                return p_sInfo;
                            if (m_module.Run(axisXY.p_axisY.WaitReady()))
                                return p_sInfo;

                            camStain.Grab();

                            IntPtr ptr = mem.GetPtr(v);
                            byte[] arr = camStain.p_ImageData.m_aBuf;
                            int byteperpxl = camStain.p_ImageData.GetBytePerPixel();
                            Parallel.For(0, nCamHeight, (j) =>
                            {
                                Marshal.Copy(arr, (nCamHeight - j - 1) * nCamWidth * byteperpxl, (IntPtr)((long)ptr + (x * nCamWidth * byteperpxl) + ((j + nCamHeight * y) * mem.W)), nCamWidth * byteperpxl);
                            });

                            camStain.StopGrab();
                        }
                    }
                    StainGrabMode.SetLight(false);
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
