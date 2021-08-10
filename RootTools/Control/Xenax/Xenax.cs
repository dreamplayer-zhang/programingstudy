using RootTools.Trees;

namespace RootTools.Control.Xenax
{
    public class Xenax : IToolSet, IControl
    {
        #region ITool
        public string p_id { get; set; }
        #endregion

        #region IControl
        public Axis GetAxis(string id, Log log)
        {
            return m_listAxis.GetAxis(id, log);
        }

        public AxisXY GetAxisXY(string id, Log log)
        {
            return m_listAxis.GetAxisXY(id, log);
        }

        public AxisXZ GetAxisXZ(string id, Log log)
        {
            return m_listAxis.GetAxisXZ(id, log);
        }

        public Axis3D GetAxis3D(string id, Log log)
        {
            return m_listAxis.GetAxis3D(id, log);
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.Init);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
        }
        #endregion

        IEngineer m_engineer;
        Log m_log;
        public TreeRoot m_treeRoot;
        public XenaxListAxis m_listAxis = new XenaxListAxis();

        public void Init(string id, IEngineer engineer)
        {
            p_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);
            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            m_listAxis.Init(id + ".Axis", engineer);

            for (int i = 0; i < m_listAxis.m_aAxis.Count; i++)
            {
                m_listAxis.m_aAxis[i].RunTreeInterlock(Tree.eMode.RegRead);
            }

            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
        }

        public void ThreadStop()
        {
            m_listAxis.ThreadStop();
        }
    }
}
