﻿using Root_Vega.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_Vega
{
    public class Vega_Handler : ObservableObject, IHandler
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
        public Vega_Recipe m_recipe;
        public Vega_Process m_process;
        public Vega m_vega; 
        public Robot_RND m_robot;
        public Loadport[] m_aLoadport = new Loadport[2];
        public SideVision m_sideVision;
        public PatternVision m_patternVision;
        public FDC m_FDC;

        void InitModule()
        {
            m_moduleList = new ModuleList(m_enginner);
            m_vega = new Vega("Vega", m_enginner);
            InitModule(m_vega); 
            m_robot = new Robot_RND("Robot", m_enginner);
            InitModule(m_robot);
            m_aLoadport[0] = new Loadport("LoadportA", "LP1", m_enginner);
            InitModule(m_aLoadport[0]);
            m_aLoadport[1] = new Loadport("LoadportB", "LP2", m_enginner);
            InitModule(m_aLoadport[1]);
            m_sideVision = new SideVision("SideVision", m_enginner);
            InitModule(m_sideVision);
            m_patternVision = new PatternVision("PatternVision", m_enginner);
            InitModule(m_patternVision);
            m_FDC = new FDC("FDC", m_enginner);
            InitModule(m_FDC); 
            m_robot.AddChild(m_aLoadport[0], m_aLoadport[1], m_sideVision, m_patternVision);
            m_robot.ReadInfoReticle_Registry();
            m_recipe = new Vega_Recipe("Recipe", m_enginner);
            m_recipe.AddModule(m_sideVision, m_patternVision, m_robot);
            m_process = new Vega_Process("Process", m_enginner, this);
        }

        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module);
            m_moduleList.AddModule(module, ui);
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
            m_process.AddInfoReticle(infoSlot);
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
            foreach (Loadport loadport in m_aLoadport)
            {
                foreach (GemSlotBase slot in loadport.m_infoPod.m_aGemSlot)
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
                        m_process.p_sInfo = m_process.RunNextSequence();
                        break;
                }
            }
        }
        #endregion

        string m_id;
        public Vega_Engineer m_enginner;
        public GAF m_gaf;
        IGem m_gem; 

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_enginner = (Vega_Engineer)engineer;
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
