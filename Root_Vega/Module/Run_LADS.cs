using System;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;

namespace Root_Vega.Module
{
    public class Run_LADS : ModuleRunBase
    {
        SideVision m_module;

        public RPoint m_rpAxis = new RPoint();
        public double m_fRes = 1;       //단위 um
        public int m_nFocusPos = 0;
        public CPoint m_cpMemory = new CPoint();
        public int m_nScanGap = 1000;
        public int m_yLine = 1000;  // Y축 Reticle Size
        public int m_xLine = 1000;  // X축 Reticle Size
        public int m_nMaxFrame = 100;  // Camera max Frame 스펙
        public int m_nScanRate = 100;   // Camera Frame Spec 사용률 ? 1~100 %
        public GrabMode m_grabMode = null;
        string _sGrabMode = "LADS";
        string p_sGrabMode
        {
            get
            {
                return _sGrabMode;
            }
            set
            {
                _sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }

        public Run_LADS(SideVision module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_LADS run = new Run_LADS(m_module);
            run.m_fRes = m_fRes;
            run.m_nFocusPos = m_nFocusPos;
            run.m_rpAxis = new RPoint(m_rpAxis);
            run.m_cpMemory = new CPoint(m_cpMemory);
            run.m_yLine = m_yLine;
            run.m_xLine = m_xLine;
            run.m_nMaxFrame = m_nMaxFrame;
            run.m_nScanRate = m_nScanRate;
            run.m_nScanGap = m_nScanGap;

            return run;
        }

        public void RunTree(TreeRoot treeRoot, Tree.eMode mode)
        {
            treeRoot.p_eMode = mode;
            RunTree(treeRoot, true);
        }
        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            //tree.p_bVisible = bVisible;
            m_rpAxis = tree.Set(m_rpAxis, m_rpAxis, "Center Axis Position", "Center Axis Position (mm ?)", bVisible);
            m_fRes = tree.Set(m_fRes, m_fRes, "Cam Resolution", "Resolution  um", bVisible);
            m_nFocusPos = tree.Set(m_nFocusPos, 0, "Focus Z Pos", "Focus Z Pos", bVisible);
            m_cpMemory = tree.Set(m_cpMemory, m_cpMemory, "Memory Position", "Grab Start Memory Position (pixel)", bVisible);
            m_nScanGap = tree.Set(m_nScanGap, m_nScanGap, "Scan Gab", "Scan 방향간의 Memory 상 Gab (Bottom, Left 간의 Memory 위치 차이)", bVisible);
            m_yLine = tree.Set(m_yLine, m_yLine, "Reticle YSize", "# of Grab Lines", bVisible);
            m_xLine = tree.Set(m_xLine, m_xLine, "Reticle XSize", "# of Grab Lines", bVisible);
            //m_eScanPos = (eScanPos)tree.Set(m_eScanPos, m_eScanPos, "Scan 위치", "Scan 위치, 0 Position 이 Bottom", bVisible);
            m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
            m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            if (m_grabMode != null)
                m_grabMode.RunTree(tree.GetTree("Grab Mode", false, bVisible), bVisible, true);
            //base.RunTree(tree, bVisible, bRecipe);
        }

        public override string Run()
        {
            if (m_grabMode == null)
                return "Grab Mode == null";

            Camera_Basler cam = m_module.p_CamLADS;
            ImageData img = cam.p_ImageViewer.p_ImageData;
            AxisXY axisXY = m_module.p_axisXY;
            Axis axisZ = m_module.p_axisZ;
            Axis axisTheta = m_module.p_axisTheta;

            try
            {
                int nScanLine = 0;
                m_grabMode.SetLight(true);
                m_module.SetLightByName("LADS", 100);
                m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_fRes) * cam.GetRoiSize().Y;        // 축해상도 0.1um로 하드코딩. 트리거 발생 주기.
                int nLinesY = Convert.ToInt32(m_yLine * 1000 / m_fRes);      // Grab 할 총 Line 갯수.
                int nLinesX = Convert.ToInt32(m_xLine * 1000 / m_fRes);      // Grab 할 총 Line 갯수.
                m_cpMemory.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X;
                //m_cpMemory.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X + (int)m_eScanPos * (nLinesX + m_nScanGap);

                if (EQ.IsStop())
                    return "OK";

                double yAxis = m_grabMode.m_dTrigger * nLinesY;     // 총 획득할 Image Y  
                /*왼쪽에서 오른쪽으로 찍는것을 정방향으로 함, 즉 Y 축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향*/
                /* Grab하기 위해 이동할 Y축의 시작 끝 점*/
                double yPos1 = m_rpAxis.Y - yAxis / 2 - m_grabMode.m_intervalAcc;   //y 축 이동 시작 지점 
                double yPos0 = m_rpAxis.Y + yAxis / 2 + m_grabMode.m_intervalAcc;  // Y 축 이동 끝 지점.
                double nPosX = m_rpAxis.X;   // X축 찍을 위치 
                double nPosZ = m_nFocusPos + nLinesX * m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * m_grabMode.m_dTrigger; //해상도추가필요
                //double nPosZ = m_nFocusPos;
                //double nPosX = m_rpAxis.X + nLines * m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * m_grabMode.m_dTrigger; //해상도추가필요
                double fPosTheta = axisTheta.GetPosValue(SideVision.eAxisPosTheta.Snap) + (int)m_grabMode.m_eScanPos * 360000 / 4;

                //m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

                if (m_module.Run(axisXY.p_axisX.StartMove(-50000)))
                    return p_sInfo;
                if (m_module.Run(axisXY.p_axisX.WaitReady()))
                    return p_sInfo;
                if (m_module.Run(axisTheta.StartMove(fPosTheta)))
                    return p_sInfo;
                if (m_module.Run(axisTheta.WaitReady()))
                    return p_sInfo;
                if (m_module.Run(axisZ.StartMove(nPosZ)))
                    return p_sInfo;
                if (m_module.Run(axisZ.WaitReady()))
                    return p_sInfo;
                if (m_module.Run(axisXY.StartMove(new RPoint(nPosX, yPos0))))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;
                //if (m_module.Run(axisXY.p_axisX.WaitReady()))
                //    return p_sInfo;
                //if (m_module.Run(axisXY.p_axisY.WaitReady()))
                //    return p_sInfo;

                /* Trigger Set*/
                double yTrigger0 = m_rpAxis.Y - yAxis / 2;
                double yTrigger1 = m_rpAxis.Y + yAxis / 2;
                m_module.p_axisXY.p_axisY.SetTrigger(yPos1, yTrigger1 + 100000, m_grabMode.m_dTrigger, true);

                string sPool = "pool";
                string sGroup = "group";
                string sMem = "mem";
                MemoryData mem = m_module.m_engineer.ClassMemoryTool().GetMemory(sPool, sGroup, sMem);

                int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * m_grabMode.m_camera.GetRoiSize().Y * (double)m_nScanRate / 100);
                /* 방향 바꾸는 코드 들어가야함*/
                m_grabMode.StartGrab(mem, m_cpMemory, nLinesY);
                if (m_module.Run(axisXY.p_axisY.StartMove(yPos1, nScanSpeed)))
                    return p_sInfo;
                if (m_module.Run(axisXY.p_axisY.WaitReady()))
                    return p_sInfo;
                axisXY.p_axisY.RunTrigger(false);

                nScanLine++;
                m_cpMemory.X += m_grabMode.m_camera.GetRoiSize().X;

                return "OK";
            }
            finally
            {
                axisXY.p_axisY.RunTrigger(false);
                m_grabMode.SetLight(false);
            }
        }
    }
}