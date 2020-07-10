using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RootTools.Control.ACS
{
    public class ACSDIO : IToolDIO //forgetACS
    {
        #region ListDIO
        ACSListDIO _listDI = new ACSListDIO();
        public ListDIO p_listDI { get { return _listDI; } }

        ACSListDIO _listDO = new ACSListDIO();
        public ListDIO p_listDO { get { return _listDO; } }

        List<IDIO> _listIDIO = new List<IDIO>();
        public List<IDIO> p_listIDIO { get { return _listIDIO; } }

        void InitList()
        {
            _listDI.Init(ListDIO.eDIO.Input, m_log);
            _listDO.Init(ListDIO.eDIO.Output, m_log);
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread;
        void RunThread()
        {
            m_bThread = true;
            Thread.Sleep(5000);
            while (m_bThread)
            {
                try
                {
                    Thread.Sleep(2);
                    _listDI.ReadIO();
                    _listDO.ReadIO();
                    foreach (IDIO idio in p_listIDIO) idio.RunDIO();
                }
                catch (Exception) { }
            }
        }
        #endregion

        #region Tree
        public void RunTree(Tree treeRoot)
        {
            RunInfoTree(treeRoot.GetTree("Info"));
            _listDI.RunTree(treeRoot.GetTree("Input"));
            _listDO.RunTree(treeRoot.GetTree("Output"));
        }

        public void RunInfoTree(Tree tree)
        {
            if (tree.p_treeRoot.p_eMode == Tree.eMode.Init)
            {
                m_nInputModule = tree.Set(m_nInputModule, 0, "InputModule", "Input Module Count", true, true);
                m_nOutputModule = tree.Set(m_nOutputModule, 0, "OuputModule", "Output Module Count", true, true);
            }
        }
        #endregion

        string m_id;
        Log m_log;
        public int m_nInputModule;
        public int m_nOutputModule;
        public void Init(string id, int nInputModule, int nOutputModule)
        {
            m_id = id;
            m_log = LogView.GetLog(id);
            InitList();
            m_nInputModule = nInputModule;
            m_nOutputModule = nOutputModule;
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join();
            }
        }
    }
}
