using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RootTools.Trees
{
    class TreeItem_stringList : Tree, ITreeItem
    {
        string _value = "";
        public string p_value
        {
            get { return _value; }
            set
            {
                if (value == null) return; 
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

        public TreeItem_stringList(string sName, Tree treeParent, string value, List<string> list, string sDesc, Log log)
        {
            p_sName = sName;
            p_treeParent = treeParent;
            p_treeRoot = treeParent.p_treeRoot;
            p_id = treeParent.p_id + "." + sName;
            p_sDescription = sDesc;
            _value = value;
            m_log = log;
            SetList(list); 
        }

        ObservableCollection<string> _list = new ObservableCollection<string>();
        public ObservableCollection<string> p_list
        {
            get { return _list; }
            set 
            {
                _list = value;
                OnPropertyChanged(); 
            }
        }

        public void SetList(List<string> aList)
        {
            if (IsSameList(aList)) return;
            _list.Clear();
            foreach (string sList in aList) _list.Add(sList);
        }

        bool IsSameList(List<string> aList)
        {
            if (aList.Count != p_list.Count) return false;
            for (int n = 0; n < aList.Count; n++)
            {
                if (aList[n] != p_list[n]) return false;
            }
            return true;
        }

    }
}
