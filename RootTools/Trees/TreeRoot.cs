namespace RootTools.Trees
{
    public class TreeRoot : Tree
    {
        #region UpdateTree
        public delegate void dgUpdateTree();
        public event dgUpdateTree UpdateTree;
        public void UpdateTreeItems()
        {
            if (UpdateTree != null) UpdateTree();
        }
        #endregion

        #region Property
        eMode _eMode = eMode.RegRead;
        public eMode p_eMode
        {
            get { return _eMode; }
            set
            {
                _eMode = value;
                switch (_eMode)
                {
                    case eMode.Init:
                        RemoveUnUseTreeItem();
                        break;
                    case eMode.Update:
                        ClearUpdated();
                        break;
                }
            }
        }
        #endregion

        public TreeRoot(string id, Log log, bool bReadOnly = false)
        {
            p_id = id;
            p_treeRoot = this;
            p_treeParent = this;
            p_sName = id;
            p_id = id;
            p_bExpand = true;
            p_bEnable = !bReadOnly; 
            m_log = log;
            m_reg = new Registry(id); 
        }
    }
}
