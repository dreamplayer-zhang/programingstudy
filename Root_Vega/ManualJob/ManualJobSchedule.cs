using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

        public void ShowPopup()
        {
            if (ManualJobSchedule_UI.m_bShow) return;
            ManualJobSchedule_UI jobschedulePopup = new ManualJobSchedule_UI();
            jobschedulePopup.Init(this);
            jobschedulePopup.Show();
        }
        #endregion
    }
}
