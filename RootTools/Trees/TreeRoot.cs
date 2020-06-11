﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            ObservableCollection<Tree> m_aChild;
            Tree m_treeChild; 

            public void RunLink()
            {
                foreach (Tree tree in m_aChild)
                {
                    if (tree.p_id == m_treeChild.p_id) return; 
                }
                m_aChild.Add(m_treeChild); 
            }

            public Link(ObservableCollection<Tree> aChild, Tree treeChild)
            {
                m_aChild = aChild;
                m_treeChild = treeChild; 
            }
        }
        Queue<Link> m_qLink = new Queue<Link>(); 

        public void AddQueue(ObservableCollection<Tree> aChild, Tree treeChild)
        {
             m_qLink.Enqueue(new Link(aChild, treeChild)); 
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            while (m_qLink.Count > 0) m_qLink.Dequeue().RunLink();
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

            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

    }
}
