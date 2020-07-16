using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RootTools.Control.ACS
{
    public class ACSDIO : IToolDIO
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
            _listDI.Init(ListDIO.eDIO.Input, m_acs);
            _listDO.Init(ListDIO.eDIO.Output, m_acs);
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
            _listDI.RunTree(treeRoot.GetTree("Input"));
            _listDO.RunTree(treeRoot.GetTree("Output"));
        }
        #endregion

        string m_id;
        ACS m_acs; 
        Log m_log;
        public void Init(string id, ACS acs)
        {
            m_id = id;
            m_acs = acs; 
            m_log = acs.m_log;
            InitList();
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
