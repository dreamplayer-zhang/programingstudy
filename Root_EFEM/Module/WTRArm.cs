using RootTools;
using RootTools.Module;
using RootTools.Trees;

namespace Root_EFEM.Module
{
    public class WTRArm : NotifyProperty
    {
        #region InfoWafer
        Registry m_reg;
        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get { return _infoWafer; }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                m_reg.Write("sInfoWafer", m_sInfoWafer);
                OnPropertyChanged();
            }
        }

        public void ReadInfoWafer_Registry()
        {
            m_reg = new Registry(m_id);
            m_sInfoWafer = (string)m_reg.Read("sInfoWafer", m_sInfoWafer);
            p_infoWafer = ((ModuleBase)m_wtr).m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        }
        #endregion

        #region Wafer Size
        public bool IsEnableWaferSize(InfoWafer infoWafer)
        {
            if (m_bEnable == false) return false;
            return m_waferSize.GetData(infoWafer.p_eSize).m_bEnable;
        }

        public bool m_bEnable = true;
        public virtual void RunTree(Tree tree)
        {
            m_bEnable = tree.Set(m_bEnable, m_bEnable, "Enable", "Enable WTR Arm");
            m_waferSize.RunTree(tree.GetTree("Wafer Size", false), m_bEnable);
        }
        #endregion

        public string m_id;
        IWTR m_wtr;
        InfoWafer.WaferSize m_waferSize;
        public void Init(string id, IWTR wtr)
        {
            m_id = id; 
            m_wtr = wtr; 
            m_waferSize = new InfoWafer.WaferSize(m_id, true, false);
        }
    }
}
