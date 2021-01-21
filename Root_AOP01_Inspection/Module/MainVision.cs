using Emgu.CV;
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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using static RootTools.Control.Axis;

namespace Root_AOP01_Inspection.Module
{
    public class MainVision : ModuleBase, IWTRChild
    {
        #region ToolBox
        Axis m_axisRotate;
        Axis m_axisZ;
        Axis m_axisSideZ;
        AxisXY m_axisXY;
        public DIO_I m_diExistVision;
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
            public LADSInfo(RPoint _axisPos, double _endYPos, int arrcap/*heightinfo capacity*/)
            {
                axisPos = _axisPos;
                endYPos = _endYPos;
                m_Heightinfo = new double[arrcap];
            }
        }

        static List<LADSInfo> ladsinfos;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_diExistVision, this, "Reticle Exist on Vision");
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

        #region Axis Position
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
        #endregion

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
            //MainVision.Main.
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryMain = m_memoryGroup.CreateMemory(App.mMainMem, 1, 1, 1000, 1000);

            m_memorySideLeft = m_memoryGroup.CreateMemory(App.mSideLeftMem, 1, 1, 1000, 1000);
            m_memorySideBottom = m_memoryGroup.CreateMemory(App.mSideBotMem, 1, 1, 1000, 1000);
            m_memorySideRight = m_memoryGroup.CreateMemory(App.mSideRightMem, 1, 1, 1000, 1000);
            m_memorySideTop = m_memoryGroup.CreateMemory(App.mSideTopMem, 1, 1, 1000, 1000);

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

            //if (m_CamTDI90 != null && m_CamTDI90.p_CamInfo.p_eState == eCamState.Init)
            //    m_CamTDI90.Connect();
            //if (m_CamTDI45 != null && m_CamTDI45.p_CamInfo.p_eState == eCamState.Init)
            //    m_CamTDI45.Connect();
            //if (m_CamLADS.p_CamInfo._OpenStatus == false)
            //    m_CamLADS.Connect();
            //if (m_CamTDISide != null && m_CamTDISide.p_CamInfo.p_eState == eCamState.Init)
            //    m_CamTDISide.Connect();

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

        #region Inspection Result
        bool m_bAlignKeyPass = true;
        public bool p_bAlignKeyPass
        {
            get { return m_bAlignKeyPass; }
            set 
            {
                m_bAlignKeyPass = value;
                OnPropertyChanged();
            }
        }

        bool m_bPatterhShiftPass = true;
        public bool p_bPatternShiftPass
        {
            get { return m_bPatterhShiftPass; }
            set
            {
                m_bPatterhShiftPass = value;
                OnPropertyChanged();                
            }
        }

        double m_dPatternShiftDistance = 0.0;
        public double p_dPatternShiftDistance
        {
            get { return m_dPatternShiftDistance; }
            set 
            {
                m_dPatternShiftDistance = value;
                OnPropertyChanged();
            }
        }

        double m_dPatternShiftAngle = 0.0;
        public double p_dPatternShiftAngle
        {
            get { return m_dPatternShiftAngle; }
            set
            {
                m_dPatternShiftAngle = value;
                OnPropertyChanged();
            }
        }

        bool m_bBarcodePass = true;
        public bool p_bBarcodePass
        {
            get { return m_bBarcodePass; }
            set
            {
                m_bBarcodePass = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Vision Algorithm
        Mat GetMatImage(MemoryData mem, CRect crtROI)
        {
            if (crtROI.Width < 1 || crtROI.Height < 1) return null;
            if (crtROI.Left < 0 || crtROI.Top < 0) return null;
            ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
            IntPtr p = mem.GetPtr();
            img.SetData(p, crtROI, (int)mem.W);
            Mat matReturn = new Mat((int)img.p_Size.Y, (int)img.p_Size.X, Emgu.CV.CvEnum.DepthType.Cv8U, img.p_nByte, img.GetPtr(), (int)img.p_Stride);

            return matReturn;
        }

        bool TemplateMatching(MemoryData mem, CRect crtSearchArea, Image<Gray, byte> imgSrc, Image<Gray, byte> imgTemplate, out CPoint cptCenter, double dMatchScore)
        {
            // variable
            int nWidthDiff = 0;
            int nHeightDiff = 0;
            Point ptMaxRelative = new Point();
            float fMaxScore = float.MinValue;
            bool bFoundTemplate = false;

            // implement
            Image<Gray, float> imgResult = imgSrc.MatchTemplate(imgTemplate, TemplateMatchingType.CcorrNormed);
            nWidthDiff = imgSrc.Width - imgResult.Width;
            nHeightDiff = imgSrc.Height - imgResult.Height;
            float[,,] matches = imgResult.Data;

            for (int x = 0; x < matches.GetLength(1); x++)
            {
                for (int y = 0; y < matches.GetLength(0); y++)
                {
                    if (fMaxScore < matches[y, x, 0] && dMatchScore <= matches[y, x, 0])
                    {
                        fMaxScore = matches[y, x, 0];
                        ptMaxRelative.X = x;
                        ptMaxRelative.Y = y;
                        bFoundTemplate = true;
                    }
                }
            }
            cptCenter = new CPoint();
            cptCenter.X = (int)(crtSearchArea.Left + ptMaxRelative.X) + (int)(nWidthDiff / 2);
            cptCenter.Y = (int)(crtSearchArea.Top + ptMaxRelative.Y) + (int)(nHeightDiff / 2);

            return bFoundTemplate;
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Grab(this), true, "Run Grab");
            AddModuleRunList(new Run_Grab45(this), true, "Run Grab 45");
            AddModuleRunList(new Run_GrabSideScan(this), true, "Run Side Scan");
            AddModuleRunList(new Run_LADS(this), true, "Run LADS");
            AddModuleRunList(new Run_BarcodeInspection(this), true, "Run Barcode Inspection");
            AddModuleRunList(new Run_MakeAlignTemplateImage(this), true, "Run MakeAlignTemplateImage");
            AddModuleRunList(new Run_PatternAlign(this), true, "Run PatternAlign");
            AddModuleRunList(new Run_PatternShiftAndRotation(this), true, "Run Pattern ShiftAndRotation");
            AddModuleRunList(new Run_AlignKeyInspection(this), true, "Run AlignKeyInspection");
            AddModuleRunList(new Run_Flip(this), true, "Run Reticle Flip");
        }
        #endregion

        public WTRCleanUnit p_wtr
        {
            get
            {
                AOP01_Handler handler = (AOP01_Handler)m_engineer.ClassHandler();
                return (WTRCleanUnit)handler.m_wtr;
            }
        }

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
        public class Run_Flip : ModuleRunBase
        {
            MainVision m_module;
            public Run_Flip(MainVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_Flip run = new Run_Flip(m_module);
                return run;
            }
            string m_sFlip = "Flip";
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sFlip = tree.Set(m_sFlip, m_sFlip, "Flip", "Reticle Flip Glass to Bottom", bVisible, true);
            }
            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                m_module.p_wtr.StartFlip();
                while (m_module.p_wtr.IsBusy()) Thread.Sleep(10);
                return "OK";
            }
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
        public class Run_Grab : ModuleRunBase
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
                Run_Grab45 run = new Run_Grab45(m_module);
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
                    if (m_grabMode.pUseRADS)
                    {
                        if (!axisZ.EnableCompensation(1))
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
                    int nScanOffset_pulse = 200000; //가속버퍼구간

                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                        double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

                        double dPosX = m_rpAxisCenter.X/*중심축값*/ + (nReticleSizeY_px * (double)m_grabMode.m_dTrigger / 2 /*레티클 절반*/) - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

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
                        for (int i = 0; i < darrScanAxisPos.Length; i++)
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

                        CAXM.AxmContiStart(((AjinAxis)axisXY.p_axisY).m_nAxis, 0, 0);
                        Thread.Sleep(10);
                        uint unRunning = 0;
                        while (true)
                        {
                            CAXM.AxmContiIsMotion(((AjinAxis)axisXY.p_axisY).m_nAxis, ref unRunning);
                            if (unRunning == 0) break;
                            Thread.Sleep(100);
                        }

                        //if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                        //    return p_sInfo;
                        //if (m_module.Run(axisXY.WaitReady()))
                        //    return p_sInfo;
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

                // Min-Max 구하기
                double dMin = 480;
                double dMax = 0;
                double dCenter = 240;
                double dPixelPerPulse = 500;
                for (int i = 0; i < darrZAxisPos.Length; i++)
                {
                    if (darrZAxisPos[i] < dMin) dMin = darrZAxisPos[i];
                    if (darrZAxisPos[i] > dMax) dMax = darrZAxisPos[i];
                }
                dCenter = ((dMax - dMin) / 2) + dMin;

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
                        darrPosition[iIdxZ] = ((darrZAxisPos[i] - dCenter) * dPixelPerPulse) + m_nFocusPosZ;//m_module.m_axisZ.GetPosValue(eAxisPos.ScanPos);
                        CAXM.AxmLineMove(nScanAxisNo, darrPosition, dMaxVelocity, dMaxAccel, dMaxDecel);
                    }
                }
                else
                {
                    for (int i = 0; i < nPointCount; i++)
                    {
                        darrPosition[iIdxScan] = darrScanAxisPos[i];
                        darrPosition[iIdxZ] = ((darrZAxisPos[i] - dCenter) * dPixelPerPulse) + m_nFocusPosZ;//m_module.m_axisZ.GetPosValue(eAxisPos.ScanPos);
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

            public int m_nLaserThreshold = 70;              // Laser Threshold
            public int m_nUptime = 40;                      // Trigger Uptime
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
                run.m_nLaserThreshold = m_nLaserThreshold;
                run.m_nUptime = m_nUptime;
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
                m_nLaserThreshold = tree.Set(m_nLaserThreshold, m_nLaserThreshold, "Laser Threshold", "Laser Threshold", bVisible);
                m_nUptime = tree.Set(m_nUptime, m_nUptime, "Trigger Uptime", "Trigger Uptime", bVisible);
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
                    ladsinfos.Clear();

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
                        double dTriggerEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2;
                        axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, m_grabMode.m_dTrigger * nCamHeight, m_nUptime, true);

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
                        m_grabMode.m_camera.StopGrab();
                        //CalculateHeight(nScanLine, mem, nReticleSizeY_px, new RPoint(dPosX, dStartPosY), dEndPosY);
                        CalculateHeight_ESCHO(mem, m_grabMode.m_ScanStartLine + nScanLine, nReticleSizeY_px);

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

            unsafe void CalculateHeight_ESCHO(MemoryData mem, int nCurrentLine, int nReticleHeight_px)
            {
                IntPtr p = mem.GetPtr();
                int nCamWidth = m_grabMode.m_camera.GetRoiSize().X;
                int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;
                int nCount = nReticleHeight_px / nCamHeight;
                LADSInfo ladsinfo = new LADSInfo(new RPoint(), 0, nCount);

                for (int i = 0; i < nCount; i++)
                {
                    int nLeft = nCurrentLine * nCamWidth;
                    int nTop = i * nCamHeight;
                    int nRight = nLeft + nCamWidth;
                    int nBottom = nTop + nCamHeight;
                    CRect crtROI = new CRect(nLeft, nTop, nRight, nBottom);
                    ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
                    img.SetData(p, crtROI, (int)mem.W);
                    ladsinfo.m_Heightinfo[i] = CalculatingHeight(img);
                }
                ladsinfos.Add(ladsinfo);
            }

            #region VEGA LADS
            unsafe double CalculatingHeight(ImageData img)
            {
                // variable
                int nImgWidth = m_grabMode.m_camera.GetRoiSize().X;
                int nImgHeight = m_grabMode.m_camera.GetRoiSize().Y;
                double[] daHeight = new double[nImgWidth];

                // implement
                byte* pSrc = (byte*)img.GetPtr().ToPointer();
                for (int x = 0; x < nImgWidth; x++, pSrc++)
                {
                    byte* pSrcY = pSrc;
                    int nSum = 0;
                    int nYSum = 0;
                    for (int y = 0; y < nImgHeight; y++, pSrcY += nImgWidth)
                    {
                        if (*pSrcY < m_nLaserThreshold) continue;
                        nSum += *pSrcY;
                        nYSum += *pSrcY * y;
                    }
                    int iIndex = x;
                    daHeight[iIndex] = (nSum != 0) ? ((double)nYSum / (double)nSum) : 0.0;
                }

                return GetHeightAverage(daHeight);
            }

            double GetHeightAverage(double[] daHeight)
            {
                // variable
                double dSum = 0.0;
                int nHitCount = 0;

                // implement
                for (int i = 0; i < daHeight.Length; i++)
                {
                    if (daHeight[i] < double.Epsilon) continue;
                    nHitCount++;
                    dSum += daHeight[i];
                }
                if (nHitCount == 0) return -1;
                return dSum / nHitCount;
            }
            #endregion
            #region 이지혜 LADS
            private void SaveFocusMapImage(int nX, int nY)
            {
                int thumsize = 30;
                int nCamHeight = m_grabMode.m_camera.GetRoiSize().Y;
                Mat ResultMat = new Mat();

                // Min-Max 값 알아내기
                int nMin = 480;
                int nMax = 0;
                for (int x = 0; x < nX; x++)
                {
                    for (int y = 0; y < nY; y++)
                    {
                        if (ladsinfos[x].m_Heightinfo[y] < nMin && ladsinfos[x].m_Heightinfo[y] > -1) nMin = (int)ladsinfos[x].m_Heightinfo[y];
                        if (ladsinfos[x].m_Heightinfo[y] > nMax) nMax = (int)ladsinfos[x].m_Heightinfo[y];
                    }
                }

                for (int x = 0; x < nX; x++)
                {
                    Mat Vmat = new Mat();
                    for (int y = 0; y < nY; y++)
                    {
                        Mat ColorImg = new Mat(thumsize, thumsize, DepthType.Cv8U, 3);
                        MCvScalar color = HeatColor(ladsinfos[x].m_Heightinfo[y], nMin, nMax);
                        ColorImg.SetTo(color);

                        if (y == 0)
                            Vmat = ColorImg;
                        else
                            CvInvoke.VConcat(ColorImg, Vmat, Vmat);
                    }
                    if (x == 0)
                        ResultMat = Vmat;
                    else
                        CvInvoke.HConcat(ResultMat, Vmat, ResultMat);

                    CvInvoke.Imwrite(@"D:\Test\" + x + ".bmp", ResultMat);

                }
                CvInvoke.Imwrite(@"D:\FocusMap.bmp", ResultMat);
            }

            MCvScalar HeatColor(double dValue, double dMin, double dMax)
            {
                double r = 0, g = 0, b = 0;
                double x = (dValue - dMin) / (dMax - dMin);
                r = 255 * (-4 * Math.Abs(x - 0.75) + 2);
                g = 255 * (-4 * Math.Abs(x - 0.50) + 2);
                b = 255 * (-4 * Math.Abs(x) + 2);

                return new MCvScalar(b, g, r);
            }
            #endregion
        }

        #region Barcode Inspection
        public class Run_BarcodeInspection : ModuleRunBase
        {
            public enum eSearchDirection
            {
                TopToBottom = 0,
                LeftToRight,
                RightToLeft,
                BottomToTop,
            }

            MainVision m_module;
            public CPoint m_cptBarcodeLTPoint = new CPoint(0, 0);
            public int m_nROIWidth = 1000;
            public int m_nROIHeight = 5000;
            public bool m_bDarkBackground = true;
            public int m_nThreshold = 70;

            public Run_BarcodeInspection(MainVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_BarcodeInspection run = new Run_BarcodeInspection(m_module);
                run.m_cptBarcodeLTPoint = m_cptBarcodeLTPoint;
                run.m_nROIWidth = m_nROIWidth;
                run.m_nROIHeight = m_nROIHeight;
                run.m_bDarkBackground = m_bDarkBackground;
                run.m_nThreshold = m_nThreshold;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_cptBarcodeLTPoint = (tree.GetTree("Barcode ROI", false, bVisible)).Set(m_cptBarcodeLTPoint, m_cptBarcodeLTPoint, "Left Top Point", "Left Top Point", bVisible);
                m_nROIWidth = (tree.GetTree("Barcode ROI", false, bVisible)).Set(m_nROIWidth, m_nROIWidth, "Barcode ROI Width", "Barcode ROI Width", bVisible);
                m_nROIHeight = (tree.GetTree("Barcode ROI", false, bVisible)).Set(m_nROIHeight, m_nROIHeight, "Barcode ROI Height", "Barcode ROI Height", bVisible);
                m_bDarkBackground = tree.Set(m_bDarkBackground, m_bDarkBackground, "Dark Background", "Dark Background", bVisible);
                m_nThreshold = tree.Set(m_nThreshold, m_nThreshold, "Find Edge Threshold", "Find Edge Threshold", bVisible);
            }

            public override string Run()
            {
                MemoryData mem = m_module.m_engineer.GetMemory(App.mPool, App.mGroup, App.mMainMem);
                CPoint cptStartROIPoint = m_cptBarcodeLTPoint;
                CPoint cptEndROIPoint = new CPoint(cptStartROIPoint.X + m_nROIWidth, cptStartROIPoint.Y + m_nROIHeight);
                CRect crtROI = new CRect(cptStartROIPoint, cptEndROIPoint);
                CRect crtHalfLeft = new CRect(cptStartROIPoint, new CPoint(crtROI.Center().X, cptEndROIPoint.Y));
                CRect crtHalfRight = new CRect(new CPoint(crtROI.Center().X, cptStartROIPoint.Y), cptEndROIPoint);

                // ROI따기
                int nTop = GetEdge(mem, crtROI, 50, eSearchDirection.TopToBottom, m_nThreshold, m_bDarkBackground);
                int nBottom = GetEdge(mem, crtROI, 50, eSearchDirection.BottomToTop, m_nThreshold, m_bDarkBackground);
                //CRect crtTopBox = new CRect(new CPoint(cptStartROIPoint.X, cptStartROIPoint.Y + nTop), new CPoint(cptEndROIPoint.X, cptStartROIPoint.Y + nTop + 100));
                //int nLeft = GetEdge(mem, crtTopBox, 10, eSearchDirection.LeftToRight, m_nThreshold, m_bDarkBackground);
                //int nRight = GetEdge(mem, crtTopBox, 10, eSearchDirection.RightToLeft, m_nThreshold, m_bDarkBackground);
                int nLeft = GetBarcodeSideEdge(mem, crtROI, 10, eSearchDirection.LeftToRight, m_nThreshold, m_bDarkBackground);
                int nRight = GetBarcodeSideEdge(mem, crtROI, 10, eSearchDirection.RightToLeft, m_nThreshold, m_bDarkBackground);
                CRect crtBarcode = new CRect(m_cptBarcodeLTPoint.X + nLeft, m_cptBarcodeLTPoint.Y + nTop, m_cptBarcodeLTPoint.X + nRight, m_cptBarcodeLTPoint.Y + nBottom);
                Mat matBarcode = m_module.GetMatImage(mem, crtBarcode);
                matBarcode.Save("D:\\BeforeRotation.bmp");

                // 회전각도 알아내기
                int nLeftTop = GetEdge(mem, crtHalfLeft, 10, eSearchDirection.TopToBottom, m_nThreshold, m_bDarkBackground);
                CPoint cptLeftTop = new CPoint(crtHalfLeft.Center().X, nLeftTop);
                int nRightTop = GetEdge(mem, crtHalfRight, 10, eSearchDirection.TopToBottom, m_nThreshold, m_bDarkBackground);
                CPoint cptRightTop = new CPoint(crtHalfRight.Center().X, nRightTop);
                double dThetaRadian = Math.Atan2((double)(cptRightTop.Y - cptLeftTop.Y), (double)(cptRightTop.X - cptLeftTop.X));
                double dThetaDegree = dThetaRadian * (180 / Math.PI);

                // Barcode 회전
                Mat matAffine = new Mat();
                Mat matRotation = new Mat();
                CvInvoke.GetRotationMatrix2D(new System.Drawing.PointF(matBarcode.Width / 2, matBarcode.Height / 2), dThetaDegree, 1.0, matAffine);
                CvInvoke.WarpAffine(matBarcode, matRotation, matAffine, new System.Drawing.Size(matBarcode.Width, matBarcode.Height));
                matRotation.Save("D:\\AfterRotation.bmp");

                // 회전 후 외곽영역 Cutting
                int y1 = 100;
                int y2 = matRotation.Rows - 100;
                int x1 = 100;
                int x2 = matRotation.Cols - 100;
                Mat matCutting = new Mat(matRotation, new Range(y1, y2), new Range(x1, x2));
                matCutting.Save("D:\\Cutting.bmp");

                // Profile 구하기
                Mat matSub = GetRowProfileMat(matCutting);

                // 차영상 구하기
                Mat matResult;
                if (m_bDarkBackground) matResult = matSub - matCutting;
                else matResult = matCutting - matSub;
                //Mat matResult = matCutting - matSub;
                matResult.Save("D:\\Result.bmp");

                // 차영상에서 Blob Labeling
                Mat matBinary = new Mat();
                CvInvoke.Threshold(matResult, matBinary, 70, 255, ThresholdType.Binary);
                matBinary.Save("D:\\BinaryResult.bmp");
                CvBlobs blobs = new CvBlobs();
                CvBlobDetector blobDetector = new CvBlobDetector();
                Image<Gray, byte> img = matBinary.ToImage<Gray, byte>();
                blobDetector.Detect(img, blobs);

                foreach (CvBlob blob in blobs.Values)
                {
                    if (blob.BoundingBox.Width > 10/*Spec*/ || blob.BoundingBox.Height > 10/*Spec*/)
                    {
                        m_module.p_bBarcodePass = false;
                        return "Fail";
                    }
                    //Console.WriteLine("Width:" + blob.BoundingBox.Width + ", Height:" + blob.BoundingBox.Height);
                }

                return "OK";
            }

            unsafe int GetEdge(MemoryData mem, CRect crtROI, int nProfileSize, eSearchDirection eDirection, int nThreshold, bool bDarkBackground)
            {
                if (nProfileSize > crtROI.Width) return 0;
                if (nProfileSize > crtROI.Height) return 0;

                // variable
                ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
                IntPtr p = mem.GetPtr();
                byte* bp;

                // implement
                img.SetData(p, crtROI, (int)mem.W);
                int nCount = 0;
                switch (eDirection)
                {
                    case eSearchDirection.TopToBottom:
                        for (int y = 0; y < img.p_Size.Y; y++)
                        {
                            nCount = 0;
                            bp = (byte*)img.GetPtr() + y * img.p_Stride + (img.p_Size.X / 2);
                            for (int x = -(nProfileSize / 2); x < (nProfileSize / 2); x++)
                            {
                                byte* bpCurrent = bp + x;
                                if (bDarkBackground)
                                {
                                    if (*bpCurrent > nThreshold) nCount++;
                                }
                                else
                                {
                                    if (*bpCurrent < nThreshold) nCount++;
                                }
                            }
                            if (nCount == nProfileSize) return y;
                        }
                        break;
                    case eSearchDirection.LeftToRight:
                        for (int x = 0; x < img.p_Size.X; x++)
                        {
                            nCount = 0;
                            bp = (byte*)img.GetPtr() + x + (img.p_Size.Y / 2) * img.p_Stride;
                            for (int y = -(nProfileSize / 2); y < (nProfileSize / 2); y++)
                            {
                                byte* bpCurrent = bp + y * img.p_Stride;
                                if (bDarkBackground)
                                {
                                    if (*bpCurrent > nThreshold) nCount++;
                                }
                                else
                                {
                                    if (*bpCurrent < nThreshold) nCount++;
                                }
                            }
                            if (nCount == nProfileSize) return x;
                        }
                        break;
                    case eSearchDirection.RightToLeft:
                        for (int x = img.p_Size.X - 1; x >= 0; x--)
                        {
                            nCount = 0;
                            bp = (byte*)img.GetPtr() + x + (img.p_Size.Y / 2) * img.p_Stride;
                            for (int y = -(nProfileSize / 2); y < (nProfileSize / 2); y++)
                            {
                                byte* bpCurrent = bp + y * img.p_Stride;
                                if (bDarkBackground)
                                {
                                    if (*bpCurrent > nThreshold) nCount++;
                                }
                                else
                                {
                                    if (*bpCurrent < nThreshold) nCount++;
                                }
                            }
                            if (nCount == nProfileSize) return x;
                        }
                        break;
                    case eSearchDirection.BottomToTop:
                        for (int y = img.p_Size.Y - 2; y >= 0; y--) // img의 마지막줄은 0으로 채워질 수 있기 때문에 마지막의 전줄부터 탐색
                        {
                            nCount = 0;
                            bp = (byte*)img.GetPtr() + y * img.p_Stride + (img.p_Size.X / 2);
                            for (int x = -(nProfileSize / 2); x < (nProfileSize / 2); x++)
                            {
                                byte* bpCurrent = bp + x;
                                if (bDarkBackground)
                                {
                                    if (*bpCurrent > nThreshold) nCount++;
                                }
                                else
                                {
                                    if (*bpCurrent < nThreshold) nCount++;
                                }
                            }
                            if (nCount == nProfileSize) return y;
                        }
                        break;
                }

                return 0;
            }

            unsafe int GetBarcodeSideEdge(MemoryData mem, CRect crtROI, int nProfileSize, eSearchDirection eDirection, int nThreshold, bool bDarkBackground)
            {
                if (nProfileSize > crtROI.Width) return 0;
                if (nProfileSize > crtROI.Height) return 0;

                // variable
                ImageData img = new ImageData(crtROI.Width, crtROI.Height, 1);
                IntPtr p = mem.GetPtr();
                byte* bp;

                // implement
                img.SetData(p, crtROI, (int)mem.W);
                int nFlipCount = 0;
                bool bCurrentDark = false;
                if (bDarkBackground) bCurrentDark = true;
                
                switch (eDirection)
                {
                    case eSearchDirection.LeftToRight:

                        for (int x = 0; x < img.p_Size.X; x++)
                        {
                            nFlipCount = 0;
                            for (int y = 0; y < img.p_Size.Y; y++)
                            {
                                bp = (byte*)img.GetPtr() + y * img.p_Stride + x;
                                if (bDarkBackground)
                                {
                                    if (*bp > nThreshold)
                                    {
                                        bCurrentDark = !bCurrentDark;
                                        nFlipCount++;
                                    }
                                }
                                else
                                {
                                    if (*bp < nThreshold)
                                    {
                                        bCurrentDark = !bCurrentDark;
                                        nFlipCount++;
                                    }
                                }
                            }
                            if (nFlipCount > 10) return x;
                        }
                        return 0;

                        break;
                    case eSearchDirection.RightToLeft:

                        for (int x = img.p_Size.X - 1; x >= 0; x--)
                        {
                            nFlipCount = 0;
                            for (int y = 0; y < img.p_Size.Y; y++)
                            {
                                bp = (byte*)img.GetPtr() + y * img.p_Stride + x;
                                if (bDarkBackground)
                                {
                                    if (*bp > nThreshold)
                                    {
                                        bCurrentDark = !bCurrentDark;
                                        nFlipCount++;
                                    }
                                }
                                else
                                {
                                    if (*bp < nThreshold)
                                    {
                                        bCurrentDark = !bCurrentDark;
                                        nFlipCount++;
                                    }
                                }
                            }
                            if (nFlipCount > 10) return x;
                        }
                        return 0;

                        break;
                    default:
                        return 0;
                        break;
                }

                return 0;
            }

            unsafe Mat GetRowProfileMat(Mat matSrc)
            {
                // variable
                long lSum = 0;
                byte* bp = null;
                Mat matReturn = new Mat(matSrc.Size, matSrc.Depth, matSrc.NumberOfChannels);
                Image<Gray, byte> img = matReturn.ToImage<Gray, byte>();
                // implement
                for (int y = 0; y < matSrc.Rows; y++)
                {
                    lSum = 0;
                    for (int x = 0; x < matSrc.Cols; x++)
                    {
                        bp = (byte*)matSrc.DataPointer + y * matSrc.Step + x;
                        lSum += *bp;
                    }
                    for (int x = 0; x < matReturn.Cols; x++)
                    {
                        img.Data[y, x, 0] = (byte)(lSum / matSrc.Cols);
                    }
                }
                matReturn = img.Mat;

                return matReturn;
            }
        }
        #endregion

        #region Make Align Template Image
        public class Run_MakeAlignTemplateImage : ModuleRunBase
        {
            public enum eSelectedTemplate
            {
                AlignMark,
                InOutFeature,
                AlignKey,
            }

            MainVision m_module;

            public eSelectedTemplate m_eSelectedTemplate = eSelectedTemplate.AlignMark;

            public CPoint m_cptTopAlignMarkCenterPos = new CPoint();
            public int m_nTopWidth = 500;
            public int m_nTopHeight = 500;
            public CPoint m_cptBottomAlignMarkCenterPos = new CPoint();
            public int m_nBottomWidth = 500;
            public int m_nBottomHeight = 500;

            public CPoint m_cptOutLTCenterPos = new CPoint();
            public int m_nOutLTWidth = 500;
            public int m_nOutLTHeight = 500;
            public CPoint m_cptOutRTCenterPos = new CPoint();
            public int m_nOutRTWidth = 500;
            public int m_nOutRTHeight = 500;
            public CPoint m_cptOutRBCenterPos = new CPoint();
            public int m_nOutRBWidth = 500;
            public int m_nOutRBHeight = 500;
            public CPoint m_cptOutLBCenterPos = new CPoint();
            public int m_nOutLBWidth = 500;
            public int m_nOutLBHeight = 500;

            public CPoint m_cptInLTCenterPos = new CPoint();
            public int m_nInLTWidth = 500;
            public int m_nInLTHeight = 500;
            public CPoint m_cptInRTCenterPos = new CPoint();
            public int m_nInRTWidth = 500;
            public int m_nInRTHeight = 500;
            public CPoint m_cptInRBCenterPos = new CPoint();
            public int m_nInRBWidth = 500;
            public int m_nInRBHeight = 500;
            public CPoint m_cptInLBCenterPos = new CPoint();
            public int m_nInLBWidth = 500;
            public int m_nInLBHeight = 500;

            // Align Key
            public CPoint m_cptLTAlignKeyCenterPos = new CPoint();
            public int m_nLTAlignKeyWidth = 500;
            public int m_nLTAlignKeyHeight = 500;
            public CPoint m_cptRTAlignKeyCenterPos = new CPoint();
            public int m_nRTAlignKeyWidth = 500;
            public int m_nRTAlignKeyHeight = 500;
            public CPoint m_cptRBAlignKeyCenterPos = new CPoint();
            public int m_nRBAlignKeyWidth = 500;
            public int m_nRBAlignKeyHeight = 500;
            public CPoint m_cptLBAlignKeyCenterPos = new CPoint();
            public int m_nLBAlignKeyWidth = 500;
            public int m_nLBAlignKeyHeight = 500;

            public Run_MakeAlignTemplateImage(MainVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_MakeAlignTemplateImage run = new Run_MakeAlignTemplateImage(m_module);

                run.m_eSelectedTemplate = m_eSelectedTemplate;

                run.m_cptTopAlignMarkCenterPos = m_cptTopAlignMarkCenterPos;
                run.m_nTopWidth = m_nTopWidth;
                run.m_nTopHeight = m_nTopHeight;
                run.m_cptBottomAlignMarkCenterPos = m_cptBottomAlignMarkCenterPos;
                run.m_nBottomWidth = m_nBottomWidth;
                run.m_nBottomHeight = m_nBottomHeight;

                run.m_cptOutLTCenterPos = m_cptOutLTCenterPos;
                run.m_nOutLTWidth = m_nOutLTWidth;
                run.m_nOutLTHeight = m_nOutLTHeight;
                run.m_cptOutRTCenterPos = m_cptOutRTCenterPos;
                run.m_nOutRTWidth = m_nOutRTWidth;
                run.m_nOutRTHeight = m_nOutRTHeight;
                run.m_cptOutRBCenterPos = m_cptOutRBCenterPos;
                run.m_nOutRBWidth = m_nOutRBWidth;
                run.m_nOutRBHeight = m_nOutRBHeight;
                run.m_cptOutLBCenterPos = m_cptOutLBCenterPos;
                run.m_nOutLBWidth = m_nOutLBWidth;
                run.m_nOutLBHeight = m_nOutLBHeight;

                run.m_cptInLTCenterPos = m_cptInLTCenterPos;
                run.m_nInLTWidth = m_nInLTWidth;
                run.m_nInLTHeight = m_nInLTHeight;
                run.m_cptInRTCenterPos = m_cptInRTCenterPos;
                run.m_nInRTWidth = m_nInRTWidth;
                run.m_nInRTHeight = m_nInRTHeight;
                run.m_cptInRBCenterPos = m_cptInRBCenterPos;
                run.m_nInRBWidth = m_nInRBWidth;
                run.m_nInRBHeight = m_nInRBHeight;
                run.m_cptInLBCenterPos = m_cptInLBCenterPos;
                run.m_nInLBWidth = m_nInLBWidth;
                run.m_nInLBHeight = m_nInLBHeight;

                // Align Key
                run.m_cptLTAlignKeyCenterPos = m_cptLTAlignKeyCenterPos;
                run.m_nLTAlignKeyWidth = m_nLTAlignKeyWidth;
                run.m_nLTAlignKeyHeight = m_nLTAlignKeyHeight;
                run.m_cptRTAlignKeyCenterPos = m_cptRTAlignKeyCenterPos;
                run.m_nRTAlignKeyWidth = m_nRTAlignKeyWidth;
                run.m_nRTAlignKeyHeight = m_nRTAlignKeyHeight;
                run.m_cptRBAlignKeyCenterPos = m_cptRBAlignKeyCenterPos;
                run.m_nRBAlignKeyWidth = m_nRBAlignKeyWidth;
                run.m_nRBAlignKeyHeight = m_nRBAlignKeyHeight;
                run.m_cptLBAlignKeyCenterPos = m_cptLBAlignKeyCenterPos;
                run.m_nLBAlignKeyWidth = m_nLBAlignKeyWidth;
                run.m_nLBAlignKeyHeight = m_nLBAlignKeyHeight;

                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eSelectedTemplate = (eSelectedTemplate)tree.Set(m_eSelectedTemplate, m_eSelectedTemplate, "Selected Template", "Selected Template", bVisible);

                m_cptTopAlignMarkCenterPos = ((tree.GetTree("Align Mark", false, bVisible)).GetTree("Top Align Mark ROI", false, bVisible)).Set(m_cptTopAlignMarkCenterPos, m_cptTopAlignMarkCenterPos, "Top Align Mark Center Position", "Top Align Mark Center Position", bVisible);
                m_nTopWidth = ((tree.GetTree("Align Mark", false, bVisible)).GetTree("Top Align Mark ROI", false, bVisible)).Set(m_nTopWidth, m_nTopWidth, "Top Align Mark Width", "Top Align Mark Width", bVisible);
                m_nTopHeight = ((tree.GetTree("Align Mark", false, bVisible)).GetTree("Top Align Mark ROI", false, bVisible)).Set(m_nTopHeight, m_nTopHeight, "Top Align Mark Height", "Top Align Mark Height", bVisible);
                m_cptBottomAlignMarkCenterPos = ((tree.GetTree("Align Mark", false, bVisible)).GetTree("Bottom Align Mark ROI", false, bVisible)).Set(m_cptBottomAlignMarkCenterPos, m_cptBottomAlignMarkCenterPos, "Bottom Align Mark Center Position", "Bottom Align Mark Center Position", bVisible);
                m_nBottomWidth = ((tree.GetTree("Align Mark", false, bVisible)).GetTree("Bottom Align Mark ROI", false, bVisible)).Set(m_nBottomWidth, m_nBottomWidth, "Bottom Align Mark Width", "Bottom Align Mark Width", bVisible);
                m_nBottomHeight = ((tree.GetTree("Align Mark", false, bVisible)).GetTree("Bottom Align Mark ROI", false, bVisible)).Set(m_nBottomHeight, m_nBottomHeight, "Bottom Align Mark Height", "Bottom Align Mark Height", bVisible);

                m_cptOutLTCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Left Top", false, bVisible)).Set(m_cptOutLTCenterPos, m_cptOutLTCenterPos, "Out Left Top Center Position", "Out Left Top Center Position", bVisible);
                m_nOutLTWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Left Top", false, bVisible)).Set(m_nOutLTWidth, m_nOutLTWidth, "Out Left Top Width", "Out Left Top Width", bVisible);
                m_nOutLTHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Left Top", false, bVisible)).Set(m_nOutLTHeight, m_nOutLTHeight, "Out Left Top Height", "Out Left Top Height", bVisible);
                m_cptOutRTCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Right Top", false, bVisible)).Set(m_cptOutRTCenterPos, m_cptOutRTCenterPos, "Out Right Top Center Position", "Out Right Top Center Position", bVisible);
                m_nOutRTWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Right Top", false, bVisible)).Set(m_nOutRTWidth, m_nOutRTWidth, "Out Right Top Width", "Out Right Top Width", bVisible);
                m_nOutRTHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Right Top", false, bVisible)).Set(m_nOutRTHeight, m_nOutRTHeight, "Out Right Top Height", "Out Right Top Height", bVisible);
                m_cptOutRBCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Right Bottom", false, bVisible)).Set(m_cptOutRBCenterPos, m_cptOutRBCenterPos, "Out Right Bottom Center Position", "Out Right Bottom Center Position", bVisible);
                m_nOutRBWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Right Bottom", false, bVisible)).Set(m_nOutRBWidth, m_nOutRBWidth, "Out Right Bottom Width", "Out Right Bottom Width", bVisible);
                m_nOutRBHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Right Bottom", false, bVisible)).Set(m_nOutRBHeight, m_nOutRBHeight, "Out Right Bottom Height", "Out Right Bottom Height", bVisible);
                m_cptOutLBCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Left Bottom", false, bVisible)).Set(m_cptOutLBCenterPos, m_cptOutLBCenterPos, "Out Left Bottom Center Position", "Out Left Bottom Center Position", bVisible);
                m_nOutLBWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Left Bottom", false, bVisible)).Set(m_nOutLBWidth, m_nOutLBWidth, "Out Left Bottom Width", "Out Left Bottom Width", bVisible);
                m_nOutLBHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("Out Left Bottom", false, bVisible)).Set(m_nOutLBHeight, m_nOutLBHeight, "Out Left Bottom Height", "Out Left Bottom Height", bVisible);

                m_cptInLTCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Left Top", false, bVisible)).Set(m_cptInLTCenterPos, m_cptInLTCenterPos, "In Left Top Center Position", "In Left Top Center Position", bVisible);
                m_nInLTWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Left Top", false, bVisible)).Set(m_nInLTWidth, m_nInLTWidth, "In Left Top Width", "In Left Top Width", bVisible);
                m_nInLTHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Left Top", false, bVisible)).Set(m_nInLTHeight, m_nInLTHeight, "In Left Top Height", "In Left Top Height", bVisible);
                m_cptInRTCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Right Top", false, bVisible)).Set(m_cptInRTCenterPos, m_cptInRTCenterPos, "In Right Top Center Position", "In Right Top Center Position", bVisible);
                m_nInRTWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Right Top", false, bVisible)).Set(m_nInRTWidth, m_nInRTWidth, "In Right Top Width", "In Right Top Width", bVisible);
                m_nInRTHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Right Top", false, bVisible)).Set(m_nInRTHeight, m_nInRTHeight, "In Right Top Height", "In Right Top Height", bVisible);
                m_cptInRBCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Right Bottom", false, bVisible)).Set(m_cptInRBCenterPos, m_cptInRBCenterPos, "In Right Bottom Center Position", "In Right Bottom Center Position", bVisible);
                m_nInRBWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Right Bottom", false, bVisible)).Set(m_nInRBWidth, m_nInRBWidth, "In Right Bottom Width", "In Right Bottom Width", bVisible);
                m_nInRBHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Right Bottom", false, bVisible)).Set(m_nInRBHeight, m_nInRBHeight, "In Right Bottom Height", "In Right Bottom Height", bVisible);
                m_cptInLBCenterPos = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Left Bottom", false, bVisible)).Set(m_cptInLBCenterPos, m_cptInLBCenterPos, "In Left Bottom Center Position", "In Left Bottom Center Position", bVisible);
                m_nInLBWidth = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Left Bottom", false, bVisible)).Set(m_nInLBWidth, m_nInLBWidth, "In Left Bottom Width", "In Left Bottom Width", bVisible);
                m_nInLBHeight = ((tree.GetTree("InOutFeature", false, bVisible)).GetTree("In Left Bottom", false, bVisible)).Set(m_nInLBHeight, m_nInLBHeight, "In Left Bottom Height", "In Left Bottom Height", bVisible);

                // Align Key
                m_cptLTAlignKeyCenterPos = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Left Top Align Key ROI", false, bVisible)).Set(m_cptLTAlignKeyCenterPos, m_cptLTAlignKeyCenterPos, "Left Top Center Position", "Left Top Center Position", bVisible);
                m_nLTAlignKeyWidth = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Left Top Align Key ROI", false, bVisible)).Set(m_nLTAlignKeyWidth, m_nLTAlignKeyWidth, "Left Top Width", "Left Top Width", bVisible);
                m_nLTAlignKeyHeight = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Left Top Align Key ROI", false, bVisible)).Set(m_nLTAlignKeyHeight, m_nLTAlignKeyHeight, "Left Top Height", "Left Top Height", bVisible);
                m_cptRTAlignKeyCenterPos = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Right Top Align Key ROI", false, bVisible)).Set(m_cptRTAlignKeyCenterPos, m_cptRTAlignKeyCenterPos, "Right Top Center Position", "Right Top Center Position", bVisible);
                m_nRTAlignKeyWidth = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Right Top Align Key ROI", false, bVisible)).Set(m_nRTAlignKeyWidth, m_nRTAlignKeyWidth, "Right Top Width", "Right Top Width", bVisible);
                m_nRTAlignKeyHeight = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Right Top Align Key ROI", false, bVisible)).Set(m_nRTAlignKeyHeight, m_nRTAlignKeyHeight, "Right Top Height", "Right Top Height", bVisible);
                m_cptRBAlignKeyCenterPos = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Right Bottom Align Key ROI", false, bVisible)).Set(m_cptRBAlignKeyCenterPos, m_cptRBAlignKeyCenterPos, "Right Bottom Center Position", "Right Bottom Center Position", bVisible);
                m_nRBAlignKeyWidth = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Right Bottom Align Key ROI", false, bVisible)).Set(m_nRBAlignKeyWidth, m_nRBAlignKeyWidth, "Right Bottom Width", "Right Bottom Width", bVisible);
                m_nRBAlignKeyHeight = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Right Bottom Align Key ROI", false, bVisible)).Set(m_nRBAlignKeyHeight, m_nRBAlignKeyHeight, "Right Bottom Height", "Right Bottom Height", bVisible);
                m_cptLBAlignKeyCenterPos = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Left Bottom Align Key ROI", false, bVisible)).Set(m_cptLBAlignKeyCenterPos, m_cptLBAlignKeyCenterPos, "Left Bottom Center Position", "Left Bottom Center Position", bVisible);
                m_nLBAlignKeyWidth = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Left Bottom Align Key ROI", false, bVisible)).Set(m_nLBAlignKeyWidth, m_nLBAlignKeyWidth, "Left Bottom Width", "Left Bottom Width", bVisible);
                m_nLBAlignKeyHeight = ((tree.GetTree("Align Key", false, bVisible)).GetTree("Left Bottom Align Key ROI", false, bVisible)).Set(m_nLBAlignKeyHeight, m_nLBAlignKeyHeight, "Left Bottom Height", "Left Bottom Height", bVisible);
            }

            public override string Run()
            {
                // variable
                Mat matTopTemplateImage = new Mat();
                Mat matBottomTemplateImage = new Mat();
                MemoryData mem = m_module.m_engineer.GetMemory(App.mPool, App.mGroup, App.mMainMem);
                string strAlignMarkPath = "D:\\AlignMarkTemplateImage\\";
                string strInOutFeaturePath = "D:\\FeatureTemplateImage\\";
                string strAlignKeyPath = "D:\\AlignKeyTemplateImage\\";

                // implement
                switch (m_eSelectedTemplate)
                {
                    case eSelectedTemplate.AlignMark:

                        if (!Directory.Exists(strAlignMarkPath))
                            Directory.CreateDirectory(strAlignMarkPath);

                        Mat matTopAlignMarkImage = new Mat();
                        Mat matBottomAlignMarkImage = new Mat();
                        CRect crtTopAlignMarkROI = new CRect(m_cptTopAlignMarkCenterPos, m_nTopWidth, m_nTopHeight);
                        CRect crtBottomAlignMarkROI = new CRect(m_cptBottomAlignMarkCenterPos, m_nBottomWidth, m_nBottomHeight);

                        matTopAlignMarkImage = m_module.GetMatImage(mem, crtTopAlignMarkROI);
                        if (matTopAlignMarkImage != null) matTopAlignMarkImage.Save(Path.Combine(strAlignMarkPath, "TopTemplateImage.bmp"));
                        matBottomAlignMarkImage = m_module.GetMatImage(mem, crtBottomAlignMarkROI);
                        if (matBottomAlignMarkImage != null) matBottomAlignMarkImage.Save(Path.Combine(strAlignMarkPath, "BottomTemplateImage.bmp"));

                        break;
                    case eSelectedTemplate.InOutFeature:

                        if (!Directory.Exists(strInOutFeaturePath))
                            Directory.CreateDirectory(strInOutFeaturePath);

                        Mat matOutLTImage = new Mat();
                        Mat matOutRTImage = new Mat();
                        Mat matOutRBImage = new Mat();
                        Mat matOutLBImage = new Mat();
                        Mat matInLTImage = new Mat();
                        Mat matInRTImage = new Mat();
                        Mat matInRBImage = new Mat();
                        Mat matInLBImage = new Mat();
                        CRect crtOutLTROI = new CRect(m_cptOutLTCenterPos, m_nOutLTWidth, m_nOutLTHeight);
                        CRect crtOutRTROI = new CRect(m_cptOutRTCenterPos, m_nOutRTWidth, m_nOutRTHeight);
                        CRect crtOutRBROI = new CRect(m_cptOutRBCenterPos, m_nOutRBWidth, m_nOutRBHeight);
                        CRect crtOutLBROI = new CRect(m_cptOutLBCenterPos, m_nOutLBWidth, m_nOutLBHeight);
                        CRect crtInLTROI = new CRect(m_cptInLTCenterPos, m_nInLTWidth, m_nInLTHeight);
                        CRect crtInRTROI = new CRect(m_cptInRTCenterPos, m_nInRTWidth, m_nInRTHeight);
                        CRect crtInRBROI = new CRect(m_cptInRBCenterPos, m_nInRBWidth, m_nInRBHeight);
                        CRect crtInLBROI = new CRect(m_cptInLBCenterPos, m_nInLBWidth, m_nInLBHeight);

                        matOutLTImage = m_module.GetMatImage(mem, crtOutLTROI);
                        if (matOutLTImage != null) matOutLTImage.Save(Path.Combine(strInOutFeaturePath, "OutLT.bmp"));
                        matOutRTImage = m_module.GetMatImage(mem, crtOutRTROI);
                        if (matOutRTImage != null) matOutRTImage.Save(Path.Combine(strInOutFeaturePath, "OutRT.bmp"));
                        matOutRBImage = m_module.GetMatImage(mem, crtOutRBROI);
                        if (matOutRBImage != null) matOutRBImage.Save(Path.Combine(strInOutFeaturePath, "OutRB.bmp"));
                        matOutLBImage = m_module.GetMatImage(mem, crtOutLBROI);
                        if (matOutLBImage != null) matOutLBImage.Save(Path.Combine(strInOutFeaturePath, "OutLB.bmp"));
                        matInLTImage = m_module.GetMatImage(mem, crtInLTROI);
                        if (matInLTImage != null) matInLTImage.Save(Path.Combine(strInOutFeaturePath, "InLT.bmp"));
                        matInRTImage = m_module.GetMatImage(mem, crtInRTROI);
                        if (matInRTImage != null) matInRTImage.Save(Path.Combine(strInOutFeaturePath, "InRT.bmp"));
                        matInRBImage = m_module.GetMatImage(mem, crtInRBROI);
                        if (matInRBImage != null) matInRBImage.Save(Path.Combine(strInOutFeaturePath, "InRB.bmp"));
                        matInLBImage = m_module.GetMatImage(mem, crtInLBROI);
                        if (matInLBImage != null) matInLBImage.Save(Path.Combine(strInOutFeaturePath, "InLB.bmp"));

                        break;
                    case eSelectedTemplate.AlignKey:

                        if (!Directory.Exists(strAlignKeyPath))
                            Directory.CreateDirectory(strAlignKeyPath);

                        Mat matLTAlignKeyImage = new Mat();
                        Mat matRTAlignKeyImage = new Mat();
                        Mat matRBAlignKeyImage = new Mat();
                        Mat matLBAlignKeyImage = new Mat();
                        CRect crtLTAlignKeyROI = new CRect(m_cptLTAlignKeyCenterPos, m_nLTAlignKeyWidth, m_nLTAlignKeyHeight);
                        CRect crtRTAlignKeyROI = new CRect(m_cptRTAlignKeyCenterPos, m_nRTAlignKeyWidth, m_nRTAlignKeyHeight);
                        CRect crtRBAlignKeyROI = new CRect(m_cptRBAlignKeyCenterPos, m_nRBAlignKeyWidth, m_nRBAlignKeyHeight);
                        CRect crtLBAlignKeyROI = new CRect(m_cptLBAlignKeyCenterPos, m_nLBAlignKeyWidth, m_nLBAlignKeyHeight);

                        matLTAlignKeyImage = m_module.GetMatImage(mem, crtLTAlignKeyROI);
                        if (matLTAlignKeyImage != null) matLTAlignKeyImage.Save(Path.Combine(strAlignKeyPath, "LT.bmp"));
                        matRTAlignKeyImage = m_module.GetMatImage(mem, crtRTAlignKeyROI);
                        if (matRTAlignKeyImage != null) matRTAlignKeyImage.Save(Path.Combine(strAlignKeyPath, "RT.bmp"));
                        matRBAlignKeyImage = m_module.GetMatImage(mem, crtRBAlignKeyROI);
                        if (matRBAlignKeyImage != null) matRBAlignKeyImage.Save(Path.Combine(strAlignKeyPath, "RB.bmp"));
                        matLBAlignKeyImage = m_module.GetMatImage(mem, crtLBAlignKeyROI);
                        if (matLBAlignKeyImage != null) matLBAlignKeyImage.Save(Path.Combine(strAlignKeyPath, "LB.bmp"));

                        break;
                }

                return "OK";
            }
        }
        #endregion

        #region PatternAlign
        public class Run_PatternAlign : ModuleRunBase
        {
            MainVision m_module;
            public int m_nSearchAreaSize = 1000;
            public double m_dMatchScore = 0.4;
            public string m_strTopTemplateImageFilePath = "D:\\AlignMarkTemplateImage\\TopTemplateImage.bmp";
            public string m_strBottomTemplateImageFilePath = "D:\\AlignMarkTemplateImage\\BottomTemplateImage.bmp";

            public Run_PatternAlign(MainVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_PatternAlign run = new Run_PatternAlign(m_module);
                run.m_nSearchAreaSize = m_nSearchAreaSize;
                run.m_dMatchScore = m_dMatchScore;
                run.m_strTopTemplateImageFilePath = m_strTopTemplateImageFilePath;
                run.m_strBottomTemplateImageFilePath = m_strBottomTemplateImageFilePath;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nSearchAreaSize = tree.Set(m_nSearchAreaSize, m_nSearchAreaSize, "Template Matching Search Area Size", "Template Matching Search Area Size", bVisible);
                m_dMatchScore = tree.Set(m_dMatchScore, m_dMatchScore, "Template Matching Pass Score", "Template Matching Pass Score", bVisible);
                m_strTopTemplateImageFilePath = (tree.GetTree("Template Image Path", false, bVisible)).SetFile(m_strTopTemplateImageFilePath, m_strTopTemplateImageFilePath, "bmp", "Top Template Image Path", "Top Template Image Path", bVisible);
                m_strBottomTemplateImageFilePath = (tree.GetTree("Template Image Path", false, bVisible)).SetFile(m_strBottomTemplateImageFilePath, m_strBottomTemplateImageFilePath, "bmp", "Bottom Template Image Path", "Bottom Template Image Path", bVisible);
            }

            public override string Run()
            {
                // variable
                Image<Gray, byte> imgTop = new Image<Gray, byte>(m_strTopTemplateImageFilePath);
                Image<Gray, byte> imgBottom = new Image<Gray, byte>(m_strBottomTemplateImageFilePath);
                CPoint cptTopCenter = new CPoint();
                CPoint cptBottomCenter = new CPoint();
                MemoryData mem = m_module.m_engineer.GetMemory(App.mPool, App.mGroup, App.mMainMem);
                bool bFoundTop = false;
                bool bFoundBottom = false;
                CPoint cptTopResultCenter;
                CPoint cptBottomResultCenter;

                // implement
                Run_MakeAlignTemplateImage moduleRun = (Run_MakeAlignTemplateImage)m_module.CloneModuleRun("MakeAlignTemplateImage");
                cptTopCenter.X = moduleRun.m_cptTopAlignMarkCenterPos.X;
                cptTopCenter.Y = moduleRun.m_cptTopAlignMarkCenterPos.Y;
                cptBottomCenter.X = moduleRun.m_cptBottomAlignMarkCenterPos.X;
                cptBottomCenter.Y = moduleRun.m_cptBottomAlignMarkCenterPos.Y;

                // Top Template Image Processing
                Point ptStart = new Point(cptTopCenter.X - (m_nSearchAreaSize / 2), cptTopCenter.Y - (m_nSearchAreaSize / 2));
                Point ptEnd = new Point(cptTopCenter.X + (m_nSearchAreaSize / 2), cptTopCenter.Y + (m_nSearchAreaSize / 2));
                CRect crtSearchArea = new CRect(ptStart, ptEnd);
                Mat matSearchArea = m_module.GetMatImage(mem, crtSearchArea);
                Image<Gray, byte> imgSrc = matSearchArea.ToImage<Gray, byte>();
                bFoundTop = m_module.TemplateMatching(mem, crtSearchArea, imgSrc, imgTop, out cptTopResultCenter, m_dMatchScore);

                // Bottom Template Image Processing
                ptStart = new Point(cptBottomCenter.X - (m_nSearchAreaSize / 2), cptBottomCenter.Y - (m_nSearchAreaSize / 2));
                ptEnd = new Point(cptBottomCenter.X + (m_nSearchAreaSize / 2), cptBottomCenter.Y + (m_nSearchAreaSize / 2));
                crtSearchArea = new CRect(ptStart, ptEnd);
                matSearchArea = m_module.GetMatImage(mem, crtSearchArea);
                imgSrc = matSearchArea.ToImage<Gray, byte>();
                bFoundBottom = m_module.TemplateMatching(mem, crtSearchArea, imgSrc, imgTop, out cptBottomResultCenter, m_dMatchScore);

                // Calculate Theta
                if (bFoundTop && bFoundBottom)  // Top & Bottom 모두 Template Matching 성공했을 경우
                {
                    double dThetaRadian = Math.Atan2((double)(cptBottomResultCenter.Y - cptTopResultCenter.Y), (double)(cptBottomResultCenter.X - cptTopResultCenter.X));
                    double dThetaDegree = dThetaRadian * (180 / Math.PI);
                    dThetaDegree -= 90;
                    // 1000 Pulse = 1 Degree
                    double dThetaPulse = dThetaDegree * 1000;

                    //Theta축 회전
                    Axis axisRotate = m_module.m_axisRotate;
                    double dActualPos = axisRotate.p_posActual;
                    axisRotate.StartMove(dActualPos - dThetaPulse);

                    //// 회전이미지 
                    //Mat matSrc = GetMatImage(mem, new CRect(1000, 1000, 7000, 33000));
                    //Mat matAffine = new Mat();
                    //Mat matRotation = new Mat();
                    //CvInvoke.GetRotationMatrix2D(new System.Drawing.PointF(matSrc.Width / 2, matSrc.Height / 2), dThetaDegree, 1.0, matAffine);
                    //CvInvoke.WarpAffine(matSrc, matRotation, matAffine, new System.Drawing.Size(matSrc.Width, matSrc.Height));
                    //matRotation.Save("D:\\ROTATIONTEST.BMP");

                    return "OK";
                }
                else
                    return "Fail";
            }
        }
        #endregion

        #region PatternArrayShift & Rotation 검사
        public class Run_PatternShiftAndRotation : ModuleRunBase
        {
            MainVision m_module;
            public int m_nSearchArea = 2000;
            public double m_dMatchScore = 0.95;
            public double m_dNGSpecDistance_um = 100.0;
            public double m_dNGSpecDegree = 0.5;

            public Run_PatternShiftAndRotation(MainVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_PatternShiftAndRotation run = new Run_PatternShiftAndRotation(m_module);
                run.m_nSearchArea = m_nSearchArea;
                run.m_dMatchScore = m_dMatchScore;
                run.m_dNGSpecDistance_um = m_dNGSpecDistance_um;
                run.m_dNGSpecDegree = m_dNGSpecDegree;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nSearchArea = tree.Set(m_nSearchArea, m_nSearchArea, "Search Area Size [px]", "Search Area Size [px]", bVisible);
                m_dMatchScore = tree.Set(m_dMatchScore, m_dMatchScore, "Template Matching Score [0.0~1.0]", "Template Matching Score [0.0~1.0]", bVisible);
                m_dNGSpecDistance_um = tree.GetTree("NG Spec", false, bVisible).Set(m_dNGSpecDistance_um, m_dNGSpecDistance_um, "Distance NG Spec [um]", "Distance NG Spec [um]", bVisible);
                m_dNGSpecDegree = tree.GetTree("NG Spec", false, bVisible).Set(m_dNGSpecDegree, m_dNGSpecDegree, "Degree NG Spec", "Degree NG Spec", bVisible);
            }

            public enum eSearchPoint
            {
                LT,
                RT,
                RB,
                LB,
                Count,
            }
            public override string Run()
            {
                // variable
                string strPool = "MainVision.Vision Memory";
                string strGroup = "MainVision";
                string strMemory = "Main";
                MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                Run_MakeAlignTemplateImage moduleRun = (Run_MakeAlignTemplateImage)m_module.CloneModuleRun("MakeAlignTemplateImage");
                string strFeatureTemplateImagePath = "D:\\FeatureTemplateImage\\";
                Image<Gray, byte> imgSearchArea;
                Image<Gray, byte> imgTemplate;
                Point ptStart, ptEnd;
                CRect crtSearchArea;
                Mat matSearchArea;
                CPoint cptSearchAreaCenter;
                bool bFound = false;
                CPoint[] cptarrOutResultCenterPositions = new CPoint[(int)eSearchPoint.Count];
                CPoint[] cptarrInResultCenterPositions = new CPoint[(int)eSearchPoint.Count];
                CPoint cptOutFeatureCentroid;
                CPoint cptInFeatureCentroid;

                // implement
                // 1. Outside Feature(LT, RT, RB, LB) TemplateMatching
                for (int i = 0; i < (int)eSearchPoint.Count; i++)
                {
                    switch (i)
                    {
                        case (int)eSearchPoint.LT:
                            cptSearchAreaCenter = new CPoint(moduleRun.m_cptOutLTCenterPos);
                            imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "OutLT.bmp"));
                            break;
                        case (int)eSearchPoint.RT:
                            cptSearchAreaCenter = new CPoint(moduleRun.m_cptOutRTCenterPos);
                            imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "OutRT.bmp"));
                            break;
                        case (int)eSearchPoint.RB:
                            cptSearchAreaCenter = new CPoint(moduleRun.m_cptOutRBCenterPos);
                            imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "OutRB.bmp"));
                            break;
                        case (int)eSearchPoint.LB:
                            cptSearchAreaCenter = new CPoint(moduleRun.m_cptOutLBCenterPos);
                            imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "OutLB.bmp"));
                            break;
                        default:
                            cptSearchAreaCenter = new CPoint();
                            imgTemplate = new Image<Gray, byte>(m_nSearchArea, m_nSearchArea);
                            break;
                    }

                    ptStart = new Point(cptSearchAreaCenter.X - (m_nSearchArea / 2), cptSearchAreaCenter.Y - (m_nSearchArea / 2));
                    ptEnd = new Point(cptSearchAreaCenter.X + (m_nSearchArea / 2), cptSearchAreaCenter.Y + (m_nSearchArea / 2));
                    crtSearchArea = new CRect(ptStart, ptEnd);
                    matSearchArea = m_module.GetMatImage(mem, crtSearchArea);
                    imgSearchArea = matSearchArea.ToImage<Gray, byte>();
                    CPoint cptFoundCenter;
                    bFound = m_module.TemplateMatching(mem, crtSearchArea, imgSearchArea, imgTemplate, out cptFoundCenter, m_dMatchScore);
                    if (bFound) cptarrOutResultCenterPositions[i] = new CPoint(cptFoundCenter);
                }
                cptOutFeatureCentroid = GetCentroidFromPolygonPointArray(cptarrOutResultCenterPositions);

                // 2. Inside Feature(LT, RT, RB, LB) TemplateMatching
                for (int i = 0; i<(int)eSearchPoint.Count; i++)
                {
                    switch (i)
                    {
                        case (int)eSearchPoint.LT:
                            cptSearchAreaCenter = new CPoint(moduleRun.m_cptInLTCenterPos);
                            imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "InLT.bmp"));
                            break;
                        case (int)eSearchPoint.RT:
                            cptSearchAreaCenter = new CPoint(moduleRun.m_cptInRTCenterPos);
                            imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "InRT.bmp"));
                            break;
                        case (int)eSearchPoint.RB:
                            cptSearchAreaCenter = new CPoint(moduleRun.m_cptInRBCenterPos);
                            imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "InRB.bmp"));
                            break;
                        case (int)eSearchPoint.LB:
                            cptSearchAreaCenter = new CPoint(moduleRun.m_cptInLBCenterPos);
                            imgTemplate = new Image<Gray, byte>(Path.Combine(strFeatureTemplateImagePath, "InLB.bmp"));
                            break;
                        default:
                            cptSearchAreaCenter = new CPoint();
                            imgTemplate = new Image<Gray, byte>(m_nSearchArea, m_nSearchArea);
                            break;
                    }

                    ptStart = new Point(cptSearchAreaCenter.X - (m_nSearchArea / 2), cptSearchAreaCenter.Y - (m_nSearchArea / 2));
                    ptEnd = new Point(cptSearchAreaCenter.X + (m_nSearchArea / 2), cptSearchAreaCenter.Y + (m_nSearchArea / 2));
                    crtSearchArea = new CRect(ptStart, ptEnd);
                    matSearchArea = m_module.GetMatImage(mem, crtSearchArea);
                    imgSearchArea = matSearchArea.ToImage<Gray, byte>();
                    CPoint cptFoundCenter;
                    bFound = m_module.TemplateMatching(mem, crtSearchArea, imgSearchArea, imgTemplate, out cptFoundCenter, m_dMatchScore);
                    if (bFound) cptarrInResultCenterPositions[i] = new CPoint(cptFoundCenter);
                }
                cptInFeatureCentroid = GetCentroidFromPolygonPointArray(cptarrInResultCenterPositions);

                // Get distance From InFeatureCentroid & OutFeatureCentroid
                double dResultDistance = GetDistanceOfTwoPoint(cptInFeatureCentroid, cptOutFeatureCentroid);
                m_module.p_dPatternShiftDistance = dResultDistance;

                // Get Degree From OutLT & OutRT
                double dThetaRadian = Math.Atan2((double)(cptarrOutResultCenterPositions[(int)eSearchPoint.RT].Y - cptarrOutResultCenterPositions[(int)eSearchPoint.LT].Y),
                                                          cptarrOutResultCenterPositions[(int)eSearchPoint.RT].X - cptarrOutResultCenterPositions[(int)eSearchPoint.LT].X);
                double dThetaDegree = dThetaRadian * (180 / Math.PI);
                m_module.p_dPatternShiftAngle = dThetaDegree;

                // Judgement
                Run_Grab moduleRunGrab = (Run_Grab)m_module.CloneModuleRun("Grab");
                if (m_dNGSpecDistance_um < (dResultDistance * moduleRunGrab.m_dResY_um))
                {
                    m_module.p_bPatternShiftPass = false;
                    return "Fail";
                }
                if (m_dNGSpecDegree < Math.Abs(dThetaDegree))
                {
                    m_module.p_bPatternShiftPass = false;
                    return "Fail";
                }
                
                return "OK";
            }

            // 다각형의 면적과 무게중심을 구하는 알고리즘 * 출처 https://lsit81.tistory.com/entry/%EB%8B%A4%EA%B0%81%ED%98%95-%EB%A9%B4%EC%A0%81%EA%B3%BC-%EB%AC%B4%EA%B2%8C-%EC%A4%91%EC%8B%AC-%EA%B5%AC%ED%95%98%EA%B8%B0
            CPoint GetCentroidFromPolygonPointArray(CPoint[] cptarr)
            {
                // variable
                int j = 0;
                CPoint cpt1, cpt2;
                double dArea = 0.0;
                CPoint cptCentroid = new CPoint();
                double dX1, dX2, dY1, dY2;
                double dCentroidX = 0, dCentroidY = 0;

                // implement
                for (int i = 0; i<cptarr.Length; i++)
                {
                    j = (i + 1) % cptarr.Length;
                    cpt1 = cptarr[i];
                    cpt2 = cptarr[j];
                    dX1 = cpt1.X;
                    dX2 = cpt2.X;
                    dY1 = cpt1.Y;
                    dY2 = cpt2.Y;
                    dArea += ((dX1 * dY2) - (dX2 * dY1));

                    dCentroidX += ((dX1 + dX2) * ((dX1 * dY2) - (dX2 * dY1)));
                    dCentroidY += ((dY1 + dY2) * ((dX1 * dY2) - (dX2 * dY1)));
                }

                dArea /= 2.0;
                dArea = Math.Abs(dArea);

                dCentroidX = (dCentroidX / (6.0 * dArea));
                dCentroidY = (dCentroidY / (6.0 * dArea));

                cptCentroid.X = (int)dCentroidX;
                cptCentroid.Y = (int)dCentroidY;

                return cptCentroid;
            }

            double GetDistanceOfTwoPoint(CPoint cpt1, CPoint cpt2)
            {
                // variable
                double dX1, dX2, dY1, dY2;
                double dResultDistance = 0;

                // implement
                dX1 = cpt1.X;
                dX2 = cpt2.X;
                dY1 = cpt1.Y;
                dY2 = cpt2.Y;

                dResultDistance = Math.Sqrt(((dX1 - dX2) * (dX1 - dX2)) + ((dY1 - dY2) * (dY1 - dY2)));

                return dResultDistance;
            }
        }
        #endregion

        #region Align Key 검사
        public class Run_AlignKeyInspection : ModuleRunBase
        {
            public enum eSearchPoint
            {
                LT,
                RT,
                RB,
                LB,
                Count,
            }

            MainVision m_module;
            public int m_nSearchArea = 2000;
            public double m_dMatchScore = 0.95;
            public int m_nThreshold = 70;
            public int m_nNGSpec_um = 30;

            public Run_AlignKeyInspection(MainVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_AlignKeyInspection run = new Run_AlignKeyInspection(m_module);
                run.m_nSearchArea = m_nSearchArea;
                run.m_dMatchScore = m_dMatchScore;
                run.m_nThreshold = m_nThreshold;
                run.m_nNGSpec_um = m_nNGSpec_um;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nSearchArea = tree.Set(m_nSearchArea, m_nSearchArea, "Search Area Size [px]", "Search Area Size [px]", bVisible);
                m_dMatchScore = tree.Set(m_dMatchScore, m_dMatchScore, "Template Matching Score [0.0~1.0]", "Template Matching Score [0.0~1.0]", bVisible);
                m_nThreshold = tree.Set(m_nThreshold, m_nThreshold, "Binary Threshold [GV]", "Binary Threshold [GV]", bVisible);
                m_nNGSpec_um = tree.Set(m_nNGSpec_um, m_nNGSpec_um, "NG Spec [um]", "NG Spec [um]", bVisible);
            }

            public override string Run()
            {
                // variable
                string strPool = "MainVision.Vision Memory";
                string strGroup = "MainVision";
                string strMemory = "Main";
                MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                Run_MakeAlignTemplateImage moduleRun = (Run_MakeAlignTemplateImage)m_module.CloneModuleRun("MakeAlignTemplateImage");
                string strAlignKeyTemplateImagePath = "D:\\AlignKeyTemplateImage\\";
                Image<Gray, byte> imgSearchArea;
                Image<Gray, byte> imgTemplate;
                Point ptStart, ptEnd;
                CRect crtSearchArea;
                Mat matSearchArea;
                CPoint cptSearchAreaCenter;
                bool bFound = false;
                Mat[] matarr = new Mat[4];

                // implement
                for (int i = 0; i < (int)eSearchPoint.Count; i++)
                {
                    switch (i)
                    {
                        case (int)eSearchPoint.LT:
                            cptSearchAreaCenter = new CPoint(moduleRun.m_cptLTAlignKeyCenterPos);
                            imgTemplate = new Image<Gray, byte>(Path.Combine(strAlignKeyTemplateImagePath, "LT.bmp"));
                            break;
                        case (int)eSearchPoint.RT:
                            cptSearchAreaCenter = new CPoint(moduleRun.m_cptRTAlignKeyCenterPos);
                            imgTemplate = new Image<Gray, byte>(Path.Combine(strAlignKeyTemplateImagePath, "RT.bmp"));
                            break;
                        case (int)eSearchPoint.RB:
                            cptSearchAreaCenter = new CPoint(moduleRun.m_cptRBAlignKeyCenterPos);
                            imgTemplate = new Image<Gray, byte>(Path.Combine(strAlignKeyTemplateImagePath, "RB.bmp"));
                            break;
                        case (int)eSearchPoint.LB:
                            cptSearchAreaCenter = new CPoint(moduleRun.m_cptLBAlignKeyCenterPos);
                            imgTemplate = new Image<Gray, byte>(Path.Combine(strAlignKeyTemplateImagePath, "LB.bmp"));
                            break;
                        default:
                            cptSearchAreaCenter = new CPoint();
                            imgTemplate = new Image<Gray, byte>(m_nSearchArea, m_nSearchArea);
                            break;
                    }

                    ptStart = new Point(cptSearchAreaCenter.X - (m_nSearchArea / 2), cptSearchAreaCenter.Y - (m_nSearchArea / 2));
                    ptEnd = new Point(cptSearchAreaCenter.X + (m_nSearchArea / 2), cptSearchAreaCenter.Y + (m_nSearchArea / 2));
                    crtSearchArea = new CRect(ptStart, ptEnd);
                    matSearchArea = m_module.GetMatImage(mem, crtSearchArea);
                    imgSearchArea = matSearchArea.ToImage<Gray, byte>();
                    CPoint cptFoundCenter;
                    bFound = m_module.TemplateMatching(mem, crtSearchArea, imgSearchArea, imgTemplate, out cptFoundCenter, m_dMatchScore);
                    
                    if (bFound) // Template Matching 성공
                    {
                        ptStart = new Point(cptFoundCenter.X - (imgTemplate.Width / 2), cptFoundCenter.Y - (imgTemplate.Height / 2));
                        ptEnd = new Point(cptFoundCenter.X + (imgTemplate.Width / 2), cptFoundCenter.Y + (imgTemplate.Height / 2));
                        CRect crtFoundRect = new CRect(ptStart, ptEnd);
                        Mat matFound = m_module.GetMatImage(mem, crtFoundRect);
                        Mat matBinary = new Mat();
                        CvInvoke.Threshold(matFound, matBinary, m_nThreshold, 128, ThresholdType.Binary);
                        Image<Gray, byte> imgBinary = matBinary.ToImage<Gray, byte>();
                        CvBlobs blobs = new CvBlobs();
                        CvBlobDetector blobDetector = new CvBlobDetector();
                        blobDetector.Detect(imgBinary, blobs);
                        int nMaxArea = 0;
                        System.Drawing.Point[] ptsContour = new System.Drawing.Point[1];
                        foreach (CvBlob blob in blobs.Values)
                        {
                            if (blob.Area > nMaxArea)
                            {
                                nMaxArea = blob.Area;
                                ptsContour = blob.GetContour();
                            }
                        }
                        CRect crtBoundingBox;
                        Mat matResult = FloodFill(matBinary, ptsContour[0], 255, out crtBoundingBox, Connectivity.EightConnected);
                        matResult = matResult - matBinary;
                        if (i == (int)eSearchPoint.RT)  // Flip Horizontal
                        {
                            CvInvoke.Flip(matResult, matResult, FlipType.Horizontal);
                        }
                        else if (i == (int)eSearchPoint.RB) // Flip Horizontal & Vertical
                        {
                            CvInvoke.Flip(matResult, matResult, FlipType.Horizontal);
                            CvInvoke.Flip(matResult, matResult, FlipType.Vertical);
                        }
                        else if (i == (int)eSearchPoint.LB) // Flip Vertical
                        {
                            CvInvoke.Flip(matResult, matResult, FlipType.Vertical);
                        }
                        matarr[i] = matResult.Clone();
                    }
                }

                // Compare All Image
                for (int i = 0; i<3; i++)
                {
                    Mat matMaster = matarr[i].Clone();
                    Image<Gray, byte> imgMaster = matMaster.ToImage<Gray, byte>();
                    for (int j = i+1; j<4; j++)
                    {
                        Mat matSlave = matarr[j].Clone();
                        Image<Gray, byte> imgSlave = matSlave.ToImage<Gray, byte>();
                        CvBlobs blobs = new CvBlobs();
                        CvBlobDetector blobDetector = new CvBlobDetector();
                        blobDetector.Detect(imgSlave, blobs);
                        foreach (CvBlob blob in blobs.Values)
                        {
                            Mat matMiniTemplate = new Mat(matSlave, blob.BoundingBox);
                            Image<Gray, byte> imgMiniTemplate = matMiniTemplate.ToImage<Gray, byte>();
                            Image<Gray, float> imgMatchResult = imgMaster.MatchTemplate(imgMiniTemplate, TemplateMatchingType.CcorrNormed);
                            float[,,] matches = imgMatchResult.Data;
                            float fMaxScore = float.MinValue;
                            CPoint cptMaxRelative = new CPoint();
                            for (int x = 0; x<matches.GetLength(1); x++)
                            {
                                for (int y = 0; y<matches.GetLength(0); y++)
                                {
                                    if (fMaxScore < matches[y, x, 0] && m_dMatchScore < matches[y, x, 0])
                                    {
                                        fMaxScore = matches[y, x, 0];
                                        cptMaxRelative.X = x;
                                        cptMaxRelative.Y = y;
                                    }
                                }
                            }
                            Image<Gray, byte> imgMasterClone = imgMaster.Clone();
                            byte[,,] barrMaster = imgMasterClone.Data;
                            byte[,,] barrMiniTemplate = imgMiniTemplate.Data;
                            for (int x = 0; x<imgMiniTemplate.Width; x++)
                            {
                                for (int y = 0; y<imgMiniTemplate.Height; y++)
                                {
                                    barrMaster[y + cptMaxRelative.Y, x + cptMaxRelative.X, 0] -= barrMiniTemplate[y, x, 0];
                                }
                            }
                            Image<Gray, byte> imgSub = new Image<Gray, byte>(barrMaster);

                            // 차영상 Blob 결과
                            bool bResult = GetResultFromImage(imgSub);

                            string strName = "";
                            if (i == (int)eSearchPoint.LT) strName += eSearchPoint.LT.ToString() + "-";
                            else if (i == (int)eSearchPoint.RT) strName += eSearchPoint.RT.ToString() + "-";
                            else if (i == (int)eSearchPoint.RB) strName += eSearchPoint.RB.ToString() + "-";
                            else strName += eSearchPoint.LB.ToString() + "-";

                            if (j == (int)eSearchPoint.LT) strName += eSearchPoint.LT;
                            else if (j == (int)eSearchPoint.RT) strName += eSearchPoint.RT;
                            else if (j == (int)eSearchPoint.RB) strName += eSearchPoint.RB;
                            else strName += eSearchPoint.LB;

                            imgSub.Save("D:\\ESCHO_" + strName + ".BMP");

                            if (bResult == false)
                            {
                                m_module.p_bAlignKeyPass = false;
                                return "Fail";
                            }
                        }
                    }
                }

                return "OK";
            }

            bool GetResultFromImage(Image<Gray, byte> img)
            {
                // variable
                CvBlobs blobs = new CvBlobs();
                CvBlobDetector blobDetector = new CvBlobDetector();

                // implement
                blobDetector.Detect(img, blobs);
                foreach(CvBlob blob in blobs.Values)
                {
                    if (blob.BoundingBox.Width > m_nNGSpec_um / 5/*Resolution*/ || blob.BoundingBox.Height > m_nNGSpec_um / 5/*Resolution*/) return false;
                }

                return true;
            }

            Mat FloodFill(Mat matSrc, System.Drawing.Point ptSeed, int nPaintValue, out CRect crtBoundingBox,Connectivity connect)
            {
                // variable
                Queue<System.Drawing.Point> q = new Queue<System.Drawing.Point>();
                bool[,] barrVisited = new bool[matSrc.Height, matSrc.Width];
                int nL = matSrc.Width - 1;
                int nT = matSrc.Height - 1;
                int nR = 0;
                int nB = 0;

                // implement
                Image<Gray, byte> img = matSrc.ToImage<Gray, byte>();
                byte[,,] imgarr = img.Data;
                for (int y = 0; y<matSrc.Height; y++)
                {
                    for (int x = 0; x<matSrc.Width; x++)
                    {
                        barrVisited[y, x] = false;
                    }
                }

                // BFS 시작
                q.Enqueue(ptSeed);
                barrVisited[ptSeed.Y, ptSeed.X] = true;
                imgarr[ptSeed.Y, ptSeed.X, 0] = (byte)nPaintValue;
                if (connect == Connectivity.FourConnected)
                {
                    while (q.Count != 0)
                    {
                        System.Drawing.Point ptTemp = q.Dequeue();
                        // 상,우,하,좌
                        if (ptTemp.Y - 1 >= 0)
                        {
                            if (barrVisited[ptTemp.Y - 1, ptTemp.X] == false && imgarr[ptTemp.Y - 1, ptTemp.X, 0] != 0)
                            {
                                barrVisited[ptTemp.Y - 1, ptTemp.X] = true;
                                imgarr[ptTemp.Y - 1, ptTemp.X, 0] = (byte)nPaintValue;
                                q.Enqueue(new System.Drawing.Point(ptTemp.X, ptTemp.Y - 1));
                                if (nL > ptTemp.X) nL = ptTemp.X;
                                if (nT > ptTemp.Y) nT = ptTemp.Y;
                                if (nR < ptTemp.X) nR = ptTemp.X;
                                if (nB < ptTemp.Y) nB = ptTemp.Y;
                            }
                        }
                        if (ptTemp.X + 1 < matSrc.Width)
                        {
                            if (barrVisited[ptTemp.Y, ptTemp.X + 1] == false && imgarr[ptTemp.Y, ptTemp.X + 1, 0] != 0)
                            {
                                barrVisited[ptTemp.Y, ptTemp.X + 1] = true;
                                imgarr[ptTemp.Y, ptTemp.X + 1, 0] = (byte)nPaintValue;
                                q.Enqueue(new System.Drawing.Point(ptTemp.X + 1, ptTemp.Y));
                                if (nL > ptTemp.X) nL = ptTemp.X;
                                if (nT > ptTemp.Y) nT = ptTemp.Y;
                                if (nR < ptTemp.X) nR = ptTemp.X;
                                if (nB < ptTemp.Y) nB = ptTemp.Y;
                            }
                        }
                        if (ptTemp.Y + 1 < matSrc.Height)
                        {
                            if (barrVisited[ptTemp.Y + 1, ptTemp.X] == false && imgarr[ptTemp.Y + 1, ptTemp.X, 0] != 0)
                            {
                                barrVisited[ptTemp.Y + 1, ptTemp.X] = true;
                                imgarr[ptTemp.Y + 1, ptTemp.X, 0] = (byte)nPaintValue;
                                q.Enqueue(new System.Drawing.Point(ptTemp.X, ptTemp.Y + 1));
                                if (nL > ptTemp.X) nL = ptTemp.X;
                                if (nT > ptTemp.Y) nT = ptTemp.Y;
                                if (nR < ptTemp.X) nR = ptTemp.X;
                                if (nB < ptTemp.Y) nB = ptTemp.Y;
                            }
                        }
                        if (ptTemp.X - 1 >= 0)
                        {
                            if (barrVisited[ptTemp.Y, ptTemp.X - 1] == false && imgarr[ptTemp.Y, ptTemp.X - 1, 0] != 0)
                            {
                                barrVisited[ptTemp.Y, ptTemp.X - 1] = true;
                                imgarr[ptTemp.Y, ptTemp.X - 1, 0] = (byte)nPaintValue;
                                q.Enqueue(new System.Drawing.Point(ptTemp.X - 1, ptTemp.Y));
                                if (nL > ptTemp.X) nL = ptTemp.X;
                                if (nT > ptTemp.Y) nT = ptTemp.Y;
                                if (nR < ptTemp.X) nR = ptTemp.X;
                                if (nB < ptTemp.Y) nB = ptTemp.Y;
                            }
                        }
                    }
                }
                else
                {
                    while (q.Count != 0)
                    {
                        System.Drawing.Point ptTemp = q.Dequeue();
                        // 좌상,상,우상,우,우하,하,좌하,좌
                        for (int y = -1; y<=1; y++)
                        {
                            for (int x = -1; x<=1; x++)
                            {
                                if (ptTemp.X + x >= 0 && ptTemp.X + x < matSrc.Width && ptTemp.Y + y >= 0 && ptTemp.Y + y < matSrc.Height)
                                {
                                    if (barrVisited[ptTemp.Y + y, ptTemp.X + x] == false && imgarr[ptTemp.Y + y, ptTemp.X + x, 0] != 0)
                                    {
                                        barrVisited[ptTemp.Y + y, ptTemp.X + x] = true;
                                        imgarr[ptTemp.Y + y, ptTemp.X + x, 0] = (byte)nPaintValue;
                                        q.Enqueue(new System.Drawing.Point(ptTemp.X + x, ptTemp.Y + y));
                                        if (nL > ptTemp.X) nL = ptTemp.X;
                                        if (nT > ptTemp.Y) nT = ptTemp.Y;
                                        if (nR < ptTemp.X) nR = ptTemp.X;
                                        if (nB < ptTemp.Y) nB = ptTemp.Y;
                                    }
                                }
                            }
                        }
                    }
                }
                crtBoundingBox = new CRect(nL, nT, nR, nB);
                Image<Gray, byte> imgResult = new Image<Gray, byte>(imgarr);
                Mat matResult = imgResult.Mat;
                return matResult;
            }
        }
        #endregion

        #region Pellicle Shift & Rotation 검사
        public class Run_PellicleShiftAndRotation : ModuleRunBase
        {
            MainVision m_module;
            public int m_nLeftFrameScanLine = 0;
            public int m_nRightFrameScanLine = 1;
            public int m_nFrameheight = 5;
            
            public Run_PellicleShiftAndRotation(MainVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_PellicleShiftAndRotation run = new Run_PellicleShiftAndRotation(m_module);
                run.m_nLeftFrameScanLine = m_nLeftFrameScanLine;
                run.m_nRightFrameScanLine = m_nRightFrameScanLine;
                run.m_nFrameheight = m_nFrameheight;

                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nLeftFrameScanLine = tree.Set(m_nLeftFrameScanLine, m_nLeftFrameScanLine, "Left Frame Scan Line Number", "Left Frame Scan Line Number", bVisible);
                m_nRightFrameScanLine = tree.Set(m_nRightFrameScanLine, m_nRightFrameScanLine, "Right Frame Scan Line Number", "Right Frame Scan Line Number", bVisible);
                m_nFrameheight = tree.Set(m_nFrameheight, m_nFrameheight, "Frame Height [mm]", "Frame Height [mm]", bVisible);
            }

            public override string Run()
            {
                Run_Grab grab = (Run_Grab)m_module.CloneModuleRun("Grab");

                if (grab.m_grabMode == null) return "Grab Mode == null";

                try
                {
                    grab.m_grabMode.SetLight(true);

                    AxisXY axisXY = m_module.m_axisXY;
                    Axis axisZ = m_module.m_axisZ;
                    CPoint cptMemoryOffset = new CPoint(grab.m_cpMemoryOffset);
                    int nScanLine = 0;
                    int nMMPerUM = 1000;
                    int nCamWidth = grab.m_grabMode.m_camera.GetRoiSize().X;
                    int nCamHeight = grab.m_grabMode.m_camera.GetRoiSize().Y;

                    double dXScale = grab.m_dResX_um * 10;
                    cptMemoryOffset.X += (m_nLeftFrameScanLine + grab.m_grabMode.m_ScanStartLine) * nCamWidth;
                    grab.m_grabMode.m_dTrigger = Convert.ToInt32(10 * grab.m_dResY_um);  // 1pulse = 0.1um -> 10pulse = 1um
                    int nReticleSizeY_px = Convert.ToInt32(grab.m_nReticleSize_mm * nMMPerUM / grab.m_dResY_um);  // 레티클 영역의 Y픽셀 갯수
                    int nTotalTriggerCount = Convert.ToInt32(grab.m_grabMode.m_dTrigger * nReticleSizeY_px);   // 스캔영역 중 레티클 스캔 구간에서 발생할 Trigger 갯수
                    int nScanOffset_pulse = 100000; //가속버퍼구간

                    for (int i = 0; i<2; i++)
                    {
                        if (EQ.IsStop()) return "OK";

                        double dStartPosY = grab.m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                        double dEndPosY = grab.m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                        grab.m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

                        double dPosX = 0;
                        if (i == 0) // Left Frame Scan
                            dPosX = grab.m_rpAxisCenter.X + nReticleSizeY_px * (double)grab.m_grabMode.m_dTrigger / 2 - (m_nLeftFrameScanLine/*nScanLine*/ + grab.m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;
                        else // Right Frame Scan
                            dPosX = grab.m_rpAxisCenter.X + nReticleSizeY_px * (double)grab.m_grabMode.m_dTrigger / 2 - (m_nRightFrameScanLine/*nScanLine*/ + grab.m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

                        if (m_module.Run(axisZ.StartMove(grab.m_nFocusPosZ - (m_nFrameheight * nMMPerUM * 10))))   // 기존높이 - 프레임높이
                            return p_sInfo;
                        if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        double dTriggerStartPosY = grab.m_rpAxisCenter.Y - nTotalTriggerCount / 2;
                        double dTriggerEndPosY = grab.m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;
                        axisXY.p_axisY.SetTrigger(dTriggerStartPosY, dTriggerEndPosY, grab.m_grabMode.m_dTrigger, true);

                        string strPool = grab.m_grabMode.m_memoryPool.p_id;
                        string strGroup = grab.m_grabMode.m_memoryGroup.p_id;
                        string strMemory = grab.m_grabMode.m_memoryData.p_id;

                        MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);
                        int nScanSpeed = Convert.ToInt32((double)grab.m_nMaxFrame * grab.m_grabMode.m_dTrigger * nCamHeight * grab.m_nScanRate / 100);
                        grab.m_grabMode.StartGrab(mem, cptMemoryOffset, nReticleSizeY_px, grab.m_grabMode.m_bUseBiDirectionScan);

                        if (m_module.Run(axisXY.p_axisY.StartMove(dEndPosY, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        axisXY.p_axisY.RunTrigger(false);

                        cptMemoryOffset.X = (m_nRightFrameScanLine + grab.m_grabMode.m_ScanStartLine) * nCamWidth;
                    }
                    grab.m_grabMode.m_camera.StopGrab();
                }
                finally
                {
                    grab.m_grabMode.SetLight(false);
                }

                return "OK";
            }
        }
        #endregion
    }
}
