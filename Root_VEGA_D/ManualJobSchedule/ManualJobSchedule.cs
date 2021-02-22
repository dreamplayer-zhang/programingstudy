using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_D
{
    public class ManualJobSchedule : NotifyProperty
    {
        public InfoCarrier m_infoCarrier = null;
        public ManualJobSchedule(InfoCarrier infoCarrier)
        {
            m_infoCarrier = infoCarrier;
            if (m_infoCarrier == null) return;
        }

        public bool ShowPopup()
        {
            if (ManualJobSchedule_UI.m_bShow) return false;
            ManualJobSchedule_UI jobschedule = new ManualJobSchedule_UI(m_infoCarrier);
            jobschedule.Init(this);
            jobschedule.ShowDialog();
            return jobschedule.DialogResult == true;
        }
    }
}
