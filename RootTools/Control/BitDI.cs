using System.Windows.Media;

namespace RootTools.Control
{
    public class BitDI : NotifyPropertyChanged
    {
        #region Porperty
        bool _bOn = false; 
        public bool p_bOn
        {
            get { return _bOn; }
            set
            {
                if (_bOn == value) return;
                if (m_log != null) m_log.Info(p_sLongID + " : " + _bOn.ToString() + " -> " + value.ToString());
                _bOn = value;
                OnPropertyChanged();
                OnPropertyChanged("p_bColor");
            }
        }
        #endregion

        #region UI Binding
        public Brush p_bColor
        {
            get { return p_bOn ? Brushes.Red : Brushes.DarkGray; }
        }

        string _sID = "";
        public string p_sID
        {
            get { return _sID; }
            set
            {
                if (_sID == value) return;
                _sID = value;
                _sLongID = m_nID.ToString("000 ") + _sID;
                string[] sIDs = _sID.Split('.');
                _sShortID = _sID.Replace(sIDs[0] + ".", ""); 
                OnPropertyChanged();
                OnPropertyChanged("p_sLongID");
                OnPropertyChanged("p_sShortID");
            }
        }

        string _sLongID = "";
        public string p_sLongID
        {
            get { return _sLongID; }
        }

        string _sShortID = "";
        public string p_sShortID
        {
            get { return _sShortID; }
        }
        #endregion

        public int m_nID = -1;
        public LogWriter m_log;
        public void Init(int nID, LogWriter log)
        {
            m_nID = nID;
            m_log = log;
            p_sID = "Input"; 
        }

        public void SetID(LogWriter log, string sID)
        {
            m_log = log;
            p_sID = sID;
        }
    }
}
