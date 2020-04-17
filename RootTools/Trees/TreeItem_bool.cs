namespace RootTools.Trees
{
    class TreeItem_bool : Tree, ITreeItem
    {  
        bool _value = false;
        public bool p_value
        {
            get { return _value; }
            set
            {
                SetValue(value);
                p_treeRoot.UpdateTreeItems();
            }
        }

        public void SetValue(dynamic value)
        {
            if (_value == value) return;
            if (m_log != null) m_log.Info(p_id + " : " + _value.ToString() + " -> " + value.ToString());
            _value = value;
            OnPropertyChanged("p_value");
        }

        public dynamic GetValue() { return p_value; }

        public TreeItem_bool(string sName, Tree treeParent, bool value, string sDesc, LogWriter log)
        {
            p_sName = sName;
            p_treeParent = treeParent;
            p_treeRoot = treeParent.p_treeRoot;
            p_id = treeParent.p_id + "." + sName;
            p_sDescription = sDesc;
            _value = value;
            m_log = log; 
        }
    }
}
