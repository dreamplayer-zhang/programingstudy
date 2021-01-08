using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_CAMELLIA.ManualJob
{
    public class ManualJobSchedule : NotifyProperty
    {
        string _sLocID = "";
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

        string _sLotID = "";
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

        string _sCarrierID = "";
        public string p_sCarrierID
        { 
            get { return _sCarrierID; }
            set
            {
                if (_sCarrierID == value) return;
                _sCarrierID = value;
                OnPropertyChanged();
            }
        }

        bool _bRnR = false;
        public bool p_bRnR
        {
            get { return _bRnR; }
            set
            {
                _bRnR = value;
                OnPropertyChanged();
            }
        }

        string _sRecipe = "";
        public string p_sRecipe
        {
            get { return _sRecipe; }
            set
            {
                if (_sRecipe == value) return;
                _sRecipe = value;
                OnPropertyChanged();
            }
        }

        public InfoCarrier m_infoCarrier = null;
        public ManualJobSchedule(InfoCarrier infoCarrier)
        {
            m_infoCarrier = infoCarrier;
            if (m_infoCarrier == null) return;
        }

        public bool ShowPopup()
        {
            if (ManualJobSchedule_UI.m_bShow) return false;
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ManualJobSchedule_UI jobschedulePopup = new ManualJobSchedule_UI(m_infoCarrier);
                jobschedulePopup.Init(this, m_infoCarrier);
                //m_handler.m_nRnR = p_bRnR ? p_nRnR : 1;   //working
                p_bRnR = false;
                jobschedulePopup.ShowDialog();
                //return jobschedulePopup.DialogResult == true;
            });
            return false;
        }
    }
}
