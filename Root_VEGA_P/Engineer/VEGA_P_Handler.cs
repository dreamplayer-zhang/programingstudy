﻿using Root_VEGA_P.Module;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
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
        //public EFEM_Process m_process; //forgetVegaP
        public VEGA_P m_VEGA; 
        public RTR m_rtr;
        public Loadport m_loadport;
        public EOP m_EOP;
        public EIP_Plate m_EIP_Plate;
        public EIP_Cover m_EIP_Cover;
        public Holder m_holder;
        public Vision m_vision; 
        void InitModule()
        {
            p_moduleList = new ModuleList(m_engineer);
            m_VEGA = new VEGA_P("VEGA P", m_engineer);
            InitModule(m_VEGA); 
            m_rtr = new RTR("RTR", m_engineer);
            InitModule(m_rtr);
            m_loadport = new Loadport("Loadport", m_engineer);
            InitModule(m_loadport);
            m_EOP = new EOP("EOP", m_engineer);
            InitModule(m_EOP);
            m_EIP_Plate = new EIP_Plate("EIP Plate", m_engineer);
            InitModule(m_EIP_Plate);
            m_EIP_Cover = new EIP_Cover("EIP Cover", m_engineer);
            InitModule(m_EIP_Cover);

            m_holder = new Holder("Holder", m_engineer, ModuleBase.eRemote.Client);
            InitModule(m_holder);
            m_vision = new Vision("Vision", m_engineer, ModuleBase.eRemote.Client);
            InitModule(m_vision);

            m_rtr.AddChild(m_loadport, m_EOP.m_dome, m_EOP.m_door, m_EIP_Plate, m_EIP_Cover, m_holder, m_vision); 
            m_rtr.RunTree(Tree.eMode.RegRead);
            m_rtr.RunTree(Tree.eMode.Init);
            m_rtr.ReadPod_Registry();

            m_recipe = new VEGA_P_Recipe("Recipe", m_engineer);
            foreach (ModuleBase module in p_moduleList.m_aModule.Keys) m_recipe.AddModule(module);
            //m_process = new EFEM_Process("Process", m_engineer, iWTR, m_aLoadport);
        }

        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module);
            p_moduleList.AddModule(module, ui);
        }

        public bool IsEnableRecovery()
        {
            foreach (IRTRChild child in m_rtr.p_aChild)
            {
                if (child.IsEnableRecovery()) return true;
            }
            return m_rtr.IsEnableRecovery();
        }
        #endregion

        #region StateHome
        public string StateHome()
        {
            string sInfo = StateHome(m_rtr);
            if (sInfo != "OK")
            {
                EQ.p_eState = EQ.eState.Init;
                return sInfo;
            }
            //sInfo = StateHome((ModuleBase)m_aLoadport[0], (ModuleBase)m_aLoadport[1]);
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
        dynamic m_infoRnRSlot;
        public string AddSequence(dynamic infoSlot)
        {
            m_infoRnRSlot = infoSlot;
            //m_process.p_sInfo = m_process.AddInfoWafer(infoSlot); //forgetVegaP
            return "OK";
        }

        public void CalcSequence()
        {
            //m_process.ReCalcSequence(); //forgetVegaP
            //CalcDockingUndocking();
        }

        public void CalcRecover()
        {
            //m_process.CalcRecover(); //forgetVegaP
            //CalcDockingUndocking();
        }
/*
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
        } */
        #endregion

        #region IHandler
        public void CheckFinish()
        {
            if (m_gem.p_cjRun == null) return;
            //if (m_process.m_qSequence.Count > 0) return;
            foreach (GemPJ pj in m_gem.p_cjRun.m_aPJ)
            {
                m_gem?.SendPJComplete(pj.m_sPJobID);
                Thread.Sleep(100);
            }
        }

        public dynamic GetGemSlot(string sSlot)
        {
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
                            //m_process.p_sInfo = m_process.RunNextSequence();
                            //CheckUnload();
                            //if ((EQ.p_nRnR > 1) && (m_process.m_qSequence.Count == 0))
                            //{
                                //m_process.p_sInfo = m_process.AddInfoWafer(m_infoRnRSlot);
                                //CalcSequence();
                                //EQ.p_nRnR--;
                                //EQ.p_eState = EQ.eState.Run;
                            //}
                        }
                        break;
                }
            }
        }
        #endregion

        #region Tree
        public void RunTreeModule(Tree tree)
        {
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
