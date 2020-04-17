using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_Wind
{
    public interface IWTRChild
    {
        string p_id
        {
            get;
            set;
        }
        
        ModuleBase.eState p_eState { get;}

        bool p_bLock { get; set; }

        InfoWafer GetInfoWafer(int nID);

        void SetInfoWafer(int nID, InfoWafer infoWafer);

        string IsGetOK(int nID);

        string IsPutOK(int nID, InfoWafer infoWafer);

        int GetWTRTeach(InfoWafer infoWafer = null); 

        string BeforeGet(int nID);

        string BeforePut(int nID);

        string AfterGet(int nID);

        string AfterPut(int nID);

        bool IsWaferExist(int nID = 0, bool bIgnoreExistSensor = false); 

        void RunTeachTree(Tree tree);

        void ReadInfoWafer_Registry(); 

        List<string> p_asChildID { get; }
    }
}
