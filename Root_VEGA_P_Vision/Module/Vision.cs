﻿using RootTools;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Root_VEGA_P_Vision.Module
{
    public class Vision : ModuleBase, IRTRChild
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_stage.GetTools(m_toolBox, bInit); 
            m_mainOptic.GetTools(m_toolBox, bInit); //TDI
            m_sideOptic.GetTools(m_toolBox, bInit);  //
            m_remote.GetTools(bInit);
        }
        #endregion

        #region Stage
        public class Stage : NotifyProperty
        {
            public AxisXY m_axisXY;
            public Axis m_axisR;
            DIO_I[] m_diStageLoad = new DIO_I[2] { null, null };
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                if (m_vision.p_eRemote == eRemote.Client) return;
                m_vision.p_sInfo = toolBox.Get(ref m_axisXY, m_vision, "Stage");
                m_vision.p_sInfo = toolBox.Get(ref m_axisR, m_vision, "Stage Rotate");
                m_vision.p_sInfo = toolBox.Get(ref m_diStageLoad[0], m_vision, "Stage Load X");
                m_vision.p_sInfo = toolBox.Get(ref m_diStageLoad[1], m_vision, "Stage Load Y");
                if (bInit)
                {

                }
            }

            public bool IsLoad()
            {
                if (m_diStageLoad[0] == null) return false;
                if (m_diStageLoad[1] == null) return false;
                return m_diStageLoad[0].p_bIn && m_diStageLoad[1].p_bIn; 
            }

            double _pulsePerRound = 360000;
            public double p_pulsePerRound
            {
                get { return _pulsePerRound; }
                set
                {
                    _pulsePerRound = value;
                    OnPropertyChanged(); 
                }
            }

            public string Rotate(double degree, bool bWait = true)
            {
                string sRun = m_axisR.StartMove(degree * p_pulsePerRound / 360);
                if (sRun != "OK") return sRun;
                return bWait ? m_axisR.WaitReady() : "OK";
            }

            double m_pulsePermm = 10000; 
            public string Move(double mmX, double mmY, bool bWait = true)
            {
                string sRun = m_axisXY.StartMove(mmX * m_pulsePermm, mmY * m_pulsePermm);
                if (sRun != "OK") return sRun;
                return bWait ? m_axisXY.WaitReady() : "OK";
            }

            public void RunTree(Tree tree)
            {
                m_pulsePermm = tree.Set(m_pulsePermm, m_pulsePermm, "Pulse / mm", "Stage XY Pulse per 1mm (pulse)"); 
                p_pulsePerRound = tree.Set(p_pulsePerRound, p_pulsePerRound, "Pulse / Round", "Stage Rotate Pulse per Round (pulse)"); 
            }

            Vision m_vision; 
            public Stage(Vision vision)
            {
                m_vision = vision; 
            }
        }
        public Stage m_stage;
        #endregion

        #region MainOptic
        public class MainOptic : NotifyProperty
        {
            public Axis m_axisZ;
            MemoryPool memoryPool;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                if (m_vision.p_eRemote == eRemote.Client) return;
                m_vision.p_sInfo = toolBox.Get(ref m_axisZ, m_vision, "Main Optic AxisZ");
                m_vision.p_sInfo = toolBox.Get(ref memoryPool, m_vision, "Main Optic Memory", 1);

                if (bInit)
                {

                }
            }

            public void InitMemorys()
            {
                //m_memoryGroup = m_memoryPool.GetGroup(p_id);
                //m_memoryMain = m_memoryGroup.CreateMemory("Main", 3, 1, 40000, 40000);
                //m_memoryMain = m_memoryGroup.CreateMemory("Layer", 1, 4, 30000, 30000); // Chip 크기 최대 30,000 * 30,000 고정 Origin ROI 메모리 할당 20.11.02 JTL 
            }

            double m_pulsePermm = 10000;
            public string Move(double mmZ, bool bWait = true)
            {
                string sRun = m_axisZ.StartMove(mmZ * m_pulsePermm);
                if (sRun != "OK") return sRun;
                return bWait ? m_axisZ.WaitReady() : "OK";
            }

            public void RunTree(Tree tree)
            {
                m_pulsePermm = tree.Set(m_pulsePermm, m_pulsePermm, "Pulse / mm", "Stage XY Pulse per 1mm (pulse)");
            }

            Vision m_vision;
            public MainOptic(Vision vision)
            {
                m_vision = vision;
            }
        }
        MainOptic m_mainOptic;
        #endregion

        #region SideOptic
        public class SideOptic : NotifyProperty
        {
            public enum eSide
            {
                Top,Left,Right,Bottom
            }
            public Axis m_axisZ;

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                if (m_vision.p_eRemote == eRemote.Client) return;
                m_vision.p_sInfo = toolBox.Get(ref m_axisZ, m_vision, "Side Optic AxisZ");

                if (bInit)
                {

                }
            }

            public void InitMemorys()
            {
                //memoryGroup = memoryPool.GetGroup(m_vision.p_id);
                //memoryTop = memoryGroup.CreateMemory(eSide.Top.ToString(), 1, 1, 1000, 1000);
                //memoryLeft = memoryGroup.CreateMemory(eSide.Left.ToString(), 1, 1, 1000, 1000);
                //memoryRight = memoryGroup.CreateMemory(eSide.Right.ToString(), 1, 1, 1000, 1000);
                //memoryBottom = memoryGroup.CreateMemory(eSide.Bottom.ToString(), 1, 1, 1000, 1000);
            }

            double m_pulsePermm = 10000;
            public string Move(double mmZ, bool bWait = true)
            {
                string sRun = m_axisZ.StartMove(mmZ * m_pulsePermm);
                if (sRun != "OK") return sRun;
                return bWait ? m_axisZ.WaitReady() : "OK";
            }

            public void RunTree(Tree tree)
            {
                m_pulsePermm = tree.Set(m_pulsePermm, m_pulsePermm, "Pulse / mm", "Stage XY Pulse per 1mm (pulse)");
            }

            Vision m_vision;
            public SideOptic(Vision vision)
            {
                m_vision = vision;
            }
        }
        SideOptic m_sideOptic;
        #endregion

        #region InfoPod
        InfoPod _infoPod = null;
        public InfoPod p_infoPod
        {
            get { return _infoPod; }
            set
            {
                int nPod = (value != null) ? (int)value.p_ePod : -1; 
                _infoPod = value;
                m_reg.Write("InfoPod", nPod);
                value.WriteReg(); 
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadPod_Registry()
        {
            int nPod = m_reg.Read("InfoPod", -1);
            if (nPod < 0) return;  
            p_infoPod = new InfoPod((InfoPod.ePod)nPod);
            p_infoPod.ReadReg();
        }
        #endregion

        #region IRTRChild
        public string IsGetOK()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready"; 
            return (p_infoPod != null) ? "OK" : p_id + " IsGetOK - Pod not Exist";
        }

        public string IsPutOK(InfoPod infoPod)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            switch (infoPod.p_ePod)
            {
                case InfoPod.ePod.EOP_Dome:
                case InfoPod.ePod.EOP_Door:
                    return p_id + " Invalid Pod Type"; 
            }
            return (p_infoPod == null) ? "OK" : p_id + " IsPutOK - Pod Exist";
        }

        public string BeforeGet()
        {
            // Move to Ready Pos ?
            // Vacuum Off ?
            return "OK";
        }

        public string BeforePut(InfoPod infoPod)
        {
            // Move to Ready Pos ?
            // Vacuum Off ?
            return "OK";
        }

        public string AfterGet()
        {
            // ??
            return "OK";
        }

        public string AfterPut()
        {
            // ??
            return "OK";
        }

        public bool IsPodExist()
        {
            return (p_infoPod != null);
        }
        #endregion

        #region Teach RTR
        Buffer.TeachRTR m_teach; 
        public int GetTeachRTR(InfoPod infoPod)
        {
            return m_teach.GetTeach(infoPod);
        }

        public void RunTreeTeach(Tree tree)
        {
            m_teach.RunTree(tree.GetTree(p_id));
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }

        public override void InitMemorys()
        {
            m_mainOptic.InitMemorys();
            m_sideOptic.InitMemorys(); 
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";

            if (p_eRemote == eRemote.Client)
            {
                m_remote.RemoteSend(Remote.eProtocol.Initial, "INIT", "INIT");
                return "OK";
            }
            else
            {
                p_sInfo = base.StateHome();
                p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
                return "OK";
            }
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_stage.RunTree(tree.GetTree("Stage"));
            m_mainOptic.RunTree(tree.GetTree("Main Optic"));
            m_sideOptic.RunTree(tree.GetTree("Side Optic"));
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
                foreach (GrabMode grabMode in m_aGrabMode) 
                    asGrabMode.Add(grabMode.p_sName);
                return asGrabMode;
            }
        }

        public GrabMode GetGrabMode(string sGrabMode)
        {
            foreach (GrabMode grabMode in m_aGrabMode)
                if (sGrabMode == grabMode.p_sName) return grabMode;
          
            return null;
        }

        void RunTreeGrabMode(Tree tree)
        {
            m_lGrabMode = tree.Set(m_lGrabMode, m_lGrabMode, "Count", "Grab Mode Count");
            while (m_aGrabMode.Count < m_lGrabMode)
            {
                string id = "Mode." + m_aGrabMode.Count.ToString("00");
                //GrabMode grabMode = new GrabMode(id, lightSet, memoryPool);
                //m_aGrabMode.Add(grabMode);
            }
            while (m_aGrabMode.Count > m_lGrabMode) m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
            foreach (GrabMode grabMode in m_aGrabMode) grabMode.RunTreeName(tree.GetTree("Name", false));
            foreach (GrabMode grabMode in m_aGrabMode)
                grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
        }

        #endregion

        public Vision(string id, IEngineer engineer, eRemote eRemote)
        {
            m_reg = new Registry(p_id + "_InfoPod");
            m_teach = new Buffer.TeachRTR(); 
            m_stage = new Stage(this);
            m_mainOptic = new MainOptic(this);
            m_sideOptic = new SideOptic(this); 
            InitBase(id, engineer, eRemote); 
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
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
                Thread.Sleep((int)(1000 * m_secDelay / 2));
                return "OK";
            }
        }

        public class Run_SideGrab : ModuleRunBase
        {
            Vision m_module;
            GrabMode SidegrabMode = null;
            string sSideGrabMode = "";
            public Run_SideGrab(Vision module)
            {
                m_module = module;
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
                return "OK";
            }
        }
        #endregion
    }
}
