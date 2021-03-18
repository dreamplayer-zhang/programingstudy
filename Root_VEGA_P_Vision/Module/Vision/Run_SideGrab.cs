using RootTools;
using RootTools.Camera;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision.Module
{
    class Run_SideGrab:ModuleRunBase
    {
        Vision m_module;
        Vision.SideOptic side;
        GrabMode SidegrabMode;
        string sSideGrabMode;
        public Run_SideGrab(Vision module)
        {
            m_module = module;
            side = m_module.m_sideOptic;
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
            AxisXY axisXY = m_module.axisXY;
            Axis axisZ = side.axisZ;

            CPoint cpMemoryOffset = new CPoint(SidegrabMode.m_cpMemoryOffset);

            int nScanLine = 0;
            int nMMPerUM = 1000;
            int nCamWidth = SidegrabMode.m_camera.GetRoiSize().X;
            int nCamHeight = SidegrabMode.m_camera.GetRoiSize().Y;
            SidegrabMode.m_dTrigger = Convert.ToInt32(10 * SidegrabMode.m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
            double dXScale = SidegrabMode.m_dResX_um * 10; //resolution to pulse
            int nWaferSizeY_px = Convert.ToInt32(SidegrabMode.m_nPodSize_mm * nMMPerUM / SidegrabMode.m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
            int nTotalTriggerCount = Convert.ToInt32(SidegrabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간의 Trigger pulse
            int nScanSpeed = Convert.ToInt32((double)SidegrabMode.m_nMaxFrame * SidegrabMode.m_dTrigger * nCamHeight * SidegrabMode.m_nScanRate / 100);
            int nScanOffset_pulse = 40000;

            string strPool = SidegrabMode.m_memoryPool.p_id;
            string strGroup = SidegrabMode.m_memoryGroup.p_id;
            string strMemory = SidegrabMode.m_memoryData.p_id;



            return "OK";
        }

        private void GrabMode_Grabed(object sender, EventArgs e)
        {
            GrabedArgs ga = (GrabedArgs)e;
            m_module.p_nProgress = ga.nProgress;
        }
    }
}
