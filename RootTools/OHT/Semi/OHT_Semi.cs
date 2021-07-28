using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.OHT.Semi
{
    public class OHT_Semi : OHTBase, ITool
    {
        #region DIO
        public DI m_diValid = new DI("2.Valid.12");
        public DI[] m_diCS = new DI[2] { new DI("1.CS0.14"), new DI("1.CS1.14") };
        public DI m_diTrReq = new DI("4.TrackReq.9");
        public DI m_diBusy = new DI("6.Busy.8");
        public DI m_diComplete = new DI("10.Complete.13");
        public DI m_diContinue = new DI("Continue");
        public DI m_diLightCurtain = new DI("LightCurtain");
        public DO m_doLoadReq = new DO("3.LoadReq.7");
        public DO m_doUnloadReq = new DO("3.UnloadReq.7");
        public DO m_doReady = new DO("5.Ready.11");
        public DO m_doHoAvailable = new DO("HoAvailable");
        public DO m_doES = new DO("ES");
        void InitDIO()
        {
            m_aDIO.Add(m_doLoadReq);
            m_aDIO.Add(m_doUnloadReq);
            m_aDIO.Add(m_doReady);
            m_aDIO.Add(m_diCS[0]);
            m_aDIO.Add(m_diCS[1]);
            m_aDIO.Add(m_diValid);
            m_aDIO.Add(m_diTrReq);
            m_aDIO.Add(m_diBusy);
            m_aDIO.Add(m_diComplete);
            m_aDIO.Add(m_diContinue);
            m_aDIO.Add(m_doHoAvailable);
            m_aDIO.Add(m_doES);
            m_aDIO.Add(m_diLightCurtain);
        }
        #endregion
        public bool m_bRFIDRead = false; //LYJ
        //public bool m_bPIOComplete = false; //LYJ 210412
        bool _bLightCurtain = false;
        public bool m_bLoadportHome = false;
        public bool p_bLightCurtain
        {
            get
            {
                return _bLightCurtain;
            }
            set
            {
                if (_bLightCurtain == value)
                    return;
                _bLightCurtain = value;
                OnPropertyChanged();
            }
        }

        bool _bProtectionBar = false;
        public bool P_bProtectionBar
        {
            get
            {
                return _bProtectionBar;
            }
            set
            {
                if (_bProtectionBar == value)
                    return;
                _bProtectionBar = value;
                OnPropertyChanged();
            }
        }

        bool _bMCReset = false;
        public bool P_MCReset
        {
            get
            {
                return _bMCReset;
            }
            set
            {
                if (_bMCReset == value)
                    return;
                _bMCReset = value;
                OnPropertyChanged();
            }
        }

        #region ITool
        List<OHT_Semi_UI> m_aUI = new List<OHT_Semi_UI>();
        public UserControl p_ui
        {
            get
            {
                OHT_Semi_UI ui = new OHT_Semi_UI();
                ui.Init(this);
                m_aUI.Add(ui);
                return ui;
            }
        }

        void CheckChangeDIO()
        {
            if (IsChangeDIO() == false)
                return;
            History history = new History();
            foreach (IDIO dio in m_aDIO)
                history.m_aData.Add(new History.Data(dio));
            foreach (OHT_Semi_UI ui in m_aUI)
                ui.m_qHistory.Enqueue(history);
            m_lastHistory = history;
        }

        History m_lastHistory = new History();
        bool IsChangeDIO()
        {
            if (m_lastHistory.m_aData.Count < m_aDIO.Count)
                return true;
            for (int n = 0; n < m_aDIO.Count; n++)
            {
                if (m_aDIO[n].p_bOn != m_lastHistory.m_aData[n].p_bOn)
                    return true;
            }
            return false;
        }

        public bool IsActiveInputOff()
        {
            if (IsCS(true) || m_diBusy.p_bOn || m_diTrReq.p_bOn || m_diValid.p_bOn || m_diComplete.p_bOn)
                return false;
            else
                return true;
        }
        #endregion

        #region Property
        DO p_doLU_Req
        {
            get
            {
                switch (m_carrier.p_eTransfer)
                {
                    case GemCarrierBase.eTransfer.ReadyToLoad:
                        return m_doLoadReq;
                    case GemCarrierBase.eTransfer.ReadyToUnload:
                        return m_doUnloadReq;
                    default:
                        p_sInfo = "Invalid Transfer State";
                        p_eState = eState.All_Off;
                        m_bOHTErr = false;
                        break;
                }
                return m_doLoadReq;
            }
        }

        bool _bHoAvailable = true; 
        public bool p_bHoAvailable
        {
            get
            {
                return m_doHoAvailable.p_bOn;
            }
            set
            {
                if (m_doHoAvailable.p_bOn == value)
                    return;
                if (m_doHoAvailable.m_do == null) return;
                if (m_doHoAvailable.m_do.m_bitDO.m_nID < 0)
                    return;
                if (_bHoAvailable != value)
                    m_log.Info("p_bHoAvailable = " + value.ToString());
                _bHoAvailable = value;
                OnPropertyChanged();
                m_doHoAvailable.p_bOn = value;
            }
        }

        bool _bES = true;
        public bool p_bES
        {
            get
            {
                return m_doES.p_bOn;
            }
            set
            {
                if (m_doES.p_bOn == value)
                    return;
                if (m_doES.m_do == null) return;
                if (m_doES.m_do.m_bitDO.m_nID < 0)
                    return;
                if (_bES != value)
                    m_log.Info("p_bES = " + value.ToString());
                _bES = value;
                OnPropertyChanged();
                m_doES.p_bOn = value;
                p_eState = eState.All_Off;
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

        StopWatch m_swTD = new StopWatch();
        //eTP m_eCheckTD = eTP.TD3;
        int m_msTD = 0;
        public void StartTD3()
        {
            //m_eCheckTD = eTP.TD3;
            m_msTD = 1000 * m_dicTP[eTP.TD3];
            m_swTD.Start();
            m_log.Info(eTP.TD3.ToString() + " Start StopWatch");
        }

        public bool m_bStartTD3 = false;
        public void ResetTD3()
        {
            m_swTD.Reset();
            m_bStartTD3 = false;
        }

        public string CheckTD3()
        {
            if (EQ.p_bSimulate)
                return "OK";
            if (m_msTD <= 0)
                return "OK";
            if (m_swTD.ElapsedMilliseconds < m_msTD)
                return "OK";
            if (m_bOHTErr == true)
            {
                return "OK";
            }
            m_msTD = 0;
            m_bTimeOutError = true;
            return "TD3 Sensor Logic \n (Carrier is placed incorrectly. \n Remove this or load stable.)";
        }


        string CheckTP()
        {
            if (EQ.p_bSimulate)
                return "OK";
            if (m_msTP <= 0)
                return "OK";
            if (m_swTP.ElapsedMilliseconds < m_msTP)
                return "OK";
            if (m_bOHTErr == true)
            {
                return "OK";
            }
            m_msTP = 0;
            m_bTimeOutError = true;
            switch (m_eCheckTP)
            {
                case eTP.TP1:
                    return "TP1 Timeout (TR_REQ signal did not\n turn ON within specified time.)";
                case eTP.TP2:
                    return "TP2 Timeout (BUSY signal did not\n turn ON within specified time.)";
                case eTP.TP3:
                    if (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad)
                        return "TP3 Timeout (Carrier was not\n detected within specified time.)";
                    return "TP3 Timeout (Carrier was not\n removed within specified time.)";
                case eTP.TP4:
                    if (m_diBusy.p_bOn)
                        return "TP4 Timeout (BUSY signal did not\n turn OFF within specified time.)";
                    if (m_diTrReq.p_bOn)
                        return "TP4 Timeout (TR_REQ signal did not\n turn OFF within specified time.)";
                    return "TP4 Timeout (COMPT signal did not\n turn ON within specified time.)";
                case eTP.TP5:
                    if (m_diValid.p_bOn)
                        return "TP5 Timeout (VALID signal did not\n turn OFF within specified time.)";
                    if (m_diComplete.p_bOn)
                        return "TP5 Timeout (COMPT signal did not\n turn OFF within specified time.)";
                    return "TP5 Timeout (CS_0 signal did not\n turn OFF within specified time.)";
                //case eTP.TD3: return "TD3 Timeout";
                    
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
            get
            {
                return _eState;
            }
            set
            {
                if (_eState == value)
                    return;
                m_log.Info(p_id + " State : " + _eState.ToString() + " -> " + value.ToString());
                _eState = value;
                p_sState = value.ToString();
                OnPropertyChanged();
                switch (value)
                {
                    case eState.LU_Req_On:
                        StartTP(eTP.TP1);
                        break;
                    case eState.Ready_On:
                        StartTP(eTP.TP2);
                        break;
                    case eState.Busy_On:
                        StartTP(eTP.TP3);
                        break;
                    case eState.LU_Req_Off:
                        StartTP(eTP.TP4);
                        break;
                    case eState.Ready_Off:
                        StartTP(eTP.TP5);
                        break;
                    default:
                        StartTP(eTP.NA);
                        break;
                }
                //if (value == eState.All_Off)
                //{
                //    m_doLoadReq.p_bOn = false;
                //    m_doUnloadReq.p_bOn = false;
                //    m_doReady.p_bOn = false;
                //}
            }
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        public bool m_bOHTErr = false;                          //KHD 201130 Modify
        public bool m_bPODExist = false;
        public bool m_bPlaced = false;
        public bool m_bPresent = false;
        public bool m_btempPlaced = false;
        public bool m_btempPresent = false;
        public bool m_bAuto = false;
        bool m_bAuto_p = false;
        public bool m_bProtectError = false;
        public bool m_bSensorError = false;
        public bool m_bSignalError = false;
        public bool m_bTimeOutError = false;
        public bool m_bCSProtectError = false;
        public bool m_bSystemError = false;
        Thread m_thread;
        void InitThread()
        {
            m_bThread = true;
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        void RunThread()                                          //KHD 201130 Modify
        {
            m_bThread = true;
            m_bOHTErr = false;

            Thread.Sleep(10);
            while (m_bThread)
            {
                
                if(IsOHTStateChange() && EQ.p_eState == EQ.eState.Error && !m_bOHTErr)
                {
                    p_sInfo = "System Error";
                    m_bOHTErr = true;
                    m_bSystemError = true;
                    //return;
                }
                //if (m_ModuleLP.p_id == "Loadport_Cymechs" && m_ModuleLP.p_eState == ModuleBase.eState.Init)
                //{
                //    //트랜스퍼블락
                //}
                //else ()
                //{
                //
                //}
                    Thread.Sleep(20);
                if (m_bAuto && m_bAuto != m_bAuto_p)
                {
                    m_carrier.p_eAccessLP = GemCarrierBase.eAccessLP.Auto;
                    if (m_bOHTErr)
                    {
                        m_carrier.p_eReqTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                    }
                    else
                    {
                       //if (!m_bPODExist)                                                     KHD 210407 del
                       //{
                       //    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToLoad;
                       //    m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Empty;
                       //}
                       //else
                       //{
                       //    //m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToUnload;
                       //    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked; //khd mod
                       //    m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Exist;
                       //}
                    }
                }
                else if (!m_bAuto && m_bAuto != m_bAuto_p)
                {
                    m_carrier.p_eAccessLP = GemCarrierBase.eAccessLP.Manual;
                    //m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToLoad; khd
                    if (m_carrier.p_eAccessLP == GemCarrierBase.eAccessLP.Manual && !m_bPresent && !m_bPlaced && m_carrier.p_eTransfer != GemCarrierBase.eTransfer.ReadyToUnload)
                    {
                        m_carrier.p_eReqTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                    }
                    else if (m_carrier.p_eAccessLP == GemCarrierBase.eAccessLP.Manual && m_bPresent && m_bPlaced)
                    {
                        m_carrier.p_eReqTransfer = GemCarrierBase.eTransfer.ReadyToLoad;
                    }
                }
                    m_bAuto_p = m_bAuto;
                    p_eAccessLP = m_carrier.p_eAccessLP;
                    //p_bModuyleReady = (m_module.p_eState == ModuleBase.eState.Ready);
                    //p_eAccessLP = GemCarrierBase.eAccessLP.Manual;
                    p_bModuyleReady = true;
                //p_bES = m_diLightCurtain.p_bOn || (m_carrier.p_eAccessLP == GemCarrierBase.eAccessLP.Manual);
                if (m_bAuto)
                {
                    if (m_bOHTErr != true /*&& m_bProtectError == true*/)
                    {
                        if (P_bProtectionBar || p_bLightCurtain)
                        {
                            m_bOHTErr = true;
                            if (P_bProtectionBar)
                            {
                                m_bOHTErr = true;
                                m_bCSProtectError = true;
                                if (P_bProtectionBar)
                                {
                                    p_sInfo = "ProtectionBar detect Error";
                                }
                                else
                                {
                                    p_sInfo = "LightCurtain detect Error";
                                }
                            }
                        }
                    }
                    else
                    {
                        if (P_bProtectionBar || p_bLightCurtain)
                        {
                            m_bProtectError = true;
                            m_bOHTErr = true;
                            if (P_bProtectionBar)
                            {
                                m_bProtectError = true;
                                //m_bOHTErr = true;
                                p_bES = true;
                                p_bHoAvailable = true;

                                if (P_bProtectionBar)
                                {
                                    p_sInfo = "ProtectionBar detect Error";
                                }
                                else
                                {
                                    p_sInfo = "LightCurtain detect Error";
                                }
                            }
                            else
                            {
                                m_bProtectError = false;
                                //m_bOHTErr = false;
                                if (m_bOHTErr != true && m_carrier.p_eTransfer != GemCarrierBase.eTransfer.TransferBlocked)
                                {
                                    p_bES = false;
                                    p_bHoAvailable = false;
                                }


                                p_sInfo = "OK";
                            }
                        }
                        else
                        {
                            m_bProtectError = false;
                            m_bOHTErr = false;
                            p_sInfo = "OK";
                        }
                    }
                }
                if (m_bProtectError == false)
                {
                    p_bES = m_bOHTErr || m_carrier.p_eAccessLP == GemCarrierBase.eAccessLP.Manual;
                    p_bHoAvailable = m_carrier.p_eTransfer == GemCarrierBase.eTransfer.TransferBlocked || m_carrier.p_eAccessLP == GemCarrierBase.eAccessLP.Manual;
                    //p_bHoAvailable = (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.TransferBlocked || m_carrier.p_eAccessLP == GemCarrierBase.eAccessLP.Manual) && !m_module.IsDocked(); 기억이..
                }
                    //p_bES = m_diLightCurtain.p_bOn 
                    //p_bHoAvailable = p_bES
                    string sTP = CheckTP();
                    if (sTP != "OK")
                    {
                        p_sInfo = sTP;
                        m_bOHTErr = true; //KHd 201130 add
                        p_eState = eState.All_Off;
                    }
                    if (!m_bOHTErr && m_bAuto)
                    {
                        switch (p_eState)
                        {
                            case eState.All_Off:
                                CheckPresent(false);
                                bool bCS = IsCS(true);
                                //if (bCS && m_diValid.p_bOn)
                                if (bCS && m_diValid.p_bOn && m_carrier.p_eTransfer != GemCarrierBase.eTransfer.TransferBlocked) //LYJ 210525 Modify 
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
                                if (m_diTrReq.p_bOn && m_carrier.p_eTransfer != GemCarrierBase.eTransfer.TransferBlocked) //LYJ 210525 Modify
                                {
                                    m_doReady.p_bOn = true;
                                    p_eState = eState.Ready_On;
                                }
                                CheckDI(m_diBusy, false);
                                CheckDI(m_diComplete, false);
                                m_doLoadReq.p_bWait = m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad;
                                m_doUnloadReq.p_bWait = m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload;
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
                                if (IsLUReqDone() && m_carrier.p_eTransfer != GemCarrierBase.eTransfer.TransferBlocked) //LYJ 210525 Modify
                            {
                                    p_doLU_Req.p_bOn = false;
                                    p_eState = eState.LU_Req_Off;
                                    ResetTD3();
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
                                if ((m_diTrReq.p_bOn == false) && (m_diBusy.p_bOn == false) && m_diComplete.p_bOn && m_carrier.p_eTransfer != GemCarrierBase.eTransfer.TransferBlocked) //LYJ 210525 Modify
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
                                    switch (m_carrier.p_ePresentSensor)
                                    {
                                        case GemCarrierBase.ePresent.Exist:
                                            m_carrier.p_eReqTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                                        m_bRFIDRead = true;
                                            break;
                                        case GemCarrierBase.ePresent.Empty:
                                            m_carrier.p_eReqTransfer = GemCarrierBase.eTransfer.ReadyToUnload;
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
        }

        GemCarrierBase.eAccessLP _eAccessLP = GemCarrierBase.eAccessLP.Manual;
        public GemCarrierBase.eAccessLP p_eAccessLP
        {
            get
            {
                return _eAccessLP;
            }
            set
            {
                //if (value == GemCarrierBase.eAccessLP.Manual)
                //{
                //    CheckCS(false);
                //    CheckDI(m_diValid, false);
                //    CheckDI(m_diTrReq, false);
                //    CheckDI(m_diBusy, false);
                //    CheckDI(m_diComplete, false);
                //    p_eState = eState.All_Off;
                //}
                if (_eAccessLP == value)
                    return;
                _eAccessLP = value;
                if (value == GemCarrierBase.eAccessLP.Auto)
                {
                    InvalidAccess(m_diCS[0].p_bOn, "CS_0");
                    InvalidAccess(m_diCS[1].p_bOn, "CS_1");
                    InvalidAccess(m_diValid.p_bOn, "VALID");
                    InvalidAccess(m_diTrReq.p_bOn, "TR_REQ");
                    InvalidAccess(m_diBusy.p_bOn, "BUSY");
                    InvalidAccess(m_diComplete.p_bOn, "COMPT");
                }
            }
        }

        bool _bModuleReady = false;
        bool p_bModuyleReady
        {
            get
            {
                return _bModuleReady;
            }
            set
            {
                if (_bModuleReady == value)
                    return;
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
            if (bOn == false)
                return;
            p_sInfo = p_id + " signal was turned ON improperly.";
        }

        StopWatch swTP3 = new StopWatch();

        void CheckTP3SensorLogic()
        {
            if ((m_module.IsPlacement() && !m_module.IsPresent()) || (!m_module.IsPlacement() && m_module.IsPresent()))
            {
                Thread.Sleep(3000);
                {
                    if ((m_module.IsPlacement() && !m_module.IsPresent()) || (!m_module.IsPlacement() && m_module.IsPresent()))
                    {
                        p_sInfo = p_id + m_eCheckTP.ToString() + " Sensor Logic \n(Carrier is placed incorrectly. \nRemove this or load stable.)";//" Illegal Present Sensor";
                        m_bOHTErr = true;
                    }
                }
            }
        }

        void CheckPresent(bool bDone)
        {
            bool bDoneOld = bDone;
            bool bpresent= false;
            if (m_module.p_eState == ModuleBase.eState.Error) return;
            GemCarrierBase.ePresent present;            
            switch (m_carrier.p_eTransfer)
            {
                case GemCarrierBase.eTransfer.ReadyToLoad:
                    //present = bDone ? GemCarrierBase.ePresent.Exist : GemCarrierBase.ePresent.Empty;   //KHD 210318 del
                    //if (m_carrier.p_ePresentSensor == GemCarrierBase.ePresent.Exist)
                    //{
                    //    bpresent = false;
                    //}
                    //else if( m_carrier.p_ePresentSensor == GemCarrierBase.ePresent.Empty)
                    //{
                    //    bpresent = true;
                    //}
                    if (p_eState == eState.All_Off || p_eState == eState.Ready_On || p_eState == eState.LU_Req_On || p_eState == eState.Busy_On)
                    {
                        if (m_bPresent && m_bPlaced)
                        {
                            bpresent = false;
                        }
                        else
                        {
                            bpresent = true;
                        }
                    }
                    else
                    {
                        if (m_bPresent || m_bPlaced)
                        {
                            bpresent = false;
                        }
                        else
                        {
                            bpresent = true;
                        }
                    }
                    
                   //if (m_bPODExist) //KHD 210318 add
                   //{
                   //    bpresent = true;
                   //}
                   //else
                   //{
                   //    bpresent = false;
                   //}

                    break;

                case GemCarrierBase.eTransfer.ReadyToUnload:
                    //present = bDone ? GemCarrierBase.ePresent.Empty : GemCarrierBase.ePresent.Exist;   //KHD 210318 del
                    //if (m_carrier.p_ePresentSensor == GemCarrierBase.ePresent.Empty)
                    //{
                    //    bpresent = false;
                    //}
                    //else if (m_carrier.p_ePresentSensor == GemCarrierBase.ePresent.Exist)
                    //{
                    //    bpresent = true;
                    //}
                    if (p_eState == eState.All_Off || p_eState == eState.Ready_On || p_eState == eState.LU_Req_On || p_eState == eState.Busy_On)
                    {
                        if (!m_bPresent && !m_bPlaced)
                        {
                            bpresent = false;
                        }
                        else
                        {
                            bpresent = true;
                        }
                    }
                    else
                    {
                        if (!m_bPresent || !m_bPlaced)
                        {
                            bpresent = false;
                        }
                        else
                        {
                            bpresent = true;
                        }
                    }
                    break;

                //if (!m_bPODExist) //KHD 210318 add
                //{
                //    bpresent = true;
                //}
                //else
                //{
                //    bpresent = false;
                //}

                //KHD 210420
                case GemCarrierBase.eTransfer.TransferBlocked:
                    m_btempPlaced = m_bPlaced;
                    m_btempPresent = m_bPresent;
                    //present = bDone ? GemCarrierBase.ePresent.Empty : GemCarrierBase.ePresent.Exist;   //KHD 210318 del
                    //if (m_carrier.p_ePresentSensor == GemCarrierBase.ePresent.Empty)
                    //{
                    //    bpresent = false;
                    //}
                    //else if (m_carrier.p_ePresentSensor == GemCarrierBase.ePresent.Exist)
                    //{
                    //    bpresent = true;
                    //}
                    if (!m_bOHTErr && p_eState == eState.All_Off && !m_bLoadportHome)
                    {
                        if (m_bPresent || m_bPlaced)
                        {
                            bpresent = true;
                        }
                        else
                        {
                            bpresent = false;
                        }
                    }
                    else
                    {
                        bpresent = false ;
                    }


                    //if (!m_bPODExist) //KHD 210318 add
                    //{
                    //    bpresent = true;
                    //}
                    //else
                    //{
                    //    bpresent = false;
                    //}
                    break;
                default:
                    return;
            }
            //if (m_carrier.p_ePresentSensor == present) return;  //kHD 210318 del
            if (bpresent == bDone)
                return;
            //if (bDoneOld == bpresent) return;
            m_bSensorError = true;
            if (IsCS(true))
            {
                m_bOHTErr = true;               
                p_sInfo = p_id + m_eCheckTP.ToString() + " Sensor Logic \n(Carrier is placed incorrectly. \nRemove this or load stable.)";//" Illegal Present Sensor";
            }
            else
            {
                m_bOHTErr = true;
                p_sInfo = p_id + /*m_eCheckTP.ToString() +*/ " Access Violation \n(Carrier is placed incorrectly. \nRemove this or load stable.)";//" Illegal Present Sensor";
            }
        }

        bool IsCS(bool bOn)
        {
            for (int n = 0; n < 2; n++)
                m_diCS[n].p_bWait = bOn;
            bool bCSOn = false;
            for (int n = 0; n < 2; n++)
                bCSOn |= m_diCS[n].p_bOn;
            return bCSOn;
        }

        void CheckCS(bool bOn)
        {
            bool bCSOn = IsCS(bOn);
            if (bCSOn == bOn)
                return;
            CheckDI(m_diCS[0], bOn);
        }

        void CheckDI(DI di, bool bOn)
        {
            //if (m_module.p_eState == ModuleBase.eState.Error) return; 
            if (di.p_bOn == bOn) return; 
            string sOn = !bOn ? "ON" : "OFF";
            m_bOHTErr = true;                  //KHD 201130 add
            m_bSignalError = true;
            p_sInfo = p_id + m_eCheckTP.ToString() + " Illegal sequence \n(" + di.p_id + " signal was turned\n " + sOn + " improperly";
        }

        public void AutoCheckDI(DI di)
        {
            //if (m_module.p_eState == ModuleBase.eState.Error) return;
            if (di.p_bOn != true)
                return;
            m_bOHTErr = true;                  //KHD 201130 add
            m_bSignalError = true;
            p_sInfo = p_id + m_eCheckTP.ToString() + " Illegal sequence \n(" + di.p_id + " signal was turned ON\n improperly";
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
            //p_eState = (eState)tree.Set(p_eState, p_eState, "State", "OHT State"); 
        }

        public override void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeInfo(m_treeRoot.GetTree("Info"));
            RunTreeDIO(m_treeRoot.GetTree("Digital Input"));
            RunTreeTP(m_treeRoot.GetTree("TP"));
        }
        #endregion

        public OHT_Semi(string id, ModuleBase module, GemCarrierBase carrier, IToolDIO toolDIO)
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
        eState temp = (eState)EQ.eState.Init;
        bool IsOHTStateChange()
        {
            if (temp != (eState)EQ.p_eState)
            {
                temp = (eState)EQ.p_eState;
                return true;
            }
            else
                return false;
        }
    }
}
