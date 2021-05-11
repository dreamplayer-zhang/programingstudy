using RootTools.Module;
using System.Collections.Generic;
using System.Threading;

namespace Root_Pine2.Module
{
    public class MagazineEVSet
    {
        public Dictionary<InfoStrip.eMagazine, MagazineEV> m_aEV = new Dictionary<InfoStrip.eMagazine, MagazineEV>();

        public InfoStrip GetInfoStrip(bool bPeek)
        {
            foreach (MagazineEV magazineEV in m_aEV.Values)
            {
                InfoStrip infoStrip = magazineEV.GetInfoStrip(bPeek);
                if (infoStrip != null) return infoStrip; 
            }
            return null; 
        }

        public string PutInfoStrip(InfoStrip infoStrip)
        {
            if (infoStrip == null) return "InfoStrip == null";
            m_aEV[infoStrip.p_eMagazine].PutInfoStrip(infoStrip);
            return "OK"; 
        }

        public string RunMove(InfoStrip infoStrip)
        {
            if (infoStrip == null) return "InfoStrip == null"; 
            MagazineEV magazineEV = m_aEV[infoStrip.p_eMagazine];
            if (magazineEV.IsBusy()) return "Magazine Elevator is Busy"; 
            if (magazineEV.p_eState != ModuleBase.eState.Ready) return "Magazine Elevator is not Ready";
            if (Run(magazineEV.StartMoveTransfer(infoStrip))) return m_sInfo;
            Thread.Sleep(100);
            while (magazineEV.IsBusy()) Thread.Sleep(10);
            return magazineEV.p_sInfo;
        }

        string m_sInfo = "OK";
        bool Run(string sRun)
        {
            m_sInfo = sRun;
            return sRun != "OK"; 
        }
    }
}
