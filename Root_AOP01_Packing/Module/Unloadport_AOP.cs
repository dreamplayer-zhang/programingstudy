using Root_EFEM;
using Root_EFEM.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace Root_AOP01_Packing.Module
{
    public class Unloadport_AOP : ModuleBase, IWTRChild
    {
        #region ToolBox
        //DIO_I m_diPlaced;
        //DIO_I m_diPresent;
        //DIO_I m_diLoad;
        //DIO_I m_diUnload;
        //DIO_I m_diDoorOpen;
        //DIO_I m_diDocked;
        //OHT m_OHT;
        public override void GetTools(bool bInit)
        {
            //p_sInfo = m_toolBox.Get(ref m_diPlaced, this, "Place");
            //p_sInfo = m_toolBox.Get(ref m_diPresent, this, "Present");
            //p_sInfo = m_toolBox.Get(ref m_diLoad, this, "Load");
            //p_sInfo = m_toolBox.Get(ref m_diUnload, this, "Unload");
            //p_sInfo = m_toolBox.Get(ref m_diDoorOpen, this, "DoorOpen");
            //p_sInfo = m_toolBox.Get(ref m_diDocked, this, "Docked");
            //p_sInfo = m_toolBox.Get(ref m_rs232, this, "RS232");
            //p_sInfo = m_toolBox.Get(ref m_OHT, this, p_infoCarrier, "OHT", m_diPlaced, m_diPresent);
            if (bInit)
            {
                //m_rs232.OnReceive += M_rs232_OnReceive;
                //m_rs232.p_bConnect = true;
            }
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
            //if (p_infoWafer == null) return m_id + " BeforeGet : InfoWafer = null";
            return CheckGetPut();
        }

        public string BeforePut(int nID)
        {
            if (p_infoWafer != null) return p_id + " BeforePut : InfoWafer != null";
            return CheckGetPut();
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
        #endregion

        #region Override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");
            m_waferSize.RunTree(tree.GetTree("Wafer Size", false), true);
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }
        #endregion

        public Unloadport_AOP(string id, IEngineer engineer)
        {
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
        }

        public class Run_Delay : ModuleRunBase
        {
            Unloadport_AOP m_module;
            public Run_Delay(Unloadport_AOP module)
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
        #endregion

    }
}
