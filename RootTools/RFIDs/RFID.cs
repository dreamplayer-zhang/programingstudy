using System;
using System.Windows.Controls;

namespace RootTools.RFIDs
{
    public class RFID : ITool
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
        IRFID m_RFID = null;
        void InitIRFID()
        {
            switch (p_eRFID)
            {
                case eRFID.Brooks: m_RFID = new RFID_Brooks(p_id, m_log); break;
                case eRFID.Ceyon: break;
            }
        }
        #endregion

        public UserControl p_ui
        {
            get {return m_RFID.p_ui; }
        }

        public string p_id { get; set; }
        Log m_log; 
        public RFID(string id, Log log)
        {
            p_id = id;
            m_log = log;
            InitIRFID(); 
        }
        
        public void ThreadStop()
        {
            m_RFID.ThreadStop(); 
        }
    }
}
