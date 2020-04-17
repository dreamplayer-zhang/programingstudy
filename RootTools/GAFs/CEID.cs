using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System.IO;

namespace RootTools.GAFs
{
    public class CEID : NotifyProperty
    {
        #region Property
        public string p_sModule { get; set; }

        public string p_id { get; set; }

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
        #endregion

        ModuleBase m_module;
        IGem m_gem;
        public CEID(ModuleBase module, string id)
        {
            m_module = module;
            p_sModule = module.p_id;
            p_id = id;
            m_gem = module.m_gem;
        }

        public void Send()
        {
            if (m_gem == null) return;
            if (p_nID < 0) return;
            m_gem.SetCEID(this);
        }

        public void RunTree(Tree tree, int nDefaultID)
        {
            p_nID = tree.Set(p_nID, nDefaultID, p_id, "CEID Number");
        }

        public void Save(StreamWriter sw)
        {
            sw.WriteLine(p_sModule + ", " + p_id + ", " + p_nID.ToString());
        }
    }
}
