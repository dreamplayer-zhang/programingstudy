using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.MemoryMappedFiles;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools.Memory
{
    public class MemoryPool : NotifyProperty, ITool
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
                    if (m_memoryTool != null && m_memoryTool.m_bMaster)
                    {
                        OpenPool();
                        if (m_MMF == null) CreatePool(value);
                    }
                }
                _fGB = value;
                OnPropertyChanged(); 
                m_reg.Write("fGB", value);
            }
        }

        bool CreatePool(double fGB)
        {
            StopWatch sw = new StopWatch();
            long nPool = (long)Math.Ceiling(fGB * c_fGB);
            try
            {
                m_MMF = MemoryMappedFile.CreateOrOpen(p_id, nPool);
                GetAddress();
            }
            catch (Exception e)
            {
                m_log.Error(p_id + " Memory Pool Create Error (ReStart PC) : " + e.Message);
                System.Windows.Forms.MessageBox.Show(p_id + " Memory Pool Create Error(ReStart PC) : " + e.Message);
                return true;
            }
            m_log.Info(p_id + " Memory Pool Create Done " + sw.ElapsedMilliseconds.ToString() + " ms");
            return false;
        }

        public string OpenPool()
        {
            try
            {
                if (m_MMF == null)
                {
                    m_MMF = MemoryMappedFile.OpenExisting(p_id);
                    GetAddress();
                }
            }
            catch(Exception e)
            { return "Open Error" + e.ToString(); }
            m_log?.Info(p_id + " Memory Pool Open Done");
            return (m_MMF != null) ? "OK" : "Error"; 
        }

        public long m_pAddress = 0; 
        void GetAddress()
        {
            unsafe
            {
                byte* p = null;
                m_MMF.CreateViewAccessor().SafeMemoryMappedViewHandle.AcquirePointer(ref p);
                m_pAddress = (long)(IntPtr)p; 
            }
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
            int nGroup = m_reg?.Read("Count", 0);
            for (int n = 0; n < nGroup; n++)
            {
                string sGroup = m_reg.Read("Group" + n.ToString(), "");
                MemoryGroup group = GetGroup(sGroup, true);
                group.UpdateMemoryData();
            }
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer(); 
        void InitTimer()
        {
            if (m_memoryTool == null) return;
            if (m_memoryTool.m_bMaster) return;
            m_log.Info(p_id + " Start Timer"); 
            m_timer.Interval = TimeSpan.FromSeconds(0.2);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            OpenPool();
            if (m_MMF == null) return;
            UpdateMemoryData();
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
            m_timer.Stop(); 
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
                group.RunTree(m_treeRoot, bVisible, m_memoryTool == null ? false : !m_memoryTool.m_bMaster);
            }
            if (m_treeRoot.IsUpdated())
            {
                if(m_memoryTool != null)
                    m_memoryTool.MemoryChanged();
            }
        }
        #endregion

        public string p_id { get; set; }
        public Log m_log;
        public Registry m_reg; 
        public MemoryMappedFile m_MMF = null;
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
            InitTimer(); 
        }


        /// <summary>
        /// RootUI(m_memoryTool)을 사용하지 않고 메모리를 사용하기 위해서
        /// </summary>
        /// <param name="id"></param>
        public MemoryPool(string id)
        {
            p_aGroup = new ObservableCollection<MemoryGroup>();
            p_id = id;
            m_reg = new Registry(p_id, "MemoryTools");
            p_fGB = m_reg.Read("fGB", p_fGB);

            InitTree();
            OpenPool();
            UpdateMemoryData();
        }

        public void ThreadStop()
        {
        }
    }
}
