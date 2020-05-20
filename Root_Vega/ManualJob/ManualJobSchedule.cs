using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Vega.ManualJob
{
    public class ManualJobSchedule : NotifyProperty
    {
        #region UI Binding
        string _sLocID = "Loadport1";
        public string p_sLocID
        {
            get { return _sLocID; }
            set
            {
                if (_sLocID == value) return;
                _sLocID = value;
                OnPropertyChanged();
            }
        }

        string _sPodID = "PodID";
        public string p_sPodID
        {
            get { return _sPodID; }
            set
            {
                if (_sPodID == value) return;
                _sPodID = value;
                OnPropertyChanged();
            }
        }

        string _sLotID = "LotID";
        public string p_sLotID
        {
            get { return _sLotID; }
            set
            {
                if (_sLotID == value) return;
                _sLotID = value;
                OnPropertyChanged();
            }
        }

        string _sReticleID = "ReticleID";
        public string p_sReticleID
        {
            get { return _sReticleID; }
            set
            {
                if (_sReticleID == value) return;
                _sReticleID = value;
                OnPropertyChanged();
            }
        }

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

        public string p_id { get; set; }
        Log m_log;
        DispatcherTimer m_UIBackTimer = new DispatcherTimer();
        bool bChangUI = false;
        Color UIBackColorT = Color.FromArgb(255, 67, 67, 122);
        Brush UIBackBrushT;
        Color UIBackColorF = Color.FromArgb(255, 45, 45, 48);
        Brush UIBackBrushF;

        public ManualJobSchedule(string id, Log log)
        {
            p_id = id;
            m_log = log;

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
