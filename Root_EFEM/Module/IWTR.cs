using RootTools.Module;
using System.Collections.Generic;

namespace Root_EFEM.Module
{
    public interface IWTR
    {
        string p_id { get; set; }

        void AddChild(params IWTRChild[] childs);

        List<IWTRChild> p_aChild { get; }

        List<WTRArm> p_aArm { get; }

        bool IsEnableRecovery();

        void ReadInfoReticle_Registry();

        ModuleRunBase CloneRunGet(string sChild, int nSlot);

        ModuleRunBase CloneRunPut(string sChild, int nSlot);

        string GetEnableAnotherArmID(ModuleRunBase runGet, WTRArm armPut, InfoWafer infoWafer);
    }

}
