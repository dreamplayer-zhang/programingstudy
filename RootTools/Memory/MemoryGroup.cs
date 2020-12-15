using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace RootTools.Memory
{
    public class MemoryGroup : ObservableObject
    {
        #region Property
        public string p_id { get; set; }

        int _mbOffset = 0; 
        public int p_mbOffset
        {
            get { return _mbOffset; }
            set
            {
                _mbOffset = value;
                p_gpOffset = (value / 1024.0).ToString("0.0 GB");
                RaisePropertyChanged();
                RaisePropertyChanged("p_gbAvailable");
            }
        }

        string _gbOffset = "";
        public string p_gpOffset
        {
            get { return _gbOffset; }
            set
            {
                _gbOffset = value;
                RaisePropertyChanged();
            }
        }

        public string p_gbAvailable
        {
            get { return (m_pool.p_fGB - p_mbOffset / 1024.0).ToString("0.0 GB"); }
            set { }
        }
        #endregion

        #region MemoryData
        public ObservableCollection<MemoryData> p_aMemory { get; set; }
    
        public MemoryData GetMemory(string id)
        {
            if (id == null) return null; 
            foreach (MemoryData memory in p_aMemory)
            {
                if (memory.p_id == id) return memory; 
            }
            return null; 
        }

        public MemoryData CreateMemory(string id, int nCount, int nByte, CPoint sz)
        {
            return CreateMemory(id, nCount, nByte, sz.X, sz.Y); 
        }

        public MemoryData CreateMemory(string id, int nCount, int nByte, int xSize, int ySize)
        {
            int mbPool = (int)(1024 * m_pool.p_fGB); 
            MemoryData memory = GetMemory(id);
            if (memory != null) DeleteMemory(id); 
            memory = new MemoryData(this, id, nCount, nByte, xSize, ySize, ref _mbOffset);
            p_mbOffset = _mbOffset;
            if (p_mbOffset > mbPool)
            {
                //MessageBox.Show("Memory Pool Size Error\n ID : " + id
                //    + "  AllocMemory : " + (int)Math.Ceiling((double)mbPool / 1000)
                //    + "[GB]  Required Memory : " + (int)Math.Ceiling((double)p_mbOffset / 1000 + 1) + "[GB]");
                m_log.Error("Memory Size Overrun");
            }
            p_aMemory.Add(memory);
            m_pool.RunTree(Tree.eMode.RegRead);
            m_pool.RunTree(Tree.eMode.Init); 
            m_pool.m_memoryTool.MemoryChanged(); 
            return memory; 
        }

        public string DeleteMemory(string id)
        {
            MemoryData memory = GetMemory(id);
            if (memory == null) return "OK";
            p_aMemory.Remove(memory);
            InitAddress();
            return "OK"; 
        }

        public void UpdateMemoryData()
        {
            Registry reg = m_pool.m_reg;
            string sID = p_id + '.'; 
            p_aMemory.Clear();
            int nMemory = reg.Read(sID + "Count", 0); 
            for (int n = 0; n < nMemory; n++)
            {
                string sMemory = reg.Read(sID + "Memory" + n.ToString(), "");
                MemoryData memory = CreateMemory(sMemory, 1, 1, new CPoint(1024, 1024));
            }
        }

        public List<string> m_asMemory = new List<string>();
        public void InitAddress()
        {
            _mbOffset = 0;
            foreach (MemoryData memory in p_aMemory) memory.InitAddress(ref _mbOffset);
            p_mbOffset = _mbOffset;
            m_asMemory.Clear();
            foreach (MemoryData memory in p_aMemory) m_asMemory.Add(memory.p_id);
        }

        public void RunTree(Tree tree, bool bVisible, bool bReadonly)
        {
            if (tree.p_treeRoot.p_eMode == Tree.eMode.RegWrite)
            {
                Registry reg = m_pool.m_reg;
                string sID = p_id + '.';
                reg.Write(sID + "Count", p_aMemory.Count); 
                for (int n = 0; n < p_aMemory.Count; n++)
                {
                    reg.Write(sID + "Memory" + n.ToString(), p_aMemory[n].p_id); 
                }
            }
            foreach (MemoryData memory in p_aMemory) memory.RunTree(tree.GetTree(memory.p_id, true, bVisible), bVisible, bReadonly);
            InitAddress();
        }
        #endregion

        public MemoryPool m_pool;
        public Log m_log; 
        public MemoryGroup(MemoryPool pool, string id)
        {
            p_aMemory = new ObservableCollection<MemoryData>(); 
            m_pool = pool; 
            m_log = pool.m_log; 
            p_id = id;
        }
    }
}
