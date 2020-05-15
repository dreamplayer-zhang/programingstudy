using RootTools.Comm;
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
        public delegate void dgOnChangeTool();
        public event dgOnChangeTool OnChangeTool;

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
            get
            {
                return m_fTotalPageFile;
            }
            set
            {
                SetProperty(ref m_fTotalPageFile, value);
            }
        }
        double m_fAvailPageFile = 0;
        public double p_fAvailPageFile
        {
            get
            {
                return m_fAvailPageFile;
            }
            set
            {
                SetProperty(ref m_fAvailPageFile, value);
            }
        }
        double m_fNotRootPageFile = 0;
        public double p_fNotRootPageFile
        {
            get
            {
                return m_fNotRootPageFile;
            }
            set
            {
                SetProperty(ref m_fNotRootPageFile, value);
            }
        }

        //ObservableCollection<UIElement> m_uiElement = new ObservableCollection<UIElement>();
        //public ObservableCollection<UIElement> p_uiElement
        //{
        //    get
        //    {
        //        return m_uiElement;
        //    }
        //    set
        //    {
        //        SetProperty(ref m_uiElement, value);
        //    }
        //}
        #endregion

        #region Pool
        public List<string> m_asPool = new List<string>(); 
        void InitPoolNames()
        {
            m_asPool.Clear();
            foreach (MemoryPool pool in m_aPool) m_asPool.Add(pool.p_id);
            RunTree(Tree.eMode.Init); 
            if (OnChangeTool != null) OnChangeTool(); 
        }

        public ObservableCollection<MemoryPool> m_aPool = new ObservableCollection<MemoryPool>();
        public ObservableCollection<MemoryPool> p_aPool
        {
            get
            {
                return m_aPool;
            }
            set
            {
                SetProperty(ref m_aPool, value);
            }
        }

        public MemoryPool GetPool(string sPool, bool bCreate)
        {
            foreach (MemoryPool pool in m_aPool)
            {
                if (pool.p_id == sPool) return pool;
            }
            if (bCreate == false) return null; 
            MemoryPool memoryPool = new MemoryPool(sPool, m_log);
            m_aPool.Add(memoryPool);
            InitPoolNames();
            return memoryPool;
        }

        public MemoryData GetMemory(string sPool, string sGroup, string sMemory)
        {
            MemoryPool pool = GetPool(sPool, false);
            return (pool == null) ? null : pool.GetMemory(sGroup, sMemory); 
        }

        public string DeletePool(string sPool)
        {
            MemoryPool memoryPool = GetPool(sPool, false);
            if (memoryPool != null) return "Memory Pool Not Exist";
            m_aPool.Remove(memoryPool);
            InitPoolNames();
            return "OK";
        }

        void RunPoolTree(Tree tree, bool bVisible)
        {
            int nPool = m_aPool.Count;
            nPool = tree.Set(nPool, nPool, "Count", "Pool Count", bVisible); 
            for (int n = 0; n < nPool; n++)
            {
                string sPool = (m_aPool.Count > n) ? m_aPool[n].p_id : "Pool";
                sPool = tree.Set(sPool, sPool, "sPool." + n.ToString(), "Pool Name", bVisible);
                MemoryPool memoryPool = GetPool(sPool, true); 
                int gbPool = (m_aPool.Count > n) ? m_aPool[n].p_gbPool : 0;
                gbPool = tree.Set(gbPool, 0, "gbPool." + n.ToString(), "Pool size gb", bVisible);
                memoryPool.p_gbPool = gbPool; 
            }
            foreach (MemoryPool pool in m_aPool) pool.RunTree(tree.GetTree(pool.p_id), bVisible); 
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
                    catch (Exception e) { p_sInfo = m_id + " StartProcess Error : " + e.Message; }
                }
            }
        }

        bool m_bStartProcess = false;
        string m_idProcess = "Root_Memory";
        string m_sProcessFile = ""; 
        void RunProcessTree(Tree tree, bool bVisible)
        {
            m_bStartProcess = tree.Set(m_bStartProcess, m_bStartProcess, "Start", "Start Memory Process", bVisible);
            m_idProcess = tree.Set(m_idProcess, m_idProcess, "ID", "Memory Process ID", bVisible && m_bStartProcess); 
            m_sProcessFile =tree.SetFile(m_sProcessFile, m_sProcessFile, "exe", "File", "Process File Name", bVisible && m_bStartProcess);
        }
        #endregion

        #region NamedPipe
        public List<NamedPipe> m_aNamedPipe = new List<NamedPipe>(); 
        public NamedPipe AddNamedPipe(string id)
        {
            foreach (NamedPipe pipe in m_aNamedPipe)
            {
                if (pipe.p_id == id) return pipe; 
            }
            NamedPipe namedPipe = new NamedPipe(id, m_log);
            m_aNamedPipe.Add(namedPipe);
            namedPipe.ReadMsg += NamedPipe_ReadMsg;
            return namedPipe; 
        }

        void SendCommand(string sMsg)
        {
            if (m_bStartProcess == false) return;
            foreach (NamedPipe pipe in m_aNamedPipe) pipe.Send(sMsg);
        }

        private void NamedPipe_ReadMsg(string sMsg)
        {
            if (sMsg.Contains("OnUpdateMemory"))
            {
                m_aPool.Clear(); 
                RunSetupTree(Tree.eMode.RegRead);
            }
        }
        #endregion

        #region Tree
        private void M_treeRootSetup_UpdateTree()
        {
            RunSetupTree(Tree.eMode.Update);
            RunSetupTree(Tree.eMode.RegWrite); 
        }

        public void RunSetupTree(Tree.eMode mode)
        {
            m_treeRootSetup.p_eMode = mode;
            RunPoolTree(m_treeRootSetup.GetTree("Pool"), true); 
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite); 
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunRunTree(m_treeRoot.GetTree("Run"));
            foreach (KeyValuePair<eRun, IRun> kv in m_dicRun)
            {
                kv.Value.RunTree(m_treeRoot.GetTree(kv.Key.ToString()), kv.Key == p_eRun); 
            }
            bool bVisible = (m_engineer.p_user.m_eLevel >= Login.eLevel.Admin); 
            RunProcessTree(m_treeRoot.GetTree("Process", false), bVisible);
        }
        #endregion

        public string p_id { get { return m_id; } }
        string m_id;
        IEngineer m_engineer;
        Log m_log;
        public TreeRoot m_treeRootSetup;
        public TreeRoot m_treeRoot;
        public MemoryTool(string id, IEngineer engineer, bool bRegRead = true)
        {
            MEMORYSTATUSEX stats = GlobalMemoryStatusEx();
            p_fTotalPageFile = stats.ullTotalPageFile / c_fGB;
            p_fAvailPageFile = stats.ullAvailPageFile / c_fGB;
            p_fNotRootPageFile = p_fTotalPageFile - p_fAvailPageFile;
            m_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);
            InitRuns(); 
            m_treeRootSetup = new TreeRoot("Memory", m_log);
            m_treeRootSetup.UpdateTree += M_treeRootSetup_UpdateTree;
            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            if (bRegRead) RunSetupTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.RegRead);
            InitThreadProcess();
            AddNamedPipe(id); 
        }

        public void ThreadStop()
        {
            if (m_bThreadProcess)
            {
                m_bThreadProcess = false;
                m_threadProcess.Join(); 
            }
            foreach (NamedPipe pipe in m_aNamedPipe) pipe.ThreadStop(); 
        }

        #region IRun
        interface IRun
        {
            void RunTree(Tree tree, bool bVisible);
            string Run();
        }

        public eRun _eRun = eRun.Create_Group;
        public eRun p_eRun
        {
            get { return _eRun; }
            set
            {
                _eRun = value;
                SetProperty(ref _eRun, value);
            }
        }

        public void ButtonRun()
        {
            p_sInfo = m_dicRun[p_eRun].Run();
            SendCommand("OnUpdateMemory");
            RunTree(Tree.eMode.Init);
            RunSetupTree(Tree.eMode.RegWrite);
            RunSetupTree(Tree.eMode.Init);
        }

        void RunRunTree(Tree tree)
        {
            p_eRun = (eRun)tree.Set(p_eRun, p_eRun, "Run", "Select Run Mode");
        }
        #endregion

        #region eRun
        public enum eRun
        {
            Create_Pool,
            Create_Group,
            Create_Memory,
            Delete_Pool, 
            Delete_Group,
            Delete_Memory,
            Save_Memory,
            Read_Memory
        }
        Dictionary<eRun, IRun> m_dicRun = new Dictionary<eRun, IRun>(); 
        void InitRuns()
        {
            m_dicRun.Add(eRun.Create_Pool, new Run_CreatePool(this));
            m_dicRun.Add(eRun.Create_Group, new Run_CreateGroup(this));
            m_dicRun.Add(eRun.Create_Memory, new Run_CreateMemory(this));
            m_dicRun.Add(eRun.Delete_Pool, new Run_DeletePool(this));
            m_dicRun.Add(eRun.Delete_Group, new Run_DeleteGroup(this));
            m_dicRun.Add(eRun.Delete_Memory, new Run_DeleteMemory(this));
            m_dicRun.Add(eRun.Save_Memory, new Run_SaveMemory(this));
            m_dicRun.Add(eRun.Read_Memory, new Run_ReadMemory(this));
        }
        #endregion

        #region Run Class
        class Run_Nothing : IRun
        {
            public void RunTree(Tree tree, bool bVisible) { }
            public string Run() { return "OK"; }
        }

        class Run_CreatePool : IRun
        {
            string m_sPool = "Pool";
            int m_gbPool = 1;
            MemoryTool m_tool; 
            public Run_CreatePool(MemoryTool memoryTool)
            {
                m_tool = memoryTool; 
            }

            public void RunTree(Tree tree, bool bVisible)
            {
                m_sPool = tree.Set(m_sPool, "Pool", "Pool", "Pool Name", bVisible);
                m_gbPool = tree.Set(m_gbPool, 1, "Size", "Pool Size (GB)", bVisible); 
            }

            public string Run()
            {
                MemoryPool memoryPool = m_tool.GetPool(m_sPool, true);
                memoryPool.p_gbPool = m_gbPool; 
                if (m_tool.OnChangeTool != null) m_tool.OnChangeTool(); 
                return "OK"; 
            }
        }

        class Run_DeletePool : IRun
        {
            string m_sPool = "Pool";
            MemoryTool m_tool;
            public Run_DeletePool(MemoryTool memoryTool)
            {
                m_tool = memoryTool;
            }

            public void RunTree(Tree tree, bool bVisible)
            {
                m_sPool = tree.Set(m_sPool, "Pool", m_tool.m_asPool, "Pool", "Pool Name", bVisible);
            }

            public string Run()
            {
                string sInfo = m_tool.DeletePool(m_sPool);
                if (m_tool.OnChangeTool != null) m_tool.OnChangeTool();
                return sInfo;
            }
        }

        class Run_CreateGroup : IRun
        {
            string m_sPool = "Pool";
            string m_sGroup = "Group";
            MemoryTool m_tool;
            public Run_CreateGroup(MemoryTool memoryTool)
            {
                m_tool = memoryTool;
            }

            public void RunTree(Tree tree, bool bVisible)
            {
                m_sPool = tree.Set(m_sPool, "Pool", m_tool.m_asPool, "Pool", "Pool Name", bVisible);
                m_sGroup = tree.Set(m_sGroup, m_sGroup, "Group", "Group Name", bVisible);
            }

            public string Run()
            {
                MemoryPool pool = m_tool.GetPool(m_sPool, false);
                if (pool == null) return "Pool not Found";
                pool.GetGroup(m_sGroup);
                return "OK"; 
            }
        }

        class Run_DeleteGroup : IRun
        {
            string m_sPool = "Pool";
            string m_sGroup = "Group";
            MemoryTool m_tool;
            public Run_DeleteGroup(MemoryTool memoryTool)
            {
                m_tool = memoryTool;
            }

            public void RunTree(Tree tree, bool bVisible)
            {
                m_sPool = tree.Set(m_sPool, "Pool", m_tool.m_asPool, "Pool", "Pool Name", bVisible);
                MemoryPool pool = m_tool.GetPool(m_sPool, false);
                if (pool == null) return; 
                m_sGroup = tree.Set(m_sGroup, m_sGroup, pool.m_asGroup, "Group", "Group Name", bVisible);
            }

            public string Run()
            {
                MemoryPool pool = m_tool.GetPool(m_sPool, false);
                if (pool == null) return "Pool not Found";
                return pool.DeleteGroup(m_sGroup);
            }
        }

        class Run_CreateMemory : IRun
        {
            string m_sPool = "Pool";
            string m_sGroup = "Group";
            string m_sMemory = "Memory";
            MemoryTool m_tool;
            public Run_CreateMemory(MemoryTool memoryTool)
            {
                m_tool = memoryTool;
            }

            int m_nCount = 1;
            int m_nByte = 1;
            CPoint m_sz = new CPoint(1024, 1024);
            public void RunTree(Tree tree, bool bVisible)
            {
                m_sPool = tree.Set(m_sPool, "Pool", m_tool.m_asPool, "Pool", "Pool Name", bVisible);
                MemoryPool pool = m_tool.GetPool(m_sPool, false);
                if (pool == null) return;
                m_sGroup = tree.Set(m_sGroup, m_sGroup, pool.m_asGroup, "Group", "Group Name", bVisible);
                MemoryGroup group = pool.GetGroup(m_sGroup);
                if (group == null) return;
                m_sMemory = tree.Set(m_sMemory, m_sMemory, "Name", "Memory Name", bVisible);
                m_nCount = tree.Set(m_nCount, m_nCount, "Count", "Memory Count", bVisible);
                m_nByte = tree.Set(m_nByte, m_nByte, "Byte", "Memory Pixel Byte (1 = Gray, 3 = RGB ...)", bVisible);
                m_sz = tree.Set(m_sz, m_sz, "Size", "Buffer Size (x, y)", bVisible);
            }

            public string Run()
            {
                MemoryPool pool = m_tool.GetPool(m_sPool, false);
                if (pool == null) return "Pool not Found";
                MemoryGroup group = pool.GetGroup(m_sGroup);
                if (group == null) return "Group not Found";
                group.CreateMemory(m_sMemory, m_nCount, m_nByte, m_sz.X, m_sz.Y);
                return "OK"; 
            }
        }

        class Run_DeleteMemory : IRun
        {
            string m_sPool = "Pool";
            string m_sGroup = "Group";
            string m_sMemory = "Memory";
            MemoryTool m_tool;
            public Run_DeleteMemory(MemoryTool memoryTool)
            {
                m_tool = memoryTool;
            }

            public void RunTree(Tree tree, bool bVisible)
            {
                m_sPool = tree.Set(m_sPool, "Pool", m_tool.m_asPool, "Pool", "Pool Name", bVisible);
                MemoryPool pool = m_tool.GetPool(m_sPool, false);
                if (pool == null) return;
                m_sGroup = tree.Set(m_sGroup, m_sGroup, pool.m_asGroup, "Group", "Group Name", bVisible);
                MemoryGroup group = pool.GetGroup(m_sGroup);
                if (group == null) return;
                m_sMemory = tree.Set(m_sMemory, m_sMemory, group.m_asMemory, "Name", "Memory Name", bVisible);
            }

            public string Run()
            {
                MemoryPool pool = m_tool.GetPool(m_sPool, false);
                if (pool == null) return "Pool not Found";
                MemoryGroup group = pool.GetGroup(m_sGroup);
                if (group == null) return "Group not Found"; 
                return group.DeleteMemory(m_sMemory);
            }
        }

        class Run_SaveMemory : IRun
        {
            string m_sPool = "Pool";
            string m_sGroup = "Group";
            string m_sMemory = "Memory";
            MemoryTool m_tool;
            public Run_SaveMemory(MemoryTool memoryTool)
            {
                m_tool = memoryTool; 
            }

            int m_nIndex = 0;
            CPoint m_cp = new CPoint();
            CPoint m_sz = new CPoint();
            MemoryData m_memory = null; 
            public void RunTree(Tree tree, bool bVisible)
            {
                m_memory = null; ; 
                m_sPool = tree.Set(m_sPool, "Pool", m_tool.m_asPool, "Pool", "Pool Name", bVisible);
                MemoryPool pool = m_tool.GetPool(m_sPool, false);
                if (pool == null) return;
                m_sGroup = tree.Set(m_sGroup, m_sGroup, pool.m_asGroup, "Group", "Group Name", bVisible);
                MemoryGroup group = pool.GetGroup(m_sGroup);
                if (group == null) return;
                m_sMemory = tree.Set(m_sMemory, m_sMemory, group.m_asMemory, "Name", "Memory Name", bVisible);
                m_memory = group.GetMemory(m_sMemory);
                if (m_memory == null) return;
                if (m_sz.X == 0) m_sz = new CPoint(m_memory.p_sz); 
                m_nIndex = tree.Set(m_nIndex, m_nIndex, "Index", "Memory Index", bVisible && (m_memory.p_nCount > 1));
                m_cp = tree.Set(m_cp, m_cp, "Org", "Memory ROI Origin Position", bVisible);
                m_sz = tree.Set(m_sz, m_sz, "Size", "Memory ROI Size", bVisible); 
            }

            public string Run()
            {
                if (m_memory == null) return "Memory not Exist"; 
                return m_memory.SaveMemory(m_nIndex, m_cp.X, m_cp.Y, m_cp.X + m_sz.X, m_cp.Y + m_sz.Y); 
            }
        }

        class Run_ReadMemory : IRun
        {
            string m_sPool = "Pool";
            string m_sGroup = "Group";
            string m_sMemory = "Memory";
            MemoryTool m_tool;
            public Run_ReadMemory(MemoryTool memoryTool)
            {
                m_tool = memoryTool;
            }

            int m_nIndex = 0;
            MemoryData m_memory = null;
            public void RunTree(Tree tree, bool bVisible)
            {
                m_memory = null; ;
                m_sPool = tree.Set(m_sPool, "Pool", m_tool.m_asPool, "Pool", "Pool Name", bVisible);
                MemoryPool pool = m_tool.GetPool(m_sPool, false);
                if (pool == null) return;
                m_sGroup = tree.Set(m_sGroup, m_sGroup, pool.m_asGroup, "Group", "Group Name", bVisible);
                MemoryGroup group = pool.GetGroup(m_sGroup);
                if (group == null) return;
                m_sMemory = tree.Set(m_sMemory, m_sMemory, group.m_asMemory, "Name", "Memory Name", bVisible);
                m_memory = group.GetMemory(m_sMemory);
                if (m_memory == null) return;
                m_nIndex = tree.Set(m_nIndex, m_nIndex, "Index", "Memory Index", bVisible && (m_memory.p_nCount > 1));
            }

            public string Run()
            {
                if (m_memory == null) return "Memory not Exist";
                return m_memory.ReadMemory(m_nIndex); 
            }
        }
        #endregion

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
}
