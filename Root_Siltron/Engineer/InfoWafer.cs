using RootTools;
using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Root_Siltron
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
            p_eSize = (eWaferSize)tree.Set(p_eSize, p_eSize, "Size", "WaferSize");
            p_umThickness = tree.Set(p_umThickness, p_umThickness, "Thickness", "Wafer Thickness (um)");
        }
        #endregion

        #region Property
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
        public double m_degNotch = 0;
        protected override void RunTreeProperty(Tree tree)
        {
            base.RunTreeProperty(tree);
            p_sFrameID = tree.Set(p_sFrameID, p_sFrameID, "Frame", "Frame ID");
            m_degNotch = tree.Set(m_degNotch, m_degNotch, "Notch Degree", "Notch Degree (Deg)", true, true); 
        }
        #endregion

        #region Recipe
        /// <summary> PJ.Recipe -> ModuleRunList </summary>
        public ModuleRunList m_moduleRunList;
        protected override void RecipeOpen()
        {
            m_moduleRunList.Clear();
            foreach (GemPJ pj in m_aPJ) m_moduleRunList.OpenJob(pj.m_sRecipeID, false);
            m_qProcess.Clear();
        }

        public void RecipeOpen(string sRecipe)
        {
            m_moduleRunList.OpenJob(sRecipe, true);
            m_qProcess.Clear();
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
        public Queue<ModuleRunBase> m_qProcess = new Queue<ModuleRunBase>();
        void RunTreeProcess(Tree tree)
        {
            ModuleRunBase[] aProcess = m_qProcess.ToArray();
            for (int n = 0; n < aProcess.Length; n++)
            {
                ModuleRunBase moduleRun = aProcess[n];
                moduleRun.RunTree(tree.GetTree(n, moduleRun.p_id, false), true);
            }
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
