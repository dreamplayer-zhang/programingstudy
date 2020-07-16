using RootTools.Trees;
using System;
using System.Collections.Generic;

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
        public void RunThreadCheck()
        {
            try
            {
                _listDI.ReadIO();
                _listDO.ReadIO();
                foreach (IDIO idio in p_listIDIO) idio.RunDIO();
            }
            catch (Exception e) { LogError("DIO Thread Check Error : " + e.Message); }
        }

        StopWatch m_swError = new StopWatch();
        void LogError(string sError)
        {
            if (m_swError.ElapsedMilliseconds < 5000) return;
            m_swError.Restart();
            m_log.Error(m_id + " " + sError); 
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
        }

        public void ThreadStop()
        {
        }
    }
}
