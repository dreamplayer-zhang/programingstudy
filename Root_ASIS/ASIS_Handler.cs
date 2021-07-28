using Root_ASIS.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_ASIS
{
    public class ASIS_Handler : IHandler
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
        public ASIS_Recipe m_recipe;
        public ASIS_Process m_process;
        public ASIS m_ASIS;
        public LoadEV m_loadEV;
        public Dictionary<Boat.eBoat, Boat> m_aBoat = new Dictionary<Boat.eBoat, Boat>();
        public Turnover m_turnover;
        public Dictionary<Cleaner.eCleaner, Cleaner> m_aCleaner = new Dictionary<Cleaner.eCleaner, Cleaner>();
        public Trays m_trays;
        public LoadEV m_paperEV;
        public Loader0 m_loader0; 
        public Loader1 m_loader1;
        public Loader2 m_loader2;
        public Loader3 m_loader3;
        public Sorter0 m_sorter0;
        public Sorter1 m_sorter1;

        void InitModule()
        {
            p_moduleList = new ModuleList(m_engineer);
            m_ASIS = new ASIS("ASIS", m_engineer);
            InitModule(m_ASIS);
            m_loadEV = new LoadEV("LoadEV", m_engineer);
            InitModule(m_loadEV);
            m_aBoat.Add(Boat.eBoat.Boat0, new Boat("Boat0", 0, m_engineer));
            InitModule(m_aBoat[Boat.eBoat.Boat0]);
            m_aBoat.Add(Boat.eBoat.Boat1, new Boat("Boat1", 1, m_engineer));
            InitModule(m_aBoat[Boat.eBoat.Boat1]);
            m_turnover = new Turnover("Turnover", m_engineer);
            InitModule(m_turnover);
            m_aCleaner.Add(Cleaner.eCleaner.Cleaner0, new Cleaner("Cleaner0", 0, m_engineer));
            InitModule(m_aCleaner[Cleaner.eCleaner.Cleaner0]);
            m_aCleaner.Add(Cleaner.eCleaner.Cleaner1, new Cleaner("Cleaner1", 1, m_engineer));
            InitModule(m_aCleaner[Cleaner.eCleaner.Cleaner1]);
            m_trays = new Trays("Trays", m_engineer);
            InitModule(m_trays);
            m_paperEV = new LoadEV("PaperEV", m_engineer);
            InitModule(m_paperEV);
            m_loader0 = new Loader0("Loader0", m_engineer, m_loadEV, m_aBoat[Boat.eBoat.Boat0]);
            InitModule(m_loader0);
            m_loader1 = new Loader1("Loader1", m_engineer, m_aBoat[Boat.eBoat.Boat0], m_turnover);
            InitModule(m_loader1);
            m_loader2 = new Loader2("Loader2", m_engineer, m_loader1, m_turnover, m_aBoat[Boat.eBoat.Boat1]);
            InitModule(m_loader2);
            m_loader3 = new Loader3("Loader3", m_engineer, m_aBoat[Boat.eBoat.Boat1], m_aCleaner);
            InitModule(m_loader3);
            m_sorter0 = new Sorter0("Sorter0", m_engineer, m_aCleaner, m_trays);
            InitModule(m_sorter0);
            m_sorter1 = new Sorter1("Sorter1", m_engineer, m_sorter0, m_trays, m_paperEV);
            InitModule(m_sorter1);
            m_recipe = new ASIS_Recipe("Recipe", m_engineer);
            m_recipe.AddModule();
            m_process = new ASIS_Process("Process", m_engineer, this);
        }

        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module);
            p_moduleList.AddModule(module, ui);
        }

        public bool IsEnableRecovery()
        {
            //            if (m_robot.p_infoReticle != null) return true;
            //            if (m_sideVision.p_infoReticle != null) return true;
            //            if (m_patternVision.p_infoReticle != null) return true;
            return false;
        }
        #endregion

        #region StateHome
        public string StateHome()
        {
            string sInfo = StateHome(p_moduleList.m_aModule);
            if (sInfo == "OK") EQ.p_eState = EQ.eState.Ready;
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
        public string AddSequence(dynamic infoSlot)
        {
            //            m_process.AddInfoWafer(infoSlot);
            return "OK";
        }

        public void CalcSequence()
        {
            //            m_process.ReCalcSequence(null);
        }
        #endregion

        #region IHandler
        public void CheckFinish()
        {
            if (m_gem.p_cjRun == null) return;
            //            if (m_process.m_qSequence.Count > 0) return;
            foreach (GemPJ pj in m_gem.p_cjRun.m_aPJ)
            {
                m_gem?.SendPJComplete(pj.m_sPJobID);
                Thread.Sleep(100);
            }
        }

        public dynamic GetGemSlot(string sSlot)
        {
            //            foreach (Loadport loadport in m_aLoadport)
            //            {
            //                foreach (GemSlotBase slot in loadport.m_infoPod.m_aGemSlot)
            //                {
            //                    if (slot.p_id == sSlot) return slot;
            //                }
            //            }
            return null;
        }

        public RnRData GetRnRData() { return null; }
        public void UpdateEvent() { return; }
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
                        //                        m_process.p_sInfo = m_process.RunNextSequence();
                        break;
                }
            }
        }
        #endregion

        string m_id;
        public ASIS_Engineer m_engineer;
        public GAF m_gaf;
        IGem m_gem;

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (ASIS_Engineer)engineer;
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
