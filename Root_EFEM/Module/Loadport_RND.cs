using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using RootTools.OHT.Semi;
using RootTools.OHTNew;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;
using static RootTools.Gem.XGem.XGem;

namespace Root_EFEM.Module
{
    public class Loadport_RND : ModuleBase, IWTRChild, ILoadport
    {
        #region ToolBox
        public DIO_I m_diPlaced;
        //OHT _OHT;
        //public OHT m_OHTNew
        //{
        //    get { return _OHT; }
        //    set
        //    {
        //        _OHT = value;
        //        OnPropertyChanged();
        //    }
        //}
        private OHT_Semi m_OHT;
        //OHT _OHT;
        public OHT_Semi m_OHTsemi
        {
            get
            {
                return m_OHT;
            }
            set
            {
                m_OHT = value;
                OnPropertyChanged();
            }
        }

        public DIO_I p_diPlaced
        {
            get
            {
                return m_diPlaced;
            }
            set
            {
                m_diPlaced = value;
            }
        }
        DIO_I m_diPresent;
        public DIO_I p_diPresent
        {
            get
            {
                return m_diPresent;
            }
            set
            {
                m_diPresent = value;
            }
        }
        DIO_I m_diLoad;
        public DIO_I p_diLoad
        {
            get
            {
                return m_diLoad;
            }
            set
            {
                m_diLoad = value;
            }
        }
        DIO_I m_diUnload;
        public DIO_I p_diUnload
        {
            get
            {
                return m_diUnload;
            }
            set
            {
                m_diUnload = value;
            }
        }
        DIO_I m_diDoorOpen;
        public DIO_I p_diDoorOpen
        {
            get
            {
                return m_diDoorOpen;
            }
            set
            {
                m_diDoorOpen = value;
            }
        }
        DIO_I m_diDocked;
        public DIO_I p_diDocked
        {
            get
            {
                return m_diDocked;
            }
            set
            {
                m_diDocked = value;
            }
        }
        RS232 m_rs232;
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

        //OHT m_OHT;
        ALID m_alid_WaferExist;
        public void SetAlarm()
        {
           // m_alid_WaferExist.Run(true, "Aligner Wafer Exist Error");
        }
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetDIO(ref m_diPlaced, this, "Place");
            p_sInfo = m_toolBox.GetDIO(ref m_diPresent, this, "Present");
            p_sInfo = m_toolBox.GetDIO(ref m_diLoad, this, "Load");
            p_sInfo = m_toolBox.GetDIO(ref m_diUnload, this, "Unload");
            p_sInfo = m_toolBox.GetDIO(ref m_diDoorOpen, this, "DoorOpen");
            p_sInfo = m_toolBox.GetDIO(ref m_diDocked, this, "Docked");
            p_sInfo = m_toolBox.GetComm(ref m_rs232, this, "RS232");
            p_sInfo = m_toolBox.GetOHT(ref m_OHT, this, p_infoCarrier, "OHT");
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
            if (m_diPlaced.p_bIn != p_diPresent.p_bIn)
                present = GemCarrierBase.ePresent.Unknown;
            else
                present = m_diPlaced.p_bIn ? GemCarrierBase.ePresent.Exist : GemCarrierBase.ePresent.Empty;
            if (p_infoCarrier.CheckPlaced(present) != "OK")
                m_alidPlaced.Run(true, "Placed Sensor Remain Checked while Pod State = " + p_infoCarrier.p_eState);
            switch (p_infoCarrier.p_ePresentSensor)
            {
                case GemCarrierBase.ePresent.Empty:
                    m_svidPlaced.p_value = false;
                    break;
                case GemCarrierBase.ePresent.Exist:
                    m_svidPlaced.p_value = true;
                    break;
            }
            return m_svidPlaced.p_value;
        }
        #endregion

        #region IWTRChild
        public bool p_bLock
        {
            get; set;
        }

        bool IsLock()
        {
            for (int n = 0; n < 10; n++)
            {
                if (p_bLock == false)
                    return false;
                Thread.Sleep(100);
            }
            return true;
        }

        public List<string> p_asChildSlot
        {
            get
            {
                return p_infoCarrier.p_asGemSlot;
            }
        }

        public InfoWafer p_infoWafer
        {
            get; set;
        }

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


        public void CopySlotInfo(InfoWafer infoWafer, GemSlotBase gemSlot)
        {
            infoWafer.p_sRecipe = gemSlot.p_sRecipe;
            infoWafer.p_sCarrierID = gemSlot.p_sCarrierID;
            infoWafer.p_sLocID = gemSlot.p_sLocID;
            infoWafer.p_sLotID = gemSlot.p_sLotID;
            infoWafer.p_eState = gemSlot.p_eState;
            infoWafer.p_sSlotID = gemSlot.p_sSlotID;
        }

        public string IsGetOK(int nID)
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (!m_diDoorOpen.p_bIn)
                return p_id + "Door Close Error";
            return p_infoCarrier.IsGetOK(nID);
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (!m_diDoorOpen.p_bIn)
                return p_id + "Door Close Error";
            return p_infoCarrier.IsPutOK(nID);
        }

        public string BeforeGet(int nID)
        {
            InfoWafer wafer = GetInfoWafer(nID);
            if (wafer == null)
                return p_id + nID.ToString("00") + " BeforeGet : InfoWafer = null";
            if (!m_diDoorOpen.p_bIn)
                return "Door Not Opened";

            MarsLogManager marsLogManager = MarsLogManager.Instance;
            marsLogManager.ChangeMaterial(EQ.p_nRunLP, wafer.m_nSlot + 1, wafer.p_sLotID, wafer.p_sCarrierID, wafer.p_sRecipe);
            if(wafer.p_eWaferOrder == InfoWafer.eWaferOrder.FirstWafer || wafer.p_eWaferOrder == InfoWafer.eWaferOrder.FirstLastWafer)
                MarsLogManager.Instance.WriteLEH(EQ.p_nRunLP, this.p_id, SSLNet.LEH_EVENTID.PROCESS_JOB_START, MarsLogManager.Instance.m_flowData, MarsLogManager.Instance.m_dataFormatter);
            return IsRunOK();
        }

        public string BeforePut(int nID)
        {
            if (GetInfoWafer(nID) != null)
                return p_id + nID.ToString("00") + " BeforePut : InfoWafer != null";
            if (!m_diDoorOpen.p_bIn)
                return "Door Not Opened";
            return IsRunOK();
        }

        public string AfterGet(int nID)
        {
            InfoWafer wafer = GetInfoWafer(nID);
            wafer.p_sInspectionID = wafer.p_sLotID + wafer.p_sWaferID +DateTime.Now.ToString("yyyyMMddhhmmss");

            p_infoCarrier.m_aGemSlot[nID].p_eState = GemSlotBase.eState.Run;

            return IsRunOK();
        }

        public string AfterPut(int nID)
        {
            p_infoCarrier.m_aGemSlot[nID].p_eState = GemSlotBase.eState.Done;
            return IsRunOK();
        }

        public bool IsWaferExist(int nID = 0)
        {
            switch (p_infoCarrier.p_eState)
            {
                case InfoCarrier.eState.Empty:
                    return false;
                case InfoCarrier.eState.Placed:
                    return false;
            }
            return (p_infoCarrier.GetInfoWafer(nID) != null);
        }

        string IsRunOK()
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
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
                if (m_sErrorMsgs[n, 0] == sCode)
                    return m_sErrorMsgs[n, 1];
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
            if (sRead == "MLD EOD")
                return;
            Run(ReplyCmd(sReads));
            if (p_sInfo != "OK")
                p_eState = eState.Error;
            m_eSendCmd = eCmd.None;
        }

        string m_sLastCmd = "";
        string WriteCmd(eCmd cmd, params object[] objs)
        {
            if (EQ.IsStop())
                return "EQ Stop";
            Thread.Sleep(10);
            if (m_eSendCmd != eCmd.None)
            {
                if (EQ.IsStop())
                    return "EQ Stop";
                Thread.Sleep(200);
                if (m_eSendCmd != eCmd.None)
                    return "RS232 Communication Error !!";
            }
            if (EQ.IsStop())
                return "EQ Stop";
            string str = m_dicCmd[cmd];
            for (int n = 0; n < objs.Length; n++)
                str += "," + objs[n].ToString();
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
                if (EQ.IsStop())
                    return "EQ Stop";
                int msDelay = 1000 * secTimeout;
                int ms10 = 0;
                if (m_rs232.p_bConnect == false)
                    return m_sLastCmd + " RS232 not Connect !!";
                while (m_eSendCmd != eCmd.None)
                {
                    if (EQ.IsStop())
                        return "EQ Stop";
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
        //public int m_secHome = 40;
        int _secHome = 40;
        public int p_secHome
        {
            get { return _secHome; }
            set
            {
                if (_secHome == value) return;
                _secHome = value;
                OnPropertyChanged();
            }
        }
        int m_secMotion = 20;
        void RunTimeoutTree(Tree tree)
        {
            m_secRS232 = tree.Set(m_secRS232, m_secRS232, "RS232", "Timeout (sec)");
            p_secHome = tree.Set(p_secHome, p_secHome, "Home", "Timeout (sec)");
            m_secMotion = tree.Set(m_secMotion, m_secMotion, "Motion", "Timeout (sec)");
        }
        #endregion

        #region RS232 Commend
        string CmdHome()
        {
            if (Run(WriteCmd(eCmd.Home)))
                return p_sInfo;
            if (Run(WaitReply(p_secHome)))
                return p_sInfo;
            return "OK";
        }

        string CmdResetCPU()
        {
            if (Run(WriteCmd(eCmd.ResetCPU)))
                return p_sInfo;
            if (Run(WaitReply(m_secRS232)))
                return p_sInfo;
            return "OK";
        }

        string CmdLoad(bool bMapping = true)
        {
            // if (IsLock()) return m_id + " Lock by WTR";
            eCmd cmd = bMapping ? eCmd.MappingLoad : eCmd.Load;
            if (Run(WriteCmd(cmd)))
                return p_sInfo;
            if (Run(WaitReply(m_secMotion)))
                return p_sInfo;
            return "OK";
        }

        string CmdUnload()
        {
            if (IsLock())
                return p_id + " Lock by WTR";
            if (Run(WriteCmd(eCmd.Unload)))
                return p_sInfo;
            if (Run(WaitReply(m_secMotion)))
                return p_sInfo;
            return "OK";
        }

        string CmdGetMapData()
        {
            if (Run(WriteCmd(eCmd.GetMapData, "1")))
                return p_sInfo;
            if (Run(WaitReply(m_secRS232)))
                return p_sInfo;
            return "OK";
        }

        string ReplyCmd(string[] sMsgs)
        {
            if (sMsgs.Length < 1)
                return p_id + " Reply Message Too Short";
            //if (m_dicCmd[m_eSendCmd] != sMsgs[0]) return p_id + " Commnuication Error : " + m_sLastCmd + " != " + sMsgs[0];
            if (m_dicCmd[eCmd.GetMapData] == sMsgs[0])
            {
                if (Run(CheckReplyError(2, sMsgs)))
                {
                    m_alidLoportReplyError.Run(true, p_sInfo);
                    return p_sInfo;
                }
                if (Run(SetLoadportMapData(sMsgs[1])))
                {   
                    return p_sInfo;
                }
            }
            else
            {
                if (Run(CheckReplyError(1, sMsgs)))
                {
                    m_alidLoportReplyError.Run(true, p_sInfo);
                    return p_sInfo;
                }
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
                    case '0':
                        aSlot.Add(GemSlotBase.eState.Empty);
                        break;
                    case '1':
                        aSlot.Add(GemSlotBase.eState.Exist);
                        break;
                    case 'D':
                        aSlot.Add(GemSlotBase.eState.Double);
                        m_alidDoubleWafer.Run(true, "Wafer Double Check");
                        break;
                    case 'C':
                        aSlot.Add(GemSlotBase.eState.Cross);
                        m_alidCrossWafer.Run(true, "Wafer Cross Check");
                        break;
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
            if (sMsgs.Length > nError)
                return p_id + " Reply Error : " + m_sLastCmd + " -> " + GetErrorString(sMsgs[nError]);
            if (sMsgs.Length < nError)
                return p_id + " Short Reply Massage : " + m_sLastCmd;
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
                if (Run(CmdResetCPU()))
                    return p_sInfo;
                if (Run(CmdHome()))
                    return p_sInfo;
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
            bool bUseXGem = m_engineer.p_bUseXGem;
            if (p_infoCarrier.m_bReqLoad)
            {
                p_infoCarrier.m_bReqLoad = false;
                if (bUseXGem) StartRun(m_runDocking);
            }
            if (p_infoCarrier.m_bReqGem)
            {
                p_infoCarrier.m_bReqGem = false;
                StartRun(m_runGem);
            }    
            if (p_infoCarrier.m_bReqUnload && p_infoCarrier.p_eState == InfoCarrier.eState.Dock)
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
        public CEID m_ceidUnloadReq;
        ALID m_alidPlaced;
        ALID m_alidDoubleWafer;
        ALID m_alidCrossWafer;
        ALID m_alidLoportReplyError;

        void InitGAF()
        {
            m_svidPlaced = m_gaf.GetSVID(this, "Placed");
            m_ceidDocking = m_gaf.GetCEID(this, "Docking");
            m_ceidUnDocking = m_gaf.GetCEID(this, "UnDocking");
            m_ceidUnloadReq = m_gaf.GetCEID(this, "Unload Request");
            m_alidPlaced = m_gaf.GetALID(this, "Placed Sensor Error", "Placed & Plesent Sensor Should be Checked");
            m_alidCrossWafer = m_gaf.GetALID(this, "Cross Wafer Check Error", "Cross Wafer Checked");
            m_alidDoubleWafer = m_gaf.GetALID(this, "Double Wafer Check Error", "Double Wafer Checked");
            m_alidLoportReplyError = m_gaf.GetALID(this, "LoadPort Reply Error", "Loadport Reply Error");
        }
        #endregion

        #region ILoadport
        public string RunDocking()
        {
            if (p_infoCarrier.p_eState == InfoCarrier.eState.Dock)
                return "OK";
            ModuleRunBase run = m_runDocking.Clone();
            StartRun(run);
            while (IsBusy() && (EQ.IsStop() == false))
                Thread.Sleep(10);
            return EQ.IsStop() ? "EQ Stop" : "OK";
        }

        public string RunUndocking()
        {
            if (p_infoCarrier.p_eState != InfoCarrier.eState.Dock)
                return "OK";
            ModuleRunBase run = m_runUndocking.Clone();
            StartRun(run);
            while (IsBusy() && (EQ.IsStop() == false))
                Thread.Sleep(10);
            return EQ.IsStop() ? "EQ Stop" : "OK";
        }

        public bool p_bPlaced
        {
            get
            {
                //return true;
                return m_diPlaced.p_bIn;
            }
        }
        public bool p_bPresent
        {
            get
            {
                //return true;
                return p_diPresent.p_bIn;
            }
        }
        #endregion

        public InfoCarrier p_infoCarrier
        {
            get; set;
        }
        //public IRFID m_rfid;
        IRFID _rfid;
        public IRFID m_rfid
        {
            get { return _rfid; }
            set
            {
                _rfid = value;
            }
        }

        public Loadport_RND(string id, IEngineer engineer, bool bEnableWaferSize, bool bEnableWaferCount)
        {
            p_bLock = false;
            InitCmd();
            p_id = id;
            p_infoCarrier = new InfoCarrier(this, id, engineer, bEnableWaferSize, bEnableWaferCount);
            if (id == "LoadportA")
                p_infoCarrier.p_sLocID = "LP1";
            else if (id == "LoadportB")
                p_infoCarrier.p_sLocID = "LP2";
            m_aTool.Add(p_infoCarrier);
            InitBase(id, engineer);
            InitGAF();
            if (m_gem != null)
                m_gem.OnGemRemoteCommand += M_gem_OnRemoteCommand;
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        private void M_gem_OnRemoteCommand(string sCmd, Dictionary<string, string> dicParam, long[] pnResult)
        {
        }

        public override bool IsExistCarrier()
        {
            if (m_diPlaced.p_bIn && p_diPresent.p_bIn)
                return true;
            else
                return false;
        }

        public override bool IsPlacement()
        {
            return m_diPlaced.p_bIn;
        }

        public override bool IsPresent()
        {
            return p_diPresent.p_bIn;
        }

        #region ModuleRun
        ModuleRunBase m_runDocking;
        ModuleRunBase m_runGem;
        ModuleRunBase m_runUndocking;

        public ModuleRunBase GetModuleRunUndocking()
        {
            return m_runUndocking;
        }
        public ModuleRunBase GetModuleRunDocking()
        {
            return m_runDocking;
        }
        public ModuleRunBase GetModuleRunGem()
        {
            return m_runGem;
        }

        protected override void InitModuleRuns()
        {
            m_runDocking = AddModuleRunList(new Run_Docking(this), false, "Docking Carrier to Work Position");
            m_runGem = AddModuleRunList(new Run_GemProcess(this), false, "Gem Slot Process Start");
            m_runUndocking = AddModuleRunList(new Run_Undocking(this), false, "Undocking Carrier from Work Position");
        }

        public class Run_Docking : ModuleRunBase
        {
            Loadport_RND m_module;
            InfoCarrier m_infoCarrier;
            //RFID_Brooks m_rfid;
            public Run_Docking(Loadport_RND module)
            {
                m_module = module;
                m_infoCarrier = module.p_infoCarrier;
                //m_rfid = (RFID_Brooks)module.m_rfid;
                InitModuleRun(module);
            }

            string m_sSimulCarrierID = "CarrierID";
            string m_sSimulSlotmap = "";
            bool m_bMapping = true;
            bool m_bReadRFID = true;
            public override ModuleRunBase Clone()
            {
                Run_Docking run = new Run_Docking(m_module);
                run.m_sSimulCarrierID = m_sSimulCarrierID;
                run.m_sSimulSlotmap = m_sSimulSlotmap;
                run.m_bMapping = m_bMapping;
                run.m_bReadRFID = m_bReadRFID;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sSimulCarrierID = tree.Set(m_sSimulCarrierID, m_sSimulCarrierID, "Simulation CarrierID", "CarrierID When p_bSimulation", bVisible && EQ.p_bSimulate);
                m_sSimulSlotmap = tree.Set(m_sSimulSlotmap, m_sSimulSlotmap, "Simulation Slotmap", "Slotmap When p_bSimulation", bVisible && EQ.p_bSimulate);
                m_bMapping = tree.Set(m_bMapping, m_bMapping, "Mapping", "Wafer Mapping When Loading", bVisible);
                m_bReadRFID = tree.Set(m_bReadRFID, m_bReadRFID, "Read RFID", "Read RFID", bVisible);
            }

            public override string Run()
            {
                MarsLogManager marsLogManager = MarsLogManager.Instance;
                string sResult = "OK";
               
                if (EQ.p_bSimulate)
                {
                    m_infoCarrier.p_ePresentSensor = GemCarrierBase.ePresent.Exist;
                    m_infoCarrier.p_sCarrierID = m_sSimulCarrierID;
                }
                else
                {
                    if (m_infoCarrier.p_eState != InfoCarrier.eState.Placed)
                        return p_id + " RunLoad, InfoCarrier.p_eState = " + m_infoCarrier.p_eState.ToString();
                    if (m_infoCarrier.p_eState == InfoCarrier.eState.Dock)
                        return p_id + " RunLoad, InfoCarrier.p_eState = " + m_infoCarrier.p_eState.ToString();

                    if (m_bReadRFID)
                    {
                        sResult = m_module.m_rfid.ReadRFID();
                        m_infoCarrier.p_sCarrierID = (sResult == "OK") ? m_module.m_rfid.m_sReadID : "";
                    }
                    else
                        m_infoCarrier.p_sCarrierID = m_sSimulCarrierID;
                }
                if (sResult == "OK")
                    m_infoCarrier.SendCarrierID(m_infoCarrier.p_sCarrierID);
                else
                    return p_sInfo + " SendCarrierID : " + m_infoCarrier.p_sCarrierID;


                marsLogManager.WriteFNC(EQ.p_nRunLP, m_module.p_id, "CarrierLoad", SSLNet.STATUS.START, type:SSLNet.MATERIAL_TYPE.FOUP);

                while (m_infoCarrier.p_eStateCarrierID != GemCarrierBase.eGemState.VerificationOK)
                {
                    Thread.Sleep(10);
                    if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                    if (m_infoCarrier.p_eStateCarrierID == GemCarrierBase.eGemState.VerificationFailed)
                        return p_sInfo + " infoCarrier.p_eStateCarrierID = " + m_infoCarrier.p_eStateCarrierID.ToString();
                }
                if (m_infoCarrier.p_eTransfer != GemCarrierBase.eTransfer.TransferBlocked)
                    return p_sInfo + " infoCarrier.p_eTransfer = " + m_infoCarrier.p_eTransfer.ToString();

                if (EQ.p_bSimulate)
                {
                    InfoCarrier infoCarrier = m_infoCarrier;
                    List<GemSlotBase.eState> aSlot = new List<GemSlotBase.eState>();
                    string sMap = m_sSimulSlotmap;
                    foreach (char ch in sMap)
                    {
                        switch (ch)
                        {
                            case '0':
                                aSlot.Add(GemSlotBase.eState.Empty);
                                break;
                            case '1':
                                aSlot.Add(GemSlotBase.eState.Exist);
                                break;
                            case 'D':
                                aSlot.Add(GemSlotBase.eState.Double);
                                break;
                            case 'C':
                                aSlot.Add(GemSlotBase.eState.Cross);
                                break;
                            default:
                                aSlot.Add(GemSlotBase.eState.Undefined);
                                break;
                        }
                    }
                    infoCarrier.SetMapData(aSlot);
                }
                else
                {
                   
                    if (m_module.Run(m_module.CmdLoad(m_bMapping)))
                        return p_sInfo;
                    if (m_module.Run(m_module.CmdGetMapData()))
                        return p_sInfo;
                    m_infoCarrier.p_eState = InfoCarrier.eState.Dock;
                }
                m_infoCarrier.SendSlotMap();
                while (m_infoCarrier.p_eStateSlotMap != GemCarrierBase.eGemState.VerificationOK)
                {
                    Thread.Sleep(10);
                    if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                    if (m_infoCarrier.p_eStateSlotMap == GemCarrierBase.eGemState.VerificationFailed)
                        return p_sInfo + " infoCarrier.p_eStateSlotMap = " + m_infoCarrier.p_eStateSlotMap.ToString();
                }

                RnRData rnrData = m_module.m_engineer.ClassHandler().GetRnRData();
                int firstIdx = -1;
                int lastIdx = -1;
                
                foreach (int sel in rnrData.SelectSlot)
                {
                    m_infoCarrier.m_aGemSlot[sel].p_eState = GemSlotBase.eState.Select;
                    m_infoCarrier.m_aGemSlot[sel].p_sCarrierID = rnrData.CarrierID;
                    m_infoCarrier.m_aGemSlot[sel].p_sLotID = rnrData.LotID;
                    if (firstIdx == -1)
                        firstIdx = sel;

                    m_module.CopySlotInfo(m_infoCarrier.m_aInfoWafer[sel], m_infoCarrier.m_aGemSlot[sel]);

                    lastIdx = sel;
                }
                if(rnrData.SelectSlot.Count != 0)
                {
                    if (firstIdx == lastIdx)
                        m_infoCarrier.m_aInfoWafer[firstIdx].p_eWaferOrder = InfoWafer.eWaferOrder.FirstLastWafer;
                    else
                    {
                        m_infoCarrier.m_aInfoWafer[firstIdx].p_eWaferOrder = InfoWafer.eWaferOrder.FirstWafer;
                        m_infoCarrier.m_aInfoWafer[lastIdx].p_eWaferOrder = InfoWafer.eWaferOrder.LastWafer;
                    }
                    m_infoCarrier.p_sCarrierID = rnrData.CarrierID;
                    m_infoCarrier.p_sLotID = rnrData.LotID;
                }


                SSLNet.DataFormatter dataformatter = new SSLNet.DataFormatter();
                dataformatter.AddData("MapID", m_infoCarrier.GetMapData());
                marsLogManager.WriteFNC(EQ.p_nRunLP, m_module.p_id, "CarrierLoad", SSLNet.STATUS.END, SSLNet.MATERIAL_TYPE.FOUP, dataformatter);
                dataformatter.ClearData();
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
                MarsLogManager marsLogManager = MarsLogManager.Instance;
                string sResult = "OK";
                bool bUseXGem = m_module.m_engineer.p_bUseXGem;
                IGem m_gem = m_module.m_gem;
                if (!EQ.p_bSimulate)
                {
                    if (m_infoCarrier.p_eState != InfoCarrier.eState.Dock)
                        return p_id + " RunUnload, InfoCarrier.p_eState = " + m_infoCarrier.p_eState.ToString();
                }

                marsLogManager.WriteFNC(EQ.p_nRunLP, m_module.p_id, "CarrierUnload", SSLNet.STATUS.START, type: SSLNet.MATERIAL_TYPE.FOUP);

                if (bUseXGem && !m_gem.p_bOffline)
                {
                    while (m_infoCarrier.p_eAccess != GemCarrierBase.eAccess.InAccessed)
                    {
                        Thread.Sleep(10);
                        if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                    }
                    if (m_gem.p_cjRun == null) return p_sInfo;
                    foreach(GemPJ pj in m_gem.p_cjRun.m_aPJ)
                    {
                        m_gem.SendPJComplete(pj.m_sPJobID);
                        Thread.Sleep(100);
                    }
                    while(m_gem.p_cjRun.p_eState != GemCJ.eState.Completed)
                    {
                        Thread.Sleep(10);
                        if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                    }
                }
                if (!EQ.p_bSimulate)
                {
                    if (m_module.Run(m_module.CmdUnload()))
                        return p_sInfo;
                }
                m_infoCarrier.p_eState = InfoCarrier.eState.Placed;
                m_infoCarrier.p_eReqTransfer = GemCarrierBase.eTransfer.ReadyToUnload;
                marsLogManager.WriteFNC(EQ.p_nRunLP, m_module.p_id, "CarrierUnload", SSLNet.STATUS.END, SSLNet.MATERIAL_TYPE.FOUP);
                marsLogManager.WriteLEH(EQ.p_nRunLP, m_module.p_id, SSLNet.LEH_EVENTID.CARRIER_UNLOAD, MarsLogManager.Instance.m_flowData);
                //m_module.m_ceidUnDocking.Send();
                return sResult;
            }
        }

        public class Run_GemProcess : ModuleRunBase
        {
            Loadport_RND m_module;
            InfoCarrier m_infoCarrier;
            public Run_GemProcess(Loadport_RND module)
            {
                m_module = module;
                m_infoCarrier = module.p_infoCarrier;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_GemProcess run = new Run_GemProcess(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                //m_sUndocking = tree.Set(m_sUndocking, m_sUndocking, "Undocking", "Carrier Undocking", bVisible, true);
            }

            public override string Run()
            {
                string sResult = "OK";
                IGem m_gem = m_module.m_gem;
                if (m_gem == null || m_gem.p_eControl != eControl.ONLINEREMOTE) return p_id + " is not Gem Ready.";
                if (!EQ.p_bSimulate)
                {
                    if (m_infoCarrier.p_eState != InfoCarrier.eState.Dock)
                        return p_id + " RunUnload, InfoCarrier.p_eState = " + m_infoCarrier.p_eState.ToString();
                }
                
                while (m_gem.p_cjRun == null) 
                { 
                    Thread.Sleep(10);
                    if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                }
                //foreach (GemPJ pj in m_gem.p_cjRun.m_aPJ)
                //{
                //    while(pj.p_eState != GemPJ.eState.Processing && m_gem.p_cjRun.p_eState != GemCJ.eState.Excuting)
                //    {
                //        Thread.Sleep(10);
                //        if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                //    }
                //    break;
                //}
                while(m_infoCarrier.p_eAccess != GemCarrierBase.eAccess.InAccessed)
                {
                    Thread.Sleep(10);
                    if (EQ.p_bStop) return p_sInfo + "EQ Stop";
                }

                int firstIdx = -1;
                int lastIdx = -1;
                for (int i=0; i<m_infoCarrier.m_aGemSlot.Count; i++)
                {
                    if (m_infoCarrier.m_aGemSlot[i].p_eState == GemSlotBase.eState.Select)
                    {
                        if (firstIdx == -1)
                            firstIdx = i;

                        m_module.CopySlotInfo(m_infoCarrier.m_aInfoWafer[i], m_infoCarrier.m_aGemSlot[i]);
                        m_infoCarrier.StartProcess(m_infoCarrier.m_aGemSlot[i].p_id);
                        lastIdx = i;
                    }
                }
                if (firstIdx == lastIdx)
                    m_infoCarrier.m_aInfoWafer[firstIdx].p_eWaferOrder = InfoWafer.eWaferOrder.FirstLastWafer;
                else
                {
                    m_infoCarrier.m_aInfoWafer[firstIdx].p_eWaferOrder = InfoWafer.eWaferOrder.FirstWafer;
                    m_infoCarrier.m_aInfoWafer[lastIdx].p_eWaferOrder = InfoWafer.eWaferOrder.LastWafer;
                }
                m_module.m_engineer.ClassHandler().UpdateEvent();
                return sResult;
            }
        }
        #endregion
    }
}
