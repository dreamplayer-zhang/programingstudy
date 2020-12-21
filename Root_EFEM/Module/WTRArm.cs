using RootTools;
using RootTools.Trees;

namespace Root_EFEM.Module
{
    public class WTRArm : NotifyProperty
    {
        #region Property
        bool _bEnable = true; 
        public bool p_bEnable
        {
            get { return _bEnable; }
            set
            {
                if (_bEnable == value) return;
                _bEnable = value;
                OnPropertyChanged(); 
            }
        }
        #endregion 

        #region InfoWafer
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

        Registry m_reg;
        public void ReadInfoWafer_Registry()
        {
            m_reg = new Registry(m_id);
            m_sInfoWafer = (string)m_reg.Read("sInfoWafer", m_sInfoWafer);
            p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        }
        #endregion

        #region Wafer Size
        InfoWafer.WaferSize m_waferSize;
        public bool IsEnableWaferSize(InfoWafer infoWafer) 
        {
            if (p_bEnable == false) return false;
            return m_waferSize.GetData(infoWafer.p_eSize).m_bEnable;
        }
        #endregion

        #region virtual IsWaferExist
        public virtual bool IsWaferExist()
        {
            return (p_infoWafer != null);
        }
        #endregion

        #region Tree
        public virtual void RunTree(Tree tree)
        {
            p_bEnable = tree.Set(p_bEnable, p_bEnable, "Enable", "Enable WTR Arm");
            m_waferSize.RunTree(tree.GetTree("Wafer Size", false), p_bEnable);
        }
        #endregion

        public string m_id;
        IEngineer m_engineer;
        public void Init(string id, IEngineer engineer, bool bEnableWaferSize, bool bEnableWaferCount)
        {
            m_id = id;
            m_engineer = engineer;
            m_waferSize = new InfoWafer.WaferSize(m_id, bEnableWaferSize, bEnableWaferCount);
        }
    }
}
