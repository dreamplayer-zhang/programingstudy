using Root_Wind.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_Wind
{
    public class Wind_Handler : NotifyProperty, IHandler
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
        public Wind_Recipe m_recipe;
        public Wind_Process m_process;
        WTR_RND _wtr = null;
        public WTR_RND p_wtr 
        { 
            get { return _wtr;} 
            set { _wtr = value; } 
        }
        Loadport_RND[] _aLoadport = new Loadport_RND[2];
        public Loadport_RND[] p_aLoadport
        { 
            get {return _aLoadport;}
            set { _aLoadport = value; }
        }
        Aligner_ATI _aligner = null;
        public Aligner_ATI p_aligner
        {
            get { return _aligner; }
            set { _aligner = value; }
        }
        Vision _vision=null;
        public Vision p_vision 
        {
            get { return _vision; }
            set { _vision = value; } 
        }

        void InitModule()
        {  
            m_moduleList = new ModuleList(m_enginner);
            p_wtr = new WTR_RND("WTR", m_enginner);
            InitModule(p_wtr);
            p_aLoadport[0] = new Loadport_RND("LoadportA", "LP1", m_enginner);
            InitModule(p_aLoadport[0]);
            p_aLoadport[1] = new Loadport_RND("LoadportB", "LP2", m_enginner);
            InitModule(p_aLoadport[1]);
            p_aligner = new Aligner_ATI("Aligner", m_enginner);
            InitModule(p_aligner);
            p_vision = new Vision("Vision", m_enginner);
            InitModule(p_vision);
            p_wtr.AddChild(p_aLoadport[0], p_aLoadport[1], p_aligner, p_vision);
            p_wtr.ReadInfoWafer_Registry();

            m_recipe = new Wind_Recipe("Recipe", m_enginner);
            m_recipe.AddModule(p_aligner, p_vision);
            m_process = new Wind_Process("Process", m_enginner, this);
        }

        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module);
            m_moduleList.AddModule(module, ui);
        }

        public bool IsEnableRecovery()
        {
            if (p_aligner.p_infoWafer != null) return true;
            if (p_vision.p_infoWafer != null) return true;
            if (p_wtr.m_dicArm[WTR_RND.eArm.Lower].p_infoWafer != null) return true;
            if (p_wtr.m_dicArm[WTR_RND.eArm.Upper].p_infoWafer != null) return true;
            return false; 
        }

        #endregion

        #region StateHome 
        public string StateHome()
        {
            string sInfo = StateHome(p_wtr);
            if (sInfo != "OK") return sInfo;
            sInfo = StateHome(p_aLoadport[0], p_aLoadport[1], p_aligner, p_vision);
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
            if (m_gem.p_cjRun == null) return;
            if (m_process.m_qSequence.Count > 0) return;
            foreach (GemPJ pj in m_gem.p_cjRun.m_aPJ)
            {
                if (m_gem != null) m_gem.SendPJComplete(pj.m_sPJobID);
                Thread.Sleep(100);
            }
        }

        public dynamic GetGemSlot(string sSlot)
        {
            foreach (Loadport_RND loadport in p_aLoadport)
            {
                foreach (GemSlotBase slot in loadport.m_infoCarrier.m_aGemSlot)
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
            Thread.Sleep(3000); 
            while (m_bThread)
            {
                Thread.Sleep(10);
                switch (EQ.p_eState)
                {
                    case EQ.eState.Home: StateHome(); break;
                    case EQ.eState.Run:
                        m_process.p_sInfo = m_process.RunNextSequence(); 
                        break; 
                }
            }
        }
        #endregion

        string m_id;
        public Wind_Engineer m_enginner;
        public GAF m_gaf;
        IGem m_gem;

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_enginner = (Wind_Engineer)engineer;
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
