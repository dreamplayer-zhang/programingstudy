using Root_Pine2.Module;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_Pine2.Engineer
{
    public class Pine2_Handler : IHandler
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

        #region Lot
        public void NewLot()
        {
            m_pine2.p_iBundle = 0;
            m_loadEV.p_iStrip = 0;
            m_sLotSend = "";
            SendLotInfo(); 
        }

        string m_sLotSend = "";
        void SendLotInfo()
        {
            if (m_sLotSend == m_pine2.p_sLotID) return;
            int nMode = (m_pine2.p_eMode == Pine2.eRunMode.Magazine) ? 1 : 0;
            if (m_pine2.p_b3D) SendLotInfo(m_aBoats[eVision.Top3D], nMode);
            SendLotInfo(m_aBoats[eVision.Top2D], nMode);
            SendLotInfo(m_aBoats[eVision.Bottom], nMode);
            m_sLotSend = m_pine2.p_sLotID;
        }

        void SendLotInfo(Boats boats, int nMode)
        {
            Pine2.VisionOption option = m_pine2.m_aVisionOption[boats.m_vision.p_eVision];
            LotInfo lotInfo = new LotInfo(nMode, p_sRecipe, m_pine2.p_sLotID, option.p_bLotMix, option.p_bBarcode, option.p_nBarcode, option.p_lBarcode);
            string sRun = boats.m_vision.SendLotInfo(lotInfo);
            if (sRun == "OK") boats.p_sInfo = lotInfo.m_sLotID; 
        }

        BackgroundWorker m_bgwSendSort = new BackgroundWorker(); 
        public void SendSortInfo(InfoStrip infoStrip)
        {
            m_bgwSendSort.RunWorkerAsync(infoStrip); 
        }

        private void M_bgwSendSort_DoWork(object sender, DoWorkEventArgs e)
        {
            InfoStrip infoStrip = e.Argument as InfoStrip; 
            if (m_pine2.p_b3D) SendSortInfo(m_aBoats[eVision.Top3D], infoStrip);
            SendSortInfo(m_aBoats[eVision.Top2D], infoStrip);
            SendSortInfo(m_aBoats[eVision.Bottom], infoStrip);
            string sRun = m_summary.SetSort(m_pine2.p_b3D, infoStrip); 
            if (sRun != "OK") m_pine2.m_alidSummary.Run(true, sRun);
        }

        void SendSortInfo(Boats boats, InfoStrip infoStrip)
        {
            SortInfo sortinfo = new SortInfo(infoStrip.m_eWorks, infoStrip.p_id, infoStrip.m_iBundle.ToString("00"));
            boats.m_vision.SendSortInfo(sortinfo); 
        }
        #endregion

        #region Recipe
        public string _sRecipe = ""; 
        public string p_sRecipe
        {
            get { return _sRecipe; }
            set
            {
                if (_sRecipe == value) return;
                if (m_bgwRecipe.IsBusy) return; 
                _sRecipe = value;
                m_pine2.RecipeOpen(value);
                m_bgwRecipe.RunWorkerAsync(); 
            }
        }

        BackgroundWorker m_bgwRecipe = new BackgroundWorker();
        private void M_bgwRecipe_DoWork(object sender, DoWorkEventArgs e)
        {
            if (m_aBoats.Count == 0) return;
            if (m_pine2.p_b3D) m_aBoats[eVision.Top3D].p_sRecipe = p_sRecipe;
            m_aBoats[eVision.Top2D].p_sRecipe = p_sRecipe;
            m_aBoats[eVision.Bottom].p_sRecipe = p_sRecipe;
        }

        public List<string> p_asRecipe
        {
            get
            {
                List<string> asRecipe = new List<string>();
                DirectoryInfo info = new DirectoryInfo(EQ.c_sPathRecipe);
                foreach (DirectoryInfo dir in info.GetDirectories()) asRecipe.Add(dir.Name);
                return asRecipe;
            }
            set { }
        }
        #endregion

        #region Module
        StopWatch m_swInit = new StopWatch(); 
        public ModuleList p_moduleList { get; set; }
        public Pine2 m_pine2;
        public LoadEV m_loadEV;
        public MagazineEVSet m_magazineEVSet;
        public Transfer m_transfer;
        public Dictionary<eVision, IVision> m_aVision = new Dictionary<eVision, IVision>();
        public Dictionary<eVision, Boats> m_aBoats = new Dictionary<eVision, Boats>(); 
        public Loader0 m_loader0;
        public Loader1 m_loader1;
        public Loader2 m_loader2;
        public Loader3 m_loader3;
        public Summary m_summary = new Summary(); 
        void InitModule()
        {
            m_swInit.Start(); 
            p_moduleList = new ModuleList(m_engineer);
            InitModule(m_pine2 = new Pine2("Pine2", m_engineer));
            InitModule(m_loadEV = new LoadEV("LoadEV", m_engineer, m_pine2));
            InitMagazineEV();
            InitVision(eVision.Bottom);
            InitVision(eVision.Top2D);
            InitVision(eVision.Top3D);
            InitBoats(eVision.Bottom);
            InitBoats(eVision.Top2D);
            InitBoats(eVision.Top3D);
            InitModule(m_transfer = new Transfer("Transter", m_engineer, m_pine2, m_magazineEVSet));
            InitModule(m_loader0 = new Loader0("Loader0", m_engineer, this));
            InitModule(m_loader1 = new Loader1("Loader1", m_engineer, this));
            InitModule(m_loader2 = new Loader2("Loader2", m_engineer, this));
            InitModule(m_loader3 = new Loader3("Loader3", m_engineer, this));
        }

        long m_msInit = 0; 
        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module);
            p_moduleList.AddModule(module, ui);
            long ms = m_swInit.ElapsedMilliseconds; 
            m_pine2.m_log.Info("InitModule " + module.p_id + " = " + (ms - m_msInit).ToString() + ", " + ms.ToString());
            m_msInit = ms; 
        }

        void InitVision(eVision eVision)
        {
            dynamic vision; 
            switch (eVision)
            {
                case eVision.Top3D: vision = new Vision3D(eVision, m_engineer, ModuleBase.eRemote.Client); break;
                default: vision = new Vision2D(eVision, m_engineer, ModuleBase.eRemote.Client); break;
            }
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(vision);
            p_moduleList.AddModule(vision, ui);
            m_aVision.Add(eVision, vision);
        }

        void InitBoats(eVision eVision)
        {
            Boats boats = new Boats(m_aVision[eVision], m_engineer, m_pine2);
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(boats);
            p_moduleList.AddModule(boats, ui);
            m_aBoats.Add(eVision, boats); 
        }

        void InitMagazineEV()
        {
            m_magazineEVSet = new MagazineEVSet(m_pine2);
            foreach (InfoStrip.eMagazine eMagazine in Enum.GetValues(typeof(InfoStrip.eMagazine)))
            {
                MagazineEV magazineEV = new MagazineEV(eMagazine, m_engineer, m_pine2);
                m_magazineEVSet.m_aEV.Add(eMagazine, magazineEV);
                InitModule(magazineEV); 
            }
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

        #region PickerSet
        public void StartPickerSet()
        {
            if (EQ.p_eState != EQ.eState.Ready) return; 
            p_moduleList.m_moduleRunList.OpenJob(m_pine2.m_sFilePickerSet);
            p_moduleList.StartModuleRuns();
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
            EQ.p_bStop = true;
            gaf?.ClearALID();
            foreach (ModuleBase module in moduleList.m_aModule.Keys) module.Reset();
            Thread.Sleep(100);
            if (IsModuleReady(moduleList)) EQ.p_eState = EQ.eState.Ready; 
            EQ.p_bStop = false;
        }

        bool IsModuleReady(ModuleList moduleList)
        {
            foreach (ModuleBase module in moduleList.m_aModule.Keys)
            {
                if (module.p_eState != ModuleBase.eState.Ready) return false; 
            }
            return true; 
        }
        #endregion

        #region IHandler
        public void CheckFinish()
        {
            if (IsFinish() == false) return;
            m_pine2.m_buzzer.RunBuzzer(Pine2.eBuzzer.Finish);
            EQ.p_eState = EQ.eState.Ready;
            foreach (MagazineEV magazineEV in m_magazineEVSet.m_aEV.Values) magazineEV.StartFinish(); 
        }

        bool IsFinish()
        {
            if (m_loader0.p_infoStrip != null) return false;
            if (m_loader1.p_infoStrip != null) return false;
            if (m_loader2.p_infoStrip != null) return false;
            if (m_loader3.p_infoStrip != null) return false;
            if (m_aBoats[eVision.Top3D].m_aBoat[eWorks.A].p_infoStrip != null) return false;
            if (m_aBoats[eVision.Top3D].m_aBoat[eWorks.B].p_infoStrip != null) return false;
            if (m_aBoats[eVision.Top2D].m_aBoat[eWorks.A].p_infoStrip != null) return false;
            if (m_aBoats[eVision.Top2D].m_aBoat[eWorks.B].p_infoStrip != null) return false;
            if (m_aBoats[eVision.Bottom].m_aBoat[eWorks.A].p_infoStrip != null) return false;
            if (m_aBoats[eVision.Bottom].m_aBoat[eWorks.B].p_infoStrip != null) return false;
            if (m_transfer.m_gripper.p_infoStrip != null) return false;
            if (m_transfer.m_pusher.p_infoStrip != null) return false;
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Stack:
                    if (m_loadEV.p_bCheck) return false;
                    break;
            }
            return true;
        }

        public dynamic GetGemSlot(string sSlot)
        {
            return null;
        }

        public string AddSequence(dynamic infoSlot)
        {
            return "OK"; 
        }

        public void CalcSequence()
        {
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

        bool _bRun = false;
        public bool p_bRun
        {
            get { return _bRun; }
            set
            {
                if (_bRun == value) return;
                _bRun = value;
            }
        }

        void RunThread()
        {
            EQ.eState m_eEQState = EQ.eState.Init; 
            m_bThread = true;
            Thread.Sleep(100);
            while (m_bThread)
            {
                Thread.Sleep(10);
                switch (EQ.p_eState)
                {
                    case EQ.eState.Home: StateHome(); break;
                    case EQ.eState.Run:
                        if (m_eEQState == EQ.eState.Ready) m_summary.LotStart(m_pine2.p_sLotID);
                        break;
                    case EQ.eState.ModuleRunList: 

                        break;
                }
                p_bRun = (EQ.p_eState == EQ.eState.Run) && (EQ.p_bPickerSet == false);
                m_eEQState = EQ.p_eState; 
            }
        }
        #endregion

        string m_id;
        public Pine2_Engineer m_engineer;
        public GAF m_gaf;
        IGem m_gem;

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (Pine2_Engineer)engineer;
            m_gaf = engineer.ClassGAF();
            m_gem = engineer.ClassGem();
            InitModule();
            InitThread();
            m_bgwRecipe.DoWork += M_bgwRecipe_DoWork;
            m_bgwSendSort.DoWork += M_bgwSendSort_DoWork;
            m_engineer.ClassMemoryTool().InitThreadProcess();
        }

        public void ThreadStop()
        {
            m_magazineEVSet.ThreadStop(); 
            if (m_bThread)
            {
                m_bThread = false;
                EQ.p_bStop = true;
                m_thread.Join();
            }
            p_moduleList.ThreadStop();
            foreach (ModuleBase module in p_moduleList.m_aModule.Keys) module.ThreadStop();
        }

        public RnRData GetRnRData()
        {
            throw new NotImplementedException();
        }

        public void UpdateEvent()
        {
            throw new NotImplementedException();
        }
    }
}
