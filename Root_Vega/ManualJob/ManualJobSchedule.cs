using Root_Vega.Module;
using RootTools;
using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Vega.ManualJob
{
    public class ManualJobSchedule : NotifyProperty
    {
        #region UI Binding
        Brush _brushJobSchedule = Brushes.Black;
        public Brush p_brushJobSchedule
        {
            get { return _brushJobSchedule; }
            set
            {
                if (_brushJobSchedule == value) return;
                _brushJobSchedule = value;
                OnPropertyChanged();
            }
        }
        #endregion

        DispatcherTimer m_UIBackTimer = new DispatcherTimer();
        bool bChangUI = false;
        Color UIBackColorT = Color.FromArgb(255, 67, 67, 122);
        Brush UIBackBrushT;
        Color UIBackColorF = Color.FromArgb(255, 45, 45, 48);
        Brush UIBackBrushF;

        public string p_id { get; set; }
        Log m_log;
        public Loadport m_loadport; 
        public ManualJobSchedule(Loadport loadport)
        {
            m_loadport = loadport; 
            p_id = loadport.p_id;
            m_log = loadport.m_log;

            UIBackBrushT = new SolidColorBrush(UIBackColorT);
            UIBackBrushF = new SolidColorBrush(UIBackColorF);

            m_UIBackTimer.Interval = TimeSpan.FromMilliseconds(1500);
            m_UIBackTimer.Tick += m_UIBackTimer_Tick;
            m_UIBackTimer.Start();
        }

        
        private void m_UIBackTimer_Tick(object sender, EventArgs e)
        {
            if (bChangUI) p_brushJobSchedule = UIBackBrushT;
            else p_brushJobSchedule = UIBackBrushF;
            bChangUI = !bChangUI;
        }

        public void ShowPopup()
        {
            if (ManualJobSchedule_UI.m_bShow) return;
            ManualJobSchedule_UI jobschedulePopup = new ManualJobSchedule_UI();
            jobschedulePopup.Init(this);
            jobschedulePopup.Show();
        }
    }
}
