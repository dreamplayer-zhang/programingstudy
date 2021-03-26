using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Root_EFEM;
using Root_EFEM.Module;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Control.Ajin;
using RootTools.GAFs;
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
    public class BacksideVision : ModuleBase, IWTRChild
    {
        MainVision m_mainVision;

        #region ToolBox
        Axis m_axisRotate;
        Axis m_axisZ;
        Axis m_axisSideZ;
        AxisXY m_axisXY;
        public DIO_I m_diExistVision;
        public DIO_I m_diReticleTiltCheck;
        public DIO_I m_diReticleFrameCheck;
        MemoryPool m_memoryPool;
        //MemoryGroup m_memoryGroup;
        //MemoryData m_memoryMain;
        //MemoryData m_memorySideLeft;
        //MemoryData m_memorySideRight;
        //MemoryData m_memorySideTop;
        //MemoryData m_memorySideBottom;
        //MemoryData m_memoryTDI45;
        //MemoryData m_memoryLADS;

        LightSet m_lightSet;
        Camera_Dalsa m_CamTDI90;
        Camera_Dalsa m_CamTDI45;
        Camera_Dalsa m_CamTDISide;
        Camera_Basler m_CamLADS;

        ALID m_alid_WaferExist;
        public void SetAlarm()
        {
            m_alid_WaferExist.Run(true, "BacksideVision Wafer Exist Error");
        }

        public override void GetTools(bool bInit)
        {
            //p_sInfo = m_toolBox.Get(ref m_diExistVision, this, "Reticle Exist on Vision");
            //p_sInfo = m_toolBox.Get(ref m_diReticleTiltCheck, this, "Reticle Tilt Check");
            //p_sInfo = m_toolBox.Get(ref m_diReticleFrameCheck, this, "Reticle Frame Check");
            //p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Axis Rotate");
            //p_sInfo = m_toolBox.Get(ref m_axisSideZ, this, "Axis Side Z");
            //p_sInfo = m_toolBox.Get(ref m_axisZ, this, "Axis Z");
            //p_sInfo = m_toolBox.Get(ref m_axisXY, this, "Axis XY");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Vision Memory", 1);
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.GetCamera(ref m_CamTDI90, this, "TDI 90");
            p_sInfo = m_toolBox.GetCamera(ref m_CamTDI45, this, "TDI 45");
            p_sInfo = m_toolBox.GetCamera(ref m_CamTDISide, this, "TDI Side");
            p_sInfo = m_toolBox.GetCamera(ref m_CamLADS, this, "LADS");
            m_alid_WaferExist = m_gaf.GetALID(this, "BacksideVision Wafer Exist", "BacksideVision Wafer Exist");
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

        #region Axis Position
        public enum eAxisPos
        {
            ReadyPos,
            ScanPos,
        }

        void InitPosAlign()
        {
            //m_axisZ.AddPos(Enum.GetNames(typeof(eAxisPos)));
            //m_axisRotate.AddPos(Enum.GetNames(typeof(eAxisPos)));
            //m_axisSideZ.AddPos(Enum.GetNames(typeof(eAxisPos)));
            //m_axisXY.AddPos(Enum.GetNames(typeof(eAxisPos)));
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
            //BacksideVision.Main.
            //m_memoryGroup = m_memoryPool.GetGroup(p_id);
            //m_memoryMain = m_memoryGroup.CreateMemory(App.mMainMem, 1, 1, 1000, 1000);

            //m_memorySideLeft = m_memoryGroup.CreateMemory(App.mSideLeftMem, 1, 1, 1000, 1000);
            //m_memorySideBottom = m_memoryGroup.CreateMemory(App.mSideBotMem, 1, 1, 1000, 1000);
            //m_memorySideRight = m_memoryGroup.CreateMemory(App.mSideRightMem, 1, 1, 1000, 1000);
            //m_memorySideTop = m_memoryGroup.CreateMemory(App.mSideTopMem, 1, 1, 1000, 1000);

            //m_memoryTDI45 = m_memoryGroup.CreateMemory("TDI45", 1, 1, 1000, 1000);
            //m_memoryLADS = m_memoryGroup.CreateMemory("LADS", 1, 1, 1000, 1000);
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
                return "OK";
            Thread.Sleep(200);
            if (base.p_eState == eState.Run) return "Invalid State : Run";
            if (EQ.IsStop()) return "Home Stop";
            p_eState = eState.Ready;
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

        #region Public Variable
        public int[] m_narrSideEdgeOffset = new int[4];
        public double m_dThetaAlignOffset = 0;
        #endregion

        #region Vision Algorithm

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

        public enum eSearchDirection
        {
            TopToBottom = 0,
            LeftToRight,
            RightToLeft,
            BottomToTop,
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
        #endregion

        #region ModuleRun

        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_TestGlass(this), true, "Run Delay");
            AddModuleRunList(new Run_GrabSideScan(this), true, "Run Side Scan");
            AddModuleRunList(new Run_GrabBacksideScan(this), true, "Run Backside Scan");            
        }

        #region Run_Test
        public class Run_TestGlass : ModuleRunBase
        {
            BacksideVision m_module;
            public RPoint m_rpAxisCenter = new RPoint();    // Side Center Position
            public Run_TestGlass(BacksideVision module)
            {
                m_module = module;
                InitModuleRun(module);

            }
            public override ModuleRunBase Clone()
            {
                Run_TestGlass run = new Run_TestGlass(m_module);
                run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
                return run;
            }
            string m_sFlip = "Test";
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpAxisCenter = tree.Set(m_rpAxisCenter, m_rpAxisCenter, "Center Axis Position", "Center Axis Position (mm)", bVisible);
                m_sFlip = tree.Set(m_sFlip, m_sFlip, "Test", "Bottom", bVisible, true);
            }
            public override string Run()
            {
                Thread.Sleep(1000);
                AxisXY axisXY = m_module.m_axisXY;
                double dStartPosY = m_rpAxisCenter.Y;



                double dPosX = m_rpAxisCenter.X;


                if (m_module.Run(axisXY.StartMove(new RPoint(dPosX, dStartPosY))))
                    return p_sInfo;
                Thread.Sleep(10000);
                //m_module.p_eState = eState.Ready;
                return "OK";
            }
        }
        #endregion
        #region Run_GragSideScan
        public class Run_GrabSideScan : ModuleRunBase
        {
            BacksideVision m_module;

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

            public int m_nLeftOffsetX = 0;
            public int m_nTopOffsetX = 0;
            public int m_nRightOffsetX = 0;
            public int m_nBottomOffsetX = 0;

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
                    m_grabMode = m_module.m_mainVision.GetGrabMode(value);
                }
            }
            public Run_GrabSideScan(BacksideVision module)
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

                run.m_nLeftOffsetX = m_nLeftOffsetX;
                run.m_nTopOffsetX = m_nTopOffsetX;
                run.m_nRightOffsetX = m_nRightOffsetX;
                run.m_nBottomOffsetX = m_nBottomOffsetX;

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
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.m_mainVision.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);

                m_nLeftOffsetX = (tree.GetTree("Scan Offset", false, bVisible)).Set(m_nLeftOffsetX, m_nLeftOffsetX, "Left Offset X", "Left Offset X", bVisible);
                m_nTopOffsetX = (tree.GetTree("Scan Offset", false, bVisible)).Set(m_nTopOffsetX, m_nTopOffsetX, "Top Offset X", "Top Offset X", bVisible);
                m_nRightOffsetX = (tree.GetTree("Scan Offset", false, bVisible)).Set(m_nRightOffsetX, m_nRightOffsetX, "Right Offset X", "Right Offset X", bVisible);
                m_nBottomOffsetX = (tree.GetTree("Scan Offset", false, bVisible)).Set(m_nBottomOffsetX, m_nBottomOffsetX, "Bottom Offset X", "Bottom Offset X", bVisible);
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
                    int nDirection = 1;
                    while (nDirection > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";
                        double nRotate = m_nRotatePulse * (p_dDegree * nScanLine) + m_module.m_dThetaAlignOffset - 2500;
                        if (m_module.Run(axisRotate.StartMove(nRotate)))
                            return p_sInfo;
                        if (m_module.Run(axisRotate.WaitReady()))
                            return p_sInfo;

                        double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                        double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

                        double dPosX = m_rpAxisCenter.X;// + (m_module.m_narrSideEdgeOffset[nScanLine] * 5);

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
                        m_grabMode.StartGrab(mem, cpMemoryOffset, nReticleSizeY_px, 0, m_grabMode.m_bUseBiDirectionScan);

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
        #endregion
        #region Run_GrabBacksideScan
        public class Run_GrabBacksideScan : ModuleRunBase
        {
            BacksideVision m_module;

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
                    m_grabMode = m_module.m_mainVision.GetGrabMode(value);
                }
            }

            public CPoint m_cpLeftEdgeCenterPos = new CPoint();
            public CPoint m_cpTopEdgeCenterPos = new CPoint();
            public CPoint m_cpRightEdgeCenterPos = new CPoint();
            public CPoint m_cpBottomEdgeCenterPos = new CPoint();
            public int m_nSearchArea = 1000;
            public int m_nEdgeThreshold = 70;

            public Run_GrabBacksideScan(BacksideVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_GrabBacksideScan run = new Run_GrabBacksideScan(m_module);
                run.m_rpAxisCenter = new RPoint(m_rpAxisCenter);
                run.m_cpMemoryOffset = new CPoint(m_cpMemoryOffset);
                run.m_dResX_um = m_dResX_um;
                run.m_dResY_um = m_dResY_um;
                run.m_nFocusPosZ = m_nFocusPosZ;
                run.m_nReticleSize_mm = m_nReticleSize_mm;
                run.m_nMaxFrame = m_nMaxFrame;
                run.m_nScanRate = m_nScanRate;
                run.p_sGrabMode = p_sGrabMode;

                run.m_cpLeftEdgeCenterPos = m_cpLeftEdgeCenterPos;
                run.m_cpTopEdgeCenterPos = m_cpTopEdgeCenterPos;
                run.m_cpRightEdgeCenterPos = m_cpRightEdgeCenterPos;
                run.m_cpBottomEdgeCenterPos = m_cpBottomEdgeCenterPos;
                run.m_nSearchArea = m_nSearchArea;
                run.m_nEdgeThreshold = m_nEdgeThreshold;

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
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.m_mainVision.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);

                m_cpLeftEdgeCenterPos = (tree.GetTree("Edge Position", false, bVisible)).Set(m_cpLeftEdgeCenterPos, m_cpLeftEdgeCenterPos, "Left Edge Position", "Left Edge Position", bVisible);
                m_cpTopEdgeCenterPos = (tree.GetTree("Edge Position", false, bVisible)).Set(m_cpTopEdgeCenterPos, m_cpTopEdgeCenterPos, "Top Edge Position", "Top Edge Position", bVisible);
                m_cpRightEdgeCenterPos = (tree.GetTree("Edge Position", false, bVisible)).Set(m_cpRightEdgeCenterPos, m_cpRightEdgeCenterPos, "Right Edge Position", "Right Edge Position", bVisible);
                m_cpBottomEdgeCenterPos = (tree.GetTree("Edge Position", false, bVisible)).Set(m_cpBottomEdgeCenterPos, m_cpBottomEdgeCenterPos, "Bottom Edge Position", "Bottom Edge Position", bVisible);
                m_nSearchArea = (tree.GetTree("Edge Position", false, bVisible)).Set(m_nSearchArea, m_nSearchArea, "Search Area Size", "Search Area Size", bVisible);
                m_nEdgeThreshold = (tree.GetTree("Edge Position", false, bVisible)).Set(m_nEdgeThreshold, m_nEdgeThreshold, "Edge Threshold", "Edge Threshold", bVisible);
            }

            public override string Run()
            {
                if (m_grabMode == null) return "Grab Mode == null";

                try
                {
                    m_grabMode.SetLight(true);

                    //((Camera_Dalsa)m_grabMode.m_camera).p_CamParam.p_eDir = DalsaParameterSet.eDir.Reverse;
                    //((Camera_Dalsa)m_grabMode.m_camera).p_CamParam.p_eTriggerMode = DalsaParameterSet.eTriggerMode.External;

                    AxisXY axisXY = m_module.m_axisXY;
                    Axis axisZ = m_module.m_axisZ;
                    Axis axisRotate = m_module.m_axisRotate;
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
                    int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * nCamHeight * m_nScanRate / 100);
                    int nScanOffset_pulse = Convert.ToInt32(nScanSpeed * 0.3); //가속버퍼구간

                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        double dStartPosY = m_rpAxisCenter.Y - nTotalTriggerCount / 2 - nScanOffset_pulse;
                        double dEndPosY = m_rpAxisCenter.Y + nTotalTriggerCount / 2 + nScanOffset_pulse;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;


                        double dPosX = m_rpAxisCenter.X + nReticleSizeY_px * (double)m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * nCamWidth * dXScale;

                        // Theta축 0으로
                        double dTheta = axisRotate.GetPosValue(eAxisPos.ScanPos.ToString());
                        dTheta += m_module.m_dThetaAlignOffset;
                        if (m_module.Run(axisRotate.StartMove(dTheta)))
                            return p_sInfo;
                        if (m_module.Run(axisRotate.WaitReady()))
                            return p_sInfo;
                        //
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

                        m_grabMode.StartGrab(mem, cpMemoryOffset, nReticleSizeY_px, 0, m_grabMode.m_bUseBiDirectionScan);

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
        #endregion

        #endregion


        public BacksideVision(string id, IEngineer engineer, MainVision mainvision)
        {
            m_mainVision = mainvision;
            base.InitBase(id, engineer);
            m_waferSize = new InfoWafer.WaferSize(id, false, false);

            m_diExistVision = mainvision.m_diExistVision;
            m_diReticleTiltCheck = mainvision.m_diReticleTiltCheck;
            m_diReticleFrameCheck = mainvision.m_diReticleFrameCheck;
            m_axisRotate = mainvision.m_axisRotate;
            m_axisSideZ = mainvision.m_axisSideZ;
            m_axisZ = mainvision.m_axisZ;
            m_axisXY = mainvision.m_axisXY;
            m_memoryPool = mainvision.m_memoryPool;
            m_lightSet = mainvision.m_lightSet;
            m_CamTDI90 = mainvision.m_CamTDI90;
            m_CamTDI45 = mainvision.m_CamTDI45;
            m_CamTDISide = mainvision.m_CamTDISide;
            m_CamLADS = mainvision.m_CamLADS;

            //InitMemorys();
            //InitPosAlign();
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}

