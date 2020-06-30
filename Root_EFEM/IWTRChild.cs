using RootTools.Trees;
using System.Collections.Generic;

namespace Root_EFEM
{
    public interface IWTRChild
    {
        string p_id { get; set; }

        bool p_bLock { get; set; }

        InfoWafer GetInfoWafer(int nID);

        void SetInfoWafer(int nID, InfoWafer infoWafer);

        string IsGetOK(int nID, ref int teachWTR);

        string IsPutOK(int nID, InfoWafer infoWafer, ref int teachWTR);

        string BeforeGet(int nID);

        string BeforePut(int nID);

        string AfterGet(int nID);

        string AfterPut(int nID);

        bool IsWaferExist(int nID, bool bIgnoreExistSensor = false);

        void RunTreeTeach(Tree tree);

        void ReadInfoWafer_Registry();

        List<string> p_asChildID { get; }
    }
}
