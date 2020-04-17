using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Memory
{
    public class MemoryPool : ObservableObject, ITool
    {
        const double c_fGB = 1024 * 1024 * 1024;

        #region OnMemoryGroupChanged
        public delegate void dgMemoryChanged();
        public event dgMemoryChanged OnMemoryChanged;

        public void MemoryChanged()
        {
            if (OnMemoryChanged != null) OnMemoryChanged(); 
        }
        #endregion

        #region Pool MMF
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
        void InitGroupNames()
        {
            m_asGroup.Clear();
            foreach (MemoryGroup group in m_aGroup) m_asGroup.Add(group.p_id);
            MemoryChanged();
        }

        ObservableCollection<MemoryGroup> m_aGroup = new ObservableCollection<MemoryGroup>();
        public ObservableCollection<MemoryGroup> p_aGroup
        {
            get { return m_aGroup; }
            set { SetProperty(ref m_aGroup, value); }
        }
        public MemoryGroup GetGroup(string sGroup)
        {
            if (sGroup == null) return null; 
            foreach (MemoryGroup group in m_aGroup)
            {
                if (group.p_id == sGroup) return group; 
            }
            MemoryGroup newGroup = new MemoryGroup(this, sGroup);
            if (newGroup != null)
            {
                m_aGroup.Add(newGroup);
                InitGroupNames();
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
            for (int n = 0; n < m_aGroup.Count; n++)
            {
                if (m_aGroup[n].p_id == sGroup)
                {
                    m_aGroup.Remove(m_aGroup[n]);
                    InitGroupNames();
                    return "OK"; 
                }
            }
            return "OK"; 
        }

        public void RunTree(Tree tree, bool bVisible)
        {
            int nGroup = m_aGroup.Count;
            nGroup = tree.Set(nGroup, nGroup, "Count", "Group Count", bVisible); 
            for (int n = 0; n < nGroup; n++)
            {
                string sGroup = (m_aGroup.Count > n) ? m_aGroup[n].p_id : "Group";
                sGroup = tree.Set(sGroup, sGroup, "sGroup." + n.ToString(), "Group Name", bVisible);
                if (m_aGroup.Count <= n) GetGroup(sGroup); 
            }
            foreach (MemoryGroup group in m_aGroup) group.RunTree(tree.GetTree(group.p_id), bVisible); 
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

        public void RunTree(Tree tree)
        {
            p_gbPool = tree.Set(p_gbPool, 1, "Pool Size", "Memory Pool Size (GB)");
        }

        string _id;
        public string p_id
        {
            get
            {
                return _id;
            }
            set
            {
                RaisePropertyChanged();
            }
        }
        public LogWriter m_log;
        MemoryMappedFile m_MMF = null;
        public MemoryPool(string id, LogWriter log)
        {
            _id = id;
            m_log = log;
        }

        public void ThreadStop()
        {
        }
    }
}
