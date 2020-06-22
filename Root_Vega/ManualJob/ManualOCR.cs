﻿using RootTools;
using System.Windows.Media.Imaging;

namespace Root_Vega.ManualJob
{
    public class ManualOCR : NotifyProperty
    {
        #region Property
        public BitmapImage p_image { get; set; }

        public string p_sOCR
        {
            get { return m_infoRetile.p_sSlotID; }
            set 
            {
                m_infoRetile.p_sSlotID = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region UI
        public void ShowOCR()
        {
            ManualOCR_UI ui = new ManualOCR_UI();
            ui.Init(this);
            ui.Show(); 
        }
        #endregion

        InfoReticle m_infoRetile = null;
        public ManualOCR(InfoReticle infoReticle, BitmapImage bitmap)
        {
            m_infoRetile = infoReticle;
            p_image = bitmap; 
        }
    }
}