using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Inspects;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Media;

namespace Root_Vega.Module
{
    public enum eScanPos
    {
        Bottom = 0,
        Left,
        Top,
        Right,
    }

    public class SideVision : ModuleBase, IRobotChild
    {
        #region ToolBox
        AxisXY m_axisXY;
        public AxisXY p_axisXY
        {
            get
            {
                return m_axisXY;
            }
        }
        Axis m_axisZ;
        public Axis p_axisZ
        {
            get
            {
                return m_axisZ;
            }
        }
        Axis m_axisTheta;
        public Axis p_axisTheta
        {
            get
            {
                return m_axisTheta;
            }
        }
        public LightSet m_lightSet;
        MemoryPool m_memoryPool;
        InspectTool m_inspectTool;
        Camera_Dalsa m_CamSide;
        public Camera_Dalsa p_CamSide
        {
            get
            {
                return m_CamSide;
            }
        }
        Camera_Dalsa m_CamBevel;
        public Camera_Dalsa p_CamBevel
        {
            get
            {
                return m_CamBevel;
            }
        }
        Camera_Basler m_CamSideVRS;
        public Camera_Basler p_CamSideVRS
        {
            get
            {
                return m_CamSideVRS;
            }
        }
        Camera_Basler m_CamBevelVRS;
        public Camera_Basler p_CamBevelVRS
        {
            get
            {
                return m_CamBevelVRS;
            }
        }
        Camera_Basler m_CamAlign1;
        public Camera_Basler p_CamAlign1
        {
            get
            {
                return m_CamAlign1;
            }
        }
        Camera_Basler m_CamAlign2;
        public Camera_Basler p_CamAlign2
        {
            get
            {
                return m_CamAlign2;
            }
        }
        Camera_Basler m_CamLADS;
        public Camera_Basler p_CamLADS
        {
            get
            {
                return m_CamLADS;
            }
        }

        #region GrabMode
        int m_lGrabMode = 0;
        public ObservableCollection<GrabMode> m_aGrabMode = new ObservableCollection<GrabMode>();
        public List<string> p_asGrabMode
        {
            get
            {
                List<string> asGrabMode = new List<string>();
                foreach (GrabMode grabMode in m_aGrabMode)
                    asGrabMode.Add(grabMode.p_sName);
                return asGrabMode;
            }
        }
        public GrabMode GetGrabMode(string sGrabMode)
        {
            foreach (GrabMode grabMode in m_aGrabMode)
            {
                if (sGrabMode == grabMode.p_sName)
                    return grabMode;
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
            while (m_aGrabMode.Count > m_lGrabMode)
                m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
            foreach (GrabMode grabMode in m_aGrabMode)
                grabMode.RunTreeName(tree.GetTree("Name", false));
            foreach (GrabMode grabMode in m_aGrabMode)
                grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
        }
        #endregion

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "AxisXY");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "AxisZ");
            p_sInfo = m_toolBox.Get(ref m_axisTheta, this, "AxisTheta");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_inspectTool, this);
            p_sInfo = m_toolBox.Get(ref m_CamBevel, this, "Bevel Main");
            p_sInfo = m_toolBox.Get(ref m_CamSide, this, "Side Main");
            p_sInfo = m_toolBox.Get(ref m_CamLADS, this, "LADS");
            p_sInfo = m_toolBox.Get(ref m_CamBevelVRS, this, "Bevel VRS");
            p_sInfo = m_toolBox.Get(ref m_CamSideVRS, this, "Side VRS");
            p_sInfo = m_toolBox.Get(ref m_CamAlign1, this, "Side_Align1");
            p_sInfo = m_toolBox.Get(ref m_CamAlign2, this, "Side_Align2");
            if (bInit) m_inspectTool.OnInspectDone += M_inspectTool_OnInspectDone;
        }

        private void M_inspectTool_OnInspectDone(InspectTool.Data data)
        {
            p_sInfo = data.p_sInfo; 
        }
        #endregion

        #region DIO Function
        public bool p_bStageVac { get; set; }

        void RunTreeDIODelay(Tree tree)
        {

        }
        #endregion

        #region Axis Function
        public enum eAxisPosTheta
        {
            Ready,
            Snap,
        }
        public enum eAxisPosZ
        {
            Safety,
            Grab,
        }

        
        void InitPosAlign()
        {  
            m_axisTheta.AddPos(Enum.GetNames(typeof(eAxisPosTheta)));
            m_axisTheta.AddPosDone();
            m_axisZ.AddPos(Enum.GetNames(typeof(eAxisPosZ)));
            m_axisZ.AddPosDone();
        }

        #endregion

        #region IRobotChild
        bool _bLock = false;
        public bool p_bLock
        {
            get { return _bLock; }
            set
            {
                if (_bLock == value) return;
                _bLock = value;
            }
        }

        bool IsLock()
        {
            for (int n = 0; n < 10; n++)
            {
                if (p_bLock == false) return false;
                Thread.Sleep(100);
            }
            return true;
        }

        public string IsGetOK(ref int posRobot)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoReticle == null) return p_id + " IsGetOK - InfoReticle not Exist";
            posRobot = m_nTeachRobot;
            return "OK";
        }

        public string IsPutOK(ref int posRobot, InfoReticle infoReticle)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoReticle == null) return p_id + " IsPutOK - InfoReticle Exist";
            posRobot = m_nTeachRobot;
            return "OK";
        }

        public string BeforeGet()
        {
            if (p_infoReticle == null) return p_id + " BeforeGet : InfoReticle = null";
            return CheckGetPut();
        }

        public string BeforePut()
        {
            if (p_infoReticle != null) return p_id + " BeforePut : InfoReticle != null";
            return CheckGetPut();
        }

        public string AfterGet()
        {
            return CheckGetPut();
        }

        public string AfterPut()
        {
            return CheckGetPut();
        }

        string CheckGetPut()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            //LoadPos ??
            return "OK";
        }

        public bool IsReticleExist(bool bIgnoreExistSensor = false)
        {
            bool bExist = false;
            if (bIgnoreExistSensor) bExist = (p_infoReticle != null);
            else
            {
                //forget
            }
            p_brushReticleExist = bExist ? Brushes.Yellow : Brushes.Green;
            return bExist;
        }

        Brush _brushReticleExist = Brushes.Green;
        public Brush p_brushReticleExist
        {
            get { return _brushReticleExist; }
            set
            {
                if (_brushReticleExist == value) return;
                _brushReticleExist = value;
                OnPropertyChanged();
            }
        }

        int m_nTeachRobot = 0;
        public void RunTeachTree(Tree tree)
        {
            m_nTeachRobot = tree.Set(m_nTeachRobot, m_nTeachRobot, p_id, "Robot Teach Index");
        }
        #endregion 

        #region InfoReticle
        string m_sInfoReticle = "";
        InfoReticle _infoReticle = null;
        public InfoReticle p_infoReticle
        {
            get { return _infoReticle; }
            set
            {
                m_sInfoReticle = (value == null) ? "" : value.p_id;
                _infoReticle = value;
                if (m_reg != null) m_reg.Write("sInfoReticle", m_sInfoReticle);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoReticle_Registry()
        {
            m_reg = new Registry(p_id + ".InfoReticle");
            m_sInfoReticle = m_reg.Read("sInfoReticle", m_sInfoReticle);
            p_infoReticle = m_engineer.ClassHandler().GetGemSlot(m_sInfoReticle);
        }
        #endregion

        #region Inspect
        void InitInspect()
        {
            InitMemory();
            InitThreadInspect(); 
        }

        int m_lMaxGrab = 3000; 
        CPoint m_szAlignROI = new CPoint();
        MemoryData m_memoryTop;
        MemoryData m_memoryLeft;
        MemoryData m_memoryRight;
        MemoryData m_memoryBottom;
        public ushort[] m_aHeight;
        double m_fScaleH = 0; 
        void InitMemory()
        {
            m_szAlignROI = p_CamLADS.p_szROI;
            //m_memoryGrab = m_memoryPool.GetGroup(p_id).CreateMemory("Grab", m_lMaxGrab, 1, m_szAlignROI);
            //m_memoryHeight = m_memoryPool.GetGroup(p_id).CreateMemory("Height", 1, 1, m_szAlignROI.X, m_lMaxGrab);
            //m_memoryBright = m_memoryPool.GetGroup(p_id).CreateMemory("Bright", 1, 1, m_szAlignROI.X, m_lMaxGrab);            
            m_memoryTop = m_memoryPool.GetGroup(p_id).CreateMemory("Top", 1, 1, m_szAlignROI.X, m_lMaxGrab);
            m_memoryLeft = m_memoryPool.GetGroup(p_id).CreateMemory("Left", 1, 1, m_szAlignROI.X, m_lMaxGrab);
            m_memoryRight = m_memoryPool.GetGroup(p_id).CreateMemory("Right", 1, 1, m_szAlignROI.X, m_lMaxGrab);
            m_memoryBottom = m_memoryPool.GetGroup(p_id).CreateMemory("Bottom", 1, 1, m_szAlignROI.X, m_lMaxGrab);
            m_aHeight = new ushort[m_szAlignROI.X * m_lMaxGrab];
            m_fScaleH = 65535.0 / m_szAlignROI.Y; 
        }

        bool m_bThreadInspect3D = false; 
        Thread m_threadInspect3D; 
        void InitThreadInspect()
        {
            m_threadInspect3D = new Thread(new ThreadStart(RunThreadInspect3D));
            m_threadInspect3D.Start();
        }

        Queue<int> m_qInspect = new Queue<int>(); 
        void RunThreadInspect3D()
        {
            m_bThreadInspect3D = true;
            Thread.Sleep(3000); 
            while (m_bThreadInspect3D)
            {
                Thread.Sleep(10); 
                while (m_qInspect.Count > 0)
                {
                    try { RunThreadInspect(m_qInspect.Dequeue()); }
                    catch (Exception) { m_qInspect.Clear(); }
                }
            }
        }

        unsafe void RunThreadInspect(int iInspect)
        {
            System.Windows.MessageBox.Show(" unsafe void RunThreadInspect(int iInspect) 주석처리되어있는 부분 확인바람.", "처리되지않음.", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);

            //byte* pSrc = (byte*)m_memoryGrab.GetPtr(iInspect).ToPointer();
            //byte* pHeight = (byte*)m_memoryHeight.GetPtr(0, 0, iInspect).ToPointer();
            //byte* pBright = (byte*)m_memoryBright.GetPtr(0, 0, iInspect).ToPointer();
            //for (int x = 0; x < m_szAlignROI.X; x++, pSrc++, pHeight++, pBright++)
            //{
            //    byte* pSrcY = pSrc;
            //    int nSum = 0;
            //    int nYSum = 0;
            //    for (int y = 0; y < m_szAlignROI.Y; y++, pSrcY += m_szAlignROI.X)
            //    {
            //        nSum += *pSrcY;
            //        nYSum = *pSrcY * y;
            //    }
            //    int nAdd = x + iInspect * m_szAlignROI.X;
            //    m_aHeight[nAdd] = (nSum != 0) ? (ushort)(m_fScaleH * nYSum / nSum) : (ushort)0;
            //    *pHeight = (byte)(m_aHeight[nAdd] >> 8);
            //    int yAve = (nSum != 0) ? (int)Math.Round(1.0 * nYSum / nSum) : 0;
            //    *pBright = pSrc[x + yAve * m_szAlignROI.X];
            //}
        }

        void StartInspect(int iInspect)
        {
            m_qInspect.Enqueue(iInspect); 
        }


        void RunTreeInspect(Tree tree)
        {
            m_lMaxGrab = tree.Set(m_lMaxGrab, m_lMaxGrab, "Max Grab", "Max Grab Count for Memory Allocate");
        }
        #endregion

        #region override Function
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            p_bStageVac = true;
            Thread.Sleep(200);

            m_axisXY.p_axisX.HomeStart();
            m_axisXY.p_axisY.HomeStart();
            m_axisZ.p_axis.HomeStart();

            if (m_axisXY.WaitReady() != "OK")
                return "Error";
            if (m_axisZ.WaitReady() != "OK")
                return "Error";

            m_axisTheta.p_axis.HomeStart();
            if (m_axisTheta.WaitReady() != "OK")
                return "Error";

            //p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            p_bStageVac = false;
            return p_sInfo;
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTreeDIODelay(tree.GetTree("DIO Delay", false));
            RunTreeGrabMode(tree.GetTree("Grab Mode", false));
            RunTreeInspect(tree.GetTree("Inspect", false)); 
        }
        #endregion

        public SideVision(string id, IEngineer engineer, string sLogGroup = "")
        {
            base.InitBase(id, engineer, sLogGroup);
            InitInspect();
            InitPosAlign();
        }

        public override void ThreadStop()
        {
            if (m_bThreadInspect3D)
            {
                m_qInspect.Clear(); 
                m_bThreadInspect3D = false;
                m_threadInspect3D.Join(); 
            }
            base.ThreadStop();
        }
         
        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
//            AddModuleRunList(new Run_Inspect(this), true, "3D Inspect");
            AddModuleRunList(new Run_Run(this), true, "Run Side Vision");
            AddModuleRunList(new Run_SideGrab(this), true, "Side Grab");
        }

        public class Run_Delay : ModuleRunBase
        {
            SideVision m_module;
            public Run_Delay(SideVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secDelay = 2;
            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }
            public override string Run()
            {
                Thread.Sleep((int)(1000 * m_secDelay));
                m_module.m_gem.STSSetProcessing(m_module.p_infoReticle, RootTools.Gem.GemSlotBase.eSTSProcess.Processed); 
                return "OK";
            }
        }

        public class Run_Run : ModuleRunBase
        {
            SideVision m_module;
            public Run_Run(SideVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secDelay = 2;
            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }
            public override string Run()
            {
                if (EQ.p_bSimulate) Thread.Sleep(100);
                else
                {
                    //forget SideVision ModuleRun_Run
                }
                m_module.m_gem.STSSetProcessing(m_module.p_infoReticle, RootTools.Gem.GemSlotBase.eSTSProcess.Processed);
                return "OK";
            }
        }

        public class Run_SideGrab : ModuleRunBase
        {
            SideVision m_module;
            public Run_SideGrab(SideVision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public GrabMode m_grabMode = null;
            string _sGrabMode = "";
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

            public RPoint m_rpAxis = new RPoint();
            public double m_fRes = 1;       //단위 um
            public int m_nFocusPos = 0;
            public CPoint m_cpMemory = new CPoint();
            public int  m_nScanGap = 1000;
            public int m_yLine = 1000;  // Y축 Reticle Size
            public int m_xLine = 1000;  // X축 Reticle Size
            public int m_nMaxFrame = 100;  // Camera max Frame 스펙
            public int m_nScanRate = 100;   // Camera Frame Spec 사용률 ? 1~100 %
          
            public eScanPos m_eScanPos = eScanPos.Bottom;
            public override ModuleRunBase Clone()
            {
                Run_SideGrab run = new Run_SideGrab(m_module);
                run.p_sGrabMode = p_sGrabMode;
                run.m_fRes = m_fRes;
                run.m_nFocusPos = m_nFocusPos;
                run.m_rpAxis = new RPoint(m_rpAxis);
                run.m_cpMemory = new CPoint(m_cpMemory);
                run.m_yLine = m_yLine;
                run.m_nMaxFrame = m_nMaxFrame;
                run.m_nScanRate = m_nScanRate;
                run.m_eScanPos = m_eScanPos;
                run.m_nScanGap = m_nScanGap;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpAxis = tree.Set(m_rpAxis, m_rpAxis, "Center Axis Position", "Center Axis Position (mm ?)", bVisible);
                m_fRes = tree.Set(m_fRes, m_fRes, "Cam Resolution", "Resolution  um", bVisible);
                m_nFocusPos = tree.Set(m_nFocusPos, 0, "Focus Z Pos", "Focus Z Pos", bVisible);
                m_cpMemory = tree.Set(m_cpMemory, m_cpMemory, "Memory Position", "Grab Start Memory Position (pixel)", bVisible);
                m_nScanGap = tree.Set(m_nScanGap, m_nScanGap, "Scan Gab", "Scan 방향간의 Memory 상 Gab (Bottom, Left 간의 Memory 위치 차이)", bVisible);
                m_yLine = tree.Set(m_yLine, m_yLine, "Reticle YSize", "# of Grab Lines", bVisible);
                m_xLine = tree.Set(m_xLine, m_xLine, "Reticle XSize", "# of Grab Lines", bVisible);
                m_eScanPos = (eScanPos)tree.Set(m_eScanPos, m_eScanPos, "Scan 위치", "Scan 위치, 0 Position 이 Bottom", bVisible);
                m_nMaxFrame = (tree.GetTree("Scan Velocity", false)).Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
                m_nScanRate = (tree.GetTree("Scan Velocity",false)).Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100 %", bVisible);
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
                if (m_grabMode != null)
                    m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true);
            }

            public override string Run()
            {
                if (m_grabMode == null)
                    return "Grab Mode == null";

                AxisXY axisXY = m_module.p_axisXY;
                Axis axisZ = m_module.p_axisZ;
                Axis axisTheta = m_module.p_axisTheta;

                try
                {
                    int nScanLine = 0;
                    m_grabMode.SetLight(true);
                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_fRes);        // 축해상도 0.1um로 하드코딩. 트리거 발생 주기.
                    m_cpMemory.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X + (int)m_eScanPos * (m_xLine + m_nScanGap);
                    int nLinesY = Convert.ToInt32(m_yLine * 1000 / m_fRes);      // Grab 할 총 Line 갯수.
                    int nLinesX = Convert.ToInt32(m_xLine * 1000 / m_fRes);      // Grab 할 총 Line 갯수.

                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";

                        double yAxis = m_grabMode.m_dTrigger * nLinesY;     // 총 획득할 Image Y  
                        /*왼쪽에서 오른쪽으로 찍는것을 정방향으로 함, 즉 Y 축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향*/
                        /* Grab하기 위해 이동할 Y축의 시작 끝 점*/
                        double yPos1 = m_rpAxis.Y - yAxis / 2 - m_grabMode.m_intervalAcc;   //y 축 이동 시작 지점 
                        double yPos0 = m_rpAxis.Y + yAxis / 2 + m_grabMode.m_intervalAcc;  // Y 축 이동 끝 지점.
                        double nPosX = m_rpAxis.X;   // X축 찍을 위치 
                        double nPosZ = m_nFocusPos + nLinesX * m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * m_grabMode.m_dTrigger; //해상도추가필요
                        //double nPosX = m_rpAxis.X + nLines * m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * m_grabMode.m_dTrigger; //해상도추가필요
                        int nPosTheta = axisTheta.GetPos(SideVision.eAxisPosTheta.Snap) + (int)m_eScanPos * 40000 / 4;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;

                        if (m_module.Run(axisXY.p_axisX.Move(0)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisTheta.Move(nPosTheta)))
                            return p_sInfo;
                        if (m_module.Run(axisTheta.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.Move(nPosZ)))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisXY.Move(new RPoint(nPosX, yPos0))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;

                        /* Trigger Set*/
                        double yTrigger0 = m_rpAxis.Y - yAxis / 2;
                        double yTrigger1 = m_rpAxis.Y + yAxis / 2;
                        m_module.p_axisXY.p_axisY.SetTrigger(yPos1, yTrigger1, m_grabMode.m_dTrigger, true);

                        /* 메모리 위치도 가져오게는 좀 다시 하자.*/
                        string sPool = "pool";
                        string sGroup = "group";
                        string sMem = "mem";
                        MemoryData mem = m_module.m_engineer.ClassMemoryTool().GetPool(sPool).GetGroup(sGroup).GetMemory(sMem);

                        int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * m_grabMode.m_dTrigger * m_grabMode.m_camera.GetRoiSize().Y * (double)m_nScanRate / 100);
                        /* 방향 바꾸는 코드 들어가야함*/
                        m_grabMode.StartGrab(mem, m_cpMemory, nLinesY);
                        if (m_module.Run(axisXY.p_axisY.Move(yPos1, nScanSpeed)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        axisXY.p_axisY.ResetTrigger();

                        nScanLine++;
                        m_cpMemory.X += m_grabMode.m_camera.GetRoiSize().X;
                    }
                    return "OK";
                }
                finally
                {
                    axisXY.p_axisY.ResetTrigger();
                    m_grabMode.SetLight(false);
                }
            }
        }
        #endregion
    }
}
