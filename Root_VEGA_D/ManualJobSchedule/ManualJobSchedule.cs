using Root_EFEM.Module;
using Root_VEGA_D.Engineer;
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
        Loadport_Cymechs m_loadport;
        VEGA_D_Engineer m_engineer;
        VEGA_D_Handler m_handler;
        public ManualJobSchedule(VEGA_D_Engineer engineer,Loadport_Cymechs loadport, InfoCarrier infoCarrier)
        {
            m_engineer = engineer;
            m_loadport = loadport;
            m_infoCarrier = infoCarrier;
            if (m_infoCarrier == null) return;
            m_handler = m_engineer.m_handler;
        }

        public bool ShowPopup(VEGA_D_Handler handler)
        {
            if (ManualJobSchedule_UI.m_bShow) return false;
            ManualJobSchedule_UI jobschedule = new ManualJobSchedule_UI(m_infoCarrier);
            jobschedule.Init(this, m_engineer, m_loadport);
            jobschedule.ShowDialog();
            p_bRnR = false;
            if (p_bRnR)
            {
                m_handler.p_nRnRCount = p_nRnR;
                //RNR_UI rnr_ui = new RNR_UI();
                //rnr_ui.Init(m_engineer);
                //rnr_ui.Show();
            }
            else if (!p_bRnR)
            {
                m_handler.p_nRnRCount = 1;
            }
            p_nRnR = 1;
            return jobschedule.DialogResult == true;
        }
        public string CheckInfoPod()
        {
            if (m_loadport.p_infoCarrier.m_aInfoWafer == null)
            {
                m_loadport.m_alidInforeticle.Run(true, "Reticle Info is null");
                return "p_infoWafer == null";
            }
            return "OK";
        }
        #region RnR Property
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

        int _nRnR = 1;
        public int p_nRnR
        {
            get { return _nRnR; }
            set
            {
                _nRnR = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }
}
