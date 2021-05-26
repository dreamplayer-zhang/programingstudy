using RootTools.Control;
using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.OHTNew
{
    public class OHT : NotifyProperty, ITool
    {
        #region Property
        public string p_id { get; set; }

        protected string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                if (value != "OK")
                {
                    m_log.Error(value);
                    m_module.p_eState = ModuleBase.eState.Error;
                }
                OnPropertyChanged();
            }
        }

        public bool p_bPresent
        {
            get { return m_loadport.p_bPresent; }
        }

        public bool p_bPlaced
        {
            get { return m_loadport.p_bPlaced; }
        }

        bool _bAuto = false; 
        public bool p_bAuto
        {
            get { return _bAuto; }
            set
            {
                if (_bAuto == value) return;
                _bAuto = value;
                if (value)
                {
                    if (p_bPresent) m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToLoad;
                    else m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToUnload;
                }
                OnPropertyChanged(); 
            }
        }

        DO p_doLU_Req
        {
            get
            {
                switch (m_carrier.p_eTransfer)
                {
                    case GemCarrierBase.eTransfer.ReadyToLoad: return m_doLoadReq;
                    case GemCarrierBase.eTransfer.ReadyToUnload: return m_doUnloadReq;
                    default:
                        p_sInfo = "Invalid Transfer State";
                        m_module.p_eState = ModuleBase.eState.Error;
                        p_eState = eState.Error;
                        break;
                }
                return m_doLoadReq;
            }
        }

        bool _bES = true;
        bool p_bES
        {
            get { return !m_doES.p_bOn; }
            set
            {
                if (m_doES.p_bOn != value) return;
                if (m_doES.m_do.m_bitDO.m_nID < 0) return;
                if (_bES != value) m_log.Info("p_bES = " + value.ToString());
                _bES = value;
                OnPropertyChanged();
                m_doES.p_bOn = !value;
                p_eState = eState.Error;
            }
        }

        bool _bHoAvailable = true;
        bool p_bHoAvailable
        {
            set
            {
                if (m_doHoAvailable.p_bOn != value) return;
                if (m_doHoAvailable.m_do.m_bitDO.m_nID < 0) return;
                if (_bHoAvailable != value) m_log.Info("p_bHoAvailable = " + value.ToString());
                _bHoAvailable = value;
                OnPropertyChanged();
                m_doHoAvailable.p_bOn = !value;
            }
        }

        bool _bAbort = false;
        bool p_bAbort
        {
            get { return _bAbort; }
            set
            {
                if (_bAbort == value) return;
                _bAbort = value;
                m_log.Info("p_bAbort = " + value.ToString());
                OnPropertyChanged();
                m_doAbort.p_bOn = value;
            }
        }
        #endregion

        #region TP
        enum eTP
        {
            NA,
            TP1,
            TP2,
            TP3,
            TP4,
            TP5,
            TD3
        }
        Dictionary<eTP, int> m_dicTP = new Dictionary<eTP, int>();
        void InitTP()
        {
            m_dicTP.Add(eTP.NA, 0);
            m_dicTP.Add(eTP.TP1, 2);
            m_dicTP.Add(eTP.TP2, 2);
            m_dicTP.Add(eTP.TP3, 60);
            m_dicTP.Add(eTP.TP4, 60);
            m_dicTP.Add(eTP.TP5, 2);
            m_dicTP.Add(eTP.TD3, 10);
        }

        StopWatch m_swTP = new StopWatch();
        eTP m_eCheckTP = eTP.TP1;
        int m_msTP = 0;
        void StartTP(eTP tp)
        {
            m_eCheckTP = tp;
            m_msTP = 1000 * m_dicTP[tp];
            m_swTP.Start();
            m_log.Info(tp.ToString() + " Start StopWatch");
        }

        string CheckTP()
        {
            if (EQ.p_bSimulate) return "OK";
            if (m_msTP <= 0) return "OK";
            if (m_swTP.ElapsedMilliseconds < m_msTP) return "OK";
            m_msTP = 0;
            p_eState = eState.Error; 
            switch (m_eCheckTP)
            {
                case eTP.TP1: return "TP1 Timeout (TR_REQ signal did not turn ON within specified time.)";
                case eTP.TP2: return "TP2 Timeout (BUSY signal did not turn ON within specified time.)";
                case eTP.TP3:
                    if (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad) return "TP3 Timeout (Carrier was not detected within specified time.)";
                    return "TP3 Timeout (Carrier was not removed within specified time.)";
                case eTP.TP4:
                    if (m_diBusy.p_bOn) return "TP4 Timeout (BUSY signal did not turn OFF within specified time.)";
                    if (m_diTrReq.p_bOn) return "TP4 Timeout (TR_REQ signal did not turn OFF within specified time.)";
                    return "TP4 Timeout (COMPT signal did not turn ON within specified time.)";
                case eTP.TP5:
                    if (m_diValid.p_bOn) return "TP5 Timeout (VALID signal did not turn OFF within specified time.";
                    if (m_diComplete.p_bOn) return "TP5 Timeout (COMPT signal did not turn OFF within specified time.";
                    return "TP5 Timeout (CS_0 signal did not turn OFF within specified time.";
                case eTP.TD3: return "TD3 Timeout";
            }
            return "OK";
        }

        void RunTreeTP(Tree tree)
        {
            foreach (eTP tp in Enum.GetValues(typeof(eTP)))
            {
                m_dicTP[tp] = tree.Set(m_dicTP[tp], m_dicTP[tp], tp.ToString(), "OHT TP Value (sec)");
            }
        }
        #endregion

        #region eState
        public enum eState
        {
            All_Off,
            LU_Req_On,
            Ready_On,
            Busy_On,
            LU_Req_Off,
            Ready_Off,
            Error
        }
        eState _eState = eState.All_Off;
        public string[] m_asState = Enum.GetNames(typeof(eState)); 
        public eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                m_log.Info(p_id + " State : " + _eState.ToString() + " -> " + value.ToString());
                _eState = value;
                OnPropertyChanged();
                switch (value)
                {
                    case eState.LU_Req_On: StartTP(eTP.TP1); break;
                    case eState.Ready_On: StartTP(eTP.TP2); break;
                    case eState.Busy_On: StartTP(eTP.TP3); break;
                    case eState.LU_Req_Off: StartTP(eTP.TP4); break;
                    case eState.Ready_Off: StartTP(eTP.TP5); break;
                    default: StartTP(eTP.NA); break;
                }
                if (value == eState.All_Off)
                {
                    m_doLoadReq.p_bOn = false;
                    m_doUnloadReq.p_bOn = false;
                    m_doReady.p_bOn = false;
                }
            }
        }
        #endregion

        #region DI
        public class DI
        {
            public DIO_I m_di = null;

            public string p_id { get; set; }

            StopWatch m_sw = new StopWatch();
            public int m_msChange = 100;
            bool _bOn = false;
            public bool p_bOn
            {
                get
                {
                    if (m_di == null) return false;
                    if (_bOn == m_di.p_bIn) m_sw.Start();
                    else if (m_sw.ElapsedMilliseconds >= m_msChange) _bOn = m_di.p_bIn;
                    return _bOn;
                }
                set { }
            }

            public bool p_bWait { get; set; }

            public void RunTree(Tree tree)
            {
                m_msChange = tree.Set(m_msChange, m_msChange, p_id, "DI Change Delay for Remove Noise (ms)");
            }

            public void RunTreeToolBox(Tree tree, OHT OHT)
            {
                if (m_di == null) m_di = new DIO_I(OHT.m_toolDIO, OHT.p_id + "." + p_id, OHT.m_log, false);
                if (m_di.RunTree(tree) != "OK") return;
                OHT.m_module.m_listDI.AddBit(m_di.m_bitDI);
            }

            public DI(string id)
            {
                p_id = id;
                p_bWait = false;
            }
        }

        public ObservableCollection<DI> p_aDI { get; set; }
        void InitDI()
        {
            p_aDI = new ObservableCollection<DI>();
            for (int n = 0; n < 8; n++) p_aDI.Add(null); 
        }

        void RunTreeDI(Tree tree)
        {
            foreach (DI DI in p_aDI)
            {
                if (DI != null) DI.RunTree(tree);
            }
        }
        #endregion

        #region DO
        public class DO
        {
            public DIO_O m_do = null;

            public string p_id { get; set; }

            public bool p_bOn
            {
                get { return (m_do == null) ? false : m_do.p_bOut; }
                set
                {
                    p_bWait = value;
                    if (m_do != null) m_do.Write(value);
                }
            }

            public bool p_bWait { get; set; }

            public void Toggle()
            {
                p_bOn = !p_bOn;
            }

            public void RunTreeToolBox(Tree tree, OHT OHT)
            {
                if (m_do == null) m_do = new DIO_O(OHT.m_toolDIO, OHT.p_id + "." + p_id, OHT.m_log, false);
                if (m_do.RunTree(tree) != "OK") return;
                OHT.m_module.m_listDO.AddBit(m_do.m_bitDO);
            }

            public DO(string id)
            {
                p_id = id;
                p_bWait = false;
            }
        }

        public ObservableCollection<DO> p_aDO { get; set; }
        void InitDO()
        {
            p_aDO = new ObservableCollection<DO>();
            for (int n = 0; n < 8; n++) p_aDO.Add(null);
        }
        #endregion

        #region eOHT Type
        public enum eOHT
        {
            Semi,
            SSEM,
        }
        eOHT m_eOHT = eOHT.Semi; 
        void RunTreeOHT(Tree tree)
        {
            m_eOHT = (eOHT)tree.Set(m_eOHT, m_eOHT, "Type", "OHT Type"); 
        }

        public DI m_diValid = new DI("Valid");
        public List<DI> m_diCS = new List<DI>();
        public DI m_diTrReq = new DI("TR_REQ");
        public DI m_diBusy = new DI("Busy");
        public DI m_diComplete = new DI("Complete");
        public DI m_diContinue = new DI("Continue");
        public DI m_diLightCurtain = new DI("LightCurtain");
        public DO m_doLoadReq = new DO("LoadReq");
        public DO m_doUnloadReq = new DO("UnloadReq");
        public DO m_doReady = new DO("Ready");
        public DO m_doHoAvailable = new DO("HoAvailable");
        public DO m_doES = new DO("ES");
        public DO m_doAbort = new DO("Abort");
        void InitOHT()
        {
            p_aDI[0] = m_diValid;
            p_aDO[0] = m_doLoadReq;
            p_aDO[1] = m_doUnloadReq;
            p_aDO[3] = m_doReady; 
            switch (m_eOHT)
            {
                case eOHT.SSEM:
                    for (int n = 0; n < 4; n++)
                    {
                        m_diCS.Add(new DI("CS" + n.ToString()));
                        p_aDI[n + 1] = m_diCS[n]; 
                    }
                    p_aDI[5] = m_diTrReq;
                    p_aDI[6] = m_diBusy;
                    p_aDI[7] = m_diComplete;
                    p_aDO[7] = m_doAbort; 
                    break;
                default:
                    for (int n = 0; n < 2; n++)
                    {
                        m_diCS.Add(new DI("CS" + n.ToString()));
                        p_aDI[n + 1] = m_diCS[n];
                    }
                    p_aDI[4] = m_diTrReq;
                    p_aDI[5] = m_diBusy;
                    p_aDI[6] = m_diComplete;
                    p_aDI[7] = m_diContinue;
                    p_aDO[6] = m_doHoAvailable;
                    p_aDO[7] = m_doES; 
                    break; 
            }
        }
        #endregion

        #region p_ui
        public UserControl p_ui
        {
            get
            {
                OHT_UI ui = new OHT_UI();
                ui.Init(this);
                return ui; 
            }
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread;
        void InitThread()
        {
            m_bThread = true;
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        void RunThread()                                          
        {
            m_bThread = true;
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(20);
                p_eAccessLP = m_carrier.p_eAccessLP;
                p_bModuleReady = (m_module.p_eState == ModuleBase.eState.Ready);

                p_bES = m_diLightCurtain.p_bOn || (m_carrier.p_eAccessLP == GemCarrierBase.eAccessLP.Manual);
                p_bHoAvailable = p_bES || (p_bModuleReady == false);
                p_bAbort = p_bES || (p_bModuleReady == false);

                string sTP = CheckTP();
                if (sTP != "OK")
                {
                    p_sInfo = sTP;
                    p_eState = eState.Error;
                }
                switch (p_eState)
                {
                    case eState.All_Off:
                        CheckPresent(false);
                        bool bCS = IsCS(true);
                        CheckDI(m_diValid, true);
                        if (bCS && m_diValid.p_bOn)
                        {
                            p_doLU_Req.p_bOn = true;
                            p_eState = eState.LU_Req_On;
                        }
                        CheckDI(m_diTrReq, false);
                        CheckDI(m_diBusy, false);
                        CheckDI(m_diComplete, false);
                        m_doLoadReq.p_bWait = false;
                        m_doUnloadReq.p_bWait = false;
                        m_doReady.p_bWait = false;
                        break;
                    case eState.LU_Req_On:
                        CheckPresent(false);
                        CheckCS(true);
                        CheckDI(m_diValid, true);
                        if (m_diTrReq.p_bOn)
                        {
                            m_doReady.p_bOn = true;
                            p_eState = eState.Ready_On;
                        }
                        CheckDI(m_diBusy, false);
                        CheckDI(m_diComplete, false);
                        m_doLoadReq.p_bWait = (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad);
                        m_doUnloadReq.p_bWait = (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload);
                        m_doReady.p_bWait = false;
                        break;
                    case eState.Ready_On:
                        CheckPresent(false);
                        CheckCS(true);
                        CheckDI(m_diValid, true);
                        CheckDI(m_diTrReq, true);
                        if (m_diBusy.p_bOn) p_eState = eState.Busy_On;
                        CheckDI(m_diComplete, false);
                        m_doLoadReq.p_bWait = (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad);
                        m_doUnloadReq.p_bWait = (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload);
                        m_doReady.p_bWait = true;
                        break;
                    case eState.Busy_On:
                        CheckCS(true);
                        CheckDI(m_diValid, true);
                        CheckDI(m_diTrReq, true);
                        CheckDI(m_diBusy, true);
                        if (IsLUReqDone())
                        {
                            p_doLU_Req.p_bOn = false;
                            p_eState = eState.LU_Req_Off;
                        }
                        CheckDI(m_diComplete, false);
                        m_doLoadReq.p_bWait = (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad);
                        m_doUnloadReq.p_bWait = (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload);
                        m_doReady.p_bWait = true;
                        break;
                    case eState.LU_Req_Off:
                        CheckPresent(true);
                        CheckCS(true);
                        CheckDI(m_diValid, true);
                        if ((m_diTrReq.p_bOn == false) && (m_diBusy.p_bOn == false) && m_diComplete.p_bOn)
                        {
                            m_doReady.p_bOn = false;
                            p_eState = eState.Ready_Off;
                        }
                        m_doLoadReq.p_bWait = false;
                        m_doUnloadReq.p_bWait = false;
                        m_doReady.p_bWait = true;
                        break;
                    case eState.Ready_Off:
                        CheckPresent(true);
                        if ((IsCS(false) == false) && (m_diValid.p_bOn == false) && (m_diComplete.p_bOn == false))
                        {
                            switch (m_carrier.p_eTransfer)
                            {
                                case GemCarrierBase.eTransfer.ReadyToLoad:
                                    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                                    break;
                                case GemCarrierBase.eTransfer.ReadyToUnload:
                                    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToLoad;
                                    break;
                            }
                            p_eState = eState.All_Off;
                        }
                        CheckDI(m_diTrReq, false);
                        CheckDI(m_diBusy, false);
                        m_doLoadReq.p_bWait = false;
                        m_doUnloadReq.p_bWait = false;
                        m_doReady.p_bWait = false;
                        break;
                }
            }
        }

        GemCarrierBase.eAccessLP _eAccessLP = GemCarrierBase.eAccessLP.Manual;
        public GemCarrierBase.eAccessLP p_eAccessLP
        {
            get { return _eAccessLP; }
            set
            {
                if (value == GemCarrierBase.eAccessLP.Manual)
                {
                    CheckCS(false);
                    CheckDI(m_diValid, false);
                    CheckDI(m_diTrReq, false);
                    CheckDI(m_diBusy, false);
                    CheckDI(m_diComplete, false);
                    //p_eState = eState.Error;
                }
                if (_eAccessLP == value) return;
                _eAccessLP = value;
                if (value == GemCarrierBase.eAccessLP.Auto)
                {
                    for (int n = 0; n < m_diCS.Count; n++) InvalidAccess(m_diCS[n].p_bOn, "CS_" + n.ToString());
                    InvalidAccess(m_diValid.p_bOn, "VALID");
                    InvalidAccess(m_diTrReq.p_bOn, "TR_REQ");
                    InvalidAccess(m_diBusy.p_bOn, "BUSY");
                    InvalidAccess(m_diComplete.p_bOn, "COMPT");
                }
                OnPropertyChanged();
            }
        }

        bool _bModuleReady = false;
        bool p_bModuleReady
        {
            get { return _bModuleReady; }
            set
            {
                if (_bModuleReady == value) return;
                _bModuleReady = value;
            }
        }

        void InvalidAccess(bool bOn, string sID)
        {
            if (bOn == false) return;
            p_sInfo = sID + " signal was turned ON improperly.";
        }

        void CheckPresent(bool bDone)
        {
            if (m_module.p_eState == ModuleBase.eState.Error) return;
            GemCarrierBase.ePresent present;
            switch (m_carrier.p_eTransfer)
            {
                case GemCarrierBase.eTransfer.ReadyToLoad:
                    present = bDone ? GemCarrierBase.ePresent.Exist : GemCarrierBase.ePresent.Empty;
                    break;
                case GemCarrierBase.eTransfer.ReadyToUnload:
                    present = bDone ? GemCarrierBase.ePresent.Empty : GemCarrierBase.ePresent.Exist;
                    break;
                default: return;
            }
            if (m_carrier.p_ePresentSensor == present) return;
            p_sInfo = p_id + " Illegal Prosent Sensor";
        }

        bool IsCS(bool bOn)
        {
            for (int n = 0; n < m_diCS.Count; n++) m_diCS[n].p_bWait = bOn;
            bool bCSOn = false;
            for (int n = 0; n < m_diCS.Count; n++) bCSOn |= m_diCS[n].p_bOn;
            if (bCSOn == bOn)
            {
                for (int n = 0; n < m_diCS.Count; n++) m_diCS[n].p_bWait = m_diCS[n].p_bOn;
            }
            return bCSOn;
        }

        void CheckCS(bool bOn)
        {
            bool bCSOn = IsCS(bOn);
            if (bCSOn == bOn) return;
            CheckDI(m_diCS[0], bOn);
        }

        void CheckDI(DI di, bool bOn)
        {
            di.p_bWait = bOn; 
            if (m_module.p_eState == ModuleBase.eState.Error) return;
            if (di.p_bOn == bOn) return;
            string sOn = bOn ? "ON" : "OFF";
            p_eState = eState.Error; 
            p_sInfo = p_id + m_eCheckTP.ToString() + " Illegal sequence (" + di.p_id + " signal was turned " + sOn + " improperly";
        }

        bool IsLUReqDone()
        {
            switch (m_carrier.p_eTransfer)
            {
                case GemCarrierBase.eTransfer.ReadyToLoad:
                    return (m_carrier.p_ePresentSensor == GemCarrierBase.ePresent.Exist);
                case GemCarrierBase.eTransfer.ReadyToUnload:
                    return (m_carrier.p_ePresentSensor == GemCarrierBase.ePresent.Empty);
            }
            return true;
        }
        #endregion

        #region Tree ToolBox
        public void RunTreeToolBox(Tree tree)
        {
            foreach (DI DI in p_aDI)
            {
                if (DI != null) DI.RunTreeToolBox(tree.GetTree(DI.p_id), this);
            }
            foreach (DO DO in p_aDO)
            {
                if (DO != null) DO.RunTreeToolBox(tree.GetTree(DO.p_id), this);
            }
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(p_id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree; ;
            RunTree(Tree.eMode.RegRead);
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeOHT(m_treeRoot.GetTree("OHT"));
            RunTreeDI(m_treeRoot.GetTree("Digital Input"));
            RunTreeTP(m_treeRoot.GetTree("TP"));
        }
        #endregion

        public ModuleBase m_module;
        dynamic m_loadport; 
        public Log m_log;
        public GemCarrierBase m_carrier = null;
        IToolDIO m_toolDIO;

        public OHT(string id, ModuleBase module, GemCarrierBase carrier, IToolDIO toolDIO)
        {
            p_id = id;
            m_module = module;
            m_loadport = module; 
            m_carrier = carrier;
            m_log = module.m_log;
            m_toolDIO = toolDIO;

            InitTP(); 
            InitDI();
            InitDO(); 
            InitTree();
            InitOHT(); 
            InitThread();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join();
            }
        }
    }
}
