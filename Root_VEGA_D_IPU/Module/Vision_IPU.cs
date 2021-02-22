using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;

namespace Root_VEGA_D_IPU.Module
{
    public class Vision_IPU : ModuleBase
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

        #region InfoWafer ??
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
            m_waferSize = new InfoWafer.WaferSize(id, false, false); //forget delete ?
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
