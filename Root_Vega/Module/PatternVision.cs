﻿using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Inspects;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using RootTools.ZoomLens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Media;

namespace Root_Vega.Module
{
    public class PatternVision : ModuleBase, IRobotChild
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
        public Camera_Dalsa m_CamMain;
        public Camera_Basler m_CamVRS;
        public Camera_Basler m_CamAlign1;
        public Camera_Basler m_CamAlign2;
        public Camera_Basler m_CamRADS;

        public LightSet m_lightSet;
        MemoryPool m_memoryPool;
        InspectTool m_inspectTool;
        ZoomLens m_ZoomLens;
        public ZoomLens p_ZoomLens;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "StageXY");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "StageZ");
            p_sInfo = m_toolBox.Get(ref m_CamMain, this, "MainCam");
            p_sInfo = m_toolBox.Get(ref m_CamVRS, this, "VRS");
            p_sInfo = m_toolBox.Get(ref m_CamAlign1, this, "Align1");
            p_sInfo = m_toolBox.Get(ref m_CamAlign2, this, "Align2");
            p_sInfo = m_toolBox.Get(ref m_CamRADS, this, "RADS");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this);
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_inspectTool, this);
            p_sInfo = m_toolBox.Get(ref m_ZoomLens, this, "ZoomLens");
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
            if (p_infoReticle != null) return p_id + " IsPutOK - InfoReticle Exist";
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

        #region override Function
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
        }
        #endregion

        public PatternVision(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        public void SetLightMainCam1(int nPower)
        {  
            m_lightSet.m_aLight[0].m_light.p_fSetPower = nPower;
        }
        public void SetLightMainCam2(int nPower)
        {
            m_lightSet.m_aLight[1].m_light.p_fSetPower = nPower;
        }
        public void SetLightVRS(int nPower)
        {
            m_lightSet.m_aLight[2].m_light.p_fSetPower = nPower;
        }
        public void SetLightINC1(int nPower)
        {
            m_lightSet.m_aLight[3].m_light.p_fSetPower = nPower;
            m_lightSet.m_aLight[4].m_light.p_fSetPower = nPower;
        }
        public void SetLightINC2(int nPower)
        {
            m_lightSet.m_aLight[5].m_light.p_fSetPower = nPower;
            m_lightSet.m_aLight[6].m_light.p_fSetPower = nPower;
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            AddModuleRunList(new Run_Run(this), true, "Run Side Vision");
            AddModuleRunList(new Run_Grab(this), true, "Run Grab");
        }

        public class Run_Delay : ModuleRunBase
        {
            PatternVision m_module;
            public Run_Delay(PatternVision module)
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

        public class Run_Run : ModuleRunBase
        {
            PatternVision m_module;
            public Run_Run(PatternVision module)
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
                Thread.Sleep((int)(1000 * m_secDelay));
                return "OK";
            }
        }

        public class Run_Grab : ModuleRunBase
        {
            PatternVision m_module;
            public Run_Grab(PatternVision module)
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
                    AxisXY axisXY = m_module.p_axisXY;
                    Axis axisZ = m_module.p_axisZ;
                    m_cpMemory.X += (nScanLine + m_grabMode.m_ScanStartLine) * m_grabMode.m_camera.GetRoiSize().X;
                    m_grabMode.m_dTrigger = Convert.ToInt32(10 * m_fRes);        // 축해상도 0.1um로 하드코딩.
                    int nLines = Convert.ToInt32(m_yLine * 1000 / m_fRes);
                    while (m_grabMode.m_ScanLineNum > nScanLine)
                    {
                        if (EQ.IsStop())
                            return "OK";
                        double yAxis = m_grabMode.m_dTrigger * nLines;     // 총 획득할 Image Y 
                        /*위에서 아래로 찍는것을 정방향으로 함, 즉 Y 축 값이 큰쪽에서 작은쪽으로 찍는것이 정방향*/
                        /* Grab하기 위해 이동할 Y축의 시작 끝 점*/
                        double yPos1 = m_rpAxis.Y - yAxis / 2 - 100000;
                        double yPos0 = m_rpAxis.Y + yAxis / 2 + 100000;

                        m_grabMode.m_eGrabDirection = eGrabDirection.Forward;
                        if (m_grabMode.m_bUseBiDirectionScan && Math.Abs(axisXY.p_axisY.p_posActual - yPos0) > Math.Abs(axisXY.p_axisY.p_posActual - yPos1))
                        {
                            double buffer = yPos0;            //yp1 <--> yp0 바꿈.
                            yPos0 = yPos1;
                            yPos1 = buffer;
                            m_grabMode.m_eGrabDirection = eGrabDirection.BackWard;
                        }

                        /* 조명 Set하는거 Test해서 넣어야됨.*/
                        //m_grabMode.SetLight(true);
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
                        m_module.p_axisXY.p_axisY.SetTrigger(yTrigger0, yTrigger1, m_grabMode.m_dTrigger, true);

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
                    m_grabMode.m_camera.StopGrab();
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
