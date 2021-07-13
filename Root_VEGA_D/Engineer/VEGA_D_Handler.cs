using Root_EFEM;
using Root_EFEM.Module;
using Root_VEGA_D.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using RootTools.OHTNew;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_VEGA_D.Engineer
{
    public class VEGA_D_Handler : IHandler
    {
        #region UI Binding
        public Brush p_brushHandler
        {
            get { return Brushes.MediumAquamarine; }
            set { }
        }

        public Brush p_brushModule
        {
            get { return Brushes.BurlyWood; }
            set { }
        }
        #endregion 

        #region Module
        public ModuleList p_moduleList { get; set; }
        public VEGA_D_Recipe m_recipe;
        public VEGA_D_Process m_process;
        public Vision m_vision;
        public HomeProgress_UI m_HomeProgress = new HomeProgress_UI();
        public Interlock m_interlock;
        public TowerLamp m_towerlamp;
        public FFU m_FFU;

        void InitModule()
        {
            p_moduleList = new ModuleList(m_engineer);
            InitWTR();
            IWTR iWTR = (IWTR)m_wtr;
            InitLoadport();
            InitRFID();
            InitAligner();
            m_vision = new Vision("Vision", m_engineer);
            InitModule(m_vision);
            iWTR.AddChild(m_vision);
            m_FFU = new FFU("FFU", m_engineer);
            InitModule(m_FFU);
            m_interlock = new Interlock("Interlock", m_engineer,m_engineer.m_ACS);
            InitModule(m_interlock);
            m_towerlamp = new TowerLamp("TowerLamp", m_engineer);
            InitModule(m_towerlamp);
            m_HomeProgress.Init(this);
            m_wtr.RunTree(Tree.eMode.RegRead);
            m_wtr.RunTree(Tree.eMode.Init);
            iWTR.ReadInfoReticle_Registry();
            m_recipe = new VEGA_D_Recipe("Recipe", m_engineer);
            foreach (ModuleBase module in p_moduleList.m_aModule.Keys) m_recipe.AddModule(module);
            m_process = new VEGA_D_Process("Process", m_engineer, iWTR);
            CalcRecover();
        }

        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module);
            p_moduleList.AddModule(module, ui);
        }

        public bool IsEnableRecovery()
        {
            IWTR iWTR = (IWTR)m_wtr;
            foreach (IWTRChild child in iWTR.p_aChild)
            {
                if (child.p_infoWafer != null) return true;
            }
            return iWTR.IsEnableRecovery();
        }
        #endregion

        #region Module WTR
        enum eWTR
        {
            RND,
            Cymechs,
        }
        eWTR m_eWTR = eWTR.Cymechs;
        public WTR_Cymechs m_wtr;
        void InitWTR()
        {
            m_wtr = new WTR_Cymechs("WTR", m_engineer);
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
                //string sID = "LP" + cLP;
                //switch (m_aLoadportType[n])
                //{
                //    case eLoadport.RND: module = new Loadport_RND(sID, m_engineer, true, true); break;
                //    case eLoadport.Cymechs: module = new Loadport_Cymechs(sID, m_engineer, true, false); break;
                //    default: module = new Loadport_RND(sID, m_engineer, true, true); break;
                //}
                m_aLoadportType[n] = eLoadport.Cymechs;
                module = new Loadport_Cymechs(sID, m_engineer, true, false);
                InitModule(module);
                m_loadport.Add(module);
                m_aLoadport.Add((ILoadport)module);
                ((IWTR)m_wtr).AddChild((IWTRChild)module);
            }
        }

        public List<IRFID> m_aRFID = new List<IRFID>();
        void InitRFID()
        {
            ModuleBase module;
            char cID = 'A';
            for(int n=0; n<m_lLoadport; n++, cID++)
            {
                string sID = "RFID" + cID;
                module = new RFID_Brooks(sID, m_engineer, m_aLoadport[n]);
                InitModule(module);
                m_aRFID.Add((IRFID)module);
                m_aLoadport[n].m_rfid = m_aRFID[n];
            }
        }

        public void RunTreeLoadport(Tree tree)
        {
            m_lLoadport = tree.Set(m_lLoadport, m_lLoadport, "Count", "Loadport Count");
            while (m_aLoadportType.Count < m_lLoadport) m_aLoadportType.Add(eLoadport.Cymechs);
            Tree treeType = tree.GetTree("Type");
            for (int n = 0; n < m_lLoadport; n++)
            {
                m_aLoadportType[n] = (eLoadport)treeType.Set(m_aLoadportType[n], m_aLoadportType[n], n.ToString("00"), "Loadport Type");
            }
        }
        #endregion

        #region Module Aligner
        enum eAligner
        {
            None,
            ATI,
            RND
        }
        eAligner m_eAligner = eAligner.None;
        void InitAligner()
        {
            ModuleBase module = null;
            switch (m_eAligner)
            {
                case eAligner.ATI: module = new Aligner_ATI("Aligner", m_engineer); break;
                case eAligner.RND: module = new Aligner_RND("Aligner", m_engineer); break;
            }
            if (module != null)
            {
                InitModule(module);
                ((IWTR)m_wtr).AddChild((IWTRChild)module);
            }
        }

        public void RunTreeAligner(Tree tree)
        {
            m_eAligner = (eAligner)tree.Set(m_eAligner, m_eAligner, "Type", "Aligner Type");
        }
        #endregion

        #region StateHome
        public bool m_bIsPossible_Recovery = false;
        public string StateHome()
        {
            m_HomeProgress.HomeProgressShow();


            //string sInfo = StateHome(p_moduleList.m_aModule);
            string sInfo = StateHome(m_wtr);
            if (sInfo != "OK")
            {
                EQ.p_eState = EQ.eState.Init;
                return sInfo;
            }
            if(!m_wtr.m_diArmClose.p_bIn) m_wtr.m_alidRTRArmError.Run(true, "RTR Arm is not close in home motion");
            sInfo = StateHome(m_interlock, (ModuleBase)m_aLoadport[0], (ModuleBase)m_aLoadport[1], m_vision, m_towerlamp, (RFID_Brooks)m_aRFID[0], (RFID_Brooks)m_aRFID[1], m_FFU);
            if (sInfo == "OK") EQ.p_eState = EQ.eState.Ready;
            if (sInfo == "OK") m_bIsPossible_Recovery = true;
            return sInfo;
        }

        protected string StateHome(params ModuleBase[] aModule)
        {
            List<ModuleBase> listModule = new List<ModuleBase>();
            foreach (ModuleBase module in aModule) listModule.Add(module);
            return StateHome(listModule);
        }

        protected string StateHome(Dictionary<ModuleBase, UserControl> aModule)
        {
            List<ModuleBase> listModule = new List<ModuleBase>();
            foreach (ModuleBase module in aModule.Keys) listModule.Add(module);
            return StateHome(listModule);
        }

        protected string StateHome(List<ModuleBase> aModule)
        {
            foreach (ModuleBase module in aModule) module.StartHome();
            bool bHoming = true;
            while (bHoming)
            {
                Thread.Sleep(10);
                bHoming = false;
                foreach (ModuleBase module in aModule)
                {
                    if (module.p_eState == ModuleBase.eState.Home) bHoming = true;
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
            Reset(m_gaf, p_moduleList);
            return "OK";
        }

        void Reset(GAF gaf, ModuleList moduleList)
        {
            gaf?.ClearALID();
            foreach (ModuleBase module in moduleList.m_aModule.Keys) module.Reset();
        }
        #endregion

        #region Calc Sequence
        public bool m_bIsRNR = false;
        public int p_nRnRCount = 0;
        dynamic m_infoRnRSlot;
        public string AddSequence(dynamic infoSlot)
        {
            m_infoRnRSlot = infoSlot;
            m_process.AddInfoWafer(infoSlot);
            return "OK";
        }

        public void CalcSequence()
        {
            m_process.ReCalcSequence();
            CalcDockingUndocking();
        }

        public void CalcRecover()
        {
            m_process.CalcRecover();
            CalcDockingUndocking();
        }

        void CalcDockingUndocking()
        {
            List<VEGA_D_Process.Sequence> aSequence = new List<VEGA_D_Process.Sequence>();
            while (m_process.m_qSequence.Count > 0) aSequence.Add(m_process.m_qSequence.Dequeue());
            List<ILoadport> aDock = new List<ILoadport>();
            foreach (ILoadport loadport in m_aLoadport)
            {
                if (CalcDocking(loadport, aSequence)) aDock.Add(loadport);
            }
            while (aSequence.Count > 0)
            {
                VEGA_D_Process.Sequence sequence = aSequence[0];
                m_process.m_qSequence.Enqueue(sequence);
                aSequence.RemoveAt(0);
                for (int n = aDock.Count - 1; n >= 0; n--)
                {
                    if (CalcUnload(aDock[n], aSequence))
                    {
                        ModuleRunBase runUndocking = aDock[n].GetModuleRunUndocking().Clone();
                        VEGA_D_Process.Sequence sequenceUndock = new VEGA_D_Process.Sequence(runUndocking, sequence.m_infoWafer);
                        m_process.m_qSequence.Enqueue(sequenceUndock);
                        aDock.RemoveAt(n);
                    }
                }
            }
            m_process.RunTree(Tree.eMode.Init);
        }

        bool CalcDocking(ILoadport loadport, List<VEGA_D_Process.Sequence> aSequence)
        {
            foreach (VEGA_D_Process.Sequence sequence in aSequence)
            {
                if (loadport.p_id == sequence.m_infoWafer.m_sModule) //return true;
                {
                    if (loadport.p_infoCarrier.p_eState == InfoCarrier.eState.Dock) return true;
                    ModuleRunBase runDocking = loadport.GetModuleRunDocking().Clone();
                    VEGA_D_Process.Sequence sequenceDock = new VEGA_D_Process.Sequence(runDocking, sequence.m_infoWafer);
                    m_process.m_qSequence.Enqueue(sequenceDock);
                    return true;
                }
            }
            return false;
        }

        bool CalcUnload(ILoadport loadport, List<VEGA_D_Process.Sequence> aSequence)
        {
            foreach (VEGA_D_Process.Sequence sequence in aSequence)
            {
                if (loadport.p_id == sequence.m_infoWafer.m_sModule) return false;
            }
            return true;
        }
        #endregion

        #region IHandler
        public void CheckFinish()
        {
            if (m_gem.p_cjRun == null) return;
            if (m_process.m_qSequence.Count > 0) return;
            foreach (GemPJ pj in m_gem.p_cjRun.m_aPJ)
            {
                m_gem?.SendPJComplete(pj.m_sPJobID);
                Thread.Sleep(100);
            }
        }

        public dynamic GetGemSlot(string sSlot)
        {
            foreach (ILoadport loadport in m_aLoadport)
            {
                foreach (GemSlotBase slot in loadport.p_infoCarrier.m_aGemSlot)
                {
                    if (slot.p_id == sSlot) return slot;
                }
            }
            return null;
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread = null;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }
        int m_nCount = 0;
        void RunThread()
        {
            m_bThread = true;
            Thread.Sleep(100);
            while (m_bThread)
            {
                Thread.Sleep(10);
                switch (EQ.p_eState)
                {
                    case EQ.eState.Home: StateHome(); break;
                    case EQ.eState.Run:
                        m_engineer.m_handler.m_bIsPossible_Recovery = false;
                        if (!m_wtr.m_diArmClose.p_bIn)
                        {
                            m_wtr.m_alidRTRArmError.Run(true, "RTR Arm is open in Cycle");
                            break;
                        }
                        if (p_moduleList.m_qModuleRun.Count == 0)
                        {
                            m_process.p_sInfo = m_process.RunNextSequence();
                            if ((EQ.p_nRnR > 1) && (m_process.m_qSequence.Count == 0))
                            {
                                m_nCount = EQ.p_nRnR < 1 ? 0 : EQ.p_nRnR;
                                m_engineer.m_handler.m_interlock.m_log.Info("RNR Start, RNR Count : " + m_nCount);
                                while (m_aLoadport[EQ.p_nRunLP].p_infoCarrier.p_eState != InfoCarrier.eState.Placed) Thread.Sleep(10);
                                m_process.p_sInfo = m_process.AddInfoWafer(m_infoRnRSlot);
                                CalcSequence();
                                //m_nRnR--;
                                EQ.p_nRnR--;
                                p_nRnRCount++;
                                EQ.p_eState = EQ.eState.Run;
                            }
                        }
                        break;
                }
            }
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

        string m_id;
        public VEGA_D_Engineer m_engineer;
        public GAF m_gaf;
        IGem m_gem;

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (VEGA_D_Engineer)engineer;
            m_gaf = engineer.ClassGAF();
            m_gem = engineer.ClassGem();
            InitModule();
            InitThread();
            m_engineer.ClassMemoryTool().InitThreadProcess();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                EQ.p_bStop = true;
                m_thread.Join();
            }
            p_moduleList.ThreadStop();
            foreach (ModuleBase module in p_moduleList.m_aModule.Keys) module.ThreadStop();
        }
    }
}
