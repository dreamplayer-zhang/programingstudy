﻿using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Root_EFEM;
using Root_EFEM.Module;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Control.Ajin;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;
using static RootTools.Control.Axis;

namespace Root_AOP01_Inspection.Module
{
    public class MainVision : ModuleBase, IWTRChild
    {
        //Mem Vision Memory.Main
        #region ToolBox
        Axis m_axisRotate;
        Axis m_axisZ;
        Axis m_axisSideZ;
        AxisXY m_axisXY;
        DIO_O m_doVac;
        DIO_O m_doBlow;
        MemoryPool m_memoryPool;
        MemoryGroup m_memoryGroup;
        MemoryData m_memoryMain;
        MemoryData m_memorySideLeft;
        MemoryData m_memorySideRight;
        MemoryData m_memorySideTop;
        MemoryData m_memorySideBottom;
        MemoryData m_memoryTDI45;
        MemoryData m_memoryLADS;

        LightSet m_lightSet;
        Camera_Dalsa m_CamTDI90;
        Camera_Dalsa m_CamTDI45;
        Camera_Dalsa m_CamTDISide;
        Camera_Basler m_CamLADS;

        class LADSInfo//한 줄에 대한 정보
        {
            public double[] m_Heightinfo;
            public RPoint axisPos;//시작점의 x,y
            public double endYPos;//끝점의 y 정보

            LADSInfo() { }
            public LADSInfo(RPoint _axisPos,double _endYPos,int arrcap/*heightinfo capacity*/)
            {
                axisPos = _axisPos;
                endYPos = _endYPos;
                m_Heightinfo = new double[arrcap];
            }
        }

        static List<LADSInfo> ladsinfos;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Axis Rotate");
            p_sInfo = m_toolBox.Get(ref m_axisSideZ, this, "Axis Side Z");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "Axis Z");
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "Axis XY");
            p_sInfo = m_toolBox.Get(ref m_doVac, this, "Stage Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doBlow, this, "Stage Blow");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Vision Memory", 1);
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.Get(ref m_CamTDI90, this, "TDI 90");
            p_sInfo = m_toolBox.Get(ref m_CamTDI45, this, "TDI 45");
            p_sInfo = m_toolBox.Get(ref m_CamTDISide, this, "TDI Side");
            p_sInfo = m_toolBox.Get(ref m_CamLADS, this, "LADS");
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
                GrabMode grabMode = new GrabMode(id, m_cameraSet, m_lightSet, m_memoryPool);
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

        public enum eAxisPos
        {
            ReadyPos,
            ScanPos,
        }

        void InitPosAlign()
        {
            m_axisZ.AddPos(Enum.GetNames(typeof(eAxisPos)));
            m_axisRotate.AddPos(Enum.GetNames(typeof(eAxisPos)));
            m_axisSideZ.AddPos(Enum.GetNames(typeof(eAxisPos)));
            m_axisXY.AddPos(Enum.GetNames(typeof(eAxisPos)));
        }

        #region InfoWafer
        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get
            {
                return _infoWafer;
            }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_reg != null)
                    m_reg.Write("sInfoWafer", m_sInfoWafer);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoWafer_Registry()
        {
            m_reg = new Registry(p_id + ".InfoWafer");
            m_sInfoWafer = m_reg.Read("sInfoWafer", m_sInfoWafer);
            p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        }
        #endregion

        #region InfoWafer UI
        InfoWaferChild_UI m_ui;
        void InitInfoWaferUI()
        {
            m_ui = new InfoWaferChild_UI();
            m_ui.Init(this);
            m_aTool.Add(m_ui);
        }
        #endregion

        #region IWTRChild
        bool _bLock = false;
        public bool p_bLock
        {
            get
            {
                return _bLock;
            }
            set
            {
                if (_bLock == value)
                    return;
                _bLock = value;
            }
        }

        bool IsLock()
        {
            for (int n = 0; n < 10; n++)
            {
                if (p_bLock == false)
                    return false;
                Thread.Sleep(100);
            }
            return true;
        }

        public List<string> p_asChildSlot
        {
            get
            {
                return null;
            }
        }

        public InfoWafer GetInfoWafer(int nID)
        {
            return p_infoWafer;
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            p_infoWafer = infoWafer;
        }

        public string IsGetOK(int nID)
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (p_infoWafer == null)
                return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (p_infoWafer != null)
                return p_id + " IsPutOK - InfoWafer Exist";
            if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false)
                return p_id + " not Enable Wafer Size";
            return "OK";
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null)
                infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        private string MoveReadyPos()
        {
            if (Run(m_axisXY.p_axisX.StartMove(eAxisPos.ReadyPos)))
                return p_sInfo;
            if (Run(m_axisXY.p_axisY.StartMove(eAxisPos.ReadyPos)))
                return p_sInfo;
            if (Run(m_axisZ.StartMove(eAxisPos.ReadyPos)))
                return p_sInfo;
            if (Run(m_axisRotate.StartMove(eAxisPos.ReadyPos)))
                return p_sInfo;

            if (Run(m_axisRotate.WaitReady()))
                return p_sInfo;
            if (Run(m_axisZ.WaitReady()))
                return p_sInfo;
            if (Run(m_axisXY.WaitReady()))
                return p_sInfo;
            return "OK";
        }

        public string BeforeGet(int nID)
        {
            string info = MoveReadyPos();
            if (info != "OK")
                return info;
            return "OK";
        }

        public string BeforePut(int nID)
        {
            string info = MoveReadyPos();
            if (info != "OK")
                return info;
            return "OK";
        }

        public string AfterGet(int nID)
        {
            return "OK";
        }

        public string AfterPut(int nID)
        {
            return "OK";
        }

        enum eCheckWafer
        {
            InfoWafer,
            Sensor
        }
        eCheckWafer m_eCheckWafer = eCheckWafer.InfoWafer;
        public bool IsWaferExist(int nID)
        {
            switch (m_eCheckWafer)
            {
                case eCheckWafer.Sensor: return false; //m_diWaferExist.p_bIn;
                default: return (p_infoWafer != null);
            }
        }

        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }
        #endregion

        #region override
        public override void InitMemorys()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            
            m_memoryMain = m_memoryGroup.CreateMemory("Main", 1, 1, 1000, 1000);
           
            m_memorySideLeft = m_memoryGroup.CreateMemory("SideLeft", 1, 1, 1000, 1000);
            m_memorySideBottom = m_memoryGroup.CreateMemory("SideBottom", 1, 1, 1000, 1000);
            m_memorySideRight = m_memoryGroup.CreateMemory("SideRight", 1, 1, 1000, 1000);
            m_memorySideTop = m_memoryGroup.CreateMemory("SideTop", 1, 1, 1000, 1000);

            m_memoryTDI45 = m_memoryGroup.CreateMemory("TDI45", 1, 1, 1000, 1000);
            m_memoryLADS = m_memoryGroup.CreateMemory("LADS", 1, 1, 1000, 1000);
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
            
            if (m_CamTDI90 != null && m_CamTDI90.p_CamInfo.p_eState == eCamState.Init)
                m_CamTDI90.Connect();
            if (m_CamTDI45 != null && m_CamTDI45.p_CamInfo.p_eState == eCamState.Init)
                m_CamTDI45.Connect();
            if (m_CamLADS.p_CamInfo._OpenStatus == false)
                m_CamLADS.Connect();
            if (m_CamTDISide != null && m_CamTDISide.p_CamInfo.p_eState == eCamState.Init)
                m_CamTDISide.Connect();

            m_axisSideZ.StartHome();
            if (m_axisSideZ.WaitReady() != "OK")
                return "Error";

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
            RunTreeSetup(tree.GetTree("Setup", false));
            RunTreeGrabMode(tree.GetTree("Grab Mode", false));
        }

        void RunTreeSetup(Tree tree)
        {
            m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Grab(this), false, "Run Grab");
            AddModuleRunList(new Run_Grab45(this), false, "Run Grab 45");
            AddModuleRunList(new Run_GrabSideScan(this), false, "Run Side Scan");
            AddModuleRunList(new Run_LADS(this), false, "Run LADS");
            AddModuleRunList(new Run_BarcodeInspection(this), false, "Run Barcode Inspection");
        }
        #endregion

        public MainVision(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            ladsinfos = new List<LADSInfo>();
            InitMemorys();
            InitPosAlign(); 
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        public class Run_GrabSideScan : ModuleRunBase
        {
            MainVision m_module;

            public RPoint m_rpAxisCenter = new RPoint();    // Side Center Position
            public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
            public double m_dResX_um = 1;                   // Camera Resolution X
            public double m_dResY_um = 1;                   // Camera Resolution Y
            double m_dDegree;                    //Rotate Degree
            public int m_nFocusPosZ = 0;                    // Focus Position Z
            public int m_nReticleSize_mm = 1000;              // Reticle Size (mm)
            public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
            public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
            public int m_nRotatePulse = 1000;
            public GrabMode m_grabMode = null;
            string m_sGrabMode = "";

            public double p_dDegree
            {
                get
                {
                    return m_dDegree;
                }
                set
                {
                    if (value > 360)
                        m_dDegree = value - 360;
                    else
                        m_dDegree = value;
                }
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
            public Run_GrabSideScan(MainVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_GrabSideScan run = new Run_GrabSideScan(m_module);
                run.m_dDegree = m_dDegree;
                run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
                run.m_cpMemoryOffset = new CPoint(m_cpMemoryOffset);
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                run.m_nFocusPosZ = m_nFocusPosZ;
                run.m_nReticleSize_mm = m_nReticleSize_mm;
                run.m_nMaxFrame = m_nMaxFrame;
                run.m_nScanRate = m_nScanRate;
                run.p_sGrabMode = p_sGrabMode;
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
                p_dDegree = tree.Set(p_dDegree, p_dDegree, "Degree", "Rotation Degree(0 ~ 360)", bVisible);
                m_nRotatePulse = tree.Set(m_nRotatePulse, m_nRotatePulse, "Theta Pulse", "Theta Pulse", bVisible);
                m_cpMemoryOffset = tree.Set(m_cpMemoryOffset, m_cpMemoryOffset, "Memory Offset", "Grab Start Memory Position (px)", bVisible);
                m_dResX_um = tree.Set(m_dResX_um, m_dResX_um, "Cam X Resolution", "X Resolution (um)", bVisible);
                m_dResY_um = tree.Set(m_dResY_um, m_dResY_um, "Cam Y Resolution", "Y Resolution (um)", bVisible);
                m_nFocusPosZ = tree.Set(m_nFocusPosZ, m_nFocusPosZ, "Focus Z Position", "Focus Z Position", bVisible);
                m_nReticleSize_mm = tree.Set(m_nReticleSize_mm, m_nReticleSize_mm, "Reticle Size Y", "Reticle Size Y", bVisible);
                m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
                m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            }
            public override string Run()
            {
                if (m_grabMode == null) return "Grab Mode == null";

                try
                {
                    m_grabMode.SetLight(true);

                    AxisXY axisXY = m_module.m_axisXY;
                    Axis axisSizeZ = m_module.m_axisSideZ;
                    Axis axisRotate = m_module.m_axisRotate;

                    CPoint cpMemoryOffset = new CPoint(m_cpMemoryOffset);
                    int nScanLine = 0;
                    int nMMPerUM = 1000;
                    int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                    int nReticleSizeY_px = Convert.ToInt32(m_nReticleSize_mm * nMMPerUM / m_dResY_um);  // 레티클 영역의 Y픽셀 갯수
                    int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nReticleSizeY_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
                    int nScanOffset_pulse = 100000; //가속버퍼구간
                    int nDirection = 4;
                    while (nDirection > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        double nRotate = m_nRotatePulse * (p_dDegree * nScanLine);
                        if (m_module.Run(axisRotate.StartMove(nRotate)))
                            return p_sInfo;
                        if (m_module.Run(axisRotate.WaitReady()))
                            return p_sInfo;

                        double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                        double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

                        double dPosX = m_rpAxisCenter.X;

                        if (m_module.Run(axisSizeZ.StartMove(m_nFocusPosZ)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                            return p_sInfo;
                        if (m_module.Run(axisRotate.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisSizeZ.WaitReady()))
                            return p_sInfo;

                        double dTriggerStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2;
                        double dTriggerEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;
                        axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

                        string strPool = m_grabMode.m_memoryPool.p_id;
                        string strGroup = m_grabMode.m_memoryGroup.p_id;
                        GrabMode.eScanPos curScanPos = (GrabMode.eScanPos)nScanLine;
                        string strMemory = curScanPos.ToString();
                        MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                        int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_nScanRate / 100);
                        m_grabMode.StartGrab(mem, cpMemoryOffset, nReticleSizeY_px, m_grabMode.m_bUseBiDirectionScan);

                        if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        axisXY.p_axisY.RunTrigger(false);

                        nScanLine++;                     
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
        public class Run_Grab: ModuleRunBase
        {
            MainVision m_module;

            public RPoint m_rpAxisCenter = new RPoint();    // Reticle Center Position
            public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
            public double m_dResX_um = 1;                   // Camera Resolution X
            public double m_dResY_um = 1;                   // Camera Resolution Y
            public int m_nFocusPosZ = 0;                    // Focus Position Z
            public int m_nReticleSize_mm = 1000;              // Reticle Size (mm)
            public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
            public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
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
            public Run_Grab(MainVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_Grab run = new Run_Grab(m_module);
                run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
                run.m_cpMemoryOffset = new CPoint(m_cpMemoryOffset);
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                run.m_nFocusPosZ = m_nFocusPosZ;
                run.m_nReticleSize_mm = m_nReticleSize_mm;
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
                m_nReticleSize_mm = tree.Set(m_nReticleSize_mm, m_nReticleSize_mm, "Reticle Size Y", "Reticle Size Y", bVisible);
                m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
                m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            }
            public override string Run()
            {
                if (m_grabMode == null) return "Grab Mode == null";

                try
                {
                    m_grabMode.SetLight(true);

                    double a = Math.Atan(55.3);
                    AxisXY axisXY = m_module.m_axisXY;
                    Axis axisZ = m_module.m_axisZ;
                    CPoint cpMemoryOffset = new CPoint(m_cpMemoryOffset);
                    int nScanLine = 0;
                    int nMMPerUM = 1000;
                    int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
                    int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

                    double dXScale = m_dResX_um * 10;
                    cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth;
                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                    int nReticleSizeY_px = Convert.ToInt32(m_nReticleSize_mm * nMMPerUM / m_dResY_um);  // 레티클 영역의 Y픽셀 갯수
                    int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nReticleSizeY_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
                    int nScanOffset_pulse = 100000; //가속버퍼구간

                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                        double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;


                        double dPosX = m_rpAxisCenter.X + nReticleSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

                        if (m_module.Run(axisZ.StartMove(m_nFocusPosZ)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        double dTriggerStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2;
                        double dTriggerEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;
                        axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

                        string strPool = m_grabMode.m_memoryPool.p_id;
                        string strGroup = m_grabMode.m_memoryGroup.p_id;
                        string strMemory = m_grabMode.m_memoryData.p_id;

                        MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                        int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_nScanRate / 100);
                        m_grabMode.StartGrab(mem, cpMemoryOffset, nReticleSizeY_px, m_grabMode.m_bUseBiDirectionScan);

                        if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        axisXY.p_axisY.RunTrigger(false);

                        nScanLine++;
                        cpMemoryOffset.X += nCamWidth;
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
        public class Run_Grab45 : ModuleRunBase
        {
            MainVision m_module;

            public RPoint m_rpAxisCenter = new RPoint();    // Reticle Center Position
            public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
            public double m_dResX_um = 1;                   // Camera Resolution X
            public double m_dResY_um = 1;                   // Camera Resolution Y
            public int m_nFocusPosZ = 0;                    // Focus Position Z
            public int m_nReticleSize_mm = 1000;              // Reticle Size (mm)
            public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
            public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
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
            public Run_Grab45(MainVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_Grab run = new Run_Grab(m_module);
                run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
                run.m_cpMemoryOffset = new CPoint(m_cpMemoryOffset);
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                run.m_nFocusPosZ = m_nFocusPosZ;
                run.m_nReticleSize_mm = m_nReticleSize_mm;
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
                m_nReticleSize_mm = tree.Set(m_nReticleSize_mm, m_nReticleSize_mm, "Reticle Size Y", "Reticle Size Y", bVisible);
                m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
                m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            }
            public override string Run()
            {
                if (m_grabMode == null) return "Grab Mode == null";

                try
                {
                    AxisXY axisXY = m_module.m_axisXY;
                    Axis axisZ = m_module.m_axisZ;
                    m_grabMode.SetLight(true);
                    if(m_grabMode.pUseRADS)
                    {
                        if(!axisZ.EnableCompensation(1))
                            return "Axis Y Compensation disabled";

                    }
                    CPoint cpMemoryOffset = new CPoint(m_cpMemoryOffset);
                    int nScanLine = 0;
                    int nMMPerUM = 1000;
                    int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
                    int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

                    double dXScale = m_dResX_um * 10;
                    cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth;
                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                    int nReticleSizeY_px = Convert.ToInt32(m_nReticleSize_mm * nMMPerUM / m_dResY_um);  // 레티클 영역의 Y픽셀 갯수
                    int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nReticleSizeY_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
                    int nScanOffset_pulse = 100000; //가속버퍼구간

                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                        double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

                        double dPosX = m_rpAxisCenter.X + nReticleSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

                        if (m_module.Run(axisZ.StartMove(m_nFocusPosZ)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        double dTriggerStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2;
                        double dTriggerEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;
                        axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, true);

                        double dTriggerDistance = Math.Abs(dTriggerEndPosY - dTriggerStartPosY);
                        double dSection = dTriggerDistance / ladsinfos[nScanLine].m_Heightinfo.Length;
                        double[] darrScanAxisPos = new double[ladsinfos[nScanLine].m_Heightinfo.Length];
                        for (int i = 0; i<darrScanAxisPos.Length; i++)
                        {
                            if (dTriggerStartPosY > dTriggerEndPosY)
                                darrScanAxisPos[i] = dTriggerStartPosY - (dSection * i);
                            else
                                darrScanAxisPos[i] = dTriggerStartPosY + (dSection * i);
                        }
                        SetFocusMap(((AjinAxis)axisXY.p_axisY).m_nAxis, ((AjinAxis)axisZ).m_nAxis, darrScanAxisPos, ladsinfos[nScanLine].m_Heightinfo, ladsinfos[nScanLine].m_Heightinfo.Length, false);

                        string strPool = m_grabMode.m_memoryPool.p_id;
                        string strGroup = m_grabMode.m_memoryGroup.p_id;
                        string strMemory = m_grabMode.m_memoryData.p_id;

                        MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                        int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_nScanRate / 100);
                        m_grabMode.StartGrab(mem, cpMemoryOffset, nReticleSizeY_px, m_grabMode.m_bUseBiDirectionScan);
                        if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        axisXY.p_axisY.RunTrigger(false);

                        nScanLine++;
                        cpMemoryOffset.X += nCamWidth;
                    }
                    m_grabMode.m_camera.StopGrab();
                    return "OK";
                }
                finally
                {
                    m_grabMode.SetLight(false);
                }
            }

            private void SetFocusMap(int nScanAxisNo, int nZAxisNo, double[] darrScanAxisPos, double[] darrZAxisPos, int nPointCount, bool bReverse)
            {
                // variable
                int iIdxScan = 0;
                int iIdxZ = 1;
                int[] narrAxisNo = new int[2];
                double[] darrPosition = new double[2];
                double dMaxVelocity = m_module.m_axisXY.p_axisY.GetSpeedValue(eSpeed.Move).m_v;
                double dMaxAccel = m_module.m_axisXY.p_axisY.GetSpeedValue(eSpeed.Move).m_acc;
                double dMaxDecel = m_module.m_axisXY.p_axisY.GetSpeedValue(eSpeed.Move).m_dec;

                // implement
                if (nZAxisNo < nScanAxisNo)
                {
                    iIdxZ = 0;
                    iIdxScan = 1;
                }
                narrAxisNo[iIdxScan] = nScanAxisNo;
                narrAxisNo[iIdxZ] = nZAxisNo;

                // Queue 초기화
                CAXM.AxmContiWriteClear(nScanAxisNo);
                // 보간구동 축 맵핑
                CAXM.AxmContiSetAxisMap(nScanAxisNo, (uint)narrAxisNo.Length, narrAxisNo);
                // 구동모드 설정 -> [0] : 절대위치구동, [1] : 상대위치구동
                uint unAbsRelMode = 0;
                CAXM.AxmContiSetAbsRelMode(nScanAxisNo, unAbsRelMode);
                // Conti 작성 시작 -> AxmContiBeginNode ~ AxmContiEndNode 사이의 AXM관련 함수들이 Conti Queue에 등록된다.
                CAXM.AxmContiBeginNode(nScanAxisNo);
                // 축별 구동위치 등록
                if (bReverse)
                {
                    for (int i = nPointCount - 1; i >= 0; i--)
                    {
                        darrPosition[iIdxScan] = darrScanAxisPos[i];
                        darrPosition[iIdxZ] = darrZAxisPos[i] + m_module.m_axisZ.GetPosValue(eAxisPos.ScanPos);
                        CAXM.AxmLineMove(nScanAxisNo, darrPosition, dMaxVelocity, dMaxAccel, dMaxDecel);
                    }
                }
                else
                {
                    for (int i = 0; i<nPointCount; i++)
                    {
                        darrPosition[iIdxScan] = darrScanAxisPos[i];
                        darrPosition[iIdxZ] = darrZAxisPos[i] + m_module.m_axisZ.GetPosValue(eAxisPos.ScanPos);
                        CAXM.AxmLineMove(nScanAxisNo, darrPosition, dMaxVelocity, dMaxAccel, dMaxDecel);
                    }
                }
                // Conti 작성 종료
                CAXM.AxmContiEndNode(nScanAxisNo);

                return;
            }
        }
        public class Run_LADS : ModuleRunBase
        {
            MainVision m_module;

            public RPoint m_rpAxisCenter = new RPoint();    // Reticle Center Position
            public CPoint m_cpMemoryOffset = new CPoint();  // Memory Offset
            public double m_dResX_um = 1;                   // Camera Resolution X
            public double m_dResY_um = 1;                   // Camera Resolution Y
            public int m_nFocusPosZ = 0;                    // Focus Position Z
            public int m_nReticleSize_mm = 1000;              // Reticle Size (mm)
            public int m_nMaxFrame = 100;                   // Camera max Frame 스펙
            public int m_nScanRate = 100;                   // Camera Frame Spec 사용률 ? 1~100 %
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
            public Run_LADS(MainVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_LADS run = new Run_LADS(m_module);
                run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
                run.m_cpMemoryOffset = new CPoint(m_cpMemoryOffset);
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                run.m_nFocusPosZ = m_nFocusPosZ;
                run.m_nReticleSize_mm = m_nReticleSize_mm;
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
                m_nReticleSize_mm = tree.Set(m_nReticleSize_mm, m_nReticleSize_mm, "Reticle Size Y", "Reticle Size Y", bVisible);
                m_nMaxFrame = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
                m_nScanRate = (tree.GetTree("Scan Velocity", false, bVisible)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            }
            
            public override string Run()
            {
                if (m_grabMode == null) return "Grab Mode == null";

                try
                {
                    m_grabMode.SetLight(true);

                    AxisXY axisXY = m_module.m_axisXY;
                    Axis axisZ = m_module.m_axisZ;
                    CPoint cpMemoryOffset = new CPoint(m_cpMemoryOffset);
                    int nScanLine = 0;
                    int nMMPerUM = 1000;
                    int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
                    int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;

                    double dXScale = m_dResX_um * 10;
                    cpMemoryOffset.X += (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth;
                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                    int nReticleSizeY_px = Convert.ToInt32(m_nReticleSize_mm * nMMPerUM / m_dResY_um);  // 레티클 영역의 Y픽셀 갯수
                    int nTotalTriggerCount = Convert.ToInt32(m_grabMode.m_dTrigger * nReticleSizeY_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
                    int nScanOffset_pulse = 100000; //가속버퍼구간

                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                        double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

                        double dPosX = m_rpAxisCenter.X + nReticleSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

                        if (m_module.Run(axisZ.StartMove(m_nFocusPosZ)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        double dTriggerStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2;
                        double dTriggerEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;
                        axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger, 40, true);

                        string strPool = m_grabMode.m_memoryPool.p_id;
                        string strGroup = m_grabMode.m_memoryGroup.p_id;
                        string strMemory = m_grabMode.m_memoryData.p_id;

                        MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                        int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_nScanRate / 100);
                        m_grabMode.StartGrab(mem, cpMemoryOffset, nReticleSizeY_px, m_grabMode.m_bUseBiDirectionScan);

                        if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        axisXY.p_axisY.RunTrigger(false);

                        CalculateHeight(nScanLine, mem, nReticleSizeY_px,new RPoint(dPosX,dStartPosY),dEndPosY);

                        nScanLine++;
                        cpMemoryOffset.X += nCamWidth;
                    }
                    m_grabMode.m_camera.StopGrab();
                    SaveFocusMapImage(nScanLine, nReticleSizeY_px / nCamHeight);
                    return "OK";
                }
                finally
                {
                    m_grabMode.SetLight(false);
                }
            }

            unsafe void CalculateHeight(int nCurLine, MemoryData mem,int ReticleHeight, RPoint Startpos,double endY)
            {
                int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
                int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;
                int nHeight = ReticleHeight / nCamHeight;
                byte* ptr =(byte*) mem.GetPtr().ToPointer(); //Gray
                LADSInfo ladsinfo = new LADSInfo(Startpos, endY, nHeight);
                for(int i=0;i<nHeight;i++)
                {
                    int s=0, e=0; //레이저 시작, 끝위치 정보
                    //탐색시작y지점
                    int nY = i * nCamHeight;
                    for(int j=0;j<nCamHeight;j++)
                    {
                        if(ptr[(int)((nY+j) *mem.W+nCamWidth*(nCurLine+0.5))]>70)
                        {
                            e = Math.Max(e, j);
                            s = Math.Min(s, j);
                        }
                    }

                    ladsinfo.m_Heightinfo[i] = (s + e) / 2;
                }
                ladsinfos.Add(ladsinfo);
            }
            private void SaveFocusMapImage(int nX, int nY)
            {
                int thumsize = 30;
                int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;
                Mat ResultMat = new Mat();
                for(int x =0;x<nX;x++)
                {
                    Mat Vmat = new Mat();
                    for(int y=0;y<nY;y++)
                    {
                        Mat ColorImg = new Mat(thumsize, thumsize, DepthType.Cv8U, 1);
                        //double nScalednum = (ladsInfo.m_Heightinfo[y,x]-110) * 255 / nCamHeight;
                        double nScalednum = (ladsinfos[nX].m_Heightinfo[y] - 110) * 255 / nCamHeight;
                        ColorImg.SetTo(new MCvScalar(nScalednum*20));

                        if (y == 0)
                            Vmat = ColorImg;
                        else
                            CvInvoke.VConcat(ColorImg, Vmat, Vmat);
                    }
                    if (x == 0)
                        ResultMat = Vmat;
                    else
                        CvInvoke.HConcat(ResultMat, Vmat, ResultMat);

                    CvInvoke.Imwrite(@"D:\Test\" + x+".bmp", ResultMat);

                }
                CvInvoke.Imwrite(@"D:\FocusMap.bmp", ResultMat);
            }
        }

        #region Barcode Inspection
        public class Run_BarcodeInspection : ModuleRunBase
        {
            MainVision m_module;
            public int m_nGaussianBlurKernalSize = 3;
            public double m_dGaussianBlurSigma = 1.5;
            public AdaptiveThresholdType m_eAdabtiveThresholdType = AdaptiveThresholdType.GaussianC;
            public ThresholdType m_eThresholdType = ThresholdType.Binary;
            public int m_nThresholdBlockSize = 3;
            public double m_dThresholdParam = 3;
            public int m_nErodeSize = 3;

            public Run_BarcodeInspection(MainVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_BarcodeInspection run = new Run_BarcodeInspection(m_module);
                run.m_nGaussianBlurKernalSize = m_nGaussianBlurKernalSize;
                run.m_dGaussianBlurSigma = m_dGaussianBlurSigma;
                run.m_eAdabtiveThresholdType = m_eAdabtiveThresholdType;
                run.m_eThresholdType = m_eThresholdType;
                run.m_nThresholdBlockSize = m_nThresholdBlockSize;
                run.m_dThresholdParam = m_dThresholdParam;
                run.m_nErodeSize = m_nErodeSize;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nGaussianBlurKernalSize = tree.Set(m_nGaussianBlurKernalSize, m_nGaussianBlurKernalSize, "GaussianBlur Kernal Size", "GaussianBlur Kernal Size", bVisible);
                m_dGaussianBlurSigma = tree.Set(m_dGaussianBlurSigma, m_dGaussianBlurSigma, "GaussianBlur Sigma", "GaussianBlur Sigma", bVisible);
                m_eAdabtiveThresholdType = (AdaptiveThresholdType)tree.Set(m_eAdabtiveThresholdType, m_eAdabtiveThresholdType, "Adaptive Threshold Type", "Adaptive Threshold Type", bVisible);
                m_eThresholdType = (ThresholdType)tree.Set(m_eThresholdType, m_eThresholdType, "Threshold Type", "Threshold Type", bVisible);
                m_nThresholdBlockSize = tree.Set(m_nThresholdBlockSize, m_nThresholdBlockSize, "Threshold Block Size", "Threshold Block Size", bVisible);
                m_dThresholdParam = tree.Set(m_dThresholdParam, m_dThresholdParam, "Threshold Param", "Threshold Param", bVisible);
                m_nErodeSize = tree.Set(m_nErodeSize, m_nErodeSize, "Erode Size", "Erode Size", bVisible);
            }

            public override string Run()
            {
                string strPool = "MainVision.Vision Memory";
                string strGroup = "MainVision";
                string strMemory = "Main";
                MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                CPoint cptStartROIPoint = new CPoint(2500, 4667);
                CPoint cptEndROIPoint = new CPoint(4600, 10652);
                CRect crtROI = new CRect(cptStartROIPoint, cptEndROIPoint);
                Mat matSrc = GetBarcodeMat(mem, crtROI);
                Mat matBinary = new Mat();
                Image<Gray, Byte> imgSrc;
                CvBlobs blobs = new CvBlobs();
                CvBlobDetector blobDetector = new CvBlobDetector();

                CvInvoke.GaussianBlur(matSrc, matBinary, new System.Drawing.Size(m_nGaussianBlurKernalSize, m_nGaussianBlurKernalSize), m_dGaussianBlurSigma);
                CvInvoke.AdaptiveThreshold(matBinary, matBinary, 255.0, m_eAdabtiveThresholdType, m_eThresholdType, m_nThresholdBlockSize, m_dThresholdParam);
                var element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(m_nErodeSize, m_nErodeSize), new System.Drawing.Point(-1, -1));
                CvInvoke.Dilate(matBinary, matBinary, element, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
                matBinary.Save("D:\\TEST.BMP");

                imgSrc = matBinary.ToImage<Gray, Byte>();
                blobDetector.Detect(imgSrc, blobs);

                return "OK";
            }

            unsafe Mat GetBarcodeMat(MemoryData mem, CRect crtROI)
            {
                ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
                IntPtr p = mem.GetPtr();
                img.SetData(p, crtROI, (int)mem.W);
                img.SaveImageSync("D:\\TEST.bmp");
                Mat matReturn = new Mat((int)img.p_Size.Y, (int)img.p_Size.X, Emgu.CV.CvEnum.DepthType.Cv8U, img.p_nByte, img.GetPtr(), (int)img.p_Stride);

                return matReturn;
            }


        }
        #endregion
    }
}
