using RootTools.Gem;
using RootTools.OHT;
using RootTools.OHT.Semi;
using RootTools;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Root_EFEM.Module;
using System.Collections.ObjectModel;
using RootTools.OHTNew;
using System.Collections.Generic;
using RootTools.GAFs;
using RootTools.Module;

namespace Root_VEGA_D.Module
{
    /// <summary>
    /// OHT_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OHT_UI : UserControl
    {
        public OHT_UI()
        {
            InitializeComponent();
        }

        #region AccessMode
        void TimerAccessMode()
        {
            bool bAuto = (m_carrier.p_eAccessLP == GemCarrierBase.eAccessLP.Auto);
            SetBrush(buttonAccessManual, !bAuto && p_bBlink);
            SetBrush(buttonAccessAuto, bAuto && p_bBlink);
        }

        void ChangeLPState(GemCarrierBase.eAccessLP State)
        {
            /*            if (State == GemCarrierBase.eAccessLP.Auto)
                        {
                            m_loadport.m_doAuto.Write(true);
                            m_loadport.m_doManual.Write(false);
                        }
                        else if (State == GemCarrierBase.eAccessLP.Manual)
                        {
                            m_loadport.m_doAuto.Write(false);
                            m_loadport.m_doManual.Write(true);
                        }
            */
        }

        private void buttonAccessManual_Click(object sender, RoutedEventArgs e)
        {
            if (!m_OHT.m_diCS[0].p_bOn)
            {
                m_OHT.m_doHoAvailable.p_bOn = false;
                m_OHT.m_doES.p_bOn = false;
                m_loadport.p_infoCarrier.p_eReqAccessLP = GemCarrierBase.eAccessLP.Manual;
                //changeLPState(GemCarrierBase.eAccessLP.Manual);
                m_OHT.m_bAuto = false;
            }
        }

        private void buttonAccessAuto_Click(object sender, RoutedEventArgs e)
        {
            if (m_OHT.m_diCS[0].p_bOn || m_OHT.m_diValid.p_bOn || m_OHT.m_diTrReq.p_bOn || m_OHT.m_diBusy.p_bOn || m_OHT.m_diComplete.p_bOn || m_OHT.m_diContinue.p_bOn)
            {
                m_OHT.AutoCheckDI(m_OHT.m_diCS[0]);
                m_OHT.AutoCheckDI(m_OHT.m_diValid);
                m_OHT.AutoCheckDI(m_OHT.m_diTrReq);
                m_OHT.AutoCheckDI(m_OHT.m_diBusy);
                m_OHT.AutoCheckDI(m_OHT.m_diComplete);
                m_OHT.AutoCheckDI(m_OHT.m_diContinue);
            }
            else
            {
                m_loadport.p_infoCarrier.p_eReqAccessLP = GemCarrierBase.eAccessLP.Auto; //KHD 210317
                                                                                         //changeLPState(GemCarrierBase.eAccessLP.Auto);
                if (!m_loadport.m_diPresent.p_bIn && !m_loadport.m_diPlaced.p_bIn)
                {
                    m_OHT.m_bPODExist = true;
                }
                else if (m_loadport.m_diPlaced.p_bIn && m_loadport.m_diPlaced.p_bIn)
                {
                    m_OHT.m_bPODExist = false;
                }
                else
                {
                    m_OHT.m_bPODExist = true;
                }
               //if (!m_loadport.m_diPlaced.p_bIn || !m_loadport.m_diPresent.p_bIn)
               //{
               //    m_OHT.m_bPODExist = true;
               //}
               //else
               //{
               //    m_OHT.m_bPODExist = false;
               //}
                m_OHT.m_doLoadReq.p_bOn = false;
                m_OHT.m_doUnloadReq.p_bOn = false;
                m_OHT.m_doReady.p_bOn = false;
                m_OHT.m_doES.p_bOn = true;
                m_OHT.m_bAuto = true;
                //if((m_loadport.m_diPlaced.p_bIn || !m_loadport.m_diPresent.p_bIn) && m_OHT.p_eState == OHT_Semi.eState.All_Off) //KHD 210317
                //{
                //    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToUnload;
                //}
            }
        }
        #endregion
        #region OHT State
        void TimerLoadportState()
        {
            blockTransferState.Text = m_carrier.p_eTransfer.ToString();
            SetBrush(buttonStateLoading, m_OHT.m_doLoadReq.p_bOn && p_bBlink);
            SetBrush(buttonStateUnloading, m_OHT.m_doUnloadReq.p_bOn && p_bBlink);
            textBlockPlaced.Foreground = !m_loadport.m_diPlaced.p_bIn ? Brushes.White : Brushes.Gray;
            textBlockPresent.Foreground = !m_loadport.m_diPresent.p_bIn ? Brushes.White : Brushes.Gray;
            m_OHT.m_bPlaced = m_loadport.m_diPlaced.p_bIn;
            m_OHT.m_bPresent = m_loadport.m_diPresent.p_bIn;

            if (!m_OHT.m_bAuto && !m_OHT.m_bOHTErr)
            {
                if (m_loadport.p_eState == ModuleBase.eState.Home)
                {
                    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                    if (m_OHT.P_bProtectionBar || m_OHT.p_bLightCurtain)
                    {
                        if (m_OHT.P_bProtectionBar)
                        {
                            m_OHT.p_sInfo = "ProtectionBar detect Error";
                            
                        }
                        else
                        {
                            m_OHT.p_sInfo = "LightCurtain detect Error";
                            
                        }
                    }
                }
                else
                {
                    if (m_OHT.P_bProtectionBar || m_OHT.p_bLightCurtain)
                    {
                        if (m_OHT.P_bProtectionBar)
                        {
                            m_OHT.p_sInfo = "ProtectionBar detect Error";
                            m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                        }
                        else
                        {
                            m_OHT.p_sInfo = "LightCurtain detect Error";
                            m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                        }
                    }
                    else
                    {
                        if (!m_loadport.m_diPlaced.p_bIn && !m_loadport.m_diPresent.p_bIn && m_carrier.p_eTransfer != GemCarrierBase.eTransfer.ReadyToUnload)
                        {
                            if (m_carrier.eBeforTransfer == GemCarrierBase.eTransfer.ReadyToUnload)
                            {
                                m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToUnload;
                            }
                            else
                            {
                                m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                            }
                        }
                        else if (!m_loadport.m_diPlaced.p_bIn && !m_loadport.m_diPresent.p_bIn && m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload)
                        {
                        }
                        else if (m_loadport.m_diPlaced.p_bIn && m_loadport.m_diPresent.p_bIn)
                        {
                            m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToLoad;
                        }
                        else
                        {
                            m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                        }
                        m_OHT.p_sInfo = "OK";
                    }
                }   
            }
            if (/*m_OHT.p_eState == OHT_Semi.eState.All_Off &&*/ !m_OHT.m_bOHTErr && (!m_loadport.m_diPlaced.p_bIn && !m_loadport.m_diPresent.p_bIn)
                && m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad && m_OHT.p_eState == OHT_Semi.eState.Busy_On)
            {
                //m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Exist;
                m_OHT.m_bPODExist = true;
            }
            else if (!m_OHT.m_bOHTErr && m_loadport.m_diPlaced.p_bIn && m_loadport.m_diPresent.p_bIn &&
                m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload && m_OHT.p_eState == OHT_Semi.eState.Busy_On)
            {
                //m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Empty;
                m_OHT.m_bPODExist = false;
                m_OHT.m_bStartTD3 = false;
            }
            if (m_OHT.p_eState == OHT_Semi.eState.Busy_On)
            {
                if (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad)
                {
                    if ((!m_loadport.m_diPlaced.p_bIn || !m_loadport.m_diPresent.p_bIn) && !m_OHT.m_bStartTD3)
                    {
                        m_OHT.StartTD3();
                        m_OHT.m_bStartTD3 = true;
                    }
                    if (!m_loadport.m_diPlaced.p_bIn && !m_loadport.m_diPresent.p_bIn && m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad)
                    {
                        m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Exist;
                        m_OHT.m_bPODExist = true;
                        m_OHT.m_bStartTD3 = false;
                    }
                }
                if (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload)
                {
                    if ((m_loadport.m_diPlaced.p_bIn || m_loadport.m_diPresent.p_bIn) && !m_OHT.m_bStartTD3)
                    {
                        m_OHT.StartTD3();
                        m_OHT.m_bStartTD3 = true;
                    }
                    else if (m_loadport.m_diPlaced.p_bIn && m_loadport.m_diPresent.p_bIn && m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload)
                    {
                        m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Empty;
                        m_OHT.m_bPODExist = false;
                        m_OHT.m_bStartTD3 = false;
                    }
                }
                if (!m_OHT.m_bOHTErr)
                {
                    string Info = m_OHT.CheckTD3();
                    if (Info != "OK")
                    {
                        m_OHT.p_sInfo = Info;
                        m_OHT.m_bOHTErr = true;
                    }
                }
            }
            else if (m_OHT.p_eState == OHT_Semi.eState.LU_Req_Off || m_OHT.p_eState == OHT_Semi.eState.Ready_Off/*|| m_OHT.p_eState == OHT_Semi.eState.Ready_Off*/)
            {
                if (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad)
                {
                    if (m_loadport.m_diPlaced.p_bIn || m_loadport.m_diPresent.p_bIn)
                    {
                        //m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Empty;
                        m_OHT.m_bPODExist = false;
                    }
                    else
                    {
                        //m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Exist;
                        m_OHT.m_bPODExist = true;
                    }
                }
                else if (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload)
                {
                    if (!m_loadport.m_diPlaced.p_bIn || !m_loadport.m_diPresent.p_bIn)
                    {
                        //m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Exist;
                        m_OHT.m_bPODExist = true;
                    }
                    else
                    {
                        //m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Empty;
                        m_OHT.m_bPODExist = false;
                    }
                }
            }
            /*else if(m_OHT.p_eState == OHT_Semi.eState.Ready_Off)
            {
                if (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad)
                {
                    if (m_loadport.m_diPlaced.p_bIn || m_loadport.m_diPresent.p_bIn)
                    {
                        //m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Empty;
                        m_OHT.m_bPODExist = false;
                    }
                    else
                    {
                        //m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Exist;
                        m_OHT.m_bPODExist = true;
                    }
                }
                else if (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload)
                {
                    if (!m_loadport.m_diPlaced.p_bIn || !m_loadport.m_diPresent.p_bIn)
                    {
                        //m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Exist;
                        m_OHT.m_bPODExist = true;
                    }
                    else
                    {
                        //m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Empty;
                        m_OHT.m_bPODExist = false;
                    }
                }
            }*/

           //if (m_loadport.p_infoCarrier.p_eReqAccessLP == GemCarrierBase.eAccessLP.Manual)
           //{
           //    //if (m_carrier.p_eTransfer != GemCarrierBase.eTransfer.TransferBlocked)
           //    //{
           //    //    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked;
           //    //}   
           //    //if(!m_loadport.m_diPlaced.p_bIn || !m_loadport.m_diPresent.p_bIn)
           //    //{
           //    //    m_carrier.p_ePresentSensor = GemCarrierBase.ePresent.Exist;
           //    //}
           //    //blockTransferState.Text = "TransferBlocked";
           //    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked; //KHD 210317
           //}
            bool bPodIn = p_bBlink ? !m_loadport.m_diPlaced.p_bIn : !m_loadport.m_diPresent.p_bIn;
            imageInPod.Visibility = bPodIn ? Visibility.Visible : Visibility.Hidden;
            imageOutPod.Visibility = bPodIn ? Visibility.Hidden : Visibility.Visible;
            //if (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToLoad)
            //{
            //    m_loadport.m_bLoadCheck = true;
            //    m_loadport.m_bUnLoadCheck = false;
            //}
            //else if (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload)
            //{
            //    m_loadport.m_bLoadCheck = false;
            //    m_loadport.m_bUnLoadCheck = true;
            //}
        }

        


        void SetBrush(Button button, bool bOn)
        {
            button.Foreground = bOn ? Brushes.Red : Brushes.Black;
            button.Background = bOn ? Brushes.Yellow : Brushes.DimGray;
        }

        #endregion

        #region OHT DI
        void TimerDI()
        {
            SetBrushDI(buttonDIValid, m_OHT.m_diValid);
            SetBrushDI(buttonDICS0, m_OHT.m_diCS[0]);
            SetBrushDI(buttonDICS1, m_OHT.m_diCS[1]);
            SetBrushDI(buttonDITrReq, m_OHT.m_diTrReq);
            SetBrushDI(buttonDIBusy, m_OHT.m_diBusy);
            SetBrushDI(buttonDICompt, m_OHT.m_diComplete);
            SetBrushDI(buttonDICont, m_OHT.m_diContinue);
        }

        void SetBrushDI(Button button, OHTBase.DI di)
        {
            bool bOn = di.p_bOn;
            button.Foreground = bOn ? Brushes.Red : Brushes.Black;
            if (di.p_bWait) bOn = p_bBlink ? bOn : !bOn;
            button.Background = bOn ? Brushes.Yellow : Brushes.DimGray;
        }
        #endregion

        #region AccessMode
        bool m_bOHTErr = true;
        bool bProtectError = false;
        bool m_bErrorState = false;
        void TimerOHTErr()
        {
            if (m_OHT.m_bOHTErr != m_bOHTErr)
            {
                if (!m_OHT.m_bOHTErr)
                {
                    if (bProtectError)
                    {
                        if ((m_loadport.m_diPlaced.p_bIn || m_loadport.m_diPresent.p_bIn) && m_OHT.p_eState == OHT_Semi.eState.All_Off)
                        {
                            m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToLoad;
                        }
                        else if ((!m_loadport.m_diPlaced.p_bIn && !m_loadport.m_diPresent.p_bIn) && m_OHT.p_eState == OHT_Semi.eState.All_Off)
                        {
                            m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToUnload;
                        }
                        buttonRetry.IsEnabled = false;
                        grPioState.Background = null;
                        grErrState.Background = null;
                        bProtectError = false;
                    }
                    else
                    {
                        buttonRetry.IsEnabled = false;
                        grPioState.Background = null;
                        grErrState.Background = null;
                    }

                }
                else
                {

                    if (m_OHT.m_bProtectError)
                    {
                        bProtectError = m_OHT.m_bProtectError;
                    }
                    if (m_carrier.p_eTransfer == GemCarrierBase.eTransfer.ReadyToUnload)
                    {
                        m_bErrorState = true;
                    }
                    else
                    {
                        m_bErrorState = false;
                    }
                    buttonRetry.IsEnabled = true;
                    m_OHT.m_doHoAvailable.p_bOn = false;
                    m_OHT.m_doES.p_bOn = false;
                    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                    grPioState.Background = Brushes.Coral;
                    grErrState.Background = Brushes.Coral;
                }
            }
            if (m_OHT.m_bOHTErr)
            {
                buttonRetry.IsEnabled = true;
                m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                grPioState.Background = Brushes.Coral;
                grErrState.Background = Brushes.Coral;
            }
            m_bOHTErr = m_OHT.m_bOHTErr;
        }

        
        #endregion
        #region OHT DO
        void TimerDO()
        {
            SetBrushDO(buttonDOLReq, m_OHT.m_doLoadReq);
            SetBrushDO(buttonDOUReq, m_OHT.m_doUnloadReq);
            SetBrushDO(buttonDOReady, m_OHT.m_doReady);
            SetBrushDO(buttonDOHoAvbl, m_OHT.m_doHoAvailable);
            SetBrushDO(buttonDOES, m_OHT.m_doES);
        }

        void SetBrushDO(Button button, OHTBase.DO dio)
        {
            bool bOn = dio.p_bOn;
            button.Foreground = bOn ? Brushes.Red : Brushes.Black;
            if (dio.p_bWait) bOn = p_bBlink ? bOn : !bOn;
            button.Background = bOn ? Brushes.Yellow : Brushes.DimGray;
        }

        private void buttonDOLReq_Click(object sender, RoutedEventArgs e)
        {
            m_OHT.m_doLoadReq.Toggle();
        }

        private void buttonDOUReq_Click(object sender, RoutedEventArgs e)
        {
            m_OHT.m_doUnloadReq.Toggle();
        }

        private void buttonDOReady_Click(object sender, RoutedEventArgs e)
        {
            m_OHT.m_doReady.Toggle();
        }

        private void buttonDOHoAvbl_Click(object sender, RoutedEventArgs e)
        {
            m_OHT.m_doHoAvailable.Toggle();
        }

        private void buttonDOES_Click(object sender, RoutedEventArgs e)
        {
            m_OHT.m_doES.Toggle();
        }
        #endregion

        #region OHT p_sInfo
        private void buttonRetry_Click(object sender, RoutedEventArgs e)
        {
            if (EQ.p_bStop == true || EQ.p_eState == EQ.eState.Error)
            {
                return;
            }
            //if (m_loadport.m_diPlaced.p_bIn != m_loadport.m_diPresent.p_bIn)
            //{
            //    m_OHT.p_sInfo = m_OHT.p_id + " Access Violation \n(Carrier is placed incorrectly. \nRemove this or load stable.)";
            //    return;
            //}
            if (m_OHT.m_bSystemError)
            {
                if (EQ.p_eState == EQ.eState.Error || m_OHT.P_bProtectionBar || m_OHT.p_bLightCurtain || m_loadport.m_diPlaced.p_bIn != m_loadport.m_diPresent.p_bIn || !m_OHT.m_bSignalError && /*!m_OHT.m_bTimeOutError && !m_OHT.m_bSensorError && */m_OHT.m_diCS[0].p_bOn || m_OHT.m_diValid.p_bOn
                        || m_OHT.m_diTrReq.p_bOn || m_OHT.m_diBusy.p_bOn || m_OHT.m_diComplete.p_bOn || m_OHT.m_diContinue.p_bOn)
                {
                    return;
                }
                else
                {
                    OHTClear();
                    return;
                }
            }
            if (m_OHT.m_bSensorError || m_OHT.m_bSignalError)
            {
                if (EQ.p_eState == EQ.eState.Error || m_OHT.P_bProtectionBar || m_OHT.p_bLightCurtain || m_loadport.m_diPlaced.p_bIn != m_loadport.m_diPresent.p_bIn ||
                    /*(!m_OHT.m_bSignalError && !m_OHT.m_bTimeOutError && !m_OHT.m_bSensorError) ||*/ m_OHT.m_diCS[0].p_bOn || m_OHT.m_diValid.p_bOn
                        || m_OHT.m_diTrReq.p_bOn || m_OHT.m_diBusy.p_bOn || m_OHT.m_diComplete.p_bOn || m_OHT.m_diContinue.p_bOn)
                {
                    return;
                }
                else
                {
                    OHTClear();
                    return;
                }
            }
            if (m_OHT.m_bTimeOutError)
            {
                if (EQ.p_eState == EQ.eState.Error || m_OHT.P_bProtectionBar || m_OHT.p_bLightCurtain || m_loadport.m_diPlaced.p_bIn != m_loadport.m_diPresent.p_bIn ||
                    /*!m_OHT.m_bSignalError || !m_OHT.m_bTimeOutError || !m_OHT.m_bSensorError &&*/ m_OHT.m_diCS[0].p_bOn || m_OHT.m_diValid.p_bOn
                        || m_OHT.m_diTrReq.p_bOn || m_OHT.m_diBusy.p_bOn || m_OHT.m_diComplete.p_bOn || m_OHT.m_diContinue.p_bOn)
                {
                    return;
                }
                else
                {
                    OHTClear();
                    return;
                }
            }
            if (m_OHT.m_bCSProtectError)
            {
                if (EQ.p_eState == EQ.eState.Error || m_loadport.m_diPlaced.p_bIn != m_loadport.m_diPresent.p_bIn || /* !m_OHT.m_bSignalError&& !m_OHT.m_bTimeOutError && !m_OHT.m_bSensorError && */ m_OHT.m_diCS[0].p_bOn || m_OHT.m_diValid.p_bOn
                        || m_OHT.m_diTrReq.p_bOn || m_OHT.m_diBusy.p_bOn || m_OHT.m_diComplete.p_bOn || m_OHT.m_diContinue.p_bOn)
                {
                    return;
                }
                else
                {
                    OHTClear();
                    return;
                }
            }
        }
        #endregion
        public void OHTClear()
        {
            if ((!m_OHT.m_bSignalError && !m_OHT.m_bTimeOutError && !m_OHT.m_bSensorError && !m_OHT.m_bCSProtectError && !m_OHT.m_bSystemError) && (m_OHT.m_diCS[0].p_bOn || m_OHT.m_diValid.p_bOn || m_OHT.m_diTrReq.p_bOn || m_OHT.m_diBusy.p_bOn || m_OHT.m_diComplete.p_bOn || m_OHT.m_diContinue.p_bOn))
            {
                m_OHT.m_bOHTErr = true;
                m_OHT.AutoCheckDI(m_OHT.m_diValid);
                m_OHT.AutoCheckDI(m_OHT.m_diCS[0]);
                m_OHT.AutoCheckDI(m_OHT.m_diTrReq);
                m_OHT.AutoCheckDI(m_OHT.m_diBusy);
                m_OHT.AutoCheckDI(m_OHT.m_diComplete);
                m_OHT.AutoCheckDI(m_OHT.m_diContinue);
                return;
            }

            else if (!m_OHT.P_bProtectionBar && !m_OHT.p_bLightCurtain)
            {
                m_OHT.m_doLoadReq.p_bOn = false;
                m_OHT.m_doUnloadReq.p_bOn = false;
                m_OHT.m_doReady.p_bOn = false;
                m_OHT.p_sInfo = "";
                m_OHT.m_bOHTErr = false;
                m_OHT.ResetTD3();
                m_OHT.m_bStartTD3 = false;
                m_OHT.m_bSignalError = false;
                m_OHT.m_bSensorError = false;
                m_OHT.m_bTimeOutError = false;
                m_OHT.m_bProtectError = false;
                m_OHT.m_bSystemError = false;
                m_OHT.p_eState = OHT_Semi.eState.All_Off;
                if (m_loadport.m_diPlaced.p_bIn && m_loadport.m_diPresent.p_bIn && m_OHT.p_eState == OHT_Semi.eState.All_Off)
                //if(m_loadport.m_diPlaced.p_bIn && m_loadport.m_diPresent.p_bIn)
                {
                    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToLoad;
                    m_OHT.m_bPODExist = false; //KHD 210407
                }
                else if (!m_loadport.m_diPlaced.p_bIn && !m_loadport.m_diPresent.p_bIn && m_OHT.p_eState == OHT_Semi.eState.All_Off)
                {
                    //m_OHT. = false;
                    //m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                    if (!m_bErrorState)
                    {
                        m_carrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                        m_OHT.m_bPODExist = true; //KHD 210407
                        m_OHT.m_bRFIDRead = true;

                    }
                    else
                    {
                        m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToUnload;
                        m_OHT.m_bPODExist = true; //KHD 210407
                        m_OHT.m_bRFIDRead = true;
                    }
                }
                else if (!m_OHT.m_bPODExist && m_OHT.p_eState == OHT_Semi.eState.All_Off)
                {
                    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToLoad;
                }
                else if (m_OHT.m_bPODExist && m_OHT.p_eState == OHT_Semi.eState.All_Off)
                {
                    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToUnload;
                }

                //else if(!m_loadport.m_bPlaced && m_OHT.p_eState == OHT_Semi.eState.All_Off)
                //{
                //    m_carrier.p_eTransfer = GemCarrierBase.eTransfer.ReadyToUnload;
                //}
                if (m_OHT.p_eState == OHT_Semi.eState.All_Off)
                {
                    m_OHT.m_doLoadReq.p_bOn = false;
                    m_OHT.m_doUnloadReq.p_bOn = false;
                    m_OHT.m_doReady.p_bOn = false;
                }
            }
            else
            {
                if (m_OHT.P_bProtectionBar)
                {
                    m_OHT.p_sInfo = "ProtectionBar detect Error";
                }
                else
                {
                    m_OHT.p_sInfo = "LightCurtain detect Error";
                }
            }
        }
        #region Timer
        int _nBlink = 0;
        bool p_bBlink { get { return _nBlink < 5; } }

        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            TimerAccessMode();
            TimerLoadportState();
            TimerDI();
            TimerDO();
            TimerOHTErr();
            //_nBlink = (_nBlink + 1) % 10;
        }


        Loadport_Cymechs m_loadport;
        OHT_Semi m_OHT;
        GemCarrierBase m_carrier = null;
        public void Init(Loadport_Cymechs loadport)
        {
            m_loadport = loadport;
            m_OHT = loadport.m_OHT;
            m_carrier = loadport.p_infoCarrier;
            this.DataContext = loadport.m_OHT;
            GemCarrierBase.eTransfer tt1 = loadport.m_OHT.m_carrier.p_eTransfer;
            m_timer.Interval = TimeSpan.FromSeconds(0.1);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }
        #endregion
    }
}
