namespace Root_EFEM.Module
{
    public interface IWTRRun
    {
        bool p_isExchange { get; set; }

        int p_nExchangeSlot { get; set; }
        string p_sChild { get; set; }

        void SetArm(WTRArm arm); 
    }
}
