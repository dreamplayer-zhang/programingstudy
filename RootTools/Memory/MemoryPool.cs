using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.MemoryMappedFiles;
using System.Windows.Controls;

namespace RootTools.Memory
{
    public class MemoryPool : ObservableObject, ITool
    {
        const double c_fGB = 1024 * 1024 * 1024;

        #region MemoryPool MMF
        int _gbPool = 0; 
        public int p_gbPool 
        {
            get { return _gbPool; }
            set
            {
                if (value == _gbPool) return;
                if (_gbPool == 0) CreatePool(value);
                _gbPool = value; 
            }
        }

        void CreatePool(int gbPool)
        {
            StopWatch sw = new StopWatch();
            long nPool = (long)Math.Ceiling(gbPool * c_fGB);
            m_MMF = MemoryMappedFile.CreateOrOpen(p_id, nPool);
            m_log.Info(p_id + " Memory Pool Allocate Done " + sw.ElapsedMilliseconds.ToString() + " ms");
        }
        #endregion

        #region MemoryGroup
        public List<string> m_asGroup = new List<string>(); 
        void GroupChanged()
        {
            m_asGroup.Clear();
            foreach (MemoryGroup group in p_aGroup) m_asGroup.Add(group.p_id);
        }

        public ObservableCollection<MemoryGroup> p_aGroup { get; set; }
        public MemoryGroup GetGroup(string sGroup, bool bCreate = true)
        {
            if (sGroup == null) return null;
            if (sGroup == "") return null; 
            foreach (MemoryGroup group in p_aGroup)
            {
                if (group.p_id == sGroup) return group; 
            }
            if (bCreate == false) return null; 
            MemoryGroup newGroup = new MemoryGroup(this, sGroup);
            if (newGroup != null)
            {
                p_aGroup.Add(newGroup);
                GroupChanged();
            }
            return newGroup;
        }

        public MemoryData GetMemory(string sGroup, string sMemory)
        {
            MemoryGroup group = GetGroup(sGroup);
            return (group == null) ? null : group.GetMemory(sMemory); 
        }

        public string DeleteGroup(string sGroup)
        {
            for (int n = 0; n < p_aGroup.Count; n++)
            {
                if (p_aGroup[n].p_id == sGroup)
                {
                    p_aGroup.Remove(p_aGroup[n]);
                    GroupChanged();
                    return "OK"; 
                }
            }
            return "OK"; 
        }

        public void RunTreeMemory(Tree tree, bool bVisible)
        {
            int nGroup = p_aGroup.Count;
            nGroup = tree.Set(nGroup, nGroup, "Count", "Group Count", bVisible); 
            for (int n = 0; n < nGroup; n++)
            {
                string sGroup = (p_aGroup.Count > n) ? p_aGroup[n].p_id : "Group";
                sGroup = tree.Set(sGroup, sGroup, "sGroup." + n.ToString(), "Group Name", bVisible);
                if (p_aGroup.Count <= n) GetGroup(sGroup); 
            }
            foreach (MemoryGroup group in p_aGroup) group.RunTreeMemory(tree.GetTree(group.p_id), bVisible); 
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                MemoryPool_UI ui = new MemoryPool_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        #region RunTree
        public void RunTreeToolBox(Tree tree)
        {
            p_gbPool = tree.Set(p_gbPool, 1, "Pool Size", "Memory Pool Size (Giga Byte)");
        }

        public void RunTreeModule(Tree tree)
        {
            foreach (MemoryGroup group in p_aGroup) RunTreeGroup(tree.GetTree(group.p_id, false), group);
            if (tree.IsUpdated()) m_memoryTool.MemoryChanged(true);
        }

        void RunTreeGroup(Tree tree, MemoryGroup group)
        {
            foreach (MemoryData memory in group.p_aMemory) memory.RunTree(tree.GetTree(memory.p_id, false), true);
        }
        #endregion

        public string p_id { get; set; }
        public Log m_log;
        MemoryMappedFile m_MMF = null;
        public MemoryTool m_memoryTool; 
        public MemoryPool(string id, MemoryTool memoryTool)
        {
            p_aGroup = new ObservableCollection<MemoryGroup>(); 
            p_id = id;
            m_memoryTool = memoryTool; 
            m_log = memoryTool.m_log;
        }

        public void ThreadStop()
        {
        }
    }
}
