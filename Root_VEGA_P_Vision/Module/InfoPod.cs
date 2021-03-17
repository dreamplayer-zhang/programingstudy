using RootTools;

namespace Root_VEGA_P_Vision.Module
{
    public class InfoPod : NotifyProperty
    {
        #region Property
        public enum ePod
        {
            EOP_Door,
            EIP_Plate,
            EIP_Cover,
            EOP_Dome
        }
        public ePod p_ePod { get; set; }

        bool _bTurn = false;
        public bool p_bTurn
        {
            get { return _bTurn; }
            set
            {
                _bTurn = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Registry
        Registry m_reg; 
        public void WriteReg()
        {
            m_reg.Write("p_ePod", (int)p_ePod);
            m_reg.Write("p_bTurn", p_bTurn); 
        }

        public void ReadReg()
        {
            p_ePod = (ePod)m_reg.Read("p_ePod", (int)p_ePod);
            p_bTurn = m_reg.Read("p_bTurn", p_bTurn); 
        }
        #endregion

        public string p_id { get; set; }
        public InfoPod(ePod ePod)
        {
            p_ePod = ePod;
            p_id = ePod.ToString();
            m_reg = new Registry("InfoPod." + p_id); 
        }
    }
}
