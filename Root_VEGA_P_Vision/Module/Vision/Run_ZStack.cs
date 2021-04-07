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
    class Run_ZStack:ModuleRunBase
    {
        Vision m_module;
        Vision.MainOptic mainOpt;
        GrabMode ZStackGrabMode;
        Camera_Matrox camZStack;
        string sZStackGrabMode;
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

        string p_sZStackGrabMode
        {
            get { return sZStackGrabMode; }
            set
            {
                sZStackGrabMode = value;
                ZStackGrabMode = m_module.GetGrabMode(value);
            }
        }

        public override ModuleRunBase Clone()
        {
            Run_ZStack run = new Run_ZStack(m_module);
            run.p_sZStackGrabMode = p_sZStackGrabMode;
            run.nstartPos = nstartPos;
            run.nendPos = nendPos;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sZStackGrabMode = tree.Set(p_sZStackGrabMode, p_sZStackGrabMode, m_module.p_asGrabMode, "Grab Mode : ZStack Grab", "Select GrabMode", bVisible);
            nstartPos = tree.Set(nstartPos, nstartPos, "Stacking Start Z", "Stacking Start Z Position",bVisible);
            nendPos = tree.Set(nendPos, nendPos, "Stacking End Z", "Stacking End Z Position",bVisible);
        }

        public override string Run()
        {
            if (p_sZStackGrabMode == null) return "Grab Mode : ZStack Grab == Null";
            try
            {
                //AxisXY axisXY = m_module.m_stage.m_axisXY;
                //Axis axisZ = mainOpt.m_axisZ;

                //ZStackGrabMode.SetLight(true);

                //if (m_module.Run(axisXY.StartMove(new RPoint(ZStackGrabMode.m_rpAxisCenter.X, ZStackGrabMode.m_rpAxisCenter.Y))))
                //    return p_sInfo;
                //if (m_module.Run(axisXY.WaitReady()))
                //    return p_sInfo;

                ////InfoPod.ePod parts = m_module.p_infoPod.p_ePod;
                ////Vision.eUpDown upDown = (Vision.eUpDown)Enum.ToObject(typeof(Vision.eUpDown), m_module.p_infoPod.p_bTurn);
                ////MemoryData mem = mainOpt.GetMemoryData(parts, Vision.MainOptic.eInsp.Stack, upDown);

                MemoryData mem = mainOpt.GetMemoryData(InfoPod.ePod.EIP_Cover, Vision.MainOptic.eInsp.Stack, Vision.eUpDown.Front);

                FocusStacking_new fs = new FocusStacking_new(mem);
                int nCamWidth = ZStackGrabMode.m_camera.GetRoiSize().X;
                int nCamHeight = ZStackGrabMode.m_camera.GetRoiSize().Y;
                //nstep = mem.p_nCount-1;

                //double dstep = Math.Abs(nendPos - nstartPos)/nstep;
                //for (int step=0;step<nstep;step++)
                //{
                //    double dPosZ = nstartPos + step * dstep;
                //    //if (m_module.Run(m_module.Move(mainOpt.m_axisZ, dPosZ)))
                //    //    return p_sInfo;
                //    if (m_module.Run(axisZ.StartMove(dPosZ)))
                //        return p_sInfo;
                //    if (m_module.Run(axisZ.WaitReady()))
                //        return p_sInfo;

                //    IntPtr ptr = mem.GetPtr(step);
                //    camZStack.LiveGrab();

                //    Parallel.For(0, nCamHeight, (i) => {
                //        Marshal.Copy(camZStack.m_ImageLive.m_aBuf, i* nCamWidth, (IntPtr)((long)ptr + (i * mem.W)), nCamWidth);
                //    });

                //    Thread.Sleep(100);
                //}

                fs.Run(nCamWidth, nCamHeight);
            }
            finally
            {
                ZStackGrabMode.SetLight(false);
            }
            return "OK";
        }
    }
}
