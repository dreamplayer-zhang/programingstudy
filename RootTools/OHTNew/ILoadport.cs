using RootTools.Module;
using System;

namespace RootTools.OHTNew
{
    public interface ILoadport
    {
        string p_id { get; set; }

        string RunDocking();

        string RunUndocking();

        InfoCarrier p_infoCarrier { get; set; }

        bool p_bPlaced { get; }

        bool p_bPresent { get; }

        ModuleRunBase GetModuleRunDocking();

        ModuleRunBase GetModuleRunUndocking();

        int p_secHome { get; set; }
    }
}
