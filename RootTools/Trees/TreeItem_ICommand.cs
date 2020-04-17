namespace RootTools.Trees
{
    class TreeItem_ICommand : Tree, ITreeItem
    {
        RelayCommand _value;
        public RelayCommand p_value
        {
            get { return _value; }
        }

        public void SetValue(dynamic value)
        {
            if (_value == value) return;
            if (m_log != null) m_log.Info(p_id + " : " + _value.ToString() + " -> " + value.ToString());
            _value = value;
            OnPropertyChanged("p_value");
        }

        public dynamic GetValue() { return p_value; }

        public string p_sButtonName { get; set; }
        public TreeItem_ICommand(string sName, Tree treeParent, RelayCommand value, string sButtonName, string sDesc, LogWriter log)
        {
            p_sName = sName;
            p_treeParent = treeParent;
            p_treeRoot = treeParent.p_treeRoot;
            p_id = treeParent.p_id + "." + sName;
            p_sDescription = sDesc;
            _value = value;
            p_sButtonName = sButtonName;
            m_log = log;
        }
    }
}
