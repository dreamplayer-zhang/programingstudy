using Root_AOP01_Packing.Module;
using Root_EFEM.Module;
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

namespace Root_AOP01_Packing
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
        public ModuleList p_moduleList { get; set; }
        public AOP01_Recipe m_recipe;
        public AOP01_Process m_process;
        public TapePacker m_tapePacker;
        public VacuumPacker m_vacuumPacker;
        public IndividualElevator m_elevator;
        public Unloadport_AOP m_unloadport;
        public Vision_AOP m_visionAOP;
        public Certification m_RNR;
        public RFID_Brooks m_RFID;

        void InitModule()
        {
            p_moduleList = new ModuleList(m_engineer);
            //InitWTR(); 
            //InitLoadport();


            //m_RNR = new Certification("Certification", m_engineer);
            //InitModule(m_RNR);

            //m_visionAOP = new Vision_AOP("Vision", m_engineer);
            //InitModule(m_visionAOP);
            //m_RFID = new RFID_Brooks("RFID", m_engineer, null);
            //InitModule(m_RFID);

            //m_tapePacker = new TapePacker("TapePacker", m_engineer);
            //InitModule(m_tapePacker);
            //((IWTR)m_aWTR[0]).AddChild((IWTRChild)m_tapePacker);
            //((IWTR)m_aWTR[1]).AddChild((IWTRChild)m_tapePacker);

            m_vacuumPacker = new VacuumPacker("VacuumPacker", m_engineer);
            InitModule(m_vacuumPacker);
            //((IWTR)m_aWTR[1]).AddChild((IWTRChild)m_vacuumPacker);

            //m_elevator = new IndividualElevator("IndividualElevator", m_engineer);
            //InitModule(m_elevator);
            //((IWTR)m_aWTR[1]).AddChild((IWTRChild)m_elevator);

            ////m_unloadport = new Unloadport_AOP("Unloadport", m_engineer);
            //InitModule(m_unloadport);
            //((IWTR)m_aWTR[1]).AddChild((IWTRChild)m_unloadport);

            //m_aWTR[0].RunTree(Tree.eMode.RegRead);
            //m_aWTR[0].RunTree(Tree.eMode.Init);
            //m_aWTR[1].RunTree(Tree.eMode.RegRead);
            //m_aWTR[1].RunTree(Tree.eMode.Init);
            //((IWTR)m_aWTR[1]).ReadInfoReticle_Registry();

            m_recipe = new AOP01_Recipe("Recipe", m_engineer);
            //m_recipe.AddModule();
            foreach (ModuleBase module in p_moduleList.m_aModule.Keys)
                m_recipe.AddModule(module);

            m_process = new AOP01_Process("Process", m_engineer, this);
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

        #region Module WTR
        public List<ModuleBase> m_aWTR = new List<ModuleBase>(); 
        void InitWTR()
        {
            m_aWTR.Add(new WTR_RND("WTR_A", m_engineer));
            InitModule(m_aWTR[0]);
            ((WTR_RND)m_aWTR[0]).m_dicArm[WTR_RND.eArm.Lower].p_bEnable = false;
            m_aWTR.Add(new WTR_RND("WTR_B", m_engineer));
            InitModule(m_aWTR[1]);
            ((WTR_RND)m_aWTR[1]).m_dicArm[WTR_RND.eArm.Lower].p_bEnable = false;
        }
        #endregion

        #region Module Loadport
        public List<ILoadport> m_aLoadport = new List<ILoadport>();
        void InitLoadport()
        {
            Loadport_Cymechs loadportA = new Loadport_Cymechs("LoadportA", m_engineer, false, false);
            InitModule(loadportA);
            m_aLoadport.Add(loadportA);
            ((IWTR)m_aWTR[0]).AddChild((IWTRChild)loadportA);

            //Loadport_AOP loadportAOP = new Loadport_AOP("LoadportB", m_engineer, false, false);
            //InitModule(loadportAOP);
            //m_aLoadport.Add(loadportAOP);
            //((IWTR)m_aWTR[1]).AddChild((IWTRChild)loadportAOP);
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
