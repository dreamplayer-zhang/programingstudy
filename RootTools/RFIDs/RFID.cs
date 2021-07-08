using RootTools.Trees;
using System;
using System.Windows.Controls;

namespace RootTools.RFIDs
{
    public class RFID : NotifyProperty, ITool
    {
        #region eRFID
        public enum eRFID
        {
            Brooks,
            Ceyon
        }
        eRFID _eRFID = eRFID.Brooks; 
        public eRFID p_eRFID
        {
            get { return _eRFID; }
            set
            {
                if (_eRFID == value) return;
                _eRFID = value;
                InitIRFID(); 
            }
        }
        #endregion

        #region IRFID
        public IRFID m_RFID = null;
        void InitIRFID()
        {
            if (m_RFID != null) return;
            switch (p_eRFID)
            {
                case eRFID.Brooks: m_RFID = new RFID_Brooks(p_id, m_log); break;
                case eRFID.Ceyon: break;
            }
        }
        #endregion

        #region Read
        string _sRFID = ""; 
        public string p_sRFID
        {
            get { return _sRFID; }
            set
            {
                if (_sRFID == value) return;
                _sRFID = value;
                OnPropertyChanged(); 
            }
        }

        public string Read(out string sRFID)
        {
            p_sRFID = "";
            if (m_RFID == null) 
                InitIRFID();
            string sRun = m_RFID.Read(out sRFID);
            p_sRFID = sRFID; 
            return sRun;
        }
        #endregion

        public UserControl p_ui
        {
            get 
            {
                RFID_UI ui = new RFID_UI();
                ui.Init(this);
                return ui; 
            }
        }

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(p_id, m_log);
            RunTree(Tree.eMode.RegRead);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            p_eRFID = (eRFID)m_treeRoot.Set(p_eRFID, p_eRFID, "RFID", "RFID Type");
            if (mode == Tree.eMode.RegRead) InitIRFID(); 
            m_RFID.RunTree(m_treeRoot.GetTree("Setup")); 
        }
        #endregion


        public string p_id { get; set; }
        Log m_log; 
        public RFID(string id, Log log)
        {
            p_id = id;
            m_log = log;
            InitTree();
            InitIRFID();
        }

        public void ThreadStop()
        {
            m_RFID.ThreadStop(); 
        }
    }
}
