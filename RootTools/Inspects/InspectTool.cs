using RootTools.Comm;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools.Inspects
{
    public class InspectTool : ITool
    {

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
        #endregion

        #region Memory
        public MemoryPool m_memoryPool;
        #endregion

        #region NamedPipe
        public NamedPipe m_namedPipe;
        void InitNamedPipe(bool bHost)
        {
            if (bHost) m_namedPipe = m_memoryTool.AddNamedPipe(m_id);
            else m_namedPipe = m_memoryTool.m_aNamedPipe[0];
            m_namedPipe.ReadMsg += M_namedPipe_ReadMsg;
        }

        private void M_namedPipe_ReadMsg(string sMsg)
        {
            p_sInfo = ReadMessage(sMsg);
        }

        string ReadMessage(string sMsg)
        {
            string[] sMsgs = sMsg.Split(NamedPipe.c_cSeparate);
            if (sMsgs.Length < 3) return "Mesage Length Too Short : " + sMsg;
            string sCmd = sMsgs[2];
            string sIndex;
            string sRecipe;
            switch (sCmd)
            {
                case "Inspect":
                    if (sMsgs.Length < 5) return "Invalid Protocol : " + sMsg;
                    sIndex = sMsgs[3];
                    sRecipe = sMsgs[4];
                    AddInspect(sIndex, sRecipe);
                    break;
                case "Done":
                    if (sMsgs.Length < 5) return "Invalid Protocol : " + sMsg;
                    sIndex = sMsgs[3];
                    if ((m_aData.Count > 0) && (m_aData[0].p_sIndex == sIndex))
                    {
                        m_aData[0].p_sInfo = sMsgs[4];
                        if (OnInspectDone != null) OnInspectDone(m_aData[0]);
                        m_log.Info("Inspect Done : " + m_aData[0].m_id);
                        m_aData.RemoveAt(0);
                    }
                    m_bRemoteInspect = false;
                    break;
            }
            return "OK";
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                InspectTool_UI ui = new InspectTool_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        #region Process
        bool m_bThreadProcess = false;
        Thread m_threadProcess = null;
        void InitThreadProcess()
        {
            if (m_bHost == false) return;
            InitKillProcess();
            m_threadProcess = new Thread(new ThreadStart(RunThreadProcess));
            m_threadProcess.Start();
        }

        static List<int> m_aValidProcessID = new List<int>();
        void InitKillProcess()
        {
            Process[] aProcess = Process.GetProcessesByName(m_idProcess);
            foreach (Process process in aProcess)
            {
                if (IsInvalidProcess(process.Id)) process.Kill();
            }
        }

        bool IsInvalidProcess(int nID)
        {
            foreach (int nValidID in m_aValidProcessID)
            {
                if (nID == nValidID) return false;
            }
            return true;
        }

        int m_nProcessID = -1;
        void RunThreadProcess()
        {
            m_bThreadProcess = true;
            Thread.Sleep(1000);
            while (m_bThreadProcess)
            {
                Thread.Sleep(100);
                if (m_bStartProcess)
                {
                    try
                    {
                        if (IsProcessRun() == false)
                        {
                            Process process = Process.Start(m_sProcessFile, m_id);
                            m_nProcessID = process.Id;
                            m_aValidProcessID.Add(process.Id);
                            m_bRemoteInspect = false;
                        }
                    }
                    catch (Exception e) { p_sInfo = m_id + " StartProcess Error : " + e.Message; }
                }
            }
        }

        bool IsProcessRun()
        {
            Process[] aProcess = Process.GetProcessesByName(m_idProcess);
            if (aProcess.Length == 0) return false;
            foreach (Process process in aProcess)
            {
                if (process.Id == m_nProcessID) return true;
            }
            return false;
        }

        bool m_bStartProcess = false;
        string m_idProcess = "Root_Inspect";
        string m_sProcessFile = "";
        void RunProcessTree(Tree tree, bool bVisible)
        {
            m_bStartProcess = tree.Set(m_bStartProcess, m_bStartProcess, "Start", "Start Memory Process", bVisible);
            m_idProcess = tree.Set(m_idProcess, m_idProcess, "ID", "Inspect Process ID", bVisible && m_bStartProcess);
            m_sProcessFile = tree.SetFile(m_sProcessFile, m_sProcessFile, "exe", "File", "Process File Name", bVisible && m_bStartProcess);
        }
        #endregion

        #region Tree
        private void M_tree_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            //            RunRunTree(m_tree.GetTree("Run"));
            //            foreach (KeyValuePair<eRun, IRun> kv in m_dicRun)
            //            {
            //                kv.Value.RunTree(m_treeRoot.GetTree(kv.Key.ToString()), kv.Key == p_eRun);
            //            }
            bool bVisible = (m_engineer.p_user.m_eLevel >= Login.eLevel.Admin);
            RunProcessTree(m_treeRoot.GetTree("Process", false), bVisible);
        }
        #endregion

        #region Inspect Data
        public class Data : NotifyProperty
        {
            #region Property
            public string p_sIndex { get; set; }

            string _sInfo = "";
            public string p_sInfo
            {
                get { return _sInfo; }
                set
                {
                    if (value == _sInfo) return;
                    _sInfo = value;
                    OnPropertyChanged();
                    _sDone = DateTime.Now.ToLongTimeString();
                    OnPropertyChanged("p_sDone");
                    _secInspect = m_swInspect.ElapsedMilliseconds / 1000.0;
                    OnPropertyChanged("p_secInspect");
                }
            }

            string _sRecipe = "Recipe";
            public string p_sRecipe
            {
                get { return _sRecipe; }
                set
                {
                    _sRecipe = value;
                    OnPropertyChanged();
                }
            }

            string _sStart = "";
            public string p_sStart
            {
                get { return _sStart; }
            }

            public void SetStart()
            {
                _sStart = DateTime.Now.ToLongTimeString();
                m_swInspect.Restart();
                _nTry++;
                OnPropertyChanged("p_sStart");
                OnPropertyChanged("p_nTry");
            }

            string _sDone = "";
            public string p_sDone
            {
                get { return _sDone; }
            }

            double _secInspect = 0;
            public string p_secInspect
            {
                get { return _secInspect.ToString(".00") + " sec"; }
            }

            int _nTry = 0;
            public int p_nTry
            {
                get { return _nTry; }
            }

            bool _bRemote = false;
            public bool p_bRemote
            {
                get { return _bRemote; }
                set
                {
                    _bRemote = value;
                    OnPropertyChanged();
                }
            }
            #endregion

            public string m_id;
            StopWatch m_swInspect = new StopWatch();
            public Data(string sIndex, string sRecipe)
            {
                p_sIndex = sIndex;
                p_sRecipe = sRecipe;
                m_id = p_sIndex + "." + p_sRecipe;
            }
        }
        public List<Data> m_aData = new List<Data>();

        int m_iIndex = 0;
        public string AddInspect(string sRecipe, out int iIndex)
        {
            string sIndex = m_iIndex.ToString("000");
            iIndex = m_iIndex;
            m_iIndex++;
            return AddInspect(sIndex, sRecipe);
        }

        string AddInspect(string sIndex, string sRecipe)
        {
            Data data = new Data(sIndex, sRecipe);
            data.p_bRemote = m_bHost && m_bStartProcess;
            m_aData.Add(data);
            m_qDataLog.Enqueue(data);
            m_log.Info("Add Inspect : " + data.m_id);
            return "OK";
        }

        public bool IsReady()
        {
            return (m_aData.Count == 0);
        }

        public delegate void dgOnInspectDone(Data data);
        public event dgOnInspectDone OnInspectDone;

        bool m_bRemoteInspect = false;
        void ThreadInspect(Data data)
        {
            if (data.p_bRemote)
            {
                if (m_bRemoteInspect) return;
                m_log.Info("Inspect Start : " + data.m_id);
                m_bRemoteInspect = true;
                data.SetStart();
                m_namedPipe.Send("Inspect", data.p_sIndex, data.p_sRecipe);
            }
            else
            {
                m_log.Info("Inspect Start : " + data.m_id);
                data.SetStart();
                data.p_sInfo = ThreadInspect(data.p_sIndex, data.p_sRecipe);
                m_aData.RemoveAt(0);
                if (OnInspectDone != null) OnInspectDone(data);
                if (m_bHost == false) m_namedPipe.Send("Done", data.p_sIndex, data.p_sInfo);
                m_log.Info("Inspect Done : " + data.m_id);
            }
        }

        string ThreadInspect(string sIndex, string sRecipe)
        {
            Thread.Sleep(7000); //forget Inspect Here
            return "OK";
        }
        #endregion

        #region Inspect Log
        public ObservableCollection<Data> p_aDataLog = new ObservableCollection<Data>();
        Queue<Data> m_qDataLog = new Queue<Data>();

        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(10);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            while (m_qDataLog.Count > 0) p_aDataLog.Add(m_qDataLog.Dequeue());
        }
        #endregion

        #region Thread Inspect
        bool m_bThread = false;
        Thread m_thread;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(ThreadRun));
            m_thread.Start();
        }

        void ThreadRun()
        {
            m_bThread = true;
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(10);
                if (m_aData.Count > 0)
                {
                    Data data = m_aData[0];
                    if (data.p_nTry > 2)
                    {
                        EQ.p_bStop = true;
                        m_aData.RemoveAt(0);
                        data.p_sInfo = "Inspect Try Error";
                        if (OnInspectDone != null) OnInspectDone(data);
                    }
                    else
                    {
                        ThreadInspect(data);
                    }
                }
            }
        }
        #endregion

        public string p_id { get { return m_id; } }
        string m_id;
        IEngineer m_engineer;
        bool m_bHost;
        MemoryTool m_memoryTool;
        Log m_log;
        public TreeRoot m_treeRoot;
        public InspectTool(string id, IEngineer engineer, bool bHost)
        {
            m_id = id;
            m_engineer = engineer;
            m_bHost = bHost;
            m_memoryTool = engineer.ClassMemoryTool();
            m_memoryTool.AddNamedPipe(m_id + ".Memory");
            m_log = LogViewer.GetLog(id);
            m_memoryPool = null;
            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_tree_UpdateTree;
            InitThreadProcess();
            InitNamedPipe(bHost);
            RunTree(Tree.eMode.RegRead);
            InitThread();
            InitTimer();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join();
            }
            if (m_bThreadProcess)
            {
                m_bThreadProcess = false;
                m_threadProcess.Join();
            }
            m_namedPipe.ThreadStop();
        }
    }
}
