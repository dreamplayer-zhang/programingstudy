using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using RootTools.OHTNew;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace Root_EFEM.Module
{
    public class Loadport_RND : ModuleBase, IWTRChild, ILoadport
    {
        #region ToolBox
        DIO_I m_diPlaced;
        DIO_I m_diPresent;
        DIO_I m_diLoad;
        DIO_I m_diUnload;
        DIO_I m_diDoorOpen;
        DIO_I m_diDocked;
        RS232 m_rs232;
        OHT m_OHT; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_diPlaced, this, "Place");
            p_sInfo = m_toolBox.Get(ref m_diPresent, this, "Present");
            p_sInfo = m_toolBox.Get(ref m_diLoad, this, "Load");
            p_sInfo = m_toolBox.Get(ref m_diUnload, this, "Unload");
            p_sInfo = m_toolBox.Get(ref m_diDoorOpen, this, "DoorOpen");
            p_sInfo = m_toolBox.Get(ref m_diDocked, this, "Docked");
            p_sInfo = m_toolBox.Get(ref m_rs232, this, "RS232");
            p_sInfo = m_toolBox.Get(ref m_OHT, this, p_infoCarrier, "OHT", m_diPlaced, m_diPresent); 
            if (bInit)
            {
                m_rs232.OnReceive += M_rs232_OnReceive;
                m_rs232.p_bConnect = true;
            }
        }
        #endregion

        #region DIO Function
        public bool CheckPlaced()
        {
            GemCarrierBase.ePresent present;
            if (m_diPlaced.p_bIn != m_diPresent.p_bIn) present = GemCarrierBase.ePresent.Unknown;
            else present = m_diPlaced.p_bIn ? GemCarrierBase.ePresent.Exist : GemCarrierBase.ePresent.Empty;
            if (p_infoCarrier.CheckPlaced(present) != "OK") m_alidPlaced.Run(true, "Placed Sensor Remain Checked while Pod State = " + p_infoCarrier.p_eState);
            switch (p_infoCarrier.p_ePresentSensor)
            {
                case GemCarrierBase.ePresent.Empty: m_svidPlaced.p_value = false; break;
                case GemCarrierBase.ePresent.Exist: m_svidPlaced.p_value = true; break;
            }
            return m_svidPlaced.p_value;
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
            get { return p_infoCarrier.m_asGemSlot; }
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

        #region ErrorCode
        string[,] m_sErrorMsgs = new string[,]
        {
            { "201", "Foup Clamp UP Error" },
            { "202", "Foup Clamp Down Error" },
            { "203", "Foup Clam Lock Error" },
            { "204", "Foup Clamp Foreward Error" },
            { "205", "Foup Clamp Back Error" },
            { "206", "Foup Docking Error" },
            { "207", "Foup Undocking Error" },
            { "208", "Door Latch Error" },
            { "209", "Door Unlatch Error" },
            { "210", "Door Suction On Error" },
            { "211", "Door Suction On/Off Error" },
            { "212", "Door Open Error" },
            { "213", "Door Close Error" },
            { "214", "Mapping Arm Home Error" },
            { "215", "Mapping Arm Position Error" },
            { "240", "Wafer Protrution Detected Error" },
            { "241", "Main Air Error" },
            { "243", "No Product on Loadport Error" },
            { "246", "Cassette Loading Error" },
            { "247", "Cassette Unloading Error" },
            { "248", "Safty bar Detected Error" },
            { "E02", "Loadport Busy Error" }
        };
        string GetErrorString(string sCode)
        {
            for (int n = 0; n < m_sErrorMsgs.Length / 2; n++)
            {
                if (m_sErrorMsgs[n, 0] == sCode) return m_sErrorMsgs[n, 1];
            }
            return "Can't Find Error Massage !!";
        }
        #endregion

        #region Protocol
        enum eCmd
        {
            None,
            Home,
            ResetCPU,
            Load,
            MappingLoad,
            Unload,
            GetMapData,
        };
        eCmd m_eSendCmd = eCmd.None;

        Dictionary<eCmd, string> m_dicCmd = new Dictionary<eCmd, string>();
        void InitCmd()
        {
            m_dicCmd.Add(eCmd.Home, "ORG");
            m_dicCmd.Add(eCmd.ResetCPU, "DRT");
            m_dicCmd.Add(eCmd.Load, "RLNP");
            m_dicCmd.Add(eCmd.MappingLoad, "RLMP");
            m_dicCmd.Add(eCmd.Unload, "RUNP");
            m_dicCmd.Add(eCmd.GetMapData, "MLD");
            m_dicCmd.Add(eCmd.None, "");
        }
        #endregion

        #region RS232
        private void M_rs232_OnReceive(string sRead)
        {
            string[] sReads = sRead.Split(' ');
            m_log.Info(" <-- Recv] " + sRead);
            if (sRead == "MLD EOD") return;
            Run(ReplyCmd(sReads));
            if (p_sInfo != "OK") p_eState = eState.Error;
            m_eSendCmd = eCmd.None;
        }

        string m_sLastCmd = "";
        string WriteCmd(eCmd cmd, params object[] objs)
        {
            if (EQ.IsStop()) return "EQ Stop";
            Thread.Sleep(10);
            if (m_eSendCmd != eCmd.None)
            {
                if (EQ.IsStop()) return "EQ Stop";
                Thread.Sleep(200);
                if (m_eSendCmd != eCmd.None) return "RS232 Communication Error !!";
            }
            if (EQ.IsStop()) return "EQ Stop";
            string str = m_dicCmd[cmd];
            for (int n = 0; n < objs.Length; n++) str += "," + objs[n].ToString();
            m_log.Info(" [ Send --> " + str);
            m_sLastCmd = str;
            m_eSendCmd = cmd;
            m_rs232.Send(str);
            return "OK";
        }

        string WaitReply(int secTimeout)
        {
            try
            {
                if (EQ.IsStop()) return "EQ Stop";
                int msDelay = 1000 * secTimeout;
                int ms10 = 0;
                if (m_rs232.p_bConnect == false) return m_sLastCmd + " RS232 not Connect !!";
                while (m_eSendCmd != eCmd.None)
                {
                    if (EQ.IsStop()) return "EQ Stop";
                    Thread.Sleep(10);
                    ms10 += 10;
                    if (ms10 > msDelay) 
                        return m_sLastCmd + " Has no Answer !!";
                }
                return "OK";
            }
            finally { m_eSendCmd = eCmd.None; }
        }
        #endregion

        #region Timeout
        int m_secRS232 = 2;
        int m_secHome = 40;
        int m_secMotion = 20;
        void RunTimeoutTree(Tree tree)
        {
            m_secRS232 = tree.Set(m_secRS232, m_secRS232, "RS232", "Timeout (sec)");
            m_secHome = tree.Set(m_secHome, m_secHome, "Home", "Timeout (sec)");
            m_secMotion = tree.Set(m_secMotion, m_secMotion, "Motion", "Timeout (sec)");
        }
        #endregion

        #region RS232 Commend
        string CmdHome()
        {
            if (Run(WriteCmd(eCmd.Home))) return p_sInfo;
            if (Run(WaitReply(m_secHome))) return p_sInfo;
            return "OK";
        }

        string CmdResetCPU()
        {
            if (Run(WriteCmd(eCmd.ResetCPU))) return p_sInfo;
            if (Run(WaitReply(m_secRS232))) return p_sInfo;
            return "OK";    
        }

        string CmdLoad(bool bMapping = true)
        {
            // if (IsLock()) return m_id + " Lock by WTR";
            eCmd cmd = bMapping ? eCmd.MappingLoad : eCmd.Load;
            if (Run(WriteCmd(cmd))) return p_sInfo;
            if (Run(WaitReply(m_secMotion))) return p_sInfo;
            return "OK";
        }

        string CmdUnload()
        {
            if (IsLock()) return p_id + " Lock by WTR";
            if (Run(WriteCmd(eCmd.Unload))) return p_sInfo;
            if (Run(WaitReply(m_secMotion))) return p_sInfo;
            return "OK";
        }

        string CmdGetMapData()
        {
            if (Run(WriteCmd(eCmd.GetMapData, "1"))) return p_sInfo;
            if (Run(WaitReply(m_secRS232))) return p_sInfo;
            return "OK";
        }

        string ReplyCmd(string[] sMsgs)
        {
            if (sMsgs.Length < 1) return p_id + " Reply Message Too Short";
            //if (m_dicCmd[m_eSendCmd] != sMsgs[0]) return p_id + " Commnuication Error : " + m_sLastCmd + " != " + sMsgs[0];
            if (m_dicCmd[eCmd.GetMapData] == sMsgs[0])
            {
                if (Run(CheckReplyError(2, sMsgs))) return p_sInfo;
                if (Run(SetLoadportMapData(sMsgs[1]))) return p_sInfo;
            }
            else
            {
                if (Run(CheckReplyError(1, sMsgs))) return p_sInfo;
            }
            return "OK";
        }

        string SetLoadportMapData(string sMap)
        {
            bool bUndefined = false;
            List<GemSlotBase.eState> aSlot = new List<GemSlotBase.eState>();
            foreach (char ch in sMap)
            {
                switch (ch)
                {
                    case '0': aSlot.Add(GemSlotBase.eState.Empty); break;
                    case '1': aSlot.Add(GemSlotBase.eState.Exist); break;
                    case 'D': aSlot.Add(GemSlotBase.eState.Double); break;
                    case 'C': aSlot.Add(GemSlotBase.eState.Cross); break;
                    default:
                        bUndefined = true;
                        aSlot.Add(GemSlotBase.eState.Undefined);
                        break;
                }
            }
            p_infoCarrier.SetMapData(aSlot);
            return bUndefined ? "Undifined MapData" : "OK";
        }

        string CheckReplyError(int nError, string[] sMsgs)
        {
            if (sMsgs.Length > nError) return p_id + " Reply Error : " + m_sLastCmd + " -> " + GetErrorString(sMsgs[nError]);
            if (sMsgs.Length < nError) return p_id + " Short Reply Massage : " + m_sLastCmd;
            return "OK";
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
            Run(CmdResetCPU());
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
                if (Run(CmdResetCPU())) return p_sInfo;
                if (Run(CmdHome())) return p_sInfo;
            }
            p_eState = eState.Ready;
            p_infoCarrier.p_eState = InfoCarrier.eState.Empty;
            p_infoCarrier.AfterHome();
            return "OK";
        }
        #endregion

        #region StateReady
        public override string StateReady()
        {
            CheckPlaced();
/*            if (m_infoCarrier.m_bReqReadCarrierID)
            {
                m_infoCarrier.m_bReqReadCarrierID = false;
                StartRun(m_runReadPodID);
            } */
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
        #endregion

        public InfoCarrier p_infoCarrier { get; set; }
        public Loadport_RND(string id, IEngineer engineer, bool bEnableWaferSize, bool bEnableWaferCount)
        {
            p_bLock = false;
            InitCmd();
            p_id = id;
            p_infoCarrier = new InfoCarrier(this, id, engineer, bEnableWaferSize, bEnableWaferCount);
            m_aTool.Add(p_infoCarrier);
            InitBase(id, engineer);
            InitGAF();
            if (m_gem != null) m_gem.OnGemRemoteCommand += M_gem_OnRemoteCommand;
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        private void M_gem_OnRemoteCommand(string sCmd, Dictionary<string, string> dicParam, long[] pnResult)
        {
        }

        #region ModuleRun
        ModuleRunBase m_runDocking;
        ModuleRunBase m_runUndocking;
        protected override void InitModuleRuns()
        {
            m_runDocking = AddModuleRunList(new Run_Docking(this), false, "Docking Carrier to Work Position");
            m_runUndocking = AddModuleRunList(new Run_Undocking(this), false, "Undocking Carrier from Work Position");
        }

        public class Run_Docking : ModuleRunBase
        {
            Loadport_RND m_module;
            InfoCarrier m_infoCarrier;
            public Run_Docking(Loadport_RND module)
            {
                m_module = module;
                m_infoCarrier = module.p_infoCarrier;
                InitModuleRun(module);
            }

            bool m_bMapping = true;
            public override ModuleRunBase Clone()
            {
                Run_Docking run = new Run_Docking(m_module);
                run.m_bMapping = m_bMapping;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bMapping = tree.Set(m_bMapping, m_bMapping, "Mapping", "Wafer Mapping When Loading", bVisible);
            }

            public override string Run()
            {
                if (m_infoCarrier.p_eState != InfoCarrier.eState.Placed) return p_id + " RunLoad, InfoCarrier.p_eState = " + m_infoCarrier.p_eState.ToString();
                if (m_module.Run(m_module.CmdLoad(m_bMapping))) return p_sInfo;
                if (m_module.Run(m_module.CmdGetMapData())) return p_sInfo;
                m_infoCarrier.p_eState = InfoCarrier.eState.Dock;
                m_module.m_ceidDocking.Send(); 
                return "OK";
            }
        }

        public class Run_Undocking : ModuleRunBase
        {
            Loadport_RND m_module;
            InfoCarrier m_infoCarrier;
            public Run_Undocking(Loadport_RND module)
            {
                m_module = module;
                m_infoCarrier = module.p_infoCarrier;
                InitModuleRun(module);
            }

            string m_sUndocking = "Undocking";
            public override ModuleRunBase Clone()
            {
                Run_Undocking run = new Run_Undocking(m_module);
                run.m_sUndocking = m_sUndocking;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sUndocking = tree.Set(m_sUndocking, m_sUndocking, "Undocking", "Carrier Undocking", bVisible, true);
            }

            public override string Run()
            {
                if (m_infoCarrier.p_eState != InfoCarrier.eState.Dock) return p_id + " RunUnload, InfoCarrier.p_eState = " + m_infoCarrier.p_eState.ToString();
                if (m_module.Run(m_module.CmdUnload())) return p_sInfo;
                m_infoCarrier.p_eState = InfoCarrier.eState.Placed;
                m_module.m_ceidUnDocking.Send(); 
                return "OK";
            }
        }
        #endregion
    }
}
