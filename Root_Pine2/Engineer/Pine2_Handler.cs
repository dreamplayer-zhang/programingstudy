using Root_Pine2.Module;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using System;
using System.Collections.Generic;
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

        #region Recipe
        string _sRecipe = ""; 
        public string p_sRecipe
        {
            get { return _sRecipe; }
            set
            {
                if (_sRecipe == value) return;
                _sRecipe = value;
                if (m_aBoats.Count > 0)
                {
                    m_aBoats[Vision2D.eVision.Top3D].p_sRecipe = value;
                    m_aBoats[Vision2D.eVision.Top2D].p_sRecipe = value;
                    m_aBoats[Vision2D.eVision.Bottom].p_sRecipe = value;
                }
            }
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
        public MagazineEVSet m_magazineEV = new MagazineEVSet();
        public Transfer m_transfer;
        public Dictionary<Vision2D.eVision, Vision2D> m_aVision = new Dictionary<Vision2D.eVision, Vision2D>();
        public Dictionary<Vision2D.eVision, Boats> m_aBoats = new Dictionary<Vision2D.eVision, Boats>(); 
        public Loader0 m_loader0;
        public Loader1 m_loader1;
        public Loader2 m_loader2;
        public Loader3 m_loader3;
        void InitModule()
        {
            m_swInit.Start(); 
            p_moduleList = new ModuleList(m_engineer);
            InitModule(m_pine2 = new Pine2("Pine2", m_engineer));
            InitModule(m_loadEV = new LoadEV("LoadEV", m_engineer, m_pine2));
            InitMagazineEV();
            InitVision(Vision2D.eVision.Bottom);
            InitVision(Vision2D.eVision.Top2D);
            InitVision(Vision2D.eVision.Top3D);
            InitBoats(Vision2D.eVision.Bottom);
            InitBoats(Vision2D.eVision.Top2D);
            InitBoats(Vision2D.eVision.Top3D);
            InitModule(m_transfer = new Transfer("Transter", m_engineer, m_pine2, m_magazineEV));
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

        void InitVision(Vision2D.eVision eVision)
        {
            Vision2D vision = new Vision2D(eVision, m_engineer, ModuleBase.eRemote.Client); 
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(vision);
            p_moduleList.AddModule(vision, ui);
            m_aVision.Add(eVision, vision);
        }

        void InitBoats(Vision2D.eVision eVision)
        {
            Boats boats = new Boats(m_aVision[eVision], m_engineer, m_pine2);
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(boats);
            p_moduleList.AddModule(boats, ui);
            m_aBoats.Add(eVision, boats); 
        }

        void InitMagazineEV()
        {
            foreach (InfoStrip.eMagazine eMagazine in Enum.GetValues(typeof(InfoStrip.eMagazine)))
            {
                MagazineEV magazineEV = new MagazineEV(eMagazine, m_engineer, m_pine2);
                m_magazineEV.m_aEV.Add(eMagazine, magazineEV);
                InitModule(magazineEV); 
            }
        }
        #endregion

        #region CheckDone
        public string CheckDone()
        {
            if (IsDone())
            {
                m_pine2.m_buzzer.RunBuzzer(Pine2.eBuzzer.Finish);
                EQ.p_eState = EQ.eState.Ready; 
            }
            return "OK";
        }

        bool IsDone()
        {
            if (m_loader0.p_infoStrip != null) return false;
            if (m_loader1.p_infoStrip != null) return false;
            if (m_loader2.p_infoStrip != null) return false;
            if (m_loader3.p_infoStrip != null) return false;
            if (m_aBoats[Vision2D.eVision.Top3D].m_aBoat[Vision2D.eWorks.A].p_infoStrip != null) return false;
            if (m_aBoats[Vision2D.eVision.Top3D].m_aBoat[Vision2D.eWorks.B].p_infoStrip != null) return false;
            if (m_aBoats[Vision2D.eVision.Top2D].m_aBoat[Vision2D.eWorks.A].p_infoStrip != null) return false;
            if (m_aBoats[Vision2D.eVision.Top2D].m_aBoat[Vision2D.eWorks.B].p_infoStrip != null) return false;
            if (m_aBoats[Vision2D.eVision.Bottom].m_aBoat[Vision2D.eWorks.A].p_infoStrip != null) return false;
            if (m_aBoats[Vision2D.eVision.Bottom].m_aBoat[Vision2D.eWorks.B].p_infoStrip != null) return false;
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
        }

        public dynamic GetGemSlot(string sSlot)
        {
            return null;
        }

        public string AddSequence(dynamic infoSlot)
        {
            throw new NotImplementedException();
        }

        public void CalcSequence()
        {
            throw new NotImplementedException();
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
            m_bThread = true;
            Thread.Sleep(100);
            while (m_bThread)
            {
                Thread.Sleep(10);
                switch (EQ.p_eState)
                {
                    case EQ.eState.Home: StateHome(); break;
                    case EQ.eState.Run: break;
                    case EQ.eState.ModuleRunList: 

                        break;
                }
                p_bRun = (EQ.p_eState == EQ.eState.Run) && (EQ.p_bPickerSet == false);
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
            m_engineer.ClassMemoryTool().InitThreadProcess();
        }

        public void ThreadStop()
        {
            m_magazineEV.ThreadStop(); 
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
