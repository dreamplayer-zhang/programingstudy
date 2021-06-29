using RootTools.Module;
using RootTools.OHT.Semi;
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

        IRFID m_rfid { get; set; }

        OHT m_OHTNew { get; set; }

        OHT_Semi m_OHTsemi { get; set; }
    }
}
