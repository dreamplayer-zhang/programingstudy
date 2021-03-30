using Root_EFEM;
using Root_EFEM.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace Root_WindII_Option.Module
{
    public class Backside : ModuleBase, IWTRChild
    {
        #region ToolBox

        public override void GetTools(bool bInit)
        {
            //forget
            m_remote.GetTools(bInit);
        }
        #endregion

        #region InfoWafer
        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get { return _infoWafer; }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_reg != null) m_reg.Write("sInfoWafer", m_sInfoWafer);
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

        public List<string> p_asChildSlot { get { return null; } }

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
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoWafer == null) return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoWafer != null) return p_id + " IsPutOK - InfoWafer Exist";
            if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false) return p_id + " not Enable Wafer Size";
            return "OK";
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null) infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string BeforeGet(int nID)
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.BeforeGet, eRemote.Client, nID);
            else
            {
                //if (p_infoWafer == null) return m_id + " BeforeGet : InfoWafer = null";
                return CheckGetPut();
            }
        }

        public string BeforePut(int nID)
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.BeforePut, eRemote.Client, nID);
            else
            {
                // Move to Ready Pos ?
                // Vacuum Off ?
                return "OK";
            }
        }

        public string AfterGet(int nID)
        {
            return CheckGetPut();
        }

        public string AfterPut(int nID)
        {
            return "OK";
        }

        string CheckGetPut()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            return "OK";
        }

        enum eCheckWafer
        {
            InfoWafer,
            Sensor
        }
        eCheckWafer m_eCheckWafer = eCheckWafer.Sensor;
        public bool IsWaferExist(int nID)
        {
            switch (m_eCheckWafer)
            {
                //case eCheckWafer.Sensor: return m_diWaferExist.p_bIn;
                default: return (p_infoWafer != null);
            }
        }

        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }

        public void SetAlarm()
        {

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
            //memoryGroup = memoryPool.GetGroup(p_id);
            //m_mainOptic.InitMemorys();
            //m_sideOptic.InitMemorys();
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.StateHome, eRemote.Client, null);
            else
            {
                Thread.Sleep(100);
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
        }
        #endregion

        public Backside(string id, IEngineer engineer, eRemote eRemote)
        {
            InitBase(id, engineer, eRemote);
            OnChangeState += Backside_OnChangeState;
        }

        private void Backside_OnChangeState(eState eState)
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

        #region Test Result
        public class TestResult
        {
            public int m_nResult = 0;
            public string m_sResult = "OK";

            public void RunTree(Tree tree, bool bVisible)
            {
                m_nResult = tree.Set(m_nResult, m_nResult, "Int", "Int Result", bVisible);
                m_sResult = tree.Set(m_sResult, m_sResult, "String", "String Result", bVisible);
            }
        }
        TestResult _testResult = new TestResult();
        public TestResult p_testResult
        {
            get { return _testResult; }
            set
            {
                _testResult = value;
                if (p_eRemote == eRemote.Server) RemoteRun(eRemoteRun.TestResult, eRemote.Server, value);
            }
        }
        #endregion

        #region RemoteRun
        public enum eRemoteRun
        {
            ServerState,
            StateHome,
            Reset,
            BeforeGet,
            BeforePut,
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
                case eRemoteRun.BeforeGet: run.m_nSlotID = value; break;
                case eRemoteRun.BeforePut: run.m_nSlotID = value; break;
                case eRemoteRun.TestResult: run.m_testResult = value; break;
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
            Backside m_module;
            public Run_Remote(Backside module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eRemoteRun m_eRemoteRun = eRemoteRun.StateHome;
            public eState m_eState = eState.Init;
            public int m_nSlotID = 0; 
            public TestResult m_testResult = new TestResult();
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                run.m_eState = m_eState;
                run.m_nSlotID = m_nSlotID;
                run.m_testResult = m_testResult;
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
                    case eRemoteRun.BeforeGet:
                    case eRemoteRun.BeforePut:
                        m_nSlotID = tree.Set(m_nSlotID, m_nSlotID, "SlotID", "WTRChild SlotID", bVisible); 
                        break;
                    case eRemoteRun.TestResult:
                        m_testResult.RunTree(tree.GetTree("TestResult", true, bVisible), bVisible);
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
                    case eRemoteRun.BeforeGet: return m_module.BeforeGet(m_nSlotID);
                    case eRemoteRun.BeforePut: return m_module.BeforePut(m_nSlotID);
                    case eRemoteRun.TestResult: m_module.p_testResult = m_testResult; break;
                }
                return "OK";
            }
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Remote(this), true, "Remote Run");
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
        }

        public class Run_Delay : ModuleRunBase
        {
            Backside m_module;
            public Run_Delay(Backside module)
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
