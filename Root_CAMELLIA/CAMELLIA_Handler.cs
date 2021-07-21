using Root_CAMELLIA;
using Root_CAMELLIA.Module;
using Root_EFEM.Module;
using Root_EFEM;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using Root_CAMELLIA.ManualJob;
using RootTools.OHTNew;
using Root_CAMELLIA.UI_UserControl;
using System.Windows;
using System;

namespace Root_CAMELLIA
{
    public class CAMELLIA_Handler : NotifyProperty, IHandler
    {
        public delegate void EventHandler();
        public event EventHandler OnRnRDone;
        void DoneEvent()
        {
            if (OnRnRDone != null)
                OnRnRDone();
        }

        public event EventHandler OnListUpdate;
        public void UpdateEvent()
        {
            if (OnListUpdate != null)
                OnListUpdate();
        }
        public ModuleList p_moduleList
        {
            get; set;
        }
        #region List InfoWafer
        //public string AddInfoWafer(InfoWafer infoWafer)
        //{
        //    return "OK";
        //}
        #endregion

        #region UI Binding
        public Brush p_brushHandler
        {
            get
            {
                return Brushes.MediumAquamarine;
            }
            set
            {
            }
        }

        public Brush p_brushModule
        {
            get
            {
                return Brushes.BurlyWood;
            }
            set
            {
            }
        }

        #endregion

        #region Module
        public ModuleList m_moduleList;
        public CAMELLIA_Recipe m_recipe;
        //public CAMELLIA_Process m_process;
        public CAMELLIA_Process p_process { get; set; }
        public Module_Camellia m_camellia;
        public HomeProgress_UI m_HomeProgress = new HomeProgress_UI();
        public Module_FDC m_FDC;
        public Module_FDC m_FDC_Vision;
        public Module_FFU m_FFU;
        public TowerLamp m_towerlamp;
        public Interlock m_interlock;
        void InitModule()
        {
            m_moduleList = new ModuleList(m_engineer);
            InitWTR();
            InitLoadport();
            InitRFID();
            InitAligner();
            m_camellia = new Module_Camellia("Camellia", m_engineer, m_aLoadport);
            InitModule(m_camellia);
            m_HomeProgress.Init(this);
            m_towerlamp = new TowerLamp("Towerlamp", m_engineer);
            InitModule(m_towerlamp);
            m_interlock = new Interlock("Interlock", m_engineer);
            InitModule(m_interlock);
            IWTR iWTR = (IWTR)m_wtr;
            iWTR.AddChild(m_camellia);
            m_wtr.RunTree(Tree.eMode.RegRead);
            m_wtr.RunTree(Tree.eMode.Init);
            iWTR.ReadInfoReticle_Registry();

            m_FDC = new Module_FDC("FDC", m_engineer);
            InitModule(m_FDC);
            m_FDC_Vision = new Module_FDC("FDC_Vision", m_engineer);
            InitModule(m_FDC_Vision);
            m_FFU = new Module_FFU("FFU", m_engineer);
            InitModule(m_FFU);
            m_recipe = new CAMELLIA_Recipe("Recipe", m_engineer);
            foreach (ModuleBase module in m_moduleList.m_aModule.Keys) m_recipe.AddModule(module);
            p_process = new CAMELLIA_Process("Process", m_engineer, iWTR, m_aLoadport);
        }

        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module); 
            m_moduleList.AddModule(module, ui);
        }

        public bool IsEnableRecovery()
        {
            if (EQ.p_eState != EQ.eState.Ready)
                return false;
            IWTR iWTR = (IWTR)m_wtr;
            //foreach (IWTRChild child in iWTR.p_aChild)
            //{
            //    if (child.p_infoWafer != null)
            //    {
            //        if (!child.p_id.Contains("Loadport"))
            //        {
            //            if (child.IsWaferExist(0) == false) return false;
            //            if (child.IsWaferExist(0) == true) return true;
            //        }
            //    }
            //    else if (child.p_infoWafer == null)
            //    {
            //        if (!child.p_id.Contains("Loadport"))
            //        {
            //            if (child.IsWaferExist(0) == true)
            //            {
            //                child.SetAlarm();
            //                return false;
            //            }
            //        }
            //    }
            //}
            //bool isRecovery = m_bIsPossible_Recovery;
            //if (m_IsCheckWTR)
            //{
            //    isRecovery = iWTR.IsEnableRecovery();
            //}

            return m_bIsPossible_Recovery;
        }
        #endregion

        #region Module WTR
        enum eWTR
        {
            RND,
            Cymechs
        }
        eWTR m_eWTR = eWTR.RND;
        public ModuleBase m_wtr;
        void InitWTR()
        {
            switch (m_eWTR)
            {
                case eWTR.Cymechs: m_wtr = new WTR_Cymechs("WTR", m_engineer); break;
                default: m_wtr = new WTR_RND("WTR", m_engineer); break;
            }
            InitModule(m_wtr);
        }

        public void RunTreeWTR(Tree tree)
        {
            m_eWTR = (eWTR)tree.Set(m_eWTR, m_eWTR, "Type", "WTR Type");
        }
        #endregion

        #region Module Loadport
        enum eLoadport
        {
            RND,
            Cymechs,
        }
        List<eLoadport> m_aLoadportType = new List<eLoadport>();
        public List<ILoadport> m_aLoadport = new List<ILoadport>();
        public List<ModuleBase> m_loadport = new List<ModuleBase>();
        int m_lLoadport = 2;
        void InitLoadport()
        {
            ModuleBase module;
            char cLP = 'A';
            for (int n = 0; n < m_lLoadport; n++, cLP++)
            {
                string sID = "Loadport" + cLP;
                switch (m_aLoadportType[n])
                {
                    case eLoadport.RND: module = new Loadport_RND(sID, m_engineer, true, true); break;
                    case eLoadport.Cymechs: module = new Loadport_Cymechs(sID, m_engineer, true, true); break;
                    default: module = new Loadport_RND(sID, m_engineer, true, true); break;
                }
                InitModule(module);
                m_loadport.Add(module);
                m_aLoadport.Add((ILoadport)module);
                ((IWTR)m_wtr).AddChild((IWTRChild)module);
            }
        }

        public void RunTreeLoadport(Tree tree)
        {
            m_lLoadport = tree.Set(m_lLoadport, m_lLoadport, "Count", "Loadport Count");
            while (m_aLoadportType.Count < m_lLoadport) m_aLoadportType.Add(eLoadport.RND);
            Tree treeType = tree.GetTree("Type");
            for (int n = 0; n < m_lLoadport; n++)
            {
                m_aLoadportType[n] = (eLoadport)treeType.Set(m_aLoadportType[n], m_aLoadportType[n], n.ToString("00"), "Loadport Type");
            }
        }
        #endregion

        #region Module RFID
        public List<IRFID> m_aRFID = new List<IRFID>();
        void InitRFID()
        {
            ModuleBase module;
            char cID = 'A';
            for (int n = 0; n < m_lLoadport; n++, cID++)
            {
                string sID = "RFID" + cID;
                module = new RFID_Brooks(sID, m_engineer, m_aLoadport[n]);
                InitModule(module);
                m_aRFID.Add((IRFID)module);
                m_aLoadport[n].m_rfid = m_aRFID[n];
            }
        }
        #endregion

        #region Module Gem
        public ModuleBase m_XGem = null;
        void InitXGem()
        {
            m_XGem = new Gem_XGem300Pro("Gem300", m_engineer, m_gem, m_aLoadport);
            InitModule(m_XGem);
        }
        #endregion

        #region Module Aligner
        enum eAligner
        {
            None,
            ATI,
            RND
        }
        eAligner m_eAligner = eAligner.ATI;
        public ModuleBase m_Aligner = null;
        void InitAligner()
        {
            
         switch (m_eAligner)
            {
                case eAligner.ATI: m_Aligner = new Aligner_ATI("Aligner", m_engineer); break;
                case eAligner.RND: m_Aligner = new Aligner_RND("Aligner", m_engineer); break;
            }
            if (m_Aligner != null)
            {
                InitModule(m_Aligner);
                ((IWTR)m_wtr).AddChild((IWTRChild)m_Aligner);
            }
        }

        public void RunTreeAligner(Tree tree)
        {
            m_eAligner = (eAligner)tree.Set(m_eAligner, m_eAligner, "Type", "Aligner Type");
        }
        #endregion

        #region Tree
        public void RunTreeModule(Tree tree)
        {
            RunTreeWTR(tree.GetTree("WTR"));
            RunTreeLoadport(tree.GetTree("Loadport"));
            RunTreeAligner(tree.GetTree("Aligner"));
        }
        #endregion

        #region StateHome
        public bool m_bIsPossible_Recovery = false;
        public bool m_IsCheckWTR = false;
        public string StateHome()
        {
            m_HomeProgress.Reset();
            m_HomeProgress.HomeProgressShow(); // 여기 수정필요
            string sInfo = StateHome(m_wtr);
            if(sInfo != "OK")
            {
                EQ.p_eState = EQ.eState.Init;
                return sInfo;
            }
            sInfo = StateHome((Loadport_RND)m_aLoadport[0], (Loadport_RND)m_aLoadport[1], m_Aligner, m_camellia, (RFID_Brooks)m_aRFID[0], (RFID_Brooks)m_aRFID[1]);
            if (sInfo == "OK") EQ.p_eState = EQ.eState.Ready;

            if (m_gem != null)
            {
                m_gem.DeleteAllJobInfo();
            }

            //m_bIsPossible_Recovery = false;
            IWTR iWTR = (IWTR)m_wtr;
            //m_bIsPossible_Recovery = iWTR.IsEnableRecovery();

            bool needRecovery = false;
            foreach (IWTRChild child in iWTR.p_aChild)
            {
                if (child.p_infoWafer != null)
                {
                    if (!child.p_id.Contains("Loadport"))
                    {
                        if (child.IsWaferExist(0) == false)
                        {
                            child.SetAlarm();
                            m_bIsPossible_Recovery = false;
                            return "Wafer Check Error";
                        }
                        else
                        {
                            needRecovery = true;
                        }
                    }
                }
                else if (child.p_infoWafer == null)
                {
                    if (!child.p_id.Contains("Loadport"))
                    {
                        if (child.IsWaferExist(0) == true)
                        {
                            child.SetAlarm();
                            m_bIsPossible_Recovery = false;
                            return "Wafer Check Error";
                        }
                    }
                }
            }


            bool wtrRecovery = false;
            if (!needRecovery)
                wtrRecovery = iWTR.IsEnableRecovery();

            m_bIsPossible_Recovery = needRecovery || wtrRecovery;
            //m_bIsPossible_Recovery = ExistWaferInfoRecovery && NotExistWaferInfoRecovery;
            return sInfo;
        }

        protected string StateHome(params ModuleBase[] aModule)
        {
            List<ModuleBase> listModule = new List<ModuleBase>();
            foreach (ModuleBase module in aModule)
                listModule.Add(module);
            return StateHome(listModule);
        }

        protected string StateHome(Dictionary<ModuleBase, UserControl> aModule)
        {
            List<ModuleBase> listModule = new List<ModuleBase>();
            foreach (ModuleBase module in aModule.Keys)
                listModule.Add(module);
            return StateHome(listModule);
        }

        protected string StateHome(List<ModuleBase> aModule)
        {
            foreach (ModuleBase module in aModule)
                module.StartHome();
            bool bHoming = true;
            while (bHoming)
            {
                Thread.Sleep(10);
                bHoming = false;
                foreach (ModuleBase module in aModule)
                {
                    if (module.p_eState == ModuleBase.eState.Home)
                        bHoming = true;
                }
            }
            foreach (ModuleBase module in aModule)
            {
                if (module.p_eState != ModuleBase.eState.Ready)
                {
                    EQ.p_bStop = true;
                    EQ.p_eState = EQ.eState.Init;
                    return module.p_id + " Home Error";
                }
            }
            return "OK";
        }
        #endregion

        #region Reset
        public string Reset()
        {
            Reset(m_gaf, m_moduleList);
            return "OK";
        }

        void Reset(GAF gaf, ModuleList moduleList)
        {
            if (gaf != null)
                gaf.ClearALID();
            foreach (ModuleBase module in moduleList.m_aModule.Keys)
                module.Reset();
        }
        #endregion

        #region Calc Sequence
        dynamic m_infoRnRSlot;
        public string AddSequence(dynamic infoSlot)
        {
            //m_process.AddInfoWafer(infoSlot);
            m_infoRnRSlot = infoSlot;
            p_process.p_sInfo = p_process.AddInfoWafer(infoSlot);
            return "OK";
        }

        public void CalcSequence()
        {
            p_process.ReCalcSequence();
            CalcDockingUndocking();
        }

        public void CalcRecover()
        {
            p_process.CalcRecover();
            CalcDockingUndocking(true);
        }

        void CalcDockingUndocking(bool isRecovery = false)
        {
            List<CAMELLIA_Process.Sequence> aSequence = new List<CAMELLIA_Process.Sequence>();
            while (p_process.p_qSequence.Count > 0) aSequence.Add(p_process.p_qSequence.Dequeue());
            List<ILoadport> aDock = new List<ILoadport>();
            foreach (ILoadport loadport in m_aLoadport)
            {
                if (CalcDocking(loadport, aSequence))
                {
                    aDock.Add(loadport);
                    //if (!isRecovery)
                    //{
                    //    CalcInitCal(loadport, aSequence);
                    //}
                }
            }
            while (aSequence.Count > 0)
            {
                CAMELLIA_Process.Sequence sequence = aSequence[0];
                p_process.p_qSequence.Enqueue(sequence);
                aSequence.RemoveAt(0);
                for (int n = aDock.Count - 1; n >= 0; n--)
                //for (int n = m_process.m_qSequence.Count - 1; n >= 0; n--)
                {
                    if (CalcUnload(aDock[n], aSequence))
                    {
                        ModuleRunBase runUndocking = aDock[n].GetModuleRunUndocking().Clone();
                        CAMELLIA_Process.Sequence sequenceUndock = new CAMELLIA_Process.Sequence(runUndocking, sequence.m_infoWafer);
                        p_process.p_qSequence.Enqueue(sequenceUndock);
                        aDock.RemoveAt(n);
                    }
                }
            }
            p_process.RunTree(Tree.eMode.Init);
        }

        bool CalcInitCal(ILoadport loadport, List<CAMELLIA_Process.Sequence> aSequence)
        {
            foreach (CAMELLIA_Process.Sequence sequence in aSequence)
            {
                //if (loadport.p_id == sequence.m_infoWafer.m_sModule) return true; 
                if (loadport.p_id == sequence.m_infoWafer.m_sModule) //return true;
                {
                    ModuleRunBase runInitCal = (Run_InitCalibration)m_camellia.CloneModuleRun("InitCalibration");
                    CAMELLIA_Process.Sequence sequenceDock = new CAMELLIA_Process.Sequence(runInitCal, sequence.m_infoWafer);
                    p_process.p_qSequence.Enqueue(sequenceDock);
                    return true;
                }
            }
            return false;
        }

        bool CalcDocking(ILoadport loadport, List<CAMELLIA_Process.Sequence> aSequence)
        {
            foreach (CAMELLIA_Process.Sequence sequence in aSequence)
            {
                //if (loadport.p_id == sequence.m_infoWafer.m_sModule) return true; 
                if (loadport.p_id == sequence.m_infoWafer.m_sModule) //return true;
                {
                    if (loadport.p_infoCarrier.p_eState == InfoCarrier.eState.Dock) return true;
                    ModuleRunBase runDocking = loadport.GetModuleRunDocking().Clone();
                    CAMELLIA_Process.Sequence sequenceDock = new CAMELLIA_Process.Sequence(runDocking, sequence.m_infoWafer);
                    p_process.p_qSequence.Enqueue(sequenceDock);
                    return true;
                }
            }
            return false;
        }

        bool CalcUnload(ILoadport loadport, List<CAMELLIA_Process.Sequence> aSequence)
        {
            foreach (CAMELLIA_Process.Sequence sequence in aSequence)
            {
                if (loadport.p_id == sequence.m_infoWafer.m_sModule) return false;
            }
            return true;
        }
        #endregion

        #region IHandler
        public void CheckFinish()
        {
            if (m_gem.p_cjRun == null)
                return;
            if (p_process.p_qSequence.Count > 0)
                return;
            foreach (GemPJ pj in m_gem.p_cjRun.m_aPJ)
            {
                if (m_gem != null)
                    m_gem.SendPJComplete(pj.m_sPJobID);
                Thread.Sleep(100);
            }
        }

        public dynamic GetGemSlot(string sSlot)
        {
            foreach(ILoadport loadport in m_aLoadport)
            {
                foreach(GemSlotBase slot in loadport.p_infoCarrier.m_aGemSlot)
                {
                    if (slot.p_id == sSlot) return slot;
                }
            }
            return null;
        }
        #endregion

        #region Thread
        bool _bThread = false;
        public bool p_bThread 
        {
            get { return _bThread; }
            set 
            {
                if (_bThread == value) return;
                _bThread = value;
                OnPropertyChanged();
            }
        }

        Thread m_thread = null;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        public bool bLoad = false;
        void RunThread()
        {
            p_bThread = true;
            Thread.Sleep(100);
            while (p_bThread)
            {
                Thread.Sleep(10);
                switch (EQ.p_eState)
                {
                    case EQ.eState.Home:
                        StateHome();
                        break;
                    case EQ.eState.Run:
                        if (m_moduleList.m_qModuleRun.Count == 0)
                        {
                            p_process.p_sInfo = p_process.RunNextSequence();

                            if ((EQ.p_nRnR > 1) && (p_process.p_qSequence.Count == 0))
                            {
                                p_process.CopyRNRSeq();
                                //SetGemSlotRnR();
                                DoneEvent();
                                EQ.p_nRnR--;
                                EQ.p_eState = EQ.eState.Run;
                                
                            }
                            else if ((EQ.p_nRnR == 1) && (p_process.p_qSequence.Count == 1))
                            {
                                MarsLogManager.Instance.WriteLEH(EQ.p_nRunLP, m_aLoadport[EQ.p_nRunLP].p_infoCarrier.p_sLocID, SSLNet.LEH_EVENTID.PROCESS_JOB_END,
                                    EQ.p_nRunLP == 0 ? MarsLogManager.Instance.m_flowDataA : MarsLogManager.Instance.m_flowDataB,
                                    EQ.p_nRunLP == 0 ? MarsLogManager.Instance.m_dataFormatterA : MarsLogManager.Instance.m_dataFormatterB);
                            }
                            else if ((EQ.p_nRnR == 1) && (p_process.p_qSequence.Count == 0))
                            {
                                DoneEvent();
                                EQ.p_nRnR--;
                                m_RnRData.ClearData();
                            }
                        }
                        break;
                }
                //CheckFinish();
                //CheckFinish();
            }
        }
        /*
        void CheckLoad()
        {
            if (m_process.m_qSequence.Count == 0) return;
            EFEM_Process.Sequence sequence = m_process.m_qSequence.Peek();
            string sLoadport = sequence.m_infoWafer.m_sModule;
            foreach (ILoadport loadport in m_aLoadport)
            {
                if (loadport.p_id == sLoadport)
                {
                    //loadport.RunDocking();
                    if (loadport.RunDocking() != "OK") return;
                    if(EQ.p_bRecovery == false)
                    {
                        InfoCarrier infoCarrier = loadport.p_infoCarrier;
                        ManualJobSchedule manualJobSchedule = new ManualJobSchedule(infoCarrier);
                        manualJobSchedule.ShowPopup();
                    }
                }
            }
        }

        void CheckUnload()
        {
            EFEM_Process.Sequence[] aSequence = m_process.m_qSequence.ToArray();
            foreach (ILoadport loadport in m_aLoadport)
            {
                if (loadport.p_infoCarrier.p_eState == InfoCarrier.eState.Dock)
                {
                    string sLoadport = loadport.p_id;
                    bool bUndock = true;
                    foreach (EFEM_Process.Sequence sequence in aSequence)
                    {
                        if (sequence.m_infoWafer.m_sModule == sLoadport) bUndock = false;
                    }
                    if (bUndock) loadport.RunUndocking();
                }
            }
        } */
        #endregion

        string m_id;
        public CAMELLIA_Engineer m_engineer;
        public GAF m_gaf;
        public IGem m_gem;

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (CAMELLIA_Engineer)engineer;
            m_gaf = engineer.ClassGAF();
            m_gem = engineer.ClassGem();
            InitModule();
            InitThread();
        }

        public void ThreadStop()
        {

            if (p_bThread)
            {
                p_bThread = false;
                EQ.p_bStop = true;
                //m_thread.Join(); // 여기서 멈춰 프로그램 종료되지 않는 현상 있음.
            }
            if (m_moduleList != null)
            {
                m_moduleList.ThreadStop();
                foreach (ModuleBase module in m_moduleList.m_aModule.Keys)
                    module.ThreadStop();
            }
           
        }

        RnRData m_RnRData = new RnRData();
        public RnRData GetRnRData()
        {
            return m_RnRData;
        }
    }
}
