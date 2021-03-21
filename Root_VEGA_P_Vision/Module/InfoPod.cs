using RootTools;
using RootTools.Trees;
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

        #region Convert String
        public override string ToString()
        {
            return ((int)p_ePod).ToString() + ',' + p_bTurn.ToString(); 
        }

        public void FromString(string sInfoPod)
        {
            try
            {
                string[] asInfoPod = sInfoPod.Split(',');
                p_ePod = (ePod)Convert.ToInt32(asInfoPod[0]);
                p_bTurn = (asInfoPod[1] == true.ToString());
                p_id = p_ePod.ToString();
                m_reg = new Registry("InfoPod." + p_id);
            }
            catch (Exception) { }
        }
        #endregion

        #region Tree
        public void RunTree(Tree tree, bool bVisible)
        {
            p_ePod = (ePod)tree.Set(p_ePod, p_ePod, "Pod", "Pod Name", bVisible, true);
            p_bTurn = tree.Set(p_bTurn, p_bTurn, "Bottom Up", "Turn Over", bVisible); 
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
