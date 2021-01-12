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

namespace Root_CAMELLIA
{
    public class CAMELLIA_Handler : IHandler
    {
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
        public EFEM_Process m_process;
        public Module_Camellia m_camellia;
        void InitModule()
        {
            m_moduleList = new ModuleList(m_engineer);

            InitWTR();
            InitLoadport();
            InitRFID();
            InitAligner();
            m_camellia = new Module_Camellia("Camellia", m_engineer);
            InitModule(m_camellia);
            IWTR iWTR = (IWTR)m_wtr;
            iWTR.AddChild(m_camellia);
            m_wtr.RunTree(Tree.eMode.RegRead);
            m_wtr.RunTree(Tree.eMode.Init);
            iWTR.ReadInfoReticle_Registry();

            m_recipe = new CAMELLIA_Recipe("Recipe", m_engineer);
            //m_recipe.AddModule(m_camellia);
            foreach (ModuleBase module in m_moduleList.m_aModule.Keys) m_recipe.AddModule(module);
            //m_process = new CAMELLIA_Process("Process", m_engineer, this);
            m_process = new EFEM_Process("Process", m_engineer, iWTR);
        }

        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module);
            m_moduleList.AddModule(module, ui);
        }

        public bool IsEnableRecovery()
        {
            //            if (m_vision.p_infoWafer != null) return true;
            //return false;
            IWTR iWTR = (IWTR)m_wtr;
            foreach(IWTRChild child in iWTR.p_aChild)
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
                m_aLoadport.Add((ILoadport)module);
                ((IWTR)m_wtr).AddChild((IWTRChild)module);
            }
        }

        void InitRFID()
        {
            ModuleBase module;
            char cID = 'A';
            for(int n=0; n<m_lLoadport; n++, cID++)
            {
                string sID = "Rfid" + cID;
                module = new RFID_Brooks(sID, m_engineer, m_aLoadport[n]);
                InitModule(module);
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

        #region Module Aligner
        enum eAligner
        {
            None,
            ATI,
            RND
        }
        eAligner m_eAligner = eAligner.ATI;
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

        #region Tree
        public void RunTreeModule(Tree tree)
        {
            RunTreeWTR(tree.GetTree("WTR"));
            RunTreeLoadport(tree.GetTree("Loadport"));
            RunTreeAligner(tree.GetTree("Aligner"));
        }
        #endregion

        #region StateHome
        public string StateHome()
        {
            string sInfo = StateHome(m_moduleList.m_aModule);
            if (sInfo == "OK")
                EQ.p_eState = EQ.eState.Ready;
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
        public int m_nRnR = 1;
        dynamic m_infoRnRSlot;
        public string AddSequence(dynamic infoSlot)
        {
            //m_process.AddInfoWafer(infoSlot);
            m_infoRnRSlot = infoSlot;
            m_process.p_sInfo = m_process.AddInfoWafer(infoSlot);
            return "OK";
        }

        public void CalcSequence()
        {
            m_process.ReCalcSequence();
        }
        #endregion

        #region IHandler
        public void CheckFinish()
        {
            if (m_gem.p_cjRun == null)
                return;
            if (m_process.m_qSequence.Count > 0)
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
        bool m_bThread = false;
        Thread m_thread = null;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }
        
        void RunThread()
        {
            m_bThread = true;
            Thread.Sleep(100);
            while (m_bThread)
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
                            //CheckLoad();
                            m_process.p_sInfo = m_process.RunNextSequence();
                            //CheckUnload();
                            if((m_nRnR > 1) && (m_process.m_qSequence.Count == 0))
                            {
                                m_process.p_sInfo = m_process.AddInfoWafer(m_infoRnRSlot);
                                m_process.ReCalcSequence();
                                m_nRnR--;
                                EQ.p_eState = EQ.eState.Run;
                            } 
                        }
                        break;
                }
            }
        }

        void CheckLoad()
        {
            EFEM_Process.Sequence sequence = m_process.m_qSequence.Peek();
            string sLoadport = sequence.m_infoWafer.m_sModule;
            foreach (ILoadport loadport in m_aLoadport)
            {
                if (loadport.p_id == sLoadport)
                {
                    //loadport.RunDocking();
                    if (loadport.RunDocking() != "OK") return;
                    InfoCarrier infoCarrier = loadport.p_infoCarrier;
                    ManualJobSchedule manualJobSchedule = new ManualJobSchedule(infoCarrier);
                    manualJobSchedule.ShowPopup(); //p_moduleList.ClickRun();
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
        }
        #endregion

        string m_id;
        public CAMELLIA_Engineer m_engineer;
        public GAF m_gaf;
        IGem m_gem;

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
            if (m_bThread)
            {
                m_bThread = false;
                EQ.p_bStop = true;
                m_thread.Join();
            }
            if (m_moduleList != null)
            {
                m_moduleList.ThreadStop();
                foreach (ModuleBase module in m_moduleList.m_aModule.Keys)
                    module.ThreadStop();
            }
        }
    }
}
