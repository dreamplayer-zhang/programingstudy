using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

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

        public string p_id { get; set; }

        public string p_sUnit { get; set; }

        string _sDescription = ""; 
        public string p_sDescription 
        { 
            get { return _sDescription; } 
            set
            {
                if (_sDescription == value) return; 
                _sDescription = value;
                string[] asDescrition = _sDescription.Split('(');
                if (asDescrition.Length < 2) return;
                string sUnit = asDescrition[asDescrition.Length - 1];
                if (sUnit[sUnit.Length - 1] != ')') return;
                p_sUnit = sUnit.Substring(0, sUnit.Length - 1);
            }
        }

        public string m_sGroup = ""; 
        public string p_sName { get; set; }

        public bool p_bEnable { get; set; }

        bool _bExpand = false; 
        public bool p_bExpand 
        { 
            get { return _bExpand; }
            set
            {
                _bExpand = value;
                OnPropertyChanged(); 
            } 
        }

        public bool _bUse = true;
        public bool p_bUse
        {
            get { return _bUse; }
            set
            {
                if (_bUse == value) return; 
                _bUse = value;
                OnPropertyChanged(); 
                OnPropertyChanged("p_bVisible");
            }
        }

        bool _bVisible = true;
        public bool p_bVisible
        {
            get { return _bVisible && _bUse; }
            set
            {
                if (_bVisible == value) return;
                _bVisible = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region RunTree Init
        public void RunTreeRemove()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                for (int n = p_aChild.Count - 1; n >= 0; n--)
                {
                    if (p_aChild[n].p_bUse) p_aChild[n].RunTreeRemove();
                    else p_aChild.RemoveAt(n);
                }
                p_bUse = false;
            });
        }

        public void RunTreeInit()
        {
            m_aChildRunInit.Clear();
            //foreach (Tree tree in p_aChild)
            //{
            //    m_aChildRunInit.Add(tree);
            //    tree.RunTreeInit(); 
            //}
            for(int i = 0; i < p_aChild.Count; i++)
            {
                m_aChildRunInit.Add(p_aChild[i]);
                p_aChild[i].RunTreeInit();
            }
        }

        public void RunTreeDone()
        {
            try
            {
                foreach (Tree tree in m_aChildRunInit)
                {
                    if (IsAlreadyExistatChild(tree) == false) p_aChild.Add(tree);
                    tree.RunTreeDone();
                }
            }
            catch (Exception) { }
        }

        bool IsAlreadyExistatChild(Tree treeChild)
        {
            foreach (Tree tree in p_aChild)
            {
                if (tree.p_id == treeChild.p_id) return true; 
            }
            return false; 
        }
        #endregion

        #region RunTree Update
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

        #region Find Tree List<>
        Tree FindTreeItem(string sName)
        {
            foreach (Tree item in m_aChildRunInit)
            {
                if (item.p_sName == sName)
                {
                    item.p_bUse = true;
                    return item;
                }
            }
            return null;
        }

        Tree FindTreeItem(string sName, int nIndex)
        {
            return FindTreeItem(nIndex.ToString("000") + "." + sName); 
        }

        Tree FindTreeItem(string sName, char cIndex)
        {
            return FindTreeItem((char)(cIndex + 'A') + "." + sName);
        }

        void AddTreeItem(Tree treeItem)
        {
            if (p_treeRoot.p_eMode != eMode.Init) return;
            p_bUse = true;
            treeItem.p_bUse = true;
            m_aChildRunInit.Add(treeItem); 
        }
        #endregion

        #region TreeGroup
        public Tree GetTree(string sName, bool bExpand = true, bool bVisible = true, bool bReadOnly = false)
        {
            Tree item = FindTreeItem(sName);
            if (item != null)
            {
                item.p_bVisible = bVisible;
                return item;
            }
            Tree newGroup = new TreeGroup(sName, this, m_log, bExpand, bVisible, bReadOnly);
            AddTreeItem(newGroup);
            return newGroup;
        }

        public Tree GetTree(int nIndex, string sName, bool bExpand = true, bool bVisible = true, bool bReadOnly = false)
        {
            Tree item = FindTreeItem(sName, nIndex);
            if (item != null)
            {
                item.p_bVisible = bVisible;
                return item;
            }
            Tree newGroup = new TreeGroup(nIndex, sName, this, m_log, bExpand, bVisible, bReadOnly);
            AddTreeItem(newGroup);
            return newGroup;
        }

        public Tree GetTree(char cIndex, string sName, bool bExpand = true, bool bVisible = true, bool bReadOnly = false) //forget
        {
            Tree item = FindTreeItem(sName, cIndex);
            if (item != null)
            {
                item.p_bVisible = bVisible;
                return item;
            }
            Tree newGroup = new TreeGroup(cIndex, sName, this, m_log, bExpand, bVisible, bReadOnly);
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

        Tree GetItemFolder(string sName, string value, string sDesc)
        {
            Tree item = FindTreeItem(sName);
            if (item != null) return item;
            Tree newitem = new TreeItem_FolderName(sName, this, value, sDesc, m_log);
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
                    return p_treeRoot.m_job.Set(GetJobID(p_id), item.p_sName, value, valueDef);
            }
            return value;
        }

        public string SetFolder(string value, string valueDef, string id, string sDesc, bool bVisible = true, bool bReadOnly = false)
        {
            Tree item = GetItemFolder(id, value, sDesc);
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
                    return p_treeRoot.m_job.Set(GetJobID(p_id), item.p_sName, value, valueDef);
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
                    return p_treeRoot.m_job.Set(GetJobID(p_id), item.p_sName, value, valueDef);
            }
            return value;
        }

        string GetJobID(string sJobID)
        {
            int nL = p_treeRoot.p_id.Length;
            return "Recipe" + sJobID.Substring(nL, sJobID.Length - nL); 
        }
        #endregion

        public void HideAllItem()
        {
            foreach (Tree item in p_aChild)
            {
                item.p_bVisible = false;
            }
        }

        public Log m_log;
        public Registry m_reg = null;
        public Job m_job = null;

        /// <summary> TreeUI Binging 용</summary>
        public ObservableCollection<Tree> p_aChild { get; set; }
        
        /// <summary> RunTree(Init)를 Thread에서 사용할 수 있게 List<Tree> 로 Copy 후 작업</summary>
        public List<Tree> m_aChildRunInit = new List<Tree>(); 
        
        public Tree()
        {
            p_aChild = new ObservableCollection<Tree>();
        }
    }
}
