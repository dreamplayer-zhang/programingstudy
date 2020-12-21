using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RootTools.OHTNew
{
    public class OHT : NotifyProperty, ITool
    {
        #region Property
        protected string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                if (value != "OK")
                {
                    m_log.Error(value);
                    m_module.p_eState = ModuleBase.eState.Error;
                }
                OnPropertyChanged();
            }
        }

        public string p_id { get; set; }

        string _sState = "Init";
        public string p_sState
        {
            get { return _sState; }
            set
            {
                if (_sState == value) return;
                _sState = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region eOHT
        public enum eOHT
        {
            Semi,
            SSEM,
        }
        eOHT m_eOHT = eOHT.Semi; 
        void RunTreeOHT(Tree tree)
        {
            m_eOHT = (eOHT)tree.Set(m_eOHT, m_eOHT, "Type", "OHT Type"); 
        }
        #endregion

        public UserControl p_ui => throw new NotImplementedException();

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(p_id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree; ;
            RunTree(Tree.eMode.RegRead);
        }

        private void M_treeRoot_UpdateTree()
        {
            throw new NotImplementedException();
        }

        void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeOHT(m_treeRoot.GetTree("OHT")); 
        }
        #endregion

        public ModuleBase m_module;
        public Log m_log;
        public GemCarrierBase m_carrier = null;
        IToolDIO m_toolDIO;
        public OHT(string id, ModuleBase module, GemCarrierBase carrier, IToolDIO toolDIO)
        {
            p_id = id;
            m_module = module;
            m_carrier = carrier;
            m_log = module.m_log;
            m_toolDIO = toolDIO;

            InitTree();

//            InitTP();
//            InitDIO();
//            InitBase(module, carrier, toolDIO);
//            InitThread();
//            CheckChangeDIO();
        }

        public void ThreadStop()
        {
            throw new NotImplementedException();
        }
    }
}
