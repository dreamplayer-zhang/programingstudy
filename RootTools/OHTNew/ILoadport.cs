using RootTools.Module;
using System;

namespace RootTools.OHTNew
{
    public interface ILoadport
    {
        string p_id { get; set; }

        string StartRunDocking();

        string StartRunUndocking();

        InfoCarrier p_infoCarrier { get; set; }

        bool p_bPlaced { get; }

        bool p_bPresent { get; }

        ModuleRunBase GetLoadModuleRun();

        ModuleRunBase GetUnLoadModuleRun();
    }
}
