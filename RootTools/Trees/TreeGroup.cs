namespace RootTools.Trees
{
    class TreeGroup : Tree
    {
        public TreeGroup(string sName, Tree treeParent, Log log, bool bExpand, bool bVisible, bool bReadOnly)
        {
            m_sGroup = sName; 
            p_sName = sName;
            p_treeParent = treeParent;
            p_treeRoot = treeParent.p_treeRoot;
            p_id = treeParent.p_id + "." + sName;
            p_bVisible = bVisible;
            p_bEnable = !bReadOnly && p_treeRoot.p_bEnable;
            m_log = log;
            p_bExpand = bExpand;
        }

        public TreeGroup(int nIndex, string sName, Tree treeParent, Log log, bool bExpand, bool bVisible, bool bReadOnly)
        {
            p_sName = nIndex.ToString("000") + "." + sName;
            p_treeParent = treeParent;
            p_treeRoot = treeParent.p_treeRoot;
            p_id = treeParent.p_id + "." + nIndex.ToString("000") + "." + sName;
            p_bVisible = bVisible;
            p_bEnable = !bReadOnly && p_treeRoot.p_bEnable; 
            m_log = log;
            p_bExpand = bExpand;
        }

        public TreeGroup(char cIndex, string sName, Tree treeParent, Log log, bool bExpand, bool bVisible, bool bReadOnly)
        {
            p_sName = (char)(cIndex + 'A') + "." + sName;
            p_treeParent = treeParent;
            p_treeRoot = treeParent.p_treeRoot;
            p_id = treeParent.p_id + "." + (char)(cIndex + 'A') + "." + sName;
            p_bVisible = bVisible;
            p_bEnable = !bReadOnly && p_treeRoot.p_bEnable;
            m_log = log;
            p_bExpand = bExpand;
        }
    }
}
