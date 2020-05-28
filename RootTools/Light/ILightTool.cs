using System.Collections.Generic;
using System.Windows.Controls;

namespace RootTools.Light
{
    public interface ILightTool
    {
        string p_id { get; set; }
        UserControl p_ui { get; }
        string p_sInfo { get; set; }
        void ThreadStop();
        LightBase GetLight(int nCh, string sNewID);
        List<LightBase> p_aLight { get; set; }
        void Deselect(LightBase light); 
    }
}
