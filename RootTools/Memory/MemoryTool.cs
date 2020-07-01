using RootTools.Inspects;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Data;
using System.Windows.Threading;

namespace RootTools.Memory
{
    public class MemoryTool : ObservableObject, IToolSet
    {
        const double c_fGB = 1024 * 1024 * 1024;

        #region Property
        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                if (value == "OK") return;
                m_log.Warn("p_sInfo = " + value);
            }
        }
        
        double m_fTotalPageFile = 0;
        public double p_fTotalPageFile
        {
            get { return m_fTotalPageFile; } 
            set { SetProperty(ref m_fTotalPageFile, value); }
        }
        
        double m_fAvailPageFile = 0;
        public double p_fAvailPageFile
        {
            get { return m_fAvailPageFile; }
            set { SetProperty(ref m_fAvailPageFile, value); }
        }
        
        double m_fNotRootPageFile = 0;
        public double p_fNotRootPageFile
        {
            get { return m_fNotRootPageFile; } 
            set { SetProperty(ref m_fNotRootPageFile, value); }
        }
        #endregion

        #region Memory Pool
        public delegate void dgOnChangeMemoryPool();
        public event dgOnChangeMemoryPool OnChangeMemoryPool;

        public List<string> m_asPool = new List<string>(); 
        public void MemoryPoolChanged()
        {
            m_asPool.Clear();
            foreach (MemoryPool pool in p_aPool) m_asPool.Add(pool.p_id);
            if (OnChangeMemoryPool != null) OnChangeMemoryPool(); 
        }

        ObservableCollection<MemoryPool> _aPool = new ObservableCollection<MemoryPool>();
        public ObservableCollection<MemoryPool> p_aPool
        {
            get { return _aPool; }
            set { SetProperty(ref _aPool, value); }
        }

        public MemoryPool GetPool(string sPool, bool bCreate)
        {
            foreach (MemoryPool pool in p_aPool)
            {
                if (pool.p_id == sPool) return pool;
            }
            if (bCreate == false) return null;
            MemoryPool memoryPool = new MemoryPool(sPool, this);
            p_aPool.Add(memoryPool);
            MemoryPoolChanged();
            return memoryPool;
        }

        public string DeletePool(string sPool)
        {
            MemoryPool memoryPool = GetPool(sPool, false);
            if (memoryPool != null) return "Memory Pool Not Exist";
            p_aPool.Remove(memoryPool);
            MemoryPoolChanged();
            return "OK";
        }

        /// <summary> Registry 관리용 </summary>
        void RunTreeMemory(Tree tree, bool bVisible)
        {
            int nPool = p_aPool.Count;
            bool bPool = (m_bMaster == false) || (tree.p_treeRoot.p_eMode != Tree.eMode.RegRead);
            if (bPool) nPool = tree.Set(nPool, nPool, "Count", "Pool Count", bVisible);
            for (int n = 0; n < nPool; n++) RunTreeMemory(tree.GetTree(n.ToString("00")), n, bVisible);
            foreach (MemoryPool pool in p_aPool) pool.RunTreeMemory(tree.GetTree(pool.p_id), bVisible);
        }

        void RunTreeMemory(Tree tree, int n, bool bVisible)
        {
            string sPool = (p_aPool.Count > n) ? p_aPool[n].p_id : "Pool";
            sPool = tree.Set(sPool, sPool, "Name", "Pool Name", bVisible);
            MemoryPool memoryPool = GetPool(sPool, true);
            int gbPool = (p_aPool.Count > n) ? p_aPool[n].p_gbPool : 1;
            gbPool = tree.Set(gbPool, 1, "Size", "Pool size (Giga Byte)", bVisible);
            if (m_bMaster == false) memoryPool.p_gbPool = gbPool;
        }
        #endregion

        #region Memoey
        public void MemoryChanged(bool bUpdate)
        {
            if (m_bMaster == false) return;
            if (bUpdate == false) RunTreeMemory(Tree.eMode.RegRead);
            RunTreeMemory(Tree.eMode.RegWrite);
            RunTreeMemory(Tree.eMode.Init);
            Process[] aProcess = Process.GetProcessesByName(m_idProcess);
            foreach (Process process in aProcess) process.Kill();
            KillInspectProcess(); 
        }

        public MemoryData GetMemory(string sPool, string sGroup, string sMemory)
        {
            MemoryPool pool = GetPool(sPool, false);
            return (pool == null) ? null : pool.GetMemory(sGroup, sMemory);
        }
        #endregion

        #region MemoryProcess
        bool m_bThreadProcess = false;
        Thread m_threadProcess = null;
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitThreadProcess()
        {
            m_threadProcess = new Thread(new ThreadStart(RunThreadProcess));
            m_threadProcess.Start();

            m_timer.Interval = TimeSpan.FromMilliseconds(10000);
            m_timer.Tick += m_timer_Tick;
            m_timer.Start();
        }

        void m_timer_Tick(object sender, EventArgs e)
        {
            MEMORYSTATUSEX stats = GlobalMemoryStatusEx();
            p_fTotalPageFile = stats.ullTotalPageFile / c_fGB;
            p_fAvailPageFile = stats.ullAvailPageFile / c_fGB;
        }

        void RunThreadProcess()
        {
            m_bThreadProcess = true;
            Thread.Sleep(10000);
            while (m_bThreadProcess)
            {
                Thread.Sleep(100);
                if (m_bStartProcess)
                {
                    try
                    {
                        Process[] aProcess = Process.GetProcessesByName(m_idProcess);
                        if (aProcess.Length == 0) Process.Start(m_sProcessFile);
                    }
                    catch (Exception e) { p_sInfo = p_id + " StartProcess Error : " + e.Message; }
                }
            }
        }

        bool m_bStartProcess = false;
        string m_idProcess = "Root_Memory";
        string m_sProcessFile = "";
        void RunTreeProcess(Tree tree, bool bVisible)
        {
            m_bStartProcess = tree.Set(m_bStartProcess, m_bStartProcess, "Start", "Start Memory Process", bVisible);
            m_idProcess = tree.Set(m_idProcess, m_idProcess, "ID", "Memory Process ID", bVisible && m_bStartProcess);
            m_sProcessFile = tree.SetFile(m_sProcessFile, m_sProcessFile, "exe", "File", "Process File Name", bVisible && m_bStartProcess);
        }
        #endregion

        #region Read & Save Memory
        public string SaveMemory()
        {
            if (m_memory == null) return "Memory not Exist";
            return m_memory.SaveMemory(m_nIndex, m_cp.X, m_cp.Y, m_cp.X + m_sz.X, m_cp.Y + m_sz.Y);
        }

        public string ReadMemory()
        {
            if (m_memory == null) return "Memory not Exist";
            return m_memory.ReadMemory(m_nIndex);
        }

        string m_sPool = "Pool";
        string m_sGroup = "Group";
        string m_sMemory = "Memory";
        int m_nIndex = 0;
        MemoryData m_memory = null;
        CPoint m_cp = new CPoint();
        CPoint m_sz = new CPoint();
        void RunTreeFile(Tree tree)
        {
            m_memory = null;
            m_sPool = tree.Set(m_sPool, "Pool", m_asPool, "Pool", "Pool Name");
            MemoryPool pool = GetPool(m_sPool, false);
            if (pool == null) return;
            m_sGroup = tree.Set(m_sGroup, m_sGroup, pool.m_asGroup, "Group", "Group Name");
            MemoryGroup group = pool.GetGroup(m_sGroup, false);
            if (group == null) return;
            m_sMemory = tree.Set(m_sMemory, m_sMemory, group.m_asMemory, "Name", "Memory Name");
            m_memory = group.GetMemory(m_sMemory);
            if (m_memory == null) return;
            if (m_sz.X == 0) m_sz = new CPoint(m_memory.p_sz);
            m_nIndex = tree.Set(m_nIndex, m_nIndex, "Index", "Memory Index", m_memory.p_nCount > 1);
            m_cp = tree.Set(m_cp, m_cp, "Org", "Memory ROI Origin Position");
            m_sz = tree.Set(m_sz, m_sz, "Size", "Memory ROI Size");
        }
        #endregion

        #region Tree
        private void M_treeRootMemory_UpdateTree()
        {
            RunTreeMemory(Tree.eMode.Update);
            RunTreeMemory(Tree.eMode.RegWrite);
            RunTreeMemory(Tree.eMode.Init);
        }

        public void RunTreeMemory(Tree.eMode mode)
        {
            m_treeRootMemory.p_eMode = mode;
            RunTreeMemory(m_treeRootMemory.GetTree("Memory"), true);
        }

        private void M_treeRootRun_UpdateTree()
        {
            RunTreeRun(Tree.eMode.Update);
            RunTreeRun(Tree.eMode.RegWrite); 
        }

        public void RunTreeRun(Tree.eMode mode)
        {
            m_treeRootRun.p_eMode = mode;
            RunTreeFile(m_treeRootRun.GetTree("File"));
            if (m_bMaster == false) return; 
            bool bVisible = (m_engineer.p_user.m_eLevel >= Login.eLevel.Admin); 
            RunTreeProcess(m_treeRootRun.GetTree("Process", false), bVisible);
        }
        #endregion

        #region Inspect Process
        public void KillInspectProcess()
        {
            if (m_bMaster == false) return;
            Process[] aProcess = Process.GetProcessesByName(InspectTool.m_idProcess);
            foreach (Process process in aProcess) process.Kill();
        }
        #endregion

        public string p_id { get; set; }
        bool m_bMaster = true; 
        IEngineer m_engineer;
        public Log m_log;
        public TreeRoot m_treeRootMemory;
        public TreeRoot m_treeRootRun;
        public MemoryViewer m_viewer; 
        public MemoryTool(string id, IEngineer engineer, bool bMaster = true)
        {
            MEMORYSTATUSEX stats = GlobalMemoryStatusEx();
            p_fTotalPageFile = stats.ullTotalPageFile / c_fGB;
            p_fAvailPageFile = stats.ullAvailPageFile / c_fGB;
            p_fNotRootPageFile = p_fTotalPageFile - p_fAvailPageFile;
            p_id = id;
            m_engineer = engineer;
            m_bMaster = bMaster; 
            m_log = LogView.GetLog(id);
            m_treeRootMemory = new TreeRoot("Memory", m_log, false, "Memory");
            m_treeRootMemory.UpdateTree += M_treeRootMemory_UpdateTree;
            m_treeRootRun = new TreeRoot(id, m_log);
            m_treeRootRun.UpdateTree += M_treeRootRun_UpdateTree;
            RunTreeRun(Tree.eMode.RegRead);
            RunTreeMemory(Tree.eMode.RegRead);
            m_viewer = new MemoryViewer(id, this, m_log);
            KillInspectProcess();
            InitThreadProcess();
        }

        public void ThreadStop()
        {
            if (m_bThreadProcess)
            {
                m_bThreadProcess = false;
                m_threadProcess.Join(); 
            }
        }

        #region MemCheck
        [DllImport("kernel32", EntryPoint = "GetLastError")]
        private extern static int __GetLastError();
        [DllImport("Kernel32.dll", EntryPoint = "GlobalMemoryStatusEx", SetLastError = true)]
        private extern static bool __GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        public static MEMORYSTATUSEX GlobalMemoryStatusEx()
        {
            MEMORYSTATUSEX memstat = new MEMORYSTATUSEX();

            memstat.dwLength = (uint)Marshal.SizeOf(memstat);
            if (__GlobalMemoryStatusEx(ref memstat) == false)
            {
                int error = __GetLastError();
                throw new Win32Exception(error);
            }
            return memstat;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }
        #endregion 
    }

    #region Converter
    public class TestConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                double fFullPage = ((MemoryTool)((MemoryTool_UI)values[2]).DataContext).p_fTotalPageFile;
                double FullWidth = (double)values[1];
                int nPoolGb = (int)values[0];
                double width = FullWidth * nPoolGb / fFullPage;
                return width;
            }
            catch
            {
                return 0;
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class TestConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                MemoryTool tool = ((MemoryTool)((MemoryTool_UI)values[1]).DataContext);
                double fFullPage = tool.p_fTotalPageFile;
                double fAvailPage = tool.p_fAvailPageFile;
                double FullWidth = (double)values[0];
                int FullGb = 0;
                for (int i = 0; i < tool.p_aPool.Count; i++)
                {
                    FullGb += tool.p_aPool[i].p_gbPool;
                }
                double width = FullWidth * (fFullPage - fAvailPage - FullGb) / fFullPage;
                return width;
            }
            catch
            {
                return 0;
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
    #endregion 
}
