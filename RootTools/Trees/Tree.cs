using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace RootTools.Trees
{
    public class Tree : NotifyProperty
    {
        #region eMode
        public enum eMode
        {
            Init,
            Update,
            RegRead,
            RegWrite,
            JobOpen,
            JobSave
        }
        #endregion

        #region Porperty
        public TreeRoot p_treeRoot { get; set; }

        public Tree p_treeParent { get; set; }

        public int p_nIndex { get; set; }

        public string p_id { get; set; }

        public string p_sDescription { get; set; }

        public string p_sName { get; set; }

        public bool p_bEnable { get; set; }

        bool _bExpand = false; 
        public bool p_bExpand 
        { 
            get { return _bExpand; }
            set
            {
                _bExpand = value; 
            } 
        }

        public bool m_bUse = true; 

        bool _bVisible = true;
        public bool p_bVisible
        {
            get { return _bVisible; }
            set
            {
                if (_bVisible == value) return;
                _bVisible = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region RunTreeInit
        public void RunTreeInit()
        {
            for (int n = p_aChild.Count - 1; n >= 0; n--)
            {
                if (p_aChild[n].m_bUse) p_aChild[n].RunTreeInit(); 
                else p_aChild.RemoveAt(n);
            }
            m_bUse = false; 
        }
        #endregion

        #region Update
        public bool m_bUpdated = false;
        protected void ClearUpdated()
        {
            m_bUpdated = false;
            foreach (Tree treeItem in p_aChild) treeItem.ClearUpdated();
        }

        public bool IsUpdated()
        {
            if (p_treeRoot.p_eMode != eMode.Update) return false;
            if (m_bUpdated) return true;
            foreach (Tree treeItem in p_aChild)
            {
                if (treeItem.IsUpdated()) return true;
            }
            return false;
        }
        #endregion

        #region Find Tree
        Tree FindTreeItem(string sName)
        {
            foreach (Tree item in p_aChild)
            {
                if (item.p_sName == sName)
                {
                    item.m_bUse = true;
                    return item;
                }
            }
            return null;
        }

        void AddTreeItem(Tree treeItem)
        {
            if (p_treeRoot.p_eMode != eMode.Init) return;
            m_bUse = true;
            treeItem.m_bUse = true;
            p_treeRoot.AddQueue(this, treeItem);
        }
        #endregion

        #region TreeGroup
        public Tree GetTree(string sName, bool bExpand = true, bool bVisible = true, bool bReadOnly = false)
        {
            Tree item = FindTreeItem(sName);
            if (item != null) return item;
            Tree newGroup = new TreeGroup(sName, this, m_log, bExpand, bVisible, bReadOnly);
            AddTreeItem(newGroup);
            return newGroup;
        }

        public Tree GetTree(int nIndex, string sName, bool bExpand = true, bool bVisible = true, bool bReadOnly = false)
        {
            Tree item = FindTreeItem(sName);
            if (item != null) return item;
            Tree newGroup = new TreeGroup(nIndex, sName, this, m_log, bExpand, bVisible, bReadOnly);
            AddTreeItem(newGroup);
            return newGroup;
        }
        #endregion

        #region TreeItem
        public Tree GetItem(string sName, dynamic value, string sDesc)
        {
            Tree item = FindTreeItem(sName);
            if (item != null) return item;
            Tree newItem = NewItem(sName, value, sDesc);
            AddTreeItem(newItem);
            return newItem;
        }

        Tree NewItem(string sName, dynamic value, string sDesc)
        {
            Type type = value.GetType();
            if (type == typeof(bool)) return new TreeItem_bool(sName, this, value, sDesc, m_log);
            if (type == typeof(int)) return new TreeItem_int(sName, this, value, sDesc, m_log); 
            if (type == typeof(long)) return new TreeItem_long(sName, this, value, sDesc, m_log);
            if (type == typeof(double)) return new TreeItem_double(sName, this, value, sDesc, m_log);
            if (type == typeof(string)) return new TreeItem_string(sName, this, value, sDesc, m_log);
            if (type == typeof(CPoint)) return new TreeItem_CPoint(sName, this, value, sDesc, m_log); 
            if (type == typeof(RPoint)) return new TreeItem_RPoint(sName, this, value, sDesc, m_log);
            return null;
        }

        Tree GetItem(string sName, string value, List<string> list, string sDesc)
        {
            Tree item = FindTreeItem(sName);
            if (item != null)
            {
                ((TreeItem_stringList)(item)).SetList(list);
                return item;
            }
            Tree newitem = new TreeItem_stringList(sName, this, value, list, sDesc, m_log);
            AddTreeItem(newitem);
            return newitem;
        }

        Tree GetItemFile(string sName, string value, string sExt, string sDesc)
        {
            Tree item = FindTreeItem(sName);
            if (item != null) return item;
            Tree newitem = new TreeItem_FileName(sName, this, value, sExt, sDesc, m_log);
            AddTreeItem(newitem);
            return newitem;
        }

        Tree GetItemPassword(string sName, string value, string sDesc)
        {
            Tree item = FindTreeItem(sName);
            if (item != null) return item;
            Tree newitem = new TreeItem_Password(sName, this, value, sDesc, m_log);
            AddTreeItem(newitem);
            return newitem;
        }

        Tree GetItemICommand(string sName, RelayCommand value, string sButtonName, string sDesc)
        {
            Tree item = FindTreeItem(sName);
            if (item != null) return item; 
            Tree newitem = new TreeItem_ICommand(sName, this, value, sButtonName, sDesc, m_log);
            AddTreeItem(newitem);
            return newitem;
        }
        #endregion

        #region Set
        public dynamic Set(dynamic value, dynamic valueDef, string id, string sDesc, bool bVisible = true, bool bReadOnly = false)
        {
            Tree item = GetItem(id, value, sDesc);
            if (item == null) return value;
            item.p_bEnable = !bReadOnly && p_treeParent.p_bEnable;
            item.p_bVisible = bVisible;
            return Set(item, value, valueDef);
        }

        public int Set(Enum value, Enum valueDef, string id, string sDesc, bool bVisible = true, bool bReadOnly = false)
        {
            List<string> lList = new List<string>();
            string sValue = value.ToString();
            string[] aList = Enum.GetNames(value.GetType());
            foreach (string str in aList) lList.Add(str);
            sValue = Set(sValue, valueDef.ToString(), lList, id, sDesc, bVisible, bReadOnly);
            for (int n = 0; n < aList.Length; n++)
            {
                if (sValue == aList[n]) return n;
            }
            return 0;
        }

        public string Set(string value, string valueDef, string[] list, string id, string sDesc, bool bVisible = true, bool bReadOnly = false)
        {
            List<string> aList = new List<string>();
            foreach (string str in list) aList.Add(str);
            return Set(value, valueDef, aList, id, sDesc, bVisible, bReadOnly);
        }
        
        public string Set(string value, string valueDef, List<string> aList, string id, string sDesc, bool bVisible = true, bool bReadOnly = false)
        {
            Tree item = GetItem(id, value, aList, sDesc);
            if (item == null) return value;
            item.p_bEnable = !bReadOnly && p_treeParent.p_bEnable;
            item.p_bVisible = bVisible;
            return (string)Set(item, value, valueDef);
        }

        public string SetPassword(string value, string valueDef, string id, string sDesc, bool bVisible = true, bool bReadOnly = false)
        {
            Tree item = GetItemPassword(id, value, sDesc);
            if (item == null) return value;
            item.p_bEnable = !bReadOnly && p_treeParent.p_bEnable;
            item.p_bVisible = bVisible;
            return (string)Set(item, value, valueDef);
        }

        public string SetFile(string value, string valueDef, string sExt, string id, string sDesc, bool bVisible = true, bool bReadOnly = false)
        {
            Tree item = GetItemFile(id, value, sExt, sDesc);
            if (item == null) return value;
            item.p_bEnable = !bReadOnly && p_treeParent.p_bEnable;
            item.p_bVisible = bVisible;
            switch (p_treeRoot.p_eMode)
            {
                case eMode.Init:
                    ((ITreeItem)item).SetValue(value);
                    break;
                case eMode.Update:
                    dynamic oValue = ((ITreeItem)item).GetValue(); 
                    item.m_bUpdated = true;
                    return (oValue == null) ? "" : oValue.ToString();
                case eMode.RegWrite:
                    p_treeRoot.m_reg.Write(p_id + "." + p_sName + "." + item.p_sName, value);
                    break;
                case eMode.RegRead:
                    return (string)p_treeRoot.m_reg.Read(p_id + "." + p_sName + "." + item.p_sName, valueDef);
                case eMode.JobOpen:
                case eMode.JobSave:
                    if (p_treeRoot.m_job == null) return valueDef;
                    return p_treeRoot.m_job.Set(p_id, item.p_sName, value, valueDef);
            }
            return value;
        }

        public RelayCommand SetButton(RelayCommand value, string sBtnName, string id, string sDesc, bool bVisible = true, bool bReadOnly = false)
        {
            TreeItem_ICommand item = (TreeItem_ICommand)GetItemICommand(id, value, sBtnName, sDesc);
            if (item == null) return value;
            item.p_bEnable = !bReadOnly && p_treeParent.p_bEnable;
            item.p_bVisible = bVisible;
            switch (p_treeRoot.p_eMode)
            {
                case eMode.Init:
                    item.SetValue(value);
                    break;
                case eMode.Update:
                case eMode.RegWrite:
                case eMode.RegRead:
                case eMode.JobOpen:
                case eMode.JobSave:
                    break;
            }

            return value;
        }

        dynamic Set(Tree item, dynamic value, dynamic valueDef)
        {
            switch (p_treeRoot.p_eMode)
            {
                case eMode.Init:
                    ((ITreeItem)item).SetValue(value);
                    break;
                case eMode.Update:
                    item.m_bUpdated = true;
                    return ((ITreeItem)item).GetValue();
                case eMode.RegWrite:
                    p_treeRoot.m_reg.Write(p_id + "." + item.p_sName, value);
                    break;
                case eMode.RegRead:
                    return p_treeRoot.m_reg.Read(p_id + "." + item.p_sName, valueDef);
                case eMode.JobOpen:
                case eMode.JobSave:
                    if (p_treeRoot.m_job == null) return valueDef;
                    return p_treeRoot.m_job.Set(p_id, item.p_sName, value, valueDef);
            }
            return value;
        }
        #endregion

        public void HideAllItem()
        {
            foreach (Tree item in p_aChild)
            {
                item.p_bVisible = false;
            }
        }

        protected Log m_log;
        public Registry m_reg = null;
        public Job m_job = null;

        public ObservableCollection<Tree> p_aChild { get; set; }
        public Tree()
        {
            p_aChild = new ObservableCollection<Tree>();
        }
    }
}
