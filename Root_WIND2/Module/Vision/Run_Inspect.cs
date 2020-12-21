using RootTools;
using RootTools.Camera;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.Module
{
    public class Run_Inspect : ModuleRunBase
    {
        Vision m_module;

        string m_sRecipeName = string.Empty;

        // Grab 관련 파라매터 (이거 나중에 구조 변경 필요할듯)
        public RPoint m_rpAxisCenter = new RPoint();    // Wafer Center Position
        public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
        public double m_dResX_um = 1;                   // Camera Resolution X
        public double m_dResY_um = 1;                   // Camera Resolution Y
        public int m_nFocusPosZ = 0;                    // Focus Position Z
        public int m_nWaferSize_mm = 1000;              // Wafer Size (mm)
        public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
        public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
        bool m_bInvDir = false;
        public GrabMode m_grabMode = null;
        string m_sGrabMode = "";

        InspectionManager_Vision inspectionVision;

        #region [Getter Setter]
        public string RecipeName 
        { 
            get => m_sRecipeName;
            set => m_sRecipeName = value;
        }

        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }

        public InspectionManager_Vision InspectionVision 
        {  
            get => inspectionVision;
            set => inspectionVision = value;
        }
        #endregion

        public Run_Inspect(Vision module)
        {
            m_module = module;
            inspectionVision = ((WIND2_Engineer)module.m_engineer).InspectionVision;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_Inspect run = new Run_Inspect(m_module);
            run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
            run.m_cpMemoryOffset = new CPoint(m_cpMemoryOffset);
            run.m_dResX_um = m_dResX_um;
            run.m_dResY_um = m_dResY_um;
            run.m_nFocusPosZ = m_nFocusPosZ;
            run.m_nWaferSize_mm = m_nWaferSize_mm;
            run.m_nMaxFrame = m_nMaxFrame;
            run.m_nScanRate = m_nScanRate;
            run.p_sGrabMode = p_sGrabMode;

            run.InspectionVision = ProgramManager.Instance.InspectionVision;

            run.RecipeName = this.RecipeName;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_sRecipeName = tree.SetFile(m_sRecipeName, m_sRecipeName, "rcp", "Recipe", "Recipe Name", bVisible);
            m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
            m_cpMemoryOffset = tree.Set(m_cpMemoryOffset, m_cpMemoryOffset, "Memory Offset", "Grab Start Memory Position (px)", bVisible);
            m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Cam X Resolution", "X Resolution (um)", bVisible);
            m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Cam Y Resolution", "Y Resolution (um)", bVisible);
            m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);
            m_nWaferSize_mm = tree.Set(m_nWaferSize_mm, m_nWaferSize_mm, "Wafer Size Y", "Wafer Size Y", bVisible);
            m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
            m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
        }

        public override string Run()
        {
            if (m_grabMode == null) return "Grab Mode == null";

            if(this.inspectionVision.Recipe.Read(m_sRecipeName, true) == false)
                return "Recipe Open Fail";

            if (this.inspectionVision.CreateInspection() == false)
                return "Create Inspection Fail";

            this.inspectionVision.Start(true);

            /// Snap Start (이거 나중에 구조 변경 필요할듯...)
            try
            {
                m_grabMode.SetLight(true);

                AxisXY axisXY = m_module.AxisXY;
                Axis axisZ = m_module.AxisZ;
                Axis axisRotate = m_module.AxisRotate;
                CPoint cpMemoryOffset = new CPoint(m_cpMemoryOffset);
                int nScanLine = 0;
                int nMMPerUM = 1000;

                double dXScale = m_dResX_um * 10;
                cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X;
                m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                int nWaferSizeY_px = Convert.ToInt32(m_nWaferSize_mm * nMMPerUM / m_dResY_um);  // 웨이퍼 영역의 Y픽셀 갯수
                int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nWaferSizeY_px);   // 스캔영역 중 웨이퍼 스캔 구간에서 발생할 Trigger 갯수
                int nScanOffset_pulse = 30000;

                int startOffsetX = cpMemoryOffset.X;
                int startOffsetY = 0;

                while (m_grabMode.m_ScanLineNum > nScanLine)
                {
                    if (EQ.IsStop())
                        return "OK";

                    // 위에서 아래로 찍는것을 정방향으로 함, 즉 Y축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향
                    // Grab하기 위해 이동할 Y축의 시작 끝 점
                    double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                    double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                    m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                    if (m_grabMode.m_bUseBiDirectionScan && Math.Abs(axisXY.p_axisY.p_posActual - dStartPosY) > Math.Abs(axisXY.p_axisY.p_posActual - dEndPosY))
                    {
                        double dTemp = dStartPosY;  // dStartPosY <--> dEndPosY 바꿈.
                        dStartPosY = dEndPosY;
                        dEndPosY = dTemp;
                        m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                    }

                    double dPosX = m_rpAxisCenter.X + nWaferSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * dXScale;

                    if (m_module.Run(axisZ.StartMove(m_nFocusPosZ)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                    if (m_module.Run(axisZ.WaitReady()))
                        return p_sInfo;

                    double dTriggerStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2;
                    double dTriggerEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2;
                    axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

                    string strPool = m_grabMode.m_memoryPool.p_id;
                    string strGroup = m_grabMode.m_memoryGroup.p_id;
                    string strMemory = m_grabMode.m_memoryData.p_id;

                    MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                    int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger *
                        m_grabMode.m_camera.GetRoiSize().Y * (double)m_nScanRate / 100);

                    m_grabMode.StartGrab(mem, cpMemoryOffset, nWaferSizeY_px, m_grabMode.m_eGrabDirection == eGrabDirection.BackWard);
                    //m_grabMode.StartGrabColor(mem, cpMemoryOffset, nWaferSizeY_px, m_grabMode.m_eGrabDirection == eGrabDirection.BackWard);

                    if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                        return p_sInfo;
                    if (m_module.Run(axisXY.WaitReady()))
                        return p_sInfo;
                    axisXY.p_axisY.RunTrigger(false);

                    WIND2EventManager.OnSnapDone(this, new SnapDoneArgs(new CPoint(startOffsetX, startOffsetY), cpMemoryOffset + new CPoint(m_grabMode.m_camera.GetRoiSize().X, nWaferSizeY_px)));

                    nScanLine++;
                    cpMemoryOffset.X += m_grabMode.m_camera.GetRoiSize().X;
                }
                m_grabMode.m_camera.StopGrab();
                return "OK";
            }
            finally
            {
                m_grabMode.SetLight(false);
            }


            return "OK";
        }
    }
}
