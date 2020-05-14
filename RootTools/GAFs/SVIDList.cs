﻿using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace RootTools.GAFs
{
    public class SVIDList
    {
        #region List SVID
        public ObservableCollection<SVID> _aSVID = new ObservableCollection<SVID>();
        public ObservableCollection<SVID> p_aSVID
        {
            get { return _aSVID; }
        }

        public void SaveFile(string sFile)
        {
            try
            {
                StreamWriter sw = new StreamWriter(new FileStream(sFile, FileMode.Create));
                foreach (SVID svid in p_aSVID)
                {
                    svid.Save(sw);
                }
                sw.Close();
            }
            catch (Exception) { }
        }
        #endregion

        #region Tree
        private void M_tree_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            foreach (GAF.Group group in m_aGroup)
            {
                RunTree(m_treeRoot.GetTree(group.m_sGroup), group);
            }
        }

        void RunTree(Tree tree, GAF.Group group)
        {
            foreach (SVID svid in group.m_aSVID)
            {
                svid.RunTree(tree, group.GetNextSVID());
            }
        }
        #endregion

        string m_id;
        Log m_log;
        List<GAF.Group> m_aGroup;
        public TreeRoot m_treeRoot;
        public void Init(string id, GAF gaf)
        {
            m_id = id;
            m_log = gaf.m_log;
            m_aGroup = gaf.m_aGroup;
            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_tree_UpdateTree;
            RunTree(Tree.eMode.RegRead);
        }

        public void ThreadStop()
        {
        }
    }
}
