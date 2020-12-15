using RootTools.Module;

namespace RootTools
{
    public interface IHandler
    {
        string StateHome();
        
        string Reset();

        string AddSequence(dynamic infoSlot);

        void CalcSequence();

        void CheckFinish();

        dynamic GetGemSlot(string sSlot);

        ModuleList p_moduleList { get; set; }
    }
}
