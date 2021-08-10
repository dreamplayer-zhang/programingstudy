using Root_JEDI_Sorter.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Module;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_JEDI_Sorter.Engineer
{
    public class JEDI_Sorter_Handler : IHandler
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
            //m_summary.ClearCount();
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

        #region Module
        public ModuleList p_moduleList { get; set; }
        public JEDI_Sorter m_JEDI; 
        public Dictionary<In.eIn, In> m_in = new Dictionary<In.eIn, In>();
        public Dictionary<Good.eGood, Good> m_good = new Dictionary<Good.eGood, Good>();
        public Dictionary<Bad.eBad, Bad> m_bad = new Dictionary<Bad.eBad, Bad>();
        public Transfer m_transfer;
        public Loader m_loader; 
        void InitModule()
        {
            p_moduleList = new ModuleList(m_engineer);
            InitModule(m_JEDI = new JEDI_Sorter("JEDI", m_engineer)); 
            InitIn(In.eIn.InA);
            InitIn(In.eIn.InB);
            InitGood(Good.eGood.GoodA);
            InitGood(Good.eGood.GoodB);
            InitBad(Bad.eBad.Reject);
            InitBad(Bad.eBad.Rework);
            InitModule(m_transfer = new Transfer("Transfer", m_engineer));
            InitModule(m_loader = new Loader("Loader", m_engineer));
        }

        void InitIn(In.eIn eIn)
        {
            m_in.Add(eIn, new In(eIn, m_engineer));
            InitModule(m_in[eIn]); 
        }

        void InitGood(Good.eGood eGood)
        {
            m_good.Add(eGood, new Good(eGood, m_engineer));
            InitModule(m_good[eGood]); 
        }

        void InitBad(Bad.eBad eBad)
        {
            m_bad.Add(eBad, new Bad(eBad, m_engineer));
            InitModule(m_bad[eBad]); 
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
        public JEDI_Sorter_Engineer m_engineer;
        public GAF m_gaf;
        IGem m_gem;

        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (JEDI_Sorter_Engineer)engineer;
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
