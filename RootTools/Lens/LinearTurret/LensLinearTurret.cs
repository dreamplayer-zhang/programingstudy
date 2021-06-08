using RootTools.Control.Xenax;
using RootTools.Trees;
using System.Collections.Generic;
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

        #region Position
        int m_lPos = 3;
        public List<string> p_asPos { get; set; }

        void RunTreePos(Tree tree)
        {
            m_lPos = tree.Set(m_lPos, m_lPos, "Count", "Position Count");
            while (p_asPos.Count < m_lPos) p_asPos.Add("Pos" + p_asPos.Count);
            while (p_asPos.Count > m_lPos) p_asPos.RemoveAt(p_asPos.Count - 1);
            for (int n = 0; n < p_asPos.Count; n++)
            {
                p_asPos[n] = tree.Set(p_asPos[n], p_asPos[n], "Pos" + n.ToString("00"), "Position ID"); 
            }
        }

        void InitAxisPos()
        {
            m_axis.AddPos(p_asPos.ToArray()); 
        }
        #endregion

        #region RunChange
        public string ChangePos(string sPos)
        {
            m_axis.StartMove(sPos); 
            return "OK"; 
        }

        public string WaitReady()
        {
            return m_axis.WaitReady();
        }
        public  string StartHome()
        {
            return m_axis.StartHome();
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
            RunTree(Tree.eMode.Init);
            m_axis.AddPos(p_asPos.ToArray());
        }

        public void RunTree(Tree.eMode mode)
        {
            p_treeRoot.p_eMode = mode;
            RunTreePos(p_treeRoot.GetTree("Position")); 
        }
        #endregion

        Log m_log;
        public LensLinearTurret(string id, Log log)
        {
            p_asPos = new List<string>(); 
            p_id = id;
            m_log = log;
            InitAxis();
            InitTree();
            InitAxisPos(); 
        }

        public void ThreadStop()
        {
            m_axis.ThreadStop(); 
        }

    }
}
