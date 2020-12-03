

using RootTools;
using RootTools.Camera;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.RADS;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Root_WIND2.Module
{
    public class Vision : ModuleBase
    {
        #region ToolBox
        Axis m_axisRotate;
        Axis m_axisZ;
        AxisXY m_axisXY;
        DIO_O m_doVac;
        DIO_O m_doBlow;
        MemoryPool m_memoryPool;
        MemoryPool m_memoryPool2;
        MemoryGroup m_memoryGroup;
        MemoryGroup m_memoryGroup2;
        MemoryData m_memoryMain;
        LightSet m_lightSet;
        RADSControl m_RADSControl;

        Camera_Dalsa m_CamMain;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Axis Rotate");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "Axis Z");
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "Axis XY");
            p_sInfo = m_toolBox.Get(ref m_doVac, this, "Stage Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doBlow, this, "Stage Blow");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory",1);
            p_sInfo = m_toolBox.Get(ref m_memoryPool2, this, "pool", 1, true);
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.Get(ref m_RADSControl, this, "RADSControl", false);
            p_sInfo = m_toolBox.Get(ref m_CamMain, this, "MainCam");
            m_axisRotate.StartMove(1000);
        }
        #endregion

        #region Grab Mode
        int m_lGrabMode = 0;
        public ObservableCollection<GrabMode> m_aGrabMode = new ObservableCollection<GrabMode>();
        public List<string> p_asGrabMode
        {
            get
            {
                List<string> asGrabMode = new List<string>();
                foreach (GrabMode grabMode in m_aGrabMode) asGrabMode.Add(grabMode.p_sName);
                return asGrabMode;
            }
        }

        public GrabMode GetGrabMode(string sGrabMode)
        {
            foreach (GrabMode grabMode in m_aGrabMode)
            {
                if (sGrabMode == grabMode.p_sName) return grabMode;
            }
            return null;
        }

        void RunTreeGrabMode(Tree tree)
        {
            m_lGrabMode = tree.Set(m_lGrabMode, m_lGrabMode, "Count", "Grab Mode Count");
            while (m_aGrabMode.Count < m_lGrabMode)
            {
                string id = "Mode." + m_aGrabMode.Count.ToString("00");
                GrabMode grabMode = new GrabMode(id, m_cameraSet, m_lightSet, m_memoryPool, m_RADSControl);
                m_aGrabMode.Add(grabMode);
            }
            while (m_aGrabMode.Count > m_lGrabMode) m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
            foreach (GrabMode grabMode in m_aGrabMode) grabMode.RunTreeName(tree.GetTree("Name", false));
            foreach (GrabMode grabMode in m_aGrabMode) grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
        }
        #endregion

        #region DIO
        public bool p_bStageVac
        {
            get
            {
                return m_doVac.p_bOut;
            }
            set
            {
                if (m_doVac.p_bOut == value)
                    return;
                m_doVac.Write(value);
            }
        }

        public bool p_bStageBlow
        {
            get
            {
                return m_doBlow.p_bOut;
            }
            set
            {
                if (m_doBlow.p_bOut == value)
                    return;
                m_doBlow.Write(value);
            }
        }

        public void RunBlow(int msDelay)
        {
            m_doBlow.DelayOff(msDelay);
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }

        public override void InitMemorys()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryMain = m_memoryGroup.CreateMemory("Main", 1, 1, 1000, 1000);
            m_memoryGroup2= m_memoryPool2.GetGroup("group");
            m_memoryGroup2.CreateMemory("mem", 3, 1, 40000, 40000);
            m_memoryGroup2.CreateMemory("ROI", 1, 4, 30000,30000); // Chip 크기 최대 30,000 * 30,000 고정 Origin ROI 메모리 할당 20.11.02 JTL 
        }

        #endregion

        #region Axis
        public int m_pulseRound = 1000;
        void RunTreeAxis(Tree tree)
        {
            m_pulseRound = tree.Set(m_pulseRound, m_pulseRound, "Rotate Pulse / Round", "Rotate Axis Pulse / 1 Round (pulse)");
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
                return "OK";
            //            p_bStageBlow = false;
            //            p_bStageVac = true;
            Thread.Sleep(200);
            if (m_CamMain != null && m_CamMain.p_CamInfo.p_eState == RootTools.Camera.Dalsa.eCamState.Init)
                m_CamMain.Connect();
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            //p_bStageVac = false;
            return "OK";
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeAxis(tree.GetTree("Axis", false));
            RunTreeGrabMode(tree.GetTree("Grab Mode", false));
        }
        #endregion

        public Vision(string id, IEngineer engineer)
        {
            //            InitLineScan();
            //            InitAreaScan();
            base.InitBase(id, engineer);
            //            InitMemory();
        }

        public override void ThreadStop()
        {
            //
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            AddModuleRunList(new Run_Rotate(this), false, "Rotate Axis");
            AddModuleRunList(new Run_GrabLineScan(this), false, "Run Grab LineScan Camera");
        }

        public class Run_Delay : ModuleRunBase
        {
            Vision m_module;
            public Run_Delay(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return "OK";
            }
        }

        public class Run_Rotate : ModuleRunBase
        {
            Vision m_module;
            public Run_Rotate(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_fDeg = 0;
            public override ModuleRunBase Clone()
            {
                Run_Rotate run = new Run_Rotate(m_module);
                run.m_fDeg = m_fDeg;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fDeg = tree.Set(m_fDeg, m_fDeg, "Degree", "Rotation Degree (0 ~ 360)", bVisible);
            }

            public override string Run()
            {
                int pulseRound = m_module.m_pulseRound;
                Axis axis = m_module.m_axisRotate;
                int pulse = (int)Math.Round(m_module.m_pulseRound * m_fDeg / 360);
                while (pulse < axis.p_posCommand)
                    pulse += pulseRound;
                {
                    axis.p_posCommand -= pulseRound;
                    axis.p_posActual -= pulseRound;
                }
                if (m_module.Run(axis.StartMove(pulse)))
                    return p_sInfo;
                return axis.WaitReady();
            }
        }

        public class Run_GrabLineScan : ModuleRunBase
        {
            Vision m_module;
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
            public string p_sGrabMode
            {
                get { return m_sGrabMode; }
                set
                {
                    m_sGrabMode = value;
                    m_grabMode = m_module.GetGrabMode(value);
                }
            }

            public Run_GrabLineScan(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_GrabLineScan run = new Run_GrabLineScan(m_module);
                run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
                run.m_cpMemoryOffset = new CPoint(m_cpMemoryOffset);
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                run.m_nFocusPosZ = m_nFocusPosZ;
                run.m_nWaferSize_mm = m_nWaferSize_mm;
                run.m_nMaxFrame = m_nMaxFrame;
                run.m_nScanRate = m_nScanRate;
                run.p_sGrabMode = p_sGrabMode;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
                m_cpMemoryOffset = tree.Set(m_cpMemoryOffset, m_cpMemoryOffset, "Memory Offset", "Grab Start Memory Position (px)", bVisible);
                m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Cam X Resolution", "X Resolution (um)", bVisible);
                m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Cam Y Resolution", "Y Resolution (um)", bVisible);
                m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);
                m_nWaferSize_mm = tree.Set(m_nWaferSize_mm, m_nWaferSize_mm, "Wafer Size Y", "Wafer Size Y", bVisible);
                m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
                m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
                if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
            }

            public override string Run()
            {
                if (m_grabMode == null) return "Grab Mode == null";

                try
                {
                    m_grabMode.SetLight(true);

                    AxisXY axisXY = m_module.m_axisXY;
                    Axis axisZ = m_module.m_axisZ;
                    Axis axisRotate = m_module.m_axisRotate;
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
                        int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * m_grabMode.m_camera.GetRoiSize().Y * (double)m_nScanRate / 100);

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
            }
        }
        #endregion
    }
}
