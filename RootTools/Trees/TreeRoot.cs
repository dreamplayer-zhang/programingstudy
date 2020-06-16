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
                        RunTreeInit();
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
        class Link
        {
            Tree m_tree;
            Tree m_treeChild; 

            public void RunLink()
            {
                foreach (Tree tree in m_tree.p_aChild)
                {
                    if (tree.p_id == m_treeChild.p_id) return; 
                }
                m_tree.p_aChild.Add(m_treeChild); 
            }

            public Link(Tree tree, Tree treeChild)
            {
                m_tree = tree;
                m_treeChild = treeChild; 
            }
        }
        Queue<Link> m_qLink = new Queue<Link>(); 

        public void AddQueue(Tree tree, Tree treeChild)
        {
             m_qLink.Enqueue(new Link(tree, treeChild)); 
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            while (m_qLink.Count > 0) m_qLink.Dequeue().RunLink();
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
