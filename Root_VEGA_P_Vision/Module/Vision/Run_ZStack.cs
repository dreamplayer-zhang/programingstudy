using RootTools.Camera.Matrox;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using RootTools.ImageProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RootTools.Control;
using RootTools;
using System.Threading;

namespace Root_VEGA_P_Vision.Module
{
    class Run_ZStack : ModuleRunBase
    {
        Vision m_module;
        Vision.MainOptic mainOpt;
        GrabMode ZStackGrabMode;
        Camera_Matrox camZStack;
        string sZStackGrabMode;
        double radius, power;
        int nstartPos;
        int nendPos;
        int nstep;

        public Run_ZStack(Vision module)
        {
            m_module = module;
            mainOpt = m_module.m_mainOptic;
            camZStack = mainOpt.camZStack;
            sZStackGrabMode = "";
            nstartPos = 0;
            nendPos = 0;
            nstep = 0;
            InitModuleRun(module);
        }
        #region Property
        string p_sZStackGrabMode
        {
            get { return sZStackGrabMode; }
            set
            {
                sZStackGrabMode = value;
                ZStackGrabMode = m_module.GetGrabMode(value);
            }
        }
        public double Radius
        {
            get => radius;
            set => radius = value;
        }
        public double Power
        {
            get => power;
            set => power = value;
        }
        #endregion
        public override ModuleRunBase Clone()
        {
            Run_ZStack run = new Run_ZStack(m_module);
            run.p_sZStackGrabMode = p_sZStackGrabMode;
            run.nstartPos = nstartPos;
            run.nendPos = nendPos;
            run.nstep = nstep;
            run.Radius = Radius;
            run.Power = Power;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sZStackGrabMode = tree.Set(p_sZStackGrabMode, p_sZStackGrabMode, m_module.p_asGrabMode, "Grab Mode : ZStack Grab", "Select GrabMode", bVisible);
            nstartPos = tree.Set(nstartPos, nstartPos, "Stacking Start Z", "Stacking Start Z Position", bVisible);
            nendPos = tree.Set(nendPos, nendPos, "Stacking End Z", "Stacking End Z Position", bVisible);
            nstep = tree.Set(nstep, nstep, "Step Count", "Snap Step Count", bVisible);
            Radius = tree.Set(Radius, Radius, "Radius", "radius", bVisible);
            Power = tree.Set(Power, Power, "Power", "Power", bVisible);
        }

        public unsafe override string Run()
        {
            if (p_sZStackGrabMode == null) return "Grab Mode : ZStack Grab == Null";
            AxisXY axisXY = m_module.m_stage.m_axisXY;
            Axis axisZ = mainOpt.m_axisZ;
            try
            {
                ZStackGrabMode.SetLight(true);

                #region localVariable
                double dAxisStartPosZ = nstartPos - 10000;
                double dAxisEndPosZ = nendPos + 10000;
                double dTriggerStartPosZ = nstartPos;
                double dTriggerEndPosZ = nendPos;
                int nMMPerUM = 1000;
                int nCamWidth = ZStackGrabMode.m_camera.GetRoiSize().X;
                int nCamHeight = ZStackGrabMode.m_camera.GetRoiSize().Y;
                int nPodSizeY_px = Convert.ToInt32(ZStackGrabMode.m_nPodYSize_mm * nMMPerUM / ZStackGrabMode.m_dResY_um);  //파드 영역의 Y픽셀 갯수
                int nPodSizeX_px = Convert.ToInt32(ZStackGrabMode.m_nPodXSize_mm * nMMPerUM / ZStackGrabMode.m_dResX_um);
                double dStartPosX = ZStackGrabMode.m_rpAxisCenter.X; /*- (nPodSizeX_px * ZStackGrabMode.m_dResX_um * 1000)/ 2;*/
                double dStartPosY = ZStackGrabMode.m_rpAxisCenter.Y; /*- (nPodSizeY_px * ZStackGrabMode.m_dResY_um * 1000)/ 2;*/
                #endregion

                if (m_module.Run(axisXY.StartMove(new RPoint(dStartPosX, dStartPosY))))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;

                //InfoPod.ePod parts = m_module.p_infoPod.p_ePod;
                //Vision.eUpDown upDown = (Vision.eUpDown)Enum.ToObject(typeof(Vision.eUpDown), m_module.p_infoPod.p_bTurn);
                //MemoryData mem = mainOpt.GetMemoryData(parts, Vision.MainOptic.eInsp.Stack, upDown);

                MemoryData mem = mainOpt.GetMemoryData(InfoPod.ePod.EIP_Cover, Vision.MainOptic.eInsp.Stack, Vision.eUpDown.Front);
                //nstep = mem.p_nCount - 1;
                if (nstep > mem.p_nCount)
                    return "nstep is over the mem count";

                double dstep = Math.Abs(dTriggerEndPosZ - dTriggerStartPosZ) / nstep;

                ZStackGrabMode.m_nScanRate = 10;
                FocusStacking_new fs = new FocusStacking_new(mem);
                double dCamHeighttoPulse = nCamHeight * ZStackGrabMode.m_dResY_um*10;
                double dCamWidthtoPulse = nCamWidth * ZStackGrabMode.m_dResX_um*10;
                int nScanSpeed = Convert.ToInt32(ZStackGrabMode.m_nMaxFrame * (dTriggerEndPosZ - dTriggerStartPosZ) / nstep * ZStackGrabMode.m_nScanRate / 100);

                int cntX = nPodSizeX_px / nCamWidth;
                int cntY = nPodSizeY_px / nCamHeight;
                
                for (int i = 0; i < cntX; i++)
                {
                    if (m_module.Run(axisZ.StartMove(dAxisStartPosZ)))
                        return p_sInfo;
                    if (m_module.Run(axisZ.WaitReady()))
                        return p_sInfo;

                    if (m_module.Run(axisXY.p_axisX.StartMove(dStartPosX - i * dCamWidthtoPulse)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;

                    for (int j = 0; j < cntY; j++)
                    {
                        if (m_module.Run(axisZ.StartMove(dAxisStartPosZ)))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        if (m_module.Run(axisXY.p_axisY.StartMove(dStartPosY - j * dCamHeighttoPulse)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;


                       axisZ.SetTrigger(dTriggerStartPosZ, dTriggerEndPosZ, Convert.ToInt32(dstep), 20, true);
                        //if (m_module.Run(m_module.Move(mainOpt.m_axisZ, dPosZ)))
                        //    return p_sInfo;
                        ZStackGrabMode.StartZGrab(mem, nstep, new CPoint(nCamWidth * i, nCamHeight * j)); //여기선 진자 찍는것만

                        if (m_module.Run(axisZ.StartMove(dAxisEndPosZ, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        axisZ.RunTrigger(false);
                        ZStackGrabMode.m_camera.StopGrab();
                    }
                }
                
                for(int i=0;i<nstep;i++)
                    VignetteFilttering((byte*)mem.GetPtr(i).ToPointer(), nCamWidth, nCamHeight, cntX, cntY, mem.W);

                fs.Run(nCamWidth, nCamHeight, cntX, cntY);
                return "OK";
            }
            finally
            {
                ZStackGrabMode.SetLight(false);
                axisZ.RunTrigger(false);
                ZStackGrabMode.m_camera.StopGrab();
            }
        }

        unsafe void VignetteFilttering(byte* ptr,int width,int height,int xCnt,int yCnt,long memWidth)
        {
            VignetteFilter vignette = new VignetteFilter(width, height, Power, Radius); //필터만들어주는애
            vignette.GenerateGradient();

            for (int x = 0; x < xCnt; x++)
                for (int y = 0; y < yCnt; y++)
                    for (int i = 0; i < height; i++)
                        for (int j = 0; j < width; j++)
                        {
                            double* Maskptr = (double*)vignette.Mask.DataPointer.ToPointer();
                            int curY = height * y + i;
                            int curX = width * x + j;
                            int cur = (int)(curY * memWidth + curX);
                            int maskcur = i * width + j;
                            double res = (ptr[cur] / Maskptr[maskcur]) > 255 ? 255 : (ptr[cur] / Maskptr[maskcur]); //255보다 크면 이미지 이상해져서 clamping
                            ptr[cur] = (byte)res;
                        }
        }
    }
}


/*하나짜리
        public override string Run()
        {
            if (p_sZStackGrabMode == null) return "Grab Mode : ZStack Grab == Null";
            try
            {
                ZStackGrabMode.SetLight(true);

                AxisXY axisXY = m_module.m_stage.m_axisXY;
                Axis axisZ = mainOpt.m_axisZ;

                double dTriggerStartPosZ = nstartPos;
                double dTriggerEndPosZ = nendPos;

                if (m_module.Run(axisXY.StartMove(new RPoint(ZStackGrabMode.m_rpAxisCenter.X, ZStackGrabMode.m_rpAxisCenter.Y))))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;

                if (m_module.Run(axisZ.StartMove(dTriggerStartPosZ)))
                    return p_sInfo;
                if (m_module.Run(axisZ.WaitReady()))
                    return p_sInfo;

                //InfoPod.ePod parts = m_module.p_infoPod.p_ePod;
                //Vision.eUpDown upDown = (Vision.eUpDown)Enum.ToObject(typeof(Vision.eUpDown), m_module.p_infoPod.p_bTurn);
                //MemoryData mem = mainOpt.GetMemoryData(parts, Vision.MainOptic.eInsp.Stack, upDown);

                MemoryData mem = mainOpt.GetMemoryData(InfoPod.ePod.EIP_Cover, Vision.MainOptic.eInsp.Stack, Vision.eUpDown.Front);
                nstep = mem.p_nCount - 1;
                double dstep = Math.Abs(nendPos - nstartPos) / nstep;
                axisZ.SetTrigger(dTriggerStartPosZ, dTriggerEndPosZ, dstep, true);

                FocusStacking_new fs = new FocusStacking_new(mem);
                int nCamWidth = ZStackGrabMode.m_camera.GetRoiSize().X;
                int nCamHeight = ZStackGrabMode.m_camera.GetRoiSize().Y;

                //ZStackGrabMode.StartZGrab(mem, nstep); //여기선 진자 찍는것만

                //double dstep = Math.Abs(nendPos - nstartPos) / nstep;  //area camera

                nstep = mem.p_nCount - 1;
                for (int step = 0; step < nstep; step++)
                {
                    double dPosZ = nstartPos + step * dstep;
                    //if (m_module.Run(m_module.Move(mainOpt.m_axisZ, dPosZ)))
                    //    return p_sInfo;
                    if (m_module.Run(axisZ.StartMove(dPosZ)))
                        return p_sInfo;
                    if (m_module.Run(axisZ.WaitReady()))
                        return p_sInfo;

                    IntPtr ptr = mem.GetPtr(step);
                    camZStack.LiveGrab();

                    Parallel.For(0, nCamHeight, (i) =>
                    {
                        Marshal.Copy(camZStack.m_ImageLive.m_aBuf, i * nCamWidth, (IntPtr)((long)ptr + (i * mem.W)), nCamWidth);
                    });

                    Thread.Sleep(100);
                } //여기가 속주석으 끝

                fs.Run(nCamWidth, nCamHeight);
            }
            finally
            {
                ZStackGrabMode.SetLight(false);
            }
            return "OK";
        }
 
 */