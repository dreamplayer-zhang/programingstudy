﻿using RootTools;
using RootTools.Trees;

namespace Root_Pine2.Module
{
    public class InfoStrip : NotifyProperty
    {
        public enum eResult
        {
            Init,
            Good,
            XOut,
            Rework,
            Error,
            Paper,
        }
        eResult _eResult = eResult.Init; 
        public eResult p_eResult
        {
            get { return _eResult; }
            set
            {
                if (_eResult == value) return;
                _eResult = value;
                OnPropertyChanged(); 
            }
        }

        public int p_nStrip { get; set; }
        public InfoStrip(int nStrip)
        {
            p_eMagazine = eMagazine.Magazine0; 
            p_nStrip = nStrip;
        }

        public enum eMagazine
        {
            Magazine0,
            Magazine1,
            Magazine2,
            Magazine3,
            Magazine4,
            Magazine5,
            Magazine6,
            Magazine7,
        }
        public eMagazine p_eMagazine { get; set; }
        public enum eMagazinePos
        {
            Up,
            Down
        }
        public eMagazinePos p_eMagazinePos { get; set; }
        public InfoStrip(eMagazine eMagazine, eMagazinePos eMagazinePos, int nStrip)
        {
            p_eMagazine = eMagazine;
            p_eMagazinePos = eMagazinePos;
            p_nStrip = nStrip;
        }

        public InfoStrip Clone()
        {
            return new InfoStrip(p_eMagazine, p_eMagazinePos, p_nStrip); 
        }

        public delegate void dgOnDispose(InfoStrip infoStrip);
        public event dgOnDispose OnDispose;
        public void Dispose()
        {
            if (OnDispose != null) OnDispose(this); 
        }

        public bool IsSame(InfoStrip infoStrip)
        {
            if (p_eMagazine != infoStrip.p_eMagazine) return false;
            if (p_eMagazinePos != infoStrip.p_eMagazinePos) return false;
            return (p_nStrip == infoStrip.p_nStrip); 
        }
    }
}
