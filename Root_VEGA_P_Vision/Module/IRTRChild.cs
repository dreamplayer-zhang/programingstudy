using RootTools.Module;
using RootTools.Trees;

namespace Root_VEGA_P_Vision.Module
{
    public interface IRTRChild
    {
        string p_id { get; set; }

        ModuleBase.eState p_eState { get; }

        InfoPod p_infoPod { get; set; }
        
        string IsGetOK();

        string IsPutOK(InfoPod infoPod);

        string BeforeGet();

        string BeforePut(InfoPod infoPod);

        string AfterGet();

        string AfterPut();

        bool IsPodExist(InfoPod.ePod ePod);

        int GetTeachRTR(InfoPod infoPod);

        void RunTreeTeach(Tree tree);

        void ReadPod_Registry(); 
    }
}
