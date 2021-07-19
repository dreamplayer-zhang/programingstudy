using Root_EFEM;
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

namespace Root_WindII.Engineer
{
    public class WindII_Handler : ObservableObject, IHandler
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
        public EFEM_Recipe m_recipe;
        public EFEM_Process m_process;
        WIND2 m_WIND2;
        public WIND2 p_WIND2
        {
            get
            {
                return m_WIND2;
            }
            set
            {
                SetProperty(ref m_WIND2, value);
            }
        }
        private Vision_Frontside m_visionFront;
        public Vision_Frontside p_VisionFront
		{
            get => m_visionFront;
            set => SetProperty(ref m_visionFront, value);
        }

        void InitModule()
        {
            p_moduleList = new ModuleList(m_engineer);
            InitWTR();
            InitLoadport();
            InitAligner();
            m_visionFront = new Vision_Frontside("Vision", m_engineer, ModuleBase.eRemote.Server);
            InitModule(m_visionFront);
            ((IWTR)m_wtr).AddChild((IWTRChild)m_visionFront);
            InitVision();
            //InitBackside(ModuleBase.eRemote.Client);
            p_WIND2 = new WIND2("WIND2", m_engineer);
            InitModule(p_WIND2);

            m_wtr.RunTree(Tree.eMode.RegRead);
            m_wtr.RunTree(Tree.eMode.Init);
            IWTR iWTR = (IWTR)m_wtr;
            iWTR.ReadInfoReticle_Registry();
            m_recipe = new EFEM_Recipe("Recipe", m_engineer);
            foreach (ModuleBase module in p_moduleList.m_aModule.Keys) m_recipe.AddModule(module);
            m_process = new EFEM_Process("Process", m_engineer, iWTR, m_aLoadport);
            CalcRecover();
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
            Cymechs,
            RTR_RND
        }
        eWTR m_eWTR = eWTR.RND;
        ModuleBase m_wtr;
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
        List<ILoadport> m_aLoadport = new List<ILoadport>();
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

        enum eRFID
        {
            Brooks,
            Ceyon,
        }
        List<eRFID> m_aRFIDType = new List<eRFID>();
        public List<IRFID> m_aRFID = new List<IRFID>();
        void InitRFID()
        {
            ModuleBase module;
            char cID = 'A';
            for (int n = 0; n < m_lLoadport; n++)
            {
                string sID = "RFID" + cID;
                switch (m_aRFIDType[n])
                {
                    case eRFID.Brooks: module = new RFID_Brooks(sID, m_engineer, m_aLoadport[n]); break;
                    case eRFID.Ceyon: module = new RFID_Ceyon(sID, m_engineer, m_aLoadport[n]); break;
                    default: module = new RFID_Brooks(sID, m_engineer, m_aLoadport[n]); break;
                }
                InitModule(module);
                m_aRFID.Add((IRFID)module);
                m_aLoadport[n].m_rfid = m_aRFID[n];
            }
        }

        public void RunTreeRFID(Tree tree)
        {
            while (m_aRFIDType.Count < m_lLoadport) m_aRFIDType.Add(eRFID.Brooks);
            Tree treeType = tree.GetTree("Type");
            for (int n = 0; n < m_lLoadport; n++)
            {
                m_aRFIDType[n] = (eRFID)treeType.Set(m_aRFIDType[n], m_aRFIDType[n], n.ToString("00"), "RFID Type");
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
            Backside,
            EBR,
            AOP,
            EdgeSide
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
                    case eVision.Backside:
                        module = new Vision_Backside(GetVisionID(n), m_engineer, ModuleBase.eRemote.Client);
                        break;
                    case eVision.EBR:
                        module = new Vision_EBR(GetVisionID(n), m_engineer);
                        break;
                    case eVision.AOP:
                        module = new Vision_AOP(GetVisionID(n), m_engineer);
                        break;
                    case eVision.EdgeSide:
                        module = new Vision_Edgeside(GetVisionID(n), m_engineer, ModuleBase.eRemote.Client);
                        break;
                    default:
                        module = new Vision_AOP(GetVisionID(n), m_engineer);
                        break;
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
                if (vision == eVision)
                    nCount++;
            }
            if (nCount == 1)
                return eVision.ToString();
            nCount = 0;
            for (int i = 0; i < n; i++)
            {
                if (m_aVisionType[i] == eVision)
                    nCount++;
            }
            return eVision.ToString() + nCount.ToString();
        }

        public void RunTreeVision(Tree tree)
        {
            m_lVision = tree.Set(m_lVision, m_lVision, "Count", "Vision Count");
            while (m_aVisionType.Count < m_lVision)
                m_aVisionType.Add(eVision.AOP);
            Tree treeType = tree.GetTree("Type");
            for (int n = 0; n < m_lVision; n++)
            {
                m_aVisionType[n] = (eVision)treeType.Set(m_aVisionType[n], m_aVisionType[n], n.ToString("00"), "Vision Type");
            }
        }
        #endregion


        //#region Backside
        //bool m_bBackside = true;
        //public Backside m_backside;
        //void InitBackside(ModuleBase.eRemote eRemote)
        //{
        //    if (m_bBackside == false) return;
        //    m_backside = new Backside("Backside", m_engineer, eRemote);
        //    InitModule(m_backside);
        //}
        //#endregion

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
            while (m_process.p_qSequence.Count > 0) aSequence.Add(m_process.p_qSequence.Dequeue());
            List<ILoadport> aDock = new List<ILoadport>();
            foreach (ILoadport loadport in m_aLoadport)
            {
                if (CalcDocking(loadport, aSequence)) aDock.Add(loadport);
            }
            while (aSequence.Count > 0)
            {
                EFEM_Process.Sequence sequence = aSequence[0];
                m_process.p_qSequence.Enqueue(sequence);
                aSequence.RemoveAt(0);
                for (int n = aDock.Count - 1; n >= 0; n--)
                {
                    if (CalcUnload(aDock[n], aSequence))
                    {
                        ModuleRunBase runUndocking = aDock[n].GetModuleRunUndocking().Clone();
                        EFEM_Process.Sequence sequenceUndock = new EFEM_Process.Sequence(runUndocking, sequence.m_infoWafer);
                        m_process.p_qSequence.Enqueue(sequenceUndock);
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
            if (m_process.p_qSequence.Count > 0) return;
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
                        if (p_moduleList.m_qModuleRun.Count == 0)
                        {
                            m_process.p_sInfo = m_process.RunNextSequence();
                            if ((EQ.p_nRnR > 1) && (m_process.p_qSequence.Count == 0))
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
        #endregion

        #region Tree
        public void RunTreeModule(Tree tree)
        {
            RunTreeWTR(tree.GetTree("WTR"));
            RunTreeLoadport(tree.GetTree("Loadport"));
            RunTreeAligner(tree.GetTree("Aligner"));
            RunTreeVision(tree.GetTree("Vision"));
            //m_bBackside = tree.Set(m_bBackside, m_bBackside, "Backside", "Use Backside");
        }
        #endregion

        string m_id;
        public WindII_Engineer m_engineer;
        public GAF m_gaf;
        IGem m_gem;

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (WindII_Engineer)engineer;
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

            if (p_moduleList != null)
            {
                p_moduleList.ThreadStop();
                foreach (ModuleBase module in p_moduleList.m_aModule.Keys) module.ThreadStop();
            }
        }

        public RnRData GetRnRData()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateEvent()
        {
            throw new System.NotImplementedException();
        }
    }
}
