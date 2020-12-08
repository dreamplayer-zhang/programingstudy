using RootTools.Trees;
using System.Collections.Generic;

namespace RootTools.Gem
{
    public class GemSlotBase : NotifyProperty
    {
        #region Property
        public string p_id { get; set; }
        #endregion

        #region Gem Property
        public enum eState
        {
            Undefined,
            Empty,
            NotEmpty,
            Exist,
            Double,
            Cross,
        }
        eState _eState = eState.Empty;
        public eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                if (m_log != null) m_log.Info(p_id + " State : " + _eState.ToString() + " -> " + value.ToString());
                _eState = value;
                OnPropertyChanged();
                RegWrite(); 
            }
        }

        string _sSlotID = "";
        public string p_sSlotID
        {
            get { return _sSlotID; }
            set
            {
                if (_sSlotID == value) return;
                if (m_log != null) m_log.Info(p_id + " Slot ID : " + _sSlotID + " -> " + value);
                _sSlotID = value;
                OnPropertyChanged();
                RegWrite();
            }
        }

        string _sLotID = "";
        /// <summary> WaferID or ReticleID </summary>
        public string p_sLotID
        {
            get { return _sLotID; }
            set
            {
                if (_sLotID == value) return;
                if (m_log != null) m_log.Info(p_id + " Lot ID : " + _sLotID + " -> " + value);
                _sLotID = value;
                OnPropertyChanged();
                RegWrite();
            }
        }

        string _sCarrierID = "";
        public string p_sCarrierID
        {
            get { return _sCarrierID; }
            set
            {
                if (_sCarrierID == value) return;
                m_log.Info(p_id + " Pod ID : " + _sCarrierID + " -> " + value);
                _sCarrierID = value;
                OnPropertyChanged();
                RegWrite();
            }
        }

        protected virtual void RunTreeProperty(Tree tree)
        {
            _eState = (eState)tree.Set(p_eState, p_eState, "State", "Slot State", true, true); 
            _sSlotID = tree.Set(p_sSlotID, p_sSlotID, "SlotID", "Slot ID", true, true);
            _sLotID = tree.Set(p_sLotID, p_sLotID, "LotID", "Lot ID", true, true);
            _sCarrierID = tree.Set(p_sCarrierID, p_sCarrierID, "CarrierID", "Carrier ID", true, true);
        }
        #endregion

        #region STS
        public enum eSTS
        {
            atSource,
            atWork,
            atDestination
        }
        eSTS _eSTS = eSTS.atSource;
        public eSTS p_eSTS
        {
            get { return _eSTS; }
            set
            {
                if (_eSTS == value) return;
                if (m_log != null) m_log.Info(p_id + " STS : " + _eSTS.ToString() + " -> " + value.ToString());
                _eSTS = value;
                OnPropertyChanged();
                RegWrite();
            }
        }

        public enum eSTSProcess
        {
            NeedProcessing,
            InProcess,
            Processed,
            Aborted,
            Stopped,
            Rejected,
            Lost,
            Skiped
        }
        eSTSProcess _eSTSProcess = eSTSProcess.NeedProcessing;
        public eSTSProcess p_eSTSProcess
        {
            get { return _eSTSProcess; }
            set
            {
                if (_eSTSProcess == value) return;
                if (m_log != null) m_log.Info(p_id + " STS Process : " + _eSTSProcess.ToString() + " -> " + value.ToString());
                _eSTSProcess = value;
                OnPropertyChanged();
                RegWrite();
            }
        }

        string _sLocID = "";
        public string p_sLocID
        {
            get { return _sLocID; }
            set
            {
                if (_sLocID == value) return;
                if (m_log != null) m_log.Info(p_id + " LocID : " + _sLocID.ToString() + " -> " + value.ToString());
                _sLocID = value;
                OnPropertyChanged();
                RegWrite();
            }
        }

        public string STSSetTransport(string sLocID, bool bDestination)
        {
            eSTS sts = bDestination ? eSTS.atDestination : eSTS.atWork;
            if (sts == eSTS.atWork) p_eSTSProcess = eSTSProcess.NeedProcessing;
            if (m_gem != null) m_gem.STSSetTransport(sLocID, this, sts);
            return "OK";
        }

        public void STSProcessDone()
        {
            m_gem.STSSetProcessing(this, eSTSProcess.Processed); 
        }

        void RunTreeSTS(Tree tree)
        {
            _eSTS = (eSTS)tree.Set(p_eSTS, p_eSTS, "State", "STS Location");
            _eSTSProcess = (eSTSProcess)tree.Set(p_eSTSProcess, p_eSTSProcess, "Process", "STS Location");
            _sLocID = tree.Set(p_sLocID, p_sLocID, "Location ID", "STS Location ID");
        }
        #endregion

        #region GemPJ
        string _sRecipe = ""; 
        public string p_sRecipe
        {
            get { return _sRecipe; }
            set
            {
                string[] recipe_path = value.Split('\\');
                _sRecipe = recipe_path[3];
                OnPropertyChanged(); 
            }
        }
        public List<GemPJ> m_aPJ = new List<GemPJ>(); 
        public void AddPJ(GemPJ pj)
        {
            m_aPJ.Add(pj);
            RecipeOpen(); 
        }
        protected virtual void RecipeOpen() { }

        void RunTreePJ(Tree tree)
        {
            foreach (GemPJ pj in m_aPJ)
            {
                tree.Set(pj.m_sRecipeID, pj.m_sRecipeID, pj.m_sPJobID, "PJ Recipe ID", true, true); 
            }
        }
        #endregion

        #region Tree
        public virtual void RunTree(Tree tree)
        {
            RunTreeProperty(tree.GetTree("Property", false));
            RunTreeSTS(tree.GetTree("STS", false));
            RunTreePJ(tree.GetTree("PJob", false));
        }
        #endregion

        #region Registry
        TreeRoot m_treeRoot; 
        public void RegWrite()
        {
            m_treeRoot.p_eMode = Tree.eMode.RegWrite;
            RunTree(m_treeRoot); 
        }

        public void RegRead()
        {
            m_treeRoot.p_eMode = Tree.eMode.RegRead;
            RunTree(m_treeRoot); 
        }
        #endregion

        protected IEngineer m_engineer; 
        protected Log m_log;
        protected IGem m_gem; 
        protected void InitBase(string id, IEngineer engineer)
        {
            p_id = id; 
            m_engineer = engineer;
            m_log = LogView.GetLog(id, "InfoSlot");
            m_gem = engineer.ClassGem();
            m_treeRoot = new TreeRoot(id, m_log); 
        }
    }
}
