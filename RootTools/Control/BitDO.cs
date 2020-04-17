namespace RootTools.Control
{
    public class BitDO : BitDI
    {
        public virtual void Write(bool bOn)
        {
        }

        public new void Init(int nID, LogWriter log)
        {
            m_nID = nID;
            m_log = log;
            p_sID = "Output";
        }
    }
}
