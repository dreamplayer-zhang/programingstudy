using Root_Vega.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_Vega
{
    /// <summary> InfoReticle에 있는 Recipe를 읽어 Sequence를 만든다 </summary>
    public class Vega_Process : NotifyProperty
    {
        #region List InfoReticle
        /// <summary> 작업 할 InfoReticle List </summary>
        List<InfoReticle> m_aInfoReticle = new List<InfoReticle>();
        public string AddInfoReticle(InfoReticle infoReticle)
        {
            if (m_aInfoReticle.Count > 0) return "Already Exist InfoReticle";
            if (infoReticle.m_moduleRunList.m_aModuleRun.Count == 0) return "Empty Recipe";
            CalcInfoReticleProcess(infoReticle);
            RunTree(Tree.eMode.Init); 
            return "OK";
        }

        void CalcInfoReticleProcess(InfoReticle infoReticle)
        {
            Queue<ModuleRunBase> qProcess = infoReticle.m_qProcess;
            qProcess.Clear();
            string sLoadport = infoReticle.m_sLoadport;
            qProcess.Enqueue(m_robot.GetRunMotion(Robot_RND.eMotion.Get, sLoadport));
            for (int n = 0; n < infoReticle.m_moduleRunList.m_aModuleRun.Count; n++)
            {
                ModuleRunBase moduleRun = infoReticle.m_moduleRunList.m_aModuleRun[n];
                string sChild = moduleRun.m_moduleBase.p_id;
                bool bGetPut = (sChild != m_robot.p_id);
                bool bPut = !IsSameModule(infoReticle.m_moduleRunList, n - 1, n); 
                if (bPut && bGetPut) qProcess.Enqueue(m_robot.GetRunMotion(Robot_RND.eMotion.Put, sChild));
                qProcess.Enqueue(moduleRun);
                bool bGet = !IsSameModule(infoReticle.m_moduleRunList, n, n + 1);
                if (bGet && bGetPut) qProcess.Enqueue(m_robot.GetRunMotion(Robot_RND.eMotion.Get, sChild));
            }
            qProcess.Enqueue(m_robot.GetRunMotion(Robot_RND.eMotion.Put, sLoadport));
            foreach (ModuleRunBase data in qProcess) data.m_infoObject = infoReticle;
            infoReticle.SetObservableProcess(); 
            m_aInfoReticle.Add(infoReticle);
        }

        bool IsSameModule(ModuleRunList moduleRunList, int i0, int i1)
        {
            if (i0 < 0) return false;
            if (i1 >= moduleRunList.m_aModuleRun.Count) return false;
            return (moduleRunList.m_aModuleRun[i0].m_moduleBase.p_id == moduleRunList.m_aModuleRun[i1].m_moduleBase.p_id); 
        }

        public void ClearInfoReticle()
        {
            m_aInfoReticle.Clear();
            foreach (Locate locate in m_aLocate) locate.ClearInfoReticle(); 
            ReCalcSequence();
            RunTree(Tree.eMode.Init);
        }
        #endregion

        #region Locate
        /// <summary> Reticle Locate 관리용 </summary>
        public class Locate
        {
            public string m_id;
            public IRobotChild m_child = null;
            public Robot_RND m_robot = null;

            /// <summary> 실재 InfoReticle </summary>
            public InfoReticle p_infoReticle
            {
                get { return (m_child != null) ? m_child.p_infoReticle : m_robot.p_infoReticle; }
                set
                {
                    if (m_child != null) m_child.p_infoReticle = value;
                    else m_robot.p_infoReticle = value; 
                }
            }

            public void ClearInfoReticle()
            {
                m_bIgnoreExistSensor = false; 
                if (p_infoReticle == null) return;
                if (IsReticleExist() == false) p_infoReticle = null; 
            }

            bool m_bIgnoreExistSensor = false;
            bool IsReticleExist()
            {
                if (m_child != null) return m_child.IsReticleExist(m_bIgnoreExistSensor);
                return m_robot.IsReticleExist(m_bIgnoreExistSensor);
            }

            public Locate(IRobotChild child)
            {
                m_id = child.p_id;
                m_child = child;
            }

            public Locate(Robot_RND robot)
            {
                m_id = robot.p_id;
                m_robot = robot; 
            }

            public void RunTree(Tree tree)
            {
                string sReticle = (p_infoReticle == null) ? "Empty" : p_infoReticle.p_id;
                tree.GetTree("InfoReticle").Set(sReticle, sReticle, m_id, "InfoReticle ID", true, true);
                m_bIgnoreExistSensor = tree.GetTree("Ignore Exist Sensor", false).Set(m_bIgnoreExistSensor, m_bIgnoreExistSensor, m_id, "Ignore Exist Check Sensor"); 
            }
        }

        /// <summary> Reticle Locate List </summary>
        public List<Locate> m_aLocate = new List<Locate>();
        /// <summary> 프로그램 시작시 Registry 에서 Reticle 정보 읽기 </summary>
        void InitLocate()
        {
            m_aLocate.Clear();
            InitLocateArm();
            foreach (IRobotChild child in m_robot.m_aChild) InitLocateChild(child);
            CalcRecover();
        }

        void InitLocateArm()
        {
            Locate locate = new Locate(m_robot);
            m_aLocate.Add(locate);
        }

        void InitLocateChild(IRobotChild child)
        {
            Locate locate = new Locate(child);
            m_aLocate.Add(locate);
        }

        public Locate GetLocate(string sLocate)
        {
            foreach (Locate locate in m_aLocate)
            {
                if (locate.m_id == sLocate) return locate;
            }
            return null;
        }

        Locate GetLocate(Robot_RND robot)
        {
            return GetLocate(robot.p_id);
        }
        #endregion

        #region Recover
        /// <summary> 예약된 Sequence를 다 지우고 Loadport로 다 넣을 수 있도록 Sequence 다시 만듬 </summary>
        public void CalcRecover()
        {
            m_aInfoReticle.Clear();
            m_qSequence.Clear();
            CalcRecoverArm();
            foreach (IRobotChild child in m_robot.m_aChild) CalcRecoverChild(child);
            ReCalcSequence();
            RunTree(Tree.eMode.Init);
        }

        void CalcRecoverArm()
        {
            Locate locate = GetLocate(m_robot);
            if (locate.p_infoReticle == null) return;
            InfoReticle infoReticle = locate.p_infoReticle;
            m_aInfoReticle.Add(infoReticle);
            infoReticle.m_qProcess.Clear();
            ModuleRunBase moduleRun = m_robot.GetRunMotion(Robot_RND.eMotion.Put, infoReticle.m_sLoadport);
            moduleRun.m_infoObject = infoReticle;
            infoReticle.m_qProcess.Enqueue(moduleRun);
        }

        void CalcRecoverChild(IRobotChild child)
        {
            if (child.p_id.Contains("Loadport")) return;
            Locate locate = GetLocate(child.p_id);
            if (locate.p_infoReticle == null) return;
            InfoReticle infoReticle = locate.p_infoReticle;
            m_aInfoReticle.Add(infoReticle);
            infoReticle.m_qProcess.Clear();
            ModuleRunBase moduleRunGet = m_robot.GetRunMotion(Robot_RND.eMotion.Get, child.p_id);
            moduleRunGet.m_infoObject = infoReticle;
            infoReticle.m_qProcess.Enqueue(moduleRunGet);
            ModuleRunBase moduleRunPut = m_robot.GetRunMotion(Robot_RND.eMotion.Put, infoReticle.m_sLoadport);
            moduleRunPut.m_infoObject = infoReticle;
            infoReticle.m_qProcess.Enqueue(moduleRunPut);
        }
        #endregion

        #region Sequence
        /// <summary> RunThread에서 실행 될 ModuleRun List (from Handler when EQ.p_eState == Run) </summary>
        public Queue<ModuleRunBase> m_qSequence = new Queue<ModuleRunBase>();
        public string ReCalcSequence()
        {
            m_qSequence.Clear();
            try
            {
                if (m_aInfoReticle.Count <= 0) return "OK"; 
                foreach (InfoReticle infoReticle in m_aInfoReticle)
                {
                    if (infoReticle.m_qProcess.Count > 0)
                    {
                        foreach (ModuleRunBase moduleRun in infoReticle.m_qProcess) m_qSequence.Enqueue(moduleRun);
                        infoReticle.SetObservableProcess(); 
                    }
                }
                FinishLoadport(m_aInfoReticle[0]);
                RunTree(Tree.eMode.Init);
                return "OK";
            }
            catch (Exception)
            {
                return "ReCalc Sequence Error";
            }
        }

        void FinishLoadport(InfoReticle infoReticlePut)
        {
            ModuleBase module = m_handler.m_moduleList.GetModule(infoReticlePut.m_sLoadport);
            ModuleRunBase moduleRun = ((Loadport)module).GetRunUndocking();
            moduleRun.m_infoObject = infoReticlePut;
            m_qSequence.Enqueue(moduleRun);
        }

        /// <summary> m_aSequence에 있는 ModuleRun을 가능한 동시 실행한다 </summary>
        public string RunNextSequence()
        {
            if (!EQ.p_bSimulate && (EQ.p_eState != EQ.eState.Ready)) return "EQ not Ready"; 
            if (m_qSequence.Count == 0) return "OK";
            ModuleRunBase moduleRun = m_qSequence.Peek();
            p_sInfo = moduleRun.Run();
            if (p_sInfo == "OK")
            {
                m_qSequence.Dequeue();
                InfoReticle infoReticle = moduleRun.m_infoObject;
                if (infoReticle.m_qProcess.Count > 0) infoReticle.m_qProcess.Dequeue(); 
                if (m_qSequence.Count == 0) ClearInfoReticle(); 
            }
            RunTree(Tree.eMode.Init);
            return p_sInfo;
        }
        #endregion

        #region UI Binding
        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Tree
        private void M_treeLocate_UpdateTree()
        {
            RunTreeLocate(Tree.eMode.Update); 
        }

        public void RunTree(Tree.eMode mode)
        {
            RunTreeReticle(mode);
            RunTreeLocate(mode);
            RunTreeSequence(mode);
        }

        void RunTreeReticle(Tree.eMode mode)
        {
            TreeRoot tree = m_treeReticle;
            tree.p_eMode = mode;
            foreach (InfoReticle infoReticle in m_aInfoReticle)
            {
                infoReticle.RunTree(tree.GetTree(infoReticle.p_id, false));
            }
        }

        void RunTreeLocate(Tree.eMode mode)
        {
            TreeRoot tree = m_treeLocate;
            tree.p_eMode = mode;
            foreach (Locate locate in m_aLocate) locate.RunTree(tree);
        }

        void RunTreeSequence(Tree.eMode mode)
        {
            TreeRoot tree = m_treeSequence;
            tree.p_eMode = mode;
            ModuleRunBase[] aModuleRun = m_qSequence.ToArray(); 
            for (int n = 0; n < aModuleRun.Length; n++)
            {
                ModuleRunBase moduleRun = aModuleRun[n];
                InfoReticle infoReticle = moduleRun.m_infoObject;
                string sTree = "(" + infoReticle.p_id + ")." + moduleRun.p_id;
                moduleRun.RunTree(tree.GetTree(n, sTree, false), true);

            }
        }
        #endregion

        public string m_id;
        IEngineer m_engineer;
        public Vega_Handler m_handler;
        Robot_RND m_robot;
        Log m_log;
        public TreeRoot m_treeReticle;
        public TreeRoot m_treeLocate;
        public TreeRoot m_treeSequence;
        public Vega_Process(string id, IEngineer engineer, Vega_Handler handler)
        {
            m_id = id;
            m_engineer = engineer;
            m_handler = handler;
            m_robot = handler.m_robot; 
            m_log = LogViewer.GetLog(id);

            m_treeReticle = new TreeRoot(id + "Reticle", m_log, true);
            m_treeLocate = new TreeRoot(id + "Locate", m_log);
            m_treeSequence = new TreeRoot(id + "Sequence", m_log, true);

            m_treeLocate.UpdateTree += M_treeLocate_UpdateTree;

            InitLocate();
        }
    }
}
