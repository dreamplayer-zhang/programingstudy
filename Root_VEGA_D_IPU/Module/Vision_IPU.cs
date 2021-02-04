using Root_EFEM.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace Root_VEGA_D_IPU.Module
{
    public class Vision_IPU : ModuleBase, IWTRChild
    {
        #region ToolBox

        public override void GetTools(bool bInit)
        {
            switch (p_eRemote)
            {
                case eRemote.Client: GetClientTools(bInit); break;
                case eRemote.Server: GetServerTools(bInit); break; 
            }
            m_remote.GetTools(bInit);
        }

        void GetClientTools(bool bInit)
        {

        }

        void GetServerTools(bool bInit)
        {

        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }

        public override void InitMemorys()
        {
            switch (p_eRemote)
            {
                case eRemote.Client:
                    break;
                case eRemote.Server:
                    break; 
            }
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

        public List<string> p_asChildSlot
        {
            get { return null; }
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
            switch (p_eRemote)
            {
                case eRemote.Client:
                    break;
                case eRemote.Server:
                    break; 
            }
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            switch (p_eRemote)
            {
                case eRemote.Client: return "OK"; 
                case eRemote.Server:
                    if (p_eState != eState.Ready) return p_id + " eState not Ready";
                    if (p_infoWafer != null) return p_id + " IsPutOK - InfoWafer Exist";
                    if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false) return p_id + " not Enable Wafer Size";
                    break; 
            }
            return "OK";
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null) infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string BeforeGet(int nID)
        {
            switch (p_eRemote)
            {
                case eRemote.Client:
                    return m_remote.RemoteSend(Remote.eProtocol.BeforeGet, "Get", "Get");
                case eRemote.Server:
                    //forget
                    break; 
            }
            return "OK";
        }

        public override string ServerBeforeGet()
        {
            return BeforeGet(0);
        }

        public string BeforePut(int nID)
        {
            switch (p_eRemote)
            {
                case eRemote.Client:
                    break;
                case eRemote.Server:
                    break; 
            }
            return "OK";
        }
        public override string ServerBeforePut()
        {
            return BeforePut(0);
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
                case eCheckWafer.Sensor: return false; // m_diWaferExist.p_bIn;
                default: return (p_infoWafer != null);
            }
        }

        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }

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

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            switch (p_eRemote)
            {
                case eRemote.Client:
                    m_remote.RemoteSend(Remote.eProtocol.Initial, "INIT", "INIT");
//                    ClearData();
                    return "OK";
                case eRemote.Server:
                    Thread.Sleep(200);
                    //if (m_CamMain != null && m_CamMain.p_CamInfo.p_eState == eCamState.Init) m_CamMain.Connect();
                    p_sInfo = base.StateHome();
                    p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
                    //p_bStageVac = false;
                    //ClearData();
                    return "OK";
            }
            return "OK";
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
//            RunTreeAxis(tree.GetTree("Axis", false));
//            RunTreeGrabMode(tree.GetTree("Grab Mode", false));
        }
        #endregion

        public Vision_IPU(string id, IEngineer engineer, eRemote eRemote)
        {
            //            InitLineScan();+
            //            InitAreaScan();
            base.InitBase(id, engineer, eRemote);
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            //            InitMemory();
        }

        public override void ThreadStop()
        {
            //
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
//            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
//            AddModuleRunList(new Run_Rotate(this), false, "Rotate Axis");
//            AddModuleRunList(new Run_GrabLineScan(this), true, "Run Grab LineScan Camera");
//            AddModuleRunList(new Run_Inspect(this), true, "Run Inspect");
//            AddModuleRunList(new Run_VisionAlign(this), true, "Run VisionAlign");
//            AddModuleRunList(new Run_AutoFocus(this), false, "Run AutoFocus");
        }
        #endregion

    }
}
