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
        double _fGB = 0; 
        public double p_fGB
        {
            get { return _fGB; }
            set
            {
                if (value == _fGB) return;
                if (_fGB == 0)
                {
                    if (CreatePool(value)) m_memoryTool.MemoryChanged();
                }
                _fGB = value;
                m_reg.Write("fGB", value);
            }
        }

        bool CreatePool(double fGB)
        {
            StopWatch sw = new StopWatch();
            long nPool = (long)Math.Ceiling(fGB * c_fGB);
            try { m_MMF = MemoryMappedFile.CreateOrOpen(p_id, nPool); }
            catch (Exception) { return false; }
            m_log.Info(p_id + " Memory Pool Allocate Done " + sw.ElapsedMilliseconds.ToString() + " ms");
            return true;
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

        public void UpdateMemoryData()
        {
            p_aGroup.Clear();
            int nGroup = m_reg.Read("Count", 0);
            for (int n = 0; n < nGroup; n++)
            {
                string sGroup = m_reg.Read("Group" + n.ToString(), "");
                MemoryGroup group = GetGroup(sGroup, true);
                group.UpdateMemoryData();
            }
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
        public TreeRoot m_treeRoot; 
        void InitTree()
        {
            m_treeRoot = new TreeRoot(p_id, m_log, false, "MemoryTools");
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead); 
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
            RunTree(Tree.eMode.Init);
        }

        public string m_sSelectedGroup = ""; 
        public void RunTree(Tree.eMode eMode)
        {
            m_treeRoot.p_eMode = eMode;
            if (eMode == Tree.eMode.RegWrite)
            {
                m_reg.Write("Count", p_aGroup.Count); 
                for (int n = 0; n < p_aGroup.Count; n++)
                {
                    m_reg.Write("Group" + n.ToString(), p_aGroup[n].p_id); 
                }
            }
            foreach (MemoryGroup group in p_aGroup)
            {
                bool bVisible = (group.p_id == m_sSelectedGroup); 
                group.RunTree(m_treeRoot, bVisible, !m_memoryTool.m_bMaster);
            }
            if (m_treeRoot.IsUpdated()) m_memoryTool.MemoryChanged();
        }
        #endregion

        public string p_id { get; set; }
        public Log m_log;
        public Registry m_reg; 
        MemoryMappedFile m_MMF = null;
        public MemoryTool m_memoryTool;
        public MemoryViewer m_viewer; 
        public MemoryPool(string id, MemoryTool memoryTool, double fGB)
        {
            p_aGroup = new ObservableCollection<MemoryGroup>();
            p_id = id;
            m_memoryTool = memoryTool;
            m_log = memoryTool.m_log;
            m_viewer = new MemoryViewer(id, this, m_log);
            m_reg = new Registry(p_id, "MemoryTools");
            p_fGB = m_reg.Read("fGB", fGB);
            InitTree(); 
        }

        public void ThreadStop()
        {
        }
    }
}
