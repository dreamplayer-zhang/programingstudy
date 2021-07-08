using Root_VEGA_P.Module;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_VEGA_P.Engineer
{
    public class VEGA_P_Process : NotifyProperty
    {
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

        #region Recipe
        ModuleRunList m_runList = null; 
        public string RecipeOpen()
        {
            m_recipe.RecipeOpen(m_runList);
            InitModuleRunQueue(); 
            return "OK"; 
        }

        public string Clear()
        {
            m_runList.Clear();
            InitModuleRunQueue(); 
            return "OK";
        }
        #endregion

        #region Run
        Queue<ModuleRunBase> m_qModuleRun = new Queue<ModuleRunBase>(); 
        void InitModuleRunQueue()
        {
            m_qModuleRun.Clear();
            foreach (ModuleRunBase run in m_runList.p_aModuleRun) m_qModuleRun.Enqueue(run);
        }

        public int p_nQueue { get { return m_qModuleRun.Count; } }

        public string StartRunStep()
        {
            if (m_qModuleRun.Count == 0) return "Empty Queue"; 
            ModuleRunBase run = m_qModuleRun.Dequeue();
            if (run.m_moduleBase.p_eState != ModuleBase.eState.Ready) return "Module not Ready";
            run.StartRun(); 
            return "OK"; 
        }

        public string StartRunProcess()
        {
            if (m_qModuleRun.Count == 0) return "Empty Queue";
            EQ.p_eState = EQ.eState.Run; 
            return "OK";
        }

        bool _bRnR = false;
        public bool p_bRnR
        {
            get { return _bRnR; }
            set
            {
                if (_bRnR == value) return;
                _bRnR = value;
                OnPropertyChanged();
            }
        }
        public string StartRunRnR(bool bRnR)
        {
            p_bRnR = bRnR;
            if (bRnR) EQ.p_eState = EQ.eState.Run;
            return "OK"; 
        }

        public string RunProcess()
        {
            if (m_qModuleRun.Count == 0) 
                return "OK";
            ModuleRunBase run = m_qModuleRun.Peek();
            if (run is RTR.Run_GetPut)
            {
                RTR.Run_GetPut runRTR = run as RTR.Run_GetPut;
                if (runRTR.m_moduleBase.IsBusy()) return "OK"; 
                string sGet = CheckChild(runRTR.m_sChildGet);
                if (sGet != "OK") return sGet;
                string sPut = CheckChild(runRTR.m_sChildPut);
                if (sPut != "OK") return sPut;
            }
            run.StartRun();
            m_qModuleRun.Dequeue();
            Thread.Sleep(200);
            return "OK";
        }

        string CheckChild(string sChild)
        {
            ModuleBase moduleGet = m_handler.p_moduleList.GetModule(sChild);
            if (moduleGet == null) return "Child not Exist : " + sChild;
            switch (moduleGet.p_eState)
            {
                case ModuleBase.eState.Ready: return "OK";
                case ModuleBase.eState.Run: return "Run";
                default: EQ.p_eState = EQ.eState.Error; return "Error";
            }
        }
        #endregion

        #region Recover
        public bool IsEnableRecover()
        {
            foreach (IRTRChild child in m_rtr.p_aChild)
            {
                if (child.IsEnableRecovery()) return true;
            }
            return m_rtr.IsEnableRecovery();
        }

        public void CalcRecover()
        {
            m_qModuleRun.Clear();
            if (m_loadport.p_infoPod == null) CalcRecover(InfoPod.ePod.EOP_Door);
            InfoPod.ePod ePod = (m_loadport.p_infoPod == null) ? InfoPod.ePod.EOP_Door : m_loadport.p_infoPod.p_ePod; 
            switch (ePod)
            {
                case InfoPod.ePod.EOP_Door:
                    CalcRecover(InfoPod.ePod.EIP_Plate);
                    CalcRecover(InfoPod.ePod.EIP_Cover);
                    CalcRecover(InfoPod.ePod.EOP_Dome);
                    break;
                case InfoPod.ePod.EIP_Plate:
                    CalcRecover(InfoPod.ePod.EIP_Cover);
                    CalcRecover(InfoPod.ePod.EOP_Dome);
                    break;
                case InfoPod.ePod.EIP_Cover:
                    CalcRecover(InfoPod.ePod.EOP_Dome);
                    break;
            }
        }

        void CalcRecover(InfoPod.ePod ePod)
        {
            if (m_rtr.p_infoPod != null)
            {
                if (m_rtr.p_infoPod.p_ePod == ePod)
                {
                    if (m_rtr.p_infoPod.p_bTurn)
                    {
                        m_qModuleRun.Enqueue(m_rtr.GetModuleRunPut(m_holder.p_id));
                        m_qModuleRun.Enqueue(m_rtr.GetModuleRunGetPut(m_holder.p_id, m_loadport.p_id));
                        return; 
                    }
                    else
                    {
                        m_qModuleRun.Enqueue(m_rtr.GetModuleRunPut(m_loadport.p_id)); 
                        return; 
                    }
                }
            }
            else
            {
                IRTRChild child = FindChild(ePod);
                if (child == null) return; 
                if (child.p_infoPod.p_bTurn)
                {
                    m_qModuleRun.Enqueue(m_rtr.GetModuleRunGetPut(child.p_id, m_holder.p_id));
                    m_qModuleRun.Enqueue(m_rtr.GetModuleRunGetPut(m_holder.p_id, m_loadport.p_id));
                    return; 
                }
                else
                {
                    m_qModuleRun.Enqueue(m_rtr.GetModuleRunGetPut(child.p_id, m_loadport.p_id));
                    return;
                }
            }
        }

        IRTRChild FindChild(InfoPod.ePod ePod)
        {
            foreach (IRTRChild child in m_rtr.p_aChild)
            {
                if ((child.p_infoPod != null) && (child.p_infoPod.p_ePod == ePod)) return child;
            }
            return null;
        }
        #endregion

        #region OnTimer
        int m_nQueue = 0; 
        public void OnTimer()
        {
            if (m_nQueue != p_nQueue) RunTree(Tree.eMode.Init); 
            if ((EQ.p_eState == EQ.eState.Run) && m_handler.IsFinish())
            {
                if (p_bRnR) InitModuleRunQueue();
                else EQ.p_eState = EQ.eState.Ready; 
            }
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree(string id)
        {
            m_treeRoot = new TreeRoot(id, m_log, true);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
        }

        public void RunTree(Tree.eMode mode)
        {
            TreeRoot tree = m_treeRoot;
            tree.p_eMode = mode;
            ModuleRunBase[] aRun = m_qModuleRun.ToArray(); 
            for (int n = 0; n < aRun.Length; n++)
            {
                ModuleRunBase run = aRun[n];
                run.RunTree(tree.GetTree(n, run.p_id), true);
            }
        }
        #endregion

        public string p_id { get; set; }
        public VEGA_P_Handler m_handler;
        public VEGA_P_Recipe m_recipe;
        RTR m_rtr;
        Loadport m_loadport;
        Holder m_holder; 
        Log m_log;
        public VEGA_P_Process(string id, VEGA_P_Engineer engineer)
        {
            p_id = id;
            m_handler = (VEGA_P_Handler)engineer.ClassHandler();
            m_recipe = m_handler.m_recipe;
            m_rtr = m_handler.m_rtr;
            m_loadport = m_handler.m_loadport;
            m_holder = m_handler.m_holder; 
            m_log = LogView.GetLog(id);
            m_runList = new ModuleRunList("Recipe", engineer);
            InitTree(id);
        }
    }
}
