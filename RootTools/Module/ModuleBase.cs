using RootTools.Camera;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RootTools.Module
{
    public class ModuleBase : NotifyProperty
    {
        #region eState
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
                if (_eState.ToString() == value.ToString()) return;
                m_log.Info("State : " + _eState.ToString() + " -> " + value.ToString()); 
                _eState = value;
                EQ.p_bPause = false;
                OnPropertyChanged(); 
            }
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
                if (m_log != null) m_log.Info("Info : " + _sInfo);
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
        #endregion

        #region virtual
        /// <summary> InitModuleRuns() : ModuleBase.m_aModuleRun 에 ModuleRun을 등록한다 </summary>
        protected virtual void InitModuleRuns() { }

        public virtual void RunTree(Tree tree)
        {
            p_sModuleRun = tree.GetTree("RunMode").Set(p_sModuleRun, "", m_asModuleRun, "ModuleRun", "Select ModuleRun");
            foreach (ModuleRunBase moduleRun in m_aModuleRun)
            {
                bool bVisible = p_sModuleRun == moduleRun.m_sModuleRun; 
                moduleRun.RunTree(tree.GetTree(moduleRun.m_sModuleRun, true, bVisible), bVisible);
            }
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
            p_eState = eState.Init;
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
            RunTree(Tree.eMode.Init);
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
        }

        protected virtual void RunThread()
        {
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
                    if (sStateHome == "OK") p_eState = eState.Ready;
                    else StopHome();
                    break;
                case eState.Ready:
                    p_bEnableHome = true;
                    p_sRun = p_sModuleRun;
                    string sStateReady = StateReady();
                    if (sStateReady != "OK")
                    {
                        p_eState = eState.Error;
                        m_qModuleRun.Clear();
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

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
            RunTree(Tree.eMode.Init);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTree(m_treeRoot);
        }

        private void M_treeToolBox_UpdateTree()
        {
            RunToolTree(Tree.eMode.Update);
            RunToolTree(Tree.eMode.Init);
            RunToolTree(Tree.eMode.RegWrite);
        }

        public void RunToolTree(Tree.eMode mode)
        {
            m_treeToolBox.p_eMode = mode;
            m_listDI.m_aDIO.Clear();
            m_listDO.m_aDIO.Clear();
            m_listAxis.Clear();
            GetTools(mode == Tree.eMode.RegRead);

            for (int i = 0; i<m_listAxis.Count; i++)
            {
                m_listAxis[i].RunTreeInterlock(Tree.eMode.RegRead);
            }

            if (OnChangeTool != null) OnChangeTool();
        }

        public delegate void dgOnChangeTool();
        public event dgOnChangeTool OnChangeTool;
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
        #endregion

        #region Home
        public string StateHome(params Axis[] aAxis)
        {
            List<Axis> listAxis = new List<Axis>();
            foreach (Axis axis in aAxis) listAxis.Add(axis);
            return StateHome(listAxis);
        }

        string StateHome(List<Axis> aAxis)
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
        protected ModuleRunBase AddModuleRunList(ModuleRunBase moduleRun, bool bRecipe, string sDesc)
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
            m_qModuleRun.Enqueue(moduleRun);
            p_sInfo = "StartRun : " + moduleRun.m_sModuleRun;
            return "OK";
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
            m_qModuleRun.Dequeue();
            if (p_sInfo != "OK")
            {
                EQ.p_bStop = true;
                p_eState = eState.Error;
                moduleRun.SetALID(p_sInfo);
            }
            return p_sInfo;
        }
        #endregion

        public string p_id { get; set; }

        public IEngineer m_engineer;
        public Log m_log;
        public IGem m_gem;
        public GAF m_gaf;
        public ToolBox m_toolBox;
        public TreeRoot m_treeRoot;
        public TreeRoot m_treeToolBox;
        public void InitBase(string id, IEngineer enginner)
        {
            p_id = id;
            m_engineer = enginner;
            m_log = LogView.GetLog(id, id);
            m_gem = enginner.ClassGem();
            m_gaf = enginner.ClassGAF();
            m_toolBox = enginner.ClassToolBox();
            InitDIO();

            p_eState = eState.Init;
            InitModuleRuns();

            m_treeToolBox = new TreeRoot(id + ".ToolBox", m_log);
            m_treeToolBox.UpdateTree += M_treeToolBox_UpdateTree;
            RunToolTree(Tree.eMode.RegRead);

            InitMemorys(); 

            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

            m_thread = new Thread(new ThreadStart(ThreadRun));
            m_thread.Start();
        }

        public virtual void ThreadStop()
        {
            if (m_bThread)
            {
                m_qModuleRun.Clear();
                m_bThread = false;
                EQ.p_bStop = true;
                m_thread.Join();
            }
        }
    }
}
