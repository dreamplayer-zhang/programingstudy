using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;
using RootTools;

namespace RootTools.Control.Ajin
{
    public class AjinDIO : ObservableObject, IToolDIO
    {
        #region ListDIO
        AjinListDIO _listDI = new AjinListDIO();
        public ListDIO p_listDI { get { return _listDI; } }

        AjinListDIO _listDO = new AjinListDIO();
        public ListDIO p_listDO { get { return _listDO; } }

        List<IDIO> _listIDIO = new List<IDIO>();
        public List<IDIO> p_listIDIO { get { return _listIDIO; } }

        void InitList()
        {
            _listDI.Init(ListDIO.eDIO.Input, m_log);
            _listDO.Init(ListDIO.eDIO.Output, m_log); 
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
                m_nInputModule = tree.Set(m_nInputModule, 0, "InputModule", "Input 모듈 총 갯수", true, true);
                m_nOutputModule = tree.Set(m_nOutputModule, 0, "OuputModule", "Output 모듈 총 갯수", true, true);
            }
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
                catch (Exception)
                {
                }
            }
        }
        #endregion

        string m_id;
        LogWriter m_log;
        public int m_nInputModule;
        public int m_nOutputModule;
        public void Init(string id, LogView logView, int nInputModule, int nOutputModule)
        {
            m_id = id;
            m_log = logView.GetLog(LogView.eLogType.ENG, id);
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
