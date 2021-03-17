using RootTools;
using System;

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
            m_reg?.Write("p_ePod", (int)p_ePod);
            m_reg?.Write("p_bTurn", p_bTurn); 
        }

        public void ReadReg()
        {
            if (m_reg == null) return; 
            p_ePod = (ePod)m_reg.Read("p_ePod", (int)p_ePod);
            p_bTurn = m_reg.Read("p_bTurn", p_bTurn); 
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            int nTurn = p_bTurn ? 1 : 0; 
            return ((int)p_ePod).ToString() + ',' + nTurn.ToString(); 
        }

        public InfoPod(string sInfoPod)
        {
            try
            {
                string[] asInfoPod = sInfoPod.Split(',');
                p_ePod = (ePod)Convert.ToInt32(asInfoPod[0]); 
                p_bTurn = (asInfoPod[1] == "1");
            }
            catch (Exception) { }
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
