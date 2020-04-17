using RootTools.Trees;
using System.Windows.Media;

namespace RootTools.DMC
{
    public class DMCDIO : NotifyPropertyChanged
    {
        public string p_id { get; set; }

        public string p_sID
        { 
            get { return m_nID.ToString("00") + ". " + p_sName; }
        }

        string _sName = ""; 
        string p_sName
        { 
            get { return _sName; }
            set
            {
                if (_sName == value) return;
                _sName = value;
                OnPropertyChanged("p_sID"); 
            }
        }

        bool _bOn = false;
        public bool p_bOn
        {
            get { return _bOn; }
            set
            {
                if (_bOn == value) return;
                _bOn = value;
                OnPropertyChanged();
                OnPropertyChanged("p_bColor");
            }
        }

        public Brush p_bColor
        {
            get { return p_bOn ? Brushes.Red : Brushes.DarkGray; }
        }

        public void RunTree(Tree tree)
        {
            p_sName = tree.Set(p_sName, p_sName, p_id, "Change DIO ID"); 
        }

        int m_nID;
        public DMCDIO(int nID, string sID)
        {
            m_nID = nID;
            p_sName = sID;
            p_id = nID.ToString("00") + "." + sID;
        }
    }
}
