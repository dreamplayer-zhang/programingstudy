﻿using Root_EFEM.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_EFEM
{
    public class EFEM_Handler : IHandler
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
        public EFEM_Recipe m_recipe;
        public EFEM_Process m_process;

        void InitModule()
        {
            m_moduleList = new ModuleList(m_engineer);
            InitWTR(); 
            InitLoadport();
            InitAligner();
            InitVision();
            InitEFEM(); 
            m_wtr.RunTree(Tree.eMode.RegRead);
            m_wtr.RunTree(Tree.eMode.Init);
            //            m_FDC = new FDC("FDC", m_engineer);
            //            InitModule(m_FDC);
            ((IWTR)m_wtr).ReadInfoReticle_Registry(); 
            m_recipe = new EFEM_Recipe("Recipe", m_engineer);
            foreach (ModuleBase module in m_moduleList.m_aModule.Keys) m_recipe.AddModule(module);
//            m_process = new EFEM_Process("Process", m_engineer, this);
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

        #region Module WTR
        enum eWTR
        {
            RND
        }
        eWTR m_eWTR = eWTR.RND;
        ModuleBase m_wtr;
        void InitWTR()
        {
            switch (m_eWTR)
            {
                case eWTR.RND:
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
                    case eLoadport.RND: module = new Loadport_RND(sID, m_engineer); break;
                    case eLoadport.Cymechs: module = new Loadport_Cymechs(sID, m_engineer); break; 
                    default: module = new Loadport_RND(sID, m_engineer); break;
                }
                InitModule(module);
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

        #region Module Vision
        enum eVision
        {
            Vision,
            Backside,
            EBR,
            AOP
        }
        List<eVision> m_aVisionType = new List<eVision>();
        int m_lVision = 1; 
        void InitVision()
        {
            ModuleBase module; 
            for (int n = 0; n < m_lVision; n++)
            {
                string sN = n.ToString("00"); 
                string sID = "Vision" + sN; 
                switch (m_aVisionType[n])
                {
                    case eVision.Backside: module = new Vision_Backside(GetVisionID(n), m_engineer); break;
                    case eVision.EBR: module = new Vision_EBR(GetVisionID(n), m_engineer); break;
                    case eVision.AOP: module = new Vision_AOP(GetVisionID(n), m_engineer); break;
                    case eVision.Vision:
                    default: module = new Vision(GetVisionID(n), m_engineer); break; 
                }
                InitModule(module);
                ((IWTR)m_wtr).AddChild((IWTRChild)module);
            }
        }

        string GetVisionID(int n)
        {
            eVision eVision = m_aVisionType[n];
            int nCount = 0;
            foreach (eVision vision in m_aVisionType)
            {
                if (vision == eVision) nCount++; 
            }
            if (nCount == 1) return eVision.ToString();
            nCount = 0; 
            for (int i = 0; i < n; i++)
            {
                if (m_aVisionType[i] == eVision) nCount++;
            }
            return eVision.ToString() + nCount.ToString(); 
        }

        public void RunTreeVision(Tree tree)
        {
            m_lVision = tree.Set(m_lVision, m_lVision, "Count", "Vision Count");
            while (m_aVisionType.Count < m_lVision) m_aVisionType.Add(eVision.Vision);
            Tree treeType = tree.GetTree("Type");
            for (int n = 0; n < m_lVision; n++)
            {
                m_aVisionType[n] = (eVision)treeType.Set(m_aVisionType[n], m_aVisionType[n], n.ToString("00"), "Vision Type");
            }
        }
        #endregion

        #region Module EFEM
        enum eEFEM
        {
            EFEM
        };
        eEFEM m_eEFEM = eEFEM.EFEM; 
        void InitEFEM()
        {
            ModuleBase module = null;
            switch (m_eEFEM)
            {
                case eEFEM.EFEM: module = new EFEM_AOP("EFEM", m_engineer); break;
            }
            if (module != null) InitModule(module);
        }

        public void RunTreeEFEM(Tree tree)
        {
            m_eEFEM = (eEFEM)tree.Set(m_eEFEM, m_eEFEM, "Type", "EFEM Type");
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
                        if (m_moduleList.m_qModuleRun.Count == 0)
                        {
                            //m_process.p_sInfo = m_process.RunNextSequence();
                        }
                        break;
                }
            }
        }
        #endregion

        string m_id;
        public EFEM_Engineer m_engineer;
        public GAF m_gaf;
        IGem m_gem;

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (EFEM_Engineer)engineer;
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
            m_moduleList.ThreadStop();
            foreach (ModuleBase module in m_moduleList.m_aModule.Keys) module.ThreadStop();
        }
    }
}
