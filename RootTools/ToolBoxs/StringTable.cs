using RootTools.RTC5s;
using RootTools.Trees;
using System.Collections.Generic;
using System.Linq;

namespace RootTools.ToolBoxs
{
    public static class StringTable
    {
        public static _StringTable _stringTable = new _StringTable();

        public static Group Get(string sGroup, string[] aString)
        {
            return _stringTable.Get(sGroup, aString);
        }

        public class Group
        {
            public string m_sGroup = "";
            public Dictionary<string, string> m_aString = new Dictionary<string, string>();

            public string Get(string sKey)
            {
                if (m_aString.ContainsKey(sKey)) return m_aString[sKey];
                return sKey;
            }

            public void RunTree(Tree tree)
            {
                foreach (var kv in m_aString.ToList())
                {
                    string sValue = tree.Set(kv.Value, kv.Value, kv.Key, "String Table Value");
                    if (kv.Value != sValue) m_reg.Write(m_sGroup + "." + kv.Key, sValue);
                    m_aString[kv.Key] = sValue;
                }
            }

            Registry m_reg;
            public Group(string sGroup, string[] aString, Registry reg)
            {
                m_reg = reg;
                m_sGroup = sGroup;
                foreach (string sKey in aString)
                {
                    string sValue = m_reg.Read(sGroup + "." + sKey, sKey);
                    m_aString.Add(sKey, sValue);
                }
            }
        }
    }

    public class _StringTable : IToolSet
    {
        static readonly object m_csLock = new object();
        List<StringTable.Group> m_aGroup = new List<StringTable.Group>();

        #region Group
        public StringTable.Group Get(string sGroup, string[] aString)
        {
            lock (m_csLock)
            {
                foreach (StringTable.Group group in m_aGroup)
                {
                    if (group.m_sGroup == sGroup) return group;
                }
                StringTable.Group newGroup = new StringTable.Group(sGroup, aString, m_reg);
                m_aGroup.Add(newGroup);
                RunTree(Tree.eMode.Init); 
                return newGroup;
            }
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
            lock (m_csLock)
            {
                foreach (StringTable.Group group in m_aGroup) group.RunTree(m_treeRoot.GetTree(group.m_sGroup));
            }
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
                m_treeRoot?.m_job?.Close(); 
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
                m_treeRoot?.m_job?.Close();
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
            m_treeRoot = new TreeRoot(sModel + "." + p_id, null);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        public void ThreadStop()
        {
        }
    }
}
