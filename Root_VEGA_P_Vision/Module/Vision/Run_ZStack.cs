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

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            p_sZStackGrabMode = tree.Set(p_sZStackGrabMode, p_sZStackGrabMode, m_module.p_asGrabMode, "Grab Mode : ZStack Grab", "Select GrabMode", bVisible);
            nstartPos = tree.Set(nstartPos, nstartPos, "Stacking Start Z", "Stacking Start Z Position");
            nendPos = tree.Set(nendPos, nendPos, "Stacking End Z", "Stacking End Z Position");
        }

        public override string Run()
        {
            if (p_sZStackGrabMode == null) return "Grab Mode : ZStack Grab == Null";
            try
            {
                ZStackGrabMode.SetLight(true);
                InfoPod.ePod parts = m_module.p_infoPod.p_ePod;
                Vision.eUpDown upDown = (Vision.eUpDown)Enum.ToObject(typeof(Vision.eUpDown), m_module.p_infoPod.p_bTurn);
                MemoryData mem = mainOpt.GetMemoryData(parts, Vision.MainOptic.eInsp.Stack, upDown);
                FocusStacking_new fs = new FocusStacking_new(mem);
                int nCamWidth = ZStackGrabMode.m_camera.GetRoiSize().X;
                int nCamHeight = ZStackGrabMode.m_camera.GetRoiSize().Y;
                nstep = mem.p_nCount-1;

                double dstep = (nendPos - nstartPos)/nstep;
                for (int step=0;step<nstep;step++)
                {
                    double dPosZ = nstartPos + step * dstep;
                    if (m_module.Run(m_module.Move(mainOpt.m_axisZ, dPosZ)))
                        return p_sInfo;

                    IntPtr ptr = mem.GetPtr(step);
                    camZStack.LiveGrab();

                    Parallel.For(0, nCamHeight, (i) => {
                        Marshal.Copy(camZStack.p_aBuf, 0, (IntPtr)((long)ptr + (i * mem.W)), nCamWidth);
                    });
                }

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
