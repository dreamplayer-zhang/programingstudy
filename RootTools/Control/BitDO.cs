namespace RootTools.Control
{
    public class BitDO : BitDI
    {
        public virtual void Write(bool bOn)
        {
            if (EQ.p_bSimulate) p_bOn = bOn; 
        }

        public new void Init(int nID, Log log)
        {
            m_nID = nID;
            m_log = log;
            p_sID = "Output";
        }
    }
}
