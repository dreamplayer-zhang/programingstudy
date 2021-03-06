using Root_Rinse_Unloader.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_Rinse_Unloader.Engineer
{
    public class RinseU_Handler : IHandler
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
        public RinseU m_rinse;
        public Storage m_storage;
        public Rail m_rail;
        public Roller m_roller;
        public Loader m_loader; 

        void InitModule()
        {
            p_moduleList = new ModuleList(m_engineer);
            m_rinse = new RinseU("Rinse", m_engineer);
            InitModule(m_rinse);
            m_storage = new Storage("Storage", m_engineer, m_rinse);
            InitModule(m_storage);
            m_rail = new Rail("Rail", m_engineer, m_rinse, m_storage);
            InitModule(m_rail);
            m_roller = new Roller("Roller", m_engineer, m_rinse, m_rail, m_storage);
            InitModule(m_roller);
            m_loader = new Loader("Loader", m_engineer, m_rinse, m_storage, m_rail, m_roller);
            InitModule(m_loader);
        }

        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module);
            p_moduleList.AddModule(module, ui);
        }
        #endregion

        #region StateHome
        public string StateHome()
        {
            m_loader.RunVacuum(false);
            Thread.Sleep(100);
            m_loader.RunPickerDown(false);
            Thread.Sleep(100);
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
            if (m_rail.IsStripExist())
            {
                EQ.p_bStop = true;
                EQ.p_eState = EQ.eState.Init;
                return "Check Strip in Rail";
            }
            m_rail.RunPusherDown(true); 
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
            EQ.p_bStop = true; 
            Reset(m_gaf, p_moduleList);
            Thread.Sleep(100); 
            EQ.p_bStop = false; 
            return "OK";
        }

        void Reset(GAF gaf, ModuleList moduleList)
        {
            gaf?.ClearALID();
            foreach (ModuleBase module in moduleList.m_aModule.Keys) module.Reset();
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
                StartRun(value);
            }
        }

        void StartRun(bool bRun)
        {
            if (bRun)
            {
                m_rail.RunPusherDown(true);
                m_loader.StartRun();
                m_rail.StartRun();
                m_roller.StartRun();
                m_storage.StartRun(); 
            }
            else
            {

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
                    case EQ.eState.Run:
                        //forget
                        break;
                }
                p_bRun = (EQ.p_eState == EQ.eState.Run) && (EQ.p_bPickerSet == false);
            }
        }
        #endregion

        #region PickerSet
        public string StartPickerSet()
        {
            if (m_loader.m_sFilePickerSet == "") return "PickerSet ModuleRun File ot Exist";
            EQ.p_bPickerSet = true;
            p_moduleList.m_moduleRunList.OpenJob(m_loader.m_sFilePickerSet);
            p_moduleList.StartModuleRuns();
            return "OK";
        }
        #endregion

        string m_id;
        public RinseU_Engineer m_engineer;
        public GAF m_gaf;
        IGem m_gem;

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (RinseU_Engineer)engineer;
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
