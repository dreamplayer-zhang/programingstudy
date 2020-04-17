using RootTools;
using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_Wind
{
    public class InfoWafer : GemSlotBase
    {
        #region Wafer Info
        public enum eWaferSize
        {
            e300mm,
            e300mmRF,
            e200mm,
            e4inch,
            e5inch,
            e6inch,
            e8inch,
            eError
        }
        eWaferSize _eSize = eWaferSize.e300mm;
        public eWaferSize p_eSize
        {
            get { return _eSize; }
            set
            {
                if (_eSize == value) return;
                m_log.Info(p_id + " Wafer Size : " + _eSize.ToString() + " -> " + value.ToString());
                _eSize = value;
                OnPropertyChanged();
                RegWrite(); 
            }
        }
        public double p_mmWaferSize
        {
            get
            {
                switch (p_eSize)
                {
                    case eWaferSize.e300mm:; return 300;
                    case eWaferSize.e300mmRF: return 300;
                    case eWaferSize.e200mm: return 200;
                    case eWaferSize.e4inch: return 100;
                    case eWaferSize.e5inch: return 125;
                    case eWaferSize.e6inch: return 150;
                    case eWaferSize.e8inch: return 200;
                    default: return 300;
                }
            }
        }

        /// <summary> Default Wafer Thickness </summary>
        public const int c_umThickness = 700;
        int _umThickness = c_umThickness;
        public int p_umThickness
        {
            get { return _umThickness; }
            set
            {
                if (_umThickness == value) return;
                _umThickness = value;
                m_log.Info(p_id + " p_umthickness = " + value.ToString());
                OnPropertyChanged();
            }
        }

        void RunTreeWaferInfo(Tree tree)
        {
            _eSize = (eWaferSize)tree.Set(p_eSize, p_eSize, "Size", "WaferSize");
            _umThickness = tree.Set(p_umThickness, p_umThickness, "Thickness", "Wafer Thickness (um)");
        }
        #endregion

        #region Property
        string _sWaferID = "";
        public string p_sWaferID
        {
            get { return _sWaferID; }
            set
            {
                if (_sWaferID == value) return;
                m_log.Info(p_id + " Wafer ID : " + _sWaferID + " -> " + value);
                _sWaferID = value;
                OnPropertyChanged();
                RegWrite(); 
            }
        }

        string _sFrameID = "";
        public string p_sFrameID
        {
            get { return _sFrameID; }
            set
            {
                if (_sFrameID == value) return;
                m_log.Info(p_id + " Frame ID : " + _sFrameID + " -> " + value);
                _sFrameID = value;
                OnPropertyChanged();
                RegWrite(); 
            }
        }

        protected override void RunTreeProperty(Tree tree)
        {
            _sWaferID = tree.Set(p_sWaferID, p_sWaferID, "Wafer", "Wafer ID");
            _sFrameID = tree.Set(p_sFrameID, p_sFrameID, "Frame", "Frame ID");
            base.RunTreeProperty(tree.GetTree("Gem")); 
        }
        #endregion

        #region Recipe
        /// <summary> PJ.Recipe -> ModuleRunList </summary>
        public ModuleRunList m_moduleRunList;
        protected override void RecipeOpen()
        {
            m_moduleRunList.Clear();
            foreach (GemPJ pj in m_aPJ) m_moduleRunList.OpenJob(pj.m_sRecipeID, false);
            m_aProcess.Clear();
        }

        public void RecipeOpen(string sRecipe)
        {
            m_moduleRunList.OpenJob(sRecipe, true);
            m_aProcess.Clear();
        }

        public string m_sManualRecipe = "";
        void RunTreeRecipe(Tree tree)
        {
            string sRecipe = m_sManualRecipe;
            m_sManualRecipe = tree.SetFile(m_sManualRecipe, m_sManualRecipe, EQ.m_sModel, "Recipe", "Recipe Name", m_gem.p_bOffline);
            if (sRecipe != m_sManualRecipe) RecipeOpen(m_sManualRecipe);
            if (m_moduleRunList != null) m_moduleRunList.RunTree(tree);
        }
        #endregion

        #region Process
        /// <summary> Recipe ModuleRunList에 WTR Get, Put 추가 -> Process </summary>
        public List<ModuleRunBase> m_aProcess = new List<ModuleRunBase>(); 
        public ModuleRunBase GetProcess(int nNext)
        {
            if (nNext < 0) return null;
            if (m_aProcess.Count <= nNext) return null;
            return m_aProcess[nNext]; 
        }

        void RunTreeProcess(Tree tree)
        {
            for (int n = 0; n < m_aProcess.Count; n++)
            {
                ModuleRunBase moduleRun = m_aProcess[n];
                moduleRun.RunTree(tree.GetTree(n, moduleRun.p_id, false), true);
            }
        }

        #endregion

        #region Calc Process 
        /// <summary> Process 계산 및 Simulation </summary>
        public Queue<ModuleRunBase> m_qCalcProcess = new Queue<ModuleRunBase>();
        public void InitCalcProcess()
        {
            m_qCalcProcess.Clear();
            foreach (ModuleRunBase data in m_aProcess) m_qCalcProcess.Enqueue(data);
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            RunTreeWaferInfo(tree.GetTree("Wafer Info", false));
            base.RunTree(tree);
            RunTreeRecipe(tree.GetTree("Recipe", false)); 
            RunTreeProcess(tree.GetTree("Process", false));
        }
        #endregion

        public double m_degNotch = 0;
        public int m_nRnR = 0;

        public string m_sLoadport;

        public InfoWafer(string id, IEngineer engineer)
        {
            string[] asID = id.Split('.');
            m_sLoadport = asID[0];
            InitBase(id, engineer);
            m_moduleRunList = new ModuleRunList(id, engineer);
            m_moduleRunList.Clear();
        }
    }
}
