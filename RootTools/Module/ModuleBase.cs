using RootTools.Camera;
using RootTools.Comm;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Lens;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RootTools.Module
{
    public class ModuleBase : NotifyProperty
    {
        
        #region eState
        public delegate void dgOnChangeState(eState eState);
        public event dgOnChangeState OnChangeState;

        public enum eState
        {
            Init,
            Home,
            Ready,
            Run,
            Error
        };
        protected eState _eState = eState.Error;
        public eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                m_log.Info("State : " + _eState.ToString() + " -> " + value.ToString()); 
                _eState = value;
                EQ.p_bPause = false;
                OnPropertyChanged();
                if (OnChangeState != null) OnChangeState(value); 
            }
        }

        string[] m_asState = Enum.GetNames(typeof(eState));
        eState GetState(string sState)
        {
            for (int n = 0; n < m_asState.Length; n++)
            {
                if (sState == m_asState[n]) return (eState)n;
            }
            return eState.Error;
        }
        
        protected int _nProgress = 0;
        public int p_nProgress
        {
            get { return _nProgress; }
            set 
            { 
                //if (_nProgress == value) return;
                _nProgress = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy()
        {
            if (p_eState == eState.Run) return true;
            if (p_eState == eState.Home) return true;
            return m_qModuleRun.Count > 0; 
        }
        #endregion

        #region Property
        public Listp_sInfo m_infoList = new Listp_sInfo();
        string _sInfo = "Info";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value == "OK") return;
                m_log?.Info("Info : " + _sInfo);
                m_infoList.Add(_sInfo);
                EQ.p_sInfo = p_id + " : " + value; 
            }
        }
        public bool Run(string sInfo)
        {
            p_sInfo = sInfo;
            if (EQ.IsStop()) p_sInfo = "EQ Stop";
            return sInfo != "OK";
        }

        ICamera m_GrabCam;
        public ICamera p_GrabCam
        {
            get
            {
                return m_GrabCam;
            }
            set
            {
                m_GrabCam = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region virtual
        /// <summary> InitModuleRuns() : ModuleBase.m_aModuleRun 에 ModuleRun을 등록한다 </summary>
        protected virtual void InitModuleRuns() { }

        public virtual ObservableCollection<string> GetModuleruns()
        {
            ObservableCollection<string> result = new ObservableCollection<string>();
            for (int i = 0; i < m_asModuleRun.Count; i++)
            {
                result.Add(m_asModuleRun[i]);
            }
            return result;
        }
        
        public virtual void GetTools(bool bInit) { }
        public virtual void InitMemorys() { }

        public virtual void Reset()
        {
            EQ.p_bSimulate = false;
            if (p_eState == eState.Error) p_eState = eState.Ready;
        }

        public virtual void StartHome()
        {
            p_eState = eState.Home;
        }

        public virtual string StateHome()
        {
            return StateHome(m_listAxis);
        }

        protected virtual void StopHome()
        {
            EQ.p_bStop = true;
            foreach (Axis axis in m_listAxis) if (axis != null) axis.ServoOn(false);
            p_eState = eState.Error;
        }

        public virtual string StateReady()
        {
            return "OK";
        }
        #endregion

        #region UI Binding
        string _sRun = "";
        public string p_sRun
        {
            get { return _sRun; }
            set
            {
                _sRun = value;
                OnPropertyChanged(); 
            }
        }

        public void ButtonRun()
        {
            switch (p_eState)
            {
                case eState.Init: p_eState = eState.Home; break;
                case eState.Home: EQ.p_bStop = true; break;
                case eState.Ready: StartRun(p_sModuleRun); break;
                case eState.Run: EQ.p_bStop = true; break;
                case eState.Error: Reset(); break;
                default: break;
            }
            RunTreeRun(Tree.eMode.Init);
        }

        public virtual void ButtonHome()
        {
            p_eState = eState.Home;
        }

        public bool p_bEnableRun
        {
            get { return (EQ.p_eState == EQ.eState.Ready); }
        }

        bool _bEnableHome = true; 
        public bool p_bEnableHome
        {
            get { return _bEnableHome; }
            set
            {
                if (_bEnableHome == value) return;
                _bEnableHome = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region RemoteThread
        Thread m_threadRemote;
        void ThreadRemote()
        {
            m_bThread = true;
            Thread.Sleep(5000);
            while (m_bThread)
            {
                Thread.Sleep(10);
                StateRemote(); 
            }
        }

        /// <summary> m_qModuleRemote : RunThread에서 실행 대기 중인 Remote ModuleRunBase (EQ.IsStop() 인 경우만 정지) </summary>
        public Queue<ModuleRunBase> m_qModuleRemote = new Queue<ModuleRunBase>();
        bool StateRemote()
        {
            if (m_qModuleRemote.Count == 0) return false;
            ModuleRunBase moduleRun = m_qModuleRemote.Peek();
            if (moduleRun.m_eRemote == eRemote.Local && p_eRemote == eRemote.Local) return false;
            try
            {
                m_swRun.Restart();
                m_log.Info("RemoteClient : " + moduleRun.p_id + " Start");
                moduleRun.p_eRunState = ModuleRunBase.eRunState.Run;
                p_sInfo = m_remote.RemoteSend(moduleRun);
            }
            catch (Exception e) 
            { 
                p_sInfo = "RemoteClient Exception = " + e.Message; 
            }
            moduleRun.p_eRunState = ModuleRunBase.eRunState.Done;
            m_log.Info("RemoteClient : " + moduleRun.p_id + " Done : " + (m_swRun.ElapsedMilliseconds / 1000.0).ToString("0.00 sec"));
            if (m_qModuleRemote.Count > 0) m_qModuleRemote.Dequeue();
            return true;
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread;
        void ThreadRun()
        {
            m_bThread = true;
            Thread.Sleep(5000);
            while (m_bThread)
            {
                Thread.Sleep(10);
                RunThread();
            }
            RunThreadStop(); 
        }

        protected virtual void RunThreadStop() { }

        protected virtual void RunThread()
        {
            if (StateServer()) return; 
            switch (p_eState)
            {
                case eState.Init:
                    p_bEnableHome = true;
                    p_sRun = "Initialize";
                    break;
                case eState.Home:
                    p_bEnableHome = false;
                    p_sRun = "Stop";
                    string sStateHome = StateHome();
                    if (sStateHome == "OK")
                        p_eState = eState.Ready;
                    else
                        StopHome();
                    break;
                case eState.Ready:
                    p_bEnableHome = true;
                    p_sRun = p_sModuleRun;
                    if (p_eRemote != eRemote.Client)
                    {
                        string sStateReady = StateReady();
                        if (sStateReady != "OK")
                        {
                            p_eState = eState.Error;
                            m_qModuleRun.Clear();
                        }
                    }
                    if (m_qModuleRun.Count > 0) p_eState = eState.Run;
                    break;
                case eState.Run:
                    p_bEnableHome = false;
                    p_sRun = "Stop";
                    string sStateRun = StateRun();
                    if (sStateRun != "OK")
                    {
                        p_eState = eState.Error;
                        EQ.p_bStop = true;
                        m_qModuleRun.Clear();
                        return;
                    }
                    if (m_qModuleRun.Count == 0) p_eState = eState.Ready;
                    break;
                case eState.Error:
                    p_bEnableHome = false;
                    p_sRun = "Reset";
                    break;
            }
        }
        #endregion

        #region List Tool
        public ListDIO m_listDI = new ListDIO();
        public ListDIO m_listDO = new ListDIO();
        void InitDIO()
        {
            m_listDI.Init(ListDIO.eDIO.Input);
            m_listDO.Init(ListDIO.eDIO.Output);
        }

        public List<Axis> m_listAxis = new List<Axis>();
        public List<ITool> m_aTool = new List<ITool>();

        public CameraSet m_cameraSet = null;
        public LensSet m_lensSet = null; 
        #endregion

        #region Home
        public string StateHome(params Axis[] aAxis)
        {
            List<Axis> listAxis = new List<Axis>();
            foreach (Axis axis in aAxis) listAxis.Add(axis);
            return StateHome(listAxis);
        }

        public string StateHome(List<Axis> aAxis)
        {
            if (aAxis.Count == 0) return "OK";
            if (p_eState == eState.Run) return "Invalid State : Run";
            if (EQ.IsStop()) return "Home Stop";

            foreach (Axis axis in aAxis)
            {
                if (axis != null) axis.ServoOn(true); 
            }
            Thread.Sleep(200);
            if (EQ.IsStop()) return "Home Stop";
            foreach (Axis axis in aAxis)
            {
                if (axis != null) p_sInfo = axis.StartHome();
            }

            while (true)
            {
                Thread.Sleep(10);
                if (EQ.IsStop(1000)) return "Home Stop";
                bool bDone = true;
                foreach (Axis axis in aAxis)
                {
                    if ((axis != null) && (axis.p_eState == Axis.eState.Home)) bDone = false;
                }
                if (bDone) return "OK";
            }
        }
        #endregion

        #region ModuleRunList
        string _sModuleRun = "";
        public string p_sModuleRun
        {
            get { return _sModuleRun; }
            set
            {
                _sModuleRun = value;
                OnPropertyChanged();
            }
        }

        /// <summary> m_asModuleRun : ModuleBase에 등록된 ModuleRun들의 이름 List </summary>
        public List<string> m_asModuleRun = new List<string>();
        /// <summary> m_asRecipe : ModuleBase에 등록된 Recipe에서 사용 가능한 ModuleRun들의 이름 List </summary>
        public List<string> m_asRecipe = new List<string>();
        /// <summary> m_aModuleRun : ModuleBase가 등록된 ModuleRun들의 List (for Clone) </summary>
        protected List<ModuleRunBase> m_aModuleRun = new List<ModuleRunBase>();
        public ModuleRunBase AddModuleRunList(ModuleRunBase moduleRun, bool bRecipe, string sDesc)
        {
            m_aModuleRun.Add(moduleRun);
            m_asModuleRun.Add(moduleRun.m_sModuleRun);
            if (bRecipe) m_asRecipe.Add(moduleRun.m_sModuleRun);
            if (m_gaf != null)
            {
                ALID alid = m_gaf.GetALID(this, moduleRun.m_sModuleRun, sDesc);
                moduleRun.m_alid = alid;
            }
            return moduleRun; 
        }

        public ModuleRunBase CloneModuleRun(string sModuleRun)
        {
            foreach (ModuleRunBase moduleRun in m_aModuleRun)
            {
                if (sModuleRun == moduleRun.m_sModuleRun) return moduleRun.Clone();
            }
            return null;
        }
        #endregion

        #region Run
        /// <summary> m_qModuleRun : RunThread에서 실행 대기 중인 ModuleRunBase (EQ.IsStop() 인 경우만 정지) </summary>
        public Queue<ModuleRunBase> m_qModuleRun = new Queue<ModuleRunBase>();

        protected string StartRun(string sRun)
        {
            ModuleRunBase moduleRun = CloneModuleRun(sRun);
            if (moduleRun == null)
            {
                p_sInfo = "Can't Find Module : " + p_id + ", " + sRun;
                return p_sInfo;
            }
            else return StartRun(moduleRun);
        }

        public string StartRun(ModuleRunBase moduleRun)
        {
            if (EQ.IsStop()) return "EQ Stop";
            if (moduleRun == null) return "OK";
            if ((moduleRun.m_eRemote != eRemote.Local) && (moduleRun.m_eRemote == p_eRemote) || p_eRemote == eRemote.Client)
                m_qModuleRemote.Enqueue(moduleRun);
            else
                m_qModuleRun.Enqueue(moduleRun);
            
            p_sInfo = "StartRun : " + moduleRun.m_sModuleRun;
            return "OK";
        }

        bool StateServer()
        {
            if (m_qModuleRun.Count == 0) return false;
            ModuleRunBase moduleRun = m_qModuleRun.Peek();
            if (moduleRun.m_eRemote == eRemote.Local) return false;
            try
            {
                m_swRun.Restart();
                m_log.Info("RemoteServer : " + moduleRun.p_id + " Start");
                moduleRun.p_eRunState = ModuleRunBase.eRunState.Run;
                p_sInfo = moduleRun.Run();
            }
            catch (Exception e) { p_sInfo = "RemoteServer Exception = " + e.Message; }
            moduleRun.p_eRunState = ModuleRunBase.eRunState.Done;
            m_log.Info("RemoteServer : " + moduleRun.p_id + " Done : " + (m_swRun.ElapsedMilliseconds / 1000.0).ToString("0.00 sec"));
            if (m_qModuleRun.Count > 0) m_qModuleRun.Dequeue();
            return true;
        }

        public virtual bool IsExistCarrier()
        {
            return false;
        }

        public virtual bool IsPlacement()
        {
            return false;
        }

        public virtual bool IsPresent()
        {
            return false;
        }

        public virtual bool IsDocked()
        {
            return false;
        }

        StopWatch m_swRun = new StopWatch(); 
        protected string StateRun()
        {
            if (m_qModuleRun.Count == 0) return "OK";
            ModuleRunBase moduleRun = m_qModuleRun.Peek();
            moduleRun.p_eRunState = ModuleRunBase.eRunState.Run;
            m_swRun.Restart();
            m_log.Info("ModuleRun : " + moduleRun.p_id + " Start");
			try { p_sInfo = moduleRun.Run(); }
            catch (Exception e) { p_sInfo = "StateRun Exception = " + e.Message; }
            moduleRun.p_eRunState = ModuleRunBase.eRunState.Done;
            m_log.Info("ModuleRun : " + moduleRun.p_id + " Done : " + (m_swRun.ElapsedMilliseconds / 1000.0).ToString("0.00 sec"));
            if (m_qModuleRun.Count > 0) m_qModuleRun.Dequeue();
            if (p_sInfo != "OK")
            {
                EQ.p_bStop = true;
                p_eState = eState.Error;
                moduleRun.SetALID(p_sInfo);
            }
            return p_sInfo;
        }
        #endregion

        #region RemoteRun
        public enum eRemote
        {
            Local,
            Client,
            Server
        }
        eRemote _eRemote = eRemote.Local; 
        public eRemote p_eRemote
        {
            get { return _eRemote; }
            set
            {
                if (_eRemote == value) return;
                _eRemote = value;
                OnPropertyChanged(); 
                m_remote = new Remote(this); 
            }
        }

        public class Remote : NotifyProperty
        {
            MemoryStream m_memoryStream = new MemoryStream();

            #region eCmd
            public class Protocol
            {
                public eRemote m_eRemote = eRemote.Local; 
                public string m_sCmd = "";
                
                string _sRun = ""; 
                public string p_sRun
                {
                    get { return _sRun; }
                    set
                    {
                        _sRun = value;
                        _sSend = m_eRemote.ToString() + ',' + m_sCmd + ',' + _sRun;
                    }
                }

                string _sSend = ""; 
                public string p_sSend
                {
                    get { return _sSend; }
                    set
                    {
                        _sSend = value;
                        string[] asSend = _sSend.Split(',');
                        try
                        {
                            m_eRemote = GetRemote(asSend[0]);
                            m_sCmd = asSend[1];
                            int l = asSend[0].Length + asSend[1].Length + 2;
                            _sRun = _sSend.Substring(l, _sSend.Length - l);
                        }
                        catch (Exception) { }
                    }
                }

                public bool IsSame(Protocol protocol)
                {
                    if (m_eRemote != protocol.m_eRemote) return false;
                    if (m_sCmd != protocol.m_sCmd) return false;
                    return true; 
                }

                public bool m_bDone = false;
                public string WaitDone()
                {
                    while (m_bDone == false)
                    {
                        Thread.Sleep(10);
                        if (EQ.IsStop()) return "EQ Stop";
                    }
                    return p_sRun; 
                }

                public Protocol(eRemote eRemote, string sCmd, string sRun)
                {
                    m_eRemote = eRemote; 
                    m_sCmd = sCmd;
                    p_sRun = sRun;
                }

                public Protocol(string sSend) 
                {
                    p_sSend = sSend;
                }

                eRemote GetRemote(string sRemote)
                {
                    foreach (eRemote remote in Enum.GetValues(typeof(eRemote)))
                    {
                        if (remote.ToString() == sRemote) return remote;
                    }
                    return eRemote.Local;
                }
            }
            #endregion

            #region List Send
            public List<Protocol> m_aProtocol = new List<Protocol>(); 
            void Send(Protocol protocol)
            {
                m_aProtocol.Add(protocol); 
                switch (m_module.p_eRemote)
                {
                    case eRemote.Client: m_client?.Send(protocol.p_sSend); break;
                    case eRemote.Server: m_server?.Send(protocol.p_sSend); break; 
                }
            }

            void Recieve(Protocol protocol)
            {
                int nCount = m_aProtocol.Count;
                for (int n = nCount-1; n >= 0; n--)
                {
                    if (m_aProtocol[n].IsSame(protocol))
                    {            
                        m_aProtocol[n].p_sRun = protocol.p_sRun; 
                        m_aProtocol[n].m_bDone = true;
                        m_aProtocol.RemoveAt(n);
                        // return; 
                      //  break;
                    }
                }
              
            }
            #endregion

            #region Client
            bool _bEnable = true;
            public bool p_bEnable
            {
                get { return _bEnable; }
                set
                {
                    if (_bEnable == value) return;
                    _bEnable = value;
                    OnPropertyChanged();
                }
            }

            public TCPIPClient m_client;
            void InitClient(bool bInit)
            {
                m_module.p_sInfo = m_module.m_toolBox.GetComm(ref m_client, m_module, "TCPIP");
                if (bInit) m_client.EventReciveData += M_client_EventReciveData;
            }

            public string RemoteSend(ModuleRunBase run)
            {
                if (p_bEnable == false) return "OK";
                m_memoryStream = new MemoryStream();
                m_treeRoot.m_job = new Job(m_memoryStream, true, m_log);
                m_treeRoot.p_eMode = Tree.eMode.JobSave;
                run.RunTree(m_treeRoot, true);
                m_treeRoot.m_job.Close();
                string sRun = m_treeRoot.m_job.m_sMemory;
                Protocol protocol = new Protocol(m_module.p_eRemote, run.m_sModuleRun, sRun); 
                Send(protocol);
                m_memoryStream.Close();
                m_module.p_sInfo = protocol.WaitDone();
                return m_module.p_sInfo; 
            }

            public string RemoteSend(Protocol protocol)
            {
                Send(protocol);
                m_module.p_sInfo = protocol.WaitDone();
                return m_module.p_sInfo;
            }

            private void M_client_EventReciveData(byte[] aBuf, int nSize, Socket socket)
            {
                string sSend = Encoding.Default.GetString(aBuf, 0, nSize);
                if (sSend.Length <= 0) return;
                Protocol protocol = new Protocol(sSend);
                if (protocol.m_eRemote == m_module.p_eRemote) Recieve(protocol);
                else ServerModuleRun(protocol);
            }
            #endregion

            #region Server
            public TCPIPServer m_server;
            void InitServer(bool bInit)
            {
                m_module.p_sInfo = m_module.m_toolBox.GetComm(ref m_server, m_module, "TCPIP");
                if (bInit) m_server.EventReciveData += M_server_EventReciveData;
            }

            private void M_server_EventReciveData(byte[] aBuf, int nSize, Socket socket)
            {
                string sSend = Encoding.Default.GetString(aBuf, 0, nSize);
                if (sSend.Length <= 0) return;
                Protocol protocol = new Protocol(sSend);
                if (protocol.m_eRemote == m_module.p_eRemote) Recieve(protocol);
                else ServerModuleRun(protocol);
            }

            void ServerModuleRun(Protocol protocol)
            {
                EQ.p_bStop = false; 
                ModuleRunBase run = m_module.CloneModuleRun(protocol.m_sCmd);
                if (run == null)
                {
                    protocol.p_sRun = "Unknown Cmd";
                    //Send(protocol);
                    return; 
                }
                m_memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(protocol.p_sRun));
                m_treeRoot.m_job = new Job(m_memoryStream, false, m_log);
                m_treeRoot.p_eMode = Tree.eMode.JobOpen;
                run.RunTree(m_treeRoot, true);
                m_treeRoot.m_job.Close();
                m_module.StartRun(run);
                while (m_module.IsBusy()) Thread.Sleep(10);
                protocol.p_sRun = m_module.p_sInfo; 
                Send(protocol);
                m_memoryStream.Close();
            }
            #endregion

            #region ToolBox
            public void GetTools(bool bInit)
            {
                switch (m_module.p_eRemote)
                {
                    case eRemote.Client: InitClient(bInit); break;
                    case eRemote.Server: InitServer(bInit); break;
                }
            }
            #endregion

            ModuleBase m_module;
            Log m_log; 
            TreeRoot m_treeRoot; 
            public Remote(ModuleBase module)
            {
                m_module = module;
                m_log = module.m_log; 
                m_treeRoot = new TreeRoot(module.p_id, module.m_log); 
            }
        }
        public Remote m_remote;
        #endregion

        #region Tree Tool
        public TreeRoot m_treeRootTool;
        void InitTreeTool()
        {
            m_treeRootTool = new TreeRoot(p_id + ".ToolBox", m_log);
            m_treeRootTool.UpdateTree += M_treeRootTool_UpdateTree;
            RunTreeTool(Tree.eMode.RegRead);
        }

        private void M_treeRootTool_UpdateTree()
        {
            RunTreeTool(Tree.eMode.Update);
            RunTreeTool(Tree.eMode.Init);
            RunTreeTool(Tree.eMode.RegWrite);
        }

        public delegate void dgOnChangeTool();
        public event dgOnChangeTool OnChangeTool;
        public void RunTreeTool(Tree.eMode mode)
        {
            m_treeRootTool.p_eMode = mode;
            m_listDI.m_aDIO.Clear();
            m_listDO.m_aDIO.Clear();
            m_listAxis.Clear();
            GetTools(mode == Tree.eMode.RegRead);
            for (int i = 0; i < m_listAxis.Count; i++)
            {
                m_listAxis[i].RunTreeInterlock(Tree.eMode.RegRead);
            }
            if (OnChangeTool != null) OnChangeTool();
        }
        #endregion

        #region Tree Setup
        public TreeRoot m_treeRootSetup;
        void InitTreeSetup()
        {
            m_treeRootSetup = new TreeRoot(p_id, m_log);
            m_treeRootSetup.UpdateTree += M_treeRootSetup_UpdateTree;
        }

        private void M_treeRootSetup_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
            RunTree(Tree.eMode.Init);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRootSetup.p_eMode = mode;
            RunTree(m_treeRootSetup);
        }

        public virtual void RunTree(Tree tree)
        {
        }
        #endregion

        #region Tree Run
        public TreeRoot m_treeRootRun;
        void InitTreeRun()
        {
            m_treeRootRun = new TreeRoot(p_id + ".Run", m_log);
            m_treeRootRun.UpdateTree += M_treeRootRun_UpdateTree;
            RunTreeRun(Tree.eMode.RegRead);
        }

        private void M_treeRootRun_UpdateTree()
        {
            RunTreeRun(Tree.eMode.Update);
            RunTreeRun(Tree.eMode.RegWrite);
            RunTreeRun(Tree.eMode.Init);
        }

        public void RunTreeRun(Tree.eMode mode)
        {
            m_treeRootRun.p_eMode = mode;
            p_sModuleRun = m_treeRootRun.GetTree("RunMode").Set(p_sModuleRun, "", m_asModuleRun, "ModuleRun", "Select ModuleRun");
            foreach (ModuleRunBase moduleRun in m_aModuleRun)
            {
                bool bVisible = p_sModuleRun == moduleRun.m_sModuleRun;
                moduleRun.RunTree(m_treeRootRun.GetTree(moduleRun.m_sModuleRun, true, bVisible), bVisible);
            }
        }
        #endregion

        #region Tree Queue
        public TreeRoot m_treeRootQueue;
        void InitTreeQueue()
        {
            m_treeRootQueue = new TreeRoot(p_id, m_log);
            m_treeRootQueue.UpdateTree += M_treeRootQueue_UpdateTree;
        }

        private void M_treeRootQueue_UpdateTree()
        {
            RunTreeQueue(Tree.eMode.Update);
            RunTreeQueue(Tree.eMode.RegWrite);
            RunTreeQueue(Tree.eMode.Init);
        }

        public void RunTreeQueue(Tree.eMode mode)
        {
            m_treeRootQueue.p_eMode = mode;
            RunTreeQueue(m_treeRootQueue);
        }

        void RunTreeQueue(Tree tree)
        {
            try
            {
                char c = (char)0;
                ModuleRunBase[] aModuleRemote = m_qModuleRemote.ToArray();
                foreach (ModuleRunBase run in aModuleRemote) run.RunTree(tree.GetTree(c++, run.m_sModuleRun), true);
                ModuleRunBase[] aModuleRun = m_qModuleRun.ToArray();
                foreach (ModuleRunBase run in aModuleRun) run.RunTree(tree.GetTree(c++, run.m_sModuleRun), true);
            }
            catch (Exception e)
            {
                int n = 0;
            }
        }
        #endregion
        
        string _id = "";
        public string p_id
        {
            get
            { return _id; }
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        public IEngineer m_engineer;
        public Log m_log;
        public IGem m_gem;
        public GAF m_gaf;
        public ToolBox m_toolBox;
        public void InitBase(string id, IEngineer enginner, eRemote eRemote = eRemote.Local)
        {
            p_id = id;
            m_engineer = enginner;
            p_eRemote = eRemote; 
            m_log = LogView.GetLog(id, id);
            m_gem = enginner.ClassGem();
            m_gaf = enginner.ClassGAF();
            m_toolBox = enginner.ClassToolBox();
            InitDIO();

            InitModuleRuns();
            InitTreeTool(); 
            InitMemorys();
            InitTreeSetup(); 
            InitTreeRun();
            InitTreeQueue();
            p_eState = eState.Init;

            RunTree(Tree.eMode.RegRead);

            m_thread = new Thread(new ThreadStart(ThreadRun));
            m_thread.Start();
            m_threadRemote = new Thread(new ThreadStart(ThreadRemote));
            m_threadRemote.Start();
        }

        public virtual void ThreadStop()
        {
            if (m_bThread)
            {
                m_qModuleRun.Clear();
                m_bThread = false;
                EQ.p_bStop = true;
                m_thread.Join();
                m_threadRemote.Join(); 
            }
        }
    }
}
