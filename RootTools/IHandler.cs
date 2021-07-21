using RootTools.Module;
using System.Collections.Generic;

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

        RnRData GetRnRData();
        void UpdateEvent();
        ModuleList p_moduleList { get; set; }

        //ILoadport 
    }
}
