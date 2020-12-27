using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_EFEM.Module
{
    public interface IWTRChild
    {
        string p_id { get; set; }

        ModuleBase.eState p_eState { get; }

        bool p_bLock { get; set; }

        InfoWafer p_infoWafer { get; set; }

        List<string> p_asChildSlot { get; }

        InfoWafer GetInfoWafer(int nID);

        void SetInfoWafer(int nID, InfoWafer infoWafer);

        int GetTeachWTR(InfoWafer infoWafer = null);

        string IsGetOK(int nID);

        string IsPutOK(InfoWafer infoWafer, int nID);

        string BeforeGet(int nID);

        string BeforePut(int nID);

        string AfterGet(int nID);

        string AfterPut(int nID);

        bool IsWaferExist(int nID);

        void RunTreeTeach(Tree tree);

        void ReadInfoWafer_Registry();
    }
}
