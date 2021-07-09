using Root_VEGA_D.Module;
using Root_EFEM.Module;
using RootTools;
using RootTools.Module;
using RootTools.OHTNew;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_VEGA_D.Engineer
{
    public class VEGA_D_Process : NotifyProperty
    {
        #region Locate
        /// <summary> Wafer Locate 관리용 </summary>
        public class Locate
        {
            public string m_id;
            public IWTRChild m_child = null;
            public WTRArm m_arm = null;

            /// <summary> Sequence 계산용 InfoWafer </summary>
            InfoWafer _calcWafer = null;
            public InfoWafer p_calcWafer
            {
                get { return _calcWafer; }
                set { _calcWafer = value; }
            }

            /// <summary> 실재 InfoWafer </summary>
            public InfoWafer p_infoWafer
            {
                get { return (m_child != null) ? m_child.GetInfoWafer(0) : m_arm.p_infoWafer; }
                set
                {
                    if (m_child != null) m_child.SetInfoWafer(0, value);
                    else m_arm.p_infoWafer = value;
                }
            }

            public void ClearInfoWafer()
            {
                if (p_infoWafer == null) return;
                if (IsWaferExist() == false) p_infoWafer = null;
            }

            bool IsWaferExist()
            {
                return (m_child != null) ? m_child.IsWaferExist(0) : m_arm.IsWaferExist();
            }

            public Locate(IWTRChild child)
            {
                m_id = child.p_id;
                m_child = child;
            }

            public Locate(WTRArm arm)
            {
                m_id = arm.m_id;
                m_arm = arm;
            }

            public void RunTree(Tree tree)
            {
                string sInfoWafer = (p_infoWafer == null) ? "Empty" : p_infoWafer.p_id;
                tree.GetTree("InfoWafer").Set(sInfoWafer, sInfoWafer, m_id, "InfoWafer ID", true, true);
            }
        }

        /// <summary> Wafer Locate List </summary>
        public List<Locate> m_aLocate = new List<Locate>();
        /// <summary> 프로그램 시작시 Registry 에서 Wafer 정보 읽기 </summary>
        void InitLocate()
        {
            if (m_wtr == null)
                return;
            m_aLocate.Clear();
            foreach (WTRArm arm in m_wtr.p_aArm) InitLocateArm(arm);
            foreach (IWTRChild child in m_wtr.p_aChild) InitLocateChild(child);
        }

        void InitLocateArm(WTRArm arm)
        {
            Locate locate = new Locate(arm);
            m_aLocate.Add(locate);
        }

        void InitLocateChild(IWTRChild child)
        {
            if (child.p_id.Contains("Loadport")) return;
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
        #endregion

        #region List InfoWafer
        /// <summary> 작업 할 InfoWafer List </summary>
        List<InfoWafer> m_aInfoWafer = new List<InfoWafer>();
        public string AddInfoWafer(InfoWafer infoWafer)
        {
            if (CheckExistInfoWafer(infoWafer)) return "Already Exist InfoWafer";
            if (infoWafer.m_moduleRunList.p_aModuleRun.Count == 0) return "Empty Recipe";
            CalcInfoWaferProcess(infoWafer);
            RunTree(Tree.eMode.Init);
            return "OK";
        }

        bool CheckExistInfoWafer(InfoWafer infoWafer)
        {
            foreach (InfoWafer wafer in m_aInfoWafer)
            {
                if (wafer.p_id == infoWafer.p_id) return true;
            }
            return false;
        }

        void CalcInfoWaferProcess(InfoWafer infoWafer)
        {
            if (infoWafer == null) return;
            Queue<ModuleRunBase> qProcess = infoWafer.m_qProcess;
            qProcess.Clear();
            qProcess.Enqueue(m_wtr.CloneRunGet(infoWafer.m_sModule, infoWafer.m_nSlot));
            for (int n = 0; n < infoWafer.m_moduleRunList.p_aModuleRun.Count; n++)
            {
                ModuleRunBase moduleRun = infoWafer.m_moduleRunList.p_aModuleRun[n];
                string sChild = moduleRun.m_moduleBase.p_id;
                bool bGetPut = (sChild != m_wtr.p_id);
                bool bPut = !IsSameModule(infoWafer.m_moduleRunList, n - 1, n);
                if (bPut && bGetPut) qProcess.Enqueue(m_wtr.CloneRunPut(sChild, -1));
                qProcess.Enqueue(moduleRun);
                bool bGet = !IsSameModule(infoWafer.m_moduleRunList, n, n + 1);
                if (bGet && bGetPut) qProcess.Enqueue(m_wtr.CloneRunGet(sChild, -1));
            }
            qProcess.Enqueue(m_wtr.CloneRunPut(infoWafer.m_sModule, infoWafer.m_nSlot));
            m_aInfoWafer.Add(infoWafer);
        }

        bool IsSameModule(ModuleRunList moduleRunList, int i0, int i1)
        {
            if (i0 < 0) return false;
            if (i1 >= moduleRunList.p_aModuleRun.Count) return false;
            return (moduleRunList.p_aModuleRun[i0].m_moduleBase.p_id == moduleRunList.p_aModuleRun[i1].m_moduleBase.p_id);
        }

        public void ClearInfoWafer()
        {
            m_aInfoWafer.Clear();
            foreach (Locate locate in m_aLocate) locate.ClearInfoWafer();
            ReCalcSequence();
            RunTree(Tree.eMode.Init);
        }
        #endregion

        #region Calc Sequence
        public class Sequence
        {
            public ModuleRunBase m_moduleRun;
            public InfoWafer m_infoWafer;
            public Sequence(ModuleRunBase moduleRun, InfoWafer infoWafer)
            {
                m_moduleRun = moduleRun;
                m_infoWafer = infoWafer;
            }
        }
        /// <summary> Simulation 계산용 InfoWafer List </summary>
        List<InfoWafer> m_aCalcWafer = new List<InfoWafer>();
        /// <summary> RunThread에서 실행 될 ModuleRun List (from Handler when EQ.p_eState == Run) </summary>
        public Queue<Sequence> m_qSequence = new Queue<Sequence>();
        public double m_dSequencePercent = 0;
        public double m_dOneSequencePercent = 0;
        public string ReCalcSequence()
        {
            try
            {
                InitCalc();
                int lProcess = 0;
                foreach (InfoWafer infoWafer in m_aCalcWafer) lProcess += infoWafer.m_qCalcProcess.Count;
                while (CalcSequence()) ;
                if (lProcess > m_qSequence.Count)
                {
                    InitCalc();
                    foreach (InfoWafer infoWafer in m_aCalcWafer)
                    {
                        foreach (ModuleRunBase run in infoWafer.m_qCalcProcess) m_qSequence.Enqueue(new Sequence(run, infoWafer));
                    }
                }
                RunTree(Tree.eMode.Init);
                return "OK";
            }
            catch (Exception e)
            {
                return "ReCalc Sequence Error : " + e.Message;
            }
        }

        public void InitCalc()
        {
            m_qSequence.Clear();
            m_aCalcWafer.Clear();
            foreach (Locate locate in m_aLocate) locate.p_calcWafer = locate.p_infoWafer;
            foreach (InfoWafer infoWafer in m_aInfoWafer)
            {
                    infoWafer.m_qCalcProcess.Clear();

                if (ManualJobSchedule_UI.bParallel)
                {
                    ModuleRunBase[] aProcess = infoWafer.m_qProcess.ToArray();
                    foreach (ModuleRunBase run in aProcess) infoWafer.m_qCalcProcess.Enqueue(run);
                }
                else
                {
                    infoWafer.InitCalcProcess();
                }
                m_aCalcWafer.Add(infoWafer);
            }
        }

        bool CalcSequence()
        {
            foreach (WTRArm arm in m_wtr.p_aArm)
            {
                if (GetLocate(arm.m_id).p_calcWafer != null)
                {
                    CalcSequence(arm);
                    return true;
                }
            }
            return GetNextInfoWafer();
        }

        void CalcSequence(WTRArm armPut)
        {
            Locate locateArmPut = GetLocate(armPut.m_id);
            InfoWafer infoWaferPut = locateArmPut.p_calcWafer;
            ModuleRunBase run = infoWaferPut.m_qCalcProcess.Dequeue();
            if ((run is IWTRRun) == false)
            {
                m_qSequence.Enqueue(new Sequence(run, infoWaferPut));
                return;
            }
            IWTRRun runPut = (IWTRRun)run; //forget WTR ModuleRun
            runPut.SetArm(armPut);
            Locate locateChild = GetLocate(runPut.p_sChild);
            InfoWafer infoWaferGet = (locateChild == null) ? null : locateChild.p_calcWafer;
            if ((infoWaferGet != null) && (locateChild != null))
            {

                ModuleRunBase runGet = infoWaferGet.m_qCalcProcess.Dequeue();
                string sArmGet = m_wtr.GetEnableAnotherArmID(runGet, armPut, infoWaferGet);
                Locate locateArmGet = GetLocate(sArmGet);
                if (locateArmGet != null)
                {
                    locateArmGet.p_calcWafer = locateChild.p_calcWafer;
                    locateChild.p_calcWafer = null;
                    m_qSequence.Enqueue(new Sequence(runGet, infoWaferGet));
                }
            }
            if (locateChild != null) locateChild.p_calcWafer = infoWaferPut;
            locateArmPut.p_calcWafer = null;
            m_qSequence.Enqueue(new Sequence((ModuleRunBase)runPut, infoWaferPut));
            m_aCalcWafer.Remove(infoWaferPut);
            m_aCalcWafer.Add(infoWaferPut);
            CalcSequenceChild(infoWaferPut);
        }

        void CalcSequenceChild(InfoWafer infoWaferPut)
        {
            if (infoWaferPut.m_qCalcProcess.Count == 0)
            {
                m_aCalcWafer.Remove(infoWaferPut);
                return;
            }
            ModuleRunBase moduleRun = infoWaferPut.m_qCalcProcess.Peek();
            if (moduleRun == null) return;
            if (moduleRun.m_moduleBase.p_id == m_wtr.p_id) return;
            m_qSequence.Enqueue(new Sequence(infoWaferPut.m_qCalcProcess.Dequeue(), infoWaferPut));
            CalcSequenceChild(infoWaferPut);
        }

        bool GetNextInfoWafer()
        {
            if (m_aCalcWafer.Count == 0) return false;
            for (int n = 0; n < m_aCalcWafer.Count; n++)
            {
                InfoWafer infoWaferGet = m_aCalcWafer[0];
                for (int iArm = 0; iArm < m_wtr.p_aArm.Count; iArm++)
                {
                    if (GetNextInfoWafer(iArm, infoWaferGet)) return true;
                }
                m_aCalcWafer.RemoveAt(0);
                m_aCalcWafer.Add(infoWaferGet);
            }
            return false;
        }
        bool GetNextInfoWafer(int iArm, InfoWafer infoWaferGet)
        {
            WTRArm armGet = m_wtr.p_aArm[iArm];
            if (armGet.IsEnableWaferSize(infoWaferGet) == false) return false;
            IWTRChild child = GetNextChild(infoWaferGet);
            if (child != null)
            {
                InfoWafer infoWaferChild = GetLocate(child.p_id).p_calcWafer;
                if (infoWaferChild != null)
                {
                    if (IsEnableWaferSizeAnotherArm(iArm, infoWaferGet) == false) return false;
                }
            }
            IWTRRun wtrRun = (IWTRRun)infoWaferGet.m_qCalcProcess.Dequeue();
            wtrRun.SetArm(armGet);
            GetLocate(armGet.m_id).p_calcWafer = infoWaferGet;
            Locate locateChild = GetLocate(wtrRun.p_sChild);
            //if (locateChild != null) locateChild.p_calcWafer = null; //forget
            m_qSequence.Enqueue(new Sequence((ModuleRunBase)wtrRun, infoWaferGet));
            return true;
        }

        bool IsEnableWaferSizeAnotherArm(int iArmGet, InfoWafer infoWaferPut)
        {
            for (int iArm = 0; iArm < m_wtr.p_aArm.Count; iArm++)
            {
                if (iArm != iArmGet)
                {
                    if (m_wtr.p_aArm[iArm].IsEnableWaferSize(infoWaferPut)) return true;
                }
            }
            return false;
        }

        IWTRChild GetNextChild(InfoWafer infoWaferGet)
        {
            for (int n = 1; n < infoWaferGet.m_qCalcProcess.Count; n++)//check 필요
            {
                ModuleRunBase moduleRun = infoWaferGet.m_qCalcProcess.Peek();
                if (moduleRun.m_moduleBase.p_id != m_wtr.p_id) return (IWTRChild)moduleRun.m_moduleBase;
            }
            return null;
        }
        #endregion

        #region Recover
        /// <summary> 예약된 Sequence를 다 지우고 Loadport로 다 넣을 수 있도록 Sequence 다시 만듬 </summary>
        public void CalcRecover()
        {
            m_aInfoWafer.Clear();
            m_qSequence.Clear();
            foreach (WTRArm arm in m_wtr.p_aArm) CalcRecoverArm(arm);
            foreach (IWTRChild child in m_wtr.p_aChild) CalcRecoverChild(child);
            ReCalcSequence();
            RunTree(Tree.eMode.Init);
            if (EQ.p_nRnR > 0) EQ.p_nRnR = 0;
        }

        void CalcRecoverArm(WTRArm arm)
        {
            Locate locate = GetLocate(arm.m_id);
            if (locate.p_infoWafer == null) return;
            InfoWafer infoWafer = locate.p_infoWafer;
            m_aInfoWafer.Add(infoWafer);
            infoWafer.m_qProcess.Clear();
            ModuleRunBase moduleRun = m_wtr.CloneRunPut(infoWafer.m_sModule, infoWafer.m_nSlot);
            infoWafer.m_qProcess.Enqueue(moduleRun);
        }

        void CalcRecoverChild(IWTRChild child)
        {
            Locate locate = GetLocate(child.p_id);
            if ((locate == null) || (locate.p_infoWafer == null)) return;
            InfoWafer infoWafer = locate.p_infoWafer;
            m_aInfoWafer.Add(infoWafer);
            infoWafer.m_qProcess.Clear();
            ModuleRunBase moduleRunGet = m_wtr.CloneRunGet(child.p_id, -1);
            infoWafer.m_qProcess.Enqueue(moduleRunGet);
            ModuleRunBase moduleRunPut = m_wtr.CloneRunPut(infoWafer.m_sModule, infoWafer.m_nSlot);
            infoWafer.m_qProcess.Enqueue(moduleRunPut);
        }
        #endregion

        #region RunSequence

        /// <summary> m_aSequence에 있는 ModuleRun을 가능한 동시 실행한다 </summary>
        public string RunNextSequence()
        {
            //ModuleBase wtr = (ModuleBase)m_wtr;
            if ((m_qSequence.Count == 0) || EQ.IsStop())
            {
                EQ.p_eState = EQ.eState.Ready;
                ClearInfoWafer();
                return EQ.IsStop() ? "EQ Stop" : "OK";
            }
            Sequence sequence = m_qSequence.Peek();
            if (sequence.m_moduleRun.p_id.Contains(".Docking") && !EQ.p_bRecovery)
            {
                if (handler.m_bIsRNR) handler.m_interlock.m_log.Info("<<<<<<<Lot Start>>>>>>>");
                ModuleRunBase VisionPM = handler.m_vision.m_runPM.Clone();
                handler.m_vision.StartRun(VisionPM);
                while (handler.m_vision.IsBusy() && (EQ.IsStop() == false)) Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
            }
            ModuleBase module = sequence.m_moduleRun.m_moduleBase;
            //bool bLoadport = sequence.m_moduleRun.m_moduleBase is ILoadport;
			//if ((sequence.m_moduleRun.m_moduleBase == wtr) || bLoadport)
			//{
			//	sequence.m_moduleRun.StartRun();
			//	while (wtr.IsBusy() && (EQ.IsStop() == false)) Thread.Sleep(10);
			//}
			sequence.m_moduleRun.StartRun();
            while (module.IsBusy() && (EQ.IsStop() == false)) Thread.Sleep(10);
            m_qSequence.Dequeue();
            m_dSequencePercent += m_dOneSequencePercent;
            InfoWafer infoWafer = sequence.m_infoWafer;
            if (infoWafer.m_qProcess.Count > 0) infoWafer.m_qProcess.Dequeue();
            if (m_qSequence.Count == 0)
            {
                m_dSequencePercent = 100;
                ClearInfoWafer();
            }
            RunTree(Tree.eMode.Init);
            return "OK";
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
        public TreeRoot m_treeWafer;
        public TreeRoot m_treeLocate;
        public TreeRoot m_treeSequence;
        void InitTree(string id)
        {
            m_treeWafer = new TreeRoot(id + "Wafer", m_log, true);
            m_treeLocate = new TreeRoot(id + "Locate", m_log);
            m_treeSequence = new TreeRoot(id + "Sequence", m_log, true);
            m_treeLocate.UpdateTree += M_treeLocate_UpdateTree;
        }

        private void M_treeLocate_UpdateTree()
        {
            RunTreeLocate(Tree.eMode.Update);
        }

        public void RunTree(Tree.eMode mode)
        {
            RunTreeWafer(mode);
            RunTreeLocate(mode);
            RunTreeSequence(mode);
        }

        void RunTreeWafer(Tree.eMode mode)
        {
            if ((mode == Tree.eMode.Init) && m_treeWafer.m_bFocus) return;
            TreeRoot tree = m_treeWafer;
            tree.p_eMode = mode;
            for (int n = 0; n < m_aInfoWafer.Count; n++)
            {
                InfoWafer infoWafer = m_aInfoWafer[n];
                infoWafer.RunTree(tree.GetTree(infoWafer.p_id, false));
            }
        }

        void RunTreeLocate(Tree.eMode mode)
        {
            if ((mode == Tree.eMode.Init) && m_treeLocate.m_bFocus) return;
            TreeRoot tree = m_treeLocate;
            tree.p_eMode = mode;
            foreach (Locate locate in m_aLocate) locate.RunTree(tree);
        }

        void RunTreeSequence(Tree.eMode mode)
        {
            if ((mode == Tree.eMode.Init) && m_treeSequence.m_bFocus) return;
            TreeRoot tree = m_treeSequence;
            tree.p_eMode = mode;
            Sequence[] aSequence = m_qSequence.ToArray();
            for (int n = 0; n < aSequence.Length; n++)
            {
                ModuleRunBase moduleRun = aSequence[n].m_moduleRun;
                ModuleBase module = moduleRun.m_moduleBase;
                InfoWafer infoWafer = aSequence[n].m_infoWafer;
                string sTree = "(" + infoWafer.p_id.Replace("Loadport", "") + ")." + moduleRun.p_id;
                switch (moduleRun.m_sModuleRun)
                {
                    case "Get": sTree += "." + ((IWTRRun)moduleRun).p_sChild; break;
                    case "Put": sTree += "." + ((IWTRRun)moduleRun).p_sChild; break;
                }
                moduleRun.RunTree(tree.GetTree(n, sTree, false), true);
            }
        }
        #endregion

        public string m_id;
        IEngineer m_engineer;
        public IHandler m_handler;
        VEGA_D_Handler handler;
        IWTR m_wtr;
        Log m_log;
        public VEGA_D_Process(string id, IEngineer engineer, IWTR wtr)
        {
            m_id = id;
            m_engineer = engineer;
            m_handler = engineer.ClassHandler();
            handler = (VEGA_D_Handler)m_engineer.ClassHandler();
            m_wtr = wtr;
            m_log = LogView.GetLog(id);
            InitTree(id);
            InitLocate();
        }
    }
}
