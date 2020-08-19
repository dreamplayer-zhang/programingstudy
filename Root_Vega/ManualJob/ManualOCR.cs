using RootTools;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Root_Vega.ManualJob
{
    public class ManualOCR : NotifyProperty
    {
        #region Property
        public BitmapImage p_image { get; set; }

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
        public bool m_bShowDialog = false; 
        public void ShowOCR()
        {
            m_bShowDialog = true; 
            m_timer.Start(); 
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            m_timer.Stop(); 
            ManualOCR_UI ui = new ManualOCR_UI();
            ui.Init(this);
            ui.ShowDialog();
            m_bShowDialog = false; 
        }
        #endregion

        InfoReticle m_infoRetile = null;
        public ManualOCR(InfoReticle infoReticle, BitmapImage bitmap)
        {
            m_infoRetile = infoReticle;
            p_image = bitmap;

            m_timer.Interval = TimeSpan.FromMilliseconds(10);
            m_timer.Tick += M_timer_Tick;
        }
    }
}
