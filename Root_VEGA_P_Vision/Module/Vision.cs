using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Threading;

namespace Root_VEGA_P_Vision.Module
{
    public class Vision : ModuleBase, IRTRChild
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_stage.GetTools(m_toolBox, bInit);
            m_mainOptic.GetTools(m_toolBox, bInit);
            m_sideOptic.GetTools(m_toolBox, bInit); 
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
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                if (m_vision.p_eRemote == eRemote.Client) return;
                m_vision.p_sInfo = toolBox.Get(ref m_axisZ, m_vision, "Main Optic AxisZ");
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
            public Axis m_axisZ;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                if (m_vision.p_eRemote == eRemote.Client) return;
                m_vision.p_sInfo = toolBox.Get(ref m_axisZ, m_vision, "Main Optic AxisZ");
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
            public SideOptic(Vision vision)
            {
                m_vision = vision;
            }
        }
        SideOptic m_sideOptic;
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
        }
        #endregion

        public Vision(string id, IEngineer engineer, eRemote eRemote)
        {
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
            ModuleBase m_module;
            public Run_Delay(ModuleBase module)
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
        #endregion
    }
}
