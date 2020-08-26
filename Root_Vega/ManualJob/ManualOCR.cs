using RootTools;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Root_Vega.ManualJob
{
    public class ManualOCR : NotifyProperty
    {
        #region Property
        BitmapImage _image = null; 
        public BitmapImage p_image 
        { 
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged(); 
            }
        }

        public string p_sOCR
        {
            get { return (m_infoRetile != null) ? m_infoRetile.p_sSlotID : ""; }
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
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ManualOCR_UI ui = new ManualOCR_UI();
                ui.Init(this);
                ui.ShowDialog();
            }); 
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
