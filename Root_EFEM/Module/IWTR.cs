using System;

namespace Root_EFEM.Module
{
    public interface IWTR
    {
        string p_id { get; set; }

        void AddChild(params IWTRChild[] childs);
        
        void ReadInfoReticle_Registry();
    }
}
