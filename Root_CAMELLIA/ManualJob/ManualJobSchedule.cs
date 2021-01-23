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

        bool _ManualJobBlick = false;
        public bool p_ManualJobBlink
        {
            get { return _ManualJobBlick; }
            set
            {
                if (_ManualJobBlick == value) return;
                _ManualJobBlick = value;
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
            p_ManualJobBlink = true;
            if (ManualJobSchedule_UI.m_bShow) return false;
            ManualJobSchedule_UI jobschedulePopup = new ManualJobSchedule_UI(m_infoCarrier);
            jobschedulePopup.Init(this, m_infoCarrier);
            //m_handler.m_nRnR = p_bRnR ? p_nRnR : 1;   //working
            p_bRnR = false;
            jobschedulePopup.ShowDialog();
            return jobschedulePopup.DialogResult == true;
        }

        public void SetInfoCarrier(InfoCarrier infoCarrier)
        {
            m_infoCarrier = infoCarrier;
        }

        public InfoCarrier GetInfoCarrier()
        {
            return m_infoCarrier;
        }
    }
}
