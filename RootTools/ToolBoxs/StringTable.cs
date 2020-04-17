using RootTools.Trees;
using System.Collections.Generic;

namespace RootTools.ToolBoxs
{
    public static class StringTable
    {
        public static _StringTable _stringTable = new _StringTable();

        public static String Get(string sKey, string sGroup = "StringTable")
        {
            return _stringTable.Get(sKey, sGroup);
        }

        public class String : NotifyProperty
        {
            string _sKey = ""; 
            public string p_sKey { get { return _sKey; } set { _sKey = value; } }
            string _sGroup = ""; 
            public string p_sGroup { get { return _sGroup; } set { _sGroup = value; } }

            string _sValue = "";
            public string p_sValue
            {
                get { return _sValue; }
                set
                {
                    if (_sValue == value) return;
                    _sValue = value;
                    m_stringTable.m_reg.Write(p_sGroup + "." + p_sKey, _sValue);
                    OnPropertyChanged();
                }
            }

            public override string ToString()
            {
                return "(" + p_sKey + ", " + p_sValue + ")";
            }

            public void RunTree(Tree tree)
            {
                p_sValue = tree.Set(p_sValue, p_sValue, p_sKey, "Change String Table Value");
            }

            _StringTable m_stringTable = null;
            public String(string sKey, string sGroup, _StringTable stringTable)
            {
                p_sKey = sKey;
                p_sGroup = sGroup;
                m_stringTable = stringTable;
                if (stringTable.m_reg == null) _sValue = sKey;
                else _sValue = stringTable.m_reg.Read(sGroup + "." + sKey, sKey);
            }

            public void Init()
            {
                _sValue = m_stringTable.m_reg.Read(p_sGroup + "." + p_sKey, p_sKey);
            }
        }
    }

    public class _StringTable : IToolSet
    {
        #region String
        public List<StringTable.String> m_aString = new List<StringTable.String>();
        public StringTable.String Get(string sKey, string sGroup)
        {
            foreach (StringTable.String str in m_aString)
            {
                if (str.p_sKey == sKey) return str;
            }
            StringTable.String newString = new StringTable.String(sKey, sGroup, this);
            m_aString.Add(newString);
            RunTree(Tree.eMode.Init); 
            return newString;
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update); 
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            foreach (StringTable.String str in m_aString) str.RunTree(m_treeRoot.GetTree(str.p_sGroup)); 
        }
        #endregion

        #region File
        public void FileOpen(string sFile)
        {
            try
            {
                m_treeRoot.m_job = new Job(sFile, false, null);
                RunTree(Tree.eMode.JobOpen);
                RunTree(Tree.eMode.Init);
            }
            finally 
            {
                if (m_treeRoot.m_job != null) m_treeRoot.m_job.Close(); 
            }
        }

        public void FileSave(string sFile)
        {
            try
            {
                m_treeRoot.m_job = new Job(sFile, true, null);
                RunTree(Tree.eMode.JobSave);
            }
            finally
            {
                if (m_treeRoot.m_job != null) m_treeRoot.m_job.Close();
            }
        }
        #endregion

        public string p_id { get; set; }
        public Registry m_reg;
        public TreeRoot m_treeRoot;
        public void Init(string sModel)
        {
            p_id = "StringTable"; 
            m_reg = new Registry(p_id, sModel);
            foreach (StringTable.String str in m_aString) str.Init();
            m_treeRoot = new TreeRoot(sModel + "." + p_id, null);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        public void ThreadStop()
        {
        }
    }
}
