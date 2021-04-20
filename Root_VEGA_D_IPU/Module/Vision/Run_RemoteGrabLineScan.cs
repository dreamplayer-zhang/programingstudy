using RootTools;
using RootTools.Camera;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Database;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_VEGA_D_IPU.Module
{
    public class Run_RemoteGrabLineScan : ModuleRunBase
    {
        Vision_IPU m_module;
        
        //bool m_bInvDir = false;
        public GrabMode m_grabMode = null;
        double m_dTDIToVRSOffsetZ = 0;
        string m_sGrabMode = "";
        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }

        public Run_RemoteGrabLineScan(Vision_IPU module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_RemoteGrabLineScan run = new Run_RemoteGrabLineScan(m_module);
            run.p_sGrabMode = p_sGrabMode;
            run.m_dTDIToVRSOffsetZ = m_dTDIToVRSOffsetZ;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        { 
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            m_dTDIToVRSOffsetZ = tree.Set(m_dTDIToVRSOffsetZ, m_dTDIToVRSOffsetZ,"TDI To VRS Offset Z", "TDI To VRS Offset Z", bVisible);
            //if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
        }

        public override string Run()
        {
            if (m_grabMode == null) return "Grab Mode == null";

            // StopWatch 설정
            StopWatch snapTimeWatcher = new StopWatch();
            snapTimeWatcher.Start();

            int nScanLine = 0;

            CPoint cpMemoryOffset = new CPoint(m_grabMode.m_cpMemoryOffset);

            string strPool = m_grabMode.m_memoryPool.p_id;
            string strGroup = m_grabMode.m_memoryGroup.p_id;
            string strMemory = m_grabMode.m_memoryData.p_id;
            MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

            try
            {
                while (m_grabMode.m_ScanLineNum > nScanLine)
                {
                    // Grab Thread 생성
                    m_grabMode.StartGrab(mem, cpMemoryOffset, m_grabMode.m_nRemoteGrabLine, m_grabMode.m_GD);

                    // 라인 스캔 끝나기까지 대기
                    Camera_Dalsa camera = (Camera_Dalsa)m_grabMode.m_camera;
                    //while (!camera.p_GrabThread.Join(10) || camera.p_CamInfo.p_eState == eCamState.GrabMem)
                    //{
                    //}

                    // 라인 개수 증가 및 메모리 위치 옵셋 값 증가
                    nScanLine++;
                    cpMemoryOffset.X += m_grabMode.m_GD.m_nFovSize;
                }

                m_grabMode.m_camera.StopGrab();

                snapTimeWatcher.Stop();

                // Log
                TempLogger.Write("Snap", string.Format("{0:F3}", (double)snapTimeWatcher.ElapsedMilliseconds / (double)1000));

                return "OK";
            }
            catch (Exception e)
            {
                m_log.Info(e.Message);
                return "OK";
            }
            finally
            {
            }
        }
    }
}
