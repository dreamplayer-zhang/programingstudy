using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace RootTools.Trees
{
    public class TreeRoot : Tree
    {
        #region UpdateTree
        public delegate void dgUpdateTree();
        public event dgUpdateTree UpdateTree;
        public void UpdateTreeItems()
        {
            if (UpdateTree != null) UpdateTree();
        }
        #endregion

        #region Property
        readonly object m_csLock = new object();
        eMode _eMode = eMode.RegRead;
        public eMode p_eMode
        {
            get { return _eMode; }
            set
            {
                _eMode = value;
                switch (_eMode)
                {
                    case eMode.Init:
                        RunTreeRemove();
                        lock (m_csLock)
                        {
                            RunTreeInit();
                        }
                        m_timer.Start(); 
                        break;
                    case eMode.Update:
                        ClearUpdated();
                        break;
                }
            }
        }
        #endregion

        #region Timer Init
        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            lock (m_csLock)
            {
                RunTreeDone();
            }
            m_timer.Stop(); 
        }  
        #endregion

        public TreeRoot(string id, Log log, bool bReadOnly = false, string sModel = "")
        {
            p_id = id;
            p_treeRoot = this;
            p_treeParent = this;
            p_sName = id;
            p_id = id;
            p_bExpand = true;
            p_bEnable = !bReadOnly; 
            m_log = log;
            m_reg = new Registry(id, sModel);

            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
        }

    }
}
