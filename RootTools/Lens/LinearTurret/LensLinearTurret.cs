using RootTools.Control.Xenax;
using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.Lens.LinearTurret
{
    public class LensLinearTurret : NotifyProperty, ILens
    {
        #region Property
        public string p_id { get; set; }

        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value == "OK") return;
                m_log.Warn(value);
            }
        }

        public UserControl p_ui
        {
            get
            {
                LensLinearTurret_UI ui = new LensLinearTurret_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Xenax
        public XenaxAxis m_axis = new XenaxAxis();
        void InitAxis()
        {
            m_axis.Init(null, p_id, m_log); 
        }
        #endregion

        #region Tree
        public TreeRoot p_treeRoot { get; set; }
        void InitTree()
        {
            p_treeRoot = new TreeRoot(p_id, m_log);
            RunTree(Tree.eMode.RegRead);
            p_treeRoot.UpdateTree += P_treeRoot_UpdateTree;
        }

        private void P_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            p_treeRoot.p_eMode = mode;
        }
        #endregion

        Log m_log;
        public LensLinearTurret(string id, Log log)
        {
            p_id = id;
            m_log = log;
            InitAxis();
            InitTree(); 
        }

        public void ThreadStop()
        {
            m_axis.ThreadStop(); 
        }

    }
}
