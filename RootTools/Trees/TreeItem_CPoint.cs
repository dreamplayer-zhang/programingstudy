namespace RootTools.Trees
{
    class TreeItem_CPoint : Tree, ITreeItem
    {
        CPoint _value = new CPoint();
        public CPoint p_value {  get { return _value; } }

        public int p_valueX
        {
            get { return _value.X; }
            set
            {
                SetValue(new CPoint(value, _value.Y));
                p_treeRoot.UpdateTreeItems();
            }
        }

        public int p_valueY
        {
            get { return _value.Y; }
            set
            {
                SetValue(new CPoint(_value.X, value));
                p_treeRoot.UpdateTreeItems();
            }
        }

        public void SetValue(dynamic value)
        {
            if (_value == value) return;
            m_log?.Info(p_id + " : " + _value.ToString() + " -> " + value.ToString());
            _value = value;
            OnPropertyChanged("p_valueX");
            OnPropertyChanged("p_valueY");
        }

        public dynamic GetValue() { return p_value; }

        public TreeItem_CPoint(string sName, Tree treeParent, CPoint value, string sDesc, Log log)
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
