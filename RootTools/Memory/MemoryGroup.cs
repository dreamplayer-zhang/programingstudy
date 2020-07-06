using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
                RaisePropertyChanged();
                RaisePropertyChanged("p_mbAvailable");
            }
        }

        public string p_mbAvailable
        {
            get { return (1024 * m_pool.p_gbPool - p_mbOffset).ToString() + " MB"; }
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
            int mbPool = 1024 * m_pool.p_gbPool; 
            MemoryData memory = GetMemory(id);
            if (memory != null) DeleteMemory(id); 
            memory = new MemoryData(this, id, nCount, nByte, xSize, ySize, ref _mbOffset);
            p_mbOffset = _mbOffset;
            if (p_mbOffset > mbPool) return null; 
            p_aMemory.Add(memory);
            InitAddress();
            m_pool.m_memoryTool.MemoryChanged(false); 
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

        public List<string> m_asMemory = new List<string>();
        public void InitAddress()
        {
            _mbOffset = 0;
            foreach (MemoryData memory in p_aMemory) memory.InitAddress(ref _mbOffset);
            p_mbOffset = _mbOffset;
            m_asMemory.Clear();
            foreach (MemoryData memory in p_aMemory) m_asMemory.Add(memory.p_id);
        }

        public void RunTreeMemory(Tree tree, bool bCount, bool bVisible)
        {
            int nMemory = p_aMemory.Count;
            if (bCount) nMemory = tree.Set(nMemory, nMemory, "Count", "Memory Count", bVisible); 
            for (int n = 0; n < nMemory; n++)
            {
                string sMemory = (p_aMemory.Count > n) ? p_aMemory[n].p_id : "Memory";
                sMemory = tree.Set(sMemory, sMemory, "sMemory." + n.ToString(), "Memory Name", bVisible);
                if (p_aMemory.Count <= n) CreateMemory(sMemory, 1, 1, 1, 1); 
            }
            foreach (MemoryData memory in p_aMemory) memory.RunTree(tree.GetTree(memory.p_id), bVisible);
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
