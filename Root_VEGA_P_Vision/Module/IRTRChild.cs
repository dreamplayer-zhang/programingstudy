using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision.Module
{
    public interface IRTRChild
    {
        string p_id { get; set; }

        ModuleBase.eState p_eState { get; }

        InfoPod GetInfoPod(InfoPod.ePod ePod);

        void SetInfoPod(InfoPod.ePod ePod, InfoPod infoPod);

        int GetTeachRTR(InfoPod infoPod);

        string IsGetOK(InfoPod.ePod ePod); 
    }
}
