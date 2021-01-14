using Root_EFEM;
using Root_EFEM.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_WIND2
{
    public class WIND2_Process : NotifyProperty
    {
        #region List InfoWafer
        public string AddInfoWafer(InfoWafer infoWafer)
        {
            return "OK";
        }
        
        public void ClearInfoWafer()
        {

        }
        #endregion

        #region UI Binding
        string _sInfo = "OK";
        public string p_sInfo
        {
            get
            {
                return _sInfo;
            }
            set
            {
                if (_sInfo == value)
                    return;
                _sInfo = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Recover
        public void CalcRecover()
        {

        }
        #endregion

        #region Sequence
        /// <summary> RunThread에서 실행 될 ModuleRun List (from Handler when EQ.p_eState == Run) </summary>
        public Queue<ModuleRunBase> m_qSequence = new Queue<ModuleRunBase>();
        public string ReCalcSequence()
        {
            return "OK";
        }

        public string RunNextSequence()
        {
            return "OK";
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
            //            foreach (InfoWafer infoWafer in m_aInfoWafer)
            //            {
            //                infoWafer.RunTree(tree.GetTree(infoWafer.p_id, false));
            //            }
        }

        void RunTreeLocate(Tree.eMode mode)
        {
            TreeRoot tree = m_treeLocate;
            tree.p_eMode = mode;
            //            foreach (Locate locate in m_aLocate) locate.RunTree(tree);
        }

        void RunTreeSequence(Tree.eMode mode)
        {
            TreeRoot tree = m_treeSequence;
            tree.p_eMode = mode;
            //            ModuleRunBase[] aModuleRun = m_qSequence.ToArray();
            //            for (int n = 0; n < aModuleRun.Length; n++)
            //            {
            //                ModuleRunBase moduleRun = aModuleRun[n];
            //                InfoWafer infoWafer = moduleRun.m_infoObject;
            //                string sTree = "(" + infoWafer.p_id + ")." + moduleRun.p_id;
            //                moduleRun.RunTree(tree.GetTree(n, sTree, false), true);
            //            }
        }
        #endregion

        public string m_id;
        IEngineer m_engineer;
        public WIND2_Handler m_handler;
        //        Robot_RND m_robot;
        Log m_log;
        public TreeRoot m_treeWafer;
        public TreeRoot m_treeLocate;
        public TreeRoot m_treeSequence;
        public WIND2_Process(string id, IEngineer engineer, WIND2_Handler handler)
        {
            m_id = id;
            m_engineer = engineer;
            m_handler = handler;
            //            m_robot = handler.m_robot;
            m_log = LogView.GetLog(id);

            m_treeWafer = new TreeRoot(id + "Wafer", m_log, true);
            m_treeLocate = new TreeRoot(id + "Locate", m_log);
            m_treeSequence = new TreeRoot(id + "Sequence", m_log, true);

            m_treeLocate.UpdateTree += M_treeLocate_UpdateTree;

            //            InitLocate();
        }
    }
}
