using Root.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Module;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace Root
{
    public class Root_Handler : IHandler
    {
        #region Module
        public ModuleList m_moduleList;
        public Test m_test;
        public ScareCrow m_scarecrow;
        public Siltron m_siltron;
        public BayerConvert m_bayer; 
        void InitModule()
        {
            m_moduleList = new ModuleList(m_enginner);
            //m_test = new Test("Test", m_enginner);
            //InitModule(m_test);
            //m_scarecrow =new ScareCrow("ScareCrow", m_enginner);
            //InitModule(m_scarecrow);
            m_siltron = new Siltron("Siltrion", m_enginner);
            InitModule(m_siltron);
            m_bayer = new BayerConvert("BayerConvert", m_enginner);
            InitModule(m_bayer);
        }

        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module);
            m_moduleList.AddModule(module, ui);
        }

        public bool IsEnableRecovery()
        {
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
//            m_process.AddInfoReticle(infoSlot);
            return "OK";
        }

        public void CalcSequence()
        {
//            m_process.ReCalcSequence(true);
        }
        #endregion

        #region IHandler
        public void CheckFinish()
        {
        }

        public dynamic GetGemSlot(string sSlot)
        {
            return null; 
        }
        #endregion

        string m_id;
        public Root_Engineer m_enginner;
        GAF m_gaf; 
        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_enginner = (Root_Engineer)engineer;
            m_gaf = engineer.ClassGAF(); 
            InitModule(); 
        }

        public void ThreadStop()
        {
            m_moduleList.ThreadStop();
            foreach (ModuleBase module in m_moduleList.m_aModule.Keys) module.ThreadStop(); 
        }
    }
}
