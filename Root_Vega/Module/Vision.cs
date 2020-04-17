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
using System.Threading;

namespace Root_Vega.Module
{
    public class Vision : ModuleBase, IRobotChild
    {
        #region ToolBox
        AxisXY m_axisXY;
        Axis m_axisZ;
        Axis m_axisLifter;
        Axis m_axisRotate;
        DIO_IO[] m_dioVacuum = new DIO_IO[2];
        DIO_O[] m_doBlow = new DIO_O[2];
        DIO_I m_diRingFrame;
        DIO_I m_diWaferHome;
        DIO_I m_diWaferLoad;
        DIO_I[] m_diLoadPos = new DIO_I[2];
        DIO_O m_doLiftVacuum;
        DIO_O m_doLiftBlow;
        LightSet m_lightSet; 
        MemoryPool m_memoryPool;
        InspectTool m_inspectTool;
        public Camera_Dalsa m_camDalsa;
        Camera_Basler m_camVRS; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "StageXY");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "StageZ");
            p_sInfo = m_toolBox.Get(ref m_axisLifter, this, "Lifter");
            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "StageRotate");
            p_sInfo = m_toolBox.Get(ref m_dioVacuum[0], this, "VacuumA");
            p_sInfo = m_toolBox.Get(ref m_dioVacuum[1], this, "VacuumB");
            p_sInfo = m_toolBox.Get(ref m_doBlow[0], this, "BlowA");
            p_sInfo = m_toolBox.Get(ref m_doBlow[1], this, "BlowB");
            p_sInfo = m_toolBox.Get(ref m_diRingFrame, this, "RingFrame");
            p_sInfo = m_toolBox.Get(ref m_diWaferHome, this, "CheckWafer at Home");
            p_sInfo = m_toolBox.Get(ref m_diWaferLoad, this, "CheckWafer at Load");
            p_sInfo = m_toolBox.Get(ref m_diLoadPos[0], this, "CheckLoadPos X");
            p_sInfo = m_toolBox.Get(ref m_diLoadPos[1], this, "CheckLoadPos Y");
            p_sInfo = m_toolBox.Get(ref m_doLiftVacuum, this, "Lifter Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doLiftBlow, this, "Lifter Vlow");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_camDalsa, this, "2D");
            p_sInfo = m_toolBox.Get(ref m_camVRS, this, "VRS");
            p_sInfo = m_toolBox.Get(ref m_inspectTool, this);
            if (bInit) m_inspectTool.OnInspectDone += M_inspectTool_OnInspectDone;
        }

        private void M_inspectTool_OnInspectDone(InspectTool.Data data)
        {
            p_sInfo = data.p_sInfo; //forget
        }
        #endregion

        #region DIO Fuction
        int m_msStateVacOn = 100; 
        int m_msStateBlow = 100;
        public bool p_bStageVac
        {
            get { return m_dioVacuum[1].p_bIn && m_dioVacuum[1].p_bIn; }
            set
            {
                if (value)
                {
                    m_dioVacuum[0].Write(true);
                    Thread.Sleep(m_msStateVacOn);
                    m_dioVacuum[1].Write(true); 
                }
                else
                {
                    m_dioVacuum[0].Write(false);
                    m_dioVacuum[1].Write(false);
                    m_doBlow[0].Write(true);
                    m_doBlow[1].Write(true);
                    Thread.Sleep(m_msStateBlow);
                    m_doBlow[0].Write(false);
                    m_doBlow[1].Write(false);
                }
            }
        }

        int m_msLiftBlow = 100; 
        public bool p_bLiftVac
        {
            get { return m_doLiftVacuum.p_bOut; }
            set
            {
                if (m_doLiftVacuum.p_bOut == value) return;
                m_doLiftVacuum.Write(value);
                if (value == false)
                {
                    m_doLiftBlow.Write(true);
                    Thread.Sleep(m_msLiftBlow);
                    m_doLiftBlow.Write(false);
                }
            }
        }

        public bool p_bLoadPos
        {
            get { return m_diLoadPos[0].p_bIn && m_diLoadPos[1].p_bIn; }
        }

        void RunTreeDIODelay(Tree tree)
        {
            m_msStateVacOn = tree.Set(m_msStateVacOn, m_msStateVacOn, "Stage Vacuum On", "Stage Vacuum On Delay (ms)");
            m_msStateBlow = tree.Set(m_msStateBlow, m_msStateBlow, "State Blow", "Stage Blow Delay (ms)");
            m_msLiftBlow = tree.Set(m_msLiftBlow, m_msLiftBlow, "Lifter Blow", "Lifter Blow Delay (ms)"); 
        }
        #endregion

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

        #region InfoWafer
        string m_sInfoWafer = "";
        InfoReticle _infoReticle = null;
        public InfoReticle p_infoReticle
        {
            get { return _infoReticle; }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoReticle = value;
                if (m_reg != null) m_reg.Write("sInfoWafer", m_sInfoWafer);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoReticle_Registry()
        {
            m_reg = new Registry(p_id + ".InfoReticle");
            m_sInfoWafer = m_reg.Read("sInfoReticle", m_sInfoWafer);
            p_infoReticle = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
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
            //forget
            return (p_infoReticle != null);
        }

        int m_nTeachRobot = 0;
        public void RunTeachTree(Tree tree)
        {
            m_nTeachRobot = tree.Set(m_nTeachRobot, m_nTeachRobot, p_id, "Robot Teach Index");
        }
        #endregion 


        public enum eWaferSize
        {
            e300mm,
            e300mmRF,
            e200mm,
            e4inch,
            e5inch,
            e6inch,
            e8inch,
            eError
        }

        //eWaferSize m_waferSize = eWaferSize.e300mm;

        #region override
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            p_bStageVac = true;
            Thread.Sleep(200);
            p_sInfo = base.StateHome();
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
            //m_waferSize.RunTree(tree.GetTree("Wafer Size", false), true);
        }
        #endregion

        public Vision(string id, IEngineer engineer, string sLogGroup = "")
        {
            //m_waferSize = new WaferSize(id, false, false); 
            base.InitBase(id, engineer, sLogGroup);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            AddModuleRunList(new Run_Inspect(this), false, "Vision Inspect");
            AddModuleRunList(new Run_Grab(this), false, "Run Vision Grab");
        }

        public class Run_Delay : ModuleRunBase
        {
            Vision m_module;
            public Run_Delay(Vision module)
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
                return "OK";
            }
        }

        public class Run_Inspect : ModuleRunBase
        {
            Vision m_module;
            public Run_Inspect(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public string m_sRecipe = "Recipe";
            public override ModuleRunBase Clone()
            {
                Run_Inspect run = new Run_Inspect(m_module);
                run.m_sRecipe = m_sRecipe;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sRecipe = tree.Set(m_sRecipe, m_sRecipe, "Recipe", "Inspect Recipe", bVisible);
            }
            public override string Run()
            {
                int iIndex = 0; 
                return m_module.m_inspectTool.AddInspect(m_sRecipe, out iIndex); 
            }
        }

        public class Run_Grab : ModuleRunBase
        {
            Vision m_module;
            public Run_Grab(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public GrabMode m_grabMode = null; 
            string _sGrabMode = "";
            string p_sGrabMode
            {
                get { return _sGrabMode; }
                set
                {
                    _sGrabMode = value;
                    m_grabMode = m_module.GetGrabMode(value); 
                }
            }

            bool m_bInvDir = false;
            public RPoint m_rpAxis = new RPoint();
            public double m_fRes = 1;
            public int m_nFocusPos = 0;
            public CPoint m_cpMemory = new CPoint();
            public int m_yLine = 1000; 
            public override ModuleRunBase Clone()
            {
                Run_Grab run = new Run_Grab(m_module);
                run.p_sGrabMode = p_sGrabMode;
                run.m_fRes = m_fRes;
                run.m_bInvDir = m_bInvDir;
                run.m_nFocusPos = m_nFocusPos;
                run.m_rpAxis = new RPoint(m_rpAxis);
                run.m_cpMemory = new CPoint(m_cpMemory);
                run.m_yLine = m_yLine; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_rpAxis = tree.Set(m_rpAxis, m_rpAxis, "Center Axis Position", "Center Axis Position (mm ?)", bVisible);
                m_fRes = tree.Set(m_fRes, m_fRes, "Cam Resolution", "Resolution  um", bVisible);
                m_nFocusPos = tree.Set(m_nFocusPos, 0, "Focus Z Pos", "Focus Z Pos", bVisible);
                m_cpMemory = tree.Set(m_cpMemory, m_cpMemory, "Memory Position", "Grab Start Memory Position (pixel)", bVisible);
                m_bInvDir = tree.Set(m_bInvDir, m_bInvDir, "Inverse Direction", "Grab Direction", bVisible);
                m_yLine = tree.Set(m_yLine, m_yLine, "WaferSize", "# of Grab Lines", bVisible);
                p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
                if (m_grabMode != null) m_grabMode.RunTree(tree.GetTree("Grab Mode", false), bVisible, true); 
            }

            public override string Run()
            {
                if (m_grabMode == null) return "Grab Mode == null"; 
                try
                {
                    int nScanLine = 0;
                    m_grabMode.SetLight(true);
                    AxisXY axisXY = m_module.m_axisXY;
                    Axis axisZ = m_module.m_axisZ;
                    m_cpMemory.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X;
                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_fRes);        // 축해상도 0.1um로 하드코딩.
                    int nLines = Convert.ToInt32(m_yLine * 1000 / m_fRes);
                    while (m_grabMode.m_ScanLineNum > nScanLine )
                    {
                        if (EQ.IsStop())
                            return "OK";
                        double yAxis = m_grabMode.m_dTrigger * nLines;     // 총 획득할 Image Y 
                        /*위에서 아래로 찍는것을 정방향으로 함, 즉 Y 축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향*/
                        /* Grab하기 위해 이동할 Y축의 시작 끝 점*/
                        double yPos0 = m_rpAxis.Y - yAxis / 2 - m_grabMode.m_intervalAcc;
                        double yPos1 = m_rpAxis.Y + yAxis / 2 + m_grabMode.m_intervalAcc;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                        if (m_grabMode.m_bUseBiDirectionScan && Math.Abs(axisXY.p_axisY.p_posActual - yPos0) > Math.Abs(axisXY.p_axisY.p_posActual - yPos1))
                        {
                            double buffer = yPos0;            //yp1 <--> yp0 바꿈.
                            yPos0 = yPos1;
                            yPos1 = buffer;
                            m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                        }

                        double nPosX = m_rpAxis.X + nLines * m_grabMode.m_dTrigger / 2 - (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X * m_grabMode.m_dTrigger; //해상도추가필요

                        if (m_module.Run(axisZ.Move(m_nFocusPos)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.Move(new RPoint(nPosX, yPos0))))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        if (m_module.Run(axisZ.WaitReady()))
                            return p_sInfo;

                        double yTrigger0 = m_rpAxis.Y - yAxis / 2;
                        double yTrigger1 = m_rpAxis.Y + yAxis / 2;
                        m_module.m_axisXY.p_axisY.SetTrigger(yTrigger0, yTrigger1, m_grabMode.m_dTrigger, true);

                        /* 메모리 위치도 가져오게는 좀 다시 하자.*/
                        string sPool = "pool";
                        string sGroup = "group";
                        string sMem = "mem";
                        MemoryData mem = m_module.m_engineer.GetMemory(sPool, sGroup, sMem);

                        /* 방향 바꾸는 코드 들어가야함*/
                        m_grabMode.StartGrab(mem, m_cpMemory, nLines, m_grabMode.m_eGrabDirection == eGrabDirection.BackWard);
                        if (m_module.Run(axisXY.p_axisY.Move(yPos1)))
                            return p_sInfo;
                        if (m_module.Run(axisXY.WaitReady()))
                            return p_sInfo;
                        axisXY.p_axisY.ResetTrigger();

                        nScanLine++;
                        m_cpMemory.X += m_grabMode.m_camera.GetRoiSize().X;
                    }
                    m_grabMode.m_camera.StopGrabbing();
                    #region Grab1차 Test후에 코드 부분
                    //double yp0 = m_bInvDir ? m_rpAxis.Y + yAxis + m_grabMode.m_mmAcc : m_rpAxis.Y - m_grabMode.m_mmAcc;
                    //double yp1 = m_bInvDir ? m_rpAxis.Y - m_grabMode.m_mmAcc : m_rpAxis.Y + yAxis + m_grabMode.m_mmAcc;

                    //m_grabMode.SetLight(true);

                    //double yp0 = m_bInvDir ? m_rpAxis.Y + yAxis + m_grabMode.m_mmAcc : m_rpAxis.Y - m_grabMode.m_mmAcc;
                    //double yp1 = m_bInvDir ? m_rpAxis.Y - m_grabMode.m_mmAcc : m_rpAxis.Y + yAxis + m_grabMode.m_mmAcc;
                    //if (m_module.Run(axisXY.Move(new RPoint(m_rpAxis.X, yp1))))
                    //    return p_sInfo;
                    //if (m_module.Run(axisXY.WaitReady()))
                    //    return p_sInfo;

                    //double yTrigger0 = m_bInvDir ? m_rpAxis.Y - m_grabMode.m_mmAcc : m_rpAxis.Y;
                    //double yTrigger1 = m_bInvDir ? m_rpAxis.Y + yAxis : m_rpAxis.Y + yAxis;
                    //m_module.m_axisXY.m_axisY.SetTrigger(yTrigger0, yTrigger1, 10, true);
                    ////m_grabMode.SetTrigger(m_module.m_axisXY.m_axisY, yTrigger0, yTrigger1);
                    //string sPool = "pool";
                    //string sGroup = "grou p";
                    //string sMem = "mem";
                    //MemoryData mem = m_module.m_engineer.ClassMemoryTool().GetPool(sPool).GetGroup(sGroup).GetMemory(sMem);

                    ////m_grabMode.StartGrab(m_grabMode.m_memoryData, new CPoint(0, 0), m_yLine, m_bInvDir);
                    //m_grabMode.StartGrab(mem, m_cpMemory, m_yLine, m_bInvDir);
                    //if (m_module.Run(axisXY.m_axisY.Move(yp0)))
                    //    return p_sInfo;
                    //if (m_module.Run(axisXY.WaitReady()))
                    //    return p_sInfo;
                    //axisXY.m_axisY.ResetTrigger();
                    #endregion
                    return "OK";
                }
                finally
                {
                    //m_grabMode.ResetTrigger(m_module.m_axisXY.m_axisY);
                    m_grabMode.SetLight(false); 
                }
            }
        }
        #endregion
    }
}
