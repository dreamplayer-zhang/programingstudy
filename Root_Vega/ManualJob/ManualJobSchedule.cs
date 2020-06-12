using Root_Vega.Module;
using RootTools;
using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Vega.ManualJob
{
    public class ManualJobSchedule : NotifyProperty
    {
        public string p_id { get; set; }
        Log m_log;
        public Loadport m_loadport; 
        public ManualJobSchedule(Loadport loadport)
        {
            m_loadport = loadport; 
            p_id = loadport.p_id;
            m_log = loadport.m_log;
        }

        public bool ShowPopup()
        {
            if (ManualJobSchedule_UI.m_bShow) return false;
            ManualJobSchedule_UI jobschedulePopup = new ManualJobSchedule_UI();
            jobschedulePopup.Init(this);
            jobschedulePopup.ShowDialog();
            //return jobschedulePopup.ShowDialog() == true;
            return jobschedulePopup.DialogResult == true;
        }
    }
}
