using Root_EFEM;
using Root_EFEM.Module;
using Root_WIND2.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using RootTools.OHTNew;
using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_WIND2
{
    public class WIND2_Handler : ObservableObject, IHandler
    {
        public ModuleList p_moduleList { get; set; }

        #region List InfoWafer
        public string AddInfoWafer(InfoWafer infoWafer)
        {
            return "OK";
        }
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
        public WIND2_Recipe m_recipe;
        public EFEM_Process m_process;
        Vision m_vision;
        public Vision p_Vision
        {
            get { return m_vision; }
            set
            {
                SetProperty(ref m_vision, value);
            }
        }
        EdgeSideVision m_edgesideVision;
        public EdgeSideVision p_EdgeSideVision
        {
            get { return m_edgesideVision; }
            set
            {
                SetProperty(ref m_edgesideVision, value);
            }
        }
        public BackSideVision m_backSideVision;
        public BackSideVision p_BackSideVision
        {
            get { return m_backSideVision; }
            set
            {
                SetProperty(ref m_backSideVision, value);
            }
        }

        void InitModule()
        {
            p_moduleList = new ModuleList(m_engineer);
            switch (m_engineer.m_eMode)
            {
                case WIND2_Engineer.eMode.Vision: 
                    InitVisionModule(); 
                    break;
                case WIND2_Engineer.eMode.EFEM:
                    InitEFEMModule();
                    break;
            }
        }

        void InitVisionModule()
        {
            IWTR iWTR = (IWTR)m_wtr;

            m_vision = new Vision("Vision", m_engineer, ModuleBase.eRemote.Server);
            InitModule(m_vision);

            m_recipe = new WIND2_Recipe("Recipe", m_engineer);
            foreach (ModuleBase module in p_moduleList.m_aModule.Keys) m_recipe.AddModule(module);
            m_process = new EFEM_Process("Process", m_engineer, iWTR);
        }

        void InitEFEMModule()
        {
            InitWTR();
            IWTR iWTR = (IWTR)m_wtr;
            InitLoadport();
            InitAligner();

            iWTR.AddChild(m_edgesideVision);
            m_backSideVision = new BackSideVision("BackSide Vision", m_engineer);
            InitModule(m_backSideVision);
            iWTR.AddChild(m_backSideVision);
            m_edgesideVision = new EdgeSideVision("EdgeSide Vision", m_engineer);
            InitModule(m_edgesideVision);
            iWTR.AddChild(m_edgesideVision);
            m_vision = new Vision("Vision", m_engineer, ModuleBase.eRemote.Client);
            if (ProgramManager.Instance.Engineer.m_bVisionEnable)
            {
                InitModule(m_vision);
                iWTR.AddChild(m_vision);
            }

            m_wtr.RunTree(Tree.eMode.RegRead);
            m_wtr.RunTree(Tree.eMode.Init);
            iWTR.ReadInfoReticle_Registry();

            m_recipe = new WIND2_Recipe("Recipe", m_engineer);
            foreach (ModuleBase module in p_moduleList.m_aModule.Keys) m_recipe.AddModule(module);
            m_process = new EFEM_Process("Process", m_engineer, iWTR);
        }

        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module);
            p_moduleList.AddModule(module, ui);
        }

        public bool IsEnableRecovery()
        {
            //            if (m_vision.p_infoWafer != null) return true;
            return false;
        }
        #endregion

        #region Module WTR
        enum eWTR
        {
            RND,
            Cymechs
        }
        eWTR m_eWTR = eWTR.RND;
        ModuleBase m_wtr;
        public ModuleBase p_WTR
        {
            get
            {
                return m_wtr;
            }
            set
            {
                SetProperty(ref m_wtr, value);
            }
        }
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
        public ObservableCollection<ILoadport> m_aLoadport = new ObservableCollection<ILoadport>();
        public ObservableCollection<ILoadport> p_aLoadport
        {
            get
            {
                return m_aLoadport;
            }
            set
            {
                SetProperty(ref m_aLoadport, value);
            }
        }
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

        ModuleBase m_Aligner;
        public ModuleBase p_Aligner
        {
            get
            {
                return m_Aligner;
            }
            set
            {
                SetProperty(ref m_Aligner, value);
            }
        }

        void InitAligner()
        {
            ModuleBase module = null;
            switch (m_eAligner)
            {
                case eAligner.ATI: p_Aligner = new Aligner_ATI("Aligner", m_engineer); break;
                case eAligner.RND: p_Aligner = new Aligner_RND("Aligner", m_engineer); break;
            }
            if (p_Aligner != null)
            {
                InitModule(p_Aligner);
                ((IWTR)m_wtr).AddChild((IWTRChild)p_Aligner);
            }
        }

        public void RunTreeAligner(Tree tree)
        {
            m_eAligner = (eAligner)tree.Set(m_eAligner, m_eAligner, "Type", "Aligner Type");
        }
        #endregion

        #region StateHome
        public string StateHome()
        {
            string sInfo = StateHome(p_moduleList.m_aModule);
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
            Reset(m_gaf, p_moduleList);
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
        public string AddSequence(dynamic infoSlot)
        {
            m_process.AddInfoWafer(infoSlot);
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
            //            foreach (Loadport loadport in m_aLoadport) //forget
            //            {
            //                foreach (GemSlotBase slot in loadport.m_infoPod.m_aGemSlot)
            //                {
            //                    if (slot.p_id == sSlot) return slot;
            //                }
            //            }
            return null;
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread = null;
        public int m_nRnR = 1;
        dynamic m_infoRnRSlot;
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
                        if (p_moduleList.m_qModuleRun.Count == 0)
                        {
                            CheckLoad();
                            m_process.p_sInfo = m_process.RunNextSequence();
                            //CheckUnload();
                            if ((m_nRnR > 1) && (m_process.m_qSequence.Count == 0))
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
        #endregion

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
                    if (loadport.StartRunDocking() != "OK") return;
                    if (EQ.p_bRecovery == false)
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
                    if (bUndock) loadport.StartRunUndocking();
                }
            }

        }
        #region Tree
        public void RunTreeModule(Tree tree)
        {
            if (m_engineer.m_eMode == WIND2_Engineer.eMode.Vision) return;
            RunTreeWTR(tree.GetTree("WTR"));
            RunTreeLoadport(tree.GetTree("Loadport"));
            RunTreeAligner(tree.GetTree("Aligner"));
        }
        #endregion

        string m_id;
        public WIND2_Engineer m_engineer;
        public GAF m_gaf;
        IGem m_gem;
        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (WIND2_Engineer)engineer;
            m_gaf = engineer.ClassGAF();
            m_gem = engineer.ClassGem();
            InitModule();
            InitThread();
            engineer.ClassMemoryTool().InitThreadProcess();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                EQ.p_bStop = true;
                m_thread.Join();
            }
            if (p_moduleList != null)
            {
                p_moduleList.ThreadStop();
                foreach (ModuleBase module in p_moduleList.m_aModule.Keys)
                    module.ThreadStop();
            }
        }
    }
}
