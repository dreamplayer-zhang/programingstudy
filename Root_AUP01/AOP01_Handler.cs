using Root_AOP01.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_AOP01
{
    public class AOP01_Handler : IHandler
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
        public ModuleList m_moduleList;
        public AOP01_Recipe m_recipe;
        public AOP01_Process m_process;
        public TapePacker m_tapePacker;
        public VacuumPacker m_vacuumPacker; 
        //        public Robot_RND m_robot;

        void InitModule()
        {
            m_moduleList = new ModuleList(m_engineer);
            m_tapePacker = new TapePacker("TapePacker", m_engineer);
            InitModule(m_tapePacker);
            m_vacuumPacker = new VacuumPacker("VacuumPacker", m_engineer);
            InitModule(m_vacuumPacker); 
            //((IWTR)m_wtr).ReadInfoReticle_Registry();
            m_recipe = new AOP01_Recipe("Recipe", m_engineer);
            m_recipe.AddModule();
            m_process = new AOP01_Process("Process", m_engineer, this);
        }

        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module);
            m_moduleList.AddModule(module, ui);
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
            string sInfo = StateHome(m_moduleList.m_aModule);
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
            Reset(m_gaf, m_moduleList);
            return "OK";
        }

        void Reset(GAF gaf, ModuleList moduleList)
        {
            if (gaf != null) gaf.ClearALID();
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
                if (m_gem != null) m_gem.SendPJComplete(pj.m_sPJobID);
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
        public AOP01_Engineer m_engineer;
        public GAF m_gaf;
        IGem m_gem;

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (AOP01_Engineer)engineer;
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
            m_moduleList.ThreadStop();
            foreach (ModuleBase module in m_moduleList.m_aModule.Keys) module.ThreadStop();
        }
    }
}
