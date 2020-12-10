using Root_EFEM.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_EFEM
{
    /// <summary> InfoWafer에 있는 Recipe를 읽어 Sequence를 만든다 </summary>
    public class EFEM_Process
    {
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
            Queue<ModuleRunBase> qProcess = infoWafer.m_qProcess;
            qProcess.Clear();
            string[] asInfoWafer = infoWafer.p_id.Split('.');
            string sLoadport = asInfoWafer[0];
            string sLoadportID = infoWafer.p_id;
            qProcess.Enqueue(m_wtr.CloneRunGet(sLoadport, sLoadportID));
            for (int n = 0; n < infoWafer.m_moduleRunList.m_aModuleRun.Count; n++)
            {
                ModuleRunBase moduleRun = infoWafer.m_moduleRunList.m_aModuleRun[n];
                string sChild = moduleRun.m_moduleBase.p_id;
                string sChildID = "01";
                bool bGetPut = (sChild != m_wtr.p_id);
                bool bPut = !IsSameModule(infoWafer.m_moduleRunList, n - 1, n);
                if (bPut && bGetPut) aProcess.Add(wtr.GetRunMotion(ref iWTRMotion, WTR_RND.eMotion.Put, sChild, sChildID));
                aProcess.Add(moduleRun);
                bool bGet = !IsSameModule(infoWafer.m_moduleRunList, n, n + 1);
                if (bGet && bGetPut) aProcess.Add(wtr.GetRunMotion(ref iWTRMotion, WTR_RND.eMotion.Get, sChild, sChildID));
            }
            aProcess.Add(wtr.GetRunMotion(ref iWTRMotion, WTR_RND.eMotion.Put, sLoadport, sLoadportID));
            m_aInfoWafer.Add(infoWafer);
        }

        bool IsSameModule(ModuleRunList moduleRunList, int i0, int i1)
        {
            if (i0 < 0) return false;
            if (i1 >= moduleRunList.m_aModuleRun.Count) return false;
            return (moduleRunList.m_aModuleRun[i0].m_moduleBase.p_id == moduleRunList.m_aModuleRun[i1].m_moduleBase.p_id);
        }

        public void ClearInfoWafer()
        {
            m_aInfoWafer.Clear();
            foreach (Locate locate in m_aLocate) locate.ClearInfoWafer();
            ReCalcSequence(null);
            RunTree(Tree.eMode.Init);
        }
        #endregion

        public TreeRoot m_treeWafer;
        public TreeRoot m_treeLocate;
        public TreeRoot m_treeSequence;


        //===============================================================

         /*       #region Locate
                /// <summary> Wafer Locate 관리용 </summary>
                public class Locate
                {
                    public string m_id;
                    public IWTRChild m_child = null;
                    public WTR_RND.Arm m_arm = null;

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
                        m_bIgnoreExistSensor = false;
                        if (p_infoWafer == null) return;
                        if (IsWaferExist() == false) p_infoWafer = null;
                    }

                    bool m_bIgnoreExistSensor = false;
                    bool IsWaferExist()
                    {
                        return (m_child != null) ? m_child.IsWaferExist(0, m_bIgnoreExistSensor) : m_arm.IsWaferExist(m_bIgnoreExistSensor);
                    }

                    public Locate(IWTRChild child)
                    {
                        m_id = child.p_id;
                        m_child = child;
                    }

                    public Locate(WTR_RND.eArm eArm, WTR_RND.Arm arm)
                    {
                        m_id = eArm.ToString();
                        m_arm = arm;
                    }

                    public void RunTree(Tree tree)
                    {
                        string sInfoWafer = (p_infoWafer == null) ? "Empty" : p_infoWafer.p_id;
                        tree.GetTree("InfoWafer").Set(sInfoWafer, sInfoWafer, m_id, "InfoWafer ID", true, true);
                        m_bIgnoreExistSensor = tree.GetTree("Ignore Exist Sensor", false).Set(m_bIgnoreExistSensor, m_bIgnoreExistSensor, m_id, "Ignore Exist Check Sensor");
                    }
                }

                /// <summary> Wafer Locate List </summary>
                public List<Locate> m_aLocate = new List<Locate>();
                /// <summary> 프로그램 시작시 Registry 에서 Wafer 정보 읽기 </summary>
                void InitLocate()
                {
                    m_aLocate.Clear();
                    InitLocateArm(WTR_RND.eArm.Lower);
                    InitLocateArm(WTR_RND.eArm.Upper);
                    foreach (IWTRChild child in m_handler.p_wtr.m_aChild) InitLocateChild(child);
                    CalcRecover();
                }

                void InitLocateArm(WTR_RND.eArm arm)
                {
                    Locate locate = new Locate(arm, m_handler.p_wtr.m_dicArm[arm]);
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

                Locate GetLocate(WTR_RND.eArm arm)
                {
                    return GetLocate(arm.ToString());
                }
                #endregion

                #region Recover
                /// <summary> 예약된 Sequence를 다 지우고 Loadport로 다 넣을 수 있도록 Sequence 다시 만듬 </summary>
                public void CalcRecover()
                {
                    m_aInfoWafer.Clear();
                    m_qSequence.Clear();
                    CalcRecoverArm(WTR_RND.eArm.Lower);
                    CalcRecoverArm(WTR_RND.eArm.Upper);
                    foreach (IWTRChild child in m_handler.p_wtr.m_aChild) CalcRecoverChild(child);
                    ReCalcSequence(null);
                    RunTree(Tree.eMode.Init);
                }

                void CalcRecoverArm(WTR_RND.eArm arm)
                {
                    Locate locate = GetLocate(arm);
                    if (locate.p_infoWafer == null) return;
                    InfoWafer infoWafer = locate.p_infoWafer;
                    m_aInfoWafer.Add(infoWafer);
                    infoWafer.m_aProcess.Clear();
                    int iWTRMotion = 0;
                    string[] asInfoWafer = infoWafer.p_id.Split('.');
                    string sLoadport = asInfoWafer[0];
                    string sLoadportID = asInfoWafer[1];
                    ModuleRunBase moduleRun = m_handler.p_wtr.GetRunMotion(ref iWTRMotion, WTR_RND.eMotion.Put, sLoadport, sLoadportID);
                    infoWafer.m_aProcess.Add(moduleRun);
                }

                void CalcRecoverChild(IWTRChild child)
                {
                    Locate locate = GetLocate(child.p_id);
                    if ((locate == null) || (locate.p_infoWafer == null)) return;
                    InfoWafer infoWafer = locate.p_infoWafer;
                    m_aInfoWafer.Add(infoWafer);
                    WTR_RND wtr = m_handler.p_wtr;
                    infoWafer.m_aProcess.Clear();
                    int iWTRMotion = 0;
                    string sChild = child.p_id;
                    string sChildID = "01";
                    ModuleRunBase moduleRunGet = wtr.GetRunMotion(ref iWTRMotion, WTR_RND.eMotion.Get, sChild, sChildID);
                    infoWafer.m_aProcess.Add(moduleRunGet);
                    string[] asInfoWafer = infoWafer.p_id.Split('.');
                    string sLoadport = asInfoWafer[0];
                    string sLoadportID = asInfoWafer[1];
                    ModuleRunBase moduleRunPut = wtr.GetRunMotion(ref iWTRMotion, WTR_RND.eMotion.Put, sLoadport, sLoadportID);
                    infoWafer.m_aProcess.Add(moduleRunPut);
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

                public string ReCalcSequence(Sequence sequence)
                {
                    InitCalc();
                    try
                    {
                        if (sequence != null) m_qSequence.Enqueue(sequence);
                        int lProcess = 0;
                        foreach (InfoWafer infoWafer in m_aCalcWafer) lProcess += infoWafer.m_qCalcProcess.Count;
                        while (CalcSequence()) ;
                        if (lProcess > m_qSequence.Count)
                        {
                            InitCalc();
                            foreach (InfoWafer infoWafer in m_aCalcWafer)
                            {
                                foreach (ModuleRunBase moduleRun in infoWafer.m_aProcess) m_qSequence.Enqueue(new Sequence(moduleRun, infoWafer));
                            }
                        }
                        RunTree(Tree.eMode.Init);
                        return "OK";
                    }
                    catch (Exception)
                    {
                        return "ReCalc Sequence Error";
                    }
                }

                void InitCalc()
                {
                    m_qSequence.Clear();
                    foreach (Locate locate in m_aLocate) locate.p_calcWafer = locate.p_infoWafer;
                    foreach (InfoWafer infoWafer in m_aInfoWafer)
                    {
                        m_aCalcWafer.Add(infoWafer);
                        infoWafer.InitCalcProcess();
                    }
                }

                bool CalcSequence()
                {
                    if (GetLocate(WTR_RND.eArm.Lower).p_calcWafer != null)
                    {
                        CalcSequence(WTR_RND.eArm.Lower);
                        return true;
                    }
                    if (GetLocate(WTR_RND.eArm.Upper).p_calcWafer != null)
                    {
                        CalcSequence(WTR_RND.eArm.Upper);
                        return true;
                    }
                    return GetNextInfoWafer();
                }

                void CalcSequence(WTR_RND.eArm armPut)
                {
                    Locate locateArmPut = GetLocate(armPut);
                    InfoWafer infoWaferPut = locateArmPut.p_calcWafer;
                    WTR_RND.Run_Put moduleRunPut = (WTR_RND.Run_Put)infoWaferPut.m_qCalcProcess.Dequeue();
                    moduleRunPut.m_eArm = armPut;
                    Locate locateChild = GetLocate(moduleRunPut.m_sChild);
                    InfoWafer infoWaferGet = (locateChild == null) ? null : locateChild.p_calcWafer;
                    if ((infoWaferGet != null) && (locateChild != null))
                    {
                        WTR_RND.eArm armGet = (WTR_RND.eArm)(1 - (int)armPut);
                        Locate locateArmGet = GetLocate(armGet);
                        WTR_RND.Run_Get moduleRunGet = (WTR_RND.Run_Get)infoWaferGet.m_qCalcProcess.Dequeue();
                        moduleRunGet.m_eArm = armGet;
                        GetLocate(armGet).p_calcWafer = locateChild.p_calcWafer;
                        locateChild.p_calcWafer = null;
                        m_qSequence.Enqueue(new Sequence(moduleRunGet, infoWaferGet));
                    }
                    if (locateChild != null) locateChild.p_calcWafer = infoWaferPut;
                    locateArmPut.p_calcWafer = null;
                    m_qSequence.Enqueue(new Sequence(moduleRunPut, infoWaferPut));
                    m_aCalcWafer.Remove(infoWaferPut);
                    m_aCalcWafer.Add(infoWaferPut);
                    CalcSequenceChild(infoWaferPut);
                    if (locateChild == null) CheckFinishLoadport(infoWaferPut);
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

                void CheckFinishLoadport(InfoWafer infoWaferPut)
                {
                    string[] sLoadport = infoWaferPut.p_id.Split('.');
                    foreach (InfoWafer infoWafer in m_aCalcWafer)
                    {
                        if ((infoWafer.m_qCalcProcess.Count > 0) && infoWafer.p_id.Contains(sLoadport[0])) return;
                    }
                    ModuleBase module = m_handler.m_moduleList.GetModule(sLoadport[0]);
                    ModuleRunBase moduleRun = ((Loadport_RND)module).GetRunUnload();
                    m_qSequence.Enqueue(new Sequence(moduleRun, infoWaferPut));
                }

                bool GetNextInfoWafer()
                {
                    if (m_aCalcWafer.Count == 0) return false;
                    for (int n = 0; n < m_aCalcWafer.Count; n++)
                    {
                        InfoWafer infoWaferGet = m_aCalcWafer[0];
                        if (GetNextInfoWafer(WTR_RND.eArm.Upper, infoWaferGet)) return true;
                        if (GetNextInfoWafer(WTR_RND.eArm.Lower, infoWaferGet)) return true;
                        m_aCalcWafer.RemoveAt(0);
                        m_aCalcWafer.Add(infoWaferGet);
                    }
                    return false;
                }

                bool GetNextInfoWafer(WTR_RND.eArm armGet, InfoWafer infoWaferGet)
                {
                    if (m_wtr.m_dicArm[armGet].IsEnableWaferSize(infoWaferGet) == false) return false;
                    IWTRChild child = GetNextChild(infoWaferGet);
                    if (child != null)
                    {
                        InfoWafer infoWaferChild = GetLocate(child.p_id).p_calcWafer;
                        if (infoWaferChild != null)
                        {
                            WTR_RND.eArm armChild = (WTR_RND.eArm)(1 - (int)armGet);
                            if (m_wtr.m_dicArm[armChild].IsEnableWaferSize(infoWaferChild) == false) return false;
                        }
                    }
                    WTR_RND.Run_Get moduleRunGet = (WTR_RND.Run_Get)infoWaferGet.m_qCalcProcess.Dequeue();
                    moduleRunGet.m_eArm = armGet;
                    GetLocate(moduleRunGet.m_eArm).p_calcWafer = infoWaferGet;
                    Locate locateChild = GetLocate(moduleRunGet.m_sChild);
                    if (locateChild != null) locateChild.p_calcWafer = null;
                    m_qSequence.Enqueue(new Sequence(moduleRunGet, infoWaferGet));
                    return true;
                }

                IWTRChild GetNextChild(InfoWafer infoWaferGet)
                {
                    for (int n = 1; n < infoWaferGet.m_qCalcProcess.Count; n++)
                    {
                        ModuleRunBase moduleRun = infoWaferGet.m_qCalcProcess.Peek();
                        if (moduleRun.m_moduleBase.p_id != m_wtr.p_id) return (IWTRChild)moduleRun.m_moduleBase;
                    }
                    return null;
                }
                #endregion

                #region RunSequence
                /// <summary> m_aSequence에 있는 ModuleRun을 가능한 동시 실행한다 </summary>
                public string RunNextSequence()
                {
                    if (m_wtr.p_eState != ModuleBase.eState.Ready) return "WTR not Ready";
                    if (m_qSequence.Count == 0)
                    {
                        EQ.p_eState = EQ.eState.Ready;
                        return "OK";
                    }
                    Sequence sequence = m_qSequence.Peek();
                    if (sequence.m_moduleRun.m_moduleBase == m_wtr) p_sInfo = sequence.m_moduleRun.Run();
                    else p_sInfo = sequence.m_moduleRun.StartRun();
                    if (p_sInfo == "OK")
                    {
                        m_qSequence.Dequeue();
                        InfoWafer infoWafer = sequence.m_infoWafer;
                        if (infoWafer.m_aProcess.Count > 0) infoWafer.m_aProcess.RemoveAt(0);
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
                    RunTreeWafer(mode);
                    RunTreeLocate(mode);
                    RunTreeSequence(mode);
                }

                void RunTreeWafer(Tree.eMode mode)
                {
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
                    TreeRoot tree = m_treeLocate;
                    tree.p_eMode = mode;
                    foreach (Locate locate in m_aLocate) locate.RunTree(tree);
                }

                void RunTreeSequence(Tree.eMode mode)
                {
                    TreeRoot tree = m_treeSequence;
                    tree.p_eMode = mode;
                    Sequence[] aSequence = m_qSequence.ToArray();
                    for (int n = 0; n < aSequence.Length; n++)
                    {
                        ModuleRunBase moduleRun = aSequence[n].m_moduleRun;
                        ModuleBase module = moduleRun.m_moduleBase;
                        InfoWafer infoWafer = aSequence[n].m_infoWafer;
                        string sTree = "(" + infoWafer.p_id + ")." + moduleRun.p_id;
                        switch (moduleRun.m_sModuleRun)
                        {
                            case "Get": sTree += "." + ((WTR_RND.Run_Get)moduleRun).m_sChild; break;
                            case "Put": sTree += "." + ((WTR_RND.Run_Put)moduleRun).m_sChild; break;
                        }
                        moduleRun.RunTree(tree.GetTree(n, sTree, false), true);
                    }
                }
                #endregion
*/
        public string m_id;
                IEngineer m_engineer;
                public IHandler m_handler;
                IWTR m_wtr;
                Log m_log;
                public EFEM_Process(string id, IEngineer engineer, IWTR wtr)
                {
                    m_id = id;
                    m_engineer = engineer;
                    m_handler = engineer.ClassHandler();
                    m_wtr = wtr;
                    m_log = LogView.GetLog(id);

                    m_treeWafer = new TreeRoot(id + "Wafer", m_log, true);
                    m_treeLocate = new TreeRoot(id + "Locate", m_log);
                    m_treeSequence = new TreeRoot(id + "Sequence", m_log, true);

//                    m_treeLocate.UpdateTree += M_treeLocate_UpdateTree;

//                    InitLocate();
                }
    }
}
