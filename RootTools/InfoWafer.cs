using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace RootTools
{
    public class InfoWafer : GemSlotBase
    {
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

        string _sInspectionID = "";
        public string p_sInspectionID
        {
            get
            {
                return _sInspectionID;
            }
            set
            {
                if (_sInspectionID == value)
                    return;
                m_log.Info(p_id + "Inspection ID : " + _sInspectionID + " -> " + value);
                _sInspectionID = value;
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

        public double m_degNotch = 0;

        protected override void RunTreeProperty(Tree tree)
        {
            _sWaferID = tree.Set(p_sWaferID, p_sWaferID, "Wafer", "Wafer ID");
            _sFrameID = tree.Set(p_sFrameID, p_sFrameID, "Frame", "Frame ID");
            m_degNotch = tree.Set(m_degNotch, m_degNotch, "Notch", "Notch Direction (deg)"); 
            base.RunTreeProperty(tree.GetTree("Gem"));
        }
        #endregion

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
        public enum eWaferOrder
        {
            FirstWafer,
            MiddleWafer,
            LastWafer,
            FirstLastWafer,
        }
        eWaferOrder _eWaferOrder = eWaferOrder.MiddleWafer;
        public eWaferOrder p_eWaferOrder
        {
            get { return _eWaferOrder; }
            set
            {
                _eWaferOrder = value;
            }
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

        #region WaferSize
        public class WaferSize
        {
            #region Data
            public class Data
            {
                public eWaferSize m_eWaferSize = eWaferSize.e300mm;
                public string m_sWaferSize;
                public bool m_bEnable = false;
                public int m_lWafer = 1;
                public int m_teachWTR = -1;

                public Data(eWaferSize waferSize)
                {
                    m_eWaferSize = waferSize;
                    m_sWaferSize = waferSize.ToString();
                    if (waferSize == eWaferSize.e300mm) m_bEnable = true; 
                }

                public void RunTreeEnable(Tree tree, bool bVisible)
                {
                    m_bEnable = tree.Set(m_bEnable, m_bEnable, m_sWaferSize, "Enable Wafer Size", bVisible);
                }

                public void RunTreeCount(Tree tree, bool bVisible)
                {
                    m_lWafer = tree.Set(m_lWafer, m_lWafer, m_sWaferSize, "Wafer Count", m_bEnable && bVisible);
                }

                public void RunTreeTeach(Tree tree)
                {
                    m_teachWTR = tree.Set(m_teachWTR, m_teachWTR, m_sWaferSize, "WTR Teach Index", m_bEnable);
                }
            }
            List<Data> m_aData = new List<Data>();

            void InitDatas()
            {
                int lCount = Enum.GetNames(typeof(eWaferSize)).Length;
                for (int n = 0; n < lCount; n++)
                {
                    Data data = new Data((eWaferSize)n);
                    m_aData.Add(data);
                }
            }

            public Data GetData(eWaferSize size)
            {
                foreach (Data data in m_aData)
                {
                    if (data.m_eWaferSize == size) return data;
                }
                return null;
            }
            #endregion

            public string m_id;
            public bool m_bUseEnable = false;
            public bool m_bUseCount = false;
            public WaferSize(string id, bool bUseEnable, bool bUseCount)
            {
                m_id = id;
                m_bUseEnable = bUseEnable;
                m_bUseCount = bUseCount;
                InitDatas();
            }

            public void RunTree(Tree tree, bool bVisible)
            {
                if ((m_bUseEnable && m_bUseCount) == false) return; 
                foreach (Data data in m_aData)
                {
                    data.RunTreeEnable(tree.GetTree("Enable"), bVisible && m_bUseEnable);
                    data.RunTreeCount(tree.GetTree("Count"), bVisible && m_bUseCount);
                    if (m_bUseEnable == false) data.m_bEnable = true;
                }
            }

            public void RunTreeTeach(Tree tree)
            {
                foreach (Data data in m_aData)
                {
                    data.RunTreeTeach(tree);
                }
            }

            public List<string> GetEnableNames()
            {
                List<string> asEnable = new List<string>();
                foreach (Data data in m_aData)
                {
                    if (data.m_bEnable) asEnable.Add(data.m_sWaferSize);
                }
                return asEnable;
            }
        }
        #endregion

        #region Recipe
        /// <summary> PJ.Recipe -> ModuleRunList </summary>
        public ModuleRunList m_moduleRunList;
        protected override void RecipeOpen()
        {
            m_moduleRunList.Clear();
            foreach (GemPJ pj in m_aPJ)
            {
                m_sManualRecipe = pj.m_sRecipeID;
                p_sRecipe = pj.m_sRecipeID;
                //m_moduleRunList.OpenJob(pj.m_sRecipeID, false);
                m_moduleRunList.OpenJob(pj.m_sRecipeID, true);
            }
            m_qProcess.Clear();
        }

        public void RecipeOpen(string sRecipe)
        {
            m_sManualRecipe = sRecipe;
            p_sRecipe = sRecipe;
            if(m_moduleRunList != null)
                m_moduleRunList.OpenJob(sRecipe, true);
            m_qProcess.Clear();
        }

        public string m_sManualRecipe = "";
        void RunTreeRecipe(Tree tree)
        {
            string sRecipe = m_sManualRecipe;
            bool bOffline = (m_gem == null) ? true : m_gem.p_bOffline; 
            m_sManualRecipe = tree.SetFile(m_sManualRecipe, m_sManualRecipe, EQ.m_sModel, "Recipe", "Recipe Name", true, !bOffline);
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

        #region Calc Process (Sequence)
        /// <summary> Sequence 계산용 임시 Process </summary>
        public Queue<ModuleRunBase> m_qCalcProcess = new Queue<ModuleRunBase>();
        public void InitCalcProcess()
        {
            m_qCalcProcess.Clear(); 
        //    if (EQ.p_nRnR > 1) foreach (ModuleRunBase run in m_moduleRunList.p_aModuleRun) m_qCalcProcess.Enqueue(run);
        //    else
            {
                ModuleRunBase[] aProcess = m_qProcess.ToArray();
                foreach (ModuleRunBase run in aProcess) m_qCalcProcess.Enqueue(run); 
            }
        }

        public override void ClearInfo()
        {
            base.ClearInfo();
            m_moduleRunList.Clear();
            m_qCalcProcess.Clear(); 
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

        public string m_sModule;
        public int m_nSlot = 0; 
        public InfoWafer(string sModule, int nSlot, IEngineer engineer)
        {
            m_sModule = sModule;
            m_nSlot = nSlot; 
            string id = sModule + "." + ((nSlot >= 0) ? (nSlot + 1).ToString("00") : "Recover"); 
            InitBase(id, engineer);
            m_moduleRunList = new ModuleRunList(id, engineer);
            m_moduleRunList.Clear();
        }

        public InfoWafer(InfoWafer info)
        {
            m_sModule = info.m_sModule;
            m_nSlot = info.m_nSlot;
            string id = m_sModule + "." + ((m_nSlot >= 0) ? (m_nSlot + 1).ToString("00") : "Recover");
            InitBase(id, info.m_engineer);
            m_moduleRunList = new ModuleRunList(id, m_engineer);
            m_moduleRunList.Clear();
        }
    }
}
