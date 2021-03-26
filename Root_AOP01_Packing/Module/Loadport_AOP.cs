using Root_EFEM.Module;
using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using RootTools.OHTNew;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_AOP01_Packing.Module
{
    public class Loadport_AOP : ModuleBase, IWTRChild, ILoadport
    {
        #region ToolBox
        DIO_I[] m_diPodCheck = new DIO_I[3]; 
        DIO_I2O2 m_dioGuide;
        Axis m_axisDoor;
        DIO_I[] m_diDoor = new DIO_I[2];
        DIO_O m_doManual;
        DIO_O m_doAuto;
        DIO_O m_doPresent;
        DIO_O m_doPlaced;
        DIO_O m_doLoad;
        DIO_O m_doUnload;
        DIO_O m_doAlarm;
        //OHT m_OHT;
        OHT _OHT;
        public OHT m_OHTNew
        {
            get { return _OHT; }
            set
            {
                _OHT = value;
                OnPropertyChanged();
            }
        }
        ALID m_alid_WaferExist;
        public void SetAlarm()
        {
            m_alid_WaferExist.Run(true, "Wafer Exist Error");
        }
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetDIO(ref m_diPodCheck[0], this, "POD Check0");
            p_sInfo = m_toolBox.GetDIO(ref m_diPodCheck[1], this, "POD Check1");
            p_sInfo = m_toolBox.GetDIO(ref m_diPodCheck[2], this, "POD Check2");
            p_sInfo = m_toolBox.GetDIO(ref m_dioGuide, this, "Guide", "Up", "Down");
            p_sInfo = m_toolBox.GetAxis(ref m_axisDoor, this, "Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diDoor[0], this, "Door Close");
            p_sInfo = m_toolBox.GetDIO(ref m_diDoor[1], this, "Door Open");
            p_sInfo = m_toolBox.GetDIO(ref m_doManual, this, "Manual");
            p_sInfo = m_toolBox.GetDIO(ref m_doAuto, this, "Auto");
            p_sInfo = m_toolBox.GetDIO(ref m_doPresent, this, "Present");
            p_sInfo = m_toolBox.GetDIO(ref m_doPlaced, this, "Placed");
            p_sInfo = m_toolBox.GetDIO(ref m_doLoad, this, "Load");
            p_sInfo = m_toolBox.GetDIO(ref m_doUnload, this, "Unload");
            p_sInfo = m_toolBox.GetDIO(ref m_doAlarm, this, "Alram");
            //p_sInfo = m_toolBox.Get(ref m_OHT, this, p_infoCarrier, "OHT");
            p_sInfo = m_toolBox.GetOHT(ref _OHT, this, p_infoCarrier, "OHT");
            if (bInit) 
            {
                InitPos();
                m_doManual.Write(false);
                m_doAuto.Write(false);
                m_doPresent.Write(false);
                m_doPlaced.Write(false);
                m_doLoad.Write(false);
                m_doUnload.Write(true); 
            }
        }
        #endregion

        #region Digital output
        bool _doManual = false;
        bool p_doManual
        {
            get { return _doManual; }
            set
            {
                if (_doManual == value) return;
                _doManual = value;
                m_doManual.Write(value); 
            }
        }

        bool _doAuto = false;
        bool p_doAuto
        {
            get { return _doAuto; }
            set
            {
                if (_doAuto == value) return;
                _doAuto = value;
                m_doAuto.Write(value);
            }
        }

        bool _doPresent = false;
        bool p_doPresent
        {
            get { return _doPresent; }
            set
            {
                if (_doPresent == value) return;
                _doPresent = value;
                m_doPresent.Write(value);
            }
        }

        bool _doPlaced = false;
        bool p_doPlaced
        {
            get { return _doPlaced; }
            set
            {
                if (_doPlaced == value) return;
                _doPlaced = value;
                m_doPlaced.Write(value);
            }
        }

        bool _doLoad = false;
        bool p_doLoad
        {
            get { return _doLoad; }
            set
            {
                if (_doLoad == value) return;
                _doLoad = value;
                m_doLoad.Write(value);
            }
        }

        bool _doUnload = false;
        bool p_doUnload
        {
            get { return _doUnload; }
            set
            {
                if (_doUnload == value) return;
                _doUnload = value;
                m_doUnload.Write(value);
            }
        }

        bool _doAlram = false;
        bool p_doAlram
        {
            get { return _doAlram; }
            set
            {
                if (_doAlram == value) return;
                _doAlram = value;
                m_doAlarm.Write(value);
            }
        }
        #endregion

        #region Check
        public bool CheckPlaced()
        {
            GemCarrierBase.ePresent present = GemCarrierBase.ePresent.Unknown;
            int nCheck = 0; 
            for (int n = 0; n < 3; n++)
            {
                if (m_diPodCheck[n].p_bIn) nCheck++; 
            }
            switch (nCheck)
            {
                case 0: present = GemCarrierBase.ePresent.Empty; break;
                case 3: present = GemCarrierBase.ePresent.Exist; break;
                default: present = GemCarrierBase.ePresent.Unknown; break; 
            }
            if (p_infoCarrier.CheckPlaced(present) != "OK") m_alidPlaced.Run(true, "Placed Sensor Remain Checked while Pod State = " + p_infoCarrier.p_eState);
            switch (p_infoCarrier.p_ePresentSensor)
            {
                case GemCarrierBase.ePresent.Empty: m_svidPlaced.p_value = false; break;
                case GemCarrierBase.ePresent.Exist: m_svidPlaced.p_value = true; break;
            }
            return (m_svidPlaced.p_value == null) ? false : m_svidPlaced.p_value;
        }
        #endregion

        #region Guide
        public string RunGuide(bool bDown)
        {
            m_dioGuide.Write(bDown);
            return m_dioGuide.WaitDone(); 
        }
        #endregion

        #region Door
        public enum eDoor
        {
            Running,
            Close,
            Open,
            Error
        }
        eDoor _eDoor = eDoor.Running;
        public eDoor p_eDoor
        {
            get { return _eDoor; }
            set
            {
                if (_eDoor == value) return;
                _eDoor = value;
                OnPropertyChanged();
            }
        }

        public enum ePos
        {
            Close,
            Open
        }
        void InitPos()
        {
            m_axisDoor.AddPos(Enum.GetNames(typeof(ePos)));
        }

        public string RunDoorMove(ePos ePos)
        {
            m_axisDoor.StartMove(ePos);
            return m_axisDoor.WaitReady();
        }
        #endregion

        #region Thread Check
        bool m_bThreadCheck = false;
        Thread m_threadCheck;
        void InitThread()
        {
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadCheck.Start();
        }

        void RunThreadCheck()
        {
            m_bThreadCheck = true;
            Thread.Sleep(3000);
            while (m_bThreadCheck)
            {
                Thread.Sleep(10);
                CheckPlaced();
                int nDoor = m_diDoor[1].p_bIn ? 2 : 0;
                if (m_diDoor[0].p_bIn) nDoor++;
                p_eDoor = (eDoor)nDoor;
                p_doManual = (EQ.p_eState != EQ.eState.Run);
                p_doAuto = (EQ.p_eState == EQ.eState.Run);
                p_doPresent = p_bPresent;
                p_doPlaced = p_bPlaced;
                p_doAlram = (p_eState == eState.Error); 
            }
        }
        #endregion

        #region IWTRChild
        public bool p_bLock { get; set; }

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
            get { return p_infoCarrier.p_asGemSlot; }
        }

        public InfoWafer p_infoWafer { get; set; }

        public InfoWafer GetInfoWafer(int nID)
        {
            return p_infoCarrier.GetInfoWafer(nID);
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            p_infoCarrier.SetInfoWafer(nID, infoWafer);
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            return p_infoCarrier.GetTeachWTR(infoWafer);
        }

        public string IsGetOK(int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            return p_infoCarrier.IsGetOK(nID);
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            return p_infoCarrier.IsPutOK(nID);
        }

        public string BeforeGet(int nID)
        {
            if (GetInfoWafer(nID) == null) return p_id + nID.ToString("00") + " BeforeGet : InfoWafer = null";
            return IsRunOK();
        }

        public string BeforePut(int nID)
        {
            if (GetInfoWafer(nID) != null) return p_id + nID.ToString("00") + " BeforePut : InfoWafer != null";
            return IsRunOK();
        }

        public string AfterGet(int nID)
        {
            return IsRunOK();
        }

        public string AfterPut(int nID)
        {
            return IsRunOK();
        }

        public bool IsWaferExist(int nID = 0)
        {
            switch (p_infoCarrier.p_eState)
            {
                case InfoCarrier.eState.Empty: return false;
                case InfoCarrier.eState.Placed: return false;
            }
            return (p_infoCarrier.GetInfoWafer(nID) != null);
        }

        string IsRunOK()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            return p_infoCarrier.IsRunOK();
        }

        public void RunTreeTeach(Tree tree)
        {
            p_infoCarrier.m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }

        public void ReadInfoWafer_Registry()
        {
            p_infoCarrier.ReadInfoWafer_Registry();
        }
        #endregion

        #region Timeout
        public int p_secHome { get; set; }
        int m_secMotion = 20;
        void RunTimeoutTree(Tree tree)
        {
            p_secHome = tree.Set(p_secHome, p_secHome, "Home", "Timeout (sec)");
            m_secMotion = tree.Set(m_secMotion, m_secMotion, "Motion", "Timeout (sec)");
        }
        #endregion

        #region override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTimeoutTree(tree.GetTree("Timeout", false));
            p_infoCarrier.m_waferSize.RunTree(tree.GetTree("Wafer Size", false), true);
        }

        public override void Reset()
        {
            p_eState = eState.Init;
            base.Reset();
        }

        public override void ButtonHome()
        {
            base.ButtonHome();
        }
        #endregion

        #region StateHome
        public override string StateHome()
        {
            if (EQ.p_bSimulate == false)
            {
                p_sInfo = base.StateHome();
                if (p_sInfo != "OK") return p_sInfo; 
            }
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            p_infoCarrier.p_eState = InfoCarrier.eState.Empty;
            p_infoCarrier.AfterHome();
            return p_sInfo;
        }
        #endregion

        #region StateReady
        public override string StateReady()
        {
            CheckPlaced();
            if (p_infoCarrier.m_bReqLoad)
            {
                p_infoCarrier.m_bReqLoad = false;
                StartRun(m_runDocking);
            }
            if (p_infoCarrier.m_bReqUnload)
            {
                p_infoCarrier.m_bReqUnload = false;
                StartRun(m_runUndocking);
            }
            return "OK";
        }
        #endregion

        #region GAF
        SVID m_svidPlaced;
        CEID m_ceidDocking;
        CEID m_ceidUnDocking;
        ALID m_alidPlaced;
        void InitGAF()
        {
            m_svidPlaced = m_gaf.GetSVID(this, "Placed");
            m_ceidDocking = m_gaf.GetCEID(this, "Docking");
            m_ceidUnDocking = m_gaf.GetCEID(this, "UnDocking");
            m_alidPlaced = m_gaf.GetALID(this, "Placed Sensor Error", "Placed & Plesent Sensor Should be Checked");
        }
        #endregion

        #region ILoadport
        public string RunDocking()
        {
            if (p_infoCarrier.p_eState == InfoCarrier.eState.Dock) return "OK";
            ModuleRunBase run = m_runDocking.Clone();
            StartRun(run);
            while (IsBusy() && (EQ.IsStop() == false)) Thread.Sleep(10);
            return EQ.IsStop() ? "EQ Stop" : "OK";
        }

        public string RunUndocking()
        {
            if (p_infoCarrier.p_eState != InfoCarrier.eState.Dock) return "OK";
            ModuleRunBase run = m_runUndocking.Clone();
            StartRun(run);
            while (IsBusy() && (EQ.IsStop() == false)) Thread.Sleep(10);
            return EQ.IsStop() ? "EQ Stop" : "OK";
        }

        public bool p_bPlaced { get { return m_diPodCheck[0].p_bIn; } }
        public bool p_bPresent { get { return (m_diPodCheck[1].p_bIn && m_diPodCheck[2].p_bIn); } }
        #endregion

        #region Docking & Undocking
        public string RunDock()
        {
            if (p_infoCarrier.p_eState != InfoCarrier.eState.Placed) return "InfoCarrier State not Placed";
            if (Run(RunDoorMove(ePos.Open))) return p_sInfo;
            if (Run(RunGuide(true))) return p_sInfo;
            if (p_eDoor != eDoor.Open) return "Door Open Sensor Error";
            p_infoCarrier.p_eState = InfoCarrier.eState.Dock;
            p_doUnload = false; 
            p_doLoad = true; 
            m_ceidDocking.Send();
            return "OK";
        }

        public string RunUnDock()
        {
            if (p_infoCarrier.p_eState != InfoCarrier.eState.Dock) return "InfoCarrier State not Dock";
            if (Run(RunDoorMove(ePos.Close))) return p_sInfo;
            if (Run(RunGuide(true))) return p_sInfo;
            if (p_eDoor != eDoor.Close) return "Door Close Sensor Error";
            p_doUnload = true;
            p_doLoad = false;
            m_ceidUnDocking.Send();
            p_infoCarrier.p_eState = InfoCarrier.eState.Placed;
            return "OK";
        }
        #endregion

        IRFID _rfid;
        public IRFID m_rfid
        {
            get { return _rfid; }
            set
            {
                _rfid = value;
            }
        }
        public InfoCarrier p_infoCarrier { get; set; }
        public Loadport_AOP(string id, IEngineer engineer, bool bEnableWaferSize, bool bEnableWaferCount)
        {
            p_secHome = 40; 
            p_bLock = false;
            p_id = id;
            p_infoCarrier = new InfoCarrier(this, id, engineer, bEnableWaferSize, bEnableWaferCount);
            m_aTool.Add(p_infoCarrier);
            InitBase(id, engineer);
            InitGAF();
            if (m_gem != null) m_gem.OnGemRemoteCommand += M_gem_OnRemoteCommand;
            InitThread(); 
        }

        public override void ThreadStop()
        {
            if (m_bThreadCheck)
            {
                m_bThreadCheck = false;
                m_threadCheck.Join();
            }
            base.ThreadStop();
        }

        private void M_gem_OnRemoteCommand(string sCmd, Dictionary<string, string> dicParam, long[] pnResult)
        {
        }

        #region ModuleRun
        ModuleRunBase m_runDocking;
        ModuleRunBase m_runUndocking;
        public ModuleRunBase GetModuleRunUndocking()
        {
            return m_runUndocking;
        }
        public ModuleRunBase GetModuleRunDocking()
        {
            return m_runDocking;
        }

        protected override void InitModuleRuns()
        {
            m_runDocking = AddModuleRunList(new Run_Docking(this), false, "Docking Carrier to Work Position");
            m_runUndocking = AddModuleRunList(new Run_Undocking(this), false, "Undocking Carrier from Work Position");
        }

        public class Run_Docking : ModuleRunBase
        {
            Loadport_AOP m_module;
            InfoCarrier m_infoCarrier;
            public Run_Docking(Loadport_AOP module)
            {
                m_module = module;
                m_infoCarrier = module.p_infoCarrier;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Docking run = new Run_Docking(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunDock(); 
            }
        }

        public class Run_Undocking : ModuleRunBase
        {
            Loadport_AOP m_module;
            InfoCarrier m_infoCarrier;
            public Run_Undocking(Loadport_AOP module)
            {
                m_module = module;
                m_infoCarrier = module.p_infoCarrier;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Undocking run = new Run_Undocking(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUnDock();
            }
        }
        #endregion

        
    }
}

