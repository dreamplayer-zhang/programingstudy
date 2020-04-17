using System.Collections.ObjectModel;

namespace RootTools.Control
{
    public class ListDIO
    {
        public enum eDIO
        {
            Output,
            Input
        }
        public eDIO m_eDIO = eDIO.Input;

        #region UI
        public string p_sHeader
        {
            get { return "Digital " + m_eDIO.ToString(); }
        }
        #endregion

        #region List Bit
        public ObservableCollection<BitDI> m_aDIO = new ObservableCollection<BitDI>(); 
        public void AddBit(BitDI bit)
        {
            m_aDIO.Add(bit); 
        }

        public void AddBit(int nID, LogWriter log)
        {
            switch (m_eDIO)
            {
                case eDIO.Input:
                    BitDI bitDI = new BitDI();
                    bitDI.Init(nID, log);
                    m_aDIO.Add(bitDI);
                    break;
                case eDIO.Output:
                    BitDO bitDO = new BitDO();
                    bitDO.Init(nID, log);
                    m_aDIO.Add(bitDO);
                    break; 
            }
        }
        #endregion

        public void Init(eDIO dio)
        {
            m_eDIO = dio;
        }
    }
}
