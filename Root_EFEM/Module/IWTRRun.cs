namespace Root_EFEM.Module
{
    public interface IWTRRun
    {
        string p_sChild { get; set; }

        void SetArm(WTRArm arm); 
    }
}
