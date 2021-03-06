using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.OHT.SSEM
{
    public class OHT_SSEM : OHTBase, ITool
    {
        #region DIO
        DI m_diValid = new DI("2.Valid.12");
        DI[] m_diCS = new DI[4] { new DI("1.CS0.14"), new DI("1.CS1.14"), new DI("1.CS2.14"), new DI("1.CS3.14") };
        DI m_diTrReq = new DI("4.TrackReq.9");
        DI m_diBusy = new DI("6.Busy.8");
        DI m_diComplete = new DI("10.Complete.13");
        DI m_diContinue = new DI("Continue");
        DI m_diLightCurtain = new DI("LightCurtain");
        DO m_doLoadReq = new DO("3.LoadReq.7");
        DO m_doUnloadReq = new DO("3.UnloadReq.7");
        DO m_doReady = new DO("5.Ready.11");
        DO m_doAbort = new DO("Abort");
        void InitDIO()
        {
            m_aDIO.Add(m_doLoadReq);
            m_aDIO.Add(m_doUnloadReq);
            m_aDIO.Add(m_doReady);
            m_aDIO.Add(m_diCS[0]);
            m_aDIO.Add(m_diCS[1]);
            m_aDIO.Add(m_diCS[2]);
            m_aDIO.Add(m_diCS[3]);
            m_aDIO.Add(m_diValid);
            m_aDIO.Add(m_diTrReq);
            m_aDIO.Add(m_diBusy);
            m_aDIO.Add(m_diComplete);
            m_aDIO.Add(m_diContinue);
            m_aDIO.Add(m_doAbort);
            m_aDIO.Add(m_diLightCurtain);
        }
        #endregion

        #region ITool
        List<OHT_SSEM_UI> m_aUI = new List<OHT_SSEM_UI>();
        public UserControl p_ui
        {
            get
            {
                OHT_SSEM_UI ui = new OHT_SSEM_UI();
                ui.Init(this);
                m_aUI.Add(ui);
                return ui;
            }
        }

        void CheckChangeDIO()
        {
            if (IsChangeDIO() == false) return;
            History history = new History();
            foreach (IDIO dio in m_aDIO) history.m_aData.Add(new History.Data(dio));
            foreach (OHT_SSEM_UI ui in m_aUI) ui.m_qHistory.Enqueue(history);
            m_lastHistory = history;
        }

        History m_lastHistory = new History();
        bool IsChangeDIO()
        {
            if (m_lastHistory.m_aData.Count < m_aDIO.Count) return true;
            for (int n = 0; n < m_aDIO.Count; n++)
            {
                if (m_aDIO[n].p_bOn != m_lastHistory.m_aData[n].p_bOn) return true;
            }
            return false;
        }
        #endregion

        #region Property
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
                        p_eState = eState.All_Off;
                        break;
                }
                return m_doLoadReq;
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

        #region State
        public enum eState
        {
            All_Off,
            LU_Req_On,
            Ready_On,
            Busy_On,
            LU_Req_Off,
            Ready_Off
        }
        eState _eState = eState.All_Off;
        public eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                m_log.Info(p_id + " State : " + _eState.ToString() + " -> " + value.ToString());
                _eState = value;
                p_sState = value.ToString();
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

                bool bES = m_diLightCurtain.p_bOn || (m_carrier.p_eAccessLP == GemCarrierBase.eAccessLP.Manual);
                p_bAbort = bES || (p_bModuleReady == false);

                string sTP = CheckTP();
                if (sTP != "OK")
                {
                    p_sInfo = sTP;
                    p_eState = eState.All_Off;
                }

                switch (p_eState)
                {
                    case eState.All_Off:
                        CheckPresent(false);
                        if (IsCS(true) && m_diValid.p_bOn)
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
                CheckChangeDIO();
            }
        }

        GemCarrierBase.eAccessLP _eAccessLP = GemCarrierBase.eAccessLP.Manual;
        GemCarrierBase.eAccessLP p_eAccessLP
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
                    p_eState = eState.All_Off;
                }
                if (_eAccessLP == value) return;
                _eAccessLP = value; 
                if (value == GemCarrierBase.eAccessLP.Auto)
                {
                    InvalidAccess(m_diCS[0].p_bOn, "CS_0");
                    InvalidAccess(m_diCS[1].p_bOn, "CS_1");
                    InvalidAccess(m_diCS[0].p_bOn, "CS_2");
                    InvalidAccess(m_diCS[1].p_bOn, "CS_3");
                    InvalidAccess(m_diValid.p_bOn, "VALID");
                    InvalidAccess(m_diTrReq.p_bOn, "TR_REQ");
                    InvalidAccess(m_diBusy.p_bOn, "BUSY");
                    InvalidAccess(m_diComplete.p_bOn, "COMPT");
                }
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
                if (value == false)
                {
                    //p_sInfo = "Loadport State not Ready";
                    //p_eState = eState.All_Off;
                }
            }
        }

        void InvalidAccess(bool bOn, string sID)
        {
            if (bOn == false) return;
            p_sInfo = p_id + " signal was turned ON improperly.";
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
            for (int n = 0; n < 4; n++) m_diCS[n].p_bWait = bOn;
            bool bCSOn = false;
            for (int n = 0; n < 4; n++) bCSOn |= m_diCS[n].p_bOn;
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
            if (m_module.p_eState == ModuleBase.eState.Error) return;
            if (di.p_bOn == bOn) return;
            string sOn = bOn ? "ON" : "OFF";
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

        #region Tree
        void RunTreeInfo(Tree tree)
        {
            p_eState = (eState)tree.Set(p_eState, p_eState, "State", "OHT State");
        }

        public override void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeInfo(m_treeRoot.GetTree("Info"));
            RunTreeDIO(m_treeRoot.GetTree("Digital Input"));
            RunTreeTP(m_treeRoot.GetTree("TP"));
        }
        #endregion

        public OHT_SSEM(string id, ModuleBase module, GemCarrierBase carrier, IToolDIO toolDIO)
        {
            p_id = id;
            InitTP();
            InitDIO();
            InitBase(module, carrier, toolDIO);
            InitThread();
            CheckChangeDIO();
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
