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
            foreach (InfoStrip.eMagazine eMagazine in m_aMagazineDown)
            {
                MagazineEV magazineEV = m_aEV[eMagazine];
                if ((magazineEV != null) && (magazineEV.p_eState == ModuleBase.eState.Ready))
                {
                    MagazineEV.Magazine magazine = magazineEV.m_aMagazine[InfoStrip.eMagazinePos.Down];
                    if ((magazine != null) && (magazine.m_qStripReady.Count > 0)) return magazine.GetInfoStrip(bPeek);
                }
            }
            foreach (InfoStrip.eMagazine eMagazine in m_aMagazineUp)
            {
                MagazineEV magazineEV = m_aEV[eMagazine];
                if ((magazineEV != null) && (magazineEV.p_eState == ModuleBase.eState.Ready) && (magazineEV.m_aMagazine[InfoStrip.eMagazinePos.Down] == null))
                {
                    MagazineEV.Magazine magazine = magazineEV.m_aMagazine[InfoStrip.eMagazinePos.Up];
                    if ((magazine != null) && (magazine.m_qStripReady.Count > 0)) magazine.GetInfoStrip(bPeek);
                }
            }
            return null; 
        }

        List<InfoStrip.eMagazine> m_aMagazineDown = new List<InfoStrip.eMagazine>();
        List<InfoStrip.eMagazine> m_aMagazineUp = new List<InfoStrip.eMagazine>();
        public void MagazineLoaded(InfoStrip.eMagazine eMagazine, InfoStrip.eMagazinePos eMagazinePos)
        {
            if (m_pine2.p_eMode != Pine2.eRunMode.Magazine) return; 
            switch (eMagazinePos)
            { 
                case InfoStrip.eMagazinePos.Up: m_aMagazineUp.Add(eMagazine); break;
                case InfoStrip.eMagazinePos.Down: m_aMagazineDown.Add(eMagazine); break;
            }
        }

        public void MagazineUnloaded(InfoStrip.eMagazine eMagazine, InfoStrip.eMagazinePos eMagazinePos)
        {
            if (m_pine2.p_eMode != Pine2.eRunMode.Magazine) return;
            switch (eMagazinePos)
            {
                case InfoStrip.eMagazinePos.Up: m_aMagazineUp.Remove(eMagazine); break;
                case InfoStrip.eMagazinePos.Down: m_aMagazineDown.Remove(eMagazine); break;
            }
        }

        public double CalcXOffset(InfoStrip infoStrip)
        {
            return m_aEV[infoStrip.p_eMagazine].CalcXOffset(infoStrip); 
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

        public bool IsEnableStack(InfoStrip.eMagazine eMagazine, InfoStrip.eResult eResult, bool bMatch)
        {
            if (m_aEV[eMagazine].IsBusy()) return false; 
            MagazineEV.Stack stack = m_aEV[eMagazine].m_stack;
            if (stack == null) return false;
            if (stack.p_eResult == eResult) return true;
            if (bMatch) return false; 
            if (stack.p_eResult != InfoStrip.eResult.Init) return false;
            stack.p_eResult = eResult;
            if (eResult == InfoStrip.eResult.DEF) stack.p_iBundle = m_pine2.p_iBundle++; 
            return true; 
        }

        public bool IsMagazineUp()
        {
            foreach (MagazineEV ev in m_aEV.Values)
            {
                if (ev.m_elevator.IsMagazineUp()) return true; 
            }
            return false; 
        }

        string m_sInfo = "OK";
        bool Run(string sRun)
        {
            m_sInfo = sRun;
            return sRun != "OK"; 
        }

        #region Thread
        bool m_bThread = false;
        Thread m_thread; 
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        void RunThread()
        {
            int nBlink = 0;
            m_bThread = true;
            Thread.Sleep(5000);
            while (m_bThread)
            {
                Thread.Sleep(150);
                foreach (MagazineEV magazine in m_aEV.Values)
                {
                    magazine.m_conveyor.RunSwitch(nBlink);
                }
                nBlink = (nBlink + 1) % 8;
            }
        }
        #endregion

        Pine2 m_pine2;
        public MagazineEVSet(Pine2 pine2)
        {
            m_pine2 = pine2; 
            InitThread(); 
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join(); 
            }
        }
    }
}
