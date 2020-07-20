using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System.IO;

namespace RootTools.GAFs
{
    public class SVID : NotifyProperty
    {
        #region Property
        public string p_sModule { get; set; }

        public string p_id { get; set; }

        public enum eType
        {
            SV,
            DV
        };
        eType _eType = eType.SV;
        public eType p_eType
        {
            get { return _eType; }
            set
            {
                if (_eType == value) return;
                _eType = value;
                OnPropertyChanged();
            }
        }

        int _nID = -1;
        public int p_nID
        {
            get { return _nID; }
            set
            {
                if (_nID == value) return;
                _nID = value;
                OnPropertyChanged();
            }
        }

        dynamic _value;
        public dynamic p_value
        {
            get { return (_value != null) ? _value : ""; }
            set
            {
                if (_value == value) return;
                //if (p_nID < 0) return;
                if (m_gem != null) m_gem.SetSV(this, value);
                if (m_log != null && _value != null) m_log.Info(m_sID + " : " + _value.ToString() + " -> " + value.ToString());
                _value = value;
                OnPropertyChanged();
            }
        }
        #endregion

        ModuleBase m_module;
        string m_sID;
        IGem m_gem;
        Log m_log;
        public SVID(ModuleBase module, string id, eType type = eType.SV)
        {
            m_module = module;
            p_sModule = module.p_id;
            p_id = id;
            m_sID = "SV." + p_sModule + "." + p_id;
            p_eType = type;
            m_gem = module.m_gem;
            m_log = module.m_log;
        }

        public void RunTree(Tree treeParent, int nDefaultID)
        {
            Tree tree = treeParent.GetTree(p_id);
            p_nID = tree.Set(p_nID, nDefaultID, "Number", "SVID Number");
            p_eType = (eType)tree.Set(p_eType, p_eType, "Type", "SVID Type");
        }

        public void Save(StreamWriter sw)
        {
            sw.WriteLine(p_sModule + ", " + p_id + ", " + p_eType.ToString() + ", " + p_nID.ToString());
        }
    }
}
