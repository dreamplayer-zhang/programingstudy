using Root_JEDI.Module;
using Root_JEDI_Sorter.Module;
using Root_JEDI_Vision.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_JEDI.Engineer
{
    public class JEDI_Handler : IHandler
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
        public string _sRecipe = "";
        public string p_sRecipe
        {
            get { return _sRecipe; }
            set
            {
                if (_sRecipe == value) return;
                _sRecipe = value;
                m_JEDI.RecipeOpen(value);
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

        #region Lot
        public void NewLot()
        {
            SendLotInfo();
            m_qSortInfoTray.Clear();
            //m_summary.ClearCount();
        }

        string SendLotInfo()
        {
            foreach (IVision vision in m_vision.Values)
            {
                LotInfo lotInfo = new LotInfo(p_sRecipe, m_JEDI.p_sLotID, m_JEDI.p_thickness);
                vision.SendLotInfo(lotInfo);
            }
            return "OK";
        }

        Queue<InfoTray> m_qSortInfoTray = new Queue<InfoTray>();
        public void SendSortInfo(InfoTray infoTray)
        {
            if (infoTray.p_bInspect == false) return; 
            m_qSortInfoTray.Enqueue(infoTray); 
        }
        
        string SendSortInfo()
        {
            if (m_qSortInfoTray.Count == 0) return "OK";
            InfoTray infoTray = m_qSortInfoTray.Dequeue();
            foreach (IVision vision in m_vision.Values)
            {
                SortInfo sortInfo = new SortInfo(infoTray);
                vision.SendSortInfo(sortInfo);
            }
            //string sRun = m_summary.SetSort(m_pine2.p_b3D, infoStrip);
            //if (sRun != "OK") m_pine2.m_alidSummary.Run(true, sRun);
            return "OK"; 
        }
        #endregion

        #region Module
        public ModuleList p_moduleList { get; set; }
        public JEDI m_JEDI;
        public Dictionary<eVision, IVision> m_vision = new Dictionary<eVision, IVision>(); 

        void InitModule()
        {
            p_moduleList = new ModuleList(m_engineer);
            InitModule(m_JEDI = new JEDI("JEDI", m_engineer));
            InitVision(eVision.Top2D, new Vision2D(eVision.Top2D, m_engineer, ModuleBase.eRemote.Client));
            InitVision(eVision.Bottom, new Vision2D(eVision.Bottom, m_engineer, ModuleBase.eRemote.Client));
        }

        void InitVision(eVision eVision, IVision vision)
        {
            m_vision.Add(eVision, vision);
            InitModule((ModuleBase)m_vision[eVision]); 
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
            EQ.p_bStop = false;
        }
        #endregion

        #region IHandler
        public void CheckFinish() { }
        public dynamic GetGemSlot(string sSlot) { return null; }
        public string AddSequence(dynamic infoSlot) { return "OK"; }
        public void CalcSequence() { }
        public RnRData GetRnRData() { return null; }
        public void UpdateEvent() { return; }
        #endregion

        string m_id;
        public JEDI_Engineer m_engineer;
        public GAF m_gaf;
        IGem m_gem;

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (JEDI_Engineer)engineer;
            m_gaf = engineer.ClassGAF();
            m_gem = engineer.ClassGem();
            InitModule();
            m_engineer.ClassMemoryTool().InitThreadProcess();
        }

        public void ThreadStop()
        {
            p_moduleList.ThreadStop();
            foreach (ModuleBase module in p_moduleList.m_aModule.Keys) module.ThreadStop();
        }
    }
}
