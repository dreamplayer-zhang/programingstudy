using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Root_WindII
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

        public InfoCarrier m_infoCarrier = null;
        public ManualJobSchedule(InfoCarrier infoCarrier)
        {
            m_infoCarrier = infoCarrier;
            if (m_infoCarrier == null) return;
        }

        public bool ShowPopup()
        {
            if (ManualJobSchedule_UI.m_bShow) return false;
            //사용할 메서드 및 동작
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
