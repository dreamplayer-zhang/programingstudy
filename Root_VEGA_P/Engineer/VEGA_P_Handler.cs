using Root_EFEM;
using Root_EFEM.Module;
using Root_VEGA_P.Module;
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

namespace Root_VEGA_P.Engineer
{
    public class VEGA_P_Handler : IHandler
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
        public VEGA_P_Recipe m_recipe;
        public EFEM_Process m_process;

        void InitModule()
        {
            p_moduleList = new ModuleList(m_engineer);
            InitWTR();
            InitLoadport();
            InitParticleCounter();

            IWTR iWTR = (IWTR)m_wtr;
            m_wtr.RunTree(Tree.eMode.RegRead);
            m_wtr.RunTree(Tree.eMode.Init);
            iWTR.ReadInfoReticle_Registry();

            m_recipe = new VEGA_P_Recipe("Recipe", m_engineer);
            foreach (ModuleBase module in p_moduleList.m_aModule.Keys) m_recipe.AddModule(module);
            m_process = new EFEM_Process("Process", m_engineer, iWTR, m_aLoadport);
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

        void InitParticleCounter()
        {
            ModuleBase module;
            module = new ParticleCounter(m_id, m_engineer);
            InitModule(module);
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

        #region StateHome
        public bool m_bIsPossible_Recovery = false;
        public string StateHome()
        {
            //string sInfo = StateHome(p_moduleList.m_aModule); lyj temp
            string sInfo = StateHome(m_wtr);
            if (sInfo != "OK")
            {
                EQ.p_eState = EQ.eState.Init;
                return sInfo;
            }
            sInfo = StateHome((ModuleBase)m_aLoadport[0], (ModuleBase)m_aLoadport[1]);
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
        dynamic m_infoRnRSlot;
        public string AddSequence(dynamic infoSlot)
        {
            m_infoRnRSlot = infoSlot;
            m_process.p_sInfo = m_process.AddInfoWafer(infoSlot);
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
            List<EFEM_Process.Sequence> aSequence = new List<EFEM_Process.Sequence>();
            while (m_process.m_qSequence.Count > 0) aSequence.Add(m_process.m_qSequence.Dequeue());
            List<ILoadport> aDock = new List<ILoadport>();
            foreach (ILoadport loadport in m_aLoadport)
            {
                if (CalcDocking(loadport, aSequence)) aDock.Add(loadport);
            }
            while (aSequence.Count > 0)
            {
                EFEM_Process.Sequence sequence = aSequence[0];
                m_process.m_qSequence.Enqueue(sequence);
                aSequence.RemoveAt(0);
                for (int n = aDock.Count - 1; n >= 0; n--)
                {
                    if (CalcUnload(aDock[n], aSequence))
                    {
                        ModuleRunBase runUndocking = aDock[n].GetModuleRunUndocking().Clone();
                        EFEM_Process.Sequence sequenceUndock = new EFEM_Process.Sequence(runUndocking, sequence.m_infoWafer);
                        m_process.m_qSequence.Enqueue(sequenceUndock);
                        aDock.RemoveAt(n);
                    }
                }
            }
            m_process.RunTree(Tree.eMode.Init);
        }

        bool CalcDocking(ILoadport loadport, List<EFEM_Process.Sequence> aSequence)
        {
            foreach (EFEM_Process.Sequence sequence in aSequence)
            {
                if (loadport.p_id == sequence.m_infoWafer.m_sModule) return true; 
            }
            return false;
        }

        bool CalcUnload(ILoadport loadport, List<EFEM_Process.Sequence> aSequence)
        {
            foreach (EFEM_Process.Sequence sequence in aSequence)
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
                        if (p_moduleList.m_qModuleRun.Count == 0)
                        {
                            //CheckLoad();
                            m_process.p_sInfo = m_process.RunNextSequence();
                            //CheckUnload();
                            if ((EQ.p_nRnR > 1) && (m_process.m_qSequence.Count == 0))
                            {
                                m_process.p_sInfo = m_process.AddInfoWafer(m_infoRnRSlot);
                                CalcSequence();
                                EQ.p_nRnR--;
                                EQ.p_eState = EQ.eState.Run;
                            }
                        }
                        break;
                }
            }
        }
        /*
        void CheckLoad()
        {
            EFEM_Process.Sequence sequence = m_process.m_qSequence.Peek();
            string sLoadport = sequence.m_infoWafer.m_sModule;
            foreach (ILoadport loadport in m_aLoadport)
            {
                if (loadport.p_id == sLoadport) loadport.RunDocking();
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

        #region Tree
        public void RunTreeModule(Tree tree)
        {
            RunTreeWTR(tree.GetTree("WTR"));
            RunTreeLoadport(tree.GetTree("Loadport"));
        }
        #endregion

        string m_id;
        public VEGA_P_Engineer m_engineer;
        public GAF m_gaf;
        IGem m_gem;

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (VEGA_P_Engineer)engineer;
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
