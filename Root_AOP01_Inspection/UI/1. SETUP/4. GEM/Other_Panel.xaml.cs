using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using RootTools.Gem;
using RootTools.Module;
using RootTools;
using static Root_AOP01_Inspection.AOP01_Handler;
using Root_AOP01_Inspection.Module;
using RootTools_Vision;
using RootTools.OHTNew;
using RootTools.GAFs;
using RootTools.Gem.XGem;

//namespace Root_AOP01_Inspection.UI._1._SETUP._4._GEM
namespace Root_AOP01_Inspection
{
    /// <summary>
    /// Other_Panel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Other_Panel : UserControl
    {
        public Other_Panel()
        {
            InitializeComponent();
        }

        InfoCarrier m_infoCarrier;
        public IEngineer m_engineer;
                
        public IHandler m_handle;
        ILoadport m_loadport;
        IGem m_gem;


        public void Init(IEngineer engineer, ILoadport loadport)
        {
            m_engineer = engineer;
            m_handle = engineer.ClassHandler();
            m_gem = engineer.ClassGem();
            m_loadport = loadport;

            m_infoCarrier = m_loadport.p_infoCarrier;
            InitTimer();
        }

        private void ButtonTestStart_Click(object sender, RoutedEventArgs e)
        {
            p_eGemTestStep = eGemTestStep.JobReserve;
        }

        private void ButtonTestEnd_Click(object sender, RoutedEventArgs e)
        {
            p_eGemTestStep = eGemTestStep.Ready;
        }

        public enum eGemTestStep
        {
            Ready,
            JobReserve,
            MaterialRecieve,
            CarrierIDRead,
            Docking,
            JobStart, 
            CJCreate,
            CheckStart,
            Blow,
            Insp,
            InspEnd, 
            CheckFinish,
            Undocking,
            MaterialRemove,
            Finish,
        }

        eGemTestStep _eGemTestStep = eGemTestStep.Ready;

        public eGemTestStep p_eGemTestStep
        {
            get { return _eGemTestStep; }
            set
            {
                if (_eGemTestStep == value) return;
                _eGemTestStep = value;
            }
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
              
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(10);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        void StopTimer()
        {
            m_timer.Stop();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            UpdateState();
            TimerGemTest();
        }

        public void UpdateState()
        {
            if (pnlTestStep != null)
            {
                pnlTestStep.Text = p_eGemTestStep.ToString();
            }
        }

        public void TimerGemTest()
        {
            if (p_eGemTestStep > eGemTestStep.Ready)
            {
                switch (p_eGemTestStep)
                {
                    case eGemTestStep.JobReserve:
                        {
                            m_gem.SendLPInfo(m_infoCarrier);
                            m_gem.SetCEID(8211);
                            
                            m_gem.SetCEID(8200); //OHT Start
                            //m_gem.CMSDelCarrierInfo(m_infoCarrier);

                            m_infoCarrier.p_ePresentSensor = GemCarrierBase.ePresent.Exist;
                            System.Threading.Thread.Sleep(100);
                            p_eGemTestStep = eGemTestStep.MaterialRecieve;
                            break;
                        }
                    case eGemTestStep.MaterialRecieve:
                        {
                            m_infoCarrier.p_eAccessLP = GemCarrierBase.eAccessLP.Manual;
                            System.Threading.Thread.Sleep(100);
                            
                            //m_infoCarrier.p_eTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                            System.Threading.Thread.Sleep(100);

                            p_eGemTestStep = eGemTestStep.CarrierIDRead;
                            break;
                        }
                    case eGemTestStep.CarrierIDRead:
                        {
                            m_gem.SetCEID(8201); 
                            System.Threading.Thread.Sleep(100);
                            p_eGemTestStep = eGemTestStep.Docking;
                            break;
                        }
                    case eGemTestStep.Docking:
                        {
                            GemDocking();

                            System.Threading.Thread.Sleep(1000);
                            p_eGemTestStep = eGemTestStep.JobStart;
                            break;
                        }
                    case eGemTestStep.JobStart:
                        {
                            m_infoCarrier.p_eAccess = GemCarrierBase.eAccess.InAccessed;
                            p_eGemTestStep = eGemTestStep.CJCreate;
                            break;
                        }
                    case eGemTestStep.CJCreate:
                        {
                            
                            while (m_gem.p_cjRun == null)
                            {
                                System.Threading.Thread.Sleep(10);
                            }
                            
                            p_eGemTestStep = eGemTestStep.CheckStart;
                            break;
                        }
                    case eGemTestStep.CheckStart:
                        {
                            foreach (GemPJ pj in m_gem.p_cjRun.m_aPJ)
                            {
                                if (pj.p_eState == GemPJ.eState.Processing)
                                {
                                    p_eGemTestStep = eGemTestStep.Blow;
                                    
                                }
                                else
                                {
                                    p_eGemTestStep = eGemTestStep.CheckStart;
                                }
                            }
                            break;
                        }
                    case eGemTestStep.Blow:
                        {
                            System.Threading.Thread.Sleep(2000);
                            m_gem.SetCEID(8205); //Pod Move To Vision

                            m_gem.SetCEID(8000); //Blow Start
                            
                            System.Threading.Thread.Sleep(2000);
                            m_gem.SetCEID(8001); // Blow End

                            p_eGemTestStep = eGemTestStep.Insp;
                            break;
                        }
                    case eGemTestStep.Insp:
                        {
                            //Insp Start/End
                            m_gem.SetCEID(8002); //Insp Start
                            
                            System.Threading.Thread.Sleep(2000);

                            m_gem.SetCEID(8003);
                            p_eGemTestStep = eGemTestStep.InspEnd;
                            break;
                        }
                    case eGemTestStep.InspEnd:
                        {
                            //Pod Move To UnloadPort - ResultData 보고 - PJ, CJ End
                            m_gem.SetCEID(8206);

                            m_gem.SetCEID(8300);



                            foreach (GemPJ pj in m_gem.p_cjRun.m_aPJ)
                            {
                                m_gem.SendPJComplete(pj.m_sPJobID);
                            }
                            System.Threading.Thread.Sleep(100);
                                                       

                            p_eGemTestStep = eGemTestStep.CheckFinish;
                            break;
                        }

                    case eGemTestStep.CheckFinish:
                        {
                            while (m_infoCarrier.p_eAccess != GemCarrierBase.eAccess.InAccessed)
                            {
                                System.Threading.Thread.Sleep(10);
                            }                        

                            if (m_gem.p_cjRun != null)
                            {
                                if (m_gem.p_cjRun.p_eState == GemCJ.eState.Completed)
                                {
                                    p_eGemTestStep = eGemTestStep.Undocking;
                                }
                                else
                                {
                                    p_eGemTestStep = eGemTestStep.CheckFinish;
                                }
                            }
                            else { p_eGemTestStep = eGemTestStep.Undocking; }
                            

                            break;
                        
                        }   
                    case eGemTestStep.Undocking:
                        {
                            GemUndocking();
                            System.Threading.Thread.Sleep(2000);

                            p_eGemTestStep = eGemTestStep.MaterialRemove;
                            break;
                        }
                    case eGemTestStep.MaterialRemove:
                        {
                            m_infoCarrier.p_eReqTransfer = GemCarrierBase.eTransfer.ReadyToUnload;

                            m_gem.SetCEID(8200); //OHT Start

                            //Material Remove
                            m_infoCarrier.p_ePresentSensor = GemCarrierBase.ePresent.Empty; // -> MeterialRemove - CarrierID Deleted

                            System.Threading.Thread.Sleep(100);
                            
                            //System.Threading.Thread.Sleep(100);
                            m_gem.SetCEID(8201); //OHT End
                            
                            m_infoCarrier.p_eStateSlotMap = GemCarrierBase.eGemState.NotRead;
                            System.Threading.Thread.Sleep(500);
                            p_eGemTestStep = eGemTestStep.Finish;
                            break;
                        }
                    case eGemTestStep.Finish:
                        { 
                            //OHT에서 Pod 가져가면 ReadyToUnload
                            m_infoCarrier.p_eReqTransfer = GemCarrierBase.eTransfer.TransferBlocked;
                            System.Threading.Thread.Sleep(500);
                            m_infoCarrier.p_eReqTransfer = GemCarrierBase.eTransfer.ReadyToLoad;
                            p_eGemTestStep = eGemTestStep.Ready;
                            break;
                        }
                }
            }
            
        }
        #endregion

        public string GemDocking()
        {
            //Docking 시작
            m_gem.SetCEID(8202); //Docking Start
            m_infoCarrier.p_eStateCarrierID = GemCarrierBase.eGemState.NotRead;
            m_infoCarrier.p_eStateSlotMap = GemCarrierBase.eGemState.NotRead;

            System.Threading.Thread.Sleep(100);

            m_infoCarrier.p_sCarrierID = "TestCarrier";
            System.Threading.Thread.Sleep(100);


            m_infoCarrier.SetReqAssociated(GemCarrierBase.eAssociated.Associated);

            m_infoCarrier.SendCarrierID(m_infoCarrier.p_sCarrierID); // NotAssociated -> Associated, WaitForHost

            while (m_infoCarrier.p_eStateCarrierID != GemCarrierBase.eGemState.VerificationOK)
            {
                System.Threading.Thread.Sleep(10);
            }
            

            m_gem.SetCEID(8203); //Docking 종료 보고

            System.Threading.Thread.Sleep(100);

            m_infoCarrier.SendSlotMap();
            System.Threading.Thread.Sleep(100);

            while (m_infoCarrier.p_eStateSlotMap != GemCarrierBase.eGemState.VerificationOK)
            {
                System.Threading.Thread.Sleep(10);
            }

            return "OK";
        }

        public string GemUndocking()
        {
            m_gem.SetCEID(8207); //Undocking Start
            
            System.Threading.Thread.Sleep(1000);

            
            m_infoCarrier.p_eState = InfoCarrier.eState.Placed;

            m_infoCarrier.SetReqAssociated(GemCarrierBase.eAssociated.NotAssociated);

            m_gem.SetCEID(8208); //Undocking End

            System.Threading.Thread.Sleep(100);

            return "OK";
        }
    }
}