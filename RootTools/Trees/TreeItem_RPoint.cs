namespace RootTools.Trees
{
    class TreeItem_RPoint : Tree, ITreeItem
    {
        RPoint _value = new RPoint();
        public RPoint p_value { get { return _value; } }

        public double p_valueX
        {
            get { return _value.X; }
            set
            {
                SetValue(new RPoint(value, _value.Y));
                p_treeRoot.UpdateTreeItems();
            }
        }

        public double p_valueY
        {
            get { return _value.Y; }
            set
            {
                SetValue(new RPoint(_value.X, value));
                p_treeRoot.UpdateTreeItems();
            }
        }

        public void SetValue(dynamic value)
        {
            if (_value == value) return;
            if (m_log != null) m_log.Info(p_id + " : " + _value.ToString() + " -> " + value.ToString());
            _value = value;
            OnPropertyChanged("p_valueX");
            OnPropertyChanged("p_valueY");
        }

        public dynamic GetValue() { return p_value; }

        public TreeItem_RPoint(string sName, Tree treeParent, RPoint value, string sDesc, Log log)
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
