using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;

namespace Root_VEGA_P_Vision.Module
{
    public class Holder : ModuleBase, IRTRChild
    {
        #region ToolBox
        DIO_I2O2 m_dioLifter;
        DIO_I m_diCover;
        DIO_I m_diPlate;
        DIO_I m_diExist;
        public override void GetTools(bool bInit)
        {
            if (p_eRemote == eRemote.Server)
            {
                p_sInfo = m_toolBox.GetDIO(ref m_diCover, this, "Cover Check");
                p_sInfo = m_toolBox.GetDIO(ref m_diPlate, this, "Plate Check");
                p_sInfo = m_toolBox.GetDIO(ref m_diExist, this, "Exist");
                p_sInfo = m_toolBox.GetDIO(ref m_dioLifter, this, "Lifter", "Down", "Up");
            }
            m_remote.GetTools(bInit);
        }
        #endregion

        #region DIO
        public enum eCheck
        {
            Empty,
            Cover,
            Plate,
            Error
        }

        public eCheck CheckSensor()
        {
            if (m_diExist.p_bIn)
            {
                if (m_diCover.p_bIn && !m_diPlate.p_bIn) return eCheck.Cover;
                if (!m_diCover.p_bIn && m_diPlate.p_bIn) return eCheck.Plate;
                return eCheck.Error;
            }
            else
            {
                return (m_diCover.p_bIn || m_diPlate.p_bIn) ? eCheck.Error : eCheck.Empty;
            }
        }

        public string RunLifter(bool bUp)
        {
            return m_dioLifter.RunSol(bUp);
        }
        public bool IsLifterUp()
        {
            if (m_dioLifter.m_aBitDI[1]==null) return false;
            return m_dioLifter.m_aBitDI[1].p_bOn;
        }
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
                value?.WriteReg();
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadPod_Registry()
        {
            m_reg = new Registry("InfoPod");
            int nPod = m_reg.Read(p_id, -1);
            if (nPod < 0) return;
            p_infoPod = new InfoPod((InfoPod.ePod)nPod);
            p_infoPod.ReadReg();
        }
        #endregion

        #region IRTRChild
        public bool p_bLock { get; set; }

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
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.BeforeGet, eRemote.Client, null);
            else
            {
                switch (CheckSensor())
                {
                    case eCheck.Cover:
                        if (!p_infoPod.p_ePod.Equals(InfoPod.ePod.EIP_Cover)) return "Not Cover";
                        if (Run(RunLifter(true))) return p_sInfo;
                        break;
                    case eCheck.Plate:
                        if (!p_infoPod.p_ePod.Equals(InfoPod.ePod.EIP_Plate)) return "Not Plate";
                        if (Run(RunLifter(false))) return p_sInfo;
                        break;
                    case eCheck.Empty:
                        return "Buffer Is Empty!";
                    case eCheck.Error:
                        return "Error";
                }
                p_infoPod = null;
                return "OK";
            }
        }

        public string BeforePut(InfoPod infoPod)
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.BeforePut, eRemote.Client, infoPod);
            else
            {
                eCheck state = CheckSensor();
                if (!state.Equals(eCheck.Empty))
                    return "Buffer Is Not Empty!";

                if (infoPod.p_ePod.Equals(InfoPod.ePod.EIP_Cover))
                {
                    if (Run(RunLifter(true)))
                        return p_sInfo;
                }
                else if (infoPod.p_ePod.Equals(InfoPod.ePod.EIP_Plate))
                {
                    if (Run(RunLifter(false))) 
                        return p_sInfo;
                }

                p_infoPod = infoPod;
                p_infoPod.p_bTurn = !p_infoPod.p_bTurn; //아직 flipping 하면서 put 하는게 안됨

                return "OK";
            }
        }

        public string AfterGet()
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.AfterGet, eRemote.Client, null);
            else
            {
                if (Run(RunLifter(false)))
                    return p_sInfo;

                return "OK";
            }
        }

        public string AfterPut()
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.AfterPut, eRemote.Client, null);
            else
            {
                if (Run(RunLifter(false)))
                    return p_sInfo;

                return "OK";
            }
        }

        public bool IsPodExist(InfoPod.ePod ePod)
        {
            return (p_infoPod != null);
        }

        public bool IsEnableRecovery()
        {
            return p_infoPod != null;
        }
        #endregion

        #region Teach RTR
        public class TeachRTR
        {
            int[] m_teachPlate = new int[2] { 0, 0 };
            int[] m_teachCover = new int[2] { 0, 0 };

            public int GetTeach(InfoPod infoPod)
            {
                int nTurn = infoPod.p_bTurn ? 1 : 0;
                switch (infoPod.p_ePod)
                {
                    case InfoPod.ePod.EIP_Cover: return m_teachCover[nTurn];
                    case InfoPod.ePod.EIP_Plate: return m_teachPlate[nTurn];
                }
                return -1;
            }

            public void RunTree(Tree tree)
            {
                RunTree(tree.GetTree("EIP Cover"), m_teachCover);
                RunTree(tree.GetTree("EIP Plate"), m_teachPlate);
            }

            void RunTree(Tree tree, int[] teach)
            {
                teach[0] = tree.Set(teach[0], teach[0], "Top", "RND RTR Teach");
                teach[1] = tree.Set(teach[1], teach[1], "Bottom", "RND RTR Teach");
            }
        }
        TeachRTR m_teach;

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
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.Reset, eRemote.Client, null);
            else
            {
                base.Reset();
            }
        }

        public override void InitMemorys()
        {
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.StateHome, eRemote.Client, null);
            else
            {
                Thread.Sleep(2000);
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
            //
        }
        #endregion

        public Holder(string id, IEngineer engineer, eRemote eRemote)
        {
            m_reg = new Registry(id + "_InfoPod");
            m_teach = new TeachRTR();
            InitBase(id, engineer, eRemote);
            OnChangeState += Holder_OnChangeState;
        }

        private void Holder_OnChangeState(eState eState)
        {
            switch (p_eState)
            {
                case eState.Init:
                case eState.Error:
                    RemoteRun(eRemoteRun.ServerState, eRemote.Server, eState);
                    break;
            }
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region RemoteRun
        public enum eRemoteRun
        {
            ServerState,
            StateHome,
            Reset,
            BeforeGet,
            BeforePut,
            AfterGet,
            AfterPut,
            TestResult,
        }

        Run_Remote GetRemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            Run_Remote run = new Run_Remote(this);
            run.m_eRemoteRun = eRemoteRun;
            run.m_eRemote = eRemote;
            switch (eRemoteRun)
            {
                case eRemoteRun.ServerState: run.m_eState = value; break;
                case eRemoteRun.StateHome: break;
                case eRemoteRun.Reset: break;
                case eRemoteRun.BeforeGet: break;
                case eRemoteRun.BeforePut: run.m_infoPod = value; break;
                case eRemoteRun.AfterGet: break;
                case eRemoteRun.AfterPut: break;
            }
            return run;
        }

        string RemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            Run_Remote run = GetRemoteRun(eRemoteRun, eRemote, value);
            StartRun(run);
            while (run.p_eRunState != ModuleRunBase.eRunState.Done)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
            }
            return p_sInfo;
        }

        public class Run_Remote : ModuleRunBase
        {
            Holder m_module;
            public Run_Remote(Holder module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eRemoteRun m_eRemoteRun = eRemoteRun.StateHome;
            public eState m_eState = eState.Init;
            public InfoPod m_infoPod = new InfoPod(InfoPod.ePod.EIP_Cover);
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                run.m_eState = m_eState;
                run.m_infoPod = m_infoPod;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eRemoteRun = (eRemoteRun)tree.Set(m_eRemoteRun, m_eRemoteRun, "RemoteRun", "Select Remote Run", bVisible);
                m_eRemote = (eRemote)tree.Set(m_eRemote, m_eRemote, "Remote", "Remote", false);
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.ServerState:
                        m_eState = (eState)tree.Set(m_eState, m_eState, "State", "Module State", bVisible);
                        break;
                    case eRemoteRun.BeforePut:
                        m_infoPod.RunTree(tree.GetTree("InfoPod", true, bVisible), bVisible);
                        break;
                }
            }

            public override string Run()
            {
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.ServerState: m_module.p_eState = m_eState; break;
                    case eRemoteRun.StateHome: return m_module.StateHome();
                    case eRemoteRun.Reset: m_module.Reset(); break;
                    case eRemoteRun.BeforeGet: return m_module.BeforeGet();
                    case eRemoteRun.BeforePut: return m_module.BeforePut(m_infoPod);
                    case eRemoteRun.AfterGet: return m_module.AfterGet();
                    case eRemoteRun.AfterPut: return m_module.AfterPut();
                }
                return "OK";
            }
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Remote(this), false, "Remote Run");
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
        }

        public class Run_Delay : ModuleRunBase
        {
            Holder m_module;
            public Run_Delay(Holder module)
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
