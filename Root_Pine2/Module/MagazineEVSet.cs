﻿using RootTools.Module;
using System.Collections.Generic;
using System.Threading;

namespace Root_Pine2.Module
{
    public class MagazineEVSet
    {
        public Dictionary<InfoStrip.eMagazine, MagazineEV> m_aEV = new Dictionary<InfoStrip.eMagazine, MagazineEV>();

        InfoStrip.eMagazine m_eMagazineGet = InfoStrip.eMagazine.Magazine0; 
        public InfoStrip GetInfoStrip(bool bPeek)
        {
            int iMagazine = (int)m_eMagazineGet; 
            for (int n = 0; n < m_aEV.Count; n++)
            {
                m_eMagazineGet = (InfoStrip.eMagazine)((n + iMagazine) % m_aEV.Count); 
                MagazineEV magazineEV = m_aEV[m_eMagazineGet];
                InfoStrip infoStrip = magazineEV.GetInfoStrip(bPeek);
                if (infoStrip != null) return infoStrip;
            }
            return null; 
        }

        public string PutRequest(InfoStrip infoStrip) //forget
        {
            if (infoStrip == null) return "InfoStrip == null";
            m_aEV[infoStrip.p_eMagazine].m_infoStripUnload = infoStrip;
            return "OK"; 
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
            return true; 
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
        void initThread()
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
                Thread.Sleep(200);
                foreach (MagazineEV magazine in m_aEV.Values)
                {
                    magazine.m_conveyor.RunSwitch(nBlink); 
                }
                nBlink = (nBlink + 1) % 8;
            }
        }

        #endregion

        public MagazineEVSet()
        {
            initThread(); 
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